# State Diagrams - HRM System

UML state machine diagrams for every status-bearing entity across the full HRM system, derived from the `Status`/enum fields in [Database Design](../Database/README.md) and [`openapi.yaml`](../API/openapi.yaml), and the guard conditions in each module's Use Cases.

**Notation** — transitions are written in UML's `event [guard] / effect` form: a short event name, an optional `[guard]` stating the condition that must hold for the transition to fire (phrased positively, as what *allows* it, not what blocks it), and an optional `/ effect` for a side effect worth calling out. User Story/Acceptance-Criteria/Business-Rule references and any longer explanation are kept out of the diagram and given as prose underneath instead.

## 1. Review Period Status (`HrSalaryReviewPeriod.Status`)

```mermaid
stateDiagram-v2
    [*] --> IN_PROGRESS : Create
    IN_PROGRESS --> SUBMITTED : Submit [every eligible employee has an outcome]
    IN_PROGRESS --> CANCELLED : Cancel
    SUBMITTED --> CANCELLED : Cancel [no non-cancelled decision exists]
    SUBMITTED --> CLOSED : Apply
```

- **Create** enters `IN_PROGRESS` directly — proposed grades are calculated in the same request, so there is no separate Draft status (`US-SGP-01`).
- **Submit**'s guard is that every eligible employee with a proposed grade already has an Approved/Rejected outcome (`US-SGP-05`).
- **Cancel** from `SUBMITTED` is guarded by the absence of a non-cancelled Salary Decision (`US-SGP-11`); from `IN_PROGRESS` it is unguarded.
- **Apply** is triggered by the associated Salary Decision's own `Apply` transition succeeding (`US-SGP-07`) — see Salary Decision Status below.
- `CLOSED` and `CANCELLED` are terminal — neither transitions anywhere else. In particular `CLOSED` never reverts to `SUBMITTED`, because an `APPLIED` Salary Decision can never be cancelled.

## 2. Review Outcome (`HrSalaryReviewEmployee.Outcome`, per employee within a period)

```mermaid
stateDiagram-v2
    [*] --> PENDING : Create
    PENDING --> APPROVED : Approve
    PENDING --> REJECTED : Reject [reason provided]
    REJECTED --> APPROVED : Approve / clear rejection reason
    APPROVED --> REJECTED : Reject [reason provided]
```

- **Create** happens as part of the Review Period's own `Create` (Section 1), and only for an employee found eligible — a not-eligible employee has no `Outcome` at all (`ineligibleReason` is populated instead) and never enters this state machine.
- **Approve**/**Reject** both require the containing Review Period to still be `IN_PROGRESS` — not shown as a per-transition guard since it applies uniformly to every transition here (`US-SGP-04` AC10).
- **Reject**'s guard is that a reason was provided (`US-SGP-04` AC02, AC03).
- Changing `REJECTED` back to `APPROVED` clears the previously recorded rejection reason (`US-SGP-04` AC08).

## 3. Salary Decision Status (`HrSalaryDecision.Status`)

```mermaid
stateDiagram-v2
    [*] --> DRAFT : Create
    DRAFT --> APPLIED : Apply / closes the review period
    DRAFT --> CANCELLED : Cancel
```

- **Create**'s guard is a `SUBMITTED` review period with no existing non-cancelled decision, including only its Approved employees (`US-SGP-06`).
- **Apply** applies every included employee's change together (all-or-nothing) and, as a side effect on the associated Review Period, transitions it to `CLOSED` (`US-SGP-07`).
- **Cancel** is only reachable from `DRAFT` (`US-SGP-10`).
- `APPLIED` and `CANCELLED` are both terminal — an Applied decision can never be cancelled, and there is no un-cancel action. After `CANCELLED`, the review period returns to being eligible for a new decision (`US-SGP-10` AC04) — the decision itself does not reopen.

## 4. Employment Status (`HrEmployee.EmploymentStatus`)

```mermaid
stateDiagram-v2
    [*] --> ACTIVE : Create
    ACTIVE --> ON_LEAVE : Change Status [target = ON_LEAVE]
    ON_LEAVE --> ACTIVE : Change Status [target = ACTIVE]
    ACTIVE --> TERMINATED : Change Status [target = TERMINATED]
    ON_LEAVE --> TERMINATED : Change Status [target = TERMINATED]
    note right of TERMINATED : Return to ACTIVE from here is unresolved — OQ-EMP-01
```

- All 4 transitions are the same event (`POST /employees/{id}/employment-status`, `US-EMP-05`), guarded by which target status was requested.
- `TERMINATED` has no outgoing transition in this diagram, by design rather than omission: whether a Terminated employee can return to `ACTIVE` on the same profile, or must be rehired as a new profile, is open question `OQ-EMP-01` in [`UserStories_EmployeeProfile.md`](../Requirements/UserStories_EmployeeProfile.md) — flagged on the note above rather than drawn as a transition, since it is not yet a defined transition at all.
- Changing status never deletes the employee profile or its history (`BR-EMP-11`), regardless of which state it is in.

## 5. Active/Inactive Toggle (shared pattern — `OrganizationalUnit`, `JobTitle`, `SalaryScale`, `SalaryGrade`)

`HrOrganizationalUnit.Status`, `HrJobTitle.Status`, `HrSalaryScale.Status`, and `HrSalaryGrade.Status` all use the same 2-state shape (`ActiveStatus` enum) and the same `Deactivate`/`Reactivate` events, but each has its own guard. One diagram for the shared shape; the table below is this diagram's guard reference, giving the `[condition]` that belongs in each entity's `Deactivate`/`Reactivate` transition.

```mermaid
stateDiagram-v2
    [*] --> ACTIVE : Create
    ACTIVE --> INACTIVE : Deactivate [see guard table]
    INACTIVE --> ACTIVE : Reactivate [see guard table]
```

| Entity | `Deactivate` guard | `Reactivate` guard |
|---|---|---|
| `OrganizationalUnit` | `[no active child units and no active employees assigned]` (`BR-ORG-10`, `BR-ORG-11` — cross-domain, see [ClassDiagram.md](ClassDiagram.md#3-organization-management)) | `[unit has no parent, or its parent is Active]` (`BR-ORG-14`) |
| `JobTitle` | `[always]` — existing holders are unaffected and keep the title (`BR-ORG-19`) | `[always]` (`BR-ORG-20`) |
| `SalaryScale` | `[scale contains no active Salary Grade]` (`BR-SAL-20`) | `[always]` (`BR-SAL-22`) |
| `SalaryGrade` | `[no active employee currently assigned to it]` (`BR-SAL-15` — cross-domain, see [ClassDiagram.md](ClassDiagram.md#4-salary-master-data)) | `[containing Salary Scale is Active]` (`BR-SAL-18`) |

Neither state is ever terminal for these 4 entities — deactivation is always reversible, unlike the lifecycle-driven statuses in Sections 1–4 above. Deactivating never deletes the row or its historical data (`BR-ORG-12`, `BR-ORG-19`, `BR-SAL-14`, `BR-SAL-19`); it only blocks the row from being selected for new assignments (`BR-ORG-12`, `BR-SAL-16`, `BR-SAL-21`) and, for a Salary Grade, causes it to be skipped when Salary Grade Promotion determines a proposed grade (`BR-SAL-17`).
