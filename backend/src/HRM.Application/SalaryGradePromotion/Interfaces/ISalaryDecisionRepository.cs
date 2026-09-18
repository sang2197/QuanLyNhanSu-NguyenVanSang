using HRM.Domain.Entities;

namespace HRM.Application.SalaryGradePromotion.Interfaces;

public interface ISalaryDecisionRepository
{
    /// <summary>Includes Details (+ their Employee/BaselineSalaryGrade/NewSalaryGrade) and ReviewPeriod.</summary>
    Task<HrSalaryDecision?> GetByIdAsync(int decisionId, CancellationToken ct = default);

    Task<HrSalaryDecision?> GetNonCancelledByPeriodAsync(int periodId, CancellationToken ct = default);

    /// <summary>For SearchDecisions — the service composes .Where() clauses
    /// for status/reviewPeriodId on top of this.</summary>
    IQueryable<HrSalaryDecision> Query();

    Task AddAsync(HrSalaryDecision decision, CancellationToken ct = default);
    Task RemoveDetailAsync(HrSalaryDecisionDetail detail, CancellationToken ct = default);

    /// <summary>Next system-generated DecisionNumber (openapi.yaml: readOnly, e.g. "SD-2026-001").</summary>
    Task<string> NextDecisionNumberAsync(CancellationToken ct = default);
}
