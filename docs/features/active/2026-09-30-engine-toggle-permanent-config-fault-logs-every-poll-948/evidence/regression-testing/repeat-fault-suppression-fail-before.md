# Repeat Fault Suppression Fail-Before (P1-T4) [expect-fail]

Timestamp: 2026-10-01T23-45
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\948\p1-t4" "/Logger:trx;LogFileName=p1-t4.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
VSTEST_EXIT_CODE: 1
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=39 executed=39 passed=32 failed=7
RESULT_COUNT: 39
RESULT GetPressed_AfterSuppressedFaults_LaterSuccessfulPrimeCachesStateAndInvalidatesOnce = Failed
RESULT GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns = Passed
RESULT ExecuteToggleAsync_WhenToggleFaults_PropagatesUnchanged = Passed
RESULT GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse = Passed
RESULT HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate = Passed
RESULT GetPressed_WhenPrimeFaults_FirstReportStatesRepeatsAreNotLoggedAgain = Failed
RESULT GetPressed_WhenFailureKindChanges_LogsNewKindOnce = Failed
RESULT GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly = Failed
RESULT GetPressed_WhenSpamFaultIsSuppressed_FirstTriageFaultIsStillLogged = Failed
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
RESULT GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker = Passed
RESULT HandleToggleClickAsync_AfterSuppressedPrimeFault_StillLogsToggleFault = Failed
RESULT GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly = Failed
RESULT GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime = Passed

FAILED and MESSAGE lines (no absolute path occurred in any message):

```
FAILED GetPressed_AfterSuppressedFaults_LaterSuccessfulPrimeCachesStateAndInvalidatesOnce
MESSAGE GetPressed_AfterSuppressedFaults_LaterSuccessfulPrimeCachesStateAndInvalidatesOnce :: Expected harness.Errors to contain a single item because the second identical fault is a suppressed repeat, but found {TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }, TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }}.
FAILED GetPressed_WhenPrimeFaults_FirstReportStatesRepeatsAreNotLoggedAgain
MESSAGE GetPressed_WhenPrimeFaults_FirstReportStatesRepeatsAreNotLoggedAgain :: Expected harness.Errors[0].Message "Reading the activation state for engine 'Spam' failed; its toggle continues to report unchecked." to contain "not logged again" because the entry must state that repeats are suppressed.
FAILED GetPressed_WhenFailureKindChanges_LogsNewKindOnce
MESSAGE GetPressed_WhenFailureKindChanges_LogsNewKindOnce :: Expected harness.Errors to contain 2 item(s) because each distinct failure kind is reported once, but found 4: {TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }, TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }, TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }, TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }}.
FAILED GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly
MESSAGE GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly :: Expected harness.Errors.Count to be 1 because a repeated fault of one kind for one engine is reported once, but found 5.
FAILED GetPressed_WhenSpamFaultIsSuppressed_FirstTriageFaultIsStillLogged
MESSAGE GetPressed_WhenSpamFaultIsSuppressed_FirstTriageFaultIsStillLogged :: Expected harness.Errors to contain 2 item(s) because suppression is keyed by engine as well as by kind, but found 3: {TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }, TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }, TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }}.
FAILED HandleToggleClickAsync_AfterSuppressedPrimeFault_StillLogsToggleFault
MESSAGE HandleToggleClickAsync_AfterSuppressedPrimeFault_StillLogsToggleFault :: Expected harness.Errors to contain 2 item(s) because one suppressed prime repeat, every toggle fault, but found 3: {TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }, TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }, TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }}.
FAILED GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly
MESSAGE GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly :: Expected harness.Errors to contain a single item because repeated cancellations are one failure kind, but found {TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }, TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }, TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }, TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }, TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests+LoggedError{ }}.
```

FAIL-BEFORE-ERROR-COUNT: 5 (parsed from the test A message by `but found (\d+)`)

## Environment of the control:

The production file TaskMaster/Ribbon/EngineToggleStateCoordinator.cs is byte-identical to MERGE-BASE 59cbab04f1c854baa2a03b6cbf755c1df4f961b4: its CMD-HASH value is recorded as PROD-HASH-AT-CONTROL: EFC6F0DB3766495223014357FEAEE8D9AF530FA5A4C73717E42454D4FB3A05BF, which equals ANCHOR-HASH-PROD from P0-T18. The new partial (CMD-HASH 606063A3EDD3139B4E7D35004217DB4B4F849DB24C0733AF17837C2A2FE3C15F) and its compile entry are present. The run settings are unchanged: `git diff --exit-code MERGE-BASE -- TaskMaster.runsettings scripts/vscode/TaskMaster.cli.runsettings` exited 0 (RUNSETTINGS_DIFF_EXIT=0).

Pre-run process check: FOREIGN_CANDIDATES: 0; STRAY_TEST_PROCESSES: 0.

## Acceptance checks

- TRX_PRESENT True; SEQUENCE_FILES 0; EXIT_CODE non-zero (1)
- COUNTERS total 39 equals BASELINE-TOTAL (32) plus 7
- all seven NEW-NAMES-948 tests Failed (D-5)
- the test A message contains `a repeated fault of one kind for one engine is reported once`; FAIL-BEFORE-ERROR-COUNT: 5
- all seven existing NAMES-948 tests Passed
- every FAILED name is one of the seven new tests (BASELINE-FAILED is NONE)
- PROD-HASH-AT-CONTROL equals ANCHOR-HASH-PROD
