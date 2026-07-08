# Policy Compliance Audit: qfc-form-viewer-testability (Issue #223)

**Audit Date:** 2026-06-29
**Audit Type:** Cycle-1 remediation closing REAUDIT (exit timestamp).
**Code Under Test:** C# (15 `.cs` + 2 `.csproj`). Production: `QuickFiler/Controllers/QfcFormController.cs`, `QfcFormController.Actions.cs` (NEW), `QfcFormController.EventHandlers.cs` (NEW), `QfcFormController.SetupDisposal.cs` (NEW), `QfcFormKeyHandler.cs` (NEW), `QfcCollectionController.cs`, `QfcHomeController.cs`, `Interfaces/IQfcFormViewer.cs`, `Viewers/QfcFormViewer.cs`, `Viewers/QfcFormViewerDark.cs`, `Viewers/QfcFormViewerExpanded.cs`, `QuickFiler/QuickFiler.csproj`. Tests: `QfcFormControllerSeamTests.cs` (NEW), `QfcFormKeyHandlerTests.cs` (NEW), `QfcFormControllerTests.cs`, `QfcHomeControllerRunAsyncTests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`.

**Base branch:** `main` (resolved `origin/main`). **Merge-base:** `86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a`. **Head:** `f4b455e6a3ca536b3fc47fa7026b076efbacf453`. Scope is the full branch diff against the merge-base (74 files, +3751 / -992).

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 15 `.cs` + 2 `.csproj` | 4566 first-party tests | ✅ 4566 pass, 0 fail | 39.24% lines (QfcFormController, changed-type baseline); repo-wide first-party 73.35%–74.11% (pre-existing) | 51.86% lines (QfcFormController); repo-wide first-party 73.35% (testable denominator) / 74.11% (Cobertura root) | 100% (QfcFormKeyHandler) |
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
- C# post-change coverage evidence: `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/evidence/qa-gates/final-tests-coverage.2026-06-28T21-30.md` and `evidence/regression-testing/coverage-delta.2026-06-28T20-52.md`
- C# canonical machine-readable coverage artifact (`artifacts/csharp/coverage.xml`): **PRESENT** — verified this reaudit (8,971,897 bytes; well-formed Cobertura; root `line-rate="0.741108"`, `lines-covered="71654"`, `lines-valid="96685"`). See Section 1.2 and Section 8.
- Per-language comparison summary: Section 1.2.1 below

