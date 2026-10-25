# QFC Closed-List Fail-Before (Issue #1007, P12-T3)

Timestamp: 2026-10-25T22-23
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; then vstest.console.exe QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeCoordinatorIssue1007Tests|FullyQualifiedName~QuickFiler.Test.Viewers.BreadcrumbBridgeCoordinatorTests|FullyQualifiedName~BreadcrumbSelectorCoordinatorTests" (results under SCRATCH)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: build `0 Error(s)`, `0 Warning(s)`. Tests: total 44, executed 44, passed 36, failed 8. Every G6 non-guard test (5) failed and both G6 guards passed. Three of the four P12-T2 updated tests failed; `UnhandledRightArrow_RaisesUnhandledArrowRight` passed, and the reason is recorded in fail-before-exception.2026-10-25T22-23.md in this folder. The remaining 28 passing rows are unchanged tests of the three classes. Summary derived from the trx.

| Outcome | Class | Test | Failure message (from the trx, shortened) |
|---|---|---|---|
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | ClosedRight_OpensSelectorWithPendingOnVisibleCommittedSubfolder | Expected IsSelectorOpen to be True, but found False. |
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | ClosedRight_SubfolderHidden_PendingOnRow | Expected IsSelectorOpen to be True, but found False. |
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | ClosedLeft_IsConsumedWithoutEffect | Expected h.Events to be empty, but found {"unhandled:Left"}. |
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | ClosedHomeEnd_CommitFirstAndLastTopLevelRow | Expected the Beta path after End, but found the Alpha path. |
| Failed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorKeyParent_Closed_CommitsImmediately | Expected the Projects path, but found the Alpha path. |
| Passed (guard) | BreadcrumbBridgeCoordinatorIssue1007Tests | ClosedPageKeys_AreNoOps | - |
| Passed (guard) | BreadcrumbBridgeCoordinatorIssue1007Tests | ClosedUpDown_KeepDirectCommitOfAdjacentTopLevelRow | - |
| Failed (P12-T2) | BreadcrumbBridgeCoordinatorTests | UnhandledLeftArrow_RaisesUnhandledArrowLeft | Expected directions to be empty, but found {Left}. |
| Failed (P12-T2) | BreadcrumbBridgeCoordinatorTests | ArrowMessages_RaiseSyntheticFolderKeyDown | Expected keys {Right}, but {Right, Left} contains 1 item too many. |
| Failed (P12-T2) | BreadcrumbSelectorCoordinatorTests | ExistingLeftAndRightMessages_StillForwardOnce | Expected synthetic to be 0, but found 1. |
| Passed (P12-T2, see exception dossier) | BreadcrumbBridgeCoordinatorTests | UnhandledRightArrow_RaisesUnhandledArrowRight | - |
