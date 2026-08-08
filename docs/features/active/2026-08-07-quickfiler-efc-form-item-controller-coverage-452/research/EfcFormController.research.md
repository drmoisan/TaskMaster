# Research: `QuickFiler/Controllers/EfcFormController.cs` (F9 / issue #452, epic #136)

- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a721e5b2426cc0b97`
- Target file: `QuickFiler/Controllers/EfcFormController.cs` — **1,086 lines**, `internal class EfcFormController : IFilerFormController`, `[ExcludeFromCodeCoverage]` at line 27 (verified).
- Research date: 2026-08-07.
- Scope of this artifact: this one production file. `EfcItemController.cs`, `EfcViewer.cs`, and `EfcViewer.Designer.cs` are covered by sibling artifacts, except where this file's seam plan changes their contracts (called out explicitly).

---

## 0. Verified baseline and constraint corrections

### 0.1 The file is UNMEASURED, not uncovered — confirmed

`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
contains **no** `<class ... filename="QuickFiler\Controllers\EfcFormController.cs">` element. The nearest matches
are `QuickFiler.EfcHomeController` (line 9, 96.85% line / 89.06% branch) and method signatures that merely
*mention* `EfcFormController` as a parameter type (lines 1225, 1641-1774). The `[ExcludeFromCodeCoverage]`
attribute removes the type from instrumentation entirely. The brief's premise is correct: F9 starts this file
from an unknown, effectively zero, baseline.

### 0.2 Existing tests for this type

Exactly one file: `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` (55 lines, 1 test method).

- `EfcFormControllerTests.cs:18-28` — `CreateMinimalController()` obtains the **private no-arg constructor**
  (`EfcFormController.cs:79`) by reflection and invokes it, producing an all-null-field instance.
  **This is a proven, already-merged technique in this repository and F9 must build on it rather than reinvent it.**
- `EfcFormControllerTests.cs:34-53` — the single test,
  `PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel`, is a regression test for
  issue #145 pinning the `_formViewer == null` early return at `EfcFormController.cs:1029-1031`.

**F9 must preserve this test verbatim.** It is part of the spec (`CLAUDE.md` §7.3) and it pins a real
post-await null race.

`QuickFiler.Test/Controllers/EfcHomeController*Tests.cs` (5 files) reference `EfcFormController` only as a
factory *return type* through the F8 delegate seams; none exercises a member of this type.

### 0.3 Issue #450 — does NOT concern this type

`gh` is unavailable in this session; issue #450 was read from `https://github.com/drmoisan/TaskMaster/issues/450`.

> **Title:** `Refactor: quickfiler-formcontroller-tests-file-size-split` #450
> **Subject:** `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` — 827 lines, 42 test methods, breaching
> the 500-line limit.

Verified locally: `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` exists and is registered at
`QuickFiler.Test.csproj:117`. **#450 is about the QFC form-controller tests (F6 territory), not the EFC ones.**
F9's own new test files must nonetheless each stay under 500 lines, which is why the test inventory in §6 is
partitioned across seven test files.

### 0.4 Corrections to the epic manifest / brief

| Claim | Verdict | Evidence |
| --- | --- | --- |
| `EfcFormController.cs` is 1,086 lines | **Correct** | file ends at line 1086 |
| `[ExcludeFromCodeCoverage]` at line 27 | **Correct** | `EfcFormController.cs:27` |
| Epic §Scope: "21 compiled files carry a real attribute" | **Correct**, but the epic contradicts itself | Epic lines 224 and 324 both still say "the 33 existing `[ExcludeFromCodeCoverage]` attributes" after the marker-accuracy note corrected 33 → 21. F1's ledger must use 21; the two stale "33" sentences should be fixed when the epic is next edited. |
| Brief: "`EfcViewerQueue.Dequeue()` … consumed as a method group" | **Correct but not applicable to this file.** `EfcFormController.cs` never references `EfcViewerQueue`. The method-group consumption is at `EfcHomeControllerDependencyFactories.cs:40` and `:112` (F8). No F9 action; recorded in §7.3 as an inherited cross-child note only. |
| Brief: "This controller drives `BreadcrumbBridgeRouter` (owned by sibling F12)" | **Correct.** `BreadcrumbBridgeRouter.cs` is listed under F12 in the epic (line 409). Note it physically lives in `QuickFiler/Controllers/`, not `QuickFiler/Viewers/`, and is registered at `QuickFiler.csproj:292`. |
| Epic F9 line counts (`EfcViewer.cs` 162) | **Correct** | `EfcViewer.cs` is 162 lines |
| Implied assumption that pooled `EfcViewer` reuse causes handler accumulation | **FALSE — do not plan around it.** `ViewerQueueCore.Dequeue` (`ViewerQueueCore.cs:63-85`) is consume-once: it dequeues an instance and refills the queue with **new** instances from `_viewerFactory()`. `EfcViewerQueue.CreateProductionViewer` (`EfcViewerQueue.cs:81-84`) is `new EfcViewer()`. Every `EfcHomeController` session therefore receives a fresh `EfcViewer`. The "pooled-viewer re-initialization" language in `WebView2BreadcrumbHost.cs:18-19,88-89` is defensive, not descriptive. |

---

## 1. Structural map

### 1.1 File-level

| Lines | Content |
| --- | --- |
| 1-23 | 23 `using` directives (incl. `Microsoft.Office.Interop.Outlook` at 14, `System.Windows.Forms` at 13) |
| 25 | `namespace QuickFiler.Controllers` |
| 27 | `[ExcludeFromCodeCoverage]` — **the attribute F9 must remove** |
| 28 | `internal class EfcFormController : IFilerFormController` |
| 1085-1086 | closing braces |

Unused usings observed: `System.Diagnostics` (4), `System.Drawing.Drawing2D` (7), `System.IO` (8), `System.Text` (10), `TaskVisualization` (19) is used only by `ManageFilters` at 563, `ToDoModel` (20) has no visible use. Removing them is safe formatting hygiene during the split.

### 1.2 Member inventory

Legend for **Kind**: `PURE` = no host dependency; `WF` = WinForms-bound; `COM` = Outlook Interop-bound; `EVT` = event handler; `ORCH` = orchestration only.

#### `#region Constructors` (30-121)

| Lines | Member | Kind | Notes |
| --- | --- | --- | --- |
| 32-51 | `.ctor(IApplicationGlobals, EfcDataModel, EfcViewer, EfcHomeController, Action, QfEnums.InitTypeEnum, CancellationToken)` | WF | Reads `_formViewer.ItemViewer` (49) and `_formViewer.L0vh_TLP` (50) — Designer fields. Pinned by F8 delegate `FormControllerWithDataFactoryDelegate` (`EfcHomeControllerDependencies.cs:15-23`). |
| 53-77 | `.ctor(IApplicationGlobals, EfcViewer, EfcHomeController, Action, QfEnums.InitTypeEnum, CancellationToken)` | WF + COM | Same Designer reads plus `new EfcItemController(...)` (69-75). Pinned by F8 delegate `FormControllerWithoutDataFactoryDelegate` (`:25-32`). |
| 79 | `private EfcFormController()` | PURE | Reflection-reachable; already used by the existing test. |
| 81-99 | `Initialize()` | ORCH | Calls 6 setup members + `new EfcItemController(... _dataModel ...)` (87-94) + fire-and-forget `_ = PopulateFolderCombobox()` (97). Bound by `EfcHomeControllerDependencyFactories.cs:80`. |
| 101-111 | `InitializeWithoutData()` | ORCH | Same minus data; `_itemController.InitializeWithoutData()` (107). Bound by `EfcHomeControllerDependencyFactories.cs:92`. |
| 113-119 | `InitializeDataFields(EfcDataModel)` | ORCH | Bound by `EfcHomeControllerDependencyFactories.cs:105`. Signature is an F8 contract — **must not change**. |

#### `#region Private Properties` (123-162)

Fields only. `logger` (125-127); `_globals`, `_parentCleanup`, `_dataModel`, `_formViewer`, `_folderRows` (136),
`_breadcrumbHost` (140), `_router` (141), `_homeController`, `_itemController`, `_itemViewer`, `_initType`,
`_listTipsDetails` (150, `IList<IQfcTipsDetails>` — **already an interface, mockable**), `_itemTlp` (151),
`_itemViewerTlpRow`, `_tlpHeightExpanded`, `_tlpHeightCollapsed`, `_tlpHeightDiff`, `_themes` (156),
`_listButtons`, `_listDefault`, `_listCheckBox`, `_listHighlighted`.

#### `#region Setup and Cleanup Methods` (164-250)

| Lines | Member | Kind | Notes |
| --- | --- | --- | --- |
| 166-187 | `CaptureConfigureItemViewer()` | COM + WF | `_globals.Ol.GetExplorerScreenSize()` (168, `IOlObjects.cs:36`); `_itemTlp.RowStyles[1].Height` (169); `_itemViewer.Height`/`.MinimumSize.Height` (170-171); `_itemTlp.GetPositionFromControl(_itemViewer).Row` (173); `_itemViewer.L0vh_Tlp.RowStyles` (175); mutates `bodyRow.Height` (181) and form `MinimumSize`/`Size` (182-186). **Contains 7 lines of pure arithmetic that are extractable.** |
| 189-196 | `Cleanup()` | COM | `_globals.Ol.PropertyChanged -= DarkMode_Changed` (191) then nulls 3 fields and invokes `_parentCleanup` (195). |
| 198-206 | `ConfigureFind()` | WF | Pure `HasFlag` branch (200) writing three `.Text` properties. |
| 208-235 | `ResolveControlGroups()` | WF | `_formViewer.TipsLabels` projection into `QfcTipsDetails` (210-213); `GetAllChildren(except:)` extension (215, `WinFormsExtensions.cs:160`); type-partitioning into 4 control lists (217-234). |
| 237-248 | `SetupThemes()` | WF | `EfcThemeHelper.SetupFormThemes(...)` (`EfcThemeHelper.cs:249-255`, F4-owned, `public static`, `IList<Control>` params) then `_activeTheme = LoadTheme()`. |

#### `#region Public Properties` (252-352)

| Lines | Member | Kind | Notes |
| --- | --- | --- | --- |
| 254-264 | `ActiveTheme` get/set | PURE-ish | Getter binds `Initializer.GetOrLoad<string>(ref, Func<string>, bool strict, params object[])` (`Initializer.cs:124-139`) with `strict: true` and `_themes` as the sole dependency. **When `_themes` is null this THROWS `ArgumentNullException`** (`Initializer.cs:310-321`). Setter routes `_themes[x].SetTheme(async: true)` — `KeyNotFoundException` on an unknown key. |
| 266-271 | `LoadTheme()` | PURE-ish | `_themes[activeTheme].SetTheme()` — NRE when `_themes` is null. |
| 273-285 | `DarkMode` get/set | COM | Getter binds `Initializer.GetOrLoad<bool>(ref, Func<bool>, bool strict:false, params object[]{_globals, _globals.Ol})`. **`_globals.Ol` is evaluated eagerly as an array element, so the getter NREs when `_globals` is null.** Setter writes `_globals.Ol.DarkMode`. |
| 287 | `FormHandle` | WF | `_formViewer.Handle`. |
| 289-295 | `SelectedFolder` | PURE | `_router?.SelectedFolderPath` — null-safe delegation to the F12 router. |
| 297-343 | `SaveAttachments`, `SaveEmail`, `SavePictures`, `MoveConversation` | PURE | Plain backing-field auto-property equivalents with dead commented lines. |
| 345-350 | `Token` get/set | PURE | |

#### `#region Event Handlers` (354-698)

