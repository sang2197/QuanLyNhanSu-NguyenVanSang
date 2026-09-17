using HRM.Domain.Enums;

namespace HRM.Api.DTOs.Responses;

public class ReviewPeriodEmployeeResponse
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int? Department { get; set; }
    public string CurrentGrade { get; set; } = null!;
    public decimal? CurrentCoefficient { get; set; }
    public string? ProposedGrade { get; set; }
    public decimal? ProposedCoefficient { get; set; }
    public EligibilityStatus EligibilityStatus { get; set; }
    public string? EligibilityReason { get; set; }
    public ReviewOutcome ReviewOutcome { get; set; }
    public string? Reason { get; set; }
}

public class ReviewPeriodEmployeePageResponse
{
    public IReadOnlyList<ReviewPeriodEmployeeResponse> Items { get; set; } = Array.Empty<ReviewPeriodEmployeeResponse>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
}

public class SalaryHistoryEntrySummaryResponse
{
    public string Grade { get; set; } = null!;
    public decimal Coefficient { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}

public class ReviewPeriodEmployeeDetailResponse : ReviewPeriodEmployeeResponse
{
    public DateTime SalarySnapshotAt { get; set; }
    public IReadOnlyList<SalaryHistoryEntrySummaryResponse> RecentSalaryHistory { get; set; } = Array.Empty<SalaryHistoryEntrySummaryResponse>();
}

public class BulkActionResultResponse
{
    public IReadOnlyList<int> SucceededEmployeeIds { get; set; } = Array.Empty<int>();
    public IReadOnlyList<BulkActionFailureResponse> Failed { get; set; } = Array.Empty<BulkActionFailureResponse>();
}

public class BulkActionFailureResponse
{
    public int EmployeeId { get; set; }
    public string Reason { get; set; } = null!;
}
