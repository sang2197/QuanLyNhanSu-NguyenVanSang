using System.ComponentModel.DataAnnotations;
using HRM.Domain.Enums;

namespace HRM.Api.DTOs.Requests;

public class CreateReviewPeriodRequest
{
    [Required] public string Code { get; set; } = null!;
    [Required] public string Name { get; set; } = null!;
    [Required] public ReviewType ReviewType { get; set; }
    [Required] public DateOnly ReviewDate { get; set; }
    public DateOnly? EffectiveDate { get; set; }
    public string? Description { get; set; }
}
