using FluentAssertions;
using HRM.Application.EmployeeManagement.Interfaces;
using HRM.Application.EmployeeManagement.Models;
using HRM.Application.EmployeeManagement.Services;
using HRM.Application.Common;
using HRM.Application.Exceptions;
using HRM.Application.OrganizationManagement.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Moq;
using Xunit;

namespace HRM.Application.Tests.EmployeeManagement.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IOrganizationalUnitService> _organizationalUnitService = new();
    private readonly Mock<IJobTitleService> _jobTitleService = new();
    private readonly EmployeeService _sut;

    public EmployeeServiceTests()
    {
        // Default: unit and job title are active, so the BR-EMP-04/05 guards
        // don't need explicit setup in every unrelated test.
        _organizationalUnitService.Setup(s => s.IsUnitActiveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _jobTitleService.Setup(s => s.IsJobTitleActiveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _sut = new EmployeeService(_repo.Object, _unitOfWork.Object, _organizationalUnitService.Object, _jobTitleService.Object);
    }

    private static CreateEmployeeInput ValidCreateInput(string code = "EMP-001") =>
        new(code, "An Nguyen", OrganizationalUnitId: 1, JobTitleId: 1, new DateOnly(2024, 1, 1), EmploymentStatus.ACTIVE);

    // ---------- CreateEmployeeAsync (BR-EMP-01/02/04/05) ----------

    [Fact]
    public async Task CreateEmployeeAsync_DuplicateCode_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByCodeAsync("EMP-001", It.IsAny<CancellationToken>())).ReturnsAsync(new HrEmployee { Id = 1, EmployeeCode = "EMP-001" });

        var act = () => _sut.CreateEmployeeAsync(ValidCreateInput());

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateEmployeeAsync_OrganizationalUnitInactive_ThrowsValidation()
    {
        _organizationalUnitService.Setup(s => s.IsUnitActiveAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = () => _sut.CreateEmployeeAsync(ValidCreateInput());

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateEmployeeAsync_JobTitleInactive_ThrowsValidation()
    {
        _jobTitleService.Setup(s => s.IsJobTitleActiveAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = () => _sut.CreateEmployeeAsync(ValidCreateInput());

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateEmployeeAsync_ValidRequest_CreatesAndSaves()
    {
        var employee = await _sut.CreateEmployeeAsync(ValidCreateInput());

        employee.EmployeeCode.Should().Be("EMP-001");
        employee.EmploymentStatus.Should().Be(EmploymentStatus.ACTIVE);
        _repo.Verify(r => r.AddAsync(It.IsAny<HrEmployee>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- SearchEmployeesAsync (BR-EMP-06/07) ----------

    [Fact]
    public async Task SearchEmployeesAsync_FiltersByCodeOrName()
    {
        var employees = new List<HrEmployee>
        {
            new() { Id = 1, EmployeeCode = "EMP-001", FullName = "An Nguyen" },
            new() { Id = 2, EmployeeCode = "EMP-002", FullName = "Binh Tran" }
        };
        _repo.Setup(r => r.Query()).Returns(employees.AsQueryable());

        var result = await _sut.SearchEmployeesAsync("An", null, null, null, 1, 20);

        result.Items.Should().ContainSingle(e => e.Id == 1);
    }

    [Fact]
    public async Task SearchEmployeesAsync_CombinesFiltersAndPaginates()
    {
        var employees = Enumerable.Range(1, 5)
            .Select(i => new HrEmployee { Id = i, EmployeeCode = $"EMP-{i:000}", FullName = "Someone", OrganizationalUnitId = 1, EmploymentStatus = EmploymentStatus.ACTIVE })
            .ToList();
        _repo.Setup(r => r.Query()).Returns(employees.AsQueryable());

        var result = await _sut.SearchEmployeesAsync(null, 1, null, EmploymentStatus.ACTIVE, page: 2, pageSize: 2);

        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(5);
        result.Page.Should().Be(2);
    }

    // ---------- GetEmployeeAsync ----------

    [Fact]
    public async Task GetEmployeeAsync_NotFound_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrEmployee?)null);

        var act = () => _sut.GetEmployeeAsync(1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ---------- UpdateEmployeeAsync (BR-EMP-03/04/05/08/09) ----------

    [Fact]
    public async Task UpdateEmployeeAsync_NotFound_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrEmployee?)null);

        var act = () => _sut.UpdateEmployeeAsync(1, new UpdateEmployeeInput(null, null, null, null, null));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateEmployeeAsync_ChangedCodeDuplicatesExisting_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrEmployee { Id = 1, EmployeeCode = "EMP-001" });
        _repo.Setup(r => r.GetByCodeAsync("EMP-002", It.IsAny<CancellationToken>())).ReturnsAsync(new HrEmployee { Id = 2, EmployeeCode = "EMP-002" });

        var act = () => _sut.UpdateEmployeeAsync(1, new UpdateEmployeeInput("EMP-002", null, null, null, null));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateEmployeeAsync_ChangedUnitInactive_ThrowsValidation()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrEmployee { Id = 1, OrganizationalUnitId = 1 });
        _organizationalUnitService.Setup(s => s.IsUnitActiveAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = () => _sut.UpdateEmployeeAsync(1, new UpdateEmployeeInput(null, null, 2, null, null));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateEmployeeAsync_ChangedJobTitleInactive_ThrowsValidation()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrEmployee { Id = 1, JobTitleId = 1 });
        _jobTitleService.Setup(s => s.IsJobTitleActiveAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = () => _sut.UpdateEmployeeAsync(1, new UpdateEmployeeInput(null, null, null, 2, null));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateEmployeeAsync_UnchangedUnit_DoesNotRevalidate()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrEmployee { Id = 1, OrganizationalUnitId = 1 });

        await _sut.UpdateEmployeeAsync(1, new UpdateEmployeeInput(null, null, 1, null, null));

        _organizationalUnitService.Verify(s => s.IsUnitActiveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_ValidChanges_UpdatesAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrEmployee { Id = 1, FullName = "Old Name" });

        var result = await _sut.UpdateEmployeeAsync(1, new UpdateEmployeeInput(null, "New Name", null, null, null));

        result.FullName.Should().Be("New Name");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- ChangeEmploymentStatusAsync (BR-EMP-10/11, OQ-EMP-01) ----------

    [Fact]
    public async Task ChangeEmploymentStatusAsync_CurrentlyTerminated_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrEmployee { Id = 1, EmploymentStatus = EmploymentStatus.TERMINATED });

        var act = () => _sut.ChangeEmploymentStatusAsync(1, EmploymentStatus.ACTIVE);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Theory]
    [InlineData(EmploymentStatus.ACTIVE, EmploymentStatus.ON_LEAVE)]
    [InlineData(EmploymentStatus.ON_LEAVE, EmploymentStatus.ACTIVE)]
    [InlineData(EmploymentStatus.ACTIVE, EmploymentStatus.TERMINATED)]
    [InlineData(EmploymentStatus.ON_LEAVE, EmploymentStatus.TERMINATED)]
    public async Task ChangeEmploymentStatusAsync_ValidTransition_UpdatesAndSaves(EmploymentStatus from, EmploymentStatus to)
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrEmployee { Id = 1, EmploymentStatus = from });

        var result = await _sut.ChangeEmploymentStatusAsync(1, to);

        result.EmploymentStatus.Should().Be(to);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- Cross-domain exposure (ADR-03) ----------

    [Fact]
    public async Task HasActiveEmployeesInUnitAsync_DelegatesToRepository()
    {
        _repo.Setup(r => r.HasActiveInUnitAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        (await _sut.HasActiveEmployeesInUnitAsync(5)).Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveEmployeesAsync_ReturnsFromRepository()
    {
        _repo.Setup(r => r.QueryActive()).Returns(new[] { new HrEmployee { Id = 1, EmploymentStatus = EmploymentStatus.ACTIVE } }.AsQueryable());

        (await _sut.GetActiveEmployeesAsync()).Should().ContainSingle();
    }
}
