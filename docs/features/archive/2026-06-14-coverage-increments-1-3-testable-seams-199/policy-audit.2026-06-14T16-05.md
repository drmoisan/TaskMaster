# Policy Compliance Audit: coverage-increments-1-3-testable-seams (Issue #199) — Phase 5 re-audit

**Audit Date:** 2026-06-14
**Code Under Test:** Feature branch `refactor/coverage-increments-1-3-199` @ `aa3a7542` vs base `origin/main` @ `d436a06f` (merge base `d436a06f`). Full branch diff `d436a06f..aa3a7542`. This is a re-audit after the maintainer-authorized Phase 5 scope change (remediation-inputs.2026-06-14T15-10.md), which lifted the zero-production-change Non-Goal for exactly two narrow seams. Changed files (branch diff): 16 new/modified C# test files (14 new test files + 3 additive `.Test.csproj` `<Compile Include>` registrations) plus **two authorized production source files** (`UtilitiesCS/Properties/AssemblyInfo.cs`, `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`); remainder are feature scoping/evidence docs and agent-memory notes.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 16 files (14 new/changed tests + 3 test csproj registrations; 2 authorized production seams) | 185 MSTest (Phase-5 final suite ToDoModel.Test + TaskMaster.Test = 185; prior cycle reported 99 across all three projects) | ✅ 185 pass, 0 fail (per `evidence/qa-gates/p5-mstest-coverage.2026-06-14T15-10.md`) | 71.65% production-only testable denominator (post-#197, authority 197-COV-001) | strictly > 71.65% (numerator increased on the two authorized seams; denominator effectively unchanged) | 100% line-rate on the only new executable production code (the extracted static `MatchBestSpecialFolder` helper, lines 81-91, 8/8 reachable lines hit) |

**Note:** C# is the only language with changed files in the branch diff. No Python, TypeScript, PowerShell, or Bash files changed; those coverage checklist lines below are `N/A - out of scope` because zero files of those languages changed on the branch.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope` (no TypeScript files changed)
- TypeScript post-change coverage artifact: `N/A - out of scope` (no TypeScript files changed)
- PowerShell baseline coverage artifact: `N/A - out of scope` (no PowerShell files changed)
- PowerShell post-change coverage artifact: `N/A - out of scope` (no PowerShell files changed)
- C# baseline coverage artifact: `artifacts/csharp/coverage-firstparty.cobertura.xml` (pre-feature, post-#197; reviewer-parsed first-party line-rate 0.768 aggregate, production-only 71.65% per 197-COV-001)
- C# post-change coverage artifact: `artifacts/csharp/p5-coverage.cobertura.xml` (post-Phase-5; reviewer-parsed directly)
- Per-language comparison summary: Section 1.2.1 below

