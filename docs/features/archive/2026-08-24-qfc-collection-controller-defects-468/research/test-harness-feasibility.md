# Test-Harness Feasibility Research — `QfcCollectionController` defects (Issue #468)

- Feature: `docs/features/active/qfc-collection-controller-defects-468`
- Date: 2026-08-24
- Scope: what is testable, how, and which minimal production seams are required
- Method: direct reading of production and test sources in the isolated worktree; every claim below
  carries a `file:line` citation verified by reading the file.

All paths are relative to the worktree root
`<repo-root>`, an isolated agent worktree under `<user-profile>\repos\TaskMaster\.claude\worktrees\`.

---

## 1. How the existing tests construct and exercise `QfcCollectionController`

There are exactly two existing test files for this controller, and they use **two different and
complementary construction strategies**. Both are already wired into the test project
(`QuickFiler.Test/QuickFiler.Test.csproj:116-117`).

### 1.1 Strategy A — `FormatterServices.GetUninitializedObject` + reflection field injection

File: `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`.

The class docstring states the rationale directly
(`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:17-22`): the constructor requires
WinForms UI components, so the instance is allocated without running the constructor and the
required private fields are then set by reflection.

Canonical shape (`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:36-37`):

```csharp
var controller = (QfcCollectionController)
    FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));
```

Field injection helper (`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:380-383`):

```csharp
private static void SetControllerField(object target, string name, object value) =>
    typeof(QfcCollectionController)
        .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
        ?.SetValue(target, value);
```

Note the `?.` — a typo in the field name silently no-ops rather than failing. The sibling
`QfcItemControllerTestSupport.SetField` at
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:37-47` is the better pattern: it
asserts `field.Should().NotBeNull(...)` before setting. New helpers for this feature should follow
the `QfcItemController.TestSupport.cs` form.

Three purpose-built builders exist in this file:

| Builder | Lines | Fields injected | Purpose |
|---|---|---|---|
| `CreateControllerWithOneGroup` | `:30-74` | `_itemGroupsToMove` (a `ConcurrentDictionary<QfcItemGroup,int>` holding one group) | `GetMoveDiagnostics` tests |
| `CreateControllerWithGroups` | `:142-183` | `_itemGroups` (list of groups), `_removeGroupByEntryId` (delegate seam) | `RemoveBelowThresholdAsync` tests |
| `CreateControllerForSwap` | `:338-365` | `_kbdHandler`, `_moveMonitor`, `_formViewer`, `_digits`, `_itemGroups` | navigation register/unregister tests |

Mocks used, with the exact setup calls:

- `Mock<MailItemHelper>(MockBehavior.Loose)` with `SetupGet` on `Subject`, `SenderName`,
  `ToRecipientsName`, `SentDate` (`:40-44`). `MailItemHelper` is a class, and Moq mocks it
  successfully today, so it is non-sealed with virtual members.
- `Mock<IQfcItemController>(MockBehavior.Loose)` with `SetupGet` on `ItemHelper`, `SelectedFolder`
  (`:46-48`), and on `TopFolderScore` (`:157`).
- `Mock<MailItem>(MockBehavior.Loose)` with `SetupGet(x => x.EntryID)` (`:153-154`, `:373-374`).
  `Microsoft.Office.Interop.Outlook.MailItem` is a COM **interface**, so Moq mocks it directly with
  no live Outlook.
- `Mock<IQfcKeyboardHandler>`, `Mock<IEmailMoveMonitor>`, `Mock<IQfcFormViewer>` (`:348-356`). Note
  `formViewer.SetupGet(x => x.L1v0L2L3v_TableLayout).Returns((TableLayoutPanel)null)` at `:354-356`
  — a **null** TableLayoutPanel is deliberately returned so no WinForms control is created.
- A **real** `KbdActions<string, KaStringAsync, Func<string, Task>>` (`:346`) rather than a mock,
  because the tests assert on its contents.

`QfcItemGroup`'s members are `internal` (`QuickFiler/Controllers/QfcItemGroup.cs:26,32,39,50`) and
`QuickFiler/Properties/AssemblyInfo.cs:5` declares
`[assembly: InternalsVisibleTo("QuickFiler.Test")]`, so `new QfcItemGroup { MailItem = ..., }` and
`group.ItemController = ...` are directly assignable from the test project. The reflection detour at
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:51-64` (setting `_itemController` by
field) is **not required**; `QfcCollectionControllerDarkModeTests.cs:74` already uses the direct
form `new QfcItemGroup { ItemController = mockItemController.Object }`. Prefer the direct form.

An important documented pitfall is recorded at
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:334-337`: because
`GetUninitializedObject` bypasses field initializers, `_digits` is `0` rather than its declared `1`
(`QuickFiler/Controllers/QfcCollectionController.cs:113`), and the `Digits` getter
(`QuickFiler/Controllers/QfcCollectionController.cs:114-128`) then sets `_digitRefreshNeeded = true`,
which routes `RegisterNavigation` into the WinForms-bound `SetVisualDigits` path
(`QuickFiler/Controllers/QfcCollectionController.cs:1332-1336`). Any new test that reaches
`RegisterNavigation`, `UnregisterNavigation`, or `RemoveSpecificControlGroupAsync` must inject
`_digits = 1` unless it *wants* that path. The same caveat applies to
`_moveMonitor = new EmailMoveMonitor()` (`QuickFiler/Controllers/QfcCollectionController.cs:78`) and
`BackgroundLoadingTasks = []` (`:80`), both of which are **null** on an uninitialized instance.

### 1.2 Strategy B — the real constructor with fully mocked collaborators

File: `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs`.

`CreateController` (`:31-60`) calls the real constructor:

```csharp
return new QfcCollectionController(
    mockGlobals.Object,        // Mock<IApplicationGlobals>, SetupGet(g => g.Ol) -> Mock<IOlObjects>
    mockFormViewer.Object,     // Mock<IQfcFormViewer>
    QfEnums.InitTypeEnum.Sort,
    mockHomeController.Object, // Mock<IFilerHomeController>, SetupGet(h => h.KeyboardHandler)
    mockParent.Object,         // Mock<IFilerFormController>
    tokenSource,               // real CancellationTokenSource
    token,                     // real CancellationToken
    new TlpCellStates()        // real, side-effect-free
);
```

This works because every constructor parameter is an interface or a side-effect-free concrete type,
and because the two WinForms-typed reads in the constructor body
(`QuickFiler/Controllers/QfcCollectionController.cs:44-45`) come from the *mocked* `IQfcFormViewer`
and therefore return `null` under a default `Mock<IQfcFormViewer>`. The constructor's only other
side effect is `SetupLightDark(_globals.Ol.DarkMode)` (`:52`), which subscribes to
`_globals.Ol.PropertyChanged` (`:2117`) — a mockable `INotifyPropertyChanged` event that the test
raises with `mockOl.Raise(...)` (`QfcCollectionControllerDarkModeTests.cs:112-116`).

