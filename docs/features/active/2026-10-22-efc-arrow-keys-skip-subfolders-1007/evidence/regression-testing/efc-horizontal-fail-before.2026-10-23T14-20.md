# EFC Horizontal-Key and Double-Click Fail-Before (Issue #1007, P8-T7)

Timestamp: 2026-10-23T14-20
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; then vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterIssue1007Tests|FullyQualifiedName~QuickFiler.Test.Controllers.BreadcrumbBridgeRouterTests|FullyQualifiedName~BreadcrumbRowStateTests|FullyQualifiedName~BreadcrumbBridgeRouterQueueTests" (results under SCRATCH)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: build `0 Error(s)`. Tests: total 106, executed 106, passed 80, failed 26. Every G4 non-guard test, `LeftArrow_NeverHidesTrailingSegment` and both rewritten `.Selection.cs` tests failed on their assertions; the four guards passed. Summary derived from the trx.

Run note: the first attempt of this run did not finish within 10 minutes. `ArrowRight_ReentrantDuringFetch_EndsExpandedWithOneQuery` awaited the second Right before releasing the held fetch, and on the pre-fix code that second Right also awaits the held fetch, so the test could not complete. The test host tree of that run (started from this worktree) was stopped, both held-fetch tests were changed to release the fetch before awaiting any key task, and the run above is the re-run. The scoped launcher now stops a run that exceeds 300 seconds.

| Outcome | Test | Failure message (from the trx, shortened) |
|---|---|---|
| Failed | ArrowRight_OnExpandedRow_SelectsFirstDescendantWithNodeId | Expected "Clients\North\Q3", but "Clients\North". |
| Failed | ArrowRight_OnUnexpandedNodeWithChildren_ExpandsNode | MockException: expected the Q3 subfolder query once, but was 0 times. |
| Failed | ArrowRight_OnExpandedNode_SelectsFirstChildNode | Expected "Clients\North\Q3\Invoices", but "Clients\North\Q3". |
| Failed | ArrowRight_ReentrantDuringFetch_EndsExpandedWithOneQuery | MockException: expected the North subfolder query once, but was 2 times. |
| Failed | ArrowLeft_OnExpandedRow_CollapsesAndKeepsSelection | Expected the latest render to contain ">Q3</span>" after Right (the #440 walk had moved the active segment). |
| Failed | ArrowLeft_OnExpandedNode_CollapsesOnlyThatNode | Expected the latest render to contain ">Q4</span>" (the #440 walk collapsed the row). |
| Failed | ArrowLeft_OnNonExpandedRow_IsNoOp | Expected Posts.Count to be 1, but found 2. |
| Failed | ArrowLeft_OnCollapsedNode_IsNoOp | Expected Posts.Count to be 3, but found 4. |
| Failed | CollapseHidingSelection_RecommitsNearestVisibleAncestor | Expected "Clients\North\Q3", but "Clients\North\Q3\Invoices". |
| Failed | ParentLeft_OnRow_ActivatesParentSegmentCommitsAndShortens | Expected "Clients", but "Clients\North". |
| Failed | ParentLeft_OnNode_SelectsParentKeepingExpansion | Expected "Clients\North\Q3", but "Clients\North\Q3\Invoices". |
| Passed (guard) | ArrowRight_OnUnexpandedRowWithChildren_Expands | - |
| Passed (guard) | ArrowRight_OnExpandedNodeWithFetchInFlight_DoesNothing | - |
| Passed (guard) | ArrowRight_OnLeaf_PostsNothing | - |
| Passed (guard) | ParentLeft_AtFirstSegment_IsNoOp | - |
| Failed | SegmentDoubleClick_ActivatesCommitsAndShortensAtomically | Expected "Clients\Acme", but "Clients\Acme\Beta". |
| Failed | LeafExpandToggle_OnShortenedRow_ListsSegmentChildren | MockException: expected the Acme subfolder query once, but was 0 times. |
| Failed | ArrowRight_OnShortenedRow_ListsSegmentChildrenWithSingleQuery | MockException: expected the Acme subfolder query once, but was 0 times. |
| Failed | ShortenedRowExpanded_NextRight_SelectsFirstChild | Expected "Clients\Acme", but found null. |
| Failed | ShortenedRowExpanded_RendersCollapseAffordanceAtTerminal | Expected the latest render to contain the reexpand collapse affordance (minus). |
| Failed | LeftArrow_NeverHidesTrailingSegment (P8-T5) | Expected row.LeftArrow() to be False, but found True. |
| Failed | LeafExpandToggle_OnCollapsedRow_ReExpandsWithoutProviderQuery (P8-T4) | MockException: expected the Inbox subfolder query once, but was 0 times. |
| Failed | ArrowKeyRight_WhenCollapsed_ReExpandsWithoutProviderQuery (P8-T4) | MockException: expected the Inbox subfolder query once, but was 0 times. |
| Failed | HandleArrowKey_LeftOnMultiSegmentRow_ActivatesParentSegment (P8-T3) | Expected _posted.Count to be 0 because Left on a non-expanded row is a no-op, but found 1. |
| Failed | HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior (P8-T3) | Expected no row-1 render, but found row-scoped renders. |
| Failed | HandleArrowKey_RightOnActivatedParent_ExpandsViaSingleImmediateSubfolderCall (P8-T3) | MockException: expected the Inbox\Projects query once, but was 0 times. |
| Failed | HandleArrowKey_RightAfterExpansion_DescendsByChildActivation (P8-T3) | Expected the last post to contain aria-selected (no node was highlighted). |
| Failed | Boundary_EfcLeftAtRootAndRightOnChildlessNode_RemainSilentNoOps (P8-T3) | Expected _posted.Count to be 0, but found 4. |
| Passed (P8-T3) | SegmentDoubleClick_OnNonLeafSegment_CollapsesAndReRenders | - |
| Failed | RightArrow_WhenCollapsed_RestoresFullBreadcrumb (P8-T5) | Expected anchored.IsLeafExpanded to be True, but found False. |
| Failed | ToggleLeafExpanded_WhileCollapsed_IsNoOp (P8-T5) | Expected anchored.ToggleLeafExpanded() to be True, but found False. |
| Passed (P8-T6) | SegmentDoubleClick_ValidIndex_ViaHostEvent_CollapsesRowAndPostsRender | - |
