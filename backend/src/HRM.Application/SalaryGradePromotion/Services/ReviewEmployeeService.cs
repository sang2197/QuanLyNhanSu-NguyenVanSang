using HRM.Application.Common;
using HRM.Application.Exceptions;
using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Application.SalaryGradePromotion.Models;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.SalaryGradePromotion.Services;

/// <inheritdoc cref="IReviewEmployeeService"/>
public class ReviewEmployeeService : IReviewEmployeeService
{
    private readonly IReviewEmployeeRepository _repository;
    private readonly IReviewPeriodRepository _periodRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewEmployeeService(IReviewEmployeeRepository repository, IReviewPeriodRepository periodRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<PagedResult<HrSalaryReviewEmployee>> ListReviewEmployeesAsync(
        int periodId, int? organizationalUnitId, bool? eligible, ReviewOutcome? outcome,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _repository.QueryByPeriod(periodId);

        if (organizationalUnitId is int unitId)
        {
            query = query.Where(e => e.Employee.OrganizationalUnitId == unitId); // US-SGP-03 AC06
        }
        if (eligible is bool isEligible)
        {
            query = query.Where(e => e.Eligible == isEligible);
        }
        if (outcome is ReviewOutcome o)
        {
            query = query.Where(e => e.Outcome == o);
        }

        var totalItems = query.Count();
        var items = query.OrderBy(e => e.EmployeeId).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PagedResult<HrSalaryReviewEmployee> { Items = items, Page = page, PageSize = pageSize, TotalItems = totalItems });
    }

    public async Task<HrSalaryReviewEmployee> GetReviewEmployeeAsync(int periodId, int employeeId, CancellationToken ct = default) =>
        await GetOrThrowAsync(periodId, employeeId, ct);

    public async Task<HrSalaryReviewEmployee> ApproveEmployeeAsync(int periodId, int employeeId, CancellationToken ct = default)
    {
        await EnsurePeriodInProgressAsync(periodId, ct); // US-SGP-04 AC10
        var entry = await GetOrThrowEligibleAsync(periodId, employeeId, ct);

        entry.Outcome = ReviewOutcome.APPROVED;
        entry.RejectionReason = null; // US-SGP-04 AC08
        await _unitOfWork.SaveChangesAsync(ct);
        return entry;
    }

    public async Task<HrSalaryReviewEmployee> RejectEmployeeAsync(int periodId, int employeeId, string reason, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ValidationException("Reason is required to reject a proposal."); // US-SGP-04 AC02/AC03
        }
        await EnsurePeriodInProgressAsync(periodId, ct);
        var entry = await GetOrThrowEligibleAsync(periodId, employeeId, ct);

        entry.Outcome = ReviewOutcome.REJECTED;
        entry.RejectionReason = reason;
        await _unitOfWork.SaveChangesAsync(ct);
        return entry;
    }

    public async Task<BulkActionResult> BulkApproveAsync(int periodId, IReadOnlyList<int> employeeIds, CancellationToken ct = default)
    {
        var result = new BulkActionResult();
        foreach (var employeeId in employeeIds)
        {
            try
            {
                await ApproveEmployeeAsync(periodId, employeeId, ct);
                result.SucceededEmployeeIds.Add(employeeId);
            }
            catch (Exception ex) when (ex is NotFoundException or ConflictException) // US-SGP-04 AC04/AC05
            {
                result.Failed.Add(new BulkActionFailure(employeeId, ex.Message));
            }
        }
        return result;
    }

    public async Task<BulkActionResult> BulkRejectAsync(int periodId, IReadOnlyList<int> employeeIds, string reason, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ValidationException("Reason is required to bulk reject."); // US-SGP-04 AC07
        }

        var result = new BulkActionResult();
        foreach (var employeeId in employeeIds)
        {
            try
            {
                await RejectEmployeeAsync(periodId, employeeId, reason, ct);
                result.SucceededEmployeeIds.Add(employeeId);
            }
            catch (Exception ex) when (ex is NotFoundException or ConflictException) // US-SGP-04 AC06/AC09
            {
                result.Failed.Add(new BulkActionFailure(employeeId, ex.Message));
            }
        }
        return result;
    }

    private async Task EnsurePeriodInProgressAsync(int periodId, CancellationToken ct)
    {
        var period = await _periodRepository.GetByIdAsync(periodId, ct)
            ?? throw new NotFoundException($"Review period {periodId} was not found.");
        if (period.Status != ReviewPeriodStatus.IN_PROGRESS)
        {
            throw new ConflictException("Review period is no longer in progress.");
        }
    }

    private async Task<HrSalaryReviewEmployee> GetOrThrowAsync(int periodId, int employeeId, CancellationToken ct) =>
        await _repository.GetByPeriodAndEmployeeAsync(periodId, employeeId, ct)
            ?? throw new NotFoundException($"Employee {employeeId} was not found in review period {periodId}.");

    private async Task<HrSalaryReviewEmployee> GetOrThrowEligibleAsync(int periodId, int employeeId, CancellationToken ct)
    {
        var entry = await GetOrThrowAsync(periodId, employeeId, ct);
        if (!entry.Eligible)
        {
            throw new ConflictException("Employee has no proposal to review.");
        }
        return entry;
    }
}
