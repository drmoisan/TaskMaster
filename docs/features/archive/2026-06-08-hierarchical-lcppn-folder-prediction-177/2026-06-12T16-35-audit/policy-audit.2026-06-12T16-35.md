# Policy Compliance Audit: hierarchical-lcppn-folder-prediction (Issue #177) — Cycle 1 Exit Reaudit

**Audit Date:** 2026-06-12
**Cycle:** Remediation cycle 1 end-of-cycle reaudit (exit timestamp 2026-06-12T16-35 UTC)
**Code Under Test:** Full cumulative branch diff of `TaskMaster-wt-2026-06-08-12-06` (head `e159bead`) against the resolved base (merge-base `742d4f1656367ddb1d43ea66e1bdd59776f1a287` on `main`), covering commits `0223bc60`, `d06f5c00`, and the cycle-1 fix `e159bead`. Changed C# production files: `UtilitiesCS/EmailIntelligence/Bayesian/{IFolderPredictor,FolderHierarchyNode,FolderHierarchyTree,PerParentClassifier,LcppnFolderPredictorConfig,LcppnFolderPredictor}.cs`, `UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierGroup.cs`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/{EmailFiler,SortEmail}.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs`, `UtilitiesCS/EmailIntelligence/Evaluation/{EvaluationResult,FolderPredictorEvaluator}.cs`, `UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs`, `TaskMaster/AppGlobals/AppAutoFileObjects.cs`. Changed/new test files: eight files under `UtilitiesCS.Test/EmailIntelligence/`, two `.csproj` `<Compile Include>` additions. One docs/tooling file (`.claude/skills/invoke-atomic-planner/SKILL.md`) and the feature documentation/evidence set are also in the diff.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 20 source files | 3904 suite (incl. feature tests) | ✅ 3904 pass, 0 fail | 85.31% lines (strict) UtilitiesCS.dll | 85.45% lines (strict) UtilitiesCS.dll | F2 targets 100.00% / 97.71% strict; remaining new types 92.7–100% strict |
| TypeScript | 0 files | N/A | N/A | N/A - out of scope | N/A - out of scope | N/A - out of scope |
| PowerShell | 0 files | N/A | N/A | N/A - out of scope | N/A - out of scope | N/A - out of scope |
| Python | 0 files | N/A | N/A | N/A - out of scope | N/A - out of scope | N/A - out of scope |

