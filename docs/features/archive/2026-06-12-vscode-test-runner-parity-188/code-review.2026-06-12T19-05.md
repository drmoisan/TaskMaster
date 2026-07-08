# Code Review: vscode-test-runner-parity (#188)

**Review Date:** 2026-06-12
**Reviewer:** feature-review agent
**Feature Folder:** `docs/features/active/2026-06-12-vscode-test-runner-parity-188`
**Feature Folder Selection Rule:** Caller-specified active feature folder for issue #188.
**Base Branch:** `main` (merge-base `aa63315bd432ffbf092cfbb5caa02ee673e7b326`)
**Head Branch:** `bug/vscode-test-runner-parity-188` (uncommitted working tree; diff taken against base)
**Review Type:** Initial review

---

## Executive Summary

This change aligns the VS Code MSTest task runners with Visual Studio by applying the repo-root `TaskMaster.runsettings` via `/Settings:` to `vstest.console.exe`. The implementation follows the repository's preferred PowerShell wrapper-seam pattern: external executable invocations are extracted into `Invoke-VsTestExe` and `Invoke-DotnetCoverageExe` (single typed array parameters named `VsTestArgs`/`DotnetCoverageArgs`, not `Args`), and argument construction is extracted into pure functions (`Get-VsTestArgumentList`, `Get-DotnetCoverageArgumentList`) plus a deterministic path resolver (`Resolve-RunSettingsPath`). A `-NoExecute` switch mirrors the existing `Invoke-VSBuild.ps1` convention and enables dot-source-and-assert testing without launching executables.

The scope is small and configuration-focused: 2 production scripts modified, 1 test file added. The reviewed evidence includes Phase 0 baselines and Phase 2 QA gates under `evidence/`. The reviewer independently re-ran PSScriptAnalyzer (16 == baseline, 0 net-new) and the new Pester file (9/9 pass), and confirmed `TaskMaster.runsettings` and `.vscode/tasks.json` are unmodified.

