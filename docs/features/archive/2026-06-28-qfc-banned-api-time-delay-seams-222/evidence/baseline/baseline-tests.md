# Baseline — Tests with Coverage (P0-T9)

Timestamp: 2026-06-28T19-23
Command: vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:TaskMaster.runsettings /InIsolation /EnableCodeCoverage
EXIT_CODE: 0
(Coverage extracted via: dotnet-coverage merge <.coverage> --output cobertura)

Output Summary:
- Test Run Successful. Total tests: 181, Passed: 181, Failed: 0. Total time ~4.1s.
- /InIsolation used because QuickFiler.Test uses Moq (per repo runtime requirement; avoids STTE Setup FileNotFound).
- /Settings runsettings applies module excludes (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest).

Numeric baseline coverage values (this QuickFiler.Test-only run):
- Single-run overall cobertura line-rate: 0.1145 (11.46%). NOTE: this is NOT a true repo-wide figure — only QuickFiler.Test executed, so the denominator includes incidentally-loaded modules (UtilitiesCS, System.Linq.Async, log4net) that are exercised by other test assemblies in a full run. Recorded for baseline-vs-final consistency only.
- QuickFiler assembly (package) line-rate: 0.30952 (30.95%).
- QfcHomeController.cs class line-rate: 0.9018 (90.18%).
- QfcHomeController.Metrics.cs class line-rate: 0.5493 (54.93%) — the four DateTime.Now sites and the 20 ms delay catch-branch live here; this is the figure the new tests must improve / not regress.
- QfcHomeController.Iteration.cs: 1.0.
- QfcDatamodel: NOT present in coverage output, confirming its class-level [ExcludeFromCodeCoverage] exemption (its delay-site tests are correctness-only).

Coverage comparison authority: P5-T5 compares these baselines to the post-change run (P5-T4) using the identical command.
