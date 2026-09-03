# Finding 3 — admission scoring-delegate pin, passing run after the fix

Timestamp: 2026-09-03T14-26

Task: [P3-T6]
Issue: #731

## Command

1. Rebuild:

```
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

MSBuild executable actually invoked: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`, recorded in full under the Evidence path-hygiene rule's stated exception for an external build-tool executable.

2. Filtered test run:

```
<vstest> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcDatamodelTests"
```

vstest console: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` (VSTest version 18.9.0, x64).

EXIT_CODE: 0

The build exited 0 and the test run exited 0.

## Output Summary

Build summary lines, as observed:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Test run output, as observed:

```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
  Passed TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsAndHooksWithoutScoring [144 ms]
  Passed QfcRemainingQueueAdmission_DeclaresNoScoringDelegate [37 ms]
  Passed DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive [57 ms]
  Passed TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsBelowThresholdCandidate [< 1 ms]
  Passed TryQueueRemainingMailItemAsync_HighConfidenceDisabled_AddsAndHooksWithoutScoring [< 1 ms]
  Passed TryQueueRemainingMailItemAsync_NullMailItem_DoesNotScoreAddOrHook [< 1 ms]
  Passed ToggleOfflineMode_WhenOnline_AwaitsInjectedFiveMillisecondDelay [30 ms]
  Passed WaitForQueue_WhenWorkerBusyAndQueueShort_AwaitsInjectedTwoHundredMsDelay [1 ms]
  Passed ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder [25 ms]

Test Run Successful.
Total tests: 9
     Passed: 9
 Total time: 1.6985 Seconds
```

- Total tests: **9**
- Passed: **9**
- Failed: **0**
- Skipped: **0**

`QfcRemainingQueueAdmission_DeclaresNoScoringDelegate` is among the passed tests. It failed in `EVIDENCE/regression-testing/finding3-admission-pin-fail-before.md` because the constructor still declared `Func<MailItem, CancellationToken, Task<long>> scoreLoader`; [P3-T3] removed that parameter together with `IApplicationGlobals globals`, the `scoreLoader` null-guard, and the then-unconsumed `using UtilitiesCS;`, and the pin now passes.

Every retained test method name survived the factory reduction, so the pinned intent set was not reduced: the four `TryQueueRemainingMailItemAsync_*` methods listed above are the same four that existed before [P3-T5], and `ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder` continues to pass, confirming that `ScoreRemainingQueueMailItemAsync` itself was left in place. That method is declared in the untouched partial `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:274` and remains independently used at `:186`; [P3-T4] removed only the admission-scoring lambda at the construction site in `QuickFiler/Controllers/QfcDatamodel.cs`.

The three positive-path admission tests now converge on identical bodies, which is the expected and correct consequence of admission being provably independent of the settings and scoring collaborators. Their distinct names continue to document distinct intents.
