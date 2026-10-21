# Research — Issue #944: prime-marker registration races its own removal

- **Issue:** #944
- **Feature folder:** `docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/`
- **Researched:** 2026-09-30T08-00
- **Mode:** research only (no source changes)
- **Evidence tags:** `[V]` verified by reading the named file or fetched source in this session; `[V-web]` verified against the .NET Framework reference source fetched from `raw.githubusercontent.com/microsoft/referencesource/main/...`; `[D]` derived by reasoning from `[V]` facts; `[B]` binding context supplied by the orchestrator about in-flight #942 (not independently readable here).

## 1. Current state

### 1.1 Production code (`TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, 415 lines pre-#942) `[V]`

- `StartPrimeIfNeeded` takes `lock (_primeGate)`, returns if `_primeTasks.ContainsKey(engineName)`, otherwise executes `_primeTasks[engineName] = StartObservedPrime(engines, engineName, controlId);`.
- `StartObservedPrime` returns `ApplyPrimeAsync(...).ContinueWith(completed => CompletePrime(completed, engineName), CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default)`. The continuation task is the value stored in `_primeTasks` and is the handle returned by `GetPrimeTask`.
- `ApplyPrimeAsync` is an `async` method with no `catch`: `NextSequence()`, then `await engines.EngineActiveAsync(engineName).ConfigureAwait(false)`, then `TryApplyState` and conditional `_invalidateControl`. Its synchronous prefix (including the call into `EngineActiveAsync`) runs on the thread that called `GetPressed`, while `_primeGate` is held.
- `CompletePrime` (pre-#942): returns on `RanToCompletion`; otherwise `_primeTasks.TryRemove(engineName, out _)` (no lock), then `_logError(...)`. Post-#942 `[B]` the order becomes log-then-`TryRemove`.
- `GetPrimeTask` returns the stored task or `Task.CompletedTask`. `_primeTasks` is written only by `StartPrimeIfNeeded` and removed only by `CompletePrime`. A successful prime's marker stays registered for the session (success never removes) `[V]`.
- The only production caller is `RibbonController.EngineCommands.cs` (`EngineToggles` lazily constructs the coordinator; `IsEngineToggleActive` → `GetPressed`; the `logError` sink is `logger.Error(message, exception)`). Production never calls `GetPrimeTask` `[V]` (grep of `GetPrimeTask|_primeTasks|StartObservedPrime|CompletePrime` finds only the production file and the two fixture partials).

### 1.2 Why an already-failed task is a realistic input `[V]`

- `AppItemEngines.EngineActiveAsync` is `async` and begins with `await Globals.AF.Manager.Configuration`.
- `ManagerAsyncLazy.Configuration` is an `AsyncLazy<ConcurrentDictionary<string, SmartSerializableLoader>>`. `AsyncLazy<T>` wraps a `Lazy<Task<T>>`, so a faulted configuration load is cached and every later `await` rethrows synchronously. `EngineActiveAsync` therefore returns an already-faulted task, `ApplyPrimeAsync` returns an already-faulted task, and the hazard window is at its widest. (`ResetConfigAsyncLazy()` exists and replaces the lazy; it is the only recovery path.)

### 1.3 Tests `[V]`

- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs`: 459 lines (460 with trailing newline), `public partial class`, holds the private `Harness` (strict `Mock<IAppItemEngines>`, recording `Invalidations`, `Notifications`, `Errors`, optional `OnInvalidate`) and `LoggedError`. #942 `[B]` adds `OnLogError` (~+8 lines).
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs`: 277 lines; #735 last-writer and CR-2 canceled-prime tests.
- `TaskMaster.Test/TaskMaster.Test.csproj` uses explicit `<Compile Include>` items: `Ribbon\EngineToggleStateCoordinatorTests.cs` and `Ribbon\EngineToggleStateCoordinatorTests.Race.cs`. #942 `[B]` adds `Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs`.
- Hazard B is already reachable from existing tests `[D]`: the re-prime at the end of `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` and the second read in `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` both re-enter an already-faulted/canceled `probe.Task`. Neither asserts anything the race can falsify (the second draws its conclusion from `NotBeSameAs(firstPrime)`, which holds whether `GetPrimeTask` returns the stale continuation or `Task.CompletedTask`). This matches NB-2 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/code-review.2026-09-03T06-19.md` (lines 188-221), which recommended "take `_primeGate` around the `TryRemove`, or register a placeholder marker before attaching the continuation".

## 2. Q1 — hazard mechanism on .NET Framework 4.8 TPL

1. `[V]` All TaskMaster projects target `v4.8.1`.
2. `[V-web]` `StandardTaskContinuation.Run(completedTask, bCanInlineContinuationTask)` inlines **only** when `bCanInlineContinuationTask && (options & TaskContinuationOptions.ExecuteSynchronously) != 0`; otherwise it calls `continuationTask.ScheduleAndStart(needsProtection: true)`.
3. `[V-web]` `ThreadPoolTaskScheduler.QueueTask` for a non-`LongRunning` task calls `ThreadPool.UnsafeQueueCustomWorkItem(task, forceToGlobalQueue)`; it never executes the task on the queuing thread.
4. `[D]` (the `ContinueWithCore` body could not be retrieved; the fetched page was truncated) Whether the antecedent is already complete when `ContinueWith` is called or completes later, dispatch goes through `StandardTaskContinuation.Run`. With `TaskContinuationOptions.None`, item 2 means the continuation is **always queued** to the thread pool and **never runs inline on the registering thread**. The only other inlining route is `TryExecuteTaskInline` via a `Wait` on the continuation task, and nothing on the registering thread waits on it.
5. **Race window** `[D]`: from the instant the continuation is queued (inside `ContinueWith`) until the dictionary store `_primeTasks[engineName] = ...` completes on the registering thread. A pool thread that dequeues the continuation inside that window runs `CompletePrime`; its `TryRemove` finds no entry (first prime) or removes nothing relevant, and the registering thread then stores a continuation that has already finished or is finishing. `ContainsKey` is then true for the rest of the session. The window is not limited to synchronously-failed primes: any prime whose antecedent completes on another thread between `ContinueWith` registration and the store has the same exposure. A synchronously-failed prime simply opens the window at the earliest possible point.
6. **Consequence for fix design** `[D]`: because the continuation is never inline on the registering thread, Monitor re-entrancy does not currently defeat a lock-in-`CompletePrime` fix. It would if `ExecuteSynchronously` were ever added (item 2), which #942 already rejected for this reason `[B]`.

## 3. Q2 — candidate fix shapes

Evaluation criteria: correctness under every interleaving (including a hypothetical inline continuation), #942 compatibility (report-then-clear; handle captured before the trigger is the same instance seen inside the sink; that handle completes only after report and removal; `GetPrimeTask` afterwards is `Task.CompletedTask`), `GetPrimeTask` never-faults contract, at-most-one prime per engine, single-`catch` invariant, deadlock freedom, size, single production file.

| Shape | Fixes hazard B | Inline-safe | #942 assertions | Deterministic RED test possible | Size |
|---|---|---|---|---|---|
| (i) `_primeGate` around `TryRemove` in `CompletePrime` | yes (cross-thread only) | **no** (Monitor re-entrant) | pass | **no** | ~4 lines |
| (ii) identity-conditional removal alone | **no** | n/a | pass | no | ~2 lines |
| (iii) register-before-start with a `TaskCompletionSource<bool>` marker | yes | yes | pass | **yes** | ~+12 lines |
| (iv-a) cold `Task<Task>` + `Unwrap` + `RunSynchronously` | yes | yes | pass | yes | ~+4 lines |

### (i) Lock in `CompletePrime`

Correct today: the removal blocks until the registering thread leaves `_primeGate`, which is after the store. `_logError` runs before the lock under #942's order, so no user code runs under the gate; deadlock-free. Rejected because (a) its correctness depends on the continuation never running on the registering thread — Monitor re-entrancy would let an inline continuation remove before the store — so it is a property of `TaskContinuationOptions.None` rather than of the type; (b) no program-order discriminator can tell it apart from the defect (the handle is still unregistered while `EngineActiveAsync` runs), so the bugfix workflow's "fails before the fix" test is impossible without a scheduler seam, which #942 already rejected `[B]`.

### (ii) Identity-conditional removal alone

`((ICollection<KeyValuePair<string, Task>>)_primeTasks).Remove(new KeyValuePair<string, Task>(engineName, handle))` is available on net48 (the public `TryRemove(KeyValuePair)` overload is .NET 5+). Alone it does not fix the defect: when removal precedes registration there is nothing to remove, and the later store still leaves a finished marker. It also needs the continuation to know its own handle, which with the current shape is only obtainable through a closure variable assigned after `ContinueWith` returns — the same race. Rejected as a fix; see §4.4 for its status as optional hardening.

### (iii) Register-before-start — **recommended**

Inside the existing `lock (_primeGate)`: after the `ContainsKey` check, create `var marker = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);`, store `_primeTasks[engineName] = marker.Task;`, **then** call `StartObservedPrime(engines, engineName, controlId, marker)`. `StartObservedPrime` becomes `void`, discards the continuation task (`_ = ...` for MA0134), and the continuation body is `try { CompletePrime(completed, engineName); } finally { marker.SetResult(true); }`. `CompletePrime` is unchanged from its post-#942 form.

- **Correctness, all interleavings** `[D]`: the marker is in the dictionary before `ApplyPrimeAsync` is invoked, so it is present before the prime can complete on any thread, including inline. `_primeTasks[engineName]` has exactly one writer (`StartPrimeIfNeeded`, gated by `ContainsKey` under `_primeGate`), so while the marker is registered no other value can replace it; the only removal is that marker's own `CompletePrime`. Therefore `TryRemove(engineName, out _)` in `CompletePrime` always removes this prime's own marker, and a finished failed prime can never leave a marker behind.
- **#942 compatibility** `[D]`: the handle captured before `probe.SetException` is `marker.Task`; inside the sink (before `TryRemove`) `GetPrimeTask` returns the same `marker.Task`; `SetResult` runs in `finally` after `CompletePrime` returns, i.e., after the report and after the removal, so `await prime` resumes only then, and `GetPrimeTask` afterwards returns `Task.CompletedTask`. The #942 `<returns>` guarantee ("the handle is incomplete while the marker is registered", scoped to the failure path) holds. On the success path the marker stays registered and completed, exactly as the continuation handle does today.
- **Never-faults contract** `[D]`: `marker.Task` is only ever completed with `SetResult`, so it cannot fault or cancel — stronger than today, where a throwing sink would fault the continuation handle.
- **At-most-one prime** `[D]`: unchanged; the check-and-register remains atomic under `_primeGate`, and registration now precedes start.
- **Single-`catch` invariant** `[V]`/`[D]`: `try`/`finally` adds no `catch`.
- **Deadlock** `[D]`: no new lock acquisition. `_primeGate` is still held across a dictionary probe, a store, and the synchronous prefix of the prime start — identical to today. `CompletePrime` takes no lock.
- **`RunContinuationsAsynchronously`** `[V]`/`[V-web]`: present in the .NET Framework reference `Task.cs` and already used in production TaskMaster (`TaskMaster/AppGlobals/NonBlockingDelay.cs:68`, `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs:52`). It is recommended so that awaiters of the handle (tests) resume on their own pool work item instead of running inside the coordinator's `finally`. It is not required for correctness.
- **Synchronous start preserved** `[D]`: `ApplyPrimeAsync` is still invoked directly on the calling thread, inside the lock, as today.
- **Scope**: one production file, net ≈ +12 lines including comment and doc updates.

### (iv-a) Cold start — rejected

`var start = new Task<Task>(() => ApplyPrimeAsync(...)); _primeTasks[engineName] = start.Unwrap().ContinueWith(...); start.RunSynchronously(TaskScheduler.Default);` also registers before start and keeps the continuation as the handle. Rejected: `RunSynchronously` falls back to queue-and-`Wait` when the inlining stack guard refuses, which would block the Outlook STA the class remarks forbid blocking; and the `Task<Task>`/`Unwrap` indirection is less readable than an explicit marker (Simplicity first).

### Rejected alternatives (summary)

- `await Task.Yield()` at the top of `ApplyPrimeAsync`: moves `EngineActiveAsync` off the calling thread and does not close the window for antecedents that complete on another thread.
- `TaskContinuationOptions.ExecuteSynchronously`: makes the defect deterministic (inline removal before the store) `[B]`.
- Scheduler constructor seam: rejected by #942 `[B]`; unnecessary because (iii) admits a program-order test.

## 4. Recommended design

### 4.1 State model

`_primeTasks[engine]` holds a *marker* — `TaskCompletionSource<bool>.Task` — for the lifetime of an in-flight prime and, after success, for the session. Transitions: absent → registered-incomplete (under `_primeGate`, before start) → on success: registered-complete (permanent) | on failure: report → removed → complete.

### 4.2 Production changes (one file: `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`)

1. `StartPrimeIfNeeded`: create and register the marker before calling `StartObservedPrime`; add a *why* comment naming #944.
2. `StartObservedPrime`: signature gains `TaskCompletionSource<bool> marker`, returns `void`; continuation body `try { CompletePrime(completed, engineName); } finally { marker.SetResult(true); }`; keep `CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default`. Rewrite its `<remarks>` sentence "The returned continuation task always completes successfully, which is what makes it safe for a test to await." to describe the marker.
3. Doc updates: `_primeTasks` field summary ("The in-flight — or most recently completed — prime per engine key" → the registered marker, registered before the prime starts); `_primeGate` summary ("a dictionary probe and a task start" → probe, marker registration, and task start); `GetPrimeTask` summary/returns as rewritten by #942 remain true — verify wording after the #942 merge rather than re-edit it.
4. `CompletePrime`: no code change.

Expected post-change size: ≈ 415 + #942 delta + ~12, well under 500 `[D]`.

### 4.3 Invariants preserved

report-then-clear ordering; #942 test assertions; all existing tests in both partials (§5.3); at most one concurrent prime per engine; exactly one `catch` in the type; no new lock, no deadlock path; `ApplyPrimeAsync` starts synchronously on the caller.

### 4.4 Optional hardening (not recommended for this issue)

Identity-conditional removal of `marker.Task` becomes race-free under (iii) because the marker exists before the continuation is created. It is not needed (§3 (iii) single-writer proof), no reachable interleaving distinguishes it, and it would edit the #942-owned `TryRemove` line. Keep `TryRemove(engineName, out _)`.

## 5. Q3 — deterministic regression tests

The raw race cannot be forced: `ContinueWith` is hard-wired to `TaskScheduler.Default` and nothing observable executes between the queueing inside `ContinueWith` and the dictionary store. The deterministic RED therefore targets the invariant whose absence *is* the defect — "the marker is registered before the prime can complete" — using program order only.

### 5.1 Test 1 — program-order discriminator (deterministic RED)

`GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns`

- Arrange: `harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(() => { handleSeenDuringRead = harness.Coordinator.GetPrimeTask(SpamEngine); handleCompletedDuringRead = handleSeenDuringRead.IsCompleted; return Task.FromException<bool>(failure); });` — this callback runs synchronously inside `ApplyPrimeAsync` on the test thread. Record only; assert outside the callback (an assertion thrown inside would become a prime fault).
- Act: `harness.Coordinator.GetPressed(SpamEngine);`
- Assert: `handleCompletedDuringRead.Should().BeFalse(...)`; then `await handleSeenDuringRead;`; `Errors` contains a single entry whose exception `BeSameAs(failure)`; `GetPrimeTask(SpamEngine)` `NotBeSameAs(handleSeenDuringRead)` and `IsCompleted` is true.
- Before the fix: inside the callback the dictionary has no entry, so `GetPrimeTask` returns `Task.CompletedTask` and `handleCompletedDuringRead` is `true`. The first assertion fails on every run, by program order alone (the store happens only after `StartObservedPrime` returns, which is after `EngineActiveAsync` was called). **Deterministic RED.**
- After the fix: the marker is registered and cannot be complete (its completion requires the continuation, which requires `ApplyPrimeAsync`'s task, which has not yet returned). Awaiting the marker resumes only after report and removal, so the remaining assertions are deterministic. **Deterministic GREEN.**
- Do not assert `GetPrimeTask(SpamEngine)` identity immediately after `GetPressed` returns: post-fix the queued continuation may already have removed the marker, so that assertion would be timing-dependent.
- Do not rely on `BeSameAs(Task.CompletedTask)` as the discriminator; `IsCompleted` is sufficient and independent of the lazily cached singleton.

### 5.2 Tests 2 and 3 — behavioral re-prime guards (deterministic GREEN; RED not deterministic)

`GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime` and `GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime`

- Arrange: `SetupSequence(x => x.EngineActiveAsync(SpamEngine)).Returns(Task.FromException<bool>(failure)).Returns(Task.FromResult(true))` (canceled variant: `Task.FromCanceled<bool>(new CancellationToken(true))`, requires `using System.Threading;`).
- Act: `GetPressed`; `await GetPrimeTask(SpamEngine)`; second `GetPressed`; `var secondPrime = GetPrimeTask(SpamEngine); await secondPrime;`.
- Assert: `Engines.Verify(x => x.EngineActiveAsync(SpamEngine), Times.Exactly(2), ...)`; `GetPressed(SpamEngine)` is `true`; `Invalidations` equals `[SpamToggleControlId]`; `Errors` contains a single entry (faulted: `BeSameAs(failure)`; canceled: `BeAssignableTo<OperationCanceledException>`).
- Post-fix determinism `[D]`: the first `GetPrimeTask` returns either the marker (awaiting it resumes after removal) or `Task.CompletedTask` (which means removal has already happened); in both cases the second read starts a new prime. The second prime completes synchronously on the test thread, so the invalidation and cache write are complete before `GetPressed` returns, and the success marker stays registered until awaited.
- Pre-fix: these fail only when the pool thread wins the race (stale marker → `Times.Exactly(2)` fails). That is timing-dependent, so these tests are regression guards for the user-visible outcome, not the RED gate. The plan must state that Test 1 carries the "fails before the fix" obligation.
- Strict-mock note: a third `EngineActiveAsync` call would return `null` from the exhausted sequence; none occurs because the post-success read is a cache hit.

### 5.3 Existing tests under the recommended design `[D]`

All existing tests remain green — 22 methods / 24 cases pre-#942 (main fixture: 16 methods, 18 cases because `GetPressed_WithNullOrWhitespaceKey_ReturnsFalseWithoutPrimeOrInvalidate` has 3 data rows; `.Race.cs`: 6 methods), counted by reading both files. Every prime-related one obtains the handle through `GetPrimeTask` and awaits it; the marker completes after `CompletePrime` on every outcome; `NotBeSameAs(firstPrime)` in the CR-2 test holds whether the second read returns the second marker or `Task.CompletedTask`. The #942 test passes per §3 (iii).

### 5.4 Policy conformance

MSTest `[TestMethod]`, Moq strict harness, FluentAssertions; no `Thread.Sleep`, `Task.Delay`, wall-clock waits, retries, temp files, `[DoNotParallelize]`, or `Workers=1`. Each test constructs its own `Harness`, so tests are independent under ClassLevel parallelism.

## 6. Q4 — files and line budgets

New test partial name: **`TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs`** (distinct from `.Race.cs` and #942's `.PrimeFaultOrdering.cs`). Estimated 170-200 lines.

| File | Change | Budget |
|---|---|---|
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | modify (§4.2) | ≈ 430-445 after #942 + #944; < 500 |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs` | create (3 tests) | ≈ 170-200; < 500 |
| `TaskMaster.Test/TaskMaster.Test.csproj` | add `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs" />` | n/a |

