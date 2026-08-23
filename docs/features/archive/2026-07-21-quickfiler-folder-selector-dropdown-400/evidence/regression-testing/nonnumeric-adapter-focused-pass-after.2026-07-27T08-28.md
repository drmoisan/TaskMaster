# P9-T14 nonnumeric adapter focused pass after

Timestamp: 2026-07-27T08-28
Command: Start-Process resolved vstest.console.exe against QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with /InIsolation, detailed console, canonical TRX, and the required six-class fully qualified-name selection.
EXIT_CODE: 0

## Result

TRX: evidence/regression-testing/nonnumeric-adapter-focused.2026-07-27T08-28.trx.
Total: 60. Passed: 60. Failed: 0. Skipped or other: 0.

The following ten P9-T13 tests were each discovered exactly once and passed:

- BreadcrumbItemViewerLifecycleCoordinatorTests.HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder
- BreadcrumbItemViewerLifecycleCoordinatorTests.CandidateFailure_CleansMessengerAndReadiness
- BreadcrumbItemViewerLifecycleCoordinatorTests.ResetDispose_LateCallbackDoesNotReattach
- BreadcrumbItemViewerLifecycleCoordinatorTests.SelectorDelegation_UsesCoordinator
- BreadcrumbItemViewerLifecycleCoordinatorTests.QueuedGeometryAndFocusGuards_RunOnCreatorThread
- BreadcrumbPopupUiOperationsDirectAdapterTests.CoreProbe_AbsentAndPresentPaths
- BreadcrumbPopupUiOperationsDirectAdapterTests.Initializer_ThrowAndNullTaskPaths
- BreadcrumbPopupUiOperationsDirectAdapterTests.MessengerConstructionFailure_DisposesReadiness
- BreadcrumbPopupUiOperationsDirectAdapterTests.NavigationBinder_TranslatesDetachesAndCleansOnThrow
- BreadcrumbPopupUiOperationsDirectAdapterTests.TwoResourceCleanup_ReportsFirstFailureAfterAllCleanup

Both fixtures use explicit QueuedCreatorThreadSynchronizationContext queue/drain control; DrainOnCreatorThread asserts the creator managed thread before callback execution, and the geometry/focus case asserts callback and focus thread identity.

Cleanup proof: runner PID 263964 no longer exists; direct descendants 0; live issue-400 QuickFiler.Test vstest.console.exe/testhost.exe processes 0. No termination was necessary.

Result: PASS. The fresh build/freshness evidence is nonnumeric-adapter-focused-build.2026-07-27T08-27.md. Earlier P9-T14 build, TRX, and failure/pass artifacts remain historical.
