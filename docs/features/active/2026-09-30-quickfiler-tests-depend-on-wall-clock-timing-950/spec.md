# 2026-09-30-quickfiler-tests-depend-on-wall-clock-timing (Spec)

- **Issue:** #950
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-01
- **Status:** Ready for planning
- **Version:** 1.0
- **Work Mode:** full-bug (this file is the only acceptance-criteria source; no user-story.md is produced)
- **Research:** research/2026-10-01T00-00-wall-clock-waits-research.md in this feature folder (read-only input; not a write target)

> Formatting convention (do not change): a backticked repository path in this document is a write target. Every file this fix creates or modifies is backticked at least once and is listed under "Files/modules to change". Files that are cited but not modified (comparison files, the fixture, the concurrent-writer test class, the runsettings files, `UiThread`) are written as plain prose without backticks. Line citations such as QfcDatamodel.cs:273 are plain prose for the same reason.

## Context

Several QuickFiler.Test tests fail intermittently under load. They were observed during parallel run bugs-2026-09-28:

- `QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests`, including `RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces`. Two failures stopped the P3-T8 gate of #944, and one failure occurred during the local run of #929.
- `QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores` (referred to below as R4), which failed once in CI on an earlier #929 head.

Research (linked above) established two distinct root causes, both resolved in this issue:

- **Defect A (timing).** The datamodel tests block on real-time bounded waits because the production code starts its background worker on a thread-pool thread that the test cannot control.
- **Defect B (raced static state).** R4 does not wait on time. It fails because a gate-free writer in another test class races the process-wide `UiThread._dispatcher` static under class-level parallelism.

Environment:

- OS/version: Windows 11 (local, with concurrent coverage runs) and windows-latest (CI).
- Language/framework: C# on .NET Framework 4.8.1; MSTest 4.4.1, Moq 4.21.0, FluentAssertions 8.11.0.
- Test regime: the repository runsettings (scripts/vscode/TaskMaster.cli.runsettings, used by Invoke-MSTestWithCoverage.ps1 locally and verbatim by the CI MSTest-with-coverage required check) sets Workers=0 and Scope=ClassLevel. CI therefore runs test classes in parallel, the same as local runs.
- Command: Invoke-MSTestWithCoverage.ps1 under scripts/vscode, and the CI MSTest-with-coverage required check.

Impact / Severity:

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

The affected tests sit on a required CI check and on executor toolchain gates; an intermittent failure stops unrelated work.

## Repro & Evidence

Steps to Reproduce:

1. Run QuickFiler.Test under the repository runsettings (Workers=0, Scope=ClassLevel) while the machine is loaded, for example with a second coverage run in progress.
2. Observe intermittent failures in the tests named in Context.

Expected:

The tests are deterministic regardless of machine load and regardless of which other test classes run concurrently.

Actual:

- QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs uses `SpinWait.SpinUntil(condition, TimeSpan.FromSeconds(5))` at line 56 (inside its `WaitForState` helper, reached from lines 106, 176, 223 and 249) and `Task.Wait(TimeSpan.FromSeconds(5))` at lines 103 and 173. A slow scheduler turns these into assertion failures.
- QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs has the same pattern: `SpinWait.SpinUntil` at line 67 (`WaitForState`, used at line 225) and `Task.Wait(TimeSpan.FromSeconds(5))` at line 220.
- QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs has `Task.Wait(TimeSpan.FromSeconds(5))` at line 161.
- R4 (declared at line 206 of QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs) fails at the `BeSameAs(original)` assertion (lines 244-250) with the message "Expected observedByB to refer to `<null>` ... but found ... Dispatcher { ... Name = "UiThreadDispatcherFixture.ParkedDispatcher" }".

Logs / Evidence:

- #944 evidence for P3-T8 (first run); #929 PR #949 CI history (run 36722780748).
- #823 flake-watch log (docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/flake-watch-uithread-dispatcher-transaction.2026-09-09T00-15.md) records one R4 failure in four runs, all under Workers=0 / ClassLevel, consistent with a class-interleaving race.
- Research verified that every target file is byte-identical between CI head b9692658 and the branch head, so the line citations in this spec describe the code that failed.

