# Code Review: VS coverage F#/Deedle exclusion + VS Code runner parity (#189 / #188)

**Review Date:** 2026-06-12
**Reviewer:** feature-reviewer agent
**Feature Folder:** `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189`
**Feature Folder Selection Rule:** #189 is the consolidating folder for the coupled Option A change; #188 ships with it and its artifacts live in `docs/features/active/2026-06-12-vscode-test-runner-parity-188`.
**Base Branch:** `main`
**Head Branch:** staged working tree (uncommitted)
**Review Type:** Initial review (combined minor-audit, two coupled issues)

---

## Executive Summary

This review covers the coupled, uncommitted Option A change set for issues #189 (Visual Studio coverage F#/Deedle exclusion) and #188 (VS Code test-runner parallelization parity). The two changes are interdependent: adding the Visual Studio Code Coverage exclusion block to the repo-root `TaskMaster.runsettings` force-activates coverage at the CLI, so the #188 runner scripts had to be repointed away from the root runsettings onto a new off-root parallelization-only CLI runsettings. The change set was reviewed against the actual working-tree files via direct read and `git diff`, not against claims.

**What changed:**
- `scripts/vscode/TaskMaster.cli.runsettings` (NEW, 9 lines): `<MSTest><Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize></MSTest>` only; no `<DataCollectors>`. Confirmed by read and `grep -c "DataCollector"` = 0.
- `TaskMaster.runsettings` (MODIFIED): purely additive `<DataCollectionRunSettings><DataCollectors><DataCollector friendlyName="Code Coverage">` block with the 7 `coverage.config` module excludes; `<MSTest><Parallelize>` preserved; no `enabled` attribute. Confirmed by `git diff` (additive hunk only) and `grep -i enabled` = none.
- `scripts/vscode/Invoke-MSTest.ps1` and `Invoke-MSTestWithCoverage.ps1` (MODIFIED): `Resolve-RunSettingsPath` resolves `scripts\vscode\TaskMaster.cli.runsettings` via `$PSScriptRoot`, throws a specific error when absent, and both arg-list builders emit `/Settings:<CLI-runsettings>`. The coverage script's inner vstest still omits `/collect`; the outer `dotnet-coverage --settings coverage.config` is preserved.
- `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` (MODIFIED): assertions repointed to the CLI runsettings; only the wrapper seams are mocked.

The implementation is small, cohesive, and follows the repository PowerShell wrapper-seam pattern. The central design — collector block only in the IDE-auto-detected root runsettings, parallelization-only file for the CLI — is correctly realized.

**Top 3 risks:**
1. The coverage-exclusion runtime effect (the actual suppression of the `VerificationException` under VS "Analyze Code Coverage") is not CLI-reproducible and depends on user VS confirmation (AC8). This is an inherent limitation of the platform, documented in the evidence, not a code defect.
2. Aggregate-file PowerShell coverage on the two scripts is 77.06% (below the 80% repo target), entirely from pre-existing untested top-level I/O bodies; no regression on changed lines.
3. Two pre-existing `PSAvoidUsingWriteHost` analyzer findings remain in `Invoke-MSTest.ps1`; not introduced by this change but still present in a touched file.

**PR readiness recommendation:** **Go** — the code is complete, policy-compliant, and design-correct; AC8 is a pending post-merge/pre-merge user VS check, not a code blocker.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `TaskMaster.runsettings` | DataCollectionRunSettings block (lines 9-29) | Coverage exclude block lives only in the root runsettings (VS auto-detect); no `enabled` attribute; additive-only diff | None — this is the intended Option A design | Keeping the collector solely in the IDE-detected file is what prevents CLI force-activation | `git diff TaskMaster.runsettings`; `grep -i enabled` = none |
| Info | `scripts/vscode/TaskMaster.cli.runsettings` | whole file | New off-root CLI runsettings carries parallelization only, no DataCollectors | None | Confirms the CLI never sees the coverage collector | Read; `grep -c "DataCollector"` = 0; XML well-formed |
| Minor | `scripts/vscode/Invoke-MSTest.ps1` | lines 119-120 | Two pre-existing `PSAvoidUsingWriteHost` findings remain in a touched file | Optional: migrate to `Write-Information`/`Write-Output` in a follow-up; not required here | Pre-existing debt, not introduced by this change; no net-new debt | `evidence/qa-gates/powershell-toolchain-final.2026-06-12T19-22.md` |
| Info | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | `Get-DotnetCoverageArgumentList` (lines 70-76) | Inner vstest arg list omits `/collect`; outer `--settings coverage.config` preserved and distinct | None | Prevents double collection with dotnet-coverage | `evidence/regression-testing/koverage-no-double-collect.2026-06-12T19-22.md`; read |
| Info | `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` | mocks (lines 68-72, 132-135) | Only wrapper seams mocked; mock signatures match production parameters exactly | None | Satisfies powershell.md mock-the-wrapper-only rule | Read |

No Blocker or Major findings.

---

## Implementation Audit

### PowerShell implementation audit

#### What changed well

