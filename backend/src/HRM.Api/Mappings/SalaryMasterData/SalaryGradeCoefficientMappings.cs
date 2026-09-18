using HRM.Api.DTOs.Responses.SalaryMasterData;
using HRM.Domain.Entities;

namespace HRM.Api.Mappings.SalaryMasterData;

public static class SalaryGradeCoefficientMappings
{
    public static SalaryGradeCoefficientResponse ToResponse(this HrSalaryGradeCoefficient coefficient) => new()
    {
        Id = coefficient.Id,
        SalaryGradeId = coefficient.SalaryGradeId,
        Coefficient = coefficient.Coefficient,
        EffectiveDate = coefficient.EffectiveDate,
        CreatedAt = coefficient.CreatedAt
    };
}
