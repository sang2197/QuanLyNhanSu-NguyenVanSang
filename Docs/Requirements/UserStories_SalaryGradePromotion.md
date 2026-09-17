# User Stories -- Salary Grade Promotion

## 1. Overview

The **Salary Grade Promotion** feature enables HR Staff and Approvers to
manage a salary grade promotion cycle from employee screening through
final salary decision.

The scope includes:

-   Creating and managing salary review periods.
-   Automatically determining eligible employees and their proposed
    salary grades.
-   Reviewing and recording outcomes for eligible employees.
-   Submitting completed review periods for approval.
-   Creating and managing salary decisions.
-   Applying salary decisions to make approved salary grade changes
    effective.
-   Viewing employee salary history.
-   Cancelling review periods and draft salary decisions under defined
    conditions.

The feature uses salary scales (**Ngạch lương**) and ordered salary
grades (**Bậc lương**) maintained in **Salary Master Data**. When
determining a proposed grade, inactive grades are skipped and the next
active grade in ascending order within the employee's current salary
scale is used.

This document describes business needs and expected behavior. It does
not prescribe screen design, API design, database structure, or other
implementation details.

------------------------------------------------------------------------

## 2. Review Period Lifecycle

A salary review period has the following business statuses:

-   **IN_PROGRESS** -- the review period has been created successfully,
    proposed grades have been calculated, and HR Staff can process
    eligible employees.
-   **SUBMITTED** -- every eligible employee with a proposed grade has
    an outcome and the period has been submitted to the Approver.
-   **CANCELLED** -- the review period has been cancelled and cannot be
    processed further.
-   **CLOSED** -- the salary decision associated with the period has
    been applied successfully and the review cycle is complete.

A successfully created review period enters **IN_PROGRESS** directly.
There is no user-visible Draft state for a review period.

Valid lifecycle transitions are:

-   Create successfully → **IN_PROGRESS**
-   IN_PROGRESS → **SUBMITTED**
-   IN_PROGRESS → **CANCELLED**
-   SUBMITTED → **CANCELLED**, only when no non-cancelled salary
    decision exists
-   SUBMITTED → **CLOSED**, when its salary decision is successfully
    applied

**CANCELLED** and **CLOSED** are terminal states.

------------------------------------------------------------------------

## 3. User Story Summary

| ID | User Story | Actor | Priority |
|---|---|---|---|
| US-SGP-01 | Create Salary Review Period | HR Staff | Must |
| US-SGP-02 | Search and Filter Review Periods | HR Staff | Should |
| US-SGP-03 | View Employees and Proposed Grades | HR Staff | Must |
| US-SGP-04 | Review Proposed Grades | HR Staff | Must |
| US-SGP-05 | Submit Review Period | HR Staff | Must |
| US-SGP-06 | Create Salary Decision | Approver | Must |
| US-SGP-07 | Apply Salary Decision | Approver | Must |
| US-SGP-08 | View Employee Salary History | HR Staff / Approver | Should |
| US-SGP-09 | View and Resume Salary Decisions | Approver | Must |
| US-SGP-10 | Cancel Draft Salary Decision | Approver | Should |
| US-SGP-11 | Cancel Review Period | HR Staff | Should |

------------------------------------------------------------------------

## 4. Salary Review Period

## US-SGP-01 -- Create Salary Review Period

### User Story

**As an** HR Staff,\
**I want to** create a salary review period,\
**so that** eligible employees can be reviewed for salary grade
promotion within a defined review cycle.

### Business Rules

-   A review period must have a unique code and name.
-   A review period has a review date and review type, such as annual,
    mid-year, or special.
-   Creating a review period automatically determines employee
    eligibility and proposed salary grades as part of the same business
    action.
-   A successfully created review period enters **IN_PROGRESS** status.
-   If the review period and its employee proposals cannot be created
    successfully as a complete operation, the review period must not
    become available for processing.

### Acceptance Criteria

#### AC01 -- Create a review period

**Given** I enter valid required information with a unique code and
name\
**When** I create the review period\
**Then** the review period is created successfully\
**And** its status is IN_PROGRESS\
**And** eligibility and proposed grades have already been determined for
the included employees.

#### AC02 -- Duplicate review period code

**Given** another review period already uses the entered code\
**When** I attempt to create the review period\
**Then** the request is rejected\
**And** no new review period is created.

#### AC03 -- Duplicate review period name

**Given** another review period already uses the entered name\
**When** I attempt to create the review period\
**Then** the request is rejected\
**And** no new review period is created.

#### AC04 -- Proposal calculation cannot complete

