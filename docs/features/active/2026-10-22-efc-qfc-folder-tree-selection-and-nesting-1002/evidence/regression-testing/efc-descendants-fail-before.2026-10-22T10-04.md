# Group A Fail-Before: EFC Descendant Tree (#1002)

Timestamp: 2026-10-22T10-04
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbMessageCodecIssue1002Tests|FullyQualifiedName~BreadcrumbRowDescendantsIssue1002Tests|FullyQualifiedName~BreadcrumbHtmlRendererIssue1002Tests|FullyQualifiedName~BreadcrumbBridgeRouterIssue1002Tests|FullyQualifiedName~BreadcrumbBridgeRouterStaleCompletionIssue1002Tests" /ResultsDirectory:SCRATCH/results-issue-1002-p2t6 /Logger:"trx;LogFileName=issue-1002-p2t6.trx"
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- Build: `0 Error(s)`, `0 Warning(s)` (first attempt failed with CS0104, an ambiguous `Match` between Moq and System.Text.RegularExpressions in the new router test file; fixed by full qualification and rebuilt).
- Test counts (derived from the trx): total 59, executed 59, passed 3, failed 56. Data rows count separately (the three malformed-nodeId codec tests carry two rows each).
- Every test in the Group A fail-before set is reported Failed (marked FB below).
- Deviation: the new QuickFiler.Test files carry no `#nullable enable`, because QuickFiler.Test compiles as C# 7.3 (no `LangVersion`), where the directive is a compile error; no QuickFiler.Test file carries it ("consistent with their neighbors", spec Constraints). The UtilitiesCS.Test files carry it.

## Outcomes (one row per test result)

