# P0-T5 Baseline shared-string census over app.config files

Timestamp: 2026-10-02T03-08
Command: Grep pattern `oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0"` count mode, glob `**/app.config`; Grep pattern `name="System.ClientModel"` with -A 1 and -n, content mode, glob `**/app.config` (path `<execution-worktree-root>`; glob substitution as recorded in p0-t4)
EXIT_CODE: 0

Shared-string counts (17 occurrences in 11 files):

```text
TaskTree/app.config 2
Tags/app.config 2
TaskMaster/app.config 2
QuickFiler/app.config 2
ToDoModel/app.config 2
TaskVisualization/app.config 2
UtilitiesCS.Test/app.config 1
SVGControl.Test/app.config 1
QuickFiler.Test/app.config 1
ToDoModel.Test/app.config 1
TaskVisualization.Test/app.config 1
```

System.ClientModel blocks: 16 (SVGControl/app.config carries none). Per-file values (line is the redirect line):

```text
CLIENTMODEL TaskTree/app.config=1.3.0.0 (line 103)
CLIENTMODEL TaskMaster/app.config=1.3.0.0 (line 115)
CLIENTMODEL Tags/app.config=1.3.0.0 (line 103)
CLIENTMODEL QuickFiler/app.config=1.3.0.0 (line 103)
CLIENTMODEL TaskVisualization/app.config=1.3.0.0 (line 103)
CLIENTMODEL ToDoModel/app.config=1.3.0.0 (line 108)
CLIENTMODEL VBFunctions.Test/app.config=1.16.0.0 (line 99)
CLIENTMODEL SVGControl.Test/app.config=1.16.0.0 (line 47)
CLIENTMODEL TaskMaster.Test/app.config=1.16.0.0 (line 99)
CLIENTMODEL UtilitiesCS.Test/app.config=1.16.0.0 (line 123)
CLIENTMODEL Tags.Test/app.config=1.16.0.0 (line 223)
CLIENTMODEL QuickFiler.Test/app.config=1.16.0.0 (line 103)
CLIENTMODEL UtilitiesCS/app.config=1.16.0.0 (line 108)
CLIENTMODEL TaskVisualization.Test/app.config=1.16.0.0 (line 103)
CLIENTMODEL ToDoModel.Test/app.config=1.16.0.0 (line 103)
CLIENTMODEL TaskTree.Test/app.config=1.16.0.0 (line 223)
```

Acceptance: both expectations hold. 17 occurrences across 11 files with 2 each in QuickFiler, Tags, TaskMaster, TaskTree, TaskVisualization, ToDoModel and 1 each in QuickFiler.Test, SVGControl.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test. 16 System.ClientModel blocks, at 1.3.0.0 for exactly Tags 103, TaskVisualization 103, TaskTree 103, QuickFiler 103, TaskMaster 115, ToDoModel 108 and at 1.16.0.0 for the other 10. No TREE-DRIFT.

Output Summary: 17 shared-string occurrences in 11 files; 16 System.ClientModel blocks (6 at 1.3.0.0, 10 at 1.16.0.0) at the plan's line numbers. Matches the plan.
