# Policy Compliance Audit: Hierarchical LCPPN Folder Prediction — Cycle 4 Exit Re-audit (#177)

**Audit Date:** 2026-06-16
**Code Under Test:** Cycle 4 produced zero production/test source diff. Full branch diff vs `main` (merge-base `c12aaf1c`) is C# only (30 `.cs` files). Cycle-4 changes are documentation/evidence only: `user-story.md` (AC25 disposition), `.claude/agent-memory/orchestrator/MEMORY.md`, and new files under the feature folder `evidence/` and `artifacts/research/`.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 0 files (cycle 4); 30 files (full branch) | 3912 tests | ✅ 3912 pass, 0 fail | 84.62% lines (FilePathHelper.cs class line-rate) | 84.62% lines (unchanged; no code diff) | N/A (no new/changed C# code in cycle 4) |
| PowerShell | 0 files | N/A | N/A | N/A - out of scope | N/A - out of scope | N/A - out of scope |
| Python | 0 files | N/A | N/A | N/A - out of scope | N/A - out of scope | N/A - out of scope |
| TypeScript | 0 files | N/A | N/A | N/A - out of scope | N/A - out of scope | N/A - out of scope |

**Note:** C# is the only language with changed files in the branch diff. PowerShell, Python, and TypeScript have zero changed files and are out of scope (legitimate N/A).

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- C# coverage artifact (of record): `artifacts/csharp/coverage.xml` (cycle-3 run dated 2026-06-12; unchanged in cycle 4 because no C# code changed)
- Per-language comparison summary: see Section 1.2.1 below.

**Non-negotiable verdict rule:** This audit reports numeric C# baseline and post-change coverage; PowerShell/Python/TypeScript are N/A because they have zero changed files in the branch diff.

**Fail-closed rule:** No required baseline, QA, or coverage-comparison artifact is missing. The cycle-4 Phase 0 baseline evidence set is present and green.

---

## Executive Summary

Cycle 4 was opened to fix a previously-reported latent `FilePathHelper` Json.NET deserialize `NullReferenceException` (AC25). The investigation concluded the defect is not reproducible on HEAD and made no production or test source change. This re-audit confirms the no-fix-required close is sound and nothing regressed.

The audit scope is the full branch diff against the resolved base `main` (merge-base `c12aaf1c`), not the cycle-4 task scope. The branch diff is C# only (30 `.cs` files). Cycle 4 itself produced zero source diff, verified against the cycle-3 exit commit `ac3d6b53`:
- `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs` — empty diff vs `ac3d6b53`.
- `UtilitiesCS.Test/HelperClasses/FilePathHelper_Tests.cs` — empty diff vs `ac3d6b53`.
- Working tree contains no `.cs`/`.ps1`/`.ts`/`.py` changes; only `user-story.md` (+18 lines AC25), orchestrator memory, and documentation/evidence.

The AC25 deserialize-safety conclusion is corroborated by source inspection: `StemInitialized()`/`TryParseFileName()` self-heal the stem fields before the `AdjustForMaxPath()` dereference (`FilePathHelper.cs:183-308`), and `DoNotSerializeContractResolver("Config")` excludes `Config`/`Disk` from the LCPPN load path (`LcppnFolderPredictorStore.cs:63`). Either mechanism alone prevents the NRE.

**Policy documents evaluated:**
- ✅ `general-code-change` (CLAUDE.md + `.claude/rules/general-code-change.md`)
- ✅ `general-unit-test` (CLAUDE.md + `.claude/rules/general-unit-test.md`)

**Language-specific policies evaluated:**
- ✅ C#: `.claude/rules/csharp.md`, CLAUDE.md C# Code Change Policy + C# Unit Test Policy
- N/A `python` (zero changed Python files)
- N/A `powershell` (zero changed PowerShell files)
- N/A `typescript` (zero changed TypeScript files)

The cycle-4 Phase 0 baseline recorded a green toolchain (csharpier exit 0; analyzers 0W/0E; nullable/TWAE 0W/0E; tests 3912/3912) and AC23 tests 10/10 with the Config exclusion present. No code changed, so the gate result carries forward unchanged.

