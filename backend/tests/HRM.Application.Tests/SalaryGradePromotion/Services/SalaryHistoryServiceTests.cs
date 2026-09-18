using FluentAssertions;
using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Application.SalaryGradePromotion.Services;
using HRM.Domain.Entities;
using Moq;
using Xunit;

namespace HRM.Application.Tests.SalaryGradePromotion.Services;

public class SalaryHistoryServiceTests
{
    private readonly Mock<IEmployeeSalaryRepository> _repo = new();
    private readonly SalaryHistoryService _sut;

    public SalaryHistoryServiceTests()
    {
        _sut = new SalaryHistoryService(_repo.Object);
    }

    private static HrSalaryGrade Grade(int gradeNumber) => new() { GradeNumber = gradeNumber };

    // ---------- GetHistoryAsync (US-SGP-08) ----------

    [Fact]
    public async Task GetHistoryAsync_ReturnsNewestFirst_AndMarksCurrent()
    {
        var older = new HrEmployeeSalary { EmployeeId = 5, SalaryGrade = Grade(3), EffectiveDate = new DateOnly(2022, 1, 1), Coefficient = 3.0m, Reason = "Initial assignment" };
        var newer = new HrEmployeeSalary { EmployeeId = 5, SalaryGrade = Grade(4), EffectiveDate = new DateOnly(2024, 1, 1), Coefficient = 3.66m, Reason = "Promotion" };
        _repo.Setup(r => r.QueryHistory(5)).Returns(new[] { older, newer }.AsQueryable());

        var result = await _sut.GetHistoryAsync(5, null, null);

        result.Should().HaveCount(2);
        result[0].EffectiveDate.Should().Be(newer.EffectiveDate);
        result[0].IsCurrent.Should().BeTrue();
        result[1].EffectiveDate.Should().Be(older.EffectiveDate);
        result[1].IsCurrent.Should().BeFalse();
    }

    [Fact]
    public async Task GetHistoryAsync_LinksToCausingDecision_WhenPresent()
    {
        var entry = new HrEmployeeSalary
        {
            EmployeeId = 5,
            SalaryGrade = Grade(4),
            EffectiveDate = new DateOnly(2024, 1, 1),
            Coefficient = 3.66m,
            Reason = "Applied decision",
            SalaryDecisionId = 7,
            SalaryDecision = new HrSalaryDecision { Id = 7, DecisionNumber = "SD-2024-001" }
        };
        _repo.Setup(r => r.QueryHistory(5)).Returns(new[] { entry }.AsQueryable());

        var result = await _sut.GetHistoryAsync(5, null, null);

        result[0].DecisionNumber.Should().Be("SD-2024-001"); // US-SGP-08 AC02
    }

    [Fact]
    public async Task GetHistoryAsync_FiltersByDateRange()
    {
        var entry2022 = new HrEmployeeSalary { EmployeeId = 5, SalaryGrade = Grade(3), EffectiveDate = new DateOnly(2022, 1, 1), Coefficient = 3.0m, Reason = "x" };
        var entry2024 = new HrEmployeeSalary { EmployeeId = 5, SalaryGrade = Grade(4), EffectiveDate = new DateOnly(2024, 1, 1), Coefficient = 3.66m, Reason = "y" };
        _repo.Setup(r => r.QueryHistory(5)).Returns(new[] { entry2022, entry2024 }.AsQueryable());

        var result = await _sut.GetHistoryAsync(5, fromDate: new DateOnly(2023, 1, 1), toDate: null);

        result.Should().ContainSingle();
        result[0].EffectiveDate.Should().Be(entry2024.EffectiveDate);
    }

    // ---------- HasActiveEmployeeOnGradeAsync (cross-domain for Salary Master Data, BR-SAL-15) ----------

    [Fact]
    public async Task HasActiveEmployeeOnGradeAsync_DelegatesToRepository()
    {
        _repo.Setup(r => r.HasActiveEmployeeOnGradeAsync(11, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        (await _sut.HasActiveEmployeeOnGradeAsync(11)).Should().BeTrue();
    }
}
