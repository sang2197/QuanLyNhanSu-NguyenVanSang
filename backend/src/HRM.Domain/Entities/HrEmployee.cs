using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

public class HrEmployee
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int OrganizationalUnitId { get; set; }
    public int JobTitleId { get; set; }
    public DateOnly JoinDate { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HrOrganizationalUnit OrganizationalUnit { get; set; } = null!;
    public HrJobTitle JobTitle { get; set; } = null!;
    public ICollection<HrEmployeeSalary> Salaries { get; set; } = new List<HrEmployeeSalary>();
    public ICollection<HrSalaryReviewEmployee> ReviewEntries { get; set; } = new List<HrSalaryReviewEmployee>();
    public ICollection<HrSalaryDecisionDetail> DecisionDetails { get; set; } = new List<HrSalaryDecisionDetail>();
}
