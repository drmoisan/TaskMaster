# Remediation Baseline — Test + Coverage (Issue #202)

Timestamp: 2026-06-15T13-29

Command: `vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll QuickFiler.Test/bin/Debug/QuickFiler.Test.dll Tags.Test/bin/Debug/Tags.Test.dll TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll ToDoModel.Test/bin/Debug/ToDoModel.Test.dll VBFunctions.Test/bin/Debug/VBFunctions.Test.dll /Settings:TaskMaster.runsettings /EnableCodeCoverage /InIsolation /ResultsDirectory:TestResults/remed-baseline`

EXIT_CODE: 0

Output Summary:

- Total tests: 4194. Passed: 4194. Failed: 0. Total time: ~50 s. (Build at current HEAD,
  before any remediation change.)
- All seven first-party test assemblies run together so the repository-wide coverage figure is
  comparable to the post-change run.

Numeric coverage (from `.coverage` merged to `TestResults/remed-baseline.cobertura.xml` via
`dotnet-coverage merge -f cobertura`):

- Raw overall Cobertura line-rate (all packages incl. third-party + vendored + test
  assemblies): 76.36%. (Recorded for traceability; not the policy metric.)
- First-party production-only line coverage (packages QuickFiler, Tags, TaskMaster,
  TaskVisualization, ToDoModel, UtilitiesCS, VBFunctions; deduped by file+line; excludes test
  assemblies and vendored SVGControl / Swordfish.NET.General): 75.12% (36436 / 48504).
- `TaskMaster.ApplicationGlobals` (primary class) line-rate: 77.63%.
- New-code recorder coverage: `TaskMaster.StartupTimingRecorder` line-rate 100%;
  `TaskMaster.NullStartupTimingRecorder` line-rate 100%. Meets the >= 90% new-code floor.

Baseline established: 4194 passing, EXIT_CODE 0, numeric coverage recorded (no placeholders).
