using HRM.Application.Common;
using HRM.Application.SalaryGradePromotion.Models;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.SalaryGradePromotion.Interfaces;

public interface ISalaryDecisionService
{
    /// <summary>Period must be SUBMITTED with no non-cancelled decision
    /// (US-SGP-06 AC01/AC03), only Approved employees (AC02), effective date
    /// on or after the review date (AC06/AC07).</summary>
    Task<HrSalaryDecision> CreateDecisionAsync(CreateSalaryDecisionInput input, CancellationToken ct = default);

    Task<PagedResult<HrSalaryDecision>> SearchDecisionsAsync(
        SalaryDecisionStatus? status, int? reviewPeriodId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>SUBMITTED periods with no non-cancelled decision (US-SGP-09 AC04).</summary>
    Task<IReadOnlyList<HrSalaryReviewPeriod>> GetEligibleReviewPeriodsAsync(CancellationToken ct = default);

    Task<HrSalaryDecision> GetDecisionAsync(int decisionId, CancellationToken ct = default);

    /// <summary>Effective date only, Draft only.</summary>
    Task<HrSalaryDecision> SaveDraftAsync(int decisionId, DateOnly effectiveDate, CancellationToken ct = default);

    /// <summary>Draft only; employees cannot be added back (US-SGP-06 AC04/AC05).</summary>
    Task RemoveEmployeeAsync(int decisionId, int employeeId, CancellationToken ct = default);

    /// <summary>All-or-nothing; revalidates each BaselineSalaryGradeId against
    /// the employee's current grade (US-SGP-07 AC02/AC04).</summary>
    Task<HrSalaryDecision> ApplyDecisionAsync(int decisionId, CancellationToken ct = default);

    /// <summary>Draft only (US-SGP-10).</summary>
    Task<HrSalaryDecision> CancelDecisionAsync(int decisionId, CancellationToken ct = default);
}
