# Policy Compliance Audit: hierarchical-lcppn-folder-prediction (#177) — Cycle 3 Exit Reaudit

**Audit Date:** 2026-06-16
**Code Under Test:** Full branch diff `TaskMaster-wt-2026-06-08-12-06` vs merge-base `c12aaf1c` (main). Cycle-3 production-migration delta is `0b589c83..HEAD` (commits `cc769a05`, `c7ef085a`, `f4159154`). C#-only source scope. Changed source files (cycle 3):
- `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/LcppnFolderPredictorStore.cs` (NEW)
- `TaskMaster/AppGlobals/AppAutoFileObjects.FolderPredictorLoad.cs` (NEW)
- `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs` (MODIFIED)
- `UtilitiesCS/EmailIntelligence/Bayesian/LcppnFolderPredictorConfig.cs` (MODIFIED — doc-only on `UseLcppnPredictor`)
- `UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs` (MODIFIED — `UseLcppnPredictor` getter added)
- `TaskMaster/AppGlobals/AppAutoFileObjects.cs` (MODIFIED — `partial` keyword + 2 wiring lines)
- `TaskMaster/Properties/Settings.Designer.cs` / `Settings.settings` / `app.config` (generated/config — `UseLcppnPredictor` setting, default True)
- Tests: `UtilitiesCS.Test/EmailIntelligence/FolderPredictorSeam_DefaultOn_Tests.cs` (NEW), `UtilitiesCS.Test/EmailIntelligence/LcppnFolderPredictorStore_Tests.cs` (NEW), `TaskMaster.Test/AppGlobals/AppAutoFileObjectsFolderPredictorTests.cs` (NEW)

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 8 source + 3 test files (cycle 3) | 4019 (combined) | ✅ 4019 pass, 0 fail | First-party prod-only (deduped) 61.98% lines; OlFolderClassifierGroup.cs 65.38% | First-party prod-only (deduped) 62.04% lines; OlFolderClassifierGroup.cs 72.73% | New files 100% (Store 32/32, FolderPredictorLoad 10/10); changed testable lines 100% |
| PowerShell | 0 files | N/A | N/A | N/A (no changed files) | N/A (no changed files) | N/A (no changed files) |
| Python | 0 files | N/A | N/A | N/A (no changed files) | N/A (no changed files) | N/A (no changed files) |
| TypeScript | 0 files | N/A | N/A | N/A (no changed files) | N/A (no changed files) | N/A (no changed files) |

**Note:** Only C# source files changed across the full branch diff. PowerShell, Python, and TypeScript have zero changed files; their verdicts are N/A by the scope invariant (acceptable only because zero files of those languages changed).

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope` (zero changed TypeScript files)
- TypeScript post-change coverage artifact: `N/A - out of scope` (zero changed TypeScript files)
- PowerShell baseline coverage artifact: `N/A - out of scope` (zero changed PowerShell files)
- PowerShell post-change coverage artifact: `N/A - out of scope` (zero changed PowerShell files)
- C# baseline coverage artifact: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/test-coverage-baseline.2026-06-16T01-04.md`
- C# post-change coverage artifact: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/final-test-coverage.2026-06-16T01-04.md` (merged Cobertura `final-cov.cobertura.xml`)
- Per-language comparison summary: Section 1.2.1 below; artifact `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/coverage-delta.2026-06-16T01-04.md`

**Non-negotiable verdict rule:** This audit reports PASS for C#. Numeric baseline and post-change coverage are provided for C# (the only in-scope language) plus new/changed-code coverage.

**Fail-closed rule:** All required baseline, QA, and coverage-comparison artifacts are present (verified on disk). No fail-closed condition is triggered.

**Evidence rule:** All metrics are read from the recorded cycle-3 QA-gate evidence artifacts and verified against the live diff and file line counts. No values are synthesized from memory.

---

## Executive Summary

Cycle 3 is the Option-B production-migration scope expansion for #177. It makes the previously merged additive, flag-gated LCPPN folder predictor reachable in production (default ON via a persisted setting) and persistent across restart (own-file serialize + startup rehydrate), while preserving flat-only behavior when the setting is toggled OFF. The work was verified against the live branch diff (`0b589c83..HEAD`) and the recorded cycle-3 QA-gate evidence.

