# Fail-before run — the barrier defect (P2-T5)

Timestamp: 2026-09-01T10-46
Task: [P2-T5] [expect-fail]
Working directory: WORKTREE

Command (leading executable substituted with the absolute path recorded by P0-T14):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcFormControllerUndoHandoffTests" "/Logger:trx;LogFileName=p2-t5.trx" /ResultsDirectory:FEATURE\evidence\regression-testing\p2-t5
```

EXIT_CODE: 1
ExpectedExitCode: 1

A non-zero exit code is the expected and required outcome of this task. The run was made against the
Phase 1 tree, which carries the behaviour-preserving `ItemProcessor` seam but not the fix.

## TRX outcome counts

| Measure | Value |
|---|---|
| `UnitTestResult` elements | 2 |
| `UnitTestResult` elements with `outcome="Failed"` | **2** |
| `UnitTestResult` elements with `outcome="Passed"` | 0 |
| `ResultSummary` `Counters` `total` / `executed` / `passed` / `failed` | 2 / 2 / 0 / 2 |
| Raw count of the literal `outcome="Failed"` anywhere in the file | 3 |

The raw literal count is 3 rather than 2 because a TRX produced by a failing run carries a run-level
`<ResultSummary outcome="Failed">` element in addition to one `<UnitTestResult>` element per test. The
per-test failed count, which is what the acceptance condition means and what the `Counters` element
independently corroborates, is **2**. The P1-T5 artifact's `outcome="Passed"` count of 5 was not
affected by the same element because `ResultSummary` carries `outcome="Completed"` on a successful run.

## Failures

### 1. `BackGroundMoveAsync_WithPendingQueueItem_DoesNotWriteMetricsBeforeDrain`

Failing assertion: the pre-release metrics-count assertion.

Verbatim failure message:

```
Expected CountOf(MetricsToken) to be 0 because the barrier withholds the metrics dispatch until the queue has drained, but found 1 (difference of 1).
```

### 2. `BackGroundMoveAsync_WithPendingQueueItem_DoesNotDispatchCleanupBeforeDrain`

Failing assertion: the pre-release metrics-count clause of its two-part pre-release assertion.

Verbatim failure message:

```
Expected CountOf(MetricsToken) to be 0 because cleanup is reached only through metrics, so a metrics dispatch made while the queue is undrained proves cleanup is unguarded too, but found 1 (difference of 1).
```

Both test names appear among the failures, and each failure names its own assertion.

Output Summary: The fail-before witness is genuine and discriminating. Both barrier tests fail against
the pre-fix tree, and both fail for exactly the reason the plan predicted: with one item still parked in
the queue behind a closed gate, the metrics recorder count is already 1 by the time the ordering probe
completes. That is direct evidence of the defect — `BackGroundMoveAsync` dispatched to the UI dispatcher
while the batch's filing work, and therefore its undo pushes, were still outstanding.

The observed count of 1 also confirms the determinism argument rather than merely being consistent with
it. The mocked `MoveEmailsAsync` returns an already-completed task, so the metrics operation is enqueued
at `ContextIdle` synchronously before `BackGroundMoveAsync()` returns to the caller; the probe posted
afterwards at the same priority therefore cannot complete until that metrics operation has run. Had the
ordering been a race rather than an enqueue-order fact, the count would have been unstable rather than
deterministically 1.

The `REMEDIATION-REQUIRED: fail-before witness did not fail` branch was not taken; neither test passed.
P4-T6 is the pass-after half of this record.
