using HRM.Domain.Enums;

namespace HRM.Api.DTOs.Responses;

public class SalaryDecisionResponse
{
    public int Id { get; set; }
    public string DecisionNumber { get; set; } = null!;
    public int ReviewPeriodId { get; set; }
    public DateOnly DecisionDate { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DecisionType DecisionType { get; set; }
    public SalaryDecisionStatus Status { get; set; }
    public int? SignerEmployeeId { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SalaryDecisionEmployeeDetailResponse
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? OldGrade { get; set; }
    public decimal? OldCoefficient { get; set; }
    public string NewGrade { get; set; } = null!;
    public decimal NewCoefficient { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public string? Reason { get; set; }
}

public class SalaryDecisionDetailResponse : SalaryDecisionResponse
{
    public IReadOnlyList<SalaryDecisionEmployeeDetailResponse> Employees { get; set; } = Array.Empty<SalaryDecisionEmployeeDetailResponse>();
}

public class SalaryDecisionPageResponse
{
    public IReadOnlyList<SalaryDecisionResponse> Items { get; set; } = Array.Empty<SalaryDecisionResponse>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
}
