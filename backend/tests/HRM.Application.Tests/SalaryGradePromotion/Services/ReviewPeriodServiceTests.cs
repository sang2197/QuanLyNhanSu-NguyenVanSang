using FluentAssertions;
using HRM.Application.Common;
using HRM.Application.EmployeeManagement.Interfaces;
using HRM.Application.Exceptions;
using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Application.SalaryGradePromotion.Models;
using HRM.Application.SalaryGradePromotion.Rules;
using HRM.Application.SalaryGradePromotion.Services;
using HRM.Application.SalaryMasterData.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Moq;
using Xunit;

namespace HRM.Application.Tests.SalaryGradePromotion.Services;

public class ReviewPeriodServiceTests
{
    private readonly Mock<IReviewPeriodRepository> _periodRepo = new();
    private readonly Mock<IReviewEmployeeRepository> _reviewEmployeeRepo = new();
    private readonly Mock<ISalaryDecisionRepository> _decisionRepo = new();
    private readonly Mock<IEmployeeSalaryRepository> _employeeSalaryRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ISalaryPromotionEligibilityRule> _eligibilityRule = new();
    private readonly Mock<IEmployeeService> _employeeService = new();
    private readonly Mock<ISalaryGradeService> _salaryGradeService = new();
    private readonly ReviewPeriodService _sut;

