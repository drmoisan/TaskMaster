# quickfiler-date-time-format-missing-invariant-culture (Issue #742)

- Date captured: 2026-09-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-date-time-format-missing-invariant-culture/ (Issue #742)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #742
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/742
- Last Updated: 2026-09-02
## Summary

Every date/time `.ToString(...)` call in `QfcHomeController.Metrics.cs`, `EfcHomeController.Metrics.cs`,
`QfcItemController.ViewerSetup.cs`, `QfcCollectionController.cs`, and `EfcItemController.cs` omits
`CultureInfo.InvariantCulture`, while the adjacent numeric fields in the same methods pass it
explicitly. The `:` character in a .NET custom format string is the `TimeSeparator` custom specifier,
not a literal colon, so it resolves to `DateTimeFormatInfo.TimeSeparator` for whichever culture is in
effect. Under a culture such as `it-IT`, `TimeSeparator` is `.` rather than `:`, so `now.ToString("HH:mm")`
can render `13.05` instead of `13:05` if the host machine's regional settings differ from the author's
assumption.

## Environment

- OS/version: Windows 11, Outlook VSTO add-in host
- Python version: not applicable (C# / .NET Framework 4.8)
- Command/flags used: not applicable (static code review finding)
- Data source or fixture: the session-metrics CSV emitted by the QuickFiler and EFC metrics writers, and
  on-screen/exception summary strings built from `SentDate`

## Steps to Reproduce

1. Set the Windows regional format (or thread `CurrentCulture`) to a locale whose
   `DateTimeFormatInfo.TimeSeparator` is not `:` (for example `it-IT`, whose separator is `.`).
2. Run a QuickFiler filing session or an EFC move session so the session-metrics CSV writer executes,
   or trigger a code path that builds a `SentDate`/`SentTime` display string.
3. Inspect the emitted CSV time column, or the on-screen/exception summary string.

## Expected Behavior

Every date/time field emitted by these files renders with a fixed, invariant separator regardless of
the host machine's regional settings, matching the invariant-culture handling already applied to the
adjacent numeric fields in the same methods (per the existing comment at
`EfcHomeController.Metrics.cs:101-103`: "the metrics file is machine-read, so numeric fields are
rendered with the invariant culture rather than the operator's locale").

## Actual Behavior

None of the date/time `.ToString(...)` calls in the affected files pass `CultureInfo.InvariantCulture`,
so the rendered separator character is culture-dependent. Confirmed sites:

- `QuickFiler/Controllers/QfcHomeController.Metrics.cs`: `curDateText`/`dataLineBeg`
  (`"MM/dd/yyyy"`, `"hh:mm"`/`"HH:mm"` after issue #645's fix) at lines 48 and 127.
- `QuickFiler/Controllers/EfcHomeController.Metrics.cs`: `curDateText`, `curTimeText`
  (`"MM/dd/yyyy"`, `"hh:mm"`/`"HH:mm"` after issue #645's fix) at line 95-96, and the `SentDate`
  field at lines 118-119 (`"MM/dd/yyyy"`, `"HH:mm:ss"`) — `SentDate` is the CSV field issue #645 cites
  as the target 24-hour convention, and it is itself uncultured.
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:498` — `SentDate.ToString("HH:mm")` in a
  user-facing summary string.
- `QuickFiler/Controllers/QfcCollectionController.cs:1294,2300` — `SentDate.ToString("HH:mm")` in
  controller/log summary strings.
- `QuickFiler/Controllers/EfcItemController.cs:612` — `SentDate.ToString("HH:mm")`, exposed as the
  `SentTime` property.

The CSV-writer sites (`QfcHomeController.Metrics.cs`, `EfcHomeController.Metrics.cs`) carry the higher
severity, since a wrong separator corrupts a machine-read artifact in the same way the numeric fields
were already protected against (see the comment cited above); the UI-facing sites
(`QfcItemController.ViewerSetup.cs`, `QfcCollectionController.cs`, `EfcItemController.cs`) are lower
severity, cosmetic-only.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: none of the `.ToString("MM/dd/yyyy")`, `.ToString("hh:mm")`/`.ToString("HH:mm")`, or
  `.ToString("HH:mm:ss")` calls at the sites listed above pass a `CultureInfo` argument; contrast with
  the `durationText`/`durationMinutesText` calls in the same two Metrics.cs files, which do
  (`.ToString("##0", CultureInfo.InvariantCulture)`).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium for the two CSV-writer files (a wrong time separator on a machine-read, write-only artifact is
the same defect class already fixed for the numeric fields); Low/cosmetic for the three UI-facing
`ToString("HH:mm")` call sites. Filed as a single Medium entry since the writer sites dominate.

## Suspected Cause / Notes

Discovered during research for issue #645 ("Bug: quickfiler-session-metrics-twelve-hour-time-format")
while investigating whether `CultureInfo.InvariantCulture` should be added to the three `hh:mm` -> `HH:mm`
sites that issue fixes. Tracing the issue's own cited target convention (`SentDate`'s `"HH:mm:ss"`)
showed that convention is itself uncultured, which means the gap is systemic across both files rather
than isolated to the three sites issue #645 touches. Deliberately excluded from issue #645's scope: it
is a distinct defect (culture-dependent separator vs. 12-hour ambiguity), and issue #645's own
acceptance criteria do not mention culture-invariance.

Confirmed the `H`/`h`/`m` letter specifiers in these format strings render ASCII 0-9 digits regardless
of culture (they are not digit-substituting specifiers); the only culture-dependent element in
`"HH:mm"`/`"MM/dd/yyyy"`/`"HH:mm:ss"` is the separator character, which resolves via
`DateTimeFormatInfo.DateSeparator` / `DateTimeFormatInfo.TimeSeparator`.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: add or extend clock-seam tests in
  `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` and
  `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` that assert output is stable under a
  non-`:`-separator culture (e.g. temporarily setting `CurrentCulture` to `it-IT` within the test, or
  asserting the invariant-culture literal directly), plus coverage for the UI-facing sites if they have
  existing tests.
- [x] Integration scenario to retest: a full `QuickFiler.Test` run after adding
  `CultureInfo.InvariantCulture` to every date/time `.ToString(...)` call in the five files listed
  above, confirming no existing literal assertions regress (they should not, since the default test
  culture is invariant-equivalent for `:`/`.`/`-`  separators in `en-US`).
- [x] Manual verification notes: a repository search for `.ToString("MM/dd/yyyy")`,
  `.ToString("HH:mm")`, `.ToString("hh:mm")`, or `.ToString("HH:mm:ss")` under `QuickFiler/Controllers/`
  with no adjacent `CultureInfo.InvariantCulture` argument should return no match after the fix.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
