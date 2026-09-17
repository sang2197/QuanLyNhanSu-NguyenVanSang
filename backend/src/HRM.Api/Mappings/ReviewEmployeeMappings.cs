using HRM.Api.DTOs.Responses;
using HRM.Application.SalaryManagement.Models;
using HRM.Domain.Entities;

namespace HRM.Api.Mappings;

public static class ReviewEmployeeMappings
{
    public static ReviewPeriodEmployeeResponse ToResponse(this HrSalaryReviewEmployee entry) => new()
    {
        EmployeeId = entry.EmployeeId,
        EmployeeCode = entry.Employee.EmployeeCode,
        FullName = entry.Employee.FullName,
        Department = entry.Employee.DepartmentId,
        CurrentGrade = entry.CurrentGrade.GradeNumber.ToString(),
        CurrentCoefficient = entry.CurrentCoefficient,
        ProposedGrade = entry.ProposedGrade?.GradeNumber.ToString(),
        ProposedCoefficient = entry.ProposedCoefficient,
        EligibilityStatus = entry.EligibilityStatus,
        EligibilityReason = entry.EligibilityReason,
        ReviewOutcome = entry.ReviewStatus,
        Reason = entry.Reason
    };

    public static BulkActionResultResponse ToResponse(this BulkActionResult result) => new()
    {
        SucceededEmployeeIds = result.SucceededEmployeeIds,
        Failed = result.Failed.Select(f => new BulkActionFailureResponse { EmployeeId = f.EmployeeId, Reason = f.Reason }).ToList()
    };
}
