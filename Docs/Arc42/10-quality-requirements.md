# 10. Quality Requirements

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System (Salary Grade Promotion).*

> No implementation exists yet, so none of the success measures below have been measured in production. All are example/draft targets pending confirmation with the business — tracked collectively as RISK-09.

| ID | Quality Goal | Scenario | Environment | Example Success Measure *(draft)* |
|---|---|---|---|---|
| QS-01 | `#secure` | An auditor asks what grade Employee X had on a past date (see [Section 6.3](06-runtime-view.md#63-quality-goal-scenario--auditing-a-past-salary)). | Normal | 100% of historical queries return a result consistent with the decision log; zero salary history rows are ever overwritten. |
| QS-02 | `#usable` | HR Staff opens a review period with 150+ employees and needs to screen all of them. | Peak (large annual review) | HR Staff can complete screening within a single working session; bulk actions process at least 50 employees per request. |
| QS-03 | `#suitable` | HR Staff tries to submit a review period while some employees are still unprocessed (see [US-05](../Requirements/UserStories_SalaryGradePromotion.md#us-05-submit-a-review-period-to-the-approver)). | Normal | The system blocks the submission and lists what's missing; 100% of invalid actions are rejected, never silently accepted. |
| QS-04 | `#flexible` | A future Allowance Management feature is added under Salary Management. | Normal (development-time) | Zero changes to Employee Management source files/tables are required. |
| QS-05 | `#operable` | The Backend API needs to be restarted or scaled during a review period. | Degraded / maintenance | Health-check endpoint reports readiness; restarting one instance does not lose already-persisted data; recovery takes under 1 minute. |