### 1.3 STA / apartment handling in the existing files

**Neither existing file uses any STA attribute, `[STAThread]`, or a pump.** Both run on the default
MSTest MTA thread. This is a direct consequence of never materialising a real `TableLayoutPanel`
(`QfcCollectionControllerTests.cs:354-356` returns null) and never constructing an `ItemViewer`.

### 1.4 Cheapest legal construction path

`FormatterServices.GetUninitializedObject` + targeted field injection (Strategy A) is the cheapest
and is what the majority of the defects below should use. Strategy B is only necessary when the test
must observe constructor-installed state (the dark-mode subscription).

---

## 2. Constructor requirements

There is exactly **one** constructor
(`QuickFiler/Controllers/QfcCollectionController.cs:30-53`).

| # | Parameter | Type | Classification | Notes |
|---|---|---|---|---|
| 1 | `AppGlobals` | `IApplicationGlobals` | Interface — Moq | Must have `.Ol` non-null: `:52` calls `SetupLightDark(_globals.Ol.DarkMode)` and `:2117` subscribes to `_globals.Ol.PropertyChanged`. `Mock<IOlObjects>` supplies a raisable event. |
| 2 | `viewerInstance` | `IQfcFormViewer` | Interface — Moq | `:44-45` read `L1v0L2L3v_TableLayout` (`TableLayoutPanel`) and `L1v0L2_PanelMain` (`Panel`). A default `Mock<IQfcFormViewer>` returns `null` for both — that is acceptable and is what the existing tests rely on. |
| 3 | `InitType` | `QfEnums.InitTypeEnum` | Enum | Trivial. |
| 4 | `homeController` | `IFilerHomeController` | Interface — Moq | `:49` reads `.KeyboardHandler`; supply `Mock<IQfcKeyboardHandler>`. |
| 5 | `parent` | `IFilerFormController` | Interface — Moq | Stored at `:50`. Note the `(QfcFormController)_parent` downcast at `:1232` (issue #474 defect 1) — a Moq substitute throws `InvalidCastException` there. |
| 6 | `tokenSource` | `CancellationTokenSource` | Concrete, side-effect-free | Real instance. |
| 7 | `token` | `CancellationToken` | Struct | Real. |
| 8 | `tlpStates` | `TlpCellStates` | Concrete, side-effect-free | Real instance (`QfcCollectionControllerDarkModeTests.cs:58`). |

**No parameter is a concrete Outlook Interop type and no parameter is a WinForms control.** The
constructor therefore requires neither a live Outlook process nor an STA apartment. The WinForms
coupling is entirely *inside the method bodies*, via the private fields `_itemTlp`
(`:68`, a `TableLayoutPanel`), `_itemPanel` (`:67`, a `Panel`), `_template`/`_templateExpanded`
(`:73-74`, `RowStyle`), and via `ItemViewerQueue.Dequeue` (`:617`, `:958`) and
`UiThread.Dispatcher` (`:1195`, `:1226`, `:1472`, `:1500`, `:1518`, `:1595`).

### 2.1 The `UiThread.Dispatcher` hard static

`UiThread.Dispatcher` is a static WPF `Dispatcher` (`UtilitiesCS/Threading/UiThread.cs:135-140`)
whose backing field is `null!` until `UiThread.Init()` runs. `Init()` calls `Initialize()`
(`UtilitiesCS/Threading/UiThread.cs:48-79`), which constructs a `SyncContextForm` and calls
`.Show()` at `:54`. **Calling `UiThread.Init()` from a test would display a window and is therefore
prohibited by the repository unit-test policy.** Consequently any test that reaches
`await UiThread.Dispatcher.InvokeAsync(...)` will throw `NullReferenceException`. That is a
constraint for most defects — and, usefully, the mechanism that makes the #286 test cheap (§3.1).

---

## 3. Per-defect testability triage

Legend: **Direct** = deterministic MSTest regression test with no new production seam.
**Seam-first** = a minimal production seam is required before a legal test can exist.
**Not achievable** = no deterministic test is available; document a fail-before exception dossier.

### 3.1 #286 — `RemoveSpecificControlGroupAsync` reentrancy-counter leak — **Direct**

Code: counter declared `private static int removespecificcontrolgroupcounter = 0` at
`QuickFiler/Controllers/QfcCollectionController.cs:1157`; incremented at `:1161`; compared at
`:1237`; decremented at `:1247`, the method's final statement, with no `try`/`finally` anywhere
between `:1161` and `:1247`.

**Observing the counter.** The field is `private static`, so `InternalsVisibleTo` does not reach it.
Use reflection:

```
typeof(QfcCollectionController).GetField(
    "removespecificcontrolgroupcounter",
    BindingFlags.NonPublic | BindingFlags.Static)
```

This is the smallest option and needs no production change. Two alternatives, both larger and not
recommended: change the field to `internal static` (a production visibility change, but one that
`InternalsVisibleTo("QuickFiler.Test")` at `QuickFiler/Properties/AssemblyInfo.cs:5` would then make
directly readable), or add an `internal static int ReentrancyCounter => ...` accessor.

**Forcing a throw between increment and decrement (no mocking of anything hard).** Allocate an
uninitialized controller. `_itemGroups` is then `null`. `:1161` increments, then `:1162` calls
`UnregisterNavigation()`, whose first statement is `for (int i = 0; i < _itemGroups.Count; i++)`
(`:1345`) and throws `NullReferenceException`. The throw is unambiguously *after* the increment.

Test sketch:

1. `[TestInitialize]` and `[TestCleanup]` reset the static field to `0` — mandatory, because the
   field is process-wide shared state and the General Unit Test Policy requires independence.
2. Read the pre-call value.
3. `await controller.Invoking(c => c.RemoveSpecificControlGroupAsync(1)).Should().ThrowAsync<NullReferenceException>();`
4. Assert the field equals the pre-call value. **Fails before the fix** (it is pre-call + 1),
   **passes after** the `try`/`finally`.

A second test should assert the same invariant for a throw *later* in the body, e.g. by injecting
`_itemGroups` with one group whose `Mock<IQfcItemController>` throws from `IsActiveUI` (`:1165`), so
the fix is proven to cover the whole span and not just the first statement.

No STA, no COM, no UI, no sleeps.

### 3.2 #471 — `EliminateSpaceForItems` sign error — **Direct, but STA**

Code: `EliminateSpaceForItems` at `QuickFiler/Controllers/QfcCollectionController.cs:2013-2027`.
Line `:2017` assigns a **negative** magnitude and lines `:2020` and `:2025` **subtract** it.
`MakeSpaceForItems` at `:2029-2042` computes a positive magnitude and adds.

Reachability: both methods are **public and on the interface**
(`QuickFiler/Interfaces/IQfcCollectionController.cs:47` and `:87`), so no reflection is needed to
call them.

`_itemTlp` (`:68`) and `_template` (`:73`) are private fields, both injectable by the established
reflection helper. `RowStyle` is a plain WinForms data object, not a control:
`new RowStyle(SizeType.Absolute, 30f)` is free. `TableLayoutPanel` **is** a control.

Two ordering facts matter:

- `EliminateSpaceForItems` calls `TableLayoutHelper.RemoveSpecificRow(_itemTlp, removalIndex, removalCount)`
  **first** (`:2015`). `RemoveSpecificRow` early-returns when `rowIndex >= panel.RowCount`
  (`UtilitiesCS/HelperClasses/Windows Forms/TableLayoutHelper.cs:68-71`), so a test can pass a
  removal index at or beyond `RowCount` and isolate the size arithmetic entirely.
- `RemoveSpecificRow` evaluates `panel.InvokeRequired` at
  `UtilitiesCS/HelperClasses/Windows Forms/TableLayoutHelper.cs:62` before that early return, and
  `MakeSpaceForItems` reaches `InsertSpecificRow`, which does the same at `:21`. A handle-less
  control's `InvokeRequired` resolves through the WinForms marshaling control, so a window handle is
  created on the calling thread.

**Repository precedent is unambiguous**: `UtilitiesCS.Test/HelperClasses/WindowsForms/ScreenAndTableLayoutTests.cs:41`
marks `TableLayoutHelper_Tests` `[STATestClass]` and constructs bare `new TableLayoutPanel()`
instances at `:47`, `:61`, `:75`. `UtilitiesCS.Test/test.runsettings:2-5` records the standing
decision that global STA is disabled and STA tests must opt in per-method/per-class. The
maintainer-ratified refinement at `docs/features/epics/winforms-testability-refactor/epic.md:62-74`
permits in-memory, never-shown WinForms **controls** on STA as a last resort, requires the tests to
live in a dedicated `*.StaTests.cs` file marked `[STATestClass]`/`[STATestMethod]`, forbids
`Show()`/`ShowDialog()` and message-pump reliance, and requires per-test disposal.

Recommended test set, in a new `QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs`
marked `[STATestClass]`:

1. Inject `_itemTlp = new TableLayoutPanel { MinimumSize = new Size(W, 300), Size = new Size(W, 300) }`
   and `_template = new RowStyle(SizeType.Absolute, 30f)`. Call
   `EliminateSpaceForItems(removalIndex: <RowCount>, removalCount: 2)`. Assert
   `MinimumSize.Height == 240` and `Size.Height == 240`. **Fails before the fix** (both become 360).
2. Height-neutrality: record the two heights, call `MakeSpaceForItems(0, 3)` then
   `EliminateSpaceForItems(0, 3)`, assert both heights returned to the recorded values.
3. `Dispose()` the panel in `[TestCleanup]`.

**Seam alternative that avoids STA entirely.** Extract the arithmetic into a pure internal helper,
for example `internal static Size ShrinkByRows(Size current, float templateHeight, int removalCount)`,
and have both `:2018-2026` and `:2031-2034` call the shared helper. The unit test is then pure and
runs MTA. This is strictly better under the epic rule "seams remain the required first approach"
(`epic.md:66-67`), at the cost of not proving the caller applies the helper with the correct sign.
**Recommendation: do both** — the pure helper carries the arithmetic assertions, and one STA test
carries the end-to-end panel assertion that the sign is applied correctly at the call site.

### 3.3 #470 defect 3 — `SetVisualDigits` with a null `ItemController` — **Direct**

Code: `private void SetVisualDigits(int digits)` at
`QuickFiler/Controllers/QfcCollectionController.cs:130-146`. Line `:140` dereferences
`grp.ItemController.ItemNumberDigits` unguarded; `:141-142` guard the *same* object with `?.`.

**Reaching the method.** It is `private`, so `InternalsVisibleTo` does not help. Use reflection, in
the shape already established at
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:66-80`
(`GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)` then `Invoke`). Reflection wraps
the thrown exception, so the assertion must be
`.Should().Throw<TargetInvocationException>().WithInnerException<NullReferenceException>()`, or the
test must unwrap `ex.InnerException` explicitly.

Arrangement: uninitialized controller, `_itemGroups = new List<QfcItemGroup> { new QfcItemGroup() }`
(so `ItemController` is `null` and `EmailsLoaded == 1`, passing the `> 0` guard at `:132`).
Pre-fix: `NullReferenceException` at `:140`. Post-fix: no throw.

**Design constraint the planner must decide before writing the test.** If the fix only guards
`:140`, execution reaches `:141`, which dereferences `grp.ItemViewer.LblItemNumber` — and
`ItemViewer` is also `null` in this arrangement, producing a *different* NRE and leaving the test
red. Constructing a real `ItemViewer` is not an option: it is a `UserControl`
(`QuickFiler/Viewers/ItemViewer.cs:21`) with a WebView2 surface. The correct minimal fix is
therefore to **skip the group entirely when `ItemController` is null** (or when `ItemViewer` is
null), which the test then asserts as "no throw, and no viewer text written". No seam is required.

### 3.4 #470 defect 1 — `ToggleGroupConv` / `PromoteFirstChild` with a `-1` index — **Direct**

Code: `ToggleGroupConv(string)` at `:1733-1766`; the `-1` branch at `:1743-1746`;
`PromoteFirstChild` at `:1970-1985`, whose `FindIndex` at `:1972` can also return `-1`, after which
`:1975` evaluates `_itemGroups[indexOriginal].ItemViewer`.

Both methods are public and on the interface (`IQfcCollectionController.cs:80` and — for
`PromoteFirstChild` — public on the class though not on the interface, `:1970`).

Critical ordering fact: `_itemTlp` is not touched until `:1976`, which is **after** the
`_itemGroups[-1]` subscript at `:1975`. The failure therefore occurs with `_itemTlp` still `null`,
so no WinForms control is needed.

Two tests, both on an uninitialized controller with `_itemGroups` holding one or two groups whose
`Mock<IQfcItemController>` returns `ConvOriginID = null` and whose `Mail` is a `Mock<MailItem>` with
a non-matching `EntryID`:

1. Direct: `int childCount = 0; controller.Invoking(c => c.PromoteFirstChild("missing", ref childCount))`
   — pre-fix throws `ArgumentOutOfRangeException`; post-fix must return the documented sentinel or
   throw an explicit, typed error (planner decides which; the potential document at
   `docs/features/potential/promoted/2026-08-07-qfc-collection-conversation-index-defects.md:59`
   requires "handled explicitly rather than used to subscript").
2. End-to-end through `ToggleGroupConv("missing")`, which exercises `:1735-1749` including the
   second `-1` consumer `ChangeConversationSilently(indexOriginal, true)` at `:1749`
   (`ChangeConversationSilently(int, bool)` at `:1714-1717` subscripts `_itemGroups` the same way).

No STA, no COM, no reflection, no UI.

### 3.5 #470 defect 2 — `EnumerateConversationMembers` count above / equal / below — **Direct, with a caveat**

Code: `EnumerateConversationMembers` at `:1875-1922`; the reservation is computed by the caller
`ToggleUnGroupConv` at `:1823` (`insertCount = conversationCount - 1`) and applied at `:1827-1829`;
the loop bound `insertions.Count` at `:1888-1889` is derived independently from
`resolver.ConversationItems.SameFolder` at `:1883-1886`.

`EnumerateConversationMembers` is public (`:1875`) and on the interface indirectly through
`ToggleUnGroupConv` (`IQfcCollectionController.cs:81-86`). Call it directly.

**The resolver is constructible without COM.** `new ConversationResolver(globals, mailItem)` is
already used by an existing test at
`QuickFiler.Test/Helper Classes/ConversationResolverTests.cs:75`, and `ConversationItems` has a
public setter (`QuickFiler/Helper Classes/ConversationResolver.Loading.cs:171-176`) that bypasses
the lazy `GetOrLoad` COM path. Inject
`resolver.ConversationItems = new Pair<IList<MailItem>>(sameFolder: <list>, expanded: <list>)`,
matching the shape constructed at
`QuickFiler/Helper Classes/ConversationResolver.Loading.cs:182`. Members are `Mock<MailItem>` with
`EntryID` and `SentOn` set up — the loop filters on `EntryID` and orders by `SentOn` (`:1884-1885`).

**Caveat — the loop body needs COM.** The first statement inside the loop is `InitializeGroup`
(`:1894`), which calls `LoadItemViewer_03` (`:1851`) → `ItemViewerQueue.Dequeue` (`:958`) and then
constructs a real `QfcItemController` (`:1853-1862`). Any iteration that actually executes is
therefore untestable. This constrains the three cases to arrangements in which the *first* observable
behaviour is the count reconciliation, before any iteration:

- **Above the reservation:** `_itemGroups.Count == insertionIndex` (zero reserved slots) and
  `insertions.Count == 1`. Pre-fix, `_itemGroups[0 + insertionIndex]` at `:1893` throws
  `ArgumentOutOfRangeException` on the first iteration, before `InitializeGroup` runs. Post-fix, the
  reconciliation must surface the disagreement (an explicit typed exception) **before** the loop —
  which is exactly what `...conversation-index-defects.md:103-104` requires ("fixed by reconciling
  the counts before insertion, not by clamping the loop"). Both sides of the test are COM-free.
- **Equal:** `conversationCount == 1` (reserved 0) with `insertions.Count == 0`. Loop never runs;
  assert no throw, pre- and post-fix.
- **Below the reservation:** `conversationCount == 3` (reserved 2) with `insertions.Count == 0`.
  Loop never runs. Pre-fix the method returns silently, leaving the placeholder `QfcItemGroup`s
  created at `:2008` with a `null` `ItemController` — assert that pre-fix state, and assert
  post-fix that the explicit reconciliation error is raised instead.

Because the post-fix reconciliation must run before the loop, all three tests are deterministic and
COM-free. No new seam is required; the fix's own shape (reconcile before insert) is what makes it
testable.

### 3.6 #469 defect 1 — `GetMoveDiagnostics` with a null `ItemController` — **Direct**

Code: `GetMoveDiagnostics` at `:2272-2328`; `var qf = TryGetItemGroupByIndex(k)?.ItemController;`
at `:2288`; unguarded dereference at `:2289` (`qf.ItemHelper`) and again at `:2312`; the guard at
`:2313` and its dead `else` at `:2318-2322`.

Reuse `CreateControllerWithOneGroup`
(`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:30-74`) but inject a
`QfcItemGroup` **without** an `ItemController` into `_itemGroupsToMove`. `TryGetItemGroupByIndex(0)`
returns the group; `?.ItemController` yields `null`; `:2289` throws `NullReferenceException`.

Test: `controller.Invoking(c => c.GetMoveDiagnostics(...)).Should().Throw<NullReferenceException>()`
pre-fix; post-fix assert no throw **and** that `result[0]` contains the intended
`"To Unknown,Sender Unknown,Email,Folder Unknown,..."` text from `:2320-2321`, which proves the dead
branch became live. `ref AppointmentItem olAppointment` can be `null`, exactly as the existing tests
do at `:90` and `:118`. No COM, no STA.

### 3.7 #469 defect 2 — returned array length — **Direct**

Code: `new string[_itemGroupsToMove.Count + 1]` at `:2284`; the loop fills `0..Count-1` at
`:2286-2325`.

Reuse `CreateControllerWithOneGroup` verbatim and assert `result.Length.Should().Be(1)` (and, for a
stronger case, a 3-group builder asserting `Length == 3` and `result.Should().NotContainNulls()`).
Pre-fix the length is `Count + 1` with a trailing `null`. Fully COM-free; this is the cheapest test
in the whole feature.

### 3.8 #469 defect 3 — `TryGetItemGroupByIndex` stability across a mutation — **Not deterministically achievable pre-fix; needs a structural companion test**

Code: `_itemGroupsToMove` is declared `ConcurrentDictionary<QfcItemGroup, int>` at `:71`;
`TryGetItemGroupByIndex` is `private` and does `_itemGroupsToMove.ElementAt(index).Key` at `:2260-2270`;
the two independent `0..Count-1` walks are `MoveEmailsAsync` at `:2220-2223` and `GetMoveDiagnostics`
at `:2286-2288`.

The method is `private`, so it is reachable only by reflection — that part is not the obstacle.

**The obstacle is determinism.** `ConcurrentDictionary` enumeration order is unspecified and, with
reference-type keys under the default comparer, is a function of runtime identity hash codes, which
differ between processes. A test asserting "index `i` resolves to a different group after a
mutation" is therefore **flaky by construction** and violates the determinism requirement in
`.claude/rules/general-unit-test.md`. It could pass on a run where the hash order happened to match
insertion order.

Recommended two-part strategy:

1. **Structural fail-before guard (deterministic).** Assert the declared type of the backing field
   is order-preserving:
   `typeof(QfcCollectionController).GetField("_itemGroupsToMove", BindingFlags.NonPublic | BindingFlags.Instance).FieldType`
   must be assignable to an ordered contract (`IList` / `IReadOnlyList<>`), or the field must be
   accompanied by a declared ordered index source. This fails deterministically before the fix and
   passes after. It is the same species of structural guard as
   `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:17-36`, which is established repository practice.
2. **Behavioural contract test (deterministic post-fix).** After the backing store becomes ordered,
   assert that index→group identity equals insertion order and is preserved across an add and a
   remove: build `[A, B, C]`, remove `B`, add `D`, assert index resolution yields `A, C, D`. This is
   guaranteed post-fix and is the assertion that actually protects the behaviour.

Record in the fail-before dossier that part 2 has no deterministic pre-fix red state, with the
`ConcurrentDictionary` unspecified-order reason cited. Note that changing the field's type is a
production change confined to `QfcCollectionController.cs`, which the feature owns
(`docs/features/active/qfc-collection-controller-defects-468/issue.md:40`).

### 3.9 #469 defect 4 — `MoveEmailsAsync` and the `stackMovedItems` undo stack — **Direct, but triage first**

Code: `MoveEmailsAsync(SloStack<IMovedMailInfo> stackMovedItems)` at `:2206-2228`. The parameter is
never read anywhere in the body.

**Triage evidence (this changes the recommended resolution).** The caller supplies
`_movedItems` (`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225`), which is
`_globals.AF.MovedMails` (`QuickFiler/Controllers/QfcFormController.cs:49`, field at `:86`). That
same stack **is** populated on the real move path: `MoveMailAsync` enqueues an `EmailFiler`
(`QuickFiler/Controllers/QfcItemController.MailActions.cs:100-111`), and `EmailFiler` pushes the
moved-mail record at `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:188`
(`Globals.AF.MovedMails.Push(info)`; the sibling path is
`UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs:1325`). The undo consumer reads the
same global stack at `QuickFiler/Controllers/QfcFormController.Actions.cs:206-250`.

The undo record is therefore **not** silently dropped, and the correct resolution is most likely
"remove the redundant parameter" rather than "populate it". **Scheduling constraint:** removing the
parameter requires editing `QuickFiler/Interfaces/IQfcCollectionController.cs:50` (owned) **and**
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225` (**not** listed in
`issue.md:40-45` "Files This Feature Owns"). The planner must either extend the owned-file list or
choose the populate option.