**Given** the review period information is valid\
**When** eligibility and proposed grades cannot be determined
successfully as part of creation\
**Then** the review period does not become available for processing as
an incomplete review period.

------------------------------------------------------------------------

## US-SGP-02 -- Search and Filter Review Periods

### User Story

**As an** HR Staff,\
**I want to** search and filter salary review periods,\
**so that** I can quickly find the review period I need to work with.

### Business Rules

-   Review periods can be filtered by review date range, review type,
    and status.
-   Search and filters may be used together.

### Acceptance Criteria

#### AC01 -- Filter review periods

**Given** multiple review periods exist\
**When** I filter them by review date range, review type, or status\
**Then** only review periods matching the selected criteria are
returned.

#### AC02 -- Combine filter criteria

**Given** multiple review periods exist\
**When** I apply more than one filter criterion\
**Then** only review periods satisfying all applied criteria are
returned.

#### AC03 -- No matching review periods

**Given** no review period matches the applied criteria\
**When** I perform the search\
**Then** no matching review periods are returned.

------------------------------------------------------------------------

## US-SGP-03 -- View Employees and Proposed Grades

### User Story

**As an** HR Staff,\
**I want to** view employees in a salary review period together with
their eligibility and proposed salary grade,\
**so that** I can understand who can be considered for salary grade
promotion and why.

### Business Rules

-   The system determines eligibility automatically for employees
    included in the review period.
-   An employee is eligible for a proposal only when all of the
    following are true:
    1.  The employee has held the current salary grade for at least **24
        months** as of the review date.
    2.  At least one higher **active** salary grade exists within the
        employee's current salary scale.
    3.  The employee has not already received another proposal in the
        same review period.
-   For an eligible employee, the proposed salary grade is the first
    active grade above the current grade in ascending grade order within
    the same salary scale.
-   Inactive salary grades are skipped when determining the proposed
    grade.
-   An employee who is not eligible has no proposed grade and must have
    the reason for ineligibility recorded.

### Acceptance Criteria

#### AC01 -- View eligible employee proposal

**Given** an employee satisfies all eligibility rules\
**When** I view the review period\
**Then** I can see the employee's current salary grade\
**And** the employee is shown as eligible\
**And** the proposed salary grade is the next active grade in ascending
order within the employee's current salary scale.

#### AC02 -- Current grade held for less than 24 months

**Given** an employee has held the current salary grade for less than 24
months as of the review date\
**When** eligibility is determined\
**Then** the employee is shown as not eligible\
**And** the 24-month requirement is identified as the reason\
**And** no proposed salary grade is assigned.

#### AC03 -- No higher active grade

**Given** no active salary grade exists above the employee's current
grade within the same salary scale\
**When** eligibility is determined\
**Then** the employee is shown as not eligible\
**And** the absence of a higher active grade is identified as the
reason\
**And** no proposed salary grade is assigned.

#### AC04 -- Skip inactive grades

**Given** one or more salary grades immediately above an employee's
current grade are inactive\
**And** a higher active salary grade exists in the same scale\
**When** the proposed salary grade is determined\
**Then** the inactive grades are skipped\
**And** the first higher active grade in ascending order is proposed.

#### AC05 -- Prevent duplicate proposal in the same period

**Given** an employee already has a proposal in the review period\
**When** proposals for that period are determined\
**Then** no additional proposal is created for that employee.

#### AC06 -- Filter employees in the review period

**Given** employees are included in a review period\
**When** I filter them by Organizational Unit, eligibility, or review
outcome\
**Then** only employees matching the applied criteria are returned.

------------------------------------------------------------------------

## US-SGP-04 -- Review Proposed Grades

### User Story

**As an** HR Staff,\
**I want to** approve or reject proposed salary grades for eligible
employees,\
**so that** each proposal is screened before the review period is
submitted to the Approver.

### Business Rules

-   Only an eligible employee with a proposed salary grade can receive
    an Approved or Rejected outcome.
-   A rejection requires a reason.
-   HR Staff may process proposals individually or in bulk.
-   In a bulk action, each selected proposal is validated individually.
-   An outcome can be changed only while the review period is
    **IN_PROGRESS**.
-   Once the review period is submitted, proposal outcomes cannot be
    changed by HR Staff.

### Acceptance Criteria

#### AC01 -- Approve a proposal

**Given** an eligible employee has a proposed salary grade\
**And** the review period is IN_PROGRESS\
**When** I approve the proposal\
**Then** the employee's review outcome is recorded as Approved.

#### AC02 -- Reject a proposal with a reason

