# itemviewer-breadcrumb-lifecycle-defects (#488, #475) — Research

- **Feature:** `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488`
- **Issues closed:** #488 (primary), #475
- **Epic:** `quickfiler-bug-family`, wave 3
- **Author:** task-researcher
- **Date:** 2026-08-25
- **Worktree HEAD at time of research:** `988e819b` (merge of PR #604); all citations below re-verified
  against the working-tree contents of that checkout.

---

## 0. Method, and one unmet input

### 0.1 The #488 correction comment could not be retrieved

The delegation prompt directed me to read the correction comment on issue #488 via
`gh issue view 488 --json comments`. **The Bash tool is disabled in this session**, so `gh` could not
be invoked. Two fallbacks were attempted:

1. A repository search for a cached copy of the issue body or comments. The feature folder contains
   `issue.md` (a locally authored feature record, not the GitHub body) and a `spec.md` that is still
   the unmodified template. No cached comment exists anywhere under `docs/`.
2. An unauthenticated `WebFetch` of `https://github.com/drmoisan/TaskMaster/issues/488`. It returned
   a summary of the issue body and reported "There are no comments on this issue." That result is
   **not trustworthy** for this purpose: it is an anonymous render summarised by a small model, and
   it contradicts the orchestrator's first-hand statement that a correction comment exists.

**How this research proceeds.** The orchestrator relayed the comment's three substantive claims in
the delegation prompt. I treated those relayed claims as hypotheses and verified each one directly
against the current source. Every finding in §2 and §3 below is grounded in file evidence I read in
this session, not in the comment's text. Where my reading of the source refines or contradicts the
relayed claim, I say so explicitly.

**Action for the planner:** before the spec is finalised, one person with `gh` access should read the
comment verbatim and diff it against §2 and §3. This is the only unverified input in this document.

### 0.2 Line-citation discipline

The potential entries were written on 2026-08-07 and several of their line citations have since
drifted. Every citation in this document was re-derived from the current file contents. §1.1 records
the corrections so a reader of the potential entry is not misled.

---

## 1. Current-state map

### 1.1 Corrections to the potential entries' line citations

| Claim | Potential entry says | Current truth |
| --- | --- | --- |
| `ConfigureBreadcrumbDropDown` idempotence guard | `ItemViewer.Breadcrumb.cs:147-153` | `:147-153` — **unchanged** |
| Second-host construction | `:158-168` (orchestrator) | `:158-168` — **unchanged** |
| `InitializeBreadcrumbPipeline` guard | `:45-48` | `:45-48` — **unchanged** |
| Coordinator write | `:59` | `:59` — **unchanged** |
| `SetBreadcrumbTheme` | `:197-198` | `:197-198` — **unchanged** |
| `EnsureBreadcrumbResourceOwnership` container creation | `:286-288` | **`:300-310`**, container at **`:307`**, `Add` at **`:309`** |
| `DisposeBreadcrumbResources` | `:291-296` | **`:312-317`** |
| `ReleaseHostCore` | `BreadcrumbItemViewerLifecycleCoordinator.cs:127-142` | **`:292-304`**; `coordinator.Release()` at **`:302`** |
| `SetBridgeCoordinator` reference compare | `:66-69` | **`:62-77`**, compare at **`:66-69`** — unchanged |
| `SetTheme` | `:155-160` | `:155-160` — unchanged |
| `ConfigureHost` | `:120-152` | `:108-153`; post opens at **`:120`**, generation guard at **`:122-125`** |
| `IBreadcrumbDropDownHost : IDisposable` | `IBreadcrumbDropDownHost.cs:19` | `:19` — unchanged |
| `BreadcrumbUiDispatcher.CaptureCurrent` | `BreadcrumbUiDispatcher.cs:43-54` | **`:44-56`** |
| `CreateForCurrentThreadTests` doc comment | `:58-60` | **`:58-61`**, method at **`:62-65`** |
| `CaptureCurrentOrTests` | `BreadcrumbPopupUiOperations.cs:86-89` | `:86-89` — unchanged |
| Injectable ctor | `BreadcrumbPopupUiOperations.cs:62-78` | `:62-78` — **unchanged, confirmed** |
| Controller pipeline guard | `QfcItemController.ViewerSetup.cs:140-146` | **`:143-149`** (method at `:136-161`) |
| Controller configure + theme | `:166-167` | **`:169-170`** (method at `:164-171`) |
| Pooled-reuse reset | `:396` | **`:400`** (`Cleanup()` opens at `:396`) |
| `ItemViewer.Designer.cs Dispose(bool)` | `:16-23` | `:16-23` — **confirmed exactly** |
| Test asserting one host disposal | `BreadcrumbDropDownIntegrationTests.cs:308` | `:308` inside the test at **`:296-312`** — confirmed |

### 1.2 Ownership and control flow

Production entry points into the breadcrumb pipeline, exhaustively (grep of `QuickFiler/` for
`InitializeBreadcrumbPipeline|ConfigureBreadcrumbDropDown|EnsureBreadcrumbPipeline|SetBreadcrumbTheme`):

- `QfcItemController.ViewerSetup.cs:110` — `EnsureBreadcrumbPipeline()` inside
  `InitializeWebViewAsync` (`:42`), after `await _itemViewer.UiSyncContext` (`:58`).
- `QfcItemController.FolderHandling.cs:176` — `EnsureBreadcrumbPipeline()` inside
  `AssignFolderComboBox`, which marshals through `_itemViewer.Invoke` first (`:164-168`).
- `QfcItemController.ViewerSetup.cs:169-170` — `viewer.ConfigureBreadcrumbDropDown(environment, _webViewInitializer)`
  then `viewer.SetBreadcrumbTheme(...)`, reached from `ConfigureAndAttachBreadcrumbAsync` (`:182`),
  reached from `InitializeWebViewAsync` (`:116`).
- `QfcItemController.ViewerSetup.cs:400` — `ResetBreadcrumb()` in `Cleanup()`.

**Both entry points are UI-thread-bound today.** There is no `ConfigureAwait(false)` anywhere on
`InitializeWebViewAsync`'s path (verified by grep of `QfcItemController.ViewerSetup.cs`), so the
continuation after `await _itemViewer.UiSyncContext` resumes on the WinForms synchronization context
and `SynchronizationContext.Current` is non-null and reference-equal to `ItemViewer.UiSyncContext`
for the whole sequence.

The dispatcher's inline rule (`BreadcrumbUiDispatcher.cs:255-278`) is load-bearing for every
conclusion below: `Dispatch`/`DispatchValue` run **inline** when the caller is already inside a
callback of the same dispatcher (`:258-261`) or when `SynchronizationContext.Current` is
reference-equal to the captured context (`:269-272`). On the production UI thread both hold, so
`PostAsync` is synchronous in production.

### 1.3 `ItemViewer` is coverage-exempt

`QuickFiler/Viewers/ItemViewer.cs:20` carries `[ExcludeFromCodeCoverage]` on the `ItemViewer` partial
type declaration. A type-level attribute on one part applies to the whole partial type, so **every
member in `ItemViewer.Breadcrumb.cs` is already excluded from coverage measurement**. Consequence for
this feature: the regression tests are required by the Bugfix Workflow and by AC, but they will move
no coverage number for `ItemViewer.Breadcrumb.cs`. Fixes placed in
`BreadcrumbItemViewerLifecycleCoordinator.cs` / `BreadcrumbPopupUiOperations.cs` /
`BreadcrumbDropDownHost.cs` *are* measured.

---

## 2. Confirmed root cause per defect

Six defect units. For each: mechanism, verified evidence, and production reachability.

### D1 — host replacement on WebView2 environment change

**Reconciliation with the correction comment (delegation item 2).** The relayed claim was that
`BreadcrumbDropDownOpenCoordinator.Release()` *does* dispose the host, asynchronously via a discarded
`PostAsync` lambda, and skips entirely when `Invalidate(release: true)` returns false. **Verified, in
both parts, and refined:**

```csharp
// QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:183-192
internal void Release()
{
    if (!Invalidate(release: true))
        return;
    _ = _operations.PostAsync(() =>
    {
        _detachPopupMessenger();
        _host.Dispose();
    });
}
```

- `_host.Dispose()` is present at `:190`. **The original potential's claim that the host "is never
  disposed by this file" is false as of current HEAD** and must not be restated.
- The `PostAsync` result is discarded (`_ =` at `:187`), so no caller can observe a dispose failure.
- `Invalidate` (`:321-333`) returns `false` only when `_released` is already `true`, and `_released`
  is set only by `Release()` itself (`Reset()` calls `Invalidate(release: false)` and is gated by the
  same `if (_released) return false;` at `:325-326`). **The skip is therefore reachable only on a
  second `Release()`, after the first has already enqueued a dispose.** It is not, by itself, a
  leak. I would not carry the comment's "skips entirely" framing into the spec as a defect; it is
  accurate as a description of the code and inaccurate as a description of a leak.

**Residual defect, precisely.** Three separable facts remain:

**(D1a) Disposal is not ordered against the replacement's construction.** `ItemViewer.Breadcrumb.cs`
constructs the replacement host **synchronously at `:159`**, before any release is even scheduled:

```
:147-153  early-return when the existing host's Environment is reference-equal
:158-168  host = new BreadcrumbDropDownHost(_l0vhBreadcrumb_WebView2, environment, ...)   <-- replacement exists here
:169-176  ConfigureBreadcrumbDropDown(host, anchorBounds, workingArea)
             -> lifecycle.ConfigureHost(host, ...)                       (coordinator :108-153)
                -> _operations.PostAsync(lambda)                          (coordinator :120)
                   -> ReleaseHostCore()                                   (coordinator :129 -> :292-304)
                      -> coordinator.Release()                            (coordinator :302)
                         -> _operations.PostAsync(dispose lambda)         (open-coordinator :187)
                   -> _openCoordinator = new BreadcrumbDropDownOpenCoordinator(host2)   (coordinator :130)
```

On the production UI thread every `PostAsync` runs inline, so the outgoing host is in fact disposed
before `:130` executes and the observable ordering is currently correct. Off the captured boundary —
which `BreadcrumbUiDispatcher.cs:263-268` documents as a real scenario after a
`ConfigureAwait(false)` resumption — the outer lambda is genuinely posted and the inner dispose
lambda is enqueued **behind** it, so `host1.Dispose()` runs *after* `_openCoordinator` has adopted
`host2`. Both hosts then coexist over the same `_l0vhBreadcrumb_WebView2` anchor.

The cross-talk is concrete, not theoretical. Both hosts capture the *same* two shared callbacks from
`ItemViewer.Breadcrumb.cs`: `FocusBreadcrumbCore` (`:165`) and `() => BreadcrumbCoordinator?.CancelSelector()`
(`:166`), and the second resolves the **current** bridge coordinator at invocation time. If the
outgoing host is open at replacement time, `DisposeCoreAsync` (`BreadcrumbDropDownHost.cs:300-323`)
takes the `if (OpenState && !_resetPending)` branch at `:303`, calls `CompleteClose` → `FinishClose`
(`:410-420`), which invokes `_cancelSelection()` and `_focusAnchor` — cancelling the *new* host's
live selector session and pulling focus back to the anchor. If the outgoing host is closed, no
cross-talk occurs; the residual is then only the unobserved-failure item below.

**(D1b) The dispose failure is unobservable.** `_ = _operations.PostAsync(...)` discards the task, and
`BreadcrumbUiDispatcher.Dispatch` catches and routes any exception to the error sink
(`BreadcrumbUiDispatcher.cs:86-89`, sink defaults to `LogFailure` at `:280-283`). A host that fails to
dispose leaves a WebView2-backed `ToolStripDropDown` alive with only a log line.

**(D1c) A generation change between the schedule and the run leaks the *new* host.** `ConfigureHost`
captures `int generation = _generation` at `:119` and returns early at `:122-125` if
`IsCurrent(generation)` is false. `Reset()` increments `_generation` at `:194`; `Dispose()` at `:209`.
Production calls `ResetBreadcrumb()` from `QfcItemController.Cleanup()` (`ViewerSetup.cs:400`). So an
off-boundary `ConfigureBreadcrumbDropDown` followed by a `Cleanup()` drops the host constructed at
`ItemViewer.Breadcrumb.cs:159` on the floor: it is never adopted and never disposed. **This is an
unconditional leak and it is a distinct defect from the one #488 filed** (it leaks the incoming host,
not the outgoing one). Scope recommendation in §3.6.

