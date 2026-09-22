using FluentAssertions;
using HRM.Application.Common;
using HRM.Application.ContractManagement.Interfaces;
using HRM.Application.ContractManagement.Models;
using HRM.Application.ContractManagement.Services;
using HRM.Application.EmployeeManagement.Interfaces;
using HRM.Application.Exceptions;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Moq;
using Xunit;

namespace HRM.Application.Tests.ContractManagement.Services;

public class ContractServiceTests
{
    private readonly Mock<IContractRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IEmployeeService> _employeeService = new();
    private readonly ContractService _sut;

    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public ContractServiceTests()
    {
        // Default: employee exists and is not Terminated, so BR-CON-07/18
        // guards don't need explicit setup in every unrelated test.
        _employeeService.Setup(s => s.GetEmployeeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrEmployee { Id = 1, EmploymentStatus = EmploymentStatus.ACTIVE });

        _sut = new ContractService(_repo.Object, _unitOfWork.Object, _employeeService.Object);
    }

    private static CreateContractInput ValidCreateInput(
        ContractType type = ContractType.FIXED_TERM,
        ContractStatus status = ContractStatus.DRAFT,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        bool noEndDate = false) =>
        new(EmployeeId: 1, type, "CT-001", startDate ?? Today, noEndDate ? null : (endDate ?? Today.AddYears(1)), 20_000_000m, null, status);

    private static UpdateContractInput ValidUpdateInput() =>
        new(ContractType.FIXED_TERM, "CT-001", Today, Today.AddYears(1), 20_000_000m, null);

    // ---------- CreateContractAsync (BR-CON-01/02/04-10) ----------

