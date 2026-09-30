# Coordinator Tests Baseline (P0-T17)

Timestamp: 2026-09-30T13-25
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\944\p0-t17" "/Logger:trx;LogFileName=p0-t17.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (CMD-VSTEST, ASSEMBLY-TM, FILTER-COORD, NAMES-944; vstest resolved through vswhere)
EXIT_CODE: 0
Output Summary: VSTEST_EXIT_CODE: 0. TRX_PRESENT: True; SEQUENCE_FILES: 0. COUNTERS total=25 executed=25 passed=25 failed=0. The issue 942 test and the at-most-one-prime test passed; no RESULT line names any of the three new tests. BASELINE-FAILED: NONE.

## Observed

- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- BASELINE-COUNTERS: total=25 executed=25 passed=25 failed=0
- BASELINE-TOTAL: 25
- RESULT_COUNT: 25

RESULT lines (NAMES-944 members present in the trx):

```
RESULT GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker = Passed
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
RESULT GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse = Passed
RESULT GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime = Passed
```

No RESULT line names GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns, GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime or GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime.

BASELINE-FAILED: NONE

Verdict: the issue 942 test is compiled into the assembly and green at the anchor; no UPSTREAM 942 TEST NOT GREEN AT ANCHOR.
