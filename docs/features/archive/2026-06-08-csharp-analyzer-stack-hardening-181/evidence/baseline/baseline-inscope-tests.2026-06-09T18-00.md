# Baseline In-Scope Tests (Cycle 7)

Timestamp: 2026-06-09T18-00
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry,StartTimer_RaisesElapsedEvent,StopTimer_PreventsPendingElapsedEvent,StartNew_ConfiguresAutoResetAndInvokesCallback /InIsolation
EXIT_CODE: 0

Resolved vstest.console: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe

## Output Summary

```
  Passed StartTimer_RaisesElapsedEvent [81 ms]
  Passed StopTimer_PreventsPendingElapsedEvent [261 ms]
  Passed StartNew_ConfiguresAutoResetAndInvokesCallback [30 ms]
  Passed GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry [280 ms]

Total tests: 4
     Passed: 4
```

All four named tests currently PASS while the prohibited waits are still in place:
- J1 (`GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry`)
  relies on `Thread.Sleep(20)` (its 280 ms duration reflects the synchronous block plus
  Task.Run/test overhead).
- B1 (`StartTimer_RaisesElapsedEvent`) relies on `signal.Wait(500)`.
- B2 (`StopTimer_PreventsPendingElapsedEvent`) relies on `signal.Wait(250)` (its 261 ms
  duration is the full wait-timeout elapsing, confirming the wall-clock dependency).
- B3 (`StartNew_ConfiguresAutoResetAndInvokesCallback`) relies on `signal.Wait(500)`.

This is the conversion baseline: these four tests are green BUT depend on the prohibited
timing primitives. Cycle 7 converts them to deterministic seams.
