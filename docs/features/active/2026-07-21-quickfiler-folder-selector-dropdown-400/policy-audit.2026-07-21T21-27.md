# Policy Compliance Audit: QuickFiler Folder Selector Drop-Down (#400)

**Audit Date:** 2026-07-21
**Reviewer:** Codex feature-review (independent post-remediation review)
**Base Requested:** `origin/main` at `fd9fb5ee1ca0c044b8dd0e02a81a22f58c6f3f68`
**Merge Base:** `df5ad49c909f6b739edef45d0336151f44e827a6`
**Head:** `bug/quickfiler-folder-selector-dropdown-400` at `b38a87751669f3522928dd01ac0f4f97b82572ed`, plus the current tracked and untracked remediation worktree
**Code Under Test:** The complete 104-path committed feature range plus the current remediation delta. Executable scope comprises C# production/test sources, legacy C# project includes, and `QuickFiler/Resources/FolderBreadcrumb.html` with embedded JavaScript and CSS. The canonical inventory is in `artifacts/pr_context.appendix.txt`.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---:|---:|---|---:|---:|---:|
| C# | Feature/remediation production and test sources | 5,849 repository tests; 141 issue-related test methods | **PASS:** 5,849 passed, 0 failed, 0 skipped | 87,397/104,178 = 83.8920% lines | 89,255/106,048 = 84.1647% lines | **PASS:** 1,141/1,143 = 99.8250%; every measurable selector type/member >=90% |
| HTML with embedded JavaScript/CSS | 1 compiled resource | `FolderBreadcrumbAssetContractTests` | **PASS:** baseline 3 passed/8 failed; post-change 12 passed/0 failed | 3/11 = 27.2727% targeted semantic contracts | 12/12 = 100.0000% targeted semantic contracts | **PASS:** 12/12 = 100.0000% deterministic compiled-resource contracts; no repository line-instrumentation gate applies to this resource |

Coverage evidence:

- C# baseline: `evidence/baseline/coverage-baseline.2026-07-21T16-00.cobertura.xml`.
- C# post-change: `evidence/qa-gates/coverage-final.2026-07-21T21-09.cobertura.xml`.
- C# comparison: `evidence/qa-gates/coverage-delta.2026-07-21T21-18.md`.
- HTML/JavaScript/CSS baseline and post-change semantic counts: `evidence/regression-testing/fail-before-html-asset.2026-07-21T16-55.md` and `evidence/regression-testing/pass-after-html-asset.2026-07-21T17-04.md`.
- TypeScript and PowerShell: N/A - zero changed files in the merge-base feature range or remediation delta.

- TypeScript baseline coverage artifact: N/A - out of scope; zero changed TypeScript files.
- TypeScript post-change coverage artifact: N/A - out of scope; zero changed TypeScript files.
- PowerShell baseline coverage artifact: N/A - out of scope; zero changed PowerShell files.
- PowerShell post-change coverage artifact: N/A - out of scope; zero changed PowerShell files.
- Per-language comparison summary: Section 1.2.1.

## Executive Summary

The numeric coverage gates and ordered C# toolchain pass, but the implementation is not policy-complete. This audit evaluated `.github/copilot-instructions.md`, the general code-change and unit-test policies, the C# code-change and unit-test policies, and the feature-review workflow.

Seven Major runtime correctness findings remain: non-unique row identities, off-UI-thread WebView2 posts, collapsed-surface replay before document readiness, selector-session mutation outside the router lock, unsuppressed suggestion upgrades after reset/disposal, inability to cancel a pending native open through `Close`, and subfolder activation that is not committed into the open selector session. These defects leave concurrency, lifecycle, accessibility, and scenario-completeness policy requirements unsatisfied. Exact findings and source locations are in `code-review.2026-07-21T21-27.md`.

