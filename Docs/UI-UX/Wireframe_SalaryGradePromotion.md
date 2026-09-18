# Wireframe & Screen Behavior - Salary Grade Promotion

This document expands `ScreensHierarchy_SalaryGradePromotion.md` into concrete page behavior, interaction states, form content, list columns, validation behavior, and data references.

For UI conventions shared across all HRM modules, see `UXGuidelines_HRM.md`.

## 1. Review Period List

**Main Behavior**
- Allows searching and filtering Review Periods by Review Date range, Review Type, and Status (US-SGP-02).
- Displays the Review Period information required for users to identify and access an existing period.
- "Create Review Period" opens the Create Review Period modal.
- Clicking "View" opens Review Period Detail.
- The Effective Date column shows the Review Period's target Effective Date when one is defined. This value is later offered as the default Effective Date when creating a Salary Decision from the period (US-SGP-06 AC07).
- Row actions depend on the Review Period's current state.
- CANCELLED and CLOSED periods are read-only.
- Review Periods are not hard-deleted once review or decision data exists; lifecycle changes use the defined business statuses and cancellation flow.

### Create Review Period
- Captures:
  - Code
  - Name
  - Review Type
  - Review Date
  - Effective Date (optional)
  - Description (optional)
- Code, Name, Review Type, and Review Date are required.
- Creating the Review Period also creates the employee review snapshot and system-generated promotion proposals according to the eligibility rules.
- Successful creation results in an `IN_PROGRESS` Review Period.
- If validation or a business rule fails, the Review Period is not created and the modal remains open with feedback.
- Successful creation closes the modal and returns to Review Period List.

**Data References:** Review Period, Employee Review.

## 2. Review Period Detail

**Main Behavior**
- Serves as the primary HR Staff working screen for processing Employee Reviews within a Review Period.
- The page header displays:
  - Code
  - Name
  - Review Type
  - Review Date
  - Effective Date, if defined
  - Description, if defined
  - Status
- Loads the Employee Review snapshot created for the period.
- Displays each employee's salary snapshot at the Review Date, including Current Grade / Coefficient and the system-generated Proposed Grade / Coefficient when eligible.
- Allows filtering the employee list by:
  - Organizational Unit
  - Eligibility
  - Review Outcome
- Clicking an Employee Review row opens Employee Review Detail.
- Review actions are available only while the Review Period is `IN_PROGRESS`.
- Once the period is `SUBMITTED`, review outcomes are locked.

### Bulk Approve
- Available for selected Employee Reviews while the period is `IN_PROGRESS`.
- Does not require a reason.
- Each selected Employee Review is validated independently.
- Valid Employee Reviews are updated to Approved.
- Invalid Employee Reviews remain unchanged.
- The result clearly reports which selected records succeeded and which failed.
- If a previously Rejected Employee Review is successfully changed to Approved, its rejection reason associated with the current outcome is cleared.

### Bulk Reject
- Available for selected Employee Reviews while the period is `IN_PROGRESS`.
- Requires one rejection reason.
- The same reason is recorded for every selected Employee Review that is successfully rejected.
- Each selected Employee Review is validated independently.
- Invalid Employee Reviews remain unchanged.
- Failure of one selected record does not roll back successful updates to other valid selected records.
- If no rejection reason is provided, the action is rejected and no selected Employee Review outcome is changed.
- The result clearly reports which selected records succeeded and which failed.

### Submit Review Period
- Available while the Review Period is `IN_PROGRESS`.
- Submission is allowed only when every eligible Employee Review with a Proposed Grade has been processed with an Approved or Rejected outcome.
- Ineligible Employee Reviews do not require an outcome.
- Submission requires confirmation.
- Successful submission changes the Review Period status to `SUBMITTED`.
- Employee Review outcomes become read-only after submission.
- If the submission conditions are not satisfied, the status remains unchanged and the confirmation dialog stays open with feedback.

