# P1-T17 Unsafe re-verification after the edits (AC2 evidence)

Timestamp: 2026-10-02T03-21
Command: Grep `name="System.Runtime.CompilerServices.Unsafe"` -A 1 (content mode, no head limit), glob `**/app.config`, path `<execution-worktree-root>` (glob substitution recorded at P0-T4); git -C <execution-worktree-root> diff -U0 860d67bf4fddecb929e0d6c166065fd1ee752feb -- '*/app.config'
EXIT_CODE: 0

```text
UNSAFE BLOCKS: 17, every following line reads oldVersion="0.0.0.0-6.0.3.0" newVersion="6.0.3.0"
Files (17): QuickFiler, QuickFiler.Test, SVGControl, SVGControl.Test, Tags, Tags.Test, TaskMaster, TaskMaster.Test,
  TaskTree, TaskTree.Test, TaskVisualization, TaskVisualization.Test, ToDoModel, ToDoModel.Test, UtilitiesCS,
  UtilitiesCS.Test, VBFunctions.Test
```

The diff of the app.config files against BASE_SHA contains 22 content lines: 11 removed lines, each `        <bindingRedirect oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0" />`, and 11 added lines, each `        <bindingRedirect oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0" />`, one pair per file in the 11 Write Set configs (hunks at QuickFiler.Test 47, QuickFiler 51, SVGControl.Test 19, Tags 47, TaskMaster 51, TaskTree 47, TaskVisualization.Test 47, TaskVisualization 47, ToDoModel.Test 47, ToDoModel 52, UtilitiesCS.Test 47). No content line contains `Unsafe`. The diff was taken with `-U0`, which removes context lines only; the content lines are the same as with default context.

Acceptance: both hold. 17 Unsafe blocks unchanged at 6.0.3.0; the 22 diff content lines are exactly the 11 removed and 11 added Fizzler redirect lines and none contains `Unsafe`.

Output Summary: Unsafe redirects unchanged. 17 blocks all at 6.0.3.0; the app.config diff against BASE_SHA holds only the 11 Fizzler redirect line replacements (22 content lines), none mentioning Unsafe.
