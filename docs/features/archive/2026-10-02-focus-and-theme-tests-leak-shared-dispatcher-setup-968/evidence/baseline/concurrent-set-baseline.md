# Baseline: concurrent run of the fixture-tests and focus-and-theme classes (issue #968, task P0-T14)

Timestamp: 2026-10-03T02-50
Command: pwsh -NoProfile -Command '<CMD-VSTEST payload>' with ASSEMBLY `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, FILTER `FullyQualifiedName~QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.|FullyQualifiedName~QuickFiler.Controllers.Tests.QfcItemController_FocusAndThemeTests.` (FILTER-BASELINE-CONCURRENT), TASKID p0-t14 and an empty NAMES list (`$names = @()`); the payload is the Command Reference CMD-VSTEST macro executed verbatim with PREFIX and TOOLS expanded and WORKTREE substituted
Canonical command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-BASELINE-CONCURRENT" "/ResultsDirectory:coverage\test-results\968\p0-t14" "/Logger:trx;LogFileName=p0-t14.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- RESULT_COUNT: 25
- BASELINE-CONCURRENT-COUNTERS: COUNTERS total=25 executed=25 passed=25 failed=0
- BASELINE-CONCURRENT-FAILED: NONE

RESULT lines (trx-derived; the trx stays under the ignored coverage directory):

- InvokeBeginInvoke_WhenAsync_UsesBeginInvoke = Passed duration=00:00:00.0011730
- SetThemeLight_FromNormal_SelectsLightNormalTheme = Passed duration=00:00:00.0002698
- EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose = Passed duration=00:00:00.0011329
- Transaction_DisposedTwice_DoesNotOverReleaseTheGate = Passed duration=00:00:00.0019664
- ToggleNavigationAsync_AwaitsPositionTipsToggleAsync = Passed duration=00:00:00.0012680
- ToggleFocusOnAsync_ActivatesUiAndSwitchesToActiveTheme = Passed duration=00:00:00.0015491
- InvokeBeginInvoke_WhenSynchronous_UsesInvoke = Passed duration=00:00:00.0004615
- Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Passed duration=00:00:00.0036387
- EnsureDispatcher_ScopeDisposedTwice_IsIdempotent = Passed duration=00:00:00.0041188
- ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke = Passed duration=00:00:00.0010535
- ToggleNavigation_WithState_TogglesPositionTipsWithState = Passed duration=00:00:00.0006501
- Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException = Passed duration=00:00:00.0047744
- TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition = Passed duration=00:00:00.0025458
- BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing = Passed duration=00:00:00.0483000
- ToggleSaveAttachments_DoesNotThrow = Passed duration=00:00:00.0002902
- ToggleFocusOffAsync_DeactivatesUiAndSwitchesToNormalTheme = Passed duration=00:00:00.0011325
- ToggleFocus_StateOverload_Off_FromActive_DeactivatesUiAndSwitchesToNormalTheme = Passed duration=00:00:00.0018879
- ToggleNavigation_Synchronous_TogglesPositionTips = Passed duration=00:00:00.0055640
- HtmlDarkConverter_WhenWebViewNotInitialized_DoesNotNavigate = Passed duration=00:00:00.0005872
- ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke = Passed duration=00:00:00.3641647
- ToggleFocus_ParameterlessOverload_FromActive_DeactivatesUiAndSwitchesToNormalTheme = Passed duration=00:00:00.0008017
- EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt = Passed duration=00:00:00.0729268
- ToggleTipsAsync_WithEmptyCollections_Completes = Passed duration=00:00:00.0010125
- ToggleTips_Synchronous_DispatchesAndExecutesDelegate = Passed duration=00:00:00.0004438
- SetThemeDark_FromNormal_SelectsDarkNormalTheme = Passed duration=00:00:00.0004225

No MESSAGE lines were printed (no non-Passed outcome). No Timeout or Aborted outcome and no Sequence file (no BASELINE HANG).
