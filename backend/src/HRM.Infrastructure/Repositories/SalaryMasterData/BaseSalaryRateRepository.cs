using HRM.Application.SalaryMasterData.Interfaces;
using HRM.Domain.Entities;
using HRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRM.Infrastructure.Repositories.SalaryMasterData;

/// <inheritdoc cref="IBaseSalaryRateRepository"/>
public class BaseSalaryRateRepository : IBaseSalaryRateRepository
{
    private readonly HrmDbContext _context;

    public BaseSalaryRateRepository(HrmDbContext context)
    {
        _context = context;
    }

    public Task<HrBaseSalaryRate?> GetLatestAsync(CancellationToken ct = default) =>
        _context.BaseSalaryRates.OrderByDescending(r => r.EffectiveDate).FirstOrDefaultAsync(ct);

    public Task<HrBaseSalaryRate?> GetAsOfAsync(DateOnly date, CancellationToken ct = default) =>
        _context.BaseSalaryRates
            .Where(r => r.EffectiveDate <= date)
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<HrBaseSalaryRate>> ListAsync(DateOnly? asOfDate, CancellationToken ct = default)
    {
        var query = _context.BaseSalaryRates.AsQueryable();
        if (asOfDate is DateOnly date)
        {
            query = query.Where(r => r.EffectiveDate <= date);
        }
        return await query.OrderByDescending(r => r.EffectiveDate).ToListAsync(ct);
    }

    public async Task AddAsync(HrBaseSalaryRate rate, CancellationToken ct = default) =>
        await _context.BaseSalaryRates.AddAsync(rate, ct);
}
