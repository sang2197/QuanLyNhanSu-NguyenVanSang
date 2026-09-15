# C4 Model Diagrams

Architecture diagrams for the HRM System, following the [C4 model](https://c4model.com/). Diagrams are written in [Mermaid](https://mermaid.js.org/) and rendered directly by GitHub — no external tool needed to view or diff them.

The C4 model shows the system at 4 levels, from most general to most detailed:

1. **System Context** — the system and who/what uses it. ✅ Done
2. **Container** — the main parts inside the system (e.g. web app, API, database). ✅ Done
3. **Component** — the main parts inside one container. ✅ Done
4. **Code** — class/code level detail. ✅ Done

## 1. System Context Diagram

```mermaid
flowchart LR
    HR([HR Staff<br/>Person])
    APR([Approver<br/>Person])
    SYS[["HRM System<br/>Software System<br/>Manages employee information and HR processes"]]

    HR -->|Uses to manage HR operations| SYS
    APR -->|Reviews and approves| SYS
```

Shows the HRM System and its 2 main users:

- **HR Staff** — performs day-to-day HR operations.
- **Approver** — has authority to review and make final decisions on HR requests.

## 2. Container Diagram

```mermaid
flowchart TB
    HR([HR Staff])
    APR([Approver])

    subgraph HRM["HRM System"]
        WEB[["HRM Web Application<br/>Container: JavaScript / Angular<br/>Provides the UI for HR operations"]]
        API[["HRM Backend API<br/>Container: ASP.NET Core<br/>Handles HR business logic"]]
        DB[("HRM Database<br/>Container: SQL Server<br/>Stores HRM operational data")]
    end

    HR -->|Use| WEB
    APR -->|Use| WEB
    WEB -->|Make API request<br/>HTTPS/REST/JSON| API
    API -->|Reads from and writes to<br/>SQL| DB
```

Shows the 3 main containers inside the HRM System:

- **HRM Web Application** (JavaScript / Angular) — provides the user interface for HR operations.
- **HRM Backend API** (ASP.NET Core) — handles HR business logic and exposes APIs to client applications.
- **HRM Database** (SQL Server) — stores HRM operational data.

The Web Application calls the Backend API over HTTPS/REST/JSON, and the Backend API reads/writes the Database over SQL.

## 3. Component Diagram

```mermaid
flowchart TB
    WEB([HRM Web Application])

    subgraph API["HRM Backend API"]
        EMP[["Employee Management<br/>Component<br/>Manages employee profiles and employment information"]]
        SAL[["Salary Management<br/>Component<br/>Handles salary grades, reviews, decisions, and history"]]
    end

    DB[("HRM Database")]

    WEB -->|HTTPS/REST/JSON| EMP
    WEB -->|HTTPS/REST/JSON| SAL
    EMP -->|Reads from and writes to SQL| DB
    SAL -->|Reads from and writes to SQL| DB
    SAL -.->|Provides employee information| EMP
```

Shows the 2 main components inside the **HRM Backend API** container:

- **Employee Management** — manages employee profiles and employment information.
- **Salary Management** — handles salary grades, salary reviews, salary decisions, and salary history.

Both components are called by the Web Application over HTTPS/REST/JSON and read/write the Database over SQL. Salary Management also reads employee information from Employee Management.

## 4. Code Diagram

```mermaid
flowchart TB
    subgraph SAL["Salary Management"]
        CTRL[["SalaryReviewController<br/>Class<br/>Handles salary review requests"]]
        SVC[["SalaryReviewService<br/>Class<br/>Handles salary review business logic"]]
        REPO[["SalaryRepository<br/>Class<br/>Handles salary data access"]]
    end

    CTRL -->|Delegates business processing to| SVC
    SVC -->|Accesses salary data through| REPO
```

Zooms into the **Salary Management** component, showing its main classes:

- **SalaryReviewController** — handles salary review requests.
- **SalaryReviewService** — handles salary review business logic.
- **SalaryRepository** — handles salary data access.

The flow is: Controller delegates to Service, Service accesses data through Repository.

## Legacy source files

The `.drawio` and `.png` files in this folder (`System-context-diagram.*`, `Container-diagram.*`, `Component-diagram.*`, `Code-diagram.*`) are the original draw.io versions these Mermaid diagrams were standardized from. They are kept for now as a historical reference but are no longer the source of truth — update the Mermaid blocks above when the architecture changes.
