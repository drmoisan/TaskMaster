# webview2-host-initializer-defects (Spec)

- **Issue:** #476 (also closes #458, #477)
- **Parent (optional):** epic `quickfiler-bug-family`; upstream epic #136, child F13 (#455)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-24T10-05
- **Status:** Ready for planning
- **Version:** 1.0
- **Work Mode:** `full-bug` — this file is the authoritative acceptance-criteria source. No
  `user-story.md` exists or may be created for this feature.

Evidence basis for every design decision in this document is the read-only research artifact
`docs/features/active/webview2-host-initializer-defects-476/research/2026-08-24T00-45-webview2-host-initializer-defects-research.md`
(cited below as "research §N"). Every `file:line` reference below is taken from that artifact or
from a direct reading of the cited file. Claims the research artifact records as UNVERIFIED are
carried forward with that qualification and are not upgraded here.

---

## Context

Three pre-existing defect reports — #458, #476, #477 — share one file set, one lifecycle area, and
one remediation window. They are delivered as a single epic child so that the breadcrumb host's
subscription lifetime, its thread-affinity contract, and the core-initializer seam's contract are
corrected in one pass rather than three passes over the same three files.

| Issue | Defect | Severity (as filed) | Primary file |
| --- | --- | --- | --- |
| #458 | Constructor-side unhook cannot remove a predecessor's subscription | Medium | `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` |
| #476 defect 1 | Unmarshalled WebView2 SDK access on the caller's thread | High | `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` |
| #476 defect 2 | Unsynchronized cross-thread publication of `IsCoreInitialized` | High | `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` |
| #477 defect 1 | False "1:1 forward" contract claim; hard-coded `browserExecutableFolder` | Medium | `QuickFiler/Viewers/IWebViewCoreInitializer.cs`, `QuickFiler/Viewers/WebView2CoreInitializer.cs` |
| #477 defect 2 | No argument validation on either seam member | Medium | `QuickFiler/Viewers/WebView2CoreInitializer.cs` |

- **Observed environment:** Windows 11 Pro 10.0.26200; .NET Framework 4.8.1 WinForms VSTO add-in;
  `Microsoft.Web.WebView2` 1.0.4129.50 (`QuickFiler/packages.config:29`).
- **Impact and severity:** #476 is the highest-severity item because its failure mode is
  intermittent and apartment-dependent rather than deterministic, which makes it expensive to
  diagnose from a production report. #458 and #477 are latent: neither misbehaves on the current
  happy path (see the premise corrections below).
- **First observed:** all three were found during preparation research for issue #455 (epic #136,
  child F13) on 2026-08-07. The affected code was introduced by issue #349. All three were deferred
  out of #455 because #455 carries a hard no-behaviour-change NFR and each of these fixes changes
  observable behaviour.

### Defect inventory

#### #458 — constructor-side unhook is a no-op

- **Location:** `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:48-50`; the same shape recurs at
  `:131-132` for `core.WebMessageReceived`.
- **Offending shape:**

  ```csharp
  // Idempotent hookup: pooled viewers re-run initialization, so unhook before hooking.
  _control.CoreWebView2InitializationCompleted -= OnCoreInitializationCompleted;   // :49
  _control.CoreWebView2InitializationCompleted += OnCoreInitializationCompleted;   // :50
  ```

- **Root cause:** `OnCoreInitializationCompleted` is an instance method
  (`WebView2BreadcrumbHost.cs:115-118`). The delegate formed by `-=` in the constructor has `this`
  — the instance under construction — as its target. Delegate equality in .NET is pairwise over
  `(target, method)`, so the removal can only match a subscription made by an instance that has
  made none. The removal matches nothing (research §1.1).
- **Observable consequence:** when a second host is constructed over the same `WebView2` control,
  the predecessor stays subscribed, is retained for the control's lifetime, and handles
  initialization completion in addition to the live host. A downstream retention edge compounds it:
  `BreadcrumbBridgeRouter.cs:54` subscribes `_host.MessageReceived` and never unsubscribes, so a
  stale host also retains its stale router (research §1.1).

#### #476 defect 1 — unmarshalled SDK access

- **Location:** `PostMessageJson` at `WebView2BreadcrumbHost.cs:72-84` (property read at `:74`, SDK
  call at `:83`); `NavigateToString` at `:66-69`.
