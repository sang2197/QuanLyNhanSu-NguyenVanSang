using HRM.Domain.Enums;

namespace HRM.Application.SalaryGradePromotion.Models;

public record CreateReviewPeriodInput(
    string Code,
    string Name,
    ReviewType ReviewType,
    DateOnly ReviewDate,
    DateOnly? EffectiveDate,
    string? Description);
