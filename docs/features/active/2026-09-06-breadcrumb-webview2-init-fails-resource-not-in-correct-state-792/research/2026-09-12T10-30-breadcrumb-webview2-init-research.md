# Breadcrumb CoreWebView2 initialization fails with 0x8007139F (Issue #792) — research

- Date: 2026-09-12
- Issue: #792 (kind: bug, work mode: full-bug)
- Branch/worktree state at time of research: branch `worktree-agent-a381ab17672038e9c` in worktree
  `.claude/worktrees/agent-a381ab17672038e9c`, HEAD `2405a829d6afd3b12eb7c228d57158a97cb4e2ca`, which
  equals `origin/main` at the time of this run. (An earlier draft of this line named a sibling
  worktree's branch; that was the harness-reported state of a different checkout and is corrected
  here by the orchestrator.)
- Scope: verification of the maintainer's stated mechanism, root-cause determination, and fix shaping
  for AC-U1 through AC-U4. AC-U5 is manual live-Outlook verification and is out of scope for
  automation, as instructed.

All citations below were re-derived against the tree as it stands now. No line number was copied
from the issue body. Every claim names the file, the symbol, and the line span actually read.

---

## Executive summary

The maintainer's mechanism is **partly confirmed, partly refuted, and the root cause is different
from every candidate listed in the issue body.**

**Root cause (new, and authoritatively documented).** The add-in creates `CoreWebView2Environment`
instances from three production sites against the *same* user-data folder
`%LocalAppData%\WindowsFormsWebView2` but with *two different* `CoreWebView2EnvironmentOptions`:

| # | Site | Additional browser arguments |
|---|---|---|
| 1 | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:61` (`InitializeWebViewAsync`, :48-130) | `"--incognito "` |
| 2 | `QuickFiler/Controllers/EfcItemController.cs:187-189` (`InitializeWebViewAsync`, :178-211), constant at `:176` | `"--incognito "` |
| 3 | `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:250` (`InitializeAsync`, :239-270) | **none** (`new CoreWebView2EnvironmentOptions()`) |

Microsoft's WebView2 Win32 reference for `CreateCoreWebView2EnvironmentWithOptions` states verbatim:

> As a browser process may be shared among WebViews, WebView creation fails with
> `HRESULT_FROM_WIN32(ERROR_INVALID_STATE)` if the specified options does not match the options of
> the WebViews that are currently running in the shared browser process.

and, in the same page's error table:

> `HRESULT_FROM_WIN32(ERROR_INVALID_STATE)` — Specified options do not match the options of the
> WebViews that are currently running in the shared browser process.

`HRESULT_FROM_WIN32(ERROR_INVALID_STATE)` is `0x8007139F`, the exact HRESULT in the logs. The .NET
`CoreWebView2Environment.CreateAsync` reference states the same rule in prose on the `options`
parameter ("WebView creation fails if the specified `options` does not match the options of the
WebViews that are currently running in the shared browser process").

Site 3 is the *only* site in the add-in that omits `--incognito`, and site 3 is the *only* site that
fails. Once any QuickFiler item-body or Efc item-body WebView is running (both are `--incognito`),
the shared browser process is pinned to those options and the breadcrumb host's no-argument
environment cannot produce a controller. This explains, without appeal to timing:

- why the failure was originally intermittent (it depends on whether an `--incognito` WebView was
  already running in the Outlook process when the Efc breadcrumb initialized), and
- why it is now deterministic on both reported entry points: a pop-out always follows an open
  QuickFiler, and the logged 19:56 Sort Email open followed ten pop-outs in the same session.

**Clause-by-clause verdict on the maintainer's mechanism:**

| Clause | Verdict |
|---|---|
| "A half-initialized host never raises `CoreInitialized`" | **Confirmed.** `WebView2BreadcrumbHost.OnCoreInitializationCompleted` returns on `!e.IsSuccess` at `:335-342` before the `CoreInitialized?.Invoke` at `:353`. |
| "so the pending document is dropped" | **Confirmed and sharpened.** `_pendingDocument` has exactly one drain, `BreadcrumbBridgeRouter.NotifyCoreInitialized` (`BreadcrumbBridgeRouter.cs:320-329`), reachable only from that event. The outbound *queue* is likewise never flushed. |
| "fire-and-forget tasks swallow the failure with a log-only outcome" | **Half confirmed, half already fixed.** `InitializeBreadcrumbHostAsync` (`EfcFormController.cs:1072-1082`) is log-only. `PopulateFolderCombobox` (`EfcFormController.cs:1251-1272`) **already** routes through `TryReportBoundaryFault` at `:1270`. |
| "two log lines suggest initialization is attempted from two paths" | **Refuted.** It is one failure logged twice: the SDK's `CoreWebView2InitializationCompleted(IsSuccess=false)` handler logs once, and the same failure faults the task awaited in `InitializeBreadcrumbHostAsync`, which logs again. There is exactly one `InitializeAsync` call site for the Efc breadcrumb host. |
| "initialized while its handle or parent was not yet in a valid state" | **Refuted as the cause.** Not the documented meaning of this HRESULT. |
| "a second initialization against a control already mid-initialization" | **Refuted.** Single call site; the per-control owner registry (`WebView2BreadcrumbHost.cs:46-51, 101-110`) makes a second host detach the first. |
| "initialization against a disposed or pooled-and-reused control" | **Refuted for this HRESULT**, though pooling *is* real here (see Q6 — the issue's own pooling claim is wrong in the opposite direction). |
| "a user-data-folder or environment conflict between two WebView2 instances in the same process" | **Confirmed — this is the cause**, specifically an *options* conflict, not a folder conflict. The folder is identical at all three sites. |
| "apply the #678 carry pattern to the pop-out path" | **Valid but orthogonal.** It fixes a real second defect (Efc rebuilds prediction from scratch) but would not have prevented a single 0x8007139F. |

**Prior findings.** `docs/features/potential/promoted/2026-08-07-webview2breadcrumbhost-handler-retention-pooled-viewer.md` (#458) and
`docs/features/potential/promoted/2026-08-07-webview2breadcrumbhost-unmarshalled-sdk-call-and-unsynchronized-state.md` (#476)
are **both already fixed in the current tree** and are **neither the same defect nor contributing
causes** of #792. Evidence in Q2.

---

## Numeric Derivation Evidence

One enumeration below is load-bearing for the proposed fix ("every production WebView2 environment
option set must agree"), so it is derived twice by independent means.

### Claim: the add-in has exactly THREE production `CoreWebView2EnvironmentOptions` construction sites, of which exactly ONE omits the additional browser arguments

The eleven canonical single-line declarations follow. The expanded narrative beneath them records the
same derivation with its per-hit classification.

- Complete Family: CoreWebView2EnvironmentOptions, CreateEnvironmentAsync, CoreWebView2Environment.CreateAsync
- Exhaustive Search Scope: the entire repository source tree, covering every C-sharp file in all six production assemblies and in the test projects
- Inclusion Rules: production object-creation expressions that build a WebView2 environment options instance and hand it to an environment creation call, in explicit, target-typed, and object-initializer spellings
- Exclusion Rules: parameter declarations, XML documentation references, commented-out lines, the pass-through adapter forward that carries no options of its own, and every file under the test projects
- Primary Search Strategy or Query Expression: Grep the entire repository tree for the type name CoreWebView2EnvironmentOptions, then read every hit and retain only object-creation expressions whose instance is handed to CreateEnvironmentAsync or to CoreWebView2Environment.CreateAsync
- Primary Member Set: QuickFiler/Viewers/WebView2BreadcrumbHost.cs, QuickFiler/Controllers/QfcItemController.ViewerSetup.cs, QuickFiler/Controllers/EfcItemController.cs
- Primary Count: 3
- Cross-check Search Strategy or Query Expression: Approach the family from the consumer end instead of the type name, running git grep over every tracked C-sharp file across the complete source tree for CoreWebView2Environment.CreateAsync and for CreateEnvironmentAsync, then opening each production hit to recover the CoreWebView2EnvironmentOptions value that call is given
- Cross-check Member Set: QuickFiler/Controllers/EfcItemController.cs, QuickFiler/Viewers/WebView2BreadcrumbHost.cs, QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
- Cross-check Count: 3
- Member-set Comparison: The primary and cross-check member sets are identical element for element, so the two independent derivations match on both membership and count.

Expanded narrative of the same derivation:

- **Complete Family**: every production (non-test) expression in the repository that constructs a
  `Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions` instance and hands it, directly or
  through the `IWebViewCoreInitializer` seam, to a `CoreWebView2Environment` creation call.
- **Exhaustive Search Scope**: all `*.cs` files under the six production assemblies `QuickFiler/`,
  `UtilitiesCS/`, `TaskMaster/`, `ToDoModel/`, `Tags/`, `TaskVisualization/`. WebView2 package
  references exist only in QuickFiler; the other five were searched to prove absence rather than
  assumed empty.
- **Inclusion Rules**: object-creation expressions of the type, in any spelling — explicit
  (`new CoreWebView2EnvironmentOptions(...)`), target-typed (`new(...)` on a declared variable of
  that type), and object-initializer forms.
- **Exclusion Rules**: occurrences in parameter lists, `<see cref>` documentation, commented-out
  code, and any file under `QuickFiler.Test/` (test doubles are not production option sets).
- **Primary Search Strategy / Query Expression**: type-name grep
  `CoreWebView2EnvironmentOptions` restricted to the six production assembly globs, then manual
  classification of every hit into declaration / parameter / doc / comment / construction.
- **Primary Member Set** (constructions only):
  1. `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:250` — `new CoreWebView2EnvironmentOptions()` — **no arguments**
  2. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:61` — `new("--incognito ")` (target-typed) — `--incognito `
  3. `QuickFiler/Controllers/EfcItemController.cs:187-189` — `new CoreWebView2EnvironmentOptions(IncognitoArgument)` with `IncognitoArgument = "--incognito "` at `:176` — `--incognito `

  Non-construction hits excluded: `IWebViewCoreInitializer.cs:17` (doc), `:51` (parameter),
  `WebView2CoreInitializer.cs:37` (parameter), `:69` (parameter),
  `QfcItemController.ViewerSetup.cs:60` (commented-out), `EfcItemController.cs:168` (doc),
  `EfcItemController.cs:186` (commented-out).
