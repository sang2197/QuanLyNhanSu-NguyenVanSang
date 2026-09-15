# 11. Risks and Technical Debt

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System (Salary Grade Promotion).*

## Risk Register

Ordered Critical → High → Medium → Low.

| ID | Priority | Risk | Related | Mitigation |
|---|---|---|---|---|
| RISK-08 | Critical | The eligibility rule (24 months in grade, next grade must exist in scale, no duplicate proposal — [US-03](../Requirements/UserStories_SalaryGradePromotion.md#us-03-view-employees-and-their-proposed-grade-in-a-review-period)) is defined at the requirements level but not yet enforced as a database constraint or validation rule. | [Section 8.1](08-crosscutting-concepts.md#81-audit--history) | Add a database constraint or application-level validation before go-live; do not rely on the UI alone. |
| RISK-05 | High | The role model is not finalized — e.g. whether one person can hold both HR Staff and Approver roles — blocking [ADR-07](09-architecture-decisions.md#adr-07-authentication-and-authorization-mechanism) implementation. | ADR-07 | Finalize the role model with the business before implementing authentication. |
| RISK-01 | Medium | Frontend/backend API contract changes could break the Web Application if not versioned. | ADR-01 | Adopt API versioning conventions before the first breaking change. |
| RISK-04 | Medium | SQL Server licensing cost may not fit the final budget or hosting environment. | ADR-06 | Confirm licensing/hosting budget before committing to SQL Server in production. |
| RISK-06 | Medium | Approval workflows may become more complex if multiple approval levels are introduced. | [Section 6](06-runtime-view.md) | Revisit the Runtime View and User Stories if multi-level approval is requested. |
| RISK-07 | Medium | Salary policies and promotion eligibility rules may change over time. | US-03 | Keep eligibility rules in one place ([Section 8.1](08-crosscutting-concepts.md#81-audit--history) / Salary Management service) so they're easy to update. |
| RISK-02 | Low | `HrEmployeeSalary` (and similar history tables) grow without bound over time. | ADR-02 | Plan an archiving strategy once real data volume is known. |
| RISK-03 | Low | Splitting Employee/Salary Management may add unnecessary complexity if Salary Management's scope doesn't grow as expected. | ADR-03 | Revisit if Salary Management stays simple long-term. |
| RISK-09 | Low | None of the [Section 10](10-quality-requirements.md) quality targets have been measured yet, since no implementation exists. | Section 10 | Re-validate all QS-xx targets once the first implementation is deployed. |

## Technical Debt

No technical debt has accumulated yet, since no implementation exists. This section will begin tracking `DEBT-xx` items once development starts.
