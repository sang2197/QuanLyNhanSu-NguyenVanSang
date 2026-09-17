using HRM.Application.Common;
using HRM.Application.SalaryManagement.Models;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.SalaryManagement.Interfaces;

public interface ISalaryDecisionService
{
    Task<HrSalaryDecision> CreateDecisionAsync(CreateDecisionInput input, CancellationToken ct = default);
    Task<PagedResult<HrSalaryDecision>> SearchDecisionsAsync(
        SalaryDecisionStatus? status, int? reviewPeriodId, int page, int pageSize, CancellationToken ct = default);
    Task<HrSalaryDecision> GetDecisionAsync(int decisionId, CancellationToken ct = default);
    Task RemoveEmployeeAsync(int decisionId, int employeeId, CancellationToken ct = default);
    Task<HrSalaryDecision> ApplyDecisionAsync(int decisionId, CancellationToken ct = default);
    Task<HrSalaryDecision> CancelDecisionAsync(int decisionId, CancellationToken ct = default);
}
