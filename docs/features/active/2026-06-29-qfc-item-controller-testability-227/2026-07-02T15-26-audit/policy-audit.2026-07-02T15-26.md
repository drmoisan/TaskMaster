# Policy Compliance Audit: QfcItemController Testability — Cycle-3 Targeted Residual Reduction (#227)

**Audit Date:** 2026-07-02
**Code Under Test:** C# only. New: `UtilitiesCS/Threading/IUiDispatcher.cs`, `WpfUiDispatcher.cs` (carried
from cycle-2, unchanged); `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs`;
`UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs`,
`FolderPredictor.IFolderSearchHandler.cs`; `UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.DispatcherTests.cs`.
Modified: the 10 `QuickFiler/Controllers/QfcItemController*.cs` partials (Initialization,
ViewerSetup, FolderHandling, EventWiring, EventHandlers, Navigation, FocusAndTheme, MailActions —
seams for `FolderPredictor` factory-delegates and `_uiDispatcher` de-exemptions);
`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs` (new `IUiDispatcher` constructor parameter,
retrofitted `SetQfcThemeAsync`/`SetQfcTheme(async:true)`/`SetMailRead(async:true)` call sites, render
body extracted to `Theme.Rendering.cs`); `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` (added
`partial`, no other change); `QuickFiler/Helper Classes/QfcThemeHelper.cs` (new `uiDispatcher`
parameter, four call-site arguments); `QuickFiler/Interfaces/MailItemActionsAdapter.cs` (removed
redundant `[ExcludeFromCodeCoverage]`); ~9 per-cluster test files; `QuickFiler.csproj`,
`QuickFiler.Test.csproj`, `UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj` (`<Compile Include>` entries).

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | ~14 production + ~10 test (cycle-3 delta) | 347 (QuickFiler.Test) + 4093 (UtilitiesCS.Test) = 4440 | ✅ 4440 pass, 0 fail | Affected `QfcItemController` non-exempt denom 73.59% (989/32/323 of 1344, P0-T5); repo-wide QuickFiler.dll 45.69%, UtilitiesCS.dll 85.62% | Affected non-exempt denom **77.40%** (1243/33/330 of 1606); repo-wide QuickFiler.dll 47.69%, UtilitiesCS.dll 85.86% | New/extracted cycle-3 production surface **100%** (all new/changed executable lines have direct or transitive test coverage; see §1.2.1) |

