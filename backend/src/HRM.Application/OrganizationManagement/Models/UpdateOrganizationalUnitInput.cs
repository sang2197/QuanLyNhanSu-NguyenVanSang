namespace HRM.Application.OrganizationManagement.Models;

public record UpdateOrganizationalUnitInput(
    string? Name,
    string? UnitType,
    string? ContactEmail,
    string? ContactPhone);
