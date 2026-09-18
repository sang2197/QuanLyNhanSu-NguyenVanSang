namespace HRM.Api.DTOs.Responses.SalaryMasterData;

public class BaseSalaryRateResponse
{
    public int Id { get; set; }
    public decimal Rate { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DateTime? CreatedAt { get; set; }
}
