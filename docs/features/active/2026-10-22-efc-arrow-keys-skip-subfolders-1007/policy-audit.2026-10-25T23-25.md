# Policy Audit: EFC and QFC arrow keys skip expanded subfolders (Issue #1007)

- Branch: `bug/efc-arrow-keys-skip-subfolders-1007`
- Head: `8bdd970ee` (merge of `origin/main` into the branch; the #1007 change is commit `8082ef3b2` plus the documentation commits before it)
- Base: `main` (resolved `origin/main`) at merge base `3b9a1ebdd`
- Review timestamp: 2026-10-25T23-25 (first review of this branch)
- Work mode: `full-bug` (AC source: `spec.md` only)
- Reviewer: feature-review agent

## Template Provenance

The structure of this artifact follows the canonical major headings listed in `.claude/skills/policy-audit-template-usage/SKILL.md` (Executive Summary, sections 1 to 10, Appendix A and Appendix B), in the same shape as the most recent validated audits in this repository (`docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/policy-audit.2026-10-22T17-10.md` and the #1002 audits). The template-asset selector step was not run from this agent session. The artifact is validated with `validate_orchestration_artifacts` (artifact type `policy-audit`).

## Rejected Scope Narrowing

No scope narrowing was detected. The caller prompt states: "The manual Outlook verification is a human step, recorded separately by the orchestrator as a human_interaction exception. It is not a blocking finding." This directs the disposition of one manual step; it does not remove any file, language, toolchain step or coverage check from the audit. The caller also states: "The branch was just merged with main; that merge brought in only #1005, which is not under review." The two-dot diff `3b9a1ebdd..HEAD` is taken against the merge base, which already contains #1005, so the statement does not narrow the branch diff; the reviewer confirmed that `git diff 3b9a1ebdd..HEAD` contains only #1007 files (116 files). The audit scope is that full branch diff.

## Executive Summary

The branch adds keyboard navigation over the visible tree order in both breadcrumb viewers. A shared pure helper (`BreadcrumbVisibleOrder`) flattens rows and expanded descendants in pre-order. The EFC router gains a Navigation partial (Up, Down, Home, End, PageUp, PageDown, Ctrl+Left, no-selection start, atomic segment double-click); the QFC selection session gains a Tree partial (pending descendant node, boundary, page and parent moves, row-shape snapshot restored by Escape); the QFC router and coordinator gain Navigation and Keyboard partials (pending-aware Left and Right, closed-list Right opens the list, leaf Right cancels then pops out). The #440 Left parent walk, the Left segment hiding and the `[+]` restore of trailing segments are removed in both viewers. AC14 is fixed at the source (`OutlookFolderHierarchyProvider.MapNode` derives `HasChildren` from `snapshot.GetChildren`) and defended in both renderers and the QFC page script.

Overall verdict: **REMEDIATION_REQUIRED (1 blocking finding)**.

- Toolchain: PASS (CSharpier format and check, analyzer `/t:Rebuild`, warnings-as-errors `/t:Rebuild`, `Invoke-MSTestWithCoverage.ps1`: 7734 of 7734 passed under the class-level parallel runsettings; node suites 90 of 90, re-run by the reviewer).
- Test policy (MSTest, Moq, FluentAssertions, no temporary files, no sleeps or real waits, parallel-safe, AAA, documented intent): PASS.
- Repository C# coverage: PASS (86.40% line, 80.73% branch; floors 80% and 75% from root `CLAUDE.md`).
- New and changed C# code: PASS (new files 91.84% to 100% line; every instrumented changed line covered; independently recomputed by the reviewer from the post-processed Cobertura document).
- File size: PASS (largest changed code file 500 lines).
- Blocking B-1 (autonomous): the QFC page styles `.sub.selected` but not `.sub.active`. A pending descendant that is not the committed subfolder renders as `class="sub active"`, and the row's own `active` class is suppressed while a pending node owns the highlight, so arrowing into a subfolder in the open QFC list leaves no visible highlight anywhere in the list. AC4 is evaluated PARTIAL and unchecked.

## 1. General Unit Test Policy Compliance

### 1.1 Core principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Each new test class builds its own router, session, coordinator or fixture; no shared mutable static field in any changed test file (reviewer scan of the 25 changed C# test files). `csharp-isolation.2026-10-25T23-12.md`: nine single-class processes and one combined process, 126 of 126 each. |
| Isolation | PASS | One behaviour per test (decision-table row, boundary, or failure branch); names follow `Unit_Condition_Result`. |
| Fast execution | PASS | Full suite 42.7 s for 7734 tests (`final-toolchain-tests.2026-10-25T23-09.md`). |
| Determinism | PASS | In-flight provider calls are held on a `TaskCompletionSource` (`BreadcrumbBridgeRouterIssue1007Tests.Hold`, re-entrant Right and in-flight node tests). Reviewer scan of all changed test files: 0 `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `setTimeout`, `[Ignore]` or `Assert.Inconclusive`. Node GUIDs are read from rendered `data-node-id` attributes, never asserted as literals. |
| Readability | PASS with note | Arrange/Act/Assert comments and FluentAssertions `because` text throughout. Note (N-2, non-blocking): six rewritten #440 tests keep names that now contradict their assertions (for example `HandleArrowKey_LeftOnMultiSegmentRow_ActivatesParentSegment` asserts that Left does not activate the parent); their summaries cite #1007 and the governing AC. |
| No temporary files | PASS | 0 `Path.GetTemp`, `GetTempFileName`, `File.Write`, `File.Create` or `Directory.Create` tokens in the changed test files. |

### 1.2 Coverage requirements

Coverage thresholds applied: the root `CLAUDE.md` states C# line `>= 80%` and branch `>= 75%`, new modules, classes and methods `>= 90%`, and "Code changes or refactors must not reduce coverage for the lines that were changed"; these govern under the threshold precedence of `.claude/rules/general-unit-test.md`. The canonical artifact `artifacts/csharp/coverage.xml` is the package-level JaCoCo projection of the final pass (git-ignored, untracked); its nine package counters sum to 58327 covered of 67510 lines and 14409 covered of 17848 branches, equal to the one-line summary in `evidence/qa-gates/final-toolchain-tests.2026-10-25T23-09.md`. Per-file figures were recomputed by the reviewer from the post-processed Cobertura document of the same run (`coverage/coverage.cobertura.xml`, ignored) and from the retained baseline Cobertura document in the session scratch folder; every per-file figure in `evidence/qa-gates/coverage-comparison.2026-10-25T23-09.md` matched exactly. `quality-tiers.yml` is absent at the repository root on the base branch, so tier classification is not a finding.

Freshness note: the coverage run (2026-10-25 19:09 local) precedes the merge commit `8bdd970ee` (19:18 local), which brought in #1005 only. No #1007 file is touched by that merge, so the per-file figures stand; the repository-wide figure describes the pre-merge tree.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 25 production (5 new, 20 modified), 25 test (11 new, 14 modified), 4 project files | MSTest | 7734 passed, 0 failed | 86.30% | 86.40% | 91.84% |
| JavaScript | 1 production asset (`FolderBreadcrumb.js`), 3 node test files | node --test | 90 passed, 0 failed | 100% | 100% | 100% |
| TypeScript | 0 | none | none | N/A | N/A | N/A |
| Python | 0 | none | none | N/A | N/A | N/A |
| PowerShell | 0 | none | none | N/A | N/A | N/A |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/baseline-coverage.2026-10-23T13-43.md` (retained baseline Cobertura in the session scratch folder)
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml` with summaries `evidence/qa-gates/final-toolchain-tests.2026-10-25T23-09.md` and `evidence/qa-gates/coverage-comparison.2026-10-25T23-09.md`
- JavaScript baseline coverage artifact: `evidence/baseline/baseline-node-suites.2026-10-23T13-41.md`
- JavaScript post-change coverage artifact: `evidence/regression-testing/node-suites-run.2026-10-25T22-50.md`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 86.30% lines (57539/66677) / 80.57% branches (14035/17419) -> Post-change: 86.40% lines (58327/67510) / 80.73% branches (14409/17848). Change: +0.10% lines, +0.16% branches. New/changed-code coverage: 91.84%. Disposition: PASS. Evidence: artifacts/csharp/coverage.xml; evidence/qa-gates/coverage-comparison.2026-10-25T23-09.md; reviewer per-file recomputation from both Cobertura documents (section 5).
- JavaScript: Baseline: 100% lines (FolderBreadcrumb.js) -> Post-change: 100% lines (FolderBreadcrumb.js). Change: 0.00% lines. New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/baseline/baseline-node-suites.2026-10-23T13-41.md; evidence/regression-testing/node-suites-run.2026-10-25T22-50.md; reviewer re-run of the four node suites.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Verdicts

C# repo-wide coverage verdict: PASS (86.40% line at or above 80, 80.73% branch at or above 75; both rose against the baseline of 86.30% and 80.57%).

C# new-code coverage verdict: PASS (line coverage of the five new production files: `BreadcrumbVisibleOrder.cs` 100.00%, `BreadcrumbSelectionSession.Tree.cs` 99.16%, `FolderBreadcrumbBridgeRouter.Navigation.cs` 93.44%, `QuickFiler/Controllers/BreadcrumbBridgeRouter.Navigation.cs` 91.84%, `BreadcrumbBridgeCoordinator.Keyboard.cs` 94.52%; all at or above 90). Branch figures 100.00%, 91.10%, 78.33%, 90.00% and 90.00%; `FolderBreadcrumbBridgeRouter.Navigation.cs` branch is above the 75% floor but below 90 (N-3, non-blocking).

C# changed-line coverage verdict: PASS (every instrumented added or modified line is covered in 18 of 20 modified files; `BreadcrumbRow.Descendants.cs` covers 32 of 34 changed lines, all additions with no replaced instrumented line, and `BreadcrumbStateModel.cs` covers 37 of 38 changed lines (97.37%) against 32 of 34 replaced lines (94.12%) at baseline; reviewer recomputation matches the comparison artifact).

C# modified-file coverage verdict: PASS (every modified production file is at or above 80% line and 75% branch at head; lowest are `BreadcrumbBridgeRouter.Arrows.cs` 91.73% / 80.77% and `BreadcrumbBridgeCoordinator.cs` 97.59% / 80.23%). Note N-1: 27 unchanged instrumented lines in six modified files lost their only test execution because rewritten #440 tests or retired routes no longer reach them (section 5); no changed line regressed and no file fell below a floor.

JavaScript coverage verdict: PASS (node `--experimental-test-coverage` over `QuickFiler/Resources/FolderBreadcrumb.js`: 100% line, branch and function before and after; reviewer re-run 90 of 90). CI runs no node step; the CI-visible contracts are `FolderBreadcrumbKeysIssue1007Tests.cs` and `BreadcrumbBridgeJsKeysIssue1007Tests.cs`.

### 1.3 Scenario completeness

PASS. Positive (each decision-table row, every movement key), negative (unknown row, unknown node, invalid `pageSize` absent, zero, negative and non-integer, Ctrl+Left at segment 0), edge (no selection, first and last entry, collapsed node skipped, in-flight fetch, zero-children fetch, shortened row), error handling (provider fault paths unchanged; codec rejection) and state transitions (open, pending, commit, cancel with shape restore, collapse hiding the selection) are covered. The gap is the visible rendering of a pending node (B-1), which no test asserts at the stylesheet level.

## 2. General Code Change Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| Design: simplicity and separation of concerns | PASS | One pure static helper serves both viewers (S2); state stays where it was (EFC: `_selectedRowId` plus `SelectedDescendantId`; QFC: `PendingIdentity` plus `PendingNode`). |
| Reusability | PASS | `BreadcrumbVisibleOrder.AppendNodes` and `NodeHasChildren` are shared by the EFC row and the QFC model. |
| Error handling | PASS | Invalid `pageSize`, unknown keys and rejected segment indices are logged no-ops through log4net; constructor guards on the two new selector messages; no broad catch added. |
| File size (500 lines) | PASS | Reviewer `awk` line counts: largest changed code file `BreadcrumbSelectorCoordinatorTests.cs` 500 (unchanged from baseline), `FolderBreadcrumbBridgeRouterIssue1007Tests.cs` 499, `BreadcrumbSelectionSession.cs` 492. Note N-4: the spec allowed only call-throughs in `BreadcrumbSelectionSession.cs` (446 at baseline); it grew by 46 lines. |
| Public contracts documented | PASS | XML documentation on every new public member (`BreadcrumbVisiblePosition`, `BreadcrumbVisibleOrder`, `BreadcrumbPendingArrowAction`, the new router and session members). |
| Public API compatibility | PASS | Additive constructor overloads and optional parameters; `HandleSelectorKey(BreadcrumbSelectorKey)` keeps its signature (moved to the Keyboard partial). Intentional behaviour removals (#440 walk, Left segment hiding, `[+]` restore) are maintainer decisions D-Q9 and D-Q23. |
| Bugfix workflow (failing test first) | PASS | Eight fail-before and eight pass-after projections under `evidence/regression-testing/`; one exception dossier, judged valid (section 8). |
| Comments explain why | PASS | Each changed rule cites its decision (#1007 AC, D-Q, R or S identifier). |
| Host-identifier hygiene in committed files | PASS | `evidence-form.2026-10-25T23-12.md` reports 0 drive-rooted paths and 0 profile segments; reviewer spot reads of the evidence files found scratch locations written as the token `SCRATCH`. |

## 3. Language-Specific Code Change Policy Compliance

### 3.1 C# (`CLAUDE.md` C# Code Change Policy)

| Rule | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `final-toolchain-format.2026-10-25T23-07.md`: format rewrote nothing (porcelain identical before and after), `csharpier check .` exit 0 over 1691 files. |
| Analyzer rebuild | PASS | `final-toolchain-analyzers.2026-10-25T23-07.md`: `/t:Rebuild`, 0 warnings, 0 errors. |
| Nullable rebuild (warnings as errors) | PASS | `final-toolchain-typecheck.2026-10-25T23-07.md`: `/t:Rebuild`, 0 warnings, 0 errors, no `/p:Nullable=enable`; all five new production files carry `#nullable enable` (reviewer read). |
| Legacy `<Compile Include>` registration | PASS | Diff adds 5 production entries (`BreadcrumbBridgeRouter.Navigation.cs`, `BreadcrumbBridgeCoordinator.Keyboard.cs` in `QuickFiler.csproj`; `FolderBreadcrumbBridgeRouter.Navigation.cs`, `BreadcrumbSelectionSession.Tree.cs`, `BreadcrumbVisibleOrder.cs` in `UtilitiesCS.csproj`) and 11 test entries (5 in `QuickFiler.Test.csproj`, 6 in `UtilitiesCS.Test.csproj`), one per new file. |
| net48 constraints | PASS | `BreadcrumbVisiblePosition` is a plain sealed class (no `record` or `init`). |
| Thread affinity (#1002 I6) | PASS | No `ConfigureAwait(false)` and no lock in the EFC router files (`QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs`); the QFC router keeps its existing `_sync` lock and `ConfigureAwait(false)` pattern; the coordinator marshals through `_dispatcher.Dispatch`. |
| Coverage exclusions | PASS | `coverage.config` not in the diff; 0 added `ExcludeFromCodeCoverage`. |
| Suppressions | PASS | No `#pragma warning disable` or `SuppressMessage` added. |

### 3.2 JavaScript and CSS assets

| Rule | Verdict | Evidence |
|---|---|---|
| Page-script behaviour paired with C# string contracts | PASS | Every node assertion in `issue-1007-keys.test.mjs` has a twin in `FolderBreadcrumbKeysIssue1007Tests.cs` or `BreadcrumbBridgeJsKeysIssue1007Tests.cs`. |
| Selection state has matching CSS | FAIL | B-1: `FolderBreadcrumb.js` line 169 assigns `"sub active"` to a pending, non-committed descendant; `QuickFiler/Resources/FolderBreadcrumb.html` styles `.row.selected`, `.row.active` and `.sub.selected` only (lines 109 to 112 and 158). `makeRow` suppresses the row's `active` class while the pending node owns the highlight, so no element in the list is visibly highlighted. |
| `FolderBreadcrumb.js` size | PASS | 393 lines (350 at baseline; spec ceiling 500). |

## 4. Language-Specific Unit Test Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| MSTest attributes | PASS | `[TestClass]` and `[TestMethod]` throughout; 0 xUnit or NUnit references. |
| Moq | PASS | `Mock<IFolderHierarchyProvider>`, `Mock<IWebViewMessenger>`, `Mock<IFolderTreeService>` and the existing fixtures (`EfcDescendantRouterFixture`, coordinator harnesses). |
| FluentAssertions | PASS | 0 MSTest `Assert.*` calls added in the changed test files; every new assertion is `.Should()`. |
| No serialization as a fix | PASS | 0 `[DoNotParallelize]`; every run used `scripts/vscode/TaskMaster.cli.runsettings` (`Workers` 0, `Scope` ClassLevel). |
| Test location | PASS | New tests sit in `QuickFiler.Test` and `UtilitiesCS.Test` folders mirroring production; node tests under `tests/quickfiler-breadcrumb/`. |
| Rewritten tests record the reason | PASS | Each rewritten #440 test summary cites #1007 and the governing AC (reviewer read of the `BreadcrumbBridgeRouterTests.cs` and `BreadcrumbStateModelSequenceTests.cs` diffs). |

## 5. Test Coverage Detail

Per-file figures recomputed by the reviewer from the Cobertura `<class>` `lines/line` nodes, de-duplicated per file and line number (maximum hits), at head (`coverage/coverage.cobertura.xml`, the source of `artifacts/csharp/coverage.xml`) and in the retained baseline document.

| File | Status | Baseline line / branch | Post-change line / branch | Gate |
|---|---|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbVisibleOrder.cs` | new | absent | 100.00% / 100.00% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Tree.cs` | new | absent | 99.16% / 91.10% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Navigation.cs` | new | absent | 93.44% / 78.33% | PASS (line); branch note N-3 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Navigation.cs` | new | absent | 91.84% / 90.00% | PASS |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Keyboard.cs` | new | absent | 94.52% / 90.00% | PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | modified | 96.40% / 89.29% | 91.73% / 80.77% | PASS; N-1 |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | modified | 100.00% / 88.76% | 97.59% / 80.23% | PASS; N-1 |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs` | modified | 97.22% / 92.19% | 94.08% / 88.24% | PASS; N-1 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` | modified | 100.00% / 97.06% | 99.11% / 95.61% | PASS; N-1 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | modified | 98.46% / 92.86% | 97.76% / 95.24% | PASS; N-1 |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` | modified | 93.47% / 87.50% | 93.03% / 86.73% | PASS; N-1 |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | modified | 100.00% / 95.00% | 99.61% / 93.33% | PASS; N-1 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | modified | 98.37% / 95.59% | 97.98% / 91.89% | PASS; N-1 |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | modified | 98.35% / 92.31% | 98.35% / 92.31% | PASS |
| Other 11 modified production files | modified | see coverage-comparison | each at least 98.28% line and 92.86% branch (`BreadcrumbDocumentAssets.cs` constants only); no unchanged line lost coverage | PASS |

N-1 detail (unchanged lines that lost their only test execution; reviewer mapping of baseline line numbers through the `git diff -U0` hunks):

- `BreadcrumbBridgeRouter.Arrows.cs` lines 24 to 30: the non-anchored `ReExpand` branch of `HandleLeafToggleAsync` (a `[+]` click on a row collapsed away from its active segment). The Right-arrow twin of this branch is covered by `ArrowRight_OnRowCollapsedAwayFromActiveSegment_RestoresChain`; the toggle path lost `LeafExpandToggle_OnCollapsedRow_ReExpandsWithoutProviderQuery`, which was rewritten to the anchored case.
- `FolderBreadcrumbBridgeRouter.Descendants.cs` lines 70 to 72 (`ToggleAsync` non-anchored `ReExpand`) and 117 to 118 (closed-list `ArrowAsync` handled without a fetch): the coordinator now consumes closed-list Left and Right, and the QFC toggle tests were rewritten to the anchored case.
- `BreadcrumbBridgeCoordinator.cs` lines 321 to 324 (`PublishRouterOutputs` raising `UnhandledArrow`) and 417 to 419 (`RaiseSyntheticArrowKey` for an `unhandledArrow` report); `FolderBreadcrumbBridgeRouter.cs` line 362 (inbound `UnhandledArrowMessage`): the page no longer posts `unhandledArrow` (S6), and the router's unhandled echo is reached only on rare open-list paths.
- `BreadcrumbSelectionSession.cs` lines 437 to 439 (closed-list `Move` commit branch now that `Move` delegates open moves), `BreadcrumbStateModel.cs` lines 239 to 240 (`Expand` non-anchored `ReExpand`), `BreadcrumbStateModel.Row.cs` line 245, `BreadcrumbBridgeRouter.Selection.cs` line 328.

All of these paths remain reachable, every file stays above the floors, and the CLAUDE.md changed-line rule is met; the disposition is non-blocking, with a recommendation to restore direct tests in the same remediation cycle.

## 6. Test Execution Metrics

- MSTest full suite with coverage (final pass): 7734 total, 7734 passed, 0 failed (`final-toolchain-tests.2026-10-25T23-09.md`; reviewer read of `coverage/test-results/mstest-coverage-run.summary.txt`). Baseline 7599 passed.
- Parallel configuration: `scripts/vscode/TaskMaster.cli.runsettings` (`Workers` 0, `Scope` ClassLevel), `/InIsolation`.
- Isolation runs: nine single-class processes and one combined process over the nine new classes, 126 of 126 each (`csharp-isolation.2026-10-25T23-12.md`).
- Node suites: five file orders, 90 of 90 each (`node-suites-run.2026-10-25T22-50.md`); reviewer re-run of the four suites: 90 tests, 90 pass, 0 fail, `FolderBreadcrumb.js` 100% line, branch and function coverage.
- Fail-before and pass-after: eight pairs under `evidence/regression-testing/` (AC14, visible order, EFC vertical, EFC horizontal, QFC open, QFC closed, QFC clicks, page scripts).
- The reviewer did not re-run the C# toolchain; C# figures were read from the committed evidence and recomputed from the Cobertura documents.

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
|---|---|---|---|
| Test-policy token scan | Grep of the 29 changed test files for sleep, delay, clock, temp-file, `DoNotParallelize`, `Ignore`, xUnit, NUnit, `#pragma` and MSTest `Assert.` tokens | 0 matches | PASS |
| Coverage exclusion scan | `git diff --stat 3b9a1ebdd..HEAD -- coverage.config`; Grep for `ExcludeFromCodeCoverage` in added lines | empty; 0 | PASS |
| Raw evidence scan | `git diff --name-only 3b9a1ebdd..HEAD` filtered for `artifacts/`, `.trx`, `.coverage`, `cobertura`, `jacoco` | empty | PASS |
| Workflow change scan | `git diff --stat 3b9a1ebdd..HEAD -- .github` | empty | PASS |
| Stylesheet parity scan | Grep of `QuickFiler/Resources/FolderBreadcrumb.html` for `.sub.active`, `.active`, `aria-selected` | `.row.active` only; no `.sub.active` rule | FAIL (B-1) |
| AC14 single source | Grep of `UtilitiesCS` for `ChildKeys.Count` and `new FolderBreadcrumbSegment(` | one constructor site (`MapNode`); no remaining raw `ChildKeys.Count` derivation | PASS |

## Evidence Location Compliance

`git diff --name-only 3b9a1ebdd..HEAD` lists no file under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`. All committed evidence is under `<FEATURE>/evidence/{baseline,regression-testing,qa-gates,other}/` as Markdown summaries, which `CLAUDE.md` "Committed Test Evidence Format" permits. `validate_evidence_locations.py` is not present in this repository (Glob over the worktree found no copy), so the check was performed by diff inspection. Verdict: PASS.

## 8. Gaps and Exceptions

Blocking:

- B-1 (autonomous): no stylesheet rule for `.sub.active` in `QuickFiler/Resources/FolderBreadcrumb.html`, so the QFC open-list pending highlight is invisible whenever it rests on a descendant that is not the committed subfolder. AC4 is PARTIAL and was unchecked in `spec.md` and the `issue.md` mirror. Remedy: style `.sub.active` like `.sub.selected` (or a distinct pending style) and add a C# string-contract assertion over the document HTML.

Fail-before exception (`fail-before-exception.2026-10-25T22-23.md`): valid. The updated `UnhandledRightArrow_RaisesUnhandledArrowRight` only adds an `OpenSelector()` precondition; the asserted observation (one `UnhandledArrow(Right)` for an open-list Right on a childless leaf) held at the plan base through the router's unhandled path and holds after Phase 11 and Phase 13, so no code state can make it fail. The AC6 behaviour itself has failing evidence through `OpenRight_OnLeaf_CancelsThenRaisesUnhandledArrowRight` (`qfc-open-fail-before.2026-10-25T22-08.md`). The dossier carries `WhyFailingRunImpossible`, an alternative static proof and a search record.

Non-blocking:

- N-1: unchanged lines that lost their only test execution (section 5).
- N-2: six rewritten #440 tests keep names that contradict their new assertions.
- N-3: `FolderBreadcrumbBridgeRouter.Navigation.cs` branch coverage 78.33% (line 93.44%); the CLAUDE.md 90% new-code target is met on lines only.
- N-4: `BreadcrumbSelectionSession.cs` grew from 446 to 492 lines although the spec limited it to call-throughs; still under 500.
- N-5: AC14 defence-in-depth edge: after a zero-children fetch, collapsing with Left re-shows `+` on an EFC row shortened at its active segment (`BreadcrumbHtmlRenderer` suppresses only `anchoredOpen && HasNoLoadedChildren`) and on a QFC row (`zeroLoaded` requires `LeafExpanded`). The primary fix in `MapNode` makes this state unlikely.
- O-1: `artifacts/pr_context.summary.txt` and `.appendix.txt` are absent from this worktree; the PR-context MCP route is not available to this agent, so scope was derived from `git diff 3b9a1ebdd..HEAD`. Regenerate them before the PR is authored.
- O-2: TaskMaster CI checks have not run (no pull request yet); they are a PR-time gate.
- O-3: the manual Outlook verification listed in the spec is a human step recorded by the orchestrator; not a finding.

## 9. Summary of Changes

- Commits `b699b7b78` to `d50eae161`: promotion, grill decisions, research, spec and preflight-cleared plan.
- Commit `8082ef3b2`: shared visible-order helper; EFC router Navigation partial and Arrows rewrite; QFC session Tree partial, router Navigation partial and coordinator Keyboard partial; model and projection changes for the removed #440 behaviour and the shortened-anchor expansion; AC14 source fix and renderer defences; page-script changes in `BridgeJs` and `FolderBreadcrumb.js`; 11 new and 14 rewritten C# test files; one new node suite; evidence.
- Commit `8bdd970ee`: merge of `origin/main` (#1005 only; not reviewed here).

## 10. Compliance Verdict

**REMEDIATION_REQUIRED.** Toolchain, test policy, repository coverage, new-code coverage, changed-line coverage, file sizes, project registration, thread affinity and evidence form pass. One blocking finding (B-1, autonomous) remains: the QFC pending-descendant highlight has no stylesheet rule and is not visible.

## Appendix A: Test Inventory

| Test file | Tests | Status | Scope |
|---|---|---|---|
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbVisibleOrderTests.cs` | 13 | new | pure visible-order helper |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTreeIssue1007Tests.cs` | 19 | new | QFC open-list session |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.cs` | 17 | new | QFC router pending arrows |
| `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderIssue1007Tests.cs` | 3 | new | AC14 primary |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbEmptyExpanderIssue1007Tests.cs` | 5 | new | AC14 renderer and projection |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbBridgeJsKeysIssue1007Tests.cs` | 6 | new | EFC page-script contract |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.cs` (+ `.Navigation.cs`, `.DoubleClick.cs`) | 37 | new | EFC router navigation |
| `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.cs` | 18 | new | QFC coordinator keyboard |
| `QuickFiler.Test/Viewers/FolderBreadcrumbKeysIssue1007Tests.cs` | 8 | new | QFC page-script contract |
| `tests/quickfiler-breadcrumb/issue-1007-keys.test.mjs` | node | new | QFC page script |
| 14 existing C# test files and 2 node files | various | modified | dispositions per spec Test Strategy table |

## Appendix B: Toolchain Commands Reference

| Stage | Command | Run by |
|---|---|---|
| C# format | `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` | executor |
| C# analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | executor |
| C# type check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | executor |
| C# test and coverage | `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1` | executor |
| Node suites | `node --test --test-concurrency=1 --experimental-test-coverage` over `tests/quickfiler-breadcrumb/` | executor and reviewer |
| Diff scope | `git diff 3b9a1ebdd..HEAD --stat`; `git diff -U0 3b9a1ebdd HEAD -- <path>` | reviewer |
| Coverage reading | Python read of `<class>` `lines/line` nodes in the head and baseline Cobertura documents (scratch script) | reviewer |
