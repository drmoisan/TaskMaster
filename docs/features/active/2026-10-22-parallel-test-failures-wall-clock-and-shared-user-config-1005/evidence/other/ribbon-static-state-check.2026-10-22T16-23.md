# RibbonControllerTests Static-State Check (AC8)

Timestamp: 2026-10-22T16-23
Command: Read of the files listed below in full; Grep for `\bstatic\b`, `Settings\.Default`, `Environment\.`, `log4net`, `LogManager`, `UiThread`, `Dispatcher`, `SynchronizationContext` over `TaskMaster/Ribbon/RibbonController*.cs`, `TaskMaster/AppGlobals/ApplicationGlobals.cs`, the test partials, and the UtilitiesCS folder-tree types the tests construct
EXIT_CODE: 0
Output Summary: No mutable process-global static state is written by any test in RibbonControllerTests other than the high-confidence Settings.Default snapshot, which P4-T10 removes. Verdict: REMOVE.

## Files read

- `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` (whole file)
- `TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs` (whole file)
- `TaskMaster/Ribbon/RibbonController.cs` (constructor, `Globals`, `SetGlobals`, `FolderTreeService`, static logger)
- `TaskMaster/Ribbon/RibbonController.Intelligence.cs` (whole file; settings-menu and high-confidence helpers)
- `TaskMaster/Ribbon/RibbonController.FolderTree.cs` (whole file)
- `TaskMaster/Ribbon/RibbonController.EngineCommands.cs` (static-member search)
- `TaskMaster/AppGlobals/ApplicationGlobals.cs` (`_quickFilerSettings`, `QfSettings`, `InternalQfSettings`, `Engines`, static members)
- `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` (after P4-T1: instance store field only)
- UtilitiesCS folder-tree types constructed by the comparison tests: `FolderTreeSnapshot`, `FolderTreeSnapshotNode`, `FolderTreeNodeKey`, `FolderTreeRequest`, `FolderTreeSnapshotQueries`, `FolderTreeCompatibilityView`, `FolderWrapper`, and the three `FolderWrapper*Comparer` types (static-member search)

## Static members found

| Static member | Mutable | Written by a test in the class | Notes |
| --- | --- | --- | --- |
| `RibbonController.logger` (`static readonly log4net.ILog`) | no (readonly, assigned once by the type initializer) | no | log4net configuration is not changed by any test in the class |
| `ApplicationGlobals.logger` (`static readonly log4net.ILog`) | no | no | the class creates `ApplicationGlobals` through `FormatterServices.GetUninitializedObject`, which runs no instance constructor |
| `ApplicationGlobals.StopAndRestart` (static method) | n/a (no state) | no | not reached by the tests |
| `RibbonController.FormatFileSize`, `CompareFolderSnapshots`, `CreateViewForFolder`, `CompareFolderNodes`, `CompareFolderNodeMembers` (static methods) | n/a (pure functions over arguments) | no | `GetStats_*` and `CompareFolderSnapshots_*` call them; no shared state |
| `FolderWrapper.logger` (`static readonly`) | no | no | |
| `FolderTreeSnapshotQueries`, `FolderTreeRequest.AllStores`/`ForStore`, `FolderTreeNodeKey`/`FolderTreeSnapshotNode` private static helpers | n/a (pure) | no | |
| `SynchronizationContext.SetSynchronizationContext(...)` in `RibbonController` members (`ScrapeAndMineAsync`, `TriageAsync`, `SB`, `PromptUserToSelectFolder`, and others) | thread-local, not process-global | no | none of these members is invoked by a test in the class; `GetFolderTreeSnapshotAsync` (both overloads), `GetStats`, `IsHighConfidenceModeActive`, `ToggleHighConfidenceMode`, `SetHighConfidenceModeForLaunch`, `Get/SetHighConfidenceThresholdText`, and `Engines` do not touch it |
| `TaskMaster.Properties.Settings.Default` high-confidence members | yes | yes before P4-T10 (snapshot, restore, and setter saves) | removed by P4-T10: the tests inject `new AppQuickFilerSettings(store)` over a per-test `InMemorySettingsStore` |
| Other `Settings.Default` members | n/a | no | no test in the class reads or writes any other setting |
| Environment variables, UI-thread or dispatcher statics, singletons | none found | no | `RibbonFolderOperations_DoNotConstructThrowawayFolderTrees` reads a tracked source file read-only; it writes nothing |

## Verdict

REMOVE

Rationale: after P4-T10, every test in `RibbonControllerTests` (both partial files) operates on per-test instances (`RibbonController`, an uninitialized `ApplicationGlobals`, its own `AppQuickFilerSettings` over its own `InMemorySettingsStore`, and Moq mocks). No mutable process-global static state is written, so the class-level `[DoNotParallelize]` has no remaining reason.
