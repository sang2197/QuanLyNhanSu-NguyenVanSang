using HRM.Application.Common;
using HRM.Application.EmployeeManagement.Interfaces;
using HRM.Application.EmployeeManagement.Models;
using HRM.Application.Exceptions;
using HRM.Application.OrganizationManagement.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.EmployeeManagement.Services;

/// <inheritdoc cref="IEmployeeService"/>
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrganizationalUnitService _organizationalUnitService;
    private readonly IJobTitleService _jobTitleService;

    public EmployeeService(
        IEmployeeRepository repository,
        IUnitOfWork unitOfWork,
        IOrganizationalUnitService organizationalUnitService,
        IJobTitleService jobTitleService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _organizationalUnitService = organizationalUnitService;
        _jobTitleService = jobTitleService;
    }

    public async Task<HrEmployee> CreateEmployeeAsync(CreateEmployeeInput input, CancellationToken ct = default)
    {
        if (await _repository.GetByCodeAsync(input.EmployeeCode, ct) is not null)
        {
            throw new ConflictException("Employee code already in use."); // BR-EMP-01
        }
        if (!await _organizationalUnitService.IsUnitActiveAsync(input.OrganizationalUnitId, ct))
        {
            throw new ValidationException("Organizational unit is inactive."); // BR-EMP-04
        }
        if (!await _jobTitleService.IsJobTitleActiveAsync(input.JobTitleId, ct))
        {
            throw new ValidationException("Job title is inactive."); // BR-EMP-05
        }

        var employee = new HrEmployee
        {
            EmployeeCode = input.EmployeeCode,
            FullName = input.FullName,
            OrganizationalUnitId = input.OrganizationalUnitId,
            JobTitleId = input.JobTitleId,
            JoinDate = input.JoinDate,
            EmploymentStatus = input.EmploymentStatus
        };
        await _repository.AddAsync(employee, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return employee;
    }

    // HRM.Application has no EF Core reference, so IQueryable<T> from the
    // repository is composed and materialized here with plain synchronous
    // LINQ (not ToListAsync/CountAsync — those are EF-specific extensions
    // this project can't reference, and unit tests mock Query() with a
    // LINQ-to-Objects IQueryable that doesn't support them anyway).
    public Task<PagedResult<HrEmployee>> SearchEmployeesAsync(
        string? search, int? organizationalUnitId, int? jobTitleId, EmploymentStatus? employmentStatus,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _repository.Query();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.EmployeeCode.Contains(search) || e.FullName.Contains(search)); // BR-EMP-06/07
        }
        if (organizationalUnitId is int unitId)
        {
            query = query.Where(e => e.OrganizationalUnitId == unitId);
        }
        if (jobTitleId is int titleId)
        {
            query = query.Where(e => e.JobTitleId == titleId);
        }
        if (employmentStatus is EmploymentStatus status)
        {
            query = query.Where(e => e.EmploymentStatus == status);
        }

        var totalItems = query.Count();
        var items = query
            .OrderBy(e => e.EmployeeCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<HrEmployee> { Items = items, Page = page, PageSize = pageSize, TotalItems = totalItems });
    }

    public async Task<HrEmployee> GetEmployeeAsync(int employeeId, CancellationToken ct = default) =>
        await GetOrThrowAsync(employeeId, ct);

    public async Task<HrEmployee> UpdateEmployeeAsync(int employeeId, UpdateEmployeeInput input, CancellationToken ct = default)
    {
        var employee = await GetOrThrowAsync(employeeId, ct);

        if (input.EmployeeCode is not null && input.EmployeeCode != employee.EmployeeCode)
        {
            if (await _repository.GetByCodeAsync(input.EmployeeCode, ct) is not null)
            {
                throw new ConflictException("Employee code already in use."); // BR-EMP-08
            }
            employee.EmployeeCode = input.EmployeeCode;
        }

        if (input.OrganizationalUnitId is int unitId && unitId != employee.OrganizationalUnitId)
        {
            if (!await _organizationalUnitService.IsUnitActiveAsync(unitId, ct))
            {
                throw new ValidationException("Organizational unit is inactive."); // BR-EMP-04
            }
            employee.OrganizationalUnitId = unitId;
        }

        if (input.JobTitleId is int titleId && titleId != employee.JobTitleId)
        {
            if (!await _jobTitleService.IsJobTitleActiveAsync(titleId, ct))
            {
                throw new ValidationException("Job title is inactive."); // BR-EMP-05
            }
            employee.JobTitleId = titleId;
        }

        if (input.FullName is not null)
        {
            employee.FullName = input.FullName;
        }
        if (input.JoinDate is DateOnly joinDate)
        {
            employee.JoinDate = joinDate;
        }

        // Updating an employee profile modifies the existing profile and
        // does not create a new one (BR-EMP-09) — we mutate the tracked
        // entity above rather than constructing a replacement.
        await _unitOfWork.SaveChangesAsync(ct);
        return employee;
    }

    public async Task<HrEmployee> ChangeEmploymentStatusAsync(int employeeId, EmploymentStatus newStatus, CancellationToken ct = default)
    {
        var employee = await GetOrThrowAsync(employeeId, ct);

        if (employee.EmploymentStatus == EmploymentStatus.TERMINATED)
        {
            throw new ConflictException("Transition from Terminated is unresolved — OQ-EMP-01."); // StateDiagrams.md §4
        }

        // BR-EMP-11: the profile and its existing information are retained —
        // only the status field changes.
        employee.EmploymentStatus = newStatus;
        await _unitOfWork.SaveChangesAsync(ct);
        return employee;
    }

    public async Task<bool> HasActiveEmployeesInUnitAsync(int organizationalUnitId, CancellationToken ct = default) =>
        await _repository.HasActiveInUnitAsync(organizationalUnitId, ct);

    public Task<IReadOnlyList<HrEmployee>> GetActiveEmployeesAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<HrEmployee>>(_repository.QueryActive().ToList());

    private async Task<HrEmployee> GetOrThrowAsync(int employeeId, CancellationToken ct) =>
        await _repository.GetByIdAsync(employeeId, ct)
            ?? throw new NotFoundException($"Employee {employeeId} was not found.");
}
