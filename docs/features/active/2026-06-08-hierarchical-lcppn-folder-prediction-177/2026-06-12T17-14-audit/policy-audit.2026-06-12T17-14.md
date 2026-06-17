# Policy Compliance Audit: hierarchical-lcppn-folder-prediction (Issue #177) — Cycle 2 Exit Reaudit

**Audit Date:** 2026-06-12
**Cycle:** Remediation cycle 2 end-of-cycle reaudit (exit timestamp 2026-06-12T17-14 UTC)
**Code Under Test:** Full cumulative branch diff of `TaskMaster-wt-2026-06-08-12-06` (head `31cbb12e`) against the resolved base (merge-base `742d4f1656367ddb1d43ea66e1bdd59776f1a287` on `main`), covering commits `0223bc60`, `d06f5c00`, `e159bead`, the cycle-1 exit-audit commit `f4ca954c`, and the cycle-2 fix `31cbb12e`. Changed C# production files: `UtilitiesCS/EmailIntelligence/Bayesian/{IFolderPredictor,FolderHierarchyNode,FolderHierarchyTree,PerParentClassifier,LcppnFolderPredictorConfig,LcppnFolderPredictor}.cs`, `UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierGroup.cs`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/{EmailFiler,SortEmail}.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs`, `UtilitiesCS/EmailIntelligence/Evaluation/{EvaluationResult,FolderPredictorEvaluator}.cs`, `UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs`, `TaskMaster/AppGlobals/AppAutoFileObjects.cs`. Changed/new test files: nine files under `UtilitiesCS.Test/EmailIntelligence/` (including the new cycle-2 `LcppnFolderPredictor_Classify_Tests.cs`), with matching `<Compile Include>` entries in `UtilitiesCS.Test.csproj`. One docs/tooling file (`.claude/skills/invoke-atomic-planner/SKILL.md`) and the feature documentation/evidence set are also in the diff.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 20 source files | 3904 suite (incl. feature tests) | ✅ 3903 pass, 1 out-of-scope pre-existing flake (passes in isolation) | 85.31% lines (strict) UtilitiesCS.dll | 85.46% lines (strict) UtilitiesCS.dll | F2 targets 100.00% / 97.71% strict; remaining new types 92.67–100% strict |
| TypeScript | 0 files | N/A | N/A | N/A - out of scope | N/A - out of scope | N/A - out of scope |
| PowerShell | 0 files | N/A | N/A | N/A - out of scope | N/A - out of scope | N/A - out of scope |
| Python | 0 files | N/A | N/A | N/A - out of scope | N/A - out of scope | N/A - out of scope |

