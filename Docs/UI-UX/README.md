# UI/UX Design - HRM System

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-18 · **Implementation Baseline Commit:** `77e5716`

Screen design for the HRM system, in design order: information architecture, screens hierarchy, wireframe behavior, then visual design. All four modules — **Employee Profile**, **Organization Management**, **Salary Master Data**, and **Salary Grade Promotion** — have an Information Architecture entry, a Screens Hierarchy, and a wireframe write-up. The final visual design is provided as PNG images (see [Visual Design](#visual-design)). **Contract Management** also has an Information Architecture entry, a Screens Hierarchy, a wireframe write-up, and visual design images.

## Information Architecture

- [`InformationArchitecture_HRM.md`](InformationArchitecture_HRM.md) — Sitemap of the whole system's menu structure, covering Employee Management, Organization, Salary Management (Salary Grade Promotion and Salary Master Data), and Contract Management.

## Screens Hierarchy

- [`ScreensHierarchy_SalaryGradePromotion.md`](ScreensHierarchy_SalaryGradePromotion.md) — Every individual screen state (pages, modals, confirmation dialogs) for the Salary Grade Promotion module, with the action that triggers each transition. Expands the Information Architecture sitemap into a direct blueprint for the wireframe.
- [`ScreensHierarchy_EmployeeProfile.md`](ScreensHierarchy_EmployeeProfile.md) — Screen states for the Employee Profile module (Employee List, Employee Detail, and their modals/dialogs).
- [`ScreensHierarchy_OrganizationManagement.md`](ScreensHierarchy_OrganizationManagement.md) — Screen states for the Organization Management module (Organization Structure, Job Titles, and their modals/dialogs).
- [`ScreensHierarchy_SalaryMasterData.md`](ScreensHierarchy_SalaryMasterData.md) — Screen states for the Salary Master Data module (Base Salary Rate, Salary Scales, Salary Scale Detail, and their modals/dialogs).
- [`ScreensHierarchy_ContractManagement.md`](ScreensHierarchy_ContractManagement.md) — Screen states for the Contract Management module (Contract List, Contract Detail, and their modals/dialogs), including the expiring-soon quick filter and the contextual link to Employee Detail.

## Wireframe & Screen Behavior

- [`Wireframe_SalaryGradePromotion.md`](Wireframe_SalaryGradePromotion.md) — Main behavior, validation, and business data references per screen for the Salary Grade Promotion module, plus the recommended navigation flow.
- [`Wireframe_EmployeeProfile.md`](Wireframe_EmployeeProfile.md) — Main behavior, validation, and business data references per screen for the Employee Profile module.
- [`Wireframe_OrganizationManagement.md`](Wireframe_OrganizationManagement.md) — Main behavior, validation, and business data references per screen for the Organization Management module.
- [`Wireframe_SalaryMasterData.md`](Wireframe_SalaryMasterData.md) — Main behavior, validation, and business data references per screen for the Salary Master Data module.
- [`Wireframe_ContractManagement.md`](Wireframe_ContractManagement.md) — Main behavior, form fields, list columns, validation, and business data references per screen for the Contract Management module, plus the recommended navigation flow.
- [`UXGuidelines_HRM.md`](UXGuidelines_HRM.md) — UI/UX conventions shared across every HRM module (status badges, confirmation, pagination, etc.), not specific to any single feature's wireframe.

## Visual Design

The PNG images below are the authoritative final screen designs. They cover the key screens of Employee Profile, Organization Management, Salary Grade Promotion, and Contract Management. Salary Master Data consists of basic CRUD screens, so it is specified by its [wireframe](Wireframe_SalaryMasterData.md) and [screens hierarchy](ScreensHierarchy_SalaryMasterData.md) only, by design — no separate design image is needed.

### Employee Profile

#### Employee List
![Employee List](Employee%20List.png)

### Organization Management

#### Organization Structure
![Organization Structure](Organization%20Structure.png)

### Salary Grade Promotion

#### Review Period List
![Review Period List](Review%20Period%20List.png)

#### Review Period Detail
![Review Period Detail](Review%20Period%20Detail.png)

#### Employee Review Detail
![Employee Review Detail](Employee%20Review%20Detail.png)

#### Salary Decision List
![Salary Decision List](Salary%20Decision%20List.png)

#### Salary Decision Detail
![Salary Decision Detail](Salary%20Decision%20Detail.png)

#### Employee Salary History
![Employee Salary History](Employee%20Salary%20History.png)

### Contract Management

#### Contract List
![Contract List](Contract%20List.png)

#### Contract Detail
![Contract Detail](Contract%20Detail.png)

#### Create Contract
![Create Contract](Create%20Contract.png)
