# Policy Audit: csharp-latent-hazards-uithread-ilglobals-comments (Issue #930), Remediation Cycle 1 Re-audit

- Timestamp (caller-supplied artifact stamp): 2026-09-29T10-16
- Audit type: re-audit at the exit of remediation cycle 1 (prior reduced audit: 2026-09-29T00-45)
- Branch: `bug/csharp-latent-hazards-uithread-ilglobals-comments-930`
- Base: `origin/main`; execution self-anchor BASE-SHA `ac819907f479ee18026993054e714dc2e056142f` (evidence/baseline/tree-anchor.md); cycle self-anchor `39845d4a3f2f38d5c018f41e15e8372d432553c5` (evidence/qa-gates/r1-footprint.md)
- Work mode: `minor-audit` (issue.md marker). AC source: issue.md `## Acceptance Criteria` (AC1 to AC7).
- Reviewer: feature-review agent, reduced (small-path) re-audit
- Review worktree: `<repo-root>/.claude/worktrees/agent-ab7f72be619adf22f`

## Template Resolution Deviation

The MCP tools `resolve_policy_audit_template_asset` and `validate_orchestration_artifacts` are not on this agent's tool surface, and the caller prohibited the Bash tool. The artifact was hand-authored preserving the twelve canonical major headings of `.claude/skills/policy-audit-template-usage/SKILL.md`. PR-context artifacts do not exist in the review worktree; scope was taken from the prior audit, the cycle footprint evidence (r1-footprint.md, r1-toolchain-exemption.md) and direct reads and Grep scans of the files on disk. No command was executed by this reviewer; no figure below is a reviewer re-measurement except the Grep scans identified as such.

## Rejected Scope Narrowing

None detected. The caller's statement that no source, test or project file changed in the cycle and that the C# evidence is unchanged is a factual claim, not an instruction to skip a check. It was verified against r1-footprint.md (`OUTSIDE_TRACKED_CHANGES=0`), r1-toolchain-exemption.md (`CODE_OR_CONFIG_TRACKED_CHANGES=0`, `CODE_OR_CONFIG_PORCELAIN=0`) and a spot Grep of the changed production files. The C# coverage verdict below is stated explicitly for the six C# files that remain in the branch diff.

## Evidence Location Compliance

