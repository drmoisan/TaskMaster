# QA Gate: File Sizes (#1002)

Timestamp: 2026-10-22T10-16
Command: pwsh: foreach file, @(Get-Content -LiteralPath <file>).Count (SCRATCH/issue-1002-linecount.ps1); git diff $(git merge-base origin/main HEAD) -- QuickFiler/Controllers/BreadcrumbBridgeRouter.cs
EXIT_CODE: 0
Output Summary: every new or modified C# file and `FolderBreadcrumb.js` is at most 500 physical lines; `BreadcrumbBridgeRouter.cs` gains only the two delegating case arms and the preferred-row parameter (with its documentation and its call); none of the seven near-limit test files is in the change set.

| File | Baseline (P0-T7) | Now |
|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | 361 | 370 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs` | new | 199 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs` | 173 | 197 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs` | 203 | 220 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | 234 | 294 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` | 149 | 179 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 459 | 471 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs` | new | 95 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | 211 | 222 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | 236 | 305 |
| `QuickFiler/Controllers/EfcFormController.Actions.cs` | 184 | 193 |
| `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` | 208 | 215 |
| `QuickFiler/Resources/FolderBreadcrumb.js` | 347 | 350 |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecIssue1002Tests.cs` | new | 108 |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowDescendantsIssue1002Tests.cs` | new | 449 |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererIssue1002Tests.cs` | new | 278 |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1002Tests.cs` | new | 459 |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterStaleCompletionIssue1002Tests.cs` | new | 244 |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTrashIssue1002Tests.cs` | new | 131 |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs` | new | 140 |
| `QuickFiler.Test/Viewers/FolderBreadcrumbNestingIssue1002Tests.cs` | new | 101 |

## `BreadcrumbBridgeRouter.cs` merge-base diff (summary)

Three hunks, additions only: the `<param name="preferredRowText">` documentation (4 lines) and the trailing parameter `string? preferredRowText = null` on the internal `BindRowsAsync`; the call `SelectPreferredRow(preferredRowText);` before `DeliverDocument();`; and two case arms in `ProcessInboundAsync` delegating to `HandleDescendantToggleAsync(row, message.NodeId!)` and `ActivateDescendant(row, message.NodeId!)`. No other logic.

## Near-limit test files

`EfcFormControllerTests.cs` (485), `EfcFormControllerTests.Part2.cs` (490), `BreadcrumbBridgeRouterTests.cs` (462), `BreadcrumbBridgeRouterQueueTests.cs` (462), `BreadcrumbBridgeRouterScoreJoinTests.cs` (463), `BreadcrumbBridgeRouterIssue439Tests.cs` (455), `FolderBreadcrumbAssetContractTests.cs` (415): counts unchanged from baseline, and none appears in the P8-T1 path list.

Counts are taken before the Phase 9 format pass; P9-T1 records whether that pass rewrote any file.
