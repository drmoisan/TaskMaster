# QA Gate — Phase 4 Test File Line Counts (P4-T7)

Timestamp: 2026-06-28T20-15
Command: wc -l <touched test files>

| Test file | Lines | <= 500 |
|-----------|-------|--------|
| QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs | 421 | yes |
| QuickFiler.Test/Controllers/QfcDatamodelTests.cs | 276 | yes |

Both touched test files remain under the 500-line limit; no split required.

New seam tests added (all passing):
- QfcHomeControllerMetricsTests: WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps (P4-T1), QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine (P4-T2), NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay (P4-T3).
- QfcDatamodelTests: ToggleOfflineMode_WhenOnline_AwaitsInjectedFiveMillisecondDelay (P4-T4), WaitForQueue_WhenWorkerBusyAndQueueShort_AwaitsInjectedTwoHundredMsDelay (P4-T5).
- P4-T6 LaunchAsync: documented scope-exclusion dossier at evidence/regression-testing/launchasync-test-scope.md (COM-isolation not deterministically feasible; binary outcome = dossier written).
