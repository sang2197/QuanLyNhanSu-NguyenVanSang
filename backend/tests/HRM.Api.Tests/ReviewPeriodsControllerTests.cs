using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HRM.Api.DTOs.Requests;
using HRM.Api.DTOs.Responses;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Xunit;

namespace HRM.Api.Tests;

/// <summary>
/// Exercises the real HTTP pipeline (routing, model binding, status codes)
/// against openapi.yaml's declared contract for the Review Periods group.
/// Business-rule edge cases already have thorough coverage in
/// HRM.Application.Tests — these tests are about the HTTP contract, not
/// re-proving the business logic.
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

    private (int employeeId, int scaleId, int gradeId) SeedEligibleEmployee(DateOnly effectiveFrom)
    {
        using var db = _factory.CreateDbContext();
        var scale = new HrSalaryScale { Code = $"SCALE-{Guid.NewGuid():N}", Name = "Scale A", EffectiveFrom = new DateOnly(2020, 1, 1) };
        db.SalaryScales.Add(scale);
        db.SaveChanges();

        var grade = new HrSalaryGrade { SalaryScaleId = scale.Id, GradeNumber = 1, Coefficient = 2.0m, EffectiveFrom = new DateOnly(2020, 1, 1) };
        var nextGrade = new HrSalaryGrade { SalaryScaleId = scale.Id, GradeNumber = 2, Coefficient = 2.3m, EffectiveFrom = new DateOnly(2020, 1, 1) };
        db.SalaryGrades.AddRange(grade, nextGrade);
        db.SaveChanges();

        var employee = new HrEmployee { EmployeeCode = $"EMP-{Guid.NewGuid():N}", FullName = "Test Employee" };
        db.Employees.Add(employee);
        db.SaveChanges();

        db.EmployeeSalaries.Add(new HrEmployeeSalary
        {
            EmployeeId = employee.Id,
            SalaryScaleId = scale.Id,
            SalaryGradeId = grade.Id,
            Coefficient = grade.Coefficient,
            EffectiveFrom = effectiveFrom
        });
        db.SaveChanges();

        return (employee.Id, scale.Id, grade.Id);
    }

    [Fact]
    public async Task CreateReviewPeriod_ValidRequest_Returns201_WithInProgressStatus()
    {
        SeedEligibleEmployee(new DateOnly(2020, 1, 1));
        var request = new CreateReviewPeriodRequest
        {
            Code = $"RP-{Guid.NewGuid():N}",
            Name = "Annual Review",
            ReviewType = ReviewType.PERIODIC,
            ReviewDate = new DateOnly(2026, 3, 15)
        };

        var response = await _client.PostAsJsonAsync("/review-periods", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ReviewPeriodResponse>(TestJson.Options);
        body!.Code.Should().Be(request.Code);
        body.Status.Should().Be(ReviewPeriodStatus.IN_PROGRESS);
    }

    [Fact]
    public async Task CreateReviewPeriod_DuplicateCode_Returns409()
    {
        var code = $"RP-{Guid.NewGuid():N}";
        var request = new CreateReviewPeriodRequest { Code = code, Name = "First", ReviewType = ReviewType.PERIODIC, ReviewDate = new DateOnly(2026, 3, 15) };
        (await _client.PostAsJsonAsync("/review-periods", request, TestJson.Options)).EnsureSuccessStatusCode();

        var duplicate = new CreateReviewPeriodRequest { Code = code, Name = "Second", ReviewType = ReviewType.PERIODIC, ReviewDate = new DateOnly(2026, 3, 15) };
        var response = await _client.PostAsJsonAsync("/review-periods", duplicate, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(TestJson.Options);
        error!.Code.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateReviewPeriod_MissingRequiredField_Returns400()
    {
        var payload = new { name = "Missing code and reviewDate" };

        var response = await _client.PostAsJsonAsync("/review-periods", payload, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetReviewPeriod_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/review-periods/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SubmitReviewPeriod_WithPendingEmployees_Returns409()
    {
        SeedEligibleEmployee(new DateOnly(2020, 1, 1));
        var request = new CreateReviewPeriodRequest { Code = $"RP-{Guid.NewGuid():N}", Name = "Annual", ReviewType = ReviewType.PERIODIC, ReviewDate = new DateOnly(2026, 3, 15) };
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
    public async Task GetReviewPeriods_ReturnsPagedResult()
    {
        var response = await _client.GetAsync("/review-periods?page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<ReviewPeriodPageResponse>(TestJson.Options);
        page!.Page.Should().Be(1);
        page.PageSize.Should().Be(5);
    }
}