Test either way — both are COM-free:

- *Populate resolution:* `new SloStack<IMovedMailInfo>()` is constructible in memory with no
  filesystem access (`UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/SloStack.cs:31-35`).
  Inject one group whose `Mock<IQfcItemController>.MoveMailAsync()` returns `Task.CompletedTask`,
  `await controller.MoveEmailsAsync(stack)`, assert `stack.Count == 1`. Fails before the fix
  (stack stays empty).
- *Remove-parameter resolution:* the regression test is a contract test asserting the
  `IQfcCollectionController` member's parameter list, which fails to compile / fails the reflection
  assertion before the change.

### 3.10 #473 defect 1 — a task added during the `BackgroundLoadingTasks` reset window — **Seam-first**

Code: `internal ConcurrentBag<Task> BackgroundLoadingTasks = []` at `:80`; the defective sequence
`await Task.WhenAll(BackgroundLoadingTasks); BackgroundLoadingTasks = [];` appears twice, at
`:398-399` and at `:492-493`.

The field is `internal`, so `InternalsVisibleTo` gives the test direct access. The **enclosing
methods are not testable**: `LoadControlsAndHandlers_01Async` requires `MailItemHelper.FromMailItemAsync`
(`:300-305`), `ItemViewerQueue.Dequeue` (`:617`), a real `QfcItemController` (`:620-630`), and
`_formViewer.InvokeRequired` (`:329`, `:407`).

