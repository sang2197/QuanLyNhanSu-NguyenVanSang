using FluentAssertions;
using HRM.Application.Exceptions;
using HRM.Application.SalaryManagement.Interfaces;
using HRM.Application.SalaryManagement.Models;
using HRM.Application.SalaryManagement.Services;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Moq;
using Xunit;

namespace HRM.Application.Tests.SalaryManagement.Services;

public class SalaryDecisionServiceTests
{
    private readonly Mock<ISalaryRepository> _salaryRepo = new();
    private readonly SalaryDecisionService _sut;

    public SalaryDecisionServiceTests()
    {
        _sut = new SalaryDecisionService(_salaryRepo.Object);
    }

    private static CreateDecisionInput Input(int periodId, params int[] employeeIds) =>
        new(periodId, employeeIds, "SD-2026-001", DecisionType.PERIODIC, new DateOnly(2026, 4, 1), null, null);

    // ---------- CreateDecision (US-06) ----------

    [Fact]
    public async Task CreateDecision_PeriodNotSubmitted_ThrowsConflict()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.IN_PROGRESS });

        var act = () => _sut.CreateDecisionAsync(Input(1, 5));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateDecision_PeriodAlreadyHasNonCancelledDecision_ThrowsConflict()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.SUBMITTED });
        _salaryRepo.Setup(r => r.HasNonCancelledDecisionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.CreateDecisionAsync(Input(1, 5));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateDecision_EmployeeNotApprovedInPeriod_ThrowsValidation()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.SUBMITTED });
        _salaryRepo.Setup(r => r.HasNonCancelledDecisionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _salaryRepo.Setup(r => r.GetReviewEmployeeAsync(1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewEmployee { EmployeeId = 5, ReviewStatus = ReviewOutcome.PENDING });

        var act = () => _sut.CreateDecisionAsync(Input(1, 5));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateDecision_AllApproved_CopiesProposedGradeIntoDetail_AsDraft()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.SUBMITTED });
        _salaryRepo.Setup(r => r.HasNonCancelledDecisionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _salaryRepo.Setup(r => r.GetReviewEmployeeAsync(1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewEmployee
            {
                EmployeeId = 5,
                ReviewStatus = ReviewOutcome.APPROVED,
                CurrentSalaryId = 100,
                CurrentGradeId = 10,
                CurrentCoefficient = 3.33m,
                ProposedGradeId = 11,
                ProposedCoefficient = 3.66m
            });

        HrSalaryDecision? captured = null;
        _salaryRepo.Setup(r => r.AddDecisionAsync(It.IsAny<HrSalaryDecision>(), It.IsAny<CancellationToken>()))
            .Callback<HrSalaryDecision, CancellationToken>((d, _) => captured = d)
            .Returns(Task.CompletedTask);

        var decision = await _sut.CreateDecisionAsync(Input(1, 5));

        decision.Status.Should().Be(SalaryDecisionStatus.DRAFT);
        decision.ReviewPeriodId.Should().Be(1);
        captured!.Details.Should().ContainSingle();
        var detail = captured.Details.Single();
        detail.EmployeeId.Should().Be(5);
        detail.OldGradeId.Should().Be(10);
        detail.NewSalaryGradeId.Should().Be(11);
        detail.NewCoefficient.Should().Be(3.66m);
    }

    // ---------- RemoveEmployee (US-06) ----------

    [Fact]
    public async Task RemoveEmployee_DecisionNotDraft_ThrowsConflict()
    {
        var decision = new HrSalaryDecision { Id = 1, Status = SalaryDecisionStatus.APPLIED, Details = new List<HrSalaryDecisionDetail>() };
        _salaryRepo.Setup(r => r.GetDecisionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(decision);

        var act = () => _sut.RemoveEmployeeAsync(1, 5);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task RemoveEmployee_Draft_RemovesDetail()
    {
        var detail = new HrSalaryDecisionDetail { EmployeeId = 5 };
        var decision = new HrSalaryDecision { Id = 1, Status = SalaryDecisionStatus.DRAFT, Details = new List<HrSalaryDecisionDetail> { detail } };
        _salaryRepo.Setup(r => r.GetDecisionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(decision);

        await _sut.RemoveEmployeeAsync(1, 5);

        _salaryRepo.Verify(r => r.RemoveDecisionDetailAsync(detail, It.IsAny<CancellationToken>()), Times.Once);
        _salaryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- ApplyDecision (US-07 — all-or-nothing) ----------

    [Fact]
    public async Task ApplyDecision_NotDraft_ThrowsConflict()
    {
        var decision = new HrSalaryDecision { Id = 1, Status = SalaryDecisionStatus.APPLIED, Details = new List<HrSalaryDecisionDetail>() };
        _salaryRepo.Setup(r => r.GetDecisionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(decision);

        var act = () => _sut.ApplyDecisionAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ApplyDecision_OneEmployeeHasConflict_NoOneIsUpdated()
    {
        var grade = new HrSalaryGrade { Id = 11, SalaryScaleId = 1, GradeNumber = 4, Coefficient = 3.66m };
        var detail1 = new HrSalaryDecisionDetail { EmployeeId = 5, EffectiveFrom = new DateOnly(2026, 4, 1), NewSalaryGradeId = 11, NewCoefficient = 3.66m, NewSalaryGrade = grade };
        var detail2 = new HrSalaryDecisionDetail { EmployeeId = 6, EffectiveFrom = new DateOnly(2026, 4, 1), NewSalaryGradeId = 11, NewCoefficient = 3.66m, NewSalaryGrade = grade };
        var decision = new HrSalaryDecision { Id = 1, DecisionNumber = "SD-2026-001", Status = SalaryDecisionStatus.DRAFT, Details = new List<HrSalaryDecisionDetail> { detail1, detail2 } };
        _salaryRepo.Setup(r => r.GetDecisionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(decision);

        _salaryRepo.Setup(r => r.HasEffectiveDateConflictAsync(5, detail1.EffectiveFrom, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _salaryRepo.Setup(r => r.HasEffectiveDateConflictAsync(6, detail2.EffectiveFrom, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.ApplyDecisionAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
        // All-or-nothing: neither employee's salary should have been touched.
        _salaryRepo.Verify(r => r.CloseSalaryAsync(It.IsAny<HrEmployeeSalary>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        _salaryRepo.Verify(r => r.AddSalaryAsync(It.IsAny<HrEmployeeSalary>(), It.IsAny<CancellationToken>()), Times.Never);
        _salaryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApplyDecision_NoConflicts_ClosesOldSalaryAndCreatesNew_ForEveryEmployee_AndClosesReviewPeriod()
    {
        var grade = new HrSalaryGrade { Id = 11, SalaryScaleId = 1, GradeNumber = 4, Coefficient = 3.66m };
        var detail = new HrSalaryDecisionDetail { EmployeeId = 5, EffectiveFrom = new DateOnly(2026, 4, 1), NewSalaryGradeId = 11, NewCoefficient = 3.66m, NewSalaryGrade = grade };
        var reviewPeriod = new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.SUBMITTED };
        var decision = new HrSalaryDecision { Id = 1, DecisionNumber = "SD-2026-001", Status = SalaryDecisionStatus.DRAFT, Details = new List<HrSalaryDecisionDetail> { detail }, ReviewPeriod = reviewPeriod };
        _salaryRepo.Setup(r => r.GetDecisionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(decision);
        _salaryRepo.Setup(r => r.HasEffectiveDateConflictAsync(5, detail.EffectiveFrom, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var currentSalary = new HrEmployeeSalary { Id = 200, EmployeeId = 5, SalaryScaleId = 1, EffectiveFrom = new DateOnly(2020, 1, 1) };
        _salaryRepo.Setup(r => r.GetCurrentSalaryAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(currentSalary);

        var result = await _sut.ApplyDecisionAsync(1);

        result.Status.Should().Be(SalaryDecisionStatus.APPLIED);
        reviewPeriod.Status.Should().Be(ReviewPeriodStatus.CLOSED);
        _salaryRepo.Verify(r => r.CloseSalaryAsync(currentSalary, detail.EffectiveFrom.AddDays(-1), It.IsAny<CancellationToken>()), Times.Once);
        _salaryRepo.Verify(r => r.AddSalaryAsync(It.Is<HrEmployeeSalary>(s => s.EmployeeId == 5 && s.SalaryGradeId == 11 && s.DecisionId == 1), It.IsAny<CancellationToken>()), Times.Once);
        _salaryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- CancelDecision (US-10) — Draft only, Applied is permanent ----------

    [Fact]
    public async Task CancelDecision_AlreadyCancelled_ThrowsConflict()
    {
        var decision = new HrSalaryDecision { Id = 1, Status = SalaryDecisionStatus.CANCELLED, Details = new List<HrSalaryDecisionDetail>() };
        _salaryRepo.Setup(r => r.GetDecisionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(decision);

        var act = () => _sut.CancelDecisionAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CancelDecision_Applied_ThrowsConflict_AppliedIsPermanent()
    {
        var decision = new HrSalaryDecision { Id = 1, Status = SalaryDecisionStatus.APPLIED, Details = new List<HrSalaryDecisionDetail>() };
        _salaryRepo.Setup(r => r.GetDecisionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(decision);

        var act = () => _sut.CancelDecisionAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
        decision.Status.Should().Be(SalaryDecisionStatus.APPLIED);
    }

    [Fact]
    public async Task CancelDecision_Draft_SetsCancelled_AndNeverTouchesSalary()
    {
        var decision = new HrSalaryDecision { Id = 1, Status = SalaryDecisionStatus.DRAFT, Details = new List<HrSalaryDecisionDetail>() };
        _salaryRepo.Setup(r => r.GetDecisionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(decision);

        var result = await _sut.CancelDecisionAsync(1);

        result.Status.Should().Be(SalaryDecisionStatus.CANCELLED);
        _salaryRepo.Verify(r => r.CloseSalaryAsync(It.IsAny<HrEmployeeSalary>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        _salaryRepo.Verify(r => r.AddSalaryAsync(It.IsAny<HrEmployeeSalary>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