**What changed:**
- `scripts/vscode/Invoke-MSTest.ps1`: added path resolver, pure argument builder, `Invoke-VsTestExe` seam, `-NoExecute`; now passes `/Settings:` + `/InIsolation`.
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1`: added path resolver, `Get-DotnetCoverageArgumentList`, `Invoke-DotnetCoverageExe` seam, `-NoExecute`; inner vstest segment now carries `/Settings:` while the distinct outer `--settings coverage.config` is preserved.
- `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`: 9 Pester tests covering both scripts.

**Top 3 risks:**
1. Two wrapper-seam execution bodies (`& <exe> @Args`) are uncovered by design; a regression inside the one-line splat would not be caught by unit tests (low risk — the bodies are trivial and the policy mandates this).
2. The `Get-VsTestArgumentList`/`Get-DotnetCoverageArgumentList` functions are duplicated `Resolve-RunSettingsPath` definitions across both scripts (intentional per the per-script self-contained convention; minor duplication, not extracted to a shared helper).
3. Parity convergence will surface the deferred OCR failures under VS Code (intended outcome, documented as out of scope).

**PR readiness recommendation:** **Go** — The change is small, policy-aligned, and verified; no Blocker or Major findings.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `scripts/vscode/Invoke-MSTest.ps1` | lines 116-117 | Two pre-existing `PSAvoidUsingWriteHost` warnings carried over unchanged (originally lines 49/50). | No action required for this change; address in a separate cleanup if desired. | Not new debt; analyzer count equals baseline. | `evidence/qa-gates/final-poshqc-analyze.md`; reviewer re-run reported 16 == baseline. |
| Info | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | `Resolve-RunSettingsPath` | `Resolve-RunSettingsPath` is defined identically in both scripts rather than shared. | Acceptable; each task script is intentionally self-contained. Optionally factor into a shared helper in a future change. | Minor duplication; does not violate the 500-line or DRY thresholds materially. | Diff inspection. |
| Info | `scripts/vscode/Invoke-MSTest.ps1` | line 71 / `Invoke-MSTestWithCoverage.ps1` line 90 | Wrapper-seam `& <exe> @Args` bodies are intentionally uncovered. | Keep as-is; required by mocking policy. | `.claude/rules/powershell.md` forbids executing real executables in tests. | `evidence/qa-gates/final-coverage-comparison.md`. |

No Blockers or Major findings.

---

## Implementation Audit

### PowerShell implementation audit

#### What changed well

- Correct application of the wrapper-seam pattern: single typed array parameter, parameter names avoid the `Args` automatic-variable collision, and the splat (`& $VsTestPath @VsTestArgs`) is the only thing inside the seam — keeping the untestable surface minimal.
- Clean separation of concerns: deterministic path resolution, pure argument construction, and execution are distinct functions. The pure builders are fully unit-tested.
- The distinct semantics of the outer `dotnet-coverage --settings coverage.config` (instrumentation excludes) versus the inner vstest `/Settings:<TaskMaster.runsettings>` are preserved and explicitly documented in a why-comment, and asserted by a dedicated test.
- Reuse of the established `-NoExecute` / dot-source-and-assert convention from `Invoke-VSBuild.ps1` keeps the codebase consistent.

#### API and safety notes

- New functions use mandatory typed parameters and approved verbs (`Resolve-`, `Get-`, `Invoke-`) with singular nouns (`...ArgumentList`), satisfying PSScriptAnalyzer.
- `Resolve-RunSettingsPath` fails fast with a specific message naming the missing path, satisfying the explicit-error policy.
- No global mutable state introduced; data flows through parameters.

#### Error handling and logging

- Fail-fast `throw` for the missing runsettings file is specific and actionable.
- `$ErrorActionPreference = 'Stop'` and the post-invocation `$LASTEXITCODE` check are retained, preserving the existing failure surfacing.
- The two pre-existing `Write-Host` lines remain; not in scope for this change.

---

## Test Quality Audit

The new Pester file is deterministic and isolates each behavior. It mocks only the wrapper seams with signatures matching production exactly, never the real `vstest.console.exe`/`dotnet-coverage`. The `BeforeAll` dot-sources both scripts under `-NoExecute`, tolerating top-level body errors (vswhere/dotnet-coverage absence) while still importing the functions under test — a sound pattern for environment-independent unit testing. Coverage of all policy-testable new lines is 100%; the only uncovered lines are the two seam bodies the policy forbids executing.

### Reviewed test and QA artifacts

- `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` — asserts `/Settings:` for both scripts, distinct `--settings coverage.config`, argument ordering, and the fail-fast throw. Reviewer re-ran in isolation: 9/9 pass, 1.61s.
- `evidence/qa-gates/final-poshqc-format.md` — format idempotent, EXIT_CODE 0.
- `evidence/qa-gates/final-poshqc-analyze.md` — 16 folder-wide == baseline 16; 0 net-new. Reviewer-reverified.
- `evidence/qa-gates/final-pester.md` — 9/9 new tests pass; directory-scoped exit 1 is the unrelated `Install-RepoDotNetSdk` SDK-version failure.
- `evidence/qa-gates/final-coverage-comparison.md` — 84.21% raw / 100% policy-testable new-code; no regression.

### Quality assessment prompts

- **Determinism:** `$PSScriptRoot`-relative paths; mocks registered before invocation; no PATH/CWD assumptions. Reviewer re-run reproduced identical results.
- **Isolation:** One behavior per `It`; grouped by function under test.
- **Speed:** 1.61s for 9 tests (observed).
- **Diagnostics:** `Should -Be`/`Should -Contain`/`Should -Throw -ExpectedMessage` give specific failure output.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | ✅ PASS | Diff inspection; no credentials or tokens introduced. |
| No unsafe subprocess or command construction | ✅ PASS | Executables invoked via splat of typed arrays; no `Invoke-Expression`; arguments are constructed from resolved paths, not interpolated user input. |
| Input validation at boundaries | ✅ PASS | `Resolve-RunSettingsPath` validates file existence and throws a specific error; mandatory typed parameters on all new functions. |
| Error handling remains explicit | ✅ PASS | Fail-fast throw + retained `$LASTEXITCODE` check. |
| Configuration / path handling is safe | ✅ PASS | Runsettings path resolved deterministically from `$repoRoot` via `Join-Path`; no relative/CWD-dependent resolution. |

---

## Research Log

No external research was required. The review is grounded in the branch diff, the feature-folder evidence artifacts, `.claude/rules/powershell.md`, and the reviewer's independent re-run of the analyzer and Pester tests.

---

## Verdict

The change is ready for normal PR flow. It is a small, well-structured configuration-parity fix that correctly applies the repository's PowerShell wrapper-seam and pure-function patterns, passes the full PowerShell toolchain (format/analyze/test) with zero net-new analyzer debt, and is covered to 100% of policy-testable new lines. The only sub-90% raw coverage figure is a documented, policy-mandated exception. `TaskMaster.runsettings` and `.vscode/tasks.json` are unmodified, and the deferred OCR and SDK-version failures were confirmed untouched. This verdict is consistent with the Findings Table (no Blocker/Major findings) and the Go readiness recommendation.
