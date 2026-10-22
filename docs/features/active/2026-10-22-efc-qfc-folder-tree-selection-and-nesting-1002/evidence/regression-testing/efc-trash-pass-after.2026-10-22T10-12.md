# Group B Pass-After: EFC Trash Preselection (#1002)

Timestamp: 2026-10-22T10-12
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~Breadcrumb|FullyQualifiedName~EfcFormController|FullyQualifiedName~EfcHomeController" /ResultsDirectory:SCRATCH/results-issue-1002-p5t3 /Logger:"trx;LogFileName=issue-1002-p5t3.trx"
EXIT_CODE: 0
Output Summary:
- Build: `0 Error(s)`, `0 Warning(s)`.
- Test counts (derived from the trx): total 1009, executed 1009, passed 1009, failed 0. Covers every breadcrumb suite (including BreadcrumbBridgeRouterIssue614Tests and BreadcrumbBridgeRouterTests), EfcFormControllerTests (three partials), EfcFormControllerIssue995Tests, and EfcHomeControllerExecuteMovesTests.

## Group B fail-before set and named tests: pass-after outcomes

| Outcome | Test |
|---|---|
| Passed | BindRows_PreferredTrashRow_SelectsTrashInDeliveredDocument |
| Passed | BindRows_PreferredTrashRow_LastSelectionNotificationCarriesTrash |
| Passed | BindRows_PreferredTrashRowBeforeCoreInit_PendingDocumentCarriesSelection |
| Passed | ActionDeleteAsync_WithRouter_SelectsTrashEntry |
| Passed | ActionDeleteAsync_WithRouter_DeliveredDocumentMarksTrashSelected |
| Passed | ActionDeleteAsync_AwaitedTwice_KeepsTrashSelectedAndSingleTrashRow |
| Passed | BindRows_PreferredTrashRow_PostsNoFollowUpRender |
| Passed | BindRows_PreferredRowAbsent_SelectsNothing |
| Passed | BindRows_NoPreferredRow_KeepsExistingClearBehavior |
| Passed | ActionDeleteAsync_BindFault_IsReportedAtBindBoundary |
| Passed | ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows |
| Passed | AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow (all five data rows) |