Findings: the default-ON wiring reaches production callers with no per-call edits and no `UtilitiesCS -> TaskMaster.Properties.Settings` layering violation (only a resolved `bool` crosses the `IAppAutoFileObjects` boundary). The persistence round-trips through the dedicated `LcppnFolder.json` (the `Config`/`Disk` exclusion via `DoNotSerializeContractResolver("Config")` is verified by an explicit round-trip test, so rehydration is not a silent no-op), and the load path is fail-soft on missing/unreadable file (does not throw). Containment is held: ZERO diff in `SpamBayes.cs`, `Triage.cs`, `CategoryClassifierGroup.cs`, `MulticlassEngine.cs`; `ManagerAsyncLazy` value typing unchanged; flat rebuild retained. The over-cap caller files (`FolderScorer.cs` 608, `SortEmail.cs` 1406, `EmailFiler.cs`) are unchanged; all new files are <= 500 lines.

The full C# toolchain is green in a single final pass per the recorded evidence (csharpier exit 0, analyzers exit 0, nullable/TWAE exit 0, 4019 tests pass with coverage). New/changed code meets the >= 90% strict floor. The repository-wide deduped figure (74.11%; first-party prod-only 62.04%) remains below 80%, but this is the pre-existing repo state dominated by VSTO/WinForms/Outlook-Interop COM-host-bound code that CLAUDE.md formally exempts from the floor (testable-denominator exemption). Cycle 3 added no new untestable surface beyond the minimal COM-bound wiring (the two `LoadParallel`/`LoadSequential` call sites and the serialize block inside the COM-bound `BuildClassifiersAsync`); all new testable logic is 100% covered. This repo-wide shortfall is therefore classified as exempt, not a blocking finding.

**Policy documents evaluated:**
- ✅ `CLAUDE.md` (all sections, incl. COM/VSTO/WinForms coverage exemption and the 80%/90% floors on the testable denominator)
- ✅ `.claude/rules/general-code-change.md`
- ✅ `.claude/rules/general-unit-test.md`

**Language-specific policies evaluated:**
- ✅ C#: `.claude/rules/csharp.md` + CLAUDE.md C# Code Change Policy + C# Unit Test Policy
- N/A Python (zero changed files)
- N/A PowerShell (zero changed files)
- N/A TypeScript (zero changed files)

