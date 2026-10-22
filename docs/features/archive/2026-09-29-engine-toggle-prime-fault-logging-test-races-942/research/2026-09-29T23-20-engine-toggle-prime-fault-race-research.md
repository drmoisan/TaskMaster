# Research: issue #942 — `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` races the prime-fault log

- Issue: #942
- Feature folder: `docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/`
- Branch: `bug/engine-toggle-prime-fault-logging-test-races-942`
- Date: 2026-09-29T23-20
- Evidence tags: `[V]` verified by reading the named file/lines in this worktree; `[D]` derived from documented .NET Framework 4.8 TPL behaviour; `[M]` recalled from agent memory and re-checked against the worktree where possible. Bash was unavailable in this session, so no git history was consulted and no test was executed.

## 1. Summary of conclusions

1. **H1 (fault observed in a continuation the test does not await) — REFUTED.** `StartObservedPrime` returns the `ContinueWith` continuation itself and that continuation is what `_primeTasks` stores and `GetPrimeTask` returns (`EngineToggleStateCoordinator.cs:276, 296-302, 254`). Awaiting the handle covers `CompletePrime`, including `_logError`, whenever the handle is obtained.
2. **H2 (marker removed before the log, so a post-trigger `GetPrimeTask` can return `Task.CompletedTask` while the log is still pending) — CONFIRMED.** `CompletePrime` executes `_primeTasks.TryRemove` at line 348 and `_logError` at line 354. The continuation runs on a thread-pool thread (`TaskContinuationOptions.None`, `TaskScheduler.Default`, lines 299-301), while the test calls `GetPrimeTask` on its own thread *after* the trigger (`EngineToggleStateCoordinatorTests.cs:223-224`). If the pool thread reaches line 348 first, the test awaits `Task.CompletedTask` and asserts on `Errors` before, or concurrently with, the `Errors.Add` at line 415.
3. A second, pre-existing ordering hazard (registration at line 276 racing the removal at line 348 when the prime completes synchronously) is reachable from the same test's re-prime at line 237 and in production after a cached configuration-load fault. It does not fail any current test. It was recorded as NB-2 in the #735 code review with a recommendation to promote it to its own issue; no such issue or potential entry exists. It is out of scope for #942 and should be promoted separately.
4. **Recommended fix:** reorder `CompletePrime` so the fault is reported through `_logError` before the marker is cleared (production, two statements), which makes the documented `GetPrimeTask` contract true on every path; add one deterministic regression test that probes the prime handle from inside the injected log sink (no sleeps, gates, timers, blocking, `[DoNotParallelize]`, or worker limits); the negative control is the same test run against the current statement order, where it fails deterministically.
5. No other test in the repository has the same pattern. Two other production sites observe faults in discarded continuations (the genuine H1 shape), but no test asserts on their logs.

## 2. Current state analysis

### 2.1 Production: `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` `[V]`

| Lines | Member | Behaviour relevant to the race |
|---|---|---|
| 77-80 | `_primeTasks` | `ConcurrentDictionary<string, Task>`; doc at 72-76 says its *presence* is the at-most-one-prime guard and its *value* is the test-observable handle. |
| 136-150 | `GetPressed` | Cache miss with engines available calls `StartPrimeIfNeeded` (148) and returns `false`. |
| 247-255 | `GetPrimeTask` | Returns `_primeTasks[engineName]` or `Task.CompletedTask` when no marker is registered. Doc (238-246): "exposed so tests can await the prime deterministically instead of polling or sleeping … The returned task never faults: a prime fault is observed inside the prime itself and reported through `logError`." |
| 261-278 | `StartPrimeIfNeeded` | Under `lock (_primeGate)`: `ContainsKey` check (271), then `_primeTasks[engineName] = StartObservedPrime(...)` (276). `StartObservedPrime` is fully evaluated — including scheduling the continuation — before the assignment. |
| 290-303 | `StartObservedPrime` | `ApplyPrimeAsync(...).ContinueWith(completed => CompletePrime(completed, engineName), CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default)`. The **continuation task** is what is returned and stored. Doc (284-288): "The returned continuation task always completes successfully, which is what makes it safe for a test to await." |
| 310-325 | `ApplyPrimeAsync` | `await engines.EngineActiveAsync(engineName).ConfigureAwait(false)` (319); no `catch`. |
| 341-355 | `CompletePrime` | `if RanToCompletion return;` (343) → `_primeTasks.TryRemove(engineName, out _)` (**348**) → unwrap/synthesize exception (350-352) → `_logError(BuildPrimeFailedMessage(engineName), failure)` (**354**). |

