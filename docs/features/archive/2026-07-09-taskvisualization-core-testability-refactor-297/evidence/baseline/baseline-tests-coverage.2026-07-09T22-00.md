# Baseline — Tests + Coverage (P0-T10)

Timestamp: 2026-07-09T22-00
Command: vstest.console.exe TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll /EnableCodeCoverage /InIsolation
        (numeric coverage captured via a second run with /Settings:TaskVisualization.Test/coverage.runsettings, Cobertura format, honoring [ExcludeFromCodeCoverage])
EXIT_CODE: 0

Output Summary:
- Total tests: 1, Passed: 1 (Disabled_GetFlagsToSet_TestMultiple — a `Z.Disabled`
  namespace placeholder that exercises no production code).
- Baseline TaskVisualization production line coverage: 0.00% (0 of 0 measured lines
  loaded). The single disabled test instantiates no production type, so the
  `TaskVisualization.dll` module is never loaded — the Cobertura report contains an
  empty `<packages />` set ("No code coverage data available. Profiler was not
  initialized.").
- `TaskController` currently carries a class-level `[ExcludeFromCodeCoverage]` (source
  line 20), so all 1861 lines of the controller are excluded from the coverage
  denominator today.
- Raw Cobertura output copied to `artifacts/csharp/coverage.xml`.

Numeric baseline coverage percent for TaskVisualization: 0.00%.
