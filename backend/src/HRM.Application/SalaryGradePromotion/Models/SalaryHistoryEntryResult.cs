namespace HRM.Application.SalaryGradePromotion.Models;

/// <summary>One row of an employee's salary timeline (US-SGP-08), newest first.</summary>
public record SalaryHistoryEntryResult(
    int SalaryGradeId,
    int GradeNumber,
    decimal Coefficient,
    DateOnly EffectiveDate,
    string Reason,
    int? SalaryDecisionId,
    string? DecisionNumber,
    bool IsCurrent);
