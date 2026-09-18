using FluentAssertions;
using HRM.Application.Common;
using HRM.Application.Exceptions;
using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Application.SalaryGradePromotion.Models;
using HRM.Application.SalaryGradePromotion.Services;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Moq;
using Xunit;

namespace HRM.Application.Tests.SalaryGradePromotion.Services;

public class SalaryDecisionServiceTests
{
    private readonly Mock<ISalaryDecisionRepository> _decisionRepo = new();
    private readonly Mock<IReviewPeriodRepository> _periodRepo = new();
    private readonly Mock<IReviewEmployeeRepository> _reviewEmployeeRepo = new();
    private readonly Mock<IEmployeeSalaryRepository> _employeeSalaryRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly SalaryDecisionService _sut;

    public SalaryDecisionServiceTests()
    {
        _decisionRepo.Setup(r => r.NextDecisionNumberAsync(It.IsAny<CancellationToken>())).ReturnsAsync("SD-2026-001");

        _sut = new SalaryDecisionService(_decisionRepo.Object, _periodRepo.Object, _reviewEmployeeRepo.Object, _employeeSalaryRepo.Object, _unitOfWork.Object);
    }

    private static CreateSalaryDecisionInput ValidInput(int periodId = 1) =>
        new(periodId, new[] { 1 }, new DateOnly(2026, 4, 1));

    private static HrSalaryReviewPeriod SubmittedPeriod(int id = 1) =>
        new() { Id = id, Status = ReviewPeriodStatus.SUBMITTED, ReviewDate = new DateOnly(2026, 3, 1) };

    // ---------- CreateDecisionAsync (US-SGP-06) ----------

