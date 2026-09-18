using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using HRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRM.Infrastructure.Repositories.SalaryGradePromotion;

/// <inheritdoc cref="IEmployeeSalaryRepository"/>
public class EmployeeSalaryRepository : IEmployeeSalaryRepository
{
    private readonly HrmDbContext _context;

    public EmployeeSalaryRepository(HrmDbContext context)
    {
        _context = context;
    }

    public Task<HrEmployeeSalary?> GetCurrentAsync(int employeeId, CancellationToken ct = default) =>
        _context.EmployeeSalaries
            .Include(s => s.SalaryGrade)
            .Where(s => s.EmployeeId == employeeId)
            .OrderByDescending(s => s.EffectiveDate)
            .FirstOrDefaultAsync(ct);

    public IQueryable<HrEmployeeSalary> QueryHistory(int employeeId) =>
        _context.EmployeeSalaries
            .Include(s => s.SalaryGrade)
            .Include(s => s.SalaryDecision)
            .Where(s => s.EmployeeId == employeeId);

    public async Task AddAsync(HrEmployeeSalary salary, CancellationToken ct = default) =>
        await _context.EmployeeSalaries.AddAsync(salary, ct);

    /// <summary>
    /// "Currently assigned" (BR-SAL-15) means the employee's most recent
    /// HrEmployeeSalary row is on this grade AND the employee is Active.
    /// Walked explicitly (not a single GroupBy query) to stay portable across
    /// EF providers — this table is not expected to be large enough for the
    /// extra round trips to matter.
    /// </summary>
    public async Task<bool> HasActiveEmployeeOnGradeAsync(int gradeId, CancellationToken ct = default)
    {
        var employeeIdsOnGrade = await _context.EmployeeSalaries
            .Where(s => s.SalaryGradeId == gradeId)
            .Select(s => s.EmployeeId)
            .Distinct()
            .ToListAsync(ct);

        foreach (var employeeId in employeeIdsOnGrade)
        {
            var current = await GetCurrentAsync(employeeId, ct);
            if (current?.SalaryGradeId != gradeId)
            {
                continue;
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId, ct);
            if (employee?.EmploymentStatus == EmploymentStatus.ACTIVE)
            {
                return true;
            }
        }

        return false;
    }
}
