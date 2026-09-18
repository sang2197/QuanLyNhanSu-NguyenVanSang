using HRM.Domain.Enums;

namespace HRM.Api.DTOs.Responses.SalaryGradePromotion;

public class ReviewPeriodEmployeeResponse
{
    public int ReviewPeriodId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? OrganizationalUnitName { get; set; }
    public int CurrentSalaryGradeId { get; set; }
    public int CurrentGradeNumber { get; set; }
    public decimal CurrentCoefficient { get; set; }
    public bool Eligible { get; set; }
    public int? ProposedSalaryGradeId { get; set; }
    public int? ProposedGradeNumber { get; set; }
    public decimal? ProposedCoefficient { get; set; }
    public string? IneligibleReason { get; set; }
    public ReviewOutcome? Outcome { get; set; }
    public string? RejectionReason { get; set; }
}

public class ReviewPeriodEmployeePageResponse
{
    public IReadOnlyList<ReviewPeriodEmployeeResponse> Items { get; set; } = Array.Empty<ReviewPeriodEmployeeResponse>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
}

public class BulkActionFailureResponse
{
    public int EmployeeId { get; set; }
    public string Reason { get; set; } = null!;
}

public class BulkActionResultResponse
{
    public IReadOnlyList<int> SucceededEmployeeIds { get; set; } = Array.Empty<int>();
    public IReadOnlyList<BulkActionFailureResponse> Failed { get; set; } = Array.Empty<BulkActionFailureResponse>();
}
