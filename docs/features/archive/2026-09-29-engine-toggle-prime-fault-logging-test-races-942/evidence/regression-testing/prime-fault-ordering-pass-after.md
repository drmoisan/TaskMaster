# Pass-after: prime fault ordering (issue 942)

Timestamp: 2026-09-30T07-39
Task: P2-T4 (creates this file); P2-T5 and P3-T7 append.
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\942\p2-t4" "/Logger:trx;LogFileName=p2-t4.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

Output Summary:
- Identical to the P1-T5 command except the task id segments (p2-t4); vstest.console.exe resolved through vswhere; the trx stays under the ignored coverage directory.
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=25 executed=25 passed=25 failed=0
- RESULT_COUNT: 25
- RESULT GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker = Passed
- RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
- RESULT GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked = Passed
- RESULT GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse = Passed
- No FAILED line was printed.
- The only production difference between this run and the P1-T5 fail-before run is the statement reorder and documentation in `CompletePrime` and `GetPrimeTask` of `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`. The Harness hook, the new partial and its csproj entry are test-side and were present in both runs.
- PROD-HASH-AFTER: D9C915AE9B00BB7AAB80183A7A0BA11748DE393781D7E5ADE2BDE29073B7002B (differs from BASE-HASH-PROD F2A961DD50F2E4678B5CF8B7FAA3F0316AA22D2FB8FE904AE5D08057F26ACEF0)

## POPULATION-COMPARISON:

Timestamp: 2026-09-30T07-39 (P2-T5; read from evidence/baseline/coordinator-tests-baseline.md and this file)

- BASELINE-TOTAL: 24; pass-after total: 25 = BASELINE-TOTAL plus 1 (the new test).
- Pass-after failed: 0.
- BASELINE-FAILED: NONE, so no baseline failure needs to be re-checked; no test is recorded as still failing.
- Result: the pass-after population is the baseline population plus the new regression test, all passed. No PASS-AFTER NOT GREEN.

## FINAL-FIXTURE-RUN:

Timestamp: 2026-09-30T07-48 (P3-T7, on the assembly rebuilt by the P3-T5 and P3-T6 gates)
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\942\p3-t7" "/Logger:trx;LogFileName=p3-t7.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=25 executed=25 passed=25 failed=0 (total = BASELINE-TOTAL plus 1)
- RESULT GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked = Passed
- RESULT GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse = Passed
- RESULT GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker = Passed
- RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
- No FAILED line.
