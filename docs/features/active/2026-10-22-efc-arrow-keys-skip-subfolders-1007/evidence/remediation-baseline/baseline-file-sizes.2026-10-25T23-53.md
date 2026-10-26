# Baseline File Sizes (Issue #1007, remediation cycle 1, P0-T5)

Timestamp: 2026-10-25T23-53
Command: physical line count per file, `@(Get-Content -LiteralPath <file>).Count` (not `Measure-Object -Line`), from a SCRATCH helper
EXIT_CODE: 0
Output Summary: Six existing WRITE-SET C# files were counted. Every count is at most 500, and the largest is `FolderBreadcrumbBridgeRouterIssue1007Tests.cs` at 499 lines, which is not edited. The two new files do not yet exist.

| File | Physical lines |
|---|---|
| QuickFiler.Test/Viewers/FolderBreadcrumbKeysIssue1007Tests.cs | 158 |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.DoubleClick.cs | 185 |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs | 444 |
| QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.cs (P3-T5 modifier edit only) | 477 |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs | 293 |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.cs (not edited) | 499 |
| QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.Unhandled.cs | absent (created by P3-T6) |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.Unanchored.cs | absent (created by P3-T2) |
