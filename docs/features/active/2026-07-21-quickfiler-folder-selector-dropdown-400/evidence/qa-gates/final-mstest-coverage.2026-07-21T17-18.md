# Final MSTest Coverage

Timestamp: 2026-07-21T17:18:44Z

Command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput 'docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\qa-gates\coverage-final.2026-07-21T17-17.cobertura.xml'
```

EXIT_CODE: 0

Total: 5803

Passed: 5803

Failed: 0

Skipped: 0

Elapsed test time: 57.8898 seconds

Wrapper wall time: 76.4 seconds

Filter: `/TestCaseFilter:TestCategory!=LiveOutlook`

Cobertura: `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final.2026-07-21T17-17.cobertura.xml`

Discovered first-party test assemblies: 8

- `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
- `Tags.Test/bin/Debug/Tags.Test.dll`
- `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`
- `TaskTree.Test/bin/Debug/TaskTree.Test.dll`
- `TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll`
- `ToDoModel.Test/bin/Debug/ToDoModel.Test.dll`
- `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`
- `VBFunctions.Test/bin/Debug/VBFunctions.Test.dll`

The run output named all five issue #398 tests: `ReplaceRows_PreservesSelectionWhenIndexStillValid`, `ReplaceRows_ClearsSelectionWhenIndexBeyondNewCount`, `SetSuggestionsAsync_WhileUpgradeInFlight_RowCountNeverDropsBelowPreUpgradeCount`, `SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives`, and `SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection`.

The run also discovered every P7-T1 issue #400 class filter: `BreadcrumbSelectionSessionTests`, `BreadcrumbSelectorMessagesTests`, `BreadcrumbStateModelSelectorTests`, `BreadcrumbRenderProjectionSelectorTests`, `FolderBreadcrumbBridgeRouter*`, `BreadcrumbBridgeCoordinatorProbabilityTests`, `BreadcrumbSelectorCoordinatorTests`, `BreadcrumbMessengerHubTests`, `BreadcrumbPopupPlacementTests`, `BreadcrumbDropDownHostTests`, `BreadcrumbDropDownLifecycleTests`, `BreadcrumbDropDownIntegrationTests`, `ItemViewerBreadcrumbDropDownContractTests`, `QfcItemControllerBreadcrumbDropDownTests`, and `FolderBreadcrumbAssetContractTests`.

Output Summary: The complete coverage-enabled first-party test run passed without failures or skips and produced the direct Cobertura artifact.
