using HRM.Application.Common;
using HRM.Application.ContractManagement.Models;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.ContractManagement.Interfaces;

public interface IContractService
{
    Task<HrLaborContract> CreateContractAsync(CreateContractInput input, CancellationToken ct = default);

    Task<PagedResult<HrLaborContract>> SearchContractsAsync(
        string? search, ContractType? contractType, ContractStatus? status, DateOnly? fromDate, DateOnly? toDate,
        bool expiringSoon, int window, int page, int pageSize, CancellationToken ct = default);

    Task<HrLaborContract> GetContractAsync(int contractId, CancellationToken ct = default);
    Task<HrLaborContract> UpdateContractAsync(int contractId, UpdateContractInput input, CancellationToken ct = default);
    Task DeleteContractAsync(int contractId, CancellationToken ct = default);
    Task<HrLaborContract> ActivateContractAsync(int contractId, CancellationToken ct = default);
    Task<HrLaborContract> ExpireContractAsync(int contractId, CancellationToken ct = default);
    Task<HrLaborContract> TerminateContractAsync(int contractId, DateOnly terminationDate, string terminationReason, CancellationToken ct = default);
}
