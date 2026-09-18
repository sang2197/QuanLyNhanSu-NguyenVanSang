using HRM.Application.OrganizationManagement.Interfaces;
using HRM.Domain.Entities;
using HRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRM.Infrastructure.Repositories.OrganizationManagement;

/// <inheritdoc cref="IJobTitleRepository"/>
public class JobTitleRepository : IJobTitleRepository
{
    private readonly HrmDbContext _context;

    public JobTitleRepository(HrmDbContext context)
    {
        _context = context;
    }

    public Task<HrJobTitle?> GetByIdAsync(int jobTitleId, CancellationToken ct = default) =>
        _context.JobTitles.FirstOrDefaultAsync(t => t.Id == jobTitleId, ct);

    public Task<HrJobTitle?> GetByNameAsync(string name, CancellationToken ct = default) =>
        _context.JobTitles.FirstOrDefaultAsync(t => t.Name == name, ct);

    public async Task<IReadOnlyList<HrJobTitle>> GetAllAsync(CancellationToken ct = default) =>
        await _context.JobTitles.ToListAsync(ct);

    public async Task AddAsync(HrJobTitle jobTitle, CancellationToken ct = default) =>
        await _context.JobTitles.AddAsync(jobTitle, ct);
}
