namespace HRM.Domain.Entities;

/// <summary>
/// One employee's line within a salary decision. At Apply time, only
/// BaselineSalaryGradeId is revalidated against the employee's actual
/// current grade (US-SGP-07 AC04) — BaselineCoefficient is stored purely
/// for display/traceability, not itself a revalidation trigger. No
/// per-line EffectiveDate — the effective date is a single attribute of
/// the parent HrSalaryDecision, applied atomically to every included
/// employee.
/// </summary>
public class HrSalaryDecisionDetail
{
    public int Id { get; set; }
    public int SalaryDecisionId { get; set; }
    public int EmployeeId { get; set; }
    public int BaselineSalaryGradeId { get; set; }
    public decimal BaselineCoefficient { get; set; }
    public int NewSalaryGradeId { get; set; }
    public decimal NewCoefficient { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HrSalaryDecision SalaryDecision { get; set; } = null!;
    public HrEmployee Employee { get; set; } = null!;
    public HrSalaryGrade BaselineSalaryGrade { get; set; } = null!;
    public HrSalaryGrade NewSalaryGrade { get; set; } = null!;
}
