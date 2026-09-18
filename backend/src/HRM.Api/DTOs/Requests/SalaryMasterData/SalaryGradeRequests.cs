using System.ComponentModel.DataAnnotations;

namespace HRM.Api.DTOs.Requests.SalaryMasterData;

public class AddSalaryGradeCoefficientRequest
{
    [Required] public decimal Coefficient { get; set; }
    [Required] public DateOnly EffectiveDate { get; set; }
}
