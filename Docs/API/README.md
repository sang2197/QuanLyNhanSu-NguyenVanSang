# API Documentation - Salary Grade Promotion

REST API design for the **Salary Grade Promotion** feature, written as an [OpenAPI 3.0](https://swagger.io/specification/) spec.

- [`openapi.yaml`](openapi.yaml) — the spec. Paste its content into the [Swagger Editor](https://editor.swagger.io/) to view it as interactive documentation.

## Coverage

16 endpoints across 4 groups, each mapped to the [user story](../Requirements/UserStories_SalaryGradePromotion.md) it implements:

| Group | Endpoints | Stories |
|---|---|---|
| Review Periods | Create, search/filter, view detail, submit | US-01, US-02, US-03, US-05 |
| Review Period Employees | List, view detail, approve/reject (single + bulk) | US-03, US-04 |
| Salary Decisions | Draft, list, view detail, remove employee, apply | US-06, US-07 |
| Salary History | Look up an employee's history (read-only) | US-08 |

## Out of scope

- **Employee / Salary Scale / Salary Grade master data APIs** — not designed yet (see [Information Architecture](../UI-UX/InformationArchitecture_SalaryGradePromotion.md)). This spec only references `employeeId` / grade values; it does not define CRUD endpoints for them.
- **Login endpoint** — [ADR-07](../Arc42/09-architecture-decisions.md#adr-07-authentication-and-authorization-mechanism) proposes JWT bearer auth via ASP.NET Core Identity, and the spec below now declares the `bearerAuth` security scheme accordingly, but the actual `/login` (token-issuing) endpoint itself is not yet designed here.

## Notes

- Status enum values (`ReviewPeriodStatus`, `SalaryDecisionStatus`, etc.) were updated to match the corrected HR Staff / Approver workflow in the User Stories — they differ slightly from the older example values written earlier in the [Database Design](../Database/README.md) docx, which has not been revisited since the workflow correction.
- All endpoints use path/response shapes only — no example server has been built yet.
