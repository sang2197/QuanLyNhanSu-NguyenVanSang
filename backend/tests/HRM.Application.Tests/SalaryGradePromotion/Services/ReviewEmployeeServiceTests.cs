using FluentAssertions;
using HRM.Application.Common;
using HRM.Application.Exceptions;
using HRM.Application.SalaryGradePromotion.Interfaces;
using HRM.Application.SalaryGradePromotion.Services;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Moq;
using Xunit;

namespace HRM.Application.Tests.SalaryGradePromotion.Services;

public class ReviewEmployeeServiceTests
{
    private readonly Mock<IReviewEmployeeRepository> _repo = new();
    private readonly Mock<IReviewPeriodRepository> _periodRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ReviewEmployeeService _sut;

    public ReviewEmployeeServiceTests()
    {
        // Default: period is IN_PROGRESS, so the US-SGP-04 AC10 guard doesn't
        // need explicit setup in every unrelated test.
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.IN_PROGRESS });

        _sut = new ReviewEmployeeService(_repo.Object, _periodRepo.Object, _unitOfWork.Object);
    }

    private static HrSalaryReviewEmployee EligibleEntry(int employeeId = 1) =>
        new() { ReviewPeriodId = 1, EmployeeId = employeeId, Eligible = true, Outcome = ReviewOutcome.PENDING };

    // ---------- ApproveEmployeeAsync (US-SGP-04) ----------

    [Fact]
    public async Task ApproveEmployeeAsync_PeriodNotInProgress_ThrowsConflict()
    {
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.SUBMITTED });

        var act = () => _sut.ApproveEmployeeAsync(1, 1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ApproveEmployeeAsync_NotEligible_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByPeriodAndEmployeeAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewEmployee { ReviewPeriodId = 1, EmployeeId = 1, Eligible = false });

        var act = () => _sut.ApproveEmployeeAsync(1, 1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ApproveEmployeeAsync_ChangingFromRejected_ClearsRejectionReason()
    {
        var entry = EligibleEntry();
        entry.Outcome = ReviewOutcome.REJECTED;
        entry.RejectionReason = "Not ready";
        _repo.Setup(r => r.GetByPeriodAndEmployeeAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(entry);

        var result = await _sut.ApproveEmployeeAsync(1, 1);

        result.Outcome.Should().Be(ReviewOutcome.APPROVED);
        result.RejectionReason.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- RejectEmployeeAsync (US-SGP-04) ----------

    [Fact]
    public async Task RejectEmployeeAsync_MissingReason_ThrowsValidation()
    {
        var act = () => _sut.RejectEmployeeAsync(1, 1, "");

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RejectEmployeeAsync_PeriodNotInProgress_ThrowsConflict()
    {
        _periodRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrSalaryReviewPeriod { Id = 1, Status = ReviewPeriodStatus.SUBMITTED });

        var act = () => _sut.RejectEmployeeAsync(1, 1, "Not ready");

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task RejectEmployeeAsync_ValidReason_RecordsRejectionReason()
    {
        _repo.Setup(r => r.GetByPeriodAndEmployeeAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(EligibleEntry());

        var result = await _sut.RejectEmployeeAsync(1, 1, "Not ready");

        result.Outcome.Should().Be(ReviewOutcome.REJECTED);
        result.RejectionReason.Should().Be("Not ready");
    }

    // ---------- BulkApproveAsync (US-SGP-04 AC04/AC05) ----------

    [Fact]
    public async Task BulkApproveAsync_PartialSuccess_ReportsSucceededAndFailed()
    {
        _repo.Setup(r => r.GetByPeriodAndEmployeeAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(EligibleEntry(1));
        _repo.Setup(r => r.GetByPeriodAndEmployeeAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewEmployee { ReviewPeriodId = 1, EmployeeId = 2, Eligible = false });

        var result = await _sut.BulkApproveAsync(1, new[] { 1, 2 });

        result.SucceededEmployeeIds.Should().ContainSingle(id => id == 1);
        result.Failed.Should().ContainSingle(f => f.EmployeeId == 2);
    }

    // ---------- BulkRejectAsync (US-SGP-04 AC06/AC07/AC09) ----------

    [Fact]
    public async Task BulkRejectAsync_MissingReason_ThrowsValidationForWholeBatch()
    {
        var act = () => _sut.BulkRejectAsync(1, new[] { 1, 2 }, "");

        await act.Should().ThrowAsync<ValidationException>();
        _repo.Verify(r => r.GetByPeriodAndEmployeeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BulkRejectAsync_PartialSuccess_RetainsReasonForSuccessfulProposals()
    {
        _repo.Setup(r => r.GetByPeriodAndEmployeeAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(EligibleEntry(1));
        _repo.Setup(r => r.GetByPeriodAndEmployeeAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrSalaryReviewEmployee { ReviewPeriodId = 1, EmployeeId = 2, Eligible = false });

        var result = await _sut.BulkRejectAsync(1, new[] { 1, 2 }, "Budget freeze");

        result.SucceededEmployeeIds.Should().ContainSingle(id => id == 1);
        result.Failed.Should().ContainSingle(f => f.EmployeeId == 2);
    }

    // ---------- ListReviewEmployeesAsync (US-SGP-03 AC06) ----------

    [Fact]
    public async Task ListReviewEmployeesAsync_FiltersByEligibleAndOutcome()
    {
        var entries = new[]
        {
            new HrSalaryReviewEmployee { ReviewPeriodId = 1, EmployeeId = 1, Eligible = true, Outcome = ReviewOutcome.PENDING, Employee = new HrEmployee() },
            new HrSalaryReviewEmployee { ReviewPeriodId = 1, EmployeeId = 2, Eligible = false, Employee = new HrEmployee() }
        };
        _repo.Setup(r => r.QueryByPeriod(1)).Returns(entries.AsQueryable());

        var result = await _sut.ListReviewEmployeesAsync(1, null, eligible: true, outcome: null, page: 1, pageSize: 20);

        result.Items.Should().ContainSingle(e => e.EmployeeId == 1);
    }

    // ---------- GetReviewEmployeeAsync ----------

    [Fact]
    public async Task GetReviewEmployeeAsync_NotFound_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetByPeriodAndEmployeeAsync(1, 99, It.IsAny<CancellationToken>())).ReturnsAsync((HrSalaryReviewEmployee?)null);

        var act = () => _sut.GetReviewEmployeeAsync(1, 99);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
