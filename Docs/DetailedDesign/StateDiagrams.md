# State Diagrams - Salary Grade Promotion

Status lifecycles already defined as enums in [`openapi.yaml`](../API/openapi.yaml), shown visually.

## Review Period Status

```mermaid
stateDiagram-v2
    [*] --> DRAFT : Create (US-01)
    DRAFT --> IN_PROGRESS : System calculates proposed grades
    IN_PROGRESS --> SUBMITTED : Submit (US-05) — only when every employee is processed
    SUBMITTED --> CLOSED : Linked decision is applied (US-07)
```

## Review Outcome (per employee within a period)

```mermaid
stateDiagram-v2
    [*] --> PENDING : Proposed grade calculated
    PENDING --> APPROVED : HR Staff approves (US-04)
    PENDING --> REJECTED : HR Staff rejects with reason (US-04)
```

## Salary Decision Status

```mermaid
stateDiagram-v2
    [*] --> DRAFT : Create decision (US-06)
    DRAFT --> APPLIED : Apply (US-07) — all-or-nothing
    DRAFT --> CANCELLED : Cancel draft
    APPLIED --> CANCELLED : Formal cancellation/revocation (never a hard delete)
```