| Lines | Member | Kind | Notes |
| --- | --- | --- | --- |
| 356-368 | `RegisterAlwaysOnAsyncKeyActions()` | WF | Writes `_formViewer.KeyboardHandler.AlwaysOnKeyActionsAsync`. **Note the asymmetry: this is the only place that reads `_formViewer.KeyboardHandler`; every other keyboard access uses `_homeController.KeyboardHandler`.** |
| 370-402 | `WireEventHandlers()` | WF + COM | `ForAllControls` bulk key wiring (375-386); 13 discrete `+=` subscriptions; `ConfigureBreadcrumbControl()` (393); `_globals.Ol.PropertyChanged += DarkMode_Changed` (401). |
| 404-413 | `SearchText_DownArrow(object, KeyEventArgs)` | EVT + WF | Branch on `e.KeyCode == Keys.Down` (406); `_formViewer.FolderListBox.Select()` (410); `_router?.SelectFirstRow()` (411). |
| 415-429 | `ButtonCancel_Click` | EVT `async void` | SyncContext bootstrap (419-420); `await ActionCancelAsync()`; catch → `logger.Error` + **rethrow** (424-428). |
| 431-445 | `ButtonOK_Click` | EVT `async void` | Same shape over `ActionOkAsync`. |
| 447-461 | `ButtonRefresh_Click` | EVT `async void` | Same shape over `RefreshSuggestionsAsync`. |
| 463-521 | `ButtonCreate_Click` | EVT `async void` + COM | 3-way branch: `MessageBox.Show` (472-474); Find path (476-482); OneDrive lookup + `CreateFolderAsync` cast to `MAPIFolder` (490-498) + `MoveToFolderAsync` (502-509). |
| 523-534 | `ButtonDelete_Click` | EVT `async void` | **Does not set the synchronization context**, unlike its 4 siblings. |
| 536-554 | `SaveAttachments_CheckedChanged`, `SaveEmail_CheckedChanged`, `SavePictures_CheckedChanged`, `MoveConversation_CheckedChanged` | EVT + WF | Each is a one-line copy from a `ToolStripMenuItem.Checked`. |
| 556-559 | `SearchText_TextChanged` | EVT + WF | `BindFolderRows(_dataModel.FindMatches(_formViewer.SearchText.Text))`. |
| 561-566 | `EditFiltersMenuItem_Click` | EVT + WF | Constructs and `Show()`s a live `TaskVisualization.ManageFilters` form (`ManageFilters.cs:17`). **A unit test must never reach the un-seamed version of this.** |
| 568-570 | `_characterAsyncActions` / `CharacterAsyncActions` | PURE | `Initializer.GetOrLoad(ref, Func<T>)` (`Initializer.cs:103-110`). |
| 572-603 | `GetAsyncCharacterActions()` | PURE construction | Builds 7 `KaCharAsync` entries; the captured lambdas reference `_formViewer.SearchText`/`.FolderListBox`/`.MoveOptionsMenu` but are not invoked at construction. |
| 605-623 | commented-out `GetKbdActions` block | DEAD | 19 lines. Delete during the split. |
| 625-627 | `_characterActions` / `CharacterActions` | PURE | |
| 629-677 | `GetKbdActions()` | PURE construction | Builds 8 `KaChar` entries. |
| 679-696 | `DarkMode_Changed(object, PropertyChangedEventArgs)` | EVT + COM | `nameof(_globals.Ol.DarkMode)` guard (681); reads `_globals.Ol.DarkMode` (683); sets `ActiveTheme` (686/690); `_router?.ApplyTheme(DarkMode)` (694). |

#### `#region Major Actions` (700-808)

| Lines | Member | Kind | Notes |
| --- | --- | --- | --- |
| 702-731 | `ActionOkAsync()` | ORCH + WF | Banner guard `StartsWith("====")` (708) + `MessageBox.Show` (710); `_formViewer.Hide()` (715); 3-way `initType` branch (716-727) including `throw new NotImplementedException()` (726); `_formViewer.Dispose()` + `Cleanup()` (728-729). |
| 733-740 | `ActionCancelAsync()` | ORCH + WF | `await _formViewer.UiSyncContext` (736); `Close()`; `Cleanup()`. |
| 742-750 | `ActionDeleteAsync()` | PURE-ish | `await UiSyncContext`; inserts `"Trash to Delete"` at index 0 of `_folderRows` (747-748); `BindFolderRows`. |
| 752-795 | `CreateFolderAsync()` | ORCH + COM | `IsValidSelection` guard + `MessageBox.Show` (754-757); Find branch (758-761); OneDrive lookup (766-769); synchronous `FolderHelper.CreateFolder` wrapped in `Task.FromResult` (770-777); `MoveToFolderAsync` (780-789). |
| 797-806 | `RefreshSuggestionsAsync()` | ORCH | Two `Task.Run` calls over `_dataModel` (799-803); **reads `_formViewer.SearchText.Text` from inside the second `Task.Run` lambda** (801). |

#### `#region Helper Methods` (810-1054)

| Lines | Member | Kind | Notes |
| --- | --- | --- | --- |
| 812-816 | `KbdExecuteAsync(Func<Task>)` | ORCH | `_homeController.KeyboardHandler.ToggleKeyboardDialogAsync()` then `action()`. `IQfcKeyboardHandler` is an interface — mockable. |
| 818-822 | `KbdExecuteAsync(Action)` | ORCH | Same. |
| 824-829 | `JumpToAsync(Control)` | WF | `control.Focus()`. |
| 834-854 | `ConfigureBreadcrumbControl()` | WF + COM | `new WebView2BreadcrumbHost(_formViewer.BreadcrumbWebView, new WebView2CoreInitializer())` (836-839); `new OutlookFolderHierarchyProvider(_globals.Ol.FolderTreeService)` (840-842); `new BreadcrumbBridgeRouter(provider, host, new BreadcrumbMessageCodec(), new BreadcrumbHtmlRenderer(), new BreadcrumbOutboundQueue(host))` (843-849); 2 event hookups (850-851); `_router.ApplyTheme(DarkMode)` (852); fire-and-forget `_ = InitializeBreadcrumbHostAsync()` (853). |
| 858-868 | `InitializeBreadcrumbHostAsync()` | WF | `await _breadcrumbHost.InitializeAsync(_formViewer.UiSyncContext)` inside a logged error boundary. |
| 873-883 | `BindFolderRows(string[])` | PURE | Local-capture null guard (875-879); `_folderRows = rows ?? Array.Empty<string>()` (881); fire-and-forget bind (882). |
| 886-903 | `BindBreadcrumbRowsAsync(string[])` | ORCH | `_dataModel?.FolderHelper?.Suggestions?.ToScoredArray() ?? Array.Empty<FolderScore>()` (890-892); `await _router.BindRowsAsync(rows, scores, Token)` (893); `OperationCanceledException` arm (895-898); general arm (899-902). |
| 905-913 | `MaximizeFormViewer()` / `MinimizeFormViewer()` | WF | `_formViewer.WindowState = ...`. Both are `IFilerFormController` members. |
| 915 | `ShowMenu(ToolStripMenuItem)` | WF | `menu.ShowDropDown()`. |
| 917-921 | `ToggleCheckboxAsync(CheckBox)` | WF | Currently unreferenced in this file (the 4 call sites are commented out at 583-586). |
| 923-930 | `ToggleOffNavigation(bool)` | ORCH | Removes `CharacterActions.Keys` from `_homeController.KeyboardHandler.CharActions`; `ToggleTips`; `_itemController.ToggleNavigation`. |
| 932-939 | `ToggleOffNavigationAsync()` | ORCH | Async twin. |
| 941-946 | `ToggleOnNavigation(bool)` | ORCH | |
| 948-955 | `ToggleOnNavigationAsync()` | ORCH | |
| 957-970 | `ToggleTips(bool)` | WF | `_formViewer.BeginInvoke` / `.Invoke` over `IQfcTipsDetails.Toggle(true)`. |
| 972-989 | `ToggleTips(bool, Enums.ToggleState)` | WF | Same shape with a desired state. |
| 991-1007 | `ToggleTipsAsync(Enums.ToggleState)` | PURE | `Token.ThrowIfCancellationRequested()` (993); `Task.WhenAll` over `IQfcTipsDetails.ToggleAsync` (996-1000). **Already fully testable with `Mock<IQfcTipsDetails>`.** |
| 1009-1022 | `LoadUserSettings()` | WF + global state | Reads the `QuickFiler.Properties.Settings.Default` static singleton (4 reads) and writes 4 menu-item `.Checked` values. |
| 1024-1038 | `PopulateFolderCombobox(object = null)` | ORCH | Local-capture guard (1029-1031, pinned by the existing test); `_dataModel.InitFolderHandlerAsync` (1033); `await formViewer.UiSyncContext` (1035); `BindFolderRows(_dataModel.FolderHelper.FolderArray)` (1037). |
| 1040-1052 | `IsValidSelection` | **PURE** | 4-clause guard on `SelectedFolder`. The single most trivially coverable member in the file. |

#### Outside all regions

| Lines | Member | Kind |
| --- | --- | --- |
| 1056-1084 | `ToggleExpansionStyle(Enums.ToggleState)` | WF — TLP row height + form `MinimumSize`/`Size` arithmetic + `WindowState`. Called by `EfcItemController.cs:864` and `:909`. |

### 1.3 Aggregate

- 1 type, 2 public constructors, 1 private constructor, 3 initializers.
- 14 public/internal properties, 45 methods, 19 lines of commented-out dead code (605-623).
- COM-bound member count: 8 (`CaptureConfigureItemViewer`, `Cleanup`, `DarkMode`, `WireEventHandlers`, `ButtonCreate_Click`, `DarkMode_Changed`, `CreateFolderAsync`, `ConfigureBreadcrumbControl`).
- WinForms-bound member count: 22.
- Genuinely pure member count: 9 (`IsValidSelection`, `SelectedFolder`, the four settings properties, `Token`, `BindFolderRows`, `ToggleTipsAsync`).

---

## 2. Candidate approaches

### 2.1 Approach A (rejected) — parameterless-constructor + reflection field injection only

Extend the existing `CreateMinimalController()` technique: build the controller via the private ctor and
reflect fields into place, without introducing production seams.

**Rejected.** It cannot reach any member that calls `MessageBox.Show` (472, 710, 756), constructs
`ManageFilters` (563), constructs `WebView2BreadcrumbHost` (836), or touches Designer control properties.
Those are ~55% of the file's statements. Reflection injection of a live `EfcViewer` is impossible without
constructing a `Form` — which the epic's Shared Design §2 forbids in unit tests. It would also leave the
`[ExcludeFromCodeCoverage]` attribute justified, which is the exact outcome the epic forbids.

### 2.2 Approach B (rejected) — an `IEfcDataModel` / `IEfcHomeController` interface pair with adapters

Extract narrow interfaces over `EfcDataModel` and `EfcHomeController` and wrap them in adapters.

**Rejected on two grounds.**
1. `EfcDataModel.cs` is **F5-owned** (epic line 359) and `EfcHomeController.cs` is **F8-owned** (epic line 380).
   Making them implement an F9 interface is a cross-child edit that breaks the epic's disjointness invariant.
2. An adapter over `EfcDataModel` is itself poorly coverable. `EfcDataModel` is `internal class` with **no
   virtual members**, so Moq cannot mock it; a test would have to construct a real one. That is possible
   headlessly (`EfcDataModel.cs:234-252` `TryGetFirstInSelection` catches every exception and returns null,
   so `new EfcDataModel(mockGlobals, null, cts, token)` succeeds with `Mail == null`), but
   `FolderHelper` stays null, so `FindMatches` (`:374-387`) NREs on `_folderHelper`. The adapter's forwarding
   lines would sit permanently uncovered — a new file below the 90% new-file floor.

### 2.3 Approach C — SELECTED — host-neutral controller behind `IEfcFormViewer`, plus in-family injectable-delegate seams

Three coordinated moves:

