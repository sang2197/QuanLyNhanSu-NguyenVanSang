using HRM.Api.DTOs.Responses.SalaryGradePromotion;
using HRM.Api.Mappings.SalaryGradePromotion;
using HRM.Application.SalaryGradePromotion.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers.SalaryGradePromotion;

[ApiController]
[Route("employees")]
public class SalaryHistoryController : ControllerBase
{
    private readonly ISalaryHistoryService _service;

    public SalaryHistoryController(ISalaryHistoryService service)
    {
        _service = service;
    }

    // US-SGP-08
    [HttpGet("{employeeId:int}/salary-history")]
    [ProducesResponseType(typeof(SalaryHistoryPageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalaryHistory(int employeeId, [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, CancellationToken ct)
    {
        var history = await _service.GetHistoryAsync(employeeId, fromDate, toDate, ct);
        return Ok(new SalaryHistoryPageResponse { EmployeeId = employeeId, Items = history.Select(h => h.ToResponse()).ToList() });
    }
}
