# breadcrumb-dispatch-message-and-handoff-record-inaccurate (Issue #941)

- Date captured: 2026-09-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-dispatch-message-and-handoff-record-inaccurate/ (Issue #941)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #941
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/941
- Last Updated: 2026-09-30
## Summary

The breadcrumb test dispatcher's exception message, and one earlier handoff record, describe the `DispatchValue` path inaccurately. These are wording and documentation defects only; behavior is correct.

## Environment

- OS/version: n/a
- Python version: n/a (C#)
- Command/flags used: n/a
- Data source or fixture: n/a

## Steps to Reproduce

1. Read `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` lines ~101 and ~183, and the assertion at `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:305`.
2. Compare the message with what the owner-thread-only `DispatchValue` path actually does.
3. Read the earlier breadcrumb thread-affinity follow-up handoff record, which attributes one `DispatchValue` site to the owner-thread-id check.

## Expected Behavior

The message and the asserted wording describe the specific `DispatchValue` behavior, and the handoff record attributes each site to the check that actually governs it.

## Actual Behavior

- The message "The owner-thread-only test dispatcher cannot marshal cross-thread UI work." is broader than the `DispatchValue` mechanism, and the test asserts the broad wording with `*cannot marshal cross-thread UI work*`.
- The earlier handoff record misattributes one `DispatchValue` site to the owner-thread-id check.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: see the PR #939 body, section Follow-ups, items 3 and 4.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Suspected Cause / Notes

Both originate from the breadcrumb thread-affinity work, and both were found by the #931 review. Source: `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/qa-gates/p4-t13-follow-up-handoff.2026-09-29T09-46.md`.

## Proposed Fix / Validation Ideas

- [ ] Narrow the message text at both sites and update the test's `WithMessage` pattern to match.
- [ ] Correct the handoff record's attribution.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
