# User Stories -- Organization Management

## 1. Overview

The **Organization Management** module enables HR Staff to maintain the
organization's current structure and the job titles used across the
organization.

The scope includes:

-   Managing organizational units in a hierarchical tree.
-   Distinguishing organizational units by unit type.
-   Creating, viewing, updating, moving, deactivating, and reactivating
    organizational units.
-   Creating, updating, deactivating, and reactivating job titles.
-   Providing active organizational units and job titles for other HRM
    modules, including **Employee Profile**, to reference.

An **Organizational Unit** represents an element in the organization
structure, such as a company, division, department, or team. Each unit
may have a parent unit, except a top-level unit.

This document describes business needs and expected behavior. It does
not prescribe screen design, API design, database structure, or other
implementation details.

------------------------------------------------------------------------

## 2. User Story Summary

| ID | User Story | Actor | Priority |
|---|---|---|---|
| US-ORG-01 | Create Organizational Unit | HR Staff | Must |
| US-ORG-02 | View Organization Structure | HR Staff | Should |
| US-ORG-03 | Update Organizational Unit | HR Staff | Should |
| US-ORG-04 | Move Organizational Unit | HR Staff | Should |
| US-ORG-05 | Deactivate or Reactivate Organizational Unit | HR Staff | Should |
| US-ORG-06 | Create Job Title | HR Staff | Must |
| US-ORG-07 | Update Job Title | HR Staff | Should |
| US-ORG-08 | Deactivate or Reactivate Job Title | HR Staff | Should |

------------------------------------------------------------------------

## 3. Organizational Structure

## US-ORG-01 -- Create Organizational Unit

### User Story

**As an** HR Staff,\
**I want to** create an organizational unit,\
**so that** the organization's current structure can be represented and
maintained in the HRM system.

### Business Rules

-   Each organizational unit has a unit type, such as Company, Division,
    Department, or Team.
-   An organizational unit may have a parent unit.
-   A unit without a parent is a top-level unit.
-   An organizational unit name must be unique among units that have the
    same parent.
-   Only an active organizational unit can be selected as the parent of
    a new unit.

### Acceptance Criteria

#### AC01 -- Create a child organizational unit

**Given** I have entered the required organizational unit information\
**And** selected an active parent unit\
**When** I save the organizational unit\
**Then** the unit is created successfully\
**And** it appears under the selected parent in the organization
structure.

#### AC02 -- Create a top-level organizational unit

**Given** I have entered the required organizational unit information\
**And** no parent unit is selected\
**When** I save the organizational unit\
**Then** the unit is created successfully\
**And** it appears at the top level of the organization structure.

#### AC03 -- Duplicate name under the same parent

**Given** another organizational unit with the same name already exists
under the selected parent\
**When** I attempt to create the organizational unit\
**Then** the system rejects the request\
**And** informs me that the name is already in use under that parent\
**And** no organizational unit is created.

#### AC04 -- Same name under a different parent

**Given** an organizational unit with the same name exists under a
different parent\
**When** I create the organizational unit under the selected parent\
**Then** the organizational unit can be created if all other business
rules are satisfied.

#### AC05 -- Select an inactive parent

**Given** an organizational unit is inactive\
**When** I attempt to select it as the parent of a new organizational
unit\
**Then** the inactive unit is not available for selection.

------------------------------------------------------------------------

## US-ORG-02 -- View Organization Structure

### User Story

**As an** HR Staff,\
**I want to** view the organization structure as a hierarchy,\
**so that** I can understand the organization's current organizational
structure.

### Business Rules

-   The hierarchy includes organizational units even when no employees
    are currently assigned to them.
-   Both active and inactive organizational units remain visible in the
    hierarchy so that the existing structure and retained records can be
    understood.
-   Each organizational unit is shown according to its parent-child
    relationship.

### Acceptance Criteria

#### AC01 -- View hierarchical structure

**Given** organizational units exist\
**When** I view the organization structure\
**Then** the units are displayed according to their parent-child
relationships\
**And** top-level units are displayed at the root of the hierarchy.

#### AC02 -- View units without employees

**Given** an organizational unit has no employees assigned to it\
**When** I view the organization structure\
**Then** the organizational unit is still included in the hierarchy.

#### AC03 -- View inactive units

**Given** an organizational unit is inactive\
**When** I view the organization structure\
**Then** the organizational unit remains visible\
**And** its inactive status can be identified.

