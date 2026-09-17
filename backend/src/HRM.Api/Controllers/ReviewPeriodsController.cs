using HRM.Api.DTOs.Requests;
using HRM.Api.DTOs.Responses;
using HRM.Api.Mappings;
using HRM.Application.SalaryManagement.Interfaces;
using HRM.Application.SalaryManagement.Models;
using HRM.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers;

[ApiController]
[Route("review-periods")]
public class ReviewPeriodsController : ControllerBase
{
    private readonly ISalaryReviewService _salaryReviewService;
    private readonly ISalaryHistoryService _salaryHistoryService;

    public ReviewPeriodsController(ISalaryReviewService salaryReviewService, ISalaryHistoryService salaryHistoryService)
    {
        _salaryReviewService = salaryReviewService;
        _salaryHistoryService = salaryHistoryService;
    }

    // US-01
    [HttpPost]
    [ProducesResponseType(typeof(ReviewPeriodResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateReviewPeriod([FromBody] CreateReviewPeriodRequest request, CancellationToken ct)
    {
        var input = new CreateReviewPeriodInput(request.Code, request.Name, request.ReviewType, request.ReviewDate, request.EffectiveDate, request.Description);
        var period = await _salaryReviewService.CreateReviewPeriodAsync(input, ct);
        return CreatedAtAction(nameof(GetReviewPeriod), new { periodId = period.Id }, period.ToResponse());
    }

    // US-02
    [HttpGet]
    [ProducesResponseType(typeof(ReviewPeriodPageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewPeriods(
        [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate,
        [FromQuery] ReviewType? reviewType, [FromQuery] ReviewPeriodStatus? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _salaryReviewService.SearchReviewPeriodsAsync(fromDate, toDate, reviewType, status, page, pageSize, ct);
        return Ok(new ReviewPeriodPageResponse
        {
            Items = result.Items.Select(p => p.ToResponse()).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems
        });
    }

    // US-03
    [HttpGet("{periodId:int}")]
    [ProducesResponseType(typeof(ReviewPeriodDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewPeriod(int periodId, CancellationToken ct)
    {
        var detail = await _salaryReviewService.GetReviewPeriodDetailAsync(periodId, ct);
        return Ok(detail.ToDetailResponse());
    }

    // US-05
    [HttpPost("{periodId:int}/submit")]
    [ProducesResponseType(typeof(ReviewPeriodResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitReviewPeriod(int periodId, CancellationToken ct)
    {
        var period = await _salaryReviewService.SubmitReviewPeriodAsync(periodId, ct);
        return Ok(period.ToResponse());
    }

    // US-11
    [HttpPost("{periodId:int}/cancel")]
    [ProducesResponseType(typeof(ReviewPeriodResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelReviewPeriod(int periodId, CancellationToken ct)
    {
        var period = await _salaryReviewService.CancelReviewPeriodAsync(periodId, ct);
        return Ok(period.ToResponse());
    }

    // US-03
    [HttpGet("{periodId:int}/employees")]
    [ProducesResponseType(typeof(ReviewPeriodEmployeePageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees(
        int periodId, [FromQuery] string? department, [FromQuery] EligibilityStatus? eligibility,
        [FromQuery] ReviewOutcome? outcome, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _salaryReviewService.SearchReviewEmployeesAsync(periodId, department, eligibility, outcome, page, pageSize, ct);
        return Ok(new ReviewPeriodEmployeePageResponse
        {
            Items = result.Items.Select(e => e.ToResponse()).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems
        });
    }

    // US-03
    [HttpGet("{periodId:int}/employees/{employeeId:int}")]
    [ProducesResponseType(typeof(ReviewPeriodEmployeeDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployee(int periodId, int employeeId, CancellationToken ct)
    {
        var entry = await _salaryReviewService.GetReviewEmployeeAsync(periodId, employeeId, ct);
        var history = await _salaryHistoryService.GetHistoryAsync(employeeId, null, null, ct);
        var basic = entry.ToResponse();

        var response = new ReviewPeriodEmployeeDetailResponse
        {
            EmployeeId = basic.EmployeeId,
            EmployeeCode = basic.EmployeeCode,
            FullName = basic.FullName,
            Department = basic.Department,
            CurrentGrade = basic.CurrentGrade,
            CurrentCoefficient = basic.CurrentCoefficient,
            ProposedGrade = basic.ProposedGrade,
            ProposedCoefficient = basic.ProposedCoefficient,
            EligibilityStatus = basic.EligibilityStatus,
            EligibilityReason = basic.EligibilityReason,
            ReviewOutcome = basic.ReviewOutcome,
            Reason = basic.Reason,
            SalarySnapshotAt = DateTime.UtcNow,
            RecentSalaryHistory = history.Take(5).Select(h => new SalaryHistoryEntrySummaryResponse
            {
                Grade = h.Grade,
                Coefficient = h.Coefficient,
                EffectiveFrom = h.EffectiveFrom,
                EffectiveTo = h.EffectiveTo
            }).ToList()
        };
        return Ok(response);
    }

    // US-04
    [HttpPost("{periodId:int}/employees/{employeeId:int}/approve")]
    [ProducesResponseType(typeof(ReviewPeriodEmployeeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveEmployee(int periodId, int employeeId, CancellationToken ct)
    {
        var entry = await _salaryReviewService.ApproveEmployeeAsync(periodId, employeeId, ct);
        return Ok(entry.ToResponse());
    }

    // US-04
    [HttpPost("{periodId:int}/employees/{employeeId:int}/reject")]
    [ProducesResponseType(typeof(ReviewPeriodEmployeeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectEmployee(int periodId, int employeeId, [FromBody] RejectRequest request, CancellationToken ct)
    {
        var entry = await _salaryReviewService.RejectEmployeeAsync(periodId, employeeId, request.Reason, ct);
        return Ok(entry.ToResponse());
    }

    // US-04
    [HttpPost("{periodId:int}/employees/bulk-approve")]
    [ProducesResponseType(typeof(BulkActionResultResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkApprove(int periodId, [FromBody] BulkEmployeeActionRequest request, CancellationToken ct)
    {
        var result = await _salaryReviewService.BulkApproveAsync(periodId, request.EmployeeIds, ct);
        return Ok(result.ToResponse());
    }

    // US-04
    [HttpPost("{periodId:int}/employees/bulk-reject")]
    [ProducesResponseType(typeof(BulkActionResultResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkReject(int periodId, [FromBody] BulkRejectRequest request, CancellationToken ct)
    {
        var result = await _salaryReviewService.BulkRejectAsync(periodId, request.EmployeeIds, request.Reason, ct);
        return Ok(result.ToResponse());
    }
}
