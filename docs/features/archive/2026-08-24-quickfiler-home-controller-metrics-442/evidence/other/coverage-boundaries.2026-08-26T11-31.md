# Known Coverage Boundaries

Timestamp: 2026-08-26T11-31
Task: [P7-T3]
Command: not applicable; this artifact records two declared coverage boundaries
EXIT_CODE: 0

The spec declares two boundaries rather than papering over them. Both are recorded here with their
reasons and with the substitute evidence used in each case.

## Boundary 1 — QFC seconds truncation is not asserted numerically

**What is not asserted.** No test asserts a specific numeric duration value produced by the QFC
truncation fix at `QuickFiler/Controllers/QfcHomeController.Metrics.cs`.

**Why.** A `Stopwatch` cannot be given an arbitrary elapsed value through its public surface. The
only ways to make one report a chosen interval are to reflect into its internal tick field or to
start it, wait a real interval, and stop it. The second is a wall-clock wait, prohibited by
`.claude/rules/general-unit-test.md` and by AC-17, and it would also be non-deterministic: a short
wait does not guarantee any particular elapsed value, and a long enough wait to distinguish 90
seconds from the 0-59 `Seconds` component would make the suite unusable.

Reflection into the internal tick field **is** used, by the `StoppedStopwatchWithElapsed(int)`
helper, and it is what makes `WriteMetricsAsync_ReadsMovedStopwatchForDuration` deterministic. But
that helper pins the internal representation of a BCL type, so it is used only for the coarse
assertion the plan specifies, `It.Is<double>(d => d > 0)`, and not to assert an exact rendered
duration string on the QFC side.

**Where the fix is asserted instead.** The truncation fix is asserted on the **EFC** side, where the
elapsed value is a plain `double` parameter rather than a stopwatch reading. The named test

`BuildQuickFileMetricLines_WithNinetySeconds_RendersUntruncatedDuration`

invokes `BuildQuickFileMetricLines` with `elapsedSeconds = 90` and one moved item and asserts the
rendered line contains `,90,1.50,`. That is the exact numeric assertion the QFC side cannot carry.

**Falsifiable half on the QFC side.** That EFC test passes both before and after the fix, because
the 0-59 truncation defect lives where the `TimeSpan` component is read, not inside
`BuildQuickFileMetricLines`. The falsifiable half is therefore the search gate: `Elapsed.Seconds`
under `QuickFiler/Controllers/` returned 4 hits before and returns 0 after, recorded in
`evidence/qa-gates/qfc-stopwatch-search-census.2026-08-26T11-16.md`. Together the numeric EFC
assertion and the QFC search gate cover AC-7.

## Boundary 2 — `OlStartTime` is not asserted

**What is not asserted.** No test asserts the value of `OlStartTime`, the appointment start that
`WriteMetricsAsync` computes at `QuickFiler/Controllers/QfcHomeController.Metrics.cs:125`.

**Why.** `OlStartTime` is consumed only by `WriteMoveToCalendar`, which passes it to
`olAppointment.Start`. `UtilitiesCS.Calendar.GetCalendar("Email Time", Globals.Ol.App.Session)`
returns `null` in every unit fixture, because the fixture's calendar root enumerates no subfolders.
`WriteMoveToCalendar` then takes its `OlAppointment = null` branch and the computed start value
reaches no observer that a test can inspect. Making it observable would require a real MAPI
`Folder` whose `Items.Add()` returns a real `AppointmentItem` accepting a `Start` assignment and a
`Save()`, which is a live Outlook process.

**Where the change is verified instead.** By the [P4-T11] search census, recorded in
`evidence/qa-gates/qfc-stopwatch-search-census.2026-08-26T11-16.md`:

- `git grep -n "OlEndTime.Subtract" -- QuickFiler/Controllers/QfcHomeController.Metrics.cs` returns
  exactly one hit, and its text contains `_stopWatchMoved.Elapsed`:

  ```
  QuickFiler/Controllers/QfcHomeController.Metrics.cs:125:            OlStartTime = OlEndTime.Subtract(_stopWatchMoved.Elapsed);
  ```

- `git grep -nF "(int)Duration" -- QuickFiler/Controllers/QfcHomeController.Metrics.cs` returns no
  hit, confirming the truncating cast that previously reconstructed the span is gone.

AC-8 states explicitly that it is verified by inspection rather than by a test for this reason.

## Related residual, recorded for completeness

The same live-Outlook boundary accounts for the only member below the 90.00% coverage floor in
[P6-T6]: `QuickFileMetrics_WRITE` measures 88.37% across its four overloads, with the uncovered
residue confined to the Outlook-appointment block at
`QuickFiler/Controllers/QfcHomeController.Metrics.cs:79-89`. That block is exempt under the
COM/VSTO/WinForms coverage exemption in CLAUDE.md § UT2 (c). The justification is member-specific
and is recorded in `evidence/qa-gates/coverage-delta.2026-08-26T11-30.md`.
