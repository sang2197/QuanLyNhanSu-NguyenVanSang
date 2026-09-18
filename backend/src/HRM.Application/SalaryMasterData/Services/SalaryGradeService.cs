using HRM.Application.Common;
using HRM.Application.Exceptions;
using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Application.SalaryMasterData.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.SalaryMasterData.Services;

/// <inheritdoc cref="ISalaryGradeService"/>
public class SalaryGradeService : ISalaryGradeService
{
    private readonly ISalaryGradeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISalaryScaleService _scaleService;

    // Lazy: ISalaryHistoryService lives in Salary Grade Promotion, which
    // isn't implemented until a later milestone. A plain constructor
    // injection would fail DI resolution for every SalaryGradesController
    // request in the meantime, not just DeactivateGradeAsync's — deferring
    // resolution until actually invoked avoids that.
    private readonly Lazy<ISalaryHistoryService> _historyService;

    public SalaryGradeService(
        ISalaryGradeRepository repository,
        IUnitOfWork unitOfWork,
        ISalaryScaleService scaleService,
        Lazy<ISalaryHistoryService> historyService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _scaleService = scaleService;
        _historyService = historyService;
    }

    public async Task<HrSalaryGradeCoefficient> AddCoefficientAsync(int gradeId, decimal coefficient, DateOnly effectiveDate, CancellationToken ct = default)
    {
        var grade = await GetOrThrowAsync(gradeId, ct);
        if (grade.Status != ActiveStatus.ACTIVE)
        {
            throw new ConflictException("Salary grade is inactive."); // BR-SAL-13A
        }
        if (coefficient <= 0)
        {
            throw new ValidationException("Coefficient must be greater than zero."); // BR-SAL-10
        }

        var latest = await _repository.GetLatestCoefficientAsync(gradeId, ct);
        if (latest is not null && effectiveDate <= latest.EffectiveDate)
        {
            throw new ConflictException("Effective date must be later than the latest recorded effective date."); // BR-SAL-13
        }

        var newCoefficient = new HrSalaryGradeCoefficient { SalaryGradeId = gradeId, Coefficient = coefficient, EffectiveDate = effectiveDate };
        await _repository.AddCoefficientAsync(newCoefficient, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return newCoefficient;
    }

    public async Task<IReadOnlyList<HrSalaryGradeCoefficient>> ListCoefficientsAsync(int gradeId, CancellationToken ct = default) =>
        await _repository.ListCoefficientsAsync(gradeId, ct);

    public async Task<HrSalaryGrade> DeactivateGradeAsync(int gradeId, CancellationToken ct = default)
    {
        var grade = await GetOrThrowAsync(gradeId, ct);

        if (await _historyService.Value.HasActiveEmployeeOnGradeAsync(gradeId, ct))
        {
            throw new ConflictException("An active employee is currently assigned to this grade."); // BR-SAL-15
        }

        grade.Status = ActiveStatus.INACTIVE;
        await _unitOfWork.SaveChangesAsync(ct);
        return grade;
    }

    public async Task<HrSalaryGrade> ReactivateGradeAsync(int gradeId, CancellationToken ct = default)
    {
        var grade = await GetOrThrowAsync(gradeId, ct);

        if (!await _scaleService.IsScaleActiveAsync(grade.SalaryScaleId, ct))
        {
            throw new ConflictException("Salary scale must be reactivated first."); // BR-SAL-18
        }

        grade.Status = ActiveStatus.ACTIVE;
        await _unitOfWork.SaveChangesAsync(ct);
        return grade;
    }

    public async Task<HrSalaryGrade?> GetNextActiveGradeAsync(int scaleId, int currentGradeNumber, CancellationToken ct = default)
    {
        var grades = await _repository.ListByScaleAsync(scaleId, ct);
        return grades
            .Where(g => g.GradeNumber > currentGradeNumber && g.Status == ActiveStatus.ACTIVE)
            .OrderBy(g => g.GradeNumber)
            .FirstOrDefault(); // BR-SAL-17: inactive grades skipped, first active above wins
    }

    public async Task<decimal> GetCurrentCoefficientAsync(int gradeId, CancellationToken ct = default)
    {
        var latest = await _repository.GetLatestCoefficientAsync(gradeId, ct);
        return latest?.Coefficient
            ?? throw new NotFoundException($"Salary grade {gradeId} has no recorded coefficient.");
    }

    private async Task<HrSalaryGrade> GetOrThrowAsync(int gradeId, CancellationToken ct) =>
        await _repository.GetByIdAsync(gradeId, ct)
            ?? throw new NotFoundException($"Salary grade {gradeId} was not found.");
}
