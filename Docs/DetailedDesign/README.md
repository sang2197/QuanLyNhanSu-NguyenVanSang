# Detailed Design

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-22 · **Implementation Baseline Commit:** `77e5716`

UML class, sequence, and state diagrams for the full HRM system — Employee Management, Organization Management, Salary Master Data, and Salary Grade Promotion — derived from [Code Structure](../CodeStructure/README.md), [API Documentation](../API/README.md), [Database Design](../Database/README.md), and [C4 Architecture Diagrams](../c4/README.md). The last design artifact before implementation; the backend in [`backend/`](../../backend/README.md) was implemented from these diagrams.

- [`ClassDiagram.md`](ClassDiagram.md) — Domain model (13 entities) and, for each of the 5 implemented C4 components, its Controller → Service → Repository classes with explicit `<<interface>>` boxes and cross-component dependencies.
- [`SequenceDiagrams.md`](SequenceDiagrams.md) — 25 key business-rule flows across all 5 modules (validation, guard conditions, or a transaction), one independent scenario per diagram, using `break` for early-exit guards and an explicit `HrmDbContext` for any transaction spanning more than one repository; simple unguarded CRUD is not diagrammed.
- [`StateDiagrams.md`](StateDiagrams.md) — Status lifecycles for Review Period, Review Outcome, Salary Decision, Employment Status, the shared Active/Inactive toggle (Organizational Unit, Job Title, Salary Scale, Salary Grade), and Contract Status.
