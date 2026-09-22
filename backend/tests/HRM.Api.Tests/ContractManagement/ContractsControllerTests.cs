using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HRM.Api.DTOs.Requests.ContractManagement;
using HRM.Api.DTOs.Responses.ContractManagement;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Xunit;

namespace HRM.Api.Tests.ContractManagement;

/// <summary>
/// Exercises the real HTTP pipeline against openapi.yaml's declared
/// contract for the Contracts group. Business-rule edge cases already
/// have thorough coverage in HRM.Application.Tests.
/// </summary>
public class ContractsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public ContractsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>A fresh employee per call — HrLaborContract enforces at most
    /// one Active contract per employee, and CustomWebApplicationFactory is
    /// one database per test class, so reusing an employee across tests
    /// would leak state between them.</summary>
    private int SeedEmployee(EmploymentStatus status = EmploymentStatus.ACTIVE)
    {
        using var db = _factory.CreateDbContext();
        var unit = new HrOrganizationalUnit { Name = $"Unit-{Guid.NewGuid():N}", UnitType = "Department", Status = ActiveStatus.ACTIVE };
        var jobTitle = new HrJobTitle { Name = $"Title-{Guid.NewGuid():N}", Status = ActiveStatus.ACTIVE };
        db.OrganizationalUnits.Add(unit);
        db.JobTitles.Add(jobTitle);
        db.SaveChanges();

        var employee = new HrEmployee
        {
            EmployeeCode = $"EMP-{Guid.NewGuid():N}",
            FullName = "Contract Test Employee",
            OrganizationalUnitId = unit.Id,
            JobTitleId = jobTitle.Id,
            JoinDate = new DateOnly(2020, 1, 1),
            EmploymentStatus = status
        };
        db.Employees.Add(employee);
        db.SaveChanges();
        return employee.Id;
    }

    private HrLaborContract SeedContract(int employeeId, ContractStatus status, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        using var db = _factory.CreateDbContext();
        var contract = new HrLaborContract
        {
            EmployeeId = employeeId,
            ContractType = ContractType.FIXED_TERM,
            ContractNumber = $"CT-{Guid.NewGuid():N}",
            StartDate = startDate ?? Today,
            EndDate = endDate ?? Today.AddYears(1),
            ContractSalaryAmount = 20_000_000m,
            Status = status
        };
        db.Contracts.Add(contract);
        db.SaveChanges();
        return contract;
    }

    private CreateContractRequest ValidCreateRequest(int employeeId) => new()
    {
        EmployeeId = employeeId,
        ContractType = ContractType.FIXED_TERM,
        ContractNumber = $"CT-{Guid.NewGuid():N}",
        StartDate = Today,
        EndDate = Today.AddYears(1),
        ContractSalaryAmount = 20_000_000m,
        Status = ContractStatus.DRAFT
    };

    [Fact]
    public async Task CreateContract_ValidRequest_Returns201()
    {
        var employeeId = SeedEmployee();
        var request = ValidCreateRequest(employeeId);

        var response = await _client.PostAsJsonAsync("/contracts", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ContractDetailResponse>(TestJson.Options);
        body!.ContractNumber.Should().Be(request.ContractNumber);
        body.Status.Should().Be(ContractStatus.DRAFT);
        body.EmployeeCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateContract_MissingRequiredField_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/contracts", new { }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateContract_DuplicateNumber_Returns409()
    {
        var employeeId = SeedEmployee();
        var request = ValidCreateRequest(employeeId);
        (await _client.PostAsJsonAsync("/contracts", request, TestJson.Options)).EnsureSuccessStatusCode();

        var duplicate = ValidCreateRequest(SeedEmployee());
        duplicate.ContractNumber = request.ContractNumber;
        var response = await _client.PostAsJsonAsync("/contracts", duplicate, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateContract_EndDateMismatchForType_Returns400()
    {
        var request = ValidCreateRequest(SeedEmployee());
        request.ContractType = ContractType.INDEFINITE_TERM; // EndDate is still set — mismatch (BR-CON-04)

        var response = await _client.PostAsJsonAsync("/contracts", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateContract_EmployeeTerminated_Returns409()
    {
        var employeeId = SeedEmployee(EmploymentStatus.TERMINATED);
        var request = ValidCreateRequest(employeeId);

        var response = await _client.PostAsJsonAsync("/contracts", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task SearchContracts_ReturnsOk()
    {
        var response = await _client.GetAsync("/contracts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchContracts_ExpiringSoon_ReturnsOk()
    {
        var response = await _client.GetAsync("/contracts?expiringSoon=true&window=60");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ContractPageResponse>(TestJson.Options);
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task GetContract_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/contracts/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetContract_ValidRequest_Returns200WithEmployeeFields()
    {
        var employeeId = SeedEmployee();
        var contract = SeedContract(employeeId, ContractStatus.DRAFT);

        var response = await _client.GetAsync($"/contracts/{contract.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ContractDetailResponse>(TestJson.Options);
        body!.EmployeeCode.Should().NotBeNullOrEmpty();
        body.OrganizationalUnitName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateContract_NotFound_Returns404()
    {
        var request = new UpdateContractRequest
        {
            ContractType = ContractType.FIXED_TERM,
            ContractNumber = $"CT-{Guid.NewGuid():N}",
            StartDate = Today,
            EndDate = Today.AddYears(1),
            ContractSalaryAmount = 20_000_000m
        };

        var response = await _client.PutAsJsonAsync("/contracts/999999", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateContract_NotDraft_Returns409()
    {
        var employeeId = SeedEmployee();
        var contract = SeedContract(employeeId, ContractStatus.ACTIVE);
        var request = new UpdateContractRequest
        {
            ContractType = ContractType.FIXED_TERM,
            ContractNumber = contract.ContractNumber,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            ContractSalaryAmount = 25_000_000m
        };

        var response = await _client.PutAsJsonAsync($"/contracts/{contract.Id}", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateContract_ValidRequest_Returns200()
    {
        var employeeId = SeedEmployee();
        var contract = SeedContract(employeeId, ContractStatus.DRAFT);
        var request = new UpdateContractRequest
        {
            ContractType = ContractType.FIXED_TERM,
            ContractNumber = contract.ContractNumber,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            ContractSalaryAmount = 30_000_000m,
            SalaryNote = "Adjusted"
        };

        var response = await _client.PutAsJsonAsync($"/contracts/{contract.Id}", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ContractDetailResponse>(TestJson.Options);
        body!.ContractSalaryAmount.Should().Be(30_000_000m);
    }

    [Fact]
    public async Task DeleteContract_NotDraft_Returns409()
    {
        var employeeId = SeedEmployee();
        var contract = SeedContract(employeeId, ContractStatus.ACTIVE);

        var response = await _client.DeleteAsync($"/contracts/{contract.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeleteContract_Draft_Returns204()
    {
        var employeeId = SeedEmployee();
        var contract = SeedContract(employeeId, ContractStatus.DRAFT);

        var response = await _client.DeleteAsync($"/contracts/{contract.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _client.GetAsync($"/contracts/{contract.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivateContract_ValidDraft_Returns200()
    {
        var employeeId = SeedEmployee();
        var contract = SeedContract(employeeId, ContractStatus.DRAFT);

        var response = await _client.PostAsync($"/contracts/{contract.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ContractDetailResponse>(TestJson.Options);
        body!.Status.Should().Be(ContractStatus.ACTIVE);
    }

    [Fact]
    public async Task ActivateContract_NotDraft_Returns409()
    {
        var employeeId = SeedEmployee();
        var contract = SeedContract(employeeId, ContractStatus.ACTIVE);

        var response = await _client.PostAsync($"/contracts/{contract.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ExpireContract_ValidActive_Returns200()
    {
        var employeeId = SeedEmployee();
        var contract = SeedContract(employeeId, ContractStatus.ACTIVE, startDate: Today.AddYears(-1), endDate: Today.AddDays(-1));

        var response = await _client.PostAsync($"/contracts/{contract.Id}/expire", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ContractDetailResponse>(TestJson.Options);
        body!.Status.Should().Be(ContractStatus.EXPIRED);
    }

    [Fact]
    public async Task ExpireContract_NotYetEnded_Returns409()
    {
        var employeeId = SeedEmployee();
        var contract = SeedContract(employeeId, ContractStatus.ACTIVE);

        var response = await _client.PostAsync($"/contracts/{contract.Id}/expire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task TerminateContract_MissingReason_Returns400()
    {
        var employeeId = SeedEmployee();
        var contract = SeedContract(employeeId, ContractStatus.ACTIVE);

        var response = await _client.PostAsJsonAsync(
            $"/contracts/{contract.Id}/terminate",
            new { terminationDate = Today },
            TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TerminateContract_ValidRequest_Returns200()
    {
        var employeeId = SeedEmployee();
        var contract = SeedContract(employeeId, ContractStatus.ACTIVE);

        var response = await _client.PostAsJsonAsync(
            $"/contracts/{contract.Id}/terminate",
            new TerminateContractRequest { TerminationDate = Today, TerminationReason = "Mutual agreement" },
            TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ContractDetailResponse>(TestJson.Options);
        body!.Status.Should().Be(ContractStatus.TERMINATED);
        body.TerminationReason.Should().Be("Mutual agreement");
    }
}
