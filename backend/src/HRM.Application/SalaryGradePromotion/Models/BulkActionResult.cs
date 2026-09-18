namespace HRM.Application.SalaryGradePromotion.Models;

/// <summary>
/// US-SGP-04 — bulk approve/reject checks each employee individually; only
/// the valid ones go through, and the caller is told how many succeeded/
/// failed and why.
/// </summary>
public class BulkActionResult
{
    public List<int> SucceededEmployeeIds { get; } = new();
    public List<BulkActionFailure> Failed { get; } = new();
}

public record BulkActionFailure(int EmployeeId, string Reason);
