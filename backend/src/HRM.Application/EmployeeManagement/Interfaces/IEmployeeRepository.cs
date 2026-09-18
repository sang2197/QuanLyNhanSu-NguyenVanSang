using HRM.Domain.Entities;

namespace HRM.Application.EmployeeManagement.Interfaces;

public interface IEmployeeRepository
{
    Task<HrEmployee?> GetByIdAsync(int employeeId, CancellationToken ct = default);
    Task<HrEmployee?> GetByCodeAsync(string employeeCode, CancellationToken ct = default);

    /// <summary>For SearchEmployees (BR-EMP-06/07) — the service composes
    /// .Where() clauses for code/name/unit/title/status on top of this.</summary>
    IQueryable<HrEmployee> Query();

    /// <summary>All active employees — used by CreateReviewPeriod (US-SGP-01,
    /// cross-domain via IEmployeeService.GetActiveEmployees) to calculate a
    /// proposed grade for every one of them.</summary>
    IQueryable<HrEmployee> QueryActive();

    Task AddAsync(HrEmployee employee, CancellationToken ct = default);

    /// <summary>Backs the Organization Management deactivate-unit guard
    /// (BR-ORG-11, cross-domain via IEmployeeService.HasActiveEmployeesInUnit).</summary>
    Task<bool> HasActiveInUnitAsync(int organizationalUnitId, CancellationToken ct = default);
}
