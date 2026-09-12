# Research: `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs`

- Feature: #455 (`quickfiler-breadcrumb-dropdown-webview-coverage`), epic child F13 of epic #136
- Production file: `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` — **477 lines**
- Researched: 2026-08-07
- Complexity band: C3. **This is the file that earns F13 its C3 band.** It owns the generation
  lease, the cancellation completion source, the `lock`, and every open/close ordering invariant.

---

## 0. Measured baseline and deviation notices

### 0.1 Measured baseline (indicative, from committed Cobertura)

Source: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`, line 10104:

```
<class line-rate="0.991254" branch-rate="0.918605" complexity="98"
       name="QuickFiler.Viewers.BreadcrumbDropDownOpenLease"
       filename="QuickFiler\Viewers\BreadcrumbDropDownOpenLifetime.cs">
```

| Metric | Measured | Gate | Status |
| --- | --- | --- | --- |
| Line coverage | **99.13%** (340/343 coverable) | >= 80% (issue #136 AC1) | PASS |
| Branch coverage | **91.86%** (79/86 outcomes) | >= 75% (`.claude/rules/general-unit-test.md`) | PASS |

Exactly **three** lines are uncovered: `:197`, `:238`, `:359`.

Independent arithmetic check confirming the line-number mapping is exact: the seven uncovered branch
outcomes enumerated in §2 below (`:75`, `:123`, `:237`, `:260` x2, `:295`, `:358`) sum to exactly
`86 - 79 = 7`. The report's line numbers therefore align with the current worktree file with no
offset.

### 0.2 DEVIATION — the class `name` attribute is misleading; do not key the harness on it

The Cobertura `<class name="...">` for this file is `QuickFiler.Viewers.BreadcrumbDropDownOpenLease`
— the 11-line `readonly struct` declared first in the file (`:10-20`) — **not**
`BreadcrumbDropDownOpenLifetime`, which is the 453-line class that supplies nearly all of the
`<lines>` content (`complexity="98"`). This is direct confirmation of the epic's harness directive
("Aggregate per file, not per class", epic.md:530): a harness that keys on `name` and looks for
`BreadcrumbDropDownOpenLifetime` will report this file as absent. **The harness must key on
`filename`.** F1's brief already says this; this file is a concrete positive control for it.

### 0.3 DEVIATION — the branch-coverage premise in the delegation brief is disproved

The brief anticipates branch coverage below the 75% floor. Measured branch coverage is **91.86%**,
16.9 points above the floor. Three of the seven uncovered outcomes are structurally unreachable
(see §2.1). The genuinely reachable, genuinely valuable gaps number **four**, and two of those
(`:197`, `:237`/`:238`) are cancellation/error paths that carry real behavioral meaning.

---

## 1. Structural map

Two types in the file.

### 1.1 `internal readonly struct BreadcrumbDropDownOpenLease` (`:10-20`)

| Member | Lines |
| --- | --- |
| ctor `(long generation, Task cancellation)` | `:12-16` |
| `long Generation { get; }` | `:18` |
| `Task Cancellation { get; }` | `:19` |

Immutable value object; the unit of "is this open attempt still the current one". No branches.

### 1.2 `internal sealed class BreadcrumbDropDownOpenLifetime : IDisposable` (`:23-476`)

Fields:

| Field | Line | Role |
| --- | --- | --- |
| `readonly object _sync` | `:25` | the single lock protecting every mutable field below |
| `readonly BreadcrumbDropDownHost _host` | `:26` | back-reference (circular by construction: `BreadcrumbDropDownHost.cs:172` passes `this`) |
| `readonly BreadcrumbPopupUiOperations _uiOperations` | `:27` | dispatch/adapter seam |
| `TaskCompletionSource<bool> _cancellation` | `:28` | current generation's cancellation signal; replaced by `InvalidateCore` |
| `TaskCompletionSource<bool>? _openCompletion` | `:29` | completion handed to `OpenAsync` callers |
| `volatile TaskCompletionSource<bool>? _pendingCloseCompletion` | `:30` | **the only `volatile` in the file** |
| `Task<bool>? _openTask` | `:31` | shared task returned to concurrent openers |
| `long _generation` | `:32` | lease generation counter |
| `bool _disposed` | `:33` | terminal flag |

Members:

| Member | Lines | Access |
| --- | --- | --- |
| ctor `(host, uiOperations)` | `:35-42` | `internal` |
| `OpenAsync(Rectangle, Rectangle, Size)` | `:44-71` | `internal` |
| `TryCancelPendingOpen(Action)` | `:73-94` | `internal` |
| `IsCurrent(lease)` | `:96-100` | `internal` |
| `IsPendingClose` | `:102` | `internal` |
| `Schedule(Action)` | `:104-111` | `internal` |
| `Schedule(Func<Task>)` | `:113-125` | `internal` |
| `InvalidateAndSchedule(Action)` | `:127-132` | `internal` |
| `InvalidateAndSchedule(Func<Task>)` | `:134-135` | `internal` |
| `DisposeAndSchedule(Func<Task>)` | `:137-138` | `internal` |
| `Dispose()` | `:140-151` | `public` |
| `CompleteOpenAsync(kickoff, lease, completion)` | `:153-185` | private `async` |
| `CompletePendingCloseAsync(kickoff, completion)` | `:187-212` | private `async` |
| `OpenCoreAsync(anchor, work, size, lease)` | `:214-254` | private `async` |
| `ShowCurrentSurface(placement, lease)` | `:256-276` | private |
| `ValidatePlacement(placement)` | `:278-285` | private |
| `FocusCurrentSurface(lease)` | `:287-305` | private |
| `EnsureSurfaceAsync(lease)` | `:307-360` | private `async` |
| `RetainCurrentSurface(installed, lease)` | `:362-374` | private, returns `bool?` |
| `RunIfCurrent(lease, operation)` | `:376-377` | private |
| `HandleOpenFailureAsync(exception, lease)` | `:379-403` | private `async` |
| `IsLifecycleCurrent(lease, allowDisposed)` | `:405-409` | private |
| `IsCurrentCore(lease, allowDisposed)` | `:411-414` | private |
| `ScheduleInvalidating(operation, disposing)` | `:416-437` | private |
| `ScheduleObserved(operation)` | `:439-440` | private |
| `RunOnOwnerAsync<T>(operation)` | `:442-451` | private `async` |
| `ObserveScheduledAsync(kickoff)` | `:453-464` | private static `async` |
| `InvalidateCore()` | `:466-472` | private |
| `NewCompletionSource()` | `:474-475` | private static |

### 1.3 Seams

Only two injected dependencies, both via the constructor (`:35-42`): the host and
`BreadcrumbPopupUiOperations`. The latter is the entire dispatch/UI seam — its own constructor
(`BreadcrumbPopupUiOperations.cs:62-78`) takes six delegates, and its `BreadcrumbUiDispatcher`
(`BreadcrumbUiDispatcher.cs:25`) takes a `SynchronizationContext` and an `Action<Exception>` error
sink. **That is the complete seam surface, and it is sufficient.** No new seam is needed for any
recommended test in §8.

The type is `internal`; `QuickFiler/Properties/AssemblyInfo.cs:5` grants
`InternalsVisibleTo("QuickFiler.Test")`, so tests construct it directly — see
`BreadcrumbDropDownLifecycleCoverageTests.cs:70` (`new OpenLifetime(_harness.Host, _harness.Operations)`)
and `:230-235` (null-guard `ParamName` assertions).

---

## 2. Branch inventory — the core deliverable

| # | Line | Construct | Sides | Gap | Covering test |
| --- | --- | --- | --- | --- | --- |
| B1 | `:40` | `host ?? throw ArgumentNullException` | 2/2 | none | `BreadcrumbDropDownLifecycleCoverageTests.Host_CoreConstructorNullDependencies_*` (`:230`) |
| B2 | `:41` | `uiOperations ?? throw` | 2/2 | none | same (`:233`) |
| B3 | `:55` | `if (_openTask != null) return _openTask;` | 2/2 | none | `ConcurrentOpenAsync_PendingInitializationIsSharedAndOpensOnePopup` (`BreadcrumbDropDownLifecycleConcurrencyTests.cs:22`) |
| B4 | `:75` | `_ = closeOperation ?? throw ArgumentNullException` | **1/2** | **throw side uncovered** | — |
| B5 | `:80` | `if (_disposed \|\| _openCompletion == null \|\| _pendingCloseCompletion != null) return false;` | 6/6 | none | `BreadcrumbPendingOpenCloseTests` (`:22`, `:124`), `Host_DisposeAndUseAfterDispose_*` |
| B6 | `:118` | `if (_disposed) return;` in `Schedule(Func<Task>)` | 2/2 | none | `OpenLifetime_DisposeIsIdempotentAndSuppressesLaterSchedules` (`:68`) |
| B7 | `:123` | ternary `IsLifecycleCurrent(lease, false) ? operation() : Task.CompletedTask` | **1/2** | **stale-lease (`Task.CompletedTask`) side uncovered** | — |
| B8 | `:145` | `if (_disposed) return;` in `Dispose()` | 2/2 | none | `OpenLifetime_DisposeIsIdempotentAndSuppressesLaterSchedules` |
| B9 | `:164` | `result = opened && IsCurrent(lease);` | 2/2 | none | `ResetLifecycle_LateFailureCannotOverwriteLaterSuccessfulLifecycle` (`:163`) |
| B10 | `:174` | `if (!ReferenceEquals(_pendingCloseCompletion, completion))` | 2/2 | none | `BreadcrumbPendingOpenCloseTests.CloseWhileFactoryPending_*` |
| B11 | `:176` | `if (ReferenceEquals(_openCompletion, completion))` | 2/2 | none | same |
| B12 | `:181` | `completion.TrySetResult(result && IsCurrentCore(lease, false));` | 2/2 | none | `Reset_DuringPendingInitializationDisposesLateSuccessAndAllowsFreshOpen` (`:54`) |
| B13 | `:202` | `if (ReferenceEquals(_openCompletion, completion))` in `CompletePendingCloseAsync` | 2/2 | none | `BreadcrumbPendingOpenCloseTests` |
| B14 | `:207` | `if (ReferenceEquals(_pendingCloseCompletion, completion))` | 2/2 | none | same |
| B15 | `:237` | `if (!placement.HasValue)` | **1/2** | **null-placement side uncovered; `:238` `return false;` never executes** | — |
| B16 | `:243` | `if (!shown) return false;` | 2/2 | none | `OpenAsync_ShowCallbackResetsLifecycle_StopsBeforeFocus` (`BreadcrumbDropDownCoverageThresholdTests.cs:117`) |
| B17 | `:260` | `RunIfCurrent(A) && RunIfCurrent(B) && RunIfCurrent(C)` in `ShowCurrentSurface` (two `&&` jumps) | **2/4** | **both `&&` false-short-circuits uncovered** | — |
| B18 | `:274` | `return IsCurrent(lease) && _host.OpenState;` (third operand body) | 2/2 | none | `OpenAsync_ShowCallbackResetsLifecycle_StopsBeforeFocus` |
| B19 | `:280` | `if (placement.Bounds.Width == 0 \|\| placement.Bounds.Height == 0) throw` | 4/4 | none | `OpenAsync_ZeroWorkingArea_RestoresSelectionAndFocus` (`BreadcrumbDropDownHostTests.cs:167`) |
| B20 | `:288` | first `RunIfCurrent` of `FocusCurrentSurface`'s `&&` | 2/2 | none | `OpenAsync_FocusCallbackFailsAfterShow_ClosesThenPermitsRetry` (`:141`) |
| B21 | `:292` | `if (!_host.OpenState) return false;` | 2/2 | none | same |
| B22 | `:295` | `return IsCurrent(lease) && _host.OpenState;` in `FocusCurrentSurface` | **1/2** | **false side uncovered** | — |
| B23 | `:309` | `if (_host.HasInstalledSurface)` | 2/2 | none | `Host_InstalledMessengerAndAlreadyOpenPath_ReuseAndFocusCurrentSurface` (`:157`) |
| B24 | `:324` | `if (installed == null) return false;` | 2/2 | none | `OpenAsync_ResetWhileReadinessPending_CancellationRejectsSurface` (`:214`) |
| B25 | `:330` | `if (retained == true) return true;` | 2/2 | none | happy path + `OpenAsync_ReadyHandlerResetsLifecycle_RejectsInstalledSurface` (`:92`) |
| B26 | `:332` | `if (!retained.HasValue)` | 2/2 | none | same |
| B27 | `:349` | `if (installed != null \|\| IsCurrent(lease))` in the outer `catch` | 4/4 | none | `OpenLifetime_StaleAndFailedRetention_CleansEachSurfaceExactlyOnce` (`:99`) |
| B28 | `:358` | `throw;` — compiler-attributed jump inside the async rewrite | **1/2** | **uncovered; `:359` (closing brace of the rethrowing `catch`) never executes** | — |
| B29 | `:367` | `if (!IsCurrent(lease)) return null;` in `RetainCurrentSurface` | 2/2 | none | `OpenAsync_ReadyHandlerResetsLifecycle_RejectsInstalledSurface` |
| B30 | `:377` | `IsCurrent(lease) && operation()` in `RunIfCurrent` | 2/2 | none | `OpenAsync_ShowCallbackResetsLifecycle_StopsBeforeFocus` |
| B31 | `:390` | `if (!IsCurrent(lease)) return;` in `HandleOpenFailureAsync` | 2/2 | none | `ResetLifecycle_LateFailureCannotOverwriteLaterSuccessfulLifecycle` (`:163`) |
| B32 | `:412` | `(allowDisposed \|\| !_disposed) && lease.Generation == _generation && !lease.Cancellation.IsCompleted` | 6/6 | none | broad |
| B33 | `:422` | `if (_disposed) return;` in `ScheduleInvalidating` | 2/2 | none | `Host_DisposeAndUseAfterDispose_FollowDeterministicContract` (`:197`) |
| B34 | `:426` | `if (_pendingCloseCompletion == null)` | 2/2 | none | `BreadcrumbPendingOpenCloseTests.ToggleAndEscapeWhileOpenIsPending_*` (`:124`) |
| B35 | `:435` | ternary `IsLifecycleCurrent(lease, disposing) ? operation() : Task.CompletedTask` | 2/2 | none | `Reset_DuringPendingInitialization*` |
| B36 | `:448` | `if (running == null) throw new InvalidOperationException("The popup operation could not be scheduled.")` | 2/2 | none | reached via the owner-thread-only dispatcher (`BreadcrumbUiDispatcher.cs:97-105`) |

Non-branching control flow that still carries behavior (no condition-coverage attribution, but must
be asserted):

| Line(s) | Construct | Currently asserted? |
| --- | --- | --- |
| `:160-169` | `try/catch (Exception)` in `CompleteOpenAsync` -> `HandleOpenFailureAsync` | Yes — `CurrentLifecycle_FactoryFailureRemainsObservableAndRestoresOnce` (`:198`) |
| `:170-184` | `finally` with `lock (_sync)` completion arbitration | Yes |
| `:196-197` | `catch { }` in `CompletePendingCloseAsync` | **No — `:197` uncovered** |
| `:198-211` | `finally` with `lock (_sync)` + unconditional `TrySetResult(false)` | Yes |
| `:221-253` | `try/catch (Exception)` in `OpenCoreAsync` | Yes |
| `:345-359` | outer `catch` + inner `catch (Exception cleanupFailure)` in `EnsureSurfaceAsync` | Partly — inner catch body `:356` covered; `:359` uncovered |
| `:384-402` | `try/catch (Exception rollbackFailure)` in `HandleOpenFailureAsync` | Yes — `OpenLifetime_RollbackReporterFailure_IsContainedAndPrimaryIsRetained` (`:82`) |
| `:455-463` | `try/catch { }` in `ObserveScheduledAsync` | Yes — `OpenLifetime_ScheduleOverloads_RunSuccessAndContainReportedFaults` (`:48`) |

**Uncovered lines:** `:197`, `:238`, `:359`. No others.

### 2.1 Reachability triage of the seven uncovered outcomes

| Gap | Reachable? | How |
| --- | --- | --- |
| B4 `:75` | **Yes, trivially** | `lifetime.TryCancelPendingOpen(null)` must throw `ArgumentNullException` with `ParamName == "closeOperation"`. |
| B7 `:123` | **Yes** | With the queued `CapturingSynchronizationContext`: `Schedule(op)` captures a lease and posts; then `InvalidateAndSchedule(noop)` advances the generation; then `DrainAll()`. The first operation must not run. Distinct from B6, which is the *disposed* early return at `:118`. |
| B15 `:237`/`:238` | **Yes** | `BreadcrumbPopupUiOperations.PlaceSurfaceAsync` returns `null` when `isCurrent()` flips at any of its four checkpoints (`BreadcrumbPopupUiOperations.cs:204`, `:211`, `:214`, `:217`). Hook the surface `Control.SizeChanged` (fired by `control.Size = ...` at `:216`) to call `lifetime.InvalidateAndSchedule(() => { })` — a pure generation bump with no teardown. The existing `LifecycleHarness.InvalidateOnFirstPlacement` (`BreadcrumbDropDownLifecycleCoverageTests.cs:404`) hooks the same event but calls `Host.Reset()`, whose re-entrant teardown makes a later statement fault instead, which is why `:238` is measured uncovered today. |
| B17 `:260` first `&&` | **Yes** | Make `IsCurrent(lease)` false *before* `ShowCurrentSurface` runs but after placement succeeds — i.e. invalidate between the `PlaceSurfaceAsync` dispatch and the `RunAsync(() => ShowCurrentSurface(...))` dispatch. With the queued pump this is a single `DrainOne()` boundary. |
| B17 `:260` second `&&` | **No — structurally unreachable** | Operand 2 is `RunIfCurrent(lease, () => { _host.OpenState = true; return true; })`, which returns `false` only when the lease is stale. But a stale lease would already have short-circuited at operand 1, and operand 1 (`RunIfCurrent(lease, () => ValidatePlacement(placement))`) invokes no injectable callback that could invalidate in between — `ValidatePlacement` (`:278-285`) only reads `placement.Bounds`. Record as irreducible. |
| B22 `:295` | **Yes** | Inject a `focusPending` delegate that invalidates the lifetime (e.g. calls `Host.Reset()`); `_host.FocusPending()` at `:294` then makes `IsCurrent(lease)` false at `:295`. Existing coverage only makes `focusPending` *throw* (`OpenAsync_FocusCallbackFailsAfterShow_ClosesThenPermitsRetry`), not invalidate. |
| B28 `:358`/`:359` | **No — compiler artifact** | `:358` is an unconditional `throw;` and `:359` is the closing brace of the `catch` block it terminates. Control can only reach `:359` if the rethrow does not occur, which cannot happen. This is the async state-machine rewrite attributing a jump and a leave-target to source lines. **`:359` is why this file cannot reach 100% line coverage**, and `:358`'s second outcome is why it cannot reach 100% branch. Record both as irreducible. |
| `:197` (line, not branch) | **Yes** | `catch { }` in `CompletePendingCloseAsync` fires when the scheduled close operation faults. Route: with an open pending (factory `TaskCompletionSource` unresolved), inject a `cancelSelection` that throws, then call `Host.Close(Uncommitted)`. `Close` takes the `TryCancelPendingOpen` path (`BreadcrumbDropDownHost.cs:254`); the close operation is `() => CompleteClose(reason, OpenState)`; `CompleteClose` proceeds because `IsPendingClose` is already `true` (`BreadcrumbDropDownHost.cs:387`) and `FinishClose` calls `_cancelSelection()` (`:433`), which throws out of `CompleteAll` (`:471`). The kickoff task faults and `:197` swallows it. |

---

## 3. Concurrency and ordering invariants

### 3.1 The generation-lease state machine

The core abstraction is the **lease**: `(Generation, Cancellation)` captured under `_sync`
(`:59`, `:120`, `:431`). Every asynchronous step re-validates its lease before mutating anything.
`IsCurrentCore` (`:411-414`) is the single predicate:

```
(allowDisposed || !_disposed) && lease.Generation == _generation && !lease.Cancellation.IsCompleted
```

`InvalidateCore` (`:466-472`) is the single invalidation primitive: it increments `_generation`,
swaps in a fresh `_cancellation`, and **returns the old completion source so the caller can signal it
outside the lock** (`:64`, `:86`, `:150`, `:433`). That "signal outside the lock" discipline is
deliberate and load-bearing — `TrySetResult` runs continuations, so signalling under `_sync` would
risk lock re-entrancy. All four call sites obey it.

Lifecycle states and legal transitions:

| State | Representation | Legal successors |
| --- | --- | --- |
| Idle | `_openTask == null`, `_openCompletion == null`, `_pendingCloseCompletion == null` | Opening (via `OpenAsync`), Disposed |
| Opening | `_openTask != null`, `_openCompletion != null`, `_pendingCloseCompletion == null` | Opening (a second `OpenAsync` returns the same task, `:55-56`), PendingClose (`TryCancelPendingOpen`), Idle (completion, `:178-179`), Invalidated |
| PendingClose | `_pendingCloseCompletion == _openCompletion` | Idle (`CompletePendingCloseAsync` finally, `:202-208`) |
| Invalidated | generation advanced; old leases now stale | any; the stale attempt drains harmlessly to `false` |
| Disposed | `_disposed == true` | terminal |

Illegal transitions and their defences:

- **Double open.** `:55-56` returns the existing `_openTask` before doing any work. Neither a second
  lease nor a second factory invocation occurs. Covered
  (`BreadcrumbDropDownLifecycleConcurrencyTests.cs:22`, asserts `FactoryCount == 1`).
- **Two concurrent pending closes.** `:80` rejects when `_pendingCloseCompletion != null`. Covered
  (`BreadcrumbPendingOpenCloseTests.ToggleAndEscapeWhileOpenIsPending_EachClosesHostExactlyOnce`,
  `:124`).
- **Close after dispose.** `:80` rejects on `_disposed`. Covered.
- **A late open success overwriting a newer lifecycle.** `:164` and `:181` conjoin the result with
  `IsCurrent(lease)`; `RetainCurrentSurface` (`:367`) refuses to install a stale surface and returns
  `null` so `EnsureSurfaceAsync` (`:332-342`) disposes it. Covered
  (`Reset_DuringPendingInitializationDisposesLateSuccessAndAllowsFreshOpen`, `:54`).
- **A late open *failure* overwriting a newer lifecycle's exception state.** `HandleOpenFailureAsync`
  (`:390`) returns without touching `_host.LastInitializationException` when the lease is stale.
  Covered (`ResetLifecycle_LateFailureCannotOverwriteLaterSuccessfulLifecycle`, `:163`).
- **Completion arbitration between `CompleteOpenAsync` and `CompletePendingCloseAsync`.** Both take
  `_sync` in their `finally` and check `ReferenceEquals` on the *same* completion object (`:174`,
  `:176`, `:202`, `:207`). `CompleteOpenAsync` deliberately declines to complete when the pending-close
  path owns the completion (`:174`), leaving the arbitration to `:210`
  (`completion.TrySetResult(false)`). This is the subtlest ordering rule in F13 and it **is**
  covered by `BreadcrumbPendingOpenCloseTests`.

### 3.2 Primitive inventory with file:line

| Primitive | Line(s) | Notes |
| --- | --- | --- |
| `lock (_sync)` | `:53`, `:79`, `:98`, `:117`, `:143`, `:172`, `:200`, `:407`, `:420` | nine acquisitions; all short and non-nested |
| `volatile` | `:30` (`_pendingCloseCompletion`) | read lock-free by `IsPendingClose` (`:102`), which `BreadcrumbDropDownHost.CompleteClose` (`:387`) consults off-lock |
| `TaskCompletionSource<bool>` | `:28`, `:29`, `:30`, `:50`, `:76`, `:142`, `:418`, `:474-475` | all created with `TaskCreationOptions.RunContinuationsAsynchronously` (`:475`) |
| `TrySetResult` | `:64`, `:86`, `:150`, `:181`, `:210`, `:433` | never `SetResult`; double-completion is tolerated by design |
| `Task<Task<bool>>` / `Task<Task>` kickoff | `:66`, `:87`, `:154`, `:188` | the "kickoff returns the inner running task" pattern |
| Fire-and-forget `_ = ...Async(...)` | `:69`, `:92`, `:440` | three sites; all three funnel into a `try/catch` that cannot leak (`:153-185`, `:187-212`, `:453-464`) |
| `.ConfigureAwait(false)` | 17 sites, `:162`-`:457` | uniform |
| `async` methods | `:153`, `:187`, `:214`, `:307`, `:379`, `:442`, `:453` | seven |
| `async void` | **none** | |
| `CancellationToken` | **none** | cancellation is modelled as a `Task` (`BreadcrumbDropDownOpenLease.Cancellation`, `:19`) consumed by `Task.WhenAny` in `BreadcrumbPopupUiOperations.CreateAndInstallSurfaceAsync` (`:265`) |
| `Interlocked` / `SemaphoreSlim` / `Monitor.Wait` / thread creation | **none** | |

### 3.3 Thread affinity

`RunOnOwnerAsync<T>` (`:442-451`) is the single choke point: it posts a lambda that *creates* the
inner `RunAsync` task on the owner boundary, awaits the post, then awaits the inner task. If the post
never executed the lambda, `running` is `null` and `:449` throws
`InvalidOperationException("The popup operation could not be scheduled.")` — measured covered (B36).

`ScheduleObserved` (`:439-440`) wraps every scheduled operation in `ObserveScheduledAsync`
(`:453-464`), whose empty `catch` is documented at `:462` ("Dispatch and operation failures are
reported before their tasks fault"). That comment is accurate:
`BreadcrumbUiDispatcher.DispatchValue` reports to the error sink before faulting the task
(`BreadcrumbUiDispatcher.cs:217-218`).

**Every one of the invariants in §3.1 has a covering test today except the two cancellation gaps
identified in §2.1** (`:197` — close-operation faults during pending-close; `:237`/`:238` —
placement cancelled mid-flight).

---

## 4. Time dependence

**No wall-clock read, no timer, no delay, no timeout anywhere in this file.** Verified across all
477 lines: no `DateTime`, `Stopwatch`, `Timer`, `Task.Delay`, `Thread.Sleep`, `TimeProvider`,
`CancellationTokenSource(TimeSpan)`.

There is therefore **no clock seam to inject and none should be added.** The issue.md constraint
"must be covered explicitly with an injected clock and fake timers"
(`docs/features/active/.../issue.md:66`) does not apply to this file — there is nothing for a fake
timer to advance. **Report this as a deviation from the issue text.** The determinism mechanism here
is scheduler control, not clock control.

What a deterministic test needs:

1. **A manually pumped `SynchronizationContext`** —
   `BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext` (`:346`), with
   `DrainOne()` (`:404`) as the single-step primitive. Single-stepping is essential for this file:
   the reachable gaps at `:123`, `:237`, and `:260` all require invalidating the generation
   *between* two dispatched steps.
2. **`TaskCompletionSource` control of the surface factory** — `SurfaceAttempt`
   (`BreadcrumbDropDownLifecycleConcurrencyTests.cs:331`) or `LifecycleHarness.FactoryFailure`
   (`BreadcrumbDropDownLifecycleCoverageTests.cs:340`).
3. **Event-driven re-entrancy hooks** — `Control.SizeChanged` on the surface control is the existing
   idiom for "act at the exact moment placement runs"
   (`BreadcrumbDropDownLifecycleCoverageTests.cs:404`).
4. **An error sink queue** — `ConcurrentQueue<Exception>` passed to `BreadcrumbUiDispatcher`
   (`BreadcrumbDropDownLifecycleCoverageTests.cs:292`, `:306`) so reported-and-swallowed failures
   are assertable rather than invisible.

---

## 5. Error paths

| # | Line | Construct | Kind | Reachable today? | Seam needed |
| --- | --- | --- | --- | --- | --- |
| E1 | `:40` | `host ?? throw` | guard | Yes | none |
| E2 | `:41` | `uiOperations ?? throw` | guard | Yes | none |
| E3 | `:75` | `closeOperation ?? throw` | guard | **No** | none — direct `internal` call |
| E4 | `:80-81` | disposed / no-open / already-pending early `return false` | guard | Yes | none |
| E5 | `:118-119` | `if (_disposed) return;` | guard | Yes | none |
| E6 | `:145-146` | `if (_disposed) return;` in `Dispose` | idempotence | Yes | none |
| E7 | `:166-169` | `catch (Exception exception)` -> `HandleOpenFailureAsync` | rollback | Yes | none |
| E8 | `:196-197` | `catch { }` — silent swallow of a faulted pending-close kickoff | **swallow** | **No** | none — throwing `cancelSelection` |
| E9 | `:249-253` | `catch (Exception)` in `OpenCoreAsync` -> rollback, `return false` | rollback | Yes | none |
| E10 | `:281-283` | `throw new InvalidOperationException("The active working area has no space for the folder selector popup.")` | fail-fast | Yes | none |
| E11 | `:345-359` | outer `catch` -> conditional cleanup -> `throw;` | rethrow with cleanup | Yes (`:358` artifact aside) | none |
| E12 | `:354-357` | `catch (Exception cleanupFailure) { _uiOperations.Report(cleanupFailure); }` | **report-and-swallow** | Yes — `OpenLifetime_StaleAndFailedRetention_*` (`:99`) | none |
| E13 | `:367-368` | `if (!IsCurrent(lease)) return null;` | stale-lease guard | Yes | none |
| E14 | `:399-402` | `catch (Exception rollbackFailure) { _uiOperations.Report(rollbackFailure); }` | **report-and-swallow** | Yes — `OpenLifetime_RollbackReporterFailure_*` (`:82`) | none |
| E15 | `:449` | `throw new InvalidOperationException("The popup operation could not be scheduled.")` | fail-fast | Yes | none |
| E16 | `:460-463` | `catch { }` in `ObserveScheduledAsync` (documented) | **silent swallow, documented** | Yes | none |

Three exceptions are swallowed (E8, E16) or reported-and-swallowed (E12, E14). Two of those four are
already pinned by assertions on the error-sink queue. **E8 (`:197`) is the only swallow with no test
at all**, and it is the one with real behavioural weight: it decides that a *failing close* still
completes the shared open task as `false` rather than faulting it.

**No new seam is required for any error path in this file.** Every one is reachable through the
existing constructor delegates on `BreadcrumbDropDownHost` plus the `BreadcrumbUiDispatcher` error
sink.

---

## 6. Coupling to sibling-owned files

| Referenced type | Line(s) | Owner | Blocking? |
| --- | --- | --- | --- |
| `BreadcrumbDropDownHost` | `:26`, `:40`, and 18 member accesses (`:228`-`:393`) | **F13** | no |
| `BreadcrumbPopupUiOperations` | `:27`, `:41`, `:226`, `:240`, `:245`, `:310`, `:315`, `:334`, `:356`, `:386`, `:401`, `:445-446` | **F13** | no |
| `BreadcrumbPopupPlacementResult` | `:226`, `:257`, `:279` | **F13** (`BreadcrumbPopupPlacement.cs:8`) | no |
| `IWebViewMessenger` | `:312`, `:363` | **F13** | no |
| `ToolStripControlHost`, `Control`, `Rectangle`, `Size` | various | .NET | n/a |

**No reference to any F12-owned type and none to F14's `ItemViewer.Breadcrumb.cs`.** This file is
entirely inside F13's assignment boundary.

The internal members of `BreadcrumbDropDownHost` this file reaches into are worth listing, because
they constrain how F13 may refactor the host: `_host.DropDown` (`:228`, `:336`), `_host.InstalledControlHost`
(`:229`, `:369`), `_host._popupControl` (`:230`, `:370`), `_host._popupMessenger` (`:371`),
`_host.SurfaceFactory` (`:317`), `_host.Environment` (`:318`), `_host.HasInstalledSurface` (`:309`),
`_host.OpenState` (`:265`, `:275`, `:292`, `:295`), `_host.ShowPopup` (`:273`),
`_host.FocusPending` (`:294`), `_host.LastInitializationException` (`:302`, `:392`),
`_host.PublishPopupMessengerReady` (`:372`), `_host.RestoreAfterOpenFailure` (`:393`),
`_host.DisposeSurfaceAfterFailureAsync` (`:351`). These two types are effectively one unit split
across two files for the 500-line rule; any change to one must be made with the other in view.

---

## 7. Existing test inventory

No test file targets `BreadcrumbDropDownOpenLifetime` exclusively. It is exercised from:

| Test file | Lines | Relevant methods |
| --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs` | **469** | `OpenLifetime_SharedOpenWithoutPlacement_CompletesFalseAndCleansSurface` (`:33`), `OpenLifetime_ScheduleOverloads_RunSuccessAndContainReportedFaults` (`:48`), `OpenLifetime_DisposeIsIdempotentAndSuppressesLaterSchedules` (`:68`), `OpenLifetime_RollbackReporterFailure_IsContainedAndPrimaryIsRetained` (`:82`), `OpenLifetime_StaleAndFailedRetention_CleansEachSurfaceExactlyOnce` (`:99`), `Host_CoreConstructorNullDependencies_UseExactParameterContracts` (`:228`, asserts the two ctor `ParamName`s) |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` | **477** | seven cases driving `OpenCoreAsync`/`EnsureSurfaceAsync`/`ShowCurrentSurface`/`FocusCurrentSurface` failure and invalidation paths |
| `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs` | **380** | five cases driving `TryCancelPendingOpen` and the completion arbitration |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleConcurrencyTests.cs` | **406** | six cases driving lease invalidation under reset/dispose |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleTests.cs` | **277** | five cases driving surface reuse and rollback |

Key reusable assets:

- `LifecycleHarness` (`BreadcrumbDropDownLifecycleCoverageTests.cs:290-467`) — queued
  `CapturingContext`, 9-arg core host constructor, reflected `Lifetime` handle (`:319-322`),
  settable `FactoryFailure`/`ReadyAction`/`CancelAction`/`FocusAnchorAction`/`ThrowFromErrorSink`,
  and an `ErrorSnapshot` queue. **This harness can express every recommended test in §8 with two
  additions: a settable `FocusPendingAction` and a settable `PlacementAction`.**
- `InlineSynchronizationContext` (`BreadcrumbDropDownLifecycleConcurrencyTests.cs:401`) — for cases
  where synchronous re-entrancy is the point.

Only `BreadcrumbPendingOpenCloseTests.cs` (380) and `BreadcrumbDropDownLifecycleTests.cs` (277) have
meaningful headroom under the 500-line limit. `BreadcrumbDropDownLifecycleCoverageTests.cs` (469) has
31 lines — enough for at most one small case, not five.

---

## 8. Recommended test-case list

MSTest + Moq + FluentAssertions, Arrange–Act–Assert, deterministic, no temp files, no shown forms,
no popups, no `Thread.Sleep`/`Task.Delay`.

**Target file: new `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenLifetimeCancellationTests.cs`**
(~260 lines projected, carrying its own harness derived from `LifecycleHarness`). Do not extend
`BreadcrumbDropDownLifecycleCoverageTests.cs` — 31 lines of headroom is not enough, and its harness
is a private nested class in a `sealed` non-`partial` type.

**`QuickFiler.Test` is a non-SDK project with explicit `<Compile Include>` entries and no globbing** —
verified at `QuickFiler.Test/QuickFiler.Test.csproj:81-82`. The new test file needs its own entry
there. Preserve CRLF; use the Edit tool, not `sed -i`.

| # | Test name | Closes | Mechanism |
| --- | --- | --- | --- |
| T1 | `TryCancelPendingOpen_NullCloseOperation_ThrowsWithCloseOperationParamName` | B4 `:75` | Direct `internal` call; assert `ArgumentNullException.ParamName == "closeOperation"`. |
| T2 | `Schedule_LeaseInvalidatedBeforeDrain_DoesNotRunTheOperation` | B7 `:123` | `Schedule(() => ran++)`; `InvalidateAndSchedule(() => { })`; `DrainAll()`; assert `ran == 0` and the error sink is empty. |
| T3 | `OpenAsync_LeaseInvalidatedDuringPlacement_CompletesFalseWithoutShowing` | B15 `:237`/`:238` | Hook the surface `Control.SizeChanged` to call `lifetime.InvalidateAndSchedule(() => { })` (a pure generation bump, no teardown). Assert the open task completes `false`, `ShowCount == 0`, `FocusPendingCount == 0`, `LastInitializationException` is null, and the error sink is empty. |
| T4 | `OpenAsync_LeaseInvalidatedBetweenPlacementAndShow_StopsBeforeSettingOpenState` | B17 `:260` (first `&&`) | Single-step the pump: `DrainOne()` until placement has completed, invalidate, then `DrainAll()`. Assert `IsOpen == false` and `ShowCount == 0`. |
| T5 | `OpenAsync_FocusPendingInvalidatesLifecycle_CompletesFalseAndLeavesExceptionUnset` | B22 `:295` | Inject a `focusPending` that calls `Host.Reset()`. Assert the open task completes `false`, `LastInitializationException` is null (the reset path clears it), and the surface is disposed exactly once. |
| T6 | `Close_WhileOpenPending_CloseOperationThrows_CompletesFalseWithoutFaulting` | `:197` | Factory pending; throwing `cancelSelection`; `Host.Close(Uncommitted)`; drain. Assert the shared open task `Status == RanToCompletion` with `Result == false`, `IsFaulted == false`, and the thrown exception reached the error sink exactly once. |

Six independent atomic plan tasks.

### 8.1 Explicit non-goals (record on the irreducible-remainder ledger, do not test)

| Item | Reason |
| --- | --- |
| B17 `:260` second `&&` false-short-circuit | Unreachable: operand 1 would have short-circuited first, and `ValidatePlacement` (`:278-285`) invokes no injectable callback that could invalidate between the two operands. |
| B28 `:358` second outcome and line `:359` | Compiler artifact of the async rewrite around an unconditional `throw;` at the end of a `catch` block. `:359` is the leave-target of a `catch` that always rethrows. **This is the ceiling on this file's line coverage: 99.13% is effectively 100% of reachable lines.** |

After T1–T6 the projected state is **99.7% line** (`:359` remaining) and **~97.7% branch** (two
unreachable outcomes remaining).

---

## 9. 500-line compliance

- **Current: 477 lines. Headroom: 23 lines.**
- **No production change is required for any recommended test case.** T1 needs only the existing
  `internal` method; T2–T5 need only the existing `BreadcrumbDropDownHost` constructor delegates and
  the reflected `_openLifetime` handle; T6 needs only a throwing `cancelSelection`.
- **No partial split should be proposed.** With 23 lines of headroom and zero required production
  edits, adding a `<Compile Include>` entry to `QuickFiler/QuickFiler.csproj` — a file simultaneously
  edited by up to 13 sibling children (epic.md:594-617) — would introduce fan-in conflict risk for
  no coverage benefit.
- If a future change forces a split, the natural cut is the completion-arbitration pair
  (`CompleteOpenAsync` `:153-185` and `CompletePendingCloseAsync` `:187-212`, 60 lines) into
  `BreadcrumbDropDownOpenLifetime.Completion.cs`, leaving the primary at ~417 lines. That would
  require a csproj entry adjacent to line 398 (**preserve CRLF**; use the Edit tool, not `sed -i`)
  plus an F1 ledger row classified `testable` at >= 90%.
- Note the file already carries the file-splitting cost of its own: `BreadcrumbDropDownOpenLease`
  (`:10-20`) lives here rather than in its own file, which is correct — it is an implementation
  detail of exactly one class and moving it would create a new near-empty compiled file.

---

## 10. Latent defects

**D4 — `_disposed` is assigned from `disposing` rather than OR-ed in `ScheduleInvalidating`
(`:423`).** The statement is `_disposed = disposing;`. For `InvalidateAndSchedule` (which passes
`disposing: false`) this **writes `false` into `_disposed`**. It is harmless today only because the
method returns early at `:422` when `_disposed` is already `true`, so the assignment can only ever
write `false` over an existing `false`. It is nevertheless a resurrection hazard: any future edit
that relaxes or reorders the `:422` guard would let `InvalidateAndSchedule` silently un-dispose the
lifetime. `_disposed |= disposing;` would be equivalent today and safe under future edits. Impact:
latent, no current misbehaviour. Recommend promotion to a GitHub issue as a hardening change.

**D5 — `TryCancelPendingOpen` reports success before the close has run (`:93`).** The method returns
`true` immediately after scheduling, and `BreadcrumbDropDownHost.Close` (`:254`) propagates that as
its own return value. See defect D1 in artifact 01 — same root, observed from the other side.
Report-only; recommend an XML-doc clarification on `IBreadcrumbDropDownHost.Close`.

**D6 — `CompletePendingCloseAsync`'s empty `catch` at `:197` has no comment, unlike its sibling at
`:462`.** `ObserveScheduledAsync` documents why its swallow is safe; `CompletePendingCloseAsync` does
not, and its swallow is materially different (it discards a *close* failure, not a
already-reported dispatch failure). The failure does still reach the error sink via
`BreadcrumbUiDispatcher.DispatchValue` (`BreadcrumbUiDispatcher.cs:217`), so nothing is lost — but
that is not evident from the source. Recommend a one-line comment. This is a documentation defect,
in scope for F13's own change (it is a comment, not behaviour), and T6 above will pin the behaviour
the comment describes.

Per the epic's "Latent Defect Promotion" section, promote D4 and D5 via the MCP promotion lifecycle
rather than leaving them as prose here.
