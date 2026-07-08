# Policy Compliance Audit: qfc-form-viewer-testability (Issue #223)

**Audit Date:** 2026-06-28
**Code Under Test:** C# (15 `.cs` + 2 `.csproj`). Production: `QuickFiler/Controllers/QfcFormController.cs`, `QfcFormController.Actions.cs` (NEW), `QfcFormController.EventHandlers.cs` (NEW), `QfcFormController.SetupDisposal.cs` (NEW), `QfcFormKeyHandler.cs` (NEW), `QfcCollectionController.cs`, `QfcHomeController.cs`, `Interfaces/IQfcFormViewer.cs`, `Viewers/QfcFormViewer.cs`, `Viewers/QfcFormViewerDark.cs`, `Viewers/QfcFormViewerExpanded.cs`, `QuickFiler/QuickFiler.csproj`. Tests: `QfcFormControllerSeamTests.cs` (NEW), `QfcFormKeyHandlerTests.cs` (NEW), `QfcFormControllerTests.cs`, `QfcHomeControllerRunAsyncTests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`.

**Base branch:** `main` (resolved `origin/main`). **Merge-base:** `86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a`. **Head:** `e91927105abde2ceadd10a7011bc17d714108afd`. Scope is the full branch diff against the merge-base (46 files, +2278 / -992).

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 15 `.cs` + 2 `.csproj` | 196 tests | ✅ 196 pass, 0 fail | 39.24% lines (QfcFormController, changed-type baseline); repo-wide first-party not measured | 51.86% lines (QfcFormController) | 100% (QfcFormKeyHandler 2/2) |
| Python | 0 files | N/A | N/A | N/A (no changed files) | N/A (no changed files) | N/A |
| PowerShell | 0 files | N/A | N/A | N/A (no changed files) | N/A (no changed files) | N/A |
| TypeScript | 0 files | N/A | N/A | N/A (no changed files) | N/A (no changed files) | N/A |

**Note:** C# is the only language with changed files on the branch. Python, PowerShell, TypeScript, Bash, and JSON have zero changed files and are correctly N/A.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope` (no TypeScript files changed)
- TypeScript post-change coverage artifact: `N/A - out of scope` (no TypeScript files changed)
- PowerShell baseline coverage artifact: `N/A - out of scope` (no PowerShell files changed)
- PowerShell post-change coverage artifact: `N/A - out of scope` (no PowerShell files changed)
- C# baseline coverage evidence: `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/evidence/baseline/baseline-tests-coverage.2026-06-28T20-52.md`
- C# post-change coverage evidence: `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/evidence/qa-gates/final-tests-coverage.2026-06-28T20-52.md` and `evidence/regression-testing/coverage-delta.2026-06-28T20-52.md`
- C# canonical machine-readable coverage artifact (`artifacts/csharp/coverage.xml`): **MISSING** (see Section 1.2 and Section 8)
- Per-language comparison summary: Section 1.2.1 below

**Non-negotiable verdict rule:** No policy audit may report PASS unless it includes numeric baseline and post-change coverage metrics for every language in scope, plus changed/new-code coverage when required.

**Fail-closed rule:** If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the verdict must be BLOCKED or INCOMPLETE, never PASS.

---

## Rejected Scope Narrowing

No caller instruction attempted to narrow the audit scope to a plan subset, a file subset, or to mark any language's coverage as out of scope. The caller's "Context the audit should be aware of" block was explicitly framed as "assess independently; do not treat as a scope limit," and was treated accordingly. The full feature-vs-base diff (46 files) was audited. No narrowing to record.

---

## Evidence Location Compliance

