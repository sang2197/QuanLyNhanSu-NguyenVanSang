using System.Text.Json.Serialization;
using HRM.Api.Middleware;
using HRM.Application.Common;
using HRM.Application.ContractManagement.Interfaces;
using HRM.Application.ContractManagement.Services;
using HRM.Application.EmployeeManagement.Interfaces;
using HRM.Application.EmployeeManagement.Services;
using HRM.Application.OrganizationManagement.Interfaces;
using HRM.Application.OrganizationManagement.Services;
using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Application.SalaryGradePromotion.Rules;
using HRM.Application.SalaryGradePromotion.Services;
using HRM.Application.SalaryMasterData.Interfaces;
using HRM.Application.SalaryMasterData.Services;
using HRM.Infrastructure.Persistence;
using HRM.Infrastructure.Repositories.ContractManagement;
using HRM.Infrastructure.Repositories.EmployeeManagement;
using HRM.Infrastructure.Repositories.OrganizationManagement;
using HRM.Infrastructure.Repositories.SalaryGradePromotion;
using HRM.Infrastructure.Repositories.SalaryMasterData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core — SQL Server (ADR-06). Connection string comes from appsettings /
// environment; see appsettings.json "ConnectionStrings:HrmDatabase".
builder.Services.AddDbContext<HrmDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HrmDatabase")));

// Unit of work (SaveChanges + transactions — see IUnitOfWork's doc comment)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories (Infrastructure implements interfaces defined in Application — ADR-04)
builder.Services.AddScoped<IOrganizationalUnitRepository, OrganizationalUnitRepository>();
builder.Services.AddScoped<IJobTitleRepository, JobTitleRepository>();
builder.Services.AddScoped<IBaseSalaryRateRepository, BaseSalaryRateRepository>();
builder.Services.AddScoped<ISalaryScaleRepository, SalaryScaleRepository>();
builder.Services.AddScoped<ISalaryGradeRepository, SalaryGradeRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IReviewPeriodRepository, ReviewPeriodRepository>();
builder.Services.AddScoped<IReviewEmployeeRepository, ReviewEmployeeRepository>();
builder.Services.AddScoped<ISalaryDecisionRepository, SalaryDecisionRepository>();
builder.Services.AddScoped<IEmployeeSalaryRepository, EmployeeSalaryRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();

// Rules
builder.Services.AddScoped<ISalaryPromotionEligibilityRule, SalaryPromotionEligibilityRule>();

// Services (business logic — ADR-04)
builder.Services.AddScoped<IOrganizationalUnitService, OrganizationalUnitService>();
builder.Services.AddScoped<IJobTitleService, JobTitleService>();
builder.Services.AddScoped<IBaseSalaryRateService, BaseSalaryRateService>();
builder.Services.AddScoped<ISalaryScaleService, SalaryScaleService>();
builder.Services.AddScoped<ISalaryGradeService, SalaryGradeService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IReviewPeriodService, ReviewPeriodService>();
builder.Services.AddScoped<IReviewEmployeeService, ReviewEmployeeService>();
builder.Services.AddScoped<ISalaryDecisionService, SalaryDecisionService>();
builder.Services.AddScoped<ISalaryHistoryService, SalaryHistoryService>();
builder.Services.AddScoped<IContractService, ContractService>();

// OrganizationalUnitService and EmployeeService depend on each other
// (ADR-03 cross-domain calls: BR-ORG-11, BR-EMP-04/05) — a genuine
// constructor-injection cycle. SalaryGradeService -> ISalaryHistoryService
// (BR-SAL-15) is one-way, not a cycle, but used the same Lazy<> mechanism
// while Salary Grade Promotion didn't exist yet — kept for consistency now
// that it does, since SalaryGradeService is already built against it.
builder.Services.AddScoped(sp => new Lazy<IEmployeeService>(() => sp.GetRequiredService<IEmployeeService>()));
builder.Services.AddScoped(sp => new Lazy<ISalaryHistoryService>(() => sp.GetRequiredService<ISalaryHistoryService>()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// NOTE: no auth middleware yet — RISK-05 (role model not finalized) blocks
// implementing [Authorize(Roles=...)] meaningfully. See ADR-07.
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program { }