**Given** an eligible employee has a proposed salary grade\
**And** the review period is IN_PROGRESS\
**When** I reject the proposal and provide a reason\
**Then** the employee's review outcome is recorded as Rejected\
**And** the rejection reason is retained.

#### AC03 -- Reject without a reason

**Given** an eligible employee has a proposed salary grade\
**When** I attempt to reject the proposal without a reason\
**Then** the request is rejected\
**And** the proposal outcome remains unchanged.

#### AC04 -- Process proposals in bulk

**Given** multiple employee proposals are selected\
**And** the review period is IN_PROGRESS\
**When** I apply the same review outcome to them in bulk\
**Then** each selected proposal is validated individually\
**And** valid proposals are updated\
**And** invalid proposals remain unchanged\
**And** the result identifies which proposals succeeded or failed and
why.

#### AC05 -- Attempt to change an outcome after submission

**Given** the review period is no longer IN_PROGRESS\
**When** I attempt to approve, reject, or change an employee's outcome\
**Then** the request is rejected\
**And** the existing outcome remains unchanged.

------------------------------------------------------------------------

## US-SGP-05 -- Submit Review Period

### User Story

**As an** HR Staff,\
**I want to** submit a completed salary review period to the Approver,\
**so that** approved promotion proposals can proceed to the salary
decision stage.

### Business Rules

-   A review period can be submitted only while it is **IN_PROGRESS**.
-   Every **eligible employee with a proposed salary grade** must have
    an Approved or Rejected outcome before submission.
-   Employees who are not eligible do not require a review outcome.
-   After successful submission, the review period status becomes
    **SUBMITTED**.
-   Proposal outcomes cannot be changed by HR Staff after submission.

### Acceptance Criteria

#### AC01 -- Submit a completed review period

**Given** the review period is IN_PROGRESS\
**And** every eligible employee with a proposed salary grade has an
outcome\
**When** I submit the review period and confirm the action\
**Then** the review period status becomes SUBMITTED\
**And** it becomes available for the Approver to process.

#### AC02 -- Eligible employee remains unprocessed

**Given** at least one eligible employee with a proposed salary grade
has no outcome\
**When** I attempt to submit the review period\
**Then** the submission is rejected\
**And** the review period remains IN_PROGRESS\
**And** the unprocessed proposals can be identified.

#### AC03 -- Not-eligible employees have no outcomes

**Given** all eligible employees with proposed salary grades have
outcomes\
**And** one or more not-eligible employees have no outcome\
**When** I submit the review period\
**Then** the review period can be submitted if all other business rules
are satisfied.

------------------------------------------------------------------------

## 5. Salary Decision

## US-SGP-06 -- Create Salary Decision

### User Story

**As an** Approver,\
**I want to** create a draft salary decision from a submitted review
period,\
**so that** approved salary grade changes can be prepared before they
take effect.

### Business Rules

-   A salary decision can be created only from a **SUBMITTED** review
    period.
-   Only employees with an **Approved** review outcome in that review
    period can be included.
-   Creating a draft salary decision does not change any employee's
    current salary grade.
-   A review period can have at most one non-cancelled salary decision
    at a time.
-   Employees to be included are selected when the draft decision is
    created.
-   An employee may be removed while the decision remains Draft.
-   Additional employees cannot be added to an existing draft. If a
    different employee set is required, the draft must be cancelled and
    a new decision created.
-   The decision's effective date must be on or after the review
    period's review date.

### Acceptance Criteria

#### AC01 -- Create a draft salary decision

**Given** a review period is SUBMITTED\
**And** it has no existing non-cancelled salary decision\
**When** I select approved employees and create a salary decision\
**Then** a Draft salary decision is created for that review period\
**And** only the selected approved employees are included\
**And** no employee's current salary grade is changed.

#### AC02 -- Attempt to include a non-approved employee

**Given** an employee in the review period does not have an Approved
outcome\
**When** I attempt to include that employee in the salary decision\
**Then** the employee cannot be included.

#### AC03 -- Existing non-cancelled decision

**Given** the review period already has a Draft or Applied salary
decision\
**When** I attempt to create another salary decision from the same
period\
**Then** the request is rejected.

#### AC04 -- Remove an employee from a draft

**Given** a salary decision is Draft\
**And** an employee is included in it\
**When** I remove that employee\
**Then** the employee is removed from the draft\
**And** the employee's current salary grade remains unchanged.

#### AC05 -- Attempt to add an employee after draft creation

**Given** a salary decision has already been created\
**When** I attempt to add another employee to the existing draft\
**Then** the request is rejected.