All executor-produced evidence artifacts are written under the canonical `<FEATURE>/evidence/<kind>/` path (`baseline/`, `qa-gates/`, `regression-testing/`, `other/`). A scan of the branch diff for files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` returned zero matches. **Verdict: PASS** — no evidence-location violations.

---

## Executive Summary

This is a C# WinForms testability refactor that narrows `IQfcFormViewer` to 23 intent-level members across four seams (A–D) and splits the 1142-line `QfcFormController.cs` into four partial-class files to satisfy the 500-line cap before adding code. The structural refactor is well-executed: the interface no longer exposes raw clickable control types, pure Alt-key routing logic is extracted to a testable static (`QfcFormKeyHandler.IsAltKeyCommand`), and new MSTest coverage exercises command-event routing, skip-flow state, and the `CaptureItemSettings` populated/null paths through `Mock<IQfcFormViewer>`.

The four executor toolchain gates (csharpier, .NET analyzers, nullable/TreatWarningsAsErrors, MSTest) each recorded `EXIT_CODE: 0`, and an independent CSharpier check on the four most-changed C# files returned exit 0. New-code coverage (`QfcFormKeyHandler` 100%) and changed-type no-regression (`QfcFormController` +12.62pp) are evidenced.

One blocking coverage gap exists: the canonical machine-readable C# coverage artifact (`artifacts/csharp/coverage.xml`) is absent, and no measured repo-wide first-party (testable-denominator) coverage figure exists — the only repo-wide number recorded is the single-assembly process-wide 12.86%, which the executor itself disclaims as not the policy gate. Under the workflow's mandatory-coverage rule, an absent canonical coverage artifact for a language with changed files is a FAIL and a remediation trigger. Two pre-existing 500-line-cap files (`QfcCollectionController.cs` 2296 lines, `[ExcludeFromCodeCoverage]`; `QfcFormControllerTests.cs` 821 lines) are touched with net-negative/net-neutral edits and are recorded as accepted pre-existing-debt dispositions (non-blocking PARTIAL observations).

**Policy documents evaluated:**
- ✅ `CLAUDE.md` and `.claude/rules/general-code-change.md`
- ✅ `.claude/rules/general-unit-test.md`

**Language-specific policies evaluated:**
- ✅ C#: `.claude/rules/csharp.md` (C# Code Change Policy + C# Unit Test Policy)
- N/A `python` (no changed files)
- N/A `powershell` (no changed files)
- N/A Bash / JSON (no changed files)

**Temporary artifacts cleanup:**
- ✅ No temporary or one-time scripts were created by this review.
- ✅ Review is read-only against source; no source or policy files were modified.

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | ✅ PASS | New tests (`QfcFormKeyHandlerTests`, `QfcFormControllerSeamTests`) construct fresh `Mock<IQfcFormViewer>` per test; no shared mutable static state observed. |
| **Isolation** - Each test targets single behavior | ✅ PASS | `QfcFormKeyHandlerTests` has one assertion theme per `[TestMethod]` (Alt, Alt+Left, Control, None); seam tests isolate a single event-routing or state-transition path each. |
| **Fast Execution** - Tests complete quickly | ✅ PASS | 196 tests run under vstest with `/InIsolation`; no network or disk I/O in the changed tests. |
| **Determinism** - Consistent results | ✅ PASS | No `DateTime.Now`/`Random`/network/temp-file usage in changed tests (grep clean). Moq-driven event raising is deterministic. |
| **Readability & Maintainability** - Clear structure | ✅ PASS | Descriptive `[TestMethod]` names; Arrange-Act-Assert structure with FluentAssertions. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | ✅ PASS | **Baseline (pre-development):** QfcFormController 39.24% lines (301/767).<br>**Command:** `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`<br>**Timestamp:** 2026-06-28T20-52<br>Source: `evidence/baseline/baseline-tests-coverage.2026-06-28T20-52.md`. |
| **No Coverage Regression** | ✅ PASS | **Post-change coverage:** QfcFormController 51.86% lines (363/700).<br>**Change:** +12.62 percentage points.<br>**Status:** No regression (denominator decreased because Seam D moved a ~58-line construction block into `[ExcludeFromCodeCoverage]` Form code; covered lines rose 301→363). Source: `evidence/regression-testing/coverage-delta.2026-06-28T20-52.md`. |
| **New Code Coverage ≥90%** | ✅ PASS | **New file:** `QfcFormKeyHandler.cs`.<br>**New code coverage:** 100% (2/2 lines).<br>**Calculation method:** dotnet-coverage merged Cobertura keyed by (filename, line). Source: coverage-delta. |
| **Repo-wide ≥80% (testable denominator)** | ❌ FAIL | No measured repo-wide first-party coverage figure exists. The only repo-wide number recorded is single-assembly process-wide **12.86%** (9800/76203), which the executor explicitly disclaims as instrumenting all loaded modules and not the policy gate. The canonical machine-readable artifact `artifacts/csharp/coverage.xml` is **absent**. Coverage verification of the repo-wide floor is therefore unverified; per the workflow's mandatory-coverage rule this is a FAIL and a remediation trigger. |
| **Comprehensive Coverage** | ✅ PASS | New seam tests cover `IsAltKeyCommand` (4 cases), command-event routing (Ok/Cancel/Undo/Skip/ItemsPerLoad), skip-flow `SkipButtonText`/`SkipButtonEnabled`, and `CaptureItemSettings` populated/null paths. |
| **Positive Flows** - Valid inputs | ✅ PASS | Event-routing tests raise each command event via Moq and `Verify` the controller method executes. |
| **Negative Flows** - Invalid inputs | ✅ PASS | Null `CaptureTlpCellStates()` and null-RowStyles early-return paths tested per seam test file. |
| **Edge Cases** - Boundary conditions | ✅ PASS | `IsAltKeyCommand` boundary combinations (`Keys.Alt`, `Keys.Alt \| Keys.Left`, `Keys.Control`, `Keys.None`). |
| **Error Handling** - Error paths | ✅ PASS | Skip-flow and capture null paths assert intended fallbacks rather than throwing. |
| **Concurrency** - If applicable | N/A | Refactor introduces no new concurrency. |
| **State Transitions** - If applicable | ✅ PASS | Skip-flow state transitions verified via `VerifySet` on `SkipButtonText`/`SkipButtonEnabled`. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 39.24% lines -> Post-change: 51.86% lines. Change: +12.62% lines. New/changed-code coverage: 100%. Disposition: FAIL. Evidence: `evidence/regression-testing/coverage-delta.2026-06-28T20-52.md`.
- TypeScript: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (no TypeScript files changed).
- PowerShell: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (no PowerShell files changed).
- Python: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (no Python files changed).

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | ✅ PASS | FluentAssertions used throughout new tests, producing descriptive failure output. |
| **Arrange-Act-Assert Pattern** | ✅ PASS | Each new `[TestMethod]` follows Arrange (mock setup) / Act (event raise or call) / Assert (Verify/Should). |
| **Document Intent** | ✅ PASS | Test method names describe scenario and expected behavior. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | ✅ PASS | No databases, networks, processes, or filesystem access in changed tests. |
| **Use Mocks/Stubs** | ✅ PASS | `Mock<IQfcFormViewer>` (Moq) isolates the Form boundary; event routing exercised via Moq `Raise`. |
| **Environment Stability** | ✅ PASS | No temp files (grep for `GetTempPath`/`GetTempFileName`/`File.WriteAll`/`FileStream` clean); no mutable global state. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | ✅ PASS | This document is the required policy review. One outstanding item: C# coverage artifact (Section 8). |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | ✅ PASS | Objective stated in `issue.md`/`spec.md` (#223): maximize unit testability via Passive-View MVP interface narrowing. |
| **Read existing change plans** | ✅ PASS | `plan.2026-06-28T20-20.md` present and followed (Phase 0 split + Seams A–D). |
| **Document the plan** | ✅ PASS | Atomic plan and per-phase evidence committed under `evidence/`. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | ✅ PASS | `IsAltKeyCommand` is a one-line pure predicate; seams expose intent members rather than raw controls. |
| **Reusability** | ✅ PASS | `IsAltKeyCommand` shared across all three form variants' `ProcessCmdKey`. |
| **Extensibility** | ✅ PASS | Command events / state properties allow controllers to evolve without coupling to WinForms control types. |
| **Separation of concerns** | ✅ PASS | Pure routing logic separated from Form; Form-bound code stays `[ExcludeFromCodeCoverage]`. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | ✅ PASS | `QfcFormController` split into SetupDisposal / EventHandlers / Actions partials by responsibility region. |
| **Under 500 lines** | ⚠️ PARTIAL | Files modified-and-grown this cycle are all < 500 (QfcFormController.cs 195, .Actions 311, .EventHandlers 399, .SetupDisposal 232, QfcFormKeyHandler 20, IQfcFormViewer 51, QfcFormViewer 262, Dark/Expanded 55, QfcHomeController 454, seam test 326). Two pre-existing cap violations remain: `QfcCollectionController.cs` 2296 (baseline 2299, net -3, `[ExcludeFromCodeCoverage]`) and `QfcFormControllerTests.cs` 821 (baseline 823, net -2). Both are accepted pre-existing-debt dispositions (net-negative; not blocking). See Section 8. |
| **Public vs internal** | ✅ PASS | `QfcFormKeyHandler` is `internal static`; partials are `internal partial class`. Interface remains `public` (consumed cross-assembly). |
| **No circular dependencies** | ✅ PASS | Seam direction is controller -> interface -> Form; no new cycles introduced. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | ✅ PASS | `IsAltKeyCommand`, `SwapItemTableLayout`, `CaptureTlpCellStates`, `GetKeyEventExclusionControls` are intent-revealing. |
| **Docs/docstrings** | ✅ PASS | `QfcFormKeyHandler` carries XML doc on class and method; interface members carry seam-rationale comments. |
| **Comment why, not what** | ✅ PASS | Interface comments explain the seam motivation (e.g., setter removed by Seam C). |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ✅ PASS | **Command:** `dotnet tool run csharpier check .`<br>**Result:** EXIT_CODE 0 (executor `evidence/qa-gates/final-csharpier`); reviewer re-ran `csharpier check` on 4 key files → exit 0. |
| **2. Linting** | ✅ PASS | **Command:** `msbuild TaskMaster.sln -t:Build ... -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`<br>**Result:** EXIT_CODE 0 (`evidence/qa-gates/final-analyzers`). |
| **3. Type checking** | ✅ PASS | **Command:** `msbuild TaskMaster.sln -t:Build ... -p:Nullable=enable -p:TreatWarningsAsErrors=true`<br>**Result:** EXIT_CODE 0 (`evidence/qa-gates/final-nullable`). |
| **4. Testing** | ✅ PASS | **Command:** `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`<br>**Result:** 196 passed, 0 failed (`evidence/qa-gates/final-tests-coverage`). |
| **Full toolchain loop** | ✅ PASS | Per-phase (p1/p2/p3) and final gate evidence all EXIT_CODE 0. |
| **Explicit reporting** | ✅ PASS | Commands and results recorded in executor evidence and this audit. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | ✅ PASS | `ac-traceability.2026-06-28T20-52.md` maps AC1–AC7 to tasks and evidence. |
| **Design choices explained** | ✅ PASS | Seam rationale documented in `spec.md` and research docs. |
| **Update supporting documents** | ✅ PASS | issue/spec/plan and evidence committed. |
| **Provide next steps** | ⚠️ PARTIAL | Toolchain complete; outstanding next step is to produce the canonical C# coverage artifact / repo-wide measurement (Section 8). |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3C#: C# Code Change Policy Compliance

#### 3C#.1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with CSharpier** | ✅ PASS | `dotnet tool run csharpier check .` EXIT_CODE 0; independent reviewer check of 4 files exit 0. |
| **Linting with .NET analyzers** | ✅ PASS | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` EXIT_CODE 0. |
| **Type checking with nullable analysis** | ✅ PASS | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` EXIT_CODE 0. |
| **Testing with MSTest** | ✅ PASS | `vstest.console.exe ... /EnableCodeCoverage` 196/196 pass. |

#### 3C#.2 C# Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Strong contracts / explicit APIs** | ✅ PASS | `IQfcFormViewer` exposes typed intent members (events, `decimal ItemsPerLoadValue`, `Padding ItemViewerTemplateMargin`, `IReadOnlyList<Control>`). |
| **Null-safety by default** | ✅ PASS | Nullable build passes with `TreatWarningsAsErrors`. |
| **Composition / focused types** | ✅ PASS | Partial-class split keeps each file scoped to one responsibility region. |
| **Async / resource safety** | N/A | No new async or disposable resources introduced by the seams. |

#### 3C#.3 Error Handling, Naming, Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Fail-fast exceptions** | ✅ PASS | No new broad catches introduced; behavior preserved. |
| **PascalCase/camelCase conventions** | ✅ PASS | Types/members PascalCase; locals/fields camelCase. |
| **`internal` for non-public APIs** | ✅ PASS | `QfcFormKeyHandler` and controller partials are `internal`. |

---

## 4. Language-Specific Unit Test Policy Compliance

### Section 4C#: C# Unit Test Policy Compliance

#### 4C#.1 Framework and Scope

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use MSTest** | ✅ PASS | `using Microsoft.VisualStudio.TestTools.UnitTesting; [TestClass]/[TestMethod]` in both new test files. No xUnit/NUnit (grep clean). |
| **Use Moq** | ✅ PASS | `using Moq;` with `Mock<IQfcFormViewer>`, `Raise`, `Verify`, `VerifySet`. |
| **Prefer FluentAssertions** | ✅ PASS | `using FluentAssertions;` in both new test files. |
| **Coverage expectation** | ⚠️ PARTIAL | New code 100% and changed-type no-regression PASS; repo-wide >= 80% floor unverified (Section 1.2 FAIL). |

#### 4C#.2 Test Style, Naming, Toolchain

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Focused unit tests** | ✅ PASS | One behavior per `[TestMethod]`. |
| **Mocking sparingly** | ✅ PASS | Only the Form boundary mocked. |
| **Naming/readability** | ✅ PASS | Descriptive method names; AAA structure. |
| **No alternative test runners** | ✅ PASS | MSTest only. |

---

## 5. Test Coverage Detail

### QfcFormKeyHandler (1 test class, 4 tests)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| IsAltKeyCommand with Keys.Alt | Positive | 18 | ✅ |
| IsAltKeyCommand with Keys.Alt \| Keys.Left | Edge Case | 18 | ✅ |
| IsAltKeyCommand with Keys.Control | Negative | 18 | ✅ |
| IsAltKeyCommand with Keys.None | Negative | 18 | ✅ |

**Coverage:** 100% of `QfcFormKeyHandler` (2/2 instrumented lines).

**Not covered:** None.

### QfcFormController seam behavior (QfcFormControllerSeamTests, 11 tests)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| Command-event routing (Ok/Cancel/Undo/Skip) | Positive | event handler bodies | ✅ |
| ItemsPerLoadValueChanged routing | Positive | spinner handler | ✅ |
| Skip-flow toggles SkipButtonText/SkipButtonEnabled | State Transition | skip handler | ✅ |
| CaptureItemSettings with populated CaptureTlpCellStates | Positive | CaptureItemSettings | ✅ |
| CaptureItemSettings with null CaptureTlpCellStates | Negative | CaptureItemSettings null path | ✅ |
| RegisterFormEventHandlers uses exclusion controls | Positive | RegisterFormEventHandlers | ✅ |

**Coverage:** QfcFormController changed-type 51.86% (363/700), +12.62pp vs baseline; Form-bound members remain `[ExcludeFromCodeCoverage]`.

**Not covered:** Form-derived and Designer code (formally exempt per repo COM/VSTO/WinForms exemption).

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 196 | ✅ |
| Tests Passed | 196 (100%) | ✅ |
| Tests Failed | 0 | ✅ |
| Execution Time | not separately recorded (single vstest run) | ✅ |
| Functions/Classes Tested | QfcFormKeyHandler + QfcFormController seam paths | ✅ |
| Test File Size | seam tests 326 lines; key-handler tests 67 lines | ✅ |
| Code Coverage (changed type) | 51.86% lines (QfcFormController); new code 100% | ⚠️ (repo-wide floor unverified) |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `dotnet tool run csharpier check .` | EXIT_CODE 0 | ✅ |
| .NET Analyzers | `msbuild TaskMaster.sln -t:Build -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true` | EXIT_CODE 0 | ✅ |
| Nullable Type Check | `msbuild TaskMaster.sln -t:Build -p:Nullable=enable -p:TreatWarningsAsErrors=true` | EXIT_CODE 0 | ✅ |
| MSTest Tests | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage` | 196 pass 0 fail | ✅ |

