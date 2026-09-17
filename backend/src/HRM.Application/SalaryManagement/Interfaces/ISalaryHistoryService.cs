using HRM.Application.SalaryManagement.Models;

namespace HRM.Application.SalaryManagement.Interfaces;

public interface ISalaryHistoryService
{
    Task<IReadOnlyList<SalaryHistoryEntryResult>> GetHistoryAsync(
        int employeeId, DateOnly? fromDate, DateOnly? toDate, CancellationToken ct = default);
}
