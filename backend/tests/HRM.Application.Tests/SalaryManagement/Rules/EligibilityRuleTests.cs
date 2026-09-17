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

    private static HrSalaryGrade Grade(int id, int scaleId, int gradeNumber, decimal coefficient) => new()
    {
        Id = id,
        SalaryScaleId = scaleId,
        GradeNumber = gradeNumber,
        Coefficient = coefficient,
        EffectiveFrom = new DateOnly(2020, 1, 1)
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
        result.Reason.Should().Contain("highest grade");
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

    [Fact]
    public void Evaluate_NextGradeMustBeExactlyOneNumberHigher_SkipsNonAdjacentGrades()
    {
        var currentGrade = Grade(1, scaleId: 1, gradeNumber: 3, coefficient: 3.33m);
        // Grade 5 exists but grade 4 does not — should not treat 5 as "next".
        var gradeFive = Grade(3, scaleId: 1, gradeNumber: 5, coefficient: 4.0m);
        var reviewDate = new DateOnly(2026, 3, 15);
        var effectiveFrom = new DateOnly(2020, 1, 1);

        var result = _rule.Evaluate(currentGrade, effectiveFrom, new[] { currentGrade, gradeFive }, reviewDate, alreadyHasProposalThisPeriod: false);

        result.IsEligible.Should().BeFalse();
        result.Reason.Should().Contain("highest grade");
    }
}
