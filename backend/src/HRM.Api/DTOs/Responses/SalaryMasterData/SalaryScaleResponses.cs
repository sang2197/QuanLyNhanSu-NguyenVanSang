using HRM.Domain.Enums;

namespace HRM.Api.DTOs.Responses.SalaryMasterData;

public class SalaryScaleResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public ActiveStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SalaryGradeResponse
{
    public int Id { get; set; }
    public int SalaryScaleId { get; set; }
    public int GradeNumber { get; set; }

    /// <summary>The coefficient with the latest effectiveDate on or before today.</summary>
    public decimal CurrentCoefficient { get; set; }
    public ActiveStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SalaryScaleDetailResponse : SalaryScaleResponse
{
    public IReadOnlyList<SalaryGradeResponse> Grades { get; set; } = Array.Empty<SalaryGradeResponse>();
}
