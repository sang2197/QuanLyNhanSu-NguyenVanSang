using FluentAssertions;
using HRM.Application.Common;
using HRM.Application.Exceptions;
using HRM.Application.OrganizationManagement.Interfaces;
using HRM.Application.OrganizationManagement.Services;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Moq;
using Xunit;

namespace HRM.Application.Tests.OrganizationManagement.Services;

public class JobTitleServiceTests
{
    private readonly Mock<IJobTitleRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly JobTitleService _sut;

    public JobTitleServiceTests()
    {
        _sut = new JobTitleService(_repo.Object, _unitOfWork.Object);
    }

    // ---------- CreateJobTitleAsync (BR-ORG-17) ----------

    [Fact]
    public async Task CreateJobTitleAsync_DuplicateName_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByNameAsync("Engineer", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrJobTitle { Id = 1, Name = "Engineer" });

        var act = () => _sut.CreateJobTitleAsync("Engineer");

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateJobTitleAsync_ValidName_CreatesActiveJobTitleAndSaves()
    {
        var jobTitle = await _sut.CreateJobTitleAsync("Engineer");

        jobTitle.Name.Should().Be("Engineer");
        jobTitle.Status.Should().Be(ActiveStatus.ACTIVE);
        _repo.Verify(r => r.AddAsync(It.IsAny<HrJobTitle>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- ListJobTitlesAsync ----------

    [Fact]
    public async Task ListJobTitlesAsync_ReturnsAllJobTitles()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HrJobTitle> { new() { Id = 1 }, new() { Id = 2 } });

        var result = await _sut.ListJobTitlesAsync();

        result.Should().HaveCount(2);
    }

    // ---------- UpdateJobTitleAsync (BR-ORG-17/18) ----------

    [Fact]
    public async Task UpdateJobTitleAsync_NotFound_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrJobTitle?)null);

        var act = () => _sut.UpdateJobTitleAsync(1, "Senior Engineer");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateJobTitleAsync_ChangedNameDuplicatesExisting_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrJobTitle { Id = 1, Name = "Engineer" });
        _repo.Setup(r => r.GetByNameAsync("Manager", It.IsAny<CancellationToken>())).ReturnsAsync(new HrJobTitle { Id = 2, Name = "Manager" });

        var act = () => _sut.UpdateJobTitleAsync(1, "Manager");

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateJobTitleAsync_SameName_DoesNotCheckDuplicateAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrJobTitle { Id = 1, Name = "Engineer" });

        var result = await _sut.UpdateJobTitleAsync(1, "Engineer");

        result.Name.Should().Be("Engineer");
        _repo.Verify(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateJobTitleAsync_ValidNewName_UpdatesAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrJobTitle { Id = 1, Name = "Engineer" });
        _repo.Setup(r => r.GetByNameAsync("Senior Engineer", It.IsAny<CancellationToken>())).ReturnsAsync((HrJobTitle?)null);

        var result = await _sut.UpdateJobTitleAsync(1, "Senior Engineer");

        result.Name.Should().Be("Senior Engineer");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- DeactivateJobTitleAsync / ReactivateJobTitleAsync (BR-ORG-19/20, no guard) ----------

    [Fact]
    public async Task DeactivateJobTitleAsync_NoGuard_SetsInactiveAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrJobTitle { Id = 1, Status = ActiveStatus.ACTIVE });

        var result = await _sut.DeactivateJobTitleAsync(1);

        result.Status.Should().Be(ActiveStatus.INACTIVE);
    }

    [Fact]
    public async Task ReactivateJobTitleAsync_NoGuard_SetsActiveAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrJobTitle { Id = 1, Status = ActiveStatus.INACTIVE });

        var result = await _sut.ReactivateJobTitleAsync(1);

        result.Status.Should().Be(ActiveStatus.ACTIVE);
    }

    // ---------- IsJobTitleActiveAsync (cross-domain for Employee Management, BR-EMP-05) ----------

    [Fact]
    public async Task IsJobTitleActiveAsync_Active_ReturnsTrue()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrJobTitle { Id = 1, Status = ActiveStatus.ACTIVE });

        (await _sut.IsJobTitleActiveAsync(1)).Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsJobTitleActiveAsync_InactiveOrMissing_ReturnsFalse(bool exists)
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists ? new HrJobTitle { Id = 1, Status = ActiveStatus.INACTIVE } : null);

        (await _sut.IsJobTitleActiveAsync(1)).Should().BeFalse();
    }
}
