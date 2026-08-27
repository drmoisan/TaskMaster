# qfcremainingqueueadmission-dead-scoreloader (Issue #622)

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfcremainingqueueadmission-dead-scoreloader/ (Issue #622)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #622
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/622
- Last Updated: 2026-08-26
## Summary


`QfcRemainingQueueAdmission` accepts a `scoreLoader` delegate it never uses. The parameter is declared at `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs:17` and null-checked at `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs:23-26`, but it is never assigned to a field and never invoked. The null check makes the dead parameter look load-bearing and forces every caller to supply a non-null delegate that cannot affect behavior, which is actively misleading when the scoring signature changes.

## Environment

- OS/version:
- Python version:
- Command/flags used:
- Data source or fixture:

## Steps to Reproduce


1. Read `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs:17` and observe the `scoreLoader` parameter.
2. Read `:23-26` and observe the null guard.
3. Search the file for any assignment or invocation of `scoreLoader` and find none.

## Expected Behavior


Either the delegate participates in admission scoring, or the parameter and its guard are removed and callers stop constructing one.

## Actual Behavior


The parameter is guarded, then discarded. Callers pay the cost of building a delegate that is never called.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet:

## Impact / Severity


- [x] Low

No runtime misbehavior; a maintainability and API-honesty defect.

## Suspected Cause / Notes


Confirmed during issue #446 (`docs/features/active/quickfiler-bug-family-446`). #446 widened the score loader to a `(long Score, string TopFolder)` tuple across the real consumers and specifically established that this file needed no change precisely because the parameter is never invoked. That finding is what makes the deadness certain rather than suspected. The file was not in #446's owned set.

## Proposed Fix / Validation Ideas


- [ ] Decide whether admission should score; if not, delete the parameter, the guard, and the caller-side construction
- [ ] If it should score, wire it to a field and invoke it, with unit coverage proving the delegate is consulted
- [ ] Confirm no caller relies on the guard for its own null validation

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
