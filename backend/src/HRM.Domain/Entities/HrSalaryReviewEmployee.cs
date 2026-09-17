using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

/// <summary>
/// One employee's screening record within one review period — kept
/// separate from the official decision (ADR-05) so proposals can be
/// redone/rejected freely without touching real payroll data.
/// </summary>
public class HrSalaryReviewEmployee
{
    public int Id { get; set; }
    public int ReviewPeriodId { get; set; }
    public int EmployeeId { get; set; }
    public int CurrentSalaryId { get; set; }
    public int CurrentGradeId { get; set; }
    public int? ProposedGradeId { get; set; }
    public decimal? CurrentCoefficient { get; set; }
    public decimal? ProposedCoefficient { get; set; }
    public EligibilityStatus EligibilityStatus { get; set; }
    public string? EligibilityReason { get; set; }
    public ReviewOutcome ReviewStatus { get; set; }
    public string? Reason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HrSalaryReviewPeriod ReviewPeriod { get; set; } = null!;
    public HrEmployee Employee { get; set; } = null!;
    public HrEmployeeSalary CurrentSalary { get; set; } = null!;
    public HrSalaryGrade CurrentGrade { get; set; } = null!;
    public HrSalaryGrade? ProposedGrade { get; set; }
}
