# User Stories - Salary Grade Promotion

This document lists the user stories for the **Salary Grade Promotion** feature, written from the point of view of the people who will use it: **HR Staff** and **Approver**.

Format: `As a [role], I want [goal], so that [benefit]`. Each story has:
- **Business Rules** — the constraints that make the story correct (in plain business language).
- **Acceptance Criteria** — Given/When/Then, describing only the interaction, not implementation details.

Priority uses MoSCoW (Must / Should / Could).

## Process Overview

1. HR Staff creates a review period. The system automatically works out a proposed new grade for each eligible employee in it.
2. HR Staff goes through the batch and marks each employee's proposed grade as approved or not approved (one at a time, or several at once).
3. Once every employee in the period has been marked, HR Staff submits the period to the Approver.
4. The Approver reviews the submitted period and drafts a salary decision from the approved employees.
5. The Approver applies the decision, making the new grades official.
6. Anyone can look up an employee's salary history afterwards.

## Summary

`Size` is a rough T-shirt estimate (S/M/L) to gauge relative effort, not a committed number — to be refined once the team sizes the backlog. `Depends on` lists the story that must be functionally complete first; it does not mean the story can't be *built* independently, only that it can't be *tested end-to-end* without its dependency.

| ID | Story | Role | Priority | Depends on | Size |
|---|---|---|---|---|---|
| US-01 | Create a review period | HR Staff | Must | — | S |
| US-02 | Search and filter review periods | HR Staff | Should | US-01 | S |
| US-03 | View employees and their proposed grade in a review period | HR Staff | Must | US-01 | M |
| US-04 | Approve or reject each employee's proposed grade | HR Staff | Must | US-03 | L |
| US-05 | Submit a review period to the Approver | HR Staff | Must | US-04 | S |
| US-06 | Review a submitted period and draft a salary decision | Approver | Must | US-05 | M |
| US-07 | Apply a salary decision | Approver | Must | US-06 | L |
| US-08 | Look up an employee's salary history | HR Staff / Approver | Should | — | S |
| US-09 | List salary decisions and resume a draft | Approver | Must | — | S |
| US-10 | Cancel a salary decision | Approver | Should | US-06 | S |
| US-11 | Cancel a review period | HR Staff | Should | US-01 | S |

---

### US-01: Create a review period

**As** HR Staff, **I want** to create a new salary review period, **so that** I can start reviewing employees for salary grade promotion in a defined cycle.

