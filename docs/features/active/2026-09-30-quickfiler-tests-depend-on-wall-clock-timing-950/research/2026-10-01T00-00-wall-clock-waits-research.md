# Research: QuickFiler.Test wall-clock waits and the R4 transaction flake (Issue #950)

> **Status: complete.** The first pass stopped for quota with the items in section 7 open; the orchestrator closed them on 2026-10-01 by direct reads in the item worktree. Section 7 now records how each item was closed. Findings are verified by reading the named files unless marked `[UNVERIFIED]` or recorded as an execution-time assumption.

- Issue: #950 — Bug: quickfiler-tests-depend-on-wall-clock-timing
- Branch: `bug/quickfiler-tests-depend-on-wall-clock-timing-950` (based on origin/main `9b3eea58`)
- Date: 2026-10-01
- Tooling note: the Bash tool was disabled in this session, so `git show`/`git diff` against CI head `b9692658` could not be run. The CI failure text quoted in the delegation matches the current file verbatim (assertion at `QfcItemController.UiThreadDispatcherFixtureTests.cs:244-250`, test declared at `:206`), so the current worktree copy is treated as the text under analysis. Closure (section 7, item 1): `git diff --stat b9692658 HEAD -- QuickFiler.Test/Controllers QuickFiler.Test/TestSupport QuickFiler/Controllers/QfcDatamodel.cs UtilitiesCS/Threading/UiThread.cs` printed nothing, so every target file is byte-identical between CI head `b9692658` and the branch head.

---

## 1. Constraints the fix must honor (recorded)

| Constraint | Source |
|---|---|
| No `Thread.Sleep`, `Task.Delay`, real wall-clock waits in tests; use `FakeTimeProvider` / controllable scheduler | `.claude/rules/general-unit-test.md` "Determinism Infrastructure"; `BannedSymbols.txt` via `.claude/rules/csharp.md` |
| Tests keep running in parallel: `Workers=0`, `Scope=ClassLevel` | `scripts/vscode/TaskMaster.cli.runsettings:4-7` (used by `Invoke-MSTestWithCoverage.ps1:33,89`, which CI runs verbatim per `.github/workflows/_mstest-coverage.yml:83`); `TaskMaster.runsettings:4-7` carries the same values |
| Fix must never be `Workers=1`, `[DoNotParallelize]`, retries, or longer timeouts | issue.md "Proposed Fix"; user memory `feedback_tests_must_run_parallel_serial_masks_isolation_violation` |
| No temporary files in tests | CLAUDE.md UT4 |
| MSTest 4.4.1 + Moq 4.21.0 + FluentAssertions 8.11.0 | `QuickFiler.Test/packages.config:8,42-45` |
| net481: no `init`, no `record` | user memory `reference_net48_no_init_record_struct` |
| 500-line file cap | CLAUDE.md §4 |
| `QfcDatamodel` is COM-bound and carries a type-level `[ExcludeFromCodeCoverage]` (`QuickFiler/Controllers/QfcDatamodel.cs:25`), so any production seam added there is coverage-exempt and must stay minimal | verified |
| CI parallelism: `_mstest-coverage.yml` runs `Invoke-MSTestWithCoverage.ps1`, which passes `/Settings:TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook` (`:89-91`). CI therefore runs ClassLevel-parallel, which supersedes the older memory note that CI ran sequentially. | verified |

---

## 2. Defect A — `QfcDatamodelLivenessTests.cs` (255 lines)

### 2.1 Every wall-clock wait in the file