Production wiring `[V]` `TaskMaster/Ribbon/RibbonController.EngineCommands.cs:67-77`: `logError` is `(message, exception) => logger.Error(message, exception)`; `invalidateControl` is `controlId => _viewer?.InvalidateEngineToggle(controlId)`. Neither re-enters the coordinator. `GetPrimeTask` has no production caller (grep over `*.cs`: the only non-test hits are its declaration and its own `<see cref>`).

### 2.2 Tests `[V]`

`TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` (459 lines; the 500-line cap is close):

- 213-243 `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` (the flaky test):
  - 220 `GetPressed(SpamEngine)` starts the prime against a held `TaskCompletionSource<bool>` (`probe`).
  - 223 `probe.SetException(failure)`; 224 `await harness.Coordinator.GetPrimeTask(SpamEngine)` — **the handle is fetched after the trigger**.
  - 227-234 assert `Errors` has exactly one entry, its message and exception, and `Invalidations` empty.
  - 236-239 `GetPressed` again (re-primes, because the marker was cleared); 242 cleanup `await GetPrimeTask` (also post-trigger).
- 403-441 `Harness`: `Errors` is a plain `List<LoggedError>` (440) appended from the injected `logError` lambda (415); `OnInvalidate` (434) is an extra observer invoked from inside the invalidation sink (409-413) — the existing "probe from inside the sink" precedent used by the ordering test at 270-276.

`TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` (277 lines, second partial):

- 204-246 `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker`: **captures `firstPrime = GetPrimeTask(...)` at 211 before `probe.SetCanceled()` at 214** and awaits that captured handle (215) before asserting `Errors` (218-229). This is the capture-before-trigger pattern and is immune to H2 for its error assertion. Its remarks (195-202) already document why the error count must be asserted before the re-prime.
- 253-273 `GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked`: post-trigger `GetPrimeTask` at 263, but the only assertion (266-269) is `GetPressed == false`, which holds under every interleaving because a canceled prime never writes the cache.
- 38-74 and the two toggle tests use `SetResult` (success path); `CompletePrime` returns at 343 without touching the marker, so `GetPrimeTask` always returns the continuation and the await is deterministic.

Run regime `[V]`: `TaskMaster.runsettings:4-7` and `scripts/vscode/TaskMaster.cli.runsettings:4-7` both set `Workers=0`, `Scope=ClassLevel`. MSTest is 4.4.1 (`TaskMaster.Test/packages.config:42-44`). `[M]` MSTest 4.4 runs no-`[Timeout]` test bodies on a `Task.Run` worker, so the test thread is itself a thread-pool thread (memory `taskrun-getresult-inlines-on-pool-thread-900`, re-checked against the packages.config version).

## 3. Hypothesis verdicts

### H1 — REFUTED

`StartObservedPrime` (296-302) returns `ApplyPrimeAsync(...).ContinueWith(...)`, i.e. the continuation, and line 276 stores exactly that in `_primeTasks`. `GetPrimeTask` (254) returns the stored value. Awaiting it therefore resumes only after `CompletePrime` has returned, which is after `_logError` has returned. The fault observer is *inside* the awaited task. The issue's "Suspected Cause" is therefore not the mechanism.

### H2 — CONFIRMED

The mechanism is the order of lines 348 and 354 combined with the test fetching the handle after the trigger. Detailed interleaving in section 4.

## 4. Exact interleaving that produces the failure

Let T be the MSTest worker executing the test (a pool thread `[M]`), and P another pool thread.

