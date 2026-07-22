# P5 dispose-race pass-after

Timestamp: 2026-07-22T06:29:27.2372956Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests'`

EXIT_CODE: 0

Output Summary: VSTest 18.8.0 discovered and passed all 12 tests in the two plan-defined P5-T41 classes in 2.3100 seconds with 0 failures and 0 skips. Passing results were `WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary`, `PopupHost_WorkerCompletions_RunOnlyWhenCreatorThreadDrainsBoundary`, `PopupHost_FocusFailureAfterShow_NativeClosesThenRetriesClosedSession`, `PopupHost_FirstSchedulingFailure_SettlesFalseThenRetriesAndObservesLifecycle`, `MouseToggle_FirstOpenFaultsAfterAwait_SecondClickRetriesCleanly`, `SetFolderDroppedDownTrue_UsesSameOpenRequestAsMouseSelectorToggle`, all four `Placement_StaleCurrentCheck_StopsSubsequentMutations` data rows, `HostedCleanup_HostDisposeFailure_PreservesPrimaryAndDisposesAllOnce`, and `Dispose_WhenResetAndOpenWorkAreQueued_HasNoLateActivity`. The results verify scheduling-failure false completion and retry, focus-failure rollback to authoritative closed state, Reset/Dispose invalidation of queued work, no late UI/error/native-close/cancel/focus callbacks, exactly-once owned-resource disposal, synchronized queue/error snapshots, and creator-thread-only draining.
