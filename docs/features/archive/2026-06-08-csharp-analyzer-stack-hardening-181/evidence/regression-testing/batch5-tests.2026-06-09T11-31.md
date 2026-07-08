# Phase 5 (Batch 5) — Regression Evidence

Timestamp: 2026-06-09T11-31
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~FolderRemapTree_Tests|FullyQualifiedName~OlTableExtensions_Tests" /InIsolation
EXIT_CODE: 0

Output Summary:
- Total tests: 89; Passed: 89; Failed: 0.
- Converted rows:
  - K1 FolderRemapTree_Tests.WireNotifications_OnMappedToChange_RaisesPropertyChanged —
    `eventFired.Wait(TimeSpan.FromMilliseconds(500))` replaced with deterministic firing: the test injects a
    `ManualFireTimerWrapper`-backed `TimedBatchAction` into `_batchNotifier` (seam S6 mechanism), mutates
    `MappedTo`, fires the stub synchronously, and asserts `eventFired.IsSet`. (~2 ms, down from up to 500 ms.)
  - J1 OlTableExtensions_Tests.GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry —
    `timeoutMs: 5` passed via seam S5; mock first-call block reduced from `Thread.Sleep(2100)` to
    `Thread.Sleep(20)`. Documented PARTIAL improvement (Risk R5) — see scope-change-J1; residual 20 ms is
    genuinely-required synchronization to exceed the timeout, not a flakiness mask. (~0.2 s, down from ~2 s.)

Production seams:
- S6: `FolderRemapTree` gained an `internal` constructor accepting `Func<TimeSpan, ITimerWrapper>
  batchNotifierTimerFactory` that re-initializes `_batchNotifier = new TimedBatchAction(50 ms, null, factory)`;
  the public constructors keep the field-initializer default (real timer) so production behavior is unchanged.
  `TimedBatchAction.cs` was NOT modified (its internal timerFactory ctor already existed).
- S5: `OlTableExtensions.GetTableInViewAsync` gained `int timeoutMs = 2000`, passed through to
  `TimeOutTask.RunWithTimeout` and propagated into the recursive retry. Production callers receive the 2000 ms
  default (unchanged). `TimeOutTask.cs` was NOT modified.

Regression fix (in-scope, mechanically necessary): adding the optional `timeoutMs` parameter changed the
`GetTableInViewAsync` reflection signature, so three pre-existing tests
(`GetTableInViewAsync_NullTableView_ThrowsInvalidOperationException`,
`GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException`,
`GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot`) that pin the method via a
reflection parameter-type array were updated from the 3-type signature to the 4-type signature and pass the
explicit default `2000`. These updates restore them to green (zero-regression) and do not change their intent.

Retained (intentionally NOT converted): B1-B3 (TimerWrapper_Tests, real-timer integration, `[DoNotParallelize]`)
and L1 (ThreadSafeSingleShotGuard_Tests, no-timeout start gate). See retained-waits-justification.

Build analyzer-clean (0 errors).
