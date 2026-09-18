# Sequence Diagrams - HRM System

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-18 · **Implementation Baseline Commit:** `77e5716`

UML sequence diagrams for the full HRM system's key business-rule flows — the ones with validation, guard conditions, branching, or a transaction, one per module. Simple unguarded CRUD (plain create/update/list/view with only "required field" validation — e.g. creating a Job Title or a Salary Scale, updating a Salary Scale's name, reactivating a Salary Scale, or opening/resuming a Salary Decision by id) is intentionally not diagrammed; its request/response shape is already fully specified in [`openapi.yaml`](../API/openapi.yaml) and its class/method is in [ClassDiagram.md](ClassDiagram.md).

Class, interface, and repository names match [ClassDiagram.md](ClassDiagram.md). Endpoints match [`openapi.yaml`](../API/openapi.yaml). A cross-domain call (per [ADR-03](../Arc42/09-architecture-decisions.md#adr-03-split-the-backend-by-business-domain)) always targets another domain's Service interface, never its Repository — see the Traceability note in [ClassDiagram.md](ClassDiagram.md#traceability).

**Notation**

- Guard conditions on `alt`/`else`/`opt`/`break`/`loop` fragments are written in UML's `[condition]` form.
- `break [condition] ... end` marks a validation failure that ends the interaction right there, used instead of nesting further `alt`/`else`. The messages that follow a chain of `break` blocks are the success path, reached only when none of them fired. `alt`/`else` is kept for a genuine branch between two valid, non-error outcomes that both continue (e.g. "Deactivate requested" vs. "Reactivate requested" were previously modeled that way — see the "split" note below).
- Activation bars (`+`/`-` on the triggering/replying arrow) mark Controller, Service, and Repository (and cross-domain Service) participants for as long as they are doing work for their caller. A `break` block's error reply does not close the Controller/Service's outer activation early — the bar closes once, at the diagram's actual final reply — matching how Mermaid renders activation continuously across `alt`/`break` fragments.
- A transaction (`BeginTransaction`/`CommitTransaction`/`RollbackTransaction`) is owned by the shared `HrmDbContext` (participant `UOW`, used only in the 2 flows below that write across more than one repository), never by a single Repository. Every repository in [ClassDiagram.md](ClassDiagram.md) already shares one injected `HrmDbContext` instance per request, so the context — not an arbitrarily chosen repository — is what's actually able to coordinate a transaction spanning more than one of them. In the implementation, Services reach it through the Application-owned `IUnitOfWork` abstraction (implemented in `HRM.Infrastructure` by a class wrapping `HrmDbContext`), so `HRM.Application` never references the DbContext directly.
- Diagrams that used to bundle independent scenarios behind a top-level `alt` (Deactivate vs. Reactivate; Approve vs. Reject; list/resume/create) are now split one scenario per diagram, each numbered separately.

---

## 1. Employee Management

### 1.1 Create Employee Profile (US-EMP-01)

`POST /employees`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as EmployeesController
    participant SVC as EmployeeService
    participant ORGSVC as IOrganizationalUnitService
    participant JOBSVC as IJobTitleService
    participant REPO as EmployeeRepository
    participant DB as HRM Database

    HR->>FE: Fill "Create Employee" form
    FE->>+API: POST /employees {employeeCode, fullName, organizationalUnitId, jobTitleId, joinDate, employmentStatus}
    API->>+SVC: CreateEmployee(request)
    SVC->>+REPO: FindByCode(employeeCode)
    REPO->>DB: SELECT HrEmployee WHERE EmployeeCode=...
    DB-->>REPO: row?
    REPO-->>-SVC: exists? yes/no
    break [employee code already exists]
        SVC-->>API: 409 Conflict — employee code already in use (BR-EMP-01)
        API-->>FE: 409 Conflict
    end
    SVC->>+ORGSVC: IsUnitActive(organizationalUnitId)
    ORGSVC-->>-SVC: active? yes/no
    break [organizational unit is inactive]
        SVC-->>API: 400 Bad Request — organizational unit is inactive (BR-EMP-04)
        API-->>FE: 400 Bad Request
    end
    SVC->>+JOBSVC: IsJobTitleActive(jobTitleId)
    JOBSVC-->>-SVC: active? yes/no
    break [job title is inactive]
        SVC-->>API: 400 Bad Request — job title is inactive (BR-EMP-05)
        API-->>FE: 400 Bad Request
    end
    SVC->>+REPO: Add(employee)
    REPO->>DB: INSERT HrEmployee
    DB-->>REPO: OK
    REPO-->>-SVC: OK
    SVC-->>-API: Employee
    API-->>-FE: 201 Created
    FE-->>HR: Employee appears in the Employee List
```

### 1.2 Change Employment Status (US-EMP-05)

`POST /employees/{employeeId}/employment-status`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as EmployeesController
    participant SVC as EmployeeService
    participant REPO as EmployeeRepository
    participant DB as HRM Database

    HR->>FE: Select a new employment status for the employee
    FE->>+API: POST /employees/{employeeId}/employment-status {employmentStatus}
    API->>+SVC: ChangeEmploymentStatus(employeeId, newStatus)
    SVC->>+REPO: FindById(employeeId)
    REPO->>DB: SELECT HrEmployee
    DB-->>REPO: row
    REPO-->>-SVC: employee
    break [current status is TERMINATED]
        SVC-->>API: 409 Conflict — transition from Terminated is unresolved (OQ-EMP-01)
        API-->>FE: 409 Conflict
    end
    SVC->>+REPO: Update(employee with new status)
    REPO->>DB: UPDATE HrEmployee SET EmploymentStatus=...
    DB-->>REPO: OK
    REPO-->>-SVC: OK
    SVC-->>-API: Employee
    API-->>-FE: 200 OK
    FE-->>HR: New status shown in the profile and Employee List — profile and history are retained (BR-EMP-11)
```

---

## 2. Organization Management

### 2.1 Create Organizational Unit (US-ORG-01)

`POST /organizational-units`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as OrganizationalUnitsController
    participant SVC as OrganizationalUnitService
    participant REPO as OrganizationalUnitRepository
    participant DB as HRM Database

    HR->>FE: Fill "Create Unit" form
    FE->>+API: POST /organizational-units {name, parentId?, unitType, contactEmail?, contactPhone?}
    API->>+SVC: CreateUnit(request)
    break [contactEmail is present and not a valid email format]
        SVC-->>API: 400 Bad Request — invalid email format (BR-ORG-22)
        API-->>FE: 400 Bad Request
    end
    opt [parentId is provided]
        SVC->>+REPO: FindById(parentId)
        REPO->>DB: SELECT HrOrganizationalUnit
        DB-->>REPO: parent
        REPO-->>-SVC: parent
    end
    break [parentId is provided and parent is inactive]
        SVC-->>API: 400 Bad Request — parent unit is inactive (BR-ORG-04)
        API-->>FE: 400 Bad Request
    end
    SVC->>+REPO: FindByNameUnderParent(parentId, name)
    REPO->>DB: SELECT HrOrganizationalUnit WHERE ParentId=... AND Name=...
    DB-->>REPO: row?
    REPO-->>-SVC: exists?
    break [name already used under that parent, or among top-level units]
        SVC-->>API: 409 Conflict — duplicate name (BR-ORG-03)
        API-->>FE: 409 Conflict
    end
    SVC->>+REPO: Add(unit)
    REPO->>DB: INSERT HrOrganizationalUnit
    DB-->>REPO: OK
    REPO-->>-SVC: OK
    SVC-->>-API: OrganizationalUnit
    API-->>-FE: 201 Created
    FE-->>HR: New unit appears in Organization Structure
```

### 2.2 Move Organizational Unit (US-ORG-04)

`POST /organizational-units/{unitId}/move`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as OrganizationalUnitsController
    participant SVC as OrganizationalUnitService
    participant REPO as OrganizationalUnitRepository
    participant DB as HRM Database

    HR->>FE: Select "Move" on a unit row, pick a target parent
    FE->>+API: POST /organizational-units/{unitId}/move {targetParentId}
    API->>+SVC: MoveUnit(unitId, targetParentId)
    SVC->>+REPO: GetDescendantIds(unitId)
    REPO->>DB: SELECT recursive descendants
    DB-->>REPO: descendantIds
    REPO-->>-SVC: descendantIds
    break [targetParentId is the unit itself or one of its descendants]
        SVC-->>API: 400 Bad Request — invalid target (BR-ORG-07)
        API-->>FE: 400 Bad Request
    end
    SVC->>+REPO: FindById(targetParentId)
    REPO->>DB: SELECT HrOrganizationalUnit
    DB-->>REPO: target
    REPO-->>-SVC: target
    break [target parent is inactive]
        SVC-->>API: 400 Bad Request — target parent is inactive (BR-ORG-04)
        API-->>FE: 400 Bad Request
    end
    SVC->>+REPO: FindByNameUnderParent(targetParentId, unit.Name)
    REPO->>DB: SELECT HrOrganizationalUnit WHERE ParentId=... AND Name=...
    DB-->>REPO: row?
    REPO-->>-SVC: exists?
    break [name already used under the target parent]
        SVC-->>API: 409 Conflict — duplicate name under target parent (BR-ORG-03)
        API-->>FE: 409 Conflict
    end
    SVC->>+REPO: Update(unit, ParentId=targetParentId)
    REPO->>DB: UPDATE HrOrganizationalUnit SET ParentId=...
    DB-->>REPO: OK
    REPO-->>-SVC: OK
    SVC-->>-API: OrganizationalUnit
    API-->>-FE: 200 OK
    Note over SVC,DB: The existing subtree moves with it automatically — descendants' ParentId values are untouched (BR-ORG-08)
    FE-->>HR: Unit now shown under its new parent
```

Moving a unit to the top level is rejected as a request-validation error (`targetParentId` is required on this endpoint) — `BR-ORG-09` — not diagrammed as a separate business-rule branch.

### 2.3 Deactivate Organizational Unit (US-ORG-05)

`POST /organizational-units/{unitId}/deactivate`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as OrganizationalUnitsController
    participant SVC as OrganizationalUnitService
    participant REPO as OrganizationalUnitRepository
    participant EMPSVC as IEmployeeService
    participant DB as HRM Database

    HR->>FE: Choose "Deactivate" on a unit row, confirm
    FE->>+API: POST /organizational-units/{unitId}/deactivate
    API->>+SVC: DeactivateUnit(unitId)
    SVC->>+REPO: CountActiveChildren(unitId)
    REPO->>DB: SELECT COUNT(*) HrOrganizationalUnit WHERE ParentId=... AND Status='ACTIVE'
    DB-->>REPO: count
    REPO-->>-SVC: count
    break [count > 0]
        SVC-->>API: 409 Conflict — unit has active child units (BR-ORG-10)
        API-->>FE: 409 Conflict
    end
    SVC->>+EMPSVC: HasActiveEmployeesInUnit(unitId)
    EMPSVC-->>-SVC: true/false
    break [has active employees assigned]
        SVC-->>API: 409 Conflict — active employees still assigned (BR-ORG-11)
        API-->>FE: 409 Conflict
    end
    SVC->>+REPO: Update(unit, Status=INACTIVE)
    REPO->>DB: UPDATE HrOrganizationalUnit SET Status='INACTIVE'
    DB-->>REPO: OK
    REPO-->>-SVC: OK
    SVC-->>-API: OrganizationalUnit
    API-->>-FE: 200 OK
    FE-->>HR: Unit's status updated in Organization Structure
```

`EMPSVC: IEmployeeService` is the cross-domain dependency named in [ClassDiagram.md §3](ClassDiagram.md#3-organization-management).

### 2.4 Reactivate Organizational Unit (US-ORG-05)

`POST /organizational-units/{unitId}/reactivate`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as OrganizationalUnitsController
    participant SVC as OrganizationalUnitService
    participant REPO as OrganizationalUnitRepository
    participant DB as HRM Database

    HR->>FE: Choose "Reactivate" on a unit row, confirm
    FE->>+API: POST /organizational-units/{unitId}/reactivate
    API->>+SVC: ReactivateUnit(unitId)
    SVC->>+REPO: FindById(unitId)
    REPO->>DB: SELECT HrOrganizationalUnit
    DB-->>REPO: unit
    REPO-->>-SVC: unit
    break [unit has a parent and that parent is inactive]
        SVC-->>API: 409 Conflict — parent unit must be reactivated first (BR-ORG-14)
        API-->>FE: 409 Conflict
    end
    SVC->>+REPO: Update(unit, Status=ACTIVE)
    REPO->>DB: UPDATE HrOrganizationalUnit SET Status='ACTIVE'
    DB-->>REPO: OK
    REPO-->>-SVC: OK
    SVC-->>-API: OrganizationalUnit
    API-->>-FE: 200 OK
    FE-->>HR: Unit's status updated in Organization Structure
```

---

## 3. Salary Master Data

### 3.1 Maintain Base Salary Rate (US-SAL-01)

`POST /base-salary-rates`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as BaseSalaryRatesController
    participant SVC as BaseSalaryRateService
    participant REPO as BaseSalaryRateRepository
    participant DB as HRM Database

    HR->>FE: Enter a new rate and its effective date
    FE->>+API: POST /base-salary-rates {rate, effectiveDate}
    API->>+SVC: AddRate(request)
    break [rate <= 0]
        SVC-->>API: 400 Bad Request — rate must be greater than zero (BR-SAL-04)
        API-->>FE: 400 Bad Request
    end
    SVC->>+REPO: GetLatest()
    REPO->>DB: SELECT HrBaseSalaryRate ORDER BY EffectiveDate DESC LIMIT 1
    DB-->>REPO: latest
    REPO-->>-SVC: latest
    break [effectiveDate <= latest.EffectiveDate]
        SVC-->>API: 409 Conflict — effective date must be later than the latest recorded date (BR-SAL-03)
        API-->>FE: 409 Conflict
    end
    SVC->>+REPO: Add(rate)
    REPO->>DB: INSERT HrBaseSalaryRate
    DB-->>REPO: OK
    REPO-->>-SVC: OK
    SVC-->>-API: BaseSalaryRate
    API-->>-FE: 201 Created
    FE-->>HR: New rate shown at the top of the history
```

### 3.2 Create Salary Grade (US-SAL-04)

`POST /salary-scales/{scaleId}/grades`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as SalaryScalesController
    participant SVC as SalaryScaleService
    participant REPO as SalaryScaleRepository
    participant GREPO as SalaryGradeRepository
    participant DB as HRM Database

    HR->>FE: Open Salary Scale Detail, "Add Grade"
    FE->>+API: POST /salary-scales/{scaleId}/grades {gradeNumber, coefficient}
    API->>+SVC: CreateGrade(scaleId, request)
    SVC->>+REPO: FindById(scaleId)
    REPO->>DB: SELECT HrSalaryScale
    DB-->>REPO: scale
    REPO-->>-SVC: scale
    break [salary scale is inactive]
        SVC-->>API: 409 Conflict — salary scale is inactive (BR-SAL-11)
        API-->>FE: 409 Conflict
    end
    break [coefficient <= 0]
        SVC-->>API: 400 Bad Request — coefficient must be greater than zero (BR-SAL-10)
        API-->>FE: 400 Bad Request
    end
    SVC->>+GREPO: FindByScaleAndNumber(scaleId, gradeNumber)
    GREPO->>DB: SELECT HrSalaryGrade WHERE SalaryScaleId=... AND GradeNumber=...
    DB-->>GREPO: row?
    GREPO-->>-SVC: exists?
    break [grade number already used in this scale]
        SVC-->>API: 409 Conflict — grade number already in use (BR-SAL-09)
        API-->>FE: 409 Conflict
    end
    SVC->>+GREPO: Add(grade), AddCoefficient(initial coefficient — no separate effective date, BR-SAL-12)
    GREPO->>DB: INSERT HrSalaryGrade, INSERT HrSalaryGradeCoefficient
    DB-->>GREPO: OK
    GREPO-->>-SVC: OK
    SVC-->>-API: SalaryGrade
    API-->>-FE: 201 Created
    FE-->>HR: New grade shown in Salary Scale Detail
```

### 3.3 Deactivate Salary Grade (US-SAL-06)

`POST /salary-grades/{gradeId}/deactivate`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as SalaryGradesController
    participant SVC as SalaryGradeService
    participant REPO as SalaryGradeRepository
    participant HISTSVC as ISalaryHistoryService
    participant DB as HRM Database

    HR->>FE: Choose "Deactivate" on a grade row, confirm
    FE->>+API: POST /salary-grades/{gradeId}/deactivate
    API->>+SVC: DeactivateGrade(gradeId)
    SVC->>+HISTSVC: HasActiveEmployeeOnGrade(gradeId)
    HISTSVC-->>-SVC: true/false
    break [an active employee is currently assigned]
        SVC-->>API: 409 Conflict — active employee assignment must be handled first (BR-SAL-15)
        API-->>FE: 409 Conflict
    end
    SVC->>+REPO: Update(grade, Status=INACTIVE)
    REPO->>DB: UPDATE HrSalaryGrade SET Status='INACTIVE'
    DB-->>REPO: OK
    REPO-->>-SVC: OK
    SVC-->>-API: SalaryGrade
    API-->>-FE: 200 OK
    FE-->>HR: Grade's status updated in Salary Scale Detail
```

`HISTSVC: ISalaryHistoryService` is the cross-domain dependency (to Salary Grade Promotion) named in [ClassDiagram.md §4](ClassDiagram.md#4-salary-master-data).

### 3.4 Reactivate Salary Grade (US-SAL-06)

`POST /salary-grades/{gradeId}/reactivate`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as SalaryGradesController
    participant SVC as SalaryGradeService
    participant REPO as SalaryGradeRepository
    participant SCALESVC as ISalaryScaleService
    participant DB as HRM Database

    HR->>FE: Choose "Reactivate" on a grade row, confirm
    FE->>+API: POST /salary-grades/{gradeId}/reactivate
    API->>+SVC: ReactivateGrade(gradeId)
    SVC->>+REPO: FindById(gradeId)
    REPO->>DB: SELECT HrSalaryGrade
    DB-->>REPO: grade
    REPO-->>-SVC: grade
    SVC->>+SCALESVC: IsScaleActive(grade.SalaryScaleId)
    SCALESVC-->>-SVC: active?
    break [salary scale is inactive]
        SVC-->>API: 409 Conflict — salary scale must be reactivated first (BR-SAL-18)
        API-->>FE: 409 Conflict
    end
    SVC->>+REPO: Update(grade, Status=ACTIVE)
    REPO->>DB: UPDATE HrSalaryGrade SET Status='ACTIVE'
    DB-->>REPO: OK
    REPO-->>-SVC: OK
    SVC-->>-API: SalaryGrade
    API-->>-FE: 200 OK
    FE-->>HR: Grade's status updated in Salary Scale Detail
```

`SCALESVC` is a same-domain call to the sibling `SalaryScaleService`, not a cross-domain dependency.

### 3.5 Deactivate Salary Scale (US-SAL-07)

`POST /salary-scales/{scaleId}/deactivate`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as SalaryScalesController
    participant SVC as SalaryScaleService
    participant REPO as SalaryScaleRepository
    participant GREPO as SalaryGradeRepository
    participant DB as HRM Database

    HR->>FE: Choose "Deactivate" on a scale row, confirm
    FE->>+API: POST /salary-scales/{scaleId}/deactivate
    API->>+SVC: DeactivateScale(scaleId)
    SVC->>+GREPO: CountActiveByScale(scaleId)
    GREPO->>DB: SELECT COUNT(*) HrSalaryGrade WHERE SalaryScaleId=... AND Status='ACTIVE'
    DB-->>GREPO: count
    GREPO-->>-SVC: count
    break [count > 0]
        SVC-->>API: 409 Conflict — scale still has active salary grades (BR-SAL-20)
        API-->>FE: 409 Conflict
    end
    SVC->>+REPO: Update(scale, Status=INACTIVE)
    REPO->>DB: UPDATE HrSalaryScale SET Status='INACTIVE'
    DB-->>REPO: OK
    REPO-->>-SVC: OK
    SVC-->>-API: SalaryScale
    API-->>-FE: 200 OK
    FE-->>HR: Scale's status updated in the Salary Scales list
```

Reactivating a Salary Scale (`POST /salary-scales/{scaleId}/reactivate`) has no guard at all (`BR-SAL-22`) — a plain status flip, not diagrammed.

---

## 4. Salary Grade Promotion

### 4.1 Create a Review Period — eligibility & proposed-grade calculation (US-SGP-01 / US-SGP-03)

`POST /review-periods` — the most business-rule-heavy flow in the system: it drives [Runtime View 6.1](../Arc42/06-runtime-view.md#61-happy-path--process-a-review-period-end-to-end) step 1.

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as ReviewPeriodsController
    participant SVC as ReviewPeriodService
    participant EMPSVC as IEmployeeService
    participant RULE as SalaryPromotionEligibilityRule
    participant GRADESVC as ISalaryGradeService
    participant PREPO as ReviewPeriodRepository
    participant EREPO as ReviewEmployeeRepository
    participant UOW as HrmDbContext
    participant DB as HRM Database

    HR->>FE: Fill "Create Review Period" form
    FE->>+API: POST /review-periods {code, name, reviewType, reviewDate, effectiveDate?, description?}
    API->>+SVC: CreateReviewPeriod(request)
    SVC->>+PREPO: FindByCode(code) / FindByName(name)
    PREPO->>DB: SELECT HrSalaryReviewPeriod WHERE Code=... OR Name=...
    DB-->>PREPO: row?
    PREPO-->>-SVC: duplicate? yes/no
    break [code or name already used]
        SVC-->>API: 409 Conflict — duplicate code or name (US-SGP-01 AC02, AC03)
        API-->>FE: 409 Conflict
    end
    SVC->>+UOW: BeginTransaction()
    UOW-->>-SVC: transaction
    SVC->>+PREPO: Add(period, Status=IN_PROGRESS)
    PREPO->>DB: INSERT HrSalaryReviewPeriod
    DB-->>PREPO: OK
    PREPO-->>-SVC: OK
    SVC->>+EMPSVC: GetActiveEmployees()
    EMPSVC-->>-SVC: employees[]
    loop [for each active employee]
        SVC->>+EREPO: GetCurrentSalaryGrade(employeeId)
        EREPO->>DB: SELECT latest HrEmployeeSalary
        DB-->>EREPO: currentGrade, effectiveDate
        EREPO-->>-SVC: currentGrade
        SVC->>+RULE: DetermineEligibility(employee, currentGrade, reviewDate)
        RULE-->>-SVC: eligible? — held current grade at least 24 months as of reviewDate (US-SGP-03 AC02)
        alt [eligible]
            SVC->>+GRADESVC: GetNextActiveGrade(scaleId, currentGrade.GradeNumber)
            GRADESVC-->>-SVC: proposedGrade or none — inactive grades skipped (BR-SAL-17)
            alt [no higher active grade exists]
                SVC->>EREPO: BufferSnapshotRow(employee, currentGrade, Eligible=false, IneligibleReason="No higher active grade") — US-SGP-03 AC03
            else [higher active grade found]
                SVC->>EREPO: BufferSnapshotRow(employee, currentGrade, Eligible=true, ProposedGrade, Outcome=PENDING) — US-SGP-03 AC01, AC04
            end
        else [not eligible — held current grade less than 24 months]
            SVC->>EREPO: BufferSnapshotRow(employee, currentGrade, Eligible=false, IneligibleReason="Held current grade less than 24 months") — US-SGP-03 AC02
        end
    end
    break [eligibility or proposed-grade calculation could not complete for every employee]
        SVC->>UOW: RollbackTransaction()
        SVC-->>API: 500 Internal Server Error — period not created (US-SGP-01 AC04)
        API-->>FE: 500 Internal Server Error
    end
    SVC->>+EREPO: AddRange(bufferedSnapshotRows)
    EREPO->>DB: INSERT HrSalaryReviewEmployee (one row per employee)
    DB-->>EREPO: OK
    EREPO-->>-SVC: OK
    SVC->>+UOW: CommitTransaction()
    UOW-->>-SVC: OK
    SVC-->>-API: ReviewPeriod (IN_PROGRESS)
    API-->>-FE: 201 Created
    FE-->>HR: Review period created — employees, eligibility, and proposed grades ready to review
```

`EMPSVC` (Employee Management) and `GRADESVC` (Salary Master Data) are the 2 cross-domain dependencies already named in the [C4 diagrams' Cross-component Data Dependencies](../c4/README.md#cross-component-data-dependencies) and in [ClassDiagram.md §5](ClassDiagram.md#5-salary-grade-promotion). `UOW: HrmDbContext` owns the transaction because it spans writes through both `PREPO` and `EREPO`.

### 4.2 Approve an employee's proposed grade (US-SGP-04)

`POST /review-periods/{periodId}/employees/{employeeId}/approve`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as ReviewPeriodEmployeesController
    participant SVC as ReviewEmployeeService
    participant PREPO as ReviewPeriodRepository
    participant EREPO as ReviewEmployeeRepository
    participant DB as HRM Database

    HR->>FE: Click "Approve" on an employee row
    FE->>+API: POST .../employees/{employeeId}/approve
    API->>+SVC: ApproveEmployee(periodId, employeeId)
    SVC->>+PREPO: GetStatus(periodId)
    PREPO->>DB: SELECT HrSalaryReviewPeriod
    DB-->>PREPO: status
    PREPO-->>-SVC: status
    break [period is not IN_PROGRESS]
        SVC-->>API: 409 Conflict — period no longer in progress (US-SGP-04 AC10)
        API-->>FE: 409 Conflict
    end
    SVC->>+EREPO: FindByPeriodAndEmployee(periodId, employeeId)
    EREPO->>DB: SELECT HrSalaryReviewEmployee
    DB-->>EREPO: row
    EREPO-->>-SVC: reviewEmployee
    break [not eligible — no proposed grade]
        SVC-->>API: 409 Conflict — employee has no proposal to review
        API-->>FE: 409 Conflict
    end
    SVC->>+EREPO: Update(Outcome=APPROVED, RejectionReason=null) — clears any prior rejection reason (US-SGP-04 AC08)
    EREPO->>DB: UPDATE HrSalaryReviewEmployee SET Outcome='APPROVED', RejectionReason=NULL
    DB-->>EREPO: OK
    EREPO-->>-SVC: OK
    SVC-->>-API: ReviewPeriodEmployee
    API-->>-FE: 200 OK
    FE-->>HR: Row shows "Approved"
```

Bulk Approve applies this same guard chain to each selected employee individually, with partial success (`US-SGP-04` AC04, AC05 — see `BulkActionResult` in [ClassDiagram.md](ClassDiagram.md#5-salary-grade-promotion)).

### 4.3 Reject an employee's proposed grade (US-SGP-04)

`POST /review-periods/{periodId}/employees/{employeeId}/reject`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as ReviewPeriodEmployeesController
    participant SVC as ReviewEmployeeService
    participant PREPO as ReviewPeriodRepository
    participant EREPO as ReviewEmployeeRepository
    participant DB as HRM Database

    HR->>FE: Click "Reject" on an employee row, enter a reason
    FE->>+API: POST .../employees/{employeeId}/reject {reason}
    API->>+SVC: RejectEmployee(periodId, employeeId, reason)
    break [reason is missing]
        SVC-->>API: 400 Bad Request — reason is required (US-SGP-04 AC03)
        API-->>FE: 400 Bad Request
    end
    SVC->>+PREPO: GetStatus(periodId)
    PREPO->>DB: SELECT HrSalaryReviewPeriod
    DB-->>PREPO: status
    PREPO-->>-SVC: status
    break [period is not IN_PROGRESS]
        SVC-->>API: 409 Conflict — period no longer in progress (US-SGP-04 AC10)
        API-->>FE: 409 Conflict
    end
    SVC->>+EREPO: FindByPeriodAndEmployee(periodId, employeeId)
    EREPO->>DB: SELECT HrSalaryReviewEmployee
    DB-->>EREPO: row
    EREPO-->>-SVC: reviewEmployee
    break [not eligible — no proposed grade]
        SVC-->>API: 409 Conflict — employee has no proposal to review
        API-->>FE: 409 Conflict
    end
    SVC->>+EREPO: Update(Outcome=REJECTED, RejectionReason=reason)
    EREPO->>DB: UPDATE HrSalaryReviewEmployee SET Outcome='REJECTED', RejectionReason=...
    DB-->>EREPO: OK
    EREPO-->>-SVC: OK
    SVC-->>-API: ReviewPeriodEmployee
    API-->>-FE: 200 OK
    FE-->>HR: Row shows "Rejected"
```

Bulk Reject applies this same guard chain to each selected employee individually, with one shared reason recorded for every employee that succeeds — partial success (`US-SGP-04` AC06, AC07, AC09).

### 4.4 Submit a Review Period (US-SGP-05)

`POST /review-periods/{periodId}/submit`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as ReviewPeriodsController
    participant SVC as ReviewPeriodService
    participant EREPO as ReviewEmployeeRepository
    participant PREPO as ReviewPeriodRepository
    participant DB as HRM Database

    HR->>FE: Click "Submit for Approval"
    FE->>+API: POST /review-periods/{periodId}/submit
    API->>+SVC: SubmitReviewPeriod(periodId)
    SVC->>+EREPO: CountUnprocessedEligible(periodId)
    EREPO->>DB: SELECT COUNT(*) WHERE Eligible=1 AND Outcome='PENDING'
    DB-->>EREPO: count
    EREPO-->>-SVC: count
    break [count > 0]
        SVC-->>API: 409 Conflict — eligible employees still unprocessed (US-SGP-05 AC02)
        API-->>FE: 409 Conflict
    end
    SVC->>+PREPO: UpdateStatus(periodId, SUBMITTED)
    PREPO->>DB: UPDATE HrSalaryReviewPeriod SET Status='SUBMITTED'
    DB-->>PREPO: OK
    PREPO-->>-SVC: OK
    SVC-->>-API: ReviewPeriod
    API-->>-FE: 200 OK
    FE-->>HR: Period marked "Submitted"
```

### 4.5 Create a Salary Decision from a Review Period (US-SGP-06 / US-SGP-09)

`GET /salary-decisions/eligible-review-periods`, then `POST /salary-decisions`

```mermaid
sequenceDiagram
    actor APR as Approver
    participant FE as React Web App
    participant API as SalaryDecisionsController
    participant SVC as SalaryDecisionService
    participant PREPO as ReviewPeriodRepository
    participant EREPO as ReviewEmployeeRepository
    participant DREPO as SalaryDecisionRepository
    participant DB as HRM Database

    APR->>FE: Open "Salary Decisions", click "Create New"
    FE->>+API: GET /salary-decisions/eligible-review-periods
    API->>+SVC: GetEligibleReviewPeriods()
    SVC->>+PREPO: ListSubmittedWithoutNonCancelledDecision()
    PREPO->>DB: SELECT HrSalaryReviewPeriod WHERE Status='SUBMITTED' AND no non-cancelled HrSalaryDecision
    DB-->>PREPO: rows
    PREPO-->>-SVC: periods[]
    SVC-->>-API: ReviewPeriod[]
    API-->>-FE: 200 OK
    FE-->>APR: Prompts to pick one of the eligible periods (US-SGP-09 AC04)
    APR->>FE: Picks a review period, selects which Approved employees to include
    FE->>+API: POST /salary-decisions {reviewPeriodId, employeeIds, effectiveDate}
    API->>+SVC: CreateDecision(request)
    SVC->>+PREPO: FindById(reviewPeriodId)
    PREPO->>DB: SELECT HrSalaryReviewPeriod
    DB-->>PREPO: period
    PREPO-->>-SVC: period
    break [period is not SUBMITTED, already has a non-cancelled decision, or effectiveDate is before reviewDate]
        SVC-->>API: 409 or 400 Bad Request — see US-SGP-06 AC03, AC06
        API-->>FE: error
    end
    SVC->>+EREPO: FindApproved(reviewPeriodId, employeeIds)
    EREPO->>DB: SELECT HrSalaryReviewEmployee WHERE Outcome='APPROVED'
    DB-->>EREPO: matching rows
    EREPO-->>-SVC: all requested ids approved? yes/no
    break [any employeeId not Approved in this period]
        SVC-->>API: 400 Bad Request (US-SGP-06 AC02)
        API-->>FE: 400 Bad Request
    end
    SVC->>+DREPO: Add(decision, DRAFT), AddDetail (one line per employee, Baseline = current grade snapshot)
    DREPO->>DB: INSERT HrSalaryDecision, INSERT HrSalaryDecisionDetail
    DB-->>DREPO: OK
    DREPO-->>-SVC: OK
    SVC-->>-API: SalaryDecision (Draft)
    API-->>-FE: 201 Created
    FE-->>APR: Opens the new draft for this period
```

Listing existing decisions (`GET /salary-decisions`) and opening one by id, whether to resume a Draft or view an Applied/Cancelled decision read-only (`GET /salary-decisions/{decisionId}`), is a plain read with no backend branching — `GetDecision` returns the same shape regardless of status, only the frontend's editability differs (`US-SGP-09` AC02, AC03) — so it is not diagrammed separately.

### 4.6 Apply a Salary Decision — all-or-nothing (US-SGP-07)

`POST /salary-decisions/{decisionId}/apply` — the transaction rule from [Runtime View 6.2](../Arc42/06-runtime-view.md#62-errorrecovery--applying-a-decision-fails-partway).

```mermaid
sequenceDiagram
    actor APR as Approver
    participant FE as React Web App
    participant API as SalaryDecisionsController
    participant SVC as SalaryDecisionService
    participant DREPO as SalaryDecisionRepository
    participant SALREPO as EmployeeSalaryRepository
    participant PREPO as ReviewPeriodRepository
    participant UOW as HrmDbContext
    participant DB as HRM Database

    APR->>FE: Click "Apply Decision" and confirm
    FE->>+API: POST /salary-decisions/{decisionId}/apply
    API->>+SVC: ApplyDecision(decisionId)
    SVC->>+DREPO: ListDetails(decisionId)
    DREPO->>DB: SELECT HrSalaryDecisionDetail WHERE SalaryDecisionId=...
    DB-->>DREPO: rows
    DREPO-->>-SVC: details[]
    loop [for each employee in details]
        SVC->>+SALREPO: GetCurrent(employeeId)
        SALREPO->>DB: SELECT current HrEmployeeSalary
        DB-->>SALREPO: currentGradeId
        SALREPO-->>-SVC: currentGradeId
        SVC->>SVC: compare currentGradeId to detail.BaselineSalaryGradeId (US-SGP-07 AC04)
    end
    break [any employee's current grade no longer matches its snapshot baseline]
        SVC-->>API: 409 Conflict — employee X's grade changed since the decision was created (US-SGP-07 AC04)
        API-->>FE: 409 Conflict
        FE-->>APR: Error shown — no employee updated, decision remains Draft (US-SGP-07 AC02)
    end
    SVC->>+UOW: BeginTransaction()
    UOW-->>-SVC: transaction
    loop [for each employee in details]
        SVC->>+SALREPO: Add(new HrEmployeeSalary row, SalaryDecisionId=decisionId)
        SALREPO->>DB: INSERT HrEmployeeSalary
        SALREPO-->>-SVC: OK
    end
    SVC->>+DREPO: UpdateStatus(decisionId, APPLIED)
    DREPO->>DB: UPDATE HrSalaryDecision SET Status='APPLIED'
    DREPO-->>-SVC: OK
    SVC->>+PREPO: UpdateStatus(reviewPeriodId, CLOSED)
    PREPO->>DB: UPDATE HrSalaryReviewPeriod SET Status='CLOSED'
    PREPO-->>-SVC: OK
    SVC->>+UOW: CommitTransaction()
    UOW-->>-SVC: OK
    SVC-->>-API: SalaryDecisionDetail
    API-->>-FE: 200 OK
    FE-->>APR: Decision applied — salaries updated, review period closed
```

The comparison loop is a read-only validation pass and runs before any transaction starts. `UOW: HrmDbContext` then owns the transaction because the write phase spans `SALREPO`, `DREPO`, and `PREPO` together — no single one of those repositories could safely commit or roll back on behalf of the other two.

### 4.7 Look up an employee's salary history (US-SGP-08)

`GET /employees/{employeeId}/salary-history`

```mermaid
sequenceDiagram
    actor U as HR Staff / Approver
    participant FE as React Web App
    participant API as SalaryHistoryController
    participant SVC as SalaryHistoryService
    participant REPO as EmployeeSalaryRepository
    participant DB as HRM Database

    U->>FE: Search employee, open Salary History
    FE->>+API: GET /employees/{employeeId}/salary-history
    API->>+SVC: GetHistory(employeeId, filter)
    SVC->>+REPO: GetHistory(employeeId, filter)
    REPO->>DB: SELECT HrEmployeeSalary LEFT JOIN HrSalaryDecision WHERE EmployeeId=... ORDER BY EffectiveDate DESC
    DB-->>REPO: rows, newest first
    REPO-->>-SVC: history[]
    SVC-->>-API: SalaryHistoryPage
    API-->>-FE: 200 OK
    FE-->>U: Timeline rendered, newest first, each entry links to its Salary Decision when present (US-SGP-08 AC02)
```

### 4.8 Cancel a Review Period (US-SGP-11)

`POST /review-periods/{periodId}/cancel`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as ReviewPeriodsController
    participant SVC as ReviewPeriodService
    participant PREPO as ReviewPeriodRepository
    participant DREPO as SalaryDecisionRepository
    participant DB as HRM Database

    HR->>FE: Click "Cancel" on a review period row, confirm
    FE->>+API: POST /review-periods/{periodId}/cancel
    API->>+SVC: CancelReviewPeriod(periodId)
    SVC->>+PREPO: FindById(periodId)
    PREPO->>DB: SELECT HrSalaryReviewPeriod
    DB-->>PREPO: period
    PREPO-->>-SVC: period
    break [status is CLOSED or CANCELLED]
        SVC-->>API: 409 Conflict — already Closed/Cancelled (US-SGP-11 AC04, AC05)
        API-->>FE: 409 Conflict
    end
    SVC->>+DREPO: FindNonCancelledByPeriod(periodId)
    DREPO->>DB: SELECT HrSalaryDecision WHERE ReviewPeriodId=... AND Status<>'CANCELLED'
    DB-->>DREPO: row?
    DREPO-->>-SVC: exists?
    break [a non-cancelled decision already exists]
        SVC-->>API: 409 Conflict — cancel the decision first (US-SGP-11 AC03)
        API-->>FE: 409 Conflict
    end
    SVC->>+PREPO: UpdateStatus(periodId, CANCELLED)
    PREPO->>DB: UPDATE HrSalaryReviewPeriod SET Status='CANCELLED'
    DB-->>PREPO: OK
    PREPO-->>-SVC: OK
    SVC-->>-API: ReviewPeriod
    API-->>-FE: 200 OK
    FE-->>HR: Period marked "Cancelled"
```

### 4.9 Cancel a Salary Decision (US-SGP-10)

`POST /salary-decisions/{decisionId}/cancel`

```mermaid
sequenceDiagram
    actor APR as Approver
    participant FE as React Web App
    participant API as SalaryDecisionsController
    participant SVC as SalaryDecisionService
    participant REPO as SalaryDecisionRepository
    participant DB as HRM Database

    APR->>FE: Click "Cancel Decision" and confirm
    FE->>+API: POST /salary-decisions/{decisionId}/cancel
    API->>+SVC: CancelDecision(decisionId)
    SVC->>+REPO: FindById(decisionId)
    REPO->>DB: SELECT HrSalaryDecision
    DB-->>REPO: decision
    REPO-->>-SVC: decision
    break [status is APPLIED or already CANCELLED]
        SVC-->>API: 409 Conflict — only a Draft decision can be cancelled (US-SGP-10 AC02, AC03)
        API-->>FE: 409 Conflict
    end
    SVC->>+REPO: UpdateStatus(decisionId, CANCELLED)
    REPO->>DB: UPDATE HrSalaryDecision SET Status='CANCELLED'
    DB-->>REPO: OK
    REPO-->>-SVC: OK
    Note over SVC,DB: HrEmployeeSalary is never touched — a cancelled decision was never Applied, so there is nothing to revert.
    SVC-->>-API: SalaryDecisionDetail
    API-->>-FE: 200 OK
    FE-->>APR: Decision marked "Cancelled" — its review period can now have a new decision drafted (US-SGP-10 AC04)
```
