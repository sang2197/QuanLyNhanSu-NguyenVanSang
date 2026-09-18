# 1. Introduction and Goals

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System. Follows the [arc42](https://arc42.org/) template.*

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-18 · **Implementation Baseline Commit:** `77e5716`

**Detail level:** ESSENTIAL. The project has requirements, UI/UX design, C4 diagrams, database and API design, detailed design, and a working ASP.NET Core backend for all four modules with automated tests (the React frontend is design-only, not yet implemented). Sections that describe things that do not exist yet — [07-deployment-view](07-deployment-view.md) (no real infrastructure) and [10-quality-requirements](10-quality-requirements.md) (targets not yet measured) — still contain **draft/example values pending confirmation**, clearly labeled where they appear.

## 1.1 Overview

The HRM System supports core human resource management processes within
an organization.

**Scope of this document:** Four modules — *Employee Profile* (Employee Management), *Department/Unit* (Organization Management), and, within the *Salary Management* function group, *Salary Master Data* and *Salary Grade Promotion*. The remaining function groups of the HRM mind map (Reward/Discipline, Attendance, Contract) are out of scope here and not yet started — see the [mind map](../Requirements/Quản%20lý%20nhân%20sự.xmind).

The system is primarily used by HR Staff and authorized Approvers.

## 1.2 Quality Goals

| Priority | Quality Goal | Q42 Tag | Why it matters | Example Success Measure *(draft — confirm with business)* |
|---|---|---|---|---|
| 1 | Data integrity / auditability | `#secure` | Salary history must never be overwritten silently; every change must be traceable to a decision. | 100% of salary changes are traceable to a decision; zero salary history rows are ever overwritten. |
| 2 | Usability for bulk processing | `#usable` | HR Staff may screen hundreds of employees in one review period; filtering and bulk actions are required. | HR Staff can screen 150+ employees in a single sitting; bulk actions process at least 50 employees per request. |
| 3 | Correctness of business rules | `#suitable` | Eligibility, approval, and submission rules must be enforced consistently, not just in the UI. | 100% of approve/reject/submit/apply actions are validated against the rules in [User Stories](../Requirements/UserStories_SalaryGradePromotion.md) before persisting. |
| 4 | Maintainability | `#flexible` | The backend should stay easy to extend as more Salary Grade Promotion features (e.g. Allowance Management) are added later. | Adding a feature under Salary Grade Promotion requires zero changes to the other three components' source files. |
| 5 | Operability | `#operable` | The system must be observable and safely restartable once deployed, without manual, error-prone steps. | Backend API exposes a health-check endpoint *(not yet implemented — see [DEBT-01](11-risks-and-technical-debt.md#technical-debt))*; instances can be restarted without losing already-persisted data. |

## 1.3 Stakeholders

**HR Staff**

Performs day-to-day HR operations and manages employee-related
information and processes.

**Approver / Manager**

Reviews HR requests and makes final decisions when approval is required.