**Temporary artifacts cleanup:**
- ✅ No temporary/one-time scripts were created by this review.
- ✅ No throwaway scripts remain; the review is evidence-verification only.
- Scripts created during development: none (review agent produces audit artifacts only, no source/script changes).

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | ✅ PASS | New tests share no mutable static state across cases; `AppAutoFileObjectsFolderPredictorTests` saves/restores `Settings.Default.UseLcppnPredictor` per test in `[TestInitialize]`/`[TestCleanup]`. Seam tests build fresh mock globals per case. |
| **Isolation** - Each test targets single behavior | ✅ PASS | Each `[TestMethod]` exercises one selection/load/round-trip path (e.g., `DefaultOn_NoExplicitFlag_SelectsLcppnWhenHeld`, `LoadFolderPredictorAsync_SettingOnButFileMissing_LeavesHolderNull`). |
| **Fast Execution** - Tests complete quickly | ✅ PASS | All new tests are in-memory (Moq seams, in-memory serialize). Combined suite of 4019 passes; evidence `final-test-coverage.2026-06-16T01-04.md` exit 0. |
| **Determinism** - Consistent results | ✅ PASS | No clock/network/filesystem dependence; deserialization uses the injectable `FolderPredictorDeserializer` delegate; round-trip uses `SerializeToString`/`DeserializeObject` in memory. |
| **Readability & Maintainability** - Clear structure | ✅ PASS | Descriptive names map to AC IDs in comments; AAA structure with explicit Arrange/Act/Assert markers. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | ✅ PASS | **Baseline (pre-cycle-3):** first-party prod-only 61.98% lines; OlFolderClassifierGroup.cs 65.38%.<br>**Command:** `vstest.console.exe ... /InIsolation /Settings:TaskMaster.runsettings; dotnet-coverage merge -f cobertura`<br>**Timestamp:** 2026-06-16T01-04<br>**Artifact:** `evidence/baseline/test-coverage-baseline.2026-06-16T01-04.md`. |
| **No Coverage Regression** | ✅ PASS | **Post-change:** first-party prod-only 62.04% lines; OlFolderClassifierGroup.cs 72.73%.<br>**Change:** +0.06% repo prod-only; OlFolderClassifierGroup +7.35%.<br>**Status:** No regression on changed lines (changed regions 8/8 = 100%). Baseline 61.98% -> Post-change 62.04% (+0.06%) ✅ on the changed surface. |
| **New Code Coverage >=90%** | ✅ PASS | **New files:** `LcppnFolderPredictorStore.cs` 32/32 = 100.00%; `AppAutoFileObjects.FolderPredictorLoad.cs` 10/10 = 100.00%.<br>**New code coverage:** 100% (>= 90%).<br>**Calculation:** per-file line-rate from merged Cobertura, evidence `coverage-delta.2026-06-16T01-04.md`. |
| **Comprehensive Coverage** | ✅ PASS | Selection (AC21), fallback (AC22), persistence/load (AC23), flag-off parity (AC13) each have dedicated tests. Untested code on touched files is the pre-existing Outlook-COM-bound `BuildClassifiersAsync` body and VSTO load orchestration (exempt). |
| **Positive Flows** - Valid inputs | ✅ PASS | `DefaultOn_NoExplicitFlag_SelectsLcppnWhenHeld`, `LoadFolderPredictorAsync_SettingOnWithPersistedFile_PopulatesHolder`, `RoundTrip_WithDedicatedConfig_PreservesContentAndFileName`. |
| **Negative Flows** - Invalid inputs | ✅ PASS | `BuildConfig_NullOrEmptyAppData_Throws` (null/empty AppData fail-fast). |
| **Edge Cases** - Boundary conditions | ✅ PASS | `LoadFolderPredictorAsync_AppDataMissing_FailsSoftToNull`, `DefaultOn_NoHeldPredictor_FallsBackToFlat` (ON but holder null). |
| **Error Handling** - Error paths | ✅ PASS | `LoadFolderPredictorAsync_SettingOnButReadThrows_FailsSoftToNull` (IOException caught, holder null, no throw); `LoadFolderPredictorAsync_SettingOnButFileMissing_LeavesHolderNull`. |
| **Concurrency** - If applicable | ✅ PASS (N/A behavior) | Load path is awaited single-shot at startup; no new concurrent state introduced. `ManagerAsyncLazy` typing unchanged. |
| **State Transitions** - If applicable | ✅ PASS | Holder transitions (null -> populated on successful load; stays null on fail-soft) are tested across the five load-path cases. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 61.98% lines (first-party production-only, deduped) -> Post-change: 62.04% lines. Change: +0.06% lines. New/changed-code coverage: 100%. Disposition: PASS. Evidence: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/coverage-delta.2026-06-16T01-04.md`, `.../evidence/qa-gates/final-test-coverage.2026-06-16T01-04.md`.
- PowerShell: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (zero changed PowerShell files).
- Python: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (zero changed Python files).
- TypeScript: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (zero changed TypeScript files).

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | ✅ PASS | FluentAssertions with rationale strings (e.g., `.Should().BeTrue("the persisted production default is ON")`, `.Should().BeFalse("OFF must not attempt a load")`). |
| **Arrange-Act-Assert Pattern** | ✅ PASS | All new tests use explicit `// Arrange` / `// Act` / `// Assert` comments. |
| **Document Intent** | ✅ PASS | Class-level XML summaries cite the ACs covered; each test has an AC-tagged comment. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | ✅ PASS | No Outlook COM, no network, no DB. Globals/AF/FS are Moq mocks; serialization is in-memory. |
| **Use Mocks/Stubs** | ✅ PASS | `Mock<IApplicationGlobals>`, `Mock<IAppAutoFileObjects>`, `Mock<IFileSystemFolderPaths>`; deserialization via injectable `FolderPredictorDeserializer` delegate. |
| **Environment Stability** | ✅ PASS | No temporary files (round-trip uses `SerializeToString`/`DeserializeObject`). `Settings.Default.UseLcppnPredictor` is saved/restored per test, so no leaked global state. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | ✅ PASS | This artifact plus `code-review.2026-06-16T02-06.md` and `feature-audit.2026-06-16T02-06.md` constitute the cycle-3 exit reaudit. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | ✅ PASS | Cycle-3 scope and ACs defined in `2026-06-16T01-04-remediation/remediation-inputs.2026-06-16T01-04.md` (F4/F5, AC21–AC24). |
| **Read existing change plans** | ✅ PASS | `2026-06-16T01-04-remediation/remediation-plan.2026-06-16T01-04.md` present and executed (Phases 0–5). |
| **Document the plan** | ✅ PASS | Plan + phase-by-phase QA-gate evidence under `evidence/qa-gates/` and `evidence/baseline/`. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | ✅ PASS | A single `bool` (`UseLcppnPredictor`) crosses the `IAppAutoFileObjects` boundary; the config is resolved once via lazy getter. No new abstraction framework. |
| **Reusability** | ✅ PASS | File name/path centralized in `LcppnFolderPredictorStore` (single constant + `BuildConfig`/`BuildSettings`), shared by serialize (build) and deserialize (load) paths. |
| **Extensibility** | ✅ PASS | `FolderPredictorConfig` getter/setter and `FolderPredictorDeserializer` delegate are injectable seams; explicit config still overrides the persisted default. |
| **Separation of concerns** | ✅ PASS | Settings access lives in `AppAutoFileObjects.FolderPredictorLoad.cs` (TaskMaster); `UtilitiesCS` never references `TaskMaster.Properties.Settings`. Pure store/config logic separated from VSTO load orchestration. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | ✅ PASS | New `LcppnFolderPredictorStore` (persistence location) and `AppAutoFileObjects.FolderPredictorLoad` partial (load/rehydration) each have a single purpose. |
| **Under 500 lines** | ✅ PASS | Verified by `awk NR`: Store 67, FolderPredictorLoad 102, OlFolderClassifierGroup 345, LcppnFolderPredictorConfig 125, IAppAutoFileObjects 63, test files 168/101/161. Pre-existing over-cap `AppAutoFileObjects.cs` is 849 (was 847; +2 wiring + partial keyword — minimal, permitted; load body lives in the new partial). Over-cap callers `FolderScorer.cs` 608 / `SortEmail.cs` 1406 unchanged in cycle 3. |
| **Public vs internal** | ✅ PASS | `FolderPredictorDeserializer` is `internal` (test seam); `LoadFolderPredictorAsync`/`UseLcppnPredictor` are the intended public surface; store helpers are `public static` for test reach. |
| **No circular dependencies** | ✅ PASS | `UtilitiesCS` -> only the resolved `bool` via `IAppAutoFileObjects`; TaskMaster -> UtilitiesCS. No reverse reference to `TaskMaster.Properties.Settings` from UtilitiesCS. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | ✅ PASS | `ResolveFolderPredictorConfigFromSettings`, `LoadFolderPredictorAsync`, `BuildConfig`, `BuildSettings`, `FolderPredictorDeserializer`. |
| **Docs/docstrings** | ✅ PASS | XML doc comments on the new public/interface members and on the rationale for the `Config` exclusion. |
| **Comment why, not what** | ✅ PASS | Comments explain the layering boundary (bool crossing globals), the fail-soft rationale, and why `Config`/`Disk` is excluded from the serialized document. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ✅ PASS | **Command:** `csharpier check .`<br>**Result:** Clean; checked 1080 files; exit 0 (`final-csharpier.2026-06-16T01-04.md`). |
| **2. Linting** | ✅ PASS | **Command:** `msbuild TaskMaster.sln /t:Build /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`<br>**Result:** Build succeeded, 0 analyzer errors; exit 0 (`final-analyzers.2026-06-16T01-04.md`). |
| **3. Type checking** | ✅ PASS | **Command:** `msbuild TaskMaster.sln /t:Build /p:Nullable=enable /p:TreatWarningsAsErrors=true`<br>**Result:** 0 warnings, 0 errors; exit 0 (`final-nullable.2026-06-16T01-04.md`). |
| **4. Testing** | ✅ PASS | **Command:** `vstest.console.exe ... /InIsolation /Settings:TaskMaster.runsettings` (both assemblies)<br>**Result:** 4019 passed, 0 failed; exit 0 (`final-test-coverage.2026-06-16T01-04.md`). |
| **Full toolchain loop** | ✅ PASS | Final pass clean with no auto-fix; csharpier reported no files changed, so no restart required. |
| **Explicit reporting** | ✅ PASS | Commands and results recorded in the cycle-3 QA-gate evidence files. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | ✅ PASS | Commit messages `cc769a05`/`c7ef085a`/`f4159154` and `evidence/issue-updates/ac-status.2026-06-16T01-04.md`. |
| **Design choices explained** | ✅ PASS | Layering boundary, lazy config resolution, own-file persistence, and `Config` exclusion documented in code and remediation plan. |
| **Update supporting documents** | ✅ PASS | `user-story.md` AC21–AC24 added and checked off; AC status evidence recorded. |
| **Provide next steps** | ✅ PASS | Exit condition (`blocking_count == 0`) defined in remediation-inputs; this reaudit confirms it. |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3A: Python — N/A (zero changed Python files)
### Section 3B: PowerShell — N/A (zero changed PowerShell files)
### Section 3C: Bash — N/A (zero changed Bash files)
### Section 3D: JSON — N/A (no governed JSON schema files changed; `.settings`/`app.config` are XML config, not governed JSON)

