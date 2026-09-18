namespace HRM.Domain.Entities;

/// <summary>Effective-dated coefficient history for one salary grade
/// (BR-SAL-12/13) — the grade's initial coefficient (set at creation, no
/// separate effective date required) is simply the first row.</summary>
public class HrSalaryGradeCoefficient
{
    public int Id { get; set; }
    public int SalaryGradeId { get; set; }
    public decimal Coefficient { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DateTime? CreatedAt { get; set; }

    public HrSalaryGrade SalaryGrade { get; set; } = null!;
}
