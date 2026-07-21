# Baseline UtilitiesCS.Test Run With Coverage (P0-T3)

Timestamp: 2026-07-19T10-54

Command: `pwsh -NoProfile ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot 'UtilitiesCS.Test' -CoverageOutput 'docs/features/active/2026-07-18-utilitiescs-nullable-residuals-375/evidence/baseline/coverage-baseline.cobertura.xml'`
(wraps `dotnet-coverage collect --output-format cobertura --settings coverage.config -- <vstest.console.exe> UtilitiesCS.Test.dll /Settings:TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`)

EXIT_CODE: 0

Output Summary:
- Test result: Test Run Successful.
- Total tests: 4511; Passed: 4511; Failed: 0; Skipped: 0.
- Total time: 26.16 s.
- Coverage (Cobertura root aggregate): line-rate 0.65299 (65.30%), branch-rate 0.613274 (61.33%);
  lines-covered 67625 / lines-valid 103562; branches-covered 15690 / branches-valid 25584. The
  aggregate is diluted by sibling assemblies instrumented at 0% (ToDoModel, QuickFiler, TaskTree,
  TaskVisualization, Tags) because only UtilitiesCS.Test executes here.
- Coverage (UtilitiesCS assembly package — the assembly under test and the one this child edits):
  line-rate 0.8874674813 (88.75%), branch-rate 0.8251334859 (82.51%).
- Coverage artifact: docs/features/active/2026-07-18-utilitiescs-nullable-residuals-375/evidence/baseline/coverage-baseline.cobertura.xml

This establishes the pre-edit reference (AC6). No pre-existing test failure exists, so no failure can
be attributed to this child. The edits in this child are annotation-only (`#nullable enable` pragma,
`?`, `= null!`, `!`); they add no executable runtime lines and are expected to be coverage-neutral.
