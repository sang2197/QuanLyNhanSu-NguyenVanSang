namespace HRM.Domain.Enums;

/// <summary>HR Staff's screening outcome for one employee within a review
/// period (US-SGP-04). PENDING until approved/rejected; not set at all for
/// a not-eligible employee.</summary>
public enum ReviewOutcome
{
    PENDING,
    APPROVED,
    REJECTED
}
