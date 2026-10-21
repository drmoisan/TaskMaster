# Policy Audit: Dependabot repair, borrowed packages and transitive redirects (Issue #985)

- Branch: `bug/dependabot-repair-borrowed-packages-and-transitive-redirects-985`
- Head: `de397b992868882530a447c662d21b3fbff123eb`
- Base: `origin/main` at merge base `9911fe138952e2b93476850582847c2831e1cbbd` (committed 2026-10-07T08:22:27-04:00)
- Review timestamp: 2026-10-09T14-55
- Work mode: `full-bug` (AC source: `spec.md`)
- Reviewer: feature-review agent

## Template Provenance

The structure of this artifact follows the canonical major headings listed in `.claude/skills/policy-audit-template-usage/SKILL.md` step 5 (Executive Summary, sections 1 to 10, Appendix A and Appendix B); the template-asset selector step was not run from this agent session. The artifact is validated with `validate_orchestration_artifacts` (artifact type `policy-audit`).

## Rejected Scope Narrowing

No scope narrowing was detected in the caller prompt. The caller supplied the base branch, merge base, head and feature folder; these were re-derived (`git merge-base`, `git rev-parse HEAD`) and match. The audit scope is the full branch diff `9911fe138...de397b992` (70 files).

## Executive Summary

The branch declares four borrowed NuGet packages in three test-project manifests, removes a duplicate `Microsoft.Web.WebView2.Core` reference from `QuickFiler.Test.csproj`, adds a solution-wide binding-redirect synchronisation module (`scripts/dependencies/BindingRedirectSync.psm1`), wires it unconditionally into `Repair-PackageManifestConsistency.ps1`, and adds a repository Pester gate over orphaned HintPaths. No production C# source, no `.cs` file and no file under `.github/workflows/**` changed.

Overall verdict: **FAIL (one blocking finding, autonomous)**.

