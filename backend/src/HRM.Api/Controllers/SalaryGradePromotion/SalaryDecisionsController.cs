using HRM.Api.DTOs.Requests.SalaryGradePromotion;
using HRM.Api.DTOs.Responses.SalaryGradePromotion;
using HRM.Api.Mappings.SalaryGradePromotion;
using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Application.SalaryGradePromotion.Models;
using HRM.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers.SalaryGradePromotion;

[ApiController]
[Route("salary-decisions")]
public class SalaryDecisionsController : ControllerBase
{
    private readonly ISalaryDecisionService _service;

    public SalaryDecisionsController(ISalaryDecisionService service)
    {
        _service = service;
    }

    // US-SGP-06
    [HttpPost]
    [ProducesResponseType(typeof(SalaryDecisionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDecision([FromBody] CreateSalaryDecisionRequest request, CancellationToken ct)
    {
        var input = new CreateSalaryDecisionInput(request.ReviewPeriodId, request.EmployeeIds, request.EffectiveDate);
        var decision = await _service.CreateDecisionAsync(input, ct);
        return CreatedAtAction(nameof(GetDecision), new { decisionId = decision.Id }, decision.ToResponse());
    }

    [HttpGet]
    [ProducesResponseType(typeof(SalaryDecisionPageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchDecisions(
        [FromQuery] SalaryDecisionStatus? status, [FromQuery] int? reviewPeriodId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _service.SearchDecisionsAsync(status, reviewPeriodId, page, pageSize, ct);
        return Ok(new SalaryDecisionPageResponse
        {
            Items = result.Items.Select(d => d.ToResponse()).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems
        });
    }

    // US-SGP-09 AC04
    [HttpGet("eligible-review-periods")]
    [ProducesResponseType(typeof(IReadOnlyList<HRM.Api.DTOs.Responses.SalaryGradePromotion.ReviewPeriodResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEligibleReviewPeriods(CancellationToken ct)
    {
        var periods = await _service.GetEligibleReviewPeriodsAsync(ct);
        return Ok(periods.Select(p => p.ToResponse()).ToList());
    }

    [HttpGet("{decisionId:int}")]
    [ProducesResponseType(typeof(SalaryDecisionDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDecision(int decisionId, CancellationToken ct)
    {
        var decision = await _service.GetDecisionAsync(decisionId, ct);
        return Ok(decision.ToDetailResponse());
    }

    [HttpPut("{decisionId:int}")]
    [ProducesResponseType(typeof(SalaryDecisionDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveDraft(int decisionId, [FromBody] UpdateSalaryDecisionRequest request, CancellationToken ct)
    {
        var decision = await _service.SaveDraftAsync(decisionId, request.EffectiveDate, ct);
        return Ok(decision.ToDetailResponse());
    }

    // US-SGP-06 AC04
    [HttpDelete("{decisionId:int}/employees/{employeeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveEmployee(int decisionId, int employeeId, CancellationToken ct)
    {
        await _service.RemoveEmployeeAsync(decisionId, employeeId, ct);
        return NoContent();
    }

    // US-SGP-07
    [HttpPost("{decisionId:int}/apply")]
    [ProducesResponseType(typeof(SalaryDecisionDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApplyDecision(int decisionId, CancellationToken ct)
    {
        var decision = await _service.ApplyDecisionAsync(decisionId, ct);
        return Ok(decision.ToDetailResponse());
    }

    // US-SGP-10
    [HttpPost("{decisionId:int}/cancel")]
    [ProducesResponseType(typeof(SalaryDecisionDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelDecision(int decisionId, CancellationToken ct)
    {
        var decision = await _service.CancelDecisionAsync(decisionId, ct);
        return Ok(decision.ToDetailResponse());
    }
}
