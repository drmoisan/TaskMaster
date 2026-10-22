# Feature Audit: sort-email-latent-logic-defects (Issue #959; the PR also closes #966) — re-review after Phase 7

- Artifact: `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/feature-audit.2026-10-06T17-40.md`
- Review label: `2026-10-06T17-40` (not a host-clock reading; chosen after the last reflog epoch 2026-10-06T17:25:53 -0400 and before this file was written)

## Executive Summary

Work mode `full-bug`; AC source spec.md `## Acceptance Criteria`, AC1 to AC27. Twenty-five criteria evaluate PASS on evidence read from the worktree at HEAD `2d7e338f4`, the diffs, the two Cobertura documents and the committed projections at ITERATION 2. AC6 and AC27 depend on the pull-request body, which the orchestrator's pr-author step authors after Phase 8; per the caller's directive they are evaluated PENDING (not FAIL) and stay unchecked. No criterion evaluates FAIL or PARTIAL. No new check-off was required: every PASS criterion was already checked by the executor (P6-T19 to P6-T44) and Phase 7 changed no check box (P7-T16 `AC-CHECKED: 25`, `AC-ANY-UNCHECKED: 2`). The Phase 7 remediation strengthened AC9, AC21, AC24 and AC25 without altering any criterion's verdict; the prior review's G-4 caveat on AC24 no longer applies.

## Scope and Baseline

