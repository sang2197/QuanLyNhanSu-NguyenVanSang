namespace HRM.Domain.Enums;

/// <summary>Shared Active/Inactive toggle used by OrganizationalUnit, JobTitle,
/// SalaryScale, and SalaryGrade — each has its own deactivate/reactivate
/// guard rather than a shared one.</summary>
public enum ActiveStatus
{
    ACTIVE,
    INACTIVE
}
