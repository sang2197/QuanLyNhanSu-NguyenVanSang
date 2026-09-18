using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HRM.Api.DTOs.Requests.SalaryGradePromotion;
using HRM.Api.DTOs.Responses.SalaryGradePromotion;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Xunit;

namespace HRM.Api.Tests.SalaryGradePromotion;

/// <summary>
/// Exercises the real HTTP pipeline against openapi.yaml's declared
/// contract for the Review Period Employees group. Business-rule edge
/// cases already have thorough coverage in HRM.Application.Tests.
/// </summary>
public class ReviewPeriodEmployeesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ReviewPeriodEmployeesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>Seeds an eligible employee, then creates a review period
    /// through the real POST /review-periods endpoint so the resulting
    /// snapshot row goes through the full DI-wired eligibility calculation
    /// (Sequence 4.1), not a direct DB insert.</summary>
    private async Task<(int periodId, int employeeId)> CreateReviewPeriodWithEligibleEmployeeAsync()
    {
        int employeeId;
        using (var db = _factory.CreateDbContext())
        {
            var unit = new HrOrganizationalUnit { Name = $"Unit-{Guid.NewGuid():N}", UnitType = "Department", Status = ActiveStatus.ACTIVE };
            var jobTitle = new HrJobTitle { Name = $"Title-{Guid.NewGuid():N}", Status = ActiveStatus.ACTIVE };
            db.OrganizationalUnits.Add(unit);
            db.JobTitles.Add(jobTitle);
            db.SaveChanges();

            var scale = new HrSalaryScale { Code = $"NL-{Guid.NewGuid():N}", Name = $"Scale-{Guid.NewGuid():N}", Status = ActiveStatus.ACTIVE };
            db.SalaryScales.Add(scale);
            db.SaveChanges();

            var grade1 = new HrSalaryGrade { SalaryScaleId = scale.Id, GradeNumber = 1, Status = ActiveStatus.ACTIVE };
            var grade2 = new HrSalaryGrade { SalaryScaleId = scale.Id, GradeNumber = 2, Status = ActiveStatus.ACTIVE };
            db.SalaryGrades.AddRange(grade1, grade2);
            db.SaveChanges();

            db.SalaryGradeCoefficients.Add(new HrSalaryGradeCoefficient { SalaryGradeId = grade1.Id, Coefficient = 2.0m, EffectiveDate = new DateOnly(2018, 1, 1) });
            db.SalaryGradeCoefficients.Add(new HrSalaryGradeCoefficient { SalaryGradeId = grade2.Id, Coefficient = 2.3m, EffectiveDate = new DateOnly(2018, 1, 1) });
            db.SaveChanges();

            var employee = new HrEmployee
            {
                EmployeeCode = $"EMP-{Guid.NewGuid():N}",
                FullName = "Eligible Employee",
                OrganizationalUnitId = unit.Id,
                JobTitleId = jobTitle.Id,
                JoinDate = new DateOnly(2018, 1, 1),
                EmploymentStatus = EmploymentStatus.ACTIVE
            };
            db.Employees.Add(employee);
            db.SaveChanges();

            db.EmployeeSalaries.Add(new HrEmployeeSalary
            {
                EmployeeId = employee.Id,
                SalaryGradeId = grade1.Id,
                Coefficient = 2.0m,
                EffectiveDate = new DateOnly(2020, 1, 1),
                Reason = "Initial assignment"
            });
            db.SaveChanges();
            employeeId = employee.Id;
        }

        var request = new CreateReviewPeriodRequest { Code = $"RP-{Guid.NewGuid():N}", Name = $"Annual-{Guid.NewGuid():N}", ReviewType = ReviewType.ANNUAL, ReviewDate = new DateOnly(2026, 3, 15) };
        var response = await _client.PostAsJsonAsync("/review-periods", request, TestJson.Options);
        var period = await response.Content.ReadFromJsonAsync<ReviewPeriodResponse>(TestJson.Options);

        return (period!.Id, employeeId);
    }

    [Fact]
    public async Task GetReviewEmployee_NotFound_Returns404()
    {
        var (periodId, _) = await CreateReviewPeriodWithEligibleEmployeeAsync();

        var response = await _client.GetAsync($"/review-periods/{periodId}/employees/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApproveEmployee_EligibleAndPending_Returns200Approved()
    {
        var (periodId, employeeId) = await CreateReviewPeriodWithEligibleEmployeeAsync();

        var response = await _client.PostAsync($"/review-periods/{periodId}/employees/{employeeId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ReviewPeriodEmployeeResponse>(TestJson.Options);
        body!.Outcome.Should().Be(ReviewOutcome.APPROVED);
    }

    [Fact]
    public async Task RejectEmployee_MissingReason_Returns400()
    {
        var (periodId, employeeId) = await CreateReviewPeriodWithEligibleEmployeeAsync();

        var response = await _client.PostAsJsonAsync($"/review-periods/{periodId}/employees/{employeeId}/reject", new { }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RejectEmployee_ValidReason_Returns200Rejected()
    {
        var (periodId, employeeId) = await CreateReviewPeriodWithEligibleEmployeeAsync();

        var response = await _client.PostAsJsonAsync(
            $"/review-periods/{periodId}/employees/{employeeId}/reject",
            new RejectRequest { Reason = "Not ready this cycle" },
            TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ReviewPeriodEmployeeResponse>(TestJson.Options);
        body!.Outcome.Should().Be(ReviewOutcome.REJECTED);
    }

    [Fact]
    public async Task BulkApprove_ReturnsSucceededEmployeeIds()
    {
        var (periodId, employeeId) = await CreateReviewPeriodWithEligibleEmployeeAsync();

        var response = await _client.PostAsJsonAsync(
            $"/review-periods/{periodId}/employees/bulk-approve",
            new BulkEmployeeActionRequest { EmployeeIds = new List<int> { employeeId } },
            TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<BulkActionResultResponse>(TestJson.Options);
        body!.SucceededEmployeeIds.Should().Contain(employeeId);
    }

    [Fact]
    public async Task ListReviewEmployees_ReturnsOk()
    {
        var (periodId, employeeId) = await CreateReviewPeriodWithEligibleEmployeeAsync();

        var response = await _client.GetAsync($"/review-periods/{periodId}/employees?pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ReviewPeriodEmployeePageResponse>(TestJson.Options);
        // Not ContainSingle(): GetActiveEmployeesAsync (US-SGP-01) sees every
        // active employee in this test class's shared database, including
        // ones seeded by earlier tests in this class, so this period's
        // snapshot can include more than just the one this test seeded.
        body!.Items.Should().Contain(e => e.EmployeeId == employeeId);
    }
}
