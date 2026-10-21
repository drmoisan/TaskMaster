# Coordinator Tests Baseline (P0-T16)

Timestamp: 2026-10-01T23-26
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\948\p0-t16" "/Logger:trx;LogFileName=p0-t16.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
EXIT_CODE: 0
Output Summary: VSTEST_EXIT_CODE 0; TRX_PRESENT True; SEQUENCE_FILES 0; COUNTERS total=32 executed=32 passed=32 failed=0 (equals EXPECTED-CASES 32 from P0-T7); all seven existing NAMES-948 tests Passed; no new-test RESULT line; BASELINE-FAILED NONE.

Pre-run process check: FOREIGN_CANDIDATES: 0; STRAY_TEST_PROCESSES: 0.

```
VSTEST_EXIT_CODE: 0
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=32 executed=32 passed=32 failed=0
RESULT_COUNT: 32
RESULT GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker = Passed
RESULT ExecuteToggleAsync_WhenToggleFaults_PropagatesUnchanged = Passed
RESULT GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns = Passed
RESULT HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate = Passed
RESULT GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse = Passed
RESULT GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime = Passed
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
```

BASELINE-COUNTERS: total=32 executed=32 passed=32 failed=0

BASELINE-TOTAL: 32

BASELINE-FAILED: NONE

No RESULT line names any of the seven NEW-NAMES-948 tests (they do not exist yet).
