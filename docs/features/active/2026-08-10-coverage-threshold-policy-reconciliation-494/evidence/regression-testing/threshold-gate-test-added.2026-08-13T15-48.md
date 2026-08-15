Timestamp: 2026-08-13T15-48
Command: `git diff -- tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`
EXIT_CODE: 0
Output Summary:

- Added five isolated in-memory Cobertura threshold cases: missing summary, non-numeric summary, below 80%, exactly 80%, and above 80%.
- Added one isolated main-entrypoint case with collection/conversion mocks registered before `Invoke-MSTestWithCoverageMain` executes; it requires the generated `0.8` Cobertura XML to reach the threshold evaluator.
- No test uses a temporary file, executable, network dependency, or ambient path.
- The helper test file is 498 lines and the runsettings test file is 459 lines before formatting.
