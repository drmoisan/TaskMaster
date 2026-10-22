# PR Description Inputs (PD-11)

Timestamp: 2026-10-06T15-12
Command: git -C WORKTREE rev-parse HEAD (read-only)
EXIT_CODE: 0
Output Summary: verbatim inputs for the orchestrator's pr-author step: the two closing references, the four behavior changes of the spec's Data / API / Config Impact section, the UT5 call-out (phase one of L3 only), the untraced terminal sink of the L2 rethrow as a maintainer question, and the research and spec corrections PD-6, PD-10 and PD-14.

- HEAD-SHA: d857282fe0a8a0250d038ff71c2323a12607938f (observation at the time this artifact was written)

## Closing references

- Closes #959
- Closes #966

## Behavior changes (spec, Data / API / Config Impact)

1. Rethrow instead of hang under a held YesToAll: in the YesToAll-held persistent-denial state a filing operation now fails with the original `UnauthorizedAccessException` (logged at error level) instead of hanging, and the mail is not moved.
2. Alternate save path re-rooted under the destination folder: an alternate-name save on the live path now lands under the configured save folder rather than relative to the working directory.
3. Single tab-separated header line on first use: the moved-mails log receives one tab-separated header line on first use when the file is absent (previously never). Existing moved-mails files are not rewritten; a pre-existing file written by the old (no-op) path has no header and keeps none.
4. Prompt-state reset after a filer exception: sticky prompt answers are cleared after a filer exception (`EfcDataModel.MoveToFolderAsync` resets them in a `finally`).

Logging: four log4net records in the try-save handler (two warn, two error) replace three debug-output calls.

## UT5 call-out (copied verbatim from the spec's Test Strategy; applies to phase one of L3 only)

**UT5 call-out (required in the change description; applies to phase one of L3 only).** `Cleanup_Files_ResetsEveryPromptAnswerField` writes static state of `SortEmail` through reflection. Order-independence argument (R1 4.4): the only writers of the four fields are `Cleanup_Files` and the dialog-driven members `SaveAttachment`, `SaveAttachmentAsync`, `SaveCaseAsync` and `SaveAttachmentsOld`, none of which any test executes at that phase; the only concurrent writer in a test run is `Cleanup_Files_DoesNotThrow` in SortEmail_Tests.cs, which writes `Empty`, the value the assertion expects; before the fix it cannot write `_attachmentsAltName` at all, so it cannot cause a false pass, and after the fix a concurrent reset can only make the assertion true earlier; no test writes a non-`Empty` value except this one, for its own row, immediately before its own `Cleanup_Files` call. The test is order-independent in both states and needs no `[DoNotParallelize]`. The test is replaced by the read-only structural test in phase F1 and does not exist in the final tree.

## Maintainer question (spec, Risks)

L2 changes a production outcome from "never returns" to "throws". With D11 the rethrow now runs `Cleanup_Files` through the `finally`; the terminal sink of the exception beyond EfcFormController and EfcHomeController was not traced (R1 3.3). Please confirm the user-visible handling of the L2 rethrow.

## Research and spec corrections (already applied to FEATURE/spec.md where noted)

- PD-6 (research correction): each of the two `GetAttachmentsInfo` data tests gains TWO rows rather than one: the spec's (saveAttachments true, savePictures true) row, which AC13 names, plus the complementary row that covers the other filter body ((true, false) for the synchronous test, (false, true) for the asynchronous test). Without the complementary row one filter statement per method would stay uncovered once its exclusion is removed.
- PD-10 (research correction): the fail-before exception dossier is named `fail-before-exception.<TS>.md` (written as regression-testing/fail-before-exception.2026-10-03T12-33.md) to match the mandatory `fail-before-exception.*.md` search pattern, instead of the spec's `fail-before-exceptions.md` spelling.
- PD-14 (spec technical-section correction, already applied to FEATURE/spec.md): the try-save seam is the nested non-generic `SortEmail.TrySaveAttachmentDelegate` with the signature `(Attachment, string) -> Task<bool>`, not a `Func<Attachment, string, Task<bool>>`, because a generic instantiation over an Outlook interop type fails with CS1769 under the embedded interop types of UtilitiesCS.

## Acceptance (P6-T18, all three required)

1. EXIT_CODE: 0: met.
2. The artifact contains the two closing references, the four behavior changes and the literal `UT5 call-out`: met.
3. The artifact contains no absolute path: met.
