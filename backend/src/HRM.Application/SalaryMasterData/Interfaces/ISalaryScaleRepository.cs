using HRM.Domain.Entities;

namespace HRM.Application.SalaryMasterData.Interfaces;

public interface ISalaryScaleRepository
{
    Task<HrSalaryScale?> GetByIdAsync(int scaleId, CancellationToken ct = default);
    Task<HrSalaryScale?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<HrSalaryScale?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>Includes Grades, for Salary Scale Detail.</summary>
    Task<HrSalaryScale?> GetDetailAsync(int scaleId, CancellationToken ct = default);

    Task<IReadOnlyList<HrSalaryScale>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(HrSalaryScale scale, CancellationToken ct = default);
}
