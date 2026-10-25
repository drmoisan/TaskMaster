# Scope Fences (Issue #1007, P18-T1)

Timestamp: 2026-10-25T22-44
Command: git diff --name-status <merge-base of origin/main and HEAD> plus git status --porcelain --untracked-files=all, each path classified against the plan's File Sets by a helper under SCRATCH
EXIT_CODE: 0
Output Summary: 91 distinct paths. Classes: 21 PROD-MODIFY, 5 PROD-CREATE, 14 TEST-CREATE, 14 DISPOSITION-REGISTER, 4 project files, 2 named others (the browser harness and the promoted potential entry), and 31 paths under `<FEATURE>/`. 0 paths outside the fence and 0 EXCLUDED paths. The only project files are the four legacy project files `UtilitiesCS/UtilitiesCS.csproj`, `QuickFiler/QuickFiler.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj`. The merge base is recorded by derivation, not by commit identifier.

Note: `PROD-MODIFY` lists 21 files in the plan; every one appears in the diff. `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` and `QuickFiler/Resources/FolderBreadcrumb.js` are among them. The Disposition Register files `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` and `FolderBreadcrumbBridgeRouterInFlightTests.cs` are partials of register classes.

| Status | Class | Path |
|---|---|---|
| A | NAMED-OTHER | docs/features/potential/promoted/2026-10-22-efc-arrow-keys-skip-subfolders.md |
| M | NAMED-OTHER | tests/quickfiler-breadcrumb/issue-997-browser-harness.mjs |
| M | PROJECT-FILE | QuickFiler.Test/QuickFiler.Test.csproj |
| M | PROJECT-FILE | QuickFiler/QuickFiler.csproj |
| M | PROJECT-FILE | UtilitiesCS.Test/UtilitiesCS.Test.csproj |
| M | PROJECT-FILE | UtilitiesCS/UtilitiesCS.csproj |
| ?? | PROD-CREATE | QuickFiler/Controllers/BreadcrumbBridgeRouter.Navigation.cs |
| ?? | PROD-CREATE | QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Keyboard.cs |
| ?? | PROD-CREATE | UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Tree.cs |
| ?? | PROD-CREATE | UtilitiesCS/OutlookObjects/Folder/BreadcrumbVisibleOrder.cs |
| ?? | PROD-CREATE | UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Navigation.cs |
| M | PROD-MODIFY | QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs |
| M | PROD-MODIFY | QuickFiler/Controllers/BreadcrumbBridgeRouter.cs |
| M | PROD-MODIFY | QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs |
| M | PROD-MODIFY | QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs |
| M | PROD-MODIFY | QuickFiler/Resources/FolderBreadcrumb.js |
| M | PROD-MODIFY | QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs |
| M | PROD-MODIFY | UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs |
| M | PROD-MODIFY | UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs |
| M | PROD-MODIFY | UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs |
| M | PROD-MODIFY | UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs |
| M | PROD-MODIFY | UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs |
| M | PROD-MODIFY | UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs |
| M | PROD-MODIFY | UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs |
| M | PROD-MODIFY | UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs |
| M | PROD-MODIFY | UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs |
| M | PROD-MODIFY | UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs |
| M | PROD-MODIFY | UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs |
| M | PROD-MODIFY | UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs |
| M | PROD-MODIFY | UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs |
| M | PROD-MODIFY | UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs |
| M | PROD-MODIFY | UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs |
| ?? | TEST-CREATE | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.cs |
| ?? | TEST-CREATE | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.DoubleClick.cs |
| ?? | TEST-CREATE | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.Navigation.cs |
| ?? | TEST-CREATE | QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.cs |
| ?? | TEST-CREATE | QuickFiler.Test/Viewers/FolderBreadcrumbKeysIssue1007Tests.cs |
| ?? | TEST-CREATE | tests/quickfiler-breadcrumb/issue-1007-keys.test.mjs |
| ?? | TEST-CREATE | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbBridgeJsKeysIssue1007Tests.cs |
| ?? | TEST-CREATE | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbEmptyExpanderIssue1007Tests.cs |
| ?? | TEST-CREATE | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTreeIssue1007Tests.cs |
| ?? | TEST-CREATE | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbVisibleOrderTests.cs |
| ?? | TEST-CREATE | UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.cs |
| ?? | TEST-CREATE | UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderIssue1007Tests.cs |
| M | DISPOSITION-REGISTER | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs |
| M | DISPOSITION-REGISTER | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs |
| M | DISPOSITION-REGISTER | QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs |
| M | DISPOSITION-REGISTER | QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs |
| M | DISPOSITION-REGISTER | QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs |
| M | DISPOSITION-REGISTER | QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs |
| M | DISPOSITION-REGISTER | tests/quickfiler-breadcrumb/issue-997-common.test.mjs |
| M | DISPOSITION-REGISTER | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs |
| M | DISPOSITION-REGISTER | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTests.cs |
| M | DISPOSITION-REGISTER | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs |
| M | DISPOSITION-REGISTER | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs |
| M | DISPOSITION-REGISTER | UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs |
| M | DISPOSITION-REGISTER | UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs |
| M | DISPOSITION-REGISTER | UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs |
| A or ?? | FEATURE | 31 paths under `<FEATURE>/` (issue, spec, plan, grill transcript, research and evidence Markdown) |
