# 1. Introduction and Goals

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System (Salary Grade Promotion). Follows the [arc42](https://arc42.org/) template.*

**Detail level:** ESSENTIAL. The project has requirements, C4 diagrams, and a database design, but no implementation yet — so some sections (e.g. [07-deployment-view](07-deployment-view.md), [10-quality-requirements](10-quality-requirements.md)) contain **draft/example values pending confirmation**, clearly labeled where they appear.

## 1.1 Overview

The HRM System supports core human resource management processes within
an organization.

**Scope of this document:** Only the *Salary Grade Promotion* feature, part of the *Salary Management* function group. The HRM system has 5 other function groups (Employee Profile, Department/Unit, Reward/Discipline, Attendance, Contract) that are out of scope here — see the [mind map](../Requirements/Quản%20lý%20nhân%20sự.xmind).

The system is primarily used by HR Staff and authorized Approvers.

## 1.2 Quality Goals

| Priority | Quality Goal | Q42 Tag | Why it matters | Example Success Measure *(draft — confirm with business)* |
|---|---|---|---|---|
| 1 | Data integrity / auditability | `#secure` | Salary history must never be overwritten silently; every change must be traceable to a decision. | 100% of salary changes are traceable to a decision; zero salary history rows are ever overwritten. |
| 2 | Usability for bulk processing | `#usable` | HR Staff may screen hundreds of employees in one review period; filtering and bulk actions are required. | HR Staff can screen 150+ employees in a single sitting; bulk actions process at least 50 employees per request. |
| 3 | Correctness of business rules | `#suitable` | Eligibility, approval, and submission rules must be enforced consistently, not just in the UI. | 100% of approve/reject/submit/apply actions are validated against the rules in [User Stories](../Requirements/UserStories_SalaryGradePromotion.md) before persisting. |
| 4 | Maintainability | `#flexible` | The backend should stay easy to extend as more Salary Management features (e.g. Allowance Management) are added later. | Adding a feature under Salary Management requires zero changes to Employee Management source files. |
| 5 | Operability | `#operable` | The system must be observable and safely restartable once deployed, without manual, error-prone steps. | Backend API exposes a health-check endpoint; instances can be restarted without losing already-persisted data. |

## 1.3 Stakeholders

**HR Staff**

Performs day-to-day HR operations and manages employee-related
information and processes.

**Approver**

Reviews HR requests and makes final decisions when approval is required.
