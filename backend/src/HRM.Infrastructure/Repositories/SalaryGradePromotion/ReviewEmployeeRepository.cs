using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using HRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRM.Infrastructure.Repositories.SalaryGradePromotion;

/// <inheritdoc cref="IReviewEmployeeRepository"/>
public class ReviewEmployeeRepository : IReviewEmployeeRepository
{
    private readonly HrmDbContext _context;

    public ReviewEmployeeRepository(HrmDbContext context)
    {
        _context = context;
    }

    public IQueryable<HrSalaryReviewEmployee> QueryByPeriod(int periodId) =>
        _context.ReviewEmployees
            .Include(e => e.Employee).ThenInclude(emp => emp.OrganizationalUnit)
            .Include(e => e.CurrentSalaryGrade)
            .Include(e => e.ProposedSalaryGrade)
            .Where(e => e.ReviewPeriodId == periodId);

    public Task<HrSalaryReviewEmployee?> GetByPeriodAndEmployeeAsync(int periodId, int employeeId, CancellationToken ct = default) =>
        _context.ReviewEmployees
            .Include(e => e.Employee).ThenInclude(emp => emp.OrganizationalUnit)
            .Include(e => e.CurrentSalaryGrade)
            .Include(e => e.ProposedSalaryGrade)
            .FirstOrDefaultAsync(e => e.ReviewPeriodId == periodId && e.EmployeeId == employeeId, ct);

    public async Task AddRangeAsync(IEnumerable<HrSalaryReviewEmployee> entries, CancellationToken ct = default) =>
        await _context.ReviewEmployees.AddRangeAsync(entries, ct);

    public Task<int> CountUnprocessedEligibleAsync(int periodId, CancellationToken ct = default) =>
        _context.ReviewEmployees.CountAsync(e => e.ReviewPeriodId == periodId && e.Eligible && e.Outcome == ReviewOutcome.PENDING, ct);

    public async Task<IReadOnlyList<HrSalaryReviewEmployee>> GetApprovedAsync(int periodId, IReadOnlyList<int> employeeIds, CancellationToken ct = default) =>
        await _context.ReviewEmployees
            .Where(e => e.ReviewPeriodId == periodId && employeeIds.Contains(e.EmployeeId) && e.Outcome == ReviewOutcome.APPROVED)
            .ToListAsync(ct);
}