**Note:** This is a C#-only feature. No TypeScript, PowerShell, Python, Bash, or JSON source files have changed lines in the branch diff; their verdicts are `N/A - out of scope` because they have zero changed files on the branch.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- C# baseline coverage artifact: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/coverage-p0/baseline.xml` (and prior `evidence/baseline/2026-06-10T12-31/coverage.xml`)
- C# post-change coverage artifact (canonical): `artifacts/csharp/coverage.xml`
- Per-language comparison summary: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/2026-06-12T15-54/coverage-comparison.md` and Section 1.2.1 below.

**Non-negotiable verdict rule:** Numeric baseline and post-change C# coverage are recorded below and were independently re-parsed from `artifacts/csharp/coverage.xml` by this audit (Visual Studio `<results><modules>` format; not taken on trust from the comparison document).

**Fail-closed rule:** All required baseline, QA-gate, and coverage-comparison artifacts are present.

---

## Executive Summary

This reaudit verifies remediation cycle 1, which addressed exactly two findings from the prior review (artifacts dated 2026-06-12T15-43): F1 (Major) — flag-on LCPPN path unreachable — and F2 (Minor) — strict new-code coverage below 90% for two real-logic types. Both were independently verified as resolved against the current branch head `e159bead`.

- **F1 resolved (verified).** The dead per-instance `_lcppnPredictor` field is removed (`grep -rn "_lcppnPredictor" --include="*.cs"` returns no matches). A Folder-only holder `IFolderPredictor FolderPredictor { get; set; }` now lives on the shared `IAppAutoFileObjects` surface (`IAppAutoFileObjects.cs:44`, implemented `AppAutoFileObjects.cs:617`). It is set at the registration site `OlFolderClassifierGroup.BuildClassifiersAsync` (`OlFolderClassifierGroup.cs:281`) when `UseLcppnPredictor` is on and resolved in `GetFolderPredictorAsync` (`OlFolderClassifierGroup.cs:80-91`). Because all three production callers share the same `globals`, the held predictor is reachable from any fresh per-call `OlFolderClassifierGroup` instance. The regression test `GetFolderPredictorAsync_FlagOn_ReachableThroughFreshPerCallInstance` (`FolderPredictorSeam_Tests.cs:232-260`) proves this with two independent instances over shared globals; flag-off byte-for-byte behavior is preserved by `GetFolderPredictorAsync_FlagOff_FreshPerCallInstance_ReturnsFlat` (`FolderPredictorSeam_Tests.cs:266-283`).
- **F2 resolved (verified).** Independently re-parsed from `artifacts/csharp/coverage.xml`: `FolderHierarchyTree` 100.00% strict (81/81 lines, 0 partial, 0 not-covered); `LcppnFolderPredictor` 97.71% strict (171/175, 4 partial, 0 not-covered). Both exceed the 90% strict new-code target. Repo-wide UtilitiesCS.dll is 85.45% strict (covered 35646 / partial 909 / not-covered 5161 of 41716), above the 80% floor and with no regression vs the 85.31% baseline.
- **Containment held (verified).** `git diff <merge-base> HEAD` for `ManagerAsyncLazy.cs`, `Triage.cs`, `SpamBayes.cs`, `CategoryClassifierGroup.cs`, and `MulticlassEngine.cs` is empty (zero diff). The shared `Manager` value type `ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>` is unchanged. No `.github/` workflow files were modified, so `modified-workflow-needs-green-run` and the `ci-workflows.md` deliberately-failing-step rule are not triggered.
- **Toolchain (verified by evidence).** The Phase 3 final QA pass shows all four C# steps clean in a single pass (`evidence/qa-gates/2026-06-12T15-54/final-{csharpier,analyzers,nullable,tests}.md`: EXIT 0 each; tests 3904/3904 pass).

One **new FAIL-level finding is recorded in this cycle** that the prior audit did not surface and that the cycle-1 remediation introduced:

1. **File-size cap (Section 2.3) — NEW FAIL.** The cycle-1 F2 work (plan task P2-T2) added 136 lines to the **new** test file `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Tests.cs`, taking it from 418 lines (at pre-cycle-1 head `d06f5c00`) to **554 lines** — over the 500-line cap. The General Code Change Policy applies the 500-line cap to "production code, test code, or reusable script file" with exceptions only for throwaway scripts and raw text fixtures; a registered MSTest test file is neither. This file is NEW in the branch (absent at the merge-base), so it is not a pre-existing overage: the feature created an over-cap file, and the cycle-1 remediation is the commit that crossed the cap. This is a FAIL-level policy finding and is added to the remediation triggers.

Two PARTIAL findings carry over from the prior cycle and remain recorded/accepted (out-of-scope for cycle 1 by the remediation inputs, not new failures):

2. **Pre-existing modified-file overages (Section 2.3) — PARTIAL (accepted).** `BayesianClassifierGroup.cs` 515 lines (baseline 513, +2 interface declaration this feature), `FolderScorer.cs` 608 (baseline 607), and `SortEmail.cs` 1406 (baseline 1407) exceed the cap. All were over the cap before this feature; the feature did not push any from under to over. Recorded as accepted follow-ups per the cycle-1 remediation-inputs out-of-scope list.
3. **`FolderHierarchyNode` strict coverage (Section 1.2) — PARTIAL (accepted).** 60.0% strict / 100.0% inclusive; the strict shortfall is auto-generated record members, every line exercised. Recorded/accepted per cycle-1 remediation-inputs out-of-scope list; not a cycle-1 in-scope F2 target.

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

The delegating prompt for this reaudit listed the out-of-scope items (FolderHierarchyNode strict coverage; BayesianClassifierGroup.cs 515-line cap) and instructed the reviewer to "assess them as recorded/accepted, not as new FAILs." That instruction is consistent with the full-branch audit scope and was honored for those two specific pre-existing items. It did NOT attempt to narrow the audit below the full branch diff, and this reaudit covers the full cumulative diff of all branch commits (`0223bc60`, `d06f5c00`, `e159bead`) against the merge-base.

No instruction attempted to exclude the newly-introduced over-cap test file from scope; the full-diff scan surfaced it and it is reported as a FAIL. No scope-narrowing text required verbatim recording.

---

## Evidence Location Compliance

A scan of the branch diff for files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` returned no matches. All feature evidence is under the canonical `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/<kind>/` location (`baseline/`, `qa-gates/`, `regression-testing/`, `other/`). The canonical post-change C# coverage artifact at `artifacts/csharp/coverage.xml` is the repo-standard C# coverage location, not an evidence-kind path. The `validate_evidence_locations.py` script is not present in this checkout (only the SKILL.md is), so verification was performed by direct `git diff --name-only` path scan. Verdict: PASS.

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** | ✅ PASS | Each new/modified test constructs fixtures in-method; `FolderPredictorSeam_Tests.cs` builds a fresh `OlFolderClassifierGroup` per test and a real backing store on the mock (`SetupProperty(x => x.FolderPredictor)`, line 84) — no shared mutable static state. |
| **Isolation** | ✅ PASS | Each `[TestMethod]` targets one behavior; the F1 regression tests isolate the per-call reachability behavior precisely. |
| **Fast Execution** | ✅ PASS | Tests are in-memory, no I/O or sleeps. The full suite of 3904 tests completed in the final run (`final-tests.md`). |
| **Determinism** | ✅ PASS | No `DateTime.Now`/`UtcNow`/`Random`/`Thread.Sleep` in new code; evaluator split is index-based; serialization tests use in-memory JSON. The one pre-existing `IdleAsyncQueue` flake did not reproduce on the final run (3904/3904). |
| **Readability & Maintainability** | ✅ PASS | Descriptive AC/F-tagged test names; Arrange/Act/Assert comments; the F1 regression tests carry explanatory comments tying them to the production per-call pattern. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | ✅ PASS | Cycle-1 baseline UtilitiesCS.dll strict 85.31% at `evidence/baseline/coverage-p0/baseline.xml` (P0-T6); per-type baselines `FolderHierarchyTree` 86.42% strict, `LcppnFolderPredictor` 89.14% strict. |
| **No Coverage Regression** | ✅ PASS | Post-change 85.45% strict, independently re-parsed from `artifacts/csharp/coverage.xml` (covered 35646 / partial 909 / not-covered 5161 of 41716). Baseline 85.31% → 85.45% (+0.14 pp). No regression. |
| **New Code Coverage ≥90%** | ✅ PASS | F2 targets independently re-parsed from the canonical XML: `FolderHierarchyTree` 100.00% strict (81/81, 0 partial, 0 not-covered); `LcppnFolderPredictor` 97.71% strict (171/175, 4 partial, 0 not-covered). Both ≥ 90% strict. Other new types: `PerParentClassifier` 92.7%, `FolderPredictorEvaluator` 92.9%, config/value objects 100%. `FolderHierarchyNode` 60.0% strict / 100.0% inclusive is the accepted auto-generated-record exception (out-of-scope for cycle 1). |
| **Comprehensive Coverage** | ✅ PASS | Cycle-1 F2 tests added the previously-uncovered `FolderHierarchyTree` `GetChildren`/`NodeKeys`/`GetNode`/`IsLeaf`/`ContainsNode` branches and the `LcppnFolderPredictor` terminal-leaf, zero-score, abstention, beam-trim, and missing-parent UnTrain branches. |
| **Positive Flows** | ✅ PASS | Beam descent returns path-product top leaf; flag-on returns the held LCPPN predictor. |
| **Negative Flows** | ✅ PASS | Config validation throws on out-of-range values; abstention returns empty. |
| **Edge Cases** | ✅ PASS | Single-segment path, duplicate paths, empty-tree serialization, root abstention, missing-parent UnTrain. |
| **Error Handling** | ✅ PASS | `ArgumentNullException`/`ArgumentException`/`ArgumentOutOfRangeException` paths asserted. |
| **Concurrency** | N/A | New prediction/evaluation logic is single-threaded; the seam reuses existing async patterns. |
| **State Transitions** | ✅ PASS | Train/UnTrain localized-update transitions asserted (off-path nodes unchanged). |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.31% lines (strict) -> Post-change: 85.45% lines (strict). Change: +0.14 pp lines. New/changed-code coverage: 97.71%. Disposition: PASS. Evidence: `artifacts/csharp/coverage.xml`, `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/2026-06-12T15-54/coverage-comparison.md`. (Both cycle-1 F2 target types now exceed the 90% strict new-code gate — `FolderHierarchyTree` 100.00%, `LcppnFolderPredictor` 97.71% — and the repo-wide/regression gates pass. Independently re-parsed from the canonical XML; not taken on trust.)
- TypeScript: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (zero changed files).
- PowerShell: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (zero changed files).

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | ✅ PASS | FluentAssertions with `because` reasons (e.g., `BeSameAs(lcppn, "the held predictor is reachable per-call")`, line 257). |
| **Arrange-Act-Assert Pattern** | ✅ PASS | Explicit Arrange/Act/Assert comment blocks in all new test methods. |
| **Document Intent** | ✅ PASS | AC/F-tagged comments above each test method. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | ✅ PASS | No temp-file / filesystem / network / process APIs in new test files. |
| **Use Mocks/Stubs** | ✅ PASS | Moq isolates `IApplicationGlobals`/`IAppAutoFileObjects` (with a real `FolderPredictor` backing store via `SetupProperty`), `IFolderWrapper`, and `IFolderPredictor`. |
| **Environment Stability** | ✅ PASS | No temporary files; no mutable global static state; serialization is in-memory JSON. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | ✅ PASS | This reaudit plus `evidence/qa-gates/2026-06-12T15-54/coverage-comparison.md` and the Phase 3 final-step gates. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | ✅ PASS | `remediation-inputs.2026-06-12T15-54.md` scopes exactly F1 and F2. |
| **Read existing change plans** | ✅ PASS | `remediation-plan.2026-06-12T15-54.md` present and followed. |
| **Document the plan** | ✅ PASS | Atomic remediation plan and `evidence/other/development-log.md`. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | ✅ PASS | F1 uses the smallest seam — a single Folder-only holder on the existing AF surface reachable by all callers — rather than retyping `Manager`. |
| **Reusability** | ✅ PASS | The holder reuses the existing shared `globals.AF` that all three callers already read. |
| **Extensibility** | ✅ PASS | `IFolderPredictor` seam unchanged; both implementations satisfy it. |
| **Separation of concerns** | ✅ PASS | Pure logic remains COM-free; Outlook interaction stays in `OlFolderClassifierGroup`. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | ✅ PASS | One type per file; clear namespace separation. |
| **Under 500 lines** | ❌ FAIL | **NEW FAIL:** `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Tests.cs` is a NEW file at **554 lines** (verified `awk END{NR}` = 554, `wc -l` = 554). It was 418 lines at the pre-cycle-1 head `d06f5c00` and crossed the 500-line cap in cycle-1 commit `e159bead` (P2-T2 added 136 lines). The 500-line cap applies to test code; an MSTest file is not a throwaway script or raw text fixture, so no exception applies. **PARTIAL (accepted, pre-existing):** modified files `BayesianClassifierGroup.cs` 515 (baseline 513), `FolderScorer.cs` 608 (baseline 607), `SortEmail.cs` 1406 (baseline 1407) — all over the cap before this feature; recorded out-of-scope per cycle-1 remediation inputs. New production files are all under cap (largest `LcppnFolderPredictor.cs` 363). |
| **Public vs internal** | ✅ PASS | `SetLcppnPredictor` is `internal`; the new `FolderPredictor` holder is a typed interface property. |
| **No circular dependencies** | ✅ PASS | `Evaluation` depends on `Bayesian`; no reverse edge; the AF holder references `IFolderPredictor` only. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | ✅ PASS | `FolderPredictor`, `GetFolderPredictorAsync`, regression test names express behavior. |
| **Docs/docstrings** | ✅ PASS | XML doc comments on the new `IAppAutoFileObjects.FolderPredictor` and `AppAutoFileObjects.FolderPredictor` explain the holder's purpose and null semantics; `SetLcppnPredictor`/`GetFolderPredictorAsync` docs updated to describe the shared-holder routing. |
| **Comment why, not what** | ✅ PASS | Comments explain why a Folder-only AF holder closes the per-call reachability gap without retyping `Manager`. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ✅ PASS | `dotnet tool run csharpier check .` EXIT 0; 1076 files, no unformatted (`final-csharpier.md`). |
| **2. Linting** | ✅ PASS | analyzer msbuild EXIT 0; 0 warnings, 0 errors (`final-analyzers.md`). |
| **3. Type checking** | ✅ PASS | nullable/TreatWarningsAsErrors msbuild EXIT 0; 0 warnings, 0 errors (`final-nullable.md`). |
| **4. Testing** | ✅ PASS | `vstest.console.exe ... /EnableCodeCoverage /InIsolation` EXIT 0; 3904/3904 pass (`final-tests.md`). |
| **Full toolchain loop** | ✅ PASS | Single final pass with all four steps clean (Phase 3 final gates). |
| **Explicit reporting** | ✅ PASS | Commands and results recorded in each `final-*.md` gate artifact. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | ✅ PASS | Commit `e159bead` message and development log. |
| **Design choices explained** | ✅ PASS | F1 mechanism selection and justification recorded in `remediation-plan.2026-06-12T15-54.md`. |
| **Update supporting documents** | ✅ PASS | Evidence and AC check-offs updated. |
| **Provide next steps** | ✅ PASS | Containment-check and reachability evidence recorded. |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3C#: C# Code Change Policy Compliance

