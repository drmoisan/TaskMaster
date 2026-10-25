# EFC Vertical-Key Pass-After (Issue #1007, P7-T5)

Timestamp: 2026-10-23T14-03
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; then vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~Breadcrumb|FullyQualifiedName~OutlookFolderHierarchyProvider" (results under SCRATCH)
EXIT_CODE: 0
Output Summary: build `0 Error(s)`, `0 Warning(s)`. BREADCRUMB-FILTER run: total 985, executed 985, passed 985, failed 0. Every G3 test passed; `ArrowKey_UnknownKey_IsLoggedNoOp` and the unchanged `BreadcrumbMessageCodecTests` and `BreadcrumbMessageCodecIssue1002Tests` classes passed in the same run. `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` is 477 physical lines (at or below 500).

| Outcome | Test |
|---|---|
| Passed | ArrowDown_FromExpandedRow_SelectsFirstVisibleDescendant |
| Passed | ArrowDown_FromSelectedDescendant_SelectsNextVisibleEntry |
| Passed | ArrowDown_SkipsChildrenOfCollapsedNode |
| Passed | ArrowUp_FromRowBelowExpandedBlock_SelectsLastVisibleDescendant |
| Passed | ArrowDown_OntoDescendant_CommitsArchiveRelativePathAndRaisesChanged |
| Passed | ArrowDown_OntoDescendant_ThenRowConfirmed_RaisesConfirmationForDescendant |
| Passed | Home_SelectsFirstEntryWithoutFocusSearch |
| Passed | End_SelectsLastVisibleDescendant |
| Passed | PageDown_MovesByPageSizeAndClamps |
| Passed | PageUp_MovesByPageSizeAndClamps |
| Passed | PageDown_NonIntegerPageSize_IsRejectedByCodec |
| Passed | NoSelection_DownAndHomeSelectFirst_UpAndEndSelectLast |
| Passed | ArrowDown_OnLastVisibleEntry_PostsNothing |
| Passed | ArrowUp_OnFirstEntry_RaisesFocusSearchRequested |
| Passed | PageKey_InvalidPageSize_IsLoggedNoOp |