------------------------------------------------------------------------

## US-ORG-03 -- Update Organizational Unit

### User Story

**As an** HR Staff,\
**I want to** update an organizational unit's information,\
**so that** the HRM system reflects the organization's current structure
accurately.

### Business Rules

-   An organizational unit name must remain unique among units that have
    the same parent.
-   Updating an organizational unit must not create a new organizational
    unit.
-   Moving an organizational unit to a different parent is handled
    separately in **US-ORG-04**.

### Acceptance Criteria

#### AC01 -- Update valid organizational unit information

**Given** an organizational unit exists\
**When** I update its information with valid values\
**Then** the changes are saved successfully\
**And** the latest information is displayed.

#### AC02 -- Rename to a duplicate sibling name

**Given** another organizational unit with the same name exists under
the same parent\
**When** I attempt to rename the unit to that name\
**Then** the system rejects the change\
**And** the existing organizational unit information remains unchanged.

#### AC03 -- Rename to a name used under another parent

**Given** an organizational unit with the same name exists under a
different parent\
**When** I rename the current unit to that name\
**Then** the change can be saved if all other business rules are
satisfied.

------------------------------------------------------------------------

## US-ORG-04 -- Move Organizational Unit

### User Story

**As an** HR Staff,\
**I want to** move an organizational unit under a different parent,\
**so that** organizational restructuring can be reflected without
recreating the affected unit.

### Business Rules

-   An organizational unit cannot be moved under itself.
-   An organizational unit cannot be moved under one of its descendants.
-   An organizational unit can only be moved under an active parent
    unit.
-   After the move, the unit's name must remain unique among units under
    its new parent.
-   Moving an organizational unit does not automatically detach or
    relocate its descendants; its existing subtree moves with it.

### Acceptance Criteria

#### AC01 -- Move to a valid parent

**Given** an organizational unit and a valid active target parent exist\
**And** the move does not create a duplicate sibling name\
**When** I move the organizational unit under the target parent\
**Then** the organizational unit becomes a child of the target parent\
**And** its existing descendants remain under it.

#### AC02 -- Move a unit under itself

**Given** an organizational unit exists\
**When** I attempt to move it under itself\
**Then** the system rejects the move\
**And** the existing hierarchy remains unchanged.

#### AC03 -- Move a unit under its descendant

**Given** an organizational unit has one or more descendants\
**When** I attempt to move it under one of its descendants\
**Then** the system rejects the move\
**And** the existing hierarchy remains unchanged.

#### AC04 -- Move to an inactive parent

**Given** the target parent unit is inactive\
**When** I attempt to move an organizational unit under it\
**Then** the system rejects the move\
**And** the existing hierarchy remains unchanged.

#### AC05 -- Duplicate name under the new parent

**Given** the target parent already contains another unit with the same
name\
**When** I attempt to move the organizational unit under that parent\
**Then** the system rejects the move\
**And** the existing hierarchy remains unchanged.

------------------------------------------------------------------------

## US-ORG-05 -- Deactivate or Reactivate Organizational Unit

### User Story

**As an** HR Staff,\
**I want to** deactivate or reactivate an organizational unit,\
**so that** units no longer in use are unavailable for new assignments
without losing organizational records.

### Business Rules

-   Deactivating an organizational unit does not delete it.
-   An organizational unit cannot be deactivated while it has active
    child units.
-   An organizational unit cannot be deactivated while active employees
    are assigned to it.
-   A deactivated organizational unit is not available for new employee
    assignments.
-   A deactivated organizational unit cannot be selected as a parent for
    a newly created or moved organizational unit.
-   A deactivated organizational unit remains visible in the
    organization hierarchy.
-   A deactivated organizational unit can be reactivated.

### Acceptance Criteria

#### AC01 -- Deactivate an eligible organizational unit

**Given** an organizational unit is active\
**And** it has no active child units\
**And** no active employees are assigned to it\
**When** I deactivate the organizational unit\
**Then** its status becomes inactive\
**And** it remains visible in the organization structure\
**And** it is no longer available for new employee assignments.

#### AC02 -- Unit has active child units

**Given** an active organizational unit has one or more active child
units\
**When** I attempt to deactivate it\
**Then** the system rejects the request\
**And** informs me that active child units must be handled first\
**And** the organizational unit remains active.

#### AC03 -- Unit has active employees

