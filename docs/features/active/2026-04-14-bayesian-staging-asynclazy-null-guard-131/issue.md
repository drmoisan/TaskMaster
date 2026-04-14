# bayesian-staging-asynclazy-null-guard (Issue #131)

- Date captured: 2026-04-14
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/bayesian-staging-asynclazy-null-guard/ (Issue #131)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #131
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/131
- Last Updated: 2026-04-14
- Work Mode: minor-audit

## Summary

One or two sentences on what is broken.

## Environment

- OS/version:
- Python version:
- Command/flags used:
- Data source or fixture:

## Steps to Reproduce

1. ...
2. ...
3. ...

## Expected Behavior

What you expected to happen.

## Actual Behavior

What actually happened (include key error text).

## Acceptance Criteria

- [x] Bayesian staging JSON no longer attempts to deserialize `FolderWrapper.ItemHelpers` or other non-deserializable runtime-only members.
- [x] The null-or-empty guard used by the staging load path throws a deterministic argument exception without dereferencing a null reflected caller method.
- [x] Regression tests cover both the staging deserialization boundary and the safe null-or-empty guard behavior.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet:

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

Optional early hunches, related changes, or files to inspect.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas
- [ ] Integration scenario to retest
- [ ] Manual verification notes

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch