# Phase 2 (Batch 2) — Regression Evidence

Timestamp: 2026-06-09T11-31
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~TimedQueueOfActions_Tests|FullyQualifiedName~BayesianClassifierGroup_Tests|FullyQualifiedName~BayesianClassifierGroupTests" /InIsolation
EXIT_CODE: 0

Output Summary:
- Total tests: 50; Passed: 50; Failed: 0.
- Converted rows verified deterministic:
  - C1 Enqueue_InvokesBatchActionsOnTimerInterval — `signal.Wait(1000)` removed; `ManualFireTimerWrapper`
    injected via `TimerFactory`; `FireElapsed()` dispatches the batch synchronously.
  - C2 EmptyQueue_AfterSeveralIntervals_StopsTimer — `SpinWait.SpinUntil(..., 5000)` removed; explicit
    `FireElapsed()` loop (bounded safety cap 10) drives the empty-tick threshold until the timer auto-stops.
  - C3 ConcurrentEnqueue_BatchesAllItems — `signal.Wait(1000)` removed; concurrent enqueues complete, then
    a single `FireElapsed()` dispatches all 25 items in one batch.
  - C4 Configuration_PropertyChanged_RestartsTimerOnWriteIntervalChange — `Thread.Sleep(50)` removed; the
    PropertyChanged-driven StopTimer+TryStartTimer restart is synchronous via the injected factory.
  - I1 BayesianClassifierGroup_Tests.GetReportMessage_WithCompletedItems_FormatsCorrectly — `Thread.Sleep(10)`
    replaced with `SpinWait.SpinUntil(() => sw.Elapsed > TimeSpan.Zero, 100)` (structural non-zero elapsed, Risk R7).
  - I2 BayesianClassifierGroupTests.GetReportMessage_WithCompletedItems_IncludesSpeed — same conversion; `per sec`
    string still asserted.

Production seam:
- S2: `TimedQueueOfActions<T>.TimerFactory` (internal Func<TimeSpan, ITimerWrapper>) added, default
  `interval => new TimerWrapper(interval)`; `StartTimer` now calls `TimerFactory(Config.WriteInterval)`.
  Production default behavior unchanged; the stop/dispose-then-recreate lifecycle is preserved (factory
  invoked on each StartTimer). Build is analyzer-clean (0 errors).

No `Thread.Sleep`, `signal.Wait(<timeout>)`, or `SpinWait.SpinUntil(..., <timeout>)` remain in
TimedQueueOfActions_Tests.cs; the only `SpinWait` usages introduced (I1/I2) are structural
non-zero-elapsed guarantees that return in microseconds, not wall-clock waits.
