using HRM.Application.Common;
using HRM.Application.SalaryGradePromotion.Models;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.SalaryGradePromotion.Interfaces;

public interface IReviewPeriodService
{
    /// <summary>Unique code/name (US-SGP-01 AC02/AC03); snapshots eligibility
    /// and proposed grade for every active employee in the same operation.</summary>
    Task<HrSalaryReviewPeriod> CreateReviewPeriodAsync(CreateReviewPeriodInput input, CancellationToken ct = default);

    Task<PagedResult<HrSalaryReviewPeriod>> SearchReviewPeriodsAsync(
        DateOnly? fromDate, DateOnly? toDate, ReviewType? reviewType, ReviewPeriodStatus? status,
        int page, int pageSize, CancellationToken ct = default);

    Task<ReviewPeriodDetailResult> GetReviewPeriodDetailAsync(int periodId, CancellationToken ct = default);

    /// <summary>Blocked while an eligible employee has no outcome (US-SGP-05).</summary>
    Task<HrSalaryReviewPeriod> SubmitReviewPeriodAsync(int periodId, CancellationToken ct = default);

    /// <summary>Blocked if CLOSED/CANCELLED, or SUBMITTED with a non-cancelled decision (US-SGP-11).</summary>
    Task<HrSalaryReviewPeriod> CancelReviewPeriodAsync(int periodId, CancellationToken ct = default);
}
