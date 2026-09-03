Timestamp: 2026-09-03T12-05
Command: PowerShell scan of every non-packages `*.csproj` under the worktree root, resolving each `<Analyzer Include>` path joined to the project directory, plus a `packages.config` Meziantou.Analyzer/Roslynator.Analyzers version cross-check.
EXIT_CODE: 0

Per-project results (18 first-party csproj files scanned, `packages\` subtree excluded):

| Project | Analyzer Items | Resolved | Unresolved |
|---|---|---|---|
| QuickFiler\QuickFiler.csproj | 9 | 9 | 0 |
| QuickFiler.Test\QuickFiler.Test.csproj | 11 | 11 | 0 |
| SVGControl\SVGControl.csproj | 0 | 0 | 0 |
| SVGControl.Test\SVGControl.Test.csproj | 2 | 2 | 0 |
| Tags\Tags.csproj | 9 | 9 | 0 |
| Tags.Test\Tags.Test.csproj | 11 | 11 | 0 |
| TaskMaster\TaskMaster.csproj | 9 | 9 | 0 |
| TaskMaster.Test\TaskMaster.Test.csproj | 11 | 11 | 0 |
| TaskTree\TaskTree.csproj | 9 | 9 | 0 |
| TaskTree.Test\TaskTree.Test.csproj | 11 | 11 | 0 |
| TaskVisualization\TaskVisualization.csproj | 9 | 9 | 0 |
| TaskVisualization.Test\TaskVisualization.Test.csproj | 11 | 11 | 0 |
| ToDoModel\ToDoModel.csproj | 9 | 9 | 0 |
| ToDoModel.Test\ToDoModel.Test.csproj | 11 | 11 | 0 |
| UtilitiesCS\UtilitiesCS.csproj | 9 | 9 | 0 |
| UtilitiesCS.Test\UtilitiesCS.Test.csproj | 11 | 11 | 0 |
| VBFunctions\VBFunctions.csproj | 9 | 9 | 0 |
| VBFunctions.Test\VBFunctions.Test.csproj | 11 | 11 | 0 |

TOTAL_RESOLVED: 152
TOTAL_UNRESOLVED: 0

For every project carrying a `packages.config` entry for `Meziantou.Analyzer` and `Roslynator.Analyzers`, the version token embedded in the `<Analyzer Include>` path matched the `packages.config`-declared version (`Meziantou.Analyzer` 3.0.194, `Roslynator.Analyzers` 5.0.0) in all cases.

ANALYZER_SKEW_BLOCKING: none

Output Summary: 152 of 152 `<Analyzer Include>` items resolve to an on-disk DLL across all 18 first-party projects; 0 unresolved. Meziantou/Roslynator version tokens agree with packages.config on every project that references them. No analyzer wiring skew detected in this execution pass.
