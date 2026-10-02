# P0-T6 Baseline Unsafe census over app.config files (AC2 evidence)

Timestamp: 2026-10-02T03-08
Command: Grep pattern `name="System.Runtime.CompilerServices.Unsafe"` with -A 1 and -n, content mode, path `<execution-worktree-root>`, glob `**/app.config` (glob substitution as recorded in p0-t4)
EXIT_CODE: 0

17 blocks; the line following every assemblyIdentity line reads `<bindingRedirect oldVersion="0.0.0.0-6.0.3.0" newVersion="6.0.3.0" />`. Files (assemblyIdentity line number in parentheses):

```text
QuickFiler/app.config (6)
QuickFiler.Test/app.config (18)
SVGControl/app.config (18)
SVGControl.Test/app.config (22)
Tags/app.config (18)
Tags.Test/app.config (10)
TaskMaster/app.config (22)
TaskMaster.Test/app.config (10)
TaskTree/app.config (18)
TaskTree.Test/app.config (10)
TaskVisualization/app.config (18)
TaskVisualization.Test/app.config (18)
ToDoModel/app.config (11)
ToDoModel.Test/app.config (18)
UtilitiesCS/app.config (11)
UtilitiesCS.Test/app.config (14)
VBFunctions.Test/app.config (10)
```

Acceptance: 17 blocks, every following line reads `oldVersion="0.0.0.0-6.0.3.0" newVersion="6.0.3.0"`; the 17 files are listed above.

Output Summary: 17 Unsafe redirect blocks, all at 6.0.3.0 (AC2 baseline holds; verified, not edited).
