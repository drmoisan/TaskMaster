# Research: `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` (F12 / issue #495)

- Timestamp: 2026-08-08T02-10
- Epic: `docs/features/epics/quickfiler-per-file-coverage/epic.md` (#136), child F12
- Child issue: #495
- Branch: `feature/quickfiler-breadcrumb-bridge-coverage-r2` (based on
  `epic/quickfiler-per-file-coverage-integration`)
- Scope: ONE production file, per the #136 one-research-artifact-per-file mandate.
- Sibling artifacts: `2026-08-08T01-15-breadcrumb-bridge-coordinator.md` (format template and
  quality bar; its **R1** and **LD-1** are answered from this file's side in §4.1 and §7.1).

---

## 0. Executive summary

| Question | Answer |
| --- | --- |
| Baseline confirmed? | **Yes, exactly** — 294 coverable lines, 100.00% line, 96.61% branch. First F12 file whose numeric table survives re-measurement unchanged. |
| Residual untaken branch outcomes | **4** (of 118), on source lines `:326`, `:329`, `:442`, `:451`. |
| Closable? | **All 4, with zero production change**, from `QuickFiler.Test` through the existing `InternalsVisibleTo` grant. Projected 118/118 = **100.00% branch**. |
| Production edit verdict | **None required, none recommended.** |
| Dominant risk | This file's 100% line figure is **not** self-sustaining: 124 of its 294 coverable lines belong to two types (`BreadcrumbCollapsedAttachment`, `BreadcrumbResourceOwner`) that no test names in a file F12 would think of as "hub tests", and 13 of them are covered only as a side effect of live `ItemViewer` construction in **F13- and F14-owned** test files. |
| Latent defects | 3 verified. LD-1 (concur + refine the sibling's LD-1) and LD-2 recommended for promotion; LD-3 recorded only. |

---

## 1. Current State — verified

### 1.1 File shape

`QuickFiler/Viewers/BreadcrumbMessengerHub.cs` is **456 physical lines** (last line, the namespace
close brace, is `BreadcrumbMessengerHub.cs:456`). Against the 500-line ceiling in
`.claude/rules/general-code-change.md` § File Size Limit that is **44 lines of headroom** — three
times the sibling coordinator's 13, but still not room for a seam class.

**The file declares three top-level types, not one.** This is the single most consequential shape
fact in the artifact and it is not reflected anywhere in the F12 brief:

| Type | Declared at | Accessibility | Base / interfaces | `partial`? |
| --- | --- | --- | --- | --- |
| `BreadcrumbMessengerHub` | `:15` | `public sealed` | `IWebViewMessenger, IDisposable` | no |
| `BreadcrumbMessengerHub.Attachment` | `:17` (nested) | `private sealed` | — | no |
| `BreadcrumbMessengerHub.CachedState` | `:35` (nested) | `private sealed` | — | no |
| `BreadcrumbCollapsedAttachment` | `:277` | `internal sealed` | `IDisposable` | no |
| `BreadcrumbResourceOwner` | `:436` | `internal sealed` | `System.ComponentModel.Component` | no |

- **No `[ExcludeFromCodeCoverage]` anywhere in the file.** Verified by full read: the `using` set is
  `System`, `System.Collections.Generic`, `System.ComponentModel`, `System.Linq`,
  `System.Threading.Tasks`, `UtilitiesCS.OutlookObjects.Folder` (`:2-7`). There is no
  `System.Diagnostics.CodeAnalysis` import and no attribute usage.
- No `System.Windows.Forms` and no `Microsoft.Office.Interop.Outlook` reference. `System.ComponentModel`
  is present solely because `BreadcrumbResourceOwner` derives from `Component` (`:436`) — a
  non-visual, handle-free type, so **no STA apparatus is required to construct any type in this file**.
- No WebView2 type is referenced. The WebView2 boundary is reached only through the
  `IWebViewMessenger` abstraction.

**Constructor surface and test reachability.** `QuickFiler/Properties/AssemblyInfo.cs:5` contains
`[assembly: InternalsVisibleTo("QuickFiler.Test")]`, so every `internal` member here is directly
callable from the test assembly. It already is:

- `new BreadcrumbMessengerHub()` — implicit public parameterless ctor; 22 direct construction sites
  across 7 test files (§4.2).
- `internal BreadcrumbCollapsedAttachment(BreadcrumbMessengerHub, BreadcrumbCollapsedSurfaceController)`
  (`:288-291`) — constructed at `BreadcrumbMessengerHubTests.cs:226`,
  `BreadcrumbMessengerHubCoverageTests.cs:153/154/174/240/272/306/338`.
- `internal BreadcrumbResourceOwner(Action dispose)` (`:440`) — **constructed by zero tests.** Its only
  construction site anywhere is production: `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:287`.

**File-cohesion observation (not a defect, not in scope).** Housing an attachment coordinator and a
`Component`-derived resource holder inside a file named for the messenger hub is in tension with
`.claude/rules/general-code-change.md` § "Keep modules cohesive". Splitting is technically
straightforward (both extra types are `internal sealed` and have no partial dependency on the hub),
but it would require two `<Compile Include>` additions to `QuickFiler/QuickFiler.csproj`, two new
ledger rows under the epic's "Mid-Wave File Creation" rules, and would change this file's
denominator mid-epic for zero coverage benefit. **Recommend against; record as an optional
post-epic follow-up.**

### 1.2 Collaborator table with owning child

| Symbol | Declared at | Owner | How this file uses it |
| --- | --- | --- | --- |
| `IWebViewMessenger` | `QuickFiler/Viewers/IWebViewMessenger.cs:13` | **F13** | The hub **implements** it (`:15`) and **consumes** it (`Attach`/`Detach`/`PostToSurface`). Public interface, 2 members. |
| `BreadcrumbCollapsedSurfaceController` | `QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs:11` | **F13** | Constructor dependency of `BreadcrumbCollapsedAttachment` (`:290`); `AttachAsync`/`ReadyMessenger`/`Reset`/`Dispose` consumed at `:368`, `:372`, `:383`, `:415`, `:417`. |
| `BreadcrumbNavigationReadiness` | `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs:19` | **F13** | Second element of the candidate tuple (`:298`); `Dispose` at `:328`, `:340`. |
| `BreadcrumbSelectorViewMode` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs` | **UtilitiesCS** | Attachment mode (`:21`, `:64`, `:232`, `:375`). |
| `BreadcrumbSelectorViewMessage` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs:159` | **UtilitiesCS** | Cast target in `RewriteSelectorMode` (`:214`). |
| `BreadcrumbSelectorMessageSerializer` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs:142` | **UtilitiesCS** | `Parse` at `:215`. |
| `log4net.LogManager` | third-party | — | `:269-271`, the only logging in the file. |
| `BreadcrumbBridgeCoordinator` | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:25` | **F12** | *Inbound* dependency only — the coordinator type-tests `_messenger is BreadcrumbMessengerHub` at `BreadcrumbBridgeCoordinator.cs:297`. This file does not reference it. |
| `BreadcrumbItemViewerLifecycleCoordinator` | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:16` | **F12** | *Inbound* — holds the hub and the attachment; calls `Attach`/`Detach`/`Dispose` at `:215`, `:254`, `:260`, `:264`, `:277`, `:288`. |
| `ItemViewer` | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:263-267`, `:287` | **F14** | *Inbound* — the only production constructor of `BreadcrumbMessengerHub`, `BreadcrumbCollapsedAttachment`, and `BreadcrumbResourceOwner`. |

**Mapping the WebView2 surface boundary precisely.** This file sits exactly one abstraction step
away from WebView2 and never touches the SDK:

```
BreadcrumbMessengerHub.PostToSurface (:206)   -> IWebViewMessenger.PostJson   [F13-owned interface]
   production implementation: WebView2Messenger.PostJson (WebView2Messenger.cs:55)  [F13, [ExcludeFromCodeCoverage] at :20]
      -> BreadcrumbUiDispatcher.Dispatch  [F13]
         -> CoreWebView2.PostWebMessageAsJson (WebView2Messenger.cs:66)  [WebView2 SDK]
```

The interface the hub implements *and* consumes is `IWebViewMessenger`; it does **not** consume
`IWebViewCoreInitializer` (`QuickFiler/Viewers/IWebViewCoreInitializer.cs`, F13) at all. The
consequence for planning is favourable: **every WebView2 dependency of this file is already behind a
2-member public interface that a plain in-memory double satisfies**, which is why the existing
tests need no SDK, no runtime, and no dispatcher.

The consequence for risk is less favourable and is developed in §4: two of the three types in this
file depend on **F13-owned `internal` types** (`BreadcrumbCollapsedSurfaceController`,
`BreadcrumbNavigationReadiness`) whose signatures F13's spec — not F12 — controls.

### 1.3 Concurrency and determinism inventory

Verified by full read of all 456 lines plus a targeted grep for
`DateTime|Stopwatch|Timer|Task.Delay|Thread.Sleep|TimeProvider|SynchronizationContext|Interlocked|volatile|ConfigureAwait|CancellationToken`,
which returned **zero matches**.

| Construct | Present? | Line anchors |
| --- | --- | --- |
| `lock` | **Yes, 5 sites, one monitor** | `_sync` declared `:47`; taken at `:71` (`Attach`), `:105` (`Detach`), `:126` (`PostJson`), `:141` (`Dispose`), `:160` (`OnSurfaceMessageReceived`) |
| `Interlocked` / `Volatile` / `volatile` | no | — |
| Timer / clock / `TimeProvider` / `Stopwatch` / `DateTime` | **no** | — |
| `Thread.Sleep` / `Task.Delay` | **no** | — |
| `SynchronizationContext` | no (direct) | — |
| `async` / `await` | 1 method, 1 await | `async Task CompleteAsync` `:357`; `await _controller.AttachAsync(...)` `:368` |
| `ConfigureAwait` | **absent, deliberately** | `:367` comment: "Preserve the ItemViewer synchronization context for the hub subscription/replay." |
| Fire-and-forget discard | 1 | `_ = CompleteAsync(messenger, readiness, generation, completion);` `:336` |
| `TaskCompletionSource` | 1 factory | `:312`, `:431-432` (`TaskCreationOptions.RunContinuationsAsynchronously`) |
| `CancellationToken` | **no** | — |
| Disposal flags | 3 | `BreadcrumbMessengerHub._disposed` `:55`; `BreadcrumbCollapsedAttachment._disposed` `:286`; `BreadcrumbResourceOwner._dispose` nulled at `:450` |
| Generation / re-entrancy guard | 2 | hub `_sequence` `:54`; attachment `_generation` `:285`, checked by `IsCurrent` `:420-423` |
| Re-entrancy guard proper | **none** | the `lock` is the only mutual exclusion; `Monitor` is re-entrant |

#### Determinism finding — the brief's "injected clock and fake timers" instruction is REFUTED for this file

`docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/issue.md:70` and `:95`,
and `spec.md:69-70` and `:112`, direct this child to "use an injected clock and fake timers". **There
is no time dependency of any kind in this file** — zero occurrences of `DateTime`, `Stopwatch`,
`Timer`, `Task.Delay`, `Thread.Sleep`, or `TimeProvider`. Introducing a clock seam here would add a
seam with nothing to control, consuming production headroom for no benefit.

This is the same conclusion F13 ratified in
`docs/features/active/2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455/spec.md:381-390`
(§8.1: "Determinism here is **scheduler** control, not clock control. Any plan task that introduces
an injected clock or a fake-timer facility is out of scope and must be rejected"), and the same
conclusion the sibling `BreadcrumbBridgeCoordinator` artifact reached for its file.

**Refinement for this file specifically.** Determinism here is not even scheduler control — it is
**completion-source control**. The one asynchronous edge (`:368`) is driven entirely by a
caller-supplied `BreadcrumbNavigationReadiness`, and the returned `Task<bool>` is a
`TaskCompletionSource<bool>` the test awaits. With MSTest's default `SynchronizationContext.Current == null`,
the unconfigured `await` at `:368` resumes on the thread pool and the awaited
`completion.Task` (created `RunContinuationsAsynchronously`, `:432`) is the synchronization point. No
`SynchronizationContext`, no pump, no `Drain()` is required, and none of the ten existing tests in
`BreadcrumbMessengerHubCoverageTests.cs` installs one. **Any plan task that installs a
`SynchronizationContext` for this file is also unnecessary.**

#### Lock-ordering picture and inversion check

Outward calls made **while `BreadcrumbMessengerHub._sync` is held**:

| Line | Outward call | Under lock taken at |
| --- | --- | --- |
| `:84` | `messenger.MessageReceived += handler` | `:71` |
| `:85` -> `:179` -> `:206` | `attachment.Messenger.PostJson(json)` (replay) | `:71` |
| `:133` -> `:206` | `attachment.Messenger.PostJson(json)` (broadcast) | `:126` |
| `:113` / `:150` -> `:265` | `attachment.Messenger.MessageReceived -= attachment.Handler` | `:105` / `:141` |

Inbound path: `OnSurfaceMessageReceived` (`:157-173`) takes `_sync` at `:160`, snapshots the
subscriber at `:170`, **releases the lock**, and invokes at `:172` outside it. That is correct
discipline and means the hub's own inbound path introduces **no** reverse ordering.

The full ordering picture across the cluster:

```
order A (outbound render):   BreadcrumbCoordinatorUpgradeLifetime._sync   (TryRunCurrent, UpgradeLifetime.cs:139)
                          -> BreadcrumbMessengerHub._sync                 (PostJson, :126)
                          -> CoreWebView2.PostWebMessageAsJson            (WebView2Messenger.cs:66)

order B (re-entrant inbound, only if a surface raises MessageReceived synchronously from PostJson):
                             BreadcrumbMessengerHub._sync                 (:126, still held)
                          -> BreadcrumbMessengerHub._sync                 (:160, re-entrant, succeeds)
                          -> BreadcrumbBridgeCoordinator inbound handling (:172)
                          -> BreadcrumbCoordinatorUpgradeLifetime._sync
```

A lock-order **inversion pair** therefore exists in shape (A: lifetime -> hub; B: hub -> lifetime).
**No deadlock is demonstrable**, because order B is reachable only by synchronous re-entrancy on the
same thread, `Monitor` is re-entrant, and every path into the hub is marshalled onto the single
`BreadcrumbUiDispatcher` boundary. The residual hazard is atomicity, not deadlock — see LD-1 (§7.1).

`BreadcrumbCollapsedSurfaceController` (F13) also has a `_sync` (`:17`) but never calls the hub, and
it releases its lock before every outward `SafeDispose` (`:75-76`, `:99-100`, `:162-163`, `:235-238`).
`BreadcrumbCollapsedAttachment.CompleteAsync` calls `_hub.Attach` at `:375` outside any lock. Neither
adds an ordering edge.

---

## 2. Measured Baseline — independently re-measured, not taken on trust

Source: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`.

### 2.1 Harness rules applied

- **Exactly one `<class>` element** carries `filename="QuickFiler\Viewers\BreadcrumbMessengerHub.cs"`,
  at XML line **11407**, closing at XML **12217**. A grep of the entire report for
  `BreadcrumbCollapsedAttachment` returns **one** hit — a method *signature* at XML 7852 — and for
  `BreadcrumbResourceOwner`, **zero**. Neither has its own `<class>` element.
- Nevertheless the class-level `<lines>` block (XML **11718-12216**) spans source lines **19 through
  454**, i.e. it already contains every line of all three types and their lifted lambdas. This report
  is the *merged* form described by epic issue **#478**: a correct class-level union with a
  primary-only `<methods>` subtree. Union-by-filename is therefore already satisfied for this file;
  no cross-class max-hits merge was needed.
- Counted **only** the class-level `<lines>` block. Never `class.iter('line')`, never `.//lines/line`
  (epic directive 3, issue **#441**) — the method-level blocks for `Attach`, `.ctor` and others
  duplicate source lines 47-53 and 65-92 and would double-count them.
- **Recomputed** both rates from the `<line>` elements. The emitted attributes were not read as
  figures (see §2.4).

### 2.2 Recomputed figures

| Metric | Value |
| --- | --- |
| Coverable lines (class-level `<line>` count) | **294** |
| Lines with `hits="0"` | **0** |
| **Line coverage** | **100.00%** |
| Branching lines (`branch="True"`) | 48 |
| Branch outcomes valid (sum of `condition-coverage` denominators) | **118** |
| Branch outcomes taken (sum of numerators) | **114** |
| **Branch coverage** | **114 / 118 = 96.6102% -> 96.61%** |

Denominator arithmetic, for auditability: 40 branching lines carry 2 outcomes (80), five carry 4
(`:177`, `:308`, `:322`, `:326`, `:421` = 20), three carry 6 (`:162`, `:185`, `:369` = 18);
80 + 20 + 18 = 118. Four outcomes are untaken (`:326` 3/4, `:329` 1/2, `:442` 1/2, `:451` 1/2), so
114 are taken.

**Verdict: the brief's "294 lines / 100.0% / 96.6%" is CONFIRMED exactly.** Both floors
(>= 80% line, >= 75% branch, epic "Coverage-Target Reconciliation") are cleared today, so the bar is
**retain-or-improve on both axes**, plus closure of the four residual outcomes if cheap — and §3
shows all four are cheap.

### 2.3 Per-type attribution of the 294 lines (needed for §4)

Derived by partitioning the class-level block on source-line ranges against the type declarations
in §1.1:

| Type | Source-line ranges in the block | Coverable lines | Branch outcomes (taken / valid) |
| --- | --- | --- | --- |
| `BreadcrumbMessengerHub` incl. nested `Attachment` / `CachedState` | 19-28, 37-41, 47-53, 65-273 | **170** | 58 / 58 (100%) |
| `BreadcrumbCollapsedAttachment` | 288-432 | **111** | 52 / 54 (96.30%) |
| `BreadcrumbResourceOwner` | 440-454 | **13** | 4 / 6 (66.67%) |
| **Total** | | **294** | **114 / 118** |

The hub type proper is already at 100% line **and** 100% branch. **Every open branch outcome, and
every retain-or-improve risk of consequence, lives in the two non-hub types.**

### 2.4 Emitted attributes are wrong — do not read them

The `<class>` element emits `line-rate="1" branch-rate="0.977273"`. The branch figure is
43/44 = 0.977273, computed over the primary-method subtree only; the true per-file figure is
114/118 = 0.96610. The line figure happens to agree at 1.0 only because the file has no uncovered
lines at all. This is a fresh instance of the distortion the epic documents at
"Directives for F1's Ledger and Harness" item 5, and it runs in the **optimistic** direction here
(0.977 vs 0.966), exactly the direction that would falsely pass a gate.

### 2.5 Line-number drift check against the working tree: none

Every anchor in the Cobertura block resolves to the construct predicted on the current file:
`:19` = `Attachment` ctor, `:37` = `CachedState` ctor, `:47` = `_sync` field, `:65` = `Attach` body
open, `:273` = `SafeUnsubscribe` close, `:288` = `BreadcrumbCollapsedAttachment` ctor, `:326` =
`if (_disposed || generation != _generation)`, `:329` = `(messenger as IDisposable)?.Dispose();`,
`:442` = `_dispose = dispose ?? throw ...`, `:451` = `dispose?.Invoke();`, `:454` = `Dispose(bool)`
close. **No re-anchoring is required.** The #424 report and the current working tree agree on this
file line-for-line.

---

## 3. Complete branch-point census and gap inventory

### 3.1 Census — all 48 branching lines

**Fully covered (44 lines).** In `BreadcrumbMessengerHub`: `:66`, `:74`, `:100`, `:107`, `:121`,
`:131`, `:143`, `:149`, `:162` (6/6), `:172`, `:177` (4/4), `:185` (6/6), `:195`, `:216`, `:223`,
`:224`, `:225`, `:226`, `:231`, `:240`, `:246`, `:247`, `:248`, `:255`. In
`BreadcrumbCollapsedAttachment`: `:293`, `:294`, `:301`, `:305`, `:308` (4/4), `:320`, `:321`,
`:322` (4/4), `:340`, `:341`, `:342`, `:369` (6/6), `:382`, `:389`, `:402`, `:412`, `:414`,
`:421` (4/4), `:427`. In `BreadcrumbResourceOwner`: `:447`.

**Partial (4 lines, 4 untaken outcomes).** `:326` (3/4), `:329` (1/2), `:442` (1/2), `:451` (1/2).

### 3.2 Which side is untaken — determined from evidence, not assumed

| Line | Construct | Condition index | Untaken side | Evidence |
| --- | --- | --- | --- | --- |
| `:326` | `if (_disposed \|\| generation != _generation)` | **0** (`_disposed`) at 50%; condition 1 at 100% | the **true** side of `_disposed` | `ThrowIfDisposed()` at `:304` runs first, so `_disposed` can only become true *during* the factory call at `:319`. No test disposes from inside its factory. The block body `:327-331` reports `hits="1"`, so the `if` was entered — necessarily via condition 1, which is at 100%. |
| `:329` | `(messenger as IDisposable)?.Dispose();` | 0 | the **null** side (`messenger` is not `IDisposable`) | The only path into `:327-331` is exercised by `Attachment_StaleFactoryCandidateAndReadyReset_CleanExactlyOnce` (`BreadcrumbMessengerHubCoverageTests.cs:283-292`), which supplies a `TrackingMessenger`; that double **does** implement `IDisposable` (`:421`) and the test asserts `staleSurface.DisposeCount.Should().Be(1)` (`:292`). Contrast the structurally identical `:341` in the `catch` arm, which reads **2/2** precisely because `attachment.AttachAsync(() => null)` (`:182`) drives a null messenger through it. |
| `:442` | `_dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));` | 0 | the **throw** side | The only construction site is `ItemViewer.Breadcrumb.cs:287`, which passes the non-null method group `DisposeBreadcrumbResources`. No test constructs `BreadcrumbResourceOwner` at all (grep across `QuickFiler.Test/`: zero references). |
| `:451` | `dispose?.Invoke();` | 0 | the **null** side (a second disposal) | `:442`'s throw arm is untaken, so `_dispose` is always non-null after construction; `:447` is 2/2 so `Dispose(true)` ran at least once; therefore the non-null arm at `:451` is the observed one and the null arm — reachable only by a *second* `Dispose(true)` after `:450` nulls the field — is the untaken one. |

### 3.3 Gap inventory grouped into atomic test tasks

Four gaps, four atomic tasks. Every one is fully reachable with **no production change**.

---

#### G1 — `AttachAsync` candidate arriving after the attachment was disposed *by its own factory* (`:326` condition 0)

**Construct.** `BreadcrumbMessengerHub.cs:326`, the `_disposed` half of the stale-candidate guard in
`BreadcrumbCollapsedAttachment.AttachAsync`.

**Why untaken today, and which test almost reaches it.**
`Attachment_StaleFactoryCandidateAndReadyReset_CleanExactlyOnce`
(`BreadcrumbMessengerHubCoverageTests.cs:269-300`) is one step away: its factory calls
`attachment.Reset()` (`:285`) before returning a candidate, which reaches `:326` — but `Reset()` is
`Release(dispose: false)` (`:349`), so it bumps `_generation` (`:405`) and leaves `_disposed` false.
It therefore closes condition 1 and never touches condition 0.
`Attachment_PendingDisposeIsIdempotentAndBlocksLaterAttach` (`:334-364`) does dispose, but *after*
`AttachAsync` has returned, so its candidate passes `:326` cleanly and is rejected later by
`IsCurrent` inside `CompleteAsync` (`:389`).

**Reachability verdict: fully reachable via an `internal` member**, which `QuickFiler.Test` already
consumes (`AssemblyInfo.cs:5`). No reflection, no production change.

**Arrange.** `var hub = new BreadcrumbMessengerHub(); var controller = new BreadcrumbCollapsedSurfaceController();
var attachment = new BreadcrumbCollapsedAttachment(hub, controller);` plus a `TrackingMessenger`
(local double, `IDisposable`) and a `Readiness(...)`-style `BreadcrumbNavigationReadiness` built with
the existing helper shape at `BreadcrumbMessengerHubCoverageTests.cs:390-406`.

**Act.** `bool attached = await attachment.AttachAsync(() => { attachment.Dispose(); return Candidate(surface, readiness); });`

**Assert.** `attached.Should().BeFalse();` — and, decisively, that the *candidate was disposed rather
than leaked*: `surface.DisposeCount.Should().Be(1)`, the readiness lease's detach callback fired
exactly once, `surface.SubscribeAttempts.Should().Be(0)` (the hub was never asked to attach), and a
following `attachment.AttachAsync(...)` throws `ObjectDisposedException` (`:304` -> `:428`).

**Behavioral contract pinned.** *A candidate produced by a factory that disposes the attachment
mid-flight is released, never attached, and never leaked.* This is a real transactional invariant —
the disposal race is exactly what `:326` exists to defend — not a shape assertion.

---

#### G2 — stale candidate whose messenger does not implement `IDisposable` (`:329`)

**Construct.** `BreadcrumbMessengerHub.cs:329`, the null-conditional in the stale-candidate release
path.

**Why untaken today.** Every `IWebViewMessenger` double in the suite implements `IDisposable`:
`BreadcrumbMessengerHubTests.TrackingMessenger` (`:364`) and
`BreadcrumbMessengerHubCoverageTests.TrackingMessenger` (`:421`). The stale path has therefore only
ever seen a disposable messenger. The interface itself does **not** require `IDisposable`
(`IWebViewMessenger.cs:13-26`, two members: `MessageReceived`, `PostJson`), so a non-disposable
implementation is a legitimate, unexercised contract case.

**Reachability verdict: fully reachable, `internal` member, no production change.**

**Arrange.** A `private sealed class NonDisposableMessenger : IWebViewMessenger` local to the new
test class — deliberately hand-written rather than `Mock<IWebViewMessenger>` because the assertion
turns on the *runtime interface set* of the object, and a hand-written double states that intent
unambiguously where a mocking-library proxy's interface set is an implementation detail. (Moq
remains the default elsewhere; this is a documented, narrow exception.)

**Act.** Reuse the proven stale-generation trigger: a factory that calls `attachment.Reset()` and
then returns `Tuple.Create<IWebViewMessenger, BreadcrumbNavigationReadiness>(nonDisposable, readiness)`.

**Assert.** The task completes `false`; no exception is thrown; the **readiness lease was still
disposed** (`:328`) even though the messenger could not be; and a subsequent `AttachAsync` with a
disposable candidate still succeeds.

**Behavioral contract pinned.** *Releasing a stale candidate is best-effort per resource: a messenger
that does not own disposable resources is skipped without aborting the release of the readiness
lease.* Note this is the exact contract already pinned at `:341` for the exception arm; G2 extends it
to the stale arm, restoring symmetry.

> **Planner note.** G1 and G2 could be merged into a single test (a factory that both disposes the
> attachment and returns a non-disposable messenger closes both outcomes). **Do not merge them.**
> They pin two independent contracts, and a merged test would report one failure for two distinct
> regressions.

---

#### G3 — `BreadcrumbResourceOwner` rejects a null disposal callback (`:442`)

**Construct.** `BreadcrumbMessengerHub.cs:442`, `_dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));`

**Why untaken today.** No test constructs `BreadcrumbResourceOwner`. The type's 13 covered lines are
covered *incidentally*, through `ItemViewer.Breadcrumb.cs:287`, by test files that construct a live
`ItemViewer` — see §4.1. There is no analogue of
`Attachment_ConstructorFactoryAndCandidateGuards_AllowRetry`'s explicit null-guard assertions
(`BreadcrumbMessengerHubCoverageTests.cs:153-154`) for this type.

**Reachability verdict: fully reachable via an `internal` constructor.** `Component` is non-visual,
allocates no window handle, and needs no STA thread — construction in a plain `[TestClass]` is safe.

**Arrange / Act.** `Action construct = () => new BreadcrumbResourceOwner(null);`

**Assert.** `construct.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("dispose");`

**Behavioral contract pinned.** *A resource owner cannot be registered into a `Container` in a state
where its later disposal would silently do nothing.* Asserting `ParamName` (matching the established
`AssertParameter` helper at `BreadcrumbMessengerHubCoverageTests.cs:411-412`) is what makes this a
contract test.

---

#### G4 — `BreadcrumbResourceOwner` disposal is idempotent and runs the callback exactly once (`:451`)

**Construct.** `BreadcrumbMessengerHub.cs:449-451`, the read-null-invoke sequence in
`Dispose(bool disposing)`.

**Why untaken today.** In production the owner is added to `ItemViewer.components`
(`ItemViewer.Breadcrumb.cs:288`) and disposed exactly once when the form's container is disposed.
`Control`'s own disposal guard means no second `Dispose(true)` ever reaches it, so the null arm is
never observed.

**Reachability verdict: fully reachable, `internal` constructor plus the public `IDisposable.Dispose`
inherited from `Component`. No production change.**

**Arrange.** `int calls = 0; var owner = new BreadcrumbResourceOwner(() => calls++);`

**Act.** `owner.Dispose(); owner.Dispose();`

**Assert.** `calls.Should().Be(1);`

**Behavioral contract pinned.** *Double disposal must not re-run the breadcrumb teardown.* This
matters concretely: the callback in production is
`ItemViewer.DisposeBreadcrumbResources` (`ItemViewer.Breadcrumb.cs:291-296`), which disposes the
lifecycle coordinator and nulls `BreadcrumbCoordinator`; running it twice would call
`BreadcrumbItemViewerLifecycleCoordinator.Dispose` twice, which in turn calls `_hub.Dispose()`
(`BreadcrumbItemViewerLifecycleCoordinator.cs:215`). The idempotence is load-bearing, not cosmetic.

---

#### G5 — deterministic finalizer-path contract (`:447`, currently covered but **not deterministically**)

This gap closes **no** untaken outcome today. It is included because §4.3 shows the outcome that is
currently taken is taken by accident.

**Construct.** `BreadcrumbMessengerHub.cs:447`, `if (disposing)`, currently reading 2/2.

**Why the `false` arm is currently covered.** `Dispose(bool)` is `protected override`; the only
callers are `Component.Dispose()` (which passes `true` and calls `GC.SuppressFinalize`) and
`Component`'s finalizer (which passes `false`). No test invokes it. Several test files construct an
`ItemViewer`, drive `InitializeBreadcrumbPipeline`, and do **not** dispose the viewer — for example
`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs:236` and `:327`, and
`QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs:386`. Those owners are reclaimed
by the GC and finalized, which is the only mechanism that can reach `Dispose(false)`.
**That makes the `false` arm GC-timing dependent and therefore not reproducible on demand.** This is
an inference from the call-graph, clearly labelled as such — but it is the only mechanism the
language permits, so the confidence is high.

**Reachability verdict: reachable deterministically only via reflection** on the `protected` method.
That is policy-compliant (no external dependency, no filesystem, no timing) and there is in-repo
precedent for reflection into private members of this cluster —
`QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs:381-392` resolves a private method on
`BreadcrumbBridgeCoordinator` the same way.

**Arrange / Act.** Resolve `Dispose(bool)` via
`typeof(BreadcrumbResourceOwner).GetMethod("Dispose", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(bool) }, null)`
and invoke with `false` on a freshly constructed owner; then call the public `Dispose()`.

**Assert.** The callback did **not** run for the `false` invocation, and **did** run exactly once for
the subsequent public `Dispose()`.

**Behavioral contract pinned.** *A finalizer must not invoke the managed teardown callback* — running
`DisposeBreadcrumbResources` from a finalizer thread would touch `ItemViewer` state and the WebView2
lifecycle off the UI thread. This is the correct .NET dispose pattern and is worth a real test on its
own merits, independent of coverage.

### 3.4 Projected result

| Axis | Before | After G1-G4 | After G1-G5 | Floor |
| --- | --- | --- | --- | --- |
| Line | 294/294 = 100.00% | 294/294 = 100.00% | 294/294 = 100.00% | >= 80% |
| Branch | 114/118 = 96.61% | **118/118 = 100.00%** | 118/118 = 100.00%, and **no outcome depends on GC timing** | >= 75% |

**No branch outcome in this file is unreachable.** Zero documented deviations are required for
reachability, and no exemption argument is needed. The only judgement call is G5, which uses
reflection against an existing in-repo precedent.

---

## 4. Retain-or-Improve Risk Analysis — the centrepiece

This file is at 100% line coverage: every one of its 294 coverable lines is load-bearing on some
existing test, so the only available movement is downward. Four risks, in descending severity.

### 4.1 R1 (highest) — 13 lines of this file are covered only as a by-product of live `ItemViewer` construction in F13/F14 test files

`BreadcrumbResourceOwner` (`:436-455`, **13 coverable lines, 6 branch outcomes**) has **zero direct
test references anywhere in `QuickFiler.Test/`**. Verified by repository-wide grep: the only
occurrences of the identifier are the declaration (`:436`, `:440`) and the F14-owned production
consumer `ItemViewer.Breadcrumb.cs:16`, `:262`, `:279`, `:287`.

Its coverage arrives through this chain:

```
ItemViewer.InitializeBreadcrumbPipeline(...)                     [F14-owned]
  -> ItemViewer.Breadcrumb.cs:262  EnsureBreadcrumbResourceOwnership()
  -> ItemViewer.Breadcrumb.cs:287  new BreadcrumbResourceOwner(DisposeBreadcrumbResources)
  -> ItemViewer.Breadcrumb.cs:288  components.Add(...)
  ... form/container disposal -> Component.Dispose() -> BreadcrumbMessengerHub.cs:445 Dispose(true)
```

Test files that construct a real `ItemViewer` and call `InitializeBreadcrumbPipeline`, all of which
sit in **F13** or **F14** territory rather than F12's:

| Test file | Anchor | Nominal owner |
| --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | `:338`, `:340` | F13 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | `:255`, `:260` | F13 |
| `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs` | `:163`, `:363` | F13 |
| `QuickFiler.Test/Viewers/BreadcrumbSubfolderActivationTests.cs` | `:305`, `:306` | F13 |
| `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | `:413`, `:415` | F13 |
| `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs` | `:281-284`, `:373` | F10/F13 |
| `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs` | `:122`, `:477` | F12 (only F12-owned member of this list) |

**Impact if `ItemViewer`-constructing tests are retired or headless-ified:** 13 lines fall to zero
hits and 4 branch outcomes are lost.

| Scenario | Line | Branch |
| --- | --- | --- |
| Today | 294/294 = **100.00%** | 114/118 = **96.61%** |
| `BreadcrumbResourceOwner` loses all coverage | 281/294 = **95.58%** | 110/118 = **93.22%** |

Both remain above the floors, but both are clear regressions against the retain-or-improve bar and
issue #136 AC8. Three named sibling plans could trigger it: **F14 (#456)**, whose brief is precisely
about `ItemViewer` and its partials; **F13 (#455)**, which owns five of the seven harnesses above;
and open bug **#491 `quickfiler-test-form1-live-form`**, which is about removing a live form from
this very test project.

**Mitigation — write both into F12's plan as acceptance criteria:**

1. **AC:** F12 adds direct tests for `BreadcrumbResourceOwner` (G3, G4, G5). After these land, the
   type's coverage no longer depends on any `ItemViewer`-constructing test, and R1 is *eliminated*
   rather than merely monitored. This is the decisive mitigation and is the strongest single
   argument for including G5.
2. **AC:** re-measure this file after the epic's integration fan-in and record the post-merge
   per-file figure as evidence under `<FEATURE>/evidence/qa-gates/`, so a regression introduced by a
   sibling's merge is caught at F16 rather than in production.

### 4.2 R2 — the fixture surface is 9 named test files plus ~7 indirect ones, and the two dominant files are nearly full

`BreadcrumbMessengerHub` is referenced by name in **9 test files (33 occurrences)**, and reached
indirectly by roughly 7 more through `ItemViewer`. Full inventory, from a repository-wide grep:

| Test file | Occurrences | Constructs a real hub? |
| --- | --- | --- |
| `Viewers/BreadcrumbMessengerHubCoverageTests.cs` | 11 | yes, `:19`, `:46`, `:73`, `:100`, `:129`, `:151`, `:205`, `:271`, `:305`, `:336` |
| `Viewers/BreadcrumbMessengerHubTests.cs` | 10 | yes, `:108`, `:141`, `:166`, `:202`, `:223`, `:270`, `:295`, `:313`; plus a reflective `Activator.CreateInstance` at `:334-342` |
| `Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs` | 3 | yes, `:146` |
| `Viewers/BreadcrumbSelectorCoordinatorTests.cs` | 2 | yes, `:198`, `:295` |
| `Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 2 | yes, `:228` |
| `Viewers/BreadcrumbDropDownReadinessTests.cs` | 2 | yes, `:308` |
| `Viewers/FolderBreadcrumbAssetContractTests.cs` | 1 | yes, `:178` |
| `Viewers/BreadcrumbDropDownLifecycleConcurrencyTests.cs` | 1 | yes, `:227` |
| `Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | 1 | yes, `:432` (harness property) |

The 170 lines of the hub type proper are broadly and redundantly covered — this half of the file is
robust. **The concentration risk is on `BreadcrumbCollapsedAttachment` (111 lines, 37.8% of the
file), which is exercised by exactly two files**: `BreadcrumbMessengerHubCoverageTests.cs`
(5 of its 10 test methods) and `BreadcrumbMessengerHubTests.cs` (1 test method, `:219-264`).

| Scenario | Line | Branch |
| --- | --- | --- |
| `BreadcrumbCollapsedAttachment` loses all coverage | 183/294 = **62.24%** — **fails the 80% floor** | 62/118 = **52.54%** — **fails the 75% floor** |

Neither of those two files is on any sibling's editing path (both are hub-named), so the residual
probability is low — but the *consequence* is a two-floor failure, which is why it is recorded.

**Mitigation:** an explicit F12 acceptance criterion that `BreadcrumbMessengerHubTests.cs` and
`BreadcrumbMessengerHubCoverageTests.cs` retain their `BreadcrumbCollapsedAttachment` coverage, and
that any refactor of those files is accompanied by a per-file re-measure.

### 4.3 R3 — one currently-covered branch outcome depends on garbage-collection timing

As established in G5, `BreadcrumbMessengerHub.cs:447`'s `disposing == false` arm can only be reached
by `Component`'s finalizer. Nothing in the suite invokes it deliberately. If a future run's GC
schedule differs — a plausible consequence of any sibling child adding, removing, or reordering
tests — that outcome disappears:

| Scenario | Branch |
| --- | --- |
| Today | 114/118 = **96.61%** |
| GC does not finalize an undisposed `BreadcrumbResourceOwner` during the run | 113/118 = **95.76%** |

This would present as an unexplained regression with no diff to account for it. **Mitigation: G5**,
which converts the outcome from incidental to asserted. This is the reason G5 is recommended even
though it closes no gap in the current numbers.

### 4.4 R4 — `BreadcrumbCollapsedAttachment` compiles against two F13-owned `internal` types

`BreadcrumbCollapsedSurfaceController` (`BreadcrumbCollapsedSurfaceController.cs:11`) and
`BreadcrumbNavigationReadiness` (`BreadcrumbWebViewSurfaceFactory.cs:19`) are both `internal sealed`
and both appear on **F13's** file list in the epic manifest (`epic.md` § F13: `BreadcrumbCollapsedSurfaceController.cs`
(308), `BreadcrumbWebViewSurfaceFactory.cs` (225)). This file consumes:

- `controller.AttachAsync(messenger, readiness)` — `:368`
- `controller.ReadyMessenger` — `:372`
- `controller.Reset()` / `controller.Dispose()` — `:383`, `:415`, `:417`
- `readiness.Dispose()` — `:328`, `:340`
- the `BreadcrumbNavigationReadiness` constructor and `BeginNavigation` / `NavigationStarted` /
  `NavigationCompleted` / `Completion` surface, used by every F12 test that drives an attachment
  (`BreadcrumbMessengerHubCoverageTests.cs:390-406`)

Any F13 signature change to these breaks F12 at **compile time**, which is loud rather than silent —
a favourable failure mode. F13's spec commits to **no public or internal signature changes** to its
15 files (`.../455/spec.md:49-50`); **F12's plan should cite that commitment explicitly** as its
protection, exactly as the sibling coordinator artifact does for `BreadcrumbUiDispatcher`.

Being `sealed` and `internal`, neither type can be mocked; F12's tests use the real types. That is
acceptable — both are deterministic, allocate no I/O, and carry their own F13 coverage — but it means
F12's attachment tests are integration tests across a child boundary, and should be labelled as such
in the plan.

### 4.5 R5 — the hub's `internal` types are invisible to a `<class name>`-keyed harness

Recorded as a harness risk rather than a coverage risk. Because this report emits a single `<class>`
element named `QuickFiler.Viewers.BreadcrumbMessengerHub` for a file containing three types, an F1
harness keyed on `<class name>` would silently omit `BreadcrumbCollapsedAttachment` and
`BreadcrumbResourceOwner` (they have no `<class>` element of their own, verified by grep). Keying on
`filename` — as the epic's harness directive requires — produces the correct 294/118 figures. **This
file is a useful positive control for that directive** and F12's evidence should say which key its
run used.

---

## 5. Production Edit Verdict

**No production edit to `BreadcrumbMessengerHub.cs` is required or recommended.**

All four untaken branch outcomes are reachable from `QuickFiler.Test` using seams that already exist:
the `InternalsVisibleTo` grant at `AssemblyInfo.cs:5`, the `internal` constructors at `:288` and
`:440`, the public `IWebViewMessenger` interface, and one reflection call against an established
in-repo precedent (G5).

Consequences:

- **The 44-line headroom (456/500) is not consumed.** No new seam, no new adapter type, no new
  member, no new file.
- **No `QuickFiler/QuickFiler.csproj` edit**, therefore **no "Mid-Wave File Creation" ledger row**
  and no `>= 90%` new-file obligation.
- **The #457 measurement trap does not apply.** No `[ExcludeFromCodeCoverage]` is introduced at
  either level, so there is no lifted-lambda leak to reason about. Recorded for completeness: had a
  thin-forwarder adapter been required, it would have to be a `sealed`, **non-`partial`** type with a
  **type-level** attribute (epic § "Measurement Trap", § "fourth exemption ground" condition 4).
- **No behavior change**, satisfying the epic NFR without a deviation.

**Rejected alternatives, for the record.**

1. *Split the file into three.* Would improve module cohesion (§1.1) but requires two
   `QuickFiler.csproj` entries, two ledger rows, a fresh `>= 90%` obligation per new file, and a
   changed denominator mid-epic — for zero coverage gain, since all three types are already at or
   near 100%. Rejected.
2. *Make `BreadcrumbCollapsedAttachment` take an interface instead of the concrete F13-owned
   `BreadcrumbCollapsedSurfaceController`.* Would decouple R4, but is a production API change with no
   coverage benefit (the concrete type is already fully drivable from tests) and would consume
   headroom. Rejected under the no-behavior-change NFR.
3. *Copy `_attachments.Values` to an array before the broadcast at `:131` to harden against
   re-entrancy.* This is the fix for LD-1/LD-2 and is a genuine improvement, but it changes
   concurrency semantics and belongs in a promoted issue, not in a coverage child. Rejected here;
   see §7.

---

## 6. Test-File Plan

### 6.1 Headroom against the 500-line test-file limit

| File | Lines | `[TestMethod]` | Headroom |
| --- | --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs` | 478 | 10 | **22** |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` | 414 | 12 | 86 |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 327 | 10 | 173 |
| `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` | 122 | 4 | 378 |

All counts verified directly. Neither existing hub test file can absorb five new tests: the coverage
file has 22 lines of headroom, and the 86 lines in `BreadcrumbMessengerHubTests.cs` would be consumed
almost exactly by the new tests plus their doubles, leaving no margin for a later child.

### 6.2 Recommendation — one new standalone `[TestClass]`, not a `.Part2.cs` companion

**Create `QuickFiler.Test/Viewers/BreadcrumbHubDisposalContractTests.cs`.**

| Task | Test method | Closes |
| --- | --- | --- |
| T1 | `AttachAsync_FactoryDisposesAttachment_ReleasesCandidateWithoutAttaching` | `:326` cond 0 |
| T2 | `AttachAsync_StaleCandidateWithNonDisposableMessenger_StillReleasesReadinessLease` | `:329` |
| T3 | `ResourceOwner_NullDisposalCallback_ThrowsForTheExpectedParameter` | `:442` |
| T4 | `ResourceOwner_DoubleDispose_RunsTheCallbackExactlyOnce` | `:451` |
| T5 | `ResourceOwner_FinalizerPath_DoesNotRunTheManagedCallback` | hardens `:447` (R3) |

Five `[TestMethod]` declarations. Estimated 150-190 lines including a `NonDisposableMessenger`
double, a minimal `TrackingMessenger`, and a local `Readiness(...)` helper mirroring
`BreadcrumbMessengerHubCoverageTests.cs:390-406` — comfortably inside 500.

Why a standalone class rather than a partial companion:

1. `BreadcrumbMessengerHubCoverageTests` is declared `public sealed class` at
   `BreadcrumbMessengerHubCoverageTests.cs:14` and `BreadcrumbMessengerHubTests` likewise at
   `:14` — **neither is `partial`**. A `.Part2.cs` would require editing a 478-line or 414-line file's
   class declaration, adding fan-in conflict surface on files F13-adjacent tests also read.
2. The five tests form one cohesive theme — *disposal and release contracts* — that neither existing
   file is named for.
3. T3/T4/T5 need no hub, no controller, and no readiness at all; hosting them beside the heavy
   attachment fixtures would be gratuitous coupling.

The repository does have `.Part2.cs` precedent (`QuickFiler.Test/QuickFiler.Test.csproj:82` and
`:85`), so the pattern is available if a reviewer prefers it. It is simply not needed here.

### 6.3 csproj registration

`QuickFiler.Test/QuickFiler.Test.csproj` is a non-SDK project with 107 explicit `<Compile Include>`
entries and no globbing. Add exactly one line:

```
    <Compile Include="Viewers\BreadcrumbHubDisposalContractTests.cs" />
```

**Insert immediately after line 87** (`<Compile Include="Viewers\BreadcrumbMessengerHubCoverageTests.cs" />`),
keeping the edit to a single adjacent hunk in the breadcrumb block. **Preserve CRLF** — use the Edit
tool, never a git-bash `sed -i` (epic § "Cross-Child Constraints" 1b). Indentation is four spaces, as
on the surrounding lines. No `QuickFiler/QuickFiler.csproj` edit is required by this file.

### 6.4 Projected post-change figures

| Axis | Before | After | Floor | Verdict |
| --- | --- | --- | --- | --- |
| Line | 294/294 = 100.00% | 294/294 = **100.00%** | >= 80% | retained |
| Branch | 114/118 = 96.61% | 118/118 = **100.00%** | >= 75% | **improved +3.39 pp** |

Plus the non-numeric improvement that matters most: after T3-T5, **no line in this file depends on a
sibling child's test fixture or on GC timing.**

---

## 7. Determinism contract for every new test

**Framework and style.** MSTest `[TestClass]` / `[TestMethod]`, Moq where interaction verification is
wanted, FluentAssertions for all assertions, explicit Arrange / Act / Assert section comments —
matching `BreadcrumbMessengerHubCoverageTests.cs` and `BreadcrumbMessengerHubTests.cs`.

**Concrete deterministic vehicles already present and green in `QuickFiler.Test/`:**

1. **Synchronous in-memory `IWebViewMessenger` doubles.**
   `BreadcrumbMessengerHubCoverageTests.TrackingMessenger` (`:421-476`) and
   `BreadcrumbMessengerHubTests.TrackingMessenger` (`:364-412`) — explicit `add`/`remove` accessors
   with subscribe/unsubscribe counters, `ThrowOnPost` / `ThrowOnSubscribe` / `ThrowOnUnsubscribe` /
   `ThrowOnDispose` switches, and `ReceiveFrom(object sender, string json)` for driving inbound
   messages from an arbitrary sender. No threading, no scheduling.
2. **`TaskCompletionSource`-driven readiness.** The `Readiness(ulong, bool?, Action)` helper at
   `BreadcrumbMessengerHubCoverageTests.cs:390-406` builds a real `BreadcrumbNavigationReadiness` and
   completes it synchronously; the asynchronous edge is then resolved by awaiting the `Task<bool>`
   returned by `AttachAsync`, which is a `RunContinuationsAsynchronously` completion source
   (`BreadcrumbMessengerHub.cs:431-432`). This is the sanctioned pattern for every `async Task` test
   in the two hub files.
3. **`AssertFaultAsync<T>` / `AssertThrows<T>` / `AssertParameter`**
   (`BreadcrumbMessengerHubCoverageTests.cs:408-419`) — FluentAssertions wrappers that avoid any
   timeout or polling.
4. **Reflection against a private/protected member**, precedent at
   `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs:381-392` — for T5 only.

**No `SynchronizationContext` is required or should be installed** (see §1.3). No
`BreadcrumbUiDispatcher` is required: this file never touches one.

**Prohibited and must be absent from every new test:**

- `Thread.Sleep`, `Task.Delay`, any wall-clock wait, any real-time polling, any timeout-based await.
- Injected clocks, `TimeProvider`, `FakeTimeProvider`, fake timers — there is no time dependency to
  control (§1.3).
- Temporary files or any filesystem write (`CLAUDE.md` §UT4; approved exceptions: none).
- External services, external processes, network access, the WebView2 Evergreen runtime.
- Live or shown forms, `.Show()`, `.ShowDialog()`, popups, message-pump entry.
- STA attributes or STA threads — no type in this file needs one (§1.1).
- Mutable static or ambient state; every test constructs its own hub, controller, and attachment.

---

## 8. Latent Defects — verified, assessed, NOT fixed

Cross-checked against every currently open issue. The open-issue list was retrieved directly and
contains #495, #491, #488, #476, #475, #462, #458, #456, #455, #440, #438, #431; a keyword search for
`messenger OR hub OR lock OR reentrancy` returned no open issue covering any defect below. **None of
the three is a duplicate.** Per the epic's "Latent Defect Promotion" section the orchestrator
promotes these through the MCP lifecycle; this artifact does not.

### 8.1 LD-1 — outward SDK call under two nested locks (concur with, and refine, the sibling's LD-1)

**Severity: Low-Medium. Recommend promoting to a GitHub issue.**

The sibling `BreadcrumbBridgeCoordinator` artifact recorded a nested two-lock acquisition
`lifetime._sync -> hub._sync` with `PostToSurface` called while the hub lock is held, plus an STA
re-entrancy concern. **I concur, and can refine it in three ways from this file's side:**

1. **The chain is confirmed and is one hop longer than stated.** Verified end to end:
   `BreadcrumbCoordinatorUpgradeLifetime.cs:139` `lock (_sync)` -> `:145` `action()` ->
   `BreadcrumbMessengerHub.cs:126` `lock (_sync)` -> `:133` `PostToSurface` -> `:206`
   `attachment.Messenger.PostJson(json)` -> `WebView2Messenger.cs:55` -> `:62`
   `_dispatcher.Dispatch(...)` (**inline** when already on the boundary) -> `:66`
   `_coreWebView.PostWebMessageAsJson(json)`. The WebView2 SDK call is therefore made with **two**
   monitors held, not one.
2. **The inversion pair is real but cannot deadlock.** The reverse order (hub -> lifetime) exists
   only through synchronous re-entrancy from within the broadcast (§1.3, "order B"). Because that
   requires the same thread, and `Monitor` is re-entrant, **no deadlock is demonstrable**. The
   sibling's conclusion that the inbound path does not invert is correct for the *non-re-entrant*
   case: `OnSurfaceMessageReceived` releases at `:171` and invokes at `:172`.
3. **The hub's own lock has the same atomicity weakness the sibling identified in the lifetime.**
   `PostJson` holds `_sync` across `CacheState` (`:130`) and the full broadcast loop (`:131-134`). An
   STA COM call from `PostWebMessageAsJson` that pumps messages can re-enter managed code on the same
   thread; `lock` is re-entrant, so a re-entrant `Attach` (`:71`) or `Detach` (`:105`) would acquire
   `_sync` successfully and mutate `_attachments` **while the `foreach` at `:131` is enumerating it**,
   producing `InvalidOperationException: Collection was modified`.

   The re-entrant path is multi-hop and asynchronous in the current wiring — inbound `selectorToggle`
   -> `BreadcrumbItemViewerLifecycleCoordinator.OnSelectorOpenStateChanged` (`:221-222`) -> drop-down
   open -> `OnPopupMessengerReady` (`:224-240`) -> `_operations.PostAsync(...)` ->
   `AttachPopupMessenger` -> `_hub.Attach` (`:254`/`:264`) — so **I have not demonstrated it end to
   end**, and the severity assessment reflects that. Note that `ReplayCachedState`'s enumeration
   (`:177`) is *not* exposed to the same hazard, because `Enumerable.OrderBy` buffers its source
   before yielding.

**Why out of scope.** The fix — snapshot `_attachments.Values` into an array before the loop, and/or
move the outward `PostJson` outside the lock with a currency re-check — changes concurrency semantics
and observable ordering, which the epic's no-behavior-change NFR forbids in a coverage child.

### 8.2 LD-2 — a throwing surface aborts the broadcast mid-way, yet the message stays cached as delivered

**Severity: Low-Medium. Recommend promoting to a GitHub issue.**

Verified call chain:

1. `BreadcrumbMessengerHub.cs:130` — `CacheState(type, json)` records the message in `_cachedStates`
   **before** any surface has received it.
2. `:131-134` — `foreach (Attachment attachment in _attachments.Values) PostToSurface(attachment, json, type);`
   with **no `try`/`catch`** anywhere in `PostJson`. Contrast `Attach`, which does wrap its replay in
   `try`/`catch` with an explicit rollback (`:82-93`).
3. If the *first* attachment's `PostJson` throws, attachments 2..n never receive the message, the
   exception propagates out of `BreadcrumbMessengerHub.PostJson` to the caller, and there is no
   rollback of `_cachedStates`.

This is reachable in production: `WebView2Messenger.PostJson` throws `ObjectDisposedException`
(`WebView2Messenger.cs:61` -> `:130-136`) once its `Dispose` has been requested, and the hub is not
notified of a surface's disposal — `Detach` is a separate, independently ordered call
(`BreadcrumbItemViewerLifecycleCoordinator.cs:277`, `:288`). A disposed-but-still-attached popup
surface therefore silently starves every attachment later in dictionary enumeration order, while the
hub's replay cache records the message as if it had been delivered — so a subsequent `Attach` replays
a state the surviving surfaces never saw, and no re-delivery ever occurs.

No existing test covers this: the two `ThrowOnPost` tests
(`BreadcrumbMessengerHubTests.cs:199-217`, `BreadcrumbMessengerHubCoverageTests.cs:317-322`) both
throw during **`Attach`-time replay**, which *is* rolled back, never during a multi-surface broadcast.

**Why out of scope.** Any fix — catching per-surface, deferring the cache write until after a
successful broadcast, or auto-detaching a failed surface — is an observable behavior change.

**Related but distinct from #476** (`webview2breadcrumbhost-unmarshalled-sdk-call-and-unsynchronized-state`),
which concerns `WebView2BreadcrumbHost`, not the hub's broadcast contract. Recommend cross-linking
rather than merging.

### 8.3 LD-3 — `MessageType` is a naive string scan and can mis-classify a message

**Severity: Low (robustness). Recommend recording; promotion optional.**

`MessageType` (`:236-251`) locates the first literal occurrence of `"type"` in the raw JSON and reads
the next quoted token. It does not parse. Two consequences, both verified by reading:

- A `"type"` marker appearing **inside a string value** earlier in the document is matched first, so
  `{"label":"\"type\":\"selectorView\"","type":"render"}` is classified as `selectorView`. It would
  then be cached under the `selectorView` key at `:190`, evicting the real selector state, and passed
  to `RewriteSelectorMode` at `:199`.
- `RewriteSelectorMode` casts unconditionally at `:214`
  (`(BreadcrumbSelectorViewMessage)BreadcrumbSelectorMessageSerializer.Parse(json)`) and the
  surrounding `catch` at `:201` catches only `FormatException`. If `Parse` returns a *different*
  selector message type for such a document, the resulting `InvalidCastException` escapes `PostJson`
  **while `_sync` is held**, aborting the broadcast (compounding LD-2).

The inputs required are contrived and no production producer emits them — every outbound message
originates from `BreadcrumbSelectorMessageSerializer` — so this is a robustness observation rather
than a live defect. It is recorded because the "preserve invalid outbound JSON verbatim" comment at
`:203` states an intent (never throw on malformed outbound JSON) that the `FormatException`-only
catch does not fully deliver. Existing tests confirm the *intended* behavior for the realistic
malformed cases (`BreadcrumbMessengerHubTests.cs:291-307`,
`BreadcrumbMessengerHubCoverageTests.cs:97-124`).

### 8.4 Assessed and explicitly NOT defects

- **`Detach` omits `ThrowIfDisposed()`** (`:98-116`) where `Attach` (`:73`) and `PostJson` (`:128`)
  include it. This asymmetry is deliberate and *tested*: `BreadcrumbMessengerHubCoverageTests.cs:36`
  asserts `hub.Detach(surface).Should().BeFalse()` after disposal. Detach-after-dispose is a
  documented no-op, not an oversight.
- **`GC.SuppressFinalize(this)` at `:154` is skipped on a second `Dispose()`** because the early
  return at `:145` exits from inside the lock. Harmless: the hub declares no finalizer.
- **`_attachments.Add` at `:81` precedes the subscribe/replay at `:84-85`**, so an inbound message
  arriving during replay is routed from a surface whose attach may still roll back. Behaviorally
  benign given the rollback at `:90-91`, and no observable contract depends on it.

---

## 9. Corrections to the Brief

### 9.1 Corrections — evidence disproves these

1. **The `Lines` column in `issue.md:21-27` and `spec.md:15-21` is *coverable* lines, not physical
   lines, and is not labelled as such.** For this file coverable = 294 while physical = **456**
   (the epic manifest's F12 section, `epic.md` § F12, has the physical figure right). The practical
   consequence is a planner trap: 500 − 294 suggests 206 lines of production headroom when the true
   figure is **44**. The same mislabelling affects all five rows — e.g. `BreadcrumbBridgeCoordinator.cs`
   is listed at 280 (coverable) against 487 physical.
2. **This is not a single-type file, and the brief nowhere says so.** It declares three top-level
   types (`:15`, `:277`, `:436`) plus two nested ones (`:17`, `:35`). **124 of the 294 coverable
   lines — 42.2% — belong to `BreadcrumbCollapsedAttachment` and `BreadcrumbResourceOwner`**, whose
   coverage comes from different tests with different sibling owners. Every risk in §4 follows from
   this fact, and a plan written against "the hub" alone would miss all of it.
3. **"Determinism. Use an injected clock and fake timers" (`issue.md:70`, `:95`; `spec.md:69-70`,
   `:112`) is REFUTED for this file** and must be struck and replaced with a completion-source /
   scheduler-control statement, recorded as a documented deviation. A grep for
   `DateTime|Stopwatch|Timer|Task.Delay|Thread.Sleep|TimeProvider` returns **zero** matches. This
   confirms and adopts F13's ruling at `.../455/spec.md:381-390` and matches the sibling coordinator
   artifact's identical finding.
4. **The seeded test condition "Cancellation and cancelled-token paths" (`issue.md:90`,
   `spec.md:107`) is inapplicable to this file.** There is no `CancellationToken` anywhere in it.
   Attachment invalidation is generation-based (`_generation`, `:285`, `:311`, `:405`, `:420-423`),
   not token-based.
5. **The `#457` trap guidance (`issue.md:74-77`, `spec.md:73-76`) does not apply to this file.** No
   `[ExcludeFromCodeCoverage]` exists here and none is introduced, so no thin-forwarder adapter and
   no attribute-placement decision arises. The guidance remains correct for the child in general.
6. **The characterisation "the work is branch-gap closure plus retain-or-improve on the other four"
   (`issue.md:39`, `spec.md:33`) understates this file's risk profile.** Only 4 of 118 outcomes are
   open here, so gap closure is trivial; the substantive work is *protecting* a 100%/96.61% position
   whose weakest 13 lines are held up entirely by F13/F14-owned test fixtures (§4.1) and one of whose
   currently-covered outcomes depends on GC timing (§4.3).
7. **Do not read the emitted rates: the `<class>` element's `branch-rate="0.977273"` is wrong** (it
   is 43/44 over the primary-method subtree; the true per-file figure is 114/118 = 0.96610), and it
   errs **optimistically** — the direction that falsely passes a gate. `line-rate="1"` is correct
   only by coincidence, because the file has no uncovered lines.

### 9.2 Confirmed — evidence supports these exactly

1. **294 coverable lines** — exact, recomputed from the class-level `<lines>` block alone.
2. **100.0% line coverage** — exact; **zero** `<line>` elements carry `hits="0"`.
3. **96.6% branch coverage** — exact; 114/118 = 96.6102%.
4. **456 physical lines** (`epic.md` § F12) — exact.
5. **No `[ExcludeFromCodeCoverage]` on any of the five F12 files** (`issue.md:38`) — confirmed for
   this file by full read.
6. **`InternalsVisibleTo("QuickFiler.Test")`** at `QuickFiler/Properties/AssemblyInfo.cs:5` — present,
   and already exercised against this file's `internal` types.
7. **Both csproj files are non-SDK with explicit `<Compile Include>` entries and no globbing**;
   `QuickFiler.Test.csproj` carries the breadcrumb block at lines 62-91.
8. **Sibling boundaries** (`issue.md:72-73`): `BreadcrumbCollapsedSurfaceController.cs` and
   `BreadcrumbWebViewSurfaceFactory.cs` are F13-owned; `ItemViewer.Breadcrumb.cs` is F14-owned. Both
   confirmed against the epic manifest, and both are load-bearing collaborators of this file (§1.2,
   §4.1, §4.4).
9. **Line-number drift: none.** All 294 source-line anchors in the #424 Cobertura block resolve
   exactly on the current working-tree file.
10. **`csharpier` 1.2.6 requires a subcommand** (`issue.md:81-82`) — consistent with the epic's
    verified toolchain note; unchanged by this research.
