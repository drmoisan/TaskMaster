# flaky-physicalfileinfoadapter-open-fileshare-none (Issue #278)

- Date captured: 2026-07-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/flaky-physicalfileinfoadapter-open-fileshare-none/ (Issue #278)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #278
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/278
- Last Updated: 2026-07-08
- Work Mode: minor-audit

## Summary

The unit test `UtilitiesCS.Test.HelperClasses.PhysicalFileSystemAdapters_Tests.PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo` is non-deterministic (flaky). It intermittently fails the required CI check with an `IOException` because it opens the real repository `TaskMaster.sln` file with `FileShare.None` while the CI build/coverage process holds that file open.

## Environment

- OS/version: Windows; GitHub Actions `windows`-class runner (CI check "Format, build, analyze, and test")
- Python version: N/A (C# MSTest)
- Command/flags used: `vstest.console.exe ... /EnableCodeCoverage` (parallel test execution + coverage instrumentation)
- Data source or fixture: the real `TaskMaster.sln` at the repository root

## Steps to Reproduce

1. Run the full MSTest suite with coverage under parallel execution on CI while another process (build/coverage) holds `TaskMaster.sln` open.
2. Observe `PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo` intermittently fail.

Observed on PR #272 (issue #270): 4995 passed, 1 failed, 1 skipped; the sole failure was this test. A re-run of the failed job passed, confirming non-determinism.

## Expected Behavior

The test must be deterministic: it must verify the adapter's `Open(FileMode, FileAccess)` delegation without acquiring a real `FileShare.None` handle on a shared file, and without using any temporary/scratch file (prohibited by the unit-test policy).

## Actual Behavior

```
System.IO.IOException: The process cannot access the file 'D:\a\TaskMaster\TaskMaster\TaskMaster.sln' because it is being used by another process.
   at UtilitiesCS.HelperClasses.FileSystem.PhysicalFileInfoAdapter.Open(FileMode mode, FileAccess access) in ...\PhysicalFileInfoAdapter.cs:line 134
   at ...PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo() in ...\PhysicalFileSystemAdapters_Tests.cs:line 207
```

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: see Actual Behavior. Failing job: https://github.com/drmoisan/TaskMaster/actions/runs/28914676821/job/85779070610

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Rationale: intermittently fails a required CI check, blocking unrelated PRs until a re-run happens to pass. Not a production defect, but a recurring merge-flow disruptor.

## Suspected Cause / Notes

`PhysicalFileInfoAdapter` (`UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs`) already has an injectable-delegate seam for `AppendText()`, the 1-arg `Open(FileMode)`, and `OpenWrite()`. The 2-arg `Open(FileMode, FileAccess)` (line 134) is NOT seamed and calls `_fileInfo.Open(mode, access)` directly, which uses `FileShare.None`. The test at `PhysicalFileSystemAdapters_Tests.cs:207` exercises that overload against the real `TaskMaster.sln`, so under concurrent CI access it throws.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: extend the adapter's existing injectable-delegate seam to cover `Open(FileMode, FileAccess)` (a `Func<FileMode, FileAccess, FileStream>` bound by default to `_fileInfo.Open` in the public constructor, unchanged production behavior). Update the test to verify that overload's delegation via a test-owned sentinel stream (the pattern already used for the write-mode opens in the same test), removing the real `FileShare.None` open of `TaskMaster.sln`.
- [ ] Integration scenario to retest: N/A.
- [x] Manual verification notes: after the fix, the test acquires no `FileShare.None` handle on any shared/real file and is deterministic under parallel CI.

## Acceptance Criteria

- [x] AC1: `PhysicalFileInfoAdapter.Open(FileMode, FileAccess)` (currently `PhysicalFileInfoAdapter.cs:134`) delegates through an injectable seam field (e.g. `Func<FileMode, FileAccess, FileStream>`), bound by default to `_fileInfo.Open` in the public constructor so production behavior is unchanged. The internal test-only constructor accepts the new delegate.
- [x] AC2: `PhysicalFileSystemAdapters_Tests.PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo` no longer opens the real `TaskMaster.sln` (or any real/shared file) with `FileShare.None`. The 2-arg `Open(FileMode, FileAccess)` delegation is verified through a test-owned sentinel stream via the seam, matching the existing write-mode-open sentinel pattern in the same test.
- [x] AC3: No temporary/scratch file is created or used by the test (unit-test policy). Sentinels are in-memory or read-only `FileShare.ReadWrite` opens as already used in the file.
- [x] AC4: The test remains meaningful — it still asserts the 2-arg `Open` overload's delegation (returns the seam-provided stream), so coverage of `PhysicalFileInfoAdapter.Open(FileMode, FileAccess)` is preserved (>= its prior coverage; the production line executes via the default-bound delegate in other paths or via a dedicated default-binding assertion).
- [x] AC5: The full C# toolchain passes in order (CSharpier -> .NET analyzers -> nullable/type-check -> MSTest) with no new warnings on touched files, and coverage on changed lines does not regress.
- [x] AC6: Scope is limited to `UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs` (production) and `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` (test). No unrelated files are changed. (Note: the analogous non-seamed read opens `OpenRead()`/`OpenText()` may be assessed; only extend scope to them if required to make the test deterministic, otherwise leave as a documented note.)

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
