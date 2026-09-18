using HRM.Application.Common;
using HRM.Application.EmployeeManagement.Models;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.EmployeeManagement.Interfaces;

public interface IEmployeeService
{
    Task<HrEmployee> CreateEmployeeAsync(CreateEmployeeInput input, CancellationToken ct = default);

    Task<PagedResult<HrEmployee>> SearchEmployeesAsync(
        string? search, int? organizationalUnitId, int? jobTitleId, EmploymentStatus? employmentStatus,
        int page, int pageSize, CancellationToken ct = default);

    Task<HrEmployee> GetEmployeeAsync(int employeeId, CancellationToken ct = default);
    Task<HrEmployee> UpdateEmployeeAsync(int employeeId, UpdateEmployeeInput input, CancellationToken ct = default);
    Task<HrEmployee> ChangeEmploymentStatusAsync(int employeeId, EmploymentStatus newStatus, CancellationToken ct = default);

    // ---- Exposed cross-domain (ADR-03) ----

    /// <summary>For Organization Management's deactivate-unit guard (BR-ORG-11).</summary>
    Task<bool> HasActiveEmployeesInUnitAsync(int organizationalUnitId, CancellationToken ct = default);

    /// <summary>For Salary Grade Promotion's review-period snapshot (US-SGP-01).</summary>
    Task<IReadOnlyList<HrEmployee>> GetActiveEmployeesAsync(CancellationToken ct = default);
}
