using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

/// <summary>
/// One employee's per-period screening snapshot — eligibility, current/
/// proposed grade, and the ineligibility reason are frozen at creation and
/// never recalculated afterward (UC-SGP-01/03), even if the employee's
/// real salary or Salary Master Data changes later. Kept separate from the
/// official decision (ADR-05) so proposals can be redone/rejected freely
/// without touching real payroll data. No ApprovedBy — actor identity
/// belongs to Identity &amp; Access Management, out of scope here.
/// </summary>
public class HrSalaryReviewEmployee
{
    public int Id { get; set; }
    public int ReviewPeriodId { get; set; }
    public int EmployeeId { get; set; }
    public int CurrentSalaryGradeId { get; set; }
    public decimal CurrentCoefficient { get; set; }
    public bool Eligible { get; set; }
    public int? ProposedSalaryGradeId { get; set; }
    public decimal? ProposedCoefficient { get; set; }
    public string? IneligibleReason { get; set; }
    public ReviewOutcome? Outcome { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HrSalaryReviewPeriod ReviewPeriod { get; set; } = null!;
    public HrEmployee Employee { get; set; } = null!;
    public HrSalaryGrade CurrentSalaryGrade { get; set; } = null!;
    public HrSalaryGrade? ProposedSalaryGrade { get; set; }
}
