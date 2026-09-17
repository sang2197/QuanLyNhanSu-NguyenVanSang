using HRM.Domain.Entities;

namespace HRM.Application.SalaryManagement.Rules;

/// <inheritdoc cref="IEligibilityRule"/>
public class EligibilityRule : IEligibilityRule
{
    public const int MinMonthsInGrade = 24;

    public EligibilityResult Evaluate(
        HrSalaryGrade currentGrade,
        DateOnly currentGradeEffectiveFrom,
        IReadOnlyList<HrSalaryGrade> gradesInScale,
        DateOnly reviewDate,
        bool alreadyHasProposalThisPeriod)
    {
        var monthsInGrade = MonthsBetween(currentGradeEffectiveFrom, reviewDate);
        if (monthsInGrade < MinMonthsInGrade)
        {
            return EligibilityResult.Ineligible(
                $"Employee has held the current grade for {monthsInGrade} month(s) as of the review date; " +
                $"minimum required is {MinMonthsInGrade} months.");
        }

        // US-SAL-06: an inactive grade is never proposed — skip forward to the
        // nearest active grade above the current one, regardless of any gap
        // in grade numbers (not just an exact +1 match).
        var nextGrade = gradesInScale
            .Where(g => g.GradeNumber > currentGrade.GradeNumber && g.IsActive)
            .OrderBy(g => g.GradeNumber)
            .FirstOrDefault();
        if (nextGrade is null)
        {
            return EligibilityResult.Ineligible("Employee is already at the highest active grade of their salary scale.");
        }

        if (alreadyHasProposalThisPeriod)
        {
            return EligibilityResult.Ineligible("Employee already has a proposal in this review period.");
        }

        return EligibilityResult.Eligible(nextGrade);
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
