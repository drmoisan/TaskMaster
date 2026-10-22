# Baseline: Physical Line Counts (#1002)

Timestamp: 2026-10-22T09-52
Command: pwsh: foreach file, @(Get-Content -LiteralPath <file>).Count (launcher SCRATCH/issue-1002-linecount.ps1)
EXIT_CODE: 0
Output Summary: physical line counts for every existing Write Set code file and the seven near-limit test files; new Write Set files are absent at baseline.

## Write Set code files (existing)

| File | Lines |
|---|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | 361 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs` | 173 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs` | 203 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | 234 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` | 149 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 459 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | 211 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | 236 |
| `QuickFiler/Controllers/EfcFormController.Actions.cs` | 184 |
| `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` | 208 |
| `QuickFiler/Resources/FolderBreadcrumb.js` | 347 |

## Write Set code files (new; absent at baseline)

`BreadcrumbRow.Descendants.cs`, `BreadcrumbBridgeRouter.Descendants.cs`, and the eight new test files (`BreadcrumbMessageCodecIssue1002Tests.cs`, `BreadcrumbRowDescendantsIssue1002Tests.cs`, `BreadcrumbHtmlRendererIssue1002Tests.cs`, `BreadcrumbBridgeRouterIssue1002Tests.cs`, `BreadcrumbBridgeRouterStaleCompletionIssue1002Tests.cs`, `BreadcrumbBridgeRouterTrashIssue1002Tests.cs`, `EfcFormControllerTests.Issue1002.cs`, `FolderBreadcrumbNestingIssue1002Tests.cs`) and `tests/quickfiler-breadcrumb/issue-1002-nesting.test.mjs`: 0 (absent).

## Near-limit test files (must not be extended)

| File | Lines |
|---|---|
| QuickFiler.Test/Controllers/EfcFormControllerTests.cs | 485 |
| QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs | 490 |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs | 462 |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs | 462 |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs | 463 |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs | 455 |
| QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs | 415 |
