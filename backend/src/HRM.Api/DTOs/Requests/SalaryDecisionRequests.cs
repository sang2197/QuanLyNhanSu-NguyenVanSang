using System.ComponentModel.DataAnnotations;
using HRM.Domain.Enums;

namespace HRM.Api.DTOs.Requests;

public class CreateSalaryDecisionRequest
{
    [Required] public int ReviewPeriodId { get; set; }
    [Required, MinLength(1)] public List<int> EmployeeIds { get; set; } = new();
    [Required] public string DecisionNumber { get; set; } = null!;
    [Required] public DecisionType DecisionType { get; set; }
    [Required] public DateOnly EffectiveDate { get; set; }
    public int? SignerEmployeeId { get; set; }
    public string? Description { get; set; }
}
