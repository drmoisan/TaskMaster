# Group A Pass-After: EFC Descendant Tree (#1002)

Timestamp: 2026-10-22T10-08
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~Breadcrumb" /ResultsDirectory:SCRATCH/results-issue-1002-p3t10 /Logger:"trx;LogFileName=issue-1002-p3t10.trx"
EXIT_CODE: 0
Output Summary:
- Build: `0 Error(s)`, `0 Warning(s)`.
- Test counts (derived from the trx): total 905, executed 905, passed 905, failed 0. The filter covers the five new Group A classes and every existing breadcrumb suite in both assemblies (including BreadcrumbHtmlRendererTests, BreadcrumbMessageCodecTests, BreadcrumbRowStateTests, BreadcrumbBridgeRouterTests and its partials, BreadcrumbBridgeRouterQueueTests, BreadcrumbBridgeRouterIssue439Tests, BreadcrumbBridgeRouterIssue614Tests, BreadcrumbBridgeRouterIssue995Tests).
- All 59 results of the five new Group A classes passed.
- Intermediate run: the first pass-after run reported 904 passed and 1 failed: `DescendantActivate_Grandchild_CommitsArchiveRelativeStem` asserted `data-depth="2"` for the grandchild, but the renderer numbers the first keyed level (Q3) as depth 0, so Invoices under Q3 is depth 1. The test expectation was corrected to depth 1 (a test-authoring error; the production depth numbering matches the plan, which places the first level at the row's children container). The rerun above is the recorded result.

## Group A fail-before set: pass-after outcomes

| Outcome | Test |
|---|---|
| Passed | DeserializeInbound_DescendantToggle_ReturnsNodeId |
| Passed | DeserializeInbound_DescendantActivate_ReturnsNodeId |
| Passed | SetLeafDescendants_KeyedChildren_BuildsOneNodePerChild |
| Passed | TryBeginDescendantExpand_KnownNodeWithChildren_ReturnsKeyAndGeneration |
| Passed | RenderRowFragment_KeyedChildWithChildren_RendersNodeAffordanceAfterName |
| Passed | BridgeJs_NodeAffordanceBranch_PrecedesChildAndLeafBranches |
| Passed | BaseCss_Child_IsFlexRow |
| Passed | DescendantToggle_SecondLevelNode_FetchesWithNodeKeyAndRendersChildren |
| Passed | DescendantActivate_Grandchild_CommitsArchiveRelativeStem |
| Passed | ExpandLeaf_KeyedChildren_RenderNodeAffordances |
| Passed | ExpandLeaf_CompletesAfterRebind_PostsNothingForReplacedRow |

## Other task-named tests

| Outcome | Test |
|---|---|
| Passed | Issue995BridgeJsPostsConfirmationForPlainEnterAndScrollsSelectedRowIntoView |
| Passed | DescendantActivate_OutsideArchiveRoot_LeavesSelectionAndHighlightUnchanged |
| Passed | ExpandLeaf_CurrentRow_StillPostsSubfolderResultThenRender |
| Passed | the five DescendantFetch_ tests (CompletesAfterCollapse, CompletesAfterRebind, ReverseCompletion, ProviderFault, Canceled) |
| Passed | the six RenderRowFragment_ tests, the three BridgeJs_ tests, all twenty-three BreadcrumbRowDescendantsIssue1002Tests, all eleven BreadcrumbBridgeRouterIssue1002Tests, all five codec test methods (eight data rows) |

`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` is 465 physical lines after the two case arms (at most 500).
