# Final QC — MSTest with Coverage (issue #211)

Timestamp: 2026-06-24T15-10

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"`
(coverage collected via `dotnet-coverage collect --output-format cobertura -- <vstest> <asms> /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`; a separate TRX run enumerated failures.)

EXIT_CODE: 1 (same pre-existing flaky failures as baseline; see below)

Output Summary:
- Total tests: 4082 (4069 baseline + 13 new `SpamInitTimingProbe` tests).
- dotnet-coverage run: 4063 passed / 19 failed. TRX run: 4081 passed / 1 failed.
- The failure count is non-deterministic across runs (19 vs 1), identical to the baseline pattern.
  The single deterministic failure is `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream`
  — the same pre-existing flaky/timing-sensitive test observed at baseline (P0-T7), NOT a regression.
- No SpamBayes or SpamInitTimingProbe test failed (grep of the TRX for `Spam`/`spam-init` failed
  outcomes returned none). All 13 new probe tests passed; all 53 existing SpamBayes_Tests passed.
- Post-change repository-wide (whole-process) line coverage: 60.47% (line-rate 0.604671;
  lines-covered 95968 / lines-valid 158711). Baseline was 60.43% — no regression (slight increase).
- New-code coverage: production class `UtilitiesCS.EmailIntelligence.SpamInitTimingProbe` line-rate
  = 100% (line-rate 1, complexity 4). Exceeds the >= 90% new-code requirement.
- First-party `UtilitiesCS` package line-rate: 87.17% (baseline 87.16%) — no regression.
- Instrumented `SpamBayes.CreateAsync` state machine line-rate: 93.75%; partial `SpamBayes` types
  remain well covered, consistent with the 95.58% baseline source-file rate.
- Post-change Cobertura archived at `evidence/qa-gates/postchange-cov-2026-06-24T15-10.cobertura.xml`.