**Note:** C# is the only language in scope. `git diff --name-status 4611fd60..HEAD` (working tree,
including uncommitted cycle-3 changes) contains zero Python, PowerShell, Bash, TypeScript, or JSON
files, so those coverage categories are `N/A - out of scope` (zero changed files), not narrowed by any
caller instruction.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/remediation-baseline/baseline-tests-coverage.2026-07-02T11-15.md`
- C# post-change coverage artifact: `evidence/qa-gates/final-tests-coverage.2026-07-02T15-12.md`
- TypeScript baseline coverage artifact: `N/A - out of scope` (zero changed TypeScript files)
- TypeScript post-change coverage artifact: `N/A - out of scope` (zero changed TypeScript files)
- PowerShell baseline coverage artifact: `N/A - out of scope` (zero changed PowerShell files)
- PowerShell post-change coverage artifact: `N/A - out of scope` (zero changed PowerShell files)
- Per-language comparison summary: `evidence/regression-testing/coverage-delta.2026-07-02T15-14.md` and §1.2.1 below.

**Verdict rule note:** Numeric baseline and post-change coverage are present for the only in-scope
language (C#), so the PASS-eligibility precondition is satisfied.

---

## Rejected Scope Narrowing

No caller/orchestrator prompt attempted to narrow the audit scope to a plan, task, phase, or file
subset, and none attempted to mark any language's coverage as out of scope or informational-only. The
delegation prompt explicitly reiterated the full-branch-vs-base scope invariant and asked for an
independent reduction-honesty check rather than trusting the delivered claims. **Nothing to reject.**

---

## Evidence Location Compliance

Scanned the full working-tree change set (`git status --short`, `git diff --name-status 4611fd60`) for
files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`.
**None exist.** All cycle-3 audit-trail evidence is under the canonical
`docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/<kind>/` scheme
(`remediation-baseline/`, `qa-gates/`, `regression-testing/`, `other/`). The coverage artifacts under
`artifacts/csharp/` (`coverage.xml`, `coverage-r2-*.cobertura.xml`) are coverage-tooling outputs (the
canonical location for `coverage.xml`), not evidence artifacts, so they are not a location violation.
`validate_evidence_locations.py` was not found in this checkout (consistent with cycle-2's finding);
the PreToolUse hook `.claude/hooks/enforce-evidence-locations.ps1` is present. **No FAIL-level
evidence-location findings.**

No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` was required (no non-canonical evidence path was supplied in
the delegation).

---

## Executive Summary

Cycle 3 executes the maintainer-authorized targeted residual reduction: an independent re-audit of
cycle-2's 41-member `[ExcludeFromCodeCoverage]` boundary
(`artifacts/research/2026-07-02T11-00-qfc-item-controller-residual-reaudit-research.md`) found 17
members actionable without new invariant violations, and Phases 9–10 de-exempt them (41→24), matching
the itemized grep count exactly
(`evidence/qa-gates/final-residual-verification.2026-07-02T15-16.md`). The full C# toolchain is green
in order (csharpier → analyzers → nullable/TWAE → MSTest+coverage, each `EXIT_CODE 0`), 347/347
QuickFiler.Test and 4093/4093 UtilitiesCS.Test pass (4440 total, 0 fail, 0 regression), all
touched/created files are ≤ 500 lines except the documented pre-existing `FolderPredictor.cs` exception
(823 lines, unchanged this cycle beyond `partial`), and no leaf-control interface layer (Option B) was
introduced.

**One material reduction-honesty finding was identified during this independent re-verification** (not
self-reported by the delivered evidence): two of the 17 claimed de-exemptions —
`QfcItemController.ToggleFocus()` and `ToggleFocus(Enums.ToggleState)`
(`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:27`, `:83`) — are exercised only by tests
that verify the method calls `_itemViewer.Invoke(...)` once; the tests explicitly (and honestly, in
their own inline comments) never execute the delegate passed to `Invoke`, so none of the method's
actual behavior (the `_activeUI`/`_activeTheme` state transition, `RegisterFocusAsyncActions`/
`UnregisterFocusAsyncActions` calls, or the `_themes[...].SetQfcTheme(async:false)` call) is verified by
any test. This is a materially weaker test than the pattern it is claimed to mirror
(`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:124-151` vs. the cited sibling
`SetThemeDark`/`SetThemeLight` tests at lines 329-360, which decouple state-mutation from the
Theme-render call via `async: true` deferral and DO assert the resulting `_activeTheme` field). Cycle-2's
own accepted code review already documented the underlying barrier as genuine ("the exempt `ToggleFocus`
uses `SetQfcTheme(async: false)` which synchronously dereferences the null `_lblItemNumber` on the
color-only `BuildColorTheme` double — a genuine, verified barrier",
`2026-07-02T10-47-audit/code-review.2026-07-02T10-47.md:126-128`); cycle-3 does not resolve that barrier,
it avoids triggering it by never invoking the delegate. See Gaps §8, item 1, and the AC8/AC10 PARTIAL
disposition in the companion feature-audit.

A second, recurring, non-code-quality item: **cycle-3's delivery is uncommitted** in the working tree
(same process/merge gate flagged in cycle-2's review). Committed HEAD `0a212191` carries no cycle-3
diff; all cycle-3 production/seam/test/csproj/evidence files are modified/untracked per `git status`.

**Policy documents evaluated:**
- ✅ `CLAUDE.md` (all sections, incl. C# Code Change Policy, General/C# Unit Test Policy, Tonality)
- ✅ `.claude/rules/general-code-change.md`
- ✅ `.claude/rules/general-unit-test.md`
- ✅ `.claude/rules/csharp.md` (toolchain order, DI-seam ordering, analyzer severity-first invariant, banned symbols)
- ✅ `.claude/rules/tonality.md`
- N/A `.claude/rules/python.md`, `powershell.md`, `typescript.md` (no such files in scope)

**Temporary artifacts cleanup:**
- ✅ No throwaway scripts introduced this cycle.
- ✅ No temporary files created by tests (verified: `Theme.DispatcherTests.cs` and the dispatcher/theme
  helpers use in-process mocks and reflection injection, not temp files).

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| Independence | ✅ PASS | Each new test constructs its own controller/`Theme` via reflection/factory helpers (`QfcItemControllerTestSupport.BuildDispatchableTheme`, `BuildThemeDictionary`); no shared mutable state across tests. |
| Isolation | ✅ PASS | New tests target one member/behavior; per-cluster test files mirror the partials (`FolderHandlingTests`, `SeamDispatcherTests`, `EventHandlersTests`). |
| Fast Execution | ✅ PASS | 4440 MSTest cases total; no sleeps/retries; `WpfUiDispatcherTests` uses a deterministic `ManualResetEventSlim` signal instead of polling. |
| Determinism | ✅ PASS | No network/clock/temp-file dependence; `Mock<IUiDispatcher>`/`Mock<IFolderSearchHandler>` execute synchronously or are explicitly non-executing by design. |
| Readability & Maintainability | ⚠️ PARTIAL | Most new tests are descriptively named with AAA structure and inline rationale comments. Two tests (`ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke`, `ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke`) are named/commented honestly but assert only plumbing, not the documented behavior — see Executive Summary and §1.2. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| Baseline Coverage Documented | ✅ PASS | `evidence/remediation-baseline/baseline-tests-coverage.2026-07-02T11-15.md`: affected non-exempt denom 73.59% (989/32/323 of 1344); 328 QuickFiler.Test / 4089 UtilitiesCS.Test passing. |
| No Coverage Regression (changed lines) | ✅ PASS | `evidence/regression-testing/coverage-delta.2026-07-02T15-14.md`: every changed line has direct or transitive test coverage; +3.81pp affected-denominator improvement; no line lost coverage. |
| New Code Coverage ≥ 90% | ⚠️ PARTIAL | The new production surface (interface declarations, factory-delegate fields/parameters, `Theme._uiDispatcher` field/constructor line, three retrofitted dispatcher call sites) is reported at effectively 100% per `coverage-delta.2026-07-02T15-14.md`. However, two of the 17 newly-instrumented (de-exempted) members — `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)` — have their entire substantive body (all lines inside the `_itemViewer.Invoke(...)` lambda, `QfcItemController.FocusAndTheme.cs:29-66` and `:85-122`) unexecuted by any test, even though the lines are nominally instrumented (not excluded). This is newly-instrumented code that does not meet the ≥90% *behavioral* verification bar even though the numeric line-coverage tally is not visibly affected in the headline figure (the lambda-body lines are correctly counted as `lines_not_covered` in the raw Cobertura data, so the 77.40% aggregate is honest; the ≥90% new-code claim in the evidence narrative overstates verification quality for these two members specifically). |
| Comprehensive Coverage (testable denominator ≥ 80%) | ⚠️ PARTIAL | Affected non-exempt denominator is 77.40% (1243/33/330 of 1606) — below the 80% floor referenced by AC5/spec's Coverage Target, though improved +3.81pp from baseline and not regressed. This is the same denominator interpretation cycle-1/2 used; the affected-denominator figure has not yet crossed 80% post-cycle-3 (see Gaps §8, item 2). |
| Positive / Negative / Edge / Error flows | ✅ PASS | Seam routing (positive), null/empty collaborator handling (negative), cancellation and empty-folder-array cases (edge) preserved from prior cycles; `BtnFlagTask_Click`'s sentinel-exception test (error) genuinely exercises the delegator-to-`FlagAsTask()` path. |
| Concurrency | N/A | UI-thread marshaling is seam-mocked or exercised against a real dedicated-STA-thread dispatcher (`WpfUiDispatcherTests`); no new shared-state concurrency logic added. |
| State Transitions | ⚠️ PARTIAL | Most state transitions this cycle are genuinely tested (`ToggleFocusAsync`/`ToggleFocusAsync(ToggleState)` assert `_activeUI`/`_activeTheme` post-call at `SeamDispatcherTests.cs:256-264`/`305-306`; `ApplyReadEmailFormat` asserts `ItemHelper.UnRead`/`mailActions` calls at `SeamDispatcherTests.cs:346-349`). The synchronous `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)` state transition (`_activeUI`, `_activeTheme`) is **not** asserted by any test — see Executive Summary. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 73.59% lines (989 covered + 32 partial / 1344 total, affected `QfcItemController` non-exempt denominator, `evidence/remediation-baseline/baseline-tests-coverage.2026-07-02T11-15.md`). Post-change: 77.40% lines (1243 covered + 33 partial / 1606 total). Change: +3.81% lines (denominator grew from 1344 to 1606 as 17 previously-exempted members became instrumented; no previously-covered line lost coverage). New/changed-code coverage: 100% (all new/changed executable lines outside the two flagged members have direct or transitive test coverage per `evidence/regression-testing/coverage-delta.2026-07-02T15-14.md`; two newly-instrumented members' substantive bodies are uncovered — see §1.2 above). Disposition: INCOMPLETE (no regression and net improvement, but the 80% affected-denominator floor is not yet met and two de-exemptions lack behavioral test coverage). Evidence: `evidence/qa-gates/final-tests-coverage.2026-07-02T15-12.md`, `evidence/regression-testing/coverage-delta.2026-07-02T15-14.md`.
- Python / PowerShell / TypeScript / Bash / JSON: `N/A - out of scope` (zero changed files on the branch).

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| Clear Failure Messages | ✅ PASS | FluentAssertions with `because` reasons throughout new tests (e.g., `WpfUiDispatcherTests.cs:46`). |
| Arrange-Act-Assert | ✅ PASS | Explicit Arrange/Act/Assert blocks in all reviewed test files. |
| Document Intent | ✅ PASS | Descriptive method names and inline XML/`//` rationale comments citing the specific cycle-3 task ID (e.g., "Cycle-3 P9-T7", "Cycle-3 P10-T34"), including the two flagged tests, whose comments honestly disclose the non-execution ("its delegate body is never executed"). |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| Avoid External Dependencies | ✅ PASS | No DB/network/Outlook-host/live-control dependence; `WpfUiDispatcherTests` uses an in-process, dedicated-STA-thread WPF `Dispatcher` (no live WinForms/WPF application host), consistent with the External Dependencies rule. |
| Use Mocks/Stubs | ✅ PASS | `Mock<IItemViewer>`, `Mock<IUiDispatcher>`, `Mock<IFolderSearchHandler>`, `Mock<MailItem>`, reflection-injected `_themes`/`_folderPredictorFactory`. |
| Environment Stability (no temp files) | ✅ PASS | No temp files; dispatcher helpers use background STA threads reclaimed at process exit; `Theme.DispatcherTests.cs` uses mocked `IUiDispatcher`. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| Pre-submission Review | ✅ PASS | This document is the cycle-3 policy review. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| Clarify the objective | ✅ PASS | Driven by the maintainer's 2026-07-02 in-session directive + the residual re-audit research + `spec.md` v0.4 cycle-3 scope (items 9-11). |
| Read existing change plans | ✅ PASS | `2026-07-02T11-15-remediation/remediation-plan.2026-07-02T11-15.md` (Phases 9-11, 62 tasks) executed. |
| Document the plan | ✅ PASS | Plan + per-phase evidence under `evidence/qa-gates/p9-*`, `p10a-*`, `p10b-*`. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| Simplicity first | ✅ PASS | `FolderPredictor` factory-delegate mirrors the already-built `EmailFiler`/`FlagTasks`/`ConversationResolver` pattern; `Theme`+`IUiDispatcher` retrofit extends (not duplicates) the existing seam type. |
| Reusability | ✅ PASS | `QfcItemControllerTestSupport.BuildDispatchableTheme`/`BuildThemeDictionary`/`StartRunningDispatcher` shared harness helpers avoid copy-paste across the new tests. |
| Extensibility | ✅ PASS | `Theme`'s new `uiDispatcher` constructor parameter is optional/defaulted (`?? new WpfUiDispatcher()`); no breaking change to existing `Theme` call sites. |
| Separation of concerns | ✅ PASS | `Theme.Rendering.cs` cleanly separates the render body from construction/dispatch logic in `Theme.cs`, also resolving Theme.cs's pre-existing 544-line over-cap condition (now 451 lines, `evidence/qa-gates/final-file-sizes.2026-07-02T15-15.md`). |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| Cohesive modules | ✅ PASS | `Theme.Rendering.cs` (render body only), `FolderPredictor.IFolderSearchHandler.cs` (single empty partial declaration), `IFolderSearchHandler.cs` (interface only) are each single-purpose. |
| Under 500 lines | ✅ PASS (one documented pre-existing exception) | Independently re-measured (`wc -l`): all 10 controller partials, `Theme.cs` (451), `Theme.Rendering.cs` (105), `Theme.DispatcherTests.cs` (148), `IFolderSearchHandler.cs` (32), `FolderPredictor.IFolderSearchHandler.cs` (10) are ≤ 500. `FolderPredictor.cs` remains 823 lines but was NOT grown this cycle (diff is `+1/-1`, the `partial` keyword only; independently confirmed via `git diff --numstat`) — pre-existing debt, per spec item 11 explicitly out of scope this cycle. |
| Public vs internal | ✅ PASS | `IFolderSearchHandler` is `public` (implemented by `FolderPredictor`, consumed by the controller); controller stays `internal partial`. |
| No circular dependencies | ✅ PASS | `Theme` → `UtilitiesCS.Threading.IUiDispatcher` is a new one-directional dependency; no cycle introduced. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| Descriptive names | ✅ PASS | `IFolderSearchHandler`, `_folderPredictorFactory`, `_uiDispatcher` follow existing conventions. |
| Docs/docstrings | ✅ PASS | XML docs on `WpfUiDispatcher`, `IFolderSearchHandler`; inline task-ID comments on every de-exempted member and its test. |
| Comment why, not what | ⚠️ PARTIAL | Most residual-exemption and de-exemption comments explain rationale well. The `ToggleFocus`/`ToggleFocus(Enums.ToggleState)` test comments are honest about *what* is not executed but do not flag that this leaves the method's actual behavior unverified — a documentation gap that likely allowed the reduction-honesty issue to pass internal review undetected. |

