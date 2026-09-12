# Research: `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`

- Feature: #455 (`quickfiler-breadcrumb-dropdown-webview-coverage`), epic child F13 of epic #136
- Production file: `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` — **480 lines**
- Researched: 2026-08-07
- Complexity band: C3 (concurrency/ordering invariants)

---

## 0. Measured baseline and deviation notices

### 0.1 Measured baseline (indicative, from committed Cobertura)

Source: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`, line 12304:

```
<class line-rate="0.99422" branch-rate="0.914894" complexity="109"
       name="QuickFiler.Viewers.BreadcrumbDropDownHost"
       filename="QuickFiler\Viewers\BreadcrumbDropDownHost.cs">
```

| Metric | Measured | Gate | Status |
| --- | --- | --- | --- |
| Line coverage | **99.42%** (344/346 coverable) | >= 80% (issue #136 AC1) | PASS |
| Branch coverage | **91.49%** | >= 75% (`.claude/rules/general-unit-test.md`) | PASS |

Exactly **two** lines are uncovered: `:335` and `:377`.

The report's line numbers align exactly with the current worktree file. Verified against three
distinctive landmarks: `:46` reported as a 4/4 two-condition jump (the two `??`-throws in the
public production constructor's `this(...)` chain), `:223` as 4/4 (`HasInstalledSurface`'s
three-term `&&`), `:369` as 8-outcome (the four-term `expected != null && (...)` guard in
`TakeOwnedSurface`). The file has not changed between the #424 measurement branch and this
worktree.

### 0.2 DEVIATION — the branch-coverage premise in the delegation brief is disproved

The delegation brief states that branch coverage against the 75% floor is "a separate, likely-unmet
gate," citing F8's `EfcHomeController.Timing.cs` at 66.67% branch. **That does not hold for this
file.** The measured branch rate is 91.49%, which passes the 75% floor with 16.5 points of margin.
The same holds for both siblings (91.86% and 92.05%). See the corresponding sections in artifacts
02 and 03.

The consequence for planning is material: F13's work on these three files is **not** gap-closure
against a failing gate. It is *outcome pinning* — a small number of specific uncovered outcomes,
several of which are structurally unreachable and belong on an irreducible-remainder record rather
than in a test task. A plan that budgets a large test-authoring effort against an assumed branch
shortfall will be spending against a shortfall that does not exist.

### 0.3 DEVIATION — no `[ExcludeFromCodeCoverage]` on this file

The epic (`docs/features/epics/quickfiler-per-file-coverage/epic.md:186`) groups "the WebView2 trio
(F13)" among files absent from instrumentation. `BreadcrumbDropDownHost.cs` carries **no**
`[ExcludeFromCodeCoverage]` attribute at any level and is fully instrumented. The three genuinely
exempt F13 files are `WebView2BreadcrumbHost.cs`, `WebView2CoreInitializer.cs`, and
`WebView2Messenger.cs`, which are outside this artifact's scope.

---

## 1. Structural map

`public sealed class BreadcrumbDropDownHost : IBreadcrumbDropDownHost` (`:22`). Single type in the
file. Namespace-level `using` aliases at `:10-19` define `InstalledSurface`, `LegacySurfaceFactory`,
`OwnedSurface`, `ReadySurfaceFactory`.

### 1.1 Constructors — a six-member forwarding chain

| Lines | Access | Distinguishing parameter | Purpose |
| --- | --- | --- | --- |
| `:37-55` | `public` | `IWebViewCoreInitializer initializer, string html` | Production entry point; captures `BreadcrumbPopupUiOperations.CaptureCurrent()` |
| `:57-76` | `internal` | `+ BreadcrumbPopupUiOperations operations` | Injects operations; builds surface factory via `BreadcrumbWebViewSurfaceFactory.Create` |
| `:79-99` | `public` | `LegacySurfaceFactory surfaceFactory` | Host-neutral 2-tuple factory seam; uses `CaptureCurrentOrTests()` |
| `:101-119` | `internal` | `ReadySurfaceFactory surfaceFactory` | 3-tuple (control, messenger, readiness) factory seam |
| `:121-141` | `internal` | `+ operations` | Supplies default `closePopup` = `(popup, reason) => popup.Close(reason)` (`:140`) |
| `:143-173` | `internal` | `+ Action<ToolStripDropDown, ToolStripDropDownCloseReason> closePopup` | **Core constructor.** All nine dependencies explicit |

`BreadcrumbDropDownLifecycleCoverageTests.cs:118` asserts exactly four forwarding constructors
exist; `:228` walks the nine core-constructor parameters asserting each `ArgumentNullException`
`ParamName`.

### 1.2 Injected seams (all already present — no new seam is required)

| Field | Line | Type | Injection form |
| --- | --- | --- | --- |
| `_factory` | `:24` | `Func<CoreWebView2Environment, Task<Tuple<Control, IWebViewMessenger, Task>>>` | injectable delegate |
| `_uiOperations` | `:25` | `BreadcrumbPopupUiOperations` | adapter object (itself fully delegate-seamed at `BreadcrumbPopupUiOperations.cs:62-78`) |
| `_openLifetime` | `:26` | `BreadcrumbDropDownOpenLifetime` | constructed internally at `:172`; reachable from tests via reflection (`BreadcrumbDropDownLifecycleCoverageTests.cs:319-322`) |
| `_focusPending` | `:27` | `Action` | injectable delegate |
| `_focusAnchor` | `:28` | `Action` | injectable delegate |
| `_cancelSelection` | `:29` | `Action` | injectable delegate |
| `_showPopup` | `:30` | `Action<ToolStripDropDown, Control, Point>` | injectable delegate |
| `_closePopup` | `:31` | `Action<ToolStripDropDown, ToolStripDropDownCloseReason>` | injectable delegate — **load-bearing for the re-entrancy tests in §10** |

Mutable state: `_resetPending` (`:32`), `_programmaticClose` (`:33`), `_disposed` (`:34`),
`OpenState` (`:225`, `internal` setter), `InstalledControlHost` (`:204`), `_popupControl` (`:206`),
`_popupMessenger` (`:214`). All internal-settable, so a test can install an orphaned partial
surface without going through the factory (precedent: `BreadcrumbDropDownHostTests.cs:287-288`).

`QuickFiler/Properties/AssemblyInfo.cs:5` grants `InternalsVisibleTo("QuickFiler.Test")`, so every
`internal` member above is directly reachable from the test assembly. No reflection is strictly
required for internals (the existing tests use reflection by historical convention, not necessity).

### 1.3 Public / internal surface

| Member | Lines |
| --- | --- |
| `Anchor` / `Environment` / `DropDown` | `:176` / `:179` / `:182` |
| `ControlHost` / `PopupMessenger` / `IsOpen` / `Theme` | `:185` / `:188` / `:191` / `:194` |
| `LastInitializationException` (internal setter) | `:197` |
| `event PopupMessengerReady` | `:200` |
| `SurfaceFactory` / `InstalledControlHost` / `InstalledPopupControl` / `InstalledPopupMessenger` / `HasInstalledSurface` / `OpenState` | `:202` / `:204` / `:208-212` / `:216-220` / `:222-223` / `:225` |
| `OpenAsync` | `:228-242` |
| `Close` | `:245-255` |
| `SetTheme` | `:258-264` |
| `Reset` | `:267-272` |
| `Dispose` | `:275-282` |
| `FocusPending` / `ShowPopup` / `PublishPopupMessengerReady` | `:284` / `:286` / `:288-289` |
| `ResetCoreAsync` (private) | `:291-315` |
| `DisposeCoreAsync` (private) | `:317-340` |
| `DisposeSurfaceAsync` (private) | `:342-350` |
| `DisposeSurfaceAfterFailureAsync` (internal) | `:352-365` |
| `TakeOwnedSurface` (private) | `:367-383` |
| `CompleteClose` (private) | `:385-399` |
| `CloseNative` (private) | `:401-412` |
| `OnDropDownClosed` (private) | `:414-425` |
| `FinishClose` (private) | `:427-437` |
| `RestoreAfterOpenFailure` (internal) | `:439-451` |
| `CompleteAll` (private) | `:453-472` |
| `ThrowIfDisposed` (private) | `:474-478` |

---

## 2. Branch inventory — the core deliverable

Every conditional in the file, with the measured outcome coverage from the Cobertura evidence.
"Sides" is the tool's outcome count for that source line. Covering test named where one exists.

| # | Line | Construct | Sides | Gap | Covering test |
| --- | --- | --- | --- | --- | --- |
| B1 | `:46` | two `??`-throw guards in the public ctor's `this(...)` args (`initializer`, `html`) | 4/4 | none | `BreadcrumbDropDownHostTests.ProductionConstructor_RejectsMissingInitializerOrHtml` (`:300`) |
| B2 | `:67` | (chain forwarding, compiler jump) | 2/2 | none | `BreadcrumbDropDownLifecycleCoverageTests.Host_FourForwardingConstructors_*` (`:118`) |
| B3 | `:88` | `surfaceFactory ?? throw` (`:92`) | 2/2 | none | same |
| B4 | `:131` | (chain forwarding) | 2/2 | none | same |
| B5 | `:155` | `anchor ?? throw` | 2/2 | none | `Host_CoreConstructorNullDependencies_UseExactParameterContracts` (`:228`) |
| B6 | `:156` | `environment ?? throw` | 2/2 | none | same |
| B7 | `:157` | `surfaceFactory ?? throw` | 2/2 | none | same |
| B8 | `:158` | `focusPending ?? throw` | 2/2 | none | same |
| B9 | `:159` | `focusAnchor ?? throw` | 2/2 | none | same |
| B10 | `:160` | `cancelSelection ?? throw` | 2/2 | none | same |
| B11 | `:162` | `showPopup ?? throw` | 2/2 | none | same |
| B12 | `:163` | `closePopup ?? throw` | 2/2 | none | same |
| B13 | `:164` | `operations ?? throw` | 2/2 | none | same |
| B14 | `:223` | `InstalledControlHost != null && _popupControl != null && _popupMessenger != null` | 4/4 | none | `Host_InstalledMessengerAndAlreadyOpenPath_*` (`:157`) |
| B15 | `:235` | `if (OpenState)` in `OpenAsync` | 2/2 | none | `OpenAsync_WhenAlreadyOpen_FocusesPendingWithoutRecreatingOrShowing` (`BreadcrumbDropDownHostTests.cs:146`) |
| B16 | `:247` | `if (_disposed) return false;` in `Close` | 2/2 | none | `Host_DisposeAndUseAfterDispose_FollowDeterministicContract` (`:197`) |
| B17 | `:249` | `if (OpenState)` in `Close` | 2/2 | none | `Host_CloseFalseTrueReasonsAndRepeatedClose_HaveExactCallbacks` (`:169`) |
| B18 | `:260` | `if (string.IsNullOrWhiteSpace(theme)) throw` | 2/2 | none | `SetTheme_BlankTheme_RejectsExplicitly` (`:129`) |
| B19 | `:277` | `if (_disposed) return;` in `Dispose` | 2/2 | none | `ResetAndDispose_HandleOpenOrPartialStateAndRejectLaterUse` (`:250`) |
| B20 | `:289` | `PopupMessengerReady?.Invoke` null-conditional | 2/2 | none | `BreadcrumbDropDownLifecycleConcurrencyTests` harness subscribes/does not |
| B21 | `:298` | `if (OpenState)` inside `ResetCoreAsync` lambda | 2/2 | none | `Reset_DisposesAnOrphanedPartialSurface` (`:280`) + `ResetAndDispose_*` |
| B22 | `:314` | async `finally` dispatch of `ResetCoreAsync` (compiler-generated) | **3/4** | **exception-propagating-through-finally outcome uncovered** | — |
| B23 | `:320` | `if (OpenState && !_resetPending)` in `DisposeCoreAsync` | 4/4 | none | `Dispose_DuringPendingInitialization*` + `ResetAndDispose_*` |
| B24 | `:328` | `if (owned.Item1 != null)` (remove host from `DropDown.Items`) | 2/2 | none | `Host_DisposeAndUseAfterDispose_*` |
| B25 | `:331` | `owned.Item1?.Dispose()` null-conditional | 2/2 | none | same |
| B26 | `:334` | `owned.Item2 != null && !owned.Item2.IsDisposed` | **3/4** | **`!IsDisposed == true` outcome uncovered; `:335` `Dispose()` never executes** | — |
| B27 | `:337` | `(owned.Item3 as IDisposable)?.Dispose()` | 2/2 | none | `Host_DisposeAndUseAfterDispose_*` |
| B28 | `:369` | `expected != null && (!ReferenceEquals(host) \|\| !ReferenceEquals(control) \|\| !ReferenceEquals(messenger))` | **5/8** | **all three mismatch outcomes uncovered; `:377` early-return never executes** | — |
| B29 | `:387` | `if (!OpenState && !_openLifetime.IsPendingClose) return;` | 4/4 | none | `BreadcrumbPendingOpenCloseTests`, `NativeClosedEvent_*` (`:220`) |
| B30 | `:394` | `if (closeNative && wasOpen)` | 2/2 | none | `Host_CloseFalseTrueReasonsAndRepeatedClose_*` |
| B31 | `:416` | `if (_disposed \|\| _programmaticClose \|\| !OpenState) return;` in `OnDropDownClosed` | **4/6** | **`_disposed == true` uncovered; `_programmaticClose == true` uncovered** | `NativeClosedEvent_CancelsOnceAndIgnoresLaterCloseNotifications` covers `!OpenState` only |
| B32 | `:420` | same three-term guard re-evaluated inside the scheduled lambda | **4/6** | **`_disposed == true` uncovered; `_programmaticClose == true` uncovered** | same |
| B33 | `:432` | `if (reason == BreadcrumbDropDownCloseReason.Uncommitted)` in `FinishClose` | 2/2 | none | `ExplicitCommitAndUncommittedClose_HaveDistinctCallbacks` (`:64`) |
| B34 | `:441` | `OpenState \|\| DropDown.Visible` in `RestoreAfterOpenFailure` | 2/2 | none | `OpenAsync_ShowFailure_ClosesUncommittedAndRetainsTheFailure` (`:191`) |
| B35 | `:456` | `foreach` loop-exit in `CompleteAll` | 2/2 | none | ubiquitous |
| B36 | `:464` | `if (failure == null)` (first-failure retention) | 2/2 | none | `OpenLifetime_RollbackReporterFailure_IsContainedAndPrimaryIsRetained` (`:82`) |
| B37 | `:470` | `if (failure != null) throw failure;` | 2/2 | none | same |
| B38 | `:476` | `if (_disposed) throw new ObjectDisposedException` | 2/2 | none | `Host_DisposeAndUseAfterDispose_*` |

**Uncovered lines:** `:335` (from B26), `:377` (from B28). No others.

### 2.1 Reachability triage of the six gaps

| Gap | Reachable? | How |
| --- | --- | --- |
| B22 `:314` | **Yes** | Make the `ResetCoreAsync` try-body fault: inject a `cancelSelection` that throws, open, then `Reset()`. `RunAsync(...)` faults, the exception propagates through both `finally` blocks. |
| B26 `:334`/`:335` | **Yes** | Install an *orphaned* popup control (`InstalledPopupControl` set, `InstalledControlHost` left null) and `Dispose()`. When `Item1` is non-null it disposes its hosted control first, which is exactly why this outcome has never fired. |
| B28 `:369`/`:377` | **Yes** | Call `internal DisposeSurfaceAfterFailureAsync(expected)` (`:352`) with a fabricated `InstalledSurface` tuple that does not match the host's current fields. Three sub-outcomes, each an independent atomic case. |
| B31 `:416` `_disposed` | **Yes** | `Dispose()`, then raise `OnDropDownClosed`. Existing helper `RaiseNativeClosed` (`BreadcrumbDropDownLifecycleCoverageTests.cs:267`). |
| B31 `:416` `_programmaticClose` | **Yes** | Inject a `closePopup` delegate that synchronously re-raises `OnDropDownClosed`. Inside `CloseNative` (`:401-412`) `_programmaticClose` is `true`. The 9-arg core constructor already accepts this delegate — no production change. |
| B32 `:420` `_programmaticClose` | **Yes, but only with a queued pump** | Raise `OnDropDownClosed` while open so the lambda is *queued* on `CapturingSynchronizationContext`; then invoke `Close()` with an injected `closePopup` whose body calls `context.DrainOne()`. The queued lambda then executes while `_programmaticClose` is `true`. |
| B32 `:420` `_disposed` | **No — structurally unreachable** | For the lambda body to run, `BreadcrumbDropDownOpenLifetime.IsLifecycleCurrent(lease, allowDisposed: false)` must return `true` (`BreadcrumbDropDownOpenLifetime.cs:435`). `Host.Dispose()` (`:280`) calls `DisposeAndSchedule`, which sets the lifetime's `_disposed` and advances the generation, so a previously scheduled lambda is skipped rather than run. `_disposed` therefore cannot be `true` at `:420`. Record as irreducible branch remainder. |

---

## 3. Concurrency and ordering invariants

### 3.1 State model

Two orthogonal state variables, both host-owned:

- `OpenState` (`:225`) — logical open flag. Set `true` only in
  `BreadcrumbDropDownOpenLifetime.ShowCurrentSurface` (`BreadcrumbDropDownOpenLifetime.cs:265`);
  set `false` in `CompleteClose` (`:390`), `DisposeCoreAsync` (`:322`), `OnDropDownClosed`'s
  scheduled lambda (`:422`), and `RestoreAfterOpenFailure` (`:442`).
- `_disposed` (`:34`) — terminal. Once `true`, `OpenAsync`/`SetTheme`/`Reset` throw
  `ObjectDisposedException`; `Close` returns `false` (`:247`); `Dispose` is a no-op (`:277`).

Surface state is a third axis: `HasInstalledSurface` (`:222`) is the conjunction of
`InstalledControlHost`, `_popupControl`, `_popupMessenger`.

Legal transitions:

| From | Trigger | To | Code |
| --- | --- | --- | --- |
| Closed, no surface | `OpenAsync` | Opening (lifetime lease held) | `:241` |
| Opening | surface created + placed + shown | Open | `OpenLifetime.cs:265` |
| Opening | placement/show/focus failure | Closed + `LastInitializationException` set | `OpenLifetime.cs:392-393` -> `RestoreAfterOpenFailure` `:439` |
| Open | `OpenAsync` again | Open (focus only, no re-create) | `:236-239` |
| Open | `Close(reason)` | Closed | `:251` |
| Opening (not yet Open) | `Close(reason)` | Closed, pending open cancelled | `:254` |
| Open | native `Closed` event | Closed, `Uncommitted` | `:418-424` |
| any | `Reset()` | Closed, surface disposed, host reusable | `:271` |
| any | `Dispose()` | Disposed (terminal) | `:280` |

Illegal / defended-against transitions:

- **Double open** — `:235` short-circuits to `_openLifetime.Schedule(_focusPending)` and returns
  `Task.FromResult(true)`. Additionally `OpenLifetime.OpenAsync` returns the *same* `_openTask` for
  a concurrent second caller (`OpenLifetime.cs:55-56`). Covered by
  `ConcurrentOpenAsync_PendingInitializationIsSharedAndOpensOnePopup`
  (`BreadcrumbDropDownLifecycleConcurrencyTests.cs:22`).
- **Close during open** — `Close` at `:254` routes to `TryCancelPendingOpen`. Covered by
  `BreadcrumbPendingOpenCloseTests.CloseWhileFactoryPending_*` (`:22`) and
  `CloseWhileReadinessPending_*` (`:55`).
- **Open during close** — the generation lease makes a late open completion non-current; the result
  is downgraded to `false` at `OpenLifetime.cs:164`/`:181`.
- **Dispose during open** — `Dispose_DuringPendingInitializationDisposesLateSuccessWithoutMutation`
  (`BreadcrumbDropDownLifecycleConcurrencyTests.cs:103`) and
  `...IgnoresLateFailureWithoutMutation` (`:134`).
- **Re-entrant native close** — `_programmaticClose` (`:403`, `:410`) suppresses the `Closed` event
  raised by the host's own `_closePopup` call. **This guard is measured as never exercised**
  (B31/B32). It is the single most important untested invariant in the file.

### 3.2 Concurrency primitives with file:line

| Primitive | Line | Notes |
| --- | --- | --- |
| `Task<bool>` return of `OpenAsync` | `:228` | may be a shared task across callers |
| `Task.FromResult(true)` already-open fast path | `:238` | synchronous completion |
| `async Task ResetCoreAsync` | `:291` | two nested `finally` blocks |
| `Task DisposeCoreAsync()` | `:317` | expression-bodied, returns the dispatch task |
| `async Task DisposeSurfaceAsync` | `:342` | two sequential `ConfigureAwait(false)` awaits |
| `async Task DisposeSurfaceAfterFailureAsync` | `:352` | `reportFailure: false` |
| `.ConfigureAwait(false)` | `:301`, `:307`, `:346`, `:349`, `:356`, `:364` | six sites |
| Non-awaited scheduling (`_openLifetime.*Schedule`) | `:237`, `:251`, `:254`, `:271`, `:280` | fire-and-forget into the lifetime's observed-scheduling path |
| `GC.SuppressFinalize` | `:281` | |

**No `lock`, no `Interlocked`, no `volatile`, no `SemaphoreSlim`, no `CancellationToken`, no
`async void`, no thread creation in this file.** All mutual exclusion is delegated to
`BreadcrumbDropDownOpenLifetime` (which owns the `lock (_sync)`, see artifact 02) and all thread
affinity is delegated to `BreadcrumbPopupUiOperations` -> `BreadcrumbUiDispatcher`.

### 3.3 Thread-affinity assumption

Every mutation of the WinForms `ToolStripDropDown` and the hosted `Control` is funnelled through
`_uiOperations.RunAsync(...)` (`:295`, `:318`, `:345`, `:355`) or executed inside a lifetime-scheduled
operation, which itself dispatches via `BreadcrumbUiDispatcher.DispatchValue`
(`BreadcrumbUiDispatcher.cs:157`). The dispatcher's `IsCurrentBoundary` (`:255-278`) deliberately
refuses to treat bare owner-thread identity as proof of boundary when a context was captured — the
comment at `:263-268` documents the recycled-thread-pool-thread hazard. Tests must therefore drive
the boundary through a `SynchronizationContext`, not a thread.

Two exceptions execute on the caller's thread without dispatch: `CompleteClose` at `:251` is
*scheduled*, but `OnDropDownClosed` (`:414`) runs its guard synchronously on whatever thread raised
the native event before scheduling the body. That is correct (it is a read-only guard) but it means
the guard at `:416` and the guard at `:420` can observe different `_programmaticClose` values —
which is exactly the reachability distinction in §2.1.

---

## 4. Time dependence

**There is no wall-clock read, no timer, no delay, and no timeout anywhere in this file.** Confirmed
by inspection of all 480 lines: no `DateTime`, `Stopwatch`, `Timer`, `Task.Delay`, `Thread.Sleep`,
`CancellationTokenSource(TimeSpan)`, or `TimeProvider` usage.

Consequently **no clock seam is needed and none should be added.** The determinism requirement in
`.claude/rules/general-unit-test.md` § "Determinism Infrastructure" is satisfied here by *scheduler*
control, not clock control.

What a deterministic test needs instead:

1. **A manual pump.** `BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext`
   (`QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:346`) is the established
   vehicle: `Post` enqueues (`:367`), `DrainOne` (`:404`) executes exactly one callback on the
   creator thread and throws if called from another thread (`:406`), `DrainAll` (`:399`),
   `DrainUntil(Task)` (`:376`).
2. **Completion sources for the surface factory.** `SurfaceAttempt`
   (`BreadcrumbDropDownLifecycleConcurrencyTests.cs:331`) holds a
   `TaskCompletionSource<Tuple<Control, IWebViewMessenger>>` so the test decides when initialization
   completes.
3. **An inline context** where synchronous re-entrancy is the point:
   `InlineSynchronizationContext` (`BreadcrumbDropDownLifecycleConcurrencyTests.cs:401`).

**Caveat for the plan:** `CapturingSynchronizationContext.DrainUntil` calls
`WaitHandle.WaitAny(...)` with **no timeout** (`:382-388`). This is not a wall-clock *wait* in the
banned sense (it blocks on completion handles, not elapsed time), but a production regression that
deadlocks the state machine will hang the test host rather than fail. New tests should prefer
`DrainAll()` + explicit assertions on `Task.Status` over `DrainUntil` where the expected outcome is
"never completes."

---

## 5. Error paths

| # | Line | Construct | Kind | Reachable today? |
| --- | --- | --- | --- | --- |
| E1 | `:49` | `initializer ?? throw ArgumentNullException` | guard | Yes — `ProductionConstructor_RejectsMissingInitializerOrHtml` |
| E2 | `:50` | `html ?? throw` | guard | Yes — same |
| E3 | `:92` | `surfaceFactory ?? throw` | guard | Yes |
| E4 | `:155-164` | nine `?? throw ArgumentNullException` in the core ctor | guard | Yes — `Host_CoreConstructorNullDependencies_*` |
| E5 | `:234` | `ThrowIfDisposed()` in `OpenAsync` | guard | Yes |
| E6 | `:247` | `if (_disposed) return false;` early return | guard | Yes |
| E7 | `:261` | `throw new ArgumentException("A non-empty theme is required.", nameof(theme))` | guard | Yes |
| E8 | `:262` | `ThrowIfDisposed()` in `SetTheme` | guard | Yes |
| E9 | `:269` | `ThrowIfDisposed()` in `Reset` | guard | Yes |
| E10 | `:277` | `if (_disposed) return;` in `Dispose` | idempotence guard | Yes |
| E11 | `:293-314` | `try/finally` + nested `try/finally` in `ResetCoreAsync` | cleanup ordering | **Partly** — the exception path through both `finally`s is uncovered (B22) |
| E12 | `:369-377` | expected-surface mismatch early return | ownership guard | **No test today**; reachable via `DisposeSurfaceAfterFailureAsync` (B28) |
| E13 | `:387-388` | `CompleteClose` no-op guard | guard | Yes |
| E14 | `:404-411` | `try/finally` around `_closePopup` restoring `_programmaticClose` | re-entrancy guard | **The `finally` runs, but the guard it protects is never observed true** (B31/B32) |
| E15 | `:416` / `:420` | disposed / programmatic / not-open guards | guard | **Partly** (B31/B32) |
| E16 | `:453-472` | `CompleteAll` — runs all operations, retains the first exception, reports subsequent ones through `_uiOperations.Report`, rethrows the first | aggregation | Yes — `OpenLifetime_RollbackReporterFailure_IsContainedAndPrimaryIsRetained` |
| E17 | `:476-477` | `throw new ObjectDisposedException(nameof(BreadcrumbDropDownHost))` | guard | Yes |

**No exception is logged-and-swallowed in this file.** `CompleteAll` (`:462-468`) reports secondary
failures to `_uiOperations.Report` and rethrows the primary — an explicit, tested contract. Every
error path is reachable from a unit test with the seams that already exist. **No new seam is
required for any error path in this file.**

---

## 6. Coupling to sibling-owned files

Every type referenced by `BreadcrumbDropDownHost.cs`:

| Referenced type | Line(s) | Owner | Editable by F13? |
| --- | --- | --- | --- |
| `IBreadcrumbDropDownHost` | `:22` | **F13** (`Viewers/IBreadcrumbDropDownHost.cs`) | yes |
| `BreadcrumbDropDownCloseReason` | `:245`, `:299`, `:321`, `:385`, `:423`, `:427`, `:449` | **F13** (same file) | yes |
| `BreadcrumbPopupUiOperations` | `:25`, `:54`, `:65`, `:74`, `:98`, `:118`, `:129`, `:151` | **F13** | yes |
| `BreadcrumbDropDownOpenLifetime` | `:26`, `:172` | **F13** | yes |
| `BreadcrumbWebViewSurfaceFactory.Create` | `:70` | **F13** | yes |
| `IWebViewMessenger` | `:10`, `:15`, `:188`, `:214`, `:216` | **F13** | yes |
| `IWebViewCoreInitializer` | `:40`, `:60` | **F13** | yes |
| `CoreWebView2Environment` | `:6`, `:11`, `:16`, `:39`, `:179` | Microsoft (external) | n/a |

**There is no reference from this file to any F12-owned type** (`BreadcrumbBridgeRouter`,
`BreadcrumbBridgeCoordinator`, `BreadcrumbCoordinatorUpgradeLifetime`,
`BreadcrumbItemViewerLifecycleCoordinator`, `BreadcrumbMessengerHub`) **and none to F14's
`ItemViewer.Breadcrumb.cs`.** The dependency runs the other way:

- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:159` (F14) constructs `BreadcrumbDropDownHost` via
  the 8-arg internal constructor (`:57`), passing `lifecycle.Operations`.