    [Fact]
    public async Task CreateContractAsync_DuplicateNumber_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByNumberAsync("CT-001", It.IsAny<CancellationToken>())).ReturnsAsync(new HrLaborContract { Id = 1 });

        var act = () => _sut.CreateContractAsync(ValidCreateInput());

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateContractAsync_EmployeeTerminated_ThrowsConflict()
    {
        _employeeService.Setup(s => s.GetEmployeeAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrEmployee { Id = 1, EmploymentStatus = EmploymentStatus.TERMINATED });

        var act = () => _sut.CreateContractAsync(ValidCreateInput());

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateContractAsync_FixedTermMissingEndDate_ThrowsValidation()
    {
        var act = () => _sut.CreateContractAsync(ValidCreateInput(noEndDate: true));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateContractAsync_IndefiniteTermWithEndDate_ThrowsValidation()
    {
        var act = () => _sut.CreateContractAsync(ValidCreateInput(type: ContractType.INDEFINITE_TERM, endDate: Today.AddYears(1)));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateContractAsync_IndefiniteTermWithoutEndDate_Succeeds()
    {
        var contract = await _sut.CreateContractAsync(ValidCreateInput(type: ContractType.INDEFINITE_TERM, noEndDate: true));

        contract.EndDate.Should().BeNull();
        contract.ContractType.Should().Be(ContractType.INDEFINITE_TERM);
    }

    [Fact]
    public async Task CreateContractAsync_EndDateNotLaterThanStartDate_ThrowsValidation()
    {
        var act = () => _sut.CreateContractAsync(ValidCreateInput(startDate: Today, endDate: Today));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateContractAsync_NonPositiveSalary_ThrowsValidation()
    {
        var input = ValidCreateInput() with { ContractSalaryAmount = 0 };

        var act = () => _sut.CreateContractAsync(input);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateContractAsync_ActiveWithFutureStartDate_ThrowsConflict()
    {
        var input = ValidCreateInput(status: ContractStatus.ACTIVE, startDate: Today.AddDays(5), endDate: Today.AddYears(1));

        var act = () => _sut.CreateContractAsync(input);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateContractAsync_ActiveWithAnotherActiveContract_ThrowsConflict()
    {
        _repo.Setup(r => r.HasOtherActiveForEmployeeAsync(1, null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.CreateContractAsync(ValidCreateInput(status: ContractStatus.ACTIVE));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateContractAsync_ValidDraft_CreatesAndSaves()
    {
        var contract = await _sut.CreateContractAsync(ValidCreateInput());

        contract.Status.Should().Be(ContractStatus.DRAFT);
        _repo.Verify(r => r.AddAsync(It.IsAny<HrLaborContract>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateContractAsync_ValidActive_CreatesAsActive()
    {
        var contract = await _sut.CreateContractAsync(ValidCreateInput(status: ContractStatus.ACTIVE));

        contract.Status.Should().Be(ContractStatus.ACTIVE);
    }

    // ---------- SearchContractsAsync (BR-CON-11-14, 25-27, 32) ----------

    [Fact]
    public async Task SearchContractsAsync_FiltersBySearchTerm()
    {
        var employee1 = new HrEmployee { Id = 1, EmployeeCode = "EMP-001", FullName = "An Nguyen" };
        var employee2 = new HrEmployee { Id = 2, EmployeeCode = "EMP-002", FullName = "Binh Tran" };
        var contracts = new List<HrLaborContract>
        {
            new() { Id = 1, ContractNumber = "CT-001", Employee = employee1 },
            new() { Id = 2, ContractNumber = "CT-002", Employee = employee2 }
        };
        _repo.Setup(r => r.Query()).Returns(contracts.AsQueryable());

        var result = await _sut.SearchContractsAsync("An", null, null, null, null, false, 30, 1, 20);

        result.Items.Should().ContainSingle(c => c.Id == 1);
    }

    [Fact]
    public async Task SearchContractsAsync_ExpiringSoon_ReturnsOverdueAndWithinWindow_OrderedByEndDate()
    {
        var employee = new HrEmployee { Id = 1, EmployeeCode = "EMP-001", FullName = "An Nguyen" };
        var contracts = new List<HrLaborContract>
        {
            new() { Id = 1, ContractNumber = "CT-OVERDUE", Employee = employee, Status = ContractStatus.ACTIVE, EndDate = Today.AddDays(-5) },
            new() { Id = 2, ContractNumber = "CT-SOON", Employee = employee, Status = ContractStatus.ACTIVE, EndDate = Today.AddDays(10) },
            new() { Id = 3, ContractNumber = "CT-FAR", Employee = employee, Status = ContractStatus.ACTIVE, EndDate = Today.AddDays(90) },
            new() { Id = 4, ContractNumber = "CT-INDEFINITE", Employee = employee, Status = ContractStatus.ACTIVE, EndDate = null },
            new() { Id = 5, ContractNumber = "CT-DRAFT", Employee = employee, Status = ContractStatus.DRAFT, EndDate = Today.AddDays(1) }
        };
        _repo.Setup(r => r.Query()).Returns(contracts.AsQueryable());

        var result = await _sut.SearchContractsAsync(null, null, null, null, null, expiringSoon: true, window: 30, 1, 20);

        result.Items.Select(c => c.Id).Should().Equal(1, 2); // overdue first, then the nearest — earliest EndDate first
    }

    // ---------- GetContractAsync ----------

    [Fact]
    public async Task GetContractAsync_NotFound_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrLaborContract?)null);

        var act = () => _sut.GetContractAsync(1);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ---------- UpdateContractAsync (BR-CON-01, 04-06, 28-30) ----------

    [Fact]
    public async Task UpdateContractAsync_NotDraft_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.ACTIVE });

        var act = () => _sut.UpdateContractAsync(1, ValidUpdateInput());

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateContractAsync_ChangedNumberDuplicatesExisting_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.DRAFT, ContractNumber = "CT-001" });
        _repo.Setup(r => r.GetByNumberAsync("CT-002", It.IsAny<CancellationToken>())).ReturnsAsync(new HrLaborContract { Id = 2 });

        var act = () => _sut.UpdateContractAsync(1, ValidUpdateInput() with { ContractNumber = "CT-002" });

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateContractAsync_SameNumber_DoesNotCheckDuplicate()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.DRAFT, ContractNumber = "CT-001" });

        await _sut.UpdateContractAsync(1, ValidUpdateInput());

        _repo.Verify(r => r.GetByNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateContractAsync_InvalidEndDateForType_ThrowsValidation()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.DRAFT, ContractNumber = "CT-001" });

        var act = () => _sut.UpdateContractAsync(1, ValidUpdateInput() with { EndDate = null });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateContractAsync_ValidChanges_UpdatesAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.DRAFT, ContractNumber = "CT-001" });

        var result = await _sut.UpdateContractAsync(1, ValidUpdateInput() with { SalaryNote = "Updated" });

        result.SalaryNote.Should().Be("Updated");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- DeleteContractAsync (BR-CON-28/31) ----------

    [Fact]
    public async Task DeleteContractAsync_NotDraft_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.ACTIVE });

        var act = () => _sut.DeleteContractAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task DeleteContractAsync_Draft_RemovesAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.DRAFT });

        await _sut.DeleteContractAsync(1);

        _repo.Verify(r => r.RemoveAsync(It.IsAny<HrLaborContract>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- ActivateContractAsync (BR-CON-16/18) ----------

    [Fact]
    public async Task ActivateContractAsync_NotDraft_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.ACTIVE });

        var act = () => _sut.ActivateContractAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ActivateContractAsync_FutureStartDate_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrLaborContract { Id = 1, EmployeeId = 1, Status = ContractStatus.DRAFT, StartDate = Today.AddDays(1) });

        var act = () => _sut.ActivateContractAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ActivateContractAsync_EmployeeHasOtherActiveContract_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrLaborContract { Id = 1, EmployeeId = 1, Status = ContractStatus.DRAFT, StartDate = Today });
        _repo.Setup(r => r.HasOtherActiveForEmployeeAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.ActivateContractAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ActivateContractAsync_EmployeeTerminated_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrLaborContract { Id = 1, EmployeeId = 1, Status = ContractStatus.DRAFT, StartDate = Today });
        _employeeService.Setup(s => s.GetEmployeeAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrEmployee { Id = 1, EmploymentStatus = EmploymentStatus.TERMINATED });

        var act = () => _sut.ActivateContractAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ActivateContractAsync_ValidDraft_ActivatesAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrLaborContract { Id = 1, EmployeeId = 1, Status = ContractStatus.DRAFT, StartDate = Today });

        var result = await _sut.ActivateContractAsync(1);

        result.Status.Should().Be(ContractStatus.ACTIVE);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- ExpireContractAsync (BR-CON-16/19) ----------

    [Fact]
    public async Task ExpireContractAsync_NotActive_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.DRAFT });

        var act = () => _sut.ExpireContractAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ExpireContractAsync_NoEndDate_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.ACTIVE, EndDate = null });

        var act = () => _sut.ExpireContractAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ExpireContractAsync_EndDateNotYetPassed_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.ACTIVE, EndDate = Today });

        var act = () => _sut.ExpireContractAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ExpireContractAsync_EndDatePassed_ExpiresAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.ACTIVE, EndDate = Today.AddDays(-1) });

        var result = await _sut.ExpireContractAsync(1);

        result.Status.Should().Be(ContractStatus.EXPIRED);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- TerminateContractAsync (BR-CON-16/21/22) ----------

    [Fact]
    public async Task TerminateContractAsync_MissingReason_ThrowsValidation()
    {
        var act = () => _sut.TerminateContractAsync(1, Today, "");

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task TerminateContractAsync_NotActive_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.DRAFT });

        var act = () => _sut.TerminateContractAsync(1, Today, "Resignation");

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task TerminateContractAsync_DateBeforeStart_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.ACTIVE, StartDate = Today, EndDate = Today.AddYears(1) });

        var act = () => _sut.TerminateContractAsync(1, Today.AddDays(-1), "Resignation");

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task TerminateContractAsync_DateAfterEnd_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.ACTIVE, StartDate = Today, EndDate = Today.AddDays(10) });

        var act = () => _sut.TerminateContractAsync(1, Today.AddDays(11), "Resignation");

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task TerminateContractAsync_ValidDateAndReason_TerminatesAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.ACTIVE, StartDate = Today, EndDate = Today.AddYears(1) });

        var result = await _sut.TerminateContractAsync(1, Today.AddDays(5), "Resignation accepted");

        result.Status.Should().Be(ContractStatus.TERMINATED);
        result.TerminationDate.Should().Be(Today.AddDays(5));
        result.TerminationReason.Should().Be("Resignation accepted");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TerminateContractAsync_IndefiniteTermNoEndDate_AllowsAnyLaterDate()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrLaborContract { Id = 1, Status = ContractStatus.ACTIVE, StartDate = Today, EndDate = null });

        var result = await _sut.TerminateContractAsync(1, Today.AddYears(5), "Resignation");

        result.Status.Should().Be(ContractStatus.TERMINATED);
    }
}
