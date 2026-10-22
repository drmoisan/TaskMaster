# Refusal Path Pass-After (P1-T23, P1-T24)

## BUILD (P1-T23)

Timestamp: 2026-10-03T08-06
Task: P1-T23
Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
EXIT_CODE: 0

Output Summary:
- MSBUILD_EXIT_CODE: 0
- ERRORS: 0
- TEST_DLL_ADVANCED: True
- CSC_OUT_TASKMASTER: 2
- CSC_OUT_TASKMASTER_TEST: 2
- Verdict: PASS (`MSBUILD_EXIT_CODE: 0`, `ERRORS: 0`, `CSC_OUT_TASKMASTER:` at least 1, `TEST_DLL_ADVANCED: True`). The msbuild log stays under the git-ignored coverage/logs directory.

## TEST RUN (P1-T24)

Timestamp: 2026-10-03T08-07
Task: P1-T24
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\964\p1-t24" "/Logger:trx;LogFileName=p1-t24.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

Output Summary:
- Pre-run check: STRAY_TEST_PROCESSES: 0.
- VSTEST_EXIT_CODE: 0; TRX_PRESENT: True; SEQUENCE_FILES: 0.
- COUNTERS total=43 executed=43 passed=43 failed=0 (BASELINE-TOTAL 39 plus 4).
- All 4 NEW-NAMES-964 and all 11 INVARIANT-NAMES entries = Passed; no FAILED line.
- Verdict: PASS (no PASS-AFTER NOT MET). The three FAIL-BEFORE-NAMES that failed at P1-T14 now pass against the fixed coordinator.

CMD-VSTEST output:
```
VSTEST_EXIT_CODE: 0
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=43 executed=43 passed=43 failed=0
RESULT GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain = Passed
RESULT GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared = Passed
RESULT HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow = Passed
RESULT GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime = Passed
RESULT HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow = Passed
RESULT HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport = Passed
RESULT GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime = Passed
RESULT HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing = Passed
RESULT GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly = Passed
RESULT GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly = Passed
RESULT GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
RESULT GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns = Passed
RESULT HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing = Passed
RESULT GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime = Passed
```
