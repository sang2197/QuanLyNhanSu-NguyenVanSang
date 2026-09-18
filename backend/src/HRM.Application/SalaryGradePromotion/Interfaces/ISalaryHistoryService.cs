using HRM.Application.SalaryGradePromotion.Models;

namespace HRM.Application.SalaryGradePromotion.Interfaces;

public interface ISalaryHistoryService
{
    /// <summary>Newest first, links back to the causing decision (US-SGP-08).</summary>
    Task<IReadOnlyList<SalaryHistoryEntryResult>> GetHistoryAsync(
        int employeeId, DateOnly? fromDate, DateOnly? toDate, CancellationToken ct = default);

    /// <summary>Exposed cross-domain for Salary Master Data's deactivate-grade guard (BR-SAL-15, ADR-03).</summary>
    Task<bool> HasActiveEmployeeOnGradeAsync(int gradeId, CancellationToken ct = default);
}