| # | Line | Call | Reached from | Event awaited | Producer (production code) |
|---|---|---|---|---|---|
| A1 | 56 | `SpinWait.SpinUntil(condition, TimeSpan.FromSeconds(5))` inside `WaitForState` | 106-110 (test 1), 176-179 (helper used by tests 2-4), 223-226 (test 3), 249-252 (test 4) | 106/176: `!worker.IsBusy`; 223/249: `_remainingLoadActive == false` | `IsBusy` clears when `BackgroundWorker.WorkerThreadStart` finishes and posts `AsyncOperationCompleted` (framework, posted to the captured/default SynchronizationContext — not synchronously observable). The flag clears in the `finally` at `QfcDatamodel.cs:209-216` after `await loaderTask` (`:207`) resumes. |
| A2 | 103 | `loaderEntered.Task.Wait(TimeSpan.FromSeconds(5))` | test 1 | test-owned TCS set inside the injected `RemainingEmailLoader` | `Worker_DoWork` (`QfcDatamodel.cs:185-229`) calls `RemainingEmailLoader(_token)` at `:205`; it runs on the BackgroundWorker thread because `InitEmailQueue(0, worker)` (`:259-275`) calls `worker.RunWorkerAsync()` at `:273`. |
| A3 | 173 | `entered.Task.Wait(TimeSpan.FromSeconds(5))` | `StartHeldOpenLoader` (tests 2, 3, 4) | same as A2 | same as A2 |

