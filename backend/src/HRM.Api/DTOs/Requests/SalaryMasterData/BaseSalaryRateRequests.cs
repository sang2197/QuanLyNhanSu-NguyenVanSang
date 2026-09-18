using System.ComponentModel.DataAnnotations;

namespace HRM.Api.DTOs.Requests.SalaryMasterData;

public class AddBaseSalaryRateRequest
{
    [Required] public decimal Rate { get; set; }
    [Required] public DateOnly EffectiveDate { get; set; }
}