**Minimal seam:** extract the two identical reset sequences into one internal method on the same
class, for example:

```csharp
internal async Task DrainBackgroundLoadingTasksAsync()
```

and call it from `:398-399` and `:492-493`. This is behaviour-preserving (an extract-method
refactor of two byte-identical statement pairs) and creates a single point where the fix lands and a
single point the test drives. It also removes the current duplication, which is itself a
General Code Change Policy concern.

**Deterministic interleaving with no sleeps.** Continuations registered on a `Task` run in
registration order, and `TaskContinuationOptions.ExecuteSynchronously` runs them on the completing
thread before control returns. That yields an exact, timing-free construction of the reset window:

1. `var gate = new TaskCompletionSource<bool>();` and `var late = new TaskCompletionSource<bool>();`
2. `controller.BackgroundLoadingTasks.Add(gate.Task);`
3. Register, **before** starting the drain, a synchronous continuation on `gate.Task` that adds
   `late.Task` to `controller.BackgroundLoadingTasks`. Because it is registered first, it runs
   before the continuation that `Task.WhenAll` installs, so the add lands while the *old* bag is
   still the current one.
4. `Task drain = controller.DrainBackgroundLoadingTasksAsync();`
5. `gate.SetResult(true);`
6. Assert `drain.IsCompleted` is **false** — post-fix the drain must still be awaiting `late.Task`.
   Pre-fix the drain has already completed, having replaced the bag reference and dropped
   `late.Task`. This is the failing assertion.
