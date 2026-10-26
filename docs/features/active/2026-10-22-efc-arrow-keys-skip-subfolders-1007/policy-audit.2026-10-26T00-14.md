# Policy Audit: EFC and QFC arrow keys skip expanded subfolders (Issue #1007)

- Branch: `bug/efc-arrow-keys-skip-subfolders-1007`
- Head: `e804f5d6b` (remediation cycle 1 commit, on top of the first-review head `8bdd970ee` and the review-artifact commit `f654b6410`)
- Base: `main` (resolved `origin/main`, `3b9a1ebdd`), merge base `3b9a1ebdd` (recomputed with `git merge-base HEAD origin/main`)
- Review timestamp: 2026-10-26T00-14 (re-audit after remediation cycle 1)
- Previous review: `policy-audit.2026-10-25T23-25.md` (B-1 blocking; N-1 to N-5, O-1 to O-3 non-blocking)
- Work mode: `full-bug` (AC source: `spec.md` only; `issue.md` carries a mirror)
- Reviewer: feature-review agent

## Template Provenance

The structure follows the canonical major headings listed in `.claude/skills/policy-audit-template-usage/SKILL.md` (Executive Summary, sections 1 to 10, Appendix A and Appendix B), in the same shape as `policy-audit.2026-10-25T23-25.md`, which passed the `policy-audit` validator. `docs/features/templates/policy_audit/` is absent in TaskMaster and the template-asset selector was not run from this agent session. The artifact is validated with `validate_orchestration_artifacts` (artifact type `policy-audit`).

## Rejected Scope Narrowing

No scope narrowing was detected. The caller prompt states: "This review takes the same inputs as the first one, with no narrowing." and "The manual Outlook verification is a human step. Do not raise it as a blocking finding." The second sentence directs the disposition of one manual step; it removes no file, language, toolchain step or coverage check from the audit. The audit scope is the full branch diff `git diff 3b9a1ebdd..HEAD` (149 files), not the remediation commit alone.

## Executive Summary

Remediation cycle 1 (commit `e804f5d6b`) changes one production line and adds or renames tests only:

- B-1: `QuickFiler/Resources/FolderBreadcrumb.html` line 158 changes `.sub.selected { ... }` to `.sub.selected, .sub.active { ... }`. A pending descendant in the open QFC list is now drawn with `var(--bc-sel-back)` and `var(--bc-sel-fore)`; both variables are redefined in the explicit dark-theme block (lines 26 to 27) and the `prefers-color-scheme: dark` block (lines 39 to 40). The new C# string contract `Stylesheet_StylesPendingDescendantLikeSelectedSubfolder` reads only the `<style>` block, has a `.sub.selected` positive control, failed before the fix and passes after.
- N-1: four characterization tests now execute every previously orphaned N-1 line (each body hit count rose from 0 to 1).
- N-2: the six contradicting #440-era test names are renamed; old names have 0 matches and new names 1 each.

No C# production file changed in the cycle. The rest of the branch diff is unchanged since the first review and was re-checked against the policies below.

Overall verdict: **PASS (0 blocking findings)**.

- Toolchain: PASS (CSharpier format and check, analyzer `/t:Rebuild`, warnings-as-errors `/t:Rebuild`, `Invoke-MSTestWithCoverage.ps1`: 7761 of 7761 passed). The reviewer re-ran `csharpier check` over the changed directories (522 files, exit 0) and the four node suites (90 of 90).
- Repository C# coverage: PASS (86.49% line, 80.85% branch, recomputed by the reviewer from `artifacts/csharp/coverage.xml`).
- New C# code: PASS (five new production files 91.84% to 100.00% line).
- File size: PASS (largest changed code file 500 lines, unchanged from baseline).

## 1. General Unit Test Policy Compliance

