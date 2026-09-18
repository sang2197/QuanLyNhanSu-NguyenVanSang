# 7. Deployment View

*Part of the [Arc42 Architecture Documentation](README.md) - HRM System.*

> **Status:** Draft (planned deployment, no real infrastructure) · **Owner:** Sang2197 · **Last Reviewed:** 2026-09-18 · **Implementation Baseline Commit:** `77e5716`

This is the **planned** deployment (no real infrastructure exists yet):

- **HRM Web Application** — deployed as a static React build, served via a web server (e.g. IIS or a CDN).
- **HRM Backend API** — deployed as a single ASP.NET Core application (e.g. IIS or a container), reachable by the Web Application over HTTPS. All four Section 5 components (Employee Management, Organization Management, Salary Master Data, Salary Grade Promotion) are co-deployed inside this one process/container — they are not separately deployable units.
- **HRM Database** — a SQL Server instance, reachable only by the Backend API (not directly by the Web Application). For local development the Backend API connects to SQL Server LocalDB through the `ConnectionStrings:HrmDatabase` setting (database `HrmSystem`).
- **TLS termination** *(assumption, pending real infrastructure)*: TLS is terminated at a reverse proxy / load balancer placed in front of the Backend API (e.g. IIS Application Request Routing, Azure Application Gateway, or Nginx, depending on final hosting choice). Traffic from the reverse proxy to the Backend API process runs on the internal network.
- **Operability mechanism** (supports the `#operable` goal from [Section 1.2](01-introduction-and-goals.md#12-quality-goals)): the Backend API's health-check endpoint and structured logging ([Section 8](08-crosscutting-concepts.md)) are intended to let it be monitored and restarted independently of the Web Application and Database. **Not yet implemented** — see [DEBT-01](11-risks-and-technical-debt.md#technical-debt).