1. T, line 220: `GetPressed` → `StartPrimeIfNeeded` → `ApplyPrimeAsync` runs synchronously to line 319, where `engines.EngineActiveAsync` returns the incomplete `probe.Task` (Moq `.Returns(probe.Task)`, test line 219). The state machine suspends; because of `ConfigureAwait(false)` and the absence of a `SynchronizationContext` on an MSTest worker, the resumption is stored on `probe.Task` as a plain `Action` continuation `[D]`. `ApplyPrimeAsync` returns incomplete task A. `ContinueWith` registers continuation C on A; C is stored at line 276. `[V]`
2. T, line 223: `probe.SetException(failure)`. `TaskCompletionSource.SetException` completes `probe.Task` and runs its continuations **inline on T** (await continuations are inlined by `FinishContinuations` unless the completing thread is aborting or `RunContinuationsAsynchronously` was requested — neither applies) `[D]`. The `ApplyPrimeAsync` state machine resumes on T, the `await` rethrows `failure`, and A becomes Faulted on T. A's continuation C was registered with `TaskContinuationOptions.None` (no `ExecuteSynchronously`), so it is **queued to `TaskScheduler.Default`**, not inlined `[D]` (lines 299-301 `[V]`). Because T is a pool thread, the item lands on T's local work-stealing queue and is picked up either by T once the test body yields, or by an idle pool thread P that steals it `[D]`. `SetException` returns to the test.
3. From here T and P run concurrently:
   - T, line 224: `GetPrimeTask(SpamEngine)` → `_primeTasks.TryGetValue` (254).
   - P, running C: `CompletePrime(A, "Spam")` → status is Faulted (343) → **`TryRemove` (348)** → `GetBaseException` (350-352) → `BuildPrimeFailedMessage` (`string.Format` with `CultureInfo.CurrentCulture`, 393-401) → **`_logError` (354)** → harness `Errors.Add` (test line 415).

Interleavings:

| Label | Order | `GetPrimeTask` returns | Outcome |
|---|---|---|---|
| I-1 | T's `TryGetValue` before P's `TryRemove` | C | `await C` resumes after `CompletePrime` returns; `Errors` populated. PASS. |
| I-2 | P finishes `Errors.Add` before T's `TryGetValue` | `Task.CompletedTask` | Assertions read a list that already has one entry. PASS in practice; note that no synchronization edge orders `Errors.Add` (after `TryRemove`) before T's read, so even this passing case has no formal happens-before. |
| **I-3** | P's `TryRemove` before T's `TryGetValue`, and T's `Errors` read before P's `Errors.Add` is complete or visible | `Task.CompletedTask` | `await` returns synchronously; `harness.Errors.Should().ContainSingle()` (227-231) fails with an empty collection, or, if T reads while `List<T>.Add` is mid-write, `harness.Errors[0]` can be null and line 232 throws `NullReferenceException`. **FAIL.** |

Why I-3 is rare but real: the window on P between line 348 and the completed `Errors.Add` is ordinarily microseconds, but on the first execution in the process it includes JIT compilation of `BuildPrimeFailedMessage`, the `_logError` lambda, `LoggedError..ctor` and `List<LoggedError>.Add`, plus culture-data initialisation for `CultureInfo.CurrentCulture` `[D]`. Under `Workers=0` with other classes running on the same runner, T can be preempted after `SetException` and P can be preempted inside that window. One failure on PR #939 head `9624376dc` with passes on rerun and locally (issue.md:36) is consistent with this.

### Cancellation path

`CompletePrime` is shared by the Faulted and Canceled outcomes (343 tests only `RanToCompletion`), so the same window exists after `probe.SetCanceled()`. The two cancel tests are not exposed: `..._LogsErrorAndClearsPrimeMarker` captures the handle before the trigger (Race.cs:211-215); `..._LeavesToggleReportingUnchecked` asserts only `GetPressed == false` (Race.cs:266-269).

## 5. Complete list of ordering hazards found

