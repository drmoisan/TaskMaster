# Policy Compliance Audit: appevents-loadasync-inbox-gating (Issue #243)

**Audit Date:** 2026-07-06  
**Code Under Test:** full working-tree feature diff relative to `main`; material C# changes are `TaskMaster/AppGlobals/AppEvents.cs`, `TaskMaster.Test/AppGlobals/AppEventsTests.cs`, and `TaskMaster.Test/AppGlobals/HookReadinessCoordinatorTests.cs`. The committed branch tip currently equals `main`, so the audited implementation is the unstaged working-tree diff captured in `artifacts/pr_context.appendix.txt`.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 3 files | 198 full tests after refinement | PASS: `198/198` after refinement; coverage verdict FAIL | 79.9234% repo lines; `AppEvents.cs` 71.5000% | 8.9566% repo lines (FAIL); `AppEvents.cs` 90.7960% | 100.0000% changed executable line coverage |
| TypeScript | 0 files | N/A | N/A | N/A - no changed files | N/A - no changed files | N/A - no changed files |
| Python | 0 files | N/A | N/A | N/A - no changed files | N/A - no changed files | N/A - no changed files |
| PowerShell | 0 files | N/A | N/A | N/A - no changed files | N/A - no changed files | N/A - no changed files |
| Bash | 0 files | N/A | N/A | N/A - no changed files | N/A - no changed files | N/A - no changed files |
| JSON | 0 files | N/A | N/A | N/A - no changed files | N/A - no changed files | N/A - no changed files |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/baseline/baseline-csharp-coverage.2026-07-06T11-02.cobertura.xml`
- C# post-change coverage artifact inspected: `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/final-csharp-coverage.2026-07-06T11-02.cobertura.xml`
- Required C# artifact path from review policy: `artifacts/csharp/coverage.xml` was absent.
- TypeScript baseline coverage artifact: `N/A - no TypeScript files changed`
- TypeScript post-change coverage artifact: `N/A - no TypeScript files changed`
- PowerShell baseline coverage artifact: `N/A - no PowerShell files changed`
- PowerShell post-change coverage artifact: `N/A - no PowerShell files changed`
- Per-language comparison summary: `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/csharp-coverage-delta.2026-07-06T11-02.md`

## Executive Summary

Overall status is **REMEDIATION REQUIRED**. The behavioral fix for issue #243 is supported by focused regression evidence and code inspection: `LoadAsync()` no longer invokes `ProcessNewInboxItemsAsync()` before readiness when events are hooked, and `PerformReadinessHookup()` invokes startup inbox processing after inbox subscriptions are populated.

Policy compliance is not complete. The C# coverage gate fails because final repository-wide coverage is 8.9566%, below the 80% threshold and below the 79.9234% baseline. The required `artifacts/csharp/coverage.xml` coverage path is absent, although feature-folder Cobertura evidence exists and was inspected. Two changed files also exceed the repository 500-line limit: `TaskMaster/AppGlobals/AppEvents.cs` and `TaskMaster.Test/AppGlobals/AppEventsTests.cs` are each 507 lines after the change.

**Policy documents evaluated:**
- PASS: `AGENTS.md`
- PASS: `.agents/skills/policy-compliance-order/SKILL.md`
- PASS: `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`
- PASS: `.agents/skills/feature-review/SKILL.md`
- PASS: `.agents/skills/feature-review-workflow/SKILL.md`
- PASS: `.agents/skills/csharp/SKILL.md`

**Language-specific policies evaluated:**
- PASS: C# code and unit test policies loaded from `AGENTS.md` and `.agents/skills/csharp/SKILL.md`
- N/A: Python
- N/A: PowerShell
- N/A: TypeScript

**Temporary artifacts cleanup:**
- PASS: No temporary one-time scripts were found in the reviewed diff.
- PARTIAL: The post-change coverage evidence records a temporary no-test build-output assembly workaround, removed after command execution.

## Rejected Scope Narrowing

Caller text:

```text
Scope to review:
- TaskMaster/AppGlobals/AppEvents.cs
- TaskMaster.Test/AppGlobals/AppEventsTests.cs
- TaskMaster.Test/AppGlobals/HookReadinessCoordinatorTests.cs
- Feature evidence under docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243
```

Justification: repository review policy requires full feature-vs-base scope. The audit therefore used the full working-tree feature diff and untracked feature evidence, not only the listed paths.

## Evidence Location Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Canonical feature evidence root | PASS | All feature evidence files are under `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/`. |
| Forbidden evidence directories under `artifacts/` | PASS | Direct scan of `artifacts/baselines`, `artifacts/baseline`, `artifacts/qa`, `artifacts/qa-gates`, `artifacts/evidence`, `artifacts/coverage`, `artifacts/regression-testing`, and `artifacts/post-change` found no files. |
| Required validator script | PARTIAL | Command attempted: `python scripts/dev_tools/validate_evidence_locations.py --root .`; result: script path absent in this checkout. A recursive search for `validate_evidence_locations.py` returned no matches. |

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|---|---|---|
| Independence | PASS | MSTest setup/cleanup restores `Settings.Default.EventsHooked`; focused and full test runs passed after refinement. |
| Isolation | PASS | Issue #243 behavior is covered in `AppEventsTests.LoadAsync_WhenEventsHooked_EmitsStartupHookLifecycleLogs` and `HookReadinessCoordinatorTests.Tick_WhenGateBecomesReady_RunsStartupProcessingAfterHookupPopulatesInboxes`. |
| Fast Execution | PASS | Focused affected tests passed `14/14`; full `TaskMaster.Test` passed `198/198`. |
| Determinism | PASS | Tests use Moq and log inspection rather than live Outlook readiness, network, or filesystem dependencies. |
| Readability and maintainability | PARTIAL | Test names are descriptive, but `AppEventsTests.cs` is now 507 lines and exceeds the repo file-size policy. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|---|---|---|
| Baseline Coverage Documented | PASS | Baseline coverage artifact reports repository line coverage 79.9234% and `AppEvents.cs` 71.5000%. |
| No Coverage Regression | FAIL | Coverage delta reports repository coverage fell from 79.9234% to 8.9566%. |
| New/Changed Code Coverage >=90% | PASS | Coverage delta reports 100.0000% changed executable line coverage, 8 of 8 lines covered. |
| Comprehensive Coverage | PARTIAL | Issue behavior is covered; repository-wide coverage gate fails. |
| Positive Flows | PASS | Tests cover post-readiness startup processing path. |
| Negative Flows | PASS | Fail-before artifact captures the pre-fix unwanted pre-readiness processing. |
| Edge Cases | PASS | Coordinator tests cover not-ready, transient COM retry, completed, and non-transient exception paths. |
| Error Handling | PASS | `ProcessStartupInboxItemsAfterReadinessHookup()` observes and logs faulted startup processing tasks. |
| Concurrency | PASS | Coordinator run-once behavior and deferred readiness polling are covered. |
| State Transitions | PASS | `HookReadinessCoordinator` transition tests cover not-ready to ready and completed states. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 79.9234% lines -> Post-change: 8.9566% lines. Change: -70.9668 percentage points. New/changed-code coverage: 100.0000%. Disposition: FAIL due repository-wide threshold and regression. Evidence: `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/csharp-coverage-delta.2026-07-06T11-02.md`.

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|---|---|---|
| Clear Failure Messages | PASS | FluentAssertions messages state the readiness and ordering expectations. |
| Arrange-Act-Assert Pattern | PASS | Reviewed tests use explicit setup, action, and assertions. |
| Document Intent | PASS | Test names describe the scenario under validation. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|---|---|---|
| Avoid External Dependencies | PASS | Tests mock Outlook interfaces and do not require live Outlook. |
| Use Mocks/Stubs | PASS | Moq is used for `IApplicationGlobals`, `IOlObjects`, Outlook `Items`, `Folder`, `Reminders`, and readiness gate seams. |
| Environment Stability | PARTIAL | Unit tests are deterministic; coverage evidence required a tooling workaround and a later broad rerun timed out. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|---|---|---|
| Pre-submission Review | PASS | This artifact records the policy review and remediation triggers. |

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|---|---|---|
| Clarify the objective | PASS | Issue #243 states the startup readiness race and expected behavior. |
| Read existing change plans | PASS | `plan.2026-07-06T11-02.md` exists and is checked complete. |
| Document the plan | PASS | The plan file and checkpoint record execution through P2-T8. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|---|---|---|
| Simplicity first | PASS | The production change keeps processing in `AppEvents` and moves only the invocation point. |
| Reusability | PASS | Existing `ProcessNewInboxItemsAsync()` and readiness hookup path are reused. |
| Extensibility | PASS | No new public API surface was added. |
| Separation of concerns | PASS | Readiness state remains in `HookReadinessCoordinator`; COM hookup remains in `AppEvents`. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|---|---|---|
| Cohesive modules | PASS | Changed files remain in the AppGlobals area. |
| Under 500 lines | FAIL | `AppEvents.cs` is 507 lines; `AppEventsTests.cs` is 507 lines. Both are changed files. |
| Public vs internal | PASS | Added helper is private; no public API expansion. |
| No circular dependencies | PASS | Diff inspection found no new project references or import cycles. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|---|---|---|
| Descriptive names | PASS | `ProcessStartupInboxItemsAfterReadinessHookup` describes the invocation boundary. |
| Docs/docstrings | PASS | No new public APIs requiring XML documentation were added. |
| Comment why, not what | PARTIAL | Existing explanatory comments were removed in the edited test path; test assertions still describe intent through assertion messages. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|---|---|---|
| 1. Formatting | PASS | `dotnet tool run csharpier format .` exited 0 in post-refinement evidence; final plan evidence also records formatting pass. |
| 2. Linting | PASS | Analyzer build exited 0; artifact records existing warnings outside changed lines. |
| 3. Type checking | PASS | Nullable/type-check build exited 0 with 0 warnings and 0 errors. |
| 4. Testing | PASS | Focused tests passed `14/14`; full `TaskMaster.Test` passed `198/198` after refinement. |
| Full toolchain loop | PARTIAL | Core commands passed; coverage gate failed and a broad baseline-comparable rerun timed out. |
| Explicit reporting | PASS | Commands and outcomes are recorded in feature evidence. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|---|---|---|
| Summarize changes | PASS | Feature evidence and this audit summarize the change. |
| Design choices explained | PASS | Issue and evidence describe moving startup processing to readiness hookup. |
| Update supporting documents | PASS | `issue.md`, plan, and evidence artifacts were updated. |
| Provide next steps | PASS | Remediation inputs and plan identify required follow-up. |

## 3. Language-Specific Code Change Policy Compliance

### Section 3CSharp: C# Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Formatting with CSharpier | PASS | Post-refinement evidence: `dotnet tool run csharpier format .`, exit 0. |
| .NET analyzers | PASS | Analyzer command exited 0; existing warnings documented. |
| Nullable analysis | PASS | Nullable/type-check command exited 0 with 0 warnings and 0 errors. |
| Async/resource safety | PASS | Startup processing fault observation was added through completed-task inspection and `ContinueWith(... OnlyOnFaulted ...)`. |
| Public surface | PASS | New helper is private. |
| File size | FAIL | Two changed C# files exceed 500 lines. |

## 4. Language-Specific Unit Test Policy Compliance

### Section 4CSharp: C# Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| MSTest framework | PASS | Tests use `[TestClass]` and `[TestMethod]`. |
| Moq for mocks | PASS | Tests use Moq for Outlook and application seams. |
| FluentAssertions preferred | PASS | New assertions use FluentAssertions; MSTest `Assert.IsNotNull` is used for reflection null check. |
| Coverage expectation | FAIL | Changed-line coverage passes, but repository-wide C# coverage fails policy. |

## 5. Test Coverage Detail

### `AppEvents.LoadAsync()` and readiness startup processing

| Test Name | Scenario Type | Lines Covered | Status |
|---|---|---|---|
| `LoadAsync_WhenEventsHooked_EmitsStartupHookLifecycleLogs` | Regression / ordering | `TaskMaster/AppGlobals/AppEvents.cs` lines 72-92 and readiness hookup invocation path | PASS |
| `Tick_WhenGateBecomesReady_RunsStartupProcessingAfterHookupPopulatesInboxes` | State transition / ordering | `HookReadinessCoordinator` callback sequencing | PASS |

**Coverage:** Changed executable production lines are reported as 100.0000% covered in `csharp-coverage-delta.2026-07-06T11-02.md`.

## 6. Test Execution Metrics

| Metric | Value | Status |
|---|---:|---|
| Focused post-refinement tests | 14 total, 14 passed | PASS |
| Full `TaskMaster.Test` post-refinement tests | 198 total, 198 passed | PASS |
| Planned coverage-enabled test run | 197 total, 197 passed | PASS for tests |
| Baseline repository coverage | 79.9234% | FAIL against 80% policy threshold |
| Final repository coverage | 8.9566% | FAIL |
| Changed executable line coverage | 100.0000% | PASS |
| Changed production file size | `AppEvents.cs` 507 lines | FAIL |
| Changed test file size | `AppEventsTests.cs` 507 lines | FAIL |

## 7. Code Quality Checks

| Check | Command | Result | Status |
|---|---|---|---|
| CSharpier Formatting | `dotnet tool run csharpier format .` | Exit 0 | PASS |
| Analyzer Build | `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild` | Exit 0; existing warnings documented | PASS |
| Nullable Type Check | `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors` | Exit 0; 0 warnings, 0 errors | PASS |
| Focused Tests | `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Tests:TaskMaster.Test.AppGlobals.AppEventsTests,TaskMaster.Test.AppGlobals.HookReadinessCoordinatorTests /InIsolation` | 14 passed | PASS |
| Full Tests | `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation` | 198 passed | PASS |
| Coverage | `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot TaskMaster.Test -Configuration Debug -CoverageOutput ...final-csharp-coverage...xml` | 197 passed; repository coverage 8.9566% | FAIL |
| Whitespace | `git diff --check` | Exit 0 | PASS |
| Evidence-location validator | `python scripts/dev_tools/validate_evidence_locations.py --root .` | Script absent | PARTIAL |

## 8. Gaps and Exceptions

### Identified Gaps

1. C# repository-wide coverage fails the mandatory threshold and regresses against baseline.
2. Required C# coverage artifact `artifacts/csharp/coverage.xml` is absent.
3. `TaskMaster/AppGlobals/AppEvents.cs` and `TaskMaster.Test/AppGlobals/AppEventsTests.cs` exceed 500 lines after the change.
4. Required evidence-location validator script `validate_evidence_locations.py` is unavailable in this checkout.

### Approved Exceptions

None recorded.

### Removed/Skipped Tests

None recorded.

## 9. Summary of Changes

### Commits in This PR/Branch

No committed feature commits are present relative to `main`; `HEAD`, `main`, and merge base are all `961a768e0b093ec468c8180c9dc53996e1e6421a`. The audited implementation exists in unstaged working-tree changes and untracked feature artifacts.

### Files Modified

1. `TaskMaster/AppGlobals/AppEvents.cs` (MODIFIED)
   - Keeps non-hooked startup inbox processing in `LoadAsync()`.
   - Starts hooked-event startup processing from `PerformReadinessHookup()` after `OlInboxes` subscription setup.
   - Observes faulted startup processing tasks.

2. `TaskMaster.Test/AppGlobals/AppEventsTests.cs` (MODIFIED)
   - Adds assertions that hooked `LoadAsync()` does not start inbox processing before readiness hookup.
   - Invokes readiness hookup via reflection to verify post-hookup processing order.

3. `TaskMaster.Test/AppGlobals/HookReadinessCoordinatorTests.cs` (MODIFIED)
   - Adds coordinator sequencing coverage for startup processing after inbox population.

4. `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/**` (NEW)
   - Adds issue, plan, and evidence artifacts for issue #243.

## 10. Compliance Verdict

### Overall Status: NON-COMPLIANT

The behavioral issue appears fixed, but policy compliance fails on mandatory C# coverage and changed-file size. The review therefore requires remediation before PR readiness can be reported.

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- PASS: Before Making Changes
- PASS: Design Principles
- FAIL: Module & File Structure, due changed files over 500 lines
- PARTIAL: Naming, Docs, Comments
- PARTIAL: Toolchain Execution, due coverage failure
- PASS: Summarize & Document

#### Language-Specific Code Change Policy (Section 3)

**For C#:**
- PARTIAL: Tooling & Baseline, due coverage failure
- PASS: C# type safety and API surface
- PASS: Error handling and logging

#### General Unit Test Policy (Section 1)
- PASS: Core Principles
- FAIL: Coverage & Scenarios, due repository-wide coverage failure
- PASS: Test Structure
- PARTIAL: External Dependencies and Environment, due coverage tooling timeout/workaround
- PASS: Policy Audit

#### Language-Specific Unit Test Policy (Section 4)

**For C#:**
- PASS: Framework and libraries
- PASS: Test style and structure
- PASS: Naming and readability
- FAIL: Coverage policy

### Metrics Summary

- PASS: Focused affected tests after refinement: 14/14
- PASS: Full `TaskMaster.Test` after refinement: 198/198
- PASS: Changed executable line coverage: 100.0000%
- FAIL: Repository-wide C# coverage: 8.9566%, below 80% threshold and below 79.9234% baseline
- FAIL: Changed file sizes: `AppEvents.cs` 507 lines; `AppEventsTests.cs` 507 lines
- PASS: `git diff --check`

### Recommendation

**Needs revision.** Remediate the C# coverage gate and reduce changed files under the 500-line limit, then rerun the C# format, analyzer, nullable/type-check, test, coverage, and evidence-location checks.

## Appendix A: Test Inventory

- `TaskMaster.Test.AppGlobals.AppEventsTests.LoadAsync_WhenEventsHooked_EmitsStartupHookLifecycleLogs`
- `TaskMaster.Test.AppGlobals.HookReadinessCoordinatorTests.Tick_WhenGateBecomesReady_RunsStartupProcessingAfterHookupPopulatesInboxes`
- Existing AppEvents and HookReadinessCoordinator tests included in focused affected run

## Appendix B: Toolchain Commands Reference

```powershell
dotnet tool run csharpier format .
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors
vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Tests:TaskMaster.Test.AppGlobals.AppEventsTests,TaskMaster.Test.AppGlobals.HookReadinessCoordinatorTests /InIsolation
vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot TaskMaster.Test -Configuration Debug -CoverageOutput docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/final-csharp-coverage.2026-07-06T11-02.cobertura.xml
git diff --check
python scripts/dev_tools/validate_evidence_locations.py --root .
```

**Audit Completed By:** Codex feature-review  
**Audit Date:** 2026-07-06  
**Policy Version:** Current as of audit date
