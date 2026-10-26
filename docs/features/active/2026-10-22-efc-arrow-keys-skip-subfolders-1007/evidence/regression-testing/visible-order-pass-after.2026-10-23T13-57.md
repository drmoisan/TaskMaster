# Visible-Order Pass-After (Issue #1007, P5-T4)

Timestamp: 2026-10-23T13-57
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; then vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~Breadcrumb|FullyQualifiedName~OutlookFolderHierarchyProvider" (results under SCRATCH)
EXIT_CODE: 0
Output Summary: build `0 Error(s)`, `0 Warning(s)`. BREADCRUMB-FILTER run: total 970, executed 970, passed 970, failed 0. Every G2 test passed; the unchanged `BreadcrumbRowDescendantsIssue1002Tests` class passed within the same run.

| Outcome | Test |
|---|---|
| Passed | Step_DownFromExpandedRow_VisitsDescendantsInPreOrder |
| Passed | Step_SkipsChildrenOfCollapsedNode |
| Passed | Step_FromBoundaryOutward_ReturnsMinusOne |
| Passed | Step_PageDelta_ClampsToFirstAndLast |
| Passed | FirstAndLast_ReturnBoundaryIndices |
| Passed | FirstAndLast_EmptyOrder_ReturnMinusOne |
| Passed | IndexOf_UnknownEntry_ReturnsMinusOne |
| Passed | IndexOf_NullOrder_Throws |
| Passed | ParentOf_NodeEntry_ReturnsParentIndex |
| Passed | AppendVisiblePositions_ExpandedRow_AddsRowThenDescendantsInPreOrder |
| Passed | AppendVisiblePositions_BannerRow_AddsNothing |
| Passed | AppendVisiblePositions_ZeroChildrenNode_ReportsNoChildren |
| Passed | VisiblePositions_QfcModel_SkipsNonSelectableRowsAndCollapsedChildren |
