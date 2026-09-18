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
/// Exercises the real HTTP pipeline against openapi.yaml's declared
/// contract for the Job Titles group. Business-rule edge cases already
/// have thorough coverage in HRM.Application.Tests.
/// </summary>
public class JobTitlesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public JobTitlesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private HrJobTitle SeedJobTitle(string name, ActiveStatus status = ActiveStatus.ACTIVE)
    {
        using var db = _factory.CreateDbContext();
        var jobTitle = new HrJobTitle { Name = name, Status = status };
        db.JobTitles.Add(jobTitle);
        db.SaveChanges();
        return jobTitle;
    }

    [Fact]
    public async Task CreateJobTitle_ValidRequest_Returns201()
    {
        var request = new CreateJobTitleRequest { Name = $"Engineer-{Guid.NewGuid():N}" };

        var response = await _client.PostAsJsonAsync("/job-titles", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JobTitleResponse>(TestJson.Options);
        body!.Name.Should().Be(request.Name);
        body.Status.Should().Be(ActiveStatus.ACTIVE);
    }

    [Fact]
    public async Task CreateJobTitle_MissingRequiredField_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/job-titles", new { }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateJobTitle_DuplicateName_Returns409()
    {
        var name = $"Engineer-{Guid.NewGuid():N}";
        SeedJobTitle(name);

        var response = await _client.PostAsJsonAsync("/job-titles", new CreateJobTitleRequest { Name = name }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ListJobTitles_ReturnsOk_IncludingSeededJobTitle()
    {
        var jobTitle = SeedJobTitle($"Engineer-{Guid.NewGuid():N}");

        var response = await _client.GetAsync("/job-titles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jobTitles = await response.Content.ReadFromJsonAsync<List<JobTitleResponse>>(TestJson.Options);
        jobTitles!.Should().Contain(t => t.Id == jobTitle.Id);
    }

    [Fact]
    public async Task UpdateJobTitle_NotFound_Returns404()
    {
        var response = await _client.PutAsJsonAsync("/job-titles/999999", new UpdateJobTitleRequest { Name = "New Name" }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateJobTitle_ValidRequest_Returns200()
    {
        var jobTitle = SeedJobTitle($"Engineer-{Guid.NewGuid():N}");
        var newName = $"Senior-{Guid.NewGuid():N}";

        var response = await _client.PutAsJsonAsync($"/job-titles/{jobTitle.Id}", new UpdateJobTitleRequest { Name = newName }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JobTitleResponse>(TestJson.Options);
        body!.Name.Should().Be(newName);
    }

    [Fact]
    public async Task DeactivateJobTitle_NoGuard_Returns200()
    {
        var jobTitle = SeedJobTitle($"Engineer-{Guid.NewGuid():N}");

        var response = await _client.PostAsync($"/job-titles/{jobTitle.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JobTitleResponse>(TestJson.Options);
        body!.Status.Should().Be(ActiveStatus.INACTIVE);
    }

    [Fact]
    public async Task ReactivateJobTitle_NoGuard_Returns200()
    {
        var jobTitle = SeedJobTitle($"Engineer-{Guid.NewGuid():N}", ActiveStatus.INACTIVE);

        var response = await _client.PostAsync($"/job-titles/{jobTitle.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JobTitleResponse>(TestJson.Options);
        body!.Status.Should().Be(ActiveStatus.ACTIVE);
    }
}
