# Structural Gates

Timestamp: 2026-07-21T17:08:00Z

Baseline evidence: `evidence/baseline/git-baseline.2026-07-21T15-26.md`

BaselineCommitSHA: `df5ad49c909f6b739edef45d0336151f44e827a6`

## Changed C# Line Limits

Command:

```powershell
$changedFiles = @((git diff --name-only --diff-filter=ACMR $baselineSha -- '*.cs'); (git ls-files --others --exclude-standard -- '*.cs')) | Sort-Object -Unique; $lineCounts = @($changedFiles | ForEach-Object { [pscustomobject]@{ Path = $_; Lines = (Get-Content -LiteralPath $_).Count } }); $overLimit = @($lineCounts | Where-Object Lines -gt 500); if ($overLimit.Count) { throw "Files over 500 lines: $($overLimit.Path -join ',')" }
```

EXIT_CODE: 0

CHANGED_CS_COUNT: 29

MAX_LINES: 456

OVER_LIMIT_COUNT: 0

Line counts, descending: `FolderBreadcrumbBridgeRouter.cs=456`, `BreadcrumbStateModel.cs=449`, `BreadcrumbBridgeCoordinator.cs=448`, `QfcItemController.ViewerSetup.cs=409`, `ItemViewer.Breadcrumb.cs=392`, `BreadcrumbDropDownHost.cs=392`, `BreadcrumbDropDownIntegrationTests.cs=357`, `FolderBreadcrumbBridgeRouterEdgeTests.cs=324`, `FolderBreadcrumbBridgeRouterInFlightTests.cs=291`, `BreadcrumbDropDownLifecycleTests.cs=277`, `BreadcrumbMessengerHub.cs=254`, `BreadcrumbRenderProjection.cs=248`, `BreadcrumbSelectorCoordinatorTests.cs=241`, `BreadcrumbSelectorMessages.cs=237`, `BreadcrumbDropDownHostTests.cs=235`, `FolderBreadcrumbAssetContractTests.cs=218`, `BreadcrumbSelectionSession.cs=210`, `BreadcrumbSelectionSessionTests.cs=203`, `QfcItemControllerBreadcrumbDropDownTests.cs=190`, `BreadcrumbPopupPlacementTests.cs=169`, `BreadcrumbBridgeCoordinatorProbabilityTests.cs=162`, `BreadcrumbMessengerHubTests.cs=148`, `BreadcrumbSelectorMessagesTests.cs=143`, `BreadcrumbStateModelSelectorTests.cs=121`, `BreadcrumbRenderProjectionSelectorTests.cs=118`, `ItemViewerBreadcrumbDropDownContractTests.cs=100`, `BreadcrumbPopupPlacement.cs=87`, `ItemViewer.FolderSearch.cs=74`, `IBreadcrumbDropDownHost.cs=42`.

## Whitespace Gate

Command:

```powershell
git diff --check
```

EXIT_CODE: 0

## Protected-File Gate

Command:

```powershell
git diff --exit-code $baselineSha -- QuickFiler/Viewers/ItemViewer.Designer.cs UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs
```

EXIT_CODE: 0

UNTRACKED_COUNT: 57

TEMPORARY_FILE_COUNT: 0

Output Summary: All untracked entries are planned issue #400 source, tests, feature documents, or evidence. No temporary file was found. All protected files are unchanged.
