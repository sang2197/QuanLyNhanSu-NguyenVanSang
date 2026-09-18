# Wireframe & Screen Behavior - Salary Master Data

This document expands the screens defined in `ScreensHierarchy_SalaryMasterData.md` into concrete UI behavior and form content.

For UI conventions shared across HRM modules, see `UXGuidelines_HRM.md`.

## 1. Base Salary Rate

**Main Behavior**
- Displays the current Base Salary Rate and its effective-dated history, ordered from newest to oldest (US-SAL-01).
- The currently applicable rate is visually distinguishable from historical and future-effective records.
- Provides access to "Add New Rate".
- The rate applicable to a given past date can be determined from the effective-dated history (US-SAL-01 AC04).

**Data References:** Base Salary Rate.

## 2. Add Base Salary Rate

**Main Behavior**
- Allows HR Staff to record a new Base Salary Rate (US-SAL-01).
- Captures:
  - Rate
  - Effective Date
- Rate must be greater than zero.
- Effective Date must be later than the latest Effective Date already recorded.
- A new rate cannot be inserted between existing effective-dated records.
- Adding a new rate creates a new effective-dated record; it does not edit or remove historical rates (BR-SAL-02, BR-SAL-03).
- If validation or a business rule fails, no new rate is recorded and the modal remains open with feedback.
- Successful creation closes the modal and returns to Base Salary Rate.

**Data References:** Base Salary Rate.

## 3. Salary Scales

**Main Behavior**
- Provides catalog access to existing Salary Scales for maintenance actions.
- Displays sufficient identifying information to allow HR Staff to access an existing Salary Scale.
- Provides access to "Create Salary Scale".
- Clicking a Salary Scale opens Salary Scale Detail.
- Search, filtering, sorting, and pagination are not implied because those capabilities are not currently defined in the requirements.

**Data References:** Salary Scale.

## 4. Create Salary Scale

**Main Behavior**
- Allows HR Staff to create a Salary Scale (US-SAL-02).
- Captures:
  - Code
  - Name
- Code and Name are required.
- Code and Name must satisfy the uniqueness rules defined for Salary Scales.
- A newly created Salary Scale is Active by default.
- Salary Scale Code is fixed at creation and cannot be changed later.
- If validation or a business rule fails, the Salary Scale is not created and the modal remains open with feedback.
- Successful creation closes the modal and returns to Salary Scales.

**Data References:** Salary Scale.

## 5. Salary Scale Detail

**Main Behavior**
- Displays the selected Salary Scale's:
  - Code
  - Name
  - Status
- Code is read-only after creation.
- Provides access to:
  - Edit Scale
  - Deactivate / Reactivate Scale
  - Add Grade
- Displays the Salary Grades belonging to the scale in Grade Number order.
- Each Salary Grade displays its identifying information, currently applicable coefficient, and current status.
- Provides Grade-level actions according to the Grade's current state:
  - Update Coefficient
  - Deactivate / Reactivate Grade
- Historical coefficient records are preserved when coefficient changes occur. Their storage/history must not be represented as overwriting the previous coefficient.
- "Add Grade" is available only when the Salary Scale is Active.
- Grade actions must respect the business rules defined for Salary Grade status and coefficient changes.

**Data References:** Salary Scale, Salary Grade.

## 6. Update Salary Scale

**Main Behavior**
- Allows HR Staff to update an existing Salary Scale (US-SAL-03).
- Displays Salary Scale Code as read-only.
- Editable field:
  - Name
- Salary Scale Code cannot be changed after creation.
- Updated Name must satisfy the defined uniqueness rule.
- If validation or a business rule fails, the existing Salary Scale remains unchanged and the modal stays open with feedback.
- Successful update closes the modal and returns to Salary Scale Detail.

**Data References:** Salary Scale.

## 7. Deactivate / Reactivate Salary Scale

**Main Behavior**
- Allows HR Staff to change the status of a Salary Scale according to its current state (US-SAL-07).

**Deactivate**
- Deactivation is blocked while the Salary Scale contains any Active Salary Grade.
- If no Active Salary Grade remains, HR Staff can confirm deactivation.

**Reactivate**
- An Inactive Salary Scale can be reactivated according to the defined Salary Scale status rules.
- If validation or a business rule fails, the Salary Scale status remains unchanged and the dialog stays open with feedback.
- Successful confirmation closes the dialog and returns to Salary Scale Detail.

**Data References:** Salary Scale, Salary Grade.

## 8. Create Salary Grade

**Main Behavior**
- Allows HR Staff to create a Salary Grade within the selected Salary Scale (US-SAL-04).
- Available only while the containing Salary Scale is Active.
- Captures:
  - Grade Number
  - Initial Coefficient
- Grade Number must be unique within the Salary Scale.
- Initial Coefficient must be greater than zero.
- The initial coefficient becomes the Grade's current coefficient when the Grade is created; it does not require a separate effective date.
- A newly created Salary Grade is Active by default.
- If validation or a business rule fails, the Salary Grade is not created and the modal remains open with feedback.
- Successful creation closes the modal and returns to Salary Scale Detail.

**Data References:** Salary Scale, Salary Grade.

## 9. Update Salary Grade Coefficient

**Main Behavior**
- Allows HR Staff to record a new coefficient for an existing Salary Grade (US-SAL-05).
- Available only while the Salary Grade is Active (BR-SAL-13A).
- Captures:
  - Coefficient
  - Effective Date
- Coefficient must be greater than zero.
- Effective Date must be later than the latest coefficient Effective Date already recorded for that Grade.
- Updating a coefficient creates a new effective-dated coefficient record; it does not overwrite or remove historical coefficient values (BR-SAL-12, BR-SAL-13).
- If validation or a business rule fails, no new coefficient record is created and the modal remains open with feedback.
- Successful update closes the modal and returns to Salary Scale Detail.

**Data References:** Salary Grade.

## 10. Deactivate / Reactivate Salary Grade

**Main Behavior**
- Allows HR Staff to change the status of a Salary Grade according to its current state (US-SAL-06).

**Deactivate**
- Deactivation is blocked while an Active Employee is currently assigned to the Salary Grade.
- If no Active Employee is currently assigned, HR Staff can confirm deactivation.

**Reactivate**
- Reactivation is allowed only while the containing Salary Scale is Active.
- If the containing Salary Scale is Inactive, reactivation is blocked.
- If validation or a business rule fails, the Salary Grade status remains unchanged and the dialog stays open with feedback.
- Successful confirmation closes the dialog and returns to Salary Scale Detail.

**Data References:** Salary Scale, Salary Grade, Employee Profile.