## Scope & Non-Goals

Maintainer-approved scope (binding):

1. **Defect A.** The three five-second waits in `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` (`SpinWait.SpinUntil` at line 56 via `WaitForState`, `Task.Wait` at lines 103 and 173) exist because `QfcDatamodel.InitEmailQueue` starts its worker through `BackgroundWorker.RunWorkerAsync()` (QfcDatamodel.cs lines 273 and 300) on an uncontrollable thread-pool thread. Fix with research option D1: an `internal Action<BackgroundWorker> WorkerStarter { get; set; }` seam on `QfcDatamodel`, assigned in both constructors to `worker => worker.RunWorkerAsync()`, mirroring the `RemainingEmailLoader` convention (null on `GetUninitializedObject` instances), replacing both `RunWorkerAsync` call sites. Tests assign a synchronous starter that raises `DoWork` on the calling thread through a test-side `BackgroundWorker` subclass exposing the protected `OnDoWork`, plus a test-owned drainable `SynchronizationContext` where a continuation must be observed after release.
2. **Same root cause, same seam, in scope.** `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs` (waits at line 67 via `WaitForState` used at line 225, and line 220) and `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` (wait at line 161; the two other tests in that file start an unobserved thread-pool worker and must also assign the starter). Research section 2.5 holds the complete `InitEmailQueue` caller inventory.
3. **Defect B.** `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` in `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` (declared at line 206, failing assertion at lines 244-250). Root cause: `UiThreadDispatcherFixture.EnsureDispatcher()` writes the shared `UiThread._dispatcher` static without taking the transaction gate, racing `QfcItemController_FocusAndThemeTests` (`SetThemeDark_FromNormal_SelectsDarkNormalTheme` and `SetThemeLight_FromNormal_SelectsLightNormalTheme` call `EnsureUiThreadDispatcher` and discard the scope) under Workers=0 / ClassLevel. Test-only fix: pin a non-null baseline for the whole R4 body with `using` over `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()`, keep `NotBeSameAs(liveA)`, and update the R4 doc comment (lines 196-202) to state the cause, #950, and the W2/W5 residual-writer invariant.
4. **Prohibited.** Retries, `[DoNotParallelize]`, Workers=1, and longer timeouts. No `Thread.Sleep`, `Task.Delay`, or other wall-clock waits. No temporary files.
5. **Negative controls.** Each rewritten test needs a negative control that fails immediately rather than hanging. The mechanism for each test is stated in Test Strategy.
6. **Ambient context.** The ambient `SynchronizationContext` on the MSTest worker thread is an execution-time assumption to be recorded once during execution, not a blocker.
7. **Production footprint.** The production change is confined to `QuickFiler/Controllers/QfcDatamodel.cs` (483 total lines now; must stay at or below 500 total lines; the type carries `[ExcludeFromCodeCoverage]` at line 25, so the seam is coverage-exempt and must stay minimal). An optional shared test-support pump under QuickFiler.Test/TestSupport/ would need an explicit `<Compile Include>` in QuickFiler.Test.csproj; a private nested helper per file is the alternative (the affected files document a duplication convention).

Out of scope / non-goals (paths deliberately unbackticked because they are not modified):

- QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs. Its discarded scopes are the concurrent writer, but R4's baseline pin makes them harmless, and changing them would not close the race against any future gate-free caller.
- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs. Making `EnsureDispatcher` take the gate is rejected by the fixture's own design note (lines 26-30): callers without `[Timeout]` would block without bound.
- QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs, UtilitiesCS/Threading/UiThread.cs, and both runsettings files.
- The `fake.Advance(...)` plus `await Task.Yield()` loops in the first liveness test (lines 113-116 and 128-132). They read no clock; they are scheduling-dependent but bounded to twenty iterations and are left as they are (research section 2.1).
- Other waits in QuickFiler.Test: the unbounded `WaitOne()` in Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs line 397 (a hang risk, not a wall-clock bound) and the zero-bound `Wait(0)` / `WaitOne(0)` probes in four Viewers files. None is a wall-clock wait.
- `QfcHomeControllerRunAsyncTests` calls `InitEmailQueue` on a `Mock<IQfcDatamodel>` and never reaches `QfcDatamodel`; it is unaffected.
- A dedicated regression test for the Defect B race. MSTest cannot be made to schedule two classes concurrently on demand; the deterministic negative control in Test Strategy demonstrates the failure mode instead.

