using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.SalaryManagement.Interfaces;

public interface ISalaryRepository
{
    // Review Periods
    Task<HrSalaryReviewPeriod?> GetReviewPeriodAsync(int periodId, CancellationToken ct = default);
    Task<HrSalaryReviewPeriod?> GetReviewPeriodByCodeAsync(string code, CancellationToken ct = default);
    IQueryable<HrSalaryReviewPeriod> QueryReviewPeriods();
    Task AddReviewPeriodAsync(HrSalaryReviewPeriod period, CancellationToken ct = default);
    Task<bool> HasNonCancelledDecisionAsync(int reviewPeriodId, CancellationToken ct = default);

    // Review Employees (screening records within a period)
    Task<HrSalaryReviewEmployee?> GetReviewEmployeeAsync(int periodId, int employeeId, CancellationToken ct = default);
    IQueryable<HrSalaryReviewEmployee> QueryReviewEmployees(int periodId);
    Task AddReviewEmployeesAsync(IEnumerable<HrSalaryReviewEmployee> entries, CancellationToken ct = default);

    // Salary grades (for eligibility calculation — the grade above the current one, in-scale)
    Task<IReadOnlyList<HrSalaryGrade>> GetGradesForScaleAsync(int salaryScaleId, CancellationToken ct = default);

    // Salary Decisions
    Task<HrSalaryDecision?> GetDecisionAsync(int decisionId, CancellationToken ct = default);
    IQueryable<HrSalaryDecision> QueryDecisions();
    Task AddDecisionAsync(HrSalaryDecision decision, CancellationToken ct = default);
    Task RemoveDecisionDetailAsync(HrSalaryDecisionDetail detail, CancellationToken ct = default);

    // Employee salary (effective-dated, ADR-02)
    Task<HrEmployeeSalary?> GetCurrentSalaryAsync(int employeeId, CancellationToken ct = default);
    Task<bool> HasEffectiveDateConflictAsync(int employeeId, DateOnly effectiveFrom, CancellationToken ct = default);
    Task CloseSalaryAsync(HrEmployeeSalary salary, DateOnly effectiveTo, CancellationToken ct = default);
    Task AddSalaryAsync(HrEmployeeSalary salary, CancellationToken ct = default);
    IQueryable<HrEmployeeSalary> QuerySalaryHistory(int employeeId);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
