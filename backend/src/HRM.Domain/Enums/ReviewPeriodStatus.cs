namespace HRM.Domain.Enums;

/// <summary>
/// There is no Draft status — a successfully created period enters
/// IN_PROGRESS directly, in the same request, once eligibility and
/// proposed grades have been calculated (US-SGP-01). CLOSED and CANCELLED
/// are both terminal.
/// </summary>
public enum ReviewPeriodStatus
{
    IN_PROGRESS,
    SUBMITTED,
    CLOSED,
    CANCELLED
}