## Root Cause Analysis

### Defect A: worker start runs on an uncontrolled thread

`QfcDatamodel.InitEmailQueue(0, worker)` (QfcDatamodel.cs lines 259-275) sets `_remainingLoadActive = true` (line 272) and then calls `worker.RunWorkerAsync()` (line 273; the positive-batch path calls it at line 300). `RunWorkerAsync` runs the subscribed `Worker_DoWork` handler (lines 185-229) on a thread-pool thread through delegate `BeginInvoke`. `Worker_DoWork` is `async void`: it calls `RemainingEmailLoader(_token)` at line 205, stores the result in `_remainingLoadTask` at line 206, awaits it at line 207 without `ConfigureAwait(false)`, and clears `_remainingLoadActive` in the `finally` at lines 209-216.

Nothing in the test controls the thread on which that body runs, so each test can only poll for its effects:

| Wait | File and line | Event awaited |
|---|---|---|
| A1 | QfcDatamodelLivenessTests.cs line 56 (`WaitForState`, reached from lines 106, 176, 223, 249) | `!worker.IsBusy` (106, 176) or `_remainingLoadActive == false` (223, 249) |
| A2 | QfcDatamodelLivenessTests.cs line 103 | test-owned TCS set inside the injected `RemainingEmailLoader` |
| A3 | QfcDatamodelLivenessTests.cs line 173 (`StartHeldOpenLoader`, used by tests 2-4) | same as A2 |
| T1 | QfcDatamodelTeardownTests.cs line 67 (`WaitForState`, used at line 225) | `_remainingLoadTask != null` |
| T2 | QfcDatamodelTeardownTests.cs line 220 | loader-entered TCS |
| Z1 | QfcInitEmailQueueZeroBatchTests.cs line 161 | loader-invoked TCS |

The only existing seam is the worker body (`RemainingEmailLoader`, line 140, assigned in both constructors at lines 40 and 51). There is no seam for the worker start, which is the cause of every wait above.

Two further tests in the zero-batch file (`InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` at line 127 and `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` at line 201) do not wait, but each starts a real thread-pool worker whose body runs after the test returns. They share the cause and take the same seam.

### Defect B: gate-free writer races R4's null baseline

R4 reads `original = UiThreadDispatcherFixture.Current` (line 215), installs `liveA` under transaction A (line 216), starts transaction B on a thread-pool continuation, disposes A (line 240), and asserts that B observed `original` (lines 244-250) and did not observe `liveA` (lines 251-257).

The failure message shows `original == null` and B observed the fixture's parked singleton (thread name "UiThreadDispatcherFixture.ParkedDispatcher"). Research enumerated every writer of `UiThread._dispatcher` (W1-W7). Only W1, `UiThreadDispatcherFixture.EnsureDispatcher()`, writes the parked singleton, and only into a null field. W1 is gate-free by design. It is reachable concurrently from exactly one other class, `QfcItemController_FocusAndThemeTests`, whose two theme tests call `EnsureUiThreadDispatcher()` and discard the returned scope.

Under Workers=0 / ClassLevel the seeding can land in either of two windows R4 leaves open:

- Window 1: between the `original` read (line 215) and `Install(liveA)` (line 216). A captures the parked value as its previous value, restores it on dispose, and B observes parked.
- Window 2: between A's restore-to-null on dispose and B's `Current` read (line 230), which runs on a thread-pool continuation. Under load the gap admits a foreign `EnsureDispatcher` call that sees null and writes parked.

