# Policy Compliance Audit: QuickFiler Folder Selector Drop-Down (#400)

**Audit Date:** 2026-07-21
**Reviewer:** Codex feature-review
**Base:** main at df5ad49c909f6b739edef45d0336151f44e827a6
**Head:** bug/quickfiler-folder-selector-dropdown-400 at b38a87751669f3522928dd01ac0f4f97b82572ed
**Code Under Test:** 13 production C# files, 16 C# test files, four legacy project files, and QuickFiler/Resources/FolderBreadcrumb.html. The complete 104-file branch inventory is in artifacts/pr_context.appendix.txt.

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New/Changed Code Coverage |
|---|---:|---:|---|---:|---:|---:|
| C# | 29 .cs files | 5,830 repository tests; 115 issue-specific tests | 5,830 passed, 0 failed, 0 skipped | 87,397/104,178 = 83.8920% lines | 89,113/105,884 = 84.1610% lines | 1,030/1,030 measurable changed/new executable lines = 100.0000% |

Coverage artifacts:

- Baseline: docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/baseline/coverage-baseline.2026-07-21T16-00.cobertura.xml
- Post-change: docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final.2026-07-21T17-44.cobertura.xml
- Comparison: docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-delta.2026-07-21T17-49.md
- TypeScript baseline coverage artifact: N/A - out of scope
- TypeScript post-change coverage artifact: N/A - out of scope
- PowerShell baseline coverage artifact: N/A - out of scope
- PowerShell post-change coverage artifact: N/A - out of scope
- Per-language comparison summary: Section 1.2.1

## Executive Summary

The branch is not policy-complete and needs revision. The exact-head final toolchain evidence reports a clean CSharpier pass, analyzer and nullable builds with zero new diagnostic identities, and 5,830 passing tests. Repository line coverage increased by 0.2690 percentage points and all instrumentable changed/new production lines are covered.

Two major correctness defects remain. The popup WebView is made available to the messenger hub immediately after NavigateToString, before any navigation-completion or page-ready condition. Initial cached render, theme, and selector messages can therefore be posted before the document registers its message listener. Popup initialization also has no shared in-flight task, generation token, cancellation, or post-await reset/disposal check, allowing concurrent opens or reset/disposal during initialization to attach stale or duplicate surfaces.

The audit evaluated .github/copilot-instructions.md, general-code-change.instructions.md, general-unit-test.instructions.md, csharp-code-change.instructions.md, csharp-unit-test.instructions.md, and the feature-review workflow. No production or test file was modified by this review.

Temporary artifacts cleanup: PASS. No temporary scripts or files were created. Review artifacts are stored in the canonical active feature folder.

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|---|---|---|
| Independence | PASS | Changed tests use local harnesses and dispose owned controls; the complete suite passed. |
| Isolation | PARTIAL | Host-neutral state, placement, routing, and lifecycle are separated, but several HTML and reflection tests assert implementation text or shape rather than runtime behavior. |
| Fast execution | PASS | 5,830 tests completed in 53.4409 seconds. |
| Determinism | PASS | No sleeps, temporary files, network calls, external services, or user interaction were found in changed tests. |
| Readability and maintainability | PASS | MSTest names describe scenarios and tests use Arrange-Act-Assert comments. All changed test files are at or below 499 lines. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|---|---|---|
| Baseline coverage documented | PASS | 87,397/104,178 = 83.8920% at merge base df5ad49c909f6b739edef45d0336151f44e827a6. |
| No coverage regression | PASS | Final 89,113/105,884 = 84.1610%; modified tracked hunks improved from 32/36 to 355/355. |
| New code coverage at least 90% | PARTIAL | Every measurable selector type is at least 98.2270% and 112 measurable members are 100%, but two new production methods are excluded and have no numeric rate. |
| Positive, negative, edge, and error flows | PARTIAL | Broad deterministic coverage exists, but page readiness, pending asynchronous initialization, open-state Up routing, and automatic-close rollback after pending movement are not covered end to end. |
| Concurrency | FAIL | No test holds the surface factory incomplete while a second open, Reset, or Dispose occurs. The implementation is vulnerable at that boundary. |
| State transitions | PARTIAL | SelectionSession coverage is broad; missing composition tests leave the uncommitted native-close path after pending movement unverified. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 83.8920% lines -> Post-change: 84.1610% lines. Change: +0.2690 percentage points. New/changed-code coverage: 100.0000%. Disposition: FAIL. Evidence: coverage-delta.2026-07-21T17-49.md and coverage-accounting-scope-change.2026-07-21T18-01.md.

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|---|---|---|
| Clear failure messages | PASS | FluentAssertions is used throughout the changed tests. |
| Arrange-Act-Assert | PASS | Changed test cases use explicit phases or a comparably direct structure. |
| Document intent | PASS | Test-class summaries and scenario-specific method names state the contract under test. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|---|---|---|
| Avoid external dependencies | PASS | Host-neutral seams replace live Outlook, WebView, and display use. |
| Use mocks/stubs | PASS | Injected messenger, surface-factory, focus, geometry, and show delegates isolate external boundaries. |
| Environment stability | PASS | No prohibited temporary file creation or wall-clock sleep was found. |

