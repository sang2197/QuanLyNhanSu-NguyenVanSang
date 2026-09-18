namespace HRM.Domain.Enums;

/// <summary>APPLIED and CANCELLED are both terminal — an Applied decision
/// can never be cancelled (US-SGP-07, US-SGP-10).</summary>
public enum SalaryDecisionStatus
{
    DRAFT,
    APPLIED,
    CANCELLED
}
