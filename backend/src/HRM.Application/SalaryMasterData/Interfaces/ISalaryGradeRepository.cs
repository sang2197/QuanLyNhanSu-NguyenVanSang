using HRM.Domain.Entities;

namespace HRM.Application.SalaryMasterData.Interfaces;

public interface ISalaryGradeRepository
{
    Task<HrSalaryGrade?> GetByIdAsync(int gradeId, CancellationToken ct = default);
    Task<HrSalaryGrade?> GetByScaleAndNumberAsync(int scaleId, int gradeNumber, CancellationToken ct = default);
    Task<IReadOnlyList<HrSalaryGrade>> ListByScaleAsync(int scaleId, CancellationToken ct = default);
    Task<int> CountActiveByScaleAsync(int scaleId, CancellationToken ct = default);
    Task AddAsync(HrSalaryGrade grade, CancellationToken ct = default);

    Task AddCoefficientAsync(HrSalaryGradeCoefficient coefficient, CancellationToken ct = default);
    Task<HrSalaryGradeCoefficient?> GetLatestCoefficientAsync(int gradeId, CancellationToken ct = default);
    Task<IReadOnlyList<HrSalaryGradeCoefficient>> ListCoefficientsAsync(int gradeId, CancellationToken ct = default);
}
