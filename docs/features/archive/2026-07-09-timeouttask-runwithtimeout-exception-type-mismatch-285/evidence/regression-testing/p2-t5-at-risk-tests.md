# P2-T5 — The Two At-Risk Tests After the Fix

Timestamp: 2026-09-01T08-21

## Command

```text
<resolved-vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults\p2-t5 /TestCaseFilter:"FullyQualifiedName=UtilitiesCS.Test.TimeOutTask_Tests.RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException|FullyQualifiedName=UtilitiesCS.Test.TimeOutTask_Tests.RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries"
```

`<resolved-vstest>` is the vswhere-resolved path recorded in P0-T10.

EXIT_CODE: 0

## Output Summary

```text
  Passed RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException [55 ms]
  Passed RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries [1 ms]

Test Run Successful.
Total tests: 2
     Passed: 2
 Total time: 1.4641 Seconds
```

| Count | Value |
| --- | --- |
| Total tests | **2** |
| **Passed** | **2** |
| **Failed** | **0** |

## Individual Outcomes

| Test | Source | Outcome | Duration |
| --- | --- | --- | --- |
| `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException` | `UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs` line 190 | **Passed** | 55 ms |
| `RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries` | `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` line 106 | **Passed** | 1 ms |

Both discovered tests were found and executed: the `Total tests: 2` count confirms the filter matched
both fully qualified names, so neither result is a vacuous zero-discovery pass.

## Why This Gate Matters

These are the two tests the spec identifies as at risk of silent breakage if the fix were implemented
as a **replacement** of `catch (TimeoutException)` with `catch (TaskCanceledException)` rather than as
an **addition**. Both fake a timeout by throwing `TimeoutException` directly from the delegate and
both pass `strict: true`. Under a replacement, the directly-thrown `TimeoutException` would miss the
specific clause, fall through to the general handler at `catch (System.Exception e)`, and be rethrown
by the bare `throw;`, failing both tests.

Both pass, and neither test file was edited to make them pass. The additive filter
`catch (System.Exception e) when (e is TaskCanceledException || e is TimeoutException)` retains the
`TimeoutException` match, so both control paths remain byte-for-byte the ones they exercised before
the change. This is the direct evidence that the fix is an addition and not a replacement.

The companion proof that neither test body was modified is recorded by P2-T6.

Acceptance: met. `EXIT_CODE: 0`, and the run reports `Total tests: 2`, `Passed: 2`, `Failed: 0`.
