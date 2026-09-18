using HRM.Domain.Enums;

namespace HRM.Api.DTOs.Responses.OrganizationManagement;

public class OrganizationalUnitResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int? ParentId { get; set; }
    public string UnitType { get; set; } = null!;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public ActiveStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
