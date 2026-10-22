# Pass-after concurrent run of the four target classes with QfcItemController_FocusAndThemeTests (P4-T5)

Timestamp: 2026-10-02T01-08
P4-RESTART: 0
Command: CMD-VSTEST with ASSEMBLY-QF, FILTER-CONCURRENT (QCT. expanded), TASKID p4-t5 and empty NAMES, executed as one pwsh -NoProfile -Command payload: PREFIX, TOOLS, then the CMD-VSTEST body verbatim. One CLOCK echo line was added after PREFIX.
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-CONCURRENT" "/ResultsDirectory:coverage\test-results\950\p4-t5" "/Logger:trx;LogFileName=p4-t5.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
VSTEST_EXIT_CODE: 0
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=37 executed=37 passed=37 failed=0
RESULT_COUNT: 37
RESULT TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing = Passed duration=00:00:00.1351500
RESULT InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing = Passed duration=00:00:00.1243627
RESULT ToggleNavigation_Synchronous_TogglesPositionTips = Passed duration=00:00:00.0042419
RESULT InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker = Passed duration=00:00:00.0034524
RESULT SetThemeLight_FromNormal_SelectsLightNormalTheme = Passed duration=00:00:00.0002900
RESULT ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke = Passed duration=00:00:00.0061414
RESULT ToggleSaveAttachments_DoesNotThrow = Passed duration=00:00:00.0002462
RESULT ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke = Passed duration=00:00:00.3645132
RESULT SetThemeDark_FromNormal_SelectsDarkNormalTheme = Passed duration=00:00:00.0004669
RESULT ToggleFocusOffAsync_DeactivatesUiAndSwitchesToNormalTheme = Passed duration=00:00:00.0007734
RESULT InvokeBeginInvoke_WhenSynchronous_UsesInvoke = Passed duration=00:00:00.0004538
RESULT ToggleFocus_ParameterlessOverload_FromActive_DeactivatesUiAndSwitchesToNormalTheme = Passed duration=00:00:00.0012065
RESULT BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing = Passed duration=00:00:00.0513738
RESULT RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces = Passed duration=00:00:00.0005949
RESULT Worker_DoWork_CapturesRemainingLoadTask = Passed duration=00:00:00.0008942
RESULT ToggleTipsAsync_WithEmptyCollections_Completes = Passed duration=00:00:00.0004007
RESULT RemainingLoadActive_AfterLoaderCompletes_BecomesFalse = Passed duration=00:00:00.0005800
RESULT QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs = Passed duration=00:00:00.0067577
RESULT EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt = Passed duration=00:00:00.0558074
RESULT RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally = Passed duration=00:00:00.0005510
RESULT InvokeBeginInvoke_WhenAsync_UsesBeginInvoke = Passed duration=00:00:00.0012755
RESULT QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout = Passed duration=00:00:00.0073498
RESULT DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle = Passed duration=00:00:00.1602821
RESULT ToggleFocus_StateOverload_Off_FromActive_DeactivatesUiAndSwitchesToNormalTheme = Passed duration=00:00:00.0026943
RESULT Cleanup_CalledTwice_DoesNotThrow = Passed duration=00:00:00.0012682
RESULT HtmlDarkConverter_WhenWebViewNotInitialized_DoesNotNavigate = Passed duration=00:00:00.0005992
RESULT Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Passed duration=00:00:00.0035572
RESULT ToggleTips_Synchronous_DispatchesAndExecutesDelegate = Passed duration=00:00:00.0004116
RESULT ToggleFocusOnAsync_ActivatesUiAndSwitchesToActiveTheme = Passed duration=00:00:00.0020793
RESULT Transaction_DisposedTwice_DoesNotOverReleaseTheGate = Passed duration=00:00:00.0034823
RESULT EnsureDispatcher_ScopeDisposedTwice_IsIdempotent = Passed duration=00:00:00.0037047
RESULT TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition = Passed duration=00:00:00.0023954
RESULT EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose = Passed duration=00:00:00.0013583
RESULT InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop = Passed duration=00:00:00.1879890
RESULT ToggleNavigationAsync_AwaitsPositionTipsToggleAsync = Passed duration=00:00:00.0012641
RESULT ToggleNavigation_WithState_TogglesPositionTipsWithState = Passed duration=00:00:00.0008584
RESULT Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException = Passed duration=00:00:00.0062198

All nine target results Passed, including Transaction_SecondCallerCannotInstallUntilTheFirstRestores; both FocusAndThemeTests theme tests (SetThemeDark_FromNormal_SelectsDarkNormalTheme, SetThemeLight_FromNormal_SelectsLightNormalTheme) Passed, so no THEME TEST NULL-DISPATCHER EXPOSURE was observed.
CONCURRENT-NOT-PASSED: NONE
CONCURRENT-NEW-FAILURES: NONE
