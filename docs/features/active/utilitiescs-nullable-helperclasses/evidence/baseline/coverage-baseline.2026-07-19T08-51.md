# Coverage — Baseline (Issue #364)

- Timestamp: 2026-07-19T08-51
- Task: [P0-T5]
- Command (plan-literal): `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-helperclasses/evidence/baseline/coverage-baseline.2026-07-19T08-51.cobertura.xml`
- Command (actually executed): the script's underlying dotnet-coverage command, scoped to `UtilitiesCS.Test`:
  `dotnet-coverage collect --output <out.cobertura.xml> --output-format cobertura --settings coverage.config -- <vstest.console.exe> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
- EXIT_CODE: 0
- Cobertura XML: `docs/features/active/utilitiescs-nullable-helperclasses/evidence/baseline/coverage-baseline.2026-07-19T08-51.cobertura.xml`

## Invocation Note (single-assembly scope)

`Invoke-MSTestWithCoverage.ps1` throws under `Set-StrictMode -Version Latest` when test-assembly discovery returns a single result (`$testAssemblies.Count` on a scalar). The script is written for the multi-assembly full-repo case. To scope coverage deterministically to this feature's assembly (`UtilitiesCS.Test`, matching the plan text "the UtilitiesCS test assemblies" and every per-batch test command) and avoid the known full-suite Deedle/FSharp instrumentation and parallelism flakiness, the script's exact underlying dotnet-coverage command was invoked directly against `UtilitiesCS.Test.dll` with the same `coverage.config`, `TaskMaster.cli.runsettings`, `/InIsolation`, and `TestCategory!=LiveOutlook` filter. The same invocation will be used for the P9-T4 final coverage so the P9-T6 delta is comparable. Koverage path-rewrite post-processing was not applied; it does not affect line-rate/branch-rate.

## Output Summary

- Test result: PASS. Total tests: 4511; Passed: 4511; Failed: 0. Total time ~24.8s.
- Cobertura root `<coverage>` element (all DLLs instrumented during the UtilitiesCS.Test run):
  - line-rate: 0.7206858366517795 (72.07%); lines-covered 98272 / lines-valid 136359
  - branch-rate: 0.4845462430024442 (48.45%); branches-covered 12291 / branches-valid 25366
- Targeted `UtilitiesCS/HelperClasses/` (classes whose filename matches `HelperClasses`):
  - 263 classes; 9763 lines; 8989 hit
  - HelperClasses line coverage: 92.07%
- This HelperClasses figure (92.07%) is the reference for the P9-T6 changed-line no-regression check. Annotation-only edits are expected not to move covered-line counts materially.