### 2.5 After Making Changes — Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ✅ PASS | **Command:** `dotnet tool run csharpier format .` then `check .`<br>**Result:** "Checked 1229 files in 3670ms", 0 flagged. `evidence/qa-gates/final-csharpier.2026-07-02T15-08.md` |
| **2. Linting** | ✅ PASS | **Command:** `MSBuild.exe TaskMaster.sln -t:Build ... -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`<br>**Result:** All 17 projects built, 0 analyzer/EnforceCodeStyle errors. `evidence/qa-gates/final-analyzers.2026-07-02T15-09.md` |
| **3. Type checking** | ✅ PASS | **Command:** `MSBuild.exe TaskMaster.sln -t:Build ... -p:Nullable=enable -p:TreatWarningsAsErrors=true`<br>**Result:** All 17 projects built, 0 nullable/TWAE errors. `evidence/qa-gates/final-nullable.2026-07-02T15-10.md` |
| **4. Testing** | ✅ PASS | **Command:** `vstest.console.exe QuickFiler.Test.dll UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`<br>**Result:** 347/347 + 4093/4093 pass (4440 total). `evidence/qa-gates/final-tests-coverage.2026-07-02T15-12.md` |
| Full toolchain loop | ✅ PASS | Per-phase gates (P9, P10a, P10b) + final gate each show a single clean pass (one documented csharpier restart at P9 for a bracing diff, corrected before the recorded pass). |
| Explicit reporting | ✅ PASS | Commands + exit codes recorded in every evidence file cited above. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| Summarize changes | ✅ PASS | `evidence/other/ac-traceability.2026-07-02T15-18.md`, `exemption-boundary.2026-07-02T15-05.md`, `coverage-delta.2026-07-02T15-14.md`. |
| Design choices explained | ✅ PASS | Factory-delegate and `Theme`+`IUiDispatcher` retrofit rationale documented in `spec.md` §Redesign scope — cycle 3, items 9-10. |
| Update supporting documents | ✅ PASS | `spec.md` v0.4 updated (AC8/AC10 text, item 11 boundary list). |
| Provide next steps | ⚠️ PARTIAL | Ratification request present, but (a) the delivered work is uncommitted, and (b) this audit's ToggleFocus finding and the sub-80% affected-denominator reading are not reflected in `spec.md`'s narrative, which currently reads as fully resolved. |

