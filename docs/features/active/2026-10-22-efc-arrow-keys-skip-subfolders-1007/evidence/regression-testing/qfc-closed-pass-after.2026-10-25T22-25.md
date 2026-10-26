# QFC Closed-List Pass-After (Issue #1007, P13-T3)

Timestamp: 2026-10-25T22-25
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; then vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~Breadcrumb|FullyQualifiedName~OutlookFolderHierarchyProvider" (results under SCRATCH)
EXIT_CODE: 0
Output Summary: build `0 Error(s)`, `0 Warning(s)`. BREADCRUMB-FILTER run: total 1046, executed 1046, passed 1046, failed 0. Every G6 test (7) and every Phase 12 updated test (4) passed. Summary derived from the trx.

Implementation notes: P13-T1 implements `BreadcrumbSelectionSession.CommitBoundaryRow` (closed session only) and the D-Q19 closed-list Ctrl+Left commit (`MoveToParent` on a closed session commits the parent node, the row entry at depth 0, or the parent segment with the row shortened), exposed through `FolderBreadcrumbBridgeRouter.CommitSelectorBoundaryRow` and a closed-aware `MoveSelectorToParent`. P13-T2 adds the closed-list half of R8 to `BreadcrumbBridgeCoordinator.Keyboard.cs`: closed Right opens the list, closed Left is consumed, closed Home and End commit the first and last top-level row, closed page keys stay no-ops, and closed Up and Down keep `MoveSelector`.

| Outcome | Class | Test |
|---|---|---|
| Passed | BreadcrumbBridgeCoordinatorIssue1007Tests | ClosedRight_OpensSelectorWithPendingOnVisibleCommittedSubfolder |
| Passed | BreadcrumbBridgeCoordinatorIssue1007Tests | ClosedRight_SubfolderHidden_PendingOnRow |
| Passed | BreadcrumbBridgeCoordinatorIssue1007Tests | ClosedLeft_IsConsumedWithoutEffect |
| Passed | BreadcrumbBridgeCoordinatorIssue1007Tests | ClosedHomeEnd_CommitFirstAndLastTopLevelRow |
| Passed | BreadcrumbBridgeCoordinatorIssue1007Tests | SelectorKeyParent_Closed_CommitsImmediately |
| Passed (guard) | BreadcrumbBridgeCoordinatorIssue1007Tests | ClosedPageKeys_AreNoOps |
| Passed (guard) | BreadcrumbBridgeCoordinatorIssue1007Tests | ClosedUpDown_KeepDirectCommitOfAdjacentTopLevelRow |
| Passed (P12-T2) | BreadcrumbBridgeCoordinatorTests | UnhandledRightArrow_RaisesUnhandledArrowRight |
| Passed (P12-T2) | BreadcrumbBridgeCoordinatorTests | UnhandledLeftArrow_RaisesUnhandledArrowLeft |
| Passed (P12-T2) | BreadcrumbBridgeCoordinatorTests | ArrowMessages_RaiseSyntheticFolderKeyDown |
| Passed (P12-T2) | BreadcrumbSelectorCoordinatorTests | ExistingLeftAndRightMessages_StillForwardOnce |
