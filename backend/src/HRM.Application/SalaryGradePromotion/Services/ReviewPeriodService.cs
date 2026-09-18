using HRM.Application.Common;
using HRM.Application.EmployeeManagement.Interfaces;
using HRM.Application.Exceptions;
using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Application.SalaryGradePromotion.Models;
using HRM.Application.SalaryGradePromotion.Rules;
using HRM.Application.SalaryMasterData.Interfaces;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.SalaryGradePromotion.Services;

/// <inheritdoc cref="IReviewPeriodService"/>
public class ReviewPeriodService : IReviewPeriodService
{
    private readonly IReviewPeriodRepository _periodRepository;
    private readonly IReviewEmployeeRepository _reviewEmployeeRepository;
    private readonly ISalaryDecisionRepository _decisionRepository;
    private readonly IEmployeeSalaryRepository _employeeSalaryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISalaryPromotionEligibilityRule _eligibilityRule;
    private readonly IEmployeeService _employeeService;
    private readonly ISalaryGradeService _salaryGradeService;

    public ReviewPeriodService(
        IReviewPeriodRepository periodRepository,
        IReviewEmployeeRepository reviewEmployeeRepository,
        ISalaryDecisionRepository decisionRepository,
        IEmployeeSalaryRepository employeeSalaryRepository,
        IUnitOfWork unitOfWork,
        ISalaryPromotionEligibilityRule eligibilityRule,
        IEmployeeService employeeService,
        ISalaryGradeService salaryGradeService)
    {
        _periodRepository = periodRepository;
        _reviewEmployeeRepository = reviewEmployeeRepository;
        _decisionRepository = decisionRepository;
        _employeeSalaryRepository = employeeSalaryRepository;
        _unitOfWork = unitOfWork;
        _eligibilityRule = eligibilityRule;
        _employeeService = employeeService;
        _salaryGradeService = salaryGradeService;
    }

