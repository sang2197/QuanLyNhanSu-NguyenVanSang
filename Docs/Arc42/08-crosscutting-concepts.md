# 8. Crosscutting Concepts

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System.*

> **Status:** Current (§8.2 Security: Proposed, not implemented) · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-18 · **Implementation Baseline Commit:** `77e5716`

## 8.1 Audit & History

- **Audit fields**: mutable/current-state tables have `CreatedAt`/`UpdatedAt`; append-only history tables (`HrBaseSalaryRate`, `HrSalaryGradeCoefficient`, `HrEmployeeSalary`) have only `CreatedAt`, since a row is never updated after insert (see [Database Design](../Database/README.md)).
- **Soft status instead of hard delete**: `Status` fields (e.g. `DRAFT`, `APPLIED`, `CANCELLED`) are used everywhere instead of deleting rows, so history is never lost.
- **Effective-dated records**: a single `EffectiveDate` per row (not an `EffectiveFrom`/`EffectiveTo` range) is used across `HrBaseSalaryRate`, `HrSalaryGradeCoefficient`, and `HrEmployeeSalary` — a value is open-ended until superseded by the next row with a later `EffectiveDate` for the same grade/employee, answering "what was true at date X" by picking the row with the latest `EffectiveDate` on or before X.
- **Snapshot values**: coefficients are copied (snapshotted) into `HrSalaryReviewEmployee` and `HrSalaryDecisionDetail` so historical numbers don't change if master data is edited later.
- **Consistent UX rules**: status badges, confirmation dialogs for sensitive actions (issuing a decision), pagination for large lists — documented in [`UXGuidelines_HRM.md`](../UI-UX/UXGuidelines_HRM.md) and [`Wireframe_SalaryGradePromotion.md`](../UI-UX/Wireframe_SalaryGradePromotion.md).

## 8.2 Security

> This subsection proposes a mechanism to close a previously open gap (see [RISK-05](11-risks-and-technical-debt.md)). **It is not implemented**: the current backend has no authentication or authorization (no auth middleware, no `[Authorize]` on any endpoint), and it has not been confirmed with the full team — see [ADR-07](09-architecture-decisions.md#adr-07-authentication-and-authorization-mechanism). Until then, every endpoint is callable without credentials and no actor identity is recorded on salary changes.

- **Authentication**: users log in through ASP.NET Core Identity, which issues a JWT bearer token. The Web Application attaches this token to every Backend API request.
- **Authorization**: role-based access control. HR Staff and Approver / Manager are distinct roles; each API endpoint enforces which role(s) may call it (e.g. only Approver / Manager may draft/apply a decision — see [User Stories](../Requirements/UserStories_SalaryGradePromotion.md)). Enforced in the API, not just hidden in the UI.
- **Data protection in transit**: all traffic uses HTTPS (see [Section 7 TLS termination](07-deployment-view.md)).
- **Data protection at rest**: sensitive salary data is protected using SQL Server's standard encryption-at-rest (Transparent Data Encryption) — *pending confirmation once real infrastructure/hosting is chosen.*
- **Non-repudiation**: tying every salary change to a `HrSalaryDecision` ([Section 8.1](#81-audit--history)) gives an answer to "who changed what, under which official decision."
