# Phase 3 (Batch 3) — Regression Evidence

Timestamp: 2026-06-09T11-31
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~ObsoleteBayesianClassifier_Tests|FullyQualifiedName~BayesianPerformanceMeasurement_Tests|FullyQualifiedName~SegmentStopWatch_Tests" /InIsolation
EXIT_CODE: 0

Output Summary:
- Total tests: 52; Passed: 52; Failed: 0.
- Converted rows:
  - I3 ObsoleteBayesianClassifier_Tests.GetReportMessage_WithCompletedItems_FormatsCorrectly —
    `Thread.Sleep(10)` replaced with `SpinWait.SpinUntil(() => sw.Elapsed > TimeSpan.Zero, 100)`.
  - I4 BayesianPerformanceMeasurement_Tests.PrivateProgressHelpers_FormatExpectedMessages —
    `Thread.Sleep(20)` replaced with `SpinWait.SpinUntil(() => stopwatch.Elapsed > TimeSpan.Zero, 100)`.
  - H1 SegmentStopWatch_Tests.StartAndStop_ReturnSameInstance_AndResetClearsElapsed —
    `Thread.Sleep(20)` replaced with `SpinWait.SpinUntil(() => sut.Elapsed > TimeSpan.Zero, 200)`.
  - H2 SegmentStopWatch_Tests.LogDuration_CapturesMultipleNamedSegments — both `Thread.Sleep(20)` replaced;
    the second spins until `Elapsed > afterFirst` so each segment Duration is strictly > 0 (multi-segment
    duration-stack assertions preserved).

Production seam:
- S4 partial: `IEnumerableExtensions.ToList<T>` gained an optional `Action<int> onItemCompleted = null`
  parameter invoked per consumed item (inside the existing `WithProgressReporting` callback), alongside
  the retained `System.Threading.Timer`. Production default null (no-op) -> behavior unchanged.
  Build analyzer-clean (0 errors).

No `Thread.Sleep` remains in any of the three targeted files. The `SpinWait.SpinUntil` usages are
structural non-zero-elapsed guarantees that return in microseconds, not wall-clock waits (Risk R7).
