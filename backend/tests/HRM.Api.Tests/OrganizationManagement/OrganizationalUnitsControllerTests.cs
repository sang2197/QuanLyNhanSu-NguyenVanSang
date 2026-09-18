using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HRM.Api.DTOs.Requests.OrganizationManagement;
using HRM.Api.DTOs.Responses.OrganizationManagement;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Xunit;

namespace HRM.Api.Tests.OrganizationManagement;

/// <summary>
/// Exercises the real HTTP pipeline (routing, model binding, status codes)
/// against openapi.yaml's declared contract for the Organizational Units
/// group. Business-rule edge cases already have thorough coverage in
/// HRM.Application.Tests — these tests are about the HTTP contract, not
/// re-proving the business logic.
/// </summary>
public class OrganizationalUnitsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public OrganizationalUnitsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private HrOrganizationalUnit SeedUnit(string name, int? parentId = null, ActiveStatus status = ActiveStatus.ACTIVE)
    {
        using var db = _factory.CreateDbContext();
        var unit = new HrOrganizationalUnit { Name = name, ParentId = parentId, UnitType = "Department", Status = status };
        db.OrganizationalUnits.Add(unit);
        db.SaveChanges();
        return unit;
    }

    [Fact]
    public async Task CreateUnit_ValidRequest_Returns201()
    {
        var request = new CreateOrganizationalUnitRequest { Name = $"Sales-{Guid.NewGuid():N}", UnitType = "Department" };

        var response = await _client.PostAsJsonAsync("/organizational-units", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<OrganizationalUnitResponse>(TestJson.Options);
        body!.Name.Should().Be(request.Name);
        body.Status.Should().Be(ActiveStatus.ACTIVE);
    }

    [Fact]
    public async Task CreateUnit_MissingRequiredField_Returns400()
    {
        var payload = new { contactEmail = "no-name-or-type@corp.com" };

        var response = await _client.PostAsJsonAsync("/organizational-units", payload, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUnit_DuplicateNameAtTopLevel_Returns409()
    {
        var name = $"Sales-{Guid.NewGuid():N}";
        SeedUnit(name);
        var request = new CreateOrganizationalUnitRequest { Name = name, UnitType = "Department" };

        var response = await _client.PostAsJsonAsync("/organizational-units", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetStructure_ReturnsOk_IncludingSeededUnit()
    {
        var unit = SeedUnit($"Sales-{Guid.NewGuid():N}");

        var response = await _client.GetAsync("/organizational-units");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var units = await response.Content.ReadFromJsonAsync<List<OrganizationalUnitResponse>>(TestJson.Options);
        units!.Should().Contain(u => u.Id == unit.Id);
    }

    [Fact]
    public async Task UpdateUnit_NotFound_Returns404()
    {
        var request = new UpdateOrganizationalUnitRequest { Name = "New Name" };

        var response = await _client.PutAsJsonAsync("/organizational-units/999999", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUnit_ValidRequest_Returns200()
    {
        var unit = SeedUnit($"Sales-{Guid.NewGuid():N}");
        var request = new UpdateOrganizationalUnitRequest { UnitType = "Division" };

        var response = await _client.PutAsJsonAsync($"/organizational-units/{unit.Id}", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OrganizationalUnitResponse>(TestJson.Options);
        body!.UnitType.Should().Be("Division");
    }

    [Fact]
    public async Task MoveUnit_UnitNotFound_Returns404()
    {
        var target = SeedUnit($"Target-{Guid.NewGuid():N}");
        var request = new MoveOrganizationalUnitRequest { TargetParentId = target.Id };

        var response = await _client.PostAsJsonAsync("/organizational-units/999999/move", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MoveUnit_ValidRequest_Returns200()
    {
        var unit = SeedUnit($"Sales-{Guid.NewGuid():N}");
        var target = SeedUnit($"Target-{Guid.NewGuid():N}");
        var request = new MoveOrganizationalUnitRequest { TargetParentId = target.Id };

        var response = await _client.PostAsJsonAsync($"/organizational-units/{unit.Id}/move", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OrganizationalUnitResponse>(TestJson.Options);
        body!.ParentId.Should().Be(target.Id);
    }

    [Fact]
    public async Task DeactivateUnit_NotFound_Returns404()
    {
        var response = await _client.PostAsync("/organizational-units/999999/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateUnit_NoActiveChildrenOrEmployees_Returns200()
    {
        // Deferred from M4: this is the first test to actually resolve
        // Lazy<IEmployeeService>.Value through the real DI container (not a
        // mock), now that Employee Management is registered (M6) — proves
        // the constructor-injection cycle between OrganizationalUnitService
        // and EmployeeService is wired correctly end-to-end.
        var unit = SeedUnit($"Sales-{Guid.NewGuid():N}");

        var response = await _client.PostAsync($"/organizational-units/{unit.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OrganizationalUnitResponse>(TestJson.Options);
        body!.Status.Should().Be(ActiveStatus.INACTIVE);
    }

    [Fact]
    public async Task DeactivateUnit_HasActiveEmployeeAssigned_Returns409()
    {
        // The other half of the same end-to-end proof: BR-ORG-11 actually
        // blocks deactivation when Employee Management reports an active
        // employee in the unit, via the real cross-domain call chain
        // (OrganizationalUnitsController -> OrganizationalUnitService ->
        // Lazy<IEmployeeService> -> EmployeeService -> EmployeeRepository).
        var unit = SeedUnit($"Sales-{Guid.NewGuid():N}");
        using (var db = _factory.CreateDbContext())
        {
            var jobTitle = new HrJobTitle { Name = $"Title-{Guid.NewGuid():N}", Status = ActiveStatus.ACTIVE };
            db.JobTitles.Add(jobTitle);
            db.SaveChanges();

            db.Employees.Add(new HrEmployee
            {
                EmployeeCode = $"EMP-{Guid.NewGuid():N}",
                FullName = "Active Employee",
                OrganizationalUnitId = unit.Id,
                JobTitleId = jobTitle.Id,
                JoinDate = new DateOnly(2024, 1, 1),
                EmploymentStatus = EmploymentStatus.ACTIVE
            });
            db.SaveChanges();
        }

        var response = await _client.PostAsync($"/organizational-units/{unit.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ReactivateUnit_NoParent_Returns200()
    {
        var unit = SeedUnit($"Sales-{Guid.NewGuid():N}", status: ActiveStatus.INACTIVE);

        var response = await _client.PostAsync($"/organizational-units/{unit.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OrganizationalUnitResponse>(TestJson.Options);
        body!.Status.Should().Be(ActiveStatus.ACTIVE);
    }
}
