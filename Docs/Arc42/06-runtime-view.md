# 6. Runtime View

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System.*

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-18 · **Implementation Baseline Commit:** `77e5716`

Three scenarios, covering the happy path, an error/recovery case, and a scenario demonstrating the `#secure` quality goal. All steps are handled by the **Salary Grade Promotion** component inside the Backend API (see [Building Block View](05-building-block-view.md)), except where noted.

## 6.1 Happy path — Process a review period end-to-end

1. HR Staff creates a **Review Period** (e.g. "Annual Review H1 2026"). The system automatically works out a proposed new grade for each eligible employee in it, reading active employees from Employee Management and the next active grade from Salary Master Data (see the eligibility rule in [US-SGP-03](../Requirements/UserStories_SalaryGradePromotion.md#us-sgp-03----view-employees-and-proposed-grades)).
2. HR Staff opens the **Review Period Detail** screen, filters/searches employees, and reviews each employee's system-calculated proposed grade.
3. HR Staff marks each employee's proposal as **Approved** or **Not Approved** — one at a time or in bulk (a reason is recorded if not approved).
4. Once every employee in the period has been marked, HR Staff **submits the period** to the Approver.
5. The Approver reviews the submitted period and **drafts a salary decision** from the approved employees — either directly from the period (which pre-selects it) or from the salary decision list, picking the period there instead (see [US-SGP-09](../Requirements/UserStories_SalaryGradePromotion.md#us-sgp-09----view-and-resume-salary-decisions)).
6. The Approver **applies the decision**. In a single transaction: the system adds a new `HrEmployeeSalary` record for each included employee, linked to the decision; earlier records are never modified — the new row supersedes them by its later `EffectiveDate` (see [Section 8.1](08-crosscutting-concepts.md#81-audit--history)). The decision becomes Applied and the review period Closed in the same transaction.
7. HR Staff or the Approver can look up the result later in **Employee Salary History**, which shows the full timeline and links back to the decision.

## 6.2 Error/recovery — Applying a decision fails partway

1. The Approver applies a decision covering 20 employees.
2. While validating, the system finds that one employee's current salary grade no longer matches the grade captured when the decision was created (a conflict — the employee's salary changed in the meantime) — see [US-SGP-07](../Requirements/UserStories_SalaryGradePromotion.md#us-sgp-07----apply-salary-decision).
3. Per the all-or-nothing rule, the transaction is rolled back: **none** of the 20 employees are updated, not just the conflicting one.
4. The Approver sees an error identifying the conflicting employee and the reason.
5. The Approver removes that employee from the decision (or resolves the conflict) and re-applies.

## 6.3 Quality-goal scenario — Auditing a past salary

1. An auditor (via HR Staff or the Approver) asks: "What was Employee X's grade on 2024-06-01?"
2. HR Staff/Approver opens **Employee Salary History** for Employee X.
3. The system returns the exact effective-dated row covering 2024-06-01, together with the decision that caused it.
4. This demonstrates the `#secure` / auditability quality goal ([Section 1.2](01-introduction-and-goals.md#12-quality-goals), goal 1) in practice.
