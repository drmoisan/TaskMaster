# WebView2 host / initializer defects — implementation research

- Date: 2026-08-24T00-45
- Feature: `webview2-host-initializer-defects` (primary issue #476; closes #458, #476, #477)
- Mode: read-only research. No production or test source file was modified.
- Evidence basis: direct reading of the files cited. Every claim below carries a `file:line`.
  Statements that could not be verified from source are collected in
  [§9 Open questions](#9-open-questions-explicitly-unverified) and are marked UNVERIFIED.

All paths are repository-relative.

---

## 0. Writable / forbidden file set (restated, because it drives every recommendation)

Writable production:

- `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`
- `QuickFiler/Viewers/WebView2CoreInitializer.cs`
- `QuickFiler/Viewers/IWebViewCoreInitializer.cs`

Forbidden (sibling-owned): `QuickFiler/Viewers/WebView2Messenger.cs`,
`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`, `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`,
`QuickFiler/Viewers/BreadcrumbMessengerHub.cs`, `QuickFiler/Controllers/EfcFormController.cs`.

Two files matter to the design and are in **neither** list, so they are treated here as
"do not edit unless unavoidable, and call it out":

- `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` — the marshalling seam. **The recommended design
  requires no edit to it** (see §2.3).
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:64` — the second production caller of
  `IWebViewCoreInitializer.CreateEnvironmentAsync`. This is the file that makes #477 Option A
  infeasible inside the declared writable set (see §4.4).

---

## 1. Q1 — #458, constructor-side unhook

### 1.1 The delegate-equality no-op is confirmed

`QuickFiler/Viewers/WebView2BreadcrumbHost.cs:43-51`:

```csharp
public WebView2BreadcrumbHost(WebView2 control, IWebViewCoreInitializer initializer)
{
    _control = control ?? throw new ArgumentNullException(nameof(control));
    _initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));

    // Idempotent hookup: pooled viewers re-run initialization, so unhook before hooking.
    _control.CoreWebView2InitializationCompleted -= OnCoreInitializationCompleted;   // :49
    _control.CoreWebView2InitializationCompleted += OnCoreInitializationCompleted;   // :50
}
```

`OnCoreInitializationCompleted` is declared as an instance method at
`WebView2BreadcrumbHost.cs:115-118`. The delegate formed at `:49` therefore has `this` (the
instance under construction) as its target. Delegate equality in .NET is pairwise over
`(target, method)`, so `-=` at `:49` can only remove a subscription made by *this* instance,
which has made none. The removal matches nothing. **Confirmed as stated in the potential
document.**

The same instance-bound pattern recurs at `WebView2BreadcrumbHost.cs:131-132` for
`core.WebMessageReceived`, with the same cross-instance limitation.

There is a third, downstream retention edge in the same lifecycle: `BreadcrumbBridgeRouter.cs:54`
subscribes `_host.MessageReceived += OnHostMessageReceived` and never unsubscribes, so a stale host
also retains its stale router.

### 1.2 Reachability of the "pooled viewer reuse" premise — a correction to the issue text

The potential document and the class XML doc (`WebView2BreadcrumbHost.cs:19`) both attribute the
scenario to `EfcViewerQueue` recycling. **That attribution does not survive reading the queue.**

- `QuickFiler/Helper Classes/ViewerQueueCore.cs` is the queue implementation. It exposes
  `BuildQueue` (`:39`, `:52`), `Dequeue` (`:63`), `DequeueChunk` (`:87`), and `Reset` (`:116`).
  Every enqueue path calls `_viewerFactory()` (`:46`, `:59`, `:99`, `:104`, `:136`, `:146`). **There
  is no method that returns a previously-dequeued viewer to `_queue`.**
- `QuickFiler/Helper Classes/EfcViewerQueue.cs:81-84` — `CreateProductionViewer()` returns
  `new EfcViewer()`. `EfcViewerQueue.CreateProductionCore()` (`:71-79`) passes only four arguments,
  so `ViewerQueueCore`'s optional `disposeViewer` (`ViewerQueueCore.cs:23`) is null for this queue.

The queue is therefore a **pre-warm pool of fresh instances**, not a recycle pool. A second
`WebView2BreadcrumbHost` over the same `WebView2` instance requires the same `EfcViewer` to be
wired twice.

The single construction site is `EfcFormController.cs:836-839`, inside `ConfigureBreadcrumbControl()`
(`:834`), which is called exactly once from `WireEventHandlers()` at `:393`. `WireEventHandlers()` is
`public` (`:370`) and is called from `Initialize()` (`:96`) and `InitializeWithoutData()` (`:109`).
No in-repo caller invokes either twice on the same controller/viewer pair
(`EfcHomeControllerDependencyFactories.cs:80`, `:92`, `:120`, `:124-125` each run one initializer per
freshly constructed controller).

**Conclusion:** the defect is a real correctness defect in the type (the `-=` is dead code that
misrepresents its own comment, and the class is not safe to construct twice over one control), but
in the current production wiring it is **latent, not live**. This matters for the acceptance
criteria: a failing-first regression test must be written at the unit level (two hosts, one control)
rather than as a production-path repro. Recording this prevents an executor from spending a cycle
trying to reproduce a pooled-viewer recycle that does not exist.

### 1.3 Option (a) — `IDisposable` / `Detach()`: **not implementable within the writable set**

Exhaustive search for anything that could call it:

| Candidate call site | Evidence | Verdict |
| --- | --- | --- |
| `EfcFormController.Cleanup()` | `EfcFormController.cs:189-196` — unsubscribes `_globals.Ol.PropertyChanged`, nulls `_globals`, `_formViewer`, `_dataModel`, invokes `_parentCleanup`. **Does not touch `_breadcrumbHost` (`:140`) or `_router` (`:141`).** | Would need an edit to a forbidden file |
| `EfcFormController` disposal | `Dispose`/`IDisposable` appear in that file only at `:728` and `:791`, both `_formViewer.Dispose()`. The controller implements no `IDisposable`. | None exists |
| `EfcViewer` disposal | `EfcViewer` is a `Form` (`EfcViewer.cs:21`); disposal is Designer-generated. `EfcViewer.cs` is not in the writable set. | Not writable |
| `EfcViewerQueue` / `ItemViewerQueue` recycling | No return-to-pool path exists (§1.2). `ViewerQueueCore.Reset()` (`:116-124`) drains and calls the null `_disposeViewer`. | None exists |

**No disposal or recycling path reaches `_breadcrumbHost`.** A `Detach()`/`Dispose()` added to
`WebView2BreadcrumbHost.cs` would have zero callers and would therefore fix nothing.

There is one self-driven variant that *is* inside the writable set: the host can subscribe
`_control.Disposed` and unsubscribe itself from that handler. That is useful hygiene, but it does
**not** address #458's stated failure, which is two live hosts over one *undisposed* control.

**Residual risk of option (a) alone:** the defect remains unfixed; the only observable change is
that a disposed control no longer retains its host. Recommend adopting it as a secondary measure,
not as the fix.

### 1.4 Option (b) — per-control owner registry: **implementable, and recommended**

Mechanism, entirely inside `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`:

```
private static readonly ConditionalWeakTable<WebView2, WebView2BreadcrumbHost> _owners = new ...;
private static readonly object _ownersGate = new object();
```

In the constructor, under `_ownersGate`: `TryGetValue(control, out previous)`; if a previous owner
exists, call its private `DetachCore()` (which performs the real `-=` from the *previous* instance,
whose delegate target matches); then `Remove(control)` and `Add(control, this)`.

Feasibility / thread-safety / lifetime under .NET Framework 4.8.1:

- **Availability.** `System.Runtime.CompilerServices.ConditionalWeakTable<TKey,TValue>` is present
  since .NET Framework 4.0, so it is available on `net481`. Use the unambiguously-available
  `TryGetValue` / `Remove` / `Add` trio rather than `AddOrUpdate` (see §9, item 3).
- **Thread-safety.** Individual `ConditionalWeakTable` operations are documented thread-safe, but a
  read-then-write sequence is not atomic. An explicit `lock` over the compound operation is
  required and is cheap: the only production construction site
  (`EfcFormController.cs:836`) is single-threaded per form, so contention is nil.
- **Lifetime / leak.** The table's value is held through a dependent handle keyed on the control, so
  an entry is collectible once the control is. The static table is a process-wide root but retains
  nothing beyond the lifetime of controls that are themselves alive. Critically, the *net* effect is
  a reduction in retention: detaching the predecessor removes the `control -> stale host` edge that
  is the leak #458 describes, and the table adds no edge that outlives the control.
- **Key type.** Key on `WebView2` (or on `Control`); reference identity is the correct key semantics
  and `ConditionalWeakTable` uses reference equality unconditionally.

**Residual risk of option (b):** (i) a static registry is process-wide state, so a unit test must
either use a distinct control instance per test or expose an internal reset — recommend distinct
instances, which keeps the tests independent per `.claude/rules/general-unit-test.md`; (ii) the
registry changes notification counts, which is exactly the behavior change #458 asks for and is why
it was excluded from epic #136's no-behavior-change NFR.

**Recommendation for #458: option (b) as the fix, plus the `_control.Disposed` self-detach from
§1.3 as secondary hygiene. Both live wholly inside `WebView2BreadcrumbHost.cs`.**

---

## 2. Q2 — #476 defect 1, UI marshalling

### 2.1 `BreadcrumbUiDispatcher` — full API surface

File `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`, `#nullable enable` at `:1`,
`internal sealed class` at `:12`.

