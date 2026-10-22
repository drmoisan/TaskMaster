# Policy Audit: Parallel test failures from wall-clock deadlines and shared user.config (Issue #1005)

- Branch: `bug/parallel-test-failures-wall-clock-and-shared-user-config-1005`
- Head: `e399de597`
- Base: `main` (resolved `origin/main`) at merge base `346651242a2bd6ed25d703a4b1ecb5a055c6ae72`
- Review timestamp: 2026-10-22T17-10 (first review of this branch)
- Work mode: `full-bug` (AC source: `spec.md` only)
- Reviewer: feature-review agent

## Template Provenance

The structure of this artifact follows the canonical major headings listed in `.claude/skills/policy-audit-template-usage/SKILL.md` (Executive Summary, sections 1 to 10, Appendix A and Appendix B), in the same shape as the most recent validated audit in this repository (`docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/policy-audit.2026-10-22T14-10.md`). The template-asset selector step was not run from this agent session. The artifact is validated with `validate_orchestration_artifacts` (artifact type `policy-audit`).

## Rejected Scope Narrowing

No scope narrowing was detected. The caller prompt states: "Scope decision by the orchestrator: tests that only mutate Settings.Default in memory without saving, and the per-method [DoNotParallelize] in ApplicationGlobalsStartupTimingTests, are out of scope and tracked as issue #1006; do not raise them as blocking findings for this item." This restates the non-goals of `spec.md` section 4 and directs finding disposition; it does not remove any file, language, toolchain step or coverage check from the audit. The audit scope is the full branch diff `346651242...e399de597` (73 files), and the reviewer still verified that the named out-of-scope tests do not reach a settings `Save()` (section 8).

The Bash tool was limited by the caller to `git diff` and `git log` command forms. This is an execution constraint, not a scope narrowing: every figure below was read from committed evidence, the canonical coverage artifact, the retained baseline Cobertura document, or the source tree.

## Executive Summary

The branch fixes two parallel-execution test failures. The DfDeedle happy-path and pre-cancelled tests now pass a never-advanced `FakeTimeProvider` through the existing `timeProvider` parameter and are awaited `async Task` tests (test-only change). A new per-instance `ISettingsStore` seam (`UtilitiesCS`) with a production adapter `ApplicationSettingsStore` routes every test-reached settings save in `IDList`, `AppQuickFilerSettings`, `AppStagingFilenames` and the `AppAutoFileObjects` scalar properties through an injected store; production defaults wrap the assembly's own `Properties.Settings.Default`, so production writes and saves are unchanged. Tests inject `Mock<ISettingsStore>` or an `InMemorySettingsStore`, and the class-level `[DoNotParallelize]` is removed from `AppQuickFilerSettingsTests` and `RibbonControllerTests`.

Overall verdict: **REMEDIATION_REQUIRED (1 blocking finding)**.

- Toolchain: PASS (format, format check, analyzer rebuild, warnings-as-errors rebuild, MSTest with coverage; 7618 of 7618 tests passed under the class-level parallel runsettings).
- Parallel-isolation rules (no serialization, no sleeps, no temp files, no `user.config` redirection, no real save): PASS.
- Repository C# coverage: PASS (86.37% line, 80.66% branch; floors 80% and 75% from root `CLAUDE.md`).
- New and changed C# code: PASS (new files 100%; every instrumented changed line covered).
- Modified-file C# coverage: FAIL. `ToDoModel/Data Model/ID/IDList.cs` lost coverage of the unchanged `MaxLengthOfID` getter (7 of 7 lines and 4 of 4 branches covered at the merge base, 0 at head) because the removed test helper was its only caller; the file's branch rate fell from 20.69% to 19.35%. The committed coverage comparison does not report this. Finding B-1 (autonomous).

## 1. General Unit Test Policy Compliance

