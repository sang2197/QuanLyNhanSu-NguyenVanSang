# 12. Glossary

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System.*

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-18 · **Implementation Baseline Commit:** `77e5716`

| Term | Meaning |
|---|---|
| Review Period | A time-boxed cycle (e.g. "Annual Review H1 2026") during which employees are reviewed for salary grade promotion. |
| Salary Scale | A named group of salary grades (e.g. "Scale A - Technical"). |
| Salary Grade | One step within a salary scale, with a coefficient (e.g. Grade 3.2, coefficient 3.66). |
| Coefficient | The number used to calculate actual salary from the base salary. |
| Eligibility | Whether an employee currently qualifies for a grade review (e.g. minimum time in current grade). |
| Salary Decision | The official, signed document that changes one or more employees' salary grade. |
| Proposal | A suggested new grade for an employee within a review period, not yet official until a decision is issued. |
| SPA (Single-Page Application) | A web app (here, the React frontend — see [ADR-08](09-architecture-decisions.md#adr-08-switch-frontend-framework-to-react)) that runs in the browser and talks to a backend API instead of reloading full pages from a server. |
| REST API | An API style where the Backend API exposes resources (e.g. review periods, decisions) over HTTP, used here for Web Application ↔ Backend API communication. |
| JWT (JSON Web Token) | A signed token proving who a user is, issued at login and sent with every API request (see [ADR-07](09-architecture-decisions.md#adr-07-authentication-and-authorization-mechanism)). |
| RBAC (Role-Based Access Control) | Restricting actions by role (HR Staff vs. Approver / Manager) rather than by individual user. |
| Component (C4) | A grouping of related code inside a container (e.g. Employee Management, Salary Grade Promotion) — see [Building Block View](05-building-block-view.md). |
| Container (C4) | A separately deployable/runnable part of the system (e.g. the Web Application, the Backend API, the Database) — see [Building Block View](05-building-block-view.md). |
| ADR (Architecture Decision Record) | A short document recording one significant architecture decision, its context, and its consequences — see [Section 9](09-architecture-decisions.md). |