**Given** one or more active employees are assigned to an organizational
unit\
**When** I attempt to deactivate the unit\
**Then** the system rejects the request\
**And** informs me that active employees must be reassigned or otherwise
handled first\
**And** the organizational unit remains active.

#### AC04 -- Reactivate an organizational unit

**Given** an organizational unit is inactive\
**When** I reactivate it\
**Then** its status becomes active\
**And** it becomes available for use again subject to the applicable
business rules.

------------------------------------------------------------------------

## 4. Job Title Management

## US-ORG-06 -- Create Job Title

### User Story

**As an** HR Staff,\
**I want to** create a job title,\
**so that** the organization can maintain a consistent set of job titles
for employee assignments.

### Business Rules

-   A job title name must be unique within the job title catalog.
-   Job titles are maintained as an organization-wide catalog and are
    not owned by a single organizational unit.
-   An active job title can be assigned to employees across
    organizational units.

### Acceptance Criteria

#### AC01 -- Create a valid job title

**Given** I enter a valid and unique job title name\
**When** I save the job title\
**Then** the job title is created successfully\
**And** it becomes available for employee assignment.

#### AC02 -- Duplicate job title name

**Given** another job title already uses the entered name\
**When** I attempt to create the job title\
**Then** the system rejects the request\
**And** informs me that the job title name already exists\
**And** no new job title is created.

------------------------------------------------------------------------

## US-ORG-07 -- Update Job Title

### User Story

**As an** HR Staff,\
**I want to** update a job title's information,\
**so that** the job title catalog remains accurate when organizational
naming changes.

### Business Rules

-   A job title name must remain unique within the job title catalog.
-   Updating a job title does not create a new job title.
-   Deactivation and reactivation are handled separately in
    **US-ORG-08**.

### Acceptance Criteria

#### AC01 -- Update a job title

**Given** a job title exists\
**When** I update it with valid information\
**Then** the changes are saved successfully\
**And** the latest information is displayed.

#### AC02 -- Rename to an existing job title name

**Given** another job title already uses the entered name\
**When** I attempt to save the change\
**Then** the system rejects the update\
**And** the existing job title information remains unchanged.

------------------------------------------------------------------------

## US-ORG-08 -- Deactivate or Reactivate Job Title

### User Story

**As an** HR Staff,\
**I want to** deactivate or reactivate a job title,\
**so that** obsolete titles are unavailable for new assignments without
losing existing employee information.

### Business Rules

-   Deactivating a job title does not delete it.
-   Deactivating a job title does not change employees who already hold
    that job title.
-   A deactivated job title is not available for new employee
    assignments.
-   A deactivated job title can be reactivated.

### Acceptance Criteria

#### AC01 -- Deactivate a job title

**Given** a job title is active\
**When** I deactivate it\
**Then** its status becomes inactive\
**And** it is no longer available for new employee assignments\
**And** employees who already hold the job title retain it.

#### AC02 -- Reactivate a job title

**Given** a job title is inactive\
**When** I reactivate it\
**Then** its status becomes active\
**And** it becomes available for employee assignment again.

------------------------------------------------------------------------

## 5. Open Questions

The following business decisions should be confirmed before the affected
requirements are considered final:

| ID | Question | Affected Stories |
|---|---|---|
| OQ-ORG-01 | Do organizational units require a stable unique business code in addition to their name? | US-ORG-01, US-ORG-03 |
| OQ-ORG-02 | Do job titles require a stable unique business code in addition to their name? | US-ORG-06, US-ORG-07 |
| OQ-ORG-03 | Are the proposed organizational unit types (Company, Division, Department, Team) sufficient, or must the organization support a configurable set of unit types? | US-ORG-01 |

------------------------------------------------------------------------

## 6. Definition of Ready

A User Story is considered ready for refinement and implementation when:

-   The actor, goal, and business value are clear.
-   Required business rules have been confirmed.
-   Acceptance Criteria describe observable and testable outcomes.
-   Required reference data or related modules are identified.
-   Open questions that affect implementation have been resolved.
-   The story is sufficiently small and clear for the development team
    to estimate.

------------------------------------------------------------------------

## 7. References

-   **Employee Profile** -- references active organizational units and
    active job titles when employee information is created or updated.
-   **Identity & Access Management (IAM)** -- manages authentication,
    accounts, roles, permissions, and access control separately from
    organization management.
-   **Salary Grade Promotion** -- may use employee organization
    information as part of the salary review process.
