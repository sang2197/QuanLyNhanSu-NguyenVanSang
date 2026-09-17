namespace HRM.Application.SalaryManagement.Models;

/// <summary>One row of an employee's salary timeline (US-08), newest first.</summary>
public record SalaryHistoryEntryResult(
    string Grade,
    decimal Coefficient,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string? Reason,
    int? DecisionId,
    string? DecisionNumber,
    bool IsCurrent);
