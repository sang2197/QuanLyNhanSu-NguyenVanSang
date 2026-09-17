using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HRM.Api.DTOs.Requests;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Xunit;

namespace HRM.Api.Tests;

public class SalaryDecisionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SalaryDecisionsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private (int periodId, int employeeId) SeedSubmittedPeriodWithApprovedEmployee()
    {
        using var db = _factory.CreateDbContext();
        var scale = new HrSalaryScale { Code = $"SCALE-{Guid.NewGuid():N}", Name = "Scale A", EffectiveFrom = new DateOnly(2020, 1, 1) };
        db.SalaryScales.Add(scale);
        db.SaveChanges();

        var grade = new HrSalaryGrade { SalaryScaleId = scale.Id, GradeNumber = 1, Coefficient = 2.0m, EffectiveFrom = new DateOnly(2020, 1, 1) };
        var nextGrade = new HrSalaryGrade { SalaryScaleId = scale.Id, GradeNumber = 2, Coefficient = 2.3m, EffectiveFrom = new DateOnly(2020, 1, 1) };
        db.SalaryGrades.AddRange(grade, nextGrade);
        db.SaveChanges();

        var employee = new HrEmployee { EmployeeCode = $"EMP-{Guid.NewGuid():N}", FullName = "Approved Employee" };
        db.Employees.Add(employee);
        db.SaveChanges();

        var salary = new HrEmployeeSalary { EmployeeId = employee.Id, SalaryScaleId = scale.Id, SalaryGradeId = grade.Id, Coefficient = grade.Coefficient, EffectiveFrom = new DateOnly(2020, 1, 1) };
        db.EmployeeSalaries.Add(salary);
        db.SaveChanges();

        var period = new HrSalaryReviewPeriod
        {
            Code = $"RP-{Guid.NewGuid():N}",
            Name = "Submitted period",
            ReviewType = ReviewType.PERIODIC,
            ReviewDate = new DateOnly(2026, 3, 15),
            Status = ReviewPeriodStatus.SUBMITTED
        };
        db.ReviewPeriods.Add(period);
        db.SaveChanges();

        db.ReviewEmployees.Add(new HrSalaryReviewEmployee
        {
            ReviewPeriodId = period.Id,
            EmployeeId = employee.Id,
            CurrentSalaryId = salary.Id,
            CurrentGradeId = grade.Id,
            ProposedGradeId = nextGrade.Id,
            CurrentCoefficient = grade.Coefficient,
            ProposedCoefficient = nextGrade.Coefficient,
            EligibilityStatus = EligibilityStatus.ELIGIBLE,
            ReviewStatus = ReviewOutcome.APPROVED
        });
        db.SaveChanges();

        return (period.Id, employee.Id);
    }

    [Fact]
    public async Task CreateDecision_ApprovedEmployee_Returns201_AsDraft()
    {
        var (periodId, employeeId) = SeedSubmittedPeriodWithApprovedEmployee();
        var request = new CreateSalaryDecisionRequest
        {
            ReviewPeriodId = periodId,
            EmployeeIds = new List<int> { employeeId },
            DecisionNumber = $"SD-{Guid.NewGuid():N}",
            DecisionType = DecisionType.PERIODIC,
            EffectiveDate = new DateOnly(2026, 4, 1)
        };

        var response = await _client.PostAsJsonAsync("/salary-decisions", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateDecision_PeriodNotSubmitted_Returns409()
    {
        using var db = _factory.CreateDbContext();
        var period = new HrSalaryReviewPeriod
        {
            Code = $"RP-{Guid.NewGuid():N}",
            Name = "Still in progress",
            ReviewType = ReviewType.PERIODIC,
            ReviewDate = new DateOnly(2026, 3, 15),
            Status = ReviewPeriodStatus.IN_PROGRESS
        };
        db.ReviewPeriods.Add(period);
        db.SaveChanges();

        var request = new CreateSalaryDecisionRequest
        {
            ReviewPeriodId = period.Id,
            EmployeeIds = new List<int> { 1 },
            DecisionNumber = $"SD-{Guid.NewGuid():N}",
            DecisionType = DecisionType.PERIODIC,
            EffectiveDate = new DateOnly(2026, 4, 1)
        };

        var response = await _client.PostAsJsonAsync("/salary-decisions", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateDecision_ReviewPeriodNotFound_Returns404()
    {
        var request = new CreateSalaryDecisionRequest
        {
            ReviewPeriodId = 999999,
            EmployeeIds = new List<int> { 1 },
            DecisionNumber = $"SD-{Guid.NewGuid():N}",
            DecisionType = DecisionType.PERIODIC,
            EffectiveDate = new DateOnly(2026, 4, 1)
        };

        var response = await _client.PostAsJsonAsync("/salary-decisions", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDecision_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/salary-decisions/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelDecision_NotFound_Returns404()
    {
        var response = await _client.PostAsync("/salary-decisions/999999/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApplyDecision_NotFound_Returns404()
    {
        var response = await _client.PostAsync("/salary-decisions/999999/apply", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