---

## 3. Language-Specific Code Change Policy Compliance

C# is the only in-scope language. Python/PowerShell/Bash/JSON/TypeScript sections deleted (zero changed files).

### Section 3C-sharp: C# Code Change Policy Compliance

#### 3.C1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| Formatting with CSharpier | ✅ PASS | `final-csharpier.2026-07-02T15-08.md` EXIT_CODE 0 |
| Linting with .NET analyzers | ✅ PASS | `final-analyzers.2026-07-02T15-09.md` EXIT_CODE 0. Pre-existing suggestion-level diagnostics (unused usings, "make field readonly", "simplify null check") persist at `suggestion` severity; no cycle-3 field (`_folderPredictorFactory`, `_folderPredictorEmptyFactory`) introduces a new blocking diagnostic — verified by the same-pass green analyzer build. |
| Type checking (nullable, TWAE) | ✅ PASS | `final-nullable.2026-07-02T15-10.md` EXIT_CODE 0 |

#### 3.C2 Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| Strong contracts / explicit APIs | ✅ PASS | `IFolderSearchHandler` exposes a narrow, explicit `FindFolder(...)` contract; `Theme`'s new constructor parameter is nullable/optional with an explicit default. |
| Null-safety by default | ✅ PASS | Nullable build clean; `_uiDispatcher = uiDispatcher ?? new WpfUiDispatcher();` guard. |
| Composition & focused types | ✅ PASS | `Theme` composes `IUiDispatcher` rather than inheriting; `FolderPredictor` implements `IFolderSearchHandler` via an additive partial declaration, not a base-class change. |
| Async/await & resource safety | ✅ PASS | `InvokeAsync`/`BeginInvoke` seam calls preserve original async shape; no new disposable resources introduced. |

