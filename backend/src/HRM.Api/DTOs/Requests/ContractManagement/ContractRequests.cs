using System.ComponentModel.DataAnnotations;
using HRM.Domain.Enums;

namespace HRM.Api.DTOs.Requests.ContractManagement;

public class CreateContractRequest
{
    [Required] public int EmployeeId { get; set; }
    [Required] public ContractType ContractType { get; set; }
    [Required] public string ContractNumber { get; set; } = null!;
    [Required] public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [Required] public decimal ContractSalaryAmount { get; set; }
    public string? SalaryNote { get; set; }
    [Required] public ContractStatus Status { get; set; }
}

public class UpdateContractRequest
{
    [Required] public ContractType ContractType { get; set; }
    [Required] public string ContractNumber { get; set; } = null!;
    [Required] public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [Required] public decimal ContractSalaryAmount { get; set; }
    public string? SalaryNote { get; set; }
}

public class TerminateContractRequest
{
    [Required] public DateOnly TerminationDate { get; set; }
    [Required] public string TerminationReason { get; set; } = null!;
}
