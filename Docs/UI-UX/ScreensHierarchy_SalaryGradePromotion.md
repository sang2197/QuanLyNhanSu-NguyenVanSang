# Screens Hierarchy - Salary Grade Promotion

This document expands the [Information Architecture](InformationArchitecture_SalaryGradePromotion.md) sitemap into every individual screen state a user will actually encounter — including modals, confirmation dialogs, and read-only variants that a page-level sitemap does not show. Each transition is labeled with the user action that triggers it, so this becomes the direct blueprint for the visual design that follows.

Two kinds of nodes:
- **Page** — a full navigable screen with its own URL/location.
- **Modal / Dialog** — a transient overlay on top of a page; closing it returns to that same page.

## Hierarchy Diagram

```mermaid
flowchart TD
    Menu([Menu])

    Menu -- "Review Periods" --> RPL[Page: Review Period List]
    Menu -- "Salary Decisions" --> SDL[Page: Salary Decision List]
    Menu -- "Salary History" --> ESH[Page: Employee Salary History]

    subgraph G1["Review Period List"]
        RPL
        M_CreatePeriod{{"Modal: Create Review Period"}}
        M_CancelPeriod{{"Dialog: Cancel Review Period"}}
    end
    RPL -- "Create Review Period" --> M_CreatePeriod
    M_CreatePeriod -- "Save" --> RPL
    RPL -- "Cancel (row action)" --> M_CancelPeriod
    M_CancelPeriod -- "Confirm" --> RPL
    RPL -- "View" --> RPD[Page: Review Period Detail]

    subgraph G2["Review Period Detail"]
        RPD
        M_Submit{{"Dialog: Submit for Approval"}}
        M_BulkApprove{{"Dialog: Bulk Approve"}}
        M_BulkReject{{"Dialog: Bulk Reject (reason required)"}}
    end
    RPD -- "Open employee row" --> ERD[Page: Employee Review Detail]
    RPD -- "Submit for Approval" --> M_Submit
    M_Submit -- "Confirm" --> RPD
    RPD -- "Bulk Approve selected" --> M_BulkApprove
    M_BulkApprove -- "Confirm" --> RPD
    RPD -- "Bulk Reject selected" --> M_BulkReject
    M_BulkReject -- "Submit reason" --> RPD
    RPD -- "Create Decision / View Decision" --> CED[Page: Create/Edit Salary Decision]

    subgraph G3["Employee Review Detail"]
        ERD
        M_Reject{{"Dialog: Reject (reason required)"}}
    end
    ERD -- "Approve" --> ERD
    ERD -- "Reject" --> M_Reject
    M_Reject -- "Submit reason" --> ERD
    ERD -- "Back" --> RPD

    subgraph G4["Salary Decision List"]
        SDL
        M_PickPeriod{{"Dialog: Pick a Review Period"}}
    end
    SDL -- "Create New" --> M_PickPeriod
    M_PickPeriod -- "Select period" --> CED
    SDL -- "Open a draft row" --> CED
    SDL -- "Open an applied/cancelled row" --> CED

    subgraph G5["Create/Edit Salary Decision"]
        CED
        M_Apply{{"Dialog: Issue/Apply Decision (impact summary)"}}
        M_CancelDecision{{"Dialog: Cancel Decision"}}
    end
    CED -- "Save Draft" --> CED
    CED -- "Remove an employee" --> CED
    CED -- "Issue / Apply" --> M_Apply
    M_Apply -- "Confirm" --> CED
    CED -- "Cancel Decision (Draft or Applied)" --> M_CancelDecision
    M_CancelDecision -- "Confirm" --> CED

    ESH -- "Open a decision number" --> CED
```

## Screen Inventory

| Screen | Type | Parent | Reached by |
|---|---|---|---|
| Review Period List | Page | Menu | Menu: "Review Periods" |
| Create Review Period | Modal | Review Period List | "Create Review Period" |
| Cancel Review Period | Dialog | Review Period List | Row action "Cancel" |
| Review Period Detail | Page | Review Period List | "View" |
| Submit for Approval | Dialog | Review Period Detail | "Submit for Approval" |
| Bulk Approve | Dialog | Review Period Detail | "Bulk Approve selected" |
| Bulk Reject | Dialog | Review Period Detail | "Bulk Reject selected" |
| Employee Review Detail | Page | Review Period Detail | "Open employee row" |
| Reject (employee) | Dialog | Employee Review Detail | "Reject" |
| Salary Decision List | Page | Menu | Menu: "Salary Decisions" |
| Pick a Review Period | Dialog | Salary Decision List | "Create New" |
| Create/Edit Salary Decision | Page | Salary Decision List *(also reachable from Review Period Detail and Employee Salary History)* | "Create New" (after picking a period), "Open a draft/applied/cancelled row", "Create/View Decision", "Open a decision number" |
| Issue/Apply Decision | Dialog | Create/Edit Salary Decision | "Issue / Apply" |
| Cancel Decision | Dialog | Create/Edit Salary Decision | "Cancel Decision" (available while Draft or Applied) |
| Employee Salary History | Page | Menu | Menu: "Salary History" |

## Notes

- Every dialog returns the user to the exact page it was opened from — none of them navigate elsewhere.
- "Create/Edit Salary Decision" is the only page reachable from more than one parent; it behaves differently depending on how it was reached (a fresh draft, a resumed draft, or a read-only view of something already decided), but it is still one screen, not three.
- "Employee Review Detail" and "Review Period Detail" link back and forth to each other rather than only going one direction.
