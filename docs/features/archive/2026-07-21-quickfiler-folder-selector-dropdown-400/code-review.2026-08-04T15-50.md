# Code Review: QuickFiler folder-selector dropdown (Issue #400)

**Review Date:** 2026-08-04
**Reviewer:** feature reviewer
**Feature Folder:** `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400`
**Feature Folder Selection Rule:** The active folder's `issue.md` identifies Issue #400 and `Work Mode: full-bug`.
**Base Branch:** `origin/main`
**Head Branch:** `bug/quickfiler-folder-selector-dropdown-400`, current head `62c4eb1c2b99ae6e9fa7742a31d283ec4a8d7151`, including current in-scope worktree changes
**Review Type:** Post-remediation re-review

## Executive Summary

The full branch implements the native QuickFiler folder-selector popup, selection-session, score-preserving fallback, and associated C# test coverage required by Issue #400. This fresh post-remediation review additionally examines the final PowerShell coverage-wrapper seam. It replaces direct top-level execution with a guarded callable main function and wraps `vswhere` for deterministic Pester testing; its test file supplies mocked happy-path and explicit error-path coverage.

The audited merge-base diff is clean (`git diff --check 050f7cd52a3b13ec2786c9dafbe9f99620ebf9e8`). Current scoped analyzer output is clean and focused Pester passes 25/25. The published MCP PR-context collector was invoked against `origin/main`, but its on-disk artifacts still identify the older `83efd313` head. This review therefore uses current direct Git range and working-tree inspection as the primary current-state evidence, while retaining the stale bundle as historical secondary evidence.

**What changed:**

- The branch adds the selector implementation and acceptance tests documented in Issue #400 and `spec.md`.
- The final current changes are limited to `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and its Pester test file.
- The final remediation makes top-level coverage orchestration testable without modifying `coverage.config`, runsettings, exclusions, or thresholds.

**Top 3 risks:**

1. The repository-wide PowerShell aggregate coverage remains below policy due to unrelated inherited debt; the authorized exception is bounded and does not waive changed-wrapper coverage.
2. The canonical PR-context artifacts are stale despite a successful published-MCP collection response; future workflow automation should correct that collector behavior.
3. The coverage wrapper still depends on locally installed Visual Studio Test Platform and `dotnet-coverage` in production; unit tests correctly mock those external executables.

**PR readiness recommendation:** **Go** — no code blocker or major finding remains in the current reviewed scope, and all direct review checks pass.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `artifacts/pr_context.summary.txt` | Base/Head section | Published MCP collection returned success but left the on-disk bundle at head `83efd313`, not current head `62c4eb1`. | Correct the collector artifact-refresh behavior outside this feature's production scope. | Review must not treat stale context as current evidence. | `mcp__drm-copilot__collect_pr_context(base='origin/main')` response; direct `git rev-parse HEAD`. |
| Info | `runbooks/issue-400-repository-wide-powershell-coverage-exception.runbook.md` | Coverage exception | Pre-existing repository-wide PowerShell aggregate coverage debt remains. | Retain the bounded exception until repository-wide debt is separately remediated. | The exception leaves focused Pester, analyzer, and changed-wrapper coverage mandatory and passing. | `powershell-coverage-exception-verification.2026-08-04T11-15.md`. |

No Blockers or Major findings.

## Implementation Audit

### PowerShell implementation audit

#### What changed well

- `Invoke-MSTestWithCoverageMain` moves only top-level orchestration behind a callable seam, preserving the existing script CLI through the guarded final invocation.
- `Invoke-VsWhereExe` isolates the executable boundary and retains argument splatting rather than constructing a command string.
- The wrapper preserves explicit failures for absent search roots, `vswhere`, `vstest.console.exe`, and `dotnet-coverage`.

#### API and safety notes

- The public script entrypoint remains source-compatible; dot-sourcing does not execute it.
- Mandatory parameters on the external-executable seam are typed `string` and `string[]`.
- The changed files are 348 and 451 lines, below the policy limit.

#### Error handling and logging

- Strict mode and `$ErrorActionPreference = 'Stop'` remain enabled.
- Error messages are explicit and are exercised by the focused tests.

### C# implementation audit

The historical branch implementation and test inventory are covered by the prior branch evidence and all 19 acceptance criteria already checked in `spec.md`. No C# files changed in the final remediation; this review did not repeat mutating C# toolchain commands.

## Test Quality Audit

### Reviewed test and QA artifacts

- `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` — 25 deterministic Pester tests covering executable discovery, non-executing import, settings derivation, cleanup, and failure paths.
- `evidence/regression-testing/invoke-mstest-wrapper-focused-coverage.2026-08-04T11-15.md` — 25/25 pass, 90.00% command coverage, 89.69% line coverage.
- `evidence/qa-gates/powershell-invoke-mstest-wrapper-poshqc-format.2026-08-04T11-15.md` — scoped formatter exit 0.
- `evidence/qa-gates/powershell-invoke-mstest-wrapper-poshqc-test.2026-08-04T11-15.md` — scoped PoshQC Pester gate reports 40 tests with no failures.
- Independent review run — `Invoke-ScriptAnalyzer` found zero findings and focused Pester passed 25/25 in 2.24 seconds.

### Quality assessment prompts

- **Determinism:** external executables, filesystem writes, and coverage collection are mocked.
- **Isolation:** tests target individual helper, main-path, and error behaviors.
- **Speed:** the direct focused run completed in 2.24 seconds.
- **Diagnostics:** exact expected error messages and mock invocation counts localize failures.

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Direct inspection of the two changed paths; no credentials, tokens, or `.env` access. |
| No unsafe subprocess or command construction | PASS | `Invoke-VsWhereExe` splats a typed argument array; `Invoke-DotnetCoverageExe` retains argument-array invocation. |
| Input validation at boundaries | PASS | Mandatory seam parameters and explicit search-root/tool availability checks. |
| Error handling remains explicit | PASS | Strict mode and throw messages are preserved and tested. |
| Configuration / path handling is safe | PASS | Evidence records unchanged hashes for `coverage.config`, `TaskMaster.runsettings`, and the CLI runsettings file. |

## Research Log

No external research was required. Repository source, test, evidence, and direct command output were sufficient.

## Verdict

The final remediation is ready for normal PR flow. It resolves the review-critical testability and coverage evidence gap without expanding the production scope or altering protected coverage configuration. The stale PR-context bundle is an information-level automation defect and is not a defect in Issue #400's implementation; current Git evidence was used for this review.
