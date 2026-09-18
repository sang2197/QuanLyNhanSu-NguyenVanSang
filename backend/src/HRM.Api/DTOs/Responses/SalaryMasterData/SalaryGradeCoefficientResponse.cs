namespace HRM.Api.DTOs.Responses.SalaryMasterData;

public class SalaryGradeCoefficientResponse
{
    public int Id { get; set; }
    public int SalaryGradeId { get; set; }
    public decimal Coefficient { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DateTime? CreatedAt { get; set; }
}
