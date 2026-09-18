using System.ComponentModel.DataAnnotations;

namespace HRM.Api.DTOs.Requests.SalaryGradePromotion;

public class CreateSalaryDecisionRequest
{
    [Required] public int ReviewPeriodId { get; set; }
    [Required, MinLength(1)] public List<int> EmployeeIds { get; set; } = new();
    [Required] public DateOnly EffectiveDate { get; set; }
}

public class UpdateSalaryDecisionRequest
{
    [Required] public DateOnly EffectiveDate { get; set; }
}