**Note:** This is a C#-only feature. No TypeScript, PowerShell, Python, Bash, or JSON source files have changed lines in the branch diff; their verdicts are `N/A - out of scope` because they have zero changed files on the branch.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- C# baseline coverage artifact: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/coverage-p0/baseline.xml`
- C# post-change coverage artifact (canonical): `artifacts/csharp/coverage.xml`
- Per-language comparison summary: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/coverage-delta.2026-06-12T16-45.md` and Section 1.2.1 below.

**Non-negotiable verdict rule:** Numeric baseline and post-change C# coverage are recorded below and were independently re-parsed from `artifacts/csharp/coverage.xml` by this audit (Visual Studio `<results><modules>` format, aggregated per `type_name` over `<function>` elements within the `UtilitiesCS.dll` module; not taken on trust from the comparison document).

**Fail-closed rule:** All required baseline, QA-gate, and coverage-comparison artifacts are present.

---

## Executive Summary

This reaudit verifies remediation cycle 2, which addressed exactly one finding from the cycle-1 exit review (artifacts dated 2026-06-12T16-35): the FAIL where the new test file `LcppnFolderPredictor_Tests.cs` had reached 554 lines, over the 500-line cap (AC20). The cycle-2 fix is a test-only split, committed in `31cbb12e` on top of `e159bead` (cycle-1 head) and the cycle-1 exit-audit commit `f4ca954c`. The finding is independently verified as resolved against the current branch head `31cbb12e`, and the full feature is re-verified clean.

- **F3 (over-cap test file) resolved (verified).** `LcppnFolderPredictor_Tests.cs` is now 316 lines (`awk END{NR}` = 316) and the new sibling `LcppnFolderPredictor_Classify_Tests.cs` is 287 lines (`awk END{NR}` = 287); both are under the 500-line cap. Every feature test file is under 500 lines (largest is `FolderHierarchyTree_Tests.cs` at 334). The new file is registered in `UtilitiesCS.Test.csproj` (`<Compile Include="EmailIntelligence\Bayesian\LcppnFolderPredictor_Classify_Tests.cs" />`, line 119). All 21 `LcppnFolderPredictor` `[TestMethod]` cases are preserved across the two files (12 in `LcppnFolderPredictor_Tests.cs`, 9 `Classify_*` in `LcppnFolderPredictor_Classify_Tests.cs`); a set-diff of the pre-split (`e159bead`) test-method names against the post-split union of the two files is empty — no test dropped or renamed.
- **No regression of cycle-1 fixes (verified).** The diff between the cycle-1 head `e159bead` and HEAD touches only `LcppnFolderPredictor_Tests.cs`, `LcppnFolderPredictor_Classify_Tests.cs`, `UtilitiesCS.Test.csproj`, and documentation/evidence — zero production-code change. F1 therefore stands byte-identical: `grep -rn "_lcppnPredictor" --include="*.cs"` returns no matches; the shared holder `IFolderPredictor FolderPredictor { get; set; }` remains on `IAppAutoFileObjects.cs:45` and is implemented at `AppAutoFileObjects.cs:617`; the AC13 flag-off regression test `GetFolderPredictorAsync_FlagOff_FreshPerCallInstance_ReturnsFlat` (FolderPredictorSeam_Tests.cs:266) and the flag-on reachability test remain present and passing.
- **F2 coverage still met (verified, re-parsed).** Independently re-parsed from `artifacts/csharp/coverage.xml`: `FolderHierarchyTree` 100.00% strict (81/81 lines, 0 partial, 0 not-covered); `LcppnFolderPredictor` 97.71% strict (171/175, 4 partial, 0 not-covered). Both exceed the 90% strict new-code target. Repo-wide UtilitiesCS.dll is 85.46% strict (covered 35651 / partial 908 / not-covered 5157), above the 80% floor and with no regression vs the 85.31% cycle-0 baseline.
- **Containment held across the whole branch (verified).** `git diff <merge-base> HEAD` reports zero changed files for `ManagerAsyncLazy.cs`, `Triage.cs`, `SpamBayes.cs`, `CategoryClassifierGroup.cs`, and `MulticlassEngine.cs`. The shared `Manager` value type `ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>` is unchanged. No `.github/` workflow files were modified (`git diff --name-only <merge-base> HEAD -- .github/` is empty), so `modified-workflow-needs-green-run` and the `ci-workflows.md` deliberately-failing-step rule are not triggered.
- **Toolchain (verified by evidence).** The cycle-2 final QA pass shows steps 1-3 clean (EXIT 0) in a single pass and step 4 with the single documented out-of-scope pre-existing flake (`evidence/qa-gates/final-toolchain.2026-06-12T16-45.md`).

**Disposition of the cycle-1 FAIL (now resolved):** the cycle-1 exit FAIL on the 554-line test file is closed. No new FAIL-level finding is introduced by cycle 2.

Two PARTIAL findings carry over from prior cycles and remain recorded/accepted (out-of-scope by the remediation inputs, not new failures):

1. **Pre-existing modified-file overages (Section 2.3) — PARTIAL (accepted).** `BayesianClassifierGroup.cs` 515 lines (baseline 513, +2 interface declaration this feature), `FolderScorer.cs` 608 (baseline 607), and `SortEmail.cs` 1406 (baseline 1407) exceed the cap. All were over the cap before this feature; the feature did not push any from under to over. Recorded as accepted follow-ups per the remediation-inputs out-of-scope list.
2. **`FolderHierarchyNode` strict coverage (Section 1.2) — PARTIAL (accepted).** 60.0% strict / 100.0% inclusive; the strict shortfall is auto-generated record members, every line exercised. Recorded/accepted per the remediation-inputs out-of-scope list; not an in-scope F2 target.

**Policy documents evaluated:**
- ✅ `CLAUDE.md` — General Code Change Policy, General Unit Test Policy, C# Code Change Policy, C# Unit Test Policy, Tonality Policy
- ✅ `.claude/rules/general-code-change.md`
- ✅ `.claude/rules/general-unit-test.md`
- ✅ `.claude/rules/csharp.md`
- ✅ `.claude/rules/tonality.md`
- ✅ `.claude/rules/ci-workflows.md` (no workflow files changed; not triggered)

**Language-specific policies evaluated:**
- N/A `python` (zero changed files)
- N/A `powershell` (zero changed files)
- N/A Bash / JSON (zero changed source files)
- ✅ C# (`csharp.md`)

**Temporary artifacts cleanup:**
- ✅ No throwaway scripts were introduced by the feature diff.
- ✅ Ongoing tooling: none added by this feature.

---

## Rejected Scope Narrowing

The delegating prompt for this reaudit listed the out-of-scope items (FolderHierarchyNode strict coverage = auto-generated record members; pre-existing over-cap production files `BayesianClassifierGroup.cs` 515 / `FolderScorer.cs` 608 / `SortEmail.cs` 1406) and instructed the reviewer to re-confirm them as accepted, not new FAILs. That instruction is consistent with the full-branch audit scope and was honored for those specific pre-existing items. It did not attempt to narrow the audit below the full branch diff. This reaudit covers the full cumulative diff of all branch commits (`0223bc60`, `d06f5c00`, `e159bead`, `f4ca954c`, `31cbb12e`) against the merge-base. No scope-narrowing text required verbatim recording.

---

## Evidence Location Compliance

A scan of the branch diff for files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` returned no matches. All feature evidence is under the canonical `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/<kind>/` location (`baseline/`, `qa-gates/`, `regression-testing/`, `other/`). The canonical post-change C# coverage artifact at `artifacts/csharp/coverage.xml` is the repo-standard C# coverage location, not an evidence-kind path. The `validate_evidence_locations.py` script is not present in this checkout (only the SKILL.md is), so verification was performed by direct `git diff --name-only` path scan. Verdict: PASS.

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** | ✅ PASS | Each test constructs fixtures in-method. The split preserves the original per-method fixtures verbatim; no shared mutable static state was introduced by moving 9 methods into a sibling file. |
| **Isolation** | ✅ PASS | Each `[TestMethod]` targets one behavior. The split groups `Classify_*` cases into their own file by behavior, keeping isolation intact. |
| **Fast Execution** | ✅ PASS | Tests are in-memory, no I/O or sleeps. `temp/file-IO hits = 0` in both split files. |
| **Determinism** | ✅ PASS | No `DateTime.Now`/`UtcNow`/`Random`/`Thread.Sleep` in the split files; Classify tests build corpora directly. |
| **Readability & Maintainability** | ✅ PASS | Descriptive AC/F-tagged test names; the split improves cohesion (config/validation/train/untrain/build vs. classification). |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | ✅ PASS | Cycle-0 baseline UtilitiesCS.dll strict 85.31% at `evidence/baseline/coverage-p0/baseline.xml`. |
| **No Coverage Regression** | ✅ PASS | Post-change 85.46% strict, independently re-parsed from `artifacts/csharp/coverage.xml` (covered 35651 / partial 908 / not-covered 5157). Baseline 85.31% → 85.46% (+0.15 pp). No regression. The cycle-2 split is test-only, so production line coverage is materially unchanged from cycle 1. |
| **New Code Coverage ≥90%** | ✅ PASS | F2 targets independently re-parsed: `FolderHierarchyTree` 100.00% strict (81/81); `LcppnFolderPredictor` 97.71% strict (171/175). Other new types: `PerParentClassifier` 92.67%, `FolderPredictorEvaluator` 92.86%, `LcppnFolderPredictorConfig` 100%, `EvaluationResult` 100%. `FolderHierarchyNode` 60.0% strict / 100.0% inclusive is the accepted auto-generated-record exception. |
| **Comprehensive Coverage** | ✅ PASS | The cycle-2 split preserves all 21 `LcppnFolderPredictor` cases (set-identical pre/post), so coverage scenarios are unchanged. |
| **Positive Flows** | ✅ PASS | Beam descent returns path-product top leaf; flag-on returns the held LCPPN predictor. |
| **Negative Flows** | ✅ PASS | Config validation throws on out-of-range values; abstention returns empty. |
| **Edge Cases** | ✅ PASS | Single-segment path, duplicate paths, empty-tree serialization, root abstention, missing-parent UnTrain, beam-trim. |
| **Error Handling** | ✅ PASS | `ArgumentNullException`/`ArgumentException`/`ArgumentOutOfRangeException` paths asserted. |
| **Concurrency** | N/A | Prediction/evaluation logic is single-threaded; the seam reuses existing async patterns. |
| **State Transitions** | ✅ PASS | Train/UnTrain localized-update transitions asserted (off-path nodes unchanged). |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.31% lines (strict) -> Post-change: 85.46% lines (strict). Change: +0.15 pp lines. New/changed-code coverage: 97.71%. Disposition: PASS. Evidence: `artifacts/csharp/coverage.xml`, `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/coverage-delta.2026-06-12T16-45.md`. (Both F2 target types exceed the 90% strict new-code gate — `FolderHierarchyTree` 100.00%, `LcppnFolderPredictor` 97.71% — and the repo-wide/regression gates pass. Independently re-parsed from the canonical XML; not taken on trust. The cycle-2 change is test-only, so these production-class metrics are unchanged from cycle 1.)
- TypeScript: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (zero changed files).
- PowerShell: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (zero changed files).

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | ✅ PASS | FluentAssertions with `because` reasons retained across the split. |
| **Arrange-Act-Assert Pattern** | ✅ PASS | Arrange/Act/Assert structure preserved verbatim in moved methods. |
| **Document Intent** | ✅ PASS | AC/F-tagged comments above each test method retained. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | ✅ PASS | No temp-file / filesystem / network / process APIs in the split files (`temp/file-IO hits = 0` in both). |
| **Use Mocks/Stubs** | ✅ PASS | Moq isolates dependencies where needed; the Classify file builds in-memory corpora directly (pure-logic classification, no external system to mock). |
| **Environment Stability** | ✅ PASS | No temporary files; no mutable global static state. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | ✅ PASS | This reaudit plus `evidence/qa-gates/split-verification.2026-06-12T16-45.md`, `evidence/qa-gates/coverage-delta.2026-06-12T16-45.md`, and the cycle-2 final-toolchain gate. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | ✅ PASS | `remediation-inputs.2026-06-12T16-45.md` scopes exactly the F3 over-cap test-file split. |
| **Read existing change plans** | ✅ PASS | `remediation-plan.2026-06-12T16-45.md` present and followed. |
| **Document the plan** | ✅ PASS | Atomic remediation plan and `evidence/other/development-log.md`. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | ✅ PASS | The fix is a minimal mechanical split by behavior; no test logic altered. |
| **Reusability** | ✅ PASS | No copy-paste; the 9 `Classify_*` methods were moved, not duplicated. |
| **Extensibility** | ✅ PASS | `IFolderPredictor` seam unchanged; no production API touched. |
| **Separation of concerns** | ✅ PASS | The split improves cohesion: config/lifecycle tests vs. classification tests. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | ✅ PASS | One responsibility cluster per file; the Classify file is cohesive (classification behavior only). |
| **Under 500 lines** | ✅ PASS (with accepted pre-existing PARTIAL) | **F3 RESOLVED:** `LcppnFolderPredictor_Tests.cs` 316 lines, `LcppnFolderPredictor_Classify_Tests.cs` 287 lines — both under cap. All nine feature test files under 500 (largest 334). New production files all under cap (largest `LcppnFolderPredictor.cs` 363). **PARTIAL (accepted, pre-existing):** modified files `BayesianClassifierGroup.cs` 515 (baseline 513), `FolderScorer.cs` 608 (baseline 607), `SortEmail.cs` 1406 (baseline 1407) — all over the cap before this feature; recorded out-of-scope. |
| **Public vs internal** | ✅ PASS | No surface change; both split files are `internal` test classes. |
| **No circular dependencies** | ✅ PASS | No new dependency edges introduced by the split. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | ✅ PASS | `LcppnFolderPredictor_Classify_Tests` clearly names the classification test grouping. |
| **Docs/docstrings** | ✅ PASS | Per-test AC/F-tagged comments preserved in the split. |
| **Comment why, not what** | ✅ PASS | No new explanatory comments required; existing intent comments retained. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ✅ PASS | `dotnet tool run csharpier check .` EXIT 0; 1077 files, no reformatted (`final-toolchain.2026-06-12T16-45.md` Step 1). |
| **2. Linting** | ✅ PASS | analyzer msbuild EXIT 0; zero diagnostics reference the split files (Step 2). |
| **3. Type checking** | ✅ PASS | nullable/TreatWarningsAsErrors msbuild EXIT 0; 0 warnings (Step 3). |
| **4. Testing** | ✅ PASS (in-scope) | `vstest.console.exe ... /EnableCodeCoverage /InIsolation`: 3903 pass, 1 out-of-scope pre-existing flake (`AddEntry_UseUiThreadTrue...`, ci-flaky-test-isolation-176) that passes 1/1 in isolation and is unrelated to this work (Step 4). All in-scope tests pass. |
| **Full toolchain loop** | ✅ PASS | Single final pass; steps 1-3 clean; step 4's only failure is the excluded pre-existing flake, which does not gate this feature. |
| **Explicit reporting** | ✅ PASS | Commands and results recorded in `final-toolchain.2026-06-12T16-45.md`. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | ✅ PASS | Commit `31cbb12e` message and development log. |
| **Design choices explained** | ✅ PASS | Split rationale recorded in `remediation-plan.2026-06-12T16-45.md`. |
| **Update supporting documents** | ✅ PASS | Evidence set updated (split-verification, coverage-delta, containment, endstate). |
| **Provide next steps** | ✅ PASS | Cycle-2 endstate recorded in `evidence/qa-gates/cycle2-endstate.2026-06-12T16-45.md`. |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3C#: C# Code Change Policy Compliance