**C1. Interface seam for the viewer (tier 1 of `.claude/rules/csharp.md` §DI Seams).**
Introduce `QuickFiler/Interfaces/IEfcFormViewer.cs : UtilitiesCS.Interfaces.IWinForm.IForm` and change
`EfcViewer` to `public partial class EfcViewer : Form, IEfcFormViewer`. The controller's `_formViewer` field
becomes `IEfcFormViewer`.

This is **exactly the pattern already merged for the QFC twin**: `QfcFormViewer.cs:18`
(`public partial class QfcFormViewer : Form, IQfcFormViewer`), `IQfcFormViewer.cs:12`
(`public interface IQfcFormViewer : IForm`), `QfcFormController.cs:168` (`private IQfcFormViewer _formViewer`),
and the corresponding test harness `QfcFormControllerTests.cs:103` (`new Mock<IQfcFormViewer>()`).
`IQfcFormViewer` already demonstrates the "intent event replaces the raw `Button`" idiom
(`IQfcFormViewer.cs:36-40`: `OkClicked`, `CancelClicked`, `UndoClicked`, `SkipClicked`) and the
"intent snapshot replaces raw layout properties" idiom (`:31-34`: `CaptureTlpCellStates()`).

Verified that `IForm` transitively supplies everything the controller needs beyond the intent members:
`IControl : IComponent, IDropTarget, ISynchronizeInvoke, IWin32Window, IDisposable, IBindableComponent`
(`IControl.cs:9-15`) yields `Handle` (IWin32Window), `Dispose()` (IDisposable), `Invoke(Delegate)` /
`BeginInvoke(Delegate)` (`IControl.cs:156,176`); `IControl` itself declares `MinimumSize` (`:62`),
`Size` (`:73`), `Text` (`:77`), `Hide()` (`:169`), `Select()` (`:202`); `IForm` declares `WindowState`
(`IForm.cs:49`) and `Close()` (`:76`).

The residual WinForms wiring lands in `EfcViewer.cs`, which is a **`Form`-derived class** and therefore
legitimately `ratified-exempt` under `CLAUDE.md` §UT2 ground (b) — "WinForms form-derived classes and
Designer-generated code". This is precisely what `.claude/rules/general-unit-test.md` § Coverage Exclusion
Policy prescribes: *"extract all logic into host-neutral, testable modules and leave only the thinnest
possible wiring in the host-bound entry point."*

**C2. Injectable-delegate seams (tier 2) for the two unmockable concrete collaborators.**
`EfcDataModel` and `EfcHomeController` are reached through `internal` settable delegate properties that
default to `null` and fall through to the concrete call at the call site. This is the **already-merged
in-family idiom**: `EfcHomeController.ExecuteMoves.cs:86-109` does exactly this
(`return MoveToFolderAsyncAction is null ? _dataModel.MoveToFolderAsync(...) : MoveToFolderAsyncAction(...)`),
as do `EfcHomeController.cs:294-305` (`ViewerShowAction`, `ViewerShowAsyncAction`, `MessageBoxShowAction`)
and `EfcHomeController.ExecuteMoves.cs:22-29` (`MoveFailureMessageAction`, `MoveMetricsAction`).
Zero new files, zero edits to F5/F8 files, and each wrapper is a single-branch method that both arms of a
test can cover.

**C3. Pure-function extraction for the layout arithmetic.**
`CaptureConfigureItemViewer` (166-187) and `ToggleExpansionStyle` (1056-1084) contain host-neutral integer
arithmetic tangled with WinForms property writes. Extract the arithmetic into a new `static` class
`EfcFormLayoutMath` operating on primitives and a small readonly struct. That file is 100% testable and
satisfies the new-file 90% floor comfortably.

**Why C over B:** C introduces one interface over a file F9 already owns (`EfcViewer.cs`) and zero interfaces
over files it does not own. C's residual uncovered surface is concentrated in a file that has an
independent, ratifiable exemption ground. C's precedent is already merged and green in this repository.

---

## 3. Proposed 500-line partial split

Naming follows the two in-repo precedents: `QfcFormController.cs` / `.SetupDisposal.cs` /
`.EventHandlers.cs` / `.Actions.cs`, and `EfcHomeController.cs` / `.Metrics.cs` / `.ExecuteMoves.cs` /
`.Timing.cs`.

**Critical planning constraint:** per-file coverage is measured per Cobertura `filename`, and each partial
file emits its own `filename`. **Every partial below must independently clear 80% line / 75% branch** — the
split cannot be used to hide uncovered lines in one partial. All eight are designed to be fully testable;
none is proposed for the exemption ledger.

| # | New file | Members moved (source lines) | Projected lines |
| --- | --- | --- | --- |
| 1 | `EfcFormController.cs` | class decl + attribute removal; all fields (123-162) + new seam fields; both public ctors (32-77); private ctor (79); new `internal` test ctor overload; `Initialize` / `InitializeWithoutData` / `InitializeDataFields` (81-119) | ~200 |
| 2 | `EfcFormController.Properties.cs` | `ActiveTheme` (254-264), `LoadTheme` (266-271), `DarkMode` (273-285), `FormHandle` (287), `SelectedFolder` (289-295), the four settings properties (297-343), `Token` (345-350), `IsValidSelection` (1040-1052) | ~155 |
| 3 | `EfcFormController.Setup.cs` | `CaptureConfigureItemViewer` (166-187), `Cleanup` (189-196), `ConfigureFind` (198-206), `ResolveControlGroups` (208-235), `SetupThemes` (237-248), `LoadUserSettings` (1009-1022), `ToggleExpansionStyle` (1056-1084) | ~165 |
| 4 | `EfcFormController.EventHandlers.cs` | `RegisterAlwaysOnAsyncKeyActions` (356-368), `WireEventHandlers` (370-402), `SearchText_DownArrow` (404-413), the five `Button*_Click` (415-534), the four `*_CheckedChanged` (536-554), `SearchText_TextChanged` (556-559), `EditFiltersMenuItem_Click` (561-566), `DarkMode_Changed` (679-696) | ~265 |
| 5 | `EfcFormController.KeyboardActions.cs` | `CharacterAsyncActions` / `GetAsyncCharacterActions` (568-603), `CharacterActions` / `GetKbdActions` (625-677) **with the dead block 605-623 deleted**, `KbdExecuteAsync` ×2 (812-822), `JumpToAsync` (824-829), `ShowMenu` (915), `ToggleCheckboxAsync` (917-921), the four `ToggleOn/OffNavigation` members (923-955) | ~195 |
| 6 | `EfcFormController.Actions.cs` | `ActionOkAsync` (702-731), `ActionCancelAsync` (733-740), `ActionDeleteAsync` (742-750), `CreateFolderAsync` (752-795), `RefreshSuggestionsAsync` (797-806), `PopulateFolderCombobox` (1024-1038) | ~165 |
| 7 | `EfcFormController.Breadcrumb.cs` | `ConfigureBreadcrumbControl` (834-854), `InitializeBreadcrumbHostAsync` (858-868), `BindFolderRows` (873-883), `BindBreadcrumbRowsAsync` (886-903) + the breadcrumb factory-seam properties | ~135 |
| 8 | `EfcFormController.Tips.cs` | `ToggleTips(bool)` (957-970), `ToggleTips(bool, ToggleState)` (972-989), `ToggleTipsAsync` (991-1007), `MaximizeFormViewer` (905-908), `MinimizeFormViewer` (910-913) | ~100 |

Total ≈ 1,380 lines across 8 files (the growth over 1,086 is per-file `using`/namespace/class headers plus the
new seam declarations, less the 19 deleted dead lines). Largest file ≈ 265, comfortably under 500.

New non-partial files created by F9 for this target:

| New file | Purpose | Ledger bucket |
| --- | --- | --- |
| `QuickFiler/Interfaces/IEfcFormViewer.cs` | The viewer interface seam | **`interface-only / not-measured`** (epic §"A third ledger bucket") |
| `QuickFiler/Controllers/EfcFormLayoutMath.cs` | Pure layout arithmetic + `EfcItemViewerLayoutSnapshot` readonly struct | `testable`, target 100% |

**`QuickFiler.csproj` edits required:** 10 new `<Compile Include=...>` entries, appended adjacent to the
existing `Controllers\EfcFormController.cs` entry at line 294. Preserve CRLF; use the Edit tool, never
`sed -i` (epic §"Cross-Child Constraints" 1).

**`QuickFiler.Test.csproj` edits required:** the test project also uses explicit `<Compile Include>`
(verified `QuickFiler.Test.csproj:58-117`); each new test file must be registered.

> **Note on `net481` language level.** Per prior repository research, `net48`/`net481` here has no
> `IsExternalInit` polyfill: `init`-only setters, `record`, and `record struct` fail with CS0518. The
> `EfcItemViewerLayoutSnapshot` type must therefore be a plain `readonly struct` with a positional
> constructor, following the existing `ResourceTimingRow` precedent — not a record.

---

## 4. Concrete seam plan

Seam tier per `.claude/rules/csharp.md` §DI Seams: **interface seam > injectable delegate > adapter**.

### S1 — `IEfcFormViewer` interface seam (tier 1)

**New file** `QuickFiler/Interfaces/IEfcFormViewer.cs`:

```
public interface IEfcFormViewer : UtilitiesCS.Interfaces.IWinForm.IForm
```

Members, each justified by a specific call site:

| Member | Replaces | Call sites |
| --- | --- | --- |
| `SynchronizationContext UiSyncContext { get; }` | `EfcViewer.cs:37-40` | 420, 436, 452, 468, 705, 736, 744, 764, 790, 862, 1035 |
| `IQfcKeyboardHandler KeyboardHandler { get; }` | `EfcViewer.cs:56-59` | 358 |
| `IList<Label> TipsLabels { get; }` | `EfcViewer.cs:67-70` | 210, 229, 240 |
| `ItemViewer ItemViewer { get; }` | `EfcViewer.Designer.cs:4262` | 49, 68 |
| `TableLayoutPanel ItemTableLayout { get; }` | `EfcViewer.Designer.cs:4261` (`L0vh_TLP`) | 50, 76 |
| `Control SearchTextControl { get; }` | `EfcViewer.Designer.cs:4246` | 223, 577, 637 |
| `Control FolderListControl { get; }` | `EfcViewer.Designer.cs:4250` | 224, 410, 581, 642 |
| `string SearchTextValue { get; }` | `SearchText.Text` | 558, 801 |
| `event EventHandler SearchTextChanged` | `SearchText.TextChanged` | 398 |
| `event KeyEventHandler SearchTextKeyDown` | `SearchText.KeyDown` | 399 |
| `void FocusFolderList()` | `FolderListBox.Select()` | 410 |
| `string OkButtonText { set; }`, `string NewFolderButtonText { set; }` | `Ok.Text`, `NewFolder.Text` | 203, 204 |
| `event EventHandler OkClicked`, `CancelClicked`, `RefreshClicked`, `NewFolderClicked`, `DeleteClicked` | `Ok/Cancel/RefreshPredicted/NewFolder/BtnDelItem .Click` | 391, 394, 395, 396, 397 |
| `bool SaveAttachmentsChecked { get; set; }` + `event EventHandler SaveAttachmentsChanged` (and 3 identical triples for `SaveEmail`, `SavePictures`, `MoveConversation`) | `*MenuItem.Checked` / `.CheckedChanged` | 387-390, 538, 543, 548, 553, 1012, 1015, 1018, 1021 |
| `event EventHandler EditFiltersClicked` | `EditFiltersMenuItem.Click` | 400 |
| `void ShowMoveOptionsMenu()` | `MoveOptionsMenu.ShowDropDown()` | 599, 673, 915 |
| `void WireKeyHandlers(PreviewKeyDownEventHandler, KeyEventHandler)` | `ForAllControls(...)` (375-386) | 375 |
| `IReadOnlyList<Control> GetChildControlsExcept(IList<Control> except)` | `GetAllChildren(except:)` extension (`WinFormsExtensions.cs:160`) | 215 |
| `EfcItemViewerLayoutSnapshot CaptureItemViewerLayout()` | 169-176 | 166-187 |
| `void ApplyItemViewerLayout(float bodyRowHeight)` | 181 | 181 |
| `void SetItemViewerRowHeight(int rowIndex, float height)` | 1060, 1074 | 1056-1084 |
| `void SetMinimumAndSize(Size minimum, Size size)` | 182-186, 1061-1068, 1075-1082 | |
| `Microsoft.Web.WebView2.WinForms.WebView2 BreadcrumbWebView { get; }` | `EfcViewer.cs:92` | 837 (consumed only by the S4 factory) |

