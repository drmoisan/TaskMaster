# Policy Audit: Dependabot repair, borrowed packages and transitive redirects (Issue #985)

- Branch: `bug/dependabot-repair-borrowed-packages-and-transitive-redirects-985`
- Head: `07d3a8a982d605eeb3bca77810955b4de5773ea8`
- Base: `origin/main` at merge base `9911fe138952e2b93476850582847c2831e1cbbd` (committed 2026-10-07T08:22:27-04:00)
- Review timestamp: 2026-10-09T15-35 (re-audit after remediation cycle R1, commit `07d3a8a98`)
- Work mode: `full-bug` (AC source: `spec.md`)
- Reviewer: feature-review agent

## Template Provenance

The structure of this artifact follows the canonical major headings listed in `.claude/skills/policy-audit-template-usage/SKILL.md` step 5 (Executive Summary, sections 1 to 10, Appendix A and Appendix B); the template-asset selector step was not run from this agent session. The artifact is validated with `validate_orchestration_artifacts` (artifact type `policy-audit`).

## Rejected Scope Narrowing

No scope narrowing was detected in the caller prompt. The caller supplied the base branch, merge base, head and feature folder; the PR-context summary (generated 2026-10-09 19:32:00 UTC) names head `07d3a8a98`, equal to `git rev-parse HEAD`, so it is current. The audit scope is the full branch diff `9911fe138...07d3a8a98` (107 files, +5386/-691), not only the R1 commit.

## Executive Summary

The branch declares four borrowed NuGet packages in three test-project manifests, removes a duplicate `Microsoft.Web.WebView2.Core` reference from `QuickFiler.Test.csproj`, adds a solution-wide binding-redirect synchronisation module (`scripts/dependencies/BindingRedirectSync.psm1`), wires it unconditionally into `Repair-PackageManifestConsistency.ps1`, and adds a repository Pester gate over orphaned HintPaths. Remediation cycle R1 removed the account name from the committed plan (B-1) and, in scope, added a `Direction` field to sync repair records and report lines (CR-1), de-duplicated `WrittenPath` (CR-2) and made the handled-name check case-insensitive (CR-3), each with fail-before and pass-after evidence.

Overall verdict: **PASS (no blocking findings)**.

