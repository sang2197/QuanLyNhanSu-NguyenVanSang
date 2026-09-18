# Code Structure

Proposed source folder layout for the full HRM system as currently analyzed — Employee Management, Organization Management, Salary Master Data, and Salary Grade Promotion — mapped to the [C4 diagrams](../c4/README.md) and [Arc42 Architecture Decisions](../Arc42/09-architecture-decisions.md).

Designed from the current C4 model, [Database Design](../Database/README.md), [API Documentation](../API/README.md), and UI/UX design ([Information Architecture](../UI-UX/InformationArchitecture_HRM.md) → Screens Hierarchy → UI/UX) and Use Cases/User Stories for all four modules. The existing Class/Sequence Diagrams and the actual `backend/` implementation predate this analysis (Salary Grade Promotion only, against an earlier schema) and are not used as a reference here — they are being brought up to date separately, one part at a time.

- [`FrontendStructure.md`](FrontendStructure.md) — React (`frontend/`), feature-based folders, covering all four modules.
- [`BackendStructure.md`](BackendStructure.md) — ASP.NET Core, 3-layer (`backend/`), covering all four modules.