- **Primary Count**: 3 constructions; 1 with no additional browser arguments.
- **Cross-check Search Strategy / Query Expression**: approach the family from the *consumer* end
  instead of the type name — grep `CreateEnvironmentAsync\(|EnsureCoreWebView2Async\(|WindowsFormsWebView2`
  across the whole repository excluding `QuickFiler.Test/**`, plus an independent grep for the raw
  SDK factory `CoreWebView2Environment\.CreateAsync` across all `*.cs`. Each environment creation
  must be reached by exactly one options object, so enumerating creations enumerates option sets.
- **Cross-check Member Set**:
  - `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:246-269` — cache folder built at `:246-249`,
    options at `:250`, `_initializer.CreateEnvironmentAsync(cacheFolder, options)` at `:265-268`.
  - `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:55-77` — cache folder at `:58`, options
    at `:61`, `_webViewInitializer.CreateEnvironmentAsync(cacheFolder, options)` at `:70-73`.
  - `QuickFiler/Controllers/EfcItemController.cs:180-198` — cache folder at `:184`, options at
    `:187-189`, direct `CoreWebView2Environment.CreateAsync(null, cacheFolder, options)` at
    `:194-198` (this one bypasses the seam).
  - The only other `CoreWebView2Environment.CreateAsync` hit in production is the seam forward
    `QuickFiler/Viewers/WebView2CoreInitializer.cs:72`, which is a pass-through with no options of
    its own, and `QfcItemController.ViewerSetup.cs:123` which is commented out.
  - No hit in `UtilitiesCS/`, `TaskMaster/`, `ToDoModel/`, `Tags/`, `TaskVisualization/`.
  - `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:383` calls `EnsureCoreWebView2Async` but
    constructs **no** environment: it forwards a caller-supplied one (the `--incognito` environment
    threaded from `QfcItemController.ViewerSetup.cs:113-116, 118-122`). Correctly excluded.
- **Cross-check Count**: 3 environment creations, therefore 3 option sets; 1 with no additional
  browser arguments.
- **Member-set Comparison**: normalized to `(file, member, additional-arguments)`, the primary set is
  `{(WebView2BreadcrumbHost, InitializeAsync, none), (QfcItemController.ViewerSetup, InitializeWebViewAsync, "--incognito "), (EfcItemController, InitializeWebViewAsync, "--incognito ")}`
  and the cross-check set is identical, element for element. The two searches used distinct
  expressions (type name vs. consumer API names) and distinct file scopes (six-assembly glob vs.
  whole repository minus the test project) and agree on both membership and count.

---

## Q1 — `WebView2BreadcrumbHost` initialization state machine

File: `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` (368 lines, `#nullable enable`).

**On `CoreWebView2InitializationCompleted` with `e.IsSuccess == false`** — `OnCoreInitializationCompleted`,
`:329-354` (attribute `[ExcludeFromCodeCoverage]` at `:329`):

```csharp
if (!e.IsSuccess)
{
    log.Error(
        $"Breadcrumb CoreWebView2 initialization failed: {e.InitializationException?.Message}",
        e.InitializationException
    );
    return;                                     // :341
}
```

- `CoreInitialized` is **not** raised (the invoke is at `:353`, after the early return).
- `IsCoreInitialized` is left **false**: `Volatile.Write(ref _isCoreInitialized, true)` is at `:352`,
  also after the return. The backing field is `_isCoreInitialized` at `:64`; the property is
  `Volatile.Read` at `:137`.
- `core.WebMessageReceived` is never subscribed (`:346-347`), so the inbound bridge is dead too.
- The host is **not** disposed, **not** retried, and left half-constructed: `_isAttached` stays true
  (set at `:114`), the owner-registry entry stays (`:101-110`), and the control keeps the
  `CoreWebView2InitializationCompleted` and `Disposed` subscriptions made at `:112-113`.
- There is **no** retry anywhere in the type. `InitializeAsync` (`:239-270`) is a straight-line
  method with no loop and no catch.

**Is initialization attempted from more than one path?** No. The only production construction of
`WebView2BreadcrumbHost` is `EfcFormController.ConfigureBreadcrumbControl` at
`QuickFiler/Controllers/EfcFormController.cs:1049-1052`, and the only production call of
`InitializeAsync` is `EfcFormController.InitializeBreadcrumbHostAsync` at
`QuickFiler/Controllers/EfcFormController.cs:1076`, fired once from `:1067`.

**The two log lines are one failure, logged twice.** Line 1 (`QuickFiler.Viewers.WebView2BreadcrumbHost`)
is `:337-340`. Line 2 (`QuickFiler.Controllers.EfcFormController`) is `EfcFormController.cs:1080`,
inside the `catch` of `InitializeBreadcrumbHostAsync` (`:1072-1082`), which awaits
`_breadcrumbHost.InitializeAsync(...)`. The WinForms `WebView2.EnsureCoreWebView2Async` task faults
with the same initialization exception that the event reports, so one SDK failure produces both
lines. The 59-67 ms gap between the paired lines in every logged occurrence is consistent with one
event-then-task-continuation sequence, not with two independent SDK attempts.

**Consequence for AC-U1.** The `!e.IsSuccess` branch is a dead end that a unit test cannot reach
(`CoreWebView2InitializationCompletedEventArgs` has no public constructor, which is the documented
exemption rationale at `:323-328`). The testable surface for retry and error-surfacing is the
awaited-task path in `InitializeBreadcrumbHostAsync`, which goes through the mockable
`IWebViewCoreInitializer` seam.

---

## Q2 — What produces 0x8007139F here

**Established cause: environment-options mismatch in a shared browser process.** See the executive
summary and the Numeric Derivation Evidence section for the enumeration and the two authoritative
documentation quotations. The three environment creations share the user-data folder
`%LocalAppData%\WindowsFormsWebView2` (built identically at
`WebView2BreadcrumbHost.cs:246-249`, `QfcItemController.ViewerSetup.cs:55-58`,
`EfcItemController.cs:181-184`) but disagree on `AdditionalBrowserArguments`.

**Discrimination between the issue body's candidates:**

| Candidate | Reachable in this code? | Evidence |
|---|---|---|
| Initialized before the control has a window handle / parent not valid | Reachable but not this HRESULT | `ConfigureBreadcrumbControl` runs from `WireEventHandlers` (`EfcFormController.cs:520`), which runs before `EfcHomeController.Run()`/`RunAsync()` shows the form (`EfcHomeController.cs:308-340`). So the control genuinely is un-shown at `EnsureCoreWebView2Async`. However the documented HRESULT for this condition is not `ERROR_INVALID_STATE`, and the identical pre-show ordering holds for `QfcItemController.InitializeWebViewAsync`, which does not fail. |
| Second initialization against a control already mid-initialization | **Not reachable** | One construction site, one `InitializeAsync` call site; the owner registry at `:46-51`/`:101-110` guarantees at most one attached host per control. |
| Disposed or pooled-and-reused control | Pooling is real, this HRESULT is not its symptom | `EfcViewer` instances *are* pooled (Q6), but each pooled viewer carries its own Designer-owned `WebView2`; a reused *control* would surface as the #458 duplicate-notification shape, which is already fixed. |
| User-data-folder or environment conflict between two WebView2 instances in one process | **This is the cause**, in its options form | Three sites, one folder, two option sets; documented HRESULT match. |

