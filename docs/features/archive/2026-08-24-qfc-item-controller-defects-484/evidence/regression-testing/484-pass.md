# Issue #484 — Pass-after regression run (Cleanup timer disposal and stale collaborators)

Timestamp: 2026-08-26T10-07
Task: [P4-T8]

## Step 1 — Build the test project (not a gate; decision D2)

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved MSBuild):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
```

EXIT_CODE: 0. Not an analyzer or nullable gate (decision D2). `Platform=AnyCPU` is the project-level
spelling of the solution-level `Any CPU` alias, matching the `[P4-T4]` fail-before run.

## Step 2 — Run every `Cleanup` / `ApplyReadEmailFormat` test after the #484 fixes

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved `vstest.console.exe`):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~Cleanup|FullyQualifiedName~ApplyReadEmailFormat" "/Logger:trx;LogFileName=484-pass.trx" /ResultsDirectory:docs\features\active\qfc-item-controller-defects-484\evidence\regression-testing\484-pass
```

EXIT_CODE: 0

## Results

```
Test Run Successful.
Total tests: 21
 Total time: 1.9096 Seconds
```

Failed count: **0**. Skipped count: **0**.

### The three new #484 regression tests

| Test | Outcome |
|---|---|
| `Cleanup_DisposesEmailIsReadTimerBeforeNullingIt` | **Passed** |
| `ApplyReadEmailFormat_AfterCleanup_IsInertAndDoesNotSave` | **Passed** |
| `Cleanup_NullsMailActions_AndSaveParametersRebindsIt` | **Passed** |

### The two pre-existing `Cleanup()` tests named by `[P4-T8]`

| Test | Outcome |
|---|---|
| `Cleanup_NullsTrackedPrivateFields` | **Passed** |
| `Cleanup_ResetsInjectedHostForPooledViewerReuse` | **Passed** |

### Full result list

All 21 selected results were `Passed`: `CollapsedAttachment_ReplayFailureAndDisposeDetachBeforeMessengerCleanup`,
`TwoResourceCleanup_ReportsFirstFailureAfterAllCleanup`, `Cleanup_DisposesEmailIsReadTimerBeforeNullingIt`,
`HostedCleanup_HostDisposeFailure_PreservesPrimaryAndDisposesAllOnce`,
`ApplyReadEmailFormat_AfterCleanup_IsInertAndDoesNotSave`,
`SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup`,
`CreateAndInstall_CancellationCleanupFailure_RetriesOnlyFailedResource`,
`CreateAndInstall_StaleHostCleanup_DoesNotDisposeOwnedControlDirectly`,
`InjectedFactory_CreateFailure_ReportsOnceWithoutCleanup`,
`InjectedFactory_CleanupFailure_DoesNotReplacePrimaryFailure`, `Cleanup_NullsTrackedPrivateFields`,
`Cleanup_ClearsControllerFieldsAndInvokesParentCleanup`,
`OpenAsync_CreationFailsAndCleanupSucceeds_DisposesOwnedSurfaceWithoutReport`,
`OpenAsync_CleanupDispatchFails_ReportsSecondaryOnceAndPreservesPrimary`,
`ApplyReadEmailFormat_MarksMailReadFalseAndRoutesThemeThroughInjectedDispatcherBeginInvoke`,
`Cleanup_NullsMailActions_AndSaveParametersRebindsIt`, `Cleanup_ShouldCleanupResources`,
`Cleanup_ExecutesCorrectly`, `Cleanup_ThenDarkModePropertyChanged_DoesNotThrow`,
`CleanupAsync_ThenDarkModePropertyChanged_DoesNotThrow`, `Cleanup_ResetsInjectedHostForPooledViewerReuse`.

TRX artifact: `docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/484-pass/484-pass.trx`.

Output Summary: EXIT_CODE 0, 21 of 21 Passed, 0 Failed. The three new #484 tests that failed at
`[P4-T4]` now pass, and the pre-existing `Cleanup_NullsTrackedPrivateFields` and
`Cleanup_ResetsInjectedHostForPooledViewerReuse` remain green. The pre-existing
`ApplyReadEmailFormat_MarksMailReadFalseAndRoutesThemeThroughInjectedDispatcherBeginInvoke` also remains
green, establishing that the `[P4-T7]` guard does not suppress the fully-initialized path.
