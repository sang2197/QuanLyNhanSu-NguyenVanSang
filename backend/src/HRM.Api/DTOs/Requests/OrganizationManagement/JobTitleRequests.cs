using System.ComponentModel.DataAnnotations;

namespace HRM.Api.DTOs.Requests.OrganizationManagement;

public class CreateJobTitleRequest
{
    [Required] public string Name { get; set; } = null!;
}

public class UpdateJobTitleRequest
{
    [Required] public string Name { get; set; } = null!;
}
