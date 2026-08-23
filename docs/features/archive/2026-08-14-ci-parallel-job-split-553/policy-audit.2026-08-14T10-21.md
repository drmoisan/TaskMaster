# Policy Compliance Audit — ci-parallel-job-split (Issue #553)

- **Component:** GitHub Actions CI pipeline (`.github/workflows/`)
- **Date:** 2026-08-14 (artifact timestamp 2026-08-14T10-21)
- **Reviewer:** feature-review agent
- **Base branch:** `main` (resolved `origin/main`)
- **Merge base:** `2073f717bbfac30053f3d6a4e652d99af3ae5c9c` (independently recomputed via `git merge-base HEAD origin/main`; matches caller-supplied value)
- **Branch head:** `feature/ci-parallel-job-split-553` @ `0b016c81a78f3fafc0864de472f4139cc0938002` (3 commits: `e246688b`, `955e17fa`, `0b016c81`)
- **PR context:** `artifacts/pr_context.summary.txt` / `artifacts/pr_context.appendix.txt` — fresh (recorded head SHA matches current `git rev-parse HEAD`)
- **Work mode:** `full-feature` (persisted marker in `issue.md`); AC sources: `spec.md` and `user-story.md`
- **Files under audit:** full branch diff vs merge base — 37 files (6 workflow YAML files, `.github/workflows/README.md`, 22 feature-folder docs/evidence files, 2 archival promoted-potential copies, 6 agent-memory files)

## Executive Summary

This branch decomposes the monolithic `quality-gates` CI job into five callee reusable workflows invoked by a pure-orchestrator `ci.yml`. The diff contains zero source-code files in any coverage-bearing language: `git diff --name-only 2073f717..HEAD` matched against `*.cs, *.csproj, *.props, *.targets, *.ps1, *.psm1, *.psd1, *.py, *.ts, *.tsx, *.js` returns empty. The change set is workflow YAML plus Markdown documentation and evidence.

Verdicts:

