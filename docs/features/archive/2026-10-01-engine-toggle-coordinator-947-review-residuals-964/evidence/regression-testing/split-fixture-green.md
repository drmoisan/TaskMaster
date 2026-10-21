# Split Fixture Green (P1-T8)

Timestamp: 2026-10-03T07-45
Task: P1-T8
Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"; vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\964\p1-t8" "/Logger:trx;LogFileName=p1-t8.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

Output Summary:
- Build: MSBUILD_EXIT_CODE: 0; ERRORS: 0; TEST_DLL_ADVANCED: True; CSC_OUT_TASKMASTER: 2; CSC_OUT_TASKMASTER_TEST: 2 (the split files compiled).
- Test: VSTEST_EXIT_CODE: 0; TRX_PRESENT: True; SEQUENCE_FILES: 0.
- COUNTERS total=39 executed=39 passed=39 failed=0 (total equals BASELINE-TOTAL 39).
- All 11 INVARIANT-NAMES entries Passed; no FAILED line.
- Verdict: PASS (no SPLIT CHANGED BEHAVIOUR).

Details (CMD-VSTEST output lines):
```
COUNTERS total=39 executed=39 passed=39 failed=0
RESULT GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly = Passed
RESULT GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly = Passed
RESULT GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns = Passed
RESULT GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
RESULT HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing = Passed
RESULT GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime = Passed
RESULT HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport = Passed
RESULT GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared = Passed
RESULT GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime = Passed
```