| Member | Line | Signature | Notes |
| --- | --- | --- | --- |
| `_executingDispatcher` | `:14-15` | `[ThreadStatic] private static BreadcrumbUiDispatcher?` | The inline-safety token |
| ctor (public-to-assembly) | `:25-30` | `internal BreadcrumbUiDispatcher(SynchronizationContext context, Action<Exception> errorSink)` | Throws `ArgumentNullException` on null `context` (`:27`); delegates with `ownerThreadId: null` |
| ctor (private) | `:32-41` | `private BreadcrumbUiDispatcher(SynchronizationContext?, Action<Exception>, int?)` | Throws on null `errorSink` (`:39`) |
| `CaptureCurrent` | `:44-56` | `internal static BreadcrumbUiDispatcher CaptureCurrent()` | **Throws `InvalidOperationException` when `SynchronizationContext.Current` is null (`:48-50`).** Captures that context plus `Environment.CurrentManagedThreadId`, sink = `LogFailure` |
| `CreateForCurrentThreadTests` | `:62-65` | `internal static BreadcrumbUiDispatcher CreateForCurrentThreadTests()` | Null context, owner-thread-only |
| `Dispatch` | `:71-151` | `internal Task Dispatch(Action action)` | Returns `Task` (never `Task<T>`). Inline on boundary (`:78-95`); otherwise `_context.Post` (`:122`) and completes the returned task after the action runs or fails. Throws `ArgumentNullException` on null action (`:74-77`) |
| `DispatchValue<T>` | `:157-235` | `internal Task<T> DispatchValue<T>(Func<T> action, bool reportFailure = true)` | **Runs inline only when `ReferenceEquals(_executingDispatcher, this)` (`:166`)** — i.e. only from inside a currently-executing `Dispatch` callback. Otherwise posts, or faults for the test dispatcher (`:180-188`) |
| `Report` | `:238-253` | `internal void Report(Exception exception)` | Routes to `_errorSink`, swallowing sink failure into log4net |
| `IsCurrentBoundary` | `:255-278` | `private bool` | Ambient-context reference match when `_context != null` (`:269-272`); thread-id match only for the context-less test dispatcher (`:276-277`) |
| `LogFailure` | `:280-283` | `private static void` | Not reachable from outside |

There are exactly **two** static production/test factories (`CaptureCurrent`,
`CreateForCurrentThreadTests`) and **two** dispatch overloads (`Dispatch`, `DispatchValue<T>`).

### 2.2 The `WebView2Messenger` precedent, and what its factory actually captures

`QuickFiler/Viewers/WebView2Messenger.cs:20-49, 138-145`:

- `[ExcludeFromCodeCoverage] public sealed class WebView2Messenger : IWebViewMessenger, IDisposable`
  (`:20-21`).
- Public ctor `WebView2Messenger(CoreWebView2 coreWebView)` (`:33`) chains to an **internal**
  overload `WebView2Messenger(CoreWebView2 coreWebView, BreadcrumbUiDispatcher dispatcher)` (`:36`)
  via `: this(coreWebView, CaptureProductionDispatcher(coreWebView)) { }` (`:34`).
- `private static BreadcrumbUiDispatcher CaptureProductionDispatcher(CoreWebView2 coreWebView)`
  (`:138-145`): it **null-guards the control argument (`:140-143`) and then returns
  `BreadcrumbUiDispatcher.CaptureCurrent()` (`:144`)**. It captures **nothing from the control**.
  The argument exists purely so the null-argument failure is raised before the
  ambient-context failure, preserving the documented `ArgumentNullException` contract at `:32`.
  What is actually captured is `SynchronizationContext.Current` and
  `Environment.CurrentManagedThreadId` of the constructing thread
  (`BreadcrumbUiDispatcher.cs:46-55`).
- Every SDK touch is wrapped: subscription (`:40-48`), `PostJson` (`:62-68`), `Dispose` (`:80-94`),
  and the inbound re-raise (`:104-122`). Note that `PostJson` performs the guard and the SDK call
  **inside a single `Dispatch` callback**, and never uses `DispatchValue`.

### 2.3 Can `WebView2BreadcrumbHost` be marshalled without touching `EfcFormController.cs:836`?

**Yes.** The mechanism is the exact `WebView2Messenger` shape:

```csharp
public WebView2BreadcrumbHost(WebView2 control, IWebViewCoreInitializer initializer)
    : this(control, initializer, CaptureProductionDispatcher(control)) { }

internal WebView2BreadcrumbHost(
    WebView2 control,
    IWebViewCoreInitializer initializer,
    BreadcrumbUiDispatcher dispatcher) { ... }
```

The public two-argument signature is unchanged, so `EfcFormController.cs:836-839` compiles and
behaves identically at the call site. `internal` members are visible to the test assembly:
`QuickFiler/Properties/AssemblyInfo.cs:5` declares `[assembly: InternalsVisibleTo("QuickFiler.Test")]`,
and tests already construct `BreadcrumbUiDispatcher` directly (for example
`QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:166`,
`QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs:325`) and call
`BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` (for example
`QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs:184`).

**What the dispatcher needs to capture from the `WebView2` control: nothing.** It captures the
ambient `SynchronizationContext` (and thread id) of whichever thread constructs it. This is worth
stating plainly because it is the single most likely misreading of the precedent.

**Routing the three operations.** All three must go through `Dispatch`, not `DispatchValue`:

- `NavigateToString` (`WebView2BreadcrumbHost.cs:66-69`) → `_ = _dispatcher.Dispatch(() => _control.NavigateToString(html));`
- `PostMessageJson` (`:72-84`) → one `Dispatch` callback containing **both** the
  `_control.CoreWebView2` read (currently `:74`) and the `core.PostWebMessageAsJson(json)` call
  (currently `:83`), plus the existing null-guard/log at `:75-81`.
- The `_control.CoreWebView2` read inside `OnCoreInitializationCompleted` (`:129`) already runs on
  the UI thread because the SDK raises `CoreWebView2InitializationCompleted` there; wrapping it is
  optional and would be a no-op inline dispatch.

**Do not use `DispatchValue` to read `CoreWebView2` as a separate step.** `DispatchValue` runs
inline only when `_executingDispatcher == this` (`BreadcrumbUiDispatcher.cs:166`), so calling it
outside a `Dispatch` callback on the owner-thread-only test dispatcher returns a *faulted* task
(`:180-188`). Reading and posting in one `Dispatch` callback both matches the precedent and avoids
that trap.

### 2.4 The capture-point risk, and the recommended variant

