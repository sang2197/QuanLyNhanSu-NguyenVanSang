# UX Guidelines - HRM System

This document defines shared UI/UX conventions for the HRM system. Module-specific business behavior remains defined by the corresponding User Stories, Use Cases, Business Rules, and screen documentation.

These guidelines should not introduce new business rules or override module-specific requirements.

## General Consistency

* Use consistent visual representations for the same statuses and concepts across the system.
* Use clear business terminology in labels and messages; do not expose database column names or implementation terminology to users.
* Keep common actions, form patterns, and feedback behavior consistent across modules where their meaning is equivalent.

## Forms and Validation

* Clearly identify required fields when those fields are defined as required by the corresponding requirements.
* Provide field-level validation where appropriate.
* Display validation and business-rule feedback in clear, understandable language.
* Preserve entered values when a validation failure allows the user to correct and retry the action.
* Do not infer additional required fields or validation rules that are not supported by the requirements.

## Modals and Dialogs

* Use modals or dialogs for focused tasks that can be completed without leaving the current page.
* Prefer a full page or another appropriate presentation for interactions involving large or complex datasets.
* Use confirmation dialogs for destructive, irreversible, or explicitly confirmed actions defined by the screen flow.
* Do not add confirmation steps to every data-changing action by default.

## Lists and Data Presentation

* Keep list controls consistent where equivalent capabilities exist.
* Search, filtering, sorting, and pagination should only be introduced where supported by requirements or adopted as explicit UI/UX design decisions.
* For large datasets, pagination, server-side filtering, or other scalable presentation techniques may be considered based on expected data volume and performance requirements.

## Bulk Actions

* Make the current selection clear when performing a bulk action.
* For bulk operations that allow partial success, provide clear feedback about which records were processed and which were not.
* Do not assume that all bulk operations are atomic or partial-success; follow the behavior defined by the corresponding business requirements.

## Status and Historical Data

* Clearly distinguish editable and read-only states.
* Determine editability from the applicable business rules and entity state rather than from navigation path or visual convention alone.
* Where requirements define historical, applied, cancelled, closed, or otherwise immutable data as read-only, represent that state consistently in the UI.

## Design Assumptions

* UI-level decisions such as layout, spacing, control placement, visual hierarchy, and presentation patterns may be made during wireframing and visual design.
* A UI decision must not silently introduce a new business rule, workflow, role, data requirement, or permission.
* If an unresolved requirement affects business behavior, record it as an Open Question or TBD rather than resolving it through the UI design.
