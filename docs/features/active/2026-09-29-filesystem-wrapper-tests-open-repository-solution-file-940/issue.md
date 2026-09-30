# filesystem-wrapper-tests-open-repository-solution-file (Issue #940)

- Date captured: 2026-09-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/filesystem-wrapper-tests-open-repository-solution-file/ (Issue #940)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #940
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/940
- Last Updated: 2026-09-30
- Work Mode: minor-audit

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

## Acceptance Criteria

- [ ] AC1: No test method in `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` or `UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs` locates the repository root or references `TaskMaster.sln`; both `GetRepositoryRoot` helpers and the `GetSolutionFile` helper are removed.
- [ ] AC2: No test in either file catches `IOException` (or any broader exception type) to tolerate file-system contention.
- [ ] AC3: No test in either file directs a mutating call (timestamp, attribute, read-only flag, access-control, create, copy, replace, move, or delete) at the repository root or at any tracked repository file or directory; each mutating member is exercised only against a Moq mock, a path under the test assembly's own output directory that is asserted not to exist before the call, or an existing entry of that output directory, the output directory itself, or its parent directory, on which the call is a no-op by construction.
- [ ] AC4: Each wrapper or adapter member previously exercised by a rewritten test is still exercised, and its delegation is asserted against a Moq mock, a test-owned read-only fixture, or a documented non-existent-path outcome; a member whose effect on an existing test-owned entry is a no-op by construction (`Create`, `Refresh`, and `SetAccessControl` with an unmodified security object) is instead exercised against that entry and must complete without an exception; so that coverage of the changed wrapper and adapter lines does not regress.
- [ ] AC5: No temporary file or directory is created by any test in either file, and no test introduces `DoNotParallelize`, a worker-count or scope change, a retry, or a sleep.
- [ ] AC6: A negative-control run shows that each rewritten test fails when the wrapper or adapter delegation it verifies is deliberately broken, and the control change is reverted before the final QC pass.
- [ ] AC7: The three additional root-walk sites named under Suspected Cause (`RibbonControllerTests.cs`, `FSharpCoreHintPathAlignmentTests.cs`, `SortEmail_Tests.cs`) are each classified in an evidence artifact as either a legitimate repository-file read or an instance of this defect class; none is modified by this change.
- [ ] AC8: The C# toolchain passes in order (CSharpier check, analyzer rebuild, TreatWarningsAsErrors rebuild, MSTest with coverage) with no new failures relative to the baseline and with repository line coverage at or above the baseline figure.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
