# N-1 Direct Tests (Issue #1007, remediation cycle 1, P3-T8)

Timestamp: 2026-10-25T23-58
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true, then vstest.console.exe QuickFiler.Test.dll UtilitiesCS.Test.dll /InIsolation /Settings:scripts/vscode/TaskMaster.cli.runsettings "/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterIssue1007Tests|FullyQualifiedName~FolderBreadcrumbBridgeRouterIssue1007Tests|FullyQualifiedName~BreadcrumbStateModelTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorIssue1007Tests" (results under SCRATCH)
EXIT_CODE: 0
Output Summary: The ANALYZE build reported `Build succeeded.`, `0 Warning(s)`, `0 Error(s)`. Test Run Successful: 113 total, 113 passed, 0 failed. The four P3 tests passed on their first run, as expected for characterization tests of unchanged behavior.

## Trx-derived summary

| Figure | Value |
|---|---|
| Total | 113 |
| Executed | 113 |
| Passed | 113 |
| Failed | 0 |

| New test (task) | Outcome |
|---|---|
| LeafExpandToggle_OnRowCollapsedAwayFromActiveSegment_RestoresChain (P3-T1) | Passed |
| AffordanceToggle_OnRowCollapsedAwayFromActiveSegment_RestoresChainWithoutQuery (P3-T2) | Passed |
| RightArrow_OnRowCollapsedAwayFromActiveSegment_RestoresChain (P3-T4) | Passed |
| OpenUnhandledArrowLeft_OnExpandedPendingRow_RaisesUnhandledArrowFromRouterOutput (P3-T6) | Passed |
