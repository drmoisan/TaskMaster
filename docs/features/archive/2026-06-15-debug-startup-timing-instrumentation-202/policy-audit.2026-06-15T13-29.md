# Policy Compliance Audit: debug-startup-timing-instrumentation (Issue #202)

**Audit Date:** 2026-06-15
**Code Under Test:** Branch `feature/debug-startup-timing-instrumentation-202` @ `1d193d90` vs base `main` @ `a21d09e1` (merge-base `a21d09e18dfebb9e3450c1b3322e7715c09d91e6`)

Files modified (source/test/build, all C#):
- `TaskMaster/AppGlobals/IStartupTimingRecorder.cs` (NEW, 40 lines)
- `TaskMaster/AppGlobals/StartupTimingRecorder.cs` (NEW, 95 lines)
- `TaskMaster/AppGlobals/ApplicationGlobals.cs` (MODIFIED, 247 lines)
- `TaskMaster/Properties/Settings.settings` (MODIFIED)
- `TaskMaster/Properties/Settings.Designer.cs` (MODIFIED, generated)
- `TaskMaster/TaskMaster.csproj` (MODIFIED, +2 Compile includes)
- `TaskMaster.Test/AppGlobals/StartupTimingRecorderTests.cs` (NEW, 184 lines)
- `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` (MODIFIED, 687 lines; baseline 440)
- `TaskMaster.Test/TaskMaster.Test.csproj` (MODIFIED, +1 Compile include)

Documentation/evidence (non-code): 21 files under `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/**` (scoping docs, plan, evidence artifacts).

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 9 files (6 .cs, 2 .csproj, 1 .settings) | 4194 tests | ✅ 4194 pass, 0 fail | 75.08% first-party prod lines (76.30% raw) | 75.12% first-party prod lines (76.36% raw) | 100% (StartupTimingRecorder + NullStartupTimingRecorder) |

**Note:** C# is the only language with changed files in the branch diff. TypeScript, Python, PowerShell, Bash, and JSON have zero changed files and are therefore N/A for coverage.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope` (no TypeScript files changed)
- TypeScript post-change coverage artifact: `N/A - out of scope` (no TypeScript files changed)
- PowerShell baseline coverage artifact: `N/A - out of scope` (no PowerShell files changed)
- PowerShell post-change coverage artifact: `N/A - out of scope` (no PowerShell files changed)
- Per-language comparison summary: see Section 1.2.1 below; underlying data `TestResults/final-full.cobertura.xml` and `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/coverage-delta.2026-06-15T12-15.md`

**Non-negotiable verdict rule:** Numeric baseline and post-change coverage metrics are present for the single in-scope language (C#), plus new-code coverage.

**Fail-closed rule:** Verifiable Cobertura coverage data exists at `TestResults/final-full.cobertura.xml` and was parsed directly for this audit. The canonical artifact path `artifacts/csharp/coverage.xml` named by the feature-review-workflow is absent; this is recorded as a Minor process finding (Section 8), not a coverage-data gap, because the equivalent Cobertura data was located and verified.

**Evidence rule:** All coverage figures below were read directly from `TestResults/final-full.cobertura.xml` (root `line-rate`, and per-class `line-rate` for the new and modified classes) and cross-checked against the executor's `coverage-delta` and `final-test-coverage` evidence artifacts. No figures were synthesized.

---

## Executive Summary

This branch adds enable-on-demand startup-timing instrumentation to the TaskMaster Outlook VSTO add-in (issue #202). The change is gated behind a new `Settings.Default.StartupTimingEnabled` user setting (default `False`). When enabled, `ApplicationGlobals.LoadAsync(parallel: false)` records wall-clock spans for the seven established phase seams (LoadBasic, IntelConfig, OlObjects, ToDo, AutoFile, Engines, Events) and emits one formatted `[Startup timing]` table via the existing log4net logger. Recording and formatting logic is isolated in a new, COM-free `IStartupTimingRecorder` abstraction with a production `StartupTimingRecorder` and a `NullStartupTimingRecorder` no-op used on the flag-off path.

The implementation is well-structured, separates pure formatting logic from the COM-bound startup coordinator, reuses the existing `PrettyPrinters.ToFormattedText` primitive, uses `Stopwatch` (avoiding the banned `DateTime.Now`/`UtcNow` API), and is covered by 11 new MSTest tests (7 recorder, 4 wiring). New-code coverage is 100% and the modified `ApplicationGlobals` shows no coverage regression (improved). The toolchain (CSharpier, analyzer build, nullable/TreatWarningsAsErrors build, MSTest+coverage) was executed by the implementing agent and recorded as EXIT_CODE 0 in feature evidence.

One FAIL-level policy finding was identified: `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` grew from 440 to 687 lines, exceeding the repository 500-line file-size limit (General Code Change Policy §4 / `.claude/rules/general-code-change.md`). One Minor process finding: the canonical C# coverage artifact `artifacts/csharp/coverage.xml` is absent (equivalent Cobertura data exists and was verified).

**Policy documents evaluated:**
- ✅ `CLAUDE.md` general code-change and unit-test policies
- ✅ `.claude/rules/general-code-change.md`
- ✅ `.claude/rules/general-unit-test.md`

**Language-specific policies evaluated:**
- N/A `python-code-change` + `python-unit-test` (no Python files changed)
- N/A `powershell-code-change` + `powershell-unit-test` (no PowerShell files changed)
- N/A Bash (no Bash files changed)
- N/A JSON (no governed JSON files changed)
- ✅ C# Code Change Policy + C# Unit Test Policy (CLAUDE.md C#1–C#7, CUT1–CUT3)

**Temporary artifacts cleanup:**
- ✅ No temporary/one-time scripts were introduced by this branch (diff is source/test/build/docs only).
- N/A No ongoing tooling scripts added.

---

## Rejected Scope Narrowing

The caller prompt supplied the base branch, merge-base SHA, head SHA, and feature folder, and explicitly instructed: "Determine review scope yourself from the branch diff against the merge-base and execute the full SKILL contract." No instruction attempted to narrow scope to a plan/task/phase subset, to a subset of changed files, or to mark any language as out of scope. No scope-narrowing was detected; none was rejected.

Note on PR-context misclassification: `artifacts/pr_context.summary.txt` reports "Core logic changes: 0 files" and classifies all changes as "Docs/templates/agents/tooling." This is a misclassification of the C# source changes. The audit scope was taken from the actual `git diff a21d09e1..1d193d90`, which contains 6 changed `.cs` files plus build/settings files, not from the summary's overview counters.

---

## Evidence Location Compliance

Scanned the branch diff for files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`:

```
git diff --name-only a21d09e1..1d193d90 | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"
```

Result: NONE. All evidence artifacts produced by the implementing agent are written to the canonical `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/<kind>/` layout (`baseline/`, `qa-gates/`, `issue-updates/`, `other/`). No non-canonical evidence-location violations found. Disposition: PASS.

(The repository does not ship a `validate_evidence_locations.py` script; verification was performed by direct diff scan.)

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | ✅ PASS | New recorder tests inject deterministic spans and share no state. Wiring tests in `ApplicationGlobalsTests.cs` save/restore `Settings.Default.StartupTimingEnabled` in `[TestInitialize]`/`[TestCleanup]` and are marked `[DoNotParallelize]` because they mutate the process-global settings singleton and the process-global log4net logger. |
| **Isolation** - Each test targets single behavior | ✅ PASS | Recorder tests target one behavior each (ordering, zero-duration, total sum, null-name throw, emit prefix, null-logger throw, null recorder no-op). Wiring tests target flag-off no-emit, flag-on phase order, single-table emission, and ordering/yield-count parity. |
| **Fast Execution** - Tests complete quickly | ✅ PASS | All durations injected; no real timing waits. Full suite of 4194 tests reported green (EXIT_CODE 0) in `final-test-coverage.2026-06-15T12-15.md`. |
| **Determinism** - Consistent results | ✅ PASS | No clock, no sleep, no filesystem, no network. `TestableApplicationGlobals.LoadBasicMethod` sets a fixed `TimeSpan.FromMilliseconds(7)`; recorder tests inject fixed spans. |
| **Readability & Maintainability** - Clear structure | ⚠️ PARTIAL | Test names are descriptive and AAA-structured with intent comments. However `ApplicationGlobalsTests.cs` is 687 lines, exceeding the 500-line limit (see Section 2.3 and Section 8). |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | ✅ PASS | **Baseline:** 75.08% first-party production lines (36372/48447); 76.30% raw. **Command:** `vstest.console.exe <7 assemblies> /Settings:TaskMaster.runsettings /EnableCodeCoverage /InIsolation`. **Timestamp:** 2026-06-15 12:15. Source: `evidence/baseline/test-coverage-baseline.2026-06-15T12-15.md`. |
| **No Coverage Regression** | ✅ PASS | **Post-change:** 75.12% first-party production lines (36436/48504); 76.36% raw. **Change:** +0.04 first-party, +0.06 raw. **Status:** No regression (improved). Verified from `TestResults/final-full.cobertura.xml` root `line-rate="0.7636476378107266"`. |
| **New Code Coverage >=90%** | ✅ PASS | **New files:** `StartupTimingRecorder.cs`. **New code coverage:** 100%. Verified directly: `TaskMaster.StartupTimingRecorder` `line-rate="1"` and `TaskMaster.NullStartupTimingRecorder` `line-rate="1"` in `final-full.cobertura.xml`. |
| **Comprehensive Coverage** | ✅ PASS | `StartupTimingRecorder.RecordPhase/FormatTable/EmitTable` and `NullStartupTimingRecorder` are exercised; `ApplicationGlobals` flag read, recorder selection, LoadBasic Stopwatch, per-phase recording, `StopAndRestart`, and `EmitTable` call are all covered. Uncovered remainder of `ApplicationGlobals` is the pre-existing, out-of-scope parallel path (user-story Non-Goal). |
| **Positive Flows** - Valid inputs | ✅ PASS | `RecordPhase_WithPositiveDurations_...`, `FormatTable_ContainsHeaders...`, `LoadAsync_WhenTimingEnabled_RecordsAllPhasesInStartupOrder...`, `..._EmitsExactlyOneTableWithPhaseNamesAndTotal`. |
| **Negative Flows** - Invalid inputs | ✅ PASS | `RecordPhase_WithNullPhaseName_ThrowsArgumentNullException`, `EmitTable_WithNullLogger_ThrowsArgumentNullException`. |
| **Edge Cases** - Boundary conditions | ✅ PASS | `RecordPhase_WithZeroDuration_IsCapturedAndRenderedWithoutError`; null recorder empty-table case. |
| **Error Handling** - Error paths | ✅ PASS | Null-argument throws verified with `WithParameterName`. |
| **Concurrency** - If applicable | ✅ PASS | Shared-global tests marked `[DoNotParallelize]` to avoid cross-class interference on the settings singleton and log4net logger. |
| **State Transitions** - If applicable | ✅ PASS | Recorder accumulates spans in call order; ordering asserted in `RecordedPhaseNames` and rendered output. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 75.08% first-party production lines -> Post-change: 75.12% first-party production lines. Change: +0.04% line delta. New/changed-code coverage: 100%. Disposition: PASS. Evidence: `TestResults/final-full.cobertura.xml`, `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/qa-gates/coverage-delta.2026-06-15T12-15.md`, `.../evidence/qa-gates/final-test-coverage.2026-06-15T12-15.md`.
- TypeScript: Baseline: N/A% lines -> Post-change: N/A% lines. Change: N/A% line delta. New/changed-code coverage: `N/A - out of scope`. Disposition: N/A. Evidence: `N/A - out of scope` (no TypeScript files changed in branch diff).
- PowerShell: Baseline: N/A% lines -> Post-change: N/A% lines. Change: N/A% line delta. New/changed-code coverage: `N/A - out of scope`. Disposition: N/A. Evidence: `N/A - out of scope` (no PowerShell files changed in branch diff).
- Python: Baseline: N/A% lines -> Post-change: N/A% lines. Change: N/A% line delta. New/changed-code coverage: `N/A - out of scope`. Disposition: N/A. Evidence: `N/A - out of scope` (no Python files changed in branch diff).

Repo-wide C# note: the 75.12% first-party production-only figure is below the literal 80% number, but the denominator includes the COM/VSTO/WinForms-bound and `[ExcludeFromCodeCoverage]`-exempt classes that CLAUDE.md formally exempts from the 80% floor (the floor applies to the testable denominator after exemptions). This is a pre-existing repository condition not introduced or worsened by #202; the feature improves the figure by +0.04. Disposition for the repo-wide gate: no regression; consistent with documented exemption.

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | ✅ PASS | FluentAssertions with `because` rationale strings throughout (e.g., "phases must be rendered in the order they were recorded."). |
| **Arrange-Act-Assert Pattern** | ✅ PASS | Each test uses explicit Arrange/Act/Assert comment sections. |
| **Document Intent** | ✅ PASS | Descriptive `MethodUnderTest_Condition_Expectation` names plus class/test docstrings. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | ✅ PASS | No database/network/process. log4net `MemoryAppender` is attached in-process for assertion; no live Outlook/COM. |
| **Use Mocks/Stubs** | ✅ PASS | Moq used for `IAppItemEngines` and `log4net.ILog`; `TestableApplicationGlobals` stubs COM collaborator construction via overridable seams. MSTest + Moq + FluentAssertions per CUT1/CUT2. |
| **Environment Stability** | ✅ PASS | No temporary files created. Settings singleton mutation is saved/restored per test; tests marked `[DoNotParallelize]` to isolate global state. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | ✅ PASS | This document is the required policy review. Outstanding item: 500-line test-file limit (Section 8). |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | ✅ PASS | Objective documented in `issue.md`, `spec.md`, `user-story.md` (issue #202). |
| **Read existing change plans** | ✅ PASS | `plan.2026-06-15T12-15.md` present with phased tasks and Phase 0 policy-read evidence. |
| **Document the plan** | ✅ PASS | Atomic plan plus per-phase toolchain gate evidence under `evidence/qa-gates/`. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | ✅ PASS | A small interface plus two implementations; a single shared `Stopwatch` with a `StopAndRestart` helper. No deep indirection. |
| **Reusability** | ✅ PASS | Reuses `UtilitiesCS.PrettyPrinters.ToFormattedText` (the same overload `SegmentStopWatch.GetDurations()` uses) rather than reimplementing column alignment. |
| **Extensibility** | ✅ PASS | `IStartupTimingRecorder` interface enables alternative implementations; flag selects concrete vs no-op. |
| **Separation of concerns** | ✅ PASS | Pure recording/formatting logic isolated from the COM-bound startup coordinator; recorder has no Outlook/COM/IO dependency. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | ✅ PASS | Interface, recorder+null-recorder, and coordinator wiring are each in cohesive files. |
| **Under 500 lines** | ❌ FAIL | `IStartupTimingRecorder.cs` 40, `StartupTimingRecorder.cs` 95, `ApplicationGlobals.cs` 247, `StartupTimingRecorderTests.cs` 184 are within limit. `ApplicationGlobalsTests.cs` is **687 lines** (baseline 440), exceeding the 500-line limit. The General Code Change Policy applies the limit to test code; the markdown/fixture exceptions do not apply. Remediation required (Section 8). |
| **Public vs internal** | ✅ PASS | Recorder types are `internal sealed`; consumed by tests via existing `InternalsVisibleTo("TaskMaster.Test")`. New `protected internal virtual LoadBasicMethod` is a minimal test seam, documented as such. |
| **No circular dependencies** | ✅ PASS | Recorder depends only on `System`, `UtilitiesCS`, and `log4net`; no back-dependency from those onto `ApplicationGlobals`. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | ✅ PASS | `RecordPhase`, `FormatTable`, `EmitTable`, `StartupTimingRecorder`, `NullStartupTimingRecorder`, `_loadBasicElapsed`, `StopAndRestart`. |
| **Docs/docstrings** | ✅ PASS | XML doc comments on interface members and classes describe contract, COM-free guarantee, and the deliberate non-wrapping of `SegmentStopWatch`. |
| **Comment why, not what** | ✅ PASS | Comments explain the LoadBasic-at-construction measurement rationale, the always-zero `SegmentStopWatch.GetDurations()` total pitfall, and the negligible-overhead flag-off design. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ✅ PASS | **Command:** `csharpier check .` (CSharpier v1.3.0). **Result:** EXIT_CODE 0 (`evidence/qa-gates/final-format.2026-06-15T12-15.md`). |
| **2. Linting** | ✅ PASS | **Command:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. **Result:** EXIT_CODE 0 (`evidence/qa-gates/final-analyzer.2026-06-15T12-15.md`). |
| **3. Type checking** | ✅ PASS | **Command:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. **Result:** EXIT_CODE 0 (`evidence/qa-gates/final-typecheck.2026-06-15T12-15.md`). |
| **4. Testing** | ✅ PASS | **Command:** `vstest.console.exe <7 assemblies> /Settings:TaskMaster.runsettings /EnableCodeCoverage /InIsolation /ResultsDirectory:TestResults/final-full`. **Result:** 4194/4194 passed, EXIT_CODE 0. |
| **Full toolchain loop** | ✅ PASS | Final pass recorded clean across all four steps in the feature evidence. |
| **Explicit reporting** | ✅ PASS | Commands and results documented in `evidence/qa-gates/` artifacts and re-cited in this audit. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | ✅ PASS | Plan and evidence artifacts summarize the change. |
| **Design choices explained** | ✅ PASS | `evidence/other/timing-recorder-format-reuse.2026-06-15T12-15.md` records the formatter-reuse / non-wrapping decision and the log4net-vs-Console channel decision. |
| **Update supporting documents** | ✅ PASS | `spec.md`, `user-story.md`, `issue.md` AC items updated and checked off; plan check-offs recorded. |
| **Provide next steps** | ✅ PASS | Plan verification notes describe test scope and coverage evidence. |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3 (C#): C# Code Change Policy Compliance

#### C# Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with CSharpier** | ✅ PASS | `csharpier check .` EXIT_CODE 0. `dotnet format` not used. |
| **Linting / .NET analyzers** | ✅ PASS | Analyzer build EXIT_CODE 0 with `EnableNETAnalyzers`/`EnforceCodeStyleInBuild`. |
| **Type checking / nullable** | ✅ PASS | Nullable + `TreatWarningsAsErrors` build EXIT_CODE 0. New code uses guard clauses and `new()` target-typed; no nullable warnings introduced. |

#### C# Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Strong contracts and explicit APIs** | ✅ PASS | Interface with explicit `TimeSpan`/`string`/`log4net.ILog` types; XML-documented contracts. |
| **Null-safety by default** | ✅ PASS | `RecordPhase` and `EmitTable` throw `ArgumentNullException` on null inputs. |
| **Composition and focused types** | ✅ PASS | Two small sealed implementations behind one interface; no inheritance beyond the interface. |
| **Asynchrony and resource safety** | ✅ PASS | No new disposables; existing async phase awaits unchanged. `Stopwatch` requires no disposal. |

#### C# Module & Structure / Naming / Dependencies

| Requirement | Status | Evidence |
|------------|--------|----------|
| **File focus and size** | ⚠️ PARTIAL | Production files are focused and within limit; the modified test file exceeds 500 lines (see Section 2.3 FAIL). |
| **Internal-by-default surface** | ✅ PASS | Recorder types `internal sealed`. |
| **Banned-API compliance** | ✅ PASS | Uses `System.Diagnostics.Stopwatch` (hardware-counter), explicitly avoiding the BannedApiAnalyzers `DateTime.Now`/`DateTime.UtcNow` rule, per spec. No banned APIs introduced. |
| **No new dependencies** | ✅ PASS | `UtilitiesCS.PrettyPrinters`, `Stopwatch`, `log4net` already present and approved. |

---

## 4. Language-Specific Unit Test Policy Compliance

### Section 4 (C#): C# Unit Test Policy Compliance

#### Framework and Scope

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use MSTest** | ✅ PASS | `[TestClass]`/`[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. No xUnit/NUnit introduced. |
| **Mocking with Moq** | ✅ PASS | `Mock<IAppItemEngines>`, `Mock<log4net.ILog>` with `MockBehavior.Strict`/`Loose` as appropriate. |
| **Assertions with FluentAssertions** | ✅ PASS | FluentAssertions used throughout (`.Should().Be/Contain/Throw/Equal`). |
| **Coverage expectation** | ✅ PASS | New code 100% (>=90% floor); modified `ApplicationGlobals` no regression. |

#### Test Style and Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Focused unit tests** | ✅ PASS | One behavior per test. |
| **Mocking sparingly** | ✅ PASS | Mocks limited to the COM-bound engine collaborator and the logger sink. |
| **Organization** | ✅ PASS | Test files mirror source location (`AppGlobals/`). |
| **Naming and readability** | ⚠️ PARTIAL | Names/structure are clear, but the modified test file size exceeds the limit. |

---

## 5. Test Coverage Detail

### StartupTimingRecorder / NullStartupTimingRecorder (7 tests)

| Test Name | Scenario Type | Status |
|-----------|--------------|--------|
| RecordPhase_WithPositiveDurations_PreservesPhaseNamesInRecordedOrder | Positive | ✅ |
| RecordPhase_WithZeroDuration_IsCapturedAndRenderedWithoutError | Edge Case | ✅ |
| FormatTable_ContainsHeadersPhaseNamesAndTotalEqualToSumOfInjectedSpans | Positive | ✅ |
| RecordPhase_WithNullPhaseName_ThrowsArgumentNullException | Negative | ✅ |
| EmitTable_LogsFormattedTableViaLoggerInfoWithStartupTimingPrefix | Positive | ✅ |
| EmitTable_WithNullLogger_ThrowsArgumentNullException | Negative | ✅ |
| NullStartupTimingRecorder_IsNoOp_ForFormatAndEmit | Edge Case | ✅ |

**Coverage:** 100% of `StartupTimingRecorder.cs` (30/30 lines).

**Not covered:** None.

### ApplicationGlobals startup-timing wiring (4 tests)

| Test Name | Scenario Type | Status |
|-----------|--------------|--------|
| LoadAsync_WhenTimingDisabled_RecordsNothingAndEmitsNoTable | Negative (flag off) | ✅ |
| LoadAsync_WhenTimingEnabled_RecordsAllPhasesInStartupOrderWithLoadBasicFirst | Positive | ✅ |
| LoadAsync_WhenTimingEnabled_EmitsExactlyOneTableWithPhaseNamesAndTotal | Positive | ✅ |
| LoadAsync_PreservesPhaseOrderingAndYieldCount_WhenTimingOnVersusOff | State Transition | ✅ |

**Coverage:** `TaskMaster.ApplicationGlobals` class line-rate 77.6% (file aggregate incl. nested types 73.88%, up from 60.75% baseline). New timing lines covered; uncovered remainder is the pre-existing out-of-scope parallel path.

**Not covered:** Pre-existing `LoadParallelAsync` body and parallel branch (user-story Non-Goal; uncovered in baseline; no regression).

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 4194 | ✅ |
| Tests Passed | 4194 (100%) | ✅ |
| Tests Failed | 0 | ✅ |
| New Tests Added | 11 (7 recorder + 4 wiring) | ✅ |
| Functions/Classes Tested (new) | 2/2 new classes (100%) | ✅ |
| Test File Size | `ApplicationGlobalsTests.cs` 687 lines | ❌ Exceeds 500-line limit |
| Code Coverage (new code) | 100% lines | ✅ |
| Code Coverage (repo-wide first-party prod) | 75.12% lines (no regression; COM/VSTO exemption applies) | ⚠️ Below literal 80%, exemption-consistent, no regression |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `csharpier check .` | EXIT_CODE 0 | ✅ |
| .NET Analyzer Build | `msbuild TaskMaster.sln /t:Build /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT_CODE 0 | ✅ |
| Nullable Type-check Build | `msbuild TaskMaster.sln /t:Build /p:Nullable=enable /p:TreatWarningsAsErrors=true` | EXIT_CODE 0 | ✅ |
| MSTest Tests + Coverage | `vstest.console.exe <7 assemblies> /Settings:TaskMaster.runsettings /EnableCodeCoverage /InIsolation` | 4194/4194 pass, EXIT_CODE 0 | ✅ |

**Notes:**
The repo-wide first-party production-only figure (75.12%) is below the literal 80% number but is a pre-existing condition consistent with the documented COM/VSTO/WinForms exemption; #202 improves it by +0.04. The toolchain evidence reflects the implementing agent's runs; this reviewer did not re-execute the build/test (check-only review), and parsed the existing Cobertura artifact for coverage verification.

---

## 8. Gaps and Exceptions

### Identified Gaps

- **Under 500 lines (test file):** `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` is 687 lines (baseline 440), exceeding the General Code Change Policy 500-line file-size limit. The added startup-timing wiring tests and helper methods (`AttachMemoryAppender`, `DetachMemoryAppender`, `SetEnginesMock`, expanded `TestableApplicationGlobals`) pushed the file over the limit. Remediation: split the startup-timing wiring tests (and their helpers) into a separate test file, for example `TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs`, so each file is under 500 lines. This is a FAIL-level policy finding and a remediation trigger.
- **Canonical C# coverage artifact absent (Minor/process):** The feature-review-workflow names `artifacts/csharp/coverage.xml` as the C# coverage artifact. That path does not exist. Equivalent Cobertura coverage data exists at `TestResults/final-full.cobertura.xml` and was parsed directly; all coverage thresholds were verifiable. Recommendation: emit/copy the canonical `artifacts/csharp/coverage.xml` to satisfy the workflow's artifact contract. This does not change any coverage verdict.

### Approved Exceptions

- **Repo-wide 80% floor on first-party production-only denominator:** The 75.12% figure is below the literal 80% number but is consistent with the CLAUDE.md COM/VSTO/WinForms coverage exemption (the floor applies to the testable denominator after exemptions). This is a pre-existing repository condition, not introduced or worsened by #202 (+0.04 improvement). No regression. Treated as exemption-consistent, not a new violation.

### Removed/Skipped Tests

- **None.** All planned tests implemented; 11 new tests added.

---

## 9. Summary of Changes

### Commits in This PR/Branch

Branch `feature/debug-startup-timing-instrumentation-202` @ `1d193d90` vs base `main` @ `a21d09e1`. Commit-by-commit enumeration is available via `git log a21d09e1..1d193d90`; the net diff is summarized below.

### Files Modified

1. **TaskMaster/AppGlobals/IStartupTimingRecorder.cs** (NEW) — internal recorder contract (RecordPhase/FormatTable/EmitTable), COM-free.
2. **TaskMaster/AppGlobals/StartupTimingRecorder.cs** (NEW) — production recorder (own ordered span collection, summed TOTAL, `PrettyPrinters.ToFormattedText` reuse) and `NullStartupTimingRecorder` no-op.
3. **TaskMaster/AppGlobals/ApplicationGlobals.cs** (MODIFIED) — flag read, recorder selection, LoadBasic Stopwatch measurement, per-phase recording in `LoadSequentialAsync`, `StopAndRestart` helper, end-of-load `EmitTable`, `protected internal virtual LoadBasicMethod` test seam.
4. **TaskMaster/Properties/Settings.settings** + **Settings.Designer.cs** (MODIFIED) — new `StartupTimingEnabled` user setting (default False).
5. **TaskMaster/TaskMaster.csproj** + **TaskMaster.Test/TaskMaster.Test.csproj** (MODIFIED) — Compile includes for the new files.
6. **TaskMaster.Test/AppGlobals/StartupTimingRecorderTests.cs** (NEW) — 7 recorder unit tests.
7. **TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs** (MODIFIED) — 4 wiring tests + helpers (now 687 lines; over limit).
8. Documentation/evidence (NEW) — scoping docs, plan, and evidence artifacts under the feature folder.

---

## 10. Compliance Verdict

### Overall Status: ⚠️ PARTIALLY COMPLIANT

The implementation is correct, well-structured, well-documented, fully toolchain-clean, and meets the new-code (100%) and no-regression coverage requirements. One FAIL-level policy violation prevents a clean PASS: the modified test file `ApplicationGlobalsTests.cs` (687 lines) exceeds the 500-line file-size limit. One Minor process gap: the canonical `artifacts/csharp/coverage.xml` is absent (equivalent verified data exists).

**Fail-closed reminder:** Coverage data was located and verified; no required coverage metric is missing. The blocking item is the file-size limit, which is a policy violation, not a missing-artifact condition.

---

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- ✅ Before Making Changes: complete
- ✅ Design Principles: simplicity, reuse, extensibility, separation all met
- ❌ Module & File Structure: test file exceeds 500-line limit
- ✅ Naming, Docs, Comments: strong
- ✅ Toolchain Execution: all four steps EXIT_CODE 0
- ✅ Summarize & Document: complete

#### Language-Specific Code Change Policy (Section 3 — C#)
- ✅ Tooling & Baseline: CSharpier + analyzers + nullable clean
- ✅ Design & Type-Safety: explicit contracts, null guards, banned-API avoidance
- ⚠️ Structure & Naming: test file size (see file-size FAIL)

#### General Unit Test Policy (Section 1)
- ✅ Core Principles: met (readability PARTIAL due to file size)
- ✅ Coverage & Scenarios: new-code 100%, no regression, scenarios complete
- ✅ Test Structure: AAA + clear diagnostics
- ✅ External Dependencies: no external deps, no temp files
- ✅ Policy Audit: this document

#### Language-Specific Unit Test Policy (Section 4 — C#)
- ✅ Framework & Scope: MSTest + Moq + FluentAssertions
- ✅ Test Style & Structure: focused, mirrored
- ⚠️ Naming & Readability: clear but file over size limit

---

### Metrics Summary

- ✅ 4194/4194 tests passing (100%)
- ✅ 2/2 new classes tested (100%)
- ✅ 100% new-code line coverage
- ✅ No coverage regression (+0.04 first-party, +0.06 raw)
- ✅ All code-quality checks (format/analyze/nullable/test) EXIT_CODE 0
- ❌ `ApplicationGlobalsTests.cs` 687 lines exceeds 500-line limit

---

### Recommendation

**Needs revision** — Address the 500-line test-file violation before merge by splitting `ApplicationGlobalsTests.cs`. The Minor canonical-coverage-artifact gap should also be resolved but does not change any coverage verdict. All other policy areas are compliant.

---

## Appendix A: Test Inventory

- TaskMaster.Test.AppGlobals.StartupTimingRecorderTests › RecordPhase_WithPositiveDurations_PreservesPhaseNamesInRecordedOrder
- TaskMaster.Test.AppGlobals.StartupTimingRecorderTests › RecordPhase_WithZeroDuration_IsCapturedAndRenderedWithoutError
- TaskMaster.Test.AppGlobals.StartupTimingRecorderTests › FormatTable_ContainsHeadersPhaseNamesAndTotalEqualToSumOfInjectedSpans
- TaskMaster.Test.AppGlobals.StartupTimingRecorderTests › RecordPhase_WithNullPhaseName_ThrowsArgumentNullException
- TaskMaster.Test.AppGlobals.StartupTimingRecorderTests › EmitTable_LogsFormattedTableViaLoggerInfoWithStartupTimingPrefix
- TaskMaster.Test.AppGlobals.StartupTimingRecorderTests › EmitTable_WithNullLogger_ThrowsArgumentNullException
- TaskMaster.Test.AppGlobals.StartupTimingRecorderTests › NullStartupTimingRecorder_IsNoOp_ForFormatAndEmit
- TaskMaster.Test.AppGlobals.ApplicationGlobalsTests › LoadAsync_WhenTimingDisabled_RecordsNothingAndEmitsNoTable
- TaskMaster.Test.AppGlobals.ApplicationGlobalsTests › LoadAsync_WhenTimingEnabled_RecordsAllPhasesInStartupOrderWithLoadBasicFirst
- TaskMaster.Test.AppGlobals.ApplicationGlobalsTests › LoadAsync_WhenTimingEnabled_EmitsExactlyOneTableWithPhaseNamesAndTotal
- TaskMaster.Test.AppGlobals.ApplicationGlobalsTests › LoadAsync_PreservesPhaseOrderingAndYieldCount_WhenTimingOnVersusOff

---

## Appendix B: Toolchain Commands Reference

**For C#:**
```powershell
# Formatting
dotnet tool run csharpier .

# Linting / analyzers
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

# Type checking / nullable
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true

# Testing + coverage
vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll /Settings:TaskMaster.runsettings /EnableCodeCoverage /InIsolation /ResultsDirectory:TestResults/final-full
```

Coverage verification (this audit, check-only):
```bash
# Repo-wide root line-rate
grep -oE '<coverage[^>]*line-rate="[0-9.]*"' TestResults/final-full.cobertura.xml | head -1
# New + modified class line-rates
grep -oE '<class line-rate="[0-9.]*"[^>]* name="TaskMaster\.(StartupTimingRecorder|NullStartupTimingRecorder|ApplicationGlobals)"' TestResults/final-full.cobertura.xml
```

---

**Audit Completed By:** feature-reviewer agent
**Audit Date:** 2026-06-15
**Policy Version:** Current (as of audit date)
