# Policy Compliance Audit: sort-email-latent-logic-defects (Issue #959; the PR also closes #966)

- Artifact: `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/policy-audit.2026-10-06T15-30.md`
- Review label: `2026-10-06T15-30`. This is not a host-clock reading. The review ran without a shell (caller directive), so the label was chosen strictly after the last reflog epoch of the item worktree (`1791314671` = 2026-10-06T15:24:31 -0400, the P6-T46 commit `0fab75ed6`) and before this file was written. Every evidence label and reflog epoch cited below sorts before it.
- Branch: `bug/sort-email-latent-logic-defects-959` at HEAD `0fab75ed6` (clean and pushed per the caller; the reflog's last entry is that commit).
- Base: merge-base `94287369908cc920b21b0e3256314f988ad7d2f5` (origin/main at branch cut). It equals the first reflog entry of the item worktree and the footprint gate's `ANCHOR-RECHECK` (evidence/qa-gates/p6-t12-scope-boundary.2026-10-06T13-23.md).
- Work mode: `full-bug` (issue.md line 12). AC source: spec.md `## Acceptance Criteria` only (AC1 to AC27). Issue #966 is folded into this item (issue.md lines 22 to 35; spec D18).
- Scope: the full branch diff against the merge base, taken from the caller-supplied full diff (26 paths) and code diff (17 paths), cross-checked against the worktree by Read and Grep. No plan, task or file subset narrowed it (see Rejected Scope Narrowing).
- Reviewer tool surface: Read, Grep, Glob, Write and the orchestration validator MCP. No Bash, so no command was re-run; every figure below is read from the worktree, the two Cobertura documents on disk, the caller-supplied diffs or the committed evidence projections, each cited where used.

## Executive Summary

| Item | Result |
| --- | --- |
| Compliance verdict | PASS |
| Blocking findings | 0 |
| Non-blocking findings | 7 (CR-1 to CR-7 in code-review.2026-10-06T15-30.md) plus 7 gaps and observations (section 8) |
| Acceptance criteria | 25 of 27 PASS; AC6 and AC27 pending the pull-request body (pr-author step), not evaluated FAIL per the caller's directive |
| C# first-party line coverage | 85.39% post-change against 85.36% baseline (floor 85% per .claude/rules; 80% per CLAUDE.md UT2): PASS |
| C# first-party branch coverage | 79.81% post-change against 79.75% baseline (floor 75%): PASS |
| C# new or changed code | 98.1% lines (158 of 161 valid lines across the nine members named by AC25; the three uncovered lines are braces after a rethrow), 93.9% branches (46 of 49) |
| Toolchain | One clean pass of the four CLAUDE.md C# commands on 2026-10-03 (iteration 1, LOOP-RESTARTS 0), both rebuilds with CoreCompile not skipped; one later documentation-comment-only edit (P6-T13) was verified by CSharpier only, see section 8 G-4 |
| Tests | 7393 of 7393 passed in the final coverage run (baseline 7361; +32 rows exactly accounted for) |

The item fixes the four logic defects (L1 to L4), the three same-root-cause items (EfcDataModel `finally`, the ToDoModel duplicate, the CSV header), the re-rooting defect, and the six #966 residuals, each with red-first evidence or a documented exception, inside an eighteen-path write set. The code is compliant with the general and C# code-change and unit-test policies. The two per-file readings below a floor are a pre-existing sub-floor file (`EfcDataModel.cs`, improved, no changed line uncovered) and one uncovered ternary arm in a new synchronous core (one-test remedy, CR-1); neither blocks.

## Rejected Scope Narrowing

None detected. Two caller statements were examined and classed as not narrowing:

1. "Known disposition decided by the orchestrator (not a finding by itself): a doc-comment-only edit to SortEmail_Tests.cs (P6-T13) was verified by csharpier format/check, census, footprint and identity gates; the rebuilds and coverage run were not repeated for it inside the plan." This describes a toolchain disposition, not a review-scope limit; the edit is inside the audited diff and is recorded as G-4 in section 8.
2. "The four shell-icon test classes are excluded locally by the plan's stall-probe rule (environmental hang, reproduces on main); CI covers them." This describes the coverage run's test filter (the same filter at baseline and post-change, evidence/baseline/p0-t9-stall-probe.2026-10-03T08-29.md), not a limit on which files this audit covers; the four classes touch no changed file.

The audit scope is the full branch diff against the merge base.

## Evidence Location Compliance

- Branch diff paths under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`: none. The 26 diff paths (Grep `^diff --git a/` over the caller-supplied full diff) are 17 code or project paths, 6 paths under this feature folder, the #956 spec.md, the promotion record under docs/features/potential/promoted/, and no path under artifacts/.
- Evidence produced by the executor lives under `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/{baseline,qa-gates,regression-testing,other}/` (Glob of the feature folder: 89 files; CMD-EVIDENCE-FIELDS `NONCANONICAL-SUBFOLDER-FILES: 0` in p6-t46-ac-inventory.2026-10-06T15-23.md).
- Raw documents: no `.xml`, `.trx` or `.coverage` path in the diff (file list above; footprint gate `RAW-DOC-PATHS: 0`). Committed test evidence is the JaCoCo package projection plus the one-line first-party summary (coverage-baseline.md, coverage-post-change.md) and TRX-derived summaries, as CLAUDE.md "Committed Test Evidence Format" requires.
- `validate_evidence_locations.py --root .` was not run in this review (no shell); the check above was made by enumerating every diff path instead.
- This review writes only the three audit artifacts into the feature folder root. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` event: the caller supplied canonical paths only.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles (UT1 to UT5)

| Requirement | Verdict | Evidence |
| --- | --- | --- |
| Independence and isolation | PASS | Every new test owns its `YesNoToAllPromptSession` instances, recording delegates and Moq mocks; no test reads or writes a production session (`SetValue(` 0, `SortEmail.Cleanup_Files();` 0 in the AttachmentSaving test file per the P6-T10 census; confirmed by reading the diff). The structural test reads static state only. The full run passed under scripts/vscode/TaskMaster.cli.runsettings (Workers zero, class-level scope; RUNSETTINGS-HASH equal to P0-T4). |
| Determinism | PASS | Grep of added diff lines for `DoNotParallelize`, `Thread.Sleep`, `Task.Delay`, `Timeout`, `DateTime.Now`: zero hits. The one fixed date is a literal `new DateTime(2026, 4, 3, 9, 30, 0)`. The L2 test ends an unbounded loop with a call-count tripwire (`CreateDirectoryLimit`), not a timer. |
| Fast execution | PASS | 55 SortEmail-family rows and 3 EfcDataModel rows run in-process with mocks; no I/O. |
| Readability and intent | PASS | Each test carries a `/// <summary>` naming scenario and expectation and the Arrange, Act, Assert markers (read in the diff). |
| No temporary files or external dependencies (UT4) | PASS | Sandbox roots C:\Sortemail959Sandbox, C:\Sortemail956Sandbox and C:\Sortemail945Sandbox reported absent before and after every recorded run (`SANDBOX-*-EXISTS-*: False` in coverage-baseline.md, coverage-post-change.md and pass-after-regression-tests.md). Added lines contain no `Directory.CreateDirectory`, `File.`, `GetTempPath` or `MemoryAppender` (census and diff). |
| Scenario completeness (UT2) | PASS | SaveCase: No, NoToAll, Yes, YesToAll, Empty. SaveCaseAsync: Yes and YesToAll, No with Yes, NoToAll with YesToAll across two calls, No with NoToAll across two calls, cancelled twice, Empty. Async core: absent file, image prompt, document prompt, Yes released, YesToAll kept, No then Yes to alternate. Sync core: absent file, Yes, NoToAll. Re-rooting, structural cleanup, two CSV cases, three EfcDataModel cases, T12 retry bound. One gap: the synchronous core's image arm (CR-1, non-blocking). |
| Test file location | PASS with repo convention | Tests live in `UtilitiesCS.Test/EmailIntelligence/` and `QuickFiler.Test/Controllers/`, the repository's established `<Project>.Test/` layout (pre-existing convention; the `tests/` mirror of .claude/rules/general-unit-test.md is not used anywhere in this solution). No test was colocated with production source. |
| UT5 call-out | PASS | The phase-one reflective-write test no longer exists in the tree (`Cleanup_Files_ResetsEveryPromptAnswerField` 0 hits in the final file). The UT5 call-out is in spec.md Test Strategy and in evidence/qa-gates/pr-description-inputs.2026-10-06T15-12.md for the PR body. |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `coverage/baseline-959.cobertura.xml` (git-ignored, present on disk in the item worktree; class nodes read directly; committed projection `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/baseline/coverage-baseline.md`)
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml` (root `lines-covered="56410" lines-valid="66058" branches-covered="13681" branches-valid="17141"`, `timestamp="1791045728"` = 2026-10-03T12:42:08 -0400, equal to the FINAL-FIRST-PARTY counters of `docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/coverage-post-change.md`)
- TypeScript baseline coverage artifact: none consulted (zero TypeScript files changed on this branch)
- TypeScript post-change coverage artifact: none consulted (zero TypeScript files changed on this branch)
- PowerShell baseline coverage artifact: none consulted (zero PowerShell files changed on this branch)
- PowerShell post-change coverage artifact: none consulted (zero PowerShell files changed on this branch)
- Python baseline coverage artifact: none consulted (zero Python files changed on this branch)
- Python post-change coverage artifact: none consulted (zero Python files changed on this branch)
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.36% lines (56212/65855), 79.75% branches (13620/17078). Post-change: 85.39% lines (56410/66058), 79.81% branches (13681/17141). Change: +0.03% lines and +0.06% branches, no changed line lost coverage. New/changed-code coverage: 98.1%. Disposition: PASS. Evidence: coverage/baseline-959.cobertura.xml and artifacts/csharp/coverage.xml read at the class nodes, with the committed projections coverage-baseline.md, coverage-post-change.md and coverage-comparison.md under the feature folder's evidence subfolders.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

Languages with changed files on the branch: C# only (`.cs` and `.csproj`; every other diff path is Markdown). The C# repo-wide figure is the first-party figure the executor's route reports, which equals the Cobertura root counters because `coverage/effective-coverage-959.config` excludes only third-party modules (`Deedle`, `FSharp`, `Castle.Core`, `FluentAssertions`, `Moq`, `Microsoft.Testing`, `MSTest`) and `.*\.Test\.dll$`; no production module and no production source path is excluded, so the Coverage Exclusion Policy of .claude/rules/general-unit-test.md is met (no `exclude` entry matches a production path). The four shell-icon test classes are filtered from the run at baseline and post-change alike; they touch no changed file.

Floors applied: .claude/rules/general-unit-test.md and quality-tiers.md (85% line, 75% branch, no regression on changed lines); CLAUDE.md UT2 (80% line, 75% branch on the testable denominator, 90% for new members); spec D16 and AC25 (the CLAUDE.md floors plus 90% per named member). The first-party figures clear every floor. Per-file and per-member readings are in section 5.

Coverage Metrics by Language:

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
| --- | --- | --- | --- | --- | --- | --- |
| C# | 17 (5 modified production files, 2 deleted production files, 3 project files, 3 modified test files, 4 new test files) | 7393 in the final full run (7361 at baseline) | 7393 passed, 0 failed, 0 skipped | 85.36% lines, 79.75% branches | 85.39% lines, 79.81% branches | 98.1% lines, 93.9% branches |
| TypeScript | 0 | 0 | none run (zero files changed) | N/A | N/A | N/A |
| PowerShell | 0 | 0 | none run (zero files changed) | N/A | N/A | N/A |
| Python | 0 | 0 | none run (zero files changed) | N/A | N/A | N/A |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
| --- | --- | --- |
| Clarify objective, read plans, document the plan | PASS | spec.md revision 0.2 (D1 to D18), plan.2026-10-02T05-07.md revision 1.9 (114 of 114 tasks executed), two research records, preflight clearance under evidence/other/. |
| Bugfix workflow: failing regression test first | PASS | Six red-first artifacts under evidence/regression-testing/ each carry `EXIT_CODE: 1` with `ExpectedExitCode: 1` and name the failing rows (fail-before-save-case.md, fail-before-cleanup-files-phase-one.md, fail-before-write-csv.md, fail-before-try-save-retry.md, fail-before-redirect-save-folder.md, fail-before-efc-filer-cleanup.md; Grep `^EXIT_CODE:` over the folder). Refactor-only steps carry the eight-entry dossier fail-before-exception.2026-10-03T12-33.md and the compile-red record. The structural test received a mutation control (negative-controls.md: `but found 3`, hashes restored). |
| Minimal, targeted fix | PASS | Each defect's fix is the smallest change named in the spec decision (stacked labels; one `bool` flag and one guard; a `foreach` over one property; a seamed core with a single header line; one two-statement helper; one `try`/`finally` with a virtual seam). Opportunistic edits were limited to the #966 residuals the maintainer folded in. |
| Design principles (simplicity, reuse, extensibility, separation of concerns) | PASS | Prompt state moved from four hand-rolled enum fields to the existing `YesNoToAllPromptSession` type; cores take `Func<string, bool>`, sessions and a named delegate so logic is testable without disk or dialog; wrappers are one-statement forwards passing method groups. The named `TrySaveAttachmentDelegate` is justified in code (CS1769 over an embedded interop type). |
| Classes and functions | PASS | New members are small static helpers on the existing static partial class and one virtual seam on `EfcDataModel`; no new type beyond the delegate. |
| Error handling, logging, contracts | PASS | The no-op outer `catch (System.Exception) { throw; }` is removed; the handler logs through the class log4net logger at Warn and Error with the exception object; the new guard rethrows with a bare `throw;` (instance preserved, asserted by `BeSameAs`). `EfcDataModel.MoveToFolderAsync` resets prompt state in a `finally` whose callee cannot throw. `Debug.WriteLine` is gone from the family (Grep: 0). |
| Module and file structure (500-line limit) | PASS | Twelve C# files in the write set, MAX-LINES 488 (P6-T10 CMD-LINES). Read confirmed AttachmentSaving 329 and TrySaveAttachment 204; EfcDataModel.cs 485 and SortEmail_Tests.cs 488 are the closest (CR-4). Deleted: LegacyAttachmentSaving.cs (240) and SortItemsToExistingFolder.cs (402, never compiled). |
| Naming, docs, comments | PASS | Descriptive names following the #956 `RemoveReadOnlyPrompt` precedent; `/// <summary>` on every new member; "why" comments on the property-not-field choice, the retry bound, the delegate type and the `finally`. |
| Dependencies | PASS | None added; csproj edits are Compile Include lines only (UCS 0/1, UCT 3/0, QFT 1/0 numstat). |
| I/O boundaries | PASS | Real `File.Exists`, `FileIO2.WriteTextFile`, `YesNoToAll.ShowDialog` and `Directory.CreateDirectory` are reached only from `[ExcludeFromCodeCoverage]` one-statement wrappers; every core is exercised with delegates. |
| Match existing style; no breaking public API | PASS | Public signatures `SaveAttachment(this AttachmentHelper)`, `SaveAttachmentAsync(this AttachmentHelper)`, `SaveAttachmentAsync(this AttachmentHelper, string)`, `Cleanup_Files()` and `WriteCSV_StartNewFileIfDoesNotExist(string, string)` unchanged; callers EmailFiler.cs:445, SortEmail.cs:156, SortEmail.MailItemSort.cs:144 and 290, AppOlObjects.cs:301 and EfcDataModel.cs:346 compile without edits (Grep over the worktree). The five-argument `TrySaveAttachmentAsync` keeps its parameter list; the eleven pre-existing try-save tests are textually unchanged (TST2-DELETED-LINES 0). |
| Toolchain loop | PASS | toolchain-final-pass.md: format (WRITESET-CHANGED-COUNT 0), check (1639 files), analyzer rebuild and nullable rebuild (SKIP_CORECOMPILE_LINES 0, four CSC output lines), scoped runs 55/55, 3/3, 11/11, full coverage run 7393/7393; iteration 1, LOOP-RESTARTS 0. See G-4 for the later doc-comment edit. |
| Supporting documents updated | PASS | #956 spec.md: three CR-1 edits exactly (diff hunk: 4 added, 2 deleted lines; current lines 99, 151 and 157 read). Plan status line and AC check-offs current. |
| Clear next steps | PASS | pr-description-inputs.2026-10-06T15-12.md carries the closing references, the four behavior changes, the UT5 call-out and the maintainer question on the L2 rethrow sink. |

## 3. Language-Specific Code Change Policy Compliance

C# is the only language with changed files.

| Requirement (CLAUDE.md C#1 to C#7) | Verdict | Evidence |
| --- | --- | --- |
| CSharpier through `dotnet tool run`, format then check | PASS | p6-t1-csharpier-format.2026-10-03T12-34.md and p6-t2-csharpier-check.2026-10-03T12-35.md, exit 0; the P6-T13 doc-comment edit re-checked in p6-t14-doc-comment-format-and-census.2026-10-06T13-22.md. |
| Analyzer rebuild (`/t:Rebuild`, analyzers and code style enforced) | PASS | p6-t3-msbuild-analyzers.2026-10-03T12-36.md, exit 0, CoreCompile not skipped on UtilitiesCS, UtilitiesCS.Test, QuickFiler, QuickFiler.Test. |
| Nullable rebuild (`/p:TreatWarningsAsErrors=true`, no `/p:Nullable=enable`) | PASS | p6-t4-msbuild-nullable.2026-10-03T12-37.md, exit 0, same CoreCompile check. All five surviving partials keep `#nullable enable` on line 1 (Read). |
| Strong contracts, explicit types at public boundaries | PASS | New members are `internal` or `private`; the one new `protected internal virtual` member sits on an internal class; parameters are typed delegates and sessions, no `dynamic`, no `var` at a public boundary. |
| Null safety | PASS | New delegate and session parameters are non-nullable; no new nullable state; nullable rebuild green. |
| Composition, focused types | PASS | Sessions composed into the static class; no inheritance added. |
| Asynchrony and resource safety | PASS | `async`/`await` kept on the async cores; wrappers return the inner `Task` directly (no `async` wrapper overhead); nothing disposable introduced. |
| Exceptions at clear boundaries | PASS | The only `catch (System.Exception)` left is the pre-existing inner clear-failure handler that logs and returns false; the rethrow-only outer catch was removed. |
| Logging pattern | PASS | `logger.Warn`/`logger.Error` (the log4net field of SortEmail.cs); no console or debug output. |
| XML docs on non-obvious APIs | PASS | Present on all seamed cores, the delegate, `RedirectSaveFolder`, the CSV core and `ResetFilerPromptState`. |
| Analyzer configuration and suppressions | PASS | No added `#pragma`, `SuppressMessage` or `[Obsolete]` on added lines (Grep over the code diff: 0). The pre-existing CS0618 pragma in `GetAttachmentsInfoAsync` is unchanged (only its exclusion attribute was removed). |
| Project files | PASS | `.csproj` edits are the four Compile Include lines named by the spec; kept out of CSharpier by .csharpierignore as documented. |

## 4. Language-Specific Unit Test Policy Compliance

| Requirement (CUT1 to CUT3) | Verdict | Evidence |
| --- | --- | --- |
| MSTest framework | PASS | `[TestClass]`, `[TestMethod]`, `[DataTestMethod]` with `[DataRow(..., DisplayName = ...)]` in all four new or changed test files; NON-APPROVED-FRAMEWORKS 0. |
| Moq for mocks | PASS | `Mock<Attachment>`, `Mock<IOlObjects>`, `Mock<IApplicationGlobals>`, `Mock<IFileSystemFolderPaths>` (strict where a stray read must fail). |
| FluentAssertions | PASS | `Should().Equal`, `ContainSingle`, `HaveCount`, `OnlyHaveUniqueItems`, `ThrowAsync<>().Which.Should().BeSameAs`, `OnlyContain`. Moq `Verify(..., Times.Once/Never/Exactly(2))` is used only for call counts on the interop mock. |
| Toolchain command selection | PASS | The four commands of CLAUDE.md "C# Toolchain", with step 4 as the DIRECT route of Invoke-MSTestWithCoverage.ps1 (dotnet-coverage collect around vstest.console.exe with the fixed TRX name), as spec D17 allows. |
| Coverage floors (UT2) | PASS | First-party 85.39% lines, 79.81% branches; new members at or above 90% lines (section 5). |

## 5. Test Coverage Detail

### 5.1 Per-file readings (Cobertura `<class>` nodes, baseline and post-change documents)

| File | Change | Baseline line / branch | Post-change line / branch | Changed-line check | Row verdict |
| --- | --- | --- | --- | --- | --- |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs | modified | 100% / 100% (10 valid lines; most members were attribute-excluded) | 100% / 97.96% (142 valid lines; 48 of 49 branches) | every valid line hits 1; the one uncovered arm is line 143, the `IsImage` true arm of the synchronous `SaveAttachment` core (CR-1) | PASS |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs | modified | 95.45% / 92.86% (66 valid; 13 of 14) | 95.70% / 90.00% (93 valid; 18 of 20) | every changed line hits 1; the new guard (line 123) reads 4 of 4; uncovered lines 36, 133, 190, 191 are the pre-existing wrapper lambda and the braces after a `throw;` (content-identified in coverage-comparison.md); the one added uncovered arm is the unreachable fall-through of the new `throw;` at line 132, the same shape as the pre-existing `throw;` at line 189 (baseline line 153, 1 of 2 at baseline) | PASS |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.UndoAndMoveLog.cs | modified | 100% / 100% (8 valid) | 100% / 100% (45 valid) | the four-parameter CSV core and `MovedMailsHeader` fully exercised | PASS |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs | usings only | 100% / 100% (5 valid) | 100% / 100% (5 valid) | no executable change | PASS |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs | usings only | one closure node at 0% (the `ForEachAsync` lambda at line 153, inside the excluded `SortAsync`) | the same node at 0% (line 144) | pre-existing; the executor's non-exempt set hash equals baseline | PASS (no regression) |
| QuickFiler/Controllers/EfcDataModel.cs | modified | 75.69% / 73.08% | 76.34% / 73.08% | changed lines 311, 318, 320 and the new member at 345 to 347 all read `hits="1"` (class node read directly); both file figures sit below the 85/75 and 80/75 floors at baseline and after; line figure improved by 0.65 points; branch figure unchanged | FAIL on the per-file threshold limb (pre-existing), PASS on the no-regression limb; non-blocking, see section 8 G-1 |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs | deleted | attribute-excluded (0 valid lines) | no node | removed from the denominator | PASS |
| ToDoModel/Email Utilities/SortItemsToExistingFolder.cs | deleted | no node (never compiled) | no node | nothing measured before or after | PASS |
| The four new and three modified test files | tests | excluded by the `.*\.Test\.dll$` module rule | same | not in the denominator by policy | PASS |

### 5.2 Per-member readings (new or changed members, AC25; coverage-comparison.md P6-T9, confirmed at the class nodes)

| Member | Lines valid / covered | Branches | Reading |
| --- | --- | --- | --- |
| `SaveAttachmentAsync` core (AttachmentSaving 194 to 224) | 21 / 21 | 4 of 4 | 100% |
| `SaveAttachment` core (130 to 156) | 19 / 19 | 3 of 4 | 100% lines; the `IsImage` true arm untested (CR-1) |
| `SaveCaseAsync` (253 to 287) | 20 / 20 | 12 of 12 | 100% |
| `SaveCase` (289 to 309) | 8 / 8 | 7 of 7 (switch) | 100% |
| `RedirectSaveFolder` (244 to 251) | 4 / 4 | none | 100% |
| `Cleanup_Files` and `AllPromptSessions` (31 to 46) | 6 / 6 plus 7 / 7 | 2 of 2 | 100% |
| `TrySaveAttachmentCoreAsync` (TrySaveAttachment 98 to 192) | 70 / 67 | 18 of 20 | 95.71% lines; the three uncovered lines are braces after `throw;` (133, 190, 191); the two uncovered arms are rethrow fall-throughs |
| `WriteCSV_StartNewFileIfDoesNotExist` four-parameter core (UndoAndMoveLog 171 to 188) | 10 / 10 | 2 of 2 | 100% |
| `EfcDataModel.ResetFilerPromptState` (344 to 347) | 3 / 3 | none | 100% |
| `EfcDataModel.MoveToFolderAsync` changed lines (311, 318, 320) | 3 / 3 | none added | 100% |

Aggregate over the nine members: 161 valid lines, 158 covered, 98.1%; 49 branches, 46 covered, 93.9%. Every member meets the 90% new-member target of CLAUDE.md UT2 and spec D16. Exemptions are content-identified and re-derived for this item (coverage-comparison.md EXEMPT-LINES 36,133,190,191; EXEMPT-GUARD-BRACE-COUNT 1), with an in-memory negative control that changes the hash (CONTROL-DIFFERS-BASELINE True).

### 5.3 Package counters (JaCoCo projection, both runs)

UtilitiesCS LINE 38909/43508 to 39100/43704 (+191 covered of +196 valid); BRANCH 9434/11293 to 9495/11356 (+61 of +63). QuickFiler LINE 10461/12754 to 10468/12761 (+7 of +7); BRANCH unchanged 2518/3217. Every other package identical, which corroborates that the change touched only the two assemblies named in the write set.

## 6. Test Execution Metrics

| Run | Command (committed projection) | Total | Passed | Failed | Exit |
| --- | --- | --- | --- | --- | --- |
| Baseline full run with coverage (P0-T11, 2026-10-03T08-33) | dotnet-coverage collect around vstest.console.exe, nine assemblies, repository runsettings, stall-probe filter | 7361 | 7361 | 0 | 0 |
| Scoped SortEmail family (P6-T5, 12-38) | vstest.console.exe UtilitiesCS.Test.dll, FullyQualifiedName~EmailIntelligence.SortEmail_ | 55 | 55 | 0 | 0 |
| Scoped QuickFiler.Test (P6-T6, 12-40) | vstest.console.exe QuickFiler.Test.dll, EfcDataModelFilerCleanupTests then EfcDataModelArchiveRootTests | 3 and 11 | 3 and 11 | 0 | 0 and 0 |
| Final full run with coverage (P6-T7, 12-46) | as baseline | 7393 | 7393 | 0 | 0 |

Delta +32 rows: SortEmail_SaveCase_Tests 12, SortEmail_AttachmentSaving_Tests 11, SortEmail_UndoAndMoveLog_Tests 2, EfcDataModelFilerCleanupTests 3, T12 1, SortEmail_Tests 15 to 18 (+3: one test removed, two tests became three-row data tests). Every test platform figure (error, timeout, aborted, notExecuted, inconclusive) is 0 in both full runs.

Clock cross-check: the post-change Cobertura root `timestamp="1791045728"` (12:42:08 -0400) sits 4 minutes before the artifact label 12-46 and 283 seconds before the P6-T7 commit epoch 1791046011; the 2026-10-06 labels (13-20 to 15-23) match their commit epochs (1791307274 = 13:21:14 through 1791314671 = 15:24:31) to the minute. Labels are clock-consistent.

## 7. Code Quality Checks

| Check | Command or method | Result |
| --- | --- | --- |
| Confidentiality masking scan | Grep over the feature folder for the account name, the short profile name, the session worktree name and the item worktree name | 0 hits in 89 files; CMD-SWEEP (p6-t46) ACCOUNT-TOKEN-FILES, PROFILE-LEAF-FILES, MACHINE-TOKEN-FILES, WORKTREE-ROOT-FILES, USERS-PATH-FILES all 0 |
| Suppression scan (added lines) | Grep of `^\+` lines in the code diff for `#pragma warning`, `SuppressMessage`, `[Obsolete`, `dynamic ` | 0 hits |
| Workflow change scan | Diff path list | no path under .github/ |
| Dead-symbol scan | Grep over every .cs file for `SaveAttachmentsOld`, `IsPicture`, `SortItemsToExistingFolder`, `_responseSaveFile`, `MAX_PATH`, `SanitizeArray(`, `Debug.WriteLine` (partials) | only two comment mentions of SortItemsToExistingFolder in the two unchanged ToDoModel.Test files |
| Banned-using scan (AC17) | Grep over the five partials for `using System.Diagnostics;`, `Deedle`, `SDILReader`, `using Outlook =`, `using UtilitiesCS;`, `ShowDialog(` | 0 hits; using counts 8, 5, 10, 9, 9 match the spec blocks |
| Exclusion attribute census (AC13) | Count of `ExcludeFromCodeCoverage` across the five partials | 18 (59 matches of usings-plus-attribute minus 41 usings), the eighteen members the spec keeps |
| Host-path and raw-document sweep of this review | the three artifacts use repository-relative paths only | clean by construction |

## 8. Gaps and Exceptions

- G-1 (non-blocking, recommend filing). `QuickFiler/Controllers/EfcDataModel.cs` reads 76.34% lines and 73.08% branches after the change against 75.69% and 73.08% before. The per-file threshold limb fails on a pre-existing condition; the no-regression limb passes (every changed line and the new member hit; the file improved). The uncovered remainder is Outlook-interop orchestration (`TryGetFirstInSelection`, `InvokeFilerAsync`, `PackageItems`, `FindMatches`, `RefreshSuggestions`) in a QuickFiler controller, the class category CLAUDE.md UT2 exemption (c) names, and raising a 485-line controller to the floor is outside a bugfix's minimal-fix mandate (CLAUDE.md Bugfix Workflow step 2). Disposition: not a blocking finding of this item; file a coverage-uplift potential entry for EfcDataModel (unrelated, for filing).
- G-2. No `artifacts/pr_context.summary.txt` or appendix exists in the item worktree, and they could not be regenerated here (no shell). Scope was derived from the caller-supplied full diff and code diff against merge base `942873699`, which equals the worktree reflog's first entry and the footprint gate's anchor, and cross-checked by Read and Grep of the worktree. The session checkout's pair belongs to another branch (#964) and was not used.
- G-3. The review label `2026-10-06T15-30` is not a host-clock reading (see the header). The executor's labels were checked against commit epochs and the Cobertura root epoch and are consistent.
- G-4. After the final toolchain pass (2026-10-03T12-54) the plan absorbed one `///` documentation-comment replacement in `SortEmail_Tests.cs` (P6-T13, 2026-10-06T13-20, one line removed and one added per P6-T16), verified by CSharpier format and check, census, footprint and identity gates but not by a repeated rebuild or coverage run. A comment line cannot change compiled output or coverage; the caller records that a full toolchain pass follows PR-time reconciliation with origin/main. Accepted, non-blocking.
- G-5. `quality-tiers.yml` is absent at the repository root (Glob), so the tier-dependent gates of .claude/rules/quality-tiers.md cannot be evaluated; pre-existing and recurring (promoted at #956).
- G-6. The test-location rule of .claude/rules/general-unit-test.md (`tests/` mirror) is not the convention of this solution; the item follows the established `<Project>.Test/` layout. Pre-existing, not a finding of this item.
- G-7. `validate_evidence_locations.py` was not executed; the Evidence Location Compliance section was established by enumerating the 26 diff paths.
- Observation. spec.md AC3 says the core "has exactly one catch clause"; the core's outer `try` has exactly one catch clause while the pre-existing inner `try` keeps its `catch (System.Exception inner)`, which the same AC names. AC13 says each `GetAttachmentsInfo` test "gains one DataRow row" while each gained two (the spec's row plus the complementary row of planner decision PD-6, disclosed in pr-description-inputs). Both are wording drift; the intent of each AC holds (feature-audit).
- Observation. `UtilitiesCS.Test/EmailIntelligence/SortEmail_SaveCase_Tests.cs` carries an unused `using System;` (IDE0005 is suggestion-level and not enforced by the build); CR-3.
- Observation. `QuickFiler/Legacy/QfcController.cs:792` calls `SortEmail.Cleanup_Files()` after `SortEmail.Run(...)` with no `try`/`finally`, the same root cause as the EfcDataModel fix; the file has no `Compile Include` in QuickFiler.csproj and `SortEmail.Run` does not exist in the partials, so the file cannot compile and has no runtime effect; CR-2.

## 9. Summary of Changes

- Production (UtilitiesCS): `SaveCase` labels corrected (L1); `TrySaveAttachmentAsync` forwards to a private core with a one-clear retry bound under a held YesToAll, logger calls replace `Debug.WriteLine`, the no-op outer rethrow is removed (L2, #966); three `YesNoToAllPromptSession` fields, an `AllPromptSessions` property and a `foreach` cleanup replace four enum fields (L3, F1); seamed `SaveAttachment`, `SaveAttachmentAsync` and `SaveCaseAsync` cores with a nested `TrySaveAttachmentDelegate`; `RedirectSaveFolder` re-roots both save paths; `WriteCSV_StartNewFileIfDoesNotExist` gains a four-parameter core that writes one tab-separated header line at the folder-then-file path only when absent, `MovedMailsHeader` added, `SanitizeArray` deleted (L4, header); seven `[ExcludeFromCodeCoverage]` removed, eighteen kept (F3); using blocks pruned (D10); `SortEmail.LegacyAttachmentSaving.cs` and its project entry deleted (F2).
- Production (QuickFiler): `MoveToFolderAsync` wraps the filer call in `try`/`finally` calling the new `protected internal virtual ResetFilerPromptState()`.
- Production (ToDoModel): the uncompiled duplicate `SortItemsToExistingFolder.cs` deleted.
- Tests: new `SortEmail_SaveCase_Tests` (12 rows), `SortEmail_AttachmentSaving_Tests` (11), `SortEmail_UndoAndMoveLog_Tests` (2), `EfcDataModelFilerCleanupTests` (3); T12 and the `CreateDirectoryLimit` tripwire added to the try-save tests; `SortEmail_Tests` loses the `SanitizeArray` test, gains two rows per `GetAttachmentsInfo` test and one corrected doc comment; four Compile Include lines.
- Docs: the #956 spec receives the three CR-1 edits; this feature folder's spec, plan, research and 89 evidence files; the promotion record under docs/features/potential/promoted/.

## 10. Compliance Verdict

PASS. Zero blocking findings. All general and C# code-change and unit-test policy requirements are met on the evidence cited; first-party C# coverage is 85.39% lines and 79.81% branches, not lower than baseline, with 98.1% line coverage on the new or changed members. Non-blocking items for the orchestrator: CR-1 (one test for the synchronous core's image arm, related, in-item), CR-2 (uncompiled legacy call site, decision owed), CR-3 (unused using in a test file), G-1 (EfcDataModel pre-existing sub-floor coverage, for filing), and the two pull-request-dependent acceptance criteria AC6 and AC27, which the pr-author step closes.

## Appendix A: Test Inventory

SortEmail family, 55 rows, all Passed (pass-after-regression-tests.md, P6-T5):

- SortEmail_SaveCase_Tests (12): SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No], [NoToAll]; SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes], [YesToAll]; SaveCase_WhenAnswerIsEmpty_DoesNotSave; SaveCaseAsync_WhenAnswerIsYesOrYesToAll_SavesToRequestedPathWithoutAltNamePrompt [Yes], [YesToAll]; SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePathAndReleasesAnswer; SaveCaseAsync_WhenAnswerIsNoToAllAndAltNameAnswerIsYesToAll_KeepsAnswerAcrossCalls; SaveCaseAsync_WhenAnswerIsNoAndAltNameAnswerIsNoToAll_DoesNotSaveAndKeepsAnswer; SaveCaseAsync_WhenAnswerIsNoAndAltNamePromptIsCancelled_DoesNotSaveAndStaysAskable; SaveCaseAsync_WhenAnswerIsEmpty_DoesNothing.
- SortEmail_AttachmentSaving_Tests (11): SaveAttachmentAsync_WhenFileDoesNotExist_SavesWithoutPrompting; SaveAttachmentAsync_WhenFileExistsAndAttachmentIsImage_AsksPicturesPromptOnly; SaveAttachmentAsync_WhenFileExistsAndAttachmentIsDocument_AsksAttachmentsPromptOnly; SaveAttachmentAsync_WhenOverwriteAnswerIsYes_ReleasesAnswerAfterSave; SaveAttachmentAsync_WhenOverwriteAnswerIsYesToAll_KeepsAnswerAndDoesNotAskAgain; SaveAttachmentAsync_WhenOverwriteAnswerIsNoAndAltNameAnswerIsYes_SavesToAlternatePath; SaveAttachment_WhenFileDoesNotExist_SavesDirectly; SaveAttachment_WhenFileExistsAndAnswerIsYes_OverwritesAndReleasesAnswer; SaveAttachment_WhenFileExistsAndAnswerIsNoToAll_SavesToAlternatePathAndKeepsAnswer; RedirectSaveFolder_RerootsPrimaryAndAlternateSavePaths; Cleanup_Files_ResetsEveryPromptSession.
- SortEmail_UndoAndMoveLog_Tests (2): WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader; WriteCSV_WhenFileExists_DoesNotWrite.
- SortEmail_TrySaveAttachment_Tests (12): the eleven pre-existing T1 to T11 (unchanged) plus TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear (T12).
- SortEmail_Tests (18): the twelve unchanged single tests (including Cleanup_Files_DoesNotThrow, TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile, TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave, SaveMessageAsMSG and SaveMessageAsMsgAsync, SanitizeArrayLineTSV, two StripTabsCrLf, two SortAsync null-argument, two InitializeSortToExisting) plus GetAttachmentsInfo_WhenSavingPicturesOnly_FiltersOutDocumentsAndOleAttachments (3 rows) and GetAttachmentsInfoAsync_WhenSavingAttachmentsOnly_FiltersOutPicturesAndOleAttachments (3 rows).

QuickFiler.Test (P6-T6): EfcDataModelFilerCleanupTests (3): MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates; MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce; MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState. Pin suite EfcDataModelArchiveRootTests (11), unchanged, all Passed.

Red-first observations (negative-controls.md): L1 four rows red (`but was 0 times`); L3 phase one one row red (`but found YesNoToAllResponse.YesToAll`); L4 two tests red (`differs at index 0`, `NullReferenceException`); L2 T12 red with the sentinel `InvalidOperationException`; re-rooting red on the alternate-path assertion; EfcDataModel throwing test red (`but found 0`); mutation control on the structural test red (`but found 3`), file restored byte-identical.

## Appendix B: Toolchain Commands Reference

1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` (P6-T1, P6-T2; re-check after P6-T13 in P6-T14).
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (P6-T3).
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (P6-T4).
4. `dotnet-coverage collect --output coverage\final-959.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-959.config -- vstest.console.exe <nine test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\959\final" "/Logger:trx;LogFileName=final-959.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"` (P6-T7, the DIRECT route of Invoke-MSTestWithCoverage.ps1; the same command with `baseline` names at P0-T11).
5. Scoped runs: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_" ...` (P6-T5) and the two QuickFiler.Test filters (P6-T6).

No command was re-run by this review.
