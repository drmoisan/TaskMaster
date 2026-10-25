# QFC Click and Double-Click Fail-Before (Issue #1007, P14-T4)

Timestamp: 2026-10-25T22-29
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; then vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeCoordinatorIssue1007Tests|FullyQualifiedName~FolderBreadcrumbBridgeRouterIssue1007Tests|FullyQualifiedName~BreadcrumbSelectorMessagesTests|FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.FolderBreadcrumbBridgeRouterTests|FullyQualifiedName~QuickFiler.Test.Viewers.BreadcrumbBridgeCoordinatorTests" (results under SCRATCH; `FolderBreadcrumbBridgeRouterTests` includes its `FolderBreadcrumbBridgeRouterInFlightTests.cs` partial)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: build `0 Error(s)`, `0 Warning(s)`. Tests: total 119, executed 119, passed 103, failed 16. Every G7 test (4 coordinator, 6 router, 3 serializer) and all three P14-T3 rewritten tests failed; the 103 passing rows are the unchanged and earlier-group tests of the same classes. Summary derived from the trx.

| Outcome | Class | Test | Failure message (from the trx, shortened) |
|---|---|---|---|
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorSegmentDoubleClick_CommitsSegmentFolderAndCloses | Expected the Projects path, but found the Beta path. |
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorSegmentDoubleClick_Closed_CommitsSegmentFolder | Expected the Projects path, but found the Beta path. |
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorSegmentDoubleClick_ThenAffordanceToggle_ListsSegmentChildren | Expected the last render to list Omega on a shortened row; the row rendered unshortened with the leaf children. |
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorHighlight_Open_MovesPendingOnlyAndKeepsListOpen | Expected the pending identity to be the Beta row, but found the Alpha row. |
| Failed | FolderBreadcrumbBridgeRouterIssue1007Tests | DoubleClickSelectorSegment_ActivatesCommitsAndShortens | NotImplementedException. |
| Failed | FolderBreadcrumbBridgeRouterIssue1007Tests | DoubleClickSelectorSegment_UnknownRowOrIndex_IsUnhandled | NotImplementedException. |
| Failed | FolderBreadcrumbBridgeRouterIssue1007Tests | AffordanceToggle_OnShortenedRow_ExpandsActiveSegmentWithOneQuery | NotImplementedException. |
| Failed | FolderBreadcrumbBridgeRouterIssue1007Tests | ShortenedRowExpanded_ProjectsMinusAtTerminal | NotImplementedException. |
| Failed | FolderBreadcrumbBridgeRouterIssue1007Tests | BridgeSegmentDoubleClick_IsLoggedNoOpWithoutStateChange | Expected outputs to be empty, but found a collapsed render. |
| Failed | FolderBreadcrumbBridgeRouterIssue1007Tests | HighlightSelector_Closed_IsUnhandled | NotImplementedException. |
| Failed | BreadcrumbSelectorMessagesTests | SegmentDoubleClickMessage_RoundTripsRowIdentityAndSegmentIndex | FormatException from Serialize/Parse (no wire form yet). |
| Failed | BreadcrumbSelectorMessagesTests | HighlightMessage_RoundTripsIdentity | FormatException from Serialize/Parse (no wire form yet). |
| Failed | BreadcrumbSelectorMessagesTests | Parse_SegmentDoubleClickWithNegativeIndex_RejectsExplicitly | Expected the message to name segmentIndex, but found "Unknown selector message type 'selectorSegmentDoubleClick'." |
| Failed (P14-T3) | FolderBreadcrumbBridgeRouterTests | Route_SegmentDoubleClick_ProducesCollapsedRenderPayload | Expected outputs to be empty, but found a collapsed render. |
| Failed (P14-T3) | FolderBreadcrumbBridgeRouterTests (InFlight partial) | Sequence_ExpandCollapseViaMessages_TransitionsDeterministically | NotImplementedException. |
| Failed (P14-T3) | BreadcrumbBridgeCoordinatorTests | InboundDoubleClick_PostsCollapsedRenderJson | InvalidOperationException: sequence contains no render. |
