using HRM.Application.Common;
using HRM.Application.Exceptions;
using HRM.Application.SalaryMasterData.Interfaces;
using HRM.Domain.Entities;

namespace HRM.Application.SalaryMasterData.Services;

/// <inheritdoc cref="IBaseSalaryRateService"/>
public class BaseSalaryRateService : IBaseSalaryRateService
{
    private readonly IBaseSalaryRateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public BaseSalaryRateService(IBaseSalaryRateRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<HrBaseSalaryRate> AddRateAsync(decimal rate, DateOnly effectiveDate, CancellationToken ct = default)
    {
        if (rate <= 0)
        {
            throw new ValidationException("Rate must be greater than zero."); // BR-SAL-04
        }

        var latest = await _repository.GetLatestAsync(ct);
        if (latest is not null && effectiveDate <= latest.EffectiveDate)
        {
            throw new ConflictException("Effective date must be later than the latest recorded effective date."); // BR-SAL-03
        }

        var newRate = new HrBaseSalaryRate { Rate = rate, EffectiveDate = effectiveDate };
        await _repository.AddAsync(newRate, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return newRate;
    }

    public async Task<IReadOnlyList<HrBaseSalaryRate>> ListRatesAsync(DateOnly? asOfDate, CancellationToken ct = default) =>
        await _repository.ListAsync(asOfDate, ct);

    public async Task<HrBaseSalaryRate> GetRateAsOfAsync(DateOnly date, CancellationToken ct = default) =>
        await _repository.GetAsOfAsync(date, ct)
            ?? throw new NotFoundException($"No base salary rate is effective as of {date}.");
}
