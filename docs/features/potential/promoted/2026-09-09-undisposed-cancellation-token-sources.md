# undisposed-cancellation-token-sources (Issue #840)

- Date captured: 2026-09-09
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/undisposed-cancellation-token-sources/ (Issue #840)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #840
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/840
- Last Updated: 2026-09-10
## Summary

Two `CancellationTokenSource` instances are constructed and never disposed by any holder.

## Environment

- OS/version: Windows 11, Outlook VSTO host
- Python version: n/a (C#, net48)
- Command/flags used: n/a, static observation
- Data source or fixture: `SubjectMapSco.Orchestration.cs:228` and `ProgressPackage.cs:25`

## Steps to Reproduce

1. Inspect the construction site at `SubjectMapSco.Orchestration.cs:228`.
2. Inspect the construction site at `ProgressPackage.cs:25`.
3. Search for a corresponding dispose call on either instance.

## Expected Behavior

Every `CancellationTokenSource` has an owner that disposes it, releasing its timer and registration resources.

## Actual Behavior

Neither instance is disposed by any holder.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none. Static observation.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

Surfaced during preparation of feature 821 in the review-residuals-2026-09-08 epic and recorded there as observation O-3, severity MEDIUM. Left out of that feature's blast radius deliberately. Fixing it requires deciding ownership, not only adding a dispose call.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: disposal assertions on both holders
- [ ] Integration scenario to retest: repeated open and close cycles of the affected components
- [ ] Manual verification notes: confirm no double-dispose on shared sources

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
