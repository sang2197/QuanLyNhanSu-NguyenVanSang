namespace HRM.Domain.Entities;

public class HrSalaryScale
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<HrSalaryGrade> Grades { get; set; } = new List<HrSalaryGrade>();
    public ICollection<HrEmployeeSalary> EmployeeSalaries { get; set; } = new List<HrEmployeeSalary>();
}
