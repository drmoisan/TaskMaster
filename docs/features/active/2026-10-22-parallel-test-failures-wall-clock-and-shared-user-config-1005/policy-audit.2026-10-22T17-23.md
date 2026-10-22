# Policy Audit: Parallel test failures from wall-clock deadlines and shared user.config (Issue #1005)

- Branch: `bug/parallel-test-failures-wall-clock-and-shared-user-config-1005`
- Head: `3df0782ab`
- Base: `main` (resolved `origin/main`) at merge base `346651242a2bd6ed25d703a4b1ecb5a055c6ae72`
- Review timestamp: 2026-10-22T17-23 (re-audit R4 after remediation cycle 1; prior review 2026-10-22T17-10 at head `e399de597`)
- Timestamp derivation: `git log -1 --format="%cI | %cr"` on `3df0782ab` reported a commit time of 17:21:21 UTC and "2 minutes ago"; the stamp is the observed current UTC minute.
- Work mode: `full-bug` (AC source: `spec.md` only)
- Reviewer: feature-review agent

## Template Provenance

The structure of this artifact follows the canonical major headings listed in `.claude/skills/policy-audit-template-usage/SKILL.md` (Executive Summary, sections 1 to 10, Appendix A and Appendix B), in the same shape as the prior validated audit for this branch (`policy-audit.2026-10-22T17-10.md`). The template-asset selector step was not run from this agent session. The artifact is validated with `validate_orchestration_artifacts` (artifact type `policy-audit`).

## Rejected Scope Narrowing

