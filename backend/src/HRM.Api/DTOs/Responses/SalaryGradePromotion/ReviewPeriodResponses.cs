using HRM.Domain.Enums;

namespace HRM.Api.DTOs.Responses.SalaryGradePromotion;

public class ReviewPeriodResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public ReviewType ReviewType { get; set; }
    public DateOnly ReviewDate { get; set; }
    public DateOnly? EffectiveDate { get; set; }
    public string? Description { get; set; }
    public ReviewPeriodStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ReviewPeriodDetailResponse : ReviewPeriodResponse
{
    public int TotalEmployees { get; set; }
    public int EligibleCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int PendingCount { get; set; }

    /// <summary>The non-cancelled decision already drafted from this period,
    /// if any. The UI shows "Create Decision" when null, "View Decision"
    /// pointing at this id when set (US-SGP-06/09).</summary>
    public int? DecisionId { get; set; }
}

public class ReviewPeriodPageResponse
{
    public IReadOnlyList<ReviewPeriodResponse> Items { get; set; } = Array.Empty<ReviewPeriodResponse>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
}
