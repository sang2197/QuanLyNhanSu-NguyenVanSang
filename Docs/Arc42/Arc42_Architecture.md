# Arc42 Architecture Documentation - HRM System (Salary Grade Promotion)

This document follows the [arc42](https://arc42.org/) template. It summarizes the architecture of the HRM System, focused on the **Salary Grade Promotion** feature that has been analyzed and designed so far.

## 1. Introduction and Goals

### 1.1 Overview

The HRM System supports core human resource management processes within
an organization.

**Scope of this document:** Only the *Salary Grade Promotion* feature, part of the *Salary Management* function group. The HRM system has 5 other function groups (Employee Profile, Department/Unit, Reward/Discipline, Attendance, Contract) that are out of scope here — see the [mind map](../Quản%20lý%20nhân%20sự.xmind).

The system is primarily used by HR Staff and authorized Approvers.

### 1.2 Goals

The system aims to centralize HR data and support HR business processes through a unified application.

Important process should maintain consistent data and preserve relevant historical changes.

### 1.3 Stakeholders

**HR Staff**

Performs day-to-day HR operations and manages employee-related
information and processes.

**Approver**

Reviews HR requests and makes final decisions when approval is required.

## 2. Architecture Constraints

### 2.1 Technology Constraints

**Frontend**

-   JavaScript
-   Angular

**Backend**

-   ASP.NET Core

**Database**

-   Microsoft SQL Server

### 2.2 Business Constraints

Important changes such as salary grade promotion must go through a
review and approval process before becoming official data.

Historical salary information and HR decisions should be preserved
rather than simply overwriting previous values.

## 3. System Scope and Context

**Business context** — see the [System Context Diagram](../c4/README.md#1-system-context-diagram):

- **HR Staff** (person) — performs day-to-day HR operations, uses the system to manage review periods and prepare decisions.
- **Approver / Manager** (person) — reviews and approves/rejects proposals, has authority to make final decisions.
- **HRM System** (software system) — manages employee information and HR processes.

**Technical context** — see the [Container Diagram](../c4/README.md#2-container-diagram): the Web Application calls the Backend API over HTTPS/REST/JSON; the Backend API reads/writes the Database over SQL.

## 4. Solution Strategy

- **3-tier web architecture**: Angular SPA (presentation) → ASP.NET Core Web API (business logic) → SQL Server (persistence). See [Container Diagram](../c4/README.md#2-container-diagram).
- **Split the backend by business domain**, not by technical layer, at the component level: `Employee Management` and `Salary Management` are separate components so Salary Management can evolve (e.g. add Allowance Management later) without touching Employee Management. See [Component Diagram](../c4/README.md#3-component-diagram).
- **Inside Salary Management, use a classic layered pattern**: Controller → Service → Repository, so business rules (eligibility, approval, effective-dating) live in one place (the Service) instead of being scattered. See [Code Diagram](../c4/README.md#4-code-diagram).
- **Never overwrite salary history**: every salary change is a new `HrEmployeeSalary` row with its own effective period, linked to the decision that caused it. This is the key decision that shapes most of the database design.
- **Separate "proposal" from "official record"**: a review result (`HrSalaryReviewEmployee`) is only a proposal and does not change any employee's real salary; only an issued `HrSalaryDecision` does. This keeps the review process safe to redo/reject without side effects.

## 5. Building Block View

This view is the C4 model, already documented in detail in [`Docs/c4/`](../c4/README.md):

1. **System Context** — HRM System + its 2 actors.
2. **Container** — HRM Web Application, HRM Backend API, HRM Database.
3. **Component** (inside HRM Backend API) — Employee Management, Salary Management.
4. **Code** (inside Salary Management component) — `SalaryReviewController` → `SalaryReviewService` → `SalaryRepository`.

**Database building blocks** — see [Database Design](../README.md#database-design) ([DBML source](../HRM_Salary_Grade_Promotion.dbml), [ER diagram](../DB_Diagram.png)): 8 tables — `HrEmployee`, `HrSalaryScale`, `HrSalaryGrade`, `HrEmployeeSalary`, `HrSalaryReviewPeriod`, `HrSalaryReviewEmployee`, `HrSalaryDecision`, `HrSalaryDecisionDetail`.

## 6. Runtime View

**Main scenario: "Process a salary grade review period end-to-end"** (matches the [Use Case Diagram](../UseCase_SalaryGradePromotion.md) and the wireframe's navigation flow):

1. HR Staff creates a **Review Period** (e.g. "Annual Review H1 2026").
2. The system loads all employees into the period and checks eligibility automatically (e.g. minimum time in current grade).
3. HR Staff opens the **Review Period Detail** screen, filters/searches employees, and opens individual employees to propose a new grade.
4. Approver opens **Employee Review Detail**, checks the salary snapshot and eligibility reason, then **Approves** or **Rejects** the proposal (reason required if rejected).
5. Once enough employees are processed, HR Staff **Submits for Approval** at the period level.
6. HR Staff opens **Create Salary Decision**, which lists only approved employees, fills in decision info, and either **Saves as Draft** or **Issues** it.
7. On issue, in a single transaction: the system closes the employee's previous `HrEmployeeSalary` record (`EffectiveTo` set) and creates a new one linked to the decision.
8. HR Staff or Approver can look up the result later in **Employee Salary History**, which shows the full timeline and links back to the decision.

## 7. Deployment View

This is the **planned** deployment:

- **HRM Web Application** — deployed as a static Angular build, served via a web server (e.g. IIS or a CDN).
- **HRM Backend API** — deployed as an ASP.NET Core application (e.g. IIS or a container), reachable by the Web Application over HTTPS.
- **HRM Database** — a SQL Server instance, reachable only by the Backend API (not directly by the Web Application).

## 8. Cross-cutting Concepts

- **Audit fields**: every table has `CreatedAt`/`UpdatedAt` (see [Database Design](../README.md#database-design)).
- **Soft status instead of hard delete**: `Status` fields (e.g. `DRAFT`, `APPROVED`, `CANCELLED`) are used everywhere instead of deleting rows, so history is never lost.
- **Effective-dated records**: `EffectiveFrom`/`EffectiveTo` pattern is reused across `HrSalaryScale`, `HrSalaryGrade`, and `HrEmployeeSalary` to answer "what was true at date X".
- **Snapshot values**: coefficients are copied (snapshotted) into review and decision tables so historical numbers don't change if master data is edited later.
- **Consistent UX rules**: status badges, confirmation dialogs for sensitive actions (issuing a decision), pagination for large lists — documented in the [Wireframe document](../HRM_Salary_Grade_Promotion_Wireframe_UIUX_EN.docx).

## 9. Architecture Decisions

### ADR-01: Separate Frontend and Backend

**Decision**

The Angular frontend communicates with the ASP.NET Core backend through
REST APIs.

**Rationale**

This separates user interface concerns from business logic and allows
the frontend and backend to evolve more independently.

### ADR-02: Preserve Salary History

**Decision**

The system does not overwrite the previous salary record when an
employee moves to a new salary grade. Instead, the previous record is
closed and a new salary record is created.

**Rationale**

This allows the system to preserve and retrieve the employee's salary
history.

## 10. Quality Requirements

### 10.1 Maintainability

Employee, Salary, Allowance, and Attendance responsibilities are
separated into clear business components to make the system easier to
maintain and extend.

### 10.2 Data Integrity

Important changes such as salary and allowance updates should only
become official according to the corresponding business process and
approval state.

### 10.3 Traceability

The system should preserve the history of important changes,
particularly employee salary history and related HR decisions.

Specific quantitative quality targets have not yet been defined within
the current project scope.

## 11. Risks and Technical Debt

Potential architectural risks include:

-   Approval workflows may become more complex if multiple approval
    levels are introduced.
-   Salary policies and promotion eligibility rules may change over
    time.
-   Attendance may introduce additional dependencies if the system scope
    later expands to full payroll calculation.

Specific technical debt items will be documented as the system evolves.

## 12. Glossary

| Term | Meaning |
|---|---|
| Review Period | A time-boxed cycle (e.g. "Annual Review H1 2026") during which employees are reviewed for salary grade promotion. |
| Salary Scale | A named group of salary grades (e.g. "Scale A - Technical"). |
| Salary Grade | One step within a salary scale, with a coefficient (e.g. Grade 3.2, coefficient 3.66). |
| Coefficient | The number used to calculate actual salary from the base salary. |
| Eligibility | Whether an employee currently qualifies for a grade review (e.g. minimum time in current grade). |
| Salary Decision | The official, signed document that changes one or more employees' salary grade. |
| Proposal | A suggested new grade for an employee within a review period, not yet official until a decision is issued. |