#### 3.C3–C7 (interfaces, error handling, structure, naming, dependencies)

| Requirement | Status | Evidence |
|------------|--------|----------|
| Interfaces when multiple implementations expected | ✅ PASS | `IFolderSearchHandler` has one production implementation (`FolderPredictor`) and one test double; consistent with existing single-impl seam pattern (`IMailItemActions`, `IWebViewCoreInitializer`). |
| Fail-fast error handling | ✅ PASS | No new broad catches introduced this cycle. |
| File-scope explicit usings; no new cycles | ✅ PASS | Explicit usings retained; `Theme` → `UtilitiesCS.Threading` is one-directional. |
| PascalCase/camelCase conventions | ✅ PASS | Verified across new seam/controller code. |
| No unapproved dependencies | ✅ PASS | No new NuGet/packages.config entries; DI-seam ordering (interface for `FolderPredictor`, delegate-extension for `Theme`) followed. |
| Banned symbols | ✅ PASS (no new) | No new banned-symbol call sites introduced. |

---

## 4. Language-Specific Unit Test Policy Compliance

C# is the only in-scope language with tests.

### Section 4C-sharp: C# Unit Test Policy Compliance

| Requirement | Status | Evidence |
|------------|--------|----------|
| MSTest framework | ✅ PASS | `[TestClass]`/`[TestMethod]` throughout all new tests. |
| Moq for mocking | ✅ PASS | `Mock<IFolderSearchHandler>`, `Mock<IUiDispatcher>`, `Mock<MailItem>`, `Mock<IItemViewer>`. |
| FluentAssertions | ✅ PASS | `.Should()...` used across new tests, including the two flagged `ToggleFocus` tests (which use `Verify`, a Moq API, appropriately alongside FluentAssertions elsewhere in the same file). |
| New code ≥ 90%, repo-wide floor | ⚠️ PARTIAL (see §1.2) | New/extracted seam code is effectively 100% covered except the two flagged `ToggleFocus` members; repo-wide floor remains below 80% for `QuickFiler.dll` (47.69%), handled under the #223 authority-scoped exception precedent (uplift tracked under #197); `UtilitiesCS.dll` (85.86%) already exceeds the floor. |
| No weakened/removed tests | ✅ PASS | 0 removed `[TestMethod]`; no `[Ignore]`/`Assert.Inconclusive`; net +19 QuickFiler.Test, +4 UtilitiesCS.Test this cycle. |

---

## 5. Test Coverage Detail

### `Theme` (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs`) — `IUiDispatcher` retrofit (4 tests)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| `SetQfcThemeAsync_RoutesThroughInjectedDispatcher` | Positive | `SetQfcThemeAsync()`'s changed line, 100% (3/3) | ✅ |
| `SetQfcTheme_Async_RoutesThroughInjectedDispatcher` | Positive | `SetQfcTheme(bool)`'s `async:true` branch line | ✅ |
| `SetMailRead_Async_RoutesThroughInjectedDispatcherBeginInvoke` | Positive | `SetMailRead(bool)`'s `async:true` branch line | ✅ |
| `Constructor_BigOverload_WithNullUiDispatcher_DefaultsToWpfUiDispatcher` | Positive/Edge (null-default) | Constructor's `_uiDispatcher = uiDispatcher ?? new WpfUiDispatcher();` line, 100% (90/90 for the constructor overall) | ✅ |

