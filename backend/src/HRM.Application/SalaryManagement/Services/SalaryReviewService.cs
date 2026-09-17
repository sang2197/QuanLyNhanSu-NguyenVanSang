using HRM.Application.Common;
using HRM.Application.Exceptions;
using HRM.Application.SalaryManagement.Interfaces;
using HRM.Application.SalaryManagement.Models;
using HRM.Application.SalaryManagement.Rules;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.SalaryManagement.Services;

/// <inheritdoc cref="ISalaryReviewService"/>
public class SalaryReviewService : ISalaryReviewService
{
    private readonly ISalaryRepository _salaryRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEligibilityRule _eligibilityRule;

    public SalaryReviewService(
        ISalaryRepository salaryRepository,
        IEmployeeRepository employeeRepository,
        IEligibilityRule eligibilityRule)
    {
        _salaryRepository = salaryRepository;
        _employeeRepository = employeeRepository;
        _eligibilityRule = eligibilityRule;
    }

    // US-01 — creating a period immediately calculates the proposed grade
    // for every eligible employee, synchronously, in this same call.
    public async Task<HrSalaryReviewPeriod> CreateReviewPeriodAsync(CreateReviewPeriodInput input, CancellationToken ct = default)
    {
        var existingCode = await _salaryRepository.GetReviewPeriodByCodeAsync(input.Code, ct);
        if (existingCode is not null)
        {
            throw new ConflictException($"A review period with code '{input.Code}' already exists.");
        }

        // US-SGP-01 AC03 — code and name are each independently unique.
        var existingName = _salaryRepository.QueryReviewPeriods().Any(p => p.Name == input.Name);
        if (existingName)
        {
            throw new ConflictException($"A review period with name '{input.Name}' already exists.");
        }

        var period = new HrSalaryReviewPeriod
        {
            Code = input.Code,
            Name = input.Name,
            ReviewType = input.ReviewType,
            ReviewDate = input.ReviewDate,
            EffectiveDate = input.EffectiveDate,
            Description = input.Description,
            Status = ReviewPeriodStatus.IN_PROGRESS,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var reviewEmployees = new List<HrSalaryReviewEmployee>();
        var gradesByScale = new Dictionary<int, IReadOnlyList<HrSalaryGrade>>();

        var employees = _employeeRepository.QueryActive().ToList();
        foreach (var employee in employees)
        {
            var currentSalary = await _salaryRepository.GetCurrentSalaryAsync(employee.Id, ct);
            if (currentSalary is null)
            {
                // No current salary on record — cannot be screened; not part
                // of the documented eligibility rule, just a data-integrity guard.
                continue;
            }

            if (!gradesByScale.TryGetValue(currentSalary.SalaryScaleId, out var grades))
            {
                grades = await _salaryRepository.GetGradesForScaleAsync(currentSalary.SalaryScaleId, ct);
                gradesByScale[currentSalary.SalaryScaleId] = grades;
            }

            var result = _eligibilityRule.Evaluate(
                currentSalary.SalaryGrade,
                currentSalary.EffectiveFrom,
                grades,
                input.ReviewDate,
                alreadyHasProposalThisPeriod: false); // brand-new period — never a duplicate yet

            reviewEmployees.Add(new HrSalaryReviewEmployee
            {
                ReviewPeriod = period, // FK fixup — period.Id isn't assigned until SaveChanges
                EmployeeId = employee.Id,
                CurrentSalaryId = currentSalary.Id,
                CurrentGradeId = currentSalary.SalaryGradeId,
                ProposedGradeId = result.ProposedGrade?.Id,
                CurrentCoefficient = currentSalary.Coefficient,
                ProposedCoefficient = result.ProposedGrade?.Coefficient,
                EligibilityStatus = result.IsEligible ? EligibilityStatus.ELIGIBLE : EligibilityStatus.INELIGIBLE,
                EligibilityReason = result.Reason,
                ReviewStatus = ReviewOutcome.PENDING,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _salaryRepository.AddReviewPeriodAsync(period, ct);
        await _salaryRepository.AddReviewEmployeesAsync(reviewEmployees, ct);
        await _salaryRepository.SaveChangesAsync(ct);

        return period;
    }

    public Task<PagedResult<HrSalaryReviewPeriod>> SearchReviewPeriodsAsync(
        DateOnly? fromDate, DateOnly? toDate, ReviewType? reviewType, ReviewPeriodStatus? status,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _salaryRepository.QueryReviewPeriods();

        if (fromDate is not null) query = query.Where(p => p.ReviewDate >= fromDate);
        if (toDate is not null) query = query.Where(p => p.ReviewDate <= toDate);
        if (reviewType is not null) query = query.Where(p => p.ReviewType == reviewType);
        if (status is not null) query = query.Where(p => p.Status == status);

        var totalItems = query.Count();
        var items = query
            .OrderByDescending(p => p.ReviewDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<HrSalaryReviewPeriod> { Items = items, Page = page, PageSize = pageSize, TotalItems = totalItems });
    }

    public async Task<ReviewPeriodDetailResult> GetReviewPeriodDetailAsync(int periodId, CancellationToken ct = default)
    {
        var period = await _salaryRepository.GetReviewPeriodAsync(periodId, ct)
            ?? throw new NotFoundException($"Review period {periodId} not found.");

        var entries = _salaryRepository.QueryReviewEmployees(periodId).ToList();
        var decision = _salaryRepository.QueryDecisions()
            .FirstOrDefault(d => d.ReviewPeriodId == periodId && d.Status != SalaryDecisionStatus.CANCELLED);

        return new ReviewPeriodDetailResult(
            Period: period,
            TotalEmployees: entries.Count,
            EligibleCount: entries.Count(e => e.EligibilityStatus == EligibilityStatus.ELIGIBLE),
            ApprovedCount: entries.Count(e => e.ReviewStatus == ReviewOutcome.APPROVED),
            RejectedCount: entries.Count(e => e.ReviewStatus == ReviewOutcome.REJECTED),
            // Only an eligible employee ever has something to act on — an
            // ineligible one has no proposal and stays PENDING forever, so
            // it must never be counted as "still pending" (US-05).
            PendingCount: entries.Count(e => e.EligibilityStatus == EligibilityStatus.ELIGIBLE && e.ReviewStatus == ReviewOutcome.PENDING),
            DecisionId: decision?.Id);
    }

    // US-05 — blocked while any ELIGIBLE employee is still unprocessed. An
    // ineligible employee has no proposal and can never be approved/rejected
    // (see EnsureApprovableOrRejectable), so it must be excluded here —
    // otherwise a period containing even one ineligible employee could never
    // be submitted at all.
    public async Task<HrSalaryReviewPeriod> SubmitReviewPeriodAsync(int periodId, CancellationToken ct = default)
    {
        var period = await _salaryRepository.GetReviewPeriodAsync(periodId, ct)
            ?? throw new NotFoundException($"Review period {periodId} not found.");

        if (period.Status != ReviewPeriodStatus.IN_PROGRESS)
        {
            throw new ConflictException("Only a review period that is IN_PROGRESS can be submitted.");
        }

        var unprocessedCount = _salaryRepository.QueryReviewEmployees(periodId)
            .Count(e => e.EligibilityStatus == EligibilityStatus.ELIGIBLE && e.ReviewStatus == ReviewOutcome.PENDING);
        if (unprocessedCount > 0)
        {
            throw new ConflictException($"{unprocessedCount} eligible employee(s) in this period are still unprocessed.");
        }

        period.Status = ReviewPeriodStatus.SUBMITTED;
        period.UpdatedAt = DateTime.UtcNow;
        await _salaryRepository.SaveChangesAsync(ct);
        return period;
    }

    // US-11 — blocked once Closed/Cancelled, or once a decision already exists.
    public async Task<HrSalaryReviewPeriod> CancelReviewPeriodAsync(int periodId, CancellationToken ct = default)
    {
        var period = await _salaryRepository.GetReviewPeriodAsync(periodId, ct)
            ?? throw new NotFoundException($"Review period {periodId} not found.");

        if (period.Status is ReviewPeriodStatus.CLOSED or ReviewPeriodStatus.CANCELLED)
        {
            throw new ConflictException($"Review period is already {period.Status}.");
        }

        if (await _salaryRepository.HasNonCancelledDecisionAsync(periodId, ct))
        {
            throw new ConflictException("This period already has a decision drafted from it — cancel the decision first (US-10).");
        }

        period.Status = ReviewPeriodStatus.CANCELLED;
        period.UpdatedAt = DateTime.UtcNow;
        await _salaryRepository.SaveChangesAsync(ct);
        return period;
    }

    public Task<PagedResult<HrSalaryReviewEmployee>> SearchReviewEmployeesAsync(
        int periodId, string? department, EligibilityStatus? eligibility, ReviewOutcome? outcome,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _salaryRepository.QueryReviewEmployees(periodId);

        // Employee Management (Department master data) is out of scope for this
        // feature (see API/README.md "Out of scope") — HrEmployee only has a
        // DepartmentId, no name to match against, so filtering only works when
        // the caller passes the id itself.
        if (!string.IsNullOrWhiteSpace(department) && int.TryParse(department, out var departmentId))
        {
            query = query.Where(e => e.Employee.DepartmentId == departmentId);
        }
        if (eligibility is not null) query = query.Where(e => e.EligibilityStatus == eligibility);
        if (outcome is not null) query = query.Where(e => e.ReviewStatus == outcome);

        var totalItems = query.Count();
        var items = query
            .OrderBy(e => e.EmployeeId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<HrSalaryReviewEmployee>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        });
    }

    public async Task<HrSalaryReviewEmployee> GetReviewEmployeeAsync(int periodId, int employeeId, CancellationToken ct = default)
    {
        return await _salaryRepository.GetReviewEmployeeAsync(periodId, employeeId, ct)
            ?? throw new NotFoundException($"Employee {employeeId} not found in review period {periodId}.");
    }

    public async Task<HrSalaryReviewEmployee> ApproveEmployeeAsync(int periodId, int employeeId, CancellationToken ct = default)
    {
        await EnsurePeriodInProgressAsync(periodId, ct);

        var entry = await _salaryRepository.GetReviewEmployeeAsync(periodId, employeeId, ct)
            ?? throw new NotFoundException($"Employee {employeeId} not found in review period {periodId}.");

        EnsureApprovableOrRejectable(entry);

        entry.ReviewStatus = ReviewOutcome.APPROVED;
        entry.ApprovedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        await _salaryRepository.SaveChangesAsync(ct);
        return entry;
    }

    public async Task<HrSalaryReviewEmployee> RejectEmployeeAsync(int periodId, int employeeId, string reason, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ValidationException("reason is required when rejecting an employee");
        }

        await EnsurePeriodInProgressAsync(periodId, ct);

        var entry = await _salaryRepository.GetReviewEmployeeAsync(periodId, employeeId, ct)
            ?? throw new NotFoundException($"Employee {employeeId} not found in review period {periodId}.");

        EnsureApprovableOrRejectable(entry);

        entry.ReviewStatus = ReviewOutcome.REJECTED;
        entry.Reason = reason;
        entry.ApprovedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;
        await _salaryRepository.SaveChangesAsync(ct);
        return entry;
    }

    public async Task<BulkActionResult> BulkApproveAsync(int periodId, IReadOnlyList<int> employeeIds, CancellationToken ct = default)
    {
        await EnsurePeriodInProgressAsync(periodId, ct);

        var result = new BulkActionResult();
        foreach (var employeeId in employeeIds)
        {
            var entry = await _salaryRepository.GetReviewEmployeeAsync(periodId, employeeId, ct);
            if (entry is null)
            {
                result.Failed.Add(new BulkActionFailure(employeeId, "Employee not found in this review period."));
                continue;
            }
            if (entry.EligibilityStatus != EligibilityStatus.ELIGIBLE || entry.ReviewStatus != ReviewOutcome.PENDING)
            {
                result.Failed.Add(new BulkActionFailure(employeeId, "Employee is not eligible, or already has an outcome."));
                continue;
            }

            entry.ReviewStatus = ReviewOutcome.APPROVED;
            entry.ApprovedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            result.SucceededEmployeeIds.Add(employeeId);
        }

        await _salaryRepository.SaveChangesAsync(ct);
        return result;
    }

    public async Task<BulkActionResult> BulkRejectAsync(int periodId, IReadOnlyList<int> employeeIds, string reason, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ValidationException("reason is required when rejecting an employee");
        }

        await EnsurePeriodInProgressAsync(periodId, ct);

        var result = new BulkActionResult();
        foreach (var employeeId in employeeIds)
        {
            var entry = await _salaryRepository.GetReviewEmployeeAsync(periodId, employeeId, ct);
            if (entry is null)
            {
                result.Failed.Add(new BulkActionFailure(employeeId, "Employee not found in this review period."));
                continue;
            }
            if (entry.EligibilityStatus != EligibilityStatus.ELIGIBLE || entry.ReviewStatus != ReviewOutcome.PENDING)
            {
                result.Failed.Add(new BulkActionFailure(employeeId, "Employee is not eligible, or already has an outcome."));
                continue;
            }

            entry.ReviewStatus = ReviewOutcome.REJECTED;
            entry.Reason = reason;
            entry.ApprovedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            result.SucceededEmployeeIds.Add(employeeId);
        }

        await _salaryRepository.SaveChangesAsync(ct);
        return result;
    }

    // US-05 — an employee's outcome can only change while the period is IN_PROGRESS.
    private async Task EnsurePeriodInProgressAsync(int periodId, CancellationToken ct)
    {
        var period = await _salaryRepository.GetReviewPeriodAsync(periodId, ct)
            ?? throw new NotFoundException($"Review period {periodId} not found.");

        if (period.Status != ReviewPeriodStatus.IN_PROGRESS)
        {
            throw new ConflictException("This review period is no longer IN_PROGRESS — outcomes cannot change after submission.");
        }
    }

    private static void EnsureApprovableOrRejectable(HrSalaryReviewEmployee entry)
    {
        if (entry.EligibilityStatus != EligibilityStatus.ELIGIBLE || entry.ReviewStatus != ReviewOutcome.PENDING)
        {
            throw new ConflictException("Employee is not eligible, or already has an outcome.");
        }
    }
}
