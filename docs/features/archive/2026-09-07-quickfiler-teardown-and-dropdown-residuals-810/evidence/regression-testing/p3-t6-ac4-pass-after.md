# [P3-T6] AC4 Regression Test — Pass-After, Whole Class

Timestamp: 2026-09-08T09-58
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p3-t6' '/TestCaseFilter:FullyQualifiedName~QfcFormControllerCleanupTests'`, with `$vstest` re-bound per D3 from the two `vswhere` lines pinned by [P0-T7]
EXIT_CODE: 0
Output Summary: All eight cases in `QfcFormControllerCleanupTests` pass after the AC4 restructure. The AC4 case that failed in [P3-T3] now passes, and the seven pre-existing issue-731 cases are unaffected by the try/finally.

TOTAL: 8
PASSED: 8
FAILED: 0

## Outcome per case

```
Passed Cleanup_CalledTwice_DoesNotThrow
Passed Cleanup_WithParkedConsumer_ReturnsWithoutWaiting
Passed Cleanup_SourceContainsNoSynchronousWait
Passed Cleanup_WithRunningConsumer_CompletesAddingBeforeDisposing
Passed Cleanup_WithRunningConsumer_ConsumerReachesRanToCompletion
Passed Cleanup_WithFaultedConsumer_ObservesAndLogsTheFault
Passed Cleanup_WithNullConsumerTask_DisposesQueueAndDoesNotThrow
Passed Cleanup_ViewerDisposeThrows_StillInvokesParentCleanupOnce
```

`Cleanup_SourceContainsNoSynchronousWait` is among the passing cases. That case scans the whole text of `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` for `.Wait(`, `.Result`, `Thread.Sleep` and `Task.Delay`, so its passing establishes that the [P3-T4] restructure introduced none of the four.

## What changed between the two runs

The test file was not edited between [P3-T3] and this run. The single production edit is [P3-T4], which wrapped the body of `Cleanup()` in a `try` and replaced the trailing invoke-then-clear pair with a `finally` that reads `_parentCleanup` into a local, clears the field, then invokes the local. The callback therefore runs whichever stage threw, and runs at most once because the field is cleared before the invoke rather than after it.

Together with [P3-T3] this is the fail-before / pass-after pair for AC4.

## D13

No TRX content is reproduced here. The counters were parsed from the run's `ResultSummary/Counters` element and the outcome list from its `UnitTestResult` elements.
