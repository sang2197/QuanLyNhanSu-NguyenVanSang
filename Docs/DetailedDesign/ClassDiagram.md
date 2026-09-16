# Class Diagram - Salary Grade Promotion

Derived from [Database Design](../Database/README.md) (fields), [Code Structure](../CodeStructure/README.md) (class/interface names), and [C4 Component/Code diagrams](../c4/README.md).

## 1. Domain Model

The 8 entities from the [Database Design](../Database/README.md), shown as domain classes with their relationships (same relationships as the [Mermaid ER diagram](../Database/README.md#er-diagram-mermaid), plus the enums used by the [OpenAPI spec](../API/openapi.yaml)).

```mermaid
classDiagram
    class HrEmployee {
        +int Id
        +string EmployeeCode
        +string FullName
        +int DepartmentId
        +int PositionId
        +DateTime JoinDate
        +string Status
    }
    class HrSalaryScale {
        +int Id
        +string Code
        +string Name
        +DateTime EffectiveFrom
        +DateTime EffectiveTo
        +string Status
    }
    class HrSalaryGrade {
        +int Id
        +int SalaryScaleId
        +int GradeNumber
        +decimal Coefficient
        +DateTime EffectiveFrom
        +DateTime EffectiveTo
        +string Status
    }
    class HrEmployeeSalary {
        +int Id
        +int EmployeeId
        +int SalaryScaleId
        +int SalaryGradeId
        +decimal Coefficient
        +DateTime EffectiveFrom
        +DateTime EffectiveTo
        +string Reason
        +int DecisionId
    }
    class HrSalaryReviewPeriod {
        +int Id
        +string Code
        +string Name
        +ReviewType ReviewType
        +DateTime ReviewDate
        +DateTime EffectiveDate
        +ReviewPeriodStatus Status
    }
    class HrSalaryReviewEmployee {
        +int Id
        +int ReviewPeriodId
        +int EmployeeId
        +int CurrentSalaryId
        +int CurrentGradeId
        +int ProposedGradeId
        +EligibilityStatus EligibilityStatus
        +ReviewOutcome ReviewOutcome
        +string Reason
    }
    class HrSalaryDecision {
        +int Id
        +int ReviewPeriodId
        +string DecisionNumber
        +DateTime DecisionDate
        +DateTime EffectiveDate
        +DecisionType DecisionType
        +SalaryDecisionStatus Status
        +int SignerEmployeeId
    }
    class HrSalaryDecisionDetail {
        +int Id
        +int DecisionId
        +int EmployeeId
        +int OldSalaryId
        +int OldGradeId
        +int NewSalaryGradeId
        +decimal NewCoefficient
        +DateTime EffectiveFrom
    }

    HrSalaryScale "1" --> "*" HrSalaryGrade : contains
    HrEmployee "1" --> "*" HrEmployeeSalary : has
    HrSalaryScale "1" --> "*" HrEmployeeSalary : used in
    HrSalaryGrade "1" --> "*" HrEmployeeSalary : assigned as
    HrSalaryDecision "1" --> "*" HrEmployeeSalary : causes
    HrSalaryReviewPeriod "1" --> "0..1" HrSalaryDecision : drafted from
    HrSalaryReviewPeriod "1" --> "*" HrSalaryReviewEmployee : contains
    HrEmployee "1" --> "*" HrSalaryReviewEmployee : is reviewed in
    HrEmployeeSalary "1" --> "*" HrSalaryReviewEmployee : current salary
    HrSalaryGrade "1" --> "*" HrSalaryReviewEmployee : current or proposed grade
    HrSalaryDecision "1" --> "*" HrSalaryDecisionDetail : contains
    HrEmployee "1" --> "*" HrSalaryDecisionDetail : affected by
    HrEmployeeSalary "1" --> "*" HrSalaryDecisionDetail : old salary
    HrSalaryGrade "1" --> "*" HrSalaryDecisionDetail : old or new grade
    HrEmployee "1" --> "*" HrSalaryDecision : signs
```

## 2. API / Service / Repository Layer (Salary Management)

Matches [`BackendStructure.md`](../CodeStructure/BackendStructure.md) — Controller → Service → Repository per [ADR-04](../Arc42/09-architecture-decisions.md#adr-04-layered-design-inside-salary-management). Controller methods map directly to [`openapi.yaml`](../API/openapi.yaml) operations.

> Each Service/Repository is coded behind an interface (`ISalaryReviewService`, `ISalaryRepository`, etc. — see [`BackendStructure.md`](../CodeStructure/BackendStructure.md)) for dependency injection. Those interfaces are real in the code but omitted here as separate boxes — they add no fields/methods of their own and only made this diagram wider without adding information.

```mermaid
classDiagram
    class ReviewPeriodsController {
        +CreateReviewPeriod(request) ReviewPeriod
        +GetReviewPeriods(filter) ReviewPeriodPage
        +GetReviewPeriod(periodId) ReviewPeriodDetail
        +SubmitReviewPeriod(periodId) ReviewPeriod
        +CancelReviewPeriod(periodId) ReviewPeriod
        +GetEmployees(periodId, filter) ReviewPeriodEmployeePage
        +GetEmployee(periodId, employeeId) ReviewPeriodEmployeeDetail
        +ApproveEmployee(periodId, employeeId) ReviewPeriodEmployee
        +RejectEmployee(periodId, employeeId, reason) ReviewPeriodEmployee
        +BulkApprove(periodId, employeeIds) BulkActionResult
        +BulkReject(periodId, employeeIds, reason) BulkActionResult
    }
    class SalaryDecisionsController {
        +CreateDecision(reviewPeriodId, employeeIds, ...) SalaryDecision
        +GetDecisions(filter) SalaryDecisionPage
        +GetDecision(decisionId) SalaryDecisionDetail
        +RemoveEmployee(decisionId, employeeId)
        +ApplyDecision(decisionId) SalaryDecisionDetail
        +CancelDecision(decisionId) SalaryDecisionDetail
    }
    class SalaryHistoryController {
        +GetSalaryHistory(employeeId, filter) SalaryHistoryPage
    }

    class SalaryReviewService {
        -SalaryRepository salaryRepository
        -EmployeeRepository employeeRepository
        +CreateReviewPeriod(request) : calculates proposed grades synchronously, US-01
        +SubmitReviewPeriod(periodId)
        +CancelReviewPeriod(periodId) : blocked if a non-cancelled decision exists, US-11
        +ApproveEmployee(periodId, employeeId) : blocked unless period is IN_PROGRESS, US-04/US-05
        +RejectEmployee(periodId, employeeId, reason) : blocked unless period is IN_PROGRESS, US-04/US-05
        +BulkApprove(periodId, employeeIds)
        +BulkReject(periodId, employeeIds, reason)
        -CalculateProposedGrade(employee) : applies the eligibility rule, US-03
    }
    class SalaryDecisionService {
        -SalaryRepository salaryRepository
        +CreateDecision(reviewPeriodId, employeeIds, ...) : employees fixed at creation, US-06
        +RemoveEmployee(decisionId, employeeId)
        +ApplyDecision(decisionId) : all-or-nothing transaction, US-07
        +CancelDecision(decisionId) : status-only, never touches HrEmployeeSalary, US-10
    }
    class SalaryHistoryService {
        -SalaryRepository salaryRepository
        +GetHistory(employeeId, fromDate, toDate)
    }

    class SalaryRepository {
        -HrmDbContext context
    }
    class EmployeeRepository {
        -HrmDbContext context
    }

    ReviewPeriodsController --> SalaryReviewService
    SalaryDecisionsController --> SalaryDecisionService
    SalaryHistoryController --> SalaryHistoryService
    SalaryReviewService --> SalaryRepository
    SalaryReviewService --> EmployeeRepository
    SalaryDecisionService --> SalaryRepository
    SalaryHistoryService --> SalaryRepository
```