- Toolchain: PASS. PowerShell format, analyze and test pass (executor evidence plus an in-session re-run of the dependencies Pester suite, 178/178). C# format, analyzer rebuild, nullable rebuild and MSTest with coverage pass (executor evidence, 7427/7427).
- Coverage: PASS for both languages with changed files (PowerShell and C#; see section 1.2.1).
- Blocking finding B-1: the committed plan `plan.2026-10-09T13-06.md` line 26 embeds the operator account name inside the encoded session-scratchpad directory name, which the repository host-identifier rule prohibits in any committed file. One-line autonomous fix.
- AC status: 6 of 7 PASS; AC7 is pending CI by design (post-merge, `@dependabot recreate` on PR #984) and is not counted as blocking.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Every `It` in `BindingRedirectSync.Tests.ps1` builds its own store; `RedirectSync.Tests.ps1` uses Context-scoped `BeforeAll` fixtures. |
| Isolation | PASS | One behaviour per `It`; module functions tested separately from the composition root. |
| Fast execution | PASS | Dependencies suite of 178 tests completed in one in-session run. |
| Determinism | PASS | Inputs are literal strings and hashtables; no clock, RNG or network. |
| Readability | PASS | Arrange/Act/Assert comments and descriptive `It` names throughout. |
| No temporary files | PASS | `evidence/qa-gates/ps-temp-file-audit.2026-10-09T14-26.md`: pattern count 0 in all three test files, positive control 1. Reviewer read of the three files confirms no file write. |

### 1.2 Coverage requirements

Coverage thresholds applied: line 85% and branch 75% (branch-capable languages only) per `.claude/rules/quality-tiers.md`; new code at least 90% per CLAUDE.md UT2. Pester measures no branch coverage, so no PowerShell branch threshold is evaluated.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| PowerShell | 2 production (1 new, 1 modified), 3 test | Pester 5.6.1 | 438 passed, 0 failed | 94.64% | 94.98% | 100% |
| C# | 4 build-configuration files (1 csproj, 3 packages.config); 0 .cs files | MSTest | 7427 passed, 0 failed | 85.41% | 85.41% | N/A (no executable source changed) |
| TypeScript | 0 | none | none | N/A | N/A | N/A |
| Python | 0 | none | none | N/A | N/A | N/A |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/mstest-coverage-baseline.2026-10-09T14-11.jacoco.xml` with summary `evidence/baseline/mstest-coverage-baseline.2026-10-09T14-11.summary.txt`
- C# post-change coverage artifact: `evidence/qa-gates/mstest-coverage-final.2026-10-09T14-31.jacoco.xml` with summary `evidence/qa-gates/mstest-coverage-final.2026-10-09T14-31.summary.txt`; Cobertura root `coverage/coverage.cobertura.xml` read by the reviewer (line-rate 0.854134, branch-rate 0.798288)
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `coverage/985-pester-baseline.xml` (direct Pester JaCoCo, 1765/1865 lines)
- PowerShell post-change coverage artifact: `coverage/985-pester-final.xml` (direct Pester JaCoCo, 1891/1991 lines) and a reviewer in-session re-run over `scripts/dependencies`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- PowerShell: Baseline: 94.64% lines (1765/1865) -> Post-change: 94.98% lines (1891/1991). Change: +0.34% lines (+126 covered, +126 measured). New/changed-code coverage: 100%. Disposition: PASS. Evidence: coverage/985-pester-baseline.xml; coverage/985-pester-final.xml; evidence/qa-gates/coverage-comparison-powershell.2026-10-09T14-26.md.
- C#: Baseline: 85.41% lines (56496/66143) / 79.83% branches (13709/17173) -> Post-change: 85.41% lines (56495/66143) / 79.83% branches (13709/17173). Change: 0.00% lines, 0.00% branches (one covered line fewer, run-to-run variance; no .cs file changed). Disposition: PASS. Evidence: evidence/qa-gates/coverage-comparison-csharp.2026-10-09T14-31.md; evidence/qa-gates/mstest-coverage-final.2026-10-09T14-31.summary.txt.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Verdicts

PowerShell coverage verdict: PASS (direct Pester 94.98% over scripts/dependencies, scripts/hygiene and scripts/vscode; new module BindingRedirectSync.psm1 100.00% lines 114/114; modified Repair-PackageManifestConsistency.ps1 93.83% to 94.14% lines with every changed line hit).

PowerShell bundled PoshQC coverage artifact `artifacts/pester/powershell-coverage.xml`: FAIL (0.00% lines, 0/10477; its 15 packages are all under `.claude/` and `.codex/`, so it never instruments `scripts/`; pre-existing tooling defect, recorded as observation O-1, non-blocking).

Reviewer in-session PowerShell coverage re-run: PASS (178/178 tests, scripts/dependencies 98.33% lines 944/960, BindingRedirectSync.psm1 100.00% 114/114, Repair-PackageManifestConsistency.ps1 94.14% 225/239).

C# coverage verdict: PASS (line 85.41% at or above 85, branch 79.83% at or above 75; no C# executable line changed, so there is no changed-line regression).

Changed-line check for the modified PowerShell script: the uncovered lines in `Repair-PackageManifestConsistency.ps1` at post-change are 56, 114, 126-130, 137, 139-143 and 327; none is a changed line. Changed lines 73, 385, 421, 449, 451, 463-465 and 487-492 all carry hit counts of 1 or more in `coverage/985-pester-final.xml`.

### 1.3 Scenario completeness

PASS. Positive (stale rewrite, transitive rewrite), negative (unknown name, unparsable version, non-configuration text), edge (empty text, block without redirect, single-version `oldVersion`, numeric versus string ordering, ambiguous own reference), error (parser throw) and state (idempotent second pass, `-WhatIf`) scenarios are each pinned by a named `It`.

## 2. General Code Change Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| Design: simplicity and separation of concerns | PASS | Pure text function `Invoke-BindingRedirectSync`; I/O confined to delegates in `Invoke-SolutionBindingRedirectSync`. |
| Reuse | PASS | Reuses `ConvertFrom-AppConfigText`, `Invoke-BindingRedirectReconciliation`, `ConvertTo-ReferenceVersionMap`, `Get-PackageManifestPath`. |
| Error handling | PASS | Parser exception propagates; unverifiable and unresolvable names reported in the result, not swallowed. |
| Logging | PASS | One `Write-Information` summary line, matching the repair script's existing precedent. |
| File size (500 lines) | PASS | 306, 493, 358, 187, 178 lines (`evidence/qa-gates/ps-line-counts.2026-10-09T14-26.md`; reviewer recount of the module 306). |
| Public contracts documented | PASS | Comment-based help on all three exported functions and the module header. |
| Dependencies | PASS | No new package or module dependency beyond in-repo modules. |
| Host-identifier hygiene in committed files | FAIL | B-1: `plan.2026-10-09T13-06.md` line 26 contains the operator account name in the encoded scratchpad directory name. |
| Bugfix workflow (failing test first) | PASS | `evidence/regression-testing/fail-before-*.md` (exit 1, right reason) precede `pass-after-*.md`. |

## 3. Language-Specific Code Change Policy Compliance

### 3.1 PowerShell (`.claude/rules/powershell.md`)

| Rule | Verdict | Evidence |
|---|---|---|
| Advanced functions with `CmdletBinding` and named parameters | PASS | All five functions in the module. |
| `SupportsShouldProcess` on state-changing function | PASS | `Invoke-SolutionBindingRedirectSync`; composition root passes `-WhatIf:$WhatIfPreference` explicitly. |
| Approved verbs | PASS | `Invoke`, `Format`, `Get`, `Test`; PoshQC analyze clean. |
| No script-scoped mutable state | PASS | Script-scope values are constant separators and an exclusion list. |
| Injectable seams (delegate pattern) | PASS | Lister, reader and writer delegates mirror `Invoke-ManifestNormalization`. |
| PowerShell 7 compatibility | PASS | PoshQC analyze clean (`evidence/qa-gates/ps-analyze.2026-10-09T14-26.md`). |

### 3.2 C# (`CLAUDE.md` C# Code Change Policy)

| Rule | Verdict | Evidence |
|---|---|---|
| Build-configuration edits limited to intent | PASS | `QuickFiler.Test.csproj` diff removes exactly one 5-line `ItemGroup`; one `Microsoft.Web.WebView2.Core` reference remains (reviewer Grep count 1). |
| Manifest entries at sibling versions | PASS | WebView2 1.0.4191.47 and ObjectListView.Official 2.9.1 match `QuickFiler/`, `UtilitiesCS/` and `TaskTree/` manifests and the existing HintPath folders. |
| CSharpier | PASS | `evidence/qa-gates/cs-format.2026-10-09T14-30.md` (format no rewrite, check exit 0). |
| Analyzer rebuild | PASS | `evidence/qa-gates/cs-analyzers.2026-10-09T14-30.md` (0 warnings, 0 errors, 18 assemblies). |
| Nullable rebuild | PASS | `evidence/qa-gates/cs-nullable.2026-10-09T14-30.md` (0 warnings, 0 errors). |

## 4. Language-Specific Unit Test Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| Pester 5 with Describe/Context/It | PASS | Three test files. |
| Test location mirrors source | PASS | `tests/scripts/dependencies/` mirrors `scripts/dependencies/`. |
| Mock signature parity | PASS | No `Mock` used; real code paths over in-memory delegates. |
| No executable mocking or live executables | PASS | None invoked. |
| MSTest/Moq/FluentAssertions (C#) | PASS | No C# test changed. |

## 5. Test Coverage Detail

| File | Status | Baseline line | Post-change line | Gate |
|---|---|---|---|---|
| `scripts/dependencies/BindingRedirectSync.psm1` | new | absent | 100.00% (114/114) | at least 90% new code: PASS |
| `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | modified | 93.83% (213/227) | 94.14% (225/239) | at least 85% and no changed-line regression: PASS |
| `QuickFiler.Test/QuickFiler.Test.csproj` and three `packages.config` | modified | no executable lines | no executable lines | none |

## 6. Test Execution Metrics

| Suite | Result | Source |
|---|---|---|
| Pester, dependencies + hygiene + vscode (executor) | 438 passed, 0 failed | `evidence/qa-gates/ps-coverage.2026-10-09T14-26.md` |
| Pester via PoshQC MCP, dependencies (executor) | 178 tests, 0 failures | `evidence/qa-gates/ps-test-mcp.2026-10-09T14-26.md` |
| Pester, dependencies (reviewer in-session) | 178 passed, 0 failed, 0 skipped | reviewer run 2026-10-09T14-52 |
| MSTest, full solution (executor) | 7427 passed, 0 failed | `evidence/qa-gates/cs-test.2026-10-09T14-31.md` |
| Integration rehearsal on PR #984 branch | analyzer and nullable rebuild 0 errors; BindingRedirectVerification 16/16 | `evidence/other/integration-rehearsal.2026-10-09T14-35.md` |

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
|---|---|---|---|
| Confidentiality masking scan | `git diff 9911fe138...HEAD` added lines, case-insensitive sweep for drive roots, the account name, the host name and the user e-mail | 1 hit: account name in `plan.2026-10-09T13-06.md` line 26; host name 0; e-mail 0 | FAIL (B-1) |
| Suppression scan (added lines) | Grep added lines for `SuppressMessage` and `#pragma` | 0 | PASS |
| Workflow change scan | `git diff --name-only 9911fe138...HEAD -- .github scripts/benchmarks .github/actions` | empty; rule `modified-workflow-needs-green-run` not triggered | PASS |

## Evidence Location Compliance

`git diff --name-only 9911fe138...HEAD` lists no path under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`. All committed evidence is under `<FEATURE>/evidence/{baseline,qa-gates,regression-testing,other}/`. The script `validate_evidence_locations.py` is not present in this checkout (no file found by name search), so the check was performed by diff inspection. Verdict: PASS.

Committed coverage evidence uses the CLAUDE.md projection forms (package-level JaCoCo projection and one-line summary); no raw Cobertura or TRX document is committed. Verdict: PASS.

## 8. Gaps and Exceptions

- B-1 (blocking, autonomous): account name in the committed plan; see the remediation inputs.
- AC7 is pending CI by design and is verified after merge by the item's orchestrator run.
- O-1 (non-blocking): the bundled PoshQC coverage artifact instruments only `.claude/` and `.codex/`; the direct Pester projection mirrors CI `_pester.yml` and is the governing figure.
- O-2 (non-blocking): the PR-context summary classifies `BindingRedirectSync.psm1` as tooling and omits the four manifest and csproj files from its overview list; language scope was derived from `git diff` instead.
- O-3 (non-blocking): the rehearsal worktree and local branch `rehearsal-985-throwaway` remain outside the repository (cleanup refused by a hook, P7-T17); no remote branch exists.

## 9. Summary of Changes

- `QuickFiler.Test/packages.config`, `UtilitiesCS.Test/packages.config`, `TaskTree.Test/packages.config`: declare the borrowed packages.
- `QuickFiler.Test/QuickFiler.Test.csproj`: remove the duplicate WebView2.Core `ItemGroup`.
- `scripts/dependencies/BindingRedirectSync.psm1` (new): stale-only redirect sync, solution wrapper, report formatter.
- `scripts/dependencies/Repair-PackageManifestConsistency.ps1`: import, override recording, unconditional sync call, `RedirectSync` result field, body block.
- Tests: `BindingRedirectSync.Tests.ps1` (18), `Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` (6), one new `It` in `RepositoryTreeConsistency.Tests.ps1`.
- Feature documents, evidence, and agent-memory index consolidation under `.claude/agent-memory/` (documentation only).

## 10. Compliance Verdict

**FAIL** pending remediation of B-1 (one autonomous blocking finding). All toolchain, coverage and acceptance gates that can be evaluated before merge pass. After B-1 is fixed, the expected verdict is PASS with AC7 pending CI.

## Appendix A: Test Inventory

| Test file | Tests | Status | Scope |
|---|---|---|---|
| `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` | 18 | new | module functions |
| `tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` | 6 | new | composition root wiring |
| `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` | 5 (1 new) | modified | repository invariant gate |
| `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` | 31 | unchanged, green | backward compatibility |

## Appendix B: Toolchain Commands Reference

| Stage | Command | Run by |
|---|---|---|
| PowerShell format | `mcp__drm-copilot__run_poshqc_format` (scan folders `scripts/dependencies`, `tests/scripts/dependencies`) | executor |
| PowerShell lint | `mcp__drm-copilot__run_poshqc_analyze` (same folders) | executor |
| PowerShell test | `mcp__drm-copilot__run_poshqc_test` (`tests/scripts/dependencies`) | executor |
| PowerShell coverage (CI mirror) | Pester 5 direct run over `tests/scripts/{dependencies,hygiene,vscode}` with coverage paths `scripts/{dependencies,hygiene,vscode}` | executor |
| PowerShell coverage (review) | `pwsh -NoProfile -Command` with `New-PesterConfiguration`, `Run.Path = tests/scripts/dependencies`, `CodeCoverage.Path = scripts/dependencies`, output to the session scratchpad | reviewer |
| C# format | `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` | executor |
| C# analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | executor |
| C# type check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | executor |
| C# test | `Invoke-MSTestWithCoverage.ps1` (Koverage route) | executor |
| Diff scope | `git diff --stat 9911fe138...HEAD`; `git diff --name-only 9911fe138...HEAD -- .github` | reviewer |
| Coverage parsing | Python `xml.etree` over `coverage/985-pester-*.xml`, `artifacts/pester/powershell-coverage.xml`, `coverage/coverage.cobertura.xml` | reviewer |
