# State Diagrams - Salary Grade Promotion

Status lifecycles already defined as enums in [`openapi.yaml`](../API/openapi.yaml), shown visually.

## Review Period Status

```mermaid
stateDiagram-v2
    [*] --> DRAFT : Create (US-01)
    DRAFT --> IN_PROGRESS : Proposed grades calculated — same request, immediately after DRAFT
    IN_PROGRESS --> SUBMITTED : Submit (US-05) — only when every employee is processed
    SUBMITTED --> CLOSED : Linked decision is applied (US-07)
    DRAFT --> CANCELLED : Cancel (US-11)
    IN_PROGRESS --> CANCELLED : Cancel (US-11)
    SUBMITTED --> CANCELLED : Cancel (US-11) — blocked if a decision already exists
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
