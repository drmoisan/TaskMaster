# Regression Testing: Pass-After Run of the Reproduction and the Regression Tests (P1-T12)

Timestamp: 2026-10-01T17-56
Task: P1-T12
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\947\p1-t12" "/Logger:trx;LogFileName=p1-t12.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

Output Summary:
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=32 executed=32 passed=32 failed=0 (total equals BASELINE-TOTAL: 28 plus 4)
- RESULT_COUNT: 32
- All twelve NAMES-947 entries read Passed, including the four new tests that failed at P1-T6; no FAILED line.
- PROD-HASH-AFTER: EFC6F0DB3766495223014357FEAEE8D9AF530FA5A4C73717E42454D4FB3A05BF (differs from BASE-HASH-PROD: B3C6FEB2A86E36E95AC34F6108D87C8E117A94949F6FCE0B3AF26D824D6E3086)
- STRAY_TEST_PROCESSES: 0 before the run.
- The only difference between this run and P1-T6 is edits E1 to E6 in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (the partial and its compile entry were present in both runs; the partial hash BEB9C785536242293C3069E05BE54A355E2E030AE1AC06398C08AEAC188361FE and the PrimeFaultOrdering hash AA88DC05B45CE2E0D935014778779C5B500025BCFD237AEE05A184CED7D6F8DB are identical in both runs).
- Result: P1-T12 acceptance holds; no PASS-AFTER NOT GREEN.

## CMD-HASH (before the run)

```
HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = EFC6F0DB3766495223014357FEAEE8D9AF530FA5A4C73717E42454D4FB3A05BF
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = BEB9C785536242293C3069E05BE54A355E2E030AE1AC06398C08AEAC188361FE
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = AA88DC05B45CE2E0D935014778779C5B500025BCFD237AEE05A184CED7D6F8DB
```

## Transcribed lines

```
COUNTERS total=32 executed=32 passed=32 failed=0
RESULT_COUNT: 32
RESULT GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker = Passed
RESULT HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate = Passed
RESULT HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport = Passed
RESULT GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse = Passed
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
RESULT GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns = Passed
RESULT GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared = Passed
RESULT GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime = Passed
RESULT GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime = Passed
```

The trx stays under the git-ignored coverage directory and is not copied into the feature folder.

## FINAL-FIXTURE-RUN:

Timestamp: 2026-10-01T18-06
Task: P2-T7 (coordinator fixture re-run on the assembly rebuilt by P2-T6)
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\947\p2-t7" "/Logger:trx;LogFileName=p2-t7.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

Output Summary:
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=32 executed=32 passed=32 failed=0 (total equals BASELINE-TOTAL: 28 plus 4)
- All twelve NAMES-947 entries read Passed; no FAILED line.
- STRAY_TEST_PROCESSES: 0 before the run.
- Result: P2-T7 acceptance holds.

```
COUNTERS total=32 executed=32 passed=32 failed=0
RESULT_COUNT: 32
RESULT GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker = Passed
RESULT HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate = Passed
RESULT GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
RESULT GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime = Passed
RESULT GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime = Passed
RESULT HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport = Passed
RESULT GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse = Passed
RESULT GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns = Passed
RESULT GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared = Passed
```
