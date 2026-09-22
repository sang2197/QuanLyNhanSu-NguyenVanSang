# 5. Building Block View

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System.*

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-22 · **Implementation Baseline Commit:** `77e5716`

This view is the C4 model, documented in detail in [`Docs/c4/`](../c4/README.md):

1. **System Context** — HRM System + its 2 actors.
2. **Container** — HRM Web Application, HRM Backend API, HRM Database.
3. **Component** (inside HRM Backend API) — Employee Management, Organization Management, Salary Master Data, Salary Grade Promotion, and Contract Management. All five are implemented in `backend/`, each split across the same four projects (`HRM.Domain`, `HRM.Application`, `HRM.Infrastructure`, `HRM.Api`).

**Database building blocks** — see [Database Design](../Database/README.md) ([DBML source](../Database/HRM_System.dbml), [Mermaid ER diagram](../Database/README.md#er-diagram-mermaid)): 13 tables across all five analyzed modules — `HrOrganizationalUnit`, `HrJobTitle`, `HrEmployee`, `HrBaseSalaryRate`, `HrSalaryScale`, `HrSalaryGrade`, `HrSalaryGradeCoefficient`, `HrEmployeeSalary`, `HrSalaryReviewPeriod`, `HrSalaryReviewEmployee`, `HrSalaryDecision`, `HrSalaryDecisionDetail`, and `HrLaborContract`. The backend implementation (`backend/`) maps all 13 tables one-to-one through EF Core (`HrmDbContext`, 13 entity configurations, the `Initial` and `AddLaborContract` migrations) — see `backend/README.md`.

**Source code locations:** see [`Docs/CodeStructure/`](../CodeStructure/README.md) for how each container/component maps to an actual frontend/backend folder.

## 5.1 Illustrative Code-Level Detail (informative only)

The C4 model used here stops at the Component level — see [`Docs/c4/README.md`](../c4/README.md). Code-level detail (`ReviewPeriodsController` → `ReviewPeriodService` → `ReviewPeriodRepository`, illustrating the layered pattern used inside every component per [ADR-04](09-architecture-decisions.md#adr-04-layered-design-inside-each-backend-component)) is documented separately in the [Class Diagram](../DetailedDesign/ClassDiagram.md). It is shown for illustration only — individual classes are not treated as a formal building-block level in this document, since arc42's Building Block View stops at the component level.