### 1.1 Core principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | The five new tests each build their own fixture, router, model or `Harness`; the two new partial files declare no field. `csharp-isolation.2026-10-26T00-06.md`: each touched class in its own process, then all in one process, zero failures. |
| Isolation | PASS | One behaviour per test: the stylesheet rule; the EFC `leafExpandToggle` restore; the QFC `affordanceToggle` restore; the state-model Right restore; the coordinator `unhandledArrow` echo. |
| Fast execution | PASS | Full suite of 7761 tests in one coverage run (`final-toolchain-tests.2026-10-26T00-03.md`); the new tests are in-memory. |
| Determinism | PASS | Code-only diff scan (`git diff 3b9a1ebdd..HEAD -- '*.cs' '*.js' '*.mjs' '*.html' '*.csproj'`, added lines): 0 matches for `Thread.Sleep`, `Task.Delay`, `setTimeout`, `DoNotParallelize`, `Path.GetTemp`, `GetTempFileName`, `ExcludeFromCodeCoverage`, `#pragma warning disable`, `SuppressMessage`. |
| Readability | PASS | N-2 resolved: each renamed test now states its asserted behaviour (for example `HandleArrowKey_LeftOnNonExpandedMultiSegmentRow_IsNoOpAndKeepsLeafActive`). New tests carry Arrange, Act and Assert comments and XML summaries citing #1007. |
| No temporary files | PASS | The B-1 test reads the embedded `FolderBreadcrumbDocument.Html` string; no test touches the file system. |

### 1.2 Coverage requirements

Coverage thresholds applied: the root `CLAUDE.md` states C# line `>= 80%`, branch `>= 75%`, new modules, classes and methods `>= 90%`, and no reduction on changed lines; these govern under the precedence in `.claude/rules/general-unit-test.md`. `quality-tiers.yml` is absent at the repository root on the base branch and no tier-classification CI stage exists, so tier classification is not applicable and is not a finding.

The canonical artifact `artifacts/csharp/coverage.xml` exists (git-ignored, untracked). The reviewer confirmed its SHA-256 equals that of `coverage/coverage.cobertura.jacoco.xml` (`b1bd0f32...178e4`) and summed its nine package counters: lines 58427 of 67553 (86.49%), branches 14440 of 17860 (80.85%), equal to the one-line summary in `evidence/qa-gates/final-toolchain-tests.2026-10-26T00-03.md`. Per-file figures were recomputed by the reviewer from `coverage/coverage.cobertura.xml` (the same run) with `lines/line` nodes de-duplicated per file and line number; they match `evidence/qa-gates/coverage-comparison.2026-10-26T00-04.md`. The coverage run (20:03 local) follows the last C# edit and precedes the commit `e804f5d6b` (20:11 local), which contains the measured tree.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 25 production (5 new, 20 modified), 26 test (13 new, 13 modified), 4 project files | MSTest | 7761 passed, 0 failed | 86.30% | 86.49% | 91.84% |
| JavaScript | 1 production asset (`FolderBreadcrumb.js`), 1 HTML resource, 3 node test files | node --test | 90 passed, 0 failed | 100% | 100% | 100% |
| TypeScript | 0 | none | none | N/A | N/A | N/A |
| Python | 0 | none | none | N/A | N/A | N/A |
| PowerShell | 0 | none | none | N/A | N/A | N/A |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/baseline-coverage.2026-10-23T13-43.md` (branch baseline) and `evidence/remediation-baseline/baseline-coverage.2026-10-25T23-53.md` (cycle baseline)
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml` with summaries `evidence/qa-gates/final-toolchain-tests.2026-10-26T00-03.md` and `evidence/qa-gates/coverage-comparison.2026-10-26T00-04.md`
- JavaScript baseline coverage artifact: `evidence/baseline/baseline-node-suites.2026-10-23T13-41.md`
- JavaScript post-change coverage artifact: `evidence/regression-testing/node-suites-run.2026-10-26T00-05.md` and the reviewer re-run in this session
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 86.30% lines (57539/66677) / 80.57% branches (14035/17419) -> Post-change: 86.49% lines (58427/67553) / 80.85% branches (14440/17860). Change: +0.19% lines, +0.28% branches. New/changed-code coverage: 91.84%. Disposition: PASS. Evidence: artifacts/csharp/coverage.xml; evidence/qa-gates/coverage-comparison.2026-10-26T00-04.md; reviewer per-file recomputation (section 5).
- JavaScript: Baseline: 100% lines (FolderBreadcrumb.js) -> Post-change: 100% lines (FolderBreadcrumb.js). Change: 0.00% lines. New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/baseline/baseline-node-suites.2026-10-23T13-41.md; evidence/regression-testing/node-suites-run.2026-10-26T00-05.md; reviewer re-run with `--experimental-test-coverage` (100.00% line, branch and function for FolderBreadcrumb.js).
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Verdicts

