# Project Include Gate

Timestamp: 2026-07-21T17:08:00Z

Command:

```powershell
$expected = [ordered]@{ 'UtilitiesCS/UtilitiesCS.csproj' = @('OutlookObjects\Folder\BreadcrumbSelectionSession.cs','OutlookObjects\Folder\BreadcrumbSelectorMessages.cs'); 'UtilitiesCS.Test/UtilitiesCS.Test.csproj' = @('OutlookObjects\Folder\BreadcrumbSelectionSessionTests.cs','OutlookObjects\Folder\BreadcrumbSelectorMessagesTests.cs','OutlookObjects\Folder\BreadcrumbStateModelSelectorTests.cs','OutlookObjects\Folder\BreadcrumbRenderProjectionSelectorTests.cs'); 'QuickFiler/QuickFiler.csproj' = @('Viewers\BreadcrumbMessengerHub.cs','Viewers\BreadcrumbPopupPlacement.cs','Viewers\IBreadcrumbDropDownHost.cs','Viewers\BreadcrumbDropDownHost.cs'); 'QuickFiler.Test/QuickFiler.Test.csproj' = @('Viewers\BreadcrumbBridgeCoordinatorProbabilityTests.cs','Viewers\BreadcrumbSelectorCoordinatorTests.cs','Viewers\BreadcrumbMessengerHubTests.cs','Viewers\BreadcrumbPopupPlacementTests.cs','Viewers\BreadcrumbDropDownHostTests.cs','Viewers\BreadcrumbDropDownLifecycleTests.cs','Viewers\ItemViewerBreadcrumbDropDownContractTests.cs','Viewers\BreadcrumbDropDownIntegrationTests.cs','Controllers\QfcItemControllerBreadcrumbDropDownTests.cs','Viewers\FolderBreadcrumbAssetContractTests.cs') }; $missing = @(); $duplicates = @(); foreach ($project in $expected.Keys) { [xml]$xml = Get-Content -LiteralPath $project -Raw; $includes = @($xml.Project.ItemGroup.Compile.Include); foreach ($include in $expected[$project]) { $count = @($includes | Where-Object { $_ -eq $include }).Count; if ($count -eq 0 -or -not (Test-Path -LiteralPath (Join-Path (Split-Path $project) $include))) { $missing += "$project::$include" }; if ($count -gt 1) { $duplicates += "$project::$include" } } }; if ($missing.Count -or $duplicates.Count) { throw "Missing=$($missing -join ','); Duplicates=$($duplicates -join ',')" }
```

EXIT_CODE: 0

Expected include count: 20

MISSING_COUNT: 0

DUPLICATE_COUNT: 0

Output Summary: Every named file exists and has exactly one explicit `Compile Include` entry in its applicable legacy project.
