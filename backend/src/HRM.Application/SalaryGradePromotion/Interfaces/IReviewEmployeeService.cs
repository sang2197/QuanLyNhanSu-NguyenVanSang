using HRM.Application.Common;
using HRM.Application.SalaryGradePromotion.Models;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.SalaryGradePromotion.Interfaces;

public interface IReviewEmployeeService
{
    Task<PagedResult<HrSalaryReviewEmployee>> ListReviewEmployeesAsync(
        int periodId, int? organizationalUnitId, bool? eligible, ReviewOutcome? outcome,
        int page, int pageSize, CancellationToken ct = default);

    Task<HrSalaryReviewEmployee> GetReviewEmployeeAsync(int periodId, int employeeId, CancellationToken ct = default);

    /// <summary>Period must be IN_PROGRESS, employee eligible and pending
    /// (US-SGP-04 AC01/AC10); clears any prior rejection reason (AC08).</summary>
    Task<HrSalaryReviewEmployee> ApproveEmployeeAsync(int periodId, int employeeId, CancellationToken ct = default);

    /// <summary>Reason required (US-SGP-04 AC02/AC03).</summary>
    Task<HrSalaryReviewEmployee> RejectEmployeeAsync(int periodId, int employeeId, string reason, CancellationToken ct = default);

    /// <summary>Each proposal validated individually, partial success (US-SGP-04 AC04/AC05).</summary>
    Task<BulkActionResult> BulkApproveAsync(int periodId, IReadOnlyList<int> employeeIds, CancellationToken ct = default);

    /// <summary>Reason required for the whole batch, partial success retains it for successes (US-SGP-04 AC06/AC07/AC09).</summary>
    Task<BulkActionResult> BulkRejectAsync(int periodId, IReadOnlyList<int> employeeIds, string reason, CancellationToken ct = default);
}