**Temporary artifacts cleanup:**
- ✅ No temporary/one-time scripts were created during cycle 4 (documentation/investigation only).
- ✅ No ongoing tooling scripts were added.
- The cycle-4 AC25 deserialize probe was an investigation step recorded in `artifacts/research/2026-06-16-lcppn-deserialize-nre-research.md`; it produced no committed code.

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | ✅ PASS | No test changed in cycle 4. Baseline full-suite run 3912/3912 passing (`phase0-tests-coverage.2026-06-16T10-26.md`). |
| **Isolation** - Each test targets single behavior | ✅ PASS | Existing MSTest suite unchanged; runs with `/InIsolation` per documented Moq assembly-load guidance. |
| **Fast Execution** - Tests complete quickly | ✅ PASS | Full UtilitiesCS.Test assembly ran to completion at baseline (exit 0). |
| **Determinism** - Consistent results | ✅ PASS | No randomness/time/IO added; no temp files. AC25 probe was not committed as a test. |
| **Readability & Maintainability** - Clear structure | ✅ PASS | No test source changed; cycle-3 structure preserved. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | ✅ PASS | **Baseline:** `FilePathHelper.cs` class line-rate 84.62% (542/638 lines per cobertura parse).<br>**Command:** `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /EnableCodeCoverage`<br>**Timestamp:** 2026-06-16 10:26<br>Source: `phase0-tests-coverage.2026-06-16T10-26.md`. |
| **No Coverage Regression** | ✅ PASS | **Post-change coverage:** 84.62% lines (unchanged).<br>**Change:** 0.00% lines.<br>**Status:** No regression — zero code diff in cycle 4. Baseline 84.62% → Post-change 84.62% (+0.00%) PASS. |
| **New Code Coverage ≥90%** | ✅ PASS | **New/modified files:** none in cycle 4.<br>**New code coverage:** N/A (no new/changed C# code; vacuously satisfied).<br>**Calculation method:** `git diff ac3d6b53` shows no `.cs` files changed. |
| **Comprehensive Coverage** | ✅ PASS | Existing FilePathHelper tests (31/31) and AC23 suite (10/10) unchanged and green; engine coverage carried forward from cycle-3 exit. |
| **Positive Flows** - Valid inputs | ✅ PASS | Covered by the unchanged existing suite (no scenario delta in cycle 4). |
| **Negative Flows** - Invalid inputs | ✅ PASS | Covered by the unchanged existing suite. |
| **Edge Cases** - Boundary conditions | ✅ PASS | `AdjustForMaxPath()` MAX_PATH boundary and uninitialized-stem path covered by existing FilePathHelper tests. |
| **Error Handling** - Error paths | ✅ PASS | AC23 fail-soft tests (missing/unreadable file, unresolved AppData) unchanged and green. |
| **Concurrency** - If applicable | N/A | No concurrency surface changed in cycle 4. |
| **State Transitions** - If applicable | N/A | No state-machine code changed in cycle 4. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 84.62% lines (FilePathHelper.cs class line-rate, cobertura parse) -> Post-change: 84.62% lines. Change: +0.00% lines. New/changed-code coverage: N/A - no new/changed C# code in cycle 4. Disposition: PASS. Evidence: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/phase0-tests-coverage.2026-06-16T10-26.md`, `artifacts/csharp/coverage.xml`.
- PowerShell: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (zero changed PowerShell files).
- Python: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (zero changed Python files).
- TypeScript: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (zero changed TypeScript files).

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | ✅ PASS | Existing FluentAssertions-based assertions unchanged. |
| **Arrange-Act-Assert Pattern** | ✅ PASS | Existing tests follow AAA; no test added. |
| **Document Intent** | ✅ PASS | Existing descriptive test names retained. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | ✅ PASS | No new dependency; unit tests mock external boundaries via Moq. |
| **Use Mocks/Stubs** | ✅ PASS | Unchanged mocking strategy. |
| **Environment Stability** | ✅ PASS | No temp files; no mutable global state introduced. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | ✅ PASS | This document is the cycle-4 exit policy review. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | ✅ PASS | Cycle-4 objective (AC25 deserialize NRE) was investigated; recorded in `artifacts/research/2026-06-16-lcppn-deserialize-nre-research.md`. |
| **Read existing change plans** | ✅ PASS | Cycle-4 Phase 0 read instructions/baseline (`phase0-instructions-read.2026-06-16T10-26.md`). |
| **Document the plan** | ✅ PASS | Cycle-4 remediation folder `2026-06-16T10-26-remediation/` documents the disposition. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | ✅ PASS | No code added; the simplest correct outcome (no-fix) was chosen after the throw was shown unreachable. |
| **Reusability** | N/A | No code changed. |
| **Extensibility** | N/A | No code changed. |
| **Separation of concerns** | ✅ PASS | Config exclusion keeps serialization concern isolated from `FilePathHelper`; unchanged. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | ✅ PASS | No module changed. |
| **Under 500 lines** | ✅ PASS | No source file added or grown in cycle 4. `FilePathHelper.cs` unchanged. |
| **Public vs internal** | ✅ PASS | Public surface unchanged. |
| **No circular dependencies** | ✅ PASS | Dependency graph unchanged. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | ✅ PASS | No code renamed. |
| **Docs/docstrings** | ✅ PASS | AC25 disposition documented in `user-story.md` and research artifact. |
| **Comment why, not what** | N/A | No code comments changed. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ✅ PASS | **Command:** `csharpier check .`<br>**Result:** Checked 1080 files, exit 0 (`phase0-csharpier.2026-06-16T10-26.md`). |
| **2. Linting** | ✅ PASS | **Command:** `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`<br>**Result:** 0 Warning(s), 0 Error(s) (`phase0-analyzers.2026-06-16T10-26.md`). |
| **3. Type checking** | ✅ PASS | **Command:** `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`<br>**Result:** 0 Warning(s), 0 Error(s) (`phase0-nullable.2026-06-16T10-26.md`). |
| **4. Testing** | ✅ PASS | **Command:** `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /EnableCodeCoverage`<br>**Result:** 3912/3912 pass, exit 0 (`phase0-tests-coverage.2026-06-16T10-26.md`). |
| **Full toolchain loop** | ✅ PASS | All four steps green at the cycle-4 baseline; no code change means the result is the final pass. |
| **Explicit reporting** | ✅ PASS | Commands and results recorded in `evidence/baseline/phase0-*.2026-06-16T10-26.md`. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | ✅ PASS | Cycle 4 = no code change; AC25 disposition added to `user-story.md`. |
| **Design choices explained** | ✅ PASS | Research artifact explains why a null-guard would be unfalsifiable defensive hardening. |
| **Update supporting documents** | ✅ PASS | `user-story.md` AC25 updated; orchestrator memory updated. |
| **Provide next steps** | ✅ PASS | Next step: PR with the unchanged C# delivery; CI is the repo-wide coverage gate of record. |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3 (C#): C# Code Change Policy Compliance

#### 3.1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with CSharpier** | ✅ PASS | `csharpier check .` exit 0, 1080 files (`phase0-csharpier.2026-06-16T10-26.md`). |
| **Linting with .NET Analyzers** | ✅ PASS | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` 0W/0E. |
| **Type checking — Nullable/TWAE** | ✅ PASS | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` 0W/0E. |
| **Testing with MSTest** | ✅ PASS | `vstest.console.exe ... /InIsolation /EnableCodeCoverage` 3912/3912. |

#### 3.2 C# Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Strong contracts / explicit APIs** | ✅ PASS | No API changed; `StemInitialized()` invariant intact (`FilePathHelper.cs:183-191`). |
| **Null-safety by default** | ✅ PASS | Nullable build clean; `AdjustForMaxPath()` self-heal prevents null deref. |
| **Composition / focused types** | ✅ PASS | No type changed. |
| **Async/resource safety** | N/A | No async/disposable code changed. |

#### 3.3 Error Handling, Logging, Contracts (C#)

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Exceptions / fail fast** | ✅ PASS | `AdjustForMaxPath()` returns false (not throw) on uninitialized stem; unchanged. |
| **Logging pattern** | N/A | No logging code changed. |
| **Contracts / invariants** | ✅ PASS | Config exclusion (`LcppnFolderPredictorStore.cs:63`) retained as INV-1. |

---

## 4. Language-Specific Unit Test Policy Compliance

### Section 4 (C#): C# Unit Test Policy Compliance

#### 4.1 Framework and Scope

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use MSTest** | ✅ PASS | Existing suite uses MSTest; no framework change. |
| **Coverage expectation** | ✅ PASS | No new code in cycle 4; FilePathHelper baseline 84.62% unchanged. Repo-wide gate of record is the PR CI run. |

#### 4.2 Test Style and Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Focused unit tests** | ✅ PASS | No test changed. |
| **Moq / FluentAssertions** | ✅ PASS | Existing tests use Moq + FluentAssertions. |
| **Organization** | ✅ PASS | Test layout mirrors code; unchanged. |

#### 4.3 Naming and Readability

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Naming conventions** | ✅ PASS | Existing descriptive names retained. |
| **Docstrings/comments** | ✅ PASS | Unchanged. |

#### 4.4 Running the Toolchain

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use vstest** | ✅ PASS | `vstest.console.exe ... /InIsolation` 3912/3912 (`phase0-tests-coverage.2026-06-16T10-26.md`). |
| **No alternative runners** | ✅ PASS | Only MSTest via vstest used. |

---

## 5. Test Coverage Detail

### FilePathHelper.cs (existing suite — 31 tests; AC23 suite — 10 tests)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| FilePathHelper_Tests (full set) | Positive/Negative/Edge | FilePathHelper.cs 542/638 (84.62% class line-rate) | ✅ |
| RoundTrip_WithDedicatedConfig_PreservesContentAndFileName | Positive (serialization) | LcppnFolderPredictorStore serialize/deserialize | ✅ |
| LoadFolderPredictorAsync_SettingOnButFileMissing_LeavesHolderNull | Error handling (fail-soft) | load path | ✅ |

**Coverage:** FilePathHelper.cs 84.62% class line-rate (baseline, unchanged in cycle 4).

**Not covered:** The residual ~15% of FilePathHelper.cs lines are unchanged from baseline; no new code introduced in cycle 4, so no new coverage obligation arises. The `AdjustForMaxPath()` null-guard was not added because the throw is structurally unreachable (no falsifiable test).

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 3912 (full UtilitiesCS.Test) | ✅ |
| Tests Passed | 3912 (100%) | ✅ |
| Tests Failed | 0 | ✅ |
| FilePathHelper_Tests | 31/31 pass | ✅ |
| AC23 suite | 10/10 pass | ✅ |
| Functions/Classes Tested | Unchanged from cycle-3 exit | ✅ |
| Code Coverage (FilePathHelper.cs) | 84.62% lines | ✅ |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `csharpier check .` | Clean, 1080 files, exit 0 | ✅ |
| .NET Analyzers | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 errors, exit 0 | ✅ |
| Nullable / TWAE | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 warnings/errors, exit 0 | ✅ |
| MSTest via vstest | `vstest.console.exe ... /InIsolation /EnableCodeCoverage` | 3912 pass / 0 fail, exit 0 | ✅ |

**Notes:** Cycle 4 changed no C# source, so the cycle-4 Phase 0 baseline is the final toolchain pass. The C# coverage artifact `artifacts/csharp/coverage.xml` (cycle-3 run, 2026-06-12) is unchanged because no C# code changed. The repo-wide C# coverage gate of record is the PR CI run; local full-assembly Cobertura is constrained per documented agent memory (Moq binding redirect on full-suite coverage).

---

## 8. Gaps and Exceptions

### Identified Gaps
**None.** All policy requirements are met. Cycle 4 introduced no code change.

### Approved Exceptions
- AC25 null-guard not added: the deserialize NRE is structurally unreachable on HEAD, so a red-before-green regression test is not achievable. Per the repository bugfix discipline (failing test required before a fix), no production change was made. Documented in `artifacts/research/2026-06-16-lcppn-deserialize-nre-research.md` and `user-story.md` AC25.

### Removed/Skipped Tests
**None.** No test was removed or skipped.

---

## 9. Summary of Changes

### Commits in This PR/Branch
Cycle 4 added no commits. HEAD remains the cycle-3 exit commit:
1. **ac3d6b53** — docs(review): cycle-3 exit reaudit artifacts for #177 (blocking_count=0, gate clean)

### Files Modified (cycle 4, documentation/evidence only)
1. **docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/user-story.md** (MODIFIED) — added AC25 disposition (+18 lines).
2. **.claude/agent-memory/orchestrator/MEMORY.md** (MODIFIED) — orchestrator memory pointer.
3. **artifacts/research/2026-06-16-lcppn-deserialize-nre-research.md** (NEW) — AC25 non-reproducibility investigation.
4. **docs/features/active/.../evidence/baseline/phase0-*.2026-06-16T10-26.md**, **.../evidence/regression-testing/fail-before-exception.2026-06-16T10-26.md**, **.../2026-06-16T10-26-remediation/** (NEW) — cycle-4 baseline and disposition evidence.

No production or test `.cs` files changed.

---

## Evidence Location Compliance

`git diff --name-only` of the working tree and `ac3d6b53` was scanned for files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. None were found. The cycle-4 evidence is written to the canonical `<FEATURE>/evidence/<kind>/` location (`evidence/baseline/`, `evidence/regression-testing/`). No evidence-location violations.

---

## 10. Compliance Verdict

### Overall Status: ✅ FULLY COMPLIANT

Cycle 4 closed AC25 as no-fix-required after establishing the deserialize NRE is not reproducible on HEAD. Zero production/test source diff was confirmed against `ac3d6b53`. The full C# toolchain is green at the cycle-4 baseline, AC1–AC24 are unchanged and not regressed, and AC25 is satisfied on HEAD. No evidence-location violations.

**Fail-closed reminder:** No required artifact is missing; the verdict is supported by the present cycle-4 Phase 0 evidence and direct source/diff inspection.

---

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- ✅ Before Making Changes: investigation documented
- ✅ Design Principles: simplest correct outcome (no-fix)
- ✅ Module & File Structure: no file changed
- ✅ Naming, Docs, Comments: AC25 documented
- ✅ Toolchain Execution: all four steps green
- ✅ Summarize & Document: user-story + research updated

#### Language-Specific Code Change Policy (Section 3)
**For C#:**
- ✅ Tooling & Baseline: csharpier/analyzers/nullable/tests green
- ✅ C# Design & Type-Safety: invariants intact
- ✅ Error Handling: Config exclusion retained

#### General Unit Test Policy (Section 1)
- ✅ Core Principles
- ✅ Coverage & Scenarios (no regression; no new code)
- ✅ Test Structure
- ✅ External Dependencies
- ✅ Policy Audit

#### Language-Specific Unit Test Policy (Section 4)
**For C#:**
- ✅ Framework & Scope
- ✅ Test Style & Structure
- ✅ Naming & Readability
- ✅ Toolchain

---

### Metrics Summary

- ✅ 3912/3912 tests passing (100%)
- ✅ FilePathHelper.cs 84.62% line coverage (unchanged baseline)
- ✅ Zero cycle-4 source diff (verified vs `ac3d6b53`)
- ✅ All four C# code-quality checks passing (exit 0)
- ✅ No evidence-location violations

---

### Recommendation

**Ready for merge.** Cycle 4 introduced no code change; the no-fix-required close is justified and no regression is present. The PR carries the unchanged cycle-3 C# delivery; the repo-wide C# coverage gate of record is the PR CI run.

---

## Appendix A: Test Inventory

### Complete Test List (cycle-4-relevant subset; full suite unchanged from cycle-3 exit)

- UtilitiesCS.Test › FilePathHelper_Tests › (31 tests; full FilePathHelper behavior — positive/negative/edge)
- UtilitiesCS.Test › LcppnFolderPredictor_Serialization_Tests › RoundTrip_WithDedicatedConfig_PreservesContentAndFileName
- UtilitiesCS.Test › LcppnFolderPredictorStore_Tests › (AC23 store/serialization suite, 10 tests total)
- TaskMaster.Test › AppAutoFileObjectsFolderPredictorTests › LoadFolderPredictorAsync_SettingOnButFileMissing_LeavesHolderNull
- TaskMaster.Test › AppAutoFileObjectsFolderPredictorTests › LoadFolderPredictorAsync_SettingOnButReadThrows_FailsSoftToNull
- TaskMaster.Test › AppAutoFileObjectsFolderPredictorTests › LoadFolderPredictorAsync_AppDataMissing_FailsSoftToNull

Full assembly: 3912 tests, all passing (`phase0-tests-coverage.2026-06-16T10-26.md`).

---

## Appendix B: Toolchain Commands Reference

**For C#:**
```powershell
# Formatting
csharpier check .

# Linting (analyzers)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

# Type checking (nullable + TWAE)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true

# Testing
vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /EnableCodeCoverage
```

---

**Audit Completed By:** feature-reviewer (Claude)
**Audit Date:** 2026-06-16
**Policy Version:** Current (as of audit date)