`BreadcrumbUiDispatcher.CaptureCurrent()` **throws** when `SynchronizationContext.Current` is null
(`:46-50`). Adopting `CaptureProductionDispatcher` verbatim therefore introduces a new throwing
precondition on the `WebView2BreadcrumbHost` constructor, which is currently total apart from its
two null guards.

Whether `SynchronizationContext.Current` is non-null at `EfcFormController.cs:836` could not be
established with certainty from source (see §9, item 1). The surrounding code is actively
defensive about exactly this: `EfcFormController.cs:451-452` and `:704-705`, and
`KeyboardHandler.cs:240-241` and `:268`, all carry
`if (SynchronizationContext.Current is null) SynchronizationContext.SetSynchronizationContext(...)`.
Those guards are in-repo evidence that a null ambient context is observed on real entry paths.

Two ways to remove the risk, both inside the writable file:

- **Preferred (V1) — capture from the argument the host is already given.** `InitializeAsync`
  already receives the UI `SynchronizationContext` explicitly
  (`WebView2BreadcrumbHost.cs:92`, null-guarded at `:94-97`, awaited at `:106`). Build the dispatcher
  there with the public internal ctor `new BreadcrumbUiDispatcher(uiSyncContext, LogDispatchFailure)`
  and keep a null-dispatcher fallback that executes inline before initialization. This needs **no
  edit to `BreadcrumbUiDispatcher.cs`** (its two-argument `internal` ctor at `:25-30` is public to
  the assembly and takes a caller-supplied sink) and adds no new throwing precondition. It is also
  well-matched to the actual call ordering: both readers only call the host *after*
  `IsCoreInitialized` is true (see §3.1), which is after `InitializeAsync` has run.
- **Alternative (V2) — verbatim precedent.** `CaptureProductionDispatcher(control)` calling
  `CaptureCurrent()`. Simpler, matches `WebView2Messenger` byte-for-byte in shape, but carries the
  new-throw risk above.

**Recommendation for #476 defect 1: internal three-argument constructor overload + V1 capture
(dispatcher installed in `InitializeAsync` from `uiSyncContext`), with the public two-argument
constructor unchanged.** If the executor can *verify* a non-null ambient context at
`EfcFormController.cs:836`, V2 is acceptable and is the closer precedent match; V1 is the choice
that does not require that verification.

---

## 3. Q3 — #476 defect 2, state publication

### 3.1 Auto-property and writer confirmed; every reader enumerated

- Declaration: `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:54` —
  `public bool IsCoreInitialized { get; private set; }`. A plain auto-property: a non-volatile
  compiler-generated backing field, no barrier.
- Sole write: `WebView2BreadcrumbHost.cs:134` — `IsCoreInitialized = true;`, inside
  `OnCoreInitializationCompleted` (`:115`), immediately after the `core.WebMessageReceived`
  subscription at `:131-132` and immediately before `CoreInitialized?.Invoke(...)` at `:135`.
- Interface declaration: `QuickFiler/Viewers/IBreadcrumbWebHost.cs:25` — `bool IsCoreInitialized { get; }`.

**Exhaustive list of in-repo readers** (`rg IsCoreInitialized --glob '*.cs'`):

| # | Reader | file:line | Kind |
| --- | --- | --- | --- |
| 1 | `BreadcrumbOutboundQueue.PostOrQueue` | `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs:44` | Production |
| 2 | `BreadcrumbBridgeRouter.DeliverDocument` | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:400` | Production |
| 3 | `EfcHomeControllerExecuteMovesTests` | `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs:257` | Test (`SetupGet` on a mock) |
| 4 | `BreadcrumbBridgeRouterTests` | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs:40` | Test (`SetupGet`) |
| 5 | `BreadcrumbBridgeRouterQueueTests` | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs:42` | Test (`SetupGet`) |

There are no other readers. There is no reader of the concrete
`WebView2BreadcrumbHost.IsCoreInitialized` anywhere — every consumer goes through the
`IBreadcrumbWebHost` seam.

### 3.2 Which readers can run off the UI thread

Both production readers can, and neither type has any thread affinity or marshalling of its own:

- **`BreadcrumbOutboundQueue`** (`QuickFiler/Controllers/BreadcrumbOutboundQueue.cs:15`) is a plain
  class holding a non-synchronised `Queue<string>` (`:18`). It contains no dispatcher, no lock, and
  no context capture. It runs on whatever thread its caller is on.
- **`BreadcrumbBridgeRouter`** (`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:16`) likewise holds
  no dispatcher. A file-scoped search for `ConfigureAwait` in `BreadcrumbBridgeRouter.cs` returns
  **zero** hits, so its `await` continuations (`:99`, `:176`, `:179`, `:191`, `:222`, `:239`, `:296`,
  `:309`, `:341`, `:350`) resume on the ambient `SynchronizationContext` of the caller — and, when
  that ambient context is null, on a thread-pool thread.

The static reachability argument for a null ambient context:

1. `EfcFormController.RefreshSuggestionsAsync()` (`:797-806`) performs
   `await Task.Run(...)` twice (`:799`, `:800-803`) with no `ConfigureAwait`, then calls
   `BindFolderRows(matches)` at `:805`.
2. `RefreshSuggestionsAsync` is reached from two kinds of entry point. `ButtonRefresh_Click`
   (`:454`) installs a context first (`:451-452`). The keyboard actions at `:592` and `:657` reach it
   through `KbdExecuteAsync` (`:812-816` / `:818-822`), which carries **no** such guard.
3. Under a null ambient context, the `Task.Run` continuation at `:805` runs on a thread-pool thread.
4. `BindFolderRows` (`:873`) fire-and-forgets `BindBreadcrumbRowsAsync` (`:882`), which awaits
   `_router.BindRowsAsync(...)` (`:893`).
5. `BreadcrumbBridgeRouter.BindRowsAsync` (`:74`) awaits `FetchChainAsync` (`:99`), which awaits the
   provider (`:341`, `:350`). The provider
   `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` uses
   `.ConfigureAwait(false)` internally at `:35`, `:46`, `:62`, so its own completion lands on a
   thread-pool thread.
6. `BindRowsAsync` then calls `DeliverDocument()` (`:115`), which **reads `_host.IsCoreInitialized`
   at `:400` and calls `_host.NavigateToString(...)` at `:402`** on that thread.
7. The same chain reaches `PostOutbound` (`:392-395`) → `BreadcrumbOutboundQueue.PostOrQueue`
   (`:37`) → `_host.IsCoreInitialized` at `:44` and `_host.PostMessageJson(json)` at `:46`.

This is a code-reading reachability argument, not a runtime observation (see §9, item 1). It is
sufficient to establish that neither reader is *structurally* confined to the UI thread, which is
the property the fix must not depend on.

One further ordering point that reinforces the fix: the router's `_pendingDocument` /
`_outboundQueue` deferral logic (`:142-148`, `:400-408`, `BreadcrumbOutboundQueue.cs:44-51`) is a
compare-and-publish protocol whose correctness depends on the reader observing both the
`core.WebMessageReceived` subscription (`WebView2BreadcrumbHost.cs:131-132`) and the flag write
(`:134`) in program order.

### 3.3 Minimal correct fix, and availability of `System.Threading.Volatile`

Replace the auto-property with an explicit backing field and a `Volatile.Read` / `Volatile.Write`
pair:

```csharp
private int _coreInitialized;   // or bool; int avoids any question about volatile-on-bool
public bool IsCoreInitialized => Volatile.Read(ref _coreInitialized) != 0;
// at :134, AFTER the subscription at :131-132:
Volatile.Write(ref _coreInitialized, 1);
```

The ordering is load-bearing: `Volatile.Write` is a release store, so a reader that observes the
flag via `Volatile.Read` (an acquire load) is guaranteed to observe the preceding subscription.
Keeping the write at its current position (after `:131-132`, before `:135`) is therefore correct and
must not be reordered by the executor.

**Availability under `net481`: verified, and already used in this very project.**

- `QuickFiler/Viewers/WebView2Messenger.cs:127` — `return Volatile.Read(ref _disposeRequested) != 0;`
  (same `QuickFiler` project, `TargetFrameworkVersion` `v4.8.1`).
- `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.Lifecycle.cs:214`
  — `private bool IsDisposed => Volatile.Read(ref _disposeState) != 0;`
- Test-side usage: `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs:312-313`,
  `AppOlObjectsFolderTreeServiceLifecycleTests.cs:155-160`, `:462` (`Volatile.Write`),
  `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs:236`.

`using System.Threading;` is already present at `WebView2BreadcrumbHost.cs:5`, so no new `using` is
required.

**Note on the interaction with §2.** If the dispatcher route (V1) is adopted, the flag is still
written on the UI thread and still read from arbitrary threads (the readers are outside the host and
are not being changed), so the `Volatile` pair is required **in addition to** the dispatcher, not
instead of it. The potential document's phrasing ("or publish state through the dispatcher") offers
these as alternatives; they are not, because the readers at
`BreadcrumbOutboundQueue.cs:44` and `BreadcrumbBridgeRouter.cs:400` call the property directly and
synchronously.

---

## 4. Q4 — #477, the `IWebViewCoreInitializer` contract

### 4.1 (a) Every in-repo implementer, including test doubles and Moq sites

**Concrete implementers: exactly one.**

| Implementer | file:line |
| --- | --- |
| `WebView2CoreInitializer` | `QuickFiler/Viewers/WebView2CoreInitializer.cs:16` (`public sealed class WebView2CoreInitializer : IWebViewCoreInitializer`) |

A search for `: IWebViewCoreInitializer` across all `*.cs` returns only that one declaration. **There
is no hand-written test fake, stub, or spy** implementing the interface anywhere in the repository.

**Moq mock sites: eleven, in eight files.**

| # | file:line | Behaviour |
| --- | --- | --- |
| 1 | `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:32` | `MockBehavior.Strict` |
| 2 | `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:68` | `MockBehavior.Strict` |
| 3 | `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:99` | `MockBehavior.Strict` |
| 4 | `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:198` | `MockBehavior.Strict` |
| 5 | `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs:340` | `MockBehavior.Strict` |
| 6 | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs:96` | `MockBehavior.Strict` |
| 7 | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs:244` | `MockBehavior.Strict` |
| 8 | `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs:307` | `MockBehavior.Strict` |
| 9 | `QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs:154` | Loose (default) |
| 10 | `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs:120` | Loose (default) |
| 11 | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:272` | Loose (default) — the only site that `Setup`s either member (`:273-288`) |

