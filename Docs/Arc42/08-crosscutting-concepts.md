# 8. Crosscutting Concepts

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System (Salary Grade Promotion).*

## 8.1 Audit & History

- **Audit fields**: every table has `CreatedAt`/`UpdatedAt` (see [Database Design](../Database/README.md)).
- **Soft status instead of hard delete**: `Status` fields (e.g. `DRAFT`, `APPROVED`, `CANCELLED`) are used everywhere instead of deleting rows, so history is never lost.
- **Effective-dated records**: `EffectiveFrom`/`EffectiveTo` pattern is reused across `HrSalaryScale`, `HrSalaryGrade`, and `HrEmployeeSalary` to answer "what was true at date X".
- **Snapshot values**: coefficients are copied (snapshotted) into review and decision tables so historical numbers don't change if master data is edited later.
- **Consistent UX rules**: status badges, confirmation dialogs for sensitive actions (issuing a decision), pagination for large lists — documented in the [Wireframe document](../UI-UX/HRM_Salary_Grade_Promotion_Wireframe_UIUX_EN.docx).

## 8.2 Security

> This subsection proposes a mechanism to close a previously open gap (see [RISK-05](11-risks-and-technical-debt.md)); it has not been implemented or confirmed with the full team yet — see [ADR-07](09-architecture-decisions.md#adr-07-authentication-and-authorization-mechanism).

- **Authentication**: users log in through ASP.NET Core Identity, which issues a JWT bearer token. The Web Application attaches this token to every Backend API request.
- **Authorization**: role-based access control. HR Staff and Approver are distinct roles; each API endpoint enforces which role(s) may call it (e.g. only Approver may draft/apply a decision — see [User Stories](../Requirements/UserStories_SalaryGradePromotion.md)). Enforced in the API, not just hidden in the UI.
- **Data protection in transit**: all traffic uses HTTPS (see [Section 7 TLS termination](07-deployment-view.md)).
- **Data protection at rest**: sensitive salary data is protected using SQL Server's standard encryption-at-rest (Transparent Data Encryption) — *pending confirmation once real infrastructure/hosting is chosen.*
- **Non-repudiation**: tying every salary change to a `HrSalaryDecision` ([Section 8.1](#81-audit--history)) gives an answer to "who changed what, under which official decision."
