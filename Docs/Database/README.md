# Database Design - Salary Grade Promotion

Database design for the **Salary Grade Promotion** feature: 8 tables covering employees, salary scales/grades, salary history, review periods, and salary decisions.

## ER Diagram (Mermaid)

```mermaid
erDiagram
    HrEmployee ||--o{ HrEmployeeSalary : has
    HrEmployee ||--o{ HrSalaryReviewEmployee : "is reviewed in"
    HrEmployee ||--o{ HrSalaryDecisionDetail : "affected by"
    HrEmployee ||--o{ HrSalaryDecision : signs
    HrSalaryScale ||--o{ HrSalaryGrade : contains
    HrSalaryScale ||--o{ HrEmployeeSalary : "used in"
    HrSalaryGrade ||--o{ HrEmployeeSalary : "assigned as"
    HrSalaryGrade ||--o{ HrSalaryReviewEmployee : "current / proposed grade"
    HrSalaryGrade ||--o{ HrSalaryDecisionDetail : "old / new grade"
    HrSalaryReviewPeriod ||--o{ HrSalaryReviewEmployee : contains
    HrSalaryDecision ||--o{ HrSalaryDecisionDetail : contains
    HrSalaryDecision ||--o{ HrEmployeeSalary : causes
    HrEmployeeSalary ||--o{ HrSalaryReviewEmployee : "current salary"
    HrEmployeeSalary ||--o{ HrSalaryDecisionDetail : "old salary"

    HrEmployee {
        int Id PK
        string EmployeeCode UK
        string FullName
        int DepartmentId
        int PositionId
        date JoinDate
        string Status
    }
    HrSalaryScale {
        int Id PK
        string Code UK
        string Name
        date EffectiveFrom
        date EffectiveTo
        string Status
    }
    HrSalaryGrade {
        int Id PK
        int SalaryScaleId FK
        int GradeNumber
        decimal Coefficient
        date EffectiveFrom
        date EffectiveTo
        string Status
    }
    HrEmployeeSalary {
        int Id PK
        int EmployeeId FK
        int SalaryScaleId FK
        int SalaryGradeId FK
        decimal Coefficient
        date EffectiveFrom
        date EffectiveTo
        string Reason
        int DecisionId FK
    }
    HrSalaryReviewPeriod {
        int Id PK
        string Code UK
        string Name
        string ReviewType
        date ReviewDate
        date EffectiveDate
        string Status
    }
    HrSalaryReviewEmployee {
        int Id PK
        int ReviewPeriodId FK
        int EmployeeId FK
        int CurrentSalaryId FK
        int CurrentGradeId FK
        int ProposedGradeId FK
        string EligibilityStatus
        string ReviewStatus
    }
    HrSalaryDecision {
        int Id PK
        string DecisionNumber UK
        date DecisionDate
        date EffectiveDate
        string DecisionType
        string Status
        int SignerEmployeeId FK
    }
    HrSalaryDecisionDetail {
        int Id PK
        int DecisionId FK
        int EmployeeId FK
        int OldSalaryId FK
        int OldGradeId FK
        int NewSalaryGradeId FK
        decimal NewCoefficient
        date EffectiveFrom
    }
```

## Files

- [`HRM_Salary_Grade_Promotion.dbml`](HRM_Salary_Grade_Promotion.dbml) — DBML source code (from dbdiagram.io). This, together with the Mermaid diagram above, is the source of truth for the schema — edit here first, then re-export SQL/PNG if the schema changes.
- [`DB_Diagram.png`](DB_Diagram.png) — Entity-relationship diagram exported from dbdiagram.io (legacy reference; the Mermaid diagram above is the standardized version).
- [`Gen_Table.sql`](Gen_Table.sql) — SQL script (generated from dbdiagram.io) to create the 8 tables, keys, indexes, and foreign keys.
- [`HRM_Salary_Grade_Promotion_Database_Design_EN.docx`](HRM_Salary_Grade_Promotion_Database_Design_EN.docx) — Database design write-up (English): purpose, keys, indexes, and business notes per table.
- [`thiet_ke_CSDL_nang_bac_luong_8_bang.docx`](thiet_ke_CSDL_nang_bac_luong_8_bang.docx) — Same database design write-up (Vietnamese).
