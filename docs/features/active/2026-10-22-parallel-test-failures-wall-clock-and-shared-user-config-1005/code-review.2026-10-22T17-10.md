# Code Review: Parallel test failures from wall-clock deadlines and shared user.config (Issue #1005)

- Branch: `bug/parallel-test-failures-wall-clock-and-shared-user-config-1005`
- Head: `e399de597`; merge base `346651242a2bd6ed25d703a4b1ecb5a055c6ae72` (`main`)
- Review timestamp: 2026-10-22T17-10
- Reviewer: feature-review agent
- Scope: the full branch diff `346651242..e399de597` (73 files: 7 production C#, 13 test C#, 4 project files, feature documents and evidence, 2 promotion records, 7 agent-memory files)

## Executive Summary

The design is narrow and consistent with the maintainer's objective that every test can run in parallel. A two-member `ISettingsStore` interface and a sealed `ApplicationSettingsStore` adapter give each settings consumer a per-instance persistence seam. Production defaults wrap the consumer assembly's own `Properties.Settings.Default`, so the indexer write plus `Save()` is behaviourally identical to the generated property setters it replaces. No static mutable seam, process-wide `SettingsProvider`, `user.config` redirection, temporary file, serialization attribute, sleep or retry is introduced. The DfDeedle fix is test-only and uses the existing `timeProvider` parameter with a never-advanced `FakeTimeProvider`.

One blocking finding: removing the cache-priming helper in `IDListGetNextToDoIDTests` (required by AC2) also removed the only test that executed `IDList.MaxLengthOfID`, so that public member lost all coverage (7 lines, 4 branches) and the committed coverage comparison does not report it. The remaining observations are minor and non-blocking.

Verdict: **REMEDIATION_REQUIRED (1 blocking, 0 major, 6 minor or informational)**.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocking | `ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs`; `ToDoModel/Data Model/ID/IDList.cs` | `IDList.MaxLengthOfID` getter (lines 104 to 114); deleted helper `CreateListWithCachedMaxLength` | B-1: the deleted helper's `_ = list.MaxLengthOfID;` was the only test execution of the getter. Baseline Cobertura: 7/7 lines, 4/4 branches covered; head: 0/7 and 0/4. `IDList.cs` branch rate fell from 20.69% to 19.35%. `coverage-comparison.2026-10-22T16-36.md` reports only line rates for the file and attributes the uncovered residue to the unchanged constructors. | Add a test (or two) in `IDListGetNextToDoIDTests.cs` that asserts `MaxLengthOfID` on a non-empty store-injected list: a cold cache returns the longest ID length, and after `GetNextToDoID` grows the cache the getter returns the cached value without recomputing. Update the coverage comparison to report `IDList.cs` branch figures and the getter. | Coverage is a supporting gate; a modified file below the floor must not regress, and a public `IIDList` member should not become untested as a side effect of a test refactor. | Baseline document `<method name="get_MaxLengthOfID">` lines 73 to 79 `hits="1"`; `artifacts/csharp/coverage.xml` lines 107 to 113 `hits="0"`; Grep of `*.Test` for `MaxLengthOfID` finds no reader. |
| Minor | `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | file-level coverage | N-1: per-file line rate 45.66% to 40.81% because 33 covered scalar-property lines moved into `AppAutoFileObjects.ScalarSettings.cs`; branch rate unchanged at 37.10%. | None for this item. The class across both files rises to 46.06% (187/406). | Movement between partial files of one class is not a coverage loss. | Cobertura class rows for both files at baseline and head. |
| Informational | `TaskTree.Test/TaskTreeControllerMoveLogicTests.cs`; `TaskMaster.Test/AppGlobals/SettingsSaveProbe.cs` | `CountToDoModelSettingsSavesOnCurrentThread`; `CountSavesOnCurrentThread` | O-4: the thread-attributed save probe is implemented twice, once per test assembly. | Accept; consider a shared test-utility project only if a third copy is needed. | The test assemblies share no helper project, and the TaskTree copy must resolve the internal ToDoModel settings type by reflection. | Diff of both files. |
| Informational | `SettingsSaveProbe.cs`; `TaskTreeControllerMoveLogicTests.cs` | `lock (settings)` around subscribe and unsubscribe | O-5: the probe locks on the shared `Settings.Default` instance, which a synchronized `SettingsBase` also uses as its own monitor. | Accept. The lock covers only the event add and remove statements, not the observed action, so it cannot serialize the code under test or deadlock with a production save. | The `SettingsSaving` accessors are not atomic; the lock prevents two concurrent probes from losing a subscription. | Code comments in both probes. |
| Informational | `IDListGetNextToDoIDTests.cs`, `AppQuickFilerSettingsTests.cs`, `AppStagingFilenamesTests.cs`, `AppAutoFileObjectsCoverageExpansionTests.cs` | `Constructor_*_UsesApplicationSettingsStore` | O-6: the default-constructor tests read the private field `_settingsStore` by reflection, so a field rename breaks them at run time rather than compile time. | Accept; the field name is stable across all four consumers and the tests fail loudly (null `FieldInfo`). | No public accessor should be added only for tests. | Diff of the four test files. |
| Informational | `ToDoModel/Data Model/ID/IDList.cs` | field initializer `_settingsStore = new ApplicationSettingsStore(Properties.Settings.Default)` | O-7: the store-taking constructors overwrite a default adapter that the field initializer has already allocated, which also touches the `Settings.Default` singleton. | Accept. The allocation is trivial, reading `Settings.Default` performs no I/O, and the initializer guarantees every existing constructor (including the deserializing ones) keeps the production default. | Matches spec D3. | `IDList.cs` lines 94 to 99. |
| Informational | `ToDoModel/Data Model/ID/IDList.cs` | `MaxLengthOfID` getter | O-8: on an empty list with a zero cache the getter calls `Max()` on an empty sequence and throws `InvalidOperationException`. Pre-existing, unchanged by this branch, and no production code reads the property. | Out of scope. When adding the B-1 test, use a non-empty list. | Recorded so the B-1 test is not written against the empty case. | `IDList.cs` line 110; Grep of non-test code for `MaxLengthOfID`. |

## Design and Correctness Review

### Settings-store seam

- `ISettingsStore` exposes only the indexer and `Save()`, the minimum that the four consumers use. `ApplicationSettingsStore` null-guards its argument and forwards both members; it is `sealed` and carries no state beyond the wrapped instance.
- Behaviour preservation: the generated property `MaxLengthOfID` is `((int)(this["MaxLengthOfID"]))` and its setter `this["MaxLengthOfID"] = value` (`ToDoModel/Properties/Settings.Designer.cs` lines 137 to 142). The new code assigns a boxed `int` through the same indexer and calls the same `Save()`, so production persistence is unchanged. `ToDoItem` still reads `Properties.Settings.Default.MaxLengthOfID` directly, which remains consistent because the production default writes to that same instance.
- `nameof(Properties.Settings.X)` keys keep setting names compile-checked against the generated class.
- `AppStagingFilenames` converts all ten settings-backed properties, preserving the lazy `InitProp` caching and the non-persisting `EmailInfoStagingFile` setter; `Read` is a small private helper.
- `AppAutoFileObjects(IApplicationGlobals)` previously only assigned `_parent`; it now chains to the internal overload, which assigns the same field plus the store. No other initialization was skipped.

### Tests

- The fail-before probe is sound: it counts `SettingsSaving` only on the test thread and sets `e.Cancel = true`, so the fail-before run itself writes nothing to `user.config`, and concurrent classes are neither counted nor cancelled.
- `InMemorySettingsStore` throws `KeyNotFoundException` for an unseeded key, so a missing arrangement fails explicitly instead of returning a default.
- The DfDeedle happy-path test is now an awaited `async Task` test with `NotThrowAsync`, verifies the production adder added `SentOn`, and leaves `columnAdder` at its default so the production branch stays exercised.
- `TaskTreeControllerMoveLogicTests.MakeController` accepts an optional `Mock<ISettingsStore>` so the probe test can verify exactly one store save while the other move tests receive a fresh mock.

### Scope fences

No change to `IIDList`, `TreeOfToDoItems`, `TaskTreeController`, `AppToDoObjects`, `ApplicationGlobals`, runsettings, `coverage.config`, CI workflows or `UtilitiesCS/Extensions/`. Out-of-scope `Settings.Default` in-memory mutation (issue #1006) is untouched.

## Best-Practice Checklist

| Area | Verdict | Note |
|---|---|---|
| Naming and documentation | PASS | XML docs on every new public and internal member; comments explain why (field initializer, lock scope, reflection limits). |
| Error handling | PASS | `ArgumentNullException` with correct `ParamName` on every new constructor; tested. |
| Async correctness | PASS | No sync-over-async remains in the converted DfDeedle tests. |
| Thread safety | PASS | Per-instance stores; no shared mutable static added. |
| File size | PASS | Largest changed file other than the two shrinking pre-existing files is 477 lines. |
| Language level | PASS | C# 7.3 constraints respected in `ToDoModel.Test` and `TaskTree.Test`. |
| Test coverage of changed behaviour | FAIL | B-1. |

## Recommendation

Address B-1 in a short remediation cycle: one or two `MaxLengthOfID` tests in `IDListGetNextToDoIDTests.cs` (the file is at 235 lines), a full C# toolchain pass in CLAUDE.md order, and a corrected coverage comparison that reports `IDList.cs` line and branch figures. No production change is needed.
