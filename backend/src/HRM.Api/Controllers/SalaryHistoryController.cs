using HRM.Api.DTOs.Responses;
using HRM.Api.Mappings;
using HRM.Application.SalaryManagement.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers;

[ApiController]
[Route("employees")]
public class SalaryHistoryController : ControllerBase
{
    private readonly ISalaryHistoryService _salaryHistoryService;

    public SalaryHistoryController(ISalaryHistoryService salaryHistoryService)
    {
        _salaryHistoryService = salaryHistoryService;
    }

    // US-08
    [HttpGet("{employeeId:int}/salary-history")]
    [ProducesResponseType(typeof(SalaryHistoryPageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalaryHistory(
        int employeeId, [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, CancellationToken ct)
    {
        var history = await _salaryHistoryService.GetHistoryAsync(employeeId, fromDate, toDate, ct);
        return Ok(new SalaryHistoryPageResponse
        {
            EmployeeId = employeeId,
            Items = history.Select(h => h.ToResponse()).ToList()
        });
    }
}
