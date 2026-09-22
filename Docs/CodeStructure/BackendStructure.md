# Backend Folder Structure (ASP.NET Core)

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-22 · **Implementation Baseline Commit:** `77e5716`

`backend/` project layout for the full HRM System — **Employee Management**, **Organization Management**, **Salary Master Data**, and **Salary Grade Promotion** — mapping the [C4 `HRM Backend API` container and its 4 implemented components](../c4/README.md#3-component-diagram) to actual source projects. Referenced from [Arc42 Section 5 (Building Block View)](../Arc42/05-building-block-view.md), which requires source code locations to be specified.

Follows [ADR-03](../Arc42/09-architecture-decisions.md#adr-03-split-the-backend-by-business-domain) (split the backend by business domain) and [ADR-04](../Arc42/09-architecture-decisions.md#adr-04-layered-design-inside-each-backend-component) (Controller → Service → Repository within each domain).

All four components are implemented in `backend/` with this layout (the C4 model also contains a fifth component, Contract Management, which is designed but not yet part of this layout), against the current [Database Design](../Database/README.md) and [`openapi.yaml`](../API/openapi.yaml) — see [`backend/README.md`](../../backend/README.md).

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

- `Controllers/`, `DTOs/Requests/`, `DTOs/Responses/`, `Mappings/`, `HRM.Application/*`, and `Repositories/` all share the same 4 top-level folders — `EmployeeManagement`, `OrganizationManagement`, `SalaryMasterData`, `SalaryGradePromotion` — matching the 4 implemented components in the [C4 Component Diagram](../c4/README.md#3-component-diagram) and [ADR-03](../Arc42/09-architecture-decisions.md#adr-03-split-the-backend-by-business-domain).
- `HRM.Api/Controllers/` — 1 controller per tag, 10 controllers total, matching the 10 tags in [`openapi.yaml`](../API/openapi.yaml) (see [API README](../API/README.md#coverage)). `SalaryHistoryController` is grouped under `SalaryGradePromotion/` (not `EmployeeManagement/`) because the Salary History tag/data belongs to that domain even though its route (`/employees/{id}/salary-history`) is nested under `/employees` — see the Information Architecture's [Scope Notes](../UI-UX/InformationArchitecture_HRM.md#7-scope-notes) ("Salary Grade Promotion owns ... Salary History").
- `HRM.Domain/Entities/` and `Infrastructure/Persistence/Configurations/` — 1 class/config per table in [Database Design](../Database/README.md), 12 entities. Kept flat (not split into the 4 component folders) because several entities are read across component boundaries — e.g. `HrEmployee` is owned by Employee Management but read by Salary Grade Promotion, `HrSalaryGrade` is owned by Salary Master Data but read by Salary Grade Promotion — matching the [Cross-component Data Dependencies](../c4/README.md#cross-component-data-dependencies) the C4 diagrams call out explicitly.
- `HRM.Application/SalaryGradePromotion/Rules/` — the eligibility rule engine that runs when a review period is created (US-SGP-01 in [`UserStories_SalaryGradePromotion.md`](../Requirements/UserStories_SalaryGradePromotion.md)).

## Contract Management (designed, not yet implemented)

The C4 model's fifth component — see the [Component Diagram](../c4/README.md#3-component-diagram), [`HrLaborContract`](../Database/HRM_System.dbml) in the Database Design, and the `Contracts` tag (8 operations) in [`openapi.yaml`](../API/openapi.yaml). It has no code in `backend/` yet. Once built, it follows the same layout as the four components above, adding a fifth top-level folder — `ContractManagement` — everywhere the other four appear:

```
HRM.Api/Controllers/ContractManagement/ContractsController.cs                   # tag: Contracts
HRM.Api/DTOs/Requests/ContractManagement/
HRM.Api/DTOs/Responses/ContractManagement/
HRM.Api/Mappings/ContractManagement/
HRM.Application/ContractManagement/Services/ContractService.cs
HRM.Application/ContractManagement/Interfaces/
HRM.Domain/Entities/HrLaborContract.cs                                          # 13th entity, flat like the other 12
HRM.Infrastructure/Persistence/Configurations/HrLaborContractConfiguration.cs
HRM.Infrastructure/Repositories/ContractManagement/
tests/HRM.Application.Tests/ContractManagement/
```

`ContractService` would depend on `IEmployeeService` (employee exists and is not Terminated — BR-CON-07, 18), the one cross-domain dependency the [C4 diagram already documents](../c4/README.md#cross-component-data-dependencies) as "planned, not yet implemented" — never `IEmployeeRepository` directly, per [ADR-03](../Arc42/09-architecture-decisions.md#adr-03-split-the-backend-by-business-domain).
