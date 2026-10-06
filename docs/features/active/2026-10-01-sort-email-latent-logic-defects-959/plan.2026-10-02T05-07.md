# 2026-10-01-sort-email-latent-logic-defects (Plan)

- **Issue:** #959 (the pull request also closes #966; spec D18)
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-06 (revision 2.0, in-place append of Phase 7 and Phase 8 after the executed P0-T1 to P6-T46 at HEAD eb7871502, the orchestrator's statement: Phase 7 remediates the two code-review residuals CR-1 and CR-3 of FEATURE/code-review.2026-10-06T15-30.md (PD-16) and runs a full toolchain pass over the whole solution; Phase 8 is the PR-time reconciliation with origin/main (PD-17), executed only after the orchestrator's re-review of Phase 7 passes; no executed task P0-T1 to P6-T46, its text or its check box was altered and no existing phase was renumbered; revision 1.9 was the in-place widening after P6-T12 at HEAD 9de3f176d: the stale `Cleanup_Files_DoesNotThrow` documentation comment of TST1 corrected by the inserted P6-T13, with its formatter, census, footprint and diff-identity gates in P6-T14 to P6-T16, and the former P6-T13 to P6-T43 renumbered P6-T16 to P6-T46; revision 1.8 was the in-place execution-time amendment after the P4-T9 `FAIL-BEFORE WRONG REASON` stop: the P4-T9 message clause restated against the observed FluentAssertions truncated message, the Test Inventory row aligned, no other task changed; revision 1.7 was the preflight delta on the revision 1.6 amendment; revision 1.6 the execution-time amendment for the P4-T7 CS1769 seam defect and the per-task commit rule)
- **Status:** P0-T1 to P6-T46 executed and committed (114 of 114, 2026-10-06; branch (a) of P6-T7; AC1 to AC5 and AC7 to AC26 checked, AC6 and AC27 deferred to the PR step, PD-11); P7-T1 to P7-T16 executed and committed (2026-10-06); the orchestrator's re-review of Phase 7 passed (PHASE 7 RE-REVIEW: PASS, artifacts of label 2026-10-06T17-40 committed at 061784a95); Phase 8 in progress (P8-T1 and P8-T2 executed, merge commit 9163994569e24c5c539a285724f9c8f9f6fd8a0e of origin/main f8ea1b5dcc6514bc0088bc80965c188bfd717557, no conflict; P8-T3 to P8-T13 pending; revision 2.0, PD-17)
- **Version:** 2.0
- **Work Mode:** full-bug (FEATURE/issue.md line 12, `- Work Mode: full-bug`). AC source: FEATURE/spec.md section `## Acceptance Criteria` (spec.md line 627 after the planner revision note was inserted at line 12), AC1 to AC27 at spec.md lines 628 to 654, all unchecked at authoring time; the AC lines read `- [ ] ACn (<title>).`. No user-story.md exists or is required.
- **Research:** FEATURE/research/2026-10-02T05-15-sort-email-latent-logic-defects-research.md (R1) and FEATURE/research/2026-10-02T05-50-sort-email-966-consolidation-research.md (R2; R2 section 0.2 supersedes R1 where it says so; R2 section 10 is the write set; R2 section 11 the red-first order).
- **Branch:** bug/sort-email-latent-logic-defects-959, cut from origin/main 94287369908cc920b21b0e3256314f988ad7d2f5 (the SHA at which every citation below was derived). Anchor: the `MERGE-BASE:` value P0-T3 records after `git fetch origin main` (`git merge-base HEAD origin/main`); every diff gate is a two-dot comparison of the working tree against that recorded SHA paired with a `git status --porcelain` span (rules G8 and G8b); under the per-task commit rule of the Execution Conventions (revision 1.6), the porcelain span is taken before the task's own commit and a gate over the committed state of earlier tasks uses the `MERGE-BASE HEAD` form. Revision 2.0: these anchor rules apply to Phases 0 to 7; Phase 8 merges origin/main into the branch (P8-T2) and anchors its own gates at the fetched origin/main SHA that P8-T1 records as `ORIGIN-MAIN-SHA:` (PD-17); no Phase 0 to 7 gate is re-run against the moved anchor.
- **Execution session requirement:** the executor runs later, non-isolated, from the item worktree, with `pwsh` available (an isolated agent is refused `pwsh` in every form). Every command-bearing task runs either one `git -C WORKTREE ...` invocation or one `pwsh -NoProfile -Command '<payload>'` process whose first statement is `Set-Location -LiteralPath "WORKTREE"`. P0-T4 probes that channel first and stops with `CHANNEL UNAVAILABLE` if it is refused. The executor never edits artifacts/orchestration/orchestrator-state.json, never runs `git update-index`, and never edits hook, permission or policy files. Script invocations use absolute script paths resolved at run time (`Join-Path (Get-Location).Path "scripts\vscode\<name>.ps1"`), never `pwsh -WorkingDirectory`.
- **Pre-implementation gate requirement:** artifacts/orchestration/orchestrator-state.json is seeded at authoring time with `issue-num` `959` (line 10), `feature-folder` docs/features/active/2026-10-01-sort-email-latent-logic-defects-959 (line 15), `route_id` `preparation` (line 3) and `lifecycle_ready` `true` (line 17). P0-T3 records the readiness fields read-only and stops with `PRE-IMPLEMENTATION GATE NOT SEEDED` when they are absent; a PreToolUse refusal at any later source edit is `PRE-IMPLEMENTATION GATE BLOCKED`, reported verbatim, and stops the run.
- **Task Count:** 143 (Phase 0: 12, Phase 1: 12, Phase 2: 11, Phase 3: 6, Phase 4: 15, Phase 5: 12, Phase 6: 46, Phase 7: 16, Phase 8: 13)

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QA artifact tasks, and coverage-comparison tasks for each in-scope language when policy requires coverage. If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Record the expected artifact path or location in each evidence-producing task. Do not mark evidence-backed work complete without the artifact.

---

## Blast Radius and Write Set

The Write Set is exactly these eighteen repository paths (R2 section 10; spec D15) plus FEATURE/** (evidence artifacts, this plan's check-offs, the AC check-off boxes of FEATURE/spec.md, of which twenty-five are checked by this plan under branch (a) of P6-T7 (twenty-three under branch (b), see P6-T46) while AC6 and AC27 stay unchecked under PD-11, and the planner revision note already present in FEATURE/spec.md). The footprint gate P6-T12 enforces exactly this set, and P7-T15 re-enforces it after the Phase 7 edits (revision 2.0: Phase 7 changes Write Set items 12, TSC, and 13, TAS, only; Phase 8 adds no path of its own, and the paths its merge commit brings from origin/main are measured by the P8-T3 fan-in gate against the fetched origin/main SHA, not against this set).

1. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs` (modify; alias A)
2. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs` (modify; alias T)
3. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs` (modify; alias U)
4. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` (modify, using block only; alias S)
5. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs` (modify, using block only; alias M)
6. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs` (delete; alias L)
7. `UtilitiesCS/UtilitiesCS.csproj` (modify: remove the one Compile Include line for L; numstat 0 added, 1 deleted)
8. `QuickFiler/Controllers/EfcDataModel.cs` (modify; alias E)
9. `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` (delete; alias TD)
10. `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` (modify; alias TST1; the revision 1.9 documentation-comment correction of P6-T13, PD-15, lies inside this path)
11. `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` (modify; alias TST2)
12. `UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs` (create; alias TSC)
13. `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` (create; alias TAS)
14. `UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs` (create; alias TUL)
15. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (modify: three Compile Include lines; numstat 3 added, 0 deleted)
16. `QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs` (create; alias TEF)
17. `QuickFiler.Test/QuickFiler.Test.csproj` (modify: one Compile Include line; numstat 1 added, 0 deleted)
18. `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md` (modify: three text edits; numstat 4 added, 2 deleted; alias SPEC956)

Not edited (scope boundaries, spec D15): UtilitiesCS/Dialogs/YesNoToAll.cs, UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs, UtilitiesCS/OutlookObjects/Attachment/AttachmentHelper.cs, UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs, TaskMaster/AppGlobals/AppOlObjects.cs, QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs, both ToDoModel.Test source files, ToDoModel/ToDoModel.csproj, .editorconfig, the #956 code-review record, UtilitiesCS/Properties/AssemblyInfo.cs, scripts/, config/, coverage.config, scripts/vscode/TaskMaster.cli.runsettings, .csharpierignore, .gitignore; FEATURE/spec.md text other than the twenty-seven check-off boxes; FEATURE/issue.md and FEATURE/research/. Local, git-ignored, never staged: coverage/ (except coverage/.gitkeep; the .gitignore entries `coverage/*` and `!coverage/.gitkeep`), every trx (the `*.trx` entry) and cobertura-named XML document (the `*cobertura*.xml` entry), packages/ (the `**/[Pp]ackages/*` entry), .dotnet-sdk/ (the `.dotnet*/` entry), bin and obj (the `[Bb]in/` and `[Oo]bj/` entries).

## Orchestrator Decisions (binding, not reopened)

Spec decisions D1 to D18 (FEATURE/spec.md lines 127 to 144) are binding and are not restated here. Items the plan relies on by number: D1 (L1 labels, exclusion removed, tests in TSC), D2 (private core with `isRetryAfterClear`, logger calls, outer catch removed), D3 (two-phase L3), D4 (L4 seam, single header line, `SanitizeArray` deleted with its test), D5 (F1 sessions, `AllPromptSessions` property, seamed cores), D6 (`RedirectSaveFolder`), D7 (F2 deletions), D8 (F3 dispositions and the two added `DataRow` rows), D9 (CR-1 three edits), D10 (using blocks), D11 (EfcDataModel `try`/`finally` with `ResetFilerPromptState`), D12 (ToDoModel deletion), D13 (red-first workflow), D14 (test policy), D15 (write set), D16 (coverage), D17 (toolchain and evidence), D18 (closure).

## Planner Decisions

- PD-1 (phase order). R2 section 11's nine steps are grouped into six implementation-and-QA phases: Phase 1 L1 and L3 phase one (both in A, both red with no production pre-step); Phase 2 L4 and the header (U, TST1, TUL); Phase 3 L2 and the try-save logging (T, TST2); Phase 4 F1 with the re-rooting defect, F2, the A-side F3 removals, the A using block and the L3 structural test (A, L, UtilitiesCS.csproj, TSC, TAS, TST1); Phase 5 the S and M using blocks, EfcDataModel, the ToDoModel deletion and CR-1; Phase 6 the final QA loop, coverage comparison, footprint and check-offs. Each defect's regression test is observed red before its fix inside its own phase, so no green gate ever runs over a path that still holds a red-first test (task-ordering rule).
- PD-2 (listings and edits). Files that are created, or rewritten wholesale because their anchors are not unique after earlier edits, are written with the Write tool from a verbatim Listing: TSC (two states, L-TSC-P1 and L-TSC-FINAL), TAS (two states, L-TAS-P1 and L-TAS-FINAL), TUL (L-TUL), TEF (L-TEF), A final (L-A-FINAL; written by P4-T4 from the revision-1.5 form and rewritten by P4-T7 from the revision-1.6 form, PD-14), T final (L-T-FINAL), U final (L-U-FINAL); P4-T7 also rewrites TSC and TAS from the revision-1.6 L-TSC-FINAL and L-TAS-FINAL. Every other production or test change is an Edit with the exact old and new text given in the Edit Specifications section; each old text was verified unique in its file at the cited tree. Source and project files are written only with the Write and Edit tools, so the pre-implementation gate observes every source write; `pwsh` payloads read source files and write only under git-ignored paths (coverage/, and the bin/, obj/, packages/ and .dotnet-sdk/ outputs of MSBuild, the restore and the SDK installer; the .gitignore entries `coverage/*`, `[Bb]in/`, `[Oo]bj/`, `**/[Pp]ackages/*` and `.dotnet*/`), except the CSharpier runs of `CMD-SCOPED-FORMAT` and `CMD-FORMAT-REPO` (in-place rewrites of Write Set files, observed by BEFORE and AFTER hashes), the two deletions of PD-3 and the stop-path fallback `CMD-RESTORE` of P4-T15 (a byte copy of the P4-T13 backup over A, run only when the Edit restore does not reproduce `FIX-HASH-A:`).
- PD-3 (deletions). The Write and Edit tools cannot delete a file, and this plan runs no `git rm`. The two deletions (L in P4-T5, TD in P5-T10) use `CMD-DELETE`, one `Remove-Item -LiteralPath` statement on the repository-relative path, recorded with `EXISTS-BEFORE:`/`EXISTS-AFTER:`. These are the only `pwsh` writes to a tracked source path other than the CSharpier formatter runs and the `CMD-RESTORE` fallback (PD-2 enumerates all of them). A tool refusal of the deletion payload is `DELETE CHANNEL REFUSED`: stop and report; the executor does not substitute another route.
- PD-4 (test totals derived by counting). Every `COUNTERS total=` expectation below is the sum of test methods plus `DataRow` rows, per file and per plan state: TST1 15 at baseline (fifteen `[TestMethod]` at lines 41, 54, 79, 104, 140, 157, 174, 182, 209, 245, 272, 291, 316, 341, 359); 14 methods after P2-T8 deletes `SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows`; 18 rows after P4-T6 turns the two `GetAttachmentsInfo` tests into three-row data tests (12 single methods plus 2 times 3 rows). TST2 11 at baseline, 12 after P3-T1 (T12). TSC 5 rows in the P1 state (2 + 2 + 1), 12 rows in the final state (5 + 2 + 1 + 1 + 1 + 1 + 1). TAS 4 rows in the P1 state (one four-row data test), 11 in the final state (6 + 3 + 1 + 1). TUL 2. TEF 3. The `FILTER-SORTEMAIL` total is therefore 26 at baseline, 35 after Phase 1, 36 after Phase 2, 37 after Phase 3 and 55 from Phase 4 onward. R2 section 13 predicted 52 by counting the `SaveCaseAsync` tests as six rows and the TST1 additions as two; the counts above supersede that prediction (seven `SaveCaseAsync` rows because the Yes/YesToAll test has two rows; six added TST1 rows under PD-6). `EfcDataModelArchiveRootTests` carries eleven `[TestMethod]` attributes, so `FILTER-EFC-ARCHIVE` totals 11.
- PD-5 (TRX row identity). Every `[DataRow]` carries `DisplayName = "<method name> [<row tag>]"`, so the TRX `testName` of each row is deterministic and a `RESULT` line can be matched by its full name. This repository's committed run summaries show that a data-driven test with fifteen rows plus one plain test reports `COUNTERS_TOTAL=16` and sixteen `RESULT` rows with no parent aggregate row (docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/regression-testing/pass-after-shape-b.2026-09-16T23-27.md lines 31 to 47), so rows are counted one each in every total above. The in-repo attribute form is `[DataTestMethod]` with `[DataRow]` (UtilitiesCS.Test/Threading/CurrentStoreContextTests.cs lines 84 to 88).
- PD-6 (F3 `GetAttachmentsInfo` rows; a research correction). R2 section 3 item 1 adds one `(saveAttachments: true, savePictures: true)` row "so both `if` bodies are covered". That row covers neither filter body: `if (!saveAttachments)` runs only when saveAttachments is false and `if (!savePictures)` only when savePictures is false. The synchronous test's existing row is (false, true) and the asynchronous test's is (true, false), so each method would keep one filter statement uncovered once its exclusion is removed, which the coverage comparison of P6-T8 would report as a new uncovered line. Each test therefore gains TWO rows: the spec's (true, true) row (AC13 names it) and the complementary row that covers the other filter body ((true, false) for the synchronous test, (false, true) for the asynchronous test). AC13's clause "gains one DataRow row with saveAttachments true and savePictures true while keeping its existing row" holds; the complementary row is additional and is reported to the orchestrator in the handoff as a research correction.
- PD-7 (negative controls). The spec's Test Strategy asks for each code fix "reverted alone with its tests kept". Every red-first observation in this plan is that state: the regression test exists and the fix is absent (P1-T6, P1-T7, P2-T6, P3-T3, P4-T9, P5-T7), each recorded with the exact failing-row set. The one test that cannot be observed red-first is the structural `Cleanup_Files_ResetsEveryPromptSession` (a refactor replacement), so it receives a mutation control: one element removed from `AllPromptSessions`, the test observed failing, the element restored by the inverse Edit and the file hash proven equal (P4-T14, P4-T15). FEATURE/evidence/qa-gates/negative-controls.md records the mutation control and a table that maps each other fix to its red-first artifact and failing-row set. Re-mutating production six more times would add six builds and six runs with no additional discrimination.
- PD-8 (coverage comparison rule for AC25). `CMD-COVERAGE-TEXTS` aggregates the uncovered lines of every class whose Cobertura filename matches `*EmailParsingSorting\SortEmail*.cs` (one merged class per file after `Merge-CoberturaClassesByFilename`) and derives, from the source text of T, four content-identified exemption sets: (1) lines containing `System.IO.Directory.CreateDirectory(path)` (the unchanged #945 wrapper lambda); (2) a line whose trimmed text is `}` and whose three preceding trimmed lines are `else`, `{`, `throw;`; (3) a line whose trimmed text is `}`, which directly follows a set-(2) line, whose next line is either a line starting with `catch (System.Exception)` (the baseline shape) or the method's closing brace (trimmed `}` at indentation eight, the final shape), and whose nearest preceding `catch (` line at the same indentation is `catch (System.UnauthorizedAccessException e)`; (4) the `}` that directly follows the first `throw;` after the guard condition line, where the guard condition line is the single line containing `isRetryAfterClear` that contains neither `bool isRetryAfterClear` nor `isRetryAfterClear:` (zero such lines at baseline, so set (4) is empty there). Every uncovered line of the family that is not in the T exemption union is printed as `NONEXEMPT-UNCOVERED <file>:<line> :: <trimmed source text>`; the sorted set of `<repository-relative path>::<trimmed text>` entries is hashed (`NONEXEMPT-SET-SHA256:`). The gate is that the final hash equals the baseline hash recorded by P0-T11 (the same statements, wherever their line numbers moved; predicted single entry: the `ForEachAsync` statement of M that was already uncovered at baseline), plus `EXEMPT-GUARD-BRACE-COUNT: 1` at final, plus an in-memory negative control that treats the lowest-numbered covered non-exempt line of T as uncovered and must change the hash (`CONTROL-DIFFERS-BASELINE: True`). Per-member coverage for the ten AC25 members is measured by `CMD-MEMBER-COVERAGE` over source spans that begin at a unique signature line and end at the first following line that is exactly eight spaces and `}`; the EfcDataModel changed lines are gated by name (`result = await InvokeFilerAsync(config, mailHelpers);`, `ResetFilerPromptState();`, `return result;` each with hits above zero). Package-level and repository-level rate comparisons are observations, except the AC25 clause that the first-party line and branch percentages are not lower than baseline, which is gated as printed to two decimals.
- PD-9 (inherited paths). Clause A: every path already changed relative to `MERGE-BASE:` when P0-T3 captures it (the orchestrator's preparation commits under FEATURE/, the promoted record docs/features/potential/promoted/2026-10-01-sort-email-latent-logic-defects.md when listed, and the P0-T1 and P0-T2 artifacts), recorded as `INHERITED-CLAUSE-A:`. Clause B: every path under .claude/agent-memory/. Footprint gates subtract Clause A and Clause B, record the subtraction, and never subtract a Write Set path. `CMD-EVIDENCE-FIELDS` (P6-T16, P6-T44, P6-T46) subtracts Clause A minus the P0-T1 artifact and the P0-T2 artifact, because both were written by this run before P0-T3 captured Clause A and must be field-checked like every other artifact of this run.
- PD-10 (evidence names). The spec's Test Strategy names fixed, digit-free artifact files; this plan uses them verbatim except the fail-before exception dossier, which the evidence conventions require to match `fail-before-exception.*.md`: it is written as FEATURE/evidence/regression-testing/fail-before-exception.<TS>.md and realizes the spec's "fail-before exception dossier named in Test Strategy" (the spec's `fail-before-exceptions.md` spelling does not match the mandatory search pattern; reported to the orchestrator in the handoff). Every other artifact is `<task-id>-<name>.<TS>.md`.
- PD-11 (AC6 and AC27 and the pull request). AC27 and the pull-request clause of AC6 ("in the pull request change description") describe the PR body, which this plan does not author, so both are deferred to the orchestrator's pr-author step and neither is checked off by this plan. P6-T18 writes FEATURE/evidence/qa-gates/pr-description-inputs.<TS>.md with the closing references, the four behavior changes and the UT5 call-out verbatim, for that step. P6-T24 records the AC6 deferral together with the plan-side evidence that is already complete (the phase-one red and green runs, the census counts); P6-T45 records the AC27 deferral; each is a `DEFERRED TO PR STEP` record that leaves the box unchecked, and P6-T46 expects exactly that state (twenty-five checked, AC6 and AC27 unchecked, under branch (a) of P6-T7; see P6-T46 for branch (b)). The plan outcome is reported as complete with AC6 and AC27 deferred, never as PASS over an unchecked AC.
- PD-12 (spec wording). The planner amended FEATURE/spec.md before handoff: "after the seam commit" became "after the seam step" at the fail-before-write-csv.md bullet and in AC15, with one revision-note line inserted at line 12 (the orchestrator's permission in the delegation prompt). The AC section heading therefore sits at line 627 and AC1 to AC27 at 628 to 654.
- PD-13 (uncompiled file deletion). TD is not compiled by any project (ToDoModel/ToDoModel.csproj lists no `Compile Include` for it; the only `SortItemsToExistingFolder` entries in any project file are the two ToDoModel.Test test files, ToDoModel.Test/ToDoModel.Test.csproj lines 75 to 76), so its deletion cannot change a build and needs no project-file edit (D12).
- PD-14 (try-save seam type; an execution-time correction, 2026-10-03). The first P4-T7 build failed with sixteen CS1769 errors (FEATURE/evidence/qa-gates/p4-t7-format-and-build.2026-10-03T10-21.md): UtilitiesCS embeds the Outlook interop types (UtilitiesCS/UtilitiesCS.csproj line 223, `<EmbedInteropTypes>True</EmbedInteropTypes>` on Microsoft.Office.Interop.Outlook) while UtilitiesCS.Test references the interop assembly without embedding (UtilitiesCS.Test/UtilitiesCS.Test.csproj line 749, `False`), and the C# compiler refuses a generic instantiation whose type argument is an embedded interop type (`Func<Attachment, string, Task<bool>>`) in a signature used across the assembly boundary, whereas it accepts a non-generic signature over the same type (interop type equivalence), which is why the tests already compile against `SaveCase(YesNoToAllResponse, Attachment, string, string)` and the three-argument `TrySaveAttachmentAsync`. The seam type is therefore the nested non-generic delegate `internal delegate Task<bool> TrySaveAttachmentDelegate(Attachment attachment, string filePath)` declared inside `SortEmail` in A (Listing L-A-FINAL, directly above the asynchronous core), with the parameter list and return type the spec's `Func<Attachment, string, Task<bool>>` spells out; the two cores take `TrySaveAttachmentDelegate trySave`, the excluded wrapper still passes the `TrySaveAttachmentAsync` method group (the two-argument overload remains the only one that converts), and the two test helpers `RecordingSave` return `SortEmail.TrySaveAttachmentDelegate`. Precedent search (revision 1.6 pass, Grep over WORKTREE): no `delegate` declaration in UtilitiesCS has an Outlook interop type among its parameters (the declared delegates are `YesNoToAllDelegate`, `ResponseDelegate`, `FolderGroupTransformer<T>` over `FolderWrapper[]`, `AltLoader`, `CSVLoader<T>`, `AltListLoader`, `StoreLockupNotifier` over `string` and `Action`, `ApplicationIdleEventHandler`); no `Func<` or `Action<` over an Outlook interop type occurs anywhere in UtilitiesCS other than the two seam lines P4-T4 wrote; UtilitiesCS.Test injects Outlook-typed values only as direct `Attachment` or `MailItem` arguments or as `Mock<Attachment>` objects (a test-local generic instantiation, which CS1769 does not concern) and its only Outlook-adjacent callback is `Func<MailItemHelper, Task>` over a UtilitiesCS type; so no precedent for a delegate over an interop type exists, and the nearest precedent is `StoreLockupNotifier` (UtilitiesCS/Threading/StoreLockupResponder.cs lines 20 to 25), a public non-generic delegate declared as a test seam in CSharpier's wrapped form, which the new declaration follows. The rest of the plan was swept for the same defect: the other new seams are `Func<string, bool> fileExists`, `Action<string, string[], string> writeTextFile`, `Action<string> createDirectory` and `Action<string> clearReadOnly` (string-typed); the EfcDataModel seam `InvokeFilerAsync(EmailFilerConfig, IList<MailItemHelper>)` and `ResetFilerPromptState()` carry UtilitiesCS and QuickFiler types only (the override form already compiles in QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs); the ToDoModel step is a deletion; the F1 session parameters are `YesNoToAllPromptSession`; none crosses an assembly boundary with a generic instantiation over an interop type. Spec D5, the Technical specifications and the Data / API impact list of FEATURE/spec.md spell the seam as `Func<Attachment, string, Task<bool>>`; AC8 defers the exact type to the Technical specifications ("exact delegate types in Technical specifications"), so no AC text changes, and because AC8 incorporates that type the planner corrected those technical sections in place (spec lines 12, 132, 195, 196, 264, 285 and 457, line count unchanged); P6-T26 verifies the correction and P6-T18 carries it beside PD-6 and PD-10. P4-T1, P4-T2 and P4-T4 are executed and keep their check boxes; P4-T7 rewrites A (with the same omitted line as P4-T4), TSC and TAS from the revision-1.6 listings before its format and build, and TOKENS-A gains the SEAMED state.
- PD-15 (documentation-comment drift in TST1; revision 1.9, under the maintainer directive that related defects are remediated inside this item). The summary element of `Cleanup_Files_DoesNotThrow` (TST1 lines 170 to 173 at HEAD 9de3f176d) still says that `Cleanup_Files` "resets all static YesNoToAllResponse tracking fields". After F1 (D5, AC7) the AttachmentSaving partial holds no field of type `YesNoToAllResponse`: `Cleanup_Files` (A lines 40 to 46) is a `foreach` over `AllPromptSessions` (A lines 31 to 38) calling `Reset()` on the four sessions, including `AttachmentsAltNamePrompt`, which replaced the `_attachmentsAltName` field that the transient L3 phase-one reset of P1-T9 targeted; that field, its initializer and the phase-one statement were deleted by P4-T4 (TOKENS-A FINAL `_attachmentsAltName=YesNoToAllResponse.Empty;` 0 and `YesNoToAllResponse_` 0). Line 171 is replaced by Edit E-TST1-DOC-CLEANUP in P6-T13 so that the comment describes the session reset; line 172 is reworded with it so that the sentence remains grammatical. This is a documentation-comment-only change: no statement, attribute, test name or assertion changes and no behavior changes, so the Bugfix Workflow's failing regression test does not apply and none is written; the verification is the two Grep-tool searches of P6-T13 (the new single-line token present once, the old phrase absent), the scoped formatter pass, the repository-wide read-only check and the TST1 census of P6-T14, the footprint of P6-T15 and the anchored diff identity of P6-T16 (`TST1-REMOVED-DOC-LINES: 1`, `TST1-ADDED-DOC-LINES: 1`). The sibling test files were searched for the same drift in the revision 1.9 pass: `tracking fields`, `static YesNoToAllResponse`, `_attachmentsAltName` and `_responseSaveFile` occur in no UtilitiesCS.Test/EmailIntelligence/SortEmail_*.cs file other than TST1 line 171, and the `#region` header at TST1 line 127 names `Cleanup_Files` only and remains accurate. The production comments that describe the reset (A lines 15 to 17, T lines 12 to 14) already state the session behavior and are not touched.
- PD-16 (code-review residuals; revision 2.0, under the maintainer directive that related defects are remediated inside this item). The initial feature review (FEATURE/code-review.2026-10-06T15-30.md with the policy-audit and feature-audit of the same label; PASS, zero blocking findings) reported seven non-blocking findings CR-1 to CR-7. The orchestrator's dispositions are binding. CR-1 REMEDIATE (Phase 7): the `IsImage` true arm of the synchronous `SaveAttachment` core, A line 143 (`var overwritePrompt = attachmentHelper.AttachmentInfo.IsImage` with the two arms at 144 and 145), reads `condition-coverage="50% (1/2)"` in the Phase 6 Cobertura document (coverage\final-959.cobertura.xml, class node for the file, method `SaveAttachment` `branch-rate="0.75"`) because SS1 to SS3 all use `report.pdf`; TAS gains one synchronous test SS4, `SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly` (Edit E-TAS-SS4), mirroring AS2: a helper built from `photo.jpg`, `Exists(true, new List<string>())`, a pictures session scripted `Yes`, an unscripted attachments session; it asserts the pictures session was asked once with the overwrite text, the attachments session never, `SaveAsFile` once on `FilePathSave` and never on `FilePathSaveAlt`, and the answer released (`Empty`). The test pins behaviour that is already correct (the asynchronous core's selection is pinned on both arms by AS2 and AS3, and the review verified the synchronous selection by reading), so this is test coverage, not a behaviour fix: the Bugfix Workflow's failing-regression-test step does not apply and no fail-before run is made. The discriminating observation is the condition coverage of A line 143 read from the Phase 6 document before the edit (`50% (1/2)`, P7-T1, the false-before state) and from the Phase 7 document after the Phase 7 coverage run (`100% (2/2)`, P7-T13); the test itself is observed green in a scoped run (P7-T5). CR-3 REMEDIATE (Phase 7): the unused `using System;` at TSC line 1 is removed by Edit E-TSC-USING after P7-T1 confirms by a Grep-tool search that TSC names no `System`-namespace type (the revision 2.0 pass found none: the alternatives `Func<`, `Action`, `DateTime`, `Exception`, `Guid`, `Math\.`, `Console`, `Environment`, `StringComparison`, `Array\.`, `Convert\.`, `Tuple`, `Nullable`, `Lazy<`, `IDisposable`, `EventArgs`, `TimeSpan` and `Enum\.` match zero lines; `Task.FromResult` lives in `System.Threading.Tasks`, `List` and `Queue` in `System.Collections.Generic`, and `RecordingSave` has returned `SortEmail.TrySaveAttachmentDelegate` rather than a `Func` since the P4-T7 rewrite); if P7-T1 finds a `System` type, P7-T3's own text authorizes the skip and records `CR-3 SKIPPED: System required` with the matching lines. CR-2 no change: QuickFiler/Legacy/QfcController.cs lines 781 to 792 (`SortEmail.Run(...)` then `SortEmail.Cleanup_Files();` with no `try`/`finally`) are not compiled (QuickFiler.csproj has no `Compile Include` under `Legacy\`) and reference a `SortEmail.Run` that no partial declares, so no fix can be built or verified; reported for filing together with U-2 (folder-level cleanup of QuickFiler/Legacy/). CR-4 no change: EfcDataModel.cs (485 lines) and SortEmail_Tests.cs (488) are under the 500-line limit; a split is not in scope. CR-5 no change: the copied fixture helpers follow spec D11 (EfcDataModelArchiveRootTests.cs is unchanged by design). CR-6 informational: no change. CR-7 no change: acting on it would alter the wording of AC3 and AC13, and no acceptance-criterion text is changed by this revision (the feature-audit evaluated both on their stated intent). Phase 7 also supplies the rebuilds and test runs that the P6-T13 documentation-comment edit did not receive (policy-audit G-4), because its full pass runs over the whole solution. Predicted sizes after Phase 7: TAS 469 lines (437 plus the thirty-two lines of E-TAS-SS4), TSC 342 (343 minus 1), both under the ceiling; `MAX-LINES:` stays 488 (TST1). Expected totals: `FILTER-ATTSAVE` 12, `FILTER-SORTEMAIL` 56, the full coverage run 7394 under the unchanged P0-T9 exclusion.
- PD-17 (PR-time reconciliation with origin/main; revision 2.0). Phase 8 runs only after the orchestrator's re-review of Phase 7 passes; P8-T1 records that authorization from the delegation prompt and stops without it. P8-T2 is the one `git merge` of this plan (`git merge --no-ff origin/main`, never a rebase), which the Execution Conventions otherwise prohibit; it is the planned anchor move: the `MERGE-BASE` rules of Phases 0 to 7 are not re-run after it, and Phase 8 anchors every diff at the fetched origin/main SHA that P8-T1 records (`ORIGIN-MAIN-SHA:`), which becomes the branch's merge base with main once the merge commit exists (P8-T3 proves that by ancestry: `ORIGIN-MAIN-SHA` is an ancestor of HEAD, the Phase 0 to 7 anchor is an ancestor of HEAD, and HEAD is not an ancestor of `ORIGIN-MAIN-SHA`). Two paths are changed on both sides. The first is QuickFiler.Test/QuickFiler.Test.csproj: this branch adds `    <Compile Include="Controllers\EfcDataModelFilerCleanupTests.cs" />` directly after the `Controllers\EfcDataModelArchiveRootTests.cs` entry (line 128 at HEAD eb7871502), while origin/main adds `    <Compile Include="Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs" />` directly after the `Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs` entry (line 204 at HEAD) and `    <Compile Include="TestSupport\SynchronousBackgroundWorker.cs" />` then `    <Compile Include="TestSupport\ArmingFakeTimeProvider.cs" />` directly after the `TestSupport\DedicatedWorkerThread.cs` entry (line 229 at HEAD). The second is .claude/agent-memory/orchestrator/MEMORY.md: this branch inserts one index line after base line 101 and origin/main f8ea1b5dcc inserts one index line after base line 137 (observed by the preflight reviewer with git diff from 94287369908cc920b21b0e3256314f988ad7d2f5 to origin/main and to HEAD 66b444507 on that path); the hunks are thirty-six lines apart and no conflict is predicted; a conflict there falls under the abort rule below. `CMD-FANIN` proves after the merge that no path changed on both sides lost a line relative to `ORIGIN-MAIN-SHA` (`SHARED-WITH-LOSS: 0`). The main-side shape was observed in the revision 2.0 pass in the sibling worktree agent-a291a7fbabf9d0229 (the only worktree under the main checkout's `.claude/worktrees/` that holds QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs), at its lines 204, 230 and 231; the planner had no git channel, so P8-T1 re-derives the main-side hunk with `git diff 94287369908cc920b21b0e3256314f988ad7d2f5 origin/main -- QuickFiler.Test/QuickFiler.Test.csproj` and records it. The two sides' hunks lie more than seventy lines apart, so a textual conflict is not expected and `CSPROJ-CONFLICT: NONE` is the predicted record; if git reports a conflict in that file alone, P8-T2 resolves it as the union, the resolved file carrying each of the four lines above exactly once, in each side's original relative order, with no conflict marker; a conflict in any other path is `MERGE CONFLICT OUTSIDE EXPECTED UNION`: `git merge --abort`, stop and report the conflicted paths. After the merge the fan-in gate P8-T3 proves with `CMD-FANIN` that, relative to `ORIGIN-MAIN-SHA`, the branch changes only its own paths (the eighteen Write Set paths, FEATURE/**, the promoted record docs/features/potential/promoted/2026-10-01-sort-email-latent-logic-defects.md and .claude/agent-memory/**), that every path changed on both sides shows additions only relative to `ORIGIN-MAIN-SHA` (`SHARED-WITH-LOSS: 0`, the csproj at `1 0`), and, by negative controls over committed ranges, that the path-set check can fire. The full toolchain pass on the merged tree (P8-T4 to P8-T11) reuses the Phase 6 and Phase 7 payloads with `STAGE` `final`; its first-party coverage figures are gated against the floors and recorded against the Phase 7 figures (recorded, not gated, because the merged tree carries main's own code and tests), the condition coverage of A line 143 stays gated at `100% (2/2)`, and the fixed-name final-pass artifacts are rewritten at `ITERATION: 3`. Every Phase 8 task commits and pushes under the per-task rule, so the merge commit reaches origin as soon as P8-T2 completes; P8-T12 is the named push step and proves that the remote tip equals HEAD.

## Execution Conventions

- `FEATURE` denotes docs/features/active/2026-10-01-sort-email-latent-logic-defects-959. `WORKTREE` denotes the absolute item-worktree root supplied by the delegation prompt, without a trailing separator. Both tokens are expanded by the executor; `WORKTREE` is never written into any artifact. Every `git` command in this plan is written in its canonical form and is issued as `git -C WORKTREE <arguments>`; `Command:` fields record the canonical form. Uppercase placeholder tokens (`PATHS`, `TOKENS`, `TASKID`, `FILTERARG`, `ASSEMBLY`, `PROJECT`, `DLL`, `STAGE`, `GATEARGS`, `EXCLUSION`, `INHERITED`, `MERGE-BASE`, `PATH`, `PROJ`, `ENTRIES`, `BASELINE-HASH`) are substituted by the executor as each task states; no shell variable survives a task boundary, so a recorded value is substituted as text.
- **Per-task commit and push (maintainer budget directive, 2026-10-03; revision 1.6).** This rule supersedes the "no commits by this plan" rule of revisions 1.0 to 1.5 and the sentence "The executor creates no commit inside a span" in P4-T1 (executed; its text is kept as history) and in the compile-red bullet below. P0-T1 to P4-T6 were committed and pushed during execution (Phases 0 to 3 in phase-sized commits, P4-T1 to P4-T4 together, P4-T5 and P4-T6 one each). From P4-T7 onward the executor commits and pushes after EVERY task, and the commit falls at one fixed point in each task: after the task's evidence artifact is written, after its acceptance has been checked against that artifact and after its check box has been ticked in this plan, and before the next task starts. The commit stages explicit pathspecs only (`git add -A -- <pathspecs>`): the Write Set paths the task changed or deleted, FEATURE/ (the artifact and this plan's check box) and .claude/agent-memory/ when a note was written; never coverage/, bin/, obj/, packages/ or .dotnet-sdk/ (git-ignored) and never artifacts/orchestration/. The message is `wip(959): <TASK-ID> <summary>` with the suffix ` (compile-red span open)` when the task lies inside an open span, and the push is `git push origin bug/sort-email-latent-logic-defects-959`. A task that stops is committed the same way with its stop record and the stop string in the summary (the P4-T7 stop of 2026-10-03T10-21 was), except the P5-T10 coordinator handoff, which writes no record. Two exceptions: P4-T14 stages FEATURE/ only, so the mutated A is never committed; P4-T15 stages A (restored, identical to the committed fix) and FEATURE/. Consequences for the gates: every `git diff` of this plan carries the `MERGE-BASE` operand (rule G8). The working-tree form `git diff <options> MERGE-BASE -- <paths>` compares the working tree with the anchor, so it shows the committed changes of earlier tasks together with the current task's uncommitted edits; it is the form used whenever a gate covers the task's own edits (the numstat gates of P5-T5 and P5-T11, the name-only diff of P5-T10, every diff in `CMD-FOOTPRINT` and `CMD-TST-IDENTITY`). The committed form `git diff <options> MERGE-BASE HEAD -- <paths>` reads the committed state of earlier tasks (P5-T3). A `git status --porcelain` span is always taken before the task's own commit, so it lists the task's own uncommitted edits and never the work of earlier tasks, which their commits have cleared; an empty porcelain for a path therefore proves only that this task did not touch it, and P5-T9 pairs it with an anchored name-only diff. This plan still runs no `git checkout`, `git reset`, `git stash`, `git merge`, `git rebase`, `git rm` or `git update-index`; `git fetch origin main` runs once in P0-T3; the per-task push is the only other network git command; a merge of main into the branch remains `ANCHOR MOVED`. Revision 2.0 exceptions, Phase 8 only: P8-T1 runs `git fetch origin main` a second time; P8-T2 runs `git merge --no-ff origin/main` and, under its conflict branch only, `git merge --abort`; P8-T12 runs the named final `git push`; no other task of Phases 7 or 8 runs a prohibited git command, and the per-task commit and push rule continues through P7-T1 to P8-T13 with the messages `wip(959): P7-T# <summary>` and `wip(959): P8-T# <summary>` (the P8-T2 merge commit carries its own `merge(959): ...` message).
- **Anchor fixed at P0-T3.** `MERGE-BASE:` is fixed at P0-T3; a merge of main into the branch after P0-T3 invalidates every `MERGE-BASE` diff gate and is `ANCHOR MOVED`: stop and report. `MAIN-IS-ANCESTOR-EXIT:` at P0-T3 is an observation; 0 means the branch already contains origin/main, which leaves this plan valid when `CITED-TREE-EXIT: 0`. P6-T12 observes it through `ANCHOR-RECHECK:` (the same `git merge-base HEAD origin/main` command issued again without a fetch and compared with the recorded `MERGE-BASE:`; an observation, not a re-derivation, and a difference is `ANCHOR MOVED`). Revision 2.0: P7-T15 repeats that recheck after the Phase 7 edits; the Phase 8 merge of origin/main (P8-T2) is the planned anchor move at PR time, it runs only after every Phase 0 to 7 gate has been recorded, none of those gates is re-run after it, and Phase 8 anchors its own diffs at `ORIGIN-MAIN-SHA:` (PD-17).
- **Phases 7 and 8 (revision 2.0).** Phase 7 and Phase 8 tasks follow every convention above, reuse the Phase 6 payloads unchanged where a payload exists, and add exactly two payloads, `CMD-LINE-CONDITION` and `CMD-FANIN`. The fixed-name final-pass artifacts (regression-testing/pass-after-regression-tests.md, qa-gates/coverage-post-change.md, qa-gates/coverage-comparison.md, qa-gates/toolchain-final-pass.md) are rewritten in full by the Phase 7 task that re-produces them, at `ITERATION: 2` with a `SUPERSEDES:` row naming the HEAD SHA at which the previous content was last committed (an observation from `git rev-parse HEAD`, issued as `git -C WORKTREE rev-parse HEAD` before the rewrite), and again by Phase 8 at `ITERATION: 3`; the superseded content remains in git history and the schema rows stay at the top of each file. Every other Phase 7 and 8 artifact is `<task-id>-<name>.<TS>.md` under the three permitted subfolders; the Phase 7 footprint record is p7-t15-scope-boundary.<TS>.md (no Phase 7 or 8 task globs a `p6-` name). The Phase 7 and Phase 8 toolchain passes carry the Phase 6 loop rule: a `WRITESET-CHANGED-COUNT:` above 0 at the repository-wide format restarts the pass at that task with `ITERATION:` incremented (at most three iterations, a fourth being `TOOLCHAIN LOOP NOT CONVERGING`), and any other failure is a stop with the failing output. Hook containment rule: the gated command shapes in `.claude/hooks` are `git worktree remove` (enforce-epic-worktree-removal-gate.ps1, enforce-parallel-worktree-removal-gate.ps1), `gh pr merge` (enforce-epic-merge-gate.ps1), `gh pr create` and `gh pr edit` (the pr-author hook), `gh issue create` and `gh issue new` (enforce-promotion-mcp-only.ps1), and a `pwsh -Command` segment is tested by raw case-insensitive containment of those words in any arrangement (hook-command-invocation.ps1), which is what refused `CMD-TST-IDENTITY` on 2026-10-03T12-56: its `$removed1` variable supplied `remove`, its `git diff` supplied `git`, and the expanded `WORKTREE` path under `.claude/worktrees/` supplies `worktree` to every payload. The two new payloads were therefore written so that neither carries `remove`, `gh` (as in `Length`, `right` or `through`), `merge`, `create`, `edit`, `issue` or `new` anywhere in its text, and both were scanned word by word in the revision 2.0 pass; no identifier of an existing payload is renamed; `CMD-TST-IDENTITY` and `CMD-DELETE` are not used by Phases 7 or 8; every `git fetch`, `git rev-parse`, `git merge-base`, `git merge`, `git log`, `git add`, `git commit` and `git push` of Phase 8 is issued as a plain `git -C WORKTREE ...` invocation, one per call, which the hooks match structurally rather than by containment. A refusal of any Phase 7 or 8 command by a hook is `HOOK BLOCKED`: stop, record the hook text verbatim and report; the executor never renames an identifier or rewrites a payload to pass a hook.
- **Compile-red spans.** P2-T1 to P2-T5 (the TUL listing, registered by P2-T2, calls the four-parameter seam that P2-T3 lands and P2-T5 first builds green), P4-T1 to P4-T7 (the final TSC and TAS listings exist before the A seams they call) and P5-T4 to P5-T6 (the TEF listing exists before the EfcDataModel seam it overrides) form compile-red spans. Under the per-task commit rule a commit inside a span carries the suffix ` (compile-red span open)` and its artifact states `COMPILE-RED SPAN OPEN` with the last completed task and the files that do not compile (as the P4-T5, P4-T6 and P4-T7 artifacts do); the span closes at the task's first green `CMD-BUILD-TEST` (P2-T5, the revision-1.6 P4-T7, P5-T6).
- **Listings.** Every file this plan creates or rewrites wholesale is given verbatim in the Listings section as an indented block: each listing line is the file line prefixed by exactly four spaces. The executor strips exactly four leading spaces from every line, writes an empty file line for an empty listing line, and ends the file with one newline. No listing line is paraphrased, reordered or completed by judgment.
- **Edits.** Every Edit is given in the Edit Specifications section as an `OLD` block and a `NEW` block (same four-space convention). The executor passes the stripped OLD text as the Edit tool's old string and the stripped NEW text as the new string, once, in the named file. An Edit whose old text is not found exactly once is `EDIT ANCHOR NOT UNIQUE`: stop and report; never adjust the anchor by judgment.
- **Evidence paths.** Every artifact is written under FEATURE/evidence/baseline/, FEATURE/evidence/regression-testing/ or FEATURE/evidence/qa-gates/ (the three subfolders AC26 and spec D17 permit; no other evidence subfolder is used, and `CMD-EVIDENCE-FIELDS` counts any file under FEATURE/evidence/ outside them as `NONCANONICAL-SUBFOLDER-FILES:` and lists each as a `NONCANONICAL:` row). Committed test evidence is limited to the JaCoCo package projection, the one-line first-party summary and TRX-derived summaries (CLAUDE.md "Committed Test Evidence Format"); no trx, Cobertura, collector or `.coverage` document is copied into FEATURE/.
- **Artifact filenames.** Fixed names (spec Test Strategy): baseline/phase0-instructions-read.md, baseline/test-run-baseline.md, baseline/coverage-baseline.md, regression-testing/fail-before-save-case.md, regression-testing/fail-before-cleanup-files-phase-one.md, regression-testing/fail-before-write-csv.md, regression-testing/fail-before-try-save-retry.md, regression-testing/compile-red-attachment-saving-seams.md, regression-testing/fail-before-redirect-save-folder.md, regression-testing/fail-before-efc-filer-cleanup.md, regression-testing/pass-after-regression-tests.md, qa-gates/toolchain-final-pass.md, qa-gates/coverage-post-change.md, qa-gates/coverage-comparison.md, qa-gates/negative-controls.md. The dossier is regression-testing/fail-before-exception.<TS>.md (PD-10). Every other artifact is `<task-id>-<name>.<TS>.md`, where `<TS>` is the write time in `yyyy-MM-ddTHH-mm` and equals the artifact's `Timestamp:` field; a later task locates it with the glob `<task-id>-<name>.*.md`, which must match exactly one file (the highest `ITERATION:`, a file without an `ITERATION:` row counting as iteration 1, when the Phase 6 loop restarted, when a task was re-run after a stop record, as P4-T5 and P4-T7 were, or when a later task re-runs an earlier task's gate over an edited tree and writes the earlier task's artifact name with `ITERATION:` incremented, as P6-T15 does for P6-T12 (revision 1.9); the stop record stays on disk unchanged, and a stop record written under a task's pre-renumbering number, p6-t13-identity-and-sweep.2026-10-03T12-56.md for the task now numbered P6-T16, stays on disk unchanged and matches no glob of this plan).
- **Artifact fields.** Every artifact this plan writes (a command-step artifact, a fixed-name artifact, a deferral record, the dossier and the inventory artifact) carries the rows `Timestamp:`, `Command:` and `EXIT_CODE:` as line-leading labels: a label may follow list or emphasis punctuation such as `- ` or `**`, never a word character, which is exactly the form `CMD-EVIDENCE-FIELDS` tests. Every command-step artifact also carries `Output Summary:`. A prefixed payload label copied into an artifact (`FORMAT_EXIT_CODE:`, `CHECK_EXIT_CODE:`, `MSBUILD_EXIT_CODE:`, `VSTEST_EXIT_CODE:`, `COLLECT_EXIT_CODE:`, `RESTORE_EXIT_CODE:`, `FETCH-EXIT:`, `TOOL-RESTORE-EXIT:`) never substitutes for the plain `EXIT_CODE:` row, and a task's enumeration of rows, or its phrase "every printed line", is additional to the three rows, never a replacement for them. `ExpectedExitCode:` is written only where a task says so, once per artifact, equal to the observed value it explains. An artifact that records several commands names the invocation its `EXIT_CODE:` row is scoped to and records the others as named `Output Summary:` lines; where a task does not name that invocation, the row is scoped to the last invocation the task runs before it writes the artifact (for a payload that prints a `*_EXIT_CODE:` label, that label's value; otherwise the process exit code) and the artifact names it beside the row. A pass-after section appended to a fail-before artifact records its own run under `PASS-AFTER-VSTEST_EXIT_CODE:`; the artifact's `EXIT_CODE:` row stays scoped to the red run. The `Timestamp:`, `Command:`, `EXIT_CODE:` and (where a task writes it) `ExpectedExitCode:` rows are written at the top of the artifact, before `Output Summary:` and before any copied payload line, because the evidence parsers read the first occurrence of each label as the artifact's record; every prefixed label and every appended section (a pass-after section, a second-run heading, the P6-T9 section of coverage-comparison.md) follows them.
- **Command channel.** Every payload is run as `pwsh -NoProfile -Command '<payload>'`: outer single quotes, the payload's lines joined by `; `, first statement `Set-Location -LiteralPath "WORKTREE"`. No payload contains a single-quote character (`[char]39` supplies one); every string literal is double-quoted; a double quote inside one is written `[char]34` or doubled; no double-quoted literal ends with a backslash before its closing quote. No payload contains a doubled backslash in a string literal or a regular expression, because Bash de-doubles it before `pwsh` receives the command string (a `[\\/]` class arrives as `[\/]` and matches only `/`); a literal backslash is supplied by `[char]92`, a path is normalized with `.Replace([string][char]92, "/")` and matched by a forward-slash pattern, and a single backslash passes through unchanged. A .NET static file API is never given a relative path (payloads use cmdlets with `-LiteralPath`, which follow `Set-Location`). A child's exit code is read from `$LASTEXITCODE`. A `pwsh` payload is also the only route to a line count, a hash or a token census; the Grep tool is used only where a task names it.
- **Long-running payloads.** `CMD-COVERAGE-DIRECT` (P0-T11, P6-T7) and `CMD-REBUILD` may run longer than a ten-minute foreground call. The executor starts them as background invocations of the same `pwsh -NoProfile -Command '<payload>'` form and polls the captured stdout with read-only calls until the payload's final line appears. No sleep is added. A run still in progress 120 minutes after start is `RUN STALLED`: stop and report.
- **File hashes.** Every SHA-256 is the `Hash` property of `Get-FileHash -Algorithm SHA256 -LiteralPath <repository-relative path>`; the `Path` property is never recorded.
- **Tool resolution.** vswhere.exe is `Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"`; vstest.console.exe is `& $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1`; MSBuild.exe is `& $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1`. `Command:` records the CLAUDE.md-canonical `msbuild ...` form with the note `resolved through vswhere`.
- **MSBuild switches.** Every msbuild invocation carries /nodeReuse:false and a normal-verbosity file logger `/flp:LogFile=coverage\logs\TASKID.msbuild.log;Verbosity=normal`; neither changes which targets run or which diagnostics are reported. `Command:` records the canonical command plus the notes `plus /nodeReuse:false` and `plus a normal-verbosity file logger`. The analyzer and nullable gates use `/t:Rebuild` and never add a Nullable property override; the intermediate project builds of `CMD-BUILD-TEST` use `/t:Build` because they are not gates (their observation is the `0 Error(s)` line and the advanced output assembly).
- **Test runs.** Every direct run uses scripts/vscode/TaskMaster.cli.runsettings (Workers 0 at line 5, Scope ClassLevel at line 6) with `/InIsolation`, an explicit results directory under coverage\test-results\959\ and a quoted trx logger with an explicit file name. vstest.console.exe exits 0 on a zero-match filter, so every scoped run asserts its expected `total`. No test is retried, serialized or edited to pass; a failure is reported with its TRX message. The four shell-icon classes are excluded from the coverage runs on this workstation by the `EXCLUSION` text P0-T9 fixes (CI covers them); under `STALL-PROBE: CLEAR` P0-T9 records `EXCLUSION: NONE` and the substituted text is the empty string, so no class is excluded.
- **Token census.** Every occurrence count is produced by `CMD-CENSUS`, which removes ALL whitespace from each file and counts case-sensitive, non-overlapping occurrences of each token with `[regex]::Matches($content, [regex]::Escape($t)).Count`. Census tokens are written without whitespace; counts include comments and string literals; the comments in the Listings were written so that they repeat no census token. `Select-String` is never used for a count.
- **Line endings.** .gitattributes declares `* text=auto` and .editorconfig sets `end_of_line = crlf`. The Write tool may write LF; the scoped CSharpier pass of each phase normalizes endings, and every content gate is whitespace-insensitive. Numstat gates are unaffected because `text=auto` normalizes endings in `git diff`.
- **Artifact hygiene.** Before text is written into an artifact, an absolute path is replaced by `<repo-root>` (or `<user-profile>`), the account name by `<user>` and the machine name by `<host>`. The sandbox literals `C:\Sortemail959Sandbox` (new tests), `C:\Sortemail956Sandbox` (TST2) and `C:\Sortemail945Sandbox` (TST1) are not host paths and are recorded as written.
- **Sandbox literals.** No code in this plan creates any sandbox directory. Every `CMD-VSTEST` run prints `SANDBOX-EXISTS-BEFORE` and `SANDBOX-EXISTS-AFTER` for the three roots from read-only `Test-Path` calls; any `True` before a run is `SANDBOX PRESENT` and any `True` after a run is `SANDBOX CREATED BY RUN`: stop and report.
- **Expect-fail wrong-reason branch.** Every `[expect-fail]` run names the expected failing rows and a substring each failure `MESSAGE` must contain. A failure whose message lacks the substring, or a failing row outside the named set, is `FAIL-BEFORE WRONG REASON`: stop and report the messages; never adjust a test or a gate to match.
- **Stop discipline.** A named stop string in a task means: stop at that task, write the artifact with the observed values, and report the string verbatim. The executor never edits a test, a gate, a listing or a policy file to make a task pass, and never re-runs a test to obtain a different outcome. A toolchain-loop restart (Phase 6 loop rule), a re-run of a stopped task that its planner-amended task text directs, and a re-run that a task's own repair branch directs after the named repair (P6-T16, P6-T46), together with the post-edit re-runs that revision 1.9 inserted as tasks of their own (P6-T14, the scoped formatter pass, the repository-wide read-only check and the TST1 census over the edited file; P6-T15, the P6-T12 anchor recheck and footprint at `ITERATION: 2`), are the only repetitions this plan allows; a re-run of a stopped task executes the task as amended (for P4-T7, the amended listings, format and build; for P4-T9, the unchanged test and command against the amended acceptance text), and no admitted repetition is a re-run of unchanged inputs to obtain a different outcome.
- **Scope rule (issue.md line 35, binding).** Any defect found during execution in the same files or with the same root cause is reported to the orchestrator for inclusion, never listed as a follow-up; only a completely unrelated defect is reported for filing. The executor does not widen the write set on its own.

## Verified Repository Facts (re-derived in this pass with Read, Grep and Glob against WORKTREE at 94287369908cc920b21b0e3256314f988ad7d2f5)

1. A (343 lines; `#nullable enable` line 1; eighteen using lines 2 to 19; `namespace UtilitiesCS` 21; partial class 23). Fields `_responseSaveFile`, `_attachmentsOverwrite`, `_attachmentsAltName`, `_picturesOverwrite` at 25 to 28; `Cleanup_Files` 30 to 36 resets three fields and calls `RemoveReadOnlyPrompt.Reset()` at 35 (no `_attachmentsAltName` reset); `[ExcludeFromCodeCoverage]` at 38, 63, 103, 164, 227, 241, 290, 311, 322, 333 (ten); `YesNoToAll.ShowDialog(` call expressions at 112, 135, 175, 198, 255 (five; 258 is a comment); `SaveAttachment` 103 to 162; `SaveAttachmentAsync(this AttachmentHelper)` 164 to 225 with the two-argument try-save call at 221; destination overload 227 to 239 setting `FolderPathSave` at 235; `SaveCaseAsync` 241 to 288 (`when` guards at 251 to 252 and 279 to 280; try-save calls 266, 281; `default: await Task.CompletedTask;` 284 to 285); `SaveCase` 290 to 309 with the L1 labels `case (YesNoToAllResponse.NoToAll | YesNoToAllResponse.No):` at 300 and `case (YesNoToAllResponse.Yes | YesNoToAllResponse.YesToAll):` at 303; `IsPicture` 311 to 320 (the only `Path.` user in A); `SaveMessageAsMsgAsync` 322 to 331; `SaveMessageAsMSG` 333 to 340.
2. T (173 lines; same header). `RemoveReadOnlyPrompt` 28 to 30; two-argument wrapper 32 to 51 (comment 32 to 34, documentation 35 to 39, attribute 40, lambda 49); three-argument forward 53 to 73; five-argument core 75 to 160 (documentation 75 to 86, signature 87 to 93, `createDirectory` 97, `Debug.WriteLine` 103, 127, 147, prompt 108 to 113, clear 120 to 133, recursion 134 to 140, No arm 142 to 150, `else { throw; }` 151 to 154, catch closing brace 155, outer `catch (System.Exception) { throw; }` 156 to 159, method brace 160); `ClearReadOnlyAttributeOnDisk` 162 to 170 (attribute 165). `catch (` occurs at 101 (indent 12), 125 (indent 20), 156 (indent 12); `throw;` at 153 and 158.
3. U (196 lines; same header). `UndoAsync` 25 to 80 (attribute 26); `PushToUndoStack` 82 to 92 (attribute 82); `CaptureMoveDetails` 94 to 107 (attribute 94); `SanitizeArrayLineTSV` 109 to 125 (attribute 109); `StripTabsCrLf` 127 to 137 (no attribute); `WriteCSV_StartNewFileIfDoesNotExist` 139 to 170 (attribute 139; `File.Exists(Path.Combine(strFileName, strFileLocation))` 147; header assignments 151 to 163; `SanitizeArray(strAryOutput, ref strOutput);` 165; `FileIO2.WriteTextFile(strFileName, strOutput!, folderpath: strFileLocation);` 166); `SanitizeArray` 172 to 193 (attribute 172; `Debug.WriteLine` 177; `strOutput![j]` 183).
4. S (278 Read lines; 277 by `Get-Content`) and M (389 Read lines; 388 by `Get-Content`) carry the same eighteen using lines at 2 to 19; S `logger` 25 to 27; S `x.SaveAttachmentAsync(saveFsPath)` 165; M `x.SaveAttachmentAsync()` 153; M `attachment.SaveAttachment();` 299 (155 and 301 are comments); M `Sort` 247 to 248. L (241 Read lines; 240 by `Get-Content`): `MAX_PATH` 25; `SaveAttachmentsOld` 27 to 238 (attribute 27).
5. Grep over `*.cs`: `SaveAttachmentsOld|IsPicture\b` matches exactly L 28 and A 312 (declarations; zero callers). `SortItemsToExistingFolder` over `*.csproj` matches only ToDoModel.Test/ToDoModel.Test.csproj 75 and 76 (the two test files); over `*.cs` it matches TD 15 (the class), ToDoModel.Test/Email Utilities/SortItemsToExistingFolderTests.cs 14 and 62 (class name and a comment) and ToDoModel.Test/Email Utilities/SortItemsToExistingFolderTests_Unfinished.cs 12 (class name). `SaveCaseAsync(` matches A 179, 203 and 242 only. `Cleanup_Files()` executable production call: QuickFiler/Controllers/EfcDataModel.cs 309 only (QuickFiler/Legacy/QfcController.cs 792 is not compiled; QfcItemController.MailActions.cs 156 and 188 are comments; TD 391 declares an unrelated member). `SortEmail.WriteCSV_StartNewFileIfDoesNotExist(` TaskMaster/AppGlobals/AppOlObjects.cs 301 only. `attachment.SaveAttachmentAsync(Config.SaveFsPath!)` EmailFiler.cs 445 (through `SaveAttachmentsPicturesAsync` 279 to 282). None of the identifiers `SortEmail_SaveCase_Tests`, `SortEmail_AttachmentSaving_Tests`, `SortEmail_UndoAndMoveLog_Tests`, `EfcDataModelFilerCleanupTests`, `ResetFilerPromptState`, `RedirectSaveFolder`, `AllPromptSessions`, `MovedMailsHeader`, `TrySaveAttachmentCoreAsync`, `AttachmentsOverwritePrompt`, `CreateDirectoryLimit` or `TrySaveAttachmentDelegate` occurs in any `*.cs` or `*.csproj` (`TrySaveAttachmentDelegate` checked in the revision 1.6 pass over the whole worktree: zero hits in any file).
6. E (465 lines; no `#nullable`). `internal partial class EfcDataModel` 21; `MailInfo => ConversationResolver?.MailHelper` 206; `TryGetArchiveRoot` 245; five-parameter `MoveToFolderAsync` 268 to 311 (guards return false at 278, 289, 294; `var result = await InvokeFilerAsync(config, mailHelpers);` 308; `SortEmail.Cleanup_Files();` 309; `return result;` 310, the only `return result;` in E); `protected internal virtual Task<bool> InvokeFilerAsync` 320 to 326; `internal async Task OpenOlFolderAsync(string folderpath)` 328 (unique); `MAPIFolder` overload 377 to 398 calling the five-parameter overload at 387. Callers of the five-parameter overload: E 387, QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs 98, QuickFiler/Controllers/EfcFormController.Actions.cs 136 (the `MAPIFolder` overload), QuickFiler/Controllers/EfcFormController.EventHandlers.cs 184 (the `MAPIFolder` overload), QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs 316.
7. TST1 (458 lines; namespace `UtilitiesCS.Test.EmailIntelligence` 15; class 33; fifteen `[TestMethod]`; `GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments` 182 to 207; `GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments` 209 to 234; try-save tests 245 to 266 and 272 to 289 calling the three-argument overload at 257 and 281; `SanitizeArrayLineTSV` test 341 to 357; `SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows` 359 to 382 (preceded by a blank line 358); `CreateAttachmentMock` 386 to 406; `System.Action` at 45, 58, 178; no `#nullable`). TST2 (376 lines; class 20; eleven `[TestMethod]` at 33, 57, 84, 110, 140, 165, 193, 217, 245, 273, 297; constants 24 to 27 with `C:\Sortemail956Sandbox\attachments`; `SaveAsync` helper documentation 319 to 322 and body 323 to 331; `Seams` 338 to 373 with `CreateDirectory` 354 to 357 and `ClearException` 352).
8. UtilitiesCS/UtilitiesCS.csproj Compile entries 817 MovedMailInfo.cs, 818 SortEmail.cs, 819 SortEmail.AttachmentSaving.cs, 820 SortEmail.LegacyAttachmentSaving.cs, 821 SortEmail.MailItemSort.cs, 822 SortEmail.TrySaveAttachment.cs, 823 SortEmail.UndoAndMoveLog.cs, 824 FolderPredictor.cs (four-space indent, backslash separators, self-closing). UtilitiesCS.Test/UtilitiesCS.Test.csproj 98 `EmailIntelligence\SortEmail_Tests.cs`, 99 `EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs`, 100 `EmailIntelligence\FilterOlFoldersController_Tests.cs`. QuickFiler.Test/QuickFiler.Test.csproj 127 `Controllers\EfcDataModelArchiveRootTests.cs`, 128 `Controllers\EfcDataModelIssue792CarryTests.cs`.
9. InternalsVisibleTo: UtilitiesCS/Properties/AssemblyInfo.cs 18 to 20 (DynamicProxyGenAssembly2, UtilitiesCS.Test, ToDoModel.Test); QuickFiler/Properties/AssemblyInfo.cs 5 (QuickFiler.Test). UtilitiesCS/Dialogs/YesNoToAll.cs enum 14 to 21 (Empty 0, Yes 1, No 2, YesToAll 4, NoToAll 8), no `[Flags]`. UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs: `internal sealed class` 13; constructor 24; `Response { get; private set; }` 33; `Ask` 40; `ReleaseSingleAnswer` 54; `Reset` 65.
10. UtilitiesCS/OutlookObjects/Attachment/AttachmentHelper.cs: three-argument constructor `(Attachment, DateTime, string)` 34 to 37; `Init` 61 to 124 sets `FilePathSave` 101 and `FilePathSaveAlt` 106 (suffix from `GetNameSuffix()` 327 to 330); `FilePathHelperSaveAlt` internal accessor 174 to 175; `FilePathSave`/`FilePathSaveAlt` 178 to 189; `FolderPathSave` 199 to 203; `CheckParameters` 262 to 298 (fails only for a null attachment or an over-long path). UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs: constructor subscribes `FilePathHelper_PropertyChanged` 23 to 26; `FolderPath` setter 83 to 91; handler recomputes `_filePath = Path.Combine(_folderPath, _fileName)` on `FolderPath` at 360 to 362 and splits `FilePath` into folder and name at 369 to 395. UtilitiesCS/OutlookObjects/Attachment/AttachmentSerializable.cs constructor 24 to 77 reads Type, BlockLevel, Class, DisplayName, FileName, Index, PathName, Position, Size, Application, Parent, PropertyAccessor, Session (a Loose Moq attachment satisfies it, as TST1 shows). `FileIO2.WriteTextFile(string filename, string[] strOutput, string folderpath)` is the single overload (UtilitiesCS/To Depricate/FileIO2.cs 36).
11. QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs (399 lines): eleven `[TestMethod]`; `MoveAsync` 314 to 323; `CreateOlObjects` 330 to 333; `CreateGlobals` 340 to 352; `SpecialFoldersWithOneDrive` 355 to 360; `SpecialFoldersWithoutOneDrive` 363 to 366; `TestableEfcDataModel` 379 to 397 (base constructor with a null mail item, `ConversationResolver` from `QuickFiler.Helper_Classes` with a parameterless `MailItemHelper`); `ArchiveRootLiteral` 35; success path stubs `ArchiveRootPath` with `Returns(ArchiveRootLiteral)` at 179.
12. SPEC956 (docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md): line 99 contains the clause `as merge-base line 944 does, so the exception boundary is unchanged.` (once in the file); line 149 ends `and no retry bound is added (L2 is out of scope).` (once); line 155 is `- The `DirectoryInfo` construction's exception boundary (path computed before the inner `try`).` (once); seventeen lines match `^- \[(x| )\] AC` (recorded, not pinned, by P0-T2 as checked and unchecked counts).
13. scripts/vscode: Install-RepoDotNetSdk.ps1, Invoke-Restore.ps1, Invoke-MSTestWithCoverage.ps1 (parameter block line 1; `Get-DotnetCoverageArgumentList` 41 with the fixed `/TestCaseFilter:TestCategory!=LiveOutlook` 91; `ConvertTo-DerivedCoverageSettingsXml` 97; discovery 348 to 355 still excludes assemblies whose repository-relative path matches `(^|\\)\.claude\\` at 353, so the DIRECT route of #956 is required from this worktree; entry guard 459), Invoke-MSTestWithCoverage.Helpers.ps1 (`Get-CoberturaClassLineSummary` 160; `Merge-CoberturaClassesByFilename` 260; `ConvertTo-KoverageCoberturaXml` 407), Invoke-MSTestWithCoverage.Threshold.ps1 (`Assert-CoberturaLineCoverageThreshold` 3; `Assert-CoberturaBranchCoverageThreshold` 58), Invoke-MSTestWithCoverage.FirstParty.ps1 (`Get-CoberturaFirstPartyCoverageReport` 123), Invoke-MSTestWithCoverage.Projection.ps1 (`ConvertTo-JacocoPackageProjection` 14; `Assert-JacocoProjectionReconciliation` 83), Invoke-MSTest.TrxSummary.ps1 (`Get-TrxRunSummary` 12; `Format-TrxRunSummary` 103). TaskMaster.cli.runsettings is 9 lines (Workers 0 at 5, Scope ClassLevel at 6).
14. Configuration: global.json SDK 8.0.205 with `.dotnet-sdk` path; dotnet-tools.json at the repository root (csharpier 1.2.6); coverage.config present; .csharpierignore excludes `**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`, `*.csproj`, `*.props`, `*.targets`, `**/packages.config`, `**/app.config`; .gitignore carries the entries `artifacts/`, `*.trx`, `*cobertura*.xml`, `coverage/*`, `!coverage/.gitkeep`, `.dotnet*/`, `[Bb]in/`, `[Oo]bj/` and `**/[Pp]ackages/*` (cited by content; line numbers are not relied on). Analyzer wiring matches packages.config in all four projects in scope: Meziantou.Analyzer 3.0.290, Roslynator.Analyzers 5.0.0, AsyncFixer 2.1.0, Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0, SonarAnalyzer.CSharp 10.34.0.3385, MSTest.Analyzers 4.4.1 (test projects).
15. The four stall-probe classes exist: UtilitiesCS.Test/HelperClasses/ShellUtilities_Tests.cs, ShellUtilitiesStatic_Tests.cs, SysImageListHelperTests.cs, UtilitiesCS.Test/EmailIntelligence/OSBrowser_Tests.cs. The #956 run on this workstation observed `STALL-PROBE: REPRODUCES` (one failing shell-icon test) and ran the coverage route with the four classes excluded; its final coverage read `First-party coverage: lines 56202/65845 (85.36%), branches 13618/17076 (79.75%)` and the SortEmail family had four uncovered lines: T 49 (lambda), T 154 (else brace), T 155 (catch brace) and M 153 (`await attachments.ForEachAsync(async x => await x.SaveAttachmentAsync());`). These are prediction anchors, not gates.
16. artifacts/orchestration/orchestrator-state.json (git-ignored): `route_id` preparation (3), `issue-num` 959 (10), `work-mode` full-bug (14), `feature-folder` (15), `plan-path` (16), `lifecycle_ready` true (17).
17. Interop embedding (revision 1.6 pass; PD-14): UtilitiesCS/UtilitiesCS.csproj lines 222 to 224 reference Microsoft.Office.Interop.Outlook with `<EmbedInteropTypes>True</EmbedInteropTypes>` at 223; UtilitiesCS.Test/UtilitiesCS.Test.csproj lines 748 to 750 reference the same assembly with `<EmbedInteropTypes>False</EmbedInteropTypes>` at 749. Grep over UtilitiesCS `*.cs` for `(Func|Action)<` with an Outlook interop type argument: only A lines 187 and 245 (the P4-T4 seam parameters, after the first P4-T7 format). Grep over UtilitiesCS `*.cs` for `delegate `: the declarations are UtilitiesCS/Dialogs/YesNoToAll.cs 25 (`YesNoToAllDelegate`), UtilitiesCS/Dialogs/NotImplementedDialog.cs 15 (`ResponseDelegate`), UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.Transform.cs 23 (`FolderGroupTransformer<T>`), UtilitiesCS/Interfaces/IReusableTypeClasses/ISCODictionary.cs 31 (`AltLoader`), UtilitiesCS/Interfaces/IReusableTypeClasses/ISerializableList.cs 9 (`CSVLoader<T>`), UtilitiesCS/Threading/StoreLockupResponder.cs 20 (`StoreLockupNotifier`), UtilitiesCS/Threading/ApplicationIdleTimer.cs 83 (`ApplicationIdleEventHandler`) and UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection.Serialization.cs 79 (`AltListLoader`); none has an Outlook interop parameter. Grep over UtilitiesCS.Test `*.cs` for the same generic forms: SortEmail_SaveCase_Tests.cs 310 and SortEmail_AttachmentSaving_Tests.cs 404 (the P4-T1 and P4-T2 `RecordingSave` return types) and ClassifierGroups/MulticlassEngine_Tests.cs 148 (`Func<MailItemHelper, Task>`, a UtilitiesCS type).
18. Revision 1.9 pass (WORKTREE at HEAD 9de3f176d, the orchestrator's statement; re-derived with Read, Grep and Glob): TST1 lines 170 to 173 are the summary element of `Cleanup_Files_DoesNotThrow` (`[TestMethod]` 174, method 175 to 180, `SortEmail.Cleanup_Files()` 178); line 171 is the only line of TST1 carrying `tracking fields`, and `AllPromptSessions` and `prompt session` occur nowhere in TST1; the `#region` header at 127 names `StripTabsCrLf and Cleanup_Files`; the nearest P4-T6 edit begins at 182. A at HEAD: the three sessions 18 to 26 with the comment 15 to 17 (`Cleanup_Files resets them after each filing operation`), the `AllPromptSessions` comment 28 to 30 and property 31 to 38 with four elements, `Cleanup_Files` 40 to 46 (`foreach (var prompt in AllPromptSessions) { prompt.Reset(); }`), and no field of type `YesNoToAllResponse` (its remaining uses are the parameter and `case` labels at 254 to 303). T at HEAD: `RemoveReadOnlyPrompt` 15 to 17 with the comment 12 to 14. Over the six UtilitiesCS.Test/EmailIntelligence/SortEmail_*.cs files, `tracking fields`, `static YesNoToAllResponse`, `_attachmentsAltName` and `_responseSaveFile` match only TST1 171. FEATURE/evidence/qa-gates holds p6-t10-post-format-census.2026-10-03T12-52.md (`ITERATION: 1`; the thirteen TOKENS-TST1 totals at its lines 175 to 187 equal the FINAL column; `LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 488` at 188 and 385), p6-t12-scope-boundary.2026-10-03T12-55.md (`ITERATION: 1`, `ANCHOR-RECHECK:` equal to the anchor, `OUTSIDE-WRITE-SET: 0`, `WRITE-SET-MISSING: 0`, seven numstat rows none of which names TST1) and p6-t13-identity-and-sweep.2026-10-03T12-56.md (the HOOK BLOCK stop record of the task now numbered P6-T16: `CMD-TST-IDENTITY` not executed because enforce-epic-worktree-removal-gate.ps1 refused the payload, `CMD-SWEEP` all-zero with `FILES: 80`, `CMD-EVIDENCE-FIELDS` not issued; `Timestamp:`, `Command:` and `EXIT_CODE:` line-leading; no host token).
19. Revision 2.0 pass (WORKTREE at HEAD eb7871502, the orchestrator's statement, confirmed by the worktree's branch ref file and by reflog line 106, `commit: docs(959): initial feature review artifacts`; the reflog carries no merge entry; re-derived with Read, Grep and Glob, no shell). A lines 111 to 156 at HEAD: the excluded wrapper 113 to 122, the synchronous core 130 to 156, line 143 `var overwritePrompt = attachmentHelper.AttachmentInfo.IsImage` with the arms `? picturesOverwritePrompt` at 144 and `: attachmentsOverwritePrompt;` at 145, `Ask` 146 to 148, `SaveCase` 149 to 154, `ReleaseSingleAnswer` 155. coverage\final-959.cobertura.xml (git-ignored, on disk; the P6-T7 document after CMD-COVERAGE-POST): exactly one `<class>` node carries `filename="UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs"` (document line 59687, `branch-rate="0.979592"`); its `SaveAttachment` method node reads `branch-rate="0.75"` (59791) with `<line number="143" hits="1" branch="True" condition-coverage="50% (1/2)">` (59802); the class-level `<lines>` block from 60001 repeats the line with the same attributes; every other `branch="True"` line of the class reads `100%`. TAS at HEAD: 437 lines by Grep `^`, eleven `[TestMethod]`, SS3's last assertion `attachments.Session.Response.Should().Be(YesNoToAllResponse.NoToAll);` at 296 (the only line of the file carrying `Be(YesNoToAllResponse.NoToAll)`), its closing brace 297, blank 298, the RR summary opening at 299 and `/// RR. Scenario: a helper built under an origin folder is redirected to a destination` at 300 (once); helpers `CreateAttachmentMock` 357, `CreateHelper` 372, `OverwritePrompt` 377, `AltNamePrompt` 382, `Exists` 391, `RecordingSave` 404, `ScriptedPrompt` 417 to 435; `SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly` 0 hits in the worktree; AS2 (68 to 92) builds the helper from `photo.jpg` and scripts the pictures session `Yes`. TSC at HEAD: 343 lines by Grep `^`; `using System;` exactly once, at line 1, followed by `using System.Collections.Generic;` at 2; the Grep of PD-16's eighteen `System`-namespace alternatives returns zero lines. QuickFiler.Test/QuickFiler.Test.csproj at HEAD: `Controllers\EfcDataModelArchiveRootTests.cs` 127, `Controllers\EfcDataModelFilerCleanupTests.cs` 128, `Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs` 204, `Controllers\QfcItemController.InitializationTests.cs` 205, `TestSupport\DedicatedWorkerThread.cs` 229, `TestSupport\WinFormsPumpHostTests.cs` 230; none of `UiThreadDispatcherPinCountTests`, `SynchronousBackgroundWorker` or `ArmingFakeTimeProvider` occurs in it. The sibling worktree agent-a291a7fbabf9d0229 (the only worktree under the main checkout's `.claude/worktrees/` holding QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs, by Glob) carries `Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs` at its line 204 between the FixtureTests entry (203) and the InitializationTests entry (205), and `TestSupport\SynchronousBackgroundWorker.cs`, `TestSupport\ArmingFakeTimeProvider.cs` at 230 to 231 between `TestSupport\DedicatedWorkerThread.cs` (229) and `TestSupport\WinFormsPumpHostTests.cs` (232), and no `EfcDataModelFilerCleanupTests` entry; this is the main-side shape PD-17 expects P8-T1 to re-derive. The main checkout's packed-refs read `refs/remotes/origin/main` f8ea1b5dcc6514bc0088bc80965c188bfd717557 at the time of the pass (an observation only; P8-T1 fetches and records the value it sees). Hooks under `.claude/hooks`: the gated shapes are `git worktree remove` (enforce-epic-worktree-removal-gate.ps1 lines 171, 172 and 380; enforce-parallel-worktree-removal-gate.ps1 129, 130 and 282), `gh pr merge` (enforce-epic-merge-gate.ps1 178, 184, 386, 387), `gh pr create` and `gh pr edit` (enforce-pr-author-skill-helpers.ps1 279, 280), `gh issue create` and `gh issue new` (enforce-promotion-mcp-only.ps1 126, 127) and the validate-bash shapes `push` (123) and `reset` (127); the basis of the Phases 7 and 8 hook containment rule. Evidence on disk: coverage-post-change.md (12-46, ITERATION 1; 7393 of 7393; `FIRST-PARTY-LINE-PERCENT: 85.39`, `FIRST-PARTY-BRANCH-PERCENT: 79.81`; `FINAL-UCS-BRANCH: 9495/11356`), toolchain-final-pass.md (12-54, ITERATION 1, LOOP-RESTARTS 0), coverage-comparison.md (12-48, ITERATION 1; `BASELINE-HASH 408A19E922396ADCFF033FE8156DCFE3C818FA8A5484469691071CC2176E142D`; `SORTEMAIL-CLASS final ...SortEmail.AttachmentSaving.cs valid=142 covered=142 uncovered=0`; `MEMBER SaveAttachmentCore span=130-156 valid=19 covered=19 percent=100`), pass-after-regression-tests.md (12-38, ITERATION 1; `RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57`; `COUNTERS total=55`), p6-t12-scope-boundary.2026-10-06T13-23.md (ITERATION 2; `SUBTRACTED-CLAUSE-A: 27`, `SUBTRACTED-CLAUSE-B: 26`; the seven numstat rows), p6-t16-identity-and-sweep.2026-10-06T15-11.md (`FILES: 84`; `EVIDENCE-FILES-CHECKED: 78`; `EVIDENCE-INHERITED-SKIPPED: 1`), p6-t46-ac-inventory.2026-10-06T15-23.md (`AC-CHECKED 25`, 89 files at its sweep), p0-t3-worktree-context.2026-10-03T08-24.md (the 27 `INHERITED-CLAUSE-A` paths at its lines 18 to 44; `ORIGIN-MAIN-SHA: 5d87e5b8e` then, an observation). The three review artifacts of 2026-10-06T15-30 sit directly under FEATURE/, not under FEATURE/evidence/, so `CMD-EVIDENCE-FIELDS` does not enumerate them and `CMD-SWEEP` counts them in `FILES:` (92 files under FEATURE/ at HEAD: the 89 of the P6-T46 sweep plus the three reviews; the P6-T46 artifact itself was written after its sweep, so the current count before any Phase 7 write is 93).

## Test Totals (PD-4; every value is a `COUNTERS total=` expectation)

| Filter | Baseline | After Phase 1 | After Phase 2 | After Phase 3 | Phase 4 onward |
| --- | --- | --- | --- | --- | --- |
| `FILTER-TST1` (SortEmail_Tests) | 15 | 15 | 14 | 14 | 18 |
| `FILTER-TRYSAVE` | 11 | 11 | 11 | 12 | 12 |
| `FILTER-SAVECASE` | 0 | 5 | 5 | 5 | 12 |
| `FILTER-ATTSAVE` | 0 | 4 | 4 | 4 | 11 |
| `FILTER-UNDO` | 0 | 0 | 2 | 2 | 2 |
| `FILTER-SORTEMAIL` (all five classes) | 26 | 35 | 36 | 37 | 55 |
| `FILTER-EFC-ARCHIVE` | 11 | 11 | 11 | 11 | 11 |
| `FILTER-EFC-CLEANUP` | 0 | 0 | 0 | 0 | 3 (from P5-T6) |

Revision 2.0 (Phase 7 onward): `FILTER-ATTSAVE` 12 and `FILTER-SORTEMAIL` 56 once E-TAS-SS4 is applied (P7-T2); every other filter keeps its Phase 4 onward value; the full coverage run under the P0-T9 exclusion totals 7394 (7393 at P6-T7 plus SS4) in Phase 7; in Phase 8 the full-run total is recorded, not predicted, because the merged tree carries main's tests, while the three scoped totals stay 56, 3 and 11 unless P8-T1's `MAIN-CHANGED-PATHS:` names a file of those classes. `NAMES-TAS-FINAL2` (12) is `NAMES-TAS-FINAL` plus `SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly`.

Test name sets. `NAMES-TST1-BASE` is the fifteen method names of TST1 (`InitializeSortToExisting_AlwaysThrows_NotImplementedException`, `InitializeSortToExisting_WithExplicitArgs_StillThrows_NotImplementedException`, `SortAsync_MailHelpers_WhenNull_ThrowsArgumentNullException`, `SortAsync_MailHelpers_WhenEmpty_ThrowsArgumentNullException`, `StripTabsCrLf_WithControlCharacters_ReturnsCleanedSingleSpacedString`, `StripTabsCrLf_WithPlainText_ReturnsOriginalString`, `Cleanup_Files_DoesNotThrow`, `GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments`, `GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments`, `TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile`, `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave`, `SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath`, `SaveMessageAsMSG_WhenSubjectNeedsSanitizing_UsesMsgSavePath`, `SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine`, `SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows`). `NAMES-TST1-FINAL` is that set minus `SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows`, with the two `GetAttachmentsInfo` names each replaced by their three `DisplayName` rows (Listing L-TST1-ROWS). `NAMES-T` is the eleven TST2 method names (T1 to T11); `NAMES-T12` adds `TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear`. `NAMES-TSC-P1`, `NAMES-TSC-FINAL`, `NAMES-TAS-P1`, `NAMES-TAS-FINAL`, `NAMES-TUL` and `NAMES-TEF` are the `DisplayName` values and method names of the corresponding Listings, enumerated in the Test Inventory section.

## Listings

### Listing L-TSC-P1 (UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs, Phase 1 state, written by P1-T1)

    using FluentAssertions;
    using Microsoft.Office.Interop.Outlook;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    namespace UtilitiesCS.Test.EmailIntelligence
    {
        /// <summary>
        /// Unit tests for the synchronous SaveCase switch of <see cref="SortEmail"/> (issue #959,
        /// defect L1). Each test passes a Moq attachment and two rooted literal paths; the mocked
        /// SaveAsFile records the call, so no file is created and no dialog is shown.
        /// </summary>
        [TestClass]
        public class SortEmail_SaveCase_Tests
        {
            // Rooted literal paths used only as in-memory values; SaveAsFile is a mock.
            private const string RequestedPath = @"C:\Sortemail959Sandbox\attachments\report.pdf";
            private const string AlternatePath = @"C:\Sortemail959Sandbox\attachments\report_alt.pdf";

            /// <summary>
            /// L1-A. Scenario: the overwrite answer is No or NoToAll. Expected: one save to the
            /// alternate path and none to the requested path.
            /// </summary>
            [DataTestMethod]
            [DataRow(
                YesNoToAllResponse.No,
                DisplayName = "SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No]"
            )]
            [DataRow(
                YesNoToAllResponse.NoToAll,
                DisplayName = "SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [NoToAll]"
            )]
            public void SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath(YesNoToAllResponse answer)
            {
                // Arrange
                var attachment = new Mock<Attachment>(MockBehavior.Loose);

                // Act
                SortEmail.SaveCase(answer, attachment.Object, RequestedPath, AlternatePath);

                // Assert
                attachment.Verify(x => x.SaveAsFile(AlternatePath), Times.Once);
                attachment.Verify(x => x.SaveAsFile(RequestedPath), Times.Never);
            }

            /// <summary>
            /// L1-B. Scenario: the overwrite answer is Yes or YesToAll. Expected: one save to the
            /// requested path and none to the alternate path.
            /// </summary>
            [DataTestMethod]
            [DataRow(
                YesNoToAllResponse.Yes,
                DisplayName = "SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes]"
            )]
            [DataRow(
                YesNoToAllResponse.YesToAll,
                DisplayName = "SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [YesToAll]"
            )]
            public void SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath(YesNoToAllResponse answer)
            {
                // Arrange
                var attachment = new Mock<Attachment>(MockBehavior.Loose);

                // Act
                SortEmail.SaveCase(answer, attachment.Object, RequestedPath, AlternatePath);

                // Assert
                attachment.Verify(x => x.SaveAsFile(RequestedPath), Times.Once);
                attachment.Verify(x => x.SaveAsFile(AlternatePath), Times.Never);
            }

            /// <summary>
            /// L1-C (control). Scenario: the answer is Empty (a cancelled prompt). Expected: no save
            /// on either path.
            /// </summary>
            [TestMethod]
            public void SaveCase_WhenAnswerIsEmpty_DoesNotSave()
            {
                // Arrange
                var attachment = new Mock<Attachment>(MockBehavior.Loose);

                // Act
                SortEmail.SaveCase(
                    YesNoToAllResponse.Empty,
                    attachment.Object,
                    RequestedPath,
                    AlternatePath
                );

                // Assert
                attachment.Verify(x => x.SaveAsFile(It.IsAny<string>()), Times.Never);
            }
        }
    }

### Listing L-TAS-P1 (UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs, Phase 1 state, written by P1-T2)

    using System.Reflection;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace UtilitiesCS.Test.EmailIntelligence
    {
        /// <summary>
        /// Unit tests for the attachment-saving partial of <see cref="SortEmail"/> (issue #959).
        /// This phase-one file carries the L3 regression test against the four static enum answer
        /// fields; a later phase of the same item replaces it when the fields become prompt
        /// sessions.
        /// </summary>
        [TestClass]
        public class SortEmail_AttachmentSaving_Tests
        {
            /// <summary>
            /// L3 (phase one). Scenario: a sticky answer is held in one of the four static answer
            /// fields and Cleanup_Files runs. Expected: the field reads Empty afterwards. The
            /// reflective static write is order-independent: the only concurrent writer in a test
            /// run writes the value this assertion expects (UT5 call-out in the feature spec).
            /// </summary>
            [DataTestMethod]
            [DataRow(
                "_responseSaveFile",
                DisplayName = "Cleanup_Files_ResetsEveryPromptAnswerField [_responseSaveFile]"
            )]
            [DataRow(
                "_attachmentsOverwrite",
                DisplayName = "Cleanup_Files_ResetsEveryPromptAnswerField [_attachmentsOverwrite]"
            )]
            [DataRow(
                "_attachmentsAltName",
                DisplayName = "Cleanup_Files_ResetsEveryPromptAnswerField [_attachmentsAltName]"
            )]
            [DataRow(
                "_picturesOverwrite",
                DisplayName = "Cleanup_Files_ResetsEveryPromptAnswerField [_picturesOverwrite]"
            )]
            public void Cleanup_Files_ResetsEveryPromptAnswerField(string fieldName)
            {
                // Arrange
                var field = typeof(SortEmail).GetField(
                    fieldName,
                    BindingFlags.NonPublic | BindingFlags.Static
                );
                field.Should().NotBeNull();
                field.SetValue(null, YesNoToAllResponse.YesToAll);

                // Act
                SortEmail.Cleanup_Files();

                // Assert
                field.GetValue(null).Should().Be(YesNoToAllResponse.Empty);
            }
        }
    }

### Listing L-TUL (UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs, written by P2-T1)

    using System;
    using System.Collections.Generic;
    using System.IO;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace UtilitiesCS.Test.EmailIntelligence
    {
        /// <summary>
        /// Unit tests for the seeded moved-mails header of <see cref="SortEmail"/> (issue #959,
        /// defect L4 and the header shape). Both tests pass recording delegates for the file-existence
        /// check and the text write, so the rooted literal paths are never touched on disk.
        /// </summary>
        [TestClass]
        public class SortEmail_UndoAndMoveLog_Tests
        {
            private const string LogFolder = @"C:\Sortemail959Sandbox\logs";
            private const string LogFileName = "MovedMails.txt";
            private const string ExpectedHeader =
                "Triage\tFolderName\tSent_On\tFrom\tTo\tCC\tSubject\tBody\tfromDomain\tConversation_ID\tEntryID\tAttachments\tFlaggedAsTask";

            /// <summary>
            /// L4-T1. Scenario: the log file does not exist. Expected: the existence check receives
            /// the folder-then-file-name combination and exactly one write of a single tab-separated
            /// header line with thirteen columns goes to that file name and folder.
            /// </summary>
            [TestMethod]
            public void WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader()
            {
                // Arrange
                var queried = new List<string>();
                var writes = new List<(string FileName, string[] Lines, string Folder)>();
                Func<string, bool> fileExists = path =>
                {
                    queried.Add(path);
                    return false;
                };
                Action<string, string[], string> writeTextFile = (fileName, lines, folder) =>
                    writes.Add((fileName, lines, folder));

                // Act
                SortEmail.WriteCSV_StartNewFileIfDoesNotExist(
                    LogFileName,
                    LogFolder,
                    fileExists,
                    writeTextFile
                );

                // Assert
                queried.Should().Equal(Path.Combine(LogFolder, LogFileName));
                writes.Should().ContainSingle();
                writes[0].FileName.Should().Be(LogFileName);
                writes[0].Folder.Should().Be(LogFolder);
                writes[0].Lines.Should().ContainSingle();
                writes[0].Lines[0].Should().Be(ExpectedHeader);
                writes[0].Lines[0].Split('\t').Should().HaveCount(13);
            }

            /// <summary>
            /// L4-T2. Scenario: the log file already exists. Expected: nothing is written.
            /// </summary>
            [TestMethod]
            public void WriteCSV_WhenFileExists_DoesNotWrite()
            {
                // Arrange
                var writes = 0;
                Func<string, bool> fileExists = _ => true;
                Action<string, string[], string> writeTextFile = (_, _, _) => writes++;

                // Act
                SortEmail.WriteCSV_StartNewFileIfDoesNotExist(
                    LogFileName,
                    LogFolder,
                    fileExists,
                    writeTextFile
                );

                // Assert
                writes.Should().Be(0);
            }
        }
    }

### Listing L-TSC-FINAL (UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs, final state, revision 1.6 form whose RecordingSave returns SortEmail.TrySaveAttachmentDelegate (PD-14); P4-T1 wrote the revision-1.5 form and P4-T7 rewrites the file from this listing)

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Office.Interop.Outlook;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    namespace UtilitiesCS.Test.EmailIntelligence
    {
        /// <summary>
        /// Unit tests for the SaveCase switch and the seamed SaveCaseAsync core of
        /// <see cref="SortEmail"/> (issue #959: defect L1 and the alternate-name prompt session).
        /// Each test passes a Moq attachment, rooted literal paths, its own scripted prompt session
        /// and a recording try-save delegate; no file is created, no dialog is shown and no
        /// production session is read or written.
        /// </summary>
        [TestClass]
        public class SortEmail_SaveCase_Tests
        {
            // Rooted literal paths used only as in-memory values; SaveAsFile is a mock.
            private const string RequestedPath = @"C:\Sortemail959Sandbox\attachments\report.pdf";
            private const string AlternatePath = @"C:\Sortemail959Sandbox\attachments\report_alt.pdf";
            private const string AltNamePrompt =
                "The file " + RequestedPath + " already exists. Save with an alternate name?";

            /// <summary>
            /// L1-A. Scenario: the overwrite answer is No or NoToAll. Expected: one save to the
            /// alternate path and none to the requested path.
            /// </summary>
            [DataTestMethod]
            [DataRow(
                YesNoToAllResponse.No,
                DisplayName = "SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No]"
            )]
            [DataRow(
                YesNoToAllResponse.NoToAll,
                DisplayName = "SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [NoToAll]"
            )]
            public void SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath(YesNoToAllResponse answer)
            {
                // Arrange
                var attachment = new Mock<Attachment>(MockBehavior.Loose);

                // Act
                SortEmail.SaveCase(answer, attachment.Object, RequestedPath, AlternatePath);

                // Assert
                attachment.Verify(x => x.SaveAsFile(AlternatePath), Times.Once);
                attachment.Verify(x => x.SaveAsFile(RequestedPath), Times.Never);
            }

            /// <summary>
            /// L1-B. Scenario: the overwrite answer is Yes or YesToAll. Expected: one save to the
            /// requested path and none to the alternate path.
            /// </summary>
            [DataTestMethod]
            [DataRow(
                YesNoToAllResponse.Yes,
                DisplayName = "SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes]"
            )]
            [DataRow(
                YesNoToAllResponse.YesToAll,
                DisplayName = "SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [YesToAll]"
            )]
            public void SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath(YesNoToAllResponse answer)
            {
                // Arrange
                var attachment = new Mock<Attachment>(MockBehavior.Loose);

                // Act
                SortEmail.SaveCase(answer, attachment.Object, RequestedPath, AlternatePath);

                // Assert
                attachment.Verify(x => x.SaveAsFile(RequestedPath), Times.Once);
                attachment.Verify(x => x.SaveAsFile(AlternatePath), Times.Never);
            }

            /// <summary>
            /// L1-C (control). Scenario: the answer is Empty (a cancelled prompt). Expected: no save
            /// on either path.
            /// </summary>
            [TestMethod]
            public void SaveCase_WhenAnswerIsEmpty_DoesNotSave()
            {
                // Arrange
                var attachment = new Mock<Attachment>(MockBehavior.Loose);

                // Act
                SortEmail.SaveCase(
                    YesNoToAllResponse.Empty,
                    attachment.Object,
                    RequestedPath,
                    AlternatePath
                );

                // Assert
                attachment.Verify(x => x.SaveAsFile(It.IsAny<string>()), Times.Never);
            }

            /// <summary>
            /// SC1. Scenario: the overwrite answer is Yes or YesToAll. Expected: one try-save to the
            /// requested path; the alternate-name session is never asked.
            /// </summary>
            [DataTestMethod]
            [DataRow(
                YesNoToAllResponse.Yes,
                DisplayName = "SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [Yes]"
            )]
            [DataRow(
                YesNoToAllResponse.YesToAll,
                DisplayName = "SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [YesToAll]"
            )]
            public async Task SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt(
                YesNoToAllResponse answer
            )
            {
                // Arrange
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                var altName = new ScriptedPrompt();
                var saves = new List<string>();

                // Act
                await SortEmail.SaveCaseAsync(
                    answer,
                    attachment.Object,
                    RequestedPath,
                    AlternatePath,
                    altName.Session,
                    RecordingSave(saves)
                );

                // Assert
                saves.Should().Equal(RequestedPath);
                altName.Messages.Should().BeEmpty();
                altName.Session.Response.Should().Be(YesNoToAllResponse.Empty);
            }

            /// <summary>
            /// SC2. Scenario: the overwrite answer is No and the alternate-name answer is Yes.
            /// Expected: the alternate-name prompt is shown once with the requested path, one
            /// try-save goes to the alternate path, and the single Yes answer is released.
            /// </summary>
            [TestMethod]
            public async Task SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePathAndReleasesAnswer()
            {
                // Arrange
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                var altName = new ScriptedPrompt(YesNoToAllResponse.Yes);
                var saves = new List<string>();

                // Act
                await SortEmail.SaveCaseAsync(
                    YesNoToAllResponse.No,
                    attachment.Object,
                    RequestedPath,
                    AlternatePath,
                    altName.Session,
                    RecordingSave(saves)
                );

                // Assert
                altName.Messages.Should().Equal(AltNamePrompt);
                saves.Should().Equal(AlternatePath);
                altName.Session.Response.Should().Be(YesNoToAllResponse.Empty);
            }

            /// <summary>
            /// SC3. Scenario: the overwrite answer is NoToAll and the alternate-name answer is
            /// YesToAll, over two calls. Expected: one prompt, two alternate-path saves, the
            /// YesToAll answer kept.
            /// </summary>
            [TestMethod]
            public async Task SaveCaseAsync_WhenAnswerIsNoToAllAndAltNameAnswerIsYesToAll_KeepsAnswerAcrossCalls()
            {
                // Arrange
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                var altName = new ScriptedPrompt(YesNoToAllResponse.YesToAll);
                var saves = new List<string>();

                // Act
                await SortEmail.SaveCaseAsync(
                    YesNoToAllResponse.NoToAll,
                    attachment.Object,
                    RequestedPath,
                    AlternatePath,
                    altName.Session,
                    RecordingSave(saves)
                );
                await SortEmail.SaveCaseAsync(
                    YesNoToAllResponse.NoToAll,
                    attachment.Object,
                    RequestedPath,
                    AlternatePath,
                    altName.Session,
                    RecordingSave(saves)
                );

                // Assert
                altName.Messages.Should().ContainSingle();
                saves.Should().Equal(AlternatePath, AlternatePath);
                altName.Session.Response.Should().Be(YesNoToAllResponse.YesToAll);
            }

            /// <summary>
            /// SC4. Scenario: the overwrite answer is No and the alternate-name answer is NoToAll,
            /// over two calls. Expected: one prompt, no save, the NoToAll answer kept.
            /// </summary>
            [TestMethod]
            public async Task SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsNoToAll_DoesNotSaveAndKeepsAnswer()
            {
                // Arrange
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                var altName = new ScriptedPrompt(YesNoToAllResponse.NoToAll);
                var saves = new List<string>();

                // Act
                await SortEmail.SaveCaseAsync(
                    YesNoToAllResponse.No,
                    attachment.Object,
                    RequestedPath,
                    AlternatePath,
                    altName.Session,
                    RecordingSave(saves)
                );
                await SortEmail.SaveCaseAsync(
                    YesNoToAllResponse.No,
                    attachment.Object,
                    RequestedPath,
                    AlternatePath,
                    altName.Session,
                    RecordingSave(saves)
                );

                // Assert
                altName.Messages.Should().ContainSingle();
                saves.Should().BeEmpty();
                altName.Session.Response.Should().Be(YesNoToAllResponse.NoToAll);
            }

            /// <summary>
            /// SC5. Scenario: the overwrite answer is No and the alternate-name prompt is cancelled
            /// (Empty), over two calls. Expected: no save, the session holds no answer, and the
            /// second call prompts again.
            /// </summary>
            [TestMethod]
            public async Task SaveCaseAsync_WhenAnswerIsNoAndAltNamePromptIsCancelled_DoesNotSaveAndStaysAskable()
            {
                // Arrange
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                var altName = new ScriptedPrompt(YesNoToAllResponse.Empty, YesNoToAllResponse.Empty);
                var saves = new List<string>();

                // Act
                await SortEmail.SaveCaseAsync(
                    YesNoToAllResponse.No,
                    attachment.Object,
                    RequestedPath,
                    AlternatePath,
                    altName.Session,
                    RecordingSave(saves)
                );
                await SortEmail.SaveCaseAsync(
                    YesNoToAllResponse.No,
                    attachment.Object,
                    RequestedPath,
                    AlternatePath,
                    altName.Session,
                    RecordingSave(saves)
                );

                // Assert
                altName.Messages.Should().Equal(AltNamePrompt, AltNamePrompt);
                saves.Should().BeEmpty();
                altName.Session.Response.Should().Be(YesNoToAllResponse.Empty);
            }

            /// <summary>
            /// SC6. Scenario: the overwrite answer is Empty. Expected: no prompt and no save.
            /// </summary>
            [TestMethod]
            public async Task SaveCaseAsync_WhenAnswerIsEmpty_DoesNothing()
            {
                // Arrange
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                var altName = new ScriptedPrompt();
                var saves = new List<string>();

                // Act
                await SortEmail.SaveCaseAsync(
                    YesNoToAllResponse.Empty,
                    attachment.Object,
                    RequestedPath,
                    AlternatePath,
                    altName.Session,
                    RecordingSave(saves)
                );

                // Assert
                altName.Messages.Should().BeEmpty();
                saves.Should().BeEmpty();
            }

            /// <summary>
            /// Returns a try-save delegate that records the requested path and reports success
            /// without touching any file.
            /// </summary>
            private static SortEmail.TrySaveAttachmentDelegate RecordingSave(List<string> saves)
            {
                return (attachment, path) =>
                {
                    saves.Add(path);
                    return Task.FromResult(true);
                };
            }

            /// <summary>
            /// Owns one prompt session whose answers are scripted; an unscripted prompt throws from
            /// the empty queue, which is how a test proves that a session was not asked.
            /// </summary>
            private sealed class ScriptedPrompt
            {
                private readonly Queue<YesNoToAllResponse> _answers;

                public ScriptedPrompt(params YesNoToAllResponse[] answers)
                {
                    _answers = new Queue<YesNoToAllResponse>(answers);
                    Session = new YesNoToAllPromptSession(Prompt);
                }

                public YesNoToAllPromptSession Session { get; }
                public List<string> Messages { get; } = new List<string>();

                private YesNoToAllResponse Prompt(string message)
                {
                    Messages.Add(message);
                    return _answers.Dequeue();
                }
            }
        }
    }

### Listing L-TAS-FINAL (UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs, final state, revision 1.6 form whose RecordingSave returns SortEmail.TrySaveAttachmentDelegate (PD-14); P4-T2 wrote the revision-1.5 form and P4-T7 rewrites the file from this listing)

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Office.Interop.Outlook;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using UtilitiesCS.EmailIntelligence;

    namespace UtilitiesCS.Test.EmailIntelligence
    {
        /// <summary>
        /// Unit tests for the seamed attachment-saving cores of <see cref="SortEmail"/> (issue #959:
        /// the prompt sessions, the re-rooting defect and the cleanup invariant). Every test owns its
        /// prompt sessions, its recording delegates and its mocks; the rooted literal paths are
        /// never touched on disk, no dialog is shown and no production session is read or written.
        /// </summary>
        [TestClass]
        public class SortEmail_AttachmentSaving_Tests
        {
            private const string SandboxFolder = @"C:\Sortemail959Sandbox\attachments";
            private const string OriginFolder = @"C:\Sortemail959Sandbox\origin";
            private const string DestinationFolder = @"C:\Sortemail959Sandbox\destination";
            private static readonly DateTime SentOn = new DateTime(2026, 4, 3, 9, 30, 0);

            /// <summary>
            /// AS1. Scenario: the destination file does not exist. Expected: the existence check is
            /// asked for the primary path, one try-save goes to the primary path, no session is asked.
            /// </summary>
            [TestMethod]
            public async Task SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting()
            {
                // Arrange
                var helper = CreateHelper(CreateAttachmentMock("photo.jpg"), SandboxFolder);
                var queried = new List<string>();
                var saves = new List<string>();
                var pictures = new ScriptedPrompt();
                var attachments = new ScriptedPrompt();
                var altName = new ScriptedPrompt();

                // Act
                await SortEmail.SaveAttachmentAsync(
                    helper,
                    Exists(false, queried),
                    pictures.Session,
                    attachments.Session,
                    altName.Session,
                    RecordingSave(saves)
                );

                // Assert
                queried.Should().Equal(helper.FilePathSave);
                saves.Should().Equal(helper.FilePathSave);
                pictures.Messages.Should().BeEmpty();
                attachments.Messages.Should().BeEmpty();
                altName.Messages.Should().BeEmpty();
            }

            /// <summary>
            /// AS2. Scenario: the file exists and the attachment is an image. Expected: only the
            /// pictures session is asked, with the overwrite text, and the Yes answer saves to the
            /// primary path.
            /// </summary>
            [TestMethod]
            public async Task SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly()
            {
                // Arrange
                var helper = CreateHelper(CreateAttachmentMock("photo.jpg"), SandboxFolder);
                var saves = new List<string>();
                var pictures = new ScriptedPrompt(YesNoToAllResponse.Yes);
                var attachments = new ScriptedPrompt();
                var altName = new ScriptedPrompt();

                // Act
                await SortEmail.SaveAttachmentAsync(
                    helper,
                    Exists(true, new List<string>()),
                    pictures.Session,
                    attachments.Session,
                    altName.Session,
                    RecordingSave(saves)
                );

                // Assert
                pictures.Messages.Should().Equal(OverwritePrompt(helper));
                attachments.Messages.Should().BeEmpty();
                altName.Messages.Should().BeEmpty();
                saves.Should().Equal(helper.FilePathSave);
            }

            /// <summary>
            /// AS3. Scenario: the file exists and the attachment is a document. Expected: only the
            /// attachments session is asked and the Yes answer saves to the primary path.
            /// </summary>
            [TestMethod]
            public async Task SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly()
            {
                // Arrange
                var helper = CreateHelper(CreateAttachmentMock("report.pdf"), SandboxFolder);
                var saves = new List<string>();
                var pictures = new ScriptedPrompt();
                var attachments = new ScriptedPrompt(YesNoToAllResponse.Yes);
                var altName = new ScriptedPrompt();

                // Act
                await SortEmail.SaveAttachmentAsync(
                    helper,
                    Exists(true, new List<string>()),
                    pictures.Session,
                    attachments.Session,
                    altName.Session,
                    RecordingSave(saves)
                );

                // Assert
                attachments.Messages.Should().Equal(OverwritePrompt(helper));
                pictures.Messages.Should().BeEmpty();
                altName.Messages.Should().BeEmpty();
                saves.Should().Equal(helper.FilePathSave);
            }

            /// <summary>
            /// AS4. Scenario: the overwrite answer is Yes. Expected: the answer is released after the
            /// save, so the session holds no answer.
            /// </summary>
            [TestMethod]
            public async Task SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave()
            {
                // Arrange
                var helper = CreateHelper(CreateAttachmentMock("report.pdf"), SandboxFolder);
                var saves = new List<string>();
                var attachments = new ScriptedPrompt(YesNoToAllResponse.Yes);

                // Act
                await SortEmail.SaveAttachmentAsync(
                    helper,
                    Exists(true, new List<string>()),
                    new ScriptedPrompt().Session,
                    attachments.Session,
                    new ScriptedPrompt().Session,
                    RecordingSave(saves)
                );

                // Assert
                saves.Should().Equal(helper.FilePathSave);
                attachments.Session.Response.Should().Be(YesNoToAllResponse.Empty);
            }

            /// <summary>
            /// AS5. Scenario: the overwrite answer is YesToAll and a second attachment follows.
            /// Expected: one prompt, two primary-path saves, the YesToAll answer kept.
            /// </summary>
            [TestMethod]
            public async Task SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain()
            {
                // Arrange
                var first = CreateHelper(CreateAttachmentMock("report.pdf"), SandboxFolder);
                var second = CreateHelper(CreateAttachmentMock("notes.pdf"), SandboxFolder);
                var saves = new List<string>();
                var attachments = new ScriptedPrompt(YesNoToAllResponse.YesToAll);
                var altName = new ScriptedPrompt();

                // Act
                await SortEmail.SaveAttachmentAsync(
                    first,
                    Exists(true, new List<string>()),
                    new ScriptedPrompt().Session,
                    attachments.Session,
                    altName.Session,
                    RecordingSave(saves)
                );
                await SortEmail.SaveAttachmentAsync(
                    second,
                    Exists(true, new List<string>()),
                    new ScriptedPrompt().Session,
                    attachments.Session,
                    altName.Session,
                    RecordingSave(saves)
                );

                // Assert
                attachments.Messages.Should().ContainSingle();
                saves.Should().Equal(first.FilePathSave, second.FilePathSave);
                attachments.Session.Response.Should().Be(YesNoToAllResponse.YesToAll);
            }

            /// <summary>
            /// AS6. Scenario: the overwrite answer is No and the alternate-name answer is Yes.
            /// Expected: one try-save to the alternate path and both single answers released.
            /// </summary>
            [TestMethod]
            public async Task SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath()
            {
                // Arrange
                var helper = CreateHelper(CreateAttachmentMock("report.pdf"), SandboxFolder);
                var saves = new List<string>();
                var attachments = new ScriptedPrompt(YesNoToAllResponse.No);
                var altName = new ScriptedPrompt(YesNoToAllResponse.Yes);

                // Act
                await SortEmail.SaveAttachmentAsync(
                    helper,
                    Exists(true, new List<string>()),
                    new ScriptedPrompt().Session,
                    attachments.Session,
                    altName.Session,
                    RecordingSave(saves)
                );

                // Assert
                altName.Messages.Should().Equal(AltNamePrompt(helper));
                saves.Should().Equal(helper.FilePathSaveAlt);
                attachments.Session.Response.Should().Be(YesNoToAllResponse.Empty);
                altName.Session.Response.Should().Be(YesNoToAllResponse.Empty);
            }

            /// <summary>
            /// SS1. Scenario: the synchronous core is called and the file does not exist. Expected:
            /// one direct SaveAsFile to the primary path and no prompt.
            /// </summary>
            [TestMethod]
            public void SaveAttachment_WhenFileDoesNotExist_SavesDirectly()
            {
                // Arrange
                var attachment = CreateAttachmentMock("report.pdf");
                var helper = CreateHelper(attachment, SandboxFolder);
                var pictures = new ScriptedPrompt();
                var attachments = new ScriptedPrompt();

                // Act
                SortEmail.SaveAttachment(
                    helper,
                    Exists(false, new List<string>()),
                    pictures.Session,
                    attachments.Session
                );

                // Assert
                attachment.Verify(x => x.SaveAsFile(helper.FilePathSave), Times.Once);
                pictures.Messages.Should().BeEmpty();
                attachments.Messages.Should().BeEmpty();
            }

            /// <summary>
            /// SS2. Scenario: the file exists and the answer is Yes. Expected: one SaveAsFile to the
            /// primary path and the answer released.
            /// </summary>
            [TestMethod]
            public void SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer()
            {
                // Arrange
                var attachment = CreateAttachmentMock("report.pdf");
                var helper = CreateHelper(attachment, SandboxFolder);
                var attachments = new ScriptedPrompt(YesNoToAllResponse.Yes);

                // Act
                SortEmail.SaveAttachment(
                    helper,
                    Exists(true, new List<string>()),
                    new ScriptedPrompt().Session,
                    attachments.Session
                );

                // Assert
                attachment.Verify(x => x.SaveAsFile(helper.FilePathSave), Times.Once);
                attachments.Session.Response.Should().Be(YesNoToAllResponse.Empty);
            }

            /// <summary>
            /// SS3. Scenario: the file exists and the answer is NoToAll. Expected: one SaveAsFile to
            /// the alternate path through the corrected SaveCase labels (L1), none to the primary
            /// path, and the NoToAll answer kept.
            /// </summary>
            [TestMethod]
            public void SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer()
            {
                // Arrange
                var attachment = CreateAttachmentMock("report.pdf");
                var helper = CreateHelper(attachment, SandboxFolder);
                var attachments = new ScriptedPrompt(YesNoToAllResponse.NoToAll);

                // Act
                SortEmail.SaveAttachment(
                    helper,
                    Exists(true, new List<string>()),
                    new ScriptedPrompt().Session,
                    attachments.Session
                );

                // Assert
                attachment.Verify(x => x.SaveAsFile(helper.FilePathSaveAlt), Times.Once);
                attachment.Verify(x => x.SaveAsFile(helper.FilePathSave), Times.Never);
                attachments.Session.Response.Should().Be(YesNoToAllResponse.NoToAll);
            }

            /// <summary>
            /// RR. Scenario: a helper built under an origin folder is redirected to a destination
            /// folder. Expected: both the primary and the alternate save path move to the
            /// destination and both file names are unchanged (the re-rooting defect).
            /// </summary>
            [TestMethod]
            public void RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths()
            {
                // Arrange
                var helper = CreateHelper(CreateAttachmentMock("photo.jpg"), OriginFolder);
                var primaryName = Path.GetFileName(helper.FilePathSave);
                var alternateName = Path.GetFileName(helper.FilePathSaveAlt);

                // Act
                SortEmail.RedirectSaveFolder(helper, DestinationFolder);

                // Assert
                Path.GetDirectoryName(helper.FilePathSave).Should().Be(DestinationFolder);
                Path.GetDirectoryName(helper.FilePathSaveAlt).Should().Be(DestinationFolder);
                Path.GetFileName(helper.FilePathSave).Should().Be(primaryName);
                Path.GetFileName(helper.FilePathSaveAlt).Should().Be(alternateName);
            }

            /// <summary>
            /// CF. Scenario: the static prompt sessions of SortEmail are enumerated by reflection.
            /// Expected: every static session field is contained by reference in the reset list the
            /// cleanup iterates, the list has no duplicate and no stray entry, and there are exactly
            /// four production sessions. The test reads static state only.
            /// </summary>
            [TestMethod]
            public void Cleanup_Files_ResetsEveryPromptSession()
            {
                // Arrange
                var sessionFields = typeof(SortEmail)
                    .GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(field => field.FieldType == typeof(YesNoToAllPromptSession))
                    .ToList();
                var property = typeof(SortEmail).GetProperty(
                    "AllPromptSessions",
                    BindingFlags.NonPublic | BindingFlags.Static
                );

                // Act
                var resetTargets = property?.GetValue(null) as YesNoToAllPromptSession[];

                // Assert
                property.Should().NotBeNull();
                resetTargets.Should().NotBeNull();
                sessionFields.Should().HaveCount(4);
                resetTargets.Should().HaveCount(4);
                resetTargets.Should().OnlyHaveUniqueItems();
                foreach (var field in sessionFields)
                {
                    var session = field.GetValue(null);
                    resetTargets.Should().Contain(target => ReferenceEquals(target, session));
                }
            }

            private static Mock<Attachment> CreateAttachmentMock(string fileName)
            {
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                attachment.SetupGet(x => x.Type).Returns(OlAttachmentType.olByValue);
                attachment.SetupGet(x => x.BlockLevel).Returns((OlAttachmentBlockLevel)0);
                attachment.SetupGet(x => x.Class).Returns(OlObjectClass.olAttachment);
                attachment.SetupGet(x => x.DisplayName).Returns(fileName);
                attachment.SetupGet(x => x.FileName).Returns(fileName);
                attachment.SetupGet(x => x.Index).Returns(1);
                attachment.SetupGet(x => x.PathName).Returns(Path.Combine(@"C:\temp", fileName));
                attachment.SetupGet(x => x.Position).Returns(2);
                attachment.SetupGet(x => x.Size).Returns(1);
                return attachment;
            }

            private static AttachmentHelper CreateHelper(Mock<Attachment> attachment, string folder)
            {
                return new AttachmentHelper(attachment.Object, SentOn, folder);
            }

            private static string OverwritePrompt(AttachmentHelper helper)
            {
                return $"The file {helper.FilePathSave} already exists. Overwrite?";
            }

            private static string AltNamePrompt(AttachmentHelper helper)
            {
                return $"The file {helper.FilePathSave} already exists. Save with an alternate name?";
            }

            /// <summary>
            /// Returns a file-existence predicate that records the queried path and answers with the
            /// scripted value without touching any file.
            /// </summary>
            private static Func<string, bool> Exists(bool exists, List<string> queried)
            {
                return path =>
                {
                    queried.Add(path);
                    return exists;
                };
            }

            /// <summary>
            /// Returns a try-save delegate that records the requested path and reports success
            /// without touching any file.
            /// </summary>
            private static SortEmail.TrySaveAttachmentDelegate RecordingSave(List<string> saves)
            {
                return (attachment, path) =>
                {
                    saves.Add(path);
                    return Task.FromResult(true);
                };
            }

            /// <summary>
            /// Owns one prompt session whose answers are scripted; an unscripted prompt throws from
            /// the empty queue, which is how a test proves that a session was not asked.
            /// </summary>
            private sealed class ScriptedPrompt
            {
                private readonly Queue<YesNoToAllResponse> _answers;

                public ScriptedPrompt(params YesNoToAllResponse[] answers)
                {
                    _answers = new Queue<YesNoToAllResponse>(answers);
                    Session = new YesNoToAllPromptSession(Prompt);
                }

                public YesNoToAllPromptSession Session { get; }
                public List<string> Messages { get; } = new List<string>();

                private YesNoToAllResponse Prompt(string message)
                {
                    Messages.Add(message);
                    return _answers.Dequeue();
                }
            }
        }
    }

### Listing L-TEF (QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs, written by P5-T4)

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using QuickFiler.Controllers;
    using QuickFiler.Helper_Classes;
    using UtilitiesCS;
    using UtilitiesCS.EmailIntelligence.EmailParsingSorting;

    namespace QuickFiler.Test.Controllers
    {
        /// <summary>
        /// Issue #959 regression tests for the prompt-state reset of the five-parameter
        /// <c>MoveToFolderAsync</c> of <see cref="EfcDataModel"/>: the reset runs whether the filer
        /// returns or throws, and does not run when a guard returns early. The probe overrides the
        /// two virtual seams, so no filer and no dialog is constructed. The fixture helpers mirror
        /// those of EfcDataModelArchiveRootTests, which is deliberately left unchanged.
        /// </summary>
        [TestClass]
        public class EfcDataModelFilerCleanupTests
        {
            private const string ArchiveRootLiteral = @"\\mailbox@example.com\Archive";
            private const string DestinationStem = @"Clients\North";

            /// <summary>
            /// Scenario: the filer throws. Expected: the exception propagates to the caller and the
            /// prompt state was reset exactly once on the way out (the sticky-answer defect).
            /// </summary>
            [TestMethod]
            public async Task MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates()
            {
                // Arrange
                var probe = CreateProbe(SpecialFoldersWithOneDrive());
                probe.Filer = () =>
                    Task.FromException<bool>(new InvalidOperationException("filer failed"));

                // Act
                Func<Task> act = () => MoveAsync(probe);

                // Assert
                await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("filer failed");
                probe.ResetCalls.Should().Be(1);
            }

            /// <summary>
            /// Scenario (control): the filer returns true. Expected: the move reports success and the
            /// prompt state was reset exactly once.
            /// </summary>
            [TestMethod]
            public async Task MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce()
            {
                // Arrange
                var probe = CreateProbe(SpecialFoldersWithOneDrive());
                probe.Filer = () => Task.FromResult(true);

                // Act
                bool moved = await MoveAsync(probe);

                // Assert
                moved.Should().BeTrue();
                probe.ResetCalls.Should().Be(1);
            }

            /// <summary>
            /// Scenario (control): the OneDrive guard returns before the filer is invoked. Expected:
            /// the move reports failure and the prompt state is not reset, as today.
            /// </summary>
            [TestMethod]
            public async Task MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState()
            {
                // Arrange
                var probe = CreateProbe(SpecialFoldersWithoutOneDrive());
                probe.Filer = () => Task.FromResult(true);

                // Act
                bool moved = await MoveAsync(probe);

                // Assert
                moved.Should().BeFalse();
                probe.ResetCalls.Should().Be(0);
            }

            /// <summary>
            /// Invokes the five-argument move overload with the argument values every test in this
            /// class shares, so no test repeats the argument list.
            /// </summary>
            private static Task<bool> MoveAsync(EfcDataModel dataModel)
            {
                return dataModel.MoveToFolderAsync(
                    DestinationStem,
                    saveAttachments: false,
                    saveEmail: false,
                    savePictures: false,
                    moveConversation: false
                );
            }

            /// <summary>
            /// Builds a probe whose archive root resolves and whose special folders are the supplied
            /// dictionary; the guard test passes a dictionary without the OneDrive entry.
            /// </summary>
            private static FilerCleanupProbe CreateProbe(
                ConcurrentDictionary<string, string> specialFolders
            )
            {
                var olObjects = CreateOlObjects();
                olObjects.SetupGet(value => value.ArchiveRootPath).Returns(ArchiveRootLiteral);
                var globals = CreateGlobals(olObjects, specialFolders);
                return new FilerCleanupProbe(globals.Object);
            }

            /// <summary>
            /// A strict <see cref="IOlObjects"/> mock with no member configured, so an unexpected
            /// read fails loudly.
            /// </summary>
            private static Mock<IOlObjects> CreateOlObjects()
            {
                return new Mock<IOlObjects>(MockBehavior.Strict);
            }

            /// <summary>
            /// A strict <see cref="IApplicationGlobals"/> mock whose <c>Ol</c> getter returns the
            /// supplied Outlook seam and whose <c>FS</c> getter returns a stub exposing the supplied
            /// special-folder dictionary.
            /// </summary>
            private static Mock<IApplicationGlobals> CreateGlobals(
                Mock<IOlObjects> olObjects,
                ConcurrentDictionary<string, string> specialFolders
            )
            {
                var fileSystem = new Mock<IFileSystemFolderPaths>(MockBehavior.Strict);
                fileSystem.SetupGet(value => value.SpecialFolders).Returns(specialFolders);

                var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
                globals.SetupGet(value => value.Ol).Returns(olObjects.Object);
                globals.SetupGet(value => value.FS).Returns(fileSystem.Object);
                return globals;
            }

            /// <summary>A special-folder dictionary that resolves the OneDrive root.</summary>
            private static ConcurrentDictionary<string, string> SpecialFoldersWithOneDrive()
            {
                var specialFolders = new ConcurrentDictionary<string, string>();
                specialFolders["OneDrive"] = "OneDriveRoot";
                return specialFolders;
            }

            /// <summary>A special-folder dictionary with no OneDrive entry.</summary>
            private static ConcurrentDictionary<string, string> SpecialFoldersWithoutOneDrive()
            {
                return new ConcurrentDictionary<string, string>();
            }

            /// <summary>
            /// The data model under test with both virtual seams overridden. The base constructor
            /// receives a null mail item, so the first-selection lookup absorbs the strict mock's
            /// failure and builds no resolver; the derived constructor then assigns a two-argument
            /// resolver carrying a parameterless <see cref="MailItemHelper"/>, which makes the mail
            /// information non-null without an Outlook fixture (the EfcDataModelArchiveRootTests
            /// arrangement). The filer seam runs the scripted delegate; the reset seam counts calls.
            /// </summary>
            private sealed class FilerCleanupProbe : EfcDataModel
            {
                public FilerCleanupProbe(IApplicationGlobals globals)
                    : base(globals, null, new CancellationTokenSource(), CancellationToken.None)
                {
                    ConversationResolver = new ConversationResolver(globals, null)
                    {
                        MailHelper = new MailItemHelper(),
                    };
                }

                public Func<Task<bool>> Filer { get; set; } = () => Task.FromResult(true);

                public int ResetCalls { get; private set; }

                protected internal override Task<bool> InvokeFilerAsync(
                    EmailFilerConfig config,
                    IList<MailItemHelper> mailHelpers
                ) => Filer();

                protected internal override void ResetFilerPromptState()
                {
                    ResetCalls++;
                }
            }
        }
    }

### Listing L-A-FINAL (UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs, final state, revision 1.6 form with the nested `TrySaveAttachmentDelegate` seam type of PD-14; P4-T4 wrote the revision-1.5 form with exactly one line omitted, the second statement of the re-rooting helper; P4-T7 rewrites the file from this listing with the same line omitted, and P4-T10 inserts that line by Edit E-A-RR-SECOND)

    #nullable enable
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Office.Interop.Outlook;
    using UtilitiesCS.EmailIntelligence;

    namespace UtilitiesCS
    {
        public static partial class SortEmail
        {
            // Production answer state of the three attachment prompts. Tests pass their own sessions
            // to the seamed cores below, so no test reads or writes these instances. Cleanup_Files
            // resets them after each filing operation.
            private static readonly YesNoToAllPromptSession AttachmentsOverwritePrompt = new(
                YesNoToAll.ShowDialog
            );
            private static readonly YesNoToAllPromptSession PicturesOverwritePrompt = new(
                YesNoToAll.ShowDialog
            );
            private static readonly YesNoToAllPromptSession AttachmentsAltNamePrompt = new(
                YesNoToAll.ShowDialog
            );

            // A property, not an array field: a static field initializer in this partial could run
            // before the RemoveReadOnlyPrompt initializer in SortEmail.TrySaveAttachment.cs and
            // capture null, because the initialization order across partial files is unspecified.
            private static YesNoToAllPromptSession[] AllPromptSessions =>
                new[]
                {
                    AttachmentsOverwritePrompt,
                    PicturesOverwritePrompt,
                    AttachmentsAltNamePrompt,
                    RemoveReadOnlyPrompt,
                };

            public static void Cleanup_Files()
            {
                foreach (var prompt in AllPromptSessions)
                {
                    prompt.Reset();
                }
            }

            internal static IEnumerable<AttachmentHelper> GetAttachmentsInfo(
                MailItem mailItem,
                string saveFsPath,
                string? deleteFsPath,
                bool saveAttachments,
                bool savePictures
            )
            {
                var attachments = mailItem
                    .Attachments.Cast<Attachment>()
                    .Where(x => x.Type != OlAttachmentType.olOLE)
                    .Select(x => new AttachmentHelper(x, mailItem.SentOn, saveFsPath, deleteFsPath!));
                if (!saveAttachments)
                {
                    attachments = attachments.Where(x => x.AttachmentInfo.IsImage);
                }

                if (!savePictures)
                {
                    attachments = attachments.Where(x => !x.AttachmentInfo.IsImage);
                }
                return attachments;
            }

            internal static IAsyncEnumerable<AttachmentHelper> GetAttachmentsInfoAsync(
                MailItem mailItem,
                string saveFsPath,
                string? deleteFsPath,
                bool saveAttachments,
                bool savePictures
            )
            {
                //TraceUtility.LogMethodCall(mailItem, saveFsPath, deleteFsPath, saveAttachments, savePictures);
                // SelectAwait is obsolete (CS0618) per the framework's migration guidance ("Use
                // Select ... overloads of Select"), but the replacement overload requires adding a
                // CancellationToken parameter to the lambda. Suppressing narrowly preserves the
                // exact pre-existing behavior (no behavior change per AC7).
    #pragma warning disable CS0618
                var attachments = mailItem
                    .Attachments.Cast<Attachment>()
                    .Where(x => x.Type != OlAttachmentType.olOLE)
                    .ToAsyncEnumerable()
                    .SelectAwait(async x =>
                        await AttachmentHelper.CreateAsync(
                            x,
                            mailItem.SentOn,
                            saveFsPath,
                            deleteFsPath!
                        )
                    );
    #pragma warning restore CS0618
                if (!saveAttachments)
                {
                    attachments = attachments.Where(x => x.AttachmentInfo.IsImage);
                }

                if (!savePictures)
                {
                    attachments = attachments.Where(x => !x.AttachmentInfo.IsImage);
                }
                return attachments;
            }

            // Excluded from coverage: wiring only. Calling it from a test would run the real
            // file-existence check and show the production dialogs (UT4).
            [ExcludeFromCodeCoverage]
            public static void SaveAttachment(this AttachmentHelper attachmentHelper)
            {
                SaveAttachment(
                    attachmentHelper,
                    File.Exists,
                    PicturesOverwritePrompt,
                    AttachmentsOverwritePrompt
                );
            }

            /// <summary>
            /// Saves the attachment synchronously through injected seams. When the primary save path
            /// exists, the overwrite prompt that matches the attachment kind is asked (the pictures
            /// session for an image, the attachments session otherwise), the answer is applied by the
            /// synchronous save switch, and a single answer is released afterwards.
            /// </summary>
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

                var overwritePrompt = attachmentHelper.AttachmentInfo.IsImage
                    ? picturesOverwritePrompt
                    : attachmentsOverwritePrompt;
                var answer = overwritePrompt.Ask(
                    $"The file {attachmentHelper.FilePathSave} already exists. Overwrite?"
                );
                SaveCase(
                    answer,
                    attachmentHelper.Attachment,
                    attachmentHelper.FilePathSave,
                    attachmentHelper.FilePathSaveAlt
                );
                overwritePrompt.ReleaseSingleAnswer();
            }

            // Excluded from coverage: wiring only. Calling it from a test would run the real
            // file-existence check, show the production dialogs and create a real directory through
            // the two-argument try-save overload (UT4).
            [ExcludeFromCodeCoverage]
            public static Task SaveAttachmentAsync(this AttachmentHelper attachmentHelper)
            {
                return SaveAttachmentAsync(
                    attachmentHelper,
                    File.Exists,
                    PicturesOverwritePrompt,
                    AttachmentsOverwritePrompt,
                    AttachmentsAltNamePrompt,
                    TrySaveAttachmentAsync
                );
            }

            /// <summary>
            /// The try-save seam of the asynchronous attachment-saving cores: saves the attachment to
            /// the given path and reports whether the save succeeded. A named non-generic delegate
            /// rather than a generic one: <c>Attachment</c> is an embedded interop type in this
            /// assembly, and a generic instantiation over an embedded interop type cannot be used
            /// from the test assembly (compiler error CS1769), whereas a non-generic delegate
            /// signature over the same type can.
            /// </summary>
            internal delegate Task<bool> TrySaveAttachmentDelegate(
                Attachment attachment,
                string filePath
            );

            /// <summary>
            /// Saves the attachment through injected seams. When the primary save path exists, the
            /// overwrite prompt that matches the attachment kind is asked, the answer is applied by
            /// the asynchronous save switch with the alternate-name session, and a single answer is
            /// released afterwards. The result of <paramref name="trySave"/> is not inspected: a
            /// persistent failure surfaces by exception from the try-save path.
            /// </summary>
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

                var overwritePrompt = attachmentHelper.AttachmentInfo.IsImage
                    ? picturesOverwritePrompt
                    : attachmentsOverwritePrompt;
                var answer = overwritePrompt.Ask(
                    $"The file {attachmentHelper.FilePathSave} already exists. Overwrite?"
                );
                await SaveCaseAsync(
                    answer,
                    attachmentHelper.Attachment,
                    attachmentHelper.FilePathSave,
                    attachmentHelper.FilePathSaveAlt,
                    altNamePrompt,
                    trySave
                );
                overwritePrompt.ReleaseSingleAnswer();
            }

            // Excluded from coverage: wiring only; re-roots the helper and forwards to the excluded
            // parameterless wrapper above.
            [ExcludeFromCodeCoverage]
            public static Task SaveAttachmentAsync(
                this AttachmentHelper attachmentHelper,
                string destinationPath
            )
            {
                RedirectSaveFolder(attachmentHelper, destinationPath);
                return SaveAttachmentAsync(attachmentHelper);
            }

            /// <summary>
            /// Re-roots both the primary and the alternate save path to the destination folder,
            /// keeping their file names. The helpers are built with the mail's Outlook folder name,
            /// so both paths must move; moving only the primary path left the alternate path relative
            /// to the process working directory (issue #959).
            /// </summary>
            internal static void RedirectSaveFolder(
                AttachmentHelper attachmentHelper,
                string destinationPath
            )
            {
                attachmentHelper.FolderPathSave = destinationPath;
                attachmentHelper.FilePathHelperSaveAlt.FolderPath = destinationPath;
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
                    case YesNoToAllResponse r
                        when (r == YesNoToAllResponse.NoToAll || r == YesNoToAllResponse.No):
                        var altAnswer = altNamePrompt.Ask(
                            $"The file {filePathSave} already exists. Save with an alternate name?"
                        );
                        if (
                            altAnswer == YesNoToAllResponse.Yes
                            || altAnswer == YesNoToAllResponse.YesToAll
                        )
                        {
                            await trySave(attachment, filePathSaveAlt);
                        }
                        altNamePrompt.ReleaseSingleAnswer();
                        break;

                    case YesNoToAllResponse r
                        when (r == YesNoToAllResponse.YesToAll || r == YesNoToAllResponse.Yes):
                        await trySave(attachment, filePathSave);
                        break;

                    default:
                        break;
                }
            }

            internal static void SaveCase(
                YesNoToAllResponse response,
                Attachment attachment,
                string filePathSave,
                string filePathSaveAlt
            )
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

            internal static async Task SaveMessageAsMsgAsync(MailItem mailItem, string fsLocation)
            {
                //TraceUtility.LogMethodCall(mailItem, fsLocation);

                var filenameSeed = FolderConverter.SanitizeFilename(mailItem.Subject);

                var strPath = AttachmentHelper.AdjustForMaxPath(fsLocation, filenameSeed, "msg", "");
                await Task.Run(() => mailItem.SaveAs(strPath, OlSaveAsType.olMSG));
            }

            internal static void SaveMessageAsMSG(MailItem mailItem, string fsLocation)
            {
                var filenameSeed = FolderConverter.SanitizeFilename(mailItem.Subject);

                var strPath = AttachmentHelper.AdjustForMaxPath(fsLocation, filenameSeed, "msg", "");
                mailItem.SaveAs(strPath, OlSaveAsType.olMSG);
            }
        }
    }

### Listing L-T-FINAL (UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs, final state, written by P3-T4)

    #nullable enable
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Office.Interop.Outlook;

    namespace UtilitiesCS
    {
        public static partial class SortEmail
        {
            // Production answer state of the read-only-removal prompt. Tests pass their own session to
            // the five-argument overload, so no test reads or writes this instance; nothing can
            // replace it. Cleanup_Files resets it after each filing operation.
            private static readonly YesNoToAllPromptSession RemoveReadOnlyPrompt = new(
                YesNoToAll.ShowDialog
            );

            // Excluded from coverage: the only behavior of this wrapper is wiring the real
            // directory-creation default, and calling it from a test would create a real directory,
            // which the unit-test policy prohibits (UT4).
            /// <summary>
            /// Saves the attachment to <paramref name="filePathSave"/> and creates the destination
            /// directory on disk. The overload that takes a directory-creation delegate is the test
            /// seam.
            /// </summary>
            [ExcludeFromCodeCoverage]
            internal static Task<bool> TrySaveAttachmentAsync(
                this Attachment attachment,
                string filePathSave
            )
            {
                return TrySaveAttachmentAsync(
                    attachment,
                    filePathSave,
                    path => System.IO.Directory.CreateDirectory(path)
                );
            }

            /// <summary>
            /// Saves the attachment to <paramref name="filePathSave"/>. The
            /// <paramref name="createDirectory"/> delegate receives the destination directory before
            /// the attachment is saved, so a caller can replace directory creation (for example, a
            /// unit test that must not touch the file system). The read-only prompt uses the
            /// production session and the read-only attribute is cleared on disk.
            /// </summary>
            internal static Task<bool> TrySaveAttachmentAsync(
                this Attachment attachment,
                string filePathSave,
                Action<string> createDirectory
            )
            {
                return TrySaveAttachmentAsync(
                    attachment,
                    filePathSave,
                    createDirectory,
                    ClearReadOnlyAttributeOnDisk,
                    RemoveReadOnlyPrompt
                );
            }

            /// <summary>
            /// Saves the attachment to <paramref name="filePathSave"/> through injected seams.
            /// <paramref name="createDirectory"/> receives the destination directory before each save
            /// attempt. When the save is denied, <paramref name="removeReadOnlyPrompt"/> supplies the
            /// answer to the read-only prompt, asking only while it holds no answer, and
            /// <paramref name="clearReadOnly"/> clears the read-only attribute of the destination
            /// directory before the save is retried. This overload forwards to the private core with
            /// the retry flag cleared; the core bounds the retry to one clear per call.
            /// </summary>
            /// <returns>
            /// True when the attachment was saved; false when the answer declined the change or the
            /// attribute could not be cleared. A cancelled prompt, and a denial that persists after
            /// the attribute was cleared under a held "to all" answer, rethrow the original exception.
            /// </returns>
            internal static Task<bool> TrySaveAttachmentAsync(
                this Attachment attachment,
                string filePathSave,
                Action<string> createDirectory,
                Action<string> clearReadOnly,
                YesNoToAllPromptSession removeReadOnlyPrompt
            )
            {
                return TrySaveAttachmentCoreAsync(
                    attachment,
                    filePathSave,
                    createDirectory,
                    clearReadOnly,
                    removeReadOnlyPrompt,
                    isRetryAfterClear: false
                );
            }

            /// <summary>
            /// The retrying save. The last parameter records whether this call is the retry that
            /// follows a successful attribute clear, which is what bounds the recursion.
            /// </summary>
            private static async Task<bool> TrySaveAttachmentCoreAsync(
                Attachment attachment,
                string filePathSave,
                Action<string> createDirectory,
                Action<string> clearReadOnly,
                YesNoToAllPromptSession removeReadOnlyPrompt,
                bool isRetryAfterClear
            )
            {
                try
                {
                    createDirectory(Path.GetDirectoryName(filePathSave));
                    await Task.Run(() => attachment.SaveAsFile(filePathSave));
                    return true;
                }
                catch (System.UnauthorizedAccessException e)
                {
                    logger.Warn(
                        $"Saving {filePathSave} was denied; the read-only prompt decides whether to retry.",
                        e
                    );

                    // The attribute was already cleared once in this call chain and a "to all" answer
                    // is never asked again, so another clear-and-retry cannot change the outcome
                    // (issue #959, L2): surface the denial to the caller instead of looping.
                    if (
                        isRetryAfterClear
                        && removeReadOnlyPrompt.Response == YesNoToAllResponse.YesToAll
                    )
                    {
                        logger.Error(
                            $"The file {filePathSave} is still denied after the read-only attribute was cleared.",
                            e
                        );
                        throw;
                    }

                    // Exception usually is thrown when readonly folder attribute is set.
                    // When the session holds no answer yet, ask whether the user wants to remove the
                    // readonly attribute and retry saving.
                    if (removeReadOnlyPrompt.Response == YesNoToAllResponse.Empty)
                    {
                        var message =
                            $"The folder {Path.GetDirectoryName(filePathSave)} is read-only. Do you want to remove the readonly attribute?";
                        removeReadOnlyPrompt.Ask(message);
                    }

                    if (
                        (removeReadOnlyPrompt.Response == YesNoToAllResponse.Yes)
                        || (removeReadOnlyPrompt.Response == YesNoToAllResponse.YesToAll)
                    )
                    {
                        var directory = Path.GetDirectoryName(filePathSave);
                        try
                        {
                            clearReadOnly(directory);
                        }
                        catch (System.Exception inner)
                        {
                            logger.Error(
                                $"The read-only attribute of {directory} could not be cleared; {filePathSave} was not saved.",
                                inner
                            );
                            return false;
                        }
                        finally
                        {
                            removeReadOnlyPrompt.ReleaseSingleAnswer();
                        }
                        return await TrySaveAttachmentCoreAsync(
                            attachment,
                            filePathSave,
                            createDirectory,
                            clearReadOnly,
                            removeReadOnlyPrompt,
                            isRetryAfterClear: true
                        );
                    }
                    else if (
                        (removeReadOnlyPrompt.Response == YesNoToAllResponse.No)
                        || (removeReadOnlyPrompt.Response == YesNoToAllResponse.NoToAll)
                    )
                    {
                        logger.Warn(
                            $"The file {filePathSave} was not saved because the read-only change was declined."
                        );
                        removeReadOnlyPrompt.ReleaseSingleAnswer();
                        return false;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Excluded from coverage: a file-system adapter whose execution requires the real file
            // system, which unit tests must not touch (UT4). Tests pass their own delegate to the
            // five-argument overload instead.
            [ExcludeFromCodeCoverage]
            private static void ClearReadOnlyAttributeOnDisk(string directoryPath)
            {
                var di = new DirectoryInfo(directoryPath);
                di.Attributes &= ~System.IO.FileAttributes.ReadOnly;
            }
        }
    }

### Listing L-U-FINAL (UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs, final state, written by P2-T7)

    #nullable enable
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

    namespace UtilitiesCS
    {
        public static partial class SortEmail
        {
            // Column names of the moved-mails log, in the order of the record fields that
            // EmailDetails.Details produces after its unused index zero.
            private static readonly string[] MovedMailsHeader =
            {
                "Triage",
                "FolderName",
                "Sent_On",
                "From",
                "To",
                "CC",
                "Subject",
                "Body",
                "fromDomain",
                "Conversation_ID",
                "EntryID",
                "Attachments",
                "FlaggedAsTask",
            };

            // Duplicative with QuickFiler but it is still mapped to main menu so I need to take it out
            [ExcludeFromCodeCoverage]
            public static async Task UndoAsync(
                SloStack<IMovedMailInfo> movedStack,
                IApplicationGlobals globals
            )
            {
                DialogResult repeatResponse = DialogResult.Yes;
                var i = 0;

                while (i < movedStack.Count && repeatResponse == DialogResult.Yes)
                {
                    var message = movedStack[i].UndoMoveMessage(globals.Ol.App);
                    if (message is null)
                    {
                        i++;
                    }
                    else
                    {
                        var undoResponse = MessageBox.Show(
                            message,
                            "Undo Dialog",
                            MessageBoxButtons.YesNo
                        );
                        if (undoResponse == DialogResult.Yes)
                        {
                            var helper = await MailItemHelper.FromMailItemAsync(
                                movedStack[i].MailItem,
                                globals,
                                default,
                                true
                            );
                            (
                                await new OlFolderClassifierGroup(globals).GetFolderPredictorAsync()
                            ).UnTrain(helper.FolderInfo!.RelativePath, helper.Tokens!, 1);
                            movedStack[i].UndoMove();
                            movedStack.Pop(i);
                        }
                        else
                        {
                            i++;
                        }
                        repeatResponse = MessageBox.Show(
                            "Continue Undoing Moves?",
                            "Undo Dialog",
                            MessageBoxButtons.YesNo
                        );
                    }
                }

                if (repeatResponse == DialogResult.Yes)
                {
                    MessageBox.Show("Nothing to undo");
                }
                movedStack.Serialize();
            }

            [ExcludeFromCodeCoverage]
            private static void PushToUndoStack(
                MailItem beforeMove,
                MailItem afterMove,
                IApplicationGlobals _globals
            )
            {
                //TODO: Delete _globals.Ol.MovedMails_Stack because it is obsolete
                var info = new MovedMailInfo(beforeMove, afterMove, _globals.Ol.Root.FolderPath);
                _globals.AF.MovedMails.Push(info);
            }

            [ExcludeFromCodeCoverage]
            private static void CaptureMoveDetails(
                MailItem mailItem,
                MailItem oMailTmp,
                IApplicationGlobals _globals
            )
            {
                //TraceUtility.LogMethodCall(mailItem, oMailTmp, _globals);

                string[] strAry = oMailTmp.Details(_globals.Ol.ArchiveRootPath).Skip(1).ToArray();
                var output = SanitizeArrayLineTSV(ref strAry);

                _globals.Ol.EmailMoveWriter.Enqueue(output);
            }

            private static string SanitizeArrayLineTSV(ref string[] strOutput)
            {
                //if (strOutput.IsInitialized())
                //{
                var line = string.Join(
                    "\t",
                    strOutput
                        //.Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => s ?? "")
                        .Select(s => StripTabsCrLf(s))
                        .ToArray()
                );
                return line;
                //}
                //else { return ""; }
            }

            internal static string StripTabsCrLf(string str)
            {
                var _regex = new Regex(@"[\t\n\r]+");
                string result = _regex.Replace(str, " ");

                // ensure max of one space per word
                _regex = new Regex(@"  +");
                result = _regex.Replace(result, " ");
                result = result.Trim();
                return result;
            }

            // Excluded from coverage: wiring of the real file-system defaults only; a test call would
            // read and write the disk (UT4). The four-parameter overload is the test seam.
            [ExcludeFromCodeCoverage]
            public static void WriteCSV_StartNewFileIfDoesNotExist(
                string strFileName,
                string strFileLocation
            )
            {
                WriteCSV_StartNewFileIfDoesNotExist(
                    strFileName,
                    strFileLocation,
                    File.Exists,
                    FileIO2.WriteTextFile
                );
            }

            /// <summary>
            /// Seeds the moved-mails log with its single tab-separated header line when the file does
            /// not exist. <paramref name="fileExists"/> answers whether the combination of the folder
            /// and the file name exists; <paramref name="writeTextFile"/> receives the file name, the
            /// lines and the folder, in that order.
            /// </summary>
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

                writeTextFile(
                    strFileName,
                    new[] { string.Join("\t", MovedMailsHeader) },
                    strFileLocation
                );
            }
        }
    }

### Listing L-TST1-ROWS (the six TRX row names of the two data-driven `GetAttachmentsInfo` tests of UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs after P4-T6; the replacement method texts are the NEW blocks of Edits E-TST1-ROWS-SYNC and E-TST1-ROWS-ASYNC)

    GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments false, savePictures true]
    GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments true, savePictures true]
    GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments true, savePictures false]
    GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments true, savePictures false]
    GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments true, savePictures true]
    GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments false, savePictures true]

Row semantics (PD-6): the two method names are kept (AC13 refers to the existing tests) and every row tag states its flag combination explicitly; the first row of each test is the pre-existing arrangement (synchronous `(false, true)`, asynchronous `(true, false)`); the second row is the spec's `(true, true)` row; the third row is the complementary filter row. Each row passes `saveAttachments`, `savePictures` and the comma-joined expected file names in attachment order (`photo.jpg,report.pdf` for the unfiltered row), so the three attachments `photo.jpg`, `report.pdf` and the OLE `ignored.ole` of the existing arrangement are reused unchanged.

## Edit Specifications

Each Edit names its file, its task and its OLD and NEW blocks (four-space convention of Execution Conventions). Every OLD text was verified to occur exactly once in its file at 94287369908cc920b21b0e3256314f988ad7d2f5, or, for an Edit applied after an earlier Edit or Write of this plan, exactly once in the state that earlier task produces (stated per Edit). Three Edits (E-A-RR-SECOND, E-A-CONTROL-MUTATE, E-A-CONTROL-RESTORE) are applied after a CSharpier pass over the file; for those, the OLD and NEW texts are identified by their trimmed lines and the executor passes them with the leading whitespace the file carries at that moment (read with the Read tool immediately before the Edit); every other Edit is passed exactly as written here.

#### E-A-L1 (P1-T8; file A; OLD occurs once at the cited tree)

OLD:

            [ExcludeFromCodeCoverage]
            internal static void SaveCase(
                YesNoToAllResponse response,
                Attachment attachment,
                string filePathSave,
                string filePathSaveAlt
            )
            {
                switch (response)
                {
                    case (YesNoToAllResponse.NoToAll | YesNoToAllResponse.No):
                        attachment.SaveAsFile(filePathSaveAlt);
                        break;
                    case (YesNoToAllResponse.Yes | YesNoToAllResponse.YesToAll):
                        attachment.SaveAsFile(filePathSave);
                        break;

NEW:

            internal static void SaveCase(
                YesNoToAllResponse response,
                Attachment attachment,
                string filePathSave,
                string filePathSaveAlt
            )
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

#### E-A-L3 (P1-T9; file A; OLD occurs once: the twelve-space-indented pair of A lines 34 to 35)

OLD:

                _picturesOverwrite = YesNoToAllResponse.Empty;
                RemoveReadOnlyPrompt.Reset();

NEW:

                _picturesOverwrite = YesNoToAllResponse.Empty;
                _attachmentsAltName = YesNoToAllResponse.Empty;
                RemoveReadOnlyPrompt.Reset();

#### E-UCT-CSPROJ-1 (P1-T3; file UtilitiesCS.Test/UtilitiesCS.Test.csproj; OLD is line 99)

OLD:

        <Compile Include="EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs" />

NEW:

        <Compile Include="EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs" />
        <Compile Include="EmailIntelligence\SortEmail_SaveCase_Tests.cs" />
        <Compile Include="EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs" />

#### E-UCT-CSPROJ-2 (P2-T2; same file; OLD is the line E-UCT-CSPROJ-1 added at position 101)

OLD:

        <Compile Include="EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs" />

NEW:

        <Compile Include="EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs" />
        <Compile Include="EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs" />

#### E-U-SEAM-HEAD (P2-T3; file U; OLD is U lines 139 to 149, once)

OLD:

            [ExcludeFromCodeCoverage]
            public static void WriteCSV_StartNewFileIfDoesNotExist(
                string strFileName,
                string strFileLocation
            )
            {
                string[]? strOutput = null;
                string[,]? strAryOutput;
                if (File.Exists(Path.Combine(strFileName, strFileLocation)))
                {
                    strAryOutput = new string[14, 2];

NEW:

            // Excluded from coverage: wiring of the real file-system defaults only; a test call would
            // read and write the disk (UT4). The four-parameter overload is the test seam.
            [ExcludeFromCodeCoverage]
            public static void WriteCSV_StartNewFileIfDoesNotExist(
                string strFileName,
                string strFileLocation
            )
            {
                WriteCSV_StartNewFileIfDoesNotExist(
                    strFileName,
                    strFileLocation,
                    File.Exists,
                    FileIO2.WriteTextFile
                );
            }

            internal static void WriteCSV_StartNewFileIfDoesNotExist(
                string strFileName,
                string strFileLocation,
                Func<string, bool> fileExists,
                Action<string, string[], string> writeTextFile
            )
            {
                string[]? strOutput = null;
                string[,]? strAryOutput;
                if (fileExists(Path.Combine(strFileName, strFileLocation)))
                {
                    strAryOutput = new string[14, 2];

#### E-U-SEAM-WRITE (P2-T3, applied after E-U-SEAM-HEAD; file U; OLD is U line 166, once)

OLD:

                    FileIO2.WriteTextFile(strFileName, strOutput!, folderpath: strFileLocation);

NEW:

                    writeTextFile(strFileName, strOutput!, strFileLocation);

The seam state of U after both Edits is the current body behind the four-parameter signature: the reversed `Path.Combine`, the inverted condition, the two-dimensional array and the `SanitizeArray` call are all still present, so L4-T1 and L4-T2 fail against it (P2-T6) before P2-T7 writes Listing L-U-FINAL.

#### E-TST1-DEL-SANITIZE (P2-T8; file TST1; OLD is TST1 lines 357 to 384, once)

OLD:

            }

            [TestMethod]
            public void SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows()
            {
                // Arrange
                var method = typeof(SortEmail).GetMethod(
                    "SanitizeArray",
                    BindingFlags.NonPublic | BindingFlags.Static
                )!;
                var values = new string[2, 2]
                {
                    { "A\tB", null },
                    { "Line1\r\nLine2", "Tail" },
                };
                var output = new string[values.GetLength(0)];
                object[] args = { values, output };

                // Act
                method.Invoke(null, args);
                output = (string[])args[1];

                // Assert
                output[0].Should().Be("A B");
                output[1].Should().Be("Line1 Line2\tTail");
            }

            #endregion

NEW:

            }

            #endregion

#### E-TST2-TRIPWIRE (P3-T1; file TST2; OLD is TST2 lines 348 to 357, once)

OLD:

                public YesNoToAllPromptSession Session { get; }
                public List<string> CreatedDirectories { get; } = new List<string>();
                public List<string> ClearedDirectories { get; } = new List<string>();
                public List<string> PromptMessages { get; } = new List<string>();
                public System.Exception ClearException { get; set; }

                public void CreateDirectory(string path)
                {
                    CreatedDirectories.Add(path);
                }

NEW:

                public YesNoToAllPromptSession Session { get; }
                public List<string> CreatedDirectories { get; } = new List<string>();
                public List<string> ClearedDirectories { get; } = new List<string>();
                public List<string> PromptMessages { get; } = new List<string>();
                public System.Exception ClearException { get; set; }

                // Tripwire for an unbounded retry: the directory-creation seam runs once per save
                // attempt, so a limit on recorded calls ends a loop deterministically without a timer.
                public int CreateDirectoryLimit { get; set; } = int.MaxValue;

                public void CreateDirectory(string path)
                {
                    if (CreatedDirectories.Count >= CreateDirectoryLimit)
                    {
                        throw new InvalidOperationException("retry bound exceeded");
                    }
                    CreatedDirectories.Add(path);
                }

#### E-TST2-T12 (P3-T1, applied after E-TST2-TRIPWIRE; file TST2; OLD is TST2 lines 319 to 322, once)

OLD:

            /// <summary>
            /// Calls the five-argument overload under test with the sandbox file path and the seams
            /// of the given recorder.
            /// </summary>

NEW:

            /// <summary>
            /// T12. Scenario: YesToAll is held, the attribute is cleared once and the retried save is
            /// denied again (issue #959, L2). Expected: the original access exception is rethrown
            /// after exactly one clear, two save attempts and one prompt; the YesToAll answer stays
            /// held. The directory-creation tripwire ends an unbounded retry deterministically.
            /// </summary>
            [TestMethod]
            public async Task TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear()
            {
                // Arrange
                var seams = new Seams(YesNoToAllResponse.YesToAll) { CreateDirectoryLimit = 3 };
                var denied = new UnauthorizedAccessException("denied");
                var attachment = new Mock<Attachment>(MockBehavior.Loose);
                attachment.Setup(x => x.SaveAsFile(SandboxFilePath)).Throws(denied);

                // Act
                Func<Task> act = () => SaveAsync(attachment, seams);

                // Assert
                (await act.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Should().BeSameAs(denied);
                seams.CreatedDirectories.Should().Equal(SandboxDirectory, SandboxDirectory);
                seams.ClearedDirectories.Should().Equal(SandboxDirectory);
                seams.PromptMessages.Should().Equal(ExpectedPrompt);
                seams.Session.Response.Should().Be(YesNoToAllResponse.YesToAll);
                attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Exactly(2));
            }

            /// <summary>
            /// Calls the five-argument overload under test with the sandbox file path and the seams
            /// of the given recorder.
            /// </summary>

#### E-A-RR-SECOND (P4-T10; file A in the extraction state after the P4-T7 rewrite and format (the revision-1.6 listing with the re-rooting line still omitted); identified by trimmed lines, indentation as found; OLD occurs once because the destination overload no longer assigns the folder itself)

OLD:

                attachmentHelper.FolderPathSave = destinationPath;
            }

NEW:

                attachmentHelper.FolderPathSave = destinationPath;
                attachmentHelper.FilePathHelperSaveAlt.FolderPath = destinationPath;
            }

#### E-A-CONTROL-MUTATE (P4-T14; file A in the final state; identified by trimmed lines, indentation as found; OLD occurs once: inside the array of `AllPromptSessions` the alternate-name element is directly followed by the read-only element, whereas in the asynchronous wrapper it is followed by the try-save method group)

OLD:

                    AttachmentsAltNamePrompt,
                    RemoveReadOnlyPrompt,

NEW:

                    RemoveReadOnlyPrompt,

#### E-A-CONTROL-RESTORE (P4-T15; file A in the mutated state; identified by trimmed lines, indentation as found; OLD occurs once because `RemoveReadOnlyPrompt,` with its trailing comma appears only in the array)

OLD:

                    RemoveReadOnlyPrompt,

NEW:

                    AttachmentsAltNamePrompt,
                    RemoveReadOnlyPrompt,

#### E-UCS-CSPROJ-REMOVE (P4-T5; file UtilitiesCS/UtilitiesCS.csproj; OLD is lines 819 to 821, once)

OLD:

        <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs" />
        <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs" />
        <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs" />

NEW:

        <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs" />
        <Compile Include="EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs" />

#### E-TST1-ROWS-SYNC (P4-T6; file TST1 after P2-T8; OLD is TST1 lines 182 to 207 of the cited tree, which P2-T8 and the P2-T9 format leave unchanged; once)

OLD:

            [TestMethod]
            public void GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments()
            {
                // Arrange
                var mailItem = CreateMailItemWithAttachments(
                    CreateAttachmentMock("photo.jpg", OlAttachmentType.olByValue).Object,
                    CreateAttachmentMock("report.pdf", OlAttachmentType.olByValue).Object,
                    CreateAttachmentMock("ignored.ole", OlAttachmentType.olOLE).Object
                );

                // Act
                var attachments = SortEmail
                    .GetAttachmentsInfo(
                        mailItem.Object,
                        GetRepositoryRoot().FullName,
                        null,
                        saveAttachments: false,
                        savePictures: true
                    )
                    .ToList();

                // Assert
                attachments.Should().ContainSingle();
                attachments[0].AttachmentInfo.FileName.Should().Be("photo.jpg");
                attachments[0].AttachmentInfo.IsImage.Should().BeTrue();
            }

NEW:

            [DataTestMethod]
            [DataRow(
                false,
                true,
                "photo.jpg",
                DisplayName = "GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments false, savePictures true]"
            )]
            [DataRow(
                true,
                true,
                "photo.jpg,report.pdf",
                DisplayName = "GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments true, savePictures true]"
            )]
            [DataRow(
                true,
                false,
                "report.pdf",
                DisplayName = "GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments [saveAttachments true, savePictures false]"
            )]
            public void GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments(
                bool saveAttachments,
                bool savePictures,
                string expectedFileNames
            )
            {
                // Arrange
                var mailItem = CreateMailItemWithAttachments(
                    CreateAttachmentMock("photo.jpg", OlAttachmentType.olByValue).Object,
                    CreateAttachmentMock("report.pdf", OlAttachmentType.olByValue).Object,
                    CreateAttachmentMock("ignored.ole", OlAttachmentType.olOLE).Object
                );

                // Act
                var attachments = SortEmail
                    .GetAttachmentsInfo(
                        mailItem.Object,
                        GetRepositoryRoot().FullName,
                        null,
                        saveAttachments: saveAttachments,
                        savePictures: savePictures
                    )
                    .ToList();

                // Assert
                attachments
                    .Select(x => x.AttachmentInfo.FileName)
                    .Should()
                    .Equal(expectedFileNames.Split(','));
                attachments
                    .Should()
                    .OnlyContain(x => x.AttachmentInfo.IsImage == (x.AttachmentInfo.FileName == "photo.jpg"));
            }

#### E-TST1-ROWS-ASYNC (P4-T6, applied after E-TST1-ROWS-SYNC; file TST1; OLD is TST1 lines 209 to 234 of the cited tree, once)

OLD:

            [TestMethod]
            public async Task GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments()
            {
                // Arrange
                var mailItem = CreateMailItemWithAttachments(
                    CreateAttachmentMock("photo.jpg", OlAttachmentType.olByValue).Object,
                    CreateAttachmentMock("report.pdf", OlAttachmentType.olByValue).Object,
                    CreateAttachmentMock("ignored.ole", OlAttachmentType.olOLE).Object
                );

                // Act
                var attachments = await CollectAsync(
                    SortEmail.GetAttachmentsInfoAsync(
                        mailItem.Object,
                        GetRepositoryRoot().FullName,
                        null,
                        saveAttachments: true,
                        savePictures: false
                    )
                );

                // Assert
                attachments.Should().ContainSingle();
                attachments[0].AttachmentInfo.FileName.Should().Be("report.pdf");
                attachments[0].AttachmentInfo.IsImage.Should().BeFalse();
            }

NEW:

            [DataTestMethod]
            [DataRow(
                true,
                false,
                "report.pdf",
                DisplayName = "GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments true, savePictures false]"
            )]
            [DataRow(
                true,
                true,
                "photo.jpg,report.pdf",
                DisplayName = "GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments true, savePictures true]"
            )]
            [DataRow(
                false,
                true,
                "photo.jpg",
                DisplayName = "GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments [saveAttachments false, savePictures true]"
            )]
            public async Task GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments(
                bool saveAttachments,
                bool savePictures,
                string expectedFileNames
            )
            {
                // Arrange
                var mailItem = CreateMailItemWithAttachments(
                    CreateAttachmentMock("photo.jpg", OlAttachmentType.olByValue).Object,
                    CreateAttachmentMock("report.pdf", OlAttachmentType.olByValue).Object,
                    CreateAttachmentMock("ignored.ole", OlAttachmentType.olOLE).Object
                );

                // Act
                var attachments = await CollectAsync(
                    SortEmail.GetAttachmentsInfoAsync(
                        mailItem.Object,
                        GetRepositoryRoot().FullName,
                        null,
                        saveAttachments: saveAttachments,
                        savePictures: savePictures
                    )
                );

                // Assert
                attachments
                    .Select(x => x.AttachmentInfo.FileName)
                    .Should()
                    .Equal(expectedFileNames.Split(','));
                attachments
                    .Should()
                    .OnlyContain(x => x.AttachmentInfo.IsImage == (x.AttachmentInfo.FileName == "photo.jpg"));
            }

#### E-S-USINGS (P5-T1; file S; OLD is S lines 2 to 19, once)

OLD:

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Deedle;
    using Microsoft.Office.Interop.Outlook;
    using SDILReader;
    using UtilitiesCS;
    using UtilitiesCS.EmailIntelligence;
    using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
    using UtilitiesCS.OutlookExtensions;
    using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;
    using Outlook = Microsoft.Office.Interop.Outlook;

NEW:

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Office.Interop.Outlook;
    using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
    using UtilitiesCS.OutlookExtensions;

#### E-M-USINGS (P5-T2; file M; OLD is M lines 2 to 19, identical to the E-S-USINGS OLD block, once in M)

OLD: the E-S-USINGS OLD block.

NEW:

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Microsoft.Office.Interop.Outlook;
    using UtilitiesCS.OutlookExtensions;

#### E-QFT-CSPROJ (P5-T5; file QuickFiler.Test/QuickFiler.Test.csproj; OLD is line 127, once)

OLD:

        <Compile Include="Controllers\EfcDataModelArchiveRootTests.cs" />

NEW:

        <Compile Include="Controllers\EfcDataModelArchiveRootTests.cs" />
        <Compile Include="Controllers\EfcDataModelFilerCleanupTests.cs" />

#### E-E-SEAM-CALL (P5-T6; file E; OLD is E lines 308 to 310, once)

OLD:

                var result = await InvokeFilerAsync(config, mailHelpers);
                SortEmail.Cleanup_Files();
                return result;

NEW:

                var result = await InvokeFilerAsync(config, mailHelpers);
                ResetFilerPromptState();
                return result;

#### E-E-SEAM-MEMBER (P5-T6, applied after E-E-SEAM-CALL; file E; OLD is E lines 325 to 326, once)

OLD:

                return new EmailFiler(config).SortAsync(mailHelpers);
            }

NEW:

                return new EmailFiler(config).SortAsync(mailHelpers);
            }

            /// <summary>
            /// Releases the sticky prompt answers held by <see cref="SortEmail"/> after a filing
            /// operation. Virtual for the same reason as <see cref="InvokeFilerAsync"/>: a test
            /// override records the call, because the answers live in internal state that
            /// QuickFiler.Test cannot observe (issue #959).
            /// </summary>
            protected internal virtual void ResetFilerPromptState()
            {
                SortEmail.Cleanup_Files();
            }

#### E-E-FINALLY (P5-T8; file E in the P5-T6 seam state after the P5-T6 format; OLD is the E-E-SEAM-CALL NEW block, once)

OLD:

                var result = await InvokeFilerAsync(config, mailHelpers);
                ResetFilerPromptState();
                return result;

NEW:

                bool result;
                try
                {
                    result = await InvokeFilerAsync(config, mailHelpers);
                }
                finally
                {
                    // Sticky "to all" prompt answers must not survive into the next filing operation
                    // when the filer throws (issue #959; the same root cause as the missing
                    // alternate-name reset in the SortEmail cleanup).
                    ResetFilerPromptState();
                }
                return result;

#### E-SPEC956-99 (P5-T11; file SPEC956; OLD is the trailing clause of line 99, once)

OLD:

    as merge-base line 944 does, so the exception boundary is unchanged.

NEW:

    as merge-base line 944 does; the `DirectoryInfo` construction, which merge-base line 944 performed before the `try`, now runs inside it as the first statement of the adapter (see "Boundaries and invariants to preserve").

#### E-SPEC956-149 (P5-T11; file SPEC956; OLD is the end of line 149, once; NEW keeps the line and appends one empty line and the note)

OLD:

    and no retry bound is added (L2 is out of scope).

NEW:

    and no retry bound is added (L2 is out of scope).

    > Superseded 2026-10-02 by #959 (closes #966): the outer rethrow is removed, `Debug.WriteLine` is replaced by the class logger, and a one-clear retry bound is added. The constraints in this paragraph and in AC6 applied to #956 only.

#### E-SPEC956-155 (P5-T11; file SPEC956; OLD is line 155, once)

OLD:

    - The `DirectoryInfo` construction's exception boundary (path computed before the inner `try`).

NEW:

    - The read-only clear's exception boundary: `Path.GetDirectoryName` is computed before the inner `try`, and the `DirectoryInfo` construction and the attribute write both run inside it through `ClearReadOnlyAttributeOnDisk` (D2 items 3 and 5). At merge-base the construction ran before the `try`; a `DirectoryInfo` constructor failure is therefore now caught and returns false. This is not reachable in production because `Directory.CreateDirectory` and `Path.GetDirectoryName` already validated the same path in the first `try` (code review CR-1, 2026-10-01).

#### E-TST1-DOC-CLEANUP (P6-T13; file TST1; OLD is lines 171 to 172 at HEAD 9de3f176d, the two text lines of the summary element of `Cleanup_Files_DoesNotThrow`, once in the file because line 171 is the only line carrying `tracking fields`; NEW is two lines, so the element and the file keep their line counts; the `/// <summary>` and `/// </summary>` lines at 170 and 173 are not part of the Edit; revision 1.9, PD-15)

OLD:

            /// Verifies that Cleanup_Files resets all static YesNoToAllResponse tracking fields
            /// without throwing, covering the state-reset method used between sort sessions.

NEW:

            /// Verifies that Cleanup_Files, which resets every prompt session in AllPromptSessions,
            /// completes without throwing, covering the state-reset method used between sort sessions.

#### E-TAS-SS4 (P7-T2; file TAS; OLD is TAS lines 296 to 300 at HEAD eb7871502, once in the file because line 296 is the only line carrying `Be(YesNoToAllResponse.NoToAll)` and line 300 the only line carrying `RR. Scenario`; NEW keeps those five lines and inserts the thirty-two lines of the SS4 test (thirty-one lines plus one separating blank line) between the closing brace of SS3 and the summary of RR, so TAS grows from 437 to 469 lines before the formatter; the SS4 comment and body repeat no TOKENS-TAS token other than the ones the SS4 column counts; revision 2.0, PD-16)

OLD:

                attachments.Session.Response.Should().Be(YesNoToAllResponse.NoToAll);
            }

            /// <summary>
            /// RR. Scenario: a helper built under an origin folder is redirected to a destination

NEW:

                attachments.Session.Response.Should().Be(YesNoToAllResponse.NoToAll);
            }

            /// <summary>
            /// SS4. Scenario: the synchronous core is called, the file exists and the attachment is
            /// an image. Expected: only the pictures session is asked, with the overwrite text, the
            /// Yes answer gives one SaveAsFile to the primary path and none to the alternate path,
            /// and the answer is released (CR-1 of the 2026-10-06 code review: the image arm of the
            /// synchronous session selection; the asynchronous arm is pinned by AS2).
            /// </summary>
            [TestMethod]
            public void SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly()
            {
                // Arrange
                var attachment = CreateAttachmentMock("photo.jpg");
                var helper = CreateHelper(attachment, SandboxFolder);
                var pictures = new ScriptedPrompt(YesNoToAllResponse.Yes);
                var attachments = new ScriptedPrompt();

                // Act
                SortEmail.SaveAttachment(
                    helper,
                    Exists(true, new List<string>()),
                    pictures.Session,
                    attachments.Session
                );

                // Assert
                pictures.Messages.Should().Equal(OverwritePrompt(helper));
                attachments.Messages.Should().BeEmpty();
                attachment.Verify(x => x.SaveAsFile(helper.FilePathSave), Times.Once);
                attachment.Verify(x => x.SaveAsFile(helper.FilePathSaveAlt), Times.Never);
                pictures.Session.Response.Should().Be(YesNoToAllResponse.Empty);
            }

            /// <summary>
            /// RR. Scenario: a helper built under an origin folder is redirected to a destination

#### E-TSC-USING (P7-T3; file TSC; OLD is TSC lines 1 to 2 at HEAD eb7871502, once because `using System;` occurs exactly once in the file, at line 1; NEW drops the first line, so TSC shrinks from 343 to 342 lines and the remaining directives keep CSharpier order; revision 2.0, PD-16, CR-3)

OLD:

    using System;
    using System.Collections.Generic;

NEW:

    using System.Collections.Generic;

#### E-AC-CHECKOFF (P6-T19 to P6-T23 and P6-T25 to P6-T44; file FEATURE/spec.md; `N` is the criterion number)

OLD: `- [ ] ACN (` (the five characters `- [ ] `, `AC`, the number and ` (`; once per criterion because the number is followed by a space and an opening parenthesis). NEW: `- [x] ACN (`. No other character of the line changes.

## Test Inventory

TRX `testName` values are bare method names, or `DisplayName` values for data rows (PD-5). Every set below is enumerated completely; a run whose `RESULT` rows are not exactly the named set is a filter or assembly mismatch, never a pass.

- `NAMES-TST1-BASE` (15): listed in the Test Totals section.
- `NAMES-TST1-FINAL` (18): `InitializeSortToExisting_AlwaysThrows_NotImplementedException`, `InitializeSortToExisting_WithExplicitArgs_StillThrows_NotImplementedException`, `SortAsync_MailHelpers_WhenNull_ThrowsArgumentNullException`, `SortAsync_MailHelpers_WhenEmpty_ThrowsArgumentNullException`, `StripTabsCrLf_WithControlCharacters_ReturnsCleanedSingleSpacedString`, `StripTabsCrLf_WithPlainText_ReturnsOriginalString`, `Cleanup_Files_DoesNotThrow`, the six rows of Listing L-TST1-ROWS, `TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile`, `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave`, `SaveMessageAsMsgAsync_WhenSubjectNeedsSanitizing_UsesMsgSavePath`, `SaveMessageAsMSG_WhenSubjectNeedsSanitizing_UsesMsgSavePath`, `SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine`. After P2-T8 and before P4-T6 the set is `NAMES-TST1-BASE` minus `SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows` (14, `NAMES-TST1-MID`).
- `NAMES-T` (11, TST2 lines 34, 58, 85, 111, 141, 166, 194, 218, 246, 274, 298): `TrySaveAttachmentAsync_WhenSaveSucceeds_DoesNotPromptOrClearReadOnly`, `TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYes_ClearsRetriesAndReleasesAnswer`, `TrySaveAttachmentAsync_WhenDeniedAndAnswerIsYesToAll_RetriesAndKeepsAnswer`, `TrySaveAttachmentAsync_WhenYesToAllIsHeld_SecondCallRetriesWithoutPrompt`, `TrySaveAttachmentAsync_WhenDeniedAndAnswerIsNo_ReturnsFalseAndReleasesAnswer`, `TrySaveAttachmentAsync_WhenNoToAllIsHeld_SecondCallReturnsFalseWithoutPrompt`, `TrySaveAttachmentAsync_WhenPromptIsCancelled_RethrowsUnauthorizedAccessException`, `TrySaveAttachmentAsync_WhenClearThrowsAfterYes_ReturnsFalseAndReleasesAnswer`, `TrySaveAttachmentAsync_WhenClearThrowsAfterYesToAll_ReturnsFalseAndKeepsAnswer`, `TrySaveAttachmentAsync_WhenSaveThrowsOtherException_PropagatesWithoutPrompt`, `TrySaveAttachmentAsync_WhenRetryIsDeniedAndSecondAnswerIsNo_ReturnsFalse`. `NAMES-T12` (12) adds `TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear`.
- `NAMES-TSC-P1` (5): `SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No]`, `SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [NoToAll]`, `SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes]`, `SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [YesToAll]`, `SaveCase_WhenAnswerIsEmpty_DoesNotSave`.
- `NAMES-TSC-FINAL` (12): `NAMES-TSC-P1` plus `SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [Yes]`, `SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [YesToAll]`, `SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePathAndReleasesAnswer`, `SaveCaseAsync_WhenAnswerIsNoToAllAndAltNameAnswerIsYesToAll_KeepsAnswerAcrossCalls`, `SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsNoToAll_DoesNotSaveAndKeepsAnswer`, `SaveCaseAsync_WhenAnswerIsNoAndAltNamePromptIsCancelled_DoesNotSaveAndStaysAskable`, `SaveCaseAsync_WhenAnswerIsEmpty_DoesNothing`.
- `NAMES-TAS-P1` (4): `Cleanup_Files_ResetsEveryPromptAnswerField [_responseSaveFile]`, `Cleanup_Files_ResetsEveryPromptAnswerField [_attachmentsOverwrite]`, `Cleanup_Files_ResetsEveryPromptAnswerField [_attachmentsAltName]`, `Cleanup_Files_ResetsEveryPromptAnswerField [_picturesOverwrite]`.
- `NAMES-TAS-FINAL` (11): `SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting`, `SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly`, `SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly`, `SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave`, `SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain`, `SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath`, `SaveAttachment_WhenFileDoesNotExist_SavesDirectly`, `SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer`, `SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer`, `RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths`, `Cleanup_Files_ResetsEveryPromptSession`.
- `NAMES-TUL` (2): `WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader`, `WriteCSV_WhenFileExists_DoesNotWrite`.
- `NAMES-TEF` (3): `MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates`, `MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce`, `MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState`.
- `NAMES-EFC-ARCHIVE` (11, QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs lines 48, 71, 94, 119, 144, 175, 196, 226, 251, 271, 293): `MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing`, `MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing`, `OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns`, `OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns`, `ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress`, `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`, `MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot`, `MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot`, `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates`, `OpenOlFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot`, `OpenFsFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot`.

Expected failing sets and the substring each `MESSAGE` must contain (Expect-fail wrong-reason branch of Execution Conventions):

| Run | Failing rows (exactly) | Passing rows | `MESSAGE` substring |
| --- | --- | --- | --- |
| P1-T6 (L1) | the four rows of `SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath` and `SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath` | `SaveCase_WhenAnswerIsEmpty_DoesNotSave` | `but was 0 times` (the mocked `SaveAsFile` was never called) |
| P1-T7 (L3 phase one) | `Cleanup_Files_ResetsEveryPromptAnswerField [_attachmentsAltName]` | the other three rows | `but found` and `YesToAll` |
| P2-T6 (L4) | both `NAMES-TUL` tests | none | first test `differs at index 0` (the queried path is file-then-folder); second test `NullReferenceException` or `but found 1` (the inverted condition enters the body) |
| P3-T3 (L2) | `TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear` | the eleven `NAMES-T` | `InvalidOperationException` (the tripwire sentinel replaced the expected access exception) |
| P4-T9 (re-rooting) | `RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths` | the other ten `NAMES-TAS-FINAL` | `GetDirectoryName(helper.FilePathSaveAlt)`, `origin"` and `destination"`, all three (the alternate path's directory still names the origin folder where the destination was expected; FluentAssertions prints a long string difference as a window after an ellipsis around the first differing index, so the full sandbox literal is never printed and is not asserted; revision 1.8) |
| P4-T14 (mutation control) | `Cleanup_Files_ResetsEveryPromptSession` | the other ten `NAMES-TAS-FINAL` | `but found 3` (three reset targets against four session fields) |
| P5-T7 (EfcDataModel) | `MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates` | the other two `NAMES-TEF` | `but found 0` (no reset before the exception propagated) |

## Census Expectations (CMD-CENSUS tokens, whitespace-stripped, case-sensitive)

Columns name plan states. A dash means the token is not asserted in that state. Every count is an acceptance value where a task says so.

### TOKENS-A (file A; states BASE = cited tree, P1 = after P1-T9, EXTRACT = after P4-T4 as executed from the revision-1.5 listing, SEAMED = after the P4-T7 rewrite from the revision-1.6 listing (PD-14), FINAL = after P4-T10)

| Token | BASE | P1 | EXTRACT | SEAMED | FINAL |
| --- | --- | --- | --- | --- | --- |
| `[ExcludeFromCodeCoverage]` | 10 | 9 | 3 | 3 | 3 |
| `caseYesNoToAllResponse.NoToAll:` | 0 | 1 | 1 | 1 | 1 |
| `caseYesNoToAllResponse.No:` | 0 | 1 | 1 | 1 | 1 |
| `caseYesNoToAllResponse.Yes:` | 0 | 1 | 1 | 1 | 1 |
| `caseYesNoToAllResponse.YesToAll:` | 0 | 1 | 1 | 1 | 1 |
| `\|YesNoToAllResponse.` (a pipe character directly before the enum name; only the two bitwise-or labels contain it) | 2 | 0 | 0 | 0 | 0 |
| `HasFlag` | 0 | 0 | 0 | 0 | 0 |
| `_attachmentsAltName=YesNoToAllResponse.Empty;` (the field initializer at A line 27 and the reset at line 275 at BASE; the `Cleanup_Files` reset added by E-A-L3 makes 3 at P1) | 2 | 3 | 0 | 0 | 0 |
| `YesNoToAllResponse_` | 4 | 4 | 0 | 0 | 0 |
| `YesNoToAll.ShowDialog(` | 6 | 6 | 0 | 0 | 0 |
| `new(YesNoToAll.ShowDialog)` | 0 | 0 | 3 | 3 | 3 |
| `AllPromptSessions` | 0 | 0 | 2 | 2 | 2 |
| `RedirectSaveFolder(` | 0 | 0 | 2 | 2 | 2 |
| `FolderPathSave=destinationPath;` | 1 | 1 | 1 | 1 | 1 |
| `FilePathHelperSaveAlt.FolderPath=destinationPath;` | 0 | 0 | 0 | 0 | 1 |
| `IsPicture` | 1 | 1 | 0 | 0 | 0 |
| `_responseSaveFile` | 2 | 2 | 0 | 0 | 0 |
| `Func<Attachment,string,Task<bool>>trySave` (the revision-1.5 seam parameter; 2 as P4-T4 wrote it, 0 from the P4-T7 rewrite onward) | 0 | 0 | 2 | 0 | 0 |
| `TrySaveAttachmentDelegatetrySave` (the two seam parameters of the asynchronous core and `SaveCaseAsync`, PD-14) | 0 | 0 | 0 | 2 | 2 |
| `delegateTask<bool>TrySaveAttachmentDelegate(` (the nested delegate declaration, PD-14) | 0 | 0 | 0 | 1 | 1 |
| `Func<string,bool>fileExists` | 0 | 0 | 2 | 2 | 2 |
| `File.Exists` | 2 | 2 | 2 | 2 | 2 |
| `TrySaveAttachmentAsync` | 3 | 3 | 1 | 1 | 1 |
| `SaveCaseAsync(` | 3 | 3 | 2 | 2 | 2 |
| `usingSystem.Diagnostics;` | 1 | 1 | 0 | 0 | 0 |
| `usingDeedle;` | 1 | 1 | 0 | 0 | 0 |
| `usingSDILReader;` | 1 | 1 | 0 | 0 | 0 |
| `usingOutlook=` | 1 | 1 | 0 | 0 | 0 |
| `usingUtilitiesCS;` | 1 | 1 | 0 | 0 | 0 |
| `#nullableenable` | 1 | 1 | 1 | 1 | 1 |

### TOKENS-T (file T; states BASE, FINAL = after P3-T4)

| Token | BASE | FINAL |
| --- | --- | --- |
| `[ExcludeFromCodeCoverage]` | 2 | 2 |
| `Debug.WriteLine(` | 3 | 0 |
| `catch(System.Exception){throw;}` | 1 | 0 |
| `catch(` | 3 | 2 |
| `catch(System.UnauthorizedAccessExceptione)` | 1 | 1 |
| `catch(System.Exceptioninner)` | 1 | 1 |
| `throw;` | 2 | 2 |
| `TrySaveAttachmentCoreAsync(` | 0 | 3 |
| `privatestaticasyncTask<bool>TrySaveAttachmentCoreAsync(` | 0 | 1 |
| `internalstaticasyncTask<bool>TrySaveAttachmentAsync(` | 1 | 0 |
| `internalstaticTask<bool>TrySaveAttachmentAsync(` | 2 | 3 |
| `boolisRetryAfterClear` | 0 | 1 |
| `isRetryAfterClear:false` | 0 | 1 |
| `isRetryAfterClear:true` | 0 | 1 |
| `isRetryAfterClear&&removeReadOnlyPrompt.Response==YesNoToAllResponse.YesToAll` | 0 | 1 |
| `logger.Warn(` | 0 | 2 |
| `logger.Error(` | 0 | 2 |
| `createDirectory(Path.GetDirectoryName(filePathSave));` | 1 | 1 |
| `System.IO.Directory.CreateDirectory(path)` | 1 | 1 |
| `removeReadOnlyPrompt.ReleaseSingleAnswer();` | 2 | 2 |
| `RemoveReadOnlyPrompt=new(` | 1 | 1 |
| `usingSystem.Diagnostics;` | 1 | 0 |
| `usingDeedle;` | 1 | 0 |
| `usingSDILReader;` | 1 | 0 |
| `usingOutlook=` | 1 | 0 |
| `usingUtilitiesCS;` | 1 | 0 |
| `#nullableenable` | 1 | 1 |

### TOKENS-U (file U; states BASE, SEAM = after P2-T3, FINAL = after P2-T7)

| Token | BASE | SEAM | FINAL |
| --- | --- | --- | --- |
| `[ExcludeFromCodeCoverage]` | 6 | 6 | 4 |
| `Path.Combine(strFileName,strFileLocation)` | 1 | 1 | 0 |
| `fileExists(Path.Combine(strFileLocation,strFileName))` | 0 | 0 | 1 |
| `SanitizeArray(` | 2 | 2 | 0 |
| `MovedMailsHeader` | 0 | 0 | 2 |
| `string.Join("\t",MovedMailsHeader)` | 0 | 0 | 1 |
| `"Triage"` | 1 | 1 | 1 |
| `"FlaggedAsTask"` | 1 | 1 | 1 |
| `string[14,2]` | 1 | 1 | 0 |
| `Func<string,bool>fileExists` | 0 | 1 | 1 |
| `Action<string,string[],string>writeTextFile` | 0 | 1 | 1 |
| `writeTextFile(` | 0 | 1 | 1 |
| `File.Exists` | 1 | 1 | 1 |
| `FileIO2.WriteTextFile` | 1 | 1 | 1 |
| `publicstaticvoidWriteCSV_StartNewFileIfDoesNotExist(` | 1 | 1 | 1 |
| `internalstaticvoidWriteCSV_StartNewFileIfDoesNotExist(` | 0 | 1 | 1 |
| `Debug.WriteLine(` | 1 | 1 | 0 |
| `usingSystem.Diagnostics;` | 1 | 1 | 0 |
| `usingSystem.Collections.Generic;` | 1 | 1 | 0 |
| `usingDeedle;` | 1 | 1 | 0 |
| `usingSDILReader;` | 1 | 1 | 0 |
| `usingOutlook=` | 1 | 1 | 0 |
| `usingUtilitiesCS;` | 1 | 1 | 0 |
| `usingUtilitiesCS.EmailIntelligence;` | 1 | 1 | 0 |
| `usingUtilitiesCS.OutlookExtensions;` | 1 | 1 | 0 |
| `#nullableenable` | 1 | 1 | 1 |

### TOKENS-S and TOKENS-M (files S and M; states BASE, FINAL = after P5-T1 and P5-T2)

| Token | S BASE | S FINAL | M BASE | M FINAL |
| --- | --- | --- | --- | --- |
| `[ExcludeFromCodeCoverage]` | 4 | 4 | 5 | 5 |
| `usingDeedle;` | 1 | 0 | 1 | 0 |
| `usingSDILReader;` | 1 | 0 | 1 | 0 |
| `usingOutlook=` | 1 | 0 | 1 | 0 |
| `usingUtilitiesCS;` | 1 | 0 | 1 | 0 |
| `usingSystem.Diagnostics;` | 1 | 0 | 1 | 0 |
| `usingSystem.Text.RegularExpressions;` | 1 | 0 | 1 | 0 |
| `usingUtilitiesCS.EmailIntelligence;` | 1 | 0 | 1 | 0 |
| `usingUtilitiesCS.ReusableTypeClasses` | 1 | 0 | 1 | 0 |
| `usingSystem.Windows.Forms;` | 1 | 0 | 1 | 1 |
| `usingUtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;` | 1 | 1 | 1 | 0 |
| `usingUtilitiesCS.OutlookExtensions;` | 1 | 1 | 1 | 1 |
| `usingSystem;` | 1 | 1 | 1 | 1 |
| `#nullableenable` | 1 | 1 | 1 | 1 |

### TOKENS-E (file E; states BASE, SEAM = after P5-T6, FINAL = after P5-T8)

| Token | BASE | SEAM | FINAL |
| --- | --- | --- | --- |
| `SortEmail.Cleanup_Files();` | 1 | 1 | 1 |
| `ResetFilerPromptState();` | 0 | 1 | 1 |
| `protectedinternalvirtualvoidResetFilerPromptState()` | 0 | 1 | 1 |
| `varresult=awaitInvokeFilerAsync(config,mailHelpers);` | 1 | 1 | 0 |
| `result=awaitInvokeFilerAsync(config,mailHelpers);` | 1 | 1 | 1 |
| `boolresult;` | 0 | 0 | 1 |
| `finally{` | 0 | 0 | 1 |
| `returnresult;` | 1 | 1 | 1 |

### TOKENS-TST1 (file TST1; states BASE, MID = after P2-T8, FINAL = after P4-T6)

| Token | BASE | MID | FINAL |
| --- | --- | --- | --- |
| `[TestMethod]` | 15 | 14 | 12 |
| `[DataTestMethod]` | 0 | 0 | 2 |
| `[DataRow(` | 0 | 0 | 6 |
| `DisplayName=` | 0 | 0 | 6 |
| `SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows` | 1 | 0 | 0 |
| `"SanitizeArray"` | 1 | 0 | 0 |
| `"SanitizeArrayLineTSV"` | 1 | 1 | 1 |
| `saveAttachments:false,savePictures:true` | 1 | 1 | 0 |
| `saveAttachments:saveAttachments,savePictures:savePictures` | 0 | 0 | 2 |
| `"photo.jpg,report.pdf"` | 0 | 0 | 2 |
| `TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile` | 1 | 1 | 1 |
| `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave` | 1 | 1 | 1 |
| `C:\Sortemail945Sandbox` | 1 | 1 | 1 |

### TOKENS-TST2 (file TST2; states BASE, FINAL = after P3-T1)

| Token | BASE | FINAL |
| --- | --- | --- |
| `[TestMethod]` | 11 | 12 |
| `CreateDirectoryLimit` | 0 | 3 |
| `TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear` | 0 | 1 |
| `"retryboundexceeded"` | 0 | 1 |
| `.Throws(denied)` | 0 | 1 |
| `BeSameAs(denied)` | 0 | 1 |
| `SetupSequence` | 11 | 11 |
| `Times.Exactly(2)` (TST2 lines 77, 103, 186, 316 at BASE; one more in E-TST2-T12) | 4 | 5 |
| `newSeams(` | 11 | 12 |
| `DoNotParallelize` | 0 | 0 |
| `Thread.Sleep` | 0 | 0 |
| `Task.Delay` | 0 | 0 |
| `Timeout` | 0 | 0 |
| `MemoryAppender` | 0 | 0 |

### TOKENS-TSC (file TSC; states P1 = Listing L-TSC-P1, FINAL = Listing L-TSC-FINAL)

| Token | P1 | FINAL |
| --- | --- | --- |
| `[TestMethod]` | 1 | 6 |
| `[DataTestMethod]` | 2 | 3 |
| `[DataRow(` | 4 | 6 |
| `DisplayName=` | 4 | 6 |
| `SortEmail.SaveCase(` | 3 | 3 |
| `SortEmail.SaveCaseAsync(` | 0 | 9 |
| `newScriptedPrompt(` | 0 | 6 |
| `RecordingSave(saves)` | 0 | 9 |
| `SortEmail.TrySaveAttachmentDelegate` (the `RecordingSave` return type, PD-14; 0 in the file as P4-T1 wrote it from the revision-1.5 listing, 1 from the P4-T7 rewrite onward) | 0 | 1 |
| `Func<Attachment,string,Task<bool>>` (1 in the file as P4-T1 wrote it, 0 from the P4-T7 rewrite onward) | 0 | 0 |
| `newYesNoToAllPromptSession(Prompt)` | 0 | 1 |
| `Times.Once` | 2 | 2 |
| `Times.Never` | 3 | 3 |
| `C:\Sortemail959Sandbox` | 2 | 2 |
| `DoNotParallelize` | 0 | 0 |
| `Thread.Sleep` | 0 | 0 |
| `Task.Delay` | 0 | 0 |
| `Timeout` | 0 | 0 |
| `Directory.CreateDirectory` | 0 | 0 |
| `File.` | 0 | 0 |
| `MemoryAppender` | 0 | 0 |

### TOKENS-TAS (file TAS; states P1 = Listing L-TAS-P1, EXTRACT and FINAL = Listing L-TAS-FINAL)

| Token | P1 | FINAL |
| --- | --- | --- |
| `[TestMethod]` | 0 | 11 |
| `[DataTestMethod]` | 1 | 0 |
| `[DataRow(` | 4 | 0 |
| `Cleanup_Files_ResetsEveryPromptAnswerField` (the declaration plus the four `DisplayName` strings, PD-5) | 5 | 0 |
| `Cleanup_Files_ResetsEveryPromptSession` | 0 | 1 |
| `SortEmail.Cleanup_Files();` | 1 | 0 |
| `field.SetValue(null,YesNoToAllResponse.YesToAll);` | 1 | 0 |
| `SetValue(` | 1 | 0 |
| `SortEmail.SaveAttachmentAsync(` | 0 | 7 |
| `SortEmail.SaveAttachment(` | 0 | 3 |
| `SortEmail.RedirectSaveFolder(` | 0 | 1 |
| `SortEmail.TrySaveAttachmentDelegate` (the `RecordingSave` return type, PD-14; 0 in the file as P4-T2 wrote it from the revision-1.5 listing, 1 from the P4-T7 rewrite onward) | 0 | 1 |
| `Func<Attachment,string,Task<bool>>` (1 in the file as P4-T2 wrote it, 0 from the P4-T7 rewrite onward) | 0 | 0 |
| `"AllPromptSessions"` | 0 | 1 |
| `typeof(YesNoToAllPromptSession)` | 0 | 1 |
| `HaveCount(4)` | 0 | 2 |
| `OnlyHaveUniqueItems()` | 0 | 1 |
| `newYesNoToAllPromptSession(Prompt)` | 0 | 1 |
| `C:\Sortemail959Sandbox` | 0 | 3 |
| `DoNotParallelize` | 0 | 0 |
| `Thread.Sleep` | 0 | 0 |
| `Task.Delay` | 0 | 0 |
| `Timeout` | 0 | 0 |
| `Directory.CreateDirectory` | 0 | 0 |
| `File.` | 0 | 0 |
| `MemoryAppender` | 0 | 0 |

### TOKENS-TAS and TOKENS-TSC after Phase 7 (revision 2.0; state SS4 = the tree after E-TAS-SS4 and E-TSC-USING; the P7-T4 census uses the full TOKENS-TAS and TOKENS-TSC lists plus the two tokens named here)

TOKENS-TAS SS4 column: `[TestMethod]` 12; `SortEmail.SaveAttachment(` 4; the new token `SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly` 1 (0 at FINAL); every other row unchanged from FINAL (`SortEmail.SaveAttachmentAsync(` 7, `SortEmail.RedirectSaveFolder(` 1, `Cleanup_Files_ResetsEveryPromptSession` 1, `Cleanup_Files_ResetsEveryPromptAnswerField` 0, `SortEmail.Cleanup_Files();` 0, `SetValue(` 0, `[DataTestMethod]` 0, `[DataRow(` 0, `SortEmail.TrySaveAttachmentDelegate` 1, `Func<Attachment,string,Task<bool>>` 0, `"AllPromptSessions"` 1, `typeof(YesNoToAllPromptSession)` 1, `HaveCount(4)` 2, `OnlyHaveUniqueItems()` 1, `newYesNoToAllPromptSession(Prompt)` 1, `C:\Sortemail959Sandbox` 3, and `DoNotParallelize`, `Thread.Sleep`, `Task.Delay`, `Timeout`, `Directory.CreateDirectory`, `File.`, `MemoryAppender` 0). The whitespace-stripped SS4 text contains none of those tokens except the three counted above (`SaveAsFile to the primary path` strips to `SaveAsFiletotheprimarypath`, which does not contain `File.`). TOKENS-TSC SS4 column: every FINAL value unchanged, plus the token `usingSystem;` at 1 in the FINAL state and 0 at SS4 (the whitespace-stripped `usingSystem;` is not a substring of `usingSystem.Collections.Generic;` because a period, not a semicolon, follows `System` there); over TAS the same token stays 1 (TAS uses `DateTime` and `Func`). Line-count predictions at SS4: TAS 469 (437 plus 32) and TSC 342 (343 minus 1), both under the ceiling; `MAX-LINES:` stays 488 (TST1).

### TOKENS-TUL (file TUL; Listing L-TUL)

`[TestMethod]` 2; `SortEmail.WriteCSV_StartNewFileIfDoesNotExist(` 2; `Path.Combine(LogFolder,LogFileName)` 1; `HaveCount(13)` 1; `"MovedMails.txt"` 1; `Triage\tFolderName\tSent_On` 1; `C:\Sortemail959Sandbox` 1; `DoNotParallelize` 0; `Thread.Sleep` 0; `Task.Delay` 0; `Timeout` 0; `Directory.CreateDirectory` 0; `File.` 0; `MemoryAppender` 0.

### TOKENS-TEF (file TEF; Listing L-TEF)

`[TestMethod]` 3; `MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates` 1; `MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce` 1; `MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState` 1; `overrideTask<bool>InvokeFilerAsync(` 1; `overridevoidResetFilerPromptState()` 1; `ResetCalls.Should().Be(1)` 2; `ResetCalls.Should().Be(0)` 1; `Task.FromException<bool>(` 1; `SpecialFoldersWithoutOneDrive()` 2; `DoNotParallelize` 0; `Thread.Sleep` 0; `Task.Delay` 0; `Timeout` 0; `Directory.CreateDirectory` 0; `File.` 0; `MemoryAppender` 0.

### Line-count expectations (CMD-LINES, `@(Get-Content).Count`, each at most 499; predictions, not gates except the ceiling)

A about 328 (315 lines in the P4-T4 state after the first P4-T7 format, plus the twelve declaration lines and one blank line of the `TrySaveAttachmentDelegate` block, PD-14), T about 205, U about 170, S 268, M 379, E about 482, TST1 484 before the P4-T7 format (457 at baseline, minus 25 by E-TST1-DEL-SANITIZE, plus 26 by each of E-TST1-ROWS-SYNC and E-TST1-ROWS-ASYNC) and about 485 to 490 after it (488 as P6-T10 recorded; unchanged by the two-line documentation-comment replacement of P6-T13, revision 1.9, which P6-T14 re-measures), TST2 about 420, TSC about 340, TAS about 445, TUL about 80, TEF about 190. TST1, E and TAS are the three files closest to the ceiling and are quoted individually under `AC23-CLOSEST:` in P6-T10.

## Command Reference

Each block is a payload in the sense of the command-channel convention: one complete statement per line (a braced block on one line is one statement), no line continuation, no single-quote character, no comment line. The executor joins the lines with `; ` inside `pwsh -NoProfile -Command '...'`. The shapes of CMD-CENSUS, CMD-SCOPED-FORMAT, CMD-BUILD-TEST, CMD-REBUILD, CMD-VSTEST, CMD-COVERAGE-DIRECT, CMD-COVERAGE-POST, CMD-FORMAT-REPO, CMD-CHECK-REPO and CMD-FOOTPRINT are those whose success-case output was observed by the #956 run on this workstation (docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/baseline/coverage-baseline.md, test-run-baseline.md, p0-t4-channel-and-toolchain.2026-10-01T20-38.md and qa-gates/coverage-comparison.md); every printed label asserted below is a label those payload shapes print on a successful run.

Filters (each is the complete `/TestCaseFilter:` argument, quoted). `FILTER-TST1` is `"/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_Tests."` (the trailing dot keeps the other four classes out); `FILTER-TRYSAVE` is `"/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_TrySaveAttachment_Tests"`; `FILTER-SAVECASE` is `"/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_SaveCase_Tests"`; `FILTER-ATTSAVE` is `"/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_AttachmentSaving_Tests"`; `FILTER-UNDO` is `"/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_UndoAndMoveLog_Tests"`; `FILTER-SORTEMAIL` is `"/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_"`; `FILTER-EFC-ARCHIVE` is `"/TestCaseFilter:FullyQualifiedName~Controllers.EfcDataModelArchiveRootTests"`; `FILTER-EFC-CLEANUP` is `"/TestCaseFilter:FullyQualifiedName~Controllers.EfcDataModelFilerCleanupTests"`; `FILTER-STALL` is `"/TestCaseFilter:FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests"`. Expected totals per state are in the Test Totals table. `EXCLUSION` (fixed by P0-T9) is empty under `STALL-PROBE: CLEAR` (P0-T9 records this as `EXCLUSION: NONE`; the substituted text is then the empty string, so `$filter` is `TestCategory!=LiveOutlook` alone) and is `&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests` under `REPRODUCES`.

Assemblies. `ASSEMBLY-UCT` is `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`; `ASSEMBLY-QFT` is `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`.

Path sets (backslash, repository-relative, each a quoted PowerShell string; `PATHS` substitutions join them with `, `). `PATHS-A` `"UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs"`; `PATHS-T` `"UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs"`; `PATHS-U` `"UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs"`; `PATHS-S` `"UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs"`; `PATHS-M` `"UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs"`; `PATHS-L` `"UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs"`; `PATHS-E` `"QuickFiler\Controllers\EfcDataModel.cs"`; `PATHS-TD` `"ToDoModel\Email Utilities\SortItemsToExistingFolder.cs"`; `PATHS-TST1` `"UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs"`; `PATHS-TST2` `"UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs"`; `PATHS-TSC` `"UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs"`; `PATHS-TAS` `"UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs"`; `PATHS-TUL` `"UtilitiesCS.Test\EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs"`; `PATHS-TEF` `"QuickFiler.Test\Controllers\EfcDataModelFilerCleanupTests.cs"`; `PATHS-FIVE` is A, T, U, S, M; `PATHS-SIX` is `PATHS-FIVE` plus L; `PATHS-TESTS6` is TST1, TST2, TSC, TAS, TUL, TEF; `PATHS-CSHARP-FINAL` is `PATHS-FIVE`, E and `PATHS-TESTS6` (twelve files). Forward-slash forms of the same paths are used wherever a git pathspec is written.

`PATHS-CITED` (P0-T3 `CITED-TREE-EXIT:` only) is the exact list of every repository path outside FEATURE/ and outside .claude/ that this plan cites by line, together with every path it cites by content other than .gitignore (explained below), written as forward-slash git pathspecs separated by spaces, a path containing a space enclosed in double quotes: `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs UtilitiesCS/UtilitiesCS.csproj UtilitiesCS/packages.config UtilitiesCS/Properties/AssemblyInfo.cs UtilitiesCS/Dialogs/YesNoToAll.cs UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs UtilitiesCS/OutlookObjects/Attachment/AttachmentHelper.cs UtilitiesCS/OutlookObjects/Attachment/AttachmentSerializable.cs UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs "UtilitiesCS/To Depricate/FileIO2.cs" UtilitiesCS.Test/UtilitiesCS.Test.csproj UtilitiesCS.Test/packages.config UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs UtilitiesCS.Test/EmailIntelligence/OSBrowser_Tests.cs UtilitiesCS.Test/HelperClasses/ShellUtilities_Tests.cs UtilitiesCS.Test/HelperClasses/ShellUtilitiesStatic_Tests.cs UtilitiesCS.Test/HelperClasses/SysImageListHelperTests.cs UtilitiesCS.Test/Threading/CurrentStoreContextTests.cs QuickFiler/packages.config QuickFiler/Properties/AssemblyInfo.cs QuickFiler/Controllers/EfcDataModel.cs QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs QuickFiler/Controllers/EfcFormController.Actions.cs QuickFiler/Controllers/EfcFormController.EventHandlers.cs QuickFiler/Controllers/QfcItemController.MailActions.cs QuickFiler/Legacy/QfcController.cs QuickFiler.Test/QuickFiler.Test.csproj QuickFiler.Test/packages.config QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs ToDoModel/ToDoModel.csproj "ToDoModel/Email Utilities/SortItemsToExistingFolder.cs" ToDoModel.Test/ToDoModel.Test.csproj "ToDoModel.Test/Email Utilities" TaskMaster/AppGlobals/AppOlObjects.cs scripts/vscode coverage.config global.json dotnet-tools.json .gitattributes .editorconfig .csharpierignore docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956 docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/regression-testing/pass-after-shape-b.2026-09-16T23-27.md`. The four files this plan creates (TSC, TAS, TUL, TEF) are listed so that a pre-existing file of the same name is detected; a pathspec that matches nothing in either tree is not an error for a two-tree `git diff`. FEATURE/ is excluded because the orchestrator's preparation commits change it (PD-9 Clause A); .claude/ is excluded because P0-T1 reads the policy files at run time and no acceptance condition cites a line of them. `.gitignore` is excluded because this plan cites its entries by content only; their effect is gated by the P6-T12 footprint (`OUTSIDE-WRITE-SET:`, `RAW-DOC-PATHS:`) and P6-T16 (`RAW-DOCUMENT-FILES:`), and the preflight round 2 report states that origin/main changed unrelated lines of it after 94287369 (#961); the planner had no git channel in the revision 1.2 pass to confirm that diff, and the content-only citation makes it immaterial to `CITED-TREE-EXIT:`.

**CMD-CENSUS** (`PATHS` and `TOKENS` substituted; prints `TOKEN <token> @ <path> = <n>` per file, `TOKEN <token> @ TOTAL = <n>`, then `LINES` and `SHA256` per file):

    Set-Location -LiteralPath "WORKTREE"
    $paths = @(PATHS)
    $tokens = @(TOKENS)
    $content = @{}
    foreach ($p in $paths) { $content[$p] = [regex]::Replace((Get-Content -LiteralPath $p -Raw -Encoding UTF8), "\s+", "") }
    foreach ($t in $tokens) { $total = 0; foreach ($p in $paths) { $n = [regex]::Matches($content[$p], [regex]::Escape($t)).Count; $total += $n; Write-Output ("TOKEN " + $t + " @ " + $p + " = " + $n) }; Write-Output ("TOKEN " + $t + " @ TOTAL = " + $total) }
    foreach ($p in $paths) { Write-Output ("LINES " + $p + " = " + @(Get-Content -LiteralPath $p -Encoding UTF8).Count); Write-Output ("SHA256 " + $p + " = " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash) }

A `TOKENS` list is the first column of the named TOKENS table, each token as a quoted string; a token containing a double quote is written with `[char]34` concatenation; a token containing a backslash is written as is (a backslash is not an escape character in a PowerShell double-quoted string).

**CMD-HASH** (`PATH` substituted): `Set-Location -LiteralPath "WORKTREE"` then `Write-Output ("SHA256: " + (Get-FileHash -Algorithm SHA256 -LiteralPath "PATH").Hash)`.

**CMD-LINES** (`PATHS` substituted; prints one `LINES` row per existing path, `MISSING` for an absent one, and the maximum):

    Set-Location -LiteralPath "WORKTREE"
    $max = 0
    foreach ($p in @(PATHS)) { if (Test-Path -LiteralPath $p) { $n = @(Get-Content -LiteralPath $p -Encoding UTF8).Count; if ($n -gt $max) { $max = $n }; Write-Output ("LINES " + $p + " = " + $n) } else { Write-Output ("MISSING " + $p) } }
    Write-Output ("MAX-LINES: " + $max)

**CMD-DELETE** (PD-3; `PATH` substituted in forward-slash form; one of the pwsh writes to a tracked source path enumerated in PD-2):

    Set-Location -LiteralPath "WORKTREE"
    Write-Output ("EXISTS-BEFORE: " + (Test-Path -LiteralPath "PATH"))
    if (Test-Path -LiteralPath "PATH") { Remove-Item -LiteralPath "PATH" -Force }
    Write-Output ("EXISTS-AFTER: " + (Test-Path -LiteralPath "PATH"))
    Write-Output ("PORCELAIN: " + (@(git status --porcelain -- "PATH") -join " | "))

**CMD-CSPROJ** (exact-line positions of the Compile entries in the three project files; prints `UCS`, `UCT` and `QFT` rows with `COUNT=` and `LINE=`):

    Set-Location -LiteralPath "WORKTREE"
    $q = [string][char]34
    $u = @(Get-Content -LiteralPath "UtilitiesCS\UtilitiesCS.csproj" -Encoding UTF8)
    $t = @(Get-Content -LiteralPath "UtilitiesCS.Test\UtilitiesCS.Test.csproj" -Encoding UTF8)
    $f = @(Get-Content -LiteralPath "QuickFiler.Test\QuickFiler.Test.csproj" -Encoding UTF8)
    foreach ($inc in @("EmailIntelligence\EmailParsingSorting\MovedMailInfo.cs", "EmailIntelligence\EmailParsingSorting\SortEmail.cs", "EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs", "EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs", "EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs", "EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs", "EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs", "OutlookObjects\Folder\FolderPredictor.cs")) { $want = "    <Compile Include=" + $q + $inc + $q + " />"; $hits = @(for ($i = 0; $i -lt $u.Count; $i++) { if ($u[$i] -ceq $want) { $i + 1 } }); Write-Output ("UCS " + $inc + " COUNT=" + $hits.Count + " LINE=" + ($hits -join ",")) }
    foreach ($inc in @("EmailIntelligence\SortEmail_Tests.cs", "EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs", "EmailIntelligence\SortEmail_SaveCase_Tests.cs", "EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs", "EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs", "EmailIntelligence\FilterOlFoldersController_Tests.cs")) { $want = "    <Compile Include=" + $q + $inc + $q + " />"; $hits = @(for ($i = 0; $i -lt $t.Count; $i++) { if ($t[$i] -ceq $want) { $i + 1 } }); Write-Output ("UCT " + $inc + " COUNT=" + $hits.Count + " LINE=" + ($hits -join ",")) }
    foreach ($inc in @("Controllers\EfcDataModelArchiveRootTests.cs", "Controllers\EfcDataModelFilerCleanupTests.cs", "Controllers\EfcDataModelIssue792CarryTests.cs")) { $want = "    <Compile Include=" + $q + $inc + $q + " />"; $hits = @(for ($i = 0; $i -lt $f.Count; $i++) { if ($f[$i] -ceq $want) { $i + 1 } }); Write-Output ("QFT " + $inc + " COUNT=" + $hits.Count + " LINE=" + ($hits -join ",")) }

Expected positions (COUNT=1 and the line shown; COUNT=0 for an absent entry):

| Entry | BASE | after P1-T3 | after P2-T2 | after P4-T5 | after P5-T5 (final) |
| --- | --- | --- | --- | --- | --- |
| UCS `...\MovedMailInfo.cs` | 817 | 817 | 817 | 817 | 817 |
| UCS `...\SortEmail.cs` | 818 | 818 | 818 | 818 | 818 |
| UCS `...\SortEmail.AttachmentSaving.cs` | 819 | 819 | 819 | 819 | 819 |
| UCS `...\SortEmail.LegacyAttachmentSaving.cs` | 820 | 820 | 820 | absent | absent |
| UCS `...\SortEmail.MailItemSort.cs` | 821 | 821 | 821 | 820 | 820 |
| UCS `...\SortEmail.TrySaveAttachment.cs` | 822 | 822 | 822 | 821 | 821 |
| UCS `...\SortEmail.UndoAndMoveLog.cs` | 823 | 823 | 823 | 822 | 822 |
| UCS `OutlookObjects\Folder\FolderPredictor.cs` | 824 | 824 | 824 | 823 | 823 |
| UCT `EmailIntelligence\SortEmail_Tests.cs` | 98 | 98 | 98 | 98 | 98 |
| UCT `EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs` | 99 | 99 | 99 | 99 | 99 |
| UCT `EmailIntelligence\SortEmail_SaveCase_Tests.cs` | absent | 100 | 100 | 100 | 100 |
| UCT `EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs` | absent | 101 | 101 | 101 | 101 |
| UCT `EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs` | absent | absent | 102 | 102 | 102 |
| UCT `EmailIntelligence\FilterOlFoldersController_Tests.cs` | 100 | 102 | 103 | 103 | 103 |
| QFT `Controllers\EfcDataModelArchiveRootTests.cs` | 127 | 127 | 127 | 127 | 127 |
| QFT `Controllers\EfcDataModelFilerCleanupTests.cs` | absent | absent | absent | absent | 128 |
| QFT `Controllers\EfcDataModelIssue792CarryTests.cs` | 128 | 128 | 128 | 128 | 129 |

**CMD-SCOPED-FORMAT** (`PATHS` and `TASKID` substituted; formats the named files and verifies them read-only):

    Set-Location -LiteralPath "WORKTREE"
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $paths = @(PATHS)
    foreach ($p in $paths) { Write-Output ("BEFORE " + $p + " = " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash) }
    $global:LASTEXITCODE = 0
    & dotnet tool run csharpier format @paths 2>&1 | Tee-Object -FilePath "coverage\logs\TASKID.csharpier-format.log"
    Write-Output ("FORMAT_EXIT_CODE: " + $LASTEXITCODE)
    foreach ($p in $paths) { Write-Output ("AFTER " + $p + " = " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash) }
    $global:LASTEXITCODE = 0
    & dotnet tool run csharpier check @paths 2>&1 | Tee-Object -FilePath "coverage\logs\TASKID.csharpier-check.log"
    Write-Output ("CHECK_EXIT_CODE: " + $LASTEXITCODE)

The success observation is the read-only check after the write (`CHECK_EXIT_CODE: 0`) together with the recorded BEFORE and AFTER hashes; the formatter's own summary line is a processed-file count printed on every run and is recorded but never gated.

**CMD-BUILD-TEST** (`PROJECT` is `UtilitiesCS.Test` or `QuickFiler.Test`; `TASKID` substituted; builds the test project and its references; classifies compiler errors by file and names the missing members; the Build target is used because this is not a gate, see MSBuild switches):

    Set-Location -LiteralPath "WORKTREE"
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
    $dll = "PROJECT\bin\Debug\PROJECT.dll"
    $before = if (Test-Path -LiteralPath $dll) { (Get-Item -LiteralPath $dll).LastWriteTimeUtc } else { [datetime]::MinValue }
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $global:LASTEXITCODE = 0
    & $msbuild "PROJECT\PROJECT.csproj" /t:Build /m /nodeReuse:false /p:Configuration=Debug /p:Platform=AnyCPU "/flp:LogFile=coverage\logs\TASKID.msbuild.log;Verbosity=normal"
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\TASKID.msbuild.log" -Raw -Encoding UTF8
    $errs = @(Get-Content -LiteralPath "coverage\logs\TASKID.msbuild.log" -Encoding UTF8 | Where-Object { $_ -like "*error CS*" })
    $apos = [string][char]39
    $newFiles = @("SortEmail_SaveCase_Tests.cs", "SortEmail_AttachmentSaving_Tests.cs", "SortEmail_UndoAndMoveLog_Tests.cs", "SortEmail_Tests.cs", "SortEmail_TrySaveAttachment_Tests.cs", "EfcDataModelFilerCleanupTests.cs")
    Write-Output ("CSC_OUT_LINES: " + [regex]::Matches($log, [regex]::Escape("/out:obj\Debug\PROJECT.dll")).Count)
    Write-Output ("ZERO_ERRORS_LINES: " + [regex]::Matches($log, [regex]::Escape(" 0 Error(s)")).Count)
    Write-Output ("ERROR_LINES: " + $errs.Count)
    Write-Output ("ERROR_LINES_TEST_FILES: " + @($errs | Where-Object { $l = $_; @($newFiles | Where-Object { $l -like ("*" + $_ + "*") }).Count -gt 0 }).Count)
    Write-Output ("ERROR_LINES_OTHER_FILES: " + @($errs | Where-Object { $l = $_; @($newFiles | Where-Object { $l -like ("*" + $_ + "*") }).Count -eq 0 }).Count)
    Write-Output ("MISSING_SAVEATTACHMENTASYNC_6: " + @($errs | Where-Object { $_ -like ("*" + $apos + "SaveAttachmentAsync" + $apos + " takes 6 arguments*") }).Count)
    Write-Output ("MISSING_SAVECASEASYNC_6: " + @($errs | Where-Object { $_ -like ("*" + $apos + "SaveCaseAsync" + $apos + " takes 6 arguments*") }).Count)
    Write-Output ("MISSING_SAVEATTACHMENT_4: " + @($errs | Where-Object { $_ -like ("*" + $apos + "SaveAttachment" + $apos + " takes 4 arguments*") }).Count)
    Write-Output ("MISSING_REDIRECTSAVEFOLDER: " + @($errs | Where-Object { $_ -like ("*" + $apos + "RedirectSaveFolder" + $apos + "*") }).Count)
    Write-Output ("ERROR_CODES: " + ((@($errs | ForEach-Object { [regex]::Match($_, "error (CS\d{4})").Groups[1].Value }) | Sort-Object -Unique) -join ","))
    Write-Output ("DLL_ADVANCED: " + ((Test-Path -LiteralPath $dll) -and ((Get-Item -LiteralPath $dll).LastWriteTimeUtc -gt $before)))

`Command:` records `msbuild PROJECT\PROJECT.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU` with the three notes. The `error CS` lines carry absolute file paths; only the counts and the error codes are transcribed.

**CMD-BUILD-PROD** (`PROJ` is a production project path such as `UtilitiesCS\UtilitiesCS.csproj`, `DLL` its output name such as `UtilitiesCS.dll`; `TASKID` substituted; rebuilds one production project with warnings as errors as an intermediate check):

    Set-Location -LiteralPath "WORKTREE"
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $global:LASTEXITCODE = 0
    & $msbuild "PROJ" /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\logs\TASKID.msbuild.log;Verbosity=normal"
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\TASKID.msbuild.log" -Raw -Encoding UTF8
    Write-Output ("PROD_CSC_OUT_LINES: " + [regex]::Matches($log, [regex]::Escape("/out:obj\Debug\DLL")).Count)
    Write-Output ("ZERO_ERRORS_LINES: " + [regex]::Matches($log, [regex]::Escape(" 0 Error(s)")).Count)
    Write-Output ("ERRORS: " + [regex]::Match($log, "(\d+) Error\(s\)").Groups[1].Value)
    Write-Output ("CS0246_LINES: " + [regex]::Matches($log, [regex]::Escape("CS0246")).Count)
    Write-Output ("CS0103_LINES: " + [regex]::Matches($log, [regex]::Escape("CS0103")).Count)
    Write-Output ("CS0104_LINES: " + [regex]::Matches($log, [regex]::Escape("CS0104")).Count)
    Write-Output ("CS1061_LINES: " + [regex]::Matches($log, [regex]::Escape("CS1061")).Count)

`Command:` records `msbuild PROJ /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true` with the notes `resolved through vswhere`, `plus /nodeReuse:false` and `plus a normal-verbosity file logger`. `ZERO_ERRORS_LINES` counts the literal with its leading space because `0 Error(s)` is a substring of `10 Error(s)`.

**CMD-REBUILD** (solution rebuild gate; `GATEARGS` is `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzer gate) or `/p:TreatWarningsAsErrors=true` (nullable gate); `TASKID` substituted):

    Set-Location -LiteralPath "WORKTREE"
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $global:LASTEXITCODE = 0
    & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" GATEARGS "/flp:LogFile=coverage\logs\TASKID.msbuild.log;Verbosity=normal"
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\TASKID.msbuild.log" -Raw -Encoding UTF8
    Write-Output ("SKIP_CORECOMPILE_LINES: " + [regex]::Matches($log, [regex]::Escape("Skipping target ""CoreCompile""")).Count)
    Write-Output ("UCS_TEST_CSC_OUT_LINES: " + [regex]::Matches($log, [regex]::Escape("/out:obj\Debug\UtilitiesCS.Test.dll")).Count)
    Write-Output ("UCS_CSC_OUT_LINES: " + [regex]::Matches($log, [regex]::Escape("/out:obj\Debug\UtilitiesCS.dll")).Count)
    Write-Output ("QF_CSC_OUT_LINES: " + [regex]::Matches($log, [regex]::Escape("/out:obj\Debug\QuickFiler.dll")).Count)
    Write-Output ("QFT_CSC_OUT_LINES: " + [regex]::Matches($log, [regex]::Escape("/out:obj\Debug\QuickFiler.Test.dll")).Count)
    Write-Output ("ZERO_ERRORS_LINES: " + [regex]::Matches($log, [regex]::Escape(" 0 Error(s)")).Count)
    Write-Output ("WARNINGS: " + [regex]::Match($log, "(\d+) Warning\(s\)").Groups[1].Value)
    Write-Output ("ERRORS: " + [regex]::Match($log, "(\d+) Error\(s\)").Groups[1].Value)
    Write-Output ("WRITESET_DIAGNOSTIC_LINES: " + @(Get-Content -LiteralPath "coverage\logs\TASKID.msbuild.log" -Encoding UTF8 | Where-Object { ($_ -like "*EmailParsingSorting\SortEmail*" -or $_ -like "*SortEmail_*Tests.cs*" -or $_ -like "*Controllers\EfcDataModel.cs*" -or $_ -like "*EfcDataModelFilerCleanupTests.cs*" -or $_ -like "*SortItemsToExistingFolder*") -and ($_ -like "*warning *" -or $_ -like "*error *") }).Count)

`Command:` records `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzer) or `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (nullable), each with the three notes. The same `WRITESET_DIAGNOSTIC_LINES` pattern is applied at baseline and at the end, so the two figures are comparable.

**CMD-VSTEST** (one assembly under the CLI runsettings with the isolation switch; `ASSEMBLY`, `FILTERARG` and `TASKID` substituted):

    Set-Location -LiteralPath "WORKTREE"
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
    $assembly = (Resolve-Path -LiteralPath "ASSEMBLY").Path
    $settings = (Resolve-Path -LiteralPath "scripts\vscode\TaskMaster.cli.runsettings").Path
    $results = Join-Path (Get-Location).Path "coverage\test-results\959\TASKID"
    if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Recurse -Force }
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    Write-Output ("RUNSETTINGS-HASH-NOW: " + (Get-FileHash -Algorithm SHA256 -LiteralPath "scripts\vscode\TaskMaster.cli.runsettings").Hash)
    Write-Output ("SANDBOX-959-EXISTS-BEFORE: " + (Test-Path -LiteralPath "C:\Sortemail959Sandbox"))
    Write-Output ("SANDBOX-956-EXISTS-BEFORE: " + (Test-Path -LiteralPath "C:\Sortemail956Sandbox"))
    Write-Output ("SANDBOX-945-EXISTS-BEFORE: " + (Test-Path -LiteralPath "C:\Sortemail945Sandbox"))
    $global:LASTEXITCODE = 0
    & $vstest $assembly "/Settings:$settings" /InIsolation FILTERARG "/ResultsDirectory:$results" "/Logger:trx;LogFileName=TASKID.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" 2>&1 | Tee-Object -FilePath "coverage\logs\TASKID.vstest.log"
    Write-Output ("VSTEST_EXIT_CODE: " + $LASTEXITCODE)
    Write-Output ("SANDBOX-959-EXISTS-AFTER: " + (Test-Path -LiteralPath "C:\Sortemail959Sandbox"))
    Write-Output ("SANDBOX-956-EXISTS-AFTER: " + (Test-Path -LiteralPath "C:\Sortemail956Sandbox"))
    Write-Output ("SANDBOX-945-EXISTS-AFTER: " + (Test-Path -LiteralPath "C:\Sortemail945Sandbox"))
    $trxPath = Join-Path $results "TASKID.trx"
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath $trxPath))
    Write-Output ("SEQUENCE_FILES: " + @(Get-ChildItem -LiteralPath $results -Recurse -Filter "Sequence_*.xml" -ErrorAction SilentlyContinue).Count)
    if (-not (Test-Path -LiteralPath $trxPath)) { Write-Output "TRX ABSENT: the run aborted before writing its result document"; exit 3 }
    [xml]$trx = Get-Content -LiteralPath $trxPath -Raw -Encoding UTF8
    $ns = New-Object System.Xml.XmlNamespaceManager($trx.NameTable)
    $ns.AddNamespace("t", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")
    $counters = $trx.SelectSingleNode("//t:ResultSummary/t:Counters", $ns)
    Write-Output ("COUNTERS total=" + $counters.GetAttribute("total") + " executed=" + $counters.GetAttribute("executed") + " passed=" + $counters.GetAttribute("passed") + " failed=" + $counters.GetAttribute("failed"))
    $all = @($trx.SelectNodes("//t:UnitTestResult", $ns))
    Write-Output ("RESULT_COUNT: " + $all.Count)
    foreach ($r in $all) { Write-Output ("RESULT " + $r.GetAttribute("testName") + " = " + $r.GetAttribute("outcome")) }
    foreach ($r in $all) { if ($r.GetAttribute("outcome") -eq "Failed") { $m = $r.SelectSingleNode("t:Output/t:ErrorInfo/t:Message", $ns); $txt = if ($null -eq $m) { "(no message)" } else { $m.InnerText }; Write-Output ("MESSAGE " + $r.GetAttribute("testName") + " :: " + $txt) } }

`Command:` records `vstest.console.exe <ASSEMBLY> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation <filter> "/ResultsDirectory:coverage\test-results\959\<task-id>" "/Logger:trx;LogFileName=<task-id>.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"` with the note `resolved through vswhere`. A `MESSAGE` line is transcribed only after the hygiene substitution. The `RESULT` rows of a data-driven test carry the `DisplayName` values (PD-5).

**CMD-COVERAGE-DIRECT** (the runner's inner collector invocation issued directly; `STAGE` is `baseline` or `final`; `EXCLUSION` substituted per P0-T9, the empty string under `EXCLUSION: NONE`):

    Set-Location -LiteralPath "WORKTREE"
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.ps1")
    $ErrorActionPreference = "Continue"
    $repo = (Get-Location).Path
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    foreach ($f in @("coverage\STAGE-959.cobertura.xml", "coverage\STAGE-959.trx", "coverage\STAGE-959.jacoco.xml")) { if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force } }
    $canonical = Get-Content -LiteralPath "coverage.config" -Raw -Encoding UTF8
    $derived = ConvertTo-DerivedCoverageSettingsXml -CanonicalSettingsXml $canonical
    $effective = Join-Path $repo "coverage\effective-coverage-959.config"
    Set-Content -LiteralPath $effective -Value $derived -Encoding UTF8 -NoNewline
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
    $rootLen = $repo.TrimEnd([char]92).Length
    $asm = @(Get-ChildItem -Path $repo -Recurse -Filter "*.Test.dll" | Where-Object { $_.FullName -like "*\bin\Debug\*" -and $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\ref\*" -and $_.FullName.Substring($rootLen) -notlike "\.claude\*" } | Select-Object -ExpandProperty FullName)
    $filter = "TestCategory!=LiveOutlook" + "EXCLUSION"
    $output = Join-Path $repo "coverage\STAGE-959.cobertura.xml"
    $settings = Join-Path $repo "scripts\vscode\TaskMaster.cli.runsettings"
    $results = Join-Path $repo "coverage\test-results\959\STAGE"
    if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Recurse -Force }
    Write-Output ("SANDBOX-959-EXISTS-BEFORE: " + (Test-Path -LiteralPath "C:\Sortemail959Sandbox"))
    Write-Output ("SANDBOX-956-EXISTS-BEFORE: " + (Test-Path -LiteralPath "C:\Sortemail956Sandbox"))
    Write-Output ("SANDBOX-945-EXISTS-BEFORE: " + (Test-Path -LiteralPath "C:\Sortemail945Sandbox"))
    $global:LASTEXITCODE = 0
    & dotnet-coverage collect --output $output --output-format cobertura --settings $effective -- $vstest @asm "/Settings:$settings" /InIsolation "/TestCaseFilter:$filter" "/ResultsDirectory:$results" "/Logger:trx;LogFileName=STAGE-959.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" 2>&1 | Tee-Object -FilePath "coverage\logs\STAGE-959.collect.log"
    Write-Output ("COLLECT_EXIT_CODE: " + $LASTEXITCODE)
    Write-Output ("SANDBOX-959-EXISTS-AFTER: " + (Test-Path -LiteralPath "C:\Sortemail959Sandbox"))
    Write-Output ("SANDBOX-956-EXISTS-AFTER: " + (Test-Path -LiteralPath "C:\Sortemail956Sandbox"))
    Write-Output ("SANDBOX-945-EXISTS-AFTER: " + (Test-Path -LiteralPath "C:\Sortemail945Sandbox"))
    Write-Output ("ASSEMBLY_COUNT: " + $asm.Count)
    $asm | ForEach-Object { Write-Output ("ASSEMBLY: " + $_.Substring($rootLen)) }
    Write-Output ("SEQUENCE_FILES: " + @(Get-ChildItem -LiteralPath $results -Recurse -Filter "Sequence_*.xml" -ErrorAction SilentlyContinue).Count)
    if (Test-Path -LiteralPath (Join-Path $results "STAGE-959.trx")) { Copy-Item -LiteralPath (Join-Path $results "STAGE-959.trx") -Destination "coverage\STAGE-959.trx" -Force }
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath "coverage\STAGE-959.trx"))

`Command:` records `dotnet-coverage collect --output coverage\<stage>-959.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-959.config -- vstest.console.exe <N test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:<filter>" "/ResultsDirectory:coverage\test-results\959\<stage>" "/Logger:trx;LogFileName=<stage>-959.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`. The route is DIRECT (Verified Repository Facts 13): the runner's own functions, explicit test assemblies discovered with the repository-relative `.claude\` exclusion, the runner's derived collector settings, the parallel CLI runsettings and `/InIsolation`. This is the CLAUDE.md step 4 route for a worktree under `.claude/worktrees`, which the runner's discovery excludes.

**CMD-COVERAGE-POST** (post-processes, projects and summarizes one stage with the runner's own functions; `STAGE` substituted):

    Set-Location -LiteralPath "WORKTREE"
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1")
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTest.TrxSummary.ps1")
    $ErrorActionPreference = "Continue"
    $repo = (Get-Location).Path
    $summary = Get-TrxRunSummary -TrxContent (Get-Content -LiteralPath "coverage\STAGE-959.trx" -Raw -Encoding UTF8)
    Write-Output "SUMMARY-BEGIN"
    Write-Output (Format-TrxRunSummary -Summary $summary)
    Write-Output "SUMMARY-END"
    Write-Output ("FAILED-SET: " + (@($summary.FailedTestName) -join ", "))
    $doc = Get-Content -LiteralPath "coverage\STAGE-959.cobertura.xml" -Raw -Encoding UTF8
    $doc = ConvertTo-KoverageCoberturaXml -XmlContent $doc -RepoRoot $repo
    Set-Content -LiteralPath "coverage\STAGE-959.cobertura.xml" -Value $doc -Encoding UTF8 -NoNewline
    try { Assert-CoberturaLineCoverageThreshold -CoberturaXml $doc; Write-Output "LINE-FLOOR: MET" } catch { Write-Output ("LINE-FLOOR: NOT MET " + $_.Exception.Message) }
    try { Assert-CoberturaBranchCoverageThreshold -CoberturaXml $doc; Write-Output "BRANCH-FLOOR: MET" } catch { Write-Output ("BRANCH-FLOOR: NOT MET " + $_.Exception.Message) }
    $fp = [string](Get-CoberturaFirstPartyCoverageReport -CoberturaXml $doc)
    Write-Output $fp
    $pct = [regex]::Matches($fp, "\((\d+\.\d+)%\)")
    Write-Output ("FIRST-PARTY-LINE-PERCENT: " + $pct[0].Groups[1].Value)
    Write-Output ("FIRST-PARTY-BRANCH-PERCENT: " + $pct[1].Groups[1].Value)
    [xml]$xml = $doc
    $projection = ConvertTo-JacocoPackageProjection -XmlDocument $xml
    Assert-JacocoProjectionReconciliation -XmlDocument $xml -ProjectionXml $projection
    Set-Content -LiteralPath "coverage\STAGE-959.jacoco.xml" -Value $projection -Encoding UTF8
    Write-Output "PROJECTION-BEGIN"
    Write-Output $projection
    Write-Output "PROJECTION-END"

The projection between `PROJECTION-BEGIN` and `PROJECTION-END`, the `First-party coverage:` line and the summary between `SUMMARY-BEGIN` and `SUMMARY-END` are the committed forms; they carry package names, counters and test names only. The `First-party coverage:` line prints two parenthesized two-decimal percentages (Format-CoberturaFirstPartyCoverageSummary), which is where the two `FIRST-PARTY-*-PERCENT:` values come from.

**CMD-COVERAGE-TEXTS** (PD-8; `STAGE` is `baseline` or `final`; `BASELINE-HASH` is `NONE` at the baseline stage and the recorded `NONEXEMPT-SET-SHA256:` of P0-T11 at the final stage; aggregates the SortEmail family of the post-processed document, derives the four content-identified exemption sets from the source text of T, prints every non-exempt uncovered line as path, line and trimmed text, hashes the sorted path-and-text set, and at the final stage compares with the baseline hash, runs the in-memory negative control and compares the first-party percentages):

    Set-Location -LiteralPath "WORKTREE"
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1")
    $b = [string][char]92
    $pattern = "*EmailParsingSorting" + $b + "SortEmail*.cs"
    $tsName = "UtilitiesCS" + $b + "EmailIntelligence" + $b + "EmailParsingSorting" + $b + "SortEmail.TrySaveAttachment.cs"
    $d = [xml](Get-Content -LiteralPath "coverage\STAGE-959.cobertura.xml" -Raw -Encoding UTF8)
    $maps = [ordered]@{}
    $t = 0
    $c = 0
    foreach ($n in @($d.SelectNodes("//class"))) { $fn = $n.GetAttribute("filename"); if ($fn -like $pattern) { $s = Get-CoberturaClassLineSummary -ClassNode $n; $t += $s.TotalLines; $c += $s.CoveredLines; $maps[$fn] = $s.LineMap; Write-Output ("SORTEMAIL-CLASS STAGE " + $fn + " valid=" + $s.TotalLines + " covered=" + $s.CoveredLines + " uncovered=" + ($s.TotalLines - $s.CoveredLines)) } }
    Write-Output ("SORTEMAIL-AGG STAGE valid=" + $t + " covered=" + $c + " uncovered=" + ($t - $c))
    Write-Output ("SORTEMAIL-DIR-CLASSES: " + @($d.SelectNodes("//class") | Where-Object { $_.GetAttribute("filename") -like ("*EmailParsingSorting" + $b + "*") }).Count)
    $src = @(Get-Content -LiteralPath "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs" -Encoding UTF8)
    $exemptLambda = @(for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i].Contains("System.IO.Directory.CreateDirectory(path)")) { $i + 1 } })
    $exemptElse = @(for ($i = 3; $i -lt $src.Count; $i++) { if ($src[$i].Trim() -ceq "}" -and $src[$i - 1].Trim() -ceq "throw;" -and $src[$i - 2].Trim() -ceq "{" -and $src[$i - 3].Trim() -ceq "else") { $i + 1 } })
    $exemptCatch = @(for ($i = 1; $i -lt ($src.Count - 1); $i++) { if ($src[$i].Trim() -ceq "}" -and ($exemptElse -contains $i) -and ($src[$i + 1].TrimStart().StartsWith("catch (System.Exception)") -or $src[$i + 1] -ceq "        }")) { $indent = $src[$i].Length - $src[$i].TrimStart().Length; $hdr = ""; for ($j = $i - 1; $j -ge 0; $j--) { if (($src[$j].Length - $src[$j].TrimStart().Length) -eq $indent -and $src[$j].TrimStart().StartsWith("catch (")) { $hdr = $src[$j].Trim(); break } }; if ($hdr -ceq "catch (System.UnauthorizedAccessException e)") { $i + 1 } } })
    $guard = @(for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i].Contains("isRetryAfterClear") -and -not $src[$i].Contains("bool isRetryAfterClear") -and -not $src[$i].Contains("isRetryAfterClear:")) { $i + 1 } })
    Write-Output ("GUARD-CONDITION-LINES: " + ($guard -join ","))
    $exemptGuard = @()
    if ($guard.Count -eq 1) { for ($i = $guard[0]; $i -lt ($src.Count - 1); $i++) { if ($src[$i].Trim() -ceq "throw;") { if ($src[$i + 1].Trim() -ceq "}") { $exemptGuard = @($i + 2) }; break } } }
    $exempt = @(@($exemptLambda) + @($exemptElse) + @($exemptCatch) + @($exemptGuard) | Sort-Object -Unique)
    Write-Output ("EXEMPT-LAMBDA-LINES: " + ($exemptLambda -join ","))
    Write-Output ("EXEMPT-LAMBDA-COUNT: " + $exemptLambda.Count)
    Write-Output ("EXEMPT-ELSE-BRACE-LINES: " + ($exemptElse -join ","))
    Write-Output ("EXEMPT-ELSE-BRACE-COUNT: " + $exemptElse.Count)
    Write-Output ("EXEMPT-CATCH-BRACE-LINES: " + ($exemptCatch -join ","))
    Write-Output ("EXEMPT-CATCH-BRACE-COUNT: " + $exemptCatch.Count)
    Write-Output ("EXEMPT-GUARD-BRACE-LINES: " + ($exemptGuard -join ","))
    Write-Output ("EXEMPT-GUARD-BRACE-COUNT: " + $exemptGuard.Count)
    Write-Output ("EXEMPT-LINES: " + ($exempt -join ","))
    $tsMap = $maps[$tsName]
    Write-Output ("TRYSAVE-CLASS-FOUND: " + ($null -ne $tsMap))
    $entries = [System.Collections.Generic.List[string]]::new()
    $exemptUncovered = 0
    foreach ($fn in @($maps.Keys)) { $m = $maps[$fn]; $lines = @(Get-Content -LiteralPath $fn -Encoding UTF8); foreach ($k in @($m.Keys | Sort-Object)) { if ($m[$k].Hits -eq 0) { if ($fn -ceq $tsName -and ($exempt -contains $k)) { $exemptUncovered++; Write-Output ("EXEMPT-UNCOVERED-LINE " + $fn + ":" + $k) } else { $text = if ($k -ge 1 -and $k -le $lines.Count) { $lines[$k - 1].Trim() } else { "(out of range)" }; $entries.Add($fn + "::" + $text); Write-Output ("NONEXEMPT-UNCOVERED " + $fn + ":" + $k + " :: " + $text) } } } }
    Write-Output ("EXEMPT-UNCOVERED: " + $exemptUncovered)
    $sorted = @($entries | Sort-Object)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $hash = [System.BitConverter]::ToString($sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes([string]::Join([string][char]10, $sorted)))).Replace("-", "")
    Write-Output ("NONEXEMPT-COUNT: " + $sorted.Count)
    Write-Output ("NONEXEMPT-SET-SHA256: " + $hash)
    Write-Output ("NONEXEMPT-SET-MATCHES-BASELINE: " + ($hash -ceq "BASELINE-HASH"))
    $ctrlLine = 0
    if ($null -ne $tsMap) { foreach ($k in @($tsMap.Keys | Sort-Object)) { if ($tsMap[$k].Hits -gt 0 -and -not ($exempt -contains $k)) { $ctrlLine = $k; break } } }
    $ctrlText = if ($ctrlLine -gt 0) { $src[$ctrlLine - 1].Trim() } else { "(none)" }
    $ctrlSorted = @(@($sorted) + @($tsName + "::" + $ctrlText) | Sort-Object)
    $ctrlHash = [System.BitConverter]::ToString($sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes([string]::Join([string][char]10, $ctrlSorted)))).Replace("-", "")
    Write-Output ("CONTROL-LINE: " + $ctrlLine)
    Write-Output ("CONTROL-SET-SHA256: " + $ctrlHash)
    Write-Output ("CONTROL-DIFFERS-BASELINE: " + ($ctrlHash -cne "BASELINE-HASH"))
    if ("STAGE" -ceq "final") { $fpB = [string](Get-CoberturaFirstPartyCoverageReport -CoberturaXml (Get-Content -LiteralPath "coverage\baseline-959.cobertura.xml" -Raw -Encoding UTF8)); $fpF = [string](Get-CoberturaFirstPartyCoverageReport -CoberturaXml (Get-Content -LiteralPath "coverage\final-959.cobertura.xml" -Raw -Encoding UTF8)); $pB = [regex]::Matches($fpB, "\((\d+\.\d+)%\)"); $pF = [regex]::Matches($fpF, "\((\d+\.\d+)%\)"); Write-Output ("BASELINE-FIRST-PARTY: " + $fpB); Write-Output ("FINAL-FIRST-PARTY: " + $fpF); Write-Output ("FIRST-PARTY-LINE-NOT-LOWER: " + ([double]$pF[0].Groups[1].Value -ge [double]$pB[0].Groups[1].Value)); Write-Output ("FIRST-PARTY-BRANCH-NOT-LOWER: " + ([double]$pF[1].Groups[1].Value -ge [double]$pB[1].Groups[1].Value)); [xml]$jb = Get-Content -LiteralPath "coverage\baseline-959.jacoco.xml" -Raw -Encoding UTF8; [xml]$jf = Get-Content -LiteralPath "coverage\final-959.jacoco.xml" -Raw -Encoding UTF8; $q = [string][char]39; foreach ($pk in @("UtilitiesCS", "QuickFiler")) { foreach ($type in @("LINE", "BRANCH")) { $xp = "/report/package[@name=" + $q + $pk + $q + "]/counter[@type=" + $q + $type + $q + "]"; $bc = $jb.SelectSingleNode($xp); $fc = $jf.SelectSingleNode($xp); if ($null -eq $bc -or $null -eq $fc) { Write-Output ("PACKAGE " + $pk + " " + $type + " MISSING"); continue }; $bCov = [int]$bc.GetAttribute("covered"); $bVal = $bCov + [int]$bc.GetAttribute("missed"); $fCov = [int]$fc.GetAttribute("covered"); $fVal = $fCov + [int]$fc.GetAttribute("missed"); Write-Output ("PACKAGE " + $pk + " " + $type + " baseline=" + $bCov + "/" + $bVal + " final=" + $fCov + "/" + $fVal) } } }

The Helpers script sets `Set-StrictMode -Version Latest`; every variable the payload reads is assigned before use. Rule (3) accepts either the baseline shape (the else-brace line followed by the catch brace and then `catch (System.Exception)`) or the final shape (followed by the method's closing brace at indentation eight); rule (4) yields an empty set whenever the guard condition line is absent (baseline) or not unique. `SORTEMAIL-DIR-CLASSES:` is the positive control of the filename pattern: a value of 0 is `SORTEMAIL FILENAME MATCH UNPROVEN`: stop and report. The `PACKAGE` rows and the two `FIRST-PARTY` lines are observations except the two `NOT-LOWER` flags (PD-8). Predictions (recorded for the reader, not acceptance conditions): baseline `NONEXEMPT-COUNT: 1` with the single `NONEXEMPT-UNCOVERED` row naming SortEmail.MailItemSort.cs and the text `await attachments.ForEachAsync(async x => await x.SaveAttachmentAsync());`; final the same row, `EXEMPT-GUARD-BRACE-COUNT: 1`, `EXEMPT-UNCOVERED: 4`.

**CMD-MEMBER-COVERAGE** (P6-T9; per-member line coverage of the ten AC25 members in the final document; a member span starts at the unique source line whose trimmed text starts with the listed signature prefix and ends at the first following line that is exactly eight spaces and `}`; the EfcDataModel changed lines are located by their trimmed text):

    Set-Location -LiteralPath "WORKTREE"
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1")
    $b = [string][char]92
    $d = [xml](Get-Content -LiteralPath "coverage\final-959.cobertura.xml" -Raw -Encoding UTF8)
    $files = @{ A = "UtilitiesCS" + $b + "EmailIntelligence" + $b + "EmailParsingSorting" + $b + "SortEmail.AttachmentSaving.cs"; T = "UtilitiesCS" + $b + "EmailIntelligence" + $b + "EmailParsingSorting" + $b + "SortEmail.TrySaveAttachment.cs"; U = "UtilitiesCS" + $b + "EmailIntelligence" + $b + "EmailParsingSorting" + $b + "SortEmail.UndoAndMoveLog.cs"; E = "QuickFiler" + $b + "Controllers" + $b + "EfcDataModel.cs" }
    $maps = @{}
    foreach ($key in @($files.Keys)) { $nodes = @($d.SelectNodes("//class") | Where-Object { $_.GetAttribute("filename") -ceq $files[$key] }); Write-Output ("CLASS-NODES " + $key + " = " + $nodes.Count); $merged = @{}; foreach ($n in $nodes) { $lm = (Get-CoberturaClassLineSummary -ClassNode $n).LineMap; foreach ($k in @($lm.Keys)) { if (-not $merged.ContainsKey($k) -or $lm[$k].Hits -gt $merged[$k]) { $merged[$k] = $lm[$k].Hits } } }; $maps[$key] = $merged }
    $specs = @("A|SaveAttachmentAsyncCore|internal static async Task SaveAttachmentAsync(", "A|SaveAttachmentCore|internal static void SaveAttachment(", "A|SaveCaseAsync|internal static async Task SaveCaseAsync(", "A|SaveCase|internal static void SaveCase(", "A|RedirectSaveFolder|internal static void RedirectSaveFolder(", "A|Cleanup_Files|public static void Cleanup_Files()", "T|TrySaveAttachmentCoreAsync|private static async Task<bool> TrySaveAttachmentCoreAsync(", "U|WriteCsvCore|internal static void WriteCSV_StartNewFileIfDoesNotExist(", "E|ResetFilerPromptState|protected internal virtual void ResetFilerPromptState()")
    $below = 0
    $unmeasured = 0
    $ambiguous = 0
    foreach ($spec in $specs) { $parts = $spec.Split("|"); $key = $parts[0]; $name = $parts[1]; $sig = $parts[2]; $src = @(Get-Content -LiteralPath $files[$key] -Encoding UTF8); $starts = @(for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i].Trim().StartsWith($sig)) { $i + 1 } }); if ($starts.Count -ne 1) { $ambiguous++; Write-Output ("MEMBER " + $name + " START-MATCHES=" + $starts.Count); continue }; $s0 = $starts[0]; $end = 0; for ($i = $s0; $i -lt $src.Count; $i++) { if ($src[$i] -ceq "        }") { $end = $i + 1; break } }; $m = $maps[$key]; $cv = 0; $cc = 0; $unc = @(); foreach ($k in @($m.Keys)) { if ($k -ge $s0 -and $k -le $end) { $cv++; if ($m[$k] -gt 0) { $cc++ } else { $unc += $k } } }; $cp = if ($cv -gt 0) { [math]::Round(100 * $cc / $cv, 2) } else { 0 }; if ($cv -eq 0) { $unmeasured++ }; if ($cv -gt 0 -and $cp -lt 90) { $below++ }; Write-Output ("MEMBER " + $name + " span=" + $s0 + "-" + $end + " valid=" + $cv + " covered=" + $cc + " percent=" + $cp + " uncovered=" + ((@($unc) | Sort-Object) -join ",")) }
    $eSrc = @(Get-Content -LiteralPath $files["E"] -Encoding UTF8)
    $eCovered = 0
    foreach ($text in @("result = await InvokeFilerAsync(config, mailHelpers);", "ResetFilerPromptState();", "return result;")) { $ln = @(for ($i = 0; $i -lt $eSrc.Count; $i++) { if ($eSrc[$i].Trim() -ceq $text) { $i + 1 } }); $hits = -1; if ($ln.Count -eq 1 -and $maps["E"].ContainsKey($ln[0])) { $hits = $maps["E"][$ln[0]] }; if ($hits -gt 0) { $eCovered++ }; Write-Output ("E-CHANGED-LINE " + $text + " matches=" + $ln.Count + " line=" + ($ln -join ",") + " hits=" + $hits) }
    Write-Output ("E-CHANGED-LINES-COVERED: " + $eCovered)
    Write-Output ("MEMBERS-AMBIGUOUS: " + $ambiguous)
    Write-Output ("MEMBERS-UNMEASURED: " + $unmeasured)
    Write-Output ("MEMBERS-BELOW-90: " + $below)

The ten AC25 members are the nine `MEMBER` rows plus the changed lines of `EfcDataModel.MoveToFolderAsync`, gated by the three `E-CHANGED-LINE` rows. The post-processed document holds one merged class element per file (Merge-CoberturaClassesByFilename), so the async state machines and closures of a file are included in that file's line map; the payload still unions every class node of the filename so a second node cannot hide a line.

**CMD-USINGS** (exact using-block check of the five surviving partials against the Technical specifications blocks; prints one `USINGS` row per file):

    Set-Location -LiteralPath "WORKTREE"
    $exp = @{}
    $exp["A"] = @("using System;", "using System.Collections.Generic;", "using System.Diagnostics.CodeAnalysis;", "using System.IO;", "using System.Linq;", "using System.Threading.Tasks;", "using Microsoft.Office.Interop.Outlook;", "using UtilitiesCS.EmailIntelligence;")
    $exp["T"] = @("using System;", "using System.Diagnostics.CodeAnalysis;", "using System.IO;", "using System.Threading.Tasks;", "using Microsoft.Office.Interop.Outlook;")
    $exp["U"] = @("using System;", "using System.Diagnostics.CodeAnalysis;", "using System.IO;", "using System.Linq;", "using System.Text.RegularExpressions;", "using System.Threading.Tasks;", "using System.Windows.Forms;", "using Microsoft.Office.Interop.Outlook;", "using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;", "using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;")
    $exp["S"] = @("using System;", "using System.Collections.Generic;", "using System.Diagnostics.CodeAnalysis;", "using System.IO;", "using System.Linq;", "using System.Threading.Tasks;", "using Microsoft.Office.Interop.Outlook;", "using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;", "using UtilitiesCS.OutlookExtensions;")
    $exp["M"] = @("using System;", "using System.Collections.Generic;", "using System.Diagnostics.CodeAnalysis;", "using System.IO;", "using System.Linq;", "using System.Threading.Tasks;", "using System.Windows.Forms;", "using Microsoft.Office.Interop.Outlook;", "using UtilitiesCS.OutlookExtensions;")
    $files = @{ A = "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs"; T = "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs"; U = "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs"; S = "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs"; M = "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs" }
    $exact = 0
    foreach ($key in @("A", "T", "U", "S", "M")) { $L = @(Get-Content -LiteralPath $files[$key] -Encoding UTF8); $block = @(); for ($i = 1; $i -lt $L.Count; $i++) { if ($L[$i].StartsWith("using ")) { $block += $L[$i] } else { break } }; $ok = (($block -join "|") -ceq ($exp[$key] -join "|")); if ($ok) { $exact++ }; Write-Output ("USINGS " + $key + " count=" + $block.Count + " exact=" + $ok + " firstline=" + ($L[0] -ceq "#nullable enable") + " blankafter=" + ($L[$block.Count + 1].Trim().Length -eq 0)) }
    Write-Output ("USINGS-EXACT-FILES: " + $exact)

**CMD-GREP-FACTS** (`STAGE` is `base` or `final`; regex counts over fixed file sets, excluding build output, packages and any `.claude` directory; prints named counts and the SortEmail file list):

    Set-Location -LiteralPath "WORKTREE"
    $root = (Get-Location).Path
    $skip = { param($f) $rel = $f.FullName.Substring($root.Length); ($rel -like "*\bin\*") -or ($rel -like "*\obj\*") -or ($rel -like "*\packages\*") -or ($rel -like "*\.claude\*") -or ($rel -like "*\.dotnet-sdk\*") }
    $cs = @(Get-ChildItem -Path $root -Recurse -Filter "*.cs" -File | Where-Object { -not (& $skip $_) })
    $csproj = @(Get-ChildItem -Path $root -Recurse -Filter "*.csproj" -File | Where-Object { -not (& $skip $_) })
    $partials = @(Get-ChildItem -LiteralPath "UtilitiesCS\EmailIntelligence\EmailParsingSorting" -Filter "SortEmail*.cs" -File)
    $tests6 = @(@("UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs", "UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs", "UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs", "UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs", "UtilitiesCS.Test\EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs", "QuickFiler.Test\Controllers\EfcDataModelFilerCleanupTests.cs") | Where-Object { Test-Path -LiteralPath $_ } | ForEach-Object { Get-Item -LiteralPath $_ })
    $count = { param($set, $rx) $n = 0; foreach ($f in $set) { $n += [regex]::Matches((Get-Content -LiteralPath $f.FullName -Raw -Encoding UTF8), $rx).Count }; $n }
    $fileCount = { param($set, $rx) @($set | Where-Object { [regex]::IsMatch((Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8), $rx) }).Count }
    Write-Output ("CS-FILES: " + $cs.Count)
    Write-Output ("DEAD-MEMBERS-CS: " + (& $count $cs "SaveAttachmentsOld|IsPicture\b"))
    Write-Output ("TODOMODEL-CSPROJ-MATCHES: " + (& $count $csproj "SortItemsToExistingFolder"))
    Write-Output ("TODOMODEL-CSPROJ-FILES: " + ((@($csproj | Where-Object { [regex]::IsMatch((Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8), "SortItemsToExistingFolder") } | ForEach-Object { $_.FullName.Substring($root.Length + 1) })) -join ","))
    Write-Output ("NEW-IDENTIFIERS: " + (& $count @($cs + $csproj) "ResetFilerPromptState|RedirectSaveFolder|AllPromptSessions|MovedMailsHeader|TrySaveAttachmentCoreAsync|CreateDirectoryLimit"))
    Write-Output ("SORTEMAIL-FILES: " + (($partials | ForEach-Object { $_.Name }) -join ","))
    Write-Output ("SORTEMAIL-FILE-COUNT: " + $partials.Count)
    Write-Output ("SHOWDIALOG-CALLS-PARTIALS: " + (& $count $partials "ShowDialog\("))
    Write-Output ("ENUM-FIELDS-A: " + (& $count @(Get-Item -LiteralPath "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs") "YesNoToAllResponse _"))
    Write-Output ("DEAD-TOKENS-PARTIALS: " + (& $count $partials "SaveAttachmentsOld|IsPicture|_responseSaveFile|MAX_PATH"))
    Write-Output ("BANNED-USINGS-PARTIALS: " + (& $count $partials "using Deedle;|using SDILReader;|using Outlook =|using UtilitiesCS;|using System\.Diagnostics;"))
    Write-Output ("USING-SYSTEM-PARTIAL-FILES: " + (& $fileCount $partials "(?m)^using System;"))
    Write-Output ("DEBUG-WRITELINE-T: " + (& $count @(Get-Item -LiteralPath "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs") "Debug\.WriteLine"))
    Write-Output ("EFCC-PARTIALS: " + (& $count $partials "\[ExcludeFromCodeCoverage\]"))
    Write-Output ("TESTS6-PRESENT: " + $tests6.Count)
    Write-Output ("BANNED-TEST-APIS: " + (& $count $tests6 "DoNotParallelize|Thread\.Sleep|Task\.Delay|Timeout|Directory\.CreateDirectory|File\.Create|File\.WriteAll|GetTempPath|MemoryAppender"))
    Write-Output ("NON-APPROVED-FRAMEWORKS: " + (& $count $tests6 "using Xunit|using NUnit|using NSubstitute|using FakeItEasy|using Shouldly"))
    Write-Output ("LEGACY-FILE-EXISTS: " + (Test-Path -LiteralPath "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs"))
    Write-Output ("TODOMODEL-FILE-EXISTS: " + (Test-Path -LiteralPath "ToDoModel\Email Utilities\SortItemsToExistingFolder.cs"))

Expected values:

| Line | base (P0-T12) | final (P6-T10) |
| --- | --- | --- |
| `CS-FILES:` (positive control of the `$skip` filter, which is root-relative because the item worktree's own absolute path contains `\.claude\worktrees\`) | at least 1 | at least 1 |
| `DEAD-MEMBERS-CS:` | 2 | 0 |
| `TODOMODEL-CSPROJ-MATCHES:` | 2 | 2 |
| `TODOMODEL-CSPROJ-FILES:` | `ToDoModel.Test\ToDoModel.Test.csproj` | same |
| `NEW-IDENTIFIERS:` | 0 | at least 1 (observation) |
| `SORTEMAIL-FILES:` | the six partial names | the five surviving names (no LegacyAttachmentSaving) |
| `SORTEMAIL-FILE-COUNT:` | 6 | 5 |
| `SHOWDIALOG-CALLS-PARTIALS:` (A 112, 135, 175, 198, 255 and the comment 258; L 165, 182 and the `InputBox.ShowDialog(` at 199) | 9 | 0 |
| `ENUM-FIELDS-A:` | 4 | 0 |
| `DEAD-TOKENS-PARTIALS:` | at least 1 (positive control; predicted 12) | 0 |
| `BANNED-USINGS-PARTIALS:` | 30 | 0 |
| `USING-SYSTEM-PARTIAL-FILES:` | 6 | 5 |
| `DEBUG-WRITELINE-T:` | 3 | 0 |
| `EFCC-PARTIALS:` | 28 | 18 |
| `TESTS6-PRESENT:` | 2 | 6 |
| `BANNED-TEST-APIS:` | 0 | 0 |
| `NON-APPROVED-FRAMEWORKS:` | 0 | 0 |
| `LEGACY-FILE-EXISTS:` | True | False |
| `TODOMODEL-FILE-EXISTS:` | True | False |

**CMD-SPEC-CHECK** (`STAGE` is `base` or `final`; read-only counts over FEATURE/issue.md, FEATURE/spec.md and SPEC956; backticks and apostrophes of the SPEC956 literals are built from `[char]96` and `[char]39`):

    Set-Location -LiteralPath "WORKTREE"
    $bt = [string][char]96
    $ap = [string][char]39
    $issue = Get-Content -LiteralPath "docs\features\active\2026-10-01-sort-email-latent-logic-defects-959\issue.md" -Raw -Encoding UTF8
    $spec = Get-Content -LiteralPath "docs\features\active\2026-10-01-sort-email-latent-logic-defects-959\spec.md" -Raw -Encoding UTF8
    $s956 = Get-Content -LiteralPath "docs\features\active\2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956\spec.md" -Raw -Encoding UTF8
    Write-Output ("WORKMODE-LINES: " + [regex]::Matches($issue, "(?m)^- Work Mode: full-bug\r?\n").Count)
    Write-Output ("AC-HEADING-LINES: " + [regex]::Matches($spec, "(?m)^## Acceptance Criteria\r?\n").Count)
    Write-Output ("AC-UNCHECKED: " + [regex]::Matches($spec, "(?m)^- \[ \] AC([1-9]|1[0-9]|2[0-7]) \(").Count)
    Write-Output ("AC-CHECKED: " + [regex]::Matches($spec, "(?m)^- \[x\] AC([1-9]|1[0-9]|2[0-7]) \(").Count)
    Write-Output ("AC6-UNCHECKED: " + [regex]::Matches($spec, "(?m)^- \[ \] AC6 \(").Count)
    Write-Output ("AC27-UNCHECKED: " + [regex]::Matches($spec, "(?m)^- \[ \] AC27 \(").Count)
    Write-Output ("AC-ANY-UNCHECKED: " + [regex]::Matches($spec, "(?m)^- \[ \] AC").Count)
    Write-Output ("USERSTORY-EXISTS: " + (Test-Path -LiteralPath "docs\features\active\2026-10-01-sort-email-latent-logic-defects-959\user-story.md"))
    Write-Output ("AC15-SEAM-STEP: " + [regex]::Matches($spec, [regex]::Escape("after the seam step")).Count)
    Write-Output ("AC6-RESET-LITERAL: " + [regex]::Matches($spec, [regex]::Escape("_attachmentsAltName = YesNoToAllResponse.Empty;")).Count)
    Write-Output ("AC25-NINETY: " + [regex]::Matches($spec, [regex]::Escape("ninety percent")).Count)
    Write-Output ("S956-OLD-99: " + [regex]::Matches($s956, [regex]::Escape("as merge-base line 944 does, so the exception boundary is unchanged.")).Count)
    Write-Output ("S956-NEW-99: " + [regex]::Matches($s956, [regex]::Escape("now runs inside it as the first statement of the adapter")).Count)
    Write-Output ("S956-149: " + [regex]::Matches($s956, [regex]::Escape("and no retry bound is added (L2 is out of scope).")).Count)
    Write-Output ("S956-NEW-149: " + [regex]::Matches($s956, [regex]::Escape("Superseded 2026-10-02 by #959 (closes #966)")).Count)
    Write-Output ("S956-OLD-155: " + [regex]::Matches($s956, [regex]::Escape("- The " + $bt + "DirectoryInfo" + $bt + " construction" + $ap + "s exception boundary (path computed before the inner " + $bt + "try" + $bt + ").")).Count)
    Write-Output ("S956-NEW-155: " + [regex]::Matches($s956, [regex]::Escape("- The read-only clear" + $ap + "s exception boundary:")).Count)
    Write-Output ("S956-AC-CHECKED: " + [regex]::Matches($s956, "(?m)^- \[x\] AC").Count)
    Write-Output ("S956-AC-UNCHECKED: " + [regex]::Matches($s956, "(?m)^- \[ \] AC").Count)
    Write-Output ("S956-LINES: " + @(Get-Content -LiteralPath "docs\features\active\2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956\spec.md" -Encoding UTF8).Count)

Expected at `base` (P0-T2): `WORKMODE-LINES: 1`, `AC-HEADING-LINES: 1`, `AC-UNCHECKED: 27`, `AC-CHECKED: 0`, `AC6-UNCHECKED: 1` and `AC27-UNCHECKED: 1` (observations), `USERSTORY-EXISTS: False`, `AC15-SEAM-STEP:` at least 1, `AC6-RESET-LITERAL:` at least 1, `AC25-NINETY:` at least 1, `S956-OLD-99: 1`, `S956-NEW-99: 0`, `S956-149: 1`, `S956-NEW-149: 0`, `S956-OLD-155: 1`, `S956-NEW-155: 0`, `S956-AC-CHECKED:` plus `S956-AC-UNCHECKED:` equal to 17 (recorded as `S956-AC-CHECKED-BASE:` and `S956-AC-UNCHECKED-BASE:`), `S956-LINES:` recorded as `S956-LINES-BASE:`. Expected after P5-T11: `S956-OLD-99: 0`, `S956-NEW-99: 1`, `S956-149: 1`, `S956-NEW-149: 1`, `S956-OLD-155: 0`, `S956-NEW-155: 1`, the two S956 check counts equal to their base values, `S956-LINES:` equal to base plus 2. Expected at P6-T46 under branch (a) of P6-T7: `AC-CHECKED: 25`, `AC6-UNCHECKED: 1`, `AC27-UNCHECKED: 1`, `AC-ANY-UNCHECKED: 2`; under branch (b) (AC21 and AC24 left unchecked by P6-T39 and P6-T42): `AC-CHECKED: 23`, `AC6-UNCHECKED: 1`, `AC27-UNCHECKED: 1`, `AC-ANY-UNCHECKED: 4`.

**CMD-LINE-CONDITION** (P7-T1, P7-T13 and P8-T10; revision 2.0; reads the `condition-coverage` attribute of A line 143 in one post-processed Cobertura document; `DOC` is the repository-relative document path, `coverage\final-959.cobertura.xml` in every use; prints the number of class nodes carrying the file, the number of class-level `<line>` nodes numbered 143, that node's `branch` and `condition-coverage` attributes and the `branch-rate` of the `SaveAttachment` method node; the payload carries none of the words the hook containment rule names and no `git` command; observed success-case shape: the Phase 6 document prints `A-CLASS-NODES: 1`, `A-LINE-143-COUNT: 1`, `A-LINE-143-BRANCH: True`, `A-LINE-143-CONDITION: 50% (1/2)` and `A-SAVEATTACHMENT-BRANCH-RATE: 0.75`, read at document lines 59687, 59791 and 59802 in the revision 2.0 pass):

    Set-Location -LiteralPath "WORKTREE"
    $b = [string][char]92
    $fn = "UtilitiesCS" + $b + "EmailIntelligence" + $b + "EmailParsingSorting" + $b + "SortEmail.AttachmentSaving.cs"
    $d = [xml](Get-Content -LiteralPath "DOC" -Raw -Encoding UTF8)
    $nodes = @($d.SelectNodes("//class") | Where-Object { $_.GetAttribute("filename") -ceq $fn })
    Write-Output ("A-CLASS-NODES: " + $nodes.Count)
    $l143 = @(foreach ($n in $nodes) { @($n.SelectNodes("lines/line")) | Where-Object { $_.GetAttribute("number") -ceq "143" } })
    Write-Output ("A-LINE-143-COUNT: " + $l143.Count)
    foreach ($l in $l143) { Write-Output ("A-LINE-143-BRANCH: " + $l.GetAttribute("branch")); Write-Output ("A-LINE-143-CONDITION: " + $l.GetAttribute("condition-coverage")) }
    $s = @(foreach ($n in $nodes) { @($n.SelectNodes("methods/method")) | Where-Object { $_.GetAttribute("name") -ceq "SaveAttachment" } })
    foreach ($m in $s) { Write-Output ("A-SAVEATTACHMENT-BRANCH-RATE: " + $m.GetAttribute("branch-rate")) }

`Command:` records `CMD-LINE-CONDITION over <DOC>`. A `100% (2/2)` value is printed only when both arms of the ternary at line 143 were executed, so the value discriminates the SS4 test's effect; the P7-T1 reading of the Phase 6 document is the false-before observation and the P7-T13 reading of the Phase 7 document the true-after one.

**CMD-FANIN** (P8-T3; revision 2.0; the additions-only fan-in gate after the Phase 8 merge; `ORIGIN-MAIN-SHA` substituted with the P8-T1 value; every diff is anchored at that SHA or at the Phase 0 to 7 anchor and the task pairs it with the printed porcelain count (rules G8 and G8b); `MAIN-ONLY-PATHS:`, `MAIN-ONLY-HAS-QFT-CSPROJ:` and `CONTROL-OUTSIDE-IF-UNFILTERED:` are negative controls over committed ranges that prove the path-set check can fire; the payload carries `git` and the expanded worktree path but no `remove`, `gh`, `merge`, `create`, `edit`, `issue` or `new` text):

    Set-Location -LiteralPath "WORKTREE"
    $base = "94287369908cc920b21b0e3256314f988ad7d2f5"
    $rows = @(git diff --name-status ORIGIN-MAIN-SHA HEAD)
    $paths = @($rows | ForEach-Object { ($_ -split "\t")[-1] })
    $write = @("UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs", "UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs", "UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs", "UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs", "UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs", "UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs", "UtilitiesCS/UtilitiesCS.csproj", "QuickFiler/Controllers/EfcDataModel.cs", "ToDoModel/Email Utilities/SortItemsToExistingFolder.cs", "UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs", "UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs", "UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs", "UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs", "UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs", "UtilitiesCS.Test/UtilitiesCS.Test.csproj", "QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs", "QuickFiler.Test/QuickFiler.Test.csproj", "docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md")
    $project = @("UtilitiesCS/UtilitiesCS.csproj", "UtilitiesCS.Test/UtilitiesCS.Test.csproj", "QuickFiler.Test/QuickFiler.Test.csproj", "docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md")
    $feature = "docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/"
    $record = "docs/features/potential/promoted/2026-10-01-sort-email-latent-logic-defects.md"
    $own = { param($p) ($write -contains $p) -or $p.StartsWith($feature) -or ($p -ceq $record) -or $p.StartsWith(".claude/agent-memory/") }
    $outside = @($paths | Where-Object { -not (& $own $_) })
    Write-Output ("FANIN-ROWS: " + $rows.Count)
    $rows | ForEach-Object { Write-Output ("FANIN " + $_) }
    Write-Output ("FANIN-OUTSIDE: " + $outside.Count)
    $outside | ForEach-Object { Write-Output ("FANIN-OUTSIDE-PATH " + $_) }
    Write-Output ("FANIN-DELETED: " + @($rows | Where-Object { $_.StartsWith("D") }).Count)
    Write-Output ("FANIN-WRITE-SET-MISSING: " + @($write | Where-Object { $paths -notcontains $_ }).Count)
    Write-Output ("NUMSTAT-QFT-VS-MAIN: " + (@(git diff --numstat ORIGIN-MAIN-SHA HEAD -- QuickFiler.Test/QuickFiler.Test.csproj) -join " "))
    Write-Output ("NUMSTAT-UCT-VS-MAIN: " + (@(git diff --numstat ORIGIN-MAIN-SHA HEAD -- UtilitiesCS.Test/UtilitiesCS.Test.csproj) -join " "))
    Write-Output ("NUMSTAT-UCS-VS-MAIN: " + (@(git diff --numstat ORIGIN-MAIN-SHA HEAD -- UtilitiesCS/UtilitiesCS.csproj) -join " "))
    $mainOnly = @(git diff --name-only $base ORIGIN-MAIN-SHA)
    Write-Output ("MAIN-ONLY-PATHS: " + $mainOnly.Count)
    Write-Output ("MAIN-ONLY-HAS-QFT-CSPROJ: " + ($mainOnly -ccontains "QuickFiler.Test/QuickFiler.Test.csproj"))
    Write-Output ("MAIN-TOUCHED-WRITE-SET: " + (@($mainOnly | Where-Object { $write -contains $_ }) -join ","))
    Write-Output ("MAIN-TOUCHED-WRITE-SET-CODE: " + (@($mainOnly | Where-Object { ($write -contains $_) -and ($project -notcontains $_) }) -join ","))
    Write-Output ("CONTROL-OUTSIDE-IF-UNFILTERED: " + @($mainOnly | Where-Object { -not (& $own $_) }).Count)
    $shared = @($mainOnly | Where-Object { $paths -ccontains $_ })
    Write-Output ("SHARED-PATHS: " + $shared.Count)
    foreach ($p in $shared) { Write-Output ("SHARED-NUMSTAT " + (@(git diff --numstat ORIGIN-MAIN-SHA HEAD -- $p) -join " ")) }
    Write-Output ("SHARED-WITH-LOSS: " + @($shared | Where-Object { (@(git diff --numstat ORIGIN-MAIN-SHA HEAD -- $_) -join "") -notmatch "^[0-9]+\t0\t" }).Count)
    Write-Output ("LOSS-CHECK-CONTROL: " + ((@(git diff --numstat ORIGIN-MAIN-SHA HEAD -- UtilitiesCS/UtilitiesCS.csproj) -join "") -notmatch "^[0-9]+\t0\t"))
    Write-Output ("PORCELAIN-COUNT: " + @(git status --porcelain --untracked-files=all).Count)

`Command:` records `CMD-FANIN with ORIGIN-MAIN-SHA <value>`. `FANIN-OUTSIDE:` is the gate: a path main did not already have that is not one of this branch's own paths; because HEAD contains `ORIGIN-MAIN-SHA` after the P8-T2 merge, the two-point diff `ORIGIN-MAIN-SHA HEAD` lists exactly what this branch adds to main, so a main change that the merge had reverted would appear in it as a non-own path. `CONTROL-OUTSIDE-IF-UNFILTERED:` applies the same own-path filter to the paths main gained since the Phase 0 anchor and must be greater than 0 (those paths would be counted if they appeared in the fan-in diff); `MAIN-ONLY-HAS-QFT-CSPROJ: True` proves that the csproj overlap is a real main-side change; `MAIN-TOUCHED-WRITE-SET-CODE:` lists the Write Set source files main changed (expected empty) and conditions the P8-T8 and P8-T10 clauses that depend on main not having edited the SortEmail family. `SHARED-PATHS:` lists the paths changed both on main since the Phase 0 anchor and on this branch relative to main; `SHARED-WITH-LOSS:` counts those whose numstat against `ORIGIN-MAIN-SHA` deletes any line; `LOSS-CHECK-CONTROL:` applies the same predicate to UtilitiesCS/UtilitiesCS.csproj (`0 1`) and must print `True`, proving the predicate can fire.

**CMD-FORMAT-REPO** (P6-T1; repository-wide format with a before-and-after tree observation; `TASKID` substituted):

    Set-Location -LiteralPath "WORKTREE"
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $ws = @("UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs", "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs", "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs", "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs", "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs", "QuickFiler\Controllers\EfcDataModel.cs", "UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs", "UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs", "UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs", "UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs", "UtilitiesCS.Test\EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs", "QuickFiler.Test\Controllers\EfcDataModelFilerCleanupTests.cs")
    $before = @{}
    foreach ($p in $ws) { $before[$p] = (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash }
    $pb = @(git status --porcelain --untracked-files=all)
    $global:LASTEXITCODE = 0
    & dotnet tool run csharpier format . 2>&1 | Tee-Object -FilePath "coverage\logs\TASKID.csharpier-format.log"
    Write-Output ("FORMAT_EXIT_CODE: " + $LASTEXITCODE)
    $changed = 0
    foreach ($p in $ws) { if ((Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash -ne $before[$p]) { $changed++; Write-Output ("WRITESET-CHANGED: " + $p) } }
    Write-Output ("WRITESET-CHANGED-COUNT: " + $changed)
    $pa = @(git status --porcelain --untracked-files=all)
    Write-Output ("PORCELAIN-BEFORE-COUNT: " + $pb.Count)
    Write-Output ("PORCELAIN-AFTER-COUNT: " + $pa.Count)
    Write-Output ("PORCELAIN-SAME: " + (($pb -join [string][char]10) -ceq ($pa -join [string][char]10)))

**CMD-CHECK-REPO** (read-only repository-wide verification; `TASKID` substituted):

    Set-Location -LiteralPath "WORKTREE"
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $global:LASTEXITCODE = 0
    & dotnet tool run csharpier check . 2>&1 | Tee-Object -FilePath "coverage\logs\TASKID.csharpier-check.log"
    Write-Output ("CHECK_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\TASKID.csharpier-check.log" -Raw -Encoding UTF8
    Write-Output ("CHECKED-LINE: " + [regex]::Match($log, "Checked \d+ files[^\r\n]*").Value)

**CMD-EVIDENCE-FIELDS** (P6-T16, P6-T44 and P6-T46; counts the evidence artifacts written by this run that lack any of the three schema fields AC26 names as a line-leading label: a label may follow list or emphasis punctuation, never a word character, so a prefixed row such as `FORMAT_EXIT_CODE:` does not satisfy `EXIT_CODE:`, and two controls prove that on every run, both built from the `$anchor` prefix variable the per-file predicate itself uses so that they exercise the predicate actually run: `FIELD-CHECK-CONTROL:` tests the literal `FORMAT_EXIT_CODE: 0` against the `EXIT_CODE:` pattern and prints `False`, and `FIELD-CHECK-POSITIVE:` tests the literal `- **EXIT_CODE:** 0` against the same pattern and prints `True`. The subfolder test normalizes each repository-relative path to forward slashes and matches the backslash-free `$canon` pattern, because a doubled backslash in a `pwsh -Command` argument arrives de-doubled from Bash; `SUBFOLDER-CHECK-FLAGS-OTHER:` (an `evidence/other/` path, must print `True`) and `SUBFOLDER-CHECK-FLAGS-QA-GATES:` (an `evidence/qa-gates/` path, must print `False`) exercise that pattern on every run. The file set is every file under FEATURE/evidence/ minus `INHERITED`; the plan, spec.md, issue.md and research/ sit outside that folder and are never counted, so `NONCANONICAL-SUBFOLDER-FILES:` reaches 0 when every artifact sits under the three permitted subfolders and rises, with one `NONCANONICAL:` row per file, for a file under any other subfolder or directly under FEATURE/evidence/. `INHERITED` substituted as the quoted `INHERITED-CLAUSE-A:` paths of P0-T3 joined by `, ` MINUS the P0-T1 artifact docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/phase0-instructions-read.md and the P0-T2 artifact p0-t2-mode-preconditions.<TS>.md in the same folder, or nothing; both were written by this run before P0-T3 captured Clause A, so they are checked, while artifacts the orchestrator committed before execution are reported separately rather than counted. A Clause A path that P0-T2's `PRE-EXISTING-EVIDENCE:` names stays in `INHERITED`; that field cannot name either of the two artifacts, because P0-T2 excludes the P0-T1 artifact from it and writes its own artifact after the check):

    Set-Location -LiteralPath "WORKTREE"
    $inherited = @(INHERITED)
    $root = (Get-Location).Path
    $anchor = "(?m)^[^\w\r\n]*"
    $files = @(Get-ChildItem -LiteralPath "docs\features\active\2026-10-01-sort-email-latent-logic-defects-959\evidence" -Recurse -File | Where-Object { $inherited -notcontains ($_.FullName.Substring($root.Length + 1).Replace([string][char]92, "/")) })
    $missing = 0
    foreach ($f in $files) { $c = Get-Content -LiteralPath $f.FullName -Raw -Encoding UTF8; $lack = @(@("Timestamp:", "Command:", "EXIT_CODE:") | Where-Object { -not [regex]::IsMatch($c, $anchor + [regex]::Escape($_)) }); if ($lack.Count -gt 0) { $missing++; Write-Output ("MISSING-FIELDS: " + $f.FullName.Substring($root.Length + 1) + " lacks " + ($lack -join ",")) } }
    Write-Output ("EVIDENCE-FILES-CHECKED: " + $files.Count)
    Write-Output ("EVIDENCE-MISSING-FIELDS: " + $missing)
    Write-Output ("FIELD-CHECK-CONTROL: " + [regex]::IsMatch("FORMAT_EXIT_CODE: 0", $anchor + [regex]::Escape("EXIT_CODE:")))
    Write-Output ("FIELD-CHECK-POSITIVE: " + [regex]::IsMatch("- **EXIT_CODE:** 0", $anchor + [regex]::Escape("EXIT_CODE:")))
    $canon = "/evidence/(baseline|regression-testing|qa-gates)/"
    Write-Output ("NONCANONICAL-SUBFOLDER-FILES: " + @($files | Where-Object { $_.FullName.Substring($root.Length + 1).Replace([string][char]92, "/") -notmatch $canon }).Count)
    $files | Where-Object { $_.FullName.Substring($root.Length + 1).Replace([string][char]92, "/") -notmatch $canon } | ForEach-Object { Write-Output ("NONCANONICAL: " + $_.FullName.Substring($root.Length + 1)) }
    Write-Output ("SUBFOLDER-CHECK-FLAGS-OTHER: " + ("docs\features\active\x\evidence\other\a.md".Replace([string][char]92, "/") -notmatch $canon))
    Write-Output ("SUBFOLDER-CHECK-FLAGS-QA-GATES: " + ("docs\features\active\x\evidence\qa-gates\a.md".Replace([string][char]92, "/") -notmatch $canon))
    Write-Output ("EVIDENCE-INHERITED-SKIPPED: " + @(Get-ChildItem -LiteralPath "docs\features\active\2026-10-01-sort-email-latent-logic-defects-959\evidence" -Recurse -File | Where-Object { $inherited -contains ($_.FullName.Substring($root.Length + 1).Replace([string][char]92, "/")) }).Count)

**CMD-FOOTPRINT** (P6-T12 and its `ITERATION: 2` re-run P6-T15; union of the anchored tracked diff and the untracked porcelain paths, minus Clause A and Clause B; `MERGE-BASE` and `INHERITED` substituted, `INHERITED` being the quoted `INHERITED-CLAUSE-A:` paths of P0-T3 joined by `, `, or nothing):

    Set-Location -LiteralPath "WORKTREE"
    $tracked = @(git diff --name-only MERGE-BASE)
    $untracked = @(git status --porcelain --untracked-files=all | Where-Object { $_.StartsWith("?? ") } | ForEach-Object { $_.Substring(3) })
    $all = @(@($tracked) + @($untracked) | Sort-Object -Unique)
    $inherited = @(INHERITED)
    $write = @("UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs", "UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs", "UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs", "UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs", "UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs", "UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs", "UtilitiesCS/UtilitiesCS.csproj", "QuickFiler/Controllers/EfcDataModel.cs", "ToDoModel/Email Utilities/SortItemsToExistingFolder.cs", "UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs", "UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs", "UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs", "UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs", "UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs", "UtilitiesCS.Test/UtilitiesCS.Test.csproj", "QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs", "QuickFiler.Test/QuickFiler.Test.csproj", "docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md")
    $feature = "docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/"
    $rest = @($all | Where-Object { ($inherited -notcontains $_) -and (-not $_.StartsWith(".claude/agent-memory/")) })
    $outside = @($rest | Where-Object { ($write -notcontains $_) -and (-not $_.StartsWith($feature)) })
    $missing = @($write | Where-Object { $all -notcontains $_ })
    Write-Output ("FOOTPRINT-PATHS: " + $all.Count)
    Write-Output ("SUBTRACTED-CLAUSE-A: " + @($all | Where-Object { $inherited -contains $_ }).Count)
    Write-Output ("SUBTRACTED-CLAUSE-B: " + @($all | Where-Object { $_.StartsWith(".claude/agent-memory/") }).Count)
    Write-Output ("OUTSIDE-WRITE-SET: " + $outside.Count)
    $outside | ForEach-Object { Write-Output ("OUTSIDE: " + $_) }
    Write-Output ("WRITE-SET-MISSING: " + $missing.Count)
    $missing | ForEach-Object { Write-Output ("MISSING: " + $_) }
    Write-Output ("DELETED-PATHS: " + (@(git diff --name-status MERGE-BASE | Where-Object { $_.StartsWith("D") } | ForEach-Object { ($_ -split "\t")[-1] }) -join ","))
    Write-Output ("RAW-DOC-PATHS: " + @($all | Where-Object { $_ -match "\.(trx|coverage|coveragexml)$" -or $_ -match "cobertura[^/]*\.xml$" }).Count)
    Write-Output ("NUMSTAT-UCS: " + (@(git diff --numstat MERGE-BASE -- UtilitiesCS/UtilitiesCS.csproj) -join " "))
    Write-Output ("NUMSTAT-UCT: " + (@(git diff --numstat MERGE-BASE -- UtilitiesCS.Test/UtilitiesCS.Test.csproj) -join " "))
    Write-Output ("NUMSTAT-QFT: " + (@(git diff --numstat MERGE-BASE -- QuickFiler.Test/QuickFiler.Test.csproj) -join " "))
    Write-Output ("NUMSTAT-SPEC956: " + (@(git diff --numstat MERGE-BASE -- docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md) -join " "))
    Write-Output ("NUMSTAT-S: " + (@(git diff --numstat MERGE-BASE -- UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs) -join " "))
    Write-Output ("NUMSTAT-M: " + (@(git diff --numstat MERGE-BASE -- UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs) -join " "))
    Write-Output ("NUMSTAT-TST2: " + (@(git diff --numstat MERGE-BASE -- UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs) -join " "))

**CMD-TST-IDENTITY** (P6-T16; AC5 textual-identity evidence and, from revision 1.9, the P6-T13 documentation-comment identity; `MERGE-BASE` substituted; the lines through `TST1-REMOVED-TRYSAVE-LINES:` are unchanged since revision 1.0 and the three lines after it were added in revision 1.9; the payload is issued through the coordinator relay unchanged under the maintainer's standing approval of 2026-10-04 after the enforce-epic-worktree-removal-gate.ps1 refusal of 2026-10-03T12-56, and no identifier of it is renamed):

    Set-Location -LiteralPath "WORKTREE"
    $n2 = @(git diff --numstat MERGE-BASE -- UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs)
    Write-Output ("NUMSTAT-TST2: " + ($n2 -join " "))
    Write-Output ("TST2-DELETED-LINES: " + $(if ($n2.Count -eq 1) { ($n2[0] -split "\s+")[1] } else { "NO-DIFF" }))
    $removed1 = @(git diff MERGE-BASE -- UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs | Where-Object { $_.StartsWith("-") -and -not $_.StartsWith("---") })
    Write-Output ("TST1-REMOVED-LINES: " + $removed1.Count)
    Write-Output ("TST1-REMOVED-SANITIZE-LINES: " + @($removed1 | Where-Object { $_.Contains("SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows") }).Count)
    Write-Output ("TST1-REMOVED-TRYSAVE-LINES: " + @($removed1 | Where-Object { $_.Contains("TrySaveAttachment") -or $_.Contains("AttachmentSandboxDirectory") -or $_.Contains("mkdir:") -or $_.Contains("disk failure") }).Count)
    Write-Output ("TST1-REMOVED-DOC-LINES: " + @($removed1 | Where-Object { $_.Contains("YesNoToAllResponse tracking fields") }).Count)
    $added1 = @(git diff MERGE-BASE -- UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs | Where-Object { $_.StartsWith("+") -and -not $_.StartsWith("+++") })
    Write-Output ("TST1-ADDED-DOC-LINES: " + @($added1 | Where-Object { $_.Contains("every prompt session in AllPromptSessions") }).Count)
    Write-Output ("TST1-PORCELAIN-LINES: " + @(git status --porcelain -- UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs).Count)

**CMD-SWEEP** (P6-T16 and P6-T46; host-identifier and raw-document sweep over FEATURE including this plan; the tokens are derived at run time and never written into an artifact; file counts, not line counts):

    Set-Location -LiteralPath "WORKTREE"
    $folder = "docs\features\active\2026-10-01-sort-email-latent-logic-defects-959"
    $all = @(Get-ChildItem -LiteralPath $folder -Recurse -File)
    $account = [regex]::Escape($env:USERNAME)
    $profileLeaf = [regex]::Escape((Split-Path -Leaf $env:USERPROFILE))
    $machine = [regex]::Escape($env:COMPUTERNAME)
    $root = [regex]::Escape((Get-Location).Path)
    $b = [regex]::Escape([string][char]92)
    $hit = { param($rx) @($all | Where-Object { [regex]::IsMatch((Get-Content -LiteralPath $_.FullName -Raw -Encoding UTF8), $rx) }).Count }
    Write-Output ("FILES: " + $all.Count)
    Write-Output ("ACCOUNT-TOKEN-FILES: " + (& $hit ("(?i)\b" + $account + "\b")))
    Write-Output ("PROFILE-LEAF-FILES: " + (& $hit ("(?i)\b" + $profileLeaf + "\b")))
    Write-Output ("MACHINE-TOKEN-FILES: " + (& $hit ("(?i)\b" + $machine + "\b")))
    Write-Output ("WORKTREE-ROOT-FILES: " + (& $hit ("(?i)" + $root)))
    Write-Output ("USERS-PATH-FILES: " + (& $hit ("(?i)[a-z]:[" + $b + "/]users[" + $b + "/]")))
    Write-Output ("RAW-DOCUMENT-FILES: " + @($all | Where-Object { $_.Extension -in @(".trx", ".xml", ".coverage", ".coveragexml") }).Count)

**CMD-CONTROL-BACKUP** (P4-T13; byte copy of the fixed A before the mutation control):

    Set-Location -LiteralPath "WORKTREE"
    $p = "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs"
    $bak = "coverage\control-959\SortEmail.AttachmentSaving.fixed.bak"
    New-Item -ItemType Directory -Path "coverage\control-959" -Force | Out-Null
    Copy-Item -LiteralPath $p -Destination $bak -Force
    Write-Output ("FIX-HASH-A: " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash)
    Write-Output ("BACKUP-HASH-A: " + (Get-FileHash -Algorithm SHA256 -LiteralPath $bak).Hash)

**CMD-RESTORE** (stop-path fallback of P4-T15 only):

    Set-Location -LiteralPath "WORKTREE"
    $p = "UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs"
    $bak = "coverage\control-959\SortEmail.AttachmentSaving.fixed.bak"
    Copy-Item -LiteralPath $bak -Destination $p -Force
    Write-Output ("RESTORED-HASH-A: " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash)
    Write-Output ("BACKUP-HASH-NOW: " + (Get-FileHash -Algorithm SHA256 -LiteralPath $bak).Hash)

## Phases

Every evidence path below is under FEATURE/evidence/baseline/, FEATURE/evidence/regression-testing/ or FEATURE/evidence/qa-gates/ (Non-Overridable Evidence Path Clause; AC26 and spec D17 permit only these three subfolders). `<TS>` is the write time (Artifact filenames convention). A task that names an artifact is not complete until the artifact exists with every field its acceptance names.

### Phase 0 — Policy Reads, Preconditions, Bootstrap and Baseline Capture

- [x] [P0-T1] Read, in this exact order, CLAUDE.md, .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md, .claude/rules/quality-tiers.md, .claude/rules/csharp.md, .claude/rules/tonality.md, .claude/rules/plan-acceptance-gates.md, .claude/skills/policy-compliance-order/SKILL.md, .claude/skills/atomic-plan-contract/SKILL.md, .claude/skills/acceptance-criteria-tracking/SKILL.md, .claude/skills/evidence-and-timestamp-conventions/SKILL.md, FEATURE/issue.md, FEATURE/spec.md, FEATURE/research/2026-10-02T05-15-sort-email-latent-logic-defects-research.md, FEATURE/research/2026-10-02T05-50-sort-email-966-consolidation-research.md and this plan, and write FEATURE/evidence/baseline/phase0-instructions-read.md with `Timestamp:`, `Command:` (`Read tool over the sixteen files listed under Policy Order`), `EXIT_CODE: 0`, `Policy Order:` (the ordered list above) and one line per file giving its repository-relative path and its integer line count (the last numbered line the Read tool reports). Acceptance: the artifact exists at that exact path, carries the three fields and lists all sixteen files, each with an integer line count.

- [x] [P0-T2] Verify the full-bug preconditions read-only: run `CMD-SPEC-CHECK` (`STAGE` `base`) and write FEATURE/evidence/baseline/p0-t2-mode-preconditions.<TS>.md with `Timestamp:`, `Command:`, `EXIT_CODE:` and every printed line, recording `S956-AC-CHECKED-BASE:`, `S956-AC-UNCHECKED-BASE:` and `S956-LINES-BASE:` as copies of the three S956 count lines, and `PRE-EXISTING-EVIDENCE:` as each path already under FEATURE/evidence/ other than the P0-T1 artifact this run wrote (FEATURE/evidence/baseline/phase0-instructions-read.md), or `NONE` (not a failure; the field feeds the `INHERITED` rule of `CMD-EVIDENCE-FIELDS`). Acceptance, all seven required: `WORKMODE-LINES: 1`; `AC-HEADING-LINES: 1`, `AC-UNCHECKED: 27` and `AC-CHECKED: 0`; `USERSTORY-EXISTS: False`; `AC15-SEAM-STEP:`, `AC6-RESET-LITERAL:` and `AC25-NINETY:` each at least 1 (otherwise `AC TEXT MISMATCH`: stop; never edit spec.md here); `S956-OLD-99: 1`, `S956-149: 1` and `S956-OLD-155: 1`; `S956-NEW-99: 0`, `S956-NEW-149: 0` and `S956-NEW-155: 0` (otherwise `CR-1 ANCHOR MISMATCH`: stop); `S956-AC-CHECKED-BASE:` plus `S956-AC-UNCHECKED-BASE:` equals 17. Any other failure is `MODE PRECONDITION FAILED`: stop and report.

- [x] [P0-T3] Record the working context, the diff anchor, the inherited-path set and the pre-implementation gate readiness in FEATURE/evidence/baseline/p0-t3-worktree-context.<TS>.md (with `Timestamp:`, `Command:` listing the git commands below in order, and `EXIT_CODE:` scoped to the `git fetch origin main` invocation, equal to `FETCH-EXIT:`) using `git -C WORKTREE` invocations, one per command, in this order: `git rev-parse --abbrev-ref HEAD` (`BRANCH:`); `git rev-parse HEAD` (`BASE-SHA:`); `git fetch origin main` (`FETCH-EXIT:`, the only network command of this plan); `git rev-parse origin/main` (`ORIGIN-MAIN-SHA:`, observation); `git merge-base HEAD origin/main` (`MERGE-BASE:`, derived once here and never re-derived; P6-T12 issues the same command again as `ANCHOR-RECHECK:` and compares the output with this recorded value, an observation that never replaces it); `git merge-base --is-ancestor MERGE-BASE HEAD` (`MERGE-BASE-IS-ANCESTOR-EXIT:`); `git merge-base --is-ancestor origin/main HEAD` (`MAIN-IS-ANCESTOR-EXIT:`); `git diff --exit-code 94287369908cc920b21b0e3256314f988ad7d2f5 MERGE-BASE -- PATHS-CITED` (`PATHS-CITED` substituted verbatim from the Command Reference; `CITED-TREE-EXIT:`, which proves that every `PATHS-CITED` path (every file outside FEATURE/ and outside .claude/ that this plan cites by line or by content, except .gitignore; the three exclusions are those the Command Reference states), derived at that SHA, is identical at the anchor); `git diff --exit-code MERGE-BASE HEAD -- UtilitiesCS UtilitiesCS.Test QuickFiler QuickFiler.Test ToDoModel docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956` (`BRANCH-SOURCE-EXIT:`); `git diff --name-only MERGE-BASE` together with `git status --porcelain --untracked-files=all` (the union of their paths, taken before this task writes its own artifact, is `INHERITED-CLAUSE-A:`; record also `INHERITED-OUTSIDE-FEATURE:` as every Clause A path outside FEATURE/ and outside .claude/agent-memory/, or `NONE`); `git status --porcelain --untracked-files=all -- UtilitiesCS UtilitiesCS.Test QuickFiler QuickFiler.Test ToDoModel` (`SOURCE-PORCELAIN:`, `EMPTY` when nothing is printed). Then read artifacts/orchestration/orchestrator-state.json with the Read tool (never edit it) and record `CHECKPOINT-ISSUE-NUM:`, `CHECKPOINT-FEATURE-FOLDER:`, `CHECKPOINT-ROUTE:` (`route_id`, else `path_selected`, else `ABSENT`), `CHECKPOINT-LIFECYCLE-READY:` and `PRE-IMPLEMENTATION GATE READY:` (`YES` only when the issue number is `959`, the folder begins docs/features/active/2026-10-01-sort-email-latent-logic-defects-959, the route is not `ABSENT` and lifecycle_ready is `true`). `MAIN-IS-ANCESTOR-EXIT:` is an observation; 0 means the branch already contains origin/main, which leaves this plan valid when `CITED-TREE-EXIT: 0`. Acceptance, all nine required: `BRANCH:` equals bug/sort-email-latent-logic-defects-959 (otherwise `BRANCH MISMATCH`: stop; never create or switch branches); `FETCH-EXIT: 0`; `BASE-SHA:` and `MERGE-BASE:` are 40-character hexadecimal values and `MERGE-BASE-IS-ANCESTOR-EXIT: 0`; `CITED-TREE-EXIT: 0` (otherwise `CITED TREE ADVANCED`: stop and report, because the line citations of this plan would need re-derivation); `BRANCH-SOURCE-EXIT: 0` (otherwise `BRANCH SOURCE NOT AT BASE`: stop); `INHERITED-CLAUSE-A:` lists no Write Set path (otherwise `WRITE SET ALREADY DIRTY`: stop); `SOURCE-PORCELAIN: EMPTY`; `PRE-IMPLEMENTATION GATE READY: YES` (otherwise `PRE-IMPLEMENTATION GATE NOT SEEDED`: stop); the artifact contains no absolute path.

- [x] [P0-T4] Probe the command channel FIRST, then bootstrap the C# toolchain and record pre-edit hashes in FEATURE/evidence/baseline/p0-t4-channel-and-toolchain.<TS>.md (with `Timestamp:`, `Command:` naming the Part 1 probe and the Part 2 payload, and `EXIT_CODE:` equal to the printed `TOOL-RESTORE-EXIT:`). Part 1: run `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; Write-Output ("PROBE-OK " + (Test-Path -LiteralPath "TaskMaster.sln"))'`; `CHANNEL: COMMAND` when `PROBE-OK True` is printed, else `CHANNEL: UNAVAILABLE` with the refusal text (hygiene applied): report `CHANNEL UNAVAILABLE` and stop. Part 2, one payload: record `PRE-EDIT-HASH-<alias>:` for the fifteen files A, T, U, S, M, L, E, TD, TST1, TST2, UCS (UtilitiesCS\UtilitiesCS.csproj), UCT (UtilitiesCS.Test\UtilitiesCS.Test.csproj), QFT (QuickFiler.Test\QuickFiler.Test.csproj), SPEC956 and RUNSETTINGS (scripts\vscode\TaskMaster.cli.runsettings) as `Get-FileHash -Algorithm SHA256 -LiteralPath` values (`RUNSETTINGS-HASH:` is the last), and `RUNSETTINGS-WORKERS-LINE:` and `RUNSETTINGS-SCOPE-LINE:` as lines 5 and 6 of the runsettings file trimmed; then run `& pwsh -NoProfile -File (Join-Path (Get-Location).Path "scripts\vscode\Install-RepoDotNetSdk.ps1")`, record `SDK-MARKER:` (`Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205"`), `dotnet --version`, `dotnet tool restore`, `dotnet tool list --local`, `MSBUILD-RESOLVED:` and `VSTEST-RESOLVED:` (vswhere, `YES` or `NO`) and `dotnet-coverage --version` (when not found, run `dotnet tool install --global dotnet-coverage` and re-run `dotnet-coverage --version` in a separate invocation). Each native call in the payload (the `Install-RepoDotNetSdk.ps1` call, `dotnet --version`, `dotnet tool restore` and `dotnet tool list --local`) is preceded by `$global:LASTEXITCODE = 0` and followed by `Write-Output ("<LABEL>: " + $LASTEXITCODE)`, with the labels `SDK-INSTALL-EXIT:`, `DOTNET-VERSION-EXIT:`, `TOOL-RESTORE-EXIT:` and `TOOL-LIST-EXIT:`; the install call is `& pwsh -NoProfile -File ...`, a child pwsh process, so `SDK-INSTALL-EXIT:` is that process's exit code (1 when the script's `throw` at its line 103 fires, else 0), recorded as an observation because `SDK-MARKER:` is the gate; the two vswhere probes are observed by their `-RESOLVED:` lines and carry no exit label; `dotnet-coverage --version` prints `DOTNET-COVERAGE-EXIT:` the same way, as does its re-run in the separate invocation after an install, the artifact recording the last value printed. Acceptance, all eight required: `CHANNEL: COMMAND`; the fifteen hashes are 64-character hexadecimal values; `RUNSETTINGS-WORKERS-LINE: <Workers>0</Workers>` and `RUNSETTINGS-SCOPE-LINE: <Scope>ClassLevel</Scope>`; `SDK-MARKER: True`; `DOTNET-VERSION-EXIT: 0` and `TOOL-RESTORE-EXIT: 0`; the `csharpier` row of the tool list shows `1.2.6`; `MSBUILD-RESOLVED: YES` and `VSTEST-RESOLVED: YES`; `DOTNET-COVERAGE-EXIT: 0` with its version recorded (the version line is the discriminating half: a command that is not found raises a non-terminating error and leaves `$LASTEXITCODE` at its reset value 0).

- [x] [P0-T5] Restore NuGet packages with one payload whose statements after the `Set-Location` are `New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null`, `$env:MSBUILDDISABLENODEREUSE = "1"`, `& pwsh -NoProfile -File (Join-Path (Get-Location).Path "scripts\vscode\Invoke-Restore.ps1") 2>&1 | Tee-Object -FilePath "coverage\logs\p0-t5.restore.log"`, `Write-Output ("RESTORE_EXIT_CODE: " + $LASTEXITCODE)` and `Write-Output ("PACKAGE-DIR-COUNT: " + @(Get-ChildItem -LiteralPath "packages" -Directory).Count)`, and write FEATURE/evidence/baseline/p0-t5-nuget-restore.<TS>.md (`Command:` `pwsh -NoProfile -File scripts\vscode\Invoke-Restore.ps1` with the note that the absolute script path was resolved at run time and MSBUILDDISABLENODEREUSE was 1; `EXIT_CODE:` the printed `RESTORE_EXIT_CODE:`). Acceptance: `EXIT_CODE: 0` and `PACKAGE-DIR-COUNT:` at least 1.

- [x] [P0-T6] Capture the baseline formatting state by running `CMD-CHECK-REPO` with `TASKID` `p0-t6` and write FEATURE/evidence/baseline/p0-t6-csharpier-check.<TS>.md (`Command:` `dotnet tool run csharpier check .`; `EXIT_CODE:` the printed `CHECK_EXIT_CODE:`; `Output Summary:` the `CHECKED-LINE:` value verbatim). Acceptance: `EXIT_CODE: 0` and `CHECKED-LINE:` matches `Checked <N> files` with a positive N. A non-zero exit is `FORMAT BASELINE NOT CLEAN`: stop and report the file list; never run `format` to repair it.

- [x] [P0-T7] Capture the baseline analyzer state with `CMD-REBUILD` (analyzer `GATEARGS`, `TASKID` `p0-t7`) and write FEATURE/evidence/baseline/p0-t7-msbuild-analyzers.<TS>.md with `EXIT_CODE:` (the printed `MSBUILD_EXIT_CODE:`) and `SKIP_CORECOMPILE_LINES:`, `UCS_TEST_CSC_OUT_LINES:`, `UCS_CSC_OUT_LINES:`, `QF_CSC_OUT_LINES:`, `QFT_CSC_OUT_LINES:`, `ZERO_ERRORS_LINES:`, `WARNINGS:` (also `ANALYZE-BASELINE-WARNINGS:`), `ERRORS:` and `WRITESET_DIAGNOSTIC_LINES:` (also `ANALYZE-BASELINE-WRITESET-DIAGNOSTICS:`). Acceptance, all four required: `EXIT_CODE: 0`; `SKIP_CORECOMPILE_LINES: 0`; the four `_CSC_OUT_LINES:` values each at least 1; `ERRORS: 0`. Otherwise `ANALYZER BASELINE NOT CLEAN`: stop and report.

- [x] [P0-T8] Capture the baseline nullable state with `CMD-REBUILD` (nullable `GATEARGS`, `TASKID` `p0-t8`; no Nullable property override, no incremental Build target) and write FEATURE/evidence/baseline/p0-t8-msbuild-nullable.<TS>.md with the P0-T7 field set (`NULLABLE-BASELINE-WRITESET-DIAGNOSTICS:`) plus `UCT-DLL-EXISTS:` and `QFT-DLL-EXISTS:` (whether `ASSEMBLY-UCT` and `ASSEMBLY-QFT` exist afterwards). Acceptance, all five required: `EXIT_CODE: 0`; `SKIP_CORECOMPILE_LINES: 0`; the four `_CSC_OUT_LINES:` values each at least 1; `ERRORS: 0`; `UCT-DLL-EXISTS: True` and `QFT-DLL-EXISTS: True`. Otherwise `NULLABLE BASELINE NOT CLEAN`: stop and report.

- [x] [P0-T9] Run the stall probe once with `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-STALL`, `TASKID` `p0-t9`) and write FEATURE/evidence/baseline/p0-t9-stall-probe.<TS>.md with `EXIT_CODE:` (the printed `VSTEST_EXIT_CODE:`, or 3 when the TRX is absent), `ExpectedExitCode:` equal to the observed value when it is non-zero (this task gates nothing on the exit code), the six `SANDBOX-` lines, `TRX_PRESENT:`, `SEQUENCE_FILES:`, the `COUNTERS` line when present and every `MESSAGE` line. Record exactly one `STALL-PROBE:` line (`CLEAR` when `EXIT_CODE: 0`, `failed=0` and `SEQUENCE_FILES: 0`; otherwise `REPRODUCES`), exactly one `EXCLUSION:` line (`NONE` under `CLEAR`; the Command Reference exclusion text verbatim under `REPRODUCES`) and exactly one `COVERAGE-ROUTE: DIRECT` line. Acceptance, all four required: `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:`; every `SANDBOX-` value is `False`; the three single-value lines are present with the values the rule derives; the probe ran once.

- [x] [P0-T10] Capture the baseline scoped runs and write FEATURE/evidence/baseline/test-run-baseline.md (fixed name): first `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-SORTEMAIL`, `TASKID` `p0-t10`), whose `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:` (`RUNSETTINGS-HASH-NOW:`, the six `SANDBOX-` lines, the `COUNTERS` line, `RESULT_COUNT:`, `SEQUENCE_FILES:` and every `RESULT` line) form the artifact's record; then `CMD-VSTEST` (`ASSEMBLY-QFT`, `FILTER-EFC-ARCHIVE`, `TASKID` `p0-t10-efc`), recorded under a heading `## QuickFiler.Test baseline (FILTER-EFC-ARCHIVE)` with `EFC-VSTEST_EXIT_CODE:`, its `COUNTERS` line and every `RESULT` line. Acceptance, all six required: `EXIT_CODE: 0`; `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:`; `COUNTERS total=26 executed=26 passed=26 failed=0` (another total is `STALE ASSEMBLY OR FILTER MISMATCH`, a failure is `BASELINE NOT GREEN`: stop); the 26 `RESULT` lines are exactly `NAMES-TST1-BASE` and `NAMES-T`, each `= Passed`; `EFC-VSTEST_EXIT_CODE: 0` with `COUNTERS total=11 executed=11 passed=11 failed=0` and the eleven `RESULT` lines exactly `NAMES-EFC-ARCHIVE`, each `= Passed`; every `SANDBOX-` value is `False` and both `SEQUENCE_FILES: 0`.

- [x] [P0-T11] Capture the baseline repository-wide test and coverage run: run `CMD-COVERAGE-DIRECT` (`STAGE` `baseline`, the P0-T9 `EXCLUSION`; the empty string when P0-T9 recorded `EXCLUSION: NONE`) as a background invocation polled until its final `TRX_PRESENT:` line, then (unless branch (d0) applies) `CMD-COVERAGE-POST` (`STAGE` `baseline`) and `CMD-COVERAGE-TEXTS` (`STAGE` `baseline`, `BASELINE-HASH` `NONE`), and write FEATURE/evidence/baseline/coverage-baseline.md (fixed name) with `Timestamp:`, `Command:` (the three payloads named, route `DIRECT`, the filter applied), `EXIT_CODE:` (the printed `COLLECT_EXIT_CODE:`), `ExpectedExitCode:` equal to the observed value when it is non-zero under branch (b), and an `Output Summary:` recording `COVERAGE-ROUTE: DIRECT`, `EXCLUSION:`, the six `SANDBOX-` lines, `ASSEMBLY_COUNT:` with every `ASSEMBLY:` line, `SEQUENCE_FILES:`, `TRX_PRESENT:`, `LINE-FLOOR:`, `BRANCH-FLOOR:`, the `First-party coverage:` line with `FIRST-PARTY-LINE-PERCENT:` and `FIRST-PARTY-BRANCH-PERCENT:` (also copied as `BASELINE-FIRST-PARTY-LINE-PERCENT:` and `BASELINE-FIRST-PARTY-BRANCH-PERCENT:`), the projection verbatim between `PROJECTION-BEGIN` and `PROJECTION-END`, the summary verbatim between `SUMMARY-BEGIN` and `SUMMARY-END`, `FAILED-SET:`, `BASELINE-UCS-LINE:`, `BASELINE-UCS-BRANCH:`, `BASELINE-QF-LINE:` and `BASELINE-QF-BRANCH:` (`<covered>/<covered plus missed>` from the projection), and every line CMD-COVERAGE-TEXTS printed, with `NONEXEMPT-SET-SHA256:` copied as `BASELINE-NONEXEMPT-HASH:`. Branches, checked in the order (d0), (c), (b), (a), (d): (d0) `SEQUENCE_FILES:` above 0 or `TRX_PRESENT: False` is `COVERAGE RUN ABORTED`: stop, do not post-process, do not re-run; (c) a `NOT MET` floor is `COVERAGE FLOOR BASELINE NOT MET`: stop; (b) a non-zero exit whose `FAILED-SET:` holds only `TryAddValuesAsync_UpdatesExistingValue` (issue #780, never retried): record and complete; (a) exit 0, both floors met, empty `FAILED-SET:`: complete; (d) anything else is `COVERAGE RUN ABORTED`. Acceptance, all eight required: the projection holds `UtilitiesCS` and `QuickFiler` packages with `LINE` and `BRANCH` counters; the `First-party coverage:` line is present with `FIRST-PARTY-LINE-PERCENT:` at least 80 and `FIRST-PARTY-BRANCH-PERCENT:` at least 75; the summary's first line begins `Test run outcome:`; `EXIT_CODE:` equals its declared expectation; `SORTEMAIL-DIR-CLASSES:` at least 1 and `TRYSAVE-CLASS-FOUND: True`; `EXEMPT-LAMBDA-COUNT: 1`, `EXEMPT-ELSE-BRACE-COUNT: 1`, `EXEMPT-CATCH-BRACE-COUNT: 1` and `EXEMPT-GUARD-BRACE-COUNT: 0` (the baseline T shape); `NONEXEMPT-SET-SHA256:` is a 64-character hexadecimal value and `NONEXEMPT-COUNT:` is recorded (predicted 1); every `SANDBOX-` value is `False` and the artifact contains no absolute path. coverage\baseline-959.cobertura.xml and coverage\baseline-959.jacoco.xml stay on disk, git-ignored, for P6-T8.

- [x] [P0-T12] Census the unmodified write-set files and record the repository facts the plan relies on in FEATURE/evidence/baseline/p0-t12-pre-edit-census.<TS>.md: run `CMD-CENSUS` with `PATHS-A` and TOKENS-A, `PATHS-T` and TOKENS-T, `PATHS-U` and TOKENS-U, `PATHS-S` and TOKENS-S, `PATHS-M` and TOKENS-M, `PATHS-E` and TOKENS-E, `PATHS-TST1` and TOKENS-TST1, `PATHS-TST2` and TOKENS-TST2; then `CMD-CSPROJ`; then `CMD-GREP-FACTS` (`STAGE` `base`); then `CMD-LINES` with `PATHS-SIX`, `PATHS-E`, `PATHS-TD`, `PATHS-TST1` and `PATHS-TST2`; record every printed line (`EXIT_CODE:` scoped to the `CMD-LINES` payload, the last invocation). Acceptance, all five required: every TOTAL equals the BASE column of its TOKENS table; every `CMD-CSPROJ` row matches the BASE column; every `CMD-GREP-FACTS` line matches its `base` expectation; the census `SHA256` of each file equals its `PRE-EDIT-HASH-<alias>:` of P0-T4; the `LINES` values read A 342, T 172, U 195, S 277, M 388, L 240, E 464, TD 402, TST1 457 and TST2 375 (`Get-Content` convention; the Read-tool last-line numbers quoted in the Verified Repository Facts are one higher because every file ends with a newline). A mismatch is `PRE-EDIT CENSUS MISMATCH`: stop and report.

### Phase 1 — L1 and L3 Phase One: Regression Tests Red, Then the Two One-Line Fixes

- [x] [P1-T1] Create UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs with the Write tool, content exactly Listing L-TSC-P1 (four leading spaces stripped); then run `CMD-CENSUS` with `PATHS-TSC` and TOKENS-TSC and write FEATURE/evidence/qa-gates/p1-t1-savecase-tests-census.<TS>.md. Acceptance: every TOTAL equals the P1 column of TOKENS-TSC (`[TestMethod]` 1, `[DataTestMethod]` 2, `[DataRow(` 4, `DisplayName=` 4, `SortEmail.SaveCase(` 3, `Times.Once` 2, `Times.Never` 3, the banned tokens 0).

- [x] [P1-T2] Create UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs with the Write tool, content exactly Listing L-TAS-P1 (four leading spaces stripped); then run `CMD-CENSUS` with `PATHS-TAS` and TOKENS-TAS and write FEATURE/evidence/qa-gates/p1-t2-attachmentsaving-tests-census.<TS>.md. Acceptance: every TOTAL equals the P1 column of TOKENS-TAS (`[DataTestMethod]` 1, `[DataRow(` 4, `Cleanup_Files_ResetsEveryPromptAnswerField` 5 (the declaration and the four `DisplayName` strings), `SortEmail.Cleanup_Files();` 1, `field.SetValue(null,YesNoToAllResponse.YesToAll);` 1, `[TestMethod]` 0, the banned tokens 0).

- [x] [P1-T3] Register the two new test files in UtilitiesCS.Test/UtilitiesCS.Test.csproj with Edit E-UCT-CSPROJ-1, then run `CMD-CSPROJ`, `git diff --numstat MERGE-BASE -- UtilitiesCS.Test/UtilitiesCS.Test.csproj` and `git status --porcelain -- UtilitiesCS.Test/UtilitiesCS.Test.csproj`, and write FEATURE/evidence/qa-gates/p1-t3-test-csproj.<TS>.md. Acceptance, all three required: every UCT row matches the "after P1-T3" column (SaveCase 100, AttachmentSaving 101, FilterOlFoldersController_Tests 102, UndoAndMoveLog COUNT=0); the numstat line reads `2`, `0` and the path; the porcelain line shows the file modified.

- [x] [P1-T4] Format the two new test files with `CMD-SCOPED-FORMAT` (`PATHS-TSC`, `PATHS-TAS`; `TASKID` `p1-t4`) and write FEATURE/evidence/qa-gates/p1-t4-scoped-format.<TS>.md with `Timestamp:`, `Command:` (the format and the check), `EXIT_CODE:` scoped to the `csharpier check` invocation (the printed `CHECK_EXIT_CODE:`), the BEFORE and AFTER hashes, `FORMAT_EXIT_CODE:`, `CHECK_EXIT_CODE:` and the formatter's summary line as an observation. Acceptance: `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`.

- [x] [P1-T5] Build the test project with the new tests against the unmodified production tree: `CMD-BUILD-TEST` (`PROJECT` `UtilitiesCS.Test`, `TASKID` `p1-t5`), preceded in the same artifact by `CMD-HASH` on `PATHS-A` (`A-HASH:`), and write FEATURE/evidence/qa-gates/p1-t5-test-build.<TS>.md (`EXIT_CODE:` scoped to the `CMD-BUILD-TEST` invocation, the printed `MSBUILD_EXIT_CODE:`). Acceptance, all four required: `A-HASH:` equals `PRE-EDIT-HASH-A:` of P0-T4 (production untouched); `MSBUILD_EXIT_CODE: 0` and `ERROR_LINES: 0`; `CSC_OUT_LINES:` and `ZERO_ERRORS_LINES:` each at least 1; `DLL_ADVANCED: True`. The L1 and L3 tests compile against the current tree because `SaveCase` is internal and the enum fields are read by reflection; a build error is `TEST BUILD NOT GREEN`: stop and report the error lines.

- [x] [P1-T6] [expect-fail] Observe L1 red: run `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-SAVECASE`, `TASKID` `p1-t6`) and write FEATURE/evidence/regression-testing/fail-before-save-case.md (fixed name) with `Timestamp:`, `Command:`, `EXIT_CODE:` (the printed `VSTEST_EXIT_CODE:`), `ExpectedExitCode:` equal to that observed non-zero value, and an `Output Summary:` with the six `SANDBOX-` lines, the `COUNTERS` line, `RESULT_COUNT:`, every `RESULT` line and every `MESSAGE` line (hygiene applied). Acceptance, all five required: `EXIT_CODE:` is non-zero and equals `ExpectedExitCode:`; `COUNTERS total=5 executed=5 passed=1 failed=4`; the `Failed` rows are exactly the four rows of `SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath` and `SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath` and the `Passed` row is `SaveCase_WhenAnswerIsEmpty_DoesNotSave` (`NAMES-TSC-P1`); every `MESSAGE` line contains `but was 0 times`; every `SANDBOX-` value is `False`. A green run is `FAIL-BEFORE NOT OBSERVED`; any other failing set or message is `FAIL-BEFORE WRONG REASON`: stop and report.

- [x] [P1-T7] [expect-fail] Observe L3 phase one red: run `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-ATTSAVE`, `TASKID` `p1-t7`) and write FEATURE/evidence/regression-testing/fail-before-cleanup-files-phase-one.md (fixed name) with the P1-T6 field set. Acceptance, all five required: `EXIT_CODE:` is non-zero and equals `ExpectedExitCode:`; `COUNTERS total=4 executed=4 passed=3 failed=1`; the `Failed` row is exactly `Cleanup_Files_ResetsEveryPromptAnswerField [_attachmentsAltName]` and the three `Passed` rows are the other `NAMES-TAS-P1` rows; the `MESSAGE` line contains `but found` and `YesToAll`; every `SANDBOX-` value is `False`. Otherwise `FAIL-BEFORE NOT OBSERVED` or `FAIL-BEFORE WRONG REASON`: stop and report.

- [x] [P1-T8] Apply the L1 fix (D1) to UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs with Edit E-A-L1 (the four stacked single-value labels; the attribute above `SaveCase` removed in the same Edit), then run `CMD-CENSUS` with `PATHS-A` and TOKENS-A and write FEATURE/evidence/qa-gates/p1-t8-l1-census.<TS>.md. Acceptance, all three required: `caseYesNoToAllResponse.NoToAll:`, `caseYesNoToAllResponse.No:`, `caseYesNoToAllResponse.Yes:` and `caseYesNoToAllResponse.YesToAll:` each 1 and `\|YesNoToAllResponse.` 0 and `HasFlag` 0; `[ExcludeFromCodeCoverage]` 9; every other TOKENS-A total equals its BASE value (in particular `_attachmentsAltName=YesNoToAllResponse.Empty;` 2, the field initializer and the `SaveCaseAsync` reset, not yet changed).

- [x] [P1-T9] Apply the L3 phase-one fix (D3) to the same file with Edit E-A-L3, then run `CMD-CENSUS` with `PATHS-A` and TOKENS-A and write FEATURE/evidence/qa-gates/p1-t9-l3-census.<TS>.md. Acceptance: every TOKENS-A total equals the P1 column (`_attachmentsAltName=YesNoToAllResponse.Empty;` 3, `[ExcludeFromCodeCoverage]` 9, the four stacked labels 1 each, `YesNoToAllResponse_` 4, `YesNoToAll.ShowDialog(` 6).

- [x] [P1-T10] Format A with `CMD-SCOPED-FORMAT` (`PATHS-A`; `TASKID` `p1-t10`) and build with `CMD-BUILD-TEST` (`PROJECT` `UtilitiesCS.Test`, `TASKID` `p1-t10`), and write FEATURE/evidence/qa-gates/p1-t10-format-and-build.<TS>.md with both outputs (`EXIT_CODE:` scoped to the `CMD-BUILD-TEST` invocation, the printed `MSBUILD_EXIT_CODE:`). Acceptance, all three required: `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`; `MSBUILD_EXIT_CODE: 0` and `ERROR_LINES: 0`; `DLL_ADVANCED: True`.

- [x] [P1-T11] Observe L1 green: run `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-SAVECASE`, `TASKID` `p1-t11`) and append to FEATURE/evidence/regression-testing/fail-before-save-case.md a section headed `## Pass-after (P1-T11)` with `PASS-AFTER-VSTEST_EXIT_CODE:`, the six `SANDBOX-` lines, the `COUNTERS` line and every `RESULT` line (the artifact's `EXIT_CODE:` row stays scoped to the red run). Acceptance, all three required: `PASS-AFTER-VSTEST_EXIT_CODE: 0`; `COUNTERS total=5 executed=5 passed=5 failed=0` with the five `RESULT` rows exactly `NAMES-TSC-P1`, each `= Passed`; every `SANDBOX-` value is `False`.

- [x] [P1-T12] Observe L3 phase one green: run `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-ATTSAVE`, `TASKID` `p1-t12`) and append to FEATURE/evidence/regression-testing/fail-before-cleanup-files-phase-one.md a section headed `## Pass-after (P1-T12)` with the P1-T11 field set. Acceptance, all three required: `PASS-AFTER-VSTEST_EXIT_CODE: 0`; `COUNTERS total=4 executed=4 passed=4 failed=0` with the four `RESULT` rows exactly `NAMES-TAS-P1`, each `= Passed`; every `SANDBOX-` value is `False`.

### Phase 2 — L4 and the Header: Seam, Tests Red, Fix, SanitizeArray Removed

- [x] [P2-T1] Create UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs with the Write tool, content exactly Listing L-TUL (four leading spaces stripped); then run `CMD-CENSUS` with `PATHS-TUL` and TOKENS-TUL and write FEATURE/evidence/qa-gates/p2-t1-undo-tests-census.<TS>.md. Acceptance: every TOTAL equals the TOKENS-TUL expectation (`[TestMethod]` 2, `SortEmail.WriteCSV_StartNewFileIfDoesNotExist(` 2, `Path.Combine(LogFolder,LogFileName)` 1, `HaveCount(13)` 1, `Triage\tFolderName\tSent_On` 1, the banned tokens 0).

- [x] [P2-T2] Register the file in UtilitiesCS.Test/UtilitiesCS.Test.csproj with Edit E-UCT-CSPROJ-2, then run `CMD-CSPROJ`, `git diff --numstat MERGE-BASE -- UtilitiesCS.Test/UtilitiesCS.Test.csproj` and `git status --porcelain -- UtilitiesCS.Test/UtilitiesCS.Test.csproj`, and write FEATURE/evidence/qa-gates/p2-t2-test-csproj.<TS>.md. Acceptance, all three required: every UCT row matches the "after P2-T2" column (SaveCase 100, AttachmentSaving 101, UndoAndMoveLog 102, FilterOlFoldersController_Tests 103); the numstat line reads `3`, `0` and the path; the porcelain line shows the file modified. P2-T1 to P2-T5 (like P4-T1 to P4-T7 and P5-T4 to P5-T6) form a compile-red span: the TUL listing, registered by this task, calls the four-parameter `WriteCSV_StartNewFileIfDoesNotExist` seam that P2-T3 lands and P2-T5 first builds green (P2-T4 only formats). The executor creates no commit inside a span. If a checkpoint commit is required inside one, the executor reports `COMPILE-RED SPAN OPEN` with the last completed task and the files that do not compile.

- [x] [P2-T3] Land the L4 seam as a pure forward of the current body (D4 red-first pre-step) in UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs with Edits E-U-SEAM-HEAD then E-U-SEAM-WRITE, then run `CMD-CENSUS` with `PATHS-U` and TOKENS-U and write FEATURE/evidence/qa-gates/p2-t3-undo-seam-census.<TS>.md. Acceptance: every TOKENS-U total equals the SEAM column (in particular `Path.Combine(strFileName,strFileLocation)` 1, `SanitizeArray(` 2, `string[14,2]` 1, `Func<string,bool>fileExists` 1, `Action<string,string[],string>writeTextFile` 1, `writeTextFile(` 1, `internalstaticvoidWriteCSV_StartNewFileIfDoesNotExist(` 1, `[ExcludeFromCodeCoverage]` 6).

- [x] [P2-T4] Format U and TUL with `CMD-SCOPED-FORMAT` (`PATHS-U`, `PATHS-TUL`; `TASKID` `p2-t4`) and write FEATURE/evidence/qa-gates/p2-t4-scoped-format.<TS>.md (`EXIT_CODE:` scoped to the `csharpier check` invocation, the printed `CHECK_EXIT_CODE:`). Acceptance: `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`.

- [x] [P2-T5] Build the test project in the seam state: `CMD-BUILD-TEST` (`PROJECT` `UtilitiesCS.Test`, `TASKID` `p2-t5`) and write FEATURE/evidence/qa-gates/p2-t5-test-build.<TS>.md (`EXIT_CODE:` the printed `MSBUILD_EXIT_CODE:`). Acceptance, all three required: `MSBUILD_EXIT_CODE: 0` and `ERROR_LINES: 0`; `CSC_OUT_LINES:` at least 1; `DLL_ADVANCED: True`.

- [x] [P2-T6] [expect-fail] Observe L4 red against the seam state: run `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-UNDO`, `TASKID` `p2-t6`) and write FEATURE/evidence/regression-testing/fail-before-write-csv.md (fixed name) with the P1-T6 field set. Acceptance, all five required: `EXIT_CODE:` is non-zero and equals `ExpectedExitCode:`; `COUNTERS total=2 executed=2 passed=0 failed=2`; the two `Failed` rows are exactly `NAMES-TUL`; the `MESSAGE` of `WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader` contains `differs at index 0` and the `MESSAGE` of `WriteCSV_WhenFileExists_DoesNotWrite` contains `NullReferenceException` or `but found 1`; every `SANDBOX-` value is `False`. Otherwise `FAIL-BEFORE NOT OBSERVED` or `FAIL-BEFORE WRONG REASON`: stop and report.

- [x] [P2-T7] Rewrite UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs with the Write tool (after reading it), content exactly Listing L-U-FINAL (four leading spaces stripped): the corrected `Path.Combine` order and condition, the single tab-joined header from `MovedMailsHeader`, `SanitizeArray` deleted, the exclusion removed from `SanitizeArrayLineTSV`, the final using block (D4, D8, D10). Run `CMD-CENSUS` with `PATHS-U` and TOKENS-U and `CMD-USINGS`, and write FEATURE/evidence/qa-gates/p2-t7-undo-final-census.<TS>.md. Acceptance, all three required: every TOKENS-U total equals the FINAL column; `USINGS U count=10 exact=True firstline=True blankafter=True`; `LINES` for the file is at most 499.

- [x] [P2-T8] Delete the `SanitizeArray` reflection test from UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs with Edit E-TST1-DEL-SANITIZE, then run `CMD-CENSUS` with `PATHS-TST1` and TOKENS-TST1 and write FEATURE/evidence/qa-gates/p2-t8-tst1-census.<TS>.md. Acceptance: every TOKENS-TST1 total equals the MID column (`[TestMethod]` 14, `SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows` 0, `"SanitizeArray"` 0, `"SanitizeArrayLineTSV"` 1, the two try-save test names 1 each).

- [x] [P2-T9] Format U and TST1 with `CMD-SCOPED-FORMAT` (`PATHS-U`, `PATHS-TST1`; `TASKID` `p2-t9`) and build with `CMD-BUILD-TEST` (`PROJECT` `UtilitiesCS.Test`, `TASKID` `p2-t9`), and write FEATURE/evidence/qa-gates/p2-t9-format-and-build.<TS>.md (`EXIT_CODE:` scoped to the `CMD-BUILD-TEST` invocation, the printed `MSBUILD_EXIT_CODE:`). Acceptance, all three required: `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`; `MSBUILD_EXIT_CODE: 0` and `ERROR_LINES: 0` (a `CS0103` naming `SanitizeArray` would mean a surviving caller: stop and report); `DLL_ADVANCED: True`.

- [x] [P2-T10] Observe L4 green: run `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-UNDO`, `TASKID` `p2-t10`) and append to FEATURE/evidence/regression-testing/fail-before-write-csv.md a section headed `## Pass-after (P2-T10)` with the P1-T11 field set. Acceptance, all three required: `PASS-AFTER-VSTEST_EXIT_CODE: 0`; `COUNTERS total=2 executed=2 passed=2 failed=0` with the two `RESULT` rows exactly `NAMES-TUL`, each `= Passed`; every `SANDBOX-` value is `False`.

- [x] [P2-T11] Confirm the TST1 pins after the deletion: run `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-TST1`, `TASKID` `p2-t11`) and write FEATURE/evidence/regression-testing/p2-t11-tst1-run.<TS>.md with the `CMD-VSTEST` field set. Acceptance, all three required: `EXIT_CODE: 0`; `COUNTERS total=14 executed=14 passed=14 failed=0` with the fourteen `RESULT` rows exactly `NAMES-TST1-MID`, each `= Passed`; every `SANDBOX-` value is `False` and `SEQUENCE_FILES: 0`.

### Phase 3 — L2 and the Try-Save Path: T12 Red, Then the Bounded Private Core

- [x] [P3-T1] Add the tripwire and T12 to UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs with Edits E-TST2-TRIPWIRE then E-TST2-T12 (every pre-existing line is kept; both Edits only insert), then run `CMD-CENSUS` with `PATHS-TST2` and TOKENS-TST2 and `git diff --numstat MERGE-BASE -- UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` together with `git status --porcelain -- UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs`, and write FEATURE/evidence/qa-gates/p3-t1-trysave-tests-census.<TS>.md. Acceptance, all three required: every TOKENS-TST2 total equals the FINAL column (`[TestMethod]` 12, `CreateDirectoryLimit` 3, the T12 name 1, `.Throws(denied)` 1, `BeSameAs(denied)` 1, `Times.Exactly(2)` 5, `newSeams(` 12, `SetupSequence` 11, the banned tokens 0); the numstat line reads a positive added count, `0` deleted and the path (AC5: the eleven pre-existing methods are textually unchanged); the porcelain line shows the file modified.

- [x] [P3-T2] Format TST2 with `CMD-SCOPED-FORMAT` (`PATHS-TST2`; `TASKID` `p3-t2`) and build with `CMD-BUILD-TEST` (`PROJECT` `UtilitiesCS.Test`, `TASKID` `p3-t2`), and write FEATURE/evidence/qa-gates/p3-t2-format-and-build.<TS>.md (`EXIT_CODE:` scoped to the `CMD-BUILD-TEST` invocation, the printed `MSBUILD_EXIT_CODE:`). Acceptance, all three required: `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`; `MSBUILD_EXIT_CODE: 0` and `ERROR_LINES: 0`; `DLL_ADVANCED: True`. The T12 test compiles against the unmodified T because it calls the existing five-argument overload.

- [x] [P3-T3] [expect-fail] Observe L2 red: run `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-TRYSAVE`, `TASKID` `p3-t3`) and write FEATURE/evidence/regression-testing/fail-before-try-save-retry.md (fixed name) with the P1-T6 field set. Acceptance, all five required: `EXIT_CODE:` is non-zero and equals `ExpectedExitCode:`; `COUNTERS total=12 executed=12 passed=11 failed=1`; the `Failed` row is exactly `TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear` and the eleven `Passed` rows are exactly `NAMES-T`; the `MESSAGE` line contains `InvalidOperationException` (the tripwire sentinel ended the unbounded retry); every `SANDBOX-` value is `False`. Otherwise `FAIL-BEFORE NOT OBSERVED` or `FAIL-BEFORE WRONG REASON`: stop and report.

- [x] [P3-T4] Rewrite UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs with the Write tool (after reading it), content exactly Listing L-T-FINAL (four leading spaces stripped): the five-argument forward, the private core with the guard, the four logger calls, the outer catch removed, the final using block (D2, D10). Run `CMD-CENSUS` with `PATHS-T` and TOKENS-T and `CMD-USINGS`, and write FEATURE/evidence/qa-gates/p3-t4-trysave-census.<TS>.md. Acceptance, all three required: every TOKENS-T total equals the FINAL column (`Debug.WriteLine(` 0, `catch(System.Exception){throw;}` 0, `catch(` 2, `throw;` 2, `TrySaveAttachmentCoreAsync(` 3, `isRetryAfterClear:true` 1, `isRetryAfterClear:false` 1, `logger.Warn(` 2, `logger.Error(` 2, `createDirectory(Path.GetDirectoryName(filePathSave));` 1, `[ExcludeFromCodeCoverage]` 2, `usingSystem.Diagnostics;` 0); `USINGS T count=5 exact=True firstline=True blankafter=True`; `LINES` for the file is at most 499.

- [x] [P3-T5] Format T with `CMD-SCOPED-FORMAT` (`PATHS-T`; `TASKID` `p3-t5`) and build with `CMD-BUILD-TEST` (`PROJECT` `UtilitiesCS.Test`, `TASKID` `p3-t5`), and write FEATURE/evidence/qa-gates/p3-t5-format-and-build.<TS>.md (`EXIT_CODE:` scoped to the `CMD-BUILD-TEST` invocation, the printed `MSBUILD_EXIT_CODE:`). Acceptance, all three required: `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`; `MSBUILD_EXIT_CODE: 0` and `ERROR_LINES: 0`; `DLL_ADVANCED: True`.

- [x] [P3-T6] Observe L2 green and the pins: run `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-TRYSAVE`, `TASKID` `p3-t6`) and then `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-TST1`, `TASKID` `p3-t6-tst1`), and append to FEATURE/evidence/regression-testing/fail-before-try-save-retry.md a section headed `## Pass-after (P3-T6)` with `PASS-AFTER-VSTEST_EXIT_CODE:`, the `COUNTERS` line and every `RESULT` line of the first run, then `TST1-VSTEST_EXIT_CODE:`, the `COUNTERS` line and every `RESULT` line of the second, and the six `SANDBOX-` lines of each. Acceptance, all four required: `PASS-AFTER-VSTEST_EXIT_CODE: 0` with `COUNTERS total=12 executed=12 passed=12 failed=0` and the twelve `RESULT` rows exactly `NAMES-T12`, each `= Passed`; `TST1-VSTEST_EXIT_CODE: 0` with `COUNTERS total=14 executed=14 passed=14 failed=0` and the rows exactly `NAMES-TST1-MID`, each `= Passed` (the two try-save pins of AC5 among them); every `SANDBOX-` value is `False`; no test was edited. A T12 failure whose `MESSAGE` names only the `BeSameAs` assertion is `T12 INSTANCE IDENTITY NOT PRESERVED`: stop and report with the message (the spec's fallback, type-and-message equality, is a planner amendment, not an executor edit).

### Phase 4 — F1 Sessions and Seams, Re-Rooting, F2 Deletions, A-Side F3 and Usings, Structural Test and Its Control

- [x] [P4-T1] Rewrite UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs with the Write tool (after reading it), content exactly Listing L-TSC-FINAL (four leading spaces stripped); then run `CMD-CENSUS` with `PATHS-TSC` and TOKENS-TSC and write FEATURE/evidence/qa-gates/p4-t1-savecase-final-census.<TS>.md. Acceptance: every TOTAL equals the FINAL column of TOKENS-TSC (`[TestMethod]` 6, `[DataTestMethod]` 3, `[DataRow(` 6, `SortEmail.SaveCase(` 3, `SortEmail.SaveCaseAsync(` 9, `newScriptedPrompt(` 6, `RecordingSave(saves)` 9, the banned tokens 0). P4-T1 to P4-T7 (and P2-T1 to P2-T5, P5-T4 to P5-T6) form a compile-red span: the final TSC and TAS listings call seams that P4-T4 lands and P4-T7 first builds green. The executor creates no commit inside a span. If a checkpoint commit is required inside one, the executor reports `COMPILE-RED SPAN OPEN` with the last completed task and the files that do not compile.

- [x] [P4-T2] Rewrite UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs with the Write tool (after reading it), content exactly Listing L-TAS-FINAL (four leading spaces stripped), which replaces the phase-one reflection test by the structural test (D3 phase F1); then run `CMD-CENSUS` with `PATHS-TAS` and TOKENS-TAS and write FEATURE/evidence/qa-gates/p4-t2-attachmentsaving-final-census.<TS>.md. Acceptance: every TOTAL equals the FINAL column of TOKENS-TAS (`[TestMethod]` 11, `Cleanup_Files_ResetsEveryPromptAnswerField` 0, `Cleanup_Files_ResetsEveryPromptSession` 1, `SetValue(` 0, `SortEmail.SaveAttachmentAsync(` 7, `SortEmail.SaveAttachment(` 3, `SortEmail.RedirectSaveFolder(` 1, `"AllPromptSessions"` 1, `HaveCount(4)` 2, the banned tokens 0).

- [x] [P4-T3] [expect-fail] Record the compile-red fail-before of the F1 tests against the pre-seam production tree: run `CMD-HASH` on `PATHS-A` (`A-HASH-BEFORE:`) then `CMD-BUILD-TEST` (`PROJECT` `UtilitiesCS.Test`, `TASKID` `p4-t3`), and write FEATURE/evidence/regression-testing/compile-red-attachment-saving-seams.md (fixed name) with `Timestamp:`, `Command:` (the CMD-BUILD-TEST canonical form), `EXIT_CODE:` (the printed `MSBUILD_EXIT_CODE:`), `ExpectedExitCode:` equal to that observed non-zero value, and an `Output Summary:` with `A-HASH-BEFORE:`, `ERROR_LINES:`, `ERROR_LINES_TEST_FILES:`, `ERROR_LINES_OTHER_FILES:`, `MISSING_SAVEATTACHMENTASYNC_6:`, `MISSING_SAVECASEASYNC_6:`, `MISSING_SAVEATTACHMENT_4:`, `MISSING_REDIRECTSAVEFOLDER:`, `ERROR_CODES:` and `DLL_ADVANCED:`. Acceptance, all six required: `EXIT_CODE:` is non-zero and equals `ExpectedExitCode:`; `A-HASH-BEFORE:` equals the `AFTER` hash of A recorded by P1-T10 (production unchanged since Phase 1); `ERROR_LINES_TEST_FILES:` at least 1 and `ERROR_LINES_OTHER_FILES: 0` (the red build is caused only by the new tests); `MISSING_SAVEATTACHMENTASYNC_6:`, `MISSING_SAVECASEASYNC_6:`, `MISSING_SAVEATTACHMENT_4:` and `MISSING_REDIRECTSAVEFOLDER:` each at least 1 (the four missing overloads named from the compiler output); `ERROR_CODES:` contains `CS1501` and `CS0117`; `DLL_ADVANCED: False`. A green build is `FAIL-BEFORE NOT OBSERVED`: stop and report.

- [x] [P4-T4] Write UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs with the Write tool (after reading it) as Listing L-A-FINAL with exactly one listing line omitted: the line whose stripped text is `attachmentHelper.FilePathHelperSaveAlt.FolderPath = destinationPath;` (the second statement of the re-rooting helper; D6 extraction step). This write lands the three sessions, `AllPromptSessions`, the `foreach` cleanup, the two excluded wrappers with their cores, the destination overload calling the one-statement helper, the six-argument `SaveCaseAsync`, the deletion of `IsPicture` and `_responseSaveFile`, the A-side F3 removals and the final using block (D5, D6 step one, D7, D8, D10). Run `CMD-CENSUS` with `PATHS-A` and TOKENS-A and `CMD-USINGS`, and write FEATURE/evidence/qa-gates/p4-t4-attachmentsaving-extract-census.<TS>.md. Acceptance, all three required: every TOKENS-A total equals the EXTRACT column (in particular `FilePathHelperSaveAlt.FolderPath=destinationPath;` 0, `FolderPathSave=destinationPath;` 1, `RedirectSaveFolder(` 2, `new(YesNoToAll.ShowDialog)` 3, `AllPromptSessions` 2, `YesNoToAllResponse_` 0, `YesNoToAll.ShowDialog(` 0, `IsPicture` 0, `_responseSaveFile` 0, `[ExcludeFromCodeCoverage]` 3, `TrySaveAttachmentAsync` 1, `SaveCaseAsync(` 2); `USINGS A count=8 exact=True firstline=True blankafter=True`; `LINES` for the file is at most 499.

- [x] [P4-T5] Delete the legacy partial and its project entry (D7): run `CMD-DELETE` with `PATH` `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs`, then apply Edit E-UCS-CSPROJ-REMOVE to UtilitiesCS/UtilitiesCS.csproj, then run `CMD-CSPROJ`, `git diff --numstat MERGE-BASE -- UtilitiesCS/UtilitiesCS.csproj` and `git status --porcelain -- UtilitiesCS/UtilitiesCS.csproj UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs`, and write FEATURE/evidence/qa-gates/p4-t5-legacy-deletion.<TS>.md with every printed line (`EXIT_CODE:` scoped to the `CMD-DELETE` payload, its process exit code). Acceptance, all four required: `EXISTS-BEFORE: True` and `EXISTS-AFTER: False`; every UCS row matches the "after P4-T5" column (LegacyAttachmentSaving COUNT=0, MailItemSort 820, TrySaveAttachment 821, UndoAndMoveLog 822, FolderPredictor 823); the numstat line reads `0`, `1` and the path; the porcelain output shows the project file modified and the deleted file as ` D`. A tool refusal of the deletion payload is `DELETE CHANNEL REFUSED`: stop and report.

- [x] [P4-T6] Turn the two `GetAttachmentsInfo` tests of UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs into three-row data tests (D8, PD-6) with Edits E-TST1-ROWS-SYNC then E-TST1-ROWS-ASYNC, then run `CMD-CENSUS` with `PATHS-TST1` and TOKENS-TST1 and write FEATURE/evidence/qa-gates/p4-t6-tst1-rows-census.<TS>.md. Acceptance: every TOKENS-TST1 total equals the FINAL column (`[TestMethod]` 12, `[DataTestMethod]` 2, `[DataRow(` 6, `DisplayName=` 6, `saveAttachments:false,savePictures:true` 0, `saveAttachments:saveAttachments,savePictures:savePictures` 2, `"photo.jpg,report.pdf"` 2, the two try-save test names 1 each, `C:\Sortemail945Sandbox` 1).

- [x] [P4-T7] Replace the generic try-save seam by the nested delegate type (PD-14; the first attempt of this task stopped at `P4-T7 BUILD RED (CS1769)`, FEATURE/evidence/qa-gates/p4-t7-format-and-build.2026-10-03T10-21.md): write UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs with the Write tool (after reading it) as Listing L-A-FINAL with exactly one listing line omitted, the same line P4-T4 omitted (stripped text `attachmentHelper.FilePathHelperSaveAlt.FolderPath = destinationPath;`); write UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs with the Write tool (after reading it) as Listing L-TSC-FINAL; write UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs with the Write tool (after reading it) as Listing L-TAS-FINAL; then run `CMD-CENSUS` three times (`PATHS-A` with TOKENS-A, `PATHS-TSC` with TOKENS-TSC, `PATHS-TAS` with TOKENS-TAS); then format A, TSC, TAS and TST1 with `CMD-SCOPED-FORMAT` (`PATHS-A`, `PATHS-TSC`, `PATHS-TAS`, `PATHS-TST1`; `TASKID` `p4-t7`) and build with `CMD-BUILD-TEST` (`PROJECT` `UtilitiesCS.Test`, `TASKID` `p4-t7`), and write FEATURE/evidence/qa-gates/p4-t7-format-and-build.<TS>.md with `ITERATION: 2` (it supersedes the 2026-10-03T10-21 stop record, which stays on disk unchanged) and every printed line (`EXIT_CODE:` scoped to the `CMD-BUILD-TEST` invocation, the printed `MSBUILD_EXIT_CODE:`). Acceptance, all five required: every TOKENS-A total equals the SEAMED column (in particular `Func<Attachment,string,Task<bool>>trySave` 0, `TrySaveAttachmentDelegatetrySave` 2, `delegateTask<bool>TrySaveAttachmentDelegate(` 1, `TrySaveAttachmentAsync` 1, `SaveCaseAsync(` 2, `FilePathHelperSaveAlt.FolderPath=destinationPath;` 0, `[ExcludeFromCodeCoverage]` 3) and `LINES` for A is at most 499; every TOKENS-TSC total equals its FINAL column and every TOKENS-TAS total equals its FINAL column (in particular `SortEmail.TrySaveAttachmentDelegate` 1 and `Func<Attachment,string,Task<bool>>` 0 in each file, `RecordingSave(saves)` 9 and `SortEmail.SaveCaseAsync(` 9 in TSC, `SortEmail.SaveAttachmentAsync(` 7 and `Cleanup_Files_ResetsEveryPromptSession` 1 in TAS); `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`; `MSBUILD_EXIT_CODE: 0` and `ERROR_LINES: 0` (stop vocabulary, each reported with the error lines and resolved only by a planner amendment: an `ERROR_CODES:` value containing `CS1769` means a generic instantiation over an embedded interop type still crosses the assembly boundary, `P4-T7 BUILD RED (CS1769)`; a `CS0121` naming `TrySaveAttachmentAsync` means the method-group conversion to `TrySaveAttachmentDelegate` is ambiguous and the spec's cast fallback is the amendment; any other code is `P4-T7 BUILD RED` with that code); `DLL_ADVANCED: True`. This task closes the compile-red span P4-T1 to P4-T7; its per-task commit stages A, TSC, TAS, TST1 and FEATURE/ with no span suffix.

- [x] [P4-T8] Observe the SaveCase and TST1 suites green on the seamed tree: run `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-SAVECASE`, `TASKID` `p4-t8`) and `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-TST1`, `TASKID` `p4-t8-tst1`), and write FEATURE/evidence/regression-testing/p4-t8-savecase-and-tst1-runs.<TS>.md with both `CMD-VSTEST` field sets (`EXIT_CODE:` scoped to the first run, `TST1-VSTEST_EXIT_CODE:` for the second). Acceptance, all four required: `EXIT_CODE: 0` with `COUNTERS total=12 executed=12 passed=12 failed=0` and the rows exactly `NAMES-TSC-FINAL`, each `= Passed`; `TST1-VSTEST_EXIT_CODE: 0` with `COUNTERS total=18 executed=18 passed=18 failed=0` and the rows exactly `NAMES-TST1-FINAL`, each `= Passed`; every `SANDBOX-` value is `False`; both `SEQUENCE_FILES: 0`.

- [x] [P4-T9] [expect-fail] Observe the re-rooting defect red after the extraction step (the first attempt of this task stopped at `FAIL-BEFORE WRONG REASON` on 2026-10-03T11-28 because its fourth clause asserted the full origin-folder literal in the failed row's message, which FluentAssertions never prints for a long string difference; revision 1.8 restates the clause against the observed message, and this re-run is the planner-amended re-run the Stop discipline admits, as P4-T7's was): run `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-ATTSAVE`, `TASKID` `p4-t9`) and rewrite FEATURE/evidence/regression-testing/fail-before-redirect-save-folder.md (fixed name) in full with the Write tool after reading it, with the P1-T6 field set followed by `ITERATION: 2` after the four schema rows (the 2026-10-03T11-28 stop record is superseded by this rewrite and survives only in its stop commit, so no later reader of the fixed name, P4-T11, P6-T17, P6-T29 or P6-T38, meets its `STOP` heading or its `NOT MET` item). Acceptance, all five required: `EXIT_CODE:` is non-zero and equals `ExpectedExitCode:`; `COUNTERS total=11 executed=11 passed=10 failed=1`; the `Failed` row is exactly `RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths` and the ten `Passed` rows are the other `NAMES-TAS-FINAL` names (the F1 cores and the structural test already pass); the `MESSAGE` entry of the failed row (the transcribed TRX message text, which spans several artifact lines) contains all three backslash-free tokens `GetDirectoryName(helper.FilePathSaveAlt)`, `origin"` and `destination"` (the observed success-case message of the 2026-10-03T11-28 run opens with `Path.GetDirectoryName(helper.FilePathSaveAlt)` and `differs at index 23`, then prints the actual and the expected string each truncated to a window after an ellipsis, the actual ending `origin"` and the expected ending `destination"`; a failure of the primary-path assertion would name `GetDirectoryName(helper.FilePathSave)` with no `Alt`, and a failure of either file-name assertion would name `GetFileName(`, so the first token is absent for every reason other than the alternate path's directory still being the origin folder); every `SANDBOX-` value is `False`. Otherwise `FAIL-BEFORE NOT OBSERVED` or `FAIL-BEFORE WRONG REASON`: stop and report.

- [x] [P4-T10] Apply the second re-rooting statement (D6 step two) to UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs with Edit E-A-RR-SECOND, then run `CMD-CENSUS` with `PATHS-A` and TOKENS-A, then `CMD-SCOPED-FORMAT` (`PATHS-A`; `TASKID` `p4-t10`) and `CMD-BUILD-TEST` (`PROJECT` `UtilitiesCS.Test`, `TASKID` `p4-t10`), and write FEATURE/evidence/qa-gates/p4-t10-rr-fix.<TS>.md with every printed line (`EXIT_CODE:` scoped to the `CMD-BUILD-TEST` invocation, the printed `MSBUILD_EXIT_CODE:`). Acceptance, all four required: every TOKENS-A total equals the FINAL column (`FilePathHelperSaveAlt.FolderPath=destinationPath;` 1, every other value as in SEAMED); `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`; `MSBUILD_EXIT_CODE: 0` and `ERROR_LINES: 0`; `DLL_ADVANCED: True`.

- [x] [P4-T11] Observe the re-rooting test green: run `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-ATTSAVE`, `TASKID` `p4-t11`) and append to FEATURE/evidence/regression-testing/fail-before-redirect-save-folder.md a section headed `## Pass-after (P4-T11)` with the P1-T11 field set. Acceptance, all three required: `PASS-AFTER-VSTEST_EXIT_CODE: 0`; `COUNTERS total=11 executed=11 passed=11 failed=0` with the eleven `RESULT` rows exactly `NAMES-TAS-FINAL`, each `= Passed`; every `SANDBOX-` value is `False`.

- [x] [P4-T12] Run the whole SortEmail family: `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-SORTEMAIL`, `TASKID` `p4-t12`) and write FEATURE/evidence/regression-testing/p4-t12-sortemail-family-run.<TS>.md with the `CMD-VSTEST` field set. Acceptance, all three required: `EXIT_CODE: 0`; `COUNTERS total=55 executed=55 passed=55 failed=0` with the fifty-five `RESULT` rows exactly the union of `NAMES-TST1-FINAL`, `NAMES-T12`, `NAMES-TSC-FINAL`, `NAMES-TAS-FINAL` and `NAMES-TUL`, each `= Passed`; every `SANDBOX-` value is `False` and `SEQUENCE_FILES: 0`.

- [x] [P4-T13] Back up the fixed A before the structural-test control (PD-7): run `CMD-CONTROL-BACKUP` and write FEATURE/evidence/regression-testing/p4-t13-control-backup.<TS>.md with `Timestamp:`, `Command:` (`CMD-CONTROL-BACKUP`), `EXIT_CODE:` (the payload's process exit code; the payload prints no exit label), `FIX-HASH-A:` and `BACKUP-HASH-A:`. Acceptance, both required: `BACKUP-HASH-A:` equals `FIX-HASH-A:`; `FIX-HASH-A:` equals the `AFTER` hash of A recorded by P4-T10.

- [x] [P4-T14] [expect-fail] Apply the mutation control (PD-7, D3 phase F1): apply Edit E-A-CONTROL-MUTATE to UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs (one element removed from `AllPromptSessions`), run `CMD-CENSUS` with `PATHS-A` and TOKENS-A, `CMD-BUILD-TEST` (`PROJECT` `UtilitiesCS.Test`, `TASKID` `p4-t14`) and `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-ATTSAVE`, `TASKID` `p4-t14`), and write FEATURE/evidence/regression-testing/p4-t14-control-applied.<TS>.md with `Timestamp:`, `Command:` (the three payloads), `EXIT_CODE:` scoped to the vstest invocation (the printed `VSTEST_EXIT_CODE:`), `ExpectedExitCode:` equal to that observed non-zero value, and an `Output Summary:` with the census `SHA256` of A (`MUTATED-HASH-A:`), `MSBUILD_EXIT_CODE:`, `ERROR_LINES:`, the `COUNTERS` line, every `RESULT` line, every `MESSAGE` line (hygiene applied) and the six `SANDBOX-` lines. Acceptance, all five required: `MUTATED-HASH-A:` differs from `FIX-HASH-A:` and every TOKENS-A total equals the FINAL column except `AllPromptSessions` 2 unchanged and the array now holding three elements (observed by the Edit); `MSBUILD_EXIT_CODE: 0` and `ERROR_LINES: 0`; `COUNTERS total=11 executed=11 passed=10 failed=1` with the `Failed` row exactly `Cleanup_Files_ResetsEveryPromptSession` and its `MESSAGE` containing `but found 3`; `EXIT_CODE:` is non-zero and equals `ExpectedExitCode:`; every `SANDBOX-` value is `False`. Any other outcome is `MUTATION PREDICTION MISMATCH`: run P4-T15, then stop and report. If the run stops at any point before P4-T15 completes, the executor's last action is the P4-T15 restore and the stop report states the restored hash. Per-task commit exception: this task's commit stages FEATURE/ only (`git add -A -- docs/features/active/2026-10-01-sort-email-latent-logic-defects-959`), so the mutated A is never committed.

- [x] [P4-T15] Restore A with Edit E-A-CONTROL-RESTORE, then run `CMD-HASH` on `PATHS-A` and record `RESTORED-HASH-A:` with `RESTORE-ROUTE: EDIT`; only when that hash differs from `FIX-HASH-A:`, run `CMD-RESTORE` and record its `RESTORED-HASH-A:` with `RESTORE-ROUTE: COPY`. Then run `CMD-BUILD-TEST` (`PROJECT` `UtilitiesCS.Test`, `TASKID` `p4-t15`) and `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-ATTSAVE`, `TASKID` `p4-t15`), and write FEATURE/evidence/regression-testing/p4-t15-control-restored.<TS>.md with every printed line (`EXIT_CODE:` scoped to the `CMD-VSTEST` invocation, the printed `VSTEST_EXIT_CODE:`; the build's code stays under `MSBUILD_EXIT_CODE:`). Acceptance, all four required: the final `RESTORED-HASH-A:` equals `FIX-HASH-A:` (otherwise `CONTROL RESTORE MISMATCH`: stop and report); `MSBUILD_EXIT_CODE: 0` and `DLL_ADVANCED: True`; `EXIT_CODE: 0` with `COUNTERS total=11 executed=11 passed=11 failed=0` and the rows exactly `NAMES-TAS-FINAL`, each `= Passed`; every `SANDBOX-` value is `False`. Per-task commit: this task's commit stages A (restored, identical to the fix committed by P4-T10, so it contributes no diff) and FEATURE/.

### Phase 5 — S and M Usings, EfcDataModel Try/Finally, ToDoModel Deletion, CR-1 and the Fail-Before Dossier

- [x] [P5-T1] Replace the using block of UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs with Edit E-S-USINGS (D10), then run `CMD-CENSUS` with `PATHS-S` and TOKENS-S and write FEATURE/evidence/qa-gates/p5-t1-sortemail-usings.<TS>.md. Acceptance: every TOKENS-S total equals the S FINAL column (`usingDeedle;`, `usingSDILReader;`, `usingOutlook=`, `usingUtilitiesCS;`, `usingSystem.Diagnostics;`, `usingSystem.Text.RegularExpressions;`, `usingSystem.Windows.Forms;`, `usingUtilitiesCS.EmailIntelligence;` and `usingUtilitiesCS.ReusableTypeClasses` each 0; `usingSystem;`, `usingUtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;` and `usingUtilitiesCS.OutlookExtensions;` each 1; `[ExcludeFromCodeCoverage]` 4).

- [x] [P5-T2] Replace the using block of UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs with Edit E-M-USINGS (D10), then run `CMD-CENSUS` with `PATHS-M` and TOKENS-M and write FEATURE/evidence/qa-gates/p5-t2-mailitemsort-usings.<TS>.md. Acceptance: every TOKENS-M total equals the M FINAL column (`usingSystem.Windows.Forms;` 1, `usingUtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;` 0, `usingUtilitiesCS.OutlookExtensions;` 1, the eight removed directives 0, `[ExcludeFromCodeCoverage]` 5).

- [x] [P5-T3] Format S and M with `CMD-SCOPED-FORMAT` (`PATHS-S`, `PATHS-M`; `TASKID` `p5-t3`), verify the five using blocks with `CMD-USINGS`, rebuild the production project with warnings as errors using `CMD-BUILD-PROD` (`PROJ` `UtilitiesCS\UtilitiesCS.csproj`, `DLL` `UtilitiesCS.dll`, `TASKID` `p5-t3`), run `git diff --numstat MERGE-BASE -- UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs` (the working-tree form: the P5-T1 and P5-T2 edits are already committed and any formatter rewrite of this task is not), `git diff --name-status MERGE-BASE HEAD -- UtilitiesCS/EmailIntelligence/EmailParsingSorting` (the committed state after the P5-T2 commit) and `git status --porcelain -- UtilitiesCS/EmailIntelligence/EmailParsingSorting` (taken before this task's commit), and write FEATURE/evidence/qa-gates/p5-t3-usings-build.<TS>.md with every printed line (`EXIT_CODE:` scoped to the `CMD-BUILD-PROD` invocation, the printed `MSBUILD_EXIT_CODE:`). Acceptance, all five required: `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`; `USINGS-EXACT-FILES: 5` with every `USINGS` row `exact=True firstline=True blankafter=True`; `MSBUILD_EXIT_CODE: 0`, `PROD_CSC_OUT_LINES:` at least 1, `ERRORS: 0` and `CS0246_LINES: 0`, `CS0103_LINES: 0`, `CS0104_LINES: 0`, `CS1061_LINES: 0` (a removed directive that was needed would surface here); the two numstat lines read `0`, `9` and the respective path (the nine surviving directives are a subsequence of the eighteen, so git records nine deletions and no addition: usings only, AC8); the name-status output is exactly six rows, `M` for each of the five surviving partials (SortEmail.cs, SortEmail.AttachmentSaving.cs, SortEmail.MailItemSort.cs, SortEmail.TrySaveAttachment.cs, SortEmail.UndoAndMoveLog.cs) and `D` for SortEmail.LegacyAttachmentSaving.cs (deleted by P4-T5 and carried by its per-task commit), and the porcelain output lists nothing under that directory other than ` M` rows for SortEmail.cs or SortEmail.MailItemSort.cs (this task's own formatter rewrite, present only when `CMD-SCOPED-FORMAT` changed the file's hash; the porcelain is recorded in both cases).

- [x] [P5-T4] Create QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs with the Write tool, content exactly Listing L-TEF (four leading spaces stripped); then run `CMD-CENSUS` with `PATHS-TEF` and TOKENS-TEF and write FEATURE/evidence/qa-gates/p5-t4-efc-tests-census.<TS>.md. Acceptance: every TOTAL equals the TOKENS-TEF expectation (`[TestMethod]` 3, the three test names 1 each, `overrideTask<bool>InvokeFilerAsync(` 1, `overridevoidResetFilerPromptState()` 1, `ResetCalls.Should().Be(1)` 2, `ResetCalls.Should().Be(0)` 1, the banned tokens 0). P5-T4 to P5-T6 (like P2-T1 to P2-T5 and P4-T1 to P4-T7) form a compile-red span: the TEF listing overrides `ResetFilerPromptState`, which P5-T6 lands and first builds green. Under the per-task commit rule the P5-T4 and P5-T5 commits carry the suffix ` (compile-red span open)` and their artifacts state `COMPILE-RED SPAN OPEN` with the last completed task and the file that does not compile (TEF); the span closes at the P5-T6 green build.

- [x] [P5-T5] Register the file in QuickFiler.Test/QuickFiler.Test.csproj with Edit E-QFT-CSPROJ, then run `CMD-CSPROJ`, `git diff --numstat MERGE-BASE -- QuickFiler.Test/QuickFiler.Test.csproj` and `git status --porcelain -- QuickFiler.Test/QuickFiler.Test.csproj` (both before this task's commit; the working-tree numstat includes this task's uncommitted edit, and the porcelain shows it as ` M`), and write FEATURE/evidence/qa-gates/p5-t5-qft-csproj.<TS>.md. Acceptance, all three required: every QFT row matches the final column (ArchiveRoot 127, FilerCleanup 128, Issue792Carry 129) and every UCS and UCT row matches the final column; the numstat line reads `1`, `0` and the path; the porcelain line shows the file modified.

- [x] [P5-T6] Extract the EfcDataModel seam without the `finally` (D11 step one) in QuickFiler/Controllers/EfcDataModel.cs with Edits E-E-SEAM-CALL then E-E-SEAM-MEMBER, then run `CMD-CENSUS` with `PATHS-E` and TOKENS-E, `CMD-SCOPED-FORMAT` (`PATHS-E`, `PATHS-TEF`; `TASKID` `p5-t6`) and `CMD-BUILD-TEST` (`PROJECT` `QuickFiler.Test`, `TASKID` `p5-t6`), and write FEATURE/evidence/qa-gates/p5-t6-efc-seam.<TS>.md with every printed line (`EXIT_CODE:` scoped to the `CMD-BUILD-TEST` invocation, the printed `MSBUILD_EXIT_CODE:`). Acceptance, all four required: every TOKENS-E total equals the SEAM column (`ResetFilerPromptState();` 1, `protectedinternalvirtualvoidResetFilerPromptState()` 1, `SortEmail.Cleanup_Files();` 1, `varresult=awaitInvokeFilerAsync(config,mailHelpers);` 1, `finally{` 0, `boolresult;` 0); `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`; `MSBUILD_EXIT_CODE: 0` and `ERROR_LINES: 0` (a `CS0507` or `CS0115` on the override would mean the seam signature differs from the probe: stop and report); `DLL_ADVANCED: True`.

- [x] [P5-T7] [expect-fail] Observe the EfcDataModel defect red after the extraction: run `CMD-VSTEST` (`ASSEMBLY-QFT`, `FILTER-EFC-CLEANUP`, `TASKID` `p5-t7`) and write FEATURE/evidence/regression-testing/fail-before-efc-filer-cleanup.md (fixed name) with the P1-T6 field set. Acceptance, all five required: `EXIT_CODE:` is non-zero and equals `ExpectedExitCode:`; `COUNTERS total=3 executed=3 passed=2 failed=1`; the `Failed` row is exactly `MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates` and the two `Passed` rows are the other `NAMES-TEF` names; the `MESSAGE` line contains `but found 0` (zero resets before the exception propagated); every `SANDBOX-` value is `False`. Otherwise `FAIL-BEFORE NOT OBSERVED` or `FAIL-BEFORE WRONG REASON`: stop and report.

- [x] [P5-T8] Apply the `try`/`finally` (D11 step two) with Edit E-E-FINALLY, then run `CMD-CENSUS` with `PATHS-E` and TOKENS-E, `CMD-SCOPED-FORMAT` (`PATHS-E`; `TASKID` `p5-t8`) and `CMD-BUILD-TEST` (`PROJECT` `QuickFiler.Test`, `TASKID` `p5-t8`), and write FEATURE/evidence/qa-gates/p5-t8-efc-finally.<TS>.md with every printed line (`EXIT_CODE:` scoped to the `CMD-BUILD-TEST` invocation, the printed `MSBUILD_EXIT_CODE:`). Acceptance, all four required: every TOKENS-E total equals the FINAL column (`finally{` 1, `boolresult;` 1, `varresult=awaitInvokeFilerAsync(config,mailHelpers);` 0, `result=awaitInvokeFilerAsync(config,mailHelpers);` 1, `ResetFilerPromptState();` 1, `returnresult;` 1); `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`; `MSBUILD_EXIT_CODE: 0` and `ERROR_LINES: 0` (a `CS0165` on `result` would mean the definite-assignment reasoning failed: stop and report); `DLL_ADVANCED: True`; and `LINES` for the file (from the census) is at most 499 (above 499 is `EFC FILE SIZE LIMIT EXCEEDED`: stop and report to the orchestrator; the spec forbids trimming an unrelated region as the remedy).

- [x] [P5-T9] Observe the EfcDataModel tests green and the archive-root pins: run `CMD-VSTEST` (`ASSEMBLY-QFT`, `FILTER-EFC-CLEANUP`, `TASKID` `p5-t9`) and `CMD-VSTEST` (`ASSEMBLY-QFT`, `FILTER-EFC-ARCHIVE`, `TASKID` `p5-t9-archive`), and append to FEATURE/evidence/regression-testing/fail-before-efc-filer-cleanup.md a section headed `## Pass-after (P5-T9)` with `PASS-AFTER-VSTEST_EXIT_CODE:`, the `COUNTERS` line and every `RESULT` line of the first run, then `ARCHIVE-VSTEST_EXIT_CODE:`, the `COUNTERS` line and every `RESULT` line of the second, and the six `SANDBOX-` lines of each. Acceptance, all four required: `PASS-AFTER-VSTEST_EXIT_CODE: 0` with `COUNTERS total=3 executed=3 passed=3 failed=0` and the rows exactly `NAMES-TEF`, each `= Passed`; `ARCHIVE-VSTEST_EXIT_CODE: 0` with `COUNTERS total=11 executed=11 passed=11 failed=0` and the rows exactly `NAMES-EFC-ARCHIVE`, each `= Passed` (the production seam body is still exercised by `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`); every `SANDBOX-` value is `False`; `git status --porcelain -- QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` prints nothing (recorded as `ART-PORCELAIN: EMPTY`; under per-task commits this proves only that this task did not touch the file) and `git diff --name-only MERGE-BASE -- QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` prints nothing (recorded as `ART-DIFF: EMPTY`; the anchored working-tree form proves the file is unchanged since the anchor in every commit state).

- [x] [P5-T10] Delete the uncompiled ToDoModel duplicate (D12, PD-13) through the coordinator: the maintainer's bypass of enforce-epic-worktree-removal-gate.ps1 covers the `CMD-DELETE` payload at P4-T5 and P5-T10 only, and the coordinator (the main session) runs it, as it did for P4-T5 (FEATURE/evidence/qa-gates/p4-t5-legacy-deletion.2026-10-03T10-17.md, `ITERATION: 2`); the executor does not run `CMD-DELETE` in any form or through any other route. Steps: substitute `PATH` `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` into the five `CMD-DELETE` lines without any other change to the payload text; run `git rev-parse HEAD`; stop before the deletion and report `DELETE HANDOFF TO COORDINATOR` with its output as `HEAD-AT-HANDOFF:`, the fully substituted payload verbatim, the task ID `P5-T10` and the worktree path, then wait for the coordinator to run the payload unchanged and return its output (if no coordinator channel answers, the run ends at this report and resumes here). The handoff writes no evidence artifact and creates no commit (the P5-T9 commit left the tree clean); the single P5-T10 artifact is written on resume and also records `HEAD-AT-HANDOFF:`, which must equal the HEAD named in the channel note (otherwise `DELETE CHANNEL REFUSED`). On resume, transcribe the coordinator's output verbatim under a `## CMD-DELETE output (coordinator-run)` heading (`EXISTS-BEFORE:`, `EXISTS-AFTER:`, `PORCELAIN:`, the payload's exit code) with a channel note naming the HEAD at which the coordinator ran it, in the form of the P4-T5 `ITERATION: 2` record; then run `git status --porcelain -- ToDoModel ToDoModel.Test` and `git diff --name-only MERGE-BASE -- ToDoModel ToDoModel.Test` (both before this task's commit; the working-tree form includes the uncommitted deletion), and write FEATURE/evidence/qa-gates/p5-t10-todomodel-deletion.<TS>.md with every printed line (`EXIT_CODE:` scoped to the `CMD-DELETE` payload, its process exit code as reported by the coordinator). Acceptance, all three required: `EXISTS-BEFORE: True` and `EXISTS-AFTER: False`; the porcelain output is exactly one ` D` row for the deleted file and the name-only diff lists exactly that path (ToDoModel.csproj and both ToDoModel.Test source files unchanged); the `PORCELAIN:` line of the payload shows ` D`. A coordinator report that lacks any of the four output lines, or a `PORCELAIN:` line without ` D`, is `DELETE CHANNEL REFUSED`: stop and report. This task's per-task commit stages the deleted path and FEATURE/.

- [x] [P5-T11] Apply the three CR-1 edits (D9) to docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md with Edits E-SPEC956-99, E-SPEC956-149 and E-SPEC956-155 (in that order), then run `CMD-SPEC-CHECK` (`STAGE` `final`), `git diff --numstat MERGE-BASE -- docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md` and `git status --porcelain -- docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956` (the numstat and the porcelain both before this task's commit; the working-tree numstat includes this task's uncommitted edits, which the porcelain shows as ` M`), and write FEATURE/evidence/qa-gates/p5-t11-cr1-spec956.<TS>.md with every printed line (`EXIT_CODE:` scoped to the `CMD-SPEC-CHECK` payload, its process exit code). Acceptance, all five required: `S956-OLD-99: 0`, `S956-NEW-99: 1`, `S956-149: 1`, `S956-NEW-149: 1`, `S956-OLD-155: 0` and `S956-NEW-155: 1`; `S956-AC-CHECKED:` equals `S956-AC-CHECKED-BASE:` and `S956-AC-UNCHECKED:` equals `S956-AC-UNCHECKED-BASE:` of P0-T2 (no checkbox changed state); `S956-LINES:` equals `S956-LINES-BASE:` plus 2; the numstat line reads `4`, `2` and the path; the porcelain output lists only spec.md under that folder (the code-review record is not in the diff).

- [x] [P5-T12] Write the fail-before exception dossier FEATURE/evidence/regression-testing/fail-before-exception.<TS>.md (PD-10; the spec's "fail-before exception dossier") with `Timestamp:`, `Command:` (`CMD-GREP-FACTS` with `STAGE` `final`, run by this task and recorded verbatim), `EXIT_CODE:` (the payload's exit code), `Output Summary:` (every CMD-GREP-FACTS line), `WhyFailingRunImpossible:` (one paragraph: the entries below are refactors with no observable defect, so no test can be red against them; each is verified by pin tests that stay green or by the compiler) and one entry per refactor-only step, each with `Entry:`, `Step:`, `Verification:` (the pin tests by name or the compiler) and `Evidence:` (the artifact path): (1) using directives (D10; P2-T7, P3-T4, P4-T4, P5-T1, P5-T2; verification `CMD-USINGS` `USINGS-EXACT-FILES: 5` and the P5-T3 production rebuild; evidence p5-t3-usings-build); (2) logger replacement and outer-catch removal (D2; P3-T4; pins T7, T10 of `NAMES-T` and `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave`; evidence fail-before-try-save-retry.md pass-after section); (3) F2 deletions (D7; P4-T4, P4-T5; verification `DEAD-MEMBERS-CS: 0`, `LEGACY-FILE-EXISTS: False` and the P4-T7 build; evidence p4-t5-legacy-deletion and p4-t7-format-and-build, each the `ITERATION: 2` artifact); (4) F3 exclusion changes (D8; P2-T7, P4-T4; verification `EFCC-PARTIALS: 18` and the coverage comparison of P6-T8); (5) ToDoModel deletion (D12; P5-T10; verification `TODOMODEL-FILE-EXISTS: False`, `TODOMODEL-CSPROJ-MATCHES: 2` naming only ToDoModel.Test.csproj, and the Phase 6 rebuilds; evidence p5-t10-todomodel-deletion); (6) CR-1 (D9; P5-T11; verification the six `S956-` literal counts; evidence p5-t11-cr1-spec956); (7) F1 seam extraction (D5, PD-14; P4-T4 and the P4-T7 rewrite to the `TrySaveAttachmentDelegate` seam type; verification the compile-red record compile-red-attachment-saving-seams.md, the P4-T7 `ITERATION: 2` green build and the P4-T8 and P4-T12 green runs); (8) structural-test replacement (D3 phase F1; P4-T2; verification the mutation control p4-t14-control-applied and p4-t15-control-restored). Acceptance, all four required: every CMD-GREP-FACTS line matches its `final` expectation (`DEAD-MEMBERS-CS: 0`, `SORTEMAIL-FILE-COUNT: 5`, `SHOWDIALOG-CALLS-PARTIALS: 0`, `ENUM-FIELDS-A: 0`, `DEAD-TOKENS-PARTIALS: 0`, `BANNED-USINGS-PARTIALS: 0`, `USING-SYSTEM-PARTIAL-FILES: 5`, `DEBUG-WRITELINE-T: 0`, `EFCC-PARTIALS: 18`, `TESTS6-PRESENT: 6`, `BANNED-TEST-APIS: 0`, `NON-APPROVED-FRAMEWORKS: 0`, `LEGACY-FILE-EXISTS: False`, `TODOMODEL-FILE-EXISTS: False`, `TODOMODEL-CSPROJ-MATCHES: 2` with `TODOMODEL-CSPROJ-FILES: ToDoModel.Test\ToDoModel.Test.csproj`); the dossier carries all eight entries; every `Evidence:` path exists; the file name matches `fail-before-exception.*.md`.

### Phase 6 — Final QA Loop, Coverage Comparison, Footprint, Evidence Assembly and Acceptance Check-Off

Loop rule. P6-T1 to P6-T7 form one toolchain pass in CLAUDE.md order (format, check, analyzer rebuild, nullable rebuild, tests, tests with coverage). Every Phase 6 artifact carries `ITERATION:` (1 on the first pass). When P6-T1 reports `WRITESET-CHANGED-COUNT:` above 0 (the formatter changed a Write Set file), the pass restarts at P6-T1 with `ITERATION:` incremented and every later Phase 6 artifact is re-written for the new iteration; at most three iterations run, and a fourth is `TOOLCHAIN LOOP NOT CONVERGING`: stop and report. Any other failure in P6-T1 to P6-T7 is a defect this plan did not predict: stop and report it with the failing output; the executor does not repair source, tests or gates. The check-off tasks P6-T19 to P6-T23 and P6-T25 to P6-T44 edit FEATURE/spec.md only in the five characters of one check box each (Edit E-AC-CHECKOFF) and edit no other file, except P6-T44's repair branch, which adds a missing schema field to the evidence artifact a `MISSING-FIELDS:` row names before its Edit; P6-T24 and P6-T45 are deferral records that do not edit FEATURE/spec.md (PD-11); a Grep-tool pattern below is a regular expression, so its brackets and parentheses are escaped. P6-T44 additionally runs the read-only `CMD-EVIDENCE-FIELDS` payload. Revision 1.9: P6-T13 is the one Phase 6 task that edits a Write Set source file (the TST1 documentation comment of PD-15), after the final pass of P6-T1 to P6-T11 (`ITERATION: 1`); P6-T14 re-runs the scoped formatter pass, the repository-wide read-only check and the TST1 census over the edited file, P6-T15 re-runs the P6-T12 anchor recheck and footprint at `ITERATION: 2`, and P6-T16 gates the edit's anchored diff identity; the P6-T13 Edit is applied exactly as specified and triggers neither the loop rule's restart form (it is not a formatter change) nor any source repair, and the rebuilds and test runs of the recorded final pass are not repeated for it (revision 1.9 log).

- [x] [P6-T1] Run the repository-wide formatter with `CMD-FORMAT-REPO` (`TASKID` `p6-t1`; canonical command `dotnet tool run csharpier format .`) and write FEATURE/evidence/qa-gates/p6-t1-csharpier-format.<TS>.md with `EXIT_CODE:` (the printed `FORMAT_EXIT_CODE:`, the only invocation), `FORMAT_EXIT_CODE:`, every `WRITESET-CHANGED:` line, `WRITESET-CHANGED-COUNT:`, both porcelain counts, `PORCELAIN-SAME:` and the formatter's summary line as an observation. Acceptance, all three required: `FORMAT_EXIT_CODE: 0`; `WRITESET-CHANGED-COUNT: 0` (the twelve C# Write Set hashes are identical before and after; a non-zero value triggers the loop rule); `PORCELAIN-SAME: True` (no file outside the Write Set changed; `False` is `FORMAT TOUCHED OUT-OF-SET FILE`: stop and report, do not revert).

- [x] [P6-T2] Verify formatting read-only with `CMD-CHECK-REPO` (`TASKID` `p6-t2`; canonical command `dotnet tool run csharpier check .`) and write FEATURE/evidence/qa-gates/p6-t2-csharpier-check.<TS>.md. Acceptance: `CHECK_EXIT_CODE: 0` recorded as `EXIT_CODE: 0`, and `CHECKED-LINE:` matches `Checked <N> files` with a positive N.

- [x] [P6-T3] Run the analyzer gate with `CMD-REBUILD` (analyzer `GATEARGS`, `TASKID` `p6-t3`) over TaskMaster.sln and write FEATURE/evidence/qa-gates/p6-t3-msbuild-analyzers.<TS>.md with the P0-T7 field set. Acceptance, all five required: `EXIT_CODE: 0`; `SKIP_CORECOMPILE_LINES: 0`; the four `_CSC_OUT_LINES:` values each at least 1 (CoreCompile ran for UtilitiesCS, UtilitiesCS.Test, QuickFiler and QuickFiler.Test); `ERRORS: 0`; `WRITESET_DIAGNOSTIC_LINES:` at most `ANALYZE-BASELINE-WRITESET-DIAGNOSTICS:` of P0-T7.

- [x] [P6-T4] Run the type-check gate with `CMD-REBUILD` (nullable `GATEARGS`, `TASKID` `p6-t4`; no Nullable property override) over TaskMaster.sln and write FEATURE/evidence/qa-gates/p6-t4-msbuild-nullable.<TS>.md with the P0-T7 field set. Acceptance, all five required: `EXIT_CODE: 0`; `SKIP_CORECOMPILE_LINES: 0`; the four `_CSC_OUT_LINES:` values each at least 1; `ERRORS: 0`; `WRITESET_DIAGNOSTIC_LINES:` at most `NULLABLE-BASELINE-WRITESET-DIAGNOSTICS:` of P0-T8.

- [x] [P6-T5] Run the final SortEmail family: `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-SORTEMAIL`, `TASKID` `p6-t5`) and write FEATURE/evidence/regression-testing/pass-after-regression-tests.md (fixed name) with `Timestamp:`, `ITERATION:`, `Command:`, `EXIT_CODE:` and an `Output Summary:` with `RUNSETTINGS-HASH-NOW:`, the six `SANDBOX-` lines, the `COUNTERS` line, `RESULT_COUNT:`, `SEQUENCE_FILES:` and every `RESULT` line. Acceptance, all five required: `EXIT_CODE: 0`; `COUNTERS total=55 executed=55 passed=55 failed=0`; the fifty-five `RESULT` rows are exactly the union of `NAMES-TST1-FINAL`, `NAMES-T12`, `NAMES-TSC-FINAL`, `NAMES-TAS-FINAL` and `NAMES-TUL`, each `= Passed`; `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:`; every `SANDBOX-` value is `False` and `SEQUENCE_FILES: 0`.

- [x] [P6-T6] Run the final QuickFiler.Test suites: `CMD-VSTEST` (`ASSEMBLY-QFT`, `FILTER-EFC-CLEANUP`, `TASKID` `p6-t6`) and `CMD-VSTEST` (`ASSEMBLY-QFT`, `FILTER-EFC-ARCHIVE`, `TASKID` `p6-t6-archive`), and append to FEATURE/evidence/regression-testing/pass-after-regression-tests.md a section headed `## QuickFiler.Test (P6-T6)` with `CLEANUP-VSTEST_EXIT_CODE:`, `ARCHIVE-VSTEST_EXIT_CODE:`, both `COUNTERS` lines, every `RESULT` line of both runs and the six `SANDBOX-` lines of each. Acceptance, all three required: `CLEANUP-VSTEST_EXIT_CODE: 0` with `COUNTERS total=3 executed=3 passed=3 failed=0` and the rows exactly `NAMES-TEF`, each `= Passed`; `ARCHIVE-VSTEST_EXIT_CODE: 0` with `COUNTERS total=11 executed=11 passed=11 failed=0` and the rows exactly `NAMES-EFC-ARCHIVE`, each `= Passed`; every `SANDBOX-` value is `False`.

- [x] [P6-T7] Run the final repository-wide test and coverage pass: `CMD-COVERAGE-DIRECT` (`STAGE` `final`, the same `EXCLUSION` as P0-T11) as a background invocation polled until its final `TRX_PRESENT:` line, then `CMD-COVERAGE-POST` (`STAGE` `final`), and write FEATURE/evidence/qa-gates/coverage-post-change.md (fixed name) with the P0-T11 field set (`FINAL-UCS-LINE:`, `FINAL-UCS-BRANCH:`, `FINAL-QF-LINE:`, `FINAL-QF-BRANCH:`, `FINAL-FIRST-PARTY-LINE-PERCENT:` and `FINAL-FIRST-PARTY-BRANCH-PERCENT:` in place of the `BASELINE-` figures) and `NEWLY-FAILING:` (the names of `FAILED-SET:` that are absent from coverage-baseline.md's `FAILED-SET:`, or `NONE`). Branches as in P0-T11, with (c) named `COVERAGE FLOOR NOT MET`. Acceptance, all seven required: the projection holds `UtilitiesCS` and `QuickFiler` packages with `LINE` and `BRANCH` counters; `FIRST-PARTY-LINE-PERCENT:` at least 80 and `FIRST-PARTY-BRANCH-PERCENT:` at least 75 with `LINE-FLOOR: MET` and `BRANCH-FLOOR: MET`; `FAILED-SET:` contains none of the names of `NAMES-TST1-FINAL`, `NAMES-T12`, `NAMES-TSC-FINAL`, `NAMES-TAS-FINAL`, `NAMES-TUL`, `NAMES-TEF` or `NAMES-EFC-ARCHIVE`; `NEWLY-FAILING: NONE`, or only `TryAddValuesAsync_UpdatesExistingValue` under branch (b); `EXIT_CODE:` equals its declared expectation; every `SANDBOX-` value is `False`; the artifact contains no absolute path. coverage\final-959.cobertura.xml and coverage\final-959.jacoco.xml stay on disk for P6-T8 and P6-T9.

- [x] [P6-T8] Compare coverage with `CMD-COVERAGE-TEXTS` (`STAGE` `final`, `BASELINE-HASH` the `BASELINE-NONEXEMPT-HASH:` of coverage-baseline.md) and write FEATURE/evidence/qa-gates/coverage-comparison.md (fixed name) with `Timestamp:`, `ITERATION:`, `Command:`, `EXIT_CODE:` (the payload's exit code) and an `Output Summary:` holding every printed line, followed by a `Reading:` paragraph that restates the PD-8 rule (the four content-identified exemption sets and their observed lines, the set comparison by path and text, the in-memory control). Acceptance, all nine required: `SORTEMAIL-DIR-CLASSES:` at least 1 and `TRYSAVE-CLASS-FOUND: True`; `EXEMPT-LAMBDA-COUNT: 1`, `EXEMPT-ELSE-BRACE-COUNT: 1`, `EXEMPT-CATCH-BRACE-COUNT: 1` and `EXEMPT-GUARD-BRACE-COUNT: 1` with `GUARD-CONDITION-LINES:` naming exactly one line; `NONEXEMPT-SET-MATCHES-BASELINE: True` (the non-exempt uncovered statements of the family are the same set as at baseline, wherever their line numbers moved; `False` is `AC25: NEW UNCOVERED LINE`: stop and report every `NONEXEMPT-UNCOVERED` row); `CONTROL-LINE:` greater than 0 and `CONTROL-DIFFERS-BASELINE: True` (`False` is `AC25 CHECK NOT DISCRIMINATING`: stop); `FIRST-PARTY-LINE-NOT-LOWER: True` and `FIRST-PARTY-BRANCH-NOT-LOWER: True` (`False` is `AC25: FIRST-PARTY RATE LOWER`: stop and report both `FIRST-PARTY` lines); `EXEMPT-UNCOVERED:` recorded (predicted 4); the `PACKAGE` rows for `UtilitiesCS` and `QuickFiler` are recorded as observations with no `MISSING`; the artifact lists every `EXEMPT-*-LINES` value and every `NONEXEMPT-UNCOVERED` row; the artifact contains no absolute path.

- [x] [P6-T9] Measure per-member line coverage with `CMD-MEMBER-COVERAGE` over coverage\final-959.cobertura.xml and append a section headed `## Per-member coverage (P6-T9)` with `Timestamp:`, `Command:`, `EXIT_CODE:` and every printed line to FEATURE/evidence/qa-gates/coverage-comparison.md. Acceptance, all five required: every `CLASS-NODES` value at least 1; `MEMBERS-AMBIGUOUS: 0` and `MEMBERS-UNMEASURED: 0` (every span found once and measured; otherwise `MEMBER SPAN UNRESOLVED`: stop and report the rows); every `MEMBER` row shows `percent=` at least 90 and `MEMBERS-BELOW-90: 0` (a lower value is `NEW CODE COVERAGE BELOW 90`: stop and report the `uncovered=` lines); each `E-CHANGED-LINE` row shows `matches=1` and `hits=` greater than 0 and `E-CHANGED-LINES-COVERED: 3`; the `TrySaveAttachmentCoreAsync` row's `uncovered=` value is a subset of the `EXEMPT-LINES:` of P6-T8 (recorded; predicted to be the else brace, the catch brace and the guard brace).

- [x] [P6-T10] Run the post-format census over the final tree and write FEATURE/evidence/qa-gates/p6-t10-post-format-census.<TS>.md with every printed line (`EXIT_CODE:` scoped to the `CMD-LINES` payload, the last `pwsh` invocation; the Grep and Read steps that follow it have no exit code): `CMD-CENSUS` for `PATHS-A` with TOKENS-A, `PATHS-T` with TOKENS-T, `PATHS-U` with TOKENS-U, `PATHS-S` with TOKENS-S, `PATHS-M` with TOKENS-M, `PATHS-E` with TOKENS-E, `PATHS-TST1` with TOKENS-TST1, `PATHS-TST2` with TOKENS-TST2, `PATHS-TSC` with TOKENS-TSC, `PATHS-TAS` with TOKENS-TAS, `PATHS-TUL` with TOKENS-TUL and `PATHS-TEF` with TOKENS-TEF; `CMD-USINGS`; `CMD-CSPROJ`; `CMD-GREP-FACTS` (`STAGE` `final`); `CMD-LINES` with `PATHS-CSHARP-FINAL`; then a Grep-tool search of UtilitiesCS/EmailIntelligence/EmailParsingSorting with glob `SortEmail*.cs`, pattern `ExcludeFromCodeCoverage` and one line of trailing context, recorded as `EFCC-MEMBERS:` (the eighteen attribute lines each followed by the member line that carries it; the `using System.Diagnostics.CodeAnalysis;` directive does not contain the pattern text, so no using line is matched); then a Read of each of the six test files for a retry attribute or retry loop, recorded as `RETRY-READ: NONE FOUND` or the finding. Acceptance, all seven required: every TOKENS total equals its FINAL column (TOKENS-S and TOKENS-M their FINAL columns; TOKENS-TSC, TOKENS-TAS, TOKENS-TUL and TOKENS-TEF their listed values); `USINGS-EXACT-FILES: 5`; every `CMD-CSPROJ` row matches the final column; every `CMD-GREP-FACTS` line matches its `final` expectation; `MAX-LINES:` at most 499 with the twelve `LINES` rows present and the rows for `QuickFiler\Controllers\EfcDataModel.cs`, `UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs` and `UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs` (predicted about 485 to 490 after the format, the closest of the twelve) quoted individually under `AC23-CLOSEST:`; `EFCC-MEMBERS:` lists eighteen attribute-and-member pairs whose member lines are exactly: in A `SaveAttachment(this AttachmentHelper attachmentHelper)`, `SaveAttachmentAsync(this AttachmentHelper attachmentHelper)` and the destination overload; in T the two-argument `TrySaveAttachmentAsync` and `ClearReadOnlyAttributeOnDisk`; in U `UndoAsync`, `PushToUndoStack`, `CaptureMoveDetails` and the two-parameter `WriteCSV_StartNewFileIfDoesNotExist`; in S `SortAsync`, `UpdatePredictiveEngineAsync`, `ProcessMailItemAsync` and `ResolvePaths`; in M three `SortAsync`, `Sort` and `ResolvePaths`; `RETRY-READ: NONE FOUND`. A failure is `POST-FORMAT CENSUS MISMATCH`: stop and report.

- [x] [P6-T11] Close the toolchain loop in FEATURE/evidence/qa-gates/toolchain-final-pass.md (fixed name) with `Timestamp:`, `ITERATION:`, `LOOP-RESTARTS:`, `Command:` (the four CLAUDE.md commands in order, naming the P6-T7 collect invocation as the one `EXIT_CODE:` is scoped to), `EXIT_CODE:` (the P6-T7 `COLLECT_EXIT_CODE:`), `ExpectedExitCode:` equal to that observed value when it is non-zero under branch (b) of P6-T7 (the value coverage-post-change.md declares; omitted under branch (a)), and one row per step of the final iteration (P6-T1 format, P6-T2 check, P6-T3 analyzer rebuild, P6-T4 nullable rebuild, P6-T5 and P6-T6 scoped tests, P6-T7 tests with coverage) giving the canonical command, the artifact path, the step's exit code and, for the two rebuilds, `SKIP_CORECOMPILE_LINES:` and the four `_CSC_OUT_LINES:` values. Acceptance, all four required: every row of the final iteration reads exit code 0, except P6-T7 which reads 0 or its declared branch (b) expectation; both rebuild rows read `SKIP_CORECOMPILE_LINES: 0` with the four `_CSC_OUT_LINES:` at least 1; the P6-T1 row reads `WRITESET-CHANGED-COUNT: 0`; `EXIT_CODE:` equals its declared expectation (default 0 when `ExpectedExitCode:` is omitted).

- [x] [P6-T12] Verify the footprint against the eighteen Write Set paths and FEATURE/: first run `git merge-base HEAD origin/main` (no fetch; `git -C WORKTREE` form) and record its output as `ANCHOR-RECHECK:`, then run `CMD-FOOTPRINT` (`MERGE-BASE` and `INHERITED` substituted; the payload pairs `git diff --name-only MERGE-BASE` with `git status --porcelain --untracked-files=all`) and write FEATURE/evidence/qa-gates/p6-t12-scope-boundary.<TS>.md with `ANCHOR-RECHECK:` and every printed line. Acceptance, all eight required: `ANCHOR-RECHECK:` equals `MERGE-BASE:` of P0-T3 (an observation compared with the recorded anchor, not a re-derivation; a difference is `ANCHOR MOVED`: stop and report, because every `MERGE-BASE` diff gate of this plan would then compare against a superseded anchor); `OUTSIDE-WRITE-SET: 0`; `WRITE-SET-MISSING: 0`; `DELETED-PATHS:` lists exactly `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs` and `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs`; `RAW-DOC-PATHS: 0`; `NUMSTAT-UCS:` reads `0 1 UtilitiesCS/UtilitiesCS.csproj`, `NUMSTAT-UCT:` reads `3 0 UtilitiesCS.Test/UtilitiesCS.Test.csproj`, `NUMSTAT-QFT:` reads `1 0 QuickFiler.Test/QuickFiler.Test.csproj` and `NUMSTAT-SPEC956:` reads `4 2` and the spec path (tab separators may print as whitespace); `NUMSTAT-S:` and `NUMSTAT-M:` each read `0 9` and the respective path; the artifact records the subtracted Clause A and Clause B counts.

- [x] [P6-T13] Correct the stale XML documentation comment of `Cleanup_Files_DoesNotThrow` in UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs (PD-15; revision 1.9; a documentation-comment-only change with no behavior change, so no regression test is written) with Edit E-TST1-DOC-CLEANUP, then verify with two Grep-tool searches of that file (the Grep tool is the named instrument of this task; no `pwsh` payload runs): the regex `every prompt session in AllPromptSessions` (the single-line, backslash-free token of the NEW block) and the regex `YesNoToAllResponse tracking fields` (the phrase of the OLD block), and write FEATURE/evidence/qa-gates/p6-t13-tst1-doc-comment.<TS>.md with `Timestamp:`, `Command:` (the Edit and the two Grep-tool searches), `EXIT_CODE: 0` (a Grep-tool task; the row names the second search), `ITERATION: 1`, and an `Output Summary:` with `NEW-DOC-TOKEN-LINES:` (the first search's line count) and `OLD-DOC-PHRASE-LINES:` (the second's). Acceptance, both required: `NEW-DOC-TOKEN-LINES: 1` (zero lines before the Edit, the false-before state, because `AllPromptSessions` occurs nowhere in TST1 at HEAD 9de3f176d, Verified Repository Facts 18); `OLD-DOC-PHRASE-LINES: 0` (one line, 171, before the Edit). An Edit whose old text is not found exactly once is `EDIT ANCHOR NOT UNIQUE`: stop and report. The commit under the per-task rule stages UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs and FEATURE/ (`wip(959): P6-T13 TST1 Cleanup_Files doc comment`).

- [x] [P6-T14] Format and re-census TST1 after the P6-T13 edit: run `CMD-SCOPED-FORMAT` (`PATHS-TST1`; `TASKID` `p6-t14`), then `CMD-CHECK-REPO` (`TASKID` `p6-t14`; canonical command `dotnet tool run csharpier check .`), then `CMD-CENSUS` with `PATHS-TST1` and TOKENS-TST1, and write FEATURE/evidence/qa-gates/p6-t14-doc-comment-format-and-census.<TS>.md with `Timestamp:`, `Command:` (the three payloads in order), `EXIT_CODE:` (scoped to the `CMD-CENSUS` payload, the last invocation, its process exit code), `ITERATION: 1`, `FORMAT_EXIT_CODE:`, the `BEFORE` and `AFTER` hash rows of TST1, the scoped payload's `CHECK_EXIT_CODE:` copied as `SCOPED-CHECK_EXIT_CODE:`, the repository-wide payload's `CHECK_EXIT_CODE:` copied as `REPO-CHECK_EXIT_CODE:` with its `CHECKED-LINE:`, and every `TOKEN`, `LINES` and `SHA256` line of the census. Acceptance, all four required: `FORMAT_EXIT_CODE: 0` with both hash rows recorded (the write-mode observation; the hashes are equal when the Edit kept the file's line endings and differ when the formatter normalized the two replaced lines, either value is recorded, and the read-only checks are the gate); `SCOPED-CHECK_EXIT_CODE: 0` and `REPO-CHECK_EXIT_CODE: 0` with `CHECKED-LINE:` matching `Checked <N> files` with a positive N; every TOKENS-TST1 total equals the FINAL column (the OLD and NEW texts of E-TST1-DOC-CLEANUP contain no TOKENS-TST1 token, so every total is unchanged from p6-t10-post-format-census); `LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 488` (the value p6-t10-post-format-census records at its `LINES` row for TST1; two comment lines replace two comment lines; the `SHA256` row is recorded, not gated). A census or line-count mismatch is `POST-FORMAT CENSUS MISMATCH` and a non-zero check exit code is `FORMAT CHECK FAILED AFTER DOC EDIT`: stop and report. The commit under the per-task rule stages UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs (when the `AFTER` hash differs from `BEFORE`) and FEATURE/.

- [x] [P6-T15] Re-verify the footprint over the edited tree (the P6-T12 gate at `ITERATION: 2`): run `git merge-base HEAD origin/main` (no fetch; `git -C WORKTREE` form) and record its output as `ANCHOR-RECHECK:`, then run `CMD-FOOTPRINT` exactly as P6-T12 ran it (`MERGE-BASE` and `INHERITED` substituted with the P0-T3 values; the payload pairs `git diff --name-only MERGE-BASE` with `git status --porcelain --untracked-files=all`), and write FEATURE/evidence/qa-gates/p6-t12-scope-boundary.<TS>.md (the P6-T12 artifact name, a new file with this task's `<TS>`, carrying `ITERATION: 2` so that the glob `p6-t12-scope-boundary.*.md` of every later reader resolves to this record under the artifact-filenames convention; the iteration-1 record p6-t12-scope-boundary.2026-10-03T12-55.md stays on disk unchanged) with `Timestamp:`, `Command:`, `EXIT_CODE:` (the payload's process exit code), `ITERATION: 2`, `WRITTEN-BY: P6-T15`, `ANCHOR-RECHECK:` and every printed line. Acceptance, all eight required, the P6-T12 clauses unchanged: `ANCHOR-RECHECK:` equals `MERGE-BASE:` of P0-T3 (a difference is `ANCHOR MOVED`: stop and report); `OUTSIDE-WRITE-SET: 0` (the P6-T13 edit lies inside Write Set path 10 and the P6-T13 and P6-T14 artifacts inside FEATURE/); `WRITE-SET-MISSING: 0`; `DELETED-PATHS:` lists exactly `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs` and `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs`; `RAW-DOC-PATHS: 0`; `NUMSTAT-UCS:` reads `0 1`, `NUMSTAT-UCT:` `3 0`, `NUMSTAT-QFT:` `1 0` and `NUMSTAT-SPEC956:` `4 2`, each with its path; `NUMSTAT-S:` and `NUMSTAT-M:` each read `0 9` and the respective path; the subtracted Clause A and Clause B counts are recorded. The commit under the per-task rule stages FEATURE/.

- [x] [P6-T16] Prove the AC5 textual identities and sweep the feature folder: run `CMD-TST-IDENTITY` (`MERGE-BASE` substituted; widened in revision 1.9 with `TST1-REMOVED-DOC-LINES:` and `TST1-ADDED-DOC-LINES:`; issued through the coordinator relay unchanged under the maintainer's standing approval of 2026-10-04, because enforce-epic-worktree-removal-gate.ps1 refused it on 2026-10-03T12-56 as the stop record p6-t13-identity-and-sweep.2026-10-03T12-56.md shows, the record of this task under its pre-renumbering number, which stays on disk unchanged and matches no glob of this plan), `CMD-SWEEP` and `CMD-EVIDENCE-FIELDS` (`INHERITED` substituted as the `INHERITED-CLAUSE-A:` list of P0-T3 minus the P0-T1 artifact phase0-instructions-read.md and the P0-T2 artifact p0-t2-mode-preconditions.<TS>.md, the two Phase 0 artifacts this run wrote before P0-T3 captured Clause A, unless P0-T2's `PRE-EXISTING-EVIDENCE:` names that path; it prints `EVIDENCE-FILES-CHECKED:`, `EVIDENCE-MISSING-FIELDS:`, `FIELD-CHECK-CONTROL:`, `FIELD-CHECK-POSITIVE:`, `SUBFOLDER-CHECK-FLAGS-OTHER:`, `SUBFOLDER-CHECK-FLAGS-QA-GATES:`, `NONCANONICAL-SUBFOLDER-FILES:`, `EVIDENCE-INHERITED-SKIPPED:`, one `MISSING-FIELDS:` row per offending path and one `NONCANONICAL:` row per file outside the three subfolders), and write FEATURE/evidence/qa-gates/p6-t16-identity-and-sweep.<TS>.md with `ITERATION: 2` (the re-run after the stop record) and every printed count and row (never the sweep tokens; `EXIT_CODE:` scoped to the `CMD-EVIDENCE-FIELDS` payload, the last invocation). Acceptance, all five required: `TST2-DELETED-LINES: 0` (the eleven pre-existing TST2 methods are textually unchanged); `TST1-REMOVED-SANITIZE-LINES: 1` and `TST1-REMOVED-TRYSAVE-LINES: 0` (the two TST1 try-save pins are textually unchanged) together with `TST1-REMOVED-DOC-LINES: 1` and `TST1-ADDED-DOC-LINES: 1` (the P6-T13 replacement removed the one line carrying `YesNoToAllResponse tracking fields` and added the one line carrying `every prompt session in AllPromptSessions`; both read 0 before P6-T13, the false-before state; `TST1-REMOVED-LINES:` is recorded, not gated; the clause count stays five); `ACCOUNT-TOKEN-FILES: 0`, `PROFILE-LEAF-FILES: 0`, `MACHINE-TOKEN-FILES: 0`, `WORKTREE-ROOT-FILES: 0`, `USERS-PATH-FILES: 0` and `RAW-DOCUMENT-FILES: 0`; `EVIDENCE-MISSING-FIELDS: 0`, `NONCANONICAL-SUBFOLDER-FILES: 0`, `FIELD-CHECK-CONTROL: False`, `FIELD-CHECK-POSITIVE: True`, `SUBFOLDER-CHECK-FLAGS-OTHER: True` and `SUBFOLDER-CHECK-FLAGS-QA-GATES: False` (every artifact this run wrote sits under baseline, regression-testing or qa-gates and carries the three line-leading fields, AC26; the two controls, built from the same `$anchor` variable as the per-file predicate, prove the field test rejects a prefixed label such as `FORMAT_EXIT_CODE:` and accepts a line-leading one such as `- **EXIT_CODE:** 0`; the clause count stays five); `TST1-PORCELAIN-LINES:` recorded. A non-zero sweep count is repaired by applying the artifact-hygiene substitution to the named artifact and re-running this task; a raw document is removed from FEATURE/ (it remains under coverage/); a missing field is repaired by adding the field to the named artifact with the value its task defines; a non-zero `NONCANONICAL-SUBFOLDER-FILES:` is `NONCANONICAL EVIDENCE FILE`: stop and report the `NONCANONICAL:` rows (the executor does not move a file it did not write); `FIELD-CHECK-CONTROL: True` or `FIELD-CHECK-POSITIVE: False` is `FIELD CHECK NOT DISCRIMINATING`: stop and report (the field test accepted a prefixed label or rejected a line-leading one, so its zero count proves nothing); `SUBFOLDER-CHECK-FLAGS-OTHER: False` or `SUBFOLDER-CHECK-FLAGS-QA-GATES: True` is `SUBFOLDER CHECK NOT DISCRIMINATING`: stop and report.

- [x] [P6-T17] Write FEATURE/evidence/qa-gates/negative-controls.md (fixed name; PD-7) with `Timestamp:`, `ITERATION:`, `Command:` (one payload of `Test-Path -LiteralPath` over the nine artifacts named below, the six fail-before artifacts and the three control artifacts, printing `ARTIFACTS-PRESENT: <n>/9`), `EXIT_CODE:`, and two sections: `## Red-first observations` with one row per code fix giving the fix, the test or rows observed failing, the failing-row set, the `MESSAGE` substring observed, the fail-before artifact and its `EXIT_CODE:`, and the pass-after section that closed it (L1: fail-before-save-case.md; L3 phase one: fail-before-cleanup-files-phase-one.md; L4 and the header: fail-before-write-csv.md; L2: fail-before-try-save-retry.md; re-rooting: fail-before-redirect-save-folder.md; EfcDataModel: fail-before-efc-filer-cleanup.md); and `## Mutation control (structural test)` quoting from p4-t13-control-backup, p4-t14-control-applied and p4-t15-control-restored the hashes `FIX-HASH-A:`, `MUTATED-HASH-A:` and the final `RESTORED-HASH-A:`, the failing row, its `MESSAGE` substring and the restored run's `COUNTERS` line. Acceptance, all three required: `ARTIFACTS-PRESENT: 9/9`; every row of the first section names a non-zero `EXIT_CODE:` and a failing-row set equal to the Test Inventory table; the second section shows `RESTORED-HASH-A:` equal to `FIX-HASH-A:` and `MUTATED-HASH-A:` different from both.

- [x] [P6-T18] Write FEATURE/evidence/qa-gates/pr-description-inputs.<TS>.md (PD-11) with `Timestamp:`, `Command:` (`git rev-parse HEAD`, run read-only), `EXIT_CODE:`, `HEAD-SHA:` (observation), and the verbatim inputs for the orchestrator's pr-author step: the closing references `Closes #959` and `Closes #966`; the four behavior changes of the spec's Data / API / Config Impact section (rethrow instead of hang under a held YesToAll, the alternate save path re-rooted under the destination folder, the single tab-separated header line on first use, the prompt-state reset after a filer exception); the UT5 call-out paragraph of the spec's Test Strategy copied verbatim and marked as applying to phase one of L3 only; the untraced terminal sink of the L2 rethrow (spec Risks) as a maintainer question; and the research corrections of PD-6 and PD-10 together with the spec technical-section correction of PD-14, already applied to FEATURE/spec.md (the try-save seam is the nested non-generic `SortEmail.TrySaveAttachmentDelegate` with the signature `(Attachment, string) -> Task<bool>`, not a `Func<Attachment, string, Task<bool>>`, because of CS1769 under the embedded interop types of UtilitiesCS). Acceptance, all three required: `EXIT_CODE: 0`; the artifact contains the two closing references, the four behavior changes and the literal `UT5 call-out`; the artifact contains no absolute path.

- [x] [P6-T19] Check off AC1 in FEATURE/spec.md: precondition: p6-t10 TOKENS-A FINAL shows `caseYesNoToAllResponse.NoToAll:`, `caseYesNoToAllResponse.No:`, `caseYesNoToAllResponse.Yes:` and `caseYesNoToAllResponse.YesToAll:` 1 each, `\|YesNoToAllResponse.` 0 (no pipe inside a case label), `HasFlag` 0 and `[ExcludeFromCodeCoverage]` 3, and `EFCC-MEMBERS:` lists no `SaveCase` entry. Edit: E-AC-CHECKOFF with N 1. Acceptance: a Grep-tool search of FEATURE/spec.md for the regex `^- \[x\] AC1 \(` returns one line and for `^- \[ \] AC1 \(` returns zero lines. If the precondition does not hold, leave the box unchecked and report `AC1 NOT MET`.

- [x] [P6-T20] Check off AC2 in FEATURE/spec.md: precondition: fail-before-save-case.md satisfies every P1-T6 acceptance condition (non-zero `EXIT_CODE:` equal to its expectation, the four failing rows named, the control row passed) and its pass-after section shows 5 of 5 passed; pass-after-regression-tests.md shows the five `NAMES-TSC-P1` rows `= Passed`; p6-t10 TOKENS-TSC shows `SortEmail.SaveCase(` 3, `Times.Once` 2, `Times.Never` 3 and `C:\Sortemail959Sandbox` 2. Edit: E-AC-CHECKOFF with N 2. Acceptance: Grep counts `^- \[x\] AC2 \(` one line and `^- \[ \] AC2 \(` zero; otherwise report `AC2 NOT MET`.

- [x] [P6-T21] Check off AC3 in FEATURE/spec.md: precondition: p6-t10 TOKENS-T FINAL holds (`internalstaticTask<bool>TrySaveAttachmentAsync(` 3 and `internalstaticasyncTask<bool>TrySaveAttachmentAsync(` 0, `privatestaticasyncTask<bool>TrySaveAttachmentCoreAsync(` 1, `TrySaveAttachmentCoreAsync(` 3, `isRetryAfterClear:false` 1, `isRetryAfterClear:true` 1, the guard condition token 1, `logger.Warn(` 2, `logger.Error(` 2, `catch(` 2, `catch(System.Exception){throw;}` 0, `Debug.WriteLine(` 0, `usingSystem.Diagnostics;` 0, `createDirectory(Path.GetDirectoryName(filePathSave));` 1, `[ExcludeFromCodeCoverage]` 2, `System.IO.Directory.CreateDirectory(path)` 1); `USINGS T exact=True`; coverage-comparison.md shows `GUARD-CONDITION-LINES:` with one line and `EXEMPT-GUARD-BRACE-COUNT: 1`; `EFCC-MEMBERS:` lists in T exactly the two-argument overload and `ClearReadOnlyAttributeOnDisk`. Edit: E-AC-CHECKOFF with N 3. Acceptance: Grep counts `^- \[x\] AC3 \(` one line and `^- \[ \] AC3 \(` zero; otherwise report `AC3 NOT MET`.

- [x] [P6-T22] Check off AC4 in FEATURE/spec.md: precondition: fail-before-try-save-retry.md satisfies every P3-T3 acceptance condition (T12 failed with `InvalidOperationException`, non-zero `EXIT_CODE:` equal to its expectation) and its pass-after section shows 12 of 12 passed; p6-t10 TOKENS-TST2 FINAL holds (`CreateDirectoryLimit` 3, `.Throws(denied)` 1, `BeSameAs(denied)` 1, `Times.Exactly(2)` 5, `Thread.Sleep`, `Task.Delay` and `Timeout` 0). Edit: E-AC-CHECKOFF with N 4. Acceptance: Grep counts `^- \[x\] AC4 \(` one line and `^- \[ \] AC4 \(` zero; otherwise report `AC4 NOT MET`.

- [x] [P6-T23] Check off AC5 in FEATURE/spec.md: precondition: p6-t16 shows `TST2-DELETED-LINES: 0` and `TST1-REMOVED-TRYSAVE-LINES: 0`; pass-after-regression-tests.md shows the eleven `NAMES-T` rows and `TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile` and `TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave` `= Passed`; p6-t10 `BANNED-TEST-APIS: 0` (no `MemoryAppender`) and TOKENS-TST2 `MemoryAppender` 0. Edit: E-AC-CHECKOFF with N 5. Acceptance: Grep counts `^- \[x\] AC5 \(` one line and `^- \[ \] AC5 \(` zero; otherwise report `AC5 NOT MET`.

- [x] [P6-T24] Record the AC6 deferral (PD-11) without editing FEATURE/spec.md: write FEATURE/evidence/qa-gates/p6-t24-ac6-deferred.<TS>.md with `Timestamp:`, `Command:` (a Grep-tool search of FEATURE/spec.md for the regex `^- \[ \] AC6 \(`), `EXIT_CODE: 0`, the line `DEFERRED TO PR STEP: AC6 pull-request clause` followed by the explanation that the clause "in the pull request change description" is satisfied by the pull-request body the orchestrator authors from FEATURE/evidence/qa-gates/pr-description-inputs.<TS>.md (the actual artifact name substituted), `AC6-UNCHECKED: 1`, and the plan-side evidence of AC6 recorded as observations: fail-before-cleanup-files-phase-one.md satisfies every P1-T7 acceptance condition (the `_attachmentsAltName` row failed, the other three passed, non-zero `EXIT_CODE:` equal to its expectation) and its pass-after section shows 4 of 4 passed; p1-t9-l3-census shows `_attachmentsAltName=YesNoToAllResponse.Empty;` 3; p1-t2-attachmentsaving-tests-census shows `[DataRow(` 4, `Cleanup_Files_ResetsEveryPromptAnswerField` 5, `field.SetValue(null,YesNoToAllResponse.YesToAll);` 1 and `DoNotParallelize` 0; pr-description-inputs.<TS>.md carries the UT5 call-out marked as phase one only. Acceptance, both required: the Grep returns exactly one line (AC6 stays unchecked); the artifact names an existing pr-description-inputs artifact and records each plan-side observation. A plan-side observation that does not hold is reported as `AC6 PLAN-SIDE EVIDENCE INCOMPLETE` naming the item; the box stays unchecked in every case.

- [x] [P6-T25] Check off AC7 in FEATURE/spec.md: precondition: p6-t10 shows `ENUM-FIELDS-A: 0`, `SHOWDIALOG-CALLS-PARTIALS: 0`, TOKENS-A `YesNoToAllResponse_` 0, `new(YesNoToAll.ShowDialog)` 3, `AllPromptSessions` 2 and `YesNoToAll.ShowDialog(` 0; a Read of UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs shows the three-line comment of Listing L-A-FINAL (beginning `// A property, not an array field`) directly above `private static YesNoToAllPromptSession[] AllPromptSessions =>`, the four-element array and the `foreach` body of `Cleanup_Files` (recorded as `AC7-READ: HOLDS` in the P6-T46 inventory artifact); pass-after-regression-tests.md shows `Cleanup_Files_ResetsEveryPromptSession = Passed` and `Cleanup_Files_DoesNotThrow = Passed`. Edit: E-AC-CHECKOFF with N 7. Acceptance: Grep counts `^- \[x\] AC7 \(` one line and `^- \[ \] AC7 \(` zero; otherwise report `AC7 NOT MET`.

- [x] [P6-T26] Check off AC8 in FEATURE/spec.md: precondition: p6-t10 TOKENS-A shows `Func<string,bool>fileExists` 2, `TrySaveAttachmentDelegatetrySave` 2, `delegateTask<bool>TrySaveAttachmentDelegate(` 1, `Func<Attachment,string,Task<bool>>trySave` 0, `SaveCaseAsync(` 2, `TrySaveAttachmentAsync` 1, `File.Exists` 2 and `[ExcludeFromCodeCoverage]` 3 with `EFCC-MEMBERS:` naming the three A wrappers; a Grep-tool search of FEATURE/spec.md for the regex `Func.Attachment` returns zero lines and for `TrySaveAttachmentDelegate` at least six lines (the technical sections AC8 incorporates name the delegate type the code declares; otherwise report `AC8 NOT MET (TECHNICAL SPECIFICATIONS TYPE DIFFERS)`); `MEMBER SaveAttachmentAsyncCore`, `MEMBER SaveAttachmentCore` and `MEMBER SaveCaseAsync` rows exist in coverage-comparison.md (the cores and the six-argument `SaveCaseAsync` exist and are measured); p6-t12 shows `NUMSTAT-S:` and `NUMSTAT-M:` `0 9` and `OUTSIDE-WRITE-SET: 0` (EmailFiler.cs unchanged); p6-t3 and p6-t4 `ERRORS: 0`; pass-after-regression-tests.md shows the six `SaveAttachmentAsync` core rows and the three `SaveAttachment` core rows `= Passed` (the prompt texts are asserted by those tests). Edit: E-AC-CHECKOFF with N 8. Acceptance: Grep counts `^- \[x\] AC8 \(` one line and `^- \[ \] AC8 \(` zero; otherwise report `AC8 NOT MET`.

- [x] [P6-T27] Check off AC9 in FEATURE/spec.md: precondition: pass-after-regression-tests.md shows the seven `SaveCaseAsync` rows of `NAMES-TSC-FINAL`, the six `SaveAttachmentAsync` core rows and the three `SaveAttachment` core rows of `NAMES-TAS-FINAL` `= Passed`; compile-red-attachment-saving-seams.md satisfies every P4-T3 acceptance condition; p6-t10 TOKENS-TSC and TOKENS-TAS show `newYesNoToAllPromptSession(Prompt)` 1 each and TOKENS-TAS `SetValue(` 0. Edit: E-AC-CHECKOFF with N 9. Acceptance: Grep counts `^- \[x\] AC9 \(` one line and `^- \[ \] AC9 \(` zero; otherwise report `AC9 NOT MET`.

- [x] [P6-T28] Check off AC10 in FEATURE/spec.md: precondition: p6-t10 TOKENS-TAS shows `Cleanup_Files_ResetsEveryPromptAnswerField` 0, `Cleanup_Files_ResetsEveryPromptSession` 1, `"AllPromptSessions"` 1, `typeof(YesNoToAllPromptSession)` 1, `HaveCount(4)` 2, `OnlyHaveUniqueItems()` 1, `SetValue(` 0 and `DoNotParallelize` 0; negative-controls.md's second section satisfies the P6-T17 acceptance (the test observed failing with `but found 3`, the element restored, hashes equal). Edit: E-AC-CHECKOFF with N 10. Acceptance: Grep counts `^- \[x\] AC10 \(` one line and `^- \[ \] AC10 \(` zero; otherwise report `AC10 NOT MET`.

- [x] [P6-T29] Check off AC11 in FEATURE/spec.md: precondition: p6-t10 TOKENS-A shows `RedirectSaveFolder(` 2, `FolderPathSave=destinationPath;` 1 and `FilePathHelperSaveAlt.FolderPath=destinationPath;` 1 with `EFCC-MEMBERS:` naming the destination overload; fail-before-redirect-save-folder.md satisfies every P4-T9 acceptance condition and its pass-after section shows 11 of 11 passed; p6-t12 `OUTSIDE-WRITE-SET: 0` (AttachmentHelper.cs unchanged). Edit: E-AC-CHECKOFF with N 11. Acceptance: Grep counts `^- \[x\] AC11 \(` one line and `^- \[ \] AC11 \(` zero; otherwise report `AC11 NOT MET`.

- [x] [P6-T30] Check off AC12 in FEATURE/spec.md: precondition: p6-t12 `DELETED-PATHS:` names the legacy partial and `NUMSTAT-UCS:` reads `0 1`; p6-t10 shows `DEAD-MEMBERS-CS: 0`, `DEAD-TOKENS-PARTIALS: 0`, `LEGACY-FILE-EXISTS: False` and TOKENS-A `IsPicture` 0 and `_responseSaveFile` 0; the dossier fail-before-exception.<TS>.md carries entry (3). Edit: E-AC-CHECKOFF with N 12. Acceptance: Grep counts `^- \[x\] AC12 \(` one line and `^- \[ \] AC12 \(` zero; otherwise report `AC12 NOT MET`.

- [x] [P6-T31] Check off AC13 in FEATURE/spec.md: precondition: p6-t10 shows `EFCC-PARTIALS: 18` with `EFCC-MEMBERS:` exactly the eighteen members listed in P6-T10 (none of `GetAttachmentsInfo`, `GetAttachmentsInfoAsync`, `SaveCaseAsync`, `SaveCase`, `SaveMessageAsMsgAsync`, `SaveMessageAsMSG`, `SanitizeArrayLineTSV` among them) and per-file `[ExcludeFromCodeCoverage]` totals A 3, T 2, U 4, S 4, M 5; TOKENS-TST1 FINAL shows `[DataRow(` 6 and `"photo.jpg,report.pdf"` 2; pass-after-regression-tests.md shows the six rows of Listing L-TST1-ROWS `= Passed`. Edit: E-AC-CHECKOFF with N 13. Acceptance: Grep counts `^- \[x\] AC13 \(` one line and `^- \[ \] AC13 \(` zero; otherwise report `AC13 NOT MET`.

- [x] [P6-T32] Check off AC14 in FEATURE/spec.md: precondition: p6-t10 TOKENS-U FINAL holds (`MovedMailsHeader` 2, `string.Join("\t",MovedMailsHeader)` 1, `fileExists(Path.Combine(strFileLocation,strFileName))` 1, `writeTextFile(` 1, `SanitizeArray(` 0, `string[14,2]` 0, `File.Exists` 1, `FileIO2.WriteTextFile` 1, `[ExcludeFromCodeCoverage]` 4) with `EFCC-MEMBERS:` naming the two-parameter wrapper and not `SanitizeArrayLineTSV`; TOKENS-TST1 shows `"SanitizeArray"` 0 and `"SanitizeArrayLineTSV"` 1; pass-after-regression-tests.md shows `SanitizeArrayLineTSV_WhenArrayContainsNullsAndWhitespaceControlCharacters_ReturnsSanitizedLine` and both `StripTabsCrLf` tests `= Passed`; p6-t12 `OUTSIDE-WRITE-SET: 0` (AppOlObjects.cs unchanged). Edit: E-AC-CHECKOFF with N 14. Acceptance: Grep counts `^- \[x\] AC14 \(` one line and `^- \[ \] AC14 \(` zero; otherwise report `AC14 NOT MET`.

- [x] [P6-T33] Check off AC15 in FEATURE/spec.md: precondition: fail-before-write-csv.md satisfies every P2-T6 acceptance condition (both tests failed after the seam step with the named substrings, non-zero `EXIT_CODE:` equal to its expectation) and its pass-after section shows 2 of 2 passed; p6-t10 TOKENS-TUL holds (`HaveCount(13)` 1, `Path.Combine(LogFolder,LogFileName)` 1, `Triage\tFolderName\tSent_On` 1, `File.` 0). Edit: E-AC-CHECKOFF with N 15. Acceptance: Grep counts `^- \[x\] AC15 \(` one line and `^- \[ \] AC15 \(` zero; otherwise report `AC15 NOT MET`.

- [x] [P6-T34] Check off AC16 in FEATURE/spec.md: precondition: p5-t11-cr1-spec956 satisfies every P5-T11 acceptance condition (the six `S956-` literal counts, unchanged check-box counts, two added lines, numstat `4 2`); p6-t12 `NUMSTAT-SPEC956:` reads `4 2` and `OUTSIDE-WRITE-SET: 0` (the #956 code-review record is not in the diff). Edit: E-AC-CHECKOFF with N 16. Acceptance: Grep counts `^- \[x\] AC16 \(` one line and `^- \[ \] AC16 \(` zero; otherwise report `AC16 NOT MET`.

- [x] [P6-T35] Check off AC17 in FEATURE/spec.md: precondition: p6-t10 shows `USINGS-EXACT-FILES: 5`, `BANNED-USINGS-PARTIALS: 0` and `USING-SYSTEM-PARTIAL-FILES: 5`; p6-t3 and p6-t4 show `EXIT_CODE: 0`, `ERRORS: 0` and `SKIP_CORECOMPILE_LINES: 0`. Edit: E-AC-CHECKOFF with N 17. Acceptance: Grep counts `^- \[x\] AC17 \(` one line and `^- \[ \] AC17 \(` zero; otherwise report `AC17 NOT MET`.

- [x] [P6-T36] Check off AC18 in FEATURE/spec.md: precondition: p6-t10 TOKENS-E FINAL holds (`boolresult;` 1, `finally{` 1, `result=awaitInvokeFilerAsync(config,mailHelpers);` 1, `varresult=awaitInvokeFilerAsync(config,mailHelpers);` 0, `ResetFilerPromptState();` 1, `protectedinternalvirtualvoidResetFilerPromptState()` 1, `SortEmail.Cleanup_Files();` 1, `returnresult;` 1); fail-before-efc-filer-cleanup.md satisfies every P5-T7 acceptance condition and its pass-after section shows 3 of 3 and the eleven archive-root tests passed with `ART-PORCELAIN: EMPTY`; p6-t12 `NUMSTAT-QFT:` reads `1 0`; pass-after-regression-tests.md shows the three `NAMES-TEF` rows `= Passed`. Edit: E-AC-CHECKOFF with N 18. Acceptance: Grep counts `^- \[x\] AC18 \(` one line and `^- \[ \] AC18 \(` zero; otherwise report `AC18 NOT MET`.

- [x] [P6-T37] Check off AC19 in FEATURE/spec.md: precondition: p6-t10 shows `TODOMODEL-FILE-EXISTS: False`, `TODOMODEL-CSPROJ-MATCHES: 2` and `TODOMODEL-CSPROJ-FILES: ToDoModel.Test\ToDoModel.Test.csproj`; p5-t10-todomodel-deletion satisfies every P5-T10 acceptance condition (only the one ` D` row under ToDoModel and ToDoModel.Test); the dossier carries entry (5); p6-t3 and p6-t4 `ERRORS: 0`. Edit: E-AC-CHECKOFF with N 19. Acceptance: Grep counts `^- \[x\] AC19 \(` one line and `^- \[ \] AC19 \(` zero; otherwise report `AC19 NOT MET`.

- [x] [P6-T38] Check off AC20 in FEATURE/spec.md: precondition: the six fail-before artifacts (fail-before-save-case.md, fail-before-cleanup-files-phase-one.md, fail-before-write-csv.md, fail-before-try-save-retry.md, fail-before-redirect-save-folder.md, fail-before-efc-filer-cleanup.md) each carry a non-zero `EXIT_CODE:` equal to their `ExpectedExitCode:` and name their failing rows, and their tasks (P1-T6, P1-T7, P2-T6, P3-T3, P4-T9, P5-T7) carry the `[expect-fail]` tag in this plan; the dossier fail-before-exception.<TS>.md satisfies the P5-T12 acceptance with its eight entries. Edit: E-AC-CHECKOFF with N 20. Acceptance: Grep counts `^- \[x\] AC20 \(` one line and `^- \[ \] AC20 \(` zero; otherwise report `AC20 NOT MET`.

- [x] [P6-T39] Check off AC21 in FEATURE/spec.md: precondition: p6-t10 shows `BANNED-TEST-APIS: 0`, `NON-APPROVED-FRAMEWORKS: 0`, `TESTS6-PRESENT: 6`, `RETRY-READ: NONE FOUND` and TOKENS-TAS `SetValue(` 0; coverage-post-change.md shows the run through `CMD-COVERAGE-DIRECT` with `RUNSETTINGS` (scripts\vscode\TaskMaster.cli.runsettings, Workers 0 and class-level scope recorded by P0-T4), `EXIT_CODE: 0` with an empty `FAILED-SET:` (branch (a) of P6-T7; the AC's "full test run passes" clause) and every `SANDBOX-` value `False`; pass-after-regression-tests.md shows every `SANDBOX-` value `False`. Under branch (b) leave AC21 unchecked and report `AC21 NOT MET` with the `FAILED-SET:` names. Edit: E-AC-CHECKOFF with N 21. Acceptance: Grep counts `^- \[x\] AC21 \(` one line and `^- \[ \] AC21 \(` zero; otherwise report `AC21 NOT MET`.

- [x] [P6-T40] Check off AC22 in FEATURE/spec.md: precondition: p6-t12 shows `OUTSIDE-WRITE-SET: 0`, `WRITE-SET-MISSING: 0`, the two `DELETED-PATHS:` and `NUMSTAT-UCT:` `3 0`; p6-t10 `CMD-CSPROJ` rows match the final column (the three UCT entries in the existing form). Edit: E-AC-CHECKOFF with N 22. Acceptance: Grep counts `^- \[x\] AC22 \(` one line and `^- \[ \] AC22 \(` zero; otherwise report `AC22 NOT MET`.

- [x] [P6-T41] Check off AC23 in FEATURE/spec.md: precondition: p6-t10 shows `MAX-LINES:` at most 499 over the twelve `PATHS-CSHARP-FINAL` rows with the `AC23-CLOSEST:` rows for EfcDataModel.cs, SortEmail_AttachmentSaving_Tests.cs and SortEmail_Tests.cs quoted. Edit: E-AC-CHECKOFF with N 23. Acceptance: Grep counts `^- \[x\] AC23 \(` one line and `^- \[ \] AC23 \(` zero; otherwise report `AC23 NOT MET`.

- [x] [P6-T42] Check off AC24 in FEATURE/spec.md: precondition: toolchain-final-pass.md satisfies every P6-T11 acceptance condition for one iteration in CLAUDE.md order, with both rebuilds showing `SKIP_CORECOMPILE_LINES: 0`, and the P6-T7 row reads exit code 0 (branch (a)); under branch (b) leave AC24 unchecked and report `AC24 NOT MET: TryAddValuesAsync_UpdatesExistingValue (#780) failed in the final coverage run`. Edit: E-AC-CHECKOFF with N 24. Acceptance: Grep counts `^- \[x\] AC24 \(` one line and `^- \[ \] AC24 \(` zero; otherwise report `AC24 NOT MET`.

- [x] [P6-T43] Check off AC25 in FEATURE/spec.md: precondition: coverage-baseline.md (P0-T11, written before P1-T8, the first production edit) and coverage-post-change.md (P6-T7, the test step of the final pass) each hold the JaCoCo projection and the one-line first-party summary with the percentages against the floors; coverage-comparison.md satisfies every P6-T8 acceptance condition (`NONEXEMPT-SET-MATCHES-BASELINE: True`, `CONTROL-DIFFERS-BASELINE: True`, both `NOT-LOWER` flags `True`, the four exemption sets listed by file and content) and every P6-T9 acceptance condition (`MEMBERS-BELOW-90: 0`, `E-CHANGED-LINES-COVERED: 3`). Edit: E-AC-CHECKOFF with N 25. Acceptance: Grep counts `^- \[x\] AC25 \(` one line and `^- \[ \] AC25 \(` zero; otherwise report `AC25 NOT MET`.

- [x] [P6-T44] Check off AC26 in FEATURE/spec.md: precondition: p6-t12 `RAW-DOC-PATHS: 0`; p6-t16 `RAW-DOCUMENT-FILES: 0`; a `CMD-EVIDENCE-FIELDS` run issued by this task (`INHERITED` substituted exactly as in P6-T16), covering every artifact written through P6-T43, prints `EVIDENCE-MISSING-FIELDS: 0`, `NONCANONICAL-SUBFOLDER-FILES: 0`, `FIELD-CHECK-CONTROL: False`, `FIELD-CHECK-POSITIVE: True`, `SUBFOLDER-CHECK-FLAGS-OTHER: True` and `SUBFOLDER-CHECK-FLAGS-QA-GATES: False`; its printed lines are recorded under the heading `## AC26 check-off evidence fields (P6-T44)` in the P6-T46 inventory artifact, as P6-T25 records `AC7-READ:` there; a non-zero count is handled as in P6-T16 before the Edit (a missing field is added to the named artifact with the value its task defines and the run is repeated; a non-zero `NONCANONICAL-SUBFOLDER-FILES:` is `NONCANONICAL EVIDENCE FILE`: stop and report the `NONCANONICAL:` rows with the box unchecked, because the executor does not move a file it did not write; `FIELD-CHECK-CONTROL: True` or `FIELD-CHECK-POSITIVE: False` is `FIELD CHECK NOT DISCRIMINATING`: stop with the box unchecked; `SUBFOLDER-CHECK-FLAGS-OTHER: False` or `SUBFOLDER-CHECK-FLAGS-QA-GATES: True` is `SUBFOLDER CHECK NOT DISCRIMINATING`: stop with the box unchecked; the precondition's printed values are gates and the acceptance remains the single Grep clause, so no clause count changes); the six fail-before artifacts carry non-zero `EXIT_CODE:` values and failing names (P6-T38 precondition); compile-red-attachment-saving-seams.md shows the four `MISSING_` counts at least 1. Edit: E-AC-CHECKOFF with N 26. Acceptance: Grep counts `^- \[x\] AC26 \(` one line and `^- \[ \] AC26 \(` zero; otherwise report `AC26 NOT MET`.

- [x] [P6-T45] Record the AC27 deferral (PD-11) without editing FEATURE/spec.md: write FEATURE/evidence/qa-gates/p6-t45-ac27-deferred.<TS>.md with `Timestamp:`, `Command:` (a Grep-tool search of FEATURE/spec.md for the regex `^- \[ \] AC27 \(`), `EXIT_CODE: 0`, the line `DEFERRED TO PR STEP: AC27 is satisfied by the pull-request body the orchestrator authors from FEATURE/evidence/qa-gates/pr-description-inputs.<TS>.md` (the actual artifact name substituted) and `AC27-UNCHECKED: 1`. Acceptance, both required: the Grep returns exactly one line; the artifact names an existing pr-description-inputs artifact.

- [x] [P6-T46] Verify the acceptance inventory read-only and run the final hygiene sweep: run `CMD-SPEC-CHECK` (`STAGE` `final`), then `CMD-EVIDENCE-FIELDS` (`INHERITED` substituted exactly as in P6-T16: the `INHERITED-CLAUSE-A:` list minus the P0-T1 artifact and the P0-T2 artifact, unless P0-T2's `PRE-EXISTING-EVIDENCE:` names that path; this third run confirms every artifact, including the P6-T45 artifact written after the P6-T44 run), and then `CMD-SWEEP` (the last command of this plan, after every check-off and every artifact write), and write FEATURE/evidence/qa-gates/p6-t46-ac-inventory.<TS>.md with `Timestamp:`, `Command:` (the three payloads in order), `EXIT_CODE:` (scoped to the `CMD-SWEEP` payload, its process exit code), a list of each AC with its check-off task and its evidence artifact, `AC7-READ: HOLDS` (the P6-T25 Read), the printed lines of the P6-T44 `CMD-EVIDENCE-FIELDS` run under the heading `## AC26 check-off evidence fields (P6-T44)`, every CMD-SPEC-CHECK line, every count and row of this task's own CMD-EVIDENCE-FIELDS run (`EVIDENCE-FILES-CHECKED:`, `EVIDENCE-MISSING-FIELDS:`, `FIELD-CHECK-CONTROL:`, `FIELD-CHECK-POSITIVE:`, `SUBFOLDER-CHECK-FLAGS-OTHER:`, `SUBFOLDER-CHECK-FLAGS-QA-GATES:`, `NONCANONICAL-SUBFOLDER-FILES:`, `EVIDENCE-INHERITED-SKIPPED:`, any `MISSING-FIELDS:` row and any `NONCANONICAL:` row) and every CMD-SWEEP count. Acceptance, all five required (the clause count is unchanged; the positive control and the two subfolder controls join the first clause): `EVIDENCE-MISSING-FIELDS: 0`, `NONCANONICAL-SUBFOLDER-FILES: 0`, `FIELD-CHECK-CONTROL: False`, `FIELD-CHECK-POSITIVE: True`, `SUBFOLDER-CHECK-FLAGS-OTHER: True` and `SUBFOLDER-CHECK-FLAGS-QA-GATES: False` in this task's run (a missing field is repaired as in P6-T16 by adding it to the named artifact, this artifact being rewritten, and never reverts a check-off box; a non-zero `NONCANONICAL-SUBFOLDER-FILES:` is `NONCANONICAL EVIDENCE FILE`: stop and report the `NONCANONICAL:` rows; `FIELD-CHECK-CONTROL: True` or `FIELD-CHECK-POSITIVE: False` is `FIELD CHECK NOT DISCRIMINATING`: stop; `SUBFOLDER-CHECK-FLAGS-OTHER: False` or `SUBFOLDER-CHECK-FLAGS-QA-GATES: True` is `SUBFOLDER CHECK NOT DISCRIMINATING`: stop); under branch (a) of P6-T7, `AC-CHECKED: 25`, `AC6-UNCHECKED: 1`, `AC27-UNCHECKED: 1` and `AC-ANY-UNCHECKED: 2` (AC6 and AC27 deferred, no other AC unchecked), or under branch (b), `AC-CHECKED: 23`, `AC6-UNCHECKED: 1`, `AC27-UNCHECKED: 1` and `AC-ANY-UNCHECKED: 4` with AC21 and AC24 reported `NOT MET` by P6-T39 and P6-T42; `ACCOUNT-TOKEN-FILES: 0`, `PROFILE-LEAF-FILES: 0`, `MACHINE-TOKEN-FILES: 0`, `WORKTREE-ROOT-FILES: 0`, `USERS-PATH-FILES: 0` and `RAW-DOCUMENT-FILES: 0` (a non-zero count is repaired as in P6-T16 and the sweep re-run, this artifact being rewritten); `FILES:` at least the number of evidence artifacts this plan names (fifty or more); the plan outcome is reported as complete with AC6 and AC27 deferred to the PR step under branch (a), or as INCOMPLETE naming AC21 and AC24 under branch (b), never as PASS over an unchecked AC, and any unchecked AC other than those is reported with its `NOT MET` reason as INCOMPLETE.

### Phase 7 — Review-Residual Remediation (CR-1 Synchronous Image-Arm Test, CR-3 Unused Using) and the Full Toolchain Pass Over the Whole Solution

Phase rule (revision 2.0; PD-16; the Phases 7 and 8 convention). P7-T1 to P7-T5 observe, edit, format, census and scope-run the two Write Set test files TAS and TSC; P7-T6 to P7-T11 form one toolchain pass in CLAUDE.md order over the whole solution (format, check, analyzer rebuild, nullable rebuild, scoped tests, tests with coverage), which is also the first rebuild and test run after the P6-T13 documentation-comment edit; P7-T12 and P7-T13 compare coverage and read the CR-1 arm; P7-T14 closes the loop record; P7-T15 re-verifies the footprint at the Phase 0 to 7 anchor; P7-T16 closes the phase. Every Phase 7 artifact carries `ITERATION:` (1 on the first pass). When P7-T6 reports `WRITESET-CHANGED-COUNT:` above 0, the pass restarts at P7-T6 with `ITERATION:` incremented and every later Phase 7 artifact is re-written for the new iteration; at most three iterations run, and a fourth is `TOOLCHAIN LOOP NOT CONVERGING`: stop and report. Any other failure in P7-T6 to P7-T11 is a defect this plan did not predict: stop and report it with the failing output; the executor does not repair source, tests or gates. Source files are written only with the Edit tool (E-TAS-SS4, E-TSC-USING); no Phase 7 task edits FEATURE/spec.md, A, T, U, S, M, E or any other Write Set path, and no check box of FEATURE/spec.md changes. Every task commits and pushes under the per-task rule (`wip(959): P7-T# <summary>`), staging the Write Set paths it changed and FEATURE/. Every `git` command is `git -C WORKTREE ...`, one per call; a Grep-tool pattern is a regular expression with escaped brackets and parentheses.

- [x] [P7-T1] Record the pre-edit observations: run `CMD-LINE-CONDITION` (`DOC` `coverage\final-959.cobertura.xml`, the Phase 6 document left on disk by P6-T7 and P6-T8); then, with the Grep tool: TAS (UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs) for the regex `^\s*\[TestMethod\]` (count mode) recorded as `TAS-TESTMETHOD-LINES:`, for `SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly` as `TAS-SS4-NAME-LINES:`, for `Be\(YesNoToAllResponse\.NoToAll\)` as `TAS-NOTOALL-ASSERT-LINES:`, for `RR\. Scenario` as `TAS-RR-SUMMARY-LINES:` and for `^` as `TAS-LINES:`; TSC (UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs) for `^using System;` as `TSC-USING-SYSTEM-LINES:`, for `^` as `TSC-LINES:` and for the PD-16 alternatives `Func<|Action|DateTime|Exception|Guid|Math\.|Console|Environment|StringComparison|Array\.|Convert\.|Tuple|Nullable|Lazy<|IDisposable|EventArgs|TimeSpan|Enum\.` as `TSC-SYSTEM-IDENTIFIER-LINES:` with every matching line quoted; then a Read of A (UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs) lines 143 to 145 recorded trimmed as `A-LINE-143-TEXT:`, `A-LINE-144-TEXT:` and `A-LINE-145-TEXT:`. Write FEATURE/evidence/qa-gates/p7-t1-pre-edit-observations.<TS>.md with `Timestamp:`, `Command:` (the payload, the eight Grep-tool searches and the Read), `EXIT_CODE:` (the payload's process exit code, the only `pwsh` invocation), `ITERATION: 1`, an `Output Summary:` and every value above. Acceptance, all six required: `A-CLASS-NODES: 1`, `A-LINE-143-COUNT: 1` and `A-LINE-143-BRANCH: True`; `A-LINE-143-CONDITION: 50% (1/2)` with `A-SAVEATTACHMENT-BRANCH-RATE: 0.75` (the false-before state of CR-1; a different value, or an absent document, is `CR-1 PRECONDITION DIFFERS`: stop and report, because the Phase 7 discriminator would then not be the one PD-16 names); `A-LINE-143-TEXT:` equals `var overwritePrompt = attachmentHelper.AttachmentInfo.IsImage`, `A-LINE-144-TEXT:` equals `? picturesOverwritePrompt` and `A-LINE-145-TEXT:` equals `: attachmentsOverwritePrompt;` (the ternary still sits at line 143; otherwise `CR-1 LINE MOVED`: stop); `TAS-TESTMETHOD-LINES: 11`, `TAS-SS4-NAME-LINES: 0`, `TAS-NOTOALL-ASSERT-LINES: 1`, `TAS-RR-SUMMARY-LINES: 1` and `TAS-LINES: 437` (the E-TAS-SS4 anchor is unique and the file is as cited); `TSC-USING-SYSTEM-LINES: 1` and `TSC-LINES: 343`; `TSC-SYSTEM-IDENTIFIER-LINES:` recorded (0 expected; a value above 0 routes P7-T3 to its skip branch and is not a stop). The commit stages FEATURE/ (`wip(959): P7-T1 pre-edit observations`).

- [x] [P7-T2] Add the CR-1 synchronous image-arm test with Edit E-TAS-SS4 on TAS, then verify with the Grep tool over TAS: `SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly` recorded as `TAS-SS4-NAME-LINES:`, `^\s*\[TestMethod\]` (count mode) as `TAS-TESTMETHOD-LINES:`, `CreateAttachmentMock\("photo\.jpg"\)` as `TAS-PHOTO-MOCK-LINES:` and `^` as `TAS-LINES:`. Write FEATURE/evidence/qa-gates/p7-t2-tas-ss4-edit.<TS>.md with `Timestamp:`, `Command:` (the Edit and the four Grep-tool searches), `EXIT_CODE: 0` (a Grep-tool task; the row names the fourth search), `ITERATION: 1`, an `Output Summary:` and the four values. Acceptance, all three required: `TAS-SS4-NAME-LINES: 1` (0 before the Edit, the false-before state recorded by P7-T1); `TAS-TESTMETHOD-LINES: 12` (11 before); `TAS-PHOTO-MOCK-LINES: 4` (3 before the Edit: AS1 at line 37, AS2 at 71 and RR at 308 pass `CreateAttachmentMock("photo.jpg")` inside a `CreateHelper(` call, and the SS4 body adds the fourth as a standalone statement) and `TAS-LINES: 469` (437 plus 32; recorded, the formatter of P7-T4 re-measures). An Edit whose old text is not found exactly once is `EDIT ANCHOR NOT UNIQUE`: stop and report. The commit stages UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs and FEATURE/ (`wip(959): P7-T2 CR-1 SS4 test`).

- [x] [P7-T3] Remove the unused directive of CR-3: when P7-T1 recorded `TSC-SYSTEM-IDENTIFIER-LINES: 0`, apply Edit E-TSC-USING on TSC; when it recorded a value above 0, this task text authorizes the skip: apply no Edit, record `CR-3 SKIPPED: System required` with the quoted matching lines, and treat the acceptance below as met with `TSC-USING-SYSTEM-LINES: 1` and `TSC-LINES: 343`. Verify with the Grep tool over TSC: `^using System;` as `TSC-USING-SYSTEM-LINES:`, `^using System\.Collections\.Generic;` as `TSC-USING-GENERIC-LINES:` and `^` as `TSC-LINES:`; then a Read of TSC lines 1 to 3 recorded as `TSC-HEAD:`. Write FEATURE/evidence/qa-gates/p7-t3-tsc-using-edit.<TS>.md with `Timestamp:`, `Command:` (the Edit or the skip statement, and the three Grep-tool searches and the Read), `EXIT_CODE: 0` (a Grep-tool task; the row names the third search), `ITERATION: 1`, an `Output Summary:` and every value. Acceptance, all three required under the edit branch: `TSC-USING-SYSTEM-LINES: 0` (1 before the Edit); `TSC-USING-GENERIC-LINES: 1` with `TSC-HEAD:` beginning `using System.Collections.Generic;`; `TSC-LINES: 342`. An Edit whose old text is not found exactly once is `EDIT ANCHOR NOT UNIQUE`: stop and report. The commit stages UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs (edit branch) and FEATURE/ (`wip(959): P7-T3 CR-3 unused using`).

- [x] [P7-T4] Format and census the two edited files: run `CMD-SCOPED-FORMAT` (`PATHS` `PATHS-TAS, PATHS-TSC`; `TASKID` `p7-t4`), then `CMD-CENSUS` with `PATHS-TAS` and the TOKENS-TAS list plus the token `SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly`, then `CMD-CENSUS` with `PATHS-TSC` and the TOKENS-TSC list plus the token `usingSystem;`, then `CMD-LINES` with `PATHS-CSHARP-FINAL`. Write FEATURE/evidence/qa-gates/p7-t4-scoped-format-and-census.<TS>.md with `Timestamp:`, `Command:` (the four payloads in order), `EXIT_CODE:` (scoped to the `CMD-LINES` payload, the last invocation, its process exit code), `ITERATION: 1`, `FORMAT_EXIT_CODE:`, the `BEFORE` and `AFTER` hash rows of both files, `CHECK_EXIT_CODE:`, every `TOKEN`, `LINES` and `SHA256` line of both censuses and every `CMD-LINES` row. Acceptance, all four required: `FORMAT_EXIT_CODE: 0` with both files' `BEFORE` and `AFTER` hashes recorded (the write-mode observation; either equality or difference is recorded, and the read-only check is the gate) and `CHECK_EXIT_CODE: 0`; every TOKENS-TAS total equals the SS4 column of the Census Expectations (`[TestMethod]` 12, `SortEmail.SaveAttachment(` 4, the SS4 name 1, the rest unchanged from FINAL); every TOKENS-TSC total equals the SS4 column (`usingSystem;` 0 under the P7-T3 edit branch, 1 under its skip branch; the rest unchanged from FINAL); `MAX-LINES:` at most 499 with the twelve `LINES` rows present and the rows for `UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs` (predicted 469) and `UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs` (predicted 342) quoted individually under `AC23-CLOSEST:` together with the TST1 and EfcDataModel.cs rows (488 and 485 as P6-T10 recorded). A census or ceiling mismatch is `POST-FORMAT CENSUS MISMATCH` and a non-zero check exit code is `FORMAT CHECK FAILED AFTER EDIT`: stop and report. The commit stages the two test files (when an `AFTER` hash differs from its `BEFORE`) and FEATURE/ (`wip(959): P7-T4 scoped format and census`).

- [x] [P7-T5] Build the test project and run the attachment-saving class: `CMD-BUILD-TEST` (`PROJECT` `UtilitiesCS.Test`; `TASKID` `p7-t5`), then `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-ATTSAVE`, `TASKID` `p7-t5`). Write FEATURE/evidence/regression-testing/p7-t5-attsave-run.<TS>.md with `Timestamp:`, `Command:` (both payloads), `EXIT_CODE:` (the printed `VSTEST_EXIT_CODE:`, the last invocation), `ITERATION: 1`, `MSBUILD_EXIT_CODE:`, `ERROR_LINES:`, `ERROR_CODES:`, `DLL_ADVANCED:`, `RUNSETTINGS-HASH-NOW:`, the six `SANDBOX-` lines, the `COUNTERS` line, `RESULT_COUNT:`, `SEQUENCE_FILES:` and every `RESULT` line. Acceptance, all five required: `MSBUILD_EXIT_CODE: 0`, `ERROR_LINES: 0` and `DLL_ADVANCED: True`; `VSTEST_EXIT_CODE: 0` with `COUNTERS total=12 executed=12 passed=12 failed=0`; the twelve `RESULT` rows are exactly `NAMES-TAS-FINAL2`, each `= Passed`, including `SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly = Passed`; `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:` of P0-T4; every `SANDBOX-` value is `False` and `SEQUENCE_FILES: 0`. A failing or absent SS4 row is `CR-1 TEST FAILED`: stop and report its `MESSAGE` (the executor does not edit the test); a build error is `BUILD RED AFTER CR EDITS`: stop and report `ERROR_CODES:`. The commit stages FEATURE/ (`wip(959): P7-T5 attachment-saving class run`).

- [x] [P7-T6] Run the repository-wide formatter with `CMD-FORMAT-REPO` (`TASKID` `p7-t6`; canonical command `dotnet tool run csharpier format .`) and write FEATURE/evidence/qa-gates/p7-t6-csharpier-format.<TS>.md with `Timestamp:`, `Command:`, `EXIT_CODE:` (the printed `FORMAT_EXIT_CODE:`, the only invocation), `ITERATION:`, `FORMAT_EXIT_CODE:`, every `WRITESET-CHANGED:` line, `WRITESET-CHANGED-COUNT:`, both porcelain counts, `PORCELAIN-SAME:` and the formatter's summary line as an observation. Acceptance, all three required: `FORMAT_EXIT_CODE: 0`; `WRITESET-CHANGED-COUNT: 0` (the twelve C# Write Set hashes are identical before and after; a non-zero value triggers the phase rule's restart); `PORCELAIN-SAME: True` (`False` is `FORMAT TOUCHED OUT-OF-SET FILE`: stop and report, do not revert). The commit stages FEATURE/ (`wip(959): P7-T6 repository format`).

- [x] [P7-T7] Verify formatting read-only with `CMD-CHECK-REPO` (`TASKID` `p7-t7`; canonical command `dotnet tool run csharpier check .`) and write FEATURE/evidence/qa-gates/p7-t7-csharpier-check.<TS>.md with `Timestamp:`, `Command:`, `EXIT_CODE:`, `ITERATION:` and `CHECKED-LINE:`. Acceptance, both required: `CHECK_EXIT_CODE: 0` recorded as `EXIT_CODE: 0`; `CHECKED-LINE:` matches `Checked <N> files` with a positive N. The commit stages FEATURE/ (`wip(959): P7-T7 repository check`).

- [x] [P7-T8] Run the analyzer gate with `CMD-REBUILD` (analyzer `GATEARGS`, `TASKID` `p7-t8`) over TaskMaster.sln and write FEATURE/evidence/qa-gates/p7-t8-msbuild-analyzers.<TS>.md with the P0-T7 field set and `ITERATION:`. Acceptance, all five required: `EXIT_CODE: 0`; `SKIP_CORECOMPILE_LINES: 0`; the four `_CSC_OUT_LINES:` values each at least 1; `ERRORS: 0`; `WRITESET_DIAGNOSTIC_LINES:` at most `ANALYZE-BASELINE-WRITESET-DIAGNOSTICS:` of P0-T7. The commit stages FEATURE/ (`wip(959): P7-T8 analyzer rebuild`).

- [x] [P7-T9] Run the type-check gate with `CMD-REBUILD` (nullable `GATEARGS`, `TASKID` `p7-t9`; no Nullable property override) over TaskMaster.sln and write FEATURE/evidence/qa-gates/p7-t9-msbuild-nullable.<TS>.md with the P0-T7 field set and `ITERATION:`. Acceptance, all five required: `EXIT_CODE: 0`; `SKIP_CORECOMPILE_LINES: 0`; the four `_CSC_OUT_LINES:` values each at least 1; `ERRORS: 0`; `WRITESET_DIAGNOSTIC_LINES:` at most `NULLABLE-BASELINE-WRITESET-DIAGNOSTICS:` of P0-T8. The commit stages FEATURE/ (`wip(959): P7-T9 nullable rebuild`).

- [x] [P7-T10] Run the scoped suites on the Phase 7 tree: `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-SORTEMAIL`, `TASKID` `p7-t10`), `CMD-VSTEST` (`ASSEMBLY-QFT`, `FILTER-EFC-CLEANUP`, `TASKID` `p7-t10-cleanup`) and `CMD-VSTEST` (`ASSEMBLY-QFT`, `FILTER-EFC-ARCHIVE`, `TASKID` `p7-t10-archive`); record `git rev-parse HEAD` as `SUPERSEDES:` before writing; then rewrite FEATURE/evidence/regression-testing/pass-after-regression-tests.md (fixed name) in full with `Timestamp:`, `ITERATION: 2`, `SUPERSEDES:`, `WRITTEN-BY: P7-T10`, `Command:` (the three payloads), `EXIT_CODE:` (the SortEmail run's `VSTEST_EXIT_CODE:`), an `Output Summary:`, and for each run `RUNSETTINGS-HASH-NOW:`, the six `SANDBOX-` lines, the `COUNTERS` line, `RESULT_COUNT:`, `SEQUENCE_FILES:` and every `RESULT` line, the two QuickFiler.Test runs under a section headed `## QuickFiler.Test (P7-T10)` with `CLEANUP-VSTEST_EXIT_CODE:` and `ARCHIVE-VSTEST_EXIT_CODE:`. Acceptance, all five required: `EXIT_CODE: 0` with `COUNTERS total=56 executed=56 passed=56 failed=0`; the fifty-six `RESULT` rows are exactly the union of `NAMES-TST1-FINAL`, `NAMES-T12`, `NAMES-TSC-FINAL`, `NAMES-TAS-FINAL2` and `NAMES-TUL`, each `= Passed`; `CLEANUP-VSTEST_EXIT_CODE: 0` with `COUNTERS total=3 executed=3 passed=3 failed=0` and the rows exactly `NAMES-TEF`, and `ARCHIVE-VSTEST_EXIT_CODE: 0` with `COUNTERS total=11 executed=11 passed=11 failed=0` and the rows exactly `NAMES-EFC-ARCHIVE`, each `= Passed`; every `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:` of P0-T4; every `SANDBOX-` value is `False` and every `SEQUENCE_FILES: 0`. The commit stages FEATURE/ (`wip(959): P7-T10 scoped suites`).

- [x] [P7-T11] Run the repository-wide test and coverage pass on the Phase 7 tree: `CMD-COVERAGE-DIRECT` (`STAGE` `final`, the same `EXCLUSION` as P0-T11; the Phase 6 document of the same name is overwritten, its line-143 reading having been recorded by P7-T1) as a background invocation polled until its final `TRX_PRESENT:` line, then `CMD-COVERAGE-POST` (`STAGE` `final`); record `git rev-parse HEAD` as `SUPERSEDES:` before writing; then rewrite FEATURE/evidence/qa-gates/coverage-post-change.md (fixed name) in full with `ITERATION: 2`, `SUPERSEDES:`, `WRITTEN-BY: P7-T11`, the P0-T11 field set with the `FINAL-` labels (`FINAL-UCS-LINE:`, `FINAL-UCS-BRANCH:`, `FINAL-QF-LINE:`, `FINAL-QF-BRANCH:`, `FINAL-FIRST-PARTY-LINE-PERCENT:`, `FINAL-FIRST-PARTY-BRANCH-PERCENT:`), `NEWLY-FAILING:` (the names of `FAILED-SET:` absent from coverage-baseline.md's `FAILED-SET:`, or `NONE`), and the transcribed Phase 6 figures `PHASE6-FIRST-PARTY-LINE-PERCENT: 85.39`, `PHASE6-FIRST-PARTY-BRANCH-PERCENT: 79.81` and `PHASE6-TOTAL: 7393`. Branches as in P0-T11 and P6-T7, with (c) named `COVERAGE FLOOR NOT MET`. Acceptance, all eight required: the projection holds `UtilitiesCS` and `QuickFiler` packages with `LINE` and `BRANCH` counters; `FIRST-PARTY-LINE-PERCENT:` at least 80 and `FIRST-PARTY-BRANCH-PERCENT:` at least 75 with `LINE-FLOOR: MET` and `BRANCH-FLOOR: MET`; `FAILED-SET:` contains none of the names of `NAMES-TST1-FINAL`, `NAMES-T12`, `NAMES-TSC-FINAL`, `NAMES-TAS-FINAL2`, `NAMES-TUL`, `NAMES-TEF` or `NAMES-EFC-ARCHIVE`; `NEWLY-FAILING: NONE`, or only `TryAddValuesAsync_UpdatesExistingValue` under branch (b); the summary reads `Total 7394, executed 7394` (7393 plus SS4 under the unchanged exclusion; a different total is `TEST TOTAL DIFFERS`: stop and report) with `passed 7394, failed 0` under branch (a); `EXIT_CODE:` equals its declared expectation; every `SANDBOX-` value is `False`; the artifact contains no absolute path. coverage\final-959.cobertura.xml and coverage\final-959.jacoco.xml stay on disk for P7-T12 and P7-T13. The commit stages FEATURE/ (`wip(959): P7-T11 coverage run`).

- [x] [P7-T12] Compare coverage with `CMD-COVERAGE-TEXTS` (`STAGE` `final`, `BASELINE-HASH` the `BASELINE-NONEXEMPT-HASH:` of coverage-baseline.md); record `git rev-parse HEAD` as `SUPERSEDES:` before writing; then rewrite FEATURE/evidence/qa-gates/coverage-comparison.md (fixed name) in full with `Timestamp:`, `ITERATION: 2`, `SUPERSEDES:`, `WRITTEN-BY: P7-T12`, `Command:`, `EXIT_CODE:` (the payload's exit code), an `Output Summary:` holding every printed line, the two Phase 6 comparison rows `PHASE6-LINE-NOT-LOWER:` and `PHASE6-BRANCH-NOT-LOWER:` (each `True` when the P7-T11 `FIRST-PARTY-LINE-PERCENT:` is at least 85.39 and `FIRST-PARTY-BRANCH-PERCENT:` at least 79.81 at two decimals, else `False`), and the `Reading:` paragraph that restates the PD-8 rule. Acceptance, all ten required: the nine P6-T8 clauses unchanged (`SORTEMAIL-DIR-CLASSES:` at least 1 and `TRYSAVE-CLASS-FOUND: True`; the four `EXEMPT-*-COUNT` values 1 with `GUARD-CONDITION-LINES:` naming one line; `NONEXEMPT-SET-MATCHES-BASELINE: True`, `False` being `AC25: NEW UNCOVERED LINE`: stop; `CONTROL-LINE:` greater than 0 and `CONTROL-DIFFERS-BASELINE: True`, `False` being `AC25 CHECK NOT DISCRIMINATING`: stop; `FIRST-PARTY-LINE-NOT-LOWER: True` and `FIRST-PARTY-BRANCH-NOT-LOWER: True`, `False` being `AC25: FIRST-PARTY RATE LOWER`: stop; `EXEMPT-UNCOVERED:` recorded; the `PACKAGE` rows recorded with no `MISSING`; every `EXEMPT-*-LINES` value and `NONEXEMPT-UNCOVERED` row listed; no absolute path); and the no-regression rule for the lines this item changed, held on the SortEmail family: the `SORTEMAIL-AGG` row reads `valid=286 covered=281 uncovered=5` and the five `SORTEMAIL-CLASS` rows carry the Phase 6 values (SortEmail.cs `valid=5 covered=5`, SortEmail.AttachmentSaving.cs `valid=142 covered=142`, SortEmail.TrySaveAttachment.cs `valid=93 covered=89`, SortEmail.UndoAndMoveLog.cs `valid=45 covered=45`, SortEmail.MailItemSort.cs `valid=1 covered=0`; Phase 7 changes no production line and SS4 adds one covered branch and no line, so any difference is `PHASE 7 FAMILY COVERAGE CHANGED`: stop and report the rows), with `PHASE6-LINE-NOT-LOWER:` and `PHASE6-BRANCH-NOT-LOWER:` recorded as observations, not gated (a `False` is reported in the `Output Summary:` as `PHASE 7 FIRST-PARTY RATE BELOW PHASE 6` with both percentage pairs and the `PACKAGE` rows; the P7-T11 floors and the P6-T8 not-lower clauses remain the gates). The commit stages FEATURE/ (`wip(959): P7-T12 coverage comparison`).

- [x] [P7-T13] Measure the AC25 members and the CR-1 arm: run `CMD-MEMBER-COVERAGE` over coverage\final-959.cobertura.xml, then `CMD-LINE-CONDITION` (`DOC` `coverage\final-959.cobertura.xml`, now the Phase 7 document), and append to FEATURE/evidence/qa-gates/coverage-comparison.md a section headed `## Per-member coverage and the CR-1 arm (P7-T13)` with `Timestamp:`, `Command:` (both payloads), `EXIT_CODE:` (scoped to the `CMD-LINE-CONDITION` payload, the last invocation) and every printed line of both. Acceptance, all seven required: every `CLASS-NODES` value at least 1; `MEMBERS-AMBIGUOUS: 0` and `MEMBERS-UNMEASURED: 0` (otherwise `MEMBER SPAN UNRESOLVED`: stop); every `MEMBER` row shows `percent=` at least 90 and `MEMBERS-BELOW-90: 0` (`NEW CODE COVERAGE BELOW 90`: stop) with `MEMBER SaveAttachmentCore span=130-156 valid=19 covered=19 percent=100`; each `E-CHANGED-LINE` row shows `matches=1` and `hits=` greater than 0 and `E-CHANGED-LINES-COVERED: 3`; the `TrySaveAttachmentCoreAsync` row's `uncovered=` value is a subset of the P7-T12 `EXEMPT-LINES:`; `A-CLASS-NODES: 1`, `A-LINE-143-COUNT: 1` and `A-LINE-143-BRANCH: True`; `A-LINE-143-CONDITION: 100% (2/2)` with `A-SAVEATTACHMENT-BRANCH-RATE: 1` (`50% (1/2)` and `0.75` at P7-T1, the false-before state; any other value is `CR-1 ARM STILL UNCOVERED`: stop and report). The commit stages FEATURE/ (`wip(959): P7-T13 member coverage and CR-1 arm`).

- [x] [P7-T14] Close the Phase 7 toolchain loop: record `git rev-parse HEAD` as `SUPERSEDES:`; then rewrite FEATURE/evidence/qa-gates/toolchain-final-pass.md (fixed name) in full with `Timestamp:`, `ITERATION: 2`, `SUPERSEDES:`, `WRITTEN-BY: P7-T14`, `LOOP-RESTARTS:`, `Command:` (the four CLAUDE.md commands in order, naming the P7-T11 collect invocation as the one `EXIT_CODE:` is scoped to), `EXIT_CODE:` (the P7-T11 `COLLECT_EXIT_CODE:`), `ExpectedExitCode:` equal to that observed value when it is non-zero under branch (b) of P7-T11 (omitted under branch (a)), one row per step of the final Phase 7 iteration (P7-T6 format, P7-T7 check, P7-T8 analyzer rebuild, P7-T9 nullable rebuild, P7-T10 scoped tests, P7-T11 tests with coverage) giving the canonical command, the artifact path, the step's exit code and, for the two rebuilds, `SKIP_CORECOMPILE_LINES:` and the four `_CSC_OUT_LINES:` values, and the sentence that this pass is the first analyzer rebuild, nullable rebuild and test run after the P6-T13 documentation-comment edit of TST1 (policy-audit G-4) and covers the P7-T2 and P7-T3 edits. Acceptance, all four required: every row of the final iteration reads exit code 0, except P7-T11 which reads 0 or its declared branch (b) expectation; both rebuild rows read `SKIP_CORECOMPILE_LINES: 0` with the four `_CSC_OUT_LINES:` at least 1; the P7-T6 row reads `WRITESET-CHANGED-COUNT: 0`; `EXIT_CODE:` equals its declared expectation. The commit stages FEATURE/ (`wip(959): P7-T14 toolchain record`).

- [x] [P7-T15] Re-verify the footprint at the Phase 0 to 7 anchor: run `git merge-base HEAD origin/main` (no fetch; `git -C WORKTREE` form) and record its output as `ANCHOR-RECHECK:`, then run `CMD-FOOTPRINT` exactly as P6-T12 ran it (`MERGE-BASE` 94287369908cc920b21b0e3256314f988ad7d2f5 and `INHERITED` the P0-T3 values; the payload pairs `git diff --name-only MERGE-BASE` with `git status --porcelain --untracked-files=all`), and write FEATURE/evidence/qa-gates/p7-t15-scope-boundary.<TS>.md with `Timestamp:`, `Command:`, `EXIT_CODE:` (the payload's process exit code), `ITERATION: 1`, `ANCHOR-RECHECK:` and every printed line. Acceptance, all eight required, the P6-T12 clauses unchanged: `ANCHOR-RECHECK:` equals `MERGE-BASE:` of P0-T3 (a difference is `ANCHOR MOVED`: stop and report); `OUTSIDE-WRITE-SET: 0` (the Phase 7 edits lie inside Write Set paths 12 and 13 and the artifacts inside FEATURE/); `WRITE-SET-MISSING: 0`; `DELETED-PATHS:` lists exactly `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs` and `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs`; `RAW-DOC-PATHS: 0`; `NUMSTAT-UCS:` reads `0 1`, `NUMSTAT-UCT:` `3 0`, `NUMSTAT-QFT:` `1 0` and `NUMSTAT-SPEC956:` `4 2`, each with its path; `NUMSTAT-S:` and `NUMSTAT-M:` each read `0 9` and the respective path; the subtracted Clause A and Clause B counts are recorded. The commit stages FEATURE/ (`wip(959): P7-T15 footprint`).

- [x] [P7-T16] Close Phase 7: run `CMD-SPEC-CHECK` (`STAGE` `final`), then `CMD-EVIDENCE-FIELDS` (`INHERITED` substituted exactly as in P6-T16), then `CMD-SWEEP` (after every other Phase 7 artifact write), and write FEATURE/evidence/qa-gates/p7-t16-phase7-closure.<TS>.md with `Timestamp:`, `Command:` (the three payloads in order), `EXIT_CODE:` (scoped to the `CMD-SWEEP` payload, its process exit code), `ITERATION: 1`, an `Output Summary:`, the CR disposition table of PD-16 (CR-1 remediated by P7-T2 with the P7-T1 and P7-T13 condition readings quoted; CR-3 remediated by P7-T3 or skipped with its reason; CR-2 no change, for filing with U-2; CR-4, CR-5 and CR-7 no change; CR-6 informational), a section headed `## Inputs for the pull-request body (Phase 7 addendum to pr-description-inputs)` carrying the SS4 test name, the new totals (`FILTER-ATTSAVE` 12, `FILTER-SORTEMAIL` 56, the P7-T11 `Total`), the CR-3 removal, the CR-2 and U-2 filing note and the sentence that no acceptance criterion changed, the list of the Phase 7 artifacts with their `<TS>` values, every CMD-SPEC-CHECK line, every count and row of the CMD-EVIDENCE-FIELDS run and every CMD-SWEEP count. Acceptance, all four required: `AC-CHECKED: 25`, `AC6-UNCHECKED: 1`, `AC27-UNCHECKED: 1` and `AC-ANY-UNCHECKED: 2` (Phase 7 edits no check box; under branch (b) of P7-T11 the executor additionally reports `PHASE 7 BRANCH (B): the AC21 and AC24 check-offs rest on the Phase 6 pass` for the orchestrator and changes no box); `EVIDENCE-MISSING-FIELDS: 0`, `NONCANONICAL-SUBFOLDER-FILES: 0`, `FIELD-CHECK-CONTROL: False`, `FIELD-CHECK-POSITIVE: True`, `SUBFOLDER-CHECK-FLAGS-OTHER: True` and `SUBFOLDER-CHECK-FLAGS-QA-GATES: False` (a missing field is repaired as in P6-T16 and the run repeated; the three stop strings of P6-T16 apply); `ACCOUNT-TOKEN-FILES: 0`, `PROFILE-LEAF-FILES: 0`, `MACHINE-TOKEN-FILES: 0`, `WORKTREE-ROOT-FILES: 0`, `USERS-PATH-FILES: 0` and `RAW-DOCUMENT-FILES: 0` (a non-zero count is repaired as in P6-T16 and the sweep re-run, this artifact being rewritten); `FILES:` at least 103 (the 89 of the P6-T46 sweep, the P6-T46 artifact, the three review artifacts and the ten Phase 7 artifacts written before this task's sweep). The commit stages FEATURE/ (`wip(959): P7-T16 phase 7 closure`); the Phase 7 outcome is reported as complete with AC6 and AC27 still deferred to the PR step, and Phase 8 is not started by the same delegation (PD-17).

### Phase 8 — PR-Time Reconciliation with origin/main (Runs Only After the Orchestrator's Re-Review of Phase 7 Passes)

Phase rule (revision 2.0; PD-17; the Phases 7 and 8 convention). Phase 8 is executed only after the orchestrator has re-reviewed the Phase 7 record and the delegation prompt that starts Phase 8 carries the line `PHASE 7 RE-REVIEW: PASS`; P8-T1 records that line and stops without it. The Phase 0 to 7 anchor 94287369908cc920b21b0e3256314f988ad7d2f5 is not used by any Phase 8 gate except as the lower bound of the two committed-range negative controls; every Phase 8 diff is anchored at `ORIGIN-MAIN-SHA:` (P8-T1) and paired with a porcelain count. P8-T4 to P8-T11 form one toolchain pass in CLAUDE.md order over the merged tree with the Phase 7 loop rule (restart at P8-T4 on `WRITESET-CHANGED-COUNT:` above 0; at most three iterations; any other failure is a stop). No Phase 8 task edits FEATURE/spec.md or any Write Set path other than QuickFiler.Test/QuickFiler.Test.csproj; the only source write is the union resolution of QuickFiler.Test/QuickFiler.Test.csproj under the conflict branch of P8-T2, applied with the Edit tool. Every task commits and pushes under the per-task rule (`wip(959): P8-T# <summary>`); every git command is a plain `git -C WORKTREE ...` invocation, one per call, with its exit code and output transcribed; a hook refusal is `HOOK BLOCKED`: stop and report the hook text verbatim.

- [x] [P8-T1] Record the authorization and the pre-merge facts: quote the delegation prompt's `PHASE 7 RE-REVIEW: PASS` line as `AUTHORIZATION:` (absent: stop with `PHASE 8 NOT AUTHORIZED`, writing no artifact and making no commit); then run, one invocation each: `git status --porcelain --untracked-files=all` (row count recorded as `CLEAN-BEFORE-MERGE:`), `git fetch origin main` (`FETCH-EXIT:`), `git rev-parse HEAD` (`HEAD-AT-P8-T1:`), `git rev-parse origin/main` (`ORIGIN-MAIN-SHA:`), `git merge-base HEAD origin/main` (`MERGE-BASE-BEFORE:`), `git merge-base --is-ancestor origin/main HEAD` (`MAIN-IS-ANCESTOR-EXIT:`), `git diff --numstat 94287369908cc920b21b0e3256314f988ad7d2f5 origin/main -- QuickFiler.Test/QuickFiler.Test.csproj` (`MAIN-QFT-NUMSTAT:`), `git diff 94287369908cc920b21b0e3256314f988ad7d2f5 origin/main -- QuickFiler.Test/QuickFiler.Test.csproj` (the hunk transcribed verbatim under `MAIN-QFT-HUNK:`, every `+` and `-` line kept) and `git diff --name-only 94287369908cc920b21b0e3256314f988ad7d2f5 origin/main` (the row count as `MAIN-CHANGED-PATHS:`, the rows that are Write Set paths listed under `MAIN-TOUCHED-WRITE-SET:`, and the rows under UtilitiesCS.Test/EmailIntelligence/ or QuickFiler.Test/Controllers/ listed under `MAIN-TOUCHED-TEST-DIRS:`). Write FEATURE/evidence/qa-gates/p8-t1-premerge-facts.<TS>.md with `Timestamp:`, `Command:` (the nine git commands), `EXIT_CODE:` (scoped to `git fetch origin main`, equal to `FETCH-EXIT:`), `ITERATION: 1`, an `Output Summary:` and every value. Acceptance, all seven required: `AUTHORIZATION:` quotes the line; `CLEAN-BEFORE-MERGE: 0` (otherwise `WORKTREE DIRTY BEFORE MERGE`: stop); `FETCH-EXIT: 0`; `MERGE-BASE-BEFORE: 94287369908cc920b21b0e3256314f988ad7d2f5` (otherwise `UNEXPECTED MERGE BASE`: stop and report, because PD-17's expectations describe a branch that has never merged main); `MAIN-IS-ANCESTOR-EXIT: 1` (0 means origin/main is already contained and there is nothing to merge: `NOTHING TO MERGE`: stop and report); `MAIN-QFT-HUNK:` carries, among its `+` lines, the three lines `    <Compile Include="Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs" />`, `    <Compile Include="TestSupport\SynchronousBackgroundWorker.cs" />` and `    <Compile Include="TestSupport\ArmingFakeTimeProvider.cs" />`, and no `-` line other than the `---` header (an additional `+` line is recorded; a removed line is `MAIN CSPROJ DELTA DIFFERS`: stop and report, because the union resolution of PD-17 assumes additions only on both sides); `MAIN-TOUCHED-WRITE-SET:` lists `QuickFiler.Test/QuickFiler.Test.csproj` (every other member is recorded as a conflict risk; the merge still proceeds and the P8-T2 conflict rule governs). The commit stages FEATURE/ (`wip(959): P8-T1 pre-merge facts`).

- [x] [P8-T2] Merge origin/main into the branch: run `git rev-parse HEAD` (`PRE-MERGE-HEAD:`), then `git merge --no-ff origin/main -m "merge(959): origin/main into bug/sort-email-latent-logic-defects-959 for the pull request"` (`MERGE-EXIT:`). When `MERGE-EXIT: 0`, record `CSPROJ-CONFLICT: NONE`. When it is non-zero, run `git diff --name-only --diff-filter=U` (`CONFLICTED-PATHS:`); if the rows are exactly `QuickFiler.Test/QuickFiler.Test.csproj`, Read the file, apply the union resolution of PD-17 with the Edit tool (the conflict markers `<<<<<<< HEAD`, `=======` and `>>>>>>> origin/main` and their lines replaced so that both sides' `Compile Include` lines remain in each side's original relative order, each of the four PD-17 lines present exactly once), run `git add QuickFiler.Test/QuickFiler.Test.csproj` and `git commit -m "merge(959): origin/main into bug/sort-email-latent-logic-defects-959 for the pull request"` (`MERGE-COMMIT-EXIT:`), and record `CSPROJ-CONFLICT: RESOLVED-AS-UNION` with the conflict hunk quoted; otherwise run `git merge --abort` and stop with `MERGE CONFLICT OUTSIDE EXPECTED UNION` listing `CONFLICTED-PATHS:`. After the merge commit exists run `git rev-parse HEAD` (`MERGE-HEAD-SHA:`), `git log -1 --format=%P HEAD` (`MERGE-PARENTS:`) and `git status --porcelain --untracked-files=all` (row count as `POST-MERGE-PORCELAIN:`); then, with the Grep tool over QuickFiler.Test/QuickFiler.Test.csproj, count the lines matching the backslash-free regular expressions `Controllers.EfcDataModelFilerCleanupTests\.cs" />` (`QFT-FILERCLEANUP-LINES:`), `Controllers.QfcItemController\.UiThreadDispatcherPinCountTests\.cs" />` (`QFT-PINCOUNT-LINES:`), `TestSupport.SynchronousBackgroundWorker\.cs" />` (`QFT-SYNCWORKER-LINES:`), `TestSupport.ArmingFakeTimeProvider\.cs" />` (`QFT-ARMINGFAKE-LINES:`) and `^(<<<<<<<|=======|>>>>>>>)` (`QFT-MARKER-LINES:`) (the `.` after the folder name stands for the path separator, so no pattern carries a backslash). Write FEATURE/evidence/qa-gates/p8-t2-merge-record.<TS>.md with `Timestamp:`, `Command:` (the git commands in order), `EXIT_CODE:` (scoped to the `git merge` invocation under `CSPROJ-CONFLICT: NONE`, or to the `git commit` invocation under `RESOLVED-AS-UNION`, the artifact naming which), `ITERATION: 1`, an `Output Summary:` and every value. Acceptance, all six required: `MERGE-HEAD-SHA:` differs from `PRE-MERGE-HEAD:`; `MERGE-PARENTS:` equals `<PRE-MERGE-HEAD> <ORIGIN-MAIN-SHA>` in that order (a single parent is `MERGE NOT A MERGE COMMIT`: stop and report); `POST-MERGE-PORCELAIN: 0`; `QFT-FILERCLEANUP-LINES: 1`, `QFT-PINCOUNT-LINES: 1`, `QFT-SYNCWORKER-LINES: 1` and `QFT-ARMINGFAKE-LINES: 1`; `QFT-MARKER-LINES: 0`; `CSPROJ-CONFLICT:` recorded as `NONE` or `RESOLVED-AS-UNION`. The commit stages FEATURE/ (`wip(959): P8-T2 merge record`) and the push carries the merge commit to origin.

- [x] [P8-T3] Prove the fan-in is additions-only: run `git merge-base --is-ancestor <ORIGIN-MAIN-SHA> HEAD` (`ANCESTRY-MAIN-EXIT:`), `git merge-base --is-ancestor 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD` (`ANCESTRY-BASE-EXIT:`), `git merge-base --is-ancestor HEAD <ORIGIN-MAIN-SHA>` (`ANCESTRY-CONTROL-EXIT:`, the reversed check) and `git merge-base HEAD origin/main` (`MERGE-BASE-AFTER:`), each one invocation with the P8-T1 value substituted; then `CMD-FANIN` (`ORIGIN-MAIN-SHA` substituted). Write FEATURE/evidence/qa-gates/p8-t3-fanin-gate.<TS>.md with `Timestamp:`, `Command:`, `EXIT_CODE:` (scoped to the `CMD-FANIN` payload, its process exit code), `ITERATION: 1`, an `Output Summary:`, the four git results and every printed line of the payload. Acceptance, all nine required: `ANCESTRY-MAIN-EXIT: 0` and `ANCESTRY-BASE-EXIT: 0`; `ANCESTRY-CONTROL-EXIT: 1` (0 is `ANCESTRY CONTROL INERT`: stop, because HEAD contained in main would make every ancestry check vacuous); `MERGE-BASE-AFTER:` equals `ORIGIN-MAIN-SHA:` (the new merge base with main; otherwise `POST-MERGE BASE DIFFERS`: stop); `FANIN-OUTSIDE: 0` (a `FANIN-OUTSIDE-PATH` row is `FAN-IN OUTSIDE OWN PATHS`: stop and report every row; the executor does not revert or edit); `FANIN-WRITE-SET-MISSING: 0` and `FANIN-DELETED: 2` (the two planned deletions remain the only `D` rows); `NUMSTAT-QFT-VS-MAIN:` reads `1 0` and the path (the shared file shows additions only), `NUMSTAT-UCT-VS-MAIN:` reads `3 0` and `NUMSTAT-UCS-VS-MAIN:` reads `0 1` (a different value for a file that P8-T1's `MAIN-TOUCHED-WRITE-SET:` names is recorded with that explanation; otherwise `FAN-IN NUMSTAT DIFFERS`: stop); `MAIN-ONLY-PATHS:` greater than 0, `MAIN-ONLY-HAS-QFT-CSPROJ: True` and `CONTROL-OUTSIDE-IF-UNFILTERED:` greater than 0 (the negative controls; a 0 or `False` is `FAN-IN CONTROL INERT`: stop); `SHARED-PATHS:` at least 1 with every `SHARED-NUMSTAT` row recorded (two rows predicted: QuickFiler.Test/QuickFiler.Test.csproj and .claude/agent-memory/orchestrator/MEMORY.md), `SHARED-WITH-LOSS: 0` and `LOSS-CHECK-CONTROL: True` (a non-zero `SHARED-WITH-LOSS:` is `FAN-IN DROPPED MAIN LINES`: stop and report the `SHARED-NUMSTAT` rows; `LOSS-CHECK-CONTROL: False` is `FAN-IN CONTROL INERT`: stop); `MAIN-TOUCHED-WRITE-SET-CODE:` recorded (expected empty; a non-empty value is carried into the P8-T8 and P8-T10 conditional clauses) and `PORCELAIN-COUNT: 0` (taken before this artifact is written). The commit stages FEATURE/ (`wip(959): P8-T3 fan-in gate`).

- [x] [P8-T4] Run the repository-wide formatter on the merged tree with `CMD-FORMAT-REPO` (`TASKID` `p8-t4`) and write FEATURE/evidence/qa-gates/p8-t4-csharpier-format.<TS>.md with the P7-T6 field set and `ITERATION:`. Acceptance, all three required: `FORMAT_EXIT_CODE: 0`; `WRITESET-CHANGED-COUNT: 0` (a non-zero value triggers the phase rule's restart at this task); `PORCELAIN-SAME: True` (`False` is `FORMAT TOUCHED OUT-OF-SET FILE`: stop and report the porcelain rows, which on the merged tree name a file main brought that is not formatter-clean; do not revert). The commit stages FEATURE/ (`wip(959): P8-T4 repository format`).

- [x] [P8-T5] Verify formatting read-only with `CMD-CHECK-REPO` (`TASKID` `p8-t5`) and write FEATURE/evidence/qa-gates/p8-t5-csharpier-check.<TS>.md with the P7-T7 field set and `ITERATION:`. Acceptance, both required: `CHECK_EXIT_CODE: 0` recorded as `EXIT_CODE: 0`; `CHECKED-LINE:` matches `Checked <N> files` with a positive N. The commit stages FEATURE/ (`wip(959): P8-T5 repository check`).

- [x] [P8-T6] Run the analyzer gate on the merged tree with `CMD-REBUILD` (analyzer `GATEARGS`, `TASKID` `p8-t6`) and write FEATURE/evidence/qa-gates/p8-t6-msbuild-analyzers.<TS>.md with the P0-T7 field set and `ITERATION:`. Acceptance, all five required: `EXIT_CODE: 0`; `SKIP_CORECOMPILE_LINES: 0`; the four `_CSC_OUT_LINES:` values each at least 1; `ERRORS: 0`; `WRITESET_DIAGNOSTIC_LINES:` at most `ANALYZE-BASELINE-WRITESET-DIAGNOSTICS:` of P0-T7. The commit stages FEATURE/ (`wip(959): P8-T6 analyzer rebuild`).

- [x] [P8-T7] Run the type-check gate on the merged tree with `CMD-REBUILD` (nullable `GATEARGS`, `TASKID` `p8-t7`; no Nullable property override) and write FEATURE/evidence/qa-gates/p8-t7-msbuild-nullable.<TS>.md with the P0-T7 field set and `ITERATION:`. Acceptance, all five required: `EXIT_CODE: 0`; `SKIP_CORECOMPILE_LINES: 0`; the four `_CSC_OUT_LINES:` values each at least 1; `ERRORS: 0`; `WRITESET_DIAGNOSTIC_LINES:` at most `NULLABLE-BASELINE-WRITESET-DIAGNOSTICS:` of P0-T8. The commit stages FEATURE/ (`wip(959): P8-T7 nullable rebuild`).

- [x] [P8-T8] Run the scoped suites on the merged tree: `CMD-VSTEST` (`ASSEMBLY-UCT`, `FILTER-SORTEMAIL`, `TASKID` `p8-t8`), `CMD-VSTEST` (`ASSEMBLY-QFT`, `FILTER-EFC-CLEANUP`, `TASKID` `p8-t8-cleanup`) and `CMD-VSTEST` (`ASSEMBLY-QFT`, `FILTER-EFC-ARCHIVE`, `TASKID` `p8-t8-archive`); record `git rev-parse HEAD` as `SUPERSEDES:`; then rewrite FEATURE/evidence/regression-testing/pass-after-regression-tests.md in full as P7-T10 did, with `ITERATION: 3`, `WRITTEN-BY: P8-T8` and the section `## QuickFiler.Test (P8-T8)`. Acceptance, all five required: `EXIT_CODE: 0` with `COUNTERS total=56 executed=56 passed=56 failed=0` and the fifty-six rows exactly the P7-T10 union, each `= Passed`, when P8-T3 recorded an empty `MAIN-TOUCHED-WRITE-SET-CODE:` and P8-T1's `MAIN-TOUCHED-TEST-DIRS:` names no `SortEmail_` file (otherwise the observed `COUNTERS` and rows are recorded, every row must still read `= Passed` with `failed=0`, and the task stops with `MAIN CHANGED SORTEMAIL FAMILY` for the orchestrator); `CLEANUP-VSTEST_EXIT_CODE: 0` with `COUNTERS total=3 executed=3 passed=3 failed=0` and the rows exactly `NAMES-TEF`; `ARCHIVE-VSTEST_EXIT_CODE: 0` with `failed=0`, `passed` equal to `total`, and `total=11` with the rows exactly `NAMES-EFC-ARCHIVE` unless `MAIN-TOUCHED-TEST-DIRS:` names EfcDataModelArchiveRootTests.cs (then the observed total and rows are recorded); every `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:` of P0-T4; every `SANDBOX-` value is `False` and every `SEQUENCE_FILES: 0`. The commit stages FEATURE/ (`wip(959): P8-T8 scoped suites on the merged tree`).

- [ ] [P8-T9] Run the repository-wide test and coverage pass on the merged tree: `CMD-COVERAGE-DIRECT` (`STAGE` `final`, the same `EXCLUSION` as P0-T11) as a background invocation polled until its final `TRX_PRESENT:` line, then `CMD-COVERAGE-POST` (`STAGE` `final`); record `git rev-parse HEAD` as `SUPERSEDES:`; then rewrite FEATURE/evidence/qa-gates/coverage-post-change.md in full as P7-T11 did, with `ITERATION: 3`, `WRITTEN-BY: P8-T9`, `MERGE-HEAD-SHA:` (from P8-T2), the transcribed Phase 7 figures `PHASE7-FIRST-PARTY-LINE-PERCENT:`, `PHASE7-FIRST-PARTY-BRANCH-PERCENT:` and `PHASE7-TOTAL:` (from the superseded record) and `NEWLY-FAILING:` against coverage-baseline.md's `FAILED-SET:`. Branches as in P6-T7. Acceptance, all seven required: the projection holds `UtilitiesCS` and `QuickFiler` packages with `LINE` and `BRANCH` counters; `FIRST-PARTY-LINE-PERCENT:` at least 80 and `FIRST-PARTY-BRANCH-PERCENT:` at least 75 with `LINE-FLOOR: MET` and `BRANCH-FLOOR: MET`; `FAILED-SET:` contains none of the names of `NAMES-TST1-FINAL`, `NAMES-T12`, `NAMES-TSC-FINAL`, `NAMES-TAS-FINAL2`, `NAMES-TUL`, `NAMES-TEF` or `NAMES-EFC-ARCHIVE`; `NEWLY-FAILING: NONE`, or only `TryAddValuesAsync_UpdatesExistingValue` under branch (b) (any other name is a test main brought that fails on this workstation: `MAIN TESTS FAILING LOCALLY`: stop and report the names and their `MESSAGE` lines, the orchestrator deciding); the summary's `Total` is recorded (not predicted; it equals the P7-T11 total plus the tests main brought) with `failed 0` under branch (a); `EXIT_CODE:` equals its declared expectation; every `SANDBOX-` value is `False` and the artifact contains no absolute path. The commit stages FEATURE/ (`wip(959): P8-T9 coverage run on the merged tree`).

- [ ] [P8-T10] Compare coverage on the merged tree: run `CMD-COVERAGE-TEXTS` (`STAGE` `final`, `BASELINE-HASH` as in P7-T12), `CMD-MEMBER-COVERAGE` over coverage\final-959.cobertura.xml and `CMD-LINE-CONDITION` (`DOC` `coverage\final-959.cobertura.xml`); record `git rev-parse HEAD` as `SUPERSEDES:`; then rewrite FEATURE/evidence/qa-gates/coverage-comparison.md in full with `Timestamp:`, `ITERATION: 3`, `SUPERSEDES:`, `WRITTEN-BY: P8-T10`, `Command:` (the three payloads), `EXIT_CODE:` (scoped to the `CMD-LINE-CONDITION` payload, the last invocation), an `Output Summary:` holding every printed line of the three payloads, the rows `PHASE7-LINE-NOT-LOWER:` and `PHASE7-BRANCH-NOT-LOWER:` (each `True` when the P8-T9 first-party percentage is at least the transcribed Phase 7 value at two decimals) and the `Reading:` paragraph. Acceptance, all seven required: `SORTEMAIL-DIR-CLASSES:` at least 1 and `TRYSAVE-CLASS-FOUND: True`; when P8-T3 recorded an empty `MAIN-TOUCHED-WRITE-SET-CODE:`, the four `EXEMPT-*-COUNT` values 1 with `GUARD-CONDITION-LINES:` naming one line, `NONEXEMPT-SET-MATCHES-BASELINE: True` and `CONTROL-DIFFERS-BASELINE: True` with `CONTROL-LINE:` greater than 0 (a `False` is the corresponding P6-T8 stop); when it was non-empty, those values are recorded and the task stops with `MAIN CHANGED SORTEMAIL FAMILY`; `FIRST-PARTY-LINE-NOT-LOWER:`, `FIRST-PARTY-BRANCH-NOT-LOWER:`, `PHASE7-LINE-NOT-LOWER:` and `PHASE7-BRANCH-NOT-LOWER:` recorded (observations on the merged tree, which carries main's own code; a `False` is reported in the `Output Summary:` as `POST-MERGE RATE LOWER` with the `PACKAGE` rows, and the floors of P8-T9 remain the gate); every `MEMBER` row shows `percent=` at least 90 with `MEMBERS-BELOW-90: 0`, `MEMBERS-AMBIGUOUS: 0`, `MEMBERS-UNMEASURED: 0` and `E-CHANGED-LINES-COVERED: 3` (the P6-T9 stops apply); `A-CLASS-NODES: 1`, `A-LINE-143-COUNT: 1`, `A-LINE-143-BRANCH: True` and `A-LINE-143-CONDITION: 100% (2/2)` with `A-SAVEATTACHMENT-BRANCH-RATE: 1` (`CR-1 ARM STILL UNCOVERED`: stop); every `EXEMPT-*-LINES` value and `NONEXEMPT-UNCOVERED` row listed; the artifact contains no absolute path. The commit stages FEATURE/ (`wip(959): P8-T10 coverage comparison on the merged tree`).

- [ ] [P8-T11] Close the Phase 8 toolchain loop: record `git rev-parse HEAD` as `SUPERSEDES:`; then rewrite FEATURE/evidence/qa-gates/toolchain-final-pass.md in full as P7-T14 did, with `ITERATION: 3`, `WRITTEN-BY: P8-T11`, `MERGE-HEAD-SHA:`, `ORIGIN-MAIN-SHA:`, one row per step (P8-T4 format, P8-T5 check, P8-T6 analyzer rebuild, P8-T7 nullable rebuild, P8-T8 scoped tests, P8-T9 tests with coverage), `EXIT_CODE:` (the P8-T9 `COLLECT_EXIT_CODE:`) with `ExpectedExitCode:` under branch (b) only, and the sentence that this pass ran on the merged tree whose merge base with origin/main is `ORIGIN-MAIN-SHA:`. Acceptance, all four required: every row reads exit code 0, except P8-T9 which reads 0 or its declared branch (b) expectation; both rebuild rows read `SKIP_CORECOMPILE_LINES: 0` with the four `_CSC_OUT_LINES:` at least 1; the P8-T4 row reads `WRITESET-CHANGED-COUNT: 0`; `EXIT_CODE:` equals its declared expectation. The commit stages FEATURE/ (`wip(959): P8-T11 toolchain record on the merged tree`).

- [ ] [P8-T12] Push and verify the remote tip: run `git push origin bug/sort-email-latent-logic-defects-959` (`PUSH-EXIT:`), `git rev-parse HEAD` (`HEAD-BEFORE-OWN-COMMIT:`), `git rev-parse origin/bug/sort-email-latent-logic-defects-959` (`REMOTE-TIP:`) and `git merge-base --is-ancestor <MERGE-HEAD-SHA> HEAD` (`MERGE-IN-HISTORY-EXIT:`), one invocation each, and write FEATURE/evidence/qa-gates/p8-t12-push-record.<TS>.md with `Timestamp:`, `Command:`, `EXIT_CODE:` (scoped to the push), `ITERATION: 1`, an `Output Summary:` and the four values. Acceptance, all three required: `PUSH-EXIT: 0` (a non-zero value, or a rejected push, is `PUSH REJECTED`: stop and report the output; the executor never force-pushes); `REMOTE-TIP:` equals `HEAD-BEFORE-OWN-COMMIT:` (the state through P8-T11 is on origin; this task's own commit and push follow under the per-task rule and are verified by P8-T13); `MERGE-IN-HISTORY-EXIT: 0`. The commit stages FEATURE/ (`wip(959): P8-T12 push record`).

- [ ] [P8-T13] Close Phase 8: run `CMD-SPEC-CHECK` (`STAGE` `final`), then `CMD-EVIDENCE-FIELDS` (`INHERITED` as in P6-T16), then `CMD-SWEEP` (after every other Phase 8 artifact write), then `git rev-parse HEAD` (`HEAD-BEFORE-OWN-COMMIT:`) and `git rev-parse origin/bug/sort-email-latent-logic-defects-959` (`REMOTE-TIP:`), and write FEATURE/evidence/qa-gates/p8-t13-phase8-closure.<TS>.md with `Timestamp:`, `Command:` (the three payloads and the two git commands in order), `EXIT_CODE:` (scoped to the `CMD-SWEEP` payload, its process exit code), `ITERATION: 1`, an `Output Summary:`, `MERGE-HEAD-SHA:`, `ORIGIN-MAIN-SHA:` (the branch's merge base with main from this point, for the pr-author step and the base-branch resolution), the statement that the Phase 0 to 7 anchor 94287369908cc920b21b0e3256314f988ad7d2f5 governed every gate through P7-T15 and that none of those gates was re-run after the merge (PD-17), the list of the Phase 8 artifacts with their `<TS>` values, a section headed `## Inputs for the pull-request body (Phase 8 addendum)` carrying `MERGE-HEAD-SHA:`, the P8-T9 totals and first-party percentages and `CSPROJ-CONFLICT:`, every CMD-SPEC-CHECK line, every count and row of the CMD-EVIDENCE-FIELDS run and every CMD-SWEEP count. Acceptance, all five required: `AC-CHECKED: 25`, `AC6-UNCHECKED: 1`, `AC27-UNCHECKED: 1` and `AC-ANY-UNCHECKED: 2` (Phase 8 edits no check box); `EVIDENCE-MISSING-FIELDS: 0`, `NONCANONICAL-SUBFOLDER-FILES: 0`, `FIELD-CHECK-CONTROL: False`, `FIELD-CHECK-POSITIVE: True`, `SUBFOLDER-CHECK-FLAGS-OTHER: True` and `SUBFOLDER-CHECK-FLAGS-QA-GATES: False` (repairs and stops as in P6-T16); `ACCOUNT-TOKEN-FILES: 0`, `PROFILE-LEAF-FILES: 0`, `MACHINE-TOKEN-FILES: 0`, `WORKTREE-ROOT-FILES: 0`, `USERS-PATH-FILES: 0` and `RAW-DOCUMENT-FILES: 0` (a non-zero count is repaired as in P6-T16 and the sweep re-run, this artifact being rewritten); `FILES:` at least 112 (the P7-T16 floor of 103 plus the P7-T16 artifact and the eight Phase 8 artifacts p8-t1 to p8-t7 and p8-t12, all written before this task's sweep); `REMOTE-TIP:` equals `HEAD-BEFORE-OWN-COMMIT:`. The commit stages FEATURE/ (`wip(959): P8-T13 phase 8 closure`) and its push is the last git command of this plan; the plan outcome is reported as complete with AC6 and AC27 deferred to the PR step, the merge commit `MERGE-HEAD-SHA:` on origin and the new merge base `ORIGIN-MAIN-SHA:` named for the pr-author step, never as PASS over an unchecked AC.

## Revision Log

Revision 1.0 (2026-10-02): initial authoring in two passes against FEATURE/spec.md revision 0.2; the first pass wrote the header through Listing L-TAS-FINAL and stopped; the completion pass wrote Listings L-TEF, L-A-FINAL, L-T-FINAL, L-U-FINAL and L-TST1-ROWS, the Edit Specifications, the Test Inventory and Census Expectations, the Command Reference, Phases 0 to 6 and this record. Research corrections carried by this plan: PD-6 (two added `GetAttachmentsInfo` rows per test instead of one, so that both filter statements of each method are covered once their exclusions are removed) and PD-10 (the dossier file name follows the mandatory `fail-before-exception.*.md` pattern instead of the spec's `fail-before-exceptions.md` spelling); PD-4 supersedes R2 section 13's predicted scoped total of 52 with 55 by counting. No related-defect widening beyond the eighteen-path write set of R2 section 10 was needed; no write-set change.

Revision 1.1 (2026-10-02, preflight round 1 delta, fifteen defects and three orchestrator decisions, applied in place): (1) P0-T3 `MAIN-IS-ANCESTOR-EXIT:` demoted to an observation (nine acceptance clauses) and the `ANCHOR MOVED` convention added; (2) `PATHS-CITED` defined in the Command Reference and used by the `CITED-TREE-EXIT:` diff; (3) P0-T12 and Verified Repository Facts item 4 restated as S 277, M 388, L 240 by `Get-Content` (278, 389, 241 Read lines); (4) TOKENS-A `_attachmentsAltName=YesNoToAllResponse.Empty;` 2 at BASE and 3 at P1 (the line-27 field initializer), with P1-T8, P1-T9 and P6-T24 following; (5) TOKENS-TST2 `Times.Exactly(2)` 4 at BASE (TST2 lines 77, 103, 186, 316) and 5 at FINAL, with P3-T1 and P6-T22 following; (6) TOKENS-TAS `Cleanup_Files_ResetsEveryPromptAnswerField` 5 at P1 (declaration plus four `DisplayName` strings) with P1-T2 following, and every other method-name token re-audited (TST2 T12 name, TAS structural name, the three TEF names, the three TST1 names: each occurs once in its listing or file, no `DisplayName` carries them); (7) `SHOWDIALOG-CALLS-PARTIALS:` base 9 (A six including the comment at 258; L three including `InputBox.ShowDialog(` at 199); (8) CMD-GREP-FACTS `$skip` made root-relative with a `CS-FILES:` positive-control row (no other payload used the absolute-path `.claude` filter; CMD-COVERAGE-DIRECT was already root-relative); (9) P5-T3 porcelain names the ` D` row of the legacy partial (no other post-deletion porcelain acceptance omitted a deleted path); (10) P6-T17 nine artifacts, `ARTIFACTS-PRESENT: 9/9`; (11) every `FEATURE/evidence/other/` path moved to `FEATURE/evidence/qa-gates/` (AC26, D17), the conventions and the Phases intro trimmed to the three permitted subfolders, `NONCANONICAL-SUBFOLDER-FILES:` added to CMD-EVIDENCE-FIELDS and gated at 0 in P6-T16 and P6-T44; (12) P6-T39 and P6-T42 made conditional on branch (a) of P6-T7 with `AC21 NOT MET` and `AC24 NOT MET` reports under branch (b), and P6-T46's expected end state given for both branches; (13) AC6 deferred to the PR step like AC27: P6-T24 is now a deferral record, `AC6-UNCHECKED:` added to CMD-SPEC-CHECK, P6-T46 expects `AC-CHECKED: 25` and `AC-ANY-UNCHECKED: 2` under branch (a), PD-11 and the AC6 mapping updated; (14) PD-2 and PD-3 name the CSharpier runs as the other `pwsh` writes to tracked paths; (15) compile-red spans P4-T1 to P4-T7 and P5-T4 to P5-T6 declared in P4-T1, P5-T4 and the Execution Conventions with the `COMPILE-RED SPAN OPEN` report. Orchestrator decisions: both `GetAttachmentsInfo` method names kept and every L-TST1-ROWS tag restated as its explicit flag combination (Listing L-TST1-ROWS and Edits E-TST1-ROWS-SYNC and E-TST1-ROWS-ASYNC); SortEmail_Tests.cs added to the `AC23-CLOSEST:` rows of P6-T10 and P6-T41 with the derived prediction (457 − 25 + 26 + 26 = 484 before the P4-T7 format); the self-review record states its `CITATION:` line count. Task count unchanged at 111; no write-set change.

Revision 1.2 (2026-10-02, preflight round 2 delta, seven defects D1 to D7 applied in place with sibling fixes): (D1) `.gitignore` removed from `PATHS-CITED` with the exclusion reason appended, and every `.gitignore` line-number citation (Write Set "Not edited" paragraph, Verified Repository Facts 14) converted to the content form; the `PATHS-CITED` definition now reads "by line, together with every path it cites by content other than .gitignore"; (D2) P6-T12 runs `git merge-base HEAD origin/main` without a fetch before `CMD-FOOTPRINT` and gates `ANCHOR-RECHECK:` equal to `MERGE-BASE:` (eight acceptance clauses); the anchor convention and P0-T3 state that this is an observation compared with the recorded value, never a re-derivation; (D3) the compile-red span P2-T1 to P2-T5 declared in the Execution Conventions and in P2-T2 (verified: Listing L-TUL calls the four-parameter overload, U at the cited tree carries only the two-parameter overload at lines 139 to 170, P2-T3 lands the internal overload through Edit E-U-SEAM-HEAD, P2-T4 formats only, P2-T5 is the first `CMD-BUILD-TEST` after P2-T2); P4-T1 and P5-T4 name all three spans; (D4) PD-2 enumerates the `CMD-RESTORE` fallback among the `pwsh` writes to tracked paths, PD-3 and the `CMD-DELETE` heading no longer claim an "only" write; (D5) P6-T46 runs `CMD-EVIDENCE-FIELDS` after `CMD-SPEC-CHECK` and before `CMD-SWEEP` with both counts gated at 0 and recorded (five acceptance clauses); the `INHERITED` value of `CMD-EVIDENCE-FIELDS` in P6-T16 and P6-T46 is the `INHERITED-CLAUSE-A:` list minus the P0-T1 and P0-T2 artifacts; sibling fix: P0-T2's `PRE-EXISTING-EVIDENCE:` now excludes the P0-T1 artifact, because that artifact always exists when P0-T2 runs and the "unless `PRE-EXISTING-EVIDENCE:` names that path" clause would otherwise have exempted it on every run; PD-9 records the subtraction rule; the AC26 mapping names P6-T46; the check demands the three fields AC26 names (`Timestamp:`, `Command:`, `EXIT_CODE:`), which P0-T1 and P0-T2 both carry; (D6) the E-AC-CHECKOFF heading reads P6-T19 to P6-T23 and P6-T25 to P6-T44; the Write Set sentence states the branch (a) and branch (b) end states; every other count statement (PD-11, CMD-SPEC-CHECK expectations, P6-T46, the AC27 mapping) already carried both branches; (D7) the `EXCLUSION` definition states the `EXCLUSION: NONE` recording and the empty substitution, with P0-T11, the `CMD-COVERAGE-DIRECT` heading and the Test-runs convention aligned. Task count unchanged at 111; no write-set change.

Revision 1.3 (2026-10-02, preflight round 3 delta, six defects D-1 to D-6 applied in place with sibling fixes): (D-1) AC26 is checked off on evidence that precedes the Edit: P6-T44 issues its own `CMD-EVIDENCE-FIELDS` run over every artifact written through P6-T43 and gates `EVIDENCE-MISSING-FIELDS: 0`, `NONCANONICAL-SUBFOLDER-FILES: 0` and `FIELD-CHECK-CONTROL: False` on it, recording the printed lines under `## AC26 check-off evidence fields (P6-T44)` in the P6-T46 inventory artifact (the forward-record mechanism P6-T25 already uses for `AC7-READ: HOLDS`); the P6-T46 run is now the third, confirming every artifact including the P6-T45 artifact; P6-T44 is named in the `CMD-EVIDENCE-FIELDS` heading, PD-9, the Phase 6 loop rule and the AC26 mapping, whose TESTS clause distinguishes the sweep count (`RAW-DOCUMENT-FILES`, P6-T16 and P6-T46 only, because P6-T44 does not run `CMD-SWEEP`) from the three field-check counts (P6-T16, the P6-T44 run, P6-T46); (D-2) the field test is line-anchored: `[regex]::IsMatch($c, "(?m)^[^\w\r\n]*" + [regex]::Escape($_))` replaces `$c.Contains($_)`, so a label counts only at the start of a line after optional list or emphasis punctuation and `FORMAT_EXIT_CODE:` no longer satisfies `EXIT_CODE:`; the payload prints `FIELD-CHECK-CONTROL:` by testing the literal `FORMAT_EXIT_CODE: 0`, gated `False` in P6-T16, P6-T44 and P6-T46 with `FIELD CHECK NOT DISCRIMINATING` on `True` (clause counts unchanged); the Artifact fields convention now states the line-leading form, that a prefixed label or an enumeration of rows never replaces the three rows, and a default scope for `EXIT_CODE:` when a task names none; the audit of every artifact-writing task added an explicit `EXIT_CODE:` scope (and `Timestamp:` and `Command:` where the enumeration lacked them) to P0-T3 (`git fetch`), P0-T4 (`dotnet tool restore`), P0-T12 and P6-T10 (`CMD-LINES`, the last payload before Grep and Read steps), P1-T4 and P2-T4 (`csharpier check`), P1-T5, P1-T10, P2-T5, P2-T9, P3-T2, P3-T5, P4-T7, P4-T10, P5-T6 and P5-T8 (`CMD-BUILD-TEST`), P5-T3 (`CMD-BUILD-PROD`), P4-T13 (the backup payload, which prints no exit label), P4-T15 (`CMD-VSTEST`), P4-T5 and P5-T10 (`CMD-DELETE`), P5-T11 (`CMD-SPEC-CHECK`), P6-T1 (`FORMAT_EXIT_CODE:`), P6-T16 (`CMD-EVIDENCE-FIELDS`) and P6-T46 (`CMD-SWEEP`), and added `Timestamp:` to the enumerations of P0-T10 and P0-T11; the fixed-name artifacts negative-controls.md (P6-T17), coverage-comparison.md (P6-T8), pr-description-inputs (P6-T18), the two deferral records (P6-T24, P6-T45), the six fail-before artifacts (P1-T6 field set), compile-red-attachment-saving-seams.md (P4-T3), pass-after-regression-tests.md (P6-T5), toolchain-final-pass.md (P6-T11), coverage-post-change.md (P0-T11 field set) and the dossier (P5-T12) already named all three; (D-3) P0-T3's `CITED-TREE-EXIT:` clause names `PATHS-CITED` and the .gitignore exclusion; (D-4) `CMD-EVIDENCE-FIELDS` prints one `NONCANONICAL:` row per file outside the three subfolders, and P6-T16, P6-T44 and P6-T46 treat a non-zero `NONCANONICAL-SUBFOLDER-FILES:` as `NONCANONICAL EVIDENCE FILE` (stop and report the rows; the executor does not move a file it did not write); the file set was verified to be rooted at FEATURE/evidence/ (`Get-ChildItem -LiteralPath "docs\...\evidence" -Recurse -File`), so the plan, spec.md, issue.md and research/ are never counted, the count reaches 0 on a clean run and rises for a file under any other subfolder or directly under FEATURE/evidence/; the heading and the Evidence-paths convention state this; (D-5) the `CMD-SWEEP` heading names P6-T16 and P6-T46; (D-6) PD-2 states that `pwsh` payloads write only under git-ignored paths (coverage/, bin/, obj/, packages/, .dotnet-sdk/) and names the .gitignore entries (`coverage/*`, `[Bb]in/`, `[Oo]bj/`, `**/[Pp]ackages/*`, `.dotnet*/`, verified by content in this pass); the Write Set "Not edited" paragraph and Verified Repository Facts 14 name the same entries; a Grep over the plan found no other coverage/-only write claim (PD-3 and the Execution Conventions describe tracked-path writes and the results directory, not the set of ignored write locations). Task count unchanged at 111; no write-set change.

Revision 1.4 (2026-10-03, preflight round 4 delta, six defects D-1 to D-6 applied in place with sibling fixes): (D-1) P0-T4's `EXIT_CODE:` is the printed `TOOL-RESTORE-EXIT:`; Part 2 states that each native call of the payload (the `Install-RepoDotNetSdk.ps1` call, `dotnet --version`, `dotnet tool restore`, `dotnet tool list --local`) is bracketed by `$global:LASTEXITCODE = 0` and a `Write-Output` of its label (`SDK-INSTALL-EXIT:`, `DOTNET-VERSION-EXIT:`, `TOOL-RESTORE-EXIT:`, `TOOL-LIST-EXIT:`), that the install call is a child `pwsh -File` process whose exit code is 1 only when the script's `throw` (its line 103) fires and is an observation because `SDK-MARKER:` is the gate, that the two vswhere probes carry no exit label, and that `dotnet-coverage --version` prints `DOTNET-COVERAGE-EXIT:` the same way including its separate re-run; acceptance clause 5 reads `DOTNET-VERSION-EXIT: 0` and `TOOL-RESTORE-EXIT: 0`, clause 8 reads `DOTNET-COVERAGE-EXIT: 0` with its version recorded (sibling fix: the clause states that the version line is the discriminating half, because a command that is not found raises a non-terminating error and leaves `$LASTEXITCODE` at 0), the count stays eight; the Artifact fields convention lists `TOOL-RESTORE-EXIT:` among the prefixed labels; (D-2) P6-T11 writes `ExpectedExitCode:` equal to the observed `COLLECT_EXIT_CODE:` when it is non-zero under branch (b) of P6-T7 (omitted under branch (a)) and gates `EXIT_CODE:` equal to its declared expectation as a fourth clause; (D-3) the Artifact fields convention states that the `Timestamp:`, `Command:`, `EXIT_CODE:` and `ExpectedExitCode:` rows are written at the top of every artifact, before `Output Summary:` and any copied payload line, and that every prefixed label and appended section follows them; a Grep over the plan for every `append` instruction (P1-T11, P1-T12, P2-T10, P3-T6, P4-T11, P5-T9, P6-T6, P6-T9) found only appended sections and no task that orders the schema rows after copied output, and P0-T10's "form the artifact's record; then ... recorded under a heading" already agrees; (D-4) the Phase 6 loop rule states the check-off tasks' write scope as FEATURE/spec.md in the five characters of one check box each and no other file, except P6-T44's repair branch, which adds a missing schema field to the artifact a `MISSING-FIELDS:` row names before its Edit; the E-AC-CHECKOFF heading (file FEATURE/spec.md, no other character of the line changes), P6-T44's own repair text and the AC26 mapping's "never reverts the box" clause agree; (D-5) P0-T3's `PATHS-CITED` parenthetical reads "every file outside FEATURE/ and outside .claude/ that this plan cites by line or by content, except .gitignore; the three exclusions are those the Command Reference states", matching the `PATHS-CITED` definition's three exclusion sentences; (D-6) `CMD-EVIDENCE-FIELDS` assigns `$anchor = "(?m)^[^\w\r\n]*"` once, before first use, and the per-file predicate, `FIELD-CHECK-CONTROL:` (`FORMAT_EXIT_CODE: 0`, must print `False`) and the new `FIELD-CHECK-POSITIVE:` (`- **EXIT_CODE:** 0`, must print `True`) all build their pattern as `$anchor + [regex]::Escape(...)`, so the controls exercise the predicate actually run; P6-T16 (clause count stays five), P6-T44 (precondition value; the acceptance remains the single Grep clause), P6-T46 (clause count stays five; the positive control joins the first clause) and the AC26 mapping gate `FIELD-CHECK-POSITIVE: True`, with `FIELD-CHECK-POSITIVE: False` mapped to the existing `FIELD CHECK NOT DISCRIMINATING` stop; the payload still contains no single quote, no `$` inside a double-quoted literal and no double-quoted literal ending in a backslash. Task count unchanged at 111 (12, 12, 11, 6, 15, 12, 43); no write-set change.

Revision 1.5 (2026-10-03, preflight round 5 delta, one defect D-1 applied in place with a related sweep): (D-1) the noncanonical-subfolder test of `CMD-EVIDENCE-FIELDS` no longer uses the regex class `[\\/]`: through the plan's own channel (Bash to `pwsh -NoProfile -Command`) a doubled backslash arrives de-doubled, so the class arrived as `[\/]`, matched only `/`, and every backslash repository-relative path was counted as noncanonical, which left the count unable to reach 0; the payload now assigns `$canon = "/evidence/(baseline|regression-testing|qa-gates)/"`, normalizes each repository-relative path with `.Replace([string][char]92, "/")` before `-notmatch $canon` on both the count line and the `NONCANONICAL:` row line, and prints two controls on every run, `SUBFOLDER-CHECK-FLAGS-OTHER:` (an `evidence/other/` path, must print `True`) and `SUBFOLDER-CHECK-FLAGS-QA-GATES:` (an `evidence/qa-gates/` path, must print `False`); the heading states the normalization, the channel reason and the two controls; P6-T16 lists both labels among the printed lines, gates both values in its fourth clause (count stays five) and maps `SUBFOLDER-CHECK-FLAGS-OTHER: False` or `SUBFOLDER-CHECK-FLAGS-QA-GATES: True` to the new stop `SUBFOLDER CHECK NOT DISCRIMINATING`; P6-T44 gates both values in its precondition with the same stop and the box unchecked (the acceptance remains the single Grep clause; sibling fix: its parenthetical no longer says the precondition "gains one printed value", a revision-1.4-relative count, and states instead that the precondition's printed values are gates); P6-T46 lists both labels, gates both values in its first clause (count stays five; sibling fix: its parenthetical names the two subfolder controls beside the positive control) and carries the same stop; the AC26 mapping names both values. Related sweep (same root cause): a Grep over the whole plan for a doubled backslash returned six lines; the two payload lines were the two replaced above, and the other four are not payload text, so no other payload changed: Verified Repository Facts 13 and two self-review entries quote the discovery filter `(^|\\)\.claude\\` of scripts/vscode/Invoke-MSTestWithCoverage.ps1 line 353 as prose, and the Listing L-TEF line `private const string ArchiveRootLiteral = @"\\mailbox@example.com\Archive";` is a C# verbatim string written by the Write tool, never passed through `pwsh`, mirroring QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs line 35. The Command channel convention gains the sentence that no payload contains a doubled backslash in a string literal or a regular expression, naming the backslash-free forms (`[char]92`, `.Replace([string][char]92, "/")`, a forward-slash pattern) that `CMD-COVERAGE-DIRECT`, `CMD-COVERAGE-TEXTS`, `CMD-MEMBER-COVERAGE`, `CMD-SWEEP` and the `INHERITED` membership test of `CMD-EVIDENCE-FIELDS` already use. Single backslashes in payloads (`$anchor = "(?m)^[^\w\r\n]*"`, `\d` in `CMD-CHECK-REPO`, the `scripts\vscode\...`, `coverage\...` and `docs\features\...` path literals) pass through the channel unchanged and are untouched. Task count unchanged at 111 (12, 12, 11, 6, 15, 12, 43); no write-set change.

Revision 1.6 (2026-10-03, in-place execution-time amendment after P4-T6 at the pushed HEAD 24bc24764; two defects reported by the orchestrator, applied with sibling fixes; no executed task P0-T1 to P4-T6 or its check box was altered): (Defect 1, CS1769) the try-save seam `Func<Attachment, string, Task<bool>>` cannot cross the UtilitiesCS to UtilitiesCS.Test boundary because UtilitiesCS embeds the Outlook interop types; PD-14 records the cause, the precedent search and the design (the nested non-generic `internal delegate Task<bool> TrySaveAttachmentDelegate(Attachment attachment, string filePath)` in A); Listing L-A-FINAL gains the twelve-line declaration above the asynchronous core and its two seam parameters read `TrySaveAttachmentDelegate trySave`; Listings L-TSC-FINAL and L-TAS-FINAL return `SortEmail.TrySaveAttachmentDelegate` from `RecordingSave`; the L-A-FINAL heading, PD-2 and the E-A-RR-SECOND heading describe the P4-T4 (revision 1.5) and P4-T7 (revision 1.6) writes; TOKENS-A gains the SEAMED state (the retyped row 2, 0, 0 across EXTRACT, SEAMED, FINAL; two new rows `TrySaveAttachmentDelegatetrySave` and `delegateTask<bool>TrySaveAttachmentDelegate(`), TOKENS-TSC and TOKENS-TAS gain the two type rows, the A line-count prediction reads about 328, Verified Repository Facts 5 and the new fact 17 record the identifier check and the interop-embedding facts; P4-T7 now rewrites A (the same omitted line as P4-T4), TSC and TAS from the revision-1.6 listings, runs the three censuses, formats, builds, writes its artifact as `ITERATION: 2` and names `CS1769` in its stop vocabulary (five acceptance clauses); P4-T10 reads "as in SEAMED"; P5-T12 entries (3) and (7), P6-T18, P6-T26 and the AC8 and AC9 mappings follow; the artifact-filenames convention admits a re-run after a stop record; spec.md's technical sections are corrected in place (AC8 incorporates the exact type from the Technical specifications, so spec lines 12, 132, 195, 196, 264, 285 and 457 name the delegate, line count unchanged, applied in the revision 1.7 pass; P6-T26 verifies the correction and P6-T18 carries it). The remainder of the plan was swept for other generic instantiations over Outlook interop types crossing into a test assembly (the F1 session parameters, the U, T and EfcDataModel seams, the ToDoModel deletion, the test-local `Mock<Attachment>` uses): none. (Defect 2, per-task commits) the Execution Conventions' "no commits" rule is replaced by the per-task commit and push rule (commit point after evidence, acceptance check and check box; explicit pathspecs; message form; the P4-T14 and P4-T15 exceptions; the two anchored diff forms and the pre-commit porcelain reading), the header's anchor sentence names the same forms, the compile-red bullet and P5-T4 drop the "no commit inside a span" sentence (P4-T1's executed copy is superseded by the rule), and every remaining task was swept: P5-T3 gates the committed state with `git diff --name-status MERGE-BASE HEAD` (six rows) and reads the porcelain as this task's own formatter rewrite; P5-T5 and P5-T11 state the pre-commit timing of their working-tree numstat and porcelain; P5-T9 pairs `ART-PORCELAIN: EMPTY` with the anchored `ART-DIFF: EMPTY`; P5-T10 hands the unchanged `CMD-DELETE` payload to the coordinator (`DELETE HANDOFF TO COORDINATOR`), records the coordinator-run output as P4-T5's `ITERATION: 2` record does, and takes its porcelain and name-only diff before its commit; P4-T7 to P4-T13, P5-T1, P5-T2, P5-T6 to P5-T8, P5-T12, P6-T2 to P6-T11, P6-T17 and P6-T19 to P6-T46 carry no porcelain or diff gate; P6-T1, P6-T12, P6-T16 and P6-T18 are unchanged because their git reads are a within-payload before-and-after comparison, working-tree diffs against `MERGE-BASE` with untracked porcelain, or a HEAD observation, each valid in every commit state. Task count unchanged at 111 (12, 12, 11, 6, 15, 12, 43); no write-set change (the delegate is declared inside A).

Revision 1.7 (2026-10-03, preflight round on the revision 1.6 delta, six textual defects D-1 to D-6 applied verbatim, no other change): (D-1) because AC8 incorporates the try-save delegate type by reference, the planner corrected FEATURE/spec.md technical sections in place under orchestrator permission (lines 132 twice with the inserted declaration sentence, 195, 196, 264, 285 and 457 now name `TrySaveAttachmentDelegate`; line 12 now records the revision 1.6 type correction and no longer states that the plan runs no git commit; line count 681 unchanged; no acceptance-criterion text changed), PD-14 and the revision 1.6 log sentence state the in-place correction, P6-T26's precondition gains the two FEATURE/spec.md Grep counts (`Func.Attachment` zero lines, `TrySaveAttachmentDelegate` at least six lines, stop `AC8 NOT MET (TECHNICAL SPECIFICATIONS TYPE DIFFERS)`) and P6-T18 states the correction is already applied; (D-2) P5-T10 reports the `git rev-parse HEAD` output as `HEAD-AT-HANDOFF:` inside the handoff report, writes no artifact and creates no commit at the handoff, and its single artifact written on resume records `HEAD-AT-HANDOFF:`, which must equal the channel note's HEAD (otherwise `DELETE CHANNEL REFUSED`); the commit bullet exempts the handoff from the stop-record commit; (D-3) the artifact-filenames convention counts a file without an `ITERATION:` row as iteration 1; (D-4) the A line-count prediction reads about 328 (315 on disk after the first P4-T7 format plus the thirteen lines of the delegate block) here and in the revision 1.6 log; (D-5) the L-TSC-FINAL and L-TAS-FINAL headings describe the revision 1.6 form (RecordingSave returns SortEmail.TrySaveAttachmentDelegate), the revision-1.5 writes by P4-T1 and P4-T2 and the P4-T7 rewrite; (D-6) the AC8 mapping's TESTS field names the P6-T26 spec Grep. Task count unchanged at 111 (12, 12, 11, 6, 15, 12, 43); no write-set change; no executed task or check box altered.

Revision 1.8 (2026-10-03, in-place execution-time amendment after the P4-T9 stop `FAIL-BEFORE WRONG REASON` of 2026-10-03T11-28 at HEAD ce3e73701; one defect, applied with a sibling sweep; no executed task P0-T1 to P4-T8 or its check box was altered): the P4-T9 fourth clause asserted the full origin-folder literal in the failed row's `MESSAGE`, but FluentAssertions prints a long string difference as a window after an ellipsis around the first differing index (23 here), so the literal is never printed on the success case and the clause could not be met; the clause now requires the three backslash-free tokens `GetDirectoryName(helper.FilePathSaveAlt)`, `origin"` and `destination"` together, which the observed message carries and which a primary-path or file-name failure would not (the first token names the alternate path's directory assertion); the Test Inventory row for P4-T9 names the same tokens (P6-T17 reads that table); P4-T9 states that its re-run rewrites the fixed-name artifact in full with `ITERATION: 2`, superseding the stop record, and that the re-run is the planner-amended re-run the Stop discipline admits. Sweep of every remaining message-substring gate: P4-T14 `but found 3` (a collection `HaveCount` message, not a string difference; `sessionFields.Should().HaveCount(4)` precedes `resetTargets.Should().HaveCount(4)` in the structural test and the static field count is untouched by the mutation) and P5-T7 `but found 0` (an integer `Be` message reached after the `ThrowAsync` assertion passes) are not truncated and carry no backslash, unchanged; P6-T17 quotes the substring observed per row from the amended table, unchanged; P6-T28, P6-T29 and the AC10, AC11 and AC18 mappings cite those gates by reference, unchanged; the executed gates P1-T6, P1-T7, P2-T6 and P3-T3 are history. FEATURE/spec.md unchanged (AC11 requires the failure "on the alternate-path assertion", which the first token proves; AC20 and AC26 name no message substring). The Stop discipline convention now names, beside the toolchain-loop restart, the re-run of a stopped task that its planner-amended task text directs (described per task for P4-T7 and P4-T9) and the repair-branch re-runs of P6-T16 and P6-T46, so P4-T9's statement that the Stop discipline admits its re-run, as P4-T7's was, holds against the convention text, and the convention's list of repetitions is complete. Task count unchanged at 111 (12, 12, 11, 6, 15, 12, 43); no write-set change.

Revision 1.9 (2026-10-06, in-place widening after P6-T12 at HEAD 9de3f176d, the orchestrator's statement; one related defect absorbed under the maintainer directive that related defects are remediated inside this item; no executed task P0-T1 to P6-T12 or its check box was altered): the XML documentation comment of `Cleanup_Files_DoesNotThrow` in TST1 (lines 170 to 173) describes the pre-F1 static `YesNoToAllResponse` fields, which no longer exist, and does not describe the `foreach` over `AllPromptSessions` that `Cleanup_Files` now is (PD-15: the alternate-name state that the transient L3 phase-one reset targeted as `_attachmentsAltName` is now `AttachmentsAltNamePrompt`, the third element of `AllPromptSessions`, reset through its session's `Reset()`). Three tasks are inserted before the former P6-T13: P6-T13 applies Edit E-TST1-DOC-CLEANUP (lines 171 to 172 replaced by two lines, the four-line element and the 488-line file kept, no behavior change, no regression test, as PD-15 states) and verifies it with two Grep-tool searches (`every prompt session in AllPromptSessions` one line, `YesNoToAllResponse tracking fields` zero lines; 0 and 1 before the Edit); P6-T14 runs `CMD-SCOPED-FORMAT` on TST1, `CMD-CHECK-REPO` and the TST1 census (TOKENS-TST1 FINAL unchanged, the OLD and NEW texts containing no census token; `LINES` 488 as P6-T10 recorded); P6-T15 re-runs the P6-T12 anchor recheck and `CMD-FOOTPRINT` over the edited tree and writes the P6-T12 artifact name at `ITERATION: 2`, which the artifact-filenames convention now names, so that every later `p6-t12` reader resolves to the post-edit record. The former P6-T13 to P6-T43 are renumbered P6-T16 to P6-T46 (new = old + 3) throughout the plan, including the earlier revision-log entries, the self-review record, the Command Reference headings, the E-AC-CHECKOFF heading, PD-9, PD-11, the Phase 6 loop rule and the AC mappings, and the artifact names p6-t13-identity-and-sweep, p6-t21-ac6-deferred, p6-t42-ac27-deferred and p6-t43-ac-inventory became p6-t16-, p6-t24-, p6-t45- and p6-t46-, so that every task ID in this file names exactly one task; a reference inside an earlier log entry or self-review bullet to one of these tasks therefore reads with the new number (subtract 3 to recover the number that entry used when it was written). The checked tasks P0-T1 to P6-T12 keep their numbers and text. `CMD-TST-IDENTITY` (now P6-T16) gains two printed labels, `TST1-REMOVED-DOC-LINES:` (removed lines of the anchored TST1 diff carrying the old phrase) and `TST1-ADDED-DOC-LINES:` (added lines carrying the new token), each gated at 1 in the second P6-T16 clause (0 and 0 before P6-T13; `TST1-REMOVED-LINES:` stays recorded, not gated; clause count stays five); the existing payload lines, including `$removed1` and the `TST1-REMOVED-*` labels, are unchanged: the enforce-epic-worktree-removal-gate.ps1 refusal of 2026-10-03T12-56 (recorded in p6-t13-identity-and-sweep.2026-10-03T12-56.md, the stop record of the task now numbered P6-T16, which stays on disk unchanged and matches no glob of this plan) is handled by the maintainer's standing approval of 2026-10-04 routing the payload through the coordinator relay unchanged, and no identifier was renamed to avoid the hook; the P6-T16 re-run writes p6-t16-identity-and-sweep.<TS>.md at `ITERATION: 2`. Every gate that encodes TST1's expected diff was re-derived against HEAD 9de3f176d plus the planned Edit: `CMD-FOOTPRINT` carries no TST1 numstat and TST1 is Write Set path 10, so `OUTSIDE-WRITE-SET:` and `WRITE-SET-MISSING:` keep their values; TOKENS-TST1 FINAL is unchanged; the `LINES` value 488 and the `AC23-CLOSEST:` prediction are unchanged (two comment lines replace two comment lines); `NAMES-TST1-FINAL`, Listing L-TST1-ROWS, the Test Inventory and the P6-T5 and P6-T7 row sets are unchanged (no test name changes); the AC5 pins (`TST1-REMOVED-TRYSAVE-LINES: 0`) are untouched because the replaced lines name neither try-save test. AC24's recorded final pass (P6-T1 to P6-T11, `ITERATION: 1`) precedes the edit; the analyzer and nullable rebuilds and the test runs are not repeated for a change confined to the prose of a summary element (no markup character is added, so the element stays well-formed, and the formatter does not reflow comments, which is why the read-only `csharpier check` of P6-T14 is the toolchain step that observes the edit); this disposition is stated for the orchestrator and is reversible through the Phase 6 loop rule's restart form (`ITERATION: 2` of P6-T1 to P6-T11) without any task-text change. FEATURE/spec.md unchanged (no acceptance-criterion text; AC22 already lists TST1; AC14's "their tests are unchanged" names the `SanitizeArrayLineTSV` and `StripTabsCrLf` tests, not `Cleanup_Files_DoesNotThrow`; AC5's two TST1 pins are untouched). Task count 114 (12, 12, 11, 6, 15, 12, 46); no write-set change.

Revision 2.0 (2026-10-06, in-place append of Phase 7 and Phase 8 after the executed P0-T1 to P6-T46 at HEAD eb7871502, the orchestrator's statement; the directive was an in-place revision that appends phases to an executed plan, so no existing phase, task, text or check box was altered and no task was renumbered): (1) PD-16 records the orchestrator's dispositions of the seven non-blocking findings of FEATURE/code-review.2026-10-06T15-30.md under the maintainer related-defect directive: CR-1 remediated by the synchronous image-arm test SS4 in TAS (Edit E-TAS-SS4; no fail-before, because the behaviour is already correct and the discriminator is the Cobertura condition coverage of A line 143, `50% (1/2)` before and `100% (2/2)` after), CR-3 remediated by removing the unused `using System;` of TSC (Edit E-TSC-USING, after a Grep confirms no `System` type is named; a skip branch is authorized in P7-T3's text), CR-2 not changed (uncompiled QuickFiler/Legacy/QfcController.cs, for filing with U-2), CR-4 not changed (both files under 500 lines), CR-5 not changed (spec D11), CR-6 informational, CR-7 not changed (acceptance-criterion wording is not altered); (2) Phase 7 (P7-T1 to P7-T16): pre-edit observations with the false-before condition reading, the two Edits with Grep verification, the scoped format and census (TOKENS-TAS and TOKENS-TSC gain an SS4 column: `[TestMethod]` 12, `SortEmail.SaveAttachment(` 4, the SS4 name 1, `usingSystem;` 0), the test-project build and the twelve-row class run, then the full toolchain pass over the whole solution in CLAUDE.md order with the Phase 6 loop rule (format, check, analyzer rebuild, nullable rebuild, the three scoped suites at 56, 3 and 11, the repository-wide coverage run at 7394 with floors), the coverage comparison with the P6-T8 clauses plus the family-identity no-regression gate (`PHASE 7 FAMILY COVERAGE CHANGED`) with the Phase 6 first-party figures 85.39 and 79.81 recorded, the per-member measurement plus the CR-1 arm at `100% (2/2)` (`CR-1 ARM STILL UNCOVERED`), the toolchain record (which also covers the P6-T13 documentation-comment edit, policy-audit G-4), the footprint at the Phase 0 to 7 anchor (P7-T15, eight P6-T12 clauses unchanged) and the closure with spec, evidence-field and hygiene checks and a pull-request-body addendum; the fixed-name final-pass artifacts are rewritten at `ITERATION: 2` with `SUPERSEDES:` rows; (3) Phase 8 (P8-T1 to P8-T13), whose heading states that it runs only after the orchestrator's re-review of Phase 7 passes and whose first task records the `PHASE 7 RE-REVIEW: PASS` authorization: the fetch and pre-merge facts including the main-side hunk of QuickFiler.Test/QuickFiler.Test.csproj, the `git merge --no-ff origin/main` with the union resolution of PD-17 for that one file and `git merge --abort` plus `MERGE CONFLICT OUTSIDE EXPECTED UNION` for any other conflict, the additions-only fan-in gate (`CMD-FANIN`: own paths only against `ORIGIN-MAIN-SHA`, `1 0` on the shared file, three ancestry checks with a reversed control, two committed-range negative controls), the full toolchain pass on the merged tree with the floors gated and the Phase 7 figures recorded, the CR-1 arm still gated, the fixed-name artifacts at `ITERATION: 3`, the named push with the remote-tip check, and the closure recording the new merge base; (4) PD-17 and the header's anchor bullet state that the Phase 0 to 7 `MERGE-BASE` rules are not re-run after the merge and that Phase 8 anchors at `ORIGIN-MAIN-SHA:`; the Execution Conventions gain the Phase 8 git exceptions (`git fetch`, `git merge --no-ff`, `git merge --abort` under the conflict branch, the named push), the Phases 7 and 8 bullet (fixed-name rewrites with `ITERATION:` and `SUPERSEDES:`, the loop rule, the hook containment rule derived from the gated shapes in .claude/hooks, which is why the two new payloads `CMD-LINE-CONDITION` and `CMD-FANIN` carry no `remove`, `gh`, `merge`, `create`, `edit`, `issue` or `new` text, why `CMD-TST-IDENTITY` and `CMD-DELETE` are not reused and why every Phase 8 git command is a plain `git -C WORKTREE` invocation); (5) Verified Repository Facts 19 records the HEAD eb7871502 facts (A lines 143 to 145, the Phase 6 Cobertura node at document lines 59687, 59791 and 59802, TAS 437 lines with the unique E-TAS-SS4 anchor, TSC 343 lines with the single `using System;`, the item and sibling-worktree csproj line positions, the packed-refs origin/main observation, the hook shapes and the evidence figures), the Test Totals gain the Phase 7 values and `NAMES-TAS-FINAL2`, the Census Expectations gain the SS4 columns, and the Edit Specifications gain E-TAS-SS4 (OLD lines 296 to 300, NEW inserting thirty-two lines) and E-TSC-USING (OLD lines 1 to 2); (6) the AC mappings of AC9, AC21, AC22, AC23, AC24, AC25 and AC26 gain the Phase 7 and Phase 8 evidence; no acceptance criterion is added or amended and no check box changes. Planner limitations recorded: no shell, so the main-side csproj hunk is an observation from a sibling worktree and the fetched origin/main value is observed by P8-T1; the comparison against the Phase 6 first-party figures is recorded rather than gated, because a same-tree two-decimal comparison has a margin of about six lines and two branches and can trip on run-to-run variance in assemblies this item does not touch; the changed-line no-regression rule is gated on the SortEmail family rows instead (preflight round on revision 2.0). Task count 143 (12, 12, 11, 6, 15, 12, 46, 16, 13); no write-set change (Phase 7 edits items 12 and 13; Phase 8 adds no path of its own). Preflight round 13 deltas on revision 2.0 applied in place at HEAD 66b444507 (2026-10-06, five defects D1 to D5 applied verbatim): D1 PD-17 names the second path changed on both sides, .claude/agent-memory/orchestrator/MEMORY.md, and `CMD-FANIN` gains the `SHARED-PATHS:`, `SHARED-NUMSTAT`, `SHARED-WITH-LOSS:` and `LOSS-CHECK-CONTROL:` lines with the matching ninth P8-T3 clause (`FAN-IN DROPPED MAIN LINES`); D2 P7-T12 gates the changed-line no-regression rule on the SortEmail family rows (`PHASE 7 FAMILY COVERAGE CHANGED`) and records the Phase 6 first-party comparison without gating it, with the AC25 mapping aligned; D3 P7-T1 counts six clauses; D4 the `FILES:` floors read 103 (P7-T16) and 112 (P8-T13); D5 the Phase 8 phase rule names the csproj union resolution as the one Write Set write; one sibling phrase in the `CMD-FANIN` note ("the one overlapping file") narrowed to the csproj overlap. Task count unchanged at 143; no check box changed.

## Planner Self-Review Record (revision 2.0 pass, 2026-10-06; the earlier passes retained below)

SELF-REVIEW: RE-DERIVED THIS PASS

Every line the preflight round 13 deltas on revision 2.0 touched in this plan (PD-17, the `CMD-FANIN` payload and its note, P7-T1, P7-T12, P7-T16, the Phase 8 phase rule, P8-T3, P8-T13, the revision 2.0 log entry, the AC25 mapping and this record), and the sibling lines in the same paragraph, payload, task, phase rule and mapping, was re-derived against WORKTREE at HEAD 66b444507 (the orchestrator's statement; no shell was available, so the git facts D1 states are the preflight reviewer's observations and PD-17 attributes them as such) with the Read, Grep and Glob tools in this pass (the "Revision 2.0 delta pass" entry below). Every line the revision 2.0 append touched in this plan (the header, the Write Set sentence, PD-16 and PD-17, the three Execution Conventions amendments, Verified Repository Facts 19, the Test Totals addendum, Edits E-TAS-SS4 and E-TSC-USING, the Census addendum, the two new payloads, Phases 7 and 8, the revision log entry, the AC9, AC21, AC22, AC23, AC24, AC25 and AC26 mappings and this record), and the sibling lines in the same file region, task, payload, convention bullet and mapping, was re-derived against WORKTREE at HEAD eb7871502 (the orchestrator's statement, confirmed by the worktree's branch ref file and reflog) with the Read, Grep and Glob tools in this pass (the "Revision 2.0 pass" entry below); no shell was available to the planner, so no git command was run and every git fact of Phase 8 is observed by its tasks rather than asserted here. Every line the revision 1.9 widening touched in this plan (the header, Write Set item 10, PD-15, the artifact-filenames and Stop-discipline conventions, Verified Repository Facts 18, the Census Expectations TST1 clause, Edit E-TST1-DOC-CLEANUP, the `CMD-FOOTPRINT` and `CMD-TST-IDENTITY` headings and the three added payload lines, the Phase 6 loop rule, the inserted P6-T13 to P6-T15, the widened P6-T16, every renumbered reference, the revision log, the AC22, AC23 and AC24 mappings and this record), and the sibling lines in the same file region, task, payload, convention bullet and mapping, was re-derived against WORKTREE at HEAD 9de3f176d (the orchestrator's statement; no shell was available) with the Read, Grep and Glob tools in this pass (the "Revision 1.9 pass" entry below). Every line the revision 1.8 amendment touched in this plan (the header, the Test Inventory row, P4-T9, the revision log and this record), and the sibling lines in the same table, task, convention bullet and AC mapping, was re-derived against WORKTREE at HEAD ce3e73701 (the orchestrator's statement; no shell was available) with the Read and Grep tools in this pass (the "Revision 1.8 pass" entry below). Every line the revision 1.7 delta touched in this plan and in FEATURE/spec.md, and the sibling lines in the same spec paragraph, convention bullet, heading, task, log entry and AC mapping, was re-derived against WORKTREE at HEAD b0720285a with the Read and Grep tools in this pass (the "Revision 1.7 pass" entry below). Every citation the revision 1.6 amendment touched, and the sibling lines in the same listing, table, convention bullet, task and AC mapping, was re-derived against WORKTREE at the pushed HEAD 24bc24764 with the Read, Grep and Glob tools in the revision 1.6 pass (the "Revision 1.6 pass" entry below). Every citation the revision 1.5 delta touched, and the sibling lines in the same task, payload, heading, convention bullet and AC mapping, was re-derived against WORKTREE (the assigned item worktree named by the delegation prompt, under the main checkout's `.claude/worktrees/` directory) with the Read, Grep and Glob tools in this pass (the "Revision 1.5 pass" entry below). The remaining entries were re-derived against the same tree in the revision 1.1, 1.2, 1.3 and 1.4 passes; no revision 1.5 edit touched them and P0-T3's `CITED-TREE-EXIT:` gate re-proves them at run time. No shell was available to the planner in any pass, so git-state facts (including whether origin/main changed .gitignore after 94287369) are observed by P0-T3 and P6-T12 rather than asserted, and the behaviour of the field-test regex is derived by reading it rather than by running it: `FIELD-CHECK-CONTROL:` and `FIELD-CHECK-POSITIVE:` are the run-time proof. The behaviour of the subfolder test through the channel is the preflight round 5 reviewer's observation, not the planner's (the five new payload lines run through Bash and `pwsh -NoProfile -Command` printed `SUBFOLDER-CHECK-FLAGS-OTHER: True` and `SUBFOLDER-CHECK-FLAGS-QA-GATES: False`, with a baseline path and a regression-testing path `False` and a file directly under evidence/ `True`); those two labels are its run-time proof on every run. The bounded record below carries exactly sixty-six `CITATION:` lines (thirty-nine from revision 1.5, the four of the revision 1.6 pass, the two of the revision 1.8 pass, the three of the revision 1.9 pass and the eighteen of the revision 2.0 pass; the TST1, A, T and FEATURE/spec.md lines additionally carry a HEAD 9de3f176d locator appended in the revision 1.9 pass, and the A, TAS and QuickFiler.Test.csproj lines a HEAD eb7871502 locator appended in the revision 2.0 pass).

- UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs (full read, 343 Read lines): usings 2 to 19; fields 25 to 28; `Cleanup_Files` 30 to 36 (no `_attachmentsAltName` reset); the ten attributes at 38, 63, 103, 164, 227, 241, 290, 311, 322 and 333; `YesNoToAll.ShowDialog(` call expressions 112, 135, 175, 198, 255 and the commented 258 (TOKENS-A base 6); `SaveCase` 290 to 309 with the combined labels at 300 and 303 (Edit E-A-L1 anchor); `SaveCaseAsync` 241 to 288 with `_attachmentsAltName = YesNoToAllResponse.Empty;` at 275 and the field initializer `private static YesNoToAllResponse _attachmentsAltName = YesNoToAllResponse.Empty;` at 27, which the whitespace-stripped census also matches (sibling: TOKENS-A base count 2, P1 count 3); destination overload 227 to 239 with the single `FolderPathSave = destinationPath` at 235; `IsPicture` 311 to 320; `SaveMessageAsMsgAsync` 322 to 331 and `SaveMessageAsMSG` 333 to 340 (carried verbatim into L-A-FINAL); `_picturesOverwrite = YesNoToAllResponse.Empty;` at 34, 128 and 191 (sibling: the E-A-L3 anchor is the two-line pair 34 to 35 and is unique).
- UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs (full read, 173 Read lines): `RemoveReadOnlyPrompt` 28 to 30; two-argument wrapper 32 to 51 (lambda 49); three-argument forward 53 to 73; five-argument core 75 to 160 with `createDirectory` 97, `catch (System.UnauthorizedAccessException e)` 101 (indent 12), `Debug.WriteLine` 103, 127, 147, prompt 108 to 113, inner `catch (System.Exception inner)` 125 (indent 20), recursion 134 to 140, `else { throw; }` 151 to 154, catch brace 155, outer `catch (System.Exception) { throw; }` 156 to 159, method brace 160; `ClearReadOnlyAttributeOnDisk` 165 to 170. Sibling check: `throw;` occurs exactly at 153 and 158 and `catch (` exactly three times (TOKENS-T base 2 and 3), which fixes the exemption rules (2) and (3) at baseline and the final-shape branch of rule (3).
- UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs (full read, 196 Read lines): the six attributes 26, 82, 94, 109, 139, 172; `StripTabsCrLf` 127 to 137 without attribute; `WriteCSV_StartNewFileIfDoesNotExist` 139 to 170 with `strOutput` 145, reversed `Path.Combine` 147, header rows 151 to 163, `SanitizeArray(strAryOutput, ref strOutput);` 165, `FileIO2.WriteTextFile(strFileName, strOutput!, folderpath: strFileLocation);` 166 (Edit E-U-SEAM-WRITE anchor, once); `SanitizeArray` 172 to 193 with `Debug.WriteLine` 177 and `strOutput![j]` 183. Sibling check: `SanitizeArrayLineTSV(` does not match the census token `SanitizeArray(`.
- UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs lines 1 to 30 and SortEmail.MailItemSort.cs lines 1 to 24: the identical eighteen directives at 2 to 19 (Edits E-S-USINGS and E-M-USINGS); S `logger` 25 to 27.
- UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs lines 1 to 30: `MAX_PATH` 25, attribute 27, `SaveAttachmentsOld` 28.
- QuickFiler/Controllers/EfcDataModel.cs: lines 1 to 60 (usings including `UtilitiesCS` 14, logger 23 to 25, constructor 48), 196 to 262 (`MailInfo` 206, `TryGetFirstInSelection` 208 to 226, `TryGetArchiveRoot` 245 to 261), 268 to 311 (guards 278, 289, 294; 308 to 310, the E-E-SEAM-CALL anchor), 320 to 326 (the E-E-SEAM-MEMBER anchor, `return new EmailFiler(config).SortAsync(mailHelpers);` once), 328, 372 to 398 (`MAPIFolder` overload) and 440 to 465; Grep: `finally` 0 occurrences, `return result;` exactly once at 310 (TOKENS-E base values).
- QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs (full read, 400 Read lines): the eleven test methods at 48, 71, 94, 119, 144, 175, 196, 226, 251, 271, 293 (`NAMES-EFC-ARCHIVE`); `MoveAsync` 314 to 323; `CreateOlObjects` 330 to 333; `CreateGlobals` 340 to 352; `SpecialFoldersWithOneDrive` 355 to 360; `SpecialFoldersWithoutOneDrive` 363 to 366; `TestableEfcDataModel` 379 to 397 with `protected internal override` on `InvokeFilerAsync` (the override form Listing L-TEF mirrors under InternalsVisibleTo).
- UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs (full read, 71 Read lines): internal sealed class 13; constructor 24; `Response` private setter 33; `Ask` 40 to 48; `ReleaseSingleAnswer` 54 to 60; `Reset` 65 to 68.
- UtilitiesCS/OutlookObjects/Attachment/AttachmentHelper.cs lines 28 to 40 (three-argument constructor 34 to 37) and 96 to 205 (`FilePathSave` and `FilePathSaveAlt` assigned 101 to 111 from strings; `FilePathHelperSaveAlt` internal 174 to 175; `FilePathSave` 178 to 182; `FilePathSaveAlt` 185 to 189; `FolderPathSave` 199 to 203); UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs lines 78 to 93 (`FolderPath` setter 83 to 91) and 345 to 399 (handler 348; `FolderPath` case recomputes `_filePath` 360 to 367; `FilePath` case splits folder and name 369 to 396), which is the basis of the re-rooting test's assertions.
- UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs (full read, 458 Read lines): the fifteen `[TestMethod]` at 41, 54, 79, 104, 140, 157, 174, 182, 209, 245, 272, 291, 316, 341, 359; `GetAttachmentsInfo` test 182 to 207 and the asynchronous test 209 to 234 (Edit anchors, verbatim); sandbox constant 238; the two try-save tests 245 to 266 and 272 to 289; `SanitizeArrayLineTSV` test 341 to 357; blank 358; `SanitizeArray` test 359 to 382; blank 383; `#endregion` 384 (Edit E-TST1-DEL-SANITIZE spans 357 to 384); helpers 386 to 455.
- UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs (full read, 376 Read lines): the eleven `[TestMethod]` at 33, 57, 84, 110, 140, 165, 193, 217, 245, 273, 297 (`NAMES-T`); constants 24 to 27; `SaveAsync` documentation 319 to 322 (Edit E-TST2-T12 anchor); `Seams` 338 to 373 with lines 348 to 357 (Edit E-TST2-TRIPWIRE anchor) and `Prompt` 368 to 372; `Times.Exactly(2)` at 77, 103, 186 and 316 (TOKENS-TST2 base 4; FINAL 5 with the one assertion of Edit E-TST2-T12); `[TestMethod]`, `new Seams(` and `SetupSequence` eleven each (lines 33 to 304).
- UtilitiesCS/UtilitiesCS.csproj lines 814 to 825 (Compile 817 to 824; Edit E-UCS-CSPROJ-REMOVE spans 819 to 821); UtilitiesCS.Test/UtilitiesCS.Test.csproj lines 94 to 103 (98 to 100); QuickFiler.Test/QuickFiler.Test.csproj lines 122 to 131 (127 to 128).
- docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md lines 96 to 157: line 99 ends `so the exception boundary is unchanged. The recursive retry passes all three seams.` (the E-SPEC956-99 OLD clause is a strict infix, once); line 149 ends `and no retry bound is added (L2 is out of scope).`; line 155 is the `DirectoryInfo` bullet. Sibling check: the inserted note goes after 149 and before the `### Boundaries and invariants` heading at 151.
- FEATURE/spec.md (full read, 682 Read lines): planner note 12; D1 to D18 at 128 to 145; Technical specifications 231 to 532; Test Strategy 567 to 624 with the artifact names 607 to 621 and the UT5 call-out 624; `## Acceptance Criteria` 627; AC1 to AC27 at 628 to 654, each line beginning `- [ ] ACn (`. FEATURE/issue.md (full read): `- Work Mode: full-bug` 12; scope rule 35.
- FEATURE/research/2026-10-02T05-15-sort-email-latent-logic-defects-research.md lines 325 to 384 (section 3.5: the tripwire 341 to 350 and the T12 assertions 358 to 366); FEATURE/research/2026-10-02T05-50-sort-email-966-consolidation-research.md (full read, 920 lines): sections 0.2, 1.3, 1.5, 1.6, 1.8, 3, 4.2, 4.3, 5.2, 5.4, 6.2, 7.2, 7.3, 8, 9.3, 10, 11, 13 and derivations N1 to N11.
- scripts/vscode/Invoke-MSTest.TrxSummary.ps1 (full read): `Get-TrxRunSummary` 12 with `FailedTestName` from `testName` 84, `Format-TrxRunSummary` 103 with the `Total ... passed ... failed` line 140 to 141; scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 lines 160 to 309 (`LineMap` keyed by `[int]` with `Hits` 192 to 215; `CoveredLines` counts `Hits -gt 0` 240 to 243; `Merge-CoberturaClassesByFilename` 260); scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1 lines 1 to 163 (two-decimal invariant percentages 90 to 91 rendered as `(nn.nn%)` 117 to 120, the source of the `FIRST-PARTY-*-PERCENT:` regex); scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1 lines 1 to 150; scripts/vscode/Invoke-MSTestWithCoverage.ps1 lines 1 to 130 (fixed `TestCategory!=LiveOutlook` 91; `ConvertTo-DerivedCoverageSettingsXml` 97) and 330 to 461 (discovery exclusion `(^|\\)\.claude\\` 353, the reason for the DIRECT route; entry guard 459); scripts/vscode/TaskMaster.cli.runsettings (full read; Workers 5, Scope 6).
- Observed success-case outputs this plan asserts over: docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/baseline/coverage-baseline.md (labels of CMD-COVERAGE-DIRECT and CMD-COVERAGE-POST, the `First-party coverage:` line shape, the projection), evidence/baseline/test-run-baseline.md (CMD-VSTEST labels and `RESULT` rows), evidence/baseline/p0-t9-stall-probe.2026-10-01T20-41.md (`STALL-PROBE: REPRODUCES`, the exclusion text), evidence/baseline/p0-t4-channel-and-toolchain.2026-10-01T20-38.md (bootstrap outputs), evidence/qa-gates/coverage-comparison.md (the baseline uncovered set of this item: T 49, 154, 155 and M 153; `CONTROL-LINE: 28`; the `SORTEMAIL-CLASS` filename form with backslashes); docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/regression-testing/pass-after-shape-b.2026-09-16T23-27.md lines 31 to 47 (data rows as individual `RESULT` entries with no parent row).
- The #956 plan docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/plan.2026-10-01T06-34.md lines 1097 to 1573 (Command Reference), 1574 to 1740 (phases) and 1753 to 1853 (record shape).
- Greps over the worktree: `DoNotParallelize|Thread\.Sleep|Task\.Delay|Timeout|Directory\.CreateDirectory|File\.Create|File\.WriteAll|GetTempPath|MemoryAppender` over UtilitiesCS.Test/EmailIntelligence/SortEmail_*.cs: 0 (TOKENS and `BANNED-TEST-APIS` base); the ten new identifiers (`ResetFilerPromptState`, `RedirectSaveFolder`, `AllPromptSessions`, `MovedMailsHeader`, `TrySaveAttachmentCoreAsync`, `CreateDirectoryLimit` and the four new class names) over `*.cs` and `*.csproj`: 0; Glob: .claude/rules/csharp.md, .claude/skills/acceptance-criteria-tracking/SKILL.md and .claude/skills/policy-compliance-order/SKILL.md exist (P0-T1 list); docs/features/potential/promoted/2026-10-01-sort-email-latent-logic-defects.md exists (Clause A).
- .claude/rules/plan-acceptance-gates.md (full read): G1 to G9; every `git diff` in this plan carries the `MERGE-BASE` ref operand and is paired with a porcelain span; no `--cov` command; the write-mode formatter commands record BEFORE and AFTER hashes and the read-only check; every Grep-tool acceptance pattern is a regular expression with escaped brackets and parentheses.
- Revision 1.1 pass (every citation the delta touched, re-derived with Read and Grep in this pass, plus its siblings): A lines 27 and 275 (`_attachmentsAltName = YesNoToAllResponse.Empty;` twice, Grep) and the sibling TOKENS-A rows re-checked against the full A read (`_responseSaveFile` at 25 and 32, `YesNoToAllResponse _` at 25 to 28, `File.Exists` at 106 and 169, `TrySaveAttachmentAsync` at 221, 266 and 281, `SaveCaseAsync(` at 179, 203 and 242, the pipe labels at 300 and 303 only, `using System.Diagnostics;` once at 4 distinct from `using System.Diagnostics.CodeAnalysis;` at 5); `ShowDialog(` Grep over `SortEmail*.cs`: A 112, 135, 175, 198, 255, 258 and L 165, 182, 199 (nine; L 199 is `InputBox.ShowDialog(`); TST2 `Times.Exactly(2)` at 77, 103, 186, 316 and the plan's Edit E-TST2-T12 line `attachment.Verify(x => x.SaveAsFile(SandboxFilePath), Times.Exactly(2));` once; TST2 `[TestMethod]`, `new Seams(`, `SetupSequence` eleven each; Grep `^` line counts A 342, T 172, U 195, S 277, M 388, L 240, E 464, TD 402, TST1 457, TST2 375, and Read tails S 278, M 389, L 241 (each ends with a newline); method-name tokens: `Cleanup_Files_ResetsEveryPromptAnswerField` five times in Listing L-TAS-P1 (four `DisplayName` strings and the declaration), `Cleanup_Files_ResetsEveryPromptSession` once in Listing L-TAS-FINAL, the three TEF names once each in Listing L-TEF, the T12 name once in Edit E-TST2-T12, `SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows` once in TST1 (360) and the two try-save names once each (246, 273); TST1 182 to 234 read verbatim and equal to the OLD blocks of E-TST1-ROWS-SYNC and E-TST1-ROWS-ASYNC; E-TST1-DEL-SANITIZE OLD 28 lines (357 to 384) and NEW 3, the two row edits OLD 26 and NEW 52 each (TST1 484 before the format); FEATURE/spec.md D17 at 144 (three evidence subfolders), AC6 at 633 (the pull-request clause), AC21 at 648 (the full run passes), AC23 at 650, AC24 at 651 (zero exit codes in one final pass), AC26 at 653 (baseline, regression-testing or qa-gates); every `PATHS-CITED` path confirmed present by Glob (QuickFiler/Controllers/QfcItemController.MailActions.cs, dotnet-tools.json at the root, the four `ToDoModel.Test/Email Utilities` files, QuickFiler/Legacy/QfcController.cs, "UtilitiesCS/To Depricate/FileIO2.cs", the four packages.config files, .gitattributes, .csharpierignore, coverage.config, global.json, ToDoModel/ToDoModel.csproj, UtilitiesCS.Test/Threading/CurrentStoreContextTests.cs, the three QuickFiler controller callers, AttachmentSerializable.cs, the #895 evidence file); the plan's own `.claude` filters (only CMD-GREP-FACTS used the absolute-path form; CMD-COVERAGE-DIRECT already substrings the root) and porcelain acceptances after P4-T5 and P5-T10 (only P5-T3 spans the deleted partial's directory); the Grep over this plan for `evidence/other` after the edit (zero task-path hits).
- Revision 1.2 pass (every citation the round 2 delta touched, re-derived with Read and Grep in this pass, plus its siblings): .gitignore in WORKTREE matched by content for the six entries `artifacts/`, `*.trx`, `*cobertura*.xml`, `coverage/*`, `!coverage/.gitkeep` and `.dotnet*/` (one line each; observed at 57, 146, 147, 150, 151 and 357 in this tree, numbers recorded as the observation only and no longer cited anywhere in the plan; Grep over the plan for `.gitignore` after the edit returns only the content-form lines); U lines 137 to 170 read verbatim: attribute 139, the two-parameter signature 140 to 143, `File.Exists(Path.Combine(strFileName, strFileLocation))` 147, `new string[14, 2]` 149, the thirteen header assignments 151 to 163, `SanitizeArray(strAryOutput, ref strOutput);` 165, `FileIO2.WriteTextFile(...)` 166, closing brace 170, equal to the OLD block of Edit E-U-SEAM-HEAD (139 to 149) and confirming that no four-parameter overload exists before P2-T3; Listing L-TUL's two calls pass four arguments (`LogFileName, LogFolder, fileExists, writeTextFile`), so the file does not compile between P2-T2 and P2-T3; the Phase 2 task text: P2-T4 runs `CMD-SCOPED-FORMAT` only (format and read-only check, no build) and P2-T5 is the first `CMD-BUILD-TEST` after P2-T2; FEATURE/spec.md lines 626 to 654 re-read: `## Acceptance Criteria` 627, AC1 to AC27 at 628 to 654, AC6 633 (pull-request clause), AC26 653 (the three subfolders and the fields `Timestamp:`, `Command:` and `EXIT_CODE:`), AC27 654; `CMD-EVIDENCE-FIELDS` tests exactly those three field names (`@("Timestamp:", "Command:", "EXIT_CODE:")`), and the P0-T1 and P0-T2 task texts write all three; `CMD-RESTORE` is `Copy-Item -LiteralPath $bak -Destination $p -Force` over A (a `pwsh` write to a tracked source path, now enumerated in PD-2); `CMD-COVERAGE-DIRECT` composes `$filter = "TestCategory!=LiveOutlook" + "EXCLUSION"`, which is well-formed when `EXCLUSION` is the empty string; Grep over the plan before the edit for `only pwsh write|only `pwsh` write` (PD-3 and the CMD-DELETE heading only), for `twenty-five|twenty-six|twenty-three|AC-CHECKED` (the Write Set sentence, PD-11, the CMD-SPEC-CHECK payload and expectations, P0-T2, P5-T11, P6-T46, the revision log and the AC27 mapping; only the Write Set sentence lacked the branch (b) state), for `EXCLUSION` (placeholder list, Test-runs convention, the definition, CMD-COVERAGE-DIRECT heading and payload, P0-T9, P0-T11, P6-T7, one CITATION), for `E-AC-CHECKOFF` (the heading, the Phase 6 loop rule and the check-off tasks) and for `^- \[ \] \[P\d+-T\d+\]` (111 tasks); sibling check of PD-9 (Clause A names the P0-T1 and P0-T2 artifacts) against P0-T2's ordering (P0-T1 writes its artifact before P0-T2 records `PRE-EXISTING-EVIDENCE:`), which is why P0-T2 now excludes that artifact from the field.
- Revision 1.3 pass (every citation the round 3 delta touched, re-derived with Read and Grep in this pass, plus its siblings): .gitignore in WORKTREE matched by content for `[Bb]in/`, `[Oo]bj/`, `**/[Pp]ackages/*` (with its `!**/[Pp]ackages/build/` exception), `coverage/*` and `.dotnet*/` (observed at 26, 27, 197, 199, 150 and 357 in this tree; numbers recorded as the observation only and not cited in the plan), the basis of the PD-2 sentence; the plan's own `CMD-EVIDENCE-FIELDS` payload read verbatim: `$files` is `Get-ChildItem -LiteralPath "docs\features\active\2026-10-01-sort-email-latent-logic-defects-959\evidence" -Recurse -File` minus `INHERITED`, so the plan file, spec.md, issue.md and research/ are outside the set and the noncanonical filter `[\\/]evidence[\\/](baseline|regression-testing|qa-gates)[\\/]` admits exactly the three subfolders; the revised field test read against the three label forms it must accept and the one it must reject: `- EXIT_CODE: 0` and `**EXIT_CODE:** 0` begin a line with non-word characters before the label and match `(?m)^[^\w\r\n]*EXIT_CODE:`, while `FORMAT_EXIT_CODE: 0` and `PASS-AFTER-VSTEST_EXIT_CODE: 0` place word characters before the label and do not; the payload remains free of single quotes and of any `$` inside a double-quoted literal, and the new `ForEach-Object` line is one statement; P6-T25's text records `AC7-READ: HOLDS` in the P6-T46 inventory artifact and P6-T46's text lists that line among its contents, which is the mechanism P6-T44 now mirrors; P6-T16's task text runs `CMD-TST-IDENTITY`, `CMD-SWEEP` and `CMD-EVIDENCE-FIELDS` and P6-T46's runs `CMD-SPEC-CHECK`, `CMD-EVIDENCE-FIELDS` and `CMD-SWEEP` (the `CMD-SWEEP` heading now names both); the printed exit labels of every payload re-read in the Command Reference: `CMD-SCOPED-FORMAT` `FORMAT_EXIT_CODE:` and `CHECK_EXIT_CODE:`, `CMD-BUILD-TEST` and `CMD-BUILD-PROD` `MSBUILD_EXIT_CODE:`, `CMD-REBUILD` `MSBUILD_EXIT_CODE:`, `CMD-VSTEST` `VSTEST_EXIT_CODE:`, `CMD-COVERAGE-DIRECT` `COLLECT_EXIT_CODE:`, `CMD-FORMAT-REPO` `FORMAT_EXIT_CODE:`, `CMD-CHECK-REPO` `CHECK_EXIT_CODE:`, the P0-T5 payload `RESTORE_EXIT_CODE:`, and `CMD-HASH`, `CMD-CENSUS`, `CMD-CSPROJ`, `CMD-LINES`, `CMD-DELETE`, `CMD-SPEC-CHECK`, `CMD-SWEEP`, `CMD-EVIDENCE-FIELDS` and `CMD-CONTROL-BACKUP` none (so every `EXIT_CODE:` scope added in this pass names either a printed label or a process exit code); every artifact-writing task of Phases 0 to 6 re-read for its field enumeration (the audit list in the Revision 1.3 log entry); the `PATHS-CITED` paragraph of the Command Reference re-read for the .gitignore exclusion sentence that the revised P0-T3 clause cites; Grep over the plan for `coverage/ directory|write only under|writes only under` (PD-2 only), for `under coverage` (the Test-runs convention's results directory, P6-T16's raw-document rule and the AC26 mapping, none a write-location-only claim), for `AC7-READ` (P6-T25, P6-T46 and the AC7 mapping), for `CMD-SWEEP` and `CMD-EVIDENCE-FIELDS` headings and for `^- \[ \] \[P\d+-T\d+\]` (111 tasks: 12, 12, 11, 6, 15, 12, 43).
- Revision 1.4 pass (every citation the round 4 delta touched, re-derived with Read, Grep and Glob in this pass, plus its siblings): scripts/vscode/Install-RepoDotNetSdk.ps1 exists in WORKTREE (Glob), opens with `[CmdletBinding()]` and `param(` at lines 1 to 2, carries one `throw` at line 103 (`Expected SDK marker ... was not created.`) and no `exit` statement and no `$LASTEXITCODE` use (Grep), so the P0-T4 call `& pwsh -NoProfile -File (Join-Path (Get-Location).Path "scripts\vscode\Install-RepoDotNetSdk.ps1")` is a child pwsh process whose exit code is 1 on that throw and 0 otherwise, which is what `SDK-INSTALL-EXIT:` records; dotnet-tools.json at the repository root lines 5 to 6 (`"csharpier"`, `"version": "1.2.6"`), the basis of P0-T4's tool-list clause (no `.config/dotnet-tools.json` exists in WORKTREE); .claude/skills/evidence-and-timestamp-conventions/SKILL.md in WORKTREE line 121 (the FIRST occurrence of a duplicated field wins in both parsers) and line 122 (the expectation is per file), the basis of the D-3 ordering sentence and of P6-T11's single `ExpectedExitCode:`; FEATURE/spec.md lines 652 to 654 re-read (AC25, AC26 naming the three subfolders and the fields `Timestamp:`, `Command:` and `EXIT_CODE:`, AC27); P0-T4's Part 2 text re-read for its native calls (the install call, `dotnet --version`, `dotnet tool restore`, `dotnet tool list --local`, the two vswhere probes recorded as `MSBUILD-RESOLVED:` and `VSTEST-RESOLVED:`, and `dotnet-coverage --version` with its conditional separate re-run) so that the D-1 label list names exactly the four bracketed calls plus the coverage tool; the `PATHS-CITED` paragraph of the Command Reference re-read: its definition excludes FEATURE/ and .claude/ in the opening sentence and .gitignore in the closing sentences, the three exclusions the revised P0-T3 parenthetical cites; the `CMD-EVIDENCE-FIELDS` payload re-read after the edit: `$anchor` is assigned on the line after `$root` and before `$files`, the per-file predicate and both controls build `$anchor + [regex]::Escape(...)`, `FORMAT_EXIT_CODE: 0` cannot match because `^[^\w\r\n]*` consumes no word character and the label must then start at column 0, and `- **EXIT_CODE:** 0` matches because `- **` are non-word characters; the payload contains no single quote, no `$` inside a double-quoted literal (`$anchor` is a bare variable) and no double-quoted literal ending in a backslash (`"(?m)^[^\w\r\n]*"` ends in `*`); the P6-T11 task text and P0-T9, P0-T11, P1-T6 and P6-T7 re-read for the existing `ExpectedExitCode:` form ("equal to the observed value when it is non-zero"), which P6-T11 now mirrors; the Phase 6 loop rule, the E-AC-CHECKOFF heading (lines 2711 to 2713), P6-T44's repair branch and the AC26 mapping re-read for the check-off write scope; Grep over the plan for `FIELD-CHECK-CONTROL` (the heading, the payload, P6-T16, P6-T44, P6-T46, the AC26 mapping, the revision log and this record) to confirm every gate clause now pairs it with `FIELD-CHECK-POSITIVE`, for `five characters` (the E-AC-CHECKOFF heading and the loop rule only), for `append` (eight appended sections, none ordering schema rows), for `^### Phase \d — ` (7 headings), for `^- \[ \] \[P\d+-T\d+\]` (111 tasks: 12, 12, 11, 6, 15, 12, 43) and for `\r$` (0 lines; the file is LF).
- Revision 1.5 pass (every citation the round 5 delta touched, re-derived with Read and Grep in this pass, plus its siblings): the `CMD-EVIDENCE-FIELDS` payload re-read after the edit: `$canon` is assigned on the line after `FIELD-CHECK-POSITIVE:` and before its first use; the count line and the `NONCANONICAL:` row line apply `.Replace([string][char]92, "/")` to `Substring($root.Length + 1)` before `-notmatch $canon`, the same normalization the `$inherited -notcontains` test of the `$files` line and the `-contains` test of the `EVIDENCE-INHERITED-SKIPPED:` line already apply (sibling: the three path forms therefore agree, and the `NONCANONICAL:` row still prints the unnormalized repository-relative path); the two control lines test the literals `docs\features\active\x\evidence\other\a.md` and `docs\features\active\x\evidence\qa-gates\a.md`; the payload contains no single quote, no `$` inside a double-quoted literal (`$canon` is a bare operand of `-notmatch`), no double-quoted literal ending in a backslash (each path literal ends in `a.md`) and no doubled backslash; the revision 1.3 pass claim below that the filter `[\\/]evidence[\\/](baseline|regression-testing|qa-gates)[\\/]` "admits exactly the three subfolders" is superseded: it described the regex as written, and the filter has now been evaluated through the channel by the preflight round 5 reviewer, where the de-doubled class matched only `/`; the replacement pattern and its two controls are the current claim. Grep over the whole plan for a doubled backslash before the edit: six lines (Verified Repository Facts 13, Listing L-TEF's `ArchiveRootLiteral` line, the two payload lines, and two entries of this record), re-derived as: scripts/vscode/Invoke-MSTestWithCoverage.ps1 line 353 in WORKTREE reads `-notmatch '(^|\\)\.claude\\'` (the prose quotation is accurate and is not a payload); QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs line 35 reads `private const string ArchiveRootLiteral = @"\\mailbox@example.com\Archive";` (Listing L-TEF line mirrors it; written by the Write tool, so no de-doubling applies); the two payload lines replaced. Grep for `[char]92` after the edit: the Command channel convention, `CMD-COVERAGE-DIRECT`, `CMD-COVERAGE-TEXTS`, `CMD-MEMBER-COVERAGE`, the five `CMD-EVIDENCE-FIELDS` path lines and `CMD-SWEEP`, which is the set the convention sentence names. FEATURE/spec.md line 653 re-read: AC26 names the baseline, regression-testing and qa-gates subfolders, the three alternatives of `$canon`. Grep over the plan for `FIELD-CHECK-POSITIVE` after the edit: the heading, the payload, P6-T16, P6-T44, P6-T46, the AC26 mapping, the revision log and this record, and every gate site among them now carries `SUBFOLDER-CHECK-FLAGS-OTHER: True` and `SUBFOLDER-CHECK-FLAGS-QA-GATES: False` beside it; Grep for `SUBFOLDER CHECK NOT DISCRIMINATING` (P6-T16, P6-T44, P6-T46, the revision log and this record); Grep for `^### Phase \d — ` (7 headings), for `^- \[ \] \[P\d+-T\d+\]` (111 tasks: 12, 12, 11, 6, 15, 12, 43), for `^CITATION: ` (39 lines) and for `\r$` (0 lines; the file is LF).
- Revision 1.6 pass (every citation the execution-time amendment touched, re-derived against WORKTREE at the pushed HEAD 24bc24764 with Read, Grep and Glob in this pass, plus its siblings; no shell was available, so git history was read from the worktree's reflog file and the commit SHAs below are observations): UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs on disk lines 160 to 269 (the P4-T4 state after the first P4-T7 format): the excluded parameterless wrapper 161 to 172 passing the `TrySaveAttachmentAsync` method group at 170, the asynchronous core 181 to 211 with `Func<Attachment, string, Task<bool>> trySave` at 187, the destination overload 215 to 223, the one-statement `RedirectSaveFolder` 231 to 237 (the E-A-RR-SECOND anchor, `attachmentHelper.FolderPathSave = destinationPath;` followed by the closing brace, once, and not touched by the delegate insertion), `SaveCaseAsync` from 239 with the second `trySave` at 245; the sibling Edit anchors E-A-CONTROL-MUTATE and E-A-CONTROL-RESTORE (`AttachmentsAltNamePrompt,` directly followed by `RemoveReadOnlyPrompt,` inside the array) do not occur in the inserted block, which contains neither identifier; UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs on disk line 310 and SortEmail_AttachmentSaving_Tests.cs line 404 (`RecordingSave` returning the generic type; a Grep of TSC for `System`-namespace identifiers finds only that `Func`, so `using System;` becomes unused in TSC and is kept, because no `.editorconfig` rule names IDE0005 and the baseline tree already compiles unused directives in S and M); UtilitiesCS/UtilitiesCS.csproj 222 to 224 (`EmbedInteropTypes` True at 223) and UtilitiesCS.Test/UtilitiesCS.Test.csproj 748 to 750 (False at 749); UtilitiesCS/Threading/StoreLockupResponder.cs 20 to 25 (`public delegate void StoreLockupNotifier(` wrapped one parameter per line, the form the new declaration follows) and UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.Transform.cs 23 to 29 (`FolderGroupTransformer<T>` over `FolderWrapper[]`, `int`, `int`, `ProgressTrackerPane`, `CancellationToken`); the Grep results recorded in Verified Repository Facts 17 (the eight delegate declarations; the generic forms over interop types in UtilitiesCS and UtilitiesCS.Test; `TrySaveAttachmentDelegate` zero hits in the worktree); FEATURE/evidence/qa-gates/p4-t7-format-and-build.2026-10-03T10-21.md (sixteen CS1769 at the nine TSC and seven TAS call sites, `ERROR_CODES: CS1769`, `DLL_ADVANCED: False`, the cause paragraph) and FEATURE/evidence/qa-gates/p4-t5-legacy-deletion.2026-10-03T10-17.md (`ITERATION: 2`, the channel note, the coordinator-run output block, the HEAD at which the payload ran), the two forms P4-T7 and P5-T10 now cite; FEATURE/evidence/qa-gates/p1-t5-test-build, p1-t10-format-and-build, p2-t5-test-build, p2-t9-format-and-build, p3-t2-format-and-build and p3-t5-format-and-build (a green `CMD-BUILD-TEST` prints `ERROR_CODES:` with an empty value, transcribed `(none)`, so P4-T7 gates `ERROR_LINES: 0` and names `ERROR_CODES:` values only as stop vocabulary); the worktree reflog (logs/HEAD under the main checkout's .git/worktrees entry for this worktree) for the execution commits: Phase 0 at 96ef42d0, Phase 1 at 7a0ff650, Phase 2 at b7efccf0, Phase 3 at 542c2ba0, P4-T1 to P4-T4 at ef0cb65a, P4-T5 at 1a1b84a3, P4-T6 at d7cb99e5 and the P4-T7 stop at 24bc2476, each message in the `wip(959): <task> <summary>` form with `(compile-red span open)` where applicable, the basis of the commit rule's description of the executed history; FEATURE/spec.md D5 at 132 and the Data / API list at 195 to 196 (the `Func<Attachment, string, Task<bool>>` spelling), AC8 at 635 ("exact delegate types in Technical specifications") and AC9 at 636 (recording `trySave` delegates, no type named), the basis of PD-14's finding that no AC text changes; the plan's own regions re-read after the edits: Listing L-TSC-FINAL and Listing L-TAS-FINAL each carry `private static SortEmail.TrySaveAttachmentDelegate RecordingSave(List<string> saves)` once, Listing L-A-FINAL carries the inserted block between the parameterless wrapper and the asynchronous core and the two `TrySaveAttachmentDelegate trySave` parameters (Grep for `Func<Attachment, string, Task<bool>> trySave` over the plan: no listing line remains; the spaced form survives only in prose), TOKENS-A with the SEAMED column (every pre-existing row carries SEAMED equal to EXTRACT except the retyped seam row), TOKENS-TSC and TOKENS-TAS with the two added rows, the Execution Conventions commit bullet and compile-red bullet, P4-T7, P4-T10, P4-T14, P4-T15, P5-T3, P5-T4, P5-T5, P5-T9, P5-T10, P5-T11, P5-T12, P6-T18 and P6-T26; the sweep of every remaining task P4-T7 to P6-T46 for `git status`, `porcelain`, `git diff` and `numstat` (hits: P5-T3, P5-T5, P5-T9, P5-T10, P5-T11; P6-T1 `PORCELAIN-SAME`, a before-and-after comparison inside one payload; P6-T12 `CMD-FOOTPRINT`, working-tree diffs against `MERGE-BASE` plus untracked porcelain; P6-T16 `CMD-TST-IDENTITY`, working-tree diffs against `MERGE-BASE` with `TST1-PORCELAIN-LINES:` recorded, not gated; P6-T18 `git rev-parse HEAD`, an observation; the P4-T13 and P4-T15 payloads `CMD-CONTROL-BACKUP` and `CMD-RESTORE` run no git command); Grep for `^### Phase \d — ` (7 headings), for `^- \[[ x]\] \[P\d+-T\d+\]` (111 tasks: 12, 12, 11, 6, 15, 12, 43; 47 checked), for `^CITATION: ` (43 lines) and for `\r$` (0 lines; the file is LF).
- Revision 1.7 pass (every line the six-delta round touched, re-derived with Read and Grep in this pass, plus its siblings; no shell was available, so the HEAD b0720285a is the orchestrator's statement and no git fact is asserted): FEATURE/spec.md before the edit: 681 lines, Grep `\r` 0 lines (LF), Grep `Func.Attachment` exactly the seven hits D-1 names (line 132 twice, 195, 196, 264, 285, 457) and `TrySaveAttachmentDelegate` 0 lines; each of the seven old texts and the line 12 old text read verbatim before replacement; after the edit: 681 lines, `Func.Attachment` 0 lines, `TrySaveAttachmentDelegate` 7 lines (12, 132, 195, 196, 264, 285, 457), so the P6-T26 gate (zero and at least six) is satisfiable and is not vacuous (it read 7 and 0 before the edit, the failing state); siblings re-read: line 132's second sentence still names the two-argument `TrySaveAttachmentAsync` overload as the only converting one (true of the nested delegate, PD-14), line 457's method-group claim is unchanged apart from the type name, lines 264 and 285 keep their four-space indent inside the two signature blocks, lines 194 and 197 (the `SaveCase` and synchronous entries) are untouched, line 635 (AC8) still reads "exact delegate types in Technical specifications" and lines 627 to 654 keep their line numbers, so the header's AC line citations hold; plan PD-14 after the edit: the sentence now states the in-place correction, and its preceding clause ("spell the seam as `Func<Attachment, string, Task<bool>>`") describes the spec as it stood before this correction, in narrative order, and was left as the reviewer wrote it; the Execution Conventions commit bullet re-read: the new exemption sits between the stop-record sentence and the two P4-T14 and P4-T15 exceptions, and P5-T10's task text now states both the no-artifact, no-commit handoff and the `HEAD-AT-HANDOFF:` equality that `DELETE CHANNEL REFUSED` enforces, which the bullet's exemption matches; the artifact-filenames bullet re-read: FEATURE/evidence/qa-gates holds p4-t5-legacy-deletion.2026-10-03T09-01.md and p4-t7-format-and-build.2026-10-03T10-21.md with no `ITERATION:` row and p4-t5-legacy-deletion.2026-10-03T10-17.md with `ITERATION: 2` at its line 6 (Glob and Grep), so the iteration-1 default resolves the P4-T5 glob today and will resolve the P4-T7 glob once its `ITERATION: 2` re-run is written; the Census Expectations A line re-read against the on-disk A (orchestrator-verified 315 lines) and Listing L-A-FINAL's twelve declaration lines plus one blank separator line (328); L-TSC-FINAL and L-TAS-FINAL headings re-read against their single `private static SortEmail.TrySaveAttachmentDelegate RecordingSave(List<string> saves)` lines; the AC8 mapping re-read: TESTS now names the P6-T26 spec Grep and the EVIDENCE field is unchanged; Grep over the plan after the edits for `about 342` and for `spec.md is not edited` (each exactly one line, this record's own quotation, and none elsewhere), `HEAD-AT-HANDOFF` (P5-T10 twice, the revision 1.7 log and this record), `TECHNICAL SPECIFICATIONS TYPE DIFFERS` (P6-T26, the revision 1.7 log and this record), `^### Phase \d — ` (7 headings), `^- \[[ x]\] \[P\d+-T\d+\]` (111 tasks: 12, 12, 11, 6, 15, 12, 43; 47 checked), `^CITATION: ` (43 lines; the spec.md citation's locator now names the seven corrected lines) and `\r$` (0 lines; the file is LF).
- Revision 1.8 pass (every line the P4-T9 amendment touched, re-derived with Read and Grep in this pass, plus its siblings; no shell was available, so HEAD ce3e73701 is the orchestrator's statement and no git fact is asserted): UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs on disk (the P4-T7 rewrite): `OriginFolder` 25 and `DestinationFolder` 26 share the twenty-three-character prefix ending in the sandbox root's separator, so the first differing index of the two directory strings is 23 and deterministic; the re-rooting test 305 to 320 with its four assertions in order at 316 (primary directory), 317 (alternate directory), 318 (primary name) and 319 (alternate name) and no assertion scope, so only the first failing assertion's message is printed, and under the extraction state (primary re-rooted, alternate not) that is line 317, whose subject text `Path.GetDirectoryName(helper.FilePathSaveAlt)` is the first token's source; sibling: the structural test's assertion order 345 to 354 (`sessionFields.Should().HaveCount(4)` 347 before `resetTargets.Should().HaveCount(4)` 348, then `OnlyHaveUniqueItems` 349 and the `Contain` loop 350 to 354), the basis of the P4-T14 `but found 3` disposition. FEATURE/evidence/regression-testing/fail-before-redirect-save-folder.md (the 2026-10-03T11-28 stop record, full read, 46 lines): schema rows 3 to 6 (`EXIT_CODE: 1`, `ExpectedExitCode: 1`), `COUNTERS total=11 executed=11 passed=10 failed=1` 16, the eleven `RESULT` rows 18 to 28, the `MESSAGE` entry 29 to 33 (the opening line naming `Path.GetDirectoryName(helper.FilePathSaveAlt)` and `differs at index 23`, the actual line ending `origin"`, the expected line ending `destination"`), the U+2026 note 36, acceptance items 40 to 44 (item 4 `NOT MET`), stop label 46; the record carries no account, profile-leaf, machine or users-path token (the `CMD-SWEEP` classes) and its three schema labels are line-leading (the `CMD-EVIDENCE-FIELDS` anchor), so even unrewritten it would trip neither P6-T16 count, and the amended P4-T9 rewrites it. Listing L-TEF in this plan 1189 to 1202, 1209 to 1221 and 1228 to 1240: the throwing test awaits `ThrowAsync` 1201 then asserts `probe.ResetCalls.Should().Be(1)` 1202 (an integer `Be`, whose `but found 0` message is not a string difference and is not truncated), the basis of the P5-T7 disposition; `ResetCalls.Should().Be(1)` twice and `Be(0)` once match TOKENS-TEF. Plan sweep after the edits: Grep `MESSAGE` over the plan hits the conventions (89, 2745, 2747), the `CMD-VSTEST` payload and note (3190, 3192), the executed gates P0-T9, P1-T6, P1-T7, P2-T6, P3-T3 and P3-T6, and the remaining gates P4-T9 (amended), P4-T14, P5-T7 and P6-T17; Grep `but found|differs at index` hits the table rows, P1-T7, P2-T6, P4-T9, P4-T14, P5-T7, P6-T28 and the AC10 and AC18 mappings, none of which asserts a long-string-difference window or a backslash literal apart from the two replaced; Grep `Sortemail959Sandbox` after the edits hits the hygiene convention, the listing constants, the census tokens, the payload sandbox probes and the P6-T20 census value, and no acceptance clause; FEATURE/spec.md AC11 638 ("observed failing on the alternate-path assertion"), AC20 647 and AC26 653 (non-zero EXIT_CODE and the failing method named) and Test Strategy row 583 ("alternate-path directory still equals the origin folder") name no message substring, so no spec line changed; Grep for `^### Phase \d — ` (7 headings), `^- \[[ x]\] \[P\d+-T\d+\]` (111 tasks: 12, 12, 11, 6, 15, 12, 43; 49 checked), `^CITATION: ` (45 lines) and `\r` (0 lines; the file is LF).
- Revision 1.8 delta pass (preflight round on the revision 1.8 amendment, one defect applied verbatim as two text replacements; no shell was available, so HEAD ed94f0af2 is the orchestrator's statement and no git fact is asserted): the Stop discipline bullet of the Execution Conventions re-read after the edit (it now names the toolchain-loop restart and the re-run of a stopped task that its planner-amended task text directs as the only repetitions, and states that such a re-run executes the unchanged test and command against the amended acceptance text and is not a re-run to obtain a different outcome); the revision 1.8 log entry re-read after the edit (the inserted sentence sits immediately before `Task count unchanged at 111`); siblings re-read against the new wording: P4-T9 (unchanged; its parenthetical "the planner-amended re-run the Stop discipline admits, as P4-T7's was" now has a referent in the convention), P4-T7 (unchanged; its re-run followed the revision 1.6 amendment of its own task text after the `P4-T7 BUILD RED (CS1769)` stop, `ITERATION: 2`, each stop code "resolved only by a planner amendment", which is the admitted form), the artifact-filenames convention ("when a task was re-run after a stop record, as P4-T5 and P4-T7 were"; the iteration rule it states is the mechanism the admitted re-run uses), the per-task commit bullet (a stopped task is committed with its stop record; the admitted re-run's own commit follows the same rule) and the Phase 6 loop rule (the toolchain-loop restart the bullet names); Grep `only repetition|repetitions this plan` over the plan: line 90 only, so no other convention or task restates the rule; Grep `re-running this task|sweep re-run`: the P6-T16 and P6-T46 repair branches (a hygiene substitution on a named artifact followed by the same task's sweep payload inside an unstopped task; the convention now lists them as the repair-branch re-runs it admits, and the branches themselves are unchanged since revision 1.0); Grep for `^### Phase \d — ` (7 headings), `^- \[[ x]\] \[P\d+-T\d+\]` (111 tasks: 12, 12, 11, 6, 15, 12, 43; 49 checked), `^CITATION: ` (45 lines) and `\r` (0 lines; the file is LF).
- Revision 1.8 second delta pass (confirming round on the revision 1.8 amendment, two defects in the Stop discipline bullet applied verbatim as three text replacements; no shell was available, so HEAD 5f7b1e6fd is the orchestrator's statement and no git fact is asserted): the Stop discipline bullet re-read in full after the edit (it names three admitted repetitions: the toolchain-loop restart, the re-run of a stopped task that its planner-amended task text directs, and the re-run a task's own repair branch directs after the named repair in P6-T16 and P6-T46; it describes the stopped-task re-run per task, P4-T7 as the amended listings, format and build and P4-T9 as the unchanged test and command against the amended acceptance text; and it ends with a period, as the surrounding Execution Conventions bullets do); the revision 1.8 log sentence re-read after the edit (it now names the per-task descriptions and the two repair-branch re-runs and sits immediately before `Task count unchanged at 111`); siblings re-read against the new wording: P4-T7 (unchanged; its revision 1.6 amendment rewrote Listings L-A-FINAL, L-TSC-FINAL and L-TAS-FINAL and its `ITERATION: 2` re-run executed the amended listings, the scoped format and the build, which is the per-task description the bullet now gives), P4-T9 (unchanged; its re-run executes the unchanged test and command against the amended acceptance text, as the bullet now states per task, and its parenthetical "as P4-T7's was" now holds because the bullet admits P4-T7's form), P6-T16 and P6-T46 (unchanged; Grep `re-running this task|sweep re-run` hits only their two repair branches, each a named hygiene substitution followed by the same task's sweep payload, which is the repair-branch re-run the bullet now admits); Grep `only repetitions|repetitions this plan` over the plan: line 90 and the two self-review records that quote the pattern (the preceding "Revision 1.8 delta pass" bullet and this one), so no other convention or task restates the rule; the preceding "Revision 1.8 delta pass" bullet corrected in one phrase only (its description of the two repair branches as outside the convention replaced by the statement that the convention now lists them); Grep for `^### Phase \d — ` (7 headings), `^- \[[ x]\] \[P\d+-T\d+\]` (111 tasks: 12, 12, 11, 6, 15, 12, 43; 49 checked), `^CITATION: ` (45 lines) and `\r` (0 lines; the file is LF).
- Revision 1.9 pass (every line the widening touched, re-derived with Read, Grep and Glob against WORKTREE at HEAD 9de3f176d, the orchestrator's statement, plus its siblings; no shell was available, so no git fact is asserted): UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs on disk lines 140 to 209 read verbatim: the summary element 170 to 173 (171 `/// Verifies that Cleanup_Files resets all static YesNoToAllResponse tracking fields`, 172 `/// without throwing, covering the state-reset method used between sort sessions.`), `[TestMethod]` 174, `Cleanup_Files_DoesNotThrow` 175 to 180 with `SortEmail.Cleanup_Files()` 178, the data-driven `GetAttachmentsInfo` test from 182 (the P4-T6 rows with `DisplayName` at 187, 193 and 199), so the E-TST1-DOC-CLEANUP OLD block (171 to 172) is nine lines above the nearest P4-T6 edit and unique (Grep `tracking fields` one line, 171; Grep `AllPromptSessions|prompt session` zero lines; Grep `Cleanup_Files` lines 127, 171, 175 and 178); the `#region` header 127; the thirteen TOKENS-TST1 tokens checked against the OLD and NEW texts with whitespace removed (none is a substring of either, so TOKENS-TST1 FINAL holds after the edit); the NEW block's two lines are 96 and 99 characters including the eight-space indent (the formatter does not reflow comments, so no wrap moves the asserted token off its line). UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs on disk lines 1 to 60: the comment 15 to 17, the three sessions 18 to 26, the `AllPromptSessions` comment 28 to 30 and property 31 to 38 (four elements: `AttachmentsOverwritePrompt`, `PicturesOverwritePrompt`, `AttachmentsAltNamePrompt`, `RemoveReadOnlyPrompt`), `Cleanup_Files` 40 to 46 (`foreach (var prompt in AllPromptSessions) { prompt.Reset(); }`); Grep `YesNoToAllResponse` over the file: the parameter and `case` uses at 254 to 303 only, no field (the basis of PD-15's statement that no `_attachmentsAltName` field remains and that the alternate-name state is reset through its session); UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs lines 1 to 30 (`RemoveReadOnlyPrompt` 15 to 17 with the comment 12 to 14). Grep over UtilitiesCS.Test/EmailIntelligence/SortEmail_*.cs for `tracking fields|static YesNoToAllResponse|_attachmentsAltName|_responseSaveFile`: TST1 171 only (sibling check: no other test file carries the same drift). FEATURE/spec.md lines 626 to 655 re-read: AC5 632 (the two TST1 pins), AC7 634 (`foreach` over `AllPromptSessions`), AC14 641 ("`SanitizeArrayLineTSV`, `StripTabsCrLf` and their tests are unchanged"), AC22 649 (TST1 listed), AC23 650, AC24 651 ("one final pass"), AC26 653; Grep `AllPromptSessions|Cleanup_Files|D5\b` over FEATURE/spec.md: D5 at 132, the Data / API entry at 199 (the four fields deleted after phase one), the property and `foreach` body at 239 to 244, the phase-one and structural tests at 579 and 584; no acceptance-criterion text changed. FEATURE/evidence/qa-gates on disk (Glob `p6-t1*.md`): p6-t1-csharpier-format.2026-10-03T12-34.md, p6-t10-post-format-census.2026-10-03T12-52.md (`ITERATION: 1` at 4; the thirteen TOKENS-TST1 totals 175 to 187 equal to the FINAL column; `LINES ... SortEmail_Tests.cs = 488` at 188 and 385; `SHA256` 189), p6-t12-scope-boundary.2026-10-03T12-55.md (`ITERATION: 1` at 4; `ANCHOR-RECHECK:` 9; `OUTSIDE-WRITE-SET: 0` 15; `WRITE-SET-MISSING: 0` 16; the seven numstat rows 19 to 25, none for TST1; eight clauses met 32 to 39) and p6-t13-identity-and-sweep.2026-10-03T12-56.md (the HOOK BLOCK stop record of the task now numbered P6-T16: `Timestamp:` 3, `ITERATION: 1` 4, `Command:` 5, `EXIT_CODE: NOT-RUN` 6, the hook text 11, the cause observation 13 naming `$removed1` and the `TST1-REMOVED-*` labels, CMD-SWEEP all-zero with `FILES: 80` 22 to 28; the file carries no account, profile-leaf, machine or users-path token and its three schema labels are line-leading, so it trips neither sweep nor field count). The plan's own regions re-read after the edits: the renumbering (89 lines matched `P6-T(1[3-9]|[2-4][0-9])` before it and 89 lines matched `P6-T(1[6-9]|[2-4][0-9])` immediately after it and before the revision 1.9 insertions, with no `P6-T13`, `P6-T14` or `P6-T15` reference surviving from before and no `P6-T47` or higher; the thirteen lowercase artifact-name occurrences then read p6-t16, p6-t24, p6-t45 and p6-t46, and the only `p6-t13-` occurrences after the insertions are the orphan stop-record name and the new p6-t13-tst1-doc-comment artifact), the task sequence P6-T1 to P6-T46 gap-free by a Grep of the task lines, the `CMD-TST-IDENTITY` payload (three added lines after `TST1-REMOVED-TRYSAVE-LINES:`, no single quote, no backslash, no `$` inside a double-quoted literal, every string double-quoted), the P6-T13 to P6-T16 texts, PD-15, Verified Repository Facts 18, the two conventions, the loop rule, Edit E-TST1-DOC-CLEANUP (its OLD lines equal TST1 171 to 172 with the four-space listing prefix), the AC22, AC23 and AC24 mappings; Grep for `^### Phase \d — ` (7 headings), `^- \[[ x]\] \[P\d+-T\d+\]` (114 tasks: 12, 12, 11, 6, 15, 12, 46; 80 checked), `^CITATION: ` (48 lines) and `\r` (0 lines; the file is LF).
- Revision 2.0 pass (every line the Phase 7 and Phase 8 append touched, re-derived with Read, Grep and Glob against WORKTREE at HEAD eb7871502, plus its siblings; no shell was available, so no git fact is asserted beyond what the ref and log files show): the worktree's branch ref file `.git/worktrees/agent-ae9faf6e1bf21ac17` HEAD pointing at refs/heads/bug/sort-email-latent-logic-defects-959, the loose ref reading eb7871502101c896e78f0b665cd943126efa40db, reflog line 106 `0fab75ed6 -> eb7871502 commit: docs(959): initial feature review artifacts` and zero `merge` entries in the reflog (Grep), the basis of the header's HEAD statement and of P8-T1's `MERGE-BASE-BEFORE` expectation; the main checkout's packed-refs `refs/remotes/origin/main` f8ea1b5dcc (no loose `refs/remotes/origin/main` file exists, Glob), recorded as an observation only. UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs lines 100 to 160 read verbatim: the excluded wrapper 111 to 122, the summary 124 to 129, the core signature 130 to 135, the absent-file branch 137 to 141, line 143 `var overwritePrompt = attachmentHelper.AttachmentInfo.IsImage`, 144 `? picturesOverwritePrompt`, 145 `: attachmentsOverwritePrompt;`, `Ask` 146 to 148 with the overwrite text, `SaveCase` 149 to 154, `ReleaseSingleAnswer` 155, brace 156 (sibling: the asynchronous core's selection at 209 is pinned by AS2 and AS3, which is why the CR-1 remedy is one synchronous test). coverage/final-959.cobertura.xml on disk (Glob of coverage/*959* lists baseline and final cobertura, jacoco and trx documents and effective-coverage-959.config): the single class node for the file at document line 59687, the `SaveAttachment` method node with `branch-rate="0.75"` at 59791, `<line number="143" hits="1" branch="True" condition-coverage="50% (1/2)">` at 59802 with `<condition number="0" type="jump" coverage="50%" />`, every other `branch="True"` line of the class at `100%`, and the class-level `<lines>` block from 60001; scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 lines 350 to 354 (the merge copies `condition-coverage` from the node with the larger denominator), so the attribute survives `ConvertTo-KoverageCoberturaXml`, which is why `CMD-LINE-CONDITION` reads the post-processed document; the `CMD-LINE-CONDITION` payload read against those nodes (`//class` filtered by `filename`, `lines/line` filtered by `number`, `methods/method` filtered by `name`), its words scanned for the hook rule (no `git`, `remove`, `gh`, `merge`, `create`, `edit`, `issue` or `new`; `GetAttribute`, `SelectNodes`, `foreach` and `Encoding` contain none), no single quote, no `$` inside a double-quoted literal, no doubled backslash (the filename is built with `[char]92`). UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs read in full (437 lines; Read shows a trailing empty row 438, so the file ends with a newline): `using System;` 1 with `DateTime` at 27 and `Func<string, bool>` at 391 (so TAS keeps its directive); AS2 at 62 to 92 (`photo.jpg`, `Exists(true, new List<string>())`, pictures scripted `Yes`, the three assertions SS4 mirrors); SS1 to SS3 at 220 to 297, all `report.pdf`; SS3's last assertion at 296 and brace at 297, blank 298, `/// <summary>` 299, the RR summary line 300 (Grep `Be\(YesNoToAllResponse\.NoToAll\)` one line, `RR\. Scenario` one line, so the E-TAS-SS4 OLD block is unique); eleven `[TestMethod]` (lines 33, 67, 98, 129, 156, 194, 224, 251, 277, 304, 328); `CreateAttachmentMock("photo.jpg")` at 37, 71 and 308 (the P7-T2 count of 4 after the Edit); the helpers `CreateAttachmentMock` 357 to 370 (a Loose mock with `SaveAsFile` unset, so `Verify(..., Times.Never)` is meaningful), `CreateHelper` 372, `OverwritePrompt` 377 to 380 (the text SS4 asserts), `Exists` 391 to 398, `ScriptedPrompt` 417 to 435 (an unscripted session throws from the empty queue, which is how SS4 proves the attachments session was not asked); the SS4 name 0 hits in the worktree (Grep over the plan before this revision and over the source tree); the NEW block of E-TAS-SS4 checked line by line for the four-space listing prefix, for CSharpier shape (signature line 94 characters at eight-space indent, every statement under 100 characters, the `SortEmail.SaveAttachment(` call broken one argument per line as SS1 to SS3 are) and for TOKENS-TAS tokens (only `[TestMethod]`, `SortEmail.SaveAttachment(` and the SS4 name occur; `SaveAsFiletotheprimarypath` does not contain `File.`). UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs lines 1 to 40 read and the whole file searched: `^using System;` one line (line 1), the eighteen PD-16 `System` alternatives zero lines, `^` 343 lines, the constants 22 to 25 and the first `[DataTestMethod]` at 31 (siblings unaffected by dropping line 1). QuickFiler.Test/QuickFiler.Test.csproj at HEAD (Grep with line numbers): ArchiveRoot 127, FilerCleanup 128, UiThreadDispatcherFixtureTests 204, InitializationTests 205, DedicatedWorkerThread 229, WinFormsPumpHostTests 230, no PinCount, SynchronousBackgroundWorker or ArmingFakeTimeProvider line; the session checkout's copy (which carries main as of its last merge) likewise has none, so the three lines post-date that merge; the sibling worktree agent-a291a7fbabf9d0229 (Glob for `**/QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs` under the main checkout's `.claude/worktrees/` returns that worktree only) reads PinCount at 204 between FixtureTests 203 and InitializationTests 205, and SynchronousBackgroundWorker 230, ArmingFakeTimeProvider 231 between DedicatedWorkerThread 229 and WinFormsPumpHostTests 232, with ArchiveRoot at 127 and no FilerCleanup line, the main-side shape PD-17 states; the `CMD-FANIN` payload scanned word by word for the hook rule (`git` and the expanded `WORKTREE` path are present, `remove`, `gh`, `merge`, `create`, `edit`, `issue` and `new` are absent; `promoted` supplies `pr` with no `gh`), for a single quote (none; the own-path test is a script block as `CMD-SWEEP` uses), for `$` inside a double-quoted literal (none; `$base` is a bare operand) and for a doubled backslash (none; `-split "\t"` is the form `CMD-FOOTPRINT` already uses). .claude/hooks (Grep `-SubcommandPath @(`): enforce-epic-worktree-removal-gate.ps1 171, 172, 380 and enforce-parallel-worktree-removal-gate.ps1 129, 130, 282 (`worktree`, `remove`), enforce-epic-merge-gate.ps1 178, 184, 386, 387 (`pr`, `merge`), enforce-pr-author-skill-helpers.ps1 279 (`pr`, `create`) and 280 (`pr`, `edit`), enforce-promotion-mcp-only.ps1 126 (`issue`, `create`) and 127 (`issue`, `new`), validate-bash.ps1 123 (`push`) and 127 (`reset`); the refusal mechanism for a `pwsh -Command` segment is the one the P6-T16 record and the revision 1.9 log describe, and the executed Phase 6 payloads confirm it by contrast (`CMD-VSTEST` and `CMD-COVERAGE-DIRECT` carry `Remove-Item` but no `git` and ran; `CMD-FOOTPRINT` carries `git` but no `remove` and ran; `CMD-TST-IDENTITY` carries both and was refused). The evidence artifacts read for the figures Phase 7 and 8 transcribe or gate: coverage-post-change.md lines 1 to 50 (ITERATION 1, 85.39, 79.81, `FINAL-UCS-BRANCH: 9495/11356`, `Total 7393`), toolchain-final-pass.md 1 to 26, coverage-comparison.md 1 to 98 (`BASELINE-HASH` 408A19E9..., the AttachmentSaving row 142/142, `MEMBER SaveAttachmentCore ... percent=100`, `EXEMPT-LINES: 36,133,190,191`), pass-after-regression-tests.md 1 to 20 (the runsettings hash, `COUNTERS total=55`), p6-t12-scope-boundary.2026-10-06T13-23.md (27 and 26 subtracted, the seven numstat rows P7-T15 expects unchanged), p6-t16-identity-and-sweep.2026-10-06T15-11.md (`FILES: 84`, 78 checked, 1 inherited skipped), p6-t46-ac-inventory.2026-10-06T15-23.md lines 1 to 40 (`AC-CHECKED 25`, 89 files), p0-t3-worktree-context.2026-10-03T08-24.md (the 27 Clause A paths, `MERGE-BASE` 94287369), p6-t1-csharpier-format.2026-10-03T12-34.md (`Formatted 1639 files`, the recorded-not-gated summary line); the three review artifacts read in full (code-review CR-1 to CR-7 and U-1 to U-3; policy-audit G-1 to G-7 and the coverage tables; feature-audit AC inventory and check-off section). FEATURE/spec.md lines 627 to 654 re-read: AC9 (636) requires the files to contain the named tests, which an added test does not break; AC22 (649) lists TSC and TAS; AC23 (650) names TAS individually; AC24 (651) and AC25 (652) name the fixed-name projections that Phases 7 and 8 rewrite with `ITERATION:` rows; no acceptance-criterion text is changed by this revision. The plan's own regions after the edits: Grep `^### Phase \d — ` 9 headings; `^- \[[ x]\] \[P\d+-T\d+\]` 143 tasks (12, 12, 11, 6, 15, 12, 46, 16, 13; 114 checked); the P7 and P8 task sequences gap-free (P7-T1 to P7-T16, P8-T1 to P8-T13); `^CITATION: ` 66 lines; `\r` 0 lines (the file is LF, and the Edit tool preserved that); every evidence path of Phases 7 and 8 under baseline/, regression-testing/ or qa-gates/ (Grep `evidence/other` over the new text: zero task-path hits); every `git diff` of the two new payloads carries a ref operand and sits beside a porcelain count (G8, G8b); the five Grep-tool patterns of P8-T2 are backslash-free regular expressions (a `.` stands for the path separator), so the contract's rule against a fixed-string search literal containing a backslash is not engaged, and no new line of this revision carries a doubled backslash (Grep over the plan for a doubled backslash after the edits: seven lines, every one a pre-existing quotation in the Command-channel convention, Verified Repository Facts 13, Listing L-TEF, the revision 1.5 log entry and the earlier self-review entries; none in PD-16, PD-17, the new conventions, the new payloads, Phases 7 or 8, the revision 2.0 log entry or this bullet).
- Revision 2.0 delta pass (preflight round 13 on revision 2.0, five defects D1 to D5 applied verbatim as fifteen text replacements plus one sibling phrase; no shell was available, so HEAD 66b444507, origin/main f8ea1b5dcc and the two MEMORY.md hunks after base lines 101 and 137 are the preflight reviewer's git observations, attributed as such in PD-17, and no git fact is asserted by the planner): FEATURE/evidence/qa-gates/coverage-comparison.md lines 8 to 13 re-read (`ITERATION: 1` at 4; `SORTEMAIL-CLASS final` rows SortEmail.cs `valid=5 covered=5 uncovered=0` 8, SortEmail.AttachmentSaving.cs `valid=142 covered=142 uncovered=0` 9, SortEmail.TrySaveAttachment.cs `valid=93 covered=89 uncovered=4` 10, SortEmail.UndoAndMoveLog.cs `valid=45 covered=45 uncovered=0` 11, SortEmail.MailItemSort.cs `valid=1 covered=0 uncovered=1` 12, `SORTEMAIL-AGG final valid=286 covered=281 uncovered=5` 13), the five class values and the aggregate the amended P7-T12 clause quotes; FEATURE/evidence/qa-gates/coverage-post-change.md lines 33 to 36 (`FIRST-PARTY-LINE-PERCENT: 85.39`, `FIRST-PARTY-BRANCH-PERCENT: 79.81`), the recorded-not-gated comparison figures; the `CMD-FANIN` payload re-read after the edit: `$paths` is assigned at its fourth line from the `ORIGIN-MAIN-SHA HEAD` name-status rows and `$mainOnly` at its twentieth from the `$base ORIGIN-MAIN-SHA` name-only rows, so the delta's variable names are the payload's own and no substitution was needed; the five inserted lines sit between `CONTROL-OUTSIDE-IF-UNFILTERED:` and `PORCELAIN-COUNT:` with the four-space listing prefix, each `git diff` carries the `ORIGIN-MAIN-SHA HEAD` ref operands and the task still pairs them with `PORCELAIN-COUNT:` (G8, G8b), the predicate `-notmatch "^[0-9]+\t0\t"` reads a numstat row whose second column is 0 as additions-only (a tab is written `\t` inside a double-quoted literal, the form the payload's `-split "\t"` already uses; no doubled backslash, no single quote, no `$` inside a double-quoted literal), and the words of the five lines (`shared`, `SHARED-PATHS`, `SHARED-NUMSTAT`, `SHARED-WITH-LOSS`, `LOSS-CHECK-CONTROL`, `foreach`, `Where-Object`, `ccontains`, `notmatch`, `join`, `numstat`) contain no `remove`, `gh`, `merge`, `create`, `edit`, `issue` or `new`; the `CMD-FANIN` note re-read after the append (its "one overlapping file" phrase narrowed to the csproj overlap, because `MAIN-ONLY-HAS-QFT-CSPROJ:` tests that one path and PD-17 now names two); P8-T3 re-counted after the insertion: nine semicolon-separated clauses (ancestry pair; reversed control; `MERGE-BASE-AFTER:`; `FANIN-OUTSIDE:`; `FANIN-WRITE-SET-MISSING:` with `FANIN-DELETED:`; the three `NUMSTAT-*-VS-MAIN:` values; the three committed-range controls; the `SHARED-*` clause; `MAIN-TOUCHED-WRITE-SET-CODE:` with `PORCELAIN-COUNT:`), matching "all nine required"; P7-T1 re-counted: six clauses (the three A node values; the condition and branch-rate pair; the three A line texts; the five TAS counts; the two TSC counts; `TSC-SYSTEM-IDENTIFIER-LINES:` recorded), matching "all six required"; P7-T12 re-counted after the replacement: the nine P6-T8 clauses plus the family-identity clause, "all ten required" unchanged, and its earlier sentence still names `PHASE6-LINE-NOT-LOWER:` and `PHASE6-BRANCH-NOT-LOWER:` as recorded rows, which the amended clause now treats as observations; P7-T16's `FILES:` parenthetical (89 plus 1 plus 3 plus 10) sums to 103 and P8-T13's (103 plus 1 plus 8) to 112, and the FEATURE folder on disk holds 93 files by Glob (the 89, the P6-T46 artifact and the three review artifacts), the lower bound the P7-T16 breakdown starts from; the Phase 8 phase rule re-read after the edit against P8-T2's conflict branch (the csproj union resolution with the Edit tool is the one Write Set write it admits) and against PD-17's abort rule; the revision 2.0 log entry re-read (the appended sentence names D1 to D5 and the sibling phrase and sits after "Phase 8 adds no path of its own)"); the AC25 mapping re-read (its Phase 7 clause now names the family rows as the gate and the first-party figures as recorded, matching P7-T12); the AC22 mapping's `p8-t3-fanin-gate` entry re-read (own paths only and `NUMSTAT-UCT-VS-MAIN 3 0`, both still printed, unchanged); Grep over the plan after the edits for `PHASE 7 RATE LOWER THAN PHASE 6`, `one shared file|one known overlap|one overlapping` and `at least 100 \(|at least 108 \(` (no task, convention, payload, phase-rule or mapping line; the only hits are the revision 2.0 log entry's quotation of the narrowed phrase and this record's own pattern text), `SHARED-WITH-LOSS` (PD-17, the payload twice, the note, P8-T3, the log entry and this record), `^### Phase \d — ` (9 headings), `^- \[[ x]\] \[P\d+-T\d+\]` (143 tasks: 12, 12, 11, 6, 15, 12, 46, 16, 13; 114 checked), `^CITATION: ` (66 lines; the coverage-comparison.md locator gains the family rows) and `\r` (0 lines; the file is LF).
- Corrections made to this plan's own earlier chunks during the completion pass: P0-T12's line counts restated in the `Get-Content` convention only; `CMD-TRX-SUMMARY` (unused, carrying an unlisted placeholder) replaced by `CMD-EVIDENCE-FIELDS`, which P6-T16 names; P6-T10's `EFCC-MEMBERS:` note corrected (the `using System.Diagnostics.CodeAnalysis;` directive does not contain the pattern text); P0-T1 now records `Command:` and `EXIT_CODE:` so that every evidence artifact carries the three schema fields AC26 requires.

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs | 343 Read lines; usings 2-19; fields 25-28; Cleanup_Files 30-36; attributes 38, 63, 103, 164, 227, 241, 290, 311, 322, 333; ShowDialog( 112, 135, 175, 198, 255, comment 258; destination overload 227-239 (235); SaveCaseAsync 241-288 (275); SaveCase 290-309 (labels 300, 303); IsPicture 311-320; SaveMessageAsMsgAsync 322-331; SaveMessageAsMSG 333-340; at HEAD 9de3f176d (revision 1.9): sessions 18-26 (comment 15-17); AllPromptSessions 31-38 (four elements); Cleanup_Files 40-46 (foreach Reset); no YesNoToAllResponse field; at HEAD eb7871502 (revision 2.0): excluded wrapper 111-122; synchronous core 130-156; line 143 IsImage ternary with arms 144-145; Ask 146-148; SaveCase 149-154; ReleaseSingleAnswer 155; asynchronous selection 209
CITATION: UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs | 173 Read lines; RemoveReadOnlyPrompt 28-30; wrapper 32-51 (lambda 49); forward 53-73; core 87-160 (97, 101, 103, 108-113, 120-133, 125, 127, 134-140, 147, 151-154, 155, 156-159, 160); ClearReadOnlyAttributeOnDisk 165-170; at HEAD 9de3f176d (revision 1.9): RemoveReadOnlyPrompt 15-17 (comment 12-14)
CITATION: UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs | 196 Read lines; attributes 26, 82, 94, 109, 139, 172; StripTabsCrLf 127-137; WriteCSV 139-170 (145, 147, 151-163, 165, 166); SanitizeArray 172-193 (177, 183)
CITATION: UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs | usings 2-19; logger 25-27; 278 Read lines (277 by Get-Content)
CITATION: UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs | usings 2-19; 389 Read lines (388 by Get-Content); ForEachAsync lambda 153
CITATION: UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs | 241 Read lines (240 by Get-Content); MAX_PATH 25; attribute 27; SaveAttachmentsOld 28; ShowDialog( 165, 182, 199
CITATION: UtilitiesCS/UtilitiesCS.csproj | Compile 817-824 (LegacyAttachmentSaving 820); Microsoft.Office.Interop.Outlook reference 222-224 with EmbedInteropTypes True at 223
CITATION: QuickFiler/Controllers/EfcDataModel.cs | 465 Read lines; usings 1-17; MailInfo 206; TryGetArchiveRoot 245; MoveToFolderAsync 268-311 (guards 278, 289, 294; 308-310); InvokeFilerAsync 320-326; MAPIFolder overload 377-398; no finally; single return result 310
CITATION: ToDoModel/Email Utilities/SortItemsToExistingFolder.cs | 403 Read lines (R2 section 8; not compiled)
CITATION: UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs | 458 Read lines; TestMethod 41, 54, 79, 104, 140, 157, 174, 182, 209, 245, 272, 291, 316, 341, 359; GetAttachmentsInfo tests 182-207, 209-234; sandbox 238; try-save tests 245-266, 272-289; SanitizeArray test 359-382; endregion 384; at HEAD 9de3f176d (revision 1.9): summary element 170-173 of Cleanup_Files_DoesNotThrow (171 the only `tracking fields` line; AllPromptSessions 0 hits), TestMethod 174, test 175-180, region header 127, 488 lines (p6-t10 LINES row)
CITATION: UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs | 376 Read lines; TestMethod 33, 57, 84, 110, 140, 165, 193, 217, 245, 273, 297; constants 24-27; SaveAsync doc 319-322; Seams 338-373 (348-357)
CITATION: UtilitiesCS.Test/UtilitiesCS.Test.csproj | Compile 98, 99, 100; Microsoft.Office.Interop.Outlook reference 748-750 with EmbedInteropTypes False at 749
CITATION: QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs | 400 Read lines; TestMethod 48, 71, 94, 119, 144, 175, 196, 226, 251, 271, 293; MoveAsync 314-323; fixture 330-366; TestableEfcDataModel 379-397
CITATION: QuickFiler.Test/QuickFiler.Test.csproj | Compile 127, 128; at HEAD eb7871502 (revision 2.0): ArchiveRoot 127, FilerCleanup 128, UiThreadDispatcherFixtureTests 204, InitializationTests 205, DedicatedWorkerThread 229, WinFormsPumpHostTests 230; no PinCount, SynchronousBackgroundWorker or ArmingFakeTimeProvider line; main-side shape from the sibling worktree agent-a291a7fbabf9d0229 at its lines 204, 230, 231
CITATION: UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs | class 13; constructor 24; Response 33; Ask 40-48; ReleaseSingleAnswer 54-60; Reset 65-68
CITATION: UtilitiesCS/OutlookObjects/Attachment/AttachmentHelper.cs | constructor 34-37; Init assignments 101-111; FilePathHelperSaveAlt 174-175; FilePathSave 178-182; FilePathSaveAlt 185-189; FolderPathSave 199-203
CITATION: UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs | FolderPath setter 83-91; handler 348; FolderPath case 360-367; FilePath case 369-396
CITATION: UtilitiesCS/Properties/AssemblyInfo.cs | InternalsVisibleTo 18-20 (Verified Repository Facts 9)
CITATION: docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md | line 99 trailing clause; line 149; line 155; heading 151
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/spec.md | planner note 12; D1-D18 128-145; Technical specifications 231-532; Test Strategy 567-624; Acceptance Criteria 627; AC1-AC27 628-654; TrySaveAttachmentDelegate at 12, 132, 195, 196, 264, 285, 457 after the revision 1.7 correction (681 lines); revision 1.9 re-read: D5 132, Data / API 199, 239-244, AC5 632, AC7 634, AC14 641, AC22 649, AC23 650, AC24 651, AC26 653 (no AC text changed)
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/issue.md | Work Mode 12; scope rule 35
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/research/2026-10-02T05-15-sort-email-latent-logic-defects-research.md | section 3.5 329-371 (tripwire 341-350; T12 358-366)
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/research/2026-10-02T05-50-sort-email-966-consolidation-research.md | sections 0.2, 1.3, 1.5, 1.6, 1.8, 3, 4.2, 4.3, 5.2, 5.4, 6.2, 7, 8, 9.3, 10, 11, 13; N1-N11
CITATION: scripts/vscode/Invoke-MSTest.TrxSummary.ps1 | Get-TrxRunSummary 12 (testName 84); Format-TrxRunSummary 103 (140-147)
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | Get-CoberturaClassLineSummary 160 (LineMap 192-257); Merge-CoberturaClassesByFilename 260
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1 | percent format 90-91; summary line 117-120; Get-CoberturaFirstPartyCoverageReport 123
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1 | ConvertTo-JacocoPackageProjection 14; Assert-JacocoProjectionReconciliation 83
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | fixed filter 91; ConvertTo-DerivedCoverageSettingsXml 97; discovery exclusion 348-355 (353); entry guard 459
CITATION: scripts/vscode/TaskMaster.cli.runsettings | 9 lines; Workers 5; Scope 6
CITATION: docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/qa-gates/coverage-comparison.md | baseline uncovered set of this item 121-129 (T 49, 154, 155; M 153); CONTROL-LINE 28; filename form 9-15
CITATION: docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/baseline/coverage-baseline.md | CMD-COVERAGE-DIRECT and CMD-COVERAGE-POST success-case labels 7-76
CITATION: docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/baseline/test-run-baseline.md | CMD-VSTEST success-case labels 7-30
CITATION: docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/baseline/p0-t9-stall-probe.2026-10-01T20-41.md | STALL-PROBE REPRODUCES 19; EXCLUSION 20
CITATION: docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/regression-testing/pass-after-shape-b.2026-09-16T23-27.md | data rows as RESULT entries 31-47
CITATION: .gitignore | entries by content: artifacts/, *.trx, *cobertura*.xml, coverage/*, !coverage/.gitkeep, .dotnet*/, [Bb]in/, [Oo]bj/, **/[Pp]ackages/* (one line each; excluded from PATHS-CITED)
CITATION: .claude/rules/plan-acceptance-gates.md | G1-G9 rule table; attribution window; checkable-literal definition
CITATION: .claude/skills/atomic-plan-contract/SKILL.md | canonical format; Phase 0 requirements; coverage evidence contract; planner internal review record
CITATION: .claude/skills/evidence-and-timestamp-conventions/SKILL.md | first occurrence of a duplicated field wins 121; expectation is per file 122
CITATION: scripts/vscode/Install-RepoDotNetSdk.ps1 | CmdletBinding and param 1-2; throw 103; no exit statement or LASTEXITCODE use (child pwsh -File process exit code)
CITATION: UtilitiesCS/Threading/StoreLockupResponder.cs | public non-generic delegate StoreLockupNotifier 20-25, the nearest delegate-declaration precedent (PD-14)
CITATION: UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.Transform.cs | generic delegate FolderGroupTransformer<T> 23-29 over FolderWrapper[] (no interop parameter)
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p4-t7-format-and-build.2026-10-03T10-21.md | CS1769 stop record: ERROR_CODES 40; the sixteen call sites 50-51; cause 55
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p4-t5-legacy-deletion.2026-10-03T10-17.md | ITERATION 2 coordinator-run CMD-DELETE record form 6, 9-18
CITATION: UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs | on disk after the P4-T7 rewrite: OriginFolder 25; DestinationFolder 26; RedirectSaveFolder test 305-320 (assertions 316-319); structural test assertions 345-354; at HEAD eb7871502 (revision 2.0): 437 lines; using System 1 (DateTime 27, Func 391); AS2 62-92; SS1-SS3 220-297 (NoToAll assertion 296, brace 297); RR summary 299-300; eleven TestMethod at 33, 67, 98, 129, 156, 194, 224, 251, 277, 304, 328; photo.jpg mocks 37, 71, 308; helpers 357-435; SS4 name 0 hits
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/fail-before-redirect-save-folder.md | 2026-10-03T11-28 stop record (superseded by the P4-T9 re-run): schema rows 3-6; COUNTERS 16; RESULT rows 18-28; MESSAGE entry 29-33; acceptance items 40-44; stop label 46
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t10-post-format-census.2026-10-03T12-52.md | ITERATION 1 at 4; TOKENS-TST1 totals 175-187; LINES SortEmail_Tests.cs = 488 at 188 and 385; SHA256 189
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t12-scope-boundary.2026-10-03T12-55.md | ITERATION 1 at 4; ANCHOR-RECHECK 9; OUTSIDE-WRITE-SET 0 at 15; WRITE-SET-MISSING 0 at 16; numstat rows 19-25 (no TST1 row); clauses 32-39
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t13-identity-and-sweep.2026-10-03T12-56.md | HOOK BLOCK stop record of the task now numbered P6-T16: schema rows 3-6; hook text 11; cause 13; CMD-SWEEP counts 22-28; stays on disk unchanged
CITATION: UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs | at HEAD eb7871502 (revision 2.0): 343 lines; using System once at line 1, using System.Collections.Generic at 2; the eighteen PD-16 System-namespace alternatives 0 lines; constants 22-25; first DataTestMethod 31
CITATION: coverage/final-959.cobertura.xml | git-ignored P6-T7 document on disk (revision 2.0): AttachmentSaving class node 59687 (branch-rate 0.979592); SaveAttachment method node 59791 (branch-rate 0.75); line 143 condition-coverage 50% (1/2) at 59802; class-level lines block from 60001
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | condition-coverage carried through the class merge 350-354 (revision 2.0; the basis of reading the post-processed document)
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/code-review.2026-10-06T15-30.md | findings table 24-32 (CR-1 line 143 and SS1-SS3 at 26; CR-2 at 27; CR-3 TSC line 1 at 28; CR-4 to CR-7 at 29-32); U-1 to U-3 78-82
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/policy-audit.2026-10-06T15-30.md | verdict table 13-23 (85.39, 79.81, 7393); per-file table 145-155 (AttachmentSaving row 147); G-1 to G-7 205-211 (G-4 at 208)
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/feature-audit.2026-10-06T15-30.md | AC inventory 20-48 (AC6 and AC27 unchecked); check-off section 84-88
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/coverage-post-change.md | ITERATION 1 at 4; EXIT_CODE 6; FIRST-PARTY 32-36; FAILED-SET 37; NEWLY-FAILING 38; FINAL-UCS-BRANCH 40; summary 44-50 (Total 7393)
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/toolchain-final-pass.md | ITERATION 1 at 4; LOOP-RESTARTS 5; step rows 12-18
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/coverage-comparison.md | BASELINE-HASH in Command 5; AttachmentSaving row 142/142 at 9; SORTEMAIL-CLASS rows 8-12 (5/5, 142/142, 93/89, 45/45, 1/0) and SORTEMAIL-AGG 286/281/5 at 13 (revision 2.0 delta pass); EXEMPT-LINES 24; NONEXEMPT hash 33; NOT-LOWER 40-41; MEMBER SaveAttachmentCore 74; MEMBERS-BELOW-90 88
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/pass-after-regression-tests.md | ITERATION 1 at 4; RUNSETTINGS-HASH-NOW 10; COUNTERS total=55 at 20
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t12-scope-boundary.2026-10-06T13-23.md | ITERATION 2 at 6; ANCHOR-RECHECK 10; SUBTRACTED-CLAUSE-A 27 at 14; SUBTRACTED-CLAUSE-B 26 at 15; numstat rows 20-26
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t16-identity-and-sweep.2026-10-06T15-11.md | coordinator-run CMD-TST-IDENTITY 9-34 (hook refusal 11); CMD-SWEEP FILES 84 at 39; CMD-EVIDENCE-FIELDS 51-58
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t46-ac-inventory.2026-10-06T15-23.md | Output Summary 6 (AC-CHECKED 25, 89 files); inventory table 12-40
CITATION: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/p0-t3-worktree-context.2026-10-03T08-24.md | ORIGIN-MAIN-SHA observation 11; MERGE-BASE 12; INHERITED-CLAUSE-A 17-44 (27 paths)
CITATION: .claude/hooks/enforce-epic-worktree-removal-gate.ps1 | SubcommandPath worktree remove 171, 172, 380; block reason 414
CITATION: .claude/hooks/enforce-epic-merge-gate.ps1 | SubcommandPath pr merge 178, 184, 386, 387
CITATION: .claude/hooks/enforce-pr-author-skill-helpers.ps1 | SubcommandPath pr create 279; pr edit 280
CITATION: .claude/hooks/enforce-promotion-mcp-only.ps1 | SubcommandPath issue create 126; issue new 127
AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8, AC9, AC10, AC11, AC12, AC13, AC14, AC15, AC16, AC17, AC18, AC19, AC20, AC21, AC22, AC23, AC24, AC25, AC26, AC27
AC-MAPPING: AC1 | IMPLEMENTATION: P1-T8 Edit E-A-L1 (stacked labels, attribute removed); L-A-FINAL SaveCase carried by P4-T4 | TESTS: TOKENS-A labels 1 each, pipe token 0, HasFlag 0 in P1-T8, P1-T9, P4-T4, P6-T10; EFCC-MEMBERS without SaveCase in P6-T10 | EVIDENCE: p1-t8-l1-census, p6-t10-post-format-census; check-off P6-T19
AC-MAPPING: AC2 | IMPLEMENTATION: P1-T1 Listing L-TSC-P1; P1-T3 Compile Include | TESTS: P1-T6 expect-fail (four rows failed, control passed, `but was 0 times`); P1-T11 and P6-T5 five rows Passed; TOKENS-TSC | EVIDENCE: fail-before-save-case.md, pass-after-regression-tests.md, p6-t10-post-format-census; check-off P6-T20
AC-MAPPING: AC3 | IMPLEMENTATION: P3-T4 Listing L-T-FINAL (forward, private core with guard, logger calls, outer catch removed, usings) | TESTS: TOKENS-T FINAL in P3-T4 and P6-T10; CMD-USINGS T; GUARD-CONDITION-LINES and EXEMPT-GUARD-BRACE-COUNT 1 in P6-T8; EFCC-MEMBERS T entries | EVIDENCE: p3-t4-trysave-census, p6-t10-post-format-census, coverage-comparison.md; check-off P6-T21
AC-MAPPING: AC4 | IMPLEMENTATION: P3-T1 Edits E-TST2-TRIPWIRE and E-TST2-T12 | TESTS: P3-T3 expect-fail (T12 failed with InvalidOperationException); P3-T6 twelve Passed; TOKENS-TST2 FINAL | EVIDENCE: fail-before-try-save-retry.md, p3-t1-trysave-tests-census, p6-t10-post-format-census; check-off P6-T22
AC-MAPPING: AC5 | IMPLEMENTATION: insert-only Edits on TST2 (P3-T1); no edit of the two TST1 try-save tests | TESTS: P6-T16 TST2-DELETED-LINES 0 and TST1-REMOVED-TRYSAVE-LINES 0; P6-T5 NAMES-T and the two TST1 pins Passed; BANNED-TEST-APIS 0 | EVIDENCE: p6-t16-identity-and-sweep, pass-after-regression-tests.md, p6-t10-post-format-census; check-off P6-T23
AC-MAPPING: AC6 | IMPLEMENTATION: P1-T9 Edit E-A-L3; P1-T2 Listing L-TAS-P1 (transient); the pull-request clause deferred to the orchestrator's pr-author step (PD-11) | TESTS: P1-T7 expect-fail (the `_attachmentsAltName` row failed, three passed); P1-T12 four Passed; TOKENS-A P1 (alt-name reset 3) and TOKENS-TAS P1 (phase-one name 5) | EVIDENCE: fail-before-cleanup-files-phase-one.md, p1-t9-l3-census, p1-t2-attachmentsaving-tests-census, pr-description-inputs (UT5 call-out), p6-t24-ac6-deferred; recorded as DEFERRED TO PR STEP by P6-T24, never checked by this plan
AC-MAPPING: AC7 | IMPLEMENTATION: P4-T4 Listing L-A-FINAL (three sessions, AllPromptSessions property with comment, foreach cleanup, no enum fields) | TESTS: ENUM-FIELDS-A 0, SHOWDIALOG-CALLS-PARTIALS 0, TOKENS-A new(YesNoToAll.ShowDialog) 3, YesNoToAllResponse_ 0, AllPromptSessions 2 in P6-T10; AC7 Read in P6-T25; Cleanup_Files tests Passed in P6-T5 | EVIDENCE: p6-t10-post-format-census, p6-t46-ac-inventory, pass-after-regression-tests.md; check-off P6-T25
AC-MAPPING: AC8 | IMPLEMENTATION: P4-T4 Listing L-A-FINAL (excluded wrappers forwarding method groups and field references; internal cores; six-argument SaveCaseAsync) and the P4-T7 rewrite of A from the revision-1.6 listing with the nested TrySaveAttachmentDelegate seam type (PD-14); P5-T1 and P5-T2 using blocks of S and M | TESTS: TOKENS-A SEAMED in P4-T7 and FINAL delegate tokens in P6-T10; MEMBER rows in P6-T9; NUMSTAT-S and NUMSTAT-M 0 9 and OUTSIDE-WRITE-SET 0 in P6-T12; rebuilds P6-T3 and P6-T4; core tests Passed in P6-T5; FEATURE/spec.md technical-section Grep in P6-T26 | EVIDENCE: p6-t10-post-format-census, coverage-comparison.md, p6-t12-scope-boundary, pass-after-regression-tests.md; check-off P6-T26
AC-MAPPING: AC9 | IMPLEMENTATION: P4-T1 Listing L-TSC-FINAL and P4-T2 Listing L-TAS-FINAL, both rewritten by P4-T7 from the revision-1.6 listings (RecordingSave returns SortEmail.TrySaveAttachmentDelegate, PD-14) | TESTS: P4-T3 expect-fail compile-red naming the four missing overloads; P4-T7 ITERATION 2 green build with TOKENS-TSC and TOKENS-TAS FINAL; P4-T8, P4-T11, P4-T12 and P6-T5 rows Passed; TOKENS-TSC and TOKENS-TAS own-session tokens | EVIDENCE: compile-red-attachment-saving-seams.md, pass-after-regression-tests.md, p6-t10-post-format-census; revision 2.0: p7-t2-tas-ss4-edit, p7-t4-scoped-format-and-census (TOKENS-TAS SS4 column), p7-t5-attsave-run (twelve rows with SS4 Passed), pass-after-regression-tests.md ITERATION 2 and 3; check-off P6-T27
AC-MAPPING: AC10 | IMPLEMENTATION: P4-T2 structural test replacing the phase-one test | TESTS: TOKENS-TAS FINAL (phase-one name 0, structural name 1, SetValue 0, DoNotParallelize 0) in P6-T10; mutation control P4-T13 to P4-T15 (`but found 3`, restore hash equal) | EVIDENCE: p6-t10-post-format-census, p4-t14-control-applied, p4-t15-control-restored, negative-controls.md; check-off P6-T28
AC-MAPPING: AC11 | IMPLEMENTATION: P4-T4 one-statement RedirectSaveFolder and the destination overload calling it; P4-T10 Edit E-A-RR-SECOND | TESTS: P4-T9 expect-fail (RR failed on the alternate path); P4-T11 eleven Passed; TOKENS-A re-rooting tokens; OUTSIDE-WRITE-SET 0 (AttachmentHelper.cs unchanged) | EVIDENCE: fail-before-redirect-save-folder.md, p4-t10-rr-fix, p6-t10-post-format-census, p6-t12-scope-boundary; check-off P6-T29
AC-MAPPING: AC12 | IMPLEMENTATION: P4-T4 (IsPicture and _responseSaveFile removed) and P4-T5 (CMD-DELETE of L, Edit E-UCS-CSPROJ-REMOVE) | TESTS: DEAD-MEMBERS-CS 0, DEAD-TOKENS-PARTIALS 0, LEGACY-FILE-EXISTS False in P6-T10; DELETED-PATHS and NUMSTAT-UCS 0 1 in P6-T12; dossier entry 3 | EVIDENCE: p4-t5-legacy-deletion, p6-t10-post-format-census, p6-t12-scope-boundary, fail-before-exception dossier; check-off P6-T30
AC-MAPPING: AC13 | IMPLEMENTATION: P2-T7 (SanitizeArrayLineTSV exclusion removed) and P4-T4 (six A-side removals); P4-T6 Edits E-TST1-ROWS-SYNC and E-TST1-ROWS-ASYNC | TESTS: EFCC-PARTIALS 18 and EFCC-MEMBERS in P6-T10 with per-file attribute totals; TOKENS-TST1 FINAL; the six L-TST1-ROWS rows Passed in P6-T5 | EVIDENCE: p6-t10-post-format-census, p4-t6-tst1-rows-census, pass-after-regression-tests.md; check-off P6-T31
AC-MAPPING: AC14 | IMPLEMENTATION: P2-T3 seam (Edits E-U-SEAM-HEAD and E-U-SEAM-WRITE), P2-T7 Listing L-U-FINAL (MovedMailsHeader, corrected core, excluded forward with justification comment, SanitizeArray deleted), P2-T8 Edit E-TST1-DEL-SANITIZE | TESTS: TOKENS-U FINAL and CMD-USINGS U in P2-T7 and P6-T10; TOKENS-TST1; SanitizeArrayLineTSV and StripTabsCrLf tests Passed in P6-T5; OUTSIDE-WRITE-SET 0 (AppOlObjects.cs unchanged) | EVIDENCE: p2-t7-undo-final-census, p2-t8-tst1-census, p6-t10-post-format-census, pass-after-regression-tests.md, p6-t12-scope-boundary; check-off P6-T32
AC-MAPPING: AC15 | IMPLEMENTATION: P2-T1 Listing L-TUL; P2-T2 Compile Include | TESTS: P2-T6 expect-fail after the seam step (both tests failed with the named substrings); P2-T10 two Passed; TOKENS-TUL | EVIDENCE: fail-before-write-csv.md, p2-t1-undo-tests-census, p6-t10-post-format-census; check-off P6-T33
AC-MAPPING: AC16 | IMPLEMENTATION: P5-T11 Edits E-SPEC956-99, E-SPEC956-149, E-SPEC956-155 | TESTS: CMD-SPEC-CHECK final S956 literal counts, unchanged check-box counts, two added lines; NUMSTAT-SPEC956 4 2; OUTSIDE-WRITE-SET 0 (code-review record untouched) | EVIDENCE: p5-t11-cr1-spec956, p6-t12-scope-boundary; check-off P6-T34
AC-MAPPING: AC17 | IMPLEMENTATION: using blocks of Listings L-A-FINAL, L-T-FINAL, L-U-FINAL (P4-T4, P3-T4, P2-T7) and Edits E-S-USINGS, E-M-USINGS (P5-T1, P5-T2) | TESTS: CMD-USINGS USINGS-EXACT-FILES 5, BANNED-USINGS-PARTIALS 0, USING-SYSTEM-PARTIAL-FILES 5 in P6-T10; P5-T3 production rebuild; P6-T3 and P6-T4 rebuilds | EVIDENCE: p5-t3-usings-build, p6-t10-post-format-census, p6-t3-msbuild-analyzers, p6-t4-msbuild-nullable; check-off P6-T35
AC-MAPPING: AC18 | IMPLEMENTATION: P5-T6 Edits E-E-SEAM-CALL and E-E-SEAM-MEMBER; P5-T8 Edit E-E-FINALLY; P5-T4 Listing L-TEF; P5-T5 Edit E-QFT-CSPROJ | TESTS: P5-T7 expect-fail (throwing test failed with `but found 0`); P5-T9 three Passed and eleven archive-root pins Passed with ART-PORCELAIN EMPTY; TOKENS-E FINAL; NUMSTAT-QFT 1 0 | EVIDENCE: fail-before-efc-filer-cleanup.md, p5-t6-efc-seam, p5-t8-efc-finally, p5-t5-qft-csproj, p6-t10-post-format-census, p6-t12-scope-boundary; check-off P6-T36
AC-MAPPING: AC19 | IMPLEMENTATION: P5-T10 CMD-DELETE of TD (no project-file change, PD-13) | TESTS: TODOMODEL-FILE-EXISTS False, TODOMODEL-CSPROJ-MATCHES 2 naming only ToDoModel.Test.csproj in P6-T10; single D row under ToDoModel in P5-T10; dossier entry 5; P6-T3 and P6-T4 rebuilds | EVIDENCE: p5-t10-todomodel-deletion, p6-t10-post-format-census, fail-before-exception dossier, p6-t3-msbuild-analyzers; check-off P6-T37
AC-MAPPING: AC20 | IMPLEMENTATION: the six expect-fail tasks P1-T6, P1-T7, P2-T6, P3-T3, P4-T9, P5-T7 and the dossier P5-T12 | TESTS: each fail-before artifact non-zero EXIT_CODE equal to its expectation with failing rows named; dossier with eight entries | EVIDENCE: the six fail-before artifacts, fail-before-exception dossier; check-off P6-T38
AC-MAPPING: AC21 | IMPLEMENTATION: Listings L-TSC-P1, L-TSC-FINAL, L-TAS-P1, L-TAS-FINAL, L-TUL, L-TEF and the TST1 and TST2 Edits (MSTest, Moq, FluentAssertions only; no disk, dialog, sleep, delay, timeout, retry or serialization attribute; no static write in the final tree) | TESTS: BANNED-TEST-APIS 0, NON-APPROVED-FRAMEWORKS 0, RETRY-READ NONE FOUND, TOKENS-TAS SetValue 0 in P6-T10; CMD-COVERAGE-DIRECT under the CLI runsettings in P6-T7 with EXIT_CODE 0, empty FAILED-SET and every SANDBOX False; SANDBOX False in P6-T5 and P6-T6 | EVIDENCE: p6-t10-post-format-census, coverage-post-change.md, pass-after-regression-tests.md; revision 2.0: p7-t4-scoped-format-and-census (banned-API tokens 0 in TAS and TSC), p7-t5-attsave-run, coverage-post-change.md ITERATION 2 (P7-T11) and 3 (P8-T9) with every SANDBOX False; check-off P6-T39 under branch (a), else left unchecked with AC21 NOT MET
AC-MAPPING: AC22 | IMPLEMENTATION: the eighteen-path Write Set (PD-2, PD-3) | TESTS: CMD-FOOTPRINT OUTSIDE-WRITE-SET 0, WRITE-SET-MISSING 0, DELETED-PATHS, NUMSTAT-UCT 3 0 in P6-T12 and again in P6-T15 (ITERATION 2, after the revision 1.9 doc-comment edit of TST1 by P6-T13, Write Set path 10); TST1-REMOVED-DOC-LINES 1 and TST1-ADDED-DOC-LINES 1 in P6-T16; CMD-CSPROJ final column in P6-T10 | EVIDENCE: p6-t12-scope-boundary (ITERATION 2), p6-t13-tst1-doc-comment, p6-t16-identity-and-sweep, p6-t10-post-format-census; revision 2.0: p7-t15-scope-boundary (the eight P6-T12 clauses after the TAS and TSC edits, Write Set items 12 and 13), p8-t3-fanin-gate (own paths only against ORIGIN-MAIN-SHA after the Phase 8 merge; NUMSTAT-UCT-VS-MAIN 3 0); check-off P6-T40
AC-MAPPING: AC23 | IMPLEMENTATION: every rewritten or created C# file sized by the Listings (predictions in Census Expectations) | TESTS: CMD-LINES MAX-LINES at most 499 over PATHS-CSHARP-FINAL with AC23-CLOSEST rows in P6-T10; per-phase LINES ceilings in P2-T7, P3-T4, P4-T4, P5-T8; TST1 LINES 488 re-measured by P6-T14 after the revision 1.9 doc-comment edit | EVIDENCE: p6-t10-post-format-census, p6-t14-doc-comment-format-and-census; revision 2.0: p7-t4-scoped-format-and-census (CMD-LINES over PATHS-CSHARP-FINAL after E-TAS-SS4 and E-TSC-USING, TAS predicted 469 and TSC 342, MAX-LINES at most 499); check-off P6-T41
AC-MAPPING: AC24 | IMPLEMENTATION: Phase 6 loop P6-T1 to P6-T7 in CLAUDE.md order with the loop rule | TESTS: P6-T11 rows with exit codes and SKIP_CORECOMPILE_LINES 0 for both rebuilds, the P6-T7 row at exit code 0; P6-T14 FORMAT_EXIT_CODE 0, SCOPED-CHECK_EXIT_CODE 0 and REPO-CHECK_EXIT_CODE 0 after the revision 1.9 doc-comment edit (the read-only formatter check is the toolchain step that observes a summary-prose change; the rebuilds and test runs of the recorded pass are not repeated, revision 1.9 log) | EVIDENCE: toolchain-final-pass.md, p6-t1 to p6-t4 artifacts, p6-t14-doc-comment-format-and-census, pass-after-regression-tests.md, coverage-post-change.md; revision 2.0: toolchain-final-pass.md ITERATION 2 (P7-T14; the P7-T6 to P7-T11 pass over the whole solution, which is the first rebuild and test run after the P6-T13 edit) and ITERATION 3 (P8-T11; the P8-T4 to P8-T9 pass on the merged tree), with the p7-t6 to p7-t9 and p8-t4 to p8-t7 artifacts; check-off P6-T42 under branch (a), else left unchecked with AC24 NOT MET
AC-MAPPING: AC25 | IMPLEMENTATION: baseline P0-T11 before P1-T8; final P6-T7 after P6-T4; PD-8 comparison rule (four content-identified exemption sets, non-exempt set hash, in-memory control, first-party not-lower flags); CMD-MEMBER-COVERAGE over the ten members | TESTS: P6-T8 NONEXEMPT-SET-MATCHES-BASELINE True, CONTROL-DIFFERS-BASELINE True, both NOT-LOWER True, exemption counts 1, 1, 1, 1; P6-T9 MEMBERS-BELOW-90 0, MEMBERS-UNMEASURED 0, E-CHANGED-LINES-COVERED 3 | EVIDENCE: coverage-baseline.md, coverage-post-change.md, coverage-comparison.md; revision 2.0: coverage-post-change.md and coverage-comparison.md ITERATION 2 (P7-T11 to P7-T13: the P6-T8 and P6-T9 clauses repeated, the SortEmail family aggregate and class rows identical to Phase 6, the Phase 6 first-party figures 85.39 and 79.81 recorded, the CR-1 arm A line 143 at 100% (2/2) against 50% (1/2) in p7-t1-pre-edit-observations) and ITERATION 3 (P8-T9, P8-T10: floors gated, Phase 7 figures recorded, the CR-1 arm still 100% (2/2)); check-off P6-T43
AC-MAPPING: AC26 | IMPLEMENTATION: evidence conventions of this plan (Markdown projections under the three canonical subfolders; three line-leading schema fields on every artifact, prefixed labels never substituting; raw documents stay under coverage/) | TESTS: RAW-DOC-PATHS 0 in P6-T12; RAW-DOCUMENT-FILES 0 in P6-T16 and P6-T46; EVIDENCE-MISSING-FIELDS 0, NONCANONICAL-SUBFOLDER-FILES 0, FIELD-CHECK-CONTROL False, FIELD-CHECK-POSITIVE True, SUBFOLDER-CHECK-FLAGS-OTHER True and SUBFOLDER-CHECK-FLAGS-QA-GATES False in P6-T16; in the P6-T44 run (the check-off basis, every artifact through P6-T43); and in P6-T46 (a post-check-off confirmation that also covers the P6-T45 artifact, whose three fields its task text fixes; a missing field found there is repaired by adding it and never reverts the box); non-zero EXIT_CODE and failing names in the six fail-before artifacts; MISSING_ counts in the compile-red record | EVIDENCE: p6-t12-scope-boundary, p6-t16-identity-and-sweep, p6-t46-ac-inventory (carrying the P6-T44 run under its AC26 heading), compile-red-attachment-saving-seams.md; revision 2.0: p7-t15-scope-boundary (RAW-DOC-PATHS 0), p7-t16-phase7-closure and p8-t13-phase8-closure (CMD-EVIDENCE-FIELDS and CMD-SWEEP over every Phase 7 and Phase 8 artifact, the fixed-name rewrites keeping their schema rows at the top); check-off P6-T44
AC-MAPPING: AC27 | IMPLEMENTATION: deferred to the orchestrator's pr-author step (PD-11); P6-T18 supplies the closing references, the four behavior changes and the UT5 call-out verbatim | TESTS: P6-T45 Grep confirming AC27 remains unchecked; P6-T46 AC-CHECKED 25 (23 under branch (b)), AC6-UNCHECKED 1 and AC27-UNCHECKED 1 | EVIDENCE: pr-description-inputs, p6-t45-ac27-deferred, p6-t46-ac-inventory; recorded as DEFERRED TO PR STEP, never checked by this plan
UNRESOLVED-GAPS: NONE
PREFLIGHT: VALIDATION REQUESTED (DIRECTIVE: PREFLIGHT VALIDATION ONLY through atomic-executor; the planner-side record above is not executor clearance)

