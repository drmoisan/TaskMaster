# AC4 Search - After Fix (Issue #1005)

Timestamp: 2026-10-22T16-27
Command: git grep --untracked -c -E 'Settings\.Default\.Save\(\)|_defaults\.Save\(\)|defaults\.Save\(\)' -- '*.cs'
EXIT_CODE: 0
Output Summary: Every "Required after" outcome holds. Q1 finds only the two excluded unreached sites (AppOlObjects.JunkFolders.cs 3, AppOlObjects.cs 1). Q2 finds IDList.cs 1, AppQuickFilerSettings.cs 6, AppStagingFilenames.cs 9, AppAutoFileObjects.ScalarSettings.cs 6. Q3 classifies all 46 test constructions as store-injected or default-constructed without spec-member or GetNextToDoID use. Q4 finds 0 calls in DfDeedle_COM_Tests.cs and 2 in DfDeedle_COM_Tests.QfcColumnsAsync.cs, each passing `timeProvider: new FakeTimeProvider()`; the DfDeedleQfcColumnTimeoutTests call is unchanged. Q5: no test reaches an ApplicationSettingsBase.Save(); every reaching test from the before search is converted.

Before reference: `evidence/regression-testing/ac4-search-before.2026-10-22T16-12.md`.

## Q1 - production and test settings saves

Command: `git grep --untracked -c -E 'Settings\.Default\.Save\(\)|_defaults\.Save\(\)|defaults\.Save\(\)' -- '*.cs'` (EXIT_CODE 0)

| File | Before | After |
| --- | --- | --- |
| `ToDoModel/Data Model/ID/IDList.cs` | 1 | 0 |
| `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` | 6 | 0 |
| `TaskMaster/AppGlobals/AppStagingFilenames.cs` | 9 | 0 |
| `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | 6 | 0 |
| `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` | 3 | 3 (excluded: no test reach) |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 1 | 1 (excluded: no test reach) |
| `TaskMaster.Test/AppGlobals/AppAutoFileObjectsCoverageExpansionTests.cs` | 1 | 0 |

Required after: only `AppOlObjects.JunkFolders.cs` 3 and `AppOlObjects.cs` 1. Holds.

## Q2 - store saves

Command: `git grep --untracked -c -F '_settingsStore.Save()' -- '*.cs'` (EXIT_CODE 0)

| File | Count | Required |
| --- | --- | --- |
| `ToDoModel/Data Model/ID/IDList.cs` | 1 | 1 |
| `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` | 6 | 6 |
| `TaskMaster/AppGlobals/AppStagingFilenames.cs` | 9 | 9 |
| `TaskMaster/AppGlobals/AppAutoFileObjects.ScalarSettings.cs` | 6 | 6 |

Holds.

## Q3 - test constructions of the consumers

Command: `git grep --untracked -n -E 'new (IDList|AppQuickFilerSettings|AppStagingFilenames|AppAutoFileObjects)\(' -- '*.Test/*.cs' '*.Test/**/*.cs'` (EXIT_CODE 0)

| File | Matches | Classification |
| --- | --- | --- |
| `AppAutoFileObjectsCoverageExpansionTests.cs` | 3 | `CreateSutWithSpecialFolders` is store-injected (`store ?? CreateScalarStore()`); `Constructor_ParentOnly_UsesApplicationSettingsStore` is default-constructed and only reads the private store field by reflection (no spec member); `Constructor_NullSettingsStore_ThrowsArgumentNullException` passes a null store (throws) |
| `AppAutoFileObjectsFolderPredictorTests.cs` | 5 | default-constructed; no scalar spec member called |
| `AppQuickFilerSettingsRemainingPropertiesTests.cs` | 6 | store-injected |
| `AppQuickFilerSettingsTests.cs` | 6 | 4 store-injected; `Constructor_Parameterless_UsesApplicationSettingsStore` default-constructed, reads the private field only; `Constructor_NullSettingsStore_ThrowsArgumentNullException` null store (throws) |
| `AppStagingFilenamesTests.cs` | 10 | 8 store-injected; `Constructor_Parameterless_UsesApplicationSettingsStore` default-constructed, reads the private field only; `Constructor_NullSettingsStore_ThrowsArgumentNullException` null store (throws) |
| `RibbonControllerTests.cs` | 1 | store-injected (`new AppQuickFilerSettings(store)` in `CreateController(InMemorySettingsStore)`) |
| `TaskTreeControllerMoveLogicTests.cs` | 1 | store-injected (`MakeController`) |
| `TaskTreeControllerRouteDropTests.cs` | 1 | store-injected (`MakeController`) |
| `IDListGetNextToDoIDTests.cs` | 13 | 10 store-injected; 2 null-store constructor tests (throw); `Constructor_Default_UsesApplicationSettingsStore` default-constructed, reads the private field only and does not call `GetNextToDoID` |

Required after: every match is store-injected, or default-constructed with the test body calling none of the fifteen spec members and not calling `GetNextToDoID`. Holds (the null-store constructions throw at construction and reach nothing).

## Q4 - column-add calls

Command: `git grep --untracked -n -A6 -F 'AddQfcColumnsAsync(' -- '*.Test/*.cs' '*.Test/**/*.cs'` (EXIT_CODE 0), plus `git grep --untracked -c -F 'timeProvider: new FakeTimeProvider()' -- 'UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.QfcColumnsAsync.cs'` (2)

| File | Calls | Passes `timeProvider:` |
| --- | --- | --- |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | 0 | n/a |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.QfcColumnsAsync.cs` | 2 | yes, both `timeProvider: new FakeTimeProvider()` |
| `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` | 1 (`StartColumnAdd`) | unchanged; passes `probe.Adder` and `barrier` positionally |