`IntPtr Handle`, `void Dispose()`, `void Close()`, `void Hide()`, `Size MinimumSize`, `Size Size`,
`string Text`, `FormWindowState WindowState`, `object Invoke(Delegate)`, `IAsyncResult BeginInvoke(Delegate)`
all come free from `IForm`/`IControl` — **do not redeclare them.**

**Production changes:**
- `QuickFiler/Viewers/EfcViewer.cs` (F9-owned): `public partial class EfcViewer : Form, IEfcFormViewer`;
  add the intent members as 1:1 forwards to the Designer fields. Keeps its `[ExcludeFromCodeCoverage]`
  (line 20) under `CLAUDE.md` §UT2 ground (b). Projected 162 → ~330 lines, still under 500.
- `EfcFormController.cs`: `private EfcViewer _formViewer;` → `private IEfcFormViewer _formViewer;`.
  **Both public constructor signatures keep the concrete `EfcViewer` parameter** (implicit upcast at
  assignment) so `EfcHomeControllerDependencies.FormControllerWithDataFactoryDelegate` /
  `FormControllerWithoutDataFactoryDelegate` (`EfcHomeControllerDependencies.cs:15-32`) are untouched.
- Add two `internal` constructor overloads taking `IEfcFormViewer` for tests:

```
internal EfcFormController(IApplicationGlobals globals, EfcDataModel dataModel,
    IEfcFormViewer formViewer, EfcHomeController homeController, System.Action parentCleanup,
    QfEnums.InitTypeEnum initType, CancellationToken token)
```

  plus an `internal` seam-only constructor that takes no `EfcHomeController` at all (see S2).

**No edits to `EfcHomeControllerDependencies.cs` or `EfcHomeControllerDependencyFactories.cs`.** Verified
that neither file references any member of `EfcFormController` other than the two constructors,
`Initialize()` (`EfcHomeControllerDependencyFactories.cs:80`), `InitializeWithoutData()` (`:92`), and
`InitializeDataFields(EfcDataModel)` (`:105`). All four survive unchanged.

### S2 — Injectable-delegate seams for `EfcHomeController` (tier 2)

`EfcHomeController` is `public partial class` with no virtual members; Moq cannot mock it. `IFilerHomeController`
(`IFilerHomeController.cs:11-44`) does **not** declare `ExecuteMovesAsync`, `OpenOlFolderAsync`, or
`OpenFsFolderAsync` — those are `internal`/`public` members on the concrete type
(`EfcHomeController.ExecuteMoves.cs:31`, `EfcHomeController.cs:423`, `:428`). Widening
`IFilerHomeController` is an F6/F7 file edit and is out of scope.

Add to `EfcFormController` (all `internal`, all defaulting to `null`, all consumed via the in-family
`X is null ? concrete : X` idiom):

| Seam property | Default fall-through | Call sites replaced |
| --- | --- | --- |
| `internal Func<Task> ExecuteMovesAction { get; set; }` | `_homeController.ExecuteMovesAsync()` | 718 |
| `internal Func<string, Task> OpenOlFolderAction { get; set; }` | `_homeController.OpenOlFolderAsync(p)` | 722 |
| `internal Func<string, Task> OpenFsFolderAction { get; set; }` | `_homeController.OpenFsFolderAsync(p)` | 478, 760 |
| `internal IQfcKeyboardHandler KeyboardHandlerOverride { get; set; }` | `_homeController.KeyboardHandler` | 379, 382, 814, 820, 826, 926, 935, 943, 951 |

`IQfcKeyboardHandler` is already an interface (`IQfcKeyboardHandler.cs:9`) so the override needs no adapter.

### S3 — Injectable-delegate seams for `EfcDataModel` (tier 2)

`EfcDataModel` is `internal class` with no virtual members and is **F5-owned** — must not be edited.

| Seam property | Default fall-through | Call sites replaced |
| --- | --- | --- |
| `internal Func<string, string[]> FindMatchesAction { get; set; }` | `_dataModel.FindMatches(t)` | 558, 801 |
| `internal Action RefreshSuggestionsAction { get; set; }` | `_dataModel.RefreshSuggestions()` | 799 |
| `internal Func<object, Task> InitFolderHandlerAction { get; set; }` | `_dataModel.InitFolderHandlerAsync(l)` | 1033 |
| `internal Func<string[]> FolderArrayAccessor { get; set; }` | `_dataModel.FolderHelper.FolderArray` | 1037 |
| `internal Func<FolderScore[]> SuggestionScoresAccessor { get; set; }` | `_dataModel?.FolderHelper?.Suggestions?.ToScoredArray() ?? Array.Empty<FolderScore>()` | 890-892 |
| `internal Func<string, string, string, CancellationToken, Task<object>> CreateFolderAsyncAction { get; set; }` | `_dataModel.FolderHelper.CreateFolderAsync(...)` | 492-497 |
| `internal Func<string, string, string, object> CreateFolderAction { get; set; }` | `_dataModel.FolderHelper.CreateFolder(...)` | 771-775 |
| `internal Func<object, string, bool, bool, bool, bool, Task> MoveToFolderAction { get; set; }` | `_dataModel.MoveToFolderAsync((MAPIFolder)f, ...)` | 502-509, 780-789 |

`MoveToFolderAction` takes `object` for the folder parameter so the test seam never has to fabricate a
`MAPIFolder`; the default fall-through performs the `MAPIFolder` cast (mirroring the existing cast at 498).

### S4 — Breadcrumb construction factory seam (tier 2, delegate)

`ConfigureBreadcrumbControl` (834-854) is the only member that touches WebView2. Split it:

```
internal Func<IEfcFormViewer, IApplicationGlobals, WebView2BreadcrumbHost> BreadcrumbHostFactory { get; set; }
internal Func<IApplicationGlobals, IBreadcrumbWebHost, BreadcrumbBridgeRouter> BreadcrumbRouterFactory { get; set; }
```

Defaults reproduce lines 836-849 exactly. The **wiring** (event hookups 850-851, `ApplyTheme` 852,
fire-and-forget init 853) stays in the controller and becomes fully testable.

**This seam requires no edit to any F12-owned or F13-owned file.** Verified:

- `BreadcrumbBridgeRouter` (`BreadcrumbBridgeRouter.cs:19`) is `public sealed` and therefore not mockable —
  but it is **fully constructible headlessly**. Its constructor (`:40-55`) takes
  `IFolderHierarchyProvider` (interface → `Mock<>`), `IBreadcrumbWebHost` (interface → `Mock<>`,
  `IBreadcrumbWebHost.cs:11`), and three plain classes: `BreadcrumbMessageCodec`, `BreadcrumbHtmlRenderer`,
  and `BreadcrumbOutboundQueue` (`BreadcrumbOutboundQueue.cs:23`, takes only `IBreadcrumbWebHost`).
  **No WebView2, no WinForms, no COM.** F9's tests construct a real router over mocks — a strictly better
  test than a mocked router, because it exercises the real selection contract that `SelectedFolder`
  (289-295) depends on.
- `WebView2BreadcrumbHost` (`WebView2BreadcrumbHost.cs:30`, F13-owned, already `[ExcludeFromCodeCoverage]`
  at `:29`) is only ever produced by `BreadcrumbHostFactory`. Tests set the factory to return `null` and
  assert the null-safe paths, or bypass it entirely by injecting a pre-built router.
- `BreadcrumbBridgeRouter.SelectedFolderPath` has a `private set` (`:58`), so tests drive it through the
  real public API: `await router.BindRowsAsync(rows, scores, token)` then `router.SelectFirstRow()`.
  `BreadcrumbRowBuilder.Classify` (`BreadcrumbRowBuilder.cs:150-168`) makes any non-`"===="`-prefixed,
  non-`"Trash to Delete"` string a `Suggestion`, so a single-element `new[]{"Alpha"}` bind produces a
  selectable row deterministically.

**Cross-child contract note (F12):** F9 depends on `BreadcrumbBridgeRouter`'s public surface —
`.ctor(IFolderHierarchyProvider, IBreadcrumbWebHost, BreadcrumbMessageCodec, BreadcrumbHtmlRenderer, BreadcrumbOutboundQueue)`,
`SelectedFolderPath`, `SelectFirstRow()`, `ApplyTheme(bool)`, `NotifyCoreInitialized()`,
`BindRowsAsync(IReadOnlyList<string>, IEnumerable<FolderScore>, CancellationToken)`, and the
`FocusSearchRequested` event. If F12 changes any of these, F9's tests break. **F9 must not edit
`BreadcrumbBridgeRouter.cs`.**

**Cross-child contract note (F13):** F9's `BreadcrumbHostFactory` default constructs
`WebView2BreadcrumbHost` and subscribes to its `CoreInitialized` event (line 850). Note that
`CoreInitialized` is declared on the **concrete class** (`WebView2BreadcrumbHost.cs:63`) and **not** on
`IBreadcrumbWebHost` (`IBreadcrumbWebHost.cs:11-26`). F9 therefore types `BreadcrumbHostFactory`'s return
as the concrete `WebView2BreadcrumbHost`. If F13 promotes `CoreInitialized` onto the interface, F9 can
widen the factory's return type to `IBreadcrumbWebHost` — a follow-up, not a prerequisite.

### S5 — Dialog and dependent-form seams (tier 2, delegate)

Precedent: `EfcHomeController.cs:299-305` `MessageBoxShowAction` and
`EfcHomeController.ExecuteMoves.cs:22-23` `MoveFailureMessageAction`.

| Seam property | Default | Call sites |
| --- | --- | --- |
| `internal Action<string> MessageBoxShowAction { get; set; } = text => MessageBox.Show(text);` | `MessageBox.Show` | 472-474, 710, 756 |
| `internal Action<IApplicationGlobals> ShowManageFiltersAction { get; set; }` | `var f = new ManageFilters(); f.LoadFilters(g); f.Show();` | 563-565 |

**`UtilitiesCS` grants no `InternalsVisibleTo` to `QuickFiler.Test`** (`UtilitiesCS/Properties/AssemblyInfo.cs`
grants only `DynamicProxyGenAssembly2`, `UtilitiesCS.Test`, `ToDoModel.Test`), so `MyBox.DialogInvoker` is
unreachable — hence the **local** delegate seam above, following F3's precedent. `QuickFiler` itself does
grant `InternalsVisibleTo("QuickFiler.Test")` (`QuickFiler/Properties/AssemblyInfo.cs:5`), so all `internal`
seams above are directly reachable from tests without reflection.

### S6 — User-settings seam (tier 2, delegate)

`LoadUserSettings` (1009-1022) reads the `QuickFiler.Properties.Settings.Default` static singleton — mutable
global state, banned by `.claude/rules/general-unit-test.md` § External Dependencies.

```
internal readonly struct EfcUserSettings { bool SaveAttachments, SaveEmail, SavePictures, MoveConversation }
internal Func<EfcUserSettings> UserSettingsReader { get; set; }  // default reads Settings.Default
```

(Place the struct in `EfcFormLayoutMath.cs` or a small `EfcUserSettings.cs`; a plain `readonly struct`, not
a `record struct` — see the net481 note in §3.)

