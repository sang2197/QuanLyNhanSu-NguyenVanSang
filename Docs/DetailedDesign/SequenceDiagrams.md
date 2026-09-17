# Sequence Diagrams - Salary Grade Promotion

Seven key flows, matching the [Runtime View](../Arc42/06-runtime-view.md) scenarios and the [User Stories](../Requirements/UserStories_SalaryGradePromotion.md) / [OpenAPI spec](../API/openapi.yaml) they implement. Class names match [ClassDiagram.md](ClassDiagram.md).

## 1. Approve an employee's proposed grade (US-04)

`POST /review-periods/{periodId}/employees/{employeeId}/approve` — the same
period-status guard shown here also applies to reject, bulk-approve, and
bulk-reject (US-04/US-05).

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as ReviewPeriodsController
    participant SVC as SalaryReviewService
    participant REPO as SalaryRepository
    participant DB as HRM Database

    HR->>FE: Click "Approve" on employee row
    FE->>API: POST .../employees/{employeeId}/approve
    API->>SVC: ApproveEmployee(periodId, employeeId)
    SVC->>REPO: GetReviewPeriodStatus(periodId)
    REPO->>DB: SELECT HrSalaryReviewPeriod
    DB-->>REPO: status
    REPO-->>SVC: status
    alt period is not IN_PROGRESS
        SVC-->>API: 409 Conflict — period no longer in progress (US-05)
        API-->>FE: 409 Conflict
    else period is IN_PROGRESS
        SVC->>REPO: GetReviewEmployee(periodId, employeeId)
        REPO->>DB: SELECT HrSalaryReviewEmployee
        DB-->>REPO: row
        REPO-->>SVC: ReviewEmployee
        alt not eligible or already has an outcome
            SVC-->>API: 409 Conflict
            API-->>FE: 409 Conflict
        else eligible and pending
            SVC->>REPO: UpdateReviewOutcome(id, Approved)
            REPO->>DB: UPDATE HrSalaryReviewEmployee SET ReviewOutcome='APPROVED'
            DB-->>REPO: OK
            REPO-->>SVC: OK
            SVC-->>API: ReviewPeriodEmployee
            API-->>FE: 200 OK
        end
    end
    FE-->>HR: Row shows "Approved"
```

## 2. Submit a review period (US-05)

`POST /review-periods/{periodId}/submit`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as ReviewPeriodsController
    participant SVC as SalaryReviewService
    participant REPO as SalaryRepository
    participant DB as HRM Database

    HR->>FE: Click "Submit for Approval"
    FE->>API: POST /review-periods/{periodId}/submit
    API->>SVC: SubmitReviewPeriod(periodId)
    SVC->>REPO: CountUnprocessedEmployees(periodId)
    REPO->>DB: SELECT COUNT(*) WHERE EligibilityStatus='ELIGIBLE' AND ReviewOutcome='PENDING'
    DB-->>REPO: count
    REPO-->>SVC: count
    alt count > 0
        SVC-->>API: 409 Conflict — eligible employees still unprocessed
        API-->>FE: 409 Conflict
    else count == 0
        SVC->>REPO: UpdatePeriodStatus(periodId, SUBMITTED)
        REPO->>DB: UPDATE HrSalaryReviewPeriod SET Status='SUBMITTED'
        DB-->>REPO: OK
        REPO-->>SVC: OK
        SVC-->>API: ReviewPeriod
        API-->>FE: 200 OK
    end
    FE-->>HR: Period marked "Submitted"
```

## 3. Apply a salary decision — all-or-nothing (US-07)

