# Refusal Path Fail-Before (P1-T14, expect-fail)

Timestamp: 2026-10-03T07-48
Task: P1-T14 [expect-fail]
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\964\p1-t14" "/Logger:trx;LogFileName=p1-t14.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (preceded by msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" and CMD-STRIPPED-COUNT with TOKENS-STRUCT)
EXIT_CODE: 1
ExpectedExitCode: 1

Output Summary:
- Build: MSBUILD_EXIT_CODE: 0; ERRORS: 0; TEST_DLL_ADVANCED: True; CSC_OUT_TASKMASTER: 0; CSC_OUT_TASKMASTER_TEST: 2.
- Stripped census immediately before the run: every token at its base value except the two split tokens (`internalsealedclassEngineToggleStateCoordinator` 0, `internalsealedpartialclassEngineToggleStateCoordinator` 3); the fix is absent from the coordinator under test.
- VSTEST_EXIT_CODE: 1; TRX_PRESENT: True; SEQUENCE_FILES: 0.
- COUNTERS total=43 executed=43 passed=40 failed=3 (BASELINE-TOTAL 39 plus 4).
- FAILED lines name exactly the three FAIL-BEFORE-NAMES.
- GUARD-NAME `GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain` = Passed; all 11 INVARIANT-NAMES Passed.
- Reason gate: each failure message carries its two required fragments (see Details).
- Verdict: PASS for the expect-fail task (no FAIL-BEFORE NOT REPRODUCED, FAIL-BEFORE WRONG REASON, INVARIANT GUARD RED AT BASE or UNEXPECTED FAILURE).

Details:

Stripped census (immediately before the test run):
```
STRIPPED [catch(] = 3
STRIPPED [catch(Exceptionex)] = 1
STRIPPED [catch(Exception)] = 2
STRIPPED [TryInvokeSink(] = 0
STRIPPED [_logError(] = 2
STRIPPED [_notifyUnavailable(] = 1
STRIPPED [TryInvokeSink(()=>_notifyUnavailable(BuildUnavailableMessage(engineName)),outvarnotifyFailure)] = 0
STRIPPED [TryInvokeSink(()=>_logError(BuildNotifyFailedMessage(engineName),notifyFailure),out_)] = 0
STRIPPED [TryInvokeSink(()=>_logError(BuildToggleFailedMessage(engineName),ex),out_)] = 0
STRIPPED [failure),out_)){_reportedPrimeFaults[reportKey]=0;}] = 0
STRIPPED [_reportedPrimeFaults[reportKey]=0;] = 1
STRIPPED [privatestaticboolTryInvokeSink(ActionsinkCall,outExceptionsinkFailure)] = 0
STRIPPED [privatestaticstringBuildNotifyFailedMessage(stringengineName)] = 0
STRIPPED [internalsealedclassEngineToggleStateCoordinator] = 0
STRIPPED [internalsealedpartialclassEngineToggleStateCoordinator] = 3
STRIPPED [_primeTasks.TryRemove(engineName,out_);] = 1
```

CMD-VSTEST output (absolute paths replaced with REDACTED-PATH):
```
COUNTERS total=43 executed=43 passed=40 failed=3
RESULT GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly = Passed
RESULT GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain = Passed
RESULT HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow = Failed
RESULT HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing = Passed
RESULT GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly = Passed
RESULT HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow = Failed
RESULT GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged = Passed
RESULT GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns = Passed
RESULT GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime = Passed
RESULT GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared = Passed
RESULT HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport = Passed
RESULT HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing = Failed
RESULT GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime = Passed
FAILED HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow
MESSAGE HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow :: Did not expect any exception because the refusal path contains a failure of both sinks, but found System.InvalidOperationException: notify sink failed  /    at TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests.<>c__DisplayClass45_0.<HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow>b__0(String _) in REDACTED-PATH\TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.SinkGuard.cs:line 93  /    at TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests.Harness.<.ctor>b__0_2(String message) in REDACTED-PATH\TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs:line 417  /    at TaskMaster.EngineToggleStateCoordinator.<HandleToggleClickAsync>d__10.MoveNext() in REDACTED-PATH\TaskMaster\Ribbon\EngineToggleStateCoordinator.cs:line 178  / --- End of stack trace from previous location where exception was thrown ---  /    at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()  /    at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)  /    at FluentAssertions.Specialized.NonGenericAsyncFunctionAssertions.<NotThrowAsync>d__4.MoveNext() in /_/Src/FluentAssertions/Specialized/NonGenericAsyncFunctionAssertions.cs:line 101.
FAILED HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow
MESSAGE HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow :: Did not expect any exception because a throwing notification sink must not escape the refusal path, but found System.InvalidOperationException: notify sink failed  /    at TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests.<>c.<HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow>b__43_0(String _) in REDACTED-PATH\TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.SinkGuard.cs:line 35  /    at TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests.Harness.<.ctor>b__0_2(String message) in REDACTED-PATH\TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs:line 417  /    at TaskMaster.EngineToggleStateCoordinator.<HandleToggleClickAsync>d__10.MoveNext() in REDACTED-PATH\TaskMaster\Ribbon\EngineToggleStateCoordinator.cs:line 178  / --- End of stack trace from previous location where exception was thrown ---  /    at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()  /    at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)  /    at FluentAssertions.Specialized.NonGenericAsyncFunctionAssertions.<NotThrowAsync>d__4.MoveNext() in /_/Src/FluentAssertions/Specialized/NonGenericAsyncFunctionAssertions.cs:line 101.
FAILED HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing
MESSAGE HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing :: Test method TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests.HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing threw exception:  / System.InvalidOperationException: notify sink failed
```

Reason gate:
- `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow`: contains `a throwing notification sink must not escape the refusal path` and `notify sink failed`.
- `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing`: contains `threw exception` and `notify sink failed`.
- `HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow`: contains `the refusal path contains a failure of both sinks` and `notify sink failed`.
