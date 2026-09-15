# 3. System Scope and Context

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System (Salary Grade Promotion).*

**Business context** — see the [System Context Diagram](../c4/README.md#1-system-context-diagram):

- **HR Staff** (person) — performs day-to-day HR operations, creates review periods, and screens the system's proposed grades for each employee (approves or rejects them) before submitting the batch.
- **Approver / Manager** (person) — reviews a submitted review period, drafts a salary decision from the approved employees, and applies it to make the decision official.
- **HRM System** (software system) — manages employee information and HR processes.

**Technical context** — see the [Container Diagram](../c4/README.md#2-container-diagram): the Web Application calls the Backend API over HTTPS/REST/JSON; the Backend API reads/writes the Database over SQL.

No external systems (e.g. an external identity provider, email/notification service) have been identified yet — [ADR-07](09-architecture-decisions.md#adr-07-authentication-and-authorization-mechanism) proposes a self-contained authentication mechanism (ASP.NET Core Identity, backed by the same Database), so no external interface is introduced by it. This should be revisited if that decision changes.
