using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

public class HrSalaryDecision
{
    public int Id { get; set; }
    public int ReviewPeriodId { get; set; }
    public string DecisionNumber { get; set; } = null!;
    public DateOnly DecisionDate { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DecisionType DecisionType { get; set; }
    public SalaryDecisionStatus Status { get; set; }
    public int? SignerEmployeeId { get; set; }
    public string? Description { get; set; }
    public string? FileUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HrSalaryReviewPeriod ReviewPeriod { get; set; } = null!;
    public HrEmployee? SignerEmployee { get; set; }
    public ICollection<HrSalaryDecisionDetail> Details { get; set; } = new List<HrSalaryDecisionDetail>();
}
