# Per-File Coverage Research — `QuickFiler/Controllers/KeyboardHandler.cs`

Timestamp: 2026-08-07T21-55
Feature: `quickfiler-keyboard-actions-coverage` (issue #430, epic child F3, wave 1)
Epic: `quickfiler-per-file-coverage` (issue #136)
Branch: `feature/quickfiler-keyboard-actions-coverage`

---

## 1. File Under Research

| Attribute | Value |
| --- | --- |
| Path | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aafcc2531072ca96b\QuickFiler\Controllers\KeyboardHandler.cs` |
| Line count | 414 (file ends at line 415 including trailing newline; last code line 414) |
| Type | `internal class KeyboardHandler : IQfcKeyboardHandler` (line 23) |
| Compiled | Yes — `QuickFiler/QuickFiler.csproj:339` `<Compile Include="Controllers\KeyboardHandler.cs" />` |
| `[ExcludeFromCodeCoverage]` | **Present**, line 22 |
| Existing tests | **None.** No `KeyboardHandler*Tests.cs` exists anywhere under `QuickFiler.Test/` (verified by directory enumeration of all 107 `QuickFiler.Test/**/*.cs` files and by repo-wide grep for `new KeyboardHandler`). |
| Test-assembly access | `QuickFiler/Properties/AssemblyInfo.cs:5` — `[assembly: InternalsVisibleTo("QuickFiler.Test")]`. `QuickFiler/Controllers/QfcHomeController.cs:18` repeats it. `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11` adds `DynamicProxyGenAssembly2`. The `internal` class is therefore directly constructible and Moq-proxyable from `QuickFiler.Test` with no reflection. |
| Exemption-status authority | **F1's `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`** is the sole authority on whether this file (or any residual line range within it) is `testable` or `ratified-exempt`. That artifact does not exist on disk yet; this research is written to consume it. |
| Per-file coverage measurement | Numeric per-file line coverage will be measured **at execution time** with F1's per-file coverage harness derived from the Cobertura output of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`. The harness does not exist yet, so §3 below establishes the current state by static analysis, not by measurement. |

### Central determination

Per epic.md Shared Design §1, `[ExcludeFromCodeCoverage]` on a *testable* seam is a **Blocking** finding: the CLAUDE.md § UT2 qualifier "without an injectable seam" is a live obligation, not standing permission. This research finds that **the attribute at line 22 is not justified**. Every host-bound dependency in this file except one constructor overload is reachable behind an interface seam, an existing repo seam, or a narrow injectable delegate. The recommended disposition is: **remove `[ExcludeFromCodeCoverage]` from line 22** and request a ledger entry from F1 for the single residual remainder identified in §7.

---

## 2. Structural Inventory

Dependency legend: **OL** = Outlook Interop, **WF** = WinForms, **WPF** = WPF `Dispatcher`, **STAT** = static/global state, **SIB** = sibling-owned production type, **PURE** = no host dependency.

| Lines | Member | Kind | Depends on | Seam-isolatable? |
| --- | --- | --- | --- | --- |
| 1–18 | `using` directives | — | Declares `Microsoft.Office.Interop.Outlook` (line 15) and `System.Web.UI.WebControls` (line 12), **neither of which is referenced by any member in the file**. `System.Windows.Input` (line 14) is likewise unused. | n/a — dead usings; see §9 |
| 22 | `[ExcludeFromCodeCoverage]` | attribute | — | Remove (see §1) |
| 23 | class declaration | type | `IQfcKeyboardHandler` | n/a |
| 25–27 | `logger` | static readonly field | log4net `LogManager` (STAT) | No seam needed. Runs at type init; covered incidentally by any test. log4net resolves without configuration. |
| 29–33 | `KeyboardHandler(IQfcFormViewer viewer, IFilerHomeController parent)` | ctor | `IQfcFormViewer` (**interface**, `QuickFiler/Interfaces/IQfcFormViewer.cs:12`), `IFilerHomeController` (**interface**, `QuickFiler/Interfaces/IFilerHomeController.cs:11`) | **Already testable.** Both parameters are interfaces; Moq works directly. Calls `viewer.SetKeyboardHandler(this)` (declared at `IQfcFormViewer.cs:21`). |
| 35–39 | `KeyboardHandler(EfcViewer viewer, IFilerHomeController parent)` | ctor | **`EfcViewer` is a concrete `Form`** (`QuickFiler/Viewers/EfcViewer.cs:21` — `[ExcludeFromCodeCoverage] public partial class EfcViewer : Form`), whose ctor (lines 23–30) runs `InitializeComponent()`, reads `SynchronizationContext.Current`, and calls `TaskScheduler.FromCurrentSynchronizationContext()` | **Partially.** See §5 Seam K3 and §7. This is the only irreducible-remainder candidate. |
| 41 | `_parent` | field | `IFilerHomeController` | Interface — mockable |
| 42 | `_kbdActive` | field | PURE | — |
| 44 | `_charActions` | field | `KbdActions<char, KaChar, Action<char>>` (PURE, first-party) | — |
| 45–49 | `CharActions` | property get/set | PURE | Directly testable |
| 51 | `_charActionsAsync` | field | PURE | — |
| 52–56 | `CharActionsAsync` | property get/set | PURE | Directly testable |
| 58 | `_keyActions` | field | `Keys` (WF enum — value type, no host) | — |
| 59–63 | `KeyActions` | property get/set | PURE | Directly testable |
| 65 | `_alwaysOnKeyActionsAsync` | field | PURE | — |
| 66–70 | `AlwaysOnKeyActionsAsync` | property get/set | PURE | Directly testable |
| 72 | `_keyActionsAsync` | field | PURE | — |
| 73–77 | `KeyActionsAsync` | property get/set | PURE | Directly testable |
| 79 | `_filterBuilder` | field | `StringBuilder` (PURE) | — |
| 81 | `ClearFilter()` | method | PURE | Directly testable. **Not on `IQfcKeyboardHandler`. No caller anywhere in the repo** (verified by grep across all `*.cs`). Dead public member. |
| 83 | `_stringActionsAsync` | field | PURE | — |
| 84–88 | `StringActionsAsync` | property get/set | PURE | Directly testable |
| 90–94 | `KbdActive` | property get/set | PURE | Directly testable |
| 96–102 | `KeyboardHandler_PreviewKeyDown(object, PreviewKeyDownEventArgs)` | method | `PreviewKeyDownEventArgs` (WF, **constructible in-memory**: `new PreviewKeyDownEventArgs(Keys.X)`) | **Directly testable.** Not on the interface; **no production caller** (only the `...Async` variant is wired — see §6). |
| 104–112 | `KeyboardHandler_PreviewKeyDownAsync(object, PreviewKeyDownEventArgs)` | method | `SynchronizationContext` static get/set (STAT, line 106–107), `_parent.UiSyncContext` (`IFilerHomeController.cs:23`, mockable) | **Directly testable.** Ambient `SynchronizationContext` is settable/restorable by the test (precedent: `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs:353–373`). |
| 114–131 | `KeyboardHandler_KeyDown(object, KeyEventArgs)` | method | `KeyEventArgs` (WF, constructible: `new KeyEventArgs(Keys.X)`), `Delegate.DynamicInvoke` (line 122) | **Directly testable.** Interface member (`IQfcKeyboardHandler.cs:17`) but the only production wiring is **commented out** at `QuickFiler/Controllers/EfcItemController.cs:651`. |
| 133–148 | `KeyboardHandler_KeyDownAsync(object, KeyEventArgs)` | **`async void`** | `SynchronizationContext` (STAT), `_parent.UiSyncContext`, `logger.Error` (line 143) | **Testable** with an inline (synchronously-pumping) `SynchronizationContext`; see §5 Seam K5 and §8 determinism note. |
| 150–204 | `KeyDownTaskAsync(object, KeyEventArgs)` | `async Task` | `SynchronizationContext` (STAT), `_parent.UiSyncContext`, `KbdActions<>` lookups, `_filterBuilder` | **Directly testable.** This is the single richest logic block in the file (55 lines, 9 decision points) and is entirely host-neutral once the `SynchronizationContext` line is handled. |
| 206–217 | `ToggleKeyboardDialog()` | method | `_parent.FormController` → `IFilerFormController.ToggleOffNavigation(bool)` / `ToggleOnNavigation(bool)` (`QuickFiler/Interfaces/IFilerFormController.cs:18,20`) | **Directly testable** — both are interface members. |
| 219–223 | `ToggleKeyboardDialog(object, KeyEventArgs)` | method | as above + `KeyEventArgs.Handled` | Directly testable |
| 225–236 | `ToggleKeyboardDialogAsync()` | `async Task` | `IFilerFormController.ToggleOffNavigationAsync()` / `ToggleOnNavigationAsync()` (`IFilerFormController.cs:19,21`) | Directly testable |
| 238–245 | `ToggleKeyboardDialogAsync(object, KeyEventArgs)` | **`async void`** | `SynchronizationContext` (STAT), `_parent.UiSyncContext` | Testable via inline sync context |
| 247–261 | `GetItemViewer(Control)` | `internal` recursive method | `System.Windows.Forms.Control` (WF), `ItemViewer` (SIB — F14-owned `QuickFiler/Viewers/ItemViewer.cs`) | **Directly testable.** `new ItemViewer()` constructs headlessly — established precedent, see §5.3. Plain `Panel`/`Label` parents need no handle. **No caller anywhere in the repo.** Dead internal member. |
| 263–265 | comment (#351 rationale) | — | — | — |
| 266–286 | `CboFolders_KeyDownAsync(object, KeyEventArgs)` | **`async void`** | `SynchronizationContext` (STAT), `new WindowsFormsSynchronizationContext()` (WF, line 270), `ComboBox` (WF, line 272), **`cb.DroppedDown` getter (line 278 — forces handle creation semantics on the setter side and cannot report `true` on a handle-less control)** | **Testable with a seam.** See §5 Seam K4. |
| 288–291 | comment (#351 rationale) | — | — | — |
| 292–315 | `BreadcrumbArrowFallThrough(ItemViewer, BreadcrumbArrowDirection)` | method | `ItemViewer` (SIB, concrete), **`MyBox.ShowDialog(...)` static (lines 304–309) — shows a modal dialog**, `viewer.Controller.RightKeyActions` (`IItemControler.RightKeyActions`, `QuickFiler/Interfaces/IItemControler.cs:13` — **interface**), `viewer.SetFolderDroppedDown(false)` (line 313) | **Testable with a seam.** See §5 Seam K1 and §5.3. |
| 317–389 | `DdOpen_KeyDownAsync(ComboBox, KeyEventArgs)` | `async Task` | `SynchronizationContext` (STAT), **`MyBox.ShowDialog` (lines 350–355)**, **`UiThread.Dispatcher.Invoke` static WPF dispatcher (lines 362, 370)** (`UtilitiesCS/Threading/UiThread.cs:135`), `cbo.GetAncestor<ItemViewer>()` (`UtilitiesCS/Extensions/WinFormsExtensions.cs:176`) | **Testable with seams K1 + K2.** |
| 391–412 | `DdClosed_KeyDownAsync(ComboBox, KeyEventArgs)` | `async Task` | `SynchronizationContext` (STAT), **`UiThread.Dispatcher.InvokeAsync` (line 401)** | **Testable with seam K2.** |

### Nested types

None. The file declares exactly one type.

### Aggregate

- 2 constructors, 7 auto-style properties (6 `KbdActions<>` + `KbdActive`), 1 static field, 7 instance fields, 13 methods (4 of which are `async`), 0 events, 0 nested types.
- Decision points (branch count for coverage purposes): `KeyboardHandler_PreviewKeyDown` 3, `..._PreviewKeyDownAsync` 4, `KeyboardHandler_KeyDown` 5, `..._KeyDownAsync` 2, `KeyDownTaskAsync` 12, `ToggleKeyboardDialog()` 1, `ToggleKeyboardDialogAsync()` 1, `ToggleKeyboardDialogAsync(object,e)` 1, `GetItemViewer` 2, `CboFolders_KeyDownAsync` 3, `BreadcrumbArrowFallThrough` 2, `DdOpen_KeyDownAsync` 6, `DdClosed_KeyDownAsync` 3. **Total 45 decision points.**

---

## 3. Existing Test Coverage (static analysis)

There is **no test anywhere in the repository that constructs or exercises `KeyboardHandler`**. Every existing reference to the type from a test assembly is to the **interface** `IQfcKeyboardHandler`, replaced by a Moq double, which contributes **zero** coverage to `KeyboardHandler.cs`.

| Member (line range) | Exercised by | Coverage |
| --- | --- | --- |
| `logger` static init (25–27) | — | **none** |
| ctor `(IQfcFormViewer, IFilerHomeController)` (29–33) | — | **none** |
| ctor `(EfcViewer, IFilerHomeController)` (35–39) | — | **none** |
| `CharActions` (45–49) | — | **none** |
| `CharActionsAsync` (52–56) | — | **none** |
| `KeyActions` (59–63) | — | **none** |
| `AlwaysOnKeyActionsAsync` (66–70) | — | **none** |
| `KeyActionsAsync` (73–77) | — | **none** |
| `ClearFilter` (81) | — | **none** |
| `StringActionsAsync` (84–88) | — | **none** |
| `KbdActive` (90–94) | — | **none** |
| `KeyboardHandler_PreviewKeyDown` (96–102) | — | **none** |
| `KeyboardHandler_PreviewKeyDownAsync` (104–112) | — | **none** |
| `KeyboardHandler_KeyDown` (114–131) | — | **none** |
| `KeyboardHandler_KeyDownAsync` (133–148) | — | **none** |
| `KeyDownTaskAsync` (150–204) | — | **none** |
| `ToggleKeyboardDialog()` (206–217) | — | **none** |
| `ToggleKeyboardDialog(object,e)` (219–223) | — | **none** |
| `ToggleKeyboardDialogAsync()` (225–236) | — | **none** |
| `ToggleKeyboardDialogAsync(object,e)` (238–245) | — | **none** |
| `GetItemViewer` (247–261) | — | **none** |
| `CboFolders_KeyDownAsync` (266–286) | — | **none** |
| `BreadcrumbArrowFallThrough` (292–315) | — | **none** |
| `DdOpen_KeyDownAsync` (317–389) | — | **none** |
| `DdClosed_KeyDownAsync` (391–412) | — | **none** |

Mock-only (non-contributing) references, for completeness:

- `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:161–164` — `Mock<IQfcKeyboardHandler>(MockBehavior.Strict)` with `BreadcrumbArrowFallThrough` set up. Exercises `QfcItemController.OnBreadcrumbUnhandledArrow`, **not** `KeyboardHandler`.
- `QuickFiler.Test/Controllers/QfcItemController.{Navigation,EventWiring,Initialization,FocusAndTheme,SeamDispatcher,SeamFactory}Tests.cs`, `QfcItemControllerTests.cs`, `QfcHomeController{,Property}Tests.cs`, `QfcFormControllerSeamTests.cs`, `QfcCollectionController{,DarkMode}Tests.cs`, `EfcHomeController{Lifecycle,Dependencies,DependenciesProductionFactory,Seam}Tests.cs` — all `Mock<IQfcKeyboardHandler>`.

**Static-analysis conclusion: current per-file line coverage for `KeyboardHandler.cs` is 0%.** The `[ExcludeFromCodeCoverage]` attribute currently removes the file from the denominator entirely, so the measured figure is presently undefined rather than 0. Once the attribute is removed, F1's harness will report the real number; the plan must capture a pre-work baseline immediately after attribute removal and a post-work figure.

---

## 4. Coverage Gaps

Every member is a gap. Ordered by line weight, the genuine gaps are:

1. **`KeyDownTaskAsync` (150–204, 55 lines, 12 decision points)** — the string-filter accumulator (178–202) is the most behaviour-dense and least obvious code in the file and has never been exercised. It is entirely host-neutral.
2. **`DdOpen_KeyDownAsync` (317–389, 73 lines incl. commented-out cases, 6 live decision points)** — blocked today by two static calls (`MyBox.ShowDialog`, `UiThread.Dispatcher`).
3. **`BreadcrumbArrowFallThrough` (292–315)** — blocked by `MyBox.ShowDialog` on the `Right` branch; the `Left` branch and the null guard are unblocked.
4. **`CboFolders_KeyDownAsync` (266–286)** — the `sender is not ComboBox` early return and the `DroppedDown == false` route are unblocked; the `DroppedDown == true` route needs a seam.
5. **`DdClosed_KeyDownAsync` (391–412)** — blocked only by `UiThread.Dispatcher`.
6. **`KeyboardHandler_KeyDown` (114–131)**, **`..._PreviewKeyDown[Async]` (96–112)**, **all 7 properties**, **both `ToggleKeyboardDialog*` pairs (206–245)**, **`GetItemViewer` (247–261)**, **ctor #1 (29–33)** — no blocker at all. These are pure omissions: roughly 120 lines and 22 decision points reachable today with Moq and in-memory WinForms argument objects, with no production change whatsoever.
7. **ctor #2 (35–39)** — the only genuinely hard member.

### Branches that appear unreachable (report, do not chase)

- **Line 189, `if (actions.Length == 0)`** — unreachable. Line 181 guards with `StringActionsAsync.ContainsKey(...)`, implemented at `KbdActions.cs:49` as `_list.Any(x => x.KeyEquals(key))`; line 188 calls `FilterKeys` (`KbdActions.cs:51`) which is `_list.Where(x => x.KeyEquals(key)).ToArray()` over the same predicate and the same list. If `Any` was true, `Where` yields at least one element. The `_filterBuilder.Length = 0` body at line 190 is therefore dead defensive code. Record this in the F1 ledger as an unreachable-branch note rather than attempting a contrived test.
  - Caveat worth one characterization test: `KaStringAsync.KeyEquals` (`QuickFiler/Controllers/KaStringAsync.cs:57–79`) has **side effects** — it invokes `Update`/`ToggleControl` and always sets `Activated = false` before returning. Calling it twice (once via `ContainsKey`, once via `FilterKeys`) is therefore observable. The proposed test #37 pins this ordering so a later refactor cannot silently change it.
- **Line 191, `else if (actions.Length == 1)` with no `else`** — the `> 1` case falls through with the filter retained. That is reachable and must be tested (proposed test #35).

---

## 5. Seam Requirements

Hierarchy applied in strict order per `.claude/rules/csharp.md` § DI Seams and epic.md Shared Design §2: **(1) interface seam > (2) injectable delegate > (3) adapter.**

### Seam K1 — `IQfcDialogPrompt` (hierarchy level 1: interface seam, with a level-3 adapter as its production implementation)

**Blocks removed:** `MyBox.ShowDialog(...)` at lines 304–309 and 350–355.

**Why a seam is mandatory here, not optional.** `MyBox.ShowDialog(string, string, BoxIcon, Dictionary<string, Action>)` is `UtilitiesCS/Dialogs/MyBox.cs:141`, which constructs a `MyBoxViewer` form and calls `DialogInvoker(viewer)` (line 73/91/etc.). `UtilitiesCS` **does** carry a replaceable `internal static Func<MyBoxViewer, DialogResult> DialogInvoker` seam (`MyBox.cs:41–45`) — but `UtilitiesCS/Properties/AssemblyInfo.cs:18–20` grants `InternalsVisibleTo` only to `DynamicProxyGenAssembly2`, `UtilitiesCS.Test`, and `ToDoModel.Test`. **`QuickFiler.Test` is not on that list**, so the existing seam is unreachable from this child's tests. Any test that executed line 304 or 350 today would display a modal dialog requiring human interaction — a direct violation of the unit-test policy. Adding `InternalsVisibleTo("QuickFiler.Test")` to `UtilitiesCS` is **out of scope** (shared, non-F3-owned file, and it would leak an unrelated assembly's internals).

**Proposed shape (new files, F3-authored):**

- `QuickFiler/Interfaces/IQfcDialogPrompt.cs` — one member:
  `DialogResult ShowActionDialog(string message, string title, BoxIcon icon, Dictionary<string, Action> actions);`
- `QuickFiler/Interfaces/MyBoxDialogPrompt.cs` — `sealed class MyBoxDialogPrompt : IQfcDialogPrompt`, a 1:1 forward to `MyBox.ShowDialog`.

**Why level 1 and not level 2.** There are two call sites sharing one 4-argument shape with a return value; a named interface is Moq-verifiable with argument matchers on the `Dictionary<string, Action>` payload, which is the assertion that actually matters (that the *right* `RightKeyActions` dictionary reaches the dialog). A bare `Func<...>` would carry the same arity with no self-documenting name. Level 1 is feasible, so it wins.

**Precedent, in this child's own file set:** `QuickFiler/Interfaces/IMailItemActions.cs` (35 lines) + `QuickFiler/Interfaces/MailItemActionsAdapter.cs` (47 lines), whose XML doc at lines 5–11 states the pattern verbatim ("DI-seam 'adapter' tier, research §3.4.3 ... forwards every member 1:1"). `MailItemActionsAdapter` is covered by `QuickFiler.Test/Controllers/MailItemActionsAdapterTests.cs`. **Important difference:** `MailItemActionsAdapter` wraps `MailItem`, a *mockable COM interface*, so its forwards are fully coverable. `MyBox` is a *static class*, so `MyBoxDialogPrompt`'s single forwarding statement is **not** coverable without showing a dialog. That one statement is a ledger-ratification request (§7).

**Injection:** constructor parameter, defaulted (see Seam K3).

### Seam K2 — reuse the existing `UtilitiesCS.Threading.IUiDispatcher` (hierarchy level 1: interface seam; already exists — do not create anything)

**Blocks removed:** `UiThread.Dispatcher.Invoke(...)` at lines 362 and 370, and `UiThread.Dispatcher.InvokeAsync(...)` at line 401.

`UtilitiesCS/Threading/UiThread.cs:135–140` exposes `public static Dispatcher Dispatcher` backed by `private static Dispatcher _dispatcher = null!` set only in `Initialize()`. In a unit-test process it is null, so any test that reached lines 362/370/401 would throw a `NullReferenceException` — and if it were seeded, the lambda body `cbo.DroppedDown = false` would force real window-handle creation on the `ComboBox`.

`UtilitiesCS/Threading/IUiDispatcher.cs:15–42` already abstracts exactly this, with `WpfUiDispatcher` as the 1:1 production implementation (`UtilitiesCS/Threading/WpfUiDispatcher.cs:11`). It is already consumed by `QuickFiler/Controllers/QfcItemController.cs:66` (injected at `QfcItemController.Initialization.cs:38` as an optional parameter), `QuickFiler/Helper Classes/QfcThemeControlSet.cs:33,98`, and `QuickFiler/Helper Classes/QfcThemeHelper.cs:40,61`. Tests build synchronous doubles with `QfcItemController.TestSupport.BuildSyncDispatcher()` (`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:102`).

**Why level 1:** the interface already exists and is the repo's established answer. Creating a delegate or a new adapter would be duplication and would fail `.claude/rules/csharp.md` § Prohibited Behaviors ("introducing heavy generic abstraction frameworks without need").

**Test technique note:** a Moq `IUiDispatcher` that *records but does not execute* the `Action` is what keeps the `cbo.DroppedDown = true/false` assignments from ever running, which is what keeps the tests handle-free. Assert on `Verify(d => d.Invoke(It.IsAny<Action>()), Times.Once())` — that is the observable intent, and it matches how `BreadcrumbPopupUiOperations`/`QfcThemeControlSet` are already asserted.

### Seam K3 — additive core constructor with defaulted seam parameters (the injection point for K1/K2/K4)

Both existing public constructors delegate to a new `private KeyboardHandler(IFilerHomeController parent, IQfcDialogPrompt prompt, IUiDispatcher dispatcher, Func<ComboBox, bool> isDroppedDown)` core, and each public constructor gains **optional trailing parameters** defaulted to `null`, resolved inside the core to `new MyBoxDialogPrompt()`, `new WpfUiDispatcher()`, and `cb => cb.DroppedDown` respectively.

```
public KeyboardHandler(
    IQfcFormViewer viewer,
    IFilerHomeController parent,
    IQfcDialogPrompt prompt = null,
    IUiDispatcher uiDispatcher = null,
    Func<ComboBox, bool> isDroppedDown = null)
```

This mirrors `QfcItemController.Initialization.cs:38` exactly (`UtilitiesCS.Threading.IUiDispatcher uiDispatcher = null` as a defaulted trailing parameter). **Defaulting to `null` and resolving inside is required over defaulting to `new WpfUiDispatcher()` in the parameter list**, because a C# optional-parameter default must be a compile-time constant.

**Hierarchy note:** K3 is not itself one of the three seam tiers; it is the wiring that delivers K1, K2, and K4. It is listed separately because it is the member whose *shape* the cross-child analysis in §6 turns on.

### Seam K4 — `Func<ComboBox, bool>` dropped-down predicate (hierarchy level 2: injectable delegate)

**Block removed:** line 278, `if (cb.DroppedDown)`.

`ComboBox.DroppedDown`'s getter returns `false` unconditionally when the control has no window handle, so a handle-free `new ComboBox()` can never drive the `true` branch, and the setter force-creates a handle. The `true` branch of `CboFolders_KeyDownAsync` is therefore unreachable without either a real window or a seam.

**Why not level 1 (interface).** An interface seam here would mean abstracting the `ComboBox` itself. But `CboFolders_KeyDownAsync(object sender, KeyEventArgs e)` is a `KeyEventHandler`-shaped **interface member** (`IQfcKeyboardHandler.cs:28`) wired directly to `IItemViewer.FolderKeyDown` at `QuickFiler/Controllers/QfcItemController.EventWiring.cs:82`. The `sender` is whatever WinForms hands over. Abstracting it would change the interface contract (breaking, per §6) and would require touching F10-owned and F14-owned viewer files. A single one-property boolean read does not justify that. Level 1 is **not feasible without a breaking cross-child change**, so level 2 applies.

**Default `cb => cb.DroppedDown` preserves production behavior bit-for-bit.**

Acceptable alternative if the planner prefers zero new production surface: skip K4, test `DdOpen_KeyDownAsync` and `DdClosed_KeyDownAsync` directly (both are already `public`), and accept one uncovered branch in a 20-line method. K4 is recommended because it is 1 field + 1 defaulted parameter and closes the branch cleanly.

### Seam K5 — `EnsureUiSyncContext()` / `EnsureWinFormsSyncContext()` private helpers (refactor, not a seam)

The pattern `if (SynchronizationContext.Current is null) SynchronizationContext.SetSynchronizationContext(...)` is duplicated **seven times** in two variants:

- Parent-context variant (lines 106–107, 135–136, 152–153, 240–241) → `_parent.UiSyncContext`
- WinForms-context variant (lines 268–271, 319–322, 393–396) → `new WindowsFormsSynchronizationContext()`

Extracting each into a private helper satisfies the General Code Change Policy's reusability principle, reduces 14 duplicated lines to 2 helpers, and — relevant here — collapses seven separately-uncovered branch pairs into two. **This is a pure refactor with identical observable behavior.** It is optional; if the planner prefers minimum diff, all seven sites are individually testable and this can be dropped.

`new WindowsFormsSynchronizationContext()` is constructible without a window handle (it captures the current thread), so no seam is needed for it. Tests **must** snapshot and restore `SynchronizationContext.Current` in a `finally`/`IDisposable` scope — precedent `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs:353–373`.

### §5.3 — What needs no seam at all (recorded so the planner does not over-engineer)

- **`ItemViewer` construction is already proven headless in ordinary `[TestClass]` files.** Three independent test classes construct a real `new QuickFiler.ItemViewer()` and dispose it: `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs:363`, `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs:477`, and `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:373`. `ItemViewer.Designer.cs` (6,224 lines) contains 64 `WebView2`/`FastObjectListView`/`ButtonSVG` occurrences and still constructs cleanly. **No `*.StaTests.cs` file exists in `QuickFiler.Test`**, and none is needed for `ItemViewer`.
- **`viewer.SetFolderDroppedDown(false)` (line 313) is inert on a bare viewer.** `QuickFiler/Viewers/ItemViewer.FolderSearch.cs:31–32` forwards to `SetBreadcrumbDropDownState`, which at `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:223–232` returns immediately when `_breadcrumbLifecycleCoordinator == null` and `droppedDown == false`. It touches no `ComboBox` and creates no handle. The `Left` branch of `BreadcrumbArrowFallThrough` is therefore fully testable with a plain `new ItemViewer()` — **the STA last-resort clause does not apply.**
- **`viewer.Controller` is `IItemControler`** (`QuickFiler/Viewers/IItemViewer.cs:17`, settable), and `RightKeyActions` is `Dictionary<string, System.Action>` (`QuickFiler/Interfaces/IItemControler.cs:13`). A Moq `IItemControler` supplies the dictionary with no COM.
- **`cbo.GetAncestor<ItemViewer>()`** (`UtilitiesCS/Extensions/WinFormsExtensions.cs:176–195`) is a pure parent-chain walk, already unit-tested at `UtilitiesCS.Test/Extensions/WinFormsExtensions_Tests.cs:118–161`. A test builds the chain with `itemViewer.Controls.Add(comboBox)` — adding to a `Controls` collection does not create a handle.
- **`KeyEventArgs` / `PreviewKeyDownEventArgs`** are plain in-memory argument objects (`new KeyEventArgs(Keys.Right)`, `new PreviewKeyDownEventArgs(Keys.Down)`) with public `Handled`, `SuppressKeyPress`, `IsInputKey`, `KeyCode`, `KeyValue`. No form, no handle, no UI thread.
- **`KbdActions<>`, `KaChar`, `KaKey`, `KaKeyAsync`, `KaCharAsync`, `KaStringAsync`** are first-party, host-neutral, already covered by `KbdActionsTests.cs`, `KbdActionsRemainingBranchesTests.cs`, `KaCharTests.cs`, `KaKeyTests.cs`, `KaStringAsyncTests.cs`. Use the real types in `KeyboardHandler` tests rather than mocking them — that is what `QfcCollectionControllerTests.cs:332` already documents doing ("a real `KbdActions` behind a Loose `IQfcKeyboardHandler`").

---

## 6. Cross-Child Contract Impact

### 6.1 Construction sites of `KeyboardHandler` (the concrete type)

Exhaustive; verified by grep for `new KeyboardHandler` across all `*.cs`.

| # | File : line | Owning child | Call shape |
| --- | --- | --- | --- |
| C1 | `QuickFiler/Controllers/QfcHomeController.cs:184–189` | **F7** (`quickfiler-qfc-home-controller-coverage`) | `internal Func<IQfcFormViewer, IFilerHomeController, IQfcKeyboardHandler> QfcKeyboardHandlerLoader { get; set; } = (formViewer, homeController) => new KeyboardHandler(formViewer, homeController);` |
| C2 | `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:141–147` | **F8** (`quickfiler-efc-home-controller-coverage`) | `private static IQfcKeyboardHandler CreateProductionKeyboardHandlerInstance(EfcViewer viewer, EfcHomeController homeController) { return new KeyboardHandler(viewer, homeController); }` |

Both are **two-argument** invocations. Adding **optional trailing parameters** (Seam K3) leaves both expressions compiling unchanged. Overload resolution is unaffected: the new private core constructor has a distinct first-parameter type (`IFilerHomeController`) and cannot be selected by either call.

**Determination for K1, K2, K3, K4, K5: ADDITIVE.** No sibling-owned file requires edit.

Indirect factory indirection that also stays intact:
- `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:201–207` (`CreateProductionKeyboardHandler` → `ProductionKeyboardHandlerConstructor`)
- `QuickFiler/Controllers/EfcHomeControllerDependencies.cs:51, 175, 187–190` (`Func<EfcViewer, EfcHomeController, IQfcKeyboardHandler>` factory field and `CreateKeyboardHandlerWithFactory`)
- Test-side factory doubles: `QuickFiler.Test/Controllers/EfcHomeControllerDependenciesTests.cs:63, 188`, `EfcHomeControllerDependenciesProductionFactoryTests.cs:400, 437, 464`, `EfcHomeControllerLifecycleTests.cs:179, 301`, `EfcHomeControllerSeamTests.cs:230`, `QfcFormControllerSeamTests.cs:115`. All bind to `IQfcKeyboardHandler`, not to the concrete constructor.

### 6.2 Consumers of `IQfcKeyboardHandler` (the interface — must not change)

| File : line | Owning child | Member consumed |
| --- | --- | --- |
| `QuickFiler/Interfaces/IFilerHomeController.cs:32` | F3 (interface decl) / consumed everywhere | `IQfcKeyboardHandler KeyboardHandler { get; set; }` |
| `QuickFiler/Interfaces/IQfcHomeController.cs:10` | F7 | `IQfcKeyboardHandler KbdHndlr { get; set; }` |
| `QuickFiler/Interfaces/IQfcFormViewer.cs:21` | F6 | `void SetKeyboardHandler(IQfcKeyboardHandler)` |
| `QuickFiler/Controllers/QfcHomeController.cs:187, 421–422` | F7 | field + property |
| `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:160, 188` | F6 | `KeyboardHandler_PreviewKeyDownAsync` |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:127` | F6 | `ToggleKeyboardDialog()` |
| `QuickFiler/Controllers/QfcItemController.cs:49` | F10 | `_kbdHandler` field |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs:41` | F10 | `KeyboardHandler_PreviewKeyDownAsync` |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs:82` | F10 | `CboFolders_KeyDownAsync` |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:187` | F10 | `BreadcrumbArrowFallThrough(viewer, direction)` |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs:29, 42, 53, 60, 67, 76` | F10 | `ToggleKeyboardDialog()` / `ToggleKeyboardDialogAsync()` |
| `QuickFiler/Controllers/QfcCollectionController.cs:75, 1146, 1218` | F11 | field + both toggles |
| `QuickFiler/Controllers/EfcHomeController.cs:369–370` | F8 | field + property |
| `QuickFiler/Controllers/EfcItemController.cs:374, 649, 688, 691, 696–734, 879–903, 1146, 1152` | F9 | `KeyboardHandler_PreviewKeyDownAsync`, `CharActions`, `CharActionsAsync`, `ToggleKeyboardDialogAsync` |
| `QuickFiler/Controllers/EfcFormController.cs:358, 372–373, 379, 814, 820, 826, 919, 926–951` | F9 | `AlwaysOnKeyActionsAsync`, `CharActions`, `CharActionsAsync`, `KeyboardHandler_PreviewKeyDownAsync`, `ToggleKeyboardDialogAsync` |
| `QuickFiler/Viewers/QfcFormViewer.cs:32, 51, 68` | F15 | field, `SetKeyboardHandler`, `ToggleKeyboardDialogAsync()` |
| `QuickFiler/Viewers/QfcFormViewerExpanded.cs:29, 36` | **unassigned — see §8** | field, `SetKeyboardHandler` |
| `QuickFiler/Viewers/QfcFormViewerDark.cs:29, 36` | **unassigned — see §8** | field, `SetKeyboardHandler` |
| `QuickFiler/Viewers/EfcViewer.cs:55–64, 100` | F9 | field, `KeyboardHandler` getter, `SetKeyboardHandler`, `ToggleKeyboardDialogAsync(sender, e)` |
| `QuickFiler/Viewers/EfcViewer3.cs:44, 46, 81` | **unassigned (dead type)** | field, `SetKeyboardHandler`, `ToggleKeyboardDialog(sender, e)` |
| `QuickFiler.Test/**` (17 files listed in §3) | various | `Mock<IQfcKeyboardHandler>` |

**Mandate: `IQfcKeyboardHandler` is frozen for this child.** No member is added, removed, renamed, or re-typed. All five proposed seams live on the concrete `KeyboardHandler` class and in new files. **Determination: ADDITIVE.**

### 6.3 Changes explicitly REJECTED as breaking

| Tempting change | Why rejected |
| --- | --- |
| Widen `IQfcKeyboardHandler.BreadcrumbArrowFallThrough(ItemViewer, ...)` to `IItemViewer` | It happens to be *source-compatible* for every in-repo caller (`QfcItemController.ViewerSetup.cs:187` passes a concrete `ItemViewer`; the Moq setup at `QfcItemControllerBreadcrumbDropDownTests.cs:163` still compiles). But it is an **interface signature change** on a contract consumed by F6/F9/F10/F11/F15, and the epic mandate is that F3's change remains additive. **Minimum breaking delta if ever pursued:** one line in `QuickFiler/Interfaces/IQfcKeyboardHandler.cs:33` plus one line in `KeyboardHandler.cs:293`, with re-verification of the F10 Moq setup. **Not required** — §5.3 shows the concrete `ItemViewer` is testable headlessly, so there is no benefit. |
| Add an `IKeyboardHandlerHost { void SetKeyboardHandler(IQfcKeyboardHandler); }` and make `EfcViewer` implement it, to make ctor #2 testable | Would require editing `QuickFiler/Viewers/EfcViewer.cs`, an **F9-owned** file. Prohibited. |
| Change `CboFolders_KeyDownAsync(object, KeyEventArgs)` to take a typed parameter | Breaks the `KeyEventHandler` delegate conversion at `QfcItemController.EventWiring.cs:82` (F10-owned). Prohibited. |
| Add `ArgumentNullException` guards to the constructors | Behavior change; violates issue #430 AC "No behavior change to observable QuickFiler keyboard flows". Record as a follow-up issue instead (§8). |
| Add `InternalsVisibleTo("QuickFiler.Test")` to `UtilitiesCS` to reach `MyBox.DialogInvoker` | Modifies a shared, non-F3 assembly's public-surface policy for one child's convenience. Seam K1 achieves the same end inside F3's own boundary. |

---

## 7. Irreducible Remainder (F1 ledger ratification candidates)

Only two remainders survive the seam analysis.

### R1 — `KeyboardHandler(EfcViewer viewer, IFilerHomeController parent)`, lines 35–39 (5 lines)

`EfcViewer` is `public partial class EfcViewer : Form` (`QuickFiler/Viewers/EfcViewer.cs:20–21`), itself `[ExcludeFromCodeCoverage]`. Its constructor (lines 23–30) runs `InitializeComponent()` against `EfcViewer.Designer.cs` (4,276 lines) and calls `TaskScheduler.FromCurrentSynchronizationContext()`, which throws `InvalidOperationException` when `SynchronizationContext.Current` is null.

**Evidence that this may in fact be reducible:** `EfcViewer.Designer.cs` contains 12 `WebView2`/`FastObjectListView`/`ButtonSVG` occurrences versus 64 in `ItemViewer.Designer.cs`, and `ItemViewer` already constructs headlessly in three ordinary `[TestClass]` files (§5.3). The `SynchronizationContext.Current` precondition is satisfiable by the same scope pattern already used at `BreadcrumbPendingOpenCloseTests.cs:359–362`.

**Recommendation:** the plan should include one exploratory task that attempts `new EfcViewer()` inside a `SynchronizationContext` scope and asserts `viewer.KeyboardHandler` (internal getter, `EfcViewer.cs:56–59`, reachable via `InternalsVisibleTo`) is the handler. If it constructs, R1 is covered and no ledger entry is needed. If it fails (handle creation, designer resource, or `Form`-specific initialization), **ratify lines 35–39 as `irreducible-remainder`** with the reason: *"the parameter type is a concrete `Form`-derived, already-exempt sibling-owned viewer (F9); the only non-breaking alternative would require adding an interface to `EfcViewer.cs`, which is a sibling-owned file."* If it fails, the fallback test is a dedicated `QuickFiler.Test/Controllers/KeyboardHandler.StaTests.cs` per epic.md §3, with the written justification above.

Impact if ratified: 5 of 414 lines = 1.2%. Does not endanger the 80% floor.

### R2 — the single forwarding statement in the new `MyBoxDialogPrompt` adapter

`MyBoxDialogPrompt.ShowActionDialog` is one expression forwarding to the static `MyBox.ShowDialog`, which constructs and shows a `MyBoxViewer` form. Because `UtilitiesCS` does not grant `InternalsVisibleTo("QuickFiler.Test")`, the `DialogInvoker` stub is unreachable and the statement cannot be executed without a human-interactive modal dialog.

**Recommendation:** request an F1 ledger entry classifying `QuickFiler/Interfaces/MyBoxDialogPrompt.cs` as `ratified-exempt` under the "thinnest possible wiring in the host-bound entry point" standard of `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy. The file must contain nothing but the constructor-free forward — no branching, no state. **The interface file `IQfcDialogPrompt.cs` is interface-only with no executable behavior and is outside the coverage denominator by the same rule.**

Alternative if F1 declines: keep the default as a `private static readonly Func<...>` field inside `KeyboardHandler.cs` itself rather than a separate file, which reduces R2 to one line of `KeyboardHandler.cs` and creates no new ledger entry. Slightly worse for separation of concerns; note the trade for the planner.

### Not a remainder

`UiThread.Dispatcher`, `ComboBox.DroppedDown`, `SynchronizationContext`, `ItemViewer`, `MyBox` call *sites*, all seven properties, both toggle pairs, all key-routing methods, and `GetItemViewer` are all reducible. **No further exemption is warranted.**

---

## 8. File-Size Compliance and Split Decision

Current: 414 lines. Ceiling: 500 (General Code Change Policy §4.1; epic.md Shared Design §5).

| Delta | Lines |
| --- | --- |
| 3 new private readonly fields (`_prompt`, `_uiDispatcher`, `_isDroppedDown`) | +3 |
| Private core constructor with null-coalescing resolution | +12 |
| Optional parameters added to both public constructors (csharpier will wrap each parameter onto its own line) | +14 |
| Two `EnsureSyncContext` helpers (K5) | +10 |
| Removal of 7 duplicated 2-line sync-context blocks (K5) | −14 |
| XML doc comments on the two constructors and the seam members | +20 |
| Removal of the 3 unused `using` directives (lines 12, 14, 15) | −3 |
| **Projected total** | **~456** |

**Determination: no partial split required.** The file stays under 500 with ~44 lines of headroom.

**Contingency split, only if the ceiling is threatened** (for example if the planner adds XML docs to all 13 methods): split along the existing conceptual boundary at line 262 —

- `KeyboardHandler.cs` — usings, ctors, fields, all 7 properties, `ClearFilter`, `KeyboardHandler_PreviewKeyDown[Async]`, `KeyboardHandler_KeyDown[Async]`, `KeyDownTaskAsync`, all four `ToggleKeyboardDialog*` (lines 1–245 today, ~280 after) 
- `KeyboardHandler.FolderRouting.cs` — `GetItemViewer`, `CboFolders_KeyDownAsync`, `BreadcrumbArrowFallThrough`, `DdOpen_KeyDownAsync`, `DdClosed_KeyDownAsync` (lines 247–412 today, ~180 after)

This requires adding `partial` to the class declaration and one `<Compile Include>` entry in `QuickFiler/QuickFiler.csproj` — both F3-owned. Do **not** perform this split unless the measured line count exceeds 500.

---

## 9. Proposed Test Cases

Per epic.md, each case below becomes its **own atomic plan task**. Framework: MSTest `[TestClass]`/`[TestMethod]`, Moq, FluentAssertions, Arrange–Act–Assert. All tests live under `QuickFiler.Test/Controllers/` mirroring `QuickFiler/Controllers/`.

**Shared support file (not a test case):** `QuickFiler.Test/Controllers/KeyboardHandler.TestSupport.cs` — an internal static helper providing (a) `BuildHandler(...)` returning a `KeyboardHandler` plus its `Mock<IQfcFormViewer>`, `Mock<IFilerHomeController>`, `Mock<IFilerFormController>`, `Mock<IQfcDialogPrompt>`, `Mock<IUiDispatcher>`; (b) a `SyncContextScope : IDisposable` that snapshots and restores `SynchronizationContext.Current` (pattern from `BreadcrumbPendingOpenCloseTests.cs:353–373`); (c) an `InlineSynchronizationContext` whose `Post` invokes the callback synchronously (pattern from `BreadcrumbPendingOpenCloseTests.cs:375–378`). This is what makes the `async void` cases deterministic **with no `Thread.Sleep`, no `Task.Delay`, and no wall-clock wait**.

### File A — `KeyboardHandler.ConstructionTests.cs`

| # | Test method | Arrange / Act / Assert | Seam / mock |
| --- | --- | --- | --- |
| 1 | `Constructor_WithFormViewer_RegistersItselfWithViewer` | A: `Mock<IQfcFormViewer>`, `Mock<IFilerHomeController>`. Act: construct. Assert: `viewer.Verify(v => v.SetKeyboardHandler(It.IsAny<IQfcKeyboardHandler>()), Times.Once())` and the captured argument `Should().BeSameAs(handler)`. | K3 |
| 2 | `Constructor_WithFormViewer_DefaultsAllSixActionCollectionsToEmptyNotNull` | A: build. Act: read all six `KbdActions<>` properties. Assert: each `Should().NotBeNull()` and `.Keys.Should().BeEmpty()`. | none |
| 3 | `Constructor_WithFormViewer_DefaultsKbdActiveToFalse` | A: build. Act: read `KbdActive`. Assert: `Should().BeFalse()`. | none |
| 4 | `Constructor_WithFormViewer_UsesSuppliedDialogPromptOverProductionDefault` | A: build with an explicit `Mock<IQfcDialogPrompt>`. Act: drive `BreadcrumbArrowFallThrough(viewer, Right)`. Assert: the supplied prompt received the call (proves the optional parameter is wired, not ignored). | K1, K3 |
| 5 | `Constructor_WithNullFormViewer_ThrowsNullReferenceException` | A: `null` viewer. Act: construct. Assert: `Should().Throw<NullReferenceException>()`. **Characterization only** — pins current behavior; do not "fix" it (see §6.3). | none |
| 6 | `Constructor_WithEfcViewer_RegistersItselfWithViewer` | A: `SyncContextScope`; `new EfcViewer()`; `Mock<IFilerHomeController>`. Act: construct. Assert: `viewer.KeyboardHandler.Should().BeSameAs(handler)` (internal getter, `EfcViewer.cs:56`). **Contingent on R1** — if `new EfcViewer()` fails headlessly, this task is replaced by the F1 ledger request in §7. | K3 |

### File B — `KeyboardHandler.PropertiesTests.cs`

| # | Test method | A/A/A sketch | Seam |
| --- | --- | --- | --- |
| 7 | `CharActions_SetThenGet_RoundTripsSameInstance` | A: build + `new KbdActions<char, KaChar, Action<char>>()`. Act: set, get. Assert: `BeSameAs`. | none |
| 8 | `CharActionsAsync_SetThenGet_RoundTripsSameInstance` | as above | none |
| 9 | `KeyActions_SetThenGet_RoundTripsSameInstance` | as above | none |
| 10 | `KeyActionsAsync_SetThenGet_RoundTripsSameInstance` | as above | none |
| 11 | `AlwaysOnKeyActionsAsync_SetThenGet_RoundTripsSameInstance` | as above | none |
| 12 | `StringActionsAsync_SetThenGet_RoundTripsSameInstance` | as above | none |
| 13 | `KbdActive_SetTrue_GetReturnsTrue` | A: build. Act: `KbdActive = true`. Assert: `BeTrue`. | none |
| 14 | `ClearFilter_AfterPartialFilterAccumulation_DiscardsPendingPrefix` | A: build, `KbdActive = true`, register `KaStringAsync("src","ab",...)`. Act: `KeyDownTaskAsync` with `'a'`, then `ClearFilter()`, then `KeyDownTaskAsync` with `'b'`. Assert: the `"ab"` action was **not** invoked (the accumulator restarted). | none |

### File C — `KeyboardHandler.PreviewKeyDownTests.cs`

| # | Test method | A/A/A sketch | Seam |
| --- | --- | --- | --- |
| 15 | `PreviewKeyDown_KbdInactive_LeavesIsInputKeyFalse` | A: `KbdActive=false`, `KeyActions` containing `Keys.Down`. Act: call with `new PreviewKeyDownEventArgs(Keys.Down)`. Assert: `IsInputKey.Should().BeFalse()`. | none |
| 16 | `PreviewKeyDown_KbdActiveAndKeyRegistered_SetsIsInputKeyTrue` | A: `KbdActive=true`, register `Keys.Down`. Act/Assert: `IsInputKey` true. | none |
| 17 | `PreviewKeyDown_KbdActiveAndKeyNotRegistered_LeavesIsInputKeyFalse` | boundary: registered `Keys.Up`, pressed `Keys.Down`. | none |
| 18 | `PreviewKeyDown_KeyActionsNull_LeavesIsInputKeyFalse` | invalid-state: `KeyActions = null`, `KbdActive = true`. Assert no throw, `IsInputKey` false (pins the null guard at line 98). | none |
| 19 | `PreviewKeyDownAsync_WithNullAmbientContext_InstallsParentSyncContext` | A: `SyncContextScope` sets `Current` to null; parent mock returns a known `SynchronizationContext`. Act: call. Assert: `SynchronizationContext.Current.Should().BeSameAs(parentContext)`. | none |
| 20 | `PreviewKeyDownAsync_WithExistingAmbientContext_DoesNotReadParentContext` | A: `SyncContextScope` sets a non-null context; parent mock `MockBehavior.Strict` with **no** `UiSyncContext` setup. Act: call. Assert: no exception (pins the short-circuit at line 106). | none |
| 21 | `PreviewKeyDownAsync_KbdActiveAndAsyncKeyRegistered_SetsIsInputKeyTrue` | positive path on `KeyActionsAsync`. | none |
| 22 | `PreviewKeyDownAsync_KbdInactive_LeavesIsInputKeyFalse` | negative path. | none |
| 23 | `PreviewKeyDownAsync_KeyActionsAsyncNull_LeavesIsInputKeyFalse` | invalid-state guard at line 108. | none |

### File D — `KeyboardHandler.KeyDownSyncTests.cs`

| # | Test method | A/A/A sketch | Seam |
| --- | --- | --- | --- |
| 24 | `KeyDown_KbdInactive_InvokesNoAction` | A: `KbdActive=false`, both `KeyActions` and `CharActions` populated with recording delegates. Act: `KeyboardHandler_KeyDown`. Assert: neither recorded; `Handled` false. | none |
| 25 | `KeyDown_RegisteredKeyAction_SuppressesKeyPressAndInvokesWithKeyCode` | A: `KbdActive=true`, `KaKey("src", Keys.Delete, k => captured = k)`. Assert: `SuppressKeyPress` true, `Handled` true, `captured == Keys.Delete`. | none |
| 26 | `KeyDown_RegisteredCharAction_SuppressesKeyPressAndInvokesWithChar` | A: `CharActions` with `KaChar("src",'r',...)`; press `Keys.R` (`KeyValue == 82`). **Boundary note:** line 124 casts `(char)e.KeyValue`, which yields `'R'` (uppercase) — the registered key must match that exact char. Assert on the captured char. | none |
| 27 | `KeyDown_KeyActionAndCharActionBothRegistered_PrefersKeyAction` | precedence: register both for the same physical key; assert only the `KeyActions` delegate ran. | none |
| 28 | `KeyDown_NoMatchingAction_LeavesEventUnhandled` | negative: neither collection matches. Assert `Handled` false, `SuppressKeyPress` false. | none |
| 29 | `KeyDown_KeyActionsNullAndCharActionRegistered_FallsThroughToCharAction` | invalid-state: `KeyActions = null`. Pins the `else if` at line 124. | none |

### File E — `KeyboardHandler.KeyDownTaskTests.cs`

All `async Task` test methods; the SUT method returns `Task` so `await` is deterministic without any timer.

| # | Test method | A/A/A sketch | Seam |
| --- | --- | --- | --- |
| 30 | `KeyDownTaskAsync_AlwaysOnKeyRegistered_InvokesEvenWhenKbdInactive` | A: `KbdActive=false`, `AlwaysOnKeyActionsAsync` has `Keys.Escape`. Assert: invoked; `Handled` true. Pins lines 155–160 as gated **only** by registration. | none |
| 31 | `KeyDownTaskAsync_KbdInactive_DoesNotInvokeKeyActionsAsync` | negative for lines 162–169. | none |
| 32 | `KeyDownTaskAsync_KbdActiveAndKeyAsyncRegistered_SuppressesAndAwaitsAction` | positive path lines 164–169. | none |
| 33 | `KeyDownTaskAsync_KbdActiveAndCharAsyncRegistered_SuppressesAndAwaitsAction` | positive path lines 170–177. | none |
| 34 | `KeyDownTaskAsync_KeyAsyncAndCharAsyncBothRegistered_PrefersKeyAsync` | precedence. | none |
| 35 | `KeyDownTaskAsync_AlwaysOnAndKeyAsyncBothRegistered_InvokesBothInOrder` | ordering: assert an invocation-order list is `["alwaysOn","key"]`. | none |
| 36 | `KeyDownTaskAsync_FirstFilterCharacter_ActivatesAllStringActions` | pins line 186–187 (`_filterBuilder.Length == 1` → `ForEach(x => x.Activated = true)`). Assert each registered `KaStringAsync.Activated` observed true during matching. | none |
| 37 | `KeyDownTaskAsync_StringFilterUniqueMatch_InvokesActionAndResetsFilter` | A: single `KaStringAsync` with key `"a"`. Act: press `'a'`. Assert: delegate invoked with `"a"`; a following unrelated press starts a fresh filter. Pins lines 191–196. | none |
| 38 | `KeyDownTaskAsync_StringFilterAmbiguousPrefix_RetainsFilterWithoutInvoking` | A: two `KaStringAsync` keys `"ab"`, `"ac"`. Act: press `'a'`. Assert: neither delegate invoked, `Handled` true, and a subsequent `'b'` resolves to the `"ab"` delegate. Pins the implicit `>1` fall-through at line 191. | none |
| 39 | `KeyDownTaskAsync_StringFilterUnmatchedCharacter_RollsBackFilterLength` | A: keys `"ab"`. Act: press `'z'`. Assert: `Handled` false; a following `'a'` then `'b'` still resolves `"ab"` (proving line 200 rolled the buffer back). | none |
| 40 | `KeyDownTaskAsync_StringActionsAsyncNull_LeavesEventUnhandled` | invalid-state guard at line 178. | none |
| 41 | `KeyDownTaskAsync_UppercaseKeyValue_IsLowercasedBeforeFilterMatch` | boundary for `char.ToLower` at line 180: register key `"a"`, press `Keys.A` (`KeyValue == 65` → `'A'`). Assert the action fires. | none |
| 42 | `KeyDownTaskAsync_ContainsKeyThenFilterKeys_InvokesKeyEqualsTwicePerAction` | characterization of the double side-effecting evaluation described in §4. Use a `KaStringAsync` with a counting `Update` delegate. Documents current behavior so a later refactor cannot change it silently. | none |
| 43 | `KeyDownTaskAsync_WithNullAmbientContext_InstallsParentSyncContext` | pins lines 152–153. | none |

### File F — `KeyboardHandler.AsyncVoidTests.cs`

| # | Test method | A/A/A sketch | Seam |
| --- | --- | --- | --- |
| 44 | `KeyboardHandler_KeyDownAsync_DelegatesToKeyDownTaskAsync` | A: `SyncContextScope` installing `InlineSynchronizationContext`; register an `AlwaysOnKeyActionsAsync` returning `Task.CompletedTask` and recording. Act: call the `async void` method. Assert: recorded exactly once — deterministic because every awaited task is already completed, so no continuation is ever posted. **No sleep, no delay, no wall-clock wait.** | K5 |
| 45 | `KeyboardHandler_KeyDownAsync_ActionThrows_SwallowsExceptionAndDoesNotPropagate` | A: register an async action that throws. Act: call. Assert: `Should().NotThrow()` and the handler remains usable (a subsequent successful dispatch still works). Pins the `catch` at lines 141–147. | K5 |
| 46 | `KeyboardHandler_KeyDownAsync_WithNullAmbientContext_InstallsParentSyncContext` | pins lines 135–136. | K5 |
| 47 | `ToggleKeyboardDialogAsyncEventOverload_MarksEventHandledAndTogglesState` | A: inline context; `Mock<IFilerFormController>` returning `Task.CompletedTask`. Act: call the `async void` overload. Assert: `e.Handled` true and `KbdActive` flipped. Pins lines 238–245. | K5 |

### File G — `KeyboardHandler.ToggleTests.cs`

| # | Test method | A/A/A sketch | Seam |
| --- | --- | --- | --- |
| 48 | `ToggleKeyboardDialog_WhenInactive_CallsToggleOnNavigationAndActivates` | Assert `ToggleOnNavigation(false)` once and `KbdActive` true. Pins lines 212–216. | none |
| 49 | `ToggleKeyboardDialog_WhenActive_CallsToggleOffNavigationAndDeactivates` | Assert `ToggleOffNavigation(false)` once and `KbdActive` false. Pins lines 208–210, 216. | none |
| 50 | `ToggleKeyboardDialog_EventOverload_MarksEventHandled` | Assert `e.Handled` true. Pins lines 219–223. | none |
| 51 | `ToggleKeyboardDialogAsync_WhenInactive_AwaitsToggleOnNavigationAsync` | Assert `ToggleOnNavigationAsync()` once, `KbdActive` true. Pins lines 231–235. | none |
| 52 | `ToggleKeyboardDialogAsync_WhenActive_AwaitsToggleOffNavigationAsync` | Assert `ToggleOffNavigationAsync()` once, `KbdActive` false. Pins lines 227–229. | none |
| 53 | `ToggleKeyboardDialog_CalledTwice_ReturnsToOriginalState` | state-transition completeness (General Unit Test Policy § Scenario Completeness). | none |

### File H — `KeyboardHandler.ComboBoxRoutingTests.cs`

| # | Test method | A/A/A sketch | Seam |
| --- | --- | --- | --- |
| 54 | `CboFolders_KeyDownAsync_NonComboBoxSender_ReturnsWithoutRouting` | A: inline context, `Mock<IUiDispatcher>(MockBehavior.Strict)`. Act: call with `new object()` as sender. Assert: `dispatcher.VerifyNoOtherCalls()`, `e.Handled` false. Pins the #351 early return at lines 272–277. | K2, K5 |
| 55 | `CboFolders_KeyDownAsync_ClosedComboBox_RoutesToDdClosedPath` | A: `new ComboBox()` (handle-free, `DroppedDown` false), press `Keys.Right`. Assert: `dispatcher.Verify(d => d.InvokeAsync(It.IsAny<Action>()), Times.Once())` — the `DdClosed` Right branch. | K2, K4, K5 |
| 56 | `CboFolders_KeyDownAsync_DroppedDownComboBox_RoutesToDdOpenPath` | A: inject `isDroppedDown: _ => true`. Press `Keys.Escape`. Assert: `dispatcher.Verify(d => d.Invoke(It.IsAny<Action>()), Times.Once())` — the `DdOpen` close branch. | **K4**, K2, K5 |
| 57 | `DdOpen_KeyDownAsync_Up_LeavesEventUnhandled` | pins the `k == Keys.Up` arm at line 333. | K2 |
| 58 | `DdOpen_KeyDownAsync_Down_LeavesEventUnhandled` | pins the `k == Keys.Down` arm at line 333. | K2 |
| 59 | `DdOpen_KeyDownAsync_Right_ShowsPopOutDialogWithAncestorControllerActions` | A: `new ItemViewer()` (headless), `Mock<IItemControler>` returning a known `Dictionary<string, Action>`, assigned to `viewer.Controller`; `viewer.Controls.Add(combo)`. Act: `DdOpen_KeyDownAsync(combo, Keys.Right)`. Assert: `prompt.Verify(p => p.ShowActionDialog("Pop Out Item or Enumerate Conversation?", "Dialog", BoxIcon.Question, sameDictionary), Times.Once())`, `SuppressKeyPress` and `Handled` true. Pins lines 343–356. | **K1**, K2 |
| 60 | `DdOpen_KeyDownAsync_Left_ClosesDropDownThroughDispatcher` | Assert `dispatcher.Verify(d => d.Invoke(It.IsAny<Action>()), Times.Once())` **without executing the action**, plus `SuppressKeyPress`/`Handled` true. Pins lines 358–366. | **K2** |
| 61 | `DdOpen_KeyDownAsync_Return_ClosesDropDownThroughDispatcher` | pins the `k == Keys.Return` arm at line 367. | K2 |
| 62 | `DdOpen_KeyDownAsync_Escape_ClosesDropDownThroughDispatcher` | pins the `k == Keys.Escape` arm at line 367. | K2 |
| 63 | `DdOpen_KeyDownAsync_UnrecognizedKey_FallsThroughToKeyDownTask` | A: register an `AlwaysOnKeyActionsAsync` for `Keys.F5`. Act: `DdOpen_KeyDownAsync(combo, Keys.F5)`. Assert: the always-on action ran. Pins the `default` arm at lines 382–387. | K2 |
| 64 | `DdOpen_KeyDownAsync_WithNullAmbientContext_InstallsWindowsFormsSyncContext` | A: `SyncContextScope` nulls `Current`. Assert: `SynchronizationContext.Current.Should().BeOfType<WindowsFormsSynchronizationContext>()`. Pins lines 319–322. | K5 |
| 65 | `DdClosed_KeyDownAsync_Right_OpensDropDownThroughDispatcherAsync` | Assert `InvokeAsync(It.IsAny<Action>())` once, `SuppressKeyPress`/`Handled` true. Pins lines 399–405. | **K2** |
| 66 | `DdClosed_KeyDownAsync_UnrecognizedKey_FallsThroughToKeyDownTask` | pins the `default` arm at lines 406–410. | K2 |
| 67 | `DdClosed_KeyDownAsync_WithNullAmbientContext_InstallsWindowsFormsSyncContext` | pins lines 393–396. | K5 |

### File I — `KeyboardHandler.BreadcrumbFallThroughTests.cs`

| # | Test method | A/A/A sketch | Seam |
| --- | --- | --- | --- |
| 68 | `BreadcrumbArrowFallThrough_NullViewer_ThrowsArgumentNullExceptionNamingViewer` | Assert `Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("viewer")`. Pins lines 297–300. Pure; no viewer needed. | none |
| 69 | `BreadcrumbArrowFallThrough_Right_ShowsPopOutDialogWithControllerRightKeyActions` | A: headless `new ItemViewer()`, `Mock<IItemControler>` supplying the dictionary. Assert the prompt received the exact dictionary instance and the exact message/title/icon. Pins lines 302–310. | **K1** |
| 70 | `BreadcrumbArrowFallThrough_Left_SetsFolderDroppedDownFalseWithoutDialog` | A: headless `new ItemViewer()` with **no** breadcrumb pipeline (`SetBreadcrumbDropDownState` is inert per `ItemViewer.Breadcrumb.cs:225–231`). Assert: `Should().NotThrow()` and `prompt.VerifyNoOtherCalls()`. Pins lines 311–314. | K1 (negative verify) |

### File J — `KeyboardHandler.GetItemViewerTests.cs`

| # | Test method | A/A/A sketch | Seam |
| --- | --- | --- | --- |
| 71 | `GetItemViewer_ControlIsItemViewer_ReturnsSameInstance` | A: headless `ItemViewer`. Assert `BeSameAs`. Pins lines 249–252. | none |
| 72 | `GetItemViewer_NestedChild_WalksParentChainToItemViewer` | A: `itemViewer.Controls.Add(panel); panel.Controls.Add(label)`. Act: `GetItemViewer(label)`. Assert `BeSameAs(itemViewer)`. Pins the recursion at lines 253–256. | none |
| 73 | `GetItemViewer_NoItemViewerAncestor_ReturnsNull` | A: orphan `new Panel()`. Assert `BeNull()`. Pins lines 257–260. | none |

**Total: 73 discrete test cases** across 10 test files plus 1 shared support file. Every file stays well under the 500-line ceiling at this distribution (the largest, File H at 14 cases, projects to ~330 lines).

Coverage projection: 73 cases reach every member except R1 (lines 35–39). Even with R1 uncovered and no partial credit anywhere else, static line accounting puts the file above 95%, comfortably clearing the 80% floor. **The numeric figure will be confirmed with F1's harness and committed under `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/evidence/qa-gates/`.**

---

## 10. Risks and Open Questions

| # | Risk / question | Assessment | Proposed handling |
| --- | --- | --- | --- |
| R-1 | **F1's ledger may classify `KeyboardHandler.cs` as `ratified-exempt`**, contradicting this research. | Low. Epic Shared Design §1 pre-commits to "refactor first, exempt only the irreducible remainder", and §5 of this document demonstrates concrete seams. | F1's ledger is authoritative. If it exempts the whole file, escalate with this artifact as the counter-evidence before accepting. |
| R-2 | **`new EfcViewer()` may fail headlessly** (R1). | Medium. `ItemViewer` precedent is strong but `EfcViewer` derives from `Form`, not `UserControl`. | One exploratory plan task; on failure, request a 5-line ledger entry (§7). Do not block the rest of the child on it. |
| R-3 | **`WindowsFormsSynchronizationContext` leakage across tests.** Cases 64 and 67 deliberately install one. MSTest runs `ClassLevel` parallel per `TaskMaster.runsettings:4–7`, so a leaked ambient context could contaminate a sibling class on the same thread. | Medium. | Mandate the `SyncContextScope` `IDisposable` in every test that touches `SynchronizationContext`, restoring in `Dispose`. Same discipline as `BreadcrumbPendingOpenCloseTests.cs:368–372`. |
| R-4 | **`QfcFormViewerExpanded.cs` and `QfcFormViewerDark.cs` are not assigned to any child** in epic.md's Feature File Assignments, yet both consume `IQfcKeyboardHandler` (lines 29, 36 in each) and `QfcFormKeyHandler` (line 43 in each). `EfcViewer3.cs` is likewise unassigned. | Low for F3 (no edit needed), but it is a gap in the epic's "every one of the 121 compiled files is assigned to exactly one child" claim. | Report to the epic orchestrator / F16 capstone. Verify against `QuickFiler.csproj` whether these three are `<Compile Include>`d; if so, the assignment table needs a correction. Out of scope for F3 to fix. |
| R-5 | **`ClearFilter()` (line 81), `KeyboardHandler_PreviewKeyDown` (96–102), and `GetItemViewer` (247–261) have no callers anywhere.** Testing dead code inflates coverage without protecting behavior. | Low. | Cover them (they are cheap and the interface/`internal` surface keeps them reachable), and open a **separate** follow-up issue proposing their removal. Do **not** delete them in this child — deletion is a public-surface change and `KeyboardHandler_KeyDown` is still an interface member. |
| R-6 | **Line 189 is unreachable** (§4). An agent chasing 100% could contrive a fake `KbdActions<>` subclass to hit it. | Low. | Record as an unreachable-branch note in the F1 ledger. The 80% floor does not require it. |
| R-7 | **Unused `using` directives** at lines 12 (`System.Web.UI.WebControls`), 14 (`System.Windows.Input`), and 15 (`Microsoft.Office.Interop.Outlook`). Line 15 in particular makes the file *appear* to be Outlook-Interop-bound, which is likely how it acquired `[ExcludeFromCodeCoverage]` in the first place. **No member in the file references any Outlook Interop type.** | Low risk, high signal. | Remove all three in this child; note the finding in `spec.md` as the evidence that the exemption was never warranted. Verify with the analyzer build (IDE0005 is configured through `.editorconfig`). |
| R-8 | **Behavior-change temptation.** Several defensive improvements suggest themselves (null guards on constructors, `ConfigureAwait(false)`, converting `async void` to `async Task`). | Medium — an executor could take them. | Issue #430 AC explicitly forbids behavior change. The plan must state that constructors gain **no** guards, `async void` signatures are **unchanged** (they are `KeyEventHandler`-shaped and interface-declared), and no `ConfigureAwait` is added. Promote each as its own follow-up issue per the repo's latent-defect promotion practice. |
| R-9 | **`MyBoxDialogPrompt` is a new production file** and General Unit Test Policy requires new modules to reach >= 90%. | Medium. | Resolved by §7 R2: request ledger ratification, or fall back to the in-file `private static readonly Func<...>` default. The planner must pick one before Phase 1 and record the choice in `spec.md`. |
| R-10 | **Rebase collisions.** `QfcHomeController.cs` (F7 / in-flight #424) and `QfcItemController.ViewerSetup.cs` (F10 / in-flight #400) both reference this type. | Low — F3 edits neither. | None required; the additive determination in §6 is what keeps the merge clean. |

---

## 11. Sources

All paths relative to `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aafcc2531072ca96b\`.

**Policy**
- `CLAUDE.md` — § UT2 (COM/VSTO/WinForms coverage exemption, "testable denominator", `KbdActions<>` named as NOT exempt), § CUT1–CUT3 (MSTest/Moq/FluentAssertions, toolchain order)
- `.claude/rules/general-unit-test.md` — § Coverage Exclusion Policy, § Test File Location, § Determinism Infrastructure
- `.claude/rules/csharp.md:47–63` — DI seam hierarchy (interface > delegate > adapter), TimeProvider guidance; `:89–96` Prohibited Behaviors
- `.claude/rules/general-code-change.md` — § File Size Limit (500), § Design Principles

**Feature / epic**
- `docs/features/epics/quickfiler-per-file-coverage/epic.md` — Shared Design §§1–6 (lines 132–192), F3 assignment (lines 267–275), Known Conflict Risks (lines 405–418)
- `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/issue.md` — full

**Production under research**
- `QuickFiler/Controllers/KeyboardHandler.cs:1–414` (read in full)
- `QuickFiler/QuickFiler.csproj:321, 339, 366`

**Production — dependencies and contracts**
- `QuickFiler/Interfaces/IQfcKeyboardHandler.cs:1–37`
- `QuickFiler/Interfaces/IFilerHomeController.cs:11, 23, 30–33`
- `QuickFiler/Interfaces/IFilerFormController.cs:9–24`
- `QuickFiler/Interfaces/IQfcFormViewer.cs:12, 21`
- `QuickFiler/Interfaces/IItemControler.cs:13`
- `QuickFiler/Interfaces/IQfcItemController.cs:97`
- `QuickFiler/Interfaces/MailItemActionsAdapter.cs:1–47` (seam-pattern precedent)
- `QuickFiler/Controllers/KbdActions.cs:14, 36–51, 90–121, 141–144`
- `QuickFiler/Controllers/KaKey.cs:11–98`
- `QuickFiler/Controllers/KaStringAsync.cs:10–94` (side-effecting `KeyEquals` at 57–79)
- `QuickFiler/Controllers/QfcHomeController.cs:18, 184–189, 421–422`
- `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:141–147, 201–207`
- `QuickFiler/Controllers/EfcHomeControllerDependencies.cs:51, 175, 187–190`
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:160–189`
- `QuickFiler/Controllers/QfcItemController.EventWiring.cs:41, 82`
- `QuickFiler/Controllers/QfcItemController.Navigation.cs:29, 42, 53, 60, 67, 76`
- `QuickFiler/Controllers/QfcItemController.Initialization.cs:38` (optional-seam-parameter precedent)
- `QuickFiler/Viewers/EfcViewer.cs:20–30, 55–64, 100`
- `QuickFiler/Viewers/IItemViewer.cs:15–17, 90`
- `QuickFiler/Viewers/ItemViewer.FolderSearch.cs:31–32`
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:223–235`
- `QuickFiler/Properties/AssemblyInfo.cs:5`
- `UtilitiesCS/Dialogs/MyBox.cs:16–22, 24–45, 47–76, 141–151`
- `UtilitiesCS/Properties/AssemblyInfo.cs:18–20`
- `UtilitiesCS/Threading/UiThread.cs:135–140`
- `UtilitiesCS/Threading/IUiDispatcher.cs:15–42`
- `UtilitiesCS/Threading/WpfUiDispatcher.cs:11`
- `UtilitiesCS/Extensions/WinFormsExtensions.cs:176–201`

**Test-side precedent**
- `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs:353–378` (headless `ItemViewer`, `SyncContextScope`, `InlineSynchronizationContext`)
- `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs:469–487`
- `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:155–185, 365–383`
- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:102, 225–257`
- `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:332, 348`
- `QuickFiler.Test/Controllers/MailItemActionsAdapterTests.cs` (existence; adapter-coverage precedent)
- `TaskMaster.runsettings:1–30` (ClassLevel parallelization, coverage collector config)
- `QuickFiler.Test/SetupAssemblyInitializer.cs:14–20`
- Directory enumeration of `QuickFiler.Test/**/*.cs` (107 files) — **no `KeyboardHandler*Tests.cs`, no `*.StaTests.cs`**

**Tooling**
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (existence confirmed; F1's harness input)
