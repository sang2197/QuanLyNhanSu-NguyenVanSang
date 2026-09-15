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

---

### US-01: Create a review period

**As** HR Staff, **I want** to create a new salary review period, **so that** I can start reviewing employees for salary grade promotion in a defined cycle.

**Business Rules:**
- A review period must have a unique name/code, a review date, and a type (e.g. annual, mid-year, special).

**Acceptance Criteria:**
- Given I am on the review period list, when I fill in the required details and save, then a new review period is created and appears in the list.
- Given I try to save a review period with a code that already exists, when I submit, then I see an error and nothing is created.

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

**Acceptance Criteria:**
- Given an employee has a proposed grade, when I mark it approved, then that employee is recorded as approved for this period.
- Given an employee has a proposed grade, when I try to mark it not approved without giving a reason, then this is blocked until a reason is entered.
- Given I select several employees at once, when I approve or reject them together, then each one is checked individually, only the valid ones go through, and I'm told how many succeeded, how many failed, and why.

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

**Acceptance Criteria:**
- Given a review period has been submitted, when I open it, then I see each included employee's current grade and the grade approved for them.
- Given I am drafting a decision, when I add employees to it, then only employees approved in that period can be added.
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
