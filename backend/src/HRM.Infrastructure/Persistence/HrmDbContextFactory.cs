using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HRM.Infrastructure.Persistence;

/// <summary>
/// Design-time factory so `dotnet ef migrations add` can run directly
/// against this project without a working --startup-project (useful while
/// HRM.Api is mid-rewrite and doesn't build). Not used at runtime — the
/// real app registers HrmDbContext via DI in HRM.Api/Program.cs against
/// the real ConnectionStrings:HrmDatabase setting.
/// </summary>
public class HrmDbContextFactory : IDesignTimeDbContextFactory<HrmDbContext>
{
    public HrmDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HrmDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=HrmDatabase;Trusted_Connection=True;");
        return new HrmDbContext(optionsBuilder.Options);
    }
}
