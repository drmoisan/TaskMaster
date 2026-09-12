# Per-File Research — `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs`

- Epic: #136 `quickfiler-per-file-coverage`, child F13, feature issue #455
- Production file: `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` (225 lines, 275 lines of headroom)
- csproj entry: `QuickFiler/QuickFiler.csproj:405`
- Research date: 2026-08-07
- Builds on: `research/00-cross-cutting-context.md`

---

## 0. Headline and acceptance bar

**Both types in this file already pass both gates. The single residual branch outcome and the single
uncovered line are compiler-generated async-rewrite artifacts of `catch { await …; throw; }`; the
behavioural failure path they sit on is fully exercised. Recommendation: no new test needed for
coverage; one optional behavioural test is identified in §9.**

Recomputed from `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
(class element at XML line 13830; class-level `<lines>` block 14014-14225; denominator from `<line>`
child count per epic Directive B):

| Metric | Value | Floor | Margin |
| --- | --- | --- | --- |
| Line | **139/140 = 99.29%** | 80% | +19.29 |
| Branch | **41/42 = 97.62%** | 75% | +22.62 |

The `<class>` attributes read `line-rate="0.995763" branch-rate="0.986111"`; both are inflated by the
`<line>`-double-counting bug in open issue **#441**. My recomputation matches the delegating brief's
~99.3% / ~97.6% figures.

Uncovered line: **222 only**. Uncovered branch outcome: **line 221, one of two jumps**. Both are the
same artifact (§3.2).

---

## 1. Structural map — TWO types in one file

| Lines | Type | Kind |
| --- | --- | --- |
| 19-159 | `BreadcrumbNavigationReadiness` | `internal sealed class : IDisposable` |
| 162-224 | `BreadcrumbWebViewSurfaceFactory` | `internal static class` |

Both are `internal`; both are reachable from tests via `QuickFiler/Properties/AssemblyInfo.cs:5`.

File-scope type aliases at `:9-13`:
`ReadySurface = Tuple<Control, IWebViewMessenger, Task>`,
`ReadySurfaceFactory = Func<CoreWebView2Environment, Task<Tuple<Control, IWebViewMessenger, Task>>>`.

### 1.1 `BreadcrumbNavigationReadiness` members

| Lines | Member | Visibility |
| --- | --- | --- |
| 21-23 | `static readonly log4net.ILog log` | private static |
| 25 | `readonly object _sync` | private |
| 26 | `readonly string _surfaceName` | private |
| 27 | `readonly Action _detachHandlers` | private — **the injected seam** |
| 28-30 | `readonly TaskCompletionSource<bool> _completion` (`RunContinuationsAsynchronously`) | private |
| 31 | `ulong? _navigationId` | private |
| 32 | `bool _navigationRequested` | private |
| 33 | `bool _terminal` | private |
| 35-47 | `.ctor(string surfaceName, Action detachHandlers)` | internal |
| 50 | `Completion => _completion.Task` | internal property |
| 53-82 | `BeginNavigation(Action navigate)` | internal |
| 85-95 | `NavigationStarted(ulong navigationId)` | internal |
| 98-123 | `NavigationCompleted(ulong navigationId, bool isSuccess, string? failureStatus)` | internal |
| 126-139 | `Cancel()` | internal |
| 142-146 | `Dispose()` | public (`IDisposable`) |
| 148-158 | `DetachHandlers()` | private |

### 1.2 `BreadcrumbWebViewSurfaceFactory` members

| Lines | Member | Visibility |
| --- | --- | --- |
| 164-171 | `Create(IWebViewCoreInitializer, string html)` → `ReadySurfaceFactory` | internal static; 2-arg overload, resolves operations via `BreadcrumbPopupUiOperations.CaptureCurrent()` at `:170` |
| 173-186 | `Create(IWebViewCoreInitializer, string html, BreadcrumbPopupUiOperations operations)` → `ReadySurfaceFactory` | internal static; **the injected overload**; returns the closure `environment => CreateSurfaceAsync(...)` at `:185` |
| 188-223 | `CreateSurfaceAsync(IWebViewCoreInitializer, CoreWebView2Environment, string, BreadcrumbPopupUiOperations)` | private static `async Task<ReadySurface>` |

### 1.3 Constructor dependencies and existing seams, named precisely

| Consumer | Dependency | Injection form | Seam quality |
| --- | --- | --- | --- |
| `BreadcrumbNavigationReadiness` | `Action detachHandlers` | **constructor-injected delegate** (`:35`, null-guarded at `:45-46`) | Strong. This is the *only* collaborator; the type is otherwise pure state. |
| `BreadcrumbNavigationReadiness` | `string surfaceName` | constructor value, guarded at `:37-43` | n/a |
| `BreadcrumbWebViewSurfaceFactory` | `IWebViewCoreInitializer` (`QuickFiler/Viewers/IWebViewCoreInitializer.cs`) | **interface**, constructor-style parameter on `Create` (`:164`, `:173`), null-guarded at `:166-167` and `:179-180` | Strong. Mockable with Moq; used that way at `BreadcrumbPopupBoundaryCoverageTests.cs:96`. |
| `BreadcrumbWebViewSurfaceFactory` | `BreadcrumbPopupUiOperations` | **concrete class**, optional parameter on the 3-arg overload (`:176`), null-guarded at `:183-184` | Adequate. Not an interface, but `BreadcrumbPopupUiOperations` itself has a six-delegate constructor (`BreadcrumbPopupUiOperations.cs:62-78`) that is the real host seam — see §4. |
| `BreadcrumbWebViewSurfaceFactory` | `CoreWebView2Environment` | parameter of the returned closure (`:185`) | Forwarded only; never dereferenced by this file. Tests pass `null` safely. |

**No clock, no `TimeProvider`, no timer, no `CancellationToken` anywhere in the file.**

### 1.4 Cobertura topology — a harness-correctness finding

The report emits **exactly one `<class>` element** for this file, named
`QuickFiler.Viewers.BreadcrumbNavigationReadiness`, with `filename="QuickFiler\Viewers\BreadcrumbWebViewSurfaceFactory.cs"`.
Its `<lines>` block (XML 14014-14225) aggregates **both** types — source lines 21-158
(`BreadcrumbNavigationReadiness`) *and* source lines 165-223 (`BreadcrumbWebViewSurfaceFactory`).

But its `<methods>` block (XML 13831-14012) lists **only the nine `BreadcrumbNavigationReadiness`
members**. There is no `<method>` element for `Create`, for `CreateSurfaceAsync`, or for the async
state machine. A grep for `name="QuickFiler.Viewers.BreadcrumbWebViewSurfaceFactory"` returns **no
matches**.

Two binding consequences for F1's harness, both of which are *stronger* statements than the epic's
current Directive B:

1. **Key on `filename`, never on `<class name>`.** A harness keyed on the type name would report
   `BreadcrumbWebViewSurfaceFactory` as missing/0%. This is the second independent instance of the
   pattern in F13 — the first is `BreadcrumbPopupPlacement.cs`, whose only `<class>` is named
   `...BreadcrumbPopupPlacementResult` (see artifact `07-BreadcrumbPopupPlacement.md` §1.3).
2. **Sum the class-level `<lines>` block, never the `<method>` blocks.** Summing `<method>` children
   would undercount this file by the factory's ~29 lines and would silently drop a whole type.
   The epic's Directive B says "decide the denominator on `<line>` child count, never `line-rate`";
   it should be extended to say explicitly *class-level* `<line>` children.

This also **re-confirms the sibling artifact's refutation of epic Directive A**: there is no second
`<class>` element to union here — the writer has already merged both types and every lambda/state
machine into one element. Directive A remains a no-op for this report writer.

---

## 2. `BreadcrumbNavigationReadiness` — the navigation-ID correlation state machine

This is the concurrency core that bands F13 at C3. Documented in full, as requested.

### 2.1 State variables and the invariant they encode

Three flags under one monitor (`:25`, taken at `:60`, `:87`, `:100`, `:128`):

| Flag | Meaning | Written at |
| --- | --- | --- |
| `_navigationRequested` | `BeginNavigation` has been entered and the navigate action is about to run | `:70` |
| `_navigationId` | the first SDK navigation id observed **after** the request | `:93` |
| `_terminal` | a terminal outcome (success, failure, or cancel) has been claimed | `:106`, `:134` |

**The invariant:** exactly one terminal outcome per instance, and `_completion` resolves only for the
navigation id that the SDK reported *after* this object requested a navigation. The purpose is to stop
an unrelated in-flight navigation on the same WebView2 control from resolving this object's readiness
— the failure mode that motivated the design (see the XML doc at `:15-18`).

**Terminal-claim discipline.** Both `NavigationCompleted` (`:100-107`) and `Cancel` (`:128-135`) set
`_terminal = true` **inside** the lock and then perform `DetachHandlers()` and the `_completion`
transition **outside** it (`:109-122`, `:137-138`). That is the correct shape: the lock protects only
the claim, and no outward call (`_detachHandlers`, TCS continuations) runs under the monitor. Combined
with `RunContinuationsAsynchronously` at `:28-30`, this file is free of the lock-held-across-outward-call
hazard recorded as L2 for `BreadcrumbDropDownOpenCoordinator.cs:95` in the sibling artifact.

### 2.2 Legal transition sequence

```
construct (:35-47)
   -> BeginNavigation(navigate)          (:53-82)   sets _navigationRequested, then runs navigate()
   -> NavigationStarted(id)              (:85-95)   captures the FIRST id after the request
   -> NavigationCompleted(id, ok, status)(:98-123)  terminal; resolves _completion
   |  Cancel() / Dispose()               (:126-146) terminal; cancels _completion
