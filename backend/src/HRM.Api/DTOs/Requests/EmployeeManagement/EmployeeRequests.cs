using System.ComponentModel.DataAnnotations;
using HRM.Domain.Enums;

namespace HRM.Api.DTOs.Requests.EmployeeManagement;

public class CreateEmployeeRequest
{
    [Required] public string EmployeeCode { get; set; } = null!;
    [Required] public string FullName { get; set; } = null!;
    [Required] public int OrganizationalUnitId { get; set; }
    [Required] public int JobTitleId { get; set; }
    [Required] public DateOnly JoinDate { get; set; }
    [Required] public EmploymentStatus EmploymentStatus { get; set; }
}

public class UpdateEmployeeRequest
{
    public string? EmployeeCode { get; set; }
    public string? FullName { get; set; }
    public int? OrganizationalUnitId { get; set; }
    public int? JobTitleId { get; set; }
    public DateOnly? JoinDate { get; set; }
}

public class ChangeEmploymentStatusRequest
{
    [Required] public EmploymentStatus EmploymentStatus { get; set; }
}