- `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:53` (F12) exposes
  `IBreadcrumbDropDownHost? DropDownHost => _openCoordinator?.Host;` and `:141` subscribes to
  `PopupMessengerReady`.

**Conclusion: F13's tests for this file are not blocked on any sibling seam.** The one constraint is
*outbound*: F13 must not change the signature of the 8-arg internal constructor (`:57-76`) or the
`PopupMessengerReady` event (`:200`), because F14 and F12 respectively bind to them. Both are
already exercised by tests, so a breaking change would surface immediately.

---

## 7. Existing test inventory

| Test file | Lines | Targets | What it asserts about this file |
| --- | --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs` | **499** | Host (via reflection) | ctor ownership/no-Form field; placement arithmetic; commit-vs-uncommitted callbacks; focus transfer; `SetTheme` valid/blank; already-open path; zero working area; show-failure rollback; native-closed idempotence; reset+dispose+use-after-dispose; orphaned partial surface on `Reset`; production ctor null guards |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs` | **469** | Host + OpenLifetime | four forwarding ctors; installed-messenger reuse; close false/true/repeat callback counts; `SetTheme` contract; dispose + use-after-dispose; native-closed callback; nine core-ctor `ParamName`s |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleConcurrencyTests.cs` | **406** | Host | concurrent open sharing; reset-during-pending-init; dispose-during-pending-init (success and failure); stale-failure isolation; factory-failure observability |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleTests.cs` | **277** | Host | lazy open + surface reuse; reset then fresh init; partial-init failure rollback; dispose closes uncommitted; failed factory task |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` | **477** | Host + OpenLifetime | rollback-callback failure; ready-handler reset; show-callback reset; focus-callback failure; show-reset-then-throw; reset while readiness pending; legacy factory returns null |
| `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs` | **380** | Host | close while factory pending; close while readiness pending; canceled factory then reopen; toggle/escape while pending; automatic selector close while pending |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | **500** | Host + siblings | end-to-end |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs` | **498** | Host + operations | readiness sequencing |