Temporary artifacts cleanup: **PASS**. No temporary scripts or prohibited temporary test files were introduced. Review/QA artifacts are in the canonical feature folder, and `coverage.config` was restored exactly after the final coverage run.

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|---|---|---|
| Independence | PASS | Tests construct and dispose local hosts, messengers, controls, completion sources, and callbacks; the complete suite passes without order dependencies. |
| Isolation | PASS | Host-neutral state, projection, routing, placement, hub, lifecycle, and asset tests use injected seams. |
| Fast execution | PASS | 5,849 tests completed in 52.4323 seconds. |
| Determinism | PASS | Changed tests contain no sleeps, temporary files, network calls, external services, screenshots, or user interaction. |
| Readability and maintainability | PASS | Scenario-specific MSTest names and FluentAssertions provide clear diagnostics; all changed test files are at most 500 lines. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|---|---|---|
| Baseline coverage documented | PASS | C# baseline is 87,397/104,178 = 83.8920%; asset fail-before evidence records 3 passed and 8 failed contracts. |
| No coverage regression | PASS | C# final is 89,255/106,048 = 84.1647%, +0.2727 percentage points; modified tracked hunks improve from 42/46 to 367/367. |
| New/changed code coverage >=90% | PASS | C# changed/new measurable production is 1,141/1,143 = 99.8250%; the minimum selector type is 98.2270% and minimum measurable host/helper member is 97.5000%. |
| Positive flows | PARTIAL | Normal unique-identity selection and popup readiness pass; duplicate paths and subfolder activation do not. |
| Negative/error flows | PARTIAL | Factory/readiness/display failures are covered, but off-thread WebView posts and disposed-coordinator late completions are not bounded. |
| Edge cases | FAIL | Duplicate suggestion/recent paths, pending-open close, and persistent collapsed-surface readiness are absent. |
| Concurrency | FAIL | Selector-session mutations can race row replacement, and suggestion upgrades are not canceled/suppressed after reset/disposal. |
| State transitions | FAIL | Subfolder selection can be announced and then silently rolled back; close during pending initialization can still show/focus the popup. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 83.8920% lines -> Post-change: 84.1647% lines. Change: +0.2727 percentage points. New/changed-code coverage: 99.8250%. Disposition: PASS. Evidence: `coverage-delta.2026-07-21T21-18.md`.
- HTML with embedded JavaScript/CSS: Baseline: 3/11 = 27.2727% targeted semantic contracts -> Post-change: 12/12 = 100.0000% targeted semantic contracts. Change: +72.7273 percentage points. New/changed-code coverage: 12/12 = 100.0000% deterministic compiled-resource contracts. Disposition: PASS. Evidence: the HTML fail-before/pass-after artifacts above. The compiled resource has no repository line-instrumentation gate; this numeric comparison is the applicable semantic-contract measure.

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|---|---|---|
| Clear failure messages | PASS | New and modified assertions use FluentAssertions. |
| Arrange-Act-Assert | PASS | Test bodies use explicit phases or an equivalent direct structure. |
| Document intent | PASS | Test class summaries and method names state conditions and outcomes. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|---|---|---|
| Avoid external dependencies | PASS | Tests replace Outlook, WebView2, monitor/display, focus, and popup boundaries with seams. |
| Use mocks/stubs | PASS | Moq and focused fakes isolate external boundaries. |
| Environment stability | PASS | No prohibited temporary file, wall-clock sleep, live service, or mutable external configuration is used. |

### 1.5 Policy Audit Requirement

Pre-submission review: **FAIL**. This audit identifies remediation requirements; the branch is not ready for merge.

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|---|---|---|
| Objective clarified | PASS | `issue.md` and `spec.md` define issue #400 and full-bug scope. |
| Existing plan read | PASS | `plan.2026-07-21T10-41.md` is the executor plan of record. |
| Plan documented | PASS | The plan and prior remediation artifacts map implementation and verification work to evidence. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|---|---|---|
| Simplicity first | PARTIAL | Focused types are present, but model mutation has two synchronization owners and coordinator lifecycle ownership is incomplete. |
| Reusability | PASS | Session, projection, routing, placement, and hub components remain host-neutral and reusable. |
| Extensibility | PARTIAL | Injected seams are narrow, but row identity conflates logical row identity with output path. |
| Separation of concerns | PARTIAL | Direct WebView construction is separated, but UI-thread dispatch and collapsed readiness are not represented at the messaging boundary. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|---|---|---|
| Cohesive modules | PASS | New types have focused stated responsibilities. |
| Under 500 lines | PASS | Host 484; helper 118; largest changed test 500; all changed production/test C# files are <=500. |
| Public vs internal | PARTIAL | Most implementation is internal, but the router exposes its mutable model and permits unlocked external mutation. |
| No circular dependencies | PASS | Project/namespace inspection found no new circular dependency. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|---|---|---|
| Descriptive names | PASS | Types and lifecycle methods state their responsibilities. |
| Docs/docstrings | PASS | Public and non-obvious contract members have XML summaries. |
| Comment why, not what | PASS | Direct-adapter and lifecycle comments explain rationale. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|---|---|---|
| 1. Formatting | PASS | `csharpier format .`; exit 0; 1,432 files; no source change. |
| 2. Linting | PASS | Analyzer-enabled `msbuild`; exit 0; zero errors; five unchanged System.Reactive package warnings. |
| 3. Type checking | PASS | Nullable warnings-as-errors `msbuild`; exit 0; zero compiler/nullable warnings and zero errors. |
| 4. Testing | PASS | Canonical coverage wrapper discovered eight assemblies; 5,849 passed, zero failed/skipped. |
| Full toolchain loop | PASS | All four artifacts use run identity `final-pass-2026-07-21T21-07Z` and record one uninterrupted successful sequence. |
| Explicit reporting | PASS | Exact commands, exit codes, hashes, and counts are in the final QA artifacts. |

