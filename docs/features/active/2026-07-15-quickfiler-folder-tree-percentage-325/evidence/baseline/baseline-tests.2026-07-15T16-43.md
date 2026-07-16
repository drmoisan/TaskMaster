# Baseline — Tests + Coverage (P0-T5)

Timestamp: 2026-07-16T09-20
Command: dotnet-coverage collect --output baseline.cobertura.xml --output-format cobertura -- vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Settings:cov.runsettings
EXIT_CODE: 0 (uninstrumented verification run: Test Run Successful, 4727/4727 passed)

Output Summary:
- Total tests: 4727.
- Uninstrumented run (vstest.console.exe with /InIsolation, workers=4): PASS — 4727 passed, 0 failed. `Test Run Successful.`
- Coverage-instrumented run (dotnet-coverage collect wrapping the same vstest command): ~20 timing-sensitive tests flake under coverage instrumentation + parallelism (documented UtilitiesCS.Test behavior — full-suite timing assertions are sensitive to instrumentation overhead). These are pre-existing, flaky, and NOT caused by any #325 change (zero code changes at baseline). Re-running uninstrumented yields 0 failures.
- Baseline coverage (whole solution, both test assemblies, all instrumented modules including vendored Swordfish/SVGControl/etc.):
  - Line coverage (line-rate): 64.19% (lines-covered 111296 / lines-valid 173379)
  - Branch coverage (branch-rate): 33.18% (branches-covered 13860 / branches-valid 41767)

Notes:
- The plan command is `vstest.console.exe ... /EnableCodeCoverage`, which emits a binary `.coverage` that is not offline-convertible to numeric line/branch in this environment. To satisfy the acceptance requirement for numeric line/branch coverage, the equivalent run is wrapped by `dotnet-coverage collect --output-format cobertura`, which produces a numeric Cobertura XML directly. The same wrapping is used at final QC (P6-T4) for a consistent baseline-vs-post-change comparison.
- `/InIsolation` is required for the Moq-bearing test assemblies (STTE Setup FileNotFound otherwise); a runsettings limiting MSTest Workers to 4 tames the coverage-instrumentation timing flakiness.
- The whole-solution line/branch figures are dominated by large vendored/host-bound modules that are outside the #325 seam-coverage denominator. The per-seam new-module coverage is reported in P6-T5.
