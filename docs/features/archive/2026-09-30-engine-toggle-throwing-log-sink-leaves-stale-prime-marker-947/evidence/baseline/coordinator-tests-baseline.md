# Baseline: Coordinator Fixture Run (P0-T13)

Timestamp: 2026-10-01T17-40
Task: P0-T13
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\947\p0-t13" "/Logger:trx;LogFileName=p0-t13.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

Output Summary:
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- BASELINE-COUNTERS: total=28 executed=28 passed=28 failed=0
- BASELINE-TOTAL: 28
- RESULT_COUNT: 28
- Each of the eight pre-existing NAMES-947 entries (fifth to twelfth) reads Passed; no RESULT line names any of the four new names; no FAILED line.
- Result: P0-T13 acceptance holds; no BASELINE NOT GREEN.

## Transcribed lines

```
COUNTERS total=28 executed=28 passed=28 failed=0
RESULT_COUNT: 28
RESULT GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns = Passed
RESULT GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime = Passed
RESULT GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker = Passed
RESULT HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate = Passed
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
RESULT GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse = Passed
```

Note: executed (28) exceeds the 25 `[TestMethod]` attributes counted in plan fact 4; the plan states the executed count is read here rather than assumed, and the gate is "at least 25". The trx stays under the git-ignored coverage directory.
