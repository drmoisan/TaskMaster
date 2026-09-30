# tests-depend-on-uncontrolled-environment (Issue #931)

- Date captured: 2026-09-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/tests-depend-on-uncontrolled-environment/ (Issue #931)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #931
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/931
- Last Updated: 2026-09-28
## Summary
Consolidates #905 and #906. The shared root cause: a unit test depends on environment state it does not control, so its result depends on scheduling or on other processes rather than on the code under test.

1. **#905:** tests use `Task.Run` as the "other thread". `Task.Run` guarantees only a thread-pool thread, never a different one, so under parallel execution the guard under test can go unexercised. PR #904 (#900) fixed two instances. Remaining:
   - `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs:332`. The file is 490 lines, so the fix requires a split to stay under 500.
   - Candidates to triage:
     - `BreadcrumbSelectorToggleUiBoundaryTests.cs:75`
     - `BreadcrumbPopupControlDispatchTests.cs:29,111`
     - `BreadcrumbPopupBoundaryCoverageTests.cs:58`
     - `BreadcrumbPopupBoundaryCoverageTests.Part2.cs:192`
     - `BreadcrumbUiThreadDispatchTests.cs:90,301`
   - `Task.Run(() => tcs.SetResult(...))` calls that only complete a task are not affected.
2. **#906:** `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs:56-62` opens the repository's own `TaskMaster.sln`, found by `GetSolutionFile()` at lines 340-352, as a fixture. Resident MSBuild node-reuse workers can hold that file open, so the outcome depends on build history.

## Environment
- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable (C#, MSTest, net48)
- Command/flags used: parallel regime `/Settings:TaskMaster.runsettings` (Workers 0, Scope ClassLevel)
- Data source or fixture: files listed above, `main` at `177b6d78e`

## Steps to Reproduce
1. Inspect the cited `Task.Run` sites and confirm that each asserts a thread-identity property against the thread it obtained.
2. Inspect `FileInfoWrapper_Tests.GetSolutionFile()` and confirm it resolves the repository's own solution file.

## Expected Behavior
- A test that needs a distinct thread uses a dedicated `Thread` that is joined, and asserts inside that thread that it is distinct (for example `CheckAccess() == false`) before exercising the guard. This is the #900 pattern.
- A file-handle test uses a stream the test owns, supplied through the wrapper's seam or an in-memory stream. It never uses a repository file.
- **Temporary files are prohibited by the unit-test policy and must not be used.**

## Actual Behavior
The guard under test can pass without being exercised, and the file-open test can fail or pass depending on MSBuild worker residency.

## Logs / Screenshots
- [ ] Attached minimal logs or screenshot
- Snippet: none (static findings, verified present on 2026-09-28)

## Impact / Severity
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes
Both patterns were copied from earlier tests and survived because they usually pass. Tests must run in parallel. Do not fix either defect with `Workers=1`, `[DoNotParallelize]` or retries.

## Proposed Fix / Validation Ideas
- [ ] Apply the #900 dedicated-thread pattern at every triaged site. Split `ItemViewerBreadcrumbThreadAffinityTests.cs` so it stays at or under 500 lines, and register any new file in `QuickFiler.Test.csproj`.
- [ ] Replace the `TaskMaster.sln` fixture with a test-owned stream or an injected seam. Add a seam to the wrapper only if one does not already exist.
- [ ] Each rewritten test must be shown to fail against a deliberately broken guard, so the test demonstrably exercises the guard.
- [ ] Run the full `QuickFiler.Test` and `UtilitiesCS.Test` suites in the parallel regime.

## Next Step
- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Consolidates: #905, #906.