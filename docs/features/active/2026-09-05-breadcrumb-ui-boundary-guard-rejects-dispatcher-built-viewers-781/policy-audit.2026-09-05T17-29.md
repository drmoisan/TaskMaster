# Policy Compliance Audit: breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers (#781)

**Audit Date:** 2026-09-05

**Code Under Test:** `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` (modified, production);
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` (added, test);
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` (modified, test);
`QuickFiler.Test/QuickFiler.Test.csproj` (modified, build configuration).

- Review timestamp: 2026-09-05T17-29
- Reviewer: feature-review agent
- Feature folder: `docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781`
- Branch under review: `bug/breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781`
- Head SHA: `4f74aa39799dca7233bcf286dac90e8691eabd99`
- Resolved base branch: `main` (equals `origin/main`)
- Merge base: `a007f72e394ee3038c6c52bfdf91f007df96fd6c`
- Audit range: `a007f72e394ee3038c6c52bfdf91f007df96fd6c..4f74aa39799dca7233bcf286dac90e8691eabd99`
- Work mode: `minor-audit` (marker `- Work Mode: minor-audit` present in `issue.md`)
- Acceptance-criteria source: the `## Acceptance Criteria` section of `issue.md` (AC1 through AC8)

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 4 files | 6997 tests | 6997 pass, 0 fail | 84.8347% lines, 79.1542% branches | 84.8316% lines, 79.1421% branches | 100% |
| PowerShell | 0 files | 0 tests | N/A | N/A - out of scope | N/A - out of scope | N/A |
| Python | 0 files | 0 tests | N/A | N/A - out of scope | N/A - out of scope | N/A |
| TypeScript | 0 files | 0 tests | N/A | N/A - out of scope | N/A - out of scope | N/A |

**Unit of the C# New Code Coverage figure, and the deviation it carries.** The 100% figure is
measured in *reachable outcomes of the rewritten guard*, 3 of 3, each covered by a named passing
test that the reviewer executed. It is deliberately not stated as a line percentage, because no
line percentage exists for the changed production lines: `QuickFiler/Viewers/ItemViewer.cs` line 20
carries `[ExcludeFromCodeCoverage]` on the `ItemViewer` partial class declaration, which applies to
the whole type, so `ItemViewer.Breadcrumb.cs` emits zero `<class>` elements in the Cobertura
document. The reviewer confirmed this by enumerating every `<class filename=...>` in both the
baseline and the post-change document and finding zero matches in each, so the property is
pre-existing rather than introduced by this branch. The exemption is the maintainer-ratified
CLAUDE.md UT2(b) WinForms form-derived exemption. Full treatment in section 5.3 and finding F2.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `coverage/baseline-781.cobertura.xml`
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

---

## Executive Summary

