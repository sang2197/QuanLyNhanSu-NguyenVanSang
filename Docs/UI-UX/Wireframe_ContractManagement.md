# Wireframe & Screen Behavior - Contract Management

This document expands the Contract Management screens defined in [ScreensHierarchy_ContractManagement.md](ScreensHierarchy_ContractManagement.md) into concrete UI behavior, form content, list columns, validation behavior, and data references. It is derived from `UserStories_ContractManagement.md` and `UseCase_ContractManagement.md`.

For UI conventions shared across HRM modules, see [UXGuidelines_HRM.md](UXGuidelines_HRM.md).

**Action availability and validation.** The business conditions of an action may be used by the UI to disable or hide the action, or to guide the user before submission (for example, not offering Terminated employees when selecting an employee). They are never the only check: the backend always revalidates every condition when the request is submitted, and a rejected request is shown as feedback on the screen that submitted it.

## 1. Contract List

**Main Behavior**

* Is HR Staff's main working screen for contracts.
* Provides one search field that matches Employee Code, Employee Full Name, or Contract Number (US-CON-02).
* Allows filtering by:

  * Contract Type
  * Contract Status
  * Time Period (from date and to date) — matches contracts whose term overlaps the selected period; a contract without an End Date is treated as running from its Start Date without limit (US-CON-02).
* Search and filters can be combined. Contracts of every status are shown unless a Status filter is applied.
* Provides an **Expiring Soon** quick filter with a 30-day or 60-day window; the default window is 30 days (US-CON-05).

  * Shows the Active contracts whose End Date falls from today up to and including the last day of the window.
  * Also shows overdue contracts — Active contracts whose End Date is earlier than today — identified as **Overdue**.
  * Does not show Indefinite-Term contracts or contracts that are not Active.
  * Orders the results by End Date, earliest first.
  * Can be combined with the search and the other filters; all criteria are applied together (US-CON-05).
  * Because it lists only Active contracts, combining it with a Status filter other than Active leaves no results; the UI may disable those Status choices while Expiring Soon is active.
* Displays these columns:

  * Contract Number
  * Employee (Employee Code and Full Name)
  * Contract Type
  * Start Date
  * End Date (empty for Indefinite-Term contracts)
  * Status (status badge)
  * Overdue indicator, when the contract is overdue
* Contract Status uses one consistent status-badge representation for Draft, Active, Expired, and Terminated across the module.
* Because this is the main working list, it is expected to be paginated (UX Guidelines: Lists and Data Presentation). This is a UI/UX design decision, not a business rule.
* "Create Contract" opens the Create Contract modal.
* Clicking a contract row opens Contract Detail. The list has no row actions; status changes are made from Contract Detail.
* If no contract matches the current search and filters, the list indicates that no matching contracts were found (US-CON-02 AC06). If the Expiring Soon filter finds nothing, it indicates that no contracts are expiring soon (US-CON-05 AC06).

**Data References:** Contract, Employee.

## 2. Create Contract

**Main Behavior**

* Captures the contract information (US-CON-01):

  * Employee (required)
  * Contract Type (required): Probation, Fixed-Term, or Indefinite-Term
  * Contract Number (required)
  * Start Date (required)
  * End Date
  * Contract Salary Amount (required)
  * Salary Note (optional, free text)
  * Initial Status (required): Draft or Active
* Employee is selected by searching on Employee Code or Full Name. Terminated employees cannot be selected (BR-CON-07).
* Contract Number must be unique.
* End Date is required for Probation and Fixed-Term contracts and must not be provided for Indefinite-Term contracts (BR-CON-04); the End Date field may be disabled when Indefinite-Term is chosen. End Date must be later than Start Date.
* Contract Salary Amount must be greater than zero. It is an amount recorded from the labor contract and is not derived from, or linked to, salary scales or salary grades.
* Draft is preselected as the Initial Status. Active can be saved only when the Start Date is today or earlier and the employee has no other Active contract (BR-CON-09).
* If validation or a business rule fails, the contract is not created and the modal remains open with applicable feedback; entered values are preserved.
* After a successful save, the modal closes and the user goes to the Contract Detail of the newly created contract.

**Data References:** Contract, Employee.

## 3. Contract Detail

**Main Behavior**

* The page header displays the Contract Number and the Status badge, and identifies the contract as **Overdue** when it is Active and its End Date is earlier than today.
* Displays the contract information (US-CON-03):

  * Contract Type
  * Start Date
  * End Date, if any
  * Contract Salary Amount
  * Salary Note, if any