**Business Rules:**
- A review period must have a unique name/code, a review date, and a type (e.g. annual, mid-year, special).
- Creating a period immediately calculates the proposed grade for every eligible employee ([US-03](#us-03-view-employees-and-their-proposed-grade-in-a-review-period)) as part of the same action — there is no separate step to start screening.

**Acceptance Criteria:**
- Given I am on the review period list, when I fill in the required details and save, then a new review period is created and appears in the list.
- Given I try to save a review period with a code that already exists, when I submit, then I see an error and nothing is created.
- Given a review period was just created, when I open it right away, then every eligible employee already has a proposed grade — I don't need to trigger screening separately.

### US-02: Search and filter review periods

**As** HR Staff, **I want** to filter review periods by date range, type, and status, **so that** I can find a specific period quickly without scrolling a long list.

**Business Rules:**
- None beyond the fields already captured when a period is created.

**Acceptance Criteria:**
- Given many review periods exist, when I set a date range, type, or status filter and search, then only matching periods are shown.
- Given the result list is long, when it exceeds one page, then it is split into pages.

### US-03: View employees and their proposed grade in a review period

**As** HR Staff, **I want** to see the list of employees included in a review period along with the new grade the system has worked out for each of them, **so that** I know who is ready for me to go through.

**Business Rules:**
- The system automatically works out a proposed new grade for every eligible employee in the period; HR Staff does not calculate this by hand.
- An employee who is not eligible for review must always show the reason why, and has no proposed grade.
- An employee is eligible for a proposal in this review period only when **all** of the following are true:
  1. They have held their current grade for at least the minimum required time — **24 months** as of the review date.
  2. There is a next grade above their current one within their **own salary scale** — an employee already at the highest grade of their scale is not eligible.
  3. They have not already been given a proposal in this review period — an employee cannot receive more than one proposal per period.
- When an employee is eligible, the proposed grade is the **next grade up within their current salary scale**.

**Acceptance Criteria:**
- Given a review period is open, when I view it, then I see every included employee, their current grade, and the grade the system proposed for them (if eligible).
- Given an employee has held their current grade for less than 24 months as of the review date, when the period is opened, then they are shown as not eligible with that reason.
- Given an employee is already at the highest grade of their salary scale, when the period is opened, then they are shown as not eligible with that reason.
- Given an employee already has a proposal in this review period, when the period is opened, then they are not given a second proposal.
- Given I want to narrow the list, when I filter by department, eligibility, or review outcome, then only matching employees are shown.

### US-04: Approve or reject each employee's proposed grade

**As** HR Staff, **I want** to look at the new grade the system proposed for each employee and mark it as approved or not approved — one at a time, or several at once — **so that** I can screen the whole batch before it goes to the Approver.

**Business Rules:**
- Only an eligible employee with a system-proposed grade can be marked approved or not approved.
- When several employees are selected for the same action, each one is still individually checked against the same rules as if done one at a time.
- If an employee is marked not approved, a reason should be recorded so it can be explained later.
- An employee's outcome can only be changed while the review period is still in progress — once submitted (see [US-05](#us-05-submit-a-review-period-to-the-approver)), it can no longer be changed at all.

**Acceptance Criteria:**
- Given an employee has a proposed grade, when I mark it approved, then that employee is recorded as approved for this period.
- Given an employee has a proposed grade, when I try to mark it not approved without giving a reason, then this is blocked until a reason is entered.
- Given I select several employees at once, when I approve or reject them together, then each one is checked individually, only the valid ones go through, and I'm told how many succeeded, how many failed, and why.
- Given the review period has already been submitted, when I try to approve or reject an employee in it (single or bulk), then this is blocked entirely, even if that employee's outcome is still pending.

### US-05: Submit a review period to the Approver

**As** HR Staff, **I want** to submit a review period to the Approver once every employee in it has been marked approved or not approved, **so that** the Approver can process the results.

**Business Rules:**
- A review period can only be submitted once every employee in it has an outcome; it cannot be submitted while some are still unprocessed.
- Once submitted, HR Staff should not go back and quietly change any employee's outcome without the Approver knowing.

**Acceptance Criteria:**
- Given every employee in the period has been marked approved or not approved, when I submit the period, then I am asked to confirm before it is sent.
- Given some employees in the period are still unprocessed, when I try to submit the period, then this is blocked with a message telling me what's left.
- Given the period has been submitted, when I look at it afterwards, then it is clearly marked as submitted and visible to the Approver.

### US-06: Review a submitted period and draft a salary decision

**As** an Approver, **I want** to review a submitted review period and draft a salary decision from the employees approved in it, **so that** I can prepare the official change before it takes effect.

**Business Rules:**
- Only employees marked approved by HR Staff can be included in a decision being drafted.
- Drafting a decision must not change any employee's real salary yet.
- There are two ways to start drafting a decision, and both must end up at the same screen with the same period already selected:
  1. From the submitted review period itself (e.g. a "Create Decision" action shown once its status is Submitted) — the period is already known, so nothing needs to be picked.
  2. From the salary decision list ([US-09](#us-09-list-salary-decisions-and-resume-a-draft)) by choosing "Create New" — here the Approver must first pick which submitted review period the decision is for.
- A submitted review period can have at most one non-cancelled decision drafted from it.
- The employees included in a decision are selected up front, when the decision is created (from those approved in the picked review period). An employee can be removed from a draft afterward, but there is no way to add more later — a decision needing different employees must be re-created.

**Acceptance Criteria:**
- Given a review period has been submitted, when I open it, then I see each included employee's current grade and the grade approved for them.
- Given a submitted review period, when I start a decision from it directly, then the decision-drafting screen opens with that period already selected — I am not asked to pick one.
- Given I start a decision from the salary decision list instead, when the drafting screen opens, then I must choose a submitted review period before I can add employees.
- Given a submitted review period already has a non-cancelled decision, when I try to start another decision from it, then this is blocked.
- Given I am creating a decision, when I select which employees to include, then only employees approved in that period are offered, and my selection is saved as part of creating the draft.
- Given a decision is still a draft, when I check any included employee's salary, then it is unchanged.

### US-07: Apply a salary decision

**As** an Approver, **I want** to apply a drafted salary decision, **so that** the new salary grade for each included employee becomes real and traceable back to this decision.

**Business Rules:**
- A decision can only be applied once, and must apply to all of its employees together — if part of it cannot go through, none of it should.
- Every employee's salary change must have a clear effective date, and must not overlap with their existing salary record.
- Once applied, a decision cannot be silently deleted — only formally cancelled through a separate action.

**Acceptance Criteria:**
- Given a decision is ready, when I apply it, then I am shown a summary of its impact and asked to confirm first.
- Given I confirm, when the decision is applied, then every included employee's salary is updated to the new grade as of the decision's effective date, and their prior salary is preserved as history.
- Given something prevents one employee's change from being applied (e.g. a conflicting record), when the decision is applied, then no employee in that decision is updated, and I am told what needs to be fixed.
- Given a decision has already been applied, when I try to apply it again, then this is blocked.

### US-08: Look up an employee's salary history

**As** HR Staff or an Approver, **I want** to look up an employee's full salary history, **so that** I can answer questions about a specific past date or audit how their salary changed over time.

**Business Rules:**
- This is a read-only view; salary cannot be changed from here.

**Acceptance Criteria:**
- Given I search for an employee, when their history loads, then I see every past salary grade with its effective period and the reason/decision behind each change, newest first.
- Given I want more detail on a change, when I open the decision behind it, then I see that decision in a read-only view.

### US-09: List salary decisions and resume a draft

**As** an Approver, **I want** to see a list of all salary decisions (draft, applied, or cancelled) and reopen any draft, **so that** I don't lose track of a decision I started earlier or accidentally start a duplicate one.

**Business Rules:**
- The list shows every decision regardless of status, with which review period it belongs to.
- Opening a draft from this list returns to the exact same drafting screen used in [US-06](#us-06-review-a-submitted-period-and-draft-a-salary-decision), with its previously added employees still there.
- Applied or cancelled decisions open in read-only mode from this list (see [US-07](#us-07-apply-a-salary-decision)) — they cannot be edited.
- "Create New" from this list requires picking a submitted review period first (see US-06, entry point 2), and only review periods without an existing non-cancelled decision can be picked.

**Acceptance Criteria:**
- Given decisions exist, when I open the list, then I see each one's decision number, review period, status, and effective date.
- Given a decision in the list has Draft status, when I open it, then I return to drafting it with everything I previously added still there.
- Given a decision in the list has Applied or Cancelled status, when I open it, then I see it in read-only mode.
- Given I click "Create New", when I am asked to pick a review period, then only submitted periods without an existing non-cancelled decision are offered.

### US-10: Cancel a salary decision

**As** an Approver, **I want** to formally cancel a salary decision (draft or already applied), **so that** a mistaken or no-longer-valid decision is clearly marked instead of left active or silently deleted.

**Business Rules:**
- Cancelling only marks the decision's status as Cancelled — it does not automatically revert any employee's salary that was already applied by that decision.
- If a real salary change needs to be undone, that requires drafting and applying a separate new decision; cancelling by itself never changes `HrEmployeeSalary`.
- A cancelled decision is permanent — it cannot be un-cancelled or edited afterward.
- Cancelling a decision frees up its review period so a new decision can be drafted from it (see [US-06](#us-06-review-a-submitted-period-and-draft-a-salary-decision)'s "at most one non-cancelled decision" rule).

**Acceptance Criteria:**
- Given a decision is a draft, when I cancel it, then its status becomes Cancelled and it can no longer be edited.
- Given a decision has already been applied, when I cancel it, then its status becomes Cancelled, but every employee's salary it previously changed remains exactly as applied.
- Given a decision is already Cancelled, when I try to cancel it again, then this is blocked.
- Given a review period's only non-cancelled decision was just cancelled, when I try to draft a new decision from that period, then it is now offered again.

### US-11: Cancel a review period

**As** HR Staff, **I want** to cancel a review period that no longer needs to be carried forward, **so that** it stops cluttering the active list without deleting its record.

**Business Rules:**
- A review period can be cancelled at any status except Closed or already Cancelled.
- A review period cannot be cancelled while it already has a non-cancelled decision drafted from it — that decision must be cancelled first (see [US-10](#us-10-cancel-a-salary-decision)).
- Cancelling does not delete or change the employees already screened in it; their review outcomes remain as a record, they simply can no longer lead anywhere.

**Acceptance Criteria:**
- Given a review period is Draft, In Progress, or Submitted with no decision drafted from it, when I cancel it, then its status becomes Cancelled.
- Given a review period already has a non-cancelled decision, when I try to cancel it, then this is blocked with a message telling me to deal with the decision first.
- Given a review period is Closed, when I try to cancel it, then this is blocked — a period with an applied decision is never cancelled, only the decision itself can be (US-10).