- Branch `bug/sort-email-latent-logic-defects-959` at `2d7e338f4`; merge base `94287369908cc920b21b0e3256314f988ad7d2f5` (origin/main at branch cut; equal to the worktree reflog's first entry and the P7-T15 `ANCHOR-RECHECK`). Phase 8 (origin/main reconciliation) has not run.
- Diff: 17 code and project paths (from the caller-supplied full code diff, cross-checked against the worktree); documentation paths under this feature folder, the #956 spec, the promotion record and the agent-memory Markdown (P7-T15 footprint: 151 paths, `OUTSIDE-WRITE-SET: 0`). Phase 7 delta: two test files (+32 lines SS4, -1 line `using System;`) and the feature-folder evidence. The audit scope is the full branch diff; no narrowing was applied.
- Baseline evidence: evidence/baseline/coverage-baseline.md (2026-10-03T08-33, 7361 tests, 85.36% lines, 79.75% branches) and test-run-baseline.md; the baseline Cobertura document `coverage/baseline-959.cobertura.xml` is on disk (git-ignored) and its class nodes were read.
- Post-change evidence: evidence/qa-gates/coverage-post-change.md ITERATION 2 (P7-T11, 17-19, 7394 tests, 85.39% lines, 79.81% branches), coverage-comparison.md ITERATION 2 (P7-T12 and P7-T13: per-member figures, content-identified exemptions, the line-143 condition reading), toolchain-final-pass.md ITERATION 2 (P7-T14), pass-after-regression-tests.md ITERATION 2 (P7-T10), p7-t1-pre-edit-observations, p7-t4-scoped-format-and-census, p7-t5-attsave-run, p7-t6 to p7-t9, p7-t15-scope-boundary, p7-t16-phase7-closure; `artifacts/csharp/coverage.xml` in the worktree with root counters equal to the post-change figures (`timestamp="1791321467"`).
- Verification method: Read of the Phase 7 delta in full, Read of the two edited test files, Read of the changed production files' regions named by the criteria, Grep for AC-named tokens, Read of Cobertura class, method and line nodes, Grep of spec.md for the check-box state. No command was run (no shell).

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
| AC1 | PASS | Unchanged since the prior review (no production line changed in Phase 7; P7-T15 numstat rows equal P6-T12). SortEmail.AttachmentSaving.cs lines 289 to 309: labels `NoToAll`, `No` then `SaveAsFile(filePathSaveAlt)`; `Yes`, `YesToAll` then `SaveAsFile(filePathSave)`; `default` empty; no `HasFlag`, no pipe inside a case label; no attribute above `SaveCase`. |
| AC2 | PASS | SortEmail_SaveCase_Tests.cs (342 lines after P7-T3): the three L1 tests with the named rows and `DisplayName`s, `Mock<Attachment>`, rooted literal paths, `Verify(..., Times.Once)` and `Times.Never` (P7-T4 census: `SortEmail.SaveCase(` 3, `[DataRow(` 6, `DisplayName=` 6). fail-before-save-case.md `EXIT_CODE: 1`; pass-after 56 of 56 (P7-T10). |
| AC3 | PASS | SortEmail.TrySaveAttachment.cs unchanged (204 lines): five-argument overload a single forward with `isRetryAfterClear: false`; core with the guard before the prompt check and before any clear, `logger.Error(...)` then bare `throw;`; `logger.Warn(..., e)` first handler statement; inner clear failure `logger.Error(..., inner)` then `return false;`; declined branch `logger.Warn`; `createDirectory` first in the try; no attribute on the core; the former `catch (System.Exception) { throw; }` gone; no `Debug.WriteLine`, no `using System.Diagnostics;`. Wording note on "exactly one catch clause" unchanged (code-review CR-7). |
| AC4 | PASS | SortEmail_TrySaveAttachment_Tests.cs unchanged (412 lines): T12 with `Seams(YesToAll) { CreateDirectoryLimit = 3 }`, `Setup(...).Throws(denied)`, `BeSameAs(denied)`, `CreatedDirectories` twice, `ClearedDirectories` once, `PromptMessages` once, `Session.Response` YesToAll, `SaveAsFile` `Times.Exactly(2)`; the tripwire throws `InvalidOperationException("retry bound exceeded")`; no timeout, `Task.Delay` or `Thread.Sleep`. fail-before-try-save-retry.md `EXIT_CODE: 1`; pass-after: T12 Passed in P7-T10. |
| AC5 | PASS | P7-T15: TST2 numstat 37 added, 0 deleted (additions only); TST1 removed lines are the `SanitizeArray` test and one doc line; the two named SortEmail_Tests pins (`TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile`, `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave`) appear Passed in the P7-T10 list; `MemoryAppender` 0 in every test file (P7-T4 census for the two edited files; prior census for the rest). |
| AC6 | PENDING (pull-request body) | Plan-side clauses hold (fail-before-cleanup-files-phase-one.md `EXIT_CODE: 1`; phase-one census; the UT5 call-out in spec.md Test Strategy and in pr-description-inputs.2026-10-06T15-12.md, with the Phase 7 addendum in p7-t16-phase7-closure.2026-10-06T17-25.md). The clause "in the pull request change description" is satisfied only when the pr-author step writes the body (p6-t24-ac6-deferred.2026-10-06T15-15.md). Not FAIL per the caller's directive; stays unchecked (P7-T16 `AC6-UNCHECKED: 1`). |
| AC7 | PASS | SortEmail.AttachmentSaving.cs lines 15 to 46 unchanged: three `private static readonly YesNoToAllPromptSession` fields initialized with `YesNoToAll.ShowDialog`; the initialization-order comment above `AllPromptSessions`; `Cleanup_Files()` a `foreach` calling `Reset()`. `Cleanup_Files_ResetsEveryPromptSession` Passed (P7-T5, P7-T10). |
| AC8 | PASS | Lines 108 to 160 re-read: the excluded one-statement wrapper at 111 to 122 passes `File.Exists` and the two session fields; the sync core at 130 to 156 takes the helper, `fileExists` and the two sessions, selects by `AttachmentInfo.IsImage` at 143 to 145 and calls `SaveCase`; the async side unchanged (prior review). Both arms of the sync selection are now pinned (SS4 image, SS2/SS3 document), and the Cobertura line-143 condition reads 100% (2/2). |
| AC9 | PASS | SortEmail_AttachmentSaving_Tests.cs read in full (469 lines): six async-core tests (AS1 to AS6), four sync-core tests (SS1 to SS4), RR and CF, each with its own `ScriptedPrompt` sessions, recording `Exists`/`RecordingSave` delegates or a Moq `SaveAsFile`; no production session referenced (`SortEmail.Cleanup_Files();` 0, `SetValue(` 0 in the P7-T4 census). SortEmail_SaveCase_Tests.cs: six `SaveCaseAsync` tests (P7-T4: `SortEmail.SaveCaseAsync(` 9 call sites across nine rows). All 56 rows Passed; SS4 Passed in P7-T5, P7-T10 and the full run. |
| AC10 | PASS | `Cleanup_Files_ResetsEveryPromptSession` (lines 354 to 387) enumerates non-public static fields of type `YesNoToAllPromptSession`, reads `AllPromptSessions` by reflection, asserts count four for both, `OnlyHaveUniqueItems`, and reference containment; no static write; no `DoNotParallelize`. `Cleanup_Files_ResetsEveryPromptAnswerField` absent (P7-T4 census 0). negative-controls.md: element removed, test red with `but found 3`, file restored with equal hashes. |
| AC11 | PASS | `RedirectSaveFolder` (lines 244 to 251, unchanged) assigns `FolderPathSave` and `FilePathHelperSaveAlt.FolderPath`; the RR test (lines 331 to 352) asserts both directory names equal the destination and both file names unchanged. fail-before-redirect-save-folder.md `EXIT_CODE: 1`; RR Passed in P7-T5 and P7-T10. AttachmentHelper.cs is not in the diff. |
| AC12 | PASS | P7-T15 `DELETED-PATHS` lists SortEmail.LegacyAttachmentSaving.cs and SortItemsToExistingFolder.cs; UtilitiesCS.csproj numstat 0/1. Grep over every .cs file for `SaveAttachmentsOld` and `IsPicture\b`: 0 (prior review; no production change since). |
| AC13 | PASS | Count of `ExcludeFromCodeCoverage` across the five partials: 18 (unchanged); none remains on `GetAttachmentsInfo`, `GetAttachmentsInfoAsync`, `SaveCaseAsync`, `SaveCase`, `SaveMessageAsMsgAsync`, `SaveMessageAsMSG` or `SanitizeArrayLineTSV`. The six `GetAttachmentsInfo` rows Passed in P7-T10. Wording note on "gains one DataRow row" unchanged (CR-7). |
| AC14 | PASS | SortEmail.UndoAndMoveLog.cs unchanged (190 lines): `MovedMailsHeader` with the thirteen names; the four-parameter `internal static` core with `fileExists` and `writeTextFile`; the public overload a single excluded forward; `SanitizeArray(` 0 in the partials; the reflection test removed from SortEmail_Tests.cs; `SanitizeArrayLineTSV` and `StripTabsCrLf` tests Passed in P7-T10. |
| AC15 | PASS | SortEmail_UndoAndMoveLog_Tests.cs unchanged (82 lines): the absent-file test asserts the queried path, one write with that name and folder, `ContainSingle`, the exact tab-separated string and thirteen fields; the existing-file test asserts zero writes. fail-before-write-csv.md `EXIT_CODE: 1`; both tests Passed in P7-T10. |
| AC16 | PASS | #956 spec.md unchanged in Phase 7 (numstat 4/2; P7-T16 CMD-SPEC-CHECK S956-NEW-99 1, S956-NEW-149 1, S956-NEW-155 1, S956-OLD-99 0, S956-OLD-155 0, S956-AC-CHECKED 17, UNCHECKED 0). |
| AC17 | PASS | The five partials unchanged: Grep for `using System.Diagnostics;`, `Deedle`, `SDILReader`, `using Outlook =`, `using UtilitiesCS;`: 0; using counts 8, 5, 10, 9, 9 equal the spec blocks; both Phase 7 rebuilds green with CoreCompile not skipped (p7-t8, p7-t9). |
| AC18 | PASS | EfcDataModel.cs lines 300 to 349 re-read: `bool result; try { result = await InvokeFilerAsync(config, mailHelpers); } finally { ResetFilerPromptState(); } return result;` with the "why" comment; the new `protected internal virtual void ResetFilerPromptState()` with summary and body `SortEmail.Cleanup_Files();` (Cobertura method node: lines 345 to 347 `hits="1"`). EfcDataModelFilerCleanupTests 3 of 3 and the 11 archive-root pins Passed in P7-T10; QuickFiler.Test.csproj numstat 1/0. fail-before-efc-filer-cleanup.md `EXIT_CODE: 1`. |
| AC19 | PASS | P7-T15 `DELETED-PATHS` includes `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs`; ToDoModel.csproj and both ToDoModel.Test files not in the diff; dossier entry 5; rebuilds green. |
| AC20 | PASS | Grep `^EXIT_CODE:` over evidence/regression-testing: the six fail-before artifacts each read `EXIT_CODE: 1` with `ExpectedExitCode: 1` and name the failing rows; the compile-red record reads `EXIT_CODE: 1`; the dossier carries the eight refactor entries. SS4 is disclosed as coverage of already-correct behaviour with its false-before coverage reading (P7-T1 `A-LINE-143-CONDITION: 50% (1/2)`) rather than a behavioural red, which is the correct classification for a test added to a review residual. |
| AC21 | PASS | P7-T4 census of the two edited files: `DoNotParallelize`, `Thread.Sleep`, `Task.Delay`, `Timeout`, `Directory.CreateDirectory`, `File.`, `GetTempPath`, `MemoryAppender` all 0; no static write; every Phase 7 run under the repository runsettings (`RUNSETTINGS-HASH-NOW` equal to P0-T4 in P7-T5 and all three P7-T10 runs) through the DIRECT route, 7394 of 7394; every `SANDBOX-` value False before and after; `SEQUENCE_FILES: 0`. |
| AC22 | PASS | P7-T15: `OUTSIDE-WRITE-SET: 0`, `WRITE-SET-MISSING: 0`, numstat rows UCS 0/1, UCT 3/0, QFT 1/0, SPEC956 4/2, S 0/9, M 0/9, TST2 37/0; the promotion record and the agent-memory paths subtracted under the inherited clauses (27 and 30). Evaluated PASS on the criterion's intent (no unplanned source change) with the lifecycle record disclosed, as in the prior review. |
| AC23 | PASS | P7-T4 CMD-LINES: twelve files, MAX-LINES 488; SortEmail_AttachmentSaving_Tests.cs 469 (predicted 469; Read confirmed), SortEmail_SaveCase_Tests.cs 342, EfcDataModel.cs 485, SortEmail_Tests.cs 488; AttachmentSaving.cs 329, TrySaveAttachment.cs 204. |
| AC24 | PASS | toolchain-final-pass.md ITERATION 2 (P7-T14): the four commands, exit 0 each, LOOP-RESTARTS 0, SKIP_CORECOMPILE_LINES 0 on both rebuilds with four `_CSC_OUT_LINES: 2`, format WRITESET-CHANGED-COUNT 0, check 1639 files, scoped 56/56, 3/3, 11/11, full 7394/7394. The pass post-dates every source edit on the branch (P6-T13, P7-T2, P7-T3), so the prior review's G-4 caveat is closed. |
| AC25 | PASS | Baseline captured before the first production edit (P0-T11 ordering and the P0-T12 pre-edit census, as the prior review's correction records) and post-change after the Phase 7 pass (P7-T11, 17-19); both JaCoCo projections with the one-line summary; per-member figures all at or above 90% (lowest 95.71%; `SaveAttachmentCore` 19/19 lines and, at the Cobertura node, 4 of 4 branches); EfcDataModel changed lines covered (`E-CHANGED-LINES-COVERED: 3`; `ResetFilerPromptState` lines hit at the method node); first-party 85.39 and 79.81 not lower than 85.36 and 79.75 (`FIRST-PARTY-LINE-NOT-LOWER: True`, `FIRST-PARTY-BRANCH-NOT-LOWER: True`); exemptions content-identified and re-derived at ITERATION 2 (EXEMPT-LINES 36,133,190,191), negative control differs (`CONTROL-DIFFERS-BASELINE: True`); non-exempt set hash equals baseline. |
| AC26 | PASS | P7-T16 CMD-EVIDENCE-FIELDS: 94 files checked, 0 missing fields, 0 non-canonical subfolders, both controls discriminate; CMD-SWEEP over 103 files: every host-identifier and raw-document count 0; P7-T15 `RAW-DOC-PATHS: 0`; the Phase 7 projections carry `ITERATION: 2`, `SUPERSEDES:` and `WRITTEN-BY:` as the plan requires; the six fail-before projections read `EXIT_CODE: 1` and name the failing methods. |
| AC27 | PENDING (pull-request body) | pr-description-inputs.2026-10-06T15-12.md plus the Phase 7 addendum (p7-t16-phase7-closure.2026-10-06T17-25.md: SS4 name, new totals, CR-3 removal, CR-2/U-2 filing note) carry `Closes #959`, `Closes #966`, the four behaviour changes and the UT5 call-out for the pr-author step; the pull request does not exist yet (p6-t45-ac27-deferred.2026-10-06T15-22.md). Not FAIL per the caller's directive; stays unchecked (P7-T16 `AC27-UNCHECKED: 1`). |

## Acceptance Criteria Check-off

- Newly checked by this review: none. Every criterion evaluated PASS was already `- [x]` in spec.md (executor check-offs P6-T19 to P6-T44; Grep `^- \[ \]` over spec.md returns only lines 633 (AC6) and 654 (AC27) inside the `## Acceptance Criteria` section, plus the issue-template boxes at lines 46 to 83 that are not acceptance criteria).
- Left unchecked: AC6 (line 633) and AC27 (line 654), both PENDING on the pull-request body; the orchestrator's pr-author step owns their check-off after the PR body exists, per the deferral records and the plan's PD-11.
- No criterion text was modified; no criterion was added.

## Summary

### Acceptance Criteria Status
- Source: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/spec.md
- Total AC items: 27
- Checked off (delivered): 25
- Remaining (unchecked): 2
- Items remaining: AC6 (L3 phase one; the UT5 call-out must appear in the pull request change description), AC27 (closure; the pull request body must close #959 and #966 and state the four behavior changes and the UT5 call-out)

Verdict: PASS. Twenty-five criteria PASS; two are pending the pull-request body and are not failures. CR-1 and CR-3 of the prior review are confirmed closed at the Cobertura node and in the source file respectively. Non-blocking items for the orchestrator are listed in code-review.2026-10-06T17-40.md (CR-2, CR-4 to CR-8) and policy-audit.2026-10-06T17-40.md section 8 (G-1, G-2, G-3, G-5 to G-9). No remediation-inputs artifact is produced because no finding blocks.
