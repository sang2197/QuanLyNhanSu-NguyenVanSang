using HRM.Application.SalaryManagement.Interfaces;
using HRM.Application.SalaryManagement.Models;

namespace HRM.Application.SalaryManagement.Services;

/// <inheritdoc cref="ISalaryHistoryService"/>
public class SalaryHistoryService : ISalaryHistoryService
{
    private readonly ISalaryRepository _salaryRepository;

    public SalaryHistoryService(ISalaryRepository salaryRepository)
    {
        _salaryRepository = salaryRepository;
    }

    // US-08 — read-only, newest first.
    public Task<IReadOnlyList<SalaryHistoryEntryResult>> GetHistoryAsync(
        int employeeId, DateOnly? fromDate, DateOnly? toDate, CancellationToken ct = default)
    {
        var query = _salaryRepository.QuerySalaryHistory(employeeId);

        if (fromDate is not null) query = query.Where(s => s.EffectiveFrom >= fromDate);
        if (toDate is not null) query = query.Where(s => s.EffectiveFrom <= toDate);

        var items = query
            .OrderByDescending(s => s.EffectiveFrom)
            .ToList()
            .Select(s => new SalaryHistoryEntryResult(
                Grade: s.SalaryGrade.GradeNumber.ToString(),
                Coefficient: s.Coefficient,
                EffectiveFrom: s.EffectiveFrom,
                EffectiveTo: s.EffectiveTo,
                Reason: s.Reason,
                DecisionId: s.DecisionId,
                DecisionNumber: s.Decision?.DecisionNumber,
                IsCurrent: s.EffectiveTo is null))
            .ToList();

        return Task.FromResult<IReadOnlyList<SalaryHistoryEntryResult>>(items);
    }
}
