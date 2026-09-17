# State Diagrams - Salary Grade Promotion

Status lifecycles already defined as enums in [`openapi.yaml`](../API/openapi.yaml), shown visually.

## Review Period Status

```mermaid
stateDiagram-v2
    [*] --> IN_PROGRESS : Create (US-01) — proposed grades calculated in the same request, no separate Draft status
    IN_PROGRESS --> SUBMITTED : Submit (US-05) — only when every eligible employee is processed
    SUBMITTED --> CLOSED : Its salary decision is applied (US-07)
    IN_PROGRESS --> CANCELLED : Cancel (US-11)
    SUBMITTED --> CANCELLED : Cancel (US-11) — blocked if a decision already exists
```

CLOSED and CANCELLED are terminal — neither can transition anywhere else. In
particular CLOSED never reverts to SUBMITTED, because the salary decision
that closed it can never be cancelled once Applied (see the Salary Decision
Status diagram below and US-10).

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
    DRAFT --> CANCELLED : Cancel (US-10) — only a Draft decision can be cancelled
```

APPLIED and CANCELLED are both terminal — an Applied decision can never be
cancelled (US-10), and there is no un-cancel action.
