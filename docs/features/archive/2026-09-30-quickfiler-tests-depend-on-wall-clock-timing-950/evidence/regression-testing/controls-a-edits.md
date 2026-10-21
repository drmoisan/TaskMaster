# Negative-control batch A edit census (P5-T1 to P5-T9)

Timestamp: 2026-10-02T01-10
Command: one pwsh -NoProfile -Command payload after PREFIX: CMD-TOKEN-COUNT on THREE (TOKENS "WorkerStarter = StartSynchronously;", "WorkerStarter = _ => { };", "release.SetResult(true);", "pump.Drain();"; printed as one ROW per file) and CMD-SPAN-TOKEN-COUNT on R4SPAN (TOKENS "EnsureUiThreadDispatcher()", "IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher()"); then git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test and git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test (separate calls).
EXIT_CODE: 0

Edits applied (temporary, against the implementation commit; reverted by P5-T12):
- A1 (test 1, DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle): `model.WorkerStarter = StartSynchronously;` replaced with `model.WorkerStarter = _ => { };`
- A2 (test 3, RemainingLoadActive_AfterLoaderCompletes_BecomesFalse): `release.SetResult(true);` deleted; `pump.Drain();` kept
- A3 (test 4, RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally): `release.SetResult(true);` deleted; `pump.Drain();` kept
- A4 (Worker_DoWork_CapturesRemainingLoadTask): `model.WorkerStarter = StartSynchronously;` replaced with `model.WorkerStarter = _ => { };`
- A5 (Z1, InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker): `model.WorkerStarter = StartSynchronously;` replaced with `model.WorkerStarter = _ => { };`
- A6 (Z0, InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing): `model.WorkerStarter = StartSynchronously;` deleted
- A7 (Z2, InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop): `model.WorkerStarter = StartSynchronously;` deleted
- A8 (R4, Transaction_SecondCallerCannotInstallUntilTheFirstRestores): Delivered Source R-INJECT inserted at twenty spaces immediately before `transactionA.Install(liveA);`, inside the `baseline` using block, after the `original` read

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
Liveness (StartSynchronously, no-op starter, release.SetResult, pump.Drain): 1, 1, 1, 2
Teardown: 0, 1, 0, 0
Zero-batch: 0, 1, 0, 0
R4SPAN SPAN: 212-285; EnsureUiThreadDispatcher() 2; IDisposable baseline = ... 1
numstat:
1	3	QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs
1	1	QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs
1	3	QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs
1	0	QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
porcelain: exactly those four paths with ` M`

All P5-T9 acceptance values hold.
