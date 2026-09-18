using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HRM.Api.DTOs.Responses.SalaryGradePromotion;
using HRM.Domain.Entities;
using Xunit;

namespace HRM.Api.Tests.SalaryGradePromotion;

/// <summary>
/// Exercises the real HTTP pipeline against openapi.yaml's declared
/// contract for the Salary History group. Business-rule edge cases already
/// have thorough coverage in HRM.Application.Tests.
/// </summary>
public class SalaryHistoryControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SalaryHistoryControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSalaryHistory_NoHistory_ReturnsOkWithEmptyItems()
    {
        var response = await _client.GetAsync("/employees/999999/salary-history");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SalaryHistoryPageResponse>(TestJson.Options);
        body!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSalaryHistory_ReturnsNewestFirstAndMarksCurrent()
    {
        int employeeId;
        using (var db = _factory.CreateDbContext())
        {
            var unit = new HrOrganizationalUnit { Name = $"Unit-{Guid.NewGuid():N}", UnitType = "Department", Status = HRM.Domain.Enums.ActiveStatus.ACTIVE };
            var jobTitle = new HrJobTitle { Name = $"Title-{Guid.NewGuid():N}", Status = HRM.Domain.Enums.ActiveStatus.ACTIVE };
            db.OrganizationalUnits.Add(unit);
            db.JobTitles.Add(jobTitle);
            db.SaveChanges();

            var scale = new HrSalaryScale { Code = $"NL-{Guid.NewGuid():N}", Name = $"Scale-{Guid.NewGuid():N}", Status = HRM.Domain.Enums.ActiveStatus.ACTIVE };
            db.SalaryScales.Add(scale);
            db.SaveChanges();

            var grade = new HrSalaryGrade { SalaryScaleId = scale.Id, GradeNumber = 1, Status = HRM.Domain.Enums.ActiveStatus.ACTIVE };
            db.SalaryGrades.Add(grade);
            db.SaveChanges();

            var employee = new HrEmployee
            {
                EmployeeCode = $"EMP-{Guid.NewGuid():N}",
                FullName = "History Employee",
                OrganizationalUnitId = unit.Id,
                JobTitleId = jobTitle.Id,
                JoinDate = new DateOnly(2018, 1, 1),
                EmploymentStatus = HRM.Domain.Enums.EmploymentStatus.ACTIVE
            };
            db.Employees.Add(employee);
            db.SaveChanges();
            employeeId = employee.Id;

            db.EmployeeSalaries.Add(new HrEmployeeSalary { EmployeeId = employeeId, SalaryGradeId = grade.Id, Coefficient = 2.0m, EffectiveDate = new DateOnly(2020, 1, 1), Reason = "Initial assignment" });
            db.EmployeeSalaries.Add(new HrEmployeeSalary { EmployeeId = employeeId, SalaryGradeId = grade.Id, Coefficient = 2.3m, EffectiveDate = new DateOnly(2022, 1, 1), Reason = "Promotion" });
            db.SaveChanges();
        }

        var response = await _client.GetAsync($"/employees/{employeeId}/salary-history");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SalaryHistoryPageResponse>(TestJson.Options);
        body!.Items.Should().HaveCount(2);
        body.Items[0].EffectiveDate.Should().Be(new DateOnly(2022, 1, 1));
        body.Items[0].IsCurrent.Should().BeTrue();
    }
}