    /// <summary>Sequence 4.1 — the most business-rule-heavy flow in the
    /// system. Owns a transaction because it writes across both
    /// IReviewPeriodRepository and IReviewEmployeeRepository.</summary>
    public async Task<HrSalaryReviewPeriod> CreateReviewPeriodAsync(CreateReviewPeriodInput input, CancellationToken ct = default)
    {
        if (await _periodRepository.GetByCodeAsync(input.Code, ct) is not null ||
            await _periodRepository.GetByNameAsync(input.Name, ct) is not null)
        {
            throw new ConflictException("Duplicate review period code or name."); // US-SGP-01 AC02/AC03
        }

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var period = new HrSalaryReviewPeriod
            {
                Code = input.Code,
                Name = input.Name,
                ReviewType = input.ReviewType,
                ReviewDate = input.ReviewDate,
                EffectiveDate = input.EffectiveDate,
                Description = input.Description,
                Status = ReviewPeriodStatus.IN_PROGRESS
            };
            await _periodRepository.AddAsync(period, ct);
            await _unitOfWork.SaveChangesAsync(ct); // need period.Id before building snapshot rows

            var activeEmployees = await _employeeService.GetActiveEmployeesAsync(ct); // cross-domain, US-SGP-01
            var snapshotRows = new List<HrSalaryReviewEmployee>();

            foreach (var employee in activeEmployees)
            {
                var currentSalary = await _employeeSalaryRepository.GetCurrentAsync(employee.Id, ct);
                if (currentSalary is null)
                {
                    continue; // no salary history yet — cannot be screened
                }

                var currentGrade = currentSalary.SalaryGrade;
                var nextActiveGrade = await _salaryGradeService.GetNextActiveGradeAsync(
                    currentGrade.SalaryScaleId, currentGrade.GradeNumber, ct); // cross-domain, BR-SAL-17
                var eligibility = _eligibilityRule.DetermineEligibility(currentSalary.EffectiveDate, input.ReviewDate, nextActiveGrade);

                decimal? proposedCoefficient = eligibility.ProposedGrade is not null
                    ? await _salaryGradeService.GetCurrentCoefficientAsync(eligibility.ProposedGrade.Id, ct)
                    : null;

                snapshotRows.Add(new HrSalaryReviewEmployee
                {
                    ReviewPeriodId = period.Id,
                    EmployeeId = employee.Id,
                    CurrentSalaryGradeId = currentGrade.Id,
                    CurrentCoefficient = currentSalary.Coefficient,
                    Eligible = eligibility.IsEligible,
                    ProposedSalaryGradeId = eligibility.ProposedGrade?.Id,
                    ProposedCoefficient = proposedCoefficient,
                    IneligibleReason = eligibility.IsEligible ? null : eligibility.Reason,
                    Outcome = eligibility.IsEligible ? ReviewOutcome.PENDING : null
                });
            }

            await _reviewEmployeeRepository.AddRangeAsync(snapshotRows, ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            return period;
        }
        catch
        {
            // US-SGP-01 AC04: if the calculation can't complete for every
            // employee, the period must not become available for processing
            // — surfaces to the client as 500 via ExceptionHandlingMiddleware's
            // catch-all, this is a server fault, not a business rejection.
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    // HRM.Application has no EF Core reference — IQueryable<T> from the
    // repository is composed and materialized with plain synchronous LINQ.
    public Task<PagedResult<HrSalaryReviewPeriod>> SearchReviewPeriodsAsync(
        DateOnly? fromDate, DateOnly? toDate, ReviewType? reviewType, ReviewPeriodStatus? status,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _periodRepository.Query();

        if (fromDate is DateOnly from)
        {
            query = query.Where(p => p.ReviewDate >= from);
        }
        if (toDate is DateOnly to)
        {
            query = query.Where(p => p.ReviewDate <= to);
        }
        if (reviewType is ReviewType type)
        {
            query = query.Where(p => p.ReviewType == type);
        }
        if (status is ReviewPeriodStatus s)
        {
            query = query.Where(p => p.Status == s);
        }

        var totalItems = query.Count();
        var items = query.OrderByDescending(p => p.ReviewDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PagedResult<HrSalaryReviewPeriod> { Items = items, Page = page, PageSize = pageSize, TotalItems = totalItems });
    }

    public async Task<ReviewPeriodDetailResult> GetReviewPeriodDetailAsync(int periodId, CancellationToken ct = default)
    {
        var period = await GetOrThrowAsync(periodId, ct);
        var entries = _reviewEmployeeRepository.QueryByPeriod(periodId).ToList();

        var decision = await _decisionRepository.GetNonCancelledByPeriodAsync(periodId, ct);

        return new ReviewPeriodDetailResult(
            period,
            TotalEmployees: entries.Count,
            EligibleCount: entries.Count(e => e.Eligible),
            ApprovedCount: entries.Count(e => e.Outcome == ReviewOutcome.APPROVED),
            RejectedCount: entries.Count(e => e.Outcome == ReviewOutcome.REJECTED),
            PendingCount: entries.Count(e => e.Outcome == ReviewOutcome.PENDING),
            DecisionId: decision?.Id);
    }

    public async Task<HrSalaryReviewPeriod> SubmitReviewPeriodAsync(int periodId, CancellationToken ct = default)
    {
        var period = await GetOrThrowAsync(periodId, ct);

        if (await _reviewEmployeeRepository.CountUnprocessedEligibleAsync(periodId, ct) > 0)
        {
            throw new ConflictException("Eligible employees still unprocessed."); // US-SGP-05 AC02
        }

        period.Status = ReviewPeriodStatus.SUBMITTED;
        await _unitOfWork.SaveChangesAsync(ct);
        return period;
    }

    public async Task<HrSalaryReviewPeriod> CancelReviewPeriodAsync(int periodId, CancellationToken ct = default)
    {
        var period = await GetOrThrowAsync(periodId, ct);

        if (period.Status is ReviewPeriodStatus.CLOSED or ReviewPeriodStatus.CANCELLED)
        {
            throw new ConflictException("Review period is already Closed or Cancelled."); // US-SGP-11 AC04/AC05
        }
        if (await _decisionRepository.GetNonCancelledByPeriodAsync(periodId, ct) is not null)
        {
            throw new ConflictException("Cancel the salary decision first."); // US-SGP-11 AC03
        }

        period.Status = ReviewPeriodStatus.CANCELLED;
        await _unitOfWork.SaveChangesAsync(ct);
        return period;
    }

    private async Task<HrSalaryReviewPeriod> GetOrThrowAsync(int periodId, CancellationToken ct) =>
        await _periodRepository.GetByIdAsync(periodId, ct)
            ?? throw new NotFoundException($"Review period {periodId} was not found.");
}