### Section 3E: C# Code Change Policy Compliance

#### 3E.1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with CSharpier** | ✅ PASS | `csharpier check .` clean, exit 0. |
| **Linting / .NET analyzers** | ✅ PASS | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` 0 errors, exit 0. |
| **Type checking / nullable (TWAE)** | ✅ PASS | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` 0 warnings/0 errors, exit 0. |
| **Testing — MSTest via vstest + coverage** | ✅ PASS | 4019 pass / 0 fail; coverage merged Cobertura; exit 0. |

#### 3E.2 C# Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Strong contracts / explicit APIs** | ✅ PASS | `IAppAutoFileObjects.UseLcppnPredictor` getter; `BuildConfig`/`BuildSettings` typed returns; `Func<LcppnFolderPredictor, Task<LcppnFolderPredictor>>` seam. |
| **Null-safety by default** | ✅ PASS | Nullable gate green; `BuildConfig` guards via `ThrowIfNullOrEmpty`; load path null-checks the deserialized predictor and the `SpecialFolders` lookup. |
| **Composition / focused types** | ✅ PASS | New store is a focused static class; partial split keeps `AppAutoFileObjects` cohesive. |
| **Async / resource safety** | ✅ PASS | `LoadFolderPredictorAsync` is `async Task`, awaited from both `LoadParallel`/`LoadSequential` paths. |

