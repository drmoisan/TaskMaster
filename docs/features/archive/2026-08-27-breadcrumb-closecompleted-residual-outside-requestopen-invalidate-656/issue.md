# breadcrumb-closecompleted-residual-outside-requestopen-invalidate (Issue #656)

- Date captured: 2026-08-27
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-closecompleted-residual-outside-requestopen-invalidate/ (Issue #656)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #656
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/656
- Last Updated: 2026-08-27
- Work Mode: full-bug

## Summary

`BreadcrumbDropDownOpenCoordinator._closeCompleted` stays `true` when the drop-down host is reopened by
a path that reaches neither `RequestOpen` nor `Invalidate`, so a subsequent close is wrongly suppressed.
This is the known residual of the SR-4 two-flag close fix shipped for #462 under #501, recorded against
the host paths owned by feature #488.

## Environment

- OS/version: Windows 11, Outlook VSTO add-in host
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`
- Data source or fixture: `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` harness

## Steps to Reproduce

1. Open the breadcrumb drop-down host and close it through `CloseCore`, so `_closeCompleted` becomes `true`.
2. Reopen the host through a path that reaches neither `RequestOpen` nor `Invalidate`.
3. Request a close.

## Expected Behavior

The close request reaches `_host.Close`, because the host is genuinely open again.

## Actual Behavior

The coordinator still treats the host as already closed and suppresses the close. `_closeCompleted` was
never cleared, because it is cleared only on the `RequestOpen` and `Invalidate` paths.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: no runtime log; the residual is established by source inspection of the flag-clearing paths.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: it requires a reopen path that bypasses both entry points, which the currently exercised UI
flows do not take. It is a latent correctness gap rather than an observed user-facing failure.

## Suspected Cause / Notes

#462 was fixed by replacing the single `_closePending` flag with two flags, `_closeInFlight` and
`_closeCompleted`. `_closeCompleted` is cleared on `RequestOpen` and `Invalidate` only.

The two-flag form was chosen deliberately. The naive alternative, clearing the close flag on the
successful-close path, makes two existing must-pass tests fail by letting a second `CloseCore` reach
`_host.Close`: `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose` and
`SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired`. Both encode the repeated-close
suppression contract. The two-flag form passes all three must-pass tests with no test edit, so it was
shipped and this residual recorded rather than traded for a regression.

This belongs to feature #488, not #501: the reopen paths that bypass `RequestOpen` and `Invalidate`
live in the ItemViewer breadcrumb lifecycle host surface. #501 was not permitted to write
`BreadcrumbItemViewerLifecycleCoordinator.cs`, `BreadcrumbDropDownHost.cs` or `ItemViewer.Breadcrumb.cs`.

## Proposed Fix / Validation Ideas

- [ ] Enumerate every path that reopens the drop-down host
- [ ] For any path reaching neither `RequestOpen` nor `Invalidate`, route it through one of them or clear `_closeCompleted` explicitly
- [ ] Add a regression test driving that path, keeping the three must-pass tests unedited

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

## References

- Split out of #501 / #462; see `docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md`, SR-4 and `## Implementation Notes`
- Evidence: `docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/qa-gates/closepending-split.2026-08-27T20-53.md`