Either window produces exactly the observed message. The `NotBeSameAs(liveA)` assertion (the #230 lost-update check) did not fail; `BeSameAs(original)` encodes a property the fixture does not guarantee when the baseline is null. The two-second duration and the sixty-second `[Timeout]` are not involved: no bound expired.

Residual writers after the fix: W2 (`EnsureScope.Dispose()` holding the same parked singleton could null the field; no such foreign disposer exists today) and W5 (`UiThread.Initialize()`, latched once per process, writes the `SyncContextForm` dispatcher, never the parked singleton). Both are recorded as an invariant in the R4 doc comment rather than closed.

## Proposed Fix

### Design summary (what changes where):

- `QuickFiler/Controllers/QfcDatamodel.cs`: add the D1 worker-start seam. The property is `internal Action<BackgroundWorker> WorkerStarter { get; set; }`, assigned in both constructors to `worker => worker.RunWorkerAsync()`. Both call sites in `InitEmailQueue` (lines 273 and 300) call `WorkerStarter(worker)` instead of `worker.RunWorkerAsync()`. Production behavior is unchanged.
- The three datamodel test files assign `model.WorkerStarter` to a synchronous starter. The starter casts the worker to a test-side `SynchronousBackgroundWorker : BackgroundWorker` that exposes the protected virtual `OnDoWork(DoWorkEventArgs)` through a public `RaiseDoWork()` method. `OnDoWork` raises `DoWork` synchronously on the calling thread and so invokes the privately subscribed `Worker_DoWork` without reflection. `InitEmailQueue` then runs to `Worker_DoWork`'s first incomplete await before it returns. Every former wait becomes a synchronous assertion.
- Where a test must observe the continuation after the loader is released (liveness tests 3 and 4), the test installs a test-owned drainable `SynchronizationContext` before invoking `InitEmailQueue`, releases the loader, drains the queue, and asserts synchronously. The context is restored in a `finally` (precedent: `ViewerScope` in ItemViewerBreadcrumbThreadAffinityTests.cs lines 271-292).
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`: R4 wraps its whole body in `using (IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher())` before `BeginTransactionAsync`. The R4 `<para>` (lines 196-202) is rewritten.

### Boundaries and invariants to preserve:

- **Seam contract (one sentence).** `WorkerStarter` is the only path by which `InitEmailQueue` starts a worker; constructed instances start it with `RunWorkerAsync`, and `GetUninitializedObject` instances that do not assign it fail fast with `NullReferenceException` at the start site rather than silently starting a thread.
- **Trace of one value through the seam.** A zero-batch call on a constructed instance: `InitEmailQueue(0, worker)` sets `_remainingLoadActive = true`, calls `WorkerStarter(worker)`, which calls `worker.RunWorkerAsync()`, so `Worker_DoWork` runs on a thread-pool thread exactly as before. The same call on a test instance with the synchronous starter runs `Worker_DoWork` on the test thread up to `await loaderTask` and returns. The same call on an uninitialized instance with no starter assigned throws `NullReferenceException` from `InitEmailQueue`; no caller absorbs it, so the test fails at once. `QfcHomeController` (line 252) only reaches `InitEmailQueue` on constructed instances, so the throw path is unreachable in production.
- The #424 claim of liveness test 1 is preserved: the dequeue keeps polling on `_remainingLoadActive` (QfcDatamodel.QueueProcessing.cs lines 305 and 406), not on `worker.IsBusy`. `_worker` has no `IsBusy` reader, so removing the `IsBusy` waits removes no assertion about production behavior.
- D1 keeps `InitEmailQueue`'s flag-set lines (267-274) under test, which D2 (calling `SetupWorker` directly) would not. D2 is rejected for that reason.
- R4 keeps both assertions. `BeSameAs(original)` stays; with the pin, `original` is the non-null pinned value. `NotBeSameAs(liveA)` stays unchanged.
- Test classes keep running in parallel under Workers=0 / ClassLevel.

### Dependencies or blocked work:

None. The fix is self-contained in QuickFiler and QuickFiler.Test. No package is added: `FakeTimeProvider` (Microsoft.Extensions.TimeProvider.Testing 10.10.0) is already referenced and used.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

- `QuickFiler/Controllers/QfcDatamodel.cs` (production; legacy csproj lists it explicitly, so no project-file change)
- `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`
- `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`
- `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
- `docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md` (acceptance-criteria check-off)

Helper placement decision: the synchronous worker subclass and the drainable context are private nested helpers in each test file that needs them, following the duplication convention those files already document (QfcDatamodelLivenessTests.cs lines 19-23). This keeps the change footprint to the files above. If the plan instead promotes a shared helper to QuickFiler.Test/TestSupport/, it must add the new file and an explicit `<Compile Include>` in QuickFiler.Test.csproj (every file is listed explicitly), and this list must be amended to name both before execution.

#### Functions/classes/CLI commands impacted:

- `QfcDatamodel`: new internal property `WorkerStarter`; both constructors; `InitEmailQueue` (both overload paths that start the worker).
- `QfcDatamodelLivenessTests`: `WaitForState` removed; `DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle`, `StartHeldOpenLoader`, `RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces`, `RemainingLoadActive_AfterLoaderCompletes_BecomesFalse`, `RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally` rewritten.
- `QfcDatamodelTeardownTests`: `WaitForState` removed; `Worker_DoWork_CapturesRemainingLoadTask` rewritten. The other four tests in the file are unchanged. Their `QuiesceLoaderAsync(TimeSpan.FromSeconds(5))` arguments and `fake.Advance(TimeSpan.FromSeconds(6))` are production arguments on a `FakeTimeProvider`, not wall-clock waits, and stay.
- `QfcInitEmailQueueZeroBatchTests`: all three tests assign the starter; `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` loses its bounded wait and its justifying doc comment (lines 136-145) is rewritten.
- `QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores`: baseline pin and doc comment.

#### Data flow and validation changes:

Per-test replacement:

- **Liveness test 1 (A1 at 106, A2).** Synchronous starter; `entered.Task.IsCompleted.Should().BeTrue()` immediately after `InitEmailQueue`; the `!IsBusy` wait is deleted because the worker never starts asynchronously.
- **`StartHeldOpenLoader` (A1 at 176, A3) and liveness test 2.** Same; "flag stays true across the async-void boundary" becomes a synchronous read of `_remainingLoadActive` after `InitEmailQueue` returns.
- **Liveness tests 3 and 4 (A1 at 223 and 249).** Drainable context installed before `InitEmailQueue`; then `release.SetResult(true)`, `pump.Drain()`, and `ReadLivenessFlag(model).Should().BeFalse(...)`. For test 4 the throwing loader's continuation is drained the same way, which faults `loaderTask` and resumes `Worker_DoWork` into the `finally` and then the `catch` at lines 225-228.
- **Teardown T1/T2.** Synchronous starter; `loaderEntered.Task.IsCompleted.Should().BeTrue()` and `GetPrivateField(model, "_remainingLoadTask").Should().NotBeNull()` as synchronous reads. The test asserts nothing after `loaderRelease.TrySetResult(true)`, so no pump is required; the continuation runs inline on the releasing thread.
- **Zero-batch Z1.** The inert loader returns `Task.FromResult(true)` and completes its TCS synchronously, so `loaderInvokedTcs.Task.IsCompleted.Should().BeTrue()` holds when `InitEmailQueue` returns. No pump is needed: `await loaderTask` completes synchronously and the `finally` runs before `InitEmailQueue` returns.
- **Zero-batch Z0/Z2.** Assign the synchronous starter so no worker outlives the test.
- **R4.** Pin as described; with the field non-null, `EnsureDispatcher` from any class installs nothing (fixture lines 130-137), which closes both windows.

#### Error handling and logging updates:

None in production. `Worker_DoWork`'s existing `catch` logs through log4net `logger.Error`, which is a no-op without configuration in tests. The `NullReferenceException` on an unassigned `WorkerStarter` is the intended fail-fast behavior for uninitialized test instances, matching `RemainingEmailLoader`.

#### Rollback/feature-flag considerations (if applicable):

No flag. Rollback is a revert of the branch. The production change is a behavior-preserving indirection.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- `internal Action<BackgroundWorker> WorkerStarter { get; set; }`: input is the worker passed to `InitEmailQueue`; no return value; side effect is starting the worker. Default: `worker => worker.RunWorkerAsync()`. XML doc comment states the test-seam purpose, the constructor default, and the null-on-uninitialized behavior.
- Test-side `SynchronousBackgroundWorker.RaiseDoWork()`: calls `OnDoWork(new DoWorkEventArgs(null))` on the calling thread.
- Test-side drainable context: `Post` enqueues; `Send` throws or runs inline per the existing `DrainableSynchronizationContext` shape; `Drain()` loops only over already-queued callbacks until the queue is empty and never blocks.

#### Required configuration keys and defaults:

None. Runsettings are unchanged.

#### Backward-compatibility expectations:

`WorkerStarter` is internal (QuickFiler.Test already has `InternalsVisibleTo` access, as `RemainingEmailLoader` shows). `IQfcDatamodel` and every public member are unchanged. Production callers observe identical behavior.

#### Performance constraints (latency/throughput/memory):

The rewritten tests complete without blocking on any wait; their duration no longer depends on scheduler latency. Production adds one delegate invocation per `InitEmailQueue` call.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access):
  - The ambient `SynchronizationContext` on the MSTest 4.4.1 worker thread is not statically decidable from the repository. It is an execution-time assumption: the executor records the observed `SynchronizationContext.Current` value on the test thread once during execution, in the feature folder's evidence/other directory. It is not a blocker, because each test that observes a continuation installs and restores its own context.
  - `BackgroundWorker.OnDoWork` is `protected virtual` in .NET Framework 4.8.1 and raises `DoWork` synchronously on the calling thread (research section 2.3).
