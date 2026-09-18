using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using HRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRM.Infrastructure.Repositories.SalaryGradePromotion;

/// <inheritdoc cref="ISalaryDecisionRepository"/>
public class SalaryDecisionRepository : ISalaryDecisionRepository
{
    private readonly HrmDbContext _context;

    public SalaryDecisionRepository(HrmDbContext context)
    {
        _context = context;
    }

    public Task<HrSalaryDecision?> GetByIdAsync(int decisionId, CancellationToken ct = default) =>
        _context.Decisions
            .Include(d => d.Details).ThenInclude(det => det.Employee)
            .Include(d => d.Details).ThenInclude(det => det.BaselineSalaryGrade)
            .Include(d => d.Details).ThenInclude(det => det.NewSalaryGrade)
            .Include(d => d.ReviewPeriod)
            .FirstOrDefaultAsync(d => d.Id == decisionId, ct);

    public Task<HrSalaryDecision?> GetNonCancelledByPeriodAsync(int periodId, CancellationToken ct = default) =>
        _context.Decisions.FirstOrDefaultAsync(d => d.ReviewPeriodId == periodId && d.Status != SalaryDecisionStatus.CANCELLED, ct);

    public IQueryable<HrSalaryDecision> Query() => _context.Decisions.AsQueryable();

    public async Task AddAsync(HrSalaryDecision decision, CancellationToken ct = default) =>
        await _context.Decisions.AddAsync(decision, ct);

    public Task RemoveDetailAsync(HrSalaryDecisionDetail detail, CancellationToken ct = default)
    {
        _context.DecisionDetails.Remove(detail);
        return Task.CompletedTask;
    }

    /// <summary>Simple year-scoped sequential number (e.g. "SD-2026-001") —
    /// good enough at this project's scale; a high-concurrency system would
    /// need a dedicated sequence instead of COUNT+1.</summary>
    public async Task<string> NextDecisionNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"SD-{year}-";
        var countThisYear = await _context.Decisions.CountAsync(d => d.DecisionNumber.StartsWith(prefix), ct);
        return $"{prefix}{countThisYear + 1:000}";
    }
}
