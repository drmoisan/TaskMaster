# Baseline File Sizes (physical lines)

Timestamp: 2026-10-22T16-10
Command: @(Get-Content -LiteralPath <file>).Count for each existing Write Set .cs file
EXIT_CODE: 0
Output Summary: 14 existing Write Set .cs files counted. Two are above 500: UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs 848 and TaskMaster/AppGlobals/AppAutoFileObjects.cs 853. Largest others: RibbonControllerTests.cs 452, TaskTreeControllerMoveLogicTests.cs 414.

| File | Physical lines |
| --- | --- |
| `ToDoModel/Data Model/ID/IDList.cs` | 318 |
| `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` | 68 |
| `TaskMaster/AppGlobals/AppStagingFilenames.cs` | 139 |
| `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | 853 |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | 848 |
| `ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs` | 120 |
| `TaskTree.Test/TaskTreeControllerMoveLogicTests.cs` | 414 |
| `TaskTree.Test/TaskTreeControllerRouteDropTests.cs` | 220 |
| `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsTests.cs` | 89 |
| `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsRemainingPropertiesTests.cs` | 134 |
| `TaskMaster.Test/AppGlobals/AppStagingFilenamesTests.cs` | 146 |
| `TaskMaster.Test/AppGlobals/AppAutoFileObjectsCoverageExpansionTests.cs` | 274 |
| `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` | 452 |
| `TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs` | 73 |

New Write Set files (not yet present): `ISettingsStore.cs`, `ApplicationSettingsStore.cs`, `AppAutoFileObjects.ScalarSettings.cs`, `DfDeedle_COM_Tests.QfcColumnsAsync.cs`, `ApplicationSettingsStoreTests.cs`, `InMemorySettingsStore.cs`, `SettingsSaveProbe.cs`.
