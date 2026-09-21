# User Stories -- Contract Management

## 1. Overview

The **Contract Management** module enables HR Staff to record and track
employees' labor contracts (**Hợp đồng lao động**) in the HRM system.

The scope includes:

-   Creating labor contracts for employees.
-   Viewing the contract list, and searching and filtering contracts.
-   Viewing contract details.
-   Managing the contract lifecycle through its status.
-   Correcting or deleting Draft contracts.
-   Tracking contracts that are about to expire.

The contract types within the current scope are:

-   **Probation** (**Hợp đồng thử việc**) -- a fixed-term contract for
    the probation period.
-   **Fixed-Term** (**Hợp đồng xác định thời hạn**) -- a contract with a
    defined end date.
-   **Indefinite-Term** (**Hợp đồng không xác định thời hạn**) -- a
    contract with no end date.

### Contract Lifecycle

A contract has one of the following statuses:

-   **Draft** -- the contract has been recorded but is not yet in
    effect in the system.
-   **Active** -- the contract is currently in effect.
-   **Expired** -- the contract's end date has passed and HR Staff
    have marked it as expired.
-   **Terminated** -- the contract was ended before its normal expiry.

The allowed status changes are:

-   Draft → Active.
-   Active → Expired.
-   Active → Terminated.

**Expired** and **Terminated** are final statuses.

Contract status transitions are performed explicitly by HR Staff in the
current scope. The system does not automatically change a contract from
Draft to Active on its start date or from Active to Expired on its end
date.

The end date is the last day of a contract's term. A contract's end date
has passed when it is earlier than today (end date < today); on the end
date itself the contract is still within its term. A contract whose end
date has passed may therefore temporarily remain Active until HR Staff
marks it as Expired. Such a contract is considered **overdue**. This is
the only definition of overdue and of "end date has passed"; the
stories below refer to it.

A contract that is renewed or replaced is recorded as a new contract.
The previous contract is retained as history.

### Scope Boundaries

Employees are maintained by the **Employee Profile** module and are
referenced by contracts. The organizational unit and job title shown
with a contract come from the employee profile (see
[`UserStories_EmployeeProfile.md`](UserStories_EmployeeProfile.md)).

The salary recorded on a contract is information stated in the contract.
It is not linked to the salary scales, salary grades, or salary grade
promotion process in the current scope (see DQ-CON-02).

The following are outside the scope of this module:

-   Notifications, reminders, or alerts about expiring contracts. HR
    Staff consult the list of contracts expiring soon (US-CON-05)
    instead.
-   Editing or deleting a contract once it has become Active, Expired,
    or Terminated. Its contractual information is retained; only the
    status changes in US-CON-04 apply. (Draft contracts can be
    corrected or deleted -- see US-CON-06.)
-   Contract amendments and addenda (**Phụ lục hợp đồng**).
-   Contract documents, attachments, and electronic signatures.
-   Importing historical contracts.
-   Automatic contract status transitions based on start or end dates.
-   Authentication, user accounts, roles, permissions, and access
    control, which are specified separately under **Identity & Access
    Management (IAM)**.

This document describes business needs and expected behavior. It does
not prescribe screen design, API design, database structure, or other
implementation details.

------------------------------------------------------------------------

## 2. User Story Summary

| ID | User Story | Actor | Priority |
|---|---|---|---|
| US-CON-01 | Create Contract | HR Staff | Must |
| US-CON-02 | Search and Filter Contracts | HR Staff | Must |
| US-CON-03 | View Contract Details | HR Staff | Must |
| US-CON-04 | Update Contract Status | HR Staff | Must |
| US-CON-05 | Track Contracts Expiring Soon | HR Staff | Should |
| US-CON-06 | Update or Delete Draft Contract | HR Staff | Must |

------------------------------------------------------------------------

## US-CON-01 -- Create Contract

### User Story

**As an** HR Staff,\
**I want to** create a labor contract for an employee,\
**so that** the employee's contractual terms are recorded and can be
tracked in the HRM system.

### Business Rules

-   Each contract must have a unique contract number.
-   The following information is required:
    -   Employee.
    -   Contract type.
    -   Contract number.
    -   Start date.
    -   Contract salary amount.
