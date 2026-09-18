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
/// contract for the Salary Scales group. Business-rule edge cases already
/// have thorough coverage in HRM.Application.Tests.
/// </summary>
public class SalaryScalesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SalaryScalesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private HrSalaryScale SeedScale(string code, string name, ActiveStatus status = ActiveStatus.ACTIVE)
    {
        using var db = _factory.CreateDbContext();
        var scale = new HrSalaryScale { Code = code, Name = name, Status = status };
        db.SalaryScales.Add(scale);
        db.SaveChanges();
        return scale;
    }

    [Fact]
    public async Task CreateScale_ValidRequest_Returns201()
    {
        var request = new CreateSalaryScaleRequest { Code = $"NL-{Guid.NewGuid():N}", Name = $"Scale-{Guid.NewGuid():N}" };

        var response = await _client.PostAsJsonAsync("/salary-scales", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<SalaryScaleResponse>(TestJson.Options);
        body!.Status.Should().Be(ActiveStatus.ACTIVE);
    }

    [Fact]
    public async Task CreateScale_MissingRequiredField_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/salary-scales", new { }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateScale_DuplicateCode_Returns409()
    {
        var scale = SeedScale($"NL-{Guid.NewGuid():N}", $"Scale-{Guid.NewGuid():N}");

        var response = await _client.PostAsJsonAsync("/salary-scales", new CreateSalaryScaleRequest { Code = scale.Code, Name = $"Other-{Guid.NewGuid():N}" }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ListScales_ReturnsOk()
    {
        var response = await _client.GetAsync("/salary-scales");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetScaleDetail_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/salary-scales/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetScaleDetail_ReturnsGradesWithCurrentCoefficient()
    {
        var scale = SeedScale($"NL-{Guid.NewGuid():N}", $"Scale-{Guid.NewGuid():N}");
        using (var db = _factory.CreateDbContext())
        {
            var grade = new HrSalaryGrade { SalaryScaleId = scale.Id, GradeNumber = 1, Status = ActiveStatus.ACTIVE };
            db.SalaryGrades.Add(grade);
            db.SaveChanges();
            db.SalaryGradeCoefficients.Add(new HrSalaryGradeCoefficient { SalaryGradeId = grade.Id, Coefficient = 2.34m, EffectiveDate = new DateOnly(2020, 1, 1) });
            db.SaveChanges();
        }

        var response = await _client.GetAsync($"/salary-scales/{scale.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SalaryScaleDetailResponse>(TestJson.Options);
        body!.Grades.Should().ContainSingle(g => g.CurrentCoefficient == 2.34m);
    }

    [Fact]
    public async Task UpdateScale_NotFound_Returns404()
    {
        var response = await _client.PutAsJsonAsync("/salary-scales/999999", new UpdateSalaryScaleRequest { Name = "New" }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateScale_ValidRequest_Returns200()
    {
        var scale = SeedScale($"NL-{Guid.NewGuid():N}", $"Scale-{Guid.NewGuid():N}");
        var newName = $"Renamed-{Guid.NewGuid():N}";

        var response = await _client.PutAsJsonAsync($"/salary-scales/{scale.Id}", new UpdateSalaryScaleRequest { Name = newName }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SalaryScaleResponse>(TestJson.Options);
        body!.Name.Should().Be(newName);
    }

    [Fact]
    public async Task DeactivateScale_HasActiveGrades_Returns409()
    {
        var scale = SeedScale($"NL-{Guid.NewGuid():N}", $"Scale-{Guid.NewGuid():N}");
        using (var db = _factory.CreateDbContext())
        {
            db.SalaryGrades.Add(new HrSalaryGrade { SalaryScaleId = scale.Id, GradeNumber = 1, Status = ActiveStatus.ACTIVE });
            db.SaveChanges();
        }

        var response = await _client.PostAsync($"/salary-scales/{scale.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeactivateScale_NoActiveGrades_Returns200()
    {
        var scale = SeedScale($"NL-{Guid.NewGuid():N}", $"Scale-{Guid.NewGuid():N}");

        var response = await _client.PostAsync($"/salary-scales/{scale.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SalaryScaleResponse>(TestJson.Options);
        body!.Status.Should().Be(ActiveStatus.INACTIVE);
    }

    [Fact]
    public async Task ReactivateScale_NoGuard_Returns200()
    {
        var scale = SeedScale($"NL-{Guid.NewGuid():N}", $"Scale-{Guid.NewGuid():N}", ActiveStatus.INACTIVE);

        var response = await _client.PostAsync($"/salary-scales/{scale.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SalaryScaleResponse>(TestJson.Options);
        body!.Status.Should().Be(ActiveStatus.ACTIVE);
    }

    [Fact]
    public async Task CreateGrade_ScaleInactive_Returns409()
    {
        var scale = SeedScale($"NL-{Guid.NewGuid():N}", $"Scale-{Guid.NewGuid():N}", ActiveStatus.INACTIVE);

        var response = await _client.PostAsJsonAsync($"/salary-scales/{scale.Id}/grades", new CreateSalaryGradeRequest { GradeNumber = 1, Coefficient = 2.0m }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateGrade_ValidRequest_Returns201()
    {
        var scale = SeedScale($"NL-{Guid.NewGuid():N}", $"Scale-{Guid.NewGuid():N}");

        var response = await _client.PostAsJsonAsync($"/salary-scales/{scale.Id}/grades", new CreateSalaryGradeRequest { GradeNumber = 1, Coefficient = 2.34m }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<SalaryGradeResponse>(TestJson.Options);
        body!.CurrentCoefficient.Should().Be(2.34m);
    }
}
