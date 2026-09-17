using FluentAssertions;
using HRM.Application.Exceptions;
using HRM.Application.SalaryManagement.Interfaces;
using HRM.Application.SalaryManagement.Models;
using HRM.Application.SalaryManagement.Rules;
using HRM.Application.SalaryManagement.Services;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Moq;
using Xunit;

namespace HRM.Application.Tests.SalaryManagement.Services;

public class SalaryReviewServiceTests
{
    private readonly Mock<ISalaryRepository> _salaryRepo = new();
    private readonly Mock<IEmployeeRepository> _employeeRepo = new();
    private readonly Mock<IEligibilityRule> _eligibilityRule = new();
    private readonly SalaryReviewService _sut;

    public SalaryReviewServiceTests()
    {
        _sut = new SalaryReviewService(_salaryRepo.Object, _employeeRepo.Object, _eligibilityRule.Object);
    }

    // ---------- CreateReviewPeriod (US-01) ----------

    [Fact]
    public async Task CreateReviewPeriod_DuplicateCode_ThrowsConflict()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodByCodeAsync("RP-2026-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Code = "RP-2026-001" });

        var input = new CreateReviewPeriodInput("RP-2026-001", "Annual Review", ReviewType.PERIODIC, new DateOnly(2026, 3, 15), null, null);

        var act = () => _sut.CreateReviewPeriodAsync(input);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateReviewPeriod_CalculatesEligibilityForEveryActiveEmployee_AndSetsStatusInProgress()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HrSalaryReviewPeriod?)null);

        var employee1 = new HrEmployee { Id = 1, EmployeeCode = "EMP-001", FullName = "An" };
        var employee2 = new HrEmployee { Id = 2, EmployeeCode = "EMP-002", FullName = "Binh" };
        _employeeRepo.Setup(r => r.QueryActive()).Returns(new[] { employee1, employee2 }.AsQueryable());

        var grade3 = new HrSalaryGrade { Id = 10, SalaryScaleId = 1, GradeNumber = 3, Coefficient = 3.33m };
        var salary1 = new HrEmployeeSalary { Id = 100, EmployeeId = 1, SalaryScaleId = 1, SalaryGradeId = 10, SalaryGrade = grade3, Coefficient = 3.33m, EffectiveFrom = new DateOnly(2020, 1, 1) };
        var salary2 = new HrEmployeeSalary { Id = 101, EmployeeId = 2, SalaryScaleId = 1, SalaryGradeId = 10, SalaryGrade = grade3, Coefficient = 3.33m, EffectiveFrom = new DateOnly(2025, 9, 1) };

        _salaryRepo.Setup(r => r.GetCurrentSalaryAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(salary1);
        _salaryRepo.Setup(r => r.GetCurrentSalaryAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(salary2);
        _salaryRepo.Setup(r => r.GetGradesForScaleAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HrSalaryGrade> { grade3 });

        var grade4 = new HrSalaryGrade { Id = 11, SalaryScaleId = 1, GradeNumber = 4, Coefficient = 3.66m };
        _eligibilityRule.Setup(e => e.Evaluate(grade3, salary1.EffectiveFrom, It.IsAny<IReadOnlyList<HrSalaryGrade>>(), It.IsAny<DateOnly>(), false))
            .Returns(EligibilityResult.Eligible(grade4));
        _eligibilityRule.Setup(e => e.Evaluate(grade3, salary2.EffectiveFrom, It.IsAny<IReadOnlyList<HrSalaryGrade>>(), It.IsAny<DateOnly>(), false))
            .Returns(EligibilityResult.Ineligible("Employee has held the current grade for 6 month(s)..."));

        List<HrSalaryReviewEmployee>? capturedEntries = null;
        _salaryRepo.Setup(r => r.AddReviewEmployeesAsync(It.IsAny<IEnumerable<HrSalaryReviewEmployee>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<HrSalaryReviewEmployee>, CancellationToken>((entries, _) => capturedEntries = entries.ToList())
            .Returns(Task.CompletedTask);

        var input = new CreateReviewPeriodInput("RP-2026-001", "Annual Review", ReviewType.PERIODIC, new DateOnly(2026, 3, 15), null, null);
        var period = await _sut.CreateReviewPeriodAsync(input);

        period.Status.Should().Be(ReviewPeriodStatus.IN_PROGRESS);
        capturedEntries.Should().HaveCount(2);
        capturedEntries!.Single(e => e.EmployeeId == 1).EligibilityStatus.Should().Be(EligibilityStatus.ELIGIBLE);
        capturedEntries!.Single(e => e.EmployeeId == 1).ProposedGradeId.Should().Be(grade4.Id);
        capturedEntries!.Single(e => e.EmployeeId == 2).EligibilityStatus.Should().Be(EligibilityStatus.INELIGIBLE);
        capturedEntries!.Single(e => e.EmployeeId == 2).ProposedGradeId.Should().BeNull();
        _salaryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateReviewPeriod_EmployeeWithNoCurrentSalary_IsSkipped()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((HrSalaryReviewPeriod?)null);
        var employee = new HrEmployee { Id = 1, EmployeeCode = "EMP-001", FullName = "An" };
        _employeeRepo.Setup(r => r.QueryActive()).Returns(new[] { employee }.AsQueryable());
        _salaryRepo.Setup(r => r.GetCurrentSalaryAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrEmployeeSalary?)null);

        List<HrSalaryReviewEmployee>? capturedEntries = null;
        _salaryRepo.Setup(r => r.AddReviewEmployeesAsync(It.IsAny<IEnumerable<HrSalaryReviewEmployee>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<HrSalaryReviewEmployee>, CancellationToken>((entries, _) => capturedEntries = entries.ToList())
            .Returns(Task.CompletedTask);

        var input = new CreateReviewPeriodInput("RP-2026-001", "Annual Review", ReviewType.PERIODIC, new DateOnly(2026, 3, 15), null, null);
        await _sut.CreateReviewPeriodAsync(input);

        capturedEntries.Should().BeEmpty();
    }

    // ---------- SubmitReviewPeriod (US-05) ----------

    [Fact]
    public async Task SubmitReviewPeriod_PeriodNotFound_ThrowsNotFound()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryReviewPeriod?)null);

        var act = () => _sut.SubmitReviewPeriodAsync(1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SubmitReviewPeriod_NotInProgress_ThrowsConflict()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.SUBMITTED });

        var act = () => _sut.SubmitReviewPeriodAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task SubmitReviewPeriod_HasPendingEmployees_ThrowsConflict()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.IN_PROGRESS });
        _salaryRepo.Setup(r => r.QueryReviewEmployees(1)).Returns(new[]
        {
            new HrSalaryReviewEmployee { ReviewPeriodId = 1, EmployeeId = 1, ReviewStatus = ReviewOutcome.PENDING }
        }.AsQueryable());

        var act = () => _sut.SubmitReviewPeriodAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task SubmitReviewPeriod_NoPendingEmployees_SetsStatusSubmitted()
    {
        var period = new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.IN_PROGRESS };
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(period);
        _salaryRepo.Setup(r => r.QueryReviewEmployees(1)).Returns(new[]
        {
            new HrSalaryReviewEmployee { ReviewPeriodId = 1, EmployeeId = 1, ReviewStatus = ReviewOutcome.APPROVED }
        }.AsQueryable());

        var result = await _sut.SubmitReviewPeriodAsync(1);

        result.Status.Should().Be(ReviewPeriodStatus.SUBMITTED);
    }

    // ---------- CancelReviewPeriod (US-11) ----------

    [Theory]
    [InlineData(ReviewPeriodStatus.CLOSED)]
    [InlineData(ReviewPeriodStatus.CANCELLED)]
    public async Task CancelReviewPeriod_AlreadyClosedOrCancelled_ThrowsConflict(ReviewPeriodStatus status)
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = status });

        var act = () => _sut.CancelReviewPeriodAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CancelReviewPeriod_HasNonCancelledDecision_ThrowsConflict()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.SUBMITTED });
        _salaryRepo.Setup(r => r.HasNonCancelledDecisionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.CancelReviewPeriodAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CancelReviewPeriod_NoDecision_SetsStatusCancelled()
    {
        var period = new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.IN_PROGRESS };
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(period);
        _salaryRepo.Setup(r => r.HasNonCancelledDecisionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.CancelReviewPeriodAsync(1);

        result.Status.Should().Be(ReviewPeriodStatus.CANCELLED);
    }

    // ---------- ApproveEmployee / RejectEmployee (US-04, US-05 guard) ----------

    [Fact]
    public async Task ApproveEmployee_PeriodNotInProgress_ThrowsConflict()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.SUBMITTED });

        var act = () => _sut.ApproveEmployeeAsync(1, 5);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ApproveEmployee_NotEligible_ThrowsConflict()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.IN_PROGRESS });
        _salaryRepo.Setup(r => r.GetReviewEmployeeAsync(1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewEmployee { EligibilityStatus = EligibilityStatus.INELIGIBLE, ReviewStatus = ReviewOutcome.PENDING });

        var act = () => _sut.ApproveEmployeeAsync(1, 5);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ApproveEmployee_AlreadyHasOutcome_ThrowsConflict()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.IN_PROGRESS });
        _salaryRepo.Setup(r => r.GetReviewEmployeeAsync(1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewEmployee { EligibilityStatus = EligibilityStatus.ELIGIBLE, ReviewStatus = ReviewOutcome.APPROVED });

        var act = () => _sut.ApproveEmployeeAsync(1, 5);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ApproveEmployee_EligibleAndPending_SetsApproved()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.IN_PROGRESS });
        var entry = new HrSalaryReviewEmployee { EligibilityStatus = EligibilityStatus.ELIGIBLE, ReviewStatus = ReviewOutcome.PENDING };
        _salaryRepo.Setup(r => r.GetReviewEmployeeAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(entry);

        var result = await _sut.ApproveEmployeeAsync(1, 5);

        result.ReviewStatus.Should().Be(ReviewOutcome.APPROVED);
        _salaryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectEmployee_ReasonMissing_ThrowsValidation()
    {
        var act = () => _sut.RejectEmployeeAsync(1, 5, "   ");

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RejectEmployee_ValidReason_SetsRejectedWithReason()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.IN_PROGRESS });
        var entry = new HrSalaryReviewEmployee { EligibilityStatus = EligibilityStatus.ELIGIBLE, ReviewStatus = ReviewOutcome.PENDING };
        _salaryRepo.Setup(r => r.GetReviewEmployeeAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(entry);

        var result = await _sut.RejectEmployeeAsync(1, 5, "Performance concerns");

        result.ReviewStatus.Should().Be(ReviewOutcome.REJECTED);
        result.Reason.Should().Be("Performance concerns");
    }

    // ---------- BulkApprove / BulkReject (US-04) ----------

    [Fact]
    public async Task BulkApprove_PeriodNotInProgress_ThrowsConflict_ForWholeRequest()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.CLOSED });

        var act = () => _sut.BulkApproveAsync(1, new[] { 5, 6 });

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task BulkApprove_MixOfValidAndInvalidEmployees_ReportsSucceededAndFailedSeparately()
    {
        _salaryRepo.Setup(r => r.GetReviewPeriodAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.IN_PROGRESS });
        _salaryRepo.Setup(r => r.GetReviewEmployeeAsync(1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewEmployee { EmployeeId = 5, EligibilityStatus = EligibilityStatus.ELIGIBLE, ReviewStatus = ReviewOutcome.PENDING });
        _salaryRepo.Setup(r => r.GetReviewEmployeeAsync(1, 6, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewEmployee { EmployeeId = 6, EligibilityStatus = EligibilityStatus.INELIGIBLE, ReviewStatus = ReviewOutcome.PENDING });
        _salaryRepo.Setup(r => r.GetReviewEmployeeAsync(1, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HrSalaryReviewEmployee?)null);

        var result = await _sut.BulkApproveAsync(1, new[] { 5, 6, 7 });

        result.SucceededEmployeeIds.Should().ContainSingle().Which.Should().Be(5);
        result.Failed.Should().HaveCount(2);
        result.Failed.Select(f => f.EmployeeId).Should().BeEquivalentTo(new[] { 6, 7 });
    }

    [Fact]
    public async Task BulkReject_ReasonMissing_ThrowsValidation()
    {
        var act = () => _sut.BulkRejectAsync(1, new[] { 5 }, "");

        await act.Should().ThrowAsync<ValidationException>();
    }
}
