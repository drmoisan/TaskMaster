# Preserved Contract Correction CSharpier Gate

Timestamp: 2026-07-22T22-57

Command: `csharpier format 'UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs' 'QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs'; csharpier check 'UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs' 'QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs'`

EXIT_CODE: 0

Output Summary: The initial scoped format pass changed all 3 files, so the ordered gate restarted at P7-T19. The second scoped format pass retained identical SHA-256 hashes for all 3 files (`CHANGED_FILE_COUNT=0`), and scoped CSharpier check reported `Checked 3 files` with exit code 0. Post-format physical line counts are 474 for `BreadcrumbSelectionSession.cs`, 380 for `FolderBreadcrumbBridgeRouterInFlightTests.cs`, and 488 for `BreadcrumbBridgeCoordinatorTests.cs`; every scoped file remains at most 500 lines.