```

### 2.3 Illegal / out-of-order transitions — every one is already tested

| # | Transition | Guard | Behaviour | Test |
| --- | --- | --- | --- | --- |
| I1 | `BeginNavigation(null)` | `:55-58` | `ArgumentNullException("navigate")` | `BreadcrumbPopupBoundaryCoverageTests.Part2.cs:79-82` |
| I2 | **double `BeginNavigation`** | `:66-69` | `InvalidOperationException("Navigation has already been requested.")` | `Part2.cs:84-87` (`.WithMessage("*already*")`) |
| I3 | **`BeginNavigation` after terminal** | `:62-65` | `ObjectDisposedException(nameof(BreadcrumbNavigationReadiness))` | `Part2.cs:88-91` (after `Cancel()`) |
| I4 | **`NavigationStarted` before request** | `:89` `!_navigationRequested` | silently ignored; `_navigationId` stays null | `Part2.cs:103` (`NavigationStarted(3)` issued *before* `BeginNavigation`, then id 7 is the one captured) |
| I5 | `NavigationStarted` twice (id already captured) | `:89` `_navigationId.HasValue` | second id ignored | `Part2.cs:105-106` (7 then 8; 7 wins) |
| I6 | `NavigationStarted` after terminal | `:89` `_terminal` | ignored | `Part2.cs:143` (after `Cancel`+`Dispose`) |
| I7 | **`NavigationCompleted` with a non-matching id** | `:102` `_navigationId.Value != navigationId` | ignored; `Completion` stays pending | `Part2.cs:107-108` (`NavigationCompleted(8,…)` while 7 is captured; asserts `Completion.IsCompleted == false`) and `BreadcrumbCollapsedSurfaceReadinessTests.cs:286` (`NavigationReadiness_UnrelatedCompletionCannotReleaseExactNavigation`) |
| I8 | `NavigationCompleted` before any id captured | `:102` `!_navigationId.HasValue` | ignored | covered by the same fixtures (`:102` reports `100% (6/6)`) |
| I9 | duplicate `NavigationCompleted` after terminal | `:102` `_terminal` | ignored; first outcome wins | `Part2.cs:109-111` (success then failure; asserts `RanToCompletion`) |
| I10 | **`Cancel` after terminal** | `:130-133` | returns; detach and cancel are **not** repeated | `Part2.cs:140-142` (`Cancel(); Cancel(); Dispose();` asserts `detaches == 1`) |
| I11 | **navigate throws** | `:77-81` | `Cancel()` then `throw;` — the request is unwound and the original exception propagates | `BreadcrumbCollapsedSurfaceReadinessTests.cs:337` `NavigationReadiness_FailureAndSynchronousExceptionDetachEveryPath`; Cobertura shows `:74-82` all `hits="1"` |
| I12 | **detach throws** | `:150-157` | caught, `log.Error("Breadcrumb navigation handler detachment failed.", exception)`, completion still resolves | `Part2.cs:150-168` `Readiness_DetachFailure_IsContainedAndCompletionSucceeds` |
| I13 | blank / whitespace `surfaceName` | `:37-43` | `ArgumentException("surfaceName")` | `Part2.cs:67,69` |
| I14 | null `detachHandlers` | `:45-46` | `ArgumentNullException("detachHandlers")` | `Part2.cs:68,70` |
| I15 | null / blank `failureStatus` | `:116-117` | normalised to `"Unknown"` in the exception message | `Part2.cs:117-132` (iterates `{ null, " " }`) |
| I16 | `Dispose` idempotence | `:144` delegates to `Cancel` | second call is a no-op via `:130` | `Part2.cs:142` |

**Every illegal transition in the brief's list is already covered, and `BreadcrumbNavigationReadiness`
reports 100% branch on all of its own methods** (`:37`, `:45`, `:55`, `:62`, `:66`, `:89` (6/6),
`:102` (6/6), `:110`, `:116`, `:117`, `:130` — all `100%`). There is no state-machine test gap.

### 2.4 Synchronous-detach ordering

`NavigationCompleted` calls `DetachHandlers()` at `:109` **before** resolving `_completion` at `:112`
or `:118`. That ordering is deliberate (a late SDK event must not reach a resolved object) and is
pinned by `BreadcrumbCollapsedSurfaceReadinessTests.cs:316`
`NavigationReadiness_SynchronousSuccessDetachesBeforeNavigationReturns`.

---

## 3. Branch inventory

### 3.1 Complete conditional inventory

| file:line | Construct | `condition-coverage` | Status |
| --- | --- | --- | --- |
| `:37` | `if (string.IsNullOrWhiteSpace(surfaceName))` | `100% (2/2)` | covered |
| `:45` | `detachHandlers ?? throw` | `100% (2/2)` | covered |
| `:55` | `if (navigate == null)` | `100% (2/2)` | covered |
| `:62` | `if (_terminal)` in `BeginNavigation` | `100% (2/2)` | covered |
| `:66` | `if (_navigationRequested)` | `100% (2/2)` | covered |
| `:89` | `if (_terminal \|\| !_navigationRequested \|\| _navigationId.HasValue)` — 3 short-circuit jumps | `100% (6/6)` | covered (all three operands, both outcomes each) |
| `:102` | `if (_terminal \|\| !_navigationId.HasValue \|\| _navigationId.Value != navigationId)` — 3 jumps | `100% (6/6)` | covered |
| `:110` | `if (isSuccess)` | `100% (2/2)` | covered |
| `:116` | `failureStatus ?? "Unknown"` | `100% (2/2)` | covered |
| `:117` | `string.IsNullOrWhiteSpace(status) ? "Unknown" : status` (ternary) | `100% (2/2)` | covered |
| `:130` | `if (_terminal)` in `Cancel` | `100% (2/2)` | covered |
| `:77-81` | `catch { Cancel(); throw; }` (synchronous) | not a Cobertura branch | covered (`hits=1` on 77-80) |
| `:154` | `catch (Exception exception)` in `DetachHandlers` | not a Cobertura branch | covered |
| `:166` | `if (initializer == null)` (2-arg `Create`) | `100% (2/2)` | covered |
| `:168` | `if (html == null)` (2-arg `Create`) | `100% (2/2)` | covered |
| `:179` | `if (initializer == null)` (3-arg `Create`) | `100% (2/2)` | covered |
| `:181` | `if (html == null)` (3-arg `Create`) | `100% (2/2)` | covered |
| `:183` | `if (operations == null)` | `100% (2/2)` | covered |
| **`:221`** | `throw;` inside `catch { await …; throw; }` | **`50% (1/2)`** | **UNCOVERED (1)** — compiler artifact, §3.2 |

No `switch`, no `?.`, no pattern match, no loop, no `catch` filter in the file.

Uncovered line: **`:222`** (`}` closing the catch block), `hits="0"`.

### 3.2 The single residual gap — a compiler async-rewrite artifact, not a test gap

```
216            catch
217            {
218                await operations
219                    .DisposeSurfaceAfterFailureAsync(control, messenger)
220                    .ConfigureAwait(false);
221                throw;
222            }
223        }
```

Cobertura reports `:216`, `:217`, `:218`, `:219`, `:220` and `:221` all with `hits="1"`, and `:221`
carries a `50% (1/2)` jump; `:222` has `hits="0"`.

C# does not permit `await` inside a `catch` block at the IL level, so Roslyn rewrites the region: the
`catch` captures the exception into a hoisted field and leaves the handler; after the protected
region, generated code tests the captured-exception field, performs the `await`, and rethrows through
`ExceptionDispatchInfo`. The generated test maps back to the rethrow source span. Its *false* arm — "no
exception was captured" — is unreachable, because the success path has already returned at `:210-214`.
`:222` is the leave target after an unconditional `throw;` and is likewise unreachable.

**The behavioural failure path is fully exercised.** `BreadcrumbPopupBoundaryCoverageTests.Part2.cs`
drives `VerifyFactoryFailure` for the `create`, `initialize`, `core` and `navigate` failure stages,
including `InjectedFactory_CleanupFailure_DoesNotReplacePrimaryFailure` (`:61-62`), which asserts that
a *cleanup* failure does not replace the primary failure — i.e. that
`DisposeSurfaceAfterFailureAsync` at `:218-220` runs and the original exception still propagates from
`:221`. Lines 216-221 all having `hits="1"` is the direct evidence.

**Verdict: no test can move `:221`/`:222`.** They are an artifact of the async rewrite. Any attempt to
close them would require restructuring `CreateSurfaceAsync` to avoid `await`-in-`catch` (for example
by hoisting the exception manually), which is a behaviour-neutral refactor with real regression risk
and zero user value. **Do not do it.** This file's ceiling is 139/140 line and 41/42 branch.

### 3.3 Nested-lambda instrumentation defect — **not applicable to this file**

The file carries **no `[ExcludeFromCodeCoverage]` attribute at any level** (grep: zero occurrences).
Its two lambdas — the closure returned at `:185` (`environment => CreateSurfaceAsync(...)`) and the
`async` state machine of `CreateSurfaceAsync` — are both instrumented and both covered.

For contrast, the sibling-established defect *does* bite in `BreadcrumbPopupUiOperations.cs`, where
lines 406, 409 and 471-490 sit inside `[ExcludeFromCodeCoverage]` members at `:394` and `:457` and
remain permanently uncovered. That file is not in this artifact's scope, but the boundary matters:
**everything the factory calls into on the production path terminates in one of those exempt members**
(§4.2), which is why the factory itself stays clean.

---

## 4. Seam-boundary assessment for the three exempted WebView2 files

A colleague agent is analysing `WebView2BreadcrumbHost.cs`, `WebView2Messenger.cs`, and
`WebView2CoreInitializer.cs`. Stated from this file's side only, without duplicating that work.

### 4.1 Is this factory the natural seam boundary? — **No. It sits one level above the seam.**

`BreadcrumbWebViewSurfaceFactory` is **already host-neutral**. It names two WebView2 types and touches
neither:

- `CoreWebView2Environment` (`:190`) — received as a closure parameter and forwarded verbatim to
  `operations.BeginInitializationAsync(initializer, control, environment)` at `:200-202`. Never
  dereferenced. Tests pass `null` for it safely.
- `CoreWebView2 core` (`:204`) — received from `operations.ReadCoreAsync(control)` and forwarded
  verbatim to `operations.BeginNavigationAsync(core, control, html)` at `:205-207`. Never
  dereferenced.

Every host-bound operation is behind `BreadcrumbPopupUiOperations`, whose **six-delegate constructor is
the actual seam**: `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:62-78` takes
`Func<Control> create`, `BeginInitialization initialize`, `Func<Control, WebCore> readCore`,
`Func<WebCore, Control, string, NavigationSurface> navigate`, and `Action<Control?, Messenger?> dispose`.
The factory consumes that seam through six thin wrappers — `CreateControlAsync` (`:132`),
`BeginInitializationAsync` (`:134-145`), `ObserveInitializationAsync` (`:173-174`), `ReadCoreAsync`
(`:147-154`), `BeginNavigationAsync` (`:159-171`), `ObserveReadinessAsync` (`:176-177`),
`DisposeSurfaceAfterFailureAsync` (`:188-191`) — all of which are `internal` and dispatcher-marshalled.

**Correct statement of the boundary: `BreadcrumbPopupUiOperations` is the seam; the factory is its
first consumer and is therefore fully testable with a fake operations object today** — which is
exactly what `BreadcrumbPopupBoundaryCoverageTests.cs:107-119` (`SurfaceHarness`) and
`BreadcrumbPopupBoundaryCoverageTests.Part2.cs:61-62` already do.

### 4.2 Which of the three exempted files does this boundary cover?

| Exempt file | On the factory's path? | Through what | Boundary verdict from our side |
| --- | --- | --- | --- |
| `QuickFiler/Viewers/WebView2CoreInitializer.cs` (exempt at `:15`) | **Yes** | `IWebViewCoreInitializer` (`QuickFiler/Viewers/IWebViewCoreInitializer.cs`), injected at `Create(:164, :173)` and forwarded at `:201` | The interface seam already exists and is already mocked (`BreadcrumbPopupBoundaryCoverageTests.cs:96` uses `new Mock<IWebViewCoreInitializer>(MockBehavior.Strict)`). **Nothing this file needs changed.** The colleague's exemption argument for `WebView2CoreInitializer` should note that the *consumer side* is already 100% behind the interface. |
| `QuickFiler/Viewers/WebView2Messenger.cs` (exempt at `:20`) | **Yes, indirectly** | Constructed inside `BreadcrumbPopupUiOperations.BeginProductionNavigation` (`BreadcrumbPopupUiOperations.cs:394-409`, a member-level exemption); the factory only ever sees `IWebViewMessenger` (`:196`, `:208`, `:210-214`) | The `IWebViewMessenger` seam already exists and is already used. **Nothing this file needs changed.** The colleague's coverage work on `WebView2Messenger` is entirely below this boundary. |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` (exempt at `:29`) | **No — not on this path at all** | — | `WebView2BreadcrumbHost` is the **collapsed** (Designer-anchored) surface adapter. Its seam is `IBreadcrumbWebHost` (`QuickFiler/Viewers/IBreadcrumbWebHost.cs`), consumed by F12's `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`. This factory produces the **popup** surface, a different object with a different lifetime. **Do not attempt to route `WebView2BreadcrumbHost` through this factory** — the two paths are deliberately separate and the Designer-field constraint (`ItemViewerBreadcrumbDropDownContractTests.cs:18-29`) applies only to the collapsed path. |

