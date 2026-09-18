using System.Net;
using System.Net.Http.Json;
using System.Threading;
using FluentAssertions;
using HRM.Api.DTOs.Requests.SalaryMasterData;
using HRM.Api.DTOs.Responses.SalaryMasterData;
using HRM.Domain.Entities;
using Xunit;

namespace HRM.Api.Tests.SalaryMasterData;

/// <summary>
/// Exercises the real HTTP pipeline against openapi.yaml's declared
/// contract for the Base Salary Rate group. Business-rule edge cases
/// already have thorough coverage in HRM.Application.Tests.
/// </summary>
public class BaseSalaryRatesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public BaseSalaryRatesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// HrBaseSalaryRate is a single, global, org-wide history (unlike most
    /// other seeded entities, it has no per-test unique code/name to key
    /// on), and CustomWebApplicationFactory shares one database across every
    /// test in this class. A random date is not enough to keep tests
    /// independent — the "later than latest" guard is order-dependent, and
    /// two random draws from the same range collide about as often as not.
    /// A monotonically increasing counter guarantees every call, in any
    /// test, in any order, produces a date strictly later than every date
    /// used before it in this run.
    /// </summary>
    private static int _dateOffset;
    private static DateOnly NextEffectiveDate() => new DateOnly(2100, 1, 1).AddDays(Interlocked.Increment(ref _dateOffset));

    [Fact]
    public async Task AddRate_ValidRequest_Returns201()
    {
        var request = new AddBaseSalaryRateRequest { Rate = 1_800_000, EffectiveDate = NextEffectiveDate() };

        var response = await _client.PostAsJsonAsync("/base-salary-rates", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<BaseSalaryRateResponse>(TestJson.Options);
        body!.Rate.Should().Be(request.Rate);
    }

    [Fact]
    public async Task AddRate_RateZeroOrNegative_Returns400()
    {
        var request = new AddBaseSalaryRateRequest { Rate = 0, EffectiveDate = NextEffectiveDate() };

        var response = await _client.PostAsJsonAsync("/base-salary-rates", request, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddRate_EffectiveDateNotAfterLatest_Returns409()
    {
        var sameDate = NextEffectiveDate();
        using (var db = _factory.CreateDbContext())
        {
            db.BaseSalaryRates.Add(new HrBaseSalaryRate { Rate = 1_000_000, EffectiveDate = sameDate });
            db.SaveChanges();
        }

        var response = await _client.PostAsJsonAsync("/base-salary-rates", new AddBaseSalaryRateRequest { Rate = 1_100_000, EffectiveDate = sameDate }, TestJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ListRates_ReturnsOk()
    {
        var response = await _client.GetAsync("/base-salary-rates");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
