using HRM.Domain.Enums;

namespace HRM.Api.DTOs.Responses.ContractManagement;

public class ContractResponse
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeFullName { get; set; }
    public string ContractNumber { get; set; } = null!;
    public ContractType ContractType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal ContractSalaryAmount { get; set; }
    public string? SalaryNote { get; set; }
    public ContractStatus Status { get; set; }

    /// <summary>True when Status is ACTIVE and EndDate is earlier than today (BR-CON-20).</summary>
    public bool Overdue { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public string? TerminationReason { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ContractDetailResponse : ContractResponse
{
    public string? OrganizationalUnitName { get; set; }
    public string? JobTitleName { get; set; }
    public EmploymentStatus? EmploymentStatus { get; set; }
}

public class ContractPageResponse
{
    public IReadOnlyList<ContractResponse> Items { get; set; } = Array.Empty<ContractResponse>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
}
