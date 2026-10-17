# EFC Viewer: Enter does not confirm the highlighted destination folder (Issue #995)

- Date captured: 2026-10-17
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-10-17-efc-viewer-enter-confirmation-995/ (Issue #995)

- Issue: #995
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/995
- Last Updated: 2026-10-17
- Work Mode: full-bug
## Summary

Pressing Enter after highlighting a destination folder in EFC Viewer did not invoke OK. A deterministic regression fails before the fix and passes afterward. The dedicated `rowConfirmed` message requests the existing guarded OK boundary and preserves the activated destination. Modified/repeated Enter and ordinary selection do not confirm. The active feature's `spec.md` is the sole acceptance source; final full QA and review remain pending.

## Environment

- TaskMaster QuickFiler EFC Viewer on Windows.
- Exact deployed version was not supplied.

## Steps to Reproduce

1. Open EFC Viewer to sort an email into a folder.
2. Highlight the destination folder.
3. Press Enter.

## Expected Behavior

Enter invokes the same confirmation action as clicking OK for the highlighted valid destination.

## Actual Behavior

Nothing happens; the OK action is not invoked.

## Logs / Screenshots

No logs or screenshots supplied.

## Impact / Severity

- [x] Medium
- Keyboard confirmation of a selected destination is unavailable.

## Suspected Cause / Notes

Inspect EfcViewer keyboard routing, the WebView2 folder selection host, and controller confirmation wiring. The root cause has not been verified.

## Proposed Fix / Validation Ideas

- Add a deterministic regression test before fixing the behavior.
- Route Enter through the existing OK action while retaining invalid-selection guards.
- Verify keyboard modifiers and existing cancellation behavior.
- Run the required C# formatting, analyzers, nullable checks, and tests.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
