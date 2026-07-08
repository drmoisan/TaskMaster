# Policy Compliance Audit (Cycle 2 Exit Reaudit): qfc-high-confidence-queue-filter (Issue #218)

---

**Audit Date:** 2026-06-28
**Audit Type:** Remediation cycle 2 exit reaudit
**Base Branch:** `main` (merge-base `1b8536b6e5fb0778aba528caa39853590185bcb7`)
**Head Branch:** `bug/qfc-high-confidence-queue-filter-218` at `27ca7717e7bf020ab5d2b5788fbdad6c1a1d0943`
**Audit Scope:** Full branch diff `git diff main...HEAD` (commits `eac99432`, `b99f0e03`, `2637e4c1`, `27ca7717`), including the maintainer production split `2637e4c1` and the cycle-2 test-split completion `27ca7717`.
**Code Under Test:** `QuickFiler/Controllers/QfcDatamodel.cs` (+ partials `QfcDatamodel.FrameBuilding.cs`, `QfcDatamodel.QueueProcessing.cs`); `QuickFiler/Controllers/QfcHomeController.cs` (+ partials `QfcHomeController.Iteration.cs`, `QfcHomeController.Metrics.cs`); `QuickFiler/Controllers/EmailSorter.cs`; `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs`; `QuickFiler/QuickFiler.csproj`; `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` (+ split files `*RunAsyncTests.cs`, `*IterationTests.cs`, `*MetricsTests.cs`, `*PropertyTests.cs`, `*Issue218Tests.cs`); `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`; `QuickFiler.Test/QuickFiler.Test.csproj`; feature evidence under `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/`.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 16 source/project files | 4270 tests | PASS: 4270 pass, 0 fail | 62.02918410429243% lines (100491 / 162006) | 62.12100678830588% lines (100846 / 162338) | Issue #218 behavior subset 34/34 = 100%; aggregate changed-line 114/272 = 41.91% (driven entirely by pre-existing code relocated by maintainer split `2637e4c1`) |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: N/A - no TypeScript files changed.
- TypeScript post-change coverage artifact: N/A - no TypeScript files changed.
- PowerShell baseline coverage artifact: N/A - no PowerShell files changed.
- PowerShell post-change coverage artifact: N/A - no PowerShell files changed.
- C# baseline coverage artifact: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/baseline/coverage-baseline-218.cobertura.xml`
- C# post-change coverage artifact: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/qa-gates/final-coverage-cycle2-218.cobertura.xml`
- Per-language comparison summary: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/qa-gates/coverage-comparison-cycle2-218.md`

---

## Executive Summary

This is the cycle-2 exit reaudit. The cycle-1 entry audit (`policy-audit.2026-06-26T20-58.md`) recorded three blocking findings: (1) touched C# files exceeding the 500-line limit; (2) changed-production-line coverage not isolated as a numeric percentage; and (3) repository-wide C# line coverage below the 80% threshold. Cycle 2 resolves Finding 1 in full, satisfies the in-scope portion of Finding 2, and dispositions the residual aggregate of Finding 2 and all of Finding 3 as authorized non-blocking exceptions per the cycle-2 remediation inputs.

The audit scope is the full branch diff against `main` (merge-base `1b8536b6`). The cycle-2 diff itself is test-only (no production `.cs` modified by cycle 2); the production structure was completed by the maintainer split commit `2637e4c1`, which is included in the branch diff and therefore in scope for this audit.

Verified results: all eight touched production files and all seven touched test files are now <= 500 lines; the full C# toolchain passes (CSharpier check exit 0 independently re-verified by this reviewer over 1183 files; analyzer build exit 0; nullable build exit 0; MSTest 4270 pass / 0 fail); the issue #218 behavior subset is 100% covered; repo-wide coverage rose +0.0918 pp with no regression. No policy file was modified by the branch.

**Disposition of the three original blocking findings:**
- Finding 1 (file-size 500-line limit): PASS - RESOLVED. Not blocking.
- Finding 2 (changed-production-line coverage): in-scope PASS (issue #218 subset 100%); residual aggregate 41.91% is a non-blocking documented exception (relocated pre-existing code). Not blocking.
- Finding 3 (repo-wide coverage < 80%): non-blocking authority-scoped exception per CLAUDE.md testable-denominator exemption; carries an open dependency (maintainer ratification under `feature/csharp-coverage-uplift`). Not blocking for this bug remediation.

**Blocking findings (FAIL or blocking-PARTIAL): 0.**

**Policy documents evaluated:**
- PASS: `CLAUDE.md`
- PASS: `.claude/rules/general-code-change.md`
- PASS: `.claude/rules/general-unit-test.md`
- PASS: `.claude/rules/csharp.md`
- PASS: `.claude/rules/ci-workflows.md` (no `pwsh` workflow steps changed)
- PASS: `.claude/rules/tonality.md`

**Language-specific policies evaluated:**
- PASS: C# code change and unit test policy.
- N/A: Python, PowerShell, TypeScript, Bash, JSON policies; no files in those languages changed.

**Temporary artifacts cleanup:**
- PASS: No temporary one-time scripts were identified in the branch diff.
- PASS: Review commands did not create tracked source, test, or policy-document changes; working tree is clean.

## Rejected Scope Narrowing

None. The caller prompt defines the audit scope correctly as the full branch diff against `main` (merge-base `1b8536b6`) and does not attempt to narrow scope to a plan, task, phase, or file subset. This audit covers every C# file with changed lines in the branch diff, including the production files relocated by maintainer split `2637e4c1`.

## Evidence Location Compliance

PASS. All evidence artifacts produced for this feature are written under the canonical `<FEATURE>/evidence/<kind>/` path (`docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/{baseline,remediation-baseline,qa-gates,regression-testing,other}/`). A scan of the branch diff found no files written to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. No evidence-location violation was found.

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | PASS | New and moved tests use local mocks, local lists, and per-test setup; the four split test classes each reproduce their own `Setup` scaffolding verbatim. Evidence: `test-split-equivalence-cycle2-218.md`. |
| **Isolation** - Each test targets single behavior | PASS | Issue #218 model tests each target one queue-admission outcome; moved home-controller tests retain their single-behavior scope. |
| **Fast Execution** - Tests complete quickly | PASS | Full MSTest run completed in 42.86 s for 4270 tests. Evidence: `final-mstest-coverage-cycle2-218.md`. |
| **Determinism** - Consistent results | PASS | Tests use Moq seams and do not call live Outlook COM, network, or external services. |
| **Readability & Maintainability** - Clear structure | PASS | Descriptive MSTest method names and Arrange/Act/Assert structure preserved across the split. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | PASS | Baseline C# coverage artifact records 100491 / 162006 lines, line-rate 0.6202918410429243. |
| **No Coverage Regression** | PASS | Post-change C# coverage is 100846 / 162338 lines, line-rate 0.6212100678830588; delta +0.0918 pp. Evidence: `coverage-comparison-cycle2-218.md`. |
| **New Code Coverage >=90%** | PARTIAL (non-blocking) | Issue #218 behavior subset is 34/34 = 100%. Aggregate changed-line coverage is 41.91%, concentrated entirely in pre-existing code mechanically relocated by maintainer split `2637e4c1` (`EmailSorter`, `QfcHomeController.Metrics`, `QfcHomeController.Iteration`); no new behavior is introduced uncovered. See section 8. |
| **Repository-wide Coverage >=80%** | PARTIAL (non-blocking exception) | Raw repo-wide coverage is 62.12100678830588%, below the 80% raw threshold. Dispositioned as an authority-scoped exception under the CLAUDE.md COM/VSTO testable-denominator exemption; pre-existing repo-wide condition, no regression. Requires maintainer ratification under `feature/csharp-coverage-uplift`. Evidence: `repo-wide-coverage-exception-cycle2-218.md`. |
| **Comprehensive Coverage** | PASS | Issue #218 tests cover enabled scoring, equal-threshold admission, below-threshold rejection, disabled behavior, null-item rejection, and initial GUI load ownership. |
| **Positive Flows** - Valid inputs | PASS | Equal-threshold and disabled-mode tests cover admitted flows. |
| **Negative Flows** - Invalid inputs | PASS | Below-threshold and null-mailItem tests verify no add and no hook. |
| **Edge Cases** - Boundary conditions | PASS | Equal-threshold test verifies inclusive cutoff; null-item guard test added in P4-T2. |
| **Error Handling** - Error paths | N/A | Issue #218 introduced no new user-facing error paths; cancellation behavior is existing code. |
| **Concurrency** - If applicable | N/A | Queue admission helper is exercised directly; no concurrent behavior was added. |
| **State Transitions** - If applicable | PASS | Tests verify transition from candidate mail item to queued/hooked or rejected/unhooked. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 62.02918410429243% line coverage. Post-change: 62.12100678830588% line coverage. Change: +0.09182268401345 percentage points. New/changed-code coverage: 100% for the issue #218 behavior subset (34/34 testable changed lines: `QfcRemainingQueueAdmission.cs` 33/33 + `QfcHomeController.cs` 1/1); aggregate changed-line coverage across all eight touched production files is 41.91% (114/272). Disposition: PASS for no regression and PASS for the in-scope issue #218 subset; PARTIAL (non-blocking) for the raw repository-wide 80% threshold and the aggregate 90%-new-code threshold, both dispositioned as authorized exceptions for pre-existing relocated code. Evidence: `coverage-comparison-cycle2-218.md`, `changed-line-coverage-final-cycle2-218.md`, `repo-wide-coverage-exception-cycle2-218.md`, `final-coverage-cycle2-218.cobertura.xml`.

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | PASS | FluentAssertions includes scenario-specific because clauses; moved tests retain original assertions verbatim. |
| **Arrange-Act-Assert Pattern** | PASS | New and moved tests use explicit Arrange, Act, and Assert structure. |
| **Document Intent** | PASS | Test names state the behavior under test; issue #218 tests include summary comments. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | PASS | New tests use mocked `MailItem`, settings, and globals; no live Outlook dependency. |
| **Use Mocks/Stubs** | PASS | Moq is used for Outlook and settings boundaries; internal delegate seams isolate queue add, hook, and scoring. |
| **Environment Stability** | PASS | No temporary files or external services are used by the new or moved tests. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | PASS | This reaudit records the required policy review for the cycle-2 exit of issue #218. |

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | PASS | Objective documented in `issue.md` and the cycle-2 remediation inputs. |
| **Read existing change plans** | PASS | Plan of record: `remediation-plan.2026-06-28T19-14.md`. |
| **Document the plan** | PASS | The cycle-2 plan records all phases complete with per-task evidence. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | PASS | The high-confidence admission decision is centralized in `QfcRemainingQueueAdmission`/`TryQueueRemainingMailItemAsync`. |
| **Reusability** | PASS | Existing `FolderScoringService` is reused for scoring. |
| **Extensibility** | PASS | Internal seams support focused tests without changing `IQfcDatamodel`. |
| **Separation of concerns** | PASS | Queue-admission decision lives in the data-model path; the maintainer split further separated frame-building, queue-processing, iteration, metrics, and sorting into cohesive partials/files. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | PASS | Each extracted file has a single responsibility (frame building, queue processing, iteration, metrics, email sorting, remaining-queue admission). |
| **Under 500 lines** | PASS | Verified on disk: `QfcDatamodel.cs` 432, `QfcDatamodel.FrameBuilding.cs` 154, `QfcDatamodel.QueueProcessing.cs` 146, `EmailSorter.cs` 85, `QfcHomeController.cs` 454, `QfcHomeController.Iteration.cs` 82, `QfcHomeController.Metrics.cs` 226, `QfcRemainingQueueAdmission.cs` 58; tests `QfcHomeControllerTests.cs` 287, `*RunAsyncTests.cs` 448, `*IterationTests.cs` 352, `*MetricsTests.cs` 241, `*PropertyTests.cs` 345, `*Issue218Tests.cs` 219, `QfcDatamodelTests.cs` 177. All <= 500. Evidence: `line-counts-final-cycle2-218.md`, reviewer on-disk `wc -l`. |
| **Public vs internal** | PASS | New seams are internal; no public `IQfcDatamodel` change was introduced. |
| **No circular dependencies** | PASS | No new project dependency or circular reference is introduced. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | PASS | Names such as `TryQueueRemainingMailItemAsync` and `QfcRemainingQueueAdmission` describe intent. |
| **Docs/docstrings** | PASS | No new public API surface requiring XML documentation was added. |
| **Comment why, not what** | PASS | No decorative comments were added; issue #218 intent is documented in tests. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | PASS | Reviewer command: `dotnet tool run csharpier -- check .`; exit code 0; checked 1183 files; working tree clean. Executor evidence: `final-csharpier-cycle2-218.md`. |
| **2. Linting** | PASS | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; exit code 0. Evidence: `final-analyzer-build-cycle2-218.md`. |
| **3. Type checking** | PASS | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`; exit code 0. Evidence: `final-nullable-build-cycle2-218.md`. |
| **4. Testing** | PASS | MSTest 4270 total, 4270 passed, 0 failed. Evidence: `final-mstest-coverage-cycle2-218.md`. |
| **Full toolchain loop** | PASS | CSharpier (with one CRLF normalization restart), analyzer build, nullable build, MSTest coverage, Cobertura conversion, and comparison passed in order. |
| **Explicit reporting** | PASS | Commands and results are documented in this audit and under `evidence/qa-gates/`. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | PASS | Cycle-2 plan and evidence summarize the test split, banned-API sweep, and coverage dispositions. |
| **Design choices explained** | PASS | Evidence records the verification-only treatment of the maintainer production split and the completion of the test split. |
| **Update supporting documents** | PASS | Issue, plan, remediation inputs, and evidence were updated for cycle 2. |
| **Provide next steps** | PASS | Open follow-ups documented: maintainer ratification of the repo-wide coverage exception under `feature/csharp-coverage-uplift`; banned-API time-seam migration when RS0030 is promoted to `warning`. |

