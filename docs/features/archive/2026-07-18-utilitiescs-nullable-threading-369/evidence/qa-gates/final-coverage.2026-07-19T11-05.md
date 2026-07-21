# Final QC — Coverage-Enabled Test Gate

- Timestamp: 2026-07-19T11-05
- Task: [P9-T4]
- Planned Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-threading-369/evidence/qa-gates/final-coverage.2026-07-19T11-05.cobertura.xml`
- Executed Command (equivalent mechanism, method-consistent with the P0-T5 baseline): `dotnet-coverage collect --output docs/features/active/2026-07-18-utilitiescs-nullable-threading-369/evidence/qa-gates/final-coverage.2026-07-19T11-05.cobertura.xml --output-format cobertura --settings coverage.config -- "<VS18 vstest.console.exe>" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /Settings:<Workers=4 runsettings> /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
- EXIT_CODE: 0
- Cobertura XML: `docs/features/active/2026-07-18-utilitiescs-nullable-threading-369/evidence/qa-gates/final-coverage.2026-07-19T11-05.cobertura.xml`

## Output Summary

- Tests: **4511 passed, 0 failed** (deterministic, Workers=4). All UtilitiesCS tests green.
- Overall (Cobertura root `<coverage>`): **line-rate = 0.7206925124474786 (72.07%)**, **branch-rate = 0.48442797445399355 (48.44%)**; lines-covered=98283, lines-valid=136373; branches-covered=12288, branches-valid=25366.
- Targeted production `UtilitiesCS/Threading/` (25 production files incl. Designer partials): **lines covered=3890, valid=4748, line-rate=0.8193 (81.93%)**.

## Methodology Note

Same deviation/rationale as the P0-T5 baseline artifact: the named `Invoke-MSTestWithCoverage.ps1` throws on any flaky vstest failure before emitting the Cobertura XML and runs the full suite at `Workers=0` (documented timing flakiness). The coverage mechanism it wraps (`dotnet-coverage collect ... --settings coverage.config`) was invoked directly against `UtilitiesCS.Test.dll` at `Workers=4`, identical method to the baseline, so the P9-T6 delta comparison is method-consistent. No assertions weakened; no retries/sleeps.
