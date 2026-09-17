using HRM.Application.Common;
using HRM.Application.Exceptions;
using HRM.Application.SalaryManagement.Interfaces;
using HRM.Application.SalaryManagement.Models;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.SalaryManagement.Services;

/// <inheritdoc cref="ISalaryDecisionService"/>
public class SalaryDecisionService : ISalaryDecisionService
{
    private readonly ISalaryRepository _salaryRepository;

    public SalaryDecisionService(ISalaryRepository salaryRepository)
    {
        _salaryRepository = salaryRepository;
    }

    // US-06 — employees are selected up front, at creation; only employees
    // approved in the given (submitted) review period can be included.
    public async Task<HrSalaryDecision> CreateDecisionAsync(CreateDecisionInput input, CancellationToken ct = default)
    {
        var period = await _salaryRepository.GetReviewPeriodAsync(input.ReviewPeriodId, ct)
            ?? throw new NotFoundException($"Review period {input.ReviewPeriodId} not found.");

        if (period.Status != ReviewPeriodStatus.SUBMITTED)
        {
            throw new ConflictException("A decision can only be drafted from a review period that has been submitted.");
        }

        if (await _salaryRepository.HasNonCancelledDecisionAsync(input.ReviewPeriodId, ct))
        {
            throw new ConflictException("This review period already has a non-cancelled decision drafted from it.");
        }

        // US-SGP-06 AC06 — the decision cannot take effect before the review
        // that produced it.
        if (input.EffectiveDate < period.ReviewDate)
        {
            throw new ValidationException(
                $"Effective date {input.EffectiveDate} cannot be earlier than the review period's review date {period.ReviewDate}.");
        }

        var details = new List<HrSalaryDecisionDetail>();
        var notApproved = new List<int>();
        foreach (var employeeId in input.EmployeeIds)
        {
            var entry = await _salaryRepository.GetReviewEmployeeAsync(input.ReviewPeriodId, employeeId, ct);
            if (entry is null || entry.ReviewStatus != ReviewOutcome.APPROVED)
            {
                notApproved.Add(employeeId);
                continue;
            }

            details.Add(new HrSalaryDecisionDetail
            {
                EmployeeId = employeeId,
                OldSalaryId = entry.CurrentSalaryId,
                OldGradeId = entry.CurrentGradeId,
                OldCoefficient = entry.CurrentCoefficient,
                NewSalaryGradeId = entry.ProposedGradeId!.Value,
                NewCoefficient = entry.ProposedCoefficient!.Value,
                EffectiveFrom = input.EffectiveDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        if (notApproved.Count > 0)
        {
            throw new ValidationException(
                $"Employee(s) not approved in review period {input.ReviewPeriodId}: {string.Join(", ", notApproved)}.");
        }

        var decision = new HrSalaryDecision
        {
            ReviewPeriodId = input.ReviewPeriodId,
            DecisionNumber = input.DecisionNumber,
            DecisionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EffectiveDate = input.EffectiveDate,
            DecisionType = input.DecisionType,
            Status = SalaryDecisionStatus.DRAFT,
            SignerEmployeeId = input.SignerEmployeeId,
            Description = input.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Details = details
        };

        await _salaryRepository.AddDecisionAsync(decision, ct);
        await _salaryRepository.SaveChangesAsync(ct);
        return decision;
    }

    public Task<PagedResult<HrSalaryDecision>> SearchDecisionsAsync(
        SalaryDecisionStatus? status, int? reviewPeriodId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _salaryRepository.QueryDecisions();

        if (status is not null) query = query.Where(d => d.Status == status);
        if (reviewPeriodId is not null) query = query.Where(d => d.ReviewPeriodId == reviewPeriodId);

        var totalItems = query.Count();
        var items = query
            .OrderByDescending(d => d.DecisionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<HrSalaryDecision>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        });
    }

    public async Task<HrSalaryDecision> GetDecisionAsync(int decisionId, CancellationToken ct = default)
    {
        return await _salaryRepository.GetDecisionAsync(decisionId, ct)
            ?? throw new NotFoundException($"Salary decision {decisionId} not found.");
    }

    // US-06 — only allowed while the decision is still a draft.
    public async Task RemoveEmployeeAsync(int decisionId, int employeeId, CancellationToken ct = default)
    {
        var decision = await _salaryRepository.GetDecisionAsync(decisionId, ct)
            ?? throw new NotFoundException($"Salary decision {decisionId} not found.");

        if (decision.Status != SalaryDecisionStatus.DRAFT)
        {
            throw new ConflictException("Employees can only be removed while the decision is still a draft.");
        }

        var detail = decision.Details.FirstOrDefault(d => d.EmployeeId == employeeId)
            ?? throw new NotFoundException($"Employee {employeeId} is not part of decision {decisionId}.");

        await _salaryRepository.RemoveDecisionDetailAsync(detail, ct);
        await _salaryRepository.SaveChangesAsync(ct);
    }

    // US-07 — all-or-nothing: every employee's effective-date conflict is
    // checked BEFORE any write happens, so a conflict on one blocks all.
    public async Task<HrSalaryDecision> ApplyDecisionAsync(int decisionId, CancellationToken ct = default)
    {
        var decision = await _salaryRepository.GetDecisionAsync(decisionId, ct)
            ?? throw new NotFoundException($"Salary decision {decisionId} not found.");

        if (decision.Status != SalaryDecisionStatus.DRAFT)
        {
            throw new ConflictException($"Decision is already {decision.Status} — it can only be applied once, from Draft.");
        }

        var conflicts = new List<int>();
        var currentSalaries = new Dictionary<int, HrEmployeeSalary?>();
        foreach (var detail in decision.Details)
        {
            var currentSalary = await _salaryRepository.GetCurrentSalaryAsync(detail.EmployeeId, ct);
            currentSalaries[detail.EmployeeId] = currentSalary;

            // US-SGP-07 AC04 — the employee's grade may have moved since the
            // decision was drafted (e.g. via another applied decision); the
            // snapshot this decision was built from is no longer valid.
            if (currentSalary?.SalaryGradeId != detail.OldGradeId)
            {
                conflicts.Add(detail.EmployeeId);
                continue;
            }

            if (await _salaryRepository.HasEffectiveDateConflictAsync(detail.EmployeeId, detail.EffectiveFrom, ct))
            {
                conflicts.Add(detail.EmployeeId);
            }
        }

        if (conflicts.Count > 0)
        {
            throw new ConflictException(
                $"Employee(s) have a conflicting salary record — no employee in this decision was updated: {string.Join(", ", conflicts)}.");
        }

        foreach (var detail in decision.Details)
        {
            var currentSalary = currentSalaries[detail.EmployeeId];
            if (currentSalary is not null)
            {
                await _salaryRepository.CloseSalaryAsync(currentSalary, detail.EffectiveFrom.AddDays(-1), ct);
            }

            await _salaryRepository.AddSalaryAsync(new HrEmployeeSalary
            {
                EmployeeId = detail.EmployeeId,
                SalaryScaleId = currentSalary?.SalaryScaleId ?? detail.NewSalaryGrade.SalaryScaleId,
                SalaryGradeId = detail.NewSalaryGradeId,
                Coefficient = detail.NewCoefficient,
                EffectiveFrom = detail.EffectiveFrom,
                Reason = $"Salary decision {decision.DecisionNumber}",
                DecisionId = decision.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, ct);
        }

        decision.Status = SalaryDecisionStatus.APPLIED;
        decision.UpdatedAt = DateTime.UtcNow;

        // Terminal transition — CLOSED never reverts, since an Applied
        // decision can never be cancelled (see CancelDecisionAsync below).
        decision.ReviewPeriod.Status = ReviewPeriodStatus.CLOSED;
        decision.ReviewPeriod.UpdatedAt = DateTime.UtcNow;

        await _salaryRepository.SaveChangesAsync(ct);
        return decision;
    }

    // US-10 — only a Draft decision can be cancelled; an Applied decision is
    // permanent and can never be cancelled or edited.
    public async Task<HrSalaryDecision> CancelDecisionAsync(int decisionId, CancellationToken ct = default)
    {
        var decision = await _salaryRepository.GetDecisionAsync(decisionId, ct)
            ?? throw new NotFoundException($"Salary decision {decisionId} not found.");

        if (decision.Status != SalaryDecisionStatus.DRAFT)
        {
            throw new ConflictException($"Only a Draft decision can be cancelled — this decision is {decision.Status}.");
        }

        decision.Status = SalaryDecisionStatus.CANCELLED;
        decision.UpdatedAt = DateTime.UtcNow;
        await _salaryRepository.SaveChangesAsync(ct);
        return decision;
    }
}
