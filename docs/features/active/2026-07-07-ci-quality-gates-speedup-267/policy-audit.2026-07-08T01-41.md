# Policy Compliance Audit: ci-quality-gates-speedup (Issue #267)

**Audit Date:** 2026-07-08
**Reviewer:** feature-review agent (structure normalized to canonical template by orchestrator; findings and verdict unchanged)
**Work Mode:** minor-audit
**Branch:** `refactor/ci-quality-gates-speedup-267` @ `7ffc96cc67e85983d6034632d4fd1fd466deda5c`
**Base:** `main` (`origin/main`) @ `5c4bf31e25210eb850827f2668c74cd72d5fa231`
**Diff range:** `5c4bf31e25210eb850827f2668c74cd72d5fa231..7ffc96cc67e85983d6034632d4fd1fd466deda5c`
**AC source:** `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/issue.md`, `## Acceptance Criteria` (AC1–AC6)

**Code Under Test:**
- `.github/workflows/ci.yml` (modified — NuGet cache step, dotnet-tools cache step, `/m` added to both build passes) (+18/-2)
- 21 markdown files (feature docs and evidence). No `.cs`, `.ts`/`.tsx`, `.py`, or `.ps1`/`.psm1` file is touched.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 0 | N/A | N/A | N/A — 0 changed `.cs` files | N/A — 0 changed `.cs` files | N/A — 0 changed `.cs` files |
| TypeScript | 0 | N/A | N/A | N/A — out of scope | N/A — out of scope | N/A — out of scope |
| Python | 0 | N/A | N/A | N/A — out of scope | N/A — out of scope | N/A — out of scope |
| PowerShell | 0 | N/A | N/A | N/A — out of scope | N/A — out of scope | N/A — out of scope |

**Coverage Evidence Checklist**

- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- C# baseline coverage artifact: `N/A - zero changed .cs files on this branch`
- C# post-change coverage artifact: `N/A - zero changed .cs files on this branch`
- Per-language comparison summary: see § 1.2.1

**Fail-closed rule acknowledgment:** This branch changes only `.github/workflows/ci.yml` plus documentation/evidence markdown. Zero source or test files exist in the diff for any language, so no coverage artifact is required or applicable. This is a true zero-changed-files condition, not a scope narrowing.

---

## Executive Summary

The branch changes exactly one production file, `.github/workflows/ci.yml` (+18/-2), plus 21 markdown files (feature docs and evidence) — no `.ts`, `.py`, `.ps1`/`.psm1`, or `.cs` file is touched. AC1–AC5 are implemented, independently re-verified where practical (actionlint re-run locally: exit 0, zero findings), and are well supported by detailed, internally consistent evidence artifacts. AC6 (the `modified-workflow-needs-green-run` gate: a green CI run against the branch head) is **not satisfied** — no workflow-run evidence exists anywhere in the repository or working tree, and `gh` is unavailable in this review environment to check GitHub Actions state. This is a Blocking finding per the `modified-workflow-needs-green-run` policy rule and routes to the orchestrator's post-PR CI-green gate. No other Blocking or FAIL findings were identified.

**Policy documents evaluated:**
- [✅] `general-code-change` (`.claude/rules/general-code-change.md`)
- [✅] `general-unit-test` (`.claude/rules/general-unit-test.md`)
- [✅] `ci-workflows` (`.claude/rules/ci-workflows.md`)
- [✅] `benchmark-baselines` (`.claude/rules/benchmark-baselines.md`)

**Rejected Scope Narrowing:** No caller-supplied narrowing of audit scope was attempted. The full branch-vs-`main` diff (22 changed files) was reviewed in its entirety. The plan and issue statements that no `.cs`/`.csproj`/`packages.config`/`dotnet-tools.json`/`global.json` file is touched are factual scope statements (confirmed by `git diff --name-only`), not narrowing attempts: a true statement that a language has zero changed files is not scope narrowing under the Scope Invariant.

**Evidence Location Compliance:** PASS. All evidence added by this feature is written under the canonical `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/<kind>/` path. No file was written to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or `artifacts/evidence/`. A diff-name scan for `^artifacts/(baselines|qa|evidence|coverage)/` returns no matches.

**Temporary artifacts cleanup:**
- [✅] No review-only scripts were created or retained.

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| Independence | [N/A] | No test files are added, modified, or in scope. Zero `.cs`/`.py`/`.ts`/`.ps1` test files in the diff. |
| Isolation | [N/A] | Same — no test files in scope. |
| Fast Execution | [N/A] | Same — no test files in scope. |
| Determinism | [N/A] | Same — no test files in scope. |
| Readability & Maintainability | [N/A] | Same — no test files in scope. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| Baseline Coverage Documented | [N/A] | CI-configuration-only change; zero source/test files in the diff, so no coverage denominator changes. |
| No Coverage Regression | [N/A] | No changed lines of instrumented source; coverage is unaffected by this diff. |
| New Code Coverage ≥90% | [N/A] | No new executable source code introduced. |