C# repo-wide coverage verdict: PASS (86.49% line at or above 80, 80.85% branch at or above 75; both above the branch baseline of 86.30% and 80.57% and the cycle baseline of 86.47% and 80.81%).

C# new-code coverage verdict: PASS (line coverage of the five new production files: `BreadcrumbVisibleOrder.cs` 100.00%, `BreadcrumbSelectionSession.Tree.cs` 99.16%, `FolderBreadcrumbBridgeRouter.Navigation.cs` 93.44%, `QuickFiler/Controllers/BreadcrumbBridgeRouter.Navigation.cs` 91.84%, `BreadcrumbBridgeCoordinator.Keyboard.cs` 94.52%; all at or above 90). Branch figures 100.00%, 91.10%, 80.00%, 90.00% and 90.00%; `FolderBreadcrumbBridgeRouter.Navigation.cs` branch rose from 78.33% to 80.00% and stays an informational note (N-3).

C# changed-line coverage verdict: PASS (the cycle changed no C# production line; the first review's changed-line result stands, and the four N-1 files each rose: `BreadcrumbBridgeRouter.Arrows.cs` 91.73% to 96.24% line, `FolderBreadcrumbBridgeRouter.Descendants.cs` 94.08% to 96.05%, `BreadcrumbStateModel.cs` 97.76% to 100.00%, `BreadcrumbBridgeCoordinator.cs` 97.59% to 100.00%).

C# modified-file coverage verdict: PASS (every modified production file at or above 80% line and 75% branch; lowest are `BreadcrumbStateModel.Row.cs` 93.03% / 86.73% and `BreadcrumbBridgeCoordinator.cs` 100.00% / 84.88%).

JavaScript coverage verdict: PASS (reviewer re-run of the four node suites: 90 of 90, `FolderBreadcrumb.js` 100.00% line, branch and function). CI runs no node step; the CI-visible contracts are `FolderBreadcrumbKeysIssue1007Tests.cs` (now including the stylesheet rule) and `BreadcrumbBridgeJsKeysIssue1007Tests.cs`.

### 1.3 Scenario completeness

PASS. The first review's positive, negative, edge, error and state-transition coverage stands. The cycle closes the one gap recorded there: the visible rendering of a pending node is now pinned at the stylesheet level, and the four N-1 paths (non-anchored restores and the legacy `unhandledArrow` echo) each have a direct test.

## 2. General Code Change Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| Design: simplicity and separation of concerns | PASS | B-1 is fixed by adding one selector to an existing rule; no new variable, colour or script change. |
| Reusability | PASS | The pending style reuses the committed-subfolder declarations, so both themes stay consistent without duplication. |
| Error handling | PASS | No production logic changed in the cycle; first-review result stands. |
| File size (500 lines) | PASS | Reviewer `awk` counts: largest changed code file `BreadcrumbSelectorCoordinatorTests.cs` 500 (unchanged from baseline), `FolderBreadcrumbBridgeRouterIssue1007Tests.cs` 499 (not edited in the cycle), `BreadcrumbSelectionSession.cs` 492; cycle-edited files 230, 211, 315, 444, 477 and the two new partials 35 and 47. |
| Public contracts documented | PASS | Unchanged since the first review. |
| Public API compatibility | PASS | No public member changed in the cycle. |
| Bugfix workflow (failing test first) | PASS | `qfc-pending-style-fail-before.2026-10-25T23-55.md` (9 tests, 1 failed: `Expected declarations not to be <null> because the QFC stylesheet must style the pending descendant (.sub.active).`) and `qfc-pending-style-pass-after.2026-10-25T23-55.md` (24 of 24). |
| Comments explain why | PASS | Each new test summary states why the state is reachable (plan D3); one comment wording note in the code review (CR-I1). |
| Host-identifier hygiene in committed files | PASS | Reviewer search of the 26 evidence files added by `e804f5d6b` for drive-rooted paths, `Users` profile segments and the account name: 0 matches. |

## 3. Language-Specific Code Change Policy Compliance

### 3.1 C# (`CLAUDE.md` C# Code Change Policy)

