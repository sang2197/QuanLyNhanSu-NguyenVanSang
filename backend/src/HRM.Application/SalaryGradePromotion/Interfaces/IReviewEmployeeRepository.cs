using HRM.Domain.Entities;

namespace HRM.Application.SalaryGradePromotion.Interfaces;

public interface IReviewEmployeeRepository
{
    /// <summary>For ListReviewEmployees (US-SGP-03 AC06) — the service
    /// composes .Where() clauses for unit/eligibility/outcome on top of this.</summary>
    IQueryable<HrSalaryReviewEmployee> QueryByPeriod(int periodId);

    Task<HrSalaryReviewEmployee?> GetByPeriodAndEmployeeAsync(int periodId, int employeeId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<HrSalaryReviewEmployee> entries, CancellationToken ct = default);

    /// <summary>Eligible employees with no outcome yet — the SubmitReviewPeriod guard (US-SGP-05 AC02).</summary>
    Task<int> CountUnprocessedEligibleAsync(int periodId, CancellationToken ct = default);

    /// <summary>Rows among <paramref name="employeeIds"/> that are Approved in
    /// this period — CreateDecision's guard (US-SGP-06 AC02).</summary>
    Task<IReadOnlyList<HrSalaryReviewEmployee>> GetApprovedAsync(int periodId, IReadOnlyList<int> employeeIds, CancellationToken ct = default);
}