#### 3C#.1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with CSharpier** | ✅ PASS | `final-csharpier.md` EXIT 0. |
| **Linting with .NET analyzers** | ✅ PASS | `final-analyzers.md` EXIT 0, 0 warnings. |
| **Nullable / TreatWarningsAsErrors** | ✅ PASS | `final-nullable.md` EXIT 0, 0 warnings. |
| **Testing with vstest** | ✅ PASS | `final-tests.md` EXIT 0, 3904/3904. |

#### 3C#.2 C# Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Strong contracts / explicit APIs** | ✅ PASS | `FolderPredictor` is a nullable `IFolderPredictor` property with documented null semantics. |
| **Null-safety** | ✅ PASS | `GetFolderPredictorAsync` guards `Globals.AF.FolderPredictor is not null` before returning it. Nullable build clean. |
| **Composition / focused types** | ✅ PASS | The holder composes onto the existing AF surface; no inheritance added. |
| **Async/resource safety** | ✅ PASS | `BuildLcppnPredictorAsync`/`GetFolderPredictorAsync` use Task-based async. |

#### 3C#.3 Error Handling, Logging, Contracts

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Exceptions fail fast** | ✅ PASS | Construction validates invariants; `BuildLcppnPredictorAsync` calls `collection.ThrowIfNull()`. |
| **Logging pattern** | ✅ PASS | No ad-hoc console output added; existing log4net pattern retained. |
| **Invariants at construction** | ✅ PASS | Config and classifier validation unchanged and intact. |

