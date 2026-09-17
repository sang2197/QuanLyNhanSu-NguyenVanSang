namespace HRM.Domain.Enums;

/// <summary>
/// DRAFT is transient (only exists mid-creation); IN_PROGRESS is what a
/// freshly created period is returned as, once proposed grades have been
/// calculated synchronously in the same request (US-01).
/// </summary>
public enum ReviewPeriodStatus
{
    DRAFT,
    IN_PROGRESS,
    SUBMITTED,
    CLOSED,
    CANCELLED
}
