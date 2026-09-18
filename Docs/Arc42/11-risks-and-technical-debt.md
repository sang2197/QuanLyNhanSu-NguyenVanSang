# 11. Risks and Technical Debt

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System.*

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-18 · **Implementation Baseline Commit:** `77e5716`

## Risk Register

Ordered Critical → High → Medium → Low. **Status:** *Open* (not yet addressed), *Mitigated* (a mitigation is in place in the implementation; residual notes given).

| ID | Priority | Status | Risk | Related | Mitigation |
|---|---|---|---|---|---|
| RISK-08 | Critical | Mitigated | The eligibility rule (24 months in grade, next grade must exist in scale, no duplicate proposal — [US-SGP-03](../Requirements/UserStories_SalaryGradePromotion.md#us-sgp-03----view-employees-and-proposed-grades)) must be enforced by the system, not by the UI alone. | [Section 8.1](08-crosscutting-concepts.md#81-audit--history) | Enforced in the Application layer by `SalaryPromotionEligibilityRule` (24 whole months as of the review date; a next active grade must exist) and covered by unit tests. Duplicate proposals are prevented by a unique index on `HrSalaryReviewEmployee` (`ReviewPeriodId`, `EmployeeId`). Residual: the 24-month rule cannot be a plain database constraint, and the unique index is not exercised by the current tests (see [DEBT-02](#technical-debt)). |
| RISK-05 | High | Open | The role model is not finalized — e.g. whether one person can hold both HR Staff and Approver / Manager roles — blocking [ADR-07](09-architecture-decisions.md#adr-07-authentication-and-authorization-mechanism) implementation. As a result the backend currently has **no authentication or authorization**: every endpoint is open and no actor identity (who approved or applied a decision) is recorded. | ADR-07 | Finalize the role model with the business before implementing authentication. Until then, do not expose the API outside a trusted network. |
| RISK-01 | Medium | Open | Frontend/backend API contract changes could break the Web Application if not versioned. The API currently has no version prefix (see [DEBT-05](#technical-debt)). | ADR-01 | Adopt API versioning conventions before the first breaking change. |
| RISK-04 | Medium | Open | SQL Server licensing cost may not fit the final budget or hosting environment. | ADR-06 | Confirm licensing/hosting budget before committing to SQL Server in production. |
| RISK-06 | Medium | Open | Approval workflows may become more complex if multiple approval levels are introduced. | [Section 6](06-runtime-view.md) | Revisit the Runtime View and User Stories if multi-level approval is requested. |
| RISK-07 | Medium | Mitigated | Salary policies and promotion eligibility rules may change over time. | US-SGP-03 | The eligibility rule lives in one class (`SalaryPromotionEligibilityRule`), so it is easy to update. Residual: the 24-month threshold is a code constant, not a configuration value. |
| RISK-10 | Medium | Open | Switching from Angular to React ([ADR-08](09-architecture-decisions.md#adr-08-switch-frontend-framework-to-react)) means the team must still select and agree on supporting libraries (router, state management, HTTP client) that Angular would have provided out of the box. | ADR-08 | Decide on the frontend library set before implementation starts, as part of the React folder-structure design. |
| RISK-02 | Low | Open | `HrEmployeeSalary` (and similar history tables) grow without bound over time. | ADR-02 | Plan an archiving strategy once real data volume is known. |
| RISK-03 | Low | Open | Splitting into four components may add unnecessary complexity if Salary Grade Promotion's scope doesn't grow as expected. The split already produced one genuine circular dependency (Employee Management ↔ Organization Management) that needed a `Lazy<T>` workaround ([DEBT-04](#technical-debt)). | ADR-03 | Revisit if Salary Grade Promotion stays simple long-term. |
| RISK-09 | Low | Open | None of the [Section 10](10-quality-requirements.md) quality targets have been measured yet: the backend exists but has not been deployed or load-tested, and there is no health-check endpoint ([DEBT-01](#technical-debt)). | Section 10 | Re-validate all QS-xx targets once the first deployment exists. |

## Technical Debt

Debt observed in the current implementation (baseline commit above), verified against the code.

| ID | Debt | Related | Suggested resolution |
|---|---|---|---|
| DEBT-01 | **No health-check endpoint or structured logging.** `Program.cs` registers neither; only the default ASP.NET Core logging is active. | `#operable` goal, QS-05, [Section 4](04-solution-strategy.md), [Section 7](07-deployment-view.md) | Add a health-check endpoint (including a database check) and structured logging before the first deployment. |
| DEBT-02 | **No automated test runs against SQL Server.** `HRM.Api.Tests` use the EF Core InMemory provider, which does not enforce unique/filtered indexes and has no real transactions; `UnitOfWork` skips the explicit transaction on non-relational providers. Atomicity of `CreateReviewPeriod` / `ApplyDecision` is therefore verified only through a mocked `IUnitOfWork`. | RISK-08, ADR-02, [Runtime View 6.2](06-runtime-view.md) | Add a small integration suite against a real SQL Server (LocalDB or a container) covering the database constraints and the two transactional flows. |
| DEBT-03 | **Generated Swagger omits error responses.** `openapi.yaml` documents 400/404/409 with the `Error` schema per endpoint, but controllers only declare `[ProducesResponseType]` for success codes; `ExceptionHandlingMiddleware` does return the correct errors at runtime. | `openapi.yaml` | Declare the error response types on the controller actions so the generated Swagger matches the contract. |
| DEBT-04 | **Two `Lazy<T>` dependency-injection workarounds:** `OrganizationalUnitService` ↔ `EmployeeService` (a genuine cycle) and `SalaryGradeService` → `ISalaryHistoryService` (one-way, kept lazy for consistency). | ADR-03, RISK-03 | Revisit if the Employee ↔ Organization coupling grows; a narrower shared interface could remove the cycle. |
| DEBT-05 | **No API version prefix.** Routes are served from the root (e.g. `/review-periods`), while `openapi.yaml` lists the placeholder server `https://api.example.com/v1`. | RISK-01 | Decide the versioning scheme before the first breaking change and align the spec's server URL with it. |
