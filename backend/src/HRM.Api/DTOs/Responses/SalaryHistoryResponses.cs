namespace HRM.Api.DTOs.Responses;

public class SalaryHistoryEntryResponse
{
    public string Grade { get; set; } = null!;
    public decimal Coefficient { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string? Reason { get; set; }
    public int? DecisionId { get; set; }
    public string? DecisionNumber { get; set; }
    public bool IsCurrent { get; set; }
}

public class SalaryHistoryPageResponse
{
    public int EmployeeId { get; set; }
    public IReadOnlyList<SalaryHistoryEntryResponse> Items { get; set; } = Array.Empty<SalaryHistoryEntryResponse>();
}