- Constraints (budget, performance, compatibility):
  - `QuickFiler/Controllers/QfcDatamodel.cs` is 483 total lines and must stay at or below 500 total lines (estimated growth about eight lines). Test files: QfcDatamodelLivenessTests.cs 255, QfcDatamodelTeardownTests.cs 235, QfcInitEmailQueueZeroBatchTests.cs 212, QfcItemController.UiThreadDispatcherFixtureTests.cs 458 total lines; each must stay at or below 500.
  - net481: no `init`, no `record`.
  - No retries, `[DoNotParallelize]`, Workers=1, longer timeouts, `Thread.Sleep`, `Task.Delay`, or temporary files.
  - `QfcDatamodel` carries a type-level `[ExcludeFromCodeCoverage]` (line 25), so the seam adds no coverage denominator and must stay minimal.
- External dependencies (services, libraries, releases): none new.

## Data / API / Config Impact

- User-facing or API changes: none. One internal property is added to `QfcDatamodel`.
- Data or migration considerations: none.
- Logging/telemetry updates (if any): none.
- Compatibility notes (CLI flags, config schemas, versioning): none; runsettings and CI workflow files are unchanged.

## Test Strategy

Seeded from issue (retained):

- Replace the bounded waits with deterministic completion signals or a controllable scheduler (here: synchronous worker start plus a drainable context).
- Use no retries, `[DoNotParallelize]`, Workers=1 or longer timeouts, and find the raced static state (found: `UiThread._dispatcher` via W1).
- Negative control: show that each rewritten test fails when the awaited signal is never set.