**Non-negotiable verdict rule:** Numeric baseline and post-change metrics for the one in-scope language (C#) are present below.

**Fail-closed rule:** All required baseline, QA, and coverage-comparison artifacts exist on disk and were inspected. No artifact is missing.

---

## Executive Summary

This re-audit covers the full branch diff `d436a06f..aa3a7542` after the maintainer-authorized Phase 5 scope change. Phase 5 lifted the spec's zero-production-change Non-Goal for exactly two narrow seams to close the AC1/AC3 Flag-and-Stop coverage gaps that prior cycles left open under the test-only constraint:

1. **`UtilitiesCS/Properties/AssemblyInfo.cs`** — adds one assembly attribute `[assembly: InternalsVisibleTo("ToDoModel.Test")]`, exposing the existing internal `MyBox.DialogInvoker` seam to `ToDoModel.Test`. No `MyBox` member behavior changes; the seam still defaults to the real dialog in production. Non-executable attribute.
2. **`TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`** — extracts the pure `MatchBestSpecialFolder` matching logic into a new `internal static string MatchBestSpecialFolder(IReadOnlyDictionary<string,string>, string)` helper. The public instance method now delegates (`return MatchBestSpecialFolder(SpecialFolders, path);`). The helper body is byte-for-byte the original matching logic (only the local reference renamed from `SpecialFolders` to the `specialFolders` parameter). Added `using System.Collections.Generic;`.

The reviewer verified by `git diff d436a06f..aa3a7542 --name-status` that **exactly two production source files changed**, both authorized. No `UtilitiesCS/Dialogs/MyBox.cs` change (the existing settable `DialogInvoker` sufficed, so no third seam was added). No `.props`, `.targets`, `coverage.config`, `*.runsettings`, or pipeline-script change. No `[ExcludeFromCodeCoverage]` attribute added or removed in any production diff (grep of `git diff -- TaskMaster/ UtilitiesCS/ ToDoModel/ QuickFiler/` returned only documentation/memory text, never production source). The #197 exemption boundary is unchanged.

**No production behavior change — verified.** The `InternalsVisibleTo` attribute is non-executable and changes only test-time visibility. The `MatchBestSpecialFolder` extraction preserves semantics exactly: a null/empty collection returns null; matching uses ordinal `string.Contains`; candidates are ordered by descending value length; the first key (default `KeyValuePair.Key` = null when no match) is returned. The public instance signature and behavior are identical. Confirmed by reading the source (`TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` lines 57-91) against the diff.

Coverage was verified by directly parsing `artifacts/csharp/p5-coverage.cobertura.xml` (reviewer ran a Python Cobertura parse rather than re-running coverage generation). The only NEW executable production code is the extracted static helper; its reachable body lines (81-84, 86, 89-91) are all hit (8/8 = 100%, exceeding the >= 90% new-code threshold). The instance-delegation lines (58, 62-63) remain uncovered — unchanged from the pre-extraction deferred-gap status (no regression; unit tests exercise the pure helper directly). Per-assembly production line-rates increased on the two authorized seams: `TaskMaster.AppFileSystemFolderPaths` class line-rate rose to 0.608 (helper now covered); `ToDoModel.ProjectEntry` covers the malformed-ID branch and the `CompareTo` length tie-break. The production-only testable denominator is effectively unchanged (the two seam edits add one non-executable attribute and one extracted-but-equivalent method); the numerator increased, so the post-#197 rate strictly increases versus 71.65%.

**Residual gap (assessed; not blocking).** The `ProjectEntry` change-confirmation branch (`SetProjectId` -> `ChangeId`, committing `ProjectID = newID`) routes through the `ProjectID` property setter's RAW, un-seamed `System.Windows.Forms.MessageBox.Show`, which deadlocks the STA test host. Covering it requires a THIRD production seam (routing the property setter through `MyBox`) that was NOT authorized for Phase 5. The executor correctly flagged-and-stopped (`evidence/other/p5-projectentry-changeconfirm-gap.2026-06-14T15-10.md`) and left the DoD "all target seams covered" item unchecked. Reviewer disposition: this is an acceptable, maintainer-pending scope boundary, not a blocking finding — see Section 8.

**Policy documents evaluated:**
- ✅ `general-code-change.md` (applicable)
- ✅ `general-unit-test.md` (testing)

**Language-specific policies evaluated:**
- N/A `python-code-change` + `python-unit-test` (no Python changed)
- N/A `powershell` (no PowerShell changed)
- ✅ C#: `csharp.md` C# Code Change Policy + C# Unit Test Policy
- N/A Bash, JSON

**Temporary artifacts cleanup:**
- ✅ No temporary or throwaway scripts were created by this feature; the reviewer created none.
- ✅ Ongoing tooling unchanged.
- No development scripts created; nothing to dispose.

---

## Rejected Scope Narrowing

The caller prompt explicitly instructed: "Determine scope yourself; no scope narrowing" and "assess the full branch diff d436a06f..HEAD." No narrowing was attempted by the caller. The Phase 5 context items (the two authorized seams, the residual change-confirmation gap, the per-assembly figures) were treated as evidence to weigh, not as scope limiters. The full branch diff (`d436a06f..aa3a7542`) was audited. No rejected narrowing to record.

---

## Evidence Location Compliance

The reviewer scanned the branch diff for files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`.

- Result: NONE. `git diff --name-only d436a06f..aa3a7542 | grep -iE '^artifacts/(baselines|qa|evidence|coverage)/'` returned no matches. All feature evidence is correctly written under the canonical `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/<kind>/` path (kinds: `baseline/`, `qa-gates/`, `other/`, `issue-updates/`).
- Cobertura coverage XML artifacts (e.g., `artifacts/csharp/p5-coverage.cobertura.xml`) are coverage-tool outputs in the established repo coverage-output location, gitignored and not committed; they are not feature evidence written under a non-canonical evidence path, and are not a violation.
- The `validate_evidence_locations.py --root .` script is not present in this repository (searched the full tree; not found). The mandated scan was therefore performed manually via the `git diff --name-only` grep above, which is authoritative for the branch diff. No violations found.

No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` events occurred; no caller instruction specified a non-canonical evidence path.

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | ✅ PASS | `ProjectEntryDialogBranchesTests` seeds `MyBox.DialogInvoker` in `[TestInitialize]` and restores the real invoker in `[TestCleanup]`, so the static seam mutation does not leak across tests. Settings-reading classes snapshot/restore `Settings.Default`. No static shared mutable fields between classes. |
| **Isolation** - Each test targets single behavior | ✅ PASS | Each `[TestMethod]` exercises one method/branch (e.g., `SetProjectId_MalformedId_ShowsErrorDialogAndReturnsFalse`, `MatchBestSpecialFolder_TwoCandidatesContained_LongerValueKeyWins`). Test files mirror the target class structure. |
| **Fast Execution** - Tests complete quickly | ✅ PASS | Phase-5 suite (ToDoModel.Test + TaskMaster.Test, 185 tests) ran in 3.90 s (`evidence/qa-gates/p5-mstest-coverage.2026-06-14T15-10.md`). All in-memory; async tests use synchronously-completing delegates. No I/O, no sleep. |
| **Determinism** - Consistent results | ✅ PASS | No randomness, no clock reads, no network, no filesystem, no live Outlook. The dialog branches inject a non-modal `MyBox.DialogInvoker` stub returning a fixed `DialogResult`, so no real WinForms dialog is shown. Diff scan for `Thread.Sleep`/`Task.Delay`/`Path.GetTemp`/`Directory.Create`/`Process.Start`/`new System.Windows.Forms` in added lines returned only two comment lines affirming "no Task.Delay / Sleep" — no actual timing or filesystem dependency. |
| **Readability & Maintainability** - Clear structure | ✅ PASS | Descriptive scenario-based method names; class-level XML doc summaries explain seam reachability and why the change-confirmation branch is excluded; explicit AAA comment markers. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | ✅ PASS | **Baseline (pre-feature, post-#197):** 71.65% production-only testable denominator (authority 197-COV-001).<br>**Command:** parse `artifacts/csharp/coverage-firstparty.cobertura.xml`<br>**Timestamp:** 2026-06-14 08:22<br>Recorded in `evidence/baseline/coverage-baseline.2026-06-14T08-22.md`. |
| **No Coverage Regression** | ✅ PASS | **Post-change coverage:** reviewer-parsed `artifacts/csharp/p5-coverage.cobertura.xml`: `TaskMaster.AppFileSystemFolderPaths` class line-rate 0.608; `ToDoModel.ProjectEntry` covered 80/181 (malformed-ID + CompareTo tie-break now covered).<br>**Change:** the only changed production line that is executable is the new static helper (100% covered) and the one-line delegation (unchanged 0% — same as pre-extraction deferred gap).<br>**Status:** No regression. The `InternalsVisibleTo` attribute is non-executable; the `MatchBestSpecialFolder` extraction adds equivalent code. No previously-covered line lost coverage. |
| **New Code Coverage ≥90%** | ✅ PASS | **New/changed production file:** `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` static helper. **New executable code:** lines 81-84, 86, 89-91 — reviewer-parsed hits all >= 1 (8/8 = 100%). **Calculation method:** per-line hit parse of `p5-coverage.cobertura.xml` for class `TaskMaster.AppFileSystemFolderPaths`. The `InternalsVisibleTo` attribute is non-executable. New-code coverage 100% >= 90%. |
| **Comprehensive Coverage** | ✅ PASS | The two authorized seams covered with positive/best-match/case/trailing-separator/no-match/null/empty for `MatchBestSpecialFolder` (9 tests) and malformed-ID + CompareTo length tie-break (both directions) for `ProjectEntry` (3 tests). The only untested targeted path is the single documented change-confirmation Flag-and-Stop. |
| **Positive Flows** - Valid inputs | ✅ PASS | E.g., `MatchBestSpecialFolder_PathContainsKnownValue_ReturnsThatKey`, `CompareTo_EqualOrdinalThenShorterOtherLength_ReturnsNegativeOne`. |
| **Negative Flows** - Invalid inputs | ✅ PASS | E.g., `MatchBestSpecialFolder_NullCollection_ReturnsNull`, `MatchBestSpecialFolder_EmptyCollection_ReturnsNull`, `SetProjectId_MalformedId_ShowsErrorDialogAndReturnsFalse`. |
| **Edge Cases** - Boundary conditions | ✅ PASS | E.g., `MatchBestSpecialFolder_TwoCandidatesContained_LongerValueKeyWins`, `MatchBestSpecialFolder_CaseMismatch_DoesNotMatch_ReturnsNull`, `MatchBestSpecialFolder_ValueWithoutTrailingSeparator_StillMatchesAsSubstring`, `MatchBestSpecialFolder_EmptyPath_NoValueContained_ReturnsNull`. |
| **Error Handling** - Error paths | ✅ PASS | `MatchBestSpecialFolder_NullPath_ThrowsNullReferenceException` asserts the documented behavior preserved from the original instance method (`path.Contains` dereferences null). |
| **Concurrency** - If applicable | N/A | Targeted seams are pure matching logic and value comparison; no concurrency surface. |
| **State Transitions** - If applicable | N/A for the Phase-5 seams | The Phase-5 seams are pure functions; queue state transitions (FilerQueue/QfcQueue/KbdActions) are covered by the earlier-increment tests carried on the branch. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 71.65% lines (production-only testable denominator, authority 197-COV-001) -> Post-change: > 71.65% lines (numerator increased on the two authorized seams; denominator effectively unchanged). Change: +positive lines (new static `MatchBestSpecialFolder` helper 8/8 covered; `ProjectEntry` malformed-ID + CompareTo tie-break newly covered). New/changed-code coverage: 100%. Disposition: PASS. Evidence: `artifacts/csharp/p5-coverage.cobertura.xml`, `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/p5-coverage-delta.2026-06-14T15-10.md`.
- TypeScript: Baseline: N/A - out of scope lines -> Post-change: N/A - out of scope lines. Change: N/A. New/changed-code coverage: `N/A - out of scope`. Disposition: N/A. Evidence: `N/A - out of scope` (zero TypeScript files changed on the branch).
- PowerShell: Baseline: N/A - out of scope lines -> Post-change: N/A - out of scope lines. Change: N/A. New/changed-code coverage: `N/A - out of scope`. Disposition: N/A. Evidence: `N/A - out of scope` (zero PowerShell files changed on the branch).
- Python: Baseline: N/A - out of scope lines -> Post-change: N/A - out of scope lines. Change: N/A. New/changed-code coverage: `N/A - out of scope`. Disposition: N/A. Evidence: `N/A - out of scope` (zero Python files changed on the branch).

**Repo-wide >= 80% note (C#):** The production-only testable-denominator rate remains below the 80% floor (71.65% baseline, increasing). This shortfall is the pre-existing, authority-scoped accepted exception 197-COV-001 established by merged #197; it is NOT introduced or worsened by this feature. The feature's spec Non-Goals explicitly state the floor is not reached in Increments 1-3. Because this feature only increases coverage (no regression) and the <80% condition predates it under an accepted exception, the C# coverage disposition for this feature is PASS-with-accepted-exception, not a feature-introduced FAIL. This is recorded in Section 8 as an Approved Exception. (Note: the raw Cobertura "overall line-rate" of `p5-coverage.cobertura.xml` is 0.127 and `final-fullsuite.cobertura.xml` is 0.191; these aggregate figures are polluted by instrumented third-party assemblies — Deedle, FluentAssertions, System.Linq.Async, log4net — and are NOT the first-party production-only denominator. The authoritative production-only figure is the 71.65% denominator per 197-COV-001.)

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | ✅ PASS | FluentAssertions `Should()` with `because` reason strings throughout (e.g., `.Should().Be("AppData", "the longest contained value wins the descending-length ordering")`). |
| **Arrange-Act-Assert Pattern** | ✅ PASS | Explicit `// Arrange` / `// Act` / `// Assert` comments in every Phase-5 test method. |
| **Document Intent** | ✅ PASS | Scenario-encoding method names plus class-level XML doc summaries documenting why the change-confirmation branch is excluded. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | ✅ PASS | No DB, network, API, process, or filesystem use. The `MatchBestSpecialFolder` tests use an in-memory `Dictionary<string,string>` (no `LoadFolders`, no `Directory.CreateDirectory`). The dialog tests inject a non-modal stub so no WinForms message loop runs. |
| **Use Mocks/Stubs** | ✅ PASS | Moq used for `IProjectEntry` (the `ComparandWithShiftingProjectId` helper produces a shifting `ProjectID` to reach the length tie-break). The `MyBox.DialogInvoker` delegate seam is set to a deterministic stub. |
| **Environment Stability** | ✅ PASS | No temp files created (diff scan: none). The static `MyBox.DialogInvoker` seam is restored in `[TestCleanup]`; no other mutable global state in the Phase-5 tests. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | ✅ PASS | This audit plus `evidence/qa-gates/p5-*` constitute the required review. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | ✅ PASS | Objective in `remediation-inputs.2026-06-14T15-10.md`: maintainer-authorized two seams to close the AC1/AC3 Flag-and-Stop gaps. |
| **Read existing change plans** | ✅ PASS | `plan.2026-06-14T08-22.md` (Phase 5 tasks P5-T1..P5-T12) and Phase 0 evidence record policy-order reading. |
| **Document the plan** | ✅ PASS | Phased atomic plan present; Phase 5 scope-change input recorded in remediation-inputs. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | ✅ PASS | The seam is the smallest that fits: a pure-helper extraction (separation of concerns, no DI framework) plus one assembly attribute. No broad refactor. |
| **Reusability** | ✅ PASS | The extracted `MatchBestSpecialFolder(IReadOnlyDictionary,string)` is a reusable pure helper; the instance method delegates to it. |
| **Extensibility** | ✅ PASS | The static helper accepts the collection as a parameter, decoupling matching logic from the filesystem-bound construction. |
| **Separation of concerns** | ✅ PASS | Pure matching logic separated from the I/O-bound `LoadFolders`; this is exactly the repo design principle (pure logic separated from I/O). |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | ✅ PASS | The helper lives alongside the instance method in the same cohesive class; test files mirror target class locations. |
| **Under 500 lines** | ✅ PASS | The two production files remain well under 500 lines (the change adds ~30 lines to `AppFileSystemFolderPaths.cs`, one line to `AssemblyInfo.cs`). All 14 new test files range 81-246 lines (largest: `ToDoLoaderSetAndSaveTests.cs` 246). No file pushed over 500 lines. |
| **Public vs internal** | ✅ PASS | The new helper is `internal static` (minimal surface); reached from `TaskMaster.Test` via existing `InternalsVisibleTo`. The `MyBox.DialogInvoker` seam is reached from `ToDoModel.Test` via the newly-added (authorized) `InternalsVisibleTo`. No public surface widened. |
| **No circular dependencies** | ✅ PASS | Test projects reference production projects only; `ToDoModel.Test` -> `UtilitiesCS` via the attribute is a one-directional test-visibility grant, not a runtime dependency. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | ✅ PASS | `MatchBestSpecialFolder` retained; parameter `specialFolders` descriptive. Test method names encode scenario + expected outcome. |
| **Docs/docstrings** | ✅ PASS | The static helper carries an XML `<summary>` and `<remarks>` documenting that behavior is byte-for-byte identical and that it is a pure seam for testability. |
| **Comment why, not what** | ✅ PASS | The instance method comment explains *why* it delegates (to enable unit testing without filesystem access); the dialog test class comment explains *why* the change-confirmation branch is excluded. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ✅ PASS | **Command:** `csharpier check .` (global 1.2.6).<br>**Result:** final pass EXIT_CODE 0, `Checked 1054 files`, no remaining diff (`evidence/qa-gates/p5-csharpier.2026-06-14T15-10.md`). The two production-seam files required no formatting changes. Reviewer note: `dotnet tool run csharpier` unavailable (absent repo-local SDK); global CSharpier used as a documented substitute per CLAUDE.md approved command list. |
| **2. Linting** | ✅ PASS | **Command:** `msbuild TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.<br>**Result:** EXIT_CODE 0, build succeeded, 60 pre-existing warnings in unrelated files, 0 analyzer errors; Phase-5 changes introduced no diagnostics (`evidence/qa-gates/p5-analyzers.2026-06-14T15-10.md`). |
| **3. Type checking** | ✅ PASS | **Command:** `msbuild TaskMaster.sln /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`.<br>**Result:** first-party 0 errors; solution-aggregate non-zero is solely the two vendored projects (`SVGControl`, `UtilitiesSwordfish.NET.General`) explicitly excluded from the analyzer stack. The two Phase-5 seams introduce no nullable warnings (`evidence/qa-gates/p5-nullable.2026-06-14T15-10.md`). |
| **4. Testing** | ✅ PASS | **Command:** `vstest.console.exe ToDoModel.Test\...\ToDoModel.Test.dll TaskMaster.Test\...\TaskMaster.Test.dll /InIsolation /EnableCodeCoverage /Settings:scripts\vscode\TaskMaster.cli.runsettings`.<br>**Result:** EXIT_CODE 0, 185/185 pass (`evidence/qa-gates/p5-mstest-coverage.2026-06-14T15-10.md`). |
| **Full toolchain loop** | ✅ PASS | Phase-5 gates all EXIT_CODE 0 (first-party); final pass clean. |
| **Explicit reporting** | ✅ PASS | Commands recorded in `evidence/qa-gates/p5-*` and the PR context summary. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | ✅ PASS | spec.md updated (Invariants and Non-Goals now reflect the two authorized seams); `evidence/qa-gates/p5-invariant-check` summarizes the production-boundary verification. |
| **Design choices explained** | ✅ PASS | The change-confirmation gap memo explains why a third seam was not added (flag-and-stop). |
| **Update supporting documents** | ✅ PASS | spec.md AC1/AC3 re-pointed to Phase 5; DoD "all target seams covered" left intentionally unchecked with explanation. |
| **Provide next steps** | ✅ PASS | The gap memo names the maintainer follow-up: route the `ProjectID` property setter through `MyBox`. |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3 (C#) — C# Code Change Policy Compliance

#### C# Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting — CSharpier** | ✅ PASS | `csharpier check .` clean on final pass (global 1.2.6 substitute documented). |
| **Linting — .NET analyzers** | ✅ PASS | msbuild analyzers + code style EXIT_CODE 0; no new diagnostics on the seams. |
| **Type checking — nullable** | ✅ PASS | First-party nullable clean; seam return type/nullability identical to the original instance method. |
| **Testing — MSTest/Moq/FluentAssertions** | ✅ PASS | vstest.console.exe with coverage, 185 pass. |

#### C# Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Strong contracts / explicit types** | ✅ PASS | The helper signature is explicit (`IReadOnlyDictionary<string,string>`, `string`); `var` only for the obvious LINQ local. |
| **Null-safety** | ✅ PASS | Nullable build clean; the helper preserves the original null/empty-collection guard and the documented null-path throw. |
| **Composition / focused types** | ✅ PASS | A focused pure helper, not a new abstraction framework; the seam follows the DI-seam preference order (a structural extraction, the lightest possible). |
| **Async/resource safety** | ✅ PASS | No async change; the seam is synchronous pure logic. |

#### C# Structure, Naming, Error Handling

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Files under 500 lines** | ✅ PASS | Both production files and all 14 test files under 500 (max test 246). |
| **PascalCase/camelCase conventions** | ✅ PASS | Helper PascalCase; parameter `specialFolders` camelCase; test methods PascalCase; locals camelCase. |
| **Exceptions explicit** | ✅ PASS | The null-path test asserts the specific `NullReferenceException` documented as preserved from the original method. |

---

## 4. Language-Specific Unit Test Policy Compliance

### Section 4 (C#) — C# Unit Test Policy Compliance

#### Framework and Scope

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use MSTest** | ✅ PASS | `[TestClass]`/`[TestMethod]` (and `[STATestClass]` for the WinForms-seam class), `Microsoft.VisualStudio.TestTools.UnitTesting` in all files. No xUnit/NUnit. |
| **Coverage expectation** | ✅ PASS | New-code 100% on the static helper; repo-wide handled under the accepted 197-COV-001 exception (see 1.2.1). |

#### Test Style and Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Focused unit tests** | ✅ PASS | Single-behavior methods. |
| **Mocking** | ✅ PASS | Moq for `IProjectEntry`; delegate stub for `MyBox.DialogInvoker`; in-memory dictionaries for the folder helper. |
| **Organization** | ✅ PASS | Test file paths mirror target class paths. |

#### Libraries / Assertions

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Moq for mocking** | ✅ PASS | `new Mock<IProjectEntry>()` in `ProjectEntryDialogBranchesTests`. |
| **FluentAssertions for assertions** | ✅ PASS | `Should()` style throughout; MSTest `Assert` not used in the Phase-5 files. |

#### Running the Toolchain

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use vstest.console.exe** | ✅ PASS | Per `evidence/qa-gates/p5-mstest-coverage`. |
| **No alternative runners** | ✅ PASS | Only MSTest via vstest. |

---

## 5. Test Coverage Detail

### AppFileSystemFolderPathsMatchBestSpecialFolderTests (9 tests)

| Test Name | Scenario Type | Status |
|-----------|--------------|--------|
| MatchBestSpecialFolder_PathContainsKnownValue_ReturnsThatKey | Positive | ✅ |
| MatchBestSpecialFolder_TwoCandidatesContained_LongerValueKeyWins | Edge (best-match) | ✅ |
| MatchBestSpecialFolder_CaseMismatch_DoesNotMatch_ReturnsNull | Edge (case) | ✅ |
| MatchBestSpecialFolder_ValueWithoutTrailingSeparator_StillMatchesAsSubstring | Edge (separator) | ✅ |
| MatchBestSpecialFolder_NoValueContained_ReturnsNull | Negative | ✅ |
| MatchBestSpecialFolder_NullCollection_ReturnsNull | Negative | ✅ |
| MatchBestSpecialFolder_EmptyCollection_ReturnsNull | Negative | ✅ |
| MatchBestSpecialFolder_EmptyPath_NoValueContained_ReturnsNull | Edge | ✅ |
| MatchBestSpecialFolder_NullPath_ThrowsNullReferenceException | Error | ✅ |

**Coverage:** new static helper body (lines 81-84, 86, 89-91) 8/8 = 100% (reviewer-parsed `p5-coverage.cobertura.xml`). **Not covered:** the instance-method delegation lines (58, 62-63) — unchanged from the pre-extraction deferred gap; unit tests call the static helper directly. No regression.

### ProjectEntryDialogBranchesTests (3 tests)

| Test Name | Scenario Type | Status |
|-----------|--------------|--------|
| SetProjectId_MalformedId_ShowsErrorDialogAndReturnsFalse | Negative/Error (malformed-ID branch) | ✅ |
| CompareTo_EqualOrdinalThenShorterOtherLength_ReturnsNegativeOne | Edge (length tie-break) | ✅ |
| CompareTo_EqualOrdinalThenLongerOtherLength_ReturnsPositiveOne | Edge (length tie-break) | ✅ |

**Coverage:** `ProjectEntry.CompareTo(IProjectEntry)` length tie-break covered both directions; `SetProjectId` malformed-ID branch covered via the injected `MyBox.DialogInvoker` stub. **Not covered:** `ProjectEntry.ChangeId` change-confirmation branch (0/28) — the documented Flag-and-Stop gap (raw un-seamed `MessageBox.Show` in the `ProjectID` property setter; requires a third unauthorized production seam). See Section 8.

### Earlier-increment tests carried on the branch

The Increments 1-3 test files (ToDoLoader, IDList, BaseChanger, ProjectEntry pure cases, Ka*/Kbd*/queues, AppStagingFilenames, AppQuickFilerSettings) remain on the branch and were validated in prior cycles; the prior policy-audit (`policy-audit.2026-06-14T14-30.md`) covers them in detail. This re-audit focuses on the Phase-5 delta but confirms the full branch toolchain (185-test final suite) is green.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Phase-5 final suite tests | 185 (ToDoModel.Test + TaskMaster.Test) | ✅ |
| Tests Passed | 185 (100%) | ✅ |
| Tests Failed | 0 | ✅ |
| Execution Time | 3.90 s | ✅ Fast |
| Phase-5 new test methods | 12 (9 MatchBestSpecialFolder + 3 ProjectEntry dialog branches) | ✅ |
| Largest new test file | 246 lines (ToDoLoaderSetAndSaveTests.cs) | ✅ Maintainable |
| New-code coverage (static helper) | 8/8 lines = 100% | ✅ |
| Production assembly line-rate (p5) | TaskMaster.AppFileSystemFolderPaths 0.608; ToDoModel.ProjectEntry 80/181 | ✅ Increased |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `csharpier check .` | no diff, EXIT_CODE 0 (final pass) | ✅ |
| .NET Analyzers | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT_CODE 0, 0 analyzer errors | ✅ |
| Nullable Type-Check | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | first-party 0 errors (aggregate non-zero = vendored projects only) | ✅ |
| MSTest Tests | `vstest.console.exe <2 test dlls> /InIsolation /EnableCodeCoverage /Settings:...` | 185 pass, EXIT_CODE 0 | ✅ |

**Notes:** `dotnet tool run csharpier` unavailable in the executor environment due to an absent repo-local SDK; the executor used global CSharpier 1.2.6 and documented the substitution. The reviewer did not re-run the toolchain; verdicts are based on the existing `p5-*` qa-gate evidence artifacts and direct Cobertura parsing.

---

## 8. Gaps and Exceptions

### Identified Gaps

- **ProjectEntry change-confirmation branch** (`SetProjectId` -> `ChangeId`, committing `ProjectID = newID`): not covered (0/28). The `ProjectID` property setter's `_projectID != value` arm calls a RAW, un-seamed `System.Windows.Forms.MessageBox.Show`, which is not routed through the `MyBox.DialogInvoker` seam. A test that drives the change-confirmation path to commit a changed id triggers a real modal dialog on the STA test thread with no message pump, deadlocking the test host (verified: the malformed-ID and CompareTo seam-only tests pass in ~553 ms while the change-confirmation path times out at EXIT 124). Covering it requires routing the property setter's dialogs through `MyBox` — a THIRD production change in a THIRD location, beyond the two seams the maintainer authorized for Phase 5. The executor correctly flagged-and-stopped per the plan Flag-and-Stop Rule and the Phase 5 hard constraint. Documented in `evidence/other/p5-projectentry-changeconfirm-gap.2026-06-14T15-10.md`.

**Reviewer assessment of the residual gap (explicit reasoning):** This gap is an **acceptable, maintainer-pending scope boundary, not a blocking finding**, for the following reasons:
1. **It is correct policy behavior.** The spec's flag-and-stop rule and `csharp.md` ("no silent production change") require exactly this: do not introduce a new production seam without maintainer authorization. The Phase 5 authorization (`remediation-inputs.2026-06-14T15-10.md`) explicitly scoped the production-change lift to two named seams and stated "Any production change beyond these is a flag-and-stop." Adding the third seam silently would itself be a policy violation.
2. **It is not a feature-introduced regression.** The change-confirmation branch was 0% covered before Phase 5 and remains 0%; no coverage was lost. The feature strictly increases coverage.
3. **The two authorized AC gaps were closed.** AC1's malformed-ID dialog branch and CompareTo length tie-break, and AC3's MatchBestSpecialFolder, are now covered. The residual is a single sub-branch of AC1 that the maintainer's authorization did not extend to.
4. **It is documented and routed for follow-up.** The DoD "all target seams covered" item is intentionally left unchecked, the gap memo names the precise follow-up (route the `ProjectID` setter through `MyBox`), and it requires separate maintainer direction. This is the transparent disposition the policy prescribes.

### Approved Exceptions

- **Repo-wide C# coverage < 80% (197-COV-001):** The production-only testable-denominator rate is 71.65% baseline (increasing post-feature), below the 80% floor. This is the pre-existing authority-scoped exception established by merged #197, not introduced or worsened by this feature. The feature increases coverage with no regression. Spec Non-Goals explicitly exclude reaching the floor in Increments 1-3. Approval source: #197 (merged), authority tag 197-COV-001.
- **CSharpier invocation substitute:** global CSharpier 1.2.6 instead of `dotnet tool run csharpier` due to absent repo-local SDK in the executor environment; same formatter, file-based, no project-file mutation. Documented in evidence.
- **Maintainer-authorized production seams (Phase 5):** the two named seams (`UtilitiesCS/Properties/AssemblyInfo.cs` attribute and `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` pure-helper extraction) are an explicit, documented lift of the spec's zero-production-change Non-Goal. Approval source: `remediation-inputs.2026-06-14T15-10.md`. Both preserve runtime behavior; verified by source inspection and the invariant-check evidence.

### Removed/Skipped Tests

- **None.** All planned reachable Phase-5 tests were implemented. The single unreachable change-confirmation path is Flag-and-Stopped, not removed.

---

## 9. Summary of Changes

### Files Modified (branch diff `d436a06f..aa3a7542`)

1. **2 production source files** (MODIFIED, authorized) — `UtilitiesCS/Properties/AssemblyInfo.cs` (one `InternalsVisibleTo("ToDoModel.Test")` attribute) and `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` (pure `MatchBestSpecialFolder` helper extraction; instance method delegates). No behavior change.
2. **14 new C# test files** (NEW) — across `ToDoModel.Test` (5), `QuickFiler.Test` (6), `TaskMaster.Test` (3); the Phase-5 additions are `ProjectEntryDialogBranchesTests.cs` and `AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs`.
3. **3 test csproj** (MODIFIED) — additive `<Compile Include>` registrations only. No production project touched.
4. **Feature docs/evidence + agent-memory** (NEW/MODIFIED) — spec, plan, issue, prior review artifacts, baseline/qa-gate/other evidence under the canonical feature folder; two atomic-executor memory files.

No `.props`, `.targets`, `coverage.config`, `*.runsettings`, `MyBox.cs`, or pipeline file changed.

---

## 10. Compliance Verdict

### Overall Status: ⚠️ PARTIALLY COMPLIANT (PASS-with-accepted-exception; no blocking findings)

The Phase-5 re-audit confirms the feature is policy-compliant for all directly-controllable requirements: production change limited to the two maintainer-authorized seams (verified — no third seam, no `MyBox.cs` edit, no config/pipeline change), no production behavior change, MSTest/Moq/FluentAssertions, AAA, deterministic, no temp files, no external dependencies, no live Outlook/WinForms (dialog seam stubbed), full toolchain green, new-code coverage 100% on the new static helper. The single below-floor coverage condition is the pre-existing accepted #197 exception, which this feature improves. The one uncovered targeted sub-branch (ProjectEntry change-confirmation) is a correctly-handled, maintainer-pending Flag-and-Stop. There are no blocking findings.

**Fail-closed reminder:** All required artifacts exist and were inspected; no missing evidence.

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- ✅ Before Making Changes: documented
- ✅ Design Principles: smallest seam, separation of concerns
- ✅ Module & File Structure: all files < 500 lines
- ✅ Naming, Docs, Comments: descriptive, why-focused
- ✅ Toolchain Execution: green (per p5 evidence)
- ✅ Summarize & Document: complete

#### Language-Specific Code Change Policy (Section 3)
**For C#:**
- ✅ Tooling & Baseline: csharpier/analyzers/nullable clean
- ✅ Design & Type-Safety: explicit, null-safe, lightest seam
- ✅ Error Handling: explicit exception assertion

#### General Unit Test Policy (Section 1)
- ✅ Core Principles
- ⚠️ Coverage & Scenarios: PASS with accepted 197-COV-001 repo-wide exception; new-code PASS
- ✅ Test Structure
- ✅ External Dependencies
- ✅ Policy Audit

#### Language-Specific Unit Test Policy (Section 4)
**For C#:**
- ✅ Framework & Scope
- ✅ Test Style & Structure
- ✅ Naming & Readability
- ✅ Toolchain

### Metrics Summary

- ✅ 185/185 Phase-5 final-suite tests passing (100%)
- ✅ 12 new Phase-5 test methods covering the two authorized seams
- ✅ New-code coverage 100% on the new static helper (8/8 lines)
- ✅ Production line-rate increased on the two authorized seams
- ✅ Production change limited to two authorized seams; no behavior change
- ⚠️ Repo-wide C# coverage 71.65%+ remains below 80% (accepted 197-COV-001 exception; not feature-introduced)
- ✅ All code quality checks passing (per evidence)

### Recommendation

**Ready for merge.** No blocking findings. The Phase-5 seams are limited to the two maintainer-authorized changes, behavior-preserving, and verified by source inspection and the invariant-check evidence. The single residual change-confirmation coverage gap is an acceptable, documented, maintainer-pending Flag-and-Stop (a third seam was correctly NOT added). The below-floor repo-wide rate is the accepted #197 exemption, which this feature improves. Follow-up (out of scope): route the `ProjectID` property setter through `MyBox` under separate maintainer direction; roadmap Increments 4+.

---

## Appendix A: Test Inventory

Phase-5 additions:
- AppFileSystemFolderPathsMatchBestSpecialFolderTests › 9 tests (positive, longest-match, case, trailing-separator, no-match, null-collection, empty-collection, empty-path, null-path-throws)
- ProjectEntryDialogBranchesTests › 3 tests (malformed-ID, CompareTo length tie-break shorter, CompareTo length tie-break longer)

Earlier-increment classes carried on the branch (validated in prior cycles): ToDoLoaderSetAndSaveTests, IDListGetNextToDoIDTests, BaseChangerRemainingBranchesTests, ProjectEntryTests, KaCharTests, KaKeyTests, KaStringAsyncTests, KbdActionsRemainingBranchesTests, FilerQueueTests, QfcQueuePurePathsTests, AppStagingFilenamesTests, AppQuickFilerSettingsRemainingPropertiesTests.

## Appendix B: Toolchain Commands Reference

**For C#:**
```powershell
# Formatting
csharpier check .

# Linting (analyzers + code style)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

# Type checking (nullable, warnings-as-errors)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true

# Testing with coverage (Phase 5 final suite)
vstest.console.exe ToDoModel.Test/bin/Debug/ToDoModel.Test.dll TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /EnableCodeCoverage /Settings:scripts/vscode/TaskMaster.cli.runsettings
```

**Reviewer coverage verification (no regeneration):**
```bash
python -c "parse artifacts/csharp/p5-coverage.cobertura.xml: package line-rates + per-line hits for TaskMaster.AppFileSystemFolderPaths lines 56-91"
```

**Reviewer production-scope verification:**
```bash
git diff --name-status d436a06f..aa3a7542 | grep -iE '\.cs$' | grep -viE '\.Test/|/Test/|^docs/'
# -> only: TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs, UtilitiesCS/Properties/AssemblyInfo.cs
```

---

**Audit Completed By:** feature-reviewer agent
**Audit Date:** 2026-06-14
**Policy Version:** Current (as of audit date)
