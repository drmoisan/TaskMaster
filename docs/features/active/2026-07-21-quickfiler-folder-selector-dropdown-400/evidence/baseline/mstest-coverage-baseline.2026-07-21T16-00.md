# MSTest coverage baseline

Timestamp: 2026-07-21T16-00Z

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput 'docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\baseline\coverage-baseline.2026-07-21T16-00.cobertura.xml'`

EXIT_CODE: 0

- Total: 5713
- Passed: 5713
- Failed: 0
- Skipped: 0
- Elapsed test time: 50.5120 seconds
- Wrapper wall time: 66.3 seconds
- Cobertura: `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/baseline/coverage-baseline.2026-07-21T16-00.cobertura.xml`
- Filter: `/TestCaseFilter:TestCategory!=LiveOutlook`

The wrapper discovered and ran all eight first-party test assemblies:

- `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
- `Tags.Test/bin/Debug/Tags.Test.dll`
- `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`
- `TaskTree.Test/bin/Debug/TaskTree.Test.dll`
- `TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll`
- `ToDoModel.Test/bin/Debug/ToDoModel.Test.dll`
- `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`
- `VBFunctions.Test/bin/Debug/VBFunctions.Test.dll`

The baseline has no failing or skipped test and therefore has no fully qualified failure signature to report.

Output Summary: The exact repository coverage wrapper completed successfully, discovered all eight first-party test assemblies including `UtilitiesCS.Test.dll` and `QuickFiler.Test.dll`, and passed 5,713 of 5,713 tests with zero failures or skips. The direct baseline Cobertura artifact was produced at the recorded path.

## Collection diagnostic

Three preceding bounded attempts hung inside `dotnet-coverage` when test assemblies were instrumented along with production assemblies. Direct VSTest and complete split coverage collections proved all 5,713 tests complete. For this successful exact wrapper invocation, `.*\.Test\.dll$` was added to `coverage.config` only for process instrumentation; all test assemblies were still discovered and executed by the wrapper, while test code was excluded from coverage as repository policy requires. The configuration was restored immediately after collection, and `git diff --exit-code -- coverage.config` returned `0`.
