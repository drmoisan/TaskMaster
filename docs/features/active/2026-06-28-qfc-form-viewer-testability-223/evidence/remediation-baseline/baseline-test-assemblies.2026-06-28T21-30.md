# Baseline — First-Party Test Assemblies Present (Remediation Cycle 1, Issue #223)

Timestamp: 2026-06-28T21-46
Command: find . -path '*/bin/Debug/*' -name '*.Test.dll' -not -path '*/obj/*' -not -path '*/ref/*'
EXIT_CODE: 0

Output Summary:
A Debug build of TaskMaster.sln is present; all seven expected first-party `*.Test.dll` assemblies were discovered under `**/bin/Debug/`:

- QuickFiler.Test/bin/Debug/QuickFiler.Test.dll
- Tags.Test/bin/Debug/Tags.Test.dll
- TaskMaster.Test/bin/Debug/TaskMaster.Test.dll
- TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll
- ToDoModel.Test/bin/Debug/ToDoModel.Test.dll
- UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll
- VBFunctions.Test/bin/Debug/VBFunctions.Test.dll

These seven assemblies are the auto-discovered inputs for `dotnet-coverage collect` via `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (which filters `*.Test.dll` under `bin/Debug`, excluding `obj`/`ref`). Phase 1 refreshes the Debug build before instrumentation.
