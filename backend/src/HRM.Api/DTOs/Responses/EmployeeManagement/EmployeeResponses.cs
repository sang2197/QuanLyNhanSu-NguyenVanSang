using HRM.Domain.Enums;

namespace HRM.Api.DTOs.Responses.EmployeeManagement;

public class EmployeeResponse
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int OrganizationalUnitId { get; set; }
    public string? OrganizationalUnitName { get; set; }
    public int JobTitleId { get; set; }
    public string? JobTitleName { get; set; }
    public DateOnly JoinDate { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class EmployeePageResponse
{
    public IReadOnlyList<EmployeeResponse> Items { get; set; } = Array.Empty<EmployeeResponse>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
}