#### 3C#.1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with CSharpier** | ✅ PASS | `final-toolchain.2026-06-12T16-45.md` Step 1 EXIT 0. |
| **Linting with .NET analyzers** | ✅ PASS | Step 2 EXIT 0, zero diagnostics on split files. |
| **Nullable / TreatWarningsAsErrors** | ✅ PASS | Step 3 EXIT 0, 0 warnings. |
| **Testing with vstest** | ✅ PASS | Step 4: in-scope tests pass; only the excluded pre-existing flake failed (passes in isolation). |

#### 3C#.2 C# Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Strong contracts / explicit APIs** | ✅ PASS | No production API touched in cycle 2; F1 holder unchanged from cycle 1. |
| **Null-safety** | ✅ PASS | Nullable build clean; no production code changed this cycle. |
| **Composition / focused types** | ✅ PASS | Split increases test-file cohesion; no inheritance added. |
| **Async/resource safety** | ✅ PASS | Async test methods retained verbatim. |

#### 3C#.3 Error Handling, Logging, Contracts

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Exceptions fail fast** | ✅ PASS | Production validation unchanged; argument-exception assertions preserved in the split. |
| **Logging pattern** | ✅ PASS | No ad-hoc console output added. |
| **Invariants at construction** | ✅ PASS | Config and classifier validation unchanged and intact. |

