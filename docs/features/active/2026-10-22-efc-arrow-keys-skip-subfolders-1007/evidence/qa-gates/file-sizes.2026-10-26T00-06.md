# File Sizes (Issue #1007, remediation cycle 1, P5-T11)

Timestamp: 2026-10-26T00-06
Command: physical line count per file, `@(Get-Content -LiteralPath <file>).Count` (not `Measure-Object -Line`), from a SCRATCH helper
EXIT_CODE: 0
Output Summary: PASS. Every WRITE-SET C# file is at most 500 physical lines; the largest is 499, and that file is unedited. Neither file near the limit received a new test.

| File | P0-T5 | Final | At most 500 |
|---|---|---|---|
| QuickFiler.Test/Viewers/FolderBreadcrumbKeysIssue1007Tests.cs | 158 | 230 | yes |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.DoubleClick.cs | 185 | 211 | yes |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs | 444 | 444 | yes |
| QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.cs | 477 | 477 | yes |
| QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.Unhandled.cs | new | 35 | yes |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs | 293 | 315 | yes |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.cs | 499 | 499 | yes |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.Unanchored.cs | new | 47 | yes |
