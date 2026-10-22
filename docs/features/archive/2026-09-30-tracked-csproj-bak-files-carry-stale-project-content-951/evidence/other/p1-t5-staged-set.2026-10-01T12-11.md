# P1-T5 staged set

Timestamp: 2026-10-01T12-11
Command: git status --porcelain -- . ":(exclude)docs/features"
EXIT_CODE: 0
Output Summary: Exactly nine lines: one `M  .gitignore` and eight `D  <path>` lines for the eight paths of P1-T1.

```
M  .gitignore
D  QuickFiler.Test/QuickFiler.Test.csproj.bak
D  QuickFiler/QuickFiler.csproj.bak
D  Tags/Tags.csproj.bak
D  TaskTree/TaskTree.csproj.bak
D  TaskVisualization.Test/TaskVisualization.Test.csproj.bak
D  TaskVisualization/TaskVisualization.csproj.bak
D  ToDoModel.Test/ToDoModel.Test.csproj.bak
D  ToDoModel/ToDoModel.csproj.bak
```
