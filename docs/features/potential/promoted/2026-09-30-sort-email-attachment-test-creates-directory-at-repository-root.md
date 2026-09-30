# sort-email-attachment-test-creates-directory-at-repository-root (Issue #945)

- Date captured: 2026-09-30
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/sort-email-attachment-test-creates-directory-at-repository-root/ (Issue #945)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #945
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/945
- Last Updated: 2026-09-30
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

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
