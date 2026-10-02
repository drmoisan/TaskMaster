INCOMPLETE: stopped for quota (coordinator notice, 2026-10-02). Written so far: header, Write Set, Planner Decisions PD-1 to PD-13, Execution Conventions, Verified Repository Facts, Test Totals, Listings L-TSC-P1, L-TAS-P1, L-TUL, L-TSC-FINAL, L-TAS-FINAL. Still to write: Listings L-TEF, L-A-FINAL, L-T-FINAL, L-U-FINAL, L-TST1-ROWS; Edit Specifications; Test Inventory; Command Reference; Phases 0 to 6 (111 tasks); Planner Self-Review Record. The `<!-- APPEND-POINT -->` line at the end marks where authoring resumes.

# 2026-10-01-sort-email-latent-logic-defects (Plan)

- **Issue:** #959 (the pull request also closes #966; spec D18)
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-02 (revision 1.0, initial authoring against FEATURE/spec.md revision 0.2)
- **Status:** Draft awaiting preflight (DIRECTIVE: PREFLIGHT VALIDATION ONLY through atomic-executor)
- **Version:** 1.0
- **Work Mode:** full-bug (FEATURE/issue.md line 12, `- Work Mode: full-bug`). AC source: FEATURE/spec.md section `## Acceptance Criteria` (spec.md line 627 after the planner revision note was inserted at line 12), AC1 to AC27 at spec.md lines 628 to 654, all unchecked at authoring time; the AC lines read `- [ ] ACn (<title>).`. No user-story.md exists or is required.
- **Research:** FEATURE/research/2026-10-02T05-15-sort-email-latent-logic-defects-research.md (R1) and FEATURE/research/2026-10-02T05-50-sort-email-966-consolidation-research.md (R2; R2 section 0.2 supersedes R1 where it says so; R2 section 10 is the write set; R2 section 11 the red-first order).
- **Branch:** bug/sort-email-latent-logic-defects-959, cut from origin/main 94287369908cc920b21b0e3256314f988ad7d2f5 (the SHA at which every citation below was derived). Anchor: the `MERGE-BASE:` value P0-T3 records after `git fetch origin main` (`git merge-base HEAD origin/main`); every diff gate is a two-dot comparison of the working tree against that recorded SHA paired with a `git status --porcelain` span (rules G8 and G8b).
- **Execution session requirement:** the executor runs later, non-isolated, from the item worktree, with `pwsh` available (an isolated agent is refused `pwsh` in every form). Every command-bearing task runs either one `git -C WORKTREE ...` invocation or one `pwsh -NoProfile -Command '<payload>'` process whose first statement is `Set-Location -LiteralPath "WORKTREE"`. P0-T4 probes that channel first and stops with `CHANNEL UNAVAILABLE` if it is refused. The executor never edits artifacts/orchestration/orchestrator-state.json, never runs `git update-index`, and never edits hook, permission or policy files. Script invocations use absolute script paths resolved at run time (`Join-Path (Get-Location).Path "scripts\vscode\<name>.ps1"`), never `pwsh -WorkingDirectory`.
- **Pre-implementation gate requirement:** artifacts/orchestration/orchestrator-state.json is seeded at authoring time with `issue-num` `959` (line 10), `feature-folder` docs/features/active/2026-10-01-sort-email-latent-logic-defects-959 (line 15), `route_id` `preparation` (line 3) and `lifecycle_ready` `true` (line 17). P0-T3 records the readiness fields read-only and stops with `PRE-IMPLEMENTATION GATE NOT SEEDED` when they are absent; a PreToolUse refusal at any later source edit is `PRE-IMPLEMENTATION GATE BLOCKED`, reported verbatim, and stops the run.
- **Task Count:** 111 (Phase 0: 12, Phase 1: 12, Phase 2: 11, Phase 3: 6, Phase 4: 15, Phase 5: 12, Phase 6: 43)

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QA artifact tasks, and coverage-comparison tasks for each in-scope language when policy requires coverage. If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Record the expected artifact path or location in each evidence-producing task. Do not mark evidence-backed work complete without the artifact.

---

## Blast Radius and Write Set

