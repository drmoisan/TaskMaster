# Code Review: Koverage Coverage Allowlist `.Test` Exclusion (Issue #193)

**Review Date:** 2026-06-12
**Reviewer:** feature-reviewer (Claude)
**Feature Folder:** `docs/features/active/2026-06-12-coverage-metric-includes-test-assemblies-193/`
**Feature Folder Selection Rule:** Active folder for Issue #193 as specified by the review request.
**Base Branch:** `origin/main` (commit `7798ae1d` per request; branch HEAD `4a21a5b8` equals `origin/main`)
**Head Branch:** `feature/csharp-coverage-uplift` (working-tree changes; #193 change set is uncommitted)
**Review Type:** Initial review (minor-audit)

---

## Executive Summary

This change modifies one PowerShell helper function, `Get-KoverageProjectAllowlist`, so the Koverage coverage allowlist excludes any project whose resolved assembly name ends in `.Test`. Because `ConvertTo-KoverageCoberturaXml` removes any `<package>` whose name is not in the allowlist and then recomputes aggregate `lines-covered`/`lines-valid` from the surviving classes, dropping `.Test` names from the allowlist removes those packages from both the numerator and the denominator of the reported coverage rate. The scope is small: one production function (14 changed lines) and four added Pester regressions plus one adjusted existing test.

**What changed:**
- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`: in `Get-KoverageProjectAllowlist`, the name is now resolved once into `$resolvedName` from `<AssemblyName>` (when present) or the project-file base name (fallback), and any name ending `.Test` (case-insensitive, `OrdinalIgnoreCase`) is skipped via `continue`. The previous code added the name in two separate branches; it now adds once after the exclusion check.
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`: added a new `Get-KoverageProjectAllowlist` Describe (exclusion, retention, base-name-fallback) and a strip/recompute test in `ConvertTo-KoverageCoberturaXml`; adjusted the path-normalization test to pass `-ProjectNames @('QuickFiler.Test')` so it does not depend on the production allowlist.

**Top 3 risks:**
1. The exclusion is name-suffix based (`.Test`). A future test project not following the `.Test` suffix convention would not be excluded. This matches the documented repo convention (all current test projects use `.Test`) and is called out in `issue.md`; it is a known, accepted limitation, not a defect.
2. The default-`RepoRoot` allowlist tests read real on-disk project files, so a future repo restructure of test-project names could change test outcomes. Acceptable given the deterministic, version-controlled inputs.
3. The pre-existing `PSUseSingularNouns` analyzer warning remains in the file (outside the changed function); left as-is to avoid an out-of-scope refactor.

**PR readiness recommendation:** **Go** — The implementation is correct for both name-resolution paths, the tests are deterministic and in-memory where it matters, and the unchanged `ConvertTo-KoverageCoberturaXml` rationale is sound.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | `Get-KoverageProjectAllowlist` lines 24-44 | `.Test` exclusion correctly applies to both the `<AssemblyName>` path and the base-name fallback because the suffix check is performed on the single resolved `$resolvedName` after both branches. | None; this is the correct structure. | Confirms AC1 covers both resolution paths. | Diff inspection lines 32-43; test `applies the .Test exclusion to the project-file base-name fallback` passes. |
| Info | `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | fallback test, mock block | `Get-ChildItem` and `Get-Content` are mocked so the fallback test touches no disk and is deterministic. | None. | Satisfies determinism and no-temp-file requirements. | Diff lines 154-168; runtime 121ms. |
| Minor | `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | EOF (line 171) | File ends with no trailing newline (`\ No newline at end of file`). | Optionally add a trailing newline for POSIX-tool friendliness. | Cosmetic; does not affect Pester or analyzer results. | `git diff` shows `\ No newline at end of file`. |
| Info | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | `Get-CoberturaLineConditionCoverageParts` line 133 | Pre-existing `PSUseSingularNouns` analyzer warning, outside the changed function. | Address in a separate cleanup change; do not widen this minor-audit. | Out of scope; not a #193 regression. | `Invoke-ScriptAnalyzer` on HEAD baseline reports the same finding at line 123. |

No Blocker or Major findings.

---

## Implementation Audit

### PowerShell implementation audit

#### What changed well

- The duplicated `$projectNames.Add(...)` calls in the prior two branches are consolidated into a single resolve-then-exclude-then-add flow. This removes the prior structural asymmetry (the old `<AssemblyName>`-success branch did `Add` + `continue`, while the fallback branch did a separate `Add`) and makes the `.Test` exclusion apply uniformly to both resolution paths. Without this consolidation, a `.Test` exclusion added to only one branch would have leaked test projects through the other.
- `OrdinalIgnoreCase` on both the `EndsWith` check and the `HashSet` comparer is the correct, culture-independent choice for assembly-name matching.
- The rationale comment explains why test projects must be excluded (numerator and denominator effect), which is the non-obvious part.

#### API and safety notes

- `Get-KoverageProjectAllowlist` retains `[CmdletBinding()]`, `[OutputType([System.Array])]`, and the validated `-RepoRoot` default. No public-surface change. The downstream consumer `ConvertTo-KoverageCoberturaXml` keeps its injectable `-ProjectNames` parameter (default `(Get-KoverageProjectAllowlist)`), preserving testability.
- The unchanged `ConvertTo-KoverageCoberturaXml` is the correct decision: its package-removal loop (`$pkg.name -notin $ProjectNames`) and the subsequent `Get-CoberturaCoverageSummary` recompute already implement the numerator/denominator stripping once the allowlist excludes `.Test`. Modifying the post-processor would have duplicated the exclusion logic in two places. Fixing the allowlist alone is the minimal, single-source-of-truth fix.

#### Error handling and logging

- No new error paths. The existing fail-explicit `<packages>`-missing `throw` in the post-processor is unchanged. No silent catch-alls introduced.

---

## Test Quality Audit

The change adds failing-first regressions and a fail-before artifact demonstrates they fail without the production fix.

### Reviewed test and QA artifacts

- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` — verifies allowlist `.Test` exclusion, production retention, base-name fallback exclusion, and post-processed strip with recomputed `lines-covered=1`/`lines-valid=2`. 6/6 pass (re-run during this review).
- `docs/features/active/2026-06-12-coverage-metric-includes-test-assemblies-193/evidence/regression-testing/fail-before.2026-06-13T01-56.md` — demonstrates the two key regressions fail when the production file is stashed (3 passed, 2 failed before the fix). Confirms failing-first.
- `docs/features/active/2026-06-12-coverage-metric-includes-test-assemblies-193/evidence/qa-gates/final-toolchain.2026-06-13T01-56.md` — records format-clean, 0 new analyzer findings, 6/6 Pester, file coverage 87.98%, changed function 100%.

### Quality assessment prompts

- **Determinism:** In-memory here-string inputs; the fallback test mocks the filesystem. No randomness/clock/network.
- **Isolation:** One behavior per `It`; mocks are scoped to the single `It` that needs them.
- **Speed:** 15.87s total. The real-project-scan tests dominate (~3.7-4.0s each); acceptable. Fallback test is 121ms.
- **Diagnostics:** `Should -Contain`/`-Not -Contain`/`-Be` produce actionable failure messages, demonstrated by the fail-before output.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | ✅ PASS | No credentials/tokens in the diff. |
| No unsafe subprocess or command construction | ✅ PASS | No `Invoke-Expression`; no executable invocation added. Regex/string operations only. |
| Input validation at boundaries | ✅ PASS | `-RepoRoot` validated by existing default; assembly-name regex bounded with `[^<]+?`. |
| Error handling remains explicit | ✅ PASS | No new catch-alls; existing `throw` preserved. |
| Configuration / path handling is safe | ✅ PASS | Path handling via `[System.IO.Path]::GetFileNameWithoutExtension`; bin/obj/packages still excluded. |

---

## Research Log

No external research required. The review is based on diff inspection, independent re-runs of `Invoke-Formatter`, `Invoke-ScriptAnalyzer`, and `Invoke-Pester`, and the feature-folder evidence artifacts.

---

## Verdict

The change is correct, minimal, and well-tested. Both name-resolution paths (`<AssemblyName>` and base-name fallback) route through a single `.Test` suffix check, so the exclusion is uniform; the fixtures are in-memory or filesystem-mocked and deterministic; and leaving `ConvertTo-KoverageCoberturaXml` unchanged is the right call because the allowlist is the single source of truth for package retention. The only findings are Info/Minor (a missing trailing newline and a pre-existing out-of-scope analyzer warning). The change is ready for normal PR flow with no required follow-up.