### 4.3 The one genuinely shared contract

`BreadcrumbNavigationReadiness` (this file, `:19-159`) is used by **both** surface paths:

- popup path — `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:19` (`using Readiness = BreadcrumbNavigationReadiness;`), `:394-409`, `:457-490`;
- collapsed path — F12's `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:434-446`
  (`NavigateWithSubscription`) and `QuickFiler/Viewers/BreadcrumbMessengerHub.cs:282, 298, 315-318, 359`.

**Therefore `BreadcrumbNavigationReadiness`'s public/internal signature is frozen across three
children (F12, F13, F14).** Any change to its constructor or to the
`BeginNavigation`/`NavigationStarted`/`NavigationCompleted`/`Cancel` shape breaks F12 at compile time.

---

## 5. Concurrency, ordering, and time

| file:line | Primitive | Notes |
| --- | --- | --- |
| `:25` | `readonly object _sync` | single monitor for the readiness state machine |
| `:60`, `:87`, `:100`, `:128` | `lock (_sync)` | four critical sections, all short and all claim-only |
| `:28-30` | `TaskCompletionSource<bool> _completion` with `RunContinuationsAsynchronously` | prevents readiness continuations running under the caller's stack — load-bearing for the no-outward-call-under-lock property (§2.1) |
| `:112` | `_completion.TrySetResult(true)` | outside the lock |
| `:118-122` | `_completion.TrySetException(new InvalidOperationException(...))` | outside the lock |
| `:138` | `_completion.TrySetCanceled()` | outside the lock |
| `:75` | `navigate()` — a caller-supplied synchronous action | invoked **outside** the lock (`:73-81`), which is why a reentrant `Cancel` from inside `navigate` cannot deadlock |
| `:152` | `_detachHandlers()` | invoked outside the lock, inside try/catch |
| `:188-223` | `async Task<ReadySurface> CreateSurfaceAsync` | six sequential `await … .ConfigureAwait(false)` calls at `:199`, `:200-202`, `:203`, `:204`, `:205-207`, `:218-220` |
| `:209` | `Task readiness = operations.ObserveReadinessAsync(navigation.Item2);` | **deliberately not awaited** — the readiness task is returned to the caller as `Item3` of the tuple |

