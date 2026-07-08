# Phase 6 (P6-T1) — Residual Prohibited-Timing Scan

Timestamp: 2026-06-09T11-31
Scope: UtilitiesCS.Test/
Patterns scanned: `Thread\.Sleep`, `\.Wait\(\s*\d`, `\.Wait\(\s*TimeSpan`, `SpinWait\.SpinUntil\(.*,\s*\d`

## Thread.Sleep

| Location | Status |
|---|---|
| TestHelpers/ManualFireTimerWrapper.cs:10 | NOT CODE — the words "Thread.Sleep" appear inside an XML-doc comment describing what the helper replaces. No executable Thread.Sleep. |
| OutlookObjects/Table/OlTableExtensions_Tests.cs:1287 | J1 DOCUMENTED PARTIAL improvement — residual `Thread.Sleep(20)` with `timeoutMs: 5`. Genuinely-required synchronization to exceed the 5 ms timeout; full determinism needs an out-of-scope `TimeOutTask` refactor (see scope-change-J1). |

No other `Thread.Sleep` remains. Every cataloged `Thread.Sleep` (A1, C4, D1, E1, E2, E3, F1, F2, G1, H1, H2,
I1, I2, I3, I4) is gone.

D1 correction: the ConfigController STA-pump `Thread.Sleep(10)` is removed; the loop now pumps with
`Application.DoEvents()` and yields with `Thread.Yield()` (a scheduler yield, not the banned `Thread.Sleep`
and not a wall-clock wait). A bare `GetAwaiter().GetResult()` deadlocked because `SaveAsync` installs a
WindowsFormsSynchronizationContext and posts its continuation to the STA message queue; the deterministic
pump is required to let that continuation run. `Thread.Yield()` is not in BannedSymbols.txt.

## .Wait(<digit/TimeSpan>)

| Location | Status |
|---|---|
| ReusableTypeClasses/TimerWrapper_Tests.cs:45,62,80 | B1-B3 INTENTIONALLY RETAINED — real-timer integration tests, `[DoNotParallelize]` (see retained-waits-justification). |
| HelperClasses/QfcTipsDetails_Tests.cs:679,722 | OUT OF CATALOG — `task.Wait(TimeSpan.FromSeconds(10))` is a `Task.Wait` join in a test file that is NOT among the 12 cataloged test files (research groups A-L). Pre-existing; outside this cycle's defined conversion scope. Not a cataloged occurrence; not converted. |

The cataloged `signal.Wait(<timeout>)` occurrences (A2, A3, C1, C3, K1) are all gone. L1
(`ThreadSafeSingleShotGuard_Tests`) uses `start.Wait()` with NO timeout (a synchronization gate) and is
intentionally retained (see retained-waits-justification); it is excluded from the `\.Wait\(\s*\d` /
`\.Wait\(\s*TimeSpan` pattern because it has no numeric/TimeSpan argument.

## SpinWait.SpinUntil(..., <digit>)

All 9 matches are the APPROVED deterministic replacements introduced this cycle (Risk R7):
- IEnumerableExtensions_Tests.cs:181 (F1), AsyncMultiTasker_Tests.cs:491 (E3),
  BayesianClassifierGroup_Tests.cs:202 (I1), BayesianClassifierGroupTests.cs:315 (I2),
  ObsoleteBayesianClassifier_Tests.cs:604 (I3), BayesianPerformanceMeasurement_Tests.cs:953 (I4),
  SegmentStopWatch_Tests.cs:23,46,49 (H1, H2).
- Each is `SpinWait.SpinUntil(() => <stopwatch>.Elapsed > TimeSpan.Zero, <bound>)` (or `> afterFirst`).
  These are structural non-zero-elapsed guarantees that return in microseconds; the numeric argument is a
  safety bound, NOT an expected wall-clock wait. The cataloged prohibited `SpinWait.SpinUntil(..., <timeout>)`
  occurrences (C2 5000 ms, G1 1000 ms) were removed/replaced with FireElapsed-driven assertions.

## Conclusion

The only residual matches are:
1. B1-B3 (TimerWrapper_Tests) — intentionally retained.
2. L1 (ThreadSafeSingleShotGuard_Tests) — intentionally retained (no-timeout gate; not matched by numeric patterns).
3. J1 residual `Thread.Sleep(20)` in OlTableExtensions_Tests — documented partial improvement (scope-change-J1).
4. Approved Risk-R7 `SpinUntil(... > TimeSpan.Zero, ...)` structural guarantees (not wall-clock waits).
5. Out-of-catalog `task.Wait(TimeSpan.FromSeconds(10))` in QfcTipsDetails_Tests (pre-existing, not a
   cataloged occurrence).

Every cataloged occurrence in groups A-K (excluding the intentionally-retained/halted set) is gone. No
unexplained residual match. No blocking finding.