    [Fact]
    public async Task CreateDecisionAsync_PeriodNotFound_ThrowsNotFound()
    {
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryReviewPeriod?)null);

        var act = () => _sut.CreateDecisionAsync(ValidInput());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateDecisionAsync_PeriodNotSubmitted_ThrowsConflict()
    {
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.IN_PROGRESS });

        var act = () => _sut.CreateDecisionAsync(ValidInput());

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateDecisionAsync_NonCancelledDecisionExists_ThrowsConflict()
    {
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(SubmittedPeriod());
        _decisionRepo.Setup(r => r.GetNonCancelledByPeriodAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryDecision { Id = 9 });

        var act = () => _sut.CreateDecisionAsync(ValidInput());

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateDecisionAsync_EffectiveDateBeforeReviewDate_ThrowsValidation()
    {
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(SubmittedPeriod());
        _decisionRepo.Setup(r => r.GetNonCancelledByPeriodAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryDecision?)null);

        var input = new CreateSalaryDecisionInput(1, new[] { 1 }, new DateOnly(2026, 1, 1)); // before ReviewDate 2026-03-01

        var act = () => _sut.CreateDecisionAsync(input);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateDecisionAsync_NotAllEmployeesApproved_ThrowsValidation()
    {
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(SubmittedPeriod());
        _decisionRepo.Setup(r => r.GetNonCancelledByPeriodAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryDecision?)null);
        _reviewEmployeeRepo.Setup(r => r.GetApprovedAsync(1, It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HrSalaryReviewEmployee>()); // requested 1 employee, 0 approved

        var act = () => _sut.CreateDecisionAsync(ValidInput());

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateDecisionAsync_Valid_CreatesDraftWithDetailLinesAndSaves()
    {
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(SubmittedPeriod());
        _decisionRepo.Setup(r => r.GetNonCancelledByPeriodAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryDecision?)null);
        _reviewEmployeeRepo.Setup(r => r.GetApprovedAsync(1, It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new HrSalaryReviewEmployee { EmployeeId = 1, CurrentSalaryGradeId = 10, CurrentCoefficient = 3.0m, ProposedSalaryGradeId = 11, ProposedCoefficient = 3.66m } });

        var decision = await _sut.CreateDecisionAsync(ValidInput());

        decision.Status.Should().Be(SalaryDecisionStatus.DRAFT);
        decision.DecisionNumber.Should().Be("SD-2026-001");
        decision.Details.Should().ContainSingle(d => d.EmployeeId == 1 && d.NewSalaryGradeId == 11);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- GetEligibleReviewPeriodsAsync (US-SGP-09 AC04) ----------

    [Fact]
    public async Task GetEligibleReviewPeriodsAsync_ReturnsFromRepository()
    {
        _periodRepo.Setup(r => r.QueryEligibleForDecision()).Returns(new[] { new HrSalaryReviewPeriod { Id = 1 } }.AsQueryable());

        (await _sut.GetEligibleReviewPeriodsAsync()).Should().ContainSingle();
    }

    // ---------- SaveDraftAsync / RemoveEmployeeAsync (US-SGP-06 AC04/AC05) ----------

    [Fact]
    public async Task SaveDraftAsync_NotDraft_ThrowsConflict()
    {
        _decisionRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryDecision { Id = 1, Status = SalaryDecisionStatus.APPLIED });

        var act = () => _sut.SaveDraftAsync(1, new DateOnly(2026, 5, 1));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task SaveDraftAsync_Draft_UpdatesEffectiveDateAndSaves()
    {
        _decisionRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryDecision { Id = 1, Status = SalaryDecisionStatus.DRAFT });

        var result = await _sut.SaveDraftAsync(1, new DateOnly(2026, 5, 1));

        result.EffectiveDate.Should().Be(new DateOnly(2026, 5, 1));
    }

    [Fact]
    public async Task RemoveEmployeeAsync_NotDraft_ThrowsConflict()
    {
        var decision = new HrSalaryDecision { Id = 1, Status = SalaryDecisionStatus.APPLIED };
        decision.Details.Add(new HrSalaryDecisionDetail { EmployeeId = 1 });
        _decisionRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(decision);

        var act = () => _sut.RemoveEmployeeAsync(1, 1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task RemoveEmployeeAsync_EmployeeNotInDecision_ThrowsNotFound()
    {
        var decision = new HrSalaryDecision { Id = 1, Status = SalaryDecisionStatus.DRAFT };
        _decisionRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(decision);

        var act = () => _sut.RemoveEmployeeAsync(1, 99);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RemoveEmployeeAsync_Valid_RemovesDetailAndSaves()
    {
        var decision = new HrSalaryDecision { Id = 1, Status = SalaryDecisionStatus.DRAFT };
        var detail = new HrSalaryDecisionDetail { EmployeeId = 1 };
        decision.Details.Add(detail);
        _decisionRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(decision);

        await _sut.RemoveEmployeeAsync(1, 1);

        _decisionRepo.Verify(r => r.RemoveDetailAsync(detail, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- ApplyDecisionAsync (US-SGP-07) — all-or-nothing ----------

    private static HrSalaryDecision DraftDecisionWithOneDetail() =>
        new()
        {
            Id = 1,
            ReviewPeriodId = 1,
            DecisionNumber = "SD-2026-001",
            EffectiveDate = new DateOnly(2026, 4, 1),
            Status = SalaryDecisionStatus.DRAFT,
            Details = { new HrSalaryDecisionDetail { EmployeeId = 1, BaselineSalaryGradeId = 10, NewSalaryGradeId = 11, NewCoefficient = 3.66m } }
        };

    [Fact]
    public async Task ApplyDecisionAsync_NotDraft_ThrowsConflict()
    {
        var decision = DraftDecisionWithOneDetail();
        decision.Status = SalaryDecisionStatus.APPLIED;
        _decisionRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(decision);

        var act = () => _sut.ApplyDecisionAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ApplyDecisionAsync_EmployeeGradeNoLongerMatchesBaseline_ThrowsConflictAndWritesNothing()
    {
        var decision = DraftDecisionWithOneDetail();
        _decisionRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(decision);
        _employeeSalaryRepo.Setup(r => r.GetCurrentAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrEmployeeSalary { EmployeeId = 1, SalaryGradeId = 99 }); // no longer grade 10

        var act = () => _sut.ApplyDecisionAsync(1);

        await act.Should().ThrowAsync<ConflictException>(); // US-SGP-07 AC02/AC04
        _employeeSalaryRepo.Verify(r => r.AddAsync(It.IsAny<HrEmployeeSalary>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never); // validation runs before any transaction
    }

    [Fact]
    public async Task ApplyDecisionAsync_Valid_AppliesAndClosesReviewPeriod()
    {
        var decision = DraftDecisionWithOneDetail();
        _decisionRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(decision);
        _employeeSalaryRepo.Setup(r => r.GetCurrentAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrEmployeeSalary { EmployeeId = 1, SalaryGradeId = 10 }); // matches baseline
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.SUBMITTED });

        var result = await _sut.ApplyDecisionAsync(1);

        result.Status.Should().Be(SalaryDecisionStatus.APPLIED);
        _employeeSalaryRepo.Verify(r => r.AddAsync(It.Is<HrEmployeeSalary>(s => s.EmployeeId == 1 && s.SalaryGradeId == 11 && s.SalaryDecisionId == 1), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- CancelDecisionAsync (US-SGP-10) ----------

    [Theory]
    [InlineData(SalaryDecisionStatus.APPLIED)]
    [InlineData(SalaryDecisionStatus.CANCELLED)]
    public async Task CancelDecisionAsync_NotDraft_ThrowsConflict(SalaryDecisionStatus status)
    {
        _decisionRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryDecision { Id = 1, Status = status });

        var act = () => _sut.CancelDecisionAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CancelDecisionAsync_Draft_CancelsWithoutTouchingSalaryHistory()
    {
        _decisionRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryDecision { Id = 1, Status = SalaryDecisionStatus.DRAFT });

        var result = await _sut.CancelDecisionAsync(1);

        result.Status.Should().Be(SalaryDecisionStatus.CANCELLED);
        _employeeSalaryRepo.Verify(r => r.AddAsync(It.IsAny<HrEmployeeSalary>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
