# Baseline: coordinator fixture run (issue 942)

Timestamp: 2026-09-30T07-28
Task: P0-T12
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\942\p0-t12" "/Logger:trx;LogFileName=p0-t12.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

Output Summary:
- vstest.console.exe resolved through vswhere; the trx stays under the ignored coverage directory.
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=24 executed=24 passed=24 failed=0
- RESULT_COUNT: 24
- RESULT GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker = Passed
- RESULT GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse = Passed
- RESULT GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked = Passed
- No RESULT line names GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged (the test does not exist yet).
- No FAILED or MESSAGE line was printed.

BASELINE-COUNTERS: total=24 executed=24 passed=24 failed=0
BASELINE-TOTAL: 24
BASELINE-FAILED: NONE
