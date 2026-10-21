# New ordering tests are visible and passing (P7-T8)

Timestamp: 2026-09-01T11-12
Task: [P7-T8]
Working directory: WORKTREE

Command (leading executable substituted with the absolute path recorded by P0-T14):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcFormControllerUndoHandoffTests" "/Logger:trx;LogFileName=p7-t8.trx" /ResultsDirectory:FEATURE\evidence\qa-gates\p7-t8
```

EXIT_CODE: 0

Count of `outcome="Passed"` occurrences in the produced TRX: **5**.
Count of `outcome="Failed"` occurrences: **0**.

## Verbatim list of test names found in the produced TRX

| Outcome | Test |
|---|---|
| Passed | `BackGroundMoveAsync_WithPendingQueueItem_DoesNotDispatchCleanupBeforeDrain` |
| Passed | `BackGroundMoveAsync_WhenGroupsIsNull_ReturnsWithoutTouchingQueue` |
| Passed | `BackGroundMoveAsync_AfterQueueDrains_WritesMetricsThenCleansUp` |
| Passed | `BackGroundMoveAsync_WithPendingQueueItem_DoesNotWriteMetricsBeforeDrain` |
| Passed | `BackGroundMoveAsync_WhenParentIsNull_ReturnsWithoutThrowing` |

All five names the acceptance condition enumerates are present:

1. `BackGroundMoveAsync_WithPendingQueueItem_DoesNotDispatchCleanupBeforeDrain` — present, passed
2. `BackGroundMoveAsync_WithPendingQueueItem_DoesNotWriteMetricsBeforeDrain` — present, passed
3. `BackGroundMoveAsync_AfterQueueDrains_WritesMetricsThenCleansUp` — present, passed
4. `BackGroundMoveAsync_WhenParentIsNull_ReturnsWithoutThrowing` — present, passed
5. `BackGroundMoveAsync_WhenGroupsIsNull_ReturnsWithoutTouchingQueue` — present, passed

No `ErrorInfo` message is present in the TRX.

Output Summary: All five ordering tests are discovered and pass. Discovery is the functional half of
AC17: the tests appear in run output only because
`QuickFiler.Test/QuickFiler.Test.csproj` carries the `<Compile Include>` entry that P2-T1 added and
P6-T7 verified. Without it the file would not compile into the assembly and this run would have
discovered zero tests rather than failing to build.

Together the five cover the whole of the changed control flow in `BackGroundMoveAsync`: both barrier
cases with work outstanding, the post-drain ordering, the newly added `_parent` guard clause, and the
pre-existing `_groups` guard clause.

This artifact supplies evidence for the AC7 check-off in P8-T11, the AC8 check-off in P8-T12, the AC9
check-off in P8-T13, and, with P6-T7, the AC17 check-off in P8-T21.
