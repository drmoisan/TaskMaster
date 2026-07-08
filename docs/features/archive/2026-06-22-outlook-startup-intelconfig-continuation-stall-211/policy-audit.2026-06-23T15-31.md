# Policy Compliance Audit: Issue #211 Engines-phase + IntelConfig continuation attribution instrumentation

**Audit Date:** 2026-06-23
**Code Under Test:** Full branch diff `main..HEAD` (`bug/outlook-startup-intelconfig-continuation-stall-211`), merge-base `9385bf607aca6c5722f2da7961a895c685710942`, head `e3a84b5dc4544aaf8b498dfed4e7b45708c9c12a`.

C# files changed (production):
- `TaskMaster/AppGlobals/EngineInitTimingProbe.cs` (NEW, 97 lines)
- `TaskMaster/AppGlobals/AppItemEngines.cs` (MODIFIED, 279 lines)
- `TaskMaster/AppGlobals/ApplicationGlobals.cs` (MODIFIED, 263 lines)
- `TaskMaster/TaskMaster.csproj` (MODIFIED, +1 Compile include)

C# files changed (test):
- `TaskMaster.Test/AppGlobals/EngineInitTimingProbeTests.cs` (NEW, 142 lines)
- `TaskMaster.Test/AppGlobals/ContinuationProbeSequenceTests.cs` (NEW, 107 lines)
- `TaskMaster.Test/AppGlobals/ApplicationGlobalsStartupTimingTests.cs` (MODIFIED, 301 lines)
- `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` (MODIFIED, 485 lines)
- `TaskMaster.Test/TaskMaster.Test.csproj` (MODIFIED, +2 Compile includes)

Docs/evidence: 32 markdown files under the active feature folder (scoping docs, plans, evidence, prior review artifacts).

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 4 prod + 5 test files | 20 AppGlobals tests run by reviewer; 4318 full non-live suite (executor) | ✅ 20 pass, 0 fail (reviewer); 4318 pass, 0 fail (executor) | 64.04% lines repo-wide aggregate | 64.05% lines repo-wide aggregate | 100% (EngineInitTimingProbe) |