**Coverage:** 100% of the four changed lines (per-function breakdown in `evidence/qa-gates/final-tests-coverage.2026-07-02T15-12.md`).

**Not covered (justified):** the pre-existing, unchanged `InvokeRequired`/`else` branches in `SetQfcTheme(bool)`/`SetMailRead(bool)` are out of this cycle's diff and were already at their baseline coverage state.

### `QfcItemController.ToggleFocus()` / `ToggleFocus(Enums.ToggleState)` (2 tests)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| `ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke` | Plumbing-only (marshal verification) | Only the `_itemViewer.Invoke(...)` call line; the lambda body (state mutation + theme render) is never executed | ⚠️ |
| `ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke` | Plumbing-only (marshal verification) | Same | ⚠️ |

**Coverage:** the outer `Invoke` call line is exercised; approximately 35 lines of substantive body logic
(`QfcItemController.FocusAndTheme.cs:29-66`, `:85-122`) are instrumented but not exercised by any test.

**Not covered:** the `_activeUI`/`_activeTheme` state transition, `RegisterFocusAsyncActions`/
`UnregisterFocusAsyncActions` calls, and `_themes[_activeTheme].SetQfcTheme(async:false)` call — no
test asserts any observable outcome of calling either overload. See Executive Summary and Gaps §8,
item 1, for remediation options.

### `FolderPredictor` factory-delegate cluster (5 members, tests in `FolderHandlingTests.cs`/`EventHandlersTests.cs`)