-   The following information is optional:
    -   End date, subject to the contract type rule below.
    -   Salary note -- free-text information related to the salary, such
        as allowances stated in the contract.
-   The contract type is one of Probation, Fixed-Term, or
    Indefinite-Term.
-   Probation and Fixed-Term contracts require an end date.
    Indefinite-Term contracts must not have an end date.
-   The end date must be later than the start date.
-   The contract salary amount must be greater than zero.
-   A contract can only be created for an existing employee whose
    employment status is not Terminated.
-   A contract is created as **Draft** or **Active**.
-   A contract can be created directly as Active only when:
    -   Its start date is today or earlier.
    -   The employee has no other Active contract.
-   An employee can have at most one Active contract at a time. An
    employee can have any number of contracts in other statuses.
-   A Draft contract can be corrected or deleted before activation
    (US-CON-06). Once a contract becomes Active, its contractual
    information cannot be edited or deleted; subsequent lifecycle
    changes are handled through US-CON-04.

### Acceptance Criteria

#### AC01 -- Create a valid fixed-term or probation contract

**Given** I have selected an employee who is not Terminated\
**And** I have entered a unique contract number, a contract type of
Probation or Fixed-Term, a start date, an end date later than the start
date, and a contract salary amount greater than zero\
**When** I save the contract as Draft\
**Then** the contract is created successfully with status Draft.

#### AC02 -- Create a valid indefinite-term contract

**Given** I have selected an employee who is not Terminated\
**And** I have entered a unique contract number, a contract type of
Indefinite-Term without an end date, a start date, and a contract salary
amount greater than zero\
**When** I save the contract as Draft\
**Then** the contract is created successfully with status Draft.

#### AC03 -- Create a contract directly as Active

**Given** I have entered valid contract information\
**And** the start date is today or earlier\
**And** the employee has no other Active contract\
**When** I save the contract as Active\
**Then** the contract is created successfully with status Active.

#### AC04 -- Duplicate contract number

**Given** another contract already uses the entered contract number\
**When** I attempt to create the contract\
**Then** the system rejects the request\
**And** informs me that the contract number already exists\
**And** no new contract is created.

#### AC05 -- Missing required information

**Given** one or more required fields have not been provided\
**When** I attempt to create the contract\
**Then** the system does not create the contract\
**And** identifies the required information that is missing.

#### AC06 -- End date does not match the contract type

**Given** the contract type is Probation or Fixed-Term and no end date
has been entered\
**Or** the contract type is Indefinite-Term and an end date has been
entered\
**When** I attempt to create the contract\
**Then** the system rejects the request\
**And** informs me that the end date does not match the contract type\
**And** no new contract is created.

#### AC07 -- End date not later than the start date

**Given** the end date is the same as or earlier than the start date\
**When** I attempt to create the contract\
**Then** the system rejects the request\
**And** no new contract is created.

#### AC08 -- Non-positive contract salary

**Given** the contract salary amount is zero or negative\
**When** I attempt to create the contract\
**Then** the system rejects the request\
**And** no new contract is created.

#### AC09 -- Terminated employee

**Given** the selected employee's employment status is Terminated\
**When** I attempt to create a contract for the employee\
**Then** the system rejects the request\
**And** no new contract is created.

#### AC10 -- Contract cannot be created directly as Active

**Given** the start date is later than today\
**Or** the employee already has another Active contract\
**When** I attempt to save the contract as Active\
**Then** the system rejects the request\
**And** no new contract is created.

------------------------------------------------------------------------

## US-CON-02 -- Search and Filter Contracts

### User Story

**As an** HR Staff,\
**I want to** search and filter contracts,\
**so that** I can quickly find the contract or group of contracts I need
to work with.

### Business Rules

-   The contract list is the primary starting point for HR Staff's
    contract work.
-   Contracts can be searched by:
    -   Employee (employee code or full name).
    -   Contract number.
-   Contracts can be filtered by:
    -   Contract type.
    -   Contract status.
    -   Time period.
-   A time period filter matches contracts whose term overlaps the
    selected period. A contract without an end date is treated as
    running from its start date without limit.
-   Search and filter criteria can be used together.
-   Contracts of every status are included unless a status filter is
    applied.

### Acceptance Criteria

#### AC01 -- Search by employee

