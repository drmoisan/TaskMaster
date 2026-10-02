# Regression Testing: Fail-Before Run of the Four New Tests (P1-T6, expect-fail)

Timestamp: 2026-10-01T17-51
Task: P1-T6 [expect-fail]
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\947\p1-t6" "/Logger:trx;LogFileName=p1-t6.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 1
ExpectedExitCode: 1

Output Summary:
- VSTEST_EXIT_CODE: 1 (deliberately failing run; ExpectedExitCode: 1)
- TRX_PRESENT: True
- SEQUENCE_FILES: 0 (no hang)
- COUNTERS total=32 executed=32 passed=28 failed=4 (total equals BASELINE-TOTAL: 28 plus 4)
- RESULT_COUNT: 32
- FAILED lines name exactly the four new tests (the first four NAMES-947 entries) and no other test.
- Faulted and canceled prime variants: each MESSAGE carries `a throwing sink leaves no marker behind, so the later read starts a new prime` and `Expected invocation on the mock exactly 2 times, but was 1 times` (matches the case-insensitive pattern `exactly 2 times, but was 1 time`).
- First-prime test: MESSAGE carries `the sink exception is contained, so the marker is still cleared` and shows the stale `Task<bool>` marker still returned by GetPrimeTask.
- Click-boundary test: MESSAGE carries `the click boundary contains a failure of the sink itself` and `sink failed`; the sink exception escaped HandleToggleClickAsync at the unguarded sink call.
- RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed; RESULT HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate = Passed; every other pre-existing NAMES-947 entry reads Passed.
- Result: the negative control reproduced the defect; no FAIL-BEFORE NOT REPRODUCED.

## Environment of the control

- PROD-HASH-AT-CONTROL: B3C6FEB2A86E36E95AC34F6108D87C8E117A94949F6FCE0B3AF26D824D6E3086 (equals BASE-HASH-PROD:)
- Partial hash at control: BEB9C785536242293C3069E05BE54A355E2E030AE1AC06398C08AEAC188361FE; PrimeFaultOrdering hash: AA88DC05B45CE2E0D935014778779C5B500025BCFD237AEE05A184CED7D6F8DB (equals BASE-HASH-PFO:)
- `git diff --exit-code 2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85 -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`: exit 0
- `git diff --exit-code 2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85 -- TaskMaster.runsettings scripts/vscode/TaskMaster.cli.runsettings`: exit 0
- STRAY_TEST_PROCESSES: 0 before the run.
- The new partial TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs and its compile entry in TaskMaster.Test/TaskMaster.Test.csproj are present, and the test assembly was rebuilt at P1-T5. The production file and the run settings are at the base.

## Transcribed lines

```
COUNTERS total=32 executed=32 passed=28 failed=4
RESULT_COUNT: 32
RESULT GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker = Passed
RESULT HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate = Passed
RESULT GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared = Failed
RESULT HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport = Failed
RESULT GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns = Passed
RESULT GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime = Failed
RESULT GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
RESULT GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime = Passed
RESULT GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime = Failed
RESULT GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse = Passed
FAILED GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared
MESSAGE GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared :: Expected harness.Coordinator.GetPrimeTask(SpamEngine) to refer to System.Threading.Tasks.Task {Status=RanToCompletion} because the sink exception is contained, so the marker is still cleared, but found System.Threading.Tasks.Task`1[System.Boolean] {Status=RanToCompletion}.
FAILED HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport
MESSAGE HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport :: Did not expect any exception because the click boundary contains a failure of the sink itself, but found System.InvalidOperationException: sink failed / at TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests.<>c.<HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport>b__33_1(String _, Exception _) in REDACTED-PATH\TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs:line 196 / at TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests.Harness.<.ctor>b__0_3(String message, Exception exception) in REDACTED-PATH\TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs:line 418 / at TaskMaster.EngineToggleStateCoordinator.<HandleToggleClickAsync>d__10.MoveNext() in REDACTED-PATH\TaskMaster\Ribbon\EngineToggleStateCoordinator.cs:line 184 / --- End of stack trace from previous location where exception was thrown --- / at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw() / at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task) / at FluentAssertions.Specialized.NonGenericAsyncFunctionAssertions.<NotThrowAsync>d__4.MoveNext() in /_/Src/FluentAssertions/Specialized/NonGenericAsyncFunctionAssertions.cs:line 101.
FAILED GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime
MESSAGE GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime :: Test method TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests.GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime threw exception: / Moq.MockException: a throwing sink leaves no marker behind, so the later read starts a new prime / Expected invocation on the mock exactly 2 times, but was 1 times: x => x.EngineActiveAsync("Spam") / Performed invocations: / Mock<IAppItemEngines:26> (x): / IAppItemEngines.EngineActiveAsync("Spam") /
FAILED GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime
MESSAGE GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime :: Test method TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests.GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime threw exception: / Moq.MockException: a throwing sink leaves no marker behind, so the later read starts a new prime / Expected invocation on the mock exactly 2 times, but was 1 times: x => x.EngineActiveAsync("Spam") / Performed invocations: / Mock<IAppItemEngines:25> (x): / IAppItemEngines.EngineActiveAsync("Spam") /
```

Absolute paths inside the click-boundary MESSAGE were replaced by REDACTED-PATH before transcription. The trx stays under the git-ignored coverage directory and is not copied into the feature folder.
