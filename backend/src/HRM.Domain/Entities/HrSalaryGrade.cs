using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

/// <summary>No Coefficient column here — see HrSalaryGradeCoefficient. A
/// single mutable coefficient column cannot represent more than one
/// historical value per grade, which BR-SAL-12/13 requires.</summary>
public class HrSalaryGrade
{
    public int Id { get; set; }
    public int SalaryScaleId { get; set; }
    public int GradeNumber { get; set; }
    public ActiveStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HrSalaryScale SalaryScale { get; set; } = null!;
    public ICollection<HrSalaryGradeCoefficient> Coefficients { get; set; } = new List<HrSalaryGradeCoefficient>();
}
