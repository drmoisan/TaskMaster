# Policy Compliance Audit: QfcItemController / IItemViewer Testability Refactor (Issue #227)

**Audit Date:** 2026-06-29
**Code Under Test:** C# only. 13 new production files, 3 modified production files, 1 modified
production project file, 6 new test files, 1 modified test project file.

Production (new): `QuickFiler/Controllers/QfcItemController.Initialization.cs`,
`QfcItemController.ViewerSetup.cs`, `QfcItemController.Conversation.cs`,
`QfcItemController.FolderHandling.cs`, `QfcItemController.EventWiring.cs`,
`QfcItemController.EventHandlers.cs`, `QfcItemController.Navigation.cs`,
`QfcItemController.FocusAndTheme.cs`, `QfcItemController.MailActions.cs`,
`QuickFiler/Viewers/ItemViewer.Commands.cs`, `ItemViewer.DisplayState.cs`,
`ItemViewer.FolderSearch.cs`, `ItemViewer.WebViewThread.cs`.
Production (modified): `QuickFiler/Controllers/QfcItemController.cs`,
`QuickFiler/Viewers/IItemViewer.cs`, `QuickFiler/QuickFiler.csproj`.
Tests (new): `QuickFiler.Test/Controllers/QfcItemController.ConversationTests.cs`,
`QfcItemController.EventWiringTests.cs`, `QfcItemController.FolderHandlingTests.cs`,
`QfcItemController.MailActionsTests.cs`, `QfcItemController.NavigationTests.cs`,
`QfcItemController.PropertiesTests.cs`. Tests (modified): `QuickFiler.Test/QuickFiler.Test.csproj`.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 19 changed (16 production, 3 test) | 233 tests | ✅ 233 pass, 0 fail | 7.54% lines (QfcItemController production, sequence-point basis) | 82.74% lines (affected testable non-exempt denominator, 484/585) | 82.74% aggregate extracted non-exempt (genuinely-new narrowing logic ≥90%) |
| TypeScript | 0 files | N/A | N/A | N/A (no changed files) | N/A (no changed files) | N/A |
| PowerShell | 0 files | N/A | N/A | N/A (no changed files) | N/A (no changed files) | N/A |
| Python | 0 files | N/A | N/A | N/A (no changed files) | N/A (no changed files) | N/A |

**Note:** This change is C#-only. TS/PS/Python rows are retained per template structure and
marked N/A because those languages have zero changed files in the branch diff.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- C# baseline coverage artifact: `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/baseline/baseline-tests-coverage.2026-06-29T10-52.md`
- C# post-change coverage artifact: `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/regression-testing/coverage-delta.2026-06-29T12-50.md`
- Canonical C# coverage XML (`artifacts/csharp/coverage.xml`): **ABSENT** — see Section 8 (Gaps) and the Coverage Verification finding below.
- Per-language comparison summary: Section 1.2.1 below.

**Non-negotiable verdict rule:** This audit does NOT report an unconditional PASS. The overall
verdict is PARTIALLY COMPLIANT pending two gating items (Section 10).

---

## Executive Summary

Issue #227 is a C# testability refactor of `QfcItemController` (a ~2,498-line controller) and its
view interface `IItemViewer`, applying the issue #223 strategy. The 2,498-line monolith is split
into a 294-line main partial plus nine responsibility-scoped partial files (verbatim moves);
`IItemViewer` is narrowed from raw WinForms control types to intent-level display-state properties,
command events, and intent methods; the concrete-`ItemViewer` field-type wall that blocked
`Mock<IItemViewer>` injection is removed; four `ItemViewer.*.cs` forwarding partials implement the
narrowed members; and six per-cluster test files add 32 new tests. The four-step C# toolchain
(csharpier, .NET analyzers, nullable/TreatWarningsAsErrors, MSTest with coverage) reports
EXIT_CODE 0 in order at the final gate (233/233 tests pass).