**Production reachability.** `_webViewEnvironment` is created fresh per controller initialization
(`ViewerSetup.cs:64`) and cleared in `Cleanup()` (`:413`), while `ItemViewer` instances are pooled and
reused. `ReferenceEquals(existing.Environment, environment)` at `ItemViewer.Breadcrumb.cs:149` is
therefore false on every reuse, so the replacement path at `:158-168` is taken on every reuse.
**However**, the path is taken from the UI thread today, so D1a's harmful ordering is **latent, not
live**. D1b and D1c are live on any path that reaches the boundary check off-thread; no such path
exists in `QuickFiler` today. Severity should be recorded as *latent, high-consequence*, not *live*.

### D2 — `SetBreadcrumbTheme` lost when issued off the UI thread

`ItemViewer.Breadcrumb.cs:197-198` forwards synchronously to
`BreadcrumbItemViewerLifecycleCoordinator.SetTheme` (`:155-160`):

```csharp
internal void SetTheme(string theme)
{
    ThrowIfDisposed();
    _bridgeCoordinator?.SetTheme(theme);   // :158 — bridge is assigned synchronously, always works
    DropDownHost?.SetTheme(theme);         // :159 — DropDownHost => _openCoordinator?.Host  (:53)
}
```

`_openCoordinator` is assigned **only inside the `ConfigureHost` post** (`:130`). When that post is
genuinely deferred, `DropDownHost` is `null` at `:159`, the null-conditional swallows the call, and
the popup surface keeps its previous theme with nothing surfaced. There is no retained theme and no
replay: the coordinator has no theme field.

Confirmed exactly as the potential describes. The bridge (collapsed) surface still receives the theme
because `BreadcrumbBridgeCoordinator.SetTheme` (`BreadcrumbBridgeCoordinator.cs:230-234`) posts
through its own dispatcher, so the visible symptom is a **split theme**: collapsed breadcrumb dark,
popup light (or vice versa) — the same family as #254 / #269.

**Production reachability: latent.** `QfcItemController.ViewerSetup.cs:169-170` issues
`ConfigureBreadcrumbDropDown` then `SetBreadcrumbTheme` back to back on the UI thread, where the post
runs inline, so ordering holds today.

### D3 — a second, different `IFolderHierarchyProvider` is silently discarded

`ItemViewer.Breadcrumb.cs:45-48` returns as soon as `BreadcrumbCoordinator != null`, without
comparing providers. `BreadcrumbItemViewerLifecycleCoordinator.SetBridgeCoordinator` (`:66-69`) *does*
compare by reference before short-circuiting, so the inner coordinator is stricter than its own
wrapper. Both statements verified.

**Production reachability: NOT reachable — and this materially revises the potential entry.** The
potential asserts "Pooled viewer reuse reaches this path". It does not. `EnsureBreadcrumbPipeline`
guards the call:

```csharp
// QfcItemController.ViewerSetup.cs:143-149
if (viewer.BreadcrumbCoordinator == null)
{
    var provider = new UtilitiesCS.OutlookObjects.Folder.OutlookFolderHierarchyProvider(
        _globals.Ol.FolderTreeService
    );
    viewer.InitializeBreadcrumbPipeline(provider);
}
```

`BreadcrumbCoordinator` is nulled only by `DisposeBreadcrumbResources` (`ItemViewer.Breadcrumb.cs:316`),
which runs only on component disposal; `ResetBreadcrumb()` does not clear it. So on a pooled reuse the
second controller **never calls `InitializeBreadcrumbPipeline` at all**, and the guard at `:45` is
never reached with a different provider.

The *symptom* the potential describes is real — a viewer reused across two controllers with different
`_globals.Ol.FolderTreeService` instances keeps the first controller's provider — but it is produced
**upstream, at `ViewerSetup.cs:143`**, not at `ItemViewer.Breadcrumb.cs:45`. `ViewerSetup.cs` is owned
by feature 484 for #481/#484/#485 and is out of scope here. Fixing `:45` is therefore a **latent-defect
/ defence-in-depth fix that changes no production behaviour**, and the spec should say so plainly
rather than claim a user-visible repair. The second production caller
(`QfcItemController.FolderHandling.cs:176`) is behind the same guard and does not change this.

### D4 — non-atomic read-then-write on pipeline initialization

Three read-then-write pairs with no synchronization and no memory barrier, all verified:

| Pair | Read | Write |
| --- | --- | --- |
| Bridge coordinator | `ItemViewer.Breadcrumb.cs:45` | `:59` |
| Drop-down host | `:147-148` | `:159` (construction) / coordinator `:130` (adoption) |
| Lifecycle coordinator | `:278` | `:289` |
| Resource owner | `:302` | `:308` |

Two threads entering `InitializeBreadcrumbPipeline` concurrently both construct a
`BreadcrumbItemViewerLifecycleCoordinator` (with its own `BreadcrumbMessengerHub`, `:284`) and a
`BreadcrumbBridgeCoordinator` (`:53-57`). One pair is overwritten at `:59`/`:289` and is never
disposed, leaking the hub and the bridge's `MessageReceived` subscription
(`BreadcrumbBridgeCoordinator.cs:163-172` shows disposal is the only path that detaches it).

