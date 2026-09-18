using HRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRM.Infrastructure.Persistence;

public class HrmDbContext : DbContext
{
    public HrmDbContext(DbContextOptions<HrmDbContext> options) : base(options) { }

    public DbSet<HrOrganizationalUnit> OrganizationalUnits => Set<HrOrganizationalUnit>();
    public DbSet<HrJobTitle> JobTitles => Set<HrJobTitle>();
    public DbSet<HrEmployee> Employees => Set<HrEmployee>();
    public DbSet<HrBaseSalaryRate> BaseSalaryRates => Set<HrBaseSalaryRate>();
    public DbSet<HrSalaryScale> SalaryScales => Set<HrSalaryScale>();
    public DbSet<HrSalaryGrade> SalaryGrades => Set<HrSalaryGrade>();
    public DbSet<HrSalaryGradeCoefficient> SalaryGradeCoefficients => Set<HrSalaryGradeCoefficient>();
    public DbSet<HrEmployeeSalary> EmployeeSalaries => Set<HrEmployeeSalary>();
    public DbSet<HrSalaryReviewPeriod> ReviewPeriods => Set<HrSalaryReviewPeriod>();
    public DbSet<HrSalaryReviewEmployee> ReviewEmployees => Set<HrSalaryReviewEmployee>();
    public DbSet<HrSalaryDecision> Decisions => Set<HrSalaryDecision>();
    public DbSet<HrSalaryDecisionDetail> DecisionDetails => Set<HrSalaryDecisionDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrmDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
