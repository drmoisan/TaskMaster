# Research: `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` (F12 / issue #495)

- Timestamp: 2026-08-08T01-15
- Epic: `docs/features/epics/quickfiler-per-file-coverage/epic.md` (#136), child F12
- Child issue: #495
- Branch: `feature/quickfiler-breadcrumb-bridge-coverage` (based on `epic/quickfiler-per-file-coverage-integration`)
- Scope: ONE production file, per the #136 one-research-artifact-per-file mandate.
- Companion artifact: `2026-08-08T01-15-breadcrumb-coordinator-upgrade-lifetime.md`

---

## 1. Current State — verified

### 1.1 File shape

`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` is **487 physical lines** (last line `}` at
`BreadcrumbBridgeCoordinator.cs:487`). Against the 500-line ceiling in
`.claude/rules/general-code-change.md` § File Size Limit that is **13 lines of headroom**.

- Single type: `public sealed class BreadcrumbBridgeCoordinator : IDisposable`
  (`BreadcrumbBridgeCoordinator.cs:25`). **Not `partial`.**
- No `[ExcludeFromCodeCoverage]` anywhere in the file (verified by grep; the only matches for
  `ExcludeFromCodeCoverage`-adjacent wording are the doc-comment phrase "coverage-exempt viewer
  partial" at `:21`, which refers to F14's `ItemViewer` partial, not to this file).
- No `System.Windows.Forms`, no `Microsoft.Office.Interop.Outlook`, no WebView2 type reference.
  The only `WebView2` tokens in the file are in XML doc comments (`:22`, `:36`).
- Constructors: public 2-arg at `:39-43` delegating to `internal` 3-arg at `:45-49`.
  `QuickFiler/Properties/AssemblyInfo.cs:5` contains `[assembly: InternalsVisibleTo("QuickFiler.Test")]`,
  so the internal constructor is directly callable from `QuickFiler.Test`. It already is —
  `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorProbabilityTests.cs:147-151`.

### 1.2 Collaborators and their owners (a planner trap)

| Symbol | Declared at | Owner |
| --- | --- | --- |
| `IWebViewMessenger` | `QuickFiler/Viewers/IWebViewMessenger.cs:13` | **F13** |
| `BreadcrumbUiDispatcher` | `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:12` | **F13** |
| `BreadcrumbMessengerHub` | `QuickFiler/Viewers/BreadcrumbMessengerHub.cs:15` | F12 |
| `BreadcrumbCoordinatorUpgradeLifetime` | `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs:35` | F12 |
| `FolderBreadcrumbBridgeRouter` | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:10` | **neither — UtilitiesCS** |
| `IFolderHierarchyProvider` | `UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs:19` | UtilitiesCS |
| `BreadcrumbSelectorMessageSerializer` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs:142` | UtilitiesCS |
| `BreadcrumbBridgeSerializer` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs:185` | UtilitiesCS |

> **Correction / trap.** The type this file constructs at `:52` is
> `FolderBreadcrumbBridgeRouter`, which lives in **UtilitiesCS**. It is *not*
> `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:19` (`public sealed class BreadcrumbBridgeRouter`),
> which is a different type also assigned to F12. A planner that conflates the two will scope the
> wrong file.

### 1.3 Concurrency and determinism inventory

Verified by direct read of all 487 lines plus a targeted grep:

- `CancellationToken` parameter at `:91`; `CancellationToken.None` at `:362`.
- `async Task` methods at `:89`, `:328`, `:342`, `:358`; every `await` uses `.ConfigureAwait(false)`.
- Fire-and-forget discards at `:111`, `:141`, `:233`, `:242`.
- Mutable `Task` ordering seams: `LastDispatch` (`:74`, assigned `:325`) and `SuggestionsUpgrade`
  (`:118`, assigned `:112`).
- Exception funnel `ObserveInboundAsync` at `:328-340` routing to `_dispatcher.Report` at `:338`.
- **No `lock`, no `Interlocked`, no `volatile`, no timer in this file.**
- `System.Web.Script.Serialization.JavaScriptSerializer` at `:317`.

**Determinism finding — the brief's "injected clock and fake timers" instruction is REFUTED.**
A grep of this file for `DateTime|Stopwatch|Timer|Task.Delay|Thread.Sleep|TimeProvider` returns
**zero matches**. There is no time dependency of any kind to control. Determinism for this file is
**scheduler and `Task`/`TaskCompletionSource` control**, exactly as sibling F13 concluded and
formally ratified in
`docs/features/active/2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455/spec.md:381-390`
(§8.1: "Determinism here is **scheduler** control, not clock control. Any plan task that introduces
an injected clock or a fake-timer facility is out of scope and must be rejected — it would add a
seam with no dependency to control.").

`docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/spec.md:69-70` and `:112`
still carry the superseded "injected clock and fake timers" phrasing. **It must be struck and
replaced with the scheduler-control statement, recorded as a documented deviation.**

The deterministic vehicles that already exist and are green:

1. `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()`
   (`BreadcrumbUiDispatcher.cs:62-65`) — an owner-thread-only dispatcher that runs every
   `Dispatch(...)` **inline**, with no context and no pump. Used at
   `BreadcrumbBridgeCoordinatorProbabilityTests.cs:147-151`.
2. `BreadcrumbBridgeCoordinatorTests.InlineSynchronizationContext`
   (`BreadcrumbBridgeCoordinatorTests.cs:90-93`) — a `SynchronizationContext` whose `Post` invokes
   the callback synchronously, installed and restored in a `try/finally`
   (`BreadcrumbBridgeCoordinatorTests.cs:95-112`).
3. `BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext`
   (`BreadcrumbSelectorToggleUiBoundaryTests.cs:346-401`) — a manually-pumped queue exposing
   `WaitForPost()`, `DrainAll()`, `DrainUntil(Task)` and `ExceptionSnapshot`. Consumed cross-file by
   `BreadcrumbCoordinatorLifecycleTests.cs:334-347`.
4. Test-owned `TaskCompletionSource<FolderTreeNodeKey>` gates —
   `BreadcrumbCoordinatorLifecycleTests.cs:394-397`.

---

## 2. Measured Baseline — independently re-verified

Source: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`.

The Cobertura `<class>` element for this file is at XML line **6564** and closes at **7509**; the
class-level `<lines>` block runs XML lines **7071-7508**. Exactly **one** `<class>` element carries
`filename="QuickFiler\Viewers\BreadcrumbBridgeCoordinator.cs"`, so no cross-class union is needed
for this file (the report is already per-filename merged).

Recomputed from class-level `<line>` nodes only (never `.//lines/line`, never the `line-rate` /
`branch-rate` attributes — issue #441):

| Metric | Value |
| --- | --- |
| Coverable lines | 280 |
| Lines with `hits="0"` | **0** |
| Line coverage | **100.00%** |
| Branch points (sum of `condition-coverage` denominators) | 87 |
| Branch outcomes taken | 76 |
| Branch coverage | **87.36%** |

**The brief's table is confirmed exactly.** Floors are >= 80% line and >= 75% branch
(epic "Coverage-Target Reconciliation"), so **this file passes both gates today**; the bar is
**retain-or-improve on each axis**.

The `<class>` `branch-rate` attribute reads `0.875` — numerically close to the true 0.8736 by
coincidence, and still not the correct figure. Do not read it.

### 2.1 Branch-point census (all 38 branching lines)

Fully covered (100%): `:102`, `:152`, `:165`, `:207` (5/5 switch), `:238`, `:280`, `:285`, `:287`,
`:289`, `:291`, `:297`, `:310`, `:344`, `:349`, `:369`, `:372`, `:375`, `:379`, `:390`, `:400`,
`:435`, `:461`, `:463`, `:465`, `:467`, `:476`, `:480`.

Partial: `:51`, `:52`, `:55`, `:94`, `:133`, `:262` (3/4), `:382`, `:397` (7/8), `:441`, `:442`,
`:443`.

**Line-number drift: none.** Every reported gap line resolves to the construct the brief predicted
on the current working-tree file. No re-anchoring is required.

---

## 3. Branch-by-Branch Gap Inventory

Eleven untaken branch outcomes across eleven lines. Grouped into six atomic test tasks.

### G1 — internal-constructor null guards (`:51`, `:52`, `:55`) — 3 outcomes

| Line | Construct | Untaken side |
| --- | --- | --- |
| `:51` | `_messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));` | the `throw` |
| `:52-54` | `_router = new FolderBreadcrumbBridgeRouter(provider ?? throw new ArgumentNullException(nameof(provider)));` | the `throw` |
| `:55` | `_dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));` | the `throw` |

**Why untaken today.** `BreadcrumbBridgeCoordinatorTests.Constructor_NullArguments_Throw`
(`:408-422`) exercises the **public** 2-arg constructor. That constructor's `: this(...)` initializer
evaluates `CaptureProductionDispatcher(messenger, provider)` **first** (`:43`), and that method
throws at `:478` / `:481` before control ever reaches the internal constructor body. The internal
constructor is reached only from `BreadcrumbBridgeCoordinatorProbabilityTests.cs:147-151` and
`BreadcrumbSelectorCoordinatorTests.cs:393`, both of which pass three non-null arguments.

**Reachability: fully reachable, no production change.** The internal constructor is visible to
`QuickFiler.Test` (`AssemblyInfo.cs:5`) and already invoked there.

**Arrange/Act.** Three direct invocations of the 3-arg overload, each with exactly one null:

```
new BreadcrumbBridgeCoordinator(null, provider.Object, BreadcrumbUiDispatcher.CreateForCurrentThreadTests())
new BreadcrumbBridgeCoordinator(messenger.Object, null, BreadcrumbUiDispatcher.CreateForCurrentThreadTests())
new BreadcrumbBridgeCoordinator(messenger.Object, provider.Object, null)
```

**Assert.** `Should().Throw<ArgumentNullException>().WithParameterName("messenger" | "provider" | "dispatcher")`.
Asserting the parameter name is what makes this a contract test rather than a coverage artefact: it
pins the guard **ordering** (messenger before provider before dispatcher), which is the only
observable difference between the two constructors.

### G2 — collection-argument null guards (`:94`, `:133`) — 2 outcomes

| Line | Construct | Untaken side |
| --- | --- | --- |
| `:94` | `_ = rows ?? throw new ArgumentNullException(nameof(rows));` in `SetSuggestionsAsync` | the `throw` |
| `:133` | `_ = items ?? throw new ArgumentNullException(nameof(items));` in `AddItems` | the `throw` |

Note the asymmetry with `:102` (`SetSuggestions`), which is already 2/2 because
`BreadcrumbBridgeCoordinatorTests.SetSuggestions_NullRows_Throws` (`:362-369`) exists. G2 simply
extends that same contract to the other two entry points.

**Reachability: fully reachable, public API, no production change.**

**Arrange/Act.** A coordinator built with `CreateForCurrentThreadTests()`; then
`await coordinator.SetSuggestionsAsync(null, CancellationToken.None)` and
`coordinator.AddItems(null)`.

**Assert.** `ThrowAsync<ArgumentNullException>().WithParameterName("rows")` and
`Throw<ArgumentNullException>().WithParameterName("items")`. Additionally assert the guard fires
**before** any lease is begun — i.e. `messenger.PostJson` was never invoked — which distinguishes a
real guard from a guard placed after `BeginPopulation`.

### G3 — unleased render post (`:262`, condition 0) — 1 outcome

```
262:  if (lease != null && !_upgradeLifetime.IsCurrent(lease))
```

`condition-coverage="75% (3/4)"`, with `condition number="0"` at 50% and `condition number="1"` at
100%. Condition 0 is `lease != null`; only its **true** side has been observed.

**Why untaken.** `PostRenderAndSelectorAsync` declares `BreadcrumbUpgradeLease? lease = null`
(`:259`) but all three call sites pass a non-null lease: `:111`, `:127`, `:143`. The one reflective
invocation in the suite (`BreadcrumbCoordinatorLifecycleTests.cs:381-392`) also passes a lease.

**Reachability: reachable only through reflection or through an internal call.** The `lease == null`
path is *dead through the public and internal surface as currently wired*. It is not, however,
meaningless: it encodes the documented contract "a post with no lease publishes unconditionally".

**Recommended approach — reuse the existing in-repo reflection precedent.**
`BreadcrumbCoordinatorLifecycleTests.cs:381-392` already resolves
`typeof(BreadcrumbBridgeCoordinator).GetMethod("PostRenderAndSelectorAsync", Instance|NonPublic)` and
invokes it with `new object[] { "render", null, lease }`. The new test invokes the same method with
`new object[] { "render", null, null }`.

**Assert.** The returned `Task` is completed, and `PostJson` received exactly `"render"`. Trace:
`Guard(null, action)` returns `action` unwrapped
(`BreadcrumbCoordinatorUpgradeLifetime.cs:130`), `Dispatch` runs it inline, `_messenger.PostJson(renderJson)`
fires (`:271`), then `PostSelectorStateCore(null)` returns immediately at `:297-300` because a
`Mock<IWebViewMessenger>` is not a `BreadcrumbMessengerHub` — so passing `null` for the
`BreadcrumbSelectorState` argument is safe. **This single test also closes
`BreadcrumbCoordinatorUpgradeLifetime.cs:130`** (see the companion artifact, gap H3); the companion
artifact nonetheless recommends a direct `Guard(null, …)` test for isolation.

**Alternative considered and rejected: a production edit.** Making `lease` a required non-nullable
parameter would delete the branch outright and *reduce* line count. It is behavior-preserving. It is
rejected because (a) the epic NFR prefers zero production change, (b) it cascades into
`BreadcrumbCoordinatorUpgradeLifetime.Guard`'s own null arm, making that dead too and forcing a
second production edit in a second file, and (c) F13, F14 and F16 compile against this assembly and
the change has no coverage benefit that the reflection test does not already deliver.

### G4 — router-output selection change with no subscriber (`:382`) — 1 outcome

```
382:  SelectionChanged?.Invoke(this, EventArgs.Empty);
```

inside `PublishRouterOutputs` (`:367-385`). Only the non-null-delegate side is observed, because the
sole test that drives a router `SelectionChangeMessage` —
`BreadcrumbBridgeCoordinatorTests.InboundSelectionMessage_RaisesSelectionChangedWithMappedPath`
(`:187-206`) — subscribes first.

Note that the structurally identical `:287` (inside `PublishTransition`) is already 2/2, because
many tests call `SelectRow`/`SelectItem` without subscribing. The gap is specific to the inbound
router-output path.

**Reachability: fully reachable, public API, no production change.**

**Arrange.** Build the populated harness exactly as
`BreadcrumbBridgeCoordinatorTests.CreateHarness()` does (`:114-144`) but **do not** subscribe to
`SelectionChanged`.
*Lighter alternative for the planner to verify:* `coordinator.AddItems(new[] { "A", "B" })` supplies
plain Path-B rows with no `IFolderHierarchyProvider` interaction at all; confirm against
`UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` that an inbound
`selectionChange` over plain rows still emits a `SelectionChangeMessage` before adopting it.

**Act.** Raise `MessageReceived` with `{"type":"selectionChange","rowIndex":0}`, then await
`coordinator.LastDispatch`.

**Assert.** No exception; the selection message was posted back to the page
(`:378`); `PostSelectorStateCore` still ran (`:381`); `GetSelectedFolder()` reflects the new row.
The behavioral point being pinned is that **an unsubscribed host still receives the full posted
payload** — the event is a notification, not a gate.

### G5 — selector message matching no switch arm (`:397`, condition 3) — 1 outcome

```
397:  switch (BreadcrumbSelectorMessageSerializer.Parse(json))
```

`condition-coverage="87.5% (7/8)"`; conditions 0-2 are 100%, condition 3 is 50%. Condition 3 is the
last type-pattern test, `case BreadcrumbSelectorSubfolderActivationMessage subfolderActivation:`
(`:415`).

**Which side is untaken — determined from evidence, not inference.** `:416`-`:421` (the
subfolder-activation body) all report `hits="1"`, so the **match** side is taken. The untaken side is
therefore **"no pattern matched"** — the fall-out-of-switch path.

**Is that reachable?** Yes. `BreadcrumbSelectorMessageSerializer.Parse`
(`UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs:144-185`) can return a fifth
concrete type, `BreadcrumbSelectorViewMessage` (`:159-168`, discriminator `"selectorView"` at
`:46`), which matches none of the four arms at
`BreadcrumbBridgeCoordinator.cs:399/409/412/415`. And `IsSelectorMessage` (`:387-391`) admits it,
because `"selectorView".StartsWith("selector", Ordinal)` is true.

**Reachability: fully reachable, public API, no production change.**

**Arrange.** A coordinator with a `Mock<IWebViewMessenger>` and
`CreateForCurrentThreadTests()`; no rows required.

**Act.** Raise `MessageReceived` with
`{"type":"selectorView","mode":"collapsed","isOpen":false}` (both required fields present so `Parse`
succeeds — see `:161-165`), then await `LastDispatch`.

**Assert.** `LastDispatch` completed non-faulted; `PostJson` was never invoked; `IsSelectorOpen`,
`CommittedIdentity` and `PendingIdentity` are unchanged. This pins a genuine contract: **a
host-to-page message echoed back to the host is ignored rather than mis-dispatched or thrown.**

### G6 — degenerate `"type"` token scanning (`:441`, `:442`, `:443`) — 3 outcomes

```
440:  int colonIndex = json.IndexOf(':', markerIndex + marker.Length);
441:  int valueStart = colonIndex < 0 ? -1 : json.IndexOf('"', colonIndex + 1);
442:  int valueEnd   = valueStart < 0 ? -1 : json.IndexOf('"', valueStart + 1);
443:  return valueEnd > valueStart ? json.Substring(...) : null;
```

`:435` (`markerIndex < 0`) is already 2/2 — `BreadcrumbBridgeCoordinatorTests` `:210-228` feeds
`"{oops"`, which has no `"type"` marker at all. The three remaining lines are each 1/2: the
`colonIndex < 0` arm, the `valueStart < 0` arm, and the `valueEnd <= valueStart` arm are all
unobserved.

**Reachability: fully reachable, public API, no production change.** All three are reached with
JSON that contains the literal `"type"` but is structurally truncated after it.

**Recommended shape: one `[DataTestMethod]` with three `[DataRow]`s**, so each malformation is named
and independently diagnosable:

| Input | Effect | Line/side closed |
| --- | --- | --- |
| `{"type"}` | no `:` after the marker -> `colonIndex == -1` | `:441` true arm (and cascades through `:442`, `:443`) |
| `{"type":5}` | colon present, no `"` after it -> `valueStart == -1` | `:442` true arm, `:443` false arm |
| `{"type":"` | opening quote found, no closing quote -> `valueEnd == -1` | `:443` false arm |

**Assert.** For each input: the message was **not** treated as a selector message (no selector state
change), and the router surfaced a `BridgeErrorMessage` on the outbound channel — the same
observable contract already asserted for `"{oops"` at
`BreadcrumbBridgeCoordinatorTests.cs:221-227`. Downstream trace: `MessageType` returns `null` ->
`IsSelectorMessage` false (`:390`) -> `DispatchAsync` -> `RaiseSyntheticArrowKey` catches
`FormatException` and returns (`:455-459`) -> `_router.RouteAsync` posts the error response.

### 3.1 Projected result

Closing G1-G6 takes 11 of 11 untaken outcomes.

| Axis | Before | After (projected) | Floor |
| --- | --- | --- | --- |
| Line | 280/280 = 100.00% | 280/280 = 100.00% | >= 80% |
| Branch | 76/87 = 87.36% | 87/87 = **100.00%** | >= 75% |

No branch in this file is unreachable. **Zero documented deviations are required for reachability.**
The only judgement call is G3, which is reachable but only via the reflection precedent already
present in the suite.

---

## 4. Retain-or-Improve Risk Analysis

This file is at 100% line coverage. Every one of its 280 coverable lines is load-bearing on some
existing test, so **any test deletion or fixture change anywhere in `QuickFiler.Test/Viewers/` can
regress it**. Four concrete risks, in descending severity.

### R1 (highest) — `PostSelectorStateCore`'s concrete-type gate depends on two integration tests

```
297:  if (!(_messenger is BreadcrumbMessengerHub))
298:  {
300:      return;
301:  }
```

Lines `:302-320` — the `BreadcrumbSelectorViewMessage` serialization, the anonymous-type
`Options.Select` projection at `:310-316`, the `JavaScriptSerializer` call at `:317` and the string
splice at `:318-320` — execute **only** when the injected `IWebViewMessenger` is the concrete
`BreadcrumbMessengerHub`. Every `Mock<IWebViewMessenger>` and every `TrackingMessenger`
(`BreadcrumbCoordinatorLifecycleTests.cs:437-467`) returns at `:300`.

The only tests in the repository that pass a real hub to this coordinator are:

- `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs:198` and `:295`
  (`using (var hub = new BreadcrumbMessengerHub())`)
- `QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs:146-149`

**If either file is retired, retargeted, or has its hub replaced with a mock, 19 lines drop to zero
hits and this file falls from 100% to roughly 93% line coverage** — still above the 80% floor, but a
clear regression against the retain-or-improve bar and against issue #136 AC8.

Mitigation for the plan: an explicit AC that `BreadcrumbSelectorCoordinatorTests.cs` and
`BreadcrumbDuplicateIdentityIntegrationTests.cs` continue to construct a real
`BreadcrumbMessengerHub`, plus a post-merge re-measure.

### R2 — the fixture surface is 15 test files, not the 4 named in the brief

`BreadcrumbBridgeCoordinator` is referenced by **15 test files** (56 occurrences), not 4:

`BreadcrumbCoordinatorLifecycleTests.cs` (13), `BreadcrumbBridgeCoordinatorTests.cs` (7),
`BreadcrumbSelectorCoordinatorTests.cs` (8), `BreadcrumbUiThreadDispatchTests.cs` (6),
`BreadcrumbItemViewerLifecycleCoordinatorTests.cs` (4), `BreadcrumbBridgeCoordinatorProbabilityTests.cs` (3),
`BreadcrumbDuplicateIdentityIntegrationTests.cs` (3), `BreadcrumbDropDownReadinessTests.cs` (2),
`BreadcrumbSelectorToggleUiBoundaryTests.cs` (2), `BreadcrumbDropDownHostTests.cs` (1),
`BreadcrumbDropDownLifecycleTests.cs` (1), `BreadcrumbMessengerHubTests.cs` (1),
`FolderBreadcrumbAssetContractTests.cs` (1), `BreadcrumbPopupPlacementTests.cs` (1),
`BreadcrumbSubfolderActivationTests.cs` (1).

Several of those primarily target **F13-owned** production files (`BreadcrumbDropDown*`,
`BreadcrumbPopupPlacement`). F13's plan touches them. The 100% line figure for this F12 file is
therefore partly a by-product of F13-adjacent tests, and the two children must not both assume the
other will keep them.

### R3 — the `BreadcrumbUiDispatcher` capture path is F13-owned

The public constructor's only untestable-in-isolation step is
`CaptureProductionDispatcher` -> `BreadcrumbUiDispatcher.CaptureCurrent()`
(`BreadcrumbBridgeCoordinator.cs:471-485`; `BreadcrumbUiDispatcher.cs:44-56`). `CaptureCurrent`
**throws `InvalidOperationException` when `SynchronizationContext.Current` is null**
(`BreadcrumbUiDispatcher.cs:46-50`). Every test that uses the public constructor must therefore
install an ambient context first — `BreadcrumbBridgeCoordinatorTests.cs:95-112` and
`BreadcrumbCoordinatorLifecycleTests.cs:23-34` both do, with `finally`/`[TestCleanup]` restoration.

`BreadcrumbUiDispatcher.cs` is on **F13's** file list. Any change F13 makes to `CaptureCurrent`,
`CreateForCurrentThreadTests`, `Dispatch`, or the `[ThreadStatic] _executingDispatcher` boundary
proof (`BreadcrumbUiDispatcher.cs:14-15`, `:255-278`) changes how F12's fixtures behave. F13's own
spec (`spec.md:49-50`) commits to **no public or internal signature changes** to its 15 files and
names F12 as a dependent — that commitment is F12's protection and should be cited in F12's plan.

### R4 — private-member reflection couplings, both directions

- **Into this file:** `BreadcrumbCoordinatorLifecycleTests.cs:370-373` reads the private field
  `"_upgradeLifetime"`; `:381-384` resolves the private method `"PostRenderAndSelectorAsync"`;
  `BreadcrumbSelectorCoordinatorTests.cs:152` reads a private router field. Renaming any of those
  three members silently breaks tests at runtime, not at compile time. G3 adds a fourth such
  dependency — acceptable, since it reuses an existing anchor rather than creating a new one.
- **Out of this file (cross-child):** `QuickFiler.Test/Viewers/BreadcrumbPopupPlacementTests.cs:140`
  reads `typeof(BreadcrumbBridgeCoordinator).Assembly` purely as an assembly handle to reach
  `QuickFiler.Viewers.BreadcrumbPopupPlacement`. **Verified: the coupling is still present on this
  branch.** F13 has committed to re-anchoring it on an F13-owned type
  (`.../455/spec.md:515-518`). F12 must not pre-empt that edit — the file is F13-owned test surface
  — but should record the dependency so a fan-in conflict is expected rather than treated as a
  defect.

### R5 — cross-child construction of an F14 type inside an F12 test

`BreadcrumbCoordinatorLifecycleTests.ViewerScope` (`:469-487`) constructs
`new QuickFiler.ItemViewer()` and calls `InitializeBreadcrumbPipeline` /
`AttachBreadcrumbMessenger` / `ResetBreadcrumb` (`:122-127`). `ItemViewer` is **F14-owned**. This is
the only F12 test that instantiates a form-derived type. It is pre-existing and out of F12's scope to
change; flag it so F14 does not break it unknowingly.

---

## 5. Production Edit Verdict

**No production edit to `BreadcrumbBridgeCoordinator.cs` is required or recommended.**

All eleven untaken branch outcomes are reachable from `QuickFiler.Test` using the existing seams:
the `internal` 3-arg constructor (`:45`), the public API surface, and the one reflection precedent
already committed at `BreadcrumbCoordinatorLifecycleTests.cs:381-392`.

Consequences of that verdict:

- **The 13-line headroom (487/500) is not consumed.** No new seam, no new adapter type, no new
  member.
- **The #457 measurement trap does not apply.** No `[ExcludeFromCodeCoverage]` is introduced, at
  either type or method level, so there is no lifted-lambda leak to reason about. Recorded for
  completeness: had a thin-forwarder adapter been required, it would have to be `sealed` and **not**
  `partial` with a **type-level** attribute (epic § "Measurement Trap", § "fourth exemption ground"
  condition 4).
- **No `QuickFiler/QuickFiler.csproj` edit** is needed for this file, since no production file is
  created.

Rejected alternative, for the record: converting `PostRenderAndSelectorAsync`'s optional
`BreadcrumbUpgradeLease? lease = null` to a required parameter would delete the `:262` branch and
shorten the file. It is behavior-preserving but forces a second production edit in
`BreadcrumbCoordinatorUpgradeLifetime.cs` (the now-dead `Guard` null arm) and buys nothing the
reflection test does not already deliver.

---

## 6. Test-File Plan

### 6.1 Headroom against the 500-line test-file limit

| File | Lines | `[TestMethod]` | Headroom |
| --- | --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs` | 488 | 16 | **12** |
| `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs` | 489 | 11 | **11** |
| `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorProbabilityTests.cs` | 168 | 3 | 332 |
| `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` | 122 | 4 | 378 |

All four counts independently confirmed.

### 6.2 Recommendation — one new standalone test class, no `.Part2.cs`

**Create `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorGuardTests.cs`** — a new
`[TestClass]`, **not** a partial companion.

| Task | Test method | Closes |
| --- | --- | --- |
| T1 | `InternalConstructor_NullArgument_ThrowsForTheExpectedParameter` | `:51`, `:52`, `:55` |
| T2 | `NullCollectionArguments_ThrowBeforeAnyLeaseOrPost` | `:94`, `:133` |
| T3 | `PostRenderAndSelectorAsync_NoLease_PublishesUnconditionally` | `:262` (+ lifetime `:130`) |
| T4 | `RouterSelectionOutput_WithNoSubscriber_StillPostsAndUpdatesSelection` | `:382` |
| T5 | `InboundSelectorViewMessage_MatchesNoSelectorArmAndIsIgnored` | `:397` |
| T6 | `MalformedTypeToken_IsNotTreatedAsASelectorMessage` (`[DataTestMethod]`, 3 `[DataRow]`s) | `:441`, `:442`, `:443` |

**Six new `[TestMethod]`/`[DataTestMethod]` declarations; nine executions counting the DataRows.**
Estimated 200-230 lines including a compact local harness — comfortably inside 500.

Why a standalone class rather than `BreadcrumbBridgeCoordinatorTests.Part2.cs`:

1. `BreadcrumbBridgeCoordinatorTests` is declared `public sealed class` at
   `BreadcrumbBridgeCoordinatorTests.cs:25` — it is **not** `partial`. A `.Part2.cs` companion would
   require editing that 488-line file's class declaration, which is a fan-in conflict surface F13 and
   F14 also touch.
2. Five of the six new tests need no `IFolderHierarchyProvider` chain at all — the heavy
   `ProviderMock` fixture at `BreadcrumbBridgeCoordinatorTests.cs:47-69` is unnecessary. The
   lightweight pattern from `BreadcrumbBridgeCoordinatorProbabilityTests.cs:142-152`
   (`Mock<IWebViewMessenger>` + `Mock<IFolderHierarchyProvider>` + `CreateForCurrentThreadTests()`)
   suffices in about ten lines.
3. Only T4 wants a populated coordinator; it can either replicate `CreateHarness` locally or use the
   lighter `AddItems` arrangement noted in G4.

The repo does have `.Part2.cs` precedent —
`QuickFiler.Test/QuickFiler.Test.csproj:82` (`BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`) and
`:85` (`BreadcrumbPopupBoundaryCoverageTests.Part2.cs`) — so the pattern is available if a reviewer
prefers it. It is simply not needed here.

### 6.3 csproj registration

`QuickFiler.Test/QuickFiler.Test.csproj` is a non-SDK project with explicit `<Compile Include>`
entries and no globbing. Add exactly one line, adjacent to the existing breadcrumb block:

```
<Compile Include="Viewers\BreadcrumbBridgeCoordinatorGuardTests.cs" />
```

Insert immediately after `:61` (`BreadcrumbBridgeCoordinatorProbabilityTests.cs`). **Preserve CRLF**
— use the Edit tool, never a git-bash `sed -i` (epic § "Cross-Child Constraints" 1b). No
`QuickFiler/QuickFiler.csproj` edit is required by this file.

### 6.4 Determinism contract for every new test

- Dispatcher: `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` (inline, no pump) for T1-T6.
  Where an ambient context is unavoidable, install and restore it in `try/finally` per
  `BreadcrumbBridgeCoordinatorTests.cs:100-111`.
- Async edges: `await coordinator.LastDispatch` after raising `MessageReceived`, exactly as
  `BreadcrumbBridgeCoordinatorTests.Harness.Receive` does at `:78-82`. No polling.
- Prohibited and absent: `Thread.Sleep`, `Task.Delay`, wall-clock waits, real-time polling, temporary
  files, any filesystem write, external services or processes, live or shown forms, popups, STA
  attributes, injected clocks, `TimeProvider`.
- Framework: MSTest `[TestClass]`/`[TestMethod]`/`[DataTestMethod]`, Moq, FluentAssertions,
  Arrange-Act-Assert with explicit section comments.

---

## 7. Latent Defects — verified, assessed, NOT fixed

### LD-1 — `_messenger.PostJson` executes while `BreadcrumbCoordinatorUpgradeLifetime._sync` is held

**Severity: Low-Medium. Recommend promoting to a GitHub issue.**

Verified call chain:

1. `BreadcrumbBridgeCoordinator.cs:266-275` — `_dispatcher.Dispatch(_upgradeLifetime.Guard(lease, () => { _messenger.PostJson(renderJson); PostSelectorStateCore(selectorState); }))`.
2. `BreadcrumbCoordinatorUpgradeLifetime.cs:130` — `Guard` wraps the action in
   `() => TryRunCurrent(lease, action)`.
3. `BreadcrumbCoordinatorUpgradeLifetime.cs:139-146` — `TryRunCurrent` takes `lock (_sync)` and calls
   `action()` at `:145` **inside** the lock.
4. In production the messenger is `BreadcrumbMessengerHub`, whose `PostJson`
   (`BreadcrumbMessengerHub.cs:119-136`) takes its **own** `lock (_sync)` at `:126` and, still
   holding it, calls `PostToSurface` at `:133`, which reaches the WebView2 surface.

Two consequences:

- **Nested two-lock acquisition** in a fixed order, `lifetime._sync` -> `hub._sync`. Checked for
  inversion: `BreadcrumbMessengerHub.OnSurfaceMessageReceived` (`:157-172`) snapshots the handler
  under its lock at `:170` and invokes it **outside** the lock at `:172`, so the inbound path does
  not take the reverse order. **No deadlock is demonstrable on the current code.**
- **The lock does not deliver the atomicity it appears to.** An STA COM call made from
  `PostToSurface` can pump messages and re-enter managed code on the same thread. `lock` is
  re-entrant, so a re-entrant `BeginPopulation` / `Invalidate` / `TryDispose` would acquire
  `lifetime._sync` successfully and mutate `_current` **between** the currency check at
  `BreadcrumbCoordinatorUpgradeLifetime.cs:141` and the completion of `action()` at `:145` — exactly
  the invariant `TryRunCurrent` exists to enforce.

Fixing this means moving `action()` outside the lock and re-checking currency, which is a behavior
change to concurrency semantics — squarely outside the epic's no-behavior-change NFR. Per the epic's
"Latent Defect Promotion" section it must become a GitHub issue rather than prose in a feature
folder. Also recorded in the companion artifact as its LD-A.

### LD-2 — non-injectable internal construction of the router and the upgrade lifetime

**Severity: Low (informational). Does NOT warrant a GitHub issue.**

`BreadcrumbBridgeCoordinator.cs:52` constructs `FolderBreadcrumbBridgeRouter` and `:56` constructs
`BreadcrumbCoordinatorUpgradeLifetime`; neither is injectable.

Assessment:

- `FolderBreadcrumbBridgeRouter` is a `public sealed class` in **UtilitiesCS**
  (`UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:10`). Being `sealed`, it cannot
  be mocked — but its only external dependency, `IFolderHierarchyProvider`, **is** injected straight
  through it from the coordinator's own constructor, and the router carries its own coverage in
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`,
  `…InFlightTests.cs`, and `…EdgeTests.cs`. It is deterministic and touches no I/O.
- `BreadcrumbCoordinatorUpgradeLifetime` is `internal sealed` and is reachable from tests both
  directly (`BreadcrumbCoordinatorUpgradeLifetimeTests.cs`) and by reflection through the
  coordinator (`BreadcrumbCoordinatorLifecycleTests.cs:370-379`).

The decisive evidence is the coverage itself: **the file already sits at 100% line and 87.36% branch
with these constructions in place**, and every remaining gap has a reachable arrange. The
non-injectability is a mild deviation from the `.claude/rules/csharp.md` seam hierarchy but blocks
nothing. Adding constructor overloads to inject them would consume the 13-line headroom for no
coverage gain.

### LD-3 (new, not in the brief) — `SuggestionsUpgrade` is assigned inside the guarded action

**Severity: Low. Recommend recording; promotion optional.**

`BreadcrumbBridgeCoordinator.cs:112` assigns `SuggestionsUpgrade = PopulateSuggestionsAsync(rows, lease)`
**inside** the lambda passed to `_upgradeLifetime.RunSynchronous` (`:105-114`). If the lease is not
current when `TryRunCurrent` evaluates it (`BreadcrumbCoordinatorUpgradeLifetime.cs:141`), the lambda
never runs, `SetSuggestions` returns normally, and `SuggestionsUpgrade` silently retains its
**previous** value while the caller believes a new upgrade is in flight. The window exists because
nothing spans `BeginPopulation` (`:104`) and `RunSynchronous` (`:105`) atomically. It is
concurrency-only and no test reproduces it. `AddItems` (`:131-147`) has the same structure but
exposes no observable handle at all — its dispatch task is discarded at `:141`.

### LD-4 (new, not in the brief) — concrete-type check where a capability check belongs

**Severity: Low (design). Recommend recording; promotion optional.**

`BreadcrumbBridgeCoordinator.cs:297` gates selector-state publication on
`_messenger is BreadcrumbMessengerHub`, a **concrete class** test, rather than on a capability
interface. This inverts the epic's stated seam hierarchy (interface seam > injectable delegate >
adapter, epic § "Seam hierarchy") and is the direct cause of risk **R1** above: nineteen lines of
this file are reachable only from integration tests that happen to construct the real hub. A
`ISelectorStateSink`-style capability interface would remove both the design smell and the coverage
fragility — but that is a production API change and is out of scope here.

---

## 8. Corrections to the Brief

1. **`FolderBreadcrumbBridgeRouter` is a UtilitiesCS type, not a QuickFiler one.** Declared at
   `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:10`. It is a different type
   from F12's own `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:19`
   (`public sealed class BreadcrumbBridgeRouter`). The LD-2 assessment turns on this distinction.
2. **The existing-test inventory is 15 files, not 4.** See R2. The four files the brief lists carry
   34 of 56 references; the remaining 22 sit in eleven further files, several of them F13-adjacent.
   Any retain-or-improve analysis limited to the four named files is incomplete.
3. **"Use an injected clock and fake timers" is wrong for this file and must be struck.** Zero
   `DateTime` / `Stopwatch` / `Timer` / `Task.Delay` / `Thread.Sleep` / `TimeProvider` occurrences.
   Confirms and adopts F13's ruling at `.../455/spec.md:381-390`. The phrasing survives in
   `.../495/spec.md:69-70` and `:112` and needs a documented deviation.
4. **The `:397` untaken side is "no arm matched", not an unexercised case arm.** Determined from
   `hits="1"` on `:416`-`:421` in the Cobertura class block, not inferred. The closing input is a
   `selectorView` message, whose existence as a fifth parseable selector type is easy to miss.
5. **Line-number drift: none.** All eleven gap lines re-anchor exactly on the current file.
6. Confirmed as stated in the brief: the coverage table, the eleven partial-branch lines, the
   absence of `hits="0"` lines, the 487/500 production headroom, the `AssemblyInfo.cs:5` IVT grant,
   the test-file line and `[TestMethod]` counts, the `.Part2.cs` precedent, the non-globbing test
   csproj, and the still-present `BreadcrumbPopupPlacementTests.cs:140` assembly-handle coupling.