### S7 — Item-controller seam (tier 2, delegate)

`_itemController` is the concrete `EfcItemController` (F9-owned sibling file). `EfcFormController` calls only
four of its members: `InitializeWithoutData()` (107), `InitializeDataFields(dataModel)` (116),
`ToggleNavigation(bool, ToggleState)` (929, 945), `ToggleNavigationAsync(ToggleState)` (938, 954).

Because `EfcItemController` is in F9's own assignment, either an interface seam or a delegate seam is
permissible. **Recommendation: a delegate seam here**, to keep the two files' plans independent and avoid a
new shared interface that both this file's plan and the `EfcItemController` plan would have to agree on
mid-execution:

```
internal Action<bool, Enums.ToggleState> ItemToggleNavigationAction { get; set; }
internal Func<Enums.ToggleState, Task> ItemToggleNavigationAsyncAction { get; set; }
```

### S8 — Pure-math extraction (no seam; new host-neutral module)

**New file** `QuickFiler/Controllers/EfcFormLayoutMath.cs`:

```
internal readonly struct EfcItemViewerLayoutSnapshot   // TlpExpandedHeight, ItemViewerHeight,
                                                       // ItemViewerMinHeight, ItemViewerTlpRow,
                                                       // FirstFiveRowHeights (float[]), BodyRowHeight
internal static class EfcFormLayoutMath
{
    internal static (int expanded, int collapsed, int diff) ComputeTlpHeights(int tlpExpandedHeight,
        int itemViewerHeight, int itemViewerMinHeight);
    internal static float ComputeBodyRowHeight(int collapsedHeight, IReadOnlyList<float> firstFiveRowHeights);
    internal static Size ComputeMinimumFormSize(Size explorerSize);   // 0.75 factor, lines 182-185
    internal static (Size minimum, Size size) ExpandForToggle(Size minimum, Size size, int diff);  // 1061-1068
    internal static (Size minimum, Size size) CollapseForToggle(Size minimum, Size size, int diff); // 1075-1082
}
```

Each function is total, deterministic, and reaches 100% line and branch coverage with a handful of table-driven
tests. `CaptureConfigureItemViewer` and `ToggleExpansionStyle` shrink to ~10 lines of orchestration over
`IEfcFormViewer` intent members and become fully mockable.

### S9 — Explorer-screen-size seam

`_globals.Ol.GetExplorerScreenSize()` (168) needs no new seam: `IOlObjects` is a full interface
(`IOlObjects.cs:11`) declaring `GetExplorerScreenSize()` (`:36`), `DarkMode { get; set; }` (`:30`),
`ArchiveRootPath` (`:15`), `FolderTreeService` (`:21`), and inheriting `INotifyPropertyChanged`.
`Mock<IApplicationGlobals>` + `Mock<IOlObjects>` covers all of it. This is the pattern
`QfcFormControllerTests.cs:93-102` already uses.

---

## 5. Per-member testability verdict

Verdicts assume the S1-S9 seams. **No member of this file is proposed for `[ExcludeFromCodeCoverage]`.**

### 5.1 Testable after seam — no STA, no exemption

| Member (source lines) | Enabling seam |
| --- | --- |
| `.ctor` ×2 (32-77) | S1 internal `IEfcFormViewer` overloads |
| `private .ctor` (79) | already reachable (existing test) |
| `Initialize` (81-99) | S1 + S3 + S7 (`_ = PopulateFolderCombobox()` is verifiable via `FolderArrayAccessor` invocation) |
| `InitializeWithoutData` (101-111) | S1 + S7 |
| `InitializeDataFields` (113-119) | S3 + S7 |
| `CaptureConfigureItemViewer` (166-187) | S1 (`CaptureItemViewerLayout`/`ApplyItemViewerLayout`) + S8 + S9 |
| `Cleanup` (189-196) | S9 (`Mock<IOlObjects>` for `PropertyChanged -=`) |
| `ConfigureFind` (198-206) | S1 (`Text`, `OkButtonText`, `NewFolderButtonText`) |
| `ResolveControlGroups` (208-235) | S1 (`GetChildControlsExcept` returns a test-supplied `Control[]`) |
| `SetupThemes` (237-248) | S1 (`TipsLabels` returns an empty `List<Label>`; `EfcThemeHelper.SetupFormThemes` is `public static` and tolerates empty lists) |
| `ActiveTheme` get/set, `LoadTheme` (254-271) | none needed — reflection-set `_themes` with the `Theme` map built exactly as `QfcFormControllerTests.cs:60-73` does |
| `DarkMode` get/set (273-285) | S9 |
| `FormHandle` (287) | S1 (`IWin32Window.Handle` on the mock) |
| `SelectedFolder` (289-295) | S4 (real router over mocks) |
| four settings properties (297-343) | none needed |
| `Token` (345-350) | none needed |
| `RegisterAlwaysOnAsyncKeyActions` (356-368) | S1 (`KeyboardHandler` returns `Mock<IQfcKeyboardHandler>`) |
| `WireEventHandlers` (370-402) | S1 + S4 + S9 |
| `SearchText_DownArrow` (404-413) | S1 (`FocusFolderList`) + S4 |
| `ButtonCancel_Click` / `ButtonOK_Click` / `ButtonRefresh_Click` / `ButtonDelete_Click` (415-461, 523-534) | S1 + S2 + S3. **`async void` requires a `TaskCompletionSource` gate in the test to observe completion — never a delay.** Set the seam delegate's body to complete a TCS, then `await tcs.Task`. |
| `ButtonCreate_Click` (463-521) | S1 + S2 + S3 + S5 |
| four `*_CheckedChanged` (536-554) | S1 (`SaveAttachmentsChecked` etc.) |
| `SearchText_TextChanged` (556-559) | S1 (`SearchTextValue`) + S3 |
| `EditFiltersMenuItem_Click` (561-566) | S5 |
| `CharacterAsyncActions` / `GetAsyncCharacterActions` (568-603) | S1 — construction only; assert key set and handler count, do not invoke |
| `CharacterActions` / `GetKbdActions` (625-677) | same |
| `DarkMode_Changed` (679-696) | S9 + S4 |
| `ActionOkAsync` (702-731) | S1 + S2 + S4 + S5 |
| `ActionCancelAsync` (733-740) | S1 (`UiSyncContext` returns a plain `new SynchronizationContext()`; verified `UiThread.SynchronizationContextAwaiter` accepts one — `UtilitiesCS.Test/Threading/UiThread_Tests.cs:25,40`) |
| `ActionDeleteAsync` (742-750) | S1 + S4 |
| `CreateFolderAsync` (752-795) | S1 + S2 + S3 + S5 |
| `RefreshSuggestionsAsync` (797-806) | S1 + S3 |
| `KbdExecuteAsync` ×2 (812-822) | S2 (`KeyboardHandlerOverride`) |
| `JumpToAsync` (824-829) | S2 + a plain `new Button()` as the argument (an unparented `Control`, no handle, `Focus()` returns false — no STA needed) |
| `ConfigureBreadcrumbControl` (834-854) | S4 |
| `InitializeBreadcrumbHostAsync` (858-868) | S4 (factory returns null → `NullReferenceException` caught by the boundary; assert no throw) |
| `BindFolderRows` (873-883) | none needed beyond S1 |
| `BindBreadcrumbRowsAsync` (886-903) | S3 + S4 |
| `MaximizeFormViewer` / `MinimizeFormViewer` (905-913) | S1 (`IForm.WindowState`) |
| `ShowMenu` (915) | S1 (`ShowMoveOptionsMenu`) |
| `ToggleCheckboxAsync` (917-921) | S2 + a plain `new CheckBox()` (no handle required to flip `.Checked`) |
| `ToggleOn/OffNavigation(+Async)` ×4 (923-955) | S2 + S7 |
| `ToggleTips(bool)` / `ToggleTips(bool, state)` (957-989) | S1 — `Mock<IEfcFormViewer>` intercepts `Invoke(Delegate)` / `BeginInvoke(Delegate)` and the test invokes the captured delegate itself |
| `ToggleTipsAsync` (991-1007) | none needed — `Mock<IQfcTipsDetails>` via reflection-set `_listTipsDetails` |
| `LoadUserSettings` (1009-1022) | S6 + S1 |
| `PopulateFolderCombobox` (1024-1038) | S1 + S3 |
| `IsValidSelection` (1040-1052) | S4 (or reflection-set `_router`) |
| `ToggleExpansionStyle` (1056-1084) | S1 + S8 |

### 5.2 Needs the STA last-resort clause

**None.** Every WinForms interaction in this file is either (a) a property/method on the form itself, which
`Mock<IEfcFormViewer>` covers, or (b) an unparented, never-shown `Control`/`CheckBox`/`Button` instance,
which constructs without a message loop and without a window handle. No `TableLayoutPanel.RowStyles`
manipulation, no `GetPositionFromControl`, and no `Handle` realisation remains on the controller side after
S1 + S8. **No `*.StaTests.cs` file is required for this target.**

This is a materially better outcome than the epic's F14 expectation and should be reported as such.

### 5.3 Irreducible-remainder candidates

**None in this file.**

The irreducible remainder for this cluster is `QuickFiler/Viewers/EfcViewer.cs` — the `Form`-derived adapter
that will hold the 1:1 Designer forwards after S1. It qualifies under `CLAUDE.md` §UT2 ground (b)
("WinForms form-derived classes"), it already carries `[ExcludeFromCodeCoverage]` at `EfcViewer.cs:20`, and
that attribute should be **retained with a written rationale in F1's ledger**, not removed. That disposition
is the sibling `EfcViewer.cs` artifact's call; this artifact records the dependency.

> **Blocking-finding warning for the plan author.** Under the epic's §1 policy reconciliation,
> `[ExcludeFromCodeCoverage]` on any of the eight `EfcFormController.*.cs` partials is a Blocking finding.
> The split must not be used to concentrate uncovered lines into a partial that is then exempted.

---

## 6. Proposed test inventory

Conventions for every case below: MSTest `[TestClass]`/`[TestMethod]`, Moq, FluentAssertions,
Arrange-Act-Assert, deterministic, no temp files, no external services, no live forms, no popups, no
`Thread.Sleep`/`Task.Delay`/`DateTime.Now`. `async void` handlers are observed via a
`TaskCompletionSource` completed from inside the injected seam delegate.

**Shared harness (1 file, not a test case):** `QuickFiler.Test/Controllers/EfcFormController.TestSupport.cs`,
mirroring the merged precedent `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`. Provides
`CreateController(...)`, `GetPrivateField<T>`/`SetPrivateField<T>` (copy the shape from
`QfcFormControllerTests.cs:33-53`), `CreateThemeMap()` (copy `QfcFormControllerTests.cs:60-73`),
`CreateRealRouter(out Mock<IBreadcrumbWebHost>, out Mock<IFolderHierarchyProvider>)`, and
`CreateMockViewer()`. **Retain `CreateMinimalController()` from the existing test file unchanged.**

Scenario classes: **P** positive, **N** negative, **E** edge, **X** error-handling, **S** state-transition.

### 6.1 `EfcFormControllerConstructionTests.cs` (~11 cases)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 1 | `Ctor_WithDataModel_AssignsAllInjectedCollaborators` | 32-51 | P |
| 2 | `Ctor_WithDataModel_CapturesItemViewerAndItemTableLayoutFromViewer` | 49-50 | P |
| 3 | `Ctor_WithoutDataModel_LeavesDataModelNull` | 53-77 | E |
| 4 | `PrivateParameterlessCtor_ProducesInstanceWithAllFieldsNull` | 79 | E |
| 5 | `Initialize_InvokesSetupSequenceInOrder` | 81-99 | S |
| 6 | `Initialize_ReturnsSameInstance` | 98 | P |
| 7 | `Initialize_TriggersFolderComboboxPopulation` | 97 | P |
| 8 | `InitializeWithoutData_DoesNotPopulateFolderCombobox` | 101-111 | E |
| 9 | `InitializeWithoutData_DelegatesToItemControllerInitializeWithoutData` | 107 | P |
| 10 | `InitializeDataFields_AssignsDataModelAndRepopulates` | 113-119 | S |
| 11 | `InitializeDataFields_ReturnsSameInstance` | 118 | P |

