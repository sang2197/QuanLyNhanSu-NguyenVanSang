using HRM.Domain.Entities;

namespace HRM.Application.SalaryMasterData.Interfaces;

public interface IBaseSalaryRateRepository
{
    Task<HrBaseSalaryRate?> GetLatestAsync(CancellationToken ct = default);
    Task<HrBaseSalaryRate?> GetAsOfAsync(DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<HrBaseSalaryRate>> ListAsync(DateOnly? asOfDate, CancellationToken ct = default);
    Task AddAsync(HrBaseSalaryRate rate, CancellationToken ct = default);
}