### 1.1 Core principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Every converted test builds its own store (`InMemorySettingsStore` or `Mock<ISettingsStore>`); the `Settings.Default` snapshot/restore blocks and the `TestCleanup` `defaults.Save()` are deleted. |
| Isolation | PASS | New tests each assert one behaviour (null guard, forwarding, save count, persistence branch). |
| Fast execution | PASS | `AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing` 2.1 to 2.7 ms across six runs (`parallel-repeat-runs.2026-10-22T16-30.md`), down from a 9 s wall-clock budget. |
| Determinism | PASS | The DfDeedle tests pass a `FakeTimeProvider` that is never advanced, so no deadline can fire. 0 added `Thread.Sleep`, `Task.Delay`, `[Timeout]` or retry tokens (reviewer `git diff -G` over `*.cs`, `*.runsettings`, `*.config`, `*.csproj`: the only matching files are the two that remove `[DoNotParallelize]`). |
| Readability | PASS | Descriptive names, Arrange/Act/Assert comments, FluentAssertions `because` text on probe assertions. |
| No temporary files | PASS | 0 `GetTempPath`, `GetTempFileName` or `Path.GetTemp` tokens in the diff. `ApplicationSettingsStoreTests` uses a dictionary-backed `ApplicationSettingsBase` subclass whose indexer and `Save` overrides never reach the provider. |
| No shared settings file write | PASS | The thread-attributed `SettingsSaving` probe cancels any save it observes, so the fail-before run wrote nothing; after the fix it records 0 saves on four representative tests (`settings-probe-fail-before.2026-10-22T16-15.md`, `idlist-probe-pass-after.2026-10-22T16-18.md`, `taskmaster-settings-probe-pass-after.2026-10-22T16-25.md`). |

### 1.2 Coverage requirements

Coverage thresholds applied: the root `CLAUDE.md` states C# line `>= 80%` and branch `>= 75%`, new modules, classes and methods `>= 90%`, and "Code changes or refactors must not reduce coverage for the lines that were changed"; these govern under the threshold precedence of `.claude/rules/general-unit-test.md`. The canonical artifact `artifacts/csharp/coverage.xml` is the post-processed Cobertura of the final pass; its root attributes `lines-covered="57627" lines-valid="66720" branches-covered="14060" branches-valid="17431"` equal the figures in `evidence/qa-gates/final-toolchain-tests.2026-10-22T16-35.md`. The merge-base baseline Cobertura document named in `evidence/baseline/baseline-coverage.2026-10-22T16-10.md` is retained in the session scratch folder and was read by the reviewer for the per-member comparison in section 5. `quality-tiers.yml` is absent at the repository root, so tier classification is not a finding.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 7 production (3 new, 4 modified), 13 test (4 new, 9 modified), 4 project files | MSTest | 7618 passed, 0 failed | 86.30% | 86.37% | 100% |
| TypeScript | 0 | none | none | N/A | N/A | N/A |
| Python | 0 | none | none | N/A | N/A | N/A |
| PowerShell | 0 | none | none | N/A | N/A | N/A |

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/baseline-coverage.2026-10-22T16-10.md` with projection `evidence/baseline/baseline-coverage-projection.2026-10-22T16-10.jacoco.xml`
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml` with summaries `evidence/qa-gates/final-toolchain-tests.2026-10-22T16-35.md` and `evidence/qa-gates/coverage-comparison.2026-10-22T16-36.md`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 86.30% lines (57542/66677) / 80.58% branches (14036/17419) -> Post-change: 86.37% lines (57627/66720) / 80.66% branches (14060/17431). Change: +0.07% lines, +0.08% branches. New/changed-code coverage: 100%. Disposition: FAIL. Evidence: artifacts/csharp/coverage.xml; evidence/qa-gates/coverage-comparison.2026-10-22T16-36.md; reviewer per-member read of both Cobertura documents (section 5, finding B-1).
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Verdicts

C# repo-wide coverage verdict: PASS (86.37% line at or above 80, 80.66% branch at or above 75; both rose against the merge-base baseline).

C# new-code coverage verdict: PASS (`ApplicationSettingsStore.cs` class `line-rate="1" branch-rate="1"`; `AppAutoFileObjects.ScalarSettings.cs` class `line-rate="1" branch-rate="1"`; `ISettingsStore.cs` is interface-only with no class node).

C# changed-line coverage verdict: PASS (every instrumented added or modified line is covered; reviewer confirmed `hits="1"` on the new `IDList` constructor lines 73 to 77 and 86 to 90, the field-initializer lines 97 to 99, and every `GetNextToDoID` line; class rates 1.0 for `AppQuickFilerSettings` and `AppStagingFilenames`).

