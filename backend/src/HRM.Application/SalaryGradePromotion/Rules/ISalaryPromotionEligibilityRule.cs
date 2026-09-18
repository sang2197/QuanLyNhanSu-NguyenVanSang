using HRM.Domain.Entities;

namespace HRM.Application.SalaryGradePromotion.Rules;

public interface ISalaryPromotionEligibilityRule
{
    /// <summary>
    /// US-SGP-03 eligibility rule (RISK-08 — Critical, must be enforced
    /// carefully): an employee is eligible only when BOTH:
    ///   1. They have held their current grade for at least 24 months as of
    ///      <paramref name="reviewDate"/> (AC02).
    ///   2. A higher active grade exists in their salary scale — resolved by
    ///      the caller via ISalaryGradeService.GetNextActiveGradeAsync
    ///      (Salary Master Data's responsibility, not this rule's) and
    ///      passed in as <paramref name="nextActiveGrade"/> (AC03/AC04).
    /// This rule only combines the two; it does not search for the next
    /// grade itself, to avoid duplicating grade-ordering knowledge that
    /// belongs to Salary Master Data.
    /// </summary>
    EligibilityResult DetermineEligibility(
        DateOnly currentGradeEffectiveDate,
        DateOnly reviewDate,
        HrSalaryGrade? nextActiveGrade);
}
