# Pre-change concurrent run of the four target classes with QfcItemController_FocusAndThemeTests (P0-T14)

Timestamp: 2026-10-02T00-53
Command: CMD-VSTEST with ASSEMBLY-QF, FILTER-CONCURRENT (FullyQualifiedName~ expressions for QfcDatamodelLivenessTests., QfcDatamodelTeardownTests., QfcInitEmailQueueZeroBatchTests., QfcItemController_UiThreadDispatcherFixtureTests. and QfcItemController_FocusAndThemeTests., QCT. expanded, joined with |), TASKID p0-t14 and empty NAMES, executed as one pwsh -NoProfile -Command payload: PREFIX, TOOLS, then the CMD-VSTEST body verbatim. One CLOCK echo line was added after PREFIX.
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-CONCURRENT" "/ResultsDirectory:coverage\test-results\950\p0-t14" "/Logger:trx;LogFileName=p0-t14.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
VSTEST_EXIT_CODE: 0
TRX_PRESENT: True
SEQUENCE_FILES: 0
BASELINE-CONCURRENT-COUNTERS: COUNTERS total=37 executed=37 passed=37 failed=0
RESULT_COUNT: 37
RESULT InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing = Passed duration=00:00:00.1249035
RESULT Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException = Passed duration=00:00:00.0062973
RESULT ToggleTipsAsync_WithEmptyCollections_Completes = Passed duration=00:00:00.0004655
RESULT ToggleFocus_ParameterlessOverload_FromActive_DeactivatesUiAndSwitchesToNormalTheme = Passed duration=00:00:00.0012099
RESULT DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle = Passed duration=00:00:00.1703879
RESULT HtmlDarkConverter_WhenWebViewNotInitialized_DoesNotNavigate = Passed duration=00:00:00.0005955
RESULT RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces = Passed duration=00:00:00.0007540
RESULT ToggleFocusOnAsync_ActivatesUiAndSwitchesToActiveTheme = Passed duration=00:00:00.0023641
RESULT Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Passed duration=00:00:00.0033943
RESULT SetThemeLight_FromNormal_SelectsLightNormalTheme = Passed duration=00:00:00.0002659
RESULT ToggleTips_Synchronous_DispatchesAndExecutesDelegate = Passed duration=00:00:00.0005281
RESULT ToggleSaveAttachments_DoesNotThrow = Passed duration=00:00:00.0002516
RESULT InvokeBeginInvoke_WhenAsync_UsesBeginInvoke = Passed duration=00:00:00.0013229
RESULT InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker = Passed duration=00:00:00.0038194
RESULT QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs = Passed duration=00:00:00.0066437
RESULT ToggleFocus_StateOverload_Off_FromActive_DeactivatesUiAndSwitchesToNormalTheme = Passed duration=00:00:00.0025793
RESULT Transaction_DisposedTwice_DoesNotOverReleaseTheGate = Passed duration=00:00:00.0029089
RESULT SetThemeDark_FromNormal_SelectsDarkNormalTheme = Passed duration=00:00:00.0004719
RESULT ToggleNavigationAsync_AwaitsPositionTipsToggleAsync = Passed duration=00:00:00.0013005
RESULT InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop = Passed duration=00:00:00.1930378
RESULT TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition = Passed duration=00:00:00.0022432
RESULT EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose = Passed duration=00:00:00.0012895
RESULT ToggleFocusOffAsync_DeactivatesUiAndSwitchesToNormalTheme = Passed duration=00:00:00.0008572
RESULT ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke = Passed duration=00:00:00.0009876
RESULT RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally = Passed duration=00:00:00.0155993
RESULT ToggleNavigation_Synchronous_TogglesPositionTips = Passed duration=00:00:00.0040634
RESULT InvokeBeginInvoke_WhenSynchronous_UsesInvoke = Passed duration=00:00:00.0004919
RESULT RemainingLoadActive_AfterLoaderCompletes_BecomesFalse = Passed duration=00:00:00.0038885
RESULT ToggleNavigation_WithState_TogglesPositionTipsWithState = Passed duration=00:00:00.0008005
RESULT BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing = Passed duration=00:00:00.0500834
RESULT Cleanup_CalledTwice_DoesNotThrow = Passed duration=00:00:00.0012892
RESULT TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing = Passed duration=00:00:00.1356806
RESULT QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout = Passed duration=00:00:00.0077365
RESULT EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt = Passed duration=00:00:00.0556902
RESULT ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke = Passed duration=00:00:00.3730804
RESULT EnsureDispatcher_ScopeDisposedTwice_IsIdempotent = Passed duration=00:00:00.0039765
RESULT Worker_DoWork_CapturesRemainingLoadTask = Passed duration=00:00:00.0012639

BASELINE-CONCURRENT-FAILED: NONE
