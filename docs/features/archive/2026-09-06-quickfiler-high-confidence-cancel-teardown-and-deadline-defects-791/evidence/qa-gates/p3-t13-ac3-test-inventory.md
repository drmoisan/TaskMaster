# [P3-T13] AC3 test inventory

Timestamp: 2026-09-06T15-14

AC3 requires that every regression test named in `spec.md` Test Strategy exists in the file listed
for it and passes, and that fail-before/pass-after evidence is recorded for two named tests.

Every result below is read from the `TestResults\791-p2-t14` TRX, which is the [P2-T14] run against
the delivered build (`EXIT_CODE: 0`, 76 of 76 passed). Class names are resolved through the TRX
`TestDefinitions` element rather than the bare `testName`, because `testName` is the method name
alone and one name in this inventory collides across three classes.

## AC1 tests — `spec.md` lines 222-228

`spec.md` names these as new tests in
`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs`. All seven exist in
that file and pass.

| Test named in Test Strategy | File it now lives in | Result |
|---|---|---|
| `DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance` | `QfcStreamingDequeueConfidenceGateTests.Part4.cs` | Passed |
| `DequeueAsync_ZeroAcceptedAndSourceDrained_ReportsSourceExhausted` | `QfcStreamingDequeueConfidenceGateTests.Part4.cs` | Passed |
| `DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached` | `QfcStreamingDequeueConfidenceGateTests.Part4.cs` | Passed |
| `DequeueAsync_ZeroAcceptedAndCeilingReached_StopsWhileSourceStillRefilling` | `QfcStreamingDequeueConfidenceGateTests.Part4.cs` | Passed |
| `DequeueAsync_CheckpointExpiry_LogsCutoffAndCounts` | `QfcStreamingDequeueConfidenceGateTests.Part4.cs` | Passed |
| `DequeueAsync_Launch_LogsCutoffQuantityAndBounds` | `QfcStreamingDequeueConfidenceGateTests.Part4.cs` | Passed |
| `DequeueAsync_NonEmptyPrefix_UnchangedByCheckpoint` | `QfcStreamingDequeueConfidenceGateTests.Part4.cs` | Passed |

## AC1 retargeting obligations — `spec.md` lines 230-234

| Obligation named in Test Strategy | Outcome | File | Result |
|---|---|---|---|
| `...Part3.cs` lines 174-208, `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop` | retargeted to `DequeueAsync_ZeroAcceptedAndCapReached_ReportsScanCapReachedStop` | `QfcStreamingDequeueConfidenceGateTests.Part3.cs` | Passed |
| `QfcQueuePurePathsTests.cs` lines 201-260, `DequeueNextItemGroupWithOutcomeAsync_DeadlineExpiredGate_ReportsDeadlineExpiredStop` | retargeted to `DequeueNextItemGroupWithOutcomeAsync_ZeroAcceptanceCeilingGate_ReportsScanCapReachedStop` | `QfcQueuePurePathsTests.cs` | Passed |
| `...GateTests.cs` lines 27-92, the fail-closed reflection helper updated for the two new optional parameters | `CreateGate` now looks up an eleven-type constructor and keeps its `constructor.Should().NotBeNull(...)` guard | `QfcStreamingDequeueConfidenceGateTests.cs` | exercised by every gate test in the run |
| `QfcHomeControllerIterationTests.cs` gains a sibling pin that `ScanCapReached` also leaves the queue open | `IterateQueueAsync_EmptyBatchWithScanCapReached_DoesNotCompleteAdding` | `QfcHomeControllerIterationTests.cs` | Passed |

The three further retargets D2 identified beyond these four — the four in `...Part2.cs` and the
`DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns` rebase in `...Part3.cs` — are
also all green and are enumerated in `evidence/regression-testing/p2-t14-pass-after.md`.
`DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns` is recorded here explicitly:
`QfcStreamingDequeueConfidenceGateTests.Part3.cs`, Passed.

## AC2 tests — `spec.md` lines 236-239