| # | Hazard | Where | Effect | In scope for #942 |
|---|---|---|---|---|
| A | Marker cleared (348) before the fault is reported (354). | `CompletePrime` | A `GetPrimeTask` call made after the trigger can return `Task.CompletedTask` while the report is pending; violates the documented "await the prime deterministically" contract (238-246) and is the direct cause of the CI failure. | **Yes — root cause.** |
| B | Registration (276) races removal (348) when `ApplyPrimeAsync` completes synchronously in a non-success state. | `StartPrimeIfNeeded` / `CompletePrime` | If `TryRemove` runs before the assignment lands, the assignment re-registers an already-completed continuation and the key never re-primes for the session (the CR-2 defect class). Reachable in this very test at line 237: Moq returns the same already-faulted `probe.Task`, the awaiter reports `IsCompleted`, the state machine throws synchronously, A2 is faulted before `ContinueWith` is called, and C2 is queued before line 276 executes. No assertion follows line 242, so the test cannot fail from it, but the cleanup `await` at 242 can return before the second `_logError`, leaving a stray `Errors.Add` on a pool thread after the test method returns (benign: the harness is per-test). Reachable in production: `AppItemEngines.EngineActiveAsync` (`TaskMaster/AppGlobals/AppItemEngines.cs:101-109`) awaits `Globals.AF.Manager.Configuration`, an `AsyncLazy` backed by `Lazy<Task<T>>` (`UtilitiesCS/ReusableTypeClasses/AsyncLazy/AsyncLazy.cs:24,32`) that caches a faulted task permanently (`UtilitiesCS.Test/ReusableTypeClasses/AsyncLazy_Tests.cs:58-73`), so after one configuration-load fault every later prime faults synchronously and races on each `getPressed` poll. Already documented as NB-2 in `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/code-review.2026-09-03T06-19.md:188-221` with "Recommendation for a follow-up issue"; grep of `docs/features/potential/` finds no entry for it. | **No — promote as its own issue.** The recommended #942 reorder neither worsens nor fixes it. |
| C | Test-side: `Harness.Errors` (`List<LoggedError>`) is appended on a pool thread and read on the test thread with no synchronization in I-3. | test fixture | Secondary to A; produces the `NullReferenceException` variant of the failure. Eliminated by either fix in section 6 because both restore a happens-before edge between `Errors.Add` and the assertions. | Resolved by the fix to A. |
| D | The identity assertion at Race.cs:240-245 is weaker than its comment states: `Task.CompletedTask` is also "not the same as" `firstPrime`, so it passes even when the second marker was removed (or never re-registered under hazard B) before line 235 ran. | Race.cs test | Not flaky; the conclusion "a second prime actually started" is not fully established by that assertion. Strengthening it to `NotBeSameAs(Task.CompletedTask)` would itself be subject to hazard A, so leave it. | Observation only. |

## 6. Candidate approaches

### Approach 1 — Reorder `CompletePrime`: report the fault, then clear the marker (production)

Change lines 348-354 so the order is: compute `failure` → `_logError(...)` → `_primeTasks.TryRemove(engineName, out _)`. Update the `<summary>` at 327-331 (which lists "marker cleared … and the failure reported" in the current prose order) and add a one-sentence "why" comment: the report precedes the removal so that any holder of the prime handle, and anyone who observes the marker gone, is guaranteed the fault has already been reported.

Why it fixes the test with no test change: in I-1 the await still covers the report; in I-2/I-3 the only way `GetPrimeTask` returns `Task.CompletedTask` is by observing the removal, and `Errors.Add` now precedes the removal in P's program order. `ConcurrentDictionary.TryRemove` publishes the bucket change under a lock with a volatile write and `TryGetValue` reads the bucket with a volatile read `[D]`, so observing the removal acquires everything P wrote before it, including the list append. Hazard C disappears with it.

