namespace HRM.Domain.Entities;

/// <summary>
/// Append-only per-employee salary/grade history (ADR-02) — the previous
/// record is never overwritten; a new row with a later EffectiveDate
/// becomes current. Coefficient is copied (frozen) from the grade's
/// coefficient history at the time this row is written, so a later
/// coefficient change for the grade does not retroactively rewrite past
/// history. SalaryDecisionId is set when the row originated from an
/// applied Salary Decision and left null otherwise (e.g. Reason =
/// "Initial assignment").
/// </summary>
public class HrEmployeeSalary
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int SalaryGradeId { get; set; }
    public decimal Coefficient { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public string Reason { get; set; } = null!;
    public int? SalaryDecisionId { get; set; }
    public DateTime? CreatedAt { get; set; }

    public HrEmployee Employee { get; set; } = null!;
    public HrSalaryGrade SalaryGrade { get; set; } = null!;
    public HrSalaryDecision? SalaryDecision { get; set; }
}
