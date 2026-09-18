# Database Design - HRM System

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-18 · **Implementation Baseline Commit:** `77e5716`

Database design for the full HRM system as currently analyzed: **Employee Profile**, **Organization Management**, **Salary Master Data**, and **Salary Grade Promotion** — 12 tables, traced back to the Business Rules in each module's `UserStories_*.md` / `UseCase_*.md`. The backend maps these 12 tables one-to-one through EF Core (12 entity configurations and a single `Initial` migration in `backend/src/HRM.Infrastructure/Persistence/`).

## ER Diagram (Mermaid)

```mermaid
erDiagram
    HrOrganizationalUnit ||--o{ HrOrganizationalUnit : "parent of"
    HrOrganizationalUnit ||--o{ HrEmployee : "assigned to"
    HrJobTitle ||--o{ HrEmployee : "assigned to"

    HrSalaryScale ||--o{ HrSalaryGrade : contains
    HrSalaryGrade ||--o{ HrSalaryGradeCoefficient : "coefficient history"
    HrSalaryGrade ||--o{ HrEmployeeSalary : "assigned as"
    HrSalaryGrade ||--o{ HrSalaryReviewEmployee : "current / proposed grade"
    HrSalaryGrade ||--o{ HrSalaryDecisionDetail : "baseline / new grade"

    HrEmployee ||--o{ HrEmployeeSalary : has
    HrEmployee ||--o{ HrSalaryReviewEmployee : "is reviewed in"
    HrEmployee ||--o{ HrSalaryDecisionDetail : "affected by"

    HrSalaryReviewPeriod ||--o{ HrSalaryReviewEmployee : contains
    HrSalaryReviewPeriod ||--o{ HrSalaryDecision : "drafted from"
    HrSalaryDecision ||--o{ HrSalaryDecisionDetail : contains
    HrSalaryDecision ||--o{ HrEmployeeSalary : causes

    HrOrganizationalUnit {
        int Id PK
        string Name
        int ParentId FK
        string UnitType
        string ContactEmail
        string ContactPhone
        string Status
    }
    HrJobTitle {
        int Id PK
        string Name UK
        string Status
    }
    HrEmployee {
        int Id PK
        string EmployeeCode UK
        string FullName
        int OrganizationalUnitId FK
        int JobTitleId FK
        date JoinDate
        string EmploymentStatus
    }
    HrBaseSalaryRate {
        int Id PK
        decimal Rate
        date EffectiveDate UK
    }
    HrSalaryScale {
        int Id PK
        string Code UK
        string Name UK
        string Status
    }
    HrSalaryGrade {
        int Id PK
        int SalaryScaleId FK
        int GradeNumber
        string Status
    }
    HrSalaryGradeCoefficient {
        int Id PK
        int SalaryGradeId FK
        decimal Coefficient
        date EffectiveDate
    }
    HrEmployeeSalary {
        int Id PK
        int EmployeeId FK
        int SalaryGradeId FK
        decimal Coefficient
        date EffectiveDate
        string Reason
        int SalaryDecisionId FK
    }
    HrSalaryReviewPeriod {
        int Id PK
        string Code UK
        string Name UK
        string ReviewType
        date ReviewDate
        date EffectiveDate
        string Status
    }
    HrSalaryReviewEmployee {
        int Id PK
        int ReviewPeriodId FK
        int EmployeeId FK
        int CurrentSalaryGradeId FK
        bool Eligible
        int ProposedSalaryGradeId FK
        string Outcome
    }
    HrSalaryDecision {
        int Id PK
        int ReviewPeriodId FK
        string DecisionNumber UK
        date EffectiveDate
        string Status
    }
    HrSalaryDecisionDetail {
        int Id PK
        int SalaryDecisionId FK
        int EmployeeId FK
        int BaselineSalaryGradeId FK
        int NewSalaryGradeId FK
    }
```

`HrBaseSalaryRate` has no relationships to other tables — it is a single organization-wide effective-dated value, not joined per employee or grade (see the Business Rules note in the `.dbml`).

## Mapping Tables to the UI

| Table | UI Role |
|---|---|
| `HrOrganizationalUnit` | Organization Structure tree, and the Organizational Unit picker on Employee/Review screens. |
| `HrJobTitle` | Job Titles catalog, and the Job Title picker on Employee screens. |
| `HrEmployee` | Employee List/Detail, and the employee identity shown on review/history/decision screens. |
| `HrBaseSalaryRate` | Base Salary Rate screen and its effective-dated history. |
| `HrSalaryScale` | Salary Scales list and Salary Scale Detail header. |
| `HrSalaryGrade` | Salary Grades table within Salary Scale Detail. |
| `HrSalaryGradeCoefficient` | Coefficient history shown when updating a Salary Grade. |
| `HrEmployeeSalary` | Employee Salary History timeline, and the "current salary" facts on Employee Review Detail. |
| `HrSalaryReviewPeriod` | Review Period List / Review Period Detail. |
| `HrSalaryReviewEmployee` | Employee list and review results within a Review Period; Employee Review Detail. |
| `HrSalaryDecision` | Salary Decision List / Salary Decision Detail header. |
| `HrSalaryDecisionDetail` | Included-employees table within Salary Decision Detail. |

## Files

- [`HRM_System.dbml`](HRM_System.dbml) — DBML source code. Together with the Mermaid diagram above, this is the single source of truth for the schema — edit here first, then update the Mermaid diagram to match.
