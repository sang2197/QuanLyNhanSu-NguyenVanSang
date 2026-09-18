using HRM.Application.SalaryMasterData.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using HRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRM.Infrastructure.Repositories.SalaryMasterData;

/// <inheritdoc cref="ISalaryGradeRepository"/>
public class SalaryGradeRepository : ISalaryGradeRepository
{
    private readonly HrmDbContext _context;

    public SalaryGradeRepository(HrmDbContext context)
    {
        _context = context;
    }

    public Task<HrSalaryGrade?> GetByIdAsync(int gradeId, CancellationToken ct = default) =>
        _context.SalaryGrades.FirstOrDefaultAsync(g => g.Id == gradeId, ct);

    public Task<HrSalaryGrade?> GetByScaleAndNumberAsync(int scaleId, int gradeNumber, CancellationToken ct = default) =>
        _context.SalaryGrades.FirstOrDefaultAsync(g => g.SalaryScaleId == scaleId && g.GradeNumber == gradeNumber, ct);

    public async Task<IReadOnlyList<HrSalaryGrade>> ListByScaleAsync(int scaleId, CancellationToken ct = default) =>
        await _context.SalaryGrades.Where(g => g.SalaryScaleId == scaleId).OrderBy(g => g.GradeNumber).ToListAsync(ct);

    public Task<int> CountActiveByScaleAsync(int scaleId, CancellationToken ct = default) =>
        _context.SalaryGrades.CountAsync(g => g.SalaryScaleId == scaleId && g.Status == ActiveStatus.ACTIVE, ct);

    public async Task AddAsync(HrSalaryGrade grade, CancellationToken ct = default) =>
        await _context.SalaryGrades.AddAsync(grade, ct);

    public async Task AddCoefficientAsync(HrSalaryGradeCoefficient coefficient, CancellationToken ct = default) =>
        await _context.SalaryGradeCoefficients.AddAsync(coefficient, ct);

    public Task<HrSalaryGradeCoefficient?> GetLatestCoefficientAsync(int gradeId, CancellationToken ct = default) =>
        _context.SalaryGradeCoefficients
            .Where(c => c.SalaryGradeId == gradeId)
            .OrderByDescending(c => c.EffectiveDate)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<HrSalaryGradeCoefficient>> ListCoefficientsAsync(int gradeId, CancellationToken ct = default) =>
        await _context.SalaryGradeCoefficients
            .Where(c => c.SalaryGradeId == gradeId)
            .OrderByDescending(c => c.EffectiveDate)
            .ToListAsync(ct);
}
