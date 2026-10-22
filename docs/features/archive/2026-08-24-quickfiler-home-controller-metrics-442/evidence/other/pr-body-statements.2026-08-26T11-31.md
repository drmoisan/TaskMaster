# PR Body Statements (for the `pr-author` skill to consume)

Timestamp: 2026-08-26T11-31
Task: [P7-T1]
Command: not applicable; this artifact records required PR body content
EXIT_CODE: 0

The statements below must appear in the pull request body for this feature. They exist because each
records a deliberate, externally visible behaviour change or a deliberate test disposition that a
reviewer must see rather than infer from the diff.

## Required statements

### 1. The EFC metrics row moves from 11 fields to 12

The EmailFiler session-metrics CSV row previously rendered 11 comma-separated fields, because the
interpolated `ToRecipientsName` and `SenderName` were concatenated with no separator between them,
producing a single collapsed field. The row now renders exactly 12 fields, with recipient and sender
separated. Rows written before this change remain in the file in their original 11-field shape; the
log is append-only and no migration is possible or required.

### 2. EFC durations change from zero to real values

Every EmailFiler duration was previously `0`, because `_stopWatch` was allocated with
`new Stopwatch()` and never started, so `Elapsed` reported a zero interval for the life of the
controller. The two construction sites now use `Stopwatch.StartNew()`, and durations reflect real
elapsed time.

### 3. All durations become untruncated and culture-invariant

Two changes combine here. First, duration reads move from `TimeSpan.Seconds`, which is the 0-59
component of the interval, to `TimeSpan.TotalSeconds`, which is the whole interval; a 90-second move
previously reported `30` and now reports `90`. Second, the six numeric format sites now pass
`CultureInfo.InvariantCulture`, so the decimal separator is always `.`; under a locale such as
`de-DE` the minutes field previously rendered `2,00`, which both misreported the value and inflated
the CSV field count. The date and time format calls are deliberately unchanged.

### 4. No in-repo reader of the session-metrics settings key exists

A repository-wide search for `EmailSession` across `*.cs`, `*.py`, `*.ps1`, `*.ipynb`, `*.R`, and
`*.sql` returns exactly six files, enumerated in the spec's Data / API / Config Impact section:

**Three settings-plumbing declarations**

- `TaskMaster/Properties/Settings.Designer.cs:436-454`
- `TaskMaster/AppGlobals/AppStagingFilenames.cs:85-93`
- `UtilitiesCS/Interfaces/IGlobals/IAppStagingFilenames.cs:10`

**Three writers**

- `QuickFiler/Controllers/QfcHomeController.cs:373`
- `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:229`
- `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:141`

There is no parser, no reader, and no schema consumer anywhere in the repository; the artifact is
write-only from the codebase's perspective. The residual risk is confined to a human-maintained
spreadsheet outside the repository whose EFC column count shifts from 11 to 12. That risk is stated
here rather than gated on.

### 5. The `int` to `double` widening changes `##0` rounding for multi-item EFC moves

`BuildQuickFileMetricLines` previously divided an `int` `elapsedSeconds` by `moved.Count` using
integer division, discarding the remainder before formatting. The parameter is now `double` and the
division is real. For a move of 3 items over 8 seconds the per-item duration was `2` and is now
`2.6667`, which the `##0` format renders as `3` and the `##0.00` minutes format renders as `0.04`,
against `2` and `0.03` before.

This is a deliberate behaviour change, not a rounding accident. It is pinned by the named test
`BuildQuickFileMetricLines_WithMultipleMovedItems_PinsRealDivisionRounding`, which asserts the
substring `,3,0.04,`. Both widened parameters are `internal`, so no public API changes; they are
visible only to `QuickFiler.Test` through `InternalsVisibleTo`.

### 6. Disposition of the four deliberately broken tests

| Test | Disposition |
| --- | --- |
| `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine` | **Updated.** Its expected literal now carries the separated substring `,Recipient,Sender,` in place of the concatenated `RecipientSender`, and 12 fields in place of 11. |
| `QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract` | **Deleted and replaced.** It pinned a defect (an interface member that could only throw) and no pinning assertion for a fixed defect may survive. It is replaced by two tests: `QuickFileMetricsWriteFilenameOnly_WithAbsentPrerequisites_DoesNotThrow` and `QuickFileMetricsWriteFilenameOnly_WithPrerequisites_DelegatesToThreeArgumentOverload`. |
| `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` | **Updated.** It now also sets `_stopWatchMoved` by reflection. Without that the test would dereference a null field once the duration read moved to the moved-items stopwatch. Its assertions are unchanged. |
| `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay` | **Deleted.** Its body never called `NonBlockingProducer`; it exercised the time provider's delay directly. After both `NonBlockingProducer` overloads were removed that seam has no production call site, so the test would have asserted only that the fake time provider works. Deletion also recovered line budget against the 500-line cap. |

## Outstanding item the PR body must also disclose

The full coverage-enabled test suite ends with **one failing test**, and it is not one of the four
above:

`QuickFiler.Controllers.Tests.EfcHomeControllerTests.ExecuteMovesAsync_WhenAlreadyExecuting_ReturnsWithoutAccessingNullFields`

It fails with `System.ArgumentException: Object of type 'System.Boolean' cannot be converted to type
'System.Int32'` because `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs:64` injects a
`System.Boolean` by reflection into `_isExecuting`, which this change converts from
`private volatile bool` to `private int` as AC-14 requires. That file is on this feature's
forbidden-to-write list, so the one-line delta `SetField(controller, "_isExecuting", 1);` was not
applied. Full diagnosis is in `evidence/qa-gates/mstest-coverage.2026-08-26T11-30.md`.

This must be resolved before the PR can merge and is escalated to the epic.
