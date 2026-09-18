using HRM.Api.DTOs.Requests.SalaryMasterData;
using HRM.Api.DTOs.Responses.SalaryMasterData;
using HRM.Api.Mappings.SalaryMasterData;
using HRM.Application.SalaryMasterData.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers.SalaryMasterData;

[ApiController]
[Route("base-salary-rates")]
public class BaseSalaryRatesController : ControllerBase
{
    private readonly IBaseSalaryRateService _service;

    public BaseSalaryRatesController(IBaseSalaryRateService service)
    {
        _service = service;
    }

    // BR-SAL-01
    [HttpPost]
    [ProducesResponseType(typeof(BaseSalaryRateResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddRate([FromBody] AddBaseSalaryRateRequest request, CancellationToken ct)
    {
        var rate = await _service.AddRateAsync(request.Rate, request.EffectiveDate, ct);
        return CreatedAtAction(nameof(ListRates), null, rate.ToResponse());
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BaseSalaryRateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListRates([FromQuery] DateOnly? asOfDate, CancellationToken ct)
    {
        var rates = await _service.ListRatesAsync(asOfDate, ct);
        return Ok(rates.Select(r => r.ToResponse()).ToList());
    }
}