**500-line headroom for test files is effectively zero.** `BreadcrumbDropDownIntegrationTests.cs`
is at 500, `BreadcrumbDropDownHostTests.cs` at 499, `BreadcrumbDropDownReadinessTests.cs` at 498,
`BreadcrumbDropDownCoverageThresholdTests.cs` at 477, `BreadcrumbDropDownLifecycleCoverageTests.cs`
at 469. **Every new test case for this file requires a new test file.** Precedent for the split
exists: `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` (`:12-16` documents the reason).

Recommended new file: `QuickFiler.Test/Viewers/BreadcrumbDropDownHostReentrancyTests.cs`. It should
carry its own harness (mirroring `LifecycleHarness` at
`BreadcrumbDropDownLifecycleCoverageTests.cs:290`) rather than making the existing 469-line file
`partial`, because the existing harness is a private nested class in a `sealed` non-partial class.

**`QuickFiler.Test` is a non-SDK project with explicit `<Compile Include>` entries and no globbing** —
verified at `QuickFiler.Test/QuickFiler.Test.csproj:81-82`. Any new test file needs its own entry
there. Preserve CRLF; use the Edit tool, not `sed -i`.

---

## 8. Recommended test-case list

All MSTest + Moq + FluentAssertions, Arrange–Act–Assert, deterministic, no temp files, no live forms
shown, no popups. WinForms `Panel`/`ToolStripDropDown` construction is in-memory only, matching the
established precedent in every existing file above (epic Shared Design §3).

