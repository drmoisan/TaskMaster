# progress-viewer-cancel-button (Issue #339)

- Date captured: 2026-07-16
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-07-16-progress-viewer-cancel-button-339/ (Issue #339)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #339
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/339
- Last Updated: 2026-07-16
- Work Mode: minor-audit

## Summary

`ProgressViewer` displays its Cancel button in a disabled state when a
`ProgressTracker` or `ProgressTrackerAsync` initializes the viewer for loading.
The configured `CancellationTokenSource` is stored, but the disabled control
prevents the user from requesting cooperative cancellation.

## Environment

- OS/version: Windows desktop environment supported by TaskMaster
- Runtime: .NET Framework WinForms/VSTO solution
- Entry points: `ProgressTracker.Initialize()` and `ProgressTrackerAsync.InitializeAsync()`
- Data source or fixture: Any operation that displays `ProgressViewer` while loading

## Steps to Reproduce

1. Create a `ProgressTracker` or `ProgressTrackerAsync` with a non-null
   `CancellationTokenSource`.
2. Initialize the tracker so that it constructs and displays `ProgressViewer`.
3. Observe the Cancel button while the operation is loading.

## Expected Behavior

The Cancel button is enabled while loading. Selecting it calls `Cancel()` on
the configured `CancellationTokenSource`, allowing background work that observes
the token to stop cooperatively.

## Actual Behavior

The Cancel button is disabled. The tracker assigns the token source through the
`CancelSource` property, whose setter stores the source without enabling the
button. Because the control cannot be selected, cancellation is not propagated
from this UI path.

## Acceptance Criteria

- [x] Assigning a non-null `CancellationTokenSource` through
      `ProgressViewer.CancelSource` enables the Cancel button immediately,
      including the initial loading state used by `ProgressTracker` and
      `ProgressTrackerAsync`.
- [x] Selecting the enabled Cancel button requests cancellation on the same
      configured `CancellationTokenSource` so token-observing background work
      can stop cooperatively.
- [x] A deterministic MSTest regression test fails against the current property
      setter, passes after the targeted fix, and the final C# toolchain completes
      in format, analyzer, nullable-analysis, and coverage-enabled test order
      without regression.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: No error is logged; the defect is the disabled control state.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

`ProgressViewer.SetCancellationTokenSource(...)` enables the button after
storing the source, but `ProgressTracker` and `ProgressTrackerAsync` configure
the viewer through the `CancelSource` property. The property setter omits the
equivalent enabled-state update.

## Proposed Fix / Validation Ideas

- [ ] Add a regression test that assigns `CancelSource` and verifies the Cancel
      control is enabled.
- [ ] Preserve the existing test that verifies selecting Cancel transitions the
      configured token to the cancellation-requested state.
- [ ] Run the repository C# format, analyzer, nullable-analysis, and
      coverage-enabled MSTest gates in the required order.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
