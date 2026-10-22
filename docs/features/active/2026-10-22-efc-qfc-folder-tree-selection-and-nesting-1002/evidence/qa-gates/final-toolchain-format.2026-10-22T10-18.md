# Final Toolchain: FORMAT (#1002)

Timestamp: 2026-10-22T10-18
Command: git status --porcelain --untracked-files=all; dotnet tool run csharpier format .; git status --porcelain --untracked-files=all; dotnet tool run csharpier check .
EXIT_CODE: 0
Output Summary:
- Loop pass 2 (recorded clean). `csharpier format .` printed `Formatted 1674 files in 1654ms.`; `csharpier check .` exited 0 and printed `Checked 1674 files in 5104ms.`
- The two status captures are identical (51 lines each). Because every Write Set file was already modified or untracked, status alone cannot show a rewrite, so the pass also compared SHA-256 hashes of every `.cs` and `.csproj` file under UtilitiesCS, UtilitiesCS.Test, QuickFiler, and QuickFiler.Test before and after the format: 0 files changed.
- Loop pass 1 (not clean): the hash comparison found that the format rewrote 11 files, all in the Write Set (`BreadcrumbHtmlRenderer.cs`, `BreadcrumbRow.Descendants.cs`, `BreadcrumbBridgeRouter.Descendants.cs`, and the eight new test files: line wrapping only). No file outside the Write Set was rewritten, so the loop restarted at P9-T1 rather than halting.

## Status capture (identical before and after, non-feature-folder entries)

```
 M QuickFiler.Test/QuickFiler.Test.csproj
 M QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs
 M QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs
 M QuickFiler/Controllers/BreadcrumbBridgeRouter.cs
 M QuickFiler/Controllers/EfcFormController.Actions.cs
 M QuickFiler/Controllers/EfcFormController.Breadcrumb.cs
 M QuickFiler/QuickFiler.csproj
 M QuickFiler/Resources/FolderBreadcrumb.js
 M UtilitiesCS.Test/UtilitiesCS.Test.csproj
 M UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs
 M UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs
 M UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs
 M UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs
 M UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs
 M UtilitiesCS/UtilitiesCS.csproj
?? QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1002Tests.cs
?? QuickFiler.Test/Controllers/BreadcrumbBridgeRouterStaleCompletionIssue1002Tests.cs
?? QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTrashIssue1002Tests.cs
?? QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs
?? QuickFiler.Test/Viewers/FolderBreadcrumbNestingIssue1002Tests.cs
?? QuickFiler/Controllers/BreadcrumbBridgeRouter.Descendants.cs
?? UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererIssue1002Tests.cs
?? UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecIssue1002Tests.cs
?? UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowDescendantsIssue1002Tests.cs
?? UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.Descendants.cs
?? tests/quickfiler-breadcrumb/issue-1002-nesting.test.mjs
```

The remaining 25 entries of each capture are the plan file and evidence files under the feature folder.
