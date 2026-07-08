# Policy Compliance Audit: QfcItemController Testability — Cycle-2 Seam Redesign (#227)

**Audit Date:** 2026-07-02
**Code Under Test:** C# only. New: `UtilitiesCS/Threading/IUiDispatcher.cs`, `WpfUiDispatcher.cs`;
`QuickFiler/Viewers/IWebViewCoreInitializer.cs`, `WebView2CoreInitializer.cs`;
`QuickFiler/Interfaces/IMailItemActions.cs`, `MailItemActionsAdapter.cs`; new test files
(`QfcItemController.SeamDispatcherTests.cs`, `SeamCoreTests.cs`, `SeamFactoryTests.cs`,
`WpfUiDispatcherTests.cs`, `WebView2CoreInitializerTests.cs`, `MailItemActionsAdapterTests.cs`,
`InitializationTests.cs`, `ViewerSetupTests.cs`, `EventHandlersTests.cs`, `FocusAndThemeTests.cs`,
`TestSupport.cs`). Modified: the 10 `QuickFiler/Controllers/QfcItemController*.cs` partials,
`QuickFiler/QuickFiler.csproj`, `UtilitiesCS/UtilitiesCS.csproj`, `QuickFiler.Test.csproj`, and four
existing per-cluster test files.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 16 production + ~15 test | 328 tests | ✅ 328 pass, 0 fail | Affected `QfcItemController` non-exempt denom 226/239 = 94.56% (cycle-1 end); repo-wide 13.68% | Affected non-exempt denom 885/1051 = **84.21%**; repo-wide 15.71% | New/extracted seam code **100%** |

**Note:** C# is the only language in scope. No Python, PowerShell, Bash, TypeScript, or JSON files
were changed on this branch (verified from the diff), so those coverage categories are N/A (zero
changed files).

### Coverage Evidence Checklist

- C# coverage artifact (canonical): `artifacts/csharp/coverage.xml` — **present but stale** (cycle-1,
  dated 2026-06-29). Cycle-2 numeric coverage: `artifacts/csharp/coverage-r2-final.cobertura.xml`
  (2026-07-02) with values recorded in `evidence/qa-gates/final-r2-tests-coverage.2026-07-02T10-45.md`.
- C# baseline coverage artifact: `evidence/remediation-baseline/baseline-tests-coverage.2026-07-01T21-37.md`
- C# post-change coverage artifact: `evidence/qa-gates/final-r2-tests-coverage.2026-07-02T10-45.md`
- TypeScript baseline coverage artifact: `N/A - out of scope` (zero changed TypeScript files)
- TypeScript post-change coverage artifact: `N/A - out of scope` (zero changed TypeScript files)
- PowerShell baseline coverage artifact: `N/A - out of scope` (zero changed PowerShell files)
- PowerShell post-change coverage artifact: `N/A - out of scope` (zero changed PowerShell files)
- Per-language comparison summary: `evidence/regression-testing/coverage-delta-r2.2026-07-02T10-45.md`
  and §1.2.1 below.

