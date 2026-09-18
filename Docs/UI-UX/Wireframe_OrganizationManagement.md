# Wireframe & Screen Behavior - Organization Management

This document expands the screens defined in `ScreensHierarchy_OrganizationManagement.md` into concrete UI behavior and form content.

For UI conventions shared across HRM modules, see `UXGuidelines_HRM.md`.

## 1. Organization Structure

Presented as a single **Organization Tree Table**, not a separate chart/tree switcher or a selected-unit side panel.

**Columns:** Organizational Unit | Type | Contact | Status | Actions

**Main Behavior**
- The Organizational Unit column shows the hierarchy via indentation and an expand/collapse control; top-level units appear at the root (US-ORG-02). A unit with no children has no expand/collapse control.
- Both active and inactive units remain visible, with inactive rows visually distinguished (e.g. muted background/border) without reducing text readability (US-ORG-02).
- Units with no employees assigned are still shown (US-ORG-02).
- The Contact column shows the unit's Contact Email and Contact Phone when recorded (stacked as two compact lines if both are present); if neither is recorded, it shows "—" (US-ORG-02).
- Actions are reached through a "…" contextual menu per row rather than several separate icons:
  - Non-top-level unit: Edit, Move, Deactivate/Reactivate.
  - Top-level unit: Edit, Deactivate/Reactivate (no Move — UC-ORG-04 applies only to a unit that already has a parent).
- "Edit" opens Update Organizational Unit.
- "Move" opens Move Organizational Unit.
- "Deactivate/Reactivate" opens the corresponding confirmation dialog.
- "Create Unit" (page-level action, not a row action) opens Create Organizational Unit.

> **Open Item — OQ-ORG-01:** Whether organizational units require a stable business code in addition to Name remains unresolved.
> **Open Item — OQ-ORG-03:** The currently defined Unit Types are Company, Division, Department, and Team. Whether this list is fixed or configurable remains unresolved.

**Data References:** Organizational Unit.

## 2. Create Organizational Unit

**Main Behavior**
- Allows HR Staff to create a new organizational unit (US-ORG-01).
- Captures:
  - Name (required)
  - Unit Type (required)
  - Parent Unit (optional)
  - Contact Email (optional)
  - Contact Phone (optional)
- The currently defined Unit Types are:
  - Company
  - Division
  - Department
  - Team
- Leaving Parent Unit empty creates a top-level organizational unit.
- Only active organizational units are available for selection as Parent Unit.
- Name must be unique among sibling units under the same parent.
- Contact Email and Contact Phone apply to the organizational unit itself, regardless of unit type; both stay optional whatever type is selected.
- If a Contact Email is provided, it must be a valid email format.
- If validation or a business rule fails, the unit is not created and the modal remains open with feedback.
- Successful creation closes the modal and returns to Organization Structure.

> **Open Item — OQ-ORG-01:** No separate business code field is represented while this remains unresolved.
> **Open Item — OQ-ORG-03:** Whether the Unit Type list is fixed or configurable remains unresolved.

**Data References:** Organizational Unit.

## 3. Update Organizational Unit

**Main Behavior**
- Allows HR Staff to update the organizational unit information defined by US-ORG-03.
- Editable fields:
  - Name (required)
  - Unit Type (required)
  - Contact Email (optional)
  - Contact Phone (optional)
- Parent Unit is not changed through this form; changing organizational placement is handled separately through Move Organizational Unit.
- Updated Name must remain unique among sibling units under the same parent.
- Contact Email and Contact Phone remain optional; either can be left empty.
- If a Contact Email is provided, it must be a valid email format.
- If validation or a business rule fails, the existing unit information remains unchanged and the modal stays open with feedback.
- Successful update closes the modal and returns to Organization Structure.

> **Open Item — OQ-ORG-03:** Whether the Unit Type list is fixed or configurable remains unresolved.

**Data References:** Organizational Unit.

## 4. Move Organizational Unit

**Main Behavior**
- Allows HR Staff to change the parent of an existing organizational unit (US-ORG-04).
- Allows selection of a new active Parent Unit.
- The unit cannot be moved under itself.
- The unit cannot be moved under one of its own descendants.
- An organizational unit that currently has a parent cannot be moved to the top level.
- After the move, the unit's name must remain unique among the units under its new parent.
- Moving a unit brings its existing subtree with it; descendants are not automatically detached or relocated.
- If the requested move violates a business rule, the organizational structure remains unchanged and the modal stays open with feedback.
- Successful move closes the modal and returns to Organization Structure.

**Data References:** Organizational Unit.

## 5. Deactivate/Reactivate Organizational Unit

**Main Behavior**
- Allows HR Staff to deactivate or reactivate an organizational unit according to its current status (US-ORG-05).

**Deactivate**
- Deactivation is blocked if the organizational unit has active child units.
- Deactivation is blocked if the organizational unit has active employees assigned.
- If the conditions are satisfied, HR Staff can confirm the deactivation.

**Reactivate**
- Reactivation is blocked if the organizational unit has a parent and that parent is inactive.
- If the conditions are satisfied, HR Staff can confirm the reactivation.
- If validation or a business rule fails, the organizational unit status remains unchanged and the dialog stays open with feedback.
- Successful confirmation closes the dialog and returns to Organization Structure.

**Data References:** Organizational Unit, Employee Profile.

## 6. Job Titles

**Main Behavior**
- Provides catalog access to existing job titles for maintenance actions.
- Job Titles form a single organization-wide catalog and are not owned by individual organizational units.
- Provides access to:
  - Create Job Title
  - Edit
  - Deactivate/Reactivate
- "Create Job Title" opens Create Job Title.
- "Edit" opens Update Job Title.
- "Deactivate/Reactivate" opens the corresponding confirmation dialog.
- Search, filtering, sorting, and pagination are not implied because those capabilities are not currently defined in the requirements.

> **Open Item — OQ-ORG-02:** Whether Job Titles require a stable business code in addition to Name remains unresolved.

**Data References:** Job Title.

## 7. Create Job Title

**Main Behavior**
- Allows HR Staff to create a Job Title in the organization-wide catalog (US-ORG-06).
- Captures:
  - Name
- Job Title Name must be unique across the whole organization-wide catalog.
- The Job Title is not assigned to or owned by a specific organizational unit.
- If validation or a business rule fails, the Job Title is not created and the modal remains open with feedback.
- Successful creation closes the modal and returns to Job Titles.

> **Open Item — OQ-ORG-02:** No separate business code field is represented while this remains unresolved.

**Data References:** Job Title.

## 8. Update Job Title

**Main Behavior**
- Allows HR Staff to update an existing Job Title (US-ORG-07).
- Editable field:
  - Name
- Updated Name must remain unique across the whole organization-wide catalog.
- If validation or a business rule fails, the existing Job Title remains unchanged and the modal stays open with feedback.
- Successful update closes the modal and returns to Job Titles.

**Data References:** Job Title.

## 9. Deactivate/Reactivate Job Title

**Main Behavior**
- Allows HR Staff to deactivate or reactivate an existing Job Title according to its current status (US-ORG-08).
- Deactivating a Job Title does not remove or change the title for employees who already hold it.
- A deactivated Job Title is unavailable for new employee assignments.
- Reactivating the Job Title makes it available for assignment again.
- Successful confirmation closes the dialog and returns to Job Titles.
- If the operation fails, the Job Title status remains unchanged and the dialog stays open with feedback.

**Data References:** Job Title, Employee Profile.
