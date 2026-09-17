namespace HRM.Domain.Entities;

public class HrSalaryGrade
{
    public int Id { get; set; }
    public int SalaryScaleId { get; set; }
    public int GradeNumber { get; set; }
    public decimal Coefficient { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Null/anything other than "INACTIVE" counts as active — same
    /// soft-status convention as the rest of the schema (ADR-02).</summary>
    public bool IsActive => Status is null || Status != "INACTIVE";

    public HrSalaryScale SalaryScale { get; set; } = null!;
}
