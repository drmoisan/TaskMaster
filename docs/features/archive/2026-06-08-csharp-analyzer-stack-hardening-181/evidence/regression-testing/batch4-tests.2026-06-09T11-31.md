# Phase 4 (Batch 4) — Regression Evidence

Timestamp: 2026-06-09T11-31
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~IEnumerableExtensions_Tests|FullyQualifiedName~SubjectMapSco_Orchestration_Tests|FullyQualifiedName~AsyncMultiTasker_Tests" /InIsolation
EXIT_CODE: 0

Output Summary:
- Total tests: 44; Passed: 44; Failed: 0.
- Converted rows:
  - F1 IEnumerableExtensions_Tests.GetProgressMessage_WhenItemsAreComplete_UsesMeasuredRate —
    `Thread.Sleep(25)` replaced with `SpinWait.SpinUntil(() => stopwatch.Elapsed > TimeSpan.Zero, 100)`.
  - F2 IEnumerableExtensions_Tests.ToList_InternalHelper_ConsumesEnumerableAndReportsProgress —
    `Thread.Sleep(700)` x3 removed; source is now `Enumerable.Range(1,3)` (no sleep); the test passes an
    `onItemCompleted` delegate (4th reflection arg) that reports each item's percentage through the
    tracker, deterministically producing the required `Value > 0` report. Reflection Invoke signature
    updated to 4 args (optional defaults are not auto-applied by reflection).
  - G1 SubjectMapSco_Orchestration_Tests.Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress —
    `Thread.Sleep(20)` x3 and `SpinWait.SpinUntil(..., 1000)` removed; relies on the existing #181
    per-item `progress.Report` in `Consume` (synchronous), asserting `Reports.Count >= 2` directly.
  - E1 AsyncMultiTasker_Tests.AsyncMultiTaskChunker_SyncFuncOverload_WhenWorkSpansTimerInterval_ReportsProgress —
    `Thread.Sleep(350)` removed; injects `ManualFireTimerWrapper { FireOnStart = true }` via the new
    `timerFactory` param so the timer fires Elapsed synchronously on StartTimer, producing a non-final
    progress report deterministically.
  - E2 AsyncMultiTasker_Tests.AsyncMultiTaskChunker_ActionOverload_WhenWorkSpansTimerInterval_ReportsProgress —
    same conversion (action overload).
  - E3 AsyncMultiTasker_Tests.GetReportMessage_WhenInvoked_FormatsPrefixAndCounts — `Thread.Sleep(20)`
    replaced with started-then-stopped Stopwatch guarded by `SpinWait.SpinUntil(... > TimeSpan.Zero, 100)`.

Production seams:
- S3: all four `AsyncMultiTasker.AsyncMultiTaskChunker` overloads gained an optional
  `Func<TimeSpan, ITimerWrapper> timerFactory = null` parameter; null coalesces to
  `interval => new TimerWrapper(interval)` (current behavior). Existing internal callers compile
  unchanged (optional trailing parameter). Build analyzer-clean (0 errors).
- S4 (Consume): no further production change — the existing #181 per-item hook is sufficient (see
  p4-t1-consume-hook-disposition).
- S4 (ToList): the Phase 3 `onItemCompleted` hook is sufficient for F2 (see p4-t3-tolist-hook-sufficiency).

Out-of-inventory note: `AsyncMultiTasker_Tests` contains two ADDITIONAL async-overload progress tests
(`AsyncFuncOverload_WhenWorkSpansTimerInterval_ReportsProgress`,
`AsyncTaskOverload_WhenWorkSpansTimerInterval_ReportsProgressAndCompletion`) that use `await Task.Delay(350)`.
These methods are NOT in the research's 24-row inventory (which catalogs only E1/E2/E3). `Task.Delay` is a
BannedApiAnalyzers RS0030 symbol held at `suggestion` severity (does not break the build). They are
pre-existing and outside this cycle's defined conversion scope; modifying them would widen scope beyond the
inventory. Recorded here and in the Phase 6 inventory disposition; not converted.

No `Thread.Sleep`, `signal.Wait(<timeout>)`, or `SpinWait.SpinUntil(..., <timeout>)` (as a wall-clock wait)
remain in the three targeted files; the `SpinWait.SpinUntil(... > TimeSpan.Zero, ...)` usages are structural
non-zero-elapsed guarantees (Risk R7).