Target file for all of these: **`QuickFiler.Test/Viewers/BreadcrumbDropDownHostReentrancyTests.cs`**
(new; ~230 lines projected including a local harness).

| # | Test name | Closes | Mechanism |
| --- | --- | --- | --- |
| T1 | `OnDropDownClosed_AfterDispose_ReturnsWithoutCancellingOrFocusing` | B31 `_disposed` | Open, `Dispose()`, drain, then reflect-invoke `OnDropDownClosed`. Assert `CancelCount`/`FocusAnchorCount` unchanged and no new dispatch posted. |
| T2 | `CloseNative_ReentrantClosedEvent_IsSuppressedByProgrammaticCloseGuard` | B31 `_programmaticClose` | Construct via the 9-arg internal ctor with a `closePopup` that reflect-invokes `OnDropDownClosed` on the host. Assert exactly one `cancelSelection` and one `focusAnchor` (the re-entrant notification adds none). |
| T3 | `QueuedClosedCallback_DrainedDuringProgrammaticClose_PerformsNoSecondClose` | B32 `_programmaticClose` | Open with `CapturingSynchronizationContext`; raise `OnDropDownClosed` (queues the body); then `Close(ExplicitCommit)` with a `closePopup` whose body calls `context.DrainOne()`. Assert `IsOpen == false`, one `cancelSelection`, one `focusAnchor`. |
| T4 | `Dispose_OrphanedPopupControlWithoutControlHost_DisposesTheControlExactlyOnce` | B26 `:334`/`:335` | Set `InstalledPopupControl` to a fresh undisposed `Panel` while leaving `InstalledControlHost` null; `Dispose()`. Assert the control's `Disposed` fired once. |
| T5 | `DisposeSurfaceAfterFailure_ControlHostMismatch_RetainsInstalledSurface` | B28 outcome 1 | Install a real surface; call `DisposeSurfaceAfterFailureAsync` with a tuple whose `Item1` differs. Assert `InstalledControlHost` etc. unchanged and nothing disposed. |
| T6 | `DisposeSurfaceAfterFailure_PopupControlMismatch_RetainsInstalledSurface` | B28 outcome 2 | Same with `Item2` differing. |
| T7 | `DisposeSurfaceAfterFailure_MessengerMismatch_RetainsInstalledSurface` | B28 outcome 3 + `:377` | Same with `Item3` differing. |
| T8 | `Reset_WhenCancelSelectionThrows_StillClearsSurfaceAndResetPending` | B22 `:314` | Inject a throwing `cancelSelection`; open; `Reset()`; drain. Assert the surface is disposed, `LastInitializationException` is null, and the failure reached the error sink exactly once. |