### Cancel Review Period
- An `IN_PROGRESS` Review Period may be cancelled.
- A `SUBMITTED` Review Period may be cancelled only if no non-cancelled Salary Decision exists for that period.
- If cancellation is blocked, the Review Period remains unchanged and the dialog displays the reason.
- Successful cancellation changes the status to `CANCELLED`.
- `CANCELLED` and `CLOSED` are terminal states.

### Decision Access
- Salary Decision creation belongs to the Approver / Manager workflow.
- When an authorized Approver / Manager accesses a `SUBMITTED` Review Period and no non-cancelled Salary Decision exists, "Create Decision" may be used to start a Salary Decision with this Review Period already selected.
- If a non-cancelled Salary Decision already exists, the action becomes "View Decision" and opens the existing decision.
- HR Staff's Review Period processing flow ends at submission; this decision access does not grant Salary Decision actions to HR Staff.

**Data References:** Review Period, Employee Review, Employee, Employee Salary, Salary Grade, Organizational Unit.

## 3. Employee Review Detail

**Main Behavior**
- Displays the Employee Review snapshot captured for the Review Period rather than replacing it with the employee's later current salary data.
- Displays:
  - Employee information
  - Organizational Unit
  - Salary Scale
  - Current Grade at Review Date
  - Current Coefficient at Review Date
  - Eligibility
  - Proposed Grade and Coefficient, when eligible
  - Current Review Outcome
  - Rejection Reason, when applicable
- The Proposed Grade is system-calculated from the employee's valid Salary Scale and promotion eligibility rules.
- HR Staff reviews the system-generated proposal and does not manually enter or modify the Proposed Grade.
- If the Employee Review is ineligible, the reason is clearly displayed and Approve / Reject actions are unavailable.
- Approving or rejecting an Employee Review changes only the review outcome; it does not immediately change the employee's recorded salary grade.
- Review actions are available only while the containing Review Period is `IN_PROGRESS`.

### Approve Employee
- Changes the Employee Review outcome to Approved when the review is valid for processing.
- Does not require a reason.
- If the previous outcome was Rejected, the rejection reason associated with that outcome is cleared after successful approval.

### Reject Employee
- Opens the Reject Employee dialog.
- Requires a rejection reason specific to this employee.
- If the reason is missing or validation fails, the Employee Review remains unchanged and the dialog stays open with feedback.
- Successful rejection records the reason and changes the outcome to Rejected.

### Salary History Context
- As a UI design decision, a compact read-only salary history may be shown alongside the Employee Review information to provide HR Staff with additional context without leaving the screen.
- This contextual presentation does not allow salary-history records to be edited.

**Data References:** Employee Review, Employee Salary, Salary Grade, Salary Scale, Employee.

## 4. Salary Decision List

**Main Behavior**
- Serves as the landing page when the Approver / Manager opens Salary Decisions.
- Lists Salary Decisions regardless of lifecycle state.
- Each row displays sufficient information to identify the decision, including:
  - Decision Number
  - Review Period
  - Effective Date
  - Status
- Decision statuses are:
  - `DRAFT`
  - `APPLIED`
  - `CANCELLED`
- Clicking a `DRAFT` decision opens Salary Decision Detail in editable Draft mode with its previously saved data preserved.
- Clicking an `APPLIED` or `CANCELLED` decision opens Salary Decision Detail in read-only mode.
- "Create New" starts the Pick Review Period flow.
- As a UI design decision, the list may provide Status filtering and search by Decision Number or Review Period to make existing decisions easier to locate.

### Pick Review Period
- Lists only `SUBMITTED` Review Periods that do not already have a non-cancelled Salary Decision.
- Selecting a Review Period starts Salary Decision creation and opens Salary Decision Detail with that Review Period selected.
- The Review Period cannot be replaced with an invalid or unavailable period.

**Data References:** Salary Decision, Review Period.

## 5. Salary Decision Detail

**Main Behavior**
- Reached through either:
  1. A `SUBMITTED` Review Period using "Create Decision", with the Review Period already selected.
  2. Salary Decision List → "Create New" → Pick Review Period.
