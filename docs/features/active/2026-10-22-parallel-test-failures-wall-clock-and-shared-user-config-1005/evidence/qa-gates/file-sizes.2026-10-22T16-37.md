# Final File Sizes (P7-T6)

Timestamp: 2026-10-22T16-37
Command: @(Get-Content -LiteralPath <file>).Count for every created or modified .cs file
EXIT_CODE: 0
Output Summary: PASS. 20 created or modified .cs files; every count is at most 500 except the two pre-existing oversize files, which shrank: UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs 848 -> 809 and TaskMaster/AppGlobals/AppAutoFileObjects.cs 853 -> 791. Largest other file: TaskTree.Test/TaskTreeControllerMoveLogicTests.cs 477.

| File | P0-T8 | Final | Rule | Result |
| --- | --- | --- | --- | --- |
| `UtilitiesCS/HelperClasses/Settings/ISettingsStore.cs` | new | 25 | <= 500 | PASS |
| `UtilitiesCS/HelperClasses/Settings/ApplicationSettingsStore.cs` | new | 38 | <= 500 | PASS |
| `ToDoModel/Data Model/ID/IDList.cs` | 318 | 352 | <= 500 | PASS |
| `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` | 68 | 88 | <= 500 | PASS |
| `TaskMaster/AppGlobals/AppStagingFilenames.cs` | 139 | 171 | <= 500 | PASS |
| `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | 853 | 791 | below P0-T8 | PASS |
| `TaskMaster/AppGlobals/AppAutoFileObjects.ScalarSettings.cs` | new | 88 | <= 500 | PASS |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | 848 | 809 | below P0-T8 | PASS |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.QfcColumnsAsync.cs` | new | 70 | <= 500 | PASS |
| `UtilitiesCS.Test/HelperClasses/Settings/ApplicationSettingsStoreTests.cs` | new | 99 | <= 500 | PASS |
| `ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs` | 120 | 235 | <= 500 | PASS |
| `TaskTree.Test/TaskTreeControllerMoveLogicTests.cs` | 414 | 477 | <= 500 | PASS |
| `TaskTree.Test/TaskTreeControllerRouteDropTests.cs` | 220 | 224 | <= 500 | PASS |
| `TaskMaster.Test/AppGlobals/InMemorySettingsStore.cs` | new | 39 | <= 500 | PASS |
| `TaskMaster.Test/AppGlobals/SettingsSaveProbe.cs` | new | 75 | <= 500 | PASS |
| `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsTests.cs` | 89 | 128 | <= 500 | PASS |
| `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsRemainingPropertiesTests.cs` | 134 | 125 | <= 500 | PASS |
| `TaskMaster.Test/AppGlobals/AppStagingFilenamesTests.cs` | 146 | 268 | <= 500 | PASS |
| `TaskMaster.Test/AppGlobals/AppAutoFileObjectsCoverageExpansionTests.cs` | 274 | 335 | <= 500 | PASS |
| `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` | 452 | 459 | <= 500 | PASS |

`TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs` (73 at P0-T8) is unchanged (Decision D-E).
