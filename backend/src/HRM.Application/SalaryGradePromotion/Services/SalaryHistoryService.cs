using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Application.SalaryGradePromotion.Models;

namespace HRM.Application.SalaryGradePromotion.Services;

/// <inheritdoc cref="ISalaryHistoryService"/>
public class SalaryHistoryService : ISalaryHistoryService
{
    private readonly IEmployeeSalaryRepository _repository;

    public SalaryHistoryService(IEmployeeSalaryRepository repository)
    {
        _repository = repository;
    }

    // HRM.Application has no EF Core reference — IQueryable<T> from the
    // repository is composed and materialized with plain synchronous LINQ.
    public Task<IReadOnlyList<SalaryHistoryEntryResult>> GetHistoryAsync(
        int employeeId, DateOnly? fromDate, DateOnly? toDate, CancellationToken ct = default)
    {
        var query = _repository.QueryHistory(employeeId);
        if (fromDate is DateOnly from)
        {
            query = query.Where(s => s.EffectiveDate >= from);
        }
        if (toDate is DateOnly to)
        {
            query = query.Where(s => s.EffectiveDate <= to);
        }

        var rows = query.OrderByDescending(s => s.EffectiveDate).ToList(); // US-SGP-08 AC01: newest first
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var current = rows.Where(s => s.EffectiveDate <= today).OrderByDescending(s => s.EffectiveDate).FirstOrDefault();

        var results = rows
            .Select(s => new SalaryHistoryEntryResult(
                s.SalaryGradeId,
                s.SalaryGrade.GradeNumber,
                s.Coefficient,
                s.EffectiveDate,
                s.Reason,
                s.SalaryDecisionId,
                s.SalaryDecision?.DecisionNumber, // US-SGP-08 AC02
                IsCurrent: ReferenceEquals(s, current)))
            .ToList();

        return Task.FromResult<IReadOnlyList<SalaryHistoryEntryResult>>(results);
    }

    public async Task<bool> HasActiveEmployeeOnGradeAsync(int gradeId, CancellationToken ct = default) =>
        await _repository.HasActiveEmployeeOnGradeAsync(gradeId, ct);
}
