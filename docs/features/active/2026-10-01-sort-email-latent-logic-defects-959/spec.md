# 2026-10-01-sort-email-latent-logic-defects (Spec)

- **Issue:** #959 (the pull request also closes #966; decision D18)
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-02
- **Status:** Draft revision 0.2 (consolidated scope; awaiting orchestrator acceptance before planning)
- **Version:** 0.2
- **Work mode:** full-bug. This file is the sole acceptance-criteria source for this item; no user-story.md is produced.
- **Research records:** R1 = `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/research/2026-10-02T05-15-sort-email-latent-logic-defects-research.md` (L1 to L4); R2 = `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/research/2026-10-02T05-50-sort-email-966-consolidation-research.md` (consolidation). R2 section 0.2 supersedes R1 where it says so. Sections are cited below as "R1 n" and "R2 n".
- **Revision note:** revision 0.1 followed a decision that placed #966 out of scope and flagged the conflict with issue.md. That decision is withdrawn. This revision applies issue.md lines 22 to 35 (maintainer directive, 2026-10-02): #966 is folded into this item, three same-root-cause items are added, and the binding scope rule (any defect found in the same files or with the same root cause is fixed here; only completely unrelated defects are reported for filing) was applied while writing. One further same-file defect found by R2 (section 1.6) is therefore in scope.
- **Planner revision note (2026-10-02, plan 2026-10-02T05-07):** the phrase "after the seam commit" in Test Strategy and in AC15 now reads "after the seam step"; so the red observation follows the seam task, not a commit. Plan revision 1.6 (2026-10-03): the try-save seam type in D5, the impacted-functions list, the Technical specifications and the method-group sentence now names the nested delegate TrySaveAttachmentDelegate (CS1769 under the embedded interop types of UtilitiesCS); no acceptance criterion changed.