Regression tests to update (all in namespace `QuickFiler.Controllers.Tests`):

| Test | Negative-control mechanism (temporary local edit, never committed) | Expected control outcome |
|---|---|---|
| `QfcDatamodelLivenessTests.DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle` | assign a no-op `WorkerStarter` | the loader-entered assertion fails immediately |
| `QfcDatamodelLivenessTests.RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces` | no-op `WorkerStarter` | the entered assertion in `StartHeldOpenLoader` fails immediately |
| `QfcDatamodelLivenessTests.RemainingLoadActive_AfterLoaderCompletes_BecomesFalse` | never set `release`, then `Drain()` | `Drain()` runs zero callbacks; `BeFalse` on the flag fails immediately |
| `QfcDatamodelLivenessTests.RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally` | never set `release`, then `Drain()` | the flag stays true; `BeFalse` fails immediately |
| `QfcDatamodelTeardownTests.Worker_DoWork_CapturesRemainingLoadTask` | no-op `WorkerStarter` | loader-entered and `_remainingLoadTask` assertions fail immediately |
| `QfcInitEmailQueueZeroBatchTests.InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` | no-op `WorkerStarter` | the loader-invoked TCS is incomplete; the assertion fails immediately |
| `QfcInitEmailQueueZeroBatchTests.InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` | remove the `WorkerStarter` assignment | `InitEmailQueue` throws `NullReferenceException` at the start site; the test fails immediately |
| `QfcInitEmailQueueZeroBatchTests.InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` | remove the `WorkerStarter` assignment | same as above |
| `QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores` | on the pre-fix shape (pin removed), insert one discarded `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` call between the `original` read and `Install(liveA)`, and run the test alone by fully-qualified name so the baseline is null | `BeSameAs(original)` fails immediately with the CI message (parked singleton versus null). With the pin restored and the same injected call kept, the test passes, which shows the pin neutralizes the gate-free writer |