**Given** contracts exist\
**When** I search using an employee code or employee name\
**Then** the system displays the contracts of the matching employees\
**And** every contract of those employees is included regardless of
status.

#### AC02 -- Search by contract number

**Given** contracts exist\
**When** I search using a contract number\
**Then** the system displays the contracts matching the entered contract
number.

#### AC03 -- Filter by contract type or status

**Given** contracts exist\
**When** I filter by contract type or contract status\
**Then** only contracts matching the selected criteria are displayed.

#### AC04 -- Filter by time period

**Given** contracts exist\
**When** I select a time period\
**Then** only contracts whose term overlaps the selected period are
displayed\
**And** a contract without an end date is displayed when its start date
is on or before the end of the selected period.

#### AC05 -- Combine search and filters

**Given** I have entered a search term\
**And** selected one or more filters\
**When** the criteria are applied\
**Then** only contracts matching the applicable search and filter
criteria are displayed.

#### AC06 -- No matching contracts

**Given** no contract matches the current search and filter criteria\
**When** the results are displayed\
**Then** the system indicates that no matching contracts were found.

------------------------------------------------------------------------

## US-CON-03 -- View Contract Details

### User Story

**As an** HR Staff,\
**I want to** view the details of a contract,\
**so that** I can review the employee, contract terms, salary, and status
in one place.

### Business Rules

-   Employee information shown with a contract is the employee's
    currently recorded information, not a copy taken when the contract
    was created.

### Acceptance Criteria

#### AC01 -- View an existing contract

**Given** a contract exists\
**When** I open the contract\
**Then** the system displays the contract number, contract type, and
status\
**And** the start date and the end date, if any\
**And** the contract salary amount and salary note, if any\
**And** the employee's code, full name, organizational unit, job title,
and employment status.

#### AC02 -- View a terminated contract

**Given** a contract has the status Terminated\
**When** I open the contract\
**Then** the system also displays the termination date and the
termination reason.

------------------------------------------------------------------------

## US-CON-04 -- Update Contract Status

### User Story

**As an** HR Staff,\
**I want to** update a contract's status along its lifecycle,\
**so that** the contract record reflects the actual state of the
employee's contract.

### Business Rules

-   Contract status is updated explicitly by HR Staff. Status
    transitions are not performed automatically based on dates.
-   The allowed status changes are:
    -   Draft → Active.
    -   Active → Expired.
    -   Active → Terminated.
-   No other status transition is allowed.
-   Expired and Terminated are final statuses.
-   A Draft contract can be activated only when:
    -   Its start date is today or earlier.
    -   The employee has no other Active contract.
    -   The employee's employment status is not Terminated.
-   An Active contract can be marked as Expired only when:
    -   It has an end date.
    -   Its end date is earlier than today (end date < today).
-   An Active contract can be terminated before its normal expiry. The
    following information is required:
    -   Termination date.
    -   Termination reason.
-   The termination date must not be earlier than the contract's start
    date and, if the contract has an end date, must not be later than
    its end date.
-   Changing a contract's status must not delete the contract or its
    existing contractual information.
-   Changing a contract's status does not automatically change the
    employee's employment status.
-   Likewise, changing the employee's employment status does not
    automatically change an existing contract's status. Contract
    lifecycle management remains an explicit HR Staff action in the
    current scope.

### Acceptance Criteria

#### AC01 -- Activate a Draft contract

**Given** a contract is Draft\
**And** its start date is today or earlier\
**And** the employee has no other Active contract\
**And** the employee's employment status is not Terminated\
**When** I activate the contract\
**Then** the contract status is updated to Active.

#### AC02 -- Activate before the start date

**Given** a contract is Draft\
**And** its start date is later than today\
**When** I attempt to activate the contract\
**Then** the system rejects the request\
**And** the contract remains Draft.

#### AC03 -- Activate when another Active contract exists

**Given** a contract is Draft\
**And** the employee already has another Active contract\
**When** I attempt to activate the contract\
**Then** the system rejects the request\
**And** the contract remains Draft.

#### AC04 -- Activate for a Terminated employee

**Given** a contract is Draft\
**And** the employee's employment status is Terminated\
**When** I attempt to activate the contract\
**Then** the system rejects the request\
**And** the contract remains Draft.

