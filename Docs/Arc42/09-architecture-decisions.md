# 9. Architecture Decisions

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System.*

> **Status:** Current (ADR-07: Proposed, not implemented) · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-18 · **Implementation Baseline Commit:** `77e5716`

## ADR-01: Separate Frontend and Backend

**Status:** Accepted

**Context**

The system needs a web UI usable by both HR Staff and Approver / Manager, calling a shared business-logic layer that could later support other clients (e.g. a future mobile app or integration).

**Decision**

The React frontend communicates with the ASP.NET Core backend through REST APIs.

**Implementation:** Backend REST API implemented (49 endpoints, matching `openapi.yaml`). The React frontend is design-only — no frontend code exists yet.

**Consequences**

- *Positive:* UI and business logic evolve independently; the API can be reused by future clients.
- *Negative:* Adds network latency and API-versioning overhead compared to a single server-rendered app.

**Risks created:** RISK-01

---

## ADR-02: Preserve Salary History

**Status:** Accepted

**Context**

Salary changes must be auditable and reconstructable at any past date (supports the `#secure` quality goal); overwriting a salary field in place would destroy this trail.

**Decision**

The system does not overwrite the previous salary record when an employee moves to a new salary grade. Instead, a new salary record with a later effective date is added; earlier records are left untouched and are superseded by effective date (see [Section 8.1](08-crosscutting-concepts.md#81-audit--history)).

**Implementation:** Implemented — applying a salary decision only inserts `HrEmployeeSalary` rows; no code path updates or deletes existing ones.

**Consequences**

- *Positive:* Full audit trail; supports "what was true at date X" queries.
- *Negative:* The history table grows without bound over time; every "current salary" read must filter by effective date instead of reading a single field.

**Risks created:** RISK-02

---

## ADR-03: Split the Backend by Business Domain

**Status:** Accepted

**Context**

Salary Grade Promotion's scope is expected to grow (e.g. a future Allowance Management feature), while Employee Management, Organization Management, and Salary Master Data are comparatively stable reference/core data, each owned by its own module per the requirements analysis.

**Decision**

Inside the Backend API, `Employee Management`, `Organization Management`, `Salary Master Data`, and `Salary Grade Promotion` are implemented as four separate components, split by business domain rather than by technical layer.

**Update:** `Contract Management` is added to the C4 component design as a fifth component under the same rule. It is designed (requirements and UI/UX) but not yet implemented, so the implementation notes below describe the four implemented components.

**Implementation:** All four components are implemented in `backend/`. The domain split is mirrored as `EmployeeManagement/`, `OrganizationManagement/`, `SalaryMasterData/`, and `SalaryGradePromotion/` subfolders inside `HRM.Application`, `HRM.Infrastructure`, and `HRM.Api` (entities stay flat in `HRM.Domain`, since several are read across domains). The four components are co-deployed in one process ([Section 7](07-deployment-view.md)). Cross-domain access is an in-process call to the other domain's **Service interface**, never to its repository. Two dependencies are resolved with `Lazy<T>` injection: the genuine cycle `OrganizationalUnitService` ↔ `EmployeeService`, and `SalaryGradeService` → `ISalaryHistoryService` (one-way, kept lazy for consistency) — see [DEBT-04](11-risks-and-technical-debt.md#technical-debt).

**Consequences**

- *Positive:* Each component can evolve independently; smaller, more focused codebases.
- *Negative:* Any cross-domain read (e.g. Salary Grade Promotion showing an employee's Organizational Unit) requires a call to another component's Service instead of a single local query; the Employee ↔ Organization mutual dependency needed a `Lazy<T>` workaround.

**Risks created:** RISK-03

---

## ADR-04: Layered Design inside Each Backend Component

**Status:** Accepted

**Context**

Business rules (eligibility, approval, effective-dating) need one clear home so they aren't duplicated or applied inconsistently across entry points.

**Decision**

Within each of the four components, requests flow through a Controller, then a Service, then one or more Repositories (e.g. `ReviewPeriodsController` → `ReviewPeriodService` → `ReviewPeriodRepository`). Repositories are split per aggregate (10 in total). Service and Repository interfaces are defined in `HRM.Application` and implemented in `HRM.Infrastructure` (dependency inversion). Services that must write across several repositories atomically (`CreateReviewPeriod`, `ApplyDecision`) use the Application-owned `IUnitOfWork` abstraction instead of referencing `HrmDbContext` directly.

**Implementation:** Implemented for all four components.

**Consequences**

- *Positive:* Business logic is centralized in the Service layer; easier to test in isolation.
- *Negative:* Adds boilerplate/indirection for simple pass-through operations.

**Risks created:** None — layering overhead is an accepted, well-understood tradeoff, not tracked as a project risk.

---

## ADR-05: Separate Proposal from Official Decision

**Status:** Accepted

**Context**

HR Staff and the Approver / Manager need to redo or reject proposals freely during a review period without any risk of accidentally changing real payroll data.

**Decision**

A review result (a proposed grade an employee is screened against) is stored separately from an official salary decision. Only an issued decision changes an employee's real salary.

**Implementation:** Implemented — review results live in `HrSalaryReviewEmployee`, official decisions in `HrSalaryDecision`/`HrSalaryDecisionDetail`, and only `ApplyDecision` writes `HrEmployeeSalary`.

**Consequences**

- *Positive:* Safe experimentation during review; real salary only changes at a well-defined, auditable moment.
- *Negative:* Two parallel data shapes (review/proposal vs. official decision) must be kept conceptually in sync, adding some duplication of employee/grade references.

**Risks created:** None — the duplication is an accepted tradeoff for safety.

---

## ADR-06: Technology Stack (Angular + ASP.NET Core + SQL Server)

**Status:** Partially Superseded — the frontend framework choice (Angular) is superseded by [ADR-08](#adr-08-switch-frontend-framework-to-react); the backend (ASP.NET Core) and database (SQL Server) choices below remain Accepted. Kept here as the historical record.

**Context**

This stack was chosen early in the C4 modeling phase (see [`Docs/c4/`](../c4/README.md)), before this ADR was written. It is recorded here retroactively so the decision is traceable and can be reconsidered. No externally-imposed mandate (e.g. a company-wide technology policy) has been documented for this choice.

**Decision**

Use Angular for the frontend, ASP.NET Core for the backend API, and SQL Server for the database.

**Implementation:** Backend on ASP.NET Core (.NET 8) with EF Core and the SQL Server provider is implemented. The Angular part was superseded by ADR-08.

**Consequences**

- *Positive:* Mature, well-supported ecosystem; consistent tooling across the stack; matches what is already reflected in the C4 diagrams.
- *Negative:* Locks the project into the Microsoft/.NET ecosystem (e.g. SQL Server licensing cost); Angular has a steeper learning curve than some lighter frontend frameworks.

**Risks created:** RISK-04

---

## ADR-07: Authentication and Authorization Mechanism

**Status:** Proposed — **not implemented** *(not confirmed with the full team; the backend currently has no authentication or authorization, and `openapi.yaml` declares the `bearerAuth` scheme for documentation only)*

**Context**

The system manages sensitive salary data and has 2 distinct roles (HR Staff, Approver / Manager) with different permitted actions (see [User Stories](../Requirements/UserStories_SalaryGradePromotion.md)). Before this ADR, no authentication/authorization mechanism had been decided — this was tracked as an open gap (RISK-05).

**Decision**

Use ASP.NET Core Identity to issue JWT bearer tokens after login. The Web Application attaches the token to every Backend API request. The Backend API enforces role-based access control (HR Staff vs. Approver / Manager) on each endpoint.

**Consequences**

- *Positive:* Closes a previously identified security gap; role restrictions are enforced centrally in the API rather than trusted to the frontend.
- *Negative:* Adds a token issuance/refresh flow to build; the role model needs to be finalized (e.g. can one person hold both roles?) before implementation.

**Risks created:** RISK-05

---

## ADR-08: Switch Frontend Framework to React

**Status:** Accepted — supersedes the frontend framework choice in [ADR-06](#adr-06-technology-stack-angular--aspnet-core--sql-server)

**Context**

ADR-06 originally chose Angular for the frontend, before any frontend implementation began. The team decided to use React instead. This does not affect ADR-01 (frontend/backend separation via REST API) or the backend/database choices in ADR-06 (ASP.NET Core, SQL Server), which remain unchanged.

**Decision**

Use React (instead of Angular) for the `HRM Web Application` frontend.

**Implementation:** Design-only — the React folder structure is designed in [`Docs/CodeStructure/FrontendStructure.md`](../CodeStructure/FrontendStructure.md); no frontend code exists yet.

**Consequences**

- *Positive:* Larger talent pool and ecosystem for React; being a lighter-weight library rather than a full framework, the team can pick only the tooling this project actually needs (routing, state management) instead of adopting Angular's full opinionated toolset.
- *Negative:* Loses Angular's batteries-included structure (built-in dependency injection, forms, routing) — the team must select and standardize equivalent libraries themselves.

**Risks created:** RISK-10
