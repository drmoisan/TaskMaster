# P5 Lifetime and Host Coverage Pass-After

Timestamp: 2026-07-22T11:12:49Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbDropDownLifecycleCoverageTests|FullyQualifiedName~BreadcrumbDropDownHostTests' '/Logger:console;Verbosity=detailed'`

EXIT_CODE: 0

Output Summary: PASS. VSTest 18.8.0 discovered exactly 25 cases: 12 `BreadcrumbDropDownLifecycleCoverageTests` cases and 13 `BreadcrumbDropDownHostTests` cases. All 25 passed, with 0 failed and 0 skipped, in 1.3841 seconds.

## Every test result

### BreadcrumbDropDownHostTests — 13 passed

- PASS `Constructor_OwnsAutoClosingToolStripDropDownWithoutGlobalTopmostForm`
- PASS `OpenAsync_CreatesToolStripControlHostAndUsesCalculatedScreenPlacement`
- PASS `ExplicitCommitAndUncommittedClose_HaveDistinctCallbacks`
- PASS `OpenAndClose_TransferFocusIntoPendingOptionAndBackToAnchor`
- PASS `SetTheme_RetainsLatestThemeForTheReusablePopupSurface`
- PASS `SetTheme_BlankTheme_RejectsExplicitly`
- PASS `OpenAsync_WhenAlreadyOpen_FocusesPendingWithoutRecreatingOrShowing`
- PASS `OpenAsync_ZeroWorkingArea_RestoresSelectionAndFocus`
- PASS `OpenAsync_ShowFailure_ClosesUncommittedAndRetainsTheFailure`
- PASS `NativeClosedEvent_CancelsOnceAndIgnoresLaterCloseNotifications`
- PASS `ResetAndDispose_HandleOpenOrPartialStateAndRejectLaterUse`
- PASS `Reset_DisposesAnOrphanedPartialSurface`
- PASS `ProductionConstructor_RejectsMissingInitializerOrHtml`

### BreadcrumbDropDownLifecycleCoverageTests — 12 passed

- PASS `OpenLifetime_SharedOpenWithoutPlacement_CompletesFalseAndCleansSurface`
- PASS `OpenLifetime_ScheduleOverloads_RunSuccessAndContainReportedFaults`
- PASS `OpenLifetime_DisposeIsIdempotentAndSuppressesLaterSchedules`
- PASS `OpenLifetime_RollbackReporterFailure_IsContainedAndPrimaryIsRetained`
- PASS `OpenLifetime_StaleAndFailedRetention_CleansEachSurfaceExactlyOnce`
- PASS `Host_FourForwardingConstructors_CreateWithoutInvokingSurfaceAdapters`
- PASS `Host_InstalledMessengerAndAlreadyOpenPath_ReuseAndFocusCurrentSurface`
- PASS `Host_CloseFalseTrueReasonsAndRepeatedClose_HaveExactCallbacks`
- PASS `Host_SetTheme_ValidAndBlankValues_FollowExactContract`
- PASS `Host_DisposeAndUseAfterDispose_FollowDeterministicContract`
- PASS `Host_NativeClosedCallback_CancelsOnceAndIgnoresRepeatedNotification`
- PASS `Host_CoreConstructorNullDependencies_UseExactParameterContracts`
