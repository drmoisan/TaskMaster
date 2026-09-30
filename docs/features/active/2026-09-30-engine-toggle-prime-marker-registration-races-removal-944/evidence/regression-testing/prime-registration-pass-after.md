# Prime Registration Pass-After (P2-T4)

Timestamp: 2026-09-30T13-36
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\944\p2-t4" "/Logger:trx;LogFileName=p2-t4.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (CMD-VSTEST, ASSEMBLY-TM, FILTER-COORD, NAMES-944; vstest resolved through vswhere)
EXIT_CODE: 0
Output Summary:
VSTEST_EXIT_CODE: 0 (13-35-59 to 13-36-01 UTC); TRX_PRESENT: True; SEQUENCE_FILES: 0
COUNTERS total=28 executed=28 passed=28 failed=0
RESULT_COUNT: 28
RESULT GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime = Passed
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
RESULT GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse = Passed
RESULT GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns = Passed
RESULT GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker = Passed
RESULT GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime = Passed
No FAILED line.
The only difference between this run and P1-T4 is the production edit E1 to E3 in TaskMaster/Ribbon/EngineToggleStateCoordinator.cs; the new partial and its compile entry were present in both runs.
PROD-HASH-AFTER: B3C6FEB2A86E36E95AC34F6108D87C8E117A94949F6FCE0B3AF26D824D6E3086 (differs from ANCHOR-HASH-PROD: D9C915AE9B00BB7AAB80183A7A0BA11748DE393781D7E5ADE2BDE29073B7002B)

## Verification against the acceptance

- EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS failed 0; total 28 = BASELINE-TOTAL 25 plus 3
- All seven NAMES-944 names have a RESULT line reading Passed
- No FAILED line
- The PrimeRegistration partial hash (CMD-HASH) is 6D28EBF80B5D4AE7C3C0099A8A329F7B8DF463E4258FF1B14D921CEB12C91A7C, identical to the value recorded at P1-T4, so the test side is unchanged between the two runs.

## POPULATION-COMPARISON:

P2-T5, Timestamp: 2026-09-30T13-37. Sources: evidence/baseline/coordinator-tests-baseline.md (P0-T17), evidence/regression-testing/prime-registration-fail-before.md (P1-T4) and this artifact (P2-T4).

- Baseline (P0-T17): total=25 executed=25 passed=25 failed=0; BASELINE-TOTAL: 25; BASELINE-FAILED: NONE.
- Fail-before (P1-T4): total=28 executed=28 passed=27 failed=1; FAILED GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns.
- Pass-after (P2-T4): total=28 executed=28 passed=28 failed=0.
- The pass-after total (28) equals the fail-before total (28) and equals BASELINE-TOTAL (25) plus 3.
- The pass-after failed count is 0.
- BASELINE-FAILED is NONE, so no baseline name needs to be matched.
- The single fail-before FAILED name, GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns, appears as Passed in the pass-after run.
- No name is still failing; PASS-AFTER NOT GREEN does not apply.

## FINAL-FIXTURE-RUN:

P3-T7, pass 1, Timestamp: 2026-09-30T13-48. Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\944\p3-t7" "/Logger:trx;LogFileName=p3-t7.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (CMD-VSTEST, ASSEMBLY-TM, FILTER-COORD, NAMES-944), run on the assembly rebuilt by the P3-T6 nullable rebuild from the committed tree (edc5c3af2). EXIT_CODE: 0.

Output Summary:
VSTEST_EXIT_CODE: 0 (13-48-18 to 13-48-22 UTC); TRX_PRESENT: True; SEQUENCE_FILES: 0
COUNTERS total=28 executed=28 passed=28 failed=0 (failed 0; total 28 = BASELINE-TOTAL 25 plus 3)
RESULT_COUNT: 28
RESULT GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker = Passed
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
RESULT GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse = Passed
RESULT GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns = Passed
RESULT GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime = Passed
RESULT GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime = Passed
No FAILED line. All seven NAMES-944 names Passed. Every P3-T7 clause holds.