- **No `CancellationToken`, no `SemaphoreSlim`, no `Interlocked`, no `Volatile`, no `async void`, no
  timer, no wall-clock read, no timeout, no `Thread.Sleep`/`Task.Delay`.**
- **No injected clock or `TimeProvider` seam exists, and none is needed** — nothing in this file reads
  time. `BreadcrumbNavigationReadiness` has no timeout on `Completion`; readiness resolves only when
  the SDK reports a matching navigation id, or when the owner cancels. (Whether the *absence* of a
  navigation timeout is itself a defect is out of scope: no caller in F13 or F12 imposes one either,
  and adding one would be a behaviour change.)
- **Thread affinity: none in this file.** All UI-thread marshalling happens inside
  `BreadcrumbPopupUiOperations`, which routes through `BreadcrumbUiDispatcher`. The factory's `await`s
  all use `ConfigureAwait(false)`.

**Deterministic mechanism for each currently-untested path:** none required — the only untested lines
are the unreachable compiler artifacts of §3.2. For reference, the existing fixtures achieve
determinism with `PumpSynchronizationContext` / `SurfaceHarness`
(`BreadcrumbPopupBoundaryCoverageTests.cs:87-119`) and the queued
`QueuedCreatorThreadSynchronizationContext` with `DrainOnCreatorThread()`
(`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:274-300`) — no sleeps, no live forms.

