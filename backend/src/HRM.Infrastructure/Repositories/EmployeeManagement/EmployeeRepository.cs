using HRM.Application.EmployeeManagement.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using HRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRM.Infrastructure.Repositories.EmployeeManagement;

/// <inheritdoc cref="IEmployeeRepository"/>
public class EmployeeRepository : IEmployeeRepository
{
    private readonly HrmDbContext _context;

    public EmployeeRepository(HrmDbContext context)
    {
        _context = context;
    }

    public Task<HrEmployee?> GetByIdAsync(int employeeId, CancellationToken ct = default) =>
        _context.Employees
            .Include(e => e.OrganizationalUnit)
            .Include(e => e.JobTitle)
            .FirstOrDefaultAsync(e => e.Id == employeeId, ct);

    public Task<HrEmployee?> GetByCodeAsync(string employeeCode, CancellationToken ct = default) =>
        _context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode, ct);

    public IQueryable<HrEmployee> Query() =>
        _context.Employees.Include(e => e.OrganizationalUnit).Include(e => e.JobTitle);

    public IQueryable<HrEmployee> QueryActive() =>
        _context.Employees.Where(e => e.EmploymentStatus == EmploymentStatus.ACTIVE);

    public async Task AddAsync(HrEmployee employee, CancellationToken ct = default) =>
        await _context.Employees.AddAsync(employee, ct);

    public Task<bool> HasActiveInUnitAsync(int organizationalUnitId, CancellationToken ct = default) =>
        _context.Employees.AnyAsync(e => e.OrganizationalUnitId == organizationalUnitId && e.EmploymentStatus == EmploymentStatus.ACTIVE, ct);
}
