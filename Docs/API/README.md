# API Documentation - HRM System

REST API design for the full HRM system as currently analyzed — Employee Profile, Organization Management, Salary Master Data, and Salary Grade Promotion — written as an [OpenAPI 3.0](https://swagger.io/specification/) spec.

Designed from the current User Stories / Use Cases (INVEST), Information Architecture / Screens Hierarchy / UI-UX, the C4 model (`Docs/c4/`), and the database design (`Docs/Database/HRM_System.dbml`). The existing backend and its Class/Sequence diagrams predate this analysis and were not used as a reference — see `backend/README.md`'s "Known deviations from the docs".

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

All resource IDs are `integer` (matching the `int IDENTITY` primary keys in `Docs/Database/HRM_System.dbml`) — this resolves a known mismatch the previous, Salary-Grade-Promotion-only version of this spec had against the database (it declared `uuid`).

## Design decisions worth noting

- **No CRUD endpoint fields beyond what a User Story/Business Rule supports.** For example, `HrSalaryDecision` in the database has no `decisionType`, `signerEmployeeId`, or `fileUrl` — none of those appear in any current User Story/Use Case, so the request/response schemas don't expose them either. `decisionNumber` is system-generated (read-only), matching the Wireframe.
- **`GET /organizational-units` returns the full hierarchy unpaginated** as a flat list (each unit carries `parentId`) — the UI needs the whole tree at once to render collapsible nodes, matching how the HTML prototype holds its data.
- **A dedicated `PUT /salary-decisions/{decisionId}` ("Save Draft")** exists for the effective date, matching the Wireframe's "Save Draft" action, which the earlier version of this spec didn't have.
- **`GET /salary-decisions/eligible-review-periods`** backs the "Pick a Review Period" step (Screens Hierarchy) when starting a new decision from the Salary Decision list.

## Out of scope

- **Login endpoint** — [ADR-07](../Arc42/09-architecture-decisions.md#adr-07-authentication-and-authorization-mechanism) proposes JWT bearer auth via ASP.NET Core Identity, and the spec declares the `bearerAuth` security scheme accordingly, but the actual `/login` (token-issuing) endpoint itself is not designed here.
- **Identity & Access Management** (accounts, roles, permissions) — explicitly out of scope for every module's requirements; not modeled here.

## Notes

- All endpoints use path/response shapes only — no example server has been built yet (the existing `backend/` targets the earlier, narrower schema — see its README).
- Enum values (`ReviewPeriodStatus`, `SalaryDecisionStatus`, `EmploymentStatus`, `ActiveStatus`, etc.) match the current database design and business rules, not the earlier backend implementation.