Non-wall-clock but scheduling-dependent residual in the same file (not in the issue's scope, recorded for completeness): test 1 lines 113-116 and 128-132 use `fake.Advance(...)` + `await Task.Yield()` loops (bounded to 20 iterations). They do not read a clock, but they rely on thread-pool scheduling of the gate's continuation between yields. Left as-is in the recommendation; see §2.4.

### 2.2 Why the waits exist

All three waits exist because `InitEmailQueue` starts the worker through `BackgroundWorker.RunWorkerAsync()` (`QfcDatamodel.cs:273`, `:300`), which runs `Worker_DoWork` on a thread-pool thread via delegate `BeginInvoke`. Nothing in the test controls that thread, so the test can only poll for the effects. There is no production seam for the *start* of the worker; the only existing seam is the worker *body* (`RemainingEmailLoader`, `QfcDatamodel.cs:140`, assigned in both constructors `:40`, `:51`, `null` on `GetUninitializedObject` instances).

### 2.3 Recommended replacement (one approach)

**Drive `Worker_DoWork` synchronously on the test thread under a test-owned, drainable `SynchronizationContext`; replace every wait with a synchronous assertion.**

Mechanics (verified from the production code and the .NET await contract):

1. `Worker_DoWork` is `async void`. Invoked synchronously it runs to its first incomplete await (`:207`, `await loaderTask` with no `ConfigureAwait(false)`) and returns. At that point `RemainingEmailLoader` has already been called (`:205`), `_remainingLoadTask` is set (`:206`), and `_remainingLoadActive` is whatever `InitEmailQueue` set (`true`, `:272`). So "entered" (A2/A3) becomes `entered.Task.IsCompleted.Should().BeTrue()` immediately after the call, and "flag stays true across the async-void boundary" (test 2) becomes a synchronous read. No wait.
2. The continuation of `await loaderTask` captures `SynchronizationContext.Current` at the await point. If the test installs a queueing context before invoking `Worker_DoWork`, then after `release.SetResult(true)` the continuation (the `finally` that clears the flag) is either posted to that queue or inlined; either way, after the test drains the queue the `finally` has run and `ReadLivenessFlag(model)` is `false` synchronously. Tests 3 and 4 (A1 at 223 and 249) become: `release.SetResult(true); pump.Drain(); ReadLivenessFlag(model).Should().BeFalse(...)`. For test 4 the throwing lambda's own continuation is drained the same way, which faults `loaderTask` and resumes `Worker_DoWork` into the same `finally` and then the `catch` at `:225-228` (log4net `logger.Error`, no-op without configuration).
3. Negative control (how the rewritten tests FAIL instead of hanging): if `release` is never set, `pump.Drain()` runs zero callbacks and `ReadLivenessFlag(model).Should().BeFalse()` fails immediately. If the production `finally` were removed, the same assertion fails immediately. If `Worker_DoWork` never reached the loader, `entered.Task.IsCompleted.Should().BeTrue()` fails immediately. No path blocks on anything with an unbounded wait: `Drain` loops only over already-queued callbacks.
4. The `!worker.IsBusy` waits (A1 at 106 and 176) disappear: the worker is never started asynchronously, so `IsBusy` is trivially `false`, and the gate never consulted `IsBusy` anyway (`_worker` has no `IsBusy` reader; its only uses are `QfcDatamodel.cs:78,100,118,261,316`). The #424 claim test 1 proves — the dequeue keeps polling on the flag, not on `IsBusy` — is preserved because `DequeueWithHighConfidenceGateWithOutcomeAsync` reads `() => _remainingLoadActive` (`QfcDatamodel.QueueProcessing.cs:305`) and `WaitForQueue` reads the flag at `:406`.

How to invoke `Worker_DoWork` synchronously (two sub-options; pick one in the plan):

- **D1 (recommended): minimal production seam for the worker start.** Add `internal Action<BackgroundWorker> WorkerStarter { get; set; }` to `QfcDatamodel`, assigned in both constructors (`:41`, `:52`) to `worker => worker.RunWorkerAsync()`, and replace the two `worker.RunWorkerAsync()` calls (`:273`, `:300`) with `WorkerStarter(worker)`. This mirrors the existing `RemainingEmailLoader` convention exactly (constructor-assigned, `null` on uninitialized instances, test-assigned). The test assigns `model.WorkerStarter = w => ((SynchronousBackgroundWorker)w).RaiseDoWork();` where `SynchronousBackgroundWorker : BackgroundWorker` is a test-side subclass exposing the `protected virtual OnDoWork(DoWorkEventArgs)` method (which raises the `DoWork` event synchronously on the calling thread and thereby invokes the privately-subscribed `Worker_DoWork` without reflection). `InitEmailQueue(0, worker)` then runs fully synchronously, keeping lines 267-274 under test. Production file growth: ~8 lines; `QfcDatamodel.cs` is 483 lines (cap 500).
- **D2 (no production change):** call the public `SetupWorker(worker)` (`:176-183`), set `_remainingLoadActive = true` via the file's existing `SetPrivateField`, then `RaiseDoWork()`. This skips `InitEmailQueue`'s flag-set lines in this file (they remain exercised by `QfcInitEmailQueueZeroBatchTests`), and no longer proves `InitEmailQueue` sets the flag before start. Rejected in favor of D1 because D1 keeps the tests' stated #424 claim intact.

Pump helper reuse: QuickFiler.Test already contains several private nested drainable contexts — `DrainableSynchronizationContext` (`Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs:246-265` and `Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs:339`), `PumpSynchronizationContext` (`Viewers/BreadcrumbPopupBoundaryCoverageTests.cs:299-358`, `Viewers/BreadcrumbDropDownReadinessTests.cs:420`), `QueuedCreatorThreadSynchronizationContext` (two files). None is shared; the assembly's shared folder is `QuickFiler.Test/TestSupport/` (`DedicatedWorkerThread.cs`, `WinFormsPumpHost.cs`). The `DrainableSynchronizationContext` shape (queue on `Post`, `Drain()` loops until empty, asserts creator thread) is the right one here: it never blocks. Either duplicate it privately in `QfcDatamodelLivenessTests.cs` (the file's own doc comment at `:19-23` records a duplication convention) or promote one copy to `TestSupport/` (new file requires a `<Compile Include>` in `QuickFiler.Test.csproj`, which lists every file explicitly, e.g. `:157`, `:201`, `:203`). `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing 10.10.0`, `packages.config:31`) is already referenced and already used by test 1; no new package.

Execution-time assumption (section 7, item 4; not statically decidable from the repository): the MSTest 4.4.1 worker thread has no ambient `SynchronizationContext` that would intercept the continuation. The plan must record the observed `SynchronizationContext.Current` value on the test thread once during execution. The pump design is robust either way because the test installs its own context around the invocation and restores the previous one (precedent: `ViewerScope` at `ItemViewerBreadcrumbThreadAffinityTests.cs:271-292`).

