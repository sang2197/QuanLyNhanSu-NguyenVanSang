# Backend

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-22 · **Implementation Baseline Commit:** `77e5716`

ASP.NET Core (.NET 8) implementation of the full HRM system, built from [`Docs/`](../Docs/README.md) — see [`Docs/CodeStructure/BackendStructure.md`](../Docs/CodeStructure/BackendStructure.md) for the folder-structure rationale and [`Docs/API/openapi.yaml`](../Docs/API/openapi.yaml) for the API contract. Covers all 5 modules: Employee Management, Organization Management, Salary Master Data, Salary Grade Promotion, Contract Management (13 tables, 57 endpoints across 11 tags).

## Architecture

Clean/Onion Architecture, one direction of dependency:

```
HRM.Domain → HRM.Application → HRM.Infrastructure → HRM.Api
```

- **HRM.Domain** — entities and enums only, no dependencies.
- **HRM.Application** — business rules (`Services/`), the interfaces Infrastructure implements (`Interfaces/`), and small composite models the entities alone don't cover (`Models/`). No EF Core reference — repositories are consumed through `IQueryable<T>` with plain synchronous LINQ.
- **HRM.Infrastructure** — `HrmDbContext`, EF Core configurations, repository implementations, `UnitOfWork`.
- **HRM.Api** — controllers, request/response DTOs, mapping extensions, `ExceptionHandlingMiddleware`.

Each layer is split into the same 5 domain subfolders (`EmployeeManagement/`, `OrganizationManagement/`, `SalaryMasterData/`, `SalaryGradePromotion/`, `ContractManagement/`) per ADR-03. A service may depend on another domain's *service interface* but never another domain's repository directly (a repository's own EF `Include()` for cross-domain read hydration, e.g. `ContractRepository` including `Employee.OrganizationalUnit`/`Employee.JobTitle`, is not a violation — that rule is about Service→Repository dependencies, not a repository's own joins). Two genuine cross-domain dependencies resolve through `Lazy<T>` to avoid a DI constructor cycle: `OrganizationalUnitService ↔ EmployeeService`, and `SalaryGradeService → ISalaryHistoryService`. `ContractService → IEmployeeService` is one-way (no cycle), so it's injected directly, no `Lazy<T>` needed.

`IUnitOfWork` (`HRM.Application/Common/`, implemented in `HRM.Infrastructure/Persistence/`) wraps `HrmDbContext` transactions for the two flows that write across multiple repositories: `ReviewPeriodService.CreateReviewPeriodAsync` and `SalaryDecisionService.ApplyDecisionAsync`.

## Run

```
dotnet run --project src/HRM.Api
```

Requires a SQL Server instance reachable via the `ConnectionStrings:HrmDatabase` setting in `src/HRM.Api/appsettings.json` (defaults to LocalDB). Apply the EF Core migration first:

```
dotnet ef database update --project src/HRM.Infrastructure --startup-project src/HRM.Api
```

## Test

```
dotnet test
```

- `tests/HRM.Application.Tests` — unit tests for the Service/Rules layer (Moq + FluentAssertions, no database). Every business rule and guard traced from `Docs/DetailedDesign/SequenceDiagrams.md`/`StateDiagrams.md` is covered here.
- `tests/HRM.Api.Tests` — integration tests through the real HTTP pipeline (`WebApplicationFactory` + EF Core InMemory), checking routing/status codes against `openapi.yaml`. Business-rule edge cases are deliberately left to `HRM.Application.Tests`.

Current state: 191 Application.Tests + 96 Api.Tests, all passing, 0 build warnings.

## Out of scope (see `Docs/API/README.md`)

- **JWT auth / `[Authorize]`** — [ADR-07](../Docs/Arc42/09-architecture-decisions.md#adr-07-authentication-and-authorization-mechanism) is Proposed but not implemented; the role model (RISK-05) isn't finalized yet. `openapi.yaml` declares the `bearerAuth` scheme for documentation purposes only — no endpoint enforces it.
- **Identity & Access Management** (accounts, roles, permissions, the `/login` endpoint) — explicitly out of scope for every module's requirements.
