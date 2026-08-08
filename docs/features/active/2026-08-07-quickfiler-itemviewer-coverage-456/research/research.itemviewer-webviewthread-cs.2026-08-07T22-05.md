# Research — `QuickFiler/Viewers/ItemViewer.WebViewThread.cs`

- Feature: F14 `quickfiler-itemviewer-coverage` (issue #456), child of epic #136 `quickfiler-per-file-coverage`
- Timestamp: 2026-08-07T22-05
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5e4b635834feedd7`
- Target file: `QuickFiler/Viewers/ItemViewer.WebViewThread.cs` (**37 physical lines**)
- Compile entry: `QuickFiler/QuickFiler.csproj:431-434` (`<DependentUpon>ItemViewer.cs</DependentUpon>`, `<SubType>UserControl</SubType>`)

Claims are marked **[V]** verified by direct file read / artifact inspection, or **[I]** inferred from
verified facts.

> **Companion artifact.** Sections §0 (premise verification), §1.3 (the type-level exemption coupling),
> §4 (the clock answer), and §9 (open-issue bearing) of
> `research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md` apply verbatim to this file and are not
> repeated in full here. The most important shared facts are restated in §0 below.

---

## 0. Premises — confirmations and disproofs

| # | Premise | Verdict | Evidence |
|---|---|---|---|
| P1 | This file mentions `[ExcludeFromCodeCoverage]` only in a comment; it carries no attribute and is not itself exempt | **CONFIRMED [V]** | `ItemViewer.WebViewThread.cs:11-12`: *"The whole ItemViewer type is `[ExcludeFromCodeCoverage]` via its primary partial in `ItemViewer.cs`."* The only attribute in the type is at `ItemViewer.cs:20`. epic.md `:126-128` already records this file among the five that "mention the attribute only in a doc comment and are NOT exempt". |
| P2 | No `ItemViewer.*` partial appears in the committed Cobertura report | **CONFIRMED [V]** | `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml` — grep `filename="[^"]*ItemViewer[^"]*"` yields only `Helper Classes\ItemViewerQueue.cs`, `Viewers\ItemViewerExpanded.Designer.cs`, `Viewers\ItemViewerExpanded.cs`. Sibling files in the same folder (`Viewers\BreadcrumbUiDispatcher.cs` at line 8874 etc.) **are** present, proving the folder was instrumented. |
| P3 | "Assume 0% and plan from zero" | **CONFIRMED for this file [V]** — unlike its `.Breadcrumb.cs` sibling, this file is genuinely at zero executed coverage. **No test in `QuickFiler.Test` invokes any of its eight members on a concrete `ItemViewer`.** Verified: grep for the seven member names across `QuickFiler.Test` returns 18 hits in 9 files, and every one is either a `Mock<IItemViewer>` setup/verify in a `QfcItemController.*Tests.cs` file (which routes to the Moq proxy, not to this file) or a string literal (`Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs:366`, the text `"NavigateToString rejected"` inside an exception message). |
| P4 | `TimeProvider`/`FakeTimeProvider` unavailable on net481 | **DISPROVED [V]** — see companion artifact §0/P6 and §4 below. |
| P5 | Issue #441 harness double-count | **CONFIRMED [V]** — `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:121-122` (`.//class` then `.//lines/line`, descendant axis, inflating `$totalLines` at `:123` and therefore `LinesValid`/`LineRate` at `:137-143`). |
| P6 | `UtilitiesCS` grants no `InternalsVisibleTo` to `QuickFiler.Test` | **Not applicable to this file.** Its dependencies are `BrightIdeasSoftware` (`FastObjectListView`, `OLVColumn`), `Microsoft.Web.WebView2` (`WebView2`, `CoreWebView2InitializationCompletedEventArgs`), and `System.Windows.Forms` (`ToolStripMenuItem`, `SortOrder`, `ListViewItemSelectionChangedEventHandler`, `IList`). No `UtilitiesCS` symbol at all. |
| P7 | `gh` open-issue search | **NOT PERFORMED** — Bash tool disabled this session (`Error: No such tool available: Bash`). §7 is based on repository artifacts; unverified items are labelled. |

---

## 1. Current state

### 1.1 What the file is

Eight interface-implementing forwarders, all expression-bodied, added by the "Seam D, Cluster 2d"
narrowing work recorded in the file header (`:8-12`). Each forwards to a Designer-backed control
exposed as a public property on the primary partial:

| Member | Line | Forwards to | Receiver declared at |
|---|---|---|---|
| `NavigateToString(string html)` | 15 | `L0v2h2_WebView2.NavigateToString(html)` | `ItemViewer.cs:309-313` |
| `WebViewInitializationCompleted` add | 19 | `L0v2h2_WebView2.CoreWebView2InitializationCompleted += value` | `ItemViewer.cs:309-313` |
| `WebViewInitializationCompleted` remove | 20 | `… -= value` | " |
| `SetConversationItems(IList items)` | 23 | `TopicThread.SetObjects(items)` | `ItemViewer.cs:284-288` |
| `SortConversationByDate(SortOrder order)` | 25 | `TopicThread.Sort(SentDate, order)` | `ItemViewer.cs:284-288`, `:294-298` |
| `GetSelectedConversationItems()` | 27 | `TopicThread.SelectedObjects` | `ItemViewer.cs:284-288` |
| `ConversationItemSelectionChanged` add | 31 | `TopicThread.ItemSelectionChanged += value` | " |
| `ConversationItemSelectionChanged` remove | 32 | `… -= value` | " |
| `ShowMoveOptionsMenu()` | 35 | `MoveOptionsMenu.ShowDropDown()` | `ItemViewer.cs:399-403` |

All eight are the concrete implementation of `IItemViewer` members declared at
`QuickFiler/Viewers/IItemViewer.cs:107-113`. **None can be deleted or renamed** without breaking the
interface contract and its ~9 mock-based consumers.

### 1.2 Coverable-line inventory and the exact gate arithmetic

Expression-bodied members and field-like event accessors each emit exactly one sequence point. **[I from
V code shape]** the Cobertura denominator for this file is therefore **9 lines** (`:15, :19, :20, :23,
:25, :27, :31, :32, :35`), with **zero branch points** — the file contains no `if`, no `?:`, no `?.`, no
`??`, no `&&`/`||`, no loop, no `try`.

| Gate | Requirement | Concrete target |
|---|---|---|
| Line, per epic.md § Coverage-Target Reconciliation | >= 80% | **8 of 9 lines** (`ceil(0.8 × 9) = 8`; 7/9 = 77.8% **fails**) |
| Branch | >= 75% | **N/A — 0 branch points** |

**Directive for F1's harness (extends epic.md § Directives for F1's Ledger and Harness):** a file with
`branches-valid = 0` must be reported **N/A** for branch, never 0%, and must never count as a branch
failure. This is the branch-side analogue of the already-mandated `interface-only` line-side rule at
epic.md `:519-522` / `:533-536`. `ItemViewer.WebViewThread.cs` is a live instance of the case. **Record
as a cross-child note to F1 (issue #432).**

### 1.3 The type-level exemption coupling

`[ExcludeFromCodeCoverage]` at `ItemViewer.cs:20` is applied to the **type**. Removing it un-hides all
six `ItemViewer` partials plus `ItemViewer.Designer.cs` (6,224 lines) simultaneously; re-applying it to
the Designer partial re-hides everything, because a partial type has one identity. All seven files are
F14-owned, so this is an intra-child ordering constraint rather than a cross-child conflict. **The
resolution must be settled in `spec.md` before planning** — see companion artifact §1.3 for the full
analysis and the recommended filename-based (harness/`coverage.config`) Designer exemption.

**Practical consequence for this file:** it cannot be measured at all until that decision lands, and its
9-line denominator means a single uncovered line is 11.1% — there is **no slack**.

---

## 2. Q1 — The thread boundary, mapped

### 2.1 The finding

**This file performs zero marshalling.** [V] Full read of all 37 lines: no `Invoke`, no `BeginInvoke`,
no `InvokeRequired`, no `SynchronizationContext`, no `Dispatcher`, no `Task.Run`, no `await`, no `lock`.
Every member is a **bare, unguarded, synchronous forward to a UI-thread-affine WinForms control**.

Thread-affinity responsibility is therefore **entirely delegated to the caller**, and the callers'
discipline is **inconsistent**. That inconsistency is the file's principal risk surface.

### 2.2 Caller-side marshalling discipline (verified)

| Member | Production call site | Marshalling at the call site | Verdict |
|---|---|---|---|
| `NavigateToString` | `QuickFiler/Controllers/QfcItemController.EventWiring.cs:139-146` | **Guarded** — `if (_itemViewer.InvokeRequired) { _itemViewer.Invoke(() => _itemViewer.NavigateToString(ItemHelper.Html)); } else { _itemViewer.NavigateToString(ItemHelper.Html); }` | correct |
| `NavigateToString` | `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:200` | **Unguarded** — inside `TopicThread_ItemSelectionChanged`, a WinForms event handler. Safe **only** because ListView events are raised on the UI thread. | correct by construction |
| `NavigateToString` | `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:293` | **Unguarded** | see LD-2 |
| `SetConversationItems`, `SortConversationByDate` | `QuickFiler/Controllers/QfcItemController.Conversation.cs:221-233` | **Guarded** — `SetTopicThread` opens with `if (_itemViewer.InvokeRequired) { _itemViewer.Invoke(() => SetTopicThread(conversationInfo)); return; }` | correct |
| `GetSelectedConversationItems` | `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:196` | **Unguarded**, but inside a UI-thread event handler | correct by construction |
| `ConversationItemSelectionChanged` (subscribe) | `QuickFiler/Controllers/QfcItemController.EventWiring.cs:89-90` | Unguarded; subscription is thread-safe (`Delegate.Combine` under `Interlocked`) | acceptable |
| `WebViewInitializationCompleted` (subscribe) | `QuickFiler/Controllers/QfcItemController.EventWiring.cs:87-88` | Unguarded; same | acceptable |
| `ShowMoveOptionsMenu` | `QuickFiler/Controllers/QfcItemController.Navigation.cs:81-84` | **Guarded, but by a different mechanism** — `await _uiDispatcher.InvokeAsync(() => _itemViewer.ShowMoveOptionsMenu());` where `_uiDispatcher` is a **WPF `System.Windows.Threading.Dispatcher`** (`ItemViewer.cs:13` `using System.Windows.Threading;`, `:28` `_uiDispatcher = Dispatcher.CurrentDispatcher;`, `:71-75`) | correct, but see LD-1 |

**Three distinct marshalling mechanisms coexist across the callers of one 37-line file:**
`Control.InvokeRequired`/`Control.Invoke` (WinForms), `System.Windows.Threading.Dispatcher.InvokeAsync`
(WPF), and "raised on the UI thread already" (implicit). [V] all three are visible in the table above.
`ItemViewer` itself exposes all three seams — `UiSyncContext` (`ItemViewer.cs:59-63`), `UiScheduler`
(`:65-69`), `UiDispatcher` (`:71-75`) — which is how the divergence arose.

### 2.3 Races, lost updates, out-of-order delivery

| ID | Location | Nature |
|---|---|---|
| R1 | `:15` `NavigateToString` | **No race inside the file.** The receiver `L0v2h2_WebView2` is read once. If called off the UI thread, WinForms raises `InvalidOperationException: Cross-thread operation not valid` — a *fail-fast*, not a corruption. This is the same exception class that issue #400 recorded in production for the sibling control (`docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/regression-testing/runtime-selector-toggle-thread-affinity.2026-07-22T01-29.md:25`). |
| R2 | `:23` + `:25` `SetConversationItems` then `SortConversationByDate` | **Ordering invariant, not enforced here.** The two calls are separate `IItemViewer` members; nothing in this file guarantees they execute as one unit. `QfcItemController.Conversation.cs:231-232` issues them back to back inside a single `Invoke`d call, so the pair is atomic *in that path only*. Any future caller that marshals them separately can interleave a `SetObjects` between the set and the sort, leaving the list unsorted. See LD-3. |
| R3 | `:27` `GetSelectedConversationItems` | Returns `TopicThread.SelectedObjects`, a **live snapshot built at call time**. The caller at `EventHandlers.cs:196-201` reads `objects.Count` and `objects[0]` on subsequent lines; between them the underlying `ListView` selection can change if the UI thread re-enters (it cannot within one handler, so production is safe). A time-of-check/time-of-use hazard exists for any off-handler caller. |
| R4 | `:19-20`, `:31-32` event accessors | Field-like event add/remove on `WebView2` / `FastObjectListView`. `.NET` compiles these to `Interlocked.CompareExchange` loops, so subscribe/unsubscribe is thread-safe. **No race.** The *delivery* thread is the control's UI thread. |
| R5 | `:35` `ShowMoveOptionsMenu` | `ToolStripMenuItem.ShowDropDown()` creates and shows a top-level popup window. Off-thread invocation throws. Guarded in production (§2.2). |

**Summary of the thread-boundary answer:** the boundary is **entirely outside this file**. The file is a
pure UI-thread-affine adapter layer. That is exactly what makes it a valid "thinnest possible wiring"
target under `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy — **and exactly why it must
still be covered**, because 9 lines of thin wiring is a trivially affordable coverage cost.

---

## 3. Q2 — Testability seams

### 3.1 Member classification

| Member | Line | Class | Barrier |
|---|---|---|---|
| `NavigateToString` | 15 | **COM/WebView-bound** | `Microsoft.Web.WebView2.WinForms.WebView2.NavigateToString` performs an initialization guard and requires a live `CoreWebView2` (a real browser process) to succeed |
| `WebViewInitializationCompleted` add/remove | 19-20 | **thin wiring** | field-like event on `WebView2`; no handle or core required |
| `SetConversationItems` | 23 | **thin wiring** (probable) / COM-bound (if handle required) | `ObjectListView.SetObjects` on a handle-less `FastObjectListView` — needs a spike (§3.3) |
| `SortConversationByDate` | 25 | **thin wiring** (probable) | `ObjectListView.Sort(OLVColumn, SortOrder)` |
| `GetSelectedConversationItems` | 27 | **thin wiring** | `ListView.SelectedIndices` on a handle-less control returns an empty cached collection |
| `ConversationItemSelectionChanged` add/remove | 31-32 | **thin wiring** | field-like event on `FastObjectListView` |
| `ShowMoveOptionsMenu` | 35 | **COM/WebView-bound (popup)** | `ToolStripMenuItem.ShowDropDown()` **shows a real popup window** — a direct violation of epic.md § Shared Design 2 ("never show popups; a popup requiring human interaction is a unit-test-policy violation") |

**6 thin wiring, 2 blocked (`:15`, `:35`), 1 needing a spike (`:23`).**

### 3.2 The construction technique — no seam needed for six of nine lines

**[V] The repo already has a proven, in-use technique for exercising these forwarders headlessly:**
`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:247-265` constructs an `ItemViewer` **without
running `InitializeComponent`** and assigns each Designer-backed control through the public property
setters on the primary partial:

```csharp
private static ItemViewer CreateItemViewer()
{
    var viewer = CreateUninitialized<ItemViewer>();        // :249  FormatterServices, no ctor
    viewer.LblItemNumber = new Label();
    …
    viewer.L0vhBreadcrumb_WebView2 = CreateUninitialized<WebView2>();  // :256
    viewer.TopicThread = new FastObjectListView();                     // :257
    viewer.L0v2h2_WebView2 = CreateUninitialized<WebView2>();          // :258
    SetPrivateField(viewer, "_menuItems", new List<Component> { new ToolStripMenuItem("Move") }); // :259-263
    return viewer;
}
```

Every receiver this file needs is settable the same way:
`TopicThread` (`ItemViewer.cs:284-288`), `SentDate` (`:294-298`), `L0v2h2_WebView2` (`:309-313`),
`MoveOptionsMenu` (`:399-403`) — all `public … { get; set; }`.

**This is the "router injection" prior art the orchestrator pointed at, in its concrete form.** Two
mutually reinforcing facts make the conclusion binding:

1. **Injection is by property assignment of the concrete type.** `QfcThemeHelperTests.cs:256-258`
   assigns concrete `WebView2` instances; `:257` assigns a concrete `FastObjectListView`.
2. **The concrete property types are pinned by a live contract test.**
   `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:22-28` asserts
   `property.PropertyType.Should().Be(typeof(Microsoft.Web.WebView2.WinForms.WebView2))` for the sibling
   breadcrumb property, with the class summary *"Failure-first ItemViewer surface and compatibility
   contracts for issue #400"* (`:14`).

**Design rule for F14 [I]:** **do not retype `TopicThread`, `SentDate`, `L0v2h2_WebView2`, or
`MoveOptionsMenu` to interfaces.** Retyping breaks the property-injection technique at
`QfcThemeHelperTests.cs:256-258` at compile time, breaks the pinned-type contract style, and breaks the
production call site `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:109` which passes the
concrete WebView2 control to `_webViewInitializer.EnsureCoreWebView2Async`. Instead, **inject the
collaborator through an added sibling overload / delegate field**, following the sanctioned precedent in
the same type (`ItemViewer.Breadcrumb.cs:40-43`, `:65-67`, `:179-183`).

### 3.3 Recommended seam set

| ID | Target | Seam (epic hierarchy tier) | Rationale |
|---|---|---|---|
| **W-1** | `ShowMoveOptionsMenu` `:35` | **Injectable delegate (tier 2).** Add `private Action<ToolStripMenuItem> _showDropDown = item => item.ShowDropDown();` and change `:35` to `MoveOptionsMenu.ShowDropDown()` → `_showDropDown(MoveOptionsMenu)`, with an `internal` setter or an `internal void SetMoveOptionsPresenter(Action<ToolStripMenuItem>)` used only by tests. | **Mandatory.** Without it the only way to execute `:35` is to show a real popup, which epic.md § Shared Design 2 forbids outright. After the seam, `:35` is covered and the default lambda body remains the single irreducible uncovered element — the "thinnest possible wiring" outcome `.claude/rules/general-unit-test.md` prescribes. |
| **W-2** | `NavigateToString` `:15` | **Injectable delegate (tier 2).** `private Action<string> _navigateBody = html => L0v2h2_WebView2.NavigateToString(html);` with an `internal` override hook. | **Recommended.** An alternative that requires no production change is described in §3.4; W-2 is cleaner and matches W-1. |
| — | `:19, :20, :23, :25, :27, :31, :32` | **No seam.** Reachable directly with the §3.2 construction. | Adding seams here would be gratuitous indirection, contrary to General Code Change Policy § Design Principles ("Simplicity first … avoid deep indirection"). |

**Spike required before planning (S-A):** confirm that `FastObjectListView.SetObjects(IList)` and
`.Sort(OLVColumn, SortOrder)` execute without a created window handle. `ObjectListView.SetObjects`
guards on `InvokeRequired` (false when no handle exists) and `ListView.VirtualListSize` guards on
`IsHandleCreated`, so it should succeed **[I]** — but this is an inference about a third-party library
(`BrightIdeasSoftware`), not verified behaviour. **The spike is one throwaway test run; run it before
committing case C4/C5 below.** If it fails, the fallback is `[STATestMethod]` with a real
`FastObjectListView` (still a `Control`, never a `Form`, never shown) — see §5.

### 3.4 Rejected alternatives (brief)

- **Retype the four receiver properties to interfaces.** Rejected — see §3.2; breaks
  `QfcThemeHelperTests.cs:256-258`, the pinned-type contract style, and `ViewerSetup.cs:109`.
- **Extract the eight forwarders into a separate host-neutral adapter class.** Rejected: the forwarders
  exist *because* the interface was narrowed away from raw controls (`:8-11`, `IItemViewer.cs:102-113`);
  moving them again would just relocate the same nine lines and add a file (and a `<Compile Include>`
  hunk on the contended `QuickFiler.csproj`) for no coverage gain.
- **Assert the thrown exception instead of seaming `:15` and `:35`.** For `:15` this is viable (see
  below) and is the zero-production-change option; for `:35` it is **not**, because
  `ToolStripMenuItem.ShowDropDown()` succeeds and shows a window rather than throwing. Kept as the
  fallback for `:15` only:
  > `NavigateToString_OnUninitializedWebView_SurfacesTheControlsInitializationFailure` — call
  > `viewer.NavigateToString("<html/>")` on a viewer whose `L0v2h2_WebView2` is a
  > `CreateUninitialized<WebView2>()`, and assert an exception is thrown. Line `:15` executes (the
  > forwarding call is evaluated before the callee throws), so the line is covered. **Deterministic, no
  > popup, no external process, no production change.** Slightly opaque as documentation of intent,
  > which is why W-2 is preferred.

---

## 4. Q3 — Determinism

**(a) Does this file read wall-clock time or use timers?** **No. [V]** Full read of all 37 lines: no
`DateTime`, `DateTimeOffset`, `Stopwatch`, `Timer`, `Thread.Sleep`, `Task.Delay`, `CancellationToken`.
The `using` set is `System`, `System.Collections`, `System.Windows.Forms`, `Microsoft.Web.WebView2.Core`
(`:1-4`). There is no `async`, so no continuation scheduling either.

**(b) Repo clock abstraction.** **`System.TimeProvider`, polyfilled for net481 by
`Microsoft.Bcl.TimeProvider`, with `Microsoft.Extensions.TimeProvider.Testing.FakeTimeProvider` as the
test double.** [V] `QuickFiler.Test/packages.config:18` and `:84-88`; assembly references at
`QuickFiler.Test/QuickFiler.Test.csproj:205-206` and `:255-256`; production reference at
`QuickFiler/QuickFiler.csproj:68-69`. In active use in this exact test project —
`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:318-319`,
`Controllers/QfcDatamodelTests.cs:106,254,288`, `Controllers/QfcDatamodelLivenessTests.cs:84`,
`Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs:20`. The repo's own statement of the rule,
`QfcHomeControllerMetricsTests.cs:316`: *"Moq cannot mock the non-virtual GetLocalNow();
FakeTimeProvider is the prescribed seam."* **The orchestrator's premise that `TimeProvider` is
unavailable on net481 is disproved.** No `IClock`, `ISystemClock`, or `ITimerService` seam exists in
QuickFiler.

**(c) Recommendation for these tests.** **No clock seam is required.** Every test for this file is
synchronous and side-effect-observable at the moment of the call. Determinism obligations reduce to:

- Construct with `CreateUninitialized<ItemViewer>()` (§3.2) — no constructor, therefore **no
  `SynchronizationContext` capture, no `TaskScheduler.FromCurrentSynchronizationContext()`, no
  `Dispatcher.CurrentDispatcher`** (`ItemViewer.cs:23-30`), so none of the ambient-context fragility
  described in the companion artifact §5.2 applies here.
- Dispose every `Control` created in the test (`FastObjectListView`, `ToolStripMenuItem`,
  `OLVColumn`) in a `finally`, per the STA-precedent discipline in
  `Tags.Test/CheckBoxControllerWiring.StaTests.cs:17-18` (*"every control is disposed"*).
- Zero `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `SpinWait`, or timed `Wait`.

**If a future change makes any member async, use `FakeTimeProvider`**, not a real delay.

---

## 5. Q4 — Sibling boundaries

### 5.1 Dependency inventory

| Symbol | Declared in | Owner | Public surface sufficient? |
|---|---|---|---|
| `L0v2h2_WebView2` | `QuickFiler/Viewers/ItemViewer.cs:309-313` | **F14 (own child)** | yes |
| `TopicThread` | `QuickFiler/Viewers/ItemViewer.cs:284-288` | **F14 (own child)** | yes |
| `SentDate` | `QuickFiler/Viewers/ItemViewer.cs:294-298` | **F14 (own child)** | yes |
| `MoveOptionsMenu` | `QuickFiler/Viewers/ItemViewer.cs:399-403` | **F14 (own child)** | yes |
| `IItemViewer` members `:107-113` | `QuickFiler/Viewers/IItemViewer.cs` | **F14 (own child)** | yes |
| `WebView2`, `CoreWebView2InitializationCompletedEventArgs` | `Microsoft.Web.WebView2.*` | third party | n/a |
| `FastObjectListView`, `OLVColumn` | `BrightIdeasSoftware` | third party | n/a |
| `ToolStripMenuItem`, `SortOrder`, `ListViewItemSelectionChangedEventHandler`, `IList` | BCL | n/a | n/a |

### 5.2 Cross-child notes

**This file has NO dependency on any F10, F12, or F13 file.** [V] grep of all 37 lines for `Breadcrumb`,
`IBreadcrumb`, `IWebViewMessenger`, `IWebViewCoreInitializer`, `QfcItemController` returns nothing except
the word "thread" in the header comment. **Zero cross-child signature changes are requested and zero are
needed.** Three notes for the orchestrator to carry into `spec.md`:

- **W-X1 (to F1, issue #432 — harness requirement).** A production file with **zero branch points** must
  report branch coverage as **N/A**, never 0%, and must never fail the 75% branch gate.
  `ItemViewer.WebViewThread.cs` has `branches-valid = 0` (§1.2) and is a live instance. This is the
  branch-side analogue of the already-mandated `interface-only` rule at epic.md `:519-536`. Without it
  F14 cannot close.
- **W-X2 (to F10, issue #453 — advisory, no change requested).** F10 owns all seven production call
  sites of this file's members (`QfcItemController.EventWiring.cs:87-90,139-146`,
  `.EventHandlers.cs:196-200`, `.Conversation.cs:221-233`, `.FocusAndTheme.cs:293`,
  `.Navigation.cs:81-84`). If F10's coverage work introduces `Mock<IItemViewer>` at additional call
  sites, F14's numbers are unaffected (mocks bypass this file entirely). **But if F10 changes the
  marshalling discipline at any call site, LD-1/LD-2 below change with it.** F10 and F14 should not both
  attempt to fix LD-2.
- **W-X3 (to F14's own `ItemViewer.cs` work — intra-child).** Seams W-1 and W-2 add private delegate
  fields. Placing them in `ItemViewer.WebViewThread.cs` keeps them in this file's denominator (good — the
  field initialisers are covered by construction). **Do not** place them in `ItemViewer.cs`, which would
  move covered lines into a sibling file and distort both files' per-file rates.

---

## 6. Q5 — Test plan sketch

### 6.1 Sequencing

- **T0 (prerequisite, shared with all six partials)** — remove `[ExcludeFromCodeCoverage]` from
  `ItemViewer.cs:20` and settle the `ItemViewer.Designer.cs` disposition (§1.3).
- **T0b** — run the F1 harness; record actual per-file line rate and confirm `branches-valid = 0`.
  Expected result for this file: **0.0% line, N/A branch** (§0/P3).
- **S-A spike** — confirm handle-less `FastObjectListView.SetObjects` / `.Sort` behaviour (§3.3) before
  committing C4/C5 to non-STA.

### 6.2 Case inventory

All cases: MSTest `[TestClass]`/`[TestMethod]`, Moq where a collaborator is needed, FluentAssertions,
AAA, no temp files, no external services, **no live Form, no popup**, no `Thread.Sleep`/`Task.Delay`/wall-clock wait.

**Proposed home: `QuickFiler.Test/Viewers/ItemViewerWebViewThreadTests.cs`** (new). Requires
`<Compile Include="Viewers\ItemViewerWebViewThreadTests.cs" />` in `QuickFiler.Test/QuickFiler.Test.csproj`,
inserted adjacent to `:80`, **CRLF preserved**, minimal hunk. Projected size ~200 lines — well inside 500.

Fixture `U` = `CreateUninitialized<ItemViewer>()` + property assignment, per §3.2, all controls disposed
in `finally`.

| # | Test name | Lines covered | Seam | Collaborators |
|---|---|---|---|---|
| **C1** | `WebViewInitializationCompleted_Subscribe_AddsHandlerToDesignerWebView` | `:19` | none | `U`; `L0v2h2_WebView2 = CreateUninitialized<WebView2>()`; assert via reflection on the `WebView2` event backing field that the delegate list contains the handler |
| **C2** | `WebViewInitializationCompleted_Unsubscribe_RemovesHandlerFromDesignerWebView` | `:20` | none | same; subscribe then unsubscribe, assert backing field is null |
| **C3** | `ConversationItemSelectionChanged_SubscribeAndUnsubscribe_RoundTripOnTopicThread` | `:31`, `:32` | none | `U`; `TopicThread = new FastObjectListView()`; observe by raising `ItemSelectionChanged` through the protected `ListView.OnItemSelectionChanged` reflection raiser (the `InvokeOnClick` technique of `Tags.Test/CheckBoxControllerWiring.StaTests.cs:85-94`), asserting the handler fires once then zero times |
| **C4** | `SetConversationItems_ForwardsTheListToTopicThread` | `:23` | none (pending S-A) | `U`; `TopicThread = new FastObjectListView()`; assert `TopicThread.Objects` / `GetItemCount()` reflects the supplied `IList` |
| **C5** | `SortConversationByDate_SortsTopicThreadOnTheSentDateColumn` | `:25` | none (pending S-A) | `U`; `TopicThread = new FastObjectListView()`; `SentDate = new OLVColumn()` added to `TopicThread.AllColumns`; assert `TopicThread.PrimarySortColumn` is the `SentDate` instance and `PrimarySortOrder == SortOrder.Descending` — **this is the test that pins the encapsulated `SentDate` dependency the file header calls out at `:10-11`** |
| **C6** | `GetSelectedConversationItems_WithNoSelection_ReturnsEmptyList` | `:27` | none | `U`; `TopicThread = new FastObjectListView()`; assert non-null, `Count == 0` |
| **C7** | `NavigateToString_ForwardsHtmlToTheDesignerWebView` | `:15` | **W-2** | `U`; inject a recording `Action<string>`; assert the exact html string is forwarded once. *(Fallback without W-2: assert the uninitialized-WebView2 exception per §3.4 — same line covered, weaker intent documentation.)* |
| **C8** | `ShowMoveOptionsMenu_ForwardsToTheMoveOptionsMenuPresenter` | `:35` | **W-1 (mandatory)** | `U`; `MoveOptionsMenu = new ToolStripMenuItem("Move")`; inject a recording `Action<ToolStripMenuItem>`; assert it received the exact `MoveOptionsMenu` instance. **No popup is shown.** |

**8 cases → 9 of 9 lines (100%).** Per issue #136 each becomes one atomic task.

Minimum to clear the 80% gate is **8 of 9 lines**, i.e. all cases except one. **Do not plan to the
minimum** — a 9-line file at 100% costs almost nothing and removes any #441-related measurement doubt.

### 6.3 STA determination

**Baseline recommendation: no STA test is required for this file.** Justification:

- All nine lines are reachable through `CreateUninitialized<ItemViewer>()` + property injection, a
  technique already proven in this same test project on this same type
  (`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:247-265`), executing in a plain `[TestClass]`.
- The two genuinely host-bound lines (`:15`, `:35`) are covered by **seams** W-2 and W-1, which is tier 2
  of the epic's hierarchy and strictly preferred over STA (epic.md § Shared Design 2 and 3: *"seams
  remain the required first approach"*).
- **`QuickFiler.Test` currently contains no `*.StaTests.cs` file at all** — verified: a repo-wide grep for
  `STATestClass` returns hits only in `Tags.Test` (2 files), `TaskVisualization.Test` (3 files), docs, and
  agent memory. Introducing the first one for a 37-line forwarder file would be disproportionate.

**Conditional STA fallback (C4/C5 only).** If spike S-A shows that
`FastObjectListView.SetObjects`/`.Sort` require a created window handle, move **only C4 and C5** into
`QuickFiler.Test/Viewers/ItemViewerConversationList.StaTests.cs` with `[STATestClass]`/`[STATestMethod]`,
modelled on `Tags.Test/CheckBoxControllerWiring.StaTests.cs`. Conditions that would then be satisfied:
(a) a seam was attempted first and rejected as gratuitous indirection over a third-party control the
repo does not own; (b) the file is dedicated, the control is a never-shown in-memory `FastObjectListView`
(a `Control`, never a `Form`), no `Show()`, no message pump, no timer, and every control disposed. The
file must carry an in-file summary stating why no seam was feasible, matching
`CheckBoxControllerWiring.StaTests.cs:13-19`.

Even in that fallback, dropping C4 and C5 entirely still yields 7/9 = 77.8% — **below the 80% gate**. So
if S-A fails, **at least one of C4/C5 must proceed under STA**; they cannot both simply be dropped.

---

## 7. Open-issue bearing

**Constraint: `gh` could not be run (Bash tool disabled).** Items marked *(unverified)* rest on the
orchestrator's description only.

| Issue | Bearing |
|---|---|
| **#441** — harness double-counts `<line>` nodes | **Direct. [V]** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:121-122`. With a 9-line denominator, a doubled count (18) makes 9 covered lines read as 50% instead of 100%. **This file is unusually sensitive to #441** — it is small enough that the inflation is not lost in aggregation. F14 must take its number from F1's recomputed per-file figure (deduplicated `<line>`, child axis — the same script already does this correctly at `:181,219`), and must annotate any `<class>`-attribute figure it quotes as "#441 — unreliable". |
| **#230** — WinForms message-pump test seam *(unverified)* | If a pump helper lands in `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`, the conditional STA fallback for C4/C5 (§6.3) becomes unnecessary. **Check #230's state at plan time before authoring any STA file.** |
| **#400** — `quickfiler-folder-selector-dropdown-400` (active) | **Indirect.** `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/` is live and authorises edits to `ItemViewer.Breadcrumb.cs` and `ItemViewer.cs`-adjacent surface. It does **not** touch this file's members, but it *does* touch `ItemViewer.cs`, whose properties are this file's receivers, and it owns the `ItemViewerBreadcrumbDropDownContractTests.cs` pinned-type contract style that §3.2 relies on. **F14 must read #400's merged state before planning.** |
| **#439** — efcviewer missing lineage and segment navigation *(unverified)* | EfcViewer surface (F9-assigned). This file is `ItemViewer`-only. **No bearing.** |
| **#440** — breadcrumb left/right arrow parent-child navigation *(unverified)* | Breadcrumb keyboard semantics. This file has no breadcrumb or keyboard member. **No bearing.** |
| **#426** — emailmovemonitor hook retention | **No bearing.** |

---

## 8. Latent defect promotion candidates

Each is a distinct promotion candidate, out of scope to fix under the epic's no-behavior-change NFR, to
be promoted through the MCP promotion lifecycle per epic.md § Latent Defect Promotion.

### LD-1 — `ShowMoveOptionsMenu` is marshalled onto a WPF `Dispatcher` while every sibling forwarder is marshalled onto the WinForms `Control` boundary

`QuickFiler/Controllers/QfcItemController.Navigation.cs:83` marshals with
`await _uiDispatcher.InvokeAsync(() => _itemViewer.ShowMoveOptionsMenu())`, where `_uiDispatcher`
originates from `System.Windows.Threading.Dispatcher.CurrentDispatcher` captured in the `ItemViewer`
constructor (`QuickFiler/Viewers/ItemViewer.cs:13,28,71-75`). Every other forwarder in
`ItemViewer.WebViewThread.cs` is marshalled — where it is marshalled at all — with WinForms
`Control.InvokeRequired`/`Control.Invoke` (`QfcItemController.EventWiring.cs:139-146`,
`QfcItemController.Conversation.cs:224-228`). A WPF `Dispatcher` and a WinForms
`WindowsFormsSynchronizationContext` are **different queues**; `Dispatcher.CurrentDispatcher` on the
Outlook UI thread creates a WPF dispatcher whose message loop is only pumped if a WPF component is
pumping it. In a VSTO add-in with no WPF root, work queued to that dispatcher can be delayed until a
nested WPF pump runs, or never run at all. The move-options menu therefore has a different — and weaker
— delivery guarantee than every other UI operation on the same control. Two mechanisms marshalling to
"the UI thread" through different queues on the same control is a latent ordering hazard independent of
whether the menu currently appears.

### LD-2 — `NavigateToString` is called unguarded from `QfcItemController.FocusAndTheme.cs:293` while the structurally identical call at `EventWiring.cs:141` is `InvokeRequired`-guarded

`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:293` calls
`_itemViewer.NavigateToString(ItemHelper.ToggleDark(desiredState))` with no `InvokeRequired` guard.
The same operation at `QuickFiler/Controllers/QfcItemController.EventWiring.cs:139-146` is explicitly
guarded, and the comparable topic-thread pair at `QfcItemController.Conversation.cs:224-228` is guarded.
`ItemViewer.WebViewThread.cs:15` performs no marshalling of its own, so the unguarded site is protected
only by the assumption that theme toggling always originates on the UI thread. Theme toggling is
precisely the family that produced issues #254 and #269 (dark-mode stale labels arising from
fire-and-forget async theme application), so the assumption is not obviously safe. The failure mode is
`InvalidOperationException: Cross-thread operation not valid: Control 'L0v2h2_WebView2' accessed from a
thread other than the thread it was created on` — the exact shape recorded for the sibling control in
issue #400's runtime evidence
(`docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/regression-testing/runtime-selector-toggle-thread-affinity.2026-07-22T01-29.md:25`).

### LD-3 — `SetConversationItems` and `SortConversationByDate` are two unrelated interface members that must be issued as one atomic pair, with nothing enforcing it

`ItemViewer.WebViewThread.cs:23` and `:25` are independent `IItemViewer` members
(`QuickFiler/Viewers/IItemViewer.cs:109-110`). The only production caller,
`QuickFiler/Controllers/QfcItemController.Conversation.cs:231-232`, issues them back to back inside a
single `Invoke`d `SetTopicThread` call, so the pair is atomic in that path. Nothing in the interface, the
implementation, or the XML documentation records that atomicity as a requirement. Any future caller that
marshals the two separately — or that calls `SetConversationItems` from a second code path without a
following sort — leaves the conversation list in source order rather than descending sent-date order,
with no error. A combined `SetConversationItems(IList items, SortOrder order)` member, or an explicit
contract comment, would remove the hazard.

### LD-4 — the `ItemViewer` type exposes three different UI-thread seams, and the codebase uses all three

`QuickFiler/Viewers/ItemViewer.cs` exposes `UiSyncContext` (`:59-63`), `UiScheduler` (`:65-69`), and
`UiDispatcher` (`:71-75`), all captured in the constructor (`:26-28`). Consumers of the forwarders in
`ItemViewer.WebViewThread.cs` have accordingly diverged onto three marshalling strategies (see §2.2):
`Control.Invoke` (`EventWiring.cs:141`, `Conversation.cs:226`), WPF `Dispatcher.InvokeAsync`
(`Navigation.cs:83`), and unguarded-because-already-on-the-UI-thread (`EventHandlers.cs:196,200`,
`FocusAndTheme.cs:293`). Three concurrent marshalling contracts on one control is a design-debt item
that makes every thread-affinity review of this area expensive and makes defects like LD-1 and LD-2
easy to introduce. Consolidating on one seam is out of scope for this epic but should be tracked.

### LD-5 — `ItemViewer.WebViewThread.cs:11-12` contains a comment that will become false the moment F14 completes

The header states: *"The whole ItemViewer type is `[ExcludeFromCodeCoverage]` via its primary partial in
`ItemViewer.cs`."* F14's central deliverable is removing that attribute (§1.3). This is not a code defect
but a **documentation-accuracy task that must be inside F14's own plan**, not deferred: leaving the
comment in place after the attribute is removed would mislead every future reader and would likely be
flagged by feature-review as a stale comment under General Code Change Policy § Naming, Docs, and
Comments ("Keep comments synchronized with behavior"). **Add an explicit plan task to update `:8-12`.**

---

## 9. Q6 — 500-line rule

- Current: **37 lines**. Limit 500. Headroom **463 lines**.
- Projected additions: W-1 (delegate field + `internal` setter + forwarding change) ≈ **+8 lines after
  CSharpier**; W-2 (same shape) ≈ **+8 lines**; XML doc comments on the two new `internal` members ≈
  **+6 lines**; the LD-5 header correction is net-neutral.
- **Projected post-refactor: ~59 lines. No split required, and none will ever plausibly be required.**

No new production file is created, so no `QuickFiler.csproj` `<Compile Include>` edit is needed on the
production side — **this file contributes zero conflict risk to the epic's contended csproj**
(epic.md § Cross-Child Constraints 1). The only csproj edit F14 needs for this file is the **test**
project entry for `QuickFiler.Test/Viewers/ItemViewerWebViewThreadTests.cs` in
`QuickFiler.Test/QuickFiler.Test.csproj` (adjacent to `:80`, CRLF preserved).

---

## 10. Summary of decisions the planner must make

1. **T0 first.** Nothing about this file is measurable until `[ExcludeFromCodeCoverage]` leaves
   `ItemViewer.cs:20` and the `ItemViewer.Designer.cs` disposition is settled (§1.3).
2. **Get W-X1 into F1's harness contract** — zero-branch files must report branch as N/A. Without it this
   file cannot pass the 75% branch gate, because it has no branches to cover (§1.2).
3. **Seam W-1 is mandatory**, because `:35` cannot otherwise be executed without showing a popup, which
   the epic forbids outright (§3.3).
4. **Run spike S-A before finalising C4/C5's placement.** Its outcome is the only thing that could force
   the first `*.StaTests.cs` file into `QuickFiler.Test` — and at least one of C4/C5 must proceed either
   way, because dropping both leaves the file at 77.8%, below the gate (§6.3).
5. **Plan all 8 cases to 100%, not to the 8/9 minimum.** The file is nine lines; the margin for a
   measurement surprise is one line (§6.2).
6. **Do not retype `TopicThread`, `SentDate`, `L0v2h2_WebView2`, or `MoveOptionsMenu`** (§3.2).
7. **Promote LD-1 … LD-5 as GitHub issues** before F14 completes; LD-5 is in-scope for F14's own plan.
