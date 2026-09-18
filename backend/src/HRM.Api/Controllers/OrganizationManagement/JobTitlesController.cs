using HRM.Api.DTOs.Requests.OrganizationManagement;
using HRM.Api.DTOs.Responses.OrganizationManagement;
using HRM.Api.Mappings.OrganizationManagement;
using HRM.Application.OrganizationManagement.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers.OrganizationManagement;

[ApiController]
[Route("job-titles")]
public class JobTitlesController : ControllerBase
{
    private readonly IJobTitleService _service;

    public JobTitlesController(IJobTitleService service)
    {
        _service = service;
    }

    // BR-ORG-06
    [HttpPost]
    [ProducesResponseType(typeof(JobTitleResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateJobTitle([FromBody] CreateJobTitleRequest request, CancellationToken ct)
    {
        var jobTitle = await _service.CreateJobTitleAsync(request.Name, ct);
        return CreatedAtAction(nameof(ListJobTitles), null, jobTitle.ToResponse());
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<JobTitleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListJobTitles(CancellationToken ct)
    {
        var jobTitles = await _service.ListJobTitlesAsync(ct);
        return Ok(jobTitles.Select(t => t.ToResponse()).ToList());
    }

    // BR-ORG-07
    [HttpPut("{jobTitleId:int}")]
    [ProducesResponseType(typeof(JobTitleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateJobTitle(int jobTitleId, [FromBody] UpdateJobTitleRequest request, CancellationToken ct)
    {
        var jobTitle = await _service.UpdateJobTitleAsync(jobTitleId, request.Name, ct);
        return Ok(jobTitle.ToResponse());
    }

    // BR-ORG-08 (deactivate branch, no guard)
    [HttpPost("{jobTitleId:int}/deactivate")]
    [ProducesResponseType(typeof(JobTitleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateJobTitle(int jobTitleId, CancellationToken ct)
    {
        var jobTitle = await _service.DeactivateJobTitleAsync(jobTitleId, ct);
        return Ok(jobTitle.ToResponse());
    }

    // BR-ORG-08 (reactivate branch, no guard)
    [HttpPost("{jobTitleId:int}/reactivate")]
    [ProducesResponseType(typeof(JobTitleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReactivateJobTitle(int jobTitleId, CancellationToken ct)
    {
        var jobTitle = await _service.ReactivateJobTitleAsync(jobTitleId, ct);
        return Ok(jobTitle.ToResponse());
    }
}
