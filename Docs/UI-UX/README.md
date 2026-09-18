# UI/UX Design - HRM System

Screen design for the HRM system, in design order: information architecture, screens hierarchy, wireframe behavior, then visual design. All four modules now have an Information Architecture entry, a Screens Hierarchy, a wireframe write-up, and an interactive HTML prototype — **Employee Profile**, **Organization Management**, **Salary Master Data**, and **Salary Grade Promotion** — sharing one common app shell/design system, with all four sidebars cross-linking to each other.

## Information Architecture

- [`InformationArchitecture_HRM.md`](InformationArchitecture_HRM.md) — Sitemap of the whole system's menu structure, covering all four modules (Employee Management, Organization, Salary Management, Master Data).

## Screens Hierarchy

- [`ScreensHierarchy_SalaryGradePromotion.md`](ScreensHierarchy_SalaryGradePromotion.md) — Every individual screen state (pages, modals, confirmation dialogs) for the Salary Grade Promotion module, with the action that triggers each transition. Expands the Information Architecture sitemap into a direct blueprint for the wireframe.
- [`ScreensHierarchy_EmployeeProfile.md`](ScreensHierarchy_EmployeeProfile.md) — Screen states for the Employee Profile module (Employee List, Employee Detail, and their modals/dialogs).
- [`ScreensHierarchy_OrganizationManagement.md`](ScreensHierarchy_OrganizationManagement.md) — Screen states for the Organization Management module (Organization Structure, Job Titles, and their modals/dialogs).
- [`ScreensHierarchy_SalaryMasterData.md`](ScreensHierarchy_SalaryMasterData.md) — Screen states for the Salary Master Data module (Base Salary Rate, Salary Scales, Salary Scale Detail, and their modals/dialogs).

## Wireframe & Screen Behavior

- [`Wireframe_SalaryGradePromotion.md`](Wireframe_SalaryGradePromotion.md) — Main behavior, validation, and business data references per screen for the Salary Grade Promotion module, plus the recommended navigation flow.
- [`Wireframe_EmployeeProfile.md`](Wireframe_EmployeeProfile.md) — Main behavior, validation, and business data references per screen for the Employee Profile module.
- [`Wireframe_OrganizationManagement.md`](Wireframe_OrganizationManagement.md) — Main behavior, validation, and business data references per screen for the Organization Management module.
- [`Wireframe_SalaryMasterData.md`](Wireframe_SalaryMasterData.md) — Main behavior, validation, and business data references per screen for the Salary Master Data module.
- [`UXGuidelines_HRM.md`](UXGuidelines_HRM.md) — UI/UX conventions shared across every HRM module (status badges, confirmation, pagination, etc.), not specific to any single feature's wireframe.

## Visual Design

### Employee Profile (prototype)

![Employee List](Employee%20List.png)

[`EmployeeProfile_Screens.html`](NewDesign/EmployeeProfile_Screens.html) — Interactive HTML prototype covering all 5 Employee Profile screens (Employee List, Create Employee Profile, Employee Detail, Update Employee Profile, Change Employment Status), built directly from `Wireframe_EmployeeProfile.md` and `UXGuidelines_HRM.md`. Introduces the shared global sidebar/app-shell design system. Open it in a browser to click through the flow.

### Organization Management (prototype)

![Organization Structure](Organization%20Structure.png)

[`OrganizationManagement_Screens.html`](NewDesign/OrganizationManagement_Screens.html) — Interactive HTML prototype covering all 9 Organization Management screens (Organization Structure as a hierarchy tree plus Create/Update/Move/Deactivate-Reactivate, and Job Titles plus Create/Update/Deactivate-Reactivate), built from `Wireframe_OrganizationManagement.md`. Reuses the exact same app shell/design system as `EmployeeProfile_Screens.html`; the two prototypes' sidebars link to each other for real cross-module navigation.

### Salary Master Data (prototype)

[`SalaryMasterData_Screens.html`](NewDesign/SalaryMasterData_Screens.html) — Interactive HTML prototype covering all 10 Salary Master Data screens (Base Salary Rate plus Add New Rate; Salary Scales; Salary Scale Detail plus Edit/Deactivate-Reactivate Scale and its Salary Grades plus Create/Update Coefficient/Deactivate-Reactivate Grade), built from `Wireframe_SalaryMasterData.md`. Reuses the same app shell/design system as the other prototypes.

### Salary Grade Promotion (prototype)

![Review Period List](Review%20Period%20List.png)
![Review Period Detail](Review%20Period%20Detail.png)
![Employee Review Detail](Employee%20Review%20Detail.png)
![Salary Decision List](Salary%20Decision%20List.png)
![Salary Decision Detail](Salary%20Decision%20Detail.png)
![Employee Salary History](Employee%20Salary%20History.png)

[`SalaryGradePromotion_Screens.html`](NewDesign/SalaryGradePromotion_Screens.html) — Interactive HTML prototype covering all 6 Salary Grade Promotion pages and their 9 modals/dialogs (Review Period List plus Create/Cancel; Review Period Detail plus Bulk Approve/Bulk Reject/Submit/Cancel/Decision Access; Employee Review Detail plus Approve/Reject; Salary Decision List plus Pick a Review Period; Salary Decision Detail plus Save Draft/Issue-Apply/Cancel; Employee Salary History), built from `Wireframe_SalaryGradePromotion.md`. Reuses the same app shell/design system as the other three prototypes; all four sidebars now cross-link to each other.
