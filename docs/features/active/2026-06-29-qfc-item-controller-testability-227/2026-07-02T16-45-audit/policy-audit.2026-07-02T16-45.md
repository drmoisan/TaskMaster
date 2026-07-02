# Policy Compliance Audit: QfcItemController Testability — Cycle-4 Exit Reaudit (#227)

**Audit Date:** 2026-07-02
**Code Under Test:** C# only. Modified: `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`
(the sole `.cs`/`.csproj` file changed this cycle — independently confirmed via
`git diff --stat 6291bdf6..48eb71ce -- '*.cs' '*.csproj'`). No production `.cs` file changed
(`git diff --numstat 6291bdf6..48eb71ce -- QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs`
returns no output — zero-line diff, independently re-confirmed).

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 1 test file (cycle-4 delta) | 349 (QuickFiler.Test) + 4093 (UtilitiesCS.Test) = 4442 | ✅ 4442 pass, 0 fail (independently re-confirmed: full `QuickFiler.Test.dll` run, 349/349) | Whole-process (all 18 loaded modules) 63.21%; `QuickFiler.dll` 47.69%; `UtilitiesCS.dll` 85.86% (`evidence/remediation-baseline/baseline-tests-coverage.2026-07-02T15-35.md`, cycle-3 exit state) | Whole-process **63.28%**; `QuickFiler.dll` **48.32%**; `UtilitiesCS.dll` **85.96%** (`evidence/qa-gates/final-tests-coverage.2026-07-02T16-25.md`) | 100% (the two `ToggleFocus`/`ToggleFocus(Enums.ToggleState)` overload bodies newly unblocked this cycle; 2 modified + 2 new test methods, independently re-confirmed passing; test files themselves are excluded from the application-code coverage metric per policy — see §1.2.1) |

**Note:** C# is the only language in scope. `git diff --name-status 6291bdf6..48eb71ce` (committed cycle-4
delta) contains zero Python, PowerShell, Bash, TypeScript, or JSON files (only one `.cs` test file plus
`.md` docs/evidence and two `.claude/agent-memory/**` files), so those coverage categories are
`N/A - out of scope` (zero changed files), not narrowed by any caller instruction.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/remediation-baseline/baseline-tests-coverage.2026-07-02T15-35.md`
- C# post-change coverage artifact: `evidence/qa-gates/final-tests-coverage.2026-07-02T16-25.md`
- TypeScript baseline coverage artifact: `N/A - out of scope` (zero changed TypeScript files)
- TypeScript post-change coverage artifact: `N/A - out of scope` (zero changed TypeScript files)
- PowerShell baseline coverage artifact: `N/A - out of scope` (zero changed PowerShell files)
- PowerShell post-change coverage artifact: `N/A - out of scope` (zero changed PowerShell files)
- Per-language comparison summary: `evidence/regression-testing/coverage-delta.2026-07-02T16-30.md` and §1.2.1 below.

**Verdict rule note:** Numeric baseline and post-change coverage are present for the only in-scope
language (C#), so the PASS-eligibility precondition is satisfied.

---

## Rejected Scope Narrowing

No caller/orchestrator prompt attempted to narrow the audit scope to a plan, task, phase, or file subset,
and none attempted to mark any language's coverage as out of scope or informational-only. The delegation
prompt frames "cycle 4" as a narrow, test-only delta and lists focus areas, but explicitly requires
independent verification (not acceptance of the delivered narrative), confirmation of zero production
regression across the full commit range (`6291bdf6..48eb71ce`), and a full accounting of the working tree
state and exemption count against the entire feature scope — consistent with, not a narrowing of, the
scope invariant (full branch diff vs. resolved base). This audit accordingly evaluated both the cycle-4
delta specifically (§ throughout) and re-confirmed the unchanged state of everything delivered in cycles
1-3 that the delegation's focus areas depend on (exemption count, file sizes, production-code identity).
**Nothing to reject.**

---

## Evidence Location Compliance

Scanned the full cycle-4 change set (`git diff --name-status 6291bdf6..48eb71ce`) for files under
`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`. **None exist.** All
cycle-4 audit-trail evidence is under the canonical
`docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/<kind>/` scheme
(`remediation-baseline/`, `qa-gates/`, `regression-testing/`). `validate_evidence_locations.py` was not
found in this checkout (consistent with cycles 2 and 3's findings); the PreToolUse hook
`.claude/hooks/enforce-evidence-locations.ps1` is present. **No FAIL-level evidence-location findings.**

No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` was required (no non-canonical evidence path was supplied in the
delegation for this cycle).

---

## Executive Summary

Cycle 4 is a narrow, test-only remediation of the sole material finding carried out of the cycle-3 exit
reaudit (`2026-07-02T15-26-audit/`): `QfcItemController.ToggleFocus()` and
`ToggleFocus(Enums.ToggleState)` were de-exempted in cycle 3 (part of the 41→24 reduction) but tested only
for the fact that `_itemViewer.Invoke(...)` was called, never executing the delegate carrying the
methods' substantive logic. Cycle 4 replaces the non-executing `new Mock<IItemViewer>()` in the two
affected tests with the file's own already-proven `BuildExecutingViewer()` helper, adds a new
`EnableHandlelessThemeInvoke` helper that reflection-injects 16 handle-less doubles into every `Theme` in
the controller's `_themes` dictionary (mirroring a technique already proven in
`Theme.DispatcherTests.cs:91-134`), and adds/extends assertions on the resulting `_activeUI`/`_activeTheme`
state for both directions of both overloads (4 tests total: 2 modified in place, 2 new).

