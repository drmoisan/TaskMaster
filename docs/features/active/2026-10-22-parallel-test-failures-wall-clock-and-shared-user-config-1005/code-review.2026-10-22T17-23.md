# Code Review: Parallel test failures from wall-clock deadlines and shared user.config (Issue #1005)

- Branch: `bug/parallel-test-failures-wall-clock-and-shared-user-config-1005`
- Head: `3df0782ab`; merge base `346651242a2bd6ed25d703a4b1ecb5a055c6ae72` (`main`)
- Review timestamp: 2026-10-22T17-23 (re-audit R4 after remediation cycle 1; prior review 2026-10-22T17-10 at `e399de597`)
- Reviewer: feature-review agent
- Scope: the full branch diff `346651242..3df0782ab` (91 files: 7 production C#, 13 test C#, 4 project files, feature documents and evidence, 2 promotion records, agent-memory files). Cycle-1 delta `e399de597..3df0782ab`: 19 files, one test C# file plus documents and evidence; no production change.

## Executive Summary

The design is unchanged from the prior review and remains narrow: a two-member `ISettingsStore` interface and a sealed `ApplicationSettingsStore` adapter give each settings consumer a per-instance persistence seam whose production default wraps the consumer assembly's own `Properties.Settings.Default`. No static mutable seam, process-wide `SettingsProvider`, `user.config` redirection, temporary file, serialization attribute, sleep or retry is introduced. The DfDeedle fix is test-only and uses the existing `timeProvider` parameter with a never-advanced `FakeTimeProvider`.

The prior blocking finding B-1 is resolved. Cycle 1 adds three store-injected `MaxLengthOfID` tests to `IDListGetNextToDoIDTests.cs` (287 lines at head). The getter is fully covered again (7/7 lines, 4/4 branches in `artifacts/csharp/coverage.xml`), `IDList.cs` rose to 30.18% line and 25.81% branch against the merge-base 24.36% and 20.69%, and the new coverage comparison reports both rates and names the getter.

Verdict: **PASS (0 blocking, 0 major, 7 minor or informational)**.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Resolved | `ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs`; `ToDoModel/Data Model/ID/IDList.cs` | `IDList.MaxLengthOfID` getter | B-1 (prior review): the getter had lost all test coverage. Cycle 1 adds `MaxLengthOfID_ColdCacheOnMixedLengthList_ReturnsLongestIdLength`, `MaxLengthOfID_ColdCacheOnSecondDistinctList_ReturnsItsOwnLongestIdLength` and `MaxLengthOfID_WarmCacheAfterGetNextToDoID_ReturnsCachedLengthWithoutSaving`. | None. | Coverage restored to the merge-base level on every getter line and branch; disclosure corrected. | `artifacts/csharp/coverage.xml` method `get_MaxLengthOfID` `line-rate="1" branch-rate="1"`, lines 107 to 113 `hits="1"`, branch lines `100% (2/2)`; `coverage-comparison.2026-10-22T17-12.md`. |
| Minor | `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | file-level coverage | N-1: per-file line rate 45.66% to 40.81% because covered scalar-property lines moved into `AppAutoFileObjects.ScalarSettings.cs`; branch rate unchanged at 37.10%. | None for this item. | Movement between partial files of one class is not a coverage loss. | Cobertura class rows for both files. |
| Informational | `TaskTree.Test/TaskTreeControllerMoveLogicTests.cs`; `TaskMaster.Test/AppGlobals/SettingsSaveProbe.cs` | `CountToDoModelSettingsSavesOnCurrentThread`; `CountSavesOnCurrentThread` | O-4: the thread-attributed save probe is implemented twice, once per test assembly. | Accept. | The test assemblies share no helper project. | Diff of both files. |
| Informational | `SettingsSaveProbe.cs`; `TaskTreeControllerMoveLogicTests.cs` | `lock (settings)` around subscribe and unsubscribe | O-5: the probe locks on the shared `Settings.Default` instance. | Accept; the lock covers only event add and remove. | Cannot serialize the code under test. | Code comments in both probes. |
| Informational | `IDListGetNextToDoIDTests.cs`, `AppQuickFilerSettingsTests.cs`, `AppStagingFilenamesTests.cs`, `AppAutoFileObjectsCoverageExpansionTests.cs` | `Constructor_*_UsesApplicationSettingsStore` | O-6: default-constructor tests read the private field `_settingsStore` by reflection. | Accept. | No public accessor should be added only for tests. | Diff of the four test files. |
| Informational | `ToDoModel/Data Model/ID/IDList.cs` | field initializer for `_settingsStore` | O-7: store-taking constructors overwrite a default adapter that the field initializer allocated. | Accept. | Guarantees every existing constructor keeps the production default (spec D3). | `IDList.cs` field declarations. |
| Informational | `ToDoModel/Data Model/ID/IDList.cs` | `MaxLengthOfID` getter | O-8: on an empty list with a zero cache the getter throws `InvalidOperationException` (pre-existing). The cycle-1 tests correctly use non-empty lists. | Out of scope. | Pre-existing behaviour, unchanged by this branch. | Cycle-1 diff. |
| Informational | `ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs` | `MaxLengthOfID_WarmCacheAfterGetNextToDoID_ReturnsCachedLengthWithoutSaving` | O-9: the asserted value 4 is also what a recomputation over `{ "02", "0100" }` would return, so the value assertion alone does not distinguish the cached path from the recompute path; the cached (false) branch is proven only by the Cobertura branch count. The `Save` verification (`Times.Once()`) does pin that the read does not persist. Similarly, `..._ColdCacheOnSecondDistinctList_...` re-asserts the first list's value, overlapping the first test. | Optional: a future change could seed a cache value that differs from the recomputed maximum, but `IDList` exposes no such seam and adding one is not justified for this item. | The test meets the remediation's stated purpose (both getter branches executed and the non-persisting read verified). | Cycle-1 diff; head Cobertura branch line 108 `100% (2/2)`. |

## Design and Correctness Review

### Settings-store seam

- `ISettingsStore` exposes only the indexer and `Save()`; `ApplicationSettingsStore` null-guards its argument and forwards both members.
- Behaviour preservation: generated settings properties read and write through `this[name]`, so the store indexer plus `Save()` on the default adapter is equivalent to the generated setters plus `Save()` that it replaces.
- `nameof(Properties.Settings.X)` keys keep setting names compile-checked.
- `AppStagingFilenames` keeps the lazy `InitProp` caching and the non-persisting `EmailInfoStagingFile` setter.

### Tests

- The fail-before probe counts `SettingsSaving` only on the test thread and cancels it, so the fail-before run wrote nothing.
- `InMemorySettingsStore` throws `KeyNotFoundException` for an unseeded key.
- The DfDeedle happy-path test is an awaited `async Task` test with `NotThrowAsync`.
- Cycle-1 tests: each builds its own `Mock<ISettingsStore>` and list, uses non-empty lists, follows Arrange/Act/Assert with comments, uses FluentAssertions with `because` text, and complies with C# 7.3. The second cold-cache test reads the getter on two distinct lists within one test, so both arms of the compiler-generated cached-lambda branch on line 110 execute regardless of test order.

### Scope fences

No change to `IIDList`, `TreeOfToDoItems`, `TaskTreeController`, `AppToDoObjects`, `ApplicationGlobals`, runsettings, `coverage.config`, CI workflows or `UtilitiesCS/Extensions/`. Cycle 1 touched no production, project or runsettings file. Out-of-scope `Settings.Default` in-memory mutation (issue #1006) is untouched.

## Best-Practice Checklist

| Area | Verdict | Note |
|---|---|---|
| Naming and documentation | PASS | XML docs on every new public and internal member; comments explain why. |
| Error handling | PASS | `ArgumentNullException` on every new constructor; tested. |
| Async correctness | PASS | No sync-over-async in the converted DfDeedle tests. |
| Thread safety | PASS | Per-instance stores; no shared mutable static added. |
| File size | PASS | Largest changed file other than the two shrinking pre-existing files is 477 lines; `IDListGetNextToDoIDTests.cs` is 287. |
| Language level | PASS | C# 7.3 constraints respected in `ToDoModel.Test` and `TaskTree.Test`. |
| Test coverage of changed behaviour | PASS | B-1 resolved; new code 100%; no modified file regressed. |

## Recommendation

Proceed to PR authoring. The TaskMaster CI checks (actionlint, build-analyzers, build-nullable, format-check, hygiene, mstest-coverage, pester) are a PR-time gate and have not yet run because no pull request exists.