### 2.4 Other wall-clock waits in QuickFiler.Test

The first three rows are now IN SCOPE by maintainer decision (same root cause, same seam); section 2.5 analyses them. The remaining rows stay out of scope.

| File:line | Call | Note |
|---|---|---|
| `Controllers/QfcDatamodelTeardownTests.cs:67` | `SpinWait.SpinUntil(..., 5 s)` (`WaitForState`, used at `:225`) | IN SCOPE (T1 in 2.5) |
| `Controllers/QfcDatamodelTeardownTests.cs:220` | `loaderEntered.Task.Wait(5 s)` | IN SCOPE (T2 in 2.5) |
| `Controllers/QfcInitEmailQueueZeroBatchTests.cs:161` | `loaderInvokedTcs.Task.Wait(5 s)` | IN SCOPE (Z1 in 2.5) |
| `Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:397` | `_available.AvailableWaitHandle.WaitOne()` (unbounded) | not timed, but an unbounded block; hang risk rather than wall-clock bound |
| `Viewers/BreadcrumbPopupBoundaryCoverageTests.cs:345`, `BreadcrumbSelectorToggleUiBoundaryTests.cs:419`, `BreadcrumbUiThreadDispatchTests.cs:410`, `BreadcrumbCoordinatorLifecycleTests.cs:57` | `Wait(0)` / `WaitOne(0)` | zero-bound probes; non-blocking, not wall-clock |
| `Controllers/QfcFormControllerCleanupTests.cs:380` | string literals in a banned-literal scan | not a wait |

No `Thread.Sleep` or `Task.Delay` call exists in QuickFiler.Test (only comment mentions at `KaKeyTests.cs:104`, `KaCharTests.cs:113`, `QfcCollectionControllerDefects468Tests.cs:349-350`, `QfcDatamodelTests.cs:215`).

### 2.5 In-scope extension: `QfcDatamodelTeardownTests.cs` (235 lines) and `QfcInitEmailQueueZeroBatchTests.cs` (212 lines)

Both files start a real `BackgroundWorker` through `QfcDatamodel.InitEmailQueue`, so they share the section 2.2 cause and take the same D1 seam.

Complete caller inventory of `QfcDatamodel.InitEmailQueue` on a real (non-mock) instance, from `Grep` over `QuickFiler.Test` and `QuickFiler`:

| Caller | Line | Batch | Worker start reached | Wall-clock wait |
|---|---|---|---|---|
| `QfcDatamodelLivenessTests` test 1 | `:100` | 0 | `QfcDatamodel.cs:273` | A1 (`:106`), A2 (`:103`) |
| `QfcDatamodelLivenessTests.StartHeldOpenLoader` | `:170` | 0 | `:273` | A1 (`:176`), A3 (`:173`) |
| `QfcDatamodelTeardownTests.Worker_DoWork_CapturesRemainingLoadTask` | `:218` | 0 | `:273` | T1 (`:225` via `WaitForState` `:67`), T2 (`:220`) |
| `QfcInitEmailQueueZeroBatchTests.InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` | `:127` | 0 | `:273` | none, but starts an unobserved thread-pool worker |
| `QfcInitEmailQueueZeroBatchTests.InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` | `:156` | 0 | `:273` | Z1 (`:161`) |
| `QfcInitEmailQueueZeroBatchTests.InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` | `:201` | 2 | `:300` | none, but starts an unobserved thread-pool worker |
| `QfcHomeController.cs:252` (production) | — | — | via `IQfcDatamodel` | n/a |

`QfcHomeControllerRunAsyncTests.cs:123,191,218` call `InitEmailQueue` on a `Mock<IQfcDatamodel>`, which never reaches `QfcDatamodel`; they are unaffected. No other test file reaches either `worker.RunWorkerAsync()` site. Therefore, after the three in-scope files assign the seam, no QuickFiler.Test test starts a `QfcDatamodel` worker on the thread pool.

