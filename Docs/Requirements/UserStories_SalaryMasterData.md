# User Stories -- Salary Master Data

## 1. Overview

The **Salary Master Data** module enables HR Staff to maintain the
reference data used for employee salary assignments and the **Salary
Grade Promotion** process.

The scope includes:

-   Maintaining the organization-wide base salary rate and its effective
    history.
-   Creating and maintaining salary scales (**Ngạch lương**).
-   Creating and maintaining ordered salary grades (**Bậc lương**)
    within each salary scale.
-   Maintaining effective-dated coefficients for salary grades.
-   Deactivating and reactivating salary scales and salary grades while
    preserving historical references.

The salary structure follows this relationship:

-   One **Salary Scale (Ngạch)** contains multiple **Salary Grades
    (Bậc)**.
-   Salary grades within a scale have a defined order: Grade 1 → Grade 2
    → Grade 3 → ...
-   Each salary grade has a coefficient used together with the
    applicable base salary rate.

This document describes business needs and expected behavior. It does
not prescribe screen design, API design, database structure, or other
implementation details.

------------------------------------------------------------------------

## 2. User Story Summary

| ID | User Story | Actor | Priority |
|---|---|---|---|
| US-SAL-01 | Maintain Base Salary Rate | HR Staff | Should |
| US-SAL-02 | Create Salary Scale | HR Staff | Must |
| US-SAL-03 | Update Salary Scale | HR Staff | Should |
| US-SAL-04 | Create Salary Grade | HR Staff | Must |
| US-SAL-05 | Update Salary Grade Coefficient | HR Staff | Should |
| US-SAL-06 | Deactivate or Reactivate Salary Grade | HR Staff | Should |
| US-SAL-07 | Deactivate or Reactivate Salary Scale | HR Staff | Should |

------------------------------------------------------------------------

## 3. Base Salary Rate

## US-SAL-01 -- Maintain Base Salary Rate

### User Story

**As an** HR Staff,\
**I want to** maintain the organization's base salary rate with its
effective date,\
**so that** salary calculations use the correct rate for each period
while historical rates remain available.

### Business Rules

-   Only one base salary rate applies across the organization at any
    given date.
-   Each base salary rate has an effective date.
-   Historical base salary rates must be preserved and must not be
    overwritten by a newer rate.
-   A new base salary rate may take effect immediately or on a future
    date.
-   A new base salary rate must have an effective date later than the
    latest effective date already recorded.
-   A base salary rate cannot be inserted between previously recorded
    effective dates.
-   The base salary rate must be greater than zero.

### Acceptance Criteria

#### AC01 -- Add a new base salary rate

**Given** a current base salary rate exists\
**And** I enter a new rate greater than zero with an effective date
later than the latest recorded effective date\
**When** I save the new rate\
**Then** the new rate is recorded successfully\
**And** it applies from its effective date onward\
**And** the previous rate remains available as historical data.

#### AC02 -- Schedule a future base salary rate

**Given** a current base salary rate exists\
**And** I enter a valid new rate with a future effective date later than
the latest recorded effective date\
**When** I save it\
**Then** the future rate is recorded\
**And** the current rate continues to apply until the future rate's
effective date.

#### AC03 -- Reject insertion between existing effective dates

**Given** two or more base salary rates have already been recorded with
different effective dates\
**When** I attempt to add a new rate with an effective date earlier than
or equal to the latest recorded effective date\
**Then** the system rejects the request\
**And** the existing base salary rate history remains unchanged.

#### AC04 -- Retrieve the rate for a past date

**Given** base salary rates have changed over time\
**When** the base salary rate applicable to a past date is requested\
**Then** the rate effective on that date is returned.

#### AC05 -- Reject a non-positive base salary rate

**Given** I enter a base salary rate that is zero or negative\
**When** I attempt to save it\
**Then** the system rejects the request\
**And** no new rate is recorded.

------------------------------------------------------------------------

## 4. Salary Scale Management

## US-SAL-02 -- Create Salary Scale

### User Story

**As an** HR Staff,\
**I want to** create a salary scale,\
**so that** the organization can define a salary progression structure
containing ordered salary grades.

### Business Rules

-   Each salary scale has a unique, stable code.
-   A salary scale code cannot be reused by another salary scale.
-   A salary scale name must be unique.
-   A newly created salary scale is available for salary grade
    definition while it is active.

### Acceptance Criteria

#### AC01 -- Create a valid salary scale

**Given** I enter a unique salary scale code and name\
**When** I save the salary scale\
**Then** the salary scale is created successfully.

#### AC02 -- Duplicate salary scale code

**Given** another salary scale already uses the entered code\
**When** I attempt to create the salary scale\
**Then** the system rejects the request\
**And** no salary scale is created.

