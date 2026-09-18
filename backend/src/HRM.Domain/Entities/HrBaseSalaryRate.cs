namespace HRM.Domain.Entities;

/// <summary>Single organization-wide, effective-dated rate — never
/// overwritten, only appended to (BR-SAL-01/02/03). No relationships to
/// other tables.</summary>
public class HrBaseSalaryRate
{
    public int Id { get; set; }
    public decimal Rate { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DateTime? CreatedAt { get; set; }
}
