# Finding 4 — reentrancy counter Volatile.Read proxy, passing run after the fix

Timestamp: 2026-09-03T14-33

Task: [P4-T6]
Issue: #731

## Command

1. Rebuild:

```
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

MSBuild executable actually invoked: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`, recorded in full under the Evidence path-hygiene rule's stated exception for an external build-tool executable.

2. Filtered test run:

```
<vstest> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcCollectionControllerDefects468Tests"
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
  Passed ParentFieldAndConstructorParameterAreTypedIQfcFormController [58 ms]
  Passed RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter [9 ms]
  Passed RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter [96 ms]
  Passed ShrinkByRows_WithPositiveRemovalCount_ReducesHeight [< 1 ms]
  Passed ShrinkByRows_WithNegativeRemovalCount_IncreasesHeight [< 1 ms]
  Passed DrainBackgroundLoadingTasksAsync_AwaitsATaskAddedDuringTheDrainWindow [4 ms]
  Passed TryGetMoveReadiness_WithUnassignedDestination_ReturnsFalseAndProducesNotificationText [47 ms]
  Passed TryGetMoveReadiness_WithAllDestinationsAssigned_ReturnsTrueAndEmptyNotification [< 1 ms]
  Passed ReentrancyCounterSoleReadGoesThroughVolatileRead [1 ms]

Test Run Successful.
Total tests: 9
     Passed: 9
 Total time: 1.3741 Seconds
```

- Total tests: **9**
- Passed: **9**
- Failed: **0**
- Skipped: **0**

All three tests this task names are among the passed tests:

- `ReentrancyCounterSoleReadGoesThroughVolatileRead` — failed in `EVIDENCE/regression-testing/finding4-volatile-fail-before.md` and now passes. Its four assertions together establish that the sole read of the counter goes through `Volatile.Read(ref removespecificcontrolgroupcounter)`, that the bare read `if (removespecificcontrolgroupcounter >` is gone, that both `Interlocked` writes survive unchanged, and that the field was not marked `volatile`.
- `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter` — the issue-#286 restoration test, passing unchanged.
- `RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter` — the second issue-#286 restoration test, passing unchanged.

Neither issue-#286 test was modified by this plan. Their continued passing is the evidence that the read-side change did not disturb the counter's increment/decrement restoration behaviour, which is what those two tests pin.

The edit at [P4-T5] is line-neutral: `QuickFiler/Controllers/QfcCollectionController.cs` is 2328 lines both before and after it, being the [P0-T3] baseline of 2327 plus the single [P1-T1] comment line.
