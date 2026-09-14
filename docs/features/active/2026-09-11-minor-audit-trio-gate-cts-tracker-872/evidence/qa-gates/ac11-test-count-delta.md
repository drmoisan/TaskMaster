# AC11 — Per-Assembly Executed-Test Count Delta

Timestamp: 2026-09-13T15-39
Task: [P2-T8]

Verdict: PASS

## Measured Deltas

| Assembly | Phase 0 baseline | Phase 2 value | Difference | Required |
|---|---|---|---|---|
| UtilitiesCS.Test | 4903 | 4897 | -6 | exactly -6 |
| QuickFiler.Test | 1395 | 1397 | +2 | exactly +2 |

Both differences equal their required values exactly. Both runs recorded `Failed: 0`.

Sources: the baseline values are the `TotalTests:` lines of
`evidence/baseline/tests-utilitiescs.md` (P0-T8) and `evidence/baseline/tests-quickfiler.md` (P0-T9).
The Phase 2 values are the `TotalTests:` lines of `evidence/qa-gates/qc-tests-utilitiescs.md` (P2-T5)
and `evidence/qa-gates/qc-tests-quickfiler.md` (P2-T6).

## The Arithmetic, Stated

**UtilitiesCS.Test: minus nine plus three equals minus six.**

- Minus nine for the nine test methods removed with the dormant tracker's test class when P1-T13
  deleted `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`. P0-T13 pinned that file at
  `TestMethodCount: 9` and `DataRowCount: 0`. The data-row count of zero is what makes the executed
  count equal the method count: a parameterised test would contribute one executed test per data row
  rather than one per method, and the removal term would then not equal nine.
- Plus three for the three disposal tests added by P1-T6, P1-T7 and P1-T8 to
  `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs`:
  `Dispose_WhenPackageConstructedTheSource_ReleasesIt`,
  `Dispose_WhenCallerSuppliedTheSource_LeavesItUsable` and
  `Dispose_OnSpawnedChild_DoesNotReleaseTheParentsSource`. None is parameterised.

**QuickFiler.Test: plus two.**

- Plus two for the two log-assertion tests added by P1-T2 and P1-T3 to
  `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs`:
  `DequeueAsync_ZeroAcceptedAndCapReached_LogsScanCapBoundAndStopDecision` and
  `DequeueAsync_ZeroAcceptedAndCeilingReached_LogsCeilingBoundNotScanCapBound`. Neither is
  parameterised. No test was removed from this assembly.

## Comparability Of The Two Measurements

Per D5 the `/TestCaseFilter:` value is byte-identical between P0-T8 and P2-T5 and between P0-T9 and
P2-T6. Both filters exclude the LiveOutlook category and the same four shell-icon test classes, none of
which this delivery touches, so the exclusion contributes the same constant to the baseline and to the
Phase 2 figure and cancels in the difference. A difference measured across two different filters would
not be attributable to the delivery; this one is.

Per D6 the repository coverage runner's population is wider and its total of 7218 is not used here. It
is recorded in the P2-T7 artifact for the repository headline only.

## Failure Counters

Both Phase 2 runs printed `Test Run Successful.` with the passed count equal to the total count and, per
D8, emitted no `Failed:` line and no `Skipped:` line at all on a green run. `Failed: 0` is transcribed
for both on that basis. D9's single-re-run allowance for the sporadic dictionary-extensions test
`TryAddValuesAsync_UpdatesExistingValue` was not invoked: neither command was run more than once,
because neither produced a failure.
