# EFC Vertical-Key Fail-Before (Issue #1007, P6-T3)

Timestamp: 2026-10-23T14-00
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; then vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterIssue1007Tests|FullyQualifiedName~QuickFiler.Test.Controllers.BreadcrumbBridgeRouterTests" (results under SCRATCH)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: build `0 Error(s)`. Tests: total 39, executed 39, passed 27, failed 12. Every G3 non-guard test failed on its assertion; the three guards and `ArrowKey_UnknownKey_IsLoggedNoOp` passed. Summary derived from the trx.

Deviation recorded: `QuickFiler.Test` compiles at C# 7.3 (no `LangVersion` in its project file), so the new QuickFiler.Test files cannot carry `#nullable enable`; the first build failed with CS8370 and the directive and `?` annotations were removed from those files. UtilitiesCS.Test files keep the directive.

| Outcome | Test | Failure message (from the trx) |
|---|---|---|
| Failed | ArrowDown_FromExpandedRow_SelectsFirstVisibleDescendant | SelectedFolderPath actual "Clients\South", expected "Clients\North\Q3". |
| Failed | ArrowDown_FromSelectedDescendant_SelectsNextVisibleEntry | SelectedFolderPath actual "Clients\South", expected "Clients\North\Q4". |
| Failed | ArrowDown_SkipsChildrenOfCollapsedNode | SelectedFolderPath actual "Clients\South", expected "Clients\North\Q4". |
| Failed | ArrowUp_FromRowBelowExpandedBlock_SelectsLastVisibleDescendant | Expected "Clients\North\Q4", but "Clients\North". |
| Failed | ArrowDown_OntoDescendant_CommitsArchiveRelativePathAndRaisesChanged | Expected SelectionEvents {"Clients\North\Q3"}, but {"Clients\South"}. |
| Failed | ArrowDown_OntoDescendant_ThenRowConfirmed_RaisesConfirmationForDescendant | Expected Confirmations to be 1, but found 0. |
| Failed | Home_SelectsFirstEntryWithoutFocusSearch | SelectedFolderPath actual "Clients\South", expected "Clients\North". |
| Failed | End_SelectsLastVisibleDescendant | SelectedFolderPath actual "Clients\North", expected "Clients\South\S1". |
| Failed | PageDown_MovesByPageSizeAndClamps | Expected "Clients\North\Q4", but "Clients\North". |
| Failed | PageUp_MovesByPageSizeAndClamps | SelectedFolderPath actual "Clients\South", expected "Clients\North\Q3". |
| Failed | PageDown_NonIntegerPageSize_IsRejectedByCodec | Expected a BreadcrumbMessageException to be thrown, but no exception was thrown. |
| Failed | NoSelection_DownAndHomeSelectFirst_UpAndEndSelectLast | Expected "Clients\North" because Down with no selection, but found null. |
| Passed (guard) | ArrowDown_OnLastVisibleEntry_PostsNothing | - |
| Passed (guard) | ArrowUp_OnFirstEntry_RaisesFocusSearchRequested | - |
| Passed (guard) | PageKey_InvalidPageSize_IsLoggedNoOp | - |
| Passed (edited, P6-T2) | ArrowKey_UnknownKey_IsLoggedNoOp | - |
