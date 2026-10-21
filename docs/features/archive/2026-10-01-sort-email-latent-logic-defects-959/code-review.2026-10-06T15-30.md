# Code Review: sort-email-latent-logic-defects (Issue #959; the PR also closes #966)

- Artifact: `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/code-review.2026-10-06T15-30.md`
- Review label: `2026-10-06T15-30` (not a host-clock reading; chosen after the last reflog epoch 2026-10-06T15:24:31 -0400 and before this file was written)
- Branch and base: `bug/sort-email-latent-logic-defects-959` at `0fab75ed6` against merge base `942873699`
- Scope: the full branch diff (17 code and project paths, read in full from the caller-supplied code diff and from the worktree; 9 documentation paths)
- Method: Read of every changed production file in its final form, Read of the diff for every test file, Grep over the worktree for callers and dead symbols, Read of the Cobertura class nodes for the changed files. No command was run.

## Executive Summary

The change is well-structured and policy-compliant: four logic defects and three same-root-cause items are fixed by the smallest change the spec names, every fix carries a red-first regression test or a documented exception, the hand-rolled prompt-state fields are replaced by the existing `YesNoToAllPromptSession` type behind seamed cores, and the dead legacy partial and the uncompiled ToDoModel duplicate are removed. Public signatures used by callers are unchanged (verified at every call site). Zero blocking findings. Seven non-blocking findings: one related test gap closable with a single test (CR-1), one same-root-cause call site in an uncompiled legacy file that needs an orchestrator decision (CR-2), one unused using directive in a new test file (CR-3), and four informational items.

Key logic verified by reading rather than by trusting the evidence:

