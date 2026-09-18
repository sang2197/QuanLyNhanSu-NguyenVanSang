# Backend - Salary Grade Promotion

ASP.NET Core (.NET 8) implementation of the Salary Grade Promotion API, built strictly from [`Docs/`](../Docs/README.md) — see [`Docs/CodeStructure/BackendStructure.md`](../Docs/CodeStructure/BackendStructure.md) for the folder-structure rationale and [`Docs/API/openapi.yaml`](../Docs/API/openapi.yaml) for the API contract.

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

- `tests/HRM.Application.Tests` — unit tests for the Service/Rules layer (mocked repositories, no database). This is where every business rule from `UserStories_SalaryGradePromotion.md` (US-01 → US-11) is covered, especially the eligibility rule (RISK-08).
- `tests/HRM.Api.Tests` — integration tests through the real HTTP pipeline (`WebApplicationFactory` + EF Core InMemory), checking routing/status codes against `openapi.yaml`.

## Known deviations from the docs

**This backend was built against an earlier, Salary-Grade-Promotion-only database design (8 tables).** `Docs/Database/` has since been redesigned from scratch to cover the full analyzed system (Employee Profile, Organization Management, Salary Master Data, and Salary Grade Promotion — 12 tables), based on the current `UserStories_*.md`/`UseCase_*.md` for all four modules rather than on this implementation. The backend has not been migrated to the new schema yet. Notably: `HrOrganizationalUnit`/`HrJobTitle` are now real tables (this backend still uses bare `DepartmentId`/`PositionId` ints with no FK), salary-grade coefficients now have their own effective-dated history table instead of a single mutable column, and `HrSalaryDecision.DecisionType`/`FileUrl`/`SignerEmployeeId` were dropped (none are described by any current User Story/Use Case). Migrating this backend to the new schema is a separate, not-yet-started task.

Two smaller, previously-noted deviations against the old schema (both still true today, and superseded by the redesign above rather than fixed):

- `openapi.yaml` declares path IDs (`periodId`, `employeeId`, `decisionId`) as `format: uuid`, but the database schema consistently used `int IDENTITY` primary keys. This implementation follows the database (int IDs); `openapi.yaml`'s `uuid` format appears to be a stale/default choice that was never reconciled with the DB design.
- `HrSalaryReviewPeriodConfiguration.cs` only enforces a unique index on `Code`, not `Name`, even though US-SGP-01 AC03 requires rejecting a duplicate name.

## Out of scope (see `Docs/API/README.md`)

- JWT auth / `[Authorize]` — RISK-05 (role model) not finalized yet.
- Master Data CRUD (Employees/Salary Scales/Salary Grades) — not designed; tests seed fixed data directly.