Related non-mock references: `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:39`
and `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs:129, :355, :361` reference
`typeof(IWebViewCoreInitializer)` reflectively; `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs:19,22`
constructs the concrete type and asserts assignability.

### 4.2 (b) Every in-repo caller of `CreateEnvironmentAsync`

| # | Caller | file:line | Note |
| --- | --- | --- | --- |
| 1 | `WebView2BreadcrumbHost.InitializeAsync` | `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:108-111` | **Writable** |
| 2 | `QfcItemController.InitializeWebViewAsync` | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:64-67` | **Not writable** |
| 3 | `BuildWebViewInitializerMock` (Moq `Setup`) | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:273-280` | Test, writable |

Declaration `IWebViewCoreInitializer.cs:19-22`; implementation `WebView2CoreInitializer.cs:19-22`.

Not a caller, but a second production site of the same hard-coded `null`:
`QuickFiler/Controllers/EfcItemController.cs:223-227` calls
`CoreWebView2Environment.CreateAsync(null, cacheFolder, options)` **directly on the SDK**, bypassing
the seam entirely (with an earlier variant around `:186-192`). Worth recording: fixing the seam does
not fix that call site, and that file is outside the writable set.

### 4.3 (c) Every in-repo caller of `EnsureCoreWebView2Async`

| # | Caller | file:line | Note |
| --- | --- | --- | --- |
| 1 | `WebView2BreadcrumbHost.InitializeAsync` | `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:112` | **Writable** |
| 2 | `QfcItemController.InitializeWebViewAsync` (body pane) | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:68-71` | Not writable |
| 3 | `QfcItemController.InitializeWebViewAsync` (breadcrumb pane) | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:111-114` | Not writable |
| 4 | `BreadcrumbPopupUiOperations` | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:388` | **FORBIDDEN** |
| 5 | `BuildWebViewInitializerMock` (Moq `Setup`) | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:281-288` | Test, writable |

Declaration `IWebViewCoreInitializer.cs:28`; implementation `WebView2CoreInitializer.cs:25-28`.
`EfcItemController.cs:201` and `:236` call `EnsureCoreWebView2Async` on the SDK control directly, not
through the seam.

### 4.4 Option A vs Option B for the hard-coded `browserExecutableFolder`

**Option A — surface `browserExecutableFolder` on the interface.**

Files that must change:

| File | Why | Writable? |
| --- | --- | --- |
| `QuickFiler/Viewers/IWebViewCoreInitializer.cs:19-22` | signature | Yes |
| `QuickFiler/Viewers/WebView2CoreInitializer.cs:19-22` | implementation | Yes |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:108-111` | call site | Yes |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:64-67` | call site | **No — outside the declared writable production set** |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:273-280` | Moq `Setup` argument arity | Yes (tests writable) |

**Blast radius: 5 files, one of which is outside the writable production set.** A default-valued
optional parameter would not avoid the interface edit and would not avoid the arity change in the
Moq `Setup` expression at `Part2.cs:275-278` (a `Setup` lambda must match the full signature).

If the change were ever extended to `EnsureCoreWebView2Async`, it would additionally require
`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:388`, which is **on the forbidden list**, and the
change would be blocked outright.

The potential document's own Next Step records
`Confirm whether fixed-version WebView2 distribution is a product requirement` as an
**unchecked / UNCONFIRMED** item (`docs/features/potential/promoted/2026-08-07-iwebviewcoreinitializer-contract-defects.md:121`).
Option A therefore changes a public contract on the strength of an unconfirmed requirement.

**Option B — keep the two-argument signature; document the `null` as a deliberate Evergreen-only
decision.**

Files that must change:

| File | Why | Writable? |
| --- | --- | --- |
| `QuickFiler/Viewers/IWebViewCoreInitializer.cs:7-12, 15-22` | XML doc: correct the "forwards 1:1" claim at `:10-11`; document the Evergreen-only `null` | Yes |
| `QuickFiler/Viewers/WebView2CoreInitializer.cs:8-14` | XML doc: restate the exemption on the accurate ground (external Evergreen runtime + user-data-folder creation), not "1:1 forwarding" | Yes |

**Blast radius: 2 files, both writable, zero call-site edits, zero test edits.**

**Recommendation for #477: Option B, plus the argument guards.** Option A is not achievable inside
the declared writable file set, and its motivating requirement is explicitly unconfirmed. If the
fixed-version requirement is later confirmed, Option A becomes a separate, correctly-scoped issue
that can also take in the direct SDK call at `EfcItemController.cs:223-227`.

The guards (defect 2) belong on the concrete class only:

- `CreateEnvironmentAsync`: `ArgumentNullException`/`ArgumentException` on null-or-whitespace
  `cacheFolder`; `ArgumentNullException` on null `options` (UNVERIFIED whether the SDK accepts a null
  `options` — see §9, item 4; if it does, guard `cacheFolder` only and document the tolerance).
- `EnsureCoreWebView2Async`: `ArgumentNullException` on null `control`; the SDK accepts a null
  `environment` (it then creates a default one), so do **not** guard `environment`.

Convention precedent for the guard style: `WebView2Messenger.cs:38-39`,
`WebView2BreadcrumbHost.cs:45-46`, `BreadcrumbUiDispatcher.cs:27, :39, :74-77, :159-162`.

### 4.5 Do the strict mocks break when guards are added to the concrete class? **No — nil.**

Eight `MockBehavior.Strict` sites exist (§4.1, rows 1-8). Moq generates a dynamic proxy of the
**interface**; it never executes `WebView2CoreInitializer`'s body. Guards added to the concrete class
are therefore invisible to every mock, strict or loose.

Concretely:

- None of the eight strict sites `Setup` either member; they pass `.Object` as a collaborator and, in
  one case, assert `initializer.VerifyNoOtherCalls()`
  (`QfcItemControllerBreadcrumbDropDownTests.cs:56`). A strict mock with no setups throws only if a
  member is actually invoked — which those tests assert does not happen.
- The only `Setup` of either member is on the **loose** mock at
  `QfcItemController.InitializationTests.Part2.cs:272-289`, and it uses `It.IsAny<>` matchers plus
  `ThrowsAsync`, so it is insensitive to any change in the concrete class.
- The one test that instantiates the concrete type,
  `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs:19`, uses the implicit parameterless
  constructor and invokes neither member.

**Test breakage from adding guards: nil.** This holds for both Option A and Option B; it is the
mock-arity change under Option A (`Part2.cs:275-278`), not the guards, that would touch a test file.

---

## 5. Q5 — testability under repository policy

### 5.1 (a) What STA / real-control infrastructure already exists

**A real WinForms message-pump host exists and is production-quality.**

`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` — `internal sealed class WinFormsPumpHost : IDisposable`
(`:26`). It starts a dedicated background thread with `SetApartmentState(ApartmentState.STA)`
(`:58`), installs a `WindowsFormsSynchronizationContext` (`:303-306`), and runs
`Application.Run(applicationContext)` (`:326`). It exposes `SyncContext` (`:72`), `ThreadId` (`:75`),
`InvokeAsync(Action)` (`:81`), `InvokeAsync<TResult>(Func<TResult>)` (`:111`), `RunAsync(Func<Task>)`
(`:140`), `RunAsync<TResult>` (`:176`), `StopAsync()` (`:214`), and `Dispose()` (`:232`). Its own
XML doc (`:107-110`) names `host.InvokeAsync(() => new QuickFiler.ItemViewer())` as the intended
usage. It is registered in the project at `QuickFiler.Test/QuickFiler.Test.csproj:161`, with its own
tests at `:162`.

Existing consumers: `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs:43, :86, :134, :179, :248, :359`;
`QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs:308, :379`;
`QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs:429`.

**A real `Microsoft.Web.WebView2.WinForms.WebView2` control is already constructed in QuickFiler unit
tests.** Not directly, but transitively and deliberately:

- `QuickFiler/Viewers/ItemViewer.Designer.cs:46` and `:49` construct
  `new Microsoft.Web.WebView2.WinForms.WebView2()` (breadcrumb and body panes), and `:89-90` call
  `ISupportInitialize.BeginInit()` on both.
- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs:354-385`
  (`BuildPumpHarness_DoesNotCreateTheWebViewChildHandles`) reads
  `harness.Viewer.L0v2h2_WebView2.IsHandleCreated` (`:367`) and
  `harness.Viewer.L0vhBreadcrumb_WebView2.IsHandleCreated` (`:371`) on the pump thread and asserts
  both are `true`. The remarks at `:344-353` record that this was **measured**, and that the handles
  originate in `InitializeComponent`'s `ISupportInitialize.EndInit()`.