The main fixture (459 → ≈ 467 after #942) and `.Race.cs` (277) are **not** modified. No production caller changes; `RibbonController.EngineCommands.cs` is unaffected.

## 7. Q5 — anchor tokens (use instead of line numbers)

Production:
- `_primeTasks[engineName] = StartObservedPrime(engines, engineName, controlId);`
- `if (_primeTasks.ContainsKey(engineName))`
- `private Task StartObservedPrime(`
- `completed => CompletePrime(completed, engineName),`
- `TaskContinuationOptions.None,`
- `The returned continuation task always`
- `private void CompletePrime(Task completed, string engineName)` (verify-only; do not edit)
- `_primeTasks.TryRemove(engineName, out _);` (verify-only; #942-owned)
- `Serializes the at-most-one-prime decision.`
- `The in-flight — or most recently completed — prime per engine key.`
- `internal Task GetPrimeTask(string engineName)`

Test project:
- `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.Race.cs" />` (insert the new entry adjacent; expect #942's `PrimeFaultOrdering` entry nearby after merge).
- Harness members relied on: `internal Mock<IAppItemEngines> Engines`, `internal EngineToggleStateCoordinator Coordinator`, `internal List<LoggedError> Errors`, `internal List<string> Invalidations`; constants `SpamEngine`, `SpamToggleControlId`.

## 8. Q6 — out-of-scope follow-ups

1. **Throwing `logError` sink leaves a stale marker.** Under #942's report-then-clear order, a sink that throws skips `TryRemove`, reproducing #944's symptom by a different cause. Under the recommended design the marker still completes (`finally`), but the continuation task faults unobserved. Production sink is `logger.Error`, so likelihood is low. Candidate follow-up.
2. **Log volume after a permanent configuration fault.** `AsyncLazy` caches the fault (§1.2), and each cache-miss `getPressed` poll re-primes and logs again. Today the stale marker intermittently suppressed repeats; after #944 every poll that reaches `StartPrimeIfNeeded` logs. A back-off or a `ResetConfigAsyncLazy`-based recovery is a separate decision.
3. **Spec skeleton.** `spec.md` Proposed Fix, Scope, Test Strategy and ACs are template placeholders (Test Strategy mentions "pytest"); the planner should populate them from §§4-6.

## Numeric Derivation Evidence

### Claim: existing `GetPrimeTask` call sites that the design must keep compatible = 10, in 2 files

- **Complete Family:** every invocation of `GetPrimeTask` in test code (the method is `internal`; production has no caller).
- **Exhaustive Search Scope:** `TaskMaster.Test/**` and whole repository `**/*.cs`.
- **Inclusion Rules:** source lines invoking `GetPrimeTask(`.
- **Exclusion Rules:** the declaration and XML `cref` references in the production file.
- **Primary Search Strategy or Query Expression:** Grep regex `\.GetPrimeTask\(` over repository `**/*.cs`, content mode.
- **Primary Member Set:** `.Race.cs` lines 58, 211, 235, 263, 272; main fixture lines 122, 183, 197, 224, 242.
- **Primary Count:** 10.
- **Cross-check Search Strategy or Query Expression:** Grep literal `GetPrimeTask` (no receiver or parenthesis) over `TaskMaster.Test/`, content mode.
- **Cross-check Member Set:** `.Race.cs` lines 58, 211, 235, 263, 272; main fixture lines 122, 183, 197, 224, 242.
- **Cross-check Count:** 10.
- **Member-set Comparison:** identical (same 10 file:line pairs). The #942 partial will add further call sites after merge; they are covered by §3 (iii) compatibility reasoning, not by this count.

### Claim: files the fix creates or modifies = 3

- **Complete Family:** repository files containing, or required to register, the symbols the fix changes (`StartPrimeIfNeeded`, `StartObservedPrime`, `_primeTasks`, `CompletePrime`) plus the new test partial.
- **Exhaustive Search Scope:** repository `**/*.cs` and `**/*.csproj`.
- **Inclusion Rules:** a file is in scope if it declares a changed member or must list a new compile item.
- **Exclusion Rules:** files that only call `GetPrimeTask`/`GetPressed` unchanged.
- **Primary Search Strategy or Query Expression:** Grep `GetPrimeTask|_primeTasks|StartObservedPrime|CompletePrime` count mode over `**/*.cs` → production file (12 lines), main fixture (5), `.Race.cs` (5); only the production file declares changed members.
- **Primary Member Set:** `EngineToggleStateCoordinator.cs` (modify), new `EngineToggleStateCoordinatorTests.PrimeRegistration.cs` (create), `TaskMaster.Test.csproj` (register).
- **Primary Count:** 3.
- **Cross-check Search Strategy or Query Expression:** Grep `EngineToggleStateCoordinator|TargetFrameworkVersion` over `**/*.csproj` → `TaskMaster.csproj:466` (production compile item, already present, unchanged) and `TaskMaster.Test.csproj:352,359` (fixture compile items; the new partial needs a sibling entry).
- **Cross-check Member Set:** `EngineToggleStateCoordinator.cs`, new partial, `TaskMaster.Test.csproj`.
- **Cross-check Count:** 3.
- **Member-set Comparison:** identical.
