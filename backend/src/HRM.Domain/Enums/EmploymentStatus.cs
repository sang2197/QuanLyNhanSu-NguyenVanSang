namespace HRM.Domain.Enums;

/// <summary>Whether TERMINATED can return to ACTIVE on the same employee
/// profile is unresolved (OQ-EMP-01) — not modeled as a valid transition.</summary>
public enum EmploymentStatus
{
    ACTIVE,
    ON_LEAVE,
    TERMINATED
}