This is decisive: constructing a `WebView2` **control** in a unit test does not require the Evergreen
runtime. Only `EnsureCoreWebView2Async` / `CoreWebView2Environment.CreateAsync` do, which is exactly
the boundary that `IWebViewCoreInitializer` exists to isolate.

**Other STA infrastructure (for completeness).** `[STATestMethod]` is used in
`Tags.Test/TagControllerRendering.StaTests.cs:21`, `Tags.Test/CheckBoxControllerWiring.StaTests.cs:23, :43, :64`,
`UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:366, :411, :478`, and throughout
`UtilitiesCS.Test/Extensions/WinFormsExtensions_Tests.cs`. Manual STA threads appear at
`UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs:185`, `ProgressTrackerAsync_Tests.cs:196`,
`QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:45`, and
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:277, :312`. **No `*StaTests.cs` file
exists in `QuickFiler.Test`**; that project uses `WinFormsPumpHost` instead.

**Uninitialized-instance precedent.** `FormatterServices.GetUninitializedObject` is used to
fabricate SDK types that cannot be constructed:
`QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs:305-306, :406-407`,
`BreadcrumbDropDownCoverageThresholdTests.cs:300-301`, `BreadcrumbDropDownLifecycleTests.cs:166-167`,
`BreadcrumbDropDownLifecycleConcurrencyTests.cs:252-253`, `BreadcrumbDropDownReadinessTests.cs:316-317`,
`BreadcrumbPendingOpenCloseTests.cs:211-212`, `BreadcrumbPopupBoundaryCoverageTests.cs:200, :211, :215`,
`BreadcrumbPopupBoundaryCoverageTests.Part2.cs:202, :380`.

**Structural guard to respect.** `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:16-36` asserts that
**no `System.Windows.Forms.Form`-derived type is compiled into the test assembly**. It scans only
`Assembly.GetExecutingAssembly()` (`:21`) — it does not restrict *instantiating* controls from
referenced assemblies, and `WebView2` derives from `UserControl`, not `Form`. Constructing a real
`WebView2` in a test therefore does not violate it, but defining a new `Form` subclass in the test
project would.

### 5.2 (b) Which defects can be covered failing-first, and which need a seam

`WebView2BreadcrumbHost` currently carries `[ExcludeFromCodeCoverage]` at
`QuickFiler/Viewers/WebView2BreadcrumbHost.cs:29`, with the rationale at `:22-28`.

| Defect | Failing-first test possible? | Seam required |
| --- | --- | --- |
| **#476 defect 1 — marshalling** | **Yes**, with the internal ctor overload from §2.3. | Internal 3-arg ctor in `WebView2BreadcrumbHost.cs`. No new file. |
| **#476 defect 2 — state publication** | **Partially.** See §5.2.3. | None. |
| **#458 — predecessor unhook** | **Yes**, with an internal observation point on the registry. | Internal member in `WebView2BreadcrumbHost.cs`. No new file. |
| **#477 defect 2 — guards** | **Yes**, directly. | None. |
| **#477 defect 1 — doc correction** | Doc-only; no behavioural test. | None. |

**5.2.1 #476 defect 1 (recommended test shape).** Construct a `WebView2` on `WinFormsPumpHost`
(`host.InvokeAsync(() => new Microsoft.Web.WebView2.WinForms.WebView2())`), then construct the host
through the internal 3-arg ctor with a recording `SynchronizationContext` and a recording error sink:
`new BreadcrumbUiDispatcher(recordingContext, errors.Add)` — the exact pattern at
`QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:166`. Call `PostMessageJson` /
`NavigateToString` from the MSTest thread (not the boundary) and assert the recording context
observed one `Post`. Because the recording context never *drains* the posted action, the `WebView2`
control is never touched at all, so no runtime is involved. This test fails today
(zero posts; the control is touched inline at `WebView2BreadcrumbHost.cs:68` and `:74`) and passes
after the fix. It is deterministic, uses no temp file, no external process, and no `Task.Delay`.

**5.2.2 #458 (recommended test shape).** Construct one `WebView2` on the pump, construct host A over
it, then host B over it, and assert that A is detached and B is the registered owner. Two candidate
observation points, both inside the writable file:

- an `internal bool IsAttached { get; }` on `WebView2BreadcrumbHost` set by attach/detach; or
- an `internal static bool TryGetOwner(WebView2 control, out WebView2BreadcrumbHost owner)` over the
  registry.

Either is visible to `QuickFiler.Test` via `QuickFiler/Properties/AssemblyInfo.cs:5`. Prefer the
first: it is an assertion about the *host*, not about the registry's implementation, and survives a
later change of registry mechanism. This test fails today because A stays attached.

Asserting the raw handler count on `WebView2.CoreWebView2InitializationCompleted` via reflection is
possible in principle but depends on the SDK's event implementation (field-like backing delegate vs.
`EventHandlerList`), which is UNVERIFIED (§9, item 2). Do not make it the primary assertion.

**5.2.3 #476 defect 2 — the honest limitation.** A memory-ordering defect cannot be made to fail
deterministically by a unit test: on x86/x64 the missing barrier is very unlikely to produce an
observable reordering, and a test that spins threads hoping to catch one would violate the
determinism requirement in `.claude/rules/general-unit-test.md`. Two defensible substitutes, in
order of preference:

1. **A structural test.** Assert by reflection that `WebView2BreadcrumbHost` declares an explicit
   backing field for the initialization flag and that `IsCoreInitialized` is not an auto-property
   (an auto-property's backing field carries `[CompilerGenerated]` and the
   `<IsCoreInitialized>k__BackingField` name). This is a genuine failing-first test: it fails against
   `:54` today. Precedent for structural/reflection assertions exists at
   `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:23-39` and
   `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs`.
2. **A behavioural ordering test.** Assert that a reader which observes `IsCoreInitialized == true`
   also observes the effects that precede the write — publishing the flag last. This is weaker but
   pins the intended publication order at `:131-135`.

Record explicitly in the plan that (1) is a structural proxy, not a proof of the race, so the
acceptance criterion is honest.

**5.2.4 If a new production file were needed.** It is not, under the recommended design — but for
completeness: `QuickFiler/QuickFiler.csproj` uses **explicit `<Compile Include>` entries**, and the
whole breadcrumb/WebView2 neighbourhood sits in one contiguous run at `QuickFiler.csproj:391-413`:

```xml
<Compile Include="Viewers\IWebViewCoreInitializer.cs" />   <!-- :408 -->
<Compile Include="Viewers\IBreadcrumbWebHost.cs" />        <!-- :409 -->
<Compile Include="Viewers\WebView2BreadcrumbHost.cs" />    <!-- :410 -->
<Compile Include="Viewers\IWebViewMessenger.cs" />         <!-- :411 -->
<Compile Include="Viewers\WebView2CoreInitializer.cs" />   <!-- :412 -->
<Compile Include="Viewers\WebView2Messenger.cs" />         <!-- :413 -->
```

Collision risk with concurrent sibling children is **real but bounded**: the siblings own
`WebView2Messenger.cs`, `BreadcrumbPopupUiOperations.cs`, `BreadcrumbBridgeCoordinator.cs`, and
`BreadcrumbMessengerHub.cs`, all of which are *already registered* (`:413`, `:397`, `:391`, `:401`),
so a sibling only touches this ItemGroup if it also adds a new file. If two children each append a
line to the same contiguous block, git will report a textual conflict on adjacent lines. **This is
the strongest argument for keeping every change inside the three already-registered writable files.**

### 5.3 (c) Coverage-exemption consequence

Per `CLAUDE.md` §UT2 and `.claude/rules/general-unit-test.md`, the COM/VSTO/WinForms exemption covers
only members that cannot be exercised without a live host; **testable seams within otherwise
COM-bound assemblies are explicitly NOT exempt**.

Consequences for this feature:

- Any internal seam introduced into `WebView2BreadcrumbHost.cs` (the 3-arg ctor, the registry
  observation member, the `Volatile` accessor) becomes testable and is therefore **not** covered by
  the file-level `[ExcludeFromCodeCoverage]` justification at `:22-29`.
- The class-level attribute at `:29` currently suppresses measurement of the whole type. Once
  `NavigateToString`, `PostMessageJson`, and the state accessor are reachable from tests, the
  class-level rationale text at `:23-27` ("every member forwards 1:1 to the WebView2 SDK … all
  routing/decision logic lives in the non-exempt `BreadcrumbBridgeRouter`/`BreadcrumbOutboundQueue`")
  is no longer accurate — the same false-rationale problem #477 identifies for
  `WebView2CoreInitializer.cs:8-14`. **The plan should either (i) remove the class-level attribute
  and let the genuinely host-bound members (`InitializeAsync`, `OnCoreInitializationCompleted`) carry
  member-level attributes with accurate rationales, or (ii) keep the class-level attribute and record
  in the coverage ledger why the newly testable members are still exempt — which would be
  indefensible once tests exist for them.** Option (i) is the one consistent with the rule.
- `WebView2CoreInitializer`'s exemption remains **justified** and should be retained; only its stated
  rationale changes (external Evergreen runtime plus user-data-folder creation), per
  `docs/features/potential/promoted/2026-08-07-iwebviewcoreinitializer-contract-defects.md:82-93`.
  Adding guards does not make it testable: `WebView2CoreInitializerTests.cs:17-23` can already assert
  construction, and a guard test would only exercise the throw path, which does not reach the SDK —
  so a *partial* member-level exemption is not achievable in `net481` (attributes are per-member, not
  per-branch). Recommend keeping the class-level attribute on `WebView2CoreInitializer` and *not*
  writing guard-throw tests for it, or removing the attribute and testing the guards. **Prefer the
  latter**: the guard branches are pure argument validation with no SDK dependency, so under the
  rule they are a testable seam and are not exempt. Under that choice the two forwarding bodies keep
  member-level `[ExcludeFromCodeCoverage]`, and the guards are measured.

`coverage.config` contains no entry for either type (searched); exemption in this area is entirely
attribute-driven.

---

## 6. Q6 — build and test mechanics

### 6.1 (a) `QuickFiler.Test.csproj` `Compile Include` ordering — the premise needs correcting

**The ItemGroup at `QuickFiler.Test/QuickFiler.Test.csproj:57-175` is not alphabetically sorted.** It
is grouped loosely by feature area, and the ordering is inconsistent even within a group (`:58`
`BreadcrumbBridgeRouterQueueTests.cs` precedes `:59` `BreadcrumbBridgeRouterTests.cs`, but `:60`
jumps from `Controllers\` to `Viewers\`, and `:96` returns to `Controllers\`). MSBuild imposes no
ordering requirement on `Compile` items.

There is exactly **one** `WebView2*` test file. Its exact neighbourhood, verbatim from
`QuickFiler.Test/QuickFiler.Test.csproj:158-160`:

```xml
    <Compile Include="Controllers\WpfUiDispatcherTests.cs" />
    <Compile Include="Controllers\WebView2CoreInitializerTests.cs" />
    <Compile Include="Controllers\QfcQueueTests.cs" />
