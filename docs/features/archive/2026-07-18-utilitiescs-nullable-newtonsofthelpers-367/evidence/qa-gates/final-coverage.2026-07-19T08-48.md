# Final Coverage Gate (P9-T4)

- Timestamp: 2026-07-19T08-48
- Command: `dotnet-coverage collect --output docs/features/active/2026-07-18-utilitiescs-nullable-newtonsofthelpers-367/evidence/qa-gates/final-coverage.2026-07-19T08-48.cobertura.xml --output-format cobertura --settings coverage.config -- "<VS18>/vstest.console.exe" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
- EXIT_CODE: 0
- Output Summary: `Test Run Successful. Total tests: 4511, Passed: 4511, Failed: 0`. All UtilitiesCS tests green (including `Threading/AppGlobalsConverterTests.cs`, `HelperClasses/NLogTraceWriter_Test.cs`, and the People/converter/wrapper/SDIL suites). Cobertura written to the qa-gates evidence folder.

## Numeric post-change coverage (Cobertura root)

- Overall line-rate: 0.7207039 (72.07%) — lines-covered 98286 / lines-valid 136375
- Overall branch-rate: 0.4845462 (48.45%) — branches-covered 12291 / branches-valid 25366

## Targeted UtilitiesCS/NewtonsoftHelpers production coverage (19 files, dedup)

- Post-change line-rate: 0.9381 (93.81%) — dedup lines covered 1893 / valid 2018

Same scope/method as the P0-T5 baseline (`UtilitiesCS.Test` via `dotnet-coverage` with `coverage.config` excludes; the plan's `Invoke-MSTestWithCoverage.ps1` has a single-assembly `.Count` StrictMode bug, so the equivalent direct invocation is used). No files changed by this step. Delta analysis in `coverage-delta.2026-07-19T08-48.md`.