#### 3C#.4 Dependencies / Analyzer Configuration

| Requirement | Status | Evidence |
|------------|--------|----------|
| **No new dependency** | ✅ PASS | csproj diff is a single `<Compile Include>` addition for the new test file. |
| **Analyzer config via .editorconfig** | ✅ PASS | No per-command analyzer overrides introduced. |

---

## 4. Language-Specific Unit Test Policy Compliance

### Section 4C#: C# Unit Test Policy Compliance

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Framework MSTest** | ✅ PASS | Both split files `using Microsoft.VisualStudio.TestTools.UnitTesting`; no xUnit/NUnit. |
| **Moq for mocking** | ✅ PASS | Moq used where external dependencies exist; the Classify file uses direct in-memory corpora (no mock needed). |
| **FluentAssertions** | ✅ PASS | Both split files `using FluentAssertions`. |
| **Coverage ≥90% new / ≥80% repo** | ✅ PASS | Repo-wide 85.46% strict; both F2 target types ≥ 90% strict (100.00% / 97.71%). |
| **No temp files / external deps** | ✅ PASS | `temp/file-IO hits = 0` in both split files. |

---

## 5. Test Coverage Detail

### FolderHierarchyTree (FolderHierarchyTree_Tests.cs)

| Test focus | Scenario Type | Status |
|-----------|--------------|--------|
| Construction, single-segment, duplicates, new-leaf | Positive/Edge | ✅ |
| `GetChildren`/`NodeKeys`/`GetNode`/`IsLeaf`/`ContainsNode` branches | Negative/Edge | ✅ |