| Outcome | Test | FB | Message (truncated) |
|---|---|---|---|
| Failed | DeserializeInbound_DescendantToggle_ReturnsNodeId | FB | threw BreadcrumbMessageException: Unknown inbound breadcrumb message type |
| Failed | DeserializeInbound_DescendantActivate_ReturnsNodeId | FB | threw BreadcrumbMessageException: Unknown inbound breadcrumb message type |
| Failed | DeserializeInbound_DescendantMessageWithoutNodeId_Throws (descendantToggle) |  | message "Unknown inbound breadcrumb message type 'descendantToggle'." does not match *nodeId* |
| Failed | DeserializeInbound_DescendantMessageWithoutNodeId_Throws (descendantActivate) |  | same, descendantActivate |
| Failed | DeserializeInbound_DescendantMessageWithEmptyNodeId_Throws (descendantToggle) |  | same pattern |
| Failed | DeserializeInbound_DescendantMessageWithEmptyNodeId_Throws (descendantActivate) |  | same pattern |
| Failed | DeserializeInbound_DescendantMessageWithNonStringNodeId_Throws (descendantToggle) |  | same pattern |
| Failed | DeserializeInbound_DescendantMessageWithNonStringNodeId_Throws (descendantActivate) |  | same pattern |
| Failed | SetLeafDescendants_KeyedChildren_BuildsOneNodePerChild | FB | Expected stored to be True because the active leaf has subfolders, but found False. |
| Failed | SetLeafDescendants_DuplicateDisplayNames_KeepsDistinctIdentities |  | Expected nodes to contain 2 item(s), but found 0 |
| Passed | SetLeafDescendants_RowWithoutExpandableLeaf_ReturnsFalse |  |  |
| Failed | TryBeginDescendantExpand_KnownNodeWithChildren_ReturnsKeyAndGeneration | FB | fixture: SetLeafDescendants returned False |
| Failed | TryBeginDescendantExpand_UnknownNode_ReturnsFalse |  | fixture: SetLeafDescendants returned False |
| Failed | TryBeginDescendantExpand_LeafNode_ReturnsFalse |  | fixture: SetLeafDescendants returned False |
| Failed | TryCollapseDescendant_ExpandedNode_CollapsesIt |  | fixture: SetLeafDescendants returned False |
| Failed | TryCollapseDescendant_CollapsedNode_ReturnsFalse |  | fixture: SetLeafDescendants returned False |
| Failed | TryAttachDescendantChildren_MatchingGeneration_AttachesChildren |  | fixture: SetLeafDescendants returned False |
| Failed | TryAttachDescendantChildren_SupersededGeneration_IsRejected |  | fixture: SetLeafDescendants returned False |
| Failed | TryAttachDescendantChildren_CollapsedNode_IsRejected |  | fixture: SetLeafDescendants returned False |
| Failed | TryAttachDescendantChildren_RemovedNode_IsRejected |  | fixture: SetLeafDescendants returned False |
| Failed | TryAbandonDescendantExpand_CurrentGeneration_CollapsesNode |  | fixture: SetLeafDescendants returned False |
| Failed | TryAbandonDescendantExpand_SupersededGeneration_LeavesNodeUnchanged |  | fixture: SetLeafDescendants returned False |
| Failed | GetDescendantPath_KnownNode_ReturnsFolderPath |  | fixture: SetLeafDescendants returned False |
| Failed | GetDescendantPath_UnknownNode_ReturnsNull |  | fixture: SetLeafDescendants returned False |
| Failed | TrySelectDescendant_KnownNode_SetsSelectedDescendantId |  | fixture: SetLeafDescendants returned False |
| Failed | TrySelectDescendant_UnknownNode_LeavesSelectionUnchanged |  | fixture: SetLeafDescendants returned False |
| Failed | ActivateSegment_ClearsDescendantsAndSelection |  | fixture: SetLeafDescendants returned False |
| Failed | CollapseAfter_ClearsDescendantsAndSelection |  | fixture: SetLeafDescendants returned False |
| Failed | LeftArrow_ExpandedLeaf_ClearsDescendantsAndSelection |  | fixture: SetLeafDescendants returned False |
| Failed | ToggleLeafExpanded_Collapse_ClearsDescendantsAndSelection |  | fixture: SetLeafDescendants returned False |
| Failed | SetLeafChildren_KeylessChildren_HasNoDescendants |  | fixture: SetLeafDescendants returned False |
| Failed | RenderRowFragment_KeyedChildWithChildren_RendersNodeAffordanceAfterName | FB | fixture: SetLeafDescendants returned False |
| Failed | RenderRowFragment_KeyedLeafChild_RendersNoAffordance |  | fixture: SetLeafDescendants returned False |
| Failed | RenderRowFragment_ExpandedDescendant_NestsChildrenContainerAfterNode |  | fixture: SetLeafDescendants returned False |
| Failed | RenderRowFragment_SelectedDescendant_MarksAriaSelected |  | fixture: SetLeafDescendants returned False |
| Failed | RenderRowFragment_DescendantNames_AreHtmlEncoded |  | SetLeafDescendants returned False |
| Passed | RenderRowFragment_KeylessChildren_RenderUnchanged |  |  |
| Failed | BridgeJs_NodeAffordanceBranch_PrecedesChildAndLeafBranches | FB | Expected nodeBranch to be greater than -1 because the node affordance branch exists, but found -1. |
| Failed | BridgeJs_DescendantActivateBranch_PrecedesKeylessChildBranch |  | Expected keyedChild to be greater than -1, but found -1. |
| Failed | BridgeJs_NodeAffordanceEnterOrSpace_PrecedesRowConfirmation |  | Expected space to be greater than -1, but found -1. |
| Failed | BaseCss_Child_IsFlexRow | FB | rule ".child { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }" does not contain "display: flex" |
| Failed | DescendantToggle_SecondLevelNode_FetchesWithNodeKeyAndRendersChildren | FB | the latest render must carry a node for the Q3 path, but match.Success was False |
| Failed | DescendantToggle_ExpandedNode_CollapsesWithoutProviderCall |  | latest render carries no node for Q3 |
| Failed | DescendantToggle_DoesNotChangeSelection |  | latest render carries no node for Q3 |
| Failed | DescendantToggle_PostsNoSubfolderResult |  | latest render carries no node for Q3 |
| Failed | DescendantActivate_Grandchild_CommitsArchiveRelativeStem | FB | latest render carries no node for Q3 |
| Failed | DescendantActivate_ThenRowConfirmed_RaisesConfirmationOnce |  | latest render carries no node for Q3 |
| Failed | DescendantActivate_OutsideArchiveRoot_LeavesSelectionAndHighlightUnchanged |  | latest render carries no node for Q3 |
| Failed | DescendantRequest_UnknownNode_IsRejectedWithoutStateChange |  | latest render carries no node for Q3 |
| Failed | DescendantToggle_LeafNode_IsRejectedWithoutProviderCall |  | latest render carries no node for Q4 |
| Failed | DescendantRequest_NodeFromAnotherRow_IsRejected |  | latest render carries no node for Q3 |
| Failed | ExpandLeaf_KeyedChildren_RenderNodeAffordances | FB | Expected Regex.Matches(html, "data-role=\"node\"").Count to be 1, but found 0 |
| Failed | DescendantFetch_CompletesAfterCollapse_AttachesNothing |  | latest render carries no node for Q3 |
| Failed | DescendantFetch_CompletesAfterRebind_PostsNothingForReplacedRow |  | latest render carries no node for Q3 |
| Failed | DescendantFetch_ReverseCompletion_OnlyLatestGenerationApplies |  | latest render carries no node for Q3 |
| Failed | DescendantFetch_ProviderFault_LeavesStateUnchangedAndAllowsRetry |  | latest render carries no node for Q3 |
| Failed | DescendantFetch_Canceled_LeavesStateUnchanged |  | latest render carries no node for Q3 |
| Failed | ExpandLeaf_CompletesAfterRebind_PostsNothingForReplacedRow | FB | Expected fixture.Posts.Count to be 0 because the replaced row receives no post, but found 2 |
| Passed | ExpandLeaf_CurrentRow_StillPostsSubfolderResultThenRender |  |  |

Group A fail-before set (11): all Failed.
