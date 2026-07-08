# Baseline MSTest Coverage — UtilitiesCS.Test + QuickFiler.Test (Issue #269)

- Timestamp: 2026-07-08T09-45
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation` (run via `MSYS_NO_PATHCONV=1` under git-bash; `/InIsolation` added per `.claude/agent-memory/atomic-executor/project_vstest_isolation_and_filepathhelper_serialization.md` to avoid a Moq/STTE setup failure. vstest.console.exe path: `C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe`.)
- EXIT_CODE: 0

## Output Summary

`Test Run Successful. Total tests: 4662. Passed: 4662. Total time: 50.0639 Seconds.` No pre-existing flaky failure was observed in this run (the previously-documented `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` flaky test passed).

Coverage `.coverage` output converted to Cobertura via `dotnet-coverage merge -f cobertura` (raw XML retained at `evidence/baseline/coverage-baseline.cobertura.xml`):

- Whole-process line coverage (all loaded modules, including vendored/third-party): 65.73% (112696/171461 lines).
- `QuickFiler` package (first-party, production): 72.51% line rate.
- `QuickFiler.Test` package: 95.19% line rate.
- `UtilitiesCS` package (first-party, production): 88.21% line rate.
- `UtilitiesCS.Test` package: 97.75% line rate.
- Class `UtilitiesCS.Theme` (`Theme.Rendering.cs` partial): 54.05% line rate (baseline, pre-fix).
- Class `UtilitiesCS.Theme` (`Theme.cs` partial): 66.95% line rate (baseline, pre-fix).
- Class `QuickFiler.QfcThemeHelper`: 96.45% line rate (baseline, pre-fix).

These are the baseline coverage figures against which the P2-T5 comparison task will measure post-change coverage and confirm no regression.
