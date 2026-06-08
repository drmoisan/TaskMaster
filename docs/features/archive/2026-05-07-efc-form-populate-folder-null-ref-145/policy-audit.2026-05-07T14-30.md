# Policy Compliance Audit: EfcFormController NullRef Fix (Issue #145)

**Audit Date:** 2026-05-07  
**Code Under Test:**  
- `QuickFiler/Controllers/EfcFormController.cs` (modified — null guard added)  
- `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` (new — regression test)  
- `QuickFiler.Test/QuickFiler.Test.csproj` (modified — Compile Include added)  

**Work Mode:** minor-audit  
**Branch:** `bug/efc-form-populate-folder-null-ref-145`  
**Base:** `development` @ `f35764aa`  
**Head:** `bug/efc-form-populate-folder-null-ref-145` @ `f35764aa` (working tree — uncommitted)  

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 2 prod files (1 mod), 1 test file (new) | 3991 (user-reported) / 3990 (artifact) | ✅ 3989 pass, 0 fail, 2 skip (user-reported); artifact shows 3990/3987/1 | N/A — QuickFiler excluded from coverage tooling | N/A | 0% instrumented (COM-gated guard line; QuickFiler excluded from coverage tooling — see §5) |

**Coverage Evidence Checklist**

- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- C# baseline coverage artifact: `N/A — QuickFiler project not instrumented in coverage_output.txt` (see §5)
- C# post-change coverage artifact: `N/A — QuickFiler project not instrumented`
- Per-language comparison summary: §5

