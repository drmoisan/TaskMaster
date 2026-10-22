# C# Test Isolation: New Classes in Fresh Processes (#1002)

Timestamp: 2026-10-22T10-17
Command: for each class in reverse spec Test Strategy order: vstest.console.exe <owning test assembly> /InIsolation /TestCaseFilter:"FullyQualifiedName~.<Class>." /ResultsDirectory:SCRATCH/results-issue-1002-iso-<n> /Logger:"trx;LogFileName=issue-1002-iso-<n>.trx"; then one vstest.console.exe process over both assemblies with the eight class clauses joined by `|` (launcher SCRATCH/issue-1002-isolation.ps1)
EXIT_CODE: 0
Output Summary: zero failures in every run; counts derived from each trx.

| Run | Class (fresh process) | Assembly | Exit | Total / passed / failed |
|---|---|---|---|---|
| 1 | FolderBreadcrumbNestingIssue1002Tests | QuickFiler.Test | 0 | 4 / 4 / 0 |
| 2 | EfcFormControllerTests (holds the EfcFormControllerTests.Issue1002.cs partial) | QuickFiler.Test | 0 | 36 / 36 / 0 |
| 3 | BreadcrumbBridgeRouterTrashIssue1002Tests | QuickFiler.Test | 0 | 6 / 6 / 0 |
| 4 | BreadcrumbBridgeRouterStaleCompletionIssue1002Tests | QuickFiler.Test | 0 | 7 / 7 / 0 |
| 5 | BreadcrumbBridgeRouterIssue1002Tests | QuickFiler.Test | 0 | 11 / 11 / 0 |
| 6 | BreadcrumbHtmlRendererIssue1002Tests | UtilitiesCS.Test | 0 | 10 / 10 / 0 |
| 7 | BreadcrumbRowDescendantsIssue1002Tests | UtilitiesCS.Test | 0 | 23 / 23 / 0 |
| 8 | BreadcrumbMessageCodecIssue1002Tests | UtilitiesCS.Test | 0 | 8 / 8 / 0 |
| all | all eight classes in one process | both | 0 | 105 / 105 / 0 |

Notes: the codec class reports 8 results because three methods carry two data rows each. The EfcFormControllerTests class runs its existing partials with the new partial, since a class filter cannot separate partial files.
