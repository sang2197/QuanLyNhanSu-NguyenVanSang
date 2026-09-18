using FluentAssertions;
using HRM.Application.Common;
using HRM.Application.Exceptions;
using HRM.Application.SalaryMasterData.Interfaces;
using HRM.Application.SalaryMasterData.Services;
using HRM.Domain.Entities;
using Moq;
using Xunit;

namespace HRM.Application.Tests.SalaryMasterData.Services;

public class BaseSalaryRateServiceTests
{
    private readonly Mock<IBaseSalaryRateRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly BaseSalaryRateService _sut;

    public BaseSalaryRateServiceTests()
    {
        _sut = new BaseSalaryRateService(_repo.Object, _unitOfWork.Object);
    }

    // ---------- AddRateAsync (BR-SAL-01/02/03/04) ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddRateAsync_RateZeroOrNegative_ThrowsValidation(decimal rate)
    {
        var act = () => _sut.AddRateAsync(rate, new DateOnly(2026, 1, 1));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task AddRateAsync_EffectiveDateNotAfterLatest_ThrowsConflict()
    {
        _repo.Setup(r => r.GetLatestAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrBaseSalaryRate { Id = 1, EffectiveDate = new DateOnly(2026, 1, 1) });

        var act = () => _sut.AddRateAsync(100, new DateOnly(2026, 1, 1));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task AddRateAsync_NoExistingRate_AddsAndSaves()
    {
        _repo.Setup(r => r.GetLatestAsync(It.IsAny<CancellationToken>())).ReturnsAsync((HrBaseSalaryRate?)null);

        var rate = await _sut.AddRateAsync(100, new DateOnly(2026, 1, 1));

        rate.Rate.Should().Be(100);
        _repo.Verify(r => r.AddAsync(It.IsAny<HrBaseSalaryRate>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddRateAsync_LaterEffectiveDateThanLatest_AddsAndSaves()
    {
        _repo.Setup(r => r.GetLatestAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrBaseSalaryRate { Id = 1, EffectiveDate = new DateOnly(2026, 1, 1) });

        var rate = await _sut.AddRateAsync(120, new DateOnly(2026, 6, 1));

        rate.EffectiveDate.Should().Be(new DateOnly(2026, 6, 1));
    }

    // ---------- ListRatesAsync ----------

    [Fact]
    public async Task ListRatesAsync_ReturnsFromRepository()
    {
        _repo.Setup(r => r.ListAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HrBaseSalaryRate> { new() { Id = 1 }, new() { Id = 2 } });

        var result = await _sut.ListRatesAsync(null);

        result.Should().HaveCount(2);
    }

    // ---------- GetRateAsOfAsync ----------

    [Fact]
    public async Task GetRateAsOfAsync_NoRateEffectiveByThatDate_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetAsOfAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).ReturnsAsync((HrBaseSalaryRate?)null);

        var act = () => _sut.GetRateAsOfAsync(new DateOnly(2020, 1, 1));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetRateAsOfAsync_RateExists_ReturnsIt()
    {
        var rate = new HrBaseSalaryRate { Id = 1, Rate = 100, EffectiveDate = new DateOnly(2026, 1, 1) };
        _repo.Setup(r => r.GetAsOfAsync(new DateOnly(2026, 6, 1), It.IsAny<CancellationToken>())).ReturnsAsync(rate);

        var result = await _sut.GetRateAsOfAsync(new DateOnly(2026, 6, 1));

        result.Should().Be(rate);
    }
}
