namespace HRM.Application.EmployeeManagement.Models;

public record UpdateEmployeeInput(
    string? EmployeeCode,
    string? FullName,
    int? OrganizationalUnitId,
    int? JobTitleId,
    DateOnly? JoinDate);
