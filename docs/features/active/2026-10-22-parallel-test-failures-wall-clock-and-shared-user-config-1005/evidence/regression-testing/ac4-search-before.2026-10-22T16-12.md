# AC4 Search - Before Fix (Issue #1005)

Timestamp: 2026-10-22T16-12
Command: git grep -c -E 'Settings\.Default\.Save\(\)|_defaults\.Save\(\)|defaults\.Save\(\)' -- '*.cs'
EXIT_CODE: 0
Output Summary: Q1 finds 27 settings saves in 7 files matching the planning read exactly (IDList.cs 1, AppQuickFilerSettings.cs 6, AppStagingFilenames.cs 9, AppAutoFileObjects.cs 6, AppOlObjects.JunkFolders.cs 3, AppOlObjects.cs 1, AppAutoFileObjectsCoverageExpansionTests.cs 1). Q2 finds no `_settingsStore.Save()` (git grep exit 1). Q3 finds 29 test constructions of the four consumers, all default-constructed, 19 of them in tests that reach a spec member save. Q4 finds 2 `AddQfcColumnsAsync(` calls without `timeProvider:` in DfDeedle_COM_Tests.cs plus 1 seamed helper call in DfDeedleQfcColumnTimeoutTests.cs. Q5 lists 18 reaching tests plus the TestCleanup direct save.

All searches run against the unmodified tracked tree (no code edit yet in this item).

## Q1 - production and test settings saves

Command: `git grep -c -E 'Settings\.Default\.Save\(\)|_defaults\.Save\(\)|defaults\.Save\(\)' -- '*.cs'` (EXIT_CODE 0)

