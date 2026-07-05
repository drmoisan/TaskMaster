Timestamp: 2026-07-04T18-52
Command: PowerShell XML parser over docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/remediation-baseline/remediation-cycle2-baseline-coverage.cobertura.xml; normalize active worktree and stale C:\Users\DanMoisan\repos\TaskMaster roots; merge line entries by normalized file path and line number.
EXIT_CODE: 0
Output Summary:
- Baseline repository line coverage: 46.25% (83400/180333).
- Required covered lines for 80.00% raw coverage: 144267.
- Additional covered-line gap to 80.00% raw coverage: 60867.
- Stale-root duplicate filename groups found: 393.
- Normalized coverage estimate after equivalent-path merge: 79.15% (43177/54553).
- Required covered lines for 80.00% normalized coverage: 43643.
- Residual covered-line gap after normalization: 466.

## Stale-Root Duplicate Filename Groups

Representative duplicate groups where relative worktree paths and stale `C:\Users\DanMoisan\repos\TaskMaster\...` paths describe the same repository file:

| Normalized path | Entries |
| --- | ---: |
| `Tags\Helper Classes\PrefixItem.cs` | 2 |
| `Tags\TagController.cs` | 2 |
| `Tags\TagViewer.cs` | 2 |
| `Tags\TagViewer.Designer.cs` | 2 |
| `TaskMaster\AppGlobals\AppAutoFileObjects.cs` | 2 |
| `TaskMaster\AppGlobals\AppAutoFileObjects.FolderPredictorLoad.cs` | 2 |
| `TaskMaster\AppGlobals\AppEvents.cs` | 2 |
| `TaskMaster\AppGlobals\AppEvents.ReadinessHookup.cs` | 2 |
| `TaskMaster\AppGlobals\AppFileSystemFolderPaths.cs` | 2 |
| `TaskMaster\AppGlobals\ApplicationGlobals.cs` | 2 |
| `TaskMaster\AppGlobals\AppOlObjects.cs` | 2 |
| `TaskMaster\AppGlobals\AppOlObjects.JunkFolders.cs` | 2 |
| `TaskMaster\AppGlobals\AppQuickFilerSettings.cs` | 2 |
| `TaskMaster\AppGlobals\AppStagingFilenames.cs` | 2 |
| `TaskMaster\AppGlobals\AppToDoObjects.cs` | 2 |
| `TaskMaster\AppGlobals\EngineInitTimingProbe.cs` | 2 |
| `TaskMaster\AppGlobals\HookReadinessCoordinator.cs` | 2 |
| `TaskMaster\AppGlobals\JunkFolderPathNavigator.cs` | 2 |
| `TaskMaster\AppGlobals\NonBlockingDelay.cs` | 2 |
| `TaskMaster\AppGlobals\StartupDiagnosticsProbe.cs` | 2 |
| `TaskMaster\AppGlobals\StartupInboxAttributionProbe.cs` | 2 |
| `TaskMaster\AppGlobals\StartupTimingRecorder.cs` | 2 |
| `TaskMaster\Properties\Settings.Designer.cs` | 2 |
| `TaskVisualization\FlagChangeGroup.cs` | 2 |
| `TaskVisualization\FlagChangeItem.cs` | 2 |
| `TaskVisualization\FlagChangeTrainingQueue.cs` | 2 |
| `ToDoModel\Data Model\ID\BaseChanger.cs` | 2 |
| `ToDoModel\Data Model\ID\IDList.cs` | 2 |
| `ToDoModel\Data Model\Project\ProgramData.cs` | 2 |
| `ToDoModel\Data Model\Project\ProjectData.cs` | 2 |

## Residual Candidate Table

| File | Covered | Valid | Uncovered | Coverage | Entries |
| --- | ---: | ---: | ---: | ---: | ---: |
| `SVGControl\RelativePath.cs` | 147 | 774 | 627 | 18.99% | 1 |
| `ToDoModel\Data Model\ToDo\ToDoItem.cs` | 284 | 820 | 536 | 34.63% | 2 |
| `QuickFiler\Helper Classes\EfcThemeHelper.cs` | 0 | 440 | 440 | 0.00% | 1 |
| `QuickFiler\Controllers\QfcQueue.cs` | 47 | 386 | 339 | 12.18% | 1 |
| `Tags\TagController.cs` | 249 | 578 | 329 | 43.08% | 2 |
| `ToDoModel\Data Model\Project\ProjectData.cs` | 7 | 216 | 209 | 3.24% | 2 |
| `SVGControl\ValueStringBuilder.cs` | 0 | 207 | 207 | 0.00% | 1 |
| `TaskMaster\AppGlobals\AppOlObjects.cs` | 93 | 275 | 182 | 33.82% | 2 |
| `TaskMaster\AppGlobals\AppAutoFileObjects.cs` | 224 | 403 | 179 | 55.58% | 2 |
