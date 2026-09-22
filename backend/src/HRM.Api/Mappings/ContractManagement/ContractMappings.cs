using HRM.Api.DTOs.Responses.ContractManagement;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Api.Mappings.ContractManagement;

public static class ContractMappings
{
    /// <summary>EmployeeCode/EmployeeFullName are populated only when the
    /// caller loaded the Employee navigation (see ContractRepository), same
    /// convention as EmployeeMappings' OrganizationalUnitName/JobTitleName.</summary>
    public static ContractResponse ToResponse(this HrLaborContract contract) => new()
    {
        Id = contract.Id,
        EmployeeId = contract.EmployeeId,
        EmployeeCode = contract.Employee?.EmployeeCode,
        EmployeeFullName = contract.Employee?.FullName,
        ContractNumber = contract.ContractNumber,
        ContractType = contract.ContractType,
        StartDate = contract.StartDate,
        EndDate = contract.EndDate,
        ContractSalaryAmount = contract.ContractSalaryAmount,
        SalaryNote = contract.SalaryNote,
        Status = contract.Status,
        Overdue = IsOverdue(contract),
        TerminationDate = contract.TerminationDate,
        TerminationReason = contract.TerminationReason,
        CreatedAt = contract.CreatedAt,
        UpdatedAt = contract.UpdatedAt
    };

    public static ContractDetailResponse ToDetailResponse(this HrLaborContract contract) => new()
    {
        Id = contract.Id,
        EmployeeId = contract.EmployeeId,
        EmployeeCode = contract.Employee?.EmployeeCode,
        EmployeeFullName = contract.Employee?.FullName,
        ContractNumber = contract.ContractNumber,
        ContractType = contract.ContractType,
        StartDate = contract.StartDate,
        EndDate = contract.EndDate,
        ContractSalaryAmount = contract.ContractSalaryAmount,
        SalaryNote = contract.SalaryNote,
        Status = contract.Status,
        Overdue = IsOverdue(contract),
        TerminationDate = contract.TerminationDate,
        TerminationReason = contract.TerminationReason,
        CreatedAt = contract.CreatedAt,
        UpdatedAt = contract.UpdatedAt,
        OrganizationalUnitName = contract.Employee?.OrganizationalUnit?.Name,
        JobTitleName = contract.Employee?.JobTitle?.Name,
        EmploymentStatus = contract.Employee?.EmploymentStatus
    };

    private static bool IsOverdue(HrLaborContract contract) =>
        contract.Status == ContractStatus.ACTIVE &&
        contract.EndDate is DateOnly end &&
        end < DateOnly.FromDateTime(DateTime.UtcNow);
}