Invariant check (all `[V]`):
- Lines 327-331 describe the order in prose only; no sentence names it as load-bearing.
- The #735 acceptance wording is order-agnostic: plan P3-T9 requires only that "the marker removal and the log call are on the non-completed path" (`plan.2026-09-02T12-04.md:265-266`); `spec.md:172` likewise.
- No production consumer depends on the order: the sink is `logger.Error` (does not re-enter the coordinator); `GetPrimeTask` has no production callers.
- "Does a fault re-prime rely on the marker being cleared before observers see the fault?" No. A re-prime is triggered only by a later `GetPressed` poll from Office, which is independent of the sink and of any awaiter. With the reorder, a poll arriving between the report and the removal sees the marker still present and does not re-prime on that poll; the next poll re-primes. Before the reorder the opposite window existed (a re-prime could start, and even log, before the first fault's report), so log order could invert. Neither window is asserted by any test.
- The type's "exactly one `catch`" invariant (153-155, 284-285) is untouched.
- A throwing sink: today a throwing `_logError` faults the continuation (breaking the "never faults" contract at 288) with the marker already cleared; after the reorder it would additionally leave the marker registered. The production sink is log4net's `logger.Error`, which does not throw on appender failure. If unconditional marker clearing is wanted, `try { _logError(...); } finally { _primeTasks.TryRemove(...); }` preserves it without adding a `catch`; this is optional hardening, not required for #942.

Alignment: minimal (two statements plus comments), keeps the fixture unchanged, and makes the `GetPrimeTask` doc contract true. Production changes: yes, one method.

### Approach 2 — Capture the handle before the trigger (test-only)

In the flaky test, replace lines 220-224 with: `GetPressed(SpamEngine); var prime = GetPrimeTask(SpamEngine); probe.SetException(failure); await prime;`. Precedent: Race.cs:211-215. Deterministic under the current production order because the captured handle is C regardless of when P runs.

Limitations: leaves the documented `GetPrimeTask` contract false on the failure path, so any future post-trigger call (the cleanup awaits at :242 and Race.cs:272 already are such calls) can still return early; cannot serve as a "fails before, passes after" regression test because it changes the observation, not the behaviour; the issue's proposed validation ("show that the test fails when the ordering is inverted", issue.md:57) presupposes an ordering in the code under test. Production changes: none.

### Recommendation

**Approach 1**, with a new deterministic regression test (section 8) whose red state before the reorder is the negative control. Approach 2 may additionally be applied to the original test as hardening; it is not required, and leaving the original test untouched preserves it as the "original repro" the bugfix workflow asks to re-run.

### Rejected alternatives (brief)

- `TaskContinuationOptions.ExecuteSynchronously` on the continuation: with an already-completed antecedent it runs inline *inside* `ContinueWith`, i.e. before line 276, which makes hazard B deterministic instead of racy; it also does not restore the contract when the antecedent completes later on another thread.
- Taking `_primeGate` inside `CompletePrime`: addresses hazard B, not A; out of scope.
- Injecting a `TaskScheduler` seam so tests can run the continuation on demand: adds a fifth constructor argument and a production seam that nothing else needs; the in-sink probe (section 8) obtains the same determinism without it.
- `[DoNotParallelize]`, `Workers=1`, retries, sleeps, `Task.Delay`: prohibited by the issue and by `.claude/rules/csharp.md` "Prohibited Behaviors".

## 7. Behaviour semantics after the fix

- On any non-success prime outcome the coordinator: leaves the cache unset; reports the failure through `logError` exactly once; then clears the in-flight marker so a later read may re-prime.
- `GetPrimeTask(key)` returns a task that is incomplete for as long as the marker is registered, and the marker is registered for as long as the fault report has not yet been delivered. Corollary: a caller that obtains `Task.CompletedTask` for a key whose prime failed can rely on the report having already been delivered.
- Success path, toggle path, cancellation synthesis (350-352), message text, and the "never faults" nature of the handle are unchanged.

## 8. Testing implications

### 8.1 Regression test (deterministic, no timing)

Discriminator: the state of the prime marker **at the moment the sink is invoked**. That moment is strictly after line 348 in the current code and strictly before the removal in the fixed code, on the same thread, so the outcome is a function of program order, not of scheduling.

Design (MSTest, Moq strict mock, FluentAssertions; Arrange-Act-Assert):

- Harness change (main fixture file): add `internal Action<string, Exception> OnLogError { get; set; }` and invoke it from the `logError` lambda after `Errors.Add`, mirroring `OnInvalidate` (409-413, 430-434). About eight lines; the file goes from 459 to roughly 467 lines.
- New test, suggested name `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged`:
  - Arrange: harness; held `probe`; strict `EngineActiveAsync(SpamEngine)` returns `probe.Task`; `GetPressed(SpamEngine)`; `var prime = GetPrimeTask(SpamEngine)` (captured before the trigger so the await itself is deterministic — Race.cs:211 precedent); `Task handleSeenBySink = null; harness.OnLogError = (_, _) => handleSeenBySink = harness.Coordinator.GetPrimeTask(SpamEngine);` (probe from inside the sink — the OnInvalidate precedent at 252-276).
  - Act: `probe.SetException(failure); await prime;`
  - Assert: `handleSeenBySink.Should().BeSameAs(prime, "while the fault is being reported the prime handle must still be registered, so a caller that fetches it after the trigger awaits the report")`; `harness.Errors.Should().ContainSingle()`; `harness.Errors[0].Exception.Should().BeSameAs(failure)`; `harness.Coordinator.GetPrimeTask(SpamEngine).Should().BeSameAs(Task.CompletedTask, "once the handle has completed the marker has been cleared so a later read may re-prime")`.
  - Thread safety: `handleSeenBySink` is written on P before C completes and read on T after `await prime`; task completion provides the happens-before edge. No blocking, no gate, no timer, no `Thread.Sleep`/`Task.Delay`, no temp file, no `[DoNotParallelize]`.
  - Before the reorder the sink runs after `TryRemove`, so `GetPrimeTask` returns `Task.CompletedTask` and the first assertion fails deterministically. After the reorder it returns C (`== prime`) and passes.
- A cancellation twin (`probe.SetCanceled()`, `BeAssignableTo<OperationCanceledException>`) is optional; `CompletePrime` is a single path for both outcomes, so one test covers the changed lines.

File placement: `EngineToggleStateCoordinatorTests.cs` is at 459/500 lines; put the new test in a third partial (for example `EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs`) or append a new region to `Race.cs` (277 lines). The test project uses explicit `<Compile Include>` items (`TaskMaster.Test/TaskMaster.Test.csproj:352, 359`), so a new file needs a csproj entry.

### 8.2 Negative control

Run the new test against the current statement order (before applying the reorder, per the bugfix workflow's failing-test-first step): it fails on the `BeSameAs(prime)` assertion every time. Then apply the reorder and rerun: it passes. Record both as TRX-derived summaries under `<FEATURE>/evidence/regression-testing/` (raw TRX is not committable per CLAUDE.md "Committed Test Evidence Format"). This is stronger than the original test, which cannot serve as a negative control because its failure depends on scheduling. An equivalent second demonstration is to temporarily invert the two statements after the fix and observe the same deterministic failure.

### 8.3 Existing tests

- `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` becomes deterministic without modification under Approach 1 (section 6 reasoning). Optionally harden it with the capture-before-trigger form.
- No existing assertion depends on the current order (section 6 invariant check).
- Coverage: the reorder changes no branch structure; the changed lines are exercised by the existing fault test, the two cancel tests, and the new test.

### 8.4 Alternative regression design considered and not recommended

Gating the injected `logError` on a `ManualResetEventSlim`/`TaskCompletionSource` wait so the test thread can call `GetPrimeTask` while the sink is held open also discriminates the two orders, but it blocks a pool thread on a test-controlled gate. Under `Workers=0` saturation the test thread's own resumption may need a pool thread, and a blocking wait is adjacent to the banned "real wall-clock waits". The in-sink probe gives the same discrimination without blocking.

## 9. Files that would change (repository-relative)

| File | Change |
|---|---|
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | `CompletePrime`: move `_primeTasks.TryRemove` after `_logError`; update `<summary>` at 327-331 and add the why-comment; optionally tighten the `GetPrimeTask` `<returns>` (243-246) to state the guarantee. |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | `Harness`: add `OnLogError` hook invoked from the `logError` lambda. Optional: capture-before-trigger hardening at 220-224. |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` (new partial) — or a new region in `EngineToggleStateCoordinatorTests.Race.cs` | The regression test in 8.1. |
| `TaskMaster.Test/TaskMaster.Test.csproj` | `<Compile Include>` for the new partial (only if a new file is created). |
| `docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/spec.md` | Fill Root Cause (replace the H1 wording with H2), Proposed Fix, Test Strategy, ACs. |
| `docs/features/potential/<timestamp>-engine-toggle-prime-marker-registration-race.md` | New potential entry for hazard B (NB-2), promoted through the MCP lifecycle; not part of the #942 code change. |

## 10. Survey: does any other test have the same pattern?

Exhaustive grep of `GetPrimeTask` across `*.cs` finds only the two fixture files listed in section 2.2; every post-trigger use is either on the success path (marker never removed), assertion-free, or capture-before-trigger. Production `ContinueWith` sites outside tests (`Grep "\.ContinueWith\("`, excluding `*.Test/**`): `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs:46` and `UtilitiesCS/OutlookObjects/Folder/OutlookFolderTreeService.cs:405` discard a fault-logging continuation (`_ = …`, the genuine H1 shape), but no test asserts on those logs (`TaskMaster.Test` grep for "Startup inbox processing failed"/`ProcessStartupInboxItemsAfterReadinessHookup`: none; `UtilitiesCS.Test` grep for `ObserveFault`/`cleanupFailureObserver`: none). `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs:148-162` uses `ExecuteSynchronously` plus an explicit already-completed check, and `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:240` stores its continuation as the awaited handle; neither removes a marker before logging. Conclusion: no other test in the repository currently carries the #942 pattern.

## 11. Memory hygiene

No absolute host paths are embedded in this artifact. No evidence artifacts were produced by this research session.
