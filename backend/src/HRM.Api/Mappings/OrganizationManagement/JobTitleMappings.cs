using HRM.Api.DTOs.Responses.OrganizationManagement;
using HRM.Domain.Entities;

namespace HRM.Api.Mappings.OrganizationManagement;

public static class JobTitleMappings
{
    public static JobTitleResponse ToResponse(this HrJobTitle jobTitle) => new()
    {
        Id = jobTitle.Id,
        Name = jobTitle.Name,
        Status = jobTitle.Status,
        CreatedAt = jobTitle.CreatedAt,
        UpdatedAt = jobTitle.UpdatedAt
    };
}