- L2 bound. In `TrySaveAttachmentCoreAsync` the retry is the only call that passes `isRetryAfterClear: true`, and it is reached only after a `Yes` or `YesToAll` answer and a successful `clearReadOnly`. The `finally` releases a single `Yes`, so on the retry the session holds `Empty` or `YesToAll`; the guard fires only for `YesToAll`, which is the one state that could loop. A single `Yes` still re-prompts (T11) and a successful retry still returns true (T3, T4). The Cobertura node shows the guard at 4 of 4 conditions.
- Rethrow identity. `throw;` inside the `catch (System.UnauthorizedAccessException e)` preserves the instance; the awaited `Task.Run` rethrows the same instance, which T12 pins with `BeSameAs`.
- EfcDataModel `finally`. `ResetFilerPromptState()` calls `Cleanup_Files()`, which allocates a four-element array and calls `Reset()` on each session (`Response = Empty`); nothing in that path can throw, so the `finally` cannot mask the filer's exception. The three guard returns stay before the `try`, so a guard return still performs no reset (pinned by the third test).
- Re-rooting. `AttachmentHelper.FilePathHelperSaveAlt` is `internal` (AttachmentHelper.cs:175) and `FolderPathSave` is a public setter (line 199); the helper's alternate file name is read back from the helper, never predicted, and the test asserts both directory names and both file names.
- Static initialization. `AllPromptSessions` is a property, so `RemoveReadOnlyPrompt` (declared in another partial) is read at call time; the comment above it states the reason.
- Session mapping. `Ask` prompts only while `Response == Empty`; `ReleaseSingleAnswer` clears only `Yes`/`No`; `Reset` clears everything (YesNoToAllPromptSession.cs lines 40 to 68). Each former hand-rolled site performed exactly that sequence, so the F1 refactor is behavior-preserving, and `_responseSaveFile` had no reader outside the deleted `SaveAttachmentsOld`.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| Minor | UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs | line 143 (synchronous `SaveAttachment` core); tests SS1 to SS3 in UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs | CR-1. The `IsImage ? picturesOverwritePrompt : attachmentsOverwritePrompt` selection in the synchronous core is exercised only on its false arm: all three synchronous tests use `report.pdf`. A regression that swapped the two sessions in the synchronous core would pass today. The asynchronous core's selection is pinned on both arms (AS2, AS3). | Add one synchronous test mirroring AS2: helper built from `photo.jpg`, `Exists(true, ...)`, a scripted pictures session (`Yes`) and an unscripted attachments session; assert the pictures session was asked once with the overwrite text, the attachments session never, and `SaveAsFile` once on `FilePathSave`. Related test gap in a touched file; one-test remedy, in-item per the caller's directive. | AC8 states that both cores select the overwrite session by `IsImage` inside the core; a selection that is not pinned on both arms is not verified. | artifacts/csharp/coverage.xml class node for the file: line 143 `condition-coverage="50% (1/2)"`, method `SaveAttachment` `branch-rate="0.75"`; the file otherwise reads 100% lines and 48 of 49 branches. |
| Minor | QuickFiler/Legacy/QfcController.cs | lines 781 to 792 | CR-2. `SortEmail.Run(...)` followed by `SortEmail.Cleanup_Files();` with no `try`/`finally`, the same root cause the EfcDataModel fix addresses. The file is not compiled: QuickFiler.csproj has no `Compile Include` under `Legacy\`, and `SortEmail.Run` does not exist in the partials, so the file could not compile if it were included. No runtime effect. | Orchestrator decision: either delete the uncompiled file under the D12 precedent (uncompiled duplicate with a known defect pattern, deleted rather than edited) or record it for a `QuickFiler/Legacy/` cleanup issue, since the whole folder is uncompiled and folder-level cleanup exceeds this item. No behavioral fix is possible inside the item because the code cannot compile. | The scope rule asks that same-root-cause defects be fixed here; R2's N9 reasoning (uncompiled code cannot be verified) applies equally; the two options are both defensible and the choice is the orchestrator's. | Grep over QuickFiler.csproj for `QfcController.cs` and `Legacy\`: no match; Grep over the partials for `static .* Run(`: no match; Read of QfcController.cs lines 740 to 809. |
| Minor | UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs | line 1 | CR-3. `using System;` is unused: the file references only `Mock<Attachment>`, `List<string>`, `Queue<YesNoToAllResponse>`, `Task.FromResult` and the session type; no `System.*` type is named. The build does not report it (IDE0005 is suggestion severity, as spec D10 notes). | Remove the directive. | Consistency with D10, which pruned unused directives from every production partial for the same reason. The other three new test files use `System` types (`Func`, `Action`, `DateTime`, `InvalidOperationException`). | Read of the file in the code diff (lines 1111 to 1453). |
| Informational | QuickFiler/Controllers/EfcDataModel.cs; UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs | whole files | CR-4. 485 and 488 lines against the 500-line limit after this change (EfcDataModel +11, SortEmail_Tests +30 net). | Split before the next addition (a `.Harness.cs` partial for the test fixture; a partial for EfcDataModel's suggestion members). The spec's Risks section already names EfcDataModel. | Keeps the next item from being forced into a split it did not plan. | P6-T10 CMD-LINES; Read of EfcDataModel.cs to line 354. |
| Informational | QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs | lines 112 to 162 | CR-5. About fifty lines of fixture helpers (`CreateProbe`, `CreateOlObjects`, `CreateGlobals`, the two special-folder builders) are copied from EfcDataModelArchiveRootTests rather than shared. The spec directed the copy to keep that file unchanged (D11), so this is a recorded trade-off, not a defect. | When either fixture changes next, consolidate the helpers into a shared internal fixture class in QuickFiler.Test. | General policy design principle 2 (reusability). | Diff lines 112 to 162; spec D11 and Test Strategy ("fixture helpers copied from that file's lines 330 to 366"). |
| Informational | UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs | lines 109, 141, 150 | CR-6. `Path.GetDirectoryName(filePathSave)` is computed three times in the core (the create-directory call, the prompt message, the clear). Pre-existing shape carried into the core; pure string work. | Optional: compute once at the top of the core. Not required by any AC. | Readability only. | Read of the file. |
| Informational | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/spec.md | AC3, AC13 | CR-7. AC3 says the core "has exactly one catch clause" while the core holds the outer handler plus the pre-existing inner `catch (System.Exception inner)` that the same AC names; AC13 says each `GetAttachmentsInfo` test "gains one DataRow row" while each gained two (spec row plus PD-6's complementary row, disclosed in pr-description-inputs). | No code action. If the spec is revised, say "the outer try has exactly one catch clause" and "gains the (true, true) row and keeps its existing row". | Both ACs were evaluated on their stated intent (feature-audit); the wording could mislead a later reader. | spec.md lines 630 and 640; TrySaveAttachment.cs lines 113 and 155; SortEmail_Tests.cs diff lines 1475 to 1548. |

## Review by Area

### Production: SortEmail.AttachmentSaving.cs (329 lines, read in full)

- Lines 15 to 46: three `private static readonly` sessions initialized with the `YesNoToAll.ShowDialog` method group, the property-not-field comment, `AllPromptSessions`, and the `foreach` cleanup. Correct and minimal.
- Lines 111 to 156: excluded wrapper plus synchronous core. The core calls `SaveCase` and then `ReleaseSingleAnswer()`; the Empty answer falls to `default` with no call. CR-1 applies to line 143.
- Lines 158 to 224: excluded wrapper, the nested `TrySaveAttachmentDelegate` with its CS1769 justification, and the asynchronous core. The core's summary states that the `trySave` result is not inspected and that a persistent failure surfaces by exception, which is the L2 contract.
- Lines 226 to 251: the destination overload now calls `RedirectSaveFolder` and returns the inner task; the helper assigns both folder paths.
- Lines 253 to 309: `SaveCaseAsync` with the session and delegate; `SaveCase` with stacked labels. No bitwise-or label, no `HasFlag` (Grep).
- Exclusion attributes: 3 remain (the three wrappers), each with a one-line justification comment.

### Production: SortEmail.TrySaveAttachment.cs (204 lines, read in full)

- The five-argument overload is a pure forward (lines 76 to 92); the core (98 to 192) carries the former body plus the guard at 123 to 133 and the four logger calls. The first handler statement is `logger.Warn(..., e)`; the clear-failure handler logs at Error with `inner`; the declined branch logs at Warn. No `Debug.WriteLine`, no `using System.Diagnostics;`.
- `createDirectory(Path.GetDirectoryName(filePathSave))` stays the first statement of the `try` (line 109), pinned by the IOException test.
- Exclusion attributes: 2 remain (the two-argument wrapper and `ClearReadOnlyAttributeOnDisk`).

### Production: SortEmail.UndoAndMoveLog.cs (190 lines; lines 90 to 190 read)

- `MovedMailsHeader` holds the thirteen names in `Details` order; the excluded two-parameter wrapper forwards `File.Exists` and `FileIO2.WriteTextFile` as method groups; the core checks `Path.Combine(strFileLocation, strFileName)` and writes once. `SanitizeArrayLineTSV` lost its exclusion and is exercised by its existing test; `SanitizeArray` is gone.

### Production: EfcDataModel.cs (485 lines; lines 270 to 354 read)

- `bool result; try { result = await InvokeFilerAsync(...); } finally { ResetFilerPromptState(); } return result;` with the "why" comment; the virtual seam with its summary. Definite assignment is satisfied because the `finally` never returns.

### Production: SortEmail.cs and SortEmail.MailItemSort.cs

- Using blocks only (numstat 0/9 each). The remaining directives are the ones the spec lists; both rebuilds compiled the files.

### Tests

- SortEmail_SaveCase_Tests (343 lines): twelve rows; every `DataRow` carries a `DisplayName`; the Empty control and the two-call stickiness cases are present; the scripted session proves non-asking by an empty-queue `Dequeue` throw, which is a clear and deterministic mechanism. CR-3 applies.
- SortEmail_AttachmentSaving_Tests (437 lines): six asynchronous-core, three synchronous-core, re-rooting and structural tests; the structural test asserts count four, uniqueness and reference containment and writes no static state. The `ScriptedPrompt` helper is duplicated between the two new test files (the same private class twice); acceptable for test code but a candidate for a shared test helper if a third user appears.
- SortEmail_UndoAndMoveLog_Tests (82 lines): both tests assert the queried path and the exact header; the second test asserts zero writes.
- SortEmail_TrySaveAttachment_Tests (412 lines): T12 and the `CreateDirectoryLimit` tripwire with its "why" comment; the eleven existing tests are untouched (TST2-DELETED-LINES 0).
- SortEmail_Tests (488 lines): the two `GetAttachmentsInfo` tests became three-row data tests with an `OnlyContain` image-flag assertion; the `SanitizeArray` reflection test is removed; the `Cleanup_Files_DoesNotThrow` doc comment now describes the session-based reset.
- EfcDataModelFilerCleanupTests (192 lines): strict mocks; the probe overrides both virtual seams; the three scenarios (throws, succeeds, guard) each assert the reset count. CR-5 applies.

### Project and documentation files

- UtilitiesCS.csproj: one Compile Include removed. UtilitiesCS.Test.csproj: three added after the existing SortEmail entries in the same form. QuickFiler.Test.csproj: one added. No other change.
- #956 spec.md: the three CR-1 edits exactly (lines 99, 151, 157 read in the current file; diff numstat 4 added, 2 deleted); no checkbox changed state.
- The promotion record under docs/features/potential/promoted/ is the feature-promotion-lifecycle artifact (Status: Promoted), created at the folder's initialization, not by the plan.

## Unrelated Defects for Filing

- U-1. `QuickFiler/Controllers/EfcDataModel.cs` carries pre-existing coverage debt (76.34% lines, 73.08% branches after this change; 75.69% and 73.08% before) concentrated in `TryGetFirstInSelection`, `InvokeFilerAsync`, `PackageItems`, `FindMatches` and `RefreshSuggestions`. Not a defect of this item; a coverage-uplift potential entry is recommended.
- U-2. `QuickFiler/Legacy/` is not compiled by QuickFiler.csproj and at least `QfcController.cs` references a `SortEmail.Run` that does not exist. A folder-level cleanup decision is owed (see CR-2 for the single call site related to this item).
- U-3. `quality-tiers.yml` is absent at the repository root (pre-existing, recurring; promoted at #956).
