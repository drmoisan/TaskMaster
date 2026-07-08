# Post-Fix Test-Run Evidence and Blocking Finding (Issue #244, P2-T1)

Timestamp: 2026-07-06T12-10

Command: & "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~InitEmailQueue_ZeroBatchSize|FullyQualifiedName~InitEmailQueue_PositiveBatchSize"

(Note: `|` used as the OR operator per the tooling note recorded in `evidence/baseline/baseline-test-filter.md`.)

## Actual result (NOT the acceptance-line target)

EXIT_CODE: 1 (reproduced identically across 5 consecutive runs of the exact command above, after the fix in `QfcDatamodel.cs` was applied and the solution rebuilt)

Output Summary:
- `Passed InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` — consistently Passed (5/5 runs).
- `Failed InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` — consistently Failed (5/5 runs) with `Expected worker.IsBusy to be True, but found False.`
- `Passed InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` — consistently Passed (5/5 runs).
- Total tests: 3, Passed: 2, Failed: 1 (all 5 runs).

This does **not** match the P2-T1 acceptance line ("all three tests now pass ... 3 passed, 0 failed"). This is reported honestly rather than re-run until a lucky pass, per the repository's determinism policy (`.claude/rules/general-unit-test.md` UT1: "Given the same inputs and environment, tests must produce the same results. Avoid flakiness.") and the C# policy's prohibition on masking flaky behavior via retries.

## Root-cause diagnosis (production fix is correct; the test's synchronization assumption is not)

`QfcDatamodel.Worker_DoWork` is declared `private async void Worker_DoWork(object sender, DoWorkEventArgs e)`. `BackgroundWorker`'s internal `WorkerThreadStart` invokes the `DoWork` event delegate synchronously and, immediately after that call **returns control** (which for an `async void` method happens at its first genuinely-asynchronous `await`, not at logical completion of the method body), treats the work as finished and raises `RunWorkerCompleted`, which resets `isRunning` (and therefore `BackgroundWorker.IsBusy`) back to `false`. Inside `Worker_DoWork`, the call chain `await LoadRemainingEmailsToQueueAsync(_token)` → `await Task.Run(() => _frame.GetRowsAs<IEmailSortInfo>().Values.ToArray())` hits a genuinely-asynchronous await almost immediately, so `Worker_DoWork` returns control back to `BackgroundWorker`'s internal completion logic well before the real background work has actually finished. This races directly against the test's own thread, which (per the plan's stated design: "no synchronization on the worker's asynchronous DoWork completion is required or performed") checks `worker.IsBusy` synchronously, with no wait, immediately after `InitEmailQueue` returns.

Empirically:
- Run in isolation (single-test filter, `Total tests: 1`), this same assertion **passed 3/3 times**, because a cold `ThreadPool` has measurable dispatch latency before `Delegate.BeginInvoke`'s queued work item actually starts executing, giving the calling (test) thread enough of a head start to observe `IsBusy == true` before the race resolves.
- Run as the second test in the specified 3-test filter (immediately after `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`, which itself exercises the same `RunWorkerAsync`/`async void DoWork` machinery and leaves the process's `ThreadPool` warmed with an idle worker thread), the same assertion **failed 5/5 times**, because the already-warm `ThreadPool` thread picks up and races through the DoWork handler fast enough to reset `IsBusy` to `false` before the test's very next line executes.

This is a `BackgroundWorker` + `async void` DoWork anti-pattern (a well-known .NET pitfall: `BackgroundWorker` cannot observe true completion of an `async void` handler) that exists in **pre-existing, unmodified production code** — `SetupWorker`/`Worker_DoWork`/`RunWorkerAsync()` are shared verbatim by both the pre-existing `batchSize > 0` path and the new `batchSize <= 0` guard added by this fix; neither this fix nor any other change in this plan altered that machinery. The fix under test (`InitEmailQueue`'s new `batchSize <= 0` guard) is not implicated: `worker.WorkerSupportsCancellation.Should().BeTrue()` (the synchronous, non-racy half of the same assertion, proving `SetupWorker` really did run) passes reliably in every run, and `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` (AC1) and `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` (AC3) both pass reliably and demonstrate the guard is otherwise correct.

## AC impact assessment

- **AC1** (`InitEmailQueue(0, worker)` returns empty, non-null, no throw): reliably demonstrated green (5/5 runs).
- **AC2** (worker is set up and started): the `WorkerSupportsCancellation` half of the proof is reliably green (5/5 runs); the `IsBusy` half of the proof, as specified, is not reliably reproducible and is a test-design defect, not a production regression — `SetupWorker(worker); worker.RunWorkerAsync();` are unconditionally reached and executed by the new guard branch (visible directly in the `QfcDatamodel.cs` diff and corroborated by `WorkerSupportsCancellation` passing).
- **AC3** (`batchSize > 0` retains existing behavior): reliably demonstrated green (5/5 runs).
- **AC4** (deterministic regression test reproduces red before / green after): satisfied for `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` (red confirmed in `fail-before-InitEmailQueue-zero-batch.2026-07-06T11-26.md`, green confirmed here) and for the positive-batch characterization test. **Not** satisfied for `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`: red before the fix was confirmed (`fail-before-InitEmailQueue-worker-start.2026-07-06T11-26.md`), but the post-fix state is not deterministically green as specified — it is racy, per the diagnosis above.

## Escalation (plan-delta recommendation, not applied by this executor)

This executor did not modify the test method's assertions or add any wait/sleep/retry to mask the race (`.claude/rules/csharp.md` "Prohibited Behaviors: Adding sleeps, retries, or timing hacks to mask flaky behavior"). A plan revision from `atomic-planner` is recommended to replace the `worker.IsBusy.Should().BeTrue()` assertion with a deterministic proof of `RunWorkerAsync` invocation that does not depend on `BackgroundWorker`'s (unreliable, for `async void` handlers) completion-tracking — for example, asserting only `WorkerSupportsCancellation` plus a reflection-based check that `worker.DoWork`'s invocation list is non-empty (proving `SetupWorker` wired the handler) and a call-count spy proving `RunWorkerAsync` was invoked, rather than reading `IsBusy` after the call.

[P2-T1] is left unchecked in the plan pending this revision; [P1-T2], [P1-T3], [P1-T4], and [P1-T5] evidence stands as separately and correctly recorded (each of those tasks' own acceptance criteria — demonstrating the pre-fix red state — were met and are unaffected by this finding).
