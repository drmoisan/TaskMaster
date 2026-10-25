# Page-Script Fail-Before (Issue #1007, P16-T7)

Timestamp: 2026-10-25T22-38
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~FolderBreadcrumbKeysIssue1007Tests|FullyQualifiedName~BreadcrumbBridgeJsKeysIssue1007Tests|FullyQualifiedName~FolderBreadcrumbAssetContractTests" (results under SCRATCH); then node --test --test-concurrency=1 --test-reporter=spec tests/quickfiler-breadcrumb/issue-997-common.test.mjs tests/quickfiler-breadcrumb/issue-997-recursive.test.mjs tests/quickfiler-breadcrumb/issue-1002-nesting.test.mjs tests/quickfiler-breadcrumb/issue-1007-keys.test.mjs
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: build `0 Error(s)`, `0 Warning(s)`. C# run: total 29, passed 13, failed 16: every G8 non-guard C# test (8 QFC, 5 EFC) and the three P16-T6 updated tests failed; the EFC guard `BridgeJs_NoSelection_PostsEmptyRowId` passed, as did the 12 unchanged asset-contract tests. Node run: tests 90, pass 75, fail 15: all ten G8 node tests failed, and so did the five P16-T3 updated cases whose expectation changed; the other 75 cases passed. vstest exit 1, node exit 1. Summaries derived from the trx and the node spec output.

Run notes:
- The first rebuild of this pass failed with CS8370 because `QuickFiler.Test` compiles at C# 7.3, and the new `FolderBreadcrumbKeysIssue1007Tests.cs` carried `#nullable enable`. The directive was removed; the other new QuickFiler.Test files of this plan omit it for the same reason. The run above followed a clean rebuild.
- In the first node run, `ctrl+ArrowRight posts arrowKey right` passed on the pre-fix script. Its suggestion-row fixture already posted `arrowKey` because the old script never read `ctrlKey`, so the test did not exercise Ctrl+Right. The fixture was changed, within P16-T2, to a plain row, where the pre-fix pre-filter posts `unhandledArrow`; the run above is the re-run.
- P16-T3 changed the expectation of four of the twelve arrow cases (`plain` and `empty`, left and right); the other eight already expected `arrowKey`, so their passing outcome is unchanged.

## C# string contracts

| Outcome | Class | Test |
|---|---|---|
| Failed | FolderBreadcrumbKeysIssue1007Tests | SelectorKeys_MapHomeEndPageUpPageDown (selectorKeys has no Home entry) |
| Failed | FolderBreadcrumbKeysIssue1007Tests | CtrlArrowLeft_PostsParentSelectorKey (no ctrl+ArrowLeft branch) |
| Failed | FolderBreadcrumbKeysIssue1007Tests | PageKeys_CarryPageSizeFromFullyVisibleCount (no fullyVisibleCount function) |
| Failed | FolderBreadcrumbKeysIssue1007Tests | OnArrow_AlwaysPostsArrowKey (onArrow still pre-filters) |
| Failed | FolderBreadcrumbKeysIssue1007Tests | MakeSubfolders_MarksPendingNodeActive (no pendingNodeIdentity state) |
| Failed | FolderBreadcrumbKeysIssue1007Tests | NonLeafSegmentClick_OpenList_StopsPropagationAndPostsSelectorHighlight (no selectorHighlight) |
| Failed | FolderBreadcrumbKeysIssue1007Tests | SegmentDoubleClick_PostsSelectorSegmentDoubleClick (still segmentDoubleClick) |
| Failed | FolderBreadcrumbKeysIssue1007Tests | MakeSubfolders_LoadedChildWithoutChildren_RendersNoToggle (no childrenLoaded check) |
| Failed | BreadcrumbBridgeJsKeysIssue1007Tests | BridgeJs_CtrlArrowLeft_PostsParentLeft (no ParentLeft rewrite) |
| Failed | BreadcrumbBridgeJsKeysIssue1007Tests | BridgeJs_CtrlArrowRight_PostsRight (no ctrl branch) |
| Failed | BreadcrumbBridgeJsKeysIssue1007Tests | BridgeJs_HomeEndPageKeys_MapToKeyValues (key map lacks Home) |
| Failed | BreadcrumbBridgeJsKeysIssue1007Tests | BridgeJs_PageKeys_CarryMeasuredPageSize (no pageSize) |
| Failed | BreadcrumbBridgeJsKeysIssue1007Tests | BridgeJs_Scroll_TargetsSelectedChildBeforeSelectedRow (scrolls the row only) |
| Passed (guard) | BreadcrumbBridgeJsKeysIssue1007Tests | BridgeJs_NoSelection_PostsEmptyRowId |
| Failed (P16-T6) | FolderBreadcrumbAssetContractTests | SelectorKeys_PreventBrowserScrollingAndPostNativeKeyMessages |
| Failed (P16-T6) | FolderBreadcrumbAssetContractTests | RenderReceiver_OwnsSelectedChildExpandedAndCollapsedProjection |
| Failed (P16-T6) | FolderBreadcrumbAssetContractTests | LeftAndRightBreadcrumbMessages_RemainSupported |

## Node suites

| Outcome | File | Test |
|---|---|---|
| Failed | issue-1007-keys.test.mjs | ctrl+ArrowLeft posts the parent selector key |
| Failed | issue-1007-keys.test.mjs | ctrl+ArrowRight posts arrowKey right |
| Failed | issue-1007-keys.test.mjs | Home and End post selector keys |
| Failed | issue-1007-keys.test.mjs | PageUp and PageDown post the fully visible row count as pageSize |
| Failed | issue-1007-keys.test.mjs | fullyVisibleCount counts only fully visible rows with a minimum of one |
| Failed | issue-1007-keys.test.mjs | ArrowLeft and ArrowRight always post arrowKey |
| Failed | issue-1007-keys.test.mjs | pending node carries sub active |
| Failed | issue-1007-keys.test.mjs | single click on a non-leaf segment in the open list posts selectorHighlight without bubbling |
| Failed | issue-1007-keys.test.mjs | segment double-click posts selectorSegmentDoubleClick with the row identity |
| Failed | issue-1007-keys.test.mjs | loaded child with zero children renders no toggle |
| Failed (P16-T3) | issue-997-common.test.mjs | row cells retain literal names, title, truncation and host percentage |
| Failed (P16-T3) | issue-997-common.test.mjs | arrow left routes plain state through host contract |
| Failed (P16-T3) | issue-997-common.test.mjs | arrow left routes empty state through host contract |
| Failed (P16-T3) | issue-997-common.test.mjs | arrow right routes plain state through host contract |
| Failed (P16-T3) | issue-997-common.test.mjs | arrow right routes empty state through host contract |
| Passed (P16-T3, expectation unchanged) | issue-997-common.test.mjs | arrow left and right routes suggestion, collapsed, expanded and affordance state (8 cases) |
| Passed (unchanged) | issue-997-common.test.mjs | clicking a selectable row uses its host identity |
| Passed (unchanged, P16-T1 harness default) | issue-997-recursive.test.mjs, issue-1002-nesting.test.mjs | every case (28) |
