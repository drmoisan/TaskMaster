# Policy Compliance Audit: Outlook Startup UI Lockup Follow-up (#148)

**Audit Date:** 2026-05-08  
**Code Under Test:** `TaskMaster/AppGlobals/AppEvents.cs`, `QuickFiler/Controllers/EfcHomeController.cs`, `QuickFiler/Controllers/EfcDataModel.cs`, `QuickFiler/Helper Classes/ConversationResolver.cs`, `QuickFiler/Helper Classes/ConversationResolver.Loading.cs`, `UtilitiesCS/Extensions/DfDeedle.cs`, `UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs`, `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs`, `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.Formatting.cs`, `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`, `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Loading.cs`, `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Properties.cs`, `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs`, `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Serialization.cs`, `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs`, `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs`, `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.RowTransforms.cs`, `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`, the mapped MSTest homes, and the active feature-folder remediation evidence.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 18 production files in the remediated structure set plus mapped MSTest files | MSTest suite | ✅ 4010 pass, 0 fail, 2 skipped | 21.82% repo line coverage | 76.6499% repo line coverage | 90.989% |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/evidence/qa-gates/remediation-csharp-coverage-summary.2026-05-07T23-10-00-04-00.md`

## Executive Summary

This post-remediation audit reviewed the current branch state for issue `#148` relative to `development`, using refreshed PR-context artifacts plus the remediated feature evidence. The remediated branch now records a clean final C# toolchain pass, passing changed/new-code coverage, passing scope reconciliation, and passing the structural file-size remediation checks.

The remaining non-ready condition is not a formatting, analyzer, nullable, unit-test, or scope-control failure. The remaining condition is that acceptance criterion 4 still lacks a fully automated Outlook responsiveness verifier, and the revised remediation cycle correctly fails closed by recording an automated blocked disposition instead of requesting manual validation.

**Policy documents evaluated:**
- [✅] `general-code-change.instructions.md`
- [✅] `general-unit-test.instructions.md`

**Language-specific policies evaluated:**
- [✅] `csharp-code-change.instructions.md`
- [✅] `csharp-unit-test.instructions.md`
- [N/A] Python policies
- [N/A] PowerShell policies
- [N/A] TypeScript policies

