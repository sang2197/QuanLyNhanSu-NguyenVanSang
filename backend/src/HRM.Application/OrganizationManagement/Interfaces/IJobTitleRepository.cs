using HRM.Domain.Entities;

namespace HRM.Application.OrganizationManagement.Interfaces;

public interface IJobTitleRepository
{
    Task<HrJobTitle?> GetByIdAsync(int jobTitleId, CancellationToken ct = default);
    Task<HrJobTitle?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<HrJobTitle>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(HrJobTitle jobTitle, CancellationToken ct = default);
}
