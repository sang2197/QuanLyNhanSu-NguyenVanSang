# 2. Architecture Constraints

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System.*

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-18 · **Implementation Baseline Commit:** `77e5716`

> An earlier version of this section listed the technology stack as a constraint. Applying the constraint test ("would overriding this require a business/organizational change, or could a developer simply change it?") — no external mandate for this stack has been documented anywhere in the project, so it is a **design decision**, not a constraint. It has been moved to [ADR-06](09-architecture-decisions.md#adr-06-technology-stack-angular--aspnet-core--sql-server).

## 2.1 Business Constraints

- **Approval before official change** — salary grade promotion must go through a review and approval process before becoming official data. *Reason: prevents unilateral, unaudited salary changes — a payroll-integrity and compliance requirement.*
- **Preserve historical data** — historical salary information and HR decisions must be preserved rather than overwritten. *Reason: required to answer "what was true at date X" for audits and payroll disputes — supports the `#secure` quality goal from [Section 1.2](01-introduction-and-goals.md#12-quality-goals).*

No technical, organizational, or regulatory constraints beyond these have been identified yet.
