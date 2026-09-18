# Information Architecture - HRM System

This document defines the page-level information architecture and sitemap of the HRM system based on the modules currently specified in `Docs/Requirements/`.

The current scope covers:

* **Employee Management**
* **Organization Management**
* **Salary Management**

  * Salary Grade Promotion
  * Salary Master Data

**Identity & Access Management (IAM)** is recognized as a separate HRM module but is not included in the current sitemap because its user stories, use cases, and UI requirements have not yet been specified.

This document focuses on **page-level information organization and hierarchy**. It does not define detailed screen behavior, dialogs, confirmation flows, UI states, or task navigation. Those concerns are handled separately in each module's Screens Hierarchy and subsequent UI/UX specifications.

---

## 1. Information Architecture Principles

The information architecture is organized using the following principles:

* **Organization** — pages are grouped according to their business domain and information relationships.
* **Hierarchy** — parent-child relationships reflect how users understand and access related business information.
* **Labeling** — page and navigation labels use consistent business terminology from the requirements.
* **Navigation** — major business objects have clear and predictable locations within the system.
* **Findability** — frequently accessed business objects are represented by appropriate landing pages.
* **Task Alignment** — the architecture provides locations for the information required to support the defined use cases without mapping every use case to a separate page.
* **Consistency** — the same business concept is represented consistently across modules.
* **Scalability** — the structure allows future HRM capabilities to be added without reorganizing unrelated modules.

---

## 2. Sitemap

```mermaid
flowchart TD
    Root([HRM System])

    Root --> Dashboard[Dashboard - TBD]
    Root --> EM[[Employee Management]]
    Root --> ORG[[Organization]]
    Root --> COMP[[Salary Management]]

    %% Employee Management
    EM --> EMP[Employees]
    EMP --> ED[Employee Detail]

    %% Organization Management
    ORG --> OS[Organization Structure]
    ORG --> JT[Job Titles]

    %% Salary Management
    COMP --> SGP[[Salary Grade Promotion]]
    COMP --> SMD[[Salary Master Data]]

    %% Salary Grade Promotion
    SGP --> RP[Review Periods]
    SGP --> SD[Salary Decisions]
    SGP --> SH[Salary History]

    RP --> RPD[Review Period Detail]
    RPD --> ERD[Employee Review Detail]

    SD --> SDD[Salary Decision Detail]

    %% Salary Master Data
    SMD --> BSR[Base Salary Rate]
    SMD --> SS[Salary Scales]

    SS --> SSD[Salary Scale Detail]
    SSD --> SG[Salary Grades]
```

---

## 3. Page Hierarchy

### HRM System

#### Dashboard

The Dashboard is retained as a **TBD placeholder** for future global HRM navigation.

No Dashboard-specific user stories or use cases have been defined in the current requirements, so its content and functionality are outside the current specified scope.

---

### Employee Management

#### Employees

Landing page for employee profiles.

Supports access to the employee collection, including searching and filtering employees. Employee creation may be initiated from this location, but whether creation uses a modal, dialog, drawer, or separate screen is defined later in the Employee Management Screens Hierarchy.

##### Employee Detail

Displays an individual employee profile and provides access to operations applicable to that employee.

Update and employment-status actions belong to the employee context but do not require separate IA nodes unless later screen design determines that they need independent page locations.

---

### Organization

#### Organization Structure

Represents the hierarchical structure of Organizational Units.

Organizational Units are managed within this organizational structure rather than through a separate top-level page for each unit.

Create, update, move, deactivate, and reactivate operations are screen-level interactions and therefore are not represented as separate IA nodes.

#### Job Titles

Represents the organization-wide Job Title catalog.

Job Titles are independent of individual Organizational Units in the current domain model.

Create, update, deactivate, and reactivate operations are handled within the Job Titles screen structure rather than represented as separate IA nodes.

---

### Salary Management

Salary Management contains the two currently specified salary-related capabilities:

* **Salary Grade Promotion**
* **Salary Master Data**

Keeping both under Salary Management preserves the domain boundary defined by the requirements while separating transactional promotion workflows from salary reference data.

#### Salary Grade Promotion

##### Review Periods

Landing page for Salary Review Periods.

Provides access to existing Review Periods and the entry point for creating a new Review Period.

###### Review Period Detail

Represents a specific Salary Review Period and its review workflow.

It provides the context for reviewing eligible employees, recording review outcomes, submitting the period, and performing other actions allowed by the Review Period's current state.

###### Employee Review Detail

Provides detailed review information for an employee within a specific Review Period.

It is represented as a dedicated page-level information location because reviewing an employee requires a distinct context for examining the employee's current Salary Grade, Proposed Grade, eligibility information, review outcome, and related salary information within the selected Review Period.

##### Salary Decisions

Landing page for Salary Decisions.

Provides access to existing Salary Decisions and allows the Approver / Manager to locate and resume Draft Decisions.

###### Salary Decision Detail

Represents a specific Salary Decision regardless of its lifecycle state.

The same information location may support different behavior depending on status:

* `DRAFT` — editable and may be applied or cancelled.
* `APPLIED` — read-only terminal state.
* `CANCELLED` — read-only terminal state.

Creation, editing, application, and cancellation are actions or states of a Salary Decision rather than separate IA pages.

##### Salary History

Provides access to employee Salary Grade history.

Detailed navigation from employee reviews, Salary Decisions, or other related screens is defined in Screens Hierarchy or Navigation Flow documentation rather than in this sitemap.