The branch replaces a `SynchronizationContext` reference-equality UI-boundary guard in
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` with an owner-thread identity check via
`Dispatcher.CheckAccess()` on the dispatcher captured in the `ItemViewer` constructor, deletes the
two tests that encoded the superseded comparison, and adds a seven-test class that reproduces the
production construction shape.

Verdict: **PASS**. Blocking findings: **0**. Acceptance criteria: **8 of 8 PASS** (AC8 PASS with a
recorded deviation). Remediation is **not** required and no `remediation-inputs` artifact is
emitted.

**Policy documents evaluated:**

- PASS `CLAUDE.md` (all sections)
- PASS `.claude/rules/general-code-change.md`
- PASS `.claude/rules/general-unit-test.md`
- PASS `.claude/rules/quality-tiers.md`
- PASS `.claude/rules/tonality.md`

**Language-specific policies evaluated:**

- PASS C#: CLAUDE.md C# Code Change Policy and C# Unit Test Policy, plus `.claude/rules/csharp.md`
- N/A Python, PowerShell, TypeScript, Bash, JSON: no file of those languages is in the branch diff

The review did not accept the executor's evidence at face value. Every load-bearing claim was
re-measured in this session:

- The full branch diff was recomputed with `git diff --numstat` rather than read from the PR
  context summary, which misclassifies the four changed C# and project files as documentation
  (see Finding F4).
- Formatting was re-run over the two changed directories.
- The test project was rebuilt and the four affected test classes were re-executed (33 of 33
  passed), reproducing the executor's post-change result test by test.
- The RED-first claim was independently reproduced by restoring the pre-fix production file from
  the merge base, rebuilding, and re-running the new class: 5 failed, 2 passed, matching the
  executor's `regression-fail-before` artifact test by test. The production file was then restored
  and confirmed byte-identical by SHA-256 (`A0088EE6...5CF71813`).
- The Cobertura documents were diffed class by class in this session, which contradicted the
  causal explanation the executor recorded in `coverage-delta` (see Finding F1).

**Temporary artifacts cleanup:**

- PASS All temporary scripts created during this review live outside the repository, in the
  session scratchpad, and none was committed. The reviewer created three: a Cobertura class-by-class
  diff script, a hook-simulation script, and a copy of the pre-fix production file used as the RED
  probe input.
- PASS The RED probe modified one tracked file temporarily and restored it. `git status` reports no
  modification to any tracked source path, and the restored file's SHA-256 matches the committed
  content.
- PASS No file was left behind under `artifacts/` by this review.

### Method deviations recorded for this review

1. **MCP template asset resolution.** The first draft of this audit was written without the
   canonical template, because `mcp__drm-copilot__resolve_policy_audit_template_asset` was not on
   this agent's tool surface at that time. The orchestrator subsequently resolved the asset and
   supplied it, and this document was repaired in place against it. All canonical major headings,
   the coverage-metrics-by-language table with the template's seven-column header, the Coverage
   Evidence Checklist, section 1.2.1, and both appendices are now present, and the template
   instruction block is removed.
2. **`validate_orchestration_artifacts` is not on this agent's tool surface.** The orchestrator runs
   it. Substitute check performed locally by the reviewer: the `SubagentStop` hook
   `.claude/hooks/validate-feature-review-coverage.ps1` was dot-sourced and executed against the
   three artifacts this review produced, including a stress run with the real coverage figures
   forced in. Result in Appendix B.
3. **`validate_evidence_locations.py` does not exist in this repository.** Substitute: a direct
   `git diff --name-only <merge-base>..HEAD` scan for the four non-canonical `artifacts/` evidence
   prefixes was run. Result in the Evidence Location Compliance section.

## Rejected Scope Narrowing

No caller instruction narrowed the audit scope, and none was accepted. The delegating prompt
states "Scope determination is yours per your scope invariant." The audit was performed against
the full branch diff of 44 paths.

Two statements in the supplied inputs were evaluated as candidate narrowings and both were found
**not** to be narrowings of the audit:

1. `plan.2026-09-05T10-49.md` line 21 records that removing the class-level exemption on the
   `ItemViewer` type lies outside this issue's implementation write set. That constrains the fix.
   It does not instruct any reviewer to skip a check, and the executor produced complete
   repository-wide measurement regardless. The reviewer independently measured the same figures.
2. `evidence/other/reduced-audit-handoff.2026-09-05T10-49.md` enumerates a "reduced artifact set"
   for the `minor-audit` short path. That enumeration selects which evidence documents the audit
   consumes; it does not restrict which files are diffed. The full 44-path diff was audited, and
   two branch paths absent from that enumeration — the nine `.claude/agent-memory/**` files and the
   deleted `artifacts/orchestration/orchestrator-state.json` — were audited on their own merits and
   produced Finding F5.

## Evidence Location Compliance

Scan command:
`git diff --name-only a007f72e..4f74aa39 | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"`

Result: no matching path. **PASS.**

All executor evidence is written under the canonical `<FEATURE>/evidence/<kind>/` tree:
`baseline/` (9 files), `regression-testing/` (2), `qa-gates/` (11), `issue-updates/` (1),
`other/` (4). No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` event occurred during this review; the three
artifacts produced here are written to the feature folder root as the agent contract requires.

Absolute-host-path scan over all 43 added and modified tracked paths in the range
(`C:\Users\<user>` and account-name tokens): **0 hits. PASS.** The one tracked file that did
contain an absolute host path with the OS account name,
`artifacts/orchestration/orchestrator-state.json`, is deleted by this branch (see Finding F5).

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | PASS | Every new test either owns its `ViewerScope` or constructs its viewer inside the test body; every ambient `SynchronizationContext` substitution is restored in a `finally`. No static mutable state is shared across tests. |
| **Isolation** - Each test targets single behavior | PASS | Each of the seven new tests targets one guarded member under one ambient-context shape. |
| **Fast Execution** - Tests complete quickly | PASS | Re-measured in this session: the four affected classes complete in 2.60 s for 33 tests; the seven new tests total 1.47 s, dominated by the first WinForms `InitializeComponent()` at 950 ms. |
| **Determinism** - Consistent results | PASS | Five independent executions across the executor's runs and this session, all agreeing test by test. No sleeps, timers, wall-clock waits, or temporary files. One theoretical inlining dependency is recorded as code-review finding CR-5 (Low) and was not reproduced. |
| **Readability & Maintainability** - Clear structure | PASS | Every test carries an XML `<summary>`; the two non-obvious shapes carry a `<remarks>` explaining why the repeat-call form is required rather than stylistic. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | PASS | **Baseline (pre-development):** 84.8347% lines, 79.1542% branches, `lines-covered` 54922, `lines-valid` 64740, 9 instrumented assemblies.<br>**Command:** repository-wide `Invoke-MSTestWithCoverage.ps1` run producing `coverage/baseline-781.cobertura.xml`.<br>**Timestamp:** 2026-09-05 (task P0-T8/P0-T9).<br>Re-read directly from the document by the reviewer, not transcribed from the executor's artifact. |
| **No Coverage Regression** | FAIL, non-blocking | **Post-change coverage:** 84.8316% lines, 79.1421% branches.<br>**Change:** -0.0031% lines, -0.0121% branches.<br>**Status:** No regression attributable to this branch. Denominators are identical at `lines-valid` 64740 and assembly count 9. A class-by-class diff of the two documents found 3 differing classes out of 564, all in untouched `UtilitiesCS`. Every `QuickFiler` class is identical and the `QuickFiler` package counters are unchanged at LINE missed=2376 covered=9960. The row is recorded FAIL only because the absolute figure 84.8316% sits below the 85% uniform floor; disposition in section 1.2.2. |
| **New Code Coverage >= 90%** | PASS with deviation | **New/modified production files:** `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`.<br>**New code coverage:** 100%, measured as 3 of 3 reachable outcomes of the rewritten guard, each covered by a named passing test.<br>**Calculation method:** the changed lines are outside the instrumented denominator because the enclosing type carries `[ExcludeFromCodeCoverage]`, so no line percentage exists. The reviewer enumerated the guard's reachable outcomes from the source, mapped a named test to each, executed all seven tests, and observed five of them fail against the pre-fix guard and pass against the fixed guard. Detail in section 5.3 and finding F2. |
| **Comprehensive Coverage** | PASS | **Changed member tested:**<br>- `ThrowIfOffUiBoundary(string)` (lines 428-444): 7 tests across 3 outcomes<br>- Guard call sites reached: `InitializeBreadcrumbPipeline` (line 51), both `ConfigureBreadcrumbDropDown` overloads (lines 172, 229), `EnsureBreadcrumbResourceOwnership` (line 389, reached through `EnsureBreadcrumbLifecycle` at line 361)<br>**Untested code:** none added by this branch. |
| **Positive Flows** - Valid inputs | PASS | Three ambient-context shapes on the owning thread, all expected to succeed: `InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext`, `InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow`, `InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow`, plus `ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow`.<br>**Total positive tests:** 4 |
| **Negative Flows** - Invalid inputs | PASS | Cross-thread calls must be rejected: `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` and `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic`, each asserting `InvalidOperationException` whose message contains the member name.<br>**Total negative tests:** 2 |
| **Edge Cases** - Boundary conditions | PASS | `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` nulls the private `_uiDispatcher` field by reflection, while asserting the field still exists so a rename fails loudly, and confirms the guard stays inert.<br>**Total edge case tests:** 1 |
| **Error Handling** - Error paths | PASS | Both cross-thread tests assert the exception type, that the message names the operation, and explicitly that it is not an `ObjectDisposedException`, which keeps the issue #488 D5 disposal contract distinguishable from the boundary contract.<br>**Total error handling tests:** 2 |
| **Concurrency** - If applicable | PASS | Applicable and covered at the level the contract permits. The guard is a precondition check, not a synchronization primitive; its own documentation states it declares and enforces the contract without making the read-then-write atomic. Cross-thread rejection is tested with a real second thread. A true two-thread data race is not reproducible deterministically under the repository ban on sleeps and wall-clock waits, and the tests correctly do not claim to prove race absence. |
| **State Transitions** - If applicable | PASS | The relevant transition is uninitialized to initialized pipeline. `InitializeBreadcrumbPipeline_RepeatSameProvider_DoesNotThrowAndKeepsCoordinator` and `InitializeBreadcrumbPipeline_SecondDifferentProvider_ThrowsInvalidOperationException` cover the repeat and conflicting-provider transitions and were re-executed passing by the reviewer. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 84.8347% lines (54922/64740) -> Post-change: 84.8316% lines (54920/64740). Change: -0.0031% lines (-2 covered lines, identical denominator). New/changed-code coverage: 100%. Disposition: FAIL. Evidence: `coverage/baseline-781.cobertura.xml`, `artifacts/csharp/coverage.xml`, `<FEATURE>/evidence/qa-gates/coverage-delta.2026-09-05T10-49.md`, `<FEATURE>/evidence/qa-gates/changed-code-coverage.2026-09-05T10-49.md`.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.

### 1.2.2 Disposition of the C# Coverage Row

The FAIL disposition above is **non-blocking**. Three reasons, each verified in this session:

1. **Pre-existing.** The baseline measurement at merge base `a007f72e` is 84.8347%, already below
   85%. The shortfall predates the branch and is not caused by it.
2. **Governing-floor conflict, both floors reported.** `CLAUDE.md` UT2 sets the repository floor at
   `>= 80%`; `.claude/rules/quality-tiers.md` sets it at `>= 85%` uniformly across T1-T4. The two
   documents are unreconciled in this repository. The measured 84.8316% clears the first and misses
   the second. This audit reports against the stricter of the two and therefore records FAIL, so
   that the row is not silently upgraded by choosing the looser document.
3. **No attributable movement.** A class-by-class diff of the two Cobertura documents, performed by
   the reviewer, found exactly three differing classes out of 564, all in `UtilitiesCS` and none
   touched by this branch: `UtilitiesCS.OlTableExtensions` (0.885522 -> 0.912458, improved),
   `UtilitiesCS.HelperClasses.SegmentStopWatch` (1.0 -> 0.944954), and `UtilitiesCS.SubjectMapSco`
   (0.969466 -> 0.938931). The -2 movement is run-to-run nondeterminism in unrelated code.

No `remediation-inputs` artifact is emitted on this row. The correct disposition is procedural
rather than a code defect in this change.

The new/changed-code figure of 100% is stated in outcomes rather than lines for the reason given
beside the metrics table and expanded in section 5.3 and finding F2.

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | PASS | Every FluentAssertions call in the new file supplies a `because` string. The non-vacuity assertion states the failure meaning directly: "the dispatcher operation must have captured a context that is not the thread's ambient context, or this test would pass vacuously". |
| **Arrange-Act-Assert Pattern** | PASS | All seven new tests carry literal `// Arrange`, `// Act`, `// Assert` comments in that order, with the act phase captured into an `Action` and asserted through `.Should().Throw<...>()` or `.Should().NotThrow(...)`. |
| **Document Intent** | PASS | Test names encode member, thread, ambient shape, and expectation, for example `InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow`. The class carries a `<summary>` naming issue #781 and a `<remarks>` explaining why it declares its own scope helpers rather than widening the accessibility of the sibling file's private helpers. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | PASS | No Outlook, network, database, remote API, or external process. No filesystem access. The only host facility used is the WPF `Dispatcher` for the current thread, which is in-process. |
| **Use Mocks/Stubs** | PASS | `IFolderHierarchyProvider` is a `MockBehavior.Strict` Moq double, so any unexpected call fails the test. `InertDropDownHost` is a hand-written `IBreadcrumbDropDownHost` fake. `DrainableSynchronizationContext` queues posted work so nothing escapes the test thread. |
| **Environment Stability** | PASS | Grepped the new test file for `GetTempPath`, `GetTempFileName`, and `TempDirectory`: 0 hits, so the prohibition on temporary files is met. Ambient `SynchronizationContext` is the only global touched, and every substitution is restored in a `finally` or in `ViewerScope.Dispose`. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | PASS | This document is the required pre-submission policy review. Outstanding review items are the four non-blocking residuals enumerated in section 10; none gates the PR. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| Objective clarified from stated assumptions | PASS | `issue.md` states the root cause with a runtime probe (`evidence/other/dispatcher-synccontext-probe.2026-09-05T10-40.md`) whose decisive line is `Invoke ctx == outer ambient : False`, and explicitly rejects an earlier competing hypothesis with reasons. |
| Existing change plan read | PASS | `plan.2026-09-05T10-49.md` version 1.3 is the approved plan of record; Phase 0 task P0-T1 records the mandated policy reading order and the nine files read. |
| Plan documented before execution | PASS | The plan predates the fix commit and carries a preflight revision record at `evidence/other/preflight-round1-revisions.2026-09-05T19-59.md`. |
| Bugfix workflow, failing regression test first | PASS | Independently reproduced by the reviewer. See section 6. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| Simplicity first | PASS | Net production change is 24 added and 17 removed lines; the guard body shrank by one statement while the documentation grew. |
| Reusability | PASS | The single private `ThrowIfOffUiBoundary` remains the one implementation shared by all four guard sites, so the fix applies at every site without duplication. |
| Extensibility | PASS | No public signature changed. `UiSyncContext` is retained for its existing consumers across the `QuickFiler` controllers. |
| Separation of concerns | PASS | The change is confined to a precondition check. No I/O and no control manipulation were added. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| File size limit, 500 lines | PASS | Re-measured with `awk END{print NR}`: `ItemViewer.Breadcrumb.cs` 456, `ItemViewerBreadcrumbThreadAffinityTests.cs` 419, `ItemViewerBreadcrumbLifecycleRegressionTests.cs` 388. All below 500. |
| Module cohesion | PASS | The new test file holds one test class plus three private test-support types, all serving that class. |
| Small, intentional public surface | PASS | No member was promoted in visibility. The new test file declares its own scope helpers precisely so that the sibling file's `private` helpers are not widened, which the class `<remarks>` states explicitly. |
| Explicit imports, no circular dependencies | PASS | `System.Windows.Threading` replaces the now-unused `System.Threading` in the changed production file. |
| I/O boundaries | PASS | No new I/O. |
| Dependencies | PASS | No package reference added. `WindowsBase` was already referenced by the `QuickFiler` project, and the constructor already captured `Dispatcher.CurrentDispatcher` before this branch. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| Descriptive naming | PASS | `PascalCase` types and members, `camelCase` locals throughout the diff. The local `owning` names the captured dispatcher by its role. |
| Public API documentation | PASS | `ThrowIfOffUiBoundary` carries a rewritten `<summary>` and `<remarks>`. |
| Comment why, not what | PASS | The rewritten `<remarks>` explains the dispatcher-context reason the previous comparison was unsuitable. One accuracy note is recorded as code-review finding CR-4 (Low), which does not change this verdict. |
| Dependent comments kept accurate | PASS | The `EnsureBreadcrumbResourceOwnership` statement-order comment was updated in the same commit: it now reasons about the owning dispatcher being null rather than about `UiSyncContext` being null, so the FIRST STATEMENT and FIRST ACTION argument it documents remains true after the swap. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| Full toolchain run in order | PASS | Format, then lint and analyzers, then type and nullable check, then tests. Section 7 records each command and exit code. |
| One consecutive passing pass | PASS | No stage modified a file and no stage failed, so no restart from step 1 was required. |
| Reviewer re-execution | PASS | The reviewer independently re-ran the format stage over the two changed directories and the test stage over the four affected classes. Both passed. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| Key changes summarized | PASS | `evidence/other/implementation-handoff.2026-09-05T10-49.md` and section 9 of this document. |
| Supporting documents updated | PASS | `issue.md` acceptance criteria checked off; `plan.2026-09-05T10-49.md` task checkboxes updated; `evidence/issue-updates/issue-781.2026-09-05T10-49.md` mirrors the issue update. |
| Next steps provided | PASS | `evidence/other/reduced-audit-handoff.2026-09-05T10-49.md` enumerates the review inputs and names the single reviewer-judgment item, which this audit resolves in finding F2. |

---

## 3. Language-Specific Code Change Policy Compliance

Applicable language: C#. Python, PowerShell, Bash, and JSON sections are omitted because no file of
those languages appears in the branch diff.

### Section 3C#: C# Code Change Policy Compliance

#### 3C#.1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| CSharpier formatting through `dotnet tool run` | PASS | Executor: `dotnet tool run csharpier check .` exit 0, `Checked 71772 files`, zero needing formatting. Reviewer re-ran `dotnet tool run csharpier check ./QuickFiler/Viewers ./QuickFiler.Test/Viewers`: `Checked 89 files in 614ms`, exit 0. |
| `dotnet format` not used | PASS | Absent from every recorded command. No project file was rewritten; the only `.csproj` change is one added `<Compile Include>` line. |
| Analyzers with `/t:Rebuild` | PASS | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, exit 0, 3 warnings and 0 errors, equal to baseline. The gate is non-vacuous: the log records 36 `csc.exe` command lines. The three warnings are all `MSB3061` `CoreClean` file-deletion diagnostics caused by a running Outlook process holding output binaries; they are present identically at baseline and no analyzer rule identifier appears. |
| Nullable and type check, correct command shape | PASS | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`, exit 0, 3 warnings and 0 errors, equal to baseline. `/p:Nullable=enable` correctly omitted and `/t:Build` correctly not substituted, so the gate could have failed. |

#### 3C#.2 C# Design & Type-Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| Strong contracts and explicit APIs | PASS | The guard's contract is now stated as owner-thread identity in the XML documentation and enforced by one expression. The local is explicitly typed `Dispatcher owning`, not `var`. |
| Null-safety | PASS | The null-owner early return is retained and re-keyed onto the dispatcher. No new nullable-flow warning was introduced, confirmed by the `TreatWarningsAsErrors` build. |
| Composition and focused types | PASS | No type was added to production code. |
| Asynchrony and resource safety | PASS | No async or disposable surface changed. |

#### 3C#.3 C# Error Handling and Suppressions

| Requirement | Status | Evidence |
|------------|--------|----------|
| Fail fast with explicit exceptions | PASS | `InvalidOperationException` is thrown with a message naming the operation; no broad catch was added. |
| No new suppressions | PASS | No `#pragma warning disable` and no new `[ExcludeFromCodeCoverage]` appears anywhere in the diff. The pre-existing class-level attribute on `ItemViewer` was neither added nor altered by this branch. |

---

## 4. Language-Specific Unit Test Policy Compliance

Applicable language: C#. Python and PowerShell sections are omitted because no test of those
languages appears in the branch diff.

### Section 4C#: C# Unit Test Policy Compliance

#### 4C#.1 Framework and Scope

| Requirement | Status | Evidence |
|------------|--------|----------|
| MSTest framework | PASS | `[TestClass]` and `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`; no xUnit or NUnit reference introduced. |
| Moq for mocking | PASS | `new Mock<IFolderHierarchyProvider>(MockBehavior.Strict)` in six of the seven new tests. |
| FluentAssertions for assertions | PASS | Every assertion in the new file uses `.Should()`. No MSTest `Assert.` call appears in the added code. |

#### 4C#.2 Test Style and Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| Sealed test class, no shared fixture state | PASS | `public sealed class ItemViewerBreadcrumbThreadAffinityTests` with no `[ClassInitialize]` or static field. |
| Deterministic disposal | PASS | `ViewerScope` implements `IDisposable` and is used in `using` blocks; the one test that constructs its viewer outside a scope disposes it in a `finally`. |

#### 4C#.3 Naming and Readability

| Requirement | Status | Evidence |
|------------|--------|----------|
| Behavioural test names | PASS | Names follow member, condition, expectation. See Appendix A. |

#### 4C#.4 Running the Toolchain

| Requirement | Status | Evidence |
|------------|--------|----------|
| `vstest.console.exe` with `/EnableCodeCoverage` | PASS | Section 7. |
| Test project registration | PASS | `QuickFiler.Test/QuickFiler.Test.csproj` gains exactly one `<Compile Include="Viewers\ItemViewerBreadcrumbThreadAffinityTests.cs" />`. Verified the new class is discovered and executed, 7 tests, in this session's own run. |

---

## 5. Test Coverage Detail

### 5.1 Coverage Artifact Resolution

- **C#** — 4 changed files (three `.cs`, one `.csproj`). Canonical artifact `artifacts/csharp/coverage.xml` is **present**, Cobertura format, root
  `<coverage line-rate="0.848316" branch-rate="0.791421" lines-covered="54920" lines-valid="64740" branches-covered="13174" branches-valid="16646">`,
  12,732,521 bytes, mtime 2026-09-05 17:10. Baseline counterpart `coverage/baseline-781.cobertura.xml` is present.
- **PowerShell** — 0 changed files; `artifacts/pester/powershell-coverage.xml` not required for this branch.
- **Python** — 0 changed files; `artifacts/python/lcov.info` not required for this branch.
- **TypeScript** — 0 changed files; `coverage/lcov.info` not required for this branch.

### 5.2 Repository-Wide C# Figures

All figures re-read by the reviewer directly from the Cobertura documents on disk, not transcribed
from the executor's artifacts:

| Metric | Baseline | Post-change | Floor | Row verdict |
|---|---|---|---|---|
| Repository-wide line rate | 0.848347 | 0.848316 | >= 0.85 | FAIL |
| Repository-wide branch rate | 0.791542 | 0.791421 | >= 0.75 | PASS |
| `lines-covered` | 54922 | 54920 | — | -2 |
| `lines-valid` | 64740 | 64740 | — | 0, denominators identical |
| Instrumented assemblies | 9 | 9 | equal | PASS |
| Regression on changed lines | — | none; the `QuickFiler` package counters are identical in both documents | none | PASS |

Disposition is recorded in section 1.2.2.

### 5.3 Per-File Coverage of New and Modified Code

| File | Classification | Coverage evidence |
|---|---|---|
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | modified, production | Outside the measured denominator. The reviewer independently enumerated every `<class>` element in both documents and found zero whose `filename` ends `ItemViewer.Breadcrumb.cs`, and likewise zero for `ItemViewer.cs`. The cause is `[ExcludeFromCodeCoverage]` on the `ItemViewer` partial class declaration at `QuickFiler/Viewers/ItemViewer.cs` line 20, which applies to the whole type including the members declared in this file. The attribute predates the branch, since the same enumeration returns zero on the baseline document, and it is the maintainer-ratified CLAUDE.md UT2(b) WinForms form-derived exemption. Substitute behavioural evidence is complete and was independently verified; see finding F2 and section 6. |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` | new, test | Test assembly, excluded from the production denominator by design. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 99 injects the `.*\.Test\.dll$` module exclusion into the settings at run time, and `Invoke-MSTestWithCoverage.Helpers.ps1` lines 24-46 omit `.Test`-suffixed assemblies from the allowlist so `ConvertTo-KoverageCoberturaXml` strips them from both numerator and denominator. Confirmed empirically: no `.Test` package appears among the nine packages in either document. |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | modified, test | As above. |
| `QuickFiler.Test/QuickFiler.Test.csproj` | modified, build configuration | One `<Compile Include>` line; no executable line. |

---

## 6. Test Execution Metrics

**Suite executions, executor and reviewer:**

- Baseline full suite at merge base — source `<FEATURE>/evidence/baseline/mstest-coverage.2026-09-05T10-49.md`: 6992 total, 6992 passed, 0 failed, 0 skipped, exit 0.
- RED, new class only, pre-fix guard — source `<FEATURE>/evidence/regression-testing/regression-fail-before.2026-09-05T10-49.md`: 7 total, 2 passed, 5 failed, 0 skipped, exit 1 as expected.
- RED reproduced by the reviewer — this session: 7 total, 2 passed, 5 failed, 0 skipped, exit 1.
- GREEN, four affected classes — source `<FEATURE>/evidence/regression-testing/regression-pass-after.2026-09-05T10-49.md`: 33 total, 33 passed, 0 failed, 0 skipped, exit 0.
- GREEN reproduced by the reviewer — this session: 33 total, 33 passed, 0 failed, 0 skipped, exit 0.
- GREEN, whole `QuickFiler.Test` assembly — source `<FEATURE>/evidence/qa-gates/mstest-quickfiler.2026-09-05T10-49.md`: all passed, 0 failed, 0 skipped, exit 0.
- Final repository-wide suite — source `<FEATURE>/evidence/qa-gates/mstest-coverage.2026-09-05T10-49.md`: 6997 total, 6997 passed, 0 failed, 0 skipped, exit 0.

The suite count moved 6992 to 6997, exactly seven added less two deleted.

**RED-first is proven, not asserted.** The reviewer restored
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` from merge base `a007f72e` into the working tree,
rebuilt `QuickFiler.Test` (exit 0, so a runtime red rather than a compile red), and re-ran the new
class. Per-test outcome across the guard swap, as observed by the reviewer:

- `InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext` — pre-fix Failed, post-fix Passed
- `InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow` — pre-fix Failed, post-fix Passed
- `InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow` — pre-fix Failed, post-fix Passed
- `ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow` — pre-fix Failed, post-fix Passed
- `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` — pre-fix Failed, post-fix Passed
- `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` — pre-fix Passed, post-fix Passed
- `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` — pre-fix Passed, post-fix Passed

Five tests discriminate; the two cross-thread tests corroborate that the fail-fast contract was
preserved rather than weakened. The production file was restored afterwards and its SHA-256
confirmed equal to the committed content; `git status` reports no modification to any tracked
source path.

---

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| Format check | `dotnet tool run csharpier check .` | Exit 0, zero files needing formatting. Executor, `evidence/qa-gates/csharpier-check.2026-09-05T10-49.md` |
| Format check (reviewer re-run) | `dotnet tool run csharpier check ./QuickFiler/Viewers ./QuickFiler.Test/Viewers` | Exit 0, `Checked 89 files in 614ms`. This session |
| Analyzer build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Exit 0, 3 warnings equal to baseline, 0 errors, 36 `csc.exe` invocations. Executor, `evidence/qa-gates/msbuild-analyzers.2026-09-05T10-49.md` |
| Nullable build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | Exit 0, 3 warnings equal to baseline, 0 errors. Executor, `evidence/qa-gates/msbuild-nullable.2026-09-05T10-49.md` |
| Test run | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation` | Exit 0. Executor, `evidence/qa-gates/mstest-quickfiler.2026-09-05T10-49.md` |
| Test run (reviewer re-run) | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation` with a four-class filter | Exit 0, 33 of 33 passed. This session |
| Repository-wide collection | throwaway session script over `Invoke-MSTestWithCoverage.ps1` | Exit 0, Cobertura written, 9 assemblies. Executor, `evidence/qa-gates/mstest-coverage.2026-09-05T10-49.md` |
| File size audit | `awk END{print NR}` over the three changed source files | 456, 419, 388; all below the 500-line limit |
| Evidence location scan | `git diff --name-only a007f72e..4f74aa39` filtered for non-canonical `artifacts/` evidence prefixes | Zero matches |
| Host path scan | grep for account-name and `C:\Users` tokens across all added and modified tracked paths | Zero matches |
| Workflow change scan | `git diff --name-only` filtered for `.github/workflows/`, `.github/actions/`, `scripts/benchmarks/` | Zero matches; the modified-workflow green-run rule is not triggered |
| Suppression scan (added lines) | grep for `#pragma warning disable` and `ExcludeFromCodeCoverage` in the diff | Zero additions |

Formatting, linting, type checking, and tests all passed in one consecutive pass with no file
modified by any stage, so no restart of the loop was required.

---

## 8. Gaps and Exceptions

### Identified Gaps

#### F1 — `coverage-delta` misattributes the coverage movement — **PARTIAL, non-blocking**

`evidence/qa-gates/coverage-delta.2026-09-05T10-49.md` states the two-line movement "is consistent
with the deletion of the two obsolete D4 tests in
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs`: those tests drove the
old reference-comparison throw path". That explanation is not supported by the documents it cites,
and it is self-contradictory: the same artifact establishes that the old reference-comparison throw
path lives in a type outside the denominator, so exercising it or not cannot move any counter.

Reviewer measurement: the `QuickFiler` package counters are identical in both documents
(LINE missed=2376 covered=9960); all three differing classes are in `UtilitiesCS` and none is
touched by this branch. The movement is run-to-run nondeterminism in unrelated code.

A second, smaller citation defect appears in the same artifact: it states that test projects are
excluded from instrumentation "by `coverage.config`". The committed `coverage.config` contains only
seven third-party `ModulePath` excludes and no test-assembly entry; the `.*\.Test\.dll$` exclusion
is injected at run time by `Invoke-MSTestWithCoverage.ps1` line 99. The substantive effect the
artifact relies on is real, but the citation names the wrong mechanism.

Impact: the artifact's conclusion, that no regression is attributable to this change, is correct
and is in fact strengthened by the corrected measurement, so this changes no verdict. The defect is
in the recorded rationale. The orchestrator has appended a "Correction recorded by the
orchestrator" section to that artifact carrying this finding. Not blocking.

#### F4 — PR context summary misclassifies the changed C# files — **PARTIAL, tooling defect, non-blocking**

`artifacts/pr_context.summary.txt` is fresh, since its Head SHA `4f74aa39...` equals current `HEAD`,
but its "Changed files overview" reports "Core logic changes: 0 files" and places all changes under
"Docs/templates/agents/tooling: 30 files", listing only ten Markdown paths. The branch actually
changes 44 paths including three `.cs` files and one `.csproj`.

Consequence for enforcement: `.claude/hooks/validate-feature-review-coverage.ps1` derives its
changed-language set from that summary's `- <path> (+N/-N)` bullets, so it computes an empty
language set and skips its per-language checks entirely. This audit therefore does **not** rely on
the summary for scope. Scope was recomputed with
`git diff --numstat a007f72e394ee3038c6c52bfdf91f007df96fd6c..4f74aa39799dca7233bcf286dac90e8691eabd99`,
and the verdicts in sections 1.2.1 and 5.2 are written as if the hook were enforcing them.

This is a recurring defect in the context-collection tooling, not a defect in this branch. It is
recorded here so the branch is not credited with a check that was never actually exercised.

#### F5 — Unrelated tracked-file deletion bundled into the fix commit — **Minor, non-blocking**

Commit `4f74aa39` deletes `artifacts/orchestration/orchestrator-state.json`, 386 lines, alongside
the guard fix. The file is stale checkpoint state from an unrelated feature, since its `objective`
names PR #704 CI format recovery and its `workspace_root` names a different worktree; it sits under
a path matched by `.gitignore` line 57; and it contained an absolute host path including the OS
account name. Removing it from tracking is beneficial, and the local working copy is intact on disk
at 24,709 bytes with mtime 2026-09-05 17:20, so no orchestration state was lost.

The objection is traceability only: the deletion is unmentioned in the commit subject
`fix(781): use dispatcher CheckAccess for breadcrumb UI-boundary guard` and lies outside the AC7
write set. Recommendation: mention it in the PR body. Not blocking.

#### F6 — Latent defect recorded only as feature-folder prose — **Minor, action recommended before merge**

`issue.md` records, under "Related observations, not in scope for the fix", that
`UtilitiesCS/Threading/UiThread.cs` line 100, `SynchronizationContextAwaiter.IsCompleted`, uses the
same reference comparison, so `await viewer.UiSyncContext` always posts rather than continuing
inline for a dispatcher-built viewer. The reviewer confirmed the code at that location is unchanged
by this branch and the described behaviour is accurate. This is an extra dispatch hop, not a
failure.

Feature-folder prose does not survive merge. Recommendation: promote this observation to a GitHub
issue through the promotion lifecycle before the feature folder is archived. The same applies to
the `BreadcrumbUiDispatcher.IsCurrentBoundary` divergence recorded as code-review finding CR-7.

**Status: discharged.** The orchestrator promoted the observation to issue #784, recorded at
`docs/features/potential/promoted/2026-09-05-uithread-synccontext-awaiter-always-posts-for-dispatcher-built-viewers.md`.
The finding stands as recorded; only its recommended action is now complete.

### Approved Exceptions

#### F2 — AC8's 90% new-code clause is unevaluable as a line percentage — **PASS with recorded deviation**

No line percentage exists for the changed production lines, because `ItemViewer` carries
`[ExcludeFromCodeCoverage]`. The clause is therefore answered by substitute behavioural evidence,
reported as 100% of reachable guard outcomes in the metrics table and in section 1.2.1.

The reviewer judges the substitute sufficient, on these grounds:

1. The rewritten method has exactly two conditionals and three reachable outcomes: null owner
   returns; non-null owner with `CheckAccess()` true returns; non-null owner with `CheckAccess()`
   false throws. A named, passing test exists for each — seven tests across the three outcomes.
2. Five of those tests were independently observed to fail against the pre-fix guard and pass
   against the fixed guard in this session, so they discriminate on the changed behaviour rather
   than merely execute it.
3. The exemption is pre-existing, applies to the whole type, and is the maintainer-ratified
   CLAUDE.md UT2(b) WinForms form-derived exemption. Removing it would be a materially larger
   refactor of a WinForms `UserControl` and is properly a separate work item.

Deviation recorded so a maintainer can overrule: if the maintainer requires a measured line
percentage rather than branch-complete behavioural evidence, the only remedy is to extract the
guard logic out of the `UserControl`-derived type into a host-neutral class. That is a separate
issue and not a defect in this change.

#### F3 — Repository-wide C# line coverage below the 85% uniform floor — **FAIL, non-blocking**

Recorded and disposed in section 1.2.2, with the figures in section 5.2. Pre-existing at the merge
base, not attributable to this branch, and clears the 80% floor stated in `CLAUDE.md`. The
80-versus-85 conflict between `CLAUDE.md` UT2 and `.claude/rules/quality-tiers.md` remains
unreconciled at the repository level and is outside this branch's remit.

### Removed/Skipped Tests

Two tests were removed, both deliberately and both correctly:

- `InitializeBreadcrumbPipeline_AmbientContextNull_ThrowsBoundaryDiagnostic`
- `InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic`

Both asserted the defective behaviour directly, so retaining them would require them to fail. Their
replacements invert the expectation for the same two ambient shapes and add the dispatcher-operation
shape that neither original covered, so net coverage of the guard's contract increased. No test was
skipped, marked `[Ignore]`, or otherwise disabled by this branch.

### UNVERIFIED items

1. **Manual Outlook verification of the production launch path.** `issue.md` lists manual
   verification, launching QuickFiler in standard and high-confidence mode, among the validation
   ideas; it is unchecked and no evidence artifact records it. This review cannot execute it,
   because it requires a live Outlook session with a mailbox producing folder suggestions. The
   reason is environmental and is not a gap in the executor's work. The unit-level evidence
   reconstructs the production construction shape, a viewer built inside a `Dispatcher.Invoke`
   callback with the pipeline initialized afterwards under a different ambient context, which is
   the exact shape the stack trace in `issue.md` shows failing.
2. **CI green run against the branch head.** GitHub CLI is unavailable in this environment and the
   PR context reports "CI status (HEAD): (not available)". The repository-wide local suite is green
   at 6997 of 6997. The modified-workflow green-run rule is not triggered by this branch, so no
   Blocking finding follows from this.

---

## 9. Summary of Changes

### Commits in This Branch

- `ef0b5253` — `docs(781): promote breadcrumb UI-boundary guard defect and open active bug folder`, 4 files, +361
- `4f74aa39` — `fix(781): use dispatcher CheckAccess for breadcrumb UI-boundary guard`, 42 files, +2500 / -534

### Files Modified

44 paths in range `a007f72e..4f74aa39`, recomputed with `git diff --numstat`:

| Group | Paths | Lines |
|---|---|---|
| Production C# | 1, `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | +24 / -17 |
| Test C# | 2, `ItemViewerBreadcrumbThreadAffinityTests.cs` new and `ItemViewerBreadcrumbLifecycleRegressionTests.cs` modified | +422 / -95 |
| Build configuration | 1, `QuickFiler.Test/QuickFiler.Test.csproj` | +1 / -0 |
| Feature-folder documents and evidence | 30 | +1996 / -0 |
| Promoted potential-feature record | 1 | +127 / -0 |
| Agent memory | 9 | +146 / -2 |
| Deleted orchestration state | 1 | +0 / -386 |

Behavioural change: `ThrowIfOffUiBoundary` proves UI ownership with `UiDispatcher.CheckAccess()`
instead of `ReferenceEquals(SynchronizationContext.Current, UiSyncContext)`. The null-owner early
return is retained, now keyed on a null dispatcher rather than a null context. The exception type
and the first sentence of its message are unchanged; the second sentence now states thread identity
rather than a context comparison.

---

## 10. Compliance Verdict

### Overall Status: PARTIALLY COMPLIANT — PASS with one non-blocking coverage FAIL row

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)

PASS. Design principles, file-size limit, naming, documentation accuracy, and the mandatory
toolchain loop in order are all met, with the format and test stages independently re-run by the
reviewer.

#### Language-Specific Code Change Policy (Section 3)

PASS for C#. CSharpier via `dotnet tool run`, `/t:Rebuild` analyzer and nullable builds with the
exact CI command shapes, no `dotnet format`, no `/p:Nullable=enable`, and no new suppression.

#### General Unit Test Policy (Section 1)

PASS on principles, structure, scenarios, and environment. The coverage subsection carries one FAIL
row for the repository-wide C# line figure, disposed non-blocking in section 1.2.2.

#### Language-Specific Unit Test Policy (Section 4)

PASS for C#. MSTest, Moq, and FluentAssertions as required; the new test class is registered and
was observed executing.

### Metrics Summary

| Area | Verdict |
|---|---|
| General Unit Test Policy | PASS |
| General Code Change Policy | PASS |
| C# Code Change Policy | PASS |
| C# Unit Test Policy | PASS |
| Coverage, C# | FAIL on the repository-wide row, non-blocking; no regression attributable to this branch |
| Coverage, PowerShell / Python / TypeScript | PASS, zero changed files each |
| Evidence location compliance | PASS |
| Acceptance criteria | 8 of 8 PASS, with AC8 PASS subject to the deviation recorded as F2 |
| Blocking findings | 0 |
| Remediation required | No |

### Recommendation

**GO for PR.**

Residual items owed, none blocking:

1. **Discharged.** Correct the causal attribution and the `coverage.config` citation in
   `evidence/qa-gates/coverage-delta.2026-09-05T10-49.md`. The orchestrator appended a "Correction
   recorded by the orchestrator" section carrying the reviewer's class-by-class finding; the
   reviewer verified that section reproduces both corrected facts accurately.
2. **Discharged.** Promote the `UtilitiesCS.UiThread.SynchronizationContextAwaiter.IsCompleted`
   observation from `issue.md` prose to a GitHub issue (F6, code-review CR-8). Promoted as issue
   #784.
3. Mention the `artifacts/orchestration/orchestrator-state.json` deletion in the PR body (F5,
   code-review CR-6).
4. Optionally remove the unused `DrainableSynchronizationContext.Drain()` and restate the
   `ThrowIfOffUiBoundary` `<remarks>` thread-id sentence (code-review CR-3, CR-4).

---

## Appendix A: Test Inventory

### Complete Test List

New file `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`, seven tests:

| Test | Shape | Guarded member | Expected |
|---|---|---|---|
| `InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext` | viewer built inside `Dispatcher.Invoke`, called later under a plain ambient context | `InitializeBreadcrumbPipeline` | no throw |
| `InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow` | owning thread, ambient context nulled | `InitializeBreadcrumbPipeline` | no throw, coordinator preserved |
| `InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow` | owning thread, foreign plain context installed | `InitializeBreadcrumbPipeline` | no throw |
| `ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow` | owning thread inside a dispatcher operation | `ConfigureBreadcrumbDropDown`, three-argument overload | no throw |
| `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` | `Task.Run` worker | `InitializeBreadcrumbPipeline` | `InvalidOperationException` naming the operation, not `ObjectDisposedException` |
| `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` | `Task.Run` worker | `ConfigureBreadcrumbDropDown`, three-argument overload | `InvalidOperationException` naming the operation, not `ObjectDisposedException` |
| `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` | `_uiDispatcher` nulled by reflection, called from a worker | `InitializeBreadcrumbPipeline` | no throw, guard inert |

Test support types added in the same file: `InertDropDownHost`, an `IBreadcrumbDropDownHost` fake;
`DrainableSynchronizationContext`, which queues posted work; and `ViewerScope`, which installs a
plain ambient context, constructs a real `ItemViewer`, and restores the previous context on
disposal.

Deleted from `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs`, two tests,
both of which encoded the superseded reference comparison and would otherwise assert the defect:

- `InitializeBreadcrumbPipeline_AmbientContextNull_ThrowsBoundaryDiagnostic`
- `InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic`

Retained and re-verified passing in this session, covering the issue #488 D3 and D5 behaviours:
`InitializeBreadcrumbPipeline_SecondDifferentProvider_ThrowsInvalidOperationException`,
`InitializeBreadcrumbPipeline_AfterViewerDisposed_ThrowsObjectDisposedException`,
`InitializeBreadcrumbPipeline_RepeatSameProvider_DoesNotThrowAndKeepsCoordinator`,
`ConfigureBreadcrumbDropDown_EnvironmentChange_DisposesOutgoingHostBeforeReplacement`,
`LegacySurfaceFactoryConstructor_AmbientContextNull_ThrowsInvalidOperationException`, and
`ConfigureBreadcrumbDropDown_SeededLifecycleNullUiContext_DoesNotThrow`.

---

## Appendix B: Toolchain Commands Reference

Commands executed by the reviewer in this session. All are check-only except the temporary,
fully-restored RED probe:

```
# Scope and compliance scans
git diff --numstat a007f72e394ee3038c6c52bfdf91f007df96fd6c..4f74aa39799dca7233bcf286dac90e8691eabd99
git diff --name-only a007f72e..4f74aa39 | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"

# Formatting
dotnet tool run csharpier check ./QuickFiler/Viewers ./QuickFiler.Test/Viewers

# Build
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU

# Testing
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~ItemViewerBreadcrumbThreadAffinityTests|FullyQualifiedName~ItemViewerBreadcrumbLifecycleRegressionTests|FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests|FullyQualifiedName~QfcItemControllerBreadcrumbDropDownTests"

# RED probe, applied and fully restored
git show a007f72e:QuickFiler/Viewers/ItemViewer.Breadcrumb.cs
git checkout HEAD -- QuickFiler/Viewers/ItemViewer.Breadcrumb.cs
Get-FileHash QuickFiler\Viewers\ItemViewer.Breadcrumb.cs -Algorithm SHA256
```

`vstest.console.exe` was resolved through `vswhere.exe -latest -products *` to the
`Common7\IDE\Extensions\TestPlatform\vstest.console.exe` binary, which preserves the assembly
binding redirects. The `TestWindow` sibling was not used.

Repository-standard C# toolchain, for reference, as executed by the executor and recorded in
`<FEATURE>/evidence/qa-gates/`:

```
# Formatting
dotnet tool run csharpier check .

# Linting
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

# Type checking
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true

# Testing
vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /InIsolation
```

Local artifact validation performed by the reviewer in place of the orchestrator-side MCP
validator: the `SubagentStop` hook `.claude/hooks/validate-feature-review-coverage.ps1` was
dot-sourced and `Invoke-FeatureReviewCoverageValidation` was executed against a synthetic payload
advertising the three artifact paths this review produced. It returned `Ok = True`. A stress run
additionally forced the real figures (84.83 line, 79.14 branch) into `Test-LanguageCoverageRow` for
C#, PowerShell, Python, and TypeScript, and every row returned `Ok = True`.
