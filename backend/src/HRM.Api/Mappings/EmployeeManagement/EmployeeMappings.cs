using HRM.Api.DTOs.Responses.EmployeeManagement;
using HRM.Domain.Entities;

namespace HRM.Api.Mappings.EmployeeManagement;

public static class EmployeeMappings
{
    /// <summary>OrganizationalUnitName/JobTitleName are populated only when
    /// the caller loaded those navigations (see EmployeeRepository.GetByIdAsync/Query) —
    /// null otherwise, e.g. right after CreateEmployee.</summary>
    public static EmployeeResponse ToResponse(this HrEmployee employee) => new()
    {
        Id = employee.Id,
        EmployeeCode = employee.EmployeeCode,
        FullName = employee.FullName,
        OrganizationalUnitId = employee.OrganizationalUnitId,
        OrganizationalUnitName = employee.OrganizationalUnit?.Name,
        JobTitleId = employee.JobTitleId,
        JobTitleName = employee.JobTitle?.Name,
        JoinDate = employee.JoinDate,
        EmploymentStatus = employee.EmploymentStatus,
        CreatedAt = employee.CreatedAt,
        UpdatedAt = employee.UpdatedAt
    };
}