C# modified-file coverage verdict: FAIL. `IDList.cs` 24.36% / 20.69% at baseline to 26.04% / 19.35% at head: below the 80% / 75% per-file floor at both points (pre-existing), and its branch rate regressed because the unchanged `MaxLengthOfID` getter lost all coverage (B-1, blocking). `AppAutoFileObjects.cs` 45.66% / 37.10% to 40.81% / 37.10%: below floor at both points; the line drop is entirely the move of 33 covered scalar-property lines into the new partial `AppAutoFileObjects.ScalarSettings.cs` (100%), and the class across both files rises to 46.06% (187/406); dispositioned non-blocking (N-1). `AppQuickFilerSettings.cs` 100% / 100% both points: PASS. `AppStagingFilenames.cs` 30.77% / 30.00% to 100% / 100%: PASS.

### 1.3 Scenario completeness

PASS with one gap. Positive (round-trip, forwarding), negative (null store, null settings), edge (shorter ID does not save, longer ID saves again, `EmailInfoStagingFile` does not save, cached field short-circuits the getter) and state-transition (save count across successive IDs) scenarios are present. The gap is B-1: the `IDList.MaxLengthOfID` lazy-cache getter, a public `IIDList` member, is no longer exercised by any test.

## 2. General Code Change Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| Design: simplicity and separation of concerns | PASS | A two-member interface and a four-line adapter; each consumer gains one field and one constructor overload. Persistence is isolated behind the seam. |
| No static mutable seam | PASS | The store is a per-instance `readonly` field in all four consumers; no static setter or ambient provider is introduced. |
| Production behaviour preserved | PASS | Generated settings properties are `this[name]` reads and writes, so `(bool)store[name]`, `(double)store[name]`, `(int)store[name]` and `store[name] = value` followed by `Save()` are equivalent. `ApplicationGlobals` still calls the public parameterless constructors; `AppAutoFileObjects(IApplicationGlobals)` chains to the store overload with the default adapter. |
| Error handling | PASS | Null stores and null settings are rejected with `ArgumentNullException` at construction. |
| File size (500 lines) | PASS | `evidence/qa-gates/file-sizes.2026-10-22T16-37.md`: every created or modified `.cs` file at most 477 lines except the two pre-existing oversize files, which shrank (`DfDeedle_COM_Tests.cs` 848 to 809, `AppAutoFileObjects.cs` 853 to 791). Reviewer cross-check: `IDList.cs` reads 352 lines. |
| Public contracts documented | PASS | XML documentation on `ISettingsStore`, `ApplicationSettingsStore`, the new `IDList` constructors and the new consumer constructors. |
| Minimal public surface | PASS | `ISettingsStore`, `ApplicationSettingsStore` and the new `IDList` constructors are `public` because `UtilitiesCS` and `ToDoModel` grant no `InternalsVisibleTo` to the consuming test assemblies (spec section 9); the TaskMaster overloads are `internal`. |
| Dependencies | PASS | No package added; `Microsoft.Extensions.TimeProvider.Testing` was already referenced by `UtilitiesCS.Test`. |
| Bugfix workflow (failing test first) | PASS | Four probe tests fail before the production change with recorded save counts 1, 1, 1 and 6, then pass after; the DfDeedle path has a fail-before-exception dossier (`fail-before-exception.2026-10-22T16-15.md`) quoting the maintainer's captured exception and citing the before-fix search. |
| Requirements traceability | PASS | Every AC4 site named in `spec.md` maps to a `_settingsStore.Save()` call (`ac4-search-after.2026-10-22T16-27.md` Q2: 1, 6, 9, 6). |
| Host-identifier hygiene in committed files | PASS | Reviewer search of the feature folder and the four added agent-memory notes for drive-rooted paths, `\Users\` and the account name: 0 matches. |

## 3. Language-Specific Code Change Policy Compliance

### 3.1 C# (`CLAUDE.md` C# Code Change Policy, `.claude/rules/csharp.md`)

| Rule | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `evidence/qa-gates/final-toolchain-format.2026-10-22T16-32.md`: clean pass 2, 0 rewritten files, `csharpier check .` exit 0 over 1682 files. |
| Analyzer rebuild | PASS | `final-toolchain-analyzers.2026-10-22T16-32.md`: `/t:Rebuild`, 0 warnings, 0 errors. |
| Nullable rebuild (warnings as errors) | PASS | `final-toolchain-typecheck.2026-10-22T16-33.md`: `/t:Rebuild`, 0 warnings, 0 errors, no `/p:Nullable=enable`; the new `UtilitiesCS` files carry `#nullable enable`. |
| Legacy `<Compile Include>` registration | PASS | Diff adds entries for `ApplicationSettingsStore.cs` and `ISettingsStore.cs` (`UtilitiesCS.csproj`), `AppAutoFileObjects.ScalarSettings.cs` (`TaskMaster.csproj`), `DfDeedle_COM_Tests.QfcColumnsAsync.cs` and `ApplicationSettingsStoreTests.cs` (`UtilitiesCS.Test.csproj`), `SettingsSaveProbe.cs` and `InMemorySettingsStore.cs` (`TaskMaster.Test.csproj`). |
| C# 7.3 in `ToDoModel.Test` and `TaskTree.Test` | PASS | Added lines in those projects use no target-typed `new()`, `is not`, switch expressions, `using` declarations or nullable annotations; both compile in the final rebuilds. |
| No production change to DfDeedle | PASS | `git diff --stat` shows no file under `UtilitiesCS/Extensions/`. |
| Coverage exclusions | PASS | 0 added `ExcludeFromCodeCoverage` lines (reviewer `git diff -G` scan); `coverage.config` not in the diff. |
| Suppressions | PASS | No `#pragma` or `SuppressMessage` added. |

