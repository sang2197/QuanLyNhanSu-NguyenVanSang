namespace HRM.Api.DTOs.Responses.SalaryGradePromotion;

public class SalaryHistoryEntryResponse
{
    public int SalaryGradeId { get; set; }
    public int GradeNumber { get; set; }
    public decimal Coefficient { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public string Reason { get; set; } = null!;
    public int? SalaryDecisionId { get; set; }
    public string? DecisionNumber { get; set; }
    public bool IsCurrent { get; set; }
}

public class SalaryHistoryPageResponse
{
    public int EmployeeId { get; set; }
    public IReadOnlyList<SalaryHistoryEntryResponse> Items { get; set; } = Array.Empty<SalaryHistoryEntryResponse>();
}