```

Note `Wp` precedes `We`, confirming the block is not alphabetical.

Guidance: prefer adding test methods to the existing
`QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` (already registered at `:159`, so no
csproj edit). If a separate file for the breadcrumb host is warranted — and it is, since that file's
namespace is `QuickFiler.Controllers.Tests` and its subject is the initializer — insert the new entry
immediately after line `:159` to keep the WebView2 entries contiguous and to minimise the conflict
surface. `QuickFiler.Test` is not owned by any sibling child in this epic, so the collision risk on
this ItemGroup is low compared with `QuickFiler/QuickFiler.csproj:391-413`.

### 6.2 (b) `WebView2BreadcrumbHost.cs` nullable status

**`#nullable enable` is present at `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:1`.** The file is
already conscripted into the nullable gate. Existing nullable annotations in the file:
`event EventHandler<string>? MessageReceived` (`:57`), `event EventHandler? CoreInitialized` (`:63`),
`CoreWebView2? core` (`:74`), `object? sender` (`:116`, `:138`),
`e.InitializationException?.Message` (`:123`).

Implication: **all new code in this file must be nullable-clean.** Under
`msbuild ... /p:TreatWarningsAsErrors=true` any `CS86xx` diagnostic is an error. Points of care:

- `CoreWebView2 core = _control.CoreWebView2;` at `:129` is an unannotated read of an SDK property
  from an un-annotated assembly, so it is currently `CS8600`-free only because the SDK type is
  null-oblivious. Do not change that line's shape casually.
- A new `BreadcrumbUiDispatcher?` field (under design V1, where the dispatcher is installed in
  `InitializeAsync`) must be declared nullable and null-checked at every use.
- `ConditionalWeakTable<WebView2, WebView2BreadcrumbHost>.TryGetValue` uses an `out` parameter; under
  `#nullable enable` against an un-annotated `mscorlib` this is null-oblivious, so no
  `[MaybeNullWhen]` gymnastics are needed, but the `out` variable should still be declared as
  `WebView2BreadcrumbHost? previous` and null-checked.

