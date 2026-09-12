# Per-File Research — `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`

- Epic: #136 `quickfiler-per-file-coverage`, child F13, feature issue #455
- Production file: `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` (285 lines, 215 lines of headroom)
- csproj entry: `QuickFiler/QuickFiler.csproj:396`
- Research date: 2026-08-07
- Builds on: `research/00-cross-cutting-context.md`

---

## 0. Headline and acceptance bar

**This file already passes both gates and its single residual branch outcome is structurally
unreachable. The correct recommendation is: no new test needed; retain-and-verify only.**

Recomputed from `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
(class element at XML line 8874; class-level `<lines>` block 9122-9380; denominator from `<line>`
child count per epic Directive B, because the `<class>` attributes are inflated by open issue #441):

| Metric | Value | Floor | Margin |
| --- | --- | --- | --- |
| Line | **187/187 = 100.00%** | 80% | +20.00 |
| Branch | **35/36 = 97.22%** | 75% | +22.22 |

The `<class>` attribute reads `branch-rate="0.969697"` (= 64/66). My recomputation over the 18 branch
lines in the class-level `<lines>` block gives 35/36 = 97.22%, matching the delegating brief's ~97.2%
figure. The `<class>` denominator of 66 does not correspond to any count derivable from the element's
own children — a concrete illustration of why #441 makes those attributes untrustworthy.

**Zero uncovered lines. Exactly one uncovered branch outcome, at `:276`, and §2.3 proves no
constructor path can reach it.**

---

## 1. Structural map

Single type: `internal sealed class BreadcrumbUiDispatcher`, lines 12-284. Reachable from tests via
`QuickFiler/Properties/AssemblyInfo.cs:5`.

### 1.1 Fields

| Line | Field | Role |
| --- | --- | --- |
| 14-15 | `[ThreadStatic] private static BreadcrumbUiDispatcher? _executingDispatcher` | per-thread marker naming the dispatcher whose synchronous callback is currently executing. `[ThreadStatic]` is per-thread and therefore safe under `Scope=ClassLevel` MSTest parallelism (`scripts/vscode/TaskMaster.cli.runsettings`). |
| 17-19 | `static readonly log4net.ILog log` | fallback sink when the injected error sink itself throws |
| 21 | `readonly SynchronizationContext? _context` | the captured marshalling boundary; **null only for the owner-thread-only test dispatcher** |
| 22 | `readonly Action<Exception> _errorSink` | the injected observable error sink |
| 23 | `readonly int? _ownerThreadId` | owner thread identity; **null only via the 2-arg internal constructor** |

### 1.2 Members with line ranges

| Lines | Member | Visibility |
| --- | --- | --- |
| 25-30 | `.ctor(SynchronizationContext, Action<Exception>)` | internal; null-guards `context` at `:27`, forwards `ownerThreadId: null` |
| 32-41 | `.ctor(SynchronizationContext?, Action<Exception>, int?)` | **private** — the only ctor that can produce `_ownerThreadId == null`; null-guards `errorSink` at `:39` |
| 44-56 | `static CaptureCurrent()` | internal; fails fast at `:46-50` when `SynchronizationContext.Current` is null; otherwise captures context **and** `Environment.CurrentManagedThreadId` |
| 62-65 | `static CreateForCurrentThreadTests()` | internal; `new BreadcrumbUiDispatcher(null, LogFailure, Environment.CurrentManagedThreadId)` |
| 71-151 | `Dispatch(Action)` | internal, returns `Task` |
| 157-235 | `DispatchValue<T>(Func<T>, bool reportFailure = true)` | internal, returns `Task<T>` |
| 238-253 | `Report(Exception)` | internal |
| 255-278 | `IsCurrentBoundary()` | private |
| 280-283 | `static LogFailure(Exception)` | private |

### 1.3 Constructor dependencies and how they are injected

| Dependency | Injection form | Seam quality |
| --- | --- | --- |
| `SynchronizationContext` | **constructor parameter** (`:25`) — a framework abstract class, so a test subclass is a first-class fake | Strong. This is the primary seam. |
| `Action<Exception>` error sink | **injectable delegate** (`:25`) | Strong. Lets a test observe every reported failure with no logging capture. |
| owner thread id | derived internally from `Environment.CurrentManagedThreadId` at `:54` / `:64` | Not injected. Not a problem: tests control which thread constructs the dispatcher. |
| log4net `ILog` | hard-wired static at `:17-19` | Only used at `:251` when the *sink itself* throws; that path is covered without asserting on log output. |

**No wall-clock read, no timer, no `TimeProvider`, no `CancellationToken`.** Nothing in this file
needs a clock seam and none exists.

---

## 2. Thread-affinity contract (the load-bearing per-file analysis)

This type is the UI-thread marshalling seam that the whole of F13 depends on. Its contract is
unusually precise and worth stating exactly, because several sibling files' correctness rests on it.

### 2.1 What it does and does not use

| Mechanism | Used? | Evidence |
| --- | --- | --- |
| `SynchronizationContext.Post` | **Yes** — the only marshalling primitive | `:122`, `:206` |
| `SynchronizationContext.Send` | No | grep: zero occurrences |
| `SynchronizationContext.Current` | Read at `:47` (capture) and `:271` (boundary proof) only | |
| `Control.InvokeRequired` / `Control.Invoke` / `Control.BeginInvoke` | **No** | grep: zero occurrences. The type never references `System.Windows.Forms`; its `using` set (`:2-4`) is `System`, `System.Threading`, `System.Threading.Tasks`. |
| `Dispatcher` (WPF) | No | |
| Blocking waits | **No** | Never blocks. `Dispatch` returns `Task.CompletedTask` on the inline path and a TCS-backed task otherwise. |

**Consequence for the epic's WebView2/Office.js migration non-goal (`epic.md:198-200`): this file is
already host-neutral.** It has no WinForms dependency at all and would port unchanged. That is worth
recording in the ledger as a positive.

### 2.2 Behaviour matrix

`IsCurrentBoundary()` (`:255-278`) is the decision function for `Dispatch`. `DispatchValue` uses a
**deliberately stricter** rule. The difference is the most important thing in this file.

| Situation | `Dispatch(Action)` | `DispatchValue<T>(Func<T>)` |
| --- | --- | --- |
| Called from inside this dispatcher's own executing callback (`_executingDispatcher == this`) | **Inline** (`:78` → `:258`) | **Inline** (`:166`) |
| Ambient `SynchronizationContext.Current` is reference-equal to the captured `_context` | **Inline** (`:269-271`) | **Posts anyway** — ambient identity is *not* accepted |
| Owner-thread-only dispatcher (`_context == null`) and on the owner thread | **Inline** (`:276-277`) | **Posts?** No — falls to `:180`, `_context == null` → reports and returns a faulted task |
| Any context, not on the boundary | `_context.Post` (`:122`) | `_context.Post` (`:206`) |
| `_context == null` (owner-thread-only test dispatcher) and off the owner thread | Reports `InvalidOperationException("...cannot marshal cross-thread UI work.")`, returns `Task.CompletedTask` — i.e. **swallows** (`:97-105`) | Reports **and** returns `Task.FromException<T>` — i.e. **propagates** (`:180-188`) |

The rationale for `DispatchValue`'s stricter rule is documented in-source at `:164-165` and again at
`:263-268`: a continuation resumed after `ConfigureAwait(false)` can land on a recycled thread-pool
thread whose managed thread id equals the captured owner thread id, which would run UI work inline
and complete the returned task without any post ever crossing the captured context. **Bare
owner-thread identity is explicitly rejected as a boundary proof when a context was captured.** This
is pinned by an existing test —
`BreadcrumbUiThreadDispatchTests.cs:187-215` `DispatchValue_AmbientOwningContext_StillSchedulesBeforeControlAccess`
asserts `dispatch.IsCompleted.Should().BeFalse("ambient context alone is not an inline proof")`.

### 2.3 Disposed / not-handle-created / null target

The brief asks about behaviour when "the target is disposed, not yet handle-created, or null". **Those
concepts do not exist in this type.** It has no target control, no handle, and no `Control` reference
of any kind. The three failure modes that do exist are:

| Failure mode | file:line | Behaviour |
| --- | --- | --- |
| Null `context` supplied to the public path | `:27` | `ArgumentNullException` at construction — fail fast |
| Null `errorSink` | `:39` | `ArgumentNullException` at construction |
| No ambient context at capture time | `:46-50` | `InvalidOperationException("Breadcrumb UI components must be constructed on an owning UI synchronization context.")` — fail fast |
| Post rejected by the context (a disposed WinForms context throws from `Post`) | `:144-148`, `:228-232` | caught, reported once via `ReportOnce`, and the returned task is completed (`Dispatch`) or faulted (`DispatchValue`) |

The disposed-control case is therefore modelled as "the `SynchronizationContext.Post` throws", and
that path **is already tested** — `ThrowingSynchronizationContext`
(`BreadcrumbUiThreadDispatchTests.cs:426-442`) drives `:144-148` at `:108-157` and `:228-232` at
`:255-274`.

---

## 3. Branch inventory

### 3.1 Complete conditional inventory

| file:line | Construct | `condition-coverage` | Status |
| --- | --- | --- | --- |
| `:26` | `context ?? throw` in the ctor initializer | `100% (2/2)` | covered |
| `:39` | `errorSink ?? throw` | `100% (2/2)` | covered |
| `:46` | `SynchronizationContext.Current ?? throw` | `100% (2/2)` | covered |
| `:73` | `if (action == null)` | `100% (2/2)` | covered |
| `:78` | `if (IsCurrentBoundary())` | `100% (2/2)` | covered |
| `:97` | `if (_context == null)` (owner-thread-only, off-thread) | `100% (2/2)` | covered |
| `:114` | `if (Interlocked.Exchange(ref failureReported, 1) == 0)` in `Dispatch.ReportOnce` | `100% (2/2)` | covered |
| `:159` | `if (action == null)` in `DispatchValue` | `100% (2/2)` | covered |
| `:166` | `if (ReferenceEquals(_executingDispatcher, this))` | `100% (2/2)` | covered |
| `:174` | `if (reportFailure)` (inline failure) | `100% (2/2)` | covered |
| `:180` | `if (_context == null)` | `100% (2/2)` | covered |
| `:185` | `if (reportFailure)` (owner-thread-only failure) | `100% (2/2)` | covered |
| `:197` | `if (Interlocked.Exchange(ref failureReported, 1) == 0)` in `DispatchValue.ReportOnce` | `100% (2/2)` | covered |
| `:199` | `if (reportFailure)` | `100% (2/2)` | covered |
| `:240` | `if (exception == null)` in `Report` | `100% (2/2)` | covered |
| `:258` | `if (ReferenceEquals(_executingDispatcher, this))` in `IsCurrentBoundary` | `100% (2/2)` | covered |
| `:269` | `if (_context != null)` | `100% (2/2)` | covered |
| **`:276`** | `return _ownerThreadId.HasValue && Environment.CurrentManagedThreadId == _ownerThreadId.Value;` | **`50% (1/2)`** | **UNCOVERED (1)** |

Also present but not Cobertura branches: `catch (Exception)` at `:86`, `:131`, `:144`, `:172`, `:215`,
`:228`, `:249`; `finally` at `:90`, `:135`, `:220`. All are covered (every line in the file is hit).

No `switch`, no ternary, no `?.`, no pattern match, no loop, no `catch` filter.

### 3.2 The single uncovered outcome — **structurally unreachable**

```
255  private bool IsCurrentBoundary()
256  {
258      if (ReferenceEquals(_executingDispatcher, this)) return true;
269      if (_context != null) return ReferenceEquals(SynchronizationContext.Current, _context);
276      return _ownerThreadId.HasValue
277          && Environment.CurrentManagedThreadId == _ownerThreadId.Value;
278  }
```

Line 277 has `hits="1"`, so the `_ownerThreadId.HasValue == true` outcome is covered. The uncovered
outcome is `_ownerThreadId.HasValue == false`, which requires `_context == null` **and**
`_ownerThreadId == null` simultaneously. Enumerating every construction path:

| Path | `_context` | `_ownerThreadId` | Reaches `:276`? |
| --- | --- | --- | --- |
| `new BreadcrumbUiDispatcher(context, errorSink)` (`:25-30`) | non-null (guarded at `:27`) | **null** | No — `:269` returns first |
| `CaptureCurrent()` (`:44-56`) | non-null (guarded at `:46`) | `Environment.CurrentManagedThreadId` | No — `:269` returns first |
| `CreateForCurrentThreadTests()` (`:62-65`) | **null** | `Environment.CurrentManagedThreadId` (never null) | Yes, but `HasValue` is true |
| private `.ctor(null, sink, null)` | null | null | **No call site exists** |

Grep across the repository for `new BreadcrumbUiDispatcher` returns 24 call sites (all in
`QuickFiler.Test/`) plus the two internal factories at `:51` and `:64`; the private 3-arg constructor
is invoked only from `:26` (which forces `_context` non-null) and `:51`/`:64` (which force
`_ownerThreadId` non-null). **There is no reachable `(null, null)` combination.**

**Verdict: `:276`'s false outcome is dead defensive code. No test, no seam, and no fixture can reach
it. This file's branch ceiling is 35/36 = 97.22%, not 100%.**

The instrumentation defect around `[ExcludeFromCodeCoverage]` and nested lambdas is **not applicable**:
this file carries no such attribute (grep: zero occurrences), and its four lambdas — the `Post`
callbacks at `:123-140` and `:207-224`, and the two local functions `ReportOnce` at `:112-118` and
`:195-202` — are all instrumented and all fully covered.

---

## 4. Concurrency, ordering, and time

| file:line | Primitive | Notes |
| --- | --- | --- |
| `:14-15` | `[ThreadStatic] static BreadcrumbUiDispatcher? _executingDispatcher` | The reentrancy marker. Saved/restored around every callback (`:80-81`/`:92`, `:125-126`/`:137`, `:209-210`/`:222`), always in a `finally`. Correct nesting is pinned by `BreadcrumbUiThreadDispatchTests.cs:218-252`. |
| `:107-109`, `:190-192` | `TaskCompletionSource<...>` with `RunContinuationsAsynchronously` | prevents continuations running inside the posted callback |
| `:110`, `:193` | `int failureReported` | plain int, mutated only through `Interlocked` |
| `:114`, `:197` | `Interlocked.Exchange(ref failureReported, 1)` | the report-exactly-once guarantee, asserted by `DispatcherActionFailure_IsReportedExactlyOnce` (`BreadcrumbUiThreadDispatchTests.cs:160-184`) |
| `:122`, `:206` | `_context.Post(...)` | the only outward marshalling call; both are wrapped in try/catch |
| `:170` | `Task.FromResult(action())` | inline synchronous completion |
| `:176`, `:187` | `Task.FromException<T>(...)` | |
| `:94`, `:104` | `Task.CompletedTask` | |

- **No `lock`, no `Volatile`, no `SemaphoreSlim`, no `Monitor`.** Mutual exclusion is achieved by
  thread affinity plus `Interlocked`.
- **No `async` / `await` anywhere in the file.** Every method is synchronous and returns an
  already-shaped `Task`. This is what makes the file trivially deterministic to test.
- **No `async void`.**
- **No timer, no wall-clock read, no timeout, no `Task.Delay`.**
- **No injected clock or `TimeProvider` seam exists, and none is required.**

**Deterministic mechanism for any untested path:** none is needed — every path except the unreachable
`:276` is already covered. For reference, the two proven in-repo deterministic fakes are:

1. `QueuedCreatorThreadSynchronizationContext` with `DrainOnCreatorThread()` —
   `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs:274-300`. A queued
   `SynchronizationContext` that records the creator thread id, enqueues every `Post`, and replays
   the queue on demand while asserting it is on the creator thread. No sleeps, no live form, no UI
   pump.
2. `RecordingSynchronizationContext` with `DrainUntilAsync(Task)` —
   `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:361-424`. Same idea with a
   `SemaphoreSlim` availability signal so an `async` test can drain until a specific operation
   completes. Also used with `ThrowingSynchronizationContext` (`:426-442`) for the post-rejection path.

Both satisfy the determinism rules in `.claude/rules/general-unit-test.md` (no `Thread.Sleep`, no
`Task.Delay`, no real wall-clock wait). **F13 should reuse these rather than author a third variant.**

---

## 5. Error paths

| file:line | Construct | Covered? | Seam needed? |
| --- | --- | --- | --- |
| `:27` | `context ?? throw new ArgumentNullException(nameof(context))` | Yes — `BreadcrumbPopupBoundaryCoverageTests.cs:22` | No |
| `:39` | `errorSink ?? throw new ArgumentNullException(nameof(errorSink))` | Yes — `BreadcrumbPopupBoundaryCoverageTests.cs:23` | No |
| `:46-50` | `throw new InvalidOperationException("...owning UI synchronization context.")` | Yes — `BreadcrumbUiThreadDispatchTests.cs:277-296` | No |
| `:73-76` | `throw new ArgumentNullException(nameof(action))` (`Dispatch`) | Yes | No |
| `:86-89` | `catch (Exception exception) { Report(exception); }` — inline action failure | Yes — `:160-184` | No |
| `:97-105` | owner-thread-only cross-thread guard: reports and **swallows**, returning `Task.CompletedTask` | Yes — `:298-307` (via `DispatchValue`); the `Dispatch` variant is reached from the same fixture | No |
| `:131-134` | `catch (Exception exception) { ReportOnce(exception); }` — posted action failure | Yes | No |
| `:144-148` | `catch (Exception exception) { ReportOnce(exception); completion.TrySetResult(null); }` — **post rejection** | Yes — `:108-157` with `ThrowingSynchronizationContext` | No |
| `:159-162` | `throw new ArgumentNullException(nameof(action))` (`DispatchValue`) | Yes — `:194`, `:209` | No |
| `:172-177` | `catch` on inline value action; conditional report; **faults the returned task** | Yes — `:218-252` | No |
| `:180-188` | owner-thread-only guard; reports and **faults** | Yes — `:298-307` | No |
| `:215-219` | `catch` on posted value action; reports once and faults | Yes | No |
| `:228-232` | `catch` on post rejection; reports once and faults | Yes — `:255-274` | No |
| `:240-243` | `throw new ArgumentNullException(nameof(exception))` in `Report` | Yes | No |
| `:249-252` | `catch (Exception sinkException) { log.Error("Breadcrumb UI error sink failed.", sinkException); }` — **the sink-of-last-resort** | Yes (line hits=1) | No |

**No bare `catch {}` in this file.** Every catch either reports through the injected sink, faults the
returned task, or (at `:249`) logs through the project log4net pattern. The
`BreadcrumbPopupUiOperations.cs:349` / `BreadcrumbDropDownOpenLifetime.cs:197` bare-catch finding does
not extend here.

Every error path is reachable from a unit test with **current** seams. No new interface, delegate, or
adapter is required.

---

## 6. Test-only affordances in production code — policy assessment

The brief asks whether `CreateForCurrentThreadTests()` is a policy concern. **Assessment: yes for the
naming, no for the mechanism, and the real concern is one level up in a sibling-adjacent file.**

### 6.1 The facts

- `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` (`:62-65`) is `internal static` and is called
  from **five test files only**:
  `FolderBreadcrumbAssetContractTests.cs:184`, `BreadcrumbUiThreadDispatchTests.cs:299`,
  `BreadcrumbSelectorCoordinatorTests.cs:396`, `BreadcrumbDuplicateIdentityIntegrationTests.cs:151`,
  `BreadcrumbDropDownReadinessTests.cs:314`. **No production call site.**
- However, `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:86-89` declares:
  ```csharp
  internal static BreadcrumbPopupUiOperations CaptureCurrentOrTests() =>
      SynchronizationContext.Current == null ? CreateForCurrentThreadTests() : CaptureCurrent();
  ```
  and `CreateForCurrentThreadTests()` there (`:83-84`) forwards to *our*
  `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()`.
- `CaptureCurrentOrTests()` **is** called from production: `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:98`
  and `:118`, and `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:156` and `:192` (F14-owned).

### 6.2 Assessment

The mechanism is defensible: an owner-thread-only dispatcher that *reports* rather than silently
mis-marshals is strictly safer than a `SynchronizationContext`-less dispatcher that runs UI work
inline. It is a genuine production fallback, not a test hook.

The **naming** is the defect. A production code path (`BreadcrumbDropDownHost.cs:98`) can select a
factory whose name asserts it is for tests. If `SynchronizationContext.Current` is ever null on a real
VSTO path, production silently downgrades to a dispatcher that cannot marshal cross-thread work and
reports the failure to a log instead of failing fast — the opposite of the
`CLAUDE.md` §3 "fail fast and explicitly" rule, and invisible in review because the name says "Tests".

This is **not F13's to fix under the epic's no-behaviour-change NFR**, and the ambiguity is in
`BreadcrumbPopupUiOperations.cs`, not in this file. Recorded as latent defect **D1** in §10 for MCP
promotion. Note also that `BreadcrumbDropDownOpenCoordinator`'s "no `[ExcludeFromCodeCoverage]`"
contract is pinned by an existing test
(`ItemViewerBreadcrumbDropDownContractTests.cs:102-130`), so any renaming work here must be checked
against that fixture before it is scheduled.

**Recommendation for F13: leave `CreateForCurrentThreadTests()` exactly as it is.** Renaming it to
something like `CreateOwnerThreadOnly()` would be a strictly-better name but touches six call sites
across three children (F13, F12's `BreadcrumbSelectorCoordinatorTests`, F14's
`ItemViewer.Breadcrumb.cs`) and buys zero coverage.

---

## 7. Coupling to sibling-owned files

| Direction | Their file:line | Coupling | Mockable through an existing interface? |
| --- | --- | --- | --- |
| they → us | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:484` — **F12** | `return BreadcrumbUiDispatcher.CaptureCurrent();` — F12's coordinator captures a dispatcher and takes one as a constructor parameter (see `BreadcrumbUiThreadDispatchTests.cs:319-323`, which constructs `new BreadcrumbBridgeCoordinator(messenger, provider.Object, dispatcher)`). | The coupling is on the **concrete** `BreadcrumbUiDispatcher` type, not an interface. Our tests do not need to mock it — we construct the real dispatcher with a fake `SynchronizationContext`, which is the better seam anyway. **But the ctor signature is frozen: F12 compiles against `BreadcrumbUiDispatcher(SynchronizationContext, Action<Exception>)`.** |
| they → us | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:56`, `:83` — **F14** | `BreadcrumbUiDispatcher.CaptureCurrent()` in two places. | Frozen static factory signature. |
| we → them | **none** | This file references no sibling-owned type. Its entire dependency set is BCL (`System`, `System.Threading`, `System.Threading.Tasks`) plus log4net. | — |

Neither `BreadcrumbPopupLifecycleOperations` (`BreadcrumbItemViewerLifecycleCoordinator.cs:355`) nor
`BreadcrumbNavigationSubscription` (`:337`) is referenced by this file, so F12's expected split of
that 481-line file cannot conflict with anything F13 does here.

Same-child (F13) consumers, for completeness: `BreadcrumbPopupUiOperations.cs:81,84,88`,
`WebView2Messenger.cs:144`, `BreadcrumbWebViewSurfaceFactory.cs:170` (indirectly, via
`BreadcrumbPopupUiOperations.CaptureCurrent()`).

**Net: zero outbound coupling, three inbound sibling references. Freeze every signature.**

---

## 8. Existing test inventory

| Test file | Lines | Headroom | What it asserts about this file |
| --- | --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs` | 480 | 20 | The de-facto owner fixture. Nine tests: `:23` provider completion schedules `Post` on the owning context; `:66` every inbound worker message posts and calls back on the owning context; `:108` scheduling failure reported through the observable sink; `:160` action failure reported **exactly once**; `:187` ambient owning context still schedules (the strict-`DispatchValue` contract); `:218` nested synchronous dispatch executes inline with **zero** extra posts; `:255` scheduling failure reports once **and** faults the returned task; `:277` production capture without a UI context fails fast, and the owner-thread-only dispatcher rejects cross-thread work; `:311` inbound dispatch failure is observed without escaping the event boundary. Helpers: `RecordingSynchronizationContext` (`:361-424`), `ThrowingSynchronizationContext` (`:426-442`), `TrackingMessenger` (`:444-478`). |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | 361 | 139 | `:22-24` constructor null-guards for both parameters; `:42`, `:69`, `:183` dispatcher-backed operations fixtures. |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | 486 | 14 | `:105`, `:129`, `:303`, `:311` dispatcher-backed control dispatch. |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` | 478 | 22 | `:97`, `:183`, `:255`. |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 461 | 39 | `:88`, `:128`, `:162`, `:258`. |
| `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs` | 302 | 198 | `:231` `new BreadcrumbUiDispatcher(Queue, _ => {})` with the queued fake context (`:274-300`). |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 327 | 173 | `:227`. F12-primary. |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs` | 498 | **2** | `:248`, `:314`, `:325`. |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` / `.Part2.cs` | 447 / 381 | 53 / 119 | `:331` / `:338`. |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs` | 469 | 31 | `:306`. |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` | 477 | 23 | `:315`. |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 480 | 20 | `:387`. |
| `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorProbabilityTests.cs` / `BreadcrumbSelectorCoordinatorTests.cs` / `BreadcrumbDuplicateIdentityIntegrationTests.cs` / `FolderBreadcrumbAssetContractTests.cs` | 168 / 434 / 218 / 405 | — | `CreateForCurrentThreadTests()` consumers. F12/asset-primary. |

There is no `BreadcrumbUiDispatcherTests.cs`; `BreadcrumbUiThreadDispatchTests.cs` is the owner
fixture and has **20 lines of headroom** — enough for perhaps one trivial test method, not enough for
any test with a non-trivial arrange block.

---

## 9. Recommended test-case list

**NO NEW TEST IS WARRANTED FOR THIS FILE.**

Stated plainly, because the brief asks for honesty here: the file is at 100% line and 97.22% branch;
the single residual branch outcome at `:276` is proven unreachable in §3.2 by exhaustive enumeration
of the three constructor paths; every guard, every catch, every inline/post decision, and the
report-exactly-once contract already have a named, passing test. Writing anything further would be a
shape-assertion test manufactured to move a number that cannot move, which `epic.md:521-522`
prohibits.

**The plan's work for this file is a single retain-and-verify task:**

| # | Task | Deliverable |
| --- | --- | --- |
| V1 | Re-measure per-file line and branch coverage on the F13 branch using the F1 harness (or the Directive-B fallback over `scripts/vscode/Invoke-MSTestWithCoverage.ps1` output), and confirm 187/187 line and 35/36 branch are retained. | A per-file row in `<FEATURE>/evidence/qa-gates/`. |
| V2 | Record in the epic coverage ledger that this file's branch ceiling is **35/36 = 97.22%**, with the `:276` unreachability proof, so the capstone (F16) does not flag it as an unmet 100%. | Ledger row rationale text. |

If, and only if, the planner insists on a net-new test for this file, the least-objectionable option
is a genuine behavioural gap rather than a coverage-manufacturing one:
`Report_SinkThrows_FallsBackToLogWithoutEscaping` in a new
`QuickFiler.Test/Viewers/BreadcrumbUiDispatcherReportFallbackTests.cs`, asserting that
`dispatcher.Report(ex)` with an error sink that itself throws does **not** propagate (`:249-252`).
Line `:251` already has `hits="1"`, so this adds **zero** coverage — its value is regression
protection on the sink-of-last-resort, nothing more. Recommend deferring it.

---

## 10. csproj impact

- **`QuickFiler/QuickFiler.csproj`: no change.** Existing entry at `:396`.
- **`QuickFiler.Test/QuickFiler.Test.csproj`: no change** under the recommended plan (no new test
  file). If the optional test in §9 is taken, one `<Compile Include>` line adjacent to `:66`
  (`Viewers\BreadcrumbUiThreadDispatchTests.cs`) inside the breadcrumb block at `:60-89`.
- **CRLF preservation applies to any edit.** Use `Edit` or `perl -0777` with explicit `\r\n`; never
  a git-bash `sed -i` (`epic.md:610-612`).
- **Coverage ledger:** update the existing `testable` row with the measured figures and the §3.2
  unreachability note. No new row.

---

## 11. Latent defects

| ID | file:line | Defect | Impact | Confidence |
| --- | --- | --- | --- | --- |
| **D1** | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:86-89` (selecting `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` at `:64`), consumed by production at `BreadcrumbDropDownHost.cs:98`, `:118` and `ItemViewer.Breadcrumb.cs:156`, `:192` | A production code path silently selects a dispatcher whose factory name asserts it is for tests. When `SynchronizationContext.Current` is null on a real VSTO path, `CaptureCurrentOrTests()` returns an owner-thread-only dispatcher; cross-thread UI work is then **reported to log4net and swallowed** (`BreadcrumbUiDispatcher.cs:97-105`) instead of failing fast. The name makes this invisible in review. Contrast with `CaptureCurrent()` (`:46-50`), which fails fast in exactly this situation. | Silent degradation of breadcrumb popup UI marshalling under an unexpected VSTO threading configuration, with no user-visible error. Contradicts `CLAUDE.md` §3. | Medium (behaviour verified from source; no runtime evidence that `SynchronizationContext.Current` is ever null on the production path) |
| **D2** | `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:276-277` | The `_ownerThreadId.HasValue` operand is permanently unreachable-false: no constructor path produces `_context == null && _ownerThreadId == null` (§3.2). | None at runtime — it is correct-but-dead defensive code. It caps the file's branch coverage at 97.22%, which the ledger must record so F16 does not treat it as a gap. | High (proved by enumerating all three constructor paths and all 24 `new BreadcrumbUiDispatcher` call sites) |

