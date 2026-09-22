# Frontend Folder Structure (React)

> **Status:** Design only (no frontend code exists) · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-22 · **Implementation Baseline Commit:** `77e5716`

`frontend/` project layout (design only — not yet implemented) for the full HRM System — **Employee Management**, **Organization Management**, **Salary Master Data**, **Salary Grade Promotion**, and **Contract Management** — mapping the [C4 `HRM Web Application` container](../c4/README.md#2-container-diagram) to an actual source folder. Referenced from [Arc42 Section 5 (Building Block View)](../Arc42/05-building-block-view.md), which requires source code locations to be specified.

Feature-based structure: folders are organized by business capability, matching the [Information Architecture](../UI-UX/InformationArchitecture_HRM.md) sitemap and each module's Screens Hierarchy — not by technical file type. See [ADR-08](../Arc42/09-architecture-decisions.md#adr-08-switch-frontend-framework-to-react). No `frontend/` implementation exists yet; this is the target structure.

```
frontend/
├── public/
├── src/
│   ├── api/                                # HTTP client + 1 file per openapi.yaml tag (11), grouped like features/
│   │   ├── httpClient.js                   # base client; attaches the JWT from ADR-07 to every request
│   │   ├── employeeManagement/
│   │   │   └── employeesApi.js             # tag: Employees
│   │   ├── organizationManagement/
│   │   │   ├── organizationalUnitsApi.js   # tag: Organizational Units
│   │   │   └── jobTitlesApi.js             # tag: Job Titles
│   │   ├── salaryMasterData/
│   │   │   ├── baseSalaryRatesApi.js       # tag: Base Salary Rate
│   │   │   ├── salaryScalesApi.js          # tag: Salary Scales
│   │   │   └── salaryGradesApi.js          # tag: Salary Grades
│   │   ├── salaryGradePromotion/
│   │   │   ├── reviewPeriodsApi.js         # tag: Review Periods
│   │   │   ├── reviewPeriodEmployeesApi.js # tag: Review Period Employees
│   │   │   ├── salaryDecisionsApi.js       # tag: Salary Decisions
│   │   │   └── salaryHistoryApi.js         # tag: Salary History — calls /employees/{id}/salary-history
│   │   └── contractManagement/
│   │       └── contractsApi.js             # tag: Contracts
│   ├── app/
│   │   ├── App.jsx
│   │   ├── router.jsx                      # 1 route per Page in the IA Page List
│   │   └── queryClient.js                  # server-state cache setup (TanStack Query)
│   ├── auth/                               # ADR-07: current user/role, route guards
│   │   ├── AuthContext.jsx
│   │   ├── useAuth.js
│   │   └── RequireRole.jsx                 # blocks a route unless role matches (HR Staff / Approver)
│   ├── components/                         # shared, reusable, no business logic
│   │   ├── StatusBadge/                    # consistent status representation across modules (UX Guidelines)
│   │   ├── ConfirmDialog/                  # confirmation for destructive/irreversible actions (UX Guidelines)
│   │   ├── DataTable/                      # paginated/sortable lists (UX Guidelines)
│   │   └── Pagination/
│   ├── features/                           # 1 top folder per C4 component, 1 subfolder per IA page group
│   │   ├── employeeManagement/
│   │   │   └── employees/                  # IA: Employees, Employee Detail
│   │   │       ├── components/             # CreateEmployeeModal, UpdateEmployeeModal, ChangeEmploymentStatusDialog
│   │   │       ├── hooks/                  # useEmployees, useEmployee, useCreateEmployee, useUpdateEmployee, useChangeEmploymentStatus
│   │   │       └── pages/                  # EmployeeListPage, EmployeeDetailPage
│   │   │
│   │   ├── organizationManagement/
│   │   │   ├── organizationalUnits/        # IA: Organization Structure
│   │   │   │   ├── components/             # CreateUnitModal, UpdateUnitModal, MoveUnitModal, UnitStatusDialog
│   │   │   │   ├── hooks/                  # useOrganizationalUnits, useCreateUnit, useUpdateUnit, useMoveUnit, useSetUnitStatus
│   │   │   │   └── pages/                  # OrganizationStructurePage
│   │   │   └── jobTitles/                  # IA: Job Titles
│   │   │       ├── components/             # CreateJobTitleModal, UpdateJobTitleModal, JobTitleStatusDialog
│   │   │       ├── hooks/                  # useJobTitles, useCreateJobTitle, useUpdateJobTitle, useSetJobTitleStatus
│   │   │       └── pages/                  # JobTitlesPage
│   │   │
│   │   ├── salaryMasterData/
│   │   │   ├── baseSalaryRate/             # IA: Base Salary Rate
│   │   │   │   ├── components/             # AddBaseSalaryRateModal
│   │   │   │   ├── hooks/                  # useBaseSalaryRates, useAddBaseSalaryRate
│   │   │   │   └── pages/                  # BaseSalaryRatePage
│   │   │   └── salaryScales/               # IA: Salary Scales, Salary Scale Detail (+ nested Salary Grades)
│   │   │       ├── components/             # CreateScaleModal, UpdateScaleModal, ScaleStatusDialog, CreateGradeModal, UpdateCoefficientModal, GradeStatusDialog
│   │   │       ├── hooks/                  # useSalaryScales, useSalaryScale, useCreateScale, useUpdateScale, useSetScaleStatus, useCreateGrade, useUpdateCoefficient, useSetGradeStatus
│   │   │       └── pages/                  # SalaryScalesPage, SalaryScaleDetailPage
│   │   │
│   │   ├── salaryGradePromotion/
│   │   │   ├── reviewPeriods/              # IA: Review Periods, Review Period Detail
│   │   │   │   ├── components/             # CreatePeriodModal, CancelPeriodDialog, SubmitPeriodDialog, BulkApproveDialog, BulkRejectDialog
│   │   │   │   ├── hooks/                  # useReviewPeriods, useReviewPeriod, useCreateReviewPeriod, useSubmitReviewPeriod, useCancelReviewPeriod, useBulkApprove, useBulkReject
│   │   │   │   └── pages/                  # ReviewPeriodListPage, ReviewPeriodDetailPage
│   │   │   ├── reviewEmployees/            # IA: Employee Review Detail
│   │   │   │   ├── components/             # RejectEmployeeDialog
│   │   │   │   ├── hooks/                  # useReviewEmployee, useApproveEmployee, useRejectEmployee
│   │   │   │   └── pages/                  # EmployeeReviewDetailPage
│   │   │   ├── salaryDecisions/            # IA: Salary Decisions, Salary Decision Detail
│   │   │   │   ├── components/             # PickReviewPeriodDialog, ApplyDecisionDialog, CancelDecisionDialog
│   │   │   │   ├── hooks/                  # useSalaryDecisions, useSalaryDecision, useSaveDraftDecision, useApplyDecision, useCancelDecision
│   │   │   │   └── pages/                  # SalaryDecisionListPage, SalaryDecisionDetailPage
│   │   │   └── salaryHistory/              # IA: Salary History (read-only)
│   │   │       ├── hooks/                  # useSalaryHistory
│   │   │       └── pages/                  # SalaryHistoryPage
│   │   │
│   │   └── contractManagement/
│   │       └── contracts/                  # IA: Contracts, Contract Detail
│   │           ├── components/             # CreateContractModal, EditContractModal, DeleteContractDialog, ActivateContractDialog, MarkAsExpiredDialog, TerminateContractModal
│   │           ├── hooks/                  # useContracts, useContract, useCreateContract, useUpdateContract, useDeleteContract, useActivateContract, useExpireContract, useTerminateContract
│   │           └── pages/                  # ContractListPage, ContractDetailPage
│   │
│   ├── layouts/
│   │   └── MainLayout.jsx                  # nav shell; sections = Employee Management / Organization / Salary Management / Contract Management, matching the IA sitemap
│   ├── constants/                          # enums mirrored from openapi.yaml (ReviewPeriodStatus, EmploymentStatus, ActiveStatus, etc.)
│   ├── utils/
│   └── index.jsx
├── .env.example
└── package.json
```

## Traceability

- `features/` and `api/` share the same top-level folders — `employeeManagement`, `organizationManagement`, `salaryMasterData`, `salaryGradePromotion`, `contractManagement` — matching all 5 components in the [C4 Component Diagram](../c4/README.md#3-component-diagram), and matching the same folders in [Backend `Controllers/`](BackendStructure.md) for the first 4 (Contract Management has no backend folder yet — see that document's "Contract Management (designed, not yet implemented)" section).
- Each `features/*/*/` subfolder matches one page group in the [Information Architecture](../UI-UX/InformationArchitecture_HRM.md) sitemap and is expanded page-by-page in that module's Screens Hierarchy: [Employee Profile](../UI-UX/ScreensHierarchy_EmployeeProfile.md), [Organization Management](../UI-UX/ScreensHierarchy_OrganizationManagement.md), [Salary Master Data](../UI-UX/ScreensHierarchy_SalaryMasterData.md), [Salary Grade Promotion](../UI-UX/ScreensHierarchy_SalaryGradePromotion.md), [Contract Management](../UI-UX/ScreensHierarchy_ContractManagement.md).
- `features/*/*/components/` — 1 component per Modal/Dialog/Confirmation Dialog/Form-Action Modal node in that module's Screens Hierarchy (e.g. "Update Salary Grade Coefficient" → `UpdateCoefficientModal`; "Terminate Contract" → `TerminateContractModal`).
- `salaryGradePromotion/salaryHistory/` has no `components/` folder — its Screens Hierarchy defines no modal/dialog for it (read-only page).
- `components/` (shared) — implements the cross-module conventions in [UX Guidelines](../UI-UX/UXGuidelines_HRM.md): consistent status representation, confirmation dialogs for destructive actions, consistent list/pagination controls.
- `api/*` — 1 file per tag, 11 files total, matching the 11 tags in [`openapi.yaml`](../API/openapi.yaml) (see [API README](../API/README.md#coverage)) — including `contractManagement/contractsApi.js` for the 8 designed-only `Contracts` operations.
- `layouts/MainLayout.jsx` — top-level nav matches the 4 top-level groups in the [Information Architecture](../UI-UX/InformationArchitecture_HRM.md) sitemap (Employee Management, Organization, Salary Management, Contract Management); the `Dashboard` placeholder node is not implemented since it has no current requirements.

## Libraries this structure assumes

Closes [RISK-10](../Arc42/11-risks-and-technical-debt.md) — confirm or swap before implementation starts:

| Concern | Library | Why |
|---|---|---|
| Routing | React Router | De facto standard for React SPA routing. |
| Server state / caching | TanStack Query | Matches REST calls in `openapi.yaml` directly; handles loading/error/cache without a global store. |
| Auth/session state | React Context (`auth/`) | Small amount of state (current user, role, token) — a full state-management library is not needed just for this. |
| HTTP client | Axios (or `fetch` wrapper) | Centralizes attaching the JWT bearer token (ADR-07) in one place (`httpClient.js`). |
