# Group B Fail-Before: EFC Trash Preselection (#1002)

Timestamp: 2026-10-22T10-10
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbBridgeRouterTrashIssue1002Tests|FullyQualifiedName~EfcFormControllerTests" /ResultsDirectory:SCRATCH/results-issue-1002-p4t4 /Logger:"trx;LogFileName=issue-1002-p4t4.trx"
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- Build: `0 Error(s)`, `0 Warning(s)`.
- Test counts (derived from the trx): total 42, executed 42, passed 36, failed 6. The EfcFormControllerTests filter covers the class across its three partial files (data rows counted separately).
- Every test in the Group B fail-before set is reported Failed (FB); `ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows` is reported Passed.
- `BreadcrumbBridgeRouterTrashIssue1002Tests.cs` was drafted on disk during Phase 2 but was not registered in `QuickFiler.Test/QuickFiler.Test.csproj` (so not compiled) until this phase; its registration and first compilation are part of P4-T2.

## New Group B tests and the named existing test

| Outcome | Test | FB | Message (truncated) |
|---|---|---|---|
| Failed | BindRows_PreferredTrashRow_SelectsTrashInDeliveredDocument | FB | Expected fixture.Router.SelectedFolderPath to be "Trash to Delete", but found <null>. |
| Failed | BindRows_PreferredTrashRow_LastSelectionNotificationCarriesTrash | FB | Expected fixture.SelectionEvents.Count(path => path == Trash) to be 1, but found 0. |
| Passed | BindRows_PreferredTrashRow_PostsNoFollowUpRender |  |  |
| Passed | BindRows_PreferredRowAbsent_SelectsNothing |  |  |
| Passed | BindRows_NoPreferredRow_KeepsExistingClearBehavior |  |  |
| Failed | BindRows_PreferredTrashRowBeforeCoreInit_PendingDocumentCarriesSelection | FB | Expected the pending document to contain the selected trash rowwrap; it did not. |
| Failed | ActionDeleteAsync_WithRouter_SelectsTrashEntry | FB | Expected controller.SelectedFolder to be "Trash to Delete", but found <null>. |
| Failed | ActionDeleteAsync_WithRouter_DeliveredDocumentMarksTrashSelected | FB | Expected documents.Last() to contain the selected trash rowwrap; it did not. |
| Failed | ActionDeleteAsync_AwaitedTwice_KeepsTrashSelectedAndSingleTrashRow | FB | Expected controller.SelectedFolder to be "Trash to Delete", but found <null>. |
| Passed | ActionDeleteAsync_BindFault_IsReportedAtBindBoundary |  |  |
| Passed | ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows | existing | required Passed |
| Passed | AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow (five data rows) | existing |  |

Group B fail-before set (6): all Failed.