**Coverage:** all 5 de-exempted members (`LoadFolderHandler`, `LoadFolderHandlerAsync`,
`PopulateFolderComboBox`, `PopulateFolderComboBoxAsync`, `TextBoxSearch_TextChanged`) have at least one
directly-passing dedicated test exercising real behavior (verified by source inspection: e.g.
`TextBoxSearch_TextChanged_UsesInjectedFolderSearchHandler_PopulatesAndSelectsFolder` sets up
`Mock<IFolderSearchHandler>.FindFolder(...)` and asserts the resulting folder-combobox population), not
plumbing-only verification.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 4440 (347 QuickFiler.Test + 4093 UtilitiesCS.Test) | ✅ |
| Tests Passed | 4440 (100%) | ✅ |
| Tests Failed | 0 | ✅ |
| Affected non-exempt denom coverage | 77.40% (below 80% floor; +3.81pp vs. baseline, no regression) | ⚠️ |
| New/extracted-code coverage | ~100% except 2 flagged members (§5) | ⚠️ |
| Exemptions (QfcItemController scope) | 41 → 24 (2 of the 17 removed lack behavioral test verification — see Gaps §8) | ⚠️ |
| Largest changed production file | `QfcItemController.Initialization.cs` = 466 lines | ✅ |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier | `dotnet tool run csharpier check .` | EXIT_CODE 0 | ✅ |
| Analyzers | `MSBuild TaskMaster.sln /t:Build /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT_CODE 0 | ✅ |
| Nullable | `MSBuild TaskMaster.sln /t:Build /p:Nullable=enable /p:TreatWarningsAsErrors=true` | EXIT_CODE 0 | ✅ |
| MSTest | `vstest.console.exe QuickFiler.Test.dll UtilitiesCS.Test.dll /EnableCodeCoverage` | 347/347 + 4093/4093 pass | ✅ |

**Notes:** Suggestion-level analyzer diagnostics from prior cycles persist and do not break the
`TreatWarningsAsErrors` build (non-blocking per the `.claude/rules/csharp.md` severity-first invariant).
No pre-existing test failures observed; no known flaky dispatcher test recurrence.

---

## 8. Gaps and Exceptions

### Identified Gaps

1. **[MATERIAL — reduction honesty] `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)` de-exemption is
   not behaviorally verified.** `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:27-67`,
   `:83-123`; tests at `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:124-151`.
   The tests verify only that `_itemViewer.Invoke` is called once; the delegate (all substantive logic)
   is never executed, so the state-transition/theme-selection/register-actions behavior is unverified.
   This differs from the pattern it claims to mirror (`SetThemeDark`/`SetThemeLight`, same file
   lines 329-360), which decouples the field-selection logic from the Theme-render call via `async:
   true` and DOES assert the resulting `_activeTheme`. Cycle-2's own accepted code review documented the
   underlying `Theme.SetQfcTheme(async:false)` handle-less-Theme fault as a genuine barrier justifying
   the original exemption; that barrier is unchanged. **Remediation (either):** (a) restructure
   `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)` to perform the `_activeUI`/`_activeTheme`
   state-mutation and `Register*/Unregister*FocusAsyncActions` calls directly (outside the `Invoke`
   wrapper), mirroring how `ToggleFocusOnAsync`/`ToggleFocusOffAsync` already decouple state-mutation
   from the Theme-render call, then add assertions on the resulting field state; or (b) restore
   `[ExcludeFromCodeCoverage]` on both members with the same per-member justification cycle-2 used,
   bringing the honestly-justified residual count to 26 (not 24) pending a genuine seam. Artifact
   paths affected: `evidence/other/exemption-boundary.2026-07-02T15-05.md`,
   `evidence/qa-gates/final-residual-verification.2026-07-02T15-16.md`, `spec.md` AC8/AC10 text.
2. **[MATERIAL — process/merge gate, recurring from cycle-2] Cycle-3 work is uncommitted.** Committed
   HEAD `0a212191` has no cycle-3 diff; all cycle-3 production/seam/test/csproj/evidence files are
   modified/untracked (`git status --short`). **Remediation:** commit the full cycle-3 change set, then
   re-run the final toolchain against the committed head to confirm parity with the evidence.
3. **[Minor, non-blocking, unchanged from prior cycles] Affected non-exempt denominator remains below
   the 80% floor.** 77.40% vs. the ≥80% target in spec's Coverage Target section, though improved
   +3.81pp with no regression. **Remediation:** continue coverage uplift in a future cycle or accept
   under the same documented-exception framing already applied to the repo-wide floor, with explicit
   maintainer sign-off (not yet recorded for this specific sub-target).
4. **[Minor, non-blocking] Stale canonical coverage artifact.** `artifacts/csharp/coverage.xml` is
   cycle-1 (2026-06-29, unchanged since); cycle-3 coverage lives in the evidence markdown files cited
   above. **Remediation:** emit a current canonical `coverage.xml` from the cycle-3 final run.
5. **[Nit, non-blocking] Suggestion-level analyzer debt persists.** Unchanged from cycle-2; no new
   diagnostics introduced this cycle (verified: analyzer build green with 0 errors).

### Approved Exceptions

- **Repo-wide 80% floor — authority-scoped exception.** `QuickFiler.dll` repo-wide C# line coverage
  (47.69%) remains below 80%; handled under the maintainer-ratified authority-scoped precedent from
  #223 (`docs/features/active/2026-06-28-qfc-form-viewer-testability-223/maintainer-decision.2026-06-29.md`),
  with residual uplift tracked under #197. `UtilitiesCS.dll` (85.86%) already exceeds the floor.
- **Residual 24-member `[ExcludeFromCodeCoverage]` boundary — pending ratification.** Re-submitted per
  `evidence/other/exemption-boundary.2026-07-02T15-05.md`. This audit does not dispute the 22 of 24
  residuals that remain genuinely exempt (12 no-leaf-interface/cast-invariant, 2 `TlpCellSnapShot`, 3
  deliberate virtual seams, 6 `async void` shells, 1 `WebView2CoreInitializer` external dependency); it
  disputes only whether the *reduction* of 2 specific members (`ToggleFocus`/`ToggleFocus(ToggleState)`)
  out of the 41→24 delta was genuine (see Gaps §8, item 1).

### Removed/Skipped Tests

**None.** 0 removed `[TestMethod]`; no test weakened or skipped.

---

## 9. Summary of Changes

### Commits in This PR/Branch

- Committed HEAD `0a212191` — "docs(#227): group code-review/audit and remediation artifacts by cycle timestamp" (carries cycle-1 + cycle-2).
- Cycle-3 (Phases 9-11) delivery is **uncommitted** in the working tree (see Gaps §8, item 2). No cycle-3 commit exists yet.

### Files Modified (working tree, cycle-3 delta)

1. `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs` (MODIFIED), `Theme.Rendering.cs` (NEW) —
   `IUiDispatcher` retrofit + render-body extraction (also resolves a pre-existing 544-line over-cap
   condition).
2. `UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs` (NEW),
   `FolderPredictor.IFolderSearchHandler.cs` (NEW), `FolderPredictor.cs` (MODIFIED, `partial` only) —
   factory-delegate seam.
3. `QuickFiler/Controllers/QfcItemController*.cs` (10 partials, MODIFIED) — 17 de-exemptions across
   Phases 9-10.
4. `QuickFiler/Helper Classes/QfcThemeHelper.cs` (MODIFIED) — new `uiDispatcher` parameter.
5. `QuickFiler/Interfaces/MailItemActionsAdapter.cs` (MODIFIED) — attribute removal only.
6. `QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj`,
   `UtilitiesCS/UtilitiesCS.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (MODIFIED) — explicit
   `<Compile Include>` entries for all new files (independently verified present).
7. Test files (NEW/MODIFIED) — `Theme.DispatcherTests.cs` and ~9 per-cluster `QfcItemController*Tests.cs` files.
8. `spec.md` (MODIFIED) — v0.4 cycle-3 scope + AC8/AC10 text; plus cycle-3 evidence artifacts under `evidence/`.

---

## 10. Compliance Verdict

### Overall Status: ⚠️ PARTIALLY COMPLIANT

Cycle 3's toolchain, file-size, and process mechanics are fully compliant (green toolchain, 4440/4440
tests, all files ≤ 500 lines except the documented pre-existing exception, no evidence-location
violations, no leaf-control interfaces introduced). However, an independent re-verification of the
reduction-honesty claim (requested explicitly for this cycle) found that 2 of the 17 claimed
de-exemptions (`ToggleFocus()`/`ToggleFocus(Enums.ToggleState)`) are not behaviorally verified by any
test, and the affected non-exempt denominator (77.40%) has not yet crossed the spec's 80% target. Both
items are material enough to withhold a full-PASS verdict pending remediation or an explicit,
documented maintainer exception.