> Formatting note for later editors: inline code spans around repository paths are reserved for files this item creates, modifies or deletes, plus evidence artifacts under this feature folder. Files cited for comparison but deliberately unchanged (YesNoToAll.cs, YesNoToAllPromptSession.cs, AttachmentHelper.cs, EmailFiler.cs, AppOlObjects.cs, EfcDataModelArchiveRootTests.cs, ToDoModel.csproj, the two ToDoModel.Test files, .editorconfig, the #956 code-review record) are written as plain prose on purpose. Do not add code spans to them.

## Context
The #956 preparation research found four logic defects in the SortEmail static class. #956 split the class into partials and added testability seams; it neither fixed nor depended on these defects. The #956 review (PR #965) left six residuals that were filed as #966. The maintainer directive of 2026-10-02 folds #966 into this item because it touches the same files and component, and adds three items the #959 research found in the same files or with the same root cause. The consolidated defect and residual list, with locations on this branch (R1 section 1 and R2 file aliases A, T, U, S, M, L, E, TD):

Logic defects (issue #959):
- **L1:** `SaveCase` in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs` (lines 290 to 309) has two switch labels that bitwise-or enum values, so neither label can ever match a value the enum produces.
- **L2:** the five-argument `TrySaveAttachmentAsync` in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs` (lines 87 to 160) retries without bound when a "Yes to all" answer is held and the save keeps failing with `UnauthorizedAccessException`. This path is live in production through EmailFiler.
- **L3:** `Cleanup_Files` in the AttachmentSaving partial (lines 30 to 36) never resets `_attachmentsAltName` (line 27).
- **L4:** `WriteCSV_StartNewFileIfDoesNotExist` in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs` (lines 139 to 170) passes its `Path.Combine` arguments in reverse order (L4a), writes when the file exists instead of when it is absent (L4b), and never allocates the output array it hands to `SanitizeArray` (L4c).

Review residuals (issue #966, R2 sections 1 to 6):
- **F1:** the two overwrite prompts and the alternate-name prompt still call `YesNoToAll.ShowDialog` directly through four static enum fields (A 25 to 28; call sites A 112, 135, 175, 198, 255) instead of `YesNoToAllPromptSession`.
- **F2:** `SaveAttachmentsOld` (`UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs`, the file's only method) and `IsPicture` (A 311 to 320) have zero callers (R2 derivation N2).
- **F3:** twenty-eight `[ExcludeFromCodeCoverage]` attributes remain in the six partials; seven of them sit on members that already have tests or become testable here (R2 section 3, derivation N5).
- **CR-1:** the #956 spec's wording about the `DirectoryInfo` exception boundary no longer matches where `new DirectoryInfo` runs (R2 section 4).
- **Try-save path:** three `Debug.WriteLine` calls (T 103, 127, 147) instead of the class logger, and an outer `catch (System.Exception) { throw; }` (T 156 to 159) with no effect.
- **Partial files:** the same eighteen `using` directives are replicated into every partial; `Deedle`, `SDILReader`, the `Outlook =` alias and the self-referential `using UtilitiesCS;` are unused in every surviving file (R2 section 6, derivation N7).

Same-root-cause and same-file items (issue.md lines 30 to 33; R2 sections 1.6, 7, 8, 9):
- **EfcDataModel:** `QuickFiler/Controllers/EfcDataModel.cs` lines 308 to 309 call `Cleanup_Files` after `InvokeFilerAsync` without `try`/`finally`, so a filer exception leaves sticky prompt answers for the next operation (same root cause as L3).
- **ToDoModel duplicate:** `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` lines 285 to 315 hold a private copy of the L4 helper with the same three defects. The file is not compiled by any project and its entry method has zero callers (R2 derivations N9 and N6).
- **CSV header:** the moved-mails header is written as fourteen lines (one empty, then one column name per line) into a file whose every record is one thirteen-field tab-separated line. Ruled a defect (decision D4; R2 section 9.2, derivation N11).
- **Re-rooting defect (R2 1.6, found under the scope rule):** `SaveAttachmentAsync(this AttachmentHelper, string destinationPath)` (A 227 to 239) re-roots only `FolderPathSave`; `FilePathSaveAlt` keeps the Outlook folder name the helper was built with, so an alternate-name save on the live path resolves relative to the process working directory.

Environment:
- OS/version: Windows 11 (Outlook VSTO add-in)
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: static reading of the files named above; no command was run during research (R1 and R2 evidence basis)
- Data source or fixture: n/a

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

L2 can hang a production filing operation (one awaiting state machine per iteration, thread pool kept busy, mail never moved). The re-rooting defect writes attachments to an unintended location on the live path when the overwrite answer is No and the alternate-name answer is Yes. L3 and the EfcDataModel gap make sticky answers persist for the add-in lifetime. L1 is latent (its only caller chain has no compiled entry point). L4 makes the header seeding a permanent no-op, and the header it would write is malformed for the file's format. The remaining items are maintainability residuals with no runtime effect.


## Repro & Evidence
Steps to Reproduce (static; none has been reproduced at runtime, consistent with the issue):
1. Read `SaveCase` at lines 298 to 308 of the AttachmentSaving partial. The labels are `(YesNoToAllResponse.NoToAll | YesNoToAllResponse.No)` and `(YesNoToAllResponse.Yes | YesNoToAllResponse.YesToAll)`. The enum (UtilitiesCS/Dialogs/YesNoToAll.cs lines 14 to 21) declares Empty, Yes, No, YesToAll and NoToAll with the values zero, one, two, four and eight and no `[Flags]` attribute; every value that reaches the parameter is one of those five, so both labels are dead and every call takes `default` (R1 2.1).
2. Read the `UnauthorizedAccessException` handler at lines 101 to 155 of the TrySaveAttachment partial and trace the recursion at lines 134 to 140 with `Response == YesToAll`, `clearReadOnly` succeeding and `SaveAsFile` throwing on every call (R1 3.2): line 108 is false after the first call, lines 115 to 118 are true on every call, line 132 keeps `YesToAll`, line 134 recurses with identical arguments. No counter, no changing state, no terminating condition.
3. Read `Cleanup_Files` at lines 30 to 36 of the AttachmentSaving partial: it assigns `Empty` to `_responseSaveFile`, `_attachmentsOverwrite` and `_picturesOverwrite` and calls `RemoveReadOnlyPrompt.Reset()`; `_attachmentsAltName` is absent (R1 derivation N3: four fields, three reset).
4. Read `WriteCSV_StartNewFileIfDoesNotExist` at lines 145 to 169 of the UndoAndMoveLog partial: `File.Exists(Path.Combine(strFileName, strFileLocation))` (reversed; the sole caller AppOlObjects.cs lines 301 to 304 passes a file name then a rooted folder), the body runs when the check is true (inverted), and `strOutput` is `null` at line 145 and never allocated before `SanitizeArray` writes `strOutput![j]` at line 183. Read lines 149 to 165: the header is a `string[14, 2]` with only column one of rows one to thirteen set, joined row by row, so the output is fourteen lines. Compare with the record writer EmailFiler.cs lines 191 to 199 and 453 to 461, which writes one thirteen-field tab-separated line per record (R2 9.1).
5. Read A 227 to 239 and AttachmentHelper.cs lines 174 to 203: the destination overload sets `FolderPathSave` only; `FilePathSaveAlt` is backed by a separate `FilePathHelperSaveAlt` that is never re-rooted. The live constructor calls (MailItemHelper.Properties.cs 255, MailItemHelper.cs 159) pass the Outlook folder name, so after re-rooting `FilePathSaveAlt` is a relative path (R2 1.6).
6. Read EfcDataModel.cs lines 308 to 310: `var result = await InvokeFilerAsync(config, mailHelpers); SortEmail.Cleanup_Files(); return result;` with no `try`/`finally` (R2 7.1).
7. Grep `SortItemsToExistingFolder` over all project files: ToDoModel.csproj has no `Compile Include` for the file; only the two ToDoModel.Test test files are listed (R2 derivation N9). Grep `SaveAttachmentsOld|IsPicture\b` over all C# files: two declaration hits, zero callers (R2 derivation N2).

Expected:
- `SaveCase`: No or NoToAll saves to `filePathSaveAlt`; Yes or YesToAll saves to `filePathSave`; Empty saves nothing.
- `TrySaveAttachmentAsync`: a denial that persists after the read-only attribute has been cleared under a held YesToAll answer ends the retry and surfaces the error to the caller.
- `Cleanup_Files` resets every prompt session, including the alternate-name prompt.
- `WriteCSV_StartNewFileIfDoesNotExist` writes one tab-separated header line of the thirteen column names to the combination of folder then file name, only when that file is absent, without throwing.
- The destination overload re-roots both the primary and the alternate save path.
- `EfcDataModel.MoveToFolderAsync` resets the prompt state whether the filer returns or throws.
- No dead member, dead file, unused directive or stale exclusion remains in the SortEmail partials; the #956 spec describes the current `DirectoryInfo` boundary.

Actual:
- `SaveCase` never saves on either branch.
- `TrySaveAttachmentAsync` loops without bound in the stated state.
- `Cleanup_Files` leaves `_attachmentsAltName` at its last value.
- `WriteCSV_StartNewFileIfDoesNotExist` is a no-op; once L4a and L4b alone were fixed it would throw `NullReferenceException` on the first header row, and once L4c were also fixed it would write a fourteen-line header.
- The alternate save path stays relative after re-rooting.
- A filer exception skips `Cleanup_Files`.
- The residuals listed in Context are present as described.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: #956 research findings L1 to L4 (feature folder for issue #956 under docs/features/active, research subfolder); R1 sections 2.1, 3.1, 4.1 and 5.1 and R2 sections 1.1, 1.6, 5.1, 7.1, 8.1 and 9.1 quote the current code with line numbers.


## Scope & Non-Goals
- In scope (every item below is fixed in this item; none is a follow-up):
  - L1: the `SaveCase` labels and removal of its `[ExcludeFromCodeCoverage]` (D1).
  - L2: a bounded retry in the five-argument try-save path that surfaces a persistent denial by rethrow, with the handler's `Debug.WriteLine` calls replaced by the class logger and the outer rethrow removed (D2).
  - L3: in two phases, the missing `_attachmentsAltName` reset against the current fields, then the session-based cleanup under F1 (D3, D5).
  - L4 together with the CSV header defect: a delegate seam, the corrected path and condition, a single tab-separated header line, and deletion of `SanitizeArray` with its test (D4).
  - F1: three `YesNoToAllPromptSession` instances for the overwrite and alternate-name prompts, a call-time `AllPromptSessions` property, seamed asynchronous and synchronous cores, deletion of `_responseSaveFile` (D5).
  - The re-rooting defect of R2 1.6 through an `internal RedirectSaveFolder` helper (D6).
  - F2: deletion of `SaveAttachmentsOld`, `IsPicture`, the LegacyAttachmentSaving partial and its project-file entry (D7).
  - F3: the exclusion dispositions of R2 section 3 and two added `GetAttachmentsInfo` rows (D8).
  - CR-1: the three text edits to the #956 spec (D9).
  - Using directives: the final per-file blocks of R2 6.2 (D10).
  - EfcDataModel: `try`/`finally` with a `ResetFilerPromptState` seam and a new QuickFiler.Test file (D11).
  - ToDoModel: deletion of the uncompiled duplicate file (D12).
  - Regression tests, project-file registrations and feature-folder evidence for all of the above (D13 to D17).
- Out of scope / non-goals (paths deliberately unbackticked):
  - Completely unrelated defects. R2 reports that none was found (R2 banner, scope rule applied); if execution finds one, it is reported for filing, not fixed here.
  - Retirement of the synchronous chain `Sort`, `SaveAttachment`, `SaveCase`, `GetAttachmentsInfo`, `SaveMessageAsMSG` and `ResolvePaths(IList<MailItem>, ...)`. The chain has no compiled caller (R2 derivation N6) but dead code is not a defect, the members are public, and the issue asks for L1 to be fixed, not removed (R2 1.7). The chain is routed through the sessions and seamed so L1 is exercised from its real caller; its retirement is a separate maintainer decision.
  - Editing the #956 code-review record (code-review.2026-10-01T22-24.md) or changing the state of any checked #956 acceptance criterion. CR-1 edits only the two sentences and adds the dated note of D9.
- Explicitly excluded systems, integrations, or datasets: Outlook runtime, the real file system, and the modal dialog; no test touches any of them.

Scope decisions required by issue.md and resolved here: the CSV header is ruled a defect (D4, rationale in Root Cause Analysis); the synchronous chain is kept (above); the ToDoModel file is deleted rather than edited (D12).

## Root Cause Analysis
These are legacy code paths that had no test coverage, partly because of `[ExcludeFromCodeCoverage]` attributes and dialog or file-system dependencies. #956 has merged on this branch's history, so the seams it added (the five-argument try-save overload, the `YesNoToAllPromptSession` type, `InternalsVisibleTo` for UtilitiesCS.Test) are available (R1 1.3). QuickFiler.Test has no `InternalsVisibleTo` grant (R2 7.1), which is why the EfcDataModel fix needs its own seam.

- **L1.** A bitwise-or of two enum constants is itself a constant of the enum type, so the compiler accepts `NoToAll | No` (value ten) and `Yes | YesToAll` (value five) as case labels. The author intended "either of these two values", which in C# is expressed by stacked labels or a `when` guard (the asynchronous twin `SaveCaseAsync` uses the guard form correctly).
- **L2.** The retry is a recursive call with the same five arguments. The design relies on `ReleaseSingleAnswer` to force a re-prompt after a single Yes, but a YesToAll answer is by definition never released, so nothing in the loop changes between iterations. Returning `false` would not help: every production caller discards the boolean (R1 3.3), so the mail would be moved with its attachment silently unsaved.
- **L3, F1 and EfcDataModel (one root cause).** Each prompt site hand-rolls the pattern "prompt while the field is Empty; use the answer; release Yes or No" against a private static enum field, and the cleanup list is a separate hand-maintained enumeration of those fields. `_attachmentsAltName` was added without extending the list; the cleanup call itself sits outside any `finally`, so an exception bypasses it. The `YesNoToAllPromptSession` type already encapsulates ask, release and reset for the read-only prompt; routing the other three prompts through it and resetting through one enumerated collection removes both hand-maintained lists, and the `finally` closes the exception path.
- **L4 and the header.** Three independent errors mask each other: the reversed `Path.Combine` makes `File.Exists` test a directory path (false), the inverted condition then skips the body, and the body, were it reached, dereferences a null array. The two-dimensional header array is a transliteration artifact of the VBA-era `strAryOutput(1 To 13, 1 To 1)` shape (the ToDoModel copy carries the same); the names and their order match the thirteen record fields one-to-one (R2 derivation N11), which shows that a single header row was intended. A fourteen-line header would be read by any TSV consumer as fourteen malformed records.
- **Re-rooting (R2 1.6).** `AttachmentHelper` holds two independent `FilePathHelper` instances for the primary and alternate paths; the destination overload was written when only the primary path was re-rooted and was never extended when the alternate path was added.
- **F2, ToDoModel duplicate.** Copy-and-rename evolution left a superseded method, a superseded private helper and a whole uncompiled file in place with no callers.
- **F3, usings, try-save logging, CR-1.** The #956 split replicated the full using block into every partial and preserved exclusions on members that its own tests had started to cover; the handler predates the class logger; the CR-1 sentence was written against merge-base line numbers before the adapter moved the construction inside the inner `try`.


## Proposed Fix

### Design summary (what changes where):

The orchestrator decisions below are binding on planning and execution. Each is recorded with its rationale. They replace the D1 to D10 list of revision 0.1.

- **D1 (L1).** Replace the two combined labels in `SaveCase` with four stacked single-value labels: `YesNoToAllResponse.NoToAll` and `YesNoToAllResponse.No` fall through to one `attachment.SaveAsFile(filePathSaveAlt)`; `YesNoToAllResponse.Yes` and `YesNoToAllResponse.YesToAll` fall through to one `attachment.SaveAsFile(filePathSave)`; `Empty` reaches `default` and makes no call. Remove `[ExcludeFromCodeCoverage]` from `SaveCase`. Do not use `HasFlag`. The three L1 tests live in the new file `UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs` together with the `SaveCaseAsync` tests (R2 0.2), not in the AttachmentSaving test file as revision 0.1 planned, because of the five-hundred-line limit once the F1 tests exist. Rationale: the enum is not a flags enum and no caller passes a combination; `HasFlag` would make `Empty` (zero) match every label; the method has no dialog or file-system call once `SaveAsFile` is mocked (R1 2.3, 2.4).
- **D2 (L2 and the try-save path).** The internal five-argument `TrySaveAttachmentAsync` keeps its exact signature and becomes a forward to a new `private static TrySaveAttachmentCoreAsync` taking the same five arguments plus `bool isRetryAfterClear`. In the `UnauthorizedAccessException` handler, before the prompt check and before any further clear, when `isRetryAfterClear` is true and the session holds `YesToAll`, the handler calls `logger.Error` (message naming the path, plus the exception) and rethrows with a bare `throw;`. The recursion passes `isRetryAfterClear: true`. Per R2 section 5: the outer `catch (System.Exception) { throw; }` is removed, and the three `Debug.WriteLine` calls become `logger.Warn` (first handler statement, with the exception), `logger.Error` (clear failure, with the inner exception) and `logger.Warn` (declined save), with the message texts of R2 5.2 as proposals the plan may adjust. No test asserts log output (R2 5.3: log4net without configuration makes the calls no-ops, and a root-logger appender under class-level parallelism receives events from other classes). The regression test is T12 of R1 3.5 with the `CreateDirectoryLimit` tripwire. Rationale: rethrow is the only outcome a production caller observes (R1 3.3); the same exception already propagates on the Cancel path; the condition checks the held answer because after a single Yes the answer is released and the retry must re-prompt (T11); a private core keeps the signature that eleven existing tests and the three-argument forward pin byte-identical (R1 3.4); the outer catch has no effect on propagation and T10 pins that (R2 5.1, CR956 CR-3); the logger is the project logging pattern (CLAUDE.md C#4.2).
- **D3 (L3, two phases; R2 1.5).** Phase one, against the current fields: add `_attachmentsAltName = YesNoToAllResponse.Empty;` to `Cleanup_Files` and write the four-row reflection test `Cleanup_Files_ResetsEveryPromptAnswerField` in `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs`, observed red on the `_attachmentsAltName` row, then green; the UT5 call-out for its reflective static write applies to this phase only. Phase F1 (D5) replaces the fields with sessions and replaces that test, in the same file, with the structural test `Cleanup_Files_ResetsEveryPromptSession`, which only reads static state, plus a recorded negative control (one element removed from `AllPromptSessions`, test observed failing, element restored). Rationale: the bugfix workflow requires a red test against the defect as it exists; the F1 design makes the enum fields disappear, so the final invariant ("every static session is reset") needs a test that survives the refactor; a session's `Response` has a private setter and `Ask` on a production instance shows the dialog, so a behavioral test of the production sessions is not possible, and the behavioral half (stickiness and release) is covered by the `SaveCaseAsync` and `SaveAttachmentAsync` tests with test-owned sessions.
- **D4 (L4 and the header; R2 9.3).** Add an internal four-argument overload `WriteCSV_StartNewFileIfDoesNotExist(string strFileName, string strFileLocation, Func<string, bool> fileExists, Action<string, string[], string> writeTextFile)`. The public two-argument overload becomes a one-statement forward passing `File.Exists` and `FileIO2.WriteTextFile` as method groups and keeps `[ExcludeFromCodeCoverage]` with a justification comment. The core returns when `fileExists(Path.Combine(strFileLocation, strFileName))` is true; otherwise it writes once, through `writeTextFile(strFileName, lines, strFileLocation)`, a single line that is the tab-join of a new `private static readonly string[] MovedMailsHeader` holding the thirteen names in `Details` index order (Triage, FolderName, Sent_On, From, To, CC, Subject, Body, fromDomain, Conversation_ID, EntryID, Attachments, FlaggedAsTask). The header shape is ruled a defect (R2 9.2). `SanitizeArray` loses its last caller and is deleted together with `SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows` in `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs`; `SanitizeArrayLineTSV` and `StripTabsCrLf` stay. L4c disappears by construction. Red-first is behavioral: the seam lands first as a pure forward of the current body, both L4 tests are observed red, then the logic fix is applied. Rationale: without a seam the only test would touch the real disk, which UT4 prohibits with no approved exception, so the seam is a precondition of the mandated workflow (R1 5.5); method groups keep closures out of the excluded wrapper (R1 6.5); the file is a .tsv whose record writer emits one thirteen-field line per record and no reader exists in the repository, so the single-line header has no consumer to break (R2 9.1); fixing L4a and L4b without the header change would seed a malformed header on first use.
- **D5 (F1; R2 1.3).** Replace the enum fields with three `private static readonly YesNoToAllPromptSession` fields `AttachmentsOverwritePrompt`, `PicturesOverwritePrompt` and `AttachmentsAltNamePrompt`, each initialized with the `YesNoToAll.ShowDialog` method group; add a `private static` call-time property `AllPromptSessions` returning an array of those three plus `RemoveReadOnlyPrompt`; `Cleanup_Files` iterates it and calls `Reset` on each. Delete `_responseSaveFile` (its only readers are inside `SaveAttachmentsOld`, R2 derivation N1). Seam the asynchronous path: `SaveAttachmentAsync(this AttachmentHelper)` becomes an excluded one-statement forward to an `internal static async Task SaveAttachmentAsync(AttachmentHelper, Func<string, bool> fileExists, YesNoToAllPromptSession picturesOverwritePrompt, YesNoToAllPromptSession attachmentsOverwritePrompt, YesNoToAllPromptSession altNamePrompt, TrySaveAttachmentDelegate trySave)` core; `trySave` has the nested non-generic delegate type `internal delegate Task<bool> TrySaveAttachmentDelegate(Attachment attachment, string filePath)` declared in `SortEmail` (a generic delegate over the embedded interop type `Attachment` cannot be used from UtilitiesCS.Test, compiler error CS1769; plan PD-14); `SaveCaseAsync` becomes `internal static async Task SaveCaseAsync(YesNoToAllResponse, Attachment, string filePathSave, string filePathSaveAlt, YesNoToAllPromptSession altNamePrompt, TrySaveAttachmentDelegate trySave)` and the four-argument form is removed. Seam the synchronous path: `SaveAttachment(this AttachmentHelper)` becomes an excluded one-statement forward to an `internal static void SaveAttachment(AttachmentHelper, Func<string, bool> fileExists, YesNoToAllPromptSession picturesOverwritePrompt, YesNoToAllPromptSession attachmentsOverwritePrompt)` core that calls `SaveCase`. Wrappers pass method groups and field references, never lambdas; the overwrite-session selection by `IsImage` stays inside the measured cores. The synchronous chain is kept (R2 1.7). Tests per R2 1.8 in `UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs` and `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs`. Rationale: the session API performs exactly the field pattern at every site, so the mapping is behavior-preserving (R2 1.2); a property rather than an array field avoids the unspecified static-initializer order across partial files that could capture a null `RemoveReadOnlyPrompt`; names follow the #956 `RemoveReadOnlyPrompt` precedent; among the three `TrySaveAttachmentAsync` overloads only the two-argument one converts to the `trySave` delegate type, so the method-group conversion is unambiguous (inference, confirmed by the compiler); rejected alternatives (holder type, settable static seam, selection in the wrapper, internal accessor) are listed in R2 1.3.
- **D6 (re-rooting defect; R2 1.6).** Add `internal static void RedirectSaveFolder(AttachmentHelper attachmentHelper, string destinationPath)` that assigns `attachmentHelper.FolderPathSave` and `attachmentHelper.FilePathHelperSaveAlt.FolderPath`; the destination overload calls it and forwards to the parameterless wrapper, keeping `[ExcludeFromCodeCoverage]`. Two-step red-first: land the helper with only the existing statement (behavior-preserving extraction), observe `RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths` red on the alternate-path assertion, add the second statement, green. Rationale: `FilePathHelperSaveAlt` is already `internal`, so AttachmentHelper.cs is not changed; the `FilePathHelper` setters are string operations, so the test touches no disk.
- **D7 (F2; R2 section 2).** Delete `SaveAttachmentsOld` and `IsPicture`. `SortEmail.LegacyAttachmentSaving.cs` then holds only usings, an unused `MAX_PATH` constant and an empty partial declaration (R2 derivation N3), so the file `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs` is deleted and its `Compile Include` line is removed from `UtilitiesCS/UtilitiesCS.csproj`. No regression test (zero callers, R2 derivation N2); fail-before exception dossier; the rebuild is the verification.
- **D8 (F3; R2 section 3).** Of the twenty-eight attributes in the six partials: remove seven (`GetAttachmentsInfo`, `GetAttachmentsInfoAsync`, `SaveCaseAsync`, `SaveCase`, `SaveMessageAsMsgAsync`, `SaveMessageAsMSG`, `SanitizeArrayLineTSV`); three leave with their members (`IsPicture`, `SanitizeArray`, `SaveAttachmentsOld`); keep eighteen (the three attachment-saving wrappers, the two-argument `TrySaveAttachmentAsync`, `ClearReadOnlyAttributeOnDisk`, `UndoAsync`, `PushToUndoStack`, `CaptureMoveDetails`, the two-argument `WriteCSV_StartNewFileIfDoesNotExist`, the four `SortAsync` overloads, `Sort`, `UpdatePredictiveEngineAsync`, `ProcessMailItemAsync`, both `ResolvePaths` overloads). Add one `DataRow` (saveAttachments true, savePictures true) to each of the two existing `GetAttachmentsInfo` tests in `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` so both `if` bodies are covered. Rationale: a member loses its exclusion only when it is fully exercisable without the file system, a dialog or an Outlook process; the kept members are one-statement wiring wrappers or Outlook-interop orchestration whose removal would add tens of uncovered lines (R2 section 3, item nineteen).
- **D9 (CR-1; R2 section 4).** In `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md`: replace the "Boundaries and invariants" bullet at line 155 and the trailing clause of the D2 item-three sentence at line 99 with the corrected texts quoted in Technical specifications, and insert the dated supersession note quoted there after the "Must not be widened." paragraph at line 149. No other change to that file; no checkbox state changes; the #956 code-review record is not edited. Rationale: `new DirectoryInfo` now runs inside the inner `try` through `ClearReadOnlyAttributeOnDisk` (R2 4.1); the "Must not be widened" and AC6 constraints of #956 are superseded by D2 and must say so.
- **D10 (usings; R2 6.2).** Each surviving partial's using block becomes exactly the block listed in Technical specifications (A eight, T five, U ten, S nine, M nine directives, predicted). Verification is the compiler: IDE0005 is at suggestion severity and is not reported in a command-line build, so the two msbuild rebuilds are the only enforced detection of a wrong block (R2 6.1). `using System;` stays in every file so that `Exception` never binds to the interop interface.
- **D11 (EfcDataModel; R2 section 7).** In `QuickFiler/Controllers/EfcDataModel.cs`, wrap the `InvokeFilerAsync` await in `try`/`finally` whose `finally` calls a new `protected internal virtual void ResetFilerPromptState()` whose body is `SortEmail.Cleanup_Files();`. New file `QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs` with a nested probe overriding `InvokeFilerAsync` and `ResetFilerPromptState` and three tests (`MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates`, `MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce`, `MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState`); `Compile Include` added to `QuickFiler.Test/QuickFiler.Test.csproj`. Two-step red-first: extract the seam without the `finally`, observe the throwing test red with a zero reset count, add the `finally`, green. Rationale: QuickFiler.Test cannot name `YesNoToAllPromptSession` or any `SortEmail` internal and cannot put a non-Empty answer into a production session, so a virtual seam on `EfcDataModel` (the #736 `InvokeFilerAsync` pattern) is the only dialog-free observation point; `Cleanup_Files` cannot throw, so the `finally` cannot mask the original exception; the existing EfcDataModelArchiveRootTests keep exercising the production seam body.
- **D12 (ToDoModel duplicate; R2 section 8).** Delete `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs`. No project-file change (no project lists it); the two ToDoModel.Test files reference the type only in comments and are unchanged. No regression test (fail-before exception: uncompiled code with zero callers). Rationale: fixing or delegating code the compiler never checks would be unverifiable; the F2 precedent (dead code with zero callers is deleted rather than seamed) applies; the alternative of re-pointing the copy to `SortEmail` is recorded in R2 8.2, not recommended.
- **D13 (bugfix workflow).** Every defect's regression test (L1, L2, L3 phase one, L4, re-rooting, EfcDataModel) is written first and observed failing, tagged `[expect-fail]` in the plan, with fail-before evidence under `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/`. Refactor-only steps with no observable defect (usings, logging, F2 deletions, F3 exclusion changes, ToDoModel deletion, CR-1, the F1 seam extraction and structural-test replacement) carry a fail-before exception dossier entry instead, naming the pin tests or the compiler as verification.
- **D14 (test policy).** MSTest, Moq, FluentAssertions; no temporary files, no real file-system reads or writes, no dialogs, no `[DoNotParallelize]`, no Workers set to one, no retries, no `Thread.Sleep`, `Task.Delay` or timeouts; the sandbox literal C:\Sortemail959Sandbox is only ever passed to recording delegates or mocks and is never created; the only static write in any test is the transient phase-one L3 test, which no longer exists in the final tree. Tests pass under scripts/vscode/TaskMaster.cli.runsettings (Workers zero, class-level scope).
- **D15 (write set).** Exactly the eighteen paths of R2 section 10 (table in Implementation strategy) plus this feature folder. Deliberately unchanged: UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs, UtilitiesCS/Dialogs/YesNoToAll.cs, UtilitiesCS/OutlookObjects/Attachment/AttachmentHelper.cs, UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs, TaskMaster/AppGlobals/AppOlObjects.cs, QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs, both ToDoModel.Test files, ToDoModel/ToDoModel.csproj, .editorconfig, and the #956 code-review record.
- **D16 (coverage).** CLAUDE.md floors (C# line at least eighty percent, branch at least seventy-five percent, on the testable denominator per UT2); new or changed members at least ninety percent line coverage; changed lines must not lose coverage; unreachable braces after a rethrow in the try-save file are exempt only when identified by file and content in the comparison projection, re-derived per R2 5.4 (expected: the wrapper lambda, the brace after `throw;` in the final `else`, the closing brace of the `UnauthorizedAccessException` catch re-described as the last statement-level brace before the method's closing brace, and the predicted brace after the new guard's `throw;`).
- **D17 (toolchain and evidence).** Exactly CLAUDE.md's four C# commands (listed in Test Strategy). Evidence under this feature folder's `evidence/baseline/`, `evidence/regression-testing/` and `evidence/qa-gates/`; committed test evidence is only the JaCoCo package-level projection, the one-line first-party summary and TRX-derived summaries, all Markdown; never raw XML, TRX or coverage documents.
- **D18 (closure).** The pull request closes both #959 and #966.

### Boundaries and invariants to preserve:

- The internal five-argument `TrySaveAttachmentAsync` signature (`this Attachment attachment, string filePathSave, Action<string> createDirectory, Action<string> clearReadOnly, YesNoToAllPromptSession removeReadOnlyPrompt`, returning `Task<bool>`) is byte-identical after the change. The two-argument and three-argument overloads and `ClearReadOnlyAttributeOnDisk` keep their bodies and their `[ExcludeFromCodeCoverage]` attributes.
- `createDirectory` is still called as the first statement of the try block, before every save attempt (pinned by SortEmail_Tests.cs lines 273 to 289).
- Try-save outcome table after the change (R1 section 7, R2 section 12): save succeeds on first or retried attempt returns true; denial with No or NoToAll returns false and releases a single answer; denial with Yes or YesToAll followed by a throwing clear returns false; denial with Yes, successful clear, denied retry re-prompts (T11); denial with YesToAll, successful clear, denied retry logs and rethrows the original `UnauthorizedAccessException` after exactly one clear, two save attempts and one prompt, leaving YesToAll held until `Cleanup_Files`; Cancel (Empty) rethrows (T7); a non-access exception propagates (T10).
- The public signatures `SaveAttachment(this AttachmentHelper)`, `SaveAttachmentAsync(this AttachmentHelper)`, `SaveAttachmentAsync(this AttachmentHelper, string)`, `Cleanup_Files()` and `WriteCSV_StartNewFileIfDoesNotExist(string, string)` are unchanged, so SortEmail.MailItemSort.cs line 153 and 299, SortEmail.cs line 165, EmailFiler.cs line 445, EfcDataModel.cs line 309 (now inside the seam) and AppOlObjects.cs line 301 compile without edits other than those listed.
- The three prompt message texts are unchanged. The synchronous path still does not show the alternate-name prompt (pre-existing).
- `EfcDataModel.MoveToFolderAsync`'s five-parameter signature and its three guard returns are unchanged; a guard return still performs no reset.
- The header column names and their order are unchanged; only their layout (one tab-separated line) changes.
- All surviving UtilitiesCS partials carry `#nullable enable`; new code must be null-clean under the nullable gate. EfcDataModel.cs carries no pragma.

### Dependencies or blocked work:

- #956 is merged into this branch's history: the partial files, the five-argument overload, `YesNoToAllPromptSession` and the `InternalsVisibleTo` entries exist on disk (verified by reading them). Nothing blocks execution.
- Decisions the orchestrator has issued and this spec records: the L4 seam (D4), the two-phase L3 with a reflective static write in phase one (D3), the header ruled a defect (D4), the synchronous chain kept (D5), the ToDoModel deletion (D12), the cross-project write set (D11, D12, D15).

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

The write set is exactly these eighteen paths plus this feature folder (R2 section 10). "Now" is the last line the Read tool printed; "after" is a prediction to be measured. Limit five hundred lines per C# file.

| # | File | Action | Project | Now / after |
|---|---|---|---|---|
| 1 | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs` | modify: F1 sessions, `AllPromptSessions`, seamed cores, `RedirectSaveFolder`, L1 labels, L3 phase-one reset (transient), delete `IsPicture` and `_responseSaveFile`, F3 removals, usings | UtilitiesCS | 343 / about 330 |
| 2 | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs` | modify: forward plus private core with the guard; logger calls; outer catch removed; usings | UtilitiesCS | 173 / about 205 |
| 3 | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs` | modify: seamed CSV core, `MovedMailsHeader`, `SanitizeArray` deleted, F3 removal on `SanitizeArrayLineTSV`, usings | UtilitiesCS | 196 / about 165 |
| 4 | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` | modify: usings only | UtilitiesCS | 277 / about 268 |
| 5 | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs` | modify: usings only | UtilitiesCS | 388 / about 379 |
| 6 | `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs` | delete (F2) | UtilitiesCS | 240 / 0 |
| 7 | `UtilitiesCS/UtilitiesCS.csproj` | remove the `Compile Include` line for file 6 (line 820) | UtilitiesCS | n/a |
| 8 | `QuickFiler/Controllers/EfcDataModel.cs` | modify: `try`/`finally`, `ResetFilerPromptState` | QuickFiler | 465 / about 482 |
| 9 | `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` | delete (uncompiled duplicate) | ToDoModel | 403 / 0 |
| 10 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` | modify: delete the `SanitizeArray` test (lines 359 to 382); add one `DataRow` to each `GetAttachmentsInfo` test | UtilitiesCS.Test | 458 / about 440 |
| 11 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` | modify: `CreateDirectoryLimit` tripwire on `Seams`; test T12 | UtilitiesCS.Test | 376 / about 425 |
| 12 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs` | create: three `SaveCase` tests, six `SaveCaseAsync` tests | UtilitiesCS.Test | 0 / about 250 |
| 13 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` | create: phase-one cleanup test (transient), six asynchronous-core tests, three synchronous-core tests, re-rooting test, structural cleanup test | UtilitiesCS.Test | 0 / about 340 |
| 14 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs` | create: two CSV tests | UtilitiesCS.Test | 0 / about 130 |
| 15 | `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | add three `Compile Include` items after the existing SortEmail test entries (lines 98 to 99), same form (four-space indent, backslash separators, self-closing) | UtilitiesCS.Test | n/a |
| 16 | `QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs` | create: probe and three tests | QuickFiler.Test | 0 / about 160 |
| 17 | `QuickFiler.Test/QuickFiler.Test.csproj` | add one `Compile Include` item after line 127 | QuickFiler.Test | n/a |
| 18 | `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md` | modify: lines 99 and 155; note after line 149 | docs | n/a |

Cross-project footprint: files 8, 16 and 17 (QuickFiler, QuickFiler.Test) and file 9 (ToDoModel). No caller outside the SortEmail partials and EfcDataModel changes.

#### Functions/classes/CLI commands impacted:

- `SortEmail.SaveCase(YesNoToAllResponse, Attachment, string, string)`: labels corrected; exclusion removed.
- `SortEmail.SaveCaseAsync(YesNoToAllResponse, Attachment, string, string, YesNoToAllPromptSession, SortEmail.TrySaveAttachmentDelegate)`: new internal core replacing the four-argument form; exclusion removed.
- `SortEmail.SaveAttachmentAsync(AttachmentHelper, Func<string, bool>, YesNoToAllPromptSession, YesNoToAllPromptSession, YesNoToAllPromptSession, SortEmail.TrySaveAttachmentDelegate)`: new internal core; `SaveAttachmentAsync(this AttachmentHelper)` becomes its excluded forward.
- `SortEmail.SaveAttachment(AttachmentHelper, Func<string, bool>, YesNoToAllPromptSession, YesNoToAllPromptSession)`: new internal core; `SaveAttachment(this AttachmentHelper)` becomes its excluded forward.
- `SortEmail.RedirectSaveFolder(AttachmentHelper, string)`: new internal; `SaveAttachmentAsync(this AttachmentHelper, string)` calls it.
- `SortEmail.AttachmentsOverwritePrompt`, `PicturesOverwritePrompt`, `AttachmentsAltNamePrompt`: new private static readonly sessions; `AllPromptSessions`: new private static property; `Cleanup_Files()`: body becomes a `foreach`; `_responseSaveFile`, `_attachmentsOverwrite`, `_attachmentsAltName`, `_picturesOverwrite`: deleted (after phase one).
- `SortEmail.TrySaveAttachmentAsync(this Attachment, string, Action<string>, Action<string>, YesNoToAllPromptSession)`: body becomes a forward. `SortEmail.TrySaveAttachmentCoreAsync(..., bool isRetryAfterClear)`: new, private, carries the former body plus the guard and logger calls, one catch clause.
- `SortEmail.WriteCSV_StartNewFileIfDoesNotExist(string, string)`: body becomes an excluded forward. `SortEmail.WriteCSV_StartNewFileIfDoesNotExist(string, string, Func<string, bool>, Action<string, string[], string>)`: new, internal. `SortEmail.MovedMailsHeader`: new private static readonly array. `SortEmail.SanitizeArray`: deleted.
- `SortEmail.SaveAttachmentsOld`, `SortEmail.IsPicture`, `SortEmail.MAX_PATH`: deleted.
- `SortEmail.GetAttachmentsInfo`, `GetAttachmentsInfoAsync`, `SaveMessageAsMsgAsync`, `SaveMessageAsMSG`, `SanitizeArrayLineTSV`: exclusion removed, bodies unchanged.
- `EfcDataModel.MoveToFolderAsync(string, bool, bool, bool, bool)`: `try`/`finally` around the filer call. `EfcDataModel.ResetFilerPromptState()`: new protected internal virtual.
- Test classes: `SortEmail_TrySaveAttachment_Tests` (extended), `SortEmail_Tests` (one test removed, two rows added), `SortEmail_SaveCase_Tests` (new), `SortEmail_AttachmentSaving_Tests` (new), `SortEmail_UndoAndMoveLog_Tests` (new), `EfcDataModelFilerCleanupTests` (new).

#### Data flow and validation changes:

- Header seeding: the existence check queries the folder-then-file combination; when absent, one line `Triage\tFolderName\tSent_On\tFrom\tTo\tCC\tSubject\tBody\tfromDomain\tConversation_ID\tEntryID\tAttachments\tFlaggedAsTask` is written. Existing files are not rewritten.
- Re-rooting: on the live path both `FilePathSave` and `FilePathSaveAlt` are now under `Config.SaveFsPath`; previously the alternate path was relative to the Outlook folder name.
- L2: only the terminating condition of the retry changes.
- F1: the answers travel in session instances rather than enum fields; the ask/use/release sequence at each site is unchanged (R2 1.2).
- EfcDataModel: the reset now also runs when the filer throws.
- No other data-flow change.

#### Error handling and logging updates:

- The try-save handler uses the class logger (the log4net field declared in `SortEmail.cs` lines 25 to 27, visible to every partial) at three existing sites and one new site, per the replacement table in Technical specifications; the outer no-op rethrow is removed; `using System.Diagnostics;` leaves the file.
- A short "why" comment above the new guard states that the attribute was already cleared once in this call and a YesToAll answer is never asked again, so another clear-and-retry cannot change the outcome.
- EfcDataModel: a "why" comment inside the `finally` names the sticky-answer root cause.
- No other error-handling change.

#### Rollback/feature-flag considerations (if applicable):

- No feature flag. Rollback is a revert of the fix commits; every public signature used by a caller is unchanged, so a revert has no caller impact. The CR-1 edit and the file deletions revert with the same commits.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

```csharp
// SortEmail.AttachmentSaving.cs: fields and cleanup (replaces lines 25 to 36)
private static readonly YesNoToAllPromptSession AttachmentsOverwritePrompt = new(YesNoToAll.ShowDialog);
private static readonly YesNoToAllPromptSession PicturesOverwritePrompt = new(YesNoToAll.ShowDialog);
private static readonly YesNoToAllPromptSession AttachmentsAltNamePrompt = new(YesNoToAll.ShowDialog);

// A property, not an array field: a static field initializer in this partial could run before the
// RemoveReadOnlyPrompt initializer in SortEmail.TrySaveAttachment.cs and capture null.
private static YesNoToAllPromptSession[] AllPromptSessions =>
    new[] { AttachmentsOverwritePrompt, PicturesOverwritePrompt, AttachmentsAltNamePrompt, RemoveReadOnlyPrompt };

public static void Cleanup_Files()
{
    foreach (var prompt in AllPromptSessions)
    {
        prompt.Reset();
    }
}

// SortEmail.AttachmentSaving.cs: asynchronous path
[ExcludeFromCodeCoverage] // wiring only: real File.Exists, production sessions, real directory creation (UT4)
public static Task SaveAttachmentAsync(this AttachmentHelper attachmentHelper)
{
    return SaveAttachmentAsync(attachmentHelper, File.Exists, PicturesOverwritePrompt,
        AttachmentsOverwritePrompt, AttachmentsAltNamePrompt, TrySaveAttachmentAsync);
}

internal static async Task SaveAttachmentAsync(
    AttachmentHelper attachmentHelper,
    Func<string, bool> fileExists,
    YesNoToAllPromptSession picturesOverwritePrompt,
    YesNoToAllPromptSession attachmentsOverwritePrompt,
    YesNoToAllPromptSession altNamePrompt,
    TrySaveAttachmentDelegate trySave
)
{
    if (!fileExists(attachmentHelper.FilePathSave))
    {
        await trySave(attachmentHelper.Attachment, attachmentHelper.FilePathSave);
        return;
    }
    var overwritePrompt = attachmentHelper.AttachmentInfo.IsImage ? picturesOverwritePrompt : attachmentsOverwritePrompt;
    var answer = overwritePrompt.Ask($"The file {attachmentHelper.FilePathSave} already exists. Overwrite?");
    await SaveCaseAsync(answer, attachmentHelper.Attachment, attachmentHelper.FilePathSave,
        attachmentHelper.FilePathSaveAlt, altNamePrompt, trySave);
    overwritePrompt.ReleaseSingleAnswer();
}

internal static async Task SaveCaseAsync(
    YesNoToAllResponse response,
    Attachment attachment,
    string filePathSave,
    string filePathSaveAlt,
    YesNoToAllPromptSession altNamePrompt,
    TrySaveAttachmentDelegate trySave
)
{
    switch (response)
    {
        case YesNoToAllResponse r when (r == YesNoToAllResponse.NoToAll || r == YesNoToAllResponse.No):
            var altAnswer = altNamePrompt.Ask($"The file {filePathSave} already exists. Save with an alternate name?");
            if (altAnswer == YesNoToAllResponse.Yes || altAnswer == YesNoToAllResponse.YesToAll)
            {
                await trySave(attachment, filePathSaveAlt);
            }
            altNamePrompt.ReleaseSingleAnswer();
            break;
        case YesNoToAllResponse r when (r == YesNoToAllResponse.YesToAll || r == YesNoToAllResponse.Yes):
            await trySave(attachment, filePathSave);
            break;
        default:
            break;
    }
}

// SortEmail.AttachmentSaving.cs: synchronous path
[ExcludeFromCodeCoverage] // wiring only: real File.Exists and the production sessions
public static void SaveAttachment(this AttachmentHelper attachmentHelper)
{
    SaveAttachment(attachmentHelper, File.Exists, PicturesOverwritePrompt, AttachmentsOverwritePrompt);
}

internal static void SaveAttachment(
    AttachmentHelper attachmentHelper,
    Func<string, bool> fileExists,
    YesNoToAllPromptSession picturesOverwritePrompt,
    YesNoToAllPromptSession attachmentsOverwritePrompt
)
{
    if (!fileExists(attachmentHelper.FilePathSave))
    {
        attachmentHelper.Attachment.SaveAsFile(attachmentHelper.FilePathSave);
        return;
    }
    var overwritePrompt = attachmentHelper.AttachmentInfo.IsImage ? picturesOverwritePrompt : attachmentsOverwritePrompt;
    var answer = overwritePrompt.Ask($"The file {attachmentHelper.FilePathSave} already exists. Overwrite?");
    SaveCase(answer, attachmentHelper.Attachment, attachmentHelper.FilePathSave, attachmentHelper.FilePathSaveAlt);
    overwritePrompt.ReleaseSingleAnswer();
}

internal static void SaveCase(YesNoToAllResponse response, Attachment attachment, string filePathSave, string filePathSaveAlt)
{
    switch (response)
    {
        case YesNoToAllResponse.NoToAll:
        case YesNoToAllResponse.No:
            attachment.SaveAsFile(filePathSaveAlt);
            break;
        case YesNoToAllResponse.Yes:
        case YesNoToAllResponse.YesToAll:
            attachment.SaveAsFile(filePathSave);
            break;
        default:
            break;
    }
}

// SortEmail.AttachmentSaving.cs: re-rooting
[ExcludeFromCodeCoverage] // wiring: forwards to the excluded File.Exists wrapper
public static Task SaveAttachmentAsync(this AttachmentHelper attachmentHelper, string destinationPath)
{
    RedirectSaveFolder(attachmentHelper, destinationPath);
    return SaveAttachmentAsync(attachmentHelper);
}

/// <summary>Re-roots both the primary and the alternate save path to <paramref name="destinationPath"/>,
/// keeping their file names. The helpers are built with the mail's folder name, so both paths must move.</summary>
internal static void RedirectSaveFolder(AttachmentHelper attachmentHelper, string destinationPath)
{
    attachmentHelper.FolderPathSave = destinationPath;
    attachmentHelper.FilePathHelperSaveAlt.FolderPath = destinationPath;
}

// SortEmail.TrySaveAttachment.cs
internal static Task<bool> TrySaveAttachmentAsync(
    this Attachment attachment,
    string filePathSave,
    Action<string> createDirectory,
    Action<string> clearReadOnly,
    YesNoToAllPromptSession removeReadOnlyPrompt
)
{
    return TrySaveAttachmentCoreAsync(attachment, filePathSave, createDirectory, clearReadOnly,
        removeReadOnlyPrompt, isRetryAfterClear: false);
}

private static async Task<bool> TrySaveAttachmentCoreAsync(
    Attachment attachment,
    string filePathSave,
    Action<string> createDirectory,
    Action<string> clearReadOnly,
    YesNoToAllPromptSession removeReadOnlyPrompt,
    bool isRetryAfterClear
)
{
    try { /* createDirectory; await Task.Run(SaveAsFile); return true; unchanged */ }
    catch (System.UnauthorizedAccessException e)
    {
        logger.Warn($"Saving {filePathSave} was denied; the read-only prompt decides whether to retry.", e);
        // The attribute was already cleared once in this call and a "YesToAll" answer is never
        // asked again, so another clear-and-retry cannot change the outcome (#959 L2).
        if (isRetryAfterClear && removeReadOnlyPrompt.Response == YesNoToAllResponse.YesToAll)
        {
            logger.Error($"The file {filePathSave} is still denied after the read-only attribute was cleared.", e);
            throw;
        }
        /* prompt, clear, release: unchanged, except
           inner catch: logger.Error($"The read-only attribute of {directory} could not be cleared; {filePathSave} was not saved.", inner); return false;
           declined branch: logger.Warn($"The file {filePathSave} was not saved because the read-only change was declined.");
           recursion: return await TrySaveAttachmentCoreAsync(..., isRetryAfterClear: true); */
    }
    // no outer catch (System.Exception) { throw; }
}

// SortEmail.UndoAndMoveLog.cs
private static readonly string[] MovedMailsHeader =
{
    "Triage", "FolderName", "Sent_On", "From", "To", "CC", "Subject", "Body",
    "fromDomain", "Conversation_ID", "EntryID", "Attachments", "FlaggedAsTask",
};

[ExcludeFromCodeCoverage] // wiring of real file-system defaults only; a test call would touch the disk (UT4)
public static void WriteCSV_StartNewFileIfDoesNotExist(string strFileName, string strFileLocation)
{
    WriteCSV_StartNewFileIfDoesNotExist(strFileName, strFileLocation, File.Exists, FileIO2.WriteTextFile);
}

/// <summary>Seeds the moved-mails log with its single tab-separated header line when the file
/// does not exist; the columns are those of <c>EmailDetails.Details</c> after its unused index 0.</summary>
internal static void WriteCSV_StartNewFileIfDoesNotExist(
    string strFileName,
    string strFileLocation,
    Func<string, bool> fileExists,
    Action<string, string[], string> writeTextFile
)
{
    if (fileExists(Path.Combine(strFileLocation, strFileName)))
    {
        return;
    }
    writeTextFile(strFileName, new[] { string.Join("\t", MovedMailsHeader) }, strFileLocation);
}

// QuickFiler/Controllers/EfcDataModel.cs (replaces lines 308 to 310; new member after line 326)
bool result;
try
{
    result = await InvokeFilerAsync(config, mailHelpers);
}
finally
{
    // Sticky "to all" prompt answers must not survive into the next filing operation when the
    // filer throws (#959; same root cause as the missing _attachmentsAltName reset).
    ResetFilerPromptState();
}
return result;

/// <summary>Releases the sticky prompt answers held by <see cref="SortEmail"/> after a filing operation.
/// Virtual for the same reason as <see cref="InvokeFilerAsync"/>: a test override records the call,
/// because the answers live in internal state that QuickFiler.Test cannot observe.</summary>
protected internal virtual void ResetFilerPromptState()
{
    SortEmail.Cleanup_Files();
}
```

`FileIO2.WriteTextFile(string filename, string[] strOutput, string folderpath)` (UtilitiesCS/To Depricate/FileIO2.cs lines 36 to 48) matches `Action<string, string[], string>` and `File.Exists` has a single string overload, so both convert as method groups. Among the three `TrySaveAttachmentAsync` overloads only the two-argument one converts to `TrySaveAttachmentDelegate` (R2 1.3, inference; the compiler confirms).

Logger replacement table (R2 5.2; texts are proposals the plan may adjust, the `(string, Exception)` overload is the pattern at `SortEmail.cs` line 211):

| Site (current line) | Current | Replacement |
|---|---|---|
| T 103 | `Debug.WriteLine(e.Message);` | `logger.Warn(..., e)` |
| T 127 | `Debug.WriteLine(inner.Message);` | `logger.Error(..., inner)` |
| T 147 | `Debug.WriteLine($"The file {filePathSave} was not saved.");` | `logger.Warn(...)` |
| T 156 to 159 | `catch (System.Exception) { throw; }` | deleted |
| new guard | n/a | `logger.Error(..., e); throw;` |

Final using blocks (R2 6.2; CSharpier order `System*` first, then alphabetical):

```csharp
// SortEmail.AttachmentSaving.cs
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.EmailIntelligence;

// SortEmail.TrySaveAttachment.cs
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;

// SortEmail.UndoAndMoveLog.cs
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;

// SortEmail.cs
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
using UtilitiesCS.OutlookExtensions;

// SortEmail.MailItemSort.cs
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.OutlookExtensions;
```

CR-1 texts (R2 4.2 and 4.3), applied to `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md`:

Line 155, current: `- The `DirectoryInfo` construction's exception boundary (path computed before the inner `try`).`
Line 155, corrected: `- The read-only clear's exception boundary: `Path.GetDirectoryName` is computed before the inner `try`, and the `DirectoryInfo` construction and the attribute write both run inside it through `ClearReadOnlyAttributeOnDisk` (D2 items 3 and 5). At merge-base the construction ran before the `try`; a `DirectoryInfo` constructor failure is therefore now caught and returns false. This is not reachable in production because `Directory.CreateDirectory` and `Path.GetDirectoryName` already validated the same path in the first `try` (code review CR-1, 2026-10-01).`

Line 99, current trailing clause: `` `var directory = Path.GetDirectoryName(filePathSave);` is computed **before** the inner `try`, as merge-base line 944 does, so the exception boundary is unchanged. ``
Line 99, corrected trailing clause: `` `var directory = Path.GetDirectoryName(filePathSave);` is computed **before** the inner `try`, as merge-base line 944 does; the `DirectoryInfo` construction, which merge-base line 944 performed before the `try`, now runs inside it as the first statement of the adapter (see "Boundaries and invariants to preserve"). ``

Note inserted after line 149 (the "**Must not be widened.**" paragraph): `> Superseded 2026-10-02 by #959 (closes #966): the outer rethrow is removed, `Debug.WriteLine` is replaced by the class logger, and a one-clear retry bound is added. The constraints in this paragraph and in AC6 applied to #956 only.`

#### Required configuration keys and defaults:

- None.

#### Backward-compatibility expectations:

- No public signature used by a caller changes. New members are internal or private except `EfcDataModel.ResetFilerPromptState` (protected internal virtual, on an internal class). Behavior changes visible to callers: L2's rethrow in a state that previously never returned; the alternate save path now rooted under the destination; the header written as one line on first use; prompt state reset after a filer exception.

#### Performance constraints (latency/throughput/memory):

- L2 removes an unbounded asynchronous loop; the bounded path performs at most two save attempts and one attribute clear per call. `AllPromptSessions` allocates a four-element array per `Cleanup_Files` call (once per filing operation). No measurement is required.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - The Outlook `Attachment` and `MailItem` interfaces are mockable with Moq (demonstrated by the existing SortEmail tests).
  - `throw;` preserves the exception instance and `await` rethrows the same instance from `Task.Run`, so a `BeSameAs` assertion holds (R1 3.5, inference; the test confirms it, fallback in Risks).
  - Moq's `Setup(...).Throws(...)` applies to every call, so the persistent failure does not depend on `SetupSequence` exhaustion semantics (R1 3.6).
  - `AttachmentHelper` computes `FilePathSave` and `FilePathSaveAlt` from strings only, so helper construction and re-rooting touch no disk (R2 1.8, 1.6; `FilePathHelper.cs` setters at lines 348 to 396).
  - The two-argument `TrySaveAttachmentAsync` is the only overload convertible to the `trySave` delegate type (R2 1.3, inference).
  - `Cleanup_Files` cannot throw, so the EfcDataModel `finally` cannot mask the original exception.
- Constraints (budget, performance, compatibility):
  - Every changed or new C# file stays under five hundred lines after CSharpier; `EfcDataModel.cs` (predicted about 482) and `SortEmail_AttachmentSaving_Tests.cs` (predicted about 340) are the closest to the limit.
  - Tests must be parallel-safe under class-level parallelism with Workers zero.
  - No digits and no angle-bracket or percent characters appear in acceptance-criteria lines (validator and hook constraints); counts in that section are written as words, figures and generic type arguments live in the body sections, and evidence files are referenced by the names given in Test Strategy.
- External dependencies (services, libraries, releases):
  - None added. MSTest, Moq 4.21.0 and FluentAssertions are already referenced by both test projects.

## Data / API / Config Impact
- User-facing or API changes: in the YesToAll-held persistent-denial state a filing operation now fails with the original `UnauthorizedAccessException` (logged at error level) instead of hanging, and the mail is not moved. An alternate-name save on the live path now lands under the configured save folder rather than relative to the working directory. The moved-mails log receives one tab-separated header line on first use when the file is absent (previously never). Sticky prompt answers are cleared after a filer exception.
- Data or migration considerations: none. Existing moved-mails files are not rewritten. A pre-existing moved-mails file written by the old (no-op) path has no header and keeps none.
- Logging/telemetry updates (if any): four log4net records in the try-save handler (two warn, two error) replacing three debug-output calls.
- Compatibility notes (CLI flags, config schemas, versioning): none.

## Test Strategy
Seeded from issue (restated; the authoritative criteria are in Acceptance Criteria):

- For each defect, write a failing regression test first using the prompt and file-system seams, then apply the minimal fix. Mapped to AC2, AC4, AC6, AC11, AC15, AC18 and AC20.
- For L2, stop retrying on a persistent `UnauthorizedAccessException` under a held YesToAll and surface the error by rethrow. Mapped to AC3 and AC4.

- Regression tests to add or update (fixture facts: a scripted session is `new YesNoToAllPromptSession(message => { messages.Add(message); return answers.Dequeue(); })`; an unscripted session throws from its empty queue, which proves "this session was not asked"; a recording `trySave` appends the path and returns a completed true task; helpers are built with `CreateAttachmentMock`-style mocks, `IsImage` true for `photo.jpg` and false for `report.pdf`; `FilePathSaveAlt` is read back from the helper, never predicted; all literal paths sit under C:\Sortemail959Sandbox and nothing touches the disk):

| Item | File | Tests | Red mechanism | Production pre-step |
|---|---|---|---|---|
| L1 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs` | `SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath` (rows No, NoToAll); `SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath` (rows Yes, YesToAll); `SaveCase_WhenAnswerIsEmpty_DoesNotSave` (control) | `Times.Once` on the mocked `SaveAsFile` observes zero calls | none |
| L2 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` | `TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear` (T12) | `CreateDirectoryLimit` three: the sentinel `InvalidOperationException` replaces the expected `UnauthorizedAccessException` | none |
| L3 phase one | `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` | `Cleanup_Files_ResetsEveryPromptAnswerField` (rows `_responseSaveFile`, `_attachmentsOverwrite`, `_attachmentsAltName`, `_picturesOverwrite`) | the `_attachmentsAltName` row reads YesToAll back | none; test is replaced in phase F1 |
| L4 and header | `UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs` | `WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader`; `WriteCSV_WhenFileExists_DoesNotWrite` | first: queried path is file-then-folder and no write; second: the inverted condition enters the branch (a write is recorded, or the `NullReferenceException` of L4c precedes it; either is a failure) | the four-argument seam lands first as a pure forward of the current body |
| F1 `SaveCaseAsync` | `UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs` | `SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt` (two rows); `SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePathAndReleasesAnswer`; `SaveCaseAsync_WhenAnswerIsNoToAllAndAltNameAnswerIsYesToAll_KeepsAnswerAcrossCalls`; `SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsNoToAll_DoesNotSaveAndKeepsAnswer`; `SaveCaseAsync_WhenAnswerIsNoAndAltNamePromptIsCancelled_DoesNotSaveAndStaysAskable`; `SaveCaseAsync_WhenAnswerIsEmpty_DoesNothing` | compile failure (overload absent) | seams land in the same task; refactor, fail-before exception |
| F1 cores | `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` | `SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting`; `SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly`; `SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly`; `SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave`; `SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain`; `SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath`; `SaveAttachment_WhenFileDoesNotExist_SavesDirectly`; `SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer`; `SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer` | compile failure (overloads absent) | same |
| Re-rooting | same file | `RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths` | alternate-path directory still equals the origin folder after the extraction step | `RedirectSaveFolder` lands with the existing statement only |
| L3 final | same file | `Cleanup_Files_ResetsEveryPromptSession` (reads every non-public static field of type `YesNoToAllPromptSession` and the `AllPromptSessions` value; asserts each field instance is contained by reference, the array has the same count as the fields and no duplicate, and the field count equals four) | refactor; negative control: remove one element from `AllPromptSessions`, observe the test fail, restore | replaces the phase-one test |
| F3 | `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` | one added `DataRow` (saveAttachments true, savePictures true) on each of the two existing `GetAttachmentsInfo` tests | refactor (exclusion removal); verified by the coverage comparison | none |
| EfcDataModel | `QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs` | `MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates`; `MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce` (control); `MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState` (control) | throwing test: reset count zero after the seam extraction and before the `finally` | `ResetFilerPromptState` lands as a behavior-preserving extraction |

  T12 arrangement (R1 3.5): `Seams(YesToAll) { CreateDirectoryLimit = 3 }`; `attachment.Setup(x => x.SaveAsFile(SandboxFilePath)).Throws(denied)` with a single `denied` instance; assert the awaited call throws and `.Which.Should().BeSameAs(denied)`; `CreatedDirectories` equals the sandbox directory twice; `ClearedDirectories` equals it once; `PromptMessages` equals the expected prompt once; `Session.Response` is `YesToAll`; `SaveAsFile` verified `Times.Exactly(2)`. Pins: T3, T4 (successful retry), T7 (Cancel rethrows the same instance), T10 (`IOException` propagates), T11 (re-prompt after a single Yes), and `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave` in SortEmail_Tests.cs.

  L4 tests: `fileExists` records the queried path and returns a scripted bool; `writeTextFile` records the file name, the lines and the folder. Inputs `C:\Sortemail959Sandbox\logs` and `MovedMails.txt`. The absent-file test asserts the queried path equals `Path.Combine(folder, fileName)`, one write with that file name and folder, `lines.Should().ContainSingle()`, `lines[0]` equal to `Triage\tFolderName\tSent_On\tFrom\tTo\tCC\tSubject\tBody\tfromDomain\tConversation_ID\tEntryID\tAttachments\tFlaggedAsTask`, and `lines[0].Split('\t')` of length thirteen.

  EfcDataModel probe: `FilerCleanupProbe : EfcDataModel` built like the `TestableEfcDataModel` of EfcDataModelArchiveRootTests.cs lines 379 to 397 (base constructor with a null mail item, `ConversationResolver` with a parameterless `MailItemHelper`), plus `Func<Task<bool>> Filer` and an `int ResetCalls` counter; fixture helpers copied from that file's lines 330 to 366 (about thirty-five lines) rather than editing it.

  New test files that import the Outlook interop namespace must write `System.Action`, `System.Func` and `System.Exception` where ambiguity arises, because the interop namespace declares types named `Action` and `Exception` (R1 1.3).

- Unit tests (pytest) for the fixed behavior and boundaries: n/a (C#, MSTest). The six test files above are the unit tests.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): `Empty` answer to `SaveCase` and `SaveCaseAsync` (no call); cancelled alternate-name prompt (stays askable); YesToAll held with a successful retry (T3, T4: bound must not fire); single Yes with a denied retry (T11: re-prompt); file present (no write); guard return in `MoveToFolderAsync` (no reset); the three control rows of the phase-one cleanup test.
- Error handling and logging verification: T12 asserts the rethrown instance is the original exception; the throwing EfcDataModel test asserts the filer exception propagates after the reset. Logger calls are not asserted (D2); their presence is verified by reading the diff.
- Coverage impact and targets for changed lines/modules: per D16. Baseline captured before the first production edit; post-change after the final toolchain pass; the comparison lists per-member figures for the members named in AC25 and the content-identified exemptions of R2 5.4. Members losing exclusions (D8) enter the denominator covered by existing SortEmail_Tests.cs tests plus the two added rows; the deleted files and `SanitizeArray` leave the denominator (the ToDoModel file was never in it).
- Toolchain commands to run (format → lint → type-check → test), exactly as in CLAUDE.md:
  1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. Invoke-MSTestWithCoverage.ps1 under scripts/vscode (the `test: MSTest with Coverage (Koverage)` VS Code task), which writes the fixed-name TRX under coverage\test-results and wraps vstest in dotnet-coverage. Each msbuild log must show no skipped CoreCompile target, so the rebuild is known to have compiled.
  Scoped runs during development: filter FullyQualifiedName containing `EmailIntelligence.SortEmail_` (five classes; predicted total fifty-two tests, to be measured) and `EfcDataModelFilerCleanupTests` (three), each with the TRX total asserted non-zero, because a zero-match filter exits zero.
- Evidence artifacts (Markdown projections only, fixed digit-free filenames, each with `Timestamp:`, `Command:`, `EXIT_CODE:` and an output summary):
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/coverage-baseline.md` (JaCoCo package-level projection plus the one-line first-party summary, before the first production edit)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/test-run-baseline.md` (TRX-derived summary)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-save-case.md` (L1, two positive tests RED)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-try-save-retry.md` (L2, T12 RED with the sentinel)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-cleanup-files-phase-one.md` (L3 phase one, the `_attachmentsAltName` row RED, three rows green; the intermediate GREEN run after the one-line fix)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-write-csv.md` (L4, both tests RED after the seam step and before the logic fix)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-redirect-save-folder.md` (re-rooting, RED after the extraction step)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-efc-filer-cleanup.md` (EfcDataModel, throwing test RED after the seam extraction)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/compile-red-attachment-saving-seams.md` (F1 tests: compiler errors against the pre-seam tree, naming the missing overloads)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-exceptions.md` (dossier: usings, logging and outer catch, F2 deletions, F3 exclusion changes, ToDoModel deletion, CR-1, the structural-test replacement; each entry names the pin tests or the compiler as verification and cites the research derivation)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/pass-after-regression-tests.md` (scoped runs, all six classes green, totals asserted non-zero, sandbox Test-Path results)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/toolchain-final-pass.md` (the four commands of the final pass with exit codes and the CoreCompile non-skip check)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/coverage-post-change.md` (JaCoCo projection plus one-line summary)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/coverage-comparison.md` (baseline versus post-change; per-member figures; changed-line check; content-identified exemptions)
  - `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/negative-controls.md` (each code fix reverted alone with its tests kept: L1 labels, L2 guard, L4 logic, `RedirectSaveFolder` second statement, EfcDataModel `finally`, one element of `AllPromptSessions`; only the named tests fail)
- Manual validation steps (if required): none. Sandbox check: the root C:\Sortemail959Sandbox (and the #956 root C:\Sortemail956Sandbox used by the existing tests) must not exist before or after the run; record the Test-Path result in the pass-after projection.

**UT5 call-out (required in the change description; applies to phase one of L3 only).** `Cleanup_Files_ResetsEveryPromptAnswerField` writes static state of `SortEmail` through reflection. Order-independence argument (R1 4.4): the only writers of the four fields are `Cleanup_Files` and the dialog-driven members `SaveAttachment`, `SaveAttachmentAsync`, `SaveCaseAsync` and `SaveAttachmentsOld`, none of which any test executes at that phase; the only concurrent writer in a test run is `Cleanup_Files_DoesNotThrow` in SortEmail_Tests.cs, which writes `Empty`, the value the assertion expects; before the fix it cannot write `_attachmentsAltName` at all, so it cannot cause a false pass, and after the fix a concurrent reset can only make the assertion true earlier; no test writes a non-`Empty` value except this one, for its own row, immediately before its own `Cleanup_Files` call. The test is order-independent in both states and needs no `[DoNotParallelize]`. The test is replaced by the read-only structural test in phase F1 and does not exist in the final tree.


## Acceptance Criteria
- [x] AC1 (L1 code). In `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs`, `SaveCase` has the stacked single-value labels `YesNoToAllResponse.NoToAll` and `YesNoToAllResponse.No` leading to one `attachment.SaveAsFile(filePathSaveAlt)` call and `YesNoToAllResponse.Yes` and `YesNoToAllResponse.YesToAll` leading to one `attachment.SaveAsFile(filePathSave)` call, with `default` making no call; no case label in the method contains a bitwise-or operator and the method does not call `HasFlag`; the `[ExcludeFromCodeCoverage]` attribute above `SaveCase` is removed. Verified by reading the method and by a Grep over the file confirming no pipe character inside a case label.
- [x] AC2 (L1 tests). The new file `UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs` contains `SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath` (DataRow rows No and NoToAll), `SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath` (DataRow rows Yes and YesToAll) and `SaveCase_WhenAnswerIsEmpty_DoesNotSave`; each uses a Moq mock of the Outlook `Attachment` interface with two rooted literal paths under the sandbox root named in Test Strategy and verifies `SaveAsFile` once on the expected path and never on the other; the two positive tests were observed failing before the fix, recorded with a non-zero EXIT_CODE and the failing method names in the fail-before projection for `SaveCase` named in Test Strategy; all rows pass after the fix.
- [x] AC3 (L2 and try-save path code). In `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs`, the internal five-argument `TrySaveAttachmentAsync` extension keeps its exact parameter list and return type and its body is a single forward to a new `private static` `TrySaveAttachmentCoreAsync` taking the same five arguments plus a `bool isRetryAfterClear` passed as false; the recursive call inside the core passes `isRetryAfterClear: true`; inside the `UnauthorizedAccessException` handler, before the prompt check and before any `clearReadOnly` call, when `isRetryAfterClear` is true and `removeReadOnlyPrompt.Response` equals `YesNoToAllResponse.YesToAll`, the handler calls `logger.Error` with a message naming `filePathSave` and the caught exception and then executes a bare `throw;`; the core carries no `[ExcludeFromCodeCoverage]` and has exactly one catch clause (the outer `catch (System.Exception) { throw; }` is removed); the file contains no `Debug.WriteLine` call and no `using System.Diagnostics;` directive; the first statement of the handler is a `logger.Warn` call passing the caught exception, the inner clear failure is logged with `logger.Error` passing the inner exception before `return false;`, and the declined-save branch is logged with `logger.Warn`; `createDirectory` remains the first statement of the try block; the two-argument and three-argument overloads and `ClearReadOnlyAttributeOnDisk` keep their bodies and their `[ExcludeFromCodeCoverage]` attributes. Verified by reading the diff.
- [x] AC4 (L2 regression test). `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` gains `TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear` and a `CreateDirectoryLimit` property on the private `Seams` recorder whose `CreateDirectory` throws `InvalidOperationException` once the recorded calls reach the limit; the test scripts `SaveAsFile` with `Setup` (not `SetupSequence`) to throw one `UnauthorizedAccessException` instance on every call, asserts that the awaited call throws that same instance, that the created-directory record holds the sandbox directory twice, the cleared-directory record holds it once, the prompt record holds the expected prompt once, the session still holds `YesToAll`, and `SaveAsFile` was called exactly twice; it contains no timeout, `Task.Delay` or `Thread.Sleep`; it was observed failing before the fix with the sentinel `InvalidOperationException`, recorded in the fail-before projection for the try-save retry named in Test Strategy, and passes after the fix.
- [x] AC5 (L2 unchanged behavior). The eleven pre-existing test methods of `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` are textually unchanged by the diff and pass after the fix; `TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile` and `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave` in `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` are textually unchanged and pass; no new or changed test attaches a log appender or asserts log output.
- [ ] AC6 (L3 phase one). Against the pre-F1 fields, `Cleanup_Files` in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs` gained the statement `_attachmentsAltName = YesNoToAllResponse.Empty;`, and `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` carried `Cleanup_Files_ResetsEveryPromptAnswerField` with four DataRow rows naming `_responseSaveFile`, `_attachmentsOverwrite`, `_attachmentsAltName` and `_picturesOverwrite`, each obtaining the field by reflection with non-public static binding, asserting it is found, setting it to `YesToAll`, calling `SortEmail.Cleanup_Files()` and asserting `Empty`; the `_attachmentsAltName` row was observed failing before the one-line fix while the other three rows passed, and all four rows passed after it, both runs recorded in the phase-one fail-before projection named in Test Strategy; the test carried no `[DoNotParallelize]`; the UT5 call-out for its reflective static write appears in Test Strategy and in the pull request change description, marked as applying to this phase only.
- [x] AC7 (F1 sessions and cleanup). In the final `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs`, no field of type `YesNoToAllResponse` exists (a Grep for `YesNoToAllResponse _` over the file returns zero hits); three `private static readonly YesNoToAllPromptSession` fields named `AttachmentsOverwritePrompt`, `PicturesOverwritePrompt` and `AttachmentsAltNamePrompt` are initialized with the `YesNoToAll.ShowDialog` method group; a `private static` property (not a field) named `AllPromptSessions`, preceded by a comment stating the partial-file initialization-order reason, returns an array holding those three sessions and `RemoveReadOnlyPrompt` and nothing else; `Cleanup_Files` remains `public static void` with no parameters and its body is a `foreach` over `AllPromptSessions` calling `Reset` on each element; a Grep over the six-partial family for `ShowDialog(` (a call expression) returns zero hits.
- [x] AC8 (F1 seams). In the same file, `SaveAttachmentAsync(this AttachmentHelper)` is a one-statement `[ExcludeFromCodeCoverage]` forward passing `File.Exists`, `PicturesOverwritePrompt`, `AttachmentsOverwritePrompt`, `AttachmentsAltNamePrompt` and `TrySaveAttachmentAsync` (method groups and field references, no lambda) to a new `internal static async Task SaveAttachmentAsync` core whose parameters are the helper, a file-existence predicate named `fileExists`, three session parameters named `picturesOverwritePrompt`, `attachmentsOverwritePrompt` and `altNamePrompt`, and a try-save delegate named `trySave` (exact delegate types in Technical specifications); `SaveCaseAsync` is `internal static async Task` taking the response, the attachment, the two paths, `altNamePrompt` and `trySave`, carries no `[ExcludeFromCodeCoverage]`, and the former four-argument `SaveCaseAsync` no longer exists; `SaveAttachment(this AttachmentHelper)` is a one-statement `[ExcludeFromCodeCoverage]` forward to a new `internal static void SaveAttachment` core taking the helper, `fileExists`, `picturesOverwritePrompt` and `attachmentsOverwritePrompt`, which calls `SaveCase`; both cores select the overwrite session by `AttachmentInfo.IsImage` inside the core; the three prompt message texts are unchanged; the public signatures of the three wrappers are unchanged so that EmailFiler.cs compiles without edits and `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs` and `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` change only in their using blocks.
- [x] AC9 (F1 tests). `UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs` contains the six `SaveCaseAsync` tests named in the Test Strategy table and `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` contains the six `SaveAttachmentAsync` core tests and the three `SaveAttachment` core tests named there; every one constructs its own `YesNoToAllPromptSession` instances with a scripted answer queue (an unscripted session proves it was not asked), uses recording `fileExists` and `trySave` delegates or a mocked `SaveAsFile`, and neither reads nor writes the production sessions; all pass after the change; their fail-before evidence is the compiler output against the pre-seam tree recorded in the compile-red projection named in Test Strategy.
- [x] AC10 (L3 final structural test). The final `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` does not contain `Cleanup_Files_ResetsEveryPromptAnswerField` and contains `Cleanup_Files_ResetsEveryPromptSession`, which obtains by reflection every non-public static field of `SortEmail` whose type is `YesNoToAllPromptSession` and the value of the non-public static `AllPromptSessions` property, asserts that every such field's instance is contained in the array by reference, that the array has the same element count as the field set with no duplicate, and that the field set has exactly the four production sessions named in AC7; the test writes no static state and carries no `[DoNotParallelize]`; the negative control (one element removed from `AllPromptSessions`, the test observed failing, the element restored) is recorded in the negative-controls projection named in Test Strategy.
- [x] AC11 (re-rooting defect). `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs` contains `internal static void RedirectSaveFolder(AttachmentHelper attachmentHelper, string destinationPath)` assigning `attachmentHelper.FolderPathSave` and `attachmentHelper.FilePathHelperSaveAlt.FolderPath` to the destination; `SaveAttachmentAsync(this AttachmentHelper, string)` calls it and then forwards to the parameterless wrapper, keeping `[ExcludeFromCodeCoverage]`; `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` contains `RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths`, which builds a helper under an origin folder, redirects to a destination folder, and asserts that the directory names of `FilePathSave` and `FilePathSaveAlt` both equal the destination and both file names are unchanged; the test was observed failing on the alternate-path assertion after the extraction step and before the second statement, recorded in the fail-before projection for re-rooting named in Test Strategy; it passes after; AttachmentHelper.cs is unchanged.
- [x] AC12 (F2 deletions). `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs` is deleted; `UtilitiesCS/UtilitiesCS.csproj` no longer contains a Compile Include item for it and has no other change; `IsPicture` is removed from the AttachmentSaving partial; a Grep for `SaveAttachmentsOld`, `IsPicture`, `_responseSaveFile` and `MAX_PATH` over the SortEmail partials returns zero hits, and a Grep for `SaveAttachmentsOld` and `IsPicture` over every C# file in the worktree returns zero hits; no regression test is added, and the fail-before exception dossier named in Test Strategy records the zero-caller derivation and the solution rebuild as verification.
- [x] AC13 (F3 dispositions). `[ExcludeFromCodeCoverage]` is removed from `GetAttachmentsInfo`, `GetAttachmentsInfoAsync`, `SaveCaseAsync`, `SaveCase`, `SaveMessageAsMsgAsync` and `SaveMessageAsMSG` in the AttachmentSaving partial and from `SanitizeArrayLineTSV` in the UndoAndMoveLog partial; it is retained on `SaveAttachment(this AttachmentHelper)`, `SaveAttachmentAsync(this AttachmentHelper)`, `SaveAttachmentAsync(this AttachmentHelper, string)`, the two-argument `TrySaveAttachmentAsync`, `ClearReadOnlyAttributeOnDisk`, `UndoAsync`, `PushToUndoStack`, `CaptureMoveDetails`, the two-argument `WriteCSV_StartNewFileIfDoesNotExist`, every `SortAsync` overload, `Sort`, `UpdatePredictiveEngineAsync`, `ProcessMailItemAsync` and both `ResolvePaths` overloads; no other attribute in the partials is added or removed; each of the two existing `GetAttachmentsInfo` tests (synchronous and asynchronous) in `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` gains one DataRow row with `saveAttachments` true and `savePictures` true while keeping its existing row, and both pass. Verified by a Grep for `ExcludeFromCodeCoverage` with one line of context over the partials and by reading the test diff.
- [x] AC14 (L4 and header code). In `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs`, a `private static readonly string[] MovedMailsHeader` holds, in order, Triage, FolderName, Sent_On, From, To, CC, Subject, Body, fromDomain, Conversation_ID, EntryID, Attachments and FlaggedAsTask; an `internal static` four-parameter `WriteCSV_StartNewFileIfDoesNotExist` taking the file name, the folder, a file-existence predicate named `fileExists` and a text-write action named `writeTextFile` (exact delegate types in Technical specifications) returns when `fileExists(Path.Combine(strFileLocation, strFileName))` is true and otherwise calls `writeTextFile(strFileName, lines, strFileLocation)` exactly once with `lines` holding the single tab-joined header string; the public two-parameter overload's body is a single forward passing `File.Exists` and `FileIO2.WriteTextFile` as method groups with no lambda and carries `[ExcludeFromCodeCoverage]` preceded by a justification comment; `SanitizeArray` no longer exists in any partial and `SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows` is removed from `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs`; `SanitizeArrayLineTSV`, `StripTabsCrLf` and their tests are unchanged; AppOlObjects.cs is unchanged.
- [x] AC15 (L4 tests). The new file `UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs` contains `WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader` (asserts the queried path equals the folder-then-file-name combination, one write with that file name and folder, a single line equal to the exact tab-separated string of the thirteen names in the order of AC14, and thirteen fields when that line is split on tab) and `WriteCSV_WhenFileExists_DoesNotWrite` (asserts no write); both use recording delegates only and touch no file system; both were observed failing after the seam step (the four-parameter overload as a pure forward of the previous body) and before the logic fix, recorded in the fail-before projection for the CSV helper named in Test Strategy; both pass after the fix.
- [x] AC16 (predecessor-spec wording correction). In the predecessor structural-split item's spec.md (full backticked path in Proposed Fix D9 and in the write-set table), exactly three edits are made: the "Boundaries and invariants" bullet about the `DirectoryInfo` exception boundary is replaced by the corrected bullet quoted in Technical specifications; the trailing clause of the D2 item-three sentence quoted there is replaced by the corrected clause; and the quoted dated supersession note is inserted immediately after the "Must not be widened." paragraph; no checkbox in that file changes state and no other line changes; the code-review record of that item is not in the diff.
- [x] AC17 (using directives). Each surviving SortEmail partial (`UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs`) has exactly the using block listed for it in Technical specifications, in CSharpier order; a Grep over the partials for `Deedle`, `SDILReader`, `using Outlook =`, `using UtilitiesCS;` and `using System.Diagnostics;` returns zero hits; every partial keeps `using System;`; the two msbuild rebuilds of AC24 pass, which is the only enforced verification because the unused-directive diagnostic is at suggestion severity.
- [ ] AC18 (EfcDataModel). In `QuickFiler/Controllers/EfcDataModel.cs`, inside the five-parameter `MoveToFolderAsync`, the `InvokeFilerAsync` await is the only statement of a `try` block whose `finally` calls `ResetFilerPromptState()`, the result is returned after the `finally`, the three guard returns before the `try` are unchanged, and no direct `SortEmail.Cleanup_Files()` call remains in that method; a new `protected internal virtual void ResetFilerPromptState()` has the body `SortEmail.Cleanup_Files();` with a summary comment; the new file `QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs` contains `MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates` (asserts the filer exception propagates and the reset count is one), `MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce` and `MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState` (reset count zero), using a nested probe that overrides `InvokeFilerAsync` and `ResetFilerPromptState`; `QuickFiler.Test/QuickFiler.Test.csproj` gains exactly one Compile Include item for the new file and no other change; the throwing test was observed failing with a zero reset count after the seam extraction and before the `finally`, recorded in the fail-before projection for the filer cleanup named in Test Strategy; all three pass after; EfcDataModelArchiveRootTests.cs is unchanged.
- [ ] AC19 (ToDoModel duplicate). `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` is deleted; ToDoModel.csproj and both ToDoModel.Test source files are unchanged; a Grep for `SortItemsToExistingFolder` over every project file returns only the two existing ToDoModel.Test test-file entries; the fail-before exception dossier records the not-compiled derivation and the zero-caller derivation; the solution rebuild passes.
- [ ] AC20 (bugfix workflow evidence). Every regression test for a defect (L1, L2, L3 phase one, L4, re-rooting, EfcDataModel) was observed failing before its fix with a non-zero EXIT_CODE and the failing test method named in its fail-before projection, and the plan tags those steps `[expect-fail]`; the refactor-only steps (using directives, logger replacement and outer-catch removal, F2 deletions, F3 exclusion changes, ToDoModel deletion, the predecessor-spec wording correction of AC16, the F1 seam extraction and the structural-test replacement) each have an entry in the fail-before exception dossier naming the pin tests or the compiler as verification.
- [ ] AC21 (test policy). New and changed test code uses only MSTest, Moq and FluentAssertions; a Grep over the six test files in the write set for `DoNotParallelize`, `Thread.Sleep`, `Task.Delay`, `Timeout`, `Directory.CreateDirectory`, `File.Create`, `File.WriteAll`, `GetTempPath` and `MemoryAppender` returns zero hits; no test uses a retry attribute or retry loop (verified by reading); no test in the final tree writes static state of `SortEmail` (the phase-one L3 test no longer exists); the full test run passes under the repository runsettings file (Workers zero, class-level scope) through the Invoke-MSTestWithCoverage route; the sandbox roots named in Test Strategy do not exist before or after the run, with the Test-Path results recorded in the pass-after projection.
- [ ] AC22 (write set and scope). A name-only diff against the merge base with main lists only: `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs` (deleted), `UtilitiesCS/UtilitiesCS.csproj`, `QuickFiler/Controllers/EfcDataModel.cs`, `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` (deleted), `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs`, `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs`, `UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs`, `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs`, `UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, `QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`, the predecessor structural-split item's spec.md named in AC16, and files under this feature folder; in particular it does not list YesNoToAll.cs, YesNoToAllPromptSession.cs, AttachmentHelper.cs, EmailFiler.cs, AppOlObjects.cs, EfcDataModelArchiveRootTests.cs, ToDoModel.csproj, either ToDoModel.Test source file, .editorconfig or the predecessor item's code-review record; `UtilitiesCS.Test/UtilitiesCS.Test.csproj` gains exactly three Compile Include items (one per new test file) in the existing form and has no other change.
- [ ] AC23 (line ceiling). After CSharpier, every C# file in the write set that exists after the change is under five hundred lines, with `QuickFiler/Controllers/EfcDataModel.cs` and `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` reported individually because their predictions are closest to the limit (project files, Markdown and the deleted files are outside the check).
- [ ] AC24 (toolchain). The four CLAUDE.md C# toolchain commands (CSharpier format then check; msbuild Rebuild with analyzers and code style enforced; msbuild Rebuild with warnings treated as errors; Invoke-MSTestWithCoverage) pass in one final pass with zero exit codes recorded in the toolchain projection named in Test Strategy, and each of the two msbuild logs is confirmed to contain no skipped CoreCompile target.
- [ ] AC25 (coverage). The baseline projection is captured before the first production edit and the post-change projection after the final toolchain pass, both as JaCoCo package-level projections with the one-line first-party summary; the comparison projection named in Test Strategy shows the `SaveAttachmentAsync` core, the `SaveAttachment` core, `SaveCaseAsync`, `SaveCase`, `RedirectSaveFolder`, `Cleanup_Files`, `TrySaveAttachmentCoreAsync`, the four-parameter `WriteCSV_StartNewFileIfDoesNotExist` core, `EfcDataModel.ResetFilerPromptState` and the changed lines of `EfcDataModel.MoveToFolderAsync` each at or above ninety percent line coverage; no changed line that was covered at baseline is uncovered after the change; the first-party line and branch figures on the testable denominator are recorded against the CLAUDE.md floors (eighty percent line, seventy-five percent branch) and are not lower than the baseline figures; any unreachable closing brace or wrapper lambda in the try-save file is exempted only when listed by file and content in the comparison projection, re-derived for this item rather than copied from the predecessor item.
- [ ] AC26 (evidence format). Every evidence artifact added by the diff is a Markdown projection under this feature folder's baseline, regression-testing or qa-gates evidence subfolder carrying `Timestamp:`, `Command:` and `EXIT_CODE:` fields; the diff adds no file with an xml, trx or coverage extension anywhere in the repository; the fail-before projections for L1, L2, L3 phase one, L4, re-rooting and EfcDataModel each record a non-zero EXIT_CODE and name the failing test method; the compile-red projection names the missing overloads from the compiler output.
- [ ] AC27 (closure). The pull request body carries closing references to both this item's issue and the folded review-residuals issue (numbers in Rollout & Follow-up), and its description states the behavior changes listed in Data / API / Config Impact (rethrow instead of hang, re-rooted alternate path, single-line header on first use, reset after a filer exception) and the UT5 call-out of Test Strategy.

## Risks & Mitigations
- Technical or operational risks:
  - Cross-project write set: the item edits QuickFiler (`EfcDataModel.cs`), QuickFiler.Test (new file and project file) and ToDoModel (a deletion) in addition to UtilitiesCS and UtilitiesCS.Test. A compile error in any of them fails the solution rebuild; the ToDoModel deletion cannot break a build because the file is not compiled (R2 derivation N9).
  - Parallel-run blast radius: `UtilitiesCS/UtilitiesCS.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj`, `QuickFiler/Controllers/EfcDataModel.cs` and the #956 spec are files other concurrent items may also touch. Project-file edits are single-line additions or removals that merge as unions; the EfcDataModel edit is confined to one method plus one new member; the #956 spec edit is three localized text changes. Any concurrent item touching the same lines must be sequenced by the orchestrator.
  - L2 changes a production outcome from "never returns" to "throws". With D11 the rethrow now runs `Cleanup_Files` through the `finally`; the terminal sink of the exception beyond EfcFormController and EfcHomeController was not traced (R1 3.3) and is stated in the PR description.
  - The alternate-path re-rooting changes where a live-path alternate-name save lands. The previous location (relative to the working directory) was unintended; the PR description states the change.
  - The `BeSameAs` assertion in T12 depends on `await` rethrowing the same instance from `Task.Run`; if the runtime wraps it, the assertion is relaxed to type-and-message equality and the deviation is recorded in the fail-before projection.
  - The method-group conversion of `TrySaveAttachmentAsync` to the `trySave` delegate type is an inference (R2 1.3); if the compiler reports ambiguity, the wrapper names the two-argument overload through a cast of the method group, still without a lambda.
  - `EfcDataModel.cs` is predicted at about 482 lines after the change; if CSharpier pushes it over five hundred, the probe fixture or an unrelated region is not the remedy; the plan must split the partial class before adding lines.
  - The F1 tests are compile-red until the seams land, so the test project does not build between the two steps; the plan must land the seams and the tests within one task so no intermediate commit leaves the solution unbuildable.
  - A static-initializer order hazard across partials is avoided by the `AllPromptSessions` property (D5); a later editor converting it to a field would reintroduce it, which the comment above it states.
  - Nullable flow in the UtilitiesCS partials: new delegate parameters are non-nullable and no new nullable state is introduced; the nullable gate decides any residual.
  - The unused-directive diagnostic is not enforced by the build, so a wrong using block is only caught when it removes a needed directive (compile error); a leftover unused directive would pass unnoticed. Mitigation: the blocks are specified verbatim (AC17) and verified by reading.
  - The phase-one L3 test writes static state; the order-independence argument in Test Strategy is the mitigation and the test is removed in phase F1.
- Mitigations and rollbacks:
  - Negative controls: each code fix reverted alone with its tests kept, confirming only the named tests fail (negative-controls projection).
  - Rollback is a revert of the fix commits; no signature used by a caller changes.
  - The orchestrator sequences execution per R2 section 11 (L1, L3 phase one, L4 and header, L2 and logging, F1 with re-rooting and F2, F3 and usings, EfcDataModel, ToDoModel and CR-1, toolchain) so that every red-first step runs against the code state it was designed for.

## Rollout & Follow-up
- Release/rollout steps: merges with the next add-in build; no configuration or data migration. The PR closes #959 and #966 (D18). The PR description states the four behavior changes listed in Data / API / Config Impact and reproduces the UT5 call-out.
- Post-fix monitoring or clean-up tasks: none for any same-file or same-root-cause item; all such items found by R1 and R2 are fixed in this item under the scope rule. Items recorded for information, not as follow-ups:
  - Retirement of the dead synchronous chain (`Sort`, `SaveAttachment`, `SaveCase`, `GetAttachmentsInfo`, `SaveMessageAsMSG`, `ResolvePaths(IList<MailItem>, ...)`) is a maintainer decision about public dead code, not a defect (R2 1.7). Non-goal of this item.
  - The terminal sink of an exception propagated from EfcFormController, EfcHomeController and EfcDataModel was not traced (R1 3.3); the PR description asks the maintainer to confirm the user-visible handling of the L2 rethrow. This is an observation, not a defect.
  - R2 found no completely unrelated defect. If execution finds one, it is reported for filing through the potential-to-issue lifecycle and not fixed here.
- Links: issue #959; issue #966 (SortEmail review residuals, closed by the same PR); issue #956 (structural split and seams) and its feature folder under docs/features/active; R1 and R2 under this folder's research directory; the plan scaffold (2026-10-02T05-07).
