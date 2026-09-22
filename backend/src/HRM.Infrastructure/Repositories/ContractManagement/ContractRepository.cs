using HRM.Application.ContractManagement.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using HRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRM.Infrastructure.Repositories.ContractManagement;

/// <inheritdoc cref="IContractRepository"/>
public class ContractRepository : IContractRepository
{
    private readonly HrmDbContext _context;

    public ContractRepository(HrmDbContext context)
    {
        _context = context;
    }

    public Task<HrLaborContract?> GetByIdAsync(int contractId, CancellationToken ct = default) =>
        IncludeEmployee(_context.Contracts).FirstOrDefaultAsync(c => c.Id == contractId, ct);

    public Task<HrLaborContract?> GetByNumberAsync(string contractNumber, CancellationToken ct = default) =>
        _context.Contracts.FirstOrDefaultAsync(c => c.ContractNumber == contractNumber, ct);

    public IQueryable<HrLaborContract> Query() => IncludeEmployee(_context.Contracts);

    public async Task AddAsync(HrLaborContract contract, CancellationToken ct = default) =>
        await _context.Contracts.AddAsync(contract, ct);

    public Task RemoveAsync(HrLaborContract contract, CancellationToken ct = default)
    {
        _context.Contracts.Remove(contract);
        return Task.CompletedTask;
    }

    public Task<bool> HasOtherActiveForEmployeeAsync(int employeeId, int? exceptContractId, CancellationToken ct = default) =>
        _context.Contracts.AnyAsync(c =>
            c.EmployeeId == employeeId &&
            c.Status == ContractStatus.ACTIVE &&
            (exceptContractId == null || c.Id != exceptContractId), ct);

    // Employee, plus its own Organizational Unit / Job Title, so
    // employeeCode/employeeFullName/organizationalUnitName/jobTitleName/
    // employmentStatus on the response are read live (BR-CON-15) without a
    // cross-domain Service call — the same Include pattern EmployeeRepository
    // already uses for its own cross-domain navigations.
    private static IQueryable<HrLaborContract> IncludeEmployee(IQueryable<HrLaborContract> query) =>
        query.Include(c => c.Employee).ThenInclude(e => e.OrganizationalUnit)
             .Include(c => c.Employee).ThenInclude(e => e.JobTitle);
}
