Timestamp: 2026-10-18T01-01
Command: pwsh -NoProfile -Command '. ./scripts/vscode/Invoke-MSTestWithCoverage.ps1; Invoke-MSTestWithCoverageMain -SearchRoot . -Configuration Debug -CoverageOutput coverage/issue-997-baseline-20261018005944.cobertura.xml -ResultsDirectory SCRATCH/issue-997-baseline-results-20261018005944 -ScriptRoot ./scripts/vscode'
EXIT_CODE: 0
ExpectedExitCode: 0
Output Summary:      Passed: 7471
First-party coverage: lines 56931/66178 (86.03%), branches 13792/17195 (80.21%)
Detailed snapshot: SCRATCH/issue-997-baseline-20261018005944.cobertura.xml; source/copy SHA256 equal; classes: 592.

## Per-production-file measurements
QuickFiler\Controllers\BreadcrumbBridgeRouter.Arrows.cs: 128/133 executable lines (96.24%).
QuickFiler\Controllers\BreadcrumbBridgeRouter.cs: 239/241 executable lines (99.17%).
QuickFiler\Controllers\BreadcrumbBridgeRouter.Selection.cs: 150/153 executable lines (98.04%).
QuickFiler\Controllers\BreadcrumbOutboundQueue.cs: 29/29 executable lines (100%).
QuickFiler\Controllers\EfcFormController.Breadcrumb.cs: 78/115 executable lines (67.83%).
QuickFiler\Controllers\QfcItemController.BreadcrumbWiring.cs: 0/1 executable lines (0%).
QuickFiler\Viewers\BreadcrumbBridgeCoordinator.cs: 244/244 executable lines (100%).
QuickFiler\Viewers\BreadcrumbBridgeCoordinator.Search.cs: 32/32 executable lines (100%).
QuickFiler\Viewers\BreadcrumbBridgeCoordinator.Suggestions.cs: 51/51 executable lines (100%).
QuickFiler\Viewers\BreadcrumbCollapsedSurfaceController.cs: 193/195 executable lines (98.97%).
QuickFiler\Viewers\BreadcrumbCoordinatorUpgradeLifetime.cs: 209/209 executable lines (100%).
QuickFiler\Viewers\BreadcrumbDropDownHost.cs: 254/256 executable lines (99.22%).
QuickFiler\Viewers\BreadcrumbDropDownHost.Diagnostics.cs: 28/28 executable lines (100%).
QuickFiler\Viewers\BreadcrumbDropDownHost.Open.cs: 65/65 executable lines (100%).
QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs: 234/238 executable lines (98.32%).
QuickFiler\Viewers\BreadcrumbDropDownOpenLifetime.cs: 317/320 executable lines (99.06%).
QuickFiler\Viewers\BreadcrumbDropDownOpenLifetime.Focus.cs: 19/19 executable lines (100%).
QuickFiler\Viewers\BreadcrumbItemViewerLifecycleCoordinator.cs: 300/330 executable lines (90.91%).
QuickFiler\Viewers\BreadcrumbItemViewerLifecycleCoordinator.Search.cs: 5/5 executable lines (100%).
QuickFiler\Viewers\BreadcrumbMessengerHub.cs: 306/306 executable lines (100%).
QuickFiler\Viewers\BreadcrumbPopupOwnerRegistry.cs: 12/12 executable lines (100%).
QuickFiler\Viewers\BreadcrumbPopupPlacement.cs: 48/48 executable lines (100%).
QuickFiler\Viewers\BreadcrumbPopupUiOperations.cs: 229/231 executable lines (99.13%).
QuickFiler\Viewers\BreadcrumbUiDispatcher.cs: 187/187 executable lines (100%).
QuickFiler\Viewers\BreadcrumbWebViewSurfaceFactory.cs: 139/140 executable lines (99.29%).
QuickFiler\Viewers\WebView2BreadcrumbHost.cs: 100/110 executable lines (90.91%).
UtilitiesCS\OutlookObjects\Folder\BreadcrumbBridgeMessages.cs: 268/272 executable lines (98.53%).
UtilitiesCS\OutlookObjects\Folder\BreadcrumbHtmlRenderer.cs: 129/133 executable lines (96.99%).
UtilitiesCS\OutlookObjects\Folder\BreadcrumbMessageCodec.cs: 107/107 executable lines (100%).
UtilitiesCS\OutlookObjects\Folder\BreadcrumbMessages.cs: 30/30 executable lines (100%).
UtilitiesCS\OutlookObjects\Folder\BreadcrumbRenderProjection.cs: 123/123 executable lines (100%).
UtilitiesCS\OutlookObjects\Folder\BreadcrumbRow.cs: 153/155 executable lines (98.71%).
UtilitiesCS\OutlookObjects\Folder\BreadcrumbRowBuilder.cs: 116/116 executable lines (100%).
UtilitiesCS\OutlookObjects\Folder\BreadcrumbRowIdentity.cs: 68/68 executable lines (100%).
UtilitiesCS\OutlookObjects\Folder\BreadcrumbSegment.cs: 6/6 executable lines (100%).
UtilitiesCS\OutlookObjects\Folder\BreadcrumbSelectionMap.cs: 52/52 executable lines (100%).
UtilitiesCS\OutlookObjects\Folder\BreadcrumbSelectionSession.cs: 316/316 executable lines (100%).
UtilitiesCS\OutlookObjects\Folder\BreadcrumbSelectionSession.Highlight.cs: 11/11 executable lines (100%).
UtilitiesCS\OutlookObjects\Folder\BreadcrumbSelectorMessages.cs: 178/178 executable lines (100%).
UtilitiesCS\OutlookObjects\Folder\BreadcrumbStateModel.cs: 118/120 executable lines (98.33%).
UtilitiesCS\OutlookObjects\Folder\BreadcrumbStateModel.Row.cs: 169/182 executable lines (92.86%).
UtilitiesCS\OutlookObjects\Folder\FolderBreadcrumbBridgeRouter.cs: 306/310 executable lines (98.71%).
UtilitiesCS\OutlookObjects\Folder\FolderBreadcrumbBridgeRouter.SearchPresentation.cs: 29/29 executable lines (100%).
UtilitiesCS\OutlookObjects\Folder\FolderBreadcrumbSegment.cs: 12/12 executable lines (100%).
