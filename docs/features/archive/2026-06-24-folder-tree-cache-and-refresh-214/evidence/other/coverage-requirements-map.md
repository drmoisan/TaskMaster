# Coverage Requirements Map

Issue: 214

## Thresholds

- Repository line coverage must remain `>= 80%`.
- New issue #214 modules, classes, and methods must target `>= 90%` coverage.
- Changed-line coverage must not regress below the P0 baseline evidence.

## Baseline Evidence

- Baseline coverage XML: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/baseline/baseline-coverage.xml`
- Baseline summary: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/baseline/baseline-coverage-summary.md`
- Recorded baseline repository line coverage: `82.54%`

## Issue 214 Production Coverage Targets

| Production file | Issue #214 change type | Coverage requirement |
| --- | --- | --- |
| `UtilitiesCS/OutlookObjects/Folder/FolderTreeNodeKey.cs` | New folder snapshot key model | New code target `>= 90%` |
| `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotNode.cs` | New immutable snapshot node model | New code target `>= 90%` |
| `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshot.cs` | New immutable snapshot container | New code target `>= 90%` |
| `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs` | New snapshot query helpers | New code target `>= 90%` |
| `UtilitiesCS/OutlookObjects/Folder/FolderTreeSelectionOverlay.cs` | New caller-local selection overlay | New code target `>= 90%` |
| `UtilitiesCS/OutlookObjects/Folder/FolderTreeCompatibilityView.cs` | New legacy tree projection view | New code target `>= 90%` |
| `UtilitiesCS/OutlookObjects/Folder/FolderTreeRequest.cs` | New snapshot request model | New code target `>= 90%` |
| `UtilitiesCS/OutlookObjects/Folder/IOutlookFolderTreeService.cs` | New service contract | Interface covered through consumers and service tests |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderTreeService.cs` | New cache service | New code target `>= 90%` |
| `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotBuilder.cs` | New snapshot builder | New code target `>= 90%` |
| `UtilitiesCS/OutlookObjects/Folder/IOutlookFolderHierarchyReader.cs` | New hierarchy reader contract | Interface covered through builder and service tests |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs` | New Outlook hierarchy reader | New code target `>= 90%` with fake adapters |
| `UtilitiesCS/OutlookObjects/Folder/IFolderHandleResolver.cs` | New handle resolver contract | Interface covered through resolver and caller tests |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHandleResolver.cs` | New Outlook handle resolver | New code target `>= 90%` with fake lookup seams |
| `UtilitiesCS/OutlookObjects/Folder/IOutlookFolderNotificationSink.cs` | New notification sink contract | Interface covered through service tests |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderNotificationSink.cs` | New notification sink | New code target `>= 90%` |
| `UtilitiesCS/OutlookObjects/Folder/IDeadlineClock.cs` | New deadline/yield seam | Interface covered through builder tests |
| `UtilitiesCS/OutlookObjects/Folder/DeadlineClock.cs` | New deadline implementation | New code target `>= 90%` |
| `UtilitiesCS/OutlookObjects/Folder/IDispatcherYield.cs` | New dispatcher yield seam | Interface covered through builder tests |
| `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` | New dispatcher yield implementation | New code target `>= 90%` |
| `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs` | Adds folder tree service contract | Changed-line coverage through app globals and caller tests |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | Adds lazy folder tree service ownership/disposal | Changed-line coverage through app globals tests |
| `TaskMaster/Ribbon/RibbonController.cs` | Migrates folder info and compare callers | Changed-line coverage through ribbon tests |
| `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs` | Migrates mining folder discovery and handle resolution | Changed-line coverage through email data miner tests |
| `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs` | Migrates filter view to snapshot compatibility view | Changed-line coverage through filter controller tests |
| `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/IFilterOlFoldersViewer.cs` | Adds close event to viewer abstraction | Changed-line coverage through filter controller/viewer tests |
| `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersViewer.cs` | Uses controller filtering API | Changed-line coverage through viewer tests |
| `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FolderInfoViewer.cs` | Accepts disposable compatibility view | Changed-line coverage through viewer tests |
| `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs` | Migrates subject-map folder discovery to snapshots and resolver boundary | Changed-line coverage through subject-map orchestration tests |

## Final Comparison Target

`docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/final-coverage-comparison.md` must compare final coverage against the baseline artifacts and report repository, new-code, and changed-line results.
