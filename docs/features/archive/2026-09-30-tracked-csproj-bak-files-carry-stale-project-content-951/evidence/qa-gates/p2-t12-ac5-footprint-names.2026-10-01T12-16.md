# P2-T12 AC-5 footprint, names

Timestamp: 2026-10-01T12-16
Command: git diff --cached --name-only origin/main -- . ":(exclude)docs/features"
EXIT_CODE: 0
Output Summary: Exactly nine paths: `.gitignore` and the eight deletions of P1-T1.

```
.gitignore
QuickFiler.Test/QuickFiler.Test.csproj.bak
QuickFiler/QuickFiler.csproj.bak
Tags/Tags.csproj.bak
TaskTree/TaskTree.csproj.bak
TaskVisualization.Test/TaskVisualization.Test.csproj.bak
TaskVisualization/TaskVisualization.csproj.bak
ToDoModel.Test/ToDoModel.Test.csproj.bak
ToDoModel/ToDoModel.csproj.bak
```

Companion Command: git status --porcelain -- . ":(exclude)docs/features"
Companion Output:

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

The companion output is the same nine entries as in P1-T5 and nothing else.