### 1.5 Policy Audit Requirement

Pre-submission review: FAIL. This audit identifies remediation requirements; the branch is not ready for merge.

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|---|---|---|
| Objective clarified | PASS | issue.md and spec.md define issue #400 and full-bug work mode. |
| Existing plan read | PASS | plan.2026-07-21T10-41.md is the plan of record. |
| Plan documented | PASS | The plan has completed task checkboxes and linked evidence, although review findings require a remediation plan. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|---|---|---|
| Simplicity first | PARTIAL | Focused types are used, but asynchronous host lifecycle state is incomplete. |
| Reusability | PASS | Selection, placement, messaging, and projection logic are separated into reusable host-neutral types. |
| Extensibility | PASS | IBreadcrumbDropDownHost and injected factories/delegates keep platform boundaries explicit. |
| Separation of concerns | PASS | UI adapters, selector state, message serialization, placement, and projection are separated. |

### 2.3 Module and File Structure

| Requirement | Status | Evidence |
|---|---|---|
| Cohesive modules | PASS | New types have focused responsibilities. |
| Under 500 lines | PASS | The largest changed test file is 499 lines and the largest changed production file is 456 lines. |
| Public versus internal | PASS | New implementation types are internal except for required existing contracts. |
| No circular dependencies | PASS | Project and namespace inspection found no new circular dependency. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|---|---|---|
| Descriptive names | PASS | SelectionSession, MessengerHub, PopupPlacement, and DropDownHost names match responsibilities. |
| Public API documentation | PASS | New public/internal contract members include summaries where behavior is non-obvious. |
| Comments explain rationale | PASS | Direct adapter exclusions and native close semantics include rationale comments. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|---|---|---|
| Formatting | PASS | csharpier format .; exit 0; 1,428 files; zero changes. Reviewer also ran csharpier check . at exact HEAD; exit 0. |
| Linting | PASS | Analyzer-enabled msbuild; exit 0; zero errors and zero added diagnostic identities. |
| Type checking | PASS | Nullable warnings-as-errors msbuild; exit 0; zero new nullable/compiler diagnostics. |
| Testing | PASS | Coverage wrapper discovered eight assemblies; 5,830 passed, zero failed or skipped. |
| Full toolchain loop | PASS | Evidence records one uninterrupted final pass from 17:43Z through 17:44Z. |
| Explicit reporting | PASS | Exact commands and outcomes are recorded in evidence/qa-gates/final-*.2026-07-21T17-4*.md. |

### 2.6 Summarize and Document

Change summary and design evidence are present in the spec, plan, PR context, and QA evidence. Next step: implement and verify the remediation plan generated from this review.

## 3. Language-Specific Code Change Policy Compliance

### 3A: C# Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| CSharpier formatting | PASS | Final format and reviewer check passed. |
| Analyzer build | PASS | Build succeeded with no new diagnostic identities. |
| Nullable analysis | PASS | Warnings-as-errors build succeeded with no new nullable diagnostics. |
| Strong contracts | PASS | Host-neutral types and IBreadcrumbDropDownHost make state and UI boundaries explicit. |
| Null safety | PASS | No new nullable diagnostic was reported. |
| Async/resource safety | FAIL | EnsureSurfaceAsync does not guard concurrent initialization or invalidate stale completion after Reset/Dispose. |
| Error handling | PARTIAL | Completed factory failures are cleaned up, but post-reset/disposal completion is not safely handled. |
| Dependency policy | PASS | No new external package or persisted configuration was introduced. |

## 4. Language-Specific Unit Test Policy Compliance

### 4A: C# Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| MSTest | PASS | Changed tests use TestClass and TestMethod. |
| Moq at boundaries | PASS | Existing repository approach is preserved; injected fakes are used where a full mock is unnecessary. |
| FluentAssertions | PASS | New and modified assertions use FluentAssertions. |
| Scenario completeness | FAIL | No deterministic test exercises an incomplete surface factory through concurrent open, reset, or disposal. HTML asset tests do not execute message-listener readiness. |
| Full C# toolchain | PASS | Exact-head evidence records format, analyzer, nullable, and coverage-enabled test steps in order. |

## 5. Test Coverage Detail

- Repository: 83.8920% baseline to 84.1610% final.
- Modified tracked hunks: 88.8889% baseline to 100.0000% final.
- All measurable changed/new production executable lines: 1,030/1,030.
- Measurable changed/new members: 112/112 at 100%.
- Dedicated selector types: minimum 98.2270%.
- FolderBreadcrumbBridgeRouter: 97.5490% baseline to 98.2270% final.
- New nonnumeric methods: BreadcrumbDropDownHost.CreateProductionSurfaceAsync and BreadcrumbDropDownHost.ShowOwnedPopup.
- Pre-existing nonnumeric boundaries: ItemViewer type and the existing Qfc InitializeWebViewAsync and EnsureBreadcrumbPipeline method exclusions.

