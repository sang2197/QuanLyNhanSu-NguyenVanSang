# Backend Folder Structure (ASP.NET Core, 3-layer)

Proposed `backend/` project layout for the full HRM System — **Employee Management**, **Organization Management**, **Salary Master Data**, and **Salary Grade Promotion** — mapping the [C4 `HRM Backend API` container and its 4 components](../c4/README.md#3-component-diagram) to actual source projects. Referenced from [Arc42 Section 5 (Building Block View)](../Arc42/05-building-block-view.md), which requires source code locations to be specified.

Follows [ADR-03](../Arc42/09-architecture-decisions.md#adr-03-split-the-backend-by-business-domain) (split the backend by business domain) and [ADR-04](../Arc42/09-architecture-decisions.md#adr-04-layered-design-inside-salary-grade-promotion) (Controller → Service → Repository within each domain — ADR-04 names Salary Grade Promotion specifically but states the same pattern is intended for the other three components once they are built).

Only Salary Grade Promotion is actually implemented in `backend/` today, against an earlier, narrower database/API design — see `backend/README.md`'s "Known deviations from the docs". This layout is the target structure for all four components once they are built/migrated against the current [Database Design](../Database/README.md) and [`openapi.yaml`](../API/openapi.yaml).

```
backend/
├── src/
│   ├── HRM.Api/                                    # Presentation layer
│   │   ├── Controllers/                            # 1 controller per openapi.yaml tag (10), grouped by C4 component
│   │   │   ├── EmployeeManagement/
│   │   │   │   └── EmployeesController.cs                  # tag: Employees
│   │   │   ├── OrganizationManagement/
│   │   │   │   ├── OrganizationalUnitsController.cs        # tag: Organizational Units
│   │   │   │   └── JobTitlesController.cs                  # tag: Job Titles
│   │   │   ├── SalaryMasterData/
│   │   │   │   ├── BaseSalaryRatesController.cs            # tag: Base Salary Rate
│   │   │   │   ├── SalaryScalesController.cs               # tag: Salary Scales (incl. /salary-scales/{id}/grades)
│   │   │   │   └── SalaryGradesController.cs               # tag: Salary Grades (coefficients, deactivate/reactivate)
│   │   │   └── SalaryGradePromotion/
│   │   │       ├── ReviewPeriodsController.cs              # tag: Review Periods
│   │   │       ├── ReviewPeriodEmployeesController.cs      # tag: Review Period Employees
│   │   │       ├── SalaryDecisionsController.cs            # tag: Salary Decisions
│   │   │       └── SalaryHistoryController.cs              # tag: Salary History (path is under /employees/, but the tag/domain is Salary Grade Promotion — see Traceability)
│   │   ├── DTOs/
│   │   │   ├── Requests/                           # matches openapi.yaml request schemas, same 4 folders as Controllers
│   │   │   │   ├── EmployeeManagement/
│   │   │   │   ├── OrganizationManagement/
│   │   │   │   ├── SalaryMasterData/
│   │   │   │   └── SalaryGradePromotion/
│   │   │   └── Responses/                          # same 4 folders, matches openapi.yaml response schemas
│   │   ├── Mappings/                               # DTO <-> domain entity mapping, same 4 folders
│   │   ├── Middleware/                             # JWT validation (ADR-07), global error handling
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── HRM.Application/                            # Business logic layer — 1 folder per C4 component (ADR-03)
│   │   ├── EmployeeManagement/
│   │   │   ├── Services/                           # EmployeeService (profile CRUD, employment status)
│   │   │   └── Interfaces/                         # repository interfaces, implemented in Infrastructure
│   │   ├── OrganizationManagement/
│   │   │   ├── Services/                           # OrganizationalUnitService, JobTitleService
│   │   │   └── Interfaces/
│   │   ├── SalaryMasterData/
│   │   │   ├── Services/                           # BaseSalaryRateService, SalaryScaleService, SalaryGradeService
│   │   │   └── Interfaces/
│   │   ├── SalaryGradePromotion/
│   │   │   ├── Services/                           # ReviewPeriodService, ReviewEmployeeService, SalaryDecisionService, SalaryHistoryService
│   │   │   ├── Rules/                              # eligibility rule engine — eligibility/proposed grade determined on create (US-SGP-01)
│   │   │   └── Interfaces/
│   │   ├── Common/
│   │   └── Exceptions/
│   │
│   ├── HRM.Domain/                                 # Entities/enums shared by every layer
│   │   ├── Entities/                               # 1 class per DB table (see Database Design) — 12 entities, kept flat (see Traceability)
│   │   └── Enums/                                  # ReviewPeriodStatus, SalaryDecisionStatus, EmploymentStatus, ActiveStatus, etc.
│   │
│   └── HRM.Infrastructure/                         # Data access layer
│       ├── Persistence/
│       │   ├── HrmDbContext.cs
│       │   └── Configurations/                     # 1 EF Core config per table (12), flat like Domain/Entities
│       ├── Repositories/                           # implements the Interfaces above, same 4 folders
│       │   ├── EmployeeManagement/
│       │   ├── OrganizationManagement/
│       │   ├── SalaryMasterData/
│       │   └── SalaryGradePromotion/
│       └── Migrations/
│
└── tests/
    ├── HRM.Application.Tests/                      # same 4 folders as HRM.Application
    └── HRM.Api.Tests/
```

## Traceability

- `Controllers/`, `DTOs/Requests/`, `DTOs/Responses/`, `Mappings/`, `HRM.Application/*`, and `Repositories/` all share the same 4 top-level folders — `EmployeeManagement`, `OrganizationManagement`, `SalaryMasterData`, `SalaryGradePromotion` — matching the 4 components in the [C4 Component Diagram](../c4/README.md#3-component-diagram) and [ADR-03](../Arc42/09-architecture-decisions.md#adr-03-split-the-backend-by-business-domain).
- `HRM.Api/Controllers/` — 1 controller per tag, 10 controllers total, matching the 10 tags in [`openapi.yaml`](../API/openapi.yaml) (see [API README](../API/README.md#coverage)). `SalaryHistoryController` is grouped under `SalaryGradePromotion/` (not `EmployeeManagement/`) because the Salary History tag/data belongs to that domain even though its route (`/employees/{id}/salary-history`) is nested under `/employees` — see the Information Architecture's [Scope Notes](../UI-UX/InformationArchitecture_HRM.md#7-scope-notes) ("Salary Grade Promotion owns ... Salary History").
- `HRM.Domain/Entities/` and `Infrastructure/Persistence/Configurations/` — 1 class/config per table in [Database Design](../Database/README.md), 12 entities. Kept flat (not split into the 4 component folders) because several entities are read across component boundaries — e.g. `HrEmployee` is owned by Employee Management but read by Salary Grade Promotion, `HrSalaryGrade` is owned by Salary Master Data but read by Salary Grade Promotion — matching the [Cross-component Data Dependencies](../c4/README.md#cross-component-data-dependencies) the C4 diagrams call out explicitly.
- `HRM.Application/SalaryGradePromotion/Rules/` — the eligibility rule engine that runs when a review period is created (US-SGP-01 in [`UserStories_SalaryGradePromotion.md`](../Requirements/UserStories_SalaryGradePromotion.md)).
