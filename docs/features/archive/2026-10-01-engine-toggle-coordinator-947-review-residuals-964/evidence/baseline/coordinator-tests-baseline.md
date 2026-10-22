# Baseline: coordinator fixture run (P0-T13)

Timestamp: 2026-10-03T07-38
Task: P0-T13
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\964\p0-t13" "/Logger:trx;LogFileName=p0-t13.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

Output Summary:
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=39 executed=39 passed=39 failed=0
- BASELINE-TOTAL: 39
- All 11 INVARIANT-NAMES entries: Passed. No RESULT line names a NEW-NAMES-964 entry. No FAILED line.
- Verdict: PASS (no EXISTING FIXTURE NOT GREEN AT BASE).

Details (CMD-VSTEST output lines, transcribed):
```
COUNTERS total=39 executed=39 passed=39 failed=0
RESULT GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
RESULT GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns = Passed
RESULT GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly = Passed
RESULT GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly = Passed
RESULT GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime = Passed
RESULT HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport = Passed
RESULT HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing = Passed
RESULT GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared = Passed
RESULT GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime = Passed
```
The trx stays under the git-ignored coverage directory.
