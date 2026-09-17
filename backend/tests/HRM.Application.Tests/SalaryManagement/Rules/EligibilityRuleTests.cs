using FluentAssertions;
using HRM.Application.SalaryManagement.Rules;
using HRM.Domain.Entities;
using Xunit;

namespace HRM.Application.Tests.SalaryManagement.Rules;

/// <summary>
/// RISK-08 (Critical): the eligibility rule is defined at the requirements
/// level (US-03) but not enforced anywhere else — these tests are the main
/// safety net, so every boundary of the 3 conditions is covered explicitly.
/// </summary>
public class EligibilityRuleTests
{
    private readonly EligibilityRule _rule = new();

    private static HrSalaryGrade Grade(int id, int scaleId, int gradeNumber, decimal coefficient, string? status = null) => new()
    {
        Id = id,
        SalaryScaleId = scaleId,
        GradeNumber = gradeNumber,
        Coefficient = coefficient,
        EffectiveFrom = new DateOnly(2020, 1, 1),
        Status = status
    };

    [Fact]
    public void Evaluate_ExactlyTwentyFourMonths_NextGradeExists_NoDuplicate_IsEligible()
    {
        var currentGrade = Grade(1, scaleId: 1, gradeNumber: 3, coefficient: 3.33m);
        var nextGrade = Grade(2, scaleId: 1, gradeNumber: 4, coefficient: 3.66m);
        var effectiveFrom = new DateOnly(2024, 3, 15);
        var reviewDate = new DateOnly(2026, 3, 15); // exactly 24 months later

        var result = _rule.Evaluate(currentGrade, effectiveFrom, new[] { currentGrade, nextGrade }, reviewDate, alreadyHasProposalThisPeriod: false);

        result.IsEligible.Should().BeTrue();
        result.ProposedGrade.Should().Be(nextGrade);
        result.Reason.Should().BeNull();
    }

    [Fact]
    public void Evaluate_OneDayShortOfTwentyFourMonths_IsIneligible()
    {
        var currentGrade = Grade(1, scaleId: 1, gradeNumber: 3, coefficient: 3.33m);
        var nextGrade = Grade(2, scaleId: 1, gradeNumber: 4, coefficient: 3.66m);
        var effectiveFrom = new DateOnly(2024, 3, 15);
        var reviewDate = new DateOnly(2026, 3, 14); // 1 day short of 24 months

        var result = _rule.Evaluate(currentGrade, effectiveFrom, new[] { currentGrade, nextGrade }, reviewDate, alreadyHasProposalThisPeriod: false);

        result.IsEligible.Should().BeFalse();
        result.ProposedGrade.Should().BeNull();
        result.Reason.Should().Contain("24");
    }

