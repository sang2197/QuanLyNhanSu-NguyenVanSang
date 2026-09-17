using HRM.Application.SalaryManagement.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using HRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRM.Infrastructure.Repositories;

/// <inheritdoc cref="ISalaryRepository"/>
public class SalaryRepository : ISalaryRepository
{
    private readonly HrmDbContext _context;

    public SalaryRepository(HrmDbContext context)
    {
        _context = context;
    }

    public Task<HrSalaryReviewPeriod?> GetReviewPeriodAsync(int periodId, CancellationToken ct = default) =>
        _context.ReviewPeriods.FirstOrDefaultAsync(p => p.Id == periodId, ct);

    public Task<HrSalaryReviewPeriod?> GetReviewPeriodByCodeAsync(string code, CancellationToken ct = default) =>
        _context.ReviewPeriods.FirstOrDefaultAsync(p => p.Code == code, ct);

    public IQueryable<HrSalaryReviewPeriod> QueryReviewPeriods() => _context.ReviewPeriods.AsQueryable();

    public async Task AddReviewPeriodAsync(HrSalaryReviewPeriod period, CancellationToken ct = default) =>
        await _context.ReviewPeriods.AddAsync(period, ct);

    public Task<bool> HasNonCancelledDecisionAsync(int reviewPeriodId, CancellationToken ct = default) =>
        _context.Decisions.AnyAsync(d => d.ReviewPeriodId == reviewPeriodId && d.Status != SalaryDecisionStatus.CANCELLED, ct);

    public Task<HrSalaryReviewEmployee?> GetReviewEmployeeAsync(int periodId, int employeeId, CancellationToken ct = default) =>
        _context.ReviewEmployees
            .Include(e => e.Employee)
            .Include(e => e.CurrentGrade)
            .Include(e => e.ProposedGrade)
            .FirstOrDefaultAsync(e => e.ReviewPeriodId == periodId && e.EmployeeId == employeeId, ct);

    public IQueryable<HrSalaryReviewEmployee> QueryReviewEmployees(int periodId) =>
        _context.ReviewEmployees
            .Include(e => e.Employee)
            .Include(e => e.CurrentGrade)
            .Include(e => e.ProposedGrade)
            .Where(e => e.ReviewPeriodId == periodId);

    public async Task AddReviewEmployeesAsync(IEnumerable<HrSalaryReviewEmployee> entries, CancellationToken ct = default) =>
        await _context.ReviewEmployees.AddRangeAsync(entries, ct);

    public async Task<IReadOnlyList<HrSalaryGrade>> GetGradesForScaleAsync(int salaryScaleId, CancellationToken ct = default) =>
        await _context.SalaryGrades.Where(g => g.SalaryScaleId == salaryScaleId).ToListAsync(ct);

    public Task<HrSalaryDecision?> GetDecisionAsync(int decisionId, CancellationToken ct = default) =>
        _context.Decisions
            .Include(d => d.Details).ThenInclude(det => det.NewSalaryGrade)
            .Include(d => d.Details).ThenInclude(det => det.OldGrade)
            .Include(d => d.Details).ThenInclude(det => det.Employee)
            .Include(d => d.ReviewPeriod)
            .FirstOrDefaultAsync(d => d.Id == decisionId, ct);

    public IQueryable<HrSalaryDecision> QueryDecisions() => _context.Decisions.AsQueryable();

    public async Task AddDecisionAsync(HrSalaryDecision decision, CancellationToken ct = default) =>
        await _context.Decisions.AddAsync(decision, ct);

    public Task RemoveDecisionDetailAsync(HrSalaryDecisionDetail detail, CancellationToken ct = default)
    {
        _context.DecisionDetails.Remove(detail);
        return Task.CompletedTask;
    }

    public Task<HrEmployeeSalary?> GetCurrentSalaryAsync(int employeeId, CancellationToken ct = default) =>
        _context.EmployeeSalaries
            .Include(s => s.SalaryGrade)
            .Where(s => s.EmployeeId == employeeId && s.EffectiveTo == null)
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefaultAsync(ct);

    // A conflict is a salary row already on file dated on/after the decision's
    // effective date — the employee's current (open-ended) record is always
    // dated strictly before that and is expected to be closed, not a conflict.
    public Task<bool> HasEffectiveDateConflictAsync(int employeeId, DateOnly effectiveFrom, CancellationToken ct = default) =>
        _context.EmployeeSalaries.AnyAsync(s =>
            s.EmployeeId == employeeId &&
            s.EffectiveFrom >= effectiveFrom, ct);

    public Task CloseSalaryAsync(HrEmployeeSalary salary, DateOnly effectiveTo, CancellationToken ct = default)
    {
        salary.EffectiveTo = effectiveTo;
        salary.UpdatedAt = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public async Task AddSalaryAsync(HrEmployeeSalary salary, CancellationToken ct = default) =>
        await _context.EmployeeSalaries.AddAsync(salary, ct);

    public IQueryable<HrEmployeeSalary> QuerySalaryHistory(int employeeId) =>
        _context.EmployeeSalaries
            .Include(s => s.SalaryGrade)
            .Include(s => s.Decision)
            .Where(s => s.EmployeeId == employeeId);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
