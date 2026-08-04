# P5-T172 — UI-dispatch determinism root-cause diagnosis (read-only)

Timestamp: 2026-07-22T15-07Z

Command: `cd "C:/Users/DanMoisan/repos/TaskMaster-wt/2026-07-21T10-25" && sha256sum QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs QuickFiler/Viewers/BreadcrumbUiDispatcher.cs && wc -l QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`

EXIT_CODE: 0

## Scope statement

This task is read-only. No production, test, configuration, project, runsettings, coverage-config, or evidence file other than this artifact was created or modified while performing it. No test, build, or coverage command was executed; the existing reproduction matrix is cited as-is per the task text.

## File identity under diagnosis

| File | SHA-256 | Physical lines |
|---|---|---:|
| `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs` | `e4bd60150636a83ce977681249e03c63a2fc7ca96c32c5f8ef5bbb760926e62e` | 480 |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | `224d5614b8a293665ec22b563a9c2d7421ca1e0046a369ab4d56a728347bd391` | 455 |
| `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` | `64b341920e94238f894bb885d251420e7e2cb4263f827e3b0eeaff1863519b42` | 270 |

## Failing case

`QuickFiler.Test.Viewers.BreadcrumbUiThreadDispatchTests.SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext`,
asserting at `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs` line 55:
`Expected context.PostCount to be greater than 0 because worker completion must cross the captured UI dispatcher, but found 0`.

## Reproduction matrix (cited, not re-run)

- Two instrumented 17-class `dotnet-coverage` runs: same single case failed (159/160).
- Three instrumented isolation runs of `BreadcrumbUiThreadDispatchTests` alone: 9/9 passed each time.
- One uninstrumented 17-class VSTest run: 160/160 passed.

## Harness ordering proof (established, restated)

`RecordingSynchronizationContext.Post` (`BreadcrumbUiThreadDispatchTests.cs` lines 374-383) enqueues the callback and
increments `PostCount` **inside** the `_sync` lock (lines 376-380) and only then calls `_firstPost.TrySetResult(true)`
(line 381). Therefore `FirstPost` can never be completed while `PostCount == 0`. Observing `PostCount == 0` at line 55
after `await Task.WhenAny(population, context.FirstPost)` (line 52) proves the arm that completed was `population`, i.e.
the task returned by `BreadcrumbBridgeCoordinator.SetSuggestionsAsync` ran to completion with **no** `Post` ever reaching
the captured context.

## Complete traced path

1. `BreadcrumbUiThreadDispatchTests.cs` lines 36-44: the test installs `context` as `SynchronizationContext.Current`,
   constructs `new BreadcrumbBridgeCoordinator(messenger, provider.Object)` (line 39), and calls
   `SetSuggestionsAsync` (line 40) — all on the MSTest test thread, whose managed thread ID is denoted `X`.
2. `BreadcrumbBridgeCoordinator.cs` line 41 → line 452: the public two-argument constructor resolves its dispatcher via
   `CaptureProductionDispatcher` → `BreadcrumbUiDispatcher.CaptureCurrent()`.
3. `BreadcrumbUiDispatcher.cs` lines 44-56: `CaptureCurrent()` builds the dispatcher with `_context = context` **and**
   `ownerThreadId: Environment.CurrentManagedThreadId` (line 54), so `_ownerThreadId == X`.
4. `BreadcrumbBridgeCoordinator.cs` lines 85-95: `SetSuggestionsAsync` awaits
   `_router.SetSuggestionsAsync(rows, cancellationToken).ConfigureAwait(false)` (lines 90-92). The provider leaf-key
   resolve is gated by the test's `TaskCompletionSource` (`BreadcrumbUiThreadDispatchTests.cs` lines 27-29, 342-346), so
   the await yields and `SetSuggestionsAsync` returns an incomplete task to the test thread. The test thread then
   returns to the MSTest/thread-pool scheduler at line 51-52 (`await ... .ConfigureAwait(false)`), making thread `X`
   available to the pool.
5. `BreadcrumbUiThreadDispatchTests.cs` line 51 releases the gate from a pool thread. Because the gate was created with
   `TaskCreationOptions.RunContinuationsAsynchronously` (line 28) and the await used `ConfigureAwait(false)`, the router
   continuation is **scheduled to the thread pool**, on an arbitrary pool thread — which may be the now-free thread `X`.
6. The continuation resumes at `BreadcrumbBridgeCoordinator.cs` line 93-94 and calls
   `PostRenderAndSelectorAsync(renderJson, selectorState)`, which is the dispatcher entry
   `BreadcrumbBridgeCoordinator.cs` lines 242-252, calling `_dispatcher.Dispatch(...)` at line 247.
7. `BreadcrumbUiDispatcher.Dispatch` (lines 71-151) evaluates `IsCurrentBoundary()` at line 78.

## Every branch of `Dispatch` that can return a completed task without `SynchronizationContext.Post`

`BreadcrumbUiDispatcher.cs`:

- Lines 78-95 — `IsCurrentBoundary()` true: the action is invoked **inline** at line 84 and the method returns
  `Task.CompletedTask` at line 94. `_context.Post` (line 122) is never reached.