No control blocks: `Drain()` loops only over already-queued callbacks, the synchronous starter returns at the first incomplete await, and R4's existing `[Timeout]` is unchanged. A control that hangs instead of failing is itself a defect in the rewrite. The executor records each control's outcome (test name, edit applied, observed failure message, duration) in the feature folder's evidence/other directory as a Markdown summary.

The #230 lost-update control for R4 (a fixture that releases the gate before restoring) remains probabilistic, as the class doc at lines 14-22 already records. This issue does not change that limitation.

- Unit tests (MSTest, Moq, FluentAssertions) for the fixed behavior and boundaries: the nine tests above. `WorkerStarter` itself is exercised by every zero-batch and liveness test through `InitEmailQueue`.
- Edge cases and negative scenarios: unassigned starter on an uninitialized instance (fail fast); loader that throws (test 4); loader never released (negative controls).
- Error handling and logging verification: test 4 covers the `finally`-then-`catch` path.
- Coverage impact and targets for changed lines/modules: `QfcDatamodel` is type-level `[ExcludeFromCodeCoverage]`, so the changed production lines carry no coverage obligation. Test files are excluded from the coverage denominator. No coverage figure changes.
- Toolchain commands to run (format, lint, type-check, test), in order, restarting from step 1 on any failure or auto-fix:
  1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. Invoke-MSTestWithCoverage.ps1 under scripts/vscode (the `test: MSTest with Coverage (Koverage)` task), which uses the repository runsettings with Workers=0 / ClassLevel.
- Committed evidence: Markdown projections only (test-result summary derived from the TRX, coverage summary), per CLAUDE.md "Committed Test Evidence Format". No raw TRX or coverage XML.
- Manual validation steps: none. No live Outlook, UI host, or manual gate is required.

## Acceptance Criteria

