using FluentAssertions;
using HRM.Application.Common;
using HRM.Application.EmployeeManagement.Interfaces;
using HRM.Application.Exceptions;
using HRM.Application.OrganizationManagement.Interfaces;
using HRM.Application.OrganizationManagement.Models;
using HRM.Application.OrganizationManagement.Services;
using HRM.Domain.Entities;
using HRM.Domain.Enums;
using Moq;
using Xunit;

namespace HRM.Application.Tests.OrganizationManagement.Services;

public class OrganizationalUnitServiceTests
{
    private readonly Mock<IOrganizationalUnitRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IEmployeeService> _employeeService = new();
    private readonly OrganizationalUnitService _sut;

    public OrganizationalUnitServiceTests()
    {
        // Default: no active children and no active employees, so the
        // BR-ORG-10/11 deactivate guards don't need explicit setup in every
        // unrelated test.
        _repo.Setup(r => r.CountActiveChildrenAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _employeeService.Setup(e => e.HasActiveEmployeesInUnitAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        _sut = new OrganizationalUnitService(_repo.Object, _unitOfWork.Object, new Lazy<IEmployeeService>(() => _employeeService.Object));
    }

    // ---------- CreateUnitAsync (BR-ORG-01) ----------

    [Fact]
    public async Task CreateUnitAsync_InvalidContactEmail_ThrowsValidation()
    {
        var input = new CreateOrganizationalUnitInput("Sales", null, "Department", "not-an-email", null);

        var act = () => _sut.CreateUnitAsync(input);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateUnitAsync_ParentDoesNotExist_ThrowsValidation()
    {
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((HrOrganizationalUnit?)null);
        var input = new CreateOrganizationalUnitInput("Sales", 99, "Department", null, null);

        var act = () => _sut.CreateUnitAsync(input);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateUnitAsync_ParentInactive_ThrowsValidation()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrOrganizationalUnit { Id = 1, Status = ActiveStatus.INACTIVE });
        var input = new CreateOrganizationalUnitInput("Sales", 1, "Department", null, null);

        var act = () => _sut.CreateUnitAsync(input);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateUnitAsync_DuplicateNameUnderParent_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByNameUnderParentAsync(null, "Sales", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrOrganizationalUnit { Id = 5, Name = "Sales" });
        var input = new CreateOrganizationalUnitInput("Sales", null, "Department", null, null);

        var act = () => _sut.CreateUnitAsync(input);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateUnitAsync_ValidRequest_CreatesActiveUnitAndSaves()
    {
        var input = new CreateOrganizationalUnitInput("Sales", null, "Department", "sales@corp.com", "555-0100");

        var unit = await _sut.CreateUnitAsync(input);

        unit.Name.Should().Be("Sales");
        unit.Status.Should().Be(ActiveStatus.ACTIVE);
        _repo.Verify(r => r.AddAsync(It.IsAny<HrOrganizationalUnit>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- GetStructureAsync (BR-ORG-15) ----------

    [Fact]
    public async Task GetStructureAsync_ReturnsAllUnitsActiveAndInactive()
    {
        var units = new List<HrOrganizationalUnit>
        {
            new() { Id = 1, Status = ActiveStatus.ACTIVE },
            new() { Id = 2, Status = ActiveStatus.INACTIVE }
        };
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(units);

        var result = await _sut.GetStructureAsync();

        result.Should().HaveCount(2);
    }

    // ---------- UpdateUnitAsync (BR-ORG-03/05/06) ----------

    [Fact]
    public async Task UpdateUnitAsync_UnitNotFound_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((HrOrganizationalUnit?)null);

        var act = () => _sut.UpdateUnitAsync(1, new UpdateOrganizationalUnitInput(null, null, null, null));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateUnitAsync_InvalidContactEmail_ThrowsValidation()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 1, Name = "Sales" });

        var act = () => _sut.UpdateUnitAsync(1, new UpdateOrganizationalUnitInput(null, null, "bad-email", null));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateUnitAsync_ChangedNameDuplicatesSibling_ThrowsConflict()
    {
        var unit = new HrOrganizationalUnit { Id = 1, Name = "Sales", ParentId = null };
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(unit);
        _repo.Setup(r => r.GetByNameUnderParentAsync(null, "Marketing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrOrganizationalUnit { Id = 2, Name = "Marketing" });

        var act = () => _sut.UpdateUnitAsync(1, new UpdateOrganizationalUnitInput("Marketing", null, null, null));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task UpdateUnitAsync_ValidChanges_UpdatesFieldsAndSaves()
    {
        var unit = new HrOrganizationalUnit { Id = 1, Name = "Sales", UnitType = "Department" };
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(unit);

        var result = await _sut.UpdateUnitAsync(1, new UpdateOrganizationalUnitInput(null, "Division", "new@corp.com", null));

        result.UnitType.Should().Be("Division");
        result.ContactEmail.Should().Be("new@corp.com");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- MoveUnitAsync (BR-ORG-04/07/08/09) ----------

    [Fact]
    public async Task MoveUnitAsync_TargetIsUnitItself_ThrowsValidation()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 1, Name = "Sales" });
        _repo.Setup(r => r.GetDescendantIdsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<int>());

        var act = () => _sut.MoveUnitAsync(1, 1);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task MoveUnitAsync_TargetIsDescendant_ThrowsValidation()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 1, Name = "Sales" });
        _repo.Setup(r => r.GetDescendantIdsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { 2, 3 });

        var act = () => _sut.MoveUnitAsync(1, 3);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task MoveUnitAsync_TargetParentInactive_ThrowsValidation()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 1, Name = "Sales" });
        _repo.Setup(r => r.GetDescendantIdsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<int>());
        _repo.Setup(r => r.GetByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 9, Status = ActiveStatus.INACTIVE });

        var act = () => _sut.MoveUnitAsync(1, 9);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task MoveUnitAsync_DuplicateNameUnderTargetParent_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 1, Name = "Sales" });
        _repo.Setup(r => r.GetDescendantIdsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<int>());
        _repo.Setup(r => r.GetByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 9, Status = ActiveStatus.ACTIVE });
        _repo.Setup(r => r.GetByNameUnderParentAsync(9, "Sales", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HrOrganizationalUnit { Id = 42, Name = "Sales" });

        var act = () => _sut.MoveUnitAsync(1, 9);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task MoveUnitAsync_ValidMove_UpdatesParentIdAndSaves()
    {
        var unit = new HrOrganizationalUnit { Id = 1, Name = "Sales", ParentId = 5 };
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(unit);
        _repo.Setup(r => r.GetDescendantIdsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<int>());
        _repo.Setup(r => r.GetByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 9, Status = ActiveStatus.ACTIVE });
        _repo.Setup(r => r.GetByNameUnderParentAsync(9, "Sales", It.IsAny<CancellationToken>())).ReturnsAsync((HrOrganizationalUnit?)null);

        var result = await _sut.MoveUnitAsync(1, 9);

        result.ParentId.Should().Be(9);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- DeactivateUnitAsync (BR-ORG-10/11) ----------

    [Fact]
    public async Task DeactivateUnitAsync_HasActiveChildren_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 1, Status = ActiveStatus.ACTIVE });
        _repo.Setup(r => r.CountActiveChildrenAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(2);

        var act = () => _sut.DeactivateUnitAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task DeactivateUnitAsync_HasActiveEmployees_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 1, Status = ActiveStatus.ACTIVE });
        _employeeService.Setup(e => e.HasActiveEmployeesInUnitAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.DeactivateUnitAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task DeactivateUnitAsync_NoActiveChildrenOrEmployees_SetsInactiveAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 1, Status = ActiveStatus.ACTIVE });

        var result = await _sut.DeactivateUnitAsync(1);

        result.Status.Should().Be(ActiveStatus.INACTIVE);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- ReactivateUnitAsync (BR-ORG-14) ----------

    [Fact]
    public async Task ReactivateUnitAsync_ParentInactive_ThrowsConflict()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 1, ParentId = 5, Status = ActiveStatus.INACTIVE });
        _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 5, Status = ActiveStatus.INACTIVE });

        var act = () => _sut.ReactivateUnitAsync(1);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ReactivateUnitAsync_NoParent_ReactivatesAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 1, ParentId = null, Status = ActiveStatus.INACTIVE });

        var result = await _sut.ReactivateUnitAsync(1);

        result.Status.Should().Be(ActiveStatus.ACTIVE);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReactivateUnitAsync_ParentActive_ReactivatesAndSaves()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 1, ParentId = 5, Status = ActiveStatus.INACTIVE });
        _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 5, Status = ActiveStatus.ACTIVE });

        var result = await _sut.ReactivateUnitAsync(1);

        result.Status.Should().Be(ActiveStatus.ACTIVE);
    }

    // ---------- IsUnitActiveAsync (cross-domain for Employee Management, BR-EMP-04) ----------

    [Fact]
    public async Task IsUnitActiveAsync_UnitIsActive_ReturnsTrue()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new HrOrganizationalUnit { Id = 1, Status = ActiveStatus.ACTIVE });

        (await _sut.IsUnitActiveAsync(1)).Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsUnitActiveAsync_UnitInactiveOrMissing_ReturnsFalse(bool exists)
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists ? new HrOrganizationalUnit { Id = 1, Status = ActiveStatus.INACTIVE } : null);

        (await _sut.IsUnitActiveAsync(1)).Should().BeFalse();
    }
}