#### 3E.3 C# Error Handling, Logging, Contracts

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Exceptions / fail fast** | ✅ PASS | `BuildConfig` fails fast on null/empty AppData; the load path deliberately catches at the startup boundary and re-surfaces via logging (fail-soft is the required AC23 contract). |
| **Logging** | ✅ PASS | Uses log4net `_folderPredictorLogger` (`Warn` for missing file/unresolved AppData, `Error` for genuine read/parse failure) per the project logging pattern. |
| **Contracts / invariants** | ✅ PASS | OFF setting short-circuits load; ON-with-null-holder falls back to flat (AC22); class-level config default stays `false` so direct-construct AC13 tests are not masked. |

---

## 4. Language-Specific Unit Test Policy Compliance

### Section 4A: Python — N/A (no Python tests changed)
### Section 4B: PowerShell — N/A (no PowerShell tests changed)

### Section 4C: C# Unit Test Policy Compliance

#### 4C.1 Framework and Scope

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use MSTest** | ✅ PASS | `[TestClass]`/`[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting` in all three new test files. |
| **Coverage expectation** | ✅ PASS | New files 100%; changed testable lines 100% (>= 90% strict). Repo-wide below 80% only due to the documented COM/VSTO testable-denominator exemption. |

#### 4C.2 Libraries and Conventions

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Moq for mocking** | ✅ PASS | `Mock<IApplicationGlobals>`, `Mock<IAppAutoFileObjects>`, `Mock<IFileSystemFolderPaths>`. |
| **FluentAssertions for assertions** | ✅ PASS | `.Should()...` throughout; no weakened assertions. |
| **AAA + isolation** | ✅ PASS | Explicit AAA; per-test setting save/restore; in-memory serialization. |

#### 4C.3 Running the Toolchain

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use vstest / MSTest** | ✅ PASS | `vstest.console.exe ... /InIsolation /Settings:TaskMaster.runsettings`; 4019 pass. |
| **No alternative runners** | ✅ PASS | No xUnit/NUnit introduced. |

---

## 5. Test Coverage Detail

### FolderPredictorSeam_DefaultOn_Tests (4 tests)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| DefaultOn_NoExplicitFlag_SelectsLcppnWhenHeld | Positive (AC21) | FolderPredictorConfig getter + ResolveFolderPredictorConfigFromSettings + GetFolderPredictorAsync ON branch | ✅ |
| DefaultOn_NoHeldPredictor_FallsBackToFlat | Edge (AC22) | GetFolderPredictorAsync fallback (holder null) | ✅ |
| ToggleOff_ResolvesFlatOnly_PreservingAc13 | Negative/parity (AC13) | OFF resolution path | ✅ |
| ExplicitConfig_OverridesPersistedDefault | Positive (AC21 seam) | setter override path | ✅ |

