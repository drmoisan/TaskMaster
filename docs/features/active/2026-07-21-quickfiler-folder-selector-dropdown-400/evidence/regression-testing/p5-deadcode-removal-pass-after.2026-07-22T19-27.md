# P5-T207 — Focused pass-after (dead-code removal, uninstrumented)

Timestamp: 2026-07-22T19-27Z

Command: `$installation = & 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -property installationPath; $vstestPath = Join-Path $installation 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'; $asm=(Resolve-Path 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll').Path; $f='FullyQualifiedName~BreadcrumbPopupBoundaryCoverageTests|FullyQualifiedName~BreadcrumbDropDownLifecycleCoverageTests|FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests'; & $vstestPath $asm /InIsolation "/TestCaseFilter:$f"`

EXIT_CODE: 0

## Result

- **Test Run Successful. Total tests: 50, Passed: 50, Failed: 0, Skipped: 0.** No coverage instrumentation.
- Per-class decomposition (matching the P5-T201 composition; the production-only removal adds and removes
  no case):
  - `BreadcrumbPopupBoundaryCoverageTests`: **23** cases discovered and passed.
  - `BreadcrumbDropDownOpenCoordinatorTests`: 15 cases passed.
  - `BreadcrumbDropDownLifecycleCoverageTests`: 12 cases passed.
  - 23 + 15 + 12 = 50.
- **`OpenAsync_RecoveryDispatchFails_ReportsOnceAndClearsStoredOpenTask` passed** (`Passed ... [< 1 ms]`),
  with all four assertions intact and unchanged:
  - `opening.Status.Should().Be(TaskStatus.RanToCompletion)`
  - `opening.Result.Should().BeFalse()`
  - `probe.Errors.Should().Equal(kickoffFailure, recoveryFailure)`
  - `probe.StoredOpenTask.Should().BeNull(...)`
- This pass proves the removed inner `catch (Exception recoveryFailure)` at former lines 153-156 was
  genuinely dead: the `recoveryFailure` continues to be reported exactly once by
  `HandleOpenFailureAsync`'s own internal `catch` (its `RunAsync` uses `reportFailure: false`, so the
  faulted task propagates into that catch and calls `Report`), so `Errors == [kickoffFailure,
  recoveryFailure]` still holds. No assertion was added, removed, weakened, relaxed, reordered, or made
  conditional; the case count remained 170 at the composition level (50 in this focused three-class
  subset).

## Discovered/passed case list (50)

CaptureCurrentOrTests_NullAndControlledContexts_SelectExpectedBoundaries;
ConstructorAndProviderUpdates_GuardEveryRequiredDelegate;
Dispatcher_NullInputsAndThrowingSink_AreHandledByContract;
Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction;
Dispatcher_PostedFailure_ReportsOnceAndRestoresBoundary;
HandleSelectorOpenStateChanged_AfterRelease_PostsNothingAndSkipsSelectorPredicate;
HandleSelectorOpenStateChanged_QueuedBodyDrainedAfterRelease_PerformsNoWork;
Host_CloseFalseTrueReasonsAndRepeatedClose_HaveExactCallbacks;
Host_CoreConstructorNullDependencies_UseExactParameterContracts;
Host_DisposeAndUseAfterDispose_FollowDeterministicContract;
Host_FourForwardingConstructors_CreateWithoutInvokingSurfaceAdapters;
Host_InstalledMessengerAndAlreadyOpenPath_ReuseAndFocusCurrentSurface;
Host_NativeClosedCallback_CancelsOnceAndIgnoresRepeatedNotification;
Host_SetTheme_ValidAndBlankValues_FollowExactContract;
InjectedFactory_CleanupFailure_DoesNotReplacePrimaryFailure;
InjectedFactory_CoreFailure_DisposesControlOnce;
InjectedFactory_CreateFailure_ReportsOnceWithoutCleanup;
InjectedFactory_InitializationFailure_DisposesControlOnce;
InjectedFactory_NavigationFailure_DisposesControlOnce;
InjectedFactory_Success_UsesOwnerBoundaryAndReturnsReadySurface;
NativeClosedCallback_HostClosedBeforeDrain_PerformsNoLateCloseWork;
NormalizeFactory_SuccessAndNullResultPaths_PreserveContract;
OpenAsync_CleanupDispatchFails_ReportsSecondaryOnceAndPreservesPrimary;
OpenAsync_CreationFailsAndCleanupSucceeds_DisposesOwnedSurfaceWithoutReport;
OpenAsync_LeaseSupersededDuringInstall_DisposesInstalledSurfaceExactlyOnce;
OpenAsync_RecoveryDispatchFails_ReportsOnceAndClearsStoredOpenTask;
OpenLifetime_DisposeIsIdempotentAndSuppressesLaterSchedules;
OpenLifetime_RollbackReporterFailure_IsContainedAndPrimaryIsRetained;
OpenLifetime_ScheduleOverloads_RunSuccessAndContainReportedFaults;
OpenLifetime_SharedOpenWithoutPlacement_CompletesFalseAndCleansSurface;
OpenLifetime_StaleAndFailedRetention_CleansEachSurfaceExactlyOnce;
ProductionFactoryCreate_ControlledContext_CapturesWithoutInvokingAdapters;
Readiness_BeginNavigationGuardsNullDuplicateAndTerminalRequests;
Readiness_CancelAndDispose_AreIdempotent;
Readiness_ConstructorGuardsBlankNameAndNullDetach;
Readiness_DetachFailure_IsContainedAndCompletionSucceeds;
Readiness_Failure_NormalizesNullAndBlankStatuses;
Readiness_UnrelatedAndDuplicateNotifications_CompleteCapturedSuccessOnce;
RequestOpen_ConcurrentCallersShareOneUiBoundSnapshot;
RequestOpen_FalseResultCancelsOnceAndPermitsRetry;
RequestOpen_HostSideCancellationBeforeFalseCompletionIsNotDuplicated;
RequestOpen_RollbackOperationThrows_CompletesFalseWithoutSurfacingSecondary;
RequestOpen_SelectorClosesBeforeSuccess_ClosesLatePopupExplicitly;
RequestOpen_SnapshotFailureCancelsOnceAndRetrySucceeds;
RequestOpen_SynchronousAndAsynchronousFaultsAreObserved;
Reset_AfterRelease_PostsNothingAndNeverDetachesOrResetsHost;
ResetReleaseAndCloseResults_PreserveRetryAndBlockReleasedWork;
SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired;
SetDroppedDown_AfterRelease_PostsNothingAndLeavesHostStateUntouched;
SetDroppedDown_MouseAndKeyboardPathsShareRequestAndCloseUncommitted

## Output Summary

The three-class focused filter ran uninstrumented via `/InIsolation` with natural exit 0: Total 50,
Passed 50, Failed 0, Skipped 0. `BreadcrumbPopupBoundaryCoverageTests` reported exactly 23 discovered
and 23 passed (unchanged). `OpenAsync_RecoveryDispatchFails_ReportsOnceAndClearsStoredOpenTask` passed
with its four assertions intact, proving the removed lines were dead. No filter narrowing, case
deletion, or assertion weakening was used.
