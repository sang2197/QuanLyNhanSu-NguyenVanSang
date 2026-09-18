using System.ComponentModel.DataAnnotations;

namespace HRM.Api.DTOs.Requests.SalaryMasterData;

public class CreateSalaryScaleRequest
{
    [Required] public string Code { get; set; } = null!;
    [Required] public string Name { get; set; } = null!;
}

public class UpdateSalaryScaleRequest
{
    [Required] public string Name { get; set; } = null!;
}

public class CreateSalaryGradeRequest
{
    [Required] public int GradeNumber { get; set; }
    [Required] public decimal Coefficient { get; set; }
}
