using HRM.Application.Common;
using HRM.Application.Exceptions;
using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Application.SalaryGradePromotion.Models;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.SalaryGradePromotion.Services;

/// <inheritdoc cref="ISalaryDecisionService"/>
public class SalaryDecisionService : ISalaryDecisionService
{
    private readonly ISalaryDecisionRepository _decisionRepository;
    private readonly IReviewPeriodRepository _periodRepository;
    private readonly IReviewEmployeeRepository _reviewEmployeeRepository;
    private readonly IEmployeeSalaryRepository _employeeSalaryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SalaryDecisionService(
        ISalaryDecisionRepository decisionRepository,
        IReviewPeriodRepository periodRepository,
        IReviewEmployeeRepository reviewEmployeeRepository,
        IEmployeeSalaryRepository employeeSalaryRepository,
        IUnitOfWork unitOfWork)
    {
        _decisionRepository = decisionRepository;
        _periodRepository = periodRepository;
        _reviewEmployeeRepository = reviewEmployeeRepository;
        _employeeSalaryRepository = employeeSalaryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<HrSalaryDecision> CreateDecisionAsync(CreateSalaryDecisionInput input, CancellationToken ct = default)
    {
        var period = await _periodRepository.GetByIdAsync(input.ReviewPeriodId, ct)
            ?? throw new NotFoundException($"Review period {input.ReviewPeriodId} was not found.");

        if (period.Status != ReviewPeriodStatus.SUBMITTED)
        {
            throw new ConflictException("Review period is not Submitted."); // US-SGP-06 AC01
        }
        if (await _decisionRepository.GetNonCancelledByPeriodAsync(input.ReviewPeriodId, ct) is not null)
        {
            throw new ConflictException("Review period already has a non-cancelled decision."); // US-SGP-06 AC03
        }
        if (input.EffectiveDate < period.ReviewDate)
        {
            throw new ValidationException("Effective date must be on or after the review date."); // US-SGP-06 AC06
        }

        var approved = await _reviewEmployeeRepository.GetApprovedAsync(input.ReviewPeriodId, input.EmployeeIds, ct);
        if (approved.Count != input.EmployeeIds.Count)
        {
            throw new ValidationException("All included employees must have an Approved outcome in this review period."); // US-SGP-06 AC02
        }

        var decision = new HrSalaryDecision
        {
            ReviewPeriodId = input.ReviewPeriodId,
            DecisionNumber = await _decisionRepository.NextDecisionNumberAsync(ct),
            EffectiveDate = input.EffectiveDate,
            Status = SalaryDecisionStatus.DRAFT
        };
        await _decisionRepository.AddAsync(decision, ct);

        foreach (var entry in approved)
        {
            decision.Details.Add(new HrSalaryDecisionDetail
            {
                EmployeeId = entry.EmployeeId,
                BaselineSalaryGradeId = entry.CurrentSalaryGradeId,
                BaselineCoefficient = entry.CurrentCoefficient,
                NewSalaryGradeId = entry.ProposedSalaryGradeId!.Value,
                NewCoefficient = entry.ProposedCoefficient!.Value
            });
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return decision;
    }

    // HRM.Application has no EF Core reference — IQueryable<T> from the
    // repository is composed and materialized with plain synchronous LINQ.
    public Task<PagedResult<HrSalaryDecision>> SearchDecisionsAsync(
        SalaryDecisionStatus? status, int? reviewPeriodId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _decisionRepository.Query();
        if (status is SalaryDecisionStatus s)
        {
            query = query.Where(d => d.Status == s);
        }
        if (reviewPeriodId is int periodId)
        {
            query = query.Where(d => d.ReviewPeriodId == periodId);
        }

        var totalItems = query.Count();
        var items = query.OrderByDescending(d => d.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PagedResult<HrSalaryDecision> { Items = items, Page = page, PageSize = pageSize, TotalItems = totalItems });
    }

    public Task<IReadOnlyList<HrSalaryReviewPeriod>> GetEligibleReviewPeriodsAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<HrSalaryReviewPeriod>>(_periodRepository.QueryEligibleForDecision().ToList()); // US-SGP-09 AC04

    public async Task<HrSalaryDecision> GetDecisionAsync(int decisionId, CancellationToken ct = default) =>
        await GetOrThrowAsync(decisionId, ct);

    public async Task<HrSalaryDecision> SaveDraftAsync(int decisionId, DateOnly effectiveDate, CancellationToken ct = default)
    {
        var decision = await GetOrThrowAsync(decisionId, ct);
        if (decision.Status != SalaryDecisionStatus.DRAFT)
        {
            throw new ConflictException("Only a Draft decision can be edited.");
        }

        decision.EffectiveDate = effectiveDate;
        await _unitOfWork.SaveChangesAsync(ct);
        return decision;
    }

    public async Task RemoveEmployeeAsync(int decisionId, int employeeId, CancellationToken ct = default)
    {
        var decision = await GetOrThrowAsync(decisionId, ct);
        if (decision.Status != SalaryDecisionStatus.DRAFT)
        {
            throw new ConflictException("Only a Draft decision can be edited."); // US-SGP-06 AC04
        }

        var detail = decision.Details.FirstOrDefault(d => d.EmployeeId == employeeId)
            ?? throw new NotFoundException($"Employee {employeeId} is not included in decision {decisionId}.");

        await _decisionRepository.RemoveDetailAsync(detail, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>Sequence 4.6 — read-only validation pass first (no
    /// transaction needed for reads), then an all-or-nothing write phase.
    /// Owns a transaction because it writes across IEmployeeSalaryRepository,
    /// ISalaryDecisionRepository, and IReviewPeriodRepository.</summary>
    public async Task<HrSalaryDecision> ApplyDecisionAsync(int decisionId, CancellationToken ct = default)
    {
        var decision = await GetOrThrowAsync(decisionId, ct);
        if (decision.Status != SalaryDecisionStatus.DRAFT)
        {
            throw new ConflictException("Only a Draft decision can be applied.");
        }

        foreach (var detail in decision.Details)
        {
            var current = await _employeeSalaryRepository.GetCurrentAsync(detail.EmployeeId, ct);
            if (current?.SalaryGradeId != detail.BaselineSalaryGradeId)
            {
                // US-SGP-07 AC02/AC04: no employee's grade changes, decision stays Draft.
                throw new ConflictException($"Employee {detail.EmployeeId}'s current grade no longer matches the decision's baseline.");
            }
        }

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            foreach (var detail in decision.Details)
            {
                await _employeeSalaryRepository.AddAsync(new HrEmployeeSalary
                {
                    EmployeeId = detail.EmployeeId,
                    SalaryGradeId = detail.NewSalaryGradeId,
                    Coefficient = detail.NewCoefficient,
                    EffectiveDate = decision.EffectiveDate,
                    Reason = $"Salary decision {decision.DecisionNumber}",
                    SalaryDecisionId = decision.Id
                }, ct);
            }

            decision.Status = SalaryDecisionStatus.APPLIED;

            var period = await _periodRepository.GetByIdAsync(decision.ReviewPeriodId, ct);
            if (period is not null)
            {
                period.Status = ReviewPeriodStatus.CLOSED; // side effect of Apply, per StateDiagrams.md §1
            }

            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        return decision;
    }

    public async Task<HrSalaryDecision> CancelDecisionAsync(int decisionId, CancellationToken ct = default)
    {
        var decision = await GetOrThrowAsync(decisionId, ct);
        if (decision.Status != SalaryDecisionStatus.DRAFT)
        {
            throw new ConflictException("Only a Draft decision can be cancelled."); // US-SGP-10 AC02/AC03
        }

        // HrEmployeeSalary is never touched — a cancelled decision was never
        // Applied, so there is nothing to revert.
        decision.Status = SalaryDecisionStatus.CANCELLED;
        await _unitOfWork.SaveChangesAsync(ct);
        return decision;
    }

    private async Task<HrSalaryDecision> GetOrThrowAsync(int decisionId, CancellationToken ct) =>
        await _decisionRepository.GetByIdAsync(decisionId, ct)
            ?? throw new NotFoundException($"Salary decision {decisionId} was not found.");
}
