# Pass-after run — the barrier defect (P4-T6)

Timestamp: 2026-09-01T10-55
Task: [P4-T6]
Working directory: WORKTREE

Command (leading executable substituted with the absolute path recorded by P0-T14):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcFormControllerUndoHandoffTests" "/Logger:trx;LogFileName=p4-t6.trx" /ResultsDirectory:FEATURE\evidence\regression-testing\p4-t6
```

EXIT_CODE: 0

## TRX outcome counts

Count of `outcome="Passed"` occurrences in the produced TRX file: **2**.
Count of `outcome="Failed"` occurrences: 0.

The `ResultSummary` element carries `outcome="Completed"` on a successful run rather than
`outcome="Passed"`, so the raw literal count and the per-test count agree at 2 here. That is the same
mechanism recorded in the P2-T5 artifact, where a failing run's `ResultSummary` did contribute an extra
`outcome="Failed"` occurrence.

## Results

| Outcome | Test |
|---|---|
| Passed | `BackGroundMoveAsync_WithPendingQueueItem_DoesNotWriteMetricsBeforeDrain` |
| Passed | `BackGroundMoveAsync_WithPendingQueueItem_DoesNotDispatchCleanupBeforeDrain` |

## Fail-before / pass-after pairing

| Test | P2-T5 (pre-fix) | P4-T6 (post-fix) |
|---|---|---|
| `BackGroundMoveAsync_WithPendingQueueItem_DoesNotWriteMetricsBeforeDrain` | Failed | Passed |
| `BackGroundMoveAsync_WithPendingQueueItem_DoesNotDispatchCleanupBeforeDrain` | Failed | Passed |

The same two test names that failed in P2-T5 now appear as passed. Neither test was edited between the
two runs: the only changes in between are the Phase 3 queue rewrite and the Phase 4 barrier insertion.
The tests are therefore a genuine regression pair rather than a test that was adjusted to fit the
implementation.

Output Summary: Both barrier tests pass against the fixed tree. With one item still parked behind a
closed gate, `BackGroundMoveAsync` no longer dispatches to the UI dispatcher: the metrics recorder count
is 0 when the ordering probe completes, where it was deterministically 1 before the fix. After the gate
is released and the returned task is awaited, metrics is recorded once and cleanup once, in that order.
The ordering constraint that issue 633 reported as unexpressed is now enforced by control flow.
