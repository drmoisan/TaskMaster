# Final File Sizes (Issue #1007, P18-T3)

Timestamp: 2026-10-25T22-45
Command: physical line count per file, `@(Get-Content -LiteralPath <path>).Count` (helper under SCRATCH; not Measure-Object -Line), compared with baseline-file-sizes.2026-10-23T13-42.md (P0-T8)
EXIT_CODE: 0
Output Summary: 53 new or modified C# and script files counted. The largest is QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs at 500 (P0-T8: 500). Every count is at or below 500. Every NO-GROWTH test file is at or below its P0-T8 count, and every NO-GROWTH production file is at or below 500.

## Production (modified)

| Final | P0-T8 | File |
|---|---|---|
| 410 | 370 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs |
| 257 | 200 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs |
| 303 | 298 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs |
| 201 | 185 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs |
| 234 | 220 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs |
| 209 | 197 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs |
| 264 | 268 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs |
| 411 | 408 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs (NO-GROWTH, +3 within P11-T2's five-line allowance) |
| 492 | 446 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs (NO-GROWTH, call-throughs only, at most 500) |
| 168 | 144 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs |
| 476 | 371 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs |
| 302 | 289 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs |
| 403 | 404 | UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs (NO-GROWTH) |
| 238 | 225 | UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs |
| 309 | 302 | UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs |
| 453 | 471 | QuickFiler/Controllers/BreadcrumbBridgeRouter.cs (NO-GROWTH) |
| 220 | 222 | QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs |
| 331 | 305 | QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs |
| 129 | 104 | QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs |
| 438 | 451 | QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs (NO-GROWTH) |
| 393 | 350 | QuickFiler/Resources/FolderBreadcrumb.js |

## Production (created)

| Final | P0-T8 | File |
|---|---|---|
| 170 | n/a | UtilitiesCS/OutlookObjects/Folder/BreadcrumbVisibleOrder.cs |
| 363 | n/a | UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Tree.cs |
| 232 | n/a | UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Navigation.cs |
| 253 | n/a | QuickFiler/Controllers/BreadcrumbBridgeRouter.Navigation.cs |
| 165 | n/a | QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Keyboard.cs |

## Disposition Register test files and scripts

| Final | P0-T8 | File |
|---|---|---|
| 444 | 462 | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs (NO-GROWTH) |
| 320 | 307 | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs |
| 369 | 368 | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs (P8-T6 allows at most +3) |
| 400 | 379 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs |
| 447 | 454 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs (NO-GROWTH) |
| 293 | 292 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs |
| 248 | 243 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTests.cs |
| 486 | 495 | UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs (NO-GROWTH) |
| 396 | 383 | UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs |
| 450 | 354 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs |
| 488 | 488 | QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs (NO-GROWTH) |
| 500 | 500 | QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs (NO-GROWTH) |
| 429 | 415 | QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs |
| 241 | 239 | tests/quickfiler-breadcrumb/issue-997-common.test.mjs |
| 131 | 128 | tests/quickfiler-breadcrumb/issue-997-browser-harness.mjs |

## Test files (created)

| Final | P0-T8 | File |
|---|---|---|
| 317 | n/a | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbVisibleOrderTests.cs |
| 286 | n/a | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTreeIssue1007Tests.cs |
| 417 | n/a | UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.cs |
| 135 | n/a | UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderIssue1007Tests.cs |
| 153 | n/a | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbEmptyExpanderIssue1007Tests.cs |
| 93 | n/a | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbBridgeJsKeysIssue1007Tests.cs |
| 332 | n/a | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.cs |
| 326 | n/a | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.Navigation.cs |
| 142 | n/a | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.DoubleClick.cs |
| 459 | n/a | QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.cs |
| 158 | n/a | QuickFiler.Test/Viewers/FolderBreadcrumbKeysIssue1007Tests.cs |
| 136 | n/a | tests/quickfiler-breadcrumb/issue-1007-keys.test.mjs |