**This audit independently re-verified every material claim in the delivered cycle-4 evidence rather than
accepting it at face value:**
- `git diff --numstat 6291bdf6..48eb71ce -- '*.cs'` confirms exactly one `.cs` file changed
  (`QfcItemController.FocusAndThemeTests.cs`) and confirms `QfcItemController.FocusAndTheme.cs` (the
  production file containing the two flagged methods) has a zero-line diff — no production regression is
  possible this cycle.
- `dotnet tool run csharpier check` on the sole changed file returns exit 0 (independently re-run).
- `wc -l`-equivalent count on the changed file returns 497 (<= 500 cap, independently re-measured).
- `grep -rnE "ExcludeFromCodeCoverage\]" ...` returns exactly 24 matches (unchanged from cycle 3,
  independently re-run).
- Direct `vstest.console.exe` execution of the 4 named `ToggleFocus*` tests in isolation: `Passed: 4,
  Total: 4`.
- Direct `vstest.console.exe` execution of the full `QuickFiler.Test.dll` suite (no filter): `Total tests:
  349, Passed: 349` — matches the evidence-recorded 347 baseline + 2 new exactly, zero failures.
- Direct source comparison of `QfcItemController.FocusAndTheme.cs:27-123` (both `ToggleFocus` overloads)
  against the 4 tests' assertions confirms the asserted `_activeUI`/`_activeTheme` outcomes match the
  actual branch logic for all 4 directions (On-from-inactive and Off-from-active for the `ToggleState`
  overload; both directions for the parameterless overload).
- Direct source comparison of `Theme.cs:414-432` (`SetQfcTheme(bool)`) and `Theme.Rendering.cs:8-103` (the
  private `SetQfcTheme()` it falls through to) against the 16 fields reflection-injected by
  `EnableHandlelessThemeInvoke` confirms every field the executed production code path dereferences is
  populated with a viable double, and no field is missing or superfluous.
- Independently re-traced the reported `Times.Exactly(2)` correction: `ToggleFocus`'s delegate calls
  `ToggleTips(async: false, ...)` (`FocusAndTheme.cs:44,60,100,116`), which calls
  `InvokeBeginInvoke(false, ...)` → a second, nested `_itemViewer.Invoke(action)` call
  (`FocusAndTheme.cs:255-256`). This is a genuine strengthening of a previously-inaccurate `Times.Once()`
  assumption (which could only have been observed as wrong once the delegate genuinely executes), not a
  weakening of any assertion.

**No reduction-honesty defect or scope-change irregularity was found in this reaudit.** All three
scope-change findings the executor self-reported
(`evidence/qa-gates/p1-toggle-focus-verification.2026-07-02T16-20.md`) — the missing compile-time
reference workaround via `Activator.CreateInstance(field.FieldType)`, the `QfcItemController`-own
`_tableLayoutPanels` field population, and the `Times.Exactly(2)` correction — were independently traced to
source and confirmed sound, test-only, and non-weakening.

**A second item from cycle-3's finding list (uncommitted delivery) was already resolved before cycle 4
opened** (commit `6291bdf6`), and cycle 4 itself is committed cleanly: `git status --short` returns no
output at HEAD `48eb71ce` (independently re-confirmed).

