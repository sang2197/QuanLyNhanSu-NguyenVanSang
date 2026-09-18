namespace HRM.Application.SalaryGradePromotion.Models;

/// <summary>No DecisionType/SignerEmployeeId/Description — none are
/// described by any current User Story/Use Case (see HrSalaryDecision).</summary>
public record CreateSalaryDecisionInput(
    int ReviewPeriodId,
    IReadOnlyList<int> EmployeeIds,
    DateOnly EffectiveDate);