#### AC03 -- Duplicate salary scale name

**Given** another salary scale already uses the entered name\
**When** I attempt to create the salary scale\
**Then** the system rejects the request\
**And** no salary scale is created.

------------------------------------------------------------------------

## US-SAL-03 -- Update Salary Scale

### User Story

**As an** HR Staff,\
**I want to** update a salary scale's maintainable information,\
**so that** the salary structure remains accurate when its business
information changes.

### Business Rules

-   A salary scale code is a stable business identifier and cannot be
    changed after the scale is created.
-   A salary scale name must remain unique.
-   Updating a salary scale does not create a new salary scale.
-   Deactivation and reactivation are handled separately in
    **US-SAL-07**.

### Acceptance Criteria

#### AC01 -- Update salary scale information

**Given** a salary scale exists\
**When** I update its maintainable information with valid values\
**Then** the changes are saved successfully\
**And** the salary scale code remains unchanged.

#### AC02 -- Rename to an existing salary scale name

**Given** another salary scale already uses the entered name\
**When** I attempt to save the change\
**Then** the system rejects the update\
**And** the existing salary scale information remains unchanged.

#### AC03 -- Attempt to change salary scale code

**Given** a salary scale already exists\
**When** I attempt to change its code\
**Then** the change is not allowed\
**And** the original code remains unchanged.

------------------------------------------------------------------------

## 5. Salary Grade Management

## US-SAL-04 -- Create Salary Grade

### User Story

**As an** HR Staff,\
**I want to** create an ordered salary grade within a salary scale,\
**so that** employees on the scale can be assigned to and progress
through defined salary grades.

### Business Rules

-   A salary grade belongs to exactly one salary scale.
-   Each salary grade has a grade number that defines its order within
    the salary scale.
-   Grade numbers must be unique within the same salary scale.
-   Salary grades follow an ascending order such as Grade 1 → Grade 2 →
    Grade 3.
-   A salary grade coefficient must be greater than zero.
-   A new salary grade can only be created under an active salary scale.

### Acceptance Criteria

#### AC01 -- Create a valid salary grade

**Given** an active salary scale exists\
**And** I enter a grade number not already used in that scale\
**And** I enter a coefficient greater than zero\
**When** I save the salary grade\
**Then** the salary grade is created successfully within that salary
scale\
**And** its position in the scale is determined by its grade number.

#### AC02 -- Duplicate grade number within the same scale

**Given** a salary grade with the same grade number already exists in
the selected salary scale\
**When** I attempt to create another grade with that number\
**Then** the system rejects the request\
**And** no salary grade is created.

#### AC03 -- Same grade number in a different scale

**Given** a grade number is already used in another salary scale\
**When** I create a grade with that number in the selected salary scale\
**Then** the salary grade can be created if all other business rules are
satisfied.

#### AC04 -- Non-positive coefficient

**Given** I enter a coefficient that is zero or negative\
**When** I attempt to create the salary grade\
**Then** the system rejects the request\
**And** no salary grade is created.

#### AC05 -- Create grade under an inactive scale

**Given** a salary scale is inactive\
**When** I attempt to create a salary grade under that scale\
**Then** the system rejects the request\
**And** no salary grade is created.

------------------------------------------------------------------------

## US-SAL-05 -- Update Salary Grade Coefficient

### User Story

**As an** HR Staff,\
**I want to** update a salary grade's coefficient from a specified
effective date,\
**so that** coefficient policy changes can take effect without
overwriting historical values.

### Business Rules

-   Each coefficient value has an effective date.
-   Historical coefficient values must be preserved and must not be
    overwritten.
-   A new coefficient must be greater than zero.
-   A new coefficient may take effect immediately or on a future date.
-   A new coefficient must have an effective date later than the latest
    effective date already recorded for that salary grade.
-   A coefficient cannot be inserted between previously recorded
    effective dates for the salary grade.

### Acceptance Criteria

#### AC01 -- Add a new coefficient

**Given** a salary grade exists\
**And** I enter a coefficient greater than zero with an effective date
later than the latest coefficient effective date for that grade\
**When** I save the change\
**Then** the new coefficient is recorded\
**And** it applies from its effective date onward\
**And** the previous coefficient remains available as historical data.

#### AC02 -- Schedule a future coefficient

**Given** a salary grade exists\
**And** I enter a valid coefficient with a future effective date later
than the latest recorded effective date\
**When** I save it\
**Then** the future coefficient is recorded\
**And** the current coefficient continues to apply until the future
coefficient's effective date.

#### AC03 -- Reject insertion between coefficient effective dates

**Given** the salary grade already has coefficient values with different
effective dates\
**When** I attempt to add a coefficient with an effective date earlier
than or equal to the latest recorded effective date\
**Then** the system rejects the request\
**And** the existing coefficient history remains unchanged.

