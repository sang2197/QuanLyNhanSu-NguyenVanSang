namespace HRM.Application.OrganizationManagement.Models;

public record CreateOrganizationalUnitInput(
    string Name,
    int? ParentId,
    string UnitType,
    string? ContactEmail,
    string? ContactPhone);
