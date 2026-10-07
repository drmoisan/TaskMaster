# P4-T9 MSTest test-result summary, final stage (final C# pass, iteration 1)

Timestamp: 2026-10-06T18-30
Command: dotnet-coverage collect --output coverage\final-973.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-973.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\973\final" "/Logger:trx;LogFileName=final-973.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (CMD-COVERAGE-DIRECT final), summary derived from coverage\final-973.trx by Get-TrxRunSummary and Format-TrxRunSummary (CMD-COVERAGE-POST final)
EXIT_CODE: 0
Output Summary: 7361 tests, 7361 executed, 7361 passed, 0 failed; outcome Completed; error, timeout, aborted, notExecuted and inconclusive all 0; no failed test.

COVERAGE-ROUTE: DIRECT (Planner Amendment 4)
ROUTE-FILTER: HelperClasses.ShellUtilities_Tests, HelperClasses.ShellUtilitiesStatic_Tests, HelperClasses.SysImageListHelperTests, EmailIntelligence.OSBrowser_Tests
CI-COVERS-EXCLUDED: .github/workflows/_mstest-coverage.yml

SUMMARY-BEGIN
Test run outcome: Completed
Total 7361, executed 7361, passed 7361, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END
FAILED-SET: (empty)
