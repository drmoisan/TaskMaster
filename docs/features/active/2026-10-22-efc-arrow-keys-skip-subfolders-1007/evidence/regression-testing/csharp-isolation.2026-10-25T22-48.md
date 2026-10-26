# C# Isolation Runs (Issue #1007, P18-T5)

Timestamp: 2026-10-25T22-48
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; then for each new class in reverse register order a fresh vstest.console.exe <owning test assembly> /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~<class>" process, then one vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll process over all nine classes (results under SCRATCH)
EXIT_CODE: 0
Output Summary: build `0 Error(s)`, `0 Warning(s)`. Ten runs, all under the parallel runsettings (`Workers` 0, `ClassLevel`) with no `[DoNotParallelize]`, no worker override and no retry: nine single-class processes (115 tests in total) and one combined process (115 tests). Every run had zero failures and vstest exit 0. Summaries derived from each run's trx.

| Run | Class (register group) | Assembly | Total | Passed | Failed | vstest exit |
|---|---|---|---|---|---|---|
| 1 | BreadcrumbBridgeJsKeysIssue1007Tests (G8) | UtilitiesCS.Test | 6 | 6 | 0 | 0 |
| 2 | FolderBreadcrumbKeysIssue1007Tests (G8) | QuickFiler.Test | 8 | 8 | 0 | 0 |
| 3 | BreadcrumbBridgeCoordinatorIssue1007Tests (G5, G6, G7) | QuickFiler.Test | 18 | 18 | 0 | 0 |
| 4 | FolderBreadcrumbBridgeRouterIssue1007Tests (G5, G7) | UtilitiesCS.Test | 14 | 14 | 0 | 0 |
| 5 | BreadcrumbSelectionSessionTreeIssue1007Tests (G5) | UtilitiesCS.Test | 13 | 13 | 0 | 0 |
| 6 | BreadcrumbBridgeRouterIssue1007Tests with its Navigation and DoubleClick partials (G3, G4) | QuickFiler.Test | 35 | 35 | 0 | 0 |
| 7 | BreadcrumbVisibleOrderTests (G2) | UtilitiesCS.Test | 13 | 13 | 0 | 0 |
| 8 | BreadcrumbEmptyExpanderIssue1007Tests (G1) | UtilitiesCS.Test | 5 | 5 | 0 | 0 |
| 9 | OutlookFolderHierarchyProviderIssue1007Tests (G1) | UtilitiesCS.Test | 3 | 3 | 0 | 0 |
| all | the nine classes in one process | UtilitiesCS.Test, QuickFiler.Test | 115 | 115 | 0 | 0 |
