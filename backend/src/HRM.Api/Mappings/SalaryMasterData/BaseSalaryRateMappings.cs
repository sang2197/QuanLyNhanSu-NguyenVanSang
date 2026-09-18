using HRM.Api.DTOs.Responses.SalaryMasterData;
using HRM.Domain.Entities;

namespace HRM.Api.Mappings.SalaryMasterData;

public static class BaseSalaryRateMappings
{
    public static BaseSalaryRateResponse ToResponse(this HrBaseSalaryRate rate) => new()
    {
        Id = rate.Id,
        Rate = rate.Rate,
        EffectiveDate = rate.EffectiveDate,
        CreatedAt = rate.CreatedAt
    };
}