### 6.2 `EfcFormControllerPropertiesTests.cs` (~17 cases)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 12 | `ActiveTheme_WhenThemesNull_ThrowsArgumentNullException` | 257 | X |
| 13 | `ActiveTheme_WhenUnset_LoadsFromLoadTheme` | 257 | P |
| 14 | `ActiveTheme_WhenAlreadySet_ReturnsCachedValueWithoutReloading` | 257 | E |
| 15 | `ActiveTheme_Set_AppliesThemeAsynchronously` | 259-263 | S |
| 16 | `LoadTheme_WhenDarkModeTrue_ReturnsDarkNormalAndApplies` | 266-271 | P |
| 17 | `LoadTheme_WhenDarkModeFalse_ReturnsLightNormalAndApplies` | 266-271 | P |
| 18 | `DarkMode_Get_ReadsFromOlObjects` | 276-283 | P |
| 19 | `DarkMode_Get_WhenGlobalsNull_ThrowsNullReference` | 276-283 | X (pins current behavior — see D3) |
| 20 | `DarkMode_Set_WritesThroughToOlObjects` | 284 | S |
| 21 | `FormHandle_ReturnsViewerHandle` | 287 | P |
| 22 | `SelectedFolder_WhenRouterNull_ReturnsNull` | 294 | N |
| 23 | `SelectedFolder_AfterRouterSelectsSuggestionRow_ReturnsFullPath` | 294 | P |
| 24 | `SaveAttachments_SaveEmail_SavePictures_MoveConversation_RoundTrip` | 297-343 | P |
| 25 | `Token_RoundTrips` | 345-350 | P |
| 26 | `IsValidSelection_WhenSelectedFolderNull_ReturnsFalse` | 1046 | N |
| 27 | `IsValidSelection_WhenSelectedFolderEmptyOrShorterThanThree_ReturnsFalse` | 1047-1048 | E |
| 28 | `IsValidSelection_WhenSelectedFolderStartsWithTripleEquals_ReturnsFalse` | 1049 | E |
| 29 | `IsValidSelection_WhenSelectedFolderIsRealPath_ReturnsTrue` | 1045-1050 | P |

### 6.3 `EfcFormControllerSetupTests.cs` (~14 cases)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 30 | `CaptureConfigureItemViewer_ComputesExpandedCollapsedAndDiffHeights` | 166-187 | P |
| 31 | `CaptureConfigureItemViewer_SetsFormMinimumToSeventyFivePercentOfExplorerScreen` | 182-186 | P |
| 32 | `CaptureConfigureItemViewer_TogglesExpansionStyleOff` | 174 | S |
| 33 | `Cleanup_UnsubscribesDarkModeChangedFromOlObjects` | 191 | S |
| 34 | `Cleanup_NullsGlobalsViewerAndDataModel` | 192-194 | S |
| 35 | `Cleanup_InvokesParentCleanup` | 195 | P |
| 36 | `ConfigureFind_WhenInitTypeHasFind_RewritesTitleAndTwoButtonCaptions` | 200-205 | P |
| 37 | `ConfigureFind_WhenInitTypeIsSort_LeavesCaptionsUntouched` | 200 | N |
| 38 | `ResolveControlGroups_PartitionsButtonsCheckboxesHighlightedAndDefault` | 217-234 | P |
| 39 | `ResolveControlGroups_TogglesEveryTipsDetailOff` | 213 | S |
| 40 | `ResolveControlGroups_ExcludesItemViewerFromChildEnumeration` | 215 | E |
| 41 | `SetupThemes_PopulatesThemeMapAndSetsActiveTheme` | 239-247 | P |
| 42 | `LoadUserSettings_CopiesFourSettingsIntoFieldsAndMenuChecks` | 1009-1022 | P |
| 43 | `ToggleExpansionStyle_OnAndOff_AdjustRowHeightAndFormSizeSymmetrically` | 1056-1084 | S |

### 6.4 `EfcFormControllerEventHandlerTests.cs` (~19 cases)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 44 | `RegisterAlwaysOnAsyncKeyActions_RegistersReturnKeyBoundToActionOk` | 356-368 | P |
| 45 | `WireEventHandlers_SubscribesAllThirteenViewerEvents` | 375-400 | P |
| 46 | `WireEventHandlers_SubscribesDarkModeChangedToOlPropertyChanged` | 401 | P |
| 47 | `WireEventHandlers_ConfiguresBreadcrumbControl` | 393 | S |
| 48 | `SearchTextDownArrow_WhenKeyIsDown_FocusesFolderListAndSelectsFirstRow` | 406-412 | P |
| 49 | `SearchTextDownArrow_WhenKeyIsNotDown_DoesNothing` | 406 | N |
| 50 | `SearchTextDownArrow_WhenRouterNull_DoesNotThrow` | 411 | E |
| 51 | `ButtonOkClick_InvokesActionOkAsync` | 431-445 | P |
| 52 | `ButtonOkClick_WhenActionThrows_LogsAndRethrows` | 440-444 | X |
| 53 | `ButtonCancelClick_InvokesActionCancelAsync` | 415-429 | P |
| 54 | `ButtonRefreshClick_InvokesRefreshSuggestionsAsync` | 447-461 | P |
| 55 | `ButtonDeleteClick_InvokesActionDeleteAsync` | 523-534 | P |
| 56 | `ButtonCreateClick_WhenSelectionInvalid_ShowsMessageAndDoesNotCreate` | 470-475 | N |
| 57 | `ButtonCreateClick_WhenFindMode_OpensFileSystemFolderThenClosesAndCleansUp` | 476-482 | P |
| 58 | `ButtonCreateClick_WhenOneDriveMissing_ReturnsWithoutCreating` | 485-489 | N |
| 59 | `ButtonCreateClick_WhenFolderCreated_MovesThenClosesAndCleansUp` | 500-513 | P |
| 60 | `ButtonCreateClick_WhenFolderCreationReturnsNull_LeavesFormOpen` | 500 | E |
| 61 | `MenuCheckedChangedHandlers_MirrorMenuStateIntoProperties` (4 assertions, one member each) | 536-554 | P |
| 62 | `SearchTextChanged_BindsFindMatchesResultToBreadcrumb` | 556-559 | P |
| 63 | `EditFiltersMenuItemClick_OpensManageFiltersWithGlobals` | 561-566 | P |
| 64 | `DarkModeChanged_WhenPropertyNameMatches_SwapsThemeAndRethemesBreadcrumb` | 679-695 | S |
| 65 | `DarkModeChanged_WhenPropertyNameDiffers_DoesNothing` | 681 | N |

### 6.5 `EfcFormControllerKeyboardTests.cs` (~10 cases)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 66 | `GetAsyncCharacterActions_RegistersSevenControllerKeys` | 572-603 | P |
| 67 | `CharacterAsyncActions_IsMemoizedAcrossReads` | 569-570 | E |
| 68 | `GetKbdActions_RegistersEightControllerKeys` | 629-677 | P |
| 69 | `CharacterActions_IsMemoizedAcrossReads` | 626-627 | E |
| 70 | `KbdExecuteAsync_Func_TogglesKeyboardDialogThenAwaitsAction` | 812-816 | S |
| 71 | `KbdExecuteAsync_Action_TogglesKeyboardDialogThenInvokesAction` | 818-822 | S |
| 72 | `JumpToAsync_TogglesKeyboardDialogThenFocusesControl` | 824-829 | S |
| 73 | `ToggleOffNavigation_RemovesControllerKeysTogglesTipsOffAndItemNavOff` | 923-930 | S |
| 74 | `ToggleOffNavigationAsync_RemovesAsyncKeysAndAwaitsBothToggles` | 932-939 | S |
| 75 | `ToggleOnNavigation_AddsControllerKeysTogglesTipsOnAndItemNavOn` | 941-946 | S |
| 76 | `ToggleOnNavigationAsync_AddsAsyncKeysAndAwaitsBothToggles` | 948-955 | S |

### 6.6 `EfcFormControllerActionsTests.cs` (~14 cases)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 77 | `ActionOkAsync_WhenSelectedFolderNull_ShowsMessageAndReturns` | 707-712 | N |
| 78 | `ActionOkAsync_WhenSelectedFolderIsBanner_ShowsMessageAndReturns` | 708 | E |
| 79 | `ActionOkAsync_WhenSortMode_ExecutesMovesThenDisposesAndCleansUp` | 716-729 | P |
| 80 | `ActionOkAsync_WhenFindMode_OpensOutlookFolderThenDisposesAndCleansUp` | 720-723 | P |
| 81 | `ActionOkAsync_WhenNeitherSortNorFind_ThrowsNotImplementedException` | 726 | X |
| 82 | `ActionCancelAsync_ClosesViewerAndCleansUp` | 733-740 | P |
| 83 | `ActionDeleteAsync_PrependsTrashRowAndRebinds` | 742-750 | S |
| 84 | `ActionDeleteAsync_WhenNoRowsPresented_BindsTrashRowOnly` | 747-749 | E |
| 85 | `CreateFolderAsync_WhenSelectionInvalid_ShowsMessage` | 754-757 | N |
| 86 | `CreateFolderAsync_WhenFindMode_OpensFileSystemFolder` | 758-761 | P |
| 87 | `CreateFolderAsync_WhenOneDriveMissing_ReturnsAfterHidingForm` | 764-769 | N |
| 88 | `CreateFolderAsync_WhenFolderCreated_MovesDisposesAndCleansUp` | 778-793 | P |
| 89 | `CreateFolderAsync_WhenFolderCreationReturnsNull_DoesNotDispose` | 778 | E |
| 90 | `RefreshSuggestionsAsync_RefreshesThenFindsMatchesThenBinds` | 797-806 | S |
| 91 | `PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel` | 1029-1031 | **EXISTING — migrate verbatim** |
| 92 | `PopulateFolderCombobox_InitializesFolderHandlerThenBindsFolderArray` | 1033-1037 | P |

### 6.7 `EfcFormControllerBreadcrumbTests.cs` (~11 cases)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 93 | `ConfigureBreadcrumbControl_CreatesHostAndRouterThroughFactories` | 836-849 | P |
| 94 | `ConfigureBreadcrumbControl_WiresCoreInitializedToRouterNotification` | 850 | S |
| 95 | `ConfigureBreadcrumbControl_WiresFocusSearchRequestedToSearchTextSelect` | 851 | S |
| 96 | `ConfigureBreadcrumbControl_AppliesCurrentThemeToRouter` | 852 | P |
| 97 | `InitializeBreadcrumbHostAsync_WhenHostThrows_LogsAndDoesNotPropagate` | 864-867 | X |
| 98 | `BindFolderRows_WhenViewerNull_ReturnsWithoutBinding` | 875-879 | N |
| 99 | `BindFolderRows_WhenRouterNull_ReturnsWithoutBinding` | 876 | N |
| 100 | `BindFolderRows_WhenRowsNull_StoresEmptyArray` | 881 | E |
| 101 | `BindFolderRows_StoresPresentedRowsForLaterTrashPrepend` | 881 | S |
| 102 | `BindBreadcrumbRowsAsync_JoinsSuggestionScoresIntoRouterBind` | 890-893 | P |
| 103 | `BindBreadcrumbRowsAsync_WhenSuggestionsUnavailable_PassesEmptyScoreArray` | 891-892 | E |
| 104 | `BindBreadcrumbRowsAsync_WhenCanceled_LogsDebugAndSwallows` | 895-898 | X |
| 105 | `BindBreadcrumbRowsAsync_WhenRouterThrows_LogsErrorAndSwallows` | 899-902 | X |