**Coverage:** 100.00% strict (81/81 lines).

### LcppnFolderPredictor (LcppnFolderPredictor_Tests.cs + LcppnFolderPredictor_Classify_Tests.cs + serialization)

| Test focus | Scenario Type | Status |
|-----------|--------------|--------|
| Config defaults/validation, train/untrain, build, IFolderPredictor assignability (12 cases) | Positive/Negative/Edge | ✅ (`LcppnFolderPredictor_Tests.cs`) |
| Beam descent, beam width, abstention, terminal-leaf, beam-trim (9 `Classify_*` cases) | Positive/Edge/Negative | ✅ (`LcppnFolderPredictor_Classify_Tests.cs`, cycle-2 split) |

**Coverage:** 97.71% strict (171/175; 4 partial, 0 not-covered). All 21 cases preserved across the split (set-identical pre/post).

### FolderPredictorSeam (FolderPredictorSeam_Tests.cs)

| Test focus | Scenario Type | Status |
|-----------|--------------|--------|
| Flag-off returns flat group byte-for-byte | Positive | ✅ |
| Flag-on returns held LCPPN predictor | Positive | ✅ |
| Flag-on reachable through fresh per-call instance (F1 regression) | State/Edge | ✅ |
| Flag-off fresh per-call instance returns flat (AC13 regression) | State/Edge | ✅ |

