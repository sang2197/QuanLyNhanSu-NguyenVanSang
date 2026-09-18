using HRM.Domain.Enums;

namespace HRM.Api.DTOs.Responses.SalaryGradePromotion;

public class SalaryDecisionResponse
{
    public int Id { get; set; }
    public string DecisionNumber { get; set; } = null!;
    public int ReviewPeriodId { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public SalaryDecisionStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SalaryDecisionEmployeeLineResponse
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int BaselineSalaryGradeId { get; set; }
    public int BaselineGradeNumber { get; set; }
    public decimal BaselineCoefficient { get; set; }
    public int NewSalaryGradeId { get; set; }
    public int NewGradeNumber { get; set; }
    public decimal NewCoefficient { get; set; }
}

public class SalaryDecisionDetailResponse : SalaryDecisionResponse
{
    public IReadOnlyList<SalaryDecisionEmployeeLineResponse> Employees { get; set; } = Array.Empty<SalaryDecisionEmployeeLineResponse>();
}

public class SalaryDecisionPageResponse
{
    public IReadOnlyList<SalaryDecisionResponse> Items { get; set; } = Array.Empty<SalaryDecisionResponse>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
}
