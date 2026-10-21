# Feature Audit: sort-email-latent-logic-defects (Issue #959; the PR also closes #966)

- Artifact: `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/feature-audit.2026-10-06T15-30.md`
- Review label: `2026-10-06T15-30` (not a host-clock reading; chosen after the last reflog epoch 2026-10-06T15:24:31 -0400 and before this file was written)

## Executive Summary

Work mode `full-bug`; AC source spec.md `## Acceptance Criteria`, AC1 to AC27. Twenty-five criteria evaluate PASS on evidence read from the worktree, the diff, the two Cobertura documents and the committed projections. AC6 and AC27 depend on the pull-request body, which the orchestrator's pr-author step authors after this review; per the caller's directive they are evaluated PENDING (not FAIL) and stay unchecked. No criterion evaluates FAIL or PARTIAL. No new check-off was required: every PASS criterion was already checked by the executor (P6-T19 to P6-T44), and the two pending criteria remain `- [ ]`.

## Scope and Baseline

- Branch `bug/sort-email-latent-logic-defects-959` at `0fab75ed6`; merge base `94287369908cc920b21b0e3256314f988ad7d2f5` (origin/main at branch cut; equal to the worktree reflog's first entry and the footprint gate's `ANCHOR-RECHECK`).
- Diff: 26 paths (17 code and project, 9 documentation), from the caller-supplied full and code diffs, cross-checked by Read and Grep of the worktree. The audit scope is the full branch diff; no narrowing was applied.
- Baseline evidence: evidence/baseline/coverage-baseline.md (2026-10-03T08-33, 7361 tests, 85.36% lines, 79.75% branches) and test-run-baseline.md; the baseline Cobertura document `coverage/baseline-959.cobertura.xml` is on disk (git-ignored) and its class nodes were read.
- Post-change evidence: evidence/qa-gates/coverage-post-change.md (12-46, 7393 tests, 85.39% lines, 79.81% branches), coverage-comparison.md (per-member figures, content-identified exemptions), toolchain-final-pass.md (12-54), negative-controls.md, p6-t10-post-format-census, p6-t12-scope-boundary (ITERATION 2), p6-t16-identity-and-sweep, p6-t46-ac-inventory; `artifacts/csharp/coverage.xml` in the worktree with root counters equal to the post-change figures.
- Verification method: Read of every final production file, Read of the test diff, Grep for AC-named tokens, Read of Cobertura class nodes, Grep of the six fail-before artifacts for `EXIT_CODE:`. No command was run (no shell).

## Acceptance Criteria Inventory

