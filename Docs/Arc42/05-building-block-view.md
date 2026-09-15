# 5. Building Block View

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System (Salary Grade Promotion).*

This view is the C4 model, documented in detail in [`Docs/c4/`](../c4/README.md):

1. **System Context** — HRM System + its 2 actors.
2. **Container** — HRM Web Application, HRM Backend API, HRM Database.
3. **Component** (inside HRM Backend API) — Employee Management, Salary Management.

**Database building blocks** — see [Database Design](../Database/README.md) ([DBML source](../Database/HRM_Salary_Grade_Promotion.dbml), [Mermaid ER diagram](../Database/README.md#er-diagram-mermaid)): 8 tables — `HrEmployee`, `HrSalaryScale`, `HrSalaryGrade`, `HrEmployeeSalary`, `HrSalaryReviewPeriod`, `HrSalaryReviewEmployee`, `HrSalaryDecision`, `HrSalaryDecisionDetail`.

**Source code locations:** see [`Docs/CodeStructure/`](../CodeStructure/README.md) for how each container/component maps to an actual frontend/backend folder.

## 5.1 Illustrative Code-Level Detail (informative only)

The [Code Diagram](../c4/README.md#4-code-diagram) (`SalaryReviewController` → `SalaryReviewService` → `SalaryRepository`) illustrates the layered pattern inside Salary Management (see [ADR-04](09-architecture-decisions.md#adr-04-layered-design-inside-salary-management)). It is shown for illustration only — individual classes are not treated as a formal building-block level in this document, since arc42's Building Block View stops at the component level.