## 4. Language-Specific Unit Test Policy Compliance

| Rule | Verdict | Evidence |
|---|---|---|
| MSTest attributes | PASS | `[TestClass]` and `[TestMethod]` throughout; the DfDeedle partial adds `[TestMethod]` members to the existing `[TestClass]`. |
| Moq | PASS | `Mock<ISettingsStore>` in `ToDoModel.Test` and `TaskTree.Test`; `VerifySet` and `Verify` assert assignment and save counts. |
| FluentAssertions | PASS | All new assertions use FluentAssertions, including `NotThrowAsync` for the async DfDeedle tests. |
| No serialization as a fix | PASS | No `[DoNotParallelize]` added; two class-level attributes removed. No runsettings file in the diff. |
| `[DoNotParallelize]` removal justified (AC8) | PASS | `evidence/other/ribbon-static-state-check.2026-10-22T16-23.md` records no other mutable process-global state; reviewer search of `TaskMaster/Ribbon` for `static` finds only `static readonly` loggers, immutable catalog maps and pure helpers, and `RibbonController()` is an empty constructor. |
| Test location | PASS | Tests sit in the sibling `.Test` projects mirroring production folders (`UtilitiesCS.Test/HelperClasses/Settings/` for `UtilitiesCS/HelperClasses/Settings/`). |

## 5. Test Coverage Detail

Per-file figures are the Cobertura `<class>` `line-rate` and `branch-rate` attributes for each `filename` in `artifacts/csharp/coverage.xml` (head) and in the retained merge-base Cobertura document; member figures are the `<method>` line nodes.