Note: the happy-path call was first written with a local `FakeTimeProvider` variable and was aligned to the literal `timeProvider: new FakeTimeProvider()` form before this search was recorded; the behavior is identical (a fake clock that is never advanced). The P6-T4 runs and the P7 final pass re-execute the test in this form.

Holds.

## Q5 - reach trace

| Before: reaching test | After: how it is converted |
| --- | --- |
| `TaskTreeControllerMoveLogicTests` six move tests | `MakeController` returns `new IDList(new List<string>(), store.Object)` over a per-call `Mock<ISettingsStore>`; `MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam` verifies one mock `Save()` and zero probe saves |
| `TaskTreeControllerRouteDropTests` three drop tests | `MakeController` returns `new IDList(new List<string>(), settingsStore.Object)` |
| `AppQuickFilerSettingsRemainingPropertiesTests` four setter tests | `new AppQuickFilerSettings(store)` over a per-test `InMemorySettingsStore` |
| `AppQuickFilerSettingsTests` two setter tests | `new AppQuickFilerSettings(store)` over a per-test `InMemorySettingsStore` |
| `RibbonControllerTests` four setter-reaching tests | `CreateController(InMemorySettingsStore)` injects `new AppQuickFilerSettings(store)` |
| `AppStagingFilenamesTests` two setter tests | `new AppStagingFilenames(store)` over a per-test `InMemorySettingsStore` |
| `AppAutoFileObjectsCoverageExpansionTests.ScalarSettingsProperties_WhenRoundTripped_UpdateDefaultsAndReturnValues` | `CreateSutWithSpecialFolders(..., store: store)` with a seeded `InMemorySettingsStore`; `SaveCount` 6 and zero probe saves |
| `AppAutoFileObjectsCoverageExpansionTests.TestCleanup` direct `defaults.Save()` | deleted with `TestInitialize`, `TestCleanup`, and the six snapshot fields |
| `IDListGetNextToDoIDTests` (cache-priming workaround) | `CreateListWithCachedMaxLength` deleted; every list built with `new IDList(ids, store.Object)` |

Required after: no test reaches an `ApplicationSettingsBase.Save()`. Holds: the remaining production `Settings.Default.Save()` sites (`AppOlObjects.JunkFolders.cs`, `AppOlObjects.cs`) have no test reach (research reach map), every Q3 match is store-injected or does not call a saving member, and the store-injected tests save only to `InMemorySettingsStore` or `Mock<ISettingsStore>`.
