# EFC Viewer: Enter does not confirm the highlighted destination folder (Issue #995)

- Date captured: 2026-10-17
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/EFC_Viewer_Enter_does_not_confirm_the_highlighted_destination_folder/ (Issue #995)

- Issue: #995
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/995
- Last Updated: 2026-10-17
## Summary

Pressing Enter after highlighting a destination folder in EFC Viewer does not invoke the OK action. Reported by the user; automated reproduction is pending.

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

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