### 6.8 `EfcFormControllerTipsTests.cs` (~9 cases)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 106 | `ToggleTips_WhenAsync_DispatchesEachTipThroughBeginInvoke` | 961-964 | S |
| 107 | `ToggleTips_WhenSynchronous_DispatchesEachTipThroughInvoke` | 966-968 | S |
| 108 | `ToggleTipsWithState_WhenAsync_DispatchesDesiredStateThroughBeginInvoke` | 976-981 | S |
| 109 | `ToggleTipsWithState_WhenSynchronous_DispatchesDesiredStateThroughInvoke` | 983-987 | S |
| 110 | `ToggleTipsAsync_TogglesEveryTipToDesiredStateWithSharedColumn` | 996-1000 | P |
| 111 | `ToggleTipsAsync_WhenTokenAlreadyCanceled_ThrowsOperationCanceled` | 993 | X |
| 112 | `ToggleTipsAsync_WithEmptyTipsList_CompletesWithoutThrowing` | 996-1000 | E |
| 113 | `MaximizeFormViewer_SetsWindowStateMaximized` | 905-908 | P |
| 114 | `MinimizeFormViewer_SetsWindowStateMinimized` | 910-913 | P |
| 115 | `ShowMenu_ShowsMoveOptionsDropDown` | 915 | P |
| 116 | `ToggleCheckboxAsync_TogglesKeyboardDialogThenInvertsCheckedState` | 917-921 | S |

### 6.9 `EfcFormLayoutMathTests.cs` (~9 cases, new module — 90% floor applies)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 117 | `ComputeTlpHeights_ReturnsExpandedCollapsedAndDifference` | S8 | P |
| 118 | `ComputeTlpHeights_WhenItemViewerAtMinimum_ProducesZeroDifference` | S8 | E |
| 119 | `ComputeBodyRowHeight_SubtractsSumOfFirstFiveRowsAndAddsBodyRow` | S8 | P |
| 120 | `ComputeBodyRowHeight_WhenRowHeightsEmpty_ReturnsCollapsedHeight` | S8 | E |
| 121 | `ComputeBodyRowHeight_WhenSumExceedsCollapsed_ReturnsNegative` | S8 | E |
| 122 | `ComputeMinimumFormSize_ScalesExplorerSizeBySeventyFivePercent` | S8 | P |
| 123 | `ComputeMinimumFormSize_WhenExplorerSizeZero_ReturnsZero` | S8 | E |
| 124 | `ExpandForToggle_AddsDifferenceToBothMinimumAndSize` | S8 | P |
| 125 | `CollapseForToggle_SubtractsDifferenceFromBothMinimumAndSize` | S8 | P |
| 126 | `ExpandThenCollapseForToggle_IsIdentity` | S8 | S |

**Total: ~126 test cases across 8 new test files plus 1 test-support file.** Every file projects well
under 500 lines. **Zero cases require the STA last-resort clause; no `*.StaTests.cs` file is proposed.**

---

## 7. Breadcrumb coupling boundary and cross-child contracts

### 7.1 Precise boundary map

```
EfcFormController                            OWNER   F9 may edit?
 ├─ ConfigureBreadcrumbControl  :834-854      F9      yes  (becomes factory-seam wiring)
 │   ├─ new WebView2BreadcrumbHost(...)       F13     NO   (produced by BreadcrumbHostFactory)
 │   │    └─ EfcViewer.BreadcrumbWebView      F9      yes  (EfcViewer.cs:92)
 │   ├─ new WebView2CoreInitializer()         F13     NO
 │   ├─ new OutlookFolderHierarchyProvider    UtilCS  NO   (over IOlObjects.FolderTreeService)
 │   ├─ new BreadcrumbMessageCodec()          UtilCS  NO
 │   ├─ new BreadcrumbHtmlRenderer()          UtilCS  NO
 │   ├─ new BreadcrumbOutboundQueue(host)     F2      NO   (epic line 334)
 │   └─ new BreadcrumbBridgeRouter(...)       F12     NO
 ├─ InitializeBreadcrumbHostAsync :858-868    F9      yes
 ├─ BindFolderRows              :873-883      F9      yes
 ├─ BindBreadcrumbRowsAsync     :886-903      F9      yes  -> router.BindRowsAsync
 ├─ SelectedFolder              :289-295      F9      yes  -> router.SelectedFolderPath
 ├─ SearchText_DownArrow        :404-413      F9      yes  -> router.SelectFirstRow
 └─ DarkMode_Changed            :679-696      F9      yes  -> router.ApplyTheme
```

The controller's dependence on F12 is **six public surface points only** (constructor, `BindRowsAsync`,
`SelectedFolderPath`, `SelectFirstRow`, `ApplyTheme`, `NotifyCoreInitialized`) plus the
`FocusSearchRequested` event. The S4 factory seam makes all of them reachable in tests **with a real
`BreadcrumbBridgeRouter` instance over mocked collaborators**, so F9 needs no edit to any F12- or
F13-owned file and no new interface over the router.

### 7.2 Cross-child contract notes to record in the plan

- **F12 (`BreadcrumbBridgeRouter.cs`):** F9's tests construct the real router. If F12 changes the
  constructor arity, seals off `SelectFirstRow`, or changes `SelectedFolderPath`'s derivation
  (`BreadcrumbBridgeRouter.cs:364-380`), F9's tests fail at fan-in. Record as a watch item, not a blocker.
- **F13 (`WebView2BreadcrumbHost.cs`, `IBreadcrumbWebHost.cs`, `WebView2CoreInitializer.cs`):**
  `CoreInitialized` exists on the concrete class only (`WebView2BreadcrumbHost.cs:63`), not on
  `IBreadcrumbWebHost` (`IBreadcrumbWebHost.cs:11-26`). F9's `BreadcrumbHostFactory` therefore returns the
  concrete type. If F13 promotes the event to the interface, F9 may widen the return type afterwards.
- **F2 (`BreadcrumbOutboundQueue.cs`):** consumed only by the S4 default factory body. No F9 edit.
- **F5 (`EfcDataModel.cs`):** F9 depends on `FindMatches`, `RefreshSuggestions`, `InitFolderHandlerAsync`,
  `FolderHelper`, and `MoveToFolderAsync(MAPIFolder, string, bool, bool, bool, bool)`. All are reached
  through S3 delegate seams so F9 makes **zero** edits to `EfcDataModel.cs`.
- **F8 (`EfcHomeControllerDependencies.cs`, `EfcHomeControllerDependencyFactories.cs`):** **prohibited from
  editing.** F9 preserves the exact shapes those files bind to: both public constructors with `EfcViewer`
  and `EfcDataModel` parameter types, `Initialize()`, `InitializeWithoutData()`,
  `InitializeDataFields(EfcDataModel)`. Verified against `EfcHomeControllerDependencies.cs:15-32,251-398`
  and `EfcHomeControllerDependencyFactories.cs:70-129,158-266`.
- **F4 (`EfcViewerQueue.cs`, `EfcThemeHelper.cs`):** `EfcViewerQueue.Dequeue` is a method group consumed at
  `EfcHomeControllerDependencyFactories.cs:40,112` — new behavior there must be a **new overload**, never an
  optional parameter. **`EfcFormController.cs` does not reference `EfcViewerQueue` at all**, so this
  constraint is inherited context only. `EfcThemeHelper.SetupFormThemes` (`EfcThemeHelper.cs:249-255`) is
  consumed as a `public static` call at line 239; F9 does not edit it.
- **F14 (`IItemViewer.cs`, `ItemViewer.cs`):** `IItemViewer` (`IItemViewer.cs:15-132`) declares `Height`
  (`:128`) but **not** `MinimumSize` and **not** `L0vh_Tlp`. F9 therefore must **not** retype `_itemViewer`
  as `IItemViewer`; the layout reads move behind `IEfcFormViewer.CaptureItemViewerLayout()` instead
  (S1 + S8). No F14 edit.

---

## 8. Issue #439 — implicated members and semantic-conflict risk

Issue #439 `Bug: efcviewer-missing-lineage-and-segment-navigation` is **OPEN**. Read from
`https://github.com/drmoisan/TaskMaster/issues/439` (`gh` unavailable this session).

Reported symptom: suggestion and search-result rows render a single leaf segment instead of a
root-to-leaf lineage with arrow separators; clicking a non-leaf segment does not navigate.
Named components: `EfcViewer.FolderListBox` (as `BreadcrumbWebView`), `EfcFormController`,
`BreadcrumbBridgeRouter`, `QuickFiler/Resources/FolderBreadcrumb.html`,
`EfcDataModel.FindMatches`, `FolderPredictor.Suggestions`.

### 8.1 Mechanism (verified end-to-end from source)

1. Presented rows are produced as **relative folder stems**, not rooted Outlook `FolderPath` values.
   `FolderPredictor.AddSuggestions` (`FolderPredictor.cs:804-808`) emits the literal separator
   `"========= SUGGESTIONS ========="` followed by `Suggestions.ToArray(5)` — the raw keys of
   `FolderScorer._folderNameScores` (`FolderScorer.cs:250-253`), which are folder-name stems.
   `FolderPredictor.FindFolder` (`:292-342`) returns `FolderArray` built the same way.
2. `EfcFormController` passes those strings straight through:
   `PopulateFolderCombobox:1037` → `BindFolderRows(_dataModel.FolderHelper.FolderArray)`;
   `SearchText_TextChanged:558` → `BindFolderRows(_dataModel.FindMatches(...))`;
   `RefreshSuggestionsAsync:800-805` → `BindFolderRows(matches)`.
3. `BindBreadcrumbRowsAsync:893` hands them to `BreadcrumbBridgeRouter.BindRowsAsync`.
4. The router calls `FetchChainAsync` → `_provider.ResolveLeafKeyAsync(text, ct)`
   (`BreadcrumbBridgeRouter.cs:341-344`).
5. `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` (`OutlookFolderHierarchyProvider.cs:52-71`)
   matches **`node.FolderPath`** — the rooted Outlook path including the store name (`:64-68`).
   A relative stem never matches, so it returns `null` (`:70`).
6. `FetchChainAsync` returns `null` (`BreadcrumbBridgeRouter.cs:346-347`), so no chain enters the
   `chains` dictionary (`:104-107`), so `BreadcrumbRowBuilder.BuildRows` falls back to its documented
   single-segment rendering (`BreadcrumbRowBuilder.cs:28-31`).

**That is exactly the reported "rows display as single leaf segments with no lineage or separators."**
The defect is a *path-namespace mismatch* between the suggestion producer and the hierarchy provider,
not a rendering defect.

### 8.2 Members F9 will touch that lie on the #439 path

**Every one of these is refactored by F9's split and seam plan.** The plan and its characterization tests
must pin **CURRENT** behavior, not the intended #439 behavior.

| Member | Lines | Role on the #439 path |
| --- | --- | --- |
| `PopulateFolderCombobox` | 1024-1038 | Injects the initial `FolderArray` rows |
| `SearchText_TextChanged` | 556-559 | Injects `FindMatches` search rows |
| `RefreshSuggestionsAsync` | 797-806 | Re-injects rows after a suggestion refresh |
| `ActionDeleteAsync` | 742-750 | Prepends the trash pseudo-row and re-injects |
| `BindFolderRows` | 873-883 | Single funnel for all four producers |
| `BindBreadcrumbRowsAsync` | 886-903 | Joins scores and calls `router.BindRowsAsync` |
| `ConfigureBreadcrumbControl` | 840-842 | Constructs `OutlookFolderHierarchyProvider` over `IOlObjects.FolderTreeService` — **the exact object whose path namespace does not match** |
| `SelectedFolder` | 289-295 | Consumes the router's selection; a lineage fix changes what a "selected" row's `FullPath` is |
| `IsValidSelection` | 1040-1052 | Downstream consumer of `SelectedFolder` |

