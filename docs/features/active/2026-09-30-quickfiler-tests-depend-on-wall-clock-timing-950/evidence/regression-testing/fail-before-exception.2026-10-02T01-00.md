# Fail-before exception dossier: Defect A (P1-T8)

Timestamp: 2026-10-02T01-00
Defect: A (wall-clock waits on a thread-pool worker)

WhyFailingRunImpossible: The Defect A failure occurs only when the thread pool delays the BackgroundWorker body (Worker_DoWork, started by RunWorkerAsync) past the five-second bound of SpinWait.SpinUntil or Task.Wait in the pre-fix tests. No deterministic, policy-compliant test can force that delay: sleeps, delays and wall-clock waits are prohibited, and starving the thread pool would change process-wide state that other test classes running in parallel under Workers=0 / ClassLevel share.

## Alternative proof

1. Pre-change census (FEATURE/evidence/baseline/census-baseline.md, P0-T12): `SpinWait` 1 (liveness) and 1 (teardown); `.Wait(` 2 (liveness), 1 (teardown) and 1 (zero-batch); `WaitForState` 5 (liveness) and 2 (teardown); `TIMESPAN-UNCLASSIFIED: 6` (liveness 56, 103, 173; teardown 67, 220; zero-batch 161). Every one of these is a bounded real-time wait on the thread-pool worker.
2. The two direct start sites: QuickFiler/Controllers/QfcDatamodel.cs lines 273 and 300 each call `worker.RunWorkerAsync();` (`TRIMMED-EQUAL [worker.RunWorkerAsync();] = 2`, `INITQ` `RunWorkerAsync` 2), and `WorkerStarter` is absent (`TOKEN [WorkerStarter] = 0`), so no test can control the thread on which the worker body runs (P0-T12).
3. Recorded failure history (spec Context): two failures stopped the #944 P3-T8 gate, and one failure occurred during the #929 local run, both in QfcDatamodelLivenessTests under the repository runsettings on a loaded machine.
4. Forward statement: P5-T24 appends a "Post-fix deterministic controls" section to this dossier, recording the ten post-fix negative controls (A1 to A7, B1, C1, C2), each of which fails at once when its signal is withheld.
