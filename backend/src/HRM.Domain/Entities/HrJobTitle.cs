using HRM.Domain.Enums;

namespace HRM.Domain.Entities;

public class HrJobTitle
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ActiveStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<HrEmployee> Employees { get; set; } = new List<HrEmployee>();
}