**Notes:**
The four toolchain results above are verified from executor evidence artifacts (`evidence/qa-gates/final-*`), each recording EXIT_CODE 0. The reviewer independently re-ran CSharpier check on the four most-changed C# files (exit 0). msbuild/vstest were not reproduced locally (msbuild is not on the bash PATH in this environment); their PASS status rests on the executor evidence, which is the workflow-sanctioned evidence-verification model.

---

## 8. Gaps and Exceptions

### Identified Gaps

- **C# coverage artifact missing (BLOCKING).** The canonical machine-readable artifact `artifacts/csharp/coverage.xml` does not exist. Coverage evidence exists only as narrative markdown (`coverage-delta`, `final-tests-coverage`). No measured repo-wide first-party (testable-denominator) coverage figure exists; the only repo-wide number is the disclaimed single-assembly process-wide 12.86%. Per the feature-review-workflow mandatory-coverage rule ("If no coverage artifact exists for a language that has changed files, flag as FAIL"), this is a FAIL and a remediation trigger. Remediation: produce `artifacts/csharp/coverage.xml` (Cobertura) and a repo-wide first-party testable-denominator measurement confirming the >= 80% floor.

### Approved Exceptions

- **Pre-existing 500-line-cap files (non-blocking).** `QuickFiler/Controllers/QfcCollectionController.cs` (2296 lines, `[ExcludeFromCodeCoverage]` verified at line 20) received only a net-negative Seam C edit (2299 → 2296; `ActivateQueuedTlp` now delegates to `SwapItemTableLayout`). Splitting a 2296-line exempt class is an out-of-scope broad refactor. `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` (821 lines) is pre-existing test-code debt held net-neutral (823 → 821); all 11 new seam tests were routed to the new 326-line `QfcFormControllerSeamTests.cs`. Both dispositions reduce rather than worsen the violation and are accepted as pre-existing-debt; recorded as PARTIAL observations, not blockers. Authority: spec.md risk register; disposition is a review-time decision per the issue.

