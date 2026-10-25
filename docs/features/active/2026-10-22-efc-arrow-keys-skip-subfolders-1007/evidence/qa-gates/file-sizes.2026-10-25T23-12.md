# Final File Sizes, Refreshed after the Coverage Remediation (Issue #1007, P18-T3)

Timestamp: 2026-10-25T23-12
Command: physical line count per file, `@(Get-Content -LiteralPath <path>).Count` (helper under SCRATCH; not Measure-Object -Line), compared with baseline-file-sizes.2026-10-23T13-42.md (P0-T8)
EXIT_CODE: 0
Output Summary: 53 new or modified C# and script files counted after the P19 coverage remediation. Every count is at most 500. The largest are QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs at 500 (P0-T8: 500) and UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.cs at 499 (new). Every NO-GROWTH test file is at or below its P0-T8 count, and every NO-GROWTH production file is at most 500. This file supersedes file-sizes.2026-10-25T22-45.md, whose counts are unchanged except for the four rows marked "changed since 22-45".

| Final | P0-T8 | File |
|---|---|---|
| 410 | 370 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs |
| 257 | 200 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs |
| 303 | 298 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs |
| 201 | 185 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs |
| 234 | 220 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs |
| 209 | 197 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs |
| 264 | 268 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs |
| 411 | 408 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs (NO-GROWTH, +3 within P11-T2's allowance) |
| 492 | 446 | UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs (NO-GROWTH, call-throughs only) |
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
| 170 | n/a | UtilitiesCS/OutlookObjects/Folder/BreadcrumbVisibleOrder.cs (new) |
| 363 | n/a | UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Tree.cs (new) |
| 232 | n/a | UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Navigation.cs (new) |
| 253 | n/a | QuickFiler/Controllers/BreadcrumbBridgeRouter.Navigation.cs (new) |
| 165 | n/a | QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Keyboard.cs (new) |
| 444 | 462 | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs (NO-GROWTH) |
| 320 | 307 | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs |
| 369 | 368 | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs (P8-T6 allows at most +3) |
| 400 | 379 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs |
| 447 | 454 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs (NO-GROWTH) |
| 293 | 292 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs |
| 248 | 243 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTests.cs |
| 486 | 495 | UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs (NO-GROWTH) |
| 396 | 383 | UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs |
| 456 | 354 | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs (changed since 22-45) |
| 488 | 488 | QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs (NO-GROWTH) |
| 500 | 500 | QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs (NO-GROWTH) |
| 429 | 415 | QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs |
| 241 | 239 | tests/quickfiler-breadcrumb/issue-997-common.test.mjs |
| 131 | 128 | tests/quickfiler-breadcrumb/issue-997-browser-harness.mjs |
| 317 | n/a | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbVisibleOrderTests.cs (new) |
| 416 | n/a | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTreeIssue1007Tests.cs (new; changed since 22-45) |
| 499 | n/a | UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.cs (new; changed since 22-45) |
| 135 | n/a | UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderIssue1007Tests.cs (new) |
| 153 | n/a | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbEmptyExpanderIssue1007Tests.cs (new) |
| 93 | n/a | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbBridgeJsKeysIssue1007Tests.cs (new) |
| 332 | n/a | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.cs (new) |
| 326 | n/a | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.Navigation.cs (new) |
| 185 | n/a | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.DoubleClick.cs (new; changed since 22-45) |
| 477 | n/a | QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.cs (new) |
| 158 | n/a | QuickFiler.Test/Viewers/FolderBreadcrumbKeysIssue1007Tests.cs (new) |
| 136 | n/a | tests/quickfiler-breadcrumb/issue-1007-keys.test.mjs (new) |
