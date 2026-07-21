# Phase 0 — R1 Over-Limit File Line Counts Baseline (P0-T6)

Timestamp: 2026-07-20T22-52

Command: `wc -l UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`

EXIT_CODE: 0

Output Summary (R1 FAIL starting state — both files exceed the 500-line limit):
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs = 536 lines (> 500, FAIL)
- UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs = 545 lines (> 500, FAIL)

Both files will be split into `sealed partial class` pairs (Phase 1) so every resulting file is < 500
lines, with each test method existing in exactly one file and shared helpers present exactly once.
