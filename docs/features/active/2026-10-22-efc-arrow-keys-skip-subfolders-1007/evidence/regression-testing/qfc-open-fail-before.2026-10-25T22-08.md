# QFC Open-List Fail-Before (Issue #1007, P10-T6)

Timestamp: 2026-10-25T22-08
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; then vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbSelectionSessionTreeIssue1007Tests|FullyQualifiedName~FolderBreadcrumbBridgeRouterIssue1007Tests|FullyQualifiedName~BreadcrumbBridgeCoordinatorIssue1007Tests|FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.BreadcrumbStateModelTests|FullyQualifiedName~BreadcrumbSelectionSessionTests|FullyQualifiedName~BreadcrumbSelectorMessagesTests|FullyQualifiedName~UtilitiesCS.Test.OutlookObjects.Folder.FolderBreadcrumbBridgeRouterTests" (results under SCRATCH)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: build `0 Error(s)`, `0 Warning(s)`. Tests: total 142, executed 142, passed 100, failed 42. Every G5 non-guard test (11 session, 8 router, 7 coordinator), the two G5 serializer tests, the five new `SelectorKeyMessage_RoundTripsOnlySupportedKeys` rows, and all nine rewritten tests failed; both guards and both summary-only edits passed. Summary derived from the trx.

Run note: this run was resumed after the previous executor's host process exited. The first two attempts in this session discovered zero tests because the MSTest adapter failed to load ("The module was expected to contain an assembly manifest"). A byte scan found 2860 files under the project `bin/Debug` output folders that were zero-filled at their correct length, a state left by the abrupt host exit; MSBuild's incremental copy skipped them because their size and timestamp matched the package sources. The zero-filled output files were deleted individually (no source, `obj`, evidence or `packages/` file was zero-filled; 18291 files scanned), the solution was rebuilt with the command above, a re-scan found no binary without a valid header, and the run above is the re-run. The test sources were not changed between attempts.