**Note:** This change set is C# only. There are no Python, PowerShell, TypeScript, or Bash production/test files in the branch diff; coverage for those languages is N/A because they have zero changed files on the branch.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: N/A - out of scope (zero TypeScript files in branch diff)
- TypeScript post-change coverage artifact: N/A - out of scope (zero TypeScript files in branch diff)
- PowerShell baseline coverage artifact: N/A - out of scope (zero PowerShell files in branch diff)
- PowerShell post-change coverage artifact: N/A - out of scope (zero PowerShell files in branch diff)
- C# baseline coverage artifact: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/baseline/baseline-tests-coverage-2026-06-23T14-30.md`
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml` (reviewer-regenerated Cobertura, targeted AppGlobals run) and `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/qa-gates/final-qc-tests-coverage-2026-06-23T14-30.md` (executor full non-live run)
- Per-language comparison summary: Section 1.2.1 below

**Non-negotiable verdict rule:** This audit includes numeric baseline and post-change coverage for the only in-scope language (C#) plus new-code coverage.

---

## Executive Summary

The branch delivers two behavior-preserving diagnostic instrumentation increments for issue #211:

1. Phase 1 (relocated): the inter-phase yield in `ApplicationGlobals.LoadSequentialAsync` is replaced by `YieldWithContinuationProbeAsync(string priorPhaseName)`, which times how long each continuation waits to resume on the STA and emits one `[continuation-resume]` log line per boundary.
2. Phase 3 (new): `AppItemEngines.InitAsync` is instrumented through a new testable seam `EngineInitTimingProbe` that times the upfront `Configuration` await and each per-engine factory invocation, emitting `[engine-init-config]` and `[engine-init]` lines.

Both increments are diagnostic-only and behavior-preserving: phase order, engine set, filter/select semantics, and async outcomes are unchanged. Timing uses `Stopwatch` only; no banned timing APIs (`DateTime.Now`/`UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`) are introduced in source. All touched files are <= 500 lines.

The reviewer independently ran the full C# toolchain against the branch head: CSharpier check (PASS), analyzer build (PASS), nullable/TreatWarningsAsErrors build (PASS), and the targeted AppGlobals test set with coverage (20/20 PASS). The new `EngineInitTimingProbe` seam measures 100% line coverage, exceeding the >= 90% new-code floor.

The one PARTIAL finding is repo-wide coverage: the deterministic full-suite aggregate the executor recorded is 64.05% (vs 64.04% baseline), which is below the 80% repo-wide floor but is the pre-existing repository denominator and shows no regression from this change (slight increase). The 80% floor in this repo is applied to the testable denominator after the documented COM/VSTO/WinForms exemptions; the authoritative repo-wide-vs-floor determination is the PR CI run, which is not available locally. This is recorded as a known-state PARTIAL, not a regression introduced by #211.

**Policy documents evaluated:**
- ✅ `CLAUDE.md` (all sections)
- ✅ `.claude/rules/general-code-change.md`
- ✅ `.claude/rules/general-unit-test.md`
- ✅ `.claude/rules/csharp.md`

**Language-specific policies evaluated:**
- N/A `python-code-change` + `python-unit-test` (no Python files in diff)
- N/A `powershell-code-change` + `powershell-unit-test` (no PowerShell files in diff)
- N/A Bash (no Bash files in diff)
- N/A JSON (no governed JSON files in diff)
- ✅ C# Code Change Policy + C# Unit Test Policy (`.claude/rules/csharp.md`, CLAUDE.md C# sections)

**Temporary artifacts cleanup:**
- ✅ No throwaway scripts were created by this review.
- ✅ The reviewer regenerated `artifacts/csharp/coverage.xml` (the canonical C# coverage artifact path) from a targeted vstest run for evidence verification; this is the policy-defined coverage artifact location, not a temporary script.

---

## Rejected Scope Narrowing

The caller prompt included this paragraph:

> "Context you should be aware of (not a scope instruction): the branch contains the relocated Phase 1 IntelConfig continuation probe and the new Phase 3 Engines-phase attribution instrumentation. AC1–AC8 are intended to be delivered; AC9 is a maintainer-run non-debugger runtime capture (not CI-automatable) and AC10 is an evidence-gated Phase 4 fix that is intentionally not yet implemented. Assess the delivered code and artifacts on their merits against the full diff main..HEAD."

Disposition: This text is explicitly framed by the caller as context, not a scope instruction, and it directs assessment against the full diff `main..HEAD`. It does not narrow scope to a plan/task/phase subset, does not limit the file set, and does not mark any language with changed files as out of scope. No scope narrowing was applied; the full feature-vs-base audit was performed. This entry is recorded for completeness because the prompt characterizes delivery expectations for individual ACs (AC9/AC10 not yet delivered); the reviewer evaluated every AC independently against evidence rather than accepting the asserted statuses.

No other narrowing (plan/task/phase subset, file subset, or "informational only"/"not applicable" coverage marking for a language with changed files) was detected.

---

## Evidence Location Compliance

The reviewer scanned the branch diff for files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`.

Result: NONE. All feature evidence is written under the canonical `<FEATURE>/evidence/<kind>/` paths (`evidence/baseline/`, `evidence/qa-gates/`, `evidence/other/`). No non-canonical evidence-location violations were found in the branch diff. No `validate_evidence_locations.py` script is present in this worktree, so the scan was performed via `git diff --name-only` path filtering.