---

#### Salary Master Data

##### Base Salary Rate

Provides access to the company's Base Salary Rate records and their effective-dated history.

Adding a new Base Salary Rate is an operation within this information area and does not require a separate IA page.

##### Salary Scales

Landing page for Salary Scales.

Provides access to the collection of Salary Scales and the entry point for creating a new Salary Scale.

###### Salary Scale Detail

Represents a specific Salary Scale and provides the context for maintaining the scale and its Salary Grades.

###### Salary Grades

Salary Grades belong to a specific Salary Scale and are therefore organized within Salary Scale Detail rather than exposed as an independent top-level catalog.

Creating, updating, deactivating, and reactivating Salary Grades are interactions within this context and do not require separate IA pages.

---

## 4. Page List

| Page / Information Location | Type           | Domain                  | Parent                 | Current Design Status         |
| ---------------------------- | -------------- | ----------------------- | ---------------------- | ------------------------------ |
| Dashboard                   | Page           | HRM                     | HRM System             | TBD — no current requirements |
| Employees                   | Page           | Employee Management     | Employee Management    | Prototyped (HTML)             |
| Employee Detail             | Page           | Employee Management     | Employees              | Prototyped (HTML)             |
| Organization Structure      | Page           | Organization Management | Organization           | Prototyped (HTML)             |
| Job Titles                  | Page           | Organization Management | Organization           | Prototyped (HTML)             |
| Review Periods              | Page           | Salary Grade Promotion  | Salary Grade Promotion | Prototyped (HTML)             |
| Review Period Detail        | Page           | Salary Grade Promotion  | Review Periods         | Prototyped (HTML)             |
| Employee Review Detail      | Page           | Salary Grade Promotion  | Review Period Detail   | Prototyped (HTML)             |
| Salary Decisions            | Page           | Salary Grade Promotion  | Salary Grade Promotion | Prototyped (HTML)             |
| Salary Decision Detail      | Page           | Salary Grade Promotion  | Salary Decisions       | Prototyped (HTML)             |
| Salary History              | Page           | Salary Grade Promotion  | Salary Grade Promotion | Prototyped (HTML)             |
| Base Salary Rate            | Page           | Salary Master Data      | Salary Master Data     | Prototyped (HTML)             |
| Salary Scales               | Page           | Salary Master Data      | Salary Master Data     | Prototyped (HTML)             |
| Salary Scale Detail         | Page           | Salary Master Data      | Salary Scales          | Prototyped (HTML)             |
| Salary Grades               | Nested Section | Salary Master Data      | Salary Scale Detail    | Prototyped (HTML)             |

---

## 5. Navigation and Page-Level Actions

This sitemap represents **information locations and their hierarchy**, not every possible navigation path or user action.

Actions such as:

* Create Employee
* Update Employee
* Change Employment Status
* Create or Move Organizational Unit
* Create or Update Job Title
* Create Review Period
* Review Proposed Grade
* Submit or Cancel Review Period
* Create Salary Decision
* Apply or Cancel Salary Decision
* Add Base Salary Rate
* Create or Update Salary Scale
* Create or Update Salary Grade

do not automatically require separate IA nodes.

Whether an action is implemented using a modal, dialog, drawer, dedicated screen, or interaction within an existing page is determined during **Screens Hierarchy and detailed UI/UX design**.

Similarly, task-oriented navigation such as:

```text
Review Period Detail
    → Create Salary Decision
    → Salary Decision Detail
```

or:

```text
Employee Review Detail
    → View Salary History
    → Salary History
```

belongs to Screens Hierarchy / Navigation Flow documentation rather than the core sitemap.

---

## 6. Cross-Module References

The HRM modules reference data owned by other modules.

Examples include:

* Employee Profile references an Organizational Unit and Job Title.
* Salary Grade Promotion references employee information.
* Salary Grade Promotion references Salary Scales and Salary Grades.
* Salary History is associated with individual employees.

These data relationships do not automatically imply navigation relationships.

Cross-module navigation should be introduced only when it supports a confirmed user task or navigation need. A data reference alone is not sufficient reason to create a page-to-page navigation relationship in the IA.

---

## 7. Scope Notes

* **Employee Management** owns Employee Profile information and employee-related profile operations.
* **Organization Management** owns Organizational Units and the organization-wide Job Title catalog.
* **Salary Management** contains both Salary Grade Promotion and Salary Master Data.
* **Salary Master Data** owns Base Salary Rate, Salary Scales, and Salary Grades.
* **Salary Grade Promotion** owns Review Periods, review outcomes, Salary Decisions, and promotion workflow information.
* **IAM** remains a separate HRM module but is outside the current IA scope until its requirements are specified.
* Authentication, authorization, roles, and permissions should not be modeled as Employee Management functionality.

---

## 8. References

### Employee Management

* `Docs/Requirements/UserStories_EmployeeProfile.md`
* `Docs/Requirements/UseCase_EmployeeProfile.md`

### Organization Management

* `Docs/Requirements/UserStories_OrganizationManagement.md`
* `Docs/Requirements/UseCase_OrganizationManagement.md`

### Salary Management — Salary Grade Promotion

* `Docs/Requirements/UserStories_SalaryGradePromotion.md`
* `Docs/Requirements/UseCase_SalaryGradePromotion.md`

### Salary Management — Salary Master Data

* `Docs/Requirements/UserStories_SalaryMasterData.md`
* `Docs/Requirements/UseCase_SalaryMasterData.md`
