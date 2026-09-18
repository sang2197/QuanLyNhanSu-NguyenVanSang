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
/// contract for the Salary Decisions group. Business-rule edge cases
/// already have thorough coverage in HRM.Application.Tests.
/// </summary>
public class SalaryDecisionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private static readonly DateOnly ReviewDate = new(2026, 3, 15);

    public SalaryDecisionsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>Seeds an eligible employee, then drives the real
    /// Create Review Period -> Approve -> Submit flow through HTTP so the
    /// resulting SUBMITTED period + Approved employee reflect the full
    /// DI-wired pipeline, not a direct DB insert.</summary>
    private async Task<(int periodId, int employeeId)> SubmittedPeriodWithApprovedEmployeeAsync()
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

        var createRequest = new CreateReviewPeriodRequest { Code = $"RP-{Guid.NewGuid():N}", Name = $"Annual-{Guid.NewGuid():N}", ReviewType = ReviewType.ANNUAL, ReviewDate = ReviewDate };
        var createResponse = await _client.PostAsJsonAsync("/review-periods", createRequest, TestJson.Options);
        var period = await createResponse.Content.ReadFromJsonAsync<ReviewPeriodResponse>(TestJson.Options);

        // GetActiveEmployeesAsync (US-SGP-01) sees every active employee in
        // this test class's shared database (CustomWebApplicationFactory is
        // one DB per class, not per test method) — including ones seeded by
        // earlier tests in this class — so the new period's snapshot can
        // include more than just the employee this call cares about.
        // Submitting requires every eligible employee to have an outcome,
        // so all of them need approving here, not just employeeId.
        var listResponse = await _client.GetAsync($"/review-periods/{period!.Id}/employees?pageSize=100");
        var page = await listResponse.Content.ReadFromJsonAsync<ReviewPeriodEmployeePageResponse>(TestJson.Options);
        foreach (var entry in page!.Items.Where(e => e.Eligible))
        {
            (await _client.PostAsync($"/review-periods/{period.Id}/employees/{entry.EmployeeId}/approve", null)).EnsureSuccessStatusCode();
        }

        (await _client.PostAsync($"/review-periods/{period.Id}/submit", null)).EnsureSuccessStatusCode();

        return (period.Id, employeeId);
    }

    [Fact]
    public async Task CreateDecision_PeriodNotSubmitted_Returns409()
    {
        var createRequest = new CreateReviewPeriodRequest { Code = $"RP-{Guid.NewGuid():N}", Name = $"Annual-{Guid.NewGuid():N}", ReviewType = ReviewType.ANNUAL, ReviewDate = ReviewDate };
        var createResponse = await _client.PostAsJsonAsync("/review-periods", createRequest, TestJson.Options);
        var period = await createResponse.Content.ReadFromJsonAsync<ReviewPeriodResponse>(TestJson.Options);

        // EmployeeIds needs at least one entry (MinLength(1) on the request
        // DTO) so model binding doesn't reject this with 400 before the
        // service's own period-status guard (the one under test) ever runs.
        var response = await _client.PostAsJsonAsync(
            "/salary-decisions",
            new CreateSalaryDecisionRequest { ReviewPeriodId = period!.Id, EmployeeIds = new List<int> { 1 }, EffectiveDate = ReviewDate },
            TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateDecision_ValidRequest_Returns201Draft()
    {
        var (periodId, employeeId) = await SubmittedPeriodWithApprovedEmployeeAsync();

        var response = await _client.PostAsJsonAsync(
            "/salary-decisions",
            new CreateSalaryDecisionRequest { ReviewPeriodId = periodId, EmployeeIds = new List<int> { employeeId }, EffectiveDate = ReviewDate },
            TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<SalaryDecisionResponse>(TestJson.Options);
        body!.Status.Should().Be(SalaryDecisionStatus.DRAFT);
    }

    [Fact]
    public async Task GetDecision_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/salary-decisions/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApplyDecision_Valid_Returns200AndClosesReviewPeriod()
    {
        var (periodId, employeeId) = await SubmittedPeriodWithApprovedEmployeeAsync();
        var createResponse = await _client.PostAsJsonAsync(
            "/salary-decisions",
            new CreateSalaryDecisionRequest { ReviewPeriodId = periodId, EmployeeIds = new List<int> { employeeId }, EffectiveDate = ReviewDate },
            TestJson.Options);
        var decision = await createResponse.Content.ReadFromJsonAsync<SalaryDecisionResponse>(TestJson.Options);

        var response = await _client.PostAsync($"/salary-decisions/{decision!.Id}/apply", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SalaryDecisionDetailResponse>(TestJson.Options);
        body!.Status.Should().Be(SalaryDecisionStatus.APPLIED);

        var periodResponse = await _client.GetAsync($"/review-periods/{periodId}");
        var periodDetail = await periodResponse.Content.ReadFromJsonAsync<ReviewPeriodDetailResponse>(TestJson.Options);
        periodDetail!.Status.Should().Be(ReviewPeriodStatus.CLOSED);
    }

    [Fact]
    public async Task CancelDecision_NotDraft_Returns409()
    {
        var (periodId, employeeId) = await SubmittedPeriodWithApprovedEmployeeAsync();
        var createResponse = await _client.PostAsJsonAsync(
            "/salary-decisions",
            new CreateSalaryDecisionRequest { ReviewPeriodId = periodId, EmployeeIds = new List<int> { employeeId }, EffectiveDate = ReviewDate },
            TestJson.Options);
        var decision = await createResponse.Content.ReadFromJsonAsync<SalaryDecisionResponse>(TestJson.Options);
        (await _client.PostAsync($"/salary-decisions/{decision!.Id}/apply", null)).EnsureSuccessStatusCode();

        var response = await _client.PostAsync($"/salary-decisions/{decision.Id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetEligibleReviewPeriods_ReturnsOk()
    {
        var response = await _client.GetAsync("/salary-decisions/eligible-review-periods");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