D2 is an observation for the ledger, not an issue worth promoting. **D1 is a genuine promotion
candidate** and is not among the defects already recorded by siblings
(`BreadcrumbDropDownOpenCoordinator.cs:95` lock ordering,
`BreadcrumbDropDownOpenLifetime.cs:229-230` null-forgiving deref, the two bare `catch {}` blocks, the
nested-lambda instrumentation defect). Do not fix it here; hand it to the orchestrator for MCP
promotion per `epic.md:538-546`.

---

## 12. Deviations from the delegation brief

| Brief statement | Finding |
| --- | --- |
| "`BreadcrumbUiDispatcher.cs` 100% line, ~97.2% branch" | **Confirmed exactly.** 187/187 line, 35/36 = 97.22% branch, recomputed by `<line>` child count and `condition-coverage` summation. |
| "whether it uses `InvokeRequired`/`BeginInvoke`/`SynchronizationContext`/`Control.Invoke`" | **`SynchronizationContext.Post` only.** Zero WinForms references in the file; `InvokeRequired`, `Invoke`, and `BeginInvoke` do not appear. |
| "behaviour when the target is disposed, not yet handle-created, or null" | **Refuted as framed.** The type has no target control and no handle. The equivalent failure mode is `SynchronizationContext.Post` throwing, which is caught at `:144-148` and `:228-232` and is already tested with `ThrowingSynchronizationContext`. |
| "assess whether test-only affordances in production code are a policy concern" | **Yes, but the concern is one file up.** `CreateForCurrentThreadTests()` itself has no production call site; `BreadcrumbPopupUiOperations.CaptureCurrentOrTests()` (`:86-89`) is what puts it on a production path. Recorded as D1. |
| "the proven test technique … queued fake `SynchronizationContext` with `DrainOnCreatorThread()`" | **Confirmed** at `BreadcrumbPopupUiOperationsDirectAdapterTests.cs:274-300`. A second, richer variant (`RecordingSynchronizationContext` with `DrainUntilAsync`) exists at `BreadcrumbUiThreadDispatchTests.cs:361-424`; prefer whichever matches the test's async shape. |
| Implicit premise that branch work is needed | **Refuted.** No new test is warranted; the plan item is retain-and-verify. |

---

*No commands were executed in this session; all findings are derived from the working-tree files and
the committed Cobertura report cited in §0, with exact paths and line numbers given throughout.*
