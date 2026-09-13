# P4-T6 — Per-test outcomes for the tests the acceptance criteria name

Timestamp: 2026-09-13T03-15

Command: a single pwsh payload that selects the result file under `Join-Path $env:TEMP "taskmaster-838\final-tests\results"` by the fixed selection rule, reads its `UnitTestResult` elements by their `testName` and `outcome` attributes, prints `NAME=outcome` for each of the six named tests, and exits non-zero if any named test is absent or reports any outcome other than `Passed`.

EXIT_CODE: 0

```
GetTableInViewAsync_RunWithTimeoutExhaustsRetries_ThrowsTimeoutException=Passed
GetTableInViewAsync_TimeoutSourceThrowsTaskCanceledAfterCancellingOuterToken_PropagatesCancellation=Passed
GetTableInViewAsync_CounterAtRetryCeilingWithTaskCanceled_ThrowsTimeoutException=Passed
GetTableInViewAsync_CounterAtRetryCeilingWithTimeout_ThrowsTimeoutExceptionPreservingInner=Passed
GetTableInViewAsync_AbsorbedDefaultWithOuterTokenCancelled_ThrowsOperationCanceledNotTimeout=Passed
GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException=Passed
NOT_PASSED_OR_ABSENT=0
```

Output Summary: all six named tests are present in the final result file and all six report outcome `Passed`. The first five are the new failure-contract tests listed in the plan's delivered-source table. The sixth, `GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException`, is the pre-existing test declared at `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` line 1296; its continued pass is what shows the fix did not disturb the cancellation contract the tree already asserted, and it is read here rather than assumed because that file is deliberately not edited by this change. The absence check is part of the gate rather than an aside, because a test that silently stopped being discovered would otherwise read as an unremarkable green run.