**Policy documents evaluated:**
- ✅ `CLAUDE.md` (all sections, incl. C# Code Change Policy, General/C# Unit Test Policy, Tonality)
- ✅ `.claude/rules/general-code-change.md`
- ✅ `.claude/rules/general-unit-test.md`
- ✅ `.claude/rules/csharp.md` (toolchain order, DI-seam ordering, analyzer severity-first invariant, banned symbols)
- ✅ `.claude/rules/tonality.md`
- N/A `.claude/rules/python.md`, `powershell.md`, `typescript.md` (no such files in scope; zero changed files of those languages)

**Temporary artifacts cleanup:**
- ✅ No throwaway scripts introduced this cycle.
- ✅ No temporary files created by tests (verified: `EnableHandlelessThemeInvoke`/`SetThemeField`/
  `SetThemeFieldViaActivator` use in-process reflection and doubles, not temp files; independently
  confirmed no filesystem writes occur during the direct test re-execution performed for this audit).

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| Independence | ✅ PASS | Each of the 4 `ToggleFocus*` tests constructs its own `viewer`/`controller` via `BuildExecutingViewer()`/`BuildFocusController()`; no shared mutable state across tests. Independently re-confirmed by running the 4 tests both in isolation (filtered run) and as part of the full 349-test suite with identical pass results. |
| Isolation | ✅ PASS | Each test targets one member/direction: `ToggleFocus(On)`, `ToggleFocus(Off)`, `ToggleFocus()` on, `ToggleFocus()` off. |
| Fast Execution | ✅ PASS | The 4 named tests complete in ~424ms combined per this audit's own re-run; no sleeps/retries/polling. |
| Determinism | ✅ PASS | No network/clock/temp-file dependence. `Activator.CreateInstance(field.FieldType)` is deterministic for both `FastObjectListView` and `WebView2` (both expose accessible parameterless constructors); independently re-run twice in this audit (once filtered, once as part of the full suite) with identical results both times — no flake observed. |
| Readability & Maintainability | ⚠️ PARTIAL | The two new tests (`..._Off_FromActive_...`, `..._FromActive_...`) are clearly named and documented. The two *modified* tests retain their pre-cycle-4 names (`..._MarshalsThroughItemViewerInvoke`), which now understate what the tests verify (they assert full state-transition behavior, not merely the `Invoke` marshal) — a Minor, non-blocking naming-staleness gap, not a correctness defect (see companion code-review Findings Table). |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| Baseline Coverage Documented | ✅ PASS | `evidence/remediation-baseline/baseline-tests-coverage.2026-07-02T15-35.md`: whole-process 63.21%; `QuickFiler.dll` 47.69%; `UtilitiesCS.dll` 85.86%; 347/4093 (QuickFiler.Test/UtilitiesCS.Test) passing, matching the cycle-3 exit state exactly. |
| No Coverage Regression (changed lines) | ✅ PASS | `evidence/regression-testing/coverage-delta.2026-07-02T16-30.md`: whole-process 63.21%→63.28% (+0.07pp), `QuickFiler.dll` 47.69%→48.32% (+0.63pp), `UtilitiesCS.dll` 85.86%→85.96% (+0.10pp) — no regression on any metric. Independently corroborated: a full `QuickFiler.Test.dll` run in this audit shows 349/349 passing with no failures, consistent with no test-level regression. |
| New Code Coverage ≥ 90% | ✅ PASS | New/changed-code coverage: 100% (must be ≥90%). This cycle adds test code only (excluded from the application-code coverage metric per policy); the production lines it newly exercises (`ToggleFocus`/`ToggleFocus(Enums.ToggleState)` bodies, `QfcItemController.FocusAndTheme.cs:29-66`, `:85-122`) are independently confirmed covered via direct re-execution of the 4 named tests (4/4 pass) and via the per-line `<range ... covered="yes" />` XML evidence in `coverage-delta.2026-07-02T16-30.md`, cross-checked against source. |
| Comprehensive Coverage (testable denominator ≥ 80%) | ⚠️ PARTIAL (unchanged, deferred) | The `QfcItemController`-scoped affected non-exempt denominator was 77.40% at cycle-3 exit and was not recomputed this cycle (explicitly deferred per `remediation-inputs.2026-07-02T15-35.md`, consistent with cycle-3's disposition). The two now-genuinely-covered `ToggleFocus` bodies plausibly raise this figure, but no fresh numeric recompute exists in the cycle-4 evidence trail; this audit does not synthesize one. Not a regression; an unrefreshed, previously-disclosed open item. |
| Positive / Negative / Edge / Error flows | ✅ PASS | All 4 state-transition directions required by the reaudit scope are covered: `ToggleFocus(On)` inactive→active (positive), `ToggleFocus(Off)` active→inactive (positive/edge — opposite branch), `ToggleFocus()` inactive→active and active→inactive (both directions of the parameterless overload). No negative/error-input scenario applies to these two methods (they take no externally-invalid input; `Enums.ToggleState` is a closed enum). |
| Concurrency | N/A | No new concurrency logic; `BuildExecutingViewer()`'s `Invoke`/`BeginInvoke` setups execute synchronously via `DynamicInvoke`, matching the production dispatch contract being tested. |
| State Transitions | ✅ PASS (upgraded from cycle-3's PARTIAL) | Cycle-3 flagged the `_activeUI`/`_activeTheme` state transition for `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)` as unasserted by any test. This cycle resolves that gap directly: all 4 tests assert `GetField(controller, "_activeUI").Should().Be(...)` and `GetField(controller, "_activeTheme").Should().Be(...)` against the actual resulting state, independently re-confirmed by source comparison against `FocusAndTheme.cs:27-123`'s branch logic. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 63.21% lines (whole-process, all 18 loaded modules; 109,235 covered + 2,689 partial / 177,062 total; per-module `QuickFiler.dll` 47.69%, `UtilitiesCS.dll` 85.86%). Post-change: 63.28% lines (whole-process; 109,392 covered + 2,696 partial / 177,135 total; per-module `QuickFiler.dll` 48.32%, `UtilitiesCS.dll` 85.96%). Change: +0.07% lines whole-process (+0.63% `QuickFiler.dll`, +0.10% `UtilitiesCS.dll`); no regression on any metric. New/changed-code coverage: 100% (the two `ToggleFocus`/`ToggleFocus(Enums.ToggleState)` overload bodies newly unblocked this cycle, `FocusAndTheme.cs:29-66`,`:85-122`, confirmed covered via independent direct test re-execution — 4/4 named tests pass — and per-line `<range ... covered="yes" />` XML evidence). Disposition: PASS (no regression on any recomputed metric, net improvement on all three; the separate, `QfcItemController`-scoped affected-non-exempt-denominator metric, 77.40% at cycle-3 exit, was not recomputed this cycle — carried, not regressed; see §1.2 and Gaps §8). Evidence: `evidence/qa-gates/final-tests-coverage.2026-07-02T16-25.md`, `evidence/regression-testing/coverage-delta.2026-07-02T16-30.md`, `evidence/remediation-baseline/baseline-tests-coverage.2026-07-02T15-35.md`.
- Python / PowerShell / TypeScript / Bash / JSON: `N/A - out of scope` (zero changed files on the branch this cycle).

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| Clear Failure Messages | ✅ PASS | FluentAssertions (`.Should().Be(...)`) used for the state assertions; Moq `Verify(v => v.Invoke(...), Times.Exactly(2))` for the call-count assertion, both with descriptive inline comments explaining the expected outcome. |
| Arrange-Act-Assert | ✅ PASS | Explicit Arrange/Act/Assert comment blocks in all 4 modified/new test methods, independently re-confirmed by source read. |
| Document Intent | ✅ PASS | `EnableHandlelessThemeInvoke` carries a detailed XML doc comment explaining the two distinct NREs it avoids and citing the precedent test it mirrors; inline comments on each of the 4 tests explain why `Times.Exactly(2)` (not `Times.Once()`) is correct. Two test *names* are stale (see §1.1, Readability & Maintainability). |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| Avoid External Dependencies | ✅ PASS | No DB/network/Outlook-host/live-control dependence. `Activator.CreateInstance(field.FieldType)` constructs in-process WinForms/WebView2 objects without a live handle or host process — independently confirmed by the direct test re-execution in this audit completing without any process/window dependency. |
| Use Mocks/Stubs | ✅ PASS | `BuildExecutingViewer()` (a `Mock<IItemViewer>` with executing `Invoke`/`BeginInvoke` setups); reflection-injected handle-less `Theme` field doubles. |
| Environment Stability (no temp files) | ✅ PASS | No temp files; all doubles are in-memory/reflection-constructed objects with no filesystem or window-handle dependency. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| Pre-submission Review | ✅ PASS | This document is the cycle-4 exit reaudit's policy review. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| Clarify the objective | ✅ PASS | Driven directly by the sole cycle-3 exit-reaudit finding (`R1` in `remediation-inputs.2026-07-02T15-35.md`), scoped explicitly to a test-only fix. |
| Read existing change plans | ✅ PASS | `2026-07-02T15-35-remediation/remediation-plan.2026-07-02T15-35.md` (Phase 0-2, 12 tasks) executed; each task independently spot-checked against its recorded evidence artifact in this audit. |
| Document the plan | ✅ PASS | Plan documents a detailed, source-cited design rationale (including the second-NRE discovery re: `Theme.Rendering.cs`) before any code was written. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| Simplicity first | ✅ PASS | The fix reuses the file's own pre-existing `BuildExecutingViewer()` helper rather than introducing a new mocking mechanism; `EnableHandlelessThemeInvoke` follows the exact reflection pattern `BuildColorTheme` already uses for `_uiDispatcher`. |
| Reusability | ✅ PASS | `EnableHandlelessThemeInvoke`/`SetThemeField`/`SetThemeFieldViaActivator` are shared by all 4 `ToggleFocus*` tests, avoiding per-test duplication of the 16-field injection logic. |
| Extensibility | N/A | Test-only change; no new production extension point. |
| Separation of concerns | ✅ PASS | The new helpers are confined to test-double construction; no test logic leaks into the assertion phase and no production logic is duplicated in the test file. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| Cohesive modules | ✅ PASS | The new helpers live in the same file as the tests they support, consistent with the file's existing single-cluster (`FocusAndTheme`) scope. |
| Under 500 lines | ✅ PASS | `QfcItemController.FocusAndThemeTests.cs` independently re-measured at 497 lines (<= 500). No production file changed this cycle. |
| Public vs internal | ✅ PASS | No public surface change; all new helpers are `private static`, consistent with the file's existing convention. |
| No circular dependencies | ✅ PASS | No new dependency direction introduced; test-only reflection against existing types. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| Descriptive names | ⚠️ PARTIAL | New helpers/tests are descriptively named (`EnableHandlelessThemeInvoke`, `ToggleFocus_StateOverload_Off_FromActive_DeactivatesUiAndSwitchesToNormalTheme`). The two *modified* tests retain stale names describing only their pre-cycle-4 scope (see §1.1). |
| Docs/docstrings | ✅ PASS | `EnableHandlelessThemeInvoke` carries a thorough XML doc comment citing the exact production lines and precedent test it mirrors; independently verified accurate against source. |
| Comment why, not what | ✅ PASS | Inline comments explain *why* `Times.Exactly(2)` is correct (the nested `ToggleTips`→`InvokeBeginInvoke` call) rather than merely restating the assertion — a direct improvement over cycle-3's documentation gap on this exact code (per cycle-3's policy audit §2.4 finding). |

### 2.5 After Making Changes — Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ✅ PASS | **Command (delivered):** `dotnet tool run csharpier format .`<br>**Result:** exit 0, only the one edited test file changed. **Independently re-run this audit:** `dotnet tool run csharpier check "QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs"` → `Checked 1 files in 365ms`, exit 0. `evidence/qa-gates/final-csharpier.2026-07-02T16-25.md` |
| **2. Linting** | ✅ PASS | **Command:** `MSBuild.exe TaskMaster.sln -t:Build ... -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`<br>**Result:** all projects built, 0 new diagnostics. `evidence/qa-gates/final-analyzers.2026-07-02T16-25.md` |
| **3. Type checking** | ✅ PASS | **Command:** `MSBuild.exe TaskMaster.sln -t:Build ... -p:Nullable=enable -p:TreatWarningsAsErrors=true`<br>**Result:** all projects built, 0 nullable/TWAE errors. `evidence/qa-gates/final-nullable.2026-07-02T16-25.md` |
| **4. Testing** | ✅ PASS | **Command:** `vstest.console.exe QuickFiler.Test.dll UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`<br>**Result:** 349/349 + 4093/4093 = 4442/4442 pass. **Independently re-run this audit:** the 4 named `ToggleFocus*` tests (4/4 pass) and the full `QuickFiler.Test.dll` suite (349/349 pass, matching exactly). `evidence/qa-gates/final-tests-coverage.2026-07-02T16-25.md` |
| Full toolchain loop | ✅ PASS | Single recorded pass; `final-csharpier.2026-07-02T16-25.md` explicitly notes zero files required additional changes beyond Phase 1, so the loop did not need to restart. |
| Explicit reporting | ✅ PASS | Commands and exit codes recorded in every cited evidence file; independently cross-checked in this audit. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| Summarize changes | ✅ PASS | Commit message `48eb71ce` ("test(#227): remediation cycle 4 — genuinely verify ToggleFocus behavior") accurately summarizes scope and rationale; `evidence/qa-gates/p1-toggle-focus-verification.2026-07-02T16-20.md` documents the three scope-change findings transparently. |
| Design choices explained | ✅ PASS | `remediation-plan.2026-07-02T15-35.md` §"Design decisions locked in by this plan" documents the two-NRE analysis and the resolution approach in detail before implementation. |
| Update supporting documents | N/A | `spec.md` was intentionally not modified this cycle (the AC8/AC10 checkbox-gating text already correctly anticipates a future technical resolution; the maintainer-ratification gate it describes is unaffected by this cycle's fix — see companion feature-audit). |
| Provide next steps | ✅ PASS | `remediation-plan.2026-07-02T15-35.md`'s exit condition is explicit and fully met (see Compliance Verdict below); the remaining deferred items (denominator recompute, `coverage.xml` refresh, maintainer ratification) are explicitly enumerated as non-blocking follow-ups, not left implicit. |

---

## 3. Language-Specific Code Change Policy Compliance

C# is the only in-scope language. Python/PowerShell/Bash/JSON/TypeScript sections deleted (zero changed files).

### Section 3C-sharp: C# Code Change Policy Compliance

#### 3.C1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| Formatting with CSharpier | ✅ PASS | `final-csharpier.2026-07-02T16-25.md` EXIT_CODE 0; independently re-confirmed via a direct `csharpier check` on the changed file. |
| Linting with .NET analyzers | ✅ PASS | `final-analyzers.2026-07-02T16-25.md` EXIT_CODE 0. No new field/member was introduced this cycle (test-only), so no new analyzer surface exists to regress. |
| Type checking (nullable, TWAE) | ✅ PASS | `final-nullable.2026-07-02T16-25.md` EXIT_CODE 0. |

#### 3.C2 Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| Strong contracts / explicit APIs | N/A | Test-only change; no new production contract. |
| Null-safety by default | ✅ PASS | No nullable-flow change; the two `Activator.CreateInstance` calls target non-nullable reference-type fields with runtime-guaranteed non-null results (constructor always succeeds for these two concrete WinForms/WebView2 types, independently confirmed by the passing test run). |
| Composition & focused types | N/A | Test-only change. |
| Async/await & resource safety | ✅ PASS | No new disposable resources; `BuildExecutingViewer()`'s synchronous `DynamicInvoke` execution matches the existing pattern used elsewhere in the same test file. |

#### 3.C3–C7 (interfaces, error handling, structure, naming, dependencies)

| Requirement | Status | Evidence |
|------------|--------|----------|
| Interfaces when multiple implementations expected | N/A | No new interface introduced this cycle. |
| Fail-fast error handling | ✅ PASS | `SetThemeField`/`SetThemeFieldViaActivator` use `field.Should().NotBeNull(...)` (a FluentAssertions-based fail-fast guard) before every reflection `SetValue` call, consistent with the file's existing `SetField`/`GetField` helper pattern. |
| File-scope explicit usings; no new cycles | ✅ PASS | No new `using` directive required beyond what was already present (`System.Reflection`, `System.Windows.Forms` already imported). |
| PascalCase/camelCase conventions | ✅ PASS | New helper/test names follow existing conventions. |
| No unapproved dependencies | ✅ PASS | No new NuGet/packages.config entries; no csproj change (`git diff --name-status` confirms zero `.csproj` deltas this cycle). |
| Banned symbols | ✅ PASS (no new) | No new banned-symbol call sites introduced. |

---

## 4. Language-Specific Unit Test Policy Compliance

C# is the only in-scope language with tests.

### Section 4C-sharp: C# Unit Test Policy Compliance

| Requirement | Status | Evidence |
|------------|--------|----------|
| MSTest framework | ✅ PASS | `[TestClass]`/`[TestMethod]` unchanged; the 2 new tests use the same attributes as the surrounding file. |
| Moq for mocking | ✅ PASS | `BuildExecutingViewer()` (`Mock<IItemViewer>`) reused; `viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Exactly(2))`. |
| FluentAssertions | ✅ PASS | `.Should().Be(...)` used for all 4 tests' `_activeUI`/`_activeTheme` assertions; `.Should().NotBeNull(because: ...)` used in the new reflection helpers. |
| New code ≥ 90%, repo-wide floor | ✅ PASS (production lines this cycle unblocks) / ⚠️ carried (repo-wide floor) | The production lines newly exercised this cycle (`ToggleFocus`/`ToggleFocus(Enums.ToggleState)` bodies) are independently confirmed 100% covered via direct re-execution. Repo-wide floor for `QuickFiler.dll` (48.32%) remains below 80%, handled under the maintainer-ratified authority-scoped exception precedent from #223 (uplift tracked under #197) — unchanged carried disposition, not a cycle-4 regression (48.32% is an improvement over the 47.69% baseline). `UtilitiesCS.dll` (85.96%) already exceeds the floor. |
| No weakened/removed tests | ✅ PASS | 0 removed `[TestMethod]`; no `[Ignore]`/`Assert.Inconclusive`; net +2 QuickFiler.Test this cycle (347→349, independently re-confirmed via the full-suite run). The `Times.Once()`→`Times.Exactly(2)` change is a correction to match newly-observed real behavior, not a weakening (independently re-traced to source in the Executive Summary). |

---

## 5. Test Coverage Detail

### `QfcItemController.ToggleFocus()` / `ToggleFocus(Enums.ToggleState)` (4 tests, cycle-4 remediation)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| `ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke` (modified) | Positive (On, from inactive) | `ToggleFocus(Enums.ToggleState)`'s On-from-inactive branch, `FocusAndTheme.cs:32-47`, plus the terminal `SetQfcTheme(async:false)` call — genuinely executed and asserted | ✅ |
| `ToggleFocus_StateOverload_Off_FromActive_DeactivatesUiAndSwitchesToNormalTheme` (new) | Positive (Off, from active) | `ToggleFocus(Enums.ToggleState)`'s Off-from-active branch, `FocusAndTheme.cs:48-63` | ✅ |
| `ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke` (modified) | Positive (inactive→active) | `ToggleFocus()`'s inactive→active branch, `FocusAndTheme.cs:104-119` | ✅ |
| `ToggleFocus_ParameterlessOverload_FromActive_DeactivatesUiAndSwitchesToNormalTheme` (new) | Positive (active→inactive) | `ToggleFocus()`'s active→inactive branch, `FocusAndTheme.cs:88-103` | ✅ |

**Coverage:** all 4 branches (2 per overload) of both `ToggleFocus` overloads' full method bodies —
including the previously-unexecuted `_activeUI`/`_activeTheme` state mutation,
`RegisterFocusAsyncActions`/`UnregisterFocusAsyncActions` calls, `ToggleTips` nested dispatch, and the
terminal `_themes[_activeTheme].SetQfcTheme(async: false)` call — are now genuinely exercised. Independently
re-confirmed via direct execution of all 4 named tests (4/4 pass) and via source comparison of the
asserted outcomes against `FocusAndTheme.cs:27-123`'s actual branch logic.

**Not covered:** none identified within the scope of these two methods. The two methods' full bodies are
now covered; no remaining gap.

**Cross-reference:** `evidence/regression-testing/coverage-delta.2026-07-02T16-30.md` §"Confirmation:
ToggleFocus/ToggleFocus(Enums.ToggleState) production lines are covered" reports per-line
`<range source_id="43" ... covered="yes" />` XML evidence for the full span of both method bodies
(lines 27-67, 83-123), consistent with this audit's independent test-execution-based verification.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 4442 (349 QuickFiler.Test + 4093 UtilitiesCS.Test) | ✅ |
| Tests Passed | 4442 (100%) | ✅ |
| Tests Failed | 0 | ✅ |
| Independently re-run: 4 named `ToggleFocus*` tests | 4/4 pass (~424ms combined) | ✅ |
| Independently re-run: full `QuickFiler.Test.dll` suite | 349/349 pass (~1.1s, no filter) | ✅ |
| Whole-process line coverage | 63.28% (+0.07pp vs. baseline, no regression) | ✅ |
| `QuickFiler.dll` line coverage | 48.32% (+0.63pp vs. baseline) | ⚠️ (below 80% repo-wide floor; authority-scoped exception applies, unchanged carried disposition) |
| `UtilitiesCS.dll` line coverage | 85.96% (+0.10pp vs. baseline) | ✅ |
| `QfcItemController` affected non-exempt denominator | 77.40% (cycle-3 figure; not recomputed this cycle) | ⚠️ carried, deferred |
| Exemptions (QfcItemController scope) | 24 (unchanged; independently re-confirmed via grep) | ✅ |
| Changed test file line count | 497 (<= 500 cap; independently re-measured) | ✅ |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier | `dotnet tool run csharpier check .` | EXIT_CODE 0 (delivered); independently re-run on the sole changed file, EXIT_CODE 0 | ✅ |
| Analyzers | `MSBuild TaskMaster.sln /t:Build /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT_CODE 0 | ✅ |
| Nullable | `MSBuild TaskMaster.sln /t:Build /p:Nullable=enable /p:TreatWarningsAsErrors=true` | EXIT_CODE 0 | ✅ |
| MSTest | `vstest.console.exe QuickFiler.Test.dll UtilitiesCS.Test.dll /EnableCodeCoverage` | 349/349 + 4093/4093 pass; independently re-run 349/349 QuickFiler.Test.dll | ✅ |

**Notes:** No new analyzer diagnostics possible this cycle (test-only change, no new production member).
No pre-existing test failures observed; no flaky-test recurrence across two independent re-runs performed
for this audit (filtered 4-test run and full 349-test run both passed identically).

---

## 8. Gaps and Exceptions

### Identified Gaps

1. **[Minor, non-blocking] Two of the four `ToggleFocus*` test names are stale.**
   `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:189`, `231`
   (`ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke`,
   `ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke`) retain names describing only the
   pre-cycle-4 marshal-only scope, though both tests now also assert full state-transition behavior.
   **Remediation:** rename in a future pass (non-blocking); see companion code-review Findings Table.
2. **[Minor, non-blocking, carried] Affected non-exempt denominator not recomputed.** The
   `QfcItemController`-scoped 77.40% figure (cycle-3 exit) was not recomputed after this cycle's fix, even
   though the two now-covered `ToggleFocus` bodies plausibly raise it. Explicitly deferred per
   `remediation-inputs.2026-07-02T15-35.md`; not this cycle's assigned scope. **Remediation:** recompute in
   a future cycle that touches this coverage surface again.
3. **[Minor, non-blocking, carried] Stale canonical coverage artifact.** `artifacts/csharp/coverage.xml`
   remains cycle-1-dated (2026-06-29, unchanged; independently re-confirmed via `ls -la`). **Remediation:**
   regenerate from a future full run.
4. **[Info, non-blocking, carried] Repo-wide `QuickFiler.dll` floor remains below 80%** (48.32%, improved
   from 47.69%). Handled under the maintainer-ratified authority-scoped exception precedent from #223,
   uplift tracked under #197 — unchanged disposition, not a regression.

### Approved Exceptions

- **Repo-wide 80% floor — authority-scoped exception.** `QuickFiler.dll` repo-wide C# line coverage
  (48.32%) remains below 80%; handled under the maintainer-ratified authority-scoped precedent from #223
  (`docs/features/active/2026-06-28-qfc-form-viewer-testability-223/maintainer-decision.2026-06-29.md`),
  with residual uplift tracked under #197. `UtilitiesCS.dll` (85.96%) already exceeds the floor.
- **Residual 24-member `[ExcludeFromCodeCoverage]` boundary — pending ratification.** Re-submitted per
  `evidence/other/exemption-boundary.2026-07-02T15-05.md` (unchanged this cycle; still accurate — see
  companion feature-audit AC8/AC10). This audit's independent re-verification finds all 24 residuals
  genuinely exempt with sound per-member justification, and all 17 cycle-3 de-exemptions (including the
  two `ToggleFocus` members this cycle fixes) genuinely behavior-verified. Maintainer ratification of the
  boundary remains an outstanding governance action, distinct from this audit's technical determination.

### Removed/Skipped Tests

**None.** 0 removed `[TestMethod]`; no test weakened, skipped, or `[Ignore]`d. The `Times.Once()` →
`Times.Exactly(2)` change is a correction to accurately reflect newly-observed real dispatch behavior
(independently re-traced to source in the Executive Summary), not a weakening.

---

## 9. Summary of Changes

### Commits in This PR/Branch

- `4611fd60` — merge-base (`main`)
- `bcc7d7e3` — "refactor(#227): split QfcItemController and narrow IItemViewer for testability" (cycle 1)
- `bfc8364b` — "docs(#227): cycle-0 audits and cycle-1 R1 canonical coverage evidence"
- `84789ede` — "refactor(#227): remediation cycle 2 — replace 103 coverage exemptions with seams (Option A)"
- `0a212191` — "docs(#227): group code-review/audit and remediation artifacts by cycle timestamp"
- `6291bdf6` — "refactor(#227): remediation cycle 3 — reduce residual exemptions 41 -> 24"
- `48eb71ce` — "test(#227): remediation cycle 4 — genuinely verify ToggleFocus behavior" (this cycle; HEAD)

`git status --short` at HEAD `48eb71ce` returns no output — working tree is clean, independently
re-confirmed by this audit.

### Files Modified (cycle-4 delta, `6291bdf6..48eb71ce`)

1. `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` (MODIFIED) — the sole source
   file this cycle; +140/-11 lines net; 497 total lines. Two tests rewired to `BuildExecutingViewer()` and
   extended with state assertions; two new tests added; three new private helper methods added
   (`EnableHandlelessThemeInvoke`, `SetThemeField`, `SetThemeFieldViaActivator`).
2. `docs/features/active/2026-06-29-qfc-item-controller-testability-227/2026-07-02T15-35-remediation/*`
   (NEW) — remediation-inputs and remediation-plan for this cycle.
3. `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/{qa-gates,regression-testing,remediation-baseline}/*`
   (NEW) — cycle-4 baseline and final QA evidence artifacts (12 files).
4. `.claude/agent-memory/atomic-executor/{MEMORY.md,project_qfc227_cycle4_toggle_focus_genuine_test_gotchas.md}`
   (MODIFIED/NEW) — agent-memory bookkeeping, out of audit scope (not source/test/evidence).

No production `.cs` file, `.csproj` file, or `spec.md`/`issue.md` was modified this cycle.

---

## 10. Compliance Verdict

### Overall Status: ✅ FULLY COMPLIANT

Cycle 4 fully resolves the sole material finding carried out of the cycle-3 exit reaudit. Independent
re-verification in this audit — direct re-execution of the 4 named tests and the full `QuickFiler.Test.dll`
suite, a `git diff`-based confirmation of zero production-file changes, a `csharpier check` on the sole
changed file, an exemption-count re-count, a line-by-line source comparison of the new reflection helper
against the production code it unblocks, and an independent re-trace of the `Times.Exactly(2)`
correction's root cause — corroborates every material claim in the delivered cycle-4 evidence with no
discrepancy. The exit condition stated in `remediation-plan.2026-07-02T15-35.md` (`blocking_count == 0` on
the next re-audit) is met.

**Fail-closed reminder honored:** this audit independently re-executed the toolchain-relevant checks
(rather than accepting exit-code claims from evidence markdown alone) before reaching a FULLY COMPLIANT
verdict, and explicitly carries forward the two pre-existing, non-blocking, disclosed gaps (affected-
denominator recompute; stale canonical `coverage.xml`) rather than silently treating them as resolved.

---

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- ✅ Before Making Changes: fully documented, independently spot-checked against evidence.
- ✅ Design Principles: reuses existing helper (`BuildExecutingViewer`), no new abstraction.
- ✅ Module & File Structure: sole changed file is 497 lines (<= 500 cap), independently re-measured.
- ⚠️ Naming, Docs, Comments: two modified tests carry stale names (Minor, non-blocking).
- ✅ Toolchain Execution: green in order, single pass, independently re-confirmed.
- ✅ Summarize & Document: commit message and evidence trail accurate and complete.

#### Language-Specific Code Change Policy (Section 3)
- ✅ Tooling & Baseline: csharpier/analyzers/nullable all green, independently spot-checked.
- ✅ C# Design & Type-Safety: no nullable regressions; deterministic `Activator.CreateInstance` usage.
- ✅ Structure & Naming: consistent with repo conventions (naming staleness noted above is Minor).

#### General Unit Test Policy (Section 1)
- ⚠️ Core Principles: readability/maintainability Minor-partial (2 stale test names).
- ⚠️ Coverage & Scenarios: comprehensive-coverage denominator recompute deferred (carried, non-blocking); state-transition requirement now fully PASS (upgraded from cycle-3).
- ✅ Test Structure: AAA, clear failure messages, documented intent.
- ✅ External Dependencies: no external dependencies; deterministic, independently re-confirmed across two re-runs.
- ✅ Policy Audit: this document.

#### Language-Specific Unit Test Policy (Section 4)
- ✅ Framework & Scope: MSTest/Moq/FluentAssertions used correctly.
- ✅ New code (this cycle's unblocked production lines) ≥ 90%: PASS. Repo-wide floor: unchanged carried authority-scoped exception (improved, not regressed).

---

### Metrics Summary

- ✅ 4442/4442 tests passing (100%), independently spot-re-confirmed (349/349 QuickFiler.Test.dll and 4/4 named tests)
- ✅ No coverage regression on any recomputed metric (whole-process, `QuickFiler.dll`, `UtilitiesCS.dll`)
- ⚠️ `QfcItemController` affected-denominator (77.40%) not recomputed this cycle — carried, non-blocking
- ✅ 24 residual exemptions, unchanged and independently re-confirmed; all 17 cycle-3 de-exemptions now genuinely behavior-verified (upgraded from 15/17 in cycle 3)
- ✅ Proper file organization: sole changed file 497 lines (<= 500 cap)
- ✅ All toolchain code quality checks passing, independently spot-re-confirmed
- ✅ No removed/weakened tests; `Times.Once()`→`Times.Exactly(2)` is a verified correction, not a weakening
- ✅ Working tree committed and clean at HEAD `48eb71ce`, independently re-confirmed

---

### Recommendation

**Ready for merge.** No toolchain, formatting, structural, or reduction-honesty blockers remain. The two
carried Minor items (affected-denominator recompute; stale canonical `coverage.xml`) and the one new Minor
item (two stale test names) are cosmetic/informational and do not block merge. Maintainer ratification of
the 24-member exemption boundary remains an outstanding governance action, tracked separately from this
audit's technical compliance determination.

---

## Appendix A: Test Inventory

Cycle-4 new/modified test methods (all within the pre-existing `QfcItemController_FocusAndThemeTests`
`[TestClass]`):

- `ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke` (MODIFIED — now genuinely executes and
  asserts the On-from-inactive branch)
- `ToggleFocus_StateOverload_Off_FromActive_DeactivatesUiAndSwitchesToNormalTheme` (NEW)
- `ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke` (MODIFIED — now genuinely executes
  and asserts the inactive→active branch)
- `ToggleFocus_ParameterlessOverload_FromActive_DeactivatesUiAndSwitchesToNormalTheme` (NEW)

All 4 independently re-run in isolation by this audit (`vstest.console.exe ... /Tests:...`): `Passed: 4,
Total: 4`. Full inventory (349 QuickFiler.Test + 4093 UtilitiesCS.Test = 4442 total) in
`evidence/qa-gates/final-tests-coverage.2026-07-02T16-25.md`; independently re-confirmed for
`QuickFiler.Test.dll` via a full, unfiltered re-run in this audit (349/349 pass).

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

**Independent verification commands run for this audit (bash tool, Git Bash on Windows):**
```bash
dotnet tool run csharpier check "QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs"

MSYS_NO_PATHCONV=1 MSYS2_ARG_CONV_EXCL="*" \
  "/c/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" \
  "QuickFiler.Test/bin/Debug/QuickFiler.Test.dll" \
  "/Tests:ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke,ToggleFocus_StateOverload_Off_FromActive_DeactivatesUiAndSwitchesToNormalTheme,ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke,ToggleFocus_ParameterlessOverload_FromActive_DeactivatesUiAndSwitchesToNormalTheme" \
  "/InIsolation"

MSYS_NO_PATHCONV=1 MSYS2_ARG_CONV_EXCL="*" \
  "/c/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" \
  "QuickFiler.Test/bin/Debug/QuickFiler.Test.dll" "/InIsolation"

grep -rnE "ExcludeFromCodeCoverage\]" QuickFiler/Controllers/QfcItemController*.cs \
  UtilitiesCS/Threading/WpfUiDispatcher.cs QuickFiler/Viewers/WebView2CoreInitializer.cs \
  QuickFiler/Interfaces/MailItemActionsAdapter.cs | wc -l

git diff --numstat 6291bdf6..48eb71ce -- QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs
git status --short
```

---

**Audit Completed By:** feature-reviewer (Claude)
**Audit Date:** 2026-07-02
**Policy Version:** Current (as of audit date)