- Only Employee Reviews with an Approved outcome are eligible for inclusion when the Salary Decision is created.
- The decision contains the employee set selected when the Draft is created.
- While the decision remains `DRAFT`, included employees may be removed.
- Employees outside the Decision's original selected employee set cannot be added to the existing Draft.
- If a different or expanded employee set is required, the Draft must be cancelled and a new Salary Decision created according to the defined workflow.

### Draft Information
- Captures and displays the data required for the Salary Decision, including:
  - Decision Number
  - Review Period
  - Effective Date
  - Included Employees
- Effective Date is pre-filled from the Review Period's target Effective Date when one exists.
- The Approver may change the Effective Date.
- Effective Date must not be earlier than the Review Date.
- Each included employee displays the relevant decision baseline and proposed salary change.

### Save Draft
- Saves the Salary Decision and its included employees in `DRAFT` status.
- Does not change any employee's current salary grade or salary history.
- Reopening the Draft restores its previously saved information and included employee set.

### Issue / Apply Decision
- Available only while the Salary Decision is `DRAFT`.
- Requires a confirmation dialog with an impact summary before the operation is executed.
- Before Apply, the system validates the entire decision, including:
  - Decision Number
  - Effective Date
  - Included employee validity
  - New Grade validity
  - Salary-history conflicts
  - Each employee's current Grade against the Grade baseline captured for the decision
- If an employee's Current Grade no longer matches the expected decision baseline, that employee is invalid for Apply.
- Apply uses all-or-nothing transaction semantics.
- If any included employee fails validation, the entire Apply operation is rejected.
- When Apply fails:
  - No employee salary record is changed.
  - No partial salary update is committed.
  - The Salary Decision remains `DRAFT`.
  - The dialog displays sufficient information about the validation failure.
- When Apply succeeds:
  - The Salary Decision becomes `APPLIED`.
  - The system records each included employee's salary change using the specified Effective Date.
  - The applicable previous salary record is closed.
  - A new salary record is created.
  - Salary history is preserved.
  - The containing Review Period becomes `CLOSED`.
- Applying the decision does not imply a separate future scheduler. The Effective Date represents the effective date of the recorded salary change.

### Cancel Decision
- Available only while the Salary Decision is `DRAFT`.
- Requires confirmation.
- Successful cancellation changes the Salary Decision status to `CANCELLED`.
- Cancelling a Draft does not change employee salary records.
- The associated Review Period remains `SUBMITTED`.
- After cancellation, a new Salary Decision may be created for that Review Period.
- `APPLIED` and `CANCELLED` decisions are terminal and read-only.
- An `APPLIED` Salary Decision cannot be cancelled or hard-deleted.

**Data References:** Salary Decision, Decision Line Item, Employee Salary, Salary Grade, Employee, Review Period.

## 6. Employee Salary History

**Main Behavior**
- Provides read-only salary-history lookup and audit.
- Allows selecting:
  - Employee
  - Date Range
- Displays the employee's current salary record distinctly from historical records.
- Salary history is presented newest to oldest.
- Each salary-history entry displays:
  - Salary Grade
  - Coefficient
  - Effective Date
  - Reason
  - Decision Number, when available
- When a salary-history record is associated with a Salary Decision, the Decision Number can be used to open Salary Decision Detail in read-only mode.
- No salary-history record can be directly created, edited, or deleted from this screen.
- Salary-history changes resulting from Salary Grade Promotion must occur through the Salary Decision workflow.

**Data References:** Employee, Employee Salary, Salary Decision, Salary Grade, Salary Scale.

## Recommended Navigation Flow

**HR Staff**

Review Period List
→ Review Period Detail
→ Employee Review Detail
→ Approve / Reject Employee Reviews
→ Submit Review Period

**Approver / Manager**

Salary Decision List
→ Create New
→ Pick Review Period
→ Salary Decision Detail
→ Save Draft / Issue & Apply
→ Employee Salary History

An authorized Approver / Manager may also enter the decision workflow from a `SUBMITTED` Review Period through "Create Decision".