**Coverage:** OlFolderClassifierGroup changed regions 8/8 = 100%.

### LcppnFolderPredictorStore_Tests (4 tests)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| FileName_IsDedicatedAndDistinctFromFolderJson | Positive (AC23) | FileName constant | ✅ |
| BuildConfig_TargetsDedicatedFileInBayesianFolder | Positive (AC23) | BuildConfig path | ✅ |
| BuildConfig_NullOrEmptyAppData_Throws | Negative (AC23) | ThrowIfNullOrEmpty guard | ✅ |
| RoundTrip_WithDedicatedConfig_PreservesContentAndFileName | Positive/round-trip (AC23) | BuildSettings + Config exclusion + round-trip | ✅ |

**Coverage:** LcppnFolderPredictorStore.cs 32/32 = 100%.

### AppAutoFileObjectsFolderPredictorTests (5 tests)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| LoadFolderPredictorAsync_SettingOnWithPersistedFile_PopulatesHolder | Positive (AC23) | load success branch | ✅ |
| LoadFolderPredictorAsync_SettingOnButFileMissing_LeavesHolderNull | Edge (AC22/AC23) | null-predictor branch | ✅ |
| LoadFolderPredictorAsync_SettingOnButReadThrows_FailsSoftToNull | Error (AC22/AC23) | catch/log branch | ✅ |
| LoadFolderPredictorAsync_SettingOff_DoesNotLoad | Negative (AC13) | OFF short-circuit | ✅ |
| LoadFolderPredictorAsync_AppDataMissing_FailsSoftToNull | Edge (AC23) | unresolved AppData branch | ✅ |

**Coverage:** AppAutoFileObjects.FolderPredictorLoad.cs 10/10 = 100%.

**Not covered:** Pre-existing Outlook-COM-bound `BuildClassifiersAsync` body (incl. the serialize block) and VSTO load orchestration in `AppAutoFileObjects.cs` — exempt under CLAUDE.md COM/VSTO testable-denominator exemption.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 4019 (UtilitiesCS.Test 3912 + TaskMaster.Test 107) | ✅ |
| Tests Passed | 4019 (100%) | ✅ |
| Tests Failed | 0 | ✅ |
| New cycle-3 tests | +13 (4 DefaultOn + 4 Store + 5 load-path) | ✅ |
| Functions/Classes Tested | New store + load partial + config resolver + seam: all new testable members covered | ✅ |
| Test File Size | 168 / 101 / 161 lines (all <= 500) | ✅ |
| Code Coverage | New files 100% lines; first-party prod-only 62.04% (exempt-governed) | ✅ on new/changed; exempt on repo-wide |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `csharpier check .` | Clean, 1080 files, exit 0 | ✅ |
| .NET Analyzers | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 errors, exit 0 | ✅ |
| Nullable / TWAE | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 warnings/errors, exit 0 | ✅ |
| MSTest via vstest | `vstest.console.exe ... /InIsolation /Settings:TaskMaster.runsettings` | 4019 pass / 0 fail, exit 0 | ✅ |

**Notes:** Pre-existing CS8632/CS0067 warnings in unrelated test files are unchanged and non-blocking (recorded in `final-analyzers.2026-06-16T01-04.md`). Pre-existing flaky `IdleAsyncQueue_Tests.AddEntry_UseUiThreadTrue` is out of scope (passes in isolation; tracked under `ci-flaky-test-isolation-176`).

---

## 8. Gaps and Exceptions

### Identified Gaps
**None blocking.** The repository-wide line coverage figure (74.11% deduped; 62.04% first-party production-only) is below the 80% floor, but this is a pre-existing repo state, not a cycle-3 regression (it moved +0.06%). It is fully attributable to VSTO add-in lifecycle, WinForms/Designer, and Outlook-Interop event-handler code formally exempted by CLAUDE.md.

