# Coordinator fixture run after the edits (P1-T6)

Timestamp: 2026-10-03T09-23 (host clock read at correction; the label first written was composed, not read)
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\964\p1-t6" "/Logger:trx;LogFileName=p1-t6.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0
Output Summary: 45 of 45 passed, 0 failed; R1-NAME ran 2 rows and both passed; R2-NAME and every other named test passed with 1 row.

VSTEST_EXIT_CODE: 0
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=45 executed=45 passed=45 failed=0
FIXTURE-TOTAL: 45
RESULT HandleToggleClickAsync_WithNullOrEmptyKeyAndNullEngines_NotifiesOnceWithNullTokenAndInvokesNothing rows=2 passed=2
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
