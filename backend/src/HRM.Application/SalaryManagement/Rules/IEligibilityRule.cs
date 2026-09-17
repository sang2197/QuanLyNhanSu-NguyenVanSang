using HRM.Domain.Entities;

namespace HRM.Application.SalaryManagement.Rules;

public interface IEligibilityRule
{
    /// <summary>
    /// US-03 eligibility rule (RISK-08 — Critical, must be enforced carefully):
    /// an employee is eligible only when ALL of:
    ///   1. They have held <paramref name="currentGrade"/> for at least 24
    ///      months as of <paramref name="reviewDate"/>.
    ///   2. A next grade exists above <paramref name="currentGrade"/> within
    ///      <paramref name="gradesInScale"/> (their own salary scale).
    ///   3. <paramref name="alreadyHasProposalThisPeriod"/> is false.
    /// </summary>
    EligibilityResult Evaluate(
        HrSalaryGrade currentGrade,
        DateOnly currentGradeEffectiveFrom,
        IReadOnlyList<HrSalaryGrade> gradesInScale,
        DateOnly reviewDate,
        bool alreadyHasProposalThisPeriod);
}
