# Information Architecture - Salary Grade Promotion Module

This shows the menu structure (sitemap) of the HRM system, based on the left sidebar visible in the Figma mockups. It is different from the "Recommended Navigation Flow" in the wireframe document — that one shows the step-by-step process for one task; this one shows the full menu/page structure.

## Sitemap

```mermaid
flowchart TD
    Root([HRM System])

    Root --> Dashboard[Dashboard]
    Root --> SM[[Salary Management]]
    Root --> MD[[Master Data]]

    SM --> RPL[Review Period List]
    SM --> SDEC[Salary Decision List]
    SM --> SHIS[Salary History]

    RPL --> RPD[Review Period Detail]
    RPD --> ERD[Employee Review Detail]

    SDEC --> SDCE[Create/Edit Salary Decision]

    MD --> EMP[Employees]
    MD --> SS[Salary Scales]
    MD --> SG[Salary Grades]

    ERD -. View Full History .-> SHIS
    SDCE -. Decision Number link .-> SHIS
    RPD -. "Create Decision (period pre-filled, only when Submitted)" .-> SDCE
```

## Page List

| Page | Menu Group | Parent Page | Status |
|---|---|---|---|
| Dashboard | Overview | - | Not designed yet |
| Review Period List | Salary Management | - | Designed (Screen 1) |
| Review Period Detail | Salary Management | Review Period List | Designed (Screen 2) |
| Employee Review Detail | Salary Management | Review Period Detail | Designed (Screen 3) |
| Salary Decision List | Salary Management | - | Designed (Screen 4) — new landing page for the "Salary Decisions" menu item |
| Create/Edit Salary Decision | Salary Management | Salary Decision List *(also reachable directly from Review Period Detail — see dashed link above)* | Designed (Screen 5) |
| Salary History | Salary Management | - | Designed (Screen 6) |
| Employees | Master Data | - | Not designed yet |
| Salary Scales | Master Data | - | Not designed yet |
| Salary Grades | Master Data | - | Not designed yet |