* Displays the employee's currently recorded information — Employee Code, Full Name, Organizational Unit, Job Title, and Employment Status — not a copy taken when the contract was created (BR-CON-15).
* Provides an "Open employee" link to the related employee's Employee Detail. This is navigation only; it does not change the contract or the employee.
* When the contract is Terminated, also displays the Termination Date and Termination Reason (US-CON-03 AC02).
* Available actions depend on the contract status:

  | Status | Available actions |
  |---|---|
  | Draft | Edit, Delete, Activate |
  | Active | Mark as Expired, Terminate |
  | Expired | None — read-only |
  | Terminated | None — read-only |
* "Back" returns to Contract List.

**Data References:** Contract, Employee, Organizational Unit, Job Title.

## 4. Edit Contract

**Main Behavior**

* Available only for a Draft contract (US-CON-06).
* Allows HR Staff to update:

  * Contract Type
  * Contract Number
  * Start Date
  * End Date
  * Contract Salary Amount
  * Salary Note
* Employee is shown as read-only and cannot be changed (BR-CON-29). If the wrong employee was selected, the Draft contract is deleted and a new contract is created.
* Status is not editable in this form; status changes are made through Activate Contract (US-CON-04).
* The validation rules of Create Contract apply, except the rules about the employee. Contract Number must remain unique; keeping the contract's own number is allowed.
* If validation or a business rule fails, the existing contract information remains unchanged and the modal remains open with applicable feedback.
* After a successful save, the modal closes and the user returns to Contract Detail.

**Data References:** Contract, Employee.

## 5. Delete Contract

**Main Behavior**

* Confirmation Dialog, available only for a Draft contract (US-CON-06).
* Displays the Contract Number so HR Staff can confirm the correct contract before proceeding, and may also display the Employee (Employee Code and Full Name).
* Asks HR Staff to confirm removing the Draft contract, and states that its Contract Number can be used again afterwards. No input is required.
* After a successful confirmation, the contract is removed and the user returns to Contract List, because the contract no longer exists.
* If the contract is no longer Draft, it is not deleted and the dialog displays applicable feedback.

**Data References:** Contract.

## 6. Activate Contract

**Main Behavior**

* Confirmation Dialog, available only for a Draft contract (US-CON-04).
* Displays the Contract Number so HR Staff can confirm the correct contract before proceeding, and may also display the Employee (Employee Code and Full Name).
* Asks HR Staff to confirm activating the contract. No input is required.
* Activation is allowed only when the Start Date is today or earlier, the employee has no other Active contract, and the employee's Employment Status is not Terminated (BR-CON-18).
* After a successful confirmation, the contract becomes Active, the dialog closes, and the user returns to Contract Detail.
* If a condition is not met, the contract remains Draft and the dialog displays applicable feedback.

**Data References:** Contract, Employee.

## 7. Mark as Expired

**Main Behavior**

* Confirmation Dialog, available only for an Active contract (US-CON-04).
* Displays the Contract Number so HR Staff can confirm the correct contract before proceeding, and may also display the Employee (Employee Code and Full Name).
* Asks HR Staff to confirm marking the contract as Expired. No input is required.
* Allowed only when the contract has an End Date that is earlier than today (BR-CON-19).
* After a successful confirmation, the contract becomes Expired and read-only, the dialog closes, and the user returns to Contract Detail. The contract no longer appears as overdue or in the Expiring Soon results.
* If a condition is not met, the contract remains Active and the dialog displays applicable feedback.

**Data References:** Contract.

## 8. Terminate Contract

**Main Behavior**

* Form/Action Modal, available only for an Active contract (US-CON-04).
* Displays the Contract Number so HR Staff can confirm the correct contract before proceeding, and may also display the Employee (Employee Code and Full Name).
* Captures:

  * Termination Date (required)
  * Termination Reason (required, free text)
* Termination Date must not be earlier than the contract's Start Date and, if the contract has an End Date, must not be later than it (BR-CON-22).
* If validation or a business rule fails, the contract remains Active and the modal remains open with applicable feedback; entered values are preserved.
* After a successful submission, the contract becomes Terminated with the Termination Date and Termination Reason recorded, the modal closes, and the user returns to Contract Detail. The contract and its existing information are retained.

**Data References:** Contract.

## Recommended Navigation Flow

**HR Staff — recording a contract**

Contract List
→ Create Contract
→ Contract Detail
→ Activate Contract (when the Start Date is reached and the contract was saved as Draft)

**HR Staff — following up on contracts about to end**

Contract List
→ Expiring Soon
→ Contract Detail
→ Mark as Expired / Terminate Contract

**HR Staff — correcting a Draft contract**

Contract List
→ Contract Detail
→ Edit Contract / Delete Contract
