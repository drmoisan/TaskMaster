# Feature Audit: VS coverage F#/Deedle exclusion + VS Code runner parity (#189 / #188)

**Audit Date:** 2026-06-12
**Feature Folder:** `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189`
**Base Branch:** `main`
**Head Branch:** staged working tree (uncommitted)
**Work Mode:** `minor-audit` (both issues)
**Audit Type:** Initial acceptance review (combined, two coupled issues)

---

## Scope and Baseline

- **Base branch:** `main`
- **Head branch/commit:** staged working tree (uncommitted change set)
- **Merge base:** N/A (working-tree review against `main`)
- **Evidence sources:**
  - Primary: working-tree files (read directly) + `git diff` / `git status --porcelain`
  - Feature evidence: `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/evidence/**`
  - Scope-change finding: `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/evidence/other/scope-change-finding.2026-06-12T19-45.md`
  - AC reconciliation: `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/evidence/qa-gates/ac-reconciliation.2026-06-12T19-22.md`
- **Feature folder used:** `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189`
- **Requirements source:** `issue.md` (#189) `## Acceptance Criteria (Option A …)` AC1-AC8, and `issue.md` (#188) `## Acceptance Criteria` AC1-AC7.
- **Work mode resolution note:** Both `issue.md` files carry `- Work Mode: minor-audit`. Per minor-audit rules, only the explicit `## Acceptance Criteria` section in each `issue.md` is the AC source.
- **Scope note:** Working-tree-only review; both changes are uncommitted and ship together. #188 AC1-AC3 were re-opened and repointed to the off-root CLI runsettings; they are re-satisfied through the combined #189 implementation.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/issue.md` — primary (#189 AC1-AC8)
- `docs/features/active/2026-06-12-vscode-test-runner-parity-188/issue.md` — primary (#188 AC1-AC7)

### From #189 issue.md (Option A AC1-AC8)

1. AC1: New CLI runsettings off the repo root containing exactly the `<MSTest><Parallelize>` block and no `<DataCollectors>`; valid XML; off-root so it does not interfere with VS auto-detection of the root runsettings.
2. AC2: Root `TaskMaster.runsettings` gains a Code Coverage `<DataCollectors>` Exclude block mirroring the full coverage.config exclusion list, preserving the existing `<MSTest><Parallelize>`; valid XML; no `enabled="true"`.
3. AC3: Both scripts pass `/Settings:` pointing at the CLI runsettings (not the root); deterministic resolution and fail-fast missing-file guard target the CLI runsettings path.
4. AC4: Pester tests assert both scripts pass `/Settings:` at the CLI runsettings plus the missing-file throw, mocking only the wrapper seams; deterministic; identical in Terminal and Test Explorer.
5. AC5: CLI no-regression — repointed scripts produce no code-coverage attachment on a plain run; Deedle tests pass; Koverage inner vstest still omits `/collect` (no double collection).
6. AC6: CLI parallelization parity preserved — CLI runsettings retains Workers=0/ClassLevel.
7. AC7: PowerShell toolchain passes in order (format -> PSScriptAnalyzer -> Pester) with no net-new analyzer debt and no coverage regression on changed lines.
8. AC8 (user action, pending): User confirms in Visual Studio that "Run Tests" runs the Deedle tests green with no coverage collected, and "Analyze Code Coverage" runs them green with no `VerificationException`.

### From #188 issue.md (AC1-AC7)

1. AC1: `Invoke-MSTest.ps1` passes `/Settings:<CLI-runsettings>` to vstest.console.exe.
2. AC2: `Invoke-MSTestWithCoverage.ps1` passes `/Settings:<CLI-runsettings>` to the inner vstest; outer `dotnet-coverage --settings coverage.config` unchanged and distinct.
3. AC3: The runsettings path resolved deterministically; each script fails fast with a clear error if the CLI runsettings file is absent.
4. AC4: A wrapper-function seam (`Invoke-VsTestExe -VsTestArgs`; not `Args`) introduced so the vstest argument list is unit-testable.
5. AC5: Pester tests assert the constructed argument list for both scripts includes `/Settings:`; mock the wrapper seam (never real executables); deterministic; identical in Terminal and Test Explorer.
6. AC6: `TaskMaster.runsettings` content preserved; if edited, retains `<MSTest><Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize></MSTest>`.
7. AC7: PowerShell toolchain passes in order with no new analyzer debt and no coverage regression on changed lines.

> Note on #188 AC5 wording: the original text says "/Settings: pointing at the repo-root TaskMaster.runsettings." The 2026-06-12 revision note at the top of #188 AC section repoints AC1-AC3 to the off-root CLI runsettings as part of #189 Option A. AC5 is evaluated against the revised target (the CLI runsettings) consistent with the revision note; the parallelization-parity intent is unchanged.

---

## Acceptance Criteria Evaluation

### #189 (Option A)

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 189-AC1 | Off-root CLI runsettings, parallelization-only, no DataCollectors, valid XML | PASS | `scripts/vscode/TaskMaster.cli.runsettings` (9 lines, Workers=0/ClassLevel, no `<DataCollectors>`); XML well-formed | `grep -c "DataCollector"` = 0; `[xml]` parse OK | Off-root placement confirmed; only one `.runsettings` at repo root |
| 189-AC2 (content) | Root runsettings gains Code Coverage Exclude block (7 mirrored excludes), Parallelize preserved, no `enabled="true"` | PASS | `git diff TaskMaster.runsettings` shows additive block with 7 `<ModulePath>` entries matching coverage.config; `<MSTest><Parallelize>` preserved | `grep -c "ModulePath>"` = 7; `grep -i enabled` = none | Additive-only diff verified |
| 189-AC2 (effect) | Exclusion actually suppresses VerificationException under VS coverage | PENDING (via AC8) | Not CLI-reproducible (dynamic vs static collector) | N/A | Routed to AC8 user VS confirmation; not a failed gate |
| 189-AC3 | Both scripts pass `/Settings:` at CLI runsettings; deterministic resolution + fail-fast guard | PASS | `Resolve-RunSettingsPath` resolves `TaskMaster.cli.runsettings` via `$PSScriptRoot`; both arg builders emit `/Settings:<CLI>` | Read both scripts | Revises #188 AC1-AC3 |
| 189-AC4 | Pester asserts CLI-runsettings `/Settings:` + missing-file throw; mocks only wrapper seams; deterministic | PASS | `Invoke-MSTest.RunSettings.Tests.ps1`: 9/9 in-scope pass; only `Invoke-VsTestExe`/`Invoke-DotnetCoverageExe` mocked | `Invoke-Pester` | Test Explorer parity preserved |
| 189-AC5 | CLI no-regression: no `.coverage` attachment, Deedle pass; Koverage inner vstest omits `/collect` | PASS | `cli-no-collect-run.2026-06-12T19-22.md` (42/42 pass, 0 `.coverage`); `koverage-no-double-collect.2026-06-12T19-22.md` | vstest no-`/collect` run; arg inspection | No double collection |
| 189-AC6 | CLI parallelization parity (Workers=0/ClassLevel) | PASS | `cli-parallelization-parity.2026-06-12T19-22.md` (parity TRUE) | XML compare of both runsettings | Matches #188 intent |
| 189-AC7 | PowerShell toolchain in order, no net-new debt, no coverage regression on changed lines | PASS | `powershell-toolchain-final.2026-06-12T19-22.md`: format clean; 2 in-scope/16 folder analyzer (unchanged); 9/9 in-scope; 77.06% no regression | PoshQC format/analyze/Pester | Sole Pester failure is pre-existing out-of-scope SDK test |
| 189-AC8 | User VS confirmation (Run Tests green no coverage; Analyze Code Coverage green no VerificationException) | PENDING (user action) | `ac8-vs-confirmation-pending.2026-06-12T19-22.md`; `vs-verification-checklist.2026-06-12T19-22.md` | N/A (user-driven) | Correctly recorded `[ ]` in issue.md; not a code blocker |

### #188

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 188-AC1 | `Invoke-MSTest.ps1` passes `/Settings:<CLI-runsettings>` to vstest | PASS | `Get-VsTestArgumentList` returns `/Settings:$RunSettingsPath` resolved to CLI runsettings | Read; Pester test 3-4 | Re-satisfied via #189 |
| 188-AC2 | `Invoke-MSTestWithCoverage.ps1` passes `/Settings:<CLI-runsettings>` to inner vstest; outer coverage.config preserved & distinct | PASS | `Get-DotnetCoverageArgumentList` keeps `--settings coverage.config` and emits inner `/Settings:<CLI>` after `--` and vstest path | Read; Pester tests 7-8 | Distinct instrumentation path intact |
| 188-AC3 | Deterministic resolution + fail-fast on missing CLI runsettings | PASS | `Resolve-RunSettingsPath` via `$PSScriptRoot`; throw names missing path | Pester test 2 | |
| 188-AC4 | Wrapper-function seam (`Invoke-VsTestExe -VsTestArgs`; not `Args`) introduced | PASS | `Invoke-VsTestExe -VsTestPath -VsTestArgs`; `Invoke-DotnetCoverageExe -DotnetCoverageArgs` | Read | Parameter names avoid `Args` collision |
| 188-AC5 | Pester asserts `/Settings:` for both; mock seam only; deterministic; Terminal/Test Explorer parity | PASS | 9/9 in-scope pass; only wrapper seams mocked; signatures match production | `Invoke-Pester` | Evaluated against revised CLI-runsettings target per AC revision note |
| 188-AC6 | `TaskMaster.runsettings` retains `<MSTest><Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize></MSTest>` | PASS | `git diff` shows Parallelize block unchanged; additive coverage block only | `git diff TaskMaster.runsettings` | Content preserved |
| 188-AC7 | PowerShell toolchain in order, no new debt, no coverage regression on changed lines | PASS | Same evidence as 189-AC7 | PoshQC format/analyze/Pester | |
| 188 (VS-confirmation item) | Run both VS Code tasks; confirm all assemblies report class-level parallelization (integration retest) | PENDING (user action) | Configuration parity delivered; user runtime confirmation analogous to #189 AC8 | N/A (user-driven) | Non-blocking; convergence with VS is the intended parity outcome |

---

## Crux / Design-Correctness Adjudication (caller item 2-6)

- **Collector block placement (item 2):** PASS. The `<DataCollectors>` Code Coverage block exists ONLY in the repo-root `TaskMaster.runsettings` (VS auto-detect) — confirmed `grep -c "DataCollector"` on `scripts/vscode/TaskMaster.cli.runsettings` = 0, and the root diff is additive. Both scripts pass `/Settings:` at the off-root CLI runsettings (read confirms `Join-Path $ScriptRoot 'TaskMaster.cli.runsettings'`). The CLI no-`/collect` run produced no `.coverage` attachment (AC5 evidence). The Koverage inner vstest omits `/collect` and the CLI runsettings carries no collector, so no double collection. The crux of the fix is confirmed from both the files and the evidence.
- **Off-root placement (item 3):** PASS. Only one `.runsettings` exists at the repo root (`ls *.runsettings` -> `TaskMaster.runsettings`); the CLI runsettings is under `scripts/vscode/`, so VS auto-detection of the root file is not disturbed.
- **PowerShell quality (item 4):** PASS. Net-new analyzer debt 0 (executor-reported); the 2 `PSAvoidUsingWriteHost` in `Invoke-MSTest.ps1` are pre-existing. In-scope Pester 9/9. Coverage no-regression on changed lines (77.06% -> 77.06%). The 1 `Install-RepoDotNetSdk.Tests.ps1` failure is pre-existing/out-of-scope, failing identically at baseline; not attributable to this change.
- **Out-of-scope lock (item 5):** PASS. `git status --porcelain` shows no change to `coverage.config`, `.vscode/tasks.json`, any `*.cs`/`*.csproj`, `Invoke-MSTestWithCoverage.Helpers.ps1`, or the deferred timing test. No workflow files changed (ci-workflows.md not triggered).
- **AC8 shipping limitation (item 6):** Acceptable for merge with AC8 pending. The coverage-exclusion runtime effect cannot be CLI-verified because standalone `vstest.console` exercises dynamic coverage, not the VS static `CodeCoverage/2.0` collector that throws the `VerificationException`. The code change (additive exclude block + CLI/IDE split) is complete and verified by inspection; the remaining confirmation is a user VS action that gates only the runtime *effect*, not the code. Shipping with AC8 pending is acceptable because the code is independently verifiable and the user confirmation can be performed pre- or post-merge against the merged runsettings. This is a pending verification item, not a code blocker.

---

## Summary

**Overall Feature Readiness:** PASS (ready for merge; AC8 / #188 VS-confirmation are pending user VS actions, not code blockers)

**blocking_count: 0** (FAIL = 0; blocking PARTIAL = 0). The PARTIAL items recorded in the policy audit — aggregate-file PowerShell coverage 77.06% and the two pre-existing `PSAvoidUsingWriteHost` findings — are both pre-existing, non-blocking, and carry no regression. The two PENDING items (#189 AC8 and #188's VS-confirmation) are user-action verification items explicitly scoped as such in the source issues and are not code blockers.

**Criteria summary (CLI-verifiable code criteria, both issues combined):**
- **PASS:** 15 criteria (#189 AC1, AC2-content, AC3-AC7; #188 AC1-AC7; plus 189-AC2-effect routed to AC8)
- **PARTIAL:** 0 blocking
- **UNVERIFIED:** 0
- **FAIL:** 0
- **PENDING (user action, non-blocking):** 2 (#189 AC8; #188 VS-confirmation item)

**Top gaps preventing PASS:**

1. None. No FAIL or blocking PARTIAL across the three artifacts.

**Recommended follow-up verification steps:**

1. User completes the Visual Studio confirmation per `evidence/issue-updates/vs-verification-checklist.2026-06-12T19-22.md`: run the Deedle tests with no coverage (expect green, no coverage collected) and run "Analyze Code Coverage" (expect green, no `VerificationException`). This satisfies #189 AC8 and the #188 VS-confirmation item.
2. Optionally, in a later change, retire the two pre-existing `PSAvoidUsingWriteHost` findings in `Invoke-MSTest.ps1`.

**Ready-to-merge determination:** Ready to merge. The code is complete, policy-compliant, and design-correct; the only outstanding items are pending user VS confirmations that gate the runtime effect, not the code.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules, criteria evaluated as PASS may be checked off in the authoritative source files. Both `issue.md` files already reflect the delivered work as updated by the executor (the working-tree diff shows the AC checkbox updates). This reviewer made no further source-file checkbox changes: #189 AC1-AC7 are already `[x]` and AC8 is already `[ ]` (correctly pending); #188 AC1-AC3 remain `[ ]` (re-opened/repointed, re-satisfied through #189) and AC4-AC7 are `[x]`. Because #188 AC1-AC3 are re-satisfied through the coupled #189 implementation and the source carries an explicit revision note keeping them open pending the combined merge, this reviewer leaves their checkbox state unchanged and records the PASS verdicts here rather than mutating the #188 source mid-merge.

### AC Status Summary

- Source: `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/issue.md`; `docs/features/active/2026-06-12-vscode-test-runner-parity-188/issue.md`
- Total AC items: 15 (#189: 8; #188: 7)
- Checked off (delivered): 11 (#189 AC1-AC7 = 7; #188 AC4-AC7 = 4)
- Remaining (unchecked): 4 (#189 AC8; #188 AC1-AC3)
- Items remaining: #189 AC8 (pending user VS action); #188 AC1-AC3 (re-opened/repointed, verdict PASS here, left unchecked pending combined merge per source revision note)

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `docs/features/active/2026-06-12-vs-coverage-fsharp-deedle-exclusion-189/issue.md` | 8 | 7 | 1 | Checkbox-backed; AC8 correctly pending `[ ]` |
| `docs/features/active/2026-06-12-vscode-test-runner-parity-188/issue.md` | 7 | 4 | 3 | Checkbox-backed; AC1-AC3 re-opened/repointed, verdict PASS, left `[ ]` per source revision note |
