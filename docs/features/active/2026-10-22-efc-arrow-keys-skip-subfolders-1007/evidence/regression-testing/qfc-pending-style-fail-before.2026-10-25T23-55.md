# QFC Pending Style: Fail Before (Issue #1007, B-1, P1-T2) [expect-fail]

Timestamp: 2026-10-25T23-55
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true, then vstest.console.exe QuickFiler.Test.dll UtilitiesCS.Test.dll /InIsolation /Settings:scripts/vscode/TaskMaster.cli.runsettings "/TestCaseFilter:FullyQualifiedName~FolderBreadcrumbKeysIssue1007Tests" (results under SCRATCH)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: The ANALYZE build reported `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`. The scoped run matched 9 tests: 8 passed and 1 failed. The failure is the missing `.sub.active` rule, as expected before the fix.

## Trx-derived summary

| Figure | Value |
|---|---|
| Total | 9 |
| Passed | 8 |
| Failed | 1 |

| Test | Outcome |
|---|---|
| SelectorKeys_MapHomeEndPageUpPageDown | Passed |
| CtrlArrowLeft_PostsParentSelectorKey | Passed |
| PageKeys_CarryPageSizeFromFullyVisibleCount | Passed |
| OnArrow_AlwaysPostsArrowKey | Passed |
| MakeSubfolders_MarksPendingNodeActive | Passed |
| NonLeafSegmentClick_OpenList_StopsPropagationAndPostsSelectorHighlight | Passed |
| SegmentDoubleClick_PostsSelectorSegmentDoubleClick | Passed |
| MakeSubfolders_LoadedChildWithoutChildren_RendersNoToggle | Passed |
| Stylesheet_StylesPendingDescendantLikeSelectedSubfolder | Failed |

Failure message: `Expected declarations not to be <null> because the QFC stylesheet must style the pending descendant (.sub.active).`

The failure comes from the `.sub.active` lookup. The `.sub.selected` positive-control lookup and its two variable assertions ran first and passed, so the failure isolates the missing rule.
