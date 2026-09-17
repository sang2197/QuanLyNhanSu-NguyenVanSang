using HRM.Domain.Enums;

namespace HRM.Application.SalaryManagement.Models;

public record CreateDecisionInput(
    int ReviewPeriodId,
    IReadOnlyList<int> EmployeeIds,
    string DecisionNumber,
    DecisionType DecisionType,
    DateOnly EffectiveDate,
    int? SignerEmployeeId,
    string? Description);
