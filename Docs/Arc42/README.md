# Arc42

> **Status:** Current · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-18 · **Implementation Baseline Commit:** `77e5716`

Architecture documentation for the HRM System (Employee Management, Organization Management, Salary Master Data, and Salary Grade Promotion), following the [arc42](https://arc42.org/) template — one file per section, as recommended by the [arc42-toolkit](https://github.com/MSiccDev/arc42-toolkit) so the automated `arc42-lint` check can run against it.

1. [Introduction and Goals](01-introduction-and-goals.md)
2. [Constraints](02-constraints.md)
3. [Context and Scope](03-context-and-scope.md)
4. [Solution Strategy](04-solution-strategy.md)
5. [Building Block View](05-building-block-view.md)
6. [Runtime View](06-runtime-view.md)
7. [Deployment View](07-deployment-view.md)
8. [Crosscutting Concepts](08-crosscutting-concepts.md)
9. [Architecture Decisions](09-architecture-decisions.md)
10. [Quality Requirements](10-quality-requirements.md)
11. [Risks and Technical Debt](11-risks-and-technical-debt.md)
12. [Glossary](12-glossary.md)

Source code locations for these building blocks are documented separately in [`Docs/CodeStructure/`](../CodeStructure/README.md).

Each section carries its own review metadata (Status, Owner, Last Reviewed, Implementation Baseline Commit). Sections that describe things that do not exist yet are marked accordingly: [07 Deployment View](07-deployment-view.md) and [10 Quality Requirements](10-quality-requirements.md) are *Draft*; the security mechanism ([ADR-07](09-architecture-decisions.md#adr-07-authentication-and-authorization-mechanism), [§8.2](08-crosscutting-concepts.md#82-security)) is *Proposed* and not implemented.
