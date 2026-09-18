using HRM.Application.Common;
using HRM.Application.EmployeeManagement.Interfaces;
using HRM.Application.Exceptions;
using HRM.Application.OrganizationManagement.Interfaces;
using HRM.Application.OrganizationManagement.Models;
using HRM.Domain.Entities;
using HRM.Domain.Enums;

namespace HRM.Application.OrganizationManagement.Services;

/// <inheritdoc cref="IOrganizationalUnitService"/>
public class OrganizationalUnitService : IOrganizationalUnitService
{
    private readonly IOrganizationalUnitRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    // Lazy: EmployeeService also depends on this service (IsUnitActiveAsync,
    // BR-EMP-04), so a plain constructor-injected IEmployeeService here would
    // be a circular dependency the DI container can't resolve. Deferring
    // resolution until DeactivateUnitAsync actually runs breaks the cycle.
    private readonly Lazy<IEmployeeService> _employeeService;

    public OrganizationalUnitService(
        IOrganizationalUnitRepository repository,
        IUnitOfWork unitOfWork,
        Lazy<IEmployeeService> employeeService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _employeeService = employeeService;
    }

    public async Task<HrOrganizationalUnit> CreateUnitAsync(CreateOrganizationalUnitInput input, CancellationToken ct = default)
    {
        if (input.ContactEmail is not null && !IsValidEmail(input.ContactEmail))
        {
            throw new ValidationException("Contact email is not a valid email format."); // BR-ORG-22
        }

        if (input.ParentId is int parentId)
        {
            var parent = await _repository.GetByIdAsync(parentId, ct)
                ?? throw new ValidationException("Parent organizational unit does not exist.");
            if (parent.Status != ActiveStatus.ACTIVE)
            {
                throw new ValidationException("Parent organizational unit is inactive."); // BR-ORG-04
            }
        }

        if (await _repository.GetByNameUnderParentAsync(input.ParentId, input.Name, ct) is not null)
        {
            throw new ConflictException("An organizational unit with this name already exists under the same parent."); // BR-ORG-03
        }

        var unit = new HrOrganizationalUnit
        {
            Name = input.Name,
            ParentId = input.ParentId,
            UnitType = input.UnitType,
            ContactEmail = input.ContactEmail,
            ContactPhone = input.ContactPhone,
            Status = ActiveStatus.ACTIVE
        };
        await _repository.AddAsync(unit, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return unit;
    }

    public async Task<IReadOnlyList<HrOrganizationalUnit>> GetStructureAsync(CancellationToken ct = default) =>
        await _repository.GetAllAsync(ct); // BR-ORG-15: all units, active and inactive

    public async Task<HrOrganizationalUnit> UpdateUnitAsync(int unitId, UpdateOrganizationalUnitInput input, CancellationToken ct = default)
    {
        var unit = await GetOrThrowAsync(unitId, ct);

        if (input.ContactEmail is not null && !IsValidEmail(input.ContactEmail))
        {
            throw new ValidationException("Contact email is not a valid email format.");
        }

        if (input.Name is not null && input.Name != unit.Name)
        {
            if (await _repository.GetByNameUnderParentAsync(unit.ParentId, input.Name, ct) is not null)
            {
                throw new ConflictException("An organizational unit with this name already exists under the same parent."); // BR-ORG-03
            }
            unit.Name = input.Name;
        }

        if (input.UnitType is not null)
        {
            unit.UnitType = input.UnitType;
        }
        if (input.ContactEmail is not null)
        {
            unit.ContactEmail = input.ContactEmail;
        }
        if (input.ContactPhone is not null)
        {
            unit.ContactPhone = input.ContactPhone;
        }

        // ParentId is not editable here — moving a unit is MoveUnitAsync (BR-ORG-05/06).
        await _unitOfWork.SaveChangesAsync(ct);
        return unit;
    }

    public async Task<HrOrganizationalUnit> MoveUnitAsync(int unitId, int targetParentId, CancellationToken ct = default)
    {
        var unit = await GetOrThrowAsync(unitId, ct);

        var descendantIds = await _repository.GetDescendantIdsAsync(unitId, ct);
        if (targetParentId == unitId || descendantIds.Contains(targetParentId))
        {
            throw new ValidationException("Target parent cannot be the unit itself or one of its descendants."); // BR-ORG-07
        }

        var target = await _repository.GetByIdAsync(targetParentId, ct)
            ?? throw new ValidationException("Target parent organizational unit does not exist.");
        if (target.Status != ActiveStatus.ACTIVE)
        {
            throw new ValidationException("Target parent organizational unit is inactive."); // BR-ORG-04
        }

        var existing = await _repository.GetByNameUnderParentAsync(targetParentId, unit.Name, ct);
        if (existing is not null && existing.Id != unit.Id)
        {
            throw new ConflictException("An organizational unit with this name already exists under the target parent."); // BR-ORG-03
        }

        // Descendants keep their own ParentId — moving the subtree root is
        // enough to bring the whole subtree with it (BR-ORG-08).
        unit.ParentId = targetParentId;
        await _unitOfWork.SaveChangesAsync(ct);
        return unit;
    }

    public async Task<HrOrganizationalUnit> DeactivateUnitAsync(int unitId, CancellationToken ct = default)
    {
        var unit = await GetOrThrowAsync(unitId, ct);

        if (await _repository.CountActiveChildrenAsync(unitId, ct) > 0)
        {
            throw new ConflictException("Organizational unit has active child units."); // BR-ORG-10
        }

        if (await _employeeService.Value.HasActiveEmployeesInUnitAsync(unitId, ct))
        {
            throw new ConflictException("Organizational unit has active employees assigned."); // BR-ORG-11
        }

        unit.Status = ActiveStatus.INACTIVE;
        await _unitOfWork.SaveChangesAsync(ct);
        return unit;
    }

    public async Task<HrOrganizationalUnit> ReactivateUnitAsync(int unitId, CancellationToken ct = default)
    {
        var unit = await GetOrThrowAsync(unitId, ct);

        if (unit.ParentId is int parentId)
        {
            var parent = await _repository.GetByIdAsync(parentId, ct);
            if (parent is not null && parent.Status != ActiveStatus.ACTIVE)
            {
                throw new ConflictException("Parent organizational unit must be reactivated first."); // BR-ORG-14
            }
        }

        unit.Status = ActiveStatus.ACTIVE;
        await _unitOfWork.SaveChangesAsync(ct);
        return unit;
    }

    public async Task<bool> IsUnitActiveAsync(int unitId, CancellationToken ct = default)
    {
        var unit = await _repository.GetByIdAsync(unitId, ct);
        return unit is not null && unit.Status == ActiveStatus.ACTIVE;
    }

    private async Task<HrOrganizationalUnit> GetOrThrowAsync(int unitId, CancellationToken ct) =>
        await _repository.GetByIdAsync(unitId, ct)
            ?? throw new NotFoundException($"Organizational unit {unitId} was not found.");

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new System.Net.Mail.MailAddress(email);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