---

## 6. Error paths

| file:line | Construct | Reachable from a unit test today? | Seam needed |
| --- | --- | --- | --- |
| `:37-43` | `throw new ArgumentException("A non-empty surface name is required.", nameof(surfaceName))` | Yes — `Part2.cs:67,69` | none |
| `:45-46` | `detachHandlers ?? throw new ArgumentNullException(nameof(detachHandlers))` | Yes — `Part2.cs:68,70` | none |
| `:55-58` | `throw new ArgumentNullException(nameof(navigate))` | Yes — `Part2.cs:79-82` | none |
| `:62-65` | `throw new ObjectDisposedException(nameof(BreadcrumbNavigationReadiness))` | Yes — `Part2.cs:88-91` | none |
| `:66-69` | `throw new InvalidOperationException("Navigation has already been requested.")` | Yes — `Part2.cs:84-87` | none |
| `:77-81` | `catch { Cancel(); throw; }` — **an unfiltered catch that immediately rethrows**; compliant with `.claude/rules/general-code-change.md` because it propagates with cleanup rather than swallowing | Yes — `BreadcrumbCollapsedSurfaceReadinessTests.cs:337` | none |
| `:118-122` | `_completion.TrySetException(new InvalidOperationException($"{_surfaceName} navigation failed with status '{status}'."))` | Yes — `Part2.cs:117-132` | none |
| `:150-157` | `try { _detachHandlers(); } catch (Exception exception) { log.Error("Breadcrumb navigation handler detachment failed.", exception); }` — logs and swallows, correct for a cleanup path, uses the project log4net pattern | Yes — `Part2.cs:150-168` | none |
| `:166-167`, `:168-169` | `throw new ArgumentNullException` (2-arg `Create`) | Yes — `BreadcrumbDropDownReadinessTests.cs:158-159` | none |
| `:179-180`, `:181-182`, `:183-184` | `throw new ArgumentNullException` (3-arg `Create`) | Yes — `BreadcrumbDropDownReadinessTests.cs:160-162` | none |
| `:216-222` | `catch { await operations.DisposeSurfaceAfterFailureAsync(control, messenger); throw; }` | Yes behaviourally — `BreadcrumbPopupBoundaryCoverageTests.Part2.cs:61-62` and the `VerifyFactoryFailure` cases. The residual `:221`/`:222` instrumentation is unreachable (§3.2). | none |