**Fail-closed reminder honored:** this audit does not mark PASS despite green toolchain evidence,
because the reduction-honesty and denominator gaps are real, evidence-supported findings, not merely
missing artifacts.

---

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- ✅ Before Making Changes: fully documented.
- ✅ Design Principles: seam extension is simple, reusable, well-separated.
- ✅ Module & File Structure: all files ≤ 500 lines except the documented pre-existing exception.
- ⚠️ Naming, Docs, Comments: honest but incomplete disclosure on the `ToggleFocus` tests.
- ✅ Toolchain Execution: green in order, single pass.
- ⚠️ Summarize & Document: `spec.md` narrative does not yet reflect this audit's findings.

#### Language-Specific Code Change Policy (Section 3)
- ✅ Tooling & Baseline: csharpier/analyzers/nullable all green.
- ✅ C# Design & Type-Safety: strong contracts, null-safety, composition preserved.
- ✅ Structure & Naming: consistent with repo conventions.

#### General Unit Test Policy (Section 1)
- ⚠️ Core Principles: readability/maintainability partial (2 tests).
- ⚠️ Coverage & Scenarios: comprehensive coverage and state-transition requirements partial.
- ✅ Test Structure: AAA, clear failure messages, documented intent.
- ✅ External Dependencies: no external dependencies; deterministic.
- ✅ Policy Audit: this document.

#### Language-Specific Unit Test Policy (Section 4)
- ✅ Framework & Scope: MSTest/Moq/FluentAssertions used correctly.
- ⚠️ New code ≥ 90% / repo-wide floor: partial (2 members; QuickFiler.dll repo-wide under exception).

---

### Metrics Summary

- ✅ 4440/4440 tests passing (100%)
- ⚠️ 77.40% affected-denominator line coverage (below the 80% spec target; improved, no regression)
- ⚠️ 24 residual exemptions (2 of the 17 de-exemptions this cycle lack behavioral verification)
- ✅ Proper file organization: all touched/created files ≤ 500 lines except the documented pre-existing exception
- ✅ All toolchain code quality checks passing
- ✅ No removed/weakened tests

---

### Recommendation

**Needs Revision (Conditional).** No toolchain, formatting, or structural blockers. Two concrete,
scoped items must be resolved before this cycle can be considered fully compliant: (1) either genuinely
verify or honestly re-exempt `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)`, and (2) commit the
delivered working tree. The affected-denominator sub-80% reading should be explicitly acknowledged
(accepted-with-exception or scheduled for further uplift) rather than left implicit.

---

## Appendix A: Test Inventory

Cycle-3 new/updated test classes (MSTest `[TestClass]`), each mirroring a controller cluster or seam:

- `WpfUiDispatcherTests` › `Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread` (genuine live-dispatcher execution)
- `MailItemActionsAdapterTests` (unchanged; attribute removal only, pre-existing coverage)
- `QfcItemController_EventWiringTests` › `RegisterExpandedActions_RegistersBAndDWithoutInvokingLambdaBodies`, `UnregisterExpandedActions_AfterRegister_RemovesSyncBAndD`
- `QfcItemController_NavigationTests` › `JumpToAsync_FocusesHandlelessControlAndTogglesKeyboardDialog` (genuine execution)
- `QfcItemController_ViewerSetupTests` › `PopulateControls_WithMailItem_ConstructsHelperAndAssignsControls` (genuine execution)
- `QfcItemController_FocusAndThemeTests` › `ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke`, `ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke` (plumbing-only — see §5)
- `QfcItemController_EventHandlersTests` › `BtnFlagTask_Click_InvokesFlagAsTask` (genuine execution via sentinel-exception factory), `TextBoxSearch_TextChanged_UsesInjectedFolderSearchHandler_PopulatesAndSelectsFolder` (genuine execution)
- `QfcItemController_SeamDispatcherTests` › `ToggleFocusAsync_StateOverload_WhenTurningOn_RegistersAndRoutesThemeThroughInjectedDispatcher`, `ToggleFocusAsync_ParameterlessOverload_WhenActive_RoutesToOffAndThemeThroughInjectedDispatcher`, `ApplyReadEmailFormat_MarksMailReadFalseAndRoutesThemeThroughInjectedDispatcherBeginInvoke` (all genuine execution + field/mock assertions)
- `Theme_DispatcherTests` (`UtilitiesCS.Test`) › `SetQfcThemeAsync_RoutesThroughInjectedDispatcher`, `SetQfcTheme_Async_RoutesThroughInjectedDispatcher`, `SetMailRead_Async_RoutesThroughInjectedDispatcherBeginInvoke`, `Constructor_BigOverload_WithNullUiDispatcher_DefaultsToWpfUiDispatcher`

Full inventory in `evidence/qa-gates/final-tests-coverage.2026-07-02T15-12.md` (347 + 4093 = 4440 total).

---

## Appendix B: Toolchain Commands Reference

**For C#:**
```powershell
# Formatting
dotnet tool run csharpier .

# Linting (.NET analyzers)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

# Type checking (nullable / TreatWarningsAsErrors)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true

# Testing + coverage
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation
```

---

**Audit Completed By:** feature-reviewer (Claude)
**Audit Date:** 2026-07-02
**Policy Version:** Current (as of audit date)
