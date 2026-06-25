Timestamp: 2026-06-24T20:05:00-04:00

Command:
`C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe collect --output docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\qa-gates\remediation-final-coverage.2026-06-24T19-23.xml --output-format xml --settings coverage.config -- C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`

EXIT_CODE: 0

Output Summary:
- Final supported coverage run passed.
- Total tests: 4178.
- Passed: 4178.
- Failed: 0.
- Repository line coverage: 82.98% (99577 covered-or-partial lines / 120000 total lines) across the repository's 10 counted modules.
- Issue-scoped folder tree/cache coverage: 92.54% (657/710 ranges, files=11).
- Issue-scoped EmailDataMiner folder extraction coverage: 94.52% (138/146 ranges).
- Issue-scoped FilterOlFolders snapshot coverage: 100.00% (53/53 ranges, lines 227-296).
- Issue-scoped SubjectMap orchestration coverage: 94.05% (79/84 ranges).

Additional Coverage Attempts:
- `dotnet-coverage collect ... vstest.console.exe ... /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook" /ResultsDirectory:...remediation-final-coverage-results-pass` passed 4178/4178 tests but produced a direct XML file with skipped modules only and no convertible `.coverage` attachment in the results directory.
- `vstest.console.exe ... /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook" /ResultsDirectory:...remediation-final-vstest-coverage-results` passed 4178/4178 tests but produced no `.coverage` attachment in the results directory.
- `vstest.console.exe ... /Collect:"Code Coverage" /TestCaseFilter:"TestCategory!=LiveOutlook" /ResultsDirectory:...remediation-final-vstest-collect-coverage-results` passed 4178/4178 tests but produced no `.coverage` attachment in the results directory.
- The final passing evidence uses the repository-supported wrapper method from `scripts/vscode/Invoke-MSTestWithCoverage.ps1`: outer `dotnet-coverage --settings coverage.config` instrumentation plus inner VSTest MSTest runsettings.

Coverage XML:
- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/remediation-final-coverage.2026-06-24T19-23.xml`

Result:
- P4-T4 PASS.
