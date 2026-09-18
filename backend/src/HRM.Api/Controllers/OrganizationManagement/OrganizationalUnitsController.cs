using HRM.Api.DTOs.Requests.OrganizationManagement;
using HRM.Api.DTOs.Responses.OrganizationManagement;
using HRM.Api.Mappings.OrganizationManagement;
using HRM.Application.OrganizationManagement.Interfaces;
using HRM.Application.OrganizationManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers.OrganizationManagement;

[ApiController]
[Route("organizational-units")]
public class OrganizationalUnitsController : ControllerBase
{
    private readonly IOrganizationalUnitService _service;

    public OrganizationalUnitsController(IOrganizationalUnitService service)
    {
        _service = service;
    }

    // BR-ORG-01
    [HttpPost]
    [ProducesResponseType(typeof(OrganizationalUnitResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUnit([FromBody] CreateOrganizationalUnitRequest request, CancellationToken ct)
    {
        var input = new CreateOrganizationalUnitInput(request.Name, request.ParentId, request.UnitType, request.ContactEmail, request.ContactPhone);
        var unit = await _service.CreateUnitAsync(input, ct);
        return CreatedAtAction(nameof(GetStructure), null, unit.ToResponse());
    }

    // BR-ORG-02
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrganizationalUnitResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStructure(CancellationToken ct)
    {
        var units = await _service.GetStructureAsync(ct);
        return Ok(units.Select(u => u.ToResponse()).ToList());
    }

    // BR-ORG-03
    [HttpPut("{unitId:int}")]
    [ProducesResponseType(typeof(OrganizationalUnitResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUnit(int unitId, [FromBody] UpdateOrganizationalUnitRequest request, CancellationToken ct)
    {
        var input = new UpdateOrganizationalUnitInput(request.Name, request.UnitType, request.ContactEmail, request.ContactPhone);
        var unit = await _service.UpdateUnitAsync(unitId, input, ct);
        return Ok(unit.ToResponse());
    }

    // BR-ORG-04
    [HttpPost("{unitId:int}/move")]
    [ProducesResponseType(typeof(OrganizationalUnitResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MoveUnit(int unitId, [FromBody] MoveOrganizationalUnitRequest request, CancellationToken ct)
    {
        var unit = await _service.MoveUnitAsync(unitId, request.TargetParentId, ct);
        return Ok(unit.ToResponse());
    }

    // BR-ORG-05 (deactivate branch)
    [HttpPost("{unitId:int}/deactivate")]
    [ProducesResponseType(typeof(OrganizationalUnitResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateUnit(int unitId, CancellationToken ct)
    {
        var unit = await _service.DeactivateUnitAsync(unitId, ct);
        return Ok(unit.ToResponse());
    }

    // BR-ORG-05 (reactivate branch)
    [HttpPost("{unitId:int}/reactivate")]
    [ProducesResponseType(typeof(OrganizationalUnitResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReactivateUnit(int unitId, CancellationToken ct)
    {
        var unit = await _service.ReactivateUnitAsync(unitId, ct);
        return Ok(unit.ToResponse());
    }
}
