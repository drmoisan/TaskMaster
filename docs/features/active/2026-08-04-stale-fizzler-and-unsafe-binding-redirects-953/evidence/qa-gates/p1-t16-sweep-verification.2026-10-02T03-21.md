# P1-T16 Sweep verification over app.config files (AC1 evidence)

Timestamp: 2026-10-02T03-21
Command: Grep `oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0"` (count mode) and Grep `oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0"` (count mode), glob `**/app.config`, path `<execution-worktree-root>` (glob substitution recorded at P0-T4: `*/app.config` returns no match at this worktree depth; `**/app.config` returns the same 17 root-level files); Grep `name="Fizzler"` -A 1, same glob and path; git -C <execution-worktree-root> diff --name-only 860d67bf4fddecb929e0d6c166065fd1ee752feb -- '*/app.config'; git -C <execution-worktree-root> status --porcelain -- '*/app.config'
EXIT_CODE: 0

```text
COUNT 1.3.1.0 redirect (oldVersion 0.0.0.0-1.3.1.0 / newVersion 1.3.1.0): 13 occurrences across 13 files
  QuickFiler, QuickFiler.Test, SVGControl, SVGControl.Test, Tags, TaskMaster, TaskTree, TaskVisualization,
  TaskVisualization.Test, ToDoModel, ToDoModel.Test, UtilitiesCS, UtilitiesCS.Test (1 each)
COUNT 1.3.0.0 redirect (oldVersion 0.0.0.0-1.3.0.0 / newVersion 1.3.0.0): 6 occurrences across 6 files
  QuickFiler, Tags, TaskMaster, TaskTree, TaskVisualization, ToDoModel (1 each; the System.ClientModel redirect)
FIZZLER BLOCKS: 13 blocks, every following line reads oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0"
```

```text
git diff --name-only <BASE_SHA> -- '*/app.config' (11 paths):
QuickFiler.Test/app.config
QuickFiler/app.config
SVGControl.Test/app.config
Tags/app.config
TaskMaster/app.config
TaskTree/app.config
TaskVisualization.Test/app.config
TaskVisualization/app.config
ToDoModel.Test/app.config
ToDoModel/app.config
UtilitiesCS.Test/app.config

git status --porcelain -- '*/app.config' (11 lines):
 M QuickFiler.Test/app.config
 M QuickFiler/app.config
 M SVGControl.Test/app.config
 M Tags/app.config
 M TaskMaster/app.config
 M TaskTree/app.config
 M TaskVisualization.Test/app.config
 M TaskVisualization/app.config
 M ToDoModel.Test/app.config
 M ToDoModel/app.config
 M UtilitiesCS.Test/app.config
```

The pathspec was passed unquoted from the Bash tool; the shell found no match for the glob in its own working directory and passed it to git literally, so git applied the pathspec glob. The 11 listed paths equal the 11 Write Set config paths of section 5 and the porcelain lines are exactly 11 ` M` lines for the same paths.

Acceptance: all four observations hold (13 across 13; exactly 6 across exactly the six named files; 13 Fizzler blocks at 1.3.1.0 in both attributes; diff and porcelain agree on exactly the 11 Write Set config paths).

Output Summary: Sweep verified. 13 Fizzler redirects at 1.3.1.0 across 13 files; the 6 remaining 1.3.0.0 strings are the System.ClientModel redirects; 11 config files modified, matching the Write Set.
