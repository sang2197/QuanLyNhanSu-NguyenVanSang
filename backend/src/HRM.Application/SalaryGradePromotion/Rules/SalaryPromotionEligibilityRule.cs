using HRM.Domain.Entities;

namespace HRM.Application.SalaryGradePromotion.Rules;

/// <inheritdoc cref="ISalaryPromotionEligibilityRule"/>
public class SalaryPromotionEligibilityRule : ISalaryPromotionEligibilityRule
{
    public const int MinMonthsInGrade = 24;

    public EligibilityResult DetermineEligibility(DateOnly currentGradeEffectiveDate, DateOnly reviewDate, HrSalaryGrade? nextActiveGrade)
    {
        var monthsInGrade = MonthsBetween(currentGradeEffectiveDate, reviewDate);
        if (monthsInGrade < MinMonthsInGrade)
        {
            return EligibilityResult.Ineligible(
                $"Employee has held the current grade for {monthsInGrade} month(s) as of the review date, minimum required is {MinMonthsInGrade} months.");
        }

        if (nextActiveGrade is null)
        {
            return EligibilityResult.Ineligible("Employee is already at the highest active grade of their salary scale.");
        }

        return EligibilityResult.Eligible(nextActiveGrade);
    }

    /// <summary>Whole calendar months between two dates, floored (23 months
    /// 29 days is 23, not 24) — deliberately conservative for the 24-month
    /// eligibility boundary (RISK-08).</summary>
    private static int MonthsBetween(DateOnly from, DateOnly to)
    {
        var months = (to.Year - from.Year) * 12 + (to.Month - from.Month);
        if (to.Day < from.Day)
        {
            months--;
        }
        return months;
    }
}