**No bare `catch {}` in this file.** The two unfiltered `catch` blocks at `:77` and `:216` both
rethrow; the one at `:154` logs. Neither the `BreadcrumbPopupUiOperations.cs:349` nor the
`BreadcrumbDropDownOpenLifetime.cs:197` bare-catch finding extends here.

**Every error path is reachable with current seams. No new interface, delegate, or adapter is
required for any of them.**

---

## 7. Coupling to sibling-owned files

| Direction | Their file:line | Coupling | Mockable through an existing interface? |
| --- | --- | --- | --- |
| they → us | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:80, 89, 358, 382, 385, 397, 434, 446` — **F12** | Eight references to `BreadcrumbNavigationReadiness`; `NavigateWithSubscription` (`:434-446`) **constructs** one. `BreadcrumbNavigationSubscription` is declared at `:337` and `BreadcrumbPopupLifecycleOperations` at `:355` **inside this same 481-line F12 file**. | Not applicable — we do not call into F12. **F12's expected split of that file cannot break us, because our file references nothing in it.** The reverse is not true: our type's signature is frozen for F12. |
| they → us | `QuickFiler/Viewers/BreadcrumbMessengerHub.cs:282, 290, 298, 315, 318, 359` — **F12** | Six references; the hub holds `BreadcrumbNavigationReadiness? _pendingReadiness` and produces `Tuple<IWebViewMessenger, BreadcrumbNavigationReadiness>` candidates. | Same as above. |
| they → us | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:66, 79, 102` — **F14** | Three references to `BreadcrumbNavigationReadiness` in the candidate-factory and attach signatures. | Same as above. |
| we → them | **none** | This file references no F12- or F14-owned type. Its dependency set is `IWebViewCoreInitializer`, `IWebViewMessenger`, `BreadcrumbPopupUiOperations` (all F13-owned) plus BCL/WebView2/log4net. | — |
| we ↔ same child (F13) | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:70` | `BreadcrumbWebViewSurfaceFactory.Create(initializer, html, operations)` — the host's internal ctor overload at `:57-76`. Also `BreadcrumbPopupUiOperations.cs:19` aliases our readiness type. | Intra-child; no cross-child risk. |

**Net: zero outbound sibling coupling, seventeen inbound references across F12 and F14. Freeze every
signature on both types in this file.**

---

## 8. Existing test inventory

| Test file | Lines | Headroom | What it asserts about this file |
| --- | --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 480 | **20** | The de-facto `BreadcrumbNavigationReadiness` fixture. `:65` ctor guards (blank name, null detach); `:74` `BeginNavigation` guards (null, duplicate, post-terminal); `:98` unrelated and duplicate notifications complete the captured success exactly once; `:117` failure normalises null and blank statuses to `"Unknown"`; `:135` `Cancel`/`Dispose` idempotence; `:150` detach failure is contained and completion still succeeds. Also `:60-62` factory failure/cleanup cases via `VerifyFactoryFailure`. |
| `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | 487 | **13** | `:286` unrelated completion cannot release the exact navigation; `:316` synchronous success detaches before navigation returns; `:337` failure and synchronous-exception paths detach on every route; `:376-380` `Readiness(ulong)` helper. |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | 361 | 139 | `:87-102` `ProductionFactoryCreate_ControlledContext_CapturesWithoutInvokingAdapters` — the 2-arg `Create` overload captures without invoking any adapter (asserts `PostCount == 0`); `:104-119` `InjectedFactory_Success_UsesOwnerBoundaryAndReturnsReadySurface` — the happy path, asserting the exact call order `create, initialize, core, navigate, cleanup`; `:243` a further factory case. |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs` | 498 | **2** | `:158-162` — all five null-guard combinations across both `Create` overloads. |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | 486 | 14 | `:339` factory construction under a controlled context. |
| `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs` | 302 | 198 | `:68, 87, 200` construct `BreadcrumbNavigationReadiness` against the direct adapters. |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` / `BreadcrumbMessengerHubCoverageTests.cs` | 414 / 478 | 86 / 22 | Numerous `BreadcrumbNavigationReadiness` uses as hub candidates. F12-primary. |
| `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs` | 385 | 115 | `:264-266` `CompletedReadiness(ulong)` helper. F10-boundary. |

