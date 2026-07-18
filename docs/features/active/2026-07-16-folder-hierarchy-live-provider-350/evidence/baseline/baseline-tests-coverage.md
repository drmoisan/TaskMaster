# Baseline — Tests and Coverage (Toolchain Step 4)

Timestamp: 2026-07-18T00-14

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage` (executed as `vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:cov-utilitiescs.runsettings /InIsolation`; the runsettings enables the Microsoft Code Coverage data collector in Cobertura format, scoped to `UtilitiesCS.dll`, with the repo-standard Deedle/FSharp/vendored module excludes and MSTest Workers=4 for deterministic timing per the established UtilitiesCS.Test coverage pattern)

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 4321; Passed: 4321; Failed: 0; Total time: 24.9 s.
- Baseline coverage (production assembly UtilitiesCS.dll, first-party):
  - Line coverage: 88.49% (35834 / 40496 lines).
  - Branch coverage: 82.21% (8257 / 10044 branches).
- Coverage report: Cobertura (`DanMoisan_MEGALODON4_2026-07-18.08_04_07.cobertura.xml`). Single package `UtilitiesCS`.
- Note: the collector emitted a transient "Profiler was not initialized" message from an isolated worker; the merged Cobertura attachment was nonetheless produced with the numeric rates above.
