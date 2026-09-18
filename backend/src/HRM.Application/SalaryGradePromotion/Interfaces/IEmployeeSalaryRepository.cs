using HRM.Domain.Entities;

namespace HRM.Application.SalaryGradePromotion.Interfaces;

public interface IEmployeeSalaryRepository
{
    /// <summary>The row with the latest EffectiveDate on or before today —
    /// HrEmployeeSalary is append-only (no EffectiveTo column), so "current"
    /// is a query-time concept, not a stored range.</summary>
    Task<HrEmployeeSalary?> GetCurrentAsync(int employeeId, CancellationToken ct = default);

    /// <summary>For GetHistory (US-SGP-08) — the service composes .Where()
    /// clauses for the optional date range on top of this.</summary>
    IQueryable<HrEmployeeSalary> QueryHistory(int employeeId);

    Task AddAsync(HrEmployeeSalary salary, CancellationToken ct = default);

    /// <summary>Backs the Salary Master Data deactivate-grade guard (BR-SAL-15,
    /// cross-domain via ISalaryHistoryService.HasActiveEmployeeOnGrade) — true
    /// when an Active employee's current salary row is on this grade.</summary>
    Task<bool> HasActiveEmployeeOnGradeAsync(int gradeId, CancellationToken ct = default);
}
