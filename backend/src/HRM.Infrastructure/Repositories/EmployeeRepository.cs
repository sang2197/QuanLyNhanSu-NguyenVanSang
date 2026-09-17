using HRM.Application.SalaryManagement.Interfaces;
using HRM.Domain.Entities;
using HRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRM.Infrastructure.Repositories;

/// <inheritdoc cref="IEmployeeRepository"/>
public class EmployeeRepository : IEmployeeRepository
{
    private readonly HrmDbContext _context;

    public EmployeeRepository(HrmDbContext context)
    {
        _context = context;
    }

    public Task<HrEmployee?> GetByIdAsync(int employeeId, CancellationToken ct = default) =>
        _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId, ct);

    public IQueryable<HrEmployee> QueryActive() =>
        _context.Employees.Where(e => e.Status == null || e.Status != "INACTIVE");
}
