using HRM.Domain.Entities;

namespace HRM.Application.ContractManagement.Interfaces;

public interface IContractRepository
{
    Task<HrLaborContract?> GetByIdAsync(int contractId, CancellationToken ct = default);
    Task<HrLaborContract?> GetByNumberAsync(string contractNumber, CancellationToken ct = default);

    /// <summary>For SearchContracts (BR-CON-11–14, BR-CON-25–27, BR-CON-32) —
    /// the service composes .Where() clauses for search/type/status/date-range/
    /// expiringSoon on top of this.</summary>
    IQueryable<HrLaborContract> Query();

    Task AddAsync(HrLaborContract contract, CancellationToken ct = default);
    Task RemoveAsync(HrLaborContract contract, CancellationToken ct = default);

    /// <summary>Backs BR-CON-10/18 — an employee can have at most one Active
    /// contract. exceptContractId excludes the contract being activated from
    /// its own check.</summary>
    Task<bool> HasOtherActiveForEmployeeAsync(int employeeId, int? exceptContractId, CancellationToken ct = default);
}
