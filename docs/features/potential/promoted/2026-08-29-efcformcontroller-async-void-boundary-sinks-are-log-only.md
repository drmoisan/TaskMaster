# efcformcontroller-async-void-boundary-sinks-are-log-only (Issue #697)

- Date captured: 2026-08-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efcformcontroller-async-void-boundary-sinks-are-log-only/ (Issue #697)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #697
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/697
- Last Updated: 2026-08-29
## Summary

The five `async void` click handlers in `EfcFormController` route every caught failure into a
`BoundaryErrorSink` that defaults to a log-only delegate, so a failed button press writes a log entry
and shows the user nothing. The button appears to do nothing.

## Environment

- OS/version: Windows 11, Outlook desktop (VSTO add-in host)
- Python version: not applicable; this is a .NET Framework 4.8.1 C# WinForms component
- Command/flags used: not reproducible from a command line; requires the Email Filer Combined form
- Data source or fixture: any condition that makes one of the five handler bodies throw

## Steps to Reproduce

1. Open the Email Filer Combined form in Outlook with the TaskMaster add-in loaded.
2. Put the environment into a state that makes one of the five handler bodies throw, for example an unresolvable archive root.
3. Press the corresponding button.

## Expected Behavior

A failure at a UI boundary either surfaces a redacted user-facing diagnostic or is a deliberate,
documented silent case. The user is not left unable to distinguish a failure from a no-op.

## Actual Behavior

The exception is caught and passed to `BoundaryErrorSink`, whose default implementation writes to the
log only. Nothing is shown to the user and the control returns to an idle state, so the press is
indistinguishable from a no-op.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: not captured; the failure is visible only in the log4net output, which is the defect being reported.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

Verified citations, re-derived at 2026-08-29 against commit ecdb1c84:

- `QuickFiler/Controllers/EfcFormController.cs:442`, `:460`, `:477`, `:495`, `:557` — the five `async void` click handlers.
- `QuickFiler/Controllers/EfcFormController.cs:445-458` — the representative sibling `ButtonCancelClickAsync`, showing the `catch` at `:454-457` and the sink call at `:456`.
- `QuickFiler/Controllers/EfcFormController.cs:129` — `(message, exception) => logger.Error(message, exception)`, the default sink, which is log-only.

This is the same silent-swallow symptom issue 638 addressed inside `EfcDataModel`, but at a different
layer and with a different remedy, so it was excluded from that issue's scope.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: decide whether `BoundaryErrorSink` should gain a user-notification arm rather than each of the five handlers growing its own, then cover the chosen arm with an injected capturing delegate.
- [ ] Integration scenario to retest: each of the five buttons under an induced failure.
- [ ] Manual verification notes: confirm any new user-facing text carries no mailbox address and no filesystem path.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Origin: deferred non-goal (b) of issue 638. Proposed labels: bug, quickfiler, ui-diagnostics, follow-up.
