using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using HRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRM.Infrastructure.Repositories.SalaryGradePromotion;

/// <inheritdoc cref="IReviewPeriodRepository"/>
public class ReviewPeriodRepository : IReviewPeriodRepository
{
    private readonly HrmDbContext _context;

    public ReviewPeriodRepository(HrmDbContext context)
    {
        _context = context;
    }

    public Task<HrSalaryReviewPeriod?> GetByIdAsync(int periodId, CancellationToken ct = default) =>
        _context.ReviewPeriods.FirstOrDefaultAsync(p => p.Id == periodId, ct);

    public Task<HrSalaryReviewPeriod?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        _context.ReviewPeriods.FirstOrDefaultAsync(p => p.Code == code, ct);

    public Task<HrSalaryReviewPeriod?> GetByNameAsync(string name, CancellationToken ct = default) =>
        _context.ReviewPeriods.FirstOrDefaultAsync(p => p.Name == name, ct);

    public IQueryable<HrSalaryReviewPeriod> Query() => _context.ReviewPeriods.AsQueryable();

    public IQueryable<HrSalaryReviewPeriod> QueryEligibleForDecision() =>
        _context.ReviewPeriods.Where(p => p.Status == ReviewPeriodStatus.SUBMITTED &&
            !_context.Decisions.Any(d => d.ReviewPeriodId == p.Id && d.Status != SalaryDecisionStatus.CANCELLED));

    public async Task AddAsync(HrSalaryReviewPeriod period, CancellationToken ct = default) =>
        await _context.ReviewPeriods.AddAsync(period, ct);
}
