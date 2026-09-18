using HRM.Api.DTOs.Responses.SalaryMasterData;
using HRM.Domain.Entities;

namespace HRM.Api.Mappings.SalaryMasterData;

public static class SalaryScaleMappings
{
    public static SalaryScaleResponse ToResponse(this HrSalaryScale scale) => new()
    {
        Id = scale.Id,
        Code = scale.Code,
        Name = scale.Name,
        Status = scale.Status,
        CreatedAt = scale.CreatedAt,
        UpdatedAt = scale.UpdatedAt
    };

    /// <summary>Requires scale.Grades (and each grade's Coefficients) to be
    /// loaded — see SalaryScaleRepository.GetDetailAsync.</summary>
    public static SalaryScaleDetailResponse ToDetailResponse(this HrSalaryScale scale)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var basic = scale.ToResponse();
        return new SalaryScaleDetailResponse
        {
            Id = basic.Id,
            Code = basic.Code,
            Name = basic.Name,
            Status = basic.Status,
            CreatedAt = basic.CreatedAt,
            UpdatedAt = basic.UpdatedAt,
            Grades = scale.Grades
                .Select(g => g.ToResponse(CurrentCoefficient(g, today)))
                .ToList()
        };
    }

    public static SalaryGradeResponse ToResponse(this HrSalaryGrade grade, decimal currentCoefficient) => new()
    {
        Id = grade.Id,
        SalaryScaleId = grade.SalaryScaleId,
        GradeNumber = grade.GradeNumber,
        CurrentCoefficient = currentCoefficient,
        Status = grade.Status,
        CreatedAt = grade.CreatedAt,
        UpdatedAt = grade.UpdatedAt
    };

    private static decimal CurrentCoefficient(HrSalaryGrade grade, DateOnly asOf) =>
        grade.Coefficients
            .Where(c => c.EffectiveDate <= asOf)
            .OrderByDescending(c => c.EffectiveDate)
            .FirstOrDefault()?.Coefficient ?? 0;
}