### Removed/Skipped Tests

- **None.** Baseline 181 passing tests preserved and increased to 196; no tests removed or weakened.

---

## 9. Summary of Changes

### Commits in This PR/Branch

1. **b06497d4** - docs(#223): add active feature folder, spec, and approved atomic plan
2. **e9192710** - refactor(#223): narrow IQfcFormViewer to intent-level seams for testability

### Files Modified

1. **QuickFiler/Interfaces/IQfcFormViewer.cs** (MODIFIED) — narrowed to 23 intent members; removed 4 Button + 1 NumericUpDown + 2 template properties; `L1v0L2L3v_TableLayout` get-only; added Seam B/C/D members.
2. **QuickFiler/Controllers/QfcFormKeyHandler.cs** (NEW) — `internal static bool IsAltKeyCommand(Keys)`.
3. **QuickFiler/Controllers/QfcFormController.cs** + `.Actions.cs` / `.EventHandlers.cs` / `.SetupDisposal.cs` (split; NEW partials) — Phase 0 partial split + Seam B/C/D consumer rewrites.
4. **QuickFiler/Viewers/QfcFormViewer.cs / QfcFormViewerDark.cs / QfcFormViewerExpanded.cs** (MODIFIED) — call `IsAltKeyCommand`; Dark/Expanded gain `[ExcludeFromCodeCoverage]`; QfcFormViewer implements new intent members.
5. **QuickFiler/Controllers/QfcCollectionController.cs** (MODIFIED) — `ActivateQueuedTlp` delegates to `SwapItemTableLayout` (net -3).
6. **QuickFiler/Controllers/QfcHomeController.cs** (MODIFIED) — use `ItemsPerLoadEnabled`/`SkipButtonEnabled`.
7. **QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs / QfcFormKeyHandlerTests.cs** (NEW) and `QfcFormControllerTests.cs` / `QfcHomeControllerRunAsyncTests.cs` (MODIFIED) — migrated mock setups + new seam/routing tests.
8. **QuickFiler/QuickFiler.csproj**, **QuickFiler.Test/QuickFiler.Test.csproj** (MODIFIED) — Compile Include entries for new files.

---

## 10. Compliance Verdict

### Overall Status: ⚠️ PARTIALLY COMPLIANT

The structural refactor satisfies design, structure, naming, toolchain (format/lint/type/test), and test-policy requirements, and all seven acceptance criteria are substantively delivered. One blocking coverage-evidence gap prevents a full-compliant verdict: the canonical C# coverage artifact is absent and the repo-wide first-party coverage floor is unverified.

**Fail-closed reminder:** This audit is NOT marked PASS/ready-for-merge because the required C# coverage artifact (`artifacts/csharp/coverage.xml`) and a repo-wide first-party coverage measurement are missing.

---

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- ✅ Before Making Changes: plan read and followed
- ✅ Design Principles: simplicity/reuse/separation met
- ⚠️ Module & File Structure: changed files < 500; two pre-existing cap files accepted as net-negative debt
- ✅ Naming, Docs, Comments: intent-revealing names, XML docs
- ✅ Toolchain Execution: four gates EXIT_CODE 0
- ⚠️ Summarize & Document: complete except coverage-artifact follow-up

#### Language-Specific Code Change Policy (Section 3)
**For C#:**
- ✅ Tooling & Baseline: csharpier/analyzers/nullable/MSTest pass
- ✅ Design & Type-Safety: typed intent contracts, nullable clean
- ✅ Error Handling / Structure / Naming: conformant

#### General Unit Test Policy (Section 1)
- ✅ Core Principles
- ⚠️ Coverage & Scenarios: new/changed PASS; repo-wide floor unverified (FAIL)
- ✅ Test Structure
- ✅ External Dependencies (no temp files / no external deps)
- ✅ Policy Audit

#### Language-Specific Unit Test Policy (Section 4)
**For C#:**
- ✅ Framework & Scope (MSTest/Moq/FluentAssertions)
- ✅ Test Style & Structure
- ✅ Naming & Readability
- ✅ Toolchain

---

### Metrics Summary

- ✅ 196/196 tests passing (100%)
- ✅ New code (QfcFormKeyHandler) 100% covered (>= 90% floor)
- ✅ Changed type (QfcFormController) +12.62pp, no regression
- ❌ Repo-wide first-party >= 80% floor: unverified (no canonical artifact / no measurement)
- ✅ All four C# toolchain checks EXIT_CODE 0
- ⚠️ Two pre-existing 500-cap files touched net-negative (accepted debt)

---

### Recommendation

**Needs revision (one blocking item).**

Address before merge:
1. Produce the canonical C# coverage artifact `artifacts/csharp/coverage.xml` (Cobertura) and a repo-wide first-party testable-denominator coverage measurement confirming the >= 80% floor. Until measured, the repo-wide coverage gate is unverified and the audit is fail-closed.

Non-blocking (accept as recorded): the two pre-existing 500-cap dispositions (`QfcCollectionController.cs`, `QfcFormControllerTests.cs`).

---

## Appendix A: Test Inventory

### Complete Test List (changed test files)

- QfcFormKeyHandlerTests › IsAltKeyCommand › Keys.Alt returns true
- QfcFormKeyHandlerTests › IsAltKeyCommand › Keys.Alt | Keys.Left returns true
- QfcFormKeyHandlerTests › IsAltKeyCommand › Keys.Control returns false
- QfcFormKeyHandlerTests › IsAltKeyCommand › Keys.None returns false
- QfcFormControllerSeamTests › command-event routing (Ok/Cancel/Undo/Skip) [11 seam `[TestMethod]` cases covering routing, skip-flow state, CaptureItemSettings populated/null, RegisterFormEventHandlers exclusion controls]
- QfcFormControllerTests (migrated mock setups to intent members; behavior assertions preserved)
- QfcHomeControllerRunAsyncTests (migrated to `ItemsPerLoadEnabled`/`SkipButtonEnabled`)

Full repository suite: 196 tests, all passing (`evidence/qa-gates/final-tests-coverage.2026-06-28T20-52.md`).

---

## Appendix B: Toolchain Commands Reference

**For C#:**
```powershell
# Formatting
dotnet tool run csharpier check .

# Linting (.NET analyzers)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

# Type checking (nullable)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true

# Testing + coverage
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation
```

---

**Audit Completed By:** feature-review agent
**Audit Date:** 2026-06-28
**Policy Version:** Current (as of audit date)
