# Baseline Test Disposition (symbols changed by the plan)

Timestamp: 2026-10-22T16-11
Command: Grep (ripgrep) over *.Test/**/*.cs for `\bIDList\b|GetNextToDoID|AppQuickFilerSettings|AppStagingFilenames|AppAutoFileObjects|CreateSutWithSpecialFolders|AddQfcColumnsAsync`, and separately for `CreateController`
EXIT_CODE: 0
Output Summary: 85 files match the combined pattern (426 occurrences). Nine classes construct or call the concrete changed members and are UPDATED by the plan; three are UNCHANGED by design (AppAutoFileObjectsFolderPredictorTests, RibbonControllerTests.Engines.cs, DfDeedleQfcColumnTimeoutTests). All other matches reference only interfaces (`IAppAutoFileObjects`, `IAppQuickFilerSettings`, `IAppStagingFilenames`, `IIDList`), comments, or an unrelated local helper that happens to be named `CreateController`, and are UNCHANGED. No match requires a change outside the Write Set.

## Updated by this plan

| Test class (file) | Matching symbol(s) | Disposition | Reason |
| --- | --- | --- | --- |
| `IDListGetNextToDoIDTests` (`ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs`) | `IDList`, `GetNextToDoID` | updated (P3-T2) | constructs `IDList` and reaches the `Save()` branch through the cache-priming workaround; converted to an injected `Mock<ISettingsStore>` |
| `TaskTreeControllerMoveLogicTests` (`TaskTree.Test/TaskTreeControllerMoveLogicTests.cs`) | `IDList` (`MakeController`) | updated (P1-T2, P3-T3) | `MakeController` returns `new IDList()`; six tests reach `GetNextToDoID` |
| `TaskTreeControllerRouteDropTests` (`TaskTree.Test/TaskTreeControllerRouteDropTests.cs`) | `IDList` (`MakeController`) | updated (P3-T3) | `MakeController` returns `new IDList()`; three tests reach `GetNextToDoID` |
| `AppQuickFilerSettingsTests` | `AppQuickFilerSettings` | updated (P1-T3, P4-T5) | setter tests save through `Settings.Default` |
| `AppQuickFilerSettingsRemainingPropertiesTests` | `AppQuickFilerSettings` | updated (P4-T6) | setter tests save through `Settings.Default` |
| `AppStagingFilenamesTests` | `AppStagingFilenames` | updated (P1-T4, P4-T7) | setter tests save through `Settings.Default` |
| `AppAutoFileObjectsCoverageExpansionTests` | `AppAutoFileObjects`, `CreateSutWithSpecialFolders` | updated (P1-T5, P4-T8) | scalar setters save; `TestCleanup` calls `defaults.Save()` directly |
| `RibbonControllerTests` (`TaskMaster.Test/Ribbon/RibbonControllerTests.cs`) | `AppQuickFilerSettings`, `CreateController` | updated (P4-T10) | `CreateController()` injects a default `AppQuickFilerSettings`; four tests reach the saving setters |
| `DfDeedle_COM_Tests` (`UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`) | `AddQfcColumnsAsync` | updated (P5-T1) | two calls pass neither `columnAdder` nor `timeProvider` |

## Unchanged by design

| Test class (file) | Matching symbol(s) | Disposition | Reason |
| --- | --- | --- | --- |
| `AppAutoFileObjectsFolderPredictorTests` | `AppAutoFileObjects` | unchanged | parent-only constructor `new AppAutoFileObjects(mockGlobals.Object)`; reads no scalar member; the parent-only constructor keeps its signature and defaults the store to the adapter |
| `RibbonControllerTests` partial in `RibbonControllerTests.Engines.cs` | `CreateController` | unchanged | Decision D-E: the parameterless `CreateController()` keeps its signature |
| `DfDeedleQfcColumnTimeoutTests` | `AddQfcColumnsAsync` | unchanged | already passes both `columnAdder` and `timeProvider` |

## Further matches (each unchanged)

| File(s) | Matching symbol(s) | Disposition | Reason |
| --- | --- | --- | --- |
| `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` | `AppAutoFileObjects` | unchanged | creates an uninitialized `AppAutoFileObjects` via `GetUninitializedObject` and assigns it to `_autoFileObjects`; the test (`LoadWhenIdle`) reads no scalar member, so a null store field is never dereferenced |
| `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs`, `AppToDoObjectsCoverageExpansionTests.cs` | `IDList` (property) | unchanged | read the `AppToDoObjects.IDList` property and `Count`; no `GetNextToDoID` call |
| `TaskVisualization.Test/AutoCreateProjectTests.cs`, `ToDoModel.Test/Data Model/ToDo/ToDoItemCoverageExpansionTests.cs` | `GetNextToDoID` | unchanged | calls on `Mock<IIDList>`; no real `IDList` |
| `TaskMaster.Test/AppGlobals/AppToDoObjectsTestDoubles.cs`, `AppOlObjectsTests.cs`, `AppOlObjectsCoverageTests.cs`, `QuickFiler.Test/Controllers/EfcHomeController*Tests.cs`, `UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_TestSupport.cs` | interface-typed members `IAppAutoFileObjects`, `IAppQuickFilerSettings`, `IAppStagingFilenames`, `IIDList` | unchanged | interface members of hand-written globals doubles; interfaces do not change |
| `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs`, `TaskVisualization.Test/ManageFiltersControllerTests.cs`, every `QuickFiler.Test/Controllers/Qfc*Tests.cs` match, every `UtilitiesCS.Test/EmailIntelligence/**` and `UtilitiesCS.Test/OutlookObjects/Folder/**` match | `Mock<IAppAutoFileObjects>`, `Mock<IAppQuickFilerSettings>`, `Mock<IAppStagingFilenames>` | unchanged | Moq mocks of unchanged interfaces |
| `UtilitiesCS.Test/Threading/AppGlobalsConverterTests.cs`, `ToDoModel.Test/FolderPathsTest.cs` | `IAppStagingFilenames` | unchanged | commented-out code |
| 24 files with an unrelated local `CreateController` helper: `QuickFiler.Test/Controllers/EfcFormControllerIssue995Tests.cs`, `QfcFormControllerDeactivateTests.cs`, `QfcExplorerControllerTests.cs`, `QfcFormControllerCancelTeardownTests.cs`, `QfcFormControllerCleanupTests.cs`, `QfcExplorerController.ConversationViewTests.cs`, `QfcCollectionControllerTests.cs`, `QfcCollectionControllerLayout.StaTests.cs`, `QfcCollectionControllerDefects468MoveTests.cs`, `QfcCollectionControllerDarkModeTests.cs`, `EfcHomeControllerMetricsTests.cs`, `EfcHomeControllerLifecycleTests.cs`, `EfcHomeControllerExecuteMovesTests.cs`, `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs`, `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests*.cs` (4), `StoreWrapperViewerTests.cs`, `DisabledStoresControllerTests.cs`, `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersController_Tests.cs`, `FolderRemapViewer_Tests.cs`, `FolderRemapController_Tests.cs`, `FolderRemapController_Tests2.cs` | `CreateController` (name only) | unchanged | each is a class-local helper building a different controller; none constructs `RibbonController` or `AppQuickFilerSettings` |

Result: every match carries a disposition; no match outside the Write Set must change.