#### AC04 -- Retrieve historical coefficient

**Given** a salary grade's coefficient has changed over time\
**When** the coefficient applicable to a past date is requested\
**Then** the coefficient effective on that date is returned.

#### AC05 -- Reject a non-positive coefficient

**Given** I enter a coefficient that is zero or negative\
**When** I attempt to save it\
**Then** the system rejects the request\
**And** no new coefficient is recorded.

------------------------------------------------------------------------

## US-SAL-06 -- Deactivate or Reactivate Salary Grade

### User Story

**As an** HR Staff,\
**I want to** deactivate or reactivate a salary grade,\
**so that** grades no longer in use are excluded from new salary
assignments and promotion proposals without deleting historical data.

### Business Rules

-   Deactivating a salary grade does not delete it or remove its
    historical data.
-   A salary grade cannot be deactivated while an active employee is
    currently assigned to it.
-   A deactivated salary grade cannot be selected for new employee
    salary assignments.
-   A deactivated salary grade is skipped when determining the next
    grade for a salary promotion proposal.
-   When one or more consecutive grades are inactive, the next active
    grade in ascending grade order is used as the next grade.
-   A deactivated salary grade can be reactivated.

### Acceptance Criteria

#### AC01 -- Deactivate an eligible salary grade

**Given** a salary grade is active\
**And** no active employee is currently assigned to it\
**When** I deactivate the salary grade\
**Then** its status becomes inactive\
**And** it is no longer available for new salary assignments.

#### AC02 -- Reject deactivation when active employees are assigned

**Given** one or more active employees are currently assigned to a
salary grade\
**When** I attempt to deactivate the salary grade\
**Then** the system rejects the request\
**And** informs me that the active employee assignments must be handled
first\
**And** the salary grade remains active.

#### AC03 -- Skip an inactive next grade

**Given** an employee is on an active salary grade\
**And** the immediately following grade in the same scale is inactive\
**And** a later active grade exists\
**When** the next grade is determined for a salary promotion proposal\
**Then** the next active grade in ascending grade order is used.

#### AC04 -- Skip multiple consecutive inactive grades

**Given** multiple grades following the employee's current grade are
inactive\
**And** a later active grade exists in the same scale\
**When** the next grade is determined for a salary promotion proposal\
**Then** all inactive grades are skipped\
**And** the first active grade in ascending grade order is used.

#### AC05 -- Reactivate a salary grade

**Given** a salary grade is inactive\
**When** I reactivate it\
**Then** its status becomes active\
**And** it becomes available for use again subject to the applicable
business rules.

------------------------------------------------------------------------

## US-SAL-07 -- Deactivate or Reactivate Salary Scale

### User Story

**As an** HR Staff,\
**I want to** deactivate or reactivate a salary scale,\
**so that** a scale no longer in use can be excluded from new salary
assignments without deleting its structure or historical data.

### Business Rules

-   Deactivating a salary scale does not delete the scale, its salary
    grades, or historical data.
-   A salary scale cannot be deactivated while it contains any active
    salary grade.
-   A deactivated salary scale cannot be used for new salary
    assignments.
-   A deactivated salary scale can be reactivated.

### Acceptance Criteria

#### AC01 -- Deactivate an eligible salary scale

**Given** a salary scale is active\
**And** it contains no active salary grades\
**When** I deactivate the salary scale\
**Then** its status becomes inactive\
**And** it is no longer available for new salary assignments.

#### AC02 -- Reject deactivation when active grades exist

**Given** a salary scale contains one or more active salary grades\
**When** I attempt to deactivate the salary scale\
**Then** the system rejects the request\
**And** informs me that active salary grades must be handled first\
**And** the salary scale remains active.

#### AC03 -- Reactivate a salary scale

**Given** a salary scale is inactive\
**When** I reactivate it\
**Then** its status becomes active\
**And** it becomes available for use again subject to the applicable
business rules.

------------------------------------------------------------------------

## 6. Definition of Ready

A User Story is considered ready for refinement and implementation when:

-   The actor, goal, and business value are clear.
-   Required business rules have been confirmed.
-   Acceptance Criteria describe observable and testable outcomes.
-   Required reference data and related modules are identified.
-   Blocking business decisions have been resolved.
-   The story is sufficiently small and clear for the development team
    to estimate.

------------------------------------------------------------------------

## 7. References

-   **Employee Profile** -- uses the applicable salary scale and salary
    grade as part of an employee's current salary information where
    required by the agreed scope.
-   **Salary Grade Promotion** -- uses salary scale, ordered salary
    grades, active/inactive status, and applicable salary values when
    determining and recording salary promotion outcomes.
