# Wireframe & Screen Behavior - Employee Profile

This document expands the Employee Profile screens defined in [ScreensHierarchy_EmployeeProfile.md](ScreensHierarchy_EmployeeProfile.md) into concrete UI behavior and form content.

For UI conventions shared across HRM modules, see [UXGuidelines_HRM.md](UXGuidelines_HRM.md).

## 1. Employee List

**Main Behavior**

* Allows HR Staff to search by Employee Code or Employee Name (US-EMP-02).
* Allows filtering by Organizational Unit, Job Title, and Employment Status (US-EMP-02).
* Search and filters can be combined.
* "Create Employee" opens the Create Employee Profile modal.
* Clicking an employee row opens Employee Detail.

**Data References:** Employee Profile, Organizational Unit, Job Title.

## 2. Create Employee Profile

**Main Behavior**

* Captures the required employee profile information:

  * Employee Code
  * Full Name
  * Organizational Unit
  * Job Title
  * Join Date
  * Employment Status
* All fields above are required (US-EMP-01).
* Employee Code must be unique.
* Only active Organizational Units and active Job Titles can be selected.
* If validation or a business rule fails, the employee profile is not created and the modal remains open with applicable feedback.
* After a successful save, the modal closes and the user returns to Employee List.

**Data References:** Employee Profile, Organizational Unit, Job Title.

## 3. Employee Detail

**Main Behavior**

* Displays the employee's recorded profile information.
* Displays the employee's current Organizational Unit, current Job Title, and current Employment Status (US-EMP-03).
* "Edit" opens Update Employee Profile.
* "Change Employment Status" opens Change Employment Status.

**Data References:** Employee Profile, Organizational Unit, Job Title.

## 4. Update Employee Profile

**Main Behavior**

* Allows HR Staff to update the employee's profile information:

  * Employee Code
  * Full Name
  * Organizational Unit
  * Job Title
  * Join Date
* Employment Status is not editable in this form; status changes are handled separately through Change Employment Status (US-EMP-05).
* Employee Code must remain unique.
* Only active Organizational Units and active Job Titles can be selected for assignment.
* If validation or a business rule fails, the existing employee information remains unchanged and the modal remains open with applicable feedback.
* After a successful save, the modal closes and the user returns to Employee Detail.

**Data References:** Employee Profile, Organizational Unit, Job Title.

## 5. Change Employment Status

**Main Behavior**

* Allows HR Staff to change the employee's Employment Status according to the supported status rules (US-EMP-05).
* Supported statuses are:

  * Active
  * On Leave
  * Terminated
* Changing Employment Status does not delete the employee profile or its existing information.
* After a successful confirmation, the dialog closes and the user returns to Employee Detail.
* If the requested change is not permitted, the existing status remains unchanged and the dialog displays applicable feedback.

> **Open Item — OQ-EMP-01:** Whether a Terminated employee can later return to Active using the same profile, or must go through a separate rehiring process, is unresolved. Until this is resolved, the UI must not introduce a reactivation-from-Terminated workflow.

**Data References:** Employee Profile.