## 3. Language-Specific Code Change Policy Compliance

### Section 3CSharp: C# Code Change Policy Compliance

#### 3CSharp.1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with CSharpier** | PASS | Reviewer `csharpier -- check .` exit 0 over 1183 files. |
| **Linting with .NET analyzers** | PASS | Analyzer build exit 0; one pre-existing suggestion-level `MSTEST0032` in an untouched file, non-build-breaking. |
| **Nullable analysis** | PASS | Nullable build with `TreatWarningsAsErrors=true` exit 0. |
| **Testing with MSTest coverage** | PASS | 4270 tests passed. |
| **Coverage threshold** | PARTIAL (non-blocking exception) | Repo-wide raw line coverage remains below 80%; authority-scoped exception documented; no regression. |

#### 3CSharp.2 C# Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Strong contracts and explicit APIs** | PASS | Public interface unchanged; new seams are internal. |
| **Null-safety by default** | PASS | Nullable build passed with warnings as errors. |
| **Prefer composition and focused types** | PASS | Oversized files were decomposed into cohesive single-responsibility files, all <= 500 lines. |
| **Asynchrony and resource safety** | PASS | Scoring remains asynchronous and uses `ConfigureAwait(false)` in helper methods. |

#### 3CSharp.3 C# Error Handling

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Specific exceptions** | PASS | Queue helper preserves cancellation propagation and existing logging path. |
| **Logging over console** | PASS | No production `Console.WriteLine` was added. |
| **Invariants at construction** | N/A | No new public constructor invariants were added. |