The reviewer-regenerated `artifacts/csharp/coverage.xml` is the policy-defined coverage-tool output path (per the feature-review-workflow coverage table) and is not a feature evidence artifact subject to the `<FEATURE>/evidence/` invariant.

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | ✅ PASS | New tests use local `List<string>` capture sinks and per-test SUT construction. `ContinuationProbeSequenceTests` and the modified startup-timing tests carry `[DoNotParallelize]` because they drive the process-global `ApplicationGlobals` seam; ordering independence is preserved by serialization, not shared mutable state. |
| **Isolation** - Each test targets single behavior | ✅ PASS | Each `EngineInitTimingProbeTests` method targets one probe behavior (ordered emission, null engine, config line, exception propagation, null-argument guards, null-sink guard). `ContinuationProbeSequenceTests` targets probe ordering and exact count. |
| **Fast Execution** - Tests complete quickly | ✅ PASS | Reviewer run: 20 tests in 2.96 s total; longest single test 488 ms (collaborator-materialization test), most < 10 ms. |
| **Determinism** - Consistent results | ✅ PASS | Stub factories return `Task.FromResult` / `Task.FromException`; the recording subclass overrides `YieldWithContinuationProbeAsync` without calling base, so no static `ApplicationIdleTimer` reads, no live COM, no live timer. F1-formatted `engineMs`/`waitMs` assertions use regex on shape, not exact timing. |
| **Readability & Maintainability** - Clear structure | ✅ PASS | Descriptive method names; explicit Arrange/Act/Assert comment markers; class-level XML doc summarizing the deterministic-seam strategy. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | ✅ PASS | **Baseline (pre-development):** 64.04% lines repo-wide aggregate (line-rate 0.6404305705059203; 104118/162575).<br>**Command:** `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`.<br>**Timestamp:** 2026-06-23 14:30.<br>**Source:** `evidence/baseline/baseline-tests-coverage-2026-06-23T14-30.md`. |
| **No Coverage Regression** | ✅ PASS | **Post-change coverage:** 64.05% lines repo-wide aggregate (line-rate 0.6405301074475671; 104204/162684).<br>**Change:** +0.01 pp lines (covered +86; valid +109 from 2 new files).<br>**Status:** No regression (slight increase). Source: `evidence/qa-gates/final-qc-coverage-delta-2026-06-23T14-30.md`. |
| **New Code Coverage >=90%** | ✅ PASS | **New file:** `TaskMaster/AppGlobals/EngineInitTimingProbe.cs`.<br>**New code coverage:** 100% (`TaskMaster.EngineInitTimingProbe` class line-rate=1; `<TimeEngineAsync>d__2` async state machine line-rate=1).<br>**Calculation method:** Reviewer-regenerated `artifacts/csharp/coverage.xml` (Cobertura), class node at line 6400. Exceeds 90% floor. |
| **Comprehensive Coverage** | ✅ PASS | `EngineInitTimingProbe`: constructor guard, `TimeEngineAsync` (success ordered, null engine, throwing factory, null args), `EmitConfigTiming`. `YieldWithContinuationProbeAsync` ordering/count verified via the recording subclass. |
| **Positive Flows** - Valid inputs | ✅ PASS | `TimeEngineAsync_ThreeEnginesInOrder_...`, `EmitConfigTiming_Always_...`, `LoadSequentialAsync_InvokesProbeForEachBoundaryInStartupOrder`. |
| **Negative Flows** - Invalid inputs | ✅ PASS | `TimeEngineAsync_NullArguments_ThrowArgumentNullException`, `Constructor_NullSink_ThrowsArgumentNullException`. |
| **Edge Cases** - Boundary conditions | ✅ PASS | `TimeEngineAsync_NullFactoryResult_EmitsEngineNullTrueAndSkipAndReturnsNull` (null engine -> costHint=Skip). |
| **Error Handling** - Error paths | ✅ PASS | `TimeEngineAsync_FactoryThrows_PropagatesAndEmitsNoLine` (fail-fast propagation, no emission on failure). |
| **Concurrency** - If applicable | ✅ PASS | `[DoNotParallelize]` on the global-seam tests documents the only concurrency constraint; the probe seam itself is stateless per call. |
| **State Transitions** - If applicable | N/A | The instrumentation is stateless; no state machine under test beyond the compiler-generated async state machine (covered). |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 64.04% lines -> Post-change: 64.05% lines. Change: +0.01% lines. New/changed-code coverage: 100%. Disposition: BLOCKED. Evidence: `evidence/baseline/baseline-tests-coverage-2026-06-23T14-30.md`, `evidence/qa-gates/final-qc-coverage-delta-2026-06-23T14-30.md`, `artifacts/csharp/coverage.xml`. New-code coverage PASS (100% >= 90%) and no-regression PASS (+0.01 pp); the PARTIAL is solely because the repo-wide aggregate (64.05%) is below the 80% floor before applying the documented COM/VSTO/WinForms testable-denominator exemptions, and the authoritative repo-wide-vs-floor determination is the PR CI run (not available locally).
- TypeScript: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (zero TypeScript files in branch diff).
- PowerShell: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (zero PowerShell files in branch diff).

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | ✅ PASS | FluentAssertions assertions (`Should().Equal(...)`, `Should().Contain(...)`, `Should().MatchRegex(...)`) produce descriptive failures; ordering assertion `recorded.Should().Equal("IntelConfig","OlObjects","ToDo","AutoFile","Engines")` names the expected sequence. |
| **Arrange-Act-Assert Pattern** | ✅ PASS | Every test method has explicit `// Arrange` / `// Act` / `// Assert` markers. |
| **Document Intent** | ✅ PASS | Method names encode scenario and expectation; class XML docs describe the deterministic-seam design. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | ✅ PASS | No network, DB, filesystem, or live process. Outlook `Application` is a Moq stub; engines are Moq stubs; `LoadBasicMethod` is overridden via reflection to set a fixed elapsed without live collaborators. |
| **Use Mocks/Stubs** | ✅ PASS | Moq for `IConditionalEngine<MailItemHelper>` and `OutlookApplication`; injected `Action<string>` list sink replaces the live log4net appender. |
| **Environment Stability** | ✅ PASS | No temporary files. The recording subclass avoids the static `ApplicationIdleTimer` reads in CI by overriding the probe without calling base. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | ✅ PASS | This audit plus `code-review.2026-06-23T15-31.md` and `feature-audit.2026-06-23T15-31.md` constitute the required review. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | ✅ PASS | Objective stated in `spec.md` and `issue.md` (#211): localize/attribute the multi-minute startup latency; Phase 1 + Phase 3 are diagnostic increments. |
| **Read existing change plans** | ✅ PASS | `plan.2026-06-23T14-30.md` (Phase 3 plan) and prior plans present; Phase 0 baseline evidence recorded. |
| **Document the plan** | ✅ PASS | Atomic plan and baseline evidence under the feature folder. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | ✅ PASS | Probe wraps existing awaits with a `Stopwatch` and a single emitted line; no new control flow. |
| **Reusability** | ✅ PASS | `EngineInitTimingProbe` is a small reusable seam used for both per-engine and config timing through one injected sink. |
| **Extensibility** | ✅ PASS | The injected `Action<string>` sink and the `protected internal virtual` probe method are clean extension/override seams. |
| **Separation of concerns** | ✅ PASS | Timing/emission logic is extracted from the COM-bound `AppItemEngines` into the testable `EngineInitTimingProbe`; the COM factory call stays in `AppItemEngines`. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | ✅ PASS | New file holds one cohesive probe type. |
| **Under 500 lines** | ✅ PASS | All touched files <= 500: EngineInitTimingProbe.cs 97, AppItemEngines.cs 279, ApplicationGlobals.cs 263, EngineInitTimingProbeTests.cs 142, ContinuationProbeSequenceTests.cs 107, ApplicationGlobalsStartupTimingTests.cs 301, ApplicationGlobalsTests.cs 485 (verified via `awk END{print NR}`). |
| **Public vs internal** | ✅ PASS | Probe method kept `protected internal virtual` (override seam); `EngineInitTimingProbe` is `public sealed` (consumed by the test project, which has no InternalsVisibleTo to the same effect for this seam). |
| **No circular dependencies** | ✅ PASS | New type depends only on existing `UtilitiesCS` types already referenced by `TaskMaster`. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | ✅ PASS | `YieldWithContinuationProbeAsync`, `TimeEngineAsync`, `EmitConfigTiming`, `EngineInitTimingProbe`. |
| **Docs/docstrings** | ✅ PASS | XML docs on the type, constructor, and both methods; rationale comments at both instrumentation sites. |
| **Comment why, not what** | ✅ PASS | Comments explain the diagnosis-only/behavior-preserving rationale and the `[ExcludeFromCodeCoverage]` boundary, not mechanics. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ✅ PASS | **Command:** `dotnet tool run csharpier check .`<br>**Result:** Checked 1091 files; 0 unformatted (reviewer run, exit 0). |
| **2. Linting** | ✅ PASS | **Command:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`<br>**Result:** Build succeeded, 0 analyzer errors/warnings reported at minimal verbosity (reviewer run, exit 0). |
| **3. Type checking** | ✅ PASS | **Command:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`<br>**Result:** Build succeeded, 0 nullable/warning-as-error failures (reviewer run, exit 0). |
| **4. Testing** | ✅ PASS | **Command:** `vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"...AppGlobals.EngineInitTimingProbeTests|...ContinuationProbeSequenceTests|...ApplicationGlobalsTests|...ApplicationGlobalsStartupTimingTests"`<br>**Result:** 20/20 passed (reviewer run, exit 0). Executor full non-live run: 4318/4318 passed. |
| **Full toolchain loop** | ✅ PASS | Reviewer completed format -> lint -> type-check -> test in a single clean pass with no auto-fixes. |
| **Explicit reporting** | ✅ PASS | Commands and results documented in this section and Appendix B. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | ✅ PASS | Commit messages `72520363`, `e3a84b5d`, `e2da1226` and the spec scope-expansion section. |
| **Design choices explained** | ✅ PASS | Seam-extraction and `[ExcludeFromCodeCoverage]` boundary documented in code and coverage-delta evidence. |
| **Update supporting documents** | ✅ PASS | `spec.md`, `issue.md` scope-expansion sections, plans, and evidence updated. |
| **Provide next steps** | ✅ PASS | AC9 (maintainer non-debugger re-capture) and AC10 (Phase 4 evidence-gated fix) documented as pending next steps. |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3C-Sharp: C# Code Change Policy Compliance

#### C#.1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with CSharpier** | ✅ PASS | `dotnet tool run csharpier check .` -> 0 unformatted (reviewer run). |
| **Linting with .NET analyzers** | ✅ PASS | Analyzer build succeeded with `EnableNETAnalyzers`/`EnforceCodeStyleInBuild` (reviewer run). |
| **Type checking with nullable/TWAE** | ✅ PASS | Nullable + TreatWarningsAsErrors build succeeded (reviewer run). |
| **Testing with MSTest** | ✅ PASS | MSTest via vstest; 20/20 reviewer, 4318/4318 executor. |

#### C#.2 Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Strong contracts / explicit APIs** | ✅ PASS | Explicit nullable return `Task<IConditionalEngine<MailItemHelper>?>`; guard clauses with `ArgumentNullException`. |
| **Null-safety** | ✅ PASS | Nullable annotations on returns/params; `?? throw` sink guard; `SynchronizationContext.Current?...?? "null"`. |
| **Composition / focused types** | ✅ PASS | Composition via injected delegate; sealed single-responsibility probe. |
| **Async/resource safety** | ✅ PASS | `async`/`await` with `Stopwatch.StartNew()/Stop()`; awaited factory exactly once. |
| **Banned APIs** | ✅ PASS | No `DateTime.Now`/`UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay` in source (only in explanatory comments/XML docs). Verified by `git grep` over the three changed prod files. |
| **net48 / no positional record struct** | ✅ PASS | No positional `record struct`; `public sealed class`. |

#### C#.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Files under 500 lines** | ✅ PASS | See 2.3. |
| **Intentional public surface** | ✅ PASS | One new public type plus the existing override seam. |
| **Explicit usings** | ✅ PASS | File-scoped `using` directives; AppItemEngines uses fully-qualified `System.Diagnostics`/`System.Threading` at the two instrumentation sites. |

---

## 4. Language-Specific Unit Test Policy Compliance

### Section 4C-Sharp: C# Unit Test Policy Compliance

| Requirement | Status | Evidence |
|------------|--------|----------|
| **MSTest framework** | ✅ PASS | `[TestClass]`/`[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. |
| **Moq for mocking** | ✅ PASS | `Mock<IConditionalEngine<MailItemHelper>>`, `Mock<OutlookApplication>`. |
| **FluentAssertions** | ✅ PASS | `Should()...` throughout; `ThrowAsync`/`MatchRegex`. |
| **Coverage expectation** | ✅ PASS (new) / PARTIAL (repo-wide) | New seam 100% >= 90%; repo-wide aggregate 64.05% below 80% floor before exemptions (see 1.2.1). |
| **No external dependencies** | ✅ PASS | See 1.4. |
| **Deterministic in IDE and CLI** | ✅ PASS | Shape-based assertions; no clock/network/PATH dependence. |

---

## 5. Test Coverage Detail

### TaskMaster.EngineInitTimingProbe (6 tests)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| TimeEngineAsync_ThreeEnginesInOrder_EmitsOneLinePerEngineInOrderWithFields | Positive | TimeEngineAsync body, emit | ✅ |
| TimeEngineAsync_NullFactoryResult_EmitsEngineNullTrueAndSkipAndReturnsNull | Edge Case | null-engine branch, costHint=Skip | ✅ |
| EmitConfigTiming_Always_EmitsOneConfigLineWithFields | Positive | EmitConfigTiming | ✅ |
| TimeEngineAsync_FactoryThrows_PropagatesAndEmitsNoLine | Error Handling | exception path (no emit) | ✅ |
| TimeEngineAsync_NullArguments_ThrowArgumentNullException | Negative | engineName/factory guards | ✅ |
| Constructor_NullSink_ThrowsArgumentNullException | Negative | constructor guard | ✅ |

**Coverage:** 100% of `EngineInitTimingProbe` (class line-rate=1; async state machine line-rate=1).

**Not covered:** None.

### ApplicationGlobals.YieldWithContinuationProbeAsync ordering (4 tests across 2 files)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| LoadSequentialAsync_InvokesProbeForEachBoundaryInStartupOrder | Positive | probe ordering | ✅ |
| LoadSequentialAsync_InvokesProbeExactlyOncePerBoundary | Positive | probe count (5) | ✅ |
| ApplicationGlobalsTests yield-shape regex (updated) | Positive | source-shape assertion | ✅ |
| ApplicationGlobalsStartupTimingTests YieldCount override (updated) | Positive | yield count | ✅ |

**Not covered:** The static `ApplicationIdleTimer` reads inside the production `YieldWithContinuationProbeAsync` body are intentionally not exercised under unit test (they require a live Outlook process); the recording subclass overrides without calling base. This is consistent with the COM-host-bound exemption.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests (reviewer targeted) | 20 | ✅ |
| Tests Passed | 20 (100%) | ✅ |
| Tests Failed | 0 | ✅ |
| Execution Time | 2.96 s total (reviewer) | ✅ Fast |
| Average Time per Test | ~148 ms (skewed by one 488 ms test) | ✅ Fast |
| Functions/Classes Tested (new seam) | EngineInitTimingProbe 1/1 | ✅ |
| Test File Size | <= 485 lines | ✅ Maintainable |
| Code Coverage (new seam) | 100% lines | ✅ |
| Code Coverage (repo-wide aggregate) | 64.05% lines | ⚠️ below 80% floor before exemptions |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `dotnet tool run csharpier check .` | 1091 files checked, 0 unformatted | ✅ |
| .NET Analyzers | `msbuild TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Build succeeded, 0 errors/warnings | ✅ |
| Nullable / TWAE | `msbuild TaskMaster.sln /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | Build succeeded, 0 errors | ✅ |
| MSTest Tests | `vstest.console.exe TaskMaster.Test...dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"..."` | 20/20 passed | ✅ |

**Notes:**
The pre-existing `UtilitiesCS TimedAsyncTask_Tests.RequestTask_WithProvidedTask_InvokesTaskAfterInterval` real-interval timer flake is documented by the executor as a known pre-existing flake (it did not surface in the executor 4318-test run). The reviewer ran a targeted AppGlobals subset and did not exercise that test. This flake is unrelated to the #211 change.

---

## 8. Gaps and Exceptions

### Identified Gaps
- Repo-wide coverage floor (80%): the deterministic full-suite aggregate is 64.05%, below the 80% raw floor. This is a pre-existing repository state (baseline 64.04%), not a regression from #211, and the 80% floor is defined against the testable denominator after COM/VSTO/WinForms exemptions. The authoritative repo-wide-vs-floor determination is the PR CI run, which is not available in this local environment.
- AC9 and AC10 are not delivered (AC9 is a maintainer-run non-debugger runtime capture; AC10 is the evidence-gated Phase 4 fix). These are documented as intentionally pending, not failed delivery of in-scope automatable work.

### Approved Exceptions
- COM/VSTO/WinForms testable-denominator coverage exemption (CLAUDE.md, ratified by maintainer): `AppItemEngines` is `[ExcludeFromCodeCoverage]` (COM-bound). The coverable timing/emission logic was deliberately extracted into `EngineInitTimingProbe`, which is NOT exempt and reaches 100%.

### Removed/Skipped Tests
- **None.** No tests were removed; the two modified test files were updated only to track the `YieldBetweenStartupPhasesAsync` -> `YieldWithContinuationProbeAsync(priorPhaseName)` rename.

---

## 9. Summary of Changes

### Commits in This PR/Branch
1. **cf766cab** - docs(intelconfig-stall): seed #211 IntelConfig continuation-stall diagnosis
2. **623c83b8** - chore(#211): scaffold active feature folder
3. **380e76a8** - docs(#211): spec, Phase-1 attribution-probe plan, baseline evidence
4. **72520363** - feat(#211): continuation-latency attribution probe for IntelConfig stall
5. **e2da1226** - docs(#211): reopen and expand scope to Engines-phase startup latency
6. **e3a84b5d** - feat(#211): Engines-phase per-engine attribution instrumentation (Phase 3)
7. **3812fa6b** - (docs): audit

### Files Modified
1. **TaskMaster/AppGlobals/EngineInitTimingProbe.cs** (NEW) - testable per-engine/config timing seam with injected sink; 100% covered.
2. **TaskMaster/AppGlobals/AppItemEngines.cs** (MODIFIED) - `InitAsync` wraps the `Configuration` await and per-engine factory with the probe; `[ExcludeFromCodeCoverage]`, behavior-preserving.
3. **TaskMaster/AppGlobals/ApplicationGlobals.cs** (MODIFIED) - replaced 5 `YieldBetweenStartupPhasesAsync()` call sites with `YieldWithContinuationProbeAsync(priorPhase)`; renamed/expanded the probe method.
4. **TaskMaster/TaskMaster.csproj** / **TaskMaster.Test/TaskMaster.Test.csproj** (MODIFIED) - explicit `<Compile Include>` wiring for the new files.
5. **TaskMaster.Test/AppGlobals/EngineInitTimingProbeTests.cs**, **ContinuationProbeSequenceTests.cs** (NEW) - deterministic seam tests.
6. **TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs**, **ApplicationGlobalsStartupTimingTests.cs** (MODIFIED) - updated to the renamed probe.
7. 32 docs/evidence markdown files under the feature folder (scoping, plans, evidence, prior review artifacts).

---

## 10. Compliance Verdict

### Overall Status: ⚠️ PARTIALLY COMPLIANT

The C# code-change and unit-test policies are met for the delivered automatable scope: clean four-step toolchain (CSharpier, analyzers, nullable/TWAE, MSTest), 100% coverage on the new testable seam, no banned APIs, all files <= 500 lines, behavior-preserving diagnostic-only changes, and deterministic tests. The single PARTIAL is the repo-wide coverage aggregate (64.05% vs the 80% raw floor), which is a pre-existing repository state with no regression from #211 and is governed by the documented testable-denominator exemption plus the PR CI run as the authoritative gate.

**Fail-closed reminder:** New-code coverage and no-regression are both supported by numeric artifacts. The repo-wide-vs-floor PARTIAL is recorded rather than asserted PASS because the authoritative repo-wide determination (post-exemption) is the PR CI run, unavailable locally.

---

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- ✅ Before Making Changes: plan + baseline present
- ✅ Design Principles: simple, reusable seam, separation of concerns
- ✅ Module & File Structure: all <= 500 lines
- ✅ Naming, Docs, Comments: descriptive + rationale comments
- ✅ Toolchain Execution: clean single pass (reviewer-run)
- ✅ Summarize & Document: spec/issue/plan/evidence updated

#### Language-Specific Code Change Policy (Section 3)
**For C#:**
- ✅ Tooling & Baseline: CSharpier/analyzers/nullable all pass
- ✅ Design & Type-Safety: nullable, guards, no banned APIs
- ✅ Structure & Naming: focused, descriptive, <= 500 lines

#### General Unit Test Policy (Section 1)
- ✅ Core Principles
- ⚠️ Coverage & Scenarios: new-code 100%, repo-wide aggregate below raw floor (pre-existing)
- ✅ Test Structure
- ✅ External Dependencies
- ✅ Policy Audit

#### Language-Specific Unit Test Policy (Section 4)
**For C#:**
- ✅ Framework & Scope (MSTest/Moq/FluentAssertions)
- ✅ Test Style & Structure
- ✅ Naming & Readability
- ✅ Toolchain

---

### Metrics Summary
- ✅ 20/20 reviewer-targeted tests passing (100%); 4318/4318 executor non-live suite
- ✅ New seam `EngineInitTimingProbe` 1/1 class tested, 100% line coverage
- ⚠️ 64.05% repo-wide line coverage aggregate (pre-existing; no regression)
- ✅ All touched files <= 500 lines
- ✅ All C# code-quality checks passing (reviewer-run)
- ✅ Reviewer test execution time: 2.96 s (fast)

---

### Recommendation

**Ready for merge (conditional).** The delivered Phase 1 + Phase 3 diagnostic instrumentation meets all automatable C# policy gates with 100% new-code coverage and no repo-wide regression. Two conditions are not blocking the instrumentation itself but bear on issue closure: (1) the repo-wide-vs-80% determination must be confirmed by the PR CI run against the post-exemption testable denominator; (2) issue #211's stated objective (eliminate the multi-minute startup latency) remains UNMET because AC9 (maintainer non-debugger re-capture) and AC10 (evidence-gated Phase 4 fix) are not yet delivered. Merge the instrumentation; do not close #211 on this branch.

---

## Appendix A: Test Inventory

- TaskMaster.Test.AppGlobals.EngineInitTimingProbeTests › TimeEngineAsync_ThreeEnginesInOrder_EmitsOneLinePerEngineInOrderWithFields
- TaskMaster.Test.AppGlobals.EngineInitTimingProbeTests › TimeEngineAsync_NullFactoryResult_EmitsEngineNullTrueAndSkipAndReturnsNull
- TaskMaster.Test.AppGlobals.EngineInitTimingProbeTests › EmitConfigTiming_Always_EmitsOneConfigLineWithFields
- TaskMaster.Test.AppGlobals.EngineInitTimingProbeTests › TimeEngineAsync_FactoryThrows_PropagatesAndEmitsNoLine
- TaskMaster.Test.AppGlobals.EngineInitTimingProbeTests › TimeEngineAsync_NullArguments_ThrowArgumentNullException
- TaskMaster.Test.AppGlobals.EngineInitTimingProbeTests › Constructor_NullSink_ThrowsArgumentNullException
- TaskMaster.Test.AppGlobals.ContinuationProbeSequenceTests › LoadSequentialAsync_InvokesProbeForEachBoundaryInStartupOrder
- TaskMaster.Test.AppGlobals.ContinuationProbeSequenceTests › LoadSequentialAsync_InvokesProbeExactlyOncePerBoundary
- (plus 12 ApplicationGlobalsTests / ApplicationGlobalsStartupTimingTests methods exercised in the reviewer run)

---

## Appendix B: Toolchain Commands Reference

**For C#:**
```bash
# Formatting (check-only)
dotnet tool run csharpier check .

# Linting (analyzers)
msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true

# Type checking (nullable + TreatWarningsAsErrors)
msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true

# Testing + coverage (targeted AppGlobals subset, reviewer)
vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"FullyQualifiedName~AppGlobals.EngineInitTimingProbeTests|FullyQualifiedName~AppGlobals.ContinuationProbeSequenceTests|FullyQualifiedName~AppGlobals.ApplicationGlobalsTests|FullyQualifiedName~AppGlobals.ApplicationGlobalsStartupTimingTests"

# Coverage conversion to canonical Cobertura artifact
dotnet-coverage merge <run>.coverage -f cobertura -o artifacts/csharp/coverage.xml
```

---

**Audit Completed By:** feature-reviewer agent
**Audit Date:** 2026-06-23
**Policy Version:** Current (as of audit date)
