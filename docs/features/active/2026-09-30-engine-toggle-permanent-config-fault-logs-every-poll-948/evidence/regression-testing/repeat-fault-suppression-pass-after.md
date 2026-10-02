# Repeat Fault Suppression Pass-After (P2-T5)

Timestamp: 2026-10-01T23-51
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\948\p2-t5" "/Logger:trx;LogFileName=p2-t5.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
EXIT_CODE: 0
Output Summary:
VSTEST_EXIT_CODE: 0
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=39 executed=39 passed=39 failed=0
RESULT_COUNT: 39
RESULT GetPressed_WhenPrimeFaults_FirstReportStatesRepeatsAreNotLoggedAgain = Passed
RESULT GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly = Passed
RESULT GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime = Passed
RESULT GetPressed_WhenFailureKindChanges_LogsNewKindOnce = Passed
RESULT GetPressed_WhenSpamFaultIsSuppressed_FirstTriageFaultIsStillLogged = Passed
RESULT HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate = Passed
RESULT HandleToggleClickAsync_AfterSuppressedPrimeFault_StillLogsToggleFault = Passed
RESULT GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse = Passed
RESULT GetPressed_AfterSuppressedFaults_LaterSuccessfulPrimeCachesStateAndInvalidatesOnce = Passed
RESULT GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns = Passed
RESULT ExecuteToggleAsync_WhenToggleFaults_PropagatesUnchanged = Passed
RESULT GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker = Passed
RESULT GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly = Passed
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
No FAILED line was printed.

The only difference between this run and the P1-T4 fail-before run is the production edit E1 to E4 in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`; the new partial and its compile entry were present in both runs, and the command is identical except for the task id segments.

PROD-HASH-AFTER: E937059271C23C1436EAECEDC04E33DF360F72B620A7BF373B02016528B32E67 (CMD-HASH of the production file; differs from ANCHOR-HASH-PROD EFC6F0DB3766495223014357FEAEE8D9AF530FA5A4C73717E42454D4FB3A05BF)

Pre-run process check: FOREIGN_CANDIDATES: 0; STRAY_TEST_PROCESSES: 0.

## Acceptance checks

- EXIT_CODE 0; TRX_PRESENT True; SEQUENCE_FILES 0
- COUNTERS failed 0, total 39 equals BASELINE-TOTAL (32) plus 7
- all fourteen NAMES-948 names Passed; no FAILED line

## POPULATION-COMPARISON:

Timestamp: 2026-10-01T23-52. Sources: evidence/baseline/coordinator-tests-baseline.md (BASELINE-TOTAL 32, BASELINE-FAILED NONE), evidence/regression-testing/repeat-fault-suppression-fail-before.md (total 39, failed 7) and this artifact (total 39, failed 0).

- Pass-after total 39 equals fail-before total 39 and equals BASELINE-TOTAL 32 plus 7.
- Pass-after failed: 0.
- BASELINE-FAILED is NONE, so no baseline failure needed re-checking.
- Every FAILED name of the fail-before run (GetPressed_AfterSuppressedFaults_LaterSuccessfulPrimeCachesStateAndInvalidatesOnce, GetPressed_WhenPrimeFaults_FirstReportStatesRepeatsAreNotLoggedAgain, GetPressed_WhenFailureKindChanges_LogsNewKindOnce, GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly, GetPressed_WhenSpamFaultIsSuppressed_FirstTriageFaultIsStillLogged, HandleToggleClickAsync_AfterSuppressedPrimeFault_StillLogsToggleFault, GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly) appears as Passed in this run. No test is still failing.