#### 3CSharp.4 Banned-API Analyzer (RS0030 / BannedApiAnalyzers)

| Requirement | Status | Evidence |
|------------|--------|----------|
| **No new banned-API usage introduced** | PASS | The branch introduces zero new banned-API call sites. The 8 active `DateTime.Now`/`Task.Delay` sites in the new partials were verified verbatim on `main` in the original `QfcHomeController.cs` and `QfcDatamodel.cs` (reviewer `git show main:...` diff) and were mechanically relocated by split `2637e4c1`. RS0030 is held at `suggestion` severity per `.claude/rules/csharp.md`. Deferral is policy-conformant. Evidence: `banned-api-sweep-cycle2-218.md`. |

## 4. Language-Specific Unit Test Policy Compliance

### Section 4CSharp: C# Unit Test Policy Compliance

#### 4CSharp.1 Framework and Scope

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use MSTest** | PASS | All tests use `[TestClass]`/`[TestMethod]`. |
| **Use Moq** | PASS | Outlook and settings boundaries are mocked with Moq. |
| **Prefer FluentAssertions** | PASS | Assertions use FluentAssertions. |
| **Coverage expectation** | PARTIAL (non-blocking exception) | Repo-wide raw coverage below 80%; no regression; authority-scoped exception. |

#### 4CSharp.2 Test Style and Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Focused unit tests** | PASS | Each test covers one behavior; the split preserved names and bodies. |
| **Mocking external boundaries** | PASS | Outlook COM objects are mocked; queue add/hook and scoring use internal seams. |
| **Organization** | PASS | Tests are organized into cohesive `QfcHomeController*Tests.cs` files, all <= 500 lines and wired into `QuickFiler.Test.csproj`. |

