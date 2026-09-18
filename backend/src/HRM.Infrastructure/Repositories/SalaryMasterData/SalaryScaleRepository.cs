using HRM.Application.SalaryMasterData.Interfaces;
using HRM.Domain.Entities;
using HRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRM.Infrastructure.Repositories.SalaryMasterData;

/// <inheritdoc cref="ISalaryScaleRepository"/>
public class SalaryScaleRepository : ISalaryScaleRepository
{
    private readonly HrmDbContext _context;

    public SalaryScaleRepository(HrmDbContext context)
    {
        _context = context;
    }

    public Task<HrSalaryScale?> GetByIdAsync(int scaleId, CancellationToken ct = default) =>
        _context.SalaryScales.FirstOrDefaultAsync(s => s.Id == scaleId, ct);

    public Task<HrSalaryScale?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        _context.SalaryScales.FirstOrDefaultAsync(s => s.Code == code, ct);

    public Task<HrSalaryScale?> GetByNameAsync(string name, CancellationToken ct = default) =>
        _context.SalaryScales.FirstOrDefaultAsync(s => s.Name == name, ct);

    public Task<HrSalaryScale?> GetDetailAsync(int scaleId, CancellationToken ct = default) =>
        _context.SalaryScales
            .Include(s => s.Grades).ThenInclude(g => g.Coefficients)
            .FirstOrDefaultAsync(s => s.Id == scaleId, ct);

    public async Task<IReadOnlyList<HrSalaryScale>> GetAllAsync(CancellationToken ct = default) =>
        await _context.SalaryScales.ToListAsync(ct);

    public async Task AddAsync(HrSalaryScale scale, CancellationToken ct = default) =>
        await _context.SalaryScales.AddAsync(scale, ct);
}