### 6.3 (c) `WebView2CoreInitializer.cs` and `IWebViewCoreInitializer.cs` nullable status

**Neither file is nullable-enabled.** Verified by reading both files in full:

- `QuickFiler/Viewers/WebView2CoreInitializer.cs:1` is `using System.Diagnostics.CodeAnalysis;`.
  There is no `#nullable` directive anywhere in the file (31 lines total).
- `QuickFiler/Viewers/IWebViewCoreInitializer.cs:1` is `using System.Threading.Tasks;`. There is no
  `#nullable` directive anywhere in the file (31 lines total).

There is no `Directory.Build.props` and no `<Nullable>` element in `QuickFiler/QuickFiler.csproj`, so
nullable participation in this repository is strictly per-file opt-in, exactly as `CLAUDE.md` §C#3
states.

**Consequence, and a concrete instruction for the executor: do NOT add `#nullable enable` to either
file.** Adding the directive conscripts the file into the `TreatWarningsAsErrors` gate. That is a
gratuitous risk for a bugfix whose entire production change to these two files is (i) two argument
guards and (ii) XML documentation. If a guard needs to express nullability, express it with a runtime
`ArgumentNullException`, not with an annotation.

### 6.4 (d) Test-project references and MSTest style

| Item | Evidence |
| --- | --- |
| **FluentAssertions 8.10.0** | `QuickFiler.Test/QuickFiler.Test.csproj:194-196` (`lib\net47\FluentAssertions.dll`) |
| **Moq 4.20.72** | `QuickFiler.Test/QuickFiler.Test.csproj:309-311` (`lib\net462\Moq.dll`) |
| **MSTest.TestFramework 4.3.3** | `QuickFiler.Test/QuickFiler.Test.csproj:312-317` (+ `MSTest.TestAdapter` props/targets at `:4`, `:463`) |
| **WebView2 SDK 1.0.4129.50** | `QuickFiler.Test/QuickFiler.Test.csproj:285-290` — **both** `Microsoft.Web.WebView2.Core` **and** `Microsoft.Web.WebView2.WinForms` are referenced by the test project, so a test can construct `Microsoft.Web.WebView2.WinForms.WebView2` directly |
| `System.Windows.Forms` | `QuickFiler.Test/QuickFiler.Test.csproj:412` |
| Target framework | `QuickFiler.Test/QuickFiler.Test.csproj:18` — `v4.8.1` |
| `InternalsVisibleTo` | `QuickFiler/Properties/AssemblyInfo.cs:5` — `[assembly: InternalsVisibleTo("QuickFiler.Test")]` |

**MSTest attribute style in use:** `[TestClass]` / `[TestMethod]` from
`Microsoft.VisualStudio.TestTools.UnitTesting` (for example
`QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs:13, :16` and
`QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:13, :16`), with `[Timeout(PumpTimeoutMs)]` on
pump-hosted tests (`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs:355`).
Assertions use FluentAssertions `.Should()` chains with an explicit `because:` argument on
non-obvious cases (`Part3.cs:376-385`). Tests are structured with explicit
`// Arrange` / `// Act` / `// Assert` comments (`WebView2CoreInitializerTests.cs` is the compact
exception; `NoLiveFormInTestAssemblyTests.cs:19, :23, :30` is the canonical form).

Analyzers active on the test project that constrain test code:
`MSTest.Analyzers` (`:437-438`), `SonarAnalyzer.CSharp` (`:439`), `Meziantou.Analyzer` (`:466`),
`Roslynator` (`:467-470`), `AsyncFixer` (`:471`), and `Microsoft.CodeAnalysis.BannedApiAnalyzers`
with `BannedSymbols.txt` (`:472-474`).

---

## 7. Recommended remediation design, per defect

### 7.1 #458 — predecessor unhook

**Design.** A `static ConditionalWeakTable<WebView2, WebView2BreadcrumbHost>` owner registry plus a
`static readonly object` gate, both private to `WebView2BreadcrumbHost.cs`. In the constructor, under
the gate: look up the previous owner; if present, invoke its private `DetachCore()` (which performs
the `-=` for both `CoreWebView2InitializationCompleted` and, if it had subscribed,
`core.WebMessageReceived`); then replace the registry entry with `this`. Replace the dead `-=` at
`:49` with the registry lookup; keep the `+=` at `:50`. Add a `_control.Disposed` subscription that
detaches and removes the registry entry, as secondary hygiene.

**Why this and not `IDisposable`/`Detach()`.** §1.3 establishes exhaustively that no in-repo caller
exists or could be created without editing `EfcFormController.cs` (forbidden) or `EfcViewer.cs` (not
writable). A `Detach()` with no caller is not a fix.

**Files touched.** `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` only.

**Residual risk.** Process-wide static state; mitigated by keying on control identity and by using a
distinct control instance per test. The `MessageReceived` detach path in `DetachCore` must tolerate
`_control.CoreWebView2 == null` (the predecessor may never have completed initialization).

### 7.2 #476 defect 1 — UI marshalling

**Design.** Internal three-argument constructor overload
`(WebView2, IWebViewCoreInitializer, BreadcrumbUiDispatcher)`; the existing public two-argument
constructor chains to it, leaving `EfcFormController.cs:836-839` untouched. Route
`NavigateToString` (`:66-69`) and `PostMessageJson` (`:72-84`) — including the `_control.CoreWebView2`
read at `:74` — each through a **single** `_dispatcher.Dispatch(...)` callback, matching
`WebView2Messenger.PostJson` (`WebView2Messenger.cs:62-68`). Do not use `DispatchValue`.

**Capture point.** Prefer V1 (§2.4): install the dispatcher in `InitializeAsync` from the
`uiSyncContext` argument the host already receives at `:92`, using
`new BreadcrumbUiDispatcher(uiSyncContext, sink)` (`BreadcrumbUiDispatcher.cs:25-30`), with an
inline-execution fallback for the pre-initialization window. This avoids introducing a new throwing
precondition on the constructor and requires no edit to `BreadcrumbUiDispatcher.cs`.

**Files touched.** `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` only.

**Residual risk.** `Dispatch` is fire-and-forget from the caller's perspective (`_ = ...`), so
`PostMessageJson` becomes asynchronous where it was synchronous. Ordering between successive posts is
preserved by the single `SynchronizationContext.Post` queue, and `BreadcrumbOutboundQueue`'s flush
loop (`BreadcrumbOutboundQueue.cs:61-64`) enqueues in order, so payload order is preserved. This is a
genuine behaviour change and is the reason epic #136 excluded it.

### 7.3 #476 defect 2 — state publication

**Design.** Replace the auto-property at `WebView2BreadcrumbHost.cs:54` with an explicit backing
field and a `Volatile.Read` getter; replace the write at `:134` with `Volatile.Write`, keeping it
strictly after the `core.WebMessageReceived` subscription at `:131-132` and before
`CoreInitialized?.Invoke(...)` at `:135`. `System.Threading.Volatile` is available on `net481` and is
already used at `WebView2Messenger.cs:127` in the same project; `using System.Threading;` is already
present at `:5`.

**Files touched.** `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` only.

**Residual risk.** None to behaviour; the failing-first test is a structural proxy (§5.2.3), which
must be stated honestly in the plan's acceptance criteria.

### 7.4 #477 — contract

**Design.**

1. Add guards to `WebView2CoreInitializer` (`:19-28`): `ArgumentNullException` /
   `ArgumentException` on `cacheFolder`; `ArgumentNullException` on `control`. Do not guard
   `environment` (null is a valid SDK input meaning "default environment"). Guard `options` only if
   the SDK rejects null — see §9, item 4.
2. **Option B** for the hard-coded `browserExecutableFolder`: keep the two-argument signature and
   document the `null` as a deliberate Evergreen-only decision in the interface XML doc at
   `IWebViewCoreInitializer.cs:15-22`.
3. Correct the "1:1 forward" wording at `IWebViewCoreInitializer.cs:10-11` and the exemption
   rationale at `WebView2CoreInitializer.cs:8-14`, restating the exemption on the accurate ground
   (external Evergreen runtime plus user-data-folder creation on disk).

