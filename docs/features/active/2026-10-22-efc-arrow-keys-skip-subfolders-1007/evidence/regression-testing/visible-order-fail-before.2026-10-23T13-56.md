# Visible-Order Fail-Before (Issue #1007, P4-T2)

Timestamp: 2026-10-23T13-56
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; then vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:FullyQualifiedName~BreadcrumbVisibleOrderTests (results under SCRATCH)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: build `0 Error(s)`. Tests: total 13, executed 13, passed 0, failed 13. Every G2 test failed against the Phase 1 declaration-only seams. Summary derived from the trx.

| Outcome | Test | Failure message (from the trx) |
|---|---|---|
| Failed | Step_DownFromExpandedRow_VisitsDescendantsInPreOrder | NotImplementedException: The method or operation is not implemented. |
| Failed | Step_SkipsChildrenOfCollapsedNode | NotImplementedException: The method or operation is not implemented. |
| Failed | Step_FromBoundaryOutward_ReturnsMinusOne | NotImplementedException: The method or operation is not implemented. |
| Failed | Step_PageDelta_ClampsToFirstAndLast | NotImplementedException: The method or operation is not implemented. |
| Failed | FirstAndLast_ReturnBoundaryIndices | NotImplementedException: The method or operation is not implemented. |
| Failed | FirstAndLast_EmptyOrder_ReturnMinusOne | NotImplementedException: The method or operation is not implemented. |
| Failed | IndexOf_UnknownEntry_ReturnsMinusOne | NotImplementedException: The method or operation is not implemented. |
| Failed | IndexOf_NullOrder_Throws | Expected a System.ArgumentNullException to be thrown, but found System.NotImplementedException. |
| Failed | ParentOf_NodeEntry_ReturnsParentIndex | NotImplementedException: The method or operation is not implemented. |
| Failed | AppendVisiblePositions_ExpandedRow_AddsRowThenDescendantsInPreOrder | NotImplementedException: The method or operation is not implemented. |
| Failed | AppendVisiblePositions_BannerRow_AddsNothing | NotImplementedException: The method or operation is not implemented. |
| Failed | AppendVisiblePositions_ZeroChildrenNode_ReportsNoChildren | NotImplementedException: The method or operation is not implemented. |
| Failed | VisiblePositions_QfcModel_SkipsNonSelectableRowsAndCollapsedChildren | NotImplementedException: The method or operation is not implemented. |
