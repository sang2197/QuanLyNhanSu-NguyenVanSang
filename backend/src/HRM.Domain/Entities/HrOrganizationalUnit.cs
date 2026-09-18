using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

public class HrOrganizationalUnit
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int? ParentId { get; set; }
    public string UnitType { get; set; } = null!;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public ActiveStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public HrOrganizationalUnit? Parent { get; set; }
    public ICollection<HrOrganizationalUnit> Children { get; set; } = new List<HrOrganizationalUnit>();
    public ICollection<HrEmployee> Employees { get; set; } = new List<HrEmployee>();
}
