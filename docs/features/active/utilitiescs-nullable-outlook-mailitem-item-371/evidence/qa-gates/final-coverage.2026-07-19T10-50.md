# Final QC — Coverage-Enabled Test Gate (P10-T4)

- Timestamp: 2026-07-19T10-50
- Task: [P10-T4]
- Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot UtilitiesCS.Test -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/final-coverage.2026-07-19T10-50.cobertura.xml`
  - Same scope as the P0-T5 baseline (UtilitiesCS.Test, the assembly every batch test task targets) for an apples-to-apples delta. `coverage.config` module excludes handle the Deedle/FSharp instrumentation flakiness.
- EXIT_CODE: 0
- Cobertura XML: `evidence/qa-gates/final-coverage.2026-07-19T10-50.cobertura.xml`

## Output Summary

- Tests: Total **4511**, Passed **4511**, Failed **0** (identical count to the P0-T5 baseline — no tests added or removed, consistent with annotation-only work). Total time 24.19s. All UtilitiesCS tests green, including the legacy-named duplicate test files.
- Overall (Cobertura root `<coverage>`): line-rate **0.653005** (65.30%), branch-rate **0.612853** (61.29%); lines-covered 67633 / lines-valid 103572.
- Targeted in-scope `UtilitiesCS/OutlookObjects/` production line coverage (deduped, test files excluded): **87.07%** (3320 covered / 3813 valid) across the 28 files with executable lines.