- B-1 closed: reviewer sweep of all 5386 added lines of `git diff 9911fe138...HEAD` finds 0 occurrences of the account name, its 8.3 short form, the host name, either user e-mail address, or a drive-rooted user-profile path; the positive control (the worktree `.git` pointer file) matches the account token.
- Toolchain: PASS. PowerShell format, analyze and test pass (executor R1 iteration 1 evidence; reviewer in-session Pester re-run 447/447). C# toolchain evidence from cycle 0 remains valid because R1 changed no C#-family file (`evidence/qa-gates/csharp-not-applicable-r1.2026-10-09T15-30.md`, confirmed by `git show --stat 07d3a8a98`).
- Coverage: PASS for both languages with changed files (PowerShell and C#; section 1.2.1).
- AC status: 6 of 7 PASS; AC7 is pending CI by design (post-merge `@dependabot recreate` on PR #984) and is not a blocking finding.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Every `It` in `BindingRedirectSync.Tests.ps1` builds its own text and provider; `RedirectSync.Tests.ps1` uses Context-scoped `BeforeAll` fixtures, including the new double-write Context. |
| Isolation | PASS | One behaviour per `It`; N1 to N4 each pin one `Direction` outcome, N5 the case-insensitive skip, N8 and N9 separate the fixture guard from the de-duplication assertion. |
| Fast execution | PASS | Reviewer run of 447 tests over three suites completed in one in-session invocation. |
| Determinism | PASS | Literal strings and hashtables only; no clock, RNG or network. |
| Readability | PASS | Arrange/Act/Assert comments; N5 carries a comment explaining the case-insensitive hashtable lookup. |
| No temporary files | PASS | `evidence/qa-gates/ps-temp-file-audit-r1.2026-10-09T15-29.md`: 0 in both changed test files, positive control at least 1; reviewer read confirms in-memory stores (`X:\fixture\...` keys). |

### 1.2 Coverage requirements

Coverage thresholds applied: line 85% and branch 75% (branch-capable languages only) per `.claude/rules/quality-tiers.md`; new code at least 90% per CLAUDE.md UT2. Pester measures no branch coverage, so no PowerShell branch threshold is evaluated.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| PowerShell | 2 production (1 new, 1 modified), 3 test | Pester 5.6.1 | 447 passed, 0 failed | 94.64% | 94.99% | 100% |
| C# | 4 build-configuration files (1 csproj, 3 packages.config); 0 .cs files | MSTest | 7427 passed, 0 failed | 85.41% | 85.41% | N/A (no executable source changed) |
| TypeScript | 0 | none | none | N/A | N/A | N/A |
| Python | 0 | none | none | N/A | N/A | N/A |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/mstest-coverage-baseline.2026-10-09T14-11.jacoco.xml` with summary `evidence/baseline/mstest-coverage-baseline.2026-10-09T14-11.summary.txt`
- C# post-change coverage artifact: `evidence/qa-gates/mstest-coverage-final.2026-10-09T14-31.jacoco.xml` with summary `evidence/qa-gates/mstest-coverage-final.2026-10-09T14-31.summary.txt` (no C#-family file changed after this run)
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `coverage/985-pester-baseline.xml` (branch baseline, 1765/1865 lines) and `coverage/985r1-pester-baseline.xml` (R1 start, 1891/1991 lines)
- PowerShell post-change coverage artifact: `coverage/985r1-pester-final.xml` (1898/1998 lines) and a reviewer in-session re-run to the session scratchpad with identical figures
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- PowerShell: Baseline: 94.64% lines (1765/1865) -> Post-change: 94.99% lines (1898/1998). Change: +0.35% lines (+133 covered, +133 measured). New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/baseline/ps-coverage.2026-10-09T14-04.md; evidence/qa-gates/ps-coverage-r1.2026-10-09T15-29.md; evidence/qa-gates/coverage-comparison-powershell-r1.2026-10-09T15-29.md.
- C#: Baseline: 85.41% lines (56496/66143) / 79.83% branches (13709/17173) -> Post-change: 85.41% lines (56495/66143) / 79.83% branches (13709/17173). Change: 0.00% lines, 0.00% branches (one covered line fewer, run-to-run variance; no .cs file changed). Disposition: PASS. Evidence: evidence/qa-gates/coverage-comparison-csharp.2026-10-09T14-31.md; evidence/qa-gates/mstest-coverage-final.2026-10-09T14-31.summary.txt.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Verdicts

PowerShell coverage verdict: PASS (direct Pester 94.99% over scripts/dependencies, scripts/hygiene and scripts/vscode; new module BindingRedirectSync.psm1 100.00% lines 120/120; modified Repair-PackageManifestConsistency.ps1 94.14% to 94.17% lines with every changed line hit).

PowerShell bundled PoshQC coverage artifact `artifacts/pester/powershell-coverage.xml`: FAIL (0.00% lines, 0/10477, regenerated 2026-10-09 15:27 by the R1 MCP test run; its 15 packages do not include `scripts/`, so it never instruments the changed files; pre-existing tooling defect, observation O-1, non-blocking).

Reviewer in-session PowerShell coverage re-run: PASS (447/447 tests, aggregate 94.99% lines 1898/1998, BindingRedirectSync.psm1 100.00% 120/120, Repair-PackageManifestConsistency.ps1 94.17% 226/240).

C# coverage verdict: PASS (line 85.41% at or above 85, branch 79.83% at or above 75; no C# executable line changed, so there is no changed-line regression).

Changed-line check for the modified PowerShell script: the uncovered lines in `Repair-PackageManifestConsistency.ps1` in the reviewer run are 56, 114, 126-130, 137, 139-143 and 327; none is a changed line. The R1 lines 466-467 (`$distinctWritten`) and 476 (`WrittenPath`) are hit, as are the cycle-0 changed lines.

### 1.3 Scenario completeness

PASS. Positive (stale rewrite, transitive rewrite, upgrade direction), negative (unknown name, unparsable version, non-configuration text), edge (empty text, block without redirect, single-version `oldVersion`, numeric versus string ordering, numerically equal but textually different versions, case-variant duplicate names, a file written by two passes), error (parser throw) and state (idempotent second pass, `-WhatIf`) scenarios are each pinned by a named `It`.

## 2. General Code Change Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| Design: simplicity and separation of concerns | PASS | `Get-RedirectDirection` is a small pure private helper; de-duplication is a single filter at the publication point. |
| Reuse | PASS | Existing parser and reconciler reused; no logic duplicated. |
| Error handling | PASS | Unparsable versions classify as `Unknown` rather than throwing; parser exceptions still propagate. |
| Logging | PASS | No new output; the existing `Write-Information` summary is unchanged. |
| File size (500 lines) | PASS | HEAD line counts: module 328, repair script 495, `BindingRedirectSync.Tests.ps1` 440, `RedirectSync.Tests.ps1` 218, `RepositoryTreeConsistency.Tests.ps1` 178. |
| Public contracts documented | PASS | `.DESCRIPTION` and `.OUTPUTS` help updated for the case-insensitive skip and the `Direction` field. |
| Dependencies | PASS | None added. |
| Host-identifier hygiene in committed files | PASS | Reviewer sweep of added lines: 0 hits for every token class (section 7). B-1 closed. |
| Bugfix workflow (failing test first) | PASS | `evidence/regression-testing/fail-before-sync-module.2026-10-09T15-22.md` (7 failures, right reason) and `fail-before-repair-written-path.2026-10-09T15-22.md` (2 failures, right reason) precede the pass-after artifacts. |

## 3. Language-Specific Code Change Policy Compliance

### 3.1 PowerShell (`.claude/rules/powershell.md`)

| Rule | Verdict | Evidence |
|---|---|---|
| Advanced functions with `CmdletBinding` and named parameters | PASS | `Get-RedirectDirection` declares `[CmdletBinding()]`, `[OutputType([string])]` and mandatory typed parameters. |
| `SupportsShouldProcess` on state-changing function | PASS | Unchanged; `Invoke-SolutionBindingRedirectSync`. |
| Approved verbs | PASS | `Get` for the new helper; PoshQC analyze clean (`evidence/qa-gates/ps-analyze-r1.2026-10-09T15-26.md`). |
| No script-scoped mutable state | PASS | `$distinctWritten` is a local of the script body, created once per run. |
| Private helper not exported | PASS | `Export-ModuleMember` lists only the three public functions. |
| Formatting | PASS | `evidence/qa-gates/ps-format-r1.2026-10-09T15-26.md`: no rewrite (porcelain and four hashes identical). |

### 3.2 C# (`CLAUDE.md` C# Code Change Policy)

| Rule | Verdict | Evidence |
|---|---|---|
| Build-configuration edits limited to intent | PASS | Unchanged since cycle 0: one 5-line `ItemGroup` removed from `QuickFiler.Test.csproj`; one WebView2.Core reference remains. |
| Manifest entries at sibling versions | PASS | WebView2 1.0.4191.47 and ObjectListView.Official 2.9.1 match the production manifests and HintPath folders. |
| CSharpier, analyzer rebuild, nullable rebuild | PASS | `evidence/qa-gates/cs-format.2026-10-09T14-30.md`, `cs-analyzers.2026-10-09T14-30.md`, `cs-nullable.2026-10-09T14-30.md`; R1 changed no C#-family file. |

## 4. Language-Specific Unit Test Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| Pester 5 with Describe/Context/It | PASS | Three test files; one new Context. |
| Test location mirrors source | PASS | `tests/scripts/dependencies/` mirrors `scripts/dependencies/`. |
| Mock signature parity | PASS | No `Mock`; in-memory delegates. |
| No executable mocking or live executables | PASS | None invoked. |
| MSTest/Moq/FluentAssertions (C#) | PASS | No C# test changed. |

## 5. Test Coverage Detail

| File | Status | Baseline line | Post-change line | Gate |
|---|---|---|---|---|
| `scripts/dependencies/BindingRedirectSync.psm1` | new | absent | 100.00% (120/120) | at least 90% new code: PASS |
| `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | modified | 93.83% (213/227) | 94.17% (226/240) | at least 85% and no changed-line regression: PASS |
| `QuickFiler.Test/QuickFiler.Test.csproj` and three `packages.config` | modified | no executable lines | no executable lines | none |

## 6. Test Execution Metrics

| Suite | Result | Source |
|---|---|---|
| Pester, dependencies + hygiene + vscode (executor R1) | 447 passed, 0 failed | `evidence/qa-gates/ps-coverage-r1.2026-10-09T15-29.md` |
| Pester via PoshQC MCP, dependencies (executor R1) | 187 tests, 0 failures, 0 errors | `evidence/qa-gates/ps-test-mcp-r1.2026-10-09T15-27.md` |
| Pester, dependencies + hygiene + vscode (reviewer in-session) | 447 passed, 0 failed, 0 skipped | reviewer run 2026-10-09T15-34 |
| MSTest, full solution (executor cycle 0) | 7427 passed, 0 failed | `evidence/qa-gates/cs-test.2026-10-09T14-31.md` |
| Integration rehearsal on PR #984 branch (cycle 0) | analyzer and nullable rebuild 0 errors; BindingRedirectVerification 16/16 | `evidence/other/integration-rehearsal.2026-10-09T14-35.md` |
| Live-tree `-WhatIf` smoke (R1) | 0 written, 0 sync repairs, porcelain unchanged | `evidence/other/whatif-live-tree-r1.2026-10-09T15-26.md` |

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
|---|---|---|---|
| Confidentiality masking scan | `git diff --unified=0 9911fe138...HEAD` added lines (5386), case-insensitive fixed-string match for the account name, its 8.3 short form, the host name and both user e-mail addresses, plus a drive-rooted user-profile regex | 0 hits for every token; positive control on the `.git` pointer file matched | PASS |
| Suppression scan (added lines under `scripts/` and `tests/`) | match for `SuppressMessage`, `#pragma`, `ExcludeFromCodeCoverage` | 0 | PASS |
| Workflow change scan | `git diff --name-only 9911fe138...HEAD -- .github scripts/benchmarks .github/actions artifacts` | empty; rule `modified-workflow-needs-green-run` not triggered | PASS |

## Evidence Location Compliance

`git diff --name-only 9911fe138...HEAD -- artifacts` is empty, so no path under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/` is committed. All committed evidence is under `<FEATURE>/evidence/{baseline,remediation-baseline,qa-gates,regression-testing,other}/`. The script `validate_evidence_locations.py` is not present in this checkout (`scripts/dev_tools/validate_evidence_locations.py` does not exist), so the check was performed by diff inspection. Verdict: PASS.

Committed coverage evidence uses the CLAUDE.md projection forms (package-level JaCoCo projection and one-line summary for C#; Markdown summaries for PowerShell); no raw Cobertura or TRX document is committed. Verdict: PASS.

## 8. Gaps and Exceptions

- B-1 from review 2026-10-09T14-55: closed (section 7).
- AC7 is pending CI by design and is verified after merge by the item's orchestrator run.
- O-1 (non-blocking): the bundled PoshQC coverage artifact still does not instrument `scripts/`; the direct Pester figures mirror CI `_pester.yml` and govern.
- O-2 (non-blocking): the PR-context summary classifies `BindingRedirectSync.psm1` as tooling and omits the manifest and csproj files from its overview list; language scope was derived from `git diff`.
- O-3 (non-blocking): the rehearsal worktree and local branch `rehearsal-985-throwaway` still exist outside the repository tree; no remote branch exists.
- The workflow comment drift noted in the prior review is promoted to issue #986 (`docs/features/potential/promoted/2026-10-09-dependabot-repair-workflow-comments-predate-redirect-sync.md`).

## 9. Summary of Changes

- `QuickFiler.Test/packages.config`, `UtilitiesCS.Test/packages.config`, `TaskTree.Test/packages.config`: declare the borrowed packages.
- `QuickFiler.Test/QuickFiler.Test.csproj`: remove the duplicate WebView2.Core `ItemGroup`.
- `scripts/dependencies/BindingRedirectSync.psm1` (new): stale-only redirect sync, solution wrapper, report formatter; R1 adds `Get-RedirectDirection`, the `Direction` record field and report suffix, and the case-insensitive handled-name set.
- `scripts/dependencies/Repair-PackageManifestConsistency.ps1`: import, override recording, unconditional sync call, `RedirectSync` result field, body block; R1 de-duplicates `WrittenPath`.
- Tests: `BindingRedirectSync.Tests.ps1` (24), `Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` (9), one new `It` in `RepositoryTreeConsistency.Tests.ps1`.
- Feature documents, evidence, review artifacts, the issue #986 promoted record and agent-memory notes (documentation only).

## 10. Compliance Verdict

**PASS.** No blocking finding remains. All toolchain, coverage and pre-merge acceptance gates pass; AC7 remains pending CI after merge.

## Appendix A: Test Inventory

| Test file | Tests | Status | Scope |
|---|---|---|---|
| `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` | 24 | new | module functions |
| `tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` | 9 | new | composition root wiring |
| `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` | 5 (1 new) | modified | repository invariant gate |
| `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | 31 | unchanged, green | backward compatibility |

## Appendix B: Toolchain Commands Reference

| Stage | Command | Run by |
|---|---|---|
| PowerShell format | `mcp__drm-copilot__run_poshqc_format` (scan folders `scripts/dependencies`, `tests/scripts/dependencies`) | executor |
| PowerShell lint | `mcp__drm-copilot__run_poshqc_analyze` (same folders) | executor |
| PowerShell test | `mcp__drm-copilot__run_poshqc_test` (`tests/scripts/dependencies`) | executor |
| PowerShell coverage (CI mirror) | Pester 5 direct run over `tests/scripts/{dependencies,hygiene,vscode}` with coverage paths `scripts/{dependencies,hygiene,vscode}` | executor and reviewer |
| C# format | `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` | executor (cycle 0) |
| C# analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | executor (cycle 0) |
| C# type check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | executor (cycle 0) |
| C# test | `Invoke-MSTestWithCoverage.ps1` (Koverage route) | executor (cycle 0) |
| Diff scope | `git diff --shortstat 9911fe138...HEAD`; `git show --stat 07d3a8a98`; `git diff --name-only 9911fe138...HEAD -- .github scripts/benchmarks .github/actions artifacts` | reviewer |
| Identity sweep | `pwsh -NoProfile -Command` over `git diff --unified=0 9911fe138...HEAD` added lines | reviewer |
| Coverage parsing | PowerShell `[xml]` over the reviewer JaCoCo output and `artifacts/pester/powershell-coverage.xml` | reviewer |