    public ReviewPeriodServiceTests()
    {
        // Default: no existing periods and no active employees, so
        // CreateReviewPeriodAsync's duplicate check and screening loop don't
        // need explicit setup in tests unrelated to those specifics.
        _periodRepo.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryReviewPeriod?)null);
        _periodRepo.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryReviewPeriod?)null);
        _employeeService.Setup(e => e.GetActiveEmployeesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<HrEmployee>());

        _sut = new ReviewPeriodService(
            _periodRepo.Object, _reviewEmployeeRepo.Object, _decisionRepo.Object, _employeeSalaryRepo.Object,
            _unitOfWork.Object, _eligibilityRule.Object, _employeeService.Object, _salaryGradeService.Object);
    }

    private static CreateReviewPeriodInput ValidInput() =>
        new("RP-2026-001", "Annual Review", ReviewType.ANNUAL, new DateOnly(2026, 3, 15), null, null);

    // ---------- CreateReviewPeriodAsync (US-SGP-01/US-SGP-03) ----------

    [Fact]
    public async Task CreateReviewPeriodAsync_DuplicateCode_ThrowsConflict()
    {
        _periodRepo.Setup(r => r.GetByCodeAsync("RP-2026-001", It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryReviewPeriod { Id = 1 });

        var act = () => _sut.CreateReviewPeriodAsync(ValidInput());

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateReviewPeriodAsync_DuplicateName_ThrowsConflict()
    {
        _periodRepo.Setup(r => r.GetByNameAsync("Annual Review", It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryReviewPeriod { Id = 1 });

        var act = () => _sut.CreateReviewPeriodAsync(ValidInput());

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateReviewPeriodAsync_NoActiveEmployees_CreatesPeriodInProgressAndCommits()
    {
        var period = await _sut.CreateReviewPeriodAsync(ValidInput());

        period.Status.Should().Be(ReviewPeriodStatus.IN_PROGRESS);
        _unitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateReviewPeriodAsync_EmployeeWithNoSalaryHistory_IsSkipped()
    {
        var employee = new HrEmployee { Id = 1, EmployeeCode = "EMP-001" };
        _employeeService.Setup(e => e.GetActiveEmployeesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { employee });
        _employeeSalaryRepo.Setup(r => r.GetCurrentAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrEmployeeSalary?)null);

        await _sut.CreateReviewPeriodAsync(ValidInput());

        _reviewEmployeeRepo.Verify(r => r.AddRangeAsync(
            It.Is<IEnumerable<HrSalaryReviewEmployee>>(entries => !entries.Any()), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateReviewPeriodAsync_EligibleEmployee_SnapshotsPendingOutcomeAndProposedGrade()
    {
        var employee = new HrEmployee { Id = 1, EmployeeCode = "EMP-001" };
        var currentGrade = new HrSalaryGrade { Id = 10, SalaryScaleId = 1, GradeNumber = 3 };
        var currentSalary = new HrEmployeeSalary { EmployeeId = 1, SalaryGradeId = 10, SalaryGrade = currentGrade, Coefficient = 3.0m, EffectiveDate = new DateOnly(2020, 1, 1) };
        var nextGrade = new HrSalaryGrade { Id = 11, SalaryScaleId = 1, GradeNumber = 4 };

        _employeeService.Setup(e => e.GetActiveEmployeesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { employee });
        _employeeSalaryRepo.Setup(r => r.GetCurrentAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(currentSalary);
        _salaryGradeService.Setup(s => s.GetNextActiveGradeAsync(1, 3, It.IsAny<CancellationToken>())).ReturnsAsync(nextGrade);
        _salaryGradeService.Setup(s => s.GetCurrentCoefficientAsync(11, It.IsAny<CancellationToken>())).ReturnsAsync(3.66m);
        _eligibilityRule.Setup(r => r.DetermineEligibility(currentSalary.EffectiveDate, It.IsAny<DateOnly>(), nextGrade))
            .Returns(EligibilityResult.Eligible(nextGrade));

        List<HrSalaryReviewEmployee>? captured = null;
        _reviewEmployeeRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<HrSalaryReviewEmployee>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<HrSalaryReviewEmployee>, CancellationToken>((entries, _) => captured = entries.ToList())
            .Returns(Task.CompletedTask);

        await _sut.CreateReviewPeriodAsync(ValidInput());

        captured.Should().ContainSingle();
        captured![0].Eligible.Should().BeTrue();
        captured[0].Outcome.Should().Be(ReviewOutcome.PENDING);
        captured[0].ProposedSalaryGradeId.Should().Be(11);
        captured[0].ProposedCoefficient.Should().Be(3.66m);
    }

    [Fact]
    public async Task CreateReviewPeriodAsync_IneligibleEmployee_SnapshotsReasonAndNoOutcome()
    {
        var employee = new HrEmployee { Id = 1, EmployeeCode = "EMP-001" };
        var currentGrade = new HrSalaryGrade { Id = 10, SalaryScaleId = 1, GradeNumber = 3 };
        var currentSalary = new HrEmployeeSalary { EmployeeId = 1, SalaryGradeId = 10, SalaryGrade = currentGrade, Coefficient = 3.0m, EffectiveDate = new DateOnly(2025, 9, 1) };

        _employeeService.Setup(e => e.GetActiveEmployeesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { employee });
        _employeeSalaryRepo.Setup(r => r.GetCurrentAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(currentSalary);
        _salaryGradeService.Setup(s => s.GetNextActiveGradeAsync(1, 3, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryGrade?)null);
        _eligibilityRule.Setup(r => r.DetermineEligibility(currentSalary.EffectiveDate, It.IsAny<DateOnly>(), null))
            .Returns(EligibilityResult.Ineligible("Held current grade less than 24 months."));

        List<HrSalaryReviewEmployee>? captured = null;
        _reviewEmployeeRepo.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<HrSalaryReviewEmployee>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<HrSalaryReviewEmployee>, CancellationToken>((entries, _) => captured = entries.ToList())
            .Returns(Task.CompletedTask);

        await _sut.CreateReviewPeriodAsync(ValidInput());

        var snapshotRow = captured.Should().ContainSingle().Subject;
        snapshotRow.Eligible.Should().BeFalse();
        snapshotRow.Outcome.Should().BeNull();
        snapshotRow.ProposedSalaryGradeId.Should().BeNull();
        snapshotRow.IneligibleReason.Should().Be("Held current grade less than 24 months.");
    }

    [Fact]
    public async Task CreateReviewPeriodAsync_CalculationFails_RollsBackAndPropagates()
    {
        var employee = new HrEmployee { Id = 1, EmployeeCode = "EMP-001" };
        _employeeService.Setup(e => e.GetActiveEmployeesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { employee });
        _employeeSalaryRepo.Setup(r => r.GetCurrentAsync(1, It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("boom"));

        var act = () => _sut.CreateReviewPeriodAsync(ValidInput());

        await act.Should().ThrowAsync<InvalidOperationException>(); // US-SGP-01 AC04 — surfaces as 500, period not created
        _unitOfWork.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- SearchReviewPeriodsAsync (US-SGP-02) ----------

    [Fact]
    public async Task SearchReviewPeriodsAsync_FiltersAndPaginates()
    {
        var periods = Enumerable.Range(1, 3)
            .Select(i => new HrSalaryReviewPeriod { Id = i, ReviewType = ReviewType.ANNUAL, Status = ReviewPeriodStatus.IN_PROGRESS, ReviewDate = new DateOnly(2026, i, 1) })
            .ToList();
        _periodRepo.Setup(r => r.Query()).Returns(periods.AsQueryable());

        var result = await _sut.SearchReviewPeriodsAsync(null, null, ReviewType.ANNUAL, ReviewPeriodStatus.IN_PROGRESS, page: 1, pageSize: 2);

        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(3);
    }

    // ---------- GetReviewPeriodDetailAsync ----------

    [Fact]
    public async Task GetReviewPeriodDetailAsync_NotFound_ThrowsNotFound()
    {
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryReviewPeriod?)null);

        var act = () => _sut.GetReviewPeriodDetailAsync(1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetReviewPeriodDetailAsync_ReturnsAggregateCounts()
    {
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryReviewPeriod { Id = 1 });
        _reviewEmployeeRepo.Setup(r => r.QueryByPeriod(1)).Returns(new[]
        {
            new HrSalaryReviewEmployee { Eligible = true, Outcome = ReviewOutcome.APPROVED },
            new HrSalaryReviewEmployee { Eligible = true, Outcome = ReviewOutcome.REJECTED },
            new HrSalaryReviewEmployee { Eligible = true, Outcome = ReviewOutcome.PENDING },
            new HrSalaryReviewEmployee { Eligible = false }
        }.AsQueryable());
        _decisionRepo.Setup(r => r.GetNonCancelledByPeriodAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryDecision?)null);

        var result = await _sut.GetReviewPeriodDetailAsync(1);

        result.TotalEmployees.Should().Be(4);
        result.EligibleCount.Should().Be(3);
        result.ApprovedCount.Should().Be(1);
        result.RejectedCount.Should().Be(1);
        result.PendingCount.Should().Be(1);
        result.DecisionId.Should().BeNull();
    }

    // ---------- SubmitReviewPeriodAsync (US-SGP-05) ----------

    [Fact]
    public async Task SubmitReviewPeriodAsync_EligibleEmployeesStillUnprocessed_ThrowsConflict()
    {
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.IN_PROGRESS });
        _reviewEmployeeRepo.Setup(r => r.CountUnprocessedEligibleAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(2);

        var act = () => _sut.SubmitReviewPeriodAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task SubmitReviewPeriodAsync_AllProcessed_SubmitsAndSaves()
    {
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.IN_PROGRESS });
        _reviewEmployeeRepo.Setup(r => r.CountUnprocessedEligibleAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await _sut.SubmitReviewPeriodAsync(1);

        result.Status.Should().Be(ReviewPeriodStatus.SUBMITTED);
    }

    // ---------- CancelReviewPeriodAsync (US-SGP-11) ----------

    [Theory]
    [InlineData(ReviewPeriodStatus.CLOSED)]
    [InlineData(ReviewPeriodStatus.CANCELLED)]
    public async Task CancelReviewPeriodAsync_AlreadyTerminal_ThrowsConflict(ReviewPeriodStatus status)
    {
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = status });

        var act = () => _sut.CancelReviewPeriodAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CancelReviewPeriodAsync_HasNonCancelledDecision_ThrowsConflict()
    {
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.SUBMITTED });
        _decisionRepo.Setup(r => r.GetNonCancelledByPeriodAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryDecision { Id = 5 });

        var act = () => _sut.CancelReviewPeriodAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CancelReviewPeriodAsync_NoDecision_CancelsAndSaves()
    {
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.IN_PROGRESS });
        _decisionRepo.Setup(r => r.GetNonCancelledByPeriodAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryDecision?)null);

        var result = await _sut.CancelReviewPeriodAsync(1);

        result.Status.Should().Be(ReviewPeriodStatus.CANCELLED);
    }
}
