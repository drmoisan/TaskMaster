Timestamp: 2026-07-21T15-27Z

Command: `$paths=@('UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs','UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs','UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs','QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs','QuickFiler/Viewers/ItemViewer.Breadcrumb.cs','QuickFiler/Viewers/ItemViewer.FolderSearch.cs','QuickFiler/Controllers/QfcItemController.ViewerSetup.cs','QuickFiler/Resources/FolderBreadcrumb.html','UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterEdgeTests.cs','UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs','UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs','QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs','QuickFiler/Viewers/ItemViewer.Designer.cs'); $results=@($paths | ForEach-Object { if(-not (Test-Path -LiteralPath $_)){throw "Missing existing planned file: $_"}; [pscustomobject]@{Path=$_;Lines=(Get-Content -LiteralPath $_).Count} }); $over=@($results | Where-Object {$_.Lines -gt 500 -and $_.Path -ne 'QuickFiler/Viewers/ItemViewer.Designer.cs'}); if($over.Count){throw "Non-grandfathered files over 500 lines: $($over.Path -join ',')"}`

EXIT_CODE: 0

| Path | Lines | Baseline disposition |
|---|---:|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | 340 | Planned modification; within limit |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs` | 230 | Planned modification; within limit |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | 353 | Planned modification; within limit |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 226 | Planned modification; within limit |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 141 | Planned modification; within limit |
| `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` | 81 | Planned modification; within limit |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 368 | Planned modification; within limit |
| `QuickFiler/Resources/FolderBreadcrumb.html` | 244 | Planned modification; within limit |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterEdgeTests.cs` | 233 | Planned modification; within limit |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs` | 256 | Planned modification; within limit |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs` | 443 | Protected and unchanged |
| `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs` | 460 | Protected and unchanged |
| `QuickFiler/Viewers/ItemViewer.Designer.cs` | 6224 | Generated unchanged grandfathered file |

NonGrandfatheredOverLimitCount: 0

Output Summary: Every existing planned production/test source begins at or below 500 lines. The generated `ItemViewer.Designer.cs` is the sole over-limit file and is explicitly protected as an unchanged grandfathered file.