The Write Set is exactly these eighteen repository paths (R2 section 10; spec D15) plus FEATURE/** (evidence artifacts, this plan's check-offs, the twenty-seven AC check-off boxes of FEATURE/spec.md and the planner revision note already present in FEATURE/spec.md). The footprint gate P6-T12 enforces exactly this set.

1. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs` (modify; alias A)
2. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs` (modify; alias T)
3. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs` (modify; alias U)
4. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` (modify, using block only; alias S)
5. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs` (modify, using block only; alias M)
6. `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs` (delete; alias L)
7. `UtilitiesCS/UtilitiesCS.csproj` (modify: remove the one Compile Include line for L; numstat 0 added, 1 deleted)
8. `QuickFiler/Controllers/EfcDataModel.cs` (modify; alias E)
9. `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` (delete; alias TD)
10. `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` (modify; alias TST1)
11. `UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs` (modify; alias TST2)
12. `UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs` (create; alias TSC)
13. `UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs` (create; alias TAS)
14. `UtilitiesCS.Test/EmailIntelligence/SortEmail_UndoAndMoveLog_Tests.cs` (create; alias TUL)
15. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (modify: three Compile Include lines; numstat 3 added, 0 deleted)
16. `QuickFiler.Test/Controllers/EfcDataModelFilerCleanupTests.cs` (create; alias TEF)
17. `QuickFiler.Test/QuickFiler.Test.csproj` (modify: one Compile Include line; numstat 1 added, 0 deleted)
18. `docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md` (modify: three text edits; numstat 4 added, 2 deleted; alias SPEC956)

Not edited (scope boundaries, spec D15): UtilitiesCS/Dialogs/YesNoToAll.cs, UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs, UtilitiesCS/OutlookObjects/Attachment/AttachmentHelper.cs, UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs, TaskMaster/AppGlobals/AppOlObjects.cs, QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs, both ToDoModel.Test source files, ToDoModel/ToDoModel.csproj, .editorconfig, the #956 code-review record, UtilitiesCS/Properties/AssemblyInfo.cs, scripts/, config/, coverage.config, scripts/vscode/TaskMaster.cli.runsettings, .csharpierignore, .gitignore; FEATURE/spec.md text other than the twenty-seven check-off boxes; FEATURE/issue.md and FEATURE/research/. Local, git-ignored, never staged: coverage/ (except coverage/.gitkeep, .gitignore lines 150 to 151), every trx (line 146) and cobertura-named XML document (line 147), packages/, .dotnet-sdk/ (line 357), bin and obj.

## Orchestrator Decisions (binding, not reopened)

Spec decisions D1 to D18 (FEATURE/spec.md lines 127 to 144) are binding and are not restated here. Items the plan relies on by number: D1 (L1 labels, exclusion removed, tests in TSC), D2 (private core with `isRetryAfterClear`, logger calls, outer catch removed), D3 (two-phase L3), D4 (L4 seam, single header line, `SanitizeArray` deleted with its test), D5 (F1 sessions, `AllPromptSessions` property, seamed cores), D6 (`RedirectSaveFolder`), D7 (F2 deletions), D8 (F3 dispositions and the two added `DataRow` rows), D9 (CR-1 three edits), D10 (using blocks), D11 (EfcDataModel `try`/`finally` with `ResetFilerPromptState`), D12 (ToDoModel deletion), D13 (red-first workflow), D14 (test policy), D15 (write set), D16 (coverage), D17 (toolchain and evidence), D18 (closure).

## Planner Decisions

- PD-1 (phase order). R2 section 11's nine steps are grouped into six implementation-and-QA phases: Phase 1 L1 and L3 phase one (both in A, both red with no production pre-step); Phase 2 L4 and the header (U, TST1, TUL); Phase 3 L2 and the try-save logging (T, TST2); Phase 4 F1 with the re-rooting defect, F2, the A-side F3 removals, the A using block and the L3 structural test (A, L, UtilitiesCS.csproj, TSC, TAS, TST1); Phase 5 the S and M using blocks, EfcDataModel, the ToDoModel deletion and CR-1; Phase 6 the final QA loop, coverage comparison, footprint and check-offs. Each defect's regression test is observed red before its fix inside its own phase, so no green gate ever runs over a path that still holds a red-first test (task-ordering rule).
- PD-2 (listings and edits). Files that are created, or rewritten wholesale because their anchors are not unique after earlier edits, are written with the Write tool from a verbatim Listing: TSC (two states, L-TSC-P1 and L-TSC-FINAL), TAS (two states, L-TAS-P1 and L-TAS-FINAL), TUL (L-TUL), TEF (L-TEF), A final (L-A-FINAL), T final (L-T-FINAL), U final (L-U-FINAL). Every other production or test change is an Edit with the exact old and new text given in the Edit Specifications section; each old text was verified unique in its file at the cited tree. Source and project files are written only with the Write and Edit tools, so the pre-implementation gate observes every source write; `pwsh` payloads read source files and write only under the git-ignored coverage/ directory, with the single exception of PD-3.
- PD-3 (deletions). The Write and Edit tools cannot delete a file, and this plan runs no `git rm`. The two deletions (L in P4-T5, TD in P5-T10) use `CMD-DELETE`, one `Remove-Item -LiteralPath` statement on the repository-relative path, recorded with `EXISTS-BEFORE:`/`EXISTS-AFTER:`. These are the only `pwsh` writes to a tracked source path in this plan. A tool refusal of the deletion payload is `DELETE CHANNEL REFUSED`: stop and report; the executor does not substitute another route.
- PD-4 (test totals derived by counting). Every `COUNTERS total=` expectation below is the sum of test methods plus `DataRow` rows, per file and per plan state: TST1 15 at baseline (fifteen `[TestMethod]` at lines 41, 54, 79, 104, 140, 157, 174, 182, 209, 245, 272, 291, 316, 341, 359); 14 methods after P2-T8 deletes `SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows`; 18 rows after P4-T6 turns the two `GetAttachmentsInfo` tests into three-row data tests (12 single methods plus 2 times 3 rows). TST2 11 at baseline, 12 after P3-T1 (T12). TSC 5 rows in the P1 state (2 + 2 + 1), 12 rows in the final state (5 + 2 + 1 + 1 + 1 + 1 + 1). TAS 4 rows in the P1 state (one four-row data test), 11 in the final state (6 + 3 + 1 + 1). TUL 2. TEF 3. The `FILTER-SORTEMAIL` total is therefore 26 at baseline, 35 after Phase 1, 36 after Phase 2, 37 after Phase 3 and 55 from Phase 4 onward. R2 section 13 predicted 52 by counting the `SaveCaseAsync` tests as six rows and the TST1 additions as two; the counts above supersede that prediction (seven `SaveCaseAsync` rows because the Yes/YesToAll test has two rows; six added TST1 rows under PD-6). `EfcDataModelArchiveRootTests` carries eleven `[TestMethod]` attributes, so `FILTER-EFC-ARCHIVE` totals 11.
- PD-5 (TRX row identity). Every `[DataRow]` carries `DisplayName = "<method name> [<row tag>]"`, so the TRX `testName` of each row is deterministic and a `RESULT` line can be matched by its full name. This repository's committed run summaries show that a data-driven test with fifteen rows plus one plain test reports `COUNTERS_TOTAL=16` and sixteen `RESULT` rows with no parent aggregate row (docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/regression-testing/pass-after-shape-b.2026-09-16T23-27.md lines 31 to 47), so rows are counted one each in every total above. The in-repo attribute form is `[DataTestMethod]` with `[DataRow]` (UtilitiesCS.Test/Threading/CurrentStoreContextTests.cs lines 84 to 88).
- PD-6 (F3 `GetAttachmentsInfo` rows; a research correction). R2 section 3 item 1 adds one `(saveAttachments: true, savePictures: true)` row "so both `if` bodies are covered". That row covers neither filter body: `if (!saveAttachments)` runs only when saveAttachments is false and `if (!savePictures)` only when savePictures is false. The synchronous test's existing row is (false, true) and the asynchronous test's is (true, false), so each method would keep one filter statement uncovered once its exclusion is removed, which the coverage comparison of P6-T8 would report as a new uncovered line. Each test therefore gains TWO rows: the spec's (true, true) row (AC13 names it) and the complementary row that covers the other filter body ((true, false) for the synchronous test, (false, true) for the asynchronous test). AC13's clause "gains one DataRow row with saveAttachments true and savePictures true while keeping its existing row" holds; the complementary row is additional and is reported to the orchestrator in the handoff as a research correction.
- PD-7 (negative controls). The spec's Test Strategy asks for each code fix "reverted alone with its tests kept". Every red-first observation in this plan is that state: the regression test exists and the fix is absent (P1-T6, P1-T7, P2-T6, P3-T3, P4-T9, P5-T7), each recorded with the exact failing-row set. The one test that cannot be observed red-first is the structural `Cleanup_Files_ResetsEveryPromptSession` (a refactor replacement), so it receives a mutation control: one element removed from `AllPromptSessions`, the test observed failing, the element restored by the inverse Edit and the file hash proven equal (P4-T14, P4-T15). FEATURE/evidence/qa-gates/negative-controls.md records the mutation control and a table that maps each other fix to its red-first artifact and failing-row set. Re-mutating production six more times would add six builds and six runs with no additional discrimination.
- PD-8 (coverage comparison rule for AC25). `CMD-COVERAGE-TEXTS` aggregates the uncovered lines of every class whose Cobertura filename matches `*EmailParsingSorting\SortEmail*.cs` (one merged class per file after `Merge-CoberturaClassesByFilename`) and derives, from the source text of T, four content-identified exemption sets: (1) lines containing `System.IO.Directory.CreateDirectory(path)` (the unchanged #945 wrapper lambda); (2) a line whose trimmed text is `}` and whose three preceding trimmed lines are `else`, `{`, `throw;`; (3) a line whose trimmed text is `}`, which directly follows a set-(2) line, whose next line is either a line starting with `catch (System.Exception)` (the baseline shape) or the method's closing brace (trimmed `}` at indentation eight, the final shape), and whose nearest preceding `catch (` line at the same indentation is `catch (System.UnauthorizedAccessException e)`; (4) the `}` that directly follows the first `throw;` after the guard condition line, where the guard condition line is the single line containing `isRetryAfterClear` that contains neither `bool isRetryAfterClear` nor `isRetryAfterClear:` (zero such lines at baseline, so set (4) is empty there). Every uncovered line of the family that is not in the T exemption union is printed as `NONEXEMPT-UNCOVERED <file>:<line> :: <trimmed source text>`; the sorted set of `<repository-relative path>::<trimmed text>` entries is hashed (`NONEXEMPT-SET-SHA256:`). The gate is that the final hash equals the baseline hash recorded by P0-T11 (the same statements, wherever their line numbers moved; predicted single entry: the `ForEachAsync` statement of M that was already uncovered at baseline), plus `EXEMPT-GUARD-BRACE-COUNT: 1` at final, plus an in-memory negative control that treats the lowest-numbered covered non-exempt line of T as uncovered and must change the hash (`CONTROL-DIFFERS-BASELINE: True`). Per-member coverage for the ten AC25 members is measured by `CMD-MEMBER-COVERAGE` over source spans that begin at a unique signature line and end at the first following line that is exactly eight spaces and `}`; the EfcDataModel changed lines are gated by name (`result = await InvokeFilerAsync(config, mailHelpers);`, `ResetFilerPromptState();`, `return result;` each with hits above zero). Package-level and repository-level rate comparisons are observations, except the AC25 clause that the first-party line and branch percentages are not lower than baseline, which is gated as printed to two decimals.
- PD-9 (inherited paths). Clause A: every path already changed relative to `MERGE-BASE:` when P0-T3 captures it (the orchestrator's preparation commits under FEATURE/, the promoted record docs/features/potential/promoted/2026-10-01-sort-email-latent-logic-defects.md when listed, and the P0-T1 and P0-T2 artifacts), recorded as `INHERITED-CLAUSE-A:`. Clause B: every path under .claude/agent-memory/. Footprint gates subtract Clause A and Clause B, record the subtraction, and never subtract a Write Set path.
- PD-10 (evidence names). The spec's Test Strategy names fixed, digit-free artifact files; this plan uses them verbatim except the fail-before exception dossier, which the evidence conventions require to match `fail-before-exception.*.md`: it is written as FEATURE/evidence/regression-testing/fail-before-exception.<TS>.md and realizes the spec's "fail-before exception dossier named in Test Strategy" (the spec's `fail-before-exceptions.md` spelling does not match the mandatory search pattern; reported to the orchestrator in the handoff). Every other artifact is `<task-id>-<name>.<TS>.md`.
- PD-11 (AC6 and AC27 and the pull request). AC27 and the pull-request clause of AC6 describe the PR body, which this plan does not author. P6-T15 writes FEATURE/evidence/other/pr-description-inputs.<TS>.md with the closing references, the four behavior changes and the UT5 call-out verbatim, for the orchestrator's pr-author step. AC6 is checked off on its plan-side clauses with that artifact as the PR-clause evidence (P6-T21); AC27 is left unchecked by P6-T42 with a `DEFERRED TO PR STEP` record, and P6-T43 expects exactly that state (twenty-six checked, AC27 unchecked). The plan outcome is reported as complete with AC27 deferred, never as PASS over an unchecked AC.
- PD-12 (spec wording). The planner amended FEATURE/spec.md before handoff: "after the seam commit" became "after the seam step" at the fail-before-write-csv.md bullet and in AC15, with one revision-note line inserted at line 12 (the orchestrator's permission in the delegation prompt). The AC section heading therefore sits at line 627 and AC1 to AC27 at 628 to 654.
- PD-13 (uncompiled file deletion). TD is not compiled by any project (ToDoModel/ToDoModel.csproj lists no `Compile Include` for it; the only `SortItemsToExistingFolder` entries in any project file are the two ToDoModel.Test test files, ToDoModel.Test/ToDoModel.Test.csproj lines 75 to 76), so its deletion cannot change a build and needs no project-file edit (D12).

## Execution Conventions

- `FEATURE` denotes docs/features/active/2026-10-01-sort-email-latent-logic-defects-959. `WORKTREE` denotes the absolute item-worktree root supplied by the delegation prompt, without a trailing separator. Both tokens are expanded by the executor; `WORKTREE` is never written into any artifact. Every `git` command in this plan is written in its canonical form and is issued as `git -C WORKTREE <arguments>`; `Command:` fields record the canonical form. Uppercase placeholder tokens (`PATHS`, `TOKENS`, `TASKID`, `FILTERARG`, `ASSEMBLY`, `PROJECT`, `DLL`, `STAGE`, `GATEARGS`, `EXCLUSION`, `INHERITED`, `MERGE-BASE`, `PATH`, `PROJ`, `ENTRIES`, `BASELINE-HASH`) are substituted by the executor as each task states; no shell variable survives a task boundary, so a recorded value is substituted as text.
- **No commits by this plan.** This plan runs no `git add`, `git commit`, `git checkout`, `git reset`, `git stash`, `git merge`, `git rebase`, `git rm` or `git update-index`. The only network git command is the single `git fetch origin main` of P0-T3. Every diff gate is a two-dot comparison against the recorded `MERGE-BASE:` paired with a `git status --porcelain` listing, so each gate is valid whether or not the orchestrator later commits.
- **Listings.** Every file this plan creates or rewrites wholesale is given verbatim in the Listings section as an indented block: each listing line is the file line prefixed by exactly four spaces. The executor strips exactly four leading spaces from every line, writes an empty file line for an empty listing line, and ends the file with one newline. No listing line is paraphrased, reordered or completed by judgment.
- **Edits.** Every Edit is given in the Edit Specifications section as an `OLD` block and a `NEW` block (same four-space convention). The executor passes the stripped OLD text as the Edit tool's old string and the stripped NEW text as the new string, once, in the named file. An Edit whose old text is not found exactly once is `EDIT ANCHOR NOT UNIQUE`: stop and report; never adjust the anchor by judgment.
- **Evidence paths.** Every artifact is written under FEATURE/evidence/baseline/, FEATURE/evidence/regression-testing/, FEATURE/evidence/qa-gates/ or FEATURE/evidence/other/. Committed test evidence is limited to the JaCoCo package projection, the one-line first-party summary and TRX-derived summaries (CLAUDE.md "Committed Test Evidence Format"); no trx, Cobertura, collector or `.coverage` document is copied into FEATURE/.
- **Artifact filenames.** Fixed names (spec Test Strategy): baseline/phase0-instructions-read.md, baseline/test-run-baseline.md, baseline/coverage-baseline.md, regression-testing/fail-before-save-case.md, regression-testing/fail-before-cleanup-files-phase-one.md, regression-testing/fail-before-write-csv.md, regression-testing/fail-before-try-save-retry.md, regression-testing/compile-red-attachment-saving-seams.md, regression-testing/fail-before-redirect-save-folder.md, regression-testing/fail-before-efc-filer-cleanup.md, regression-testing/pass-after-regression-tests.md, qa-gates/toolchain-final-pass.md, qa-gates/coverage-post-change.md, qa-gates/coverage-comparison.md, qa-gates/negative-controls.md. The dossier is regression-testing/fail-before-exception.<TS>.md (PD-10). Every other artifact is `<task-id>-<name>.<TS>.md`, where `<TS>` is the write time in `yyyy-MM-ddTHH-mm` and equals the artifact's `Timestamp:` field; a later task locates it with the glob `<task-id>-<name>.*.md`, which must match exactly one file (the highest `ITERATION:` when the Phase 6 loop restarted).
- **Artifact fields.** Every command-step artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. `ExpectedExitCode:` is written only where a task says so, once per artifact, equal to the observed value it explains. An artifact that records several commands names the invocation its `EXIT_CODE:` row is scoped to and records the others as named `Output Summary:` lines. A pass-after section appended to a fail-before artifact records its own run under `PASS-AFTER-VSTEST_EXIT_CODE:`; the artifact's `EXIT_CODE:` row stays scoped to the red run.
- **Command channel.** Every payload is run as `pwsh -NoProfile -Command '<payload>'`: outer single quotes, the payload's lines joined by `; `, first statement `Set-Location -LiteralPath "WORKTREE"`. No payload contains a single-quote character (`[char]39` supplies one); every string literal is double-quoted; a double quote inside one is written `[char]34` or doubled; no double-quoted literal ends with a backslash before its closing quote. A .NET static file API is never given a relative path (payloads use cmdlets with `-LiteralPath`, which follow `Set-Location`). A child's exit code is read from `$LASTEXITCODE`. A `pwsh` payload is also the only route to a line count, a hash or a token census; the Grep tool is used only where a task names it.
- **Long-running payloads.** `CMD-COVERAGE-DIRECT` (P0-T11, P6-T7) and `CMD-REBUILD` may run longer than a ten-minute foreground call. The executor starts them as background invocations of the same `pwsh -NoProfile -Command '<payload>'` form and polls the captured stdout with read-only calls until the payload's final line appears. No sleep is added. A run still in progress 120 minutes after start is `RUN STALLED`: stop and report.
- **File hashes.** Every SHA-256 is the `Hash` property of `Get-FileHash -Algorithm SHA256 -LiteralPath <repository-relative path>`; the `Path` property is never recorded.
- **Tool resolution.** vswhere.exe is `Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"`; vstest.console.exe is `& $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1`; MSBuild.exe is `& $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1`. `Command:` records the CLAUDE.md-canonical `msbuild ...` form with the note `resolved through vswhere`.
- **MSBuild switches.** Every msbuild invocation carries /nodeReuse:false and a normal-verbosity file logger `/flp:LogFile=coverage\logs\TASKID.msbuild.log;Verbosity=normal`; neither changes which targets run or which diagnostics are reported. `Command:` records the canonical command plus the notes `plus /nodeReuse:false` and `plus a normal-verbosity file logger`. The analyzer and nullable gates use `/t:Rebuild` and never add a Nullable property override; the intermediate project builds of `CMD-BUILD-TEST` use `/t:Build` because they are not gates (their observation is the `0 Error(s)` line and the advanced output assembly).
- **Test runs.** Every direct run uses scripts/vscode/TaskMaster.cli.runsettings (Workers 0 at line 5, Scope ClassLevel at line 6) with `/InIsolation`, an explicit results directory under coverage\test-results\959\ and a quoted trx logger with an explicit file name. vstest.console.exe exits 0 on a zero-match filter, so every scoped run asserts its expected `total`. No test is retried, serialized or edited to pass; a failure is reported with its TRX message. The four shell-icon classes are excluded from the coverage runs on this workstation by the `EXCLUSION` text P0-T9 fixes (CI covers them).
- **Token census.** Every occurrence count is produced by `CMD-CENSUS`, which removes ALL whitespace from each file and counts case-sensitive, non-overlapping occurrences of each token with `[regex]::Matches($content, [regex]::Escape($t)).Count`. Census tokens are written without whitespace; counts include comments and string literals; the comments in the Listings were written so that they repeat no census token. `Select-String` is never used for a count.
- **Line endings.** .gitattributes declares `* text=auto` and .editorconfig sets `end_of_line = crlf`. The Write tool may write LF; the scoped CSharpier pass of each phase normalizes endings, and every content gate is whitespace-insensitive. Numstat gates are unaffected because `text=auto` normalizes endings in `git diff`.
- **Artifact hygiene.** Before text is written into an artifact, an absolute path is replaced by `<repo-root>` (or `<user-profile>`), the account name by `<user>` and the machine name by `<host>`. The sandbox literals `C:\Sortemail959Sandbox` (new tests), `C:\Sortemail956Sandbox` (TST2) and `C:\Sortemail945Sandbox` (TST1) are not host paths and are recorded as written.
- **Sandbox literals.** No code in this plan creates any sandbox directory. Every `CMD-VSTEST` run prints `SANDBOX-EXISTS-BEFORE` and `SANDBOX-EXISTS-AFTER` for the three roots from read-only `Test-Path` calls; any `True` before a run is `SANDBOX PRESENT` and any `True` after a run is `SANDBOX CREATED BY RUN`: stop and report.
- **Expect-fail wrong-reason branch.** Every `[expect-fail]` run names the expected failing rows and a substring each failure `MESSAGE` must contain. A failure whose message lacks the substring, or a failing row outside the named set, is `FAIL-BEFORE WRONG REASON`: stop and report the messages; never adjust a test or a gate to match.
- **Stop discipline.** A named stop string in a task means: stop at that task, write the artifact with the observed values, and report the string verbatim. The executor never edits a test, a gate, a listing or a policy file to make a task pass, and never re-runs a test to obtain a different outcome. A toolchain-loop restart (Phase 6 loop rule) is the only repetition this plan allows.
- **Scope rule (issue.md line 35, binding).** Any defect found during execution in the same files or with the same root cause is reported to the orchestrator for inclusion, never listed as a follow-up; only a completely unrelated defect is reported for filing. The executor does not widen the write set on its own.

## Verified Repository Facts (re-derived in this pass with Read, Grep and Glob against WORKTREE at 94287369908cc920b21b0e3256314f988ad7d2f5)

1. A (343 lines; `#nullable enable` line 1; eighteen using lines 2 to 19; `namespace UtilitiesCS` 21; partial class 23). Fields `_responseSaveFile`, `_attachmentsOverwrite`, `_attachmentsAltName`, `_picturesOverwrite` at 25 to 28; `Cleanup_Files` 30 to 36 resets three fields and calls `RemoveReadOnlyPrompt.Reset()` at 35 (no `_attachmentsAltName` reset); `[ExcludeFromCodeCoverage]` at 38, 63, 103, 164, 227, 241, 290, 311, 322, 333 (ten); `YesNoToAll.ShowDialog(` call expressions at 112, 135, 175, 198, 255 (five; 258 is a comment); `SaveAttachment` 103 to 162; `SaveAttachmentAsync(this AttachmentHelper)` 164 to 225 with the two-argument try-save call at 221; destination overload 227 to 239 setting `FolderPathSave` at 235; `SaveCaseAsync` 241 to 288 (`when` guards at 251 to 252 and 279 to 280; try-save calls 266, 281; `default: await Task.CompletedTask;` 284 to 285); `SaveCase` 290 to 309 with the L1 labels `case (YesNoToAllResponse.NoToAll | YesNoToAllResponse.No):` at 300 and `case (YesNoToAllResponse.Yes | YesNoToAllResponse.YesToAll):` at 303; `IsPicture` 311 to 320 (the only `Path.` user in A); `SaveMessageAsMsgAsync` 322 to 331; `SaveMessageAsMSG` 333 to 340.
2. T (173 lines; same header). `RemoveReadOnlyPrompt` 28 to 30; two-argument wrapper 32 to 51 (comment 32 to 34, documentation 35 to 39, attribute 40, lambda 49); three-argument forward 53 to 73; five-argument core 75 to 160 (documentation 75 to 86, signature 87 to 93, `createDirectory` 97, `Debug.WriteLine` 103, 127, 147, prompt 108 to 113, clear 120 to 133, recursion 134 to 140, No arm 142 to 150, `else { throw; }` 151 to 154, catch closing brace 155, outer `catch (System.Exception) { throw; }` 156 to 159, method brace 160); `ClearReadOnlyAttributeOnDisk` 162 to 170 (attribute 165). `catch (` occurs at 101 (indent 12), 125 (indent 20), 156 (indent 12); `throw;` at 153 and 158.
3. U (196 lines; same header). `UndoAsync` 25 to 80 (attribute 26); `PushToUndoStack` 82 to 92 (attribute 82); `CaptureMoveDetails` 94 to 107 (attribute 94); `SanitizeArrayLineTSV` 109 to 125 (attribute 109); `StripTabsCrLf` 127 to 137 (no attribute); `WriteCSV_StartNewFileIfDoesNotExist` 139 to 170 (attribute 139; `File.Exists(Path.Combine(strFileName, strFileLocation))` 147; header assignments 151 to 163; `SanitizeArray(strAryOutput, ref strOutput);` 165; `FileIO2.WriteTextFile(strFileName, strOutput!, folderpath: strFileLocation);` 166); `SanitizeArray` 172 to 193 (attribute 172; `Debug.WriteLine` 177; `strOutput![j]` 183).
4. S (277 lines) and M (388 lines) carry the same eighteen using lines at 2 to 19; S `logger` 25 to 27; S `x.SaveAttachmentAsync(saveFsPath)` 165; M `x.SaveAttachmentAsync()` 153; M `attachment.SaveAttachment();` 299 (155 and 301 are comments); M `Sort` 247 to 248. L (240 lines): `MAX_PATH` 25; `SaveAttachmentsOld` 27 to 238 (attribute 27).
5. Grep over `*.cs`: `SaveAttachmentsOld|IsPicture\b` matches exactly L 28 and A 312 (declarations; zero callers). `SortItemsToExistingFolder` over `*.csproj` matches only ToDoModel.Test/ToDoModel.Test.csproj 75 and 76 (the two test files); over `*.cs` it matches TD 15 (the class), ToDoModel.Test/Email Utilities/SortItemsToExistingFolderTests.cs 14 and 62 (class name and a comment) and ToDoModel.Test/Email Utilities/SortItemsToExistingFolderTests_Unfinished.cs 12 (class name). `SaveCaseAsync(` matches A 179, 203 and 242 only. `Cleanup_Files()` executable production call: QuickFiler/Controllers/EfcDataModel.cs 309 only (QuickFiler/Legacy/QfcController.cs 792 is not compiled; QfcItemController.MailActions.cs 156 and 188 are comments; TD 391 declares an unrelated member). `SortEmail.WriteCSV_StartNewFileIfDoesNotExist(` TaskMaster/AppGlobals/AppOlObjects.cs 301 only. `attachment.SaveAttachmentAsync(Config.SaveFsPath!)` EmailFiler.cs 445 (through `SaveAttachmentsPicturesAsync` 279 to 282). None of the identifiers `SortEmail_SaveCase_Tests`, `SortEmail_AttachmentSaving_Tests`, `SortEmail_UndoAndMoveLog_Tests`, `EfcDataModelFilerCleanupTests`, `ResetFilerPromptState`, `RedirectSaveFolder`, `AllPromptSessions`, `MovedMailsHeader`, `TrySaveAttachmentCoreAsync`, `AttachmentsOverwritePrompt` or `CreateDirectoryLimit` occurs in any `*.cs` or `*.csproj`.
6. E (465 lines; no `#nullable`). `internal partial class EfcDataModel` 21; `MailInfo => ConversationResolver?.MailHelper` 206; `TryGetArchiveRoot` 245; five-parameter `MoveToFolderAsync` 268 to 311 (guards return false at 278, 289, 294; `var result = await InvokeFilerAsync(config, mailHelpers);` 308; `SortEmail.Cleanup_Files();` 309; `return result;` 310, the only `return result;` in E); `protected internal virtual Task<bool> InvokeFilerAsync` 320 to 326; `internal async Task OpenOlFolderAsync(string folderpath)` 328 (unique); `MAPIFolder` overload 377 to 398 calling the five-parameter overload at 387. Callers of the five-parameter overload: E 387, QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs 98, QuickFiler/Controllers/EfcFormController.Actions.cs 136 (the `MAPIFolder` overload), QuickFiler/Controllers/EfcFormController.EventHandlers.cs 184 (the `MAPIFolder` overload), QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs 316.
7. TST1 (458 lines; namespace `UtilitiesCS.Test.EmailIntelligence` 15; class 33; fifteen `[TestMethod]`; `GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments` 182 to 207; `GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments` 209 to 234; try-save tests 245 to 266 and 272 to 289 calling the three-argument overload at 257 and 281; `SanitizeArrayLineTSV` test 341 to 357; `SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows` 359 to 382 (preceded by a blank line 358); `CreateAttachmentMock` 386 to 406; `System.Action` at 45, 58, 178; no `#nullable`). TST2 (376 lines; class 20; eleven `[TestMethod]` at 33, 57, 84, 110, 140, 165, 193, 217, 245, 273, 297; constants 24 to 27 with `C:\Sortemail956Sandbox\attachments`; `SaveAsync` helper documentation 319 to 322 and body 323 to 331; `Seams` 338 to 373 with `CreateDirectory` 354 to 357 and `ClearException` 352).
8. UtilitiesCS/UtilitiesCS.csproj Compile entries 817 MovedMailInfo.cs, 818 SortEmail.cs, 819 SortEmail.AttachmentSaving.cs, 820 SortEmail.LegacyAttachmentSaving.cs, 821 SortEmail.MailItemSort.cs, 822 SortEmail.TrySaveAttachment.cs, 823 SortEmail.UndoAndMoveLog.cs, 824 FolderPredictor.cs (four-space indent, backslash separators, self-closing). UtilitiesCS.Test/UtilitiesCS.Test.csproj 98 `EmailIntelligence\SortEmail_Tests.cs`, 99 `EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs`, 100 `EmailIntelligence\FilterOlFoldersController_Tests.cs`. QuickFiler.Test/QuickFiler.Test.csproj 127 `Controllers\EfcDataModelArchiveRootTests.cs`, 128 `Controllers\EfcDataModelIssue792CarryTests.cs`.
9. InternalsVisibleTo: UtilitiesCS/Properties/AssemblyInfo.cs 18 to 20 (DynamicProxyGenAssembly2, UtilitiesCS.Test, ToDoModel.Test); QuickFiler/Properties/AssemblyInfo.cs 5 (QuickFiler.Test). UtilitiesCS/Dialogs/YesNoToAll.cs enum 14 to 21 (Empty 0, Yes 1, No 2, YesToAll 4, NoToAll 8), no `[Flags]`. UtilitiesCS/Dialogs/YesNoToAllPromptSession.cs: `internal sealed class` 13; constructor 24; `Response { get; private set; }` 33; `Ask` 40; `ReleaseSingleAnswer` 54; `Reset` 65.
10. UtilitiesCS/OutlookObjects/Attachment/AttachmentHelper.cs: three-argument constructor `(Attachment, DateTime, string)` 34 to 37; `Init` 61 to 124 sets `FilePathSave` 101 and `FilePathSaveAlt` 106 (suffix from `GetNameSuffix()` 327 to 330); `FilePathHelperSaveAlt` internal accessor 174 to 175; `FilePathSave`/`FilePathSaveAlt` 178 to 189; `FolderPathSave` 199 to 203; `CheckParameters` 262 to 298 (fails only for a null attachment or an over-long path). UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs: constructor subscribes `FilePathHelper_PropertyChanged` 23 to 26; `FolderPath` setter 83 to 91; handler recomputes `_filePath = Path.Combine(_folderPath, _fileName)` on `FolderPath` at 360 to 362 and splits `FilePath` into folder and name at 369 to 395. UtilitiesCS/OutlookObjects/Attachment/AttachmentSerializable.cs constructor 24 to 77 reads Type, BlockLevel, Class, DisplayName, FileName, Index, PathName, Position, Size, Application, Parent, PropertyAccessor, Session (a Loose Moq attachment satisfies it, as TST1 shows). `FileIO2.WriteTextFile(string filename, string[] strOutput, string folderpath)` is the single overload (UtilitiesCS/To Depricate/FileIO2.cs 36).
11. QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs (399 lines): eleven `[TestMethod]`; `MoveAsync` 314 to 323; `CreateOlObjects` 330 to 333; `CreateGlobals` 340 to 352; `SpecialFoldersWithOneDrive` 355 to 360; `SpecialFoldersWithoutOneDrive` 363 to 366; `TestableEfcDataModel` 379 to 397 (base constructor with a null mail item, `ConversationResolver` from `QuickFiler.Helper_Classes` with a parameterless `MailItemHelper`); `ArchiveRootLiteral` 35; success path stubs `ArchiveRootPath` with `Returns(ArchiveRootLiteral)` at 179.
12. SPEC956 (docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md): line 99 contains the clause `as merge-base line 944 does, so the exception boundary is unchanged.` (once in the file); line 149 ends `and no retry bound is added (L2 is out of scope).` (once); line 155 is `- The `DirectoryInfo` construction's exception boundary (path computed before the inner `try`).` (once); seventeen lines match `^- \[(x| )\] AC` (recorded, not pinned, by P0-T2 as checked and unchecked counts).
13. scripts/vscode: Install-RepoDotNetSdk.ps1, Invoke-Restore.ps1, Invoke-MSTestWithCoverage.ps1 (parameter block line 1; `Get-DotnetCoverageArgumentList` 41 with the fixed `/TestCaseFilter:TestCategory!=LiveOutlook` 91; `ConvertTo-DerivedCoverageSettingsXml` 97; discovery 348 to 355 still excludes assemblies whose repository-relative path matches `(^|\\)\.claude\\` at 353, so the DIRECT route of #956 is required from this worktree; entry guard 459), Invoke-MSTestWithCoverage.Helpers.ps1 (`Get-CoberturaClassLineSummary` 160; `Merge-CoberturaClassesByFilename` 260; `ConvertTo-KoverageCoberturaXml` 407), Invoke-MSTestWithCoverage.Threshold.ps1 (`Assert-CoberturaLineCoverageThreshold` 3; `Assert-CoberturaBranchCoverageThreshold` 58), Invoke-MSTestWithCoverage.FirstParty.ps1 (`Get-CoberturaFirstPartyCoverageReport` 123), Invoke-MSTestWithCoverage.Projection.ps1 (`ConvertTo-JacocoPackageProjection` 14; `Assert-JacocoProjectionReconciliation` 83), Invoke-MSTest.TrxSummary.ps1 (`Get-TrxRunSummary` 12; `Format-TrxRunSummary` 103). TaskMaster.cli.runsettings is 9 lines (Workers 0 at 5, Scope ClassLevel at 6).
14. Configuration: global.json SDK 8.0.205 with `.dotnet-sdk` path; dotnet-tools.json at the repository root (csharpier 1.2.6); coverage.config present; .csharpierignore excludes `**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`, `*.csproj`, `*.props`, `*.targets`, `**/packages.config`, `**/app.config`; .gitignore 57 `artifacts/`, 146 `*.trx`, 147 `*cobertura*.xml`, 150 `coverage/*`, 151 `!coverage/.gitkeep`, 357 `.dotnet*/`. Analyzer wiring matches packages.config in all four projects in scope: Meziantou.Analyzer 3.0.290, Roslynator.Analyzers 5.0.0, AsyncFixer 2.1.0, Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0, SonarAnalyzer.CSharp 10.34.0.3385, MSTest.Analyzers 4.4.1 (test projects).
15. The four stall-probe classes exist: UtilitiesCS.Test/HelperClasses/ShellUtilities_Tests.cs, ShellUtilitiesStatic_Tests.cs, SysImageListHelperTests.cs, UtilitiesCS.Test/EmailIntelligence/OSBrowser_Tests.cs. The #956 run on this workstation observed `STALL-PROBE: REPRODUCES` (one failing shell-icon test) and ran the coverage route with the four classes excluded; its final coverage read `First-party coverage: lines 56202/65845 (85.36%), branches 13618/17076 (79.75%)` and the SortEmail family had four uncovered lines: T 49 (lambda), T 154 (else brace), T 155 (catch brace) and M 153 (`await attachments.ForEachAsync(async x => await x.SaveAttachmentAsync());`). These are prediction anchors, not gates.
16. artifacts/orchestration/orchestrator-state.json (git-ignored): `route_id` preparation (3), `issue-num` 959 (10), `work-mode` full-bug (14), `feature-folder` (15), `plan-path` (16), `lifecycle_ready` true (17).

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

### Listing L-TSC-FINAL (UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs, final state, written by P4-T1)

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
            private static Func<Attachment, string, Task<bool>> RecordingSave(List<string> saves)
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

### Listing L-TAS-FINAL (UtilitiesCS.Test/EmailIntelligence/SortEmail_AttachmentSaving_Tests.cs, final state, written by P4-T2)

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
            private static Func<Attachment, string, Task<bool>> RecordingSave(List<string> saves)
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

<!-- APPEND-POINT -->
