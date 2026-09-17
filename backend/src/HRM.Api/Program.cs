using System.Text.Json.Serialization;
using HRM.Api.Middleware;
using HRM.Application.SalaryManagement.Interfaces;
using HRM.Application.SalaryManagement.Rules;
using HRM.Application.SalaryManagement.Services;
using HRM.Infrastructure.Persistence;
using HRM.Infrastructure.Repositories;
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

// Repositories (Infrastructure implements interfaces defined in Application — ADR-04)
builder.Services.AddScoped<ISalaryRepository, SalaryRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

// Rules
builder.Services.AddScoped<IEligibilityRule, EligibilityRule>();

// Services (business logic — ADR-04)
builder.Services.AddScoped<ISalaryReviewService, SalaryReviewService>();
builder.Services.AddScoped<ISalaryDecisionService, SalaryDecisionService>();
builder.Services.AddScoped<ISalaryHistoryService, SalaryHistoryService>();

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
