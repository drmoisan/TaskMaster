# Batch 2 format and size

Timestamp: 2026-07-21T16-20Z

Format Command: `csharpier format UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRenderProjectionSelectorTests.cs UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterEdgeTests.cs UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs`

Format EXIT_CODE: 0

Size Command: `Get-Item -LiteralPath 'UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs','UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs','UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRenderProjectionSelectorTests.cs','UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterEdgeTests.cs','UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs' | ForEach-Object { [pscustomobject]@{ Path = $_.FullName; Lines = (Get-Content -LiteralPath $_.FullName).Count } }`

Size EXIT_CODE: 0

| File | Lines |
|---|---:|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs` | 248 |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | 456 |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRenderProjectionSelectorTests.cs` | 118 |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterEdgeTests.cs` | 324 |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs` | 291 |

Protected File Command: `git diff --exit-code df5ad49c909f6b739edef45d0336151f44e827a6 -- UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs`

Protected File EXIT_CODE: 0

Output Summary: CSharpier formatted five files, every batch file remains at or below 500 lines, and the protected `BreadcrumbBridgeMessages.cs` file is unchanged from the baseline commit.