#### AC06 -- Effective date earlier than the review date

**Given** a review period is SUBMITTED with a given review date\
**When** I attempt to create a salary decision with an effective date
earlier than the review date\
**Then** the request is rejected\
**And** no salary decision is created.

------------------------------------------------------------------------

## US-SGP-07 -- Apply Salary Decision

### User Story

**As an** Approver,\
**I want to** apply a draft salary decision,\
**so that** the approved salary grade changes become effective and are
retained as part of each employee's salary history.

### Business Rules

-   Only a **Draft** salary decision can be applied.
-   A salary decision can be applied only once.
-   Each included employee's salary grade change has a defined effective
    date.
-   Applying a salary decision follows an **all-or-nothing** rule: all
    included employee changes must succeed together.
-   Before applying, each included employee's current salary grade must
    still match the current grade captured when the decision was
    created; if it no longer matches, that employee's change cannot be
    applied.
-   If any included employee's change cannot be applied, no employee
    salary grade in the decision is changed.
-   Applying the decision preserves each employee's prior salary
    information as history.
-   After successful application, the salary decision status becomes
    **APPLIED**.
-   The associated review period becomes **CLOSED** as part of the same
    successful business operation.
-   An Applied salary decision cannot be edited or cancelled.

### Acceptance Criteria

#### AC01 -- Apply a valid salary decision

**Given** a salary decision is Draft\
**And** all included employee salary grade changes are valid\
**When** I apply and confirm the salary decision\
**Then** every included employee is assigned the approved salary grade
from the decision's effective date\
**And** each employee's prior salary information is preserved as
history\
**And** the salary decision status becomes APPLIED\
**And** the associated review period status becomes CLOSED.

#### AC02 -- One employee change cannot be applied

**Given** a Draft salary decision contains multiple employees\
**And** at least one included employee's salary grade change cannot be
applied\
**When** I attempt to apply the decision\
**Then** no included employee's salary grade is changed\
**And** the salary decision remains Draft\
**And** the review period remains SUBMITTED\
**And** the reason the decision could not be applied can be identified.

#### AC03 -- Attempt to apply an already applied decision

**Given** a salary decision is APPLIED\
**When** I attempt to apply it again\
**Then** the request is rejected\
**And** no employee salary information is changed.

#### AC04 -- Employee's current grade no longer matches the snapshot

**Given** a Draft salary decision includes an employee\
**And** that employee's current salary grade has changed since the
decision was created, so it no longer matches the grade captured at
that time\
**When** I attempt to apply the decision\
**Then** no included employee's salary grade is changed\
**And** the salary decision remains Draft\
**And** the review period remains SUBMITTED\
**And** the employee whose grade no longer matches can be identified.

------------------------------------------------------------------------

## US-SGP-08 -- View Employee Salary History

### User Story

**As an** HR Staff or Approver,\
**I want to** view an employee's salary history,\
**so that** I can review how the employee's salary grade has changed
over time and trace changes to their source.

### Business Rules

-   Salary history is read-only within this capability.
-   Historical salary records are retained when a new salary grade
    becomes effective.
-   Where a salary change originated from a salary decision, that
    decision remains traceable from the historical record.

### Acceptance Criteria

#### AC01 -- View salary history

**Given** an employee has salary history\
**When** I view the employee's salary history\
**Then** I can see the employee's past and current salary grades with
their applicable effective periods\
**And** the records are presented from newest to oldest.

#### AC02 -- Trace a change to its salary decision

**Given** a historical salary change originated from a salary decision\
**When** I review that salary history record\
**Then** the related salary decision can be identified and viewed as
read-only.

------------------------------------------------------------------------

## US-SGP-09 -- View and Resume Salary Decisions

### User Story

**As an** Approver,\
**I want to** view salary decisions and continue working with a draft
decision,\
**so that** I can track salary decisions and complete unfinished work
without creating duplicates.

### Business Rules

-   Salary decisions can have the statuses **DRAFT**, **APPLIED**, or
    **CANCELLED**.
-   Draft decisions can be continued and modified only within the rules
    defined for a Draft decision.
-   Applied and Cancelled decisions are read-only.
-   A new salary decision can be created only for a SUBMITTED review
    period that has no non-cancelled salary decision.

### Acceptance Criteria

#### AC01 -- View salary decisions

**Given** salary decisions exist\
**When** I view salary decisions\
**Then** I can identify each decision's decision number, associated
review period, status, and effective date.

#### AC02 -- Resume a Draft decision