| Rule | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `final-toolchain-format.2026-10-26T00-00.md` (pass 1 reformatted test code added by the cycle, inside WRITE-SET files only, and the loop restarted; pass 2 rewrote nothing, `Checked 1700 files`, exit 0); reviewer `dotnet tool run csharpier check` over the changed directories: `Checked 522 files`, exit 0. |
| Analyzer rebuild | PASS | `final-toolchain-analyzers.2026-10-26T00-01.md`: `/t:Rebuild`, 0 warnings, 0 errors. |
| Nullable rebuild (warnings as errors) | PASS | `final-toolchain-typecheck.2026-10-26T00-01.md`: `/t:Rebuild`, 0 errors, no `/p:Nullable=enable`. The new UtilitiesCS.Test partial carries `#nullable enable`; the new QuickFiler.Test partial uses C# 7.3 syntax and no directive (reviewer read). |
| Legacy `<Compile Include>` registration | PASS | One entry each for `BreadcrumbBridgeCoordinatorIssue1007Tests.Unhandled.cs` and `FolderBreadcrumbBridgeRouterIssue1007Tests.Unanchored.cs`; no other project-file change in the cycle. |
| Coverage exclusions | PASS | `coverage.config` not in the diff; 0 added `ExcludeFromCodeCoverage`. |
| Suppressions | PASS | 0 `#pragma warning disable` or `SuppressMessage` added. |

### 3.2 JavaScript and CSS assets

| Rule | Verdict | Evidence |
|---|---|---|
| Page-script behaviour paired with C# string contracts | PASS | Unchanged; the stylesheet rule now has its own C# contract. |
| Selection state has matching CSS | PASS | B-1 resolved: `makeSubfolders` (line 169) emits `sub active` or `sub selected active`; `FolderBreadcrumb.html` line 158 now styles both `.sub.selected` and `.sub.active`. The EFC page styles its selected descendant through `.child[aria-selected="true"]` in `BreadcrumbDocumentAssets.cs` lines 36 and 43 (reviewer read). |
| `FolderBreadcrumb.js` size | PASS | 393 lines; not changed in the cycle. |

## 4. Language-Specific Unit Test Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| MSTest attributes | PASS | The new partials add `[TestMethod]` members and no `[TestClass]` of their own; the main partials keep `[TestClass]`. |
| Moq | PASS | The QFC router partial verifies `GetImmediateSubfoldersAsync` with `Times.Never()` on the existing provider mock. |
| FluentAssertions | PASS | Code-only diff: 0 added MSTest `Assert.*` calls. |
| No serialization as a fix | PASS | 0 `[DoNotParallelize]`; every run used `scripts/vscode/TaskMaster.cli.runsettings`. |
| Test location | PASS | New partials sit beside their main partials in `QuickFiler.Test/Viewers` and `UtilitiesCS.Test/OutlookObjects/Folder`. |
| Test names match assertions | PASS | N-2 resolved (`evidence/other/test-renames.2026-10-25T23-59.md`; reviewer read of the rename hunks in `e804f5d6b`: names only, bodies unchanged). |

## 5. Test Coverage Detail

Per-file figures recomputed by the reviewer from `coverage/coverage.cobertura.xml` (`<class>` `lines/line` nodes, de-duplicated per file and line number, maximum hits).

| File | Status | First-review line / branch | Post-cycle line / branch | Gate |
|---|---|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbVisibleOrder.cs` | new | 100.00% / 100.00% | 100.00% / 100.00% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Tree.cs` | new | 99.16% / 91.10% | 99.16% / 91.10% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Navigation.cs` | new | 93.44% / 78.33% | 93.44% / 80.00% | PASS (line); branch note N-3 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Navigation.cs` | new | 91.84% / 90.00% | 91.84% / 90.00% | PASS |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Keyboard.cs` | new | 94.52% / 90.00% | 94.52% / 90.00% | PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | modified | 91.73% / 80.77% | 96.24% / 86.54% | PASS |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | modified | 97.59% / 80.23% | 100.00% / 84.88% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs` | modified | 94.08% / 88.24% | 96.05% / 89.71% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | modified | 97.76% / 95.24% | 100.00% / 97.62% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` | modified | 93.03% / 86.73% | 93.03% / 86.73% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | modified | 99.61% / 93.33% | 100.00% / 95.00% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` | modified | 99.11% / 95.61% | 99.11% / 95.61% | PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | modified | 97.98% / 91.89% | 97.98% / 91.89% | PASS |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | modified | 98.35% / 92.31% | 98.35% / 92.31% | PASS |
| Other 11 modified production files | modified | see first review | each at least 97.78% line and 92.86% branch; `BreadcrumbDocumentAssets.cs` constants only (no instrumented lines) | PASS |

