# Policy Compliance Audit: sort-email-latent-logic-defects (Issue #959; the PR also closes #966) — re-review after Phase 7

- Artifact: `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/policy-audit.2026-10-06T17-40.md`
- Review label: `2026-10-06T17-40`. This is not a host-clock reading. The review ran without a shell (caller directive), so the label was chosen strictly after the last reflog epoch of the item worktree (`1791321953` = 2026-10-06T17:25:53 -0400, the P7-T16 commit `2d7e338f4`) and before this file was written. Every evidence label, commit epoch and Cobertura root epoch cited below sorts before it.
- Branch: `bug/sort-email-latent-logic-defects-959` at HEAD `2d7e338f4` (clean and pushed per the caller; the reflog's last entry is that commit, 124 entries, zero `merge` entries).
- Base: merge-base `94287369908cc920b21b0e3256314f988ad7d2f5` (origin/main at branch cut). It equals the first reflog entry of the item worktree and the Phase 7 footprint gate's `ANCHOR-RECHECK` (evidence/qa-gates/p7-t15-scope-boundary.2026-10-06T17-23.md). Phase 8 (origin/main reconciliation) has not run; no merge from main is in the branch.
- Work mode: `full-bug` (issue.md line 12). AC source: spec.md `## Acceptance Criteria` only (AC1 to AC27 at lines 628 to 654).
- Prior review: policy-audit, code-review and feature-audit `.2026-10-06T15-30.md` at HEAD `0fab75ed6` (PASS, zero blocking, CR-1 to CR-7, G-1 to G-7, U-1 to U-3). This re-review evaluates the whole branch at HEAD `2d7e338f4`, confirms the Phase 7 closure of CR-1 and CR-3, and carries every still-open item.
- Scope: the full branch diff against the merge base. Code and project paths: 17 (the caller-supplied full code diff, Grep `^diff --git a/`, cross-checked against the worktree by Read and Grep). Documentation paths: this feature folder (spec, plan, research, 103 evidence files per the P7-T16 sweep, the three prior review artifacts), the #956 spec.md, the promotion record under docs/features/potential/promoted/, and the `.claude/agent-memory/` Markdown paths the footprint gate subtracts under its Clause B (30 paths per P7-T15). No plan, task or file subset narrowed the audit (see Rejected Scope Narrowing).
- Reviewer tool surface: Read, Grep, Glob, Write and the orchestration validator MCP. No Bash, so no command was re-run; every figure below is read from the worktree, the two Cobertura documents on disk, the caller-supplied diffs or the committed evidence projections, each cited where used.

## Executive Summary

| Item | Result |
| --- | --- |
| Compliance verdict | PASS |
| Blocking findings | 0 |
| Non-blocking findings | 6 open (CR-2, CR-4 to CR-7 carried; CR-8 new) in code-review.2026-10-06T17-40.md, plus the gaps and observations of section 8 |
| Closed since the prior review | CR-1 (synchronous image arm now pinned by SS4; Cobertura line 143 condition 100% (2/2)) and CR-3 (unused `using System;` removed); G-4 (the P6-T13 doc-comment edit is now covered by a full toolchain pass, P7-T6 to P7-T11) |
| Acceptance criteria | 25 of 27 PASS; AC6 and AC27 pending the pull-request body (pr-author step), PENDING rather than FAIL per the caller's directive |
| C# first-party line coverage | 85.39% post-change (56410/66058) against 85.36% baseline (56212/65855); floor 85% per .claude/rules, 80% per CLAUDE.md UT2: PASS |
| C# first-party branch coverage | 79.81% post-change (13680/17141) against 79.75% baseline (13620/17078); floor 75%: PASS |
| C# new or changed code | 98.1% lines (158 of 161 valid lines across the nine members named by AC25; the three uncovered lines are braces after a rethrow), 95.9% branches (47 of 49; the two uncovered arms are rethrow fall-throughs) |
| Toolchain | One clean Phase 7 pass of the four CLAUDE.md C# commands on 2026-10-06 (P7-T6 to P7-T11; ITERATION 2, LOOP-RESTARTS 0), both rebuilds with CoreCompile not skipped on all four projects; it covers every source edit on the branch including the P6-T13 doc comment and the P7-T2 and P7-T3 edits |
| Tests | 7394 of 7394 passed in the final coverage run (baseline 7361; +33 rows exactly accounted for: the +32 of Phase 6 plus SS4) |

The branch fixes the four logic defects (L1 to L4), the three same-root-cause items (EfcDataModel `finally`, the ToDoModel duplicate, the CSV header), the re-rooting defect and the six #966 residuals, each with red-first evidence or a documented exception, inside an eighteen-path write set. Phase 7 changed two test files only (one added test, one removed directive), re-ran the full toolchain and rewrote the three fixed-name projections at ITERATION 2 with `SUPERSEDES:` commit references. The code is compliant with the general and C# code-change and unit-test policies. The one per-file reading below a floor is the pre-existing sub-floor file `EfcDataModel.cs` (unchanged since the prior review, improved against baseline, every changed line hit); it does not block.

## Rejected Scope Narrowing

None detected. Three caller statements were examined and classed as not narrowing:

1. "Phase 7 delta since the initial review (code + evidence, excluding plan and agent-memory)". This describes the contents of one input file, not a limit on the audit; the plan and the footprint record of the agent-memory paths were read from the worktree and are covered in sections 2 and 8 (CR-8, G-9).
2. The caller's statement that Phase 8 (origin/main reconciliation) "has not run yet" and lies beyond this review. Phase 8 has produced no commit, so there is nothing on the branch to audit; the statement limits nothing that exists.
3. "The four shell-icon test classes are excluded locally by the plan's stall-probe rule" (carried from the prior review). This describes the coverage run's test filter (the same filter at baseline and post-change, evidence/baseline/p0-t9-stall-probe.2026-10-03T08-29.md), not a limit on which files this audit covers; the four classes touch no changed file.

The audit scope is the full branch diff against the merge base.

## Evidence Location Compliance

- Branch diff paths under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`: none. The footprint gate's path set at HEAD (P7-T15, `FOOTPRINT-PATHS: 151`, `OUTSIDE-WRITE-SET: 0`, `RAW-DOC-PATHS: 0`) consists of the eighteen write-set paths, paths under this feature folder, the 27 inherited Clause A paths and the 30 `.claude/agent-memory/` Clause B paths; no path under `artifacts/`.
- Evidence produced by the executor lives under `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/{baseline,qa-gates,regression-testing,other}/` (CMD-EVIDENCE-FIELDS `EVIDENCE-FILES-CHECKED: 94`, `NONCANONICAL-SUBFOLDER-FILES: 0` in p7-t16-phase7-closure.2026-10-06T17-25.md; the eleven Phase 7 artifacts were each opened by Read or in the delta diff and sit under qa-gates/ or regression-testing/).
- Raw documents: no `.xml`, `.trx` or `.coverage` path in the diff (`RAW-DOC-PATHS: 0`; CMD-SWEEP `RAW-DOCUMENT-FILES: 0` over 103 files). Committed test evidence is the JaCoCo package projection plus the one-line first-party summary (coverage-baseline.md, coverage-post-change.md ITERATION 2) and TRX-derived summaries, as CLAUDE.md "Committed Test Evidence Format" requires. `artifacts/csharp/coverage.xml` and the `coverage/*959*` documents are on disk and git-ignored (Glob lists them; the footprint gate does not).
- `validate_evidence_locations.py --root .` was not run in this review (no shell); the check above was made from the enumerated diff paths and the footprint record.
- This review writes only the three audit artifacts into the feature folder root. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` event: the caller supplied canonical paths only.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles (UT1 to UT5)

| Requirement | Verdict | Evidence |
| --- | --- | --- |
| Independence and isolation | PASS | Every new test owns its `YesNoToAllPromptSession` instances, recording delegates and Moq mocks; no test reads or writes a production session (P7-T4 census: `SetValue(` 0, `SortEmail.Cleanup_Files();` 0 in SortEmail_AttachmentSaving_Tests.cs; confirmed by reading the file in full, 469 lines). The new SS4 test (lines 299 to 329) builds its own helper, pictures and attachments sessions. The structural test reads static state only. Every scoped and full run passed under scripts/vscode/TaskMaster.cli.runsettings (Workers zero, class-level scope; `RUNSETTINGS-HASH-NOW` equal to P0-T4 in every P7-T5 and P7-T10 run). |
| Determinism | PASS | Grep of the Phase 7 delta's added lines and the P7-T4 census for `DoNotParallelize`, `Thread.Sleep`, `Task.Delay`, `Timeout`, `DateTime.Now`: zero hits. The one fixed date is a literal `new DateTime(2026, 4, 3, 9, 30, 0)`. The L2 test ends an unbounded loop with a call-count tripwire (`CreateDirectoryLimit`), not a timer. |
| Fast execution | PASS | 56 SortEmail-family rows and 3 EfcDataModel rows run in-process with mocks; no I/O. |
| Readability and intent | PASS | SS4 carries a `/// <summary>` naming scenario, expectation and the CR-1 provenance, plus the Arrange, Act, Assert markers (read); the earlier tests were read in the prior review. |
| No temporary files or external dependencies (UT4) | PASS | Sandbox roots C:\Sortemail959Sandbox, C:\Sortemail956Sandbox and C:\Sortemail945Sandbox reported absent before and after every Phase 7 run (`SANDBOX-*-EXISTS-*: False` in p7-t5-attsave-run, pass-after-regression-tests ITERATION 2 and coverage-post-change ITERATION 2). P7-T4 census: `Directory.CreateDirectory`, `File.`, `MemoryAppender` 0 in both edited files. |
| Scenario completeness (UT2) | PASS | SaveCase: No, NoToAll, Yes, YesToAll, Empty. SaveCaseAsync: seven scenarios. Async core: absent file, image prompt, document prompt, Yes released, YesToAll kept, No then Yes to alternate. Sync core: absent file, Yes, NoToAll, and now the image arm (SS4). Re-rooting, structural cleanup, two CSV cases, three EfcDataModel cases, T12 retry bound. The prior gap (CR-1) is closed. |
| Test file location | PASS with repo convention | Tests live in `UtilitiesCS.Test/EmailIntelligence/` and `QuickFiler.Test/Controllers/`, the repository's established `<Project>.Test/` layout (pre-existing convention; the `tests/` mirror of .claude/rules/general-unit-test.md is not used anywhere in this solution). No test was colocated with production source. |
| UT5 call-out | PASS | The phase-one reflective-write test no longer exists in the tree (`Cleanup_Files_ResetsEveryPromptAnswerField` 0 in the P7-T4 census). The UT5 call-out is in spec.md Test Strategy and in evidence/qa-gates/pr-description-inputs.2026-10-06T15-12.md for the PR body; the Phase 7 addendum in p7-t16-phase7-closure records that SS4 is coverage of behaviour that was already correct, so no fail-before run applies to it. |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `coverage/baseline-959.cobertura.xml` (git-ignored, present on disk in the item worktree, Glob; class nodes read directly; committed projection `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/coverage-baseline.md`)
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml` (root `lines-covered="56410" lines-valid="66058" branches-covered="13680" branches-valid="17141"`, `timestamp="1791321467"` = 2026-10-06T17:17:47 -0400, equal to the FINAL-FIRST-PARTY counters of `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/coverage-post-change.md` ITERATION 2; the caller states it is a hash-identical copy of `coverage/final-959.cobertura.xml` from the P7-T11 run)
- TypeScript baseline coverage artifact: none consulted (zero TypeScript files changed on this branch)
- TypeScript post-change coverage artifact: none consulted (zero TypeScript files changed on this branch)
- PowerShell baseline coverage artifact: none consulted (zero PowerShell files changed on this branch)
- PowerShell post-change coverage artifact: none consulted (zero PowerShell files changed on this branch)
- Python baseline coverage artifact: none consulted (zero Python files changed on this branch)
- Python post-change coverage artifact: none consulted (zero Python files changed on this branch)
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.36% lines (56212/65855), 79.75% branches (13620/17078). Post-change: 85.39% lines (56410/66058), 79.81% branches (13680/17141). Change: +0.03% lines and +0.06% branches, no changed line lost coverage. New/changed-code coverage: 98.1% lines, 95.9% branches. Disposition: PASS. Evidence: coverage/baseline-959.cobertura.xml and artifacts/csharp/coverage.xml read at the class, method and line nodes, with the committed projections coverage-baseline.md, coverage-post-change.md (ITERATION 2) and coverage-comparison.md (ITERATION 2) under the feature folder's evidence subfolders.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

Languages with changed files on the branch: C# only (`.cs` and `.csproj`; every other diff path is Markdown). The C# repo-wide figure is the first-party figure the executor's route reports, which equals the Cobertura root counters because `coverage/effective-coverage-959.config` excludes only third-party modules (`Deedle`, `FSharp`, `Castle.Core`, `FluentAssertions`, `Moq`, `Microsoft.Testing`, `MSTest`) and `.*\.Test\.dll$`; no production module and no production source path is excluded, so the Coverage Exclusion Policy of .claude/rules/general-unit-test.md is met (no `exclude` entry matches a production path). The four shell-icon test classes are filtered from the run at baseline and post-change alike; they touch no changed file.

Floors applied: .claude/rules/general-unit-test.md and quality-tiers.md (85% line, 75% branch, no regression on changed lines); CLAUDE.md UT2 (80% line, 75% branch on the testable denominator, 90% for new members); spec D16 and AC25 (the CLAUDE.md floors plus 90% per named member). The first-party figures clear every floor. Per-file and per-member readings are in section 5.

Phase 6 to Phase 7 comparison (recorded, not gated, per P7-T12): at two decimals the Phase 7 first-party rates equal the Phase 6 rates (85.39 and 79.81). The branch numerator reads 13680 against 13681 at Phase 6 (UtilitiesCS BRANCH 9494/11356 against 9495/11356) while the SortEmail family gained one covered arm (line 143 of SortEmail.AttachmentSaving.cs, 50% (1/2) to 100% (2/2)); a one-branch difference outside the family offsets it. See G-8.

Coverage Metrics by Language:

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
| --- | --- | --- | --- | --- | --- | --- |
| C# | 17 (5 modified production files, 2 deleted production files, 3 project files, 3 modified test files, 4 new test files) | 7394 in the final full run (7361 at baseline) | 7394 passed, 0 failed, 0 skipped | 85.36% lines, 79.75% branches | 85.39% lines, 79.81% branches | 98.1% lines, 95.9% branches |
| TypeScript | 0 | 0 | none run (zero files changed) | N/A | N/A | N/A |
| PowerShell | 0 | 0 | none run (zero files changed) | N/A | N/A | N/A |
| Python | 0 | 0 | none run (zero files changed) | N/A | N/A | N/A |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
| --- | --- | --- |
| Clarify objective, read plans, document the plan | PASS | spec.md revision 0.2 (D1 to D18), plan.2026-10-02T05-07.md revision 2.0 (Phases 7 and 8 appended in place; P7-T1 to P7-T16 all `[x]`, P8-T1 to P8-T13 `[ ]`), two research records, preflight clearance under evidence/other/. The plan's `- **Status:**` line (line 7) still reads "P7-T1 to P7-T16 ... pending" although every P7 task box is checked and committed: CR-8, non-blocking. |
| Bugfix workflow: failing regression test first | PASS | Six red-first artifacts under evidence/regression-testing/ each carry `EXIT_CODE: 1` with `ExpectedExitCode: 1` and name the failing rows (fail-before-save-case.md, fail-before-cleanup-files-phase-one.md, fail-before-write-csv.md, fail-before-try-save-retry.md, fail-before-redirect-save-folder.md, fail-before-efc-filer-cleanup.md). Refactor-only steps carry the eight-entry dossier fail-before-exception.2026-10-03T12-33.md and the compile-red record. The structural test received a mutation control (negative-controls.md). SS4 (Phase 7) pins behaviour that was already correct and is disclosed as such (p7-t16-phase7-closure.2026-10-06T17-25.md, PR-body addendum); its false-before state is the coverage reading `A-LINE-143-CONDITION: 50% (1/2)` recorded by P7-T1 before the edit and `100% (2/2)` after (P7-T13), which this review confirmed at the Cobertura node (document line 59802). |
| Minimal, targeted fix | PASS | Each defect's fix is the smallest change named in the spec decision. Phase 7 added one 32-line test and removed one directive; no production line changed (P7-T15 numstat rows unchanged from P6-T12; SortEmail family class rows identical to Phase 6 in coverage-comparison.md ITERATION 2). |
| Design principles (simplicity, reuse, extensibility, separation of concerns) | PASS | Prompt state moved from four hand-rolled enum fields to the existing `YesNoToAllPromptSession` type; cores take `Func<string, bool>`, sessions and a named delegate so logic is testable without disk or dialog; wrappers are one-statement forwards passing method groups. The named `TrySaveAttachmentDelegate` is justified in code (CS1769 over an embedded interop type). |
| Classes and functions | PASS | New members are small static helpers on the existing static partial class and one virtual seam on `EfcDataModel`; no new type beyond the delegate. |
| Error handling, logging, contracts | PASS | The no-op outer `catch (System.Exception) { throw; }` is removed; the handler logs through the class log4net logger at Warn and Error with the exception object; the new guard rethrows with a bare `throw;`. `EfcDataModel.MoveToFolderAsync` resets prompt state in a `finally` whose callee cannot throw (EfcDataModel.cs lines 308 to 321 and 344 to 347 read). `Debug.WriteLine` is gone from the family. |
| Module and file structure (500-line limit) | PASS | Twelve C# files in the write set, MAX-LINES 488 (P7-T4 CMD-LINES). SortEmail_AttachmentSaving_Tests.cs grew to 469 (Read confirmed 469 lines); SortEmail_SaveCase_Tests.cs 342; EfcDataModel.cs 485 and SortEmail_Tests.cs 488 are the closest (CR-4). |
| Naming, docs, comments | PASS | Descriptive names following the #956 `RemoveReadOnlyPrompt` precedent; `/// <summary>` on every new member and test; "why" comments on the property-not-field choice, the retry bound, the delegate type and the `finally`. |
| Dependencies | PASS | None added; csproj edits are Compile Include lines only (UCS 0/1, UCT 3/0, QFT 1/0 numstat, P7-T15). |
| I/O boundaries | PASS | Real `File.Exists`, `FileIO2.WriteTextFile`, `YesNoToAll.ShowDialog` and `Directory.CreateDirectory` are reached only from `[ExcludeFromCodeCoverage]` one-statement wrappers; every core is exercised with delegates. |
| Match existing style; no breaking public API | PASS | Public signatures unchanged (prior review verified every call site; no production file changed in Phase 7). |
| Toolchain loop | PASS | toolchain-final-pass.md ITERATION 2 (P7-T14): format (WRITESET-CHANGED-COUNT 0, PORCELAIN-SAME True), check (1639 files), analyzer rebuild and nullable rebuild (SKIP_CORECOMPILE_LINES 0, four CSC output lines of 2 each, 0 warnings, 0 errors), scoped runs 56/56, 3/3, 11/11, full coverage run 7394/7394; LOOP-RESTARTS 0. This pass post-dates every source edit on the branch (last source commit P7-T3 at epoch 1791320894; P7-T6 to P7-T11 commits at 1791321113 to 1791321588), so the prior review's G-4 is closed. |
| Supporting documents updated | PASS with one drift | #956 spec.md: three CR-1 edits exactly (numstat 4/2). AC check-offs current (25 checked, AC6 and AC27 deferred). The plan's P7 task boxes are checked; its Status line is stale (CR-8). pr-description-inputs received the Phase 7 addendum in p7-t16-phase7-closure. |
| Clear next steps | PASS | pr-description-inputs.2026-10-06T15-12.md plus the Phase 7 addendum carry the closing references, the four behaviour changes, the UT5 call-out, the SS4 test name, the new totals and the CR-2/U-2 filing note; Phase 8 (P8-T1 to P8-T13) is specified and gated on this re-review. |

## 3. Language-Specific Code Change Policy Compliance

C# is the only language with changed files.

| Requirement (CLAUDE.md C#1 to C#7) | Verdict | Evidence |
| --- | --- | --- |
| CSharpier through `dotnet tool run`, format then check | PASS | p7-t6-csharpier-format.2026-10-06T17-11.md (exit 0, Formatted 1639 files, no write-set hash changed) and p7-t7-csharpier-check.2026-10-06T17-12.md (exit 0, Checked 1639 files); the two Phase 7 edits were also scoped-formatted with BEFORE and AFTER hashes equal (p7-t4). |
| Analyzer rebuild (`/t:Rebuild`, analyzers and code style enforced) | PASS | p7-t8-msbuild-analyzers.2026-10-06T17-13.md, exit 0, 0 warnings, 0 errors, CoreCompile not skipped on UtilitiesCS, UtilitiesCS.Test, QuickFiler, QuickFiler.Test. |
| Nullable rebuild (`/p:TreatWarningsAsErrors=true`, no `/p:Nullable=enable`) | PASS | p7-t9-msbuild-nullable.2026-10-06T17-14.md, exit 0, same CoreCompile check, no Nullable property override. All five surviving partials keep `#nullable enable` on line 1 (prior review Read; unchanged in Phase 7). |
| Strong contracts, explicit types at public boundaries | PASS | New members are `internal` or `private`; the one new `protected internal virtual` member sits on an internal class; parameters are typed delegates and sessions, no `dynamic`, no `var` at a public boundary. |
| Null safety | PASS | New delegate and session parameters are non-nullable; no new nullable state; nullable rebuild green. |
| Composition, focused types | PASS | Sessions composed into the static class; no inheritance added. |
| Asynchrony and resource safety | PASS | `async`/`await` kept on the async cores; wrappers return the inner `Task` directly; nothing disposable introduced. |
| Exceptions at clear boundaries | PASS | The only `catch (System.Exception)` left is the pre-existing inner clear-failure handler that logs and returns false; the rethrow-only outer catch was removed. |
| Logging pattern | PASS | `logger.Warn`/`logger.Error` (the log4net field of SortEmail.cs); no console or debug output. |
| XML docs on non-obvious APIs | PASS | Present on all seamed cores, the delegate, `RedirectSaveFolder`, the CSV core and `ResetFilerPromptState`. |
| Analyzer configuration and suppressions | PASS | No added `#pragma`, `SuppressMessage` or `[Obsolete]` on added lines (Grep over the code diff: 0; the Phase 7 delta adds none). |
| Project files | PASS | `.csproj` edits are the four Compile Include lines named by the spec; kept out of CSharpier by .csharpierignore as documented. |

## 4. Language-Specific Unit Test Policy Compliance

| Requirement (CUT1 to CUT3) | Verdict | Evidence |
| --- | --- | --- |
| MSTest framework | PASS | `[TestClass]`, `[TestMethod]`, `[DataTestMethod]` with `[DataRow(..., DisplayName = ...)]` in all four new or changed test files; SS4 is a `[TestMethod]` (P7-T4 census: 12 in TAS, 6 plus 3 in TSC). |
| Moq for mocks | PASS | `Mock<Attachment>` (Loose, `SaveAsFile` unset so `Times.Never` is meaningful), `Mock<IOlObjects>`, `Mock<IApplicationGlobals>`, `Mock<IFileSystemFolderPaths>`. |
| FluentAssertions | PASS | `Should().Equal`, `BeEmpty`, `Be`, `ContainSingle`, `HaveCount`, `OnlyHaveUniqueItems`, `ThrowAsync<>().Which.Should().BeSameAs`, `OnlyContain`. Moq `Verify(..., Times.Once/Never/Exactly(2))` is used only for call counts on the interop mock. |
| Toolchain command selection | PASS | The four commands of CLAUDE.md "C# Toolchain", with step 4 as the DIRECT route of Invoke-MSTestWithCoverage.ps1 (dotnet-coverage collect around vstest.console.exe with the fixed TRX name), as spec D17 allows. |
| Coverage floors (UT2) | PASS | First-party 85.39% lines, 79.81% branches; new members at or above 90% lines (section 5). |

## 5. Test Coverage Detail

### 5.1 Per-file readings (Cobertura `<class>` nodes, baseline and post-change documents)

| File | Change | Baseline line / branch | Post-change line / branch | Changed-line check | Row verdict |
| --- | --- | --- | --- | --- | --- |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs | modified | 100% / 100% (10 valid lines; most members were attribute-excluded) | 100% / 100% (142 valid lines; 49 of 49 branches; class node `line-rate="1" branch-rate="1"` at document line 59687) | every valid line hits 1; line 143 now reads `hits="1" branch="True" condition-coverage="100% (2/2)"` (document line 59802) and the `SaveAttachment` core method node reads `branch-rate="1"` (59791); the prior review's CR-1 arm is covered | PASS |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs | modified | 95.45% / 92.86% (66 valid; 13 of 14) | 95.70% / 90.00% (93 valid; 18 of 20; class node `line-rate="0.956989" branch-rate="0.9"`, unchanged from the prior review) | every changed line hits 1; uncovered lines 36, 133, 190, 191 are the pre-existing wrapper lambda and the braces after a `throw;` (content-identified EXEMPT-LINES in coverage-comparison.md ITERATION 2); the two uncovered arms are rethrow fall-throughs, the same shape as the pre-existing `throw;` | PASS |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs | modified | 100% / 100% (8 valid) | 100% / 100% (45 valid; class node `line-rate="1" branch-rate="1"`) | the four-parameter CSV core and `MovedMailsHeader` fully exercised | PASS |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs | usings only | 100% / 100% (5 valid) | 100% / 100% (5 valid; SORTEMAIL-CLASS row 5/5) | no executable change | PASS |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs | usings only | one closure node at 0% (the `ForEachAsync` lambda inside the excluded `SortAsync`) | the same node at 0% (SORTEMAIL-CLASS row 1/0) | pre-existing; the executor's non-exempt set hash equals baseline (NONEXEMPT-SET-MATCHES-BASELINE True) | PASS (no regression) |
| QuickFiler/Controllers/EfcDataModel.cs | modified | 75.69% / 73.08% (baseline class node `line-rate="0.756863" branch-rate="0.730769"`) | 76.34% / 73.08% (class node `line-rate="0.763359" branch-rate="0.730769"`, document line 22313) | the new member `ResetFilerPromptState` reads lines 345, 346, 347 `hits="1"` (method node, document line 22541); changed lines 311, 318, 320 read hits greater than 0 per P7-T13 `E-CHANGED-LINES-COVERED: 3`; both file figures sit below the 85/75 and 80/75 floors at baseline and after; line figure improved by 0.65 points; branch figure unchanged | FAIL on the per-file threshold limb (pre-existing), PASS on the no-regression limb; non-blocking, see section 8 G-1 |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs | deleted | attribute-excluded (0 valid lines) | no node | removed from the denominator | PASS |
| ToDoModel/Email Utilities/SortItemsToExistingFolder.cs | deleted | no node (never compiled) | no node | nothing measured before or after | PASS |
| The four new and three modified test files | tests | excluded by the `.*\.Test\.dll$` module rule | same | not in the denominator by policy | PASS |

### 5.2 Per-member readings (new or changed members, AC25; coverage-comparison.md ITERATION 2, P7-T13, confirmed at the class nodes)

| Member | Lines valid / covered | Branches | Reading |
| --- | --- | --- | --- |
| `SaveAttachmentAsync` core (AttachmentSaving 194 to 224) | 21 / 21 | 4 of 4 | 100% |
| `SaveAttachment` core (130 to 156) | 19 / 19 | 4 of 4 (was 3 of 4) | 100% lines and branches; the `IsImage` true arm is pinned by SS4 |
| `SaveCaseAsync` (253 to 287) | 20 / 20 | 12 of 12 | 100% |
| `SaveCase` (289 to 309) | 8 / 8 | 7 of 7 (switch) | 100% |
| `RedirectSaveFolder` (244 to 251) | 4 / 4 | none | 100% |
| `Cleanup_Files` and `AllPromptSessions` (31 to 46) | 6 / 6 plus 7 / 7 | 2 of 2 | 100% |
| `TrySaveAttachmentCoreAsync` (TrySaveAttachment 98 to 192) | 70 / 67 | 18 of 20 | 95.71% lines; the three uncovered lines are braces after `throw;` (133, 190, 191); the two uncovered arms are rethrow fall-throughs |
| `WriteCSV_StartNewFileIfDoesNotExist` four-parameter core (UndoAndMoveLog 171 to 188) | 10 / 10 | 2 of 2 | 100% |
| `EfcDataModel.ResetFilerPromptState` (344 to 347) | 3 / 3 | none | 100% |
| `EfcDataModel.MoveToFolderAsync` changed lines (311, 318, 320) | 3 / 3 | none added | 100% |

Aggregate over the nine members: 161 valid lines, 158 covered, 98.1%; 49 branches, 47 covered, 95.9%. Every member meets the 90% new-member target of CLAUDE.md UT2 and spec D16 (MEMBERS-BELOW-90: 0, lowest 95.71). Exemptions are content-identified and re-derived at ITERATION 2 (EXEMPT-LINES 36,133,190,191; EXEMPT-GUARD-BRACE-COUNT 1), with an in-memory negative control that changes the hash (CONTROL-DIFFERS-BASELINE True).

### 5.3 Package counters (JaCoCo projection, both runs)

UtilitiesCS LINE 38909/43508 to 39100/43704 (+191 covered of +196 valid); BRANCH 9434/11293 to 9494/11356 (+60 of +63; 9495 at Phase 6). QuickFiler LINE 10461/12754 to 10468/12761 (+7 of +7); BRANCH unchanged 2518/3217. Every other package identical to baseline, which corroborates that the change touched only the two assemblies named in the write set.

## 6. Test Execution Metrics

| Run | Command (committed projection) | Total | Passed | Failed | Exit |
| --- | --- | --- | --- | --- | --- |
| Baseline full run with coverage (P0-T11, 2026-10-03T08-33) | dotnet-coverage collect around vstest.console.exe, nine assemblies, repository runsettings, stall-probe filter | 7361 | 7361 | 0 | 0 |
| Attachment-saving class after SS4 (P7-T5, 2026-10-06T17-10) | vstest.console.exe UtilitiesCS.Test.dll, FullyQualifiedName~EmailIntelligence.SortEmail_AttachmentSaving_Tests | 12 | 12 | 0 | 0 |
| Scoped SortEmail family (P7-T10, 17-15) | vstest.console.exe UtilitiesCS.Test.dll, FullyQualifiedName~EmailIntelligence.SortEmail_ | 56 | 56 | 0 | 0 |
| Scoped QuickFiler.Test (P7-T10, 17-15) | vstest.console.exe QuickFiler.Test.dll, EfcDataModelFilerCleanupTests then EfcDataModelArchiveRootTests | 3 and 11 | 3 and 11 | 0 | 0 and 0 |
| Final full run with coverage (P7-T11, 17-19) | as baseline | 7394 | 7394 | 0 | 0 |

Delta +33 rows against baseline: SortEmail_SaveCase_Tests 12, SortEmail_AttachmentSaving_Tests 12 (11 plus SS4), SortEmail_UndoAndMoveLog_Tests 2, EfcDataModelFilerCleanupTests 3, T12 1, SortEmail_Tests 15 to 18 (+3). Every test platform figure (error, timeout, aborted, notExecuted, inconclusive) is 0 in both full runs. The 56 RESULT rows of P7-T10 are the 55 Phase 6 names plus `SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly` (read in pass-after-regression-tests.md ITERATION 2).

Clock cross-check: the post-change Cobertura root `timestamp="1791321467"` (17:17:47 -0400) sits 2 minutes before the artifact label 17-19 and 121 seconds before the P7-T11 commit epoch 1791321588; the Phase 7 labels 17-06 to 17-25 match their commit epochs (1791320815 = 17:06:55 through 1791321953 = 17:25:53) to the minute. Labels are clock-consistent.

## 7. Code Quality Checks

| Check | Command or method | Result |
| --- | --- | --- |
| Confidentiality masking scan | Grep over the feature folder for the account name, the short profile name, the session worktree name, the item worktree leaf and `Users\` path fragments | 0 hits in every evidence and review file; 1 hit in plan.2026-10-02T05-07.md line 4087 (the revision 2.0 self-review names the item worktree leaf `.git/worktrees/agent-ae9faf6e1bf21ac17` and a sibling worktree leaf as repository-relative git paths; no account, host or drive-letter path), see G-9; CMD-SWEEP (p7-t16) ACCOUNT-TOKEN-FILES, PROFILE-LEAF-FILES, MACHINE-TOKEN-FILES, WORKTREE-ROOT-FILES, USERS-PATH-FILES all 0 over 103 files |
| Suppression scan (added lines) | Grep of `^\+` lines in the code diff and the Phase 7 delta for `#pragma warning`, `SuppressMessage`, `[Obsolete`, `dynamic ` | 0 hits |
| Workflow change scan | Diff path list | no path under .github/ |
| Dead-symbol scan | Grep over every .cs file for `SaveAttachmentsOld`, `IsPicture`, `SortItemsToExistingFolder`, `_responseSaveFile`, `MAX_PATH`, `SanitizeArray(`, `Debug.WriteLine` (partials) | only two comment mentions of SortItemsToExistingFolder in the two unchanged ToDoModel.Test files (prior review; no production file changed since) |
| Banned-using scan (AC17) | Grep over the five partials for `using System.Diagnostics;`, `Deedle`, `SDILReader`, `using Outlook =`, `using UtilitiesCS;`, `ShowDialog(` | 0 hits; using counts 8, 5, 10, 9, 9 match the spec blocks |
| Exclusion attribute census (AC13) | Count of `ExcludeFromCodeCoverage` across the five partials | 18, the eighteen members the spec keeps |
| CR-1 closure check | Cobertura class node for SortEmail.AttachmentSaving.cs, line 143 and method `SaveAttachment` | `condition-coverage="100% (2/2)"`, `branch-rate="1"` (P7-T1 recorded `50% (1/2)` and `0.75` on the Phase 6 document before the edit) |
| CR-3 closure check | Read of SortEmail_SaveCase_Tests.cs lines 1 to 6 | first line `using System.Collections.Generic;`; `using System;` absent (P7-T4 census `usingSystem;` 0); the file names no System-namespace type (P7-T1 TSC-SYSTEM-IDENTIFIER-LINES 0); both rebuilds green after the removal |
| Host-path and raw-document sweep of this review | the three artifacts use repository-relative paths only | clean by construction |

## 8. Gaps and Exceptions

- G-1 (non-blocking, carried, recommend filing). `QuickFiler/Controllers/EfcDataModel.cs` reads 76.34% lines and 73.08% branches after the change against 75.69% and 73.08% before. The per-file threshold limb fails on a pre-existing condition; the no-regression limb passes (every changed line and the new member hit; the file improved). The uncovered remainder is Outlook-interop orchestration in a QuickFiler controller, the class category CLAUDE.md UT2 exemption (c) names, and raising a 485-line controller to the floor is outside a bugfix's minimal-fix mandate (CLAUDE.md Bugfix Workflow step 2). Disposition unchanged: file a coverage-uplift potential entry (U-1).
- G-2 (carried). No `artifacts/pr_context.summary.txt` or appendix exists in the item worktree, and they could not be regenerated here (no shell). Scope was derived from the caller-supplied full code diff and Phase 7 delta against merge base `942873699`, which equals the worktree reflog's first entry and the footprint gate's anchor, and cross-checked by Read and Grep of the worktree and the P7-T15 footprint record. The session checkout's pair belongs to another branch and was not used.
- G-3 (carried). The review label `2026-10-06T17-40` is not a host-clock reading (see the header). The executor's labels were checked against commit epochs and the Cobertura root epoch and are consistent.
- G-4: closed. The P6-T13 documentation-comment edit, previously verified by CSharpier only, is now covered by the Phase 7 analyzer rebuild, nullable rebuild, scoped runs and full coverage run (p7-t8, p7-t9, p7-t10, p7-t11), each of which post-dates that edit and the two Phase 7 edits.
- G-5 (carried). `quality-tiers.yml` is absent at the repository root (Glob), so the tier-dependent gates of .claude/rules/quality-tiers.md cannot be evaluated; pre-existing and recurring (promoted at #956).
- G-6 (carried). The test-location rule of .claude/rules/general-unit-test.md (`tests/` mirror) is not the convention of this solution; the item follows the established `<Project>.Test/` layout. Pre-existing, not a finding of this item.
- G-7 (carried). `validate_evidence_locations.py` was not executed; the Evidence Location Compliance section was established from the enumerated diff paths and the P7-T15 footprint record.
- G-8 (new, observation). Between the Phase 6 and Phase 7 full runs the first-party branch numerator moved from 13681 to 13680 (UtilitiesCS BRANCH covered 9495 to 9494) while the SortEmail family gained one covered arm, so one branch elsewhere in UtilitiesCS flipped from covered to uncovered. Phase 7 changed no production line, every SortEmail-family class row is identical to Phase 6, the non-exempt uncovered-statement hash equals baseline, and the two-decimal rates are unchanged (85.39, 79.81); the Phase 6 document was overwritten by P7-T11, so the flipped branch cannot be localised without a shell. This is within the run-to-run band this repository's dotnet-coverage runs show on unchanged code (the same-tree comparison memory for #511) and has no bearing on any floor or changed line. Recorded, not gated, as P7-T12 did.
- G-9 (new, observation). plan.2026-10-02T05-07.md line 4087 (the revision 2.0 self-review bullet) names the item worktree leaf `agent-ae9faf6e1bf21ac17` and a sibling worktree leaf as repository-relative `.git/worktrees/` and `.claude/worktrees/` references. These are not account, host or absolute paths and the executor's CMD-SWEEP tokens do not match them; no action required. The prior review's statement "0 hits for the item worktree name" described HEAD `0fab75ed6`, before revision 2.0 was committed.
- Observation (carried). spec.md AC3 says the core "has exactly one catch clause"; the core's outer `try` has exactly one catch clause while the pre-existing inner `try` keeps its `catch (System.Exception inner)`, which the same AC names. AC13 says each `GetAttachmentsInfo` test "gains one DataRow row" while each gained two. Wording drift; the intent of each AC holds (CR-7).
- Observation (carried). `QuickFiler/Legacy/QfcController.cs:792` calls `SortEmail.Cleanup_Files()` after `SortEmail.Run(...)` with no `try`/`finally`; the file has no `Compile Include` in QuickFiler.csproj and `SortEmail.Run` does not exist in the partials. The orchestrator's PD-16 disposition is "no change; for filing with U-2" (CR-2).
- Observation (new). The plan's `- **Status:**` line (line 7) is stale after Phase 7 (CR-8).

## 9. Summary of Changes

- Production (UtilitiesCS): `SaveCase` labels corrected (L1); `TrySaveAttachmentAsync` forwards to a private core with a one-clear retry bound under a held YesToAll, logger calls replace `Debug.WriteLine`, the no-op outer rethrow is removed (L2, #966); three `YesNoToAllPromptSession` fields, an `AllPromptSessions` property and a `foreach` cleanup replace four enum fields (L3, F1); seamed `SaveAttachment`, `SaveAttachmentAsync` and `SaveCaseAsync` cores with a nested `TrySaveAttachmentDelegate`; `RedirectSaveFolder` re-roots both save paths; `WriteCSV_StartNewFileIfDoesNotExist` gains a four-parameter core, `MovedMailsHeader` added, `SanitizeArray` deleted (L4, header); seven `[ExcludeFromCodeCoverage]` removed, eighteen kept (F3); using blocks pruned (D10); `SortEmail.LegacyAttachmentSaving.cs` and its project entry deleted (F2).
- Production (QuickFiler): `MoveToFolderAsync` wraps the filer call in `try`/`finally` calling the new `protected internal virtual ResetFilerPromptState()`.
- Production (ToDoModel): the uncompiled duplicate `SortItemsToExistingFolder.cs` deleted.
- Tests: new `SortEmail_SaveCase_Tests` (12 rows), `SortEmail_AttachmentSaving_Tests` (12, including the Phase 7 SS4), `SortEmail_UndoAndMoveLog_Tests` (2), `EfcDataModelFilerCleanupTests` (3); T12 and the `CreateDirectoryLimit` tripwire added to the try-save tests; `SortEmail_Tests` loses the `SanitizeArray` test, gains two rows per `GetAttachmentsInfo` test and one corrected doc comment; four Compile Include lines. Phase 7: SS4 added, `using System;` removed from SortEmail_SaveCase_Tests.cs.
- Docs: the #956 spec receives the three CR-1 edits; this feature folder's spec, plan (revision 2.0), research, 103 evidence files (eleven from Phase 7, three fixed-name projections rewritten at ITERATION 2) and the three prior review artifacts; the promotion record under docs/features/potential/promoted/; agent-memory Markdown under .claude/agent-memory/.

## 10. Compliance Verdict

PASS. Zero blocking findings. All general and C# code-change and unit-test policy requirements are met on the evidence cited; first-party C# coverage is 85.39% lines and 79.81% branches, not lower than baseline, with 98.1% line and 95.9% branch coverage on the new or changed members; the full toolchain pass of Phase 7 post-dates every source edit on the branch. CR-1 and CR-3 of the prior review are confirmed closed. Non-blocking items for the orchestrator: CR-2 (uncompiled legacy call site, for filing with U-2 per PD-16), CR-4 to CR-7 (informational, carried), CR-8 (plan Status line stale), G-1 (EfcDataModel pre-existing sub-floor coverage, for filing), G-5 (`quality-tiers.yml` absent, recurring), and the two pull-request-dependent acceptance criteria AC6 and AC27, which the pr-author step closes.

## Appendix A: Test Inventory

SortEmail family, 56 rows, all Passed (pass-after-regression-tests.md ITERATION 2, P7-T10):

- SortEmail_SaveCase_Tests (12): SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No], [NoToAll]; SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes], [YesToAll]; SaveCase_WhenAnswerIsEmpty_DoesNotSave; SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [Yes], [YesToAll]; SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePathAndReleasesAnswer; SaveCaseAsync_WhenAnswerIsNoToAllAndAltNameAnswerIsYesToAll_KeepsAnswerAcrossCalls; SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsNoToAll_DoesNotSaveAndKeepsAnswer; SaveCaseAsync_WhenAnswerIsNoAndAltNamePromptIsCancelled_DoesNotSaveAndStaysAskable; SaveCaseAsync_WhenAnswerIsEmpty_DoesNothing.
- SortEmail_AttachmentSaving_Tests (12): SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting; SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly; SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly; SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave; SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain; SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath; SaveAttachment_WhenFileDoesNotExist_SavesDirectly; SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer; SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer; SaveAttachment_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly (SS4, Phase 7); RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths; Cleanup_Files_ResetsEveryPromptSession.
- SortEmail_UndoAndMoveLog_Tests (2): WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader; WriteCSV_WhenFileExists_DoesNotWrite.
- SortEmail_TrySaveAttachment_Tests (12): the eleven pre-existing T1 to T11 (unchanged) plus TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear (T12).
- SortEmail_Tests (18): the twelve unchanged single tests plus GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments (3 rows) and GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments (3 rows).

QuickFiler.Test (P7-T10): EfcDataModelFilerCleanupTests (3): MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates; MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce; MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState. Pin suite EfcDataModelArchiveRootTests (11), unchanged, all Passed.

Red-first observations (negative-controls.md): L1 four rows red (`but was 0 times`); L3 phase one one row red; L4 two tests red (`differs at index 0`, `NullReferenceException`); L2 T12 red with the sentinel `InvalidOperationException`; re-rooting red on the alternate-path assertion; EfcDataModel throwing test red (`but found 0`); mutation control on the structural test red (`but found 3`), file restored byte-identical. SS4 (Phase 7): coverage reading 50% (1/2) before and 100% (2/2) after; no behavioural red, disclosed as test coverage of already-correct behaviour.

## Appendix B: Toolchain Commands Reference

1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` (P7-T6, P7-T7; scoped format of the two edited files in P7-T4).
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (P7-T8).
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (P7-T9).
4. `dotnet-coverage collect --output coverage\final-959.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-959.config -- vstest.console.exe <nine test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\959\final" "/Logger:trx;LogFileName=final-959.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"` (P7-T11, the DIRECT route of Invoke-MSTestWithCoverage.ps1; the same command with `baseline` names at P0-T11).
5. Scoped runs: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_AttachmentSaving_Tests" ...` (P7-T5), `"/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_"` (P7-T10) and the two QuickFiler.Test filters (P7-T10).

No command was re-run by this review.