**Why Option B.** Option A requires editing `QfcItemController.ViewerSetup.cs:64-67`, which is
outside the declared writable production set, and its motivating requirement is recorded as
UNCONFIRMED in the potential document's own Next Step (`…-iwebviewcoreinitializer-contract-defects.md:121`).

**Files touched.** `QuickFiler/Viewers/WebView2CoreInitializer.cs` and
`QuickFiler/Viewers/IWebViewCoreInitializer.cs`. Neither gains `#nullable enable` (§6.3).

**Test impact.** Nil for the eight `MockBehavior.Strict` sites (§4.5). Guard tests go in the existing
`QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` (already registered at
`QuickFiler.Test.csproj:159`, so no csproj edit).

**Follow-up worth promoting to its own issue (out of scope here).**
`QuickFiler/Controllers/EfcItemController.cs:223-227` (and the earlier variant near `:186-192`) calls
`CoreWebView2Environment.CreateAsync(null, cacheFolder, options)` **directly**, bypassing the seam.
Any future fixed-version-distribution work must include that site, and it is not covered by fixing
`WebView2CoreInitializer`.

---

## 8. Rejected alternatives (brief)

- **#458 via `IDisposable`/`Detach()` alone.** Rejected: exhaustively verified that no caller exists
  or can be added without editing `EfcFormController.cs` (forbidden) or `EfcViewer.cs` (not writable)
  — §1.3.
- **#458 via `Control.Tag` as the owner slot.** Rejected without deep analysis: `Tag` is public,
  Designer-writable, and shared with any other consumer of the control; a `ConditionalWeakTable` keyed
  on the control has the same effect with no shared-slot hazard.
- **#476 defect 1 via `DispatchValue<T>` for the `CoreWebView2` property read.** Rejected:
  `DispatchValue` runs inline only from inside an executing `Dispatch` callback
  (`BreadcrumbUiDispatcher.cs:166`) and faults on the owner-thread-only test dispatcher
  (`:180-188`). A single `Dispatch` callback containing both the read and the call is both correct and
  the established precedent (`WebView2Messenger.cs:62-68`).
- **#476 defect 1 via changing the public constructor to take a dispatcher.** Rejected: would require
  editing `EfcFormController.cs:836-839`, which is forbidden. The internal-overload pattern achieves
  the same with no call-site change.
- **#476 defect 2 via publishing state through the dispatcher instead of `Volatile`.** Rejected: the
  two readers (`BreadcrumbOutboundQueue.cs:44`, `BreadcrumbBridgeRouter.cs:400`) call the property
  directly and synchronously from arbitrary threads, and neither file is being changed. A dispatcher
  cannot make a synchronous property read single-threaded.
- **#477 Option A (surface `browserExecutableFolder`).** Rejected: 5-file blast radius including
  `QfcItemController.ViewerSetup.cs:64-67` (outside the writable set), motivated by an explicitly
  unconfirmed requirement — §4.4.
- **#476 defect 1 with `CaptureCurrent()` at construction (V2).** Not rejected outright, but demoted
  behind V1: it introduces a new throwing precondition on the constructor
  (`BreadcrumbUiDispatcher.cs:46-50`) whose safety at `EfcFormController.cs:836` could not be
  verified — §2.4, §9 item 1.

---

## 9. Open questions (explicitly UNVERIFIED)

1. **Is `SynchronizationContext.Current` non-null on the thread that executes
   `EfcFormController.cs:836`?** Not established. The defensive guards at `EfcFormController.cs:451-452`
   and `:704-705`, and `KeyboardHandler.cs:240-241, :268`, prove that a null ambient context occurs on
   *some* entry paths, but the specific construction path
   (`EfcHomeControllerDependencyFactories.cs:80/92/120/124` → `Initialize()`/`InitializeWithoutData()`
   → `WireEventHandlers()` → `ConfigureBreadcrumbControl()`) was not traced to a thread. This is the
   sole reason V1 is preferred over V2 in §2.4. **An executor that verifies this may adopt V2.**
   Relatedly, the off-UI-thread reader argument in §3.2 is a static reachability argument, not a
   runtime observation.
2. **How is `WebView2.CoreWebView2InitializationCompleted` implemented by the SDK** (field-like
   backing delegate vs. WinForms `EventHandlerList`)? Not determined; the SDK assembly was not
   decompiled. This affects only whether a reflection-based handler-count assertion is feasible for
   the #458 test, which §5.2.2 already recommends against as a primary assertion.
3. **Is `ConditionalWeakTable<TKey,TValue>.AddOrUpdate` present on `net481`?** Not verified.
   `TryGetValue`, `Add`, `Remove`, and `GetValue` are unambiguously present since .NET Framework 4.0.
   The recommendation in §1.4 uses only the latter set, so this does not block the design.
4. **Does `CoreWebView2Environment.CreateAsync` accept a null `options` argument?** Not verified.
   If it does, guarding `options` in `WebView2CoreInitializer.CreateEnvironmentAsync` would be a
   behaviour narrowing rather than a diagnostic improvement. Note that both in-repo callers always
   supply a non-null `options` (`WebView2BreadcrumbHost.cs:103`,
   `QfcItemController.ViewerSetup.cs:55`), so a guard is safe in practice; the question is only
   whether it is *correct* as a contract. Recommend guarding `cacheFolder` and `control` for certain,
   and deciding on `options` at implementation time.
5. **Whether a bare `new Microsoft.Web.WebView2.WinForms.WebView2()` (no Designer
   `ISupportInitialize.BeginInit`/`EndInit`) constructs cleanly off the pump.** Not verified. The
   measured evidence at `QfcItemController.InitializationTests.Part3.cs:344-353` covers only the
   Designer-driven path. **Recommendation: construct it on `WinFormsPumpHost` via
   `host.InvokeAsync(() => new WebView2())`, which is safe regardless; fall back to
   `FormatterServices.GetUninitializedObject(typeof(WebView2))` (precedent:
   `BreadcrumbPopupBoundaryCoverageTests.cs:200`) only if the direct construction proves problematic,
   noting that an uninitialized instance may have a null WinForms `Events` list and could therefore
   fail on event subscription.**
6. **Whether any sibling epic child will add a new file to
   `QuickFiler/QuickFiler.csproj:391-413`.** Unknown from this worktree. §5.2.4 records the conflict
   surface; the recommended design avoids the ItemGroup entirely.

---

## 10. Summary table

| Defect | Fix | Files touched | Inside writable set? | Failing-first test |
| --- | --- | --- | --- | --- |
| #458 | `ConditionalWeakTable` owner registry + predecessor detach; `Disposed` self-detach | `WebView2BreadcrumbHost.cs` | Yes | Yes — two hosts, one control, assert predecessor detached |
| #476-1 | Internal 3-arg ctor + route `NavigateToString`/`PostMessageJson`/`CoreWebView2` read through one `BreadcrumbUiDispatcher.Dispatch` callback; capture from `InitializeAsync`'s `uiSyncContext` | `WebView2BreadcrumbHost.cs` | Yes | Yes — recording `SynchronizationContext`, assert one `Post` |
| #476-2 | Explicit backing field + `Volatile.Read`/`Volatile.Write`, write kept after the subscription | `WebView2BreadcrumbHost.cs` | Yes | Structural proxy only (documented) |
| #477-1 | Option B: document the Evergreen-only `null`; correct the "1:1 forward" wording | `IWebViewCoreInitializer.cs`, `WebView2CoreInitializer.cs` | Yes | Doc-only |
| #477-2 | `ArgumentNullException`/`ArgumentException` guards on the concrete class | `WebView2CoreInitializer.cs` | Yes | Yes — direct; nil impact on the 8 strict mocks |

No forbidden file is touched by any recommendation. No new production file is required, so
`QuickFiler/QuickFiler.csproj` needs no edit. `QuickFiler.Test/QuickFiler.Test.csproj` needs an edit
only if a new test file is added; the existing
`QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` (registered at `:159`) can absorb the
#477 guard tests without one.
