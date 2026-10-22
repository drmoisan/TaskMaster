# [P2-T6] AC3 Regression Tests — Pass-After

Timestamp: 2026-09-08T09-54
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p2-t6' '/TestCaseFilter:FullyQualifiedName~QfcHomeControllerCleanupTests'`, with `$vstest` re-bound per D3 from the two `vswhere` lines pinned by [P0-T7]
EXIT_CODE: 0
Output Summary: All three cases in `QfcHomeControllerCleanupTests` pass after the AC3 production change. The two cases that failed in [P2-T3] now pass, and the pre-existing third case is unaffected.

TOTAL: 3
PASSED: 3
FAILED: 0

## Outcome per case

```
Passed Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource
Passed Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted
Passed Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup
```

## What changed between the two runs

The test file was not edited between [P2-T3] and this run. The single production edit is [P2-T4], which added `_tokenSource = null;` immediately after `_tokenSource?.Dispose();` and `_datamodel = null;` alongside the five sibling nullings in `QuickFiler/Controllers/QfcHomeController.cs`. A repeat cleanup pass now finds both fields cleared, so the datamodel is cleaned exactly once across two passes and `TokenSource` returns null rather than a disposed source.

Together with [P2-T3] this is the fail-before / pass-after pair for AC3.

## D13

No TRX content is reproduced here. The counters were parsed from the run's `ResultSummary/Counters` element and the outcome list from its `UnitTestResult` elements.
