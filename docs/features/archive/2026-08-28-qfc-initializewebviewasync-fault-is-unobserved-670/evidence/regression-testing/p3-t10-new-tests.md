# P3-T10 — All four new tests run together

Timestamp: 2026-09-01T20-07
Command: rebuild with the P0-T10 analyzer command, then

    & $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/TestCaseFilter:FullyQualifiedName~InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault|FullyQualifiedName~WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing|FullyQualifiedName~InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink|FullyQualifiedName~InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink' /Logger:trx '/ResultsDirectory:coverage\testresults\p3-t10'

The resolved test runner is recorded as `<vs-install>\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`. The `|` separator is the vstest `TestCaseFilter` disjunction operator; the word `OR` is not accepted in this position.

EXIT_CODE: 0 (build), 0 (vstest)

## Build

    Build succeeded.
        5 Warning(s)
        0 Error(s)

    Time Elapsed 00:00:11.56

Warning count unchanged at 5 (the pre-existing System.Reactive diagnostic). Zero coded diagnostics: `: error [A-Z]+[0-9]+:` returns 0 and `: warning [A-Z]+[0-9]+:` returns 0.

## Output Summary

      Passed InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink [212 ms]
      Passed InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault [10 ms]
      Passed WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing [2 ms]
      Passed InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink [1 s]

    Test Run Successful.
    Total tests: 4
         Passed: 4

## Four tests ran, not a filter matching none

The `.trx` was copied to `evidence/regression-testing/p3-t10-new-tests.trx`. Its result summary reads `outcome=Completed`, `total=4`, `passed=4`, `failed=0`, and a fixed-string search finds each of the four names in the document:

| Test | Hits in the .trx |
| --- | --- |
| `InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault` | 3 |
| `WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing` | 3 |
| `InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink` | 3 |
| `InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink` | 3 |

A control search of the same document for a name that is not in the run returned **0** hits, so the search discriminates: a non-zero hit count is evidence the named test is genuinely recorded, not an artifact of the search always matching.

`total=4` independently excludes the failure mode this check exists for — a disjunctive filter in which one clause is misspelled would silently select fewer tests and still exit 0.

## What the four tests cover

- **Fault arm, directly awaited.** The seam faults, the guard's returned task does not fault, and the sink receives the `WebViewSentinelException`. This is the assertion the P3-T4/P3-T5 mutation pair proved discriminating.
- **Default sink lambda.** Exercises the production log4net-backed delegate body rather than a test double, so the default is covered rather than always replaced.
- **Site-192 dispatcher path, pump-hosted.** Drives `Initialize(async: false)` through the `WinFormsPumpHost`, so the fault reaches the sink through the real WPF dispatcher route the fix substitutes at line 192.
- **Cancellation arm.** A pre-cancelled token reaches `Token.ThrowIfCancellationRequested()` before any seam call, so the `catch (OperationCanceledException)` arm is entered deterministically and the sink is confirmed **not** invoked. This test is load-bearing for AC13: without it the cancellation arm would be the only uncovered region of the new file and the `>= 90%` new-module threshold would not be reachable.

Together these four cover both catch arms of the guard, the guard's non-faulting contract, and the sink's default value.

## Determinism

No test here uses `Thread.Sleep`, `Task.Delay`, polling, or a wall-clock wait. Three of the four are effectively synchronous. The only wait in the pump-hosted test is `await observed.Task` on a `TaskCompletionSource` completed from the sink callback; its `[Timeout(PumpTimeoutMs)]` attribute converts a genuine deadlock into a test failure rather than serving as a wait mechanism. This is audited independently in P4-T12.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
