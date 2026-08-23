# P9-T14 nonnumeric adapter focused pass after

Timestamp: 2026-07-27T08-13
Command: Start-Process resolved vstest.console.exe against QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with /InIsolation, detailed console, canonical TRX, and the six-class fully qualified-name selection.
EXIT_CODE: 0

## Run identity

VSTest: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe.
Assembly: QuickFiler.Test/bin/Debug/QuickFiler.Test.dll.
Runner PID: 258660.
TRX: evidence/regression-testing/nonnumeric-adapter-focused.2026-07-27T08-13.trx.

## Result

Total: 60. Passed: 60. Failed: 0. Skipped or other: 0.

The ten new tests were each discovered exactly once and passed:

- QuickFiler.Test.Viewers.BreadcrumbItemViewerLifecycleCoordinatorTests.HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder
- QuickFiler.Test.Viewers.BreadcrumbItemViewerLifecycleCoordinatorTests.CandidateFailure_CleansMessengerAndReadiness
- QuickFiler.Test.Viewers.BreadcrumbItemViewerLifecycleCoordinatorTests.ResetDispose_LateCallbackDoesNotReattach
- QuickFiler.Test.Viewers.BreadcrumbItemViewerLifecycleCoordinatorTests.SelectorDelegation_UsesCoordinator
- QuickFiler.Test.Viewers.BreadcrumbItemViewerLifecycleCoordinatorTests.QueuedGeometryAndFocusGuards_RunOnCreatorThread
- QuickFiler.Test.Viewers.BreadcrumbPopupUiOperationsDirectAdapterTests.CoreProbe_AbsentAndPresentPaths
- QuickFiler.Test.Viewers.BreadcrumbPopupUiOperationsDirectAdapterTests.Initializer_ThrowAndNullTaskPaths
- QuickFiler.Test.Viewers.BreadcrumbPopupUiOperationsDirectAdapterTests.MessengerConstructionFailure_DisposesReadiness
- QuickFiler.Test.Viewers.BreadcrumbPopupUiOperationsDirectAdapterTests.NavigationBinder_TranslatesDetachesAndCleansOnThrow
- QuickFiler.Test.Viewers.BreadcrumbPopupUiOperationsDirectAdapterTests.TwoResourceCleanup_ReportsFirstFailureAfterAllCleanup

## Branch mapping and dispatch proof

- Host subscription identity and messenger replacement order: HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder.
- Candidate cleanup: CandidateFailure_CleansMessengerAndReadiness.
- Reset/dispose late callback invalidation: ResetDispose_LateCallbackDoesNotReattach.
- Selector ownership: SelectorDelegation_UsesCoordinator.
- Geometry and focus guards: QueuedGeometryAndFocusGuards_RunOnCreatorThread.
- Required core, initialization, messenger, navigation, and two-resource cleanup branches: the five PopupUiOperationsDirectAdapterTests listed above.

Both new fixtures use QueuedCreatorThreadSynchronizationContext. DrainOnCreatorThread asserts the current managed thread equals CreatorThreadId before dequeuing callbacks; the geometry/focus test asserts both callback and focus execution on that creator thread. This is explicit queue/drain control, not ambient SynchronizationContext behavior.

## Cleanup

After VSTest completed, runner PID 258660 did not exist, direct descendant count was 0, and live issue-400 QuickFiler.Test vstest.console.exe/testhost.exe process count was 0. No process termination was necessary.

Result: PASS. The passing build artifact is nonnumeric-adapter-focused-build.2026-07-27T08-12.md. Earlier 2026-07-27T07-56 and 2026-07-27T08-02 build artifacts and the 2026-07-27T08-04 focused failure/TRX remain historical.
