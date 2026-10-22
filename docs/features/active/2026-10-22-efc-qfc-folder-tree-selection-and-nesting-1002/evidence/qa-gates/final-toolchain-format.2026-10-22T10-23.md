# Final Toolchain: FORMAT (#1002)

Timestamp: 2026-10-22T10-23
Command: git status --porcelain --untracked-files=all; dotnet tool run csharpier format .; git status --porcelain --untracked-files=all; dotnet tool run csharpier check .
EXIT_CODE: 0
Output Summary:
- Loop pass 3 (the recorded clean pass). `csharpier format .` printed `Formatted 1674 files in 1631ms.`; `csharpier check .` exited 0 and printed `Checked 1674 files in 4568ms.`
- The two status captures are identical (55 lines each). Because every Write Set file was already modified or untracked, the pass also compared SHA-256 hashes of every `.cs` and `.csproj` file under UtilitiesCS, UtilitiesCS.Test, QuickFiler, and QuickFiler.Test before and after the format: 0 files changed.
- Why pass 3: pass 1 rewrote 11 Write Set files (line wrapping in the new files) and restarted the loop; pass 2 was clean through all four steps, but P9-T5 then found two new methods below 90 percent line coverage (`SetLeafDescendants` 88.24 percent, `SelectPreferredRow` 88.24 percent: their argument-null throw and the refused-preferred-row return were untested). Two tests were added (`SetLeafDescendants_NullChildren_Throws` in BreadcrumbRowDescendantsIssue1002Tests, `BindRows_PreferredRowOutsideArchiveRoot_SelectsNothing` in BreadcrumbBridgeRouterTrashIssue1002Tests), which restarted the loop. The pass-2 artifacts (stamped 2026-10-22T10-18 to 10-21) are superseded by this pass.

## Status capture (identical before and after; source and project entries)

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

The remaining 29 entries of each capture are the plan file and evidence files under the feature folder.
