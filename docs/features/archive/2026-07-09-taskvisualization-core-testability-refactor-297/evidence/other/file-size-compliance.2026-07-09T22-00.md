# File-Size Compliance (P4-T7)

Timestamp: 2026-07-09T22-00
Command: wc -l on each in-scope production file

| File | Lines | <= 500 |
|---|---|---|
| TaskVisualization/TaskController.cs | 306 | yes |
| TaskVisualization/TaskController.Actions.cs | 490 | yes |
| TaskVisualization/TaskController.Flags.cs | 170 | yes |
| TaskVisualization/TaskController.Accelerator.cs | 500 | yes |
| TaskVisualization/TaskController.ControlMaps.cs | 296 | yes |
| TaskVisualization/TaskController.ControlRelationships.cs | 259 | yes |
| TaskVisualization/ITaskViewer.cs | 75 | yes |
| TaskVisualization/ITaskViewerControls.cs | 123 | yes |
| TaskVisualization/TaskDurationParser.cs | 61 | yes |
| TaskVisualization/TaskPriorityMapper.cs | 60 | yes |
| TaskVisualization/ITagPromptService.cs | 102 | yes |
| TaskVisualization/TagPromptService.cs | 49 | yes |

Contingency applied: `TaskController.ControlMaps.cs` initially measured 533 lines (over 500)
because the decomposition moved the ~120 lines of commented-out legacy `Create*Lookup`
blocks along with the region. Per the P4-T7 contingency, `GetControlRelationships` + the
`ControlRelationship` struct were extracted into
`TaskVisualization/TaskController.ControlRelationships.cs` (a new `public partial class
TaskController`, non-exempt / STA-measured), which was added to the csproj `<Compile>` group.
Result: ControlMaps.cs = 296, ControlRelationships.cs = 259.

AC: every listed file (including the contingency file) is <= 500 lines. CONFIRMED.