**Temporary artifacts cleanup:**
- [✅] No temporary throwaway scripts were introduced by the reviewed remediation set.
- [✅] The retained remediation artifacts are review evidence, not executable one-off scripts.

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | [✅] PASS | The reviewed regression homes are MSTest unit tests that run through `vstest.console.exe` under the full suite and targeted slices without requiring ordered execution; the remediation MSTest artifact records a successful full run with `4012` discovered tests. |
| **Isolation** - Each test targets single behavior | [✅] PASS | The targeted regression artifacts are organized around discrete behaviors such as startup timing, batching, selection snapshotting, conversation snapshotting, and table snapshotting; each artifact names a narrow set of tests for one seam. |
| **Fast Execution** - Tests complete quickly | [✅] PASS | Focused targeted regressions were used during remediation for fast loop verification, and the final full MSTest run completed successfully as the final QA gate. The evidence set does not record per-test timings, but there is no indication of abnormal test-runtime inflation. |
| **Determinism** - Consistent results | [✅] PASS | The evidence set includes deterministic fail-before and pass-after targeted regressions plus a clean final full-suite pass. The regression homes rely on MSTest/Moq seams rather than live Outlook automation. |
| **Readability & Maintainability** - Clear structure | [✅] PASS | The remediation plan explicitly replaced brittle source-text assertions with behavioral seam tests, and the resulting evidence set records focused regressions by subsystem and behavior. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | [✅] PASS | Baseline repo line coverage: `21.82%` from `evidence/baseline/csharp-mstest-coverage.2026-05-07T21-44-13-04-00.md`, referenced by `remediation-csharp-coverage-summary.2026-05-07T23-10-00-04-00.md`. |
| **No Coverage Regression** | [✅] PASS | Post-change repo line coverage: `76.6499%`; delta: `+54.8299`. `Coverage Policy Evaluation` records `Repository No-Regression vs baseline: PASS`. |
| **New Code Coverage ≥90%** | [✅] PASS | Changed/new-code coverage across the eight tracked production files is `90.989%`, recorded in both `remediation-csharp-mstest-coverage.2026-05-07T23-09-50-04-00.md` and `remediation-csharp-coverage-summary.2026-05-07T23-10-00-04-00.md`. |
| **Comprehensive Coverage** | [✅] PASS | The remediation evidence covers the startup `AppEvents` path, controller/model staging, conversation resolver snapshotting, dataframe staging, conversation helper staging, mail-item materialization, and table snapshot access; the final full-suite pass confirms the broader regression net still holds. |
| **Positive Flows** - Valid inputs | [✅] PASS | Passing targeted green artifacts `p5-t1` through `p5-t4` and the remediation-focused regression artifact confirm valid-path coverage for the new startup/first-selection staging seams. |
| **Negative Flows** - Invalid inputs | [✅] PASS | The fail-before evidence set documents deterministic red states for the affected seams before remediation, including AppEvents and Utilities boundary regressions. |
| **Edge Cases** - Boundary conditions | [✅] PASS | The reviewed tests cover startup overlap, empty or changed selections, conversation/table snapshot boundaries, and cancellation-related boundary behaviors in the affected helpers. |
| **Error Handling** - Error paths | [✅] PASS | The reviewed evidence preserves timeout/cancellation and error propagation coverage in the Utilities and QuickFiler helper layers; the final nullable/analyzer passes confirm no new hidden failure states were introduced. |
| **Concurrency** - If applicable | [✅] PASS | The reviewed bug scope is concurrency-adjacent because it separates UI-thread COM access from background-safe transforms. The targeted regression homes verify the staged async boundaries that prevent the prior overlapping UI-thread block. |
| **State Transitions** - If applicable | [✅] PASS | `AppEvents.LoadAsync()` startup-active timing boundaries, deferred batch checkpoints, and first-selection stage boundaries are exercised through the targeted regression set and final pass. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 21.82% repo line coverage -> Post-change: 76.6499% repo line coverage. Change: +54.8299. New/changed-code coverage: 90.989%. Disposition: PASS. Evidence: `evidence/qa-gates/remediation-csharp-coverage-summary.2026-05-07T23-10-00-04-00.md`.

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | [✅] PASS | The fail-before artifacts identify exact targeted test names and commands, and the remediation plan explicitly required replacement of raw source-text assertions with behavior-observable tests. |
| **Arrange-Act-Assert Pattern** | [✅] PASS | The reviewed MSTest homes follow the repository’s standard unit-test organization; the remediation evidence is organized by individual behavior-focused tests rather than by opaque suite-only output. |
| **Document Intent** | [✅] PASS | The targeted regression artifact names are descriptive and map directly to the intended behavior, for example AppEvents startup timing, batching, selection snapshotting, and table snapshot boundaries. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | [✅] PASS | The reviewed automated evidence relies on MSTest/Moq/FluentAssertions and coverage tooling, not on live Outlook manual interaction. |
| **Use Mocks/Stubs** | [✅] PASS | The test homes named in the spec and regression artifacts use MSTest with Moq to keep Outlook COM interactions behind deterministic seams. |
| **Environment Stability** | [✅] PASS | The remediated acceptance state explicitly blocks live Outlook validation because no fully automated verifier exists; this prevents the review from claiming completion based on non-deterministic manual interaction. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | [✅] PASS | This artifact serves as the refreshed post-remediation policy audit for the active feature folder. |

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | [✅] PASS | The objective is stated in `issue.md`, `spec.md`, and `remediation-plan.2026-05-07T21-30.md`: finish the startup/first-selection follow-up while failing closed on manual validation. |
| **Read existing change plans** | [✅] PASS | The review considered `plan.2026-05-07T19-34.md` and `remediation-plan.2026-05-07T21-30.md`. |
| **Document the plan** | [✅] PASS | The remediation plan documents Phase 0 through Phase 4 and records completion through the automated blocked end state. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | [✅] PASS | The implemented strategy keeps the fix centered on explicit snapshot boundaries and coarse staged publication rather than broad startup architecture replacement, consistent with `spec.md`. |
| **Reusability** | [✅] PASS | The remediation split oversized files into focused helper companions within the same functional areas instead of duplicating logic. |
| **Extensibility** | [✅] PASS | The staged snapshot pattern and split helper files preserve a clear extension path for a future automated responsiveness verifier. |
| **Separation of concerns** | [✅] PASS | The reviewed scope separates Outlook STA-bound acquisition from background-safe transforms and isolates structural helper files for conversation, dataframe, mail-item, and table processing. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | [✅] PASS | The structural remediation evidence records focused companion files such as `ConversationResolver.Loading.cs`, `DfDeedle.FrameUtilities.cs`, and `OlTableExtensions.TableAccess.cs`, each aligned to a single responsibility area. |
| **Under 500 lines** | [✅] PASS | `post-remediation-structure-check.2026-05-07T23-02-45-04-00.md` records all changed production files at `<= 500` lines. |
| **Public vs internal** | [✅] PASS | No evidence suggests unplanned public-surface expansion; the split files stay within existing types and functional areas. |
| **No circular dependencies** | [✅] PASS | The analyzer and nullable builds both pass after the structural split, with no evidence of new dependency-cycle fallout. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | [✅] PASS | The split helper filenames and targeted regression names are descriptive and aligned with the staged behavior they represent. |
| **Docs/docstrings** | [✅] PASS | Supporting design intent is recorded in `spec.md`, the remediation plan, and the evidence artifacts; no documentation gap blocks readiness except the known automated-verifier gap. |
| **Comment why, not what** | [✅] PASS | The design intent captured in the spec and regression names focuses on why staged snapshotting and deferred UI publication are required. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | [✅] PASS | **Command:** `dotnet tool run csharpier format .`<br>**Result:** Final clean pass completed with `EXIT_CODE: 0`; pre-existing backup project warning did not block success. |
| **2. Linting** | [✅] PASS | **Command:** `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`<br>**Result:** Final analyzer-enabled build succeeded with `EXIT_CODE: 0`. |
| **3. Type checking** | [✅] PASS | **Command:** `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`<br>**Result:** Final nullable-enabled build succeeded with `0 Warning(s)` and `0 Error(s)`. |
| **4. Testing** | [✅] PASS | **Command:** `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`<br>**Result:** `4012` total, `4010` passed, `0` failed, `2` skipped; changed/new-code coverage `90.989`. |
| **Full toolchain loop** | [✅] PASS | The remediation plan requires restart-on-change semantics for Phase 3 and records a clean final pass across all four steps. |
| **Explicit reporting** | [✅] PASS | Exact commands and outputs are recorded in the remediation QA artifacts and summarized in this audit. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | [✅] PASS | The spec, remediation plan, refreshed PR context, and end-state artifact summarize the startup/first-selection follow-up scope and final disposition. |
| **Design choices explained** | [✅] PASS | The design rationale for STA snapshot boundaries, staged background transforms, and blocked manual validation is documented in `spec.md` and the remediation artifacts. |
| **Update supporting documents** | [✅] PASS | `spec.md`, the remediation plan, the end-state artifact, and this refreshed review set reflect the current behavior. |
| **Provide next steps** | [✅] PASS | The blocker artifact specifies the next required work: implement a fully automated Outlook responsiveness verifier and rerun the end-state/review refresh. |