### Approved Exceptions
- **Repo-wide 80% floor — testable-denominator exemption (CLAUDE.md, ratified):** applied to the pre-existing COM/VSTO/WinForms-bound denominator. Cycle 3 introduced no new untestable surface beyond the minimal COM-bound wiring (two await call sites + the serialize block inside the COM-bound `BuildClassifiersAsync`); all new testable logic is 100% covered.
- **Fail-soft catch at the startup load boundary (AC23):** a deliberate broad catch at a defined boundary that re-surfaces via log4net, required so a missing/corrupt predictor file does not break startup. Consistent with the General Code Change Policy boundary-catch carve-out.

### Removed/Skipped Tests
**None.** No tests were removed or skipped in cycle 3.

### Latent defect noted (out of scope, not introduced this cycle)
- **`FilePathHelper` deserialization re-entrancy:** confirmed pre-existing (`FilePathHelper` has zero diff in `0b589c83..HEAD`). Cycle 3 works around it by excluding the runtime-only `Config`/`Disk` from the serialized document via `DoNotSerializeContractResolver("Config")`. Correctly out of scope; tracked separately.

### Rejected Scope Narrowing
None. The caller prompt scoped the review to the cycle-3 delta but explicitly instructed verification against the actual diff and that AC1–AC20 remain satisfied; the full-branch audit was performed (only C# source changed; all coverage verdicts are explicit). No narrowing of any language's coverage or skipping of any toolchain check was requested or applied.

### Evidence Location Compliance
No evidence artifacts were written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` in the branch diff. All cycle-3 evidence is under the canonical `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/<kind>/`. No violations. (The committed `artifacts/csharp/coverage.xml` dated 2026-06-12 is a pre-cycle-3 C# coverage artifact at the conventional C# coverage path, not an evidence-location violation.)

---

## 9. Summary of Changes

### Commits in This Cycle (cycle 3)
1. **cc769a05** — feat(folder-predictor): default-ON LCPPN config + own-file persistence/load (#177)
2. **c7ef085a** — test(folder-predictor): AC21/AC22/AC23 tests + own-file deserialize fix (#177)
3. **f4159154** — docs(folder-predictor): cycle-3 Phase 5 QA gates + AC status (#177)

### Files Modified (cycle-3 source)
1. **UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/LcppnFolderPredictorStore.cs** (NEW) — dedicated `LcppnFolder.json` location + shared serialize/load settings; `Config` excluded via `DoNotSerializeContractResolver`.
2. **TaskMaster/AppGlobals/AppAutoFileObjects.FolderPredictorLoad.cs** (NEW) — `UseLcppnPredictor` accessor (from `Properties.Settings`), injectable deserializer seam, fail-soft `LoadFolderPredictorAsync`.
3. **UtilitiesCS/.../OlFolderClassifierGroup.cs** (MODIFIED) — lazy `FolderPredictorConfig` resolved from `Globals.AF.UseLcppnPredictor`; serialize built predictor to own file in the build path.
4. **UtilitiesCS/.../LcppnFolderPredictorConfig.cs** (MODIFIED) — doc-only; class default stays `false`.
5. **UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs** (MODIFIED) — `bool UseLcppnPredictor { get; }`.
6. **TaskMaster/AppGlobals/AppAutoFileObjects.cs** (MODIFIED) — `partial` keyword + 2 wiring await calls.
7. **TaskMaster/Properties/Settings.{Designer.cs,settings}, app.config** (config) — `UseLcppnPredictor` setting, default True.
8. **Tests** (NEW x3) — DefaultOn seam, Store persistence, load-path/fail-soft.

---

## 10. Compliance Verdict

### Overall Status: ✅ FULLY COMPLIANT

Cycle 3 satisfies the General Code Change, General Unit Test, and C# policies. Default-ON wiring reaches production with no per-call edits and no layering violation; persistence round-trips and rehydrates (not a no-op); fail-soft load does not throw; containment held; over-cap callers untouched; new files <= 500 lines; full C# toolchain green in a single final pass; new/changed code coverage 100% (>= 90% strict). The only sub-80% figure is the pre-existing COM/VSTO-bound repo-wide denominator under the ratified CLAUDE.md exemption, with no new untestable surface added by this cycle.

**Fail-closed reminder:** No required baseline, QA, coverage, or comparison artifact is missing; PASS is warranted.

---

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- ✅ Before Making Changes: scope/plan/evidence present
- ✅ Design Principles: minimal bool boundary, centralized store, injectable seams, clean separation
- ✅ Module & File Structure: all new files <= 500; over-cap callers untouched; partial split minimal
- ✅ Naming, Docs, Comments: descriptive, why-focused
- ✅ Toolchain Execution: csharpier/analyzers/nullable/tests all exit 0
- ✅ Summarize & Document: commits + AC status + user-story updated

#### Language-Specific Code Change Policy (Section 3)
**For C#:**
- ✅ Tooling & Baseline: full toolchain green
- ✅ Design & Type-Safety: nullable clean, guard clauses, async correct
- ✅ Error Handling: fail-fast on config, fail-soft-with-logging at startup boundary

#### General Unit Test Policy (Section 1)
- ✅ Core Principles
- ✅ Coverage & Scenarios (new 100%; repo-wide exempt)
- ✅ Test Structure
- ✅ External Dependencies (no temp files, fully mocked)
- ✅ Policy Audit

#### Language-Specific Unit Test Policy (Section 4)
**For C#:**
- ✅ Framework & Scope (MSTest + Moq + FluentAssertions)
- ✅ Test Style & Structure (AAA, isolated)
- ✅ Naming & Readability (AC-tagged)
- ✅ Toolchain (vstest, 4019 pass)

---

### Metrics Summary
- ✅ 4019/4019 tests passing (100%)
- ✅ New files 100% line coverage (Store 32/32, FolderPredictorLoad 10/10)
- ✅ Changed testable lines 100%; OlFolderClassifierGroup 65.38% -> 72.73% (no regression)
- ⚠️ Repo-wide first-party prod-only 62.04% (below 80%, exempt — pre-existing COM/VSTO denominator)
- ✅ All C# code-quality checks passing in a single final pass
- ✅ Containment held (ZERO diff in spam/triage/category/multiclass; ManagerAsyncLazy typing unchanged)

---

### Recommendation

**Ready for merge.** `blocking_count == 0` for this artifact. Cycle-3 exit condition is met: AC21–AC24 satisfied, AC1–AC20 still satisfied (AC13 re-verified), coverage policy met on new/changed code with the repo-wide figure governed by the documented exemption, and the full C# toolchain green in a single final pass.

---

## Appendix A: Test Inventory

### Cycle-3 new tests
1. FolderPredictorSeam_DefaultOn_Tests › DefaultOn_NoExplicitFlag_SelectsLcppnWhenHeld
2. FolderPredictorSeam_DefaultOn_Tests › DefaultOn_NoHeldPredictor_FallsBackToFlat
3. FolderPredictorSeam_DefaultOn_Tests › ToggleOff_ResolvesFlatOnly_PreservingAc13
4. FolderPredictorSeam_DefaultOn_Tests › ExplicitConfig_OverridesPersistedDefault
5. LcppnFolderPredictorStore_Tests › FileName_IsDedicatedAndDistinctFromFolderJson
6. LcppnFolderPredictorStore_Tests › BuildConfig_TargetsDedicatedFileInBayesianFolder
7. LcppnFolderPredictorStore_Tests › BuildConfig_NullOrEmptyAppData_Throws
8. LcppnFolderPredictorStore_Tests › RoundTrip_WithDedicatedConfig_PreservesContentAndFileName
9. AppAutoFileObjectsFolderPredictorTests › LoadFolderPredictorAsync_SettingOnWithPersistedFile_PopulatesHolder
10. AppAutoFileObjectsFolderPredictorTests › LoadFolderPredictorAsync_SettingOnButFileMissing_LeavesHolderNull
11. AppAutoFileObjectsFolderPredictorTests › LoadFolderPredictorAsync_SettingOnButReadThrows_FailsSoftToNull
12. AppAutoFileObjectsFolderPredictorTests › LoadFolderPredictorAsync_SettingOff_DoesNotLoad
13. AppAutoFileObjectsFolderPredictorTests › LoadFolderPredictorAsync_AppDataMissing_FailsSoftToNull

Existing AC13 parity (unchanged, re-verified): FolderPredictorSeam_Tests › GetFolderPredictorAsync_FlagOff_* (4 tests).

---

## Appendix B: Toolchain Commands Reference

**For C#:**
```powershell
# Formatting
csharpier check .

# Linting (analyzers)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

# Type checking (nullable, warnings-as-errors)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true

# Testing + coverage
vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /Settings:TaskMaster.runsettings
dotnet-coverage merge <coverage files> -o final-cov.cobertura.xml -f cobertura
```

---

**Audit Completed By:** feature-review agent
**Audit Date:** 2026-06-16
**Policy Version:** Current (as of audit date)
