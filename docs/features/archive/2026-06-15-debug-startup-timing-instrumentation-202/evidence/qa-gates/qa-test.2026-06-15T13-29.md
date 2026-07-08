# QA Gate — Test + Coverage (Issue #202, P2-T4)

Timestamp: 2026-06-15T13-29

Command: `vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll QuickFiler.Test/bin/Debug/QuickFiler.Test.dll Tags.Test/bin/Debug/Tags.Test.dll TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll ToDoModel.Test/bin/Debug/ToDoModel.Test.dll VBFunctions.Test/bin/Debug/VBFunctions.Test.dll /Settings:TaskMaster.runsettings /EnableCodeCoverage /InIsolation /ResultsDirectory:TestResults/remed-final`

EXIT_CODE: 0

Output Summary:

- Total tests: 4194. Passed: 4194. Failed: 0. Total time: ~48 s. Test Run Successful. (>= 4194
  required; no test lost.)
- The four `[DoNotParallelize]` startup-timing wiring tests all passed under the new class
  `ApplicationGlobalsStartupTimingTests`:
  - `LoadAsync_WhenTimingDisabled_RecordsNothingAndEmitsNoTable` — Passed
  - `LoadAsync_WhenTimingEnabled_RecordsAllPhasesInStartupOrderWithLoadBasicFirst` — Passed
  - `LoadAsync_WhenTimingEnabled_EmitsExactlyOneTableWithPhaseNamesAndTotal` — Passed
  - `LoadAsync_PreservesPhaseOrderingAndYieldCount_WhenTimingOnVersusOff` — Passed

Numeric coverage (`.coverage` merged to `TestResults/remed-final.cobertura.xml`):

- Raw overall Cobertura line-rate: 76.37% (baseline 76.36%). Delta +0.01 (no regression).
- First-party production-only deduped line coverage (QuickFiler, Tags, TaskMaster,
  TaskVisualization, ToDoModel, UtilitiesCS, VBFunctions; excludes test + vendored):
  75.12% (36436 / 48504). Identical to baseline (36436 / 48504). No regression. >= 80% applies
  to the exempt-adjusted testable denominator; this raw first-party figure is unchanged.
- `TaskMaster.ApplicationGlobals` (primary class) line-rate: 77.63%. Identical to baseline.
- New-code recorder coverage: `TaskMaster.StartupTimingRecorder` 100%;
  `TaskMaster.NullStartupTimingRecorder` 100%. New-code floor (>= 90%) preserved.

A pure move/split did not reduce coverage; all figures equal baseline within rounding.
