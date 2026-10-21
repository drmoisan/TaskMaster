# Regression Pass-After (issue #781)

Timestamp: 2026-09-05T16-53

Task: [P1-T9]

## Invocation 1 — rebuild the test project

Command: `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU`

EXIT_CODE: 0

Result: `Build succeeded.` with 0 Warning(s) and 0 Error(s). The `AnyCPU` project-file platform
value is used for the reason stated in [P1-T4].

## Invocation 2 — re-run both affected test classes and their two siblings

Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx "/ResultsDirectory:TestResults\pass-after-781" "/TestCaseFilter:FullyQualifiedName~ItemViewerBreadcrumbThreadAffinityTests|FullyQualifiedName~ItemViewerBreadcrumbLifecycleRegressionTests|FullyQualifiedName~QfcItemControllerBreadcrumbDropDownTests|FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests" "/Settings:scripts\vscode\TaskMaster.cli.runsettings"`

EXIT_CODE: 0

Both invocations were issued from the repository root inside a `pwsh -NoProfile -Command`
process, using the `vstest.console.exe` resolved by [P0-T4].

## Output Summary

Total tests: 33. Passed: **33**. Failed: **0**. Skipped: **0**.
Run result: `Test Run Successful.`

Every executed test name with its outcome:

### `ItemViewerBreadcrumbThreadAffinityTests` (7 of 7 Passed)

| Test | Outcome | Duration |
| --- | --- | --- |
| `InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext` | Passed | 960 ms |
| `InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow` | Passed | 148 ms |
| `InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow` | Passed | 95 ms |
| `ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow` | Passed | 98 ms |
| `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` | Passed | 92 ms |
| `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` | Passed | 65 ms |
| `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` | Passed | 41 ms |

The first four of these were recorded **Failed** against the unfixed guard in
`FEATURE/evidence/regression-testing/regression-fail-before.2026-09-05T10-49.md`, as was
`InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow`. Five discriminating tests
therefore moved from Failed to Passed, and the two corroborating cross-thread tests stayed
Passed, so the fail-fast contract for a genuine cross-thread call is preserved rather than
weakened.

### `ItemViewerBreadcrumbLifecycleRegressionTests` (6 of 6 Passed)

| Test | Outcome | Duration |
| --- | --- | --- |
| `ConfigureBreadcrumbDropDown_EnvironmentChange_DisposesOutgoingHostBeforeReplacement` | Passed | 960 ms |
| `InitializeBreadcrumbPipeline_SecondDifferentProvider_ThrowsInvalidOperationException` | Passed | 130 ms |
| `InitializeBreadcrumbPipeline_RepeatSameProvider_DoesNotThrowAndKeepsCoordinator` | Passed | 140 ms |
| `InitializeBreadcrumbPipeline_AfterViewerDisposed_ThrowsObjectDisposedException` | Passed | 99 ms |
| `LegacySurfaceFactoryConstructor_AmbientContextNull_ThrowsInvalidOperationException` | Passed | < 1 ms |
| `ConfigureBreadcrumbDropDown_SeededLifecycleNullUiContext_DoesNotThrow` | Passed | 99 ms |

`ConfigureBreadcrumbDropDown_SeededLifecycleNullUiContext_DoesNotThrow` is the sibling issue #475
test that the guard change puts back under an **active** affinity check for the first time: it
nulls `_context`, not `_uiDispatcher`, so after this change the viewer still has an owning
dispatcher and the call is evaluated by `CheckAccess()` rather than short-circuiting through the
null-owner escape. It passes because the call is made on the owning thread.

### Sibling classes in the same filter (20 of 20 Passed)

`QfcItemControllerBreadcrumbDropDownTests` and `ItemViewerBreadcrumbDropDownContractTests`
contributed 20 further tests, all Passed, including
`ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily`,
`ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost`,
`Cleanup_ResetsInjectedHostForPooledViewerReuse`,
`IItemViewer_StillDeclaresUiDispatcher`, and `IItemViewer_StillDeclaresUiSyncContext`. The last
two confirm the public surface is unchanged: `UiSyncContext` was retained and not removed.

All five [P1-T9] acceptance conditions hold: both invocations exit 0; the failed count is 0; all
seven `ItemViewerBreadcrumbThreadAffinityTests` methods are Passed; the three named
`ItemViewerBreadcrumbLifecycleRegressionTests` D3 and D5 tests are Passed; and
`ConfigureBreadcrumbDropDown_SeededLifecycleNullUiContext_DoesNotThrow` is Passed.

The `.trx` produced under `TestResults\pass-after-781\` was not copied into this evidence folder.
