# filesystem-wrapper-tests-open-repository-solution-file (Issue #940)

- Date captured: 2026-09-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/filesystem-wrapper-tests-open-repository-solution-file/ (Issue #940)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #940
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/940
- Last Updated: 2026-09-30
## Summary

Several UtilitiesCS.Test file-system wrapper tests locate the repository root at run time and open or enumerate the real `TaskMaster.sln`. This is the same defect class that #931 (PR #939) fixed in `FileInfoWrapper_Tests`: the outcome of a unit test depends on repository layout and on whether another process holds the solution file open.

## Environment

- OS/version: Windows 11 (local) and windows-latest (CI)
- Python version: n/a (C# / MSTest, .NET Framework 4.8)
- Command/flags used: `Invoke-MSTestWithCoverage.ps1` (parallel run, Workers=0, Scope=ClassLevel)
- Data source or fixture: the repository's own `TaskMaster.sln`

## Steps to Reproduce

1. Hold `TaskMaster.sln` open with a share mode that excludes readers (for example a resident MSBuild node or an IDE).
2. Run `UtilitiesCS.Test` with the standard coverage route.
3. Alternatively, run the test assembly from a location whose ancestors contain no `TaskMaster.sln`.

## Expected Behavior

The wrapper tests verify wrapper delegation through test-owned fixtures or mocks, independent of repository layout and of other processes.

## Actual Behavior

- `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` walks up to the repository root (line ~361), builds a `FileInfo` over `TaskMaster.sln` (line ~376), asserts it is enumerated (lines 110-129), and swallows `IOException` in places, which hides the contention instead of removing it.
- `UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs` asserts that `TaskMaster.sln` is enumerated from the repository root (lines 60 and 79, root walk at line ~381).

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: see the PR #939 body, section Follow-ups, items 1 and 2.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

A unit test depends on environment state it does not control, which is the root cause recorded for #931. Other root-walk sites to triage under the same fix: `TaskMaster.Test/Ribbon/RibbonControllerTests.cs:429`, `TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs:27`, and `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs:405`. Some of these may legitimately read repository files, for example build-configuration alignment tests. Classify each one before changing it. Source: `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/qa-gates/p4-t13-follow-up-handoff.2026-09-29T09-46.md`.

## Proposed Fix / Validation Ideas

- [ ] Reuse the #931 pattern: exercise delegation through the internal `IFileInfo`/`IDirectoryInfo` seams with Moq, or through a test-owned stream opened with `FileShare.ReadWrite`. Use no temporary files.
- [ ] Remove the `IOException` swallowing.
- [ ] Negative control: show that each rewritten test fails against a deliberately broken wrapper.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