**Fail-closed rule acknowledgment:** No per-file coverage artifact exists for QuickFiler. This is a pre-existing tooling gap (QuickFiler produces COM-bound VSTO code not instrumented in the repo's current coverage configuration). The verdict below reflects this.

---

## Executive Summary

This audit evaluates a minimal bug fix for issue #145: a null guard added to `EfcFormController.PopulateFolderCombobox` to prevent a `NullReferenceException` caused by a race condition between `Cleanup()` and the async continuation. One new test file was added.

**Policy documents evaluated:**
- ✅ `general-code-change.instructions.md`
- ✅ `general-unit-test.instructions.md`
- ✅ `csharp-code-change.instructions.md`
- ✅ `csharp-unit-test.instructions.md`

**Language-specific policies evaluated:**
- ✅ C#: `csharp-code-change.instructions.md` + `csharp-unit-test.instructions.md`
- N/A Python, PowerShell, Bash, JSON, TypeScript

**Toolchain results (per plan artifacts and code inspection):**
- CSharpier formatting: clean (no changes)
- .NET analyzer build: 0 errors, 0 warnings
- Nullable build (`/p:Nullable=enable /p:TreatWarningsAsErrors=true`): 0 errors, 0 warnings
- Test suite (per plan phase2 artifact): 3990 total, 3987 passed, 1 failed (pre-existing OCR flaky test), 2 skipped — new test NOT present in artifact
- Test suite (user-reported final run): 3991 total, 3989 passed, 0 failed, 2 skipped — independent artifact absent

**Key finding:** The phase2 evidence artifact (`artifacts/orchestration/145-phase2-test.txt`) was captured before the new test was compiled into the test project. The artifact shows 3990 tests (pre-addition count) with one pre-existing intermittent failure (`BuildClassifiersAsync_WithFixtureAndFolderConfig_StoresBuiltFolderClassifier` — Tesseract OCR environment failure unrelated to this fix). The user's final clean run claim is plausible but not independently verifiable from the stored artifact.

**Temporary artifacts cleanup:**
- ✅ No throwaway scripts created during this fix

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | ✅ PASS | `EfcFormControllerTests` has one test that constructs its controller independently via reflection. No shared state. |
| **Isolation** - Each test targets single behavior | ✅ PASS | Single test method exercises the entry-ordering contract of `PopulateFolderCombobox`. |
| **Fast Execution** - Tests complete quickly | ✅ PASS | Test invokes a method that immediately throws on null `_dataModel`; expected to complete in < 10 ms. |
| **Determinism** - Consistent results | ✅ PASS | Uses reflection-constructed controller with all fields null. No external I/O, no time dependency, no randomness. |
| **Readability & Maintainability** - Clear structure | ✅ PASS | Method name `PopulateFolderCombobox_WhenDataModelIsNull_ThrowsNullReferenceOnDataModel` is descriptive. Inline comment explains COM constraint in full. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | N/A | QuickFiler not instrumented in repo coverage tooling (no QuickFiler entries in `coverage_output.txt`). Pre-existing tooling gap. |
| **No Coverage Regression** | N/A | No pre/post coverage artifact available for QuickFiler. Pre-existing tooling gap. |
| **New Code Coverage ≥90%** | PARTIAL | The null guard at line 950 (`if (_formViewer is null) return;`) is not exercised by the unit test because `_dataModel` (null) causes a `NullReferenceException` before the guard is reached. COM constraint is explicitly documented in the test. The method's entry point and `_dataModel` dereference are exercised. Coverage of the guard line requires a live COM execution context. |
| **Comprehensive Coverage** | PARTIAL | `PopulateFolderCombobox`: 1 test; covers method entry and first dereference. Guard line and post-guard code are COM-gated (requires Outlook COM STA). |
| **Positive Flows** - Valid inputs | N/A | Full positive path requires Outlook COM objects; not unit-testable without integration test infrastructure. |
| **Negative Flows** - Invalid inputs | ✅ PASS | `PopulateFolderCombobox_WhenDataModelIsNull_ThrowsNullReferenceOnDataModel`: null `_dataModel` → `NullReferenceException` on `_dataModel.InitFolderHandlerAsync`. |
| **Edge Cases** - Boundary conditions | PARTIAL | The critical race-condition edge case (null `_formViewer` after `InitFolderHandlerAsync` completes) is COM-gated. The test documents this constraint explicitly. |
| **Error Handling** - Error paths | ✅ PASS | Test confirms the method fails fast with `NullReferenceException` when `_dataModel` is null. |
| **Concurrency** - If applicable | N/A | COM-gated; cannot be exercised in unit tests. Test comment documents this. |
| **State Transitions** - If applicable | N/A | No stateful component transitions introduced. |

### 1.2.1 Per-Language Coverage Comparison

- TypeScript: N/A - out of scope.
- PowerShell: N/A - out of scope.
- C#: Baseline: 0% (QuickFiler project not instrumented in repo coverage tooling — no QuickFiler entries in `coverage_output.txt`) -> Post-change: 0% (not instrumented). Change: +0% (no instrumentation change). New/changed-code coverage: 0% (guard line is COM-gated; QuickFiler excluded from coverage tooling — see §5). Disposition: N/A — out of scope for instrumentation. Evidence: `coverage_output.txt` (QuickFiler absent), `artifacts/orchestration/145-phase0-baseline.txt`, `artifacts/orchestration/145-phase2-test.txt`.

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | ✅ PASS | FluentAssertions `ThrowAsync<NullReferenceException>` with explicit `because` string describing the expected ordering contract. |
| **Arrange-Act-Assert Pattern** | ✅ PASS | Test uses labeled Arrange / Act / Assert comment blocks with no interleaving. |
| **Document Intent** | ✅ PASS | 15-line comment block before the test method explains the bug, root cause, fix, and COM constraint. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | ✅ PASS | No network, filesystem, database, or COM dependencies. Controller created via private constructor reflection. |
| **Use Mocks/Stubs** | ✅ PASS | No mocks required; null fields are the "stub" for COM objects. Reflection pattern used to allocate uninitialized controller. |
| **Environment Stability** | ✅ PASS | No global state, no temp files, no environment variables. `CreateMinimalController` is a deterministic factory. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | ✅ PASS | This document serves as the required pre-submission policy audit for issue #145. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | ✅ PASS | Issue #145 documents the race condition, root cause, and proposed fix. `issue.md` has explicit `## Acceptance Criteria`. |
| **Read existing change plans** | ✅ PASS | `plan.2026-05-07T13-39.md` was created before implementation. All phases verified. |
| **Document the plan** | ✅ PASS | `plan.2026-05-07T13-39.md` present in feature folder with P0/P1/P2 phases, all tasks marked complete. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | ✅ PASS | Fix is a 3-line addition: guard `if (_formViewer is null) return;` with inline comment. No structural changes. |
| **Reusability** | ✅ PASS | No code was duplicated. Single guard statement at the correct post-await point. |
| **Extensibility** | ✅ PASS | No public API changes. Guard is internal behavior. |
| **Separation of concerns** | ✅ PASS | Fix does not cross concerns; it guards only the `_formViewer` access path in the async continuation. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | ✅ PASS | `EfcFormController.cs` retains its single-controller responsibility. `EfcFormControllerTests.cs` mirrors it in the test project. |
| **Under 500 lines** | ✅ PASS | `EfcFormControllerTests.cs`: 80 lines. `EfcFormController.cs`: well under 500 lines for the modified sections. |
| **Public vs internal** | ✅ PASS | `PopulateFolderCombobox` is `public` (unchanged); no new public surface introduced. |
| **No circular dependencies** | ✅ PASS | `EfcFormControllerTests.cs` depends on `QuickFiler.Controllers`. No new dependency cycles. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | ✅ PASS | Guard uses `is null` pattern. No new identifiers introduced. |
| **Docs/docstrings** | ✅ PASS | `CreateMinimalController` has an XML summary. Test method has a prose comment block. |
| **Comment why, not what** | ✅ PASS | Guard comment: "Guard: Cleanup() may have run and nulled `_formViewer` while `InitFolderHandlerAsync` was awaited. Dereference must be gated here to prevent NullReferenceException at the subsequent await and field accesses (issue #145)." Explains rationale. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ✅ PASS | **Command:** `dotnet tool run csharpier format .` **Result:** No changes to files under review. Evidence: plan P2-T1 checked. |
| **2. Linting** | ✅ PASS | **Command:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` **Result:** 0 errors, 0 warnings. Evidence: plan P2-T2 checked. |
| **3. Type checking** | ✅ PASS | **Command:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` **Result:** 0 errors, 0 warnings. Evidence: plan P2-T3 checked. |
| **4. Testing** | PARTIAL | **Command:** `pwsh -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug` **Artifact result:** 3990 total, 3987 passed, 1 failed (pre-existing OCR flaky test — `BuildClassifiersAsync_WithFixtureAndFolderConfig_StoresBuiltFolderClassifier`), 2 skipped; new test absent from artifact. **User-reported final:** 3991 total, 3989 passed, 0 failed, 2 skipped. Evidence: `artifacts/orchestration/145-phase2-test.txt` (intermediate capture), plan P2-T4. |
| **Full toolchain loop** | PARTIAL | Format → lint → nullable all confirmed clean in one pass per plan. Test pass is user-reported for the final run; artifact backing is incomplete. |
| **Explicit reporting** | ✅ PASS | Plan P2-T1 through P2-T4 are all checked with evidence paths. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | ✅ PASS | `issue.md` summary section and plan Phase 1 describe the change precisely. |
| **Design choices explained** | ✅ PASS | Plan includes a "Unit-test constraint note" explaining why full async race cannot be reproduced in a unit test and how the chosen approach documents it correctly. |
| **Update supporting documents** | ✅ PASS | `issue.md` AC checkboxes updated (AC1–AC4 all `[x]`). |
| **Provide next steps** | ✅ PASS | `issue.md` "Next Step" section is present. Plan Phase 2 complete. |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3C: C# Code Change Policy Compliance

#### 3C.1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with CSharpier** | ✅ PASS | `dotnet tool run csharpier format .` — no changes. Evidence: plan P2-T1. |
| **Linting with .NET analyzers** | ✅ PASS | MSBuild with `EnableNETAnalyzers=true` and `EnforceCodeStyleInBuild=true` — 0 errors, 0 warnings. Evidence: plan P2-T2. |
| **Type checking with nullable** | ✅ PASS | MSBuild with `Nullable=enable` and `TreatWarningsAsErrors=true` — 0 errors, 0 warnings. Evidence: plan P2-T3. |
| **Testing with vstest** | PARTIAL | See §2.5 testing row. Evidence artifact incomplete; user-reported final run not independently verifiable. |

#### 3C.2 C# Design & Type-Safety Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Strong contracts and explicit APIs** | ✅ PASS | `PopulateFolderCombobox` signature unchanged. Guard is internal behavior with no API impact. |
| **Null-safety by default** | ✅ PASS | `if (_formViewer is null) return;` uses the canonical C# null-check pattern. Consistent with nullable reference type rules. |
| **Prefer composition and focused types** | ✅ PASS | No new types introduced. Fix is a targeted statement in an existing method. |
| **Asynchrony and resource safety** | ✅ PASS | Guard is placed after the await boundary, which is the correct point where the null race can occur. |

#### 3C.3 Classes, Methods, and APIs (C#)

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Methods small and focused** | ✅ PASS | Guard adds 3 lines to `PopulateFolderCombobox`. Method remains focused. |
| **No god objects** | ✅ PASS | No new responsibilities added to `EfcFormController`. |
| **Interfaces and contracts** | ✅ PASS | No interface changes. |

#### 3C.4 Error Handling, Logging, and Contracts (C#)

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Fail fast** | ✅ PASS | Guard returns early (silently) when `_formViewer` is null — correct behavior for fire-and-forget async. Logging not warranted for a normal cleanup-race exit path. |
| **No broad catch-all** | ✅ PASS | No exception handling changed. |
| **Invariants at construction** | ✅ PASS | No constructor changes. |

#### 3C.5 Module & File Structure (C#)

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive files** | ✅ PASS | `EfcFormController.cs` retains its single responsibility. |
| **Public vs internal** | ✅ PASS | `internal` and `public` visibility unchanged. |
| **Imports** | ✅ PASS | No new `using` directives added. |

#### 3C.6 Naming, Docs, and Comments (C#)

| Requirement | Status | Evidence |
|------------|--------|----------|
| **PascalCase / camelCase conventions** | ✅ PASS | No new identifiers introduced. |
| **XML documentation comments** | ✅ PASS | `CreateMinimalController` has an XML summary in the test file. |
| **Comment why** | ✅ PASS | Multi-line comment on the guard explains the race condition and issue reference. |

---

## 4. Language-Specific Unit Test Policy Compliance

### Section 4C: C# Unit Test Policy Compliance

#### 4C.1 Framework Selection

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use MSTest** | ✅ PASS | `[TestClass]` and `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. No xUnit or NUnit introduced. |

#### 4C.2 C#-Specific Libraries and Conventions

| Requirement | Status | Evidence |
|------------|--------|----------|
| **FluentAssertions for assertions** | ✅ PASS | `await act.Should().ThrowAsync<NullReferenceException>(...)` — FluentAssertions used for async assertion. |
| **Moq for mocking** | ✅ PASS | No mocking needed; reflection-null pattern used instead. No Moq violation. |
| **MSTest attributes** | ✅ PASS | `[TestClass]`, `[TestMethod]` present. |

#### 4C.3 C# Toolchain Command Selection

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Step 1: CSharpier** | ✅ PASS | `dotnet tool run csharpier format .` — clean. |
| **Step 2: MSBuild analyzers** | ✅ PASS | `msbuild /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — 0 errors. |
| **Step 3: MSBuild nullable** | ✅ PASS | `msbuild /p:Nullable=enable /p:TreatWarningsAsErrors=true` — 0 errors. |
| **Step 4: vstest** | PARTIAL | See §2.5. Artifact incomplete; final clean run user-reported. |

---

## 5. Test Coverage Detail

### `EfcFormController.PopulateFolderCombobox` (1 test)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| `PopulateFolderCombobox_WhenDataModelIsNull_ThrowsNullReferenceOnDataModel` | Negative (null `_dataModel`) | Method entry → `_dataModel.InitFolderHandlerAsync` call (throws before guard) | ✅ |

**Coverage of modified section:**

| Line | Code | Covered |
|------|------|---------|
| `await _dataModel.InitFolderHandlerAsync(folderList);` | Original entry | ✅ (throws NRE on null `_dataModel`) |
| `if (_formViewer is null)` | New guard | ❌ COM-gated (requires `_dataModel` to be non-null and `InitFolderHandlerAsync` to complete) |
| `    return;` | New guard body | ❌ COM-gated |
| `await _formViewer.UiSyncContext;` | Post-guard | ❌ COM-gated |

**Documented constraint:** The guard line (AC2) is the fix; it can only be exercised when `_dataModel.InitFolderHandlerAsync(folderList)` completes without throwing, which requires live Outlook COM objects. The unit test correctly documents the ordering constraint that makes the guard effective rather than attempting an impossible COM-free full-path test. This is consistent with the `EfcHomeControllerTests` reflection pattern established in the codebase.

---

## 6. Test Execution Metrics

| Metric | Baseline (P0) | Phase2 Artifact | User-Reported Final |
|--------|---------------|-----------------|---------------------|
| Total tests | 3990 | 3990 | 3991 |
| Passed | 3988 | 3987 | 3989 |
| Failed | 0 | 1 (`BuildClassifiersAsync_WithFixtureAndFolderConfig_StoresBuiltFolderClassifier` — Tesseract OCR intermittent) | 0 |
| Skipped | 2 | 2 | 2 |
| New test present | N/A | No (artifact predates csproj update) | Yes |
| Evidence artifact | `artifacts/orchestration/145-phase0-baseline.txt` | `artifacts/orchestration/145-phase2-test.txt` | None — reported only |

**Note on phase2 discrepancy:** The phase2 artifact was captured before `EfcFormControllerTests.cs` was registered in `QuickFiler.Test.csproj`. The single failure is the Tesseract OCR environment error (`Failed loading language 'eng'`), which is a known intermittent infrastructure failure unrelated to this fix. The baseline shows this test passing (3988 passed, 0 failed), confirming it is an intermittent rather than a newly introduced failure.

---

## 7. Code Quality Checks

| Check | Status | Command | Notes |
|-------|--------|---------|-------|
| CSharpier formatting | ✅ PASS | `dotnet tool run csharpier format .` | No changes to reviewed files. |
| .NET analyzers (lint) | ✅ PASS | `msbuild TaskMaster.sln /t:Build /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 errors, 0 warnings. |
| Nullable warnings as errors | ✅ PASS | `msbuild TaskMaster.sln /t:Build /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 errors, 0 warnings. |
| Guard placement correctness | ✅ PASS | Code inspection: guard is immediately after `await _dataModel.InitFolderHandlerAsync(folderList)`, before first `_formViewer` access. AC2 satisfied. |
| No API-breaking changes | ✅ PASS | `PopulateFolderCombobox` signature unchanged. |
| No new suppressions | ✅ PASS | No `// nolint`, `#pragma warning disable`, or `[SuppressMessage]` added. |

---

## 8. Gaps and Exceptions

| Gap | Severity | Disposition |
|-----|----------|-------------|
| Phase2 evidence artifact (`145-phase2-test.txt`) was captured before new test was compiled into project. New test's pass/fail status is not independently verifiable from stored artifacts. | Minor | Document gap. Recommend capturing a fresh committed test run after commit. |
| Guard line (`if (_formViewer is null) return;`) has 0% unit test coverage due to COM constraint. | Accepted | Documented in test comment and §5. Consistent with established codebase pattern for VSTO COM-dependent code. |
| QuickFiler project not instrumented in repo coverage tooling. | Pre-existing | Pre-existing tooling gap; not introduced by this fix. |
| `BuildClassifiersAsync_WithFixtureAndFolderConfig_StoresBuiltFolderClassifier` failed in phase2 artifact run. | Pre-existing | Intermittent Tesseract OCR infrastructure failure. Unrelated to this fix. Passes in baseline and user-reported final run. |

---

## 9. Summary of Changes

**Files modified:**
1. `QuickFiler/Controllers/EfcFormController.cs` — Added `if (_formViewer is null) return;` with a multi-line explanatory comment immediately after `await _dataModel.InitFolderHandlerAsync(folderList)` in `PopulateFolderCombobox`. Change is 5 lines (guard + comment block). No other modifications.
2. `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` — New file with `EfcFormControllerTests` class and one test method `PopulateFolderCombobox_WhenDataModelIsNull_ThrowsNullReferenceOnDataModel`. Uses reflection to construct the controller, FluentAssertions for async exception assertion, MSTest attributes.
3. `QuickFiler.Test/QuickFiler.Test.csproj` — Added `<Compile Include="Controllers\EfcFormControllerTests.cs" />`.

**Design rationale:** The earliest testable point in `PopulateFolderCombobox` is the `_dataModel` dereference (which requires COM to proceed past). Placing the guard after the await and before `_formViewer` is first used is the minimum correct fix. Returning silently is correct for a fire-and-forget async method — no caller awaits or inspects the result.

---

## 10. Compliance Verdict

**Overall verdict: INCOMPLETE**

All code changes are structurally correct and policy-compliant per code inspection. The toolchain passes for formatting, lint, and nullable analysis. The single evidence gap is the test run artifact: `145-phase2-test.txt` was captured before the new test was registered in the csproj, so the new test's pass confirmation is backed only by the user's report, not a stored artifact. The pre-existing OCR test failure in the artifact is unrelated to this fix.

Coverage of the guard line is not achievable in unit tests (COM constraint) and is a documented, accepted exception consistent with the codebase pattern.

**What would change this to PASS:**
1. Commit the working-tree changes and run `pwsh -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug` after the commit. Capture output to `artifacts/orchestration/145-phase2-final.txt`. Confirm: 3991 total, 3989 passed, 0 failed, 2 skipped, including `PopulateFolderCombobox_WhenDataModelIsNull_ThrowsNullReferenceOnDataModel` ✅.

**Policy items satisfied:** All design, formatting, lint, nullable, naming, documentation, and test structure requirements. AC1–AC4 verified by code inspection.

---

## Appendix A: Test Inventory

| # | Test Class | Test Method | Framework | Type | Status |
|---|-----------|-------------|-----------|------|--------|
| 1 | `EfcFormControllerTests` | `PopulateFolderCombobox_WhenDataModelIsNull_ThrowsNullReferenceOnDataModel` | MSTest | Negative / structural-contract | ✅ PASS (user-reported; not in phase2 artifact) |

---

## Appendix B: Toolchain Commands Reference

| Step | Command | Environment |
|------|---------|-------------|
| Format | `dotnet tool run csharpier format .` | PowerShell, repo root |
| Lint | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | PowerShell, repo root |
| Type-check | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` | PowerShell, repo root |
| Test | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug` | PowerShell, repo root |
| Verify test list | `vstest.console.exe <assembly> /ListTests` | PowerShell, repo root |
