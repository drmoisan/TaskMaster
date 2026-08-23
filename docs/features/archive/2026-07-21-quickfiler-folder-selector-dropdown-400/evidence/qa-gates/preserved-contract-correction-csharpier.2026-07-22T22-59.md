# Preserved Contract Correction CSharpier Gate

Timestamp: 2026-07-22T22-59

Command: `csharpier format 'UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs' 'QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs'; csharpier check 'UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs' 'QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs'`

EXIT_CODE: 0

Output Summary: This gate supersedes `preserved-contract-correction-csharpier.2026-07-22T22-57.md` after P7-T22 identified an in-scope stale row-identity assertion. The first post-correction format pass changed only `FolderBreadcrumbBridgeRouterInFlightTests.cs`; the second scoped pass retained identical SHA-256 hashes for all 3 files (`CHANGED_FILE_COUNT=0`), and scoped check reported `Checked 3 files`. Final line counts are 474, 383, and 488; every scoped file remains at most 500 lines.