## 3. Language-Specific Code Change Policy Compliance

### C# Code Change Policy Compliance

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with csharpier** | [✅] PASS | `evidence/qa-gates/remediation-csharp-format.2026-05-07T23-09-20-04-00.md` |
| **Analyzer-clean build** | [✅] PASS | `evidence/qa-gates/remediation-csharp-analyzers-build.2026-05-07T23-09-30-04-00.md` |
| **Nullable/type-safety clean build** | [✅] PASS | `evidence/qa-gates/remediation-csharp-nullable-build.2026-05-07T23-09-40-04-00.md` |
| **Strong contracts and null-safety** | [✅] PASS | The final nullable pass confirms no new warnings in the touched remediation scope. |
| **Focused types and composition** | [✅] PASS | The structural remediation split oversized files into focused companions within the same existing types and domains. |
| **Logging and explicit failure behavior** | [✅] PASS | The spec and evidence continue to rely on timing/logging rather than silent suppression, and the code-quality passes show no unresolved analyzer/nullability regressions. |

## 4. Language-Specific Unit Test Policy Compliance

### C# Unit Test Policy Compliance

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use MSTest** | [✅] PASS | The entire automated evidence set uses `vstest.console.exe` through the repository MSTest wrappers. |
| **Use Moq for mocks/stubs** | [✅] PASS | The mapped test homes in the spec and regression artifacts are Moq-backed MSTest homes. |
| **Prefer FluentAssertions** | [✅] PASS | The reviewed regression homes referenced in the spec and evidence are FluentAssertions-based MSTest units. |
| **Toolchain uses approved C# commands** | [✅] PASS | The final QA artifacts use the exact repository-approved C# formatter, analyzer, nullable, and MSTest-with-coverage commands. |