- The branch diff outside the feature folder and `.claude/agent-memory/` lists only the six code paths recorded in evidence/qa-gates/footprint.md; the cycle added no path outside the feature folder (r1-footprint.md: `OUTSIDE_TRACKED_CHANGES=0`, `PORCELAIN_OUTSIDE_ALLOWED=0`). No path under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/` appears. Verdict: PASS.
- Cycle evidence lives under `<FEATURE>/evidence/remediation-baseline/` (r1-phase0-instructions-read, r1-tree-anchor, r1-drive-scan-baseline, r1-file-state-baseline, r1-sanitize-baseline, r1-ac-baseline) and `<FEATURE>/evidence/qa-gates/` (r1-subst-1 to r1-subst-5, r1-footprint, r1-drive-scan-final, r1-sanitize-final, r1-toolchain-exemption, r1-ac-status). Canonical.
- `validate_evidence_locations.py` does not exist in this repository; the manual substitute (review of the anchored name-only diff and porcelain listing) was applied.
- No EVIDENCE_LOCATION_OVERRIDE_REJECTED event.
- This review's three artifacts are written into the feature folder in the review worktree only; the orchestrator handles hook path resolution.

## Executive Summary

**Overall verdict: PASS. Blocking: 0. Non-blocking: 0. Informational: 9 (carried forward).**

The prior audit's single Non-blocking finding (NB-1, absolute Visual Studio install path on two committed evidence lines and in the quotations of the three prior audit artifacts) is resolved. Verified by this reviewer: a Grep over the whole feature folder for the drive-rooted pattern `\b[A-Za-z]:[\\/]\S` returns zero matches, and the same pattern returns 26 matches against the gitignored raw coverage log (independent positive control). The five substituted files carry the placeholder `VS-INSTALL-ROOT` (evidence files: one occurrence each; policy-audit: two; feature-audit: two; code-review: one). Each substitution is proven text-exact: the pre-edit masked hash (r1-file-state-baseline.md, computed by applying only the path-prefix regex to the pre-edit text) equals the post-edit raw hash in r1-subst-1 to r1-subst-5, and line counts are unchanged (229, 230, 64, 270, 68). No tracked change exists outside the feature folder and `.claude/agent-memory/`, and no C# source, test, project, props, targets, solution, runsettings or configuration file changed in the cycle. AC7 is therefore met and remains checked in issue.md; AC1 to AC6 rest on unchanged evidence and still pass.

The code diff and its toolchain evidence are those of the prior audit: CSharpier check over 1623 files exit 0, analyzer Rebuild 0 errors and 0 warnings, TreatWarningsAsErrors Rebuild 0 errors, 7322 of 7322 tests passed, first-party coverage 85.31% lines / 79.71% branches to 85.32% lines / 79.73% branches, changed executable line covered. Both floor sets (CLAUDE.md 80/75 with 90 for new code; `.claude/rules` 85/75) are satisfied.

Procedural note (not a finding): at review time the cycle's edits are working-tree changes (tracked modifications and untracked r1 evidence files, r1-footprint.md `PORCELAIN_IN_FEATURE=17`), not yet committed; the caller instructed that this review not commit. AC7 concerns committed files, so the condition holds once the cycle is committed in the exempt docs-only form. Nothing in the tree contradicts that outcome.

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | #889 test added to `UiThreadPredicateHardening_Tests`, which already carries `[DoNotParallelize]` for process-global `UiThread` statics; the two #863 tests are reflection-only. No new attribute, worker count unchanged (evidence/qa-gates/concurrency-regime.md). Unchanged by the cycle. |
| Isolation | PASS | Each new test targets one predicate exit or one structural property. |
| Fast execution | PASS | No sleeps, delays or retries added (`ADDED_Thread.Sleep=0`, `ADDED_Task.Delay=0`, `ADDED_Retry=0`). |
| Determinism | PASS | Test-owned STA dispatcher host and MTA thread, joined; no wall-clock waits; no temporary files. |
| Readability | PASS | Descriptive names, XML summaries, Arrange/Act/Assert comments. |

### 1.2 Coverage and Scenarios

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 6 | 7322 | 7322 passed / 0 failed | 85.31% lines / 79.71% branches | 85.32% lines / 79.73% branches | 100% lines (1 of 1 changed executable line) |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |
| PowerShell | 0 | N/A | N/A | N/A | N/A | N/A |

C# coverage verdict: PASS (repo-wide first-party 85.32% lines and 79.73% branches; UiThread.cs 97.76%, ILGlobals.cs 94.74%; changed-line coverage 100%; no regression on changed lines). The cycle did not touch any C# file, so the coverage evidence (evidence/baseline/baseline-04-mstest-coverage.md, evidence/qa-gates/final-06-mstest-coverage.md and the two package-level JaCoCo projections) is unchanged in every figure; the only change to the two summary files is the path-prefix text on one line each, proven by hash equality (r1-subst-1, r1-subst-2). Figures in both files re-read by this reviewer: baseline `lines 56079/65737 (85.31%), branches 13593/17052 (79.71%)`; final `lines 56084/65736 (85.32%), branches 13597/17054 (79.73%)`.

Per-file detail (unchanged; transcribed in the two evidence summaries):

- `UtilitiesCS/Threading/UiThread.cs`: 130/133 (97.74%) to 131/134 (97.76%); uncovered lines 38, 39, 40 in both runs; changed operand line has HITS=1; condition coverage on the return expression 2/2 to 4/4.
- `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`: 38/40 (95.00%) to 36/38 (94.74%); deletion-only diff, uncovered count 2 in both runs; the percentage change is arithmetic (two covered initializer lines deleted), not a changed-line regression.
- Two QuickFiler `.Search.cs` files: comment-only edits.

Method note (Decision D3): four shell-icon test classes were excluded identically from both local coverage runs; CI runs them unfiltered. The exclusion is a test-selection filter, not a coverage `exclude` entry on a production path, so the Coverage Exclusion Policy blocking rule is not engaged.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/baseline-coverage.jacoco.xml` plus `evidence/baseline/baseline-04-mstest-coverage.md`
- C# post-change coverage artifact: `evidence/qa-gates/final-coverage.jacoco.xml` plus `evidence/qa-gates/final-06-mstest-coverage.md`
- TypeScript baseline coverage artifact: `N/A - zero TypeScript files changed on this branch`
- TypeScript post-change coverage artifact: `N/A - zero TypeScript files changed on this branch`
- PowerShell baseline coverage artifact: `N/A - zero PowerShell files changed on this branch`
- PowerShell post-change coverage artifact: `N/A - zero PowerShell files changed on this branch`
- Python baseline coverage artifact: `N/A - zero Python files changed on this branch`
- Python post-change coverage artifact: `N/A - zero Python files changed on this branch`
- Per-language comparison summary: section 1.2.1 of this document

