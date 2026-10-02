# P2-T13 AC-5 footprint, status letters

Timestamp: 2026-10-01T12-16
Command: git diff --cached --name-status origin/main -- . ":(exclude)docs/features"
EXIT_CODE: 0
Output Summary: Eight `D` lines for the eight paths of P1-T1 and one `M` line for `.gitignore`. The companion `git status --porcelain -- . ":(exclude)docs/features"` is recorded in P2-T12.

```
M	.gitignore
D	QuickFiler.Test/QuickFiler.Test.csproj.bak
D	QuickFiler/QuickFiler.csproj.bak
D	Tags/Tags.csproj.bak
D	TaskTree/TaskTree.csproj.bak
D	TaskVisualization.Test/TaskVisualization.Test.csproj.bak
D	TaskVisualization/TaskVisualization.csproj.bak
D	ToDoModel.Test/ToDoModel.Test.csproj.bak
D	ToDoModel/ToDoModel.csproj.bak
```
