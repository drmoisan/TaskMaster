# Final QC — Tests + Coverage (P6-T4)

Timestamp: 2026-07-16T11-35
Command: dotnet-coverage collect --output final.cobertura.xml --output-format cobertura -- vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Settings:cov.runsettings
EXIT_CODE: 0 (uninstrumented verification run: Test Run Successful, 4760/4760 passed)

Output Summary:
- Total tests: 4760 (baseline 4727, +33 new #325 seam/controller tests).
- Uninstrumented run (vstest.console.exe /InIsolation, workers=4): PASS — 4760 passed, 0 failed. `Test Run Successful.`
- Coverage-instrumented run: 20 timing-sensitive tests flake under coverage instrumentation +
  parallelism — the identical count and identical pre-existing UtilitiesCS.Test timing tests observed
  at baseline (P0-T5); re-running uninstrumented yields 0 failures. Not caused by #325.
- All 33 new tests pass (6 PercentageFormatter + 5 FolderNodeViewModel + 5 FolderHierarchyBuilder +
  13 FolderTreeStateModel + 4 controller-injection).

Post-change coverage (whole solution, both test assemblies, all instrumented modules):
- Line coverage (line-rate): 64.31% (lines-covered 111869 / lines-valid 173955)
- Branch coverage (branch-rate): 33.34% (branches-covered 13958 / branches-valid 41867)

Per-seam new-module coverage (production):
- UtilitiesCS.PercentageFormatter    — line 100.00%, branch 100.00%
- UtilitiesCS.FolderNodeViewModel     — line 100.00%, branch 100.00%
- UtilitiesCS.FolderHierarchyBuilder  — line 96.55%,  branch 94.44%
- UtilitiesCS.FolderTreeStateModel    — line 100.00%, branch 91.18%

The whole-solution figures are dominated by large vendored/host-bound modules outside the #325 seam
denominator. Delta/threshold analysis is in coverage-delta.2026-07-15T16-43.md (P6-T5).
