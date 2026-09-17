# User Stories -- Employee Profile

## 1. Overview

The **Employee Profile** module enables HR Staff to maintain core
employee information and current employment information in the HRM
system.

The scope includes:

-   Creating employee profiles.
-   Viewing the employee list.
-   Searching and filtering employees.
-   Viewing employee details.
-   Updating employee profiles.
-   Managing employment status.

Organizational units and job titles are maintained by the
**Organization Management** module and are referenced by employee
profiles. An employee is assigned to one active organizational unit,
regardless of its unit type (see [`UserStories_OrganizationManagement.md`](UserStories_OrganizationManagement.md)).

Authentication, user accounts, roles, permissions, and access control
are outside the scope of this module and are specified separately under
**Identity & Access Management (IAM)**.

This document describes business needs and expected behavior. It does
not prescribe screen design, API design, database structure, or other
implementation details.

------------------------------------------------------------------------

## 2. User Story Summary

| ID | User Story | Actor | Priority |
|---|---|---|---|
| US-EMP-01 | Create Employee Profile | HR Staff | Must |
| US-EMP-02 | Search and Filter Employees | HR Staff | Must |
| US-EMP-03 | View Employee Profile | HR Staff | Must |
| US-EMP-04 | Update Employee Profile | HR Staff | Must |
| US-EMP-05 | Change Employment Status | HR Staff | Should |

------------------------------------------------------------------------

## US-EMP-01 -- Create Employee Profile

### User Story

**As an** HR Staff,\
**I want to** create an employee profile,\
**so that** the employee's information can be centrally recorded and
managed in the HRM system.

### Business Rules

-   Each employee must have a unique employee code.
-   The following information is required:
    -   Employee code.
    -   Full name.
    -   Organizational unit.
    -   Job title.
    -   Join date.
    -   Employment status.
-   An employee is assigned to one active organizational unit,
    regardless of its unit type.
-   The selected job title must be active.

### Acceptance Criteria

#### AC01 -- Create a valid employee profile

**Given** I have entered all required employee information\
**And** the selected organizational unit and job title are active\
**When** I save the employee profile\
**Then** the employee profile is created successfully\
**And** the employee appears in the employee list.

#### AC02 -- Duplicate employee code

**Given** another employee already uses the entered employee code\
**When** I attempt to create the employee profile\
**Then** the system rejects the request\
**And** informs me that the employee code already exists\
**And** no new employee profile is created.

#### AC03 -- Missing required information

**Given** one or more required fields have not been provided\
**When** I attempt to create the employee profile\
**Then** the system does not create the profile\
**And** identifies the required information that is missing.

#### AC04 -- Inactive organizational unit or job title

**Given** the selected organizational unit or job title is inactive\
**When** I attempt to create the employee profile\
**Then** the system rejects the request\
**And** no employee profile is created.

------------------------------------------------------------------------

## US-EMP-02 -- Search and Filter Employees

### User Story

**As an** HR Staff,\
**I want to** search and filter employees,\
**so that** I can quickly find the employee or group of employees I need
to work with.

### Business Rules

-   Employees can be filtered by:
    -   Organizational unit.
    -   Job title.
    -   Employment status.
-   Search and filter criteria can be used together.

### Acceptance Criteria

#### AC01 -- Search by employee code

**Given** employee profiles exist\
**When** I search using an employee code\
**Then** the system displays employees matching the entered code.

#### AC02 -- Search by employee name

**Given** employee profiles exist\
**When** I search using an employee name\
**Then** the system displays employees matching the entered name.

#### AC03 -- Filter employees

**Given** employee profiles exist\
**When** I filter by organizational unit, job title, or employment status\
**Then** only employees matching the selected criteria are displayed.

#### AC04 -- Combine search and filters

**Given** I have entered a search term\
**And** selected one or more filters\
**When** the criteria are applied\
**Then** only employees matching the applicable search and filter
criteria are displayed.

#### AC05 -- No matching employees

