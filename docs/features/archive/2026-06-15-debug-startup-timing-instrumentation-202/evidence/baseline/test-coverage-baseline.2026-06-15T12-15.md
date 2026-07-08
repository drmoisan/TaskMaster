# Phase 0 — MSTest Test + Coverage Baseline (Issue #202)

Timestamp: 2026-06-15T12-15

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll /Settings:TaskMaster.runsettings /EnableCodeCoverage /InIsolation /ResultsDirectory:TestResults/baseline-full`

EXIT_CODE: 0

Output Summary:

- Total tests: 4183. Passed: 4183. Failed: 0.
- The plan (P0-T5) requires "at minimum the TaskMaster.Test assembly". All seven first-party
  test assemblies were run together so the repository-wide line-coverage figure is meaningful
  and directly comparable to the Phase 5 post-change run. (A TaskMaster.Test-only run was also
  executed and passed: 91/91.)
- `/InIsolation` was used because the Moq-based test assemblies require the isolated test host
  (prior incident: STTE Setup FileNotFound without `/InIsolation`).

Numeric coverage (from merged Cobertura `TestResults/baseline-full.cobertura.xml`,
computed with `scripts/temp-cov-202.ps1` / `scripts/temp-cov-file-202.ps1`):

- Raw overall Cobertura line-rate (all packages incl. third-party + vendored + test assemblies):
  76.30% (lines-covered 96957 / lines-valid 127076). This raw figure is not the policy metric;
  it is recorded for traceability only.
- First-party production-only line coverage (packages QuickFiler, Tags, TaskMaster,
  TaskVisualization, ToDoModel, UtilitiesCS, VBFunctions; deduped by file+line; excludes test
  assemblies and the vendored SVGControl / Swordfish.NET.General packages): 75.08%
  (36372 / 48447 lines). Note: this denominator still INCLUDES the COM/VSTO/WinForms-bound and
  `[ExcludeFromCodeCoverage]`-exempt classes that CLAUDE.md formally exempts from the 80% floor;
  the policy floor applies to the testable denominator after those exemptions. This baseline
  figure is recorded as the comparison point for no-regression on changed lines.
- `ApplicationGlobals` primary class line-rate: 74.24% (Cobertura `line-rate` on
  `TaskMaster.ApplicationGlobals`).
- `ApplicationGlobals.cs` aggregate line coverage (primary class + compiler-generated async
  state machines in the same file, deduped by line): 60.75% (65 / 107 lines).

All coverage values are numeric (no placeholders). Baseline test state: PASS.