T1–T8 are eight independent atomic plan tasks. T5–T7 share one arrange block and could be one
`[DataTestMethod]`, but the epic mandates one atomic task per test case, so keep them separate.

### 8.1 Explicit non-goals (record, do not test)

| Item | Reason |
| --- | --- |
| B32 `:420` `_disposed == true` | Structurally unreachable — see §2.1. `Host.Dispose()` invalidates the lifetime lease before the queued lambda can run, so the lambda is skipped rather than executed with `_disposed == true`. Record on the irreducible-branch-remainder ledger. |

After T1–T8, the projected file state is **100% line** (both `:335` and `:377` covered) and
**~97.9% branch** (one unreachable outcome of 94 remaining at `:420`).

---

## 9. 500-line compliance

- **Current: 480 lines. Headroom: 20 lines.**
- **No production change is required for any recommended test case.** Every seam T1–T8 needs already
  exists: the 9-arg internal core constructor (`:143`), the injectable `closePopup` (`:31`), the
  internal `InstalledPopupControl`/`InstalledControlHost`/`InstalledPopupMessenger` setters
  (`:204-220`), the internal `DisposeSurfaceAfterFailureAsync` (`:352`), the private
  `OnDropDownClosed` reachable by reflection with an existing helper.
- **Therefore no partial split is needed and none should be proposed.** Adding a seam would consume
  the 20-line headroom for no coverage benefit and would create a `QuickFiler.csproj` edit that
  conflicts with 13 concurrent siblings.
