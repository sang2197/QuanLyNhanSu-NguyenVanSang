using HRM.Application.Common;
using HRM.Application.Exceptions;
using HRM.Application.SalaryMasterData.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.SalaryMasterData.Services;

/// <inheritdoc cref="ISalaryScaleService"/>
public class SalaryScaleService : ISalaryScaleService
{
    private readonly ISalaryScaleRepository _scaleRepository;
    private readonly ISalaryGradeRepository _gradeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SalaryScaleService(
        ISalaryScaleRepository scaleRepository,
        ISalaryGradeRepository gradeRepository,
        IUnitOfWork unitOfWork)
    {
        _scaleRepository = scaleRepository;
        _gradeRepository = gradeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<HrSalaryScale> CreateScaleAsync(string code, string name, CancellationToken ct = default)
    {
        if (await _scaleRepository.GetByCodeAsync(code, ct) is not null)
        {
            throw new ConflictException("A salary scale with this code already exists."); // BR-SAL-05
        }
        if (await _scaleRepository.GetByNameAsync(name, ct) is not null)
        {
            throw new ConflictException("A salary scale with this name already exists."); // BR-SAL-06
        }

        var scale = new HrSalaryScale { Code = code, Name = name, Status = ActiveStatus.ACTIVE }; // BR-SAL-05A
        await _scaleRepository.AddAsync(scale, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return scale;
    }

    public async Task<IReadOnlyList<HrSalaryScale>> ListScalesAsync(CancellationToken ct = default) =>
        await _scaleRepository.GetAllAsync(ct);

    public async Task<HrSalaryScale> GetScaleDetailAsync(int scaleId, CancellationToken ct = default) =>
        await _scaleRepository.GetDetailAsync(scaleId, ct)
            ?? throw new NotFoundException($"Salary scale {scaleId} was not found.");

    public async Task<HrSalaryScale> UpdateScaleAsync(int scaleId, string name, CancellationToken ct = default)
    {
        var scale = await GetOrThrowAsync(scaleId, ct);

        if (name != scale.Name)
        {
            if (await _scaleRepository.GetByNameAsync(name, ct) is not null)
            {
                throw new ConflictException("A salary scale with this name already exists."); // BR-SAL-06
            }
            scale.Name = name;
        }

        // Code is fixed at creation (BR-SAL-07) — not editable here.
        await _unitOfWork.SaveChangesAsync(ct);
        return scale;
    }

    public async Task<HrSalaryScale> DeactivateScaleAsync(int scaleId, CancellationToken ct = default)
    {
        var scale = await GetOrThrowAsync(scaleId, ct);

        if (await _gradeRepository.CountActiveByScaleAsync(scaleId, ct) > 0)
        {
            throw new ConflictException("Salary scale still has active salary grades."); // BR-SAL-20
        }

        scale.Status = ActiveStatus.INACTIVE;
        await _unitOfWork.SaveChangesAsync(ct);
        return scale;
    }

    public async Task<HrSalaryScale> ReactivateScaleAsync(int scaleId, CancellationToken ct = default)
    {
        // No guard (BR-SAL-22).
        var scale = await GetOrThrowAsync(scaleId, ct);
        scale.Status = ActiveStatus.ACTIVE;
        await _unitOfWork.SaveChangesAsync(ct);
        return scale;
    }

    public async Task<HrSalaryGrade> CreateGradeAsync(int scaleId, int gradeNumber, decimal coefficient, CancellationToken ct = default)
    {
        var scale = await GetOrThrowAsync(scaleId, ct);
        if (scale.Status != ActiveStatus.ACTIVE)
        {
            throw new ConflictException("Salary scale is inactive."); // BR-SAL-11
        }
        if (coefficient <= 0)
        {
            throw new ValidationException("Coefficient must be greater than zero."); // BR-SAL-10
        }
        if (await _gradeRepository.GetByScaleAndNumberAsync(scaleId, gradeNumber, ct) is not null)
        {
            throw new ConflictException("Grade number already in use within this scale."); // BR-SAL-09
        }

        var grade = new HrSalaryGrade { SalaryScaleId = scaleId, GradeNumber = gradeNumber, Status = ActiveStatus.ACTIVE }; // BR-SAL-09A
        await _gradeRepository.AddAsync(grade, ct);

        // Initial coefficient needs no separate effective date (BR-SAL-12) —
        // set via the SalaryGrade navigation so EF resolves the FK on the
        // same SaveChanges as the grade itself, in one atomic call.
        var initialCoefficient = new HrSalaryGradeCoefficient
        {
            SalaryGrade = grade,
            Coefficient = coefficient,
            EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        await _gradeRepository.AddCoefficientAsync(initialCoefficient, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        return grade;
    }

    public async Task<bool> IsScaleActiveAsync(int scaleId, CancellationToken ct = default)
    {
        var scale = await _scaleRepository.GetByIdAsync(scaleId, ct);
        return scale is not null && scale.Status == ActiveStatus.ACTIVE;
    }

    private async Task<HrSalaryScale> GetOrThrowAsync(int scaleId, CancellationToken ct) =>
        await _scaleRepository.GetByIdAsync(scaleId, ct)
            ?? throw new NotFoundException($"Salary scale {scaleId} was not found.");
}
