# C# Isolation Runs (Issue #1007, remediation cycle 1, P5-T9)

Timestamp: 2026-10-26T00-06
Command: for each class, a fresh vstest.console.exe QuickFiler.Test.dll UtilitiesCS.Test.dll /InIsolation /Settings:scripts/vscode/TaskMaster.cli.runsettings "/TestCaseFilter:FullyQualifiedName~.<Class>." process, then one process with the six filters joined by `|`; against the P5-T3/P5-T4 build; results under SCRATCH
EXIT_CODE: 0
Output Summary: Seven runs: six single-class processes and one combined process. Every run matched at least one test and had 0 failures. The combined run's 146 tests equal the sum of the six single-class runs. The runsettings are parallel (Workers 0, ClassLevel), with no worker override and no retry. Counts are derived from each trx.

The `.<Class>.` filter form, with dots on both sides, keeps `BreadcrumbBridgeRouterIssue1007Tests` and `BreadcrumbBridgeRouterTests` from also matching the `FolderBreadcrumb...` classes.

| Run | Class order | Total | Passed | Failed |
|---|---|---|---|---|
| 1 | FolderBreadcrumbKeysIssue1007Tests | 9 | 9 | 0 |
| 2 | BreadcrumbBridgeRouterIssue1007Tests | 38 | 38 | 0 |
| 3 | BreadcrumbBridgeRouterTests | 24 | 24 | 0 |
| 4 | FolderBreadcrumbBridgeRouterIssue1007Tests | 18 | 18 | 0 |
| 5 | BreadcrumbStateModelTests | 38 | 38 | 0 |
| 6 | BreadcrumbBridgeCoordinatorIssue1007Tests | 19 | 19 | 0 |
| all | the six classes above, one process | 146 | 146 | 0 |

The full suite also ran in parallel in P5-T4 (7761 passed).
