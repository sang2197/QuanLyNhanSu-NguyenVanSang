using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

public class HrSalaryScale
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public ActiveStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<HrSalaryGrade> Grades { get; set; } = new List<HrSalaryGrade>();
}
