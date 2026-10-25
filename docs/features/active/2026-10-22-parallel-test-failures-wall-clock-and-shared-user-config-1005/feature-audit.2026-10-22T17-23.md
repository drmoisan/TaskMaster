# Feature Audit: Parallel test failures from wall-clock deadlines and shared user.config (Issue #1005)

- Review timestamp: 2026-10-22T17-23 (re-audit R4 after remediation cycle 1; prior review 2026-10-22T17-10)
- Reviewer: feature-review agent

## Scope and Baseline

- Branch: `bug/parallel-test-failures-wall-clock-and-shared-user-config-1005`, head `3df0782ab`.
- Resolved base branch: `main`, merge base `346651242a2bd6ed25d703a4b1ecb5a055c6ae72` (confirmed by `artifacts/pr_context.summary.txt`, generated 2026-10-22 17:21:51 UTC for head `3df0782ab`). The audit diff is `git diff 346651242..HEAD` (91 files).
- Work mode: `full-bug` (`- Work Mode: full-bug` in `issue.md`); AC source: `spec.md` `## Acceptance Criteria` only. `user-story.md` carries no criteria.
- Evidence basis: executor projections under `evidence/{baseline,regression-testing,qa-gates,other}/` including the cycle-1 files, the canonical coverage artifact `artifacts/csharp/coverage.xml` (cycle-1 final pass), and reviewer reads of source and diff.

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
| AC1 | PASS | `DfDeedle_COM_Tests.QfcColumnsAsync.cs`: both tests are `async Task`, pass `timeProvider: new FakeTimeProvider()` (never advanced), leave `columnAdder` at the production default, await with `NotThrowAsync`; no `GetAwaiter().GetResult()`, sleep or delay. No file under `UtilitiesCS/Extensions/` in the diff. Unchanged by cycle 1. |
| AC2 | PASS | `IDList.cs`: field initializer defaults the store to `ApplicationSettingsStore` over `ToDoModel.Properties.Settings.Default`; two new public constructors null-guard the store; `GetNextToDoID` assigns through the indexer and calls `_settingsStore.Save()` under the unchanged condition. Persistence-branch and null-store tests present and passing (7621 of 7621). `CreateListWithCachedMaxLength` remains deleted; the cycle-1 tests do not reintroduce it and every new list is store-injected. |
| AC3 | PASS | Both `MakeController` helpers return store-injected `IDList` instances; every `IDListGetNextToDoIDTests` list that calls `GetNextToDoID`, including the cycle-1 warm-cache test, is store-injected. Probe: 1 save before (`settings-probe-fail-before.2026-10-22T16-15.md`), 0 after (`idlist-probe-pass-after.2026-10-22T16-18.md`). |
| AC4 | PASS | `ac4-search-before.2026-10-22T16-12.md` and `ac4-search-after.2026-10-22T16-27.md`: after the fix, production `Settings.Default.Save()` remains only in the unreached `AppOlObjects` sites (spec non-goal); `_settingsStore.Save()` covers every named site; the test-code `defaults.Save()` is deleted; both `AddQfcColumnsAsync` calls pass the fake clock. Cycle 1 adds no settings-reaching test. |
| AC5 | PASS | Probe fail-before run: 4 of 4 failed with 1, 1, 1 and 6 cancelled saves; pass-after projections for each path. DfDeedle: `fail-before-exception.2026-10-22T16-15.md` with `WhyFailingRunImpossible`, quoted exception and the before-fix AC4 search. |
| AC6 | PASS | `remediation-tests.2026-10-22T17-12.md`: 7621 of 7621 passed under `scripts/vscode/TaskMaster.cli.runsettings` (`Workers` 0, `Scope` ClassLevel) with `/InIsolation`; `parallel-repeat-runs.2026-10-22T16-30.md` repeat runs with zero failures; `maxlengthofid-tests.2026-10-22T17-09.md` 3 of 3 with parallelization enabled. No added `[DoNotParallelize]`, `Workers`, retry, sleep, delay, timeout or temp-path token across the branch, including cycle 1. |
| AC7 | PASS | Cycle-1 final pass in CLAUDE.md order: CSharpier format (no change) and check exit 0, analyzer `/t:Rebuild` 0/0, warnings-as-errors `/t:Rebuild` 0/0 without a Nullable override, `Invoke-MSTestWithCoverage.ps1` 7621 passed. Changed lines lose no coverage; `ApplicationSettingsStore` and `AppAutoFileObjects.ScalarSettings` at 100%; seam members in `IDList`, `AppQuickFilerSettings`, `AppStagingFilenames` at 100%; `coverage-comparison.2026-10-22T17-12.md` records the post-processed Cobertura figures. No `[ExcludeFromCodeCoverage]` added. File sizes: all at or under 500 (`IDListGetNextToDoIDTests.cs` 287) except `DfDeedle_COM_Tests.cs` 848 to 809 and `AppAutoFileObjects.cs` 853 to 791, both smaller than at the merge base. The prior B-1 regression is resolved. |
| AC8 | PASS | `[DoNotParallelize]` removed from `AppQuickFilerSettingsTests` and `RibbonControllerTests`; `evidence/other/ribbon-static-state-check.2026-10-22T16-23.md` records the whole-class check with verdict REMOVE. Unchanged by cycle 1. |

## Summary

All eight acceptance criteria evaluate PASS against code and evidence at head `3df0782ab`. The prior blocking finding B-1 (loss of `IDList.MaxLengthOfID` coverage) is resolved by three store-injected tests: the getter is at 7/7 lines and 4/4 branches, and `IDList.cs` stands at 30.18% line and 25.81% branch against the merge-base 24.36% and 20.69%. No blocking finding remains; the review verdict is **PASS**. The TaskMaster CI checks are a PR-time gate and have not run because no pull request exists.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/spec.md`
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none

## Acceptance Criteria Check-off

All eight items were already `- [x]` in `spec.md` when this review began, and all eight evaluate PASS, so no box was changed and no criterion text was edited. No checked item is disputed.
