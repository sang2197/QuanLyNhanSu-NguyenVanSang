using HRM.Domain.Entities;

namespace HRM.Application.OrganizationManagement.Interfaces;

public interface IOrganizationalUnitRepository
{
    Task<HrOrganizationalUnit?> GetByIdAsync(int unitId, CancellationToken ct = default);

    /// <summary><paramref name="parentId"/> null means top-level (BR-ORG-03).</summary>
    Task<HrOrganizationalUnit?> GetByNameUnderParentAsync(int? parentId, string name, CancellationToken ct = default);

    /// <summary>All units, active and inactive (BR-ORG-15) — unpaginated flat
    /// list, each unit carrying its own ParentId, matching how the UI renders
    /// the whole tree at once.</summary>
    Task<IReadOnlyList<HrOrganizationalUnit>> GetAllAsync(CancellationToken ct = default);

    /// <summary>For MoveUnit's not-self/not-descendant guard (BR-ORG-07).</summary>
    Task<IReadOnlyList<int>> GetDescendantIdsAsync(int unitId, CancellationToken ct = default);

    Task AddAsync(HrOrganizationalUnit unit, CancellationToken ct = default);
    Task<int> CountActiveChildrenAsync(int unitId, CancellationToken ct = default);
}
