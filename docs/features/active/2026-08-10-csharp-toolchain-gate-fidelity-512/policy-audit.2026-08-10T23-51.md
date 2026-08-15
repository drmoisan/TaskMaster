# Policy Audit — 2026-08-10-csharp-toolchain-gate-fidelity-512

- **Artifact:** `policy-audit.2026-08-10T23-51.md`
- **Reviewer:** feature-review agent
- **Branch under review:** `bug/csharp-toolchain-gate-fidelity-512` (head `9773d6f5`)
- **Base:** `origin/epic/build-ci-coverage-gate-fidelity-integration` (`143984a7`)
- **Merge base (recomputed by this reviewer):** `a5e336e5ae3443d4197caf5f87036fae1d538f89` — matches the caller-supplied value and the value recorded throughout the feature evidence.
- **Work mode:** `full-bug` (marker verified in `issue.md`; AC source is `spec.md`)
- **Audit scope:** full branch diff `origin/epic/build-ci-coverage-gate-fidelity-integration...HEAD` (60 files: 6 source/config/governance edits, 54 feature-folder documentation and evidence files)

## Executive Summary

The delivery corrects the documented C# toolchain gate commands (format, analyzer, type-check) at the three authorized governance sites and in the three executable carriers, with the negative-path proof, CI-parity comparison, repository-wide site reconciliation, and both-language toolchain evidence all present and independently spot-verified by this reviewer. Overall verdict: **PASS with zero blocking findings**. One procedural coverage-artifact finding is recorded and dispositioned non-blocking (section 1.2.1). One minor documentation-consistency observation and two informational observations are recorded in `code-review.2026-08-10T23-51.md`.

## Rejected Scope Narrowing

None detected. The caller's prompt mandated the full branch diff against the resolved base and did not attempt to narrow scope to any plan, task, phase, or file subset. The caller's instruction that governance edits inside the three enumerated files are authorized is an authorization-boundary declaration, not a scope narrowing; this reviewer independently verified the authorization's basis (epic charter "Execution Authorization Required" as quoted in `spec.md` and `issue.md` § Governance-Document Authorization) and independently verified its boundaries were respected (section 2 below), rather than accepting the assertion.

## 1. Coverage Verification

### 1.1 Changed-language enumeration (from the branch diff, independently derived)

`git diff --name-status origin/epic/build-ci-coverage-gate-fidelity-integration...HEAD`:

- **PowerShell** — changed files: `scripts/vscode/Invoke-VSBuild.ps1` (modified, production), `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` (modified, test).
- **C#** — zero `.cs`, `.csproj`, `.props`, `.targets` files in the branch diff. Independently verified; the AC4 negative-control perturbation of `UtilitiesCS/Extensions/QueueExtensions.cs` was reverted before commit (revert proven three ways in `evidence/qa-gates/typecheck-negative-control.2026-08-10T23-58.md`) and does not appear in the diff. The C# coverage gate is therefore not engaged for this branch.
- **Python** — zero changed files; coverage gate not engaged.
- **TypeScript** — zero changed files; coverage gate not engaged.

PR context artifacts were absent from this worktree at review start; this reviewer regenerated `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` manually from `git diff --numstat` / `git diff` (the `collect_pr_context` MCP tool is not available in this session). The regenerated summary classifies the changed languages exactly as above, so the recurring C#-misclassification defect cannot occur here.

### 1.2 Per-language coverage verdicts

#### 1.2.1 PowerShell

