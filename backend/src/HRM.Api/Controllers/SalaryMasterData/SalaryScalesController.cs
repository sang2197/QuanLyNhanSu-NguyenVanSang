using HRM.Api.DTOs.Requests.SalaryMasterData;
using HRM.Api.DTOs.Responses.SalaryMasterData;
using HRM.Api.Mappings.SalaryMasterData;
using HRM.Application.SalaryMasterData.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers.SalaryMasterData;

[ApiController]
[Route("salary-scales")]
public class SalaryScalesController : ControllerBase
{
    private readonly ISalaryScaleService _service;

    public SalaryScalesController(ISalaryScaleService service)
    {
        _service = service;
    }

    // BR-SAL-05/06
    [HttpPost]
    [ProducesResponseType(typeof(SalaryScaleResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateScale([FromBody] CreateSalaryScaleRequest request, CancellationToken ct)
    {
        var scale = await _service.CreateScaleAsync(request.Code, request.Name, ct);
        return CreatedAtAction(nameof(GetScaleDetail), new { scaleId = scale.Id }, scale.ToResponse());
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SalaryScaleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListScales(CancellationToken ct)
    {
        var scales = await _service.ListScalesAsync(ct);
        return Ok(scales.Select(s => s.ToResponse()).ToList());
    }

    [HttpGet("{scaleId:int}")]
    [ProducesResponseType(typeof(SalaryScaleDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScaleDetail(int scaleId, CancellationToken ct)
    {
        var scale = await _service.GetScaleDetailAsync(scaleId, ct);
        return Ok(scale.ToDetailResponse());
    }

    // BR-SAL-07/08
    [HttpPut("{scaleId:int}")]
    [ProducesResponseType(typeof(SalaryScaleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateScale(int scaleId, [FromBody] UpdateSalaryScaleRequest request, CancellationToken ct)
    {
        var scale = await _service.UpdateScaleAsync(scaleId, request.Name, ct);
        return Ok(scale.ToResponse());
    }

    // BR-SAL-20
    [HttpPost("{scaleId:int}/deactivate")]
    [ProducesResponseType(typeof(SalaryScaleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateScale(int scaleId, CancellationToken ct)
    {
        var scale = await _service.DeactivateScaleAsync(scaleId, ct);
        return Ok(scale.ToResponse());
    }

    // BR-SAL-22 (no guard)
    [HttpPost("{scaleId:int}/reactivate")]
    [ProducesResponseType(typeof(SalaryScaleResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReactivateScale(int scaleId, CancellationToken ct)
    {
        var scale = await _service.ReactivateScaleAsync(scaleId, ct);
        return Ok(scale.ToResponse());
    }

    // BR-SAL-09/10/11/12
    [HttpPost("{scaleId:int}/grades")]
    [ProducesResponseType(typeof(SalaryGradeResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateGrade(int scaleId, [FromBody] CreateSalaryGradeRequest request, CancellationToken ct)
    {
        var grade = await _service.CreateGradeAsync(scaleId, request.GradeNumber, request.Coefficient, ct);
        return CreatedAtAction(nameof(GetScaleDetail), new { scaleId }, grade.ToResponse(request.Coefficient));
    }
}
