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
/// contract for the Review Periods group. Business-rule edge cases already
/// have thorough coverage in HRM.Application.Tests — these tests are about
/// the HTTP contract, not re-proving the business logic.
/// </summary>
public class ReviewPeriodsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ReviewPeriodsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>Seeds one employee, eligible as of reviewDate: an active
    /// grade 1 (>= 24 months old) with an active grade 2 above it in the
    /// same scale.</summary>
    private int SeedEligibleEmployee(DateOnly currentGradeEffectiveDate)
    {
        using var db = _factory.CreateDbContext();

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
            EffectiveDate = currentGradeEffectiveDate,
            Reason = "Initial assignment"
        });
        db.SaveChanges();

        return employee.Id;
    }

    [Fact]
    public async Task CreateReviewPeriod_ValidRequest_Returns201WithInProgressStatus()
    {
        SeedEligibleEmployee(new DateOnly(2020, 1, 1));
        var request = new CreateReviewPeriodRequest
        {
            Code = $"RP-{Guid.NewGuid():N}",
            Name = $"Annual Review {Guid.NewGuid():N}",
            ReviewType = ReviewType.ANNUAL,
            ReviewDate = new DateOnly(2026, 3, 15)
        };

        var response = await _client.PostAsJsonAsync("/review-periods", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ReviewPeriodResponse>(TestJson.Options);
        body!.Status.Should().Be(ReviewPeriodStatus.IN_PROGRESS);
    }

    [Fact]
    public async Task CreateReviewPeriod_DuplicateCode_Returns409()
    {
        var code = $"RP-{Guid.NewGuid():N}";
        var request = new CreateReviewPeriodRequest { Code = code, Name = $"First-{Guid.NewGuid():N}", ReviewType = ReviewType.ANNUAL, ReviewDate = new DateOnly(2026, 3, 15) };
        (await _client.PostAsJsonAsync("/review-periods", request, TestJson.Options)).EnsureSuccessStatusCode();

        var duplicate = new CreateReviewPeriodRequest { Code = code, Name = $"Second-{Guid.NewGuid():N}", ReviewType = ReviewType.ANNUAL, ReviewDate = new DateOnly(2026, 3, 15) };
        var response = await _client.PostAsJsonAsync("/review-periods", duplicate, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateReviewPeriod_MissingRequiredField_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/review-periods", new { name = "Missing code and reviewDate" }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetReviewPeriod_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/review-periods/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SubmitReviewPeriod_WithPendingEligibleEmployee_Returns409()
    {
        SeedEligibleEmployee(new DateOnly(2020, 1, 1));
        var request = new CreateReviewPeriodRequest { Code = $"RP-{Guid.NewGuid():N}", Name = $"Annual-{Guid.NewGuid():N}", ReviewType = ReviewType.ANNUAL, ReviewDate = new DateOnly(2026, 3, 15) };
        var createResponse = await _client.PostAsJsonAsync("/review-periods", request, TestJson.Options);
        var period = await createResponse.Content.ReadFromJsonAsync<ReviewPeriodResponse>(TestJson.Options);

        var response = await _client.PostAsync($"/review-periods/{period!.Id}/submit", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CancelReviewPeriod_NotFound_Returns404()
    {
        var response = await _client.PostAsync("/review-periods/999999/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchReviewPeriods_ReturnsPagedResult()
    {
        var response = await _client.GetAsync("/review-periods?page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<ReviewPeriodPageResponse>(TestJson.Options);
        page!.Page.Should().Be(1);
        page.PageSize.Should().Be(5);
    }
}
