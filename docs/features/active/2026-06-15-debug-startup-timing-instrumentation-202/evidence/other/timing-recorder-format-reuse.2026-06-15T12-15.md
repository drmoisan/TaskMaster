# Design Resolution — Startup Timing Recorder Table-Format Reuse (Issue #202)

Timestamp: 2026-06-15T12-15

Preconditions reviewed:
- `UtilitiesCS/HelperClasses/SegmentStopWatch.cs` (reviewed)
- `UtilitiesCS/HelperClasses/PrettyPrint.cs` (reviewed)

## Selected design (no alternatives)

`StartupTimingRecorder` maintains its OWN ordered collection of
`(string phaseName, TimeSpan elapsed)` pairs in insertion order. It does NOT wrap or call
`SegmentStopWatch`.

## Verified reason for not wrapping SegmentStopWatch

`SegmentStopWatch.GetDurations()` (`UtilitiesCS/HelperClasses/SegmentStopWatch.cs` line 90)
builds the TOTAL row from the watch's own `this.Elapsed`:

```csharp
_durations.Push(("TOTAL", this.Elapsed));
```

`this.Elapsed` is the elapsed time of the `Stopwatch` base class. For a recorder that is fed
PRE-MEASURED, injected `TimeSpan` spans (as required for deterministic unit testing without a
live Outlook process), the watch is never started/stopped, so `this.Elapsed` is
`TimeSpan.Zero`. Wrapping `SegmentStopWatch` for injected spans would therefore yield an
always-zero TOTAL row, which is unsatisfiable against AC3 (TOTAL must reflect aggregate
elapsed time) and P2-T7 (TOTAL must equal the sum of injected spans). The recorder instead
computes its TOTAL as the SUM of all recorded spans.

## Reused formatting primitive (genuinely reusable)

`UtilitiesCS.HelperClasses.PrettyPrinters.ToFormattedText(this string[][] jagged, string[] headers = null, Enums.Justification[] justifications = default, string title = null)`
defined in `UtilitiesCS/HelperClasses/PrettyPrint.cs` lines 179-184.

This is the same jagged `string[][]` overload that `SegmentStopWatch.GetDurations` calls
(`SegmentStopWatch.cs` lines 103-107). The recorder reuses this primitive for column
alignment and does NOT reimplement column alignment.

Note on resolved namespace: although `PrettyPrinters` is documented in some prose with a
`UtilitiesCS.HelperClasses` label, the class is actually declared in `namespace UtilitiesCS`
(see `PrettyPrint.cs` line 7). The extension method resolves via `using UtilitiesCS;`. The
`Enums.Justification` enum is `UtilitiesCS.Enums.Justification` (`UtilitiesCS/Interfaces/Enums.cs`).

## Call convention

`FormatTable()` invokes the primitive with:
- headers: `["Duration", "Action"]`
- justifications: `[Enums.Justification.Right, Enums.Justification.Left]`

consistent with the existing `SegmentStopWatch.GetDurations` convention. One row is emitted per
recorded phase, followed by a final `TOTAL` row whose duration equals the SUM of all recorded
spans.

No production code was modified by this task.
