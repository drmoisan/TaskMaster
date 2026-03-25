# getmovediagnostics-null-guard (Issue #97)

- Date captured: 2026-03-25
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/ (Issue #97)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #97
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/97
- Last Updated: 2026-03-25
- Work Mode: minor-audit

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

## Root Cause

- `UtilitiesCS.Calendar.GetCalendar("Email Time", ...)` returns `null` when no matching subfolder exists.
- `WriteMoveToCalendar` propagates `null` as `OlAppointment` when this happens.
- `QuickFileMetrics_WRITE` dereferences `olEmailCalendar.Items` without a null check.
- `GetMoveDiagnostics` dereferences `olAppointment.Body` without a null check.

## Acceptance Criteria

- [ ] In `GetMoveDiagnostics`, all `olAppointment` access is guarded with `if (olAppointment is not null)`.
- [ ] In `QuickFileMetrics_WRITE`, `olEmailCalendar` is guarded with a null check before calling `.Items.Add()`.
- [ ] Regression test: `GetMoveDiagnostics` called with null `olAppointment` completes without throwing.
- [ ] Regression test: `QuickFileMetrics_WRITE` completes without throwing when `GetCalendar` returns null.
- [ ] All existing tests pass with no regressions.
- [ ] Full C# toolchain passes: csharpier → msbuild analyzers → msbuild nullable → vstest coverage.