#### 4CSharp.3 Running the Toolchain

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use MSTest/VSTest** | PASS | `vstest.console.exe ... /EnableCodeCoverage`. Evidence: `final-mstest-coverage-cycle2-218.md`. |
| **No alternative test runners** | PASS | No xUnit or NUnit usage was introduced. |

## 5. Test Coverage Detail

### Test-split integrity (cycle-2 structural work)

| Check | Result | Status |
|-------|--------|--------|
| Every `QfcHomeController*Tests.cs` file <= 500 lines | Largest 448 (RunAsync) | PASS |
| Duplicate `[TestMethod]` definitions across compiled suite | 0 | PASS |
| Compiled active `[TestMethod]` count preserved | 32 (Tests 3 + RunAsync 6 + Iteration 6 + Metrics 2 + Property 13 + Issue218 2) | PASS |
| 27 moved tests name+body equivalence vs canonical originals | 27/27 EQUIVALENT | PASS |
| Four split files wired into `QuickFiler.Test.csproj` | All four present (reviewer-verified lines 72-75) | PASS |
| `QfcFormViewerDerived` disposition | Retained in residual file; provably unreferenced | PASS |

Evidence: `test-split-verification-cycle2-218.md`, `test-split-equivalence-cycle2-218.md`, `test-split-build-cycle2-218.md`.

