# [P2-T3] AC3 Regression Tests — Fail-Before

Timestamp: 2026-09-08T09-52
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p2-t3' '/TestCaseFilter:FullyQualifiedName~QfcHomeControllerCleanupTests'`, with `$vstest` re-bound per D3 from the two `vswhere` lines pinned by [P0-T7]
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Three cases in `QfcHomeControllerCleanupTests` ran, one passed and two failed. Both failures are the AC3 coverage added by [P2-T1], and both fail against the unmodified production code for the reasons AC3 describes. This is the required fail-before observation for this `[expect-fail]` task.

TOTAL: 3
PASSED: 1
FAILED: 2

## Outcome per case

```
Failed Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted
Passed Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup
Failed Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource
```

FAILURE-REASON: both AC3 cases fail because `Cleanup()` releases each resource without clearing the field that holds it.

- `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` — the datamodel is not nulled, so its `Cleanup()` runs twice. The Moq verification observed two invocations of `IQfcDatamodel.Cleanup()` across the two cleanup passes where exactly one is required. `QuickFiler/Controllers/QfcHomeController.cs:388` calls `_datamodel?.Cleanup()` and the five sibling nullings at `:390-394` clear `Globals`, `_formViewer`, `_explorerController`, `_formController` and `_keyboardHandler` but not `_datamodel`, so the second pass finds the field still populated and cleans it again.
- `Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource` — the token source is disposed but not nulled. `QuickFiler/Controllers/QfcHomeController.cs:389` calls `_tokenSource?.Dispose()` and does not assign null, so `TokenSource`, the plain getter at `:470-473` over the field declared at `:469`, still returns the disposed source after cleanup. The assertion observed a non-null `CancellationTokenSource` whose `Token` property throws because the source is disposed, which is precisely the reachable-disposed-source condition AC3 names.

## Why the injection in the second case is load-bearing

The two-argument constructor at `QuickFiler/Controllers/QfcHomeController.cs:29-33` assigns only `Globals` and `ParentCleanup`, so a controller built without the reflection injection reports a null `TokenSource` before `Cleanup()` as well as after, and the test would pass against unmodified production code. The pre-act non-null assertion is what makes the post-act null assertion discriminating, and its presence is evidenced by this run failing rather than passing.

## Expectation

This task is tagged `[expect-fail]`. A failing run is its required outcome and `ExpectedExitCode: 1` declares that expectation, so the observed `EXIT_CODE: 1` normalizes to a pass. The pass-after counterpart is [P2-T6].

## D13

No TRX content is reproduced here. The counters were parsed from the run's `ResultSummary/Counters` element, the outcome list from its `UnitTestResult` elements, and the two failure causes are stated in prose.