- **Root cause:** both members touch the WebView2 control on whatever thread invoked them. The same
  file states the requirement it violates at `:105` ("WebView2 controls must be touched on the
  WinForms UI (STA) thread") and honours it in `InitializeAsync` by awaiting `uiSyncContext` at
  `:106`. The sibling adapter `WebView2Messenger` routes every SDK touch through
  `BreadcrumbUiDispatcher` (`WebView2Messenger.cs:40-48, :62-68, :80-94, :104-122`), so the two
  adapters disagree about the thread-affinity contract of the same SDK.
- **Observable consequence:** a non-UI-thread caller is reachable. Research §3.2 gives a static
  reachability argument: `EfcFormController.RefreshSuggestionsAsync` (`:797-806`) awaits `Task.Run`
  without `ConfigureAwait`, and the keyboard entry paths at `:592` and `:657` reach it through
  `KbdExecuteAsync` (`:812-822`) with no ambient-context guard, so the continuation at `:805` can
  run on a thread-pool thread; the chain reaches `BreadcrumbBridgeRouter.DeliverDocument` (`:400`,
  `:402`) and `BreadcrumbOutboundQueue.PostOrQueue` (`:44`, `:46`) on that thread. This is a
  code-reading reachability argument, not a runtime observation (research §9 item 1); it is
  sufficient to establish that neither reader is structurally confined to the UI thread, which is
  the property the fix must not depend on.

#### #476 defect 2 — unsynchronized state publication

- **Location:** declaration at `WebView2BreadcrumbHost.cs:54`
  (`public bool IsCoreInitialized { get; private set; }`); sole write at `:134`; interface
  declaration at `QuickFiler/Viewers/IBreadcrumbWebHost.cs:25`.
- **Root cause:** a plain auto-property has a non-volatile compiler-generated backing field and
  therefore no barrier. The write at `:134` sits deliberately after the `core.WebMessageReceived`
  subscription at `:131-132` and before `CoreInitialized?.Invoke(...)` at `:135`, which is a
  compare-and-publish ordering, but nothing guarantees that ordering is visible to another thread.
- **Observable consequence:** a reader can observe `IsCoreInitialized == false` after
  initialization completed (dropping a payload through the guard at `:76`), or observe it `true`
  before the subscription at `:131-132` is visible. Research §3.1 enumerates every in-repo reader:
  `BreadcrumbOutboundQueue.cs:44` and `BreadcrumbBridgeRouter.cs:400` in production, plus three
  `SetupGet` sites on mocks in tests. Neither production reader holds a dispatcher or a lock.

#### #477 defect 1 — false contract claim and hard-coded SDK argument

- **Location:** `QuickFiler/Viewers/IWebViewCoreInitializer.cs:10-11` (the "forwards 1:1 to the
  WebView2 SDK" claim); `QuickFiler/Viewers/WebView2CoreInitializer.cs:8-14` (the coverage-exemption
  rationale resting on that claim); `WebView2CoreInitializer.cs:19-22` (the implementation).
- **Offending shape:**

  ```csharp
  public Task<CoreWebView2Environment> CreateEnvironmentAsync(
      string cacheFolder,
      CoreWebView2EnvironmentOptions options
  ) => CoreWebView2Environment.CreateAsync(null, cacheFolder, options);
  ```

- **Root cause:** the SDK signature is
  `CreateAsync(string browserExecutableFolder, string userDataFolder, CoreWebView2EnvironmentOptions options)`.
  The seam drops `browserExecutableFolder` and passes `null` unconditionally, pinning every caller
  to the Evergreen runtime. The forward is therefore not 1:1, and both the interface doc and the
  exemption rationale that rests on it are false.
- **Observable consequence:** latent and architectural. The repository cannot adopt a fixed-version
  WebView2 distribution without editing this file, and nothing documents that constraint.

#### #477 defect 2 — no argument validation

- **Location:** `WebView2CoreInitializer.cs:19-22` and `:25-28`.
- **Root cause:** neither member validates any argument. A null `control` at `:28` produces a bare
  `NullReferenceException` with no parameter name. A null or whitespace `cacheFolder` at `:22` is
  forwarded to the SDK, which surfaces a less specific failure than a guard would.
- **Observable consequence:** diagnostic. Failures surface without a parameter name and are harder
  to triage from a production log. Every sibling seam in this area guards its arguments
  (`WebView2Messenger.cs:38-39`, `WebView2BreadcrumbHost.cs:45-46`,
  `BreadcrumbPopupUiOperations.cs:71-77`, `BreadcrumbUiDispatcher.cs:27, :39, :74-77, :159-162`), so
  this file is the outlier, not the convention. `CLAUDE.md` §C#4 requires validating preconditions
  and failing fast with explicit exceptions.

---

## Repro & Evidence

### Premise correction 1 — `EfcViewerQueue` is not a recycle pool

The #458 issue text, the #458 potential document, and the class XML doc at
`WebView2BreadcrumbHost.cs:19` all attribute the failure to `EfcViewerQueue` recycling a viewer.
**That attribution does not survive reading the queue** (research §1.2):

- `QuickFiler/Helper Classes/ViewerQueueCore.cs` exposes `BuildQueue` (`:39`, `:52`), `Dequeue`
  (`:63`), `DequeueChunk` (`:87`), and `Reset` (`:116`). Every enqueue path calls `_viewerFactory()`
  (`:46`, `:59`, `:99`, `:104`, `:136`, `:146`). **There is no method that returns a
  previously-dequeued viewer to `_queue`.**
- `QuickFiler/Helper Classes/EfcViewerQueue.cs:81-84` — `CreateProductionViewer()` returns
  `new EfcViewer()`. `CreateProductionCore()` (`:71-79`) passes only four arguments, so
  `ViewerQueueCore`'s optional `disposeViewer` (`ViewerQueueCore.cs:23`) is null for this queue.
- The single construction site is `EfcFormController.cs:836-839`, inside
  `ConfigureBreadcrumbControl()` (`:834`), called once from `WireEventHandlers()` (`:393`).
  `WireEventHandlers()` is reached from `Initialize()` (`:96`) and `InitializeWithoutData()`
  (`:109`); no in-repo caller invokes either twice on the same controller/viewer pair
  (`EfcHomeControllerDependencyFactories.cs:80, :92, :120, :124-125`).

**Consequence for this spec.** The queue is a pre-warm pool of fresh instances, not a recycle pool.
#458 is a real correctness defect **at the type level** — the `-=` is dead code that cannot do what
its own comment claims, and the class is not safe to construct twice over one control — but in the
current production wiring it is **latent, not live**. There is no production repro. The regression
test must therefore be unit-level (two hosts, one control), and this spec states no production
repro and writes no acceptance criterion asserting one.

### Premise correction 2 — the `QuickFiler.Test.csproj` `Compile Include` block is not alphabetical

Planning inputs elsewhere assume the `Compile Include` ItemGroup at
`QuickFiler.Test/QuickFiler.Test.csproj:57-175` is alphabetically ordered. It is not (research
§6.1). It is grouped loosely by feature area and is inconsistent within a group: `:58`
`BreadcrumbBridgeRouterQueueTests.cs` precedes `:59` `BreadcrumbBridgeRouterTests.cs`, but `:60`
jumps from `Controllers\` to `Viewers\` and `:96` returns to `Controllers\`. The WebView2
neighbourhood, verbatim from `:158-160`, is:

```xml
    <Compile Include="Controllers\WpfUiDispatcherTests.cs" />
    <Compile Include="Controllers\WebView2CoreInitializerTests.cs" />
    <Compile Include="Controllers\QfcQueueTests.cs" />
```

`Wp` precedes `We`, confirming the block is not alphabetical. MSBuild imposes no ordering
requirement on `Compile` items. If a new test file is added, insert its entry immediately after
`:159` to keep the WebView2 entries contiguous and minimise the textual conflict surface; do not
re-sort the block.

### Evidence for the defects themselves

- #458: delegate-equality no-op confirmed by direct reading (research §1.1).
- #476 defect 1: unmarshalled access confirmed by direct reading; off-UI-thread reachability is a
  static code-reading argument, explicitly not a runtime observation (research §3.2, §9 item 1).
- #476 defect 2: auto-property, sole write site, and exhaustive reader list confirmed by direct
  reading and by `rg IsCoreInitialized --glob '*.cs'` (research §3.1).
- #477: both defects confirmed by direct reading of the two 31-line files.

Frequency and determinism: #458 and #477 are deterministic at the type level and unreachable in the
current production wiring. #476 is concurrency-dependent, intermittent, and apartment-dependent.

---

## Scope & Non-Goals

### In scope — production files this feature owns and may write

- `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`
- `QuickFiler/Viewers/WebView2CoreInitializer.cs`
- `QuickFiler/Viewers/IWebViewCoreInitializer.cs`

Test files under `QuickFiler.Test/` are writable.

### Forbidden — production files this feature must NOT write

Owned by concurrent sibling epic children:

- `QuickFiler/Viewers/WebView2Messenger.cs` (siblings 501, 488)
- `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` (siblings 501, 488)
- `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` (siblings 501, 488)
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` (siblings 501, 488)
- `QuickFiler/Controllers/EfcFormController.cs` (feature 464)

Two further files are in neither list and are treated as "do not edit":

- `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` — the marshalling seam. The chosen design requires
  **no** edit to it: its two-argument `internal` constructor at `:25-30` is already visible to the
  whole assembly and accepts a caller-supplied error sink (research §2.3).
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` — the second production caller of
  `CreateEnvironmentAsync` (`:64-67`). This is the file that makes #477 Option A infeasible
  (research §4.4).

### Non-goals

- Adopting a fixed-version WebView2 distribution. Whether that is a product requirement is recorded
  as UNCONFIRMED in the #477 potential document's own Next Step
  (`docs/features/potential/promoted/2026-08-07-iwebviewcoreinitializer-contract-defects.md:121`)
  and is not resolved here.
- Fixing the direct SDK calls in `QuickFiler/Controllers/EfcItemController.cs` (see Cross-Feature
  Notes).
- Adding a new production file. No `QuickFiler/QuickFiler.csproj` edit is required, which avoids the
  contiguous `Compile Include` run at `:391-413` that concurrent sibling children also occupy
  (research §5.2.4).
- Removing the coverage exemption from genuinely host-bound WebView2 SDK calls that have no
  injectable seam. Only the seams this feature makes testable lose their exemption.

---

## Root Cause Analysis

The five defects reduce to three root causes:

1. **Instance-bound delegate removal used as a cross-instance de-duplication mechanism** (#458).
   The mechanism is structurally incapable of the effect its comment claims.
2. **Absent thread-affinity enforcement on an SDK the same file documents as thread-affine**
   (#476 defect 1), and absent memory-ordering enforcement on a flag whose publication order is
   load-bearing for a compare-and-publish protocol in `BreadcrumbBridgeRouter` /
   `BreadcrumbOutboundQueue` (#476 defect 2).
3. **Documentation asserting a property the code does not have** (#477 defect 1), which suppressed
   examination of the type via a coverage exemption resting on that same false claim, which in turn
   left the missing argument validation unexamined (#477 defect 2).

Affected components: `QuickFiler/Viewers/` breadcrumb host and core-initializer seam only. The
consumers (`BreadcrumbBridgeRouter`, `BreadcrumbOutboundQueue`, `EfcFormController`,
`QfcItemController`) are not changed.

---

## Proposed Fix

### Design summary (what changes where)

All five defects are fixed inside the three writable files. No forbidden file is touched, no call
site changes, and no `.csproj` edit is required unless a new test file is added.

#### D1 — #458: per-control owner registry inside `WebView2BreadcrumbHost.cs`

A `private static readonly ConditionalWeakTable<WebView2, WebView2BreadcrumbHost>` owner registry
plus a `private static readonly object` gate, both private to `WebView2BreadcrumbHost.cs`. In the
constructor, under the gate: look up the previous owner for this control; if one exists, invoke its
private `DetachCore()` (which performs the real `-=` from the **predecessor** instance, whose
delegate target matches, for both `CoreWebView2InitializationCompleted` and, if it had subscribed,
`core.WebMessageReceived`); then replace the registry entry with `this`. The dead `-=` at `:49` is
replaced by the registry lookup; the `+=` at `:50` is kept. A `_control.Disposed` subscription that
detaches and removes the registry entry is added as secondary hygiene.

Feasibility notes carried from research §1.4:

- `System.Runtime.CompilerServices.ConditionalWeakTable<TKey,TValue>` has been present since .NET
  Framework 4.0 and is available on `net481`. Use only `TryGetValue` / `Add` / `Remove`, which are
  unambiguously present. Whether `AddOrUpdate` exists on `net481` is UNVERIFIED (research §9 item
  3); the design does not use it.
- Individual `ConditionalWeakTable` operations are documented thread-safe, but a read-then-write
  sequence is not atomic, so the compound operation must be taken under an explicit `lock`.
  Contention is nil: the only production construction site (`EfcFormController.cs:836`) is
  single-threaded per form.
- The table's value is held through a dependent handle keyed on the control, so an entry is
  collectible once the control is. The net effect is a **reduction** in retention: detaching the
  predecessor removes the `control -> stale host` edge that is the leak #458 describes, and the
  table adds no edge outliving the control.
- The key is `WebView2`; `ConditionalWeakTable` uses reference equality unconditionally, which is
  the correct key semantics here.

**Why not `IDisposable` / `Detach()`.** Rejected. Research §1.3 establishes exhaustively that no
in-repo caller exists and none could be added without editing `QuickFiler/Controllers/EfcFormController.cs`
(forbidden — `Cleanup()` at `:189-196` does not touch `_breadcrumbHost` at `:140`, and the
controller implements no `IDisposable`) or `EfcViewer.cs` (not writable; disposal is
Designer-generated). No disposal or recycling path reaches `_breadcrumbHost`. A `Detach()` with no
caller is not a fix. The `_control.Disposed` self-detach is adopted as secondary hygiene only; it
does not address #458's stated failure, which is two live hosts over one **undisposed** control.

**Why not `Control.Tag` as the owner slot.** Rejected: `Tag` is public, Designer-writable, and
shared with any other consumer of the control. A `ConditionalWeakTable` keyed on the control has the
same effect with no shared-slot hazard.

#### D2 — #476 defect 1: internal three-argument constructor plus single-`Dispatch` routing

Add an `internal WebView2BreadcrumbHost(WebView2 control, IWebViewCoreInitializer initializer, BreadcrumbUiDispatcher dispatcher)`
overload. The existing **public** two-argument constructor chains to it, so
`EfcFormController.cs:836-839` compiles and behaves identically and needs no edit. `internal`
members are visible to the test assembly via
`QuickFiler/Properties/AssemblyInfo.cs:5` (`[assembly: InternalsVisibleTo("QuickFiler.Test")]`).
This is the shape `WebView2Messenger` already uses (`WebView2Messenger.cs:33-36`).

Route each of the three operations through a **single** `BreadcrumbUiDispatcher.Dispatch(...)`
callback:

- `NavigateToString` (`:66-69`) → one `Dispatch` callback containing `_control.NavigateToString(html)`.
- `PostMessageJson` (`:72-84`) → one `Dispatch` callback containing **both** the
  `_control.CoreWebView2` read (currently `:74`) and the `core.PostWebMessageAsJson(json)` call
  (currently `:83`), plus the existing null-guard and log at `:75-81`. This matches
  `WebView2Messenger.PostJson` (`WebView2Messenger.cs:62-68`).
- The `_control.CoreWebView2` read inside `OnCoreInitializationCompleted` (`:129`) already runs on
  the UI thread because the SDK raises `CoreWebView2InitializationCompleted` there; wrapping it is
  optional and would be a no-op inline dispatch.

**`DispatchValue` must not be used.** `BreadcrumbUiDispatcher.DispatchValue<T>` runs inline only
when `ReferenceEquals(_executingDispatcher, this)` (`BreadcrumbUiDispatcher.cs:166`) — that is, only
from inside a currently-executing `Dispatch` callback — and otherwise faults on the
owner-thread-only test dispatcher (`:180-188`). Reading and posting inside one `Dispatch` callback
is both correct and the established precedent.

**Capture point: variant V1, not V2.** The dispatcher is built in `InitializeAsync` from the
`uiSyncContext` argument the host already receives (`WebView2BreadcrumbHost.cs:92`, null-guarded at
`:94-97`, awaited at `:106`), using `new BreadcrumbUiDispatcher(uiSyncContext, sink)`
(`BreadcrumbUiDispatcher.cs:25-30`), with a null-dispatcher inline-execution fallback for the
pre-initialization window. `BreadcrumbUiDispatcher.CaptureCurrent()` at construction (variant V2) is
**rejected** because it throws `InvalidOperationException` when `SynchronizationContext.Current` is
null (`BreadcrumbUiDispatcher.cs:46-50`), and whether the ambient context is non-null at
`EfcFormController.cs:836` is UNVERIFIED (research §9 item 1). In-repo evidence that a null ambient
context is observed on real entry paths exists at `EfcFormController.cs:451-452`, `:704-705`, and
`KeyboardHandler.cs:240-241, :268`, all of which install a context defensively. V1 introduces no new
throwing precondition on the constructor and requires no edit to `BreadcrumbUiDispatcher.cs`. It is
also well matched to the actual call ordering: both production readers call the host only after
`IsCoreInitialized` is true, which is after `InitializeAsync` has run.

The dispatcher captures **nothing from the `WebView2` control**; it captures a
`SynchronizationContext` and, for `CaptureCurrent` only, a managed thread id
(`BreadcrumbUiDispatcher.cs:46-55`). This is stated explicitly because it is the most likely
misreading of the `WebView2Messenger` precedent, whose `CaptureProductionDispatcher(coreWebView)`
(`WebView2Messenger.cs:138-145`) takes the control argument solely to order its null-guard failure
ahead of the ambient-context failure.

#### D3 — #476 defect 2: explicit backing field with `Volatile.Read` / `Volatile.Write`

Replace the auto-property at `:54` with an explicit backing field and a `Volatile.Read` getter;
replace the write at `:134` with `Volatile.Write`. The write must stay **strictly after** the
`core.WebMessageReceived` subscription at `:131-132` and **before** `CoreInitialized?.Invoke(...)`
at `:135`. The ordering is load-bearing: `Volatile.Write` is a release store, so a reader observing
the flag through `Volatile.Read` (an acquire load) is guaranteed to observe the preceding
subscription. An executor must not reorder these statements.

`System.Threading.Volatile` is available on `net481` and is already used in this same project at
`WebView2Messenger.cs:127`, and elsewhere at
`UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.Lifecycle.cs:214`.
`using System.Threading;` is already present at `WebView2BreadcrumbHost.cs:5`, so no new `using` is
required.

**The `Volatile` pair is required in addition to D2, not instead of it.** The potential document
offers "publish state through the dispatcher" as an alternative; it is not one. The two production
readers (`BreadcrumbOutboundQueue.cs:44`, `BreadcrumbBridgeRouter.cs:400`) call the property
directly and synchronously from arbitrary threads and are not being changed, and a dispatcher cannot
make a synchronous property read single-threaded.

#### D4 — #477: Option B for the hard-coded `browserExecutableFolder`, plus guards

1. **Option B is chosen.** The two-argument `CreateEnvironmentAsync` signature is **kept** and the
   unconditional `null` is documented as a deliberate Evergreen-only decision in the interface XML
   doc at `IWebViewCoreInitializer.cs:15-22`.
2. The false "1:1 forward" wording is corrected at `IWebViewCoreInitializer.cs:10-11`, and the
   exemption rationale at `WebView2CoreInitializer.cs:8-14` is restated on the accurate ground:
   external Evergreen runtime plus user-data-folder creation on disk.
3. Argument guards are added to the concrete `WebView2CoreInitializer` (`:19-28`):
   - `CreateEnvironmentAsync`: `ArgumentNullException` on a null `cacheFolder` and
     `ArgumentException` on a whitespace `cacheFolder`.
   - `EnsureCoreWebView2Async`: `ArgumentNullException` on a null `control`.
   - Do **not** guard `environment`: the SDK accepts null and creates a default environment.
   - `options` is decided at implementation time. Whether `CoreWebView2Environment.CreateAsync`
     accepts a null `options` is UNVERIFIED (research §9 item 4). Both in-repo callers always supply
     a non-null `options` (`WebView2BreadcrumbHost.cs:103`, `QfcItemController.ViewerSetup.cs:55`),
     so a guard is safe in practice; whether it is *correct* as a contract is the open question. If
     the SDK is found to tolerate null, guard `cacheFolder` only and document the tolerance.

**Option A is rejected** on two independent grounds, both recorded in research §4.4:

- Its blast radius is five files, one of which — `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:64-67`
  — is outside this feature's writable production set. A default-valued optional parameter would not
  avoid the interface edit and would not avoid the Moq `Setup` arity change at
  `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:275-278`, because a
  `Setup` lambda must match the full signature. Extending the same change to
  `EnsureCoreWebView2Async` would additionally require `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:388`,
  which is on the forbidden list.
- Its motivating requirement is explicitly UNCONFIRMED: the #477 potential document's own Next Step
  records `Confirm whether fixed-version WebView2 distribution is a product requirement` as an
  unchecked item (`…-iwebviewcoreinitializer-contract-defects.md:121`). Option A would change a
  public contract on the strength of an unconfirmed requirement.

If the fixed-version requirement is later confirmed, Option A becomes a separate, correctly-scoped
issue that must also take in the direct SDK call at `EfcItemController.cs:223-227`.

#### D5 — nullable directives

**Neither `QuickFiler/Viewers/WebView2CoreInitializer.cs` nor `QuickFiler/Viewers/IWebViewCoreInitializer.cs`
may gain a `#nullable enable` directive.** Verified: neither file contains one today
(`WebView2CoreInitializer.cs:1` is `using System.Diagnostics.CodeAnalysis;`;
`IWebViewCoreInitializer.cs:1` is `using System.Threading.Tasks;`; both files are 31 lines).
There is no `Directory.Build.props` and no `<Nullable>` element in `QuickFiler/QuickFiler.csproj`,
so nullable participation is strictly per-file opt-in, exactly as `CLAUDE.md` §C#1.3 states. Adding
the directive would conscript the file into the `/p:TreatWarningsAsErrors=true` gate for no benefit,
when the entire production change to these two files is two argument guards and XML documentation.
Express nullability with runtime `ArgumentNullException` guards, not with annotations.

`QuickFiler/Viewers/WebView2BreadcrumbHost.cs:1` **is** `#nullable enable`, so all new code there
must be nullable-clean under `/p:TreatWarningsAsErrors=true`. Points of care (research §6.2):

- `CoreWebView2 core = _control.CoreWebView2;` at `:129` is currently `CS8600`-free only because the
  SDK type is null-oblivious. Do not change that line's shape casually.
- The new `BreadcrumbUiDispatcher?` field must be declared nullable and null-checked at every use.
- The `ConditionalWeakTable.TryGetValue` `out` variable should be declared as
  `WebView2BreadcrumbHost? previous` and null-checked.

### Boundaries and invariants to preserve

- The **public** `WebView2BreadcrumbHost(WebView2, IWebViewCoreInitializer)` signature is unchanged.
- The `IWebViewCoreInitializer` **member signatures** are unchanged.
- The `IBreadcrumbWebHost.IsCoreInitialized` property shape is unchanged from a consumer's view.
- The publication order at `:131-135` (subscribe, publish flag, raise event) is preserved exactly.
- No forbidden file is touched; no `.csproj` `Compile Include` block is re-sorted.

### Dependencies or blocked work

None blocking. This feature depends on no sibling epic child and produces no artifact a sibling
consumes. The only coupling is the forbidden-file list, which is a constraint rather than a
dependency.

### Files/modules to change

| File | Change |
| --- | --- |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | Owner registry + predecessor detach (#458); internal 3-arg ctor + `Dispatch` routing (#476-1); `Volatile` backing field (#476-2); coverage-attribute restructuring |
| `QuickFiler/Viewers/WebView2CoreInitializer.cs` | Argument guards (#477-2); corrected exemption rationale (#477-1); coverage-attribute restructuring |
| `QuickFiler/Viewers/IWebViewCoreInitializer.cs` | XML documentation only (#477-1) |
| `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` | New guard tests (already registered at `QuickFiler.Test.csproj:159`; no csproj edit) |
| New or existing `QuickFiler.Test` file for the host tests | Regression tests for #458 and #476; a new file requires one `Compile Include` entry inserted after `QuickFiler.Test.csproj:159` |

### Functions/classes impacted

`WebView2BreadcrumbHost` constructor, `NavigateToString`, `PostMessageJson`, `InitializeAsync`,
`OnCoreInitializationCompleted`, `IsCoreInitialized`, plus new private `DetachCore` and new internal
observation member. `WebView2CoreInitializer.CreateEnvironmentAsync` and
`EnsureCoreWebView2Async`. `IWebViewCoreInitializer` documentation only.

### Data flow and validation changes

- `NavigateToString` and `PostMessageJson` payloads now traverse a `SynchronizationContext.Post`
  queue rather than executing inline. Payload content is unchanged.
- New validation: `WebView2CoreInitializer` rejects a null/whitespace `cacheFolder` and a null
  `control` with a named-parameter exception instead of forwarding them to the SDK.

### Error handling and logging updates

- The existing `log.Error("PostMessageJson called before CoreWebView2 initialization; payload dropped.")`
  at `:77-79` moves inside the `Dispatch` callback and is otherwise unchanged.
- Dispatch failures are routed to the error sink supplied when the dispatcher is constructed;
  `BreadcrumbUiDispatcher.Report` (`:238-253`) already swallows a failing sink into log4net.
- No new log category or level is introduced.

### Rollback / feature-flag considerations

None. The change is small, contained in three files, and revertible as a unit. No feature flag is
warranted for a defect fix of this size.

---

## Interface Contract Change (#477)

`CLAUDE.md` §7.2 and §C#3.3 require a breaking public-API change to be called out clearly and all
in-repo callers to be updated. This section discharges that requirement explicitly.

### What changes on `IWebViewCoreInitializer`

**XML documentation only. No member signature changes.** Specifically:

1. The claim at `IWebViewCoreInitializer.cs:10-11` that `WebView2CoreInitializer` "forwards 1:1 to
   the WebView2 SDK" is corrected: the forward is not 1:1 because `browserExecutableFolder` is not
   surfaced.
2. `CreateEnvironmentAsync`'s doc (`:15-22`) gains an explicit statement that the SDK's
   `browserExecutableFolder` argument is passed as `null` unconditionally, that this is a deliberate
   Evergreen-only decision, and that selecting a fixed-version WebView2 distribution therefore
   requires a contract change.
3. `<exception>` documentation is added for the guards introduced on the concrete implementation.

### Every in-repo implementer (research §4.1)

**Concrete implementers: exactly one.**

| Implementer | file:line |
| --- | --- |
| `WebView2CoreInitializer` | `QuickFiler/Viewers/WebView2CoreInitializer.cs:16` |

A search for `: IWebViewCoreInitializer` across all `*.cs` returns only that declaration. There is
no hand-written test fake, stub, or spy implementing the interface anywhere in the repository.

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
`typeof(IWebViewCoreInitializer)` reflectively;
`QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs:19, :22` constructs the concrete type
and asserts assignability.

### Every in-repo caller of `CreateEnvironmentAsync` (research §4.2)

| # | Caller | file:line | Writable? |
| --- | --- | --- | --- |
| 1 | `WebView2BreadcrumbHost.InitializeAsync` | `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:108-111` | Yes |
| 2 | `QfcItemController.InitializeWebViewAsync` | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:64-67` | No |
| 3 | `BuildWebViewInitializerMock` (Moq `Setup`) | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:273-280` | Yes (test) |

Declaration `IWebViewCoreInitializer.cs:19-22`; implementation `WebView2CoreInitializer.cs:19-22`.

### Every in-repo caller of `EnsureCoreWebView2Async` (research §4.3)

| # | Caller | file:line | Writable? |
| --- | --- | --- | --- |
| 1 | `WebView2BreadcrumbHost.InitializeAsync` | `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:112` | Yes |
| 2 | `QfcItemController.InitializeWebViewAsync` (body pane) | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:68-71` | No |
| 3 | `QfcItemController.InitializeWebViewAsync` (breadcrumb pane) | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:111-114` | No |
| 4 | `BreadcrumbPopupUiOperations` | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:388` | No — FORBIDDEN |
| 5 | `BuildWebViewInitializerMock` (Moq `Setup`) | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:281-288` | Yes (test) |

Declaration `IWebViewCoreInitializer.cs:28`; implementation `WebView2CoreInitializer.cs:25-28`.

### Which callers require a code change under Option B

**None. Zero call sites change, in production or in tests.**

The reason is that Option B changes no member signature and no parameter arity, so every call
expression above still binds to the same member. The only behavioural narrowing is on arguments that
were previously **undefined behaviour**: a null `control` previously produced a bare
`NullReferenceException` with no parameter name, and a null or whitespace `cacheFolder` was
forwarded to the SDK, which surfaced a less specific failure. Neither was a defined, relied-upon
contract. Every in-repo caller already passes non-null values — `WebView2BreadcrumbHost.cs:99-103`
computes a non-null `cacheFolder` from `Environment.GetFolderPath` and constructs a non-null
`options`; `QfcItemController.ViewerSetup.cs:55` likewise supplies a non-null `options`; every
`EnsureCoreWebView2Async` caller passes a Designer-owned control — so no caller crosses the newly
guarded boundary.

### Why the eight `MockBehavior.Strict` sites cannot break

Adding guards to the concrete `WebView2CoreInitializer` is invisible to every Moq mock, strict or
loose, because **Moq generates a dynamic proxy of the interface and never executes the concrete
class's body** (research §4.5). Concretely:

- None of the eight strict sites `Setup`s either member. They pass `.Object` as a collaborator and,
  in one case, assert `initializer.VerifyNoOtherCalls()`
  (`QfcItemControllerBreadcrumbDropDownTests.cs:56`). A strict mock with no setups throws only if a
  member is actually invoked, which those tests assert does not happen.
- The only `Setup` of either member is on the **loose** mock at
  `QfcItemController.InitializationTests.Part2.cs:272-289`, which uses `It.IsAny<>` matchers plus
  `ThrowsAsync` and is therefore insensitive to any change in the concrete class.
- The one test that instantiates the concrete type
  (`QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs:19`) uses the implicit parameterless
  constructor and invokes neither member.

Expected test breakage from the guards: **nil**.

---

## Cross-Feature Notes

Every remediation avenue that would require a file this feature may not write is recorded here
rather than dropped silently.

1. **`QuickFiler/Controllers/EfcFormController.cs` — owned by feature 464.** It is the sole
   construction site of `WebView2BreadcrumbHost` (`:836-839`) and holds the only field
   (`_breadcrumbHost` at `:140`) that could own a disposal call. `Cleanup()` (`:189-196`) does not
   touch it, and the controller implements no `IDisposable`. This is precisely why D1 chose the
   owner-registry design over `IDisposable`/`Detach()`: a `Detach()` added here would have zero
   callers. If feature 464 or a later change adds host disposal to that controller, the registry
   design remains compatible and a `Detach()` could then be exposed as an additional, caller-backed
   API.

2. **`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:64-67` — outside the writable set.**
   It would have to change under #477 Option A, because a `Setup` lambda and a call expression must
   both match the full signature. This is one of the two reasons D4 chose Option B. Extending Option
   A to `EnsureCoreWebView2Async` would additionally require
   `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:388`, which is on the forbidden list, and
   would be blocked outright.

3. **FOLLOW-UP DEFECT FOUND DURING RESEARCH — out of scope here, must not be fixed by this feature,
   and warrants its own issue.** `QuickFiler/Controllers/EfcItemController.cs:223-227` (with an
   earlier variant near `:186-192`) calls `CoreWebView2Environment.CreateAsync(null, cacheFolder, options)`
   **directly on the SDK**, bypassing the `IWebViewCoreInitializer` seam entirely. The same file
   also calls `EnsureCoreWebView2Async` directly on the control at `:201` and `:236`. Consequences:
   (a) fixing the seam does not fix that call site; (b) any future fixed-version-distribution work
   must include it, or the seam change will be silently incomplete; (c) that code path is not
   mock-isolable and therefore not routing-testable. `EfcItemController.cs` is outside this
   feature's writable production set. **Action: promote this to its own GitHub issue through the
   promotion lifecycle so it is not lost when this feature folder is archived.**

4. **`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` — in neither list.** The chosen design requires
   no edit to it. Its two-argument `internal` constructor (`:25-30`) is already assembly-visible and
   accepts a caller-supplied error sink, which is exactly what variant V1 needs.

5. **`QuickFiler/QuickFiler.csproj:391-413` — the contiguous breadcrumb/WebView2 `Compile Include`
   run.** Concurrent sibling children own four files already registered in that block (`:391`,
   `:397`, `:401`, `:413`). If two children each append a line to it, git reports a textual conflict
   on adjacent lines. This feature adds no production file and therefore does not touch that
   ItemGroup at all. Whether a sibling will add a file there is UNVERIFIED from this worktree
   (research §9 item 6).

---

## Coverage and Exemption Impact

This section is load-bearing and must be implemented as written; it is not background.

### The governing rule

`CLAUDE.md` §UT2 and `.claude/rules/general-unit-test.md` state that the COM/VSTO/WinForms coverage
exemption applies only to code that cannot be exercised without a live host, and that **testable
seams within otherwise COM-bound assemblies are explicitly NOT eligible for the exemption**. The
`coverage.config` file contains no entry for either type (research §5.3), so exemption in this area
is entirely attribute-driven and is therefore reviewable in the pull-request diff.

### `WebView2BreadcrumbHost` — the class-level attribute becomes false and must be removed

`WebView2BreadcrumbHost.cs:29` carries a class-level `[ExcludeFromCodeCoverage]` whose stated
rationale at `:22-28` is that "every member forwards 1:1 to the WebView2 SDK or reacts to its events
on a live control that cannot exist in a unit-test host; all routing/decision logic lives in the
non-exempt `BreadcrumbBridgeRouter`/`BreadcrumbOutboundQueue`".

That rationale becomes **false** once this feature lands, because the internal three-argument
constructor, the dispatcher routing decision in `NavigateToString` and `PostMessageJson`, the owner
registry and its detach path, and the `Volatile` state accessor are all reachable from tests. That
is the identical false-rationale defect #477 identifies in `WebView2CoreInitializer`. Fixing one
while creating the other is not acceptable.

**Required outcome:**

1. The class-level `[ExcludeFromCodeCoverage]` at `:29` is **removed**, and the class-level remarks
   at `:22-28` are rewritten to state accurately what remains exempt and why.
2. Member-level `[ExcludeFromCodeCoverage]` with an **accurate**, member-specific rationale is
   applied only to the genuinely host-bound members — those that cannot execute without a live
   WebView2 runtime. Those members are `OnCoreInitializationCompleted` and `OnWebMessageReceived`
   (each raised only by the live SDK) plus the two extracted private SDK forwards described in
   item 3.

   **`InitializeAsync` is NOT exempt and must be measured.** Its only SDK-reaching statements go
   through the injectable `IWebViewCoreInitializer` seam, so it executes end-to-end against a
   `Mock<IWebViewCoreInitializer>` with no Evergreen runtime present. Exempting a member that the
   plan demonstrably tests would recreate precisely the false-rationale defect that #477 reports
   against `WebView2CoreInitializer`, which this feature exists to correct. The governing rule is
   unchanged: a member reachable through an injectable seam is a testable seam and is not eligible
   for the COM/VSTO/WinForms exemption.
3. Where a public member contains **both** a testable decision (an argument guard, or the choice to
   marshal through the dispatcher) **and** a host-bound SDK forward, the SDK forward is extracted
   into a small private method that carries the member-level attribute. The testable decision then
   remains measured and only the unavoidable SDK call is exempt. `[ExcludeFromCodeCoverage]` applies
   per member, not per branch, so this extraction is the only mechanism that satisfies both
   requirements simultaneously.

### `WebView2CoreInitializer` — the same treatment

The exemption on this type remains substantively justified: both members require the Evergreen
runtime (an external process) and `CreateEnvironmentAsync` creates a user-data folder on disk, so
executing either in a unit test is prohibited by `CLAUDE.md` §UT4 (external dependencies; temporary
files) rather than merely difficult. Only the **stated rationale** is false.

**Required outcome:**

1. The rationale at `WebView2CoreInitializer.cs:8-14` is restated on the accurate ground — external
   Evergreen runtime plus user-data-folder creation on disk — replacing the "1:1 forwarding" claim.
2. The **new argument guards are pure validation with no SDK dependency**, so under the rule they
   are a testable seam and are **not** exempt. They must be measured.
3. The two SDK forwards keep an exemption carrying the accurate rationale, at member level (or, if
   the guard and the forward share a member body, with the forward extracted into a small private
   method that carries the attribute, per the same mechanism above).

### Risk to record

Removing a class-level exemption moves previously unmeasured lines into the coverage denominator and
can therefore move the repository coverage figure. The mitigation is that member-level exemptions on
the genuinely host-bound members keep the added denominator small and bounded: only the seams this
feature actually makes testable enter measurement, and each of them gains a regression test in the
same change. The executor should capture the coverage figure before and after and record the delta
in the feature evidence folder.

---

## Assumptions, Constraints, Dependencies

**Assumptions**

- `QuickFiler.Test` can construct a real `Microsoft.Web.WebView2.WinForms.WebView2` control without
  the Evergreen runtime. Evidence: the test project references both
  `Microsoft.Web.WebView2.Core` and `Microsoft.Web.WebView2.WinForms`
  (`QuickFiler.Test.csproj:285-290`), and `ItemViewer.Designer.cs:46, :49` already constructs two of
  them in a path measured by
  `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs:354-385`. Only
  `EnsureCoreWebView2Async` / `CoreWebView2Environment.CreateAsync` require the runtime — which is
  exactly the boundary `IWebViewCoreInitializer` exists to isolate.
- Whether a bare `new WebView2()` (with no Designer `ISupportInitialize.BeginInit`/`EndInit`)
  constructs cleanly **off** the pump is UNVERIFIED (research §9 item 5). Mitigation: construct it on
  `WinFormsPumpHost` via `host.InvokeAsync(() => new WebView2())`, which is safe regardless.
  `FormatterServices.GetUninitializedObject(typeof(WebView2))` is a documented fallback (precedent
  `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs:200`) but may leave a null
  WinForms `Events` list and therefore fail on event subscription.

**Constraints**

- Target framework `v4.8.1` for both `QuickFiler` and `QuickFiler.Test`
  (`QuickFiler.Test.csproj:18`).
- MSTest 4.3.3, Moq 4.20.72, FluentAssertions 8.10.0 (`QuickFiler.Test.csproj:194-196`, `:309-311`,
  `:312-317`).
- Analyzers active on the test project constrain test code: `MSTest.Analyzers`,
  `SonarAnalyzer.CSharp`, `Meziantou.Analyzer`, `Roslynator`, `AsyncFixer`, and
  `Microsoft.CodeAnalysis.BannedApiAnalyzers` with `BannedSymbols.txt`
  (`QuickFiler.Test.csproj:437-439, :466-474`).
- `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:16-36` asserts no `System.Windows.Forms.Form`-derived
  type is compiled into the test assembly. `WebView2` derives from `UserControl`, not `Form`, so
  constructing one does not violate the guard; defining a new `Form` subclass in the test project
  would.
- No temporary files may be created by tests (`CLAUDE.md` §UT4; approved exceptions: none).

**External dependencies**

`Microsoft.Web.WebView2` 1.0.4129.50. No new package is added.

---

## Data / API / Config Impact

- **User-facing changes:** none. No UI, command, or setting changes.
- **API changes:** documentation-only on `IWebViewCoreInitializer`; a new `internal` constructor
  overload on `WebView2BreadcrumbHost` (not part of the public surface, but visible to
  `QuickFiler.Test` via `InternalsVisibleTo`); new throwing preconditions on
  `WebView2CoreInitializer` for arguments that were previously undefined behaviour.
- **Data or migration:** none.
- **Logging/telemetry:** unchanged content; the existing `PostMessageJson` drop message moves inside
  the dispatch callback. Dispatcher failures route through `BreadcrumbUiDispatcher.Report`
  (`:238-253`), which already falls back to log4net.
- **Compatibility:** no CLI flag, config schema, or version change.

---

## Test Strategy

### Framework and style requirements (uniform across all new tests)

- MSTest `[TestClass]` / `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`; add
  `[Timeout(...)]` on pump-hosted tests, following
  `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs:355`.
- Moq for mocks and stubs.
- FluentAssertions `.Should()` chains, with an explicit `because:` argument on every non-obvious
  assertion (precedent `Part3.cs:376-385`).
- Explicit `// Arrange` / `// Act` / `// Assert` comments (canonical form
  `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:19, :23, :30`).
- No temporary files, no `Task.Delay`, no `Thread.Sleep`, no real wall-clock waits, no external
  process, no network. Tests must be independent, isolated, deterministic, and order-independent.
- Use a distinct `WebView2` control instance per test so the process-wide owner registry cannot
  couple tests to one another.

### Per-defect strategy (research §5.2)

| Defect | Genuine failing-first regression test? | Mechanism |
| --- | --- | --- |
| #476 defect 1 | **Yes** | Recording `SynchronizationContext` through the internal 3-arg constructor |
| #458 | **Yes** | Two hosts over one control; internal attachment-state observation point |
| #476 defect 2 | **No** — structural proxy only | Reflection assertion on the backing field |
| #477 defect 2 | **Yes** | Direct guard tests on the concrete type |
| #477 defect 1 | No behavioural test — documentation only | n/a |

**#476 defect 1 — YES.** Construct the `WebView2` on `WinFormsPumpHost`
(`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs:26`, registered at `QuickFiler.Test.csproj:161`)
via `host.InvokeAsync(() => new Microsoft.Web.WebView2.WinForms.WebView2())`. Construct the host
through the **internal three-argument constructor** with a **recording** `SynchronizationContext`
and a recording error sink — `new BreadcrumbUiDispatcher(recordingContext, errors.Add)`, the exact
pattern at `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:166`. Call `PostMessageJson`
and `NavigateToString` from the MSTest thread (not the boundary) and assert the recording context
observed exactly one `Post`. Because the recording context never **drains** the posted action, the
control is never touched and no WebView2 runtime is involved. This test fails today (zero posts; the
control is touched inline at `WebView2BreadcrumbHost.cs:68` and `:74`) and passes after the fix.

**#458 — YES.** Construct one `WebView2` on the pump; construct host A over it, then host B over it;
assert that A is detached and B is the registered owner. The observation point is an `internal`
member on `WebView2BreadcrumbHost` (for example `internal bool IsAttached { get; }`), visible to
`QuickFiler.Test` via `QuickFiler/Properties/AssemblyInfo.cs:5`. **Prefer an assertion about the
host's attachment state over one about the registry's internals**, because the former survives a
later change of registry mechanism. A reflection-based assertion on the raw handler count of
`WebView2.CoreWebView2InitializationCompleted` **must not** be the primary assertion: whether the
SDK implements that event as a field-like backing delegate or through a WinForms `EventHandlerList`
is UNVERIFIED (research §9 item 2). This test fails today because A stays attached.

**#476 defect 2 — NO genuine race test is possible.** A memory-ordering defect cannot be made to
fail deterministically. On x86/x64 the missing barrier is very unlikely to produce an observable
reordering, and a test that spins threads hoping to catch one would violate the determinism
requirement in `.claude/rules/general-unit-test.md` and `CLAUDE.md` §UT1. The substitute is a
**structural** test asserting by reflection that `WebView2BreadcrumbHost` declares an explicit
backing field for the initialization flag and that `IsCoreInitialized` is **not** an auto-property
(an auto-property's backing field carries `[CompilerGenerated]` and the name
`<IsCoreInitialized>k__BackingField`). This is a genuine failing-first test — it fails against `:54`
today — but it is a **structural proxy, not a proof of the race**, and the acceptance criterion says
so explicitly. Precedent for structural/reflection assertions:
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:23-39` and
`QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs`. A secondary behavioural test may additionally
pin the publication order at `:131-135`, but it is weaker and does not replace the structural test.

**#477 defect 2 — YES, directly.** Guard tests go in the existing
`QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs`, which is already registered at
`QuickFiler.Test.csproj:159`, so **no `.csproj` edit is needed**. Assert `ArgumentNullException` with
the correct `ParamName` for a null `cacheFolder` and a null `control`, and `ArgumentException` for a
whitespace `cacheFolder`. The throw paths do not reach the SDK, so no runtime is involved.

**#477 defect 1 — documentation only.** No behavioural test. Verified by diff review against the
acceptance criteria.

### Edge cases and negative scenarios to cover

- Predecessor detach when `_control.CoreWebView2` is null (the predecessor never completed
  initialization) must not throw.
- Constructing a host with a null `control` or null `initializer` still throws
  `ArgumentNullException` with the correct `ParamName` (existing behaviour at `:45-46`, preserved).
- `PostMessageJson` before initialization: the pre-dispatcher window executes inline; after
  initialization it posts. Both must leave the existing null-`CoreWebView2` log-and-drop behaviour
  intact.
- Guard tests assert `ParamName`, not just exception type.

### Coverage impact and targets

Per `CLAUDE.md` §UT2, new or modified members target `>= 90%` coverage, and changed lines must not
regress. The seams introduced here (internal constructor, dispatcher routing decision, registry
detach, `Volatile` accessor, argument guards) enter measurement for the first time — see the
Coverage and Exemption Impact section. Capture the repository coverage figure before and after and
record the delta in the feature evidence folder.

### Toolchain commands to run, in this exact order

1. `dotnet tool run csharpier format .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

If any step fails or auto-fixes files, restart from step 1.

### Manual validation

None required. All five defects are addressed by automated tests or by diff review, and no
production repro exists for #458 (see Premise correction 1).

---

## Behaviour Changes and Residual Risks

Each item is stated as a genuine change, not minimised.

1. **`PostMessageJson` and `NavigateToString` become asynchronous relative to the caller.**
   `BreadcrumbUiDispatcher.Dispatch` is invoked fire-and-forget (`_ = _dispatcher.Dispatch(...)`),
   so both members return before the SDK call executes. Payload **order is preserved** by the single
   `SynchronizationContext.Post` queue, and `BreadcrumbOutboundQueue`'s flush loop
   (`BreadcrumbOutboundQueue.cs:61-64`) enqueues in order. This is a real behaviour change and is
   precisely why epic #136's no-behaviour-change NFR excluded this work and forced it into its own
   issue.

2. **A pre-initialization window remains unmarshalled.** Under variant V1 the dispatcher is captured
   in `InitializeAsync`, so before `InitializeAsync` has run there is no captured dispatcher and
   calls fall back to inline execution on the caller's thread. The fix is therefore **not total**:
   it closes the window in which the production readers actually call (both call only after
   `IsCoreInitialized` is true, which is after `InitializeAsync`), but it does not close the window
   before that. This limitation is stated rather than implied.

3. **The owner registry introduces process-wide static state.** Mitigated by keying on control
   identity through a `ConditionalWeakTable`, whose entries are collectible with the control, and by
   requiring a distinct control instance per test so tests stay independent. The registry adds no
   retention edge that outlives the control, and the net effect is a reduction in retention because
   the `control -> stale host` edge is removed.

4. **The predecessor detach path must tolerate `_control.CoreWebView2 == null`.** A predecessor may
   never have completed initialization and therefore may never have subscribed to
   `core.WebMessageReceived`. `DetachCore` must null-check before attempting that unsubscription.

5. **Notification counts change** for the two-hosts-over-one-control case: exactly one host now
   handles initialization completion. That is the behaviour #458 asks for, and it is unobservable in
   the current production wiring because that case does not occur there (Premise correction 1).

6. **New throwing preconditions on `WebView2CoreInitializer`** for arguments that were previously
   undefined behaviour. No in-repo caller crosses the guarded boundary (see Interface Contract
   Change).

7. **Coverage figure movement** from removing the class-level exemption on `WebView2BreadcrumbHost`.
   See Coverage and Exemption Impact for the mitigation.

8. **Residual open questions carried from research §9**, none of which block the design: the ambient
   `SynchronizationContext` at `EfcFormController.cs:836` (UNVERIFIED — the reason V1 is preferred);
   the SDK's event implementation (UNVERIFIED — the reason the handler-count assertion is not
   primary); `ConditionalWeakTable.AddOrUpdate` availability on `net481` (UNVERIFIED — not used);
   whether `CoreWebView2Environment.CreateAsync` tolerates a null `options` (UNVERIFIED — decided at
   implementation time); off-pump bare `new WebView2()` construction (UNVERIFIED — mitigated by
   constructing on the pump).

---

## Acceptance Criteria

Authoritative acceptance-criteria source for work mode `full-bug`. Check an item off only after the
work satisfying it is implemented **and** verified.

**#458 — predecessor detach via a per-control owner registry**

- [ ] `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` declares a `private static readonly ConditionalWeakTable<WebView2, WebView2BreadcrumbHost>` owner registry and a `private static readonly object` gate, and the constructor performs its lookup-detach-replace sequence under that gate using only `TryGetValue`, `Add`, and `Remove`.
- [ ] The dead `_control.CoreWebView2InitializationCompleted -= OnCoreInitializationCompleted;` at `WebView2BreadcrumbHost.cs:49` no longer exists; the predecessor's subscription is removed by invoking a detach on the **predecessor instance**, and the misleading comment at `:48` is corrected or removed.
- [ ] A regression test in `QuickFiler.Test` constructs one `WebView2` on `WinFormsPumpHost`, constructs host A then host B over it, and asserts through an `internal` attachment-state member on `WebView2BreadcrumbHost` that A is detached and B is the registered owner. The test fails against the pre-fix code and passes after. Its primary assertion is about the host's attachment state, not about a reflected SDK handler count.
- [ ] The detach path tolerates `_control.CoreWebView2 == null` (a predecessor that never completed initialization) without throwing, and a test covers that case.
- [ ] A `_control.Disposed` subscription detaches the host and removes its registry entry.

**#476 defect 1 — UI marshalling of every SDK touch**

- [ ] `WebView2BreadcrumbHost` declares an `internal` three-argument constructor `(WebView2, IWebViewCoreInitializer, BreadcrumbUiDispatcher)`, and the existing **public** two-argument constructor chains to it with an unchanged signature, so `QuickFiler/Controllers/EfcFormController.cs:836-839` requires no edit.
- [ ] `NavigateToString` executes `_control.NavigateToString(html)` inside a single `BreadcrumbUiDispatcher.Dispatch(...)` callback.
- [ ] `PostMessageJson` performs the `_control.CoreWebView2` read, the null guard with its existing log-and-drop message, and `core.PostWebMessageAsJson(json)` inside **one** `Dispatch` callback.
- [ ] `BreadcrumbUiDispatcher.DispatchValue` is not used anywhere in `WebView2BreadcrumbHost.cs`.
- [ ] The dispatcher is constructed in `InitializeAsync` from the `uiSyncContext` argument (variant V1) using `new BreadcrumbUiDispatcher(uiSyncContext, sink)`; `BreadcrumbUiDispatcher.CaptureCurrent()` is not called from `WebView2BreadcrumbHost.cs`, and the constructor gains no new throwing precondition.
- [ ] `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` is unmodified.
- [ ] A regression test constructs the host through the internal three-argument constructor with a **recording** `SynchronizationContext` and a recording error sink, calls `PostMessageJson` and `NavigateToString` from the test thread, and asserts the recording context observed exactly one `Post` per call. The recording context never drains the posted action, so no WebView2 runtime is involved. The test fails against the pre-fix code.

**#476 defect 2 — synchronized state publication (structural evidence only)**

- [ ] `IsCoreInitialized` is backed by an explicit private field read through `Volatile.Read`; the auto-property at `WebView2BreadcrumbHost.cs:54` no longer exists.
- [ ] The write uses `Volatile.Write` and remains strictly **after** the `core.WebMessageReceived` subscription (currently `:131-132`) and **before** `CoreInitialized?.Invoke(...)` (currently `:135`).
- [ ] A structural test asserts by reflection that `WebView2BreadcrumbHost` declares an explicit backing field for the initialization flag and that no `[CompilerGenerated]` `<IsCoreInitialized>k__BackingField` exists. **This evidence is a structural proxy for the memory-ordering fix and is explicitly NOT a proof that the race is eliminated**; no deterministic race test is possible without violating the determinism requirement in `.claude/rules/general-unit-test.md`.

**#477 defect 1 — interface contract documentation (Option B)**

- [ ] `QuickFiler/Viewers/IWebViewCoreInitializer.cs` no longer claims a 1:1 forward to the WebView2 SDK, and its `CreateEnvironmentAsync` documentation states that `browserExecutableFolder` is passed as `null` unconditionally as a deliberate Evergreen-only decision, with `<exception>` documentation for the guards.
- [ ] The `CreateEnvironmentAsync` and `EnsureCoreWebView2Async` **member signatures on `IWebViewCoreInitializer` are unchanged**, and no in-repo caller of either member and no Moq `Setup` expression is modified.
- [ ] The coverage-exemption rationale at `QuickFiler/Viewers/WebView2CoreInitializer.cs:8-14` is restated on the accurate ground — external Evergreen runtime plus user-data-folder creation on disk — with no residual "1:1 forwarding" claim.

**#477 defect 2 — argument guards**

- [ ] `WebView2CoreInitializer.CreateEnvironmentAsync` throws `ArgumentNullException(nameof(cacheFolder))` for a null `cacheFolder` and `ArgumentException` for a whitespace `cacheFolder`, before any SDK call.
- [ ] `WebView2CoreInitializer.EnsureCoreWebView2Async` throws `ArgumentNullException(nameof(control))` for a null `control`, before any SDK call, and does **not** guard `environment` (null is a valid SDK input meaning "default environment").
- [ ] Guard tests are added to the existing `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs`, assert the exception type **and** `ParamName`, and require no `QuickFiler.Test.csproj` edit.
- [ ] All eleven Moq mock sites listed in the Interface Contract Change section, including the eight `MockBehavior.Strict` sites, pass unmodified.

**Scope containment**

- [ ] None of the following files is modified: `QuickFiler/Controllers/EfcFormController.cs`, `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, `QuickFiler/Controllers/EfcItemController.cs`, `QuickFiler/Viewers/WebView2Messenger.cs`, `QuickFiler/Viewers/BreadcrumbMessengerHub.cs`, `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`, `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`, `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`, `QuickFiler/QuickFiler.csproj`.
- [ ] The production diff is confined to `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`, `QuickFiler/Viewers/WebView2CoreInitializer.cs`, and `QuickFiler/Viewers/IWebViewCoreInitializer.cs`.
- [ ] The follow-up defect at `QuickFiler/Controllers/EfcItemController.cs:223-227` (direct `CoreWebView2Environment.CreateAsync` call bypassing the seam) is **not** fixed by this feature, is recorded in this spec's Cross-Feature Notes, and is handed to the orchestrator for promotion through the promotion lifecycle. The executor does not create the issue; the promotion tooling is orchestrator-only, and the promotion is deliberately deferred out of the epic-preparation run because the issue-promotion tool has no idempotent path.
- [ ] If a new test file is added, its `Compile Include` entry is inserted immediately after `QuickFiler.Test/QuickFiler.Test.csproj:159` and the surrounding ItemGroup is not re-sorted.

**Nullable participation**

- [ ] Neither `QuickFiler/Viewers/WebView2CoreInitializer.cs` nor `QuickFiler/Viewers/IWebViewCoreInitializer.cs` contains a `#nullable enable` directive after the change; nullability in those two files is expressed only through runtime `ArgumentNullException` / `ArgumentException` guards.
- [ ] All new code in `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` is nullable-clean, producing no `CS86xx` diagnostic under `/p:TreatWarningsAsErrors=true`.

**Coverage exemption correctness**

- [ ] The class-level `[ExcludeFromCodeCoverage]` at `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:29` is removed, and the class remarks no longer assert that every member forwards 1:1 to the WebView2 SDK.
- [ ] Member-level `[ExcludeFromCodeCoverage]` with an accurate, member-specific rationale is applied only to the genuinely host-bound members of `WebView2BreadcrumbHost`; the internal constructor, the dispatcher-routing decisions, the registry detach path, and the `Volatile` state accessor are all measured.
- [ ] Wherever a member combines a testable decision with a host-bound SDK forward, the SDK forward is extracted into a small private method that carries the member-level attribute, so the testable decision is measured.
- [ ] In `WebView2CoreInitializer`, the argument guards are measured (not exempt) and the two SDK forwards carry an exemption whose rationale is the external Evergreen runtime plus user-data-folder creation on disk.
- [ ] The repository coverage figure before and after the change is captured and the delta is recorded in the feature evidence folder. The baseline figure is recorded under `docs/features/active/webview2-host-initializer-defects-476/evidence/baseline/` and the post-change figure and delta under `docs/features/active/webview2-host-initializer-defects-476/evidence/qa-gates/`. These are the canonical evidence sub-paths; `evidence/coverage/` is not a canonical sub-path and must not be used.

**Test policy conformance**

- [ ] Every new test uses MSTest `[TestClass]`/`[TestMethod]`, Moq for mocks, and FluentAssertions `.Should()` with a `because:` argument on non-obvious assertions, and carries explicit `// Arrange` / `// Act` / `// Assert` comments.
- [ ] No new test creates a temporary file, uses `Task.Delay` or `Thread.Sleep`, waits on wall-clock time, or depends on an external process, network, or the WebView2 Evergreen runtime.
- [ ] Each host regression test uses a distinct `WebView2` control instance so the process-wide owner registry cannot couple tests, and the tests pass in any order.

**Toolchain**

- [ ] A single clean toolchain pass completed in this exact order with no failures and no file rewrites: `dotnet tool run csharpier format .`; then `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; then `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`; then `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`.

---

## Risks & Mitigations

| Risk | Mitigation |
| --- | --- |
| `PostMessageJson` / `NavigateToString` become asynchronous, changing timing on a live UI path | Order is preserved by the single `Post` queue; the change is intentional and is why #136 deferred this work. Covered by a regression test asserting one `Post` per call. |
| Pre-initialization window remains unmarshalled under variant V1 | Documented explicitly. Both production readers call only after `IsCoreInitialized` is true, which is after `InitializeAsync`. |
| Ambient `SynchronizationContext` at the construction site is UNVERIFIED | Variant V1 avoids the dependency entirely by capturing from `InitializeAsync`'s `uiSyncContext` argument. V2 is not adopted. |
| Process-wide static owner registry couples tests | Distinct control instance per test; entries are collectible with the control. |
| Removing the class-level coverage exemption moves the repository coverage figure | Member-level exemptions keep the added denominator small and bounded; each newly measured seam gains a regression test in the same change; the delta is captured as evidence. |
| Structural test for #476 defect 2 could be mistaken for proof of the race fix | The acceptance criterion states in its own text that the evidence is a structural proxy, not a proof. |
| Textual conflict with a sibling epic child on a `Compile Include` block | No production file is added, so `QuickFiler/QuickFiler.csproj:391-413` is untouched. A new test file adds one line to `QuickFiler.Test.csproj`, which no sibling owns. |
| `options` guard could narrow behaviour if the SDK tolerates null | Decided at implementation time; both in-repo callers already supply non-null. If tolerance is confirmed, guard `cacheFolder` only and document the tolerance. |

---

## Rollout & Follow-up

**Rollout**

Standard epic-child delivery: land on the child branch, merge into
`epic/quickfiler-bug-family-integration`, then to `main` through the epic's integration pull
request. No staged rollout, no feature flag, no migration.

**Follow-up**

1. Promote the `QuickFiler/Controllers/EfcItemController.cs:223-227` seam-bypass defect to its own
   GitHub issue (see Cross-Feature Notes item 3).
2. Resolve the UNCONFIRMED question of whether a fixed-version WebView2 distribution is a product
   requirement (`…-iwebviewcoreinitializer-contract-defects.md:121`). If confirmed, open a
   correctly-scoped issue for #477 Option A that includes `QfcItemController.ViewerSetup.cs:64-67`,
   `BreadcrumbPopupUiOperations.cs:388`, and `EfcItemController.cs:223-227`.
3. Consider aligning `BreadcrumbBridgeRouter.cs:54` (subscribes `_host.MessageReceived` and never
   unsubscribes) with the host's new lifecycle, in the feature that owns that file.
4. Consider whether the two WebView2 adapters (`WebView2BreadcrumbHost` and `WebView2Messenger`)
   should converge on one documented thread-affinity contract, which this change makes possible for
   the first time.

**Links**

- Issue #476 — https://github.com/drmoisan/TaskMaster/issues/476 (primary)
- Issue #458, Issue #477 — also closed by this feature
- Issue #455 — F13, breadcrumb drop-down and WebView2 host coverage, where all three were found
- Issue #136 — parent epic; its no-behaviour-change NFR is why these were deferred
- Issue #349 — introduced the WebView2 breadcrumb control
- Issue #432 — F1 coverage ledger; the ratified exemption rationale referenced by #477
- Research artifact — `docs/features/active/webview2-host-initializer-defects-476/research/2026-08-24T00-45-webview2-host-initializer-defects-research.md`
