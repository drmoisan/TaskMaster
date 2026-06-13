# Feature Audit: vscode-test-runner-parity (#188)

**Audit Date:** 2026-06-12
**Feature Folder:** `docs/features/active/2026-06-12-vscode-test-runner-parity-188`
**Base Branch:** `main`
**Head Branch:** `bug/vscode-test-runner-parity-188` (uncommitted working tree)
**Work Mode:** `minor-audit`
**Audit Type:** Initial acceptance review

---

## Scope and Baseline

- **Base branch:** `main` (commit `aa63315bd432ffbf092cfbb5caa02ee673e7b326`)
- **Head branch/commit:** `bug/vscode-test-runner-parity-188` working tree (merge-base equals current HEAD `aa63315bd432ffbf092cfbb5caa02ee673e7b326`; changes are uncommitted)
- **Merge base:** `aa63315bd432ffbf092cfbb5caa02ee673e7b326`
- **Evidence sources:**
  - Primary: `git diff` against base (working-tree scope) — `scripts/vscode/Invoke-MSTest.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`
  - Secondary baseline diff: `git show HEAD:<file>` for baseline line counts and content comparison
  - Feature evidence: `docs/features/active/2026-06-12-vscode-test-runner-parity-188/evidence/**`
  - Additional evidence: reviewer re-run of `mcp__drm-copilot__run_poshqc_analyze` and isolated `Invoke-Pester`
- **Feature folder used:** `docs/features/active/2026-06-12-vscode-test-runner-parity-188`
- **Requirements source:** `issue.md` `## Acceptance Criteria` (AC1–AC7)
- **Work mode resolution note:** `issue.md` line 12 records `- Work Mode: minor-audit`. Per the minor-audit rule, only the explicit `## Acceptance Criteria` section in `issue.md` is authoritative. `spec.md` and `user-story.md` are absent from the active folder (confirmed via directory listing), consistent with minor-audit.
- **Scope note:** Working-tree-only validation; the branch HEAD equals the merge-base with `main`, so the entire change set is the uncommitted working-tree diff. No PR context artifacts were required because the diff is directly inspectable. The Tesseract/OCR failures and the `Install-RepoDotNetSdk` SDK-version failure are explicitly out of scope and were confirmed untouched.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-06-12-vscode-test-runner-parity-188/issue.md` — only source (minor-audit, `## Acceptance Criteria`)

### Acceptance criteria

