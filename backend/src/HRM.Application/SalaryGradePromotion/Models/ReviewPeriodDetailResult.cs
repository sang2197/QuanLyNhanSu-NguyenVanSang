using HRM.Domain.Entities;

namespace HRM.Application.SalaryGradePromotion.Models;

/// <summary>
/// Wraps a review period with the aggregate counts and decision-linkage the
/// "Review Period Detail" screen needs (ReviewPeriodDetail schema in openapi.yaml).
/// </summary>
public record ReviewPeriodDetailResult(
    HrSalaryReviewPeriod Period,
    int TotalEmployees,
    int EligibleCount,
    int ApprovedCount,
    int RejectedCount,
    int PendingCount,
    int? DecisionId);