7. `late.SetResult(true); await drain;` — cleanup, still no timers.

There are no `Thread.Sleep`, `Task.Delay`, or wall-clock waits anywhere in this recipe; every state
transition is driven by an explicit `SetResult`.

A candidate fix shape that satisfies the assertion: drain in a loop
(`while` the bag is non-empty, `Interlocked.Exchange` the bag for a fresh one and `await Task.WhenAll`
on the swapped-out contents) so no add can be dropped.

### 3.11 #473 defect 2 — one log entry for a null group; `OperationCanceledException` propagates — **Split: one half Direct, one half Seam-first**

Code: `TryMoveEmailByGroupAsync` at `:2236-2258`. Outer `catch (System.Exception e)` at `:2242`;
`group.MailItem.Subject` at `:2247`; inner `catch (System.Exception e2)` at `:2249` logging
`"Unable to retrieve subject ..."` at `:2251`; the second `logger.Error(...)` at `:2253-2256`.
`TryGetItemGroupByIndex` can return `null` (`:2266-2269`), and `TryMoveEmailByGroupIndexAsync`
(`:2230-2234`) passes it straight through.

#### Logger identification (answers research question 4)

The logger is a **file-private static readonly log4net logger**, declared at
`QuickFiler/Controllers/QfcCollectionController.cs:24-26`:

