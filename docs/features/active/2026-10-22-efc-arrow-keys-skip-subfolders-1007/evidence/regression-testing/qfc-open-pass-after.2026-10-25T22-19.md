# QFC Open-List Pass-After (Issue #1007, P11-T7)

Timestamp: 2026-10-25T22-19
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; then vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~Breadcrumb|FullyQualifiedName~OutlookFolderHierarchyProvider" (results under SCRATCH)
EXIT_CODE: 0
Output Summary: build `0 Error(s)`, `0 Warning(s)`. BREADCRUMB-FILTER run: total 1039, executed 1039, passed 1039, failed 0. Every G5 test (13 session, 8 router, 7 coordinator, 2 serializer tests and the 5 new key rows) and every Phase 10 edited test passed. BASELINE-FAILURES is empty, so no failure is excused. Summary derived from the trx.

Run note: the first run of this pass (same build command) had 4 failures, all in the unchanged `BreadcrumbSelectorCoordinatorTests` (`BoundaryAndInvalidOperations_AreDeterministicNoOps`, `ClosedDown_CommitsNextSelectableAndRaisesOneSelection`, `EscapeAndUncommittedClose_RestoreOpeningSelectionWithoutNotification`, `OpenDown_ChangesPendingOnlyThenEnterCommitsAndCloses`) with `TargetParameterCountException`. That class binds `HandleSelectorKey` by name through reflection with one argument, so the optional `pageSize` parameter named in R8 broke the existing public signature. The fix keeps the public one-argument `HandleSelectorKey(BreadcrumbSelectorKey)` unchanged and carries the page size through a private `HandleSelectorKeyCore(key, pageSize)` that `HandleSelectorMessage` calls with `key.PageSize`. The R8 behaviour (page keys move by the page size carried by the `selectorKey` message) is unchanged; only the signature placement differs from the R8 wording. The run above is the re-run after CSharpier and the same rebuild.

Implementation notes for P11-T1 to P11-T6:
- P11-T1: `ParseKey` accepts every lowercase key name (the spelling `Serialize` writes); `pageSize` is read as an optional non-negative integer and written when present; `pendingNodeIdentity` is read and written on `selectorView`, and a blank value is rejected by the constructor guard. `BreadcrumbSelectorMessages.cs` is 445 physical lines.
- P11-T2: the #440 walks and `TryRightTreeTransition` are removed; Right applies R3 through `BreadcrumbStateModel.Expand`; `ActiveSegmentHasSubfolders` accepts `CollapsedAfterIndex == ActiveSegmentIndex`. `BreadcrumbStateModel.Row.cs` is 411 physical lines (408 at P0-T8, +3).
- P11-T3: `BreadcrumbSelectionSession.cs` gained call-throughs only (open snapshot, open-list `Move`, `CommitPending` node commit, `Cancel` shape restore, `EndOpenSession` clear, `Snapshot` pending node, and a `PendingIdentity` setter that clears the pending node); 492 physical lines.
- P11-T6: `BreadcrumbBridgeCoordinator.cs` is 435 physical lines; `BreadcrumbBridgeCoordinator.Keyboard.cs` is registered in `QuickFiler/QuickFiler.csproj`.

