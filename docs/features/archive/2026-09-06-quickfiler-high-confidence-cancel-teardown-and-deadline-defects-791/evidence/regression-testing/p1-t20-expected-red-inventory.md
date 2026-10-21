# [P1-T20] Expected-red inventory at the end of Phase 1

Timestamp: 2026-09-06T14-48

This is the complete set of tests that are red at the end of Phase 1, consolidated from the four
fail-before artifacts. It is the set Phase 2 must turn green, and nothing else. Every entry carries
one of the three tags `NEW`, `RETARGETED` or `SEAM-BLOCKED`.

## Count reconciliation

| Source task | Artifact | Failure count |
|---|---|---|
| [P1-T16] | `p1-t16-gate-fail-before.md` | 12 |
| [P1-T17] | `p1-t17-cancel-teardown-fail-before.md` | 6 |
| [P1-T18] | `p1-t18-home-cleanup-fail-before.md` | 2 |
| [P1-T19] | `p1-t19-datamodel-teardown-fail-before.md` | 5 |
| **Sum of the four recorded failure counts** | | **25** |
| **Entries in this inventory** | | **25** |

INVENTORY-COUNT: 25
SUM-OF-FAIL-BEFORE-COUNTS: 25
RECONCILES: YES

## Inventory

All names below are in namespace `QuickFiler.Controllers.Tests`.

### From [P1-T16] — 12 entries

| # | Tag | Class | Method |
|---|---|---|---|
| 1 | NEW | `QfcStreamingDequeueConfidenceGateTests` | `DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance` |
| 2 | NEW | `QfcStreamingDequeueConfidenceGateTests` | `DequeueAsync_ZeroAcceptedAndSourceDrained_ReportsSourceExhausted` |
| 3 | NEW | `QfcStreamingDequeueConfidenceGateTests` | `DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached` |
| 4 | NEW | `QfcStreamingDequeueConfidenceGateTests` | `DequeueAsync_ZeroAcceptedAndCeilingReached_StopsWhileSourceStillRefilling` |
| 5 | NEW | `QfcStreamingDequeueConfidenceGateTests` | `DequeueAsync_CheckpointExpiry_LogsCutoffAndCounts` |
| 6 | NEW | `QfcStreamingDequeueConfidenceGateTests` | `DequeueAsync_Launch_LogsCutoffQuantityAndBounds` |
| 7 | RETARGETED | `QfcStreamingDequeueConfidenceGateTests` | `DequeueAsync_LowYieldStream_ContinuesPastDefaultDeadlineToTheQualifier` |
| 8 | RETARGETED | `QfcStreamingDequeueConfidenceGateTests` | `DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesToSourceExhaustion` |
| 9 | RETARGETED | `QfcStreamingDequeueConfidenceGateTests` | `DequeueAsync_AfterScanCapReached_StopsTakingAndLeavesUnscannedCandidates` |
| 10 | RETARGETED | `QfcStreamingDequeueConfidenceGateTests` | `DequeueAsync_CheckpointExpiry_EmitsCheckpointLineAndKeepsPerCandidateLogging` |
| 11 | RETARGETED | `QfcStreamingDequeueConfidenceGateTests` | `DequeueAsync_ZeroAcceptedAndCapReached_ReportsScanCapReachedStop` |
| 12 | RETARGETED | `QfcQueuePurePathsTests` | `DequeueNextItemGroupWithOutcomeAsync_ZeroAcceptanceCeilingGate_ReportsScanCapReachedStop` |

### From [P1-T17] — 6 entries

| # | Tag | Class | Method |
|---|---|---|---|
| 13 | NEW | `QfcFormControllerCancelTeardownTests` | `ActionCancelAsync_ResetsKbdActive_WhenKeyboardDialogActive` |
| 14 | NEW | `QfcFormControllerCancelTeardownTests` | `ActionCancelAsync_ParksFocusAndCancelsBreadcrumbSelectors` |
| 15 | NEW | `QfcFormControllerCancelTeardownTests` | `ActionCancelAsync_UnregistersHandlersBeforeGroupsCleanup` |
| 16 | NEW | `QfcFormControllerCancelTeardownTests` | `ActionCancelAsync_AwaitsLoaderQuiesceBeforeGroupsCleanup` |
| 17 | NEW | `QfcFormControllerCancelTeardownTests` | `ActionCancelAsync_GroupsCleanupThrows_StillInvokesParentCleanup` |
| 18 | NEW | `QfcFormControllerCancelTeardownTests` | `ButtonCancel_Click_ActionThrows_DoesNotRethrow` |

### From [P1-T18] — 2 entries

| # | Tag | Class | Method |
|---|---|---|---|
| 19 | NEW | `QfcHomeControllerCleanupTests` | `Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup` |
| 20 | NEW | `QfcHomeControllerCleanupTests` | `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` |

### From [P1-T19] — 5 entries

| # | Tag | Class | Method |
|---|---|---|---|
| 21 | NEW | `QfcDatamodelTeardownTests` | `TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing` |
| 22 | SEAM-BLOCKED | `QfcDatamodelTeardownTests` | `QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout` |
| 23 | SEAM-BLOCKED | `QfcDatamodelTeardownTests` | `QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs` |
| 24 | NEW | `QfcDatamodelTeardownTests` | `Cleanup_CalledTwice_DoesNotThrow` |
| 25 | SEAM-BLOCKED | `QfcDatamodelTeardownTests` | `Worker_DoWork_CapturesRemainingLoadTask` |

## Tag definitions used here

- **NEW** — a test this plan created that asserts behaviour Phase 2 supplies. It is red because the
  production behaviour does not exist yet.
- **RETARGETED** — a pre-existing test whose assertion was rewritten against the superseding
  behaviour. It was green at `BASE-SHA` under its old name or old assertion ([P0-T14] recorded all
  seven) and is red now.
- **SEAM-BLOCKED** — red because it reads or writes `QfcDatamodel._remainingLoadTask`, a field
  [P2-T4] adds. Its fail-closed reflective field lookup fires during Arrange, before the assertion
  the test exists for is reached. See the divergence note in
  `p1-t19-datamodel-teardown-fail-before.md`.

## Tests deliberately NOT in this inventory

Three tests touched by Phase 1 are green at the end of Phase 1 and must stay green. They are listed
so a reader does not read their absence as an omission:

- `QfcStreamingDequeueConfidenceGateTests.DequeueAsync_NonEmptyPrefix_UnchangedByCheckpoint` (NEW) —
  the #608 regression pin; it asserts behaviour this change must not alter.
- `QfcStreamingDequeueConfidenceGateTests.DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns`
  (RETARGETED) — its bound moved from a 3 s deadline to a cap of 3, which admits the same three
  candidates at one second per score.
- `QfcFormControllerCancelTeardownTests.ActionCancelAsync_DoesNotToggle_WhenInactive` and
  `...ActionCancelAsync_CalledTwice_InvokesParentCleanupOnce` (both NEW) — a negative control and a
  D6 capture pin respectively.
