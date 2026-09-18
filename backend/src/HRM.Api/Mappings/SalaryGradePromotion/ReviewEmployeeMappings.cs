using HRM.Api.DTOs.Responses.SalaryGradePromotion;
using HRM.Application.SalaryGradePromotion.Models;
using HRM.Domain.Entities;

namespace HRM.Api.Mappings.SalaryGradePromotion;

public static class ReviewEmployeeMappings
{
    /// <summary>Requires Employee (+ its OrganizationalUnit), CurrentSalaryGrade,
    /// and ProposedSalaryGrade to be loaded — see ReviewEmployeeRepository.</summary>
    public static ReviewPeriodEmployeeResponse ToResponse(this HrSalaryReviewEmployee entry) => new()
    {
        ReviewPeriodId = entry.ReviewPeriodId,
        EmployeeId = entry.EmployeeId,
        EmployeeCode = entry.Employee.EmployeeCode,
        FullName = entry.Employee.FullName,
        OrganizationalUnitName = entry.Employee.OrganizationalUnit?.Name,
        CurrentSalaryGradeId = entry.CurrentSalaryGradeId,
        CurrentGradeNumber = entry.CurrentSalaryGrade.GradeNumber,
        CurrentCoefficient = entry.CurrentCoefficient,
        Eligible = entry.Eligible,
        ProposedSalaryGradeId = entry.ProposedSalaryGradeId,
        ProposedGradeNumber = entry.ProposedSalaryGrade?.GradeNumber,
        ProposedCoefficient = entry.ProposedCoefficient,
        IneligibleReason = entry.IneligibleReason,
        Outcome = entry.Outcome,
        RejectionReason = entry.RejectionReason
    };

    public static BulkActionResultResponse ToResponse(this BulkActionResult result) => new()
    {
        SucceededEmployeeIds = result.SucceededEmployeeIds,
        Failed = result.Failed.Select(f => new BulkActionFailureResponse { EmployeeId = f.EmployeeId, Reason = f.Reason }).ToList()
    };
}