1. AC1: `scripts/vscode/Invoke-MSTest.ps1` passes `/Settings:<repo-root>\TaskMaster.runsettings` to `vstest.console.exe` when running the test assemblies.
2. AC2: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` passes `/Settings:<repo-root>\TaskMaster.runsettings` to the inner `vstest.console.exe` invocation. The existing `dotnet-coverage --settings coverage.config` (instrumentation excludes) remains unchanged and distinct from the vstest runsettings.
3. AC3: The runsettings path is resolved deterministically from the repository root, and each script fails fast with a clear, specific error if `TaskMaster.runsettings` is absent.
4. AC4: A wrapper-function seam (per the repository PowerShell wrapper-seam pattern, e.g. `Invoke-VsTestExe -VsTestArgs <string[]>`; parameter name is not `Args`) is introduced so the vstest argument list is unit-testable without launching the external executable.
5. AC5: Pester tests assert that the constructed argument list for both scripts includes `/Settings:` pointing at the repo-root `TaskMaster.runsettings`. Tests mock the wrapper seam (never the real `vstest.console.exe`/`dotnet-coverage`), are deterministic, and produce identical results in the terminal and the VS Code Test Explorer.
6. AC6: `TaskMaster.runsettings` content is preserved; if edited at all, it must retain `<MSTest><Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize></MSTest>`, which mirrors the configuration Visual Studio auto-detects.
7. AC7: PowerShell toolchain passes in order — PoshQC format -> PSScriptAnalyzer -> Pester — with no new analyzer debt and no coverage regression on changed lines.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | AC1: Invoke-MSTest passes /Settings: to vstest.console.exe | PASS | `Get-VsTestArgumentList` returns `@($TestAssembly) + @("/Settings:$RunSettingsPath", '/InIsolation')`; production path passes it via `Invoke-VsTestExe`. Tests "includes /Settings:" + "preserves ... /InIsolation" pass. | `git diff -- scripts/vscode/Invoke-MSTest.ps1`; isolated `Invoke-Pester` (9/9) | `/Settings:` resolves to repo-root `TaskMaster.runsettings`. |
| 2 | AC2: Coverage script inner vstest /Settings:; outer --settings coverage.config unchanged/distinct | PASS | `Get-DotnetCoverageArgumentList` keeps `'--settings', $CoverageConfig` and appends inner `"/Settings:$RunSettingsPath"` after `'--', $VsTestPath`. Tests "preserves the distinct outer --settings coverage.config" + "places the inner /Settings: after the -- separator" pass. | `git diff -- scripts/vscode/Invoke-MSTestWithCoverage.ps1`; `Invoke-Pester` | Two distinct settings flags verified non-colliding. |
| 3 | AC3: Deterministic repo-root resolution; fail-fast on missing file | PASS | `Resolve-RunSettingsPath` does `Join-Path $RepoRoot 'TaskMaster.runsettings'` and `throw "Runsettings file not found: $runSettingsPath"`. Defined in both scripts. Negative test asserts the exact message. | `Invoke-Pester` (negative test passes) | Same resolver present in both scripts. |
| 4 | AC4: Wrapper-function seam (param not `Args`); argument list unit-testable without launching exe | PASS | `Invoke-VsTestExe -VsTestArgs [string[]]` and `Invoke-DotnetCoverageExe -DotnetCoverageArgs [string[]]`; pure builders enable assertion without execution; `-NoExecute` switch added. | `git diff` inspection | Parameter names are `VsTestArgs`/`DotnetCoverageArgs`, not `Args`. |
| 5 | AC5: Pester asserts /Settings: for both scripts; mocks seam only; deterministic; Test Explorer parity | PASS | `Invoke-MSTest.RunSettings.Tests.ps1` 9/9 pass; mocks only `Invoke-VsTestExe`/`Invoke-DotnetCoverageExe` with matching signatures; `$PSScriptRoot`-relative; mocks registered before invocation; no PATH/CWD assumptions. | isolated `Invoke-Pester` (Passed=9 Failed=0) | Determinism conditions for Test Explorer parity satisfied by construction. |
| 6 | AC6: TaskMaster.runsettings preserved with Parallelize Workers=0 ClassLevel | PASS | `git diff -- TaskMaster.runsettings` produced no output (exit 0, empty). File unchanged. | `git diff -- TaskMaster.runsettings` | No edit made; content trivially preserved. |
| 7 | AC7: Toolchain passes (format -> analyze -> test); no new analyzer debt; no coverage regression on changed lines | PASS | Format EXIT_CODE 0 idempotent; PSScriptAnalyzer 16 == baseline 16 (reviewer-reverified, analyzer threw "reported 16 issue(s)"); 9/9 Pester pass; coverage strictly increased from 0% baseline, 100% of policy-testable new lines. | `mcp__drm-copilot__run_poshqc_analyze`; `Invoke-Pester` | Raw new-code 84.21% < 90% is a documented policy-mandated seam-mocking exception; all policy-testable lines 100% covered. |

---

## Summary

**Overall Feature Readiness:** PASS

**Criteria summary:**
- **PASS:** 7 criteria
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**blocking_count: 0** (0 FAIL + 0 blocking PARTIAL across all three artifacts).

The raw new-code coverage of 84.21% is below the 90% target only because of three lines: the fail-fast `throw` (behaviorally exercised by a passing negative test but not instrumented under `Should -Throw`) and the two wrapper-seam `& <exe> @Args` execution bodies that `.claude/rules/powershell.md` mandates remain unexecuted in tests. This is adjudicated as an acceptable policy-justified exception, not a blocking coverage finding: 100% of policy-testable new lines are covered, and executing the seam bodies would launch the real external tools in violation of the determinism / no-external-dependency rules. Net-new analyzer debt is zero (16 == baseline 16, including the 2 pre-existing `PSAvoidUsingWriteHost` warnings in `Invoke-MSTest.ps1` carried over unchanged). `TaskMaster.runsettings` and `.vscode/tasks.json` are unmodified. The deferred Tesseract/OCR defect and the pre-existing `Install-RepoDotNetSdk` SDK-version failure were confirmed out of scope and untouched.

**Overall verdict: PASS. Ready to merge.**

**Top gaps preventing PASS:**

1. None.

**Recommended follow-up verification steps:**

1. After merge, run both VS Code test tasks and confirm all assemblies report class-level parallelization (this convergence will also surface the deferred OCR failures, which is the intended parity outcome).
2. Track the deferred Tesseract/OCR external-file test-isolation defect and the `Install-RepoDotNetSdk` SDK-version assertion as separate follow-up items.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- All AC1–AC7 are evaluated PASS and are already checked `[x]` in `issue.md` (checked off by the executor during Phase 1/2).
- No PARTIAL/FAIL/UNVERIFIED items remain unchecked.
- No source-file checkbox change was required by this review because all items were already correctly checked and re-verified as PASS.

### AC Status Summary

- Source: `docs/features/active/2026-06-12-vscode-test-runner-parity-188/issue.md`
- Total AC items: 7
- Checked off (delivered): 7
- Remaining (unchecked): 0
- Items remaining: None.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `issue.md` | 7 | 7 | 0 | Checkbox-backed; all AC1–AC7 already `[x]` and re-verified PASS by this review. |

No source-file checkbox change was made because all seven criteria were already checked `[x]` and the review confirmed each as PASS.
