using HRM.Application.OrganizationManagement.Models;
using HRM.Domain.Entities;

namespace HRM.Application.OrganizationManagement.Interfaces;

public interface IOrganizationalUnitService
{
    Task<HrOrganizationalUnit> CreateUnitAsync(CreateOrganizationalUnitInput input, CancellationToken ct = default);
    Task<IReadOnlyList<HrOrganizationalUnit>> GetStructureAsync(CancellationToken ct = default);
    Task<HrOrganizationalUnit> UpdateUnitAsync(int unitId, UpdateOrganizationalUnitInput input, CancellationToken ct = default);
    Task<HrOrganizationalUnit> MoveUnitAsync(int unitId, int targetParentId, CancellationToken ct = default);
    Task<HrOrganizationalUnit> DeactivateUnitAsync(int unitId, CancellationToken ct = default);
    Task<HrOrganizationalUnit> ReactivateUnitAsync(int unitId, CancellationToken ct = default);

    /// <summary>Exposed cross-domain for Employee Management (BR-EMP-04, ADR-03).</summary>
    Task<bool> IsUnitActiveAsync(int unitId, CancellationToken ct = default);
}