N-1 lines (from `coverage-comparison.2026-10-26T00-04.md`, confirmed by the per-file rises above): the bodies of `BreadcrumbBridgeRouter.HandleLeafToggleAsync` non-anchored restore, `FolderBreadcrumbBridgeRouter.ToggleAsync` non-anchored restore, `BreadcrumbStateModel.Expand` non-anchored restore, and the `UnhandledArrowMessage` arms of `BreadcrumbBridgeCoordinator.PublishRouterOutputs` and `RaiseSyntheticArrowKey` each moved from 0 hits to at least 1. N-1 is resolved.

## 6. Test Execution Metrics

- MSTest full suite with coverage (final pass): 7761 total, 7761 passed, 0 failed; 7756 at the cycle baseline plus the 5 new tests (`final-toolchain-tests.2026-10-26T00-03.md`).
- Parallel configuration: `scripts/vscode/TaskMaster.cli.runsettings` (`Workers` 0, `Scope` ClassLevel), `/InIsolation`.
- Isolation runs: each touched class in its own process plus one combined process, zero failures (`csharp-isolation.2026-10-26T00-06.md`).
- Node suites: normal and reversed order, 90 of 90 each (`node-suites-run.2026-10-26T00-05.md`); reviewer re-run: 90 of 90, `FolderBreadcrumb.js` 100% coverage.
- B-1 fail-before and pass-after: `qfc-pending-style-fail-before.2026-10-25T23-55.md` (exit 1 expected; the single failure names `.sub.active`) and `qfc-pending-style-pass-after.2026-10-25T23-55.md` (24 of 24).
- N-1: `n1-direct-tests.2026-10-25T23-58.md` (113 of 113; the four new tests passed on their first run as characterization tests).
- The reviewer did not re-run the full C# build or MSTest; C# figures were read from committed evidence and recomputed from the coverage documents of the same run.

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
|---|---|---|---|
| Test-policy token scan | Grep of added lines in `git diff 3b9a1ebdd..HEAD -- '*.cs' '*.js' '*.mjs' '*.html' '*.csproj'` for sleep, delay, `setTimeout`, temp-file, `DoNotParallelize`, `ExcludeFromCodeCoverage`, `#pragma warning disable`, `SuppressMessage` | 0 matches | PASS |
| MSTest `Assert` scan | Grep of added C# lines for `Assert.AreEqual`, `Assert.IsTrue`, `Assert.IsFalse`, `Assert.IsNull`, `Assert.That` | 0 matches | PASS |
| Coverage exclusion scan | `git diff --name-only 3b9a1ebdd..HEAD` filtered for `coverage.config` | empty | PASS |
| Raw evidence scan | `git diff --name-only 3b9a1ebdd..HEAD` filtered for `^artifacts/`, `.trx`, `.coverage`, `cobertura`, `jacoco` | empty | PASS |
| Policy and workflow scan | `git diff --name-only 3b9a1ebdd..HEAD` filtered for `^.claude/`, `^.github/` | empty | PASS |
| Stylesheet parity scan | Grep of `QuickFiler/Resources/FolderBreadcrumb.html` for `.sub.` | line 158: `.sub.selected, .sub.active { background: var(--bc-sel-back); color: var(--bc-sel-fore); }` | PASS (B-1 resolved) |
| Formatting re-check | `dotnet tool run csharpier check` over the changed C# directories | `Checked 522 files`, exit 0 | PASS |

## Evidence Location Compliance

