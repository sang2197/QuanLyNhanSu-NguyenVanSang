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

## Known deviation from the docs

`openapi.yaml` declares path IDs (`periodId`, `employeeId`, `decisionId`) as `format: uuid`, but the database schema (`Gen_Table.sql`, DBML, ER diagram) consistently uses `int IDENTITY` primary keys across all 8 tables. This implementation follows the database (int IDs) since that is the schema actually agreed on in 3 separate documents; `openapi.yaml`'s `uuid` format appears to be a stale/default choice that was never reconciled with the DB design. Worth fixing in the docs later.

## Out of scope (see `Docs/API/README.md`)

- JWT auth / `[Authorize]` — RISK-05 (role model) not finalized yet.
- Master Data CRUD (Employees/Salary Scales/Salary Grades) — not designed; tests seed fixed data directly.
