# Requirements Analysis

Requirements analysis documents for the HRM system: the overall mind map, user stories (centered on **Salary Grade Promotion**, plus the surrounding Organization Management / Employee Profile / Salary Master Data modules), and the use case diagram derived from them.

## Mind Map

- [`Quản lý nhân sự.xmind`](Quản%20lý%20nhân%20sự.xmind) — Top-down mind map breaking down the whole HRM system into its 6 function groups, down to feature-level detail.

## User Stories

- [`UserStories_SalaryGradePromotion.md`](UserStories_SalaryGradePromotion.md) — User stories with business rules and acceptance criteria (Given/When/Then) for the review-and-decision workflow. Written first, independent of any screen or database design.
- [`UserStories_OrganizationManagement.md`](UserStories_OrganizationManagement.md) — Organizational units (as a tree, typed as Company/Division/Department/Team) and job titles.
- [`UserStories_EmployeeProfile.md`](UserStories_EmployeeProfile.md) — Employee list/search/detail, employment status. Login account/role is out of scope, deferred to a future Identity & Access Management (IAM) module.
- [`UserStories_SalaryMasterData.md`](UserStories_SalaryMasterData.md) — Base salary rate, salary scales, and salary grades.

## Use Case Diagram

- [`UseCase_SalaryGradePromotion.md`](UseCase_SalaryGradePromotion.md) — Use case diagram and description (actors and main actions).
