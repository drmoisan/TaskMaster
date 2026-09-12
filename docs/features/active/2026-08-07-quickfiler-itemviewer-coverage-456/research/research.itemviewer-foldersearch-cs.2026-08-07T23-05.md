# Research — `QuickFiler/Viewers/ItemViewer.FolderSearch.cs`

- Feature: F14 `quickfiler-itemviewer-coverage` (issue #456), child of epic #136 `quickfiler-per-file-coverage`
- Timestamp: 2026-08-07T23-05
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5e4b635834feedd7`
- Target file: `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` (74 physical lines)
- Compile entry: `QuickFiler/QuickFiler.csproj:427-430` (`<DependentUpon>ItemViewer.cs</DependentUpon>`, `<SubType>UserControl</SubType>`)

Claims are marked **[V]** (verified by direct file read, grep, or fetched issue text) or **[I]**
(inferred). No Bash tool and therefore no `gh` was available; GitHub issue text was obtained by WebFetch
against the public issue pages and is marked **[V-web]**.

**This is the only one of the three files assigned to this researcher that contains branch logic.** It is
therefore the only one where the `>= 75%` branch gate is a real constraint.

---

## 0. Premise verification

| # | Supplied premise | Verdict | Evidence |
|---|---|---|---|
| P1 | `ItemViewer` is a `UserControl` | **CONFIRMED [V]** | `QuickFiler/Viewers/ItemViewer.cs:21` |
| P2 | This file carries no real `[ExcludeFromCodeCoverage]`; `:17` only mentions it | **CONFIRMED [V]** | Full read of all 74 lines. The token appears only in the comment block `:9-17`, whose last sentence is *"The whole ItemViewer type is [ExcludeFromCodeCoverage] via its primary partial in ItemViewer.cs."* |
| P3 | No `ItemViewer.*` partial in the committed Cobertura report | **CONFIRMED [V]** | `research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md:20` (grep against `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`) |
| P4 | Assume ~0% measured coverage | **CONFIRMED for this file [V]** | Repo-wide grep for its 17 member names returns production call sites (`QfcItemController.EventHandlers.cs:164-189,211`, `EventWiring.cs:77-91`, `Navigation.cs:54`) plus `Mock<IItemViewer>` verifications (`QfcItemController.NavigationTests.cs:198`). **No test executes a line of this file.** |
| P5 | `InternalsVisibleTo("QuickFiler.Test")` granted by QuickFiler | **CONFIRMED and load-bearing here [V]** | `QuickFiler/Properties/AssemblyInfo.cs:5`. Unlike the other two files, this one's tests need internal access: `BreadcrumbBridgeCoordinator`'s three-argument constructor (`BreadcrumbBridgeCoordinator.cs:45`), `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` (`BreadcrumbUiDispatcher.cs:62`), `BreadcrumbPopupUiOperations.CreateForCurrentThreadTests()` (`BreadcrumbPopupUiOperations.cs:83`), and `ItemViewer.BreadcrumbCoordinator` (`ItemViewer.Breadcrumb.cs:25`) are all `internal`. |
| P6 | `UtilitiesCS` grants QuickFiler.Test no internals access | **CONFIRMED as a constraint, but not engaged [V]** | The only `UtilitiesCS` types this file touches are the **public** `FolderRow` (`:22`) and, transitively, the **public** `IFolderHierarchyProvider`. No `UtilitiesCS` internal is required, so `epic.md:619-631` does not bind and `UtilitiesCS/Properties/AssemblyInfo.cs` is not touched. |
| P7 | Issue #441 corrupts `<class>` `line-rate` | **CONFIRMED [V]** | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:121-122` |
| **Q4 premise** | *"`ItemViewer.FolderSearch.cs` likely filters or searches an Outlook folder collection"* | **DISPROVED [V]** | See §3. The file contains no folder enumeration, no filtering predicate, and no reference to `Microsoft.Office.Interop.Outlook.*`. That machinery was decommissioned by issue #351 and now lives in F12-owned types. |

---

## 1. What the file is

Seventeen forwarding members split into two clusters, per the file's own header comment (`:9-17`):

- **Folder cluster (`:20-56`)** — ten members plus two events, every one a thin delegation to
  `BreadcrumbCoordinator` (a `BreadcrumbBridgeCoordinator`, declared at `ItemViewer.Breadcrumb.cs:25`) or
  to a sibling breadcrumb wrapper in `ItemViewer.Breadcrumb.cs`.
- **Search cluster (`:58-72`)** — four members forwarding to the Designer-backed `TxtboxSearch`
  (`System.Windows.Forms.TextBox`, wrapper property `ItemViewer.cs:389-393`).

The comment records the design intent accurately and is worth quoting because it is the reason the Q4
premise fails: *"Every folder member is a thin delegation to the host-neutral, unit-tested
BreadcrumbBridgeCoordinator pipeline (see ItemViewer.Breadcrumb.cs); the legacy CboFolders owner-draw
machinery and the FolderHierarchyBuilder.Build call are decommissioned (AC-5). … On a bare viewer (no
pipeline yet) the members are inert: setters no-op and getters return the legacy empty-combo values."*

That last sentence is the file's **only real behavioural contract**, and it is exactly what the `?.` /
`??` / `&&` branches implement. Covering it is the substance of this file's test work.

---

## 2. Q1 — Member-by-member classification (exhaustive)

`using` set: `System`, `System.Collections.Generic`, `System.Linq`, `System.Windows.Forms`, `UtilitiesCS`
(`:1-5`). **No `Microsoft.Office.Interop.Outlook` import and no COM type anywhere in the file. [V]**

| # | Member | Lines | Coverable | Conditions | Class | Delegates to |
|---|---|---|---|---|---|---|
| 1 | `SetFolderItems(string[])` | 20 | 20 | 1 (`?.`) | **thin wiring** | `BreadcrumbBridgeCoordinator.AddItems` (`:131`) |
| 2 | `SetFolderSuggestions(IReadOnlyList<FolderRow>)` | 22-23 | 22 | 1 (`?.`) | thin wiring | `.SetSuggestions` (`:100`) |
| 3 | `GetSelectedFolder()` | 25 | 25 | 1 (`?.`) | thin wiring | `.GetSelectedFolder()` (`:190`) |
| 4 | `SetFolderSelectedIndex(int)` | 27 | 27 | 1 (`?.`) | thin wiring | `.SelectRow` (`:175`) |
| 5 | `SetFolderSelectedItem(string)` | 29 | 29 | 1 (`?.`) | thin wiring | `.SelectItem` (`:184`) |
| 6 | `SetFolderDroppedDown(bool)` | 31-32 | 31 | 0 | thin wiring | `ItemViewer.SetBreadcrumbDropDownState` (`ItemViewer.Breadcrumb.cs:223-235`) |
| 7 | `ClearFolderItems()` | 34 | 34 | 1 (`?.`) | thin wiring | `.Clear()` (`:150`) |
| 8 | `FocusFolderDropDown()` | 36 | 36 | 0 | thin wiring | `ItemViewer.FocusBreadcrumb` (`Breadcrumb.cs:200-209`) |
| 9 | `FolderContains(string)` | 38-39 | 38 | 1 (`!= null &&`) | **pure/host-neutral** — the only member with a composed expression | `.Contains` (`:196`) |
| 10 | `GetFolderItems()` | 41-42 | 41 | 2 (`?.`, `??`) | **pure/host-neutral** | `.GetFolderItems()` (`:193`) |
| 11 | `FolderSelectionChanged` add/remove | 44-48 | 46, 47 | 0 | thin wiring | `_folderSelectionChangedHandlers` field (`Breadcrumb.cs:34`) |
| 12 | `FolderKeyDown` add/remove | 52-56 | 54, 55 | 0 | thin wiring | `_folderKeyDownHandlers` field (`Breadcrumb.cs:35`) |
| 13 | `SearchText` get | 58 | 58 | 0 | thin wiring | `TxtboxSearch.Text` |
| 14 | `SearchTextChanged` add/remove | 60-64 | 62, 63 | 0 | thin wiring | `TxtboxSearch.TextChanged` |
| 15 | `SearchKeyDown` add/remove | 66-70 | 68, 69 | 0 | thin wiring | `TxtboxSearch.KeyDown` |
| 16 | `FocusSearch()` | 72 | 72 + lambda | 0 | **thin wiring, host-bound** | `TxtboxSearch.Invoke(new Action(() => TxtboxSearch.Focus()))` — the only member requiring a **window handle** |

**Totals: ~21 coverable lines (20 statements + 1 lambda body), ~10 condition points (~20 branch
outcomes), 2 pure/host-neutral members, 14 thin-wiring members, 1 host-bound member (`FocusSearch`),
0 COM-bound members.**

Exact line-to-sequence-point mapping is **[I]** until F1's harness produces a report; expression-bodied
members spanning two physical lines (`:22-23`, `:31-32`, `:38-39`, `:41-42`) may emit one sequence point
each rather than two. The *rate* is insensitive to this because covered and coverable move together.

---

## 3. Q4 — Folder search: the COM question, answered

**There is no COM in this file, no folder enumeration in this file, and no filtering predicate in this
file. The separation the brief asks about already exists and was made by issue #351.**

Verified chain:

1. **No `Microsoft.Office.Interop.Outlook` reference.** Grep of the file returns nothing; the five
   `using` directives are listed in §2. No `MAPIFolder`, no `Store`, no `Folder`, no `Application`.
2. **The legacy machinery is explicitly recorded as removed.** File comment `:11-12`: *"the legacy
   CboFolders owner-draw machinery and the FolderHierarchyBuilder.Build call are decommissioned
   (AC-5)."*
3. **The predicate lives in an already host-neutral, already-unit-tested type.**
   `BreadcrumbBridgeCoordinator` (`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:25`) is
   `public sealed class ... : IDisposable` whose own XML summary (`:13-24`) states it is *"Host-neutral
   coordinator … keeping this type free of WinForms/WebView2/COM usage"*. Its `Contains`, `GetFolderItems`,
   and `GetSelectedFolder` all delegate to `_router` (`:190`, `:193`, `:196`), a
   `FolderBreadcrumbBridgeRouter` constructed at `:52-54`. Those types are **F12-owned** (`epic.md` F12,
   `Controllers/BreadcrumbBridgeRouter.cs`, `Viewers/BreadcrumbBridgeCoordinator.cs`).
4. **The actual folder search is in the controller, not the viewer.**
   `QfcItemController.EventHandlers.cs:166-171` calls `_folderHandler.FindFolder(searchString: "*" +
   _itemViewer.SearchText + "*", …, objItem: Mail)` — that is where the COM-touching search happens, and
   it is **F10-owned**.

**Reporting conclusion:** the exact separation point is
`QfcItemController.EventHandlers.cs:167` (`_itemViewer.SearchText` — the viewer's only contribution to
the search is to surface the raw textbox string) and
`QfcItemController.EventHandlers.cs:172-173` (`ClearFolderItems()` / `SetFolderItems(folders)` — the
viewer receives an already-computed `string[]`). **Nothing needs to be extracted from this file.**

### 3.1 Issue #438 `quickfiler-search-keystroke-focus-steal` — direct bearing, precisely localised

**[V-web]** #438 (open, `bug`) reports that typing into the QuickFiler folder-search textbox loses focus
after one to two characters because *"each keystroke triggers a `TextChanged` event that opens the
breadcrumb dropdown, which pulls focus away from the search field"*, and states the expected behaviour:
*"The dropdown should open and update on every keystroke while keyboard focus remains in the search
textbox. Focus should only move to the dropdown through explicit user actions."* Severity is recorded as
High/Blocker.

The chain runs **through this file** and is fully verified:

```
TxtboxSearch.TextChanged
  -> ItemViewer.FolderSearch.cs:60-64   SearchTextChanged  (this file — the subscription seam)
  -> QfcItemController.EventWiring.cs:77-79  TextBoxSearch_TextChanged
  -> QfcItemController.EventHandlers.cs:164-178
       :172 ClearFolderItems()          -> this file :34
       :173 SetFolderItems(folders)     -> this file :20
       :176 SetFolderSelectedIndex(1)   -> this file :27   (only when folders.Length >= 2)
       :177 SetFolderDroppedDown(true)  -> this file :31-32   <-- the focus steal enters here
  -> ItemViewer.Breadcrumb.cs:223-235   SetBreadcrumbDropDownState(true)
       :225 if (_breadcrumbLifecycleCoordinator == null)
       :227   if (droppedDown)
       :229     FocusBreadcrumb();                       <-- unconditional focus on open (fallback path)
       :234 else _breadcrumbLifecycleCoordinator.SetDroppedDown(droppedDown, FocusBreadcrumbCore);
                                                          <-- F12-owned path, same coupling
```

**The defect is the "droppedDown implies focus" coupling inside `SetBreadcrumbDropDownState`, reached
once per keystroke through `ItemViewer.FolderSearch.cs:31-32`.** `ItemViewer.FolderSearch.cs` itself
contains no defect — it is a faithful forwarder — but it is the boundary a fix will cross.

**Two consequences the planner must act on:**

1. **Do not fix #438 in F14.** The epic's no-behavior-change NFR forbids it, and the fix belongs in
   `SetBreadcrumbDropDownState` (F14-owned `ItemViewer.Breadcrumb.cs`, sibling researcher) and/or
   `BreadcrumbItemViewerLifecycleCoordinator.SetDroppedDown` (**F12-owned — out of bounds for F14**).
2. **Do not let F14's tests cement the defect.** Case F11 in §6.2 covers
   `SetFolderDroppedDown(true) -> FocusBreadcrumb()`. Written naively it becomes a red test the moment
   #438 is fixed, and a reviewer may then "fix" the test instead of accepting the behaviour change. Each
   such case **must carry an in-code comment naming issue #438** and asserting the *current* behaviour
   explicitly as current, following the pattern the breadcrumb sibling artifact recommends for issue #440
   (`research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md:669`). This is the single most important
   cross-issue instruction in this artifact.

---

## 4. Q5 — Command/event dispatch and the F3 boundary

**F14 requires nothing from F3 and must edit none of the F3-owned keyboard-action files.**

Verified: a grep of the entire `QuickFiler/Viewers/` folder for `KbdActions`, `KaChar`, `KaKey`,
`KaStringAsync`, and `IMailItemActions` returns **no matches [V]**. The viewer surfaces raw WinForms
events; the *controller* binds one of them to a keyboard handler:

- `QfcItemController.EventWiring.cs:81-83` — `_itemViewer.FolderKeyDown += _kbdHandler.CboFolders_KeyDownAsync`
  (`_kbdHandler` is an `IQfcKeyboardHandler`, F10/F3 territory, consumed only from the controller side).
- `QfcItemController.EventWiring.cs:91` — `_itemViewer.SearchKeyDown += this.TextBoxSearch_KeyDown`
  (handler at `EventHandlers.cs:180-189`).
- `QfcItemController.EventWiring.cs:86` — `_itemViewer.FolderSelectionChanged += this.CboFolders_SelectedIndexChanged`
  (handler at `EventHandlers.cs:209-212`).
- `QfcItemController.EventWiring.cs:77-79` — `_itemViewer.SearchTextChanged += this.TextBoxSearch_TextChanged`.
- `QfcItemController.Navigation.cs:54` — `_itemViewer.FocusSearch()`.

Bearing of the two issues the brief named:

| Issue | Bearing |
|---|---|
| **#445** `quickfiler-keyboard-action-contract-defects` **[V-web]** — three contract defects in the QuickFiler keyboard-action types, explicitly *not* fixed by #430 | **None on this file.** The zero-match grep above is the evidence. The nearest coupling is the controller-side subscription at `EventWiring.cs:81-83`, which F14 does not touch. If #445's fix changes `IQfcKeyboardHandler.CboFolders_KeyDownAsync`'s signature, the break lands in `QfcItemController.EventWiring.cs` (F10), not here. |
| **#444** `kbdactions-enumerable-ctor-bypasses-duplicate-guard` **[V-web]** — `KbdActions<TKey,UClass,VDelegate>`'s `IEnumerable<UClass>` constructor bypasses the `Add` duplicate guard, so duplicate registrations throw `InvalidOperationException` on later lookup | **None on this file.** Same evidence. |

`FolderKeyDown`'s existing public surface is sufficient for testing: it is a plain `KeyEventHandler`
event over a private delegate field (`Breadcrumb.cs:35`), raised by `OnBreadcrumbFolderArrowKeyDown`
(`Breadcrumb.cs:242-248`). No F3 type is required to exercise it.

---

## 5. Q2 — Seam recommendation

### 5.1 The one testability obstacle

Fifteen of the seventeen members are reachable with **no production change at all**, using a bare
`CreateUninitialized<ItemViewer>()` for the null branches and an assigned `TxtboxSearch` for the search
cluster. The obstacle is the **non-null** side of the seven `?.`/`&&`/`??` branches, which requires
`ItemViewer.BreadcrumbCoordinator` to be non-null. That property is declared

```
internal BreadcrumbBridgeCoordinator BreadcrumbCoordinator { get; private set; }   // ItemViewer.Breadcrumb.cs:25
```

The getter is reachable from `QuickFiler.Test` via `InternalsVisibleTo`; the **setter is `private`** and
is written only by `InitializeBreadcrumbPipeline` (`Breadcrumb.cs:59`).

`BreadcrumbBridgeCoordinator` is `public sealed` (`BreadcrumbBridgeCoordinator.cs:25`) with non-virtual
methods, so **Moq cannot mock it**. It must be constructed for real — which is cheap and legitimate,
because it is host-neutral by design and its internal three-argument constructor takes only injectable
collaborators:

```
internal BreadcrumbBridgeCoordinator(IWebViewMessenger messenger,
                                     IFolderHierarchyProvider provider,
                                     BreadcrumbUiDispatcher dispatcher)   // :45-59
```

with `IWebViewMessenger` and `IFolderHierarchyProvider` both mockable interfaces and
`BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` (`BreadcrumbUiDispatcher.cs:62-65`) supplying a
context-free, pump-free dispatcher — an existing, sanctioned test factory.

### 5.2 Recommendation

**Primary — S-FS-1: widen `ItemViewer.Breadcrumb.cs:25`'s setter from `private set` to `internal set`.**
One token. No new type, no new file, no csproj change, no public-surface change (the property is already
`internal`, and `QuickFiler/Properties/AssemblyInfo.cs:5` grants `QuickFiler.Test` access). Zero
behaviour change. In-repo precedent: the existing suite already calls `internal` QuickFiler production
members directly, and issue #400's remediation established the pattern of adding narrow internal
testability surface to this very type (`remediation-plan.2026-07-21T21-37.md:725`, the
`BreadcrumbPopupUiOperations` overload).

With S-FS-1 a test is fully isolated:

```
Arrange: viewer = CreateUninitialized<ItemViewer>();
         coordinator = new BreadcrumbBridgeCoordinator(
             new Mock<IWebViewMessenger>().Object,
             new Mock<IFolderHierarchyProvider>().Object,
             BreadcrumbUiDispatcher.CreateForCurrentThreadTests());
         viewer.BreadcrumbCoordinator = coordinator;
```

No `SynchronizationContext`, no `InitializeComponent`, no F12 lifecycle machinery, no `components`
container.

**Fallback — S-FS-2 (zero production change): drive the production initialiser.** If S-FS-1 is rejected
during planning, the same non-null state is reachable with no production edit:

```
previous = SynchronizationContext.Current;
SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
viewer.InitializeBreadcrumbPipeline(providerMock.Object,
                                    BreadcrumbPopupUiOperations.CreateForCurrentThreadTests());
```

`InitializeBreadcrumbPipeline(provider, operations)` (`Breadcrumb.cs:40-60`) is the internal seam issue
#400 added for exactly this purpose, and the ambient-context pattern is established at nine existing call
sites (`BreadcrumbDropDownIntegrationTests.cs:336-338` and siblings). **Cost:** the test then exercises
`EnsureBreadcrumbLifecycle` (`Breadcrumb.cs:253-277`), `BreadcrumbMessengerHub`,
`BreadcrumbCollapsedSurfaceController`, and `EnsureBreadcrumbResourceOwnership` — all F12/F13 code —
which weakens isolation (`.claude/rules/general-unit-test.md` § Core Principles, isolation) and makes an
F12 change able to break an F14 test. **S-FS-1 is preferred on isolation grounds.**

**Rejected — S-FS-3: reflection on the compiler-generated backing field
`<BreadcrumbCoordinator>k__BackingField`.** The repo has a private-field reflection helper
(`QfcThemeHelperTests.cs:287-294`) but it targets *declared* fields. Depending on a compiler-generated
name is brittle and adds no benefit over S-FS-1.

**Rejected — extracting a host-neutral `ItemViewerFolderFacade`.** The delegation targets are already
host-neutral (`BreadcrumbBridgeCoordinator`); a facade would relocate seven null-conditional forwarders
without removing any host dependency, add a file to the denominator under the `>= 90%` new-file rule
(`epic.md:583-585`), and duplicate F12's assignment.

### 5.3 `FocusSearch()` (`:72`) — recommend leaving it uncovered

```csharp
public void FocusSearch() => TxtboxSearch.Invoke(new Action(() => TxtboxSearch.Focus()));
```

`Control.Invoke` requires a created window handle; on a handle-less `TextBox` it throws
`InvalidOperationException` **[I]**, standard .NET Framework behaviour. Covering `:72` therefore needs
either a real HWND (forced via `var _ = textBox.Handle;`) or a seam.

**Recommendation: cover neither. Leave `:72` and its lambda body uncovered and record them as this
file's irreducible residual (2 of ~21 lines).** Arithmetic: 19/21 = **90.5% line**, comfortably over the
80% gate, with **100% branch** (line 72 contains no condition). Rationale:

- A seam here would be an interface seam via `UtilitiesCS.Threading.IUiDispatcher`
  (`UtilitiesCS/Threading/IUiDispatcher.cs:15-18`, `void Invoke(Action action)` — a **public** interface,
  so no internals problem). But adding an `IUiDispatcher` field to `ItemViewer` changes the type's
  construction path and adds production surface to buy one line of coverage the gate does not need.
  General Code Change Policy § Design Principle 1 (simplicity first) argues against it.
- Forcing a real HWND is the only alternative and would be the sole reason to introduce STA scoping to
  `QuickFiler.Test`. That is precisely what the epic's last-resort clause (`epic.md:234-241`) exists to
  prevent when a cheaper answer is available. **See §6.4.**
- The uncovered lambda is the one place open issue **#457** could bite. The lambda captures only `this`,
  so Roslyn emits it as a private instance method on `ItemViewer` (not on a `<>c` display class) **[I]**,
  which means it stays attributed to this file rather than escaping into a compiler-generated type. It
  will appear in the denominator once the type attribute is removed. Budget for it.

The ledger row for this file should record the residual explicitly: *`FocusSearch` marshals through
`Control.Invoke`, which requires a window handle; 2 lines left uncovered by design; branch coverage
unaffected.*

---

## 6. Q6 — Test plan

### 6.1 Fixtures

| ID | Fixture | Use |
|---|---|---|
| **B** (bare) | `CreateUninitialized<ItemViewer>()` with **nothing** assigned | every null-branch case (`BreadcrumbCoordinator` is null, `_breadcrumbLifecycleCoordinator` is null) |
| **C** (coordinator) | fixture **B** plus `viewer.BreadcrumbCoordinator = new BreadcrumbBridgeCoordinator(mockMessenger.Object, mockProvider.Object, BreadcrumbUiDispatcher.CreateForCurrentThreadTests())` via seam **S-FS-1** | every non-null-branch case |
| **S** (search) | fixture **B** plus `viewer.TxtboxSearch = new TextBox()` (public setter, `ItemViewer.cs:392`) | the four search-cluster members |

`CreateUninitialized<T>` precedent and helper: `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:249`
and `:331-335`; the same file assigns `viewer.TxtboxSearch = new TextBox()` at `:254`. Mock behaviour
should be **Loose**, not Strict: `BreadcrumbBridgeCoordinator`'s constructor subscribes to
`IWebViewMessenger.MessageReceived` (`:58`) and constructs a `FolderBreadcrumbBridgeRouter` from the
provider (`:52-54`), and a Strict provider mock would fail on any incidental call.

Event-raise helpers, all with in-repo precedent:
- `TxtboxSearch.TextChanged` / `.KeyDown` — reflect the protected `Control.OnTextChanged(EventArgs)` /
  `Control.OnKeyDown(KeyEventArgs)`, exactly as `QfcItemController.EventWiringTests.cs:266-269` and
  `QfcThemeHelperTests.cs:277-285` already do.
- `FolderSelectionChanged` / `FolderKeyDown` — invoke the private raisers
  `ItemViewer.OnBreadcrumbSelectionChanged()` (`Breadcrumb.cs:239-240`) and
  `OnBreadcrumbFolderArrowKeyDown(BreadcrumbArrowDirection)` (`Breadcrumb.cs:242-248`) by reflection,
  following `QfcItemControllerTestSupport.InvokeNonPublic` (used at
  `QfcItemController.ViewerSetupTests.cs:391-395`).

MSTest `[TestClass]`/`[TestMethod]`, Moq, FluentAssertions, Arrange–Act–Assert, no temp files, no
external services, no live Form, no popup, no `Thread.Sleep`/`Task.Delay`/wall-clock wait, no
`SynchronizationContext` needed under S-FS-1.

**Proposed home:** `QuickFiler.Test/Viewers/ItemViewerFolderSearchForwardingTests.cs` (new). Requires one
`<Compile Include="Viewers\ItemViewerFolderSearchForwardingTests.cs" />` entry in
`QuickFiler.Test/QuickFiler.Test.csproj` (CRLF preserved, minimal adjacent hunk). Projected size at 25
cases with shared fixture helpers: ~420 lines — under 500, but close; if it exceeds, split the search
cluster (F23-F25) into `...Tests.Part2.cs`.

### 6.2 Case inventory

Gate arithmetic: ~21 coverable lines and ~10 condition points (~20 branch outcomes). `>= 80%` line means
`>= 17` lines; `>= 75%` branch means `>= 15` outcomes. The 25 cases below cover **19/21 lines (90.5%)**
and **20/20 branch outcomes (100%)**. Per issue #136 each row is one atomic task.

| # | Test name | Production lines / branches | Fixture | Seam | Mocks |
|---|---|---|---|---|---|
| F1 | `SetFolderItems_BeforePipeline_IsNoOp` | `:20` **`?.` null arm** | B | — | none |
| F2 | `SetFolderItems_WithCoordinator_AppendsItemsToPage` | `:20` **`?.` non-null arm** | C | S-FS-1 | `IWebViewMessenger`, `IFolderHierarchyProvider`; assert via `viewer.GetFolderItems()` |
| F3 | `SetFolderSuggestions_BeforePipeline_IsNoOp` | `:22` **null arm** | B | — | none |
| F4 | `SetFolderSuggestions_WithCoordinator_PublishesRows` | `:22` **non-null arm** | C | S-FS-1 | as F2; supply a `List<FolderRow>` |
| F5 | `GetSelectedFolder_BeforePipeline_ReturnsNull` | `:25` **null arm** | B | — | none. Pins the documented "legacy empty-combo value" contract (`:15-16`) |
| F6 | `GetSelectedFolder_WithCoordinator_ReturnsRouterSelection` | `:25` **non-null arm** | C | S-FS-1 | as F2; seed rows then `SetFolderSelectedIndex` |
| F7 | `SetFolderSelectedIndex_BeforePipeline_IsNoOp` | `:27` **null arm** | B | — | none |
| F8 | `SetFolderSelectedIndex_WithCoordinator_SelectsRow` | `:27` **non-null arm** | C | S-FS-1 | as F2 |
| F9 | `SetFolderSelectedItem_BeforePipeline_IsNoOp` | `:29` **null arm** | B | — | none |
| F10 | `SetFolderSelectedItem_WithCoordinator_SelectsMatchingItem` | `:29` **non-null arm** | C | S-FS-1 | as F2 |
| F11 | `SetFolderDroppedDown_True_BeforePipeline_RoutesToBreadcrumbFocusFallback` | `:31`; also `Breadcrumb.cs:225,227,229,202-205,213-217` | B | — | none. **Must carry an in-code comment citing issue #438** (§3.1) stating the assertion pins current behaviour |
| F12 | `SetFolderDroppedDown_False_BeforePipeline_IsNoOp` | `:31` (re-hit); `Breadcrumb.cs:227` **false arm**, `:231` | B | — | none |
| F13 | `ClearFolderItems_BeforePipeline_IsNoOp` | `:34` **null arm** | B | — | none |
| F14 | `ClearFolderItems_WithCoordinator_EmptiesPage` | `:34` **non-null arm** | C | S-FS-1 | as F2; seed via `SetFolderItems` then assert `GetFolderItems()` is empty |
| F15 | `FocusFolderDropDown_BeforePipeline_DoesNotThrow` | `:36`; `Breadcrumb.cs:202-205,213-217` | B | — | none. `FocusBreadcrumbCore`'s guard short-circuits on `_l0vhBreadcrumb_WebView2 == null` (`Breadcrumb.cs:215`) |
| F16 | `FolderContains_BeforePipeline_ReturnsFalse` | `:38` **`&&` left arm false (short-circuit)** | B | — | none |
| F17 | `FolderContains_WithCoordinatorAndKnownItem_ReturnsTrue` | `:38` **both arms true** | C | S-FS-1 | as F2; seed via `SetFolderItems` |
| F18 | `FolderContains_WithCoordinatorAndUnknownItem_ReturnsFalse` | `:38` **left true, right false** | C | S-FS-1 | as F2 |
| F19 | `GetFolderItems_BeforePipeline_ReturnsEmptyArray` | `:41` **`?.` null arm + `??` right arm** | B | — | none. Asserts `Array.Empty<string>()`, the documented legacy value |
| F20 | `GetFolderItems_WithCoordinator_ReturnsRouterItems` | `:41` **`?.` non-null arm + `??` left arm** | C | S-FS-1 | as F2 |
| F21 | `FolderSelectionChanged_AddThenRemove_TracksHandlerField` | `:46`, `:47`; incidentally `Breadcrumb.cs:239-240` **both arms** | B | reflection on the private raiser `OnBreadcrumbSelectionChanged` | none |
| F22 | `FolderKeyDown_AddThenRemove_TracksHandlerField` | `:54`, `:55`; incidentally `Breadcrumb.cs:242-248` | B | reflection on `OnBreadcrumbFolderArrowKeyDown` | none. **Coordinate with the `ItemViewer.Breadcrumb.cs` plan** (its cases C40-C43 cover the same raisers — see §7, X-F2) |
| F23 | `SearchText_ReturnsSearchTextBoxText` | `:58` | S | `TxtboxSearch` setter | none |
| F24 | `SearchTextChanged_AddThenRemove_SubscribesAndUnsubscribesTextBoxTextChanged` | `:62`, `:63` | S | `TxtboxSearch` setter + reflected `OnTextChanged` | none |
| F25 | `SearchKeyDown_AddThenRemove_SubscribesAndUnsubscribesTextBoxKeyDown` | `:68`, `:69` | S | `TxtboxSearch` setter + reflected `OnKeyDown` | none |

**Not authored:** `FocusSearch()` (`:72` + lambda) — §5.3. Two lines, no branches, recorded as the ledger
residual.

### 6.3 Cases the planner should NOT author

- **Do not** author a `FocusSearch` case that forces a window handle (`var _ = textBox.Handle;`). It
  buys 2 of 21 lines, introduces the first real HWND into `QuickFiler.Test`, and is the only thing that
  would push this file toward STA scoping.
- **Do not** author per-member `NullReferenceException` cases for the search cluster. The null-control
  path throws from the same line the positive case already covers.
- **Do not** assert on `BreadcrumbBridgeCoordinator`'s internal ordering or on `FolderBreadcrumbBridgeRouter`
  behaviour. Those are F12's coverage obligation; F14 asserts only that the forwarder reached them.

### 6.4 STA determination

**No case requires the STA clause, and none should be created.** Evidence, not argument:

- Fixtures **B** and **C** construct no WinForms control at all (`GetUninitializedObject` runs no
  constructor). Fixture **S** constructs one `System.Windows.Forms.TextBox` with no handle, no parent,
  and no `Show()`.
- Nine existing `QuickFiler.Test` classes construct the **entire** `ItemViewer` Designer tree in plain
  `[TestClass]`es (`QfcItemController.EventWiringTests.cs:236`, `ViewerSetupTests.cs:386`,
  `Viewers/BreadcrumbDropDownIntegrationTests.cs:338`, `BreadcrumbCoordinatorLifecycleTests.cs:477`,
  `BreadcrumbCollapsedSurfaceReadinessTests.cs:413`, `BreadcrumbPendingOpenCloseTests.cs:363`,
  `BreadcrumbSelectorOpenRetryTests.cs:255`, `BreadcrumbSubfolderActivationTests.cs:305`,
  `QfcItemControllerBreadcrumbDropDownTests.cs:373`) — strictly more host machinery than any case above.
- There is **no `[STATestClass]` anywhere in `QuickFiler.Test`**; the repo's only STA files are
  `Tags.Test/CheckBoxControllerWiring.StaTests.cs` and
  `TaskVisualization.Test/TaskControllerAccelerator.StaTests.cs`.
- The single member that could argue for STA (`FocusSearch`, `:72`) is deliberately left uncovered
  (§5.3), and the file still clears both gates at 90.5% line / 100% branch.

**Do not create the first `*.StaTests.cs` in `QuickFiler.Test` for this file.**

---

## 7. Q7 — 500-line rule, and cross-child notes

### 7.1 File size

- Current: **74 physical lines**. Limit 500. Headroom **426 lines**.
- Projected additions to *this* file: **zero**. Seam S-FS-1 changes one token in
  `ItemViewer.Breadcrumb.cs:25` (that file: 298 -> 298 lines).
- **Projected post-refactor: 74 lines. No split. No `<Compile Include>` addition to
  `QuickFiler/QuickFiler.csproj`. No mid-wave ledger row for a new production file** (`epic.md:560-587`
  is not engaged).

### 7.2 Cross-child notes

**F14 requires ZERO signature changes in any F3, F10, F12, or F13 file.** Every collaborator is reachable
at its current visibility.

| Symbol | Declared in | Owner | Sufficient as-is? |
|---|---|---|---|
| `BreadcrumbBridgeCoordinator` + `AddItems`/`SetSuggestions`/`GetSelectedFolder`/`SelectRow`/`SelectItem`/`Clear`/`Contains`/`GetFolderItems` | `Viewers/BreadcrumbBridgeCoordinator.cs:25,100,131,150,175,184,190,193,196` | **F12** | **YES** — all `public`; the internal 3-arg ctor at `:45` is reachable via `InternalsVisibleTo`. No change requested. |
| `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` | `Viewers/BreadcrumbUiDispatcher.cs:62-65` | **F13** | **YES** — `internal static`. No change requested. |
| `BreadcrumbPopupUiOperations.CreateForCurrentThreadTests()` | `Viewers/BreadcrumbPopupUiOperations.cs:83-84` | **F13** | **YES** (only needed under fallback S-FS-2). |
| `IWebViewMessenger` | `Viewers/IWebViewMessenger.cs` | **F13** | **YES** — mockable interface. |
| `IFolderHierarchyProvider`, `FolderRow` | `UtilitiesCS/OutlookObjects/Folder/` | outside the epic | **YES** — both public. |
| `TxtboxSearch` wrapper property | `ItemViewer.cs:389-393` | **F14 (own)** | yes — `public` get/set |
| `SetBreadcrumbDropDownState`, `FocusBreadcrumb`, `BreadcrumbCoordinator`, `_folderSelectionChangedHandlers`, `_folderKeyDownHandlers`, `OnBreadcrumbSelectionChanged`, `OnBreadcrumbFolderArrowKeyDown` | `ItemViewer.Breadcrumb.cs:25,34,35,200,223,239,242` | **F14 (own)** | yes, subject to X-F1 below |
| `QfcItemController.EventHandlers.cs`, `.EventWiring.cs`, `.Navigation.cs` call sites | `QuickFiler/Controllers/` | **F10** (issue #453) | consumer only; **no edit requested** |
| `IQfcKeyboardHandler.CboFolders_KeyDownAsync` | F10/F3 | **F3/F10** | not touched — bound controller-side only |

**X-F1 (intra-F14, requires reconciliation with the `ItemViewer.Breadcrumb.cs` plan).** Seam **S-FS-1**
edits `ItemViewer.Breadcrumb.cs:25`, changing `{ get; private set; }` to `{ get; internal set; }`. That
file is F14-owned but was researched by a sibling researcher whose artifact
(`research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md:761-772`) proposes seams S-1 and S-2 in the same
file and does not contemplate this one-token change. The two plans must be merged into a single edit list
for `ItemViewer.Breadcrumb.cs`; the changes are non-overlapping (line 25 versus lines 77-98 and 142-177),
so this is a coordination item, not a conflict. **If the F14 plan declines S-FS-1, fall back to S-FS-2 and
record the isolation cost.**

**X-F2 (intra-F14, duplicate-coverage warning).** Cases F11, F12, F15, F21, and F22 execute lines in
`ItemViewer.Breadcrumb.cs` (`:200-209`, `:213-221`, `:223-235`, `:239-240`, `:242-248`) that the
breadcrumb artifact's cases C29-C36 and C40-C43 also target. Because both files are F14-owned, the
duplication is a wasted-effort risk rather than a correctness risk: **after the attribute at
`ItemViewer.cs:20` is removed and the F1 harness is run, prune whichever set the measured data shows is
redundant.** This is the same "measure before authoring" directive the breadcrumb artifact issues at its
§7.2 (T0/T0b).

**X-F3 (advisory, F12/F13 -> F14 — freeze request).** `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()`
(`BreadcrumbUiDispatcher.cs:62-65`) is fixture **C**'s only mechanism for constructing a
`BreadcrumbBridgeCoordinator` without an ambient `SynchronizationContext`. `BreadcrumbBridgeCoordinator`'s
three-argument internal constructor (`:45-59`) is the other. If F12 or F13's own coverage work removes,
renames, or reorders either, every non-null-branch case in §6.2 breaks. **Record both as frozen
contracts.** This extends the X-1/X-2 freeze requests the breadcrumb artifact already made
(`research.itemviewer-breadcrumb-cs.2026-08-07T22-05.md:492-501`) to two additional symbols.

**X-F4 (advisory, F14 -> whoever schedules #438).** A fix for #438 will change the behaviour asserted by
case F11, and possibly by F12. Whoever schedules #438 should expect exactly one or two red F14 tests and
should treat them as the intended signal. The precise localisation is in §3.1.

---

## 8. Latent defect promotion candidates

Promotion candidates for the MCP lifecycle per `epic.md:538-543`. All are out of scope to fix under
F14's no-behavior-change NFR.

### LD-1 — `SetFolderItems` appends rather than sets, contradicting its own name and its `IItemViewer` contract

`ItemViewer.FolderSearch.cs:20` is `public void SetFolderItems(string[] items) =>
BreadcrumbCoordinator?.AddItems(items);`. The target is `BreadcrumbBridgeCoordinator.AddItems`
(`BreadcrumbBridgeCoordinator.cs:131`), whose own XML summary at `:130` reads *"Appends Path B plain rows
verbatim and re-renders (legacy AddRange semantics)."* So a second `SetFolderItems` call **adds to** the
existing page rather than replacing it. `IItemViewer.cs:80` declares the member as `SetFolderItems`, and
the file's own comment at `:14-15` claims the legacy semantics are *"preserved bit-for-bit"*. The defect
is masked in production only because the sole caller pairs it with an immediate clear:
`QfcItemController.EventHandlers.cs:172-173` calls `ClearFolderItems()` then `SetFolderItems(folders)`.
Any future caller that omits the clear — or any reordering of those two lines — silently accumulates
duplicate folder rows across keystrokes. Either rename the member to `AddFolderItems` (an `IItemViewer`
contract change) or make it clear-then-add; both are behaviour changes and out of scope here.

### LD-2 — `FocusSearch` and `FocusSubject` use incompatible threading discipline on the same control

`ItemViewer.FolderSearch.cs:72` marshals through `TxtboxSearch.Invoke(new Action(() =>
TxtboxSearch.Focus()))`, while the sibling member `ItemViewer.DisplayState.cs:79` calls
`LblSubject.Focus()` with no marshalling at all. Both are `IItemViewer` members
(`IItemViewer.cs:100`, `:54`) on the same `UserControl`, called from the same controller
(`QfcItemController.Navigation.cs:54` and `QfcItemController.MailActions.cs:64` respectively). Three
concrete consequences: (a) `Control.Invoke` throws `InvalidOperationException` if the handle has not yet
been created, so `FocusSearch` is unsafe during viewer construction/pooled reuse while `FocusSubject` is
not; (b) `Control.Invoke` is a **blocking** synchronous marshal, so calling it from a non-UI thread while
the UI thread is inside a modal Outlook dialog deadlocks — a real hazard in a VSTO add-in; (c) the two
members give a reader contradictory signals about whether `IItemViewer` is thread-affine. The type should
pick one discipline. Note the repo already has the right abstraction available:
`UtilitiesCS.Threading.IUiDispatcher` (`UtilitiesCS/Threading/IUiDispatcher.cs:15-18`) exposes
`void Invoke(Action)` / `Task InvokeAsync(Action)` and is already mocked in `QuickFiler.Test`
(`QfcThemeHelperTests.cs:301`). Review alongside **#438**, which concerns focus behaviour on the same
textbox.

### LD-3 — Nullability annotation is silently erased at the `ItemViewer` boundary

`BreadcrumbBridgeCoordinator.cs:1` declares `#nullable enable` and `:190` declares
`public string? GetSelectedFolder()`. `ItemViewer.FolderSearch.cs` has **no** `#nullable` directive and
`:25` declares `public string GetSelectedFolder() => BreadcrumbCoordinator?.GetSelectedFolder();` — a
non-annotated `string` that returns `null` on two distinct paths (no coordinator, or no selection).
`IItemViewer.cs:87` likewise declares `string GetSelectedFolder();`. The null therefore crosses into
nullable-oblivious code with no compiler signal. The immediate consumer merely stores it
(`QfcItemController.EventHandlers.cs:211`, `_selectedFolder = _itemViewer.GetSelectedFolder();`) so no
NRE was traced at that site; **downstream consumption of `_selectedFolder` was not traced in this
research and should be checked when the issue is triaged.** The same erasure applies to any other
nullable-returning coordinator member surfaced through this file. Confidence: the annotation loss is
verified; the downstream impact is not.

### LD-4 — Unused `using System.Linq;`

`ItemViewer.FolderSearch.cs:3` imports `System.Linq`, but no LINQ operator appears anywhere in the file
(`Array.Empty<string>()` at `:42` is `System.Array`). This is an IDE0005 candidate and a leftover from
the decommissioned `CboFolders` code the header comment describes. **In-scope for F14's own execution** as
a trivial cleanup, not a promotion candidate. Note the removal is safe only if the analyzer configuration
does not treat it as a public-API-affecting change — it does not.

### LD-5 — Event accessors, backing fields, and raisers for the same two events are split across three regions of two partials

`FolderSelectionChanged` and `FolderKeyDown` have their `add`/`remove` accessors in
`ItemViewer.FolderSearch.cs:44-56`, their private delegate fields in `ItemViewer.Breadcrumb.cs:34-35`, and
their raisers in `ItemViewer.Breadcrumb.cs:239-248`. A reader of either file alone sees an incomplete
picture, and a coverage reader attributing lines per file sees the subscription in one file and the raise
in another. This is a cohesion defect introduced when #351 moved the breadcrumb pipeline; the natural
correction is to co-locate all three in whichever partial owns the event. Cosmetic, no behaviour change,
and deliberately deferred: moving the fields would touch two files that are both under active F14
research (X-F1/X-F2).

### LD-6 — Stale exemption comment at `ItemViewer.FolderSearch.cs:17`

Identical in kind to the comments at `ItemViewer.Commands.cs:10` and `ItemViewer.DisplayState.cs:9-10`.
These three comments produced the epic's original 33-file over-count (`epic.md:121-130`). **In-scope for
F14's own execution** — update all three in the change that removes `ItemViewer.cs:20`.

---

## 9. Open-issue scan

**Method:** WebFetch against the public GitHub issue pages for `drmoisan/TaskMaster` (no Bash tool, so no
`gh`). Terms run: `ItemViewer`, `focus OR viewer OR "folder search"`, `coverage`. Issues fetched in full:
**#438**, **#444**, **#445**, **#457**.

| Issue | Title | Bearing on `ItemViewer.FolderSearch.cs` |
|---|---|---|
| **#438** | `quickfiler-search-keystroke-focus-steal` **[V-web]**, open, `bug`, High/Blocker | **Direct and central.** Full chain analysis in §3.1. The per-keystroke focus steal enters via this file's `:31-32`. F14 must not fix it, and must annotate cases F11/F12 so the fix produces a legible red test. |
| **#457** | `excludefromcodecoverage-does-not-suppress-nested-lambdas` **[V-web]**, open, `bug` | **Direct, uniquely among the three files this researcher covers.** This is the only one of the three that contains a lambda (`:72`). The lambda captures only `this`, so Roslyn emits it as an instance method on `ItemViewer` rather than on a `<>c` display class **[I]**, meaning #457's display-class escape does not apply — but the line still enters the denominator once the type attribute is removed. It is one of the two lines §5.3 deliberately leaves uncovered. Also supporting evidence for the wider F14 decision that attribute-based suppression is unreliable. |
| **#441** | Cobertura post-processing double-counts `<line>` nodes | Direct. Annotate any quoted `line-rate` as "#441 — unreliable"; use F1's recomputed per-file figure. **This file's branch data is the part to trust** — the sibling artifact verified that `branch-rate`/`<condition>` entries match the raw data even where `line-rate` is corrupt. |
| **#432** | `quickfiler-coverage-ledger` (F1) | Direct. This file needs a `testable` ledger row recording the `FocusSearch` residual (§5.3). |
| **#440** | `breadcrumb-left-right-arrow-parent-child-navigation` | **Adjacent.** It targets `OnBreadcrumbFolderArrowKeyDown` (`Breadcrumb.cs:242-248`), which is the raiser behind this file's `FolderKeyDown` (`:52-56`). Case F22 should assert only *that the handler field is invoked*, not the `Keys.Left`/`Keys.Right` mapping, so a #440 fix does not break it. The mapping itself is pinned by the breadcrumb artifact's C41/C42. |
| **#462** | `breadcrumb-dropdown-coordinator-stale-closepending-drops-reopen` | Adjacent; F13/F12 territory. Reached from this file only via `SetFolderDroppedDown` -> `SetBreadcrumbDropDownState` -> `_breadcrumbLifecycleCoordinator.SetDroppedDown`. All §6.2 cases use the **null-coordinator** fallback path, so none of them touch the code #462 concerns. No conflict. |
| **#458** | `webview2breadcrumbhost-handler-retention-pooled-viewer` | F13 territory; no path from this file's tests. |
| **#444**, **#445** | keyboard-action contract defects | **No bearing** — §4, zero-match grep over `QuickFiler/Viewers/`. |
| **#230** | WinForms message-pump test seam | **Would matter only if `FocusSearch` (`:72`) were to be covered.** §5.3 recommends leaving it uncovered, so #230 is not a dependency. If a later reviewer insists on covering `:72`, check #230's state first and prefer a pump seam over STA. |

No open issue targets `ItemViewer.FolderSearch.cs` by name.

---

## 10. Verified vs inferred

**Verified:**

- The file's full contents, its 17 members, its five `using` directives (and that `System.Linq` is
  unused), its ~10 condition points, and its single lambda.
- That it carries no real `[ExcludeFromCodeCoverage]` and contains no `Microsoft.Office.Interop.Outlook`
  reference.
- That `BreadcrumbBridgeCoordinator` is `public sealed` (unmockable) with `public` non-virtual delegation
  targets at `:100,131,150,175,184,190,193,196`, an `internal` 3-arg constructor at `:45`, and an in-code
  claim of host-neutrality at `:13-24`.
- That `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` (`:62-65`) and
  `BreadcrumbPopupUiOperations.CreateForCurrentThreadTests()` (`:83-84`) exist and are `internal static`.
- That `ItemViewer.BreadcrumbCoordinator` is `internal ... { get; private set; }` at
  `ItemViewer.Breadcrumb.cs:25`, and that the private setter is written only at `:59`.
- The complete #438 call chain: `EventWiring.cs:77-79` -> `EventHandlers.cs:164-178` (`:172`, `:173`,
  `:176`, `:177`) -> this file `:34`, `:20`, `:27`, `:31-32` -> `Breadcrumb.cs:223-235` -> `:200-209` ->
  `:211-221`.
- That `QuickFiler/Viewers/` contains no reference to `KbdActions`, `KaChar`, `KaKey`, `KaStringAsync`, or
  `IMailItemActions`.
- That `_folderSelectionChangedHandlers`/`_folderKeyDownHandlers` are declared in
  `ItemViewer.Breadcrumb.cs:34-35` and raised at `:239-248`.
- That `QfcThemeHelperTests.cs:249,254,277-294,331-335` supplies the uninitialised-viewer fixture, a
  `TxtboxSearch` assignment, the protected-event reflection helper, and the private-field setter.
- The compile entry at `QuickFiler.csproj:427-430`.
- Issue text for #438, #444, #445, #457 (WebFetch).

**Inferred** (reasoning, not executed — no code-execution tool was available):

- That `Control.Invoke` on a handle-less `TextBox` throws `InvalidOperationException` (§5.3). This is the
  premise for leaving `:72` uncovered; if it proves wrong, `:72` becomes trivially coverable and the
  file's rate rises, so the plan is safe either way.
- That the `() => TxtboxSearch.Focus()` lambda is emitted as an instance method on `ItemViewer` rather
  than on a `<>c` display class, because it captures only `this` (§9, #457 row).
- That `Control.IsDisposed` reads `false` on an object produced by `FormatterServices.GetUninitializedObject`
  (all fields zeroed), which is what makes cases F11/F15 safe when they reach
  `Breadcrumb.cs:213-217`. Fallback if wrong: assign `viewer.L0vhBreadcrumb_WebView2 =
  CreateUninitialized<WebView2>()`, exactly as `QfcThemeHelperTests.cs:256` does.
- The exact line-to-sequence-point mapping for the four two-line expression-bodied members, and hence the
  precise value "21 coverable lines" (§2). The 90.5% figure is therefore approximate; the margin over the
  80% gate is large enough that the conclusion does not depend on it.
