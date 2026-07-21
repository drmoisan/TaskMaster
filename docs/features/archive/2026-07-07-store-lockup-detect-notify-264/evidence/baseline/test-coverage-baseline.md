# Pre-Change Test + Coverage Baseline (P0-T9)

Timestamp: 2026-07-08T07-59

Command (semantic equivalent of the plan's `/EnableCodeCoverage` in Cobertura output mode):
`vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /Settings:<cobertura.runsettings> /TestCaseFilter:"TestCategory!=LiveOutlook"`

Notes on command:
- The Code Coverage DataCollector is enabled via the runsettings (`<DataCollector friendlyName="Code Coverage">` with `<Format>Cobertura</Format>`), which is the `/EnableCodeCoverage` mechanism producing an offline-parseable Cobertura XML rather than the raw binary `.coverage` (not reliably convertible in this environment). Module excludes mirror the repo `TaskMaster.runsettings` (Deedle/FSharp/Castle/FluentAssertions/Moq/MSTest/Microsoft.Testing) plus `*.Test.dll`.
- `/InIsolation` is required for the Moq-based test assemblies in this environment.

EXIT_CODE: 0

Output Summary:
- Total tests: 4441. Passed: 4441. Failed: 0. Total time: 41.86 s.
- Coverage (Cobertura root): line-rate = 0.56511 => 56.51% (lines-covered=40604, lines-valid=71851); branch-rate = 0.47306 => 47.31%.
- First-party package line-rates (the apples-to-apples baseline for F4's no-regression check):
  - UtilitiesCS: 0.88248 => 88.25% (F4's UtilitiesCS classes live here)
  - TaskMaster: 0.66532 => 66.53% (F4's AppOlObjects wrap + ExcludeFromCodeCoverage ThisAddIn wiring live here)
- The raw overall 56.51% is deflated by (a) first-party assemblies whose tests are not in this
  two-DLL run (QuickFiler 0%, ToDoModel 2.25%, Tags 0%, TaskVisualization 18.31%) and (b) vendored
  packages (Swordfish 46.53%, SVGControl 16.22%, System.Interactive, System.Linq.Async,
  Mono.Reflection, log4net). The testable-denominator 80% floor (CLAUDE.md) is evaluated on
  first-party production code; UtilitiesCS is already 88.25%. Post-change comparison at P9-T4/T5
  uses the identical command and Cobertura methodology.