Per-test replacement under D1 (test assigns `model.WorkerStarter` to a synchronous starter that raises `DoWork` on the calling thread through a `BackgroundWorker` subclass exposing `OnDoWork`):

- **T1/T2 (`Worker_DoWork_CapturesRemainingLoadTask`).** `Worker_DoWork` assigns `_remainingLoadTask` at `QfcDatamodel.cs:206`, before its first await at `:207`. Under the synchronous starter, `InitEmailQueue` returns only after that line ran, so `loaderEntered.Task.IsCompleted.Should().BeTrue()` and `GetPrivateField(model, "_remainingLoadTask").Should().NotBeNull()` are both synchronous reads. The file's own doc comment (`:24-25`) already claims "no wall-clock waits"; the rewrite makes that claim true. Negative control: a starter that does not raise `DoWork` leaves both values unset and the assertions fail at once.
- **Z1 (`InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`).** The inert loader (`:97-108`) completes its TCS synchronously when invoked, so under the synchronous starter `loaderInvokedTcs.Task.IsCompleted.Should().BeTrue()` holds as soon as `InitEmailQueue` returns. The doc comment at `:136-145` that justifies the bounded wait must be rewritten. Negative control: a no-op starter leaves the TCS incomplete and the assertion fails at once.
- **Z0/Z2 (`..._ReturnsEmptyListWithoutThrowing`, `..._PositiveBatchSize_...`).** No wait, but each currently starts a real thread-pool worker whose body runs after the test returns. They must assign the same synchronous starter (or a recording starter) so no worker escapes the test. Under D1 a test that leaves `WorkerStarter` unassigned on an uninitialized instance receives `null` and `InitEmailQueue` raises `NullReferenceException` at the start site; this is the same convention `RemainingEmailLoader` already has (`QfcDatamodel.cs:135-140`) and it fails fast rather than silently starting a thread.

Inline-continuation note for Z1/Z0/Z2 and T1/T2: the inert loader returns `Task.FromResult(true)` (Z-tests), so `await loaderTask` at `:207` completes synchronously and the `finally` at `:209-216` runs before `InitEmailQueue` returns; no pump is needed for those. The T-test loader awaits a test-owned TCS, so its continuation follows the same rules as section 2.3 point 2: the test either drains a test-owned context after `loaderRelease.TrySetResult(true)` or, if it asserts nothing after release, leaves the continuation to run inline on the releasing thread.

---

## 3. Defect B — `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` (file: 458 lines)

### 3.1 What `observedByB` reads

`observedByB = UiThreadDispatcherFixture.Current` (`:230`) → `(Dispatcher)DispatcherField.GetValue(null)` under `FieldLock` (`QfcItemController.UiThreadDispatcherFixture.cs:62-71`), where `DispatcherField` is the private static `UtilitiesCS.UiThread._dispatcher` (`UtilitiesCS/Threading/UiThread.cs:285`; resolved at fixture `:197-205`). It is a plain process-wide static field — not `[ThreadStatic]`, not `AsyncLocal`, not `Dispatcher.CurrentDispatcher`.

`original = UiThreadDispatcherFixture.Current` is read at `:215` in a separate lock acquisition from `transactionA.Install(liveA)` at `:216` (`Exchange`, fixture `:77-85`).

### 3.2 The failure signature decoded

FluentAssertions message: "Expected observedByB to refer to `<null>` … but found … Dispatcher { Thread = … Name = "UiThreadDispatcherFixture.ParkedDispatcher" }". Therefore in the failing run `original == null` and `observedByB` was the fixture's singleton parked dispatcher (`_parkedDispatcher`, created only at fixture `:213-241` with that exact thread name at `:231`).

### 3.3 Every writer of `UiThread._dispatcher` in the repository