| Outcome | Class | Test |
|---|---|---|
| Passed | BreadcrumbSelectionSessionTreeIssue1007Tests | SelectorKeyDown_Open_MovesPendingThroughVisibleDescendants |
| Passed | BreadcrumbSelectionSessionTreeIssue1007Tests | SelectorKeyUp_Open_FromRowBelowExpandedBlock_PendsLastVisibleDescendant |
| Passed | BreadcrumbSelectionSessionTreeIssue1007Tests | CommitPending_WithPendingNode_SelectsDescendant |
| Passed | BreadcrumbSelectionSessionTreeIssue1007Tests | Cancel_RestoresOriginalAndClearsPendingNode |
| Passed | BreadcrumbSelectionSessionTreeIssue1007Tests | MoveToBoundary_Open_PendsFirstAndLastVisibleEntry |
| Passed | BreadcrumbSelectionSessionTreeIssue1007Tests | MovePage_Open_MovesByPageSizeAndClamps |
| Passed | BreadcrumbSelectionSessionTreeIssue1007Tests | MovePage_InvalidPageSize_IsNoOp |
| Passed | BreadcrumbSelectionSessionTreeIssue1007Tests | MoveToParent_Open_OnNode_PendsParentKeepingExpansion |
| Passed | BreadcrumbSelectionSessionTreeIssue1007Tests | MoveToParent_Open_OnRow_ShortensPendingRowAtParentSegment |
| Passed | BreadcrumbSelectionSessionTreeIssue1007Tests | MoveToParent_AtFirstSegment_IsNoOp |
| Passed | BreadcrumbSelectionSessionTreeIssue1007Tests | SelectorKeyParent_Open_ThenEscape_RestoresRowShapeAndValue |
| Passed (guard) | BreadcrumbSelectionSessionTreeIssue1007Tests | SelectorKeyDown_Open_OnLastEntry_KeepsPending |
| Passed (guard) | BreadcrumbSelectionSessionTreeIssue1007Tests | OpenMove_DoesNotChangeCommittedValue |
| Passed | FolderBreadcrumbBridgeRouterIssue1007Tests | OpenRight_OnPendingRowWithChildren_ExpandsPendingRow |
| Passed | FolderBreadcrumbBridgeRouterIssue1007Tests | OpenRight_OnExpandedPendingEntry_PendsFirstChild |
| Passed | FolderBreadcrumbBridgeRouterIssue1007Tests | OpenLeft_OnExpandedPendingEntry_CollapsesAndKeepsPending |
| Passed | FolderBreadcrumbBridgeRouterIssue1007Tests | CollapseHidingPending_MovesPendingToNearestVisibleAncestor |
| Passed | FolderBreadcrumbBridgeRouterIssue1007Tests | ClassifyPendingArrow_OpenLeftOnNonExpanded_ReturnsCommitAndClose |
| Passed | FolderBreadcrumbBridgeRouterIssue1007Tests | ClassifyPendingArrow_OpenRightOnLeaf_ReturnsCancelAndPopOut |
| Passed | FolderBreadcrumbBridgeRouterIssue1007Tests | ArrowRight_OnShortenedPendingRow_ListsSegmentChildren |
| Passed | FolderBreadcrumbBridgeRouterIssue1007Tests | GetSelectedFolder_ShortenedRow_ReturnsActiveSegmentValue |
| Passed | BreadcrumbBridgeCoordinatorIssue1007Tests | OpenLeft_OnNonExpandedEntry_CommitsAndCloses |
| Passed | BreadcrumbBridgeCoordinatorIssue1007Tests | OpenRight_OnLeaf_CancelsThenRaisesUnhandledArrowRight |
| Passed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorKeyParent_Open_MovesPendingOnly |
| Passed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorKeyHomeEnd_Open_MovePendingToFirstAndLastVisibleEntry |
| Passed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorKeyPageDown_Open_MovesPendingByPageSize |
| Passed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorView_PendingNode_CarriesPendingNodeIdentity |
| Passed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorKeyEnter_Open_WithPendingNode_CommitsDescendantAndRaisesSelectionChanged |
| Passed | BreadcrumbSelectorMessagesTests | SelectorKeyMessage_PageSize_RoundTrips |
| Passed | BreadcrumbSelectorMessagesTests | ViewMessage_PendingNodeIdentity_RoundTrips |
| Passed | BreadcrumbSelectorMessagesTests | SelectorKeyMessage_RoundTripsOnlySupportedKeys ("Parent") |
| Passed | BreadcrumbSelectorMessagesTests | SelectorKeyMessage_RoundTripsOnlySupportedKeys ("Home") |
| Passed | BreadcrumbSelectorMessagesTests | SelectorKeyMessage_RoundTripsOnlySupportedKeys ("End") |
| Passed | BreadcrumbSelectorMessagesTests | SelectorKeyMessage_RoundTripsOnlySupportedKeys ("PageUp") |
| Passed | BreadcrumbSelectorMessagesTests | SelectorKeyMessage_RoundTripsOnlySupportedKeys ("PageDown") |
| Passed | BreadcrumbSelectorMessagesTests | SelectorKeyMessage_RoundTripsOnlySupportedKeys ("Up", "Down", "Enter", "Escape"; unchanged rows) |
| Passed (P10-T4) | BreadcrumbStateModelTests | LeftArrow_QfcMultiSegmentRow_SelectsParentNode |
| Passed (P10-T4) | BreadcrumbStateModelTests | RightArrow_QfcSelectedParentNode_ExpandsIntoChildren |
| Passed (P10-T4) | BreadcrumbStateModelTests (Sequence partial) | Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges |
| Passed (P10-T4) | BreadcrumbStateModelTests (Sequence partial) | RightArrow_OnCollapsedRow_ReExpandsBeforeLeafExpansion |
| Passed (P10-T4) | BreadcrumbStateModelTests (Sequence partial) | LeftArrow_RepeatedOnThreeSegmentChain_WalksToRootThenReportsUnhandled |
| Passed (P10-T4) | BreadcrumbStateModelTests (Sequence partial) | LeftArrow_WalkFromAnOpenLeafExpansion_ClearsTheExpansionAndStillReachesTheRoot |
| Passed (P10-T4) | BreadcrumbStateModelTests (Sequence partial) | LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses |
| Passed (P10-T4) | BreadcrumbSelectionSessionTests | BreadcrumbLeftAndRightTransitions_DoNotMutateSelectorSession |
| Passed (P10-T5) | FolderBreadcrumbBridgeRouterTests | Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft |
| Passed (P10-T5) | FolderBreadcrumbBridgeRouterTests | ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition |
| Passed (P10-T5) | FolderBreadcrumbBridgeRouterTests | ArrowAsync_QfcRightOnSelectedParentNode_RoutesChildExpansion |
| Passed (unaffected, D-Q29) | BreadcrumbSubfolderActivationTests | OpenSelector_SubfolderActivation_PublishesOneDurableRenderAndNoLegacySelectionChange |
| Passed (unaffected, reflective binding) | BreadcrumbSelectorCoordinatorTests | BoundaryAndInvalidOperations_AreDeterministicNoOps |
| Passed (unaffected, reflective binding) | BreadcrumbSelectorCoordinatorTests | OpenDown_ChangesPendingOnlyThenEnterCommitsAndCloses |
