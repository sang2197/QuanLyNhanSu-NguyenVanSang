using HRM.Application.Common;
using HRM.Application.SalaryManagement.Models;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.SalaryManagement.Interfaces;

public interface ISalaryReviewService
{
    Task<HrSalaryReviewPeriod> CreateReviewPeriodAsync(CreateReviewPeriodInput input, CancellationToken ct = default);
    Task<PagedResult<HrSalaryReviewPeriod>> SearchReviewPeriodsAsync(
        DateOnly? fromDate, DateOnly? toDate, ReviewType? reviewType, ReviewPeriodStatus? status,
        int page, int pageSize, CancellationToken ct = default);
    Task<ReviewPeriodDetailResult> GetReviewPeriodDetailAsync(int periodId, CancellationToken ct = default);
    Task<HrSalaryReviewPeriod> SubmitReviewPeriodAsync(int periodId, CancellationToken ct = default);
    Task<HrSalaryReviewPeriod> CancelReviewPeriodAsync(int periodId, CancellationToken ct = default);

    Task<PagedResult<HrSalaryReviewEmployee>> SearchReviewEmployeesAsync(
        int periodId, string? department, EligibilityStatus? eligibility, ReviewOutcome? outcome,
        int page, int pageSize, CancellationToken ct = default);
    Task<HrSalaryReviewEmployee> GetReviewEmployeeAsync(int periodId, int employeeId, CancellationToken ct = default);
    Task<HrSalaryReviewEmployee> ApproveEmployeeAsync(int periodId, int employeeId, CancellationToken ct = default);
    Task<HrSalaryReviewEmployee> RejectEmployeeAsync(int periodId, int employeeId, string reason, CancellationToken ct = default);
    Task<BulkActionResult> BulkApproveAsync(int periodId, IReadOnlyList<int> employeeIds, CancellationToken ct = default);
    Task<BulkActionResult> BulkRejectAsync(int periodId, IReadOnlyList<int> employeeIds, string reason, CancellationToken ct = default);
}