**Production reachability: not reachable today, and nothing declares the constraint.** Both
production callers are UI-thread-bound (§1.2), but neither `ItemViewer.Breadcrumb.cs` nor
`IItemViewer` states a thread-affinity requirement, and `AttachBreadcrumbWebViewAsync` (`:62-75`) is
async-facing, which invites off-thread callers. This is a contract gap, correctly filed.

### D5 — a `Container` created during teardown is never disposed

```csharp
// ItemViewer.Breadcrumb.cs:300-310
private void EnsureBreadcrumbResourceOwnership()
{
    if (_breadcrumbResourceOwner != null) { return; }
    components ??= new Container();                                   // :307
    _breadcrumbResourceOwner = new BreadcrumbResourceOwner(DisposeBreadcrumbResources);
    components.Add(_breadcrumbResourceOwner);                         // :309
}
```

```csharp
// ItemViewer.Designer.cs:16-23  (confirmed verbatim)
protected override void Dispose(bool disposing)
{
    if (disposing && (components != null))
    {
        components.Dispose();
    }
    base.Dispose(disposing);
}
```

`Dispose(bool)` disposes `components` only if it is non-null **at the moment it runs**, and
`Control.Dispose` does not run twice. A `Container` created after that point is never disposed, so
`BreadcrumbResourceOwner.Dispose` (`BreadcrumbMessengerHub.cs:445-454`) never fires,
`DisposeBreadcrumbResources` (`ItemViewer.Breadcrumb.cs:312-317`) never runs, and the hub, the
lifecycle coordinator and the bridge coordinator all leak.

**Correction to the potential's reachability mechanism.** The potential says this is "reachable via
the deferred `ConfigureHost` post ... racing `Control.Dispose`". **That is wrong.**
`BreadcrumbItemViewerLifecycleCoordinator.ConfigureHost`'s posted lambda (`:120-152`) does not call
back into `ItemViewer.EnsureBreadcrumbResourceOwnership`; it only constructs a
`BreadcrumbDropDownOpenCoordinator` and attaches a messenger. `EnsureBreadcrumbResourceOwnership` is
reached only synchronously, from `EnsureBreadcrumbLifecycle` (`:283`), which is reached from
`InitializeBreadcrumbPipeline` (`:50`) and both `ConfigureBreadcrumbDropDown` overloads (`:155`,
`:191`).

**The correct reachability story.** No path in `ItemViewer.Breadcrumb.cs` guards on `IsDisposed`
except `FocusBreadcrumbCore` (`:213-217`). `QfcItemController.InitializeWebViewAsync` is an
`async Task` (`ViewerSetup.cs:42`) that awaits three times (`:58`, `:64`, `:68`, `:111`) before
reaching `EnsureBreadcrumbPipeline()` at `:110` and `ConfigureBreadcrumbDropDown` at `:169`. If the
pooled viewer is disposed while that initialization is in flight — QuickFiler form teardown during
WebView2 environment creation — the continuation runs against a disposed `ItemViewer`, creates a
fresh `Container`, and leaks everything hung off it. This is a plausible live path and should be the
reachability claim in the spec, replacing the potential's incorrect one.

### #475 — `CaptureCurrentOrTests()` inverts the fail-fast guard

Verified verbatim:

```csharp
// QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:80-89
internal static BreadcrumbPopupUiOperations CaptureCurrent() =>
    new BreadcrumbPopupUiOperations(BreadcrumbUiDispatcher.CaptureCurrent());

internal static BreadcrumbPopupUiOperations CreateForCurrentThreadTests() =>
    new BreadcrumbPopupUiOperations(BreadcrumbUiDispatcher.CreateForCurrentThreadTests());

internal static BreadcrumbPopupUiOperations CaptureCurrentOrTests() =>
    SynchronizationContext.Current == null
        ? CreateForCurrentThreadTests()
        : CaptureCurrent();
```

`BreadcrumbUiDispatcher.CaptureCurrent` (`:44-56`) throws `InvalidOperationException` on a null
context; `CreateForCurrentThreadTests` (`:62-65`) constructs a dispatcher with `_context = null` and
`_ownerThreadId = Environment.CurrentManagedThreadId`.

**Refinement of the failure mode.** The potential says the popup "silently never opens". The precise
behaviour is narrower and worth stating correctly, because it determines what a regression test can
assert. A null-context dispatcher still runs work **inline on its owner thread** — `IsCurrentBoundary`
falls through to the thread-identity comparison at `:276-277`. It fails only when work must cross
threads. The drop-down open path crosses: `BreadcrumbDropDownOpenCoordinator.OpenCoreAsync`
(`:194-211`) awaits with `.ConfigureAwait(false)` three times, so the continuation resumes on a
thread-pool thread; `DispatchValue` then hits `_context == null` at `:180-188` and returns
`Task.FromException<T>` with the "owner-thread-only test dispatcher cannot marshal cross-thread UI
work" message, routed to the error sink. `OpenCoreAsync`'s `catch` (`:207-210`) converts that to
`RollbackAsync`, which returns `false`. So: the open resolves `false`, the selector is cancelled, no
popup appears, no exception escapes, one log line. The potential's user-visible description is right;
the mechanism above is what a test would drive.

**The four call sites, with production reachability re-derived (delegation item 6, and the
potential's own open question).**

| # | Site | Overload | Production caller? |
| --- | --- | --- | --- |
| 1 | `ItemViewer.Breadcrumb.cs:156` | argument to `EnsureBreadcrumbLifecycle` inside `ConfigureBreadcrumbDropDown(CoreWebView2Environment, IWebViewCoreInitializer)` | **Yes** — `QfcItemController.ViewerSetup.cs:169` |
| 2 | `ItemViewer.Breadcrumb.cs:192` | argument to `EnsureBreadcrumbLifecycle` inside `ConfigureBreadcrumbDropDown(IBreadcrumbDropDownHost, ...)` | Indirectly — reached from site 1 via `:169-176`; the 3-arg overload itself has no direct production caller |
| 3 | `BreadcrumbDropDownHost.cs:98` | `public` 7-param `LegacySurfaceFactory` ctor (`:79-99`) | **No in-repo production caller** |
| 4 | `BreadcrumbDropDownHost.cs:118` | `internal` 7-param `ReadySurfaceFactory` ctor (`:101-119`) | **No in-repo production caller** |

Production constructs the host at `ItemViewer.Breadcrumb.cs:159` through the **8-param internal ctor**
(`BreadcrumbDropDownHost.cs:57-76`), which receives `lifecycle.Operations` explicitly (`:167`) and
chains to `:121-141` → `:143-173`. Sites 3 and 4 are reached only by tests and by the public API
surface. A repo-wide grep for `new BreadcrumbDropDownHost(` returns exactly eleven test sites plus
`ItemViewer.Breadcrumb.cs:159`.

**Answer to the potential's unconfirmed Next Step — "Confirm whether any production path legitimately
runs without a synchronization context": NO.**

1. Sites 3 and 4 have no production caller at all.
2. Sites 1 and 2 are reached only from `QfcItemController.InitializeWebViewAsync`, which resumes on
   the WinForms context at `ViewerSetup.cs:58` and uses no `ConfigureAwait(false)` anywhere on the
   path (verified by grep of the whole file). `SynchronizationContext.Current` is therefore non-null
   at both sites in every production invocation.

Consequence: #475 is a **latent** defect on production paths and a **live** design defect (a
test-only affordance selected at runtime by probing ambient state, reachable from a `public`
constructor). Restoring fail-fast carries no known production regression. Severity "High" in the
potential is defensible on consequence but should be annotated *latent* in the spec.

---

## 3. Minimal-fix design per defect

All designs honour the Bugfix Workflow: smallest targeted change, no opportunistic refactor. **File
ownership is a hard constraint** — see §7.

### 3.1 D1 — what disposes the outgoing host, and where

**Rejected alternatives (brief).**

- *Make `BreadcrumbDropDownOpenCoordinator.Release()` synchronous.* This is the cleanest fix: `Release()`
  has exactly one caller (`BreadcrumbItemViewerLifecycleCoordinator.cs:302`), and that caller already
  calls `DetachPopupMessenger()` synchronously at `:301`, so the posted `_detachPopupMessenger()` at
  `:189` is redundant and the whole lambda could collapse to two synchronous statements.
  `BreadcrumbDropDownHost.Dispose()` (`:258-265`) only sets `_disposed` and hands the real teardown to
  `_openLifetime.DisposeAndSchedule` (`BreadcrumbDropDownOpenLifetime.cs:138-139`), so calling it from
  any thread is safe. **Rejected on ownership, not on merit:**
  `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` is on sibling feature 501's *owned* list
  (`docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md:120`) for issue #462's
  `_closePending` split. Two concurrent branches editing that file is exactly the conflict the epic's
  file-assignment exists to prevent.
