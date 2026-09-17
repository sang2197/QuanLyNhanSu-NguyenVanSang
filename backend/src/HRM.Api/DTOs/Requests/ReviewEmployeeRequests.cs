using System.ComponentModel.DataAnnotations;

namespace HRM.Api.DTOs.Requests;

public class RejectRequest
{
    [Required] public string Reason { get; set; } = null!;
}

public class BulkEmployeeActionRequest
{
    [Required, MinLength(1)] public List<int> EmployeeIds { get; set; } = new();
}

public class BulkRejectRequest : BulkEmployeeActionRequest
{
    [Required] public string Reason { get; set; } = null!;
}