| File | Count |
| --- | --- |
| `ToDoModel/Data Model/ID/IDList.cs` | 1 |
| `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` | 6 |
| `TaskMaster/AppGlobals/AppStagingFilenames.cs` | 9 |
| `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | 6 |
| `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` | 3 |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 1 |
| `TaskMaster.Test/AppGlobals/AppAutoFileObjectsCoverageExpansionTests.cs` | 1 |

Matches the seven per-file counts in the plan's AC4 Search Definition.

## Q2 - store saves

Command: `git grep -c -F '_settingsStore.Save()' -- '*.cs'` (EXIT_CODE 1, no matches). Before the fix no consumer saves through a store.

## Q3 - test constructions of the consumers

Command: `git grep -n -E 'new (IDList|AppQuickFilerSettings|AppStagingFilenames|AppAutoFileObjects)\(' -- '*.Test/*.cs' '*.Test/**/*.cs'` (EXIT_CODE 0)

| File | Matches | Enclosing tests | Classification (before) |
| --- | --- | --- | --- |
| `TaskMaster.Test/AppGlobals/AppAutoFileObjectsCoverageExpansionTests.cs` | 1 (`CreateSutWithSpecialFolders`) | all 8 tests of the class; `ScalarSettingsProperties_WhenRoundTripped_UpdateDefaultsAndReturnValues` calls the six scalar setters | default-constructed; one test reaches spec members (not compliant) |
| `TaskMaster.Test/AppGlobals/AppAutoFileObjectsFolderPredictorTests.cs` | 5 | folder-predictor load tests | default-constructed, no spec member called (compliant) |
| `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsRemainingPropertiesTests.cs` | 6 | 4 setter tests reach `MoveEntireConversation`, `SaveAttachments`, `SavePictures`, `SaveEmailCopy` setters; 2 getter tests | default-constructed; setter tests not compliant |
| `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsTests.cs` | 4 | `HighConfidenceModeEnabled_WhenSetTrue_ReadsBackTrue`, `HighConfidenceThreshold_WhenSet_ReadsBackSameValue` reach setters; 2 getter tests | default-constructed; setter tests not compliant |
| `TaskMaster.Test/AppGlobals/AppStagingFilenamesTests.cs` | 6 | `ConditionalReminders_Setter_RoundTripsThroughSettingsDefault`, `CommonWords_GetterAfterSet_ReturnsCachedValueWithoutReReadingSettings` reach setters; `EmailInfoStagingFile_Setter_DoesNotWriteSettingsDefault` (non-saving) and 3 getter tests | default-constructed; two setter tests not compliant |
| `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` | 1 (`CreateController`) | `ToggleHighConfidenceMode_FlipsStoredValue`, `SetHighConfidenceModeForLaunch_True_EnablesMode`, `StandardLaunchAfterHighConfidenceLaunch_DoesNotEnableMode`, `SetHighConfidenceThresholdText_WithValidPercentage_PersistsProbability` reach setters | default-constructed; four tests not compliant |
| `TaskTree.Test/TaskTreeControllerMoveLogicTests.cs` | 1 (`MakeController`) | six tests reach `GetNextToDoID` (Q5) | default-constructed; not compliant |
| `TaskTree.Test/TaskTreeControllerRouteDropTests.cs` | 1 (`MakeController`) | three tests reach `GetNextToDoID` (Q5) | default-constructed; not compliant |
| `ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs` | 3 | null and empty seed tests throw before any save; `CreateListWithCachedMaxLength` primes the cache so four tests avoid the save by workaround | default-constructed with a workaround that leaves the save path reachable (not compliant per AC2) |

## Q4 - column-add calls

Command: `git grep -n -F 'AddQfcColumnsAsync(' -- '*.Test/*.cs' '*.Test/**/*.cs'` (EXIT_CODE 0)

| File | Call | Passes `timeProvider:` |
| --- | --- | --- |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | `AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing` | no (neither seam) |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | `AddQfcColumnsAsync_PreCancelledToken_CompletesGracefully` | no (neither seam) |
| `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` | helper `StartColumnAdd` | passes `columnAdder` and `timeProvider` positionally (`probe.Adder`, `barrier`) |

Before: two calls without `timeProvider:` in `DfDeedle_COM_Tests.cs`.

## Q5 - reach trace

| Production save reached | Reaching test (enclosing class) |
| --- | --- |
| `IDList.GetNextToDoID` | `TaskTreeControllerMoveLogicTests`: `MoveObjectsToSibling_RootTarget_RemovesFromRootsAndReseeds`, `MoveObjectsToSibling_RootNotInRoots_FiresMessageSeam`, `MoveObjectsToSibling_ChildTarget_InsertsIntoParentChildren`, `MoveObjectsToChildren_RootSource_ReparentsAndRemovesFromRoots`, `MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam`, `MoveObjectsToChildren_NonRootSource_ReparentsUnderTarget` |
| `IDList.GetNextToDoID` | `TaskTreeControllerRouteDropTests`: `RouteDrop_Item_RoutesToChildren_ReturnsTrue`, `RouteDrop_AboveItem_InsertsSiblingBeforeTarget_ReturnsTrue`, `RouteDrop_BelowItem_InsertsSiblingAfterTarget_ReturnsTrue` |
| `AppQuickFilerSettings.MoveEntireConversation` / `SaveAttachments` / `SavePictures` / `SaveEmailCopy` setters | `AppQuickFilerSettingsRemainingPropertiesTests`: the four `*_Setter_RoundTripsThroughSettingsDefault` tests |
| `AppQuickFilerSettings.HighConfidenceModeEnabled` setter | `AppQuickFilerSettingsTests.HighConfidenceModeEnabled_WhenSetTrue_ReadsBackTrue`; `RibbonControllerTests`: `ToggleHighConfidenceMode_FlipsStoredValue`, `SetHighConfidenceModeForLaunch_True_EnablesMode`, `StandardLaunchAfterHighConfidenceLaunch_DoesNotEnableMode` |
| `AppQuickFilerSettings.HighConfidenceThreshold` setter | `AppQuickFilerSettingsTests.HighConfidenceThreshold_WhenSet_ReadsBackSameValue`; `RibbonControllerTests.SetHighConfidenceThresholdText_WithValidPercentage_PersistsProbability` |
| `AppStagingFilenames.ConditionalReminders` setter | `AppStagingFilenamesTests.ConditionalReminders_Setter_RoundTripsThroughSettingsDefault` |
| `AppStagingFilenames.CommonWords` setter | `AppStagingFilenamesTests.CommonWords_GetterAfterSet_ReturnsCachedValueWithoutReReadingSettings` |
| `AppAutoFileObjects` six scalar setters | `AppAutoFileObjectsCoverageExpansionTests.ScalarSettingsProperties_WhenRoundTripped_UpdateDefaultsAndReturnValues` |
| Direct test-code save (`defaults.Save()`) | `AppAutoFileObjectsCoverageExpansionTests.TestCleanup` (runs after every test of the class) |

The reaching set equals the research reach map (Test-to-site reach map) plus the `TestCleanup` direct save.