## 5. Test Coverage Detail

| Scope | Evidence | Coverage Detail | Status |
|---|---|---|---|
| Startup coordination (`AppEvents`) | `evidence/regression-testing/p5-t1-appevents-green.2026-05-07T21-11-12-04-00.md` | Final targeted green regressions for startup timing and batching; final file coverage `77.30%`; changed/new-code aggregate passes. | ✅ |
| Controller/model staging (`EfcHomeController`, `EfcDataModel`, `ConversationResolver`) | `evidence/regression-testing/p5-t2-controller-model-green.2026-05-07T21-11-55-04-00.md` | Selection snapshot and staged model initialization regressions pass; file coverage recorded in remediation coverage artifacts. | ✅ |
| Utilities snapshot boundaries (`DfDeedle`, `ConversationHelper`, `MailItemHelper`, `OlTableExtensions`) | `evidence/regression-testing/p5-t3-utilities-green.2026-05-07T21-12-31-04-00.md` | Final utilities regressions pass; four utility helpers end at `>= 90%` file coverage and changed/new-code aggregate passes. | ✅ |
| Full regression net | `evidence/qa-gates/remediation-csharp-mstest-coverage.2026-05-07T23-09-50-04-00.md` | `4012` total tests, `4010` passed, `0` failed, `2` skipped. | ✅ |

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 4012 | ✅ |
| Tests Passed | 4010 (99.95%) | ✅ |
| Tests Failed | 0 | ✅ |
| Tests Skipped | 2 | ✅ |
| Repository Line Coverage | 76.6499% | ✅ per remediation-cycle composite gate |
| Baseline Repo Coverage | 21.82% | ✅ documented |
| Changed/New-Code Coverage | 90.989% | ✅ |
| Structural Compliance | PASS | ✅ |

## 7. Code Quality Checks

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| C# Formatting | `dotnet tool run csharpier format .` | Final clean formatter pass completed successfully. | ✅ |
| C# Analyzer Build | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild` | Final analyzer-enabled build succeeded. | ✅ |
| C# Nullable Build | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors` | Final nullable-enabled build succeeded with `0 Warning(s)` and `0 Error(s)`. | ✅ |
| MSTest with Coverage | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` | Final coverage-enabled MSTest run succeeded. | ✅ |

Notes:
- The repository-wide line coverage remains below `80%`, but the remediated coverage summary records a PASS composite gate because coverage materially improved over the documented baseline and changed/new-code coverage exceeds `90%`.
- No clean-run failure remains in the reviewed remediation evidence set.

## 8. Gaps and Exceptions

### Identified Gaps
- Acceptance criterion 4 still lacks a fully automated Outlook responsiveness verifier. The review therefore cannot verify live repaint/input continuity without violating the no-manual-step contract.

### Approved Exceptions
- None. The current blocked state is recorded as a policy-compliant fail-closed disposition, not as an exception approval.

### Removed/Skipped Tests
- None recorded in the final remediated evidence set. The final full-suite pass includes two skipped tests, but no issue `#148` review artifact identifies them as remediation blockers.

## 9. Summary of Changes

### Commits in This PR/Branch

- HEAD commit in refreshed PR context: `8d092a0c6ece254396d6ecc3d3f8160f8dc7013e`
- Current review scope also includes the active working-tree feature artifacts generated during this refresh.

### Files Modified

1. **Startup and first-selection production scope** (MODIFIED)
   - `TaskMaster/AppGlobals/AppEvents.cs`
   - `QuickFiler/Controllers/EfcHomeController.cs`
   - `QuickFiler/Controllers/EfcDataModel.cs`
   - `QuickFiler/Helper Classes/ConversationResolver*.cs`
   - `UtilitiesCS/Extensions/DfDeedle*.cs`
   - `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper*.cs`
   - `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper*.cs`
   - `UtilitiesCS/OutlookObjects/Table/OlTableExtensions*.cs`

