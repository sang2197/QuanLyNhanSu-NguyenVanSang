# Backend Folder Structure (ASP.NET Core, 3-layer)

Proposed `backend/` project layout for the **Salary Grade Promotion** feature, mapping the [C4 `HRM Backend API` container](../c4/README.md#2-container-diagram) to actual source projects. Referenced from [Arc42 Section 5 (Building Block View)](../Arc42/05-building-block-view.md), which requires source code locations to be specified.

Follows [ADR-04](../Arc42/09-architecture-decisions.md#adr-04-layered-design-inside-salary-management) (Controller → Service → Repository) and [ADR-03](../Arc42/09-architecture-decisions.md#adr-03-split-the-backend-by-business-domain) (split by business domain).

```
backend/
├── src/
│   ├── HRM.Api/                          # Presentation layer
│   │   ├── Controllers/
│   │   │   ├── ReviewPeriodsController.cs
│   │   │   ├── SalaryDecisionsController.cs
│   │   │   └── SalaryHistoryController.cs
│   │   ├── Middleware/                   # JWT validation, global error handling
│   │   ├── DTOs/
│   │   │   ├── Requests/                 # matches openapi.yaml request schemas
│   │   │   └── Responses/                # matches openapi.yaml response schemas
│   │   ├── Mappings/                     # DTO <-> domain entity mapping
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── HRM.Application/                  # Business logic layer
│   │   ├── EmployeeManagement/           # matches the C4 Component of the same name
│   │   │   └── Services/
│   │   └── SalaryManagement/             # matches the C4 Component of the same name
│   │       ├── Services/
│   │       │   ├── SalaryReviewService.cs    # matches the Code Diagram
│   │       │   ├── SalaryDecisionService.cs
│   │       │   └── SalaryHistoryService.cs
│   │       ├── Rules/                    # eligibility rule engine (US-03)
│   │       └── Interfaces/               # repository interfaces, implemented in Infrastructure
│   │
│   ├── HRM.Domain/                       # Entities/enums shared by every layer
│   │   ├── Entities/                     # 1 class per DB table (see Database Design)
│   │   └── Enums/                        # ReviewPeriodStatus, EligibilityStatus, etc.
│   │
│   └── HRM.Infrastructure/               # Data access layer
│       ├── Persistence/
│       │   ├── HrmDbContext.cs
│       │   └── Configurations/           # 1 EF Core config per table
│       ├── Repositories/
│       │   ├── SalaryRepository.cs       # matches the Code Diagram
│       │   └── EmployeeRepository.cs
│       └── Migrations/
│
└── tests/
    ├── HRM.Application.Tests/
    └── HRM.Api.Tests/
```

## Traceability

- `HRM.Domain/Entities/` — 1 class per table in [Database Design](../Database/README.md) (8 entities).
- `HRM.Api/Controllers/` and `DTOs/` — 1:1 with the paths/schemas in [`openapi.yaml`](../API/openapi.yaml).
- `HRM.Application/SalaryManagement/Services/SalaryReviewService.cs` — the same class named in the [Code Diagram](../c4/README.md#4-code-diagram).
- `EmployeeManagement/` vs `SalaryManagement/` — the same split as the [Component Diagram](../c4/README.md#3-component-diagram).