### 2.6 Summarize and Document

Documentation and evidence are extensive. The required next step is execution of the new remediation plan followed by a fresh final QA pass and feature review.

## 3. Language-Specific Code Change Policy Compliance

### 3A: C# Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| CSharpier formatting | PASS | Final CSharpier pass succeeded without modifying source. |
| Analyzer build | PASS | Analyzer-enabled build succeeded with zero errors. |
| Nullable analysis | PASS | Nullable warnings-as-errors reports zero compiler/nullable warnings. |
| Strong contracts and null safety | PARTIAL | Null contracts pass, but row identity uniqueness and router mutation ownership are not enforced. |
| Async/resource safety | FAIL | WebView posts can occur off the UI thread; upgrades can post after reset/disposal; pending close does not invalidate open work. |
| Error handling | FAIL | Late coordinator work can call a disposed hub, and `async void` inbound dispatch can surface an off-thread WebView exception. |
| Dependency policy | PASS | No new package, persisted configuration, threshold, or coverage-filter change remains. |

### 3B: HTML/Embedded JavaScript/CSS Resource Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Accessibility semantics | FAIL | Duplicate identities can assign active styling and `aria-selected=true` to multiple options. |
| Input routing | PARTIAL | Unique-row keyboard and activation contracts pass; duplicate-row navigation and subfolder/session composition fail. |
| Theme and presentation | PARTIAL | Static contracts pass, but initial collapsed theme/render delivery is not gated on document readiness. |

## 4. Language-Specific Unit Test Policy Compliance

### 4A: C# Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| MSTest | PASS | Changed tests use `[TestClass]` and `[TestMethod]`. |
| Moq/test doubles | PASS | Moq and focused fakes are used at external boundaries. |
| FluentAssertions | PASS | New and modified assertions use FluentAssertions. |
| Scenario completeness | FAIL | Seven material runtime scenarios have no regression coverage. |
| Full C# toolchain | PASS | Formatting, analyzers, nullable analysis, and all tests passed in order. |

## 5. Test Coverage Detail

- Repository C#: 83.8920% baseline -> 84.1647% final.
- Modified tracked production hunks: 91.3043% baseline -> 100.0000% final.
- Changed/new measurable production: 1,141/1,143 = 99.8250%.
- All 21 measurable selector types are >=98.2270%; all measurable host/helper members are >=97.5000%.
- The two permitted nonnumeric direct adapters are enumerated in `acceptance-verification.2026-07-21T21-19.md`; no threshold/filter/exclusion was weakened.
- HTML/embedded JavaScript/CSS: 12/12 post-change compiled-resource contracts pass, compared with 3/11 before implementation.
- Coverage verdicts remain PASS, but line coverage does not establish the missing duplicate-identity, thread-affinity, readiness, lock-interleaving, disposal, pending-close, and subfolder-session scenarios.

## 6. Test Execution Metrics

| Metric | Value | Status |
|---|---:|---|
| Total tests | 5,849 | PASS |
| Passed | 5,849 (100%) | PASS |
| Failed | 0 | PASS |
| Skipped | 0 | PASS |
| Execution time | 52.4323 seconds | PASS |
| Test assemblies | 8 | PASS |
| Repository C# line coverage | 84.1647% | PASS |
| Changed/new measurable C# coverage | 99.8250% | PASS |
| HTML asset contracts | 12/12 | PASS |

## 7. Code Quality Checks

