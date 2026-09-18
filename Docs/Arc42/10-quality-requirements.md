# 10. Quality Requirements

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System.*

> **Status:** Draft (targets not yet measured) · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-18 · **Implementation Baseline Commit:** `77e5716`

> The backend is implemented (see the baseline commit above), but none of the success measures below have been measured: there is no deployment, no load test, and no health-check endpoint yet. All are example/draft targets pending confirmation with the business — tracked collectively as RISK-09. Automated tests (unit + HTTP-level) cover the business-rule rejection behind QS-03; nothing yet covers QS-02 or QS-05.

| ID | Quality Goal | Scenario | Environment | Example Success Measure *(draft)* |
|---|---|---|---|---|
| QS-01 | `#secure` | An auditor asks what grade Employee X had on a past date (see [Section 6.3](06-runtime-view.md#63-quality-goal-scenario--auditing-a-past-salary)). | Normal | 100% of historical queries return a result consistent with the decision log; zero salary history rows are ever overwritten. |
| QS-02 | `#usable` | HR Staff opens a review period with 150+ employees and needs to screen all of them. | Peak (large annual review) | HR Staff can complete screening within a single working session; bulk actions process at least 50 employees per request. |
| QS-03 | `#suitable` | HR Staff tries to submit a review period while some employees are still unprocessed (see [US-SGP-05](../Requirements/UserStories_SalaryGradePromotion.md#us-sgp-05----submit-review-period)). | Normal | The system blocks the submission and lists what's missing; 100% of invalid actions are rejected, never silently accepted. |
| QS-04 | `#flexible` | A future Allowance Management feature is added under Salary Grade Promotion. | Normal (development-time) | Zero changes to the source files/tables of the other three components (Employee Management, Organization Management, Salary Master Data) are required. |
| QS-05 | `#operable` | The Backend API needs to be restarted or scaled during a review period. | Degraded / maintenance | Health-check endpoint reports readiness; restarting one instance does not lose already-persisted data; recovery takes under 1 minute. |
