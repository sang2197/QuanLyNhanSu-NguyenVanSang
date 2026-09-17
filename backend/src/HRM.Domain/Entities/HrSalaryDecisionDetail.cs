namespace HRM.Domain.Entities;

public class HrSalaryDecisionDetail
{
    public int Id { get; set; }
    public int DecisionId { get; set; }
    public int EmployeeId { get; set; }
    public int? OldSalaryId { get; set; }
    public int? OldGradeId { get; set; }
    public decimal? OldCoefficient { get; set; }
    public int NewSalaryGradeId { get; set; }
    public decimal NewCoefficient { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public string? Reason { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HrSalaryDecision Decision { get; set; } = null!;
    public HrEmployee Employee { get; set; } = null!;
    public HrEmployeeSalary? OldSalary { get; set; }
    public HrSalaryGrade? OldGrade { get; set; }
    public HrSalaryGrade NewSalaryGrade { get; set; } = null!;
}
