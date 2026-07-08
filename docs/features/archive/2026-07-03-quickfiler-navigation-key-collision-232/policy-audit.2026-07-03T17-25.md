# Policy Compliance Audit: QuickFiler Navigation-Key Collision Fix (Issue #232)

**Audit Date:** 2026-07-03
**Cycle:** Re-audit after remediation cycle 1 (exit reaudit)
**Code Under Test:** Full branch diff `TaskMaster-wt-2026-07-03-10-11` @ `b495fd34e341b4816be8676295c3f4a04613764b` vs base `main` @ merge-base `00507b595297c3e6970634a1855f1144c987dbdf`. Changed C# files:
- `QuickFiler/Controllers/QfcCollectionController.cs` (Part A navigation-key fix)
- `QuickFiler/Controllers/QfcDatamodel.cs` (Part B additive debug logging; caller-context string corrected in cycle 1)
- `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` (Part B additive debug logging + new `logger` field)
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` (Part B additive debug logging)
- `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` (4 new MSTest regression tests)

Non-code changes: feature-folder docs (`spec.md`, `issue.md`, `plan.2026-07-03T10-36.md`, prior audits, remediation artifacts), evidence artifacts under `.../evidence/**` (now including the persisted Cobertura `coverage.xml`), and task-researcher agent-memory markdown. No Python, PowerShell, TypeScript, or Bash source files changed on this branch.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 4 production + 1 test | 4641 (full suite; +4 new) | 4641 pass, 0 fail | 76.5758% lines (repo-wide) | 76.5750% lines (repo-wide; 40355/52700) | `QfcHighConfidencePreFilter.cs` 100% (artifact-verified) |
| Python | 0 files | N/A | N/A | N/A (not in scope) | N/A (not in scope) | N/A |
| PowerShell | 0 files | N/A | N/A | N/A (not in scope) | N/A (not in scope) | N/A |
| TypeScript | 0 files | N/A | N/A | N/A (not in scope) | N/A (not in scope) | N/A |

**Note:** C# is the only language with changed source files on this branch; its coverage verdict is a required explicit PASS/FAIL (see Section 1.2.1).

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- C# baseline coverage artifact (machine-readable Cobertura): **PRESENT** — repo-wide baseline figures recorded in `evidence/qa-gates/coverage-verification.2026-07-03T16-58.md` (recorded prior-cycle baseline 76.5758%, this-cycle Phase 0 baseline 76.5750%); no repo-wide regression.
- C# post-change coverage artifact (machine-readable Cobertura): **PRESENT** — `artifacts/csharp/coverage.xml` and committable copy `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/coverage/2026-07-03T16-58/coverage.xml` (byte-identical, SHA-256 `a80f5ae3d3d4f9de59d886be445c2dd4df3789aca35c8a14e3b7181eb10f19d7`). Root `<coverage>` line-rate 0.76574952561669829 (76.5750%), lines-covered 40355, lines-valid 52700.
- Per-language comparison summary: Section 1.2.1 below.

**Non-negotiable verdict rule:** No PASS may be reported without numeric baseline and post-change coverage plus changed/new-code coverage for every in-scope language. The C# coverage verdict is **PASS**: the machine-readable Cobertura artifact is present, all six `QfcHighConfidencePreFilter.cs` classes report line-rate=1 (100% >= 90%), and repo-wide coverage is flat within variance versus baseline (no regression). The residual raw repo-wide figure below the 80% floor is the pre-existing, ratified COM/VSTO/WinForms exemption state (CLAUDE.md; Issue #227), not introduced or worsened by this change.

---

## Executive Summary

This is the exit re-audit of the full branch diff for Issue #232 against base `main` (merge-base `00507b59`, head `b495fd34`) following remediation cycle 1. The branch delivers two bundled, non-overlapping C# changes: (Part A) a navigation-key duplicate-registration crash fix in `QfcCollectionController.cs` with four new MSTest regression tests, and (Part B) additive folder-confidence probability debug logging across three controller files. The excluded larger dequeue-time high-confidence rework (feature #233) is out of scope by design and confirmed untouched.

Remediation cycle 1 addressed the sole prior blocking finding — the absent machine-readable C# coverage artifact — by regenerating and persisting the Cobertura `coverage.xml` at the canonical `artifacts/csharp/coverage.xml` and a committable copy under the feature evidence tree, then re-verifying coverage directly from that artifact. Cycle 1 also applied a one-line caller-context log-string correction in `QfcDatamodel.cs` (the string previously named a different method than the one it was emitted from; it now correctly names `ScoreRemainingQueueMailItemAsync`).

All four toolchain gates are accounted for: csharpier (0 files changed), .NET analyzers (0 errors, no new diagnostics), and MSTest (4641/4641 pass) are clean; the nullable gate exits non-zero solely due to pre-existing legacy VSTO nullable debt, with a documented byte-identical before/after error population proving zero new nullable diagnostics from this change. The four regression tests are deterministic, isolated, and mock-based (no live-Outlook/COM dependency, no temp files).

**Blocking findings: 0.** The prior blocking coverage-artifact finding is resolved and independently verified from the persisted XML. Coverage now verifies directly from a machine-readable artifact: the non-exempt `QfcHighConfidencePreFilter.cs` changed lines are 100% (>= 90%), and repo-wide coverage is flat at ~76.57% (exemption-governed testable denominator). Remaining items are non-blocking and pre-existing: repo-wide raw coverage below the generic 80% floor (ratified exemption), the 2308-line `QfcCollectionController.cs` (pre-existing overage), and the test file now at exactly the 500-line cap.

**Policy documents evaluated:**
- `CLAUDE.md` (all sections; C# Code Change Policy, General/C# Unit Test Policy, COM/VSTO/WinForms coverage exemption)
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md` (via CLAUDE.md embedded C# policy)

**Language-specific policies evaluated:**
- N/A `python-*` (no Python files changed)
- N/A `powershell-*` (no PowerShell files changed)
- N/A Bash / JSON (no such files changed)
- C# Code Change Policy + C# Unit Test Policy (MSTest/Moq/FluentAssertions)

**Temporary artifacts cleanup:**
- No temporary/one-time scripts were created by this review (review is check-only, no mutation of source or policy).
- N/A No ongoing tooling scripts were added by the change.

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | PASS | The 4 new tests build fresh controller instances via `FormatterServices.GetUninitializedObject` and a fresh `KbdActions` per test (`CreateControllerForSwap`). No shared mutable static state; the registry is per-test. |
| **Isolation** - Each test targets single behavior | PASS | Each `[TestMethod]` targets one behavior: reported-repro registration boundary, swap add/remove, duplicate-register throw, guarded-skip final state. |
| **Fast Execution** - Tests complete quickly | PASS | Full suite 4641 tests in 52.88s (`vstest-final.2026-07-03T16-58.md`); the Part B subset ran 29 tests in ~1.3s. New tests are pure in-memory. |
| **Determinism** - Consistent results | PASS | No randomness, time, or I/O; Moq `Loose` mocks and an in-memory `KbdActions`. No temp files. |
| **Readability & Maintainability** - Clear structure | PASS | Descriptive method names encode scenario; each test carries an XML-doc summary and Arrange/Act/Assert comments. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | PASS | **Baseline:** 76.5758% lines repo-wide (recorded prior-cycle), 76.5750% (this-cycle Phase 0); `QfcHighConfidencePreFilter.cs` 100%. Recorded in `evidence/qa-gates/coverage-verification.2026-07-03T16-58.md` with the persisted Cobertura XML as source of truth. |
| **No Coverage Regression** | PASS | **Baseline 76.5758% → Post-change 76.5750% lines** (delta -0.00084 pp, within run-to-run variance). No changed production line is uncovered. Verified from the persisted XML root `<coverage>` element (40355/52700). |
| **New Code Coverage >=90%** | PASS | **New/modified non-exempt file:** `QfcHighConfidencePreFilter.cs`. **New/changed-code coverage: 100%** — all six mapped `<class>` elements report line-rate=1, verified directly from `evidence/coverage/2026-07-03T16-58/coverage.xml`. |
| **Comprehensive Coverage** | PASS | Part A behavior verified via 4 mock-based tests (register/unregister ordering, duplicate-key throw, guarded skip). Part B verified as no-regression against 29 pre-existing tests (`part-b-logging-no-regression.md`). |
| **Positive Flows** - Valid inputs | PASS | `LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys`, `SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey`. |
| **Negative Flows** - Invalid inputs | PASS | `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException` asserts the duplicate-key `ArgumentException`. |
| **Edge Cases** - Boundary conditions | PASS | 1-item→0-item→swap transition and orphaned-key overlap encoded in `LoadControlsAndHandlers_01_ReportedRepro_...`. |
| **Error Handling** - Error paths | PASS | `ArgumentException` throw/no-throw paths asserted via FluentAssertions `Should().Throw`/`Should().NotThrow`. |
| **Concurrency** - If applicable | PARTIAL | The reentrancy counter (`removespecificcontrolgroupcounter`) and its race guard are not directly tested; the fix uses a method-local `bool` and does not alter the counter. Documented out-of-scope follow-up (AC8). Non-blocking. |
| **State Transitions** - If applicable | PASS | The registry-membership invariant across page swaps is asserted by counting `"Collection"`-sourced entries before/after swap. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 76.5758% lines. Post-change: 76.5750% lines. Change: -0.00084% lines. New/changed-code coverage: 100%. Disposition: PASS. Evidence: `evidence/coverage/2026-07-03T16-58/coverage.xml` (and byte-identical `artifacts/csharp/coverage.xml`), `evidence/qa-gates/coverage-verification.2026-07-03T16-58.md`, `evidence/qa-gates/coverage-delta.md`. Disposition rationale: the machine-readable Cobertura artifact is present; all six `<class>` elements mapped to the non-exempt `QfcHighConfidencePreFilter.cs` report line-rate=1 (100% >= 90%), and repo-wide coverage is flat within variance versus baseline (no regression). The residual raw repo-wide figure below the 80% generic floor is the pre-existing, ratified COM/VSTO/WinForms testable-denominator exemption (CLAUDE.md; Issue #227), not introduced or worsened by this change.
- Python: Baseline: N/A - out of scope. Post-change: N/A - out of scope. Change: N/A. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (no Python files changed).
- PowerShell: Baseline: N/A - out of scope. Post-change: N/A - out of scope. Change: N/A. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (no PowerShell files changed).
- TypeScript: Baseline: N/A - out of scope. Post-change: N/A - out of scope. Change: N/A. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (no TypeScript files changed).

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | PASS | FluentAssertions `WithMessage("*Key 2 SourceId Collection*")` and count assertions yield actionable diagnostics. |
| **Arrange-Act-Assert Pattern** | PASS | Every new test is explicitly commented Arrange/Act/Assert. |
| **Document Intent** | PASS | Each test has an XML-doc summary tying it to the AC/phase task (e.g., `[P3-T1] (AC1) ...`). |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | PASS | No database, network, Outlook/COM, or filesystem access; `IQfcFormViewer.L1v0L2L3v_TableLayout` returns null to avoid WinForms paths. |
| **Use Mocks/Stubs** | PASS | Moq `Loose` mocks for `IQfcKeyboardHandler`, `IEmailMoveMonitor`, `IQfcFormViewer`, and `MailItem`; a real in-memory `KbdActions` as the system-under-observation. |
| **Environment Stability** | PASS | No temp files, no mutable global config. `GetUninitializedObject` + reflection field injection is contained to the test helpers. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | PASS | This document plus `code-review.2026-07-03T17-25.md` and `feature-audit.2026-07-03T17-25.md` constitute the required review. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | PASS | Objective stated in `issue.md`/`spec.md` (Issue #232); root cause diagnosed in the research artifact. |
| **Read existing change plans** | PASS | `plan.2026-07-03T10-36.md` and `remediation-plan.2026-07-03T16-58.md` present; Phase 0 evidence records policy reads. |
| **Document the plan** | PASS | Atomic plan with phases P0–P6 and per-task evidence; remediation cycle 1 plan documents the coverage-artifact regeneration. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | PASS | Part A reuses the existing correct `SwapItemGroups` method rather than duplicating register/unregister logic; guard is a single method-local `bool`. |
| **Reusability** | PASS | The fix routes through an existing (previously dead) method, eliminating a divergent copy of the swap logic. |
| **Extensibility** | PASS | No public API change; `LoadControlsAndHandlers_01` signature and callers unchanged. |
| **Separation of concerns** | PASS | Part B logging is additive and read-only; no control-flow or scoring change. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | PASS | Changes are confined to `QuickFiler/Controllers/` and its dedicated test file. |
| **Under 500 lines** | PARTIAL | `QfcHighConfidencePreFilter.cs` 191, `QfcItemController.FolderHandling.cs` 216, `QfcDatamodel.cs` 442 — all under 500. `QfcCollectionControllerTests.cs` is exactly 500 (at the cap; not exceeding, but any further addition will violate). `QfcCollectionController.cs` is 2308 lines — a **pre-existing** overage not introduced by this change (net +12 lines to an already-2296-line legacy COM-bound controller). Non-blocking. See Section 8. |
| **Public vs internal** | PASS | `QfcHighConfidencePreFilter` remains `internal static`; new `logger` field is `private static readonly`. |
| **No circular dependencies** | PASS | No new type or namespace dependency introduced; only a new call site to an existing sibling method. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | PASS | `swapAlreadyRegistered` guard, test helper names, and log context strings are descriptive. |
| **Docs/docstrings** | PASS | New test helpers and tests carry XML-doc comments. |
| **Comment why, not what** | PASS | Both Part A edits carry a rationale comment citing Issue #232 (why the swap is routed through `SwapItemGroups` and why the trailing register is guarded). |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | PASS | `csharpier format .` EXIT 0, 0 files changed (`csharpier-final.2026-07-03T16-58.md`). |
| **2. Linting** | PASS | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` EXIT 0, `Build succeeded. 0 Error(s)`, no new diagnostics in touched files (`msbuild-analyzers-final.2026-07-03T16-58.md`). |
| **3. Type checking** | PARTIAL | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` EXIT 1 due to pre-existing legacy VSTO nullable debt. Forced-recompile before/after error population is byte-identical; zero new nullable diagnostics from this change (`msbuild-nullable-final.2026-07-03T16-58.md`). Non-blocking. |
| **4. Testing** | PASS | `vstest.console.exe ... /InIsolation` EXIT 0, 4641/4641 pass, 0 fail (`vstest-final.2026-07-03T16-58.md`). |
| **Full toolchain loop** | PASS | Order csharpier → analyzers → nullable → vstest; formatter changed 0 files, so no restart required. |
| **Explicit reporting** | PASS | Commands and results recorded in the `evidence/qa-gates/*` artifacts. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | PASS | `issue.md` Resolution and `spec.md` Proposed Fix summarize both parts; remediation-inputs and remediation-plan document cycle 1. |
| **Design choices explained** | PASS | Rejected Approach B (idempotent `Add`) documented; reuse of `SwapItemGroups` justified. |
| **Update supporting documents** | PASS | Spec ACs checked off; evidence inventory, coverage-verification, and follow-up candidates recorded. |
| **Provide next steps** | PASS | Follow-ups (#233, dormant #171 wiring, reentrancy hygiene) recorded in `evidence/other/follow-up-candidates.md`. |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3C-#: C# Code Change Policy Compliance

#### C#.1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with CSharpier** | PASS | `csharpier format .` EXIT 0; 0 files changed. |
| **Linting with .NET analyzers** | PASS | Analyzer build EXIT 0; no new diagnostics in the 5 touched files. |
| **Type checking (nullable)** | PARTIAL | Whole-solution nullable gate EXIT 1 from pre-existing legacy debt; zero new diagnostics proven by identical forced-recompile population. Non-blocking. |
| **Testing with MSTest via vstest** | PASS | 4641/4641 pass. |

#### C#.2 Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Strong contracts / explicit APIs** | PASS | No public signature change; `SwapItemGroups` reused unchanged. |
| **Null-safety by default** | PASS | Log calls use null-conditional access (`ItemHelper?.Subject`, `_folderHandler?.Suggestions?.TopScore() ?? 0`); no new nullable warnings. |
| **Composition / focused types** | PASS | Fix delegates to an existing method; no new inheritance. |
| **Async / resource safety** | PASS | `SkipGroupAsync` awaited within the dispatcher lambda; no new disposable resources. |

#### C#.3 Error Handling, Logging, Contracts

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Exceptions fail fast** | PASS | `KbdActions.Add` throw-on-duplicate contract preserved; fix removes the reachable collision rather than masking it. |
| **Logging pattern** | PASS | New `logger.Debug(...)` calls follow the existing log4net `ILog` convention; new `logger` field mirrors the established pattern. The `QfcDatamodel` caller-context string now accurately names the enclosing method (`ScoreRemainingQueueMailItemAsync`) after the cycle-1 correction. |
| **Contracts / invariants** | PASS | Registry-membership invariant (exactly one `"Collection"` entry per live key) enforced by the swap path. |

---

## 4. Language-Specific Unit Test Policy Compliance

### Section 4C-#: C# Unit Test Policy Compliance

#### 4C.1 Framework and Scope

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use MSTest** | PASS | New tests use `[TestClass]`/`[TestMethod]` (`Microsoft.VisualStudio.TestTools.UnitTesting`). |
| **Coverage expectation** | PASS | Non-exempt `QfcHighConfidencePreFilter.cs` changed lines 100% — verified from the persisted Cobertura artifact (all six mapped `<class>` line-rate=1). |

#### 4C.2 Libraries and Conventions

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Moq for mocks** | PASS | `Mock<IQfcKeyboardHandler>`, `Mock<IEmailMoveMonitor>`, `Mock<IQfcFormViewer>`, `Mock<MailItem>`. |
| **FluentAssertions** | PASS | `Should().Throw<ArgumentException>()`, `Should().Be(...)`, `Should().NotThrow()`. |
| **MSTest style** | PASS | Standard attributes; no xUnit/NUnit introduced. |

#### 4C.3 Naming and Readability

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Naming conventions** | PASS | Behavior-encoding method names; PascalCase types/members. |
| **Docstrings/comments** | PASS | XML-doc summaries reference AC/phase tasks. |

#### 4C.4 Running the Toolchain

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use vstest.console.exe** | PASS | `vstest-final.2026-07-03T16-58.md`, `part-b-logging-no-regression.md`, per-test evidence files. |
| **No alternative runners** | PASS | Only vstest/MSTest used. |

---

## 5. Test Coverage Detail

### QfcCollectionController navigation-key swap (4 tests)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| `LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix` | Edge Case / Regression boundary | swap path via `SwapItemGroups` + `RegisterNavigation` | PASS |
| `LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys` | Positive | unregister-outgoing + register-incoming | PASS |
| `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException` | Negative / Error Handling | duplicate-key throw | PASS |
| `SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey` | Positive / State Transition | guarded-skip final registry state | PASS |

**Coverage:** `QfcCollectionController.cs` carries a ratified `[ExcludeFromCodeCoverage]` (COM/WinForms exemption); behavioral verification is via the mock seam above rather than a numeric coverage obligation.

**Not covered (numeric):** N/A — file is coverage-exempt; behavior is asserted at the testable seam.

### QfcHighConfidencePreFilter / QfcDatamodel / QfcItemController.FolderHandling — Part B logging (29 pre-existing tests, no-regression)

| Test Group | Scenario Type | Status |
|-----------|--------------|--------|
| `QfcHighConfidencePreFilterTests.cs` (9) | No-regression over new log line + `logger` field | PASS |
| `QfcDatamodelTests.cs` (7) | No-regression over `ScoreRemainingQueueMailItemAsync` log | PASS |
| `QfcItemController.FolderHandlingTests.cs` (13) | No-regression over 4 `LoadFolderHandler(Async)` logs | PASS |

**Coverage:** `QfcHighConfidencePreFilter.cs` (non-exempt) changed lines 100% (artifact-verified). The other two files are within the ratified exemption.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 4641 | PASS |
| Tests Passed | 4641 (100%) | PASS |
| Tests Failed | 0 | PASS |
| Execution Time | 52.88s total | PASS - Fast |
| Average Time per Test | ~11.4ms | PASS - Fast |
| Discovery Time | Not separately reported | Info |
| Functions/Classes Tested | Part A seam + 3 Part B call sites | PASS |
| Test File Size | `QfcCollectionControllerTests.cs` 500 lines (at cap) | PARTIAL - At limit |
| Code Coverage | Repo-wide 76.5750% lines; non-exempt changed file 100% (artifact-verified) | PASS |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `csharpier format .` | 0 files changed | PASS |
| .NET Analyzers | `msbuild TaskMaster.sln /t:Build /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 errors; no new diagnostics | PASS |
| Nullable Type Check | `msbuild TaskMaster.sln /t:Build /p:Nullable=enable /p:TreatWarningsAsErrors=true` | EXIT 1; zero new diagnostics | PARTIAL |
| MSTest Tests | `vstest.console.exe ... /InIsolation` | 4641/4641 pass | PASS |

**Notes:**
The nullable gate exit-1 is a pre-existing legacy VSTO/.NET Framework condition unrelated to this change; `msbuild-nullable-final.2026-07-03T16-58.md` proves a byte-identical error population before and after. The single baseline vstest failure (`TryGetFileStreamWriter_...`, UtilitiesCS.Test) is a pre-existing flaky test unrelated to Issue #232 and passed on the final run.

---

## 8. Gaps and Exceptions

### Identified Gaps

- **C# coverage artifact absent — RESOLVED (was BLOCKING in cycle 0).** The machine-readable Cobertura coverage artifact is now persisted at the canonical `artifacts/csharp/coverage.xml` and the committable copy `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/coverage/2026-07-03T16-58/coverage.xml` (byte-identical, SHA-256 `a80f5ae3d3d4f9de59d886be445c2dd4df3789aca35c8a14e3b7181eb10f19d7`). This audit verified directly from the XML: all six `<class>` elements mapped to `QfcHighConfidencePreFilter.cs` report line-rate=1 (100% >= 90%); repo-wide root line-rate 0.76575 (40355/52700), flat versus baseline. Resolves `remediation-inputs.2026-07-03T16-58.md` Blocking Finding 1.
- **`QfcDatamodel` caller-context log string — RESOLVED (was Minor in cycle 0).** The `logger.Debug(...)` caller-context string now reads `[QfcDatamodel.ScoreRemainingQueueMailItemAsync (master-queue admission)]`, accurately naming the enclosing method. Verified in `git diff 00507b59..b495fd34 -- QuickFiler/Controllers/QfcDatamodel.cs`; provenance recorded in `evidence/other/qfcdatamodel-string-correction.2026-07-03T16-58.md`.
- **Repo-wide raw coverage below the 80% generic floor.** 76.5750% (40355/52700) across 7 first-party assemblies (UtilitiesCS 88.1%, QuickFiler 52.5%, TaskMaster 53.3%, Swordfish.NET.General 46.5%, TaskVisualization 18.3%, ToDoModel 2.3%, Tags 0%). This is a pre-existing repository state covered by the ratified COM/VSTO/WinForms testable-denominator exemption (CLAUDE.md; Issue #227). Not introduced or worsened by this change (flat within variance). Non-blocking.
- **`QfcCollectionController.cs` exceeds the 500-line limit (2308 lines).** Pre-existing; the change adds a net +12 lines to a legacy COM-bound controller. Refactoring is out of scope for this targeted bug fix and is not a new violation introduced by #232. Non-blocking.
- **`QfcCollectionControllerTests.cs` at exactly 500 lines.** At the cap, not exceeding. Any future addition to this file will breach the limit; a sibling test file is advisable next time. Non-blocking.

### Approved Exceptions

- COM/VSTO/WinForms coverage exemption (CLAUDE.md; ratified Issue #227) applies to `QfcCollectionController.cs`, `QfcDatamodel.cs`, and `QfcItemController.FolderHandling.cs`. `QfcHighConfidencePreFilter.cs` is NOT exempt and carries the >= 90% new/changed-code obligation (artifact-verified 100%).

### Removed/Skipped Tests

- **None.** No planned tests were removed or skipped. Part B logging is additive and verified via the 29 pre-existing tests (AC7).

---

## Rejected Scope Narrowing

**None.** The caller directed a full-branch audit against base `main` with explicit instruction not to narrow, and confirmed the dequeue/high-confidence-filtering rework remains intentionally excluded as feature #233. That exclusion is a design decision recorded in `spec.md` Scope & Non-Goals and confirmed untouched by AC8 evidence; it is not a caller-imposed narrowing of the audit scope. No narrowing to a plan, task, phase, or file subset was attempted.

## PR Context Summary Classification Note

The regenerated PR context summary overview (`artifacts/pr_context.summary.txt`, "Changed files overview") reports `Core logic changes: 0 files` and buckets the changed C# files under docs/tooling. This is a known summary-generation misclassification. This audit derived scope from `git diff 00507b59..b495fd34` directly (authoritative), which surfaces all five C# files (4 production + 1 test). Any coverage hook that reads changed languages from the summary overview would under-enforce C# coverage; this audit verified C# coverage directly from the persisted Cobertura artifact.

## Evidence Location Compliance

Scanned the branch diff for files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. **No violations found** — all evidence artifacts are written under the canonical `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/{baseline,qa-gates,regression-testing,coverage,other}/` tree. The persisted coverage XML is at the canonical evidence path `evidence/coverage/2026-07-03T16-58/coverage.xml` with a byte-identical canonical machine copy at `artifacts/csharp/coverage.xml` (an approved machine-readable-artifact location, not one of the prohibited `artifacts/{baselines,qa,evidence,coverage}/` paths). Note: the repo does not currently ship `validate_evidence_locations.py`; the scan was performed via `git diff --name-only` path inspection. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` conditions were triggered.

---

## 9. Summary of Changes

### Commits in This PR/Branch

1. Range `00507b59..b495fd34` on branch `TaskMaster-wt-2026-07-03-10-11`: the Part A crash fix + Part B logging, plus remediation cycle 1 (persisted Cobertura coverage artifact + one-line `QfcDatamodel` caller-context string correction).

### Files Modified (source)

1. **`QuickFiler/Controllers/QfcCollectionController.cs`** (MODIFIED) — route the item-groups swap in `LoadControlsAndHandlers_01` through `SwapItemGroups`; add a method-local `swapAlreadyRegistered` guard so the trailing `RegisterNavigation()` in `RemoveSpecificControlGroupAsync` is skipped after the zero-item skip path already registered.
2. **`QuickFiler/Controllers/QfcDatamodel.cs`** (MODIFIED) — one additive `logger.Debug` in `ScoreRemainingQueueMailItemAsync`; caller-context string corrected in cycle 1.
3. **`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`** (MODIFIED) — new `logger` field; one additive `logger.Debug` in the `FilterAsync` scoring lambda.
4. **`QuickFiler/Controllers/QfcItemController.FolderHandling.cs`** (MODIFIED) — four additive `logger.Debug` calls at the four folder-handler assignment points.
5. **`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`** (MODIFIED) — 4 new MSTest regression tests + test helpers (+172 lines; file now 500 lines).
6. Feature-folder docs, evidence, and the persisted Cobertura `coverage.xml` (NEW).

---

## 10. Compliance Verdict

### Overall Status: COMPLIANT

The change is well-implemented, correctly scoped, and passes the format/lint/test gates with strong regression evidence. The sole prior blocking finding (absent machine-readable C# coverage artifact) is resolved and independently verified from the persisted Cobertura XML; the prior Minor caller-context string finding is also resolved. The C# coverage verdict is PASS. There are **0 blocking findings**.

**Fail-closed check:** The mandatory machine-readable Cobertura coverage artifact for C# is present and verified; the coverage gate is satisfied.

---

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- PASS Before Making Changes: objective, plan, and change plan present.
- PASS Design Principles: simplicity via existing-method reuse.
- PARTIAL Module & File Structure: pre-existing 2308-line controller; test file at 500-line cap. Non-blocking.
- PASS Naming, Docs, Comments: rationale comments cite Issue #232.
- PARTIAL Toolchain Execution: 3/4 gates clean; nullable exit-1 is pre-existing legacy debt, zero new diagnostics. Non-blocking.
- PASS Summarize & Document: spec/issue/evidence updated.

#### Language-Specific Code Change Policy (Section 3)

**For C#:**
- PASS Tooling & Baseline: csharpier/analyzers/vstest clean; nullable no-regression.
- PASS Design & Type-Safety: null-safe additive logging; no API change.
- PASS Error Handling & Logging: throw-on-duplicate contract preserved; log4net convention followed; caller-context string corrected.

#### General Unit Test Policy (Section 1)
- PASS Core Principles: independent, isolated, fast, deterministic, readable.
- PASS Coverage & Scenarios: artifact-verified; 100% changed-line on the non-exempt file; repo-wide flat.
- PASS Test Structure: AAA + clear diagnostics.
- PASS External Dependencies: mock-based, no temp files, no COM.
- PASS Policy Audit: this document.

#### Language-Specific Unit Test Policy (Section 4)

**For C#:**
- PASS Framework & Scope: MSTest/Moq/FluentAssertions.
- PASS Coverage: 100% changed-line artifact-verified.
- PASS Naming & Readability: descriptive, documented.
- PASS Toolchain: vstest only.

---

### Metrics Summary

- 4641/4641 tests passing (100%)
- +4 deterministic mock-based regression tests
- Non-exempt changed-file coverage 100% (artifact-verified)
- Repo-wide raw coverage 76.5750% (pre-existing, exemption-covered; flat, no regression)
- csharpier / analyzers / vstest clean; nullable no-regression
- File organization: pre-existing 2308-line controller; test file at 500-line cap (non-blocking)

---

### Recommendation

**Ready for normal PR flow (0 blocking findings).** The remediation cycle 1 exit condition is met: the C# coverage gate is verified from the persisted machine-readable artifact and AC10 is PASS. All other policy dimensions are satisfied or covered by ratified exemptions. Remaining non-blocking items (pre-existing controller size, test file at cap, repo-wide raw coverage under the generic floor) are documented follow-ups, not merge blockers.

---

## Appendix A: Test Inventory

- `QfcCollectionControllerTests.cs` › LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix
- `QfcCollectionControllerTests.cs` › LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys
- `QfcCollectionControllerTests.cs` › RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException
- `QfcCollectionControllerTests.cs` › SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey
- `QfcHighConfidencePreFilterTests.cs` › FilterAsync_* (9 pre-existing; no-regression)
- `QfcDatamodelTests.cs` › ScoreRemainingQueueMailItemAsync and siblings (7 pre-existing; no-regression)
- `QfcItemController.FolderHandlingTests.cs` › LoadFolderHandler*/LoadFolderHandlerAsync* (13 pre-existing; no-regression)

---

## Appendix B: Toolchain Commands Reference

**For C#:**
```
# Formatting
csharpier format .

# Linting (analyzers)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

# Type checking (nullable)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true

# Testing + coverage
vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /Settings:<cobertura.runsettings> /ResultsDirectory:<persist-path>
```

---

**Audit Completed By:** feature-review agent
**Audit Date:** 2026-07-03 (exit re-audit, timestamp 2026-07-03T17-25)
**Policy Version:** Current (as of audit date)