The recorded scope change is structurally bounded: no class-level exclusion or coverage configuration change was added, and ShowOwnedPopup is a direct one-line WinForms adapter. However, CreateProductionSurfaceAsync contains navigation/readiness behavior whose defect is not exercised by the seam tests. AC-18 and the original plan require a numeric result for every new method, so the literal criterion remains PARTIAL.

## 6. Test Execution Metrics

| Metric | Value | Status |
|---|---:|---|
| Total tests | 5,830 | PASS |
| Passed | 5,830 | PASS |
| Failed | 0 | PASS |
| Skipped | 0 | PASS |
| Execution time | 53.4409 seconds | PASS |
| Test assemblies | 8 | PASS |
| Repository line coverage | 84.1610% | PASS |
| Changed/new measurable line coverage | 100.0000% | PASS |

## 7. Code Quality Checks

| Check | Command | Result | Status |
|---|---|---|---|
| CSharpier | csharpier format . | Exit 0; zero files changed | PASS |
| Reviewer format verification | csharpier check . | Exit 0; 1,428 files checked | PASS |
| Analyzer build | msbuild TaskMaster.sln with EnableNETAnalyzers and EnforceCodeStyleInBuild | Exit 0; zero added identities | PASS |
| Nullable build | msbuild TaskMaster.sln with Nullable=enable and TreatWarningsAsErrors=true | Exit 0; zero new diagnostics | PASS |
| MSTest and coverage | scripts/vscode/Invoke-MSTestWithCoverage.ps1 | 5,830 passed; coverage artifact produced | PASS |
| Whitespace | git diff --check df5ad49c909f6b739edef45d0336151f44e827a6...HEAD | Exit 0 | PASS |

The repo-local dotnet tool command was unavailable during review because the worktree-local SDK is not installed. The globally installed csharpier command succeeded. Existing exact-head analyzer, nullable, and test artifacts were inspected rather than rerun.

## 8. Gaps and Exceptions

### Identified Gaps

1. Popup document readiness is not part of the production surface contract. Initial state can be posted before the message listener exists.
2. Pending asynchronous surface initialization is not serialized or invalidated on reset/disposal.
3. Deterministic tests do not cover the two lifecycle races, executed DOM/message readiness, open-state Up routing, or native-close rollback after moving pending selection.
4. Two new methods have no numeric coverage despite the literal AC-18 and plan requirement.

### Approved Exceptions

None. The recorded coverage entry is a scope change, not a human exception or waiver.

### Removed or Skipped Tests

None. The final run reports zero skipped tests.

## 9. Summary of Changes

The single branch commit is b38a8775, fix(breadcrumb): restore folder selector drop-down behavior.

Production changes add selector-session state, serialization, a messenger hub, popup placement, a native drop-down host, score-preserving projection/router behavior, controller and ItemViewer integration, and a shared HTML presentation update. Test changes add 16 C# files across QuickFiler.Test and UtilitiesCS.Test. Legacy project files explicitly include new sources. The complete file list is in artifacts/pr_context.appendix.txt.

## 10. Compliance Verdict

### Overall Status: NON-COMPLIANT

The verified toolchain and numeric coverage gates pass for measurable code, but major correctness and lifecycle defects remain. The branch needs revision and a new final QA pass before it can be considered ready for merge.

### Policy-by-Policy Summary

- General code change: PARTIAL; async resource safety is incomplete.
- General unit test: PARTIAL; key lifecycle and page-readiness scenarios are absent.
- C# code change: FAIL for asynchronous lifecycle safety.
- C# unit test: FAIL for scenario completeness.
- Coverage: PARTIAL under the literal per-method requirement.

### Recommendation

Needs revision. Implement page-readiness gating, serialize and invalidate popup initialization, add deterministic regression coverage, rerun the full ordered C# toolchain, regenerate numeric coverage evidence, and repeat feature review.

## Evidence Location Compliance

PASS by manual full-diff inspection. All new baseline, regression, QA, and review evidence is under docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence or the active feature root. No audit artifact was added under artifacts/. The repository does not contain validate_evidence_locations.py, so that optional script could not be executed.

## Appendix A: Test Inventory

Issue-specific coverage includes QfcItemControllerBreadcrumbDropDownTests, BreadcrumbBridgeCoordinatorProbabilityTests, BreadcrumbDropDownHostTests, BreadcrumbDropDownIntegrationTests, BreadcrumbDropDownLifecycleTests, BreadcrumbMessengerHubTests, BreadcrumbPopupPlacementTests, BreadcrumbSelectorCoordinatorTests, FolderBreadcrumbAssetContractTests, ItemViewerBreadcrumbDropDownContractTests, BreadcrumbRenderProjectionSelectorTests, BreadcrumbSelectionSessionTests, BreadcrumbSelectorMessagesTests, BreadcrumbStateModelSelectorTests, and extended FolderBreadcrumbBridgeRouter edge/in-flight tests.

## Appendix B: Toolchain Commands Reference

1. csharpier format .
2. msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
3. msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true
4. pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final.2026-07-21T17-44.cobertura.xml

**Audit Completed By:** Codex feature-review
**Policy Version:** Current as of 2026-07-21
