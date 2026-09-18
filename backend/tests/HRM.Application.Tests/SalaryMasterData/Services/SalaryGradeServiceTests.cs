using FluentAssertions;
using HRM.Application.Common;
using HRM.Application.Exceptions;
using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Application.SalaryMasterData.Interfaces;
using HRM.Application.SalaryMasterData.Services;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Moq;
using Xunit;

namespace HRM.Application.Tests.SalaryMasterData.Services;

public class SalaryGradeServiceTests
{
    private readonly Mock<ISalaryGradeRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ISalaryScaleService> _scaleService = new();
    private readonly Mock<ISalaryHistoryService> _historyService = new();
    private readonly SalaryGradeService _sut;

    public SalaryGradeServiceTests()
    {
        // Default: no active employee on the grade and the scale is active,
        // so the BR-SAL-15/18 guards don't need explicit setup in every
        // unrelated test.
        _historyService.Setup(h => h.HasActiveEmployeeOnGradeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _scaleService.Setup(s => s.IsScaleActiveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _sut = new SalaryGradeService(_repo.Object, _unitOfWork.Object, _scaleService.Object, new Lazy<ISalaryHistoryService>(() => _historyService.Object));
    }

    // ---------- AddCoefficientAsync (BR-SAL-10/12/13/13A) ----------

    [Fact]
    public async Task AddCoefficientAsync_GradeNotFound_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryGrade?)null);

        var act = () => _sut.AddCoefficientAsync(1, 2.0m, new DateOnly(2026, 1, 1));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AddCoefficientAsync_GradeInactive_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryGrade { Id = 1, Status = ActiveStatus.INACTIVE });

        var act = () => _sut.AddCoefficientAsync(1, 2.0m, new DateOnly(2026, 1, 1));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task AddCoefficientAsync_CoefficientZeroOrNegative_ThrowsValidation()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryGrade { Id = 1, Status = ActiveStatus.ACTIVE });

        var act = () => _sut.AddCoefficientAsync(1, 0m, new DateOnly(2026, 1, 1));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task AddCoefficientAsync_EffectiveDateNotAfterLatest_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryGrade { Id = 1, Status = ActiveStatus.ACTIVE });
        _repo.Setup(r => r.GetLatestCoefficientAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryGradeCoefficient { EffectiveDate = new DateOnly(2026, 1, 1) });

        var act = () => _sut.AddCoefficientAsync(1, 2.0m, new DateOnly(2026, 1, 1));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task AddCoefficientAsync_ValidRequest_AddsAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryGrade { Id = 1, Status = ActiveStatus.ACTIVE });
        _repo.Setup(r => r.GetLatestCoefficientAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryGradeCoefficient?)null);

        var result = await _sut.AddCoefficientAsync(1, 2.5m, new DateOnly(2026, 1, 1));

        result.Coefficient.Should().Be(2.5m);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- ListCoefficientsAsync ----------

    [Fact]
    public async Task ListCoefficientsAsync_ReturnsAll()
    {
        _repo.Setup(r => r.ListCoefficientsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HrSalaryGradeCoefficient> { new() { Id = 1 } });

        (await _sut.ListCoefficientsAsync(1)).Should().HaveCount(1);
    }

    // ---------- DeactivateGradeAsync (BR-SAL-15, cross-domain) ----------

    [Fact]
    public async Task DeactivateGradeAsync_ActiveEmployeeAssigned_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryGrade { Id = 1, Status = ActiveStatus.ACTIVE });
        _historyService.Setup(h => h.HasActiveEmployeeOnGradeAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.DeactivateGradeAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task DeactivateGradeAsync_NoActiveEmployee_DeactivatesAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryGrade { Id = 1, Status = ActiveStatus.ACTIVE });

        var result = await _sut.DeactivateGradeAsync(1);

        result.Status.Should().Be(ActiveStatus.INACTIVE);
    }

    // ---------- ReactivateGradeAsync (BR-SAL-18) ----------

    [Fact]
    public async Task ReactivateGradeAsync_ScaleInactive_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryGrade { Id = 1, SalaryScaleId = 5, Status = ActiveStatus.INACTIVE });
        _scaleService.Setup(s => s.IsScaleActiveAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = () => _sut.ReactivateGradeAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ReactivateGradeAsync_ScaleActive_ReactivatesAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryGrade { Id = 1, SalaryScaleId = 5, Status = ActiveStatus.INACTIVE });

        var result = await _sut.ReactivateGradeAsync(1);

        result.Status.Should().Be(ActiveStatus.ACTIVE);
    }

    // ---------- GetNextActiveGradeAsync (BR-SAL-17, exposed cross-domain) ----------

    [Fact]
    public async Task GetNextActiveGradeAsync_NoHigherGrade_ReturnsNull()
    {
        _repo.Setup(r => r.ListByScaleAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HrSalaryGrade> { new() { Id = 1, GradeNumber = 3, Status = ActiveStatus.ACTIVE } });

        (await _sut.GetNextActiveGradeAsync(1, 3)).Should().BeNull();
    }

    [Fact]
    public async Task GetNextActiveGradeAsync_InactiveGradesSkipped_ReturnsFirstActiveAbove()
    {
        _repo.Setup(r => r.ListByScaleAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new List<HrSalaryGrade>
        {
            new() { Id = 1, GradeNumber = 1, Status = ActiveStatus.ACTIVE },
            new() { Id = 2, GradeNumber = 2, Status = ActiveStatus.INACTIVE },
            new() { Id = 3, GradeNumber = 3, Status = ActiveStatus.ACTIVE }
        });

        var next = await _sut.GetNextActiveGradeAsync(1, 1);

        next!.Id.Should().Be(3);
    }

    // ---------- GetCurrentCoefficientAsync (exposed cross-domain) ----------

    [Fact]
    public async Task GetCurrentCoefficientAsync_NoCoefficientRecorded_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetLatestCoefficientAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryGradeCoefficient?)null);

        var act = () => _sut.GetCurrentCoefficientAsync(1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetCurrentCoefficientAsync_HasCoefficient_ReturnsLatestValue()
    {
        _repo.Setup(r => r.GetLatestCoefficientAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryGradeCoefficient { Coefficient = 3.5m });

        (await _sut.GetCurrentCoefficientAsync(1)).Should().Be(3.5m);
    }
}