| Check | Command | Result | Status |
|---|---|---|---|
| CSharpier | `csharpier format .` | Exit 0; 1,432 files; unchanged source hash | PASS |
| Analyzer build | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Exit 0; 0 errors | PASS |
| Nullable build | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` | Exit 0; 0 compiler/nullable warnings | PASS |
| MSTest/coverage | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final.2026-07-21T21-09.cobertura.xml` | 5,849/5,849 passed; 84.1647% | PASS |
| Whitespace | `git diff --check` | Exit 0 | PASS |

Five System.Reactive package warnings are known baseline warnings, not added compiler/analyzer/nullable diagnostics. The temporary test-binary instrumentation exclusion did not skip tests and `coverage.config` was restored byte-for-byte.

## 8. Gaps and Exceptions

### Identified Gaps

1. Logical row identities are not unique when one path appears in suggestions and recents.
2. Async coordinator continuations can call WebView2 outside its UI thread.
3. The persistent collapsed surface replays cached state/theme before document readiness.
4. Selector-session and direct item mutations bypass the router synchronization boundary.
5. Suggestion upgrades can post after reset/disposal or duplicate a newer state.
6. `Close` does not cancel an in-flight native open before `_isOpen` becomes true.
7. Expanded subfolder activation is not durably committed into the selector session.

### Approved Exceptions

**None.** The two method-level nonnumeric surfaces are bounded direct adapters permitted by AC-18, not policy waivers.

### Removed/Skipped Tests

**None.** The final run reports zero skipped tests.

## 9. Summary of Changes

### Commits in This PR/Branch

1. `b38a87751669f3522928dd01ac0f4f97b82572ed` - `fix(breadcrumb): restore folder selector drop-down behavior`.
2. Current uncommitted remediation worktree - popup readiness correlation, host lifecycle serialization/invalidation, deterministic regression/coverage evidence, project includes, and AC reconciliation.

### Files Modified

Production changes add selector-session state, serialization, score-preserving projection/routing, multi-surface replay, native popup placement/ownership, ItemViewer/controller integration, and shared HTML presentation. The remediation modifies `BreadcrumbDropDownHost.cs`, adds `BreadcrumbWebViewSurfaceFactory.cs`, updates legacy project files, and adds readiness/lifecycle/coverage/composition tests. The complete path inventory is in `artifacts/pr_context.appendix.txt`.

## 10. Compliance Verdict

### Overall Status: NON-COMPLIANT

Formatting, static analysis, type checking, tests, and coverage pass. Seven Major correctness defects and corresponding scenario gaps remain, so the implementation cannot be considered policy-complete or ready for merge.

### Policy-by-Policy Summary

- General code change policy: **FAIL** for synchronization, lifecycle, and runtime correctness.
- General unit test policy: **FAIL** for edge, concurrency, and state-transition completeness.
- C# code change policy: **FAIL** for async/resource safety and mutation ownership.
- C# unit test policy: **FAIL** for scenario completeness.
- C# numeric coverage: **PASS**.
- HTML/embedded JavaScript/CSS contract coverage: **PASS**, while runtime accessibility/readiness behavior remains incomplete.

### Recommendation

**Needs revision.** Execute the generated remediation plan, rerun the full ordered C# toolchain and coverage comparison, reconcile the acceptance criteria, and repeat independent feature review.

## Appendix A: Test Inventory

The 141 issue-related test methods are grouped in 19 classes: `QfcItemControllerBreadcrumbDropDownTests`, `BreadcrumbBridgeCoordinatorProbabilityTests`, `BreadcrumbDropDownCoverageThresholdTests`, `BreadcrumbDropDownHostTests`, `BreadcrumbDropDownIntegrationTests`, `BreadcrumbDropDownLifecycleConcurrencyTests`, `BreadcrumbDropDownLifecycleTests`, `BreadcrumbDropDownReadinessTests`, `BreadcrumbMessengerHubTests`, `BreadcrumbPopupPlacementTests`, `BreadcrumbSelectorCoordinatorTests`, `FolderBreadcrumbAssetContractTests`, `ItemViewerBreadcrumbDropDownContractTests`, `BreadcrumbRenderProjectionSelectorTests`, `BreadcrumbSelectionSessionTests`, `BreadcrumbSelectorMessagesTests`, `BreadcrumbStateModelSelectorTests`, `FolderBreadcrumbBridgeRouterEdgeTests`, and `FolderBreadcrumbBridgeRouterInFlightTests`.

## Appendix B: Toolchain Commands Reference

1. `csharpier format .`
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final.2026-07-21T21-09.cobertura.xml`

**Audit Completed By:** Codex feature-review
**Policy Version:** Current as of 2026-07-21
