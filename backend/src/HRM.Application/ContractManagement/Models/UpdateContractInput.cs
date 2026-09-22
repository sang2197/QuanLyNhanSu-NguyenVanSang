using HRM.Domain.Enums;

namespace HRM.Application.ContractManagement.Models;

public record UpdateContractInput(
    ContractType ContractType,
    string ContractNumber,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal ContractSalaryAmount,
    string? SalaryNote);