**Given** a salary decision has Draft status\
**When** I continue working with it\
**Then** its previously saved information and included employees are
retained\
**And** permitted Draft actions remain available.

#### AC03 -- View an Applied or Cancelled decision

**Given** a salary decision has Applied or Cancelled status\
**When** I view it\
**Then** its information is available as read-only.

#### AC04 -- Determine whether a new decision can be created

**Given** I want to create a salary decision\
**When** eligible review periods are determined\
**Then** only SUBMITTED review periods without a non-cancelled salary
decision are eligible.

------------------------------------------------------------------------

## US-SGP-10 -- Cancel Draft Salary Decision

### User Story

**As an** Approver,\
**I want to** cancel a draft salary decision that should no longer
proceed,\
**so that** it is formally discontinued without deleting its record or
changing employee salary information.

### Business Rules

-   Only a **Draft** salary decision can be cancelled.
-   Cancelling a Draft decision changes its status to **CANCELLED**.
-   Cancelling a Draft decision does not change employee salary
    information.
-   A Cancelled salary decision cannot be edited, applied, or
    reactivated.
-   An **Applied** salary decision cannot be cancelled.
-   After a Draft decision is cancelled, its SUBMITTED review period may
    be used to create a new salary decision because no non-cancelled
    decision remains.

### Acceptance Criteria

#### AC01 -- Cancel a Draft salary decision

**Given** a salary decision is Draft\
**When** I cancel it\
**Then** its status becomes CANCELLED\
**And** employee salary information remains unchanged\
**And** the decision can no longer be edited or applied.

#### AC02 -- Attempt to cancel an Applied decision

**Given** a salary decision is APPLIED\
**When** I attempt to cancel it\
**Then** the request is rejected\
**And** the decision remains APPLIED\
**And** employee salary information remains unchanged.

#### AC03 -- Attempt to cancel an already Cancelled decision

**Given** a salary decision is CANCELLED\
**When** I attempt to cancel it again\
**Then** the request is rejected\
**And** the decision remains CANCELLED.

#### AC04 -- Create a replacement decision after cancellation

**Given** a Draft salary decision for a SUBMITTED review period has been
cancelled\
**When** a new salary decision is created for that review period\
**Then** the new decision can be created if all other business rules are
satisfied.

------------------------------------------------------------------------

## US-SGP-11 -- Cancel Review Period

### User Story

**As an** HR Staff,\
**I want to** cancel a salary review period that should no longer
proceed,\
**so that** the review cycle is formally discontinued without deleting
its record.

### Business Rules

-   A review period can be cancelled while it is **IN_PROGRESS**.
-   A **SUBMITTED** review period can be cancelled only when it has no
    non-cancelled salary decision.
-   A **CLOSED** or already **CANCELLED** review period cannot be
    cancelled.
-   Cancelling a review period changes its status to **CANCELLED**.
-   Cancelling a review period does not delete employee eligibility,
    proposals, or review outcomes already recorded for the period.
-   A Cancelled review period cannot be submitted, modified, reopened,
    or used to create a salary decision.

### Acceptance Criteria

#### AC01 -- Cancel an IN_PROGRESS review period

**Given** a review period is IN_PROGRESS\
**When** I cancel it\
**Then** its status becomes CANCELLED\
**And** its existing review information is retained\
**And** it cannot be processed further.

#### AC02 -- Cancel a SUBMITTED period without a non-cancelled decision

**Given** a review period is SUBMITTED\
**And** it has no non-cancelled salary decision\
**When** I cancel the review period\
**Then** its status becomes CANCELLED\
**And** its existing review information is retained.

#### AC03 -- Submitted period has a non-cancelled decision

**Given** a SUBMITTED review period has a Draft or Applied salary
decision\
**When** I attempt to cancel the review period\
**Then** the request is rejected\
**And** the review period remains SUBMITTED.

#### AC04 -- Attempt to cancel a CLOSED period

**Given** a review period is CLOSED\
**When** I attempt to cancel it\
**Then** the request is rejected\
**And** the review period remains CLOSED.

#### AC05 -- Attempt to cancel an already CANCELLED period

**Given** a review period is CANCELLED\
**When** I attempt to cancel it again\
**Then** the request is rejected\
**And** the review period remains CANCELLED.

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

-   **Employee Profile** -- provides employee information and the
    employee's current Organizational Unit.
-   **Organization Management** -- provides Organizational Units used
    for employee filtering and organizational context.
-   **Salary Master Data** -- provides the base salary rate, salary
    scales, ordered salary grades, coefficients, and active/inactive
    grade status used by this feature.
