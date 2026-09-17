using HRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HRM.Api.Tests;

/// <summary>
/// Swaps the SQL Server DbContext for EF Core InMemory so integration tests
/// exercise the real HTTP pipeline (routing, model binding, status codes)
/// without needing a real database. Each instance gets its own isolated
/// database (named by <paramref name="databaseName"/>) so tests don't leak
/// state into each other.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<HrmDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            // A dedicated internal service provider avoids EF Core's InMemory
            // provider caching getting confused when the app's own DI
            // container already configured a different provider (SQL Server)
            // for this DbContext before this override runs.
            var inMemoryServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<HrmDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.UseInternalServiceProvider(inMemoryServiceProvider);
            });
        });
    }

    public HrmDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<HrmDbContext>();
    }
}