### 8.3 Instructions for the plan author

- **F9 must NOT fix #439** (epic NFR: "No behavior change to end-user QuickFiler flows").
- Characterization tests for cases 92, 62, 90, 83, 102, 103, 23, 26-29 must assert the **present**
  contract: relative-stem rows are passed through verbatim; the router receives them unchanged; a row whose
  chain lookup yields `null` still binds. **Do not write an assertion that a multi-segment lineage appears.**
- Because F9 restructures `ConfigureBreadcrumbControl` into a factory seam, whoever eventually fixes #439
  will find the provider construction relocated. **Record this as a semantic-conflict risk against #439**
  and note in the F9 PR body that the #439 fix point is now `BreadcrumbRouterFactory`'s default body
  (formerly `EfcFormController.cs:840-842`).
- #439 is **not** in `docs/features/active/`; it is an open issue only. Per the epic's own lesson from #426
  ("a promoted-but-not-yet-active issue is invisible to a `docs/features/active/` scan"), it must be listed
  explicitly in F9's Known Conflict Risks.

---

## 9. Latent defects (report only — do NOT fix in F9)

Per the epic's "Latent Defect Promotion" directive, these should be promoted to GitHub issues via the MCP
promotion lifecycle rather than left as prose.

**D1 — `EfcViewer.SetController` is dead and leaves a latent `NullReferenceException`.**
`EfcViewer.cs:50-53` declares `internal void SetController(EfcFormController controller)`. A repository-wide
search for `SetController` finds call sites only in `QfcFormController.cs:44`, `QfcFormViewer.cs:46`,
`QfcFormViewerDark.cs:31`, `QfcFormViewerExpanded.cs:31`, and the non-compiled `Legacy/`/`EfcViewer3.cs`
files. **`EfcFormController` never calls it**, unlike its QFC twin. Consequently `EfcViewer._formController`
(`EfcViewer.cs:48`) is always null, and `EfcViewer.EditFiltersMenuItem_Click` (`EfcViewer.cs:157-160`) would
throw NRE. It is currently unreachable — `EfcViewer.Designer.cs` never wires
`EditFiltersMenuItem.Click` (verified: the only Designer references are the declaration at `:67`, the
`DropDownItems` add at `:4123`, three property assignments at `:4136-4138`, and the field at `:4275`) — so
this is dead code carrying a latent trap, not a live crash.

**D2 — `Cleanup()` is not idempotent and has no re-entrancy guard.**
`EfcFormController.cs:189-196`. A second invocation dereferences the already-nulled `_globals` at line 191.
Two independent paths can invoke the OK action for a single user gesture: the always-on Return key binding
(`:365`, `new KaKeyAsync("Collection", Keys.Return, (k) => ActionOkAsync())`) and the OK button `Click`
subscription (`:391`). If both fire, the second `ActionOkAsync` NREs at `:705` on the nulled `_formViewer`.
No `_isExecuting`-style guard exists here, unlike `EfcHomeController.TryBeginExecuteMoves`
(`EfcHomeController.ExecuteMoves.cs:48-57`).

**D3 — `DarkMode` and `ActiveTheme`/`LoadTheme` lack the null guards their QFC twins have.**
`EfcFormController.cs:276-283` passes `_globals.Ol` eagerly as a `params object[]` element, so the getter
NREs when `_globals` is null. `EfcFormController.cs:257` uses `strict: true` with `_themes` as the sole
dependency, so the getter throws `ArgumentNullException` (`Initializer.cs:310-321`) when `_themes` is null.
`EfcFormController.cs:269` NREs on a null `_themes`. The already-merged twin guards all three:
`QfcFormController.cs:103-105` (`_themes is null ? _activeTheme : ...`), `:123` (`if (_themes is not null && _themes.TryGetValue(...))`),
`:134` (`_globals?.Ol is null ? _darkMode : ...`). The EFC side is the unguarded sibling.

**D4 — `RefreshSuggestionsAsync` reads a WinForms control property from a thread-pool thread.**
`EfcFormController.cs:800-803` evaluates `_formViewer.SearchText.Text` **inside** the `Task.Run` lambda.
Reading `Control.Text` off the UI thread is an illegal cross-thread control access. Contrast
`SearchText_TextChanged:558`, which reads the same property on the UI thread.

**D5 — `ActionDeleteAsync` accumulates duplicate trash rows.**
`EfcFormController.cs:742-750` reads `_folderRows`, inserts `"Trash to Delete"` at index 0, and rebinds.
`BindFolderRows:881` then stores the *result* (which now contains the trash row) back into `_folderRows`.
A second invocation — via the `'T'` keyboard action (`:595`, `:667`) or the delete button (`:397`) —
inserts a second `"Trash to Delete"` row. No dedupe guard exists.

**D6 — `_ = PopulateFolderCombobox()` is fire-and-forget with no error boundary.**
`EfcFormController.cs:97` and `:117`. `PopulateFolderCombobox` (`:1024-1038`) has no `try`/`catch`, so any
failure inside `InitFolderHandlerAsync` or `FolderHelper.FolderArray` faults an unobserved `Task` and the
folder list silently stays empty. Contrast the sibling fire-and-forget at `:853`, whose callee
`InitializeBreadcrumbHostAsync` (`:858-868`) does carry an explicit logged boundary.

**D7 — Banner-prefix detection is inconsistent across three sites.**
`IsValidSelection:1049` tests `Substring(0, 3) == "==="` (three characters);
`ActionOkAsync:708` tests `StartsWith("====")` (four);
`BreadcrumbRowBuilder.BannerPrefix` (`BreadcrumbRowBuilder.cs:19`) is `"===="` (four).
A row beginning with exactly three `=` is rejected by `IsValidSelection` but accepted by `ActionOkAsync` and
classified `Suggestion` by the row builder.

**D8 — `ButtonDelete_Click` omits the synchronization-context bootstrap its four siblings perform.**
`EfcFormController.cs:523-534` versus `:415-429`, `:431-445`, `:447-461`, `:463-521`. The callee
`ActionDeleteAsync:744` awaits `_formViewer.UiSyncContext` first, so the observable behavior is probably
equivalent, but the asymmetry is unexplained and fragile.

**D9 — Five `async void` handlers rethrow from their catch block.**
`EfcFormController.cs:424-428`, `:440-444`, `:456-460`, `:516-520`, `:529-533` each do
`logger.Error(...); throw;` inside an `async void` method. A rethrow from `async void` is posted to the
synchronization context as an unhandled exception, which terminates the host process rather than surfacing
a handled error. This is a boundary-design defect, not a regression, and is out of scope for F9.

**D10 — 19 lines of commented-out dead code.**
`EfcFormController.cs:605-623` (a superseded `GetKbdActions` dictionary implementation). Also
`:147-148`, `:304-305`, `:311-312`, `:317-318`, `:323-324`, `:583-586`, `:735,737`, `:827`, `:1002-1006`.
Deleting these during the partial split is in-scope hygiene, not a behavior change.

**Observation (not a defect):** `BindBreadcrumbRowsAsync:891` calls `ToScoredArray()` with no `topN`,
returning the full scored set, while the presented rows come from `Suggestions.ToArray(5)`
(`FolderPredictor.cs:807`). Because `BreadcrumbRowBuilder.BuildRows` joins scores by path equality and
`FolderScore.Probability` is max-normalized over the full ordered set regardless of `topN`
(`FolderScorer.cs:275-302`), the surplus scores are inert. Recorded so a future reader does not mistake it
for a defect.

---

## 10. Testing implications and acceptance evidence

- **Gates for this file:** >= 80% line and >= 75% branch **per partial file**, measured from the Cobertura
  produced by F1's harness on F9's own branch and committed under
  `docs/features/active/2026-08-07-quickfiler-efc-form-item-controller-coverage-452/evidence/qa-gates/`.
  New files (`EfcFormLayoutMath.cs`) carry the 90% new-module floor (`CLAUDE.md` §UT2).
- **Harness correctness (epic §"Two harness correctness requirements"):** each partial will emit multiple
  Cobertura `<class>` elements sharing one `filename` (the type plus its `<>c` closure classes, of which
  this file will produce many because of the `KbdActions` lambda sets at 572-603 and 629-677 and the
  `Initializer.SetAndSave`/`GetOrLoad` lambdas). The report must union them by `filename` taking max hits
  per line, and must key the denominator on `<line>` child count, not `line-rate`.
- **Ledger rows F9 must append (epic §"Mid-Wave File Creation"):** 8 partials + `EfcFormLayoutMath.cs` as
  `testable`; `IEfcFormViewer.cs` as `interface-only / not-measured` (reported N/A, never 0%, and receiving
  **no** `[ExcludeFromCodeCoverage]`).
- **Determinism:** no `Thread.Sleep`, `Task.Delay`, or `DateTime.Now`; `async void` handlers observed via
  `TaskCompletionSource`; `UiSyncContext` supplied as a plain `new SynchronizationContext()` (proven
  compatible with `UiThread.SynchronizationContextAwaiter` by `UtilitiesCS.Test/Threading/UiThread_Tests.cs:25,40`).
- **Toolchain:** `csharpier .` → `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  → `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` → `vstest.console.exe <assemblies> /EnableCodeCoverage`,
  restarting from step 1 on any failure or file change. When running the test step locally, exclude
  `\.claude\` paths from any recursive `*.Test.dll` search so stale agent-worktree builds are not picked up.
- **Toolchain risk to flag in the plan:** step 3 runs with `/p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  `EfcFormController.cs` currently has no `#nullable enable` directive, whereas
  `BreadcrumbBridgeRouter.cs:1` and `BreadcrumbOutboundQueue.cs:1` do. Splitting into 8 partials multiplies
  the surface exposed to that gate. Budget a task for nullable-warning cleanup and do **not** add
  `#nullable enable` to the new partials unless the plan also budgets the annotation work.

---

## 11. Summary of recommendations

1. Adopt **Approach C**: `IEfcFormViewer : IForm` interface seam over `EfcViewer` (mirroring the merged
   `IQfcFormViewer` / `QfcFormViewer` / `QfcFormController` triple), plus in-family injectable-delegate
   seams for `EfcDataModel`, `EfcHomeController`, `EfcItemController`, the breadcrumb construction, the
   dialogs, and the settings singleton, plus a pure `EfcFormLayoutMath` module.
2. Split into **8 partial files** plus 2 new non-partial files; largest projected partial ~265 lines.
   **Every partial must independently clear 80%/75%** — none goes on the exemption ledger.
3. **Remove `[ExcludeFromCodeCoverage]` from `EfcFormController.cs:27`.** Keep it on
   `EfcViewer.cs:20`, which becomes the ratified irreducible remainder under `CLAUDE.md` §UT2 ground (b).
4. **No STA last-resort tests are required for this file.**
5. Migrate the existing `PopulateFolderCombobox_WhenFormViewerIsNull_...` test verbatim; preserve
   `CreateMinimalController()`.
6. ~126 named test cases across 8 test files plus a shared `EfcFormController.TestSupport.cs` harness.
7. Record #439 as a semantic-conflict risk; pin CURRENT behavior in all breadcrumb characterization tests.
8. Promote D1-D10 to GitHub issues via the MCP promotion lifecycle; fix none of them under F9.
9. Zero edits to `EfcHomeControllerDependencies.cs`, `EfcHomeControllerDependencyFactories.cs`,
   `EfcDataModel.cs`, `BreadcrumbBridgeRouter.cs`, `BreadcrumbOutboundQueue.cs`,
   `WebView2BreadcrumbHost.cs`, `IItemViewer.cs`, `EfcViewerQueue.cs`, `EfcThemeHelper.cs`, or
   `UtilitiesCS/Properties/AssemblyInfo.cs`.
