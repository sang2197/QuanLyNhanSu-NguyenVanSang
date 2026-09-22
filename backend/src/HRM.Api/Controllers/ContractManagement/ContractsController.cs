using HRM.Api.DTOs.Requests.ContractManagement;
using HRM.Api.DTOs.Responses.ContractManagement;
using HRM.Api.Mappings.ContractManagement;
using HRM.Application.ContractManagement.Interfaces;
using HRM.Application.ContractManagement.Models;
using HRM.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Api.Controllers.ContractManagement;

[ApiController]
[Route("contracts")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _service;

    public ContractsController(IContractService service)
    {
        _service = service;
    }

    // BR-CON-01/02/04-10
    [HttpPost]
    [ProducesResponseType(typeof(ContractDetailResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateContract([FromBody] CreateContractRequest request, CancellationToken ct)
    {
        var input = new CreateContractInput(
            request.EmployeeId, request.ContractType, request.ContractNumber, request.StartDate,
            request.EndDate, request.ContractSalaryAmount, request.SalaryNote, request.Status);
        var contract = await _service.CreateContractAsync(input, ct);
        return CreatedAtAction(nameof(GetContract), new { contractId = contract.Id }, contract.ToDetailResponse());
    }

    // BR-CON-11-14, BR-CON-25-27, BR-CON-32
    [HttpGet]
    [ProducesResponseType(typeof(ContractPageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchContracts(
        [FromQuery] string? search, [FromQuery] ContractType? contractType, [FromQuery] ContractStatus? status,
        [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate,
        [FromQuery] bool expiringSoon = false, [FromQuery] int window = 30,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _service.SearchContractsAsync(search, contractType, status, fromDate, toDate, expiringSoon, window, page, pageSize, ct);
        return Ok(new ContractPageResponse
        {
            Items = result.Items.Select(c => c.ToResponse()).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems
        });
    }

    [HttpGet("{contractId:int}")]
    [ProducesResponseType(typeof(ContractDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContract(int contractId, CancellationToken ct)
    {
        var contract = await _service.GetContractAsync(contractId, ct);
        return Ok(contract.ToDetailResponse());
    }

    // BR-CON-28-30
    [HttpPut("{contractId:int}")]
    [ProducesResponseType(typeof(ContractDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateContract(int contractId, [FromBody] UpdateContractRequest request, CancellationToken ct)
    {
        var input = new UpdateContractInput(
            request.ContractType, request.ContractNumber, request.StartDate,
            request.EndDate, request.ContractSalaryAmount, request.SalaryNote);
        var contract = await _service.UpdateContractAsync(contractId, input, ct);
        return Ok(contract.ToDetailResponse());
    }

    // BR-CON-28, BR-CON-31
    [HttpDelete("{contractId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteContract(int contractId, CancellationToken ct)
    {
        await _service.DeleteContractAsync(contractId, ct);
        return NoContent();
    }

    // BR-CON-18
    [HttpPost("{contractId:int}/activate")]
    [ProducesResponseType(typeof(ContractDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateContract(int contractId, CancellationToken ct)
    {
        var contract = await _service.ActivateContractAsync(contractId, ct);
        return Ok(contract.ToDetailResponse());
    }

    // BR-CON-19
    [HttpPost("{contractId:int}/expire")]
    [ProducesResponseType(typeof(ContractDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExpireContract(int contractId, CancellationToken ct)
    {
        var contract = await _service.ExpireContractAsync(contractId, ct);
        return Ok(contract.ToDetailResponse());
    }

    // BR-CON-21/22
    [HttpPost("{contractId:int}/terminate")]
    [ProducesResponseType(typeof(ContractDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> TerminateContract(int contractId, [FromBody] TerminateContractRequest request, CancellationToken ct)
    {
        var contract = await _service.TerminateContractAsync(contractId, request.TerminationDate, request.TerminationReason, ct);
        return Ok(contract.ToDetailResponse());
    }
}
