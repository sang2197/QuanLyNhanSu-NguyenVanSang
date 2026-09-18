using HRM.Api.DTOs.Responses.OrganizationManagement;
using HRM.Domain.Entities;

namespace HRM.Api.Mappings.OrganizationManagement;

public static class OrganizationalUnitMappings
{
    public static OrganizationalUnitResponse ToResponse(this HrOrganizationalUnit unit) => new()
    {
        Id = unit.Id,
        Name = unit.Name,
        ParentId = unit.ParentId,
        UnitType = unit.UnitType,
        ContactEmail = unit.ContactEmail,
        ContactPhone = unit.ContactPhone,
        Status = unit.Status,
        CreatedAt = unit.CreatedAt,
        UpdatedAt = unit.UpdatedAt
    };
}
