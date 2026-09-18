using HRM.Domain.Enums;

namespace HRM.Application.EmployeeManagement.Models;

public record CreateEmployeeInput(
    string EmployeeCode,
    string FullName,
    int OrganizationalUnitId,
    int JobTitleId,
    DateOnly JoinDate,
    EmploymentStatus EmploymentStatus);