#### 3C#.4 Dependencies / Analyzer Configuration

| Requirement | Status | Evidence |
|------------|--------|----------|
| **No new dependency** | ✅ PASS | No new NuGet/package; csproj diffs are `<Compile Include>` only. |
| **Analyzer config via .editorconfig** | ✅ PASS | No per-command analyzer overrides introduced. |

---

## 4. Language-Specific Unit Test Policy Compliance

### Section 4C#: C# Unit Test Policy Compliance

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Framework MSTest** | ✅ PASS | All test files use `Microsoft.VisualStudio.TestTools.UnitTesting`; no xUnit/NUnit. |
| **Moq for mocking** | ✅ PASS | Moq used for `IApplicationGlobals`, `IAppAutoFileObjects`, `IFolderWrapper`, `IFolderPredictor`. |
| **FluentAssertions** | ✅ PASS | FluentAssertions used throughout. |
| **Coverage ≥90% new / ≥80% repo** | ✅ PASS | Repo-wide 85.45% strict; both F2 target types ≥ 90% strict (100.00% / 97.71%). |
| **No temp files / external deps** | ✅ PASS | Confirmed; in-memory JSON for serialization tests. |

---

## 5. Test Coverage Detail

### FolderHierarchyTree (FolderHierarchyTree_Tests.cs)

