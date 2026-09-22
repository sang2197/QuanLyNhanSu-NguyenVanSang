# Docs - HRM System

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-18 · **Implementation Baseline Commit:** `77e5716`

Analysis and design documents for the HRM system's four analyzed modules: **Employee Management**, **Organization Management**, **Salary Master Data**, and **Salary Grade Promotion**. The backend for all four is implemented in [`backend/`](../backend/README.md); the frontend is design-only.

## Requirements Analysis

- [`Requirements/`](Requirements/README.md) — Mind map, user stories, and use cases for all five specified modules (including Contract Management).

## Screen Design (UI/UX)

- [`UI-UX/`](UI-UX/README.md) — Information architecture (sitemap), screens hierarchy, wireframe & screen behavior, UX guidelines, and the final screen design images.

## C4 Architecture Diagrams

- [`c4/`](c4/README.md) — C4 model architecture diagrams: System Context, Container, and Component (3 levels; the Code level is intentionally not included).

## Arc42 Architecture Documentation

- [`Arc42/`](Arc42/README.md) — Arc42 architecture documentation report (goals, constraints, solution strategy, runtime view, decisions, risks, glossary).

## Database Design

- [`Database/`](Database/README.md) — Database design for the full analyzed system (13 tables across all five modules — 12 implemented, `HrLaborContract` for Contract Management designed only): Mermaid ER diagram and DBML source.

## API Documentation

- [`API/`](API/README.md) — OpenAPI 3.0 spec for the full REST API (57 endpoints across 11 tags: 49 implemented by `backend/`, 8 for Contract Management designed only), mapped to the user stories.

## Code Structure

- [`CodeStructure/`](CodeStructure/README.md) — Frontend (React, design-only) and backend (ASP.NET Core, implemented) source folder layout, mapped to the C4 diagrams and Arc42 decisions.

## Detailed Design

- [`DetailedDesign/`](DetailedDesign/README.md) — Class diagrams, sequence diagrams, and state diagrams for the API, derived from Code Structure, API Documentation, Database Design, and the C4 diagrams. The last design step before implementation; the backend was implemented from them.
