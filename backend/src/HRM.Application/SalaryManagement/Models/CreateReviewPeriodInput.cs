using HRM.Domain.Enums;

namespace HRM.Application.SalaryManagement.Models;

public record CreateReviewPeriodInput(
    string Code,
    string Name,
    ReviewType ReviewType,
    DateOnly ReviewDate,
    DateOnly? EffectiveDate,
    string? Description);
