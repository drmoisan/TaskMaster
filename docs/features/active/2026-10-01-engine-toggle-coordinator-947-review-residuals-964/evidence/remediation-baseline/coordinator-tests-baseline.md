# Coordinator fixture baseline (P0-T9)

Timestamp: 2026-10-03T09-23 (host clock read at correction; the label first written was composed, not read)
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\964\p0-t9" "/Logger:trx;LogFileName=p0-t9.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0
Output Summary: 43 of 43 passed, 0 failed; R1-NAME has 0 rows (false-before); every existing SinkGuard and invariant name has 1 passing row.

VSTEST_EXIT_CODE: 0
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=43 executed=43 passed=43 failed=0
BASELINE-TOTAL: 43
RESULT HandleToggleClickAsync_WithNullOrEmptyKeyAndNullEngines_NotifiesOnceWithNullTokenAndInvokesNothing rows=0 passed=0
RESULT HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow rows=1 passed=1
RESULT HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing rows=1 passed=1
RESULT HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow rows=1 passed=1
RESULT GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain rows=1 passed=1
RESULT GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns rows=1 passed=1
RESULT GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime rows=1 passed=1
RESULT GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime rows=1 passed=1
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged rows=1 passed=1
RESULT GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime rows=1 passed=1
RESULT GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime rows=1 passed=1
RESULT GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared rows=1 passed=1
RESULT HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport rows=1 passed=1
RESULT GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly rows=1 passed=1
RESULT GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly rows=1 passed=1
RESULT HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing rows=1 passed=1
FAILED lines: none