- *Dispose the outgoing host inside `ReleaseHostCore()`.* In our file, but it produces **two**
  `Dispose()` calls on the same host (ours plus `Release()`'s posted one). The concrete host is
  idempotent (`:260-261`), but `Mock<IBreadcrumbDropDownHost>` is not, and
  `BreadcrumbDropDownIntegrationTests.cs:296-312` asserts `host.Dispose()` `Times.Once()` on viewer
  disposal — which routes through `Dispose()` → `ReleaseHostCore()`. That test would go red for a
  reason unrelated to the defect. Rejected.

**Recommended.** Dispose the outgoing host in `ItemViewer.ConfigureBreadcrumbDropDown(CoreWebView2Environment, IWebViewCoreInitializer)`,
between the guard and the replacement's construction. The `is BreadcrumbDropDownHost existing`
pattern variable declared at `:148` is already in scope for the whole method body:

```csharp
// ItemViewer.Breadcrumb.cs, after the :147-153 guard, before :155
// Issue #488 D1: the outgoing host must be disposed before its replacement is constructed over the
// same anchor control. Relying on BreadcrumbDropDownOpenCoordinator.Release()'s posted dispose puts
// the teardown behind ConfigureHost's own post, so off the captured UI boundary the two hosts
// briefly share the anchor and the outgoing host's close callbacks reach the *new* bridge session.
if (BreadcrumbDropDownHost is BreadcrumbDropDownHost outgoing)
{
    outgoing.Dispose();
}
```

Why this is the right shape:

- **Ordering is guaranteed by statement order**, on every thread, with no dispatcher reasoning
  required. That is precisely what the delegation asked for.
- **Exactly one effective disposal.** `Release()`'s later posted `_host.Dispose()` hits the
  `if (_disposed) return;` early-return at `BreadcrumbDropDownHost.cs:260-261` and is a no-op. No mock
  is involved on this overload — it takes a `CoreWebView2Environment`, so the host is always the
  concrete `BreadcrumbDropDownHost` — so no `Times.Once()` assertion is disturbed.
- **Diff is 4 lines in one file this feature owns.**
- The same-environment early return at `:147-153` still fires first, so
  `ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost`
  (`QfcItemControllerBreadcrumbDropDownTests.cs:91-122`) is untouched.

Known limitation to record in the spec: this covers the concrete environment-change path only. The
3-arg injected overload (`:179-195`) can also replace a host, but the outgoing host is not knowable
there until inside the post, and that overload has no production caller. Recording the limitation is
better than widening the fix.

**D1b** (discarded task) is not separately fixed: it lives in 501's file. Record as a known residual.

**D1c** (generation-drop leak of the *incoming* host) — see §3.6.

### 3.2 D2 — order after host configuration, or record a deferred application

**Which is smaller.** Neither "ordering" option is actually available at low cost:

- *Reorder the caller.* `QfcItemController.ViewerSetup.cs:169-170` already calls configure-then-theme.
  The problem is not caller order; it is that host installation is posted while `SetTheme` is not.
  Nothing to reorder. Also `ViewerSetup.cs` is 484-owned.
- *Post `SetTheme` on the same queue and rely on FIFO.* One line, and it looks attractive: the
  `ConfigureHost` post and the `SetTheme` post would be FIFO on the same `SynchronizationContext`.
  **It does not actually fix the defect.** It only orders the case where both calls are issued from
  the same side of the boundary. If `ConfigureBreadcrumbDropDown` is issued off-boundary (posted) and
  `SetBreadcrumbTheme` is issued *on* the UI thread, `SetTheme` runs inline and still precedes the
  configure post. Rejected as incorrect, not merely weaker.

**Recommended: record a deferred application** — retain the last theme on the lifecycle coordinator
and replay it onto a host at the moment the host is adopted.

```csharp
// BreadcrumbItemViewerLifecycleCoordinator.cs
private string? _theme;                                    // new field, near :26

internal void SetTheme(string theme)                       // replaces :155-160
{
    ThrowIfDisposed();
    _theme = theme;                                        // retained for a host adopted later
    _bridgeCoordinator?.SetTheme(theme);
    DropDownHost?.SetTheme(theme);
}
```

and, inside `ConfigureHost`'s post, in the newly-adopted branch only (after `:141`):

```csharp
    if (!string.IsNullOrWhiteSpace(_theme))
    {
        host.SetTheme(_theme);   // BreadcrumbDropDownHost.SetTheme rejects null/whitespace (:243-244)
    }
```

- Diff: 1 field + 1 statement + 3 lines. Entirely inside a file this feature owns.
- **Deterministically testable** (see §4, D2): queue the `ConfigureHost` post, call `SetTheme`, then
  drain, and assert the host received the theme. Today the assertion fails because `DropDownHost` is
  null when `SetTheme` runs.
- The `else` branch (same host, `UpdateRequestProviders` at `:145`) deliberately does **not** replay:
  the host already holds the theme, and replaying there would add a redundant `SetTheme` call that
  `Mock<IBreadcrumbDropDownHost>`-based tests could observe.
- The guard on null/whitespace is required because `BreadcrumbDropDownHost.SetTheme` throws
  `ArgumentException` on it (`:243-244`).

**Highest-risk interaction, must be verified during implementation:**
`QfcItemControllerBreadcrumbDropDownTests.cs:187-262`
(`ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession`) asserts at
`:257-259` that **no stale pooled theme is replayed** after `ResetBreadcrumb()` + a dark→light flip.
The retained value is overwritten by `SetBreadcrumbTheme("light")` at `ViewerSetup.cs:170` before the
re-attach, so the replay should carry "light" and the test should stay green — but this is the single
assertion most likely to be perturbed and it must be re-run explicitly.

### 3.3 D3 — fail fast vs. explicit re-initialization

**Is matching `SetBridgeCoordinator`'s reference comparison sufficient? Yes — with one addition.**
`SetBridgeCoordinator` compares `BreadcrumbBridgeCoordinator` references (`:66-69`). The wrapper's
input is an `IFolderHierarchyProvider`, and `BreadcrumbBridgeCoordinator` **does not expose the
provider** (verified: the ctor at `BreadcrumbBridgeCoordinator.cs:45-53` passes it straight into
`FolderBreadcrumbBridgeRouter`; there is no `Provider` member). So the comparison needs one new
private field on `ItemViewer`.

**Recommended: fail fast.**

```csharp
// ItemViewer.Breadcrumb.cs
private IFolderHierarchyProvider _breadcrumbProvider;      // new field near :16

// replacing the :45-48 guard
if (BreadcrumbCoordinator != null)
{
    if (!ReferenceEquals(_breadcrumbProvider, provider))
    {
        throw new InvalidOperationException(
            "The breadcrumb pipeline is already initialized with a different folder hierarchy provider."
        );
    }
    return;
}
...
_breadcrumbProvider = provider;                            // alongside :59
```

- Reference equality is sufficient **to make the discard non-silent**, which is what the AC asks for
  ("either fails fast or re-initializes explicitly"). It matches the coordinator's own contract, so
  the wrapper stops being laxer than the thing it wraps.
- It is **not** sufficient to support re-initialization, and re-initialization should not be built:
  §2/D3 establishes that no production caller ever reaches the guard with a second provider, so a
  re-initialization branch would be unreachable code carrying real teardown risk (the existing
  coordinator, hub and bridge would have to be disposed and re-created mid-flight).
- `DisposeBreadcrumbResources` (`:312-317`) must also null `_breadcrumbProvider`, so a re-created
  pipeline after disposal is not blocked by a stale reference. That is a one-line addition.
- Spec wording note: this changes no production behaviour (§2/D3). State that explicitly so a reviewer
  does not expect a user-visible repair.

### 3.4 D4 — atomic initialization vs. declared-and-enforced UI-thread affinity

**Which the existing type shape supports with the smaller diff: declared-and-enforced affinity.**

- *Atomic initialization* would need `Interlocked.CompareExchange` (or a lock) on
  `_breadcrumbLifecycleCoordinator`, `BreadcrumbCoordinator`, and `_breadcrumbResourceOwner`, plus a
  disposal path for the loser of each race. That is three synchronized regions and new teardown code.
  It also **does not solve the underlying problem**: `ItemViewer` is a `UserControl`, `components` is
  WinForms state, and `_l0vhBreadcrumb_WebView2` is a `Control`. Making the *fields* atomic would
  legitimise off-thread access to control state that is not thread-safe at all. Rejected.
- *Declared-and-enforced affinity* costs one private helper and four call-sites, and it converts an
  undocumented assumption into an enforced contract — which is what §2/D4 identifies as the actual
  gap.

**Recommended.**

```csharp
// ItemViewer.Breadcrumb.cs
/// <summary>
/// Issue #488 D4: the breadcrumb pipeline is UI-thread-affine. The boundary proof is reference
/// equality against the context captured in the ItemViewer constructor, matching
/// BreadcrumbUiDispatcher.IsCurrentBoundary (BreadcrumbUiDispatcher.cs:269-272). Bare managed
/// thread identity is deliberately not used: a ConfigureAwait(false) continuation can land on a
/// recycled pool thread whose id matches.
/// </summary>
private void ThrowIfOffUiBoundary(string operation)
{
    if (UiSyncContext != null && !ReferenceEquals(SynchronizationContext.Current, UiSyncContext))
    {
        throw new InvalidOperationException(
            $"{operation} must be called on the ItemViewer's owning UI synchronization context."
        );
    }
}
```

Called first in `InitializeBreadcrumbPipeline(provider, operations)` (`:44`), both
`ConfigureBreadcrumbDropDown` overloads (`:146`, `:184`), and `EnsureBreadcrumbResourceOwnership`
(`:301`).

- Diff: ~10 lines plus four one-line calls, all in one owned file.
- `UiSyncContext` already exists (`ItemViewer.cs:59-63`, backed by `_context` assigned at `:26`), so
  no new state is captured. The `UiSyncContext != null` guard keeps a viewer constructed without an
  ambient context (a test shape) from throwing — see the verification task below.
- Explicit limitation for the spec: this **declares and enforces** the contract; it does not make the
  read-then-write atomic. A caller that violates the contract now gets a diagnostic instead of a
  silent leak. The AC permits exactly this ("Pipeline initialization is atomic, **or** UI-thread
  affinity is declared and enforced").

**Verification task the planner must schedule.** Every existing test that reaches these members must
construct its `ItemViewer` *after* installing its ambient context, or `_context` will be null (guard
passes) or a different instance (guard throws). Sites to check, in order:
`BreadcrumbDropDownIntegrationTests.cs:334-340` (context at `:337`, viewer at `:338` — OK),
`QfcItemControllerBreadcrumbDropDownTests.cs:369-373` (context `:372`, viewer `:373` — OK),
`BreadcrumbSelectorOpenRetryTests.cs:252-260` (context `:254`, viewer `:255` — OK),
`BreadcrumbSubfolderActivationTests.cs:271-306` (context `:274-276`, viewer `:305` — OK),
`BreadcrumbCollapsedSurfaceReadinessTests.cs:404-415`, `BreadcrumbPendingOpenCloseTests.cs:160-189`,
`BreadcrumbCoordinatorLifecycleTests.cs:26-34, :122`. The first four were read and confirmed; the last
three must be confirmed during implementation.

### 3.5 D5 — dispose a `Container` created during teardown, or refuse creation during teardown

**Recommended: refuse creation during teardown.**

```csharp
// ItemViewer.Breadcrumb.cs, first statement of EnsureBreadcrumbResourceOwnership (:300)
if (IsDisposed || Disposing)
{
    throw new ObjectDisposedException(nameof(ItemViewer));
}
```

- **Smaller and safer than the alternative.** Disposing a late-created `Container` would require
  either editing `ItemViewer.Designer.cs` — **6224 lines**, designer-generated, already far past the
  500-line ceiling, and a file sibling feature 489 may also touch (§7) — or adding a second disposal
  path with its own re-entrancy problem. The guard is three lines in a file this feature owns.
- `Control.IsDisposed` and `Control.Disposing` are both public WinForms properties, so no new state is
  needed. `Disposing` covers the window *during* `Dispose(bool)`, which `IsDisposed` alone does not.
- Fail-fast is the repository default (`CLAUDE.md` § "Error Handling", `.claude/rules/general-code-change.md`
  § "Error Handling and Logging"). A silent early-return would leave `BreadcrumbCoordinator` null and
  degrade `AttachBreadcrumbWebViewAsync` to a `false` return with no diagnostic — the same class of
  silent degradation #475 exists to remove.
- **Consequence to record and verify:** the throw propagates out of `EnsureBreadcrumbPipeline`
  (`ViewerSetup.cs:136-161`, which is `[ExcludeFromCodeCoverage]` at `:135`) and faults
  `InitializeWebViewAsync`'s task. The planner must confirm that fault is observed by the caller and
  does not become an unobserved `TaskException`. If it is not observed, the correct response is a new
  issue against `ViewerSetup.cs` (484-owned), not a weakening of this guard.

### 3.6 D1c — the incoming-host leak: recommend a follow-up issue

`ConfigureHost`'s generation guard (`BreadcrumbItemViewerLifecycleCoordinator.cs:122-125`) drops the
host constructed at `ItemViewer.Breadcrumb.cs:159` without disposing it when `_generation` advanced
between the schedule and the run. The fix is small (dispose `host` in the early-return branch), but:

- It leaks the **incoming** host, which is a different defect from the one #488 Defect 1 states.
- Adding a `host.Dispose()` in a branch no current test exercises would be an unpinned behaviour
  change inside a bugfix change-set.

**Recommendation: promote to a new issue** through the potential→issue lifecycle rather than absorbing
it. Record the mechanism and the two triggers (`Reset()` at `:194` via
`QfcItemController.Cleanup()` → `ViewerSetup.cs:400`; `Dispose()` at `:209`) in the new potential entry
so the follow-up does not have to re-derive it.

### 3.7 #475 — delete `CaptureCurrentOrTests()`, with one required adjustment

**The potential's preferred remedy is correct and is confirmed viable.** Three edits:

1. **Delete** `BreadcrumbPopupUiOperations.CaptureCurrentOrTests()` (`:86-89`). Keep
   `CreateForCurrentThreadTests()` (`:83-84`) and `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()`
   (`:62-65`) — seven tests call them directly and injecting a test dispatcher explicitly is exactly
   the discipline the potential asks for. Only the *ambient-probing selector* is removed.
2. **`BreadcrumbDropDownHost.cs:98` and `:118` → `CaptureCurrent()`.** No production caller reaches
   either (§2/#475), and every test that does installs an ambient context first (§5). One ordering
   subtlety to preserve: at `:91-93` the `surfaceFactory ?? throw` inside `NormalizeFactory` is
   evaluated **before** the operations argument at `:98`, which is why
   `BreadcrumbDropDownIntegrationTests.cs:21-39` (`Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory`)
   passes today without an ambient context. Do not reorder those arguments.
3. **`ItemViewer.Breadcrumb.cs:156` and `:192` → make the operations argument lazy.** This is the one
   addition beyond the potential's text, and it is **required, not opportunistic**.

   Both sites pass the operations object as an *eagerly evaluated argument* to
   `EnsureBreadcrumbLifecycle` (`:274-298`), which **discards it whenever `_breadcrumbLifecycleCoordinator`
   is already non-null** (`:278-281`). Swapping in `CaptureCurrent()` without laziness would make a
   pure no-op call throw on any thread without a context. Concretely, `BreadcrumbSelectorOpenRetryTests.cs:264`
   calls `Viewer.ConfigureBreadcrumbDropDown(Host, ...)` on a viewer whose lifecycle was already
   seeded at `:260` with an injected `operations` — the `:192` argument is constructed and thrown
   away on every such call.

   ```csharp
   // ItemViewer.Breadcrumb.cs — change the helper's parameter
   private BreadcrumbItemViewerLifecycleCoordinator EnsureBreadcrumbLifecycle(
       Func<BreadcrumbPopupUiOperations> operationsFactory
   )
   {
       if (_breadcrumbLifecycleCoordinator != null) { return _breadcrumbLifecycleCoordinator; }
       ...
       BreadcrumbPopupUiOperations operations = operationsFactory();
       ...
   }
   ```

   Call sites become `EnsureBreadcrumbLifecycle(() => operations)` at `:50` and
   `EnsureBreadcrumbLifecycle(BreadcrumbPopupUiOperations.CaptureCurrent)` (method group) at `:155`
   and `:191`. Diff: three call sites plus one parameter type. This **preserves every existing test's
   seam** without any test edit, which is the property the potential's "so no test loses its seam"
   claim depends on.

   The injectable constructor the potential cites (`BreadcrumbPopupUiOperations.cs:62-78`) is
   confirmed present and unchanged, and is the seam used by
   `BreadcrumbSelectorToggleUiBoundaryTests.cs:96-98`, `:182-184`, `:254-256`,
   `BreadcrumbSelectorOpenRetryTests.cs:161-163`, `:257-259`,
   `BreadcrumbPopupBoundaryCoverageTests.Part2.cs:386-388`,
   `BreadcrumbDropDownCoverageThresholdTests.cs:314-316`, and
   `BreadcrumbItemViewerLifecycleCoordinatorTests.cs:272`.

**Exactly which existing tests must change (delegation item 6, #475 half).** One:

- `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs:170-195` —
  `CaptureCurrentOrTests_NullAndControlledContexts_SelectExpectedBoundaries`. It calls the deleted
  method twice (`:178`, `:186`) and asserts the silent fallback. **Delete and replace** with a
  fail-fast assertion using the same file's existing `WithContext` helper:
  `WithContext(null, BreadcrumbPopupUiOperations.CaptureCurrent)` should throw
  `InvalidOperationException`. Retain the second half (`:184-194`, controlled context) unchanged
  against `CaptureCurrent`.

No other test references `CaptureCurrentOrTests` (repo-wide grep; the only other hits are in prose
inside `docs/features/active/webview2-host-initializer-defects-476/research/...`).

---

## 4. Regression-test feasibility per defect

Repository constraints honoured throughout: MSTest + Moq + FluentAssertions; no `Thread.Sleep`, no
`Task.Delay`, no wall-clock waits, no temporary files; tests live under `QuickFiler.Test/`.

**Reusable harnesses already in the tree — use these, do not invent new ones:**

| Harness | Location | What it gives |
| --- | --- | --- |
| `QueuedCreatorThreadSynchronizationContext` | `BreadcrumbItemViewerLifecycleCoordinatorTests.cs:354-380` | A queue that only runs on explicit `DrainOnCreatorThread()`. **The single most useful seam for this feature** — it makes "posted but not yet run" a first-class, deterministic state. |
| `LifecycleFixture` | same file, `:259-292` | A fully wired `BreadcrumbItemViewerLifecycleCoordinator` over that queue. |
| `RecordingHost` | same file, `:294-352` | A hand-written `IBreadcrumbDropDownHost` that records event add/remove. `SetTheme` (`:344`) and `Dispose` (`:348`) are currently empty and are the natural places to add recorders. |
| `CapturingSynchronizationContext` | `BreadcrumbSelectorToggleUiBoundaryTests.cs:346-...` | Drainable queue with exception/thread snapshots; already shared by five test files. |
| `InvokeAmbientNull` | `BreadcrumbSelectorToggleUiBoundaryTests.cs:325-344` | Runs a delegate with `SynchronizationContext.Current` nulled, on the same thread. **No second thread required.** |
| `ViewerScope` | `QfcItemControllerBreadcrumbDropDownTests.cs:365-383` | Real `ItemViewer` under a plain ambient context, disposed deterministically. |
| Uninitialized environment | `FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment))`, e.g. `QfcItemControllerBreadcrumbDropDownTests.cs:30-31` | A `CoreWebView2Environment` identity token without an SDK call. |

| Defect | Deterministic MSTest regression test? | Seam and shape |
| --- | --- | --- |
| **D1** | **Yes** | `ViewerScope` + two `FormatterServices`-produced environments + a strict `Mock<IWebViewCoreInitializer>` with no setups (existing tests prove initialization stays lazy — `initializer.VerifyNoOtherCalls()` at `QfcItemControllerBreadcrumbDropDownTests.cs:56`). Configure with `env1`, capture the host via the existing private-property reflection helper (`Host(viewer)`, `:335-345`), configure with `env2`, then assert the first host is disposed. **Observation point:** `host1.DropDown.IsDisposed` — `DropDown` is a public property (`BreadcrumbDropDownHost.cs:182`) and `DisposeCoreAsync` calls `DropDown.Dispose` at `:321`, which runs inline under the ambient context. Secondary assertion: `host1.Close(reason)` returns `false` (`:230-231`). Red before the fix (the outgoing host is disposed only by the later posted lambda, which under `ViewerScope`'s inline dispatcher currently *does* run — so the test must be written against a **drainable** context, `CapturingSynchronizationContext`, to make "posted but not drained" observable). |
| **D2** | **Yes — cleanest of the six** | `LifecycleFixture` + `QueuedCreatorThreadSynchronizationContext` + `RecordingHost` extended with a `List<string> ThemesApplied` in `SetTheme`. Arrange: `Coordinator.ConfigureHost(host, ...)` (post queued, **not** drained). Act: `Coordinator.SetTheme("dark")`, then `Queue.DrainOnCreatorThread()`. Assert: `host.ThemesApplied.Should().Equal("dark")`. Fails today (`DropDownHost` is null at `BreadcrumbItemViewerLifecycleCoordinator.cs:159`, so the host never sees the theme); passes after the retained-theme replay. No threads, no timing. |
| **D3** | **Yes, trivially** | `ViewerScope` + two distinct `Mock<IFolderHierarchyProvider>(MockBehavior.Strict)`. `InitializeBreadcrumbPipeline(p1)`; then `Action act = () => viewer.InitializeBreadcrumbPipeline(p2)` → `act.Should().Throw<InvalidOperationException>()`. Companion positive case: re-calling with `p1` `.Should().NotThrow()` and `viewer.BreadcrumbCoordinator` unchanged (`BeSameAs`). |
| **D4** | **No — a true data race cannot be reproduced deterministically.** Two threads with no barrier, and the repository bans sleeps and wall-clock waits, so there is no way to force the interleaving. State this explicitly in the spec. | **Closest deterministic proxy:** assert the *declared contract* rather than the race. Construct the viewer under context A (`ViewerScope`), then invoke `InitializeBreadcrumbPipeline` inside the existing `BreadcrumbSelectorToggleUiBoundaryTests.InvokeAmbientNull` helper (`:325-337`) — same thread, ambient context nulled — and assert `InvalidOperationException`. A second case using a *different* non-null context proves the reference comparison, not mere null-checking. This proves the guard fires; it does not prove the race is gone, and the spec must say so. |
| **D5** | **Yes** | `ViewerScope`: `viewer.Dispose()`, then `Action act = () => viewer.InitializeBreadcrumbPipeline(provider)` → `act.Should().Throw<ObjectDisposedException>()`. Pre-fix the call succeeds; add `viewer.BreadcrumbCoordinator.Should().BeNull()` to pin that no pipeline is built against a dead viewer. Deterministic, no timing. Note `ViewerScope.Dispose()` disposes the viewer again (`:380`) — `Control.Dispose` is idempotent, so the scope is safe. |
| **#475** | **Yes, two tests** | (1) Replacing the deleted test in `BreadcrumbPopupBoundaryCoverageTests.Part2.cs`: `WithContext(null, BreadcrumbPopupUiOperations.CaptureCurrent)` → `.Should().Throw<InvalidOperationException>()`, plus the retained controlled-context half. (2) Seam-preservation guard: on a viewer whose lifecycle was already seeded via `InitializeBreadcrumbPipeline(provider, operations)`, call the 3-arg `ConfigureBreadcrumbDropDown(host, ...)` inside `InvokeAmbientNull` and assert `.Should().NotThrow()`. Test (2) is red before the laziness change of §3.7 item 3 and green after — but only if items 1–3 land as one change-set, which they must. |

---

## 5. Existing test inventory and blast radius

### 5.1 Tests that pin the current (defective) behaviour and must be updated

| # | Test | Location | Why | Disposition |
| --- | --- | --- | --- | --- |
| 1 | `CaptureCurrentOrTests_NullAndControlledContexts_SelectExpectedBoundaries` | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs:170-195` | Directly exercises and asserts the silent fallback #475 removes; calls the deleted method at `:178` and `:186`. | **Must be deleted and replaced.** This is the only mandatory test edit in the whole change-set. |

That is the complete list of *mandatory* edits. The #488 correction comment's warning that epic #136's
children wrote tests pinning current behaviour is well-founded as a caution, but on this surface only
one such test exists.

### 5.2 Tests that constrain the fix and must be re-run and re-reasoned (no edit expected)

| Test | Location | Constraint it imposes |
| --- | --- | --- |
| `ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces` | `BreadcrumbDropDownIntegrationTests.cs:296-312` | `host.Dispose()` `Times.Once()` on viewer disposal. **This is the assertion that rules out §3.1's rejected alternatives.** Any D1 design producing two `Dispose()` calls on a `Mock<IBreadcrumbDropDownHost>` breaks it. |
| `ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost` | `QfcItemControllerBreadcrumbDropDownTests.cs:91-122` | Same-environment configure must reuse the host and must not dispose anything. Pins the `:147-153` early return. |
| `ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily` | `QfcItemControllerBreadcrumbDropDownTests.cs:24-58` | `Theme == "dark"` and `ControlHost == null` immediately after configure+theme. D2's replay must remain additive. |
| `ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam` | `:60-89` | Same, light. |
| `ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession` | `:187-262`, assertion at `:257-259` | **"no stale pooled theme is replayed."** The highest-risk interaction with D2's retained theme. Must be re-run explicitly and the reasoning recorded. |
| `HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder` | `BreadcrumbItemViewerLifecycleCoordinatorTests.cs:17-49` | Asserts subscription order `add`/`remove` across a host swap. Does **not** currently assert disposal (`RecordingHost.Dispose` at `:348` is empty), so it stays green — but it is the natural place to *add* a D1 assertion at the coordinator level. |
| `ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks` | `BreadcrumbDropDownIntegrationTests.cs:226-261` | Reset-then-reconfigure with the **same** host must take the `UpdateRequestProviders` branch. D1 and D2 must not disturb it. |
| `SetBridgeCoordinator_SameReference_DoesNotDuplicateSubscriptions` | `BreadcrumbItemViewerLifecycleCoordinatorTests.cs:134-151` | The reference-comparison precedent D3 mirrors. |
| `DisposedCoordinator_SetBridgeCoordinatorThrows` | `:203-216` | The `ObjectDisposedException` precedent D5 mirrors. |

### 5.3 Tests that must be re-verified against D4's affinity guard

Every test that constructs an `ItemViewer` and then calls a guarded member. The guard compares
`SynchronizationContext.Current` against the context captured in the `ItemViewer` constructor, so a
test that constructs the viewer *before* installing its context would break.

| Test harness | Context installed at | Viewer constructed at | Verified |
| --- | --- | --- | --- |
| `ItemViewerDropDownHarness` | `BreadcrumbDropDownIntegrationTests.cs:337` | `:338` | Yes — OK |
| `ViewerScope` | `QfcItemControllerBreadcrumbDropDownTests.cs:372` | `:373` | Yes — OK |
| `SelectorOpenHarness` | `BreadcrumbSelectorOpenRetryTests.cs:254` | `:255` | Yes — OK |
| `SubfolderActivationHarness` | `BreadcrumbSubfolderActivationTests.cs:274-276` | `:305` | Yes — OK |
| `ViewerScope` (readiness) | `BreadcrumbCollapsedSurfaceReadinessTests.cs:410` | near `:404-415` | **Not yet confirmed** |
| `ViewerScope` (pending open/close) | `BreadcrumbPendingOpenCloseTests.cs`, used at `:160-189` | — | **Not yet confirmed** |
| lifecycle scope | `BreadcrumbCoordinatorLifecycleTests.cs:28-29`, used at `:122` | — | **Not yet confirmed** |

### 5.4 Adjacent tests that touch these types but are unaffected

All eleven `new BreadcrumbDropDownHost(...)` test sites were inspected. Every one either passes an
explicit `BreadcrumbPopupUiOperations` (so it never reaches `:98`/`:118`) or installs an ambient
`SynchronizationContext` before construction:

- Explicit operations: `BreadcrumbSelectorToggleUiBoundaryTests.cs:115`, `:195`, `:260`;
  `BreadcrumbSelectorOpenRetryTests.cs:178`; `BreadcrumbPopupBoundaryCoverageTests.Part2.cs:378`;
  `BreadcrumbDropDownCoverageThresholdTests.cs:317`.
- Ambient context installed first: `BreadcrumbPendingOpenCloseTests.cs:213` (context at `:205-207`);
  `BreadcrumbDropDownLifecycleConcurrencyTests.cs:254` (context at `:234-236`).
- Null-argument guards that throw before the operations argument is evaluated:
  `BreadcrumbDropDownIntegrationTests.cs:25`; `BreadcrumbDropDownHostTests.cs:312`, `:314` (the latter
  two go through the production 7-param ctor at `BreadcrumbDropDownHost.cs:37-55`, which already uses
  `CaptureCurrent()` at `:54`).

---

## 6. File-size headroom (500-line ceiling is an acceptance criterion)

Counts are line counts of the current files.

### Production files this feature would touch

| File | Lines | Headroom | Expected delta | Verdict |
| --- | --- | --- | --- | --- |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | **319** | 181 | D1 +4, D3 +8, D4 +14, D5 +4, #475 +3 ≈ **+33** | Comfortable — lands near 352 |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | **481** | **19** | D2 ≈ **+6** | **Tight but sufficient.** Lands at ~487. No other edit may target this file. |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | **494** | **6** | #475 **−4** (delete `:86-89`) | Improves to ~490 |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | **463** | 37 | #475 **0** (two identifier swaps) | Fine |

### Production files read but NOT edited

| File | Lines | Note |
| --- | --- | --- |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | 355 | Owned by feature 501 (#462). Do not edit. |
| `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` | 285 | No edit required. |
| `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs` | 68 | No interface change. |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 456 | Owned by feature 501 (#500 + #501). |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 487 | Owned by feature 501 (#502). |
| `QuickFiler/Viewers/ItemViewer.cs` | 432 | Owned by feature 489. |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 430 | Owned by feature 484. |
| `QuickFiler/Viewers/ItemViewer.Designer.cs` | **6224** | **Already 12x over the ceiling.** Designer-generated. **Do not edit** — §3.5 is designed specifically to avoid it. |

### Test files

| File | Lines | Headroom | Note |
| --- | --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 382 | 118 | Best home for the D2 coordinator-level test and a D1 coordinator-level assertion. |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 480 | **20** | The #475 replacement test goes here; deleting the 26-line old test frees room first. |
| `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs` | 385 | 115 | Available. |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | **500** | **0** | **At the ceiling. Cannot grow by one line.** |

**Proposed split, required by the two constrained files.** Put the viewer-level regression tests
(D1, D3, D4, D5) in a **new** file
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs`, registered with **one**
`<Compile Include="Viewers\ItemViewerBreadcrumbLifecycleRegressionTests.cs" />` in
`QuickFiler.Test/QuickFiler.Test.csproj`, inserted at its alphabetical position among the existing
`Viewers\ItemViewer*` entries. That is inside this feature's alphabetical region and satisfies the
"do not edit the csproj outside this feature's alphabetical region" constraint. No
`QuickFiler/QuickFiler.csproj` edit is required — this feature adds no production file.

---

## 7. File ownership across concurrently prepared siblings

This was checked because several sibling features are in `docs/features/active/` right now and touch
adjacent breadcrumb types.

**Feature 501 (`breadcrumb-coordinator-hub-defects-501`, closing #462/#500/#501/#502) explicitly cedes
this feature's four files.** Its spec, `## Scope & Non-Goals` → "Explicitly excluded systems and
files" (`docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md:155-164`):

> - `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`,
>   `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`,
>   `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`, `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` —
>   sibling feature 488.

and its cross-feature notes (`:177-185`) name feature 488 as the owner of the
`DetachCollapsedMessenger` / `DetachPopupMessenger` ordering and of the host-reopen residual behind
#462's known limitation. **501 owns** `BreadcrumbDropDownOpenCoordinator.cs`,
`BreadcrumbCoordinatorUpgradeLifetime.cs`, `BreadcrumbMessengerHub.cs`,
`BreadcrumbBridgeCoordinator.cs`, and a new `BreadcrumbBridgeCoordinator.Suggestions.cs` (`:120-128`).
This is the reason §3.1 rejects the otherwise-best D1 fix.

**Feature 476 (`webview2-host-initializer-defects-476`) does not overlap.** Its
`### Files/modules to change` table (`spec.md:457-465`) lists only `WebView2BreadcrumbHost.cs`,
`WebView2CoreInitializer.cs`, `IWebViewCoreInitializer.cs`, and two test files. It records
`BreadcrumbPopupUiOperations.cs:388` as **FORBIDDEN** to itself (`:564`).

**Feature 484 (`qfc-item-controller-defects-484`) owns `QfcItemController.ViewerSetup.cs`.** See §8.

---

## 8. Citing the 484 upstream-contract table (delegation item 9)

`docs/features/active/qfc-item-controller-defects-484/spec.md`, heading
`### Upstream contract (exhaustive) — required by features 464 and 489` at **`:329`**, running to
`:394`, is the authoritative, five-round-adversarially-reviewed enumeration of the `QfcItemController`
surface delta. **This research cites it rather than re-deriving member lists or detach counts.** The
relevant rows for this feature:

- `:358` — `Cleanup()` (`ViewerSetup.cs:396-425`) will gain an `UnwireEvents()` call plus
  `_emailIsReadTimer` disposal and a `_mailActions` null. **This feature does not edit `Cleanup()`**,
  so there is no conflict; only the statement-order constraints at `:387-394` matter, and they place
  `UnwireEvents()` immediately after `ResetBreadcrumb()` (`:400`), which this feature also does not
  move.
- `:363` — `InitializeWebViewAsync()` (`ViewerSetup.cs:42-128`) remains `internal async Task` and
  remains `[ExcludeFromCodeCoverage]`, with only the `WebResourceRequested` lambda replaced. The
  breadcrumb call sequence at `:110-120` is untouched by 484.
- `:367-372` — **no member removed, no public member added, no interface modified.** So nothing this
  feature reads from `QfcItemController` disappears.

**Verification of the three `ViewerSetup.cs` citations that sit outside 484's table** (the potential
entries' citations, which the delegation flagged as needing re-checking):

| Potential's citation | Current location | Status |
| --- | --- | --- |
| `ViewerSetup.cs:140-146` — `EnsureBreadcrumbPipeline` guard on `viewer.BreadcrumbCoordinator == null` | **`:143-149`**, inside the method at `:136-161` | Drifted +3; content unchanged |
| `ViewerSetup.cs:166-167` — configure then `SetBreadcrumbTheme` | **`:169-170`**, inside `ConfigureBreadcrumbDropDown` at `:164-171` | Drifted +3; content unchanged |
| `ViewerSetup.cs:396` — `ResetBreadcrumb()` on reuse | `Cleanup()` opens at **`:396`**; the `ResetBreadcrumb()` call is at **`:400`** | The method line is right, the call line is `:400` |

Independently corroborating: 484's own table cites `Cleanup()` as `ViewerSetup.cs:396-425` (`:358`),
`ResetBreadcrumb()` at `:400` (`:392`), and `_itemViewer = null` at `:407` (`:387`) — all three match
the file exactly, so 484's table is line-accurate against current HEAD.

---

## Dependencies on 489

Sibling feature `itemviewer-surface-defects-489` (issues #486, #487, #489, #490) is being prepared
concurrently on another branch and is not present in this worktree. Its owned surface was determined
from the four promoted potential records
(`docs/features/potential/promoted/2026-08-07-itemviewer-move-option-menu-defects.md` (#486),
`...-itemviewer-parentchanged-console-and-cast.md` (#487),
`...-itemviewer-ui-thread-marshalling-divergence.md` (#489),
`...-itemviewer-display-and-folder-contract-defects.md` (#490)), read locally, plus a fetch of issue
#489 confirming its body is generated from the #489 potential file.

**No fix in this feature consumes a member that 489 defines.** Every recommended change is confined to
`ItemViewer.Breadcrumb.cs`, `BreadcrumbItemViewerLifecycleCoordinator.cs`,
`BreadcrumbPopupUiOperations.cs`, and `BreadcrumbDropDownHost.cs`, none of which appears on 489's
surface. The list below is therefore **assumptions about 489 not changing things this feature reads**,
not contract requests. Each is one targeted re-check.

| # | File | Member / contract | What this feature assumes |
| --- | --- | --- | --- |
| D489-1 | `QuickFiler/Viewers/ItemViewer.cs` | `public SynchronizationContext UiSyncContext { get; }` (`:60-63`), backed by `_context` assigned in the constructor at `:26` | That it still exists and still returns the context captured at construction. §3.4's `ThrowIfOffUiBoundary` uses it as the boundary proof. 489's #489 Defect 4 proposes consolidating `UiSyncContext` / `UiScheduler` (`:66-69`) / `UiDispatcher` (`:72-75`) onto **one** seam; if `UiSyncContext` is the survivor there is no impact, if it is not the guard must be re-pointed at the survivor. |
| D489-2 | `QuickFiler/Viewers/ItemViewer.cs` | `[ExcludeFromCodeCoverage]` on the `ItemViewer` partial type declaration at `:20` | That it is not removed. It exempts every member of `ItemViewer.Breadcrumb.cs` from coverage measurement (§1.3). Removing it would put ~350 lines into the coverage denominator and change this feature's coverage target. |
| D489-3 | `QuickFiler/Viewers/ItemViewer.Designer.cs` | `protected override void Dispose(bool disposing)` at `:16-23` — disposes `components` only when non-null | That its shape is unchanged. It is the entire basis of D5 (§2/D5, §3.5). 489's #487 Defect 1 proposes deleting the `L0v2h2_WebView2_ParentChanged` handler and its designer wiring at `:256` in this same file; that edit does not touch `Dispose(bool)`, but it does put the file in 489's diff. This feature deliberately does **not** edit this file. |
| D489-4 | `QuickFiler/Viewers/ItemViewer.*.cs` (all partials of the `ItemViewer` type) | Member-name uniqueness across the partial type | That 489 introduces no member named `ThrowIfOffUiBoundary`, `_breadcrumbProvider`, or `outgoing` at type scope. 489 owns `ItemViewer.cs`, `ItemViewer.WebViewThread.cs`, `ItemViewer.FolderSearch.cs`, `ItemViewer.DisplayState.cs`, `ItemViewer.Commands.cs`; this feature owns `ItemViewer.Breadcrumb.cs`. A name collision across partials is a **compile error, not a merge conflict**, so it would surface only at integration, not at fan-in. |
| D489-5 | `QuickFiler.Test/QuickFiler.Test.csproj` | `<Compile Include=...>` ordering | That 489's added test-file entries do not occupy the alphabetical slot this feature uses for `Viewers\ItemViewerBreadcrumbLifecycleRegressionTests.cs` (§6). Adjacent, not identical, entries are expected; a same-name entry is not. |
| D489-6 | `QuickFiler/Viewers/IItemViewer.cs` | No change to the interface | 489's #490 Defect 1 proposes renaming or re-specifying `SetFolderItems`. This feature does not call `SetFolderItems`, but `BreadcrumbDropDownIntegrationTests` and `BreadcrumbSubfolderActivationTests` — which this feature must keep green — do (`BreadcrumbDropDownIntegrationTests.cs:248`, `:341`). If 489 changes that member's semantics, those tests change under 489, not here. |

Six items, all narrow. There is no case where this feature must wait on 489's spec to be finalised.

---

## 9. Summary of recommendations

| Unit | Root cause confirmed | Production reachability today | Recommended minimal fix | Owned file |
| --- | --- | --- | --- | --- |
| D1 | Yes, **materially revised** — the host *is* disposed, but by a discarded post that is not ordered against the replacement's synchronous construction | Replacement path taken on every pooled reuse; harmful ordering **latent** (production is on the captured boundary) | Dispose the outgoing host in `ItemViewer.ConfigureBreadcrumbDropDown(env, initializer)` before constructing the replacement (4 lines) | `ItemViewer.Breadcrumb.cs` |
| D2 | Yes | **Latent** (configure and theme are issued back to back on the UI thread) | Retain the theme on the lifecycle coordinator and replay it when a host is adopted (~6 lines) | `BreadcrumbItemViewerLifecycleCoordinator.cs` |
| D3 | Yes, but **not reachable** — the controller guard prevents the second call; the stale-provider symptom originates at `ViewerSetup.cs:143` | **Not reachable** | Fail fast on a different provider, matching `SetBridgeCoordinator`'s reference comparison (~8 lines, needs one new field) | `ItemViewer.Breadcrumb.cs` |
| D4 | Yes | Not reachable (both callers UI-thread-bound); contract is undeclared | Declare and enforce UI-thread affinity against `UiSyncContext` (~14 lines) | `ItemViewer.Breadcrumb.cs` |
| D5 | Yes, **reachability mechanism corrected** — not the `ConfigureHost` post; it is `InitializeWebViewAsync` resuming against a disposed pooled viewer | **Plausibly live** | Refuse creation during teardown: `if (IsDisposed || Disposing) throw` (3 lines) | `ItemViewer.Breadcrumb.cs` |
| #475 | Yes, mechanism refined | **Latent on production paths**; live as a design defect (test affordance selected by ambient probing, reachable from a `public` ctor) | Delete `CaptureCurrentOrTests()`; point `BreadcrumbDropDownHost.cs:98`/`:118` at `CaptureCurrent()`; make `EnsureBreadcrumbLifecycle`'s operations argument lazy | `BreadcrumbPopupUiOperations.cs`, `BreadcrumbDropDownHost.cs`, `ItemViewer.Breadcrumb.cs` |
| D1c (new) | Yes — `ConfigureHost`'s generation guard drops the **incoming** host without disposing it | Reachable via `ResetBreadcrumb()` after an off-boundary configure | **Out of scope — promote to a new issue** | — |

Open items for the planner, in priority order:

1. **Read the #488 correction comment verbatim** and diff it against §2 and §3 (§0.1). This is the
   only unverified input.
2. Confirm the three unverified test harnesses in §5.3 before committing to D4's guard.
3. Re-run and reason explicitly about
   `ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession`
   (`QfcItemControllerBreadcrumbDropDownTests.cs:257-259`) against D2's retained-theme replay.
4. Confirm that a faulted `InitializeWebViewAsync` task is observed by its caller before adopting
   D5's throw (§3.5).
5. Promote D1c to a new issue (§3.6).