    [Fact]
    public void Evaluate_MonthsInGradeUsesFloorAcrossShorterMonth_IsIneligible()
    {
        // Jan 31 -> the 24th month later is Jan 31 2026; Jan 30 2026 is 1 day short.
        var currentGrade = Grade(1, scaleId: 1, gradeNumber: 1, coefficient: 1.0m);
        var nextGrade = Grade(2, scaleId: 1, gradeNumber: 2, coefficient: 1.2m);
        var effectiveFrom = new DateOnly(2024, 1, 31);
        var reviewDate = new DateOnly(2026, 1, 30);

        var result = _rule.Evaluate(currentGrade, effectiveFrom, new[] { currentGrade, nextGrade }, reviewDate, alreadyHasProposalThisPeriod: false);

        result.IsEligible.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_MoreThanTwentyFourMonths_NextGradeExists_IsEligible()
    {
        var currentGrade = Grade(1, scaleId: 1, gradeNumber: 3, coefficient: 3.33m);
        var nextGrade = Grade(2, scaleId: 1, gradeNumber: 4, coefficient: 3.66m);
        var effectiveFrom = new DateOnly(2020, 1, 1);
        var reviewDate = new DateOnly(2026, 3, 15);

        var result = _rule.Evaluate(currentGrade, effectiveFrom, new[] { currentGrade, nextGrade }, reviewDate, alreadyHasProposalThisPeriod: false);

        result.IsEligible.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_AlreadyAtHighestGradeInScale_IsIneligible_EvenWithEnoughTenure()
    {
        var currentGrade = Grade(1, scaleId: 1, gradeNumber: 5, coefficient: 5.0m);
        var effectiveFrom = new DateOnly(2020, 1, 1);
        var reviewDate = new DateOnly(2026, 3, 15);

        // gradesInScale only contains the current grade — no grade 6 exists.
        var result = _rule.Evaluate(currentGrade, effectiveFrom, new[] { currentGrade }, reviewDate, alreadyHasProposalThisPeriod: false);

        result.IsEligible.Should().BeFalse();
        result.ProposedGrade.Should().BeNull();
        result.Reason.Should().Contain("highest active grade");
    }

    [Fact]
    public void Evaluate_AlreadyHasProposalThisPeriod_IsIneligible_EvenWhenOtherwiseEligible()
    {
        var currentGrade = Grade(1, scaleId: 1, gradeNumber: 3, coefficient: 3.33m);
        var nextGrade = Grade(2, scaleId: 1, gradeNumber: 4, coefficient: 3.66m);
        var effectiveFrom = new DateOnly(2020, 1, 1);
        var reviewDate = new DateOnly(2026, 3, 15);

        var result = _rule.Evaluate(currentGrade, effectiveFrom, new[] { currentGrade, nextGrade }, reviewDate, alreadyHasProposalThisPeriod: true);

        result.IsEligible.Should().BeFalse();
        result.Reason.Should().Contain("already has a proposal");
    }

    // US-SAL-06 AC03/AC04 — "next grade" is the nearest ACTIVE grade above the
    // current one, in ascending order; a gap in grade numbers (whether from a
    // number that was simply never used, or from a deactivated grade) does
    // not stop the lookup — it just keeps looking further up.

    [Fact]
    public void Evaluate_GapInGradeNumbers_NextHigherActiveGradeIsStillUsed()
    {
        var currentGrade = Grade(1, scaleId: 1, gradeNumber: 3, coefficient: 3.33m);
        // Grade 5 exists but grade 4 does not — 5 is still the correct "next".
        var gradeFive = Grade(3, scaleId: 1, gradeNumber: 5, coefficient: 4.0m);
        var reviewDate = new DateOnly(2026, 3, 15);
        var effectiveFrom = new DateOnly(2020, 1, 1);

        var result = _rule.Evaluate(currentGrade, effectiveFrom, new[] { currentGrade, gradeFive }, reviewDate, alreadyHasProposalThisPeriod: false);

        result.IsEligible.Should().BeTrue();
        result.ProposedGrade.Should().Be(gradeFive);
    }

    [Fact]
    public void Evaluate_ImmediateNextGradeIsInactive_SkipsToNextActiveGrade()
    {
        var currentGrade = Grade(1, scaleId: 1, gradeNumber: 3, coefficient: 3.33m);
        var inactiveGradeFour = Grade(2, scaleId: 1, gradeNumber: 4, coefficient: 3.66m, status: "INACTIVE");
        var activeGradeFive = Grade(3, scaleId: 1, gradeNumber: 5, coefficient: 4.0m);
        var reviewDate = new DateOnly(2026, 3, 15);
        var effectiveFrom = new DateOnly(2020, 1, 1);

        var result = _rule.Evaluate(currentGrade, effectiveFrom, new[] { currentGrade, inactiveGradeFour, activeGradeFive }, reviewDate, alreadyHasProposalThisPeriod: false);

        result.IsEligible.Should().BeTrue();
        result.ProposedGrade.Should().Be(activeGradeFive);
    }

    [Fact]
    public void Evaluate_MultipleConsecutiveInactiveGrades_SkipsAllToNextActiveGrade()
    {
        var currentGrade = Grade(1, scaleId: 1, gradeNumber: 1, coefficient: 1.0m);
        var inactiveGradeTwo = Grade(2, scaleId: 1, gradeNumber: 2, coefficient: 1.2m, status: "INACTIVE");
        var inactiveGradeThree = Grade(3, scaleId: 1, gradeNumber: 3, coefficient: 1.4m, status: "INACTIVE");
        var activeGradeFour = Grade(4, scaleId: 1, gradeNumber: 4, coefficient: 1.6m);
        var reviewDate = new DateOnly(2026, 3, 15);
        var effectiveFrom = new DateOnly(2020, 1, 1);

        var result = _rule.Evaluate(
            currentGrade, effectiveFrom,
            new[] { currentGrade, inactiveGradeTwo, inactiveGradeThree, activeGradeFour },
            reviewDate, alreadyHasProposalThisPeriod: false);

        result.IsEligible.Should().BeTrue();
        result.ProposedGrade.Should().Be(activeGradeFour);
    }

    [Fact]
    public void Evaluate_AllHigherGradesInactive_IsIneligible()
    {
        var currentGrade = Grade(1, scaleId: 1, gradeNumber: 3, coefficient: 3.33m);
        var inactiveGradeFour = Grade(2, scaleId: 1, gradeNumber: 4, coefficient: 3.66m, status: "INACTIVE");
        var reviewDate = new DateOnly(2026, 3, 15);
        var effectiveFrom = new DateOnly(2020, 1, 1);

        var result = _rule.Evaluate(currentGrade, effectiveFrom, new[] { currentGrade, inactiveGradeFour }, reviewDate, alreadyHasProposalThisPeriod: false);

        result.IsEligible.Should().BeFalse();
        result.ProposedGrade.Should().BeNull();
        result.Reason.Should().Contain("highest active grade");
    }
}