`POST /salary-decisions/{decisionId}/apply` — the transaction rule from [Runtime View 6.2](../Arc42/06-runtime-view.md#62-errorrecovery--applying-a-decision-fails-partway).

```mermaid
sequenceDiagram
    actor APR as Approver
    participant FE as React Web App
    participant API as SalaryDecisionsController
    participant SVC as SalaryDecisionService
    participant REPO as SalaryRepository
    participant DB as HRM Database

    APR->>FE: Click "Apply Decision" and confirm
    FE->>API: POST /salary-decisions/{decisionId}/apply
    API->>SVC: ApplyDecision(decisionId)
    SVC->>REPO: BeginTransaction()
    SVC->>REPO: GetDecisionDetails(decisionId)
    REPO->>DB: SELECT HrSalaryDecisionDetail WHERE DecisionId=...
    DB-->>REPO: rows
    REPO-->>SVC: details[]
    loop for each employee in details
        SVC->>REPO: CheckEffectiveDateConflict(employeeId, effectiveFrom)
        REPO->>DB: SELECT HrEmployeeSalary WHERE date range overlaps
        DB-->>REPO: conflict? yes/no
        REPO-->>SVC: result
    end
    alt any conflict found
        SVC->>REPO: RollbackTransaction()
        SVC-->>API: 409 Conflict (employee X, reason)
        API-->>FE: 409 Conflict
        FE-->>APR: Error shown — no employee updated
    else no conflicts
        loop for each employee in details
            SVC->>REPO: CloseCurrentSalary(employeeId, effectiveFrom)
            REPO->>DB: UPDATE HrEmployeeSalary SET EffectiveTo=...
            SVC->>REPO: CreateNewSalary(employeeId, newGrade, decisionId)
            REPO->>DB: INSERT HrEmployeeSalary
        end
        SVC->>REPO: UpdateDecisionStatus(decisionId, APPLIED)
        REPO->>DB: UPDATE HrSalaryDecision SET Status='APPLIED'
        SVC->>REPO: UpdatePeriodStatus(reviewPeriodId, CLOSED)
        REPO->>DB: UPDATE HrSalaryReviewPeriod SET Status='CLOSED'
        SVC->>REPO: CommitTransaction()
        REPO-->>SVC: OK
        SVC-->>API: SalaryDecisionDetail
        API-->>FE: 200 OK
        FE-->>APR: Decision applied — salaries updated, review period closed
    end
```

## 4. Look up an employee's salary history (US-08)

`GET /employees/{employeeId}/salary-history`

```mermaid
sequenceDiagram
    actor U as HR Staff / Approver
    participant FE as React Web App
    participant API as SalaryHistoryController
    participant SVC as SalaryHistoryService
    participant REPO as SalaryRepository
    participant DB as HRM Database

    U->>FE: Search employee, open Salary History
    FE->>API: GET /employees/{employeeId}/salary-history
    API->>SVC: GetHistory(employeeId, fromDate, toDate)
    SVC->>REPO: GetSalaryHistory(employeeId, fromDate, toDate)
    REPO->>DB: SELECT HrEmployeeSalary JOIN HrSalaryDecision WHERE EmployeeId=...
    DB-->>REPO: rows, newest first
    REPO-->>SVC: history[]
    SVC-->>API: SalaryHistoryPage
    API-->>FE: 200 OK
    FE-->>U: Timeline rendered, newest first
```

## 5. List salary decisions and start or resume one (US-09)

`GET /salary-decisions` then either `POST /salary-decisions` (new) or `GET /salary-decisions/{decisionId}` (resume)

```mermaid
sequenceDiagram
    actor APR as Approver
    participant FE as React Web App
    participant API as SalaryDecisionsController
    participant SVC as SalaryDecisionService
    participant REPO as SalaryRepository
    participant DB as HRM Database

    APR->>FE: Open "Salary Decisions" from the menu
    FE->>API: GET /salary-decisions
    API->>SVC: GetDecisions(filter)
    SVC->>REPO: ListDecisions(filter)
    REPO->>DB: SELECT HrSalaryDecision
    DB-->>REPO: rows
    REPO-->>SVC: decisions[]
    SVC-->>API: SalaryDecisionPage
    API-->>FE: 200 OK
    FE-->>APR: List rendered (Draft / Applied / Cancelled)

    alt clicks a Draft row
        FE->>API: GET /salary-decisions/{decisionId}
        API->>SVC: GetDecision(decisionId)
        SVC->>REPO: GetDecisionDetails(decisionId)
        REPO->>DB: SELECT HrSalaryDecision, HrSalaryDecisionDetail
        DB-->>REPO: rows
        REPO-->>SVC: decision + employees
        SVC-->>API: SalaryDecisionDetail
        API-->>FE: 200 OK
        FE-->>APR: Resumes drafting, same employees as before
    else clicks Applied or Cancelled row
        FE->>API: GET /salary-decisions/{decisionId}
        API-->>FE: 200 OK
        FE-->>APR: Opens in read-only mode
    else clicks "Create New"
        FE-->>APR: Prompts to pick a submitted period without an existing decision
        APR->>FE: Picks a review period, then selects which approved employees to include
        FE->>API: POST /salary-decisions {reviewPeriodId, employeeIds, ...}
        API->>SVC: CreateDecision(reviewPeriodId, employeeIds, ...)
        SVC->>REPO: CheckNoExistingDecision(reviewPeriodId)
        alt period already has a non-cancelled decision
            SVC-->>API: 409 Conflict
            API-->>FE: 409 Conflict
        else no existing decision
            SVC->>REPO: CheckAllApproved(reviewPeriodId, employeeIds)
            REPO->>DB: SELECT HrSalaryReviewEmployee WHERE ReviewOutcome='APPROVED'
            DB-->>REPO: matching rows
            REPO-->>SVC: all approved? yes/no
            alt any employeeId not approved in this period
                SVC-->>API: 400 Bad Request
                API-->>FE: 400 Bad Request
            else all approved
                SVC->>REPO: InsertDecision(reviewPeriodId, employeeIds, ...)
                REPO->>DB: INSERT HrSalaryDecision, INSERT HrSalaryDecisionDetail (one per employee)
                SVC-->>API: SalaryDecision (Draft)
                API-->>FE: 201 Created
                FE-->>APR: Opens the new draft for this period
            end
        end
    end
```

## 6. Cancel a review period (US-11)

`POST /review-periods/{periodId}/cancel`

```mermaid
sequenceDiagram
    actor HR as HR Staff
    participant FE as React Web App
    participant API as ReviewPeriodsController
    participant SVC as SalaryReviewService
    participant REPO as SalaryRepository
    participant DB as HRM Database

    HR->>FE: Click "Cancel" on a review period row
    FE->>API: POST /review-periods/{periodId}/cancel
    API->>SVC: CancelReviewPeriod(periodId)
    SVC->>REPO: GetReviewPeriod(periodId)
    REPO->>DB: SELECT HrSalaryReviewPeriod
    DB-->>REPO: row
    REPO-->>SVC: period
    alt status is CLOSED or CANCELLED
        SVC-->>API: 409 Conflict — already Closed/Cancelled
        API-->>FE: 409 Conflict
    else status is IN_PROGRESS or SUBMITTED
        SVC->>REPO: CheckNoExistingDecision(periodId)
        REPO->>DB: SELECT HrSalaryDecision WHERE ReviewPeriodId=... AND Status<>'CANCELLED'
        DB-->>REPO: row?
        REPO-->>SVC: exists? yes/no
        alt a non-cancelled decision already exists
            SVC-->>API: 409 Conflict — cancel the decision first (US-10)
            API-->>FE: 409 Conflict
        else no decision exists
            SVC->>REPO: UpdatePeriodStatus(periodId, CANCELLED)
            REPO->>DB: UPDATE HrSalaryReviewPeriod SET Status='CANCELLED'
            DB-->>REPO: OK
            REPO-->>SVC: OK
            SVC-->>API: ReviewPeriod
            API-->>FE: 200 OK
        end
    end
    FE-->>HR: Period marked "Cancelled"
```

## 7. Cancel a salary decision (US-10)

`POST /salary-decisions/{decisionId}/cancel`

```mermaid
sequenceDiagram
    actor APR as Approver
    participant FE as React Web App
    participant API as SalaryDecisionsController
    participant SVC as SalaryDecisionService
    participant REPO as SalaryRepository
    participant DB as HRM Database

    APR->>FE: Click "Cancel Decision" and confirm
    FE->>API: POST /salary-decisions/{decisionId}/cancel
    API->>SVC: CancelDecision(decisionId)
    SVC->>REPO: GetDecision(decisionId)
    REPO->>DB: SELECT HrSalaryDecision
    DB-->>REPO: row
    REPO-->>SVC: decision
    alt status is APPLIED or already CANCELLED
        SVC-->>API: 409 Conflict — only a Draft decision can be cancelled (US-10)
        API-->>FE: 409 Conflict
    else status is DRAFT
        SVC->>REPO: UpdateDecisionStatus(decisionId, CANCELLED)
        REPO->>DB: UPDATE HrSalaryDecision SET Status='CANCELLED'
        DB-->>REPO: OK
        REPO-->>SVC: OK
        Note over SVC,DB: HrEmployeeSalary is never touched — a cancelled<br/>decision was never Applied, so there is nothing to revert.
        SVC-->>API: SalaryDecisionDetail
        API-->>FE: 200 OK
    end
    FE-->>APR: Decision marked "Cancelled" — its review period can now have a new decision drafted
```