| # | Writer | Value written | Gate-aware? | Reachable from QuickFiler.Test concurrently? |
|---|---|---|---|---|
| W1 | `UiThreadDispatcherFixture.EnsureDispatcher()` — `QfcItemController.UiThreadDispatcherFixture.cs:132` | parked singleton, only when the field is `null` | **No** — by design never takes `TransactionGate` (`:26-30`, `:116-121`) | Yes: via `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` (`QfcItemController.TestSupport.cs:238-239`) from **`QfcItemController_FocusAndThemeTests.SetThemeDark_FromNormal_SelectsDarkNormalTheme` (`QfcItemController.FocusAndThemeTests.cs:452`) and `SetThemeLight_FromNormal_SelectsLightNormalTheme` (`:468`)** — both discard the returned scope, so they never revert; and from R1/R2/R3 in the same class as R4 (`:60`, `:119`, `:166`), which run serially with R4 and do revert. |
| W2 | `EnsureScope.Dispose()` — fixture `:271` via `CompareExchange(_installed, null)` | `null`, only when the field still holds the parked instance that scope installed | No | Only same-class R2/R3 dispose scopes today; no other class disposes one. |
| W3 | `UiThreadDispatcherTransaction.Install` — fixture `:316` (`Exchange`) | arbitrary | Yes (holder of `TransactionGate`) | callers: FixtureTests `:56,116,165,216,282,331`; `WpfUiDispatcherTests.cs:63`; `QfcFormControllerUndoHandoffTests.cs:236,287,343`; `QfcItemController.InitializationTests.Part2.cs:132`; `QfcHomeControllerRunAsyncTests.cs:355` — all gated. |
| W4 | `UiThreadDispatcherTransaction.Dispose` — fixture `:336` (`CompareExchange(_installedValue, _previous)`) | captured previous | Yes | gated |
| W5 | `UiThread.Initialize()` — `UiThread.cs:82` (`Dispatcher = _syncContextForm.UiDispatcher`), reached from `UiThread.Init()` `:57`, which the lazy getters `UiSyncContext` (`:224`) and `AutoScaleFactor` (`:298`) call when their fields are null; requires an STA caller (`:30-34`) | the `SyncContextForm`'s dispatcher (thread name differs from "ParkedDispatcher") | No | Not observed in this failure (wrong thread name). No QuickFiler.Test call to `UiThread.Init(` was found (only a commented one at `QfcHomeControllerTests.cs:240`); production reads of `UiThread.Dispatcher` (e.g. `ItemViewerQueue.cs:21,27`, `EfcViewerQueue.cs:20`) go through the throwing getter at `:266-284`, which never writes. Closure (section 7, item 3): QuickFiler.Test contains no call to `UiThread.Init(`, `UiThread.UiSyncContext` or `UiThread.AutoScaleFactor` (only comment mentions at `QfcCollectionControllerDefects468Tests.cs:106`, `QfcHomeControllerRunAsyncTests.cs:328`, `QfcHomeControllerTests.cs:240`), while 23 QuickFiler production files reference `UiSyncContext`/`AutoScaleFactor`, so static reachability through production code cannot be ruled out. W5 is nevertheless excluded as the producer of this failure by value identity: `Init()` throws before `Initialize()` on a non-STA caller (`UiThread.cs:30-34`), and when it does run it writes the `SyncContextForm`'s dispatcher, never the parked singleton. W5 remains a residual writer for the pinned-baseline fix: it is latched once per process (`:51-59`), and a W5 write during R4 would make B observe a value other than `original`. Recorded as a residual risk, not a cause. |
| W6 | `UiThread.ResetForTesting()` — `UiThread.cs:126` | `null` | No | Called only from `UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs:89` (different assembly). |
| W7 | `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs:78,109` and `UiThreadStateScope.cs:162,183` | their own values | No | Different assembly; those install `Dispatcher.CurrentDispatcher` of their own threads, never the QuickFiler.Test parked instance. Closure (section 7, item 3): `Invoke-MSTestWithCoverage.ps1` passes every `*.Test.dll` to one vstest invocation (`:117`, `:348`) with `/InIsolation` (`:90`), and neither runsettings file sets `DisableAppDomain`, so on .NET Framework the MSTest adapter loads each test assembly in its own AppDomain and the `UiThread` statics are per-AppDomain. Whether vstest uses one or several testhost processes is therefore not load-bearing. Independently of hosting, W7 is excluded by value identity: it never writes the QuickFiler.Test parked singleton. |

