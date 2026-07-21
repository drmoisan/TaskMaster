# Final QC — Coverage Gate (Issue #364)

- Timestamp: 2026-07-19T10-07
- Task: [P9-T4]
- Command (plan-literal): `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/final-coverage.2026-07-19T10-07.cobertura.xml`
- Command (actually executed): the script's underlying dotnet-coverage command scoped to `UtilitiesCS.Test` (same invocation as P0-T5 baseline for a comparable delta; see P0-T5 for the single-assembly StrictMode workaround rationale):
  `dotnet-coverage collect --output <out.cobertura.xml> --output-format cobertura --settings coverage.config -- <vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
- EXIT_CODE: 0
- Cobertura XML: `docs/features/active/utilitiescs-nullable-helperclasses/evidence/qa-gates/final-coverage.2026-07-19T10-07.cobertura.xml`

## Output Summary

- Test result: PASS. Total tests: 4511; Passed: 4511; Failed: 0. Total time ~22.7s. All UtilitiesCS tests green.
- Cobertura root `<coverage>` element (post-change):
  - line-rate: 0.7207090961077427 (72.07%); lines-covered 98304 / lines-valid 136399
  - branch-rate: 0.4844673973034771 (48.45%); branches-covered 12289 / branches-valid 25366
- Targeted `UtilitiesCS/HelperClasses/` (classes whose filename matches `HelperClasses`):
  - 263 classes; 9803 lines; 9027 hit
  - HelperClasses line coverage: 92.08%
- Comparison to P0-T5 baseline (line 0.7206858 / branch 0.4845462 / HelperClasses 92.07%): overall line-rate and branch-rate are flat (delta within noise: +0.0002% line, -0.0079% branch), and HelperClasses line coverage rose slightly to 92.08%. No coverage regression from the annotation-only changes.
- No files changed by this step; the toolchain loop proceeds without restart.
