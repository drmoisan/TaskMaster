# P9-T26 unbuffered non-numeric adapter failure classification

Timestamp: 2026-07-27T09:23:58.8432204Z

Command:

```text
C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\Tags.Test\bin\Debug\Tags.Test.dll" "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll" "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskTree.Test\bin\Debug\TaskTree.Test.dll" "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll" "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll" "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook /Logger:Console;Verbosity=Detailed /ResultsDirectory:"C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\regression-testing" /Logger:trx;LogFileName=nonnumeric-adapter-coverage-failure-classification-unbuffered.2026-07-27T09-23.trx
```

EXIT_CODE: 1

Output Summary: 6,066 total and executed; 6,058 passed; 8 failed; no skipped, timed-out, aborted, not-runnable, or not-executed tests. The process completed before the eight-minute cleanup deadline.

## Ordered assemblies

1. `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
2. `Tags.Test/bin/Debug/Tags.Test.dll`
3. `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`
4. `TaskTree.Test/bin/Debug/TaskTree.Test.dll`
5. `TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll`
6. `ToDoModel.Test/bin/Debug/ToDoModel.Test.dll`
7. `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`
8. `VBFunctions.Test/bin/Debug/VBFunctions.Test.dll`

## Output and ownership receipts

- Canonical TRX: `nonnumeric-adapter-coverage-failure-classification-unbuffered.2026-07-27T09-23.trx`
- TRX SHA-256: `977B6C65E18F4FB4BABA9F4687AE894CC3CA7029649DC9A9741AB06E410B3D30`
- Direct stdout receipt: `nonnumeric-adapter-coverage-failure-classification-unbuffered.2026-07-27T09-23.stdout.txt` (`0E44F1BDDCE8308C6D90F4E59BD741E3482C50E6E6D4AB52861DD88C54803FC1`)
- Direct stderr receipt: `nonnumeric-adapter-coverage-failure-classification-unbuffered.2026-07-27T09-23.stderr.txt` (`DCF8C268BFDE477F9C77551D83B416CC0BE9010237FCF913F56278D347965D07`)
- Process-tree receipt: `nonnumeric-adapter-coverage-failure-classification-unbuffered.2026-07-27T09-23.process-tree.json` (`01F0E9AA159ACA56DBA2CC8B3EF1FD9A242BDF86E14130CD9FE2411CDC153574`)
- Runner PID: `253348`; direct VSTest PID: `271176`; observed testhost PID: `273968`; observed conhost PID: `265872`.
- Cleanup deadline: `2026-07-27T09:31:58.8432204Z` (eight minutes after launch).
- Timed out: `False`; terminated descendants: none; post-run worktree-related VSTest/testhost/dotnet processes: 0.

## Failed test classification

| Fully qualified test identity | Failure message and stack detail | Exact source boundary | Correction boundary |
| --- | --- | --- | --- |
| `QuickFiler.Test.Viewers.BreadcrumbPopupControlDispatchTests.SurfaceFactory_ReadinessFailure_ReportsOnceThenDisposesSurface` | `InvalidOperationException: Popup CoreWebView2 initialization completed without a core instance.` Stack: `BreadcrumbPopupUiOperations.ReadRequiredAsync` line 149 → `BreadcrumbWebViewSurfaceFactory.CreateSurfaceAsync` lines 204/221 → test fixture drain/complete lines 272/341 → test line 89. | The fixture's `readCore` delegate returns `null` at `BreadcrumbPopupControlDispatchTests.cs:308`; the production guard at `BreadcrumbPopupUiOperations.cs:149` correctly rejects it before readiness. | Test-only: make the fixture provide a valid non-null test core through the direct adapter seam so this test reaches the readiness-failure path. Do not weaken the production null guard. |
| `QuickFiler.Test.Viewers.BreadcrumbPopupControlDispatchTests.SurfaceFactory_NavigationActionFailure_ReportsOnceAndCleansUp` | Expected `navigation rejected`, but received the same core-instance `InvalidOperationException`. Stack: production lines 149, 204, 221; assertion at test line 77. | `BreadcrumbPopupControlDispatchTests.cs:308` returns `null` before the configured navigation delegate at lines 309-315 can execute. | Test-only: correct the shared fixture's non-null core setup; retain the navigation-error production path. |
| `QuickFiler.Test.Viewers.BreadcrumbPopupControlDispatchTests.SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup` | `InvalidOperationException: Popup CoreWebView2 initialization completed without a core instance.` Stack: production lines 149, 204, 221 → fixture drain/complete lines 272/341 → test line 32. | `BreadcrumbPopupControlDispatchTests.cs:308` returns `null`, preventing the worker-completion scenario from reaching navigation/readiness. | Test-only: correct the shared fixture's non-null core setup; retain production validation. |
| `QuickFiler.Test.Viewers.BreadcrumbPopupControlDispatchTests.SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp` | `fixture.Messenger.DisposeCount` expected `1`, actual `0`; assertion at test line 202. | The same fixture core result at line 308 prevents the `kind == 2` navigation result at lines 189-195 from executing, so the expected messenger cleanup cannot occur. | Test-only: correct shared non-null core setup, then re-evaluate the existing invalid-navigation cleanup assertion against the now-reached branch. |
| `QuickFiler.Test.Viewers.BreadcrumbSelectorOpenRetryTests.MouseToggle_FirstOpenFaultsAfterAwait_SecondClickRetriesCleanly` | `NullReferenceException`. Stack: `SelectorOpenHarness` constructor line 259 → test line 35. | The P9-T12 lifecycle refactor removed ItemViewer's `_breadcrumbPopupUiOperations`; the reflection call at lines 259-264 dereferences a null `FieldInfo`. | Test-only: replace stale private-field reflection with the lifecycle coordinator's supported operations seam; do not restore the removed field. |
| `QuickFiler.Test.Viewers.BreadcrumbSelectorOpenRetryTests.SetFolderDroppedDownTrue_UsesSameOpenRequestAsMouseSelectorToggle` | `NullReferenceException`. Stack: `SelectorOpenHarness` constructor line 259 → test line 59. | Same stale `_breadcrumbPopupUiOperations` reflection boundary at lines 259-264. | Same test-only lifecycle-coordinator seam correction. |
| `QuickFiler.Test.Viewers.BreadcrumbCollapsedSurfaceReadinessTests.ViewerAttachment_PendingCachesAndReplaysCurrentStateExactlyOnce` | `NullReferenceException`. Stack: `ViewerIntegrationHarness` constructor line 425 after `GetField("_breadcrumbHub")` at lines 416-420 → test line 188. | The P9-T12 lifecycle refactor moved the hub into `BreadcrumbItemViewerLifecycleCoordinator`; `_breadcrumbHub` no longer exists on `ItemViewer`. | Test-only: obtain the hub through the lifecycle coordinator seam; do not restore the removed field. |
| `QuickFiler.Test.Viewers.BreadcrumbCollapsedSurfaceReadinessTests.ViewerAttachment_FailureResetReuseAndDisposalLeaveNoStaleAttachment` | `NullReferenceException`. Stack: `ViewerIntegrationHarness` constructor line 425 after stale `_breadcrumbHub` reflection → test line 234. | Same stale hub-reflection boundary at lines 416-420. | Same test-only lifecycle-coordinator hub seam correction. |

The one-shot direct run used the same eight assemblies, runsettings, isolation, and filter as P9-T19. Its exact eight failures are therefore classified into three bounded test-fixture corrections: one non-null core adapter fixture, one operations-seam reflection update, and one hub-seam reflection update. No production, coverage, filter, exclusion, or configuration correction is established by this evidence.

## Plan outcome

P9-T26 remains unchecked because the classified correction requires an in-place plan revision before implementation. P9-T19 through P9-T23 remain blocked, and no coverage retry was performed.