### 1.2.1 Per-Language Coverage Comparison

- TypeScript: N/A - out of scope (0 changed `.ts`/`.tsx` files).
- PowerShell: N/A - out of scope (0 changed `.ps1`/`.psm1` files; the embedded `pwsh` blocks inside `ci.yml` `run:` steps are workflow YAML content, evaluated under `.claude/rules/ci-workflows.md` in § 7, not the PowerShell coverage policy).
- Python: N/A - out of scope (0 changed `.py` files).
- C#: Baseline: N/A (0 changed `.cs` files) -> Post-change: N/A (0 changed `.cs` files). Change: +0% (no instrumented source changed). New/changed-code coverage: N/A. Disposition: N/A — zero changed C# files on this branch. Evidence: `git diff --name-only 5c4bf31e..7ffc96cc` extension breakdown (21 `.md`, 1 `.yml`).

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| Clear Failure Messages | [N/A] | No tests in scope. |
| Arrange-Act-Assert Pattern | [N/A] | No tests in scope. |
| Document Intent | [N/A] | No tests in scope. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| Avoid External Dependencies | [N/A] | No tests in scope. |
| Use Mocks/Stubs | [N/A] | No tests in scope. |
| Environment Stability | [N/A] | No tests in scope. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| Pre-submission Review | [✅] [PASS] | This document is the required pre-submission policy audit for issue #267. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| Clarify the objective | [✅] [PASS] | Objective captured in `issue.md`: reduce CI wall-clock time via caching, `/m`, and (dropped) consolidation. |
| Read existing change plans | [✅] [PASS] | Authoritative plan reviewed at `plan.2026-07-07T20-45.md` (Version 1.1, Option A). |
| Document the plan | [✅] [PASS] | Feature folder contains the completed minor-audit plan plus Phase 0/Phase 2 evidence. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| Simplicity first | [✅] [PASS] | Additive-only cache steps plus one flag (`/m`) per existing step; no restructuring beyond the documented Scope Decision revert. |
| Reusability | [✅] [PASS] | Uses the standard `actions/cache@v4` action rather than a bespoke caching script. |
| Extensibility | [N/A] | CI/YAML configuration change; no public API surface. |
| Separation of concerns | [N/A] | No application code touched. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| Cohesive modules | [✅] [PASS] | Change confined to the `quality-gates` job of one workflow file. |
| Under 500 lines | [✅] [PASS] | `.github/workflows/ci.yml` is 153 lines post-change (`wc -l`, independently verified). No changed file exceeds 500 lines. |
| Public vs internal | [N/A] | No API surface in scope. |
| No circular dependencies | [N/A] | No code dependencies changed. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| Descriptive names | [✅] [PASS] | New step names ("Cache NuGet packages", "Cache dotnet tools") are descriptive. |
| Docs/docstrings | [✅] [PASS] | The Scope Decision and evidence artifacts document the rationale, including the dropped consolidation. |
| Comment why, not what | [✅] [PASS] | The retained-two-pass decision and its rationale are documented in `issue.md` and the plan. |

### 2.5 After Making Changes — Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| 1. Formatting | [N/A] | No `.cs` files changed; CSharpier not applicable to this diff's own content. YAML lint via actionlint (§ 7). |
| 2. Linting | [✅] [PASS] | `actionlint` re-run independently on the modified workflow: exit 0, zero findings. |
| 3. Type checking | [N/A] | No source code changed. |
| 4. Testing | [N/A] | No test files changed. |
| Full toolchain loop | [✅] [PASS] | Applicable gate (actionlint) passed; build passes verified from executor evidence (§ 7). |
| Explicit reporting | [✅] [PASS] | Commands and outcomes recorded in this audit and the feature evidence artifacts. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| Summarize changes | [✅] [PASS] | This audit and the feature evidence summarize the caching, `/m`, and retained-two-pass change. |
| Design choices explained | [✅] [PASS] | The Scope Decision explains why consolidation was dropped. |
| Update supporting documents | [✅] [PASS] | `issue.md`, plan, and the evidence bundle were updated. |
| Provide next steps | [✅] [PASS] | AC6 is recorded as the out-of-band post-PR CI-green gate. |

---

## 3. Language-Specific Code Change Policy Compliance

