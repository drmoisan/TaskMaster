# IDList Probe - Pass After (P3-T4)

Timestamp: 2026-10-22T16-18
Command: vstest.console.exe ToDoModel.Test\bin\Debug\ToDoModel.Test.dll TaskTree.Test\bin\Debug\TaskTree.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~IDListGetNextToDoIDTests&TestCategory!=LiveOutlook|FullyQualifiedName~TaskTreeControllerMoveLogicTests&TestCategory!=LiveOutlook|FullyQualifiedName~TaskTreeControllerRouteDropTests&TestCategory!=LiveOutlook" /ResultsDirectory:SCRATCH\issue-1005-results-p3t4-idlist "/Logger:trx;LogFileName=issue-1005-p3t4-idlist.trx"
EXIT_CODE: 0
Output Summary: 37 total, 37 executed, 37 passed, 0 failed. MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam passes: the probe records zero saves on ToDoModel.Properties.Settings.Default and the injected Mock<ISettingsStore> records exactly one Save(). All seven new IDListGetNextToDoIDTests register tests pass.

## Preceding build (ANALYZE)

- Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- EXIT_CODE: 0
- Result: `0 Warning(s)`, `0 Error(s)` (ToDoModel.Test and TaskTree.Test compile under C# 7.3).

## Trx-derived results

| Test | Outcome |
| --- | --- |
| `IDListGetNextToDoIDTests.Constructor_Default_UsesApplicationSettingsStore` | Passed |
| `IDListGetNextToDoIDTests.Constructor_EnumerableWithNullStore_ThrowsArgumentNullException` | Passed |
| `IDListGetNextToDoIDTests.Constructor_NullStoreOnly_ThrowsArgumentNullException` | Passed |
| `IDListGetNextToDoIDTests.Constructor_StoreOnly_SavesThroughInjectedStore` | Passed |
| `IDListGetNextToDoIDTests.GetNextToDoID_EmptySeed_ThrowsArgumentException` | Passed |
| `IDListGetNextToDoIDTests.GetNextToDoID_FirstIdOnFreshList_AssignsLengthAndSavesOnce` | Passed |
| `IDListGetNextToDoIDTests.GetNextToDoID_IdAlreadyPresent_AdvancesPastTheCollision` | Passed |
| `IDListGetNextToDoIDTests.GetNextToDoID_IdNotLongerThanCachedLength_DoesNotSaveAgain` | Passed |
| `IDListGetNextToDoIDTests.GetNextToDoID_LengthBoundaryRollover_ProducesLongerId` | Passed |
| `IDListGetNextToDoIDTests.GetNextToDoID_LongerIdAfterFirst_SavesAgain` | Passed |
| `IDListGetNextToDoIDTests.GetNextToDoID_MultipleConsecutiveCollisions_AdvancesToFirstFreeValue` | Passed |
| `IDListGetNextToDoIDTests.GetNextToDoID_NoCollision_ReturnsNextBase36ValueAndAddsIt` | Passed |
| `IDListGetNextToDoIDTests.GetNextToDoID_NullSeed_ThrowsArgumentException` | Passed |
| `TaskTreeControllerMoveLogicTests` (17 tests, including `MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam`) | 17 Passed |
| `TaskTreeControllerRouteDropTests` (7 tests) | 7 Passed |

## Fail-before reference

`settings-probe-fail-before.2026-10-22T16-15.md` recorded `MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam` failing with 1 save on the test thread before the fix.
