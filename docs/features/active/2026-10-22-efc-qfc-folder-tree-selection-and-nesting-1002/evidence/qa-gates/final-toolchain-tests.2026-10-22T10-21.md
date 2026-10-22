# Final Toolchain: TEST-COVERAGE (#1002)

Timestamp: 2026-10-22T10-21
Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .
EXIT_CODE: 0
Output Summary:
- Loop pass 2. Test run outcome Completed: total 7577, executed 7577, passed 7577, failed 0; error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0 (summary derived from the trx; skipped 0 derived as total minus executed).
- Shell-icon carve-out: not needed; the run did not stall.
- First-party coverage summary line: `First-party coverage: lines 57523/66780 (86.14%), branches 14014/17439 (80.36%)`.
- The post-processed Cobertura and the trx were copied to SCRATCH (`issue-1002-final-cobertura.xml`, `issue-1002-final.trx`) and are not committed.

## New tests (derived from the trx)

| Class | Results | Methods | Passed | Not passed |
|---|---|---|---|---|
| BreadcrumbMessageCodecIssue1002Tests | 8 | 5 | 8 | 0 |
| BreadcrumbRowDescendantsIssue1002Tests | 23 | 23 | 23 | 0 |
| BreadcrumbHtmlRendererIssue1002Tests | 10 | 10 | 10 | 0 |
| BreadcrumbBridgeRouterIssue1002Tests | 11 | 11 | 11 | 0 |
| BreadcrumbBridgeRouterStaleCompletionIssue1002Tests | 7 | 7 | 7 | 0 |
| BreadcrumbBridgeRouterTrashIssue1002Tests | 6 | 6 | 6 | 0 |
| FolderBreadcrumbNestingIssue1002Tests | 4 | 4 | 4 | 0 |
| EfcFormControllerTests (Issue1002 partial): ActionDeleteAsync_WithRouter_SelectsTrashEntry, ActionDeleteAsync_WithRouter_DeliveredDocumentMarksTrashSelected, ActionDeleteAsync_AwaitedTwice_KeepsTrashSelectedAndSingleTrashRow, ActionDeleteAsync_BindFault_IsReportedAtBindBoundary | 4 | 4 | 4 | 0 |

Every new test name from the spec Test Strategy and the plan Test Name Register appears as Passed (70 methods, 73 results). The node suite is not run by this step; see `node-suites-run`.

## Kept-green existing classes (derived from the trx)

| Class | Results | Failed |
|---|---|---|
| BreadcrumbHtmlRendererTests | 15 | 0 |
| BreadcrumbMessageCodecTests | 26 | 0 |
| BreadcrumbRowStateTests | 22 | 0 |
| BreadcrumbBridgeRouterTests (with .Selection partial) | 24 | 0 |
| BreadcrumbBridgeRouterQueueTests (with .Part2 partial) | 26 | 0 |
| BreadcrumbBridgeRouterScoreJoinTests | 7 | 0 |
| BreadcrumbBridgeRouterIssue439Tests (with .Activation partial) | 10 | 0 |
| BreadcrumbBridgeRouterIssue614Tests | 8 | 0 |
| BreadcrumbBridgeRouterIssue637Tests | 10 | 0 |
| BreadcrumbBridgeRouterIssue792Tests | 4 | 0 |
| BreadcrumbBridgeRouterIssue995Tests | 10 | 0 |
| EfcFormControllerTests (including ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows: Passed) | 36 | 0 |
| EfcFormControllerIssue995Tests | 4 | 0 |
| EfcHomeControllerExecuteMovesTests | 7 | 0 |
| FolderBreadcrumbAssetContractTests | 15 | 0 |
| FolderBreadcrumbRecursiveAssetIssue997Tests | 3 | 0 |
