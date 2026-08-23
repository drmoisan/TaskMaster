# P2-T3 — ResolveControlGroupsAsync De-Exemption Tests

Issue: #230
Task: [P2-T3]

## Step 1 — Build

- Timestamp: 2026-08-07T22-15
- Command: `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -v:m`
- EXIT_CODE: 0
- Output Summary: Build succeeded, 0 errors, across all 20 projects.

## Step 2 — Filtered test run (D6 command form)

- Timestamp: 2026-08-07T22-15
- Command:
  ```powershell
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~ViewerSetupTests"
  ```
- EXIT_CODE: 0
- Output Summary: **Total tests: 10 — Passed: 10, Failed: 0.** Total time 3.1116
  seconds. The new pump-hosted test
  `ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups`
  passed in 104 ms; the nine pre-existing ViewerSetup tests are unaffected.

### Executed tests

```
Passed PopulateControls_WithHelper_StoresHelperAndAssignsViewerFields [320 ms]
Passed PopulateControls_WithMailItem_ConstructsHelperAndAssignsControls [186 ms]
Passed PopulateControlsAsync_WithMailItem_LoadsHelperViaFromMailItemAsyncAndAssignsControls [53 ms]
Passed AssignControls_WhenNotInvokeRequired_WritesAllIntentMembersFromSettings [2 ms]
Passed AssignControls_WhenTaskFlagUnset_SetsCancelDialogResult [< 1 ms]
Passed AssignControls_WhenInvokeRequired_MarshalsViaInvoke [3 ms]
Passed AssignControlsAsync_DispatchesAssignThroughViewerDispatcher [1 ms]
Passed Cleanup_NullsTrackedPrivateFields [3 ms]
Passed ResolveControlGroups_WithHeadlessItemViewer_PopulatesConcreteControlCollections [906 ms]
Passed ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups [104 ms]
```

## Change recorded by this phase

- **P2-T1** — added
  `QfcItemController_ViewerSetupTests.ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups`
  to `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs`. It
  constructs the real `QuickFiler.ItemViewer` via
  `host.InvokeAsync(() => new QuickFiler.ItemViewer())` so `UiSyncContext` binds to
  the pump thread, injects `_itemViewer` through
  `QfcItemControllerTestSupport.SetField`, sets `Token`, awaits
  `ResolveControlGroupsAsync(viewer)` from the MSTest thread, and asserts
  `_itemPositionTips` non-null, `_listTipsDetails` non-empty, `_listTipsExpanded`
  non-null, and both `TableLayoutPanels` and `Buttons` populated. `[Timeout(60000)]`
  harness bound; host released in `finally` via `StopAsync`. No MSTest-thread
  context mutation.
- **P2-T2** — removed the `[ExcludeFromCodeCoverage]` attribute from
  `ResolveControlGroupsAsync(ItemViewer)` in
  `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` in the same change, and
  replaced its residual-barrier comment with a `#230` note recording that the pump
  barrier is resolved and naming the covering test. No other attribute in the file
  was touched — `InitializeWebViewAsync` (line 38) and `EnsureBreadcrumbPipeline`
  (line 132) both retain theirs.

Exemption sites remaining in `QfcItemController.ViewerSetup.cs` after this phase: 2.
