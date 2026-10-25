# EFC Horizontal-Key and Double-Click Pass-After (Issue #1007, P9-T9)

Timestamp: 2026-10-23T14-24
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; then vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~Breadcrumb|FullyQualifiedName~OutlookFolderHierarchyProvider" (results under SCRATCH)
EXIT_CODE: 0
Output Summary: build `0 Error(s)`, `0 Warning(s)`. BREADCRUMB-FILTER run: total 1004, executed 1004, passed 1004, failed 0. Every G4 test (20) and every Phase 8 edited test passed; the unchanged `BreadcrumbRowDescendantsIssue1002Tests`, `BreadcrumbHtmlRendererTests` and `DescendantToggle_DoesNotChangeSelection` passed in the same run. 34 outcome rows below (20 G4, 11 Phase 8 edited tests, 3 unchanged spot checks).

| Outcome | Class | Test |
|---|---|---|
| Passed | BreadcrumbBridgeRouterTests | ArrowKeyRight_WhenCollapsed_ReExpandsWithoutProviderQuery |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ArrowLeft_OnCollapsedNode_IsNoOp |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ArrowLeft_OnExpandedNode_CollapsesOnlyThatNode |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ArrowLeft_OnExpandedRow_CollapsesAndKeepsSelection |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ArrowLeft_OnNonExpandedRow_IsNoOp |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ArrowRight_OnExpandedNode_SelectsFirstChildNode |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ArrowRight_OnExpandedNodeWithFetchInFlight_DoesNothing |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ArrowRight_OnExpandedRow_SelectsFirstDescendantWithNodeId |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ArrowRight_OnLeaf_PostsNothing |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ArrowRight_OnShortenedRow_ListsSegmentChildrenWithSingleQuery |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ArrowRight_OnUnexpandedNodeWithChildren_ExpandsNode |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ArrowRight_OnUnexpandedRowWithChildren_Expands |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ArrowRight_ReentrantDuringFetch_EndsExpandedWithOneQuery |
| Passed | BreadcrumbBridgeRouterTests | Boundary_EfcLeftAtRootAndRightOnChildlessNode_RemainSilentNoOps |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | CollapseHidingSelection_RecommitsNearestVisibleAncestor |
| Passed | BreadcrumbBridgeRouterIssue1002Tests | DescendantToggle_DoesNotChangeSelection |
| Passed | BreadcrumbBridgeRouterTests | HandleArrowKey_LeftOnMultiSegmentRow_ActivatesParentSegment |
| Passed | BreadcrumbBridgeRouterTests | HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior |
| Passed | BreadcrumbBridgeRouterTests | HandleArrowKey_RightAfterExpansion_DescendsByChildActivation |
| Passed | BreadcrumbBridgeRouterTests | HandleArrowKey_RightOnActivatedParent_ExpandsViaSingleImmediateSubfolderCall |
| Passed | BreadcrumbBridgeRouterTests | LeafExpandToggle_OnCollapsedRow_ReExpandsWithoutProviderQuery |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | LeafExpandToggle_OnShortenedRow_ListsSegmentChildren |
| Passed | BreadcrumbRowStateTests | LeftArrow_NeverHidesTrailingSegment |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ParentLeft_AtFirstSegment_IsNoOp |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ParentLeft_OnNode_SelectsParentKeepingExpansion |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ParentLeft_OnRow_ActivatesParentSegmentCommitsAndShortens |
| Passed | BreadcrumbRowStateTests | RightArrow_WhenCollapsed_RestoresFullBreadcrumb |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | SegmentDoubleClick_ActivatesCommitsAndShortensAtomically |
| Passed | BreadcrumbBridgeRouterQueueTests | SegmentDoubleClick_BannerRow_ViaHostEvent_ShortCircuitsBeforeRangeCheck |
| Passed | BreadcrumbBridgeRouterTests | SegmentDoubleClick_OnNonLeafSegment_CollapsesAndReRenders |
| Passed | BreadcrumbBridgeRouterQueueTests | SegmentDoubleClick_ValidIndex_ViaHostEvent_CollapsesRowAndPostsRender |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ShortenedRowExpanded_NextRight_SelectsFirstChild |
| Passed | BreadcrumbBridgeRouterIssue1007Tests | ShortenedRowExpanded_RendersCollapseAffordanceAtTerminal |
| Passed | BreadcrumbRowStateTests | ToggleLeafExpanded_WhileCollapsed_IsNoOp |
