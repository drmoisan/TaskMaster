# Fail-before exception dossier: Defect A (P1-T8)

Timestamp: 2026-10-02T01-00
Defect: A (wall-clock waits on a thread-pool worker)

WhyFailingRunImpossible: The Defect A failure occurs only when the thread pool delays the BackgroundWorker body (Worker_DoWork, started by RunWorkerAsync) past the five-second bound of SpinWait.SpinUntil or Task.Wait in the pre-fix tests. No deterministic, policy-compliant test can force that delay: sleeps, delays and wall-clock waits are prohibited, and starving the thread pool would change process-wide state that other test classes running in parallel under Workers=0 / ClassLevel share.

## Alternative proof

1. Pre-change census (FEATURE/evidence/baseline/census-baseline.md, P0-T12): `SpinWait` 1 (liveness) and 1 (teardown); `.Wait(` 2 (liveness), 1 (teardown) and 1 (zero-batch); `WaitForState` 5 (liveness) and 2 (teardown); `TIMESPAN-UNCLASSIFIED: 6` (liveness 56, 103, 173; teardown 67, 220; zero-batch 161). Every one of these is a bounded real-time wait on the thread-pool worker.
2. The two direct start sites: QuickFiler/Controllers/QfcDatamodel.cs lines 273 and 300 each call `worker.RunWorkerAsync();` (`TRIMMED-EQUAL [worker.RunWorkerAsync();] = 2`, `INITQ` `RunWorkerAsync` 2), and `WorkerStarter` is absent (`TOKEN [WorkerStarter] = 0`), so no test can control the thread on which the worker body runs (P0-T12).
3. Recorded failure history (spec Context): two failures stopped the #944 P3-T8 gate, and one failure occurred during the #929 local run, both in QfcDatamodelLivenessTests under the repository runsettings on a loaded machine.
4. Forward statement: P5-T24 appends a "Post-fix deterministic controls" section to this dossier, recording the ten post-fix negative controls (A1 to A7, B1, C1, C2), each of which fails at once when its signal is withheld.

## Post-fix deterministic controls

Appended by P5-T24 at 2026-10-02T01-16. Sources: FEATURE/evidence/regression-testing/negative-controls-batch-a.md (P5-T11), FEATURE/evidence/regression-testing/negative-controls-batch-b.md (P5-T16), FEATURE/evidence/regression-testing/negative-controls-batch-c.md (P5-T22).

| Control | Test | Signal withheld | Outcome | Duration | Source |
|---|---|---|---|---|---|
| A1 | DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle | no-op WorkerStarter | Failed | 00:00:00.1859136 | negative-controls-batch-a.md |
| A2 | RemainingLoadActive_AfterLoaderCompletes_BecomesFalse | release never set, then Drain() | Failed | 00:00:00.0085546 | negative-controls-batch-a.md |
| A3 | RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally | release never set, then Drain() | Failed | 00:00:00.0017485 | negative-controls-batch-a.md |
| A4 | Worker_DoWork_CapturesRemainingLoadTask | no-op WorkerStarter | Failed | 00:00:00.1862274 | negative-controls-batch-a.md |
| A5 | InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker | no-op WorkerStarter | Failed | 00:00:00.0077444 | negative-controls-batch-a.md |
| A6 | InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing | WorkerStarter assignment removed | Failed | 00:00:00.1859851 | negative-controls-batch-a.md |
| A7 | InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop | WorkerStarter assignment removed | Failed | 00:00:00.2862076 | negative-controls-batch-a.md |
| B1 | RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces | no-op WorkerStarter in StartHeldOpenLoader | Failed | 00:00:00.2592311 | negative-controls-batch-b.md |
| C1 | RemainingLoadActive_AfterLoaderCompletes_BecomesFalse | Drain() removed (loader released) | Failed | 00:00:00.1620202 | negative-controls-batch-c.md |
| C2 | RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally | Drain() removed (loader released) | Failed | 00:00:00.0021400 | negative-controls-batch-c.md |

Each rewritten test fails at once (every duration is under 0.3 seconds) when its signal is withheld, with an assertion or a NullReferenceException and no hang. The pre-fix tests could show the same failure only by waiting out a five-second bound on a thread-pool worker.
