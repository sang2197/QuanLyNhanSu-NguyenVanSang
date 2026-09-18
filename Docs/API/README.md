# API Documentation - HRM System

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-18 · **Implementation Baseline Commit:** `77e5716`

REST API for the full HRM system — Employee Profile, Organization Management, Salary Master Data, and Salary Grade Promotion — written as an [OpenAPI 3.0](https://swagger.io/specification/) spec and implemented by [`backend/`](../../backend/README.md).

Designed from the current User Stories / Use Cases (INVEST), Information Architecture / Screens Hierarchy / UI-UX, the C4 model (`Docs/c4/`), and the database design (`Docs/Database/HRM_System.dbml`). The backend was then implemented against this spec: the 49 operations (method + path) of the generated Swagger document match `openapi.yaml` exactly, across the same 10 tags.

- [`openapi.yaml`](openapi.yaml) — the spec. Paste its content into the [Swagger Editor](https://editor.swagger.io/) to view it as interactive documentation, or run `npx @redocly/cli lint openapi.yaml` to validate it.

## Coverage

49 endpoints across 10 tags, each mapped to the user story it implements:

| Tag | Endpoints | Stories |
|---|---|---|
| Employees | Create, search/filter, view, update, change employment status | US-EMP-01–05 |
| Organizational Units | Create, view structure, update, move, deactivate/reactivate | US-ORG-01–05 |
| Job Titles | Create, list, update, deactivate/reactivate | US-ORG-06–08 |
| Base Salary Rate | Add, list history (with as-of-date lookup) | US-SAL-01 |
| Salary Scales | Create, list, view detail, update, deactivate/reactivate | US-SAL-02, 03, 07 |
| Salary Grades | Create, add/list coefficient history, deactivate/reactivate | US-SAL-04–06 |
| Review Periods | Create, search/filter, view detail, submit, cancel | US-SGP-01, 02, 05, 11 |
| Review Period Employees | List, view detail, approve/reject (single + bulk) | US-SGP-03, 04 |
| Salary Decisions | Draft, list, view detail, save draft, remove employee, apply, cancel | US-SGP-06, 07, 09, 10 |
| Salary History | Look up an employee's history (read-only) | US-SGP-08 |

All resource IDs are `integer`, matching the `int IDENTITY` primary keys in `Docs/Database/HRM_System.dbml`.

## Design decisions worth noting

- **No CRUD endpoint fields beyond what a User Story/Business Rule supports.** For example, `HrSalaryDecision` in the database has no `decisionType`, `signerEmployeeId`, or `fileUrl` — none of those appear in any current User Story/Use Case, so the request/response schemas don't expose them either. `decisionNumber` is system-generated (read-only), matching the Wireframe.
- **`GET /organizational-units` returns the full hierarchy unpaginated** as a flat list (each unit carries `parentId`) — the UI needs the whole tree at once to render collapsible nodes, so the client does not need one request per level.
- **A dedicated `PUT /salary-decisions/{decisionId}` ("Save Draft")** exists for the effective date, matching the Wireframe's "Save Draft" action.
- **`GET /salary-decisions/eligible-review-periods`** backs the "Pick a Review Period" step (Screens Hierarchy) when starting a new decision from the Salary Decision list.

## Out of scope

- **Login endpoint** — [ADR-07](../Arc42/09-architecture-decisions.md#adr-07-authentication-and-authorization-mechanism) proposes JWT bearer auth via ASP.NET Core Identity, and the spec declares the `bearerAuth` security scheme accordingly, but the actual `/login` (token-issuing) endpoint itself is not designed here.
- **Identity & Access Management** (accounts, roles, permissions) — explicitly out of scope for every module's requirements; not modeled here.

## Implementation status

- **All 49 endpoints are implemented** in `backend/` (ASP.NET Core, one controller per tag) and covered by HTTP-level integration tests that check routing and status codes against this spec.
- **Error responses:** `ExceptionHandlingMiddleware` returns the `Error` schema with 400 (validation), 404 (not found), 409 (conflict), and 500 (unexpected). The generated Swagger only declares success responses, so the error responses documented here are not yet visible in Swagger UI — see [DEBT-03](../Arc42/11-risks-and-technical-debt.md#technical-debt).
- **Server URL / versioning:** the spec lists the placeholder server `https://api.example.com/v1`; the implementation serves routes from the root without a version prefix — see [DEBT-05](../Arc42/11-risks-and-technical-debt.md#technical-debt).
- **Authentication:** the `bearerAuth` scheme is declared for documentation only. No endpoint enforces it — the backend has no authentication or authorization yet ([ADR-07](../Arc42/09-architecture-decisions.md#adr-07-authentication-and-authorization-mechanism), [RISK-05](../Arc42/11-risks-and-technical-debt.md)).
- Enum values (`ReviewPeriodStatus`, `SalaryDecisionStatus`, `EmploymentStatus`, `ActiveStatus`, etc.) match the database design and are the same enums used by the backend.