| Test focus | Scenario Type | Status |
|-----------|--------------|--------|
| Construction, single-segment, duplicates, new-leaf | Positive/Edge | ✅ |
| `GetChildren` null-key / unknown-node early returns | Negative/Edge | ✅ (cycle-1 F2 add) |
| `NodeKeys` accessor, `GetNode` null/missing, `IsLeaf`/`ContainsNode` false branches | Edge | ✅ (cycle-1 F2 add) |

**Coverage:** 100.00% strict (81/81 lines).

### LcppnFolderPredictor (LcppnFolderPredictor_Tests.cs + serialization)

| Test focus | Scenario Type | Status |
|-----------|--------------|--------|
| Beam descent, configurable beam width, abstention, localized update | Positive/Edge/Negative | ✅ |
| Terminal-leaf emission, zero-score terminal, beam-trim, missing-parent UnTrain | Edge | ✅ (cycle-1 F2 add) |

**Coverage:** 97.71% strict (171/175; 4 partial, 0 not-covered).

### FolderPredictorSeam (FolderPredictorSeam_Tests.cs)

| Test focus | Scenario Type | Status |
|-----------|--------------|--------|
| Flag-off returns flat group byte-for-byte | Positive | ✅ |
| Flag-on returns held LCPPN predictor | Positive | ✅ |
| Flag-on reachable through fresh per-call instance (F1 regression) | State/Edge | ✅ (cycle-1 F1 add) |
| Flag-off fresh per-call instance returns flat (AC13 regression) | State/Edge | ✅ (cycle-1 F1 add) |