- If a future change does force a split, the natural cut is the disposal/cleanup cluster
  (`ResetCoreAsync`, `DisposeCoreAsync`, `DisposeSurfaceAsync`, `DisposeSurfaceAfterFailureAsync`,
  `TakeOwnedSurface` — `:291-383`, 93 lines) into `BreadcrumbDropDownHost.Disposal.cs`, which would
  leave the primary at ~387 lines. That would require a `<Compile Include="Viewers\BreadcrumbDropDownHost.Disposal.cs" />`
  entry in `QuickFiler/QuickFiler.csproj` adjacent to line 403 (**preserve CRLF**; use the Edit tool,
  not `sed`) plus an F1 ledger row classified `testable` at >= 90%.

---

## 10. Latent defects

**D1 — `Close()` returns `true` for a pending-open cancellation that has not yet closed anything
(`:254`).** `TryCancelPendingOpen` returns `true` as soon as it has *claimed* the pending-close slot
and scheduled the close operation, before that operation runs. A caller that treats the `true` as
"the popup is now closed" is wrong; `BreadcrumbDropDownOpenCoordinator.CloseCore` (`:250, :257`) does
exactly that and advances its `_generation` on the strength of it. Impact: a coordinator generation
can advance before the host has actually finished closing. No observed failure, but the contract
documented on `IBreadcrumbDropDownHost.Close` (`IBreadcrumbDropDownHost.cs:34`, "Closes with
explicit-commit or rollback semantics") does not state that the return value is a *claim* rather
than a *completion*. Recommend an XML-doc clarification, not a behavior change. Report-only.

**D2 — `RestoreAfterOpenFailure` reads `DropDown.Visible` (`:441`) off the dispatch path's
guarantee.** `RestoreAfterOpenFailure` is invoked from
`BreadcrumbDropDownOpenLifetime.HandleOpenFailureAsync` inside `_uiOperations.RunAsync(...)`
(`BreadcrumbDropDownOpenLifetime.cs:386-397`), so it is on the boundary — correct today. But it is
`internal` and unguarded, so any future caller outside a dispatched action would touch a WinForms
property off-thread. Recommend a comment or an assertion. Report-only, low severity.

**D3 — `_resetPending` (`:32`) is never reset if `ResetCoreAsync` never runs.** `Reset()` sets
`_resetPending = true` at `:270` *before* scheduling `ResetCoreAsync` at `:271`. If the schedule is
dropped because the lifecycle is no longer current (`BreadcrumbDropDownOpenLifetime.cs:435` returns
`Task.CompletedTask`), `_resetPending` stays `true` permanently. The only consumer is
`DisposeCoreAsync`'s `if (OpenState && !_resetPending)` at `:320`, so a stuck `_resetPending` would
suppress the uncommitted close on dispose. Reachable in principle by `Reset()` immediately followed
by `Dispose()`; in practice `Dispose()` also clears `OpenState` at `:322`, so the effect is limited
to skipping `_cancelSelection`. **This is the most substantive of the three.** Impact: a
selection could be left pending after a `Reset()`-then-`Dispose()` sequence. Recommend promotion to
a GitHub issue.

None of D1–D3 is in scope for F13 under the epic's no-behavior-change NFR. Per the epic's "Latent
Defect Promotion" section, promote via the MCP promotion lifecycle rather than leaving them as
prose here.