| Test named in Test Strategy | File named by Test Strategy | File it now lives in | Result |
|---|---|---|---|
| `ActionCancelAsync_ResetsKbdActive_WhenKeyboardDialogActive` | `QfcFormControllerCancelTeardownTests.cs` | same | Passed |
| `ActionCancelAsync_DoesNotToggle_WhenInactive` | `QfcFormControllerCancelTeardownTests.cs` | same | Passed |
| `ActionCancelAsync_ParksFocusAndCancelsBreadcrumbSelectors` | `QfcFormControllerCancelTeardownTests.cs` | same | Passed |
| `ActionCancelAsync_UnregistersHandlersBeforeGroupsCleanup` | `QfcFormControllerCancelTeardownTests.cs` | same | Passed |
| `ActionCancelAsync_AwaitsLoaderQuiesceBeforeGroupsCleanup` | `QfcFormControllerCancelTeardownTests.cs` | same | Passed |
| `ActionCancelAsync_GroupsCleanupThrows_StillInvokesParentCleanup` | `QfcFormControllerCancelTeardownTests.cs` | same | Passed |
| `ButtonCancel_Click_ActionThrows_DoesNotRethrow` | `QfcFormControllerCancelTeardownTests.cs` | same | Passed |
| `Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup` | `QfcHomeControllerCleanupTests.cs` | same | Passed |
| `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` | `QfcHomeControllerCleanupTests.cs` | same | Passed |
| `TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing` | `QfcDatamodelTeardownTests.cs` | same | Passed |
| `QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout` | `QfcDatamodelTeardownTests.cs` | same | Passed |
| `QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs` | `QfcDatamodelTeardownTests.cs` | same | Passed |
| `Cleanup_CalledTwice_DoesNotThrow` | `QfcDatamodelTeardownTests.cs` | same | Passed |

`Cleanup_CalledTwice_DoesNotThrow` is a method name shared by three test classes in this assembly:
`QfcDatamodelTeardownTests`, `EfcItemControllerCleanupTests` and `QfcFormControllerCleanupTests`.
The row above is resolved through the TRX `TestDefinitions` `className` and refers to
`QuickFiler.Controllers.Tests.QfcDatamodelTeardownTests.Cleanup_CalledTwice_DoesNotThrow`, which is
the one AC3 names. The other two are pre-existing tests in other classes and both also passed in the
[P2-T15] whole-assembly run.

Two tests exist in `QfcFormControllerCancelTeardownTests.cs` beyond the seven Test Strategy names —
`ActionCancelAsync_CalledTwice_InvokesParentCleanupOnce` (the D6 repeat-invocation capture pin) — and
one in `QfcDatamodelTeardownTests.cs` — `Worker_DoWork_CapturesRemainingLoadTask` (the loader-task
capture pin). Both passed. They are additions beyond AC3's requirement, not substitutes for it.

"Not proposed: any test of `RibbonController.ReleaseQuickFiler`" is honoured: no such test exists.
The guarantee is asserted at the `ParentCleanup` boundary by
`ActionCancelAsync_GroupsCleanupThrows_StillInvokesParentCleanup` and
`Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup`.

TEST-STRATEGY-NAMES-TOTAL: 26 (7 AC1 new + 4 AC1 retargeting obligations + 1 further retarget recorded explicitly + 13 AC2 named + 1 explicitly not proposed)
NAMES-MAPPED-TO-AN-EXISTING-FILE: 26
NAMES-WITH-A-PASSING-RESULT: 25 (the 26th is the deliberately-not-proposed RibbonController test)

## Required fail-before / pass-after evidence

AC3 requires this pair for two named tests. Both are recorded under this feature folder's
`evidence/regression-testing/` directory:

| Test | Fail-before | Pass-after |
|---|---|---|
| `DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance` | `evidence/regression-testing/p1-t16-gate-fail-before.md` — `ExpectedExitCode: 1`, `EXIT_CODE: 1`, entry 1 of 12, `Expected batch.Accepted to contain a single item ... but the collection is empty.` | `evidence/regression-testing/p2-t14-pass-after.md` — `PASS-AFTER` line present |
| `TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing` | `evidence/regression-testing/p1-t19-datamodel-teardown-fail-before.md` — `ExpectedExitCode: 1`, `EXIT_CODE: 1`, entry 1 of 5, `System.ArgumentException: Delegate to an instance method cannot have null 'this'` (the exact message from the field log) | `evidence/regression-testing/p2-t14-pass-after.md` — `PASS-AFTER` line present |

FAIL-BEFORE-PASS-AFTER-EVIDENCE-COMPLETE: YES

## Determination

Every test name `spec.md` Test Strategy states maps to an existing file and to a passing result in
the [P2-T14] run, and the two required fail-before/pass-after pairs are recorded. AC3 holds.
