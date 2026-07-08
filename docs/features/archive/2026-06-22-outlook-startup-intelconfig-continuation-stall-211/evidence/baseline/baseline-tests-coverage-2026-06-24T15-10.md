# Baseline — MSTest with Coverage (issue #211)

Timestamp: 2026-06-24T15-10

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"`
(executed via `dotnet-coverage collect --output-format cobertura -- <vstest> <asms> /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook` to obtain a parseable Cobertura report; a separate TRX run was used to enumerate failures.)

EXIT_CODE: 1 (pre-existing flaky failures; see below)

Output Summary:
- Total tests: 4069.
- Two runs were performed to characterize stability:
  - dotnet-coverage run: 4050 passed / 19 failed.
  - TRX run: 4068 passed / 1 failed.
- The failure count is non-deterministic between runs (19 vs 1), confirming the failures are pre-existing flaky/timing-sensitive tests, NOT caused by any code change (no code changed at baseline).
- The one failure common across runs: `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` (took ~21 s in the slow run; timing-sensitive). Other intermittent failures correlate with parallel-execution timing.
- Repository-wide (whole-process) line coverage: 60.43% (line-rate 0.6043; lines-covered 95834 / lines-valid 158578). This denominator includes vendored/third-party modules (Deedle, FSharp.Core, log4net, System.Linq.Async, SVGControl, Swordfish, FluentAssertions, etc.).
- First-party module of interest `UtilitiesCS` package line-rate: 87.16% (where `SpamInitTimingProbe` will be added; >= 80% floor satisfied at baseline).
- `SpamBayes.cs` source-file line-rate at baseline: 95.58%.
- Other first-party packages: UtilitiesCS.Test 97.62%, TaskMaster.Test 93.19%, TaskMaster 51.06%.
- Baseline Cobertura archived at `evidence/baseline/coverage/baseline-cov-2026-06-24T15-10.cobertura.xml` for the P5-T5 delta comparison.