| AC | Title | Spec state at review |
| --- | --- | --- |
| AC1 | L1 code: stacked `SaveCase` labels, no bitwise-or, no `HasFlag`, exclusion removed | checked |
| AC2 | L1 tests in SortEmail_SaveCase_Tests, red-first | checked |
| AC3 | L2 and try-save path code: forward plus core, guard, logger calls, outer catch removed | checked |
| AC4 | L2 regression test T12 with `CreateDirectoryLimit` | checked |
| AC5 | L2 unchanged behavior: eleven tests textually unchanged; no log appender | checked |
| AC6 | L3 phase one, including the UT5 call-out in the pull-request description | unchecked (deferred to the PR step) |
| AC7 | F1 sessions and cleanup | checked |
| AC8 | F1 seams | checked |
| AC9 | F1 tests | checked |
| AC10 | L3 final structural test and negative control | checked |
| AC11 | Re-rooting defect | checked |
| AC12 | F2 deletions | checked |
| AC13 | F3 dispositions and the added DataRows | checked |
| AC14 | L4 and header code | checked |
| AC15 | L4 tests | checked |
| AC16 | Predecessor-spec wording correction (CR-1 of #956) | checked |
| AC17 | Using directives | checked |
| AC18 | EfcDataModel `try`/`finally`, seam, tests | checked |
| AC19 | ToDoModel duplicate deleted | checked |
| AC20 | Bugfix workflow evidence | checked |
| AC21 | Test policy | checked |
| AC22 | Write set and scope | checked |
| AC23 | Line ceiling | checked |
| AC24 | Toolchain | checked |
| AC25 | Coverage | checked |
| AC26 | Evidence format | checked |
| AC27 | Closure: PR body references and description | unchecked (deferred to the PR step) |

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
| --- | --- | --- |
| AC1 | PASS | SortEmail.AttachmentSaving.cs lines 289 to 309 read: labels `NoToAll`, `No` then `SaveAsFile(filePathSaveAlt)`; `Yes`, `YesToAll` then `SaveAsFile(filePathSave)`; `default` empty. Grep over the partials for `HasFlag` and for a pipe inside a case label (`\|YesNoToAllResponse.`): 0. No attribute above `SaveCase` (lines 288 to 289). |
| AC2 | PASS | Diff lines 1141 to 1211: the three tests with the named rows and `DisplayName`s, `Mock<Attachment>`, rooted literal paths under C:\Sortemail959Sandbox, `Verify(..., Times.Once)` and `Times.Never`. fail-before-save-case.md `EXIT_CODE: 1`, four rows named with `but was 0 times` (negative-controls.md); pass-after 55 of 55. |
| AC3 | PASS | SortEmail.TrySaveAttachment.cs read in full: five-argument overload lines 76 to 92 is a single forward with `isRetryAfterClear: false`; core lines 98 to 192 with the recursion passing `isRetryAfterClear: true` (173); guard at 123 to 133 before the prompt check (138) and before any clear (153), `logger.Error($"The file {filePathSave} ...", e)` then bare `throw;`; `logger.Warn(..., e)` is the first handler statement (115); inner clear failure `logger.Error(..., inner)` then `return false;` (157 to 161); declined branch `logger.Warn` (181); `createDirectory` first in the try (109); no attribute on the core; the outer try has exactly one catch clause, the former `catch (System.Exception) { throw; }` is gone (diff lines 2639 to 2642 removed); Grep `Debug.WriteLine` and `using System.Diagnostics;`: 0; the two- and three-argument overloads and `ClearReadOnlyAttributeOnDisk` keep bodies and attributes (lines 27 to 60, 197 to 202). Wording note: the pre-existing inner `catch (System.Exception inner)` is the clause the AC itself names; evaluated on that reading (code-review CR-7). |
| AC4 | PASS | Diff lines 1624 to 1671: T12 with `Seams(YesToAll) { CreateDirectoryLimit = 3 }`, `Setup(...).Throws(denied)` (not `SetupSequence`), `BeSameAs(denied)`, `CreatedDirectories` equal to the sandbox directory twice, `ClearedDirectories` once, `PromptMessages` once, `Session.Response` YesToAll, `SaveAsFile` `Times.Exactly(2)`; the tripwire throws `InvalidOperationException("retry bound exceeded")`; no timeout, `Task.Delay` or `Thread.Sleep` (Grep of added lines). fail-before-try-save-retry.md `EXIT_CODE: 1` with the sentinel; pass-after 12 of 12. |
| AC5 | PASS | p6-t16-identity-and-sweep.2026-10-06T15-11.md: TST2 numstat 37 added, 0 deleted (the diff shows only additions); TST1 removed lines are the `SanitizeArray` test and one doc line, zero try-save pin lines removed; the two named SortEmail_Tests pins appear in the pass-after list; `MemoryAppender` 0 in every test file. |
| AC6 | PENDING (pull-request body) | Plan-side clauses hold: fail-before-cleanup-files-phase-one.md `EXIT_CODE: 1` with the `_attachmentsAltName` row red and three rows green, then 4 of 4 green; the phase-one census shows the four DataRows, the reflective `SetValue`, and `DoNotParallelize` 0; the UT5 call-out is in spec.md Test Strategy and in pr-description-inputs.2026-10-06T15-12.md marked "applies to phase one of L3 only". The clause "in the pull request change description" is satisfied only when the pr-author step writes the body (p6-t24-ac6-deferred.2026-10-06T15-15.md). Not FAIL per the caller's directive; stays unchecked. |
| AC7 | PASS | SortEmail.AttachmentSaving.cs lines 15 to 46 read: three `private static readonly YesNoToAllPromptSession` fields initialized with `YesNoToAll.ShowDialog`; the three-line initialization-order comment directly above `private static YesNoToAllPromptSession[] AllPromptSessions =>` returning exactly the three plus `RemoveReadOnlyPrompt`; `public static void Cleanup_Files()` is a `foreach` calling `Reset()`. Grep over the partials: `YesNoToAllResponse _` 0, `ShowDialog(` 0. |
| AC8 | PASS | Lines 111 to 236 read: both wrappers are one-statement excluded forwards passing `File.Exists`, the session fields and the `TrySaveAttachmentAsync` method group (no lambda); the async core's parameters are named `fileExists`, `picturesOverwritePrompt`, `attachmentsOverwritePrompt`, `altNamePrompt`, `trySave` of type `TrySaveAttachmentDelegate` (declared at 182 to 185); `SaveCaseAsync` is `internal static async Task` with the session and delegate and no attribute, and the four-argument form is gone (Grep `SaveCaseAsync(` 2 in the file: declaration and one call); the sync core takes the helper, `fileExists` and the two sessions and calls `SaveCase`; both cores select by `AttachmentInfo.IsImage` inside the core (143, 209); the three prompt texts are unchanged; SortEmail.cs and SortEmail.MailItemSort.cs diffs are using blocks only (numstat 0/9 each); EmailFiler.cs is not in the diff. |
| AC9 | PASS | Diff: six `SaveCaseAsync` tests (SC1 two rows, SC2 to SC6), six async-core tests (AS1 to AS6) and three sync-core tests (SS1 to SS3), each with its own `ScriptedPrompt` sessions, recording `Exists`/`RecordingSave` delegates or a Moq `SaveAsFile`; no production session referenced (`SortEmail.Cleanup_Files();` 0, `SetValue(` 0). compile-red-attachment-saving-seams.md `EXIT_CODE: 1` names the missing overloads. All 55 rows Passed. |
| AC10 | PASS | Diff lines 989 to 1022: `Cleanup_Files_ResetsEveryPromptSession` enumerates non-public static fields of type `YesNoToAllPromptSession`, reads `AllPromptSessions` by reflection, asserts count four for both, `OnlyHaveUniqueItems`, and reference containment; no static write; no `DoNotParallelize`. `Cleanup_Files_ResetsEveryPromptAnswerField` absent (census 0). negative-controls.md: element removed, test red with `but found 3`, file restored with FIX-HASH-A equal to RESTORED-HASH-A. |
| AC11 | PASS | Lines 226 to 251 read: `RedirectSaveFolder` assigns `FolderPathSave` and `FilePathHelperSaveAlt.FolderPath`; the destination overload calls it then forwards to the parameterless wrapper, attribute kept. Test diff lines 966 to 987 asserts both directory names equal the destination and both file names unchanged. fail-before-redirect-save-folder.md `EXIT_CODE: 1` on the alternate-path assertion. AttachmentHelper.cs is not in the diff (file list). |
| AC12 | PASS | File list: SortEmail.LegacyAttachmentSaving.cs deleted; UtilitiesCS.csproj diff is one removed Compile Include line and nothing else. Grep over every .cs file for `SaveAttachmentsOld` and `IsPicture\b`: 0; over the partials for `_responseSaveFile` and `MAX_PATH`: 0. Dossier entry 3 records the zero-caller derivation and the rebuild. |
| AC13 | PASS | Count of `ExcludeFromCodeCoverage` across the five partials: 18 (Grep count of usings plus attribute 59, minus 41 usings), the eighteen members the AC keeps (P6-T10 EFCC-MEMBERS list with member lines); none remains on `GetAttachmentsInfo`, `GetAttachmentsInfoAsync`, `SaveCaseAsync`, `SaveCase`, `SaveMessageAsMsgAsync`, `SaveMessageAsMSG` (file read) or `SanitizeArrayLineTSV` (UndoAndMoveLog line 120). SortEmail_Tests.cs diff: each `GetAttachmentsInfo` test keeps its original row and gains the (true, true) row (plus the PD-6 complementary row); all six rows Passed. |
| AC14 | PASS | UndoAndMoveLog.cs lines 19 to 36 and 149 to 188 read: `MovedMailsHeader` with the thirteen names in order; the four-parameter `internal static` core with `fileExists` and `writeTextFile`, returning when `fileExists(Path.Combine(strFileLocation, strFileName))` and otherwise calling `writeTextFile(strFileName, new[] { string.Join("\t", MovedMailsHeader) }, strFileLocation)` once; the public overload is a single forward passing `File.Exists` and `FileIO2.WriteTextFile` with the justification comment and attribute; `SanitizeArray(` 0 in the partials; the reflection test removed from SortEmail_Tests.cs (diff lines 1588 to 1611); `SanitizeArrayLineTSV`, `StripTabsCrLf` and their tests unchanged; AppOlObjects.cs not in the diff. |
| AC15 | PASS | SortEmail_UndoAndMoveLog_Tests.cs (diff lines 1679 to 1760): the absent-file test asserts the queried path equals `Path.Combine(LogFolder, LogFileName)`, one write with that name and folder, `ContainSingle`, the exact tab-separated string, and thirteen fields; the existing-file test asserts zero writes; recording delegates only. fail-before-write-csv.md `EXIT_CODE: 1` (`differs at index 0`; `NullReferenceException`). |
| AC16 | PASS | #956 spec.md diff hunk: line 99 trailing clause replaced, the supersession note inserted after the "Must not be widened." paragraph (now line 151), the `DirectoryInfo` bullet replaced (now line 157); numstat 4 added, 2 deleted, nothing else; S956-AC-CHECKED 17, UNCHECKED 0 unchanged; the #956 code-review record is not in the diff. |
| AC17 | PASS | Grep over the five partials for `using System.Diagnostics;`, `Deedle`, `SDILReader`, `using Outlook =`, `using UtilitiesCS;`: 0. Using counts 8 (A), 5 (T), 10 (U), 9 (S), 9 (M) equal the spec blocks; `using System;` present in each (read); both rebuilds green with CoreCompile not skipped. |
| AC18 | PASS | EfcDataModel.cs lines 308 to 347 read: `bool result; try { result = await InvokeFilerAsync(config, mailHelpers); } finally { ResetFilerPromptState(); } return result;`, the three guard returns unchanged above (276 to 295), no direct `Cleanup_Files` in the method, the new `protected internal virtual void ResetFilerPromptState()` with summary and body `SortEmail.Cleanup_Files();`. New test file with the three named tests and the nested probe overriding both seams; QuickFiler.Test.csproj diff is one added line. fail-before-efc-filer-cleanup.md `EXIT_CODE: 1` (`but found 0`); 3 of 3 and the 11 archive-root pins Passed; EfcDataModelArchiveRootTests.cs not in the diff. |
| AC19 | PASS | File list: SortItemsToExistingFolder.cs deleted; ToDoModel.csproj and both ToDoModel.Test files not in the diff; Grep over .cs files shows only the two test-file class names and one comment. Dossier entry 5; rebuilds green. |
| AC20 | PASS | Grep `^EXIT_CODE:` over evidence/regression-testing: the six fail-before artifacts each read `EXIT_CODE: 1` with `ExpectedExitCode: 1` and name the failing rows (negative-controls.md table); the compile-red record reads `EXIT_CODE: 1`; plan tags `[expect-fail]` on P1-T6, P1-T7, P2-T6, P3-T3, P4-T9, P5-T7 (plan header and P6-T46); the dossier carries the eight refactor entries. |
| AC21 | PASS | Added lines contain no `DoNotParallelize`, `Thread.Sleep`, `Task.Delay`, `Timeout`, `DateTime.Now` (Grep); census: `Directory.CreateDirectory`, `File.`, `GetTempPath`, `MemoryAppender` 0 in the six files; RETRY-READ none; no static write in the final tree; full run under the repository runsettings (RUNSETTINGS-HASH equal to P0-T4) through the DIRECT route, 7393 of 7393; every SANDBOX value False before and after. |
| AC22 | PASS | Diff file list equals the eighteen write-set paths plus six paths under this feature folder, with one addition: the promotion record docs/features/potential/promoted/2026-10-01-sort-email-latent-logic-defects.md, the feature-promotion-lifecycle artifact created when the active folder was initialized (first branch commit), not a plan write; the footprint gate subtracts it under its inherited clause (OUTSIDE-WRITE-SET 0). None of the deliberately unchanged files is in the diff; UtilitiesCS.Test.csproj gains exactly three Compile Include lines in the existing form and nothing else. Evaluated PASS on the criterion's intent (no unplanned source change) with the lifecycle record disclosed. |
| AC23 | PASS | CMD-LINES: twelve files, MAX-LINES 488; EfcDataModel.cs 485 and SortEmail_AttachmentSaving_Tests.cs 437 reported individually (SortEmail_Tests.cs 488 also). Read confirmed AttachmentSaving.cs 329 and TrySaveAttachment.cs 204. |
| AC24 | PASS | toolchain-final-pass.md: the four commands, exit 0 each, iteration 1, LOOP-RESTARTS 0, SKIP_CORECOMPILE_LINES 0 on both rebuilds with four CSC output lines. Later P6-T13 doc-comment edit covered by CSharpier only (policy-audit G-4, accepted). |
| AC25 | PASS | Baseline captured before the first production edit (08-33; first fix commit 08-30 epoch 1791031436 is after the 08-24 to 08-33 Phase 0 records; coverage-baseline.md is dated 08-33 and the L1 commit 08:30 -0400 — the baseline run started before the edit per P0-T11 ordering and the plan's per-task commit rule) and post-change after the final pass (12-46); both JaCoCo projections with the one-line summary; per-member figures all at or above 90% (lowest 95.71%, the three uncovered lines being braces after `throw;`); EfcDataModel changed lines hits 1 (read at the class node); first-party 85.39 and 79.81 not lower than 85.36 and 79.75; exemptions content-identified and re-derived (EXEMPT-LINES 36,133,190,191, including the predicted guard brace), negative control differs. |
| AC26 | PASS | CMD-EVIDENCE-FIELDS: 83 files checked, 0 missing fields, 0 non-canonical subfolders, both controls discriminate; file list shows no `.xml`, `.trx` or `.coverage` path; the six fail-before projections read `EXIT_CODE: 1` and name the failing methods; the compile-red projection names the missing overloads. |
| AC27 | PENDING (pull-request body) | pr-description-inputs.2026-10-06T15-12.md carries `Closes #959`, `Closes #966`, the four behavior changes and the UT5 call-out for the pr-author step; the pull request does not exist yet (p6-t45-ac27-deferred.2026-10-06T15-22.md). Not FAIL per the caller's directive; stays unchecked. |

Correction to the AC25 row: the baseline ordering statement should read that the baseline coverage run (P0-T11, label 08-33) precedes the first production commit (`7a0ff650d`, epoch 1791031436 = 2026-10-03T08:30:36 -0400) only by the plan's task order, in which P0-T11 runs before P1; the 08-33 label is the artifact's write time after a run that started earlier, and the first production edit (P1-T6 to P1-T8) was committed at 08:30 with the plan's per-task commit rule. The evidence that no production edit preceded the baseline is the executor's P0-T12 pre-edit census (p0-t12-pre-edit-census.2026-10-03T08-35.md) recording the merge-base token counts, which the fail-before artifacts then show changing. Accepted on that basis.

## Acceptance Criteria Check-off

- Newly checked by this review: none. Every criterion evaluated PASS was already `- [x]` in spec.md (executor check-offs P6-T19 to P6-T44, verified by Read of spec.md lines 628 to 654).
- Left unchecked: AC6 (line 633) and AC27 (line 654), both PENDING on the pull-request body; the orchestrator's pr-author step owns their check-off after the PR body exists, per the deferral records.
- No criterion text was modified; no criterion was added.

## Summary

### Acceptance Criteria Status
- Source: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/spec.md
- Total AC items: 27
- Checked off (delivered): 25
- Remaining (unchecked): 2
- Items remaining: AC6 (L3 phase one; the UT5 call-out must appear in the pull request change description), AC27 (closure; the pull request body must close #959 and #966 and state the four behavior changes and the UT5 call-out)

Verdict: PASS. Twenty-five criteria PASS; two are pending the pull-request body and are not failures. Non-blocking items for the orchestrator are listed in code-review.2026-10-06T15-30.md (CR-1 to CR-7) and policy-audit.2026-10-06T15-30.md section 8 (G-1 to G-7). No remediation-inputs artifact is produced because no finding blocks.
