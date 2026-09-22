# Backend Folder Structure (ASP.NET Core)

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-22 · **Implementation Baseline Commit:** `77e5716`

`backend/` project layout for the full HRM System — **Employee Management**, **Organization Management**, **Salary Master Data**, **Salary Grade Promotion**, and **Contract Management** — mapping the [C4 `HRM Backend API` container and its 5 components](../c4/README.md#3-component-diagram) to actual source projects. Referenced from [Arc42 Section 5 (Building Block View)](../Arc42/05-building-block-view.md), which requires source code locations to be specified.

Follows [ADR-03](../Arc42/09-architecture-decisions.md#adr-03-split-the-backend-by-business-domain) (split the backend by business domain) and [ADR-04](../Arc42/09-architecture-decisions.md#adr-04-layered-design-inside-each-backend-component) (Controller → Service → Repository within each domain).

All five components are implemented in `backend/` with this layout, against the current [Database Design](../Database/README.md) and [`openapi.yaml`](../API/openapi.yaml) — see [`backend/README.md`](../../backend/README.md).

```
backend/
├── src/
│   ├── HRM.Api/                                    # Presentation layer
│   │   ├── Controllers/                            # 1 controller per openapi.yaml tag (11), grouped by C4 component
│   │   │   ├── EmployeeManagement/
│   │   │   │   └── EmployeesController.cs                  # tag: Employees
│   │   │   ├── OrganizationManagement/
│   │   │   │   ├── OrganizationalUnitsController.cs        # tag: Organizational Units
│   │   │   │   └── JobTitlesController.cs                  # tag: Job Titles
│   │   │   ├── SalaryMasterData/
│   │   │   │   ├── BaseSalaryRatesController.cs            # tag: Base Salary Rate
│   │   │   │   ├── SalaryScalesController.cs               # tag: Salary Scales (incl. /salary-scales/{id}/grades)
│   │   │   │   └── SalaryGradesController.cs               # tag: Salary Grades (coefficients, deactivate/reactivate)
│   │   │   ├── SalaryGradePromotion/
│   │   │   │   ├── ReviewPeriodsController.cs              # tag: Review Periods
│   │   │   │   ├── ReviewPeriodEmployeesController.cs      # tag: Review Period Employees
│   │   │   │   ├── SalaryDecisionsController.cs            # tag: Salary Decisions
│   │   │   │   └── SalaryHistoryController.cs              # tag: Salary History (path is under /employees/, but the tag/domain is Salary Grade Promotion — see Traceability)
│   │   │   └── ContractManagement/
│   │   │       └── ContractsController.cs                  # tag: Contracts
│   │   ├── DTOs/
│   │   │   ├── Requests/                           # matches openapi.yaml request schemas, same 5 folders as Controllers
│   │   │   │   ├── EmployeeManagement/
│   │   │   │   ├── OrganizationManagement/
│   │   │   │   ├── SalaryMasterData/
│   │   │   │   ├── SalaryGradePromotion/
│   │   │   │   └── ContractManagement/
│   │   │   └── Responses/                          # same 5 folders, matches openapi.yaml response schemas
│   │   ├── Mappings/                               # DTO <-> domain entity mapping, same 5 folders
│   │   ├── Middleware/                             # global error handling (JWT validation per ADR-07 not yet implemented)
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
│   │   ├── ContractManagement/
│   │   │   ├── Services/                           # ContractService (create/search/update/delete, activate/expire/terminate lifecycle)
│   │   │   ├── Models/                             # CreateContractInput, UpdateContractInput
│   │   │   └── Interfaces/
│   │   ├── Common/
│   │   └── Exceptions/
│   │
│   ├── HRM.Domain/                                 # Entities/enums shared by every layer
│   │   ├── Entities/                               # 1 class per DB table (see Database Design) — 13 entities, kept flat (see Traceability)
│   │   └── Enums/                                  # ReviewPeriodStatus, SalaryDecisionStatus, EmploymentStatus, ActiveStatus, ContractType, ContractStatus, etc.
│   │
│   └── HRM.Infrastructure/                         # Data access layer
│       ├── Persistence/
│       │   ├── HrmDbContext.cs
│       │   └── Configurations/                     # 1 EF Core config per table (13), flat like Domain/Entities
│       ├── Repositories/                           # implements the Interfaces above, same 5 folders
│       │   ├── EmployeeManagement/
│       │   ├── OrganizationManagement/
│       │   ├── SalaryMasterData/
│       │   ├── SalaryGradePromotion/
│       │   └── ContractManagement/
│       └── Migrations/
│
└── tests/
    ├── HRM.Application.Tests/                      # same 5 folders as HRM.Application
    └── HRM.Api.Tests/
```

## Traceability

- `Controllers/`, `DTOs/Requests/`, `DTOs/Responses/`, `Mappings/`, `HRM.Application/*`, and `Repositories/` all share the same 5 top-level folders — `EmployeeManagement`, `OrganizationManagement`, `SalaryMasterData`, `SalaryGradePromotion`, `ContractManagement` — matching the 5 implemented components in the [C4 Component Diagram](../c4/README.md#3-component-diagram) and [ADR-03](../Arc42/09-architecture-decisions.md#adr-03-split-the-backend-by-business-domain).
- `HRM.Api/Controllers/` — 1 controller per tag, 11 controllers total, matching the 11 tags in [`openapi.yaml`](../API/openapi.yaml) (see [API README](../API/README.md#coverage)). `SalaryHistoryController` is grouped under `SalaryGradePromotion/` (not `EmployeeManagement/`) because the Salary History tag/data belongs to that domain even though its route (`/employees/{id}/salary-history`) is nested under `/employees` — see the Information Architecture's [Scope Notes](../UI-UX/InformationArchitecture_HRM.md#7-scope-notes) ("Salary Grade Promotion owns ... Salary History").
- `HRM.Domain/Entities/` and `Infrastructure/Persistence/Configurations/` — 1 class/config per table in [Database Design](../Database/README.md), 13 entities. Kept flat (not split into the 5 component folders) because several entities are read across component boundaries — e.g. `HrEmployee` is owned by Employee Management but read by Salary Grade Promotion and Contract Management, `HrSalaryGrade` is owned by Salary Master Data but read by Salary Grade Promotion — matching the [Cross-component Data Dependencies](../c4/README.md#cross-component-data-dependencies) the C4 diagrams call out explicitly.
- `HRM.Application/SalaryGradePromotion/Rules/` — the eligibility rule engine that runs when a review period is created (US-SGP-01 in [`UserStories_SalaryGradePromotion.md`](../Requirements/UserStories_SalaryGradePromotion.md)).
- `ContractService` depends on `IEmployeeService` (employee exists and is not Terminated — BR-CON-07, 18), the one cross-domain dependency the [C4 diagram documents](../c4/README.md#cross-component-data-dependencies) — never `IEmployeeRepository` directly, per [ADR-03](../Arc42/09-architecture-decisions.md#adr-03-split-the-backend-by-business-domain). The employee display information shown alongside a contract (code, name, unit, job title) is read through `ContractRepository`'s own EF `Include()` of the `Employee` navigation, the same repository-level pattern `EmployeeRepository` already uses for `OrganizationalUnit`/`JobTitle` — that is a repository's own join, not a cross-domain repository dependency, so it doesn't violate ADR-03.
