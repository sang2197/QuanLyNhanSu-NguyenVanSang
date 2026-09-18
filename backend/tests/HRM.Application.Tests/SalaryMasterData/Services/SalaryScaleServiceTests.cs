using FluentAssertions;
using HRM.Application.Common;
using HRM.Application.Exceptions;
using HRM.Application.SalaryMasterData.Interfaces;
using HRM.Application.SalaryMasterData.Services;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Moq;
using Xunit;

namespace HRM.Application.Tests.SalaryMasterData.Services;

public class SalaryScaleServiceTests
{
    private readonly Mock<ISalaryScaleRepository> _scaleRepo = new();
    private readonly Mock<ISalaryGradeRepository> _gradeRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly SalaryScaleService _sut;

    public SalaryScaleServiceTests()
    {
        // Default: no active grades in the scale, so the BR-SAL-20 deactivate
        // guard doesn't need explicit setup in every unrelated test.
        _gradeRepo.Setup(r => r.CountActiveByScaleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);

        _sut = new SalaryScaleService(_scaleRepo.Object, _gradeRepo.Object, _unitOfWork.Object);
    }

    // ---------- CreateScaleAsync (BR-SAL-05/05A/06) ----------

    [Fact]
    public async Task CreateScaleAsync_DuplicateCode_ThrowsConflict()
    {
        _scaleRepo.Setup(r => r.GetByCodeAsync("NL01", It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryScale { Id = 1, Code = "NL01" });

        var act = () => _sut.CreateScaleAsync("NL01", "Scale A");

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateScaleAsync_DuplicateName_ThrowsConflict()
    {
        _scaleRepo.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryScale?)null);
        _scaleRepo.Setup(r => r.GetByNameAsync("Scale A", It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryScale { Id = 1, Name = "Scale A" });

        var act = () => _sut.CreateScaleAsync("NL01", "Scale A");

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateScaleAsync_ValidRequest_CreatesActiveScaleAndSaves()
    {
        var scale = await _sut.CreateScaleAsync("NL01", "Scale A");

        scale.Status.Should().Be(ActiveStatus.ACTIVE);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- ListScalesAsync / GetScaleDetailAsync ----------

    [Fact]
    public async Task ListScalesAsync_ReturnsAll()
    {
        _scaleRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<HrSalaryScale> { new() { Id = 1 } });

        (await _sut.ListScalesAsync()).Should().HaveCount(1);
    }

    [Fact]
    public async Task GetScaleDetailAsync_NotFound_ThrowsNotFound()
    {
        _scaleRepo.Setup(r => r.GetDetailAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryScale?)null);

        var act = () => _sut.GetScaleDetailAsync(1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ---------- UpdateScaleAsync (BR-SAL-06/07/08) ----------

    [Fact]
    public async Task UpdateScaleAsync_NotFound_ThrowsNotFound()
    {
        _scaleRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryScale?)null);

        var act = () => _sut.UpdateScaleAsync(1, "New Name");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateScaleAsync_ChangedNameDuplicatesExisting_ThrowsConflict()
    {
        _scaleRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryScale { Id = 1, Name = "Scale A" });
        _scaleRepo.Setup(r => r.GetByNameAsync("Scale B", It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryScale { Id = 2, Name = "Scale B" });

        var act = () => _sut.UpdateScaleAsync(1, "Scale B");

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateScaleAsync_SameName_DoesNotCheckDuplicate()
    {
        _scaleRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryScale { Id = 1, Name = "Scale A" });

        await _sut.UpdateScaleAsync(1, "Scale A");

        _scaleRepo.Verify(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateScaleAsync_ValidNewName_UpdatesAndSaves()
    {
        _scaleRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryScale { Id = 1, Name = "Scale A" });
        _scaleRepo.Setup(r => r.GetByNameAsync("Scale B", It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryScale?)null);

        var result = await _sut.UpdateScaleAsync(1, "Scale B");

        result.Name.Should().Be("Scale B");
    }

    // ---------- DeactivateScaleAsync (BR-SAL-20) ----------

    [Fact]
    public async Task DeactivateScaleAsync_HasActiveGrades_ThrowsConflict()
    {
        _scaleRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryScale { Id = 1, Status = ActiveStatus.ACTIVE });
        _gradeRepo.Setup(r => r.CountActiveByScaleAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(3);

        var act = () => _sut.DeactivateScaleAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task DeactivateScaleAsync_NoActiveGrades_DeactivatesAndSaves()
    {
        _scaleRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryScale { Id = 1, Status = ActiveStatus.ACTIVE });

        var result = await _sut.DeactivateScaleAsync(1);

        result.Status.Should().Be(ActiveStatus.INACTIVE);
    }

    // ---------- ReactivateScaleAsync (BR-SAL-22, no guard) ----------

    [Fact]
    public async Task ReactivateScaleAsync_NoGuard_ReactivatesAndSaves()
    {
        _scaleRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryScale { Id = 1, Status = ActiveStatus.INACTIVE });

        var result = await _sut.ReactivateScaleAsync(1);

        result.Status.Should().Be(ActiveStatus.ACTIVE);
    }

    // ---------- CreateGradeAsync (BR-SAL-09/09A/10/11/12) ----------

    [Fact]
    public async Task CreateGradeAsync_ScaleNotFound_ThrowsNotFound()
    {
        _scaleRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryScale?)null);

        var act = () => _sut.CreateGradeAsync(1, 1, 2.0m);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateGradeAsync_ScaleInactive_ThrowsConflict()
    {
        _scaleRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryScale { Id = 1, Status = ActiveStatus.INACTIVE });

        var act = () => _sut.CreateGradeAsync(1, 1, 2.0m);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateGradeAsync_CoefficientZeroOrNegative_ThrowsValidation()
    {
        _scaleRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryScale { Id = 1, Status = ActiveStatus.ACTIVE });

        var act = () => _sut.CreateGradeAsync(1, 1, 0m);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateGradeAsync_DuplicateGradeNumberInScale_ThrowsConflict()
    {
        _scaleRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryScale { Id = 1, Status = ActiveStatus.ACTIVE });
        _gradeRepo.Setup(r => r.GetByScaleAndNumberAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryGrade { Id = 9, SalaryScaleId = 1, GradeNumber = 1 });

        var act = () => _sut.CreateGradeAsync(1, 1, 2.0m);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateGradeAsync_ValidRequest_CreatesGradeWithInitialCoefficientAndSaves()
    {
        _scaleRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryScale { Id = 1, Status = ActiveStatus.ACTIVE });
        _gradeRepo.Setup(r => r.GetByScaleAndNumberAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryGrade?)null);

        var grade = await _sut.CreateGradeAsync(1, 1, 2.0m);

        grade.Status.Should().Be(ActiveStatus.ACTIVE);
        _gradeRepo.Verify(r => r.AddAsync(It.IsAny<HrSalaryGrade>(), It.IsAny<CancellationToken>()), Times.Once);
        _gradeRepo.Verify(r => r.AddCoefficientAsync(It.Is<HrSalaryGradeCoefficient>(c => c.Coefficient == 2.0m), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- IsScaleActiveAsync (same-domain sibling + cross-domain use) ----------

    [Fact]
    public async Task IsScaleActiveAsync_Active_ReturnsTrue()
    {
        _scaleRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryScale { Id = 1, Status = ActiveStatus.ACTIVE });

        (await _sut.IsScaleActiveAsync(1)).Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsScaleActiveAsync_InactiveOrMissing_ReturnsFalse(bool exists)
    {
        _scaleRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists ? new HrSalaryScale { Id = 1, Status = ActiveStatus.INACTIVE } : null);

        (await _sut.IsScaleActiveAsync(1)).Should().BeFalse();
    }
}
