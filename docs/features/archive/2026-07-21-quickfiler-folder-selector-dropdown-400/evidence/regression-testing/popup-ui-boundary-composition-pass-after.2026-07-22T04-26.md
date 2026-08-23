# Popup UI-boundary composition pass-after

Timestamp: 2026-07-22T04:26:42.6347167Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests' '/Logger:console;verbosity=normal'`

EXIT_CODE: 0

Output Summary: VSTest discovered and passed 56 of 56 tests in 2.5653 seconds with 0 failures and 0 skips. The exact six-class filter retained the P3 coordinator dispatch guards and P4 correlated-readiness guards and passed the complete P5 composition matrix.

| Required proof | Passing deterministic test |
|---|---|
| Queued creator-thread dispatch with direct operation/thread IDs | `PopupHost_WorkerCompletions_RunOnlyWhenCreatorThreadDrainsBoundary` |
| In-dispatch stale-generation placement rejection | `Placement_StaleCurrentCheck_StopsSubsequentMutations` rows 1-4 and `Reset_WhenRetainIsQueued_RejectsAllStalePlacementAndPublicationWork` |
| Post-show failure native close, closed state, and retry | `PopupHost_FocusFailureAfterShow_NativeClosesThenRetriesClosedSession` |
| Fault-propagating kickoff settles false without hanging | `PopupHost_FirstSchedulingFailure_SettlesFalseThenRetriesAndObservesLifecycle` |
| Exactly one ItemViewer error observation | `MouseToggle_FirstOpenFaultsAfterAwait_SecondClickRetriesCleanly` |
| Host-dispose failure preserves primary error and attempts all resources once | `HostedCleanup_HostDisposeFailure_PreservesPrimaryAndDisposesAllOnce` |
| Observed Close, Reset, and Dispose scheduling | `PopupHost_FirstSchedulingFailure_SettlesFalseThenRetriesAndObservesLifecycle` |
| Mouse and keyboard request equivalence | `SetFolderDroppedDownTrue_UsesSameOpenRequestAsMouseSelectorToggle` |
| Restored retry | `MouseToggle_FirstOpenFaultsAfterAwait_SecondClickRetriesCleanly` and `PopupHost_FocusFailureAfterShow_NativeClosesThenRetriesClosedSession` |

The P4 readiness cases also passed exact navigation correlation, reset/dispose rejection of late success, current-generation-only publication, and viewer replay once per ready surface. No live WebView, display, Outlook, external service, temporary file, sleep, or manual step was used.
