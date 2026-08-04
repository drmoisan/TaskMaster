# Subfolder Core CSharpier Gate

Timestamp: 2026-07-23T02:19:50.3263601Z

Command: `csharpier format 'UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs' 'UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs' 'UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSubfolderSelectorSessionTests.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterEdgeTests.cs'; csharpier check 'UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs' 'UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs' 'UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSubfolderSelectorSessionTests.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs' 'UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterEdgeTests.cs'`

EXIT_CODE: 0

Output Summary: This corrected gate supersedes `subfolder-core-csharpier.2026-07-23T02-12.md`. A second mutating CSharpier format pass retained identical SHA-256 hashes for all six scoped files (`STABLE_FILE_COUNT=6`, `CHANGED_FILE_COUNT=0`), and scoped CSharpier check reported `Checked 6 files` with exit code 0. Post-format line counts are 331, 480, and 467 for the three production files and 195, 354, and 489 for the three test files; every scoped file remains at most 500 lines.
