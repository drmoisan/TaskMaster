# P2-T2 AC-1 working-tree absence

Timestamp: 2026-10-01T12-16
Command: Read tool on each of QuickFiler.Test/QuickFiler.Test.csproj.bak, QuickFiler/QuickFiler.csproj.bak, Tags/Tags.csproj.bak, TaskTree/TaskTree.csproj.bak, TaskVisualization.Test/TaskVisualization.Test.csproj.bak, TaskVisualization/TaskVisualization.csproj.bak, ToDoModel.Test/ToDoModel.Test.csproj.bak, ToDoModel/ToDoModel.csproj.bak (eight Read calls, limit 5), plus Read of QuickFiler/QuickFiler.csproj (limit 5) as control
EXIT_CODE: 0
Output Summary: All eight backup reads returned a file-does-not-exist error. The control read of QuickFiler/QuickFiler.csproj succeeded and returned its first lines (XML declaration and Project element).
