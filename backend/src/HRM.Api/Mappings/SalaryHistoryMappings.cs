using HRM.Api.DTOs.Responses;
using HRM.Application.SalaryManagement.Models;

namespace HRM.Api.Mappings;

public static class SalaryHistoryMappings
{
    public static SalaryHistoryEntryResponse ToResponse(this SalaryHistoryEntryResult entry) => new()
    {
        Grade = entry.Grade,
        Coefficient = entry.Coefficient,
        EffectiveFrom = entry.EffectiveFrom,
        EffectiveTo = entry.EffectiveTo,
        Reason = entry.Reason,
        DecisionId = entry.DecisionId,
        DecisionNumber = entry.DecisionNumber,
        IsCurrent = entry.IsCurrent
    };
}
