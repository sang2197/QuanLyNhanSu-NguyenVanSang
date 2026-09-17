namespace HRM.Domain.Entities;

/// <summary>
/// Effective-dated salary record (ADR-02) — the previous record is closed
/// (EffectiveTo set) rather than overwritten when an employee moves grade.
/// </summary>
public class HrEmployeeSalary
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int SalaryScaleId { get; set; }
    public int SalaryGradeId { get; set; }
    public decimal Coefficient { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string? Reason { get; set; }
    public int? DecisionId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HrEmployee Employee { get; set; } = null!;
    public HrSalaryScale SalaryScale { get; set; } = null!;
    public HrSalaryGrade SalaryGrade { get; set; } = null!;
    public HrSalaryDecision? Decision { get; set; }
}
