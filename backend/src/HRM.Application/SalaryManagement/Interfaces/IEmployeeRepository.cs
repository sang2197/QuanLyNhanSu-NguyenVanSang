using HRM.Domain.Entities;

namespace HRM.Application.SalaryManagement.Interfaces;

public interface IEmployeeRepository
{
    Task<HrEmployee?> GetByIdAsync(int employeeId, CancellationToken ct = default);

    /// <summary>All active employees — used by CreateReviewPeriod (US-01) to
    /// calculate a proposed grade for every one of them synchronously.</summary>
    IQueryable<HrEmployee> QueryActive();
}
