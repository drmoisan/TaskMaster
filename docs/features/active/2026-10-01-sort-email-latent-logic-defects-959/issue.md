# sort-email-latent-logic-defects (Issue #959)

- Date captured: 2026-10-01
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/sort-email-latent-logic-defects/ (Issue #959)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #959
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/959
- Last Updated: 2026-10-01
- Work Mode: full-bug

## Summary

The #956 preparation research found four logic defects in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs`. #956 is a structural split plus testability seams, so it neither fixes nor depends on them:
- **L1:** `SaveCase` switch cases combine enum flags, so neither case can ever match.
- **L2:** with a sticky "Yes to all" answer, a save that keeps failing on access denied retries forever. This path is live in production through `EmailFiler`.
- **L3:** `Cleanup_Files` never resets `_attachmentsAltName`.
- **L4:** `WriteCSV_StartNewFileIfDoesNotExist` passes its `Path.Combine` arguments in reverse order, and its condition is inverted.

**Consolidated scope: issue #966 is folded into this item (maintainer directive, 2026-10-02).** #966 (SortEmail review residuals from #956 / PR #965) covers the same files and component, so it is remediated here rather than tracked as a follow-up. The pull request for this item closes both #959 and #966. In-scope #966 items:
- **F1:** route the overwrite prompt and the alternate-name prompt through `YesNoToAllPromptSession`; they still call the dialog directly.
- **F2:** delete the unused `SaveAttachmentsOld` and `IsPicture`, after confirming they have no callers.
- **F3:** remove `[ExcludeFromCodeCoverage]` from members that already have tests.
- **CR-1:** correct the #956 spec wording about the `DirectoryInfo` boundary so it matches where `new DirectoryInfo` now runs.
- **Try-save path:** replace the `Debug.WriteLine` calls with the project logger, and remove the outer rethrow, which has no effect.
- **Partial files:** remove the unused `using` directives in the `SortEmail` partial files.

**Scope rule (binding on every agent working this item, and to be passed to every subagent):** any defect a researcher or reviewer finds in the same files or with the same root cause is fixed within this item. Do not list such defects as follow-ups. Only completely unrelated defects may be reported for filing.

## Environment

- OS/version: Windows 11 (Outlook VSTO add-in)
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: static reading of `SortEmail.cs`
- Data source or fixture: n/a

## Steps to Reproduce

1. Read the `SaveCase` switch, the `TrySaveAttachmentAsync` retry path, `Cleanup_Files`, and `WriteCSV_StartNewFileIfDoesNotExist` in `SortEmail.cs`.
2. Compare each one with its intended behavior as described above.

## Expected Behavior

- Switch cases match the intended flag combinations.
- A persistent access-denied failure ends the retries and surfaces the error.
- Cleanup resets all prompt state.
- The CSV helper creates the file at the correct path, and only when it is absent.

## Actual Behavior

As described in the summary. None of the four has been reproduced at runtime yet.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: #956 research (`docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/research/`), findings L1 to L4.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

L2 can hang a production filing operation. The other three are latent.

## Suspected Cause / Notes

These are legacy code paths with no test coverage, partly because of the `[ExcludeFromCodeCoverage]` and dialog dependencies that #956 removes. Sequence this after #956 merges, so that each defect can get a regression test through #956's new seams.

## Proposed Fix / Validation Ideas

- [ ] For each of L1 to L4, write a failing regression test first, using the #956 prompt and file-system seams, then apply the minimal fix.
- [ ] For L2, bound the retry, or stop retrying on a persistent `UnauthorizedAccessException`, and surface the error.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
