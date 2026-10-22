# #889 pass-after ([P1-T6])

Timestamp: 2026-09-29T09-13
Command: CMD-TEST-SCOPED with STAGE = 889-green and FILTER = FullyQualifiedName~UtilitiesCS.Test.Threading: pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage/test-results/930-889-green | Out-Null; & $vstest UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/logger:console;verbosity=normal" "/Logger:trx;LogFileName=930-889-green.trx" /ResultsDirectory:coverage/test-results/930-889-green "/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.Threading" 2>&1 | Tee-Object -FilePath coverage/930-889-green-tests.log; "VSTEST_EXIT=$LASTEXITCODE"'
Command: CMD-TRX-SUMMARY with STAGE = 889-green
EXIT_CODE: 0
Output Summary:
- VSTEST_EXIT=0; console: Test Run Successful. Total tests: 128. Passed: 128.
- Test run outcome: Completed
- Total 128, executed 128, passed 128, failed 0. (BASELINE-THREADING-TOTAL 127 plus 1)
- Failed tests: none
- SEQUENCE_FILES=0 (informational; the scoped runs attach no blame collector)
- RESULT Passed IsCompleted_WhenTheAwaiterContextIsAForeignDispatcherContextAndNoUiDispatcherWasCaptured_ReturnsFalse
- All twelve pre-existing IsCompleted tests enumerated in [P0-T13] present as RESULT Passed:
  - IsCompleted_WhenContextIsNotCurrent_ReturnsFalse
  - IsCompleted_WhenContextMatchesCurrent_ReturnsTrue
  - IsCompleted_WhenAmbientContextIsTheCapturedInstance_ReturnsTrue
  - IsCompleted_WhenAmbientContextIsNullAndCapturedContextIsNotNull_ReturnsFalse
  - IsCompleted_WhenUiThreadIdIsTheMinusOneSentinel_ReturnsFalse
  - IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue
  - IsCompleted_WhenTheDispatcherContextBelongsToADifferentThreadsDispatcher_ReturnsFalse
  - IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse
  - IsCompleted_OnTheThreadThatOwnsTheCapturedDispatcherWithTheCapturedUiContext_ReturnsTrue
  - IsCompleted_OnDefaultAwaiterOnAContextFreeThread_ReturnsTrue
  - IsCompleted_WhenTheCapturedUiContextMatchesButTheExecutingThreadOwnsNoDispatcher_ReturnsFalse
  - IsCompleted_WhenNoUiDispatcherWasCapturedAndTheExecutingThreadHasNone_ReturnsFalse
