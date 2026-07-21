# Banned-API Scan (P0-T6)

Timestamp: 2026-07-09T22-00
Command: grep -nE "DateTime\.Now|DateTime\.UtcNow|Random\.Shared|Thread\.Sleep|Task\.Delay" <files>

Banned symbols (per BannedSymbols.txt): DateTime.Now, DateTime.UtcNow, Random.Shared,
Thread.Sleep, Task.Delay.

## Currently-existing production files (must be banned-symbol-free)
- TaskVisualization/TaskController.cs — none
- TaskVisualization/TaskViewer.cs — none

## Files to be created during this plan (scanned as created; recorded at final QA)
- TaskVisualization/ITaskViewer.cs — none
- TaskVisualization/ITaskViewerControls.cs — none
- TaskVisualization/TaskDurationParser.cs — none
- TaskVisualization/TaskPriorityMapper.cs — none
- TaskVisualization/ITagPromptService.cs — none
- TaskVisualization/TagPromptService.cs — none
- TaskVisualization/TaskController.Accelerator.cs — none
- TaskVisualization/TaskController.ControlMaps.cs — none
- TaskVisualization/TaskController.Flags.cs — none
- TaskVisualization/TaskController.Actions.cs — none
- TaskVisualization.Test/*Tests.cs and STA support — none

Note: The new files above are created in later phases; this artifact is updated / reconfirmed
at final QA (P7). The two currently-existing production files are confirmed banned-symbol-free.

AC: each currently-existing production file confirmed banned-symbol-free (per-file `none`). CONFIRMED.
