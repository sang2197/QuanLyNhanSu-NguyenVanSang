using HRM.Api.DTOs.Responses;
using HRM.Application.SalaryManagement.Models;
using HRM.Domain.Entities;

namespace HRM.Api.Mappings;

public static class ReviewPeriodMappings
{
    public static ReviewPeriodResponse ToResponse(this HrSalaryReviewPeriod period) => new()
    {
        Id = period.Id,
        Code = period.Code,
        Name = period.Name,
        ReviewType = period.ReviewType,
        ReviewDate = period.ReviewDate,
        EffectiveDate = period.EffectiveDate,
        Status = period.Status,
        Description = period.Description,
        CreatedAt = period.CreatedAt,
        UpdatedAt = period.UpdatedAt
    };

    public static ReviewPeriodDetailResponse ToDetailResponse(this ReviewPeriodDetailResult result)
    {
        var basic = result.Period.ToResponse();
        return new ReviewPeriodDetailResponse
        {
            Id = basic.Id,
            Code = basic.Code,
            Name = basic.Name,
            ReviewType = basic.ReviewType,
            ReviewDate = basic.ReviewDate,
            EffectiveDate = basic.EffectiveDate,
            Status = basic.Status,
            Description = basic.Description,
            CreatedAt = basic.CreatedAt,
            UpdatedAt = basic.UpdatedAt,
            TotalEmployees = result.TotalEmployees,
            EligibleCount = result.EligibleCount,
            ApprovedCount = result.ApprovedCount,
            RejectedCount = result.RejectedCount,
            PendingCount = result.PendingCount,
            DecisionId = result.DecisionId
        };
    }
}
