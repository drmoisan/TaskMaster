# #889 fail-before ([P1-T3], expect-fail)

Timestamp: 2026-09-29T09-11
Command: CMD-TEST-SCOPED with STAGE = 889-red and FILTER = FullyQualifiedName~UtilitiesCS.Test.Threading.UiThreadPredicateHardening_Tests: pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage/test-results/930-889-red | Out-Null; & $vstest UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/logger:console;verbosity=normal" "/Logger:trx;LogFileName=930-889-red.trx" /ResultsDirectory:coverage/test-results/930-889-red "/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.Threading.UiThreadPredicateHardening_Tests" 2>&1 | Tee-Object -FilePath coverage/930-889-red-tests.log; "VSTEST_EXIT=$LASTEXITCODE"'
Command: CMD-TRX-SUMMARY with STAGE = 889-red
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- VSTEST_EXIT=1
- Console: Test Parallelization enabled (Workers: 24, Scope: ClassLevel). Failed IsCompleted_WhenTheAwaiterContextIsAForeignDispatcherContextAndNoUiDispatcherWasCaptured_ReturnsFalse; Error Message: Expected observed to be False, but found True.
- Test run outcome: Failed
- Total 3, executed 3, passed 2, failed 1.
- Failed tests: IsCompleted_WhenTheAwaiterContextIsAForeignDispatcherContextAndNoUiDispatcherWasCaptured_ReturnsFalse
- RESULT Failed IsCompleted_WhenTheAwaiterContextIsAForeignDispatcherContextAndNoUiDispatcherWasCaptured_ReturnsFalse
- RESULT Passed IsCompleted_WhenTheCapturedUiContextMatchesButTheExecutingThreadOwnsNoDispatcher_ReturnsFalse
- RESULT Passed IsCompleted_WhenNoUiDispatcherWasCapturedAndTheExecutingThreadHasNone_ReturnsFalse
- SEQUENCE_FILES=0 (informational; the scoped runs attach no blame collector)

ProductionSourceState: `git diff --stat ac819907f479ee18026993054e714dc2e056142f -- UtilitiesCS/Threading/UiThread.cs` printed nothing; the production source is unmodified.

Arranged conditions (AC1): no captured UI dispatcher (SetDispatcher(null)); captured UI thread id equal to the executing thread's managed id (SetUiThreadId on the MTA thread); awaiter context a DispatcherSynchronizationContext over a foreign STA host dispatcher, which is not the captured UI context; a non-null ambient context that differs from the awaiter context; an MTA executing thread that owns no WPF dispatcher.
