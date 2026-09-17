using HRM.Api.DTOs.Requests;
using HRM.Api.DTOs.Responses;
using HRM.Api.Mappings;
using HRM.Application.SalaryManagement.Interfaces;
using HRM.Application.SalaryManagement.Models;
using HRM.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers;

[ApiController]
[Route("salary-decisions")]
public class SalaryDecisionsController : ControllerBase
{
    private readonly ISalaryDecisionService _salaryDecisionService;

    public SalaryDecisionsController(ISalaryDecisionService salaryDecisionService)
    {
        _salaryDecisionService = salaryDecisionService;
    }

    // US-06
    [HttpPost]
    [ProducesResponseType(typeof(SalaryDecisionResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDecision([FromBody] CreateSalaryDecisionRequest request, CancellationToken ct)
    {
        var input = new CreateDecisionInput(
            request.ReviewPeriodId, request.EmployeeIds, request.DecisionNumber,
            request.DecisionType, request.EffectiveDate, request.SignerEmployeeId, request.Description);
        var decision = await _salaryDecisionService.CreateDecisionAsync(input, ct);
        return CreatedAtAction(nameof(GetDecision), new { decisionId = decision.Id }, decision.ToResponse());
    }

    // US-09
    [HttpGet]
    [ProducesResponseType(typeof(SalaryDecisionPageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDecisions(
        [FromQuery] SalaryDecisionStatus? status, [FromQuery] int? reviewPeriodId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _salaryDecisionService.SearchDecisionsAsync(status, reviewPeriodId, page, pageSize, ct);
        return Ok(new SalaryDecisionPageResponse
        {
            Items = result.Items.Select(d => d.ToResponse()).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems
        });
    }

    // US-06 / US-08
    [HttpGet("{decisionId:int}")]
    [ProducesResponseType(typeof(SalaryDecisionDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDecision(int decisionId, CancellationToken ct)
    {
        var decision = await _salaryDecisionService.GetDecisionAsync(decisionId, ct);
        return Ok(decision.ToDetailResponse());
    }

    // US-06
    [HttpDelete("{decisionId:int}/employees/{employeeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveEmployee(int decisionId, int employeeId, CancellationToken ct)
    {
        await _salaryDecisionService.RemoveEmployeeAsync(decisionId, employeeId, ct);
        return NoContent();
    }

    // US-07
    [HttpPost("{decisionId:int}/apply")]
    [ProducesResponseType(typeof(SalaryDecisionDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApplyDecision(int decisionId, CancellationToken ct)
    {
        var decision = await _salaryDecisionService.ApplyDecisionAsync(decisionId, ct);
        return Ok(decision.ToDetailResponse());
    }

    // US-10
    [HttpPost("{decisionId:int}/cancel")]
    [ProducesResponseType(typeof(SalaryDecisionDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelDecision(int decisionId, CancellationToken ct)
    {
        var decision = await _salaryDecisionService.CancelDecisionAsync(decisionId, ct);
        return Ok(decision.ToDetailResponse());
    }
}
