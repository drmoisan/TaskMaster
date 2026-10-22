# sort-email-attachment-test-creates-directory-at-repository-root (Issue #945)

- Date captured: 2026-09-30
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/sort-email-attachment-test-creates-directory-at-repository-root/ (Issue #945)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #945
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/945
- Last Updated: 2026-09-30
- Work Mode: minor-audit

## Summary

The `SortEmail_Tests` test of `TrySaveAttachmentAsync` (`UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` lines ~237-249) passes a path under the real repository root. It reaches `Directory.CreateDirectory` in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` (line ~896), so the unit test creates a directory in the working tree. The repository policy prohibits this, and it is the same defect class as #931 and #940.

## Environment

- OS/version: Windows 11 / windows-latest
- Python version: n/a (C# / MSTest)
- Command/flags used: standard MSTest coverage route
- Data source or fixture: `GetRepositoryRoot()` helper in `SortEmail_Tests.cs` (lines ~393-415)

## Steps to Reproduce

1. Run the `TrySaveAttachmentAsync` test in `SortEmail_Tests`.
2. Inspect the repository root for a newly created directory.

## Expected Behavior

The test exercises the save path without touching the real file system, through a seam or a mock, and creates no directory or file.

## Actual Behavior

`Directory.CreateDirectory` runs against a path derived from the repository root.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: #940 research, section 5 and open question 10 (`docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/research/`).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

`TrySaveAttachmentAsync` is marked `[ExcludeFromCodeCoverage]` (line ~888) and has no injectable file-system seam, so the test drives the real one. #940 classifies this site but deliberately does not modify it.

## Proposed Fix / Validation Ideas

- [ ] Introduce an injectable file-system abstraction for the directory-creation step, reusing the existing `IFileSystem`-style seams in UtilitiesCS, and mock it in the test.
- [ ] Assert that no real directory is created, using a strict mock with `VerifyNoOtherCalls`.
- [ ] Negative control: the rewritten test fails if the production code bypasses the seam.

## Acceptance Criteria

Scope decision (orchestrator, 2026-09-30, from `research/2026-09-30T07-30-sort-email-attachment-test-creates-directory-at-repository-root-research.md`): the fix edits exactly `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` and `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs`. The four other `GetRepositoryRoot()` uses in `SortEmail_Tests.cs` perform no file-system write or create and are out of scope. The pre-existing method-level `[ExcludeFromCodeCoverage]` is retained on both overloads because the seamed core still contains the `YesNoToAll.ShowDialog` WinForms branch; the exemption scope is not widened.

- [x] AC1: `SortEmail.cs` declares an `internal static` overload `TrySaveAttachmentAsync(this Attachment attachment, string filePathSave, Action<string> createDirectory)` that calls `createDirectory(Path.GetDirectoryName(filePathSave))` in place of the direct `System.IO.Directory.CreateDirectory` call, and its read-only retry path passes the same `createDirectory` delegate to the recursive call.
- [x] AC2: the existing two-parameter `TrySaveAttachmentAsync(this Attachment attachment, string filePathSave)` is reduced to a single delegation to the new overload with `path => System.IO.Directory.CreateDirectory(path)`; no production caller of `TrySaveAttachmentAsync` is edited and no static settable delegate or shared mutable seam is introduced.
- [x] AC3: `TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile` uses a rooted literal destination path not derived from `GetRepositoryRoot()`, injects a recording `createDirectory` delegate, and asserts that the delegate received the destination directory before `SaveAsFile` received the destination path, that the result is `true`, and that `SaveAsFile` was called exactly once; the test reaches no real file-system creation API.
- [x] AC4: a new test `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave` asserts that an `IOException` thrown by the injected delegate propagates and that `SaveAsFile` is never called; no test throws `UnauthorizedAccessException`, so no test can reach `YesNoToAll.ShowDialog`.
- [x] AC5: negative control: with the `createDirectory(...)` call removed from the new overload, the rewritten success test fails on its recorded-event assertion; the failing run is recorded under `evidence/regression-testing/`, and the production file is restored byte-identical to the fixed version before final QC.
- [x] AC6: a scoped run of `SortEmail_Tests` reports 15 total, 15 passed, 0 failed (14 before the change).
- [x] AC7: the full C# toolchain passes in one pass (CSharpier check, analyzer `/t:Rebuild`, nullable `/t:Rebuild` with `/p:TreatWarningsAsErrors=true`, MSTest coverage run); against the Phase 0 baseline taken with the same collector, the change adds no uncovered line to `SortEmail.cs` (per-file uncovered-line delta at most 0), and the UtilitiesCS package line and branch rates and the repository first-party line rate are each no more than 0.10 percentage points below baseline (a tolerance for the collector's run-to-run variance, ratified by the orchestrator on 2026-09-30 at preflight round 1).
- [x] AC8: the branch diff against its base changes no source file other than the two named in the scope decision above; all other changes are under this feature folder.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