No scope narrowing was detected. The caller prompt states: "Same inputs and full scope as the original review; no scope narrowing." It also records an orchestrator decision that in-memory-only `Settings.Default` mutation and the per-method `[DoNotParallelize]` in `ApplicationGlobalsStartupTimingTests` are out of scope (issue #1006). That restates the non-goals of `spec.md` section 4 and directs finding disposition; it removes no file, language, toolchain step or coverage check. The audit scope is the full branch diff `346651242..3df0782ab` (91 files).

The Bash tool was limited by the caller to `git diff` and `git log` command forms. This is an execution constraint, not a scope narrowing: every figure below was read from committed evidence, the canonical coverage artifact, or the source tree.

## Executive Summary

The branch fixes two parallel-execution test failures. The DfDeedle happy-path and pre-cancelled tests pass a never-advanced `FakeTimeProvider` through the existing `timeProvider` parameter and are awaited `async Task` tests (test-only). A per-instance `ISettingsStore` seam (`UtilitiesCS`) with a production adapter `ApplicationSettingsStore` routes every test-reached settings save in `IDList`, `AppQuickFilerSettings`, `AppStagingFilenames` and the `AppAutoFileObjects` scalar properties through an injected store; production defaults wrap each assembly's own `Properties.Settings.Default`. Tests inject `Mock<ISettingsStore>` or an `InMemorySettingsStore`, and the class-level `[DoNotParallelize]` is removed from `AppQuickFilerSettingsTests` and `RibbonControllerTests`.

Remediation cycle 1 (commit `3df0782ab`) is test-only: three `MaxLengthOfID` tests in `ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs`, a new coverage comparison, toolchain evidence, and the remediation plan. No production file changed in that commit (`git diff e399de597..3df0782ab --stat`).

Overall verdict: **PASS (0 blocking findings)**.

- Prior finding B-1: RESOLVED. `get_MaxLengthOfID` is `line-rate="1" branch-rate="1"` in `artifacts/csharp/coverage.xml` (lines 107 to 113 all `hits="1"`, both branch lines `100% (2/2)`); `IDList.cs` is 30.18% line / 25.81% branch, above the merge-base 24.36% / 20.69%.
- Toolchain: PASS (format and check, analyzer rebuild, warnings-as-errors rebuild, MSTest with coverage; 7621 of 7621 tests passed under the class-level parallel runsettings).
- Parallel-isolation rules (no serialization, no sleeps, no temp files, no `user.config` redirection, no real save): PASS.
- C# repository coverage: PASS (86.38% line, 80.68% branch; floors 80% and 75% from root `CLAUDE.md`).
- C# new and changed code: PASS (new files 100%; every instrumented changed line covered).

## 1. General Unit Test Policy Compliance

### 1.1 Core principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Every converted and added test builds its own store (`InMemorySettingsStore` or `Mock<ISettingsStore>`); the three new `MaxLengthOfID` tests each construct their own lists and mocks. |
| Isolation | PASS | Each new test asserts one behaviour (cold cache, second cold cache, warm cache without save). |
| Fast execution | PASS | `parallel-repeat-runs.2026-10-22T16-30.md`: DfDeedle happy path 2.1 to 2.7 ms, down from a 9 s wall-clock budget. |
| Determinism | PASS | Never-advanced `FakeTimeProvider`; `git diff -G` reviewer scan from the prior review found no added `Thread.Sleep`, `Task.Delay`, `[Timeout]` or retry token, and the cycle-1 diff adds none (read in full). |
| Readability | PASS | Descriptive names, Arrange/Act/Assert comments, FluentAssertions `because` text. |
| No temporary files | PASS | 0 temp-path tokens in the branch diff; cycle-1 tests use in-memory lists and Moq only. |
| No shared settings file write | PASS | Probe evidence records 0 saves after the fix (`idlist-probe-pass-after.2026-10-22T16-18.md`, `taskmaster-settings-probe-pass-after.2026-10-22T16-25.md`); every new `IDList` in cycle 1 is store-injected. |

### 1.2 Coverage requirements

Coverage thresholds applied: the root `CLAUDE.md` states C# line `>= 80%` and branch `>= 75%`, new modules, classes and methods `>= 90%`, and that changed lines must not lose coverage; these govern under the threshold precedence of `.claude/rules/general-unit-test.md`. The canonical artifact `artifacts/csharp/coverage.xml` is the post-processed Cobertura from the cycle-1 final pass; its root attributes `lines-covered="57634" lines-valid="66720" branches-covered="14064" branches-valid="17431"` equal the figures in `evidence/qa-gates/remediation-tests.2026-10-22T17-12.md`. `quality-tiers.yml` is absent at the repository root, so tier classification is not a finding.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 7 production (3 new, 4 modified), 13 test (4 new, 9 modified), 4 project files | MSTest | 7621 passed, 0 failed | 86.30% | 86.38% | 100% |
| TypeScript | 0 | none | none | N/A | N/A | N/A |
| Python | 0 | none | none | N/A | N/A | N/A |
| PowerShell | 0 | none | none | N/A | N/A | N/A |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/baseline-coverage.2026-10-22T16-10.md` with projection `evidence/baseline/baseline-coverage-projection.2026-10-22T16-10.jacoco.xml`
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml` with summaries `evidence/qa-gates/remediation-tests.2026-10-22T17-12.md` and `evidence/qa-gates/coverage-comparison.2026-10-22T17-12.md`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 86.30% lines (57542/66677) / 80.58% branches (14036/17419) -> Post-change: 86.38% lines (57634/66720) / 80.68% branches (14064/17431). Change: +0.08% lines, +0.10% branches. New/changed-code coverage: 100%. Disposition: PASS. Evidence: artifacts/csharp/coverage.xml; evidence/qa-gates/coverage-comparison.2026-10-22T17-12.md; reviewer read of the `ToDoModel.IDList` class and `get_MaxLengthOfID` method nodes.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Verdicts

C# repo-wide coverage verdict: PASS (86.38% line at or above 80, 80.68% branch at or above 75; both rose against the merge-base baseline).

C# new-code coverage verdict: PASS (`ApplicationSettingsStore.cs` class `line-rate="1" branch-rate="1"`; `AppAutoFileObjects.ScalarSettings.cs` class `line-rate="1" branch-rate="1"`; `ISettingsStore.cs` is interface-only with no class node).

C# changed-line coverage verdict: PASS (every instrumented added or modified line is covered; the remediation changed no production line, and the prior review's line-level read of `IDList`, `AppQuickFilerSettings` and `AppStagingFilenames` still applies).

C# modified-file coverage verdict: PASS for the B-1 regression check (no modified file lost coverage against the merge base on any line). Per-file floor rows in section 5: `IDList.cs` 30.18% / 25.81% and `AppAutoFileObjects.cs` 40.81% / 37.10% remain below the 80% / 75% per-file floor at both points (pre-existing), recorded as FAIL rows dispositioned non-blocking because both improved or held against the merge base with no changed-line regression (N-1, N-2).

### 1.3 Scenario completeness

PASS. Positive (round-trip, forwarding, cold-cache maximum), negative (null store, null settings), edge (shorter ID does not save, longer ID saves again, `EmailInfoStagingFile` does not save, warm cache returns the cached value without persisting) and state-transition (save count across successive IDs) scenarios are present. The B-1 gap (no test of `IDList.MaxLengthOfID`) is closed by `MaxLengthOfID_ColdCacheOnMixedLengthList_ReturnsLongestIdLength`, `MaxLengthOfID_ColdCacheOnSecondDistinctList_ReturnsItsOwnLongestIdLength` and `MaxLengthOfID_WarmCacheAfterGetNextToDoID_ReturnsCachedLengthWithoutSaving`.

## 2. General Code Change Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| Design: simplicity and separation of concerns | PASS | A two-member interface and a small adapter; each consumer gains one field and one constructor overload. |
| No static mutable seam | PASS | The store is a per-instance `readonly` field in all four consumers. |
| Production behaviour preserved | PASS | Generated settings properties are `this[name]` reads and writes, so the store indexer plus `Save()` is equivalent; `ApplicationGlobals` still calls the public parameterless constructors. Cycle 1 changed no production file. |
| Error handling | PASS | Null stores and null settings are rejected with `ArgumentNullException` at construction. |
| File size (500 lines) | PASS | `file-sizes.2026-10-22T16-37.md`: created or modified `.cs` files at most 477 lines except the two pre-existing oversize files, which shrank (`DfDeedle_COM_Tests.cs` 848 to 809, `AppAutoFileObjects.cs` 853 to 791). `IDListGetNextToDoIDTests.cs` is 287 lines at head (reviewer read; 235 plus 52 added). |
| Public contracts documented | PASS | XML documentation on `ISettingsStore`, `ApplicationSettingsStore`, the new `IDList` constructors and the new consumer constructors. |
| Minimal public surface | PASS | `public` only where no `InternalsVisibleTo` grant exists (spec section 9); TaskMaster overloads are `internal`. |
| Dependencies | PASS | No package added. |
| Bugfix workflow (failing test first) | PASS | Probe tests failed before the production change (saves 1, 1, 1, 6) and pass after; DfDeedle fail-before-exception dossier recorded. |
| Requirements traceability | PASS | Every AC4 site maps to a `_settingsStore.Save()` call (`ac4-search-after.2026-10-22T16-27.md`). |
| Host-identifier hygiene in committed files | PASS | Reviewer Grep of the feature folder for drive-rooted paths, `\Users\` and the account name: 0 matches outside this audit family's own description of the search. Cycle-1 evidence refers to build logs as `SCRATCH`. |

## 3. Language-Specific Code Change Policy Compliance

### 3.1 C# (`CLAUDE.md` C# Code Change Policy, `.claude/rules/csharp.md`)

| Rule | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `remediation-format.2026-10-22T17-09.md`: format left the tree unchanged on the final pass; `csharpier check .` exit 0 over 1682 files. |
| Analyzer rebuild | PASS | `remediation-analyzers.2026-10-22T17-09.md`: `/t:Rebuild`, 0 warnings, 0 errors. |
| Nullable rebuild (warnings as errors) | PASS | `remediation-typecheck.2026-10-22T17-09.md`: `/t:Rebuild`, 0 warnings, 0 errors, no `/p:Nullable=enable`. |
| Legacy `<Compile Include>` registration | PASS | Entries present for every new `.cs` file (prior review); cycle 1 adds no file. |
| C# 7.3 in `ToDoModel.Test` and `TaskTree.Test` | PASS | Cycle-1 tests use `var`, collection initializers and Moq only; no target-typed `new()`, `is not`, switch expressions, `using` declarations or nullable annotations. |
| No production change to DfDeedle | PASS | No file under `UtilitiesCS/Extensions/` in the diff. |
| Coverage exclusions | PASS | 0 added `ExcludeFromCodeCoverage`; `coverage.config` not in the diff. |
| Suppressions | PASS | No `#pragma` or `SuppressMessage` added. |

## 4. Language-Specific Unit Test Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| MSTest attributes | PASS | `[TestClass]` and `[TestMethod]` throughout, including the three cycle-1 tests. |
| Moq | PASS | `Mock<ISettingsStore>` in `ToDoModel.Test` and `TaskTree.Test`; the warm-cache test asserts `store.Verify(s => s.Save(), Times.Once())`. |
| FluentAssertions | PASS | All new assertions use FluentAssertions. |
| No serialization as a fix | PASS | No `[DoNotParallelize]` added; two class-level attributes removed. No runsettings file in the diff. |
| `[DoNotParallelize]` removal justified (AC8) | PASS | `evidence/other/ribbon-static-state-check.2026-10-22T16-23.md`. |
| Test location | PASS | Tests sit in the sibling `.Test` projects mirroring production folders. |

## 5. Test Coverage Detail

Per-file figures are the Cobertura `<class>` `line-rate` and `branch-rate` attributes for each `filename` in `artifacts/csharp/coverage.xml` (head) and, for the merge base, the figures recorded in the prior review and `coverage-comparison.2026-10-22T17-12.md`.

| File | Status | Baseline line / branch | Post-change line / branch | Gate |
|---|---|---|---|---|
| `UtilitiesCS/HelperClasses/Settings/ApplicationSettingsStore.cs` | new | absent | 100% / 100% | new code at least 90%: PASS |
| `UtilitiesCS/HelperClasses/Settings/ISettingsStore.cs` | new | absent | no executable lines | interface only |
| `TaskMaster/AppGlobals/AppAutoFileObjects.ScalarSettings.cs` | new | absent | 100% / 100% | new code at least 90%: PASS |
| `ToDoModel/Data Model/ID/IDList.cs` | modified | 24.36% (38/156) / 20.69% | 30.18% (51/169) / 25.81% | per-file floor FAIL (pre-existing), dispositioned non-blocking (N-2); no regression: PASS |
| `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` | modified | 100% / 100% | 100% / 100% | PASS |
| `TaskMaster/AppGlobals/AppStagingFilenames.cs` | modified | 30.77% / 30.00% | 100% / 100% | PASS |
| `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | modified | 45.66% (184/403) / 37.10% | 40.81% (151/370) / 37.10% | per-file floor FAIL (pre-existing), dispositioned non-blocking (N-1, move only) |

B-1 closure detail. Head method `get_MaxLengthOfID` of class `ToDoModel.IDList` carries `line-rate="1" branch-rate="1"`; its seven line nodes (107 to 113) are all `hits="1"` and both branch lines (108 and 110) read `condition-coverage="100% (2/2)"`, matching the merge-base 7/7 lines and 4/4 branches. Arithmetic cross-check: prior head covered 44 of 169 lines; adding the getter's 7 gives 51 of 169 = 30.18%, the observed rate, so no other `IDList` line moved. The new comparison `coverage-comparison.2026-10-22T17-12.md` reports both line and branch rates for all three states and names the getter, which closes the disclosure half of B-1.

## 6. Test Execution Metrics

- MSTest full suite with coverage (cycle-1 final pass): 7621 total, 7621 passed, 0 failed, 0 skipped (`remediation-tests.2026-10-22T17-12.md`, derived from the trx). Prior pass: 7618; the delta of 3 equals the three added `MaxLengthOfID` tests.
- Parallel configuration: `scripts/vscode/TaskMaster.cli.runsettings` (MSTest `Workers` 0, `Scope` ClassLevel), `/InIsolation`.
- Targeted run: `maxlengthofid-tests.2026-10-22T17-09.md`, 3 executed, 3 passed, parallelization enabled.
- Repeated parallel runs of the affected classes: `parallel-repeat-runs.2026-10-22T16-30.md` (five combined runs, ten solo runs, zero failures).
- The reviewer did not re-run the suites (Bash limited to `git diff` and `git log`); coverage figures were re-read from the Cobertura document.

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
|---|---|---|---|
| Confidentiality masking scan | Grep of the feature folder for drive-rooted paths, `\Users\` and the account name | 0 matches in evidence or plan files | PASS |
| Suppression scan (added lines) | `git diff 346651242..HEAD` read for `ExcludeFromCodeCoverage` (prior review `-G` scan; cycle-1 diff read in full) | none added | PASS |
| Serialization scan | prior review `git diff -G` scan for `DoNotParallelize`, `Thread\.Sleep`, `Task\.Delay`, `\[Timeout`, `Retry`, `Workers`, `GetTemp`; cycle-1 diff read in full | only two removals of `[DoNotParallelize]` | PASS |
| Workflow change scan | `git diff 346651242..HEAD --stat` filtered for `.github/` | empty | PASS |
| Raw evidence scan | `git diff 346651242..HEAD --stat` filtered for `.trx`, `.coverage`, Cobertura and `artifacts/` | empty; only two `.jacoco.xml` projections under `evidence/` | PASS |
| Scope fence scan | `git diff e399de597..3df0782ab --stat` | no production, project or runsettings file | PASS |

## Evidence Location Compliance

`git diff --stat 346651242..HEAD` lists no file under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`. All committed evidence, including the cycle-1 files, is under `<FEATURE>/evidence/{baseline,regression-testing,qa-gates,other}/` as Markdown summaries and two package-level JaCoCo projections, which `CLAUDE.md` "Committed Test Evidence Format" permits. `validate_evidence_locations.py --root .` could not be run because the caller restricted the Bash tool to `git diff` and `git log`; the check was performed by diff inspection. Verdict: PASS.

## 8. Gaps and Exceptions

Blocking: none.

Resolved:

- B-1 (prior review): `IDList.MaxLengthOfID` coverage restored to 7/7 lines and 4/4 branches; `IDList.cs` branch rate 25.81% against the merge-base 20.69%; disclosure corrected in `coverage-comparison.2026-10-22T17-12.md`.

Non-blocking:

- N-1: `AppAutoFileObjects.cs` per-file line rate fell from 45.66% to 40.81% only because covered scalar-property lines moved into the new partial (100%); the file is below the 80% floor at both points (pre-existing, Outlook-bound members).
- N-2: `IDList.cs` remains below the 80% / 75% per-file floor (30.18% / 25.81%), pre-existing and improved against the merge base; the uncovered residue is the unchanged Outlook-bound and deserializing constructors.
- O-1: The TaskMaster CI checks (actionlint, build-analyzers, build-nullable, format-check, hygiene, mstest-coverage, pester) have not run because no pull request exists; they are a PR-time gate, not an acceptance criterion and not a blocking finding for this review.
- O-2: Out-of-scope items tracked by issue #1006 were not raised as findings.
- O-3: The prior O-1 (stale PR context) is closed: `artifacts/pr_context.summary.txt` now reports head `3df0782ab` and merge base `346651242`.

## 9. Summary of Changes

- Commit `39f38024c`: promotion of the bug to the active feature folder.
- Commit `0a6f080ef`: research, spec and preflight-cleared plan.
- Commit `e399de597`: settings-store seam, DfDeedle fake-clock tests, converted settings tests, probes, `[DoNotParallelize]` removals, evidence.
- Commit `85fb603b9`: first feature-review artifacts.
- Commit `3df0782ab`: three `MaxLengthOfID` tests, remediation plan and its evidence.

## 10. Compliance Verdict

**PASS.** Toolchain, parallel-isolation rules, repository coverage, new-code coverage, changed-line coverage, file sizes, scope fences and evidence form pass. Prior blocking finding B-1 is resolved. No blocking finding remains.

## Appendix A: Test Inventory

| Test file | Tests | Status | Scope |
|---|---|---|---|
| `UtilitiesCS.Test/HelperClasses/Settings/ApplicationSettingsStoreTests.cs` | 4 | new | adapter null guard and forwarding |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.QfcColumnsAsync.cs` | 2 | new (moved and converted) | `AddQfcColumnsAsync` with a fake clock |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | 24 | modified (`partial`, two tests moved out) | unchanged tests |
| `ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs` | 16 | modified (10 added, 3 in cycle 1) | ID arithmetic, persistence branch, `MaxLengthOfID` cache |
| `TaskTree.Test/TaskTreeControllerMoveLogicTests.cs` | 17 | modified | move logic with injected store and probe |
| `TaskTree.Test/TaskTreeControllerRouteDropTests.cs` | 7 | modified | drop routing with injected store |
| `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsTests.cs` | 6 | modified (attribute removed) | high-confidence settings |
| `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsRemainingPropertiesTests.cs` | 6 | modified | remaining boolean settings |
| `TaskMaster.Test/AppGlobals/AppStagingFilenamesTests.cs` | 10 | modified | staging file names |
| `TaskMaster.Test/AppGlobals/AppAutoFileObjectsCoverageExpansionTests.cs` | 10 | modified | scalar settings and initialization paths |
| `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` | 17 (with `Engines` partial) | modified (attribute removed) | ribbon high-confidence helpers |
| `TaskMaster.Test/AppGlobals/InMemorySettingsStore.cs` | 0 | new helper | per-test fake store |
| `TaskMaster.Test/AppGlobals/SettingsSaveProbe.cs` | 0 | new helper | thread-attributed save probe |

## Appendix B: Toolchain Commands Reference

| Stage | Command | Run by |
|---|---|---|
| C# format | `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` | executor |
| C# analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | executor |
| C# type check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | executor |
| C# test and coverage | `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1` | executor |
| Targeted test run | `vstest.console.exe ToDoModel.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:<three MaxLengthOfID tests>` | executor |
| Diff scope | `git diff 346651242..HEAD --stat`; `git diff e399de597..3df0782ab --stat`; `git diff e399de597..3df0782ab -- <test file>` | reviewer |
| Coverage reading | Grep of `filename="ToDoModel\Data Model\ID\IDList.cs"` and `name="get_MaxLengthOfID"` in `artifacts/csharp/coverage.xml` | reviewer |
