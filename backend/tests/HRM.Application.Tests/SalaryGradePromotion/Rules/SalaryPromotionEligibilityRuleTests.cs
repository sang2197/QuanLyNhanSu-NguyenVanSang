using FluentAssertions;
using HRM.Application.SalaryGradePromotion.Rules;
using HRM.Domain.Entities;
using Xunit;

namespace HRM.Application.Tests.SalaryGradePromotion.Rules;

/// <summary>US-SGP-03 eligibility rule (RISK-08 — Critical, must be enforced carefully).</summary>
public class SalaryPromotionEligibilityRuleTests
{
    private readonly SalaryPromotionEligibilityRule _rule = new();

    private static HrSalaryGrade Grade(int id, int gradeNumber) => new() { Id = id, GradeNumber = gradeNumber };

    [Fact]
    public void DetermineEligibility_ExactlyTwentyFourMonths_IsEligible()
    {
        var currentGradeEffectiveDate = new DateOnly(2024, 3, 15);
        var reviewDate = new DateOnly(2026, 3, 15);
        var nextGrade = Grade(2, 4);

        var result = _rule.DetermineEligibility(currentGradeEffectiveDate, reviewDate, nextGrade);

        result.IsEligible.Should().BeTrue();
        result.ProposedGrade.Should().Be(nextGrade);
    }

    [Fact]
    public void DetermineEligibility_OneDayShortOfTwentyFourMonths_IsIneligible()
    {
        var currentGradeEffectiveDate = new DateOnly(2024, 3, 16);
        var reviewDate = new DateOnly(2026, 3, 15);

        var result = _rule.DetermineEligibility(currentGradeEffectiveDate, reviewDate, Grade(2, 4));

        result.IsEligible.Should().BeFalse();
        result.Reason.Should().Contain("23");
    }

    [Fact]
    public void DetermineEligibility_MoreThanTwentyFourMonths_IsEligible()
    {
        var currentGradeEffectiveDate = new DateOnly(2020, 1, 1);
        var reviewDate = new DateOnly(2026, 3, 15);

        var result = _rule.DetermineEligibility(currentGradeEffectiveDate, reviewDate, Grade(2, 4));

        result.IsEligible.Should().BeTrue();
    }

    [Fact]
    public void DetermineEligibility_ZeroMonths_IsIneligible()
    {
        var reviewDate = new DateOnly(2026, 3, 15);

        var result = _rule.DetermineEligibility(reviewDate, reviewDate, Grade(2, 4));

        result.IsEligible.Should().BeFalse();
    }

    [Fact]
    public void DetermineEligibility_TenureMetButNoNextActiveGrade_IsIneligible()
    {
        var currentGradeEffectiveDate = new DateOnly(2020, 1, 1);
        var reviewDate = new DateOnly(2026, 3, 15);

        var result = _rule.DetermineEligibility(currentGradeEffectiveDate, reviewDate, nextActiveGrade: null);

        result.IsEligible.Should().BeFalse();
        result.Reason.Should().Contain("highest active grade");
    }

    [Theory]
    [InlineData(23)]
    [InlineData(12)]
    [InlineData(0)]
    public void DetermineEligibility_TenureUnderThreshold_IsIneligibleRegardlessOfNextGrade(int monthsHeld)
    {
        var reviewDate = new DateOnly(2026, 3, 15);
        var currentGradeEffectiveDate = reviewDate.AddMonths(-monthsHeld);

        var result = _rule.DetermineEligibility(currentGradeEffectiveDate, reviewDate, Grade(2, 4));

        result.IsEligible.Should().BeFalse();
    }
}
