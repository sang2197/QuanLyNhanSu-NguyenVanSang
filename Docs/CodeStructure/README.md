# Code Structure

> **Status:** Current (backend implemented, frontend design-only) · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-22 · **Implementation Baseline Commit:** `77e5716`

Source folder layout for the full HRM system — Employee Management, Organization Management, Salary Master Data, Salary Grade Promotion, and Contract Management — mapped to the [C4 diagrams](../c4/README.md) and [Arc42 Architecture Decisions](../Arc42/09-architecture-decisions.md).

Designed from the current C4 model, [Database Design](../Database/README.md), [API Documentation](../API/README.md), and UI/UX design ([Information Architecture](../UI-UX/InformationArchitecture_HRM.md) → Screens Hierarchy → UI/UX) and Use Cases/User Stories for all five modules. The backend layout is **implemented** in `backend/` for all five modules and matches [`BackendStructure.md`](BackendStructure.md). The frontend layout is **design-only** for all five modules — no `frontend/` folder exists yet.

- [`FrontendStructure.md`](FrontendStructure.md) — React (`frontend/`), feature-based folders, covering all five modules. *Design only.*
- [`BackendStructure.md`](BackendStructure.md) — ASP.NET Core (`backend/`), covering all five modules. *Implemented.*
