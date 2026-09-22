using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

public class HrLaborContract
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string ContractNumber { get; set; } = null!;
    public ContractType ContractType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal ContractSalaryAmount { get; set; }
    public string? SalaryNote { get; set; }
    public ContractStatus Status { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public string? TerminationReason { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HrEmployee Employee { get; set; } = null!;
}