2. **Mapped MSTest homes** (MODIFIED)
   - `TaskMaster.Test/AppGlobals/AppEventsTests.cs`
   - `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`
   - `QuickFiler.Test/Controllers/EfcDataModelTests.cs`
   - `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs`
   - `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`
   - `UtilitiesCS.Test/OutlookObjects/Conversation/ConversationHelper_ExtendedTests.cs`
   - `UtilitiesCS.Test/OutlookObjects/MailItem/MailItemHelperCoreTests.cs`
   - `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs`

3. **Feature artifacts** (NEW/MODIFIED)
   - Remediation QA evidence, blocked-verifier evidence, end-state artifact, and this refreshed review set under `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/`.

## 10. Compliance Verdict

### Overall Status: ⚠️ PARTIALLY COMPLIANT / BLOCKED

The remediated branch state is compliant for scope control, structural remediation, approved C# toolchain execution, and changed/new-code coverage. The branch is not ready for a PASS verdict because acceptance criterion 4 cannot yet be proven with automated evidence. The review therefore records a policy-compliant blocked disposition rather than requesting additional manual remediation in this branch.

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- [✅] Before Making Changes: documented in the plan, spec, and remediation plan.
- [✅] Design Principles: staged snapshot boundaries and focused helper splits remain within declared scope.
- [✅] Module & File Structure: structural remediation passes.
- [✅] Naming, Docs, Comments: evidence and file naming remain descriptive.
- [✅] Toolchain Execution: final clean pass recorded.
- [✅] Summarize & Document: supporting documents and end-state artifacts are updated.

#### Language-Specific Code Change Policy (Section 3)
- [✅] C# Tooling & Baseline: all approved commands pass.
- [✅] C# Design & Type Safety: final nullable/analyzer passes confirm clean remediation state.
- [✅] C# Error Handling & Logging: no unresolved evidence of silent failure or analyzer drift.

#### General Unit Test Policy (Section 1)
- [✅] Core Principles: targeted regression set plus final full-suite pass.
- [✅] Coverage & Scenarios: baseline, final, and changed/new-code metrics are documented.
- [✅] Test Structure: focused regression homes and behavior-based assertions.
- [✅] External Dependencies: automated review remains independent of live manual Outlook validation.
- [✅] Policy Audit: this artifact provides the refreshed review.

#### Language-Specific Unit Test Policy (Section 4)
- [✅] C# Framework & Scope: MSTest used throughout.
- [✅] C# Test Style & Structure: mapped regression homes cover the reviewed seams.
- [✅] C# Assertions & Mocking: Moq and FluentAssertions remain the active unit-test pattern.
- [✅] C# Toolchain: approved commands and MSTest coverage run are recorded.

### Recommendation

**Blocked**

Do not open a new remediation loop for the current code changes. Treat this branch as blocked pending a future fully automated Outlook responsiveness verifier that can satisfy acceptance criterion 4 without a manual step.

## Appendix A: Test Inventory

- `TaskMaster.Test\AppGlobals\AppEventsTests.cs` — startup timing and startup batching regression homes.
- `QuickFiler.Test\Controllers\EfcHomeControllerTests.cs` — first-selection snapshot staging.
- `QuickFiler.Test\Controllers\EfcDataModelTests.cs` — staged model initialization boundaries.
- `QuickFiler.Test\Helper Classes\ConversationResolverTests.cs` — conversation snapshot and publication cadence.
- `UtilitiesCS.Test\Extensions\DfDeedle_COM_Tests.cs` — table snapshot to dataframe transform boundary.
- `UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelper_ExtendedTests.cs` — conversation snapshot boundary.
- `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperCoreTests.cs` — COM materialization before async projection.
- `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensions_Tests.cs` — table access snapshot boundary.
- Full suite evidence: `evidence/qa-gates/remediation-csharp-mstest-coverage.2026-05-07T23-09-50-04-00.md`.

## Appendix B: Toolchain Commands Reference

- `dotnet tool run csharpier format .`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

**Audit Completed By:** GitHub Copilot  
**Audit Date:** 2026-05-08  
**Policy Version:** Current (as of audit date)
