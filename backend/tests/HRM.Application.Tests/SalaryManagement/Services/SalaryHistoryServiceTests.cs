using FluentAssertions;
using HRM.Application.SalaryManagement.Interfaces;
using HRM.Application.SalaryManagement.Services;
using HRM.Domain.Entities;
using Moq;
using Xunit;

namespace HRM.Application.Tests.SalaryManagement.Services;

public class SalaryHistoryServiceTests
{
    private readonly Mock<ISalaryRepository> _salaryRepo = new();
    private readonly SalaryHistoryService _sut;

    public SalaryHistoryServiceTests()
    {
        _sut = new SalaryHistoryService(_salaryRepo.Object);
    }

    [Fact]
    public async Task GetHistory_ReturnsNewestFirst()
    {
        var grade = new HrSalaryGrade { Id = 1, GradeNumber = 3 };
        var older = new HrEmployeeSalary { EmployeeId = 5, SalaryGrade = grade, EffectiveFrom = new DateOnly(2022, 1, 1), EffectiveTo = new DateOnly(2023, 12, 31), Coefficient = 3.0m };
        var newer = new HrEmployeeSalary { EmployeeId = 5, SalaryGrade = grade, EffectiveFrom = new DateOnly(2024, 1, 1), EffectiveTo = null, Coefficient = 3.66m };
        _salaryRepo.Setup(r => r.QuerySalaryHistory(5)).Returns(new[] { older, newer }.AsQueryable());

        var result = await _sut.GetHistoryAsync(5, null, null);

        result.Should().HaveCount(2);
        result[0].EffectiveFrom.Should().Be(newer.EffectiveFrom);
        result[0].IsCurrent.Should().BeTrue();
        result[1].EffectiveFrom.Should().Be(older.EffectiveFrom);
        result[1].IsCurrent.Should().BeFalse();
    }

    [Fact]
    public async Task GetHistory_FiltersByDateRange()
    {
        var grade = new HrSalaryGrade { Id = 1, GradeNumber = 3 };
        var entry2022 = new HrEmployeeSalary { EmployeeId = 5, SalaryGrade = grade, EffectiveFrom = new DateOnly(2022, 1, 1), Coefficient = 3.0m };
        var entry2024 = new HrEmployeeSalary { EmployeeId = 5, SalaryGrade = grade, EffectiveFrom = new DateOnly(2024, 1, 1), Coefficient = 3.66m };
        _salaryRepo.Setup(r => r.QuerySalaryHistory(5)).Returns(new[] { entry2022, entry2024 }.AsQueryable());

        var result = await _sut.GetHistoryAsync(5, fromDate: new DateOnly(2023, 1, 1), toDate: null);

        result.Should().ContainSingle();
        result[0].EffectiveFrom.Should().Be(entry2024.EffectiveFrom);
    }
}
