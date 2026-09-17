using HRM.Api.DTOs.Responses;
using HRM.Domain.Entities;

namespace HRM.Api.Mappings;

public static class SalaryDecisionMappings
{
    public static SalaryDecisionResponse ToResponse(this HrSalaryDecision decision) => new()
    {
        Id = decision.Id,
        DecisionNumber = decision.DecisionNumber,
        ReviewPeriodId = decision.ReviewPeriodId,
        DecisionDate = decision.DecisionDate,
        EffectiveDate = decision.EffectiveDate,
        DecisionType = decision.DecisionType,
        Status = decision.Status,
        SignerEmployeeId = decision.SignerEmployeeId,
        Description = decision.Description,
        CreatedAt = decision.CreatedAt,
        UpdatedAt = decision.UpdatedAt
    };

    public static SalaryDecisionDetailResponse ToDetailResponse(this HrSalaryDecision decision)
    {
        var basic = decision.ToResponse();
        return new SalaryDecisionDetailResponse
        {
            Id = basic.Id,
            DecisionNumber = basic.DecisionNumber,
            ReviewPeriodId = basic.ReviewPeriodId,
            DecisionDate = basic.DecisionDate,
            EffectiveDate = basic.EffectiveDate,
            DecisionType = basic.DecisionType,
            Status = basic.Status,
            SignerEmployeeId = basic.SignerEmployeeId,
            Description = basic.Description,
            CreatedAt = basic.CreatedAt,
            UpdatedAt = basic.UpdatedAt,
            Employees = decision.Details.Select(d => new SalaryDecisionEmployeeDetailResponse
            {
                EmployeeId = d.EmployeeId,
                EmployeeCode = d.Employee?.EmployeeCode ?? string.Empty,
                FullName = d.Employee?.FullName ?? string.Empty,
                OldGrade = d.OldGrade?.GradeNumber.ToString(),
                OldCoefficient = d.OldCoefficient,
                NewGrade = d.NewSalaryGrade.GradeNumber.ToString(),
                NewCoefficient = d.NewCoefficient,
                EffectiveFrom = d.EffectiveFrom,
                Reason = d.Reason
            }).ToList()
        };
    }
}
