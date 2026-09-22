using HRM.Domain.Enums;

namespace HRM.Application.ContractManagement.Models;

public record CreateContractInput(
    int EmployeeId,
    ContractType ContractType,
    string ContractNumber,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal ContractSalaryAmount,
    string? SalaryNote,
    ContractStatus Status);