#### AC05 -- Mark an Active contract as Expired

**Given** a contract is Active\
**And** it has an end date that is earlier than today\
**When** I mark the contract as Expired\
**Then** the contract status is updated to Expired\
**And** the contract and its existing information are retained.

#### AC06 -- Expire a contract that has not reached its end date

**Given** a contract is Active\
**And** its end date is today or later, or it has no end date\
**When** I attempt to mark the contract as Expired\
**Then** the system rejects the request\
**And** the contract remains Active.

#### AC07 -- Terminate an Active contract

**Given** a contract is Active\
**And** I have entered a termination date within the contract's term\
**And** I have entered a termination reason\
**When** I terminate the contract\
**Then** the contract status is updated to Terminated\
**And** the termination date and reason are recorded\
**And** the contract and its existing information are retained.

#### AC08 -- Terminate without required information

**Given** a contract is Active\
**And** the termination date or termination reason has not been
provided\
**When** I attempt to terminate the contract\
**Then** the system rejects the request\
**And** the contract remains Active.

#### AC09 -- Termination date outside the contract term

**Given** a contract is Active\
**And** the termination date is earlier than the start date or later
than the end date, when an end date exists\
**When** I attempt to terminate the contract\
**Then** the system rejects the request\
**And** the contract remains Active.

#### AC10 -- Invalid status transition

**Given** a contract is Draft and I attempt to mark it as Expired or
Terminated\
**Or** a contract is Expired or Terminated and I attempt to change its
status\
**When** the status change is requested\
**Then** the system rejects the request\
**And** the contract status remains unchanged.

------------------------------------------------------------------------

## US-CON-05 -- Track Contracts Expiring Soon

### User Story

**As an** HR Staff,\
**I want to** see the Active contracts that are approaching or have
passed their end date,\
**so that** I can take the appropriate contract lifecycle action in
time.

### Business Rules

-   The expiring-soon view lists Active contracts whose end date falls
    from today up to and including the last day of the selected window.
-   The window is 30 days or 60 days from today. The default is 30 days.
-   Indefinite-Term contracts and contracts that are not Active are not
    listed as expiring soon.
-   Overdue contracts (see Contract Lifecycle) are included in the view
    and identified as overdue until HR Staff marks them as Expired.
-   Contracts are ordered by end date, earliest first.
-   This story does not include notifications or reminders.
-   The capability may be presented as a quick filter on the contract
    list or as a dashboard view. The presentation is not prescribed.

### Acceptance Criteria

#### AC01 -- View contracts expiring within 30 days

**Given** Active contracts exist with end dates within the next 30
days\
**When** I open the expiring-soon view with the 30-day window\
**Then** the system displays those contracts\
**And** shows each contract's end date.

#### AC02 -- View contracts expiring within 60 days

**Given** Active contracts exist with end dates within the next 60
days\
**When** I select the 60-day window\
**Then** the system displays those contracts.

#### AC03 -- Contracts that are not listed

**Given** an Indefinite-Term contract, a contract that is not Active,
and an Active contract with an end date beyond the selected window
exist\
**When** I open the expiring-soon view\
**Then** none of those contracts are displayed.

#### AC04 -- Overdue Active contracts remain visible

**Given** an Active contract has an end date that is earlier than today\
**When** I open the expiring-soon view\
**Then** the contract is displayed\
**And** it is identified as overdue\
**And** it no longer appears once its status is updated to Expired.

#### AC05 -- Order by end date

**Given** multiple contracts are displayed\
**When** the results are shown\
**Then** they are ordered by end date, earliest first.

#### AC06 -- No expiring contracts

**Given** no Active contract has an end date within the selected
window\
**And** no Active contract is overdue\
**When** I open the expiring-soon view\
**Then** the system indicates that no contracts are expiring soon.

------------------------------------------------------------------------

## US-CON-06 -- Update or Delete Draft Contract

### User Story

**As an** HR Staff,\
**I want to** correct or delete a Draft contract,\
**so that** a contract recorded by mistake can be fixed before it takes
effect.

### Business Rules

-   Only a Draft contract can be updated or deleted.
-   The following information can be updated: contract type, contract
    number, start date, end date, contract salary amount, and salary
    note.
