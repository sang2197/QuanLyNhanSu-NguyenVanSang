using HRM.Domain.Entities;

namespace HRM.Application.SalaryMasterData.Interfaces;

public interface ISalaryScaleService
{
    Task<HrSalaryScale> CreateScaleAsync(string code, string name, CancellationToken ct = default);
    Task<IReadOnlyList<HrSalaryScale>> ListScalesAsync(CancellationToken ct = default);
    Task<HrSalaryScale> GetScaleDetailAsync(int scaleId, CancellationToken ct = default);

    /// <summary>Name only — the code is fixed at creation (BR-SAL-07).</summary>
    Task<HrSalaryScale> UpdateScaleAsync(int scaleId, string name, CancellationToken ct = default);

    Task<HrSalaryScale> DeactivateScaleAsync(int scaleId, CancellationToken ct = default);
    Task<HrSalaryScale> ReactivateScaleAsync(int scaleId, CancellationToken ct = default);
    Task<HrSalaryGrade> CreateGradeAsync(int scaleId, int gradeNumber, decimal coefficient, CancellationToken ct = default);

    /// <summary>Exposed cross-domain and for the sibling SalaryGradeService
    /// (BR-SAL-18, ADR-03).</summary>
    Task<bool> IsScaleActiveAsync(int scaleId, CancellationToken ct = default);
}
