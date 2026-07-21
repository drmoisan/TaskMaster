# QC Tests With Coverage (P12-T4)

Timestamp: 2026-07-19T11-57

Command: `pwsh -NoProfile ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot 'UtilitiesCS.Test' -CoverageOutput 'docs/features/active/utilitiescs-nullable-residuals/evidence/qa-gates/coverage-postchange.cobertura.xml'`
(wraps `dotnet-coverage collect --output-format cobertura --settings coverage.config -- <vstest.console.exe> UtilitiesCS.Test.dll /Settings:TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`)

EXIT_CODE: 0

Output Summary:
- Test result: Test Run Successful.
- Total tests: 4511; Passed: 4511; Failed: 0; Skipped: 0 (identical to baseline; no regression).
- Coverage (Cobertura root aggregate): line-rate 0.653541 (65.35%), branch-rate 0.613274 (61.33%).
- Coverage (UtilitiesCS assembly package — the assembly this child edits): line-rate 0.8875250262 (88.75%),
  branch-rate 0.8251334859 (82.51%).
- Coverage artifact: docs/features/active/utilitiescs-nullable-residuals/evidence/qa-gates/coverage-postchange.cobertura.xml
- No files changed by this stage; the toolchain loop does not restart.
