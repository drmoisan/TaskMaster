Timestamp: 2026-10-18T01-14
Command: pwsh -NoProfile -File SCRATCH/issue-997-isolation.ps1 (seven fresh vstest.console.exe processes in recorded reverse fixture order)
EXIT_CODE: 0
ExpectedExitCode: 0
Output Summary: Fresh MSTest process per changed/new fixture; descending fixture order:
UtilitiesCS.Test.OutlookObjects.Folder.FolderBreadcrumbDescendantOrderingIssue997Tests: Passed!  - Failed:     0, Passed:     4, Skipped:     0, Total:     4, Duration: 374 ms - UtilitiesCS.Test.dll (net481)
UtilitiesCS.Test.OutlookObjects.Folder.BreadcrumbDescendantValidationIssue997Tests: Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 461 ms - UtilitiesCS.Test.dll (net481)
UtilitiesCS.Test.OutlookObjects.Folder.BreadcrumbDescendantSelectionIssue997Tests: Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 401 ms - UtilitiesCS.Test.dll (net481)
UtilitiesCS.Test.OutlookObjects.Folder.BreadcrumbDescendantMessagesIssue997Tests: Passed!  - Failed:     0, Passed:    10, Skipped:     0, Total:    10, Duration: 500 ms - UtilitiesCS.Test.dll (net481)
QuickFiler.Test.Viewers.FolderBreadcrumbRecursiveAssetIssue997Tests: Passed!  - Failed:     0, Passed:     3, Skipped:     0, Total:     3, Duration: 224 ms - QuickFiler.Test.dll (net481)
QuickFiler.Test.Viewers.FolderBreadcrumbAssetContractTests: Passed!  - Failed:     0, Passed:    15, Skipped:     0, Total:    15, Duration: 470 ms - QuickFiler.Test.dll (net481)
QuickFiler.Test.Viewers.BreadcrumbDescendantActivationIssue997Tests: Passed!  - Failed:     0, Passed:     4, Skipped:     0, Total:     4, Duration: 1 s - QuickFiler.Test.dll (net481)