- The CLI/IDE runsettings split is the correct resolution of the scope-change finding: rather than fighting the CLI's force-activation of a declared collector via an `enabled` attribute (which the evidence shows breaks the `/collect` path), the design places the collector only where Visual Studio auto-detects it and gives the CLI a collector-free file. This is simple and avoids per-attribute brittleness.
- `Resolve-RunSettingsPath` resolves deterministically from `$PSScriptRoot` (not from a mutable working directory), satisfying the powershell.md deterministic-resolution and Test-Explorer-parity requirements.
- The arg-list builders are pure functions with no I/O, which is why they are cleanly unit-testable; the external execution is isolated behind the `Invoke-VsTestExe` / `Invoke-DotnetCoverageExe` wrapper seams, with parameter names (`VsTestArgs`, `DotnetCoverageArgs`) deliberately avoiding the `Args` automatic-variable collision.

#### API and safety notes

- All new functions are advanced functions with mandatory typed parameters. `Set-StrictMode -Version Latest` and `$ErrorActionPreference = 'Stop'` are set. Fail-fast `throw` covers the missing-runsettings case and non-zero executable exit codes.
- Function names use approved verbs (`Resolve-`, `Get-`, `Invoke-`).

#### Error handling and logging

- Missing runsettings throws a specific, path-naming error; the test asserts the exact message. The coverage script also fails fast when `dotnet-coverage` or `vstest.console.exe` is unresolved. `Invoke-MSTest.ps1` uses `Write-Host` for status output (pre-existing pattern, flagged as Minor above); `Invoke-MSTestWithCoverage.ps1` uses `Write-Output`.

---

## Test Quality Audit

The Pester suite for both scripts is deterministic and mocks only the wrapper seams, never real executables. Tests assert both the presence and the ordering of `/Settings:` relative to the `--` separator and the vstest path, which is the meaningful correctness property for the dotnet-coverage proxy command. Coverage on the two scripts did not regress (77.06% -> 77.06%); the changed lines are covered.

### Reviewed test and QA artifacts

- `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` — 9 in-scope tests; verifies CLI-runsettings `/Settings:` for both scripts, the missing-file throw, and wrapper-seam pass-through. All pass.
- `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/evidence/qa-gates/powershell-toolchain-final.2026-06-12T19-22.md` — format/analyze/Pester results; no net-new debt; no coverage regression.
- `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/evidence/regression-testing/cli-no-collect-run.2026-06-12T19-22.md` — 42 Deedle tests pass under the CLI runsettings with no `.coverage` attachment (AC5).
- `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/evidence/regression-testing/koverage-no-double-collect.2026-06-12T19-22.md` — inner vstest omits `/collect`; no double collection.
- `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/evidence/regression-testing/cli-parallelization-parity.2026-06-12T19-22.md` — CLI runsettings retains Workers=0/ClassLevel (AC6).
- `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/evidence/regression-testing/exclusion-effect-not-cli-verifiable.2026-06-12T19-22.md` — documents why the exclusion effect routes to AC8.

### Quality assessment prompts

- **Determinism:** Mocks the wrapper seams only; no network, clock, PATH, or working-directory dependence.
- **Isolation:** Each `It` targets one behavior.
- **Speed:** Pure-function/mock tests; sub-second.
- **Diagnostics:** `Should -Be`/`Should -Contain`/`Should -Throw -ExpectedMessage` give specific failure output.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | ✅ PASS | No credentials or tokens in any changed file. |
| No unsafe subprocess or command construction | ✅ PASS | Executables invoked via splatted typed arg arrays; no `Invoke-Expression`; paths resolved from `$PSScriptRoot`/`vswhere`. |
| Input validation at boundaries | ✅ PASS | Mandatory typed parameters; `Test-Path` guards with fail-fast throws. |
| Error handling remains explicit | ✅ PASS | `$ErrorActionPreference = 'Stop'`; specific throws; non-zero exit codes re-thrown. |
| Configuration / path handling is safe | ✅ PASS | Deterministic `Join-Path` resolution; off-root CLI runsettings does not interfere with VS auto-detection (only one `.runsettings` at repo root, verified). |

---

## Research Log

No external research required. All findings are grounded in direct inspection of the working-tree files, `git diff`, `git status`, and the on-disk evidence artifacts in the #189 feature folder.

---

## Verdict

The combined #189/#188 change set is ready for normal PR flow. The implementation correctly realizes the Option A design (coverage collector only in the IDE-auto-detected root runsettings; parallelization-only off-root CLI runsettings consumed by the scripts), preserves parallelization parity, prevents double coverage collection, and passes the PowerShell toolchain with no net-new debt and no coverage regression on changed lines. There are no Blocker or Major findings. The one Minor finding (two pre-existing `PSAvoidUsingWriteHost` in `Invoke-MSTest.ps1`) is pre-existing and optional to address. The only outstanding acceptance item, #189 AC8 (and #188's VS-confirmation item), is a user action in Visual Studio that cannot be reproduced at the CLI; it is correctly recorded as PENDING and does not block the code change.