```csharp
private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
);
```

It is not injectable, not a field a test can replace without reflection, and **log4net is not
referenced by the test project**: `QuickFiler.Test/packages.config` (read in full) contains no
`log4net` entry, and `QuickFiler.Test/QuickFiler.Test.csproj` contains no `log4net` reference. Only
`QuickFiler.Test/app.config:78` carries an assembly binding redirect, which is a load-time artefact,
not a compile-time reference.

Capturing log entries therefore requires **either**:

- **(a) a log4net `MemoryAppender` in the test project** — needs a new `packages.config` entry plus a
  `<Reference>` in the csproj, and attaches an appender to the process-wide log4net repository. That
  is mutable global state shared by every test in the assembly and is a test-independence hazard; it
  also does not isolate *which* logger produced an entry without extra filtering. Not recommended.
- **(b) a minimal error-sink seam in production** — mirroring the `_removeGroupByEntryId` delegate
  seam that already exists in this same file at `:1060-1074`, which was introduced for exactly this
  purpose ("Tests inject a recording delegate so the ... logic can be verified without WinForms/COM
  state", `:1064-1065`). An `internal static Action<string, Exception> _logError` defaulting to
  `logger.Error` gives a test a call counter with no new package reference and no global state
  beyond one field the test resets. This is the smaller and more idiomatic seam for this repository.

#### The half that needs no seam at all

**`OperationCanceledException` propagation** is directly testable today. Inject one group whose
`Mock<IQfcItemController>.Setup(c => c.MoveMailAsync()).ThrowsAsync(new OperationCanceledException())`
into `_itemGroupsToMove`, then
`await controller.Invoking(c => c.MoveEmailsAsync(null)).Should().ThrowAsync<OperationCanceledException>()`.
Pre-fix the broad catch at `:2242` swallows it and the test fails; post-fix (a
`catch (OperationCanceledException) { throw; }` clause ahead of the broad catch) it passes. Fully
COM-free, deterministic.

#### The "exactly one log entry" half — a logger-free observable proxy exists

Rather than counting log calls, assert the **second dereference does not occur**. Arrange a group
whose `ItemController` is `null` (so `:2240` throws NRE and the outer catch is entered) and whose
`MailItem` is a `Mock<MailItem>` configured as
`mail.SetupGet(x => x.Subject).Throws(new System.Runtime.InteropServices.COMException())`. Pre-fix,
`:2247` reads `Subject`, throws, and produces the second log entry. Post-fix (early return after the
first catch, per `...background-task-and-catch-defects.md:97-98`), `Subject` is never read:

```csharp
mockMail.VerifyGet(x => x.Subject, Times.Never());
```

This is a deterministic, COM-free, logger-free fail-before assertion for the double-log defect. It
proves the mechanism (the second dereference) rather than the log count. If the acceptance criteria
insist on a literal "exactly one log entry" assertion, add seam (b) above; otherwise the proxy is
sufficient and cheaper.

Note that the true *null group* case (a `null` `QfcItemGroup` reaching `:2240`) cannot use the mock
proxy, because there is no object to verify against. Use the null-`ItemController` arrangement above
for the assertion, and cover the null-group path with a plain "does not throw" test through
`MoveEmailsAsync` after forcing `TryGetItemGroupByIndex` to return `null` (an index beyond the
dictionary's count is caught at `:2266-2268`).

### 3.12 #474 defect 2 — `ReadyForMove` must return a result without a `MessageBox` — **Seam-first (currently untestable by policy)**

Code: the `ReadyForMove` getter at `QuickFiler/Controllers/QfcCollectionController.cs:152-194`, with
`MessageBox.Show(...)` at `:186-191`.

Reading the property today displays a modal dialog whenever any group lacks a folder. That is a
direct violation of the repository unit-test policy and of
`docs/features/epics/winforms-testability-refactor/epic.md:58-59` ("never show popups (a popup
requiring human interaction is a unit-test-policy violation)"). The `false` path is therefore
**untestable as written**; only the all-groups-ready `true` path can be read safely today, and that
path exercises none of the defect.

**Minimal seam — two options, in increasing size:**

1. **Injectable notification delegate (recommended).** Add
   `private Action<string> _notifyNotReady;` with a lazily-defaulted property that calls
   `MessageBox.Show(msg, "Error Notification", MessageBoxButtons.OK, MessageBoxIcon.Error)`, exactly
   mirroring `_removeGroupByEntryId` / `RemoveGroupByEntryId` at `:1060-1074`. Change `:186-191` to
   invoke the delegate. Behaviour in production is bit-for-bit identical; a test injects a recording
   delegate, reads `ReadyForMove`, and asserts both the returned `false` and the captured message
   text. This is the smallest change and matches an in-file precedent.
2. **Extract the pure evaluation.** Add
   `internal bool TryEvaluateReadiness(out string notifications)` containing `:156-184`, leave the
   getter as `{ var ready = TryEvaluateReadiness(out var msg); if (!ready) MessageBox.Show(msg, ...); return ready; }`.
   The test targets the internal method and never touches the getter. This is closer to the
   potential document's stated expected behaviour
   (`...controller-coupling-and-modal-getter.md:52-54`, "a readiness check should return a result the
   caller can inspect and act on") but leaves the getter's side effect in place.

Either seam makes the test COM-free: the loop at `:161-184` reads `grp.ItemController.SelectedFolder`,
`.ItemNumber`, `.Mail.SentOn`, `.Mail.Subject`, all satisfiable with `Mock<IQfcItemController>` and
`Mock<MailItem>`. Assert the three header sentinel strings at `:165-167` are each treated as
"not assigned" — that is a genuine edge case worth covering.

Moving the `MessageBox` to the caller (the potential document's preferred end state) is **out of
scope for the owned-file list**: the caller is in `QfcFormController.*`, which `issue.md:40-45` does
not list.

---

## 4. Logging seam summary (research question 4)

- Logger: `private static readonly log4net.ILog logger`, obtained via
  `log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)` at
  `QuickFiler/Controllers/QfcCollectionController.cs:24-26`.
- Call sites relevant to this feature: `:1239-1241` (the #286 false-positive race message),
  `:2251` and `:2253-2256` (the #473 defect-2 double log).
- The test project has **no** log4net reference (`QuickFiler.Test/packages.config`,
  `QuickFiler.Test/QuickFiler.Test.csproj`), only a binding redirect at
  `QuickFiler.Test/app.config:78`.
- Minimal change to make log assertions possible: an `internal static Action<string, Exception>`
  error-sink field defaulting to `logger.Error`, following the existing `_removeGroupByEntryId`
  delegate-seam pattern at `:1060-1074`. Adding a log4net `MemoryAppender` to the test project is the
  larger alternative and introduces process-wide mutable state.
- For #473 defect 2 specifically, the logger seam can be avoided entirely by asserting the observable
  proxy described in §3.11 (`VerifyGet(x => x.Subject, Times.Never())`).

---

## 5. STA / apartment requirements (research question 5)

Neither existing `QfcCollectionController` test file uses STA, and none of the defects below needs it:

| Defect | STA needed? | Reason |
|---|---|---|
| #286 counter leak | No | Fails at `_itemGroups.Count` (`:1345`) with every WinForms field still null |
| #470 d3 `SetVisualDigits` | No | Fails at `:140` before any viewer access |
| #470 d1 `-1` index | No | Fails at `:1975` before `_itemTlp` is touched at `:1976` |
| #470 d2 count reconciliation | No | Post-fix reconciliation runs before the COM-bound loop body |
| #469 d1/d2 diagnostics | No | Pure string assembly over mocks |
| #469 d3 ordering | No | Structural + ordered-collection assertions only |
| #469 d4 undo stack | No | `SloStack<T>` is in-memory |
| #473 d1 drain | No | Pure `Task`/`TaskCompletionSource` |
| #473 d2 catch behaviour | No | Mocks only |
| #474 d2 `ReadyForMove` | No, **with** the seam | Without a seam it shows a modal dialog, which is prohibited outright — STA would not make it legal |

**Only #471 (`EliminateSpaceForItems`) has an STA question**, and only because it manipulates a real
`TableLayoutPanel` whose `InvokeRequired` is read at
`UtilitiesCS/HelperClasses/Windows Forms/TableLayoutHelper.cs:62` (and `:21` for the insert path).
The repository precedent for exactly this is
`UtilitiesCS.Test/HelperClasses/WindowsForms/ScreenAndTableLayoutTests.cs:41`
(`[STATestClass] public class TableLayoutHelper_Tests`, bare `new TableLayoutPanel()` at `:47`).

A pure-arithmetic seam (§3.2) removes even that need. **Recommendation: try the seam first, as the
ratified rule requires (`epic.md:66-67`), and keep exactly one STA test for the call-site sign
assertion, in a dedicated `QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs`
marked `[STATestClass]`, with the panel disposed per test and no `Show()`/pump reliance.**

No candidate test needs a real window handle beyond the implicit marshaling-control handle that
`InvokeRequired` creates, and none needs a message pump.
`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` (an STA `Application.Run` host, `:26-69`) exists
and is available, but no defect in this feature requires it — it is for tests that must await
continuations posted to a `WindowsFormsSynchronizationContext`, which none of these do.

`UiThread.Init()` must never be called from a test (`UtilitiesCS/Threading/UiThread.cs:54` shows a
form).

---

## 6. Existing test-project infrastructure

### 6.1 Reusable assets

| Asset | Path:line | Reusability for this feature |
|---|---|---|
| `WinFormsPumpHost` (internal, STA `Application.Run` host) | `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs:26` | Available; **not needed** by any defect here |
| `QfcItemControllerTestSupport.SetField` / `GetField` / `InvokeNonPublic` | `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:37,49,66` | Pattern to copy (asserting reflection helpers); typed to `QfcItemController`, so a `QfcCollectionController` analogue is needed |
| `HarnessController` (exposes a protected ctor) | `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:25-29` | Not applicable — `QfcCollectionController` has no protected parameterless ctor |
| `FakeQfcItemController` (hand-rolled full `IQfcItemController`) | `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:337` | `private sealed` and nested; **not reusable as-is**. `Mock<IQfcItemController>` is already the established choice in the `QfcCollectionController` tests and is preferred |
| `FakeApplicationGlobals` | `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:374`, `EfcHomeControllerTests.cs:193`, `EfcHomeControllerMetricsTests.cs:194` | `private sealed`, nested, duplicated three times; `Mock<IApplicationGlobals>` is simpler here |
| `NoLiveFormInTestAssemblyTests` (structural metadata guard) | `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:17-36` | Precedent for the structural fail-before guard proposed in §3.8 |
| `SetupAssemblyInitializer` (`EnableVisualStyles`, `SetCompatibleTextRenderingDefault`) | `QuickFiler.Test/SetupAssemblyInitializer.cs:14-20` | Already runs assembly-wide; no action needed |
| `ConversationResolver` construction + `ConversationItems` injection precedent | `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs:75`; setter at `QuickFiler/Helper Classes/ConversationResolver.Loading.cs:171-176` | Directly reusable for §3.5 |
| `[STATestClass]` + bare `TableLayoutPanel` precedent | `UtilitiesCS.Test/HelperClasses/WindowsForms/ScreenAndTableLayoutTests.cs:41,47` | Directly reusable for §3.2 |
| STA opt-in policy note | `UtilitiesCS.Test/test.runsettings:2-5` | Confirms per-attribute opt-in, no global STA |

There is **no** existing fake `IQfcFormViewer`, no dispatcher stub applicable to `UiThread`, and no
shared `QfcCollectionController` test-support file. If more than one new test file is created, a
`QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs` holding the builders and
asserting reflection helpers would prevent triplication — but see §6.2 on the csproj cost.

### 6.2 `Compile Include` entries and the exact insertion point

The item group spans `QuickFiler.Test/QuickFiler.Test.csproj:57-175`. It is **not strictly
alphabetical**: it is grouped by class family, and within a family the base file precedes its
variants (for example `QfcCollectionControllerTests.cs` at `:116` precedes
`QfcCollectionControllerDarkModeTests.cs` at `:117`, and `QfcItemControllerTests.cs` at `:138`
precedes the `QfcItemController.*` partials at `:139-156`).

Counts:

- **41** `Compile Include` entries form the contiguous `Controllers\Qfc*` block at lines **116-156**.
- **46** entries in the whole item group have a filename beginning with `Qfc`: the 41 above plus
  `:94` (`Controllers\QfcItemControllerBreadcrumbDropDownTests.cs`), `:102`
  (`Controllers\QfcQueueCoverageExpansionTests.cs`), `:103`
  (`Controllers\QfcQueuePurePathsTests.cs`), `:160` (`Controllers\QfcQueueTests.cs`), and `:173`
  (`QfcViewer_Test.cs`).

A new `QfcCollectionController*` entry belongs immediately after line 117, between these two lines
(quoted verbatim, including leading indentation):

```
    <Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />
    <Compile Include="Controllers\QfcDatamodelTests.cs" />
```

That is, the new element is inserted as a new line 118, pushing `QfcDatamodelTests.cs` to 119. This
keeps the new file inside the `QfcCollectionController` family block and minimises the merge-conflict
surface with sibling epic children, which are most likely to touch other family blocks.

**Prefer adding methods to the two existing files** — `QfcCollectionControllerTests.cs` is 500 lines
(at the 500-line ceiling in `.claude/rules/general-code-change.md`), and
`QfcCollectionControllerDarkModeTests.cs` is 155 lines, so the dark-mode file has room but is
topically wrong for these defects. Realistically at most **two** new files are justified:

1. `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` (MTA, the bulk), because
   the existing `QfcCollectionControllerTests.cs` is already at the 500-line limit.
2. `QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs`, **only if** the STA panel
   test survives the seam-first analysis in §3.2. The `*.StaTests.cs` suffix is mandatory per
   `docs/features/epics/winforms-testability-refactor/epic.md:68-70`.

---

## 7. Recommended overall test strategy

### 7.1 Framing on production seams

Inventing a seam is a **production change to a file this feature owns**
(`docs/features/active/qfc-collection-controller-defects-468/issue.md:40`), and it is permitted. It
must be **minimal and behaviour-preserving**: an extract-method, or an injectable delegate that
defaults to the exact prior call. The file already contains a ratified precedent for the delegate
form at `QuickFiler/Controllers/QfcCollectionController.cs:1060-1074`, whose XML comment states the
intent explicitly. New seams should copy that shape, including the explanatory comment, so a reviewer
can see the default preserves production behaviour.

Note for the record: the class carries `[ExcludeFromCodeCoverage]` at
`QuickFiler/Controllers/QfcCollectionController.cs:21`. Tests added by this feature will therefore not
move the coverage metric for this file unless the attribute is also removed, which is a separate
decision outside this research.

### 7.2 Tier 1 — direct unit tests, no production seam (write these first)

| Defect | Test target | Fail-before mechanism |
|---|---|---|
| #469 d2 | `GetMoveDiagnostics` array length | Length is `Count + 1` with a trailing `null` (`:2284`) |
| #469 d1 | `GetMoveDiagnostics` null `ItemController` | `NullReferenceException` at `:2289` |
| #470 d1 | `PromoteFirstChild` / `ToggleGroupConv(string)` with `-1` | `ArgumentOutOfRangeException` at `:1975` |
| #470 d3 | `SetVisualDigits` (reflection) with null `ItemController` | `NullReferenceException` at `:140` |
| #286 | Counter after a forced throw (reflection read) | Counter left at pre-call + 1 (`:1161` vs `:1247`) |
| #473 d2 (cancellation half) | `MoveEmailsAsync` with a mock throwing `OperationCanceledException` | Swallowed by the broad catch at `:2242` |
| #473 d2 (double-deref half) | `VerifyGet(x => x.Subject, Times.Never())` | `:2247` reads `Subject` after the first catch |
| #470 d2 | `EnumerateConversationMembers` above / equal / below, arranged so no loop iteration runs | Above: `ArgumentOutOfRangeException` at `:1893`; below: silent placeholder groups |
| #469 d4 | `MoveEmailsAsync` + `SloStack` (populate resolution) | Stack stays empty |

All Tier 1 tests are MTA, COM-free, UI-free, sleep-free, and belong in a single new file
`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` (§6.2 gives the exact csproj
insertion point) plus, where they fit, extensions to the existing files.

### 7.3 Tier 2 — small production seam required first

| Defect | Seam | Size |
|---|---|---|
| #473 d1 | Extract `:398-399` and `:492-493` into `internal async Task DrainBackgroundLoadingTasksAsync()` | Extract-method over two byte-identical statement pairs; also removes duplication |
| #474 d2 | `private Action<string> _notifyNotReady` defaulting to the existing `MessageBox.Show(...)` call at `:186-191`, mirroring `:1060-1074` | One field, one lazily-defaulted property, one call-site substitution |
| #471 | `internal static Size ShrinkByRows(Size, float, int)` shared by `:2018-2026` and `:2031-2034` | One pure static helper; optional, see §3.2 |
| #473 d2 (literal log-count assertion, only if the AC demands it) | `internal static Action<string, Exception>` error sink defaulting to `logger.Error` | One field; avoids adding a log4net package reference to the test project |

Each seam must be introduced in a step that changes no observable production behaviour, verified by
the existing suite passing unchanged, before the defect fix lands on top of it.

### 7.4 Tier 3 — STA, last resort

Only #471's call-site assertion, and only if the §3.2 pure-helper seam is judged insufficient. It
must live in `QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs`, be marked
`[STATestClass]`, dispose its `TableLayoutPanel` per test, never call `Show()`/`ShowDialog()`, and
carry an in-file comment stating why no seam covers the call-site sign, per
`docs/features/epics/winforms-testability-refactor/epic.md:66-70`.

### 7.5 Tier 4 — structurally untestable; document a fail-before exception dossier

- **#469 defect 3 (`TryGetItemGroupByIndex` stability).** No deterministic pre-fix red state exists,
  because `ConcurrentDictionary` enumeration order is unspecified (`:71`, `:2264`). Ship the
  deterministic structural guard and the post-fix ordering contract test described in §3.8, and
  record the absence of a deterministic pre-fix failure in the dossier with that reason.
- **#474 defect 1 (concrete-type downcast at `:1232`).** A test substituting a non-`QfcFormController`
  `IFilerFormController` would assert `InvalidCastException`, but the call site is unreachable without
  `UiThread.Dispatcher` (`:1226`), which cannot be initialised in a test (§2.1). The real fix is the
  two-interface consolidation, and
  `docs/features/potential/promoted/2026-08-07-qfc-collection-controller-coupling-and-modal-getter.md:91-93`
  records that `QuickFiler/Controllers/IQfcFormController.cs` and
  `QuickFiler/Interfaces/IFilerFormController.cs` are owned by issue #435 — though `issue.md:40-45`
  lists both files as owned by this feature, so the planner must reconcile that conflict before
  scheduling. Not addressed further here; it was not in the research scope.

### 7.6 Sequencing recommendation

1. Add the asserting reflection helpers (copy the `QfcItemController.TestSupport.cs:37-47` shape) and
   the Tier 1 tests. Confirm each is red for the stated reason before touching production code.
2. Land the Tier 2 seams as behaviour-preserving steps, each verified against the unchanged suite.
3. Add the Tier 2 tests; confirm red.
4. Apply the defect fixes; confirm green.
5. Decide #469 d4's resolution (populate vs remove) explicitly, on the §3.9 triage evidence, before
   writing its test — the two resolutions have different owned-file footprints.
6. Take the STA test only if step 2's `ShrinkByRows` helper leaves the sign-at-call-site unproven.
