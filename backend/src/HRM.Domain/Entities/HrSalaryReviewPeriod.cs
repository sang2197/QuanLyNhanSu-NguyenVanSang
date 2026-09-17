using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

public class HrSalaryReviewPeriod
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public ReviewType ReviewType { get; set; }
    public DateOnly ReviewDate { get; set; }
    public DateOnly? EffectiveDate { get; set; }
    public ReviewPeriodStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<HrSalaryReviewEmployee> ReviewEmployees { get; set; } = new List<HrSalaryReviewEmployee>();
    public ICollection<HrSalaryDecision> Decisions { get; set; } = new List<HrSalaryDecision>();
}
