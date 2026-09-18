using HRM.Api.DTOs.Responses.SalaryGradePromotion;
using HRM.Domain.Entities;

namespace HRM.Api.Mappings.SalaryGradePromotion;

public static class SalaryDecisionMappings
{
    public static SalaryDecisionResponse ToResponse(this HrSalaryDecision decision) => new()
    {
        Id = decision.Id,
        DecisionNumber = decision.DecisionNumber,
        ReviewPeriodId = decision.ReviewPeriodId,
        EffectiveDate = decision.EffectiveDate,
        Status = decision.Status,
        CreatedAt = decision.CreatedAt,
        UpdatedAt = decision.UpdatedAt
    };

    /// <summary>Requires Details (+ each detail's Employee, BaselineSalaryGrade,
    /// NewSalaryGrade) to be loaded — see SalaryDecisionRepository.GetByIdAsync.</summary>
    public static SalaryDecisionDetailResponse ToDetailResponse(this HrSalaryDecision decision)
    {
        var basic = decision.ToResponse();
        return new SalaryDecisionDetailResponse
        {
            Id = basic.Id,
            DecisionNumber = basic.DecisionNumber,
            ReviewPeriodId = basic.ReviewPeriodId,
            EffectiveDate = basic.EffectiveDate,
            Status = basic.Status,
            CreatedAt = basic.CreatedAt,
            UpdatedAt = basic.UpdatedAt,
            Employees = decision.Details.Select(d => new SalaryDecisionEmployeeLineResponse
            {
                EmployeeId = d.EmployeeId,
                EmployeeCode = d.Employee.EmployeeCode,
                FullName = d.Employee.FullName,
                BaselineSalaryGradeId = d.BaselineSalaryGradeId,
                BaselineGradeNumber = d.BaselineSalaryGrade.GradeNumber,
                BaselineCoefficient = d.BaselineCoefficient,
                NewSalaryGradeId = d.NewSalaryGradeId,
                NewGradeNumber = d.NewSalaryGrade.GradeNumber,
                NewCoefficient = d.NewCoefficient
            }).ToList()
        };
    }
}