**Given** no employee matches the current search and filter criteria\
**When** the results are displayed\
**Then** the system indicates that no matching employees were found.

------------------------------------------------------------------------

## US-EMP-03 -- View Employee Profile

### User Story

**As an** HR Staff,\
**I want to** view an employee's profile,\
**so that** I can review their recorded personal and current employment
information in one place.

### Acceptance Criteria

#### AC01 -- View an existing employee

**Given** an employee profile exists\
**When** I open the employee profile\
**Then** the system displays the employee's recorded profile
information\
**And** their current organizational unit\
**And** their current job title\
**And** their current employment status.

------------------------------------------------------------------------

## US-EMP-04 -- Update Employee Profile

### User Story

**As an** HR Staff,\
**I want to** update an employee's profile information,\
**so that** the information stored in the HRM system remains accurate
and up to date.

### Business Rules

-   The employee code must remain unique across employee profiles.
-   An employee is assigned to one active organizational unit,
    regardless of its unit type.
-   An employee can only be assigned to an active job title.
-   Updating an employee profile must not create a new employee profile.

### Acceptance Criteria

#### AC01 -- Update valid profile information

**Given** an employee profile exists\
**When** I update valid profile information and save the changes\
**Then** the employee profile is updated successfully\
**And** the latest information is displayed.

#### AC02 -- Duplicate employee code

**Given** another employee already uses the entered employee code\
**When** I attempt to save the change\
**Then** the system rejects the update\
**And** informs me that the employee code already exists.

#### AC03 -- Assign an inactive organizational unit or job title

**Given** the selected organizational unit or job title is inactive\
**When** I attempt to save the change\
**Then** the system rejects the update\
**And** the employee's existing organizational unit or job title remains
unchanged.

------------------------------------------------------------------------

## US-EMP-05 -- Change Employment Status

### User Story

**As an** HR Staff,\
**I want to** update an employee's employment status,\
**so that** the HRM system reflects the employee's current working
status.

### Business Rules

The employment statuses within the current scope are:

-   **Active** -- the employee is currently working.
-   **On Leave** -- the employee is temporarily not working.
-   **Terminated** -- the employee is no longer employed.

Changing employment status must not delete the employee profile or its
existing information.

### Acceptance Criteria

#### AC01 -- Place an active employee on leave

**Given** an employee is Active\
**When** I change the employment status to On Leave\
**Then** the employee's status is updated to On Leave\
**And** the new status is shown in the employee profile and employee
list.

#### AC02 -- Return an employee from leave

**Given** an employee is On Leave\
**When** I change the employment status to Active\
**Then** the employee's status is updated to Active.

#### AC03 -- Mark an employee as terminated

**Given** an employee is not currently Terminated\
**When** I change the employment status to Terminated\
**Then** the employee's status is updated to Terminated\
**And** the employee profile and existing information are retained.

### Open Question

-   **OQ-EMP-01:** Can a Terminated employee later return to Active
    status using the same employee profile, or must rehiring follow a
    separate process?

------------------------------------------------------------------------

## 3. Open Questions

| ID | Question | Affected Story |
|---|---|---|
| OQ-EMP-01 | Can a Terminated employee return to Active using the same profile, or is a separate rehiring process required? | US-EMP-05 |

------------------------------------------------------------------------

## 4. Definition of Ready

A User Story is considered ready for refinement and implementation when:

-   The actor, goal, and business value are clear.
-   Required business rules have been confirmed.
-   Acceptance Criteria describe observable and testable outcomes.
-   Required reference data or related modules are identified.
-   Open questions that affect implementation have been resolved.
-   The story is sufficiently small and clear for the development team
    to estimate.

------------------------------------------------------------------------

## 5. References

-   **Organization Management** -- source of Organizational Unit and Job
    Title master data.
-   **Identity & Access Management (IAM)** -- specifies authentication,
    user accounts, roles, permissions, and access control separately
    from employee profile management.
-   **Salary Grade Promotion** -- consumes employee information where
    required by the salary review process.
