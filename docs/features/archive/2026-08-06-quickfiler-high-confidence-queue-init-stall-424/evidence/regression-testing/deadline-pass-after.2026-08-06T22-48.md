# [P1-T4] Deadline Regression Test — PASS AFTER (AC 11 evidence)

- **Issue:** #424
- **Task:** [P1-T4]
- **Test:** `QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline`
- **Test file:** `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs`
- **Production state:** FIXED. `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` now carries `DefaultFirstBatchDeadline = TimeSpan.FromSeconds(12)` and the loop-top deadline exit added by `[P1-T3]`.

Timestamp: 2026-08-06T22-48

Command: `"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:"scripts\vscode\TaskMaster.cli.runsettings" /InIsolation /TestCaseFilter:"FullyQualifiedName~DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline"`

EXIT_CODE: 0

Output Summary:

```
Passed DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline [179 ms]
Test Run Successful.
Total tests: 1
     Passed: 1
 Total time: 1.2988 Seconds
```

## Fail-before / pass-after pair (AC 11 satisfied)

| Run | Production state | `tryTakeNext` invocations | Result | EXIT_CODE |
|---|---|---|---|---|
| `[P1-T2]` fail-before | pre-fix gate (no deadline) | **51** — whole source scanned to exhaustion | FAILED (`Expected takeCount to be less than or equal to 13 ... but found 51`) | 1 |
| `[P1-T4]` pass-after | post-fix gate (12 s deadline) | **<= 13** — bounded by the budget | PASSED | 0 |

The same unmodified test method produces both results; only production code changed between them. The scan is now bounded by the first-batch budget instead of by folder size.

## Implementation verified by this test

- `DefaultFirstBatchDeadline` is `internal static readonly TimeSpan` = 12 s (`QfcStreamingDequeueConfidenceGate.cs:22`), within the spec's 10-15 s range.
- The deadline is evaluated at loop top **after** the cancellation check and **before** `_tryTakeNext()` (`QfcStreamingDequeueConfidenceGate.cs:98-106`), so an expired budget returns `accepted` without taking another candidate from the source — unscanned candidates remain in the master queue.
- Elapsed time is measured through the already-injected `TimeProvider` via `GetTimestamp()` / `GetElapsedTime(start)`, so `FakeTimeProvider` drives it deterministically with no wall-clock dependence.

## Regression check at this point

The full gate suite was run immediately after `[P1-T3]` with the same runner:

```
Test Run Successful.
Total tests: 9
     Passed: 9
```

All 8 pre-existing gate tests (accept/reject/backfill, inclusive threshold, partial results on exhaustion, discard-below-threshold, cancellation, `FakeTimeProvider` empty-poll, `sourceActive` continue-polling) pass unmodified in behavior alongside the new deadline test.

**This is the AC 11 pass-after evidence.** Paired with `deadline-fail-before.2026-08-06T22-41.md`.