Observed value = parked singleton, prior value = `null`. Only **W1** writes the parked instance, and only into a `null` field. W1 is reachable concurrently from exactly one other class: `QfcItemController_FocusAndThemeTests`.

### 3.4 Established root cause (one)

**Raced process-wide static state across parallel test classes, not timing inside the test.** `UiThreadDispatcherFixture.EnsureDispatcher()` is gate-free by design and seeds the parked dispatcher whenever it observes `UiThread._dispatcher == null`. Under `Workers=0 / ClassLevel`, `QfcItemController_FocusAndThemeTests` (two tests calling `EnsureUiThreadDispatcher()` and discarding the scope) can run concurrently with R4. R4 begins with a `null` baseline (`original == null`), and the seeding can land in either of two windows the test leaves open:

- Window 1: between the `original` read (`:215`) and `Install(liveA)` (`:216`) — then `_previous` (parked) differs from `original` (`null`), `Dispose` restores parked, and B observes parked.
- Window 2: between `transactionA.Dispose()`'s `CompareExchange(liveA, null)` (`:240` → fixture `:336`) and B's `Current` read (`:230`), which runs on a thread-pool continuation after `WaitAsync` completes — under load this gap is long enough for another worker's `EnsureDispatcher` to see `null` and write parked.

Either window produces exactly the observed message. The `NotBeSameAs(liveA)` assertion (`:251-257`, the real #230 lost-update check) was not what failed; the over-strong `BeSameAs(original)` assertion (`:244-250`) encodes a property the fixture does not guarantee when the baseline is `null`. The 2-second duration and the 60 s `[Timeout]` are not involved (no bound expired; the failure is an assertion).

Discriminating observation (not needed for the conclusion, but confirms it): in the CI TRX for run 36722780748, `QfcItemController_FocusAndThemeTests.SetThemeDark_FromNormal_SelectsDarkNormalTheme` or `SetThemeLight_FromNormal_SelectsLightNormalTheme` has a `startTime`/`endTime` window overlapping the failing test's window. (Not fetched; the artifact requires authenticated access.)

Historical corroboration: the #823 flake-watch log (`docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/flake-watch-uithread-dispatcher-transaction.2026-09-09T00-15.md`) records 1 failure in 4 runs, all under `Workers 0 / ClassLevel`, with no failure text captured — consistent with a class-interleaving race.

### 3.5 Recommended fix (test-only; keep in #950, no split needed)

Pin a non-null baseline for the whole R4 body so W1 cannot write: wrap the test body in `using (IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher())` before `BeginTransactionAsync`. With the field non-null, `EnsureDispatcher` from any class installs nothing (fixture `:130-137`), closing both windows; `original` then equals the parked (or pre-existing) value and B observes it. Keep `NotBeSameAs(liveA)`. Residual exposure: a foreign `EnsureScope.Dispose()` holding the same parked singleton (W2) could null the field during the test; no such disposer exists today (`FocusAndThemeTests` discards its scopes), and `CompareExchange` compares identity against the singleton, so this must be recorded as an invariant in the R4 doc comment. Update the R4 `<para>` (`:196-202`) to replace the #823 flake-watch instruction with the established cause and #950.

Negative control for B: under a fixture that releases the gate before restoring (the #230 defect), B observes `liveA` and `NotBeSameAs(liveA)` fails — probabilistically, as the class doc at `:14-22` already records; this research does not change that limitation.

Alternatives rejected: (i) making `EnsureDispatcher` take the gate — rejected by the fixture's own design note (`:26-30`; callers without `[Timeout]` would hang unboundedly); (ii) exposing `UiThreadDispatcherTransaction._previous` and asserting against it — closes window 1 only; (iii) weakening the assertion to "original or parked" — asserts a disjunction instead of a contract.

---

## 4. Files a fix would touch

| File | Kind | Lines now | csproj | Change |
|---|---|---|---|---|
| `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` | test | 255 | `QuickFiler.Test.csproj:157` (explicit `<Compile Include>`) | remove A1-A3; synchronous driving via D1; pump helper (private or shared) |
| `QuickFiler/Controllers/QfcDatamodel.cs` | production (COM-bound, `[ExcludeFromCodeCoverage]` at `:25`) | 483 | `QuickFiler.csproj:325` (explicit `<Compile Include="Controllers\QfcDatamodel.cs" />`; legacy csproj, explicit items, verified) | D1 seam: `WorkerStarter` property + 2 ctor assignments + 2 call sites (`:273`, `:300`) |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | test | 458 | `QuickFiler.Test.csproj:203` | R4 baseline scope + doc comment |
| optional `QuickFiler.Test/TestSupport/<DrainableSynchronizationContext>.cs` | test support (new) | — | needs a new `<Compile Include>` in `QuickFiler.Test.csproj` | only if the pump is shared rather than duplicated |
| `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs` | test | 235 | existing | IN SCOPE: remove T1/T2; assign the D1 starter (section 2.5) |
| `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` | test | 212 | existing | IN SCOPE: remove Z1; assign the D1 starter in all three tests (section 2.5) |

Acceptance-gate test names (fully qualified):
- `QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle`
- `QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces`
- `QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_AfterLoaderCompletes_BecomesFalse`
- `QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally`
- `QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores`
- Concurrent-writer class for B: `QuickFiler.Controllers.Tests.QfcItemController_FocusAndThemeTests` (`SetThemeDark_FromNormal_SelectsDarkNormalTheme`, `SetThemeLight_FromNormal_SelectsLightNormalTheme`)

---

## 5. Testing implications

- A: each rewritten test gets an explicit negative-control step in the plan (run once with `release` never set, or with the production `finally` commented out locally) and must FAIL immediately, not hang. A regression test for the B race cannot rely on MSTest scheduling two classes concurrently (memory `uithread-dispatcher-restore-scope-493`, finding 4); if a regression test is wanted, it must create its own thread calling `EnsureDispatcher()` between A's dispose and B's read on the pre-fix test shape — otherwise rely on the code trace plus the baseline pin.
- No `[DoNotParallelize]`, no `Workers=1`, no retries, no timeout changes.

## 6. Automation Feasibility

No human interaction is required. All steps (edit test/production files, run the four-step toolchain, run the named tests by fully qualified name with `/Settings:scripts/vscode/TaskMaster.cli.runsettings`) are scriptable. No live Outlook, no UI pump host, no manual gate.

## 7. Items left open by the first pass, and how each was closed

1. CI head `b9692658` versus the branch for the target files — CLOSED. The diff is empty (see the tooling note at the top). Main has not changed any target file since merge-base `9b3eea584` either, so citations against the branch tree are current.
2. `QuickFiler.csproj` compile-item style — CLOSED. Explicit legacy items; `QfcDatamodel.cs` is listed at `QuickFiler.csproj:325`. Editing an existing file needs no csproj change.
3. W5 reachability and W7 process sharing — CLOSED (section 3.3 rows W5 and W7). Neither can write the parked singleton, so neither changes the established cause. W5 is recorded as a residual writer for the pinned-baseline fix.
4. Ambient `SynchronizationContext` on the MSTest worker thread — RECORDED AS AN EXECUTION-TIME ASSUMPTION (section 2.3). The test-installed context makes the design independent of the answer; the plan records the observed value once.
5. Section 2 extended to `QfcDatamodelTeardownTests.cs` and `QfcInitEmailQueueZeroBatchTests.cs` — CLOSED (section 2.5), including the complete caller inventory of `InitEmailQueue`.
6. Numeric derivation evidence: no numeric acceptance criterion is proposed, so none is required.
