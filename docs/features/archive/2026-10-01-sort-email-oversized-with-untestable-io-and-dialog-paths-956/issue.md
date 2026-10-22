# sort-email-oversized-with-untestable-io-and-dialog-paths (Issue #956)

- Date captured: 2026-10-01
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/ (Issue #956)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #956
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/956
- Last Updated: 2026-10-01
- Work Mode: full-bug

## Summary

`UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` is 1,454 lines, nearly three times the 500-line limit. It also mixes sorting logic with direct file-system and WinForms dialog calls that tests cannot reach. #945 added one injected seam for directory creation, and its review reported three residual follow-ups, recorded here together because they share one cause and one file.

## Environment

- OS/version: n/a
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: line count and review of `SortEmail.cs` after PR for #945
- Data source or fixture: n/a

## Steps to Reproduce

1. Count the lines in `SortEmail.cs`: 1,454.
2. Read `TrySaveAttachmentAsync`. It carries `[ExcludeFromCodeCoverage]` because of the `YesNoToAll.ShowDialog` prompt, and its `UnauthorizedAccessException` branches are untested.
3. Search the other `SortEmail` helpers for direct `System.IO.Directory` and `System.IO.File` calls.

## Expected Behavior

- Each file is under 500 lines, with cohesive partial classes or extracted types.
- UI prompts and file-system access sit behind injectable seams, so the coverage exclusion can be removed and the error branches tested.

## Actual Behavior

- The file is over the limit.
- The dialog branch forces a coverage exclusion.
- Other helpers may still touch the real file system.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: #945 executor and review follow-ups.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

Legacy static-class design (`SortEmail` is static) with I/O and UI inline. Per-call delegate seams, as used in #945, are safe under parallel tests. Settable static seams are not.

## Proposed Fix / Validation Ideas

- [ ] Split `SortEmail.cs` by responsibility into partial files or types under 500 lines each, with no behavior change and the existing tests green.
- [ ] Put `YesNoToAll.ShowDialog` behind an injected prompt delegate or interface, remove `[ExcludeFromCodeCoverage]` from `TrySaveAttachmentAsync`, and add tests for the `UnauthorizedAccessException` branches.
- [ ] Inventory the remaining direct `Directory` and `File` calls in `SortEmail` helpers, and give each one a seam or a recorded justification.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