| Language | Files changed | Verdict | Rationale |
|---|---|---|---|
| C# (`.cs`, `.csproj`) | 0 | N/A | Zero changed files of this type on the branch. `.claude/rules/csharp.md` code-change requirements do not apply. |
| TypeScript (`.ts`/`.tsx`) | 0 | N/A | Zero changed files of this type on the branch. |
| Python (`.py`) | 0 | N/A | Zero changed files of this type on the branch. |
| PowerShell (`.ps1`/`.psm1`) | 0 | N/A | Zero standalone PowerShell files. The `pwsh` blocks inside `ci.yml` `run:` steps are workflow YAML, evaluated under `.claude/rules/ci-workflows.md` (§ 7). |
| YAML (`.github/workflows/ci.yml`) | 1 | PASS (with one Blocking sub-finding, § 8) | Governed by `.claude/rules/ci-workflows.md` and `modified-workflow-needs-green-run`. |

Per the Coverage Verification instructions, `N/A` is acceptable here specifically because each language has zero changed files — this is not scope narrowing.

---

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Status | Evidence |
|------------|--------|----------|
| Use MSTest (C#) | [N/A] | No C# test files changed. |
| Use Moq (C#) | [N/A] | No C# test files changed. |
| Prefer FluentAssertions (C#) | [N/A] | No C# test files changed. |
| Framework selection (other languages) | [N/A] | No TypeScript/Python/PowerShell test files changed. |

No language-specific unit-test policy applies because the diff contains zero test files of any language.

---

## 5. Test Coverage Detail

No tests were added, modified, or exercised by this change. The diff contains zero source and zero test files of any instrumented language (21 `.md`, 1 `.yml`). There is therefore no per-test coverage detail to report. Coverage for the repository is unaffected because no instrumented line is added, removed, or modified.

**Not covered:** Not applicable — no executable code introduced.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 0 changed / N/A | [N/A] No test files in scope |
| Tests Passed | N/A | [N/A] |
| Tests Failed | N/A | [N/A] |
| Execution Time | N/A | [N/A] |
| Code Coverage (if applicable) | N/A — no instrumented source changed | [N/A] |

The applicable verification for this change is static (`actionlint`) and build-gate (two `msbuild` passes), reported in § 7, not the MSTest suite.

---

## 7. Code Quality Checks

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| actionlint (independent re-run) | `./actionlint-bin/actionlint.exe .github/workflows/ci.yml` | Exit 0, zero findings | [✅] |
| actionlint (executor evidence) | `pwsh -File scripts\dev-tools\run-actionlint.ps1` | Exit 0 (per `evidence/qa-gates/actionlint-final.2026-07-07T22-00.md`) | [✅] |
| msbuild pass 1 (executor evidence) | `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Exit 0, 72 warnings, 0 errors | [✅] |
| msbuild pass 2 (executor evidence) | `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` | Exit 0, 0 warnings, 0 errors | [✅] |
| File-size check | `wc -l .github/workflows/ci.yml` | 153 lines (< 500) | [✅] |
| Diff scope check | `git diff --name-only 5c4bf31e..7ffc96cc` | 22 files: 21 `.md`, 1 `.yml` | [✅] |

### 7.1 CI Workflow Content Correctness (AC1–AC5)

- **AC1 (NuGet cache):** `Cache NuGet packages` step (`actions/cache@v4`, `path: packages`, `key: nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}`, `restore-keys:` fallback) inserted between "Setup NuGet" and "Restore solution"; "Restore solution" carries no `if:` guard, so it runs on cache hit and miss. **PASS.**
- **AC2 (CSharpier tool cache):** `Cache dotnet tools` step (`path: ~/.nuget/packages`, `key: dotnet-tools-${{ runner.os }}-${{ hashFiles('dotnet-tools.json') }}`) inserted before "Setup CSharpier"; `dotnet tool restore` carries no `if:` guard. **PASS.**
- **AC3 (`/m`):** Exactly two `msbuild ... /t:Build` invocations post-change (`grep -c "/t:Build"` = 2); both carry `/m` (lines 98 and 106). **PASS.**
- **AC4 (both enforcement passes preserved, no reduction):** The two original steps are retained as two separate steps per the documented Scope Decision. Each property set is unchanged except for the added `/m`; `evidence/qa-gates/build-diagnostic-parity.2026-07-07T22-00.md` finds no enforced diagnostic dropped (pass 1: 33→72 warnings, incremental-state variance not a property change; pass 2: 0/0 in both). **PASS** (by evidence inspection; the full-solution passes were not re-run in review due to build-time cost — § 8 rationale).
- **AC5 (`actionlint`):** **PASS**, independently re-verified (exit 0, zero findings).

### 7.2 `.claude/rules/ci-workflows.md` — Deliberately-failing nested command pattern

**PASS.** No `pwsh` step intentionally invokes a failing nested command. Both modified build steps retain their pre-existing `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` guard.

### 7.3 `.claude/rules/benchmark-baselines.md`

**N/A.** No path under `scripts/benchmarks/**` appears in the diff.

---

## 8. Gaps and Exceptions

### Identified Gaps

**One Blocking gap — `modified-workflow-needs-green-run` (AC6).** The diff modifies `.github/workflows/ci.yml` (matches `.github/workflows/**`), so this rule fires. No qualifying evidence of a green workflow run against the branch head (`7ffc96cc`) exists:
- `gh` was unavailable in the review environment (per `artifacts/pr_context.summary.txt`).
- No workflow-run artifact, status file, or `workflow_dispatch` receipt exists under the feature folder or `artifacts/`.
- `issue.md` AC6 is correctly left unchecked (`- [ ] AC6`); the evidence records AC6 as an intentionally out-of-band, not-yet-satisfied gate rather than claiming false completion.

This is a process/evidence gap, not a defect in the workflow content (§ 7.1–7.3 all pass). It is resolved by the orchestrator's post-PR CI-green gate (the branch must be pushed and CI run green against the head SHA); no source-code change is required. Routed via `remediation-inputs.2026-07-08T01-41.md`.

### Non-Blocking Observations

- **Low:** The `Cache dotnet tools` step caches the broad `~/.nuget/packages` path rather than a tool-scoped path. Functional and safe; a narrower key could be considered later.
- **Informational (pre-existing, unaffected by this diff):** `dotnet-tools.json` lives at repo root rather than the conventional `.config/dotnet-tools.json`. `hashFiles('dotnet-tools.json')` correctly targets the actual location.

### Approved Exceptions

**None.**

### Removed/Skipped Tests

**None.**

---

## 9. Summary of Changes

### Commits in This Branch

Single commit `7ffc96cc` ("ci(quality-gates): cache dependencies and parallelize builds") on `refactor/ci-quality-gates-speedup-267`, rebased onto `origin/main` @ `5c4bf31e`.

### Files Modified

1. **`.github/workflows/ci.yml`** (MODIFIED, +18/-2) — Adds a NuGet `packages/` cache step before restore, a dotnet-tools cache step before CSharpier restore, and `/m` on both existing `msbuild /t:Build` passes. Two separate build passes retained (consolidation dropped per Scope Decision).
2. **21 markdown files** (ADDED) — feature-folder lifecycle docs (issue.md, plan) and evidence artifacts, plus the promoted potential doc and a follow-up potential bug entry (`2026-07-07-ci-nullable-check-skipped-vendored-projects.md`).

---

## 10. Compliance Verdict

### Overall Status: ⚠️ PARTIAL — one Blocking finding

| Area | Verdict |
|---|---|
| General Code Change Policy | PASS |
| General Unit Test Policy | N/A (no test files in scope) |
| Language-Specific Code Change Policy (C#/TS/Python/PowerShell) | N/A (zero changed files each) |
| Coverage Verification (all four languages) | N/A (zero changed files each) |
| CI Workflow Policy — `ci-workflows.md` | PASS |
| CI Workflow Policy — `benchmark-baselines.md` | N/A |
| CI Workflow Policy — `modified-workflow-needs-green-run` | **FAIL — Blocking** |
| Evidence Location Compliance | PASS |
| **Overall** | **PARTIAL — one Blocking finding (AC6 / modified-workflow-needs-green-run); resolved by the post-PR CI-green gate** |

### Recommendation

**No-go for merge until AC6 is satisfied** by a green CI run against the current branch head, recorded as evidence. All other acceptance criteria and policy checks pass. AC1–AC5 are delivered and independently verified where practical.

---

## Appendix A: Test Inventory

### Complete Test List

**None.** This change adds, modifies, and exercises zero test files of any language. The applicable verification is static (`actionlint`) and build-gate (`msbuild` two-pass), documented in § 7 and Appendix B.

---

## Appendix B: Toolchain Commands Reference

```bash
# Workflow static lint (independent re-run)
./actionlint-bin/actionlint.exe .github/workflows/ci.yml

# Workflow static lint (executor evidence)
pwsh -File scripts\dev-tools\run-actionlint.ps1

# Build pass 1 — analyzers + code style (executor evidence)
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

# Build pass 2 — nullable + warnings-as-errors (executor evidence)
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true

# Diff scope
git diff --name-only 5c4bf31e25210eb850827f2668c74cd72d5fa231..7ffc96cc67e85983d6034632d4fd1fd466deda5c
```

---

**Audit Completed By:** feature-review agent (canonical-template normalization by orchestrator; verdict unchanged)
**Audit Date:** 2026-07-08
**Policy Version:** Current (as of audit date)
