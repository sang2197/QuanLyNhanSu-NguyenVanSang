using HRM.Application.Common;
using HRM.Application.ContractManagement.Interfaces;
using HRM.Application.ContractManagement.Models;
using HRM.Application.EmployeeManagement.Interfaces;
using HRM.Application.Exceptions;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.ContractManagement.Services;

/// <inheritdoc cref="IContractService"/>
public class ContractService : IContractService
{
    private readonly IContractRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmployeeService _employeeService;

    public ContractService(IContractRepository repository, IUnitOfWork unitOfWork, IEmployeeService employeeService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _employeeService = employeeService;
    }

    public async Task<HrLaborContract> CreateContractAsync(CreateContractInput input, CancellationToken ct = default)
    {
        if (await _repository.GetByNumberAsync(input.ContractNumber, ct) is not null)
        {
            throw new ConflictException("A contract with this number already exists."); // BR-CON-01
        }

        var employee = await _employeeService.GetEmployeeAsync(input.EmployeeId, ct);
        if (employee.EmploymentStatus == EmploymentStatus.TERMINATED)
        {
            throw new ConflictException("A contract cannot be created for a Terminated employee."); // BR-CON-07
        }

        ValidateContractFields(input.ContractType, input.StartDate, input.EndDate, input.ContractSalaryAmount);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (input.Status == ContractStatus.ACTIVE)
        {
            if (input.StartDate > today)
            {
                throw new ConflictException("A contract cannot be saved as Active before its start date."); // BR-CON-09
            }
            if (await _repository.HasOtherActiveForEmployeeAsync(input.EmployeeId, exceptContractId: null, ct))
            {
                throw new ConflictException("This employee already has another Active contract."); // BR-CON-10
            }
        }

        var contract = new HrLaborContract
        {
            EmployeeId = input.EmployeeId,
            ContractType = input.ContractType,
            ContractNumber = input.ContractNumber,
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            ContractSalaryAmount = input.ContractSalaryAmount,
            SalaryNote = input.SalaryNote,
            Status = input.Status,
            Employee = employee // already loaded with OrganizationalUnit/JobTitle — hydrates the response immediately, same as EF fixup
        };
        await _repository.AddAsync(contract, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return contract;
    }

    // HRM.Application has no EF Core reference, so IQueryable<T> from the
    // repository is composed and materialized here with plain synchronous
    // LINQ — same convention as EmployeeService.SearchEmployeesAsync.
    public Task<PagedResult<HrLaborContract>> SearchContractsAsync(
        string? search, ContractType? contractType, ContractStatus? status, DateOnly? fromDate, DateOnly? toDate,
        bool expiringSoon, int window, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _repository.Query();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                c.ContractNumber.Contains(search) ||
                c.Employee.EmployeeCode.Contains(search) ||
                c.Employee.FullName.Contains(search)); // BR-CON-11
        }
        if (contractType is ContractType type)
        {
            query = query.Where(c => c.ContractType == type); // BR-CON-12
        }
        if (status is ContractStatus s)
        {
            query = query.Where(c => c.Status == s); // BR-CON-12
        }
        // Time-period overlap: a contract with no end date runs without limit (BR-CON-13).
        if (toDate is DateOnly to)
        {
            query = query.Where(c => c.StartDate <= to);
        }
        if (fromDate is DateOnly from)
        {
            query = query.Where(c => c.EndDate == null || c.EndDate >= from);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (expiringSoon)
        {
            // Active contracts ending within `window` days from today, plus
            // overdue ones (end date already passed) — a single upper-bound
            // condition covers both (BR-CON-20, BR-CON-25–27).
            var horizon = today.AddDays(window);
            query = query.Where(c => c.Status == ContractStatus.ACTIVE && c.EndDate != null && c.EndDate <= horizon);
        }

        var totalItems = query.Count();
        var ordered = expiringSoon
            ? query.OrderBy(c => c.EndDate) // BR-CON-27
            : query.OrderBy(c => c.ContractNumber);
        var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<HrLaborContract> { Items = items, Page = page, PageSize = pageSize, TotalItems = totalItems });
    }

    public async Task<HrLaborContract> GetContractAsync(int contractId, CancellationToken ct = default) =>
        await GetOrThrowAsync(contractId, ct);

    public async Task<HrLaborContract> UpdateContractAsync(int contractId, UpdateContractInput input, CancellationToken ct = default)
    {
        var contract = await GetOrThrowAsync(contractId, ct);
        if (contract.Status != ContractStatus.DRAFT)
        {
            throw new ConflictException("Only a Draft contract can be updated."); // BR-CON-28
        }

        if (input.ContractNumber != contract.ContractNumber)
        {
            if (await _repository.GetByNumberAsync(input.ContractNumber, ct) is not null)
            {
                throw new ConflictException("A contract with this number already exists."); // BR-CON-01, BR-CON-30
            }
        }

        ValidateContractFields(input.ContractType, input.StartDate, input.EndDate, input.ContractSalaryAmount);

        // BR-CON-29/30: employee and status are not editable here; the
        // tracked entity is mutated in place, same as UpdateEmployeeAsync.
        contract.ContractType = input.ContractType;
        contract.ContractNumber = input.ContractNumber;
        contract.StartDate = input.StartDate;
        contract.EndDate = input.EndDate;
        contract.ContractSalaryAmount = input.ContractSalaryAmount;
        contract.SalaryNote = input.SalaryNote;

        await _unitOfWork.SaveChangesAsync(ct);
        return contract;
    }

    public async Task DeleteContractAsync(int contractId, CancellationToken ct = default)
    {
        var contract = await GetOrThrowAsync(contractId, ct);
        if (contract.Status != ContractStatus.DRAFT)
        {
            throw new ConflictException("Only a Draft contract can be deleted."); // BR-CON-28
        }

        // The only hard delete in the whole system — a Draft contract has no
        // dependent history yet, so its number becomes reusable (BR-CON-31).
        await _repository.RemoveAsync(contract, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<HrLaborContract> ActivateContractAsync(int contractId, CancellationToken ct = default)
    {
        var contract = await GetOrThrowAsync(contractId, ct);
        if (contract.Status != ContractStatus.DRAFT)
        {
            throw new ConflictException("Only a Draft contract can be activated."); // BR-CON-16
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (contract.StartDate > today)
        {
            throw new ConflictException("This contract's start date has not been reached yet."); // BR-CON-18
        }
        if (await _repository.HasOtherActiveForEmployeeAsync(contract.EmployeeId, contract.Id, ct))
        {
            throw new ConflictException("This employee already has another Active contract."); // BR-CON-18
        }

        var employee = await _employeeService.GetEmployeeAsync(contract.EmployeeId, ct);
        if (employee.EmploymentStatus == EmploymentStatus.TERMINATED)
        {
            throw new ConflictException("This contract cannot be activated because the employee is Terminated."); // BR-CON-18
        }

        contract.Status = ContractStatus.ACTIVE;
        await _unitOfWork.SaveChangesAsync(ct);
        return contract;
    }

    public async Task<HrLaborContract> ExpireContractAsync(int contractId, CancellationToken ct = default)
    {
        var contract = await GetOrThrowAsync(contractId, ct);
        if (contract.Status != ContractStatus.ACTIVE)
        {
            throw new ConflictException("Only an Active contract can be marked as Expired."); // BR-CON-16
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (contract.EndDate is null || contract.EndDate >= today)
        {
            throw new ConflictException("This contract's end date has not passed yet."); // BR-CON-19
        }

        contract.Status = ContractStatus.EXPIRED;
        await _unitOfWork.SaveChangesAsync(ct);
        return contract;
    }

    public async Task<HrLaborContract> TerminateContractAsync(int contractId, DateOnly terminationDate, string terminationReason, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(terminationReason))
        {
            throw new ValidationException("Termination reason is required."); // BR-CON-21
        }

        var contract = await GetOrThrowAsync(contractId, ct);
        if (contract.Status != ContractStatus.ACTIVE)
        {
            throw new ConflictException("Only an Active contract can be terminated."); // BR-CON-16
        }
        if (terminationDate < contract.StartDate || (contract.EndDate is DateOnly end && terminationDate > end))
        {
            throw new ConflictException("Termination date must be within the contract term."); // BR-CON-22
        }

        contract.Status = ContractStatus.TERMINATED;
        contract.TerminationDate = terminationDate;
        contract.TerminationReason = terminationReason;
        await _unitOfWork.SaveChangesAsync(ct);
        return contract;
    }

    private static void ValidateContractFields(ContractType contractType, DateOnly startDate, DateOnly? endDate, decimal contractSalaryAmount)
    {
        var endDateRequired = contractType != ContractType.INDEFINITE_TERM;
        if (endDateRequired && endDate is null)
        {
            throw new ValidationException("End date is required for Probation and Fixed-Term contracts."); // BR-CON-04
        }
        if (!endDateRequired && endDate is not null)
        {
            throw new ValidationException("End date must not be set for an Indefinite-Term contract."); // BR-CON-04
        }
        if (endDate is DateOnly end && end <= startDate)
        {
            throw new ValidationException("End date must be later than the start date."); // BR-CON-05
        }
        if (contractSalaryAmount <= 0)
        {
            throw new ValidationException("Contract salary amount must be greater than zero."); // BR-CON-06
        }
    }

    private async Task<HrLaborContract> GetOrThrowAsync(int contractId, CancellationToken ct) =>
        await _repository.GetByIdAsync(contractId, ct)
            ?? throw new NotFoundException($"Contract {contractId} was not found.");
}
