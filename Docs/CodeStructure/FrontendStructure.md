# Frontend Folder Structure (React)

Proposed `frontend/` project layout for the **Salary Grade Promotion** feature, mapping the [C4 `HRM Web Application` container](../c4/README.md#2-container-diagram) to an actual source folder. Referenced from [Arc42 Section 5 (Building Block View)](../Arc42/05-building-block-view.md), which requires source code locations to be specified.

Feature-based structure: folders are organized by business capability (matching the [User Stories](../Requirements/UserStories_SalaryGradePromotion.md) and [Use Cases](../Requirements/UseCase_SalaryGradePromotion.md)), not by technical file type. See [ADR-08](../Arc42/09-architecture-decisions.md#adr-08-switch-frontend-framework-to-react).

```
frontend/
├── public/
├── src/
│   ├── api/                      # HTTP client + one file per openapi.yaml tag group
│   │   ├── httpClient.js         # base client; attaches the JWT from ADR-07 to every request
│   │   ├── reviewPeriodsApi.js   # calls /review-periods*
│   │   ├── salaryDecisionsApi.js # calls /salary-decisions*
│   │   └── salaryHistoryApi.js   # calls /employees/{id}/salary-history
│   ├── app/
│   │   ├── App.jsx
│   │   ├── router.jsx            # route definitions (React Router)
│   │   └── queryClient.js        # server-state cache setup (TanStack Query)
│   ├── auth/                     # ADR-07: current user/role, route guards
│   │   ├── AuthContext.jsx
│   │   ├── useAuth.js
│   │   └── RequireRole.jsx       # blocks a route unless role matches (HR Staff / Approver)
│   ├── components/               # shared, reusable, no business logic
│   │   ├── StatusBadge/          # consistent status badges (Wireframe UX rule)
│   │   ├── ConfirmDialog/        # confirmation for sensitive actions (Wireframe UX rule)
│   │   ├── DataTable/            # paginated tables (large-list `#usable` goal)
│   │   └── Pagination/
│   ├── features/                 # 1 folder per feature — mirrors the User Stories groups
│   │   ├── reviewPeriods/        # US-01, US-02
│   │   │   ├── components/
│   │   │   ├── hooks/            # useReviewPeriods, useCreateReviewPeriod, useSubmitReviewPeriod
│   │   │   └── pages/
│   │   ├── reviewEmployees/      # US-03, US-04 (screening + approve/reject, incl. bulk)
│   │   │   ├── components/
│   │   │   ├── hooks/
│   │   │   └── pages/
│   │   ├── salaryDecisions/      # US-06, US-07 (draft + apply)
│   │   │   ├── components/
│   │   │   ├── hooks/
│   │   │   └── pages/
│   │   └── salaryHistory/        # US-08 (read-only)
│   │       ├── components/
│   │       ├── hooks/
│   │       └── pages/
│   ├── layouts/
│   │   └── MainLayout.jsx        # nav shell matching the sidebar in the HTML prototype
│   ├── constants/                 # enums mirrored from openapi.yaml (ReviewPeriodStatus, etc.)
│   ├── utils/
│   └── index.jsx
├── .env.example
└── package.json
```

## Libraries this structure assumes

Closes [RISK-10](../Arc42/11-risks-and-technical-debt.md) — confirm or swap before implementation starts:

| Concern | Library | Why |
|---|---|---|
| Routing | React Router | De facto standard for React SPA routing. |
| Server state / caching | TanStack Query | Matches REST calls in `openapi.yaml` directly; handles loading/error/cache without a global store. |
| Auth/session state | React Context (`auth/`) | Small amount of state (current user, role, token) — a full state-management library is not needed just for this. |
| HTTP client | Axios (or `fetch` wrapper) | Centralizes attaching the JWT bearer token (ADR-07) in one place (`httpClient.js`). |
