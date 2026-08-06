Timestamp: 2026-08-05T05:45:00-04:00
Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/evidence/qa-gates/remediation-cycle3-coverage-final.cobertura.xml`
EXIT_CODE: 0
Output Summary: The required isolated coverage wrapper discovered eight test assemblies and passed 6,137/6,137 tests in 53.6489 seconds. Final repository coverage is 93,441/110,477 lines (84.5796%) and 21,404/27,696 branches (77.2819%). The processed Cobertura report has one source root, 531 source files, and nine packages.

Discovered test assemblies:

- `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
- `Tags.Test/bin/Debug/Tags.Test.dll`
- `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`
- `TaskTree.Test/bin/Debug/TaskTree.Test.dll`
- `TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll`
- `ToDoModel.Test/bin/Debug/ToDoModel.Test.dll`
- `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`
- `VBFunctions.Test/bin/Debug/VBFunctions.Test.dll`

Wrapper controls: the script invoked VSTest through `dotnet-coverage` with `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook`; instrumentation uses the output-adjacent effective coverage settings file.
