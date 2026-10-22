# P2-T4 — Green Run of the New Regression Test After the Fix

Timestamp: 2026-09-01T08-20

## Command

```text
<resolved-vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults\p2-t4 /TestCaseFilter:FullyQualifiedName=UtilitiesCS.Test.TimeOutTask_Tests.RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException
```

This is the same vstest command as P1-T6 with `/ResultsDirectory:TestResults\p2-t4` substituted.
`<resolved-vstest>` is the vswhere-resolved path recorded in P0-T10.

EXIT_CODE: 0

## Output Summary

```text
  Passed RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException [55 ms]

Test Run Successful.
Total tests: 1
     Passed: 1
 Total time: 1.4901 Seconds
```

| Count | Value |
| --- | --- |
| Total tests | **1** |
| **Passed** | **1** |
| **Failed** | **0** |

vstest omits the `Failed:` summary line when that count is zero; `Total tests: 1` with `Passed: 1`
and the `Test Run Successful.` header fix the failed count at 0. A scan of the captured log for
failure, error-message, or stack-trace markers returned no lines.

## Red-to-Green Transition

The identical test, run by P1-T6 against the identical assembly path with only the one-line handler
change intervening, reported `Failed: 1` with an escaping
`System.Threading.Tasks.TaskCanceledException`. It now reports `Passed: 1` in 55 ms. The only
production change between the two runs is P2-T1's replacement of `catch (TimeoutException)` with
`catch (System.Exception e) when (e is TaskCanceledException || e is TimeoutException)`. That
isolates the fix to the widened handler and confirms the defect diagnosis.

The 55 ms duration confirms the test is deterministic rather than timer-driven: `milliseconds: 30_000`
is never armed, because the injected pre-cancelled source fixes the outcome before any scheduling
decision is made. No wall-clock wait occurred.

## The Three Assertions the Test Carries

All three passed, since MSTest reports the method as `Passed` only when every assertion holds:

1. `result.Should().Be("result-42")` — the retry attempt completed and returned the delegate's value,
   proving the retry ladder is now reachable for a timer-driven cancellation.
2. `delegateCalls.Should().Be(1)` — the delegate ran exactly once. Attempt 0 never dequeued it,
   because the combined token was already cancelled when `Task.Run` was queued; only attempt 1 ran it.
3. `factoryCalls.Should().Be(2)` — the timeout-source factory was invoked exactly twice, one source
   per attempt, proving the seam is threaded through the recursive retry call rather than being
   consumed only on the first attempt.

These are the three assertions AC2 names.

Acceptance: met. `EXIT_CODE: 0`, and the run reports `Total tests: 1`, `Passed: 1`, `Failed: 0`.