-   The employee of a Draft contract cannot be changed. If the wrong
    employee was selected, the Draft contract is deleted and a new
    contract is created.
-   The validation rules defined in US-CON-01 apply to the updated
    information, except the rules about the employee, which apply only
    when a contract is created (the employee cannot change). The
    contract number must remain unique across contracts; keeping the
    contract's own number is allowed.
-   Updating a contract does not create a new contract and does not
    change its status. Status changes are handled through US-CON-04.
-   Deleting a Draft contract removes it. It no longer appears in the
    contract list or in search results, and its contract number can be
    used again.
-   Active, Expired, and Terminated contracts cannot be updated or
    deleted.

### Acceptance Criteria

#### AC01 -- Update a Draft contract

**Given** a contract is Draft\
**When** I update valid contract information and save the changes\
**Then** the contract is updated successfully\
**And** the latest information is displayed\
**And** the contract remains Draft.

#### AC02 -- Duplicate contract number

**Given** another contract already uses the entered contract number\
**When** I attempt to save the change\
**Then** the system rejects the update\
**And** informs me that the contract number already exists\
**And** the contract information remains unchanged.

#### AC03 -- Change the employee

**Given** a contract is Draft\
**When** I attempt to change the employee of the contract\
**Then** the system rejects the update\
**And** the contract remains assigned to its original employee.

#### AC04 -- Information that breaks a validation rule

**Given** the entered information violates a validation rule defined in
US-CON-01\
**When** I attempt to save the change\
**Then** the system rejects the update\
**And** the contract information remains unchanged.

#### AC05 -- Delete a Draft contract

**Given** a contract is Draft\
**When** I delete the contract\
**Then** the contract is removed\
**And** it no longer appears in the contract list\
**And** its contract number can be used for another contract.

#### AC06 -- Update or delete a contract that is not Draft

**Given** a contract is Active, Expired, or Terminated\
**When** I attempt to update or delete the contract\
**Then** the system rejects the request\
**And** the contract remains unchanged.

------------------------------------------------------------------------

## 3. Deferred Business Questions

The following questions do not block implementation of the current
Contract Management scope and are deferred to a future release:

| ID | Deferred Question | Current-Scope Decision |
|---|---|---|
| DQ-CON-01 | Are additional contract types such as seasonal, project-based, or service contracts required? | Only Probation, Fixed-Term, and Indefinite-Term are supported. |
| DQ-CON-02 | How should contract salary integrate with Salary Master Data and Salary Grade Promotion? | Contract salary is independent information recorded from the labor contract. No automatic synchronization is performed. |
| DQ-CON-03 | Should renewal or replacement contracts reference the previous contract? | Renewal/replacement is stored as a new contract without an explicit predecessor relationship. This relationship may be introduced later. |

The following implementation-affecting questions are considered
resolved for the current scope:

-   **Employee termination:** changing an employee's employment status
    to Terminated does not automatically change an existing contract's
    status.
-   **Automatic lifecycle transitions:** contract status transitions are
    manual actions performed by HR Staff. Dates act as business
    constraints but do not automatically change status.
-   **Incorrect Draft contracts:** Draft contracts may be corrected or
    deleted before activation (US-CON-06). Active, Expired, and
    Terminated contracts cannot be edited or deleted.

------------------------------------------------------------------------

## 4. Definition of Ready

A User Story is considered ready for refinement and implementation when:

-   The actor, goal, and business value are clear.
-   Required business rules have been confirmed.
-   Acceptance Criteria describe observable and testable outcomes.
-   Required reference data or related modules are identified.
-   Implementation-affecting business questions have been resolved.
-   Questions explicitly deferred from the current scope do not prevent
    implementation.
-   The story is sufficiently small and clear for the development team
    to estimate.

------------------------------------------------------------------------

## 5. References

-   **Employee Profile** -- source of employees and their employment
    status; a contract is always recorded for an employee.
-   **Organization Management** -- source of the organizational unit and
    job title displayed with a contract, through the employee profile.
-   **Salary Master Data** and **Salary Grade Promotion** -- related to
    the contract salary question in DQ-CON-02.
-   **Identity & Access Management (IAM)** -- specifies authentication,
    user accounts, roles, permissions, and access control separately
    from contract management.