**There is no `BreadcrumbWebViewSurfaceFactoryTests.cs` and no `BreadcrumbNavigationReadinessTests.cs`.**
Coverage comes from nine files. The two closest to being "the owner" —
`BreadcrumbPopupBoundaryCoverageTests.Part2.cs` (20 lines of headroom) and
`BreadcrumbCollapsedSurfaceReadinessTests.cs` (13) — have effectively none. Any new test needs a new
file.

---

## 9. Recommended test-case list

**NO NEW TEST IS WARRANTED FOR COVERAGE.** Both types are at their structural ceiling: the state
machine reports 100% branch on every one of its own methods, every guard and every catch is exercised,
and the only residual instrumentation (`:221`/`:222`) is an unreachable async-rewrite artifact.

**Plan work for this file is retain-and-verify:**

| # | Task | Deliverable |
| --- | --- | --- |
| V1 | Re-measure per-file line and branch coverage on the F13 branch; confirm 139/140 line and 41/42 branch are retained. | per-file row under `<FEATURE>/evidence/qa-gates/` |
| V2 | Record in the epic coverage ledger that this file's ceiling is 99.29% line / 97.62% branch, with the §3.2 artifact explanation, so F16 does not flag it. | ledger row rationale |
| V3 | Record the harness directives from §1.4 (key on `filename`, sum class-level `<line>` children, never sum `<method>` blocks) for F1. | ledger / F1 feedback |

**One optional behavioural test**, offered because it pins a real invariant rather than a number:

| # | Test name | Target file | Value | Coverage delta |
| --- | --- | --- | --- | --- |
| O1 | `BeginNavigation_NavigateThrowsAfterConcurrentCancel_DoesNotDoubleDetach` | NEW `QuickFiler.Test/Viewers/BreadcrumbNavigationReadinessOrderingTests.cs` | Pins that when `navigate()` throws at `:75` *after* a reentrant `Cancel()` has already claimed `_terminal` from inside the navigate action, the `catch` at `:77-81` calls `Cancel()` again, `:130` short-circuits, and the detach count stays at exactly 1 while the original exception still propagates. Today `Part2.cs:135-147` covers sequential `Cancel(); Cancel();` but **not** the reentrant-from-inside-`navigate` ordering. | **Zero** — every line and branch involved is already hit. Pure regression protection. |

O1 is a legitimate behavioural test, not a coverage-manufacturing test, so it does not fall under the
`epic.md:521-522` prohibition. It is nonetheless **optional**: recommend scheduling it only if the
child has spare change budget after the WebView2 exemption-removal work, which is where F13's real
effort belongs.

**Explicitly NOT recommended:**

- No test targeting `:221`/`:222` — unreachable (§3.2).
- No restructuring of `CreateSurfaceAsync` to avoid `await`-in-`catch`. Behaviour-neutral in intent,
  regression-risky in practice, zero user value.
- No split of the two types into separate files. `CLAUDE.md` §4.1 and §C#5.1 favour one purpose per
  file and the two types are arguably distinct concerns, but the file is 225 lines (275 of headroom),
  the two types are cohesive around "one navigation of one popup surface", and a split would create a
  new production file requiring a csproj entry, a new ledger row at the **>= 90%** new-file bar, and a
  change to the Cobertura `<class>` topology that F1's harness would have to absorb mid-wave. **Not
  worth it under a no-behaviour-change coverage epic.**

---

## 10. csproj impact

