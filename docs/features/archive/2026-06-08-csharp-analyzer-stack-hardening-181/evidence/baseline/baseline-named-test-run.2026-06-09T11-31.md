# Baseline — Named Test Run (Pre-Fix, Timing-Dependent) [expect-fail]

Timestamp: 2026-06-09T11-31
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite /InIsolation
EXIT_CODE: 0

Output Summary:
- Total tests: 2; Passed: 2; Failed: 0.
- The `/Tests:` name filter matched two same-named methods:
  - `SmartSerializableBase_Tests.Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite` (rows A1/A2)
  - `SmartSerializable_Tests.Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite` (row A3)
- Both currently PASS, but the pass is timing-dependent: the test relies on `Thread.Sleep(50)`
  (A1, line 571) to let the private `_timer` field be assigned before reflection, and on
  `signal.Wait(5000)` (A2, line 575) / `signal.Wait(1000)` (A3, line 602) to observe the
  deferred-write callback within a wall-clock window. This is the flaky/sleep-based baseline.

[expect-fail] rationale: the task is tagged `[expect-fail]` because the test's correctness is
established only via prohibited wall-clock waits; under load the `Thread.Sleep(50)` window can
be insufficient for `_timer` assignment, making the pass non-deterministic. The deterministic
conversion (Phase 1, seam S1 + ManualFireTimerWrapper) removes both wall-clock primitives and
makes the pass deterministic. The baseline result is recorded as the pre-fix, timing-dependent
state per the plan's expect-fail evidence requirement.
