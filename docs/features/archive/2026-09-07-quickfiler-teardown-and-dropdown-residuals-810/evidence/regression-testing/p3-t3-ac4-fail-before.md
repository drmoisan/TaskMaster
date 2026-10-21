# [P3-T3] AC4 Regression Test — Fail-Before

Timestamp: 2026-09-08T09-56
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p3-t3' '/TestCaseFilter:FullyQualifiedName~Cleanup_ViewerDisposeThrows_StillInvokesParentCleanupOnce'`, with `$vstest` re-bound per D3 from the two `vswhere` lines pinned by [P0-T7]
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: The AC4 regression test added by [P3-T1] fails against the unmodified production code, which is the required fail-before observation for this `[expect-fail]` task. One test was selected, zero passed, one failed. The counting delegate ran zero times where exactly one is required.

TOTAL: 1
PASSED: 0
FAILED: 1

FAILURE-REASON: the counting `System.Action` passed as the ribbon-release callback ran zero times across both cleanup passes. The observed count was 0 against a required 1. `_formViewer?.Dispose()` at `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:251` throws the planted `InvalidOperationException`, and because the body of `Cleanup()` carries no `try`, the throw propagates straight out of the method and skips every remaining statement, including `_parentCleanup?.Invoke()` at `:259`. This is the defect AC4 describes: a teardown stage that throws leaves the ribbon release callback uninvoked, so both ribbon buttons stay inert for the rest of the Outlook session.

The two `Throw<InvalidOperationException>` assertions in the test passed, which establishes that the planted fault does propagate on both passes and that the single failing assertion is the invocation count rather than the exception behaviour.

## Expectation

This task is tagged `[expect-fail]`. A failing run is its required outcome and `ExpectedExitCode: 1` declares that expectation, so the observed `EXIT_CODE: 1` normalizes to a pass. The pass-after counterpart is [P3-T6].

## D13

No TRX content is reproduced here. The counters were parsed from the run's `ResultSummary/Counters` element and the failure reason is stated in prose.
