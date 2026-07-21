# Phase 0 — Test + Coverage Baseline (P0-T11)

Timestamp: 2026-07-08T03-51

Command (plan text): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`

Command (as executed, to obtain a numeric Cobertura percentage rather than a binary
`.coverage` blob): `dotnet-coverage collect --output coverage/utilitiescs-baseline.cobertura.xml --output-format cobertura --settings coverage.config -- <vstest.console.exe> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:<cov.runsettings Workers=4> /InIsolation`
(This is the repository's canonical numeric-coverage path — dotnet-coverage collect
wrapping vstest with coverage.config instrumentation excludes, `/InIsolation` for the Moq
assembly, and MSTest Workers lowered to 4 to avoid the documented coverage-instrumentation
timing flakiness in UtilitiesCS.Test.)

EXIT_CODE: 0

Output Summary:
- Test result: Total tests: 4223. Passed: 4223. Failed: 0. Skipped: 0.
- Raw Cobertura overall line-rate (dotnet-coverage instruments every loaded DLL, including
  vendored assemblies): 72.34% line, 48.63% branch. This raw figure is polluted by
  runtime-instrumented vendored/third-party packages (Swordfish.NET 46.53%, log4net 6.08%,
  System.Interactive 2.73%, SVGControl 16.28%, TaskMaster 9.38%, Tags 0%, Mono.Reflection
  39.3%, System.Linq.Async 3.84%, ToDoModel 0%, QuickFiler 0%) that the canonical runner's
  post-processing step normally strips. It is NOT the testable-denominator figure.
- First-party TESTABLE-DENOMINATOR baseline (the meaningful number for the >= 80% floor):
  UtilitiesCS package line-rate = 88.21% (well above the 80% floor). UtilitiesCS.Test
  package = 97.76%.
- This is the baseline the P7-T4/P7-T5 post-change comparison uses: first-party UtilitiesCS
  line coverage 88.21%.
