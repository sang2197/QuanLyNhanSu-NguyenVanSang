using HRM.Application.Common;
using HRM.Application.Exceptions;
using HRM.Application.OrganizationManagement.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.OrganizationManagement.Services;

/// <inheritdoc cref="IJobTitleService"/>
public class JobTitleService : IJobTitleService
{
    private readonly IJobTitleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public JobTitleService(IJobTitleRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<HrJobTitle> CreateJobTitleAsync(string name, CancellationToken ct = default)
    {
        if (await _repository.GetByNameAsync(name, ct) is not null)
        {
            throw new ConflictException("A job title with this name already exists."); // BR-ORG-17
        }

        var jobTitle = new HrJobTitle { Name = name, Status = ActiveStatus.ACTIVE };
        await _repository.AddAsync(jobTitle, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return jobTitle;
    }

    public async Task<IReadOnlyList<HrJobTitle>> ListJobTitlesAsync(CancellationToken ct = default) =>
        await _repository.GetAllAsync(ct);

    public async Task<HrJobTitle> UpdateJobTitleAsync(int jobTitleId, string name, CancellationToken ct = default)
    {
        var jobTitle = await GetOrThrowAsync(jobTitleId, ct);

        if (name != jobTitle.Name)
        {
            if (await _repository.GetByNameAsync(name, ct) is not null)
            {
                throw new ConflictException("A job title with this name already exists."); // BR-ORG-17/18
            }
            jobTitle.Name = name;
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return jobTitle;
    }

    public async Task<HrJobTitle> DeactivateJobTitleAsync(int jobTitleId, CancellationToken ct = default)
    {
        // No guard — existing holders are unaffected (BR-ORG-19).
        var jobTitle = await GetOrThrowAsync(jobTitleId, ct);
        jobTitle.Status = ActiveStatus.INACTIVE;
        await _unitOfWork.SaveChangesAsync(ct);
        return jobTitle;
    }

    public async Task<HrJobTitle> ReactivateJobTitleAsync(int jobTitleId, CancellationToken ct = default)
    {
        // No guard (BR-ORG-20).
        var jobTitle = await GetOrThrowAsync(jobTitleId, ct);
        jobTitle.Status = ActiveStatus.ACTIVE;
        await _unitOfWork.SaveChangesAsync(ct);
        return jobTitle;
    }

    public async Task<bool> IsJobTitleActiveAsync(int jobTitleId, CancellationToken ct = default)
    {
        var jobTitle = await _repository.GetByIdAsync(jobTitleId, ct);
        return jobTitle is not null && jobTitle.Status == ActiveStatus.ACTIVE;
    }

    private async Task<HrJobTitle> GetOrThrowAsync(int jobTitleId, CancellationToken ct) =>
        await _repository.GetByIdAsync(jobTitleId, ct)
            ?? throw new NotFoundException($"Job title {jobTitleId} was not found.");
}
