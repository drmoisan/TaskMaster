# getmovediagnostics-null-guard (Potential Bug)

- Date captured: 2026-03-25
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/ (Issue #97)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

`GetMoveDiagnostics` in `QfcCollectionController` throws a `NullReferenceException` at `if (string.IsNullOrEmpty(olAppointment.Body))` when the `Email Time` Outlook calendar subfolder does not exist. The `olAppointment` parameter is set to `null` by `WriteMoveToCalendar` when `GetCalendar("Email Time", ...)` returns `null`, but neither `GetMoveDiagnostics` nor `QuickFileMetrics_WRITE` guards against a null appointment before dereferencing it.

## Environment

- OS/version: Windows (any)
- Python version: N/A (C# / WinForms VSTO add-in)
- Command/flags used: Run QuickFiler move operation on a machine where no "Email Time" Outlook calendar subfolder exists
- Data source or fixture: Any Outlook profile without an "Email Time" calendar subfolder under the default calendar

## Steps to Reproduce

1. Ensure no "Email Time" subfolder exists under the default Outlook calendar.
2. Run a QuickFiler move operation that triggers `WriteMetricsAsync` or `QuickFileMetrics_WRITE`.
3. Observe `System.NullReferenceException` at `QfcCollectionController.GetMoveDiagnostics` line 2115.

## Expected Behavior

When the `Email Time` calendar does not exist, the metrics operation should complete gracefully — either skipping calendar logging or creating the folder — without throwing an exception.

## Actual Behavior

`System.NullReferenceException` is thrown at `if (string.IsNullOrEmpty(olAppointment.Body))` because `olAppointment` is null when the `Email Time` subfolder is absent.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: `System.NullReferenceException` at `QfcCollectionController.GetMoveDiagnostics` in `QfcCollectionController.cs:line 2115`

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

Root cause identified:
- `UtilitiesCS.Calendar.GetCalendar("Email Time", ...)` returns `null` when no matching subfolder exists.
- `WriteMoveToCalendar` propagates `null` as `OlAppointment` when this happens.
- `QuickFileMetrics_WRITE` dereferences `olEmailCalendar.Items` without a null check.
- `GetMoveDiagnostics` dereferences `olAppointment.Body` without a null check.

Files to fix:
- `QuickFiler/Controllers/QfcCollectionController.cs` — `GetMoveDiagnostics` (line ~2115)
- `QuickFiler/Controllers/QfcHomeController.cs` — `QuickFileMetrics_WRITE` (line ~419) and `WriteMoveToCalendar` (line ~521)

## Proposed Fix / Validation Ideas

- [x] In `GetMoveDiagnostics`, guard all `olAppointment` access with `if (olAppointment is not null)`.
- [x] In `QuickFileMetrics_WRITE`, guard `olEmailCalendar` access before calling `.Items.Add()`.
- [x] In `WriteMoveToCalendar`, the null guard already exists (sets `OlAppointment = null`); no change needed there.
- [x] Add regression tests confirming null appointment is handled gracefully in all three callers.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
