using HRM.Api.DTOs.Requests.SalaryGradePromotion;
using HRM.Api.DTOs.Responses.SalaryGradePromotion;
using HRM.Api.Mappings.SalaryGradePromotion;
using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers.SalaryGradePromotion;

[ApiController]
[Route("review-periods/{periodId:int}/employees")]
public class ReviewPeriodEmployeesController : ControllerBase
{
    private readonly IReviewEmployeeService _service;

    public ReviewPeriodEmployeesController(IReviewEmployeeService service)
    {
        _service = service;
    }

    // US-SGP-03 AC06
    [HttpGet]
    [ProducesResponseType(typeof(ReviewPeriodEmployeePageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListReviewEmployees(
        int periodId, [FromQuery] int? organizationalUnitId, [FromQuery] bool? eligible, [FromQuery] ReviewOutcome? outcome,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _service.ListReviewEmployeesAsync(periodId, organizationalUnitId, eligible, outcome, page, pageSize, ct);
        return Ok(new ReviewPeriodEmployeePageResponse
        {
            Items = result.Items.Select(e => e.ToResponse()).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems
        });
    }

    [HttpGet("{employeeId:int}")]
    [ProducesResponseType(typeof(ReviewPeriodEmployeeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewEmployee(int periodId, int employeeId, CancellationToken ct)
    {
        var entry = await _service.GetReviewEmployeeAsync(periodId, employeeId, ct);
        return Ok(entry.ToResponse());
    }

    // US-SGP-04
    [HttpPost("{employeeId:int}/approve")]
    [ProducesResponseType(typeof(ReviewPeriodEmployeeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveEmployee(int periodId, int employeeId, CancellationToken ct)
    {
        var entry = await _service.ApproveEmployeeAsync(periodId, employeeId, ct);
        return Ok(entry.ToResponse());
    }

    [HttpPost("{employeeId:int}/reject")]
    [ProducesResponseType(typeof(ReviewPeriodEmployeeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectEmployee(int periodId, int employeeId, [FromBody] RejectRequest request, CancellationToken ct)
    {
        var entry = await _service.RejectEmployeeAsync(periodId, employeeId, request.Reason, ct);
        return Ok(entry.ToResponse());
    }

    [HttpPost("bulk-approve")]
    [ProducesResponseType(typeof(BulkActionResultResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkApprove(int periodId, [FromBody] BulkEmployeeActionRequest request, CancellationToken ct)
    {
        var result = await _service.BulkApproveAsync(periodId, request.EmployeeIds, ct);
        return Ok(result.ToResponse());
    }

    [HttpPost("bulk-reject")]
    [ProducesResponseType(typeof(BulkActionResultResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkReject(int periodId, [FromBody] BulkRejectRequest request, CancellationToken ct)
    {
        var result = await _service.BulkRejectAsync(periodId, request.EmployeeIds, request.Reason, ct);
        return Ok(result.ToResponse());
    }
}
