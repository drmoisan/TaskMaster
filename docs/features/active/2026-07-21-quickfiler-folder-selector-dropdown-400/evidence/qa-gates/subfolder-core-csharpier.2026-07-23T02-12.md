# Subfolder Core CSharpier Gate

Timestamp: 2026-07-23T02:12:29.0606785Z

Command: `csharpier format 'UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs' 'UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs' 'UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSubfolderSelectorSessionTests.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterEdgeTests.cs'; csharpier check 'UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs' 'UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs' 'UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSubfolderSelectorSessionTests.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterEdgeTests.cs'`

EXIT_CODE: 0

Output Summary: A second mutating CSharpier format pass retained identical SHA-256 hashes for all six scoped files (`STABLE_FILE_COUNT=6`, `CHANGED_FILE_COUNT=0`), and the subsequent scoped check reported `Checked 6 files` with exit code 0. Current line counts are 288, 473, and 461 for the three production files and 159, 289, and 437 for the three test files; every scoped file remains at most 500 lines. Scoped `git diff --check` also exited 0.