**Verdict rule note:** Numeric baseline and post-change coverage are present for the only in-scope
language (C#), so the PASS-eligibility precondition is satisfied. The stale canonical `coverage.xml`
is a documentation nit (cycle-2 numeric values exist in the r2 cobertura file and evidence), recorded
below as non-blocking.

---

## Rejected Scope Narrowing

No caller/orchestrator prompt attempted to narrow the audit scope to a plan, task, phase, or file
subset, and none attempted to mark any language's coverage as out of scope. The delegation directed a
full cycle-2 branch-vs-`main` audit, consistent with the scope invariant. **Nothing to reject.**

---

## Evidence Location Compliance

Scanned the working-tree change set for evidence written under non-canonical paths
(`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`). **None exist**
(`ls` of each returned "No such file or directory"). All cycle-2 audit-trail evidence is under the
canonical `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/<kind>/`
scheme (`remediation-baseline/`, `qa-gates/`, `regression-testing/`, `other/`). The coverage XML files
under `artifacts/csharp/` are coverage-tooling outputs (the canonical location for `coverage.xml`), not
evidence artifacts, so they are not a location violation. The repo `validate_evidence_locations.py`
script referenced by the reviewer skill was not found in this checkout; the PreToolUse hook
`.claude/hooks/enforce-evidence-locations.ps1` is present. Manual scan result: **no FAIL-level
evidence-location findings.**

No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` was required (no non-canonical evidence path was supplied in
the delegation).

---

## Executive Summary

Cycle 2 implements the maintainer-approved Option A (`maintainer-decision.2026-07-01.md`): the 103
cycle-1 blanket `[ExcludeFromCodeCoverage]` members are made testable through four narrow behavioral
seams and exemption removal rather than exempted, reducing the residual boundary to **41
individually-justified members** (38 controller members + 3 DI-adapter shims). The full C# toolchain is
green in order (csharpier → analyzers → nullable/TWAE → MSTest+coverage, each `EXIT_CODE 0`), 328/328
tests pass, all modified/created files are < 500 lines, and no leaf-control interface layer (Option B)
was introduced. AC5–AC10 are met.

The one material issue is a **process/merge-readiness gate**: all cycle-2 production, seam, test,
csproj, and evidence files are **uncommitted** in the working tree; the committed branch head
(`bfc8364b`) contains no cycle-2 diff. The technical acceptance criteria all PASS against the delivered
working tree, but the branch cannot merge until the work is committed and the worktree is clean.

**Policy documents evaluated:**
- ✅ `CLAUDE.md` (all sections, incl. C# Code Change Policy, General/C# Unit Test Policy, Tonality)
- ✅ `.claude/rules/general-code-change.md`
- ✅ `.claude/rules/general-unit-test.md`
- ✅ `.claude/rules/csharp.md` (toolchain order, DI-seam ordering, analyzer severity-first invariant, banned symbols)
- ✅ `.claude/rules/tonality.md`
- N/A `.claude/rules/python.md`, `powershell.md`, `typescript.md` (no such files in scope)

**Temporary artifacts cleanup:**
- ✅ No throwaway scripts introduced; test helpers live in committed `QfcItemController.TestSupport.cs`.
- ✅ No temporary files created by tests (verified: dispatcher helpers use parked/running STA threads, not temp files).

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| Independence | ✅ PASS | Each test constructs its own controller via the `HarnessController`/reflection harness; no shared mutable state. The parked-dispatcher helper deliberately isolates fire-and-forget posts so unrelated tests cannot execute them. |
| Isolation | ✅ PASS | Tests target one member/behavior; per-cluster + Seam test files mirror the partials. |
| Fast Execution | ✅ PASS | 328 MSTest cases; no sleeps/retries; dispatcher work is mock-executed or posted to a never-pumped dispatcher. |
| Determinism | ✅ PASS | No network/clock/temp-file dependence; `Mock<IUiDispatcher>` executes delegates synchronously; deferred `Theme` work never pumps. |
| Readability & Maintainability | ✅ PASS | Descriptive names, AAA structure, FluentAssertions with `because` reasons. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| Baseline Coverage Documented | ✅ PASS | `evidence/remediation-baseline/baseline-tests-coverage.2026-07-01T21-37.md`: affected non-exempt denom 226/239 = 94.56%; repo-wide 13.68%; 233 tests. |
| No Coverage Regression (changed lines) | ✅ PASS | `coverage-delta-r2.2026-07-02T10-45.md`: every changed line exercised by a passing test; per-partial figures ≥ 82.47%; no previously-covered non-exempt line lost coverage. |
| New Code Coverage ≥ 90% | ✅ PASS | New/extracted controller code (`WireIntentEvents`, five `*Core`, `HandleWebViewInitializedAsync`) 100%; seam interfaces have no executable lines; adapter shims are exempt forwarders with construction/forwarding smoke tests. |
| Comprehensive Coverage (testable denominator ≥ 80%) | ✅ PASS | Affected non-exempt denominator 885/1051 = **84.21%** (≥ 80). Denominator grew 4.4× as 62 members moved from exempt into the tested set. |
| Positive / Negative / Edge / Error flows | ✅ PASS | Seam routing (positive), resolver-null / empty-collection / cancellation branches (negative/edge), WebView init-failure rethrow (error) covered. |
| Concurrency | N/A | UI-thread marshaling is seam-mocked; no shared-state concurrency logic added. |
| State Transitions | ✅ PASS | `_activeUI`/`_activeTheme` and `_expanded` transitions tested (e.g., `ToggleFocusOnAsync`/`OffAsync`). |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 94.56% lines (affected non-exempt denom 226/239) -> Post-change: 84.21% lines (885/1051). Change: denominator +812 lines (62 members moved from exempt into the tested set), affected coverage remains above the 80% floor. New/changed-code coverage: 100%. Disposition: PASS (affected >= 80%, new >= 90%, no changed-line regression; repo-wide floor under the #223 authority-scoped exception, uplift tracked under #197). Evidence: `evidence/regression-testing/coverage-delta-r2.2026-07-02T10-45.md`.
- Python / PowerShell / TypeScript / Bash / JSON: `N/A - out of scope` (zero changed files on the branch).

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| Clear Failure Messages | ✅ PASS | FluentAssertions + `because` reasons on reflection lookups. |
| Arrange-Act-Assert | ✅ PASS | Explicit Arrange/Act/Assert blocks in all reviewed test files. |
| Document Intent | ✅ PASS | Descriptive method names and class-level XML docs (e.g., `FocusAndThemeTests`). |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| Avoid External Dependencies | ✅ PASS | No DB/network/Outlook/live-control dependence; all boundaries seam-mocked. |
| Use Mocks/Stubs | ✅ PASS | `Mock<IItemViewer>`, `Mock<IUiDispatcher>`, `Mock<IWebViewCoreInitializer>`, `Mock<IMailItemActions>`, injected `Func<>` factories, reflection-injected `_themes`/`_kbdHandler`. |
| Environment Stability (no temp files) | ✅ PASS | No temp files; dispatcher helpers use background STA threads reclaimed at process exit. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| Pre-submission Review | ✅ PASS | This document is the cycle-2 policy review. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| Clarify the objective | ✅ PASS | Driven by `maintainer-decision.2026-07-01.md` + `spec.md` v0.3 + `remediation-inputs.2026-07-01T00-30.md`. |
| Read existing change plans | ✅ PASS | `remediation-plan.2026-07-01T00-30.md` (Phases 0/5/6/7/8) executed. |
| Document the plan | ✅ PASS | Plan + per-phase evidence under `evidence/`. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| Simplicity first | ✅ PASS | Minimal purpose-specific seams; adapters are 1:1 forwarders; factory delegates instead of full interfaces where construction is the only concern. |
| Reusability | ✅ PASS | Shared `QfcItemControllerTestSupport` harness avoids reflection copy-paste. |
| Extensibility | ✅ PASS | Optional defaulted constructor parameters extend the API without breaking the 8 existing call sites. |
| Separation of concerns | ✅ PASS | UI-thread marshaling, WebView SDK init, and Outlook COM isolated behind distinct seams. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| Cohesive modules | ✅ PASS | One seam per boundary; controller partials keep their cluster responsibilities. |
| Under 500 lines | ✅ PASS | Independently re-measured: all 10 partials < 500 (max `Initialization.cs` = 446); 6 seam files 30–49; `IItemViewer.cs` = 120. Largest touched test file 340. `QfcCollectionController.cs` (2296) and `QfcFormControllerTests.cs` (821) are pre-existing and NOT modified this cycle. |
| Public vs internal | ✅ PASS | Seam interfaces `public` (cross-project); adapters `sealed`; controller stays `internal partial`. |
| No circular dependencies | ✅ PASS | Seams live in `UtilitiesCS`/`QuickFiler`; no new cycles introduced. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| Descriptive names | ✅ PASS | `IUiDispatcher`, `WireIntentEvents`, `BtnReplyCore`, etc. |
| Docs/docstrings | ✅ PASS | XML docs on all new public seam types/members. |
| Comment why, not what | ✅ PASS | Each residual exemption carries a specific per-member technical justification comment. |

### 2.5 After Making Changes — Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| 1. Formatting (csharpier) | ✅ PASS | `evidence/qa-gates/final-r2-csharpier.2026-07-02T10-45.md` EXIT_CODE 0 |
| 2. Linting (.NET analyzers) | ✅ PASS | `final-r2-analyzers.2026-07-02T10-45.md` EXIT_CODE 0, 0 errors |
| 3. Type checking (nullable/TWAE) | ✅ PASS | `final-r2-nullable.2026-07-02T10-45.md` EXIT_CODE 0 |
| 4. Testing (MSTest + coverage) | ✅ PASS | `final-r2-tests-coverage.2026-07-02T10-45.md` 328/328 pass, EXIT_CODE 0 |
| Full toolchain loop | ✅ PASS | Per-phase (p5r/p6r/p7r) + final gates all EXIT_CODE 0. |
| Explicit reporting | ✅ PASS | Commands + exit codes recorded in each evidence file. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| Summarize changes | ✅ PASS | `evidence/other/ac-traceability-r2.2026-07-02T10-45.md`, exemption-delta, coverage-delta. |
| Design choices explained | ✅ PASS | Seam shapes and Option-B decline documented in spec §Redesign + exemption boundary. |
| Update supporting documents | ✅ PASS | `spec.md` v0.3 updated; exemption boundary re-submitted for ratification. |
| Provide next steps | ⚠️ PARTIAL | Ratification note present, but the delivered work is not yet committed (see Gaps §8). |

---

## 3. Language-Specific Code Change Policy Compliance

C# is the only in-scope language. Python/PowerShell/Bash/JSON/TypeScript sections deleted (zero changed files).

### Section 3C-sharp: C# Code Change Policy Compliance

### 3.C1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| Formatting with CSharpier | ✅ PASS | `final-r2-csharpier` EXIT_CODE 0 |
| Linting with .NET analyzers | ✅ PASS | `final-r2-analyzers` EXIT_CODE 0. New IDE0005/readonly/simplify diagnostics are **suggestion** severity (do not break TWAE build); non-blocking, see Gaps §8. |
| Type checking (nullable, TWAE) | ✅ PASS | `final-r2-nullable` EXIT_CODE 0 |

### 3.C2 Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| Strong contracts / explicit APIs | ✅ PASS | Seam interfaces expose explicit, minimal contracts with XML docs. |
| Null-safety by default | ✅ PASS | Nullable build clean; `??=` seam defaults + `_itemViewer is not null` guards. |
| Composition & focused types | ✅ PASS | Composition via injected seams; no new inheritance beyond test subclasses. |
| Async/await & resource safety | ✅ PASS | `InvokeAsync`/`Task`-returning seam members; WebView handler preserves try/catch. |

### 3.C3–C7 (interfaces, error handling, structure, naming, dependencies)

| Requirement | Status | Evidence |
|------------|--------|----------|
| Interfaces when multiple implementations expected | ✅ PASS | Each seam has a production adapter + test mock. |
| Fail-fast error handling | ✅ PASS | WebView handler rethrows on init failure; no new broad catches. |
| File-scope explicit usings; no new cycles | ✅ PASS | Explicit usings retained; no circular deps. |
| PascalCase/camelCase conventions | ✅ PASS | Verified across seam/controller code. |
| No unapproved dependencies | ✅ PASS | No new packages; DI-seam ordering (interface > delegate > adapter) followed; `Microsoft.Bcl.TimeProvider` not required. |
| Banned symbols | ✅ PASS (no new) | Relocated `Task.Delay` in `HandleWebViewInitializedAsync` is pre-existing and held at `suggestion` per `BannedSymbols.txt` policy; no new banned-symbol call introduced. |

---

## 4. Language-Specific Unit Test Policy Compliance

C# is the only in-scope language with tests.

### Section 4C-sharp: C# Unit Test Policy Compliance

| Requirement | Status | Evidence |
|------------|--------|----------|
| MSTest framework | ✅ PASS | `[TestClass]`/`[TestMethod]` throughout. |
| Moq for mocking | ✅ PASS | `Mock<IItemViewer>`, `Mock<IUiDispatcher>`, etc. |
| FluentAssertions | ✅ PASS | `.Should()...` used across new tests. |
| New code ≥ 90%, repo-wide floor | ✅ PASS (with authority-scoped repo-wide exception) | New/extracted 100%; repo-wide under #197/#223 precedent. |
| No weakened/removed tests | ✅ PASS | 0 removed `[TestMethod]`; no `[Ignore]`/`Assert.Inconclusive`; +95 net tests. |

---

## 5. Test Coverage Detail

### Behavioral seams (adapter smoke + routing tests)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| `WpfUiDispatcherTests` / `WebView2CoreInitializerTests` / `MailItemActionsAdapterTests` | Positive (construction/forwarding) | adapter forwarders | ✅ |
| `SeamDispatcherTests` (`Mock<IUiDispatcher>`) | Positive/Edge (dispatched behavior + invocation) | dispatcher-routed members | ✅ |
| `SeamCoreTests` (`*Core`, `HandleWebViewInitializedAsync`) | Positive/Error (routing + WebView init-failure rethrow) | extracted cores 100% | ✅ |
| `SeamFactoryTests` (injected `Func<>`) | Positive (factory called + result applied) | factory-routed members | ✅ |

**Coverage:** new/extracted controller code 100%; seam interfaces have no executable lines; adapter
shims are exempt forwarders with smoke tests.

### De-exempted cluster members (Phase 5)

| Test Area | Scenario Type | Status |
|-----------|--------------|--------|
| Initialization ctors + `SaveParameters` | Positive (field delegation, `VerifySet`) | ✅ |
| FocusAndTheme (`ToggleNavigation`/`ToggleTips`/`SetThemeDark`/`SetThemeLight`/`InvokeBeginInvoke`/mouse handlers) | Positive/Edge | ✅ |
| Conversation / FolderHandling / EventWiring registration / Navigation | Positive/Negative/Edge | ✅ |

**Not covered (justified residuals):** 41 members carry `[ExcludeFromCodeCoverage]` with per-member
technical reasons (concrete control-tree orchestration, out-of-scope `Theme`/`FolderPredictor`/
`TlpCellSnapShot` collaborators, deliberate virtual seams, thin `async void` shells, DI-adapter shims).
See `evidence/qa-gates/p7r-residual-verification.2026-07-02T10-30.md`.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 328 | ✅ |
| Tests Passed | 328 (100%) | ✅ |
| Tests Failed | 0 | ✅ |
| Affected non-exempt denom coverage | 84.21% (≥80) | ✅ |
| New/extracted-code coverage | 100% (≥90) | ✅ |
| Exemptions (QfcItemController scope) | 103 → 41 | ✅ |
| Largest changed production file | `QfcItemController.Initialization.cs` = 446 lines | ✅ |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `dotnet tool run csharpier .` | No format drift (EXIT_CODE 0) | ✅ |
| .NET Analyzers | `msbuild TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Build succeeded, 0 errors; residual IDE0005/style diagnostics at suggestion severity | ✅ |
| Nullable / TWAE | `msbuild TaskMaster.sln /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | Build succeeded, 0 errors | ✅ |
| MSTest + Coverage | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage` | 328/328 pass | ✅ |

**Notes:** Suggestion-level analyzer diagnostics do not break the `TreatWarningsAsErrors` build and are
non-blocking per the `.claude/rules/csharp.md` severity-first invariant (see Gaps §8, item 4). No
pre-existing test failures observed.

---

## 8. Gaps and Exceptions

### Identified Gaps

1. **[MATERIAL — process/merge gate] Cycle-2 work is uncommitted.** Committed HEAD `bfc8364b`
   (cycle-1) has no cycle-2 diff; all cycle-2 production/seam/test/csproj/evidence files are
   modified/untracked in the working tree (`git status --short`). The delivered content satisfies all
   ACs, but the branch cannot merge until it is committed and `git status` is clean. **Remediation:**
   commit the full cycle-2 change set, then re-run the final toolchain against the committed head.
   Artifact paths: all files listed under "Code Under Test" above.
2. **[Minor, non-blocking] `ApplyReadEmailFormat` interleaves seam-testable statements with an exempt
   member.** `QfcItemController.FocusAndTheme.cs:347–348` (`_mailActions.UnRead=false; _mailActions.Save()`)
   use the cycle-introduced seam but sit in a wholly-exempt method whose `Theme.SetMailRead` call (line
   346) is the genuine barrier. **Remediation (optional):** extract the two seam writes into a tested
   `MarkMailReadCore()`. Not an AC failure: the member as written faults at line 346, so it is not
   end-to-end executable, and `Theme` is an out-of-scope collaborator this cycle.
3. **[Minor, non-blocking] Stale canonical coverage artifact.** `artifacts/csharp/coverage.xml` is
   cycle-1 (2026-06-29); cycle-2 coverage is `coverage-r2-final.cobertura.xml`. **Remediation:** emit
   the canonical `coverage.xml` from the r2 run.
4. **[Nit, non-blocking] Suggestion-level analyzer debt.** IDE0005 unnecessary-usings and related
   style suggestions on the controller partials (mostly pre-existing cycle-1 copy-paste of the 22-line
   using block; some usings possibly rendered dead by cycle-2 `UiThread.Dispatcher`/`Mail.*` removal).
   Suggestion severity does not break the TWAE build (`EXIT_CODE 0`). **Remediation:** clean dead usings
   in touched partials as follow-up.
5. **[Info] Theme-seam follow-up not recorded.** The exemption boundary records a `TlpCellSnapShot`
   follow-up but not a `Theme` seam, though five residuals (`ToggleFocus`×2, `ToggleFocusAsync`×2,
   `ApplyReadEmailFormat`) are `Theme`-handle-bound. **Remediation (optional):** record a named
   `Theme`-seam follow-up analogous to `TlpCellSnapShot`.

### Approved Exceptions

- **Repo-wide 80% floor — authority-scoped exception.** Repo-wide C# line coverage (15.71%) is below
  80%; this is handled under the maintainer-ratified authority-scoped precedent from #223
  (`docs/features/active/2026-06-28-qfc-form-viewer-testability-223/maintainer-decision.2026-06-29.md`),
  with residual uplift tracked under #197. In-scope affected-denominator coverage (84.21%) meets the floor.
- **Residual 41-member `[ExcludeFromCodeCoverage]` boundary — pending ratification.** Each residual is
  individually justified (no blanket/category exemption); submitted for maintainer ratification per
  `evidence/other/exemption-boundary.2026-07-02T10-30.md`.

### Removed/Skipped Tests

**None.** 0 removed `[TestMethod]`; no test weakened or skipped.

---

## 9. Summary of Changes

### Commits in This PR/Branch

- Committed HEAD `bfc8364b` — "docs(#227): cycle-0 audits and cycle-1 R1 canonical coverage evidence".
- Cycle-2 (Phases 5–8) delivery is **uncommitted** in the working tree (see Gaps §8, item 1). No cycle-2
  commit exists yet.

### Files Modified (working tree)

1. `UtilitiesCS/Threading/IUiDispatcher.cs`, `WpfUiDispatcher.cs` (NEW) — UI-dispatch seam + adapter.
2. `QuickFiler/Viewers/IWebViewCoreInitializer.cs`, `WebView2CoreInitializer.cs` (NEW) — WebView2 core-init seam + adapter.
3. `QuickFiler/Interfaces/IMailItemActions.cs`, `MailItemActionsAdapter.cs` (NEW) — Outlook COM seam + adapter.
4. `QuickFiler/Controllers/QfcItemController*.cs` (10 partials, MODIFIED) — seam/factory constructor
   params, dispatcher/COM migration, `async void` thin-delegator split, `WireEvents` split, exemption removal.
5. `QuickFiler/QuickFiler.csproj`, `UtilitiesCS/UtilitiesCS.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj`
   (MODIFIED) — explicit `<Compile Include>` entries for new files.
6. Test files (NEW/MODIFIED) — `Seam*Tests`, adapter smoke tests, per-cluster test files, `TestSupport.cs`.
7. `spec.md` (MODIFIED) — v0.3 redesign scope + AC5/AC8/AC9/AC10; plus cycle-2 evidence artifacts under `evidence/`.

---

## 10. Compliance Verdict

### Overall Status: ⚠️ PARTIALLY COMPLIANT (technically PASS; one process/merge gate outstanding)

All C# code-change, unit-test, toolchain, coverage, exemption-justification, and tonality requirements
are met for the delivered working-tree content: 103→41 exemption reduction with per-member
justification, four seams introduced per DI-seam ordering, atomic COM/dispatcher migration, preserved
event-wiring order, 328/328 tests, green toolchain, and all files < 500 lines. Option B was correctly
not introduced. The residuals are genuinely barrier-bound (verified: the de-exempted `SetThemeDark`
defers via `async:true`, while exempt `ToggleFocus` synchronously faults on the handle-less `Theme`).

The one material item is the **uncommitted delivery**: the reviewed cycle-2 changes must be committed
(clean `git status`) before the branch can merge.

### Metrics Summary

- ✅ 328/328 tests passing (100%)
- ✅ Affected non-exempt denominator 84.21% (≥ 80%); new/extracted code 100% (≥ 90%)
- ✅ Exemptions reduced 103 → 41, each individually justified
- ✅ All modified/created files < 500 lines
- ✅ Full C# toolchain green in order (EXIT_CODE 0 each)
- ⚠️ Cycle-2 changes uncommitted (process/merge gate)

### Recommendation

**Conditional Go — commit required.** No acceptance-criteria or code-quality blockers. The mandatory
pre-merge action is committing the delivered working-tree changes and confirming a clean worktree;
optionally address the four non-blocking refinements above. The reduced exemption boundary is ready for
maintainer ratification.

**Policy-audit AC/policy blocking-finding count: 0.** One material process/merge-readiness gate
(uncommitted delivery) must be cleared before merge; it is not an acceptance-criteria failure.

---

## Appendix A: Test Inventory

New/updated cycle-2 test classes (MSTest `[TestClass]`), each mirroring a controller cluster or seam:

- `QfcItemController_TestSupportSmokeTests` › `InjectThemes_ThenActiveThemeRead_ReturnsInjectedInstance`
- `QfcItemController_FocusAndThemeTests` › ToggleFocusOn/OffAsync, ToggleNavigation×2, ToggleNavigationAsync,
  ToggleTips(Async), InvokeBeginInvoke×2, ToggleSaveAttachments, SetThemeDark/Light, HtmlDarkConverter
- `QfcItemController_InitializationTests` › ctor field delegation, `SaveParameters` assignment/`VerifySet`
- `QfcItemController_ViewerSetupTests` › AssignControls / Cleanup / AssignControlsAsync
- `QfcItemController_EventHandlersTests` › checkbox/search/topic-thread/mouse handlers
- Seam suites: `SeamDispatcherTests`, `SeamCoreTests`, `SeamFactoryTests`
- Adapter smoke: `WpfUiDispatcherTests`, `WebView2CoreInitializerTests`, `MailItemActionsAdapterTests`
- Updated per-cluster: `ConversationTests` (+11), `EventWiringTests` (+7), `MailActionsTests` (+7), `NavigationTests` (+10)

Total suite: 328 tests, 328 passing (full inventory in `evidence/qa-gates/final-r2-tests-coverage.2026-07-02T10-45.md`).

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
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage
```

---

**Audit Completed By:** feature-reviewer (Claude)
**Audit Date:** 2026-07-02
**Policy Version:** Current (as of audit date)