### Issue #218 focused behavior

| Test | Status |
|------|--------|
| `TryQueueRemainingMailItemAsync_HighConfidenceEnabled_ScoresBeforeQueueAdmission` | PASS |
| `TryQueueRemainingMailItemAsync_ScoreEqualsThreshold_AddsAndHooksMailItem` | PASS |
| `TryQueueRemainingMailItemAsync_ScoreBelowThreshold_DoesNotAddOrHookMailItem` | PASS |
| `TryQueueRemainingMailItemAsync_HighConfidenceDisabled_AddsAndHooksWithoutScoring` | PASS |
| `TryQueueRemainingMailItemAsync_NullMailItem_DoesNotScoreAddOrHook` (new, P4-T2) | PASS |
| `RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch` | PASS |
| `RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter` | PASS |

Evidence: `focused-pass-after-cycle2-218.md` (7/7 pass).

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 4270 | PASS |
| Tests Passed | 4270 (100%) | PASS |
| Tests Failed | 0 | PASS |
| Execution Time | 42.86 s | PASS |
| Functions/Classes Tested | Issue #218 queue admission and initial-load paths covered | PASS |
| Test File Size | All `QfcHomeController*Tests.cs` and `QfcDatamodelTests.cs` <= 500 lines | PASS |
| Code Coverage | 62.12100678830588% C# repo-wide line coverage | PARTIAL (non-blocking exception) |

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `dotnet tool run csharpier -- check .` | Exit code 0; checked 1183 files | PASS |
| NET Analyzer Build | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Exit code 0 | PASS |
| Nullable Build | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` | Exit code 0 | PASS |
| MSTest Coverage | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation` | 4270 passed, 0 failed | PASS |
| Coverage Threshold | Final Cobertura artifacts | No regression (+0.0918 pp); repo-wide raw coverage below 80% (authority-scoped exception) | PARTIAL |

**Notes:**
No Python, PowerShell, TypeScript, Bash, or JSON checks were in scope for changed files.

## 8. Gaps and Exceptions

### Identified Gaps (all non-blocking)

