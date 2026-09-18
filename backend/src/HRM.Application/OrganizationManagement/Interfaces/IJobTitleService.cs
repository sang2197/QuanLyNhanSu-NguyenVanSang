using HRM.Domain.Entities;

namespace HRM.Application.OrganizationManagement.Interfaces;

public interface IJobTitleService
{
    Task<HrJobTitle> CreateJobTitleAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<HrJobTitle>> ListJobTitlesAsync(CancellationToken ct = default);
    Task<HrJobTitle> UpdateJobTitleAsync(int jobTitleId, string name, CancellationToken ct = default);

    /// <summary>No guard — existing holders are unaffected (BR-ORG-19).</summary>
    Task<HrJobTitle> DeactivateJobTitleAsync(int jobTitleId, CancellationToken ct = default);

    /// <summary>No guard (BR-ORG-20).</summary>
    Task<HrJobTitle> ReactivateJobTitleAsync(int jobTitleId, CancellationToken ct = default);

    /// <summary>Exposed cross-domain for Employee Management (BR-EMP-05, ADR-03).</summary>
    Task<bool> IsJobTitleActiveAsync(int jobTitleId, CancellationToken ct = default);
}