Note on the canonical artifact path: `artifacts/csharp/coverage.xml` does not exist in the review worktree, and CLAUDE.md prohibits committing the raw Cobertura document. The committed package-level JaCoCo projections plus the transcribed per-file parse serve as the coverage artifact, consistent with prior reviews of this repository.

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.31% lines (56079/65737) / 79.71% branches (13593/17052) -> Post-change: 85.32% lines (56084/65736) / 79.73% branches (13597/17054). Change: +0.01% lines / +0.02% branches. New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/qa-gates/coverage-comparison.md; evidence/baseline/baseline-coverage.jacoco.xml; evidence/qa-gates/final-coverage.jacoco.xml.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Artifact State

Same-session baseline and final runs (identical runner, filter and machine), the comparison basis this repository's prior reviews require. Unchanged by the cycle.

### 1.3 Scenario Completeness

| Scenario class | Verdict | Evidence |
|---|---|---|
| Positive flow | PASS | Pre-existing true-outcome `IsCompleted` tests (condition 4/4). |
| Negative flow | PASS | New null-dispatcher test; pre-existing different-dispatcher test. |
| Edge / boundary | PASS | Null-equals-null boundary; `PublicStaticFields_AreExactlyTheTwoOpCodeTables` pins the surface. |
| Error handling | PASS | No exception path added. |
| Concurrency | PASS | Explicit MTA thread with a separate STA host under the class-level attribute. |
| State transitions | PASS | Not applicable. |

### 1.4 Test Structure and Diagnostics

PASS. Arrange/Act/Assert comments, descriptive names, FluentAssertions with because-clauses.

### 1.5 External Dependencies and Environment

PASS. No network, database, file, process or temporary-file dependency added.

## 2. General Code Change Policy Compliance