1. **Repository-wide C# line coverage below the 80% raw threshold.**
   - Evidence: `repo-wide-coverage-exception-cycle2-218.md`; raw 62.12100678830588% (100846/162338), 17.879 pp below 80%.
   - Disposition: authority-scoped exception under the CLAUDE.md COM/VSTO testable-denominator exemption. Pre-existing repo-wide condition; no regression (+0.0918 pp). A single bug remediation cannot and should not close an 18-point raw shortfall, and the cycle-2 inputs prohibit raising coverage with out-of-scope tests.
   - Open dependency: maintainer ratification under `feature/csharp-coverage-uplift`. The exception is documented but not yet ratified.
   - Reviewer note: the exception evidence states the raw figure and the qualitative exemption basis but does not compute an explicit testable-denominator coverage percentage. The qualitative basis is accepted for this bug remediation; a computed testable-denominator figure should accompany the maintainer ratification.

2. **Aggregate changed-line coverage 41.91% (< 90%).**
   - Evidence: `changed-line-coverage-final-cycle2-218.md`; 114/272 coverable changed lines covered.
   - Disposition: the entire shortfall is in pre-existing code mechanically relocated by maintainer split `2637e4c1`, not in issue #218 behavior. The issue #218 behavior subset is 34/34 = 100%. Reviewer-verified provenance: `EmailSorter` existed on `main` inside `QfcDatamodel.cs` (line 686, identical `GetSortKey`); the `QfcHomeController.Metrics`/`Iteration` and banned-API lines existed verbatim on `main`. No new behavior is introduced uncovered.
   - Reviewer correction to the exemption rationale: the evidence groups `EmailSorter.cs` (49 uncovered) under the COM/VSTO exemption, but `EmailSorter` is pure, testable logic (sort-key arithmetic and a triage dictionary; it takes a `DateTime` parameter and has no Outlook-Interop dependency). It is therefore NOT properly COM-exempt. The correct basis for `EmailSorter` is "relocated pre-existing untested code, out of scope for issue #218." `QfcHomeController.Metrics.cs` (98 uncovered) IS genuinely Outlook-Interop-bound (`AppointmentItem`, `Calendar`, `Session`, `Folders`) and is correctly exemptable. The net disposition (non-blocking; no new uncovered behavior introduced by #218) is unchanged, but the `EmailSorter` exemption rationale is overstated and the latent gap should be tracked alongside the coverage-uplift work.

3. **Eight deferred banned-API sites (`DateTime.Now`, `Task.Delay`).**
   - Evidence: `banned-api-sweep-cycle2-218.md`.
   - Disposition: all pre-existing, carried verbatim by split `2637e4c1`; branch introduces none. RS0030 held at `suggestion` per `.claude/rules/csharp.md`, which classifies legacy call-site migration as follow-up work. Non-blocking. Follow-up: migrate to the `System.TimeProvider` seam when RS0030 is promoted to `warning`.

### Approved Exceptions

The cycle-2 remediation inputs (`remediation-inputs.2026-06-28T19-14.md`) pre-authorized handling Finding 3 (repo-wide coverage) as a documented authority-scoped exception rather than fixing it in scope, and prohibited raising coverage with out-of-scope tests. The exception is recorded but requires maintainer ratification under `feature/csharp-coverage-uplift` to be fully closed.

### Removed/Skipped Tests

None. No test was removed or weakened. The 27 moved tests are name- and body-equivalent to their canonical originals; the compiled active count rose from cycle-entry 4269 to 4270 by the single new null-mailItem admission test.

## 9. Summary of Changes

### Commits in This Branch (main..HEAD)

1. `eac99432` - `fix(qfc): move high-confidence filtering into QfcDatamodel`
2. `b99f0e03` - `refactor(qfc): extract remaining queue admission`
3. `2637e4c1` - `refactor(qfc): split oversized controllers to meet 500-line limit` (maintainer production split)
4. `27ca7717` - `test(qfc): complete oversized test-file split for issue #218 (cycle 2)`

### Files Modified (production)

1. `QuickFiler/Controllers/QfcDatamodel.cs` (432) + `QfcDatamodel.FrameBuilding.cs` (154) + `QfcDatamodel.QueueProcessing.cs` (146) - high-confidence admission moved into the data-model path; oversized file decomposed.
2. `QuickFiler/Controllers/QfcHomeController.cs` (454) + `QfcHomeController.Iteration.cs` (82) + `QfcHomeController.Metrics.cs` (226) - initial-load prefilter ownership removed; oversized file decomposed.
3. `QuickFiler/Controllers/EmailSorter.cs` (85) - relocated pre-existing sorter class.
4. `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` (58) - extracted admission seam.
5. `QuickFiler/QuickFiler.csproj` - new production files wired.

### Files Modified (tests)

6. `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` (287, trimmed) + four split files (`*RunAsyncTests.cs` 448, `*IterationTests.cs` 352, `*MetricsTests.cs` 241, `*PropertyTests.cs` 345) + `*Issue218Tests.cs` (219).
7. `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` (177) - issue #218 admission tests incl. new null-mailItem test.
8. `QuickFiler.Test/QuickFiler.Test.csproj` - all split/new test files wired.

## 10. Compliance Verdict

### Overall Status: COMPLIANT WITH DOCUMENTED NON-BLOCKING EXCEPTIONS

All three original blocking findings are resolved or dispositioned to an authorized non-blocking state. Finding 1 (file-size) is fully resolved and verified on disk. Finding 2 (changed-line coverage) is satisfied for the in-scope issue #218 behavior subset (100%); the residual aggregate is a documented non-blocking exception for relocated pre-existing code. Finding 3 (repo-wide coverage) is a documented authority-scoped exception with no regression, carrying an open dependency on maintainer ratification. The full C# toolchain passes and no policy file was modified.

**Blocking findings (FAIL or blocking-PARTIAL): 0.**

### Itemized FAIL / blocking-PARTIAL findings

None.

### Itemized non-blocking PARTIAL findings (not counted toward blocking_count)

1. Repo-wide coverage 62.12% < 80% raw - authority-scoped exception, unratified (open dependency: `feature/csharp-coverage-uplift`).
2. Aggregate changed-line coverage 41.91% < 90% - relocated pre-existing code; `EmailSorter` exemption rationale overstated (correction recorded); no new uncovered behavior.
3. Eight deferred banned-API sites - pre-existing, RS0030 at suggestion severity; follow-up time-seam migration.

### Recommendation

**Go for this bug remediation cycle, conditional on maintainer ratification of the repo-wide coverage exception.** The three cycle-1 blocking findings are cleared to an authorized non-blocking disposition; the issue #218 behavior is fully covered and all acceptance criteria pass. The repo-wide coverage exception and the banned-API/`EmailSorter`-coverage follow-ups should be tracked under `feature/csharp-coverage-uplift` and a time-seam migration cycle, respectively, and are outside the scope of this bug fix.

---

## Appendix A: Test Inventory

### Issue #218 focused tests

- `QuickFiler.Controllers.Tests.QfcDatamodelTests.TryQueueRemainingMailItemAsync_HighConfidenceEnabled_ScoresBeforeQueueAdmission`
- `QuickFiler.Controllers.Tests.QfcDatamodelTests.TryQueueRemainingMailItemAsync_ScoreEqualsThreshold_AddsAndHooksMailItem`
- `QuickFiler.Controllers.Tests.QfcDatamodelTests.TryQueueRemainingMailItemAsync_ScoreBelowThreshold_DoesNotAddOrHookMailItem`
- `QuickFiler.Controllers.Tests.QfcDatamodelTests.TryQueueRemainingMailItemAsync_HighConfidenceDisabled_AddsAndHooksWithoutScoring`
- `QuickFiler.Controllers.Tests.QfcDatamodelTests.TryQueueRemainingMailItemAsync_NullMailItem_DoesNotScoreAddOrHook`
- `QuickFiler.Controllers.Tests.QfcHomeControllerIssue218Tests.RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch`
- `QuickFiler.Controllers.Tests.QfcHomeControllerIssue218Tests.RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter`

Full suite evidence: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/qa-gates/final-mstest-coverage-cycle2-218.md` (4270 passed, 0 failed).

## Appendix B: Toolchain Commands Reference

**For C#:**
```powershell
dotnet tool run csharpier -- check .
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation /ResultsDirectory:TestResults\issue218-remediation-cycle2-final
```

---

**Audit Completed By:** feature-reviewer
**Audit Date:** 2026-06-28
**Policy Version:** Current as of audit date