**Non-negotiable verdict rule:** No policy audit may report PASS unless it includes numeric baseline and post-change coverage metrics for every language in scope, plus changed/new-code coverage when required. This audit records numeric baseline and post-change coverage for the only in-scope language (C#).

**Fail-closed rule:** If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the verdict must be BLOCKED or INCOMPLETE, never PASS. All required artifacts are present this cycle (the prior-cycle missing-artifact FAIL is resolved).

---

## Rejected Scope Narrowing

The caller's "What changed since your prior audit" block was explicitly framed as "assess independently; do not treat as a scope limit," and was treated accordingly. No caller instruction attempted to narrow the audit scope to a plan subset, a file subset, or to mark any language's coverage as out of scope, "informational only," or "not applicable." The full feature-vs-base diff (74 files) was audited and C# coverage was evaluated as a first-class explicit verdict. No narrowing to record.

---

## Evidence Location Compliance

All executor-produced evidence artifacts are written under the canonical `<FEATURE>/evidence/<kind>/` path (`baseline/`, `qa-gates/`, `regression-testing/`, `remediation-baseline/`, `issue-updates/`, `other/`). A scan of the branch diff (`git diff --name-only <merge-base> HEAD`) for files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` returned zero matches. The single canonical machine-readable coverage artifact at `artifacts/csharp/coverage.xml` is the path mandated by the coverage-verification contract and is not a per-feature evidence path. The repo's `validate_evidence_locations.py` script is not present at the repo root; the manual git-diff scan described here is the substantive equivalent. **Verdict: PASS** — no evidence-location violations.

---

## Prior-Cycle Remediation Verification

This reaudit closes feature-review remediation cycle 1. The cycle-1 audit (`policy-audit.2026-06-28T21-30.md`) recorded two blocking findings, both rooted in a single missing-coverage-evidence cause. Disposition this reaudit:

- **Finding 1 (FAIL — canonical C# coverage artifact absent; repo-wide >= 80% floor unverified): RESOLVED.** `artifacts/csharp/coverage.xml` now exists and is a well-formed Cobertura document (root `line-rate="0.741108"`; `lines-covered="71654"`; `lines-valid="96685"`). A repo-wide first-party testable-denominator figure is now recorded: 73.35% (authoritative #197 per-`<line>` method, 39585/53969) / 74.11% (Cobertura root, 71654/96685) / 76.08% (vendored-excluded). Evidence: `evidence/qa-gates/repo-wide-coverage-measurement.2026-06-28T21-30.md`, `evidence/regression-testing/repo-wide-coverage-testable-denominator.2026-06-28T21-30.md`, `evidence/qa-gates/p1-canonical-artifact-verified.2026-06-28T21-30.md`.
- **Finding 2 (PARTIAL blocking — AC5 repo-wide coverage sub-claim unverified): RESOLVED (disposition changed).** The repo-wide figure is now measured at 73.35%–74.11%, which is below the bare `>= 80%` numeric floor. That shortfall has been verified as PRE-EXISTING (not introduced by this change) and has been accepted by the project maintainer under a ratified authority-scoped exception scoped to issue #223 (`maintainer-decision.2026-06-29.md`). See Section 8 for the merits assessment. The sub-claim is therefore no longer "unverified"; it is measured and dispositioned. AC5 is satisfied-with-documented-exception (see feature-audit for the full AC5 evaluation).

---

## Executive Summary

This is a C# WinForms testability refactor that narrows `IQfcFormViewer` to 23 intent-level members across four seams (A–D) and splits the 1142-line `QfcFormController.cs` into four partial-class files to satisfy the 500-line cap before adding code. The structural refactor is well-executed: the interface no longer exposes raw clickable control types, pure Alt-key routing logic is extracted to a testable static (`QfcFormKeyHandler.IsAltKeyCommand`), and new MSTest coverage exercises command-event routing, skip-flow state, and the `CaptureItemSettings` populated/null paths through `Mock<IQfcFormViewer>`.

The four executor toolchain gates (csharpier, .NET analyzers, nullable/TreatWarningsAsErrors, MSTest) each recorded `EXIT_CODE: 0` at the cycle-close timestamp (2026-06-28T21-52); no `.cs`/`.csproj` file changed after that gate run (the only commits since are docs-only: the cycle-1 remediation artifacts and the maintainer decision), so the gate evidence reflects the current source state. The reviewer independently re-ran `csharpier check` on three key changed files this reaudit (exit 0). New-code coverage (`QfcFormKeyHandler` 100%) and changed-type no-regression (`QfcFormController` +12.62pp, 39.24% → 51.86%) are evidenced and independently re-derived from the canonical Cobertura artifact this reaudit.

The prior cycle's single blocking coverage-evidence gap is resolved: the canonical Cobertura artifact exists, is well-formed, and records a repo-wide first-party testable-denominator coverage figure. That figure (73.35%–74.11%) is below the bare `>= 80%` floor, but the shortfall is pre-existing (this change adds tests and moves Form-bound code under `[ExcludeFromCodeCoverage]`; it cannot lower first-party coverage) and is accepted under a maintainer-ratified authority-scoped exception that the repository policy expressly contemplates, with residual repo-wide uplift tracked under `feature/csharp-coverage-uplift` (#197). No blocking finding remains. Two pre-existing 500-line-cap files (`QfcCollectionController.cs` 2296 lines, `[ExcludeFromCodeCoverage]`; `QfcFormControllerTests.cs` 821 lines) are touched with net-negative edits and are recorded as accepted pre-existing-debt dispositions (non-blocking observations).

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
| **Fast Execution** - Tests complete quickly | ✅ PASS | First-party suite (4566 tests) runs under vstest; no network or disk I/O in the changed tests. |
| **Determinism** - Consistent results | ✅ PASS | No `DateTime.Now`/`Random`/network/temp-file usage in changed tests. Moq-driven event raising is deterministic. |
| **Readability & Maintainability** - Clear structure | ✅ PASS | Descriptive `[TestMethod]` names; Arrange-Act-Assert structure with FluentAssertions. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | ✅ PASS | **Baseline (pre-development):** QfcFormController 39.24% lines (301/767).<br>**Command:** `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`<br>**Timestamp:** 2026-06-28T20-52<br>Source: `evidence/baseline/baseline-tests-coverage.2026-06-28T20-52.md`. |
| **No Coverage Regression** | ✅ PASS | **Post-change coverage:** QfcFormController 51.86% lines (363/700).<br>**Change:** +12.62 percentage points.<br>**Status:** No regression (denominator decreased because Seam D moved a ~58-line construction block into `[ExcludeFromCodeCoverage]` Form code; covered lines rose 301→363). Independently re-derived from `artifacts/csharp/coverage.xml` this reaudit (363/700 = 51.86%). Source: `evidence/regression-testing/coverage-delta.2026-06-28T20-52.md`. |
| **New Code Coverage ≥90%** | ✅ PASS | **New file:** `QfcFormKeyHandler.cs`.<br>**New code coverage:** 100%.<br>**Calculation method:** Cobertura class entry parsed from `artifacts/csharp/coverage.xml` this reaudit (all instrumented lines hit). Source: coverage-delta. |
| **Repo-wide ≥80% (testable denominator)** | ✅ PASS (documented exception) | **Measured this reaudit from `artifacts/csharp/coverage.xml`:** repo-wide first-party 73.35% (testable denominator, 39585/53969) / 74.11% (Cobertura root, 71654/96685). This is below the bare `>= 80%` numeric floor. Disposition: ACCEPTED under the maintainer-ratified authority-scoped exception for issue #223 (`maintainer-decision.2026-06-29.md`). The shortfall is PRE-EXISTING and not introduced by this change (the change adds tests and exempts Form-bound code; it cannot lower first-party coverage). No policy threshold or exemption was weakened; residual repo-wide uplift is tracked under #197. Non-blocking for #223. See Section 8. |
| **Comprehensive Coverage** | ✅ PASS | New seam tests cover `IsAltKeyCommand` (4 cases), command-event routing (Ok/Cancel/Undo/Skip/ItemsPerLoad), skip-flow `SkipButtonText`/`SkipButtonEnabled`, and `CaptureItemSettings` populated/null paths. |
| **Positive Flows** - Valid inputs | ✅ PASS | Event-routing tests raise each command event via Moq and `Verify` the controller method executes. |
| **Negative Flows** - Invalid inputs | ✅ PASS | Null `CaptureTlpCellStates()` and null-RowStyles early-return paths tested per seam test file. |
| **Edge Cases** - Boundary conditions | ✅ PASS | `IsAltKeyCommand` boundary combinations (`Keys.Alt`, `Keys.Alt \| Keys.Left`, `Keys.Control`, `Keys.None`). |
| **Error Handling** - Error paths | ✅ PASS | Skip-flow and capture null paths assert intended fallbacks rather than throwing. |
| **Concurrency** - If applicable | N/A | Refactor introduces no new concurrency. |
| **State Transitions** - If applicable | ✅ PASS | Skip-flow state transitions verified via `VerifySet` on `SkipButtonText`/`SkipButtonEnabled`. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 39.24% lines -> Post-change: 51.86% lines. Change: +12.62% lines. New/changed-code coverage: 100%. Disposition: PASS. Evidence: `evidence/regression-testing/coverage-delta.2026-06-28T20-52.md`, `artifacts/csharp/coverage.xml`. Repo-wide first-party measured 73.35%/74.11% (below the bare 80% floor) is accepted under the maintainer-ratified authority-scoped exception (`maintainer-decision.2026-06-29.md`); pre-existing, non-blocking for #223; residual tracked under #197.
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
| **Environment Stability** | ✅ PASS | No temp files; no mutable global state in changed tests. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | ✅ PASS | This document is the required policy review (cycle-1 closing reaudit). No outstanding blocking items. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | ✅ PASS | Objective stated in `issue.md`/`spec.md` (#223): maximize unit testability via Passive-View MVP interface narrowing. |
| **Read existing change plans** | ✅ PASS | `plan.2026-06-28T20-20.md` and `remediation-plan.2026-06-28T21-30.md` present and followed. |
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
| **Under 500 lines** | ⚠️ PARTIAL | Files modified-and-grown this cycle are all < 500 (QfcFormController.cs 195, .Actions 311, .EventHandlers 399, .SetupDisposal 232, QfcFormKeyHandler 20, IQfcFormViewer 51, QfcFormViewer 262, Dark/Expanded 55, QfcHomeController 454, seam test 326, key-handler test 67). Two pre-existing cap violations remain: `QfcCollectionController.cs` 2296 (baseline 2299, net -3, `[ExcludeFromCodeCoverage]`) and `QfcFormControllerTests.cs` 821 (baseline 823, net -2). Both are accepted pre-existing-debt dispositions (net-negative; not blocking). See Section 8. |
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
| **1. Formatting** | ✅ PASS | **Command:** `dotnet tool run csharpier check .`<br>**Result:** EXIT_CODE 0 (`evidence/qa-gates/final-csharpier.2026-06-28T21-30.md`); reviewer re-ran `csharpier check` on 3 key files this reaudit → exit 0. |
| **2. Linting** | ✅ PASS | **Command:** `msbuild TaskMaster.sln -t:Build ... -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`<br>**Result:** EXIT_CODE 0 (`evidence/qa-gates/final-analyzers.2026-06-28T21-30.md`). |
| **3. Type checking** | ✅ PASS | **Command:** `msbuild TaskMaster.sln -t:Build ... -p:Nullable=enable -p:TreatWarningsAsErrors=true`<br>**Result:** EXIT_CODE 0 (`evidence/qa-gates/final-nullable.2026-06-28T21-30.md`). |
| **4. Testing** | ✅ PASS | **Command:** `vstest.console.exe <first-party *.Test.dll> /EnableCodeCoverage`<br>**Result:** 4566 passed, 0 failed (`evidence/qa-gates/final-tests-coverage.2026-06-28T21-30.md`). |
| **Full toolchain loop** | ✅ PASS | Per-phase and final gate evidence all EXIT_CODE 0; no `.cs`/`.csproj` changed after the cycle-close gate run. |
| **Explicit reporting** | ✅ PASS | Commands and results recorded in executor evidence and this audit. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | ✅ PASS | `ac-traceability.2026-06-28T20-52.md` maps AC1–AC7 to tasks and evidence. |
| **Design choices explained** | ✅ PASS | Seam rationale documented in `spec.md` and research docs. |
| **Update supporting documents** | ✅ PASS | issue/spec/plan, maintainer decision, and evidence committed. |
| **Provide next steps** | ✅ PASS | Toolchain complete; remediation closed; residual repo-wide uplift owned by #197. |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3C#: C# Code Change Policy Compliance

#### 3C#.1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with CSharpier** | ✅ PASS | `dotnet tool run csharpier check .` EXIT_CODE 0; independent reviewer check of 3 files exit 0 this reaudit. |
| **Linting with .NET analyzers** | ✅ PASS | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` EXIT_CODE 0. |
| **Type checking with nullable analysis** | ✅ PASS | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` EXIT_CODE 0. |
| **Testing with MSTest** | ✅ PASS | `vstest.console.exe ... /EnableCodeCoverage` 4566/4566 pass. |

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
| **Use MSTest** | ✅ PASS | `using Microsoft.VisualStudio.TestTools.UnitTesting; [TestClass]/[TestMethod]` in both new test files. No xUnit/NUnit. |
| **Use Moq** | ✅ PASS | `using Moq;` with `Mock<IQfcFormViewer>`, `Raise`, `Verify`, `VerifySet`. |
| **Prefer FluentAssertions** | ✅ PASS | `using FluentAssertions;` in both new test files. |
| **Coverage expectation** | ✅ PASS (documented exception) | New code 100% (>= 90%) and changed-type no-regression (+12.62pp) PASS; repo-wide first-party 73.35%/74.11% below the bare 80% floor is accepted under the maintainer-ratified authority-scoped exception (Section 1.2, Section 8). |

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

**Coverage:** 100% of `QfcFormKeyHandler` (all instrumented lines hit, verified in `artifacts/csharp/coverage.xml`).

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
| Total Tests (first-party) | 4566 | ✅ |
| Tests Passed | 4566 (100%) | ✅ |
| Tests Failed | 0 | ✅ |
| Execution Time | not separately recorded (single coverage-enabled run) | ✅ |
| Functions/Classes Tested | QfcFormKeyHandler + QfcFormController seam paths | ✅ |
| Test File Size | seam tests 326 lines; key-handler tests 67 lines | ✅ |
| Code Coverage (changed type) | 51.86% lines (QfcFormController); new code 100%; repo-wide first-party 73.35%/74.11% (documented exception) | ✅ |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `dotnet tool run csharpier check .` | EXIT_CODE 0 | ✅ |
| .NET Analyzers | `msbuild TaskMaster.sln -t:Build -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true` | EXIT_CODE 0 | ✅ |
| Nullable Type Check | `msbuild TaskMaster.sln -t:Build -p:Nullable=enable -p:TreatWarningsAsErrors=true` | EXIT_CODE 0 | ✅ |
| MSTest Tests | `vstest.console.exe <first-party *.Test.dll> /EnableCodeCoverage` | 4566 pass 0 fail | ✅ |

**Notes:**
The four toolchain results above are verified from executor evidence artifacts (`evidence/qa-gates/final-*.2026-06-28T21-30.md`), each recording EXIT_CODE 0 at the cycle-close timestamp. No `.cs`/`.csproj` file changed after that gate run (verified via `git diff --name-only` between the gate-evidence head `e9192710` and current head `f4b455e6` — the only intervening commits are docs-only), so the gate evidence reflects the current source. The reviewer independently re-ran CSharpier check on three changed C# files this reaudit (exit 0). msbuild/vstest were not reproduced locally (msbuild is not on the bash PATH in this environment); their PASS status rests on the executor evidence, which is the workflow-sanctioned evidence-verification model.

---

## 8. Gaps and Exceptions

### Identified Gaps

- **None blocking.** The prior cycle's blocking gap (canonical C# coverage artifact missing; repo-wide floor unverified) is resolved: `artifacts/csharp/coverage.xml` exists, is well-formed Cobertura, and records a repo-wide first-party testable-denominator figure.

### Approved Exceptions

- **Repo-wide first-party coverage below the 80% floor (maintainer-ratified authority-scoped exception; non-blocking).** Measured repo-wide first-party coverage is 73.35% (testable denominator, 39585/53969) / 74.11% (Cobertura root, 71654/96685), below the `>= 80%` floor. Merits assessment for this reaudit:
  1. **Pre-existing, not introduced.** This change is a structural/testability refactor that adds tests and moves Form-bound code under `[ExcludeFromCodeCoverage]`; it cannot lower first-party coverage. New code `QfcFormKeyHandler` is 100% covered; the changed `QfcFormController` type improved +12.62pp (39.24% → 51.86%) with no regression. The measured 73.35% is consistent with #197's known 59–76% baseline range.
  2. **Authority-ratified under a policy that contemplates it.** `CLAUDE.md` (General Unit Test Policy, COM/VSTO/WinForms coverage exemption) and `.claude/rules/general-unit-test.md` expressly permit maintainer-ratified exemptions for COM-host-bound code. `maintainer-decision.2026-06-29.md` (Dan Moisan, project maintainer; Ratified) accepts the shortfall as a pre-existing, separately-tracked condition out of scope for #223.
  3. **No policy weakening.** No `.editorconfig`, `coverage.config`, `.claude/rules/**`, or `CLAUDE.md` threshold was altered; no test was weakened or removed (verified: 4566/4566 pass; no `.cs` edits after the gate). The exemption boundary was applied as-written (Form-derived/Designer/COM-host-bound classes absent from instrumentation), not widened to inflate the figure.
  4. **Scoped and tracked.** The exception applies to #223 only; the repo-wide first-party floor remains in force and the uplift remains tracked under `feature/csharp-coverage-uplift` (#197).
  Evidence: `maintainer-decision.2026-06-29.md`, `evidence/other/repo-wide-floor-escalation-finding.2026-06-28T21-30.md`, `evidence/regression-testing/repo-wide-coverage-testable-denominator.2026-06-28T21-30.md`.
- **Pre-existing 500-line-cap files (non-blocking).** `QuickFiler/Controllers/QfcCollectionController.cs` (2296 lines, `[ExcludeFromCodeCoverage]`) received only a net-negative Seam C edit (2299 → 2296; `ActivateQueuedTlp` now delegates to `SwapItemTableLayout`). Splitting a 2296-line exempt class is an out-of-scope broad refactor. `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` (821 lines) is pre-existing test-code debt held net-negative (823 → 821); all 11 new seam tests were routed to the new 326-line `QfcFormControllerSeamTests.cs`. Both dispositions reduce rather than worsen the violation and are accepted as pre-existing-debt; recorded as PARTIAL observations, not blockers. Authority: spec.md risk register; disposition is a review-time decision per the issue.

### Removed/Skipped Tests

- **None.** No tests removed or weakened; the suite grew and is green at 4566/4566 first-party tests.

---

## 9. Summary of Changes

### Commits in This PR/Branch

1. **b06497d4** - docs(#223): add active feature folder, spec, and approved atomic plan
2. **e9192710** - refactor(#223): narrow IQfcFormViewer to intent-level seams for testability
3. **c2b05afe** - docs(#223): remediation cycle 1 — generate canonical coverage; escalate pre-existing repo-wide floor
4. **f4b455e6** - docs(#223): ratify authority-scoped coverage exception; re-check AC5

### Files Modified

1. **QuickFiler/Interfaces/IQfcFormViewer.cs** (MODIFIED) — narrowed to 23 intent members; removed 4 Button + 1 NumericUpDown + 2 template properties; `L1v0L2L3v_TableLayout` get-only; added Seam B/C/D members.
2. **QuickFiler/Controllers/QfcFormKeyHandler.cs** (NEW) — `internal static bool IsAltKeyCommand(Keys)`.
3. **QuickFiler/Controllers/QfcFormController.cs** + `.Actions.cs` / `.EventHandlers.cs` / `.SetupDisposal.cs` (split; NEW partials) — Phase 0 partial split + Seam B/C/D consumer rewrites.
4. **QuickFiler/Viewers/QfcFormViewer.cs / QfcFormViewerDark.cs / QfcFormViewerExpanded.cs** (MODIFIED) — call `IsAltKeyCommand`; Dark/Expanded gain `[ExcludeFromCodeCoverage]`; QfcFormViewer implements new intent members.
5. **QuickFiler/Controllers/QfcCollectionController.cs** (MODIFIED) — `ActivateQueuedTlp` delegates to `SwapItemTableLayout` (net -3).
6. **QuickFiler/Controllers/QfcHomeController.cs** (MODIFIED) — use `ItemsPerLoadEnabled`/`SkipButtonEnabled`.
7. **QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs / QfcFormKeyHandlerTests.cs** (NEW) and `QfcFormControllerTests.cs` / `QfcHomeControllerRunAsyncTests.cs` (MODIFIED) — migrated mock setups + new seam/routing tests.
8. **QuickFiler/QuickFiler.csproj**, **QuickFiler.Test/QuickFiler.Test.csproj** (MODIFIED) — Compile Include entries for new files.
9. **docs/features/active/2026-06-28-qfc-form-viewer-testability-223/** (NEW docs + evidence) — feature folder, plan, remediation plan/inputs, maintainer decision, prior audit artifacts, canonical coverage evidence.

---

## 10. Compliance Verdict

### Overall Status: ✅ FULLY COMPLIANT (with one documented maintainer-ratified exception)

The structural refactor satisfies design, structure, naming, toolchain (format/lint/type/test), and test-policy requirements, and all seven acceptance criteria are delivered. The prior cycle's single blocking coverage-evidence gap is resolved: the canonical C# coverage artifact exists and the repo-wide first-party coverage figure is measured. The repo-wide figure (73.35%/74.11%) is below the bare 80% floor, but the shortfall is pre-existing and is accepted under a maintainer-ratified authority-scoped exception that the repository policy expressly contemplates, with residual uplift tracked under #197. No blocking finding remains.

**Fail-closed note:** The fail-closed rule (no PASS when a required artifact is missing) is satisfied this cycle because all required coverage artifacts are present; the prior-cycle missing-artifact FAIL is resolved.

---

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- ✅ Before Making Changes: plan read and followed
- ✅ Design Principles: simplicity/reuse/separation met
- ⚠️ Module & File Structure: changed files < 500; two pre-existing cap files accepted as net-negative debt
- ✅ Naming, Docs, Comments: intent-revealing names, XML docs
- ✅ Toolchain Execution: four gates EXIT_CODE 0
- ✅ Summarize & Document: complete

#### Language-Specific Code Change Policy (Section 3)
**For C#:**
- ✅ Tooling & Baseline: csharpier/analyzers/nullable/MSTest pass
- ✅ Design & Type-Safety: typed intent contracts, nullable clean
- ✅ Error Handling / Structure / Naming: conformant

#### General Unit Test Policy (Section 1)
- ✅ Core Principles
- ✅ Coverage & Scenarios: new/changed PASS; repo-wide floor accepted under documented exception
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

- ✅ 4566/4566 first-party tests passing (100%)
- ✅ New code (QfcFormKeyHandler) 100% covered (>= 90% floor)
- ✅ Changed type (QfcFormController) +12.62pp, no regression
- ✅ Repo-wide first-party 73.35%/74.11%: below bare 80% floor but accepted under maintainer-ratified authority-scoped exception (pre-existing; residual tracked under #197)
- ✅ All four C# toolchain checks EXIT_CODE 0
- ⚠️ Two pre-existing 500-cap files touched net-negative (accepted debt)

---

### Recommendation

**Ready for merge (no blocking items).**

No remediation required. The repo-wide first-party coverage uplift to `>= 80%` is owned by #197, not by #223. The two pre-existing 500-cap dispositions (`QfcCollectionController.cs`, `QfcFormControllerTests.cs`) are accepted as net-negative debt and are non-blocking.

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

Full first-party suite: 4566 tests, all passing (`evidence/qa-gates/final-tests-coverage.2026-06-28T21-30.md`).

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
**Audit Date:** 2026-06-29
**Policy Version:** Current (as of audit date)
