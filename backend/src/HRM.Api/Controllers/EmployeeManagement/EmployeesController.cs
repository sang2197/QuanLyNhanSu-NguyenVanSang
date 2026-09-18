using HRM.Api.DTOs.Requests.EmployeeManagement;
using HRM.Api.DTOs.Responses.EmployeeManagement;
using HRM.Api.Mappings.EmployeeManagement;
using HRM.Application.EmployeeManagement.Interfaces;
using HRM.Application.EmployeeManagement.Models;
using HRM.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers.EmployeeManagement;

[ApiController]
[Route("employees")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeesController(IEmployeeService service)
    {
        _service = service;
    }

    // BR-EMP-01/02/04/05
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request, CancellationToken ct)
    {
        var input = new CreateEmployeeInput(request.EmployeeCode, request.FullName, request.OrganizationalUnitId, request.JobTitleId, request.JoinDate, request.EmploymentStatus);
        var employee = await _service.CreateEmployeeAsync(input, ct);
        return CreatedAtAction(nameof(GetEmployee), new { employeeId = employee.Id }, employee.ToResponse());
    }

    // BR-EMP-06/07
    [HttpGet]
    [ProducesResponseType(typeof(EmployeePageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchEmployees(
        [FromQuery] string? search, [FromQuery] int? organizationalUnitId, [FromQuery] int? jobTitleId,
        [FromQuery] EmploymentStatus? employmentStatus, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _service.SearchEmployeesAsync(search, organizationalUnitId, jobTitleId, employmentStatus, page, pageSize, ct);
        return Ok(new EmployeePageResponse
        {
            Items = result.Items.Select(e => e.ToResponse()).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems
        });
    }

    [HttpGet("{employeeId:int}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployee(int employeeId, CancellationToken ct)
    {
        var employee = await _service.GetEmployeeAsync(employeeId, ct);
        return Ok(employee.ToResponse());
    }

    // BR-EMP-03/04/05/08/09
    [HttpPut("{employeeId:int}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateEmployee(int employeeId, [FromBody] UpdateEmployeeRequest request, CancellationToken ct)
    {
        var input = new UpdateEmployeeInput(request.EmployeeCode, request.FullName, request.OrganizationalUnitId, request.JobTitleId, request.JoinDate);
        var employee = await _service.UpdateEmployeeAsync(employeeId, input, ct);
        return Ok(employee.ToResponse());
    }

    // BR-EMP-10/11, OQ-EMP-01
    [HttpPost("{employeeId:int}/employment-status")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeEmploymentStatus(int employeeId, [FromBody] ChangeEmploymentStatusRequest request, CancellationToken ct)
    {
        var employee = await _service.ChangeEmploymentStatusAsync(employeeId, request.EmploymentStatus, ct);
        return Ok(employee.ToResponse());
    }
}
