# Baseline Scoped Test Census ([P0-T13])

Timestamp: 2026-09-29T09-08

## Run 1: threading namespace

Command: CMD-TEST-SCOPED with STAGE = baseline-threading and FILTER = FullyQualifiedName~UtilitiesCS.Test.Threading: pwsh -NoProfile -Command '$vswhere = ...; $vstest = ...; New-Item -ItemType Directory -Force -Path coverage/test-results/930-baseline-threading | Out-Null; & $vstest UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/logger:console;verbosity=normal" "/Logger:trx;LogFileName=930-baseline-threading.trx" /ResultsDirectory:coverage/test-results/930-baseline-threading "/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.Threading" 2>&1 | Tee-Object -FilePath coverage/930-baseline-threading-tests.log; "VSTEST_EXIT=$LASTEXITCODE"'
Command: CMD-TRX-SUMMARY with STAGE = baseline-threading
THREADING_VSTEST_EXIT=0
Output Summary (run 1):
- Console: Test Run Successful. Total tests: 127. Passed: 127.
- Test run outcome: Completed
- Total 127, executed 127, passed 127, failed 0.
- Skipped 0, derived as total minus executed rather than reported by the test platform.
- Failed tests: none
- SEQUENCE_FILES=0 (informational; the scoped runs attach no blame collector)
- All twelve pre-existing IsCompleted tests appear as RESULT Passed:
  - RESULT Passed IsCompleted_WhenContextIsNotCurrent_ReturnsFalse
  - RESULT Passed IsCompleted_WhenContextMatchesCurrent_ReturnsTrue
  - RESULT Passed IsCompleted_WhenAmbientContextIsTheCapturedInstance_ReturnsTrue
  - RESULT Passed IsCompleted_WhenAmbientContextIsNullAndCapturedContextIsNotNull_ReturnsFalse
  - RESULT Passed IsCompleted_WhenUiThreadIdIsTheMinusOneSentinel_ReturnsFalse
  - RESULT Passed IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue
  - RESULT Passed IsCompleted_WhenTheDispatcherContextBelongsToADifferentThreadsDispatcher_ReturnsFalse
  - RESULT Passed IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse
  - RESULT Passed IsCompleted_OnTheThreadThatOwnsTheCapturedDispatcherWithTheCapturedUiContext_ReturnsTrue
  - RESULT Passed IsCompleted_OnDefaultAwaiterOnAContextFreeThread_ReturnsTrue
  - RESULT Passed IsCompleted_WhenTheCapturedUiContextMatchesButTheExecutingThreadOwnsNoDispatcher_ReturnsFalse
  - RESULT Passed IsCompleted_WhenNoUiDispatcherWasCapturedAndTheExecutingThreadHasNone_ReturnsFalse
- BASELINE-THREADING-TOTAL: 127

## Run 2: ILGlobals class

Command: CMD-TEST-SCOPED with STAGE = baseline-ilglobals and FILTER = FullyQualifiedName~UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.ILGlobals_Tests (same command shape as run 1 with STAGE and FILTER substituted)
Command: CMD-TRX-SUMMARY with STAGE = baseline-ilglobals
EXIT_CODE: 0
Output Summary (run 2):
- Console: Test Run Successful. Total tests: 14. Passed: 14.
- Test run outcome: Completed
- Total 14, executed 14, passed 14, failed 0.
- Failed tests: none
- SEQUENCE_FILES=0 (informational)
- RESULT lines: ProcessSpecialTypes_StringAlone_ReturnsString, LoadOpCodes_DoesNotRepublishPublishedTables, ProcessSpecialTypes_SystemInt32_ReturnsInt, SingleByteOpCodes_FieldIsInitOnly, SingleByteOpCodes_IsPublishedWithFullLength, ProcessSpecialTypes_SystemString_ReturnsString, OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes, ProcessSpecialTypes_UnknownType_ReturnsSameString, MultiByteOpCodes_FieldIsInitOnly, ProcessSpecialTypes_SystemDotstring_ReturnsString, ProcessSpecialTypes_Int32_ReturnsInt, MultiByteOpCodes_IsPublishedWithFullLength, ProcessSpecialTypes_Int_ReturnsInt, Cache_IsInitialized (all 14 RESULT Passed)
- RESULT Passed Cache_IsInitialized is present.
- BASELINE-ILGLOBALS-TOTAL: 14
