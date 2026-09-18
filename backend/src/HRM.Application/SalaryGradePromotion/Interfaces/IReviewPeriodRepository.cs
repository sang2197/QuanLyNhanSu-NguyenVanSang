using HRM.Domain.Entities;

namespace HRM.Application.SalaryGradePromotion.Interfaces;

public interface IReviewPeriodRepository
{
    Task<HrSalaryReviewPeriod?> GetByIdAsync(int periodId, CancellationToken ct = default);
    Task<HrSalaryReviewPeriod?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<HrSalaryReviewPeriod?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>For SearchReviewPeriods (US-SGP-02) — the service composes
    /// .Where() clauses for date range/type/status on top of this.</summary>
    IQueryable<HrSalaryReviewPeriod> Query();

    /// <summary>SUBMITTED periods with no non-cancelled decision (US-SGP-09 AC04).</summary>
    IQueryable<HrSalaryReviewPeriod> QueryEligibleForDecision();

    Task AddAsync(HrSalaryReviewPeriod period, CancellationToken ct = default);
}
