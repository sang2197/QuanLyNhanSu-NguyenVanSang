using HRM.Domain.Entities;

namespace HRM.Application.SalaryManagement.Rules;

public record EligibilityResult(bool IsEligible, string? Reason, HrSalaryGrade? ProposedGrade)
{
    public static EligibilityResult Eligible(HrSalaryGrade proposedGrade) => new(true, null, proposedGrade);
    public static EligibilityResult Ineligible(string reason) => new(false, reason, null);
}