**Not covered:** A small number of partial lines in `LcppnFolderPredictor` (4 partial, 0 not-covered) and auto-generated record members in `FolderHierarchyNode` (accepted out-of-scope). No business-logic line is uncovered in the F2 targets.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests (suite, final run) | 3904 | ✅ |
| Tests Passed | 3903 (in-scope: 100%) | ✅ |
| Tests Failed | 1 (out-of-scope pre-existing flake `AddEntry_UseUiThreadTrue...`, passes 1/1 in isolation) | ✅ (does not gate) |
| `LcppnFolderPredictor` test cases (post-split) | 21 (12 + 9), set-identical to pre-split | ✅ |
| Repo-wide line coverage (UtilitiesCS.dll) | 85.46% strict | ✅ |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `dotnet tool run csharpier check .` | EXIT 0, 1077 files, no reformatted | ✅ |
| .NET Analyzers | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT 0, no new diagnostics | ✅ |
| Nullable Type Check | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` | EXIT 0, 0 warnings, 0 errors | ✅ |
| MSTest via vstest | `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation` | 3903 pass; 1 out-of-scope pre-existing flake | ✅ |
| Workflow change scan | `git diff --name-only <merge-base> HEAD -- .github/` | empty (no workflow files changed) | ✅ |
| Confidentiality masking scan | review of diff for secrets/keys | none present | ✅ |
| Suppression scan (added lines) | review of cycle-2 added lines for new suppressions | none added | ✅ |

**Notes:** No `.github/` workflow files were modified by this cycle, so `modified-workflow-needs-green-run` and the `ci-workflows.md` deliberately-failing-step rule are not triggered. The pre-existing `IdleAsyncQueue` UI-thread flake (ci-flaky-test-isolation-176) is the only failing test; it passes in isolation and is unrelated to the test-file split.

---

## 8. Gaps and Exceptions

### Identified Gaps
- **File-size cap (cycle-1 FAIL now RESOLVED):** `LcppnFolderPredictor_Tests.cs` split into 316 + 287-line files, both under cap; new file registered in csproj.
- **Pre-existing modified-file overages (PARTIAL, accepted):** `BayesianClassifierGroup.cs` (515), `FolderScorer.cs` (608), `SortEmail.cs` (1406) — pre-existing; recorded out-of-scope.
- **`FolderHierarchyNode` strict coverage (PARTIAL, accepted):** 60.0% strict / 100.0% inclusive; auto-generated record members; out-of-scope.

### Approved Exceptions
- **None.** No new approved exceptions are claimed.

### Removed/Skipped Tests
- **None.** No tests were removed or skipped; the split preserves all 21 cases (set-identical pre/post).

---

## 9. Summary of Changes

### Commits in This PR/Branch (after merge-base `742d4f16`)
1. **0223bc60** - feat(folder-predictor): LCPPN hierarchy model + serialization (#177)
2. **d06f5c00** - feat(folder-predictor): wire flag-gated LCPPN seam + eval harness (#177)
3. **e159bead** - fix(folder-predictor): make flag-on LCPPN path reachable + raise coverage (#177) — cycle-1 remediation
4. **f4ca954c** - docs(review): cycle-1 exit reaudit artifacts for #177
5. **31cbb12e** - test(folder-predictor): split LcppnFolderPredictor tests under 500-line cap (#177) — cycle-2 remediation

### Files Changed (cycle-2 remediation, commit 31cbb12e)
1. `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Tests.cs` (MODIFIED) — trimmed to 316 lines (12 config/validation/train/untrain/build/assignability methods).
2. `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Classify_Tests.cs` (ADDED) — 287 lines (9 `Classify_*` methods).
3. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (MODIFIED) — one `<Compile Include>` for the new file.

Zero production-code change in cycle 2 (verified by `git diff --name-status e159bead HEAD`).

---

## 10. Compliance Verdict

### Overall Status: ✅ COMPLIANT

Remediation cycle 2 resolved the sole cycle-1 FAIL (the 554-line over-cap test file) via a test-only split, with all 21 `LcppnFolderPredictor` cases preserved (set-identical pre/post), the new file registered in the csproj, and both resulting files under the 500-line cap. Because cycle 2 changed no production code, F1 (flag-on reachability via the shared AF holder) and F2 (strict new-code coverage) stand byte-identical and were re-verified: `FolderHierarchyTree` 100.00% strict and `LcppnFolderPredictor` 97.71% strict, repo-wide 85.46% strict with no regression. Containment held (zero diff on all prohibited files), no workflow files changed, and the toolchain is clean (steps 1-3 EXIT 0; step 4's only failure is an out-of-scope pre-existing flake that passes in isolation). Two accepted, pre-existing PARTIAL items remain (the three over-cap production files and `FolderHierarchyNode` strict coverage); neither is a new failure and both are out-of-scope follow-ups.

**Fail-closed reminder:** All required baseline, QA, and coverage-comparison artifacts are present; coverage was independently re-parsed from the canonical artifact.

### Metrics Summary
- ✅ In-scope tests passing (3903/3903); 1 out-of-scope pre-existing flake (passes in isolation, does not gate)
- ✅ Repo-wide line coverage 85.46% strict (≥ 80% floor, +0.15 pp, no regression)
- ✅ F2 target new-code strict coverage 100.00% / 97.71% (≥ 90%)
- ✅ All four C# toolchain steps clean for in-scope work (single final pass)
- ✅ Containment intact (ManagerAsyncLazy.cs and out-of-scope classifiers zero diff; `Manager` value type unchanged)
- ✅ No workflow files modified
- ✅ Over-cap test file resolved: `LcppnFolderPredictor_Tests.cs` 316 / `LcppnFolderPredictor_Classify_Tests.cs` 287, both under 500
- ✅ All 21 test cases preserved across the split (set-identical pre/post)
- ⚠️ Pre-existing modified-file overages (515/608/1406) — accepted, out-of-scope
- ⚠️ `FolderHierarchyNode` 60.0% strict / 100.0% inclusive — accepted, out-of-scope (auto-generated record members)

### Recommendation

**Approve.** The cycle-1 FAIL is resolved with no production-code change, no test dropped, and all gates green for in-scope work. The two carried-over PARTIAL items are pre-existing and explicitly accepted out-of-scope. No FAIL-level findings and no blocking-PARTIAL findings remain.

---

**Audit Completed By:** feature-reviewer agent
**Audit Date:** 2026-06-12 (exit timestamp 2026-06-12T17-14 UTC)
**Policy Version:** Current (as of audit date)

---

## Appendix A: Test Inventory

Cycle-2 split (commit `31cbb12e`):

1. `LcppnFolderPredictor_Tests.cs` (316 lines) — 12 `[TestMethod]` cases: config defaults/validation, train/untrain, build, `IFolderPredictor` assignability.
2. `LcppnFolderPredictor_Classify_Tests.cs` (287 lines, new) — 9 `Classify_*` `[TestMethod]` cases: beam descent, path-product ordering, beam width, abstention, terminal-leaf emission, beam-trim.

Union of (1)+(2) is set-identical to the 21 `[TestMethod]` cases in the pre-split file at `e159bead` (verified by `diff` of sorted method-name lists → no difference).

All nine feature test files at HEAD (all under 500 lines):
- `BayesianClassifierGroup_FlatPathUnchanged_Tests.cs` (118)
- `FolderHierarchyTree_Tests.cs` (334)
- `IFolderPredictor_Tests.cs` (100)
- `LcppnFolderPredictor_Classify_Tests.cs` (287)
- `LcppnFolderPredictor_Serialization_Tests.cs` (192)
- `LcppnFolderPredictor_Tests.cs` (316)
- `PerParentClassifier_Tests.cs` (236)
- `FolderPredictorEvaluator_Tests.cs` (230)
- `FolderPredictorSeam_Tests.cs` (285)

---

## Appendix B: Toolchain Commands Reference

**For C#:**
```powershell
# Formatting
dotnet tool run csharpier check .

# Linting (analyzers)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

# Type checking (nullable)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true

# Testing with coverage
vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation
```