- [ ] AC1: `QfcDatamodel` declares an internal `WorkerStarter` property whose type is an Action delegate over `BackgroundWorker`; both constructors assign it a default that calls `RunWorkerAsync` on the supplied worker, and it carries an XML doc comment stating the seam purpose and the null-on-uninitialized behavior.
- [ ] AC2: `InitEmailQueue` contains no direct `RunWorkerAsync` call; both of its worker-start sites (the zero-batch path and the positive-batch path) call `WorkerStarter` instead.
- [ ] AC3: `QuickFiler/Controllers/QfcDatamodel.cs` stays at or below five hundred total lines after the change, measured as total lines rather than non-blank lines.
- [ ] AC4: No `SpinWait.SpinUntil` call, no `Task.Wait` call, and no `WaitForState` helper remains in any of the three datamodel test files (`QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`, `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`, `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`); any surviving `TimeSpan` value in those files is a production argument or a `FakeTimeProvider` advance, not a blocking wait.
- [ ] AC5: No `Thread.Sleep`, `Task.Delay`, `[DoNotParallelize]`, retry construct, or `[Timeout]` value change is introduced by this branch, and neither runsettings file changes.
- [ ] AC6: `QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle` passes under the repository runsettings, driving the worker through a synchronous `WorkerStarter`.
- [ ] AC7: `QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces` passes under the repository runsettings, asserting the liveness flag synchronously after `InitEmailQueue` returns.
- [ ] AC8: `QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_AfterLoaderCompletes_BecomesFalse` passes under the repository runsettings, observing the cleared flag after releasing the loader and draining a test-owned synchronization context.
- [ ] AC9: `QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally` passes under the repository runsettings, observing the cleared flag after releasing the throwing loader and draining a test-owned synchronization context.
- [ ] AC10: `QuickFiler.Controllers.Tests.QfcDatamodelTeardownTests.Worker_DoWork_CapturesRemainingLoadTask` passes under the repository runsettings, reading the loader-entered signal and the captured `_remainingLoadTask` synchronously.
- [ ] AC11: `QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests.InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` passes under the repository runsettings, reading the loader-invoked signal synchronously.
- [ ] AC12: `QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests.InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` and `QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests.InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` both assign a synchronous `WorkerStarter` and pass under the repository runsettings.
- [ ] AC13: `QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores` wraps its whole body in a `using` over `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()`, keeps both its `BeSameAs(original)` and `NotBeSameAs(liveA)` assertions, and passes under the repository runsettings.
- [ ] AC14: The R4 doc comment no longer carries the flake-watch instruction and instead names the gate-free `EnsureDispatcher` writer as the cause, cites this issue, and states the W2/W5 residual-writer invariant.
- [ ] AC15: Each test named in AC6 through AC13 has a recorded negative control, using the mechanism the Test Strategy table assigns to it, that fails immediately with an assertion or exception rather than hanging; each outcome is recorded as a Markdown summary in the feature folder's evidence/other directory.
- [ ] AC16: The observed ambient `SynchronizationContext.Current` value on the MSTest worker thread is recorded once in the feature folder's evidence/other directory.
- [ ] AC17: The full C# toolchain passes in a single pass in order: csharpier check, the analyzers rebuild, the TreatWarningsAsErrors rebuild, and Invoke-MSTestWithCoverage with the repository runsettings unchanged.

## Risks & Mitigations

- Technical or operational risks:
  - A future test class could dispose a scope holding the parked singleton (W2) during R4, or a production path could latch `UiThread.Initialize()` (W5) during R4. Either would make B observe a value other than `original`. Neither exists today.
  - A future test reaching `InitEmailQueue` on an uninitialized instance without assigning `WorkerStarter` throws `NullReferenceException`.
  - If the drainable context is not restored in a `finally`, it leaks to later tests on the same worker thread.
  - The scheduling-dependent `Task.Yield` loops in liveness test 1 remain (out of scope).
- Mitigations and rollbacks:
  - The W2/W5 invariant is written into the R4 doc comment (AC14) so a future editor sees it.
  - The `NullReferenceException` fails the test at once and matches the existing `RemainingEmailLoader` convention; the `WorkerStarter` XML doc states it.
  - Context install/restore follows the `ViewerScope` try/finally precedent.
  - Rollback is a branch revert; production behavior is unchanged.

## Rollout & Follow-up

- Release/rollout steps: merge through the normal PR flow once the required checks pass. No deployment step.
- Post-fix monitoring or clean-up tasks: the #823 flake-watch log for R4 is superseded by this root cause; no further observations are needed. If the out-of-scope `Task.Yield` loops in liveness test 1 later prove flaky, open a separate issue.
- Links: issue #950 (https://github.com/drmoisan/TaskMaster/issues/950); research document linked in the header; related #424 (liveness gate), #230 (lost update), #823 (flake-watch), #929 and #944 (where the failures were observed).
