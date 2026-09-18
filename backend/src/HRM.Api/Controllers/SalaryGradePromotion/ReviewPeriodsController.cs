using HRM.Api.DTOs.Requests.SalaryGradePromotion;
using HRM.Api.DTOs.Responses.SalaryGradePromotion;
using HRM.Api.Mappings.SalaryGradePromotion;
using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Application.SalaryGradePromotion.Models;
using HRM.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers.SalaryGradePromotion;

[ApiController]
[Route("review-periods")]
public class ReviewPeriodsController : ControllerBase
{
    private readonly IReviewPeriodService _service;

    public ReviewPeriodsController(IReviewPeriodService service)
    {
        _service = service;
    }

    // US-SGP-01
    [HttpPost]
    [ProducesResponseType(typeof(ReviewPeriodResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateReviewPeriod([FromBody] CreateReviewPeriodRequest request, CancellationToken ct)
    {
        var input = new CreateReviewPeriodInput(request.Code, request.Name, request.ReviewType, request.ReviewDate, request.EffectiveDate, request.Description);
        var period = await _service.CreateReviewPeriodAsync(input, ct);
        return CreatedAtAction(nameof(GetReviewPeriod), new { periodId = period.Id }, period.ToResponse());
    }

    // US-SGP-02
    [HttpGet]
    [ProducesResponseType(typeof(ReviewPeriodPageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchReviewPeriods(
        [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate,
        [FromQuery] ReviewType? reviewType, [FromQuery] ReviewPeriodStatus? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _service.SearchReviewPeriodsAsync(fromDate, toDate, reviewType, status, page, pageSize, ct);
        return Ok(new ReviewPeriodPageResponse
        {
            Items = result.Items.Select(p => p.ToResponse()).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems
        });
    }

    // US-SGP-03
    [HttpGet("{periodId:int}")]
    [ProducesResponseType(typeof(ReviewPeriodDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewPeriod(int periodId, CancellationToken ct)
    {
        var detail = await _service.GetReviewPeriodDetailAsync(periodId, ct);
        return Ok(detail.ToDetailResponse());
    }

    // US-SGP-05
    [HttpPost("{periodId:int}/submit")]
    [ProducesResponseType(typeof(ReviewPeriodResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitReviewPeriod(int periodId, CancellationToken ct)
    {
        var period = await _service.SubmitReviewPeriodAsync(periodId, ct);
        return Ok(period.ToResponse());
    }

    // US-SGP-11
    [HttpPost("{periodId:int}/cancel")]
    [ProducesResponseType(typeof(ReviewPeriodResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelReviewPeriod(int periodId, CancellationToken ct)
    {
        var period = await _service.CancelReviewPeriodAsync(periodId, ct);
        return Ok(period.ToResponse());
    }
}
