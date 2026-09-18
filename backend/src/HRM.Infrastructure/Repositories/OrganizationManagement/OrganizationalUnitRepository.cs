using HRM.Application.OrganizationManagement.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using HRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRM.Infrastructure.Repositories.OrganizationManagement;

/// <inheritdoc cref="IOrganizationalUnitRepository"/>
public class OrganizationalUnitRepository : IOrganizationalUnitRepository
{
    private readonly HrmDbContext _context;

    public OrganizationalUnitRepository(HrmDbContext context)
    {
        _context = context;
    }

    public Task<HrOrganizationalUnit?> GetByIdAsync(int unitId, CancellationToken ct = default) =>
        _context.OrganizationalUnits.FirstOrDefaultAsync(u => u.Id == unitId, ct);

    public Task<HrOrganizationalUnit?> GetByNameUnderParentAsync(int? parentId, string name, CancellationToken ct = default) =>
        _context.OrganizationalUnits.FirstOrDefaultAsync(u => u.ParentId == parentId && u.Name == name, ct);

    public async Task<IReadOnlyList<HrOrganizationalUnit>> GetAllAsync(CancellationToken ct = default) =>
        await _context.OrganizationalUnits.ToListAsync(ct);

    public async Task<IReadOnlyList<int>> GetDescendantIdsAsync(int unitId, CancellationToken ct = default)
    {
        // No recursive CTE via LINQ — the whole (small) org tree is loaded
        // once and walked breadth-first in memory.
        var all = await _context.OrganizationalUnits.AsNoTracking().ToListAsync(ct);
        var descendantIds = new List<int>();
        var frontier = new Queue<int>();
        frontier.Enqueue(unitId);

        while (frontier.Count > 0)
        {
            var currentId = frontier.Dequeue();
            foreach (var child in all.Where(u => u.ParentId == currentId))
            {
                descendantIds.Add(child.Id);
                frontier.Enqueue(child.Id);
            }
        }

        return descendantIds;
    }

    public async Task AddAsync(HrOrganizationalUnit unit, CancellationToken ct = default) =>
        await _context.OrganizationalUnits.AddAsync(unit, ct);

    public Task<int> CountActiveChildrenAsync(int unitId, CancellationToken ct = default) =>
        _context.OrganizationalUnits.CountAsync(u => u.ParentId == unitId && u.Status == ActiveStatus.ACTIVE, ct);
}