| Outcome | Class | Test | Failure message (from the trx, shortened) |
|---|---|---|---|
| Failed | BreadcrumbSelectionSessionTreeIssue1007Tests | SelectorKeyDown_Open_MovesPendingThroughVisibleDescendants | Expected session.PendingNode to refer to a descendant node, but found null. |
| Failed | BreadcrumbSelectionSessionTreeIssue1007Tests | SelectorKeyUp_Open_FromRowBelowExpandedBlock_PendsLastVisibleDescendant | Expected session.PendingNode to refer to a descendant node, but found null. |
| Failed | BreadcrumbSelectionSessionTreeIssue1007Tests | CommitPending_WithPendingNode_SelectsDescendant | Expected model.SelectedDescendant to refer to a descendant node, but found null. |
| Failed | BreadcrumbSelectionSessionTreeIssue1007Tests | Cancel_RestoresOriginalAndClearsPendingNode | Expected session.PendingNode not to be null because the pending highlight rests on Delta. |
| Failed | BreadcrumbSelectionSessionTreeIssue1007Tests | MoveToBoundary_Open_PendsFirstAndLastVisibleEntry | NotImplementedException. |
| Failed | BreadcrumbSelectionSessionTreeIssue1007Tests | MovePage_Open_MovesByPageSizeAndClamps | NotImplementedException. |
| Failed | BreadcrumbSelectionSessionTreeIssue1007Tests | MovePage_InvalidPageSize_IsNoOp | NotImplementedException. |
| Failed | BreadcrumbSelectionSessionTreeIssue1007Tests | MoveToParent_Open_OnNode_PendsParentKeepingExpansion | NotImplementedException. |
| Failed | BreadcrumbSelectionSessionTreeIssue1007Tests | MoveToParent_Open_OnRow_ShortensPendingRowAtParentSegment | NotImplementedException. |
| Failed | BreadcrumbSelectionSessionTreeIssue1007Tests | MoveToParent_AtFirstSegment_IsNoOp | NotImplementedException. |
| Failed | BreadcrumbSelectionSessionTreeIssue1007Tests | SelectorKeyParent_Open_ThenEscape_RestoresRowShapeAndValue | NotImplementedException. |
| Passed (guard) | BreadcrumbSelectionSessionTreeIssue1007Tests | SelectorKeyDown_Open_OnLastEntry_KeepsPending | - |
| Passed (guard) | BreadcrumbSelectionSessionTreeIssue1007Tests | OpenMove_DoesNotChangeCommittedValue | - |
| Failed | FolderBreadcrumbBridgeRouterIssue1007Tests | OpenRight_OnPendingRowWithChildren_ExpandsPendingRow | Expected Rows[0].LeafExpanded to be True, but found False. |
| Failed | FolderBreadcrumbBridgeRouterIssue1007Tests | OpenRight_OnExpandedPendingEntry_PendsFirstChild | Expected PendingNodeIdentity to be the first child node identity, but found null. |
| Failed | FolderBreadcrumbBridgeRouterIssue1007Tests | OpenLeft_OnExpandedPendingEntry_CollapsesAndKeepsPending | Expected ActiveSegmentIndex to be 2 because Left never selects a parent, but found 1. |
| Failed | FolderBreadcrumbBridgeRouterIssue1007Tests | CollapseHidingPending_MovesPendingToNearestVisibleAncestor | Expected PendingNodeIdentity to be the ancestor node identity, but found null. |
| Failed | FolderBreadcrumbBridgeRouterIssue1007Tests | ClassifyPendingArrow_OpenLeftOnNonExpanded_ReturnsCommitAndClose | NotImplementedException. |
| Failed | FolderBreadcrumbBridgeRouterIssue1007Tests | ClassifyPendingArrow_OpenRightOnLeaf_ReturnsCancelAndPopOut | NotImplementedException. |
| Failed | FolderBreadcrumbBridgeRouterIssue1007Tests | ArrowRight_OnShortenedPendingRow_ListsSegmentChildren | NotImplementedException. |
| Failed | FolderBreadcrumbBridgeRouterIssue1007Tests | GetSelectedFolder_ShortenedRow_ReturnsActiveSegmentValue | Expected the Projects value, but found the leaf value ending Projects\Alpha. |
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | OpenLeft_OnNonExpandedEntry_CommitsAndCloses | Expected IsSelectorOpen to be False, but found True. |
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | OpenRight_OnLeaf_CancelsThenRaisesUnhandledArrowRight | Expected events {"open-state", "unhandled:Right"}, but found an empty collection. |
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorKeyParent_Open_MovesPendingOnly | Expected the last render to show the shortened pending row; it was unchanged. |
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorKeyHomeEnd_Open_MovePendingToFirstAndLastVisibleEntry | Expected the pending identity after End to be the last row; it stayed on the first. |
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorKeyPageDown_Open_MovesPendingByPageSize | Expected the last view's pending identity to move by the page size; it did not move. |
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorView_PendingNode_CarriesPendingNodeIdentity | Expected the last view to carry pendingNodeIdentity; it was absent. |
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorKeyEnter_Open_WithPendingNode_CommitsDescendantAndRaisesSelectionChanged | Expected the descendant path ending Alpha\Specs, but found the row path ending Clients\Beta. |
| Failed | BreadcrumbSelectorMessagesTests | SelectorKeyMessage_PageSize_RoundTrips | FormatException from Parse (page keys are not yet accepted). |
| Failed | BreadcrumbSelectorMessagesTests | ViewMessage_PendingNodeIdentity_RoundTrips | Expected parsed.PendingNodeIdentity to be "node-1", but found null. |
| Failed | BreadcrumbSelectorMessagesTests | SelectorKeyMessage_RoundTripsOnlySupportedKeys ("Parent") | FormatException from Parse. |
| Failed | BreadcrumbSelectorMessagesTests | SelectorKeyMessage_RoundTripsOnlySupportedKeys ("Home") | FormatException from Parse. |
| Failed | BreadcrumbSelectorMessagesTests | SelectorKeyMessage_RoundTripsOnlySupportedKeys ("End") | FormatException from Parse. |
| Failed | BreadcrumbSelectorMessagesTests | SelectorKeyMessage_RoundTripsOnlySupportedKeys ("PageUp") | FormatException from Parse. |
| Failed | BreadcrumbSelectorMessagesTests | SelectorKeyMessage_RoundTripsOnlySupportedKeys ("PageDown") | FormatException from Parse. |
| Failed (P10-T4) | BreadcrumbStateModelTests | LeftArrow_QfcMultiSegmentRow_SelectsParentNode | Expected handled to be False because the #440 parent walk is removed, but found True. |
| Failed (P10-T4) | BreadcrumbStateModelTests | RightArrow_QfcSelectedParentNode_ExpandsIntoChildren | Expected CollapsedAfterIndex to be 1 because the row stays shortened, but found null. |
| Failed (P10-T4) | BreadcrumbStateModelTests (Sequence partial) | Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges | Expected ActiveSegmentIndex to be 2, but found 1. |
| Failed (P10-T4) | BreadcrumbStateModelTests (Sequence partial) | RightArrow_OnCollapsedRow_ReExpandsBeforeLeafExpansion | Expected CollapsedAfterIndex to be 0, but found null. |
| Failed (P10-T4) | BreadcrumbStateModelTests (Sequence partial) | LeftArrow_RepeatedOnThreeSegmentChain_WalksToRootThenReportsUnhandled | Expected model.LeftArrow() to be False, but found True. |
| Failed (P10-T4) | BreadcrumbStateModelTests (Sequence partial) | LeftArrow_WalkFromAnOpenLeafExpansion_ClearsTheExpansionAndStillReachesTheRoot | Expected ActiveSegmentIndex to be 2, but found 1. |
| Passed (P10-T4, summary only) | BreadcrumbStateModelTests (Sequence partial) | LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses | - |
| Passed (P10-T4, summary only) | BreadcrumbSelectionSessionTests | BreadcrumbLeftAndRightTransitions_DoNotMutateSelectorSession | - |
| Failed (P10-T5) | FolderBreadcrumbBridgeRouterTests | Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft | InvalidCastException: a RenderMessage was returned where UnhandledArrowMessage was expected. |
| Failed (P10-T5) | FolderBreadcrumbBridgeRouterTests | ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition | Expected UnhandledArrowMessage, but found RenderMessage. |
| Failed (P10-T5) | FolderBreadcrumbBridgeRouterTests | ArrowAsync_QfcRightOnSelectedParentNode_RoutesChildExpansion | Expected Rows[0].CollapsedAfterIndex to be 1, but found null. |
