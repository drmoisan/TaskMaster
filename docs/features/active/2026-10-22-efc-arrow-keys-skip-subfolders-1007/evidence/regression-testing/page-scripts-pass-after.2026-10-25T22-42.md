# Page-Script Pass-After (Issue #1007, P17-T3)

Timestamp: 2026-10-25T22-42
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~Breadcrumb|FullyQualifiedName~OutlookFolderHierarchyProvider" (results under SCRATCH); then node --test --test-concurrency=1 --test-reporter=spec tests/quickfiler-breadcrumb/issue-997-common.test.mjs tests/quickfiler-breadcrumb/issue-997-recursive.test.mjs tests/quickfiler-breadcrumb/issue-1002-nesting.test.mjs tests/quickfiler-breadcrumb/issue-1007-keys.test.mjs
EXIT_CODE: 0
Output Summary: build `0 Error(s)`, `0 Warning(s)`. BREADCRUMB-FILTER run: total 1073, executed 1073, passed 1073, failed 0. Node run: tests 90, pass 90, fail 0. Every G8 test (8 QFC and 6 EFC C# contracts, 10 node tests) passed. The unchanged `FolderBreadcrumbNestingIssue1002Tests`, `FolderBreadcrumbRecursiveAssetIssue997Tests` and the `BridgeJs` test `Issue995BridgeJsPostsConfirmationForPlainEnterAndScrollsSelectedRowIntoView` also passed. `FolderBreadcrumb.js` is 393 physical lines (350 at P0-T8); `BreadcrumbDocumentAssets.cs` is 201. Summaries derived from the trx and the node spec output.

Run note: the first pass-after run (same commands) had one C# failure, the unchanged `FolderBreadcrumbRecursiveAssetIssue997Tests.RenderDescendantWithChildren_OffersIndependentExpansionControl`. It requires the literal `if (child.hasChildren)` guard ahead of `selectorDescendantToggle`, and the first P17-T2 draft had folded the AC14 check into that condition. The script now keeps `if (child.hasChildren)` unchanged and attaches the toggle only under `if (!loadedEmpty)`. The twin `MakeSubfolders_LoadedChildWithoutChildren_RendersNoToggle` (P16-T4) was adjusted to assert that guard; it still fails on the pre-fix script, because that script has no `childrenLoaded` check (page-scripts-fail-before.2026-10-25T22-38.md). The P17-T2 structural constraints hold: no function was added inside `makeSubfolders`, `subfolderElement.appendChild(name)` still precedes `subfolderElement.appendChild(toggle)`, and the recursive call `makeSubfolders(row, child.children, selectedChildOwnsActive, depth + 1)` is unchanged. `fullyVisibleCount`, `visiblePageSize` and `lastSegmentIndex` are declared outside the region from `function makeSubfolders(` to `function render`.

## C# string contracts (G8 and the P16-T6 updated tests)

| Outcome | Class | Test |
|---|---|---|
| Passed | FolderBreadcrumbKeysIssue1007Tests | SelectorKeys_MapHomeEndPageUpPageDown |
| Passed | FolderBreadcrumbKeysIssue1007Tests | CtrlArrowLeft_PostsParentSelectorKey |
| Passed | FolderBreadcrumbKeysIssue1007Tests | PageKeys_CarryPageSizeFromFullyVisibleCount |
| Passed | FolderBreadcrumbKeysIssue1007Tests | OnArrow_AlwaysPostsArrowKey |
| Passed | FolderBreadcrumbKeysIssue1007Tests | MakeSubfolders_MarksPendingNodeActive |
| Passed | FolderBreadcrumbKeysIssue1007Tests | NonLeafSegmentClick_OpenList_StopsPropagationAndPostsSelectorHighlight |
| Passed | FolderBreadcrumbKeysIssue1007Tests | SegmentDoubleClick_PostsSelectorSegmentDoubleClick |
| Passed | FolderBreadcrumbKeysIssue1007Tests | MakeSubfolders_LoadedChildWithoutChildren_RendersNoToggle |
| Passed | BreadcrumbBridgeJsKeysIssue1007Tests | BridgeJs_CtrlArrowLeft_PostsParentLeft |
| Passed | BreadcrumbBridgeJsKeysIssue1007Tests | BridgeJs_CtrlArrowRight_PostsRight |
| Passed | BreadcrumbBridgeJsKeysIssue1007Tests | BridgeJs_HomeEndPageKeys_MapToKeyValues |
| Passed | BreadcrumbBridgeJsKeysIssue1007Tests | BridgeJs_PageKeys_CarryMeasuredPageSize |
| Passed | BreadcrumbBridgeJsKeysIssue1007Tests | BridgeJs_Scroll_TargetsSelectedChildBeforeSelectedRow |
| Passed (guard) | BreadcrumbBridgeJsKeysIssue1007Tests | BridgeJs_NoSelection_PostsEmptyRowId |
| Passed (P16-T6) | FolderBreadcrumbAssetContractTests | SelectorKeys_PreventBrowserScrollingAndPostNativeKeyMessages |
| Passed (P16-T6) | FolderBreadcrumbAssetContractTests | RenderReceiver_OwnsSelectedChildExpandedAndCollapsedProjection |
| Passed (P16-T6) | FolderBreadcrumbAssetContractTests | LeftAndRightBreadcrumbMessages_RemainSupported |
| Passed (unchanged) | FolderBreadcrumbNestingIssue1002Tests | MakeSubfolders_AppendsNameBeforeToggle, MakeSubfolders_PinnedRecursiveCallPrefixPreserved, MakeSubfolders_RecursiveCallPassesDepthPlusOne, MakeSubfolders_SetsDataDepthAndPerDepthPadding |
| Passed (unchanged) | FolderBreadcrumbRecursiveAssetIssue997Tests | RenderDescendantWithChildren_OffersIndependentExpansionControl, ComposedDocument_ContainsExactlyOneInlineScript, ComposedDocument_InsertsCompleteCompiledScriptWithoutMarker |
| Passed (unchanged) | BreadcrumbHtmlRendererTests | Issue995BridgeJsPostsConfirmationForPlainEnterAndScrollsSelectedRowIntoView |

## Node suites

| Outcome | File | Test |
|---|---|---|
| Passed | issue-1007-keys.test.mjs | ctrl+ArrowLeft posts the parent selector key |
| Passed | issue-1007-keys.test.mjs | ctrl+ArrowRight posts arrowKey right |
| Passed | issue-1007-keys.test.mjs | Home and End post selector keys |
| Passed | issue-1007-keys.test.mjs | PageUp and PageDown post the fully visible row count as pageSize |
| Passed | issue-1007-keys.test.mjs | fullyVisibleCount counts only fully visible rows with a minimum of one |
| Passed | issue-1007-keys.test.mjs | ArrowLeft and ArrowRight always post arrowKey |
| Passed | issue-1007-keys.test.mjs | pending node carries sub active |
| Passed | issue-1007-keys.test.mjs | single click on a non-leaf segment in the open list posts selectorHighlight without bubbling |
| Passed | issue-1007-keys.test.mjs | segment double-click posts selectorSegmentDoubleClick with the row identity |
| Passed | issue-1007-keys.test.mjs | loaded child with zero children renders no toggle |
| Passed (P16-T3) | issue-997-common.test.mjs | row cells retain literal names, title, truncation and host percentage; the 12 arrow cases |
| Passed (unchanged) | issue-997-common.test.mjs, issue-997-recursive.test.mjs, issue-1002-nesting.test.mjs | every other case |