| Section | Verdict | Evidence |
|---|---|---|
| Bugfix workflow | PASS | #889: 889-fail-before.md (1 of 3 failed) then fix then 889-pass-after.md (128/128). #863: 863-fail-before.md (2 of 15 failed) then fix then 863-pass-after.md (15/15). #862: comment-only edit gated by token census. Cycle 1 is documentation-only. |
| Design principles | PASS | Single boolean operand; dead members removed. |
| Classes, functions, APIs | PASS | No new production type or method. |
| Error handling and logging | PASS | No change. |
| Module and file structure (500-line limit) | PASS | evidence/qa-gates/final-03-file-size.md: all six changed files at most 500 lines. The cycle added no code file; Markdown files are exempt. |
| Naming, docs, comments | PASS | Why-comment retained; #862 numerals removed. |
| Performance, I/O, dependencies | PASS | No dependency or project file change (r1-toolchain-exemption.md: zero). |
| Interaction with existing code | PASS with obligation | Removal of public `ILGlobals.Cache` and `ILGlobals.modules` is a breaking public API change with no in-repo consumer (this reviewer's Grep over `*.cs` for `ILGlobals\.(Cache|modules)\b|public static .*\b(Cache|modules)\b` over the worktree returned zero matches; solution Rebuild 0 errors, 863-build-green.md). The change-description obligation is discharged only when the PR body names both members (I-3). |
| After making changes (toolchain loop) | PASS | toolchain-final-pass.md `LOOP: CLEAN PASS`, one iteration. The cycle changed no C# input, so the loop result stands (r1-toolchain-exemption.md). |
| Architecture boundaries | PASS | No new VSTO, Interop, COM or Ribbon reference. |

## 3. Language-Specific Code Change Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| CSharpier via `dotnet tool run` | PASS | final-01 and final-02: exit 0, 1623 files. |
| Analyzer Rebuild | PASS | final-04-analyzers.md: exit 0, 0 errors, 0 warnings, 18 project compiles (non-vacuous). |
| Nullable Rebuild | PASS | final-05-nullable.md: exit 0, 0 errors, `CS86_ERROR_LINES=0`; no `/p:Nullable=enable`. |
| Type and null safety | PASS | The guard is the null-safety fix. Reviewer Grep of UiThread.cs finds `_dispatcher is not null` at lines 184 (captured-context exit) and 199 (dispatcher exit). |
| Public surface minimal | PASS | Two public mutable statics removed; two remaining are `readonly`, pinned by test. |
| XML docs synchronized | PASS | Reviewer Grep of `QuickFiler/Viewers` for `(487|481) lines`: zero matches. |
| Analyzer stack wiring | PASS (pre-existing environment note) | No tracked wiring changed; HintPath skew is pre-existing on origin/main (I-2). |
| Prohibited behaviors | PASS | Diff confined to six code files; no weakened assertion, sleep or retry. |

## 4. Language-Specific Unit Test Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| MSTest, Moq, FluentAssertions | PASS | New tests use `[TestMethod]` and FluentAssertions; no mocking needed. |
| Test placement | PASS | `UtilitiesCS.Test/` mirrors the production folders. |
| Coverage floors (CLAUDE.md 80/75, 90 new; rules 85/75) | PASS | Section 1.2. |
| Coverage regression on changed lines | PASS | Changed executable line hit; uncovered counts unchanged. |
| Determinism infrastructure | PASS | No banned API added. |

## 5. Test Coverage Detail

- Production files changed: 4. Executable-line changes: 1 added operand line (covered, HITS=1); 3 deleted lines; 2 comment-only lines. Unchanged by the cycle.
- New production code coverage target (90%): satisfied at 100% (1 of 1).
- Condition coverage of the modified return expression: 2/2 to 4/4.
- Test methods: 7320 baseline, 7322 post-change (three tests added, one deleted).

## 6. Test Execution Metrics

- Baseline full suite: 7320 executed, 7320 passed, 0 failed (baseline-test-summary.txt).
- Final full suite with coverage: 7322 executed, 7322 passed, 0 failed; `SEQUENCE_FILES=0` (final-test-summary.txt, final-06-mstest-coverage.md).
- #889 scoped: fail-before 1 of 3 failed; pass-after 128/128. #863 scoped: fail-before 2 of 15 failed; pass-after 15/15.
- Parallel regime: Workers 0, ClassLevel, runsettings hash unchanged.
- Cycle 1 ran no test, by design: r1-toolchain-exemption.md shows `CODE_OR_CONFIG_TRACKED_CHANGES=0` and `CODE_OR_CONFIG_PORCELAIN=0` with `CONTROL_MD_PORCELAIN=24` (non-blind filter).

## 7. Code Quality Checks

- Format check: PASS. Lint errors: 0. Type errors: 0. Architecture violations: 0. File size: PASS.
- Committed evidence format (CLAUDE.md Committed Test Evidence Format): PASS. `RAW_TOOL_DOCS=0` over the feature folder (r1-sanitize-final.md, 64 files); the tool-derived committed evidence is two package-level JaCoCo projections and two trx-derived summaries.
- Evidence hygiene: PASS (previously PARTIAL). Verification:
  - Reviewer Grep of the whole feature folder for `\b[A-Za-z]:[\\/]\S`: zero matches. Reviewer positive control against the gitignored raw coverage log: 26 matches (equal to the executor's `CONTROL_LOG_HITS=26`).
  - r1-drive-scan-baseline.md: 7 hits in 5 files over 53 files, with controls (synthetic backslash and slash paths match, a URL does not match, log control 26). r1-drive-scan-final.md: 0 hits over 63 files, same controls.
  - r1-sanitize-final.md: `ACCOUNT_HITS=0`, `HOST_HITS=0`, `ROOT_HITS=0`, `DRIVE_USERS_HITS=0`, `RAW_TOOL_DOCS=0`, each with positive controls of at least 15.
  - Substitution exactness: r1-subst-1 to r1-subst-5 each report `RAW_TEXTHASH` equal to the expected pre-computed masked hash, `OCC=0`, `DRIVE_MATCHES=0`, and unchanged line counts. The hash chain is sound because the expected value was computed by applying only the path-prefix replacement to the pre-edit text, so equality after the edit proves nothing else in the file changed.
  - The reviewer read the two evidence summaries and confirmed the substituted line now reads `Runner output: Using vstest.console: VS-INSTALL-ROOT\Common7\IDE\Extensions\TestPlatform\vstest.console.exe; Discovered 9 test assemblies.` with the surrounding figures intact.
  - The gate gap that let NB-1 escape (four identity patterns, no general drive-rooted pattern) is closed for this folder by the general scan; that the gate is non-vacuous is shown by the two controls.

## 8. Gaps and Exceptions

No Blocking and no Non-blocking finding. NB-1 is resolved (see section 7). Carried-forward Informational findings:

### I-1 (Informational): Phase 0 `Timestamp:` values were estimates, later replaced by file write times

Disclosed in reduced-audit-handoff.md; affects no measured figure and no acceptance criterion; the sequence is monotone. The disclosure does not enumerate the corrected artifacts. Unchanged by the cycle (the cycle's out-of-scope list recorded it).

### I-2 (Informational, pre-existing, out of footprint): analyzer HintPath skew on origin/main

`UtilitiesCS.csproj` and `VBFunctions.csproj` name Meziantou.Analyzer 3.0.235 in `<Analyzer Include>` items while packages.config and the props import name 3.0.290; `SVGControl.Test.csproj` names MSTest.Analyzers 4.4.0 against 4.4.1. No tracked file changed by this branch. Follow-up issue recommended.

### I-3 (Informational): breaking public API change obligation

`ILGlobals.Cache` and `ILGlobals.modules` removed. No in-repo consumer. The PR body must name both removed public members. `quality-tiers.yml` is absent from the branch root, so the tier "major bump" gate is not operable; the call-out is the applicable control.

### I-4 (Informational): unused `using System.Collections.Generic;` left in ILGlobals.cs

No enabled analyzer flags it (0 warnings). One-line cleanup for a later touch; the minimal-fix rule permits leaving it.

### I-5 (Informational): six UtilitiesCS lines outside the changed files flipped missed-to-covered between runs

0.009 percentage points, within the known run-to-run variance; not attributable to the diff.

### I-6 (Informational): four shell-icon test classes excluded from both local coverage runs (Decision D3)

Identical exclusion on both sides; CI runs them unfiltered. Candidate follow-up: `Invoke-MSTestWithCoverage.ps1` exposes no filter or hang-timeout extension point.

### I-7 (Informational): PR-context artifacts absent for this branch

No `artifacts/` directory exists in the review worktree; scope derived from footprint evidence and files on disk.

### I-8 (Informational): ILGlobals.cs file-level percentage decreased while its uncovered count did not

95.00% to 94.74% because two covered initializer lines were deleted; not a changed-line regression.

### I-9 (Informational, pre-existing): test file hosts two classes

`UiThreadApartmentMeasurement_Tests.cs` contains `UiThreadPredicateHardening_Tests` and `UiThreadApartmentMeasurement_Tests`; pre-existing from #816.

## 9. Summary of Changes

- Code (unchanged since the prior audit): `UiThread.cs` +2/-0; `ILGlobals.cs` +0/-3; two QuickFiler `.Search.cs` files 1/1 each; two test files (+42; +51/-6).
- Cycle 1 (documentation only): the install-root prefix replaced by `VS-INSTALL-ROOT` on one line in each of two evidence summaries and in the quotations of the three prior audit artifacts (7 substitutions in 5 files); added a general drive-rooted scan (baseline 7, final 0, with controls); re-ran the four identity counts (all 0); confirmed zero tracked change outside the feature folder and zero code or configuration change; flipped AC7 to `[x]` in issue.md (only that marker changed, issue.md hash equals the expected value in r1-ac-status.md).
- Feature folder: remediation-inputs and remediation-plan (2026-09-29T09-50), 11 cycle evidence files under evidence/remediation-baseline/ and evidence/qa-gates/ prefixed `r1-`.

## 10. Compliance Verdict

**PASS.** Blocking findings: 0. Non-blocking: 0. Informational: 9 (I-1 to I-9, carried forward). Blocking-PARTIAL items: 0. AC7 restored to PASS; AC1 to AC6 unchanged PASS. Remediation-inputs artifact: not produced, because no remediation-required finding remains.

## Appendix A: Test Inventory

New or changed tests on this branch (unchanged by the cycle):

- `UtilitiesCS.Test.Threading.UiThreadPredicateHardening_Tests.IsCompleted_WhenTheAwaiterContextIsAForeignDispatcherContextAndNoUiDispatcherWasCaptured_ReturnsFalse` (added; fail-before recorded; passes after fix).
- `UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.ILGlobals_Tests.PublicStaticFields_AreAllInitOnly` (added; fail-before recorded; passes after fix).
- `UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.ILGlobals_Tests.PublicStaticFields_AreExactlyTheTwoOpCodeTables` (added; fail-before recorded; passes after fix).
- `UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.ILGlobals_Tests.Cache_IsInitialized` (deleted with the member it asserted).

Pre-existing tests confirmed passing after the change: the twelve `IsCompleted_*` tests and the thirteen retained `ILGlobals_Tests` methods.

## Appendix B: Toolchain Commands Reference

| Step | Command (abridged) | Artifact | Result |
|---|---|---|---|
| Format (write) | `dotnet tool run csharpier format .` | evidence/qa-gates/final-01-csharpier-format.md | exit 0, patch unchanged |
| Format (check) | `dotnet tool run csharpier check .` | evidence/qa-gates/final-02-csharpier-check.md | exit 0, 1623 files |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | evidence/qa-gates/final-04-analyzers.md | exit 0, 0 errors, 0 warnings |
| Nullable | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | evidence/qa-gates/final-05-nullable.md | exit 0, 0 errors |
| Tests with coverage | `Invoke-MSTestWithCoverageMain` from `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (Decision D3 override) | evidence/qa-gates/final-06-mstest-coverage.md | completed, 7322/7322 |
| Cycle 1 drive-rooted scan | pwsh pattern scan with synthetic and log controls | evidence/remediation-baseline/r1-drive-scan-baseline.md; evidence/qa-gates/r1-drive-scan-final.md | 7 hits to 0 hits |
| Cycle 1 footprint and exemption | anchored `git diff --name-only` and porcelain listing | evidence/qa-gates/r1-footprint.md; evidence/qa-gates/r1-toolchain-exemption.md | 0 outside; 0 code or configuration |

Reviewer verification method for this re-audit: Read, Grep and Glob over the worktree only (no Bash, per caller instruction): whole-folder drive-rooted Grep with a raw-log positive control, placeholder-occurrence counts per file, reading of every r1 evidence artifact and both substituted evidence summaries, spot Grep of UiThread.cs, `*.cs` and QuickFiler/Viewers for the fixed defects.
