# Cycle 2 P4-T5: N-5 test dispositions

Timestamp: 2026-10-22T14-02
Command: git grep -l -F -e <token> -- '*Test*' for data-child-index, class="child", renderedChildActivate, subfolderResult, BridgeJs; git diff HEAD --stat -- UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs; git diff 8e4ad1ecbf50aae00fe2c092c22ae457f4feb7a7 -- the same path
EXIT_CODE: 0
Output Summary:
Search hit lists (test files):
- data-child-index: BreadcrumbHtmlRendererIssue1002Tests.cs, BreadcrumbHtmlRendererTests.cs
- class="child": no hit as a fixed string in test files
- renderedChildActivate: BreadcrumbBridgeRouterIssue439Tests.Activation.cs, BreadcrumbBridgeRouterIssue439Tests.cs, BreadcrumbBridgeRouterIssue614Tests.cs, BreadcrumbBridgeRouterIssue995Tests.cs, BreadcrumbHtmlRendererIssue1002Tests.cs, BreadcrumbHtmlRendererTests.cs, BreadcrumbMessageCodecTests.cs
- subfolderResult: BreadcrumbBridgeRouterIssue1002Tests.cs, BreadcrumbBridgeRouterQueueTests.cs, BreadcrumbBridgeRouterStaleCompletionIssue1002Tests.cs, BreadcrumbBridgeRouterTests.cs, BreadcrumbHtmlRendererIssue1002Tests.cs, BreadcrumbMessageCodecTests.cs
- BridgeJs: BreadcrumbHtmlRendererIssue1002Tests.cs, BreadcrumbHtmlRendererTests.cs

Dispositions (all unchanged because unaffected; the scoped run of both renderer classes passes 29 of 29):
- BreadcrumbHtmlRendererIssue1002Tests.RenderRowFragment_KeylessChildren_RenderUnchanged: unchanged; the renderer is not edited, the key-less server render stays as before (AC9).
- BreadcrumbHtmlRendererTests.Issue439ActiveAncestorChildrenAndEmbeddedBridgeUseTypedStoppedActivation: unchanged; the renderer prefix and the renderedChildActivate branch are not touched.
- BreadcrumbHtmlRendererTests.RenderRowFragment_ExpandedLeaf_RendersEncodedChildren: unchanged; renderer not edited.
- Keyed-child, BridgeJs ordering, BaseCss and theme tests in BreadcrumbHtmlRendererIssue1002Tests.cs: unchanged; they index on .child[data-node-id], .child[data-child-index], type: 'renderedChildActivate' and CSS rules, none of which change.
- QuickFiler.Test router and codec tests (subfolderResult hits): unchanged; they assert the subfolderResult JSON message, not the page script.
- Node suites under QuickFiler.Test/Viewers: unchanged; they assert FolderBreadcrumb.js, not BridgeJs.
- The only test reading the subfolderResult handler text is the new BridgeJs_SubfolderResultHandler_WrapsChildNameInNameSpan.

Renderer unedited: git diff HEAD --stat for UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs is empty (no edit in this cycle). Note: the diff against the merge base is non-empty because earlier committed branch work (cycles 0 and 1) already changed this file; the cycle-2 check is therefore made against HEAD.
