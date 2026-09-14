# P3-T7 — Scoped run of the new failure-contract test class

Timestamp: 2026-09-13T03-01

Command: the same command P1-T4 used, with the test-case filter replaced by `FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Table.GetTableInViewAsyncFailureContractTests`, the results directory by `Join-Path $env:TEMP "taskmaster-838\p3-classrun"` and the result-file name by `p3-t7-classrun.trx`.

EXIT_CODE: 0

## Result-file selection

TRX_FILE_COUNT=1. The newest file selected by the fixed selection rule is `p3-t7-classrun.trx`, last written 2026-09-13T03-01-38. Counters: total 5, executed 5, passed 5, failed 0.

## Per-test outcomes

```
GetTableInViewAsync_AbsorbedDefaultWithOuterTokenCancelled_ThrowsOperationCanceledNotTimeout = Passed
GetTableInViewAsync_CounterAtRetryCeilingWithTaskCanceled_ThrowsTimeoutException = Passed
GetTableInViewAsync_CounterAtRetryCeilingWithTimeout_ThrowsTimeoutExceptionPreservingInner = Passed
GetTableInViewAsync_RunWithTimeoutExhaustsRetries_ThrowsTimeoutException = Passed
GetTableInViewAsync_TimeoutSourceThrowsTaskCanceledAfterCancellingOuterToken_PropagatesCancellation = Passed
```

Output Summary: the run exits 0 and the result file records total 5, passed 5 and failed 0, with the five `testName` values equal to the five names in the plan's delivered-source table. Every acceptance clause holds. The five tests together exercise each route of the delivered failure contract: an exhausted helper retry budget reported as a timeout, a caller token cancelled in flight reported as cancellation, the task-cancelled and the timeout routes at the retry ceiling reported as timeouts naming the retry and the budget, the timeout route additionally preserving the originating exception by reference, and the absorbed-default state with a cancelled caller token reported as cancellation rather than as a timeout. The last of these is the verifier named by the amended acceptance criterion 12; its pass is what establishes that the guard's cancellation check precedes its timeout throw, because the reverse ordering would surface a `TimeoutException` and the assertion demands an `OperationCanceledException`. The result file remains at the out-of-repository scratch root.
