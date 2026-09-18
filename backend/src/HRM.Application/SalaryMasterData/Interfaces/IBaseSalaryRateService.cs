using HRM.Domain.Entities;

namespace HRM.Application.SalaryMasterData.Interfaces;

public interface IBaseSalaryRateService
{
    Task<HrBaseSalaryRate> AddRateAsync(decimal rate, DateOnly effectiveDate, CancellationToken ct = default);
    Task<IReadOnlyList<HrBaseSalaryRate>> ListRatesAsync(DateOnly? asOfDate, CancellationToken ct = default);
    Task<HrBaseSalaryRate> GetRateAsOfAsync(DateOnly date, CancellationToken ct = default);
}