`git diff --name-only 3b9a1ebdd..HEAD` lists no file under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`, and no file under `artifacts/` at all. The cycle's evidence is under `<FEATURE>/evidence/{remediation-baseline,regression-testing,qa-gates,other}/` as Markdown projections, which `CLAUDE.md` "Committed Test Evidence Format" permits. `validate_evidence_locations.py` is not present in this repository, so the check was performed by diff inspection. Verdict: PASS.

## 8. Gaps and Exceptions

Blocking: none.

Resolved since the previous review:

- B-1: resolved (sections 3.2 and 7).
- N-1: resolved (section 5).
- N-2: resolved (section 4).

Carried forward, non-blocking and informational:

- N-3: `FolderBreadcrumbBridgeRouter.Navigation.cs` branch coverage 80.00% (line 93.44%); the CLAUDE.md 90% new-code target is met on lines.
- N-4: `BreadcrumbSelectionSession.cs` at 492 lines (446 at baseline); keep further session logic in the Tree partial.
- N-5: AC14 defence-in-depth edge after a zero-children fetch followed by a Left collapse; the `MapNode` source fix holds.
- O-1: `artifacts/pr_context.summary.txt` and `.appendix.txt` are absent from this worktree; scope was derived from `git diff 3b9a1ebdd..HEAD`. Regenerate them before the PR is authored.
- O-2: TaskMaster CI checks have not run (no pull request yet); a PR-time gate.
- O-3: the manual Outlook verification is a human step recorded by the orchestrator; not a finding.
- Fail-before exception (`fail-before-exception.2026-10-25T22-23.md`): judged valid in the first review; unchanged.

## 9. Summary of Changes

- Commits `b699b7b78` to `d50eae161`: promotion, grill decisions, research, spec and preflight-cleared plan.
- Commit `8082ef3b2`: the #1007 implementation (first review).
- Commit `8bdd970ee`: merge of `origin/main` (#1005 only; already in the merge base).
- Commit `f654b6410`: first-review artifacts.
- Commit `e804f5d6b`: remediation cycle 1 (one stylesheet selector, the B-1 string contract, four N-1 tests, six N-2 renames, two `<Compile Include>` entries, AC4 re-checked in `spec.md` and `issue.md`, remediation plan and evidence).

## 10. Compliance Verdict

**PASS.** Toolchain, test policy, repository coverage, new-code coverage, changed-line coverage, file sizes, project registration, evidence form and evidence location pass. B-1, N-1 and N-2 are resolved. No blocking finding remains; the open items are informational and PR-time gates.

## Appendix A: Test Inventory

| Test file | Tests added or changed in cycle 1 | Status | Scope |
|---|---|---|---|
| `QuickFiler.Test/Viewers/FolderBreadcrumbKeysIssue1007Tests.cs` | 1 added (`Stylesheet_StylesPendingDescendantLikeSelectedSubfolder`) | modified (new on branch) | QFC stylesheet contract (B-1) |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.DoubleClick.cs` | 1 added (`LeafExpandToggle_OnRowCollapsedAwayFromActiveSegment_RestoresChain`) | modified (new on branch) | EFC toggle restore (N-1) |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.Unanchored.cs` | 1 added (`AffordanceToggle_OnRowCollapsedAwayFromActiveSegment_RestoresChainWithoutQuery`) | new | QFC toggle restore (N-1) |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` | 1 added (`RightArrow_OnRowCollapsedAwayFromActiveSegment_RestoresChain`), 3 renamed | modified | state-model restore (N-1), names (N-2) |
| `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.Unhandled.cs` | 1 added (`OpenUnhandledArrowLeft_OnExpandedPendingRow_RaisesUnhandledArrowFromRouterOutput`) | new | coordinator unhandled echo (N-1) |
| `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.cs` | 0 (class made `partial`) | modified (new on branch) | QFC coordinator keyboard |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` | 3 renamed | modified | names (N-2) |
| Remaining 19 C# test files and 3 node test files | unchanged in cycle 1 | per first review | per first review Appendix A |

## Appendix B: Toolchain Commands Reference

| Stage | Command | Run by |
|---|---|---|
| C# format | `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` | executor; reviewer re-ran `check` over the changed directories |
| C# analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | executor |
| C# type check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | executor |
| C# test and coverage | `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1` | executor |
| Node suites | `node --test --test-concurrency=1 --experimental-test-coverage` over the four `tests/quickfiler-breadcrumb/` suites | executor and reviewer |
| Diff scope | `git diff 3b9a1ebdd..HEAD --name-status`; `git show e804f5d6b` | reviewer |
| Coverage reading | package-counter sum of `artifacts/csharp/coverage.xml`; Python read of `<class>` `lines/line` nodes in `coverage/coverage.cobertura.xml` (scratch script) | reviewer |
