using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HRM.Api.DTOs.Requests.EmployeeManagement;
using HRM.Api.DTOs.Responses.EmployeeManagement;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Xunit;

namespace HRM.Api.Tests.EmployeeManagement;

/// <summary>
/// Exercises the real HTTP pipeline against openapi.yaml's declared
/// contract for the Employees group. Business-rule edge cases already
/// have thorough coverage in HRM.Application.Tests.
/// </summary>
public class EmployeesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EmployeesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private (int unitId, int jobTitleId) SeedActiveUnitAndJobTitle()
    {
        using var db = _factory.CreateDbContext();
        var unit = new HrOrganizationalUnit { Name = $"Unit-{Guid.NewGuid():N}", UnitType = "Department", Status = ActiveStatus.ACTIVE };
        var jobTitle = new HrJobTitle { Name = $"Title-{Guid.NewGuid():N}", Status = ActiveStatus.ACTIVE };
        db.OrganizationalUnits.Add(unit);
        db.JobTitles.Add(jobTitle);
        db.SaveChanges();
        return (unit.Id, jobTitle.Id);
    }

    [Fact]
    public async Task CreateEmployee_ValidRequest_Returns201()
    {
        var (unitId, jobTitleId) = SeedActiveUnitAndJobTitle();
        var request = new CreateEmployeeRequest
        {
            EmployeeCode = $"EMP-{Guid.NewGuid():N}",
            FullName = "An Nguyen",
            OrganizationalUnitId = unitId,
            JobTitleId = jobTitleId,
            JoinDate = new DateOnly(2024, 1, 1),
            EmploymentStatus = EmploymentStatus.ACTIVE
        };

        var response = await _client.PostAsJsonAsync("/employees", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<EmployeeResponse>(TestJson.Options);
        body!.EmployeeCode.Should().Be(request.EmployeeCode);
    }

    [Fact]
    public async Task CreateEmployee_MissingRequiredField_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/employees", new { fullName = "No code, unit, or title" }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateEmployee_DuplicateCode_Returns409()
    {
        var (unitId, jobTitleId) = SeedActiveUnitAndJobTitle();
        var code = $"EMP-{Guid.NewGuid():N}";
        var request = new CreateEmployeeRequest
        {
            EmployeeCode = code,
            FullName = "First",
            OrganizationalUnitId = unitId,
            JobTitleId = jobTitleId,
            JoinDate = new DateOnly(2024, 1, 1),
            EmploymentStatus = EmploymentStatus.ACTIVE
        };
        (await _client.PostAsJsonAsync("/employees", request, TestJson.Options)).EnsureSuccessStatusCode();

        var duplicate = new CreateEmployeeRequest
        {
            EmployeeCode = code,
            FullName = "Second",
            OrganizationalUnitId = unitId,
            JobTitleId = jobTitleId,
            JoinDate = new DateOnly(2024, 1, 1),
            EmploymentStatus = EmploymentStatus.ACTIVE
        };
        var response = await _client.PostAsJsonAsync("/employees", duplicate, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateEmployee_InactiveOrganizationalUnit_Returns400()
    {
        var (_, jobTitleId) = SeedActiveUnitAndJobTitle();
        using var db = _factory.CreateDbContext();
        var inactiveUnit = new HrOrganizationalUnit { Name = $"Unit-{Guid.NewGuid():N}", UnitType = "Department", Status = ActiveStatus.INACTIVE };
        db.OrganizationalUnits.Add(inactiveUnit);
        db.SaveChanges();

        var request = new CreateEmployeeRequest
        {
            EmployeeCode = $"EMP-{Guid.NewGuid():N}",
            FullName = "An Nguyen",
            OrganizationalUnitId = inactiveUnit.Id,
            JobTitleId = jobTitleId,
            JoinDate = new DateOnly(2024, 1, 1),
            EmploymentStatus = EmploymentStatus.ACTIVE
        };

        var response = await _client.PostAsJsonAsync("/employees", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchEmployees_ReturnsOk()
    {
        var response = await _client.GetAsync("/employees");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetEmployee_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/employees/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateEmployee_NotFound_Returns404()
    {
        var response = await _client.PutAsJsonAsync("/employees/999999", new UpdateEmployeeRequest { FullName = "New Name" }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateEmployee_ValidRequest_Returns200()
    {
        var (unitId, jobTitleId) = SeedActiveUnitAndJobTitle();
        HrEmployee employee;
        using (var db = _factory.CreateDbContext())
        {
            employee = new HrEmployee
            {
                EmployeeCode = $"EMP-{Guid.NewGuid():N}",
                FullName = "Old Name",
                OrganizationalUnitId = unitId,
                JobTitleId = jobTitleId,
                JoinDate = new DateOnly(2024, 1, 1),
                EmploymentStatus = EmploymentStatus.ACTIVE
            };
            db.Employees.Add(employee);
            db.SaveChanges();
        }

        var response = await _client.PutAsJsonAsync($"/employees/{employee.Id}", new UpdateEmployeeRequest { FullName = "New Name" }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<EmployeeResponse>(TestJson.Options);
        body!.FullName.Should().Be("New Name");
    }

    [Fact]
    public async Task ChangeEmploymentStatus_CurrentlyTerminated_Returns409()
    {
        var (unitId, jobTitleId) = SeedActiveUnitAndJobTitle();
        HrEmployee employee;
        using (var db = _factory.CreateDbContext())
        {
            employee = new HrEmployee
            {
                EmployeeCode = $"EMP-{Guid.NewGuid():N}",
                FullName = "Terminated Employee",
                OrganizationalUnitId = unitId,
                JobTitleId = jobTitleId,
                JoinDate = new DateOnly(2020, 1, 1),
                EmploymentStatus = EmploymentStatus.TERMINATED
            };
            db.Employees.Add(employee);
            db.SaveChanges();
        }

        var response = await _client.PostAsJsonAsync(
            $"/employees/{employee.Id}/employment-status",
            new ChangeEmploymentStatusRequest { EmploymentStatus = EmploymentStatus.ACTIVE },
            TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ChangeEmploymentStatus_ValidTransition_Returns200()
    {
        var (unitId, jobTitleId) = SeedActiveUnitAndJobTitle();
        HrEmployee employee;
        using (var db = _factory.CreateDbContext())
        {
            employee = new HrEmployee
            {
                EmployeeCode = $"EMP-{Guid.NewGuid():N}",
                FullName = "Active Employee",
                OrganizationalUnitId = unitId,
                JobTitleId = jobTitleId,
                JoinDate = new DateOnly(2024, 1, 1),
                EmploymentStatus = EmploymentStatus.ACTIVE
            };
            db.Employees.Add(employee);
            db.SaveChanges();
        }

        var response = await _client.PostAsJsonAsync(
            $"/employees/{employee.Id}/employment-status",
            new ChangeEmploymentStatusRequest { EmploymentStatus = EmploymentStatus.ON_LEAVE },
            TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<EmployeeResponse>(TestJson.Options);
        body!.EmploymentStatus.Should().Be(EmploymentStatus.ON_LEAVE);
    }
}
