namespace HRM.Domain.Entities;

public class HrEmployee
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int? DepartmentId { get; set; }
    public int? PositionId { get; set; }
    public DateOnly? JoinDate { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<HrEmployeeSalary> Salaries { get; set; } = new List<HrEmployeeSalary>();
    public ICollection<HrSalaryReviewEmployee> ReviewEntries { get; set; } = new List<HrSalaryReviewEmployee>();
    public ICollection<HrSalaryDecisionDetail> DecisionDetails { get; set; } = new List<HrSalaryDecisionDetail>();
}
