# Feature Audit: Parallel test failures from wall-clock deadlines and shared user.config (Issue #1005)

- Review timestamp: 2026-10-22T17-10 (first review of this branch)
- Reviewer: feature-review agent

## Scope and Baseline

- Branch: `bug/parallel-test-failures-wall-clock-and-shared-user-config-1005`, head `e399de597`.
- Resolved base branch: `main`, merge base `346651242a2bd6ed25d703a4b1ecb5a055c6ae72` (caller-supplied; `origin/main` has not advanced past it). The audit diff is `git diff 346651242..HEAD` (73 files).
- Work mode: `full-bug` (`- Work Mode: full-bug` in `issue.md`); AC source: `spec.md` `## Acceptance Criteria` only. `issue.md` AC1 to AC7 are the maintainer's original statement and are carried into `spec.md`; `user-story.md` carries no criteria.
- PR context: `artifacts/pr_context.summary.txt` and `.appendix.txt` describe the #1002 branch and are stale for this branch; they could not be regenerated under the caller's Bash restriction (`git diff` and `git log` only), so scope was derived from `git diff`.
- Evidence basis: executor projections under `evidence/{baseline,regression-testing,qa-gates,other}/`, the canonical coverage artifact `artifacts/csharp/coverage.xml`, the retained merge-base Cobertura document, and reviewer reads of source and diff.

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/spec.md`, `## Acceptance Criteria`. Eight checkbox items, AC1 to AC8, all checked `[x]` at head.

