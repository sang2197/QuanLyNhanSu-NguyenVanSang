using System.ComponentModel.DataAnnotations;

namespace HRM.Api.DTOs.Requests.OrganizationManagement;

public class CreateOrganizationalUnitRequest
{
    [Required] public string Name { get; set; } = null!;
    public int? ParentId { get; set; }
    [Required] public string UnitType { get; set; } = null!;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}

public class UpdateOrganizationalUnitRequest
{
    public string? Name { get; set; }
    public string? UnitType { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}

public class MoveOrganizationalUnitRequest
{
    [Required] public int TargetParentId { get; set; }
}
