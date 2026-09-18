using HRM.Api.DTOs.Responses.SalaryGradePromotion;
using HRM.Application.SalaryGradePromotion.Models;

namespace HRM.Api.Mappings.SalaryGradePromotion;

public static class SalaryHistoryMappings
{
    public static SalaryHistoryEntryResponse ToResponse(this SalaryHistoryEntryResult entry) => new()
    {
        SalaryGradeId = entry.SalaryGradeId,
        GradeNumber = entry.GradeNumber,
        Coefficient = entry.Coefficient,
        EffectiveDate = entry.EffectiveDate,
        Reason = entry.Reason,
        SalaryDecisionId = entry.SalaryDecisionId,
        DecisionNumber = entry.DecisionNumber,
        IsCurrent = entry.IsCurrent
    };
}
