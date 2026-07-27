# P9-T14 nonnumeric adapter focused pass after

Timestamp: 2026-07-27T08-36
Command: Start-Process resolved vstest.console.exe against QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with /InIsolation, detailed console, canonical TRX, and the required six-class fully qualified-name selection.
EXIT_CODE: 0

TRX: evidence/regression-testing/nonnumeric-adapter-focused.2026-07-27T08-36.trx.
Total: 60. Passed: 60. Failed: 0. Skipped or other: 0.

The ten named P9-T13 tests were each discovered once and passed: HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder; CandidateFailure_CleansMessengerAndReadiness; ResetDispose_LateCallbackDoesNotReattach; SelectorDelegation_UsesCoordinator; QueuedGeometryAndFocusGuards_RunOnCreatorThread; CoreProbe_AbsentAndPresentPaths; Initializer_ThrowAndNullTaskPaths; MessengerConstructionFailure_DisposesReadiness; NavigationBinder_TranslatesDetachesAndCleansOnThrow; TwoResourceCleanup_ReportsFirstFailureAfterAllCleanup.

QueuedCreatorThreadSynchronizationContext is explicitly drained on its creator thread; the geometry/focus test asserts callback and focus thread identity. Runner PID 245472 exited; direct descendants 0; live issue-400 vstest.console.exe/testhost.exe processes 0. No termination was necessary.

Result: PASS. Build freshness evidence is nonnumeric-adapter-focused-build.2026-07-27T08-35.md. Earlier P9-T14 artifacts remain historical.