- PowerShell repo-wide line coverage from the canonical artifact `artifacts/pester/powershell-coverage.xml`: **FAIL (procedural)** — the artifact's summed JaCoCo LINE counters read 0.00% (covered 0 / missed 3215), below the 85% floor. Disposition: **non-blocking**, for the reasons itemized below.
- PowerShell changed-file line coverage for `scripts/vscode/Invoke-VSBuild.ps1`: **PASS** — 85.71% (42 of 49 analyzed commands), at or above the 85% floor. Baseline 85.71%, post-change 85.71%, change 0.00 pp; disposition: no regression. Evidence: `evidence/baseline/baseline-poshqc-test.2026-08-10T23-08.md`, `evidence/qa-gates/final-poshqc-test.2026-08-11T00-40.md`, `evidence/qa-gates/powershell-coverage-delta.2026-08-11T00-45.md`.
- PowerShell new/changed-code coverage: **100%** of analyzed changed commands (3 of 3: the `$arguments` array assignment enclosing `"/t:$Target"`, the `Write-Warning` replacement at line 117, and the `-Target $Target` call-site at line 158 are all executed; the remaining changed lines are parameter declarations, attributes and comments, which Pester's command-based model does not analyze). **PASS.** Evidence: `evidence/qa-gates/powershell-coverage-delta.2026-08-11T00-45.md` § 3; missed-command set proven identical to baseline (§ 4).
- PowerShell branch coverage: structurally unavailable from Pester 5.6.1, whose coverage model is command-based; the artifact contains zero `type="BRANCH"` counters (verified directly by this reviewer) and the runner's configuration/result property lists (enumerated in evidence) contain no branch counter. This matches the repository's long-standing PowerShell coverage evidence shape and does not constitute a floor violation.

**Why the repo-wide FAIL is procedural and non-blocking.** The canonical artifact was produced by the executor's bundled PoshQC test run (report header `Pester (08/10/2026 23:25:45)`), whose coverage denominator enumerates only `.claude/hooks`, `.claude/lib/blast-radius`, `.claude/lib/model-routing`, `.claude/lib/orchestrator-state`, and `.codex/hooks` — five package trees this feature does not touch — while the tests executed in that run were the `tests/scripts/vscode` suite, which exercises none of those files. The 0.00% reading is therefore an unmeasured aggregation (denominator and executed-test set are disjoint), not a measured regression, and the artifact omits the one production PowerShell file this branch changes. The measurement that does bear on this branch — the direct Pester channel over `scripts/vscode/Invoke-VSBuild.ps1` (41 tests, 41 passed) — clears the floor with zero changed-line regression and an identical missed-command set. This follows the repository's established rescoping precedent for canonical artifacts that aggregate un-instrumented modules. Residual action (non-blocking): a future PoshQC or review cycle should regenerate the canonical PowerShell artifact with a coverage path set that matches the executed test set, so the repo-wide figure is a measurement rather than an artifact of scoping.

#### 1.2.2 C#

Zero C# source, test, or project files are changed on this branch (verified in 1.1), so no C# coverage measurement is required for this diff. The recorded step-4 scope deviation (`evidence/qa-gates/csharp-test-step-scope.2026-08-11T00-55.md`) correctly documents that `vstest.console.exe` was deliberately not run because it would validate nothing this feature altered, and separately documents the pre-existing, unrelated `Run MSTest suite with coverage` failure on `main`'s tip.

#### 1.2.3 Python

Zero Python changed files on this branch; no verdict required.

#### 1.2.4 TypeScript

Zero TypeScript changed files on this branch; no verdict required.

## 2. Governance-Edit Authorization Boundary — PASS

The epic charter authorization covers exactly `CLAUDE.md`, `.claude/rules/csharp.md`, and `.claude/skills/csharp-qa-gate/SKILL.md`, at the C# toolchain command sites. Verified against the full diff:

| Check | Result |
|---|---|
| Governance edits confined to the three authorized files | **PASS** — the only governance files in the diff are those three |
| Edits within those files confined to C# toolchain command sites and adjacent rationale | **PASS** — hunks are: `CLAUDE.md` § C#1 items 1–3, § CUT3 items 1–3, § "C# Toolchain (run in this exact order)" items 1–3; `.claude/rules/csharp.md` § Toolchain items 1–3 and the § Severity-first invariant's embedded command string (invariant text preserved verbatim); `.claude/skills/csharp-qa-gate/SKILL.md` steps 1–3 plus the appended R6 evidence bullet. All match the `spec.md` replacement table |
| `CLAUDE.md` § UT2 byte-identical to merge base | **PASS** — independently verified by this reviewer via section extract diff (empty), corroborating `evidence/qa-gates/claudemd-ut2-guard.2026-08-10T23-42.md` |
| `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md` unchanged | **PASS** — independently verified: zero-length diff against the merge base |
| `AGENTS.md`, `.agents/**`, `.github/instructions/**`, `.github/agents/**`, `.codex/**`, `.github/workflows/ci.yml` unchanged | **PASS** — independently verified: zero-length diff against the merge base for all six path sets |

## 3. AC8 Adjudication — Removal of `/p:Nullable=enable` Is a Strengthening — PASS

Adjudicated on the merits against the measured evidence, as the caller required, not accepted from the spec's argument alone:

1. The defective gate performed no enforcement in the steady state: the warm `/t:Build` run measured `EXIT_CODE: 0` in 1.8 s with `CoreCompile` skipped on 18 of 18 projects (`baseline-doc-typecheck-warm.2026-08-10T22-52.md`), so `/p:Nullable=enable` never reached the compiler during a normal toolchain loop.
2. When compilation was forced with the flag retained, the gate was unpassable: `EXIT_CODE: 1` with 195 pre-existing errors, all in `UtilitiesCS.csproj` (`baseline-nullable-debt.2026-08-10T22-57.md`), debt this feature is explicitly charged not to fix.
3. The decisive test is the negative control: the corrected command, **without** the flag, returned `EXIT_CODE: 1` with `error CS8603` when a nullable violation was introduced into an opted-in file (`typecheck-negative-control.2026-08-10T23-58.md`, zero `CoreCompile` skips, perturbed project proven compiled from the log). Enrollment comes from the per-file `#nullable enable` pragma, not the command-line property, so no opted-in file loses enforcement.
4. The corrected command is character-for-character CI's command (verified by this reviewer against `.github/workflows/ci.yml`, "Build with nullable warnings treated as errors": `/t:Rebuild /m ... "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`, no `/p:Nullable=enable`), and CI's in-workflow comment states the same rationale. Branch protection consumes CI's checks, so no merge gate ever enforced the flag.

Conclusion: the change moves the gate from zero effective enforcement (or false blocking when forced) to CI parity with proven fail-capability. It is a strengthening, not a relaxation. Supplementary AC8 checks in `evidence/qa-gates/no-relaxation-review.2026-08-11T00-25.md` (no threshold reduced; four-step order and restart rule intact at all five sites; zero added suppression tokens — spot-checked by this reviewer against the diff) hold.

## 4. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Bugfix workflow: failing regression test before the fix | **PASS** | `evidence/regression-testing/red-pester-run.2026-08-10T23-20.md` (two failing `It` blocks against the pre-fix implementation), `green-pester-run.2026-08-10T23-32.md` |
| Minimal, targeted fix; no opportunistic refactor | **PASS** | Diff confined to the enumerated sites and carriers; deeper residuals routed to issue #535 instead of widening scope |
| Full toolchain run and green in a single final pass (both languages) | **PASS** | `evidence/qa-gates/completion-attestation.2026-08-11T01-08.md` § 1; C# format/check exit 0, analyze and type-check exit 0 with zero `CoreCompile` skips, PoshQC format 0 / analyze 16=16 multiset / test 0 |
| 500-line file limit | **PASS** | `scripts/vscode/Invoke-VSBuild.ps1` 167 lines; `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` 86 lines (baseline 65); `.vscode/tasks.json` 228 lines. Markdown documentation files are exempt |
| No new dependency; I/O boundaries unchanged | **PASS** | Diff inspection: no module, package, or tool addition |
| Public API compatibility | **PASS** | `-Target` defaults to `Build` so existing callers are unchanged; `-EnableNullable` retained as a bindable warning-emitting no-op (SD3 note) rather than a breaking removal |

## 5. Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence / isolation / determinism | **PASS** | Both changed `Describe` blocks unit-test pure functions through the existing `-NoExecute` dot-source seam; no mocks needed, no temp files, no external processes |
| Test location mirrors production tree | **PASS** | `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` mirrors `scripts/vscode/Invoke-VSBuild.ps1` |
| Arrange–Act–Assert and descriptive names | **PASS** | New `It 'emits /t:Rebuild in the target position when -Target Rebuild is supplied'`; renamed `It 'emits no MSBuild property for the deprecated -EnableNullable switch'` asserts exact array equality, not a subset |
| Scenario completeness proportionate to the change | **PASS** | Default-target guard retained unchanged; new-parameter positive case added; deprecated-switch negative case rewritten; `ValidateSet` rejects invalid `-Target` at bind time (edge case documented in spec) |
| No temporary files in tests | **PASS** | Diff inspection |

## 6. PowerShell Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| PoshQC format → analyze → test via MCP functions, VS Code wrappers not substituted | **PASS** | `final-poshqc-format.2026-08-11T00-34.md` (exit 0), `final-poshqc-analyze.2026-08-11T00-36.md`, `final-poshqc-test.2026-08-11T00-40.md` |
| PSScriptAnalyzer: no new finding vs the RED merge-base baseline | **PASS** | Baseline is RED with 16 pre-existing findings; final run 16 findings, `(Severity, Rule, File)` multiset identical, per-finding line shifts fully reconciled against the recorded insertion deltas (+5/+9/+10) with a zero formatter delta. Exit 1 is the pre-existing merge-base condition, not a regression |
| Change budget (direct-mode limit of two production PowerShell files) | **PASS** | One production `.ps1` plus its test file |
| Logging pattern | **PASS** | The deprecation notice uses `Write-Warning`, not `Write-Host`; no new PSAvoidUsingWriteHost finding |

## 7. Evidence Location Compliance

**PASS.** All evidence artifacts in the branch diff live under the canonical `docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/evidence/<kind>/` tree (baseline, qa-gates, regression-testing, issue-updates). The diff contains zero files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` (verified against the full name-status listing). The referenced repo-local validator script `validate_evidence_locations.py` does not exist in this repository; the scan above was performed directly against the diff instead. All plan-produced artifacts carry `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:` fields (attested mechanically in `completion-attestation.2026-08-11T01-08.md` § 3; spot-checked by this reviewer on eight artifacts).

## 8. Non-Vacuity Mechanism Adjudication (AC2 substitution)

The plan substitutes a zero-count assertion on the literal `Skipping target "CoreCompile"` in an `/fl` file log for AC2's `csc.exe`-count parenthetical, recorded as a formal deviation in `spec.md` § "Recorded deviations". Judged **at least as discriminating as the criterion's stated intent**: the measured baseline shows the `csc.exe` count is zero at `verbosity=normal` even for genuine compiles (M3, M4), so the criterion's literal mechanism cannot distinguish anything, whereas the skip count cleanly separates the vacuous runs (18 skips in M2/A2) from the genuine ones (0 skips in M3/M4/A3). The skip assertion is also stronger than the intent's minimum: `csc.exe count > 0` would prove at least one project compiled, while `skip count == 0` proves no project short-circuited. The substitution is auditable at every use site (positive control, negative control, final analyze, final type-check all record the count). Accepted.

## Verdict

**PASS.** Zero blocking findings. One procedural, non-blocking coverage-artifact finding (section 1.2.1 disposition), referred forward as a recommendation rather than a remediation trigger.
