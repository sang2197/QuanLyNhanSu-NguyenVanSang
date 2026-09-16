# Detailed Design

Class-level and behavior-level design for the Salary Grade Promotion API, derived from [Code Structure](../CodeStructure/README.md), [API Documentation](../API/README.md), [Database Design](../Database/README.md), and [C4 Architecture Diagrams](../c4/README.md). The last design artifact before implementation begins.

- [`ClassDiagram.md`](ClassDiagram.md) — Domain model (8 entities) and the API/Service/Repository class layer for Salary Management.
- [`SequenceDiagrams.md`](SequenceDiagrams.md) — 5 key flows: approve an employee, submit a review period, apply a decision (all-or-nothing), look up salary history, list/start/resume a salary decision.
- [`StateDiagrams.md`](StateDiagrams.md) — Status lifecycles for Review Period, Review Outcome, and Salary Decision.
