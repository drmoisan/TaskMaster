# quickfiler-session-metrics-twelve-hour-time-format (Issue #645)

- Date captured: 2026-08-27
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/ (Issue #645)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #645
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/645
- Last Updated: 2026-08-27
- Work Mode: full-bug

## Summary

The QuickFiler session-metrics CSV renders its time-of-day field with the .NET format string
`"hh:mm"`. Lowercase `hh` is the 12-hour clock, and the format carries no `tt` designator, so 14:30
renders as `02:30` and is indistinguishable from 02:30. Every row written since the format was
introduced carries an ambiguous time.

Three sites are affected, all in QuickFiler:

- `QuickFiler/Controllers/QfcHomeController.Metrics.cs:31`
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs:110`
- `QuickFiler/Controllers/EfcHomeController.Metrics.cs:68`

Line numbers are as of the spec that raised this note; they shift slightly after the metrics work
described below.

**Why this was split out rather than fixed alongside the metrics work.** This was identified as
cross-feature note CFN-4 while delivering issues #442, #443 and #451 (feature
`quickfiler-home-controller-metrics-442`). It was deliberately excluded from that scope for three
reasons: it is a *content* defect whereas that feature's remit was the row *shape*, the flush, and
duration correctness; fixing it breaks three currently passing tests on their asserted literals
(`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`, two clock-seam tests, and
`QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`, the formatted-row test), each of
which encodes the 12-hour rendering; and no issue in that family lists it as an acceptance
criterion. The sibling numeric-format defect at the same sites *was* fixed there: the six numeric
format calls now pass `CultureInfo.InvariantCulture`. The date and time format calls were left
untouched precisely so this defect could be tracked separately.

**Proposed fix.** Change the three format strings from `"hh:mm"` to `"HH:mm"` (24-hour) and update
the three asserted test literals to match. `"HH:mm"` is preferred over `"hh:mm tt"` because the
adjacent `SentDate` field already renders as `"HH:mm:ss"`, so 24-hour is the file's existing
convention and keeps the row internally consistent. Consider passing `CultureInfo.InvariantCulture`
to these calls at the same time, matching what the numeric fields now do.

**Backward compatibility.** The session-metrics CSV has no in-repo reader: a repository-wide search
for `EmailSession` returns three settings-plumbing declarations and three writers, and no parser or
schema consumer. The artifact is write-only from the codebase's perspective. The residual risk is
confined to a human-maintained spreadsheet outside the repository.

## Environment

- OS/version: Windows 11, Outlook VSTO add-in host
- Python version: not applicable (C# / .NET Framework 4.8)
- Command/flags used: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`
- Data source or fixture: the session-metrics CSV emitted by the QuickFiler and EFC metrics writers

## Steps to Reproduce

1. Run a QuickFiler filing session, or an EFC move session, whose metrics write occurs at any
   time of day at or after 13:00 local time.
2. Open the session-metrics CSV that the run appends to.
3. Read the time-of-day field of the appended row.

## Expected Behavior

The time-of-day field unambiguously identifies the hour, either on a 24-hour clock (`14:30`) or on a
12-hour clock with an explicit AM/PM designator (`02:30 PM`).

## Actual Behavior

The field renders `02:30` for a 14:30 event. No AM/PM designator is present, so an afternoon row is
byte-identical to a small-hours row and the recorded time cannot be recovered from the file.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: the three offending format strings are the literal `"hh:mm"` at
  `QuickFiler/Controllers/QfcHomeController.Metrics.cs:31` and `:110`, and
  `QuickFiler/Controllers/EfcHomeController.Metrics.cs:68`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: the emitted data is silently wrong rather than absent, and the file has no in-repo reader,
so nothing in the product misbehaves. The cost is borne by whoever analyses the CSV outside the
repository.

## Suspected Cause / Notes

A format-string authoring error: `hh` was used where `HH` was intended. The adjacent `SentDate`
column in the same rows already uses `"HH:mm:ss"`, which indicates the 12-hour spelling was
unintentional rather than a deliberate presentation choice.

Promoted from cross-feature note CFN-4 of
`docs/features/active/quickfiler-home-controller-metrics-442/spec.md`, per that feature's AC-25.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: the three asserted time literals in
  `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` (two clock-seam tests) and
  `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` (the formatted-row test) must be
  updated to the 24-hour rendering, and each test must continue to drive its clock through
  `FakeTimeProvider` or the injected factory rather than the wall clock.
- [x] Integration scenario to retest: a full QuickFiler test-suite run must be green after the
  literals are updated.
- [x] Manual verification notes: a repository search for the 12-hour format literal under
  `QuickFiler/` must return no match after the change.

Acceptance criteria for the resulting issue:

- All three sites render the time-of-day field on a 24-hour clock.
- A repository search for the 12-hour format literal under `QuickFiler/` returns no match.
- The three affected test literals are updated and the full QuickFiler test suite is green.
- The change is stated in the PR body, since it alters the emitted CSV content.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch

## Source

From: docs/features/potential/2026-08-27-quickfiler-session-metrics-twelve-hour-time-format.md
