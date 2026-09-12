# Research: `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` (F12 / issue #495)

- Timestamp: 2026-08-08T02-10
- Epic: `docs/features/epics/quickfiler-per-file-coverage/epic.md` (#136), child F12
- Child issue: #495
- Branch: `feature/quickfiler-breadcrumb-bridge-coverage-r2` (based on `epic/quickfiler-per-file-coverage-integration`)
- Scope: ONE production file, per the #136 one-research-artifact-per-file mandate.
- Sibling artifact (format template): `2026-08-08T01-15-breadcrumb-bridge-coordinator.md`

---

## 0. Executive summary

The brief's four headline figures for this file are **confirmed exactly** — 318 coverable lines,
90.57% line, 66.44% branch, 146 branch points, and exactly **49 untaken branch outcomes**. This is
the first sibling brief in the epic to survive re-measurement without a numeric correction.

The substantive correction is structural, not numeric: **this 481-line file declares three types and
one delegate, not one type.** 30 of the 49 untaken outcomes and 28 of the 30 uncovered lines sit in
the two *other* types — principally the `internal static class BreadcrumbPopupLifecycleOperations`,
whose `CreateCollapsedCandidate` method is **0% covered end-to-end** (20 uncovered lines, 10 untaken
outcomes). A planner that scopes this file as "the lifecycle coordinator" will mis-plan the majority
of the work.

All 49 outcomes are closable. 46 are reachable with no production change and no reflection; 3
(`:135`, `:138`, `:234`) are structurally unreachable through the API and need private-field
reflection or a documented deviation. **No production edit is required or recommended.**

---

## 1. Current State — verified

### 1.1 File shape

`QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` is **481 physical lines** (last
line `}` at `:481`, closing the namespace). Against the 500-line ceiling in
`.claude/rules/general-code-change.md` § File Size Limit that is **19 lines of headroom**.

The file declares **four top-level members**, not one:

| Member | Declared at | Kind | Coverable lines | Untaken outcomes |
| --- | --- | --- | --- | --- |
| `BreadcrumbItemViewerLifecycleCoordinator : IDisposable` | `:13` | `internal sealed class` | 222 (`:29`-`:327`) | 19 |
| `NavigationSubscriptionFactory` | `:330` | `internal delegate` | 0 | 0 |
| `BreadcrumbNavigationSubscription : IDisposable` | `:337` | `internal sealed class` | 9 (`:341`-`:351`) | 2 |
| `BreadcrumbPopupLifecycleOperations` | `:355` | `internal static class` | 87 (`:361`-`:479`) | 28 |

- **None of the three types is `partial`.** All three are `internal`; two are `sealed`, one is
  `static`. No `[ExcludeFromCodeCoverage]` appears anywhere in the file (verified by full read of
  all 481 lines and by targeted grep — zero matches). No other partial of any of these types exists,
  so there is no inherited type-level suppression of the kind confirmed on `QfcDatamodel.cs:25` and
  `ItemViewer.cs`.
- No `System.Windows.Forms`, no `Microsoft.Office.Interop.Outlook`, no WebView2 type reference. The
  only `System.Drawing` use is `Rectangle` in `ConfigureHost`'s two `Func<Rectangle>` parameters
  (`:110`-`:111`).
- **Reachability from `QuickFiler.Test`:** `QuickFiler/Properties/AssemblyInfo.cs:5` contains
  `[assembly: InternalsVisibleTo("QuickFiler.Test")]`. Every member of all three types is therefore
  directly callable from the test assembly, and already is —
  `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs:229-239` constructs the
  coordinator through its only (internal, 6-argument) constructor at `:29-36`.
- **Constructor surface:** exactly one, `internal`, 6 parameters (`hub`, `collapsedAttachment`,
  `operations`, `selectionChanged`, `folderArrow`, `unhandledArrow`), all null-guarded at `:38`-`:44`
  in that order. `BreadcrumbNavigationSubscription` has one internal 1-argument constructor (`:341`).
  `BreadcrumbPopupLifecycleOperations` is static with four `internal static` methods.

> **Cohesion note (observation, not an action).** Three unrelated types plus a delegate in one file
> is in tension with `.claude/rules/general-code-change.md` § Module Rigor / CLAUDE.md §C#5.1
> ("Keep files focused on one responsibility area"). Splitting it is **not** recommended here: it
> would require a `QuickFiler/QuickFiler.csproj` edit, a new ledger row per the epic's "Mid-Wave
> File Creation" rules, a >= 90% target on the new file, and it would invalidate the per-file
> baseline mid-epic. Record it as a post-epic candidate.

### 1.2 Collaborators and their owning child

Every declaration below was resolved by grep against this checkout.

| Symbol | Declared at | Owner |
| --- | --- | --- |
| `BreadcrumbMessengerHub` | `QuickFiler/Viewers/BreadcrumbMessengerHub.cs:15` | **F12** (sibling file) |
| `BreadcrumbCollapsedAttachment` | `QuickFiler/Viewers/BreadcrumbMessengerHub.cs:277` | **F12** (sibling file) |
| `BreadcrumbResourceOwner` | `QuickFiler/Viewers/BreadcrumbMessengerHub.cs:436` | **F12** (sibling file) |
| `BreadcrumbBridgeCoordinator` | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:25` | **F12** (sibling file) |
| `BreadcrumbPopupUiOperations` | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:29` | **F13** (#455) |
| `BreadcrumbUiDispatcher` | `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:12` | **F13** (#455) |
| `BreadcrumbDropDownOpenCoordinator` | `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:12` | **F13** (#455) |
| `BreadcrumbCollapsedSurfaceController` | `QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs:11` | **F13** (#455) |
| `BreadcrumbNavigationReadiness` | `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs:19` | **F13** (#455) |
| `IWebViewMessenger` | `QuickFiler/Viewers/IWebViewMessenger.cs:13` | **F13** (#455) |
| `IBreadcrumbDropDownHost` | `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs:19` | **F13** (#455) |
| `ItemViewer` (sole production consumer) | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:13` | **F14** (#456) |
| `BreadcrumbArrowDirection` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs:11` | **UtilitiesCS** |
| `BreadcrumbSelectorViewMode` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs:9` | **UtilitiesCS** |

> **Trap 1 — the sibling artifact's error class, repeated.** `BreadcrumbNavigationReadiness` is
> **not** declared in a file bearing its name. It lives in
> `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs:19` and is **F13-owned**, despite being
> constructed and disposed on almost every line of this file's static helper. A planner searching
> for `BreadcrumbNavigationReadiness.cs` will find nothing and may wrongly conclude the type is
> F12-owned or absent.

> **Trap 2 — two F12 collaborator types have no file of their own.**
> `BreadcrumbCollapsedAttachment` (`:277`) and `BreadcrumbResourceOwner` (`:436`) are declared
> *inside* `BreadcrumbMessengerHub.cs`. They are F12-owned and are covered by the sibling research
> artifact for that file, but a type-name-to-file mapping will fail on them.

> **Trap 3 — the sole production consumer is F14-owned.** Every non-test call into this file
> originates in `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` (`:15`, `:27`, `:30`, `:58`, `:74`,
> `:84`, `:120`, `:139`, `:194`, `:198`, `:208`, `:234`, `:237`, `:268`, `:293`). That file is
> **F14's** and must not be edited by F12. It is also the subject of open issue **#488**.

### 1.3 Concurrency and determinism inventory

A single grep over the whole file for
`DateTime|Stopwatch|Timer|Task\.Delay|Thread\.Sleep|TimeProvider|lock |Interlocked|volatile|SynchronizationContext|ConfigureAwait|CancellationToken|TaskCompletionSource|async |await `
returns **exactly one match**:

```
348:  Action? detach = System.Threading.Interlocked.Exchange(ref _detach, null);
```

Complete inventory, cross-checked against a full read:

| Construct | Present? | Evidence |
| --- | --- | --- |
| `DateTime` / `Stopwatch` / `Timer` / `Task.Delay` / `Thread.Sleep` / `TimeProvider` | **none** | grep, zero matches |
| `lock` / `Monitor` / `volatile` | **none** | grep, zero matches |
| `Interlocked` | **one** | `:348`, `Exchange(ref _detach, null)` — the single-shot disposal latch |
| `async` / `await` / `ConfigureAwait` | **none** | grep, zero matches; `Task<bool>` values are forwarded, never awaited |
| `CancellationToken` | **none** | grep, zero matches |
| `TaskCompletionSource` | **none** | grep, zero matches |
| `SynchronizationContext` | **none directly** | reached only indirectly through `BreadcrumbPopupUiOperations.PostAsync` (F13) |
| Fire-and-forget discards | **four** | `:120`, `:167`, `:227` (`_ = _operations.PostAsync(...)`); `:456`, `:458`, `:461` (`_ = dispatcher.Dispatch(...)`) |
| Generation / re-entrancy guard | **yes** | `_generation` (`:26`), bumped at `:194` and `:209`; read at `:119`, `:166`, `:226`; tested by `IsCurrent` (`:319`) |
| Disposal flag | **yes** | `_disposed` (`:27`), set at `:208`; enforced by `ThrowIfDisposed` (`:321-327`) on every method but no property |
| Mutable `Task` ordering seam | **one** | `CurrentOpenTask` (`:55-56`) forwards `_openCoordinator?.CurrentOpenTask` |

**Determinism finding — the brief's "injected clock and fake timers" instruction is REFUTED for
this file and must be struck.**

`docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/spec.md:69-70` states
"Use an injected clock and fake timers", and `:112` seeds a test condition "Deterministic
time-dependent behavior via injected clock and fake timers". There is **no time dependency of any
kind** in this file. Determinism here is **scheduler control**, exactly as sibling F13 ratified at
`docs/features/active/2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455/spec.md:381-390`
(§8.1: "Determinism here is **scheduler** control, not clock control. Any plan task that introduces
an injected clock or a fake-timer facility is out of scope and must be rejected — it would add a
seam with no dependency to control."), and exactly as the F12 sibling artifact concluded for
`BreadcrumbBridgeCoordinator.cs`. **Record as a documented deviation and strike both phrasings.**

Two further spec corrections follow from the same inventory:

- `spec.md:106-111` seeds "Cancellation and cancelled-token paths". There is **no
  `CancellationToken` in this file at all**. That seeded condition is not applicable here.
- `spec.md:42-44` characterises the branch gap as "guard clauses, cancellation paths, double-invoke
  guards, disposal guards, and out-of-order state transitions". Guard clauses are indeed the largest
  single group (**29 of 49 untaken outcomes** are `?? throw` guards), and disposal/out-of-order
  guards account for a further 4. But the second-largest group — **10 outcomes and 20 uncovered
  lines** — is an entirely **untested static factory method**, which the characterisation does not
  anticipate.

**Deterministic vehicles that already exist in `QuickFiler.Test/` and are green:**

1. `QueuedCreatorThreadSynchronizationContext` — a manually pumped `SynchronizationContext` with
   `DrainOnCreatorThread()`, `CreatorThreadId`, and `CallbackThreads`. Declared as a private nested
   class **twice**: `BreadcrumbItemViewerLifecycleCoordinatorTests.cs:299-325` and
   `BreadcrumbPopupUiOperationsDirectAdapterTests.cs:274-300`. F13's spec §8.2 names the latter as
   the ratified vehicle. New test classes replicate the same private nested pattern (the existing
   copies are `private`, so they cannot be shared without editing an existing file).
2. `new BreadcrumbUiDispatcher(queue, _ => { })` — the internal 2-argument constructor, used at
   `BreadcrumbItemViewerLifecycleCoordinatorTests.cs:227` and
   `BreadcrumbPopupUiOperationsDirectAdapterTests.cs:231`.
3. `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` (`BreadcrumbUiDispatcher.cs:62-65`) — an
   owner-thread-only dispatcher that runs every `Dispatch(...)` inline, with no context and no pump.
4. Test-owned `TaskCompletionSource` gates and the `RecordingNavigationBinding` push-driven fake
   (`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:244-272`), which lets a test fire
   navigation-started / navigation-completed / owner-disposed callbacks synchronously.

---

## 2. Measured Baseline — independently recomputed

Source: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`.

### 2.1 Harness compliance

- **Exactly one `<class>` element carries
  `filename="QuickFiler\Viewers\BreadcrumbItemViewerLifecycleCoordinator.cs"`** — XML line **7850**,
  `name="QuickFiler.Viewers.BreadcrumbItemViewerLifecycleCoordinator"`, closing at XML **8873**. A
  grep of the entire report for the filename returns one hit. **No cross-class union by filename is
  required for this file**, even though the source declares three types: the report rolls all three
  types' lines, and the lifted lambda closures, into that single class element's class-level block.
  (This is worth recording for F1's harness: the "one file can emit several `<class>` elements"
  rule is a *may*, not a *must*, and the absence of extra elements is not evidence that the other
  types are uninstrumented.)
- The `<methods>` subtree runs XML **7851-8275**; the **class-level `<lines>` block runs XML
  8276-8872** and is the only block read below. No `class.iter('line')`, no `.//lines/line`.
- `branches-valid` is non-zero (146), so the zero-branch N/A rule does not apply.

### 2.2 Recomputed figures

| Metric | Recomputed value | Brief's value | Verdict |
| --- | --- | --- | --- |
| Coverable lines (class-level `<line>` count) | **318** | 318 | **confirmed** |
| Lines with `hits="0"` | **30** | — | — |
| Line coverage (288/318) | **90.566%** | 90.6% | **confirmed** |
| Branch points (Σ `condition-coverage` denominators) | **146** | 146 | **confirmed** |
| Branch outcomes taken (Σ numerators) | **97** | — | — |
| Branch coverage (97/146) | **66.438%** | 66.4% | **confirmed** |
| Untaken branch outcomes | **49** | "roughly 49" implied | **confirmed exactly** |

Floors: >= 80% line and >= 75% branch (epic § "Coverage-Target Reconciliation"). **This file passes
the line gate and FAILS the branch gate by 8.56 points.**

### 2.3 The emitted attributes are both wrong — proof of #441 on this file

The `<class>` element emits `line-rate="0.939516"` and `branch-rate="0.688073"`. Both are inflated.
The mechanism was reconstructed exactly:

- Summing the **method-level** `<lines>` blocks gives 178 lines, all `hits="1"`.
  `(288 + 178) / (318 + 178) = 466/496 = 0.9395161` — which reproduces the emitted `line-rate` to
  seven significant figures.
- Summing the **method-level** `condition-coverage` gives 53/72.
  `(97 + 53) / (146 + 72) = 150/218 = 0.6880734` — which reproduces the emitted `branch-rate` to
  seven significant figures.

So the emitted rates are the class-level block **unioned with the method subtree**, double-counting
every line and branch that appears in both. The distortion is +3.4 points on line and +2.4 points on
branch. Notably the emitted `branch-rate` of 0.688 is *still* below the 75% floor, so on this file
#441 does not produce a false pass — but it would have understated the size of the gap by roughly a
third. **Compute; do not read.**

### 2.4 Line-number drift: none

Every one of the 318 reported line numbers resolves to the construct the analysis predicts on the
current working-tree file. Three independent anchors were checked:

- The `<method>` element for `.ctor` lists `:29`-`:49` inclusive with branch points at exactly
  `:38, :39, :41, :42, :43, :44` — matching the six `?? throw` guards in the current file, with
  `:40` (the continuation line of the two-line `_collapsedAttachment` assignment) correctly
  non-branching.
- `AttachMessenger`'s method block lists `:250`-`:268` with branches at `:251, :252, :258, :264` —
  matching `_ = messenger ?? throw`, `if (ReferenceEquals(slot, messenger))`, `if (slot != null)`,
  `if (_hub.Attach(messenger, mode))`.
- The class-level block's tail runs to `:479` (the closing brace of `NavigateWithSubscription`),
  with `:478` (the catch's closing brace) correctly absent. `:480` and `:481` are the type and
  namespace closers and are not instrumented.

**No re-anchoring is required.**

---

## 3. Complete branch-point census

68 branching lines, 146 outcomes, 97 taken, 49 untaken.

### 3.1 Fully covered (26 lines, 61 outcomes)

`:53` (2/2), `:66` (2/2), `:127` (4/4), `:130` (8/8), `:136` (2/2), `:148` (2/2), `:169` (2/2),
`:179` (2/2), `:181` (2/2), `:195` (2/2), `:196` (2/2), `:203` (2/2), `:216` (2/2), `:229` (2/2),
`:235` (2/2), `:251` (2/2), `:252` (2/2), `:258` (2/2), `:264` (2/2), `:272` (2/2), `:283` (2/2),
`:295` (2/2), `:308` (2/2), `:323` (2/2), `:416` (2/2), `:424` (2/2).

Two of these carry planning significance:

- **`:251` is already 2/2.** `_ = messenger ?? throw new ArgumentNullException(nameof(messenger))`
  in `AttachMessenger` is fully covered because
  `BreadcrumbItemViewerLifecycleCoordinatorTests.AttachCollapsedMessenger_Null_ThrowsArgumentNullException`
  (`:153-162`) exists. Do **not** duplicate it. It is also the positive control proving that
  coverlet *does* count the throw side of a `?? throw` — which is why the 0/2 readings at `:366`,
  `:388`, `:389`, `:390`, `:397`, `:406` must have a different cause (§3.3).
- **`:136` is 2/2 while its three structurally identical neighbours `:135`, `:137`, `:138` are
  1/2.** That asymmetry is the key that unlocks the untaken-side determination for the whole lambda
  group (§3.2, G11).

### 3.2 Partially covered — 42 lines, 49 untaken outcomes

Untaken sides are determined from `hits` on dependent lines and from the existing test bodies, not
by inference, except where explicitly flagged.

#### Constructor guards (`:38`, `:39`, `:41`, `:42`, `:43`, `:44`) — 6 outcomes, each 1/2

| Line | Construct | Untaken side | Evidence |
| --- | --- | --- | --- |
| `:38` | `_hub = hub ?? throw` | the `throw` | `:45`-`:49` all `hits="1"` — the ctor always completes |
| `:39` | `_collapsedAttachment = collapsedAttachment ?? throw` (continues on `:40`) | the `throw` | same |
| `:41` | `_operations = operations ?? throw` | the `throw` | same |
| `:42` | `_ = selectionChanged ?? throw` | the `throw` | same |
| `:43` | `_ = folderArrow ?? throw` | the `throw` | same |
| `:44` | `_ = unhandledArrow ?? throw` | the `throw` | same |

#### Property and instance-method guards

| Line | Construct | Cov. | Untaken side | Evidence |
| --- | --- | --- | --- | --- |
| `:56` | `_openCoordinator?.CurrentOpenTask ?? Task.FromResult(false)` | 2/4 | **both** `_openCoordinator == null` sides (cond 0 and cond 1) | The only consumer is `ItemViewer.BreadcrumbOpenTask` (`ItemViewer.Breadcrumb.cs:30`), driven by `BreadcrumbSelectorOpenRetryTests.cs:38,41,61,69` via `DrainUntil(...)`. A `DrainUntil` on `Task.FromResult(false)` would be a no-op, so those tests necessarily observe a non-null coordinator. |
| `:65` | `_ = bridgeCoordinator ?? throw` in `SetBridgeCoordinator` | 1/2 | the `throw` | `:66` is 2/2 and `:71`-`:77` are all hit |
| `:93` | `_ = messenger ?? throw` in `AttachCollapsedWithReadinessAsync` | 1/2 | the `throw` | `:95` hit |
| `:94` | `_ = readiness ?? throw` | 1/2 | the `throw` | `:95` hit |
| `:115` | `_ = host ?? throw` in `ConfigureHost` | 1/2 | the `throw` | `:119`-`:120` hit |
| `:116` | `_ = anchorBounds ?? throw` | 1/2 | the `throw` | same |
| `:117` | `_ = workingArea ?? throw` | 1/2 | the `throw` | same |
| `:122` | `if (!IsCurrent(generation))` inside the `ConfigureHost` post | 1/2 | the **true** side (stale generation) | `:123` and `:124` are `hits="0"` — decisive |
| `:158` | `_bridgeCoordinator?.SetTheme(theme)` | 1/2 | the **null** side | `SetTheme` is reached only via `ItemViewer.SetBreadcrumbTheme` from `BreadcrumbDropDownIntegrationTests.cs:151-152`, whose harness calls `InitializeBreadcrumbPipeline` at `:340` first, so a bridge always exists |
| `:159` | `DropDownHost?.SetTheme(theme)` | 1/2 | the **null** side | same test asserts `harness.Host.Verify(host => host.SetTheme("dark"), Times.Once())` at `:159`, proving the host is non-null when observed |
| `:165` | `_ = focus ?? throw` in `Focus` | 1/2 | the `throw` | `:166`-`:167` hit |
| `:222` | `_openCoordinator?.HandleSelectorOpenStateChanged()` | 1/2 | the **null** side | The only test that sets a bridge without configuring a host (`BreadcrumbItemViewerLifecycleCoordinatorTests.cs:135-151`) never triggers a selector-open transition; every test that does trigger one goes through `ItemViewer` with a configured host |
| `:234` | `IWebViewMessenger? messenger = DropDownHost?.PopupMessenger` | 1/2 | the **null** side | `:235` is 2/2 and `:237` is hit, so the non-null side is taken; `_openCoordinator` is never null while `IsCurrent` is true (§5, G10-d) |
| `:319` | `IsCurrent => !_disposed && generation == _generation` | 1/2 | the **`_disposed == true`** short-circuit | `:122`/`:169`/`:229` prove `IsCurrent` has returned both `true` and `false`, and the only `false` producer exercised today is the generation mismatch (`ResetDispose_LateCallbackDoesNotReattach`, `:77-96`, calls `Reset()` before the drain and `Dispose()` only after it) |

#### `BreadcrumbNavigationSubscription`

| Line | Construct | Cov. | Untaken side | Evidence |
| --- | --- | --- | --- | --- |
| `:343` | `_detach = detach ?? throw` | 1/2 | the `throw` | `:344` hit; the only construction site is `BreadcrumbPopupUiOperationsDirectAdapterTests.cs:263` with a non-null lambda |
| `:349` | `detach?.Invoke()` in `Dispose` | 1/2 | the **null** side (second `Dispose`) | `Interlocked.Exchange` at `:348` nulls the field on first dispose; no test disposes twice |

#### `BreadcrumbPopupLifecycleOperations.CreateNavigationSurface` (`:357`-`:378`)

| Line | Construct | Cov. | Untaken side | Evidence |
| --- | --- | --- | --- | --- |
| `:362` | `_ = readiness ?? throw` | 1/2 | the `throw` | `:365` hit |
| `:363` | `_ = createMessenger ?? throw` | 1/2 | the `throw` | `:365` hit |
| `:366` | `IWebViewMessenger messenger = createMessenger() ?? throw ...` | **0/2** | **both** | See below |

**Why `:366` reads 0/2 while `:367`-`:370` read `hits="1"`.** Both tests that call this method pass
a factory that **throws** rather than returning null:
`BreadcrumbItemViewerLifecycleCoordinatorTests.cs:63-67` and
`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:70-74`, each
`() => throw new InvalidOperationException("messenger")`. The exception escapes from inside
`createMessenger()` before either branch target of the `??` is reached, so neither outcome counts;
the `hits="1"` on `:367`-`:370` is the multi-line sequence point of the enclosing statement, and
`:371` (`return`) is `hits="0"` while `:373`-`:376` (the catch) are hit. **The success path of this
method has never executed and the `?? throw` has never fired.** `:251`'s 2/2 is the positive control
proving that a `?? throw` reached with a genuine `null` does register both outcomes.

#### `BreadcrumbPopupLifecycleOperations.CreateCollapsedCandidate` (`:380`-`:409`) — never executed

Every line `:387`-`:409` is `hits="0"` (20 lines). Five branching lines, all **0/2**:

| Line | Construct | Untaken |
| --- | --- | --- |
| `:388` | `_ = createMessenger ?? throw` | both |
| `:389` | `_ = createReadiness ?? throw` | both |
| `:390` | `IWebViewMessenger messenger = createMessenger() ?? throw ...` | both |
| `:397` | `BreadcrumbNavigationReadiness readiness = createReadiness() ?? throw ...` | both |
| `:406` | `(messenger as IDisposable)?.Dispose()` in the catch | both |

**Why it is untested.** Its only production caller is
`ItemViewer.CreateCollapsedBreadcrumbCandidate` (`ItemViewer.Breadcrumb.cs:77-98`), which
dereferences `_l0vhBreadcrumb_WebView2.CoreWebView2` — a live WebView2 — before calling it. The
likely reason it was believed covered: the test
`BreadcrumbItemViewerLifecycleCoordinatorTests.CandidateFailure_CleansMessengerAndReadiness`
(`:51-74`) is **named** for the candidate path but actually exercises `CreateNavigationSurface`
(`:64`), and is a near-duplicate of
`BreadcrumbPopupUiOperationsDirectAdapterTests.MessengerConstructionFailure_DisposesReadiness`
(`:64-79`). The misleading name is the reason the real gap went unnoticed.

#### `BreadcrumbPopupLifecycleOperations.DisposeTwoResources` (`:411`-`:432`)

| Line | Construct | Cov. | Untaken side | Evidence |
| --- | --- | --- | --- | --- |
| `:413` | `_ = disposeMessenger ?? throw` | 1/2 | the `throw` | `:415` hit |
| `:414` | `_ = disposeControl ?? throw` | 1/2 | the `throw` | `:415` hit |
| `:428` | `if (failure != null)` | 1/2 | the **false** side (both cleanups succeed) | `:429`-`:430` hit, `:432` is `hits="0"` — decisive. The only test, `TwoResourceCleanup_ReportsFirstFailureAfterAllCleanup` (`:114-135`), makes **both** actions throw, which is also why `:424` (`failure ??= exception`) is already 2/2 |

#### `BreadcrumbPopupLifecycleOperations.NavigateWithSubscription` (`:434`-`:479`)

| Line | Construct | Cov. | Untaken side | Evidence |
| --- | --- | --- | --- | --- |
| `:441` | `_ = dispatcher ?? throw` | 1/2 | the `throw` | `:445` hit |
| `:442` | `_ = navigate ?? throw` | 1/2 | the `throw` | `:445` hit |
| `:443` | `_ = createSubscription ?? throw` | 1/2 | the `throw` | `:445` hit |
| `:450` | `() => subscription?.Dispose()` (the readiness detach callback) | 1/2 | the **null** side | `:463` is 1/2 with `:464`-`:467` `hits="0"`, so `subscription` is always non-null by the time the detach callback runs |
| `:463` | `if (subscription == null)` | 1/2 | the **true** side | `:464`-`:467` are `hits="0"` — decisive. `RecordingNavigationBinding.Create` (`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:253-264`) always returns a subscription |
| `:475` | `subscription?.Dispose()` in the catch | 1/2 | the **null** side | the catch is reached only via `NavigationBinder_TranslatesDetachesAndCleansOnThrow` (`:101-110`), where `createSubscription` succeeded and `readiness.BeginNavigation(navigate)` then threw, so `subscription` is non-null |

Also uncovered but **not** a branch: `:461`, `() => _ = dispatcher.Dispatch(readiness.Cancel)`, the
owner-disposed callback body. `RecordingNavigationBinding.OwnerDisposed()`
(`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:271`) exists as a test helper and is **never
called by any test** — a dead helper that maps one-to-one onto this dead line.

#### Delegate lambdas constructed in `ConfigureHost` (`:135`, `:137`, `:138`)

All three are `_bridgeCoordinator?.X` null-conditionals passed to
`BreadcrumbDropDownOpenCoordinator`'s constructor. Each is 1/2; **the untaken side is the
`_bridgeCoordinator == null` side** in every case. Determined from the `:136` asymmetry:

- `:136` (`_isSelectorOpen`) is **2/2** because `BreadcrumbDropDownOpenCoordinator.Reset()`
  (`BreadcrumbDropDownOpenCoordinator.cs:138-147`) invokes it inside its posted lambda, and
  `BreadcrumbItemViewerLifecycleCoordinatorTests.ResetDispose_LateCallbackDoesNotReattach`
  (`:77-96`) drives exactly that path on a fixture that never sets a bridge — giving the null side.
  The ItemViewer-driven tests give the non-null side.
- `:135` (`_rowCount`) is invoked only at `BreadcrumbDropDownOpenCoordinator.cs:193` inside
  `BeginOpenCore`, reachable only from `RequestOpen` (`:112`, `:128`), both of which are gated on
  `_isSelectorOpen()` returning **true**.
- `:138` (`_cancelSelector`) is invoked at `BreadcrumbDropDownOpenCoordinator.cs:144`, `:207`,
  `:226`, `:265` — **every one** gated on `_isSelectorOpen()` returning **true**.
- `:137` (`_openSelector`) is invoked at `BreadcrumbDropDownOpenCoordinator.cs:110`, inside
  `SetDroppedDown(true)`'s posted lambda, with **no** `_isSelectorOpen()` gate.

With a null `_bridgeCoordinator`, `_isSelectorOpen()` returns `false` by construction. Therefore
`:135` and `:138` are **structurally unreachable via any call path** when the bridge is null, while
`:137` is freely reachable. This is a reachability *proof*, not an inference.

---

## 4. Gap inventory — atomic test tasks

Eleven gap groups. Column "Reach" is: **A** = fully reachable via public/internal API, no
reflection; **R** = reachable only via private-member reflection.

| Gap | Lines | Outcomes | Reach |
| --- | --- | --- | --- |
| G1 constructor null guards | `:38 :39 :41 :42 :43 :44` | 6 | A |
| G2 instance-method null guards | `:65 :93 :94 :115 :116 :117 :165` | 7 | A |
| G3 static-helper argument guards | `:343 :362 :363 :388 :389 :413 :414 :441 :442 :443` | 12 | A |
| G4 `CreateCollapsedCandidate` body | `:390 :397 :406` (+20 lines) | 6 | A |
| G5 `CreateNavigationSurface` messenger outcomes | `:366` (+2 lines) | 2 | A |
| G6 `DisposeTwoResources` clean path | `:428` (+1 line) | 1 | A |
| G7 `NavigateWithSubscription` null subscription | `:450 :463 :475` (+4 lines) | 3 | A |
| G8 owner-disposed callback | (line `:461` only) | 0 | A |
| G9 subscription double-dispose | `:349` | 1 | A |
| G10 coordinator lifecycle and null collaborators | `:56 :122 :158 :159 :222 :319` / `:234` | 7 / 1 | A / R |
| G11 host-delegate lambdas with no bridge | `:137` / `:135 :138` | 1 / 2 | A / R |
| **Total** | | **49** | **46 A / 3 R** |

---

### G1 — constructor null guards (6 outcomes)

**Construct.** Six sequential `?? throw new ArgumentNullException(...)` at `:38`-`:44`.

**Why untaken today.** The only two construction sites in the repository —
`BreadcrumbItemViewerLifecycleCoordinatorTests.cs:229-239` and
`ItemViewer.EnsureBreadcrumbLifecycle` (`ItemViewer.Breadcrumb.cs:268-275`) — both pass six non-null
arguments. `LifecycleFixture` is the test that comes closest: it constructs the coordinator on every
one of the ten existing tests, but never with a null.

**Reachability: A.** The constructor is `internal` and `AssemblyInfo.cs:5` grants
`InternalsVisibleTo("QuickFiler.Test")`.

**Arrange.** A valid 6-tuple (`hub`, `attachment`, `operations`, and three no-op delegates), built
once; then six invocations, each substituting exactly one `null`.

**Act/Assert.**
`Should().Throw<ArgumentNullException>().WithParameterName("hub" | "collapsedAttachment" |
"operations" | "selectionChanged" | "folderArrow" | "unhandledArrow")`.

**Contract pinned.** Asserting the *parameter name* rather than merely the exception type pins the
**guard ordering** (`hub` before `collapsedAttachment` before `operations` before the three
delegates). Ordering is the only externally observable property of a run of guards, and it is what a
later refactor is most likely to disturb. It also pins that `selectionChanged`, `folderArrow`, and
`unhandledArrow` are validated **before** being wrapped into the three handler fields at `:45`-`:47`
— a discard-then-wrap pattern (`_ = selectionChanged ?? throw ...` followed by
`_selectionChangedHandler = (_, __) => selectionChanged()`) that would otherwise defer the
`NullReferenceException` to first event dispatch.

---

### G2 — instance-method null guards (7 outcomes)

**Constructs.** `:65` (`SetBridgeCoordinator`), `:93`/`:94`
(`AttachCollapsedWithReadinessAsync`), `:115`/`:116`/`:117` (`ConfigureHost`), `:165` (`Focus`).

**Why untaken today.** `BreadcrumbItemViewerLifecycleCoordinatorTests` covers exactly one null-guard
of this family — `AttachCollapsedMessenger_Null_ThrowsArgumentNullException` (`:153-162`), which
closes `:251` inside the private `AttachMessenger`. No test extends the same discipline to the other
four entry points. `ConfigureHost` is called three times in that file (`:29`, `:33`, `:83`), always
with `FixtureAnchor`/`FixtureWorkingArea` (`:218`-`:220`).

**Reachability: A.**

**Arrange.** One `LifecycleFixture`-shaped harness with a manually pumped
`QueuedCreatorThreadSynchronizationContext`.

**Act/Assert.** Seven `Action`/`Func<Task>` invocations asserting
`ArgumentNullException` with parameter names `bridgeCoordinator`, `messenger`, `readiness`, `host`,
`anchorBounds`, `workingArea`, `focus`.

**Contract pinned.** In addition to the parameter names, assert that **no callback was enqueued** —
i.e. the fake context's queue is still empty after each throwing call. `ConfigureHost` (`:120`) and
`Focus` (`:167`) both post work to the dispatcher, so a guard placed *after* the post would still
throw but would leave a stale callback in the queue that fires against a half-configured
coordinator. The empty-queue assertion is what makes this a behavioural test rather than a coverage
artefact.

---

### G3 — static-helper argument guards (12 outcomes)

**Constructs.** `:343` (`BreadcrumbNavigationSubscription` ctor), `:362`/`:363`
(`CreateNavigationSurface`), `:388`/`:389` (`CreateCollapsedCandidate`), `:413`/`:414`
(`DisposeTwoResources`), `:441`/`:442`/`:443` (`NavigateWithSubscription`).

Note the asymmetry: `:388` and `:389` count **2** untaken outcomes each because their method has
never run at all, whereas the other eight are 1/2 with only the `throw` side outstanding. The
pass-through sides of `:388`/`:389` are closed for free by G4.

**Why untaken today.** `BreadcrumbPopupUiOperationsDirectAdapterTests` exercises three of the four
static methods but always with fully populated arguments. It does contain three
`NavigateToDocument` null-argument tests (`:137-188`) — but those target
`BreadcrumbPopupUiOperations` (F13's file), not `BreadcrumbPopupLifecycleOperations`. That is the
test that comes closest and stops short.

**Reachability: A.** All four methods are `internal static` on an `internal static` class in an
assembly that grants internals to `QuickFiler.Test`.

**Recommended shape.** One `[TestMethod]` per host method, each asserting two or three parameter
names, rather than one giant method — so a failure names the method under test.

**Contract pinned.** Parameter names plus, for `CreateNavigationSurface` (`:362`) specifically, that
a null `createMessenger` throws **without disposing** the caller-supplied `readiness`: the guard at
`:363` precedes the `try` at `:364`, so `readiness.Dispose()` at `:375` must not run. Assert
`readiness.Completion.IsCanceled == false` after the throw. That distinguishes a guard from a
mis-scoped `try` block, which is a real regression risk given that the method's whole purpose is
ownership transfer on failure.

---

### G4 — `CreateCollapsedCandidate` end-to-end (6 outcomes + 20 lines)

**Construct.** `:380`-`:409`. Creates a messenger, then a readiness lease, disposing the messenger
if the lease creation fails.

**Why untaken today.** Zero tests. §3.2 explains the likely cause: the one test whose *name* claims
to cover it (`CandidateFailure_CleansMessengerAndReadiness`,
`BreadcrumbItemViewerLifecycleCoordinatorTests.cs:51-74`) actually calls `CreateNavigationSurface`
at `:64`. The production caller needs a live `CoreWebView2`
(`ItemViewer.Breadcrumb.cs:82`), so nobody reached it that way either.

**Reachability: A.** The method takes two `Func<>` delegates and returns a `Tuple`. It touches no
WebView2 type and no UI. `BreadcrumbNavigationReadiness` is constructible directly —
`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:68` already does
`new BreadcrumbNavigationReadiness("Popup", () => detaches++)`.

**Four cases, one `[TestMethod]` each:**

| Case | Arrange | Closes |
| --- | --- | --- |
| a. success | `createMessenger` returns a fake messenger; `createReadiness` returns a real `BreadcrumbNavigationReadiness` | `:390` non-null side, `:397` non-null side, lines `:387`-`:394`, `:396`-`:402` |
| b. null messenger | `createMessenger` returns `null` | `:390` null side, `:391`-`:394` throw path |
| c. null readiness, messenger **not** `IDisposable` | `createMessenger` returns `new Mock<IWebViewMessenger>().Object`; `createReadiness` returns `null` | `:397` null side, `:404`-`:407`, `:406` **null** side (a Moq proxy implements only the mocked interface plus `IMocked`, so `as IDisposable` is `null`) |
| d. null readiness, messenger **is** `IDisposable` | a hand-written fake implementing both `IWebViewMessenger` and `IDisposable` | `:406` **non-null** side and `:409` |

**Contract pinned.** Case (a): the returned tuple's `Item1`/`Item2` are the exact instances the
factories produced — this is a pure ownership-transfer contract. Cases (c)/(d): the **messenger is
disposed exactly once** when readiness creation fails, and the `InvalidOperationException` message is
`"Collapsed navigation did not provide a readiness lease."`. Case (b): the message is
`"Collapsed navigation did not provide a messenger."` and **no disposal is attempted**, because
`messenger` was never assigned. The pair (b)/(c) together pin the method's real invariant: *the
messenger is owned by the caller until the readiness lease succeeds, and by this method afterwards*
— an asymmetry that is invisible without both cases.

---

### G5 — `CreateNavigationSurface` messenger outcomes (2 outcomes + 2 lines)

**Construct.** `:366`, `createMessenger() ?? throw`, reported 0/2. Lines `:371` and `:378` are
`hits="0"`.

**Why untaken today.** Both existing tests pass a factory that **throws** rather than returning
`null` (§3.2), so the exception escapes before either branch target is reached.

**Reachability: A.**

**Two cases:**
- **Success:** `createMessenger` returns a fake messenger. Closes the non-null side of `:366` plus
  lines `:371` and `:378`.
- **Null:** `createMessenger` returns `null`. Closes the null side of `:366`. `:251`'s existing
  2/2 is the positive control that this outcome does register.

**Contract pinned.** Success case: the returned tuple's `Item2` is **`readiness.Completion`**
specifically — i.e. the method hands back the lease's completion task, not a fresh task — and
`readiness` is **not** disposed. Null case: the message is
`"Popup navigation did not provide a messenger."` **and `readiness.Completion.IsCanceled` is true**,
because the catch at `:374`-`:377` disposes the lease. The pair pins the ownership rule: *a lease
handed in is released on every failure path and retained on success* — the same rule as G4 but with
the opposite parameter, which is why both are needed.

---

### G6 — `DisposeTwoResources` clean path (1 outcome + 1 line)

**Construct.** `:428`, `if (failure != null)`; false side untaken; `:432` `hits="0"`.

**Why untaken today.** `TwoResourceCleanup_ReportsFirstFailureAfterAllCleanup`
(`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:114-135`) makes **both** actions throw. There is
no clean-path test at all.

**Reachability: A.**

**Arrange/Act.** Two recording actions that both succeed; then, as a second `[DataRow]`-style case,
one that succeeds and one that throws.

**Contract pinned.** Clean case: **both** actions ran, in declaration order (`disposeMessenger`
then `disposeControl`), and no exception escaped. Mixed case: the surviving exception is the one
thrown, and the *other* action still ran — pinning "cleanup is best-effort across both resources,
and the first failure is the one reported", which is exactly the invariant `failure ??= exception`
(`:424`) encodes.

---

### G7 — `NavigateWithSubscription` with a null subscription (3 outcomes + 4 lines)

**Construct.** `:463` `if (subscription == null)`, plus the two `subscription?.Dispose()` sites at
`:450` and `:475`.

**Why untaken today.** `RecordingNavigationBinding.Create`
(`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:253-264`) unconditionally returns
`new BreadcrumbNavigationSubscription(() => DetachCount++)`. No factory in the suite returns `null`.

**Reachability: A.** `NavigationSubscriptionFactory` (`:330`) is a plain delegate; a test can supply
`(_, __, ___) => null`.

**Key insight — one test closes all three outcomes and all four lines.** With a null-returning
factory the flow is: `:463` true → `:465`-`:467` throw → catch at `:473` → `:475`
`subscription?.Dispose()` with `subscription == null` (null side) → `:476` `readiness.Dispose()`,
which invokes the detach callback created at `:450`, also with `subscription == null` (null side) →
`:477` rethrow.

**Assert.** `InvalidOperationException` with message
`"Popup navigation did not provide an event subscription."`, and
`readiness.Completion.IsCanceled == true`.

**Contract pinned.** *A factory that yields no subscription still releases the readiness lease.*
This is the one failure mode in this method where there is nothing to unsubscribe, and it is
precisely the case where a naive implementation would leak the lease. Asserting cancellation of the
completion task — rather than merely that an exception was thrown — is what makes it a contract
test.

---

### G8 — owner-disposed callback (0 outcomes, 1 line)

**Construct.** `:461`, `() => _ = dispatcher.Dispatch(readiness.Cancel)`, the third argument to
`createSubscription`.

**Why untaken today.** `RecordingNavigationBinding` **already implements**
`internal void OwnerDisposed() => _ownerDisposed();` at
`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:271`, and **no test calls it**. The helper was
written and then never used — the closest anything comes to reaching this line.

**Reachability: A.** Call the existing helper.

**Arrange/Act.** Drive `NavigateWithSubscription` to success, then `binding.OwnerDisposed()`, then
drain.

**Assert.** `readiness.Completion.IsCanceled == true` and the detach ran exactly once.

**Contract pinned.** *Owner disposal cancels an in-flight navigation rather than leaving its
completion task pending forever.* A pending, never-completed lease is the failure this callback
exists to prevent, and nothing currently proves it works.

---

### G9 — `BreadcrumbNavigationSubscription` double-dispose (1 outcome)

**Construct.** `:349`, `detach?.Invoke()`; the null side is untaken.

**Why untaken today.** No test disposes a subscription twice. The suite disposes each subscription
at most once, through `readiness.Dispose()`.

**Reachability: A.**

**Arrange/Act.** `var s = new BreadcrumbNavigationSubscription(() => count++); s.Dispose(); s.Dispose();`

**Assert.** `count == 1`.

**Contract pinned.** *Disposal is idempotent and the detach action runs exactly once*, which is the
sole reason `Interlocked.Exchange` is used at `:348` rather than a plain field read. Without this
test the `Interlocked` is unjustified by any assertion.

---

### G10 — coordinator lifecycle and null collaborators (8 outcomes + 2 lines)

Five sub-tasks. All but (d) are Reach **A**.

**(a) `:158`, `:159` — `SetTheme` with neither bridge nor host (2 outcomes).**
Untaken: both null sides. Reach: construct the coordinator and call `SetTheme("dark")` immediately,
before any `SetBridgeCoordinator` or `ConfigureHost`.
*Contract:* theming a not-yet-wired coordinator is a silent no-op, not a `NullReferenceException`.
This is directly load-bearing for **#488 Defect 2**, which is about `SetTheme` racing the deferred
`ConfigureHost` post; the null-tolerance is the behaviour that turns that race into "theme silently
lost" rather than "crash", and pinning it prevents an unrelated refactor from converting a silent
loss into a production exception.

**(b) `:56` — `CurrentOpenTask` with no host configured (2 outcomes).**
Untaken: both `_openCoordinator == null` sides. Reach: read `CurrentOpenTask` on a fresh
coordinator, then again after `ConfigureHost` + drain.
*Contract:* the property is **never null** and returns an **already-completed `false`** when no
drop-down exists, so callers may `await` it unconditionally. `ItemViewer.BreadcrumbOpenTask`
(`ItemViewer.Breadcrumb.cs:29-30`) and `BreadcrumbSelectorOpenRetryTests.cs:38` both rely on this;
assert `task.IsCompleted && task.Result == false`.

**(c) `:122` and `:319`, plus lines `:123`, `:124` — stale-generation and post-disposal posts
(2 outcomes + 2 lines).**
Two tests:
- *Stale generation:* `ConfigureHost(host, …)` → `Reset()` → drain. `Reset()` bumps `_generation`
  (`:194`), so the queued lambda's `IsCurrent(generation)` is false at `:122` and returns at `:124`.
  Assert the host's `PopupMessengerReady` was **never** subscribed (`RecordingHost.EventOperations`
  stays empty — the same recording mechanism used at
  `BreadcrumbItemViewerLifecycleCoordinatorTests.cs:38-39`).
- *Post-disposal:* `ConfigureHost(host, …)` → `Dispose()` → drain. `_disposed` is true, so `:319`'s
  first operand short-circuits.
*Contract:* **work queued before a reset or a disposal is discarded, not executed against stale
state.** This is the generation guard's entire purpose and today nothing proves the `_disposed` half
of it. `ResetDispose_LateCallbackDoesNotReattach` (`:77-96`) proves the `Reset` half only for
`OnPopupMessengerReady`, not for `ConfigureHost`.

**(d) `:234` — popup-ready callback after the host is gone (1 outcome). Reach: R.**
Untaken: `DropDownHost == null`. **Proof of API-unreachability:** `_openCoordinator` is assigned
non-null at `:130` and nulled only at `:303`, inside `ReleaseHostCore`. `ReleaseHostCore` has
exactly two callers: `ConfigureHost`'s lambda at `:129`, which reassigns at `:130` in the same
single-threaded step; and `Dispose()` at `:211`, which has already set `_disposed = true` at `:208`,
so the lambda exits at `:231` before reaching `:234`. There is no interleaving point.
*Recommended closure:* after `ConfigureHost` + drain, raise `PopupMessengerReady` (enqueuing the
lambda), then set the **own-file** private field `_openCoordinator` to `null` by reflection, then
drain. Assert no attach occurred. This uses only `BreadcrumbItemViewerLifecycleCoordinator`'s own
private state, so it creates no cross-child coupling.
*Contract:* a popup-ready notification that arrives after host release attaches nothing.
*Fallback:* leave it untaken and record a documented deviation (§6 shows the branch figure still
clears the floor by more than 22 points).

---

### G11 — host-delegate lambdas with no bridge coordinator (3 outcomes)

**(a) `:137` — `_openSelector` with a null bridge (1 outcome). Reach: A.**
Arrange: `ConfigureHost(host, …)` on a fixture that never calls `SetBridgeCoordinator`; drain; then
`SetDroppedDown(true, focus)`; drain. The posted lambda at
`BreadcrumbDropDownOpenCoordinator.cs:104-116` calls `_openSelector()` with no gate.
*Contract:* dropping down a breadcrumb with no bridge attached is a no-op that neither throws nor
opens the popup. Assert the host's `OpenAsync` was never called.

**(b) `:135` and `:138` — `_rowCount` and `_cancelSelector` with a null bridge (2 outcomes).
Reach: R.**
**Proof of API-unreachability** (§3.2): every call site of `_rowCount`
(`BreadcrumbDropDownOpenCoordinator.cs:193`) and of `_cancelSelector` (`:144`, `:207`, `:226`,
`:265`) is gated, directly or transitively, on `_isSelectorOpen()` returning `true`; with a null
bridge that lambda returns `false`.
*Recommended closure:* read the own private field `_openCoordinator`, then read
`BreadcrumbDropDownOpenCoordinator`'s private `_rowCount` / `_cancelSelector` fields by reflection
and invoke them directly.
*Contract:* with no bridge attached the popup-sizing provider reports **zero rows** (not a throw,
which would abort `BeginOpenCore` mid-open) and the cancel provider is a silent no-op.
*Risk:* this creates a **cross-child reflection coupling** onto F13-owned private field names.
F13's spec commits to no public/internal signature changes but says nothing about private fields.
*Fallback:* record as a documented deviation.

---

## 5. Projected result

| Axis | Before | After (46 reachable only) | After (all 49) | Floor |
| --- | --- | --- | --- | --- |
| Line | 288/318 = **90.57%** | **318/318 = 100.00%** | 318/318 = 100.00% | >= 80% |
| Branch | 97/146 = **66.44%** | 143/146 = **97.95%** | 146/146 = 100.00% | >= 75% |

**Line coverage reaches 100% on the reachable set alone**, because all 30 uncovered lines are
attached to reachable gaps: `:123`-`:124` (G10-c), `:371`/`:378` (G5), `:387`-`:409` — 20 lines —
(G3/G4), `:432` (G6), `:461` (G8), `:464`-`:467` (G7). None of the three reflection-only outcomes
carries an uncovered line.

**Branch coverage clears the 75% floor by 22.95 points even if all three reflection-only outcomes
are waived**, so no gap in this file is a merge blocker.

---

## 6. Production edit verdict

**No production edit to `BreadcrumbItemViewerLifecycleCoordinator.cs` is required or recommended.**

- All 46 API-reachable outcomes are closable through the `internal` surface, which
  `AssemblyInfo.cs:5` already exposes to `QuickFiler.Test`.
- The 3 remaining outcomes are closable by reflection over existing private members; extracting a
  seam for them would add production surface for two defensive null-conditionals and one
  unreachable-by-construction property read.
- The **19 lines of headroom (481/500) are preserved.** Any seam extraction would consume them
  quickly, and a split would trigger a `QuickFiler/QuickFiler.csproj` `<Compile Include>` edit, a
  ledger row per the epic's "Mid-Wave File Creation" rules, and a >= 90% target on the new file.
- **The #457 measurement trap does not apply.** No `[ExcludeFromCodeCoverage]` is introduced at
  either level, so there is no lifted-lambda leak to reason about. Recorded for completeness: were a
  thin-forwarder adapter ever required here, it would have to be a **type-level**-exempt adapter,
  `sealed` and **not `partial`** (epic § "Measurement Trap", § "fourth exemption ground" condition
  4). Note that this file contains **six lambdas** in `ConfigureHost` alone (`:120`, `:135`-`:139`)
  plus three more in `NavigateWithSubscription` (`:450`, `:455`-`:461`), so a method-level attribute
  anywhere in it would leak a large closure surface into the denominator.
- **No `QuickFiler/QuickFiler.csproj` edit is needed for this file.**

**Rejected alternatives, for the record:**

1. *Extract the five `ConfigureHost` delegates into named private methods* so `:135`-`:138` become
   directly invocable. Behaviour-preserving, and it would close G11 without reflection — but it adds
   roughly 20 lines to a file with 19 lines of headroom, forcing a split, and the gain is 2 branch
   outcomes worth 1.4 points on a metric already at 97.95%.
2. *Make `SetDroppedDown`'s `focus` parameter mandatory and validate it unconditionally.* Would
   remove the LD-2 inconsistency but is a behaviour change (see §8), which the epic NFR forbids.
3. *Split the file's three types into three files.* Correct on cohesion grounds, wrong for this
   epic: it invalidates the per-file baseline mid-wave, adds three ledger rows, and raises the bar
   on the extracted files from 80% to 90%.

---

## 7. Retain-or-improve risk analysis

This file is at 90.57% line. **Only three test files name any of its three types** —
`BreadcrumbItemViewerLifecycleCoordinatorTests.cs` (4 references),
`BreadcrumbPopupUiOperationsDirectAdapterTests.cs` (7), and
`BreadcrumbCollapsedSurfaceReadinessTests.cs` (1). But a substantial share of the covered lines is
load-bearing on tests that never mention the types at all, reaching them through **F14's
`ItemViewer`**. The full load-bearing surface is **nine files**:

| Test file | How it reaches this file | Primary target |
| --- | --- | --- |
| `Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | direct construction (`:229`) | **F12 (this file)** |
| `Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs` | direct static calls (`:71`, `:88`, `:120`, `:208`) | F13 + this file |
| `Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | `Viewer.InitializeBreadcrumbPipeline` (`:415`), `AttachBreadcrumbMessengerWhenReadyAsync` (`:438`), `ResetBreadcrumb` (`:251`, `:268`) | F13 |
| `Viewers/BreadcrumbDropDownIntegrationTests.cs` | `InitializeBreadcrumbPipeline` (`:340`), `SetTheme` (`:151-152`), `ResetBreadcrumb` (`:237`) | F13 |
| `Viewers/BreadcrumbSelectorOpenRetryTests.cs` | `InitializeBreadcrumbPipeline` (`:260`), `AttachBreadcrumbMessenger` (`:265`), `BreadcrumbOpenTask` (`:38`, `:41`, `:61`, `:69`) | F13 |
| `Viewers/BreadcrumbPendingOpenCloseTests.cs` | `InitializeBreadcrumbPipeline` (`:163`) | F13 |
| `Viewers/BreadcrumbSubfolderActivationTests.cs` | `InitializeBreadcrumbPipeline` (`:306`), `AttachBreadcrumbMessenger` (`:340`) | F12 (bridge) |
| `Viewers/BreadcrumbCoordinatorLifecycleTests.cs` | `InitializeBreadcrumbPipeline` (`:122`), `AttachBreadcrumbMessenger` (`:123`), `ResetBreadcrumb` (`:127`) | F12 (bridge) |
| `Viewers/BreadcrumbDropDownReadinessTests.cs` | `Coordinator.SetTheme` (`:366`) | F13 |

### R1 (highest) — five branch sides exist **only** because F13-targeted tests drive a real `ItemViewer`

The already-taken sides of `:56`, `:135`, `:137`, `:158`, and `:159` all require a **non-null**
`_bridgeCoordinator` and/or a **non-null** `DropDownHost`. Every arrangement producing that state
today comes from an `ItemViewer`-driven test in the table above, five of which primarily target
**F13-owned** production files. If F13 replaces any of those harnesses with a direct-unit harness
against `BreadcrumbDropDownHost` or `BreadcrumbPopupUiOperations` — a natural move for a
coverage-focused child — those five branch sides revert to untaken and this file's branch figure
falls, even though F12 changed nothing.

*Mitigation for the plan:* an explicit AC that `BreadcrumbDropDownIntegrationTests.cs`,
`BreadcrumbSelectorOpenRetryTests.cs`, and `BreadcrumbPendingOpenCloseTests.cs` continue to
initialise a real breadcrumb pipeline on a real `ItemViewer`, plus a post-merge re-measure. Also
note that F12's own new tests close the *other* side of each of those five branches, so after F12
merges the file is resilient on four of the five regardless of what F13 does.

### R2 — the brief's implicit test inventory is three files; the real inventory is nine

Six of the nine load-bearing files are not discoverable by grepping for any type declared in this
file. Any retain-or-improve analysis limited to the three files that name the types is incomplete —
the same class of error the sibling artifact recorded as its R2.

### R3 — F13 owns every deterministic vehicle this file's tests must use

`BreadcrumbPopupUiOperations.PostAsync` and `CreateDispatchedReadiness`
(`BreadcrumbPopupUiOperations.cs`) and `BreadcrumbUiDispatcher` (`BreadcrumbUiDispatcher.cs`) are
both **F13-owned** and are on the critical path of every existing and proposed test. F13's spec
(`.../455/spec.md:49-50`) commits to **no public or internal signature changes** to its fifteen
files and names F12 as a dependent. That commitment is F12's protection and should be cited
verbatim in F12's plan.

### R4 — the G11 reflection proposal couples F12 tests to F13 private field names

`BreadcrumbDropDownOpenCoordinator._rowCount` (`:19`) and `_cancelSelector` (`:22`) are private
readonly fields. Renaming them breaks the proposed tests **at runtime, not at compile time**. F13's
signature-stability commitment does not extend to private members. Either accept the coupling with
an explanatory comment naming the risk, or take the documented-deviation fallback.

### R5 — an F12 test constructs an F14-owned form-derived type

`BreadcrumbCoordinatorLifecycleTests.ViewerScope` constructs `new QuickFiler.ItemViewer()` and
drives `InitializeBreadcrumbPipeline` / `AttachBreadcrumbMessenger` / `ResetBreadcrumb`
(`:122-127`). `ItemViewer` is **F14-owned**. This is pre-existing and out of F12's scope to change,
but F14 must not break it unknowingly, and it interacts with the epic's DEC-1 ruling on unshown Form
construction and with open issue **#491** (`quickfiler-test-form1-live-form`). None of F12's
proposed new tests constructs a form.

### R6 — `ItemViewer.Breadcrumb.cs` is under concurrent pressure from three directions

It is F14-owned, it is the subject of open issue **#488**, and the epic records that **#400**'s live
remediation plan also authorises edits to it (epic § "Known Conflict Risks"). Any of the three could
change how the six indirect test files drive this file.

---

## 8. Test-file plan

### 8.1 Headroom against the 500-line test-file limit

| File | Lines | `[TestMethod]` | Headroom |
| --- | --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 327 | 10 | 173 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs` | 302 | 9 | 198 |

### 8.2 Recommendation — two new standalone `[TestClass]` files, no `.Part2.cs`

**Create both of:**

1. `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs` — the coordinator
   type. Gaps G1, G2, G10, G11. Estimated 11 `[TestMethod]`, ~280-310 lines including a private
   fixture and a private `QueuedCreatorThreadSynchronizationContext`.
2. `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs` — the static helper and the
   subscription type. Gaps G3, G4, G5, G6, G7, G8, G9. Estimated 13 `[TestMethod]`, ~250-290 lines.

**Why two files rather than one.** A single file would land at roughly 550 lines, breaching the
500-line limit in `.claude/rules/general-code-change.md`. The split is also natural: the two halves
share no fixture — the coordinator tests need a pumped `SynchronizationContext`, a
`BreadcrumbMessengerHub`, and a `BreadcrumbCollapsedAttachment`, while ten of the thirteen static
helper tests need nothing but two delegates.

**Why standalone classes rather than `.Part2.cs` companions.** Both existing classes are declared
`public sealed class` (**not `partial`**) at
`BreadcrumbItemViewerLifecycleCoordinatorTests.cs:15` and
`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:15`. A companion partial would require editing
those declarations — a fan-in conflict surface that F13 and F14 also touch, since both files sit in
`QuickFiler.Test/Viewers/`. The `.Part2.cs` pattern does exist in the repo
(`QuickFiler.Test.csproj:82`, `:85`) and is available if a reviewer prefers it; it is simply not
needed and carries avoidable conflict cost here.

### 8.3 Gap-to-test mapping

| Test class | Test method | Closes |
| --- | --- | --- |
| Guard | `Constructor_NullArgument_ThrowsForTheExpectedParameterInDeclarationOrder` | `:38 :39 :41 :42 :43 :44` |
| Guard | `SetBridgeCoordinator_Null_ThrowsBeforeAnySubscription` | `:65` |
| Guard | `AttachCollapsedWithReadinessAsync_NullArguments_ThrowWithParameterNames` | `:93 :94` |
| Guard | `ConfigureHost_NullArguments_ThrowBeforeAnythingIsPosted` | `:115 :116 :117` |
| Guard | `Focus_NullAction_ThrowsBeforeAnythingIsPosted` | `:165` |
| Guard | `SetTheme_WithNoBridgeAndNoHost_IsASilentNoOp` | `:158 :159` |
| Guard | `CurrentOpenTask_WithNoHost_IsAlreadyCompletedFalse` | `:56` |
| Guard | `ConfigureHost_ThenReset_DiscardsTheQueuedHostConfiguration` | `:122` + lines `:123 :124` |
| Guard | `ConfigureHost_ThenDispose_DiscardsTheQueuedHostConfiguration` | `:319` |
| Guard | `SelectorOpenStateChanged_WithNoHost_IsIgnored` | `:222` |
| Guard | `SetDroppedDown_WithHostButNoBridge_DoesNotOpenThePopup` | `:137` |
| Guard *(reflection)* | `PopupMessengerReady_AfterHostRelease_AttachesNothing` | `:234` |
| Guard *(reflection)* | `HostProviders_WithNoBridge_ReportZeroRowsAndNoOpCancel` | `:135 :138` |
| Ops | `NavigationSubscription_NullDetach_Throws` | `:343` |
| Ops | `NavigationSubscription_DisposedTwice_InvokesDetachOnce` | `:349` |
| Ops | `CreateNavigationSurface_NullArguments_ThrowWithoutDisposingTheLease` | `:362 :363` |
| Ops | `CreateNavigationSurface_Success_ReturnsTheLeaseCompletionAndRetainsIt` | `:366` (one side) + lines `:371 :378` |
| Ops | `CreateNavigationSurface_NullMessenger_DisposesTheLease` | `:366` (other side) |
| Ops | `CreateCollapsedCandidate_NullArguments_ThrowWithParameterNames` | `:388 :389` (throw sides) |
| Ops | `CreateCollapsedCandidate_Success_ReturnsBothFactoryResults` | `:390 :397` (non-null sides) + lines `:387`-`:402` |
| Ops | `CreateCollapsedCandidate_NullMessenger_ThrowsWithoutDisposing` | `:390` (null side) |
| Ops | `CreateCollapsedCandidate_NullReadiness_DisposesADisposableMessenger` | `:397` (null side), `:406` (non-null side), `:409` |
| Ops | `CreateCollapsedCandidate_NullReadiness_ToleratesANonDisposableMessenger` | `:406` (null side), `:404`-`:405` |
| Ops | `DisposeTwoResources_NullArguments_ThrowWithParameterNames` | `:413 :414` |
| Ops | `DisposeTwoResources_BothSucceed_RunsBothAndThrowsNothing` | `:428` + line `:432` |
| Ops | `NavigateWithSubscription_NullArguments_ThrowWithParameterNames` | `:441 :442 :443` |
| Ops | `NavigateWithSubscription_NullSubscription_ThrowsAndCancelsTheLease` | `:450 :463 :475` + lines `:464`-`:467` |
| Ops | `NavigateWithSubscription_OwnerDisposed_CancelsTheLease` | line `:461` |

28 `[TestMethod]` declarations across the two files.

### 8.4 csproj registration

`QuickFiler.Test/QuickFiler.Test.csproj` is a non-SDK project with **explicit `<Compile Include>`
entries and no globbing**. The breadcrumb block runs from line 58 to line 91; line 64 is
`BreadcrumbItemViewerLifecycleCoordinatorTests.cs` and line 65 is
`BreadcrumbPopupUiOperationsDirectAdapterTests.cs`.

Insert exactly these two lines **immediately after line 64**, preserving the file's 4-space
indentation:

```
    <Compile Include="Viewers\BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs" />
    <Compile Include="Viewers\BreadcrumbPopupLifecycleOperationsTests.cs" />
```

**Preserve CRLF — use the Edit tool, never a git-bash `sed -i`** (epic § "Cross-Child Constraints"
1b). Own entries only; no property, reference, or ordering changes. Additive fan-in conflicts on
this file are expected and are resolved by keeping both sides.

**No `QuickFiler/QuickFiler.csproj` edit is required**, since no production file is created.

### 8.5 Determinism contract for every new test

- **Framework:** MSTest `[TestClass]` / `[TestMethod]` / `[DataTestMethod]`, Moq for stubs,
  FluentAssertions for assertions, explicit Arrange / Act / Assert sections.
- **Scheduler vehicle:** a private nested `QueuedCreatorThreadSynchronizationContext` replicating
  `BreadcrumbPopupUiOperationsDirectAdapterTests.cs:274-300`, driven with
  `DrainOnCreatorThread()`. Where no post is involved,
  `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` (`BreadcrumbUiDispatcher.cs:62-65`) runs
  inline.
- **Async edges:** driven only by an explicit `Drain()` or by a test-owned
  `TaskCompletionSource` / the `RecordingNavigationBinding` push pattern. No polling.
- **Ambient context:** none of the proposed tests needs one. If one becomes unavoidable, install and
  restore it in a `try`/`finally`.
- **Prohibited and absent from every new test:** `Thread.Sleep`, `Task.Delay`, any wall-clock wait,
  any real-time polling, temporary files, any filesystem write, external services or processes, the
  WebView2 Evergreen runtime, live or shown forms, popups, `STAThread` attributes or
  `*.StaTests.cs` files, injected clocks, `TimeProvider`, and `FakeTimeProvider`.
- **500-line limit:** both new files stay well inside it; neither existing test file is modified, so
  neither can breach it.

---

## 9. Latent defects — verified, assessed, NOT fixed

All are out of scope under the epic's no-behaviour-change NFR. **The orchestrator promotes; this
artifact does not.** Each was cross-checked against open issues **#488, #491, #475, #462, #440,
#458, #476**.

### LD-1 — `SetBridgeCoordinator` replaces without disposing the previous coordinator

**Severity: Low. Recommend a GitHub issue (or an explicit amendment to #488).**

Verified chain:

1. `:66-69` — same-reference calls short-circuit.
2. `:71-72` — a *different* coordinator triggers `UnsubscribeBridge()` and then overwrites
   `_bridgeCoordinator`. The previous instance is **never disposed**.
3. `:216` — `Dispose()` *does* dispose `_bridgeCoordinator`, so the type treats the coordinator as
   owned at teardown but not at replacement. The ownership model is internally inconsistent.
4. The orphaned coordinator retains its `BreadcrumbCoordinatorUpgradeLifetime` and its
   `_messenger.MessageReceived` subscription on the shared `BreadcrumbMessengerHub` —
   `BreadcrumbBridgeCoordinator.Dispose()` (`BreadcrumbBridgeCoordinator.cs:163-172`) is the only
   thing that removes that subscription, and it never runs.

Not reachable today: the sole production caller
(`ItemViewer.InitializeBreadcrumbPipeline`, `ItemViewer.Breadcrumb.cs:53-59`) is guarded by
`if (BreadcrumbCoordinator != null) return;` at `:45-48`. **But that guard is exactly what #488
Defect 3 proposes to change**, so a fix to #488 would make this leak live. Cross-reference is
essential.

Distinct from all five #488 defects, which are all sited in `ItemViewer.Breadcrumb.cs`.

### LD-2 — `Reset()` tears down the collapsed surface synchronously and the popup surface asynchronously

**Severity: Low-Medium. Recommend a GitHub issue.**

`Reset()` (`:191-199`) detaches the collapsed messenger **synchronously** at `:197` and resets the
collapsed attachment at `:198`, but the popup messenger is detached only inside
`_openCoordinator.Reset()`'s **posted** lambda, at
`BreadcrumbDropDownOpenCoordinator.cs:138-147` (`_detachPopupMessenger()` at `:145`). Between
`Reset()` returning and the next dispatcher pump, the expanded surface's `MessageReceived` is still
wired into the hub and thence into the just-reset bridge coordinator.

On the UI thread `PostAsync` runs inline and the window closes immediately. Off the UI thread — or
after any `ConfigureAwait(false)` resumption, which #488 Defect 2 documents as a real scenario
citing `BreadcrumbUiDispatcher.cs:263-268` — the window is genuine.

This is the **same class** as #488 Defect 2 but a **different site in a different file** (#488
Defect 2 concerns `SetTheme` reading `DropDownHost`). The orchestrator may prefer to fold it into
#488 rather than open a new issue.

### LD-3 — `SetDroppedDown` validates and honours `focus` inconsistently

**Severity: Low. Recommend recording; promotion optional.**

`:176-189`. Three inconsistencies in nine lines:

- When `_openCoordinator == null` and `droppedDown == true`, `focus` flows to `Focus(focus)`
  (`:183`), which throws `ArgumentNullException` at `:165`.
- When `_openCoordinator == null` and `droppedDown == false`, a null `focus` is silently accepted.
- When `_openCoordinator != null`, `focus` is **discarded entirely** (`:188` passes only
  `droppedDown`), so a null is silently accepted and a non-null is silently ignored. A caller
  reasonably expects focus to follow a programmatic drop-down.

The sole production caller, `ItemViewer.SetBreadcrumbDropDownState`
(`ItemViewer.Breadcrumb.cs:223-235`), always passes the non-null method group
`FocusBreadcrumbCore`, so no production symptom exists. Fixing it changes observable argument
validation, which the NFR forbids.

### LD-4 — hub attachments have three concurrent owners disambiguated only by a dictionary

**Severity: Low (design). Do NOT open an issue — no leak is demonstrable.**

The same `IWebViewMessenger` instance can be tracked by `_collapsedMessenger` (`:24`),
`_popupMessenger` (`:25`), and `BreadcrumbCollapsedAttachment._readyMessenger`
(`BreadcrumbMessengerHub.cs:284`). `AttachMessenger` (`:264-267`) only records the messenger in its
slot when `_hub.Attach` returns `true`, and the hub returns `false` for an already-attached
messenger (`BreadcrumbMessengerHub.cs:74-77`) — so the second owner never takes the slot.

All four orderings were traced and in each one some owner performs the detach:
`DetachCollapsedMessenger` (`:270-279`), `DetachPopupMessenger` (`:281-290`) via
`BreadcrumbDropDownOpenCoordinator.cs:145`/`:156`, or
`BreadcrumbCollapsedAttachment.Release` (`BreadcrumbMessengerHub.cs:412-413`). **No leak is
demonstrable on current code.** Recorded because the invariant is implicit and a change to the hub's
`Attach` return semantics would silently break it.

Related: `:252-256` calls `_hub.Attach(messenger, mode)` for an already-attached messenger and
discards the result, which is by construction always `false`. The call is a deliberate mode
re-assertion (hub doc comment, `BreadcrumbMessengerHub.cs:60-63`) but reads as a no-op.

### LD-5 — properties are not disposal-guarded

**Severity: Low (design). Do NOT open an issue.**

Every method calls `ThrowIfDisposed()` (`:64`, `:83`, `:92`, `:100`, `:114`, `:157`, `:164`, `:178`,
`:193`) but none of the five properties does (`:51`, `:53`, `:55`, `:58`, `:60`). After `Dispose()`
the `Hub` property (`:60`) still hands out the hub that `Dispose()` disposed at `:215`; any
subsequent `Attach` or `PostJson` on it throws `ObjectDisposedException` naming
`BreadcrumbMessengerHub`, not the coordinator the caller actually disposed. Reachable in tests
today: `LifecycleFixture` exposes `Hub` as a property
(`BreadcrumbItemViewerLifecycleCoordinatorTests.cs:244`) and uses it in `CreateBridge()` (`:251`).

### LD-6 — the coordinator disposes two collaborators it did not create

**Severity: Low (design, informational). Do NOT open an issue.**

`Dispose()` disposes `_collapsedAttachment` (`:214`) and `_hub` (`:215`), both
constructor-injected. `ItemViewer.EnsureBreadcrumbLifecycle` (`ItemViewer.Breadcrumb.cs:263-275`)
creates both and hands them over, so the intent is clear — but it is not expressed in the API, and
`LifecycleFixture` (`BreadcrumbItemViewerLifecycleCoordinatorTests.cs:228`) retains a reference to
the hub it passes in and then has it disposed underneath it at `:254`.

### Test-quality observations (in scope for F12's own execution, not defects)

- `BreadcrumbItemViewerLifecycleCoordinatorTests.CandidateFailure_CleansMessengerAndReadiness`
  (`:51-74`) is misnamed: it exercises `CreateNavigationSurface` (`:64`), not
  `CreateCollapsedCandidate`. It is also a near-duplicate of
  `BreadcrumbPopupUiOperationsDirectAdapterTests.MessengerConstructionFailure_DisposesReadiness`
  (`:64-79`). The misleading name is the most plausible explanation for
  `CreateCollapsedCandidate` sitting at 0% coverage. **Recommend renaming it in F12's execution**
  (a test-only change, in scope).
- `RecordingNavigationBinding.OwnerDisposed()`
  (`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:271`) is a dead test helper. G8 puts it to use.
- `QueuedCreatorThreadSynchronizationContext` is duplicated verbatim across two test files
  (`BreadcrumbItemViewerLifecycleCoordinatorTests.cs:299-325` and
  `BreadcrumbPopupUiOperationsDirectAdapterTests.cs:274-300`). F12's two new files will make four
  copies. Promoting it to a shared `internal` test helper would be an improvement but touches two
  existing files owned by two children; **not recommended for F12**.

### Cross-check against the named open issues

| Issue | Overlap with this file | Verdict |
| --- | --- | --- |
| **#488** `itemviewer-breadcrumb-pipeline-lifecycle` | Defect 1 cites `:127-142` and `:300-303`; Defect 2 cites `:155-160`; Defect 3 cites `:66-69`; Defect 4 cites `:73-76`; Defect 5 cites `:120` and `:122-125` | **Heavy overlap by citation, none by remedy.** All five defects are *sited in* `ItemViewer.Breadcrumb.cs`; this file is cited as the downstream mechanism. LD-1 and LD-2 are new, and LD-1 interacts with Defect 3 (see below) |
| #475 `breadcrumb-capturecurrentortests-silently-degrades` | `BreadcrumbPopupUiOperations.CaptureCurrentOrTests` (F13), reached from `ItemViewer.Breadcrumb.cs:156,:192` | no overlap |
| #462 `breadcrumb-dropdown-coordinator-stale-closepending` | `BreadcrumbDropDownOpenCoordinator` (F13) | no overlap, but G11's reflection proposal touches the same type |
| #440 `breadcrumb-left-right-arrow-parent-child-navigation` | bridge/router | no overlap |
| #458 `webview2breadcrumbhost-handler-retention-pooled-viewer` | F13 | no overlap |
| #476 `webview2breadcrumbhost-unmarshalled-sdk-call` | F13 | no overlap |
| #491 `quickfiler-test-form1-live-form` | test infrastructure | relevant to **R5** only |

> **Correction to #488 Defect 1, for the issue owner.** Defect 1 states that
> `BreadcrumbItemViewerLifecycleCoordinator.cs:127-142` "calls `ReleaseHostCore()`, which
> unsubscribes `PopupMessengerReady` and calls `coordinator.Release()` (`:300-303`), but does not
> call `IBreadcrumbDropDownHost.Dispose()`." `ReleaseHostCore` indeed does not call `Dispose`
> **directly** — but `coordinator.Release()` at `:302` reaches
> `BreadcrumbDropDownOpenCoordinator.Release` (`BreadcrumbDropDownOpenCoordinator.cs:150-159`),
> whose posted lambda calls `_host.Dispose()` at `:157`. **The previous host is disposed on
> replacement**, asynchronously, on the next dispatcher pump. The residual risk is narrower than
> stated: the disposal is *enqueued*, so it is lost only if the dispatcher never pumps again. This
> does not invalidate the issue, but it changes both its mechanism and its severity, and should be
> corrected before #488 is scheduled.

---

## 10. Corrections to the brief, the spec, and the epic

### Confirmed as stated

1. **The coverage table is exact.** 318 coverable lines, 90.6% line, 66.4% branch, 146 branch
   points — all four reproduce to the stated precision from the class-level `<lines>` block. This is
   the first sibling brief in the epic to survive re-measurement without a numeric correction.
2. **The implied untaken count is exact.** 146 − 97 = **49**, matching the brief's "roughly 49".
3. **This file fails the branch gate and passes the line gate**, exactly as the spec asserts, and it
   is the largest single branch gap in the epic.
4. No `[ExcludeFromCodeCoverage]` attribute anywhere in the file, so there is no exemption
   disposition work (`spec.md:32-33`).
5. `QuickFiler.Test.csproj` is non-SDK with explicit `<Compile Include>` entries and no globbing.
6. The `AssemblyInfo.cs:5` `InternalsVisibleTo("QuickFiler.Test")` grant.
7. **Line-number drift: none.** Every reported line resolves to the predicted construct.
8. Exactly one `<class>` element carries this filename — no cross-class union needed. (Recorded so
   F1's harness authors do not read the absence of a `<>c` element as a bug.)

### Corrections

1. **The file declares three types and a delegate, not one.** `BreadcrumbItemViewerLifecycleCoordinator`
   (`:13`), `NavigationSubscriptionFactory` (`:330`), `BreadcrumbNavigationSubscription` (`:337`),
   and `BreadcrumbPopupLifecycleOperations` (`:355`). **30 of the 49 untaken outcomes and 28 of the
   30 uncovered lines live in the latter two types**, not in the coordinator. This is the single
   most consequential scoping correction: a plan written against "the lifecycle coordinator" will
   under-scope by more than half.
2. **`CreateCollapsedCandidate` (`:380`-`:409`) is 0% covered end-to-end** — 20 uncovered lines and
   10 untaken outcomes in one 30-line method, the largest single contiguous gap in the file. No
   brief, spec, or epic document mentions it.
3. **"Use an injected clock and fake timers" (`spec.md:69-70`, `:112`) is wrong for this file and
   must be struck.** Zero `DateTime` / `Stopwatch` / `Timer` / `Task.Delay` / `Thread.Sleep` /
   `TimeProvider` occurrences. Adopts F13's ruling at `.../455/spec.md:381-390`. Record as a
   documented deviation.
4. **"Cancellation and cancelled-token paths" (`spec.md:107`) is not applicable to this file.** Zero
   `CancellationToken` occurrences. (`BreadcrumbNavigationReadiness.Cancel` at `:461` is a lease
   operation, not token cancellation.)
5. **The spec's characterisation of the gap (`spec.md:41-44`) is only partly right.** Guard clauses
   are indeed dominant (29 of 49 untaken outcomes are `?? throw` guards) and disposal/out-of-order
   guards account for 4 more; but the second-largest group is an entirely untested static factory,
   which the characterisation does not anticipate.
6. **`BreadcrumbNavigationReadiness` is declared in `BreadcrumbWebViewSurfaceFactory.cs:19` and is
   F13-owned** — the same collaborator-ownership trap the sibling artifact recorded for
   `FolderBreadcrumbBridgeRouter`.
7. **Two F12 collaborator types have no file of their own.** `BreadcrumbCollapsedAttachment`
   (`BreadcrumbMessengerHub.cs:277`) and `BreadcrumbResourceOwner`
   (`BreadcrumbMessengerHub.cs:436`).
8. **The emitted Cobertura attributes on this class are both inflated** — `line-rate="0.939516"`
   against a true 0.90566, `branch-rate="0.688073"` against a true 0.66438 — and the exact
   double-count denominators (496 lines / 218 branch outcomes) were reconstructed, reproducing both
   emitted values to seven significant figures. A direct, per-file confirmation of **#441**.
9. **The load-bearing test surface is nine files, not the three that name the types.** Six reach
   this file only through F14's `ItemViewer`, and five of those six primarily target F13-owned
   production files.
10. **#488 Defect 1's mechanism claim is inaccurate** — the previous host *is* disposed on
    replacement, via `coordinator.Release()` → `BreadcrumbDropDownOpenCoordinator.cs:157`. See the
    note at the end of §9.
11. **Three of the 49 outcomes (`:135`, `:138`, `:234`) are structurally unreachable through the
    API**, with proofs in §3.2 and §4-G10/G11. The brief's implicit assumption that a branch gap is
    a test gap does not hold uniformly here.
