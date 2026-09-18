using HRM.Domain.Entities;

namespace HRM.Application.SalaryMasterData.Interfaces;

public interface ISalaryGradeService
{
    Task<HrSalaryGradeCoefficient> AddCoefficientAsync(int gradeId, decimal coefficient, DateOnly effectiveDate, CancellationToken ct = default);
    Task<IReadOnlyList<HrSalaryGradeCoefficient>> ListCoefficientsAsync(int gradeId, CancellationToken ct = default);
    Task<HrSalaryGrade> DeactivateGradeAsync(int gradeId, CancellationToken ct = default);
    Task<HrSalaryGrade> ReactivateGradeAsync(int gradeId, CancellationToken ct = default);

    // ---- Exposed cross-domain (ADR-03) ----

    /// <summary>First active grade above <paramref name="currentGradeNumber"/>
    /// in ascending order, inactive grades skipped (BR-SAL-17); null if none
    /// exists. Used by Salary Grade Promotion (US-SGP-03).</summary>
    Task<HrSalaryGrade?> GetNextActiveGradeAsync(int scaleId, int currentGradeNumber, CancellationToken ct = default);

    /// <summary>The coefficient with the latest EffectiveDate on or before
    /// today. Used by Salary Grade Promotion (US-SGP-03).</summary>
    Task<decimal> GetCurrentCoefficientAsync(int gradeId, CancellationToken ct = default);
}