- Gate-transplant fidelity: **PASS** — byte-identity of every transplanted gate `run:` block was independently re-verified by this review (extraction + SHA-256 comparison against the merge-base `ci.yml`), not taken from the committed evidence artifact. All 14 compared blocks match.
- Workflow lint: **PASS** — actionlint 1.7.7 exit 0 across all 7 workflow files (`evidence/qa-gates/actionlint-postchange.2026-08-14T09-54.md`); pre-change baseline also clean.
- `.claude/rules/ci-workflows.md`: **PASS** — no `pwsh` step uses the deliberately-failing nested-command pattern (assessment in section 3.2).
- `.claude/rules/benchmark-baselines.md`: **PASS** with note — runner-environment parity satisfied; the sibling-provenance requirement is scoped to baselines consumed by a benchmark regression gate, which this baseline is not (section 3.3).
- Evidence locations: **PASS** — all evidence under the canonical `docs/features/active/<feature>/evidence/` tree; zero diff files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`.
- `modified-workflow-needs-green-run`: **FAIL (Blocking)** — the branch modifies `.github/workflows/**` and no green workflow run against head `0b016c81` exists or is evidenced. This is the single blocking finding; it is procedural and is already scheduled by the plan of record (Phases 3–5), which require a live PR. See section 7.3 and `remediation-inputs.2026-08-14T10-21.md`.

No caller-supplied scope narrowing was detected; the caller's factual notes were verified independently and the audit scope is the full branch diff vs `main`.

## 1. General Unit Test Policy Compliance

| Check | Verdict | Evidence |
| --- | --- | --- |
| New or modified unit tests present | None in diff | `git diff --name-status` — zero test files changed |
| Test independence / isolation / determinism | Not exercised by this diff | No test code changed |
| Temporary-file prohibition in tests | Not exercised | No test code changed |
| Test file location rule | Not exercised | No test files added or moved |

The MSTest suite itself is unmodified; the vstest invocation that runs it in CI is transplanted byte-identically (verified in section 7.2), so test execution semantics in CI are unchanged.

### 1.1 Changed-language enumeration (full branch diff)

Enumeration command: `git diff --name-only 2073f717bbfac30053f3d6a4e652d99af3ae5c9c..HEAD` filtered per extension.

| Language | Changed files in branch diff | Coverage gate |
| --- | --- | --- |
| C# (`.cs`/`.csproj`/`.props`/`.targets`) | 0 | Zero changed files; no per-language coverage measurement is triggered for this branch |
| PowerShell (`.ps1`/`.psm1`/`.psd1`) | 0 | Zero changed files; the `pwsh` `run:` blocks inside workflow YAML are transplanted verbatim and are not PowerShell script files |
| Python (`.py`) | 0 | Zero changed files |
| TypeScript (`.ts`/`.tsx`) | 0 | Zero changed files |

Note on the PR-context summary: the "Changed files overview" classifies all 29 listed files as docs/tooling and lists 0 core-logic files. Unlike prior C#-misclassification incidents, this classification is materially accurate for this branch — the only non-doc changes are workflow YAML, which has no coverage denominator. Verified against `git diff` directly, not trusted from the summary.

## 2. General Code Change Policy Compliance

| Check | Verdict | Evidence |
| --- | --- | --- |
| Simplicity / separation of concerns | PASS | Orchestrator/callee split follows the repo's mandated reusable-workflow pattern (`.claude/skills/orchestrate/SKILL.md` § GitHub Actions Reusable Workflows); one gate per callee |
| 500-line file limit | PASS | `ci.yml` 32, `_actionlint.yml` 29, `_format-check.yml` 41, `_build-analyzers.yml` 53, `_build-nullable.yml` 60, `_mstest-coverage.yml` 96 lines (verified `grep -c ""`; matches the figures in `byte-identity.2026-08-14T09-54.md`). Markdown files are exempt by rule |
| Fail fast / error handling | PASS | `$LASTEXITCODE` guards and `throw` guards preserved verbatim; `set -euo pipefail` retained in the actionlint step |
| No new dependencies | PASS | All actions (`actions/checkout@v4`, `actions/setup-dotnet@v4`, `microsoft/setup-msbuild@v2`, `nuget/setup-nuget@v2`, `actions/cache@v4`, `actions/upload-artifact@v4`) already in use at the merge base |
| Supporting docs updated | PASS | `.github/workflows/README.md` created (was referenced by `.claude/skills/orchestrate/SKILL.md` before it existed; see section 7.5) |
| Bugfix workflow | Not applicable — feature work, not a defect fix | — |
| Policy documents modified | PASS (none) | Diff touches no `.claude/rules/` or `.github/instructions/` file |

## 3. Language-Specific Code Change Policy Compliance

### 3.1 C# Code Change Policy

Zero C# source, test, or build-configuration files changed (verified: `git diff --name-only <merge-base>..HEAD -- '*.cs' '*.csproj' '*.props' '*.targets'` returns empty). The C# toolchain loop is therefore not required for this branch, consistent with spec.md Definition of Done item 5. The CI gate commands that enforce the C# toolchain were verified byte-identical (section 7.2), so this branch does not alter C# enforcement semantics.

### 3.2 `.claude/rules/ci-workflows.md` — deliberately-failing nested command pattern

Every `pwsh` step in the new callees was assessed individually:

| File | `pwsh` step | Intentionally-failing nested command? | Assessment |
| --- | --- | --- | --- |
| `_format-check.yml` | `Setup CSharpier` (`dotnet tool restore`) | No | Failure is a genuine step failure; propagation intended |
| `_format-check.yml` | `Verify formatting` (`dotnet csharpier check .`) | No | Non-zero exit IS the gate signal |
| `_build-analyzers.yml` | `Restore solution`, analyzer build | No | Explicit `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` guard propagates deliberately |
| `_build-nullable.yml` | `Restore solution`, nullable build | No | Same guard, preserved verbatim with the `/t:Rebuild` rationale comment |
| `_mstest-coverage.yml` | `Restore solution`, `Build solution`, vstest step | No | Build step carries the same exit guard; vstest step `throw`s on non-zero, with `Set-StrictMode` and `$ErrorActionPreference = 'Stop'` |

No step invokes a command expected to fail as part of a passing path, so the rule's reset/`exit 0` requirement does not apply to any step in the committed pipeline. The README's Rules section states this explicitly and correctly distinguishes gate-signal propagation from residual-exit-code leakage. **Verdict: PASS.**

### 3.3 `.claude/rules/benchmark-baselines.md` — measured latency baseline

The branch commits `evidence/baseline/ci-sequential-baseline.2026-08-14T13-05.md`, a measured 444s baseline.

- **Rule scope check:** the rule states "This rule applies to any baseline consumed by a benchmark regression gate." This baseline is a one-time comparison reference for spec AC 10 (post-split duration comparison); it is not consumed by `scripts/benchmarks/**` tooling or any automated regression gate, and it is Markdown, not BenchmarkDotNet JSON (so the `HostEnvironmentInfo.ProcessorName` rejection condition cannot apply structurally).
- **Runner-environment parity (the rule's substantive requirement):** satisfied. The baseline was captured from GitHub-hosted `windows-latest` run 31749877507 (URL recorded), and both spec.md and the README require the post-split measurement to use the same `gh api .../runs/<id>/jobs` collection method on a GitHub-hosted run.
- **Sibling `baseline.provenance.json`:** absent, and not required under the rule's scope clause for a baseline that no regression gate consumes. The baseline nonetheless records the provenance fields inline (runner class, workflow run URL, collection command). Recorded as a non-blocking observation in the code review (finding F3): if this baseline is ever wired into an automated regression gate, a sibling provenance file must be added first.

**Verdict: PASS**, with observation F3.

### 3.4 PowerShell / Python / TypeScript policies

Not triggered: zero changed script files in those languages. The embedded `pwsh` blocks are governed by section 3.2, not by the PowerShell script-file policy (they are not `.ps1` files, are transplanted verbatim, and PoshQC does not target workflow YAML).

## 4. Language-Specific Unit Test Policy Compliance

No unit tests in any language were added or modified. MSTest/Moq/FluentAssertions framework rules are not exercised. **Verdict: not triggered by this diff.**

## 5. Test Coverage Detail

Coverage verification is mandatory for every language with changed files in the branch diff. Per section 1.1, the branch diff contains **zero changed files in every coverage-bearing language** (C#, PowerShell, Python, TypeScript), verified directly against `git diff --name-only` rather than the PR-context summary classification.

- C# — 0 changed files; no C# coverage row is required for this branch and no C# coverage artifact is required to exist for it.
- PowerShell — 0 changed `.ps1`/`.psm1` files; no PowerShell coverage row is required for this branch.
- Python — 0 changed files; no Python coverage row is required.
- TypeScript — 0 changed files; no TypeScript coverage row is required.

No per-file, new-file, or repo-wide coverage threshold applies to workflow YAML or Markdown. The CI coverage-producing step (`vstest.console.exe /EnableCodeCoverage`) is preserved byte-identically, and the `test-results` artifact upload (`TestResults/**/*.trx`, `TestResults/**/*.coverage`) is unchanged, so this branch does not reduce any coverage signal produced by CI.

## 6. Test Execution Metrics

No local test executions were required or run (no code changed in any tested language). Checks executed by the implementing session and verified by this review from committed evidence:

| Check | Result | Evidence |
| --- | --- | --- |
| actionlint pre-change baseline | exit 0, 0 findings, 2 files | `evidence/baseline/actionlint-baseline.2026-08-14T09-54.md` |
| actionlint post-change | exit 0, 0 findings, 7 files (verbose per-file table) | `evidence/qa-gates/actionlint-postchange.2026-08-14T09-54.md` |
| Byte-identity of transplanted blocks | 6/6 containment + 6/6 SHA-256, 12/12 fragment citations | `evidence/qa-gates/byte-identity.2026-08-14T09-54.md` |
| Byte-identity — independent re-verification by this review | 14/14 blocks MATCH | Section 7.2 (Appendix B command 3) |

The local actionlint run notes a genuine local-vs-CI gap (shellcheck/pyflakes integrations unavailable on the Windows host); the authoritative verification is the green run on the branch head, which is the open blocking item.

## 7. Code Quality Checks

### 7.1 Structural verification of the split (this review, direct file inspection)

- `ci.yml` (32 lines): five jobs, each `uses: ./.github/workflows/_<name>.yml`; no `steps:`, no `needs:` (grep verified across all six workflow files); `name:`, triggers, `permissions: contents: read`, and the concurrency block (`group: ci-${{ github.workflow }}-${{ github.event.pull_request.number || github.ref }}`, `cancel-in-progress: true`) are unchanged from the merge base.
- All five callees declare `on: workflow_call:` + `on: workflow_dispatch:`, own `permissions: contents: read`, right-sized `timeout-minutes` (10/10/30/30/30), and no `concurrency` block (grep verified).
- Only one `upload-artifact` in the new pipeline (`test-results` in `_mstest-coverage.yml`, `if: always()`, same name/paths/`if-no-files-found: warn`); zero `download-artifact` — no cross-job file sharing.

### 7.2 Byte-identity — independently verified

The caller instructed verification against the actual files rather than the committed artifact. This review extracted each step `run:` block (and full step blocks for cache/upload steps) from the merge-base `ci.yml` (`git show 2073f717:.github/workflows/ci.yml`) and from the new callees, dedented uniformly, and compared SHA-256 digests. Result: **14/14 MATCH** — actionlint run block; `dotnet tool restore`; `dotnet csharpier check .`; `nuget restore` (x3 callees); analyzer msbuild block with guard; nullable msbuild block with the full 7-line `/t:Rebuild` rationale comment and guard; vstest block with the zero-assembly `throw`, discovery filter, and `/EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`; the `Upload test results` step block; both cache step blocks. This independently corroborates `evidence/qa-gates/byte-identity.2026-08-14T09-54.md` (whose six SHA-256 rows and 12 fragment citations are consistent with these findings).

Gate-equivalence of the new `Build solution` step in `_mstest-coverage.yml` (no pre-split counterpart): verified to carry **no** `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`, `TreatWarningsAsErrors`, or other warning-promotion property (grep count 0 in the file). It neither weakens nor duplicates the analyzer/nullable gates: those remain enforced solely by their own required contexts, and a plain `/t:Build` produces functionally identical assemblies for test discovery (the analyzer/nullable properties affect diagnostics, not emitted behavior). The step carries the standard exit guard. Assessed: **no gate is weakened or altered**.

### 7.3 `modified-workflow-needs-green-run` (feature-review policy rule)

Trigger check (performed manually; `scripts/feature-review/Test-ModifiedWorkflowNeedsGreenRun.ps1` does not exist in this repository — see section 8): `git diff --name-only <merge-base>..HEAD` matches `.github/workflows/**` (7 files: `ci.yml` modified, 5 callees added, `README.md` added). No path matches `scripts/benchmarks/**` or `.github/actions/**`.

Evidence check: no green workflow run against branch head `0b016c81` exists — the branch has not been pushed/PR'd (plan tasks P3-T1..P3-T4 unchecked by design), GitHub CLI is unavailable in this environment (per the PR-context artifact), and no green-run evidence artifact exists in the feature folder.

**Verdict: FAIL — Blocking finding B1**, recorded in `remediation-inputs.2026-08-14T10-21.md`. Note: the spec, plan, and README all correctly anticipate this rule; the plan schedules the green run (P3-T4), the pre-migration green confirmation (P5-T15), and the `workflow_dispatch` fallback path. The finding is procedural sequencing (the rule cannot be satisfied before a live run exists), not a defect in the change set.

### 7.4 Required-status-check contract — under-gating analysis

The `main` ruleset (id 18572843, `strict_required_status_checks_policy: true`) requires `actionlint` and `Format, build, analyze, and test`. After this change both contexts cease to report (the actionlint job now reports in `<caller job> / <callee job>` form; the monolith job no longer exists). Consequence assessed from the ruleset semantics: required contexts that never report **block** merging. Both pre-PUT orderings therefore over-block; nothing in this branch can under-gate `main`. The only under-gating hazard is an incomplete contexts set in the ruleset PUT, which the spec's Required-Status-Check Contract (single atomic PUT of the full writable object, contexts captured from a live run, two-step edit prohibited) and the README's step-by-step procedure both address correctly. The PUT itself is deliberately not performed on this branch (it requires a live green run first, and plan P6-T3 marks it orchestrator-confirmation-required). **Verdict: PASS for the change set and documentation; execution pending (spec AC 6, tracked in remediation inputs as a dependency of B1's resolution sequence).**

### 7.5 README vs `.claude/skills/orchestrate/SKILL.md` § GitHub Actions Reusable Workflows

The skill section requires `.github/workflows/README.md` to contain "the full per-stage dispatch and branch-protection rename procedure." The README provides: a per-stage `workflow_dispatch` procedure (`gh workflow run` commands, UI path, and two correctly-stated caveats about standalone runs) and a five-step branch-protection rename procedure (fail-closed rationale, live-name capture, atomic PUT with writable-fields payload construction, prohibition of two-step edits, verification GET, evidence recording, rollback). It also documents the naming convention, one-level nesting, no-implicit-cross-job-filesystem property, and the two governing rules. **Verdict: PASS** — the section's requirement is satisfied. One wording imprecision noted (code review finding F2).

### 7.6 Evidence Location Compliance

`scripts/dev_tools/validate_evidence_locations.py` does not exist in this repository (see section 8); the equivalent check was performed directly: `git diff --name-only <merge-base>..HEAD` filtered for `^artifacts/(baselines|qa|evidence|coverage)/` returns **zero** files. All evidence artifacts on the branch live under the canonical `docs/features/active/2026-08-14-ci-parallel-job-split-553/evidence/<kind>/` tree (`baseline/`, `qa-gates/`, `other/`, including `other/pre-split/`). **Verdict: PASS.** No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` events: no caller instruction supplied a non-canonical evidence path.

## 8. Gaps and Exceptions

1. **Blocking gap B1:** no green workflow run against branch head `0b016c81` (`modified-workflow-needs-green-run`). Routed to remediation inputs. Resolution requires the live-PR phases of the plan of record (P3 onward).
2. **MCP template/validator tooling unavailable in this session.** `mcp__drm-copilot__resolve_policy_audit_template_asset` and `mcp__drm-copilot__validate_orchestration_artifacts` are not in this agent's tool surface. Per `policy-audit-template-usage` § Template Source fallback, this artifact reproduces the canonical major-heading set enumerated in that skill's prose. Documented assumption, consistent with prior review cycles in this repository.
3. **Referenced validator scripts absent from this repository:** `scripts/feature-review/Test-ModifiedWorkflowNeedsGreenRun.ps1` and `scripts/dev_tools/validate_evidence_locations.py` are named by skills but do not exist. Their checks were performed manually with `git diff --name-only` filters (sections 7.3, 7.6). Pre-existing documentation gap, not introduced by this branch.
4. **Artifact-layout conflict between skills (pre-existing, recorded for resolution):** `remediation-handoff-atomic-planner` specifies `audit/<ts>/` and `remediation/<ts>/` folder layouts, while the enforced hook `.claude/hooks/validate-feature-review-coverage.ps1` requires the flat `docs/features/active/<slug>/<stem>.<timestamp>.md` form. This review uses the flat form (the enforced contract). Likewise, `feature-review-workflow` step 8 assigns remediation-plan creation to the reviewer while `remediation-handoff-atomic-planner` assigns plan authorship to `atomic-planner`; this review follows the handoff skill and writes remediation inputs only.
5. **PR-context summary autoclose list** contains spurious author-asserted tokens (`#ISO-8601`, `#SHA-256`) harvested from prose. Generator quirk; noted so the eventual PR body (authored via the `pr-author` skill) lists only #553.

## 9. Summary of Changes

- `.github/workflows/ci.yml`: 160 → 32 lines; rewritten as a pure orchestrator (5 `uses:` jobs, zero `needs:`, zero inline steps); header/triggers/permissions/concurrency unchanged.
- `.github/workflows/_actionlint.yml`, `_format-check.yml`, `_build-analyzers.yml`, `_build-nullable.yml`, `_mstest-coverage.yml`: new callee reusable workflows; all gate commands transplanted byte-identically; per-job tailored setup; one new plain `Build solution` step in the MSTest callee (assessed gate-neutral, section 7.2).
- `.github/workflows/README.md`: new; documents pipeline topology, per-stage dispatch, and branch-protection rename procedures (section 7.5).
- Feature folder `docs/features/active/2026-08-14-ci-parallel-job-split-553/`: issue/spec/user-story/plan/research plus baseline, pre-split reference, and QA-gate evidence.
- `docs/features/potential/promoted/`: two archival copies recording issues #554 and #555 (latent defects found during this feature's orchestration; documentation only).
- `.claude/agent-memory/`: routine agent memory updates (6 files).

## 10. Compliance Verdict

**FAIL (blocking) — one finding.** The change set itself is compliant in every inspected dimension (structure, byte-identity, lint, file sizes, evidence locations, both governing CI rules, documentation obligations), and no acceptance criterion regressed. The single blocking finding is the unconditional `modified-workflow-needs-green-run` rule: workflow files changed and no green run against head `0b016c81` exists yet. This is expected at this stage of the plan of record — the remaining phases require a live PR — and is routed through `remediation-inputs.2026-08-14T10-21.md` for the standard handoff. Blocking finding count: **1**.

## Appendix A: Test Inventory

No test files were added, removed, or modified on this branch. The CI test gate (`_mstest-coverage.yml`) executes the pre-existing MSTest suite with an unchanged vstest invocation, unchanged discovery filter (`\bin\Debug\`, excluding `\obj\` and `\ref\`), unchanged `TestCategory!=LiveOutlook` filter, and unchanged zero-assembly `throw` guard.

## Appendix B: Toolchain Commands Reference

Commands executed by this review (check-only; no mutation of source or policy files):

1. `git rev-parse HEAD` / `git merge-base HEAD origin/main` — base/head resolution (recomputed, not trusted from caller).
2. `git diff --name-status 2073f717bbfac30053f3d6a4e652d99af3ae5c9c..HEAD` and extension-filtered `git diff --name-only` — full-diff scope and changed-language enumeration.
3. `git show 2073f717:.github/workflows/ci.yml` + Python block-extraction/SHA-256 comparison script (scratchpad) — independent byte-identity verification, 14/14 MATCH.
4. `grep -n "needs:\|concurrency" .github/workflows/_*.yml` — structural invariants (zero matches).
5. `grep -c ""` per workflow file — 500-line-limit audit.
6. `grep -rn "download-artifact\|upload-artifact" .github/workflows/` — cross-job file-sharing audit.
7. Evidence-location filter: `git diff --name-only <merge-base>..HEAD | grep -E '^artifacts/(baselines|qa|evidence|coverage)/'` — zero matches.

Checks verified from committed executor evidence rather than re-run: actionlint pre/post (exit 0; the actionlint binary is not installed on this host and CI re-runs it authoritatively on the branch head).
