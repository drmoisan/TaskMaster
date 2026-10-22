# QA Gate: File Sizes, Post-Format Recount (#1002)

Timestamp: 2026-10-22T10-28
Command: pwsh: foreach file, @(Get-Content -LiteralPath <file>).Count (SCRATCH/issue-1002-linecount.ps1)
EXIT_CODE: 0
Output Summary: recount after the Phase 9 format pass and the two coverage-remediation tests; supersedes the 2026-10-22T10-16 counts. Every new or modified C# file and `FolderBreadcrumb.js` is at most 500 physical lines; the seven near-limit test files are unchanged and absent from the change set. The `BreadcrumbBridgeRouter.cs` diff is unchanged from the 10-16 record (two case arms, the preferred-row parameter with its documentation, and its call).

| File | Baseline (P0-T7) | Now |
|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | 361 | 370 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs` | new | 200 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs` | 173 | 197 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs` | 203 | 220 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | 234 | 298 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` | 149 | 179 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 459 | 471 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs` | new | 104 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | 211 | 222 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | 236 | 305 |
| `QuickFiler/Controllers/EfcFormController.Actions.cs` | 184 | 193 |
| `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` | 208 | 215 |
| `QuickFiler/Resources/FolderBreadcrumb.js` | 347 | 350 |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecIssue1002Tests.cs` | new | 108 |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowDescendantsIssue1002Tests.cs` | new | 466 |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererIssue1002Tests.cs` | new | 297 |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1002Tests.cs` | new | 487 |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterStaleCompletionIssue1002Tests.cs` | new | 259 |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTrashIssue1002Tests.cs` | new | 155 |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs` | new | 140 |
| `QuickFiler.Test/Viewers/FolderBreadcrumbNestingIssue1002Tests.cs` | new | 101 |

Near-limit test files (unchanged): EfcFormControllerTests.cs 485, EfcFormControllerTests.Part2.cs 490, BreadcrumbBridgeRouterTests.cs 462, BreadcrumbBridgeRouterQueueTests.cs 462, BreadcrumbBridgeRouterScoreJoinTests.cs 463, BreadcrumbBridgeRouterIssue439Tests.cs 455, FolderBreadcrumbAssetContractTests.cs 415.

`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1002Tests.cs` (487) is close to the limit; it also holds the shared `EfcDescendantRouterFixture`, so a later addition belongs in a new file.
