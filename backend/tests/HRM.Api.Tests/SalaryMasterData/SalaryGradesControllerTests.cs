using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HRM.Api.DTOs.Requests.SalaryMasterData;
using HRM.Api.DTOs.Responses.SalaryMasterData;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Xunit;

namespace HRM.Api.Tests.SalaryMasterData;

/// <summary>
/// Exercises the real HTTP pipeline against openapi.yaml's declared
/// contract for the Salary Grades group. Business-rule edge cases already
/// have thorough coverage in HRM.Application.Tests.
/// </summary>
public class SalaryGradesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SalaryGradesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private HrSalaryGrade SeedGrade(ActiveStatus gradeStatus = ActiveStatus.ACTIVE, ActiveStatus scaleStatus = ActiveStatus.ACTIVE)
    {
        using var db = _factory.CreateDbContext();
        var scale = new HrSalaryScale { Code = $"NL-{Guid.NewGuid():N}", Name = $"Scale-{Guid.NewGuid():N}", Status = scaleStatus };
        db.SalaryScales.Add(scale);
        db.SaveChanges();

        var grade = new HrSalaryGrade { SalaryScaleId = scale.Id, GradeNumber = 1, Status = gradeStatus };
        db.SalaryGrades.Add(grade);
        db.SaveChanges();

        db.SalaryGradeCoefficients.Add(new HrSalaryGradeCoefficient { SalaryGradeId = grade.Id, Coefficient = 2.0m, EffectiveDate = new DateOnly(2020, 1, 1) });
        db.SaveChanges();

        return grade;
    }

    [Fact]
    public async Task AddCoefficient_ValidRequest_Returns201()
    {
        var grade = SeedGrade();

        var response = await _client.PostAsJsonAsync(
            $"/salary-grades/{grade.Id}/coefficients",
            new AddSalaryGradeCoefficientRequest { Coefficient = 2.5m, EffectiveDate = new DateOnly(2027, 1, 1) },
            TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<SalaryGradeCoefficientResponse>(TestJson.Options);
        body!.Coefficient.Should().Be(2.5m);
    }

    [Fact]
    public async Task AddCoefficient_GradeInactive_Returns409()
    {
        var grade = SeedGrade(gradeStatus: ActiveStatus.INACTIVE);

        var response = await _client.PostAsJsonAsync(
            $"/salary-grades/{grade.Id}/coefficients",
            new AddSalaryGradeCoefficientRequest { Coefficient = 2.5m, EffectiveDate = new DateOnly(2027, 1, 1) },
            TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ListCoefficients_ReturnsOk_IncludingSeededCoefficient()
    {
        var grade = SeedGrade();

        var response = await _client.GetAsync($"/salary-grades/{grade.Id}/coefficients");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<SalaryGradeCoefficientResponse>>(TestJson.Options);
        body!.Should().ContainSingle(c => c.Coefficient == 2.0m);
    }

    [Fact]
    public async Task DeactivateGrade_NotFound_Returns404()
    {
        var response = await _client.PostAsync("/salary-grades/999999/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateGrade_NoActiveEmployeeAssigned_Returns200()
    {
        // Deferred from M5: the first test to resolve Lazy<ISalaryHistoryService>
        // through the real DI container, now that Salary Grade Promotion is
        // registered (M7) — proves SalaryGradeService -> ISalaryHistoryService
        // is wired correctly end-to-end.
        var grade = SeedGrade();

        var response = await _client.PostAsync($"/salary-grades/{grade.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SalaryGradeResponse>(TestJson.Options);
        body!.Status.Should().Be(ActiveStatus.INACTIVE);
    }

    [Fact]
    public async Task DeactivateGrade_ActiveEmployeeAssigned_Returns409()
    {
        var grade = SeedGrade();
        using (var db = _factory.CreateDbContext())
        {
            var unit = new HrOrganizationalUnit { Name = $"Unit-{Guid.NewGuid():N}", UnitType = "Department", Status = ActiveStatus.ACTIVE };
            var jobTitle = new HrJobTitle { Name = $"Title-{Guid.NewGuid():N}", Status = ActiveStatus.ACTIVE };
            db.OrganizationalUnits.Add(unit);
            db.JobTitles.Add(jobTitle);
            db.SaveChanges();

            var employee = new HrEmployee
            {
                EmployeeCode = $"EMP-{Guid.NewGuid():N}",
                FullName = "Assigned Employee",
                OrganizationalUnitId = unit.Id,
                JobTitleId = jobTitle.Id,
                JoinDate = new DateOnly(2020, 1, 1),
                EmploymentStatus = EmploymentStatus.ACTIVE
            };
            db.Employees.Add(employee);
            db.SaveChanges();

            db.EmployeeSalaries.Add(new HrEmployeeSalary
            {
                EmployeeId = employee.Id,
                SalaryGradeId = grade.Id,
                Coefficient = 2.0m,
                EffectiveDate = new DateOnly(2020, 1, 1),
                Reason = "Initial assignment"
            });
            db.SaveChanges();
        }

        var response = await _client.PostAsync($"/salary-grades/{grade.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ReactivateGrade_ScaleInactive_Returns409()
    {
        var grade = SeedGrade(gradeStatus: ActiveStatus.INACTIVE, scaleStatus: ActiveStatus.INACTIVE);

        var response = await _client.PostAsync($"/salary-grades/{grade.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ReactivateGrade_ScaleActive_Returns200()
    {
        var grade = SeedGrade(gradeStatus: ActiveStatus.INACTIVE, scaleStatus: ActiveStatus.ACTIVE);

        var response = await _client.PostAsync($"/salary-grades/{grade.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SalaryGradeResponse>(TestJson.Options);
        body!.Status.Should().Be(ActiveStatus.ACTIVE);
    }
}
