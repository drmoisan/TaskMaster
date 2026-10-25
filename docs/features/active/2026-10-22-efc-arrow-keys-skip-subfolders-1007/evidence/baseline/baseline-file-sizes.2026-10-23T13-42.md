# Baseline File Sizes (Issue #1007)

Timestamp: 2026-10-23T13-42
Command: physical line count per file, `@(Get-Content -LiteralPath <path>).Count` (helper under SCRATCH; not Measure-Object -Line)
EXIT_CODE: 0
Output Summary: 50 files counted; every count below is numeric. The largest are QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs (500) and QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs (497, EXCLUDED).

## PROD-MODIFY

| Lines | File |
|---|---|
| 370 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs |
| 200 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs |
| 298 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs |
| 185 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs |
| 220 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs |
| 197 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs |
| 268 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs |
| 408 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs (NO-GROWTH) |
| 446 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs (NO-GROWTH) |
| 144 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs |
| 371 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs |
| 289 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs |
| 404 | UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs (NO-GROWTH) |
| 225 | UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs |
| 302 | UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs |
| 471 | QuickFiler/Controllers/BreadcrumbBridgeRouter.cs (NO-GROWTH) |
| 222 | QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs |
| 305 | QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs |
| 104 | QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs |
| 451 | QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs (NO-GROWTH) |
| 350 | QuickFiler/Resources/FolderBreadcrumb.js |

## Disposition Register test files (NO-GROWTH tests marked)

| Lines | File |
|---|---|
| 462 | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs (NO-GROWTH) |
| 307 | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs |
| 368 | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs |
| 379 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs |
| 454 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs (NO-GROWTH) |
| 292 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs |
| 243 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTests.cs |
| 495 | UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs (NO-GROWTH) |
| 383 | UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs |
| 354 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs |
| 488 | QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs (NO-GROWTH) |
| 500 | QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs (NO-GROWTH) |
| 415 | QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs |
| 239 | tests/quickfiler-breadcrumb/issue-997-common.test.mjs |
| 128 | tests/quickfiler-breadcrumb/issue-997-browser-harness.mjs |

## EXCLUDED

| Lines | File |
|---|---|
| 414 | QuickFiler/Controllers/KeyboardHandler.cs |
| 219 | QuickFiler/Controllers/EfcFormController.Breadcrumb.cs |
| 490 | QuickFiler/Viewers/BreadcrumbMessengerHub.cs |
| 497 | QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs |
| 395 | QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs |
| 72 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbDescendantNode.cs |
| 376 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs |
| 137 | UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshot.cs |
| 84 | UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotBuilder.cs |
| 487 | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1002Tests.cs |
| 466 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowDescendantsIssue1002Tests.cs |
| 462 | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs |
| 455 | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs |
| 480 | UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs |
