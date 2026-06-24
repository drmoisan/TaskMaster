# Policy Compliance Audit: Issue #211 outlook-startup-latency diagnostics + AC10 junk-navigation fix

**Audit Date:** 2026-06-24
**Code Under Test:** Full branch diff `bug/outlook-startup-latency-211` vs base `main` (merge-base `9385bf607aca6c5722f2da7961a895c685710942`). Changed files: 29 `.cs` (17 production, 12 test), 4 `.csproj`, plus docs/evidence (`.md`, coverage `.xml`).

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 17 prod + 12 test | 4109 | ✅ 4109 pass, 0 fail | 61.84% lines (repo-wide) | 61.90% lines (repo-wide) | New helper seams 95–100% (see §1.2.1) |

**Note:** No TypeScript, Python, PowerShell, Bash, or JSON files changed in the branch diff. Those languages have zero changed files and are correctly N/A.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope` (zero TypeScript files changed)
- TypeScript post-change coverage artifact: `N/A - out of scope` (zero TypeScript files changed)
- PowerShell baseline coverage artifact: `N/A - out of scope` (zero PowerShell files changed)
- PowerShell post-change coverage artifact: `N/A - out of scope` (zero PowerShell files changed)
- C# baseline coverage artifact: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/baseline/baseline-coverage-2026-06-24T17-30.cobertura.xml`
- C# post-change coverage artifact: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/qa-gates/postchange-coverage-2026-06-24T17-30.cobertura.xml`
- Per-language comparison summary: §1.2.1 below

**Non-negotiable verdict rule:** This audit reports numeric baseline and post-change coverage for the only in-scope language (C#) plus per-file new-code coverage.

**Fail-closed rule applied:** Repo-wide C# coverage (61.90%) is below the 80% line gate. This is recorded as FAIL against the literal gate (see §1.2 and §8); the overall verdict is therefore not PASS.

---

## Executive Summary

This branch delivers (1) a sequence of behavior-preserving startup-latency diagnostic probes for issue #211 (`[continuation-resume]`, `[engine-init]`, `[ui-heartbeat]`, `[gc-delta]`, `[startup-lifetime-heartbeat]`, `[store-filter]`, `[spam-init]`, `[store-wrapper-init]`, `[phase-net]`) and (2) one behavior-changing fix (AC10, commit `6d6209f0`): `LoadJunkCertain`/`LoadJunkPotential` now resolve the configured junk-folder path via direct navigation (`JunkFolderPathNavigator` over an `IFolderNode` abstraction) instead of `new FolderTree(Root)`, which eagerly enumerated the entire default-store folder hierarchy on the STA.

The diagnostic work follows a consistent, policy-aligned pattern: COM/UI-host-bound concerns stay in `[ExcludeFromCodeCoverage]` call sites; pure measurement/formatting/decision logic is extracted into small coverable helper types (`EngineInitTimingProbe`, `StartupDiagnosticsProbe`, `SpamInitTimingProbe`, `StoreFilterAttribution`, `StoreWrapperInitClock`, `StoreWrapperInitProbe`, `JunkFolderPathNavigator`), each covered by deterministic MSTest + Moq + FluentAssertions. All four C# toolchain steps were run and passed in this review (see Appendix B).

**Policy documents evaluated:**
- [✅] `CLAUDE.md` (all sections; embedded General Code Change, General Unit Test, C# Code Change, C# Unit Test policies)
- [✅] `.claude/rules/general-code-change.md`
- [✅] `.claude/rules/general-unit-test.md`
- [✅] `.claude/rules/csharp.md`
- [✅] `.claude/rules/ci-workflows.md` (no workflow YAML changed; not triggered)

**Language-specific policies evaluated:**
- [N/A] Python — zero Python files changed
- [N/A] PowerShell — zero PowerShell files changed
- [N/A] Bash — zero Bash files changed
- [N/A] JSON — zero governed JSON files changed
- [✅] C# Code Change Policy + C# Unit Test Policy — applicable; evaluated in §3 and §4

**Temporary artifacts cleanup:**
- [✅] The reviewer created two throwaway `cmd` wrappers under `/tmp` only (to pass `Platform="Any CPU"` quoting to msbuild); they are outside the repo and are not committed.
- No throwaway scripts were added to the repo by this branch.

---

## Rejected Scope Narrowing

The caller prompt included this context paragraph:

> "The runtime acceptance items (AC5, AC9, and the AC10 cold-start re-capture, and the per-increment runtime captures) are maintainer-run and not CI-automatable; recorded captures and placeholders are under `evidence/other/`. Several diagnostic increments were committed without per-increment review by orchestrator decision; review them now as part of the branch."

Assessment: this is NOT an attempted scope narrowing. It explicitly directs review of all increments as part of the branch and merely states that certain runtime ACs are maintainer-gated (a factual property of a VSTO add-in that requires a live Outlook host). The audit scope used here is the full branch diff `main..HEAD`, covering every changed file and every diagnostic increment.

No instruction in the caller prompt attempted to (a) narrow scope to a plan/task/phase subset, (b) limit to a subset of changed files, (c) mark any language with changed files as "informational only"/"out of scope", or (d) skip a toolchain or coverage check for a changed language. No verbatim narrowing text needs to be recorded because none was supplied.

Separately, the PR-context summary artifact (`artifacts/pr_context.summary.txt`) misclassified the change as "Core logic changes: 0 files / Docs/templates/agents/tooling: 171 files." This is a known misclassification of substantive C# production changes as docs. The reviewer verified actual scope from `git diff` (17 production `.cs` files, including `ApplicationGlobals.cs` +253 lines, the SpamBayes partial split, `StoresWrapper.cs`, and three new probe types) and audited against the real diff, not the summary's classification.

---

## Evidence Location Compliance

The reviewer scanned the branch diff for files written under the forbidden evidence paths (`artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/`, `artifacts/post-change/`).

- Command: `git diff --name-only <merge-base>..HEAD | grep -E '^artifacts/(baselines?|qa|qa-gates|evidence|coverage|regression-testing|post-change)/'`
- Result: `NONE`. No evidence files were written to forbidden `artifacts/` subpaths.
- All evidence artifacts (baselines, QA gates, regression results, coverage `.xml`) are under the canonical `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/<kind>/` scheme.
- The repo's `validate_evidence_locations.py` script was not found on disk at the documented skill path; the manual diff scan above substitutes for it and reports no violations.

No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` events occurred (the reviewer wrote only the three audit artifacts to the feature root, per workflow contract).

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | ✅ PASS | New tests use list-capturing sinks and in-memory fakes; `StoreWrapperInitClockTests` call `StoreWrapperInitClock.Reset()` for isolation of the process-global accumulator. 4109/4109 passed in a single shared run. |
| **Isolation** - Each test targets single behavior | ✅ PASS | Each probe/navigator helper has its own test class targeting one method/behavior (e.g., `JunkFolderPathNavigatorTests`, `StoreFilterAttributionTests`, `PhaseNetProbeTests`). |
| **Fast Execution** | ✅ PASS | Full run: 4109 tests in 17.18 s; new helper tests report `< 1 ms`–`7 ms` each. |
| **Determinism** | ✅ PASS | No live COM/timer/clock; `Stopwatch`/GC/Dispatcher stay in call sites and tests inject numeric values. `TestCategory!=LiveOutlook` excludes host-bound tests. |
| **Readability & Maintainability** | ✅ PASS | Descriptive method names (`Add_ConcurrentCalls_AccumulatesWithoutLostUpdates`, `ResolvePath_AccessesOnlyFoldersAlongThePath_NotEntireTree`), XML doc comments on test classes. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | ✅ PASS | Baseline repo-wide C#: 61.84% (98,261/158,895 lines). Command: `vstest.console.exe ... /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"`. Artifact: `evidence/baseline/baseline-coverage-2026-06-24T17-30.cobertura.xml`. Timestamp: 2026-06-24T17-30. |
| **No Coverage Regression** | ✅ PASS | Post-change repo-wide C#: 61.90% (98,484/159,114 lines). Change: +0.06pp. No regression. Artifact: `evidence/qa-gates/postchange-coverage-2026-06-24T17-30.cobertura.xml`. |
| **New Code Coverage ≥90%** | ✅ PASS | All new coverable helper files ≥ 95% (see §1.2.1). New-code aggregate reported by the plan: 94.92%. |
| **Repo-wide Coverage ≥80%** | ❌ FAIL | Repo-wide C# line coverage is 61.90% (< 80% gate). This is a pre-existing repo-wide deficit (baseline already 61.84%), NOT introduced or regressed by this branch. The unexempted aggregate includes large COM/VSTO/WinForms assemblies. Recorded as a FAIL against the literal gate and added to remediation triggers; see §8 and the remediation-inputs artifact. |
| **Comprehensive Coverage** | ✅ PASS | New helpers cover positive, negative (null-arg guards), edge (clamp at 0, empty/`null` path segments), and concurrency (`Add_ConcurrentCalls...`). |
| **Positive Flows** | ✅ PASS | e.g., `ResolvePath` resolves valid multi-segment paths; `Decide` returns `(true, Included)` when no rule matches. |
| **Negative Flows** | ✅ PASS | `ResolvePath` returns null for null root/path and unmatched segments; probe ctors throw `ArgumentNullException` on null sink. |
| **Edge Cases** | ✅ PASS | `ComputeNetMs` clamp (`grossMs < storeWrapperInitMs`); BFS-from-root-itself first-segment match; verbatim `'\\'` split. |
| **Error Handling** | ✅ PASS | `TimeEngineAsync` propagates factory exceptions (fail-fast) without emitting a line; verified by tests. |
| **Concurrency** | ✅ PASS | `Add_ConcurrentCalls_AccumulatesWithoutLostUpdates` exercises `Interlocked` accumulation. |
| **State Transitions** | ✅ PASS | `StartupLifetimeStopDecider.ShouldStop` consecutive-responsive-tick counter reset/advance covered. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 61.84% lines (repo-wide) -> Post-change: 61.90% lines (repo-wide). Change: +0.06% lines. New/changed-code coverage: 94.92% (per-file new helpers 95–100%; per-file detail below). Disposition: FAIL (repo-wide line coverage 61.90% < 80% gate; no regression, pre-existing deficit). Evidence: `evidence/baseline/baseline-coverage-2026-06-24T17-30.cobertura.xml`, `evidence/qa-gates/postchange-coverage-2026-06-24T17-30.cobertura.xml`.
- TypeScript: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A (zero TypeScript files in branch diff). Evidence: N/A - out of scope.
- PowerShell: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A (zero PowerShell files in branch diff). Evidence: N/A - out of scope.

Per-file new/changed C# coverage (post-change cobertura, line hits/total):
- `JunkFolderPathNavigator.cs` (NEW): 95.00% (57/60) — PASS (≥90%)
- `StartupDiagnosticsProbe.cs` (NEW): 100% (104/104) — PASS
- `EngineInitTimingProbe.cs` (NEW): 100% (30/30) — PASS
- `SpamInitTimingProbe.cs` (NEW): 100% (18/18) — PASS
- `StoreFilterAttribution.cs` (NEW): 100% (48/48) — PASS
- `StoreWrapperInitClock.cs` (NEW): 100% (12/12) — PASS
- `StoreWrapperInitProbe.cs` (NEW): 100% (17/17) — PASS
- `StoresWrapper.cs` (MODIFIED): 97.67% (210/215) — PASS (≥80%, improved)
- `StoreWrapper.cs` (MODIFIED): 94.87% (111/117) — PASS
- `SpamBayes.Conditions.cs` (NEW partial): 94.12% (64/68) — PASS
- `SpamBayes.Actions.cs` (NEW partial): 93.22% (55/59) — PASS
- `SpamBayes.cs` (MODIFIED): 82.84% (169/204) — PASS (≥80%)
- `SpamBayes.Classify.cs` (NEW partial): 66.29% (59/89) — see note
- `ApplicationGlobals.cs` (MODIFIED): 67.93% (161/237) — see note
- `AppOlObjects.JunkFolders.cs` (MODIFIED): 30.88% (21/68) — see note
- `AppItemEngines.cs` (MODIFIED): `[ExcludeFromCodeCoverage]` (COM-bound; exempt at baseline)

Note on the three sub-90% modified/partial files: the uncovered lines in `ApplicationGlobals.cs`, `AppOlObjects.JunkFolders.cs`, and `SpamBayes.Classify.cs` are COM/UI-host-bound (live `MAPIFolder` reads, `MyBox`/`PickFolder` dialogs, Dispatcher/timer scheduling, model deserialize) covered by the CLAUDE.md COM/VSTO/WinForms exemption (testable denominator). The behavior-changing AC10 logic was extracted into the fully-covered `JunkFolderPathNavigator` (95%) and the COM adapter `OutlookFolderNode` is correctly `[ExcludeFromCodeCoverage]`. The new coverable seams all meet the ≥90% new-code floor.

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | ✅ PASS | FluentAssertions `because` clauses (e.g., the red-run assertion: "the navigator must touch only the resolution path plus the first-segment BFS frontier ... but found 785"). |
| **Arrange-Act-Assert Pattern** | ✅ PASS | New test classes follow AAA with explicit arrange/act/assert structure. |
| **Document Intent** | ✅ PASS | Test classes carry XML summaries stating the invariant under test. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | ✅ PASS | New tests use in-memory fakes and injected sinks; no DB/network/process. |
| **Use Mocks/Stubs** | ✅ PASS | Moq subclasses (`TestableApplicationGlobals`) and counting/in-memory `IFolderNode` fakes isolate COM. |
| **Environment Stability** | ✅ PASS | No temporary files created in tests; process-global `StoreWrapperInitClock` reset between tests. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | ✅ PASS | This document, plus `code-review.2026-06-24T15-35.md` and `feature-audit.2026-06-24T15-35.md`. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | ✅ PASS | `spec.md` Context + Scope Expansion (2026-06-23) state the goal: eliminate multi-minute startup latency; localize via instrumentation then minimal fix. |
| **Read existing change plans** | ✅ PASS | `plan.2026-06-24T17-30.md` present; AC10 references it. |
| **Document the plan** | ✅ PASS | Phased plan + per-phase ACs in `spec.md`. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | ✅ PASS | Each probe is a single small class with `Action<string>` sink; navigator is a static class with focused methods. |
| **Reusability** | ✅ PASS | Probes share an identical `Action<string>` sink pattern; `ComputeNetMs` reused by the emitter. |
| **Extensibility** | ✅ PASS | `IFolderNode` abstraction lets the navigator work over any folder source; probes are sink-agnostic. |
| **Separation of concerns** | ✅ PASS | Pure logic (format/decide/navigate) separated from COM/UI I/O; COM stays in `[ExcludeFromCodeCoverage]` adapters/call sites. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | ✅ PASS | Each new file holds one cohesive concept; SpamBayes split into Conditions/Actions/Classify partials by responsibility. |
| **Under 500 lines** | ✅ PASS | Max changed file is `ApplicationGlobals.cs` at 464 lines. `SpamBayes.cs` was 705 at baseline (over cap) and is now 446 (brought into compliance by the partial split). All 29 changed `.cs` files ≤ 500. |
| **Public vs internal** | ✅ PASS | `IFolderNode`/`JunkFolderPathNavigator`/`OutlookFolderNode` are `internal`/`private`; probe helpers are `public` (in UtilitiesCS, consumed cross-assembly) with minimal surface. |
| **No circular dependencies** | ✅ PASS | New helpers depend only inward (UtilitiesCS helpers consumed by TaskMaster; no back-reference). |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | ✅ PASS | `JunkFolderPathNavigator`, `StoreWrapperInitClock`, `EmitPhaseNet`, `ComputeNetMs`. |
| **Docs/docstrings** | ✅ PASS | XML doc comments on all new public types/members, including an explicit equivalence contract on `JunkFolderPathNavigator`. |
| **Comment why, not what** | ✅ PASS | Comments explain the #211 rationale (e.g., the `new FolderTree(Root)` ~50s stall and why direct navigation is equivalent). |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ✅ PASS | `dotnet tool run csharpier check .` -> exit 0, "Checked 1108 files". |
| **2. Linting** | ✅ PASS | `msbuild TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` -> Build succeeded, 0 Warning(s), 0 Error(s). |
| **3. Type checking** | ✅ PASS | `msbuild TaskMaster.sln /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` -> Build succeeded, 0 Warning(s), 0 Error(s). |
| **4. Testing** | ✅ PASS | `vstest.console.exe TaskMaster.Test.dll UtilitiesCS.Test.dll /TestCaseFilter:"TestCategory!=LiveOutlook"` -> 4109/4109 passed, 0 failed, 17.18 s. |
| **Full toolchain loop** | ✅ PASS | All four steps passed in a single pass during this review (no auto-fix needed; csharpier reported no changes). |
| **Explicit reporting** | ✅ PASS | Commands and results recorded here and in Appendix B. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | ✅ PASS | `spec.md` AC notes document each increment and the AC10 fix. |
| **Design choices explained** | ✅ PASS | Equivalence-contract XML doc and spec design summary. |
| **Update supporting documents** | ✅ PASS | `spec.md`/`issue.md` updated with scope expansion and per-AC status. |
| **Provide next steps** | ✅ PASS | Maintainer-gated runtime captures documented with instructions + placeholders under `evidence/other/`. |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3 (C#) — C# Code Change Policy

#### C# Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with CSharpier** | ✅ PASS | `dotnet tool run csharpier check .` exit 0. |
| **Linting — .NET Analyzers** | ✅ PASS | Analyzer build: 0 warnings, 0 errors. No new analyzer debt; no banned-symbol additions (BannedApiAnalyzers). |
| **Type Checking — nullable** | ✅ PASS | `/p:Nullable=enable /p:TreatWarningsAsErrors=true` build: 0 warnings, 0 errors. New helpers use nullable annotations (e.g., `Task<IConditionalEngine<MailItemHelper>?>`). |

#### C# Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Strong contracts / explicit APIs** | ✅ PASS | Explicit signatures on public probe methods; `IFolderNode` contract documented. |
| **Null-safety by default** | ✅ PASS | Guard clauses (`?? throw new ArgumentNullException`), nullable annotations, `is null` checks. |
| **Composition / focused types** | ✅ PASS | Probes composed via injected `Action<string>`; no inheritance added beyond test subclasses. |
| **Asynchrony / resource safety** | ✅ PASS | `TimeEngineAsync` awaits the factory exactly once; no resource leaks introduced. |
| **No banned APIs** | ✅ PASS | Diff scan of all added non-comment `.cs` lines found no `DateTime.Now`/`DateTime.UtcNow`/`Random.Shared`/`Thread.Sleep`/`Task.Delay`. Timing uses `Stopwatch`; accumulation uses `Interlocked`. |
| **net48 compatibility** | ✅ PASS | No positional `record struct`; standard async/await. Builds succeed under the net48 solution. |

#### C# Error Handling, Logging, Contracts

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Exceptions / fail-fast** | ✅ PASS | Null-sink/null-arg guards throw; `TimeEngineAsync` propagates factory exceptions. |
| **Logging** | ✅ PASS | Uses the existing `log4net` logger via injected sink (`s => logger.Debug(s)`); no ad-hoc console output. |
| **Contracts / invariants** | ✅ PASS | Equivalence contract enforced and documented for the navigator; clamp rule on `ComputeNetMs`. |

#### Behavior-preservation review (diagnostic increments)

- The store-filter `Decide` helper mirrors the baseline `ShouldIncludeStore` short-circuit order exactly (verified line-by-line against `main`). It adds a defensive `gwsoFilePathContains is not null` guard the baseline lacked; because `GwsoFilePathContains` has a non-null default initializer, this does not change the production result. The original `ShouldIncludeStore` is retained unchanged.
- All probe call sites are additive (extra `Stopwatch`/log lines); phase order, engine set, filter result, and deserialize behavior are unchanged.

---

## 4. Language-Specific Unit Test Policy Compliance

### Section 4 (C#) — C# Unit Test Policy

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Framework: MSTest** | ✅ PASS | `[TestClass]`/`[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`. |
| **Mocking: Moq** | ✅ PASS | `TestableApplicationGlobals` and Moq usage in new tests. |
| **Assertions: FluentAssertions** | ✅ PASS | `.Should()` assertions throughout the new test classes. |
| **Coverage expectation** | ✅/❌ | New code ≥90% (PASS). Repo-wide ≥80% (FAIL — 61.90%; pre-existing, no regression). |
| **Focused / AAA / no external deps** | ✅ PASS | See §1.1–§1.4. |
| **Test files under 500 lines** | ✅ PASS | Largest changed test file `ApplicationGlobalsTests.cs` at 429 lines (reduced from 500 by extracting `TestableApplicationGlobals` to its own file). |

---

## 5. Test Coverage Detail

### JunkFolderPathNavigator (correctness + enumeration-bound)

| Test Name | Scenario Type | Status |
|-----------|--------------|--------|
| ResolvePath_AccessesOnlyFoldersAlongThePath_NotEntireTree | Edge / performance invariant | ✅ |
| ResolvePath_* correctness (5) + edge (4) | Positive/Negative/Edge | ✅ |

**Coverage:** 95.00% (57/60) of `JunkFolderPathNavigator.cs`.
**Not covered:** 3 lines (defensive branches in BFS frontier expansion); the COM adapter `OutlookFolderNode` is `[ExcludeFromCodeCoverage]`.

**Fail-before evidence:** `evidence/regression-testing/red-run-enumeration-bound-2026-06-24T17-30.md` (RED: counter 785 vs budget 4) and `green-run-enumeration-bound-2026-06-24T17-30.md`. Honors the repository bugfix workflow (failing regression first).

### Probe helpers (EngineInitTimingProbe, StartupDiagnosticsProbe, SpamInitTimingProbe, StoreFilterAttribution, StoreWrapperInitClock, StoreWrapperInitProbe)

All at 100% line coverage with positive/negative/edge/concurrency scenarios (see §1.2.1).

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 4109 | ✅ |
| Tests Passed | 4109 (100%) | ✅ |
| Tests Failed | 0 | ✅ |
| Execution Time | 17.18 s total | ✅ Fast |
| Code Coverage (repo-wide C#) | 61.90% lines | ❌ < 80% gate (pre-existing) |
| New-code Coverage | 94.92% (helpers 95–100%) | ✅ |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `dotnet tool run csharpier check .` | Checked 1108 files; no changes | ✅ |
| .NET Analyzers | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Build succeeded, 0 warnings, 0 errors | ✅ |
| Nullable / TWAE | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` | Build succeeded, 0 warnings, 0 errors | ✅ |
| MSTest + Coverage | `vstest.console.exe TaskMaster.Test.dll UtilitiesCS.Test.dll /TestCaseFilter:"TestCategory!=LiveOutlook"` | 4109/4109 passed | ✅ |

**Notes:** The AC4 spec note recorded a pre-existing `TimedAsyncTask_Tests.RequestTask_WithProvidedTask_InvokesTaskAfterInterval` real-interval timer flake; in this review's run all 4109 tests passed. The reviewer parsed the full multi-assembly Cobertura post-change artifact for the repo-wide figure; the single-assembly `artifacts/csharp/coverage.xml` (line-rate 5.95%) is a partial aggregate and was NOT used for the repo-wide verdict.

---

## 8. Gaps and Exceptions

### Identified Gaps

- **Repo-wide C# line coverage (61.90%) is below the 80% gate.** This is a pre-existing repo-level deficit (baseline 61.84%) reflecting large COM/VSTO/WinForms assemblies; this branch does not regress it (+0.06pp). The literal General/C# Unit Test Policy gate (repo-wide ≥80%) is not met. Recorded as FAIL and added to remediation triggers. Note: CLAUDE.md provides a COM/VSTO/WinForms coverage exemption against a "testable denominator," but the measured 61.90% is the unexempted aggregate; an exemption-adjusted figure is not computed in the available artifacts.

### Approved Exceptions

- **COM/VSTO/WinForms `[ExcludeFromCodeCoverage]`** on `AppItemEngines` (already exempt at baseline) and the `OutlookFolderNode` COM adapter — consistent with the CLAUDE.md exemption (live-Outlook-bound, no injectable seam). The testable navigation/probe logic these feed is fully covered.

### Removed/Skipped Tests

- **None.** No tests were removed or skipped. `TestCategory!=LiveOutlook` is the standard host-bound exclusion, not a removal.

---

## 9. Summary of Changes

### Commits in This Branch (recent)
1. `2a3770dc` - docs(#211): cold-start capture attributes latency to SpamBayes (~67.5s)
2. `bceea738` - docs(#211): record AC9 non-debugger capture
3. `1c05dacb` - docs(#211): Phase 3 feature-review artifacts
4. `e3a84b5d` - feat(#211): Engines-phase per-engine attribution instrumentation
5. `6d6209f0` - AC10 behavior-changing fix: JunkFolderPathNavigator direct path navigation (branch head)

### Files Modified (production highlights)
1. `TaskMaster/AppGlobals/ApplicationGlobals.cs` (MODIFIED, 464 lines) — per-phase probe wiring, `SampleStoreWrapperInitTotalMs` seam, `[phase-net]` emit.
2. `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` (MODIFIED) — AC10 fix: direct navigation via `JunkFolderPathNavigator` + `OutlookFolderNode` COM adapter; not-found fallback preserved verbatim.
3. `TaskMaster/AppGlobals/JunkFolderPathNavigator.cs` (NEW) — pure COM-free path navigator + `IFolderNode`.
4. `TaskMaster/AppGlobals/{EngineInitTimingProbe,StartupDiagnosticsProbe}.cs` (NEW) — coverable probe seams.
5. `UtilitiesCS/.../SpamBayes/{SpamBayes.cs split + Conditions/Actions/Classify, SpamInitTimingProbe.cs}` — partial-class split (705→446) + spam-init probe.
6. `UtilitiesCS/OutlookObjects/Store/{StoresWrapper,StoreWrapper}.cs` (MODIFIED) + `{StoreFilterAttribution,StoreWrapperInitClock,StoreWrapperInitProbe}.cs` (NEW).
7. 4 `.csproj` files — explicit `<Compile Include>` wiring for the new files (no-glob convention preserved).

---

## 10. Compliance Verdict

### Overall Status: ⚠️ PARTIALLY COMPLIANT

The branch is high quality: all four C# toolchain steps pass cleanly (format, analyzers, nullable/TWAE, 4109/4109 tests), all changed files are ≤500 lines, no banned APIs, no evidence-location violations, behavior-preservation is well-argued and the AC10 fix follows the bugfix workflow with documented red-before-green evidence. New-code coverage meets the ≥90% floor. The single policy gap is the repo-wide C# line-coverage gate (61.90% < 80%), which is pre-existing and not regressed by this branch.

**Fail-closed reminder:** Because the repo-wide ≥80% coverage gate is not met (FAIL), this audit is NOT marked fully compliant or PASS.

---

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- ✅ Before Making Changes
- ✅ Design Principles
- ✅ Module & File Structure (incl. 500-line cap; SpamBayes.cs brought into compliance)
- ✅ Naming, Docs, Comments
- ✅ Toolchain Execution (all four steps pass)
- ✅ Summarize & Document

#### Language-Specific Code Change Policy (Section 3, C#)
- ✅ Tooling & Baseline (csharpier/analyzers/nullable all clean)
- ✅ Design & Type-Safety (no banned APIs, nullable-safe)
- ✅ Error Handling / Logging

#### General Unit Test Policy (Section 1)
- ✅ Core Principles
- ⚠️ Coverage & Scenarios (new-code PASS; repo-wide ≥80% FAIL, pre-existing)
- ✅ Test Structure
- ✅ External Dependencies
- ✅ Policy Audit

#### Language-Specific Unit Test Policy (Section 4, C#)
- ✅ Framework & Libraries (MSTest/Moq/FluentAssertions)
- ✅ Test Style & Structure
- ⚠️ Coverage (new ≥90% PASS; repo-wide ≥80% FAIL)

---

### Metrics Summary

- ✅ 4109/4109 tests passing (100%)
- ✅ New-code coverage 94.92% (helpers 95–100%)
- ❌ Repo-wide C# line coverage 61.90% (< 80% gate; pre-existing, +0.06pp vs baseline)
- ✅ All 29 changed .cs files ≤ 500 lines
- ✅ All four C# toolchain checks passing
- ✅ Test execution time 17.18 s (fast)

---

### Recommendation

**Conditional Go (diagnostics + AC10 automated portion).** The automated work is mergeable on quality grounds. The repo-wide coverage gate FAIL is a pre-existing, non-regressing condition; merging is a maintainer judgment call given the documented COM/VSTO exemption. Outstanding items before issue #211 can be CLOSED (not blockers to merging the diagnostics/fix): the maintainer-gated runtime re-captures (AC5, AC9, AC10 cold-start re-capture, and per-probe runtime captures) remain placeholders, and #211's stated goal (eliminate the multi-minute latency) is not yet proven resolved by a runtime capture. See `remediation-inputs.2026-06-24T15-35.md`.

---

## Appendix A: Test Inventory (new/changed test classes)

- TaskMaster.Test.AppGlobals › JunkFolderPathNavigatorTests (correctness + enumeration-bound)
- TaskMaster.Test.AppGlobals › EngineInitTimingProbeTests
- TaskMaster.Test.AppGlobals › StartupDiagnosticsProbeTests
- TaskMaster.Test.AppGlobals › PhaseNetProbeTests
- TaskMaster.Test.AppGlobals › ContinuationProbeSequenceTests
- TaskMaster.Test.AppGlobals › ApplicationGlobalsStartupTimingTests (modified)
- TaskMaster.Test.AppGlobals › ApplicationGlobalsTests (modified; TestableApplicationGlobals extracted)
- UtilitiesCS.Test.EmailIntelligence › SpamInitTimingProbeTests
- UtilitiesCS.Test.OutlookObjects.Store › StoreFilterAttributionTests
- UtilitiesCS.Test.OutlookObjects.Store › StoreWrapperInitClockTests
- UtilitiesCS.Test.OutlookObjects.Store › StoreWrapperInitProbeTests

---

## Appendix B: Toolchain Commands Reference

**For C#:**
```bash
# Formatting
dotnet tool run csharpier check .

# Linting (.NET analyzers)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

# Type checking (nullable + TreatWarningsAsErrors)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true

# Testing + coverage
vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"TestCategory!=LiveOutlook" /EnableCodeCoverage
```

**Coverage artifacts inspected (not regenerated):**
- Baseline: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/baseline/baseline-coverage-2026-06-24T17-30.cobertura.xml`
- Post-change: `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/qa-gates/postchange-coverage-2026-06-24T17-30.cobertura.xml`

---

**Audit Completed By:** feature-reviewer agent
**Audit Date:** 2026-06-24
**Policy Version:** Current (as of audit date)