| File | Status | Baseline line / branch | Post-change line / branch | Gate |
|---|---|---|---|---|
| `UtilitiesCS/HelperClasses/Settings/ApplicationSettingsStore.cs` | new | absent | 100% / 100% | new code at least 90%: PASS |
| `UtilitiesCS/HelperClasses/Settings/ISettingsStore.cs` | new | absent | no executable lines | interface only |
| `TaskMaster/AppGlobals/AppAutoFileObjects.ScalarSettings.cs` | new | absent | 100% / 100% | new code at least 90%: PASS |
| `ToDoModel/Data Model/ID/IDList.cs` | modified | 24.36% (38/156) / 20.69% | 26.04% (44/169) / 19.35% | FAIL (B-1, blocking) |
| `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` | modified | 100% / 100% | 100% / 100% | PASS |
| `TaskMaster/AppGlobals/AppStagingFilenames.cs` | modified | 30.77% / 30.00% | 100% / 100% | PASS |
| `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | modified | 45.66% (184/403) / 37.10% | 40.81% (151/370) / 37.10% | FAIL, dispositioned non-blocking (N-1, move only) |

B-1 detail. In the baseline document, method `get_MaxLengthOfID` of class `ToDoModel.IDList` has seven line nodes (73 to 79), all `hits="1"`, and two branch lines each `condition-coverage="100% (2/2)"`. In the head document the same method (lines 107 to 113) has all seven nodes `hits="0"` and both branch lines `0% (0/2)`. The only test caller was `IDListGetNextToDoIDTests.CreateListWithCachedMaxLength` (`_ = list.MaxLengthOfID;`), deleted as AC2 requires; no other test in the repository references `MaxLengthOfID`. Arithmetic cross-check: the diff adds 13 covered lines to `IDList.cs` (two constructors of five lines and three field-initializer lines), so the covered count would be 51 if nothing were lost; the head figure is 44, a loss of exactly the getter's seven lines. `evidence/qa-gates/coverage-comparison.2026-10-22T16-36.md` reports only line rates for this file and states that the uncovered residue is the unchanged Outlook-bound and deserializing constructors, which omits this loss. The CLAUDE.md changed-line rule is met literally (the getter's lines are unchanged), but the modified file is below the floor and regressed, so the non-blocking disposition used for pre-existing sub-floor files does not apply.

## 6. Test Execution Metrics

- MSTest full suite with coverage (final pass): 7618 total, 7618 passed, 0 failed, 0 error, 0 timeout (`final-toolchain-tests.2026-10-22T16-35.md`, derived from the trx). Baseline: 7599 passed; the delta of 19 equals the 19 register tests listed in that artifact.
- Parallel configuration: `scripts/vscode/TaskMaster.cli.runsettings` (MSTest `Workers` 0, `Scope` ClassLevel), `/InIsolation`.
- Repeated parallel runs: five combined runs of the ten affected classes (116 of 116 passed each) and ten solo runs in reverse order (all passed) (`parallel-repeat-runs.2026-10-22T16-30.md`).
- Fail-before and pass-after: `settings-probe-fail-before.2026-10-22T16-15.md` (4 of 4 failed on the probe assertion), `idlist-probe-pass-after.2026-10-22T16-18.md`, `taskmaster-settings-probe-pass-after.2026-10-22T16-25.md`, `dfdeedle-pass-after.2026-10-22T16-26.md`, `settings-store-adapter-tests.2026-10-22T16-17.md`.
- The reviewer did not re-run the suites (Bash limited to `git diff` and `git log`); coverage figures were re-read from the Cobertura documents.

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
|---|---|---|---|
| Confidentiality masking scan | Grep of the feature folder and the four added agent-memory notes for drive-rooted paths, `\Users\` and the account name | 0 matches | PASS |
| Suppression scan (added lines) | `git diff 346651242..HEAD -G "ExcludeFromCodeCoverage" --stat -- *.cs` | no file | PASS |
| Serialization scan | `git diff 346651242..HEAD -G <pattern> --stat`, pattern alternating `DoNotParallelize`, `Thread\.Sleep`, `Task\.Delay`, `\[Timeout`, `Retry`, `Workers` and `GetTemp`, over code, runsettings, config and project files | 2 files, both removing `[DoNotParallelize]` | PASS |
| Workflow change scan | `git diff --stat` filtered for `.github/` | empty | PASS |
| Raw evidence scan | `git diff --stat` filtered for `.trx`, `.coverage`, Cobertura and `artifacts/` | empty; only two `.jacoco.xml` projections under `evidence/` | PASS |
| Scope fence scan | `git diff --stat` for `IIDList`, `TreeOfToDoItems`, `TaskTreeController`, `AppToDoObjects`, `ApplicationGlobals`, runsettings | empty | PASS |

## Evidence Location Compliance

`git diff --stat 346651242..HEAD` lists no file under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/`. All committed evidence is under `<FEATURE>/evidence/{baseline,regression-testing,qa-gates,other}/` as Markdown summaries and two package-level JaCoCo projections, which `CLAUDE.md` "Committed Test Evidence Format" permits. `validate_evidence_locations.py --root .` could not be run because the caller restricted the Bash tool to `git diff` and `git log`; the check was performed by diff inspection. Verdict: PASS.

## 8. Gaps and Exceptions

Blocking:

- B-1 (autonomous): `IDList.MaxLengthOfID` lost all test coverage (7 lines, 4 branches) when the cache-priming helper was removed; `IDList.cs` branch rate regressed from 20.69% to 19.35%, and the committed coverage comparison does not disclose it. Remedy: add a `ToDoModel.Test` test that reads `MaxLengthOfID` on a non-empty store-injected list (cold cache computes the maximum length; warm cache returns the cached value after `GetNextToDoID`), and correct `coverage-comparison` to report the per-file branch figures.

Non-blocking:

- N-1: `AppAutoFileObjects.cs` per-file line rate fell from 45.66% to 40.81% only because 33 covered lines moved into the new partial; no remaining line lost coverage and the class across both files is 46.06%. The file is below the 80% floor at both points (pre-existing, Outlook-bound members).
- O-1: `artifacts/pr_context.summary.txt` and `.appendix.txt` describe the #1002 branch (head `e94c26130`), not this branch; the reviewer could not regenerate them under the Bash restriction, so scope was derived from `git diff 346651242..HEAD`.
- O-2: The TaskMaster CI checks (actionlint, build-analyzers, build-nullable, format-check, hygiene, mstest-coverage, pester) have not run because no pull request exists; they are a PR-time gate, not an acceptance criterion.
- O-3: Out-of-scope tests named in the caller prompt (issue #1006) were spot-checked: `AppOlObjectsTests.LoadJunkCertain_KeepsStoredValue_WhenReplacementSelectionIsCancelled` mutates `OlJunkCertain` in memory and returns before `LoadJunkCertain` reaches `Save()` (`AppOlObjects.JunkFolders.cs` lines 143 to 149); no test calls a `DarkMode` setter on `AppOlObjects`. Not raised as findings.

## 9. Summary of Changes

- Commit `39f38024c`: promotion of the bug to the active feature folder.
- Commit `0a6f080ef`: research, spec and preflight-cleared plan.
- Commit `e399de597`: `ISettingsStore` and `ApplicationSettingsStore`; store seam in `IDList`, `AppQuickFilerSettings`, `AppStagingFilenames` and the new `AppAutoFileObjects.ScalarSettings.cs` partial; DfDeedle tests moved to a partial and given a `FakeTimeProvider`; converted settings tests, `InMemorySettingsStore`, `SettingsSaveProbe`, TaskTree probe; removal of two class-level `[DoNotParallelize]`; evidence under `evidence/`; follow-up promotion record for issue #1006; agent-memory notes.

## 10. Compliance Verdict

**REMEDIATION_REQUIRED.** Toolchain, parallel-isolation rules, repository coverage, new-code coverage, changed-line coverage, file sizes, scope fences and evidence form pass. One blocking finding (B-1, autonomous) remains: a coverage regression in the modified file `IDList.cs` that the committed coverage comparison does not report.

## Appendix A: Test Inventory

| Test file | Tests | Status | Scope |
|---|---|---|---|
| `UtilitiesCS.Test/HelperClasses/Settings/ApplicationSettingsStoreTests.cs` | 4 | new | adapter null guard and forwarding |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.QfcColumnsAsync.cs` | 2 | new (moved and converted) | `AddQfcColumnsAsync` with a fake clock |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | 24 | modified (`partial`, two tests moved out) | unchanged tests |
| `ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs` | 13 | modified (7 added) | ID arithmetic and persistence branch |
| `TaskTree.Test/TaskTreeControllerMoveLogicTests.cs` | 17 | modified | move logic with injected store and probe |
| `TaskTree.Test/TaskTreeControllerRouteDropTests.cs` | 7 | modified | drop routing with injected store |
| `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsTests.cs` | 6 | modified (2 added, attribute removed) | high-confidence settings |
| `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsRemainingPropertiesTests.cs` | 6 | modified | remaining boolean settings |
| `TaskMaster.Test/AppGlobals/AppStagingFilenamesTests.cs` | 10 | modified (4 added) | staging file names |
| `TaskMaster.Test/AppGlobals/AppAutoFileObjectsCoverageExpansionTests.cs` | 10 | modified (2 added) | scalar settings and initialization paths |
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
| Parallel repeat runs | `vstest.console.exe` over the four test assemblies with `/Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation` | executor |
| Diff scope | `git diff 346651242..HEAD --stat`; `git diff 346651242..HEAD -- <path>`; `git diff -G <tokens> --stat` | reviewer |
| Coverage reading | Grep of `<class ... filename=...>` and `<method name="get_MaxLengthOfID">` in `artifacts/csharp/coverage.xml` and the retained baseline Cobertura document | reviewer |
