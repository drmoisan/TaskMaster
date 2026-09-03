# P5-T1 — PR Body Draft (Issue #645)

## Summary

Fixes issue #645: the QuickFiler session-metrics CSV wrote its time-of-day field using the .NET
custom format string `"hh:mm"`. `hh` is the 12-hour-clock specifier and the format string carries
no `tt` (AM/PM) designator, so an afternoon timestamp such as 14:30 rendered as `02:30`, byte-
identical to 02:30 in the small hours. This change replaces the three affected `"hh:mm"` literals
with `"HH:mm"` (24-hour) at:
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs:48` (`dataLineBeg` interpolation)
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs:127` (`curTimeText` assignment)
- `QuickFiler/Controllers/EfcHomeController.Metrics.cs:96` (`curTimeText` assignment)

and updates the three dependent test literals/doc comments in
`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` and
`QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` to match.

## Data / Content Impact

This change alters the emitted session-metrics CSV: the time-of-day column now renders on a
24-hour clock (`HH:mm`) instead of the previous ambiguous 12-hour rendering (`hh:mm`, no AM/PM
designator). The session-metrics CSV has no in-repo reader; the artifact is read only by a
human-maintained spreadsheet outside the repository, which should be notified of this content
change.

## Scope

No `CultureInfo.InvariantCulture` argument was added to any of the three fixed call sites (that
gap is tracked separately as issue #742). No file under `QuickFiler/Legacy/`, no
`TaskVisualization/TaskViewer.Designer.cs`, and no push-down-owned path (`.claude/**`,
`.codex/**`, `.agents/**`, `config/blast-radius.json`, `config/orchestration-routing.json`) is
touched by this change.

## Testing

Full `QuickFiler.Test` assembly run: 1312/1312 passed under a coverage-enabled
`vstest.console.exe` run. Full toolchain (CSharpier format/check, analyzer rebuild, nullable
rebuild) passed in a single pass with no failures.