- Lines 97-105 — `_context == null`: reports and returns `Task.CompletedTask` at line 104. Not applicable here
  (`_context` is the test's `RecordingSynchronizationContext`).
- Lines 144-148 — `_context.Post` itself throws: `completion` is completed at line 147. Not applicable
  (`RecordingSynchronizationContext.Post` cannot throw), and in that branch a post was still attempted.

`IsCurrentBoundary()` (lines 255-263) is a three-way disjunction:

```
255        private bool IsCurrentBoundary()
256        {
257            return ReferenceEquals(_executingDispatcher, this)
258                || (_context != null && ReferenceEquals(SynchronizationContext.Current, _context))
259                || (
260                    _ownerThreadId.HasValue
261                    && Environment.CurrentManagedThreadId == _ownerThreadId.Value
262                );
263        }
```

On the router continuation thread at step 6:

- Line 257 — `_executingDispatcher` is `[ThreadStatic]` (line 14-15) and is set only inside an executing dispatch
  callback (lines 81, 126, 210). No dispatch callback is executing on that thread, so this disjunct is false.
- Line 258 — no drain is in progress at that moment (`RecordingSynchronizationContext.DrainOne` installs the context at
  test line 415 only while draining, and the first drain happens at test line 59, after the assertion), so
  `SynchronizationContext.Current` is `null` on the pool thread and this disjunct is false.
- Lines 259-262 — `_ownerThreadId` is `X` (step 3) and the continuation can be scheduled onto the recycled pool
  thread `X`. **This disjunct can be true.** When it is, line 84 runs the action inline, line 94 returns
  `Task.CompletedTask`, `SetSuggestionsAsync` completes, and `PostCount` stays `0`.

This is the only branch that reproduces the observed symptom, and it is production code.

## Corroborating discriminator (why only this one case fails)

`QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` lines 30-89
(`WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary`) drives the *identical* production path
through the same public two-argument coordinator constructor (line 46) and the same gated provider, but it observes the
post with a **blocking** `context.WaitForPost()` (line 64) instead of `await Task.WhenAny(...)`. Blocking keeps the
owner thread occupied, so the thread pool cannot recycle thread `X` for the router continuation and the thread-identity
disjunct at lines 259-262 cannot fire. The failing case at `BreadcrumbUiThreadDispatchTests.cs` line 52 releases the
owner thread precisely because it awaits, which is what exposes the defective disjunct. Instrumentation changes pool
scheduling pressure and thereby the probability that thread `X` is chosen, which explains the observed matrix
(instrumented 17-class fails, instrumented isolation and uninstrumented 17-class pass).

`BreadcrumbUiThreadDispatchTests.InboundWorkerMessage_SchedulesEveryPostAndCallbackOnOwningContext` (lines 65-105)
depends on the same disjunct through `BreadcrumbBridgeCoordinator.DispatchAsync` (lines 334-341) and is latently exposed
to the same nondeterminism.

## Internal contradiction inside the production type

`BreadcrumbUiDispatcher.DispatchValue<T>` (lines 157-235) deliberately refuses both weaker proofs, stating at
lines 164-165:

```
164            // Only a currently executing synchronous dispatcher callback proves that inline
165            // control access is safe. Ambient context and thread identity do not survive awaits.
```

and gating its inline path on `ReferenceEquals(_executingDispatcher, this)` alone (line 166). `Dispatch` violates the
same stated invariant at lines 259-262 by accepting bare thread identity as boundary proof after an await. This also
violates the plan's fixed rule 40, which prohibits inferring that an asynchronous operation remains UI-bound "because it
began on the owner thread".

## DETERMINATION

**DETERMINATION: B** — at least one production path completes the returned task without crossing the captured UI
dispatcher.

Deciding production file: `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`.

Deciding lines, quoted exactly:

- line 78: `            if (IsCurrentBoundary())`
- line 84: `                    action();`
- line 94: `                return Task.CompletedTask;`
- lines 259-262:
  ```
  259                || (
  260                    _ownerThreadId.HasValue
  261                    && Environment.CurrentManagedThreadId == _ownerThreadId.Value
  262                );
  ```

`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` is on the traced path (lines 90-94, 242-252, 452) but contains no
deciding line: it correctly delegates every post to the dispatcher. The correction scope is therefore the single
production file `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`.

Output Summary: DETERMINATION: B. The single instrumented 17-class failure is a genuine production dispatch defect in
`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`: `Dispatch` accepts bare owner-thread identity
(`IsCurrentBoundary()` lines 259-262) as proof of being on the owning boundary, so a router continuation resumed after
`ConfigureAwait(false)` on a recycled thread-pool thread whose managed ID equals the captured owner thread ID executes
`action()` inline (line 84) and returns `Task.CompletedTask` (line 94) without ever reaching `_context.Post` (line 122).
The harness ordering proof (PostCount incremented under `_sync` before `_firstPost.TrySetResult`) establishes that
`PostCount == 0` at test line 55 can only mean `population` completed with no post. The same type's `DispatchValue<T>`
already documents at lines 164-165 that "Ambient context and thread identity do not survive awaits" and refuses the same
inference, and fixed rule 40 prohibits it. Hashes: test `e4bd6015...26e62e` (480 lines), coordinator `224d5614...7bd391`
(455 lines), dispatcher `64b34192...3519b42` (270 lines). Branch B is therefore the applicable correction branch; the
scope is at most one production file, `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`. EXIT_CODE: 0.
