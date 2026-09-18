using HRM.Domain.Enums;

namespace HRM.Api.DTOs.Responses.OrganizationManagement;

public class JobTitleResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ActiveStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
