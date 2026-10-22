# ci-mstest-testhost-crash-in-folder-tree-service-tests (Issue #1004)

- Date captured: 2026-10-22
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ci-mstest-testhost-crash-in-folder-tree-service-tests/ (Issue #1004)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #1004
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/1004
- Last Updated: 2026-10-22
## Summary

The CI `mstest-coverage` job intermittently aborts with "Test host process crashed" while the UtilitiesCS.Test `OutlookFolderTreeService` concurrency, disposal and invalidation tests are running. No assertion fails; the test host process dies, so the run reports an unknown total and fails the job.

## Environment

- OS/version: GitHub Actions `windows` runner, Visual Studio 18 test platform (`vstest.console.exe`)
- Python version: not applicable (C# .NET Framework 4.8 MSTest)
- Command/flags used: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` via the CI `mstest-coverage` job
- Data source or fixture: UtilitiesCS.Test `OutlookFolderTreeServiceConcurrencyTests`, `OutlookFolderTreeServiceDisposalTests`, `OutlookFolderTreeServiceInvalidationTests`

## Steps to Reproduce

1. Run the CI workflow on a pull request (observed on PR #1003, run 39794073798, attempt 1, head e94c261302dcc2298b926f8682c89ad7448352f6).
2. Observe the `mstest-coverage` job: 4622 tests pass, then "The active test run was aborted. Reason: Test host process crashed".
3. Re-run the failed job on the same commit: attempt 2 passed all checks, so the crash is intermittent.

## Expected Behavior

The MSTest run completes deterministically; no test leaves an unhandled exception on a background or dispatcher thread that can terminate the test host.

## Actual Behavior

Intermittent test host crash. The last tests reported before the crash were `GetSnapshotAsync_WorkerOriginatedColdBuild_UsesCapturedStaDispatcher`, `Dispose_DuringRefresh_LeavesDisposedWithoutPublicationOrNotification`, `FolderChanged_DuringInFlightBuild_SchedulesOneFollowUpRefresh`, `NotificationRefreshAndDispose_RunOnTheCapturedDispatcher` and `NotificationRefresh_RunsOnCapturedDispatcher`.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `The active test run was aborted. Reason: Test host process crashed : Process path: ...\TestPlatform\Extensions\..\testhost.exe` / `Test Run Aborted. Total tests: Unknown Passed: 4622`

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

- An unhandled exception on a dedicated STA dispatcher thread or thread-pool continuation (for example a dispatcher callback or refresh continuation that outlives its test and throws after disposal) terminates the whole test host under .NET Framework.
- The uploaded CI artifact holds only the Cobertura document, not the trx or a blame/crash dump, so the crashing test is inferred from log order. Adding `--Blame` (crash collection) to the CI vstest invocation would name the test that was running.
- Issue #1002 / PR #1003 did not modify `OutlookFolderTreeService` or these test classes; its UtilitiesCS changes are synchronous types only.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: guard dispatcher and continuation callbacks in `OutlookFolderTreeService` against post-dispose execution; make the concurrency tests join or drain their dispatcher threads before returning
- [ ] Integration scenario to retest: repeated CI runs of UtilitiesCS.Test in the CI shape, with blame crash collection enabled
- [ ] Manual verification notes: none

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