**Not covered:** A small number of partial lines in `LcppnFolderPredictor` (4 partial, 0 not-covered) and auto-generated record members in `FolderHierarchyNode` (accepted out-of-scope). No business-logic line is uncovered in the F2 targets.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests (suite, final run) | 3904 | ✅ |
| Tests Passed | 3904 (100%) | ✅ |
| Tests Failed | 0 | ✅ |
| Pre-existing flake (out of scope) | `IdleAsyncQueue` did not reproduce on final run | ✅ |
| Repo-wide line coverage (UtilitiesCS.dll) | 85.45% strict / 87.63% inclusive | ✅ |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `dotnet tool run csharpier check .` | EXIT 0, 1076 files, no unformatted | ✅ |
| .NET Analyzers | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT 0, 0 warnings, 0 errors | ✅ |
| Nullable Type Check | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` | EXIT 0, 0 warnings, 0 errors | ✅ |
| MSTest via vstest | `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation` | EXIT 0, 3904/3904 | ✅ |

**Notes:** No `.github/` workflow files were modified by this cycle, so `modified-workflow-needs-green-run` and the `ci-workflows.md` deliberately-failing-step rule are not triggered. The pre-existing `IdleAsyncQueue` UI-thread flake (tracked separately) did not reproduce on the final full-suite run.

---

## 8. Gaps and Exceptions

### Identified Gaps
- **File-size cap (NEW FAIL):** new test file `LcppnFolderPredictor_Tests.cs` is 554 lines (> 500 cap); introduced by cycle-1 P2-T2. Remediation: split the test file into two cohesive files (e.g., descent/abstention tests vs. construction/serialization/coverage-branch tests), each under 500 lines, with matching `<Compile Include>` entries.
- **Pre-existing modified-file overages (PARTIAL, accepted):** `BayesianClassifierGroup.cs` (515), `FolderScorer.cs` (608), `SortEmail.cs` (1406) — pre-existing; recorded out-of-scope for cycle 1.
- **`FolderHierarchyNode` strict coverage (PARTIAL, accepted):** 60.0% strict / 100.0% inclusive; auto-generated record members; out-of-scope for cycle 1.

### Approved Exceptions
- **None.** No new approved exceptions are claimed.

### Removed/Skipped Tests
- **None.** No tests were removed or skipped in this cycle.

---

## 9. Summary of Changes

### Commits in This PR/Branch (after merge-base `742d4f16`)
1. **0223bc60** - feat(folder-predictor): LCPPN hierarchy model + serialization (#177)
2. **d06f5c00** - feat(folder-predictor): wire flag-gated LCPPN seam + eval harness (#177)
3. **e159bead** - fix(folder-predictor): make flag-on LCPPN path reachable + raise coverage (#177) — cycle-1 remediation

### Files Modified (cycle-1 remediation, commit e159bead)
1. `UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs` (MODIFIED) — adds `IFolderPredictor FolderPredictor { get; set; }`.
2. `TaskMaster/AppGlobals/AppAutoFileObjects.cs` (MODIFIED) — implements `FolderPredictor` auto-property.
3. `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs` (MODIFIED) — removes `_lcppnPredictor`; sets/reads `Globals.AF.FolderPredictor`.
4. `UtilitiesCS.Test/EmailIntelligence/FolderPredictorSeam_Tests.cs` (MODIFIED) — shared-holder mock setup + two F1/AC13 regression tests.
5. `UtilitiesCS.Test/EmailIntelligence/Bayesian/FolderHierarchyTree_Tests.cs` (MODIFIED) — F2 branch coverage tests (now 334 lines).
6. `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Tests.cs` (MODIFIED) — F2 branch coverage tests (now **554 lines — over cap**).

---

## 10. Compliance Verdict

### Overall Status: ⚠️ PARTIALLY COMPLIANT — one FAIL (file-size cap on a new test file)

Remediation cycle 1 successfully resolved both in-scope findings: F1 (flag-on path reachable via the shared AF holder, proven by the fresh-per-call regression test, with flag-off preserved byte-for-byte) and F2 (both target types at ≥ 90% strict, independently re-parsed). Containment held (zero diff on all prohibited files), the toolchain is green in a single final pass, and the repo-wide coverage gate passes with no regression. However, the cycle-1 F2 work pushed the new test file `LcppnFolderPredictor_Tests.cs` to 554 lines, over the 500-line cap — a FAIL-level policy violation that did not exist before this cycle and that AC20 explicitly prohibits.

**Fail-closed reminder:** All required baseline, QA, and coverage-comparison artifacts are present; coverage was independently re-parsed from the canonical artifact.

### Metrics Summary
- ✅ 3904/3904 tests passing
- ✅ Repo-wide line coverage 85.45% strict (≥ 80% floor, +0.14 pp, no regression)
- ✅ F2 target new-code strict coverage 100.00% / 97.71% (≥ 90%)
- ✅ All four C# toolchain steps clean in a single final pass
- ✅ Containment intact (ManagerAsyncLazy.cs and out-of-scope classifiers zero diff; `Manager` value type unchanged)
- ✅ No workflow files modified
- ❌ New test file `LcppnFolderPredictor_Tests.cs` 554 lines (> 500 cap)
- ⚠️ Pre-existing modified-file overages (515/608/1406) — accepted, out-of-scope

### Recommendation

**Needs revision (one FAIL).** Split `LcppnFolderPredictor_Tests.cs` into two cohesive test files each under 500 lines, re-run the C# toolchain, and re-verify F2 coverage is preserved. The F1 and F2 substantive objectives of cycle 1 are met; this is a structural file-size regression introduced by the coverage additions. All other policy dimensions pass.

---

**Audit Completed By:** feature-reviewer agent
**Audit Date:** 2026-06-12 (exit timestamp 2026-06-12T16-35 UTC)
**Policy Version:** Current (as of audit date)

---

## Appendix A: Test Inventory

Cycle-1 regression/coverage tests added to existing registered files:

1. `FolderPredictorSeam_Tests.cs::GetFolderPredictorAsync_FlagOn_ReachableThroughFreshPerCallInstance` — F1 reachability through fresh per-call instances.
2. `FolderPredictorSeam_Tests.cs::GetFolderPredictorAsync_FlagOff_FreshPerCallInstance_ReturnsFlat` — AC13 flag-off byte-for-byte preservation.
3. `FolderHierarchyTree_Tests.cs` — F2 branch tests for `GetChildren`/`NodeKeys`/`GetNode`/`IsLeaf`/`ContainsNode`.
4. `LcppnFolderPredictor_Tests.cs` — F2 branch tests for terminal-leaf/zero-score/abstention/beam-trim/missing-parent UnTrain.

Pre-existing eight feature test files remain registered (see prior-cycle Appendix A).

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