| AC | Subject |
|---|---|
| AC1 | DfDeedle happy-path test independent of wall-clock time |
| AC2 | `IDList.GetNextToDoID` persists through an injected `ISettingsStore` |
| AC3 | Every test reaching `GetNextToDoID` injects a store; probe records zero saves |
| AC4 | Before and after search; every reached settings save and seamless `AddQfcColumnsAsync` call fixed |
| AC5 | Fail-before evidence for each fix |
| AC6 | Full suite passes in parallel; no serialization, sleeps, retries, temp files or `user.config` writes |
| AC7 | Full C# toolchain clean; coverage gates; file sizes |
| AC8 | `[DoNotParallelize]` removal with a recorded static-state check |

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC1 | PASS | `DfDeedle_COM_Tests.QfcColumnsAsync.cs`: both tests are `async Task`, pass `timeProvider: new FakeTimeProvider()` (never advanced), leave `columnAdder` at the production default, and await with `NotThrowAsync`; no `GetAwaiter().GetResult()`, sleep or delay. Happy path verifies `Add("SentOn")` once. No file under `UtilitiesCS/Extensions/` is in the diff. Elapsed 2.1 to 2.7 ms in six parallel runs (`parallel-repeat-runs.2026-10-22T16-30.md`). |
| AC2 | PASS | `IDList.cs`: field initializer defaults `_settingsStore` to `new ApplicationSettingsStore(Properties.Settings.Default)` for every existing constructor; two new public constructors null-guard the store; `GetNextToDoID` assigns `MaxLengthOfID` through the indexer and calls `_settingsStore.Save()` under the unchanged condition. Tests `GetNextToDoID_FirstIdOnFreshList_AssignsLengthAndSavesOnce`, `..._IdNotLongerThanCachedLength_DoesNotSaveAgain`, `..._LongerIdAfterFirst_SavesAgain`, two null-store constructor tests and `Constructor_Default_UsesApplicationSettingsStore` pass. `CreateListWithCachedMaxLength` is deleted. Note: that deletion removed the only coverage of `MaxLengthOfID` (finding B-1); AC2 does not address that member, so the verdict is unaffected. |
| AC3 | PASS | `TaskTreeControllerMoveLogicTests.MakeController` and `TaskTreeControllerRouteDropTests.MakeController` return `new IDList(new List<string>(), store.Object)`; every `IDListGetNextToDoIDTests` list is store-injected (the default-constructed list in `Constructor_Default_UsesApplicationSettingsStore` never calls `GetNextToDoID`). `MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam` asserts 0 probe saves and verifies one mock `Save()`; it failed with 1 save before the fix (`settings-probe-fail-before.2026-10-22T16-15.md`) and passes after (`idlist-probe-pass-after.2026-10-22T16-18.md`). |
| AC4 | PASS | `ac4-search-before.2026-10-22T16-12.md` and `ac4-search-after.2026-10-22T16-27.md`. After: production `Settings.Default.Save()` remains only in `AppOlObjects.JunkFolders.cs` (3) and `AppOlObjects.cs` (1), both unreached by tests (spec non-goal); `_settingsStore.Save()` counts 1, 6, 9, 6 in `IDList`, `AppQuickFilerSettings`, `AppStagingFilenames`, `AppAutoFileObjects.ScalarSettings`; the test-code `defaults.Save()` is deleted; both `AddQfcColumnsAsync` calls pass the fake clock. Reviewer Grep of every `*.Test` project for `new IDList(`, `new AppQuickFilerSettings()`, `new AppStagingFilenames()`, `new AppAutoFileObjects(` and for the fifteen setter names confirms the classification: default-constructed instances never call a saving member, and other projects only mock the getters. |
| AC5 | PASS | Probe fail-before run: 4 of 4 failed on the probe assertion with 1, 1, 1 and 6 saves, each cancelled so nothing was written; pass-after projections for the IDList path and the three TaskMaster classes. DfDeedle: `fail-before-exception.2026-10-22T16-15.md` carries `WhyFailingRunImpossible`, quotes the `issue.md` exception, and cites the before-fix AC4 search. |
| AC6 | PASS | `final-toolchain-tests.2026-10-22T16-35.md`: 7618 of 7618 passed under `scripts/vscode/TaskMaster.cli.runsettings` (`Workers` 0, `Scope` ClassLevel). Fifteen repeat runs with zero failures. Reviewer `git diff -G` scan: no added `[DoNotParallelize]`, `Thread.Sleep`, `Task.Delay`, `[Timeout]`, retry, `Workers` or temp-path token; no runsettings file in the diff; no test reaches a real settings save. |
| AC7 | PASS | Final pass in CLAUDE.md order: CSharpier format and check clean (pass 2), analyzer `/t:Rebuild` 0/0, warnings-as-errors `/t:Rebuild` 0/0 without a Nullable override, `Invoke-MSTestWithCoverage.ps1` 7618 passed. Changed lines: every instrumented changed line covered (reviewer read of Cobertura line nodes). `ApplicationSettingsStore` and `AppAutoFileObjects.ScalarSettings` class rates 1.0; every new or changed seam member in `IDList`, `AppQuickFilerSettings`, `AppStagingFilenames` at 100%. 0 added `[ExcludeFromCodeCoverage]`. File sizes: all at or under 500 except `DfDeedle_COM_Tests.cs` 848 to 809 and `AppAutoFileObjects.cs` 853 to 791. The B-1 regression is on an unchanged member and is outside AC7's literal clauses; it is a policy finding, not an AC7 failure. |
| AC8 | PASS | `[DoNotParallelize]` removed from `AppQuickFilerSettingsTests` and `RibbonControllerTests`. `evidence/other/ribbon-static-state-check.2026-10-22T16-23.md` records the whole-class check with verdict REMOVE. Reviewer cross-check: `RibbonController()` has an empty body; `TaskMaster/Ribbon` statics are `static readonly` loggers, immutable catalog maps and pure helpers; the `Engines` partial uses only per-test instances. |

## Summary

All eight acceptance criteria evaluate PASS against code and evidence. The review nevertheless returns **REMEDIATION_REQUIRED** with one blocking policy finding, B-1 (autonomous): removing the cache-priming test helper, as AC2 requires, also removed the only test execution of the public `IDList.MaxLengthOfID` getter (7 of 7 lines and 4 of 4 branches covered at the merge base, none at head), the `IDList.cs` branch rate regressed from 20.69% to 19.35%, and the committed coverage comparison does not report it. The remedy is test-only: add `MaxLengthOfID` assertions to `IDListGetNextToDoIDTests.cs`, rerun the C# toolchain, and correct the coverage comparison.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/spec.md`
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none

## Acceptance Criteria Check-off

All eight items were already `- [x]` in `spec.md` when this review began, and all eight evaluate PASS, so no box was changed and no criterion text was edited. No checked item is disputed. Finding B-1 is a coverage-policy finding outside the literal text of every criterion; the reviewer did not uncheck any item for it.
