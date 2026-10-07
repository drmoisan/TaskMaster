# Concurrent set: the three #968 classes in one invocation (issue #968, task P6-T7)

Timestamp: 2026-10-03T03-22
Command: pwsh -NoProfile -Command '<CMD-VSTEST payload>' with ASSEMBLY `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, FILTER-CONCURRENT (`FullyQualifiedName~QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherPinCountTests.|FullyQualifiedName~QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.|FullyQualifiedName~QuickFiler.Controllers.Tests.QfcItemController_FocusAndThemeTests.`), TASKID p6-t7 and an empty NAMES list; the Command Reference CMD-VSTEST macro executed verbatim with PREFIX and TOOLS expanded and WORKTREE substituted
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-CONCURRENT" "/ResultsDirectory:coverage\test-results\968\p6-t7" "/Logger:trx;LogFileName=p6-t7.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- RESULT_COUNT: 29
- COUNTERS total=29 executed=29 passed=29 failed=0
- CONCURRENT-NOT-PASSED: NONE

This is a supporting observation under the CLI runsettings (Workers 0, ClassLevel scope); MSTest cannot be made to interleave classes on demand, so it is not the regression gate.

RESULT lines (all Passed):

- ToggleTipsAsync_WithEmptyCollections_Completes = Passed duration=00:00:00.0010188
- EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher = Passed duration=00:00:00.0034677
- EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt = Passed duration=00:00:00.0468345
- ToggleFocusOnAsync_ActivatesUiAndSwitchesToActiveTheme = Passed duration=00:00:00.0015170
- ToggleFocus_ParameterlessOverload_FromActive_DeactivatesUiAndSwitchesToNormalTheme = Passed duration=00:00:00.0007448
- InvokeBeginInvoke_WhenAsync_UsesBeginInvoke = Passed duration=00:00:00.0011343
- ToggleFocusOffAsync_DeactivatesUiAndSwitchesToNormalTheme = Passed duration=00:00:00.0007586
- Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Passed duration=00:00:00.0028801
- SetThemeDark_FromNormal_SelectsDarkNormalTheme = Passed duration=00:00:00.0004370
- HtmlDarkConverter_WhenWebViewNotInitialized_DoesNotNavigate = Passed duration=00:00:00.0005972
- EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome = Passed duration=00:00:00.0008154
- Transaction_DisposedTwice_DoesNotOverReleaseTheGate = Passed duration=00:00:00.0019311
- ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke = Passed duration=00:00:00.0010086
- ToggleFocus_StateOverload_Off_FromActive_DeactivatesUiAndSwitchesToNormalTheme = Passed duration=00:00:00.0019170
- EnsureDispatcher_ScopeDisposedTwice_IsIdempotent = Passed duration=00:00:00.0039081
- InvokeBeginInvoke_WhenSynchronous_UsesInvoke = Passed duration=00:00:00.0004533
- ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke = Passed duration=00:00:00.3533170
- BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing = Passed duration=00:00:00.0509083
- ToggleNavigationAsync_AwaitsPositionTipsToggleAsync = Passed duration=00:00:00.0011420
- ToggleNavigation_WithState_TogglesPositionTipsWithState = Passed duration=00:00:00.0006716
- EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease = Passed duration=00:00:00.0365535
- Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException = Passed duration=00:00:00.0050432
- SetThemeLight_FromNormal_SelectsLightNormalTheme = Passed duration=00:00:00.0002907
- EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores = Passed duration=00:00:00.0012340
- ToggleNavigation_Synchronous_TogglesPositionTips = Passed duration=00:00:00.0050222
- ToggleTips_Synchronous_DispatchesAndExecutesDelegate = Passed duration=00:00:00.0004073
- ToggleSaveAttachments_DoesNotThrow = Passed duration=00:00:00.0003050
- EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose = Passed duration=00:00:00.0008321
- TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition = Passed duration=00:00:00.0024514
