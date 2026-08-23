Timestamp: 2026-08-04T20:48:00-04:00
Command: N/A — historical source-backed requirements-to-evidence mapping; no command was run for this mapping artifact.
EXIT_CODE: N/A — no command was run.
Output Summary: This historical mapping marked AC1-AC6 and CR-001-CR-006 PASS while retaining AC7, AC8, and CR-007 as pending; it is superseded by the cycle-3/pass-3 controlling mapping.
P5-T1 evidence: remediation-targeted-regressions-pass.2026-08-04T20-47.md
Diff inspection: git diff origin/main -- '*.cs'. No changed production live-traversal path contains Task.Yield, a worker-local WPF dispatcher, or caller-selected traversal fallback. Dispatcher.CurrentDispatcher additions are confined to deterministic STA test hosts. WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict passed.

| Requirement | Independent passing evidence | Status |
| --- | --- | --- |
| AC1 | UtilitiesCS.Test: OutlookFolderTreeServiceConcurrencyTests.GetSnapshotAsync_WorkerOriginatedColdBuild_UsesCapturedStaDispatcher. | PASS |
| AC2 | TaskMaster.Test: AppOlObjectsFolderTreeServiceTests.FolderTreeService_WorkerFirstAccess_ComposesOnCapturedStaDispatcher; UtilitiesCS.Test: OutlookFolderTreeServiceInvalidationTests.NotificationRefresh_RunsOnCapturedDispatcher; OutlookFolderTreeServiceDisposalTests.NotificationRefreshAndDispose_RunOnTheCapturedDispatcher; OutlookFolderHierarchyReaderTests.ReadRecordsAsync_AfterForcedYield_KeepsFolderAccessOnDispatcher; FolderTreeSnapshotBuilderYieldTests.BuildSnapshotAsync_AfterForcedYield_KeepsSubsequentYieldsOnDispatcher. | PASS |
| AC3 | UtilitiesCS.Test: WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict; complete changed-production C# diff inspection. | PASS |
| AC4 | UtilitiesCS.Test: OutlookFolderTreeServiceConcurrencyTests.GetSnapshotAsync_ConcurrentInitialRequests_CoalesceOntoOneBuild; OutlookFolderTreeServiceInvalidationTests.FolderChanged_DuringInFlightBuild_SchedulesOneFollowUpRefresh and NotificationRefresh_RunsOnCapturedDispatcher; OutlookFolderTreeServiceStateTests; OutlookFolderTreeServiceDisposalTests.Dispose_DuringBuild_LeavesDisposedWithoutPublicationOrNotification and Dispose_DuringRefresh_LeavesDisposedWithoutPublicationOrNotification. | PASS |
| AC5 | UtilitiesCS.Test: FilterOlFoldersControllerInitializationTests.CreateAsync_WiresViewerOnlyAfterSnapshotCompletes, CreateAsync_ClosedBeforeSnapshotCompletes_DoesNotWireViewerOrRetainHandler, CreateAsync_SnapshotFault_PropagatesAndLeavesViewerUnwired, InjectedViewerConstructor_Readiness_PropagatesSnapshotFaultAndLeavesViewerUnwired, and CreateAsync_SnapshotFault_WhenViewerCloseFails_PropagatesSnapshotFault; TaskMaster.Test: TryFunctionalityInConstructionTests.TryLoadFolderFilterAsync_AwaitsControlledInitialization and TryLoadFolderFilter_PropagatesControlledInitializationFault. | PASS |
| AC6 | P5-T1 deterministic in-process MSTest suite passed without Outlook, network, temporary files, sleeps, retry loops, real viewer, or message loop. | PASS |
| AC7 | Final C# formatter, analyzer, nullable, coverage, comparable denominator, and threshold evidence has not yet been run in Phase 6. | PENDING FINAL QA |
| AC8 | Final specification and inventory reconciliation has not yet been completed in Phase 7. | PENDING FINAL DOCUMENTATION |

| Review finding | Independent passing evidence | Status |
| --- | --- | --- |
| CR-001 | AppOlObjectsFolderTreeServiceTests.FolderTreeService_WorkerComposition_DisposeDoesNotWaitForDispatcherWork and worker-first composition regression. | PASS |
| CR-002 | OutlookFolderTreeServiceDisposalTests.Dispose_DuringBuild_LeavesDisposedWithoutPublicationOrNotification and Dispose_DuringRefresh_LeavesDisposedWithoutPublicationOrNotification. | PASS |
| CR-003 | OutlookFolderTreeServiceDisposalTests.NotificationRefreshAndDispose_RunOnTheCapturedDispatcher. | PASS |
| CR-004 | FilterOlFoldersControllerInitializationTests close-before-completion and the three snapshot-fault regressions, including deterministic close/dispose cleanup. | PASS |
| CR-005 | TryFunctionalityInConstructionTests.TryLoadFolderFilterAsync_AwaitsControlledInitialization and TryLoadFolderFilter_PropagatesControlledInitializationFault. | PASS |
| CR-006 | WpfUiDispatcherTests.InvokeAsync_AsyncFunction_ReturnsResultFromCapturedDispatcher, InvokeAsync_AsyncFunction_PropagatesOriginalFault, and InvokeAsync_CanceledBeforeDispatch_DoesNotExecuteAction. | PASS |
| CR-007 | Comparable final coverage and the documented 90 percent per-new-method assessment are pending P6-T4 and P6-T5. | PENDING FINAL QA |

Reconciliation result: remediation remains required until Phase 6 produces passing comparable coverage evidence and Phase 7 reconciles the source specification. No forbidden traversal fallback was found.
