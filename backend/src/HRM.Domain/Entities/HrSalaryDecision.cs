using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

/// <summary>
/// At most one non-cancelled decision may exist per review period at a
/// time (US-SGP-06) — enforced by a filtered unique index, not a plain
/// one, since a period may accumulate several cancelled-then-replaced
/// decisions over its lifetime (US-SGP-10 AC04). No DecisionType,
/// Description, FileUrl, or SignerEmployeeId — none are described by any
/// current User Story/Use Case; actor identity belongs to Identity &amp;
/// Access Management, out of scope here.
/// </summary>
public class HrSalaryDecision
{
    public int Id { get; set; }
    public int ReviewPeriodId { get; set; }
    public string DecisionNumber { get; set; } = null!;
    public DateOnly EffectiveDate { get; set; }
    public SalaryDecisionStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HrSalaryReviewPeriod ReviewPeriod { get; set; } = null!;
    public ICollection<HrSalaryDecisionDetail> Details { get; set; } = new List<HrSalaryDecisionDetail>();
}