**Prior finding `2026-08-07-webview2breadcrumbhost-handler-retention-pooled-viewer.md` (#458):**
**unrelated, and already fixed.** The document describes a constructor-side `-=` that could not remove
a predecessor's subscription. The current file replaced that with a per-control
`ConditionalWeakTable<WebView2, WebView2BreadcrumbHost>` owner registry (`:46-51`) guarded by
`_ownersGate` (`:51`), which detaches the predecessor explicitly (`:101-110`, `DetachCore` at
`:308-321`), plus a `Disposed` hygiene path at `:287-301`. Not a contributing cause of #792.

**Prior finding `2026-08-07-webview2breadcrumbhost-unmarshalled-sdk-call-and-unsynchronized-state.md` (#476):**
**unrelated, and already fixed.** Defect 1 (unmarshalled SDK touch) is fixed: `NavigateToString`
(`:157-167`) and `PostMessageJson` (`:193-218`) both route through `BreadcrumbUiDispatcher.Dispatch`.
Defect 2 (unsynchronized state publication) is fixed: explicit `_isCoreInitialized` field at `:64`
with `Volatile.Read` at `:137` and `Volatile.Write` at `:352`, and the ordering comment at `:349-351`.
Not a contributing cause of #792.

**One residual worth noting (not the cause).** `EfcItemController.InitializeWebViewAsync`
(`EfcItemController.cs:178-211`) calls `CoreWebView2Environment.CreateAsync` directly at `:194-198`
rather than through `IWebViewCoreInitializer`. Any options-parity fix must cover that site too, and
it is the only environment creation not behind the mockable seam.

---

## Q3 — Pending-document path

File: `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` (221 lines).

`DeliverDocument` — `:168-180`:

```csharp
private void DeliverDocument()
{
    string document = _renderer.RenderDocument(_rows, _darkMode, _selectedRowId);
    if (_host.IsCoreInitialized)
    {
        _host.NavigateToString(document);
        _pendingDocument = null;                 // :174
    }
    else
    {
        _pendingDocument = document;             // :178
    }
}
```

Stash condition: `_host.IsCoreInitialized == false`. Nothing else.

Field declaration: `private string? _pendingDocument;` at `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:40`.

**Complete set of writes** (grep for `_pendingDocument` over all `*.cs`, four production hits):
`BreadcrumbBridgeRouter.cs:40` (declaration), `:322` (read), `:325` (clear);
`BreadcrumbBridgeRouter.Selection.cs:174` (clear), `:178` (stash). There is no occurrence in
`BreadcrumbBridgeRouter.Arrows.cs`.

**Complete set of drains** — exactly two:
1. `DeliverDocument` itself, on a later call that finds `IsCoreInitialized == true` (`:174`).
2. `BreadcrumbBridgeRouter.NotifyCoreInitialized()` — `BreadcrumbBridgeRouter.cs:320-329`:
   navigates `_pendingDocument`, nulls it, then calls `_outboundQueue.OnInitializationCompleted()`.

**Callers of `DeliverDocument`**: `BindRowsAsync(IReadOnlyList<string>, IEnumerable<FolderScore>, string, CancellationToken)`
at `BreadcrumbBridgeRouter.cs:176`, and `ApplyTheme(bool)` at `:313`.

**Caller of `NotifyCoreInitialized`**: one production site, the lambda at
`EfcFormController.cs:1064`: `_breadcrumbHost.CoreInitialized += (s, e) => _router.NotifyCoreInitialized();`

**Confirmed: a failed initialization leaves `_pendingDocument` permanently undrained.** `IsCoreInitialized`
can only become true inside `OnCoreInitializationCompleted` (`WebView2BreadcrumbHost.cs:352`), which
the `!e.IsSuccess` return at `:341` skips, and `CoreInitialized` is likewise never raised, so neither
drain can ever fire for the rest of the viewer's life. Every subsequent `BindRowsAsync` and
`ApplyTheme` overwrites the stash with a document nobody will read.

**Additional finding not in the issue body.** `BreadcrumbOutboundQueue`
(`QuickFiler/Controllers/BreadcrumbOutboundQueue.cs`, 68 lines) has the same shape and the same
single drain. `PostOrQueue` (`:37-52`) enqueues to an unbounded `Queue<string>` (`:18`) whenever
`_host.IsCoreInitialized` is false; `OnInitializationCompleted` (`:59-65`) is its only drain and is
called only from `NotifyCoreInitialized` (`BreadcrumbBridgeRouter.cs:328`). Every selection,
row-render and theme change in a failed session therefore accumulates a serialized payload that is
never released until the router is collected. AC-U2 should cover this queue as well as the document.

---

## Q4 — Fault reporting

**`TryReportBoundaryFault` does not live where the issue says it does.** It has exactly one
definition in the repository:

`QuickFiler/Controllers/EfcFormController.cs:150-168`

```csharp
private void TryReportBoundaryFault(string message, System.Exception exception)
```

Behaviour: reads the instance property `BoundaryErrorSink` into a local (`:152`); if null, falls back
to `logger.Error(message, exception)` and returns (`:153-157`); otherwise invokes the sink inside a
`try`, and on a throwing sink logs the sink failure and then the original (`:159-167`).

What a caller must hold: an `EfcFormController` instance. It is a private instance member, so only
members of `EfcFormController` (including future partial parts) can invoke it.

The sink it dispatches to: `internal System.Action<string, System.Exception> BoundaryErrorSink { get; set; }`
at `:128-129`, defaulting to `DefaultBoundaryErrorSink` (`:137-141`), which logs and then calls
`UserFaultNotifier` (`:181-185`, an `AsyncLocal`-backed injectable, `:173-174`), whose production
default is the modeless notice `ShowModelessFaultNotice` (`:200-229`, coverage-exempt).

**The item-controller files named by the issue contain a *different*, similarly-shaped member.**
`QuickFiler/Controllers/EfcItemController.WebViewFaultBoundary.cs:44-65` and
`QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs:44-65` each define
`private void TryReportWebViewInitializationFault(Exception ex)` over a separate
`WebViewInitializationErrorSink` property (`:14-15` and `:13-17` respectively). Their doc comments
explicitly state the naming is deliberate so that no shared contract with
`EfcFormController.BoundaryErrorSink` is implied. Both are reached from
`InitializeWebViewGuardedAsync` (`:25-42` / `:24-42`).

**Existing call sites of `TryReportBoundaryFault`** (all in `EfcFormController.cs`): `:556`, `:573`,
`:591`, `:653`, `:668` (the five `async void` click-handler boundaries), `:1015` (`RunKbdGuardedAsync`),
`:1127` (`BindBreadcrumbRowsAsync`), `:1270` (`PopulateFolderCombobox`).

**`EfcFormController.InitializeBreadcrumbHostAsync` — `:1070-1082`:**

```csharp
// Fire-and-forget host initialization with an error boundary (the router queues every
// outbound payload until CoreWebView2InitializationCompleted fires).
private async Task InitializeBreadcrumbHostAsync()
{
    try { await _breadcrumbHost.InitializeAsync(_formViewer.UiSyncContext); }
    catch (System.Exception ex)
    {
        logger.Error($"Breadcrumb WebView2 initialization failed: {ex.Message}", ex);  // :1080
    }
}
```

Genuinely fire-and-forget (`_ = InitializeBreadcrumbHostAsync();` at `:1067`), with a total catch,
and **log-only**. AC-U4's claim holds for this member. Note the comment at `:1070-1071` is
now inaccurate: the queue is *not* released "until initialization fires" — on failure it is never
released at all.

**`EfcFormController.PopulateFolderCombobox` — `:1250-1272`:**

```csharp
/// <summary>#464 C: both call sites discard the result, so the boundary is here.</summary>
public async Task PopulateFolderCombobox(object folderList = null)
{
    try { ... }
    catch (System.Exception ex) { TryReportBoundaryFault(ex.Message, ex); }   // :1270
}
```

It **already** reports through `TryReportBoundaryFault`. AC-U4's claim is **false for this member**;
that half of AC-U4 is already satisfied on `main`. Its two call sites, `:95` and `:115`, do discard
the task, which is why the boundary is inside the method.

**What routing `InitializeBreadcrumbHostAsync` through the boundary takes:** nothing structural —
replace the `logger.Error(...)` at `:1080` with `TryReportBoundaryFault($"Breadcrumb WebView2 initialization failed: {ex.Message}", ex)`.
Both are instance members of the same type, so no new seam is needed. The only design decision is
whether an `OperationCanceledException` should be classified as non-fault, matching the precedent in
`BindBreadcrumbRowsAsync` (`:1121-1124`, `logger.Debug` for cancellation) and
`RunKbdGuardedAsync`; the existing test
`QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs:174-200` pins that classification for
the keyboard guard, so mirroring it is the consistent choice.

---

## Q5 — Pop-out path and the issue-678 carry pattern

**What the pop-out hands over.** `QuickFiler/Controllers/QfcCollectionController.cs`, both overloads:

`PopOutControlGroup(int selection)` — `:710-720`:
```csharp
MailItem mailItem = _itemGroups[selection - 1].MailItem;
RemoveSpecificControlGroup(selection);
var popOutForm = new EfcHomeController(_globals, () => { }, mailItem);
popOutForm.Run();
```

`PopOutControlGroupAsync(int selection)` — `:722-735`: identical except
`await RemoveSpecificControlGroupAsync(selection)` (`:730`) and `await popOutForm.RunAsync()` (`:734`).

So exactly three things cross the boundary: `_globals`, an empty cleanup lambda, and the raw
`MailItem`. The already-built `QfcItemGroup` — which holds `ItemController`, `ItemViewer`,
`PredeterminedFolder` and `CarriedFolderHandler` (`QuickFiler/Controllers/QfcItemGroup.cs:32-59`) —
is discarded. **Confirmed.**

**The #678 carry pattern.**

- Carried object: `IFolderSearchHandler` (interface at `UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs:14-39`;
  `FolderPredictor` implements it via the marker partial `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.IFolderSearchHandler.cs:10`).
- Storage on the group: `internal IFolderSearchHandler CarriedFolderHandler { get; set; }` —
  `QuickFiler/Controllers/QfcItemGroup.cs:59`.
- Storage on the controller: `private IFolderSearchHandler _carriedFolderHandler;` —
  `QuickFiler/Controllers/QfcItemController.cs:259` (doc at `:251-258`).
- Construction sites: `QuickFiler/Controllers/QfcCollectionController.CarrierLoad.cs:124-156`
  (`EncapsulateItemGroup`, carry parameter at `:131`, assigned at `:137`, forwarded to the controller
  at `:152`), and `QuickFiler/Controllers/QfcQueue.Enqueue.cs:55` and `:180-190`
  (`ResolveCarriedHandler`).
- Receiver contract: `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:57-157`,
  `LoadFolderHandlerAsync(CancellationToken cancel, object varList = null)`. The adoption is confined
  to the `varList is null` branch (`:60-86`): `cancel.ThrowIfCancellationRequested()` then
  `_folderHandler = _carriedFolderHandler;` at `:79`, then return. The doc at `:62-67` states the
  contract: the carried handler must already be initialised for *this* item with the same
  `FolderPredictor.InitOptions.FromField` sequence the branch would otherwise run. Release:
  `_carriedFolderHandler = null;` at `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:433`.
- Constructor plumbing: `QuickFiler/Controllers/QfcItemController.Initialization.cs:52` and `:102`,
  both `IFolderSearchHandler carriedFolderHandler = null` as a trailing optional parameter, assigned
  at `:55` and `:116`.

**What the pop-out would have to carry to match.** The Efc analogue of `_folderHandler` is
`EfcDataModel.FolderHelper`, and the Efc analogue of `ItemHelper` is `EfcDataModel.MailInfo`. The
pop-out would need to carry (a) the source `QfcItemController`'s initialized folder handler and
(b) its loaded `MailItemHelper`, and deposit both before `EfcFormController.Initialize()` runs.

**Current receiver surface — exact signatures.**

`QuickFiler/Controllers/EfcHomeController.cs` (447 lines):
```csharp
public EfcHomeController(IApplicationGlobals globals, System.Action parentCleanup, MailItem mail = null)          // :47-52
internal EfcHomeController(IApplicationGlobals globals, System.Action parentCleanup,
                           EfcHomeControllerDependencies dependencies, MailItem mail = null)                      // :54-95
private EfcHomeController(IApplicationGlobals globals, System.Action parentCleanup)                               // :97-102
public static async Task<EfcHomeController> CreateAsync(IApplicationGlobals globals,
                           System.Action parentCleanup, MailItem mail = null)                                     // :104-111
internal static async Task<EfcHomeController> CreateAsync(IApplicationGlobals globals,
                           System.Action parentCleanup, EfcHomeControllerDependencies dependencies,
                           MailItem mail = null)                                                                  // :113-138
```

`QuickFiler/Controllers/EfcDataModel.cs` (499 lines):
```csharp
public EfcDataModel(IApplicationGlobals globals, MailItem mail,
                    CancellationTokenSource tokenSource, CancellationToken token)                                 // :48-81
private EfcDataModel(IApplicationGlobals globals, MailItem mail)                                                  // :83-87
public static async Task<EfcDataModel> CreateAsync(IApplicationGlobals globals, IList<MailItem> mailItems,
                    CancellationTokenSource tokenSource, CancellationToken token, bool loadAll)                   // :89-142
public FolderPredictor FolderHelper { get; protected set; }                                                       // :177-186
public async Task InitFolderHandlerAsync(object folderList = null)                                                // :188-221
public MailItemHelper MailInfo => ConversationResolver?.MailHelper;                                                // :241
```

**Answer: neither the synchronous constructor nor `CreateAsync` can accept a carry today.**
`FolderHelper` has a `protected set`, `ConversationResolver` has a `protected set` (`:224-228`), and
no constructor or factory takes a folder handler or a `MailItemHelper`. Note also that the two
constructors and `CreateAsync` differ in *what* they build, not in whether they accept a carry: the
sync constructor at `:48-81` loads the conversation resolver synchronously (`:68`,
`_conversationResolver.LoadDf()`), while `CreateAsync` at `:89-142` awaits
`ConversationResolver.LoadAsync` (`:116-134`). The pop-out uses the sync constructor indirectly:
`EfcHomeController` internal ctor `:66-71` calls `_dependencies.DataModelFactory(...)`, whose
production binding is `EfcHomeControllerDependencyFactories.cs:20` →
`CreateProductionDataModel`, that is, the 4-argument public `EfcDataModel` constructor.

**Deposit-point constraint (important for the plan).** `EfcFormController.Initialize()`
(`EfcFormController.cs:79-97`) fires `_ = PopulateFolderCombobox();` at `:95`, and
`InitializeDataFields(EfcDataModel)` (`:111-117`) fires it again at `:115`.
`PopulateFolderCombobox` calls `_dataModel.InitFolderHandlerAsync(folderList)` (`:1262`), which
unconditionally overwrites `FolderHelper` (`EfcDataModel.cs:194`, `:198-206`, `:211-219`). A carry
deposited *after* construction would therefore race a fire-and-forget task that overwrites it. The
carry must either be supplied before the form controller is constructed — that is, between
`EfcHomeController.cs:71` (`DataModel = ...`) and `:85` (`FormControllerWithDataFactory(...)`) — or
be consumed inside `InitFolderHandlerAsync` itself, mirroring
`QfcItemController.LoadFolderHandlerAsync`'s `varList is null` branch. The second is the closer
analogue of #678 and is the recommendation.

**Type blocker to resolve in the plan.** The QFC carry is typed `IFolderSearchHandler`
(`QfcItemController.cs:259`), but `EfcDataModel.FolderHelper` is the concrete `FolderPredictor`
(`:177-186`). Retyping `FolderHelper` to `IFolderSearchHandler` is *almost* free — the three Efc
consumers are `EfcFormController.cs:1117` (`Suggestions`), `:1266` (`FolderArray`), and
`EfcDataModel.FindMatches` `:483-488` (`FindFolder`, whose named arguments match the interface
declaration exactly) — but it is blocked by one member: `EfcDataModel.RefreshSuggestions` at
`:491-495` calls `_folderHelper.RefreshSuggestions(mailItem: Mail)`, and
`FolderPredictor.RefreshSuggestions(object objItem, int topNfolderKeys = -1)`
(`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:967`) is **not** on `IFolderSearchHandler`.
The plan must choose one of: (a) widen `IFolderSearchHandler` with a `RefreshSuggestions` member,
(b) keep `FolderHelper` concrete and carry a `FolderPredictor` (which means exposing the QFC side as
`FolderPredictor` or performing a cast), or (c) keep the carry field separate from `FolderHelper`.

**Source-side accessor gap.** `QfcItemController._folderHandler` (`QfcItemController.cs:41`) is
private and `IQfcItemController` exposes no folder-handler member — the only read seam is
`TopFolderScore` (`QfcItemController.cs:265`). `IQfcItemController.ItemHelper` **is** exposed
(`QuickFiler/Interfaces/IQfcItemController.cs:41`, `MailItemHelper ItemHelper { get; set; }`), so the
`MailItemHelper` half of the carry needs no new accessor; the folder-handler half does.

---

## Q6 — UI-thread construction

**Claim 1 — "`EfcViewerQueue.BuildQueue` has no production call site": CONFIRMED for that member,
but the conclusion drawn from it is REFUTED.**

- `public static void BuildQueue(int count)` — `QuickFiler/Helper Classes/EfcViewerQueue.cs:29-32`.
  Grep for `BuildQueue` across all `*.cs` finds only `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:43`
  as a caller of this static. **No production caller.**
- However, `ViewerQueueCore<TViewer>.Dequeue` **itself** rebuilds the queue:
  `QuickFiler/Helper Classes/ViewerQueueCore.cs:63-85` calls `BuildQueue(cachedReplacementCount, replacementPriority)`
  at `:78` on the cached path and `BuildQueue(emptyReplacementCount, replacementPriority)` at `:83`
  on the empty path.
- `EfcViewerQueue.Dequeue()` (`:34-43`) passes `emptyQueuePriority: Render`,
  `cachedReplacementCount: 1`, `emptyReplacementCount: 2`, `replacementPriority: Background`.
- `ViewerQueueCore.BuildQueue(int, DispatcherPriority)` (`:52-61`) schedules through
  `_priorityScheduler`, whose production binding for the Efc queue is
  `EfcViewerQueue.cs:16-20`: `(action, priority) => _ = UiThread.Dispatcher.InvokeAsync(action, priority)`.

So the queue **is** populated in production, on the WPF `UiThread.Dispatcher`. Only the **first**
`Dequeue` in a process finds the queue empty.

**Claim 2 — "`EfcViewer` is therefore always constructed inline on the calling thread": REFUTED as
stated; TRUE only for the first Efc open.** The empty-queue path calls
`ViewerQueueCore.CreateWithPriority` (`:126-142`), which uses `_blockingPriorityScheduler`. For the
Efc queue that binding is `EfcViewerQueue.cs:25`:
`ProductionBlockingPriorityScheduler = (action, priority) => action();` — literally inline, with the
`priority` argument discarded. (Contrast `ItemViewerQueue.cs:89-90`, whose blocking scheduler is
`UiThread.Dispatcher.Invoke(action, priority)`.) The first `EfcViewer` is therefore built on
whatever thread called `Dequeue`; every subsequent one is built on the WPF dispatcher thread.

`ProductionViewerFactory` is bound to `EfcViewerQueue.Dequeue` at
`QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:39-40` (and re-bound at `:112`), and
`EfcHomeControllerDependencies.ViewerFactory` defaults to it at `EfcHomeControllerDependencies.cs:68`.
`EfcHomeController` calls it at `:77` (sync ctor) and `:226` (`InitAsync`).

**Claim 3 — "captures `SynchronizationContext.Current` at construction": CONFIRMED.**
`QuickFiler/Viewers/EfcViewer.cs:23-30`:
```csharp
public EfcViewer()
{
    InitializeComponent();
    _context = SynchronizationContext.Current;                          // :26
    _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();   // :27
    InitTipsLabelsList();
}
```
Exposed as `UiSyncContext` at `:37-40`. This is the value `EfcFormController` passes to
`_breadcrumbHost.InitializeAsync(_formViewer.UiSyncContext)` at `:1076`.

**Can the pop-out continuation land off the UI thread?** Not established either way, and the answer
matters less than expected. Two findings:

1. **A null context fails earlier and harder than the issue says.**
   `TaskScheduler.FromCurrentSynchronizationContext()` at `EfcViewer.cs:27` throws
   `InvalidOperationException` when `SynchronizationContext.Current` is null. So an off-UI-thread,
   context-free pop-out continuation fails inside the `EfcViewer` **constructor**, before
   `UiThread.SynchronizationContextAwaiter` is ever reached. The issue's cited throw site is real —
   `UtilitiesCS/Threading/UiThread.cs:146-153`, `SynchronizationContextAwaiter(SynchronizationContext? context)`
   throws `ArgumentNullException(nameof(context))` on a null context — but it is the *second* failure
   on that path, not the first. (Note: the issue cites `UiThread.cs:91-98`; the struct now spans
   `:140-196`.)
2. **The pop-out's awaits mostly resume on the dispatcher.**
   `QfcCollectionController.PopOutControlGroupAsync` (`:722-735`) awaits
   `RemoveSpecificControlGroupAsync(selection)` (`:913-1012`) before constructing the home
   controller. That method's own awaits are `ToggleOffActiveItemAsync(false)` (`:925`) and
   `UiThread.Dispatcher.InvokeAsync(...)` (`:951`). I did not trace `ToggleOffActiveItemAsync` to a
   terminal `ConfigureAwait(false)`, so **I could not verify** that the continuation reaches
   `new EfcHomeController(...)` off the UI thread. Treat "the pop-out continuation lands off the UI
   thread" as **unverified**.

**The guard or seam a fix would use.** The repository already ratified the answer for the sibling
viewer under issue #781 (`docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781/issue.md`,
AC1-AC3): prove UI ownership by **owner-thread identity** (the `Dispatcher` captured at construction,
via `CheckAccess()`, or the constructing thread's managed thread id) rather than by
`SynchronizationContext` reference equality, because dispatcher-built viewers capture a
`DispatcherSynchronizationContext` that is never the thread's ambient context again. For #792 the
concrete seam is:
- construct the `EfcViewer` through `UiThread.Dispatcher.Invoke` rather than inline — that is, change
  `EfcViewerQueue.ProductionBlockingPriorityScheduler` (`EfcViewerQueue.cs:25`) to match
  `ItemViewerQueue.cs:89-90`; this is a one-line change in a 101-line file, and
  `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs` already substitutes that
  scheduler (`:244-261`), so it is directly testable; and/or
- assert that `_formViewer.UiSyncContext` is non-null before
  `_breadcrumbHost.InitializeAsync(...)` at `EfcFormController.cs:1076`, failing through
  `TryReportBoundaryFault` instead of letting `ArgumentNullException` escape a fire-and-forget task.

---

## Q7 — Archive-root guard

**The issue's factual claim is true; its implied severity is stale.**

`EfcFormController.BindBreadcrumbRowsAsync` — `EfcFormController.cs:1111-1129`:
```csharp
internal async Task BindBreadcrumbRowsAsync(string[] rows)
{
    try
    {
        var scores = _dataModel?.FolderHelper?.Suggestions?.ToScoredArray() ?? Array.Empty<FolderScore>();
        await _router.BindRowsAsync(rows, scores, _globals.Ol.ArchiveRootPath, Token);   // :1119
    }
    catch (OperationCanceledException) { logger.Debug("Breadcrumb bind canceled."); }     // :1121-1124
    catch (System.Exception ex) { TryReportBoundaryFault($"Breadcrumb bind failed: {ex.Message}", ex); }  // :1127
}
```

- Confirmed: `_globals.Ol.ArchiveRootPath` is read directly at `:1119`, not through a `Try...` helper.
- Confirmed: `EfcDataModel.TryGetArchiveRoot(out string archiveRoot)` exists at
  `QuickFiler/Controllers/EfcDataModel.cs:280-297` (doc `:271-279`, message constant `:267-269`) and
  is **not** used on the bind path. Its three call sites are all filing/opening paths:
  `MoveToFolderAsync` `:327`, `OpenOlFolderAsync` `:370`, `OpenFsFolderAsync` `:394`. It is also
  `private`, so `EfcFormController` cannot call it without an access change.
- **But the read is already fail-soft and already user-surfaced.** The documented
  `InvalidOperationException` is caught by the general `catch` at `:1125` and routed through
  `TryReportBoundaryFault` at `:1127`, whose default sink surfaces to the user
  (`DefaultBoundaryErrorSink` `:137-141` → `UserFaultNotifier` `:181-185`). This behaviour is pinned
  by an existing test:
  `QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs:241-271`,
  `BindBreadcrumbRowsAsync_WhenArchiveRootThrows_ReportsOnceAndDoesNotThrow`, which asserts no
  throw, exactly one sink call, and `ArchiveRootPath` read exactly once. A second existing test,
  `QuickFiler.Test/Controllers/EfcFormControllerTests.cs:61-...`,
  `Issue439BindBreadcrumbRowsAsync_SubmitsArchiveRootToRealRouter`, pins the success path.

**The remaining behavioural gap** is not the absence of a guard but the *shape* of the degradation:
`TryGetArchiveRoot` degrades to "no archive root, continue" (returns false, caller decides), whereas
the bind boundary aborts the whole bind, so an unresolvable archive root produces an **empty folder
list** — the same user-visible symptom #792 reports, from a different cause. Whether to converge on
the `TryGetArchiveRoot` semantics (bind with an empty root, which
`BreadcrumbBridgeRouter.BindRowsAsync` already tolerates: `_boundRoot` empty at
`BreadcrumbBridgeRouter.cs:115-117`, and the pass-through branches at `:192-195` and `:260-265`) is a
design decision the plan should make explicitly.

**Note on the two commits named in the assignment.** The Bash tool is disabled in this session, so I
**could not verify** commits `f50fb7271` and `655130c5a` by SHA. What I verified instead is the
current source state described above, which is what the plan must be written against. The `#799 AC4`
and `#736 finding 4/5` annotations present in the files (`QfcItemController.FolderHandling.cs:226-230`,
`EfcFormController.cs:131-136`, `:170-172`, `:176-180`) are consistent with archive-root and
boundary-sink work having already landed.

Related sibling, already open: `docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/issue.md`
covers the QFC twin of this read, and the QFC side is already guarded at
`QfcItemController.FolderHandling.cs:231-245` with an explicit `catch (InvalidOperationException)`
that degrades to `string.Empty`.

---

## Q8 — Testability and existing test coverage

All test classes below are in project **`QuickFiler.Test`**, csproj
`QuickFiler.Test/QuickFiler.Test.csproj`, which lists every source file explicitly (no globbing).

| Production type | Test classes | csproj `Compile` line |
|---|---|---|
| `WebView2BreadcrumbHost` | `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` (`WebView2BreadcrumbHostTests`), `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs` | `:214` (and the contract file, listed separately) |
| `BreadcrumbBridgeRouter` | `BreadcrumbBridgeRouterTests.cs`, `BreadcrumbBridgeRouterTests.Selection.cs`, `BreadcrumbBridgeRouterQueueTests.cs`, `BreadcrumbBridgeRouterQueueTests.Part2.cs`, `BreadcrumbBridgeRouterIssue439Tests.cs`, `...Issue439Tests.Activation.cs`, `...Issue614Tests.cs`, `...Issue637Tests.cs`, `BreadcrumbBridgeRouterScoreJoinTests.cs` (all under `QuickFiler.Test/Controllers/`) | `:60`, `:61` and adjacent |
| `EfcFormController` | `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` and `EfcFormControllerTests.Part2.cs` (one `partial class EfcFormControllerTests`, namespace `QuickFiler.Controllers.Tests`) | `:124`, `:125` |
| `EfcDataModel` | `EfcDataModelTests.cs`, `EfcDataModelArchiveRootTests.cs`, `EfcDataModelIssue614Tests.cs` | `:122`, `:123`, `:121` |
| `QfcCollectionController` | `QfcCollectionControllerTests.cs`, `QfcCollectionControllerTests.Part2.cs`, `QfcCollectionController.TestSupport.cs`, plus `...DarkModeTests`, `...Defects468*Tests`, `...Layout.StaTests`, `...NavigationDigitsTests`, `...NavigationLedgerTests` | `:139`, `:165` and adjacent |
| `EfcViewerQueue` / `ViewerQueueCore` | `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs`, `ViewerQueueCoreTests.cs` | `:226` and adjacent |

**Infrastructure already proven in this project** (this is the decisive enabling fact):

- Real `Microsoft.Web.WebView2.WinForms.WebView2` controls are constructed in unit tests on
  `WinFormsPumpHost`: `WebView2BreadcrumbHostTests.cs:36-52`. Constructing the control needs no
  Evergreen runtime; only `EnsureCoreWebView2Async` / `CoreWebView2Environment.CreateAsync` do.
- `Mock<IWebViewCoreInitializer>` drives `WebView2BreadcrumbHost.InitializeAsync` end-to-end without
  reaching the SDK: `WebView2BreadcrumbHostTests.cs:399-419` (`BuildCompletingInitializer`), which
  returns `Task.FromResult<CoreWebView2Environment>(null)` and `Task.CompletedTask`.
- `RecordingSynchronizationContext` (`WebView2BreadcrumbHostTests.cs:428-438`) counts posts without
  draining them.
- `EfcFormController` is constructible headlessly via its private no-arg constructor:
  `EfcFormControllerTests.cs:24-34` (`CreateMinimalController`), with `SetPrivateField` for
  `_globals` / `_router`.
- The Efc viewer queue's four production scheduler delegates are settable seams:
  `EfcViewerQueue.cs:10-25`, with `SetCoreForTesting` (`:48-51`),
  `ResetCoreForTesting` (`:56-60`) and `ResetProductionCoreDefaultsForTesting` (`:62-69`), already
  exercised at `ViewerQueueStaticWrapperTests.cs:18-19, 41-43, 75-77, 244-261`.

**Reachability of Q1-Q7 behaviours from a unit test today:**

| Behaviour | Reachable today? | Seam |
|---|---|---|
| Q1 failure branch of `OnCoreInitializationCompleted` | **No, and not makeable reachable.** `CoreWebView2InitializationCompletedEventArgs` has no public constructor. | none — must move the retry/surfacing logic to the awaited-task path |
| Q1 failure of `InitializeAsync` / `InitializeBreadcrumbHostAsync` | **Yes** | `Mock<IWebViewCoreInitializer>` whose `EnsureCoreWebView2Async` returns a faulted task |
| Q2 options parity across the three sites | **Yes, as a structural test** | assert the three option sets agree; the two seam sites are verifiable through `Mock<IWebViewCoreInitializer>.Verify(CreateEnvironmentAsync(folder, It.Is<CoreWebView2EnvironmentOptions>(o => o.AdditionalBrowserArguments == expected)))`; the third (`EfcItemController.cs:194`) bypasses the seam and needs one first |
| Q3 `_pendingDocument` stash and drain | **Yes, already** | `Mock<IBreadcrumbWebHost>` with `IsCoreInitialized` false then true; existing precedent `BreadcrumbBridgeRouterQueueTests.cs:117, 450-455` calls `NotifyCoreInitialized()` |
| Q3 outbound-queue drain | **Yes, already** | `BreadcrumbOutboundQueue.PendingCount` (`:29`) is public |
| Q4 boundary routing of `InitializeBreadcrumbHostAsync` | **New seam needed** | the member is `private` and reads `_breadcrumbHost` / `_formViewer`; make it `internal` and give the host field an injectable type (`IBreadcrumbWebHost` + a `Func<SynchronizationContext>` or an initializer delegate), or extract the retry policy into a testable helper |
| Q5 pop-out carry | **New seam needed** | `PopOutControlGroupAsync` news up `EfcHomeController` directly (`QfcCollectionController.cs:732`); needs a `Func<...,EfcHomeController>` factory seam, plus a read accessor for `QfcItemController._folderHandler` |
| Q6 inline vs. dispatcher viewer construction | **Yes, already** | `EfcViewerQueue.ProductionBlockingPriorityScheduler` / `ProductionPriorityScheduler` |
| Q7 archive-root throw at the bind boundary | **Yes, already covered** | `EfcFormControllerTests.Part2.cs:241-271` |

**AC-by-AC automatability:**

| AC | Automatable | Seam required |
|---|---|---|
| AC-U1 (retry; visible error state on final failure) | **Yes**, provided the retry lives in `InitializeBreadcrumbHostAsync` (or an extracted policy object) rather than in the SDK event handler | `Mock<IWebViewCoreInitializer>` returning a faulted task N times then a completed one; plus an `internal` entry point on `EfcFormController` and an injectable host/initializer. The "visible error state" must be asserted at the router/renderer level (a banner row via `BreadcrumbRowBuilder.BannerPrefix = "===="`, `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19`; banner rows are already non-selectable, `BreadcrumbBridgeRouter.Selection.cs:85-88`), not at the WebView2 level |
| AC-U2 (`_pendingDocument` never silently dropped) | **Yes** | `Mock<IBreadcrumbWebHost>` only; no new seam. Assert (a) a later successful `NotifyCoreInitialized` navigates the stash, and (b) a new failure notification produces an error document and leaves no stash. Extend the same test to `BreadcrumbOutboundQueue.PendingCount` |
| AC-U3 (pop-out carries predictor + `MailItemHelper`; `EfcViewer` on the UI thread) | **Partly.** The carry half is automatable; the "constructs on the UI thread" half is automatable only as a scheduler-delegate assertion, not as a real thread-affinity assertion | carry half: an `EfcHomeController` factory seam on `QfcCollectionController` + a folder-handler accessor on `QfcItemController`/`IQfcItemController` + a carry parameter path into `EfcDataModel.InitFolderHandlerAsync`. UI-thread half: `EfcViewerQueue.ProductionBlockingPriorityScheduler` substitution, recording the priority and the fact that the action was scheduled rather than run inline |
| AC-U4 (`PopulateFolderCombobox` and `InitializeBreadcrumbHostAsync` report through `TryReportBoundaryFault`) | **Yes** | `PopulateFolderCombobox` already satisfies it and is already covered (`EfcFormControllerTests.cs:300-...`, `PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault`) — that test asserts "logs once", so it will need strengthening to assert the sink. `InitializeBreadcrumbHostAsync` needs the seam described under AC-U1; then `BoundaryErrorSink` substitution is the assertion point |
| AC-U5 | **Not automatable** (manual live-Outlook), as instructed | — |

Policy constraints observed throughout: MSTest + Moq + FluentAssertions, no temporary files, no live
Outlook, no `Thread.Sleep`/`Task.Delay`. None of the proposed tests need a temporary file; the
`%LocalAppData%` cache folder is only ever *computed* as a string in the testable path, never created
(`WebView2BreadcrumbHost.cs:246-249` computes it and hands it to the mocked seam).

---

## Q9 — File-size and project-file constraints

**Measured line counts (re-derived in this session):**

| File | Lines | Status |
|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2329 | far over the 500 ceiling |
| `QuickFiler/Controllers/EfcFormController.cs` | 1321 | over the ceiling; any edit obliges a split |
| `QuickFiler/Controllers/EfcDataModel.cs` | 499 | **one line under**; any addition forces a split |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | 368 | headroom ~132 |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | 221 | headroom |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 407 | headroom ~93 |
| `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` | 285 | headroom |
| `QuickFiler/Viewers/EfcViewer.cs` | 169 | headroom (but `[ExcludeFromCodeCoverage]` at `:20`) |
| `QuickFiler/Helper Classes/EfcViewerQueue.cs` | 101 | headroom |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 310 | headroom |
| `QuickFiler/Controllers/EfcHomeController.cs` | 447 | headroom ~53 — tight |
| `QuickFiler/Controllers/EfcHomeControllerDependencies.cs` | 428 | headroom ~72 — tight |
| `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs` | 67 | headroom |

**Orchestrator re-measurement, 2026-09-12 (three additions and one correction to the table above).**
The following counts were taken independently by the orchestrator against the same HEAD and supersede
any figure carried from prior research:

| File | Lines | Status |
|---|---|---|
| `QuickFiler/Controllers/EfcItemController.cs` | 1121 | **over the 500 ceiling.** The root-cause remedy edits this file, so the edit obliges a split of it. The Q9 table above omitted this file and the risk list did not record the obligation. |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 467 | headroom 33. Prior research recorded 499; that figure is **wrong**. A line-neutral edit is still preferred, but the file is not at the ceiling. |
| `QuickFiler/Controllers/QfcItemController.cs` | 334 | headroom |
| `QuickFiler/Controllers/EfcHomeControllerDependencies.cs` | 428 | headroom 72 — tight |

`QuickFiler/Controllers/EfcHomeController.cs` is confirmed at 447 lines. Note also that prior drafts of
this document spelled its path as a repository-root sibling; the tracked path is
`QuickFiler/Controllers/EfcHomeController.cs` and has been corrected throughout.

**`Compile` item XML shape.** `QuickFiler/QuickFiler.csproj` is a non-SDK-style project with an
explicit item list. The plain shape is a self-closing element inside the `<ItemGroup>`:

```xml
    <Compile Include="Controllers\EfcFormController.cs" />
```

(`QuickFiler.csproj:296`). Paths use backslashes and are project-relative.

**Are partial-class files listed individually?** Yes, each on its own line, with no metadata. Direct
examples: `Controllers\BreadcrumbBridgeRouter.cs` `:291`, `Controllers\BreadcrumbBridgeRouter.Arrows.cs`
`:292`, `Controllers\BreadcrumbBridgeRouter.Selection.cs` `:293`; `Controllers\EfcDataModel.cs` `:289`
and `Controllers\EfcDataModel.FilingStem.cs` `:290`; the twelve `QfcItemController.*.cs` parts at
`:333-344`; the five `QfcFormController.*.cs` parts at `:321-325`.

**Is there a `DependentUpon` convention for partial files?** **No.** Every `DependentUpon` in the file
is a Designer/resx pairing: `:381` (`Resources.resx`), `:386` (`Settings.settings`), `:392`, `:398`,
`:431`, `:435`, `:439`, `:443`, `:447`, `:451`, `:457`, `:463`, `:469`, `:475`, `:503`, `:506`,
`:510`, `:514`, `:518`, `:522`. Hand-written partial parts carry **no** metadata and **no**
`DependentUpon`. A new partial part must therefore be added as a bare self-closing `<Compile Include="..." />`.

**Proposed split of `EfcFormController.cs`.** The type is declared at `:26` as
`internal class EfcFormController : IFilerFormController`; the split requires adding `partial` there.
The existing `#region` boundaries map cleanly onto files, and the names follow the
`QfcFormController.*` / `QfcItemController.*` convention already in the tree.

| New file (all under `QuickFiler/Controllers/`) | Source span moved | Members | Approx. lines |
|---|---|---|---|
| `EfcFormController.cs` (retained) | `1-264` | usings, `partial class` declaration, `#region Constructors` (`:28-119`: two public ctors, private no-arg ctor, `Initialize`, `InitializeWithoutData`, `InitializeDataFields`), `#region Private Properties` (`:121-264`: `logger`, `BoundaryErrorSink`, `DefaultBoundaryErrorSink`, `TryReportBoundaryFault`, `_userFaultNotifier`, `UserFaultNotifier`, `ShowModelessFaultNotice`, all fields) | ~267 |
| `EfcFormController.SetupAndProperties.cs` | `266-479` | `#region Setup and Cleanup Methods` (`CaptureConfigureItemViewer`, `Cleanup`, `ConfigureFind`, `ResolveControlGroups`, `SetupThemes`) and `#region Public Properties` (`LoadTheme`, `FormHandle`, the remaining properties) | ~240 |
| `EfcFormController.EventHandlers.cs` | `481-834` | `#region Event Handlers`: `RegisterAlwaysOnAsyncKeyActions`, `WireEventHandlers`, `SearchText_DownArrow`, the five `async void` click handlers and their `...ClickAsync` cores, the four `CheckedChanged` handlers, `SearchText_TextChanged`, `EditFiltersMenuItem_Click`, `CharacterAsyncActions`/`GetAsyncCharacterActions`, `CharacterActions`/`GetKbdActions`, `DarkMode_Changed` | ~380 |
| `EfcFormController.Actions.cs` | `836-990` | `#region Major Actions`: `ActionOkAsync`, `ActionCancelAsync`, `WithTrashRow`, `ApplyDeleteGesture`, `ActionDeleteAsync`, `CreateFolderAsync`, `MatchesForSearchText`, `RefreshSuggestionsAsync` | ~180 |
| **`EfcFormController.Breadcrumb.cs`** | `1047-1129` + `1251-1287` | `ConfigureBreadcrumbControl`, `InitializeBreadcrumbHostAsync`, `BindFolderRows`, `BindSourceFolderRows`, `BindBreadcrumbRowsAsync`, `PopulateFolderCombobox`, `IsBannerRow`, `IsSelectableFolder`, `IsValidSelection` — **this is the only new file the #792 fix edits** | ~150 |
| `EfcFormController.Helpers.cs` | `992-1045` + `1131-1249` + `1289-1320` | `RunKbdGuardedAsync`, both `KbdExecuteAsync` overloads, `JumpToAsync`, `MaximizeFormViewer`, `MinimizeFormViewer`, `ShowMenu`, `ToggleCheckboxAsync`, the four `ToggleOn/OffNavigation*`, the three `ToggleTips*`, `LoadUserSettings`, `ToggleExpansionStyle` | ~230 |

Each resulting file is under 500 lines. Six `<Compile Include="Controllers\EfcFormController.*.cs" />`
lines must be added to `QuickFiler/QuickFiler.csproj` adjacent to `:296`, with no metadata. Because
`QuickFiler.Test` references the type only through `CreateMinimalController` reflection and
`internal` members (the project grants `InternalsVisibleTo`), no test file needs to change for the
split alone.

**`EfcDataModel.cs` at 499/500.** Stated explicitly: **any addition to this file forces a split.**
The precedent partial is `QuickFiler/Controllers/EfcDataModel.FilingStem.cs` (csproj `:290`). If the
AC-U3 carry lands in `InitFolderHandlerAsync`, the recommended split target is a new
`QuickFiler/Controllers/EfcDataModel.Carry.cs` holding the carry field, the carry-aware
`InitFolderHandlerAsync` (moved from `:188-221`, 34 lines), and the adoption doc — roughly 70 lines,
leaving `EfcDataModel.cs` at about 465. An alternative, if the plan prefers not to move a live
method, is to move `ArchiveRootUnavailableMessage` + `TryGetArchiveRoot` (`:263-297`, 35 lines) into
a new `QuickFiler/Controllers/EfcDataModel.ArchiveRoot.cs`, leaving `EfcDataModel.cs` at about 464.
Either way a csproj line is required.

**`QfcCollectionController.cs` at 2329.** The AC-U3 edit to `PopOutControlGroup` /
`PopOutControlGroupAsync` (`:710-735`) obliges a split of this file too. The existing precedent part
is `QuickFiler/Controllers/QfcCollectionController.CarrierLoad.cs` (csproj `:315`). The natural
target is a new `QuickFiler/Controllers/QfcCollectionController.PopOut.cs` holding just the two
pop-out members and the new factory seam (~60 lines). Note this does **not** bring the parent file
under 500; a full split of a 2329-line file is a much larger piece of work and should be declared
out of scope explicitly rather than attempted inside a bug fix.

---

## Q10 — Contention awareness (report only)

Two sibling items in the same parallel run touch adjacent surface.

**Sibling A — `CultureInfo.InvariantCulture` on date/time formatting, editing `QfcCollectionController.cs`.**
This is issue **#645**, folder `docs/features/active/2026-09-02-quickfiler-session-metrics-twelve-hour-time-format-645/`.
Its issue body states the numeric format calls already pass `CultureInfo.InvariantCulture` and the
date/time calls were left, and proposes adding it. The un-cultured format calls in
`QuickFiler/Controllers/QfcCollectionController.cs` are at:
- `:235` — `grp.ItemController.Mail.SentOn.ToString("MM/dd/yyyy")`
- `:1296` — `c.ItemHelper.SentDate.ToString("MM/dd/yyyy")` and `.ToString("HH:mm")`
- `:2302` — `qf.ItemHelper.SentDate.ToString("MM/dd/yyyy")` and `.ToString("HH:mm")`

**Collision risk: same file, disjoint line ranges.** #792's AC-U3 edit targets `:710-735`
(`PopOutControlGroup` / `PopOutControlGroupAsync`). There is no line overlap. **However**, if #792
splits `QfcCollectionController.cs` into a new `.PopOut.cs` partial (Q9), the split deletes lines
`710-735` from the parent file and adds a `<Compile>` line to `QuickFiler/QuickFiler.csproj`. Both
are textual changes to files #645 also edits, and the csproj is a single shared item list. Declare
`QuickFiler/Controllers/QfcCollectionController.cs` and `QuickFiler/QuickFiler.csproj` in #792's
write set and expect a merge with #645 on the csproj `<ItemGroup>` ordering.

**Sibling B — a UI-marshalling seam on `ItemViewer`.** The best match is issue **#781**, folder
`docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781/`,
which replaces `ItemViewer.ThrowIfOffUiBoundary`'s `SynchronizationContext` reference comparison with
owner-thread identity. Its **AC7 explicitly scopes it to `QuickFiler/Viewers/ItemViewer*.cs`
production files and their tests**, and explicitly forbids changing
`QfcCollectionController.LoadSecondaryAsync`, `QfcItemController.AssignFolderComboBox`, or
`QfcItemController.EnsureBreadcrumbPipeline`. A second candidate is issue **#489**
(`docs/features/active/2026-08-25-itemviewer-surface-defects-489/`).

**Collision risk with #792: low but non-zero, and conceptual rather than textual.**
- No file overlap. #792 touches `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`,
  `QuickFiler/Viewers/EfcViewer.cs` (possibly), `QuickFiler/Helper Classes/EfcViewerQueue.cs`,
  `QuickFiler/Controllers/EfcFormController*.cs`, `QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs`,
  `QuickFiler/Controllers/EfcDataModel*.cs`, `QuickFiler/Controllers/QfcCollectionController*.cs`,
  `QuickFiler/Controllers/QfcItemController*.cs` (accessor), and `QuickFiler/QuickFiler.csproj`.
  #781 touches `QuickFiler/Viewers/ItemViewer*.cs` only. **Disjoint except the csproj**, and #781
  adds no file, so even that is unlikely.
- **Conceptual overlap**: both items decide how "am I on the UI boundary?" is proven.
  `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:255-278` (`IsCurrentBoundary`) uses context reference
  equality with a currently-executing-callback escape hatch, and #781's issue body explicitly
  assesses it as self-consistent and out of scope. #792's Q6 remedy should adopt the same
  owner-thread-identity idiom #781 ratifies rather than inventing a third convention, but should not
  edit `BreadcrumbUiDispatcher.cs` unless a test proves a need.
- **Third-party overlap worth flagging**: `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:61`
  (the `--incognito` options site #792 must touch) sits 40 lines from
  `QfcItemController.ViewerSetup.cs:113-122`, the breadcrumb drop-down attach path #781's manual
  retest step names. The file is 499 lines (per prior research), so it has essentially **no headroom**
  — a #792 edit that adds lines there forces yet another split. Prefer replacing the literal at `:61`
  with a reference to a shared constant, which is line-neutral.

---

## Corrections to the issue body

| Issue-body claim | Current reality |
|---|---|
| `BreadcrumbBridgeRouter.DeliverDocument` at `BreadcrumbBridgeRouter.Selection.cs:168-180` | **Correct as written.** `DeliverDocument` is at `:168-180`. |
| `WebView2BreadcrumbHost` "(`:330-342`) returns on `!e.IsSuccess`" | Close but off by one at the start: `OnCoreInitializationCompleted` spans `:329-354` (attribute at `:329`, signature `:330-333`); the `!e.IsSuccess` block is `:335-342` and the `return` is `:341`. |
| `EfcFormController.InitializeBreadcrumbHostAsync (:1071-1081)` | Now `:1072-1082` (comment at `:1070-1071`). File is 1321 lines, not 1181. |
| `EfcFormController.PopulateFolderCombobox (:1250-1271)` "fire-and-forget with total catch blocks, so the failure is log-only" | Span is now `:1250-1272` (doc comment `:1250`, signature `:1251`). **The substantive claim is false**: `:1270` already calls `TryReportBoundaryFault`. Only `InitializeBreadcrumbHostAsync` is log-only. |
| `EfcFormController.BindBreadcrumbRowsAsync (:1115-1118)` reads `_globals.Ol.ArchiveRootPath` unguarded | Span is now `:1111-1129`; the read is at `:1119`. The read is literally unguarded, but it is inside a `try` whose `catch` routes to `TryReportBoundaryFault` at `:1127`, and that behaviour is pinned by `QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs:241-271`. |
| `TryGetArchiveRoot (EfcDataModel.cs:280-297)` | **Correct.** `:280-297`, doc `:271-279`. Also note it is `private`, so `EfcFormController` cannot call it as-is. |
| `QfcCollectionController.PopOutControlGroup (:710-735)` | `PopOutControlGroup` is `:710-720`; `PopOutControlGroupAsync` is `:722-735`. The cited range covers both, which is probably intended. File is 2329 lines. |
| `EfcDataModel.cs:48-81` (sync ctor) vs `CreateAsync :89-142` | **Both correct.** |
| `QfcItemController.FolderHandling.cs:68-83`, `_carriedFolderHandler` | **Correct.** The carry branch is `:68-86` with the assignment at `:79`; `:68-83` covers the guard and the assignment. |
| "`EfcViewerQueue.BuildQueue` has no production call site, so `EfcViewer` is always constructed inline on the calling thread" | **Half wrong.** The public `EfcViewerQueue.BuildQueue(int)` (`:29-32`) indeed has no production caller, but `ViewerQueueCore.Dequeue` calls the internal `BuildQueue(count, priority)` overload at `ViewerQueueCore.cs:78` and `:83`, scheduling through `UiThread.Dispatcher.InvokeAsync` (`EfcViewerQueue.cs:20`). Only the **first** `EfcViewer` in a process is built inline; the rest are built on the WPF dispatcher. The inline behaviour comes from `ProductionBlockingPriorityScheduler = (action, priority) => action();` at `EfcViewerQueue.cs:25`, not from the absence of `BuildQueue`. |
| `EfcViewer.cs:23-30` captures the context | **Correct.** `:26` captures the context; `:27` also captures a `TaskScheduler`, which **throws** if the context is null. File is 169 lines. |
| "`UiThread.SynchronizationContextAwaiter` throws on the null context (`UiThread.cs:91-98`)" | Stale span. The struct is `UtilitiesCS/Threading/UiThread.cs:140-196`; the `ArgumentNullException` is at `:146-153`. It is also not the first throw on that path — `EfcViewer.cs:27` throws earlier. |
| "`TryReportBoundaryFault` ... likely lives in `EfcItemController.WebViewFaultBoundary.cs` and `QfcItemController.WebViewFaultBoundary.cs`" | **Wrong location.** `TryReportBoundaryFault` is defined once, at `EfcFormController.cs:150-168`. Those two files define a differently named member, `TryReportWebViewInitializationFault` (`:44-65` in each), over a separate `WebViewInitializationErrorSink`, and their docs state the separation is deliberate. |
| "Both `WebView2BreadcrumbHost` and `EfcFormController` log the same failure, suggesting the initialization is attempted from two paths" | **Refuted.** One SDK failure, two log statements (`WebView2BreadcrumbHost.cs:337-340` and `EfcFormController.cs:1080`). One construction site, one `InitializeAsync` call site. |
| Suspected cause: handle/parent state, double-init, disposed/pooled control, or user-data-folder conflict | The first three are refuted (Q2). The fourth is correct in family but must be stated as an **options** conflict, not a folder conflict: all three sites use the identical folder. |
| Reference to the `#458` pooled-viewer handler-retention history as relevant background | Both `#458` and `#476` are **already fixed in the current tree** and neither contributes to `#792` (Q2). |

---

## Recommended fix shape

### Root cause remedy (prerequisite to AC-U1 and AC-U5; not itself an AC)

Give the process **one owner** of the WebView2 environment contract and make all three sites use it.

- **New file** `QuickFiler/Viewers/WebView2EnvironmentContract.cs` (~60 lines), plus a
  `<Compile Include="Viewers\WebView2EnvironmentContract.cs" />` line in `QuickFiler/QuickFiler.csproj`
  adjacent to `:421`. Contents: `internal const string AdditionalBrowserArguments = "--incognito ";`,
  `internal static string ResolveUserDataFolder()` (the `Path.Combine(LocalApplicationData, "WindowsFormsWebView2")`
  expression currently duplicated three times), and
  `internal static CoreWebView2EnvironmentOptions CreateOptions()`.
- **Edit** `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:246-250` to use it. This is the
  behaviour-changing line: the breadcrumb environment acquires `--incognito` and stops conflicting.
- **Edit** `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:55-61` and
  `QuickFiler/Controllers/EfcItemController.cs:176, 181-189` to reference the same owner
  (line-neutral, so neither near-500-line file is pushed over).
- Consider, but do not require, routing `EfcItemController.cs:194-198` through
  `IWebViewCoreInitializer` so all three creations are behind one mockable seam.

**Test seam**: `Mock<IWebViewCoreInitializer>` verifying
`CreateEnvironmentAsync(It.Is<string>(f => f.EndsWith(@"\WindowsFormsWebView2")), It.Is<CoreWebView2EnvironmentOptions>(o => o.AdditionalBrowserArguments == WebView2EnvironmentContract.AdditionalBrowserArguments))`
for `WebView2BreadcrumbHost.InitializeAsync`. Add a structural parity test in
`QuickFiler.Test/Viewers/` asserting the three sites resolve to one constant — the repository already
has precedent for set-equality structural tests (see the ribbon catalog/XML tests noted in prior
research).

### AC-U1 — retry, and a visible error state on final failure

- **File** `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` (new partial part, Q9).
  Make `InitializeBreadcrumbHostAsync` `internal` and give it a bounded, deterministic retry (fixed
  attempt count, **no** `Task.Delay` — the determinism rule in `.claude/rules/general-unit-test.md`
  bans wall-clock waits in tests, so either retry immediately or inject a delay delegate defaulting
  to a real one and substituted with a no-op in tests).
- On final failure, call a **new** router entry point (see AC-U2) so the folder area renders a
  visible error banner rather than staying blank.
- Do **not** put the retry in `WebView2BreadcrumbHost.OnCoreInitializationCompleted`
  (`:329-354`): that branch is structurally untestable.
- **Seam the new test uses**: `Mock<IWebViewCoreInitializer>` whose `EnsureCoreWebView2Async` returns
  `Task.FromException(new COMException(..., unchecked((int)0x8007139F)))` for the first N calls;
  assert the call count and, on exhaustion, that the router received the failure notification. This
  requires `_breadcrumbHost` to be injectable — either widen the field to `IBreadcrumbWebHost` and
  add an internal setter/test constructor, or extract the retry into a small testable policy type.

### AC-U2 — `_pendingDocument` is never silently dropped

- **File** `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` (407 lines, headroom ~93). Add
  `public void NotifyInitializationFailed(Exception error)` next to `NotifyCoreInitialized`
  (`:320-329`). It must (a) render and deliver an error document through `_renderer`/`_host` or, if
  the host cannot navigate, expose the pending state rather than silently retaining it, (b) clear
  `_pendingDocument`, and (c) drain or discard `_outboundQueue` explicitly rather than leaving it to
  grow unboundedly (`BreadcrumbOutboundQueue.cs:37-52`).
- Preserve the existing success behaviour: a later successful `NotifyCoreInitialized` must still
  navigate a stash produced before the failure.
- **File** `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs`: wire the new notification next
  to the existing `_breadcrumbHost.CoreInitialized += ...` lambda (currently `EfcFormController.cs:1064`).
- **Seam the new tests use**: `Mock<IBreadcrumbWebHost>` with a settable `IsCoreInitialized`, exactly
  as `BreadcrumbBridgeRouterQueueTests.cs` already does (`:117`, `:450-455`). Assert
  `BreadcrumbOutboundQueue.PendingCount == 0` after the failure notification. No new seam needed.

### AC-U3 — pop-out carries the predictor and the loaded `MailItemHelper`, and builds the viewer on the UI thread

Carry half:
1. **`QuickFiler/Interfaces/IQfcItemController.cs`** — add a read-only
   `IFolderSearchHandler FolderHandler { get; }` (the interface already exposes `ItemHelper` at `:41`).
   Implement it on `QuickFiler/Controllers/QfcItemController.cs` over the existing `_folderHandler`
   field (`:41`), alongside the existing `TopFolderScore` accessor (`:265`).
2. **New** `QuickFiler/Controllers/QfcCollectionController.PopOut.cs` — move
   `PopOutControlGroup` (`:710-720`) and `PopOutControlGroupAsync` (`:722-735`) there, add an
   injectable `Func<IApplicationGlobals, System.Action, MailItem, IFolderSearchHandler, MailItemHelper, EfcHomeController>`
   factory seam defaulting to the production construction, and read the carry from
   `_itemGroups[selection - 1].ItemController`.
3. **`QuickFiler/Controllers/EfcHomeController.cs`** (447 lines, ~53 headroom — watch it) — add trailing optional
   carry parameters to the public ctor (`:47-52`) and the internal ctor (`:54-95`), and deposit them
   on `DataModel` between `:71` and `:73`, **before** `FormControllerWithDataFactory` at `:85`.
   If the file cannot absorb the change, split out a `EfcHomeController.Carry.cs` part (the type is
   already `public partial class`, `:18`, with an existing `EfcHomeController.Timing.cs` sibling at
   csproj `:300`).
4. **New** `QuickFiler/Controllers/EfcDataModel.Carry.cs` — **required**, because `EfcDataModel.cs`
   is 499/500. Move `InitFolderHandlerAsync` (`:188-221`) there and add the carry-adoption branch
   mirroring `QfcItemController.FolderHandling.cs:60-86`: adopt only when `folderList is null` and
   the carry is non-null, and release the carry after adoption. Resolve the
   `FolderPredictor`-vs-`IFolderSearchHandler` typing decision recorded in Q5 before writing this.
5. `QuickFiler/QuickFiler.csproj` — three to four new `<Compile Include="..." />` lines, no metadata.

UI-thread half:
- **`QuickFiler/Helper Classes/EfcViewerQueue.cs:25`** — change
  `ProductionBlockingPriorityScheduler` from `(action, priority) => action();` to
  `(action, priority) => UiThread.Dispatcher.Invoke(action, priority);`, matching
  `ItemViewerQueue.cs:89-90`. Mirror the change in `ResetProductionCoreDefaultsForTesting` (`:68`).
- **Seam the new test uses**: substitute `EfcViewerQueue.ProductionBlockingPriorityScheduler` and
  assert the action was scheduled rather than run inline — the existing
  `ViewerQueueStaticWrapperTests.cs:244-261` already does exactly this shape for the other three
  delegates. Carry-half tests use the new `EfcHomeController` factory seam and a
  `Mock<IQfcItemController>` returning a stub `IFolderSearchHandler`.

### AC-U4 — boundary reporting

- **`PopulateFolderCombobox`**: already satisfied at `EfcFormController.cs:1270`. Strengthen the
  existing test `QuickFiler.Test/Controllers/EfcFormControllerTests.cs:300-...`
  (`PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault`) to assert a
  `BoundaryErrorSink` call rather than only "does not fault", so the AC is pinned rather than
  incidentally true.
- **`InitializeBreadcrumbHostAsync`**: replace `logger.Error(...)` at `EfcFormController.cs:1080`
  with `TryReportBoundaryFault(...)`, in the new `EfcFormController.Breadcrumb.cs`. Classify
  `OperationCanceledException` as non-fault, matching `BindBreadcrumbRowsAsync` (`:1121-1124`) and
  the precedent test at `EfcFormControllerTests.Part2.cs:174-200`.
- **Seam the new test uses**: `EfcFormController.BoundaryErrorSink` (`:128-129`) substitution on a
  `CreateMinimalController()` instance, plus the injectable host/initializer required by AC-U1.
  `EfcFormController.UserFaultNotifier` (`:181-185`) is `AsyncLocal`-backed, so the test must be a
  synchronous method if it asserts at the notifier level — the precedent is
  `EfcFormControllerTests.Part2.cs:279-...`.

### Do not do

- Do not add the retry or the error surfacing inside
  `WebView2BreadcrumbHost.OnCoreInitializationCompleted` — unreachable from any unit test.
- Do not route the `EfcFormController` fault through `TryReportWebViewInitializationFault`; that is a
  different, deliberately separate contract on the item controllers.
- Do not attempt a full split of `QfcCollectionController.cs` (2329 lines) inside this bug fix.

---

## Open questions and risks

1. **Unverified: is the shared browser process cross-session persistent?** The options conflict is
   scoped to "WebViews currently running in the shared browser process". Whether closing all
   QuickFiler viewers tears the browser process down (and would therefore let the breadcrumb host
   succeed on a fresh Efc open) is **not established** from the code. It affects only the predicted
   manual-repro recipe for AC-U5, not the fix.
2. **Unverified: does the `--incognito` argument change breadcrumb behaviour?** Adopting
   `--incognito` for the breadcrumb environment means the breadcrumb document runs with no persisted
   browsing data. The breadcrumb document is generated locally by `BreadcrumbHtmlRenderer` and
   delivered via `NavigateToString`, so no cookie, cache or storage dependence is apparent, but I did
   **not** audit the generated HTML/JS for `localStorage` or similar. The plan should confirm this
   before choosing "make everything incognito" over "make nothing incognito".
3. **Unverified: which of the two option sets should win.** Making all three no-argument would also
   resolve the conflict and would preserve the breadcrumb's current behaviour while changing the item
   bodies'. The evidence in this document does not decide the direction; it only establishes that
   they must agree. Note that `--incognito` is the *older*, more widely exercised choice (two of three
   sites, and the one the item-body preview has always used), which argues for it.
4. **Unverified: commits `f50fb7271` and `655130c5a`.** The Bash tool is disabled in this session, so
   no git history was read. All findings are against the working tree at HEAD `2405a829d`.
5. **Unverified: whether the pop-out continuation reaches `new EfcHomeController(...)` off the UI
   thread.** `ToggleOffActiveItemAsync` was not traced to a terminal `ConfigureAwait(false)` (Q6).
   The AC-U3 UI-thread clause should therefore be justified as defence-in-depth and as parity with
   `ItemViewerQueue`, not as a proven reproduction.
6. **Risk: `EfcHomeController.cs` headroom.** At 447 lines it has ~53 lines of room. The AC-U3 carry
   parameters plus their XML documentation could exceed it, forcing a fourth new partial file and a
   fourth csproj line. Size the change before committing to the file list.
7. **Risk: `QfcItemController.ViewerSetup.cs` headroom — corrected.** The file is **467** lines, not
   the 499 prior research recorded, so it has 33 lines of headroom. A line-neutral options-contract
   edit at `:61` (replace the literal with a constant reference) remains preferred, but the file does
   not split on a small addition.
11. **Obligation missed above: `QuickFiler/Controllers/EfcItemController.cs` is 1121 lines.** The
    root-cause remedy edits its options construction at `:176` and `:187-189`, and the 500-line
    ceiling therefore obliges a split of that file as well. The Q9 proposal covered
    `EfcFormController.cs`, `EfcDataModel.cs` and `QfcCollectionController.cs` but not this one. The
    plan must either name a concrete `EfcItemController.*.cs` partial target and its csproj line, or
    make the edit strictly line-neutral and record explicitly why a 1121-line file is being edited
    without a split. This is the orchestrator's addition, not the researcher's.
8. **Risk: the `FolderPredictor` / `IFolderSearchHandler` typing decision (Q5) leaks into UtilitiesCS.**
   Widening `IFolderSearchHandler` with `RefreshSuggestions` touches
   `UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs` and every implementer. Prior research
   records that `UtilitiesCS` has no NetAnalyzers, so analyzer risk is low, but the blast radius
   extends outside QuickFiler and should be declared in the plan's write set.
9. **Risk: AC-U1's "visible error state" has no existing rendering primitive for errors.** The
   nearest fit is a banner row (`BreadcrumbRowBuilder.BannerPrefix = "===="`,
   `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19`; banner handling at
   `BreadcrumbHtmlRenderer.cs:104` and `BreadcrumbRowBuilder.cs:101-106`), which is already
   non-selectable at `BreadcrumbBridgeRouter.Selection.cs:85-88`. Whether the maintainer accepts a
   banner row as "a visible error state in the folder area" should be confirmed before the plan
   commits to it; the alternative is a WinForms label on `EfcViewer`, which lands in an
   `[ExcludeFromCodeCoverage]` Designer-owned file (`EfcViewer.cs:20-21`).
10. **Risk: contention with #645 on `QuickFiler/QuickFiler.csproj` and `QfcCollectionController.cs`**
    (Q10). Declare both in the write set.