This review verified scope against the actual branch diff. The PR-context summary classified all
changes as "Docs/templates/agents/tooling: 59 files; Core logic changes: 0 files." That overview
is incorrect: the branch contains 19 changed C# files (16 production, 3 test). The audit proceeds
against the real C# scope.

Adjudicated outcomes (independent of the executor self-assessment):
- AC1–AC4, AC6, AC7: PASS (independently verified).
- AC5: PASS-with-documented-exception on the testable-denominator floor and no-regression
  sub-claims; the ≥90% new/extracted sub-target is recorded as deferred to #197; the 103-method
  exemption boundary awaits maintainer ratification. AC5 is therefore PARTIAL pending that
  ratification, not a code defect.
- Coverage Verification: the workflow-mandated canonical artifact `artifacts/csharp/coverage.xml`
  is absent (same gap resolved in #223 remediation cycle 1). FAIL on artifact presence;
  substantive coverage was independently verifiable from the executor evidence files.

**Policy documents evaluated:**
- ✅ `CLAUDE.md` (all sections), `.claude/rules/general-code-change.md`
- ✅ `.claude/rules/general-unit-test.md`

**Language-specific policies evaluated:**
- ✅ C#: `.claude/rules/csharp.md` + CLAUDE.md C# Code Change Policy + C# Unit Test Policy
- N/A Python / PowerShell / Bash / JSON (zero changed files)

**Temporary artifacts cleanup:**
- ✅ No temporary scripts introduced into the tree by this branch (evidence files are Markdown under the feature folder).
- ✅ Evidence files are committed under the canonical `<FEATURE>/evidence/<kind>/` paths.

---

## Rejected Scope Narrowing

None supplied. The orchestrator prompt explicitly stated no scope narrowing was supplied and
directed a full feature-vs-base audit. The audit scope is the full branch diff
`4611fd60b7d1a782a8024f54cbfd4d28f6d4c264..bcc7d7e32a12693b732d5c5e133a681890bec412` against
base `main`. No caller instruction attempted to narrow scope to a plan/task/phase, to a file
subset, or to mark C# coverage as out-of-scope.

---

## PR-Context Summary Misclassification (recorded)

The PR-context summary overview reports `Core logic changes: 0 files` and
`Docs/templates/agents/tooling: 59 files`, classifying the C# production/test changes as docs.
`git diff --name-status` against the merge base shows 19 changed C# files (16 production, 3 test).
The audit uses the git diff as the authoritative scope source, not the summary overview.

---

## Evidence Location Compliance

Scanned the branch diff for evidence written under non-canonical paths
(`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`).
**None found.** All execution evidence is under
`docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/<kind>/`
(baseline, qa-gates, regression-testing, other), which is the canonical location per
`evidence-and-timestamp-conventions`. Disposition: **PASS**. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED`
entries required.

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | ✅ PASS | Six per-cluster MSTest classes; each test arranges its own `Mock<IItemViewer>` / controller instance; no shared mutable static state observed in the new test files. |
| **Isolation** - Each test targets single behavior | ✅ PASS | Tests are grouped by cluster (Conversation, FolderHandling, EventWiring, Navigation, MailActions, Properties); each `[TestMethod]` exercises one seam (e.g., folder-selection routing, INotifyPropertyChanged raise). |
| **Fast Execution** - Tests complete quickly | ✅ PASS | 233 tests run under the single `vstest.console.exe` invocation at the final gate (EXIT_CODE 0); pure-logic seams via Moq, no live Outlook. |
| **Determinism** - Consistent results | ✅ PASS | No temp files; Moq event raising (`Raise`), `VerifySet`/`Verify`, reflection `_kbdHandler` injection, virtual-seam subclass for `DoLoadConversationResolverCoreAsync`. No clock/network dependence. |
| **Readability & Maintainability** - Clear structure | ✅ PASS | Test files mirror the production partial structure; descriptive `[TestMethod]` names; AAA structure. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | ✅ PASS | **Baseline:** 7.54% lines QfcItemController production (246/3261, sequence-point basis); repo-wide first-party testable denominator 73.35%–74.11% (#223-measured). **Command:** `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`. **Timestamp:** 2026-06-29T10-52. |
| **No Coverage Regression** | ✅ PASS | **Post-change:** affected testable non-exempt denominator 82.74% (484/585). **Change:** strictly additive (Properties 29.23%→95.38%; FolderHandling 40.68%→88.14%; Conversation routing newly covered). **Status:** No previously-covered line became uncovered. Evidence: `evidence/regression-testing/coverage-delta.2026-06-29T12-50.md`. |
| **New Code Coverage ≥90%** | ⚠️ PARTIAL | **New/extracted aggregate non-exempt:** 82.74% (< 90%). The genuinely-new narrowing logic is ≥90% per the executor evidence; the aggregate is held below 90% by verbatim-relocated, structurally un-coverable code (EventWiring inline async-registration lambda bodies ~56 lines; Dispatcher-bound Conversation render deferred to #197; `GetItemSummary` 2 COM lines). See AC5 adjudication in Section 8. |
| **Comprehensive Coverage** | ✅ PASS | New tests cover folder-selection routing, conversation enumerate/collapse, INotifyPropertyChanged, registration structure, navigation/keyboard routing, mail-action packaging. Untested testable residual (`GetItemSummary`, inline registration lambda bodies) documented in `exemption-boundary.2026-06-29T12-40.md`. |
| **Positive Flows** - Valid inputs | ✅ PASS | Folder-selection with seeded `FolderPredictor`; conversation render with non-null resolver; property round-trips. |
| **Negative Flows** - Invalid inputs | ✅ PASS | Per spec test strategy: empty/all-missing folder arrays; null `ConversationResolver`; "Trash to Delete" present/absent. |
| **Edge Cases** - Boundary conditions | ✅ PASS | Cancellation vs non-cancellation in `LoadConversationResolverAsync`; predetermined-vs-index-1 folder selection. |
| **Error Handling** - Error paths | ✅ PASS | `LoadConversationResolverAsync` catch blocks (cancellation rethrow; non-cancel fault swallow) tested via the virtual seam. |
| **Concurrency** | N/A | Structural/testability refactor; async dispatch paths are exempt or deferred (`UiThread.Dispatcher` to #197). |
| **State Transitions** | ✅ PASS | Expansion/focus toggles and conversation-mode checkbox state covered where testable; UI-thread-marshaled transitions are exempt. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 7.54% lines -> Post-change: 82.74% lines. Change: +75.20% lines (affected testable non-exempt denominator basis, 484/585). New/changed-code coverage: 82.74%. Disposition: FAIL. Evidence: `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/regression-testing/coverage-delta.2026-06-29T12-50.md`, `evidence/qa-gates/p8-coverage-gap.2026-06-29T12-40.md`, `evidence/other/exemption-boundary.2026-06-29T12-40.md`.
- TypeScript: Baseline: N/A - out of scope. Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope.
- PowerShell: Baseline: N/A - out of scope. Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope.

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | ✅ PASS | FluentAssertions assertions per CUT2; descriptive failure messages. |
| **Arrange-Act-Assert Pattern** | ✅ PASS | New per-cluster test files follow AAA; Moq arrange, intent-method act, `Verify`/`Should` assert. |
| **Document Intent** | ✅ PASS | Descriptive `[TestMethod]` names mirroring the cluster behavior under test. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | ✅ PASS | No live Outlook, network, or DB; `Mock<IItemViewer>` and seams substitute COM/WinForms boundaries. |
| **Use Mocks/Stubs** | ✅ PASS | Moq `Mock<IItemViewer>`; reflection `_kbdHandler` injection; virtual-seam subclass for COM `ConversationResolver.LoadAsync`. |
| **Environment Stability** | ✅ PASS | No temporary files (UT4 prohibition honored); deterministic; no mutable global config. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | ✅ PASS | This artifact plus `code-review.2026-06-29T13-15.md` and `feature-audit.2026-06-29T13-15.md` constitute the required review. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | ✅ PASS | Issue #227 + spec.md state the testability-refactor objective and invariants. |
| **Read existing change plans** | ✅ PASS | `plan.2026-06-29T10-15.md` (atomic plan, Phases 0–9); `phase0-instructions-read.md` records the policy-order read. |
| **Document the plan** | ✅ PASS | Atomic plan present; per-phase qa-gates evidence p1–p8 plus final gates. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | ✅ PASS | Verbatim partial-class split; narrowing replaces raw control exposure with intent members; no new abstraction framework. |
| **Reusability** | ✅ PASS | `PopulateAndSelectFolder` retained as a static pure seam; intent methods (`GetFolderItems`, `GetSelectedFolder`) reusable by tests. |
| **Extensibility** | ✅ PASS | `IItemViewer` is now an intent-level contract; `Mock<IItemViewer>` injectable; future seams extend the interface without raw-control coupling. |
| **Separation of concerns** | ✅ PASS | Pure routing/selection logic separated from UI-thread-marshaled and COM-bound code (the latter carries method-level exemptions). |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | ✅ PASS | Each partial scoped to one responsibility (Initialization, ViewerSetup, Conversation, FolderHandling, EventWiring, EventHandlers, Navigation, FocusAndTheme, MailActions). |
| **Under 500 lines** | ✅ PASS | All 22 changed production/test files < 500. Largest: `QfcItemController.Initialization.cs` 398. Independently verified via `awk 'END{print NR}'`. See Section 6 / AC6. |
| **Public vs internal** | ✅ PASS | Cluster methods declared `internal`; intent surface declared on `IItemViewer`; concrete control fields remain private on `ItemViewer`. |
| **No circular dependencies** | ✅ PASS | Partial-class members of one type; no new cross-project dependency introduced (nullable/analyzer build passes). |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | ✅ PASS | Intent members named by behavior (`DeleteItemClicked`, `SetFolderItems`, `ConversationModeChecked`). |
| **Docs/docstrings** | ✅ PASS | XML doc and rationale comments in `IItemViewer.cs` and forwarding partials explain the seam intent. |
| **Comment why, not what** | ✅ PASS | Comments explain the narrowing rationale and the concrete-bound (P2-T4) seam boundary. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ✅ PASS | **Command:** `dotnet tool run csharpier check .` **Result:** EXIT_CODE 0 (`final-csharpier.2026-06-29T12-50.md`). |
| **2. Linting** | ✅ PASS | **Command:** `MSBuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` **Result:** EXIT_CODE 0 (`final-analyzers.2026-06-29T12-50.md`). |
| **3. Type checking** | ✅ PASS | **Command:** `MSBuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` **Result:** EXIT_CODE 0 (`final-nullable.2026-06-29T12-50.md`). |
| **4. Testing** | ✅ PASS | **Command:** `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation` **Result:** 233/233 pass, EXIT_CODE 0 (`final-tests-coverage.2026-06-29T12-50.md`). |
| **Full toolchain loop** | ✅ PASS | Per-phase gates p1–p8 each report all four steps EXIT_CODE 0; final gate at 12-50 confirms a clean single pass. |
| **Explicit reporting** | ✅ PASS | Commands and exit codes recorded in the qa-gates evidence files. |

**Reviewer note:** Toolchain results are verified from the executor evidence artifacts (the
workflow directs inspection of existing artifacts rather than re-running). msbuild/vstest were not
re-executed in this review environment.

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | ✅ PASS | spec.md, plan.2026-06-29T10-15.md, ac-traceability.2026-06-29T12-50.md. |
| **Design choices explained** | ✅ PASS | Concrete-(ItemViewer) cast seam vs full `SetupThemes` narrowing documented in the exemption boundary and spec risk section. |
| **Update supporting documents** | ✅ PASS | Spec AC checkboxes, traceability, and coverage-delta updated. |
| **Provide next steps** | ✅ PASS | Residual 90% uplift folded into #197; exemption-boundary ratification flagged for review. |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3E: C# Code Change Policy Compliance

#### 3E.1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with CSharpier** | ✅ PASS | `dotnet tool run csharpier check .` EXIT_CODE 0 (final + p1–p8). |
| **Linting with .NET Analyzers** | ✅ PASS | `MSBuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` EXIT_CODE 0. |
| **Type checking with Nullable** | ✅ PASS | `MSBuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` EXIT_CODE 0. |
| **Testing with MSTest** | ✅ PASS | `vstest.console.exe ... /EnableCodeCoverage` 233/233 pass. |

#### 3E.2 C# Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Strong contracts / explicit APIs** | ✅ PASS | `IItemViewer` narrowed to typed intent members; raw `ButtonSVG`/`ComboBox`/`WebView2`/`FastObjectListView`/`OLVColumn`/`TableLayoutPanel`/`ToolStripMenuItemCb` removed. |
| **Null-safety by default** | ✅ PASS | Nullable build with TreatWarningsAsErrors passes; no new nullable warnings. |
| **Composition / focused types** | ✅ PASS | Partial-class decomposition; forwarding partials on `ItemViewer`. |
| **Async / resource safety** | ✅ PASS | Async seams preserved verbatim; analyzer (AsyncFixer in stack) build clean. |

#### 3E.3 C# Error Handling, Logging, Contracts

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Exceptions / fail fast** | ✅ PASS | `LoadConversationResolverAsync` cancellation rethrow preserved and tested. |
| **Logging** | ✅ PASS | No ad-hoc console output introduced; behavior preserved. |
| **Contracts / invariants** | ✅ PASS | Forwarding members round-trip underlying control state; constructor seams preserved. |

### Python / PowerShell / Bash / JSON

N/A — zero changed files for these languages in the branch diff.

---

## 4. Language-Specific Unit Test Policy Compliance

### Section 4C: C# Unit Test Policy Compliance

#### 4C.1 Framework Selection

| Requirement | Status | Evidence |
|------------|--------|----------|
| **MSTest** | ✅ PASS | `[TestClass]`/`[TestMethod]` per `.claude/rules/csharp.md` and CUT1. |
| **Moq for mocking** | ✅ PASS | `Mock<IItemViewer>` event raising and `Verify`/`VerifySet`. |
| **FluentAssertions** | ✅ PASS | FluentAssertions used for new assertions per CUT2. |

#### 4C.2 Coverage Expectation

| Requirement | Status | Evidence |
|------------|--------|----------|
| **New code ≥90%** | ⚠️ PARTIAL | Aggregate extracted non-exempt 82.74% < 90%; genuinely-new narrowing logic ≥90%; residual deferred to #197. See Section 8. |
| **Repo-wide ≥80%** | ⚠️ EXCEPTION | Repo-wide first-party testable denominator 73.35%–74.11% (pre-existing), dispositioned under the #223 maintainer-ratified authority-scoped exception; residual tracked under #197. |
| **No changed-line regression** | ✅ PASS | Strictly additive; no previously-covered line uncovered. |

#### 4C.3 Test Style and Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Focused unit tests** | ✅ PASS | One behavior per `[TestMethod]`. |
| **Organization mirrors code** | ✅ PASS | Six test files mirror the production partial clusters; explicit `<Compile Include>` entries in `QuickFiler.Test.csproj`. |
| **No external dependencies** | ✅ PASS | Moq/seams only; no temp files. |

---

## 5. Test Coverage Detail

### Affected testable non-exempt denominator (per-cluster, AC5 gate metric)

| Cluster file | non-exempt covered/total | % | Status |
|---|---|---|---|
| QfcItemController.cs (Properties/INotify) | 124/130 | 95.38% | ✅ |
| QfcItemController.Conversation.cs | 70/100 | 70.00% | ⚠️ below 80% (Dispatcher-bound render deferred to #197) |
| QfcItemController.EventWiring.cs | 186/242 | 76.86% | ⚠️ below 80% (inline async-registration lambda bodies) |
| QfcItemController.FolderHandling.cs | 52/59 | 88.14% | ✅ |
| QfcItemController.MailActions.cs | 24/24 | 100.00% | ✅ |
| QfcItemController.Navigation.cs | 28/28 | 100.00% | ✅ |
| QfcItemController.ViewerSetup.cs | 0/2 | 0.00% | ⚠️ GetItemSummary (COM read, not exempted) |
| **AGGREGATE** | **484/585** | **82.74%** | ✅ ≥80% floor MET |

**Coverage:** affected testable non-exempt denominator 82.74% (484/585). Source:
`evidence/regression-testing/coverage-delta.2026-06-29T12-50.md`.

**Not covered (testable, non-exempt residual):** EventWiring inline async-registration lambda
bodies (~56 lines, un-exemptable inline closures executable only on a live key-press);
`PopulateConversationAsync` non-null render path via `UiThread.Dispatcher` (injectable-Dispatcher
seam deferred to #197; best-case ~86.8% even with the seam); `ViewerSetup.GetItemSummary` (2 COM
lines). All three are structurally un-coverable without the deferred seam or a live host.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 233 | ✅ |
| Tests Passed | 233 (100%) | ✅ |
| Tests Failed | 0 | ✅ |
| Baseline tests preserved | 201 | ✅ |
| New tests added | 32 | ✅ |
| Affected testable non-exempt coverage | 82.74% (484/585) | ✅ ≥80% |
| Largest changed file size | 398 lines (Initialization.cs) | ✅ < 500 |
| Code Coverage (aggregate new/extracted) | 82.74% | ⚠️ < 90% (deferred to #197) |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier | `dotnet tool run csharpier check .` | EXIT_CODE 0 | ✅ |
| Analyzers | `MSBuild TaskMaster.sln /t:Build /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT_CODE 0 | ✅ |
| Nullable | `MSBuild TaskMaster.sln /t:Build /p:Nullable=enable /p:TreatWarningsAsErrors=true` | EXIT_CODE 0 | ✅ |
| MSTest | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage` | 233/233 pass | ✅ |

**For PowerShell:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| Invoke-Formatter | `Invoke-PoshQCFormat -Root .` | N/A | N/A |
| PSScriptAnalyzer | `Invoke-PoshQCAnalyze -Root .` | N/A | N/A |
| Pester Tests | `Invoke-PoshQCTest -Root .` | N/A | N/A |

**For Python:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| Black Formatting | `poetry run black .` | N/A | N/A |
| Ruff Linting | `poetry run ruff check` | N/A | N/A |
| Pyright Type Checking | `poetry run pyright` | N/A | N/A |
| Pytest Tests | `poetry run pytest` | N/A | N/A |

**Notes:** PowerShell and Python rows are N/A (zero changed files). C# is the only changed language.

---

## 8. Gaps and Exceptions

### Identified Gaps

- **Canonical C# coverage artifact absent.** The workflow mandates `artifacts/csharp/coverage.xml`
  for any language with changed files. It is not present in the tree. Coverage was instead recorded
  in `evidence/regression-testing/coverage-delta.2026-06-29T12-50.md` and the qa-gates files, which
  this review used to verify coverage substantively. Disposition: **FAIL on artifact presence**;
  remediation = generate the canonical Cobertura XML via the documented #223 cycle-1 procedure.
- **≥90% new/extracted sub-target (AC5).** Aggregate extracted non-exempt coverage 82.74% < 90%.
  Adjudication: the denominator used by the executor (the entire 585-line extracted non-exempt
  aggregate) over-applies the "new code" rule to verbatim-relocated pre-existing methods. The
  genuinely-new narrowing logic (intent members, forwarding, new seams) is ≥90% per the evidence.
  The aggregate is held below 90% by structurally un-coverable code (EventWiring inline
  async-registration lambda bodies; Dispatcher-bound Conversation render deferred to #197;
  `GetItemSummary`). Disposition: **deferred to #197**, consistent with the spec Non-Goal on the
  injectable `Dispatcher`. Not a code defect.

### Approved Exceptions

- **Repo-wide ≥80% floor.** The repo-wide first-party testable denominator (73.35%–74.11%) is
  below the floor. This is a pre-existing condition not introduced by this change (the refactor
  only adds tests and moves COM/WinForms code under `[ExcludeFromCodeCoverage]`; it cannot lower
  first-party coverage). Dispositioned under the maintainer-ratified authority-scoped exception
  precedent `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/maintainer-decision.2026-06-29.md`;
  residual uplift tracked under #197.
- **103 method-level `[ExcludeFromCodeCoverage]` applications.** Verified: the source count is
  exactly 103 (Conversation 7, EventHandlers 18, EventWiring 6, FocusAndTheme 18, FolderHandling 4,
  Initialization 12, MailActions 9, Navigation 20, ViewerSetup 9), matching
  `exemption-boundary.2026-06-29T12-40.md`. Spot-check confirmed every named testable seam is NOT
  exempted (`PopulateAndSelectFolder`, `AssignFolderComboBox`, `PackageItems`, `GetItemSummary`,
  `TopFolderScore`, `NotifyPropertyChanged`, `KbdExecuteAsync`, `RegisterFocusAsyncActions`,
  `RegisterExpandedAsyncActions`, `LoadConversationResolverAsync`, `MarkItemForDeletion`).
  Sampled exempted members (EventHandlers `async void` UI click handlers) are genuinely COM/WinForms
  bound. No over-broad or coverage-inflating exemption detected. **This exemption boundary awaits
  maintainer ratification** (governance action; gates the AC5 checkoff).

### Removed/Skipped Tests

**None.** Baseline 201 tests preserved; 32 added. No test weakened or removed.

---

## 9. Summary of Changes

### Files Modified

1. **QuickFiler/Controllers/QfcItemController.cs** (MODIFIED) — reduced from ~2,498 to 294 lines; retains fields, properties, INotifyPropertyChanged; `partial` added.
2. **QfcItemController.{Initialization,ViewerSetup,Conversation,FolderHandling,EventWiring,EventHandlers,Navigation,FocusAndTheme,MailActions}.cs** (NEW) — verbatim cluster moves, each < 500 lines.
3. **QuickFiler/Viewers/IItemViewer.cs** (MODIFIED) — narrowed from raw WinForms control types to intent-level display-state properties, command events, and intent methods.
4. **QuickFiler/Viewers/ItemViewer.{Commands,DisplayState,FolderSearch,WebViewThread}.cs** (NEW) — forwarding implementations of the narrowed interface; `ItemViewer` remains `[ExcludeFromCodeCoverage]`.
5. **QuickFiler/QuickFiler.csproj** (MODIFIED) — explicit `<Compile Include>` entries for the new partials.
6. **QuickFiler.Test/Controllers/QfcItemController.{Conversation,EventWiring,FolderHandling,MailActions,Navigation,Properties}Tests.cs** (NEW) — 32 new tests mirroring the cluster structure.
7. **QuickFiler.Test/QuickFiler.Test.csproj** (MODIFIED) — explicit `<Compile Include>` entries for the new test files.

---

## 10. Compliance Verdict

### Overall Status: ⚠️ PARTIALLY COMPLIANT

The implementation quality is sound: clean verbatim split, correct interface narrowing, honest
exemption boundary, no over-broad exemptions, no behavior-change risk identified, and a green
four-step toolchain. Two items prevent an unconditional PASS:

1. **Canonical C# coverage artifact absent** (`artifacts/csharp/coverage.xml`) — workflow-mandated;
   FAIL on presence. Remediation: generate it via the #223 cycle-1 procedure.
2. **Maintainer ratification of the 103-method exemption boundary** is required before AC5 can be
   checked off (spec AC5 explicitly conditions on it). Governance action, not a code fix.

The ≥90% new/extracted residual is deferred to #197 per the spec Non-Goal and is not a blocker for
this cycle.

**Fail-closed reminder honored:** this audit does not report PASS/ready-for-merge because the
canonical coverage artifact is missing and the exemption-boundary ratification is outstanding.

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- ✅ Before Making Changes; ✅ Design Principles; ✅ Module & File Structure (all < 500);
  ✅ Naming/Docs/Comments; ✅ Toolchain Execution; ✅ Summarize & Document.

#### Language-Specific Code Change Policy (Section 3)
**For C#:** ✅ Tooling & Baseline; ✅ Design & Type-Safety; ✅ Error Handling.

#### General Unit Test Policy (Section 1)
- ✅ Core Principles; ⚠️ Coverage & Scenarios (≥90% new-code sub-target deferred to #197);
  ✅ Test Structure; ✅ External Dependencies; ✅ Policy Audit.

#### Language-Specific Unit Test Policy (Section 4)
**For C#:** ✅ Framework & Scope; ⚠️ Coverage (90% sub-target deferred; repo-wide under #223 exception);
✅ Test Style & Structure.

### Metrics Summary

- ✅ 233/233 tests passing (100%)
- ✅ Affected testable non-exempt coverage 82.74% (≥80% floor MET)
- ⚠️ Aggregate new/extracted coverage 82.74% (< 90% target; deferred to #197)
- ⚠️ Repo-wide first-party 73.35%–74.11% (under #223 authority-scoped exception; #197)
- ✅ All 22 changed production/test files < 500 lines
- ✅ Four-step C# toolchain green in order
- ❌ Canonical `artifacts/csharp/coverage.xml` absent

### Recommendation

**Conditional Go.** The change is technically mergeable once (1) the canonical
`artifacts/csharp/coverage.xml` is generated and (2) the maintainer ratifies the 103-method
exemption boundary. The ≥90% new/extracted residual is deferred to #197 and is not a blocker.
See `remediation-inputs.2026-06-29T13-15.md`.

---

## Appendix A: Test Inventory

New per-cluster MSTest classes (32 new `[TestMethod]` across six files; 201 baseline tests preserved):

- `QfcItemController.ConversationTests` — conversation enumerate/collapse, resolver load, render-count routing
- `QfcItemController.FolderHandlingTests` — folder selection routing (predetermined vs index-1), `PopulateAndSelectFolder`
- `QfcItemController.EventWiringTests` — registration structure (`Register*AsyncActions`), `KbdExecuteAsync`
- `QfcItemController.NavigationTests` — `PackageItems`, navigation/keyboard routing
- `QfcItemController.MailActionsTests` — `MarkItemForDeletion`, packaging
- `QfcItemController.PropertiesTests` — properties, `NotifyPropertyChanged`, `TopFolderScore`

Full enumeration available in the executor evidence (`p7-tests-coverage`, `final-tests-coverage`).

---

## Appendix B: Toolchain Commands Reference

**For C#:**
```
dotnet tool run csharpier check .
MSBuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
MSBuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation
```

**Review verification commands (this audit):**
```
git diff --name-status 4611fd60..bcc7d7e3
awk 'END{print NR}' <each changed .cs file>          # 500-line cap verification
grep -c 'ExcludeFromCodeCoverage' QfcItemController.*.cs   # 103-exemption count
grep -B3 '<seam method>' QfcItemController.*.cs       # testable-seam non-exemption spot-check
```

---

**Audit Completed By:** feature-reviewer agent
**Audit Date:** 2026-06-29
**Policy Version:** Current (as of audit date)
