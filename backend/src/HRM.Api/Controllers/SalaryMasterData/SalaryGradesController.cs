using HRM.Api.DTOs.Requests.SalaryMasterData;
using HRM.Api.DTOs.Responses.SalaryMasterData;
using HRM.Api.Mappings.SalaryMasterData;
using HRM.Application.SalaryMasterData.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers.SalaryMasterData;

[ApiController]
[Route("salary-grades")]
public class SalaryGradesController : ControllerBase
{
    private readonly ISalaryGradeService _service;

    public SalaryGradesController(ISalaryGradeService service)
    {
        _service = service;
    }

    // BR-SAL-10/13/13A
    [HttpPost("{gradeId:int}/coefficients")]
    [ProducesResponseType(typeof(SalaryGradeCoefficientResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddCoefficient(int gradeId, [FromBody] AddSalaryGradeCoefficientRequest request, CancellationToken ct)
    {
        var coefficient = await _service.AddCoefficientAsync(gradeId, request.Coefficient, request.EffectiveDate, ct);
        return CreatedAtAction(nameof(ListCoefficients), new { gradeId }, coefficient.ToResponse());
    }

    [HttpGet("{gradeId:int}/coefficients")]
    [ProducesResponseType(typeof(IReadOnlyList<SalaryGradeCoefficientResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCoefficients(int gradeId, CancellationToken ct)
    {
        var coefficients = await _service.ListCoefficientsAsync(gradeId, ct);
        return Ok(coefficients.Select(c => c.ToResponse()).ToList());
    }

    // BR-SAL-15 (cross-domain guard)
    [HttpPost("{gradeId:int}/deactivate")]
    [ProducesResponseType(typeof(SalaryGradeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateGrade(int gradeId, CancellationToken ct)
    {
        var grade = await _service.DeactivateGradeAsync(gradeId, ct);
        var coefficient = await _service.GetCurrentCoefficientAsync(gradeId, ct);
        return Ok(grade.ToResponse(coefficient));
    }

    // BR-SAL-18
    [HttpPost("{gradeId:int}/reactivate")]
    [ProducesResponseType(typeof(SalaryGradeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReactivateGrade(int gradeId, CancellationToken ct)
    {
        var grade = await _service.ReactivateGradeAsync(gradeId, ct);
        var coefficient = await _service.GetCurrentCoefficientAsync(gradeId, ct);
        return Ok(grade.ToResponse(coefficient));
    }
}