- **`QuickFiler/QuickFiler.csproj`: no change** under the recommended plan. Existing entry at `:405`,
  inside the contiguous F13 block `:396-411`. (F12-owned entries are interleaved at `:393-395` and
  `:400`; expect a textual conflict at fan-in, resolved additively per `epic.md:594-617`.)
- **`QuickFiler.Test/QuickFiler.Test.csproj`: no change** unless optional test O1 is taken, in which
  case one `<Compile Include="Viewers\BreadcrumbNavigationReadinessOrderingTests.cs" />` line inside
  the breadcrumb block at `:60-89`.
- **CRLF must be preserved** on any edit — `Edit` tool or `perl -0777` with explicit `\r\n`, never a
  git-bash `sed -i` (`epic.md:610-612`).
- **Coverage ledger:** update the existing `testable` row. **No new row** unless the (not
  recommended) type split is taken, in which case the new file defaults to `testable` at **>= 90%**
  line per `epic.md:583-585`.

---

## 11. Latent defects

**No new production defect found in this file.** Both types are correctly synchronised, fail fast on
every invalid argument, hold no lock across an outward call, and swallow nothing without either
rethrowing or logging.

Two observations recorded for the ledger, neither warranting promotion:

| ID | file:line | Observation | Why not promoted |
| --- | --- | --- | --- |
| O-A | `:221-222` | Permanently-unreachable instrumentation from Roslyn's `catch { await …; throw; }` rewrite. Caps the file at 139/140 line and 41/42 branch. | Not a defect — a measurement artifact. It **is** a required ledger input so F16 does not treat 100% as achievable here. Distinct from, and additional to, the sibling-recorded nested-lambda `[ExcludeFromCodeCoverage]` defect: that one concerns lambdas inside *exempt* members, whereas this file has no exemption at all and the artifact arises purely from the async rewrite. Worth cross-referencing in the same issue if the orchestrator promotes an "instrumentation fidelity" issue. |
| O-B | `:19-159` + `:162-224` | Two unrelated top-level types share one file, and the Cobertura writer names the resulting `<class>` after the *first* one, so `BreadcrumbWebViewSurfaceFactory` is invisible to any name-keyed reader (§1.4). | Style/tooling, not runtime behaviour. The mitigation is the harness directive in §9 V3, not a code change. |

Cross-references to sibling-recorded defects, for this file specifically: the
`BreadcrumbDropDownOpenCoordinator.cs:95` lock-ordering issue, the
`BreadcrumbDropDownOpenLifetime.cs:229-230` null-forgiving deref, and the bare `catch {}` blocks at
`BreadcrumbPopupUiOperations.cs:349` / `BreadcrumbDropDownOpenLifetime.cs:197` **do not extend into
this file**. The nested-lambda instrumentation defect does not apply here (no
`[ExcludeFromCodeCoverage]` attribute present), though it does affect
`BreadcrumbPopupUiOperations.cs`, which is immediately below this file's seam boundary (§4.1).

---

## 12. Deviations from the delegation brief

| Brief statement | Finding |
| --- | --- |
| "`BreadcrumbWebViewSurfaceFactory.cs` ~99.3% line, ~97.6% branch" | **Confirmed.** 139/140 = 99.29% line, 41/42 = 97.62% branch, recomputed from class-level `<line>` children and `condition-coverage` sums. |
| "declares TWO types: `BreadcrumbNavigationReadiness` (lines 19-159) and `BreadcrumbWebViewSurfaceFactory` (162-224)" | **Confirmed exactly.** |
| All six member line ranges (`BeginNavigation` 53-82, `NavigationStarted` 85-95, `NavigationCompleted` 98-123, `Cancel` 126-139, `Dispose` 142-146, `DetachHandlers` 148-158) | **All six confirmed exactly.** |
| "navigate-throws (77-81), detach-throws (150-157)" | **Confirmed**, and both are already covered. |
| "enumerate the illegal transitions and whether each is tested" | **All sixteen enumerated in §2.3; every one is tested.** There is no state-machine gap; `BreadcrumbNavigationReadiness` reports 100% branch on all nine of its members. |
| "assess whether this factory is the natural seam boundary for the three exempted WebView2 files" | **Partly refuted.** The factory is *one level above* the seam; the seam is `BreadcrumbPopupUiOperations`'s six-delegate constructor (`BreadcrumbPopupUiOperations.cs:62-78`). The factory boundary does cover `WebView2CoreInitializer` (via `IWebViewCoreInitializer`) and `WebView2Messenger` (via `IWebViewMessenger`), but **does not cover `WebView2BreadcrumbHost` at all** — that file is on the separate collapsed-surface path behind `IBreadcrumbWebHost`. |
| "the most decision-relevant file in your batch" | **Confirmed as the most decision-relevant, but not because it needs work** — it needs none. Its decision value is the seam-boundary statement in §4 and the harness directives in §1.4. |
| Epic Directive A (union multiple `<class>` elements per filename) | **Refuted again, with a new second example.** One `<class>` per `filename`; both types and all lambdas are pre-merged by the writer. A *stronger* directive is needed instead: key on `filename`, and sum the class-level `<lines>` block rather than the `<method>` blocks, because the `<methods>` collection here omits an entire type. |

---

*No commands were executed in this session; all findings are derived from the working-tree files and
the committed Cobertura report cited in §0, with exact paths and line numbers given throughout.*
