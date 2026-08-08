# EfcItemController.cs — Per-File Coverage Research (F9 / issue #452, epic #136)

- Target file: `QuickFiler/Controllers/EfcItemController.cs`
- Verified length: **1,170 lines** (last line `}` at 1170). Matches the epic manifest.
- Verified attribute: `[ExcludeFromCodeCoverage]` at **line 25**, immediately above
  `internal class EfcItemController : IItemControler` (line 26). File-level, type-scoped.
- Verified csproj entry: `QuickFiler/QuickFiler.csproj:301`
  (`<Compile Include="Controllers\EfcItemController.cs" />`).
- Verified baseline: the file does **not** appear in the committed Cobertura report
  `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`.
  `EfcHomeController.cs` **does** appear in the same report (positive control, line 9), proving the
  `Controllers/` folder is instrumented. The file is therefore **unmeasured, not covered**; the
  epic's "expect near zero" premise is confirmed by evidence, not assumed.
- Verified test-visibility: `QuickFiler/Properties/AssemblyInfo.cs:5` carries
  `[assembly: InternalsVisibleTo("QuickFiler.Test")]`, so the `internal` type and its `internal`
  members are directly reachable from `QuickFiler.Test`. No reflection is needed for `internal`
  members (only for `private` ones).

---

## 0. Constraint corrections (read before planning)

These are places where the delegation brief or the epic manifest is inaccurate for this file. Do
not build to the uncorrected premise.

### 0.1 `EfcViewerQueue.Dequeue()` is not a constraint on this file

The brief carries the F4 method-group rule forward. It is a real repo-wide rule, but
`EfcItemController.cs` **never references `EfcViewerQueue`**. The method-group consumption is in
sibling F8's `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:40` and `:112`
(`ProductionViewerFactory { get; set; } = EfcViewerQueue.Dequeue;`). F9's plan for
`EfcItemController.cs` inherits no obligation from it.

**However, F9 creates an equivalent obligation of its own** — see §3, seam S8: the recommended
theme-factory seam converts `EfcThemeHelper.SetupThemes` (F4-owned,
`QuickFiler/Helper Classes/EfcThemeHelper.cs:16`) to a delegate by method group. If F4 adds an
optional parameter to `SetupThemes`, that conversion breaks at compile time. This must be recorded
as a **new cross-child contract note to F4**, symmetric with the `EfcViewerQueue.Dequeue` rule.

### 0.2 `[STATestClass]` / `[STATestMethod]` are NOT available in `QuickFiler.Test`

The epic's STA last-resort clause (Shared Design §3) offers `[STATestClass]`/`[STATestMethod]`
"or equivalent runsettings scoping". Verified against `QuickFiler.Test/QuickFiler.Test.csproj`:
the project references `MSTest.TestFramework 4.3.3`, `MSTest.TestAdapter 4.3.3` and
`MSTest.Analyzers 4.3.3` only. **`MSTest.STAExtensions` is not referenced**, and
`<RunSettingsFilePath>` is empty (lines 29-30). `[STATestClass]` does not exist in this assembly.

The available equivalent is the manual dedicated-STA-thread helper already proven in-repo:
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:297-317`
(`StartRunningDispatcher()` — creates a `Dispatcher` on a background thread with
`SetApartmentState(ApartmentState.STA)` and `Dispatcher.Run()`), paired with
`ShutdownDispatcher()` at `:323`. Any F9 test needing STA must use that pattern in a dedicated
`EfcItemController.StaTests.cs`, not an attribute.

**Recommendation: F9 needs zero STA-bound tests.** See §4 — every member is reachable via seams.

### 0.3 `QuickFiler.Test.csproj` uses explicit `<Compile Include>` (no globbing)

Lines 57-169. Every new test file must be added to `QuickFiler.Test.csproj` as well as every new
production file being added to `QuickFiler.csproj`. Both are shared files under concurrent edit by
wave-1 siblings; the epic's CRLF-preservation and minimal-adjacent-hunk rules apply to both.

### 0.4 `UtilitiesCS.Threading.WpfUiDispatcher(Dispatcher)` is `internal` to `UtilitiesCS`

`UtilitiesCS/Threading/WpfUiDispatcher.cs:30` — the `Dispatcher`-taking constructor is `internal`.
`UtilitiesCS` grants `InternalsVisibleTo` to `DynamicProxyGenAssembly2`, `UtilitiesCS.Test` and
`ToDoModel.Test` only. QuickFiler production code can only use the public parameterless
`WpfUiDispatcher()` (line 24), which binds to the **static** `UiThread.Dispatcher`. This matters
for seam S10 (§3): F9 cannot wrap `IItemViewer.UiDispatcher` with the existing adapter and must
author a small local one.

### 0.5 `MailItemHelper` and `Theme` are not mockable, but are directly constructible

A planner might otherwise propose an unnecessary interface seam for either.

- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs:80` — `public MailItemHelper()`,
  parameterless, calls only `InitializeSafeDefaults()`. Every consumed property
  (`SenderName`, `Subject`, `Body`, `Triage`, `SentOn`, `Actionable`, `IsTaskFlagSet`, `SentDate`,
  `ToRecipientsName`, `Html`, `Item`) is publicly settable. Mixed `virtual`/non-`virtual`, so Moq
  cannot substitute it, but object-initializer construction is sufficient and COM-free.
- `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs:141` — `public Theme() { }`, and `:143`
  `public Theme(string name, Dictionary<string, ThemeControlGroup> controlGroups)`.
  `Theme.SetTheme()` (`:452`) and `SetTheme(bool)` (`:457`) only iterate `ControlGroups`, so a
  `Theme` built with an **empty** `ControlGroups` dictionary makes both calls deterministic no-ops.
  In-repo precedent: `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:166-192`.

### 0.6 The `_dataModel` dependency cannot be seamed by editing F5's file

`Controllers/EfcDataModel.cs` belongs to sibling F5. Its `ConversationResolver` setter is
`protected` (`EfcDataModel.cs:218`) and `MailInfo` is a computed non-virtual property
(`:232`). F9 must therefore introduce an F9-owned adapter (§3, seam S3), not retype or extend
`EfcDataModel`.

---

## 1. Structural map

`internal class EfcItemController : IItemControler` — one type, no nested types, no partials today.
`IItemControler` (`QuickFiler/Interfaces/IItemControler.cs:9-14`) requires only `CounterEnter`,
`CounterComboRight`, `RightKeyActions`.

Legend for the **Binding** column:
`PURE` = no host dependency · `COM` = `Microsoft.Office.Interop.Outlook.*` ·
`WF` = WinForms/BrightIdeasSoftware/WebView2 control access · `EVT` = event handler ·
`STATIC` = static-call boundary (`UiThread`, `EfcThemeHelper`, `CoreWebView2Environment`).

### Region: Constructors and Initializers (28-170)

| Lines | Member | Binding | Notes |
| --- | --- | --- | --- |
| 30-42 | `.ctor(globals, homeController, parent, itemViewer, dataModel, token)` | WF+STATIC | Chains to 59; sets `_dataModel`; calls `Initialize(async: true)` |
| 44-57 | `.ctor(..., dataModel, bool async, token)` | WF+STATIC | Chains to 59; calls `Initialize(async)` |
| 59-74 | `.ctor(globals, homeController, parent, itemViewer, token)` | PURE | Field assignment only. Reads `_homeController.KeyboardHandler`, `.ExplorerController` |
| 76-88 | `InitializeWithoutData()` | WF | `AdjustViewerForEfc` + `ResolveControlGroups` + tips toggles |
| 90-112 | `InitializeDataFields(EfcDataModel)` | WF+STATIC | `EfcThemeHelper.SetupThemes` static; `Task.Run(InitializeWebViewAsync)` |
| 114-165 | `Initialize(bool async)` | WF+STATIC | Same as 90-112 plus tips toggles; lines 115-134 are dead commented code |
| 167-169 | `logger` static field | PURE | log4net static initializer |

### Region: Item Setup and Disposal Methods (172-361)

| Lines | Member | Binding | Notes |
| --- | --- | --- | --- |
| 174-205 | `InitializeWebView()` | STATIC+WF | **Dead: zero callers repo-wide** (verified by grep) |
| 207-240 | `InitializeWebViewAsync()` | STATIC+WF | `CoreWebView2Environment.CreateAsync`; `await _itemViewer.UiSyncContext`; `TaskScheduler.FromCurrentSynchronizationContext()` |
| 242-253 | `AdjustViewerForEfc()` | WF | `RemoveControlsColsRightOf`, `LblItemNumber.Width`, `LblAcOpen.Width`, `L0vh_Tlp.GetColumn`, `ColumnStyles[n].Width -=` |
| 255-278 | `Cleanup()` | WF+EVT | Unhooks button hover + `_globals.Ol.PropertyChanged`; nulls 15 fields |
| 280-301 | `PopulateControls(EfcDataModel)` | WF | 6 label/textbox writes + `BtnFlagTask.DialogResult`; early-return when `MailInfo` null |
| 303-324 | `PopulateConversation()` | WF | Sets `ConversationResolver.UpdateUI`; reads `Count.SameFolder`; red backcolor when 0 |
| 326-352 | `ResolveControlGroups(ItemViewer)` | WF | `GetAllChildren()` extension; builds `_listTipsDetails`, `_itemPositionTips`, `_tableLayoutPanels`, `_buttons`, `_navCtrls`, `_tipsCtrls`, `_dflt2Ctrls`, `_mailCtrls` |
| 354-359 | `SetTopicThread(List<MailItemHelper>)` | WF | `TopicThread.SetObjects` + `.Sort(SentDate, Descending)` |

### Region: Private Fields and Variables (363-384)

19 private fields (365-382). `_selectorsCtrls` (381) is initialized to `null` and never assigned;
it is passed to `SetupThemes` as the `selectors` argument at line 97/144.

### Region: Exposed properties (386-638)

| Lines | Member | Binding | Notes |
| --- | --- | --- | --- |
| 389-390 | `ItemInfo` (internal get) | PURE | |
| 392-402 | `ActiveTheme` get/set | PURE-ish | `Initializer.GetOrLoad(..., strict: true, _themes)`; setter calls `_themes[x].SetTheme(async: true)` |
| 404-409 | `LoadTheme()` (internal) | PURE-ish | `DarkMode ? "DarkNormal" : "LightNormal"`; `_themes[key].SetTheme()` |
| 411-415 | `Buttons` | PURE | |
| 417-422 | `ConvOriginID` | PURE | |
| 424-429 | `CounterEnter` | PURE | `IItemControler` member |
| 431-436 | `CounterComboRight` | PURE | `IItemControler` member |
| 438-450 | `DarkMode` get/set | PURE-ish | `Initializer.GetOrLoad(ref _darkMode, () => _globals.Ol.DarkMode, false, _globals, _globals.Ol)` |
| 452-533 | (commented-out dead block) | — | 82 lines of commented `ConversationInfo`/`ConversationItems`/`DfConversation` |
| 535-538 | `Height` | WF | `_itemViewer.Height` |
| 540-544 | `IsExpanded` / `_expanded` | PURE | |
| 546-551 | `IsChild` | PURE | |
| 553-558 | `IsActiveUI` | PURE | |
| 560-564 | `ListTipsDetails` | PURE | |
| 566-570 | `Parent` | PURE | returns `EfcFormController`; **zero consumers repo-wide** |
| 572-581 | `ItemNumber` get/set | WF | setter writes `_itemViewer.LblItemNumber.Text` |
| 582-586 | `ItemIndex` | PURE | |
| 588-593 | `SelectedFolder` | WF | `_itemViewer.GetSelectedFolder()` |
| 595-598 | `Sender` | PURE | `_itemInfo.SenderName` |
| 600-603 | `SentDate` | PURE | `_itemInfo.SentDate.ToString("MM/dd/yyyy")` |
| 605-608 | `SentTime` | PURE | `_itemInfo.SentDate.ToString("HH:mm")` |
| 610-613 | `Subject` | WF | reads `_itemViewer.LblSubject.Text` (asymmetric with `Sender`, which reads `_itemInfo`) |
| 615-619 | `SuppressEvents` | PURE | |
| 621-624 | `To` | PURE | `_itemInfo.ToRecipientsName` |
| 626-629 | `TableLayoutPanels` | PURE | |
| 631-636 | `Token` | PURE | |

### Region: Event Wiring (640-737)

| Lines | Member | Binding | Notes |
| --- | --- | --- | --- |
| 642-678 | `WireEvents()` (internal) | WF+EVT | `ForAllControls(action, except)` extension over the concrete `Control`; 4 event subscriptions; button hover loop |
| 680-692 | `RegisterActions(Dictionary, bool)` (internal) | PURE | **Dead: zero callers repo-wide.** Also functionally broken — see D-2 |
| 694-719 | `RegisterAsyncFocusActions()` (internal) | PURE | Adds `'O'`, `'E'` to `CharActionsAsync`; `'B'`, `'D'` when `_expanded` |
| 721-730 | `UnregisterAsyncFocusActions()` (internal) | PURE | Symmetric removal |
| 732-735 | `UnregisterActions(List<char>)` (internal) | PURE | |

### Region: Event Handlers (739-834)

| Lines | Member | Binding | Notes |
| --- | --- | --- | --- |
| 741-755 | `ConversationResolverPropertyChanged` | EVT+WF | `async void`. Guard compares to `"Expanded"` — never raised (see D-1) |
| 757-768 | `TopicThread_ItemSelectionChanged` (private) | EVT+WF | Reads `SelectedObjects[0] as MailItemHelper`, navigates WebView |
| 770-799 | `WebView2Control_CoreWebView2InitializationCompleted` (internal) | EVT+WF | Rethrows `e.InitializationException`; sets `_isWebViewerInitialized`; dark/light navigate; hides WebView |
| 801-815 | `DarkMode_Changed` (internal) | EVT | Guard `e.PropertyName == nameof(_globals.Ol.DarkMode)` = `"DarkMode"`; sets `ActiveTheme` |
| 817-820 | `Button_MouseEnter` (private) | EVT+WF | `((Button)sender).BackColor = _themes[_activeTheme].ButtonMouseOverColor` |
| 822-832 | `Button_MouseLeave` (private) | EVT+WF | Branches on `DialogResult == OK` |

### Region: UI Navigation Methods (836-1079)

| Lines | Member | Binding | Notes |
| --- | --- | --- | --- |
| 838-848 | `ToggleExpansion()` | WF | Dispatches on `_expanded` |
| 850-860 | `ToggleExpansionAsync()` | WF | Dispatches on `_expanded` |
| 862-905 | `ToggleExpansion(ToggleState)` | WF | `_parent.ToggleExpansionStyle`; TLP column widths; `System.Threading.Timer` at 4000 ms; registers/removes `'B'`/`'D'` in `CharActions` |
| 907-929 | `ToggleExpansionAsync(ToggleState)` | WF | `_itemViewer.UiDispatcher.InvokeAsync(ToggleExpansionOn/Off)` |
| 931-942 | `ToggleExpansionOff()` (private) | WF | No keyboard unregistration (see D-3) |
| 944-956 | `ToggleExpansionOn()` (private) | WF | No keyboard registration (see D-3) |
| 958-979 | `ToggleNavigation(bool)` | WF | Toggles `_activeUI`, calls `ToggleTips(async)` |
| 981-994 | `ToggleNavigation(bool, ToggleState)` | WF | **Consumed by `EfcFormController.cs:929, :945`** |
| 996-1009 | `ToggleNavigationAsync(ToggleState)` | PURE-ish | **Consumed by `EfcFormController.cs:938, :954`** |
| 1011-1024 | `ToggleTips(bool)` | WF | `_itemViewer.BeginInvoke` / `.Invoke` |
| 1026-1043 | `ToggleTips(bool, ToggleState)` | WF | Same |
| 1045-1061 | `ToggleTipsAsync(ToggleState)` | PURE-ish | `Token.ThrowIfCancellationRequested()`; `Task.WhenAll` over `IQfcTipsDetails.ToggleAsync` |
| 1063-1070 | `ToggleSaveAttachments()` | STATIC+WF | `UiThread.Dispatcher.Invoke(...)` |
| 1072-1077 | `ToggleSaveCopyOfMail()` | STATIC+WF | `UiThread.Dispatcher.Invoke(...)` |

### Region: UI Visual Helper Methods (1081-1140)

| Lines | Member | Binding | Notes |
| --- | --- | --- | --- |
| 1083-1096 | `SetThemeDark(bool)` | PURE-ish | 2-way branch on `_activeTheme is null \|\| Contains("Normal")` |
| 1098-1108 | `HtmlDarkConverter(ToggleState)` | WF | Guarded by `_isWebViewerInitialized` |
| 1110-1123 | `SetThemeLight(bool)` | PURE-ish | Mirror of `SetThemeDark` |
| 1125-1129 | `ApplyReadEmailFormat(object state)` | COM-adjacent | `_itemInfo.UnRead = false` writes `Item.UnRead` + `Item.Save()`; then `ControlGroups["MailRelated"].ApplyTheme(async: true)` |
| 1131-1138 | `SetOlvTheme(IList<object>, Color, Color)` | WF | `HeaderFormatStyle` + `OLVColumn.HeaderFormatStyle` |

### Region: UI Keyboard Methods (1142-1168)

| Lines | Member | Binding | Notes |
| --- | --- | --- | --- |
| 1144-1148 | `KbdExecuteAsync(Func<Task>)` | PURE-ish | `_homeController.KeyboardHandler.ToggleKeyboardDialogAsync()` then `action()` |
| 1150-1155 | `JumpToAsync(Control)` (internal) | WF | Toggle dialog, `await _itemViewer.UiSyncContext`, `control.Focus()` |
| 1157-1166 | `RightKeyActions` | PURE | Returns a 1-entry dictionary `{"&Cancel", () => {}}` |

### External contract surface (what actually consumes this type)

Verified by grep across `QuickFiler/`:

- `EfcFormController.cs:69` and `:87` construct it (two of the three constructors; the
  `(…, dataModel, bool async, token)` overload at line 44 has **zero call sites**).
- `EfcFormController.cs:107` → `InitializeWithoutData()`
- `EfcFormController.cs:116` → `InitializeDataFields(dataModel)`
- `EfcFormController.cs:929, :945` → `ToggleNavigation(bool, ToggleState)`
- `EfcFormController.cs:938, :954` → `ToggleNavigationAsync(ToggleState)`
- Reverse direction: `EfcItemController.cs:864` and `:909` → `_parent.ToggleExpansionStyle(state)`
  (`EfcFormController.cs:1056`) — the **only** member of the parent it uses.

Everything else on the type is reached only via keyboard-action delegates, WinForms events, or is
outright dead. That narrow contract is what makes the seam plan in §3 cheap.

---

## 2. Partial-split proposal (500-line rule)

Repo convention confirmed by `QfcItemController.{Initialization,ViewerSetup,Conversation,
FolderHandling,EventWiring,EventHandlers,Navigation,FocusAndTheme,MailActions}.cs` and
`EfcHomeController.{Metrics,ExecuteMoves,Timing}.cs`. Split is by **concern**, matching the
existing `#region` boundaries so the diff is a near-pure move.

Overhead assumption: each partial carries ~20 `using` directives + `namespace` + `partial class`
declaration + closing braces ≈ 26 lines. New seam fields add ~35 lines to the primary partial.

| # | File | Contents (source line ranges) | Projected lines |
| --- | --- | --- | --- |
| 1 | `EfcItemController.cs` | class decl, `logger` (167-169), private fields (363-384) + new seam fields, three ctors (30-74), `InitializeWithoutData` (76-88), `InitializeDataFields` (90-112), `Initialize` (114-165), `Cleanup` (255-278) | **~250** |
| 2 | `EfcItemController.Properties.cs` | Exposed-properties region (386-638) minus `LoadTheme` (moved to #8) | **~275** |
| 3 | `EfcItemController.ViewerSetup.cs` | `AdjustViewerForEfc` (242-253), `ResolveControlGroups` (326-352), `PopulateControls` (280-301), `PopulateConversation` (303-324), `SetTopicThread` (354-359) | **~125** |
| 4 | `EfcItemController.WebView.cs` | `InitializeWebView` (174-205), `InitializeWebViewAsync` (207-240), `WebView2Control_CoreWebView2InitializationCompleted` (770-799), `HtmlDarkConverter` (1098-1108) | **~135** |
| 5 | `EfcItemController.EventWiring.cs` | `WireEvents` (642-678), `RegisterActions` (680-692), `RegisterAsyncFocusActions` (694-719), `UnregisterAsyncFocusActions` (721-730), `UnregisterActions` (732-735), `KbdExecuteAsync` (1144-1148), `JumpToAsync` (1150-1155), `RightKeyActions` (1157-1166) | **~155** |
| 6 | `EfcItemController.EventHandlers.cs` | `ConversationResolverPropertyChanged` (741-755), `TopicThread_ItemSelectionChanged` (757-768), `DarkMode_Changed` (801-815), `Button_MouseEnter` (817-820), `Button_MouseLeave` (822-832) | **~95** |
| 7 | `EfcItemController.Navigation.cs` | Whole UI-Navigation region (836-1077) | **~270** |
| 8 | `EfcItemController.Theme.cs` | `LoadTheme` (404-409), `SetThemeDark` (1083-1096), `SetThemeLight` (1110-1123), `ApplyReadEmailFormat` (1125-1129), `SetOlvTheme` (1131-1138) | **~95** |

Total ≈ 1,400 lines across 8 files (the growth over 1,170 is the 8× per-file scaffolding plus the
new seam fields). Largest file ≈ 275 lines, comfortably under 500.

**Optional (call it out in the plan, do not do it silently):** dropping the 82-line commented-out
block at 452-533 would take file #2 to ~195 lines. That is a deletion of dead commented code, not a
behavior change, but it does make the move-diff impure. Recommend keeping it in the split commit and
removing it — if at all — in a separate, clearly-labelled task.

**New production files also required by §3** (each needs a `<Compile Include>` entry in
`QuickFiler.csproj` and a ledger row per epic "Mid-Wave File Creation" rule 3):

| File | Contents | Projected lines | Ledger bucket |
| --- | --- | --- | --- |
| `Controllers/EfcItemControllerDependencies.cs` | injectable seam bundle + production defaults | ~150 | `testable` (>= 90%) |
| `Interfaces/IEfcExpansionStyleHost.cs` | 1-member interface | ~15 | `interface-only / not-measured` |
| `Interfaces/IEfcItemDataSource.cs` | 3-member interface | ~20 | `interface-only / not-measured` |
| `Interfaces/IEfcItemControlSurface.cs` | ~14-member interface | ~55 | `interface-only / not-measured` |
| `Controllers/EfcDataModelSource.cs` | adapter over `EfcDataModel` | ~45 | `testable` (>= 90%) |
| `Viewers/EfcItemControlSurface.cs` | adapter over concrete `ItemViewer` | ~120 | `ratified-exempt` (see §4.4) |
| `Viewers/ItemViewerUiDispatcher.cs` | `IUiDispatcher` over `IItemViewer.UiDispatcher` | ~60 | `testable` (>= 90%) |

Note for F1's ledger: `Interfaces/IItemControler.cs` is already assigned to F3 (epic F3 list), so
F9's new interface files go under `Interfaces/` but are F9-owned and F9-registered.

---

## 3. Seam plan

Hierarchy per `.claude/rules/csharp.md` §DI Seams: **interface seam > injectable delegate >
adapter**. Every seam below states the exact production default so no path can leave a seam null.

### S1 — `_itemViewer`: `ItemViewer` → `IItemViewer` (interface seam, already exists)

`QuickFiler/Viewers/IItemViewer.cs` already declares intent members that are **verified 1:1
forwards** to exactly the concrete members this controller uses:

| Current concrete access (line) | `IItemViewer` member | Forward proof |
| --- | --- | --- |
| `LblSender.Text` (287) | `SenderText` | `ItemViewer.DisplayState.cs:13-17` |
| `LblSubject.Text` (288, 612) | `SubjectText` | `:19-23` |
| `TxtboxBody.Text` (289) | `BodyText` | `:25-29` |
| `LblTriage.Text` (290) | `TriageText` | `:31-35` |
| `LblSentOn.Text` (291) | `SentOnText` | `:37-41` |
| `LblActionable.Text` (292) | `ActionableText` | `:43-47` |
| `LblItemNumber.Text` (579) | `ItemNumberText` | `:49-53` |
| `LblConvCt.Text` (316) | `ConversationCountText` | `:61-65` |
| `LblConvCt.BackColor` (319) | `ConversationCountBackColor` | `:67-71` |
| `BtnFlagTask.DialogResult` (295, 299) | `FlagTaskDialogResult` | `ItemViewer.Commands.cs:97-101` |
| `SaveAttachmentsMenuItem.Checked` (1066-1068) | `AttachmentsChecked` | `:79-83` |
| `SaveEmailMenuItem.Checked` (1075) | `EmailCopyChecked` | `:67-71` |
| `TopicThread.SetObjects(...)` (357, 750) | `SetConversationItems(IList)` | `ItemViewer.WebViewThread.cs:23` |
| `TopicThread.Sort(SentDate, Desc)` (358, 753) | `SortConversationByDate(SortOrder)` | `:25` |
| `TopicThread.SelectedObjects` (762) | `GetSelectedConversationItems()` | `:27` |
| `TopicThread.ItemSelectionChanged +=` (670) | `ConversationItemSelectionChanged` | `:29-32` |
| `L0v2h2_WebView2.NavigateToString(...)` (766, 787, 793, 1102) | `NavigateToString(string)` | `:15` |
| `L0v2h2_WebView2.CoreWebView2InitializationCompleted +=` (664) | `WebViewInitializationCompleted` | `:17-21` |

Plus `LeftTipsLabels`, `UiSyncContext`, `UiDispatcher`, `Height`, `GetSelectedFolder()`,
`RemoveControlsColsRightOf(Control)`, `Invoke`, `BeginInvoke` — all already on `IItemViewer`.

**Changes:** the three constructor parameters `ItemViewer itemViewer` → `IItemViewer itemViewer`;
the field `private ItemViewer _itemViewer` → `private IItemViewer _itemViewer`; the
`ResolveControlGroups(ItemViewer itemViewer)` parameter → `IItemViewer`. No call-site change:
`EfcFormController._itemViewer` is a concrete `ItemViewer`, which implements `IItemViewer`
(`ItemViewer.cs:21`), so the two `new EfcItemController(...)` sites at `EfcFormController.cs:69`
and `:87` compile unchanged.

Direct in-repo precedent: `QfcItemController.cs:51` already declares `private IItemViewer
_itemViewer`.

### S2 — `_parent`: `EfcFormController` → `IEfcExpansionStyleHost` (interface seam, new)

New F9-owned file `QuickFiler/Interfaces/IEfcExpansionStyleHost.cs`:

```
internal interface IEfcExpansionStyleHost
{
    void ToggleExpansionStyle(UtilitiesCS.Enums.ToggleState desiredState);
}
```

`EfcFormController` already has `public void ToggleExpansionStyle(Enums.ToggleState)`
(`EfcFormController.cs:1056`), so adding the interface to its declaration
(`internal class EfcFormController : IFilerFormController, IEfcExpansionStyleHost`) requires no new
member. `EfcFormController.cs` is F9-owned, so this is an intra-child change, not a cross-child one.

**Changes:** field type; three constructor parameter types; the public `Parent` property (566-570)
return type. `Parent` has **zero consumers** repo-wide, so the retype is safe.

### S3 — `_dataModel`: adapter over `EfcDataModel` (adapter seam, new — F5 file is off-limits)

New `QuickFiler/Interfaces/IEfcItemDataSource.cs`:

```
internal interface IEfcItemDataSource
{
    UtilitiesCS.MailItemHelper MailInfo { get; }
    QuickFiler.Helper_Classes.IConversationResolver ConversationResolver { get; }
    Microsoft.Office.Interop.Outlook.MailItem Mail { get; }
}
```

Those three members are the complete consumed surface: `MailInfo` (282), `ConversationResolver`
(311, 314, 315, 666, 667, 1103), `Mail.UnRead` (99, 146). `IConversationResolver`
(`QuickFiler/Helper Classes/IConversationResolver.cs:12-32`) already declares every member used —
`UpdateUI` (`:18`), `Count` as `Pair<int>` (`:16`), `ConversationInfo` as
`Pair<List<MailItemHelper>>` (`:14`), and `event PropertyChanged` (`:22`). `Pair<T>` is a
`public struct` with a `(sameFolder, expanded)` constructor at
`QuickFiler/Helper Classes/ConversationResolver.cs:18` — constructible in tests.

New `QuickFiler/Controllers/EfcDataModelSource.cs` — a 3-property pass-through over an
`EfcDataModel`. This adapter **is** testable: `new EfcDataModel(globalsMock, mail: null,
new CancellationTokenSource(), CancellationToken.None)` leaves `Mail` null and
`ConversationResolver` null because `EfcDataModel.TryGetFirstInSelection` (`:234-252`) swallows the
exception from a null `Ol.App`. One test covers all three properties. **No exemption.**

**Changes:** field `private EfcDataModel _dataModel` → `private IEfcItemDataSource _data`. The two
`EfcDataModel`-typed public entry points keep their signatures — `InitializeDataFields(EfcDataModel
dataModel)` (90) wraps internally (`_data = new EfcDataModelSource(dataModel)`), preserving the
`EfcFormController.cs:116` call site. Add one **new internal overload**
`InitializeDataFields(IEfcItemDataSource)` for tests, and one **new internal constructor** taking
`IEfcItemDataSource`. Use explicit overloads, never optional parameters (repo rule, §0.1).

### S4 — `EfcThemeHelper.SetupThemes`: injectable delegate seam

`EfcThemeHelper` is a `static class` in F4's `Helper Classes/EfcThemeHelper.cs:14`. Declare an
F9-owned delegate in `EfcItemControllerDependencies.cs` mirroring the 10-parameter signature at
`EfcThemeHelper.cs:16-27`, with the production default `EfcThemeHelper.SetupThemes` (method group).

```
internal delegate System.Collections.Generic.Dictionary<string, UtilitiesCS.Theme> EfcThemeFactory(
    IList<Control> nav, IList<Control> tips, IList<Control> dflt2, IList<Control> selectors,
    IList<Control> mail, Func<bool> isAlt, IList<object> olvColumns,
    Action<IList<object>, Color, Color> olvSetter,
    Microsoft.Web.WebView2.WinForms.WebView2 webView2, Action<Enums.ToggleState> htmlConverter);
```

**Record the cross-child note to F4:** the method-group default forbids adding an optional
parameter to `EfcThemeHelper.SetupThemes`; a new overload is required instead.

### S5 — `IWebViewCoreInitializer` (interface seam, already exists)

`QuickFiler/Viewers/IWebViewCoreInitializer.cs:13-29` already abstracts the two calls
`InitializeWebViewAsync` makes: `CreateEnvironmentAsync(cacheFolder, options)` and
`EnsureCoreWebView2Async(control, environment)`. Production default
`new QuickFiler.Viewers.WebView2CoreInitializer()`. Precedent: `QfcItemController.cs:67` +
`QfcItemController.Initialization.cs:381`. F13 owns those two files; F9 consumes, does not edit.

### S6 — `UiThread.Dispatcher` (static) → `UtilitiesCS.Threading.IUiDispatcher`

Used at `ToggleSaveAttachments` (1065) and `ToggleSaveCopyOfMail` (1074). Add field
`private UtilitiesCS.Threading.IUiDispatcher _uiDispatcher;`, production default
`new UtilitiesCS.Threading.WpfUiDispatcher()` (the public parameterless ctor, which forwards to the
same static `UiThread.Dispatcher` — behavior-identical). Precedent: `QfcItemController.cs:66` and
`QfcItemController.FocusAndTheme.cs:270`.

### S7 — `_itemViewer.UiDispatcher` (sealed WPF `Dispatcher`) → local `IUiDispatcher` adapter

`ToggleExpansionAsync(ToggleState)` (913, 922) awaits `_itemViewer.UiDispatcher.InvokeAsync(...)`.
`System.Windows.Threading.Dispatcher` is sealed and unmockable, and per §0.4 the existing
`WpfUiDispatcher(Dispatcher)` ctor is `internal` to `UtilitiesCS`.

New F9-owned `QuickFiler/Viewers/ItemViewerUiDispatcher.cs`:
`internal sealed class ItemViewerUiDispatcher : IUiDispatcher` holding an `IItemViewer` and
forwarding each member to `viewer.UiDispatcher`. Field
`private IUiDispatcher _viewerDispatcher;` defaulted to `new ItemViewerUiDispatcher(_itemViewer)`.

**Do not** reuse `_uiDispatcher` (S6) here: `UiThread.Dispatcher` and `IItemViewer.UiDispatcher`
are not provably the same instance, and substituting one for the other would be a behavior change
under the F9 no-behavior-change NFR.

`ItemViewerUiDispatcher` is coverable using the running-STA-dispatcher precedent
(`QfcItemController.TestSupport.cs:297-317`) plus a `Mock<IItemViewer>` returning it. In-repo
precedent for testing an `IUiDispatcher` adapter this way already exists:
`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`.

### S8 — `Task.Run(() => InitializeWebViewAsync())` → injectable background-start delegate

Lines 110 and 164 launch unobservable fire-and-forget work, which makes `Initialize` and
`InitializeDataFields` nondeterministic in a test. Add
`private Func<Func<Task>, Task> _backgroundRunner;`, production default `f => Task.Run(f)`.
Tests inject a recorder that captures the delegate without running it.

### S9 — `IEfcItemControlSurface` (adapter seam, new) — the residual raw-control access

The operations below have no `IItemViewer` equivalent and cannot be expressed through it. They go
behind one F9-owned interface so the *arithmetic and branching stay in the testable controller* and
only property reads/writes move into the exempt adapter.

`QuickFiler/Interfaces/IEfcItemControlSurface.cs`:

```
internal interface IEfcItemControlSurface
{
    // AdjustViewerForEfc (242-253): primitives only; the width arithmetic stays in the controller.
    int ItemNumberWidth { get; }
    int OpenActionWidth { get; }
    int GetOpenActionColumnIndex();
    void ReduceColumnWidth(int columnIndex, float delta);

    // ResolveControlGroups (326-352): pre-grouped control lists, so no control-typed
    // member access remains in the controller.
    IList<Control> GetAllChildren();
    Control ItemNumberControl { get; }        // -> _navCtrls
    IList<Control> DefaultColorControls { get; }   // -> _dflt2Ctrls (L0vh_Tlp, TxtboxBody, TopicThread)
    IList<Control> MailControls { get; }      // -> _mailCtrls (LblSender, LblSubject)

    // WireEvents (642-678): ForAllControls(this Control, action, except) is an extension on
    // Control (UtilitiesCS/Extensions/WinFormsExtensions.cs:57); the exclusion set
    // (L0vhBreadcrumb_WebView2, TxtboxSearch, TopicThread) is owned by the adapter.
    void ForEachKeyboardControl(Action<Control> action);

    // ToggleExpansionOn/Off + ToggleExpansion(state) (862-956)
    void SetBodyToggleColumnWidths(float leftWidth, float rightWidth);
    bool ConversationVisible { set; }
    bool BodyWebViewVisible { set; }

    // SetupThemes (97, 149) + InitializeWebViewAsync (201, 236)
    Microsoft.Web.WebView2.WinForms.WebView2 BodyWebView { get; }
    IList<object> ConversationColumns { get; }   // -> TopicThread.Columns.Cast<object>()
}
```

`QuickFiler/Viewers/EfcItemControlSurface.cs` implements it over the concrete `ItemViewer`.
Production default: `new EfcItemControlSurface((ItemViewer)_itemViewer)` when the injected viewer is
a concrete `ItemViewer`. Because the surface is injected, a `Mock<IEfcItemControlSurface>` makes
`AdjustViewerForEfc`, `ResolveControlGroups`, `WireEvents`, `ToggleExpansionOn/Off` and
`InitializeWebViewAsync` routing-testable — this is strictly better than the F10 precedent, which
left `((ItemViewer)_itemViewer)` casts in place and kept method-level
`[ExcludeFromCodeCoverage]` on `QfcItemController.ViewerSetup.cs:38` and `:132`.

### S10 — Dependency bundle

Mirror the shape (not the file) of F8's `EfcHomeControllerDependencies`: a single F9-owned
`EfcItemControllerDependencies` class whose constructor takes every seam as an optional argument
with a `?? production-default` fallback, exposed as get-only properties. `EfcItemController` gets
one new internal constructor overload accepting an `EfcItemControllerDependencies` and applies
defaults when it is null.

**Hard constraint respected:** `EfcHomeControllerDependencies.cs` and
`EfcHomeControllerDependencyFactories.cs` are **not modified**. They are read-only inputs. Note
that neither of them mentions `EfcItemController` — F8's factory graph stops at
`EfcFormController`, which constructs the item controller itself
(`EfcFormController.cs:69, :87`). F9's bundle is therefore additive and non-overlapping.

### Dependencies that need no seam (already interfaces / already constructible)

| Dependency | Status |
| --- | --- |
| `IApplicationGlobals` / `IOlObjects` | Interface. `IOlObjects : INotifyPropertyChanged` with `bool DarkMode { get; set; }` (`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:11, :30`). `Mock.Raise` drives `DarkMode_Changed`. |
| `IFilerHomeController` | Interface (`QuickFiler/Interfaces/IFilerHomeController.cs:11`); only `KeyboardHandler` and `ExplorerController` used. |
| `IQfcKeyboardHandler` | Interface; `CharActions`/`CharActionsAsync` are concrete `KbdActions<>` with a public parameterless ctor (`KbdActions.cs:21`). |
| `IQfcExplorerController` | Interface; only `OpenQFItem(MailItem)` used. |
| `IQfcTipsDetails` | Interface (`UtilitiesCS/Interfaces/IQuickFiler/IQfcTipsDetails.cs:7`). `_listTipsDetails` and `_itemPositionTips` are already interface-typed fields — inject via reflection. |
| `MailItemHelper` | Public parameterless ctor; see §0.5. |
| `Theme` / `ThemeControlGroup` | Public ctors; see §0.5. For `ApplyReadEmailFormat`, build the `"MailRelated"` group with the **object-setter** ctor (`ThemeControlGroup.cs:102-114`) so `_controls` stays null and `ApplyTheme(bool)` (`:212-229`) takes the `else` branch, bypassing the static `UiThread.Dispatcher` entirely. |
| `Initializer` (static) | Pure static helper, no host dependency. Semantics verified: `GetOrLoad(ref, loader, strict, deps)` at `Initializer.cs:124`; `DependenciesNotNull` at `:290-324` **throws `ArgumentNullException` when `strict: true` and a dependency is null**, and returns `false` (→ `default(T)`) when `strict: false`. |

---

## 4. Per-member testability verdict

Verdicts assume the full §3 seam set is in place. **T** = testable-after-seam ·
**STA** = needs the STA last-resort clause · **IRR** = irreducible-remainder candidate.

### 4.1 Testable after seam (the overwhelming majority)

| Member | Verdict | Reaching mechanism |
| --- | --- | --- |
| `.ctor(globals, home, parent, viewer, token)` (59-74) | T | Direct construction with mocks |
| `.ctor(..., dataModel, token)` (30-42) | T | Construct with a dependency bundle whose theme factory, background runner and control surface are mocks |
| `.ctor(..., dataModel, async, token)` (44-57) | T | Same. Note: zero production call sites |
| `InitializeWithoutData` (76-88) | T | `Mock<IEfcItemControlSurface>` + injected `_listTipsDetails` / `_itemPositionTips` |
| `InitializeDataFields` (90-112) | T | Mocked theme factory + background runner |
| `Initialize(bool)` (114-165) | T | Same |
| `InitializeWebViewAsync` (207-240) | T | `Mock<IWebViewCoreInitializer>` + `Mock<IItemViewer>.UiSyncContext` returning `new SynchronizationContext()` + `IEfcItemControlSurface.BodyWebView` returning null |
| `AdjustViewerForEfc` (242-253) | T | Surface mock returns widths/column index; assert `ReduceColumnWidth` called with the computed delta |
| `Cleanup` (255-278) | T | Assert `PropertyChanged` unsubscribed and fields nulled (reflection read-back) |
| `PopulateControls` (280-301) | T | `Mock<IItemViewer>` verify on the 6 intent setters + `FlagTaskDialogResult` |
| `PopulateConversation` (303-324) | T | `Mock<IConversationResolver>` with `Count = new Pair<int>(…)` |
| `ResolveControlGroups` (326-352) | T | Surface mock returns handle-less `Label`/`Button`/`TableLayoutPanel` lists |
| `SetTopicThread` (354-359) | T | `Mock<IItemViewer>` verify |
| `ItemInfo`, `Buttons`, `ConvOriginID`, `CounterEnter`, `CounterComboRight`, `IsExpanded`, `IsChild`, `IsActiveUI`, `ListTipsDetails`, `Parent`, `ItemIndex`, `Sender`, `SentDate`, `SentTime`, `SuppressEvents`, `To`, `TableLayoutPanels`, `Token` | T | Plain property round-trips |
| `ActiveTheme` get/set (392-402) | T | Injected `_themes`; empty-`ControlGroups` `Theme` |
| `LoadTheme` (404-409) | T | Injected `_themes` + `_globals` |
| `DarkMode` get/set (438-450) | T | `Mock<IOlObjects>.DarkMode` |
| `Height` (535-538) | T | `Mock<IItemViewer>.Height` |
| `ItemNumber` set (572-581) | T | `Mock<IItemViewer>.ItemNumberText` |
| `SelectedFolder` (588-593) | T | `Mock<IItemViewer>.GetSelectedFolder()` |
| `Subject` (610-613) | T | `Mock<IItemViewer>.SubjectText` |
| `WireEvents` (642-678) | T | Surface mock's `ForEachKeyboardControl` invoked with handle-less controls |
| `RegisterActions` (680-692) | T | Real `KbdActions<char, KaChar, Action<char>>` |
| `RegisterAsyncFocusActions` (694-719) | T | Real `KbdActions<>`; `_expanded` via reflection |
| `UnregisterAsyncFocusActions` (721-730) | T | Same |
| `UnregisterActions` (732-735) | T | Same |
| `ConversationResolverPropertyChanged` (741-755) | T | Invoke directly with a `PropertyChangedEventArgs("Expanded")` |
| `TopicThread_ItemSelectionChanged` (757-768) | T | `Mock<IItemViewer>.GetSelectedConversationItems()`; private → reflection or raise via the mock's event |
| `WebView2Control_CoreWebView2InitializationCompleted` (770-799) | T | `CoreWebView2InitializationCompletedEventArgs` — see §4.3 |
| `DarkMode_Changed` (801-815) | T | Direct invoke or `Mock<IOlObjects>.Raise` |
| `Button_MouseEnter` / `Button_MouseLeave` (817-832) | T | Handle-less `new Button()` as sender + injected `_themes`; precedent `QfcItemController.TestSupport.cs:166` |
| `ToggleExpansion()` / `ToggleExpansionAsync()` (838-860) | T | `_expanded` via reflection; assert the delegated overload's effects |
| `ToggleExpansion(ToggleState)` (862-905) | T | Surface mock + `Mock<IEfcExpansionStyleHost>` + real `KbdActions<>`. Note: creates a real `System.Threading.Timer` with a 4 s due time when `_itemInfo.UnRead` — the test asserts the field is non-null and disposes it; it never waits |
| `ToggleExpansionAsync(ToggleState)` (907-929) | T | `Mock<IUiDispatcher>` executing the action synchronously; precedent `QfcItemControllerTestSupport.BuildSyncDispatcher` (`:102-137`) |
| `ToggleExpansionOff` / `ToggleExpansionOn` (931-956) | T | Private → reflection; surface mock |
| `ToggleNavigation(bool)` (958-979) | T | |
| `ToggleNavigation(bool, ToggleState)` (981-994) | T | |
| `ToggleNavigationAsync(ToggleState)` (996-1009) | T | |
| `ToggleTips(bool)` / `ToggleTips(bool, ToggleState)` (1011-1043) | T | `Mock<IItemViewer>.Invoke/BeginInvoke` with a callback that runs the delegate — precedent `IItemViewer.cs:124-129` declares these for exactly this reason |
| `ToggleTipsAsync(ToggleState)` (1045-1061) | T | `Mock<IQfcTipsDetails>.ToggleAsync` returning `Task.CompletedTask`; cancelled-token path is a real error scenario |
| `ToggleSaveAttachments` / `ToggleSaveCopyOfMail` (1063-1077) | T | `Mock<IUiDispatcher>` (S6) + `Mock<IItemViewer>.AttachmentsChecked` / `.EmailCopyChecked` |
| `SetThemeDark` / `SetThemeLight` (1083-1123) | T | Injected `_themes` with empty `ControlGroups` |
| `HtmlDarkConverter` (1098-1108) | T | `_isWebViewerInitialized` via reflection; real `MailItemHelper` |
| `ApplyReadEmailFormat` (1125-1129) | T | `MailItemHelper { Item = Mock<MailItem>.Object }` (the `Item` property is `virtual`, `MailItemHelper.Properties.cs:92`); `"MailRelated"` group built with the object-setter ctor |
| `SetOlvTheme` (1131-1138) | T | Real `OLVColumn` instances — plain objects from ObjectListView, no handle required |
| `KbdExecuteAsync` (1144-1148) | T | `Mock<IQfcKeyboardHandler>.ToggleKeyboardDialogAsync()` |
| `JumpToAsync(Control)` (1150-1155) | T | Handle-less `new Button()`; `Focus()` on a handle-less control returns `false` without throwing |
| `RightKeyActions` (1157-1166) | T | Pure |

### 4.2 STA last-resort clause required

**None.** Every member above is reachable through a seam or a handle-less control. Constructing
handle-less `Label`, `Button`, `TextBox` and `TableLayoutPanel` instances is already done in plain
(non-STA) `[TestClass]` files in this repo — `QfcItemController.TestSupport.cs:209` does
`new Label()` inside `BuildDispatchableTheme`. No `EfcItemController.StaTests.cs` is needed.

The one place STA infrastructure is used is `ItemViewerUiDispatcher`'s own test (§3 S7), which
needs a *running* dispatcher on an STA thread. That is dispatcher infrastructure, not a WinForms
control instantiation, and it reuses the existing `StartRunningDispatcher()` helper.

### 4.3 Members needing a construction workaround (still testable, flag in the plan)

`WebView2Control_CoreWebView2InitializationCompleted(object, CoreWebView2InitializationCompletedEventArgs)`
(770-799). `CoreWebView2InitializationCompletedEventArgs` is a WebView2 SDK type with no public
constructor. The plan must verify one of these two before committing the test tasks:

1. `System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(...))` plus
   reflection-set of the `IsSuccess`/`InitializationException` backing fields; or
2. extract the handler body into an internal, parameterless-arg method
   `internal void OnWebViewInitialized(bool isSuccess, Exception initializationException)` and
   leave the event-shaped handler as a two-line forward.

**Option 2 is recommended** — it is a smaller and more honest seam, matches
"leave only the thinnest possible wiring in the host-bound entry point"
(`.claude/rules/general-unit-test.md` § Coverage Exclusion Policy), and needs no reflection into a
third-party SDK type. The two-line forwarding shim then costs ~2 uncovered lines, not a
method-level exemption.

### 4.4 Irreducible-remainder candidates

Exactly **one** production file, and **zero methods on `EfcItemController` itself**.

| Artifact | Ground | Rationale against the epic's irreducible-remainder standard |
| --- | --- | --- |
| `Viewers/EfcItemControlSurface.cs` (new adapter, §3 S9) | CLAUDE.md §UT2 (b) WinForms-bound, and (c) direct dependency on a form-derived type | Every member is a one-line forward to a member of the concrete `ItemViewer`, which is itself `[ExcludeFromCodeCoverage]` (`ItemViewer.cs:20`) and F14-owned. Exercising even one forward requires constructing a real `ItemViewer`, whose constructor runs `InitializeComponent()` over a 6,224-line Designer file that instantiates a `Microsoft.Web.WebView2.WinForms.WebView2` control (`ItemViewerExpanded.Designer.cs:44`) — that pulls in the WebView2 native loader, which is an external-process dependency and therefore prohibited by `.claude/rules/general-unit-test.md` § External Dependencies, independent of the STA question. The adapter contains **no branching, no arithmetic and no state**; all decision logic it might otherwise have absorbed was deliberately kept in the controller (see the "primitives only" comments in the interface). Direct in-repo precedent for exempting a pure forwarding adapter: `WebView2CoreInitializer.cs:15` (`[ExcludeFromCodeCoverage]`, with the same stated rationale). |

**Explicitly NOT irreducible** (do not let the plan drift into exempting these):

- `InitializeWebView()` (174-205) — dead code, zero callers. Do **not** exempt it. Either seam it
  through `IWebViewCoreInitializer` like its async twin (cost: ~10 lines) or delete it as a
  no-behavior-change removal of an uncalled `internal` method. Recommend deletion, called out as
  its own atomic task so it is reviewable.
- `InitializeWebViewAsync()` (207-240) — F10 exempted its analogue
  (`QfcItemController.ViewerSetup.cs:38`) because it kept `((ItemViewer)_itemViewer)` casts. With
  seam S9 that cast disappears and the method becomes routing-testable. Exempting it here would be
  a Blocking finding under the epic's rule.
- `WireEvents`, `ResolveControlGroups`, `AdjustViewerForEfc`, `ToggleExpansionOn/Off` — all
  reachable via S9.
- `RegisterActions` (680-692) — dead **and** defective (D-2). Do not exempt; cover it and let the
  test document the current behavior, or delete it. Recommend covering it, so the promoted defect
  issue has a characterization test to invert later.

---

## 5. Existing tests

Searched `QuickFiler.Test/` for the type and for every member name.

- **There is no `EfcItemControllerTests.cs` and no test anywhere that references
  `EfcItemController`.** A repo-wide grep for the identifier returns only: `QuickFiler.csproj:301`,
  the type's own declarations, the two `new EfcItemController(...)` sites in `EfcFormController.cs`,
  and documentation. Coverage of this file is genuinely **zero**, not merely unmeasured.

- The sibling file in F9's own assignment has exactly **one** test:
  `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` (55 lines, one `[TestMethod]`,
  `PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel`). It uses a
  reflection-invoked **private no-arg constructor** on `EfcFormController` (`:18-28`,
  `EfcFormController.cs:79`) to get a fully-null instance. `EfcItemController` has **no**
  equivalent private no-arg constructor; F9 should add one (matching the `EfcFormController`
  precedent) or use the F10 `HarnessController` subclass pattern
  (`QfcItemController.TestSupport.cs:25-29`). The `EfcFormController` precedent is closer and
  cheaper: a `private EfcItemController() { }` plus a reflection factory in the test-support file.

- Reusable harness assets that F9 should consume rather than re-author, all in
  `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`:
  `SetField`/`GetField`/`InvokeNonPublic` (`:37-80`), `BuildSyncDispatcher` (`:102-137`),
  `BuildColorTheme`/`BuildThemeDictionary` (`:166-192`), `EnsureUiThreadDispatcher` (`:238-249`),
  `StartRunningDispatcher`/`ShutdownDispatcher` (`:297-326`). These are `internal static` members of
  the `QuickFiler.Controllers.Tests` namespace in the same test assembly, so F9 can call them
  directly. Note the type name is `QfcItemControllerTestSupport`; F9 should author its own
  `EfcItemController.TestSupport.cs` for Efc-specific builders and reference the Qfc one for the
  generic reflection/dispatcher/theme helpers, rather than duplicating them.

- COM-mock builders worth copying (not referencing — they are `private static`):
  `EfcDataModelTests.cs:220-376` (`CreateGlobals`, `CreateMailItem`, `CreateAddressEntry`,
  `CreateRecipients`, `CreateAttachments`, `CreateConversationTable`). F9 needs only
  `CreateGlobals`-scale mocks because seam S3 removes the need to build a live `EfcDataModel`.

---

## 6. Proposed test inventory

Each row is one atomic task. Scenario classes: **P**ositive · **N**egative · **E**dge ·
**Err**or · **S**tate-transition. All are plain `[TestClass]`/`[TestMethod]`; **none requires the
STA clause**, so no `EfcItemController.StaTests.cs` file is proposed.

Proposed test files (all need `<Compile Include>` entries in `QuickFiler.Test.csproj`):
`EfcItemController.TestSupport.cs`, `EfcItemController.ConstructionTests.cs`,
`EfcItemController.ViewerSetupTests.cs`, `EfcItemController.PropertiesTests.cs`,
`EfcItemController.EventWiringTests.cs`, `EfcItemController.EventHandlersTests.cs`,
`EfcItemController.NavigationTests.cs`, `EfcItemController.ThemeTests.cs`,
`EfcItemController.WebViewTests.cs`, `EfcItemControlSurfaceSeamTests.cs`,
`EfcDataModelSourceTests.cs`, `ItemViewerUiDispatcherTests.cs`.
Keep every file under the 500-line limit; split with a `.Part2.cs` suffix if needed (precedent:
`QfcStreamingDequeueConfidenceGateTests.Part2.cs`).

### Construction and initialization (`EfcItemController.ConstructionTests.cs`)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 1 | `Constructor_WithFiveArguments_CapturesKeyboardHandlerAndExplorerControllerFromHomeController` | .ctor 59-74 | P |
| 2 | `Constructor_WithFiveArguments_AssignsGlobalsViewerParentAndToken` | .ctor 59-74 | P |
| 3 | `Constructor_WithDataModel_InvokesInitializeOnceWithAsyncTrue` | .ctor 30-42 | P |
| 4 | `Constructor_WithExplicitAsyncFlagFalse_PassesFlagThroughToInitialize` | .ctor 44-57 | P |
| 5 | `Initialize_BuildsThemesThroughInjectedFactoryAndSetsActiveTheme` | `Initialize` 114-165 | P |
| 6 | `Initialize_TogglesEveryTipsDetailOffAndSharesItemPositionColumn` | `Initialize` 158-161 | P |
| 7 | `Initialize_StartsWebViewInitializationThroughTheBackgroundRunnerSeam` | `Initialize` 164 | P |
| 8 | `InitializeWithoutData_AdjustsViewerAndResolvesControlGroupsWithoutTouchingThemes` | `InitializeWithoutData` 76-88 | P |
| 9 | `InitializeWithoutData_ReturnsSameControllerInstance` | `InitializeWithoutData` 87 | P |
| 10 | `InitializeDataFields_WithNullConversationResolver_SkipsConversationPopulationAndStillWiresEvents` | `InitializeDataFields` 90-112 | E |
| 11 | `InitializeDataFields_ReturnsSameControllerInstance` | `InitializeDataFields` 111 | P |
| 12 | `InitializeDataFields_IsAltProbe_ReturnsFalseWhenMailIsNull` | `InitializeDataFields` 99 | E |
| 13 | `Cleanup_UnsubscribesDarkModeChangedFromGlobalsOlPropertyChanged` | `Cleanup` 262 | S |
| 14 | `Cleanup_UnsubscribesMouseEnterAndMouseLeaveFromEveryButton` | `Cleanup` 257-261 | S |
| 15 | `Cleanup_NullsGlobalsViewerParentDataModelAndThemeState` | `Cleanup` 262-277 | S |
| 16 | `Cleanup_WhenButtonsWereNeverResolved_ThrowsNullReference` | `Cleanup` 257 | Err (characterizes D-5) |

### Viewer setup (`EfcItemController.ViewerSetupTests.cs`)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 17 | `AdjustViewerForEfc_RemovesColumnsRightOfConversationCountLabel` | 247 | P |
| 18 | `AdjustViewerForEfc_ReducesOpenActionColumnByItemNumberMinusOpenWidth` | 250-252 | P |
| 19 | `AdjustViewerForEfc_WhenOpenActionIsWiderThanItemNumber_AppliesNegativeDelta` | 250-252 | E |
| 20 | `ResolveControlGroups_BuildsOneTipsDetailPerLeftTipsLabel` | 330-332 | P |
| 21 | `ResolveControlGroups_SetsItemPositionTipsToTheItemNumberLabel` | 334 | P |
| 22 | `ResolveControlGroups_CollectsOnlyTableLayoutPanelsFromChildren` | 336-339 | P |
| 23 | `ResolveControlGroups_CollectsOnlyButtonsFromChildren` | 341 | P |
| 24 | `ResolveControlGroups_WithNoChildren_ProducesEmptyPanelAndButtonCollections` | 336-341 | E |
| 25 | `PopulateControls_WithNullMailInfo_ReturnsWithoutWritingAnyViewerText` | 282-286 | N |
| 26 | `PopulateControls_WritesSenderSubjectBodyTriageSentOnAndActionable` | 287-292 | P |
| 27 | `PopulateControls_WhenTaskFlagIsSet_SetsFlagTaskDialogResultToOk` | 293-296 | S |
| 28 | `PopulateControls_WhenTaskFlagIsNotSet_SetsFlagTaskDialogResultToCancel` | 297-300 | S |
| 29 | `PopulateConversation_WithNullConversationResolver_ReturnsWithoutWritingCount` | 310-313 | N |
| 30 | `PopulateConversation_AssignsSetTopicThreadAsTheResolverUpdateUiCallback` | 314 | P |
| 31 | `PopulateConversation_WritesSameFolderCountToTheConversationCountLabel` | 315-316 | P |
| 32 | `PopulateConversation_WhenSameFolderCountIsZero_SetsConversationCountBackColorRed` | 317-320 | E |
| 33 | `PopulateConversation_WhenSameFolderCountIsPositive_LeavesBackColorUnchanged` | 317-320 | P |
| 34 | `SetTopicThread_SetsConversationItemsThenSortsBySentDateDescending` | 354-359 | P |

### Properties (`EfcItemController.PropertiesTests.cs`)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 35 | `ActiveTheme_WhenUnset_LoadsFromThemesAndCaches` | 393-396 | P |
| 36 | `ActiveTheme_WhenThemesIsNull_ThrowsArgumentNullExceptionFromStrictDependencyCheck` | 395 | Err |
| 37 | `ActiveTheme_Setter_StoresValueAndAppliesThatThemeAsynchronously` | 396-401 | S |
| 38 | `ActiveTheme_Setter_WithUnknownKey_ThrowsKeyNotFound` | 400 | Err |
| 39 | `LoadTheme_WhenDarkModeIsTrue_SelectsDarkNormalAndAppliesIt` | 404-409 | P |
| 40 | `LoadTheme_WhenDarkModeIsFalse_SelectsLightNormalAndAppliesIt` | 404-409 | P |
| 41 | `DarkMode_ReadsFromOutlookObjectsWhenDependenciesArePresent` | 441-448 | P |
| 42 | `DarkMode_WhenGlobalsOlIsNull_ReturnsFalseWithoutThrowing` | 441-448 | N |
| 43 | `DarkMode_Setter_WritesThroughToOutlookObjects` | 449 | S |
| 44 | `ItemNumber_Setter_WritesTheNumberToTheViewerItemNumberText` | 576-580 | S |
| 45 | `ItemIndex_IsItemNumberMinusOne_AndSetterStoresValuePlusOne` | 582-586 | E |
| 46 | `SentDate_FormatsItemInfoSentDateAsMonthDayYear` | 600-603 | P |
| 47 | `SentTime_FormatsItemInfoSentDateAsTwentyFourHourClock` | 605-608 | P |
| 48 | `Sender_And_To_ReadFromItemInfoRatherThanTheViewer` | 595-598, 621-624 | P |
| 49 | `Subject_ReadsFromTheViewerSubjectTextRatherThanItemInfo` | 610-613 | E (documents the D-6 asymmetry) |
| 50 | `SelectedFolder_DelegatesToViewerGetSelectedFolder` | 588-593 | P |
| 51 | `Height_DelegatesToViewerHeight` | 535-538 | P |
| 52 | `ScalarProperties_RoundTrip_ConvOriginIdCounterEnterCounterComboRightIsChildIsActiveUiSuppressEventsToken` | 417-436, 546-558, 615-619, 631-636 | P |
| 53 | `RightKeyActions_ReturnsASingleCancelEntryWhoseActionIsANoOp` | 1157-1166 | P |

### Event wiring and keyboard (`EfcItemController.EventWiringTests.cs`)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 54 | `WireEvents_SubscribesPreviewKeyDownAndKeyDownOnEveryEligibleControl` | 645-662 | P |
| 55 | `WireEvents_SubscribesToWebViewInitializationCompleted` | 664-665 | P |
| 56 | `WireEvents_WithNullConversationResolver_SkipsResolverSubscriptionAndStillWiresTheRest` | 666-669 | N |
| 57 | `WireEvents_SubscribesToConversationItemSelectionChangedAndGlobalsPropertyChanged` | 670-672 | P |
| 58 | `WireEvents_SubscribesMouseEnterAndMouseLeaveOnEveryButton` | 673-677 | P |
| 59 | `RegisterActions_WithOverwriteDuplicatesFalse_FiltersOutKeysAlreadyRegistered` | 685-690 | P |
| 60 | `RegisterActions_WithOverwriteDuplicatesTrue_DoesNotFilter` | 685-690 | P |
| 61 | `RegisterActions_WithAnUnregisteredKey_SilentlyDropsTheAction` | 691 | Err (characterizes D-2) |
| 62 | `RegisterAsyncFocusActions_WhenCollapsed_RegistersOnlyOpenAndExpand` | 696-705 | S |
| 63 | `RegisterAsyncFocusActions_WhenExpanded_AlsoRegistersBodyAndDetailJumps` | 706-718 | S |
| 64 | `RegisterAsyncFocusActions_OpenActionInvokesExplorerControllerOpenQfItem` | 696-700 | P |
| 65 | `RegisterAsyncFocusActions_ExpandActionRoutesThroughKbdExecuteAsync` | 701-705 | P |
| 66 | `UnregisterAsyncFocusActions_WhenCollapsed_RemovesOnlyOpenAndExpand` | 723-729 | S |
| 67 | `UnregisterAsyncFocusActions_WhenExpanded_AlsoRemovesBodyAndDetailJumps` | 726-729 | S |
| 68 | `UnregisterActions_RemovesEveryRequestedKeyFromTheItemSource` | 732-735 | P |
| 69 | `UnregisterActions_WithAnEmptyKeyList_IsANoOp` | 734 | E |
| 70 | `KbdExecuteAsync_TogglesTheKeyboardDialogBeforeInvokingTheAction` | 1144-1148 | S |
| 71 | `KbdExecuteAsync_WhenTheActionThrows_PropagatesAfterTheDialogToggle` | 1147 | Err |
| 72 | `JumpToAsync_TogglesTheKeyboardDialogThenFocusesTheTargetControl` | 1150-1155 | P |

### Event handlers (`EfcItemController.EventHandlersTests.cs`)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 73 | `ConversationResolverPropertyChanged_ForExpandedProperty_ReplacesAndSortsTheConversationItems` | 746-754 | P |
| 74 | `ConversationResolverPropertyChanged_ForAnyOtherProperty_LeavesTheConversationUntouched` | 746 | N |
| 75 | `ConversationResolverPropertyChanged_IsNeverTriggeredByTheResolversOwnNotifications` | 746 | Err (characterizes D-1) |
| 76 | `TopicThreadItemSelectionChanged_WithASelectedHelper_NavigatesToItsHtml` | 762-767 | P |
| 77 | `TopicThreadItemSelectionChanged_WithNoSelection_DoesNotNavigate` | 763 | N |
| 78 | `TopicThreadItemSelectionChanged_WithAnEmptySelection_DoesNotNavigate` | 763 | E |
| 79 | `OnWebViewInitialized_WhenInitializationFailed_RethrowsTheInitializationException` | 775-778 | Err |
| 80 | `OnWebViewInitialized_WhenSuccessful_MarksTheWebViewerInitialized` | 779 | S |
| 81 | `OnWebViewInitialized_WithNullItemInfo_ReturnsBeforeNavigating` | 781-784 | N |
| 82 | `OnWebViewInitialized_InDarkMode_NavigatesToDarkToggledHtml` | 785-790 | P |
| 83 | `OnWebViewInitialized_InLightMode_NavigatesToLightToggledHtml` | 791-796 | P |
| 84 | `OnWebViewInitialized_HidesTheBodyWebViewAfterNavigating` | 798 | S |
| 85 | `DarkModeChanged_ForDarkModeProperty_WhenDarkModeIsOn_SelectsDarkNormal` | 803-809 | S |
| 86 | `DarkModeChanged_ForDarkModeProperty_WhenDarkModeIsOff_SelectsLightNormal` | 810-813 | S |
| 87 | `DarkModeChanged_ForAnyOtherProperty_LeavesTheActiveThemeUnchanged` | 803 | N |
| 88 | `ButtonMouseEnter_AppliesTheActiveThemeMouseOverColor` | 817-820 | P |
| 89 | `ButtonMouseLeave_WhenDialogResultIsOk_AppliesTheClickedColor` | 824-827 | S |
| 90 | `ButtonMouseLeave_WhenDialogResultIsNotOk_AppliesTheDefaultBackColor` | 828-831 | S |

### Navigation, expansion and tips (`EfcItemController.NavigationTests.cs`)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 91 | `ToggleExpansion_WhenCollapsed_RequestsExpansionOn` | 838-847 | S |
| 92 | `ToggleExpansion_WhenExpanded_RequestsExpansionOff` | 838-847 | S |
| 93 | `ToggleExpansionAsync_WhenCollapsed_RequestsExpansionOn` | 850-859 | S |
| 94 | `ToggleExpansionAsync_WhenExpanded_RequestsExpansionOff` | 850-859 | S |
| 95 | `ToggleExpansionOn_NotifiesTheParentExpansionStyleHost` | 864 | P |
| 96 | `ToggleExpansionOn_SetsBodyToggleColumnsToZeroAndOneHundredAndShowsBothPanes` | 867-871 | S |
| 97 | `ToggleExpansionOn_WithUnreadItem_ArmsTheReadFormatTimer` | 873-877 | S |
| 98 | `ToggleExpansionOn_WithReadItem_DoesNotArmTheTimer` | 873 | E |
| 99 | `ToggleExpansionOn_WithNullItemInfo_DoesNotArmTheTimer` | 873 | N |
| 100 | `ToggleExpansionOn_RegistersBodyAndDetailJumpKeys` | 879-888 | S |
| 101 | `ToggleExpansionOn_CalledTwice_ThrowsBecauseTheJumpKeysAreAlreadyRegistered` | 879-888 | Err (characterizes D-4) |
| 102 | `ToggleExpansionOff_SetsBodyToggleColumnsToOneHundredAndZeroAndHidesBothPanes` | 892-897 | S |
| 103 | `ToggleExpansionOff_DisposesAnArmedReadFormatTimer` | 898-901 | S |
| 104 | `ToggleExpansionOff_WithNoArmedTimer_IsANoOp` | 898 | E |
| 105 | `ToggleExpansionOff_RemovesBodyAndDetailJumpKeys` | 902-903 | S |
| 106 | `ToggleExpansionAsyncOn_DispatchesToggleExpansionOnThroughTheViewerDispatcher` | 911-919 | P |
| 107 | `ToggleExpansionAsyncOff_DispatchesToggleExpansionOffThroughTheViewerDispatcher` | 920-928 | P |
| 108 | `ToggleExpansionOnPrivate_DoesNotRegisterJumpKeys_UnlikeTheSynchronousPath` | 944-956 | Err (characterizes D-3) |
| 109 | `ToggleExpansionOffPrivate_DoesNotRemoveJumpKeys_UnlikeTheSynchronousPath` | 931-942 | Err (characterizes D-3) |
| 110 | `ToggleNavigation_WhenActive_DeactivatesAndUnregistersFocusActions` | 969-973 | S |
| 111 | `ToggleNavigation_WhenInactive_ActivatesAndRegistersFocusActions` | 974-978 | S |
| 112 | `ToggleNavigationWithState_Off_WhenActive_Deactivates` | 984-988 | S |
| 113 | `ToggleNavigationWithState_On_WhenInactive_Activates` | 989-993 | S |
| 114 | `ToggleNavigationWithState_Off_WhenAlreadyInactive_IsANoOp` | 984-993 | E |
| 115 | `ToggleNavigationWithState_On_WhenAlreadyActive_IsANoOp` | 984-993 | E |
| 116 | `ToggleNavigationAsync_Off_WhenActive_AwaitsTipsThenDeactivates` | 998-1003 | S |
| 117 | `ToggleNavigationAsync_On_WhenInactive_AwaitsTipsThenActivates` | 998-1008 | S |
| 118 | `ToggleNavigationAsync_WhenStateAlreadyMatches_LeavesActiveUiUnchanged` | 999-1008 | E |
| 119 | `ToggleTips_Asynchronous_PostsEachToggleThroughBeginInvoke` | 1015-1018 | P |
| 120 | `ToggleTips_Synchronous_PostsEachToggleThroughInvoke` | 1019-1022 | P |
| 121 | `ToggleTipsWithState_Asynchronous_PostsDesiredStateThroughBeginInvoke` | 1030-1035 | P |
| 122 | `ToggleTipsWithState_Synchronous_PostsDesiredStateThroughInvoke` | 1036-1041 | P |
| 123 | `ToggleTipsAsync_AwaitsEveryTipToggleConcurrently` | 1050-1054 | P |
| 124 | `ToggleTipsAsync_WithACancelledToken_ThrowsBeforeTogglingAnyTip` | 1047 | Err |
| 125 | `ToggleTipsAsync_WithAnEmptyTipsList_CompletesWithoutToggling` | 1050-1054 | E |
| 126 | `ToggleSaveAttachments_InvertsTheAttachmentsCheckStateThroughTheDispatcher` | 1065-1069 | S |
| 127 | `ToggleSaveCopyOfMail_InvertsTheEmailCopyCheckStateThroughTheDispatcher` | 1074-1076 | S |

### Theme (`EfcItemController.ThemeTests.cs`)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 128 | `SetThemeDark_WhenActiveThemeIsNull_SelectsDarkNormal` | 1085-1089 | E |
| 129 | `SetThemeDark_WhenActiveThemeIsANormalVariant_SelectsDarkNormal` | 1085-1089 | S |
| 130 | `SetThemeDark_WhenActiveThemeIsAnActiveVariant_SelectsDarkActive` | 1090-1094 | S |
| 131 | `SetThemeDark_SetsTheDarkModeBackingFieldWithoutWritingToOutlook` | 1095 | S |
| 132 | `SetThemeLight_WhenActiveThemeIsNull_SelectsLightNormal` | 1112-1116 | E |
| 133 | `SetThemeLight_WhenActiveThemeIsAnActiveVariant_SelectsLightActive` | 1117-1121 | S |
| 134 | `SetThemeLight_ClearsTheDarkModeBackingFieldWithoutWritingToOutlook` | 1122 | S |
| 135 | `HtmlDarkConverter_BeforeWebViewInitialization_DoesNothing` | 1100 | N |
| 136 | `HtmlDarkConverter_AfterInitialization_NavigatesToTheToggledItemHtml` | 1102 | P |
| 137 | `HtmlDarkConverter_AfterInitialization_TogglesEveryExpandedConversationItem` | 1103-1105 | P |
| 138 | `ApplyReadEmailFormat_MarksTheItemReadAndSavesTheUnderlyingMailItem` | 1127 | S |
| 139 | `ApplyReadEmailFormat_AppliesTheMailRelatedControlGroupOfTheActiveTheme` | 1128 | P |
| 140 | `SetOlvTheme_AppliesAHeaderFormatStyleWithTheGivenForeAndBackColorsToEveryColumn` | 1131-1138 | P |
| 141 | `SetOlvTheme_WithNoColumns_IsANoOp` | 1137 | E |

### WebView (`EfcItemController.WebViewTests.cs`)

| # | Test name | Member | Class |
| --- | --- | --- | --- |
| 142 | `InitializeWebViewAsync_CreatesTheEnvironmentInTheLocalAppDataWebViewCacheFolder` | 210-213 | P |
| 143 | `InitializeWebViewAsync_SwitchesToTheViewerSynchronizationContextBeforeCreatingTheEnvironment` | 220-223 | S |
| 144 | `InitializeWebViewAsync_EnsuresCoreWebViewOnTheBodyControlWithTheCreatedEnvironment` | 232-239 | P |
| 145 | `InitializeWebViewAsync_WhenEnvironmentCreationFails_PropagatesTheFault` | 223-239 | Err |
| 146 | *(only if `InitializeWebView()` is retained rather than deleted)* `InitializeWebView_CreatesTheEnvironmentAndContinuesOnTheUiScheduler` | 174-205 | P |

### New seam artifacts

| # | Test name | Target | Class |
| --- | --- | --- | --- |
| 147 | `EfcDataModelSource_ExposesMailInfoConversationResolverAndMailFromTheUnderlyingModel` | `EfcDataModelSource` | P |
| 148 | `EfcDataModelSource_WithANullModel_ThrowsArgumentNullException` | `EfcDataModelSource` ctor | Err |
| 149 | `ItemViewerUiDispatcher_InvokeRunsTheActionOnTheViewerDispatcherThread` | `ItemViewerUiDispatcher` | P |
| 150 | `ItemViewerUiDispatcher_InvokeAsyncCompletesAfterTheActionRuns` | `ItemViewerUiDispatcher` | P |
| 151 | `ItemViewerUiDispatcher_BeginInvokeDoesNotBlockTheCaller` | `ItemViewerUiDispatcher` | P |
| 152 | `ItemViewerUiDispatcher_GenericInvokeAsyncReturnsTheFunctionResult` | `ItemViewerUiDispatcher` | P |
| 153 | `ItemViewerUiDispatcher_WithANullViewer_ThrowsArgumentNullException` | ctor | Err |
| 154 | `EfcItemControllerDependencies_WithNoArguments_SuppliesEveryProductionDefault` | dependencies | P |
| 155 | `EfcItemControllerDependencies_WithSuppliedSeams_PrefersTheSuppliedInstanceOverTheDefault` | dependencies | P |

**Total: 155 test cases** (154 if `InitializeWebView()` is deleted rather than retained).
Approximate distribution: 63 positive, 12 negative, 25 edge, 16 error, 39 state-transition.

---

## 7. Latent defects (report only — do NOT fix under F9's no-behavior-change NFR)

Promote each via the MCP promotion lifecycle per the epic's "Latent Defect Promotion" section.

| ID | Location | Mechanism |
| --- | --- | --- |
| **D-1** | `EfcItemController.cs:746` | `ConversationResolverPropertyChanged` is dead in production. Its guard is `e.PropertyName == nameof(_dataModel.ConversationResolver.ConversationInfo.Expanded)`, which the compiler resolves to the literal `"Expanded"`. `ConversationResolver` only ever raises `"ConversationInfo"` (`ConversationResolver.Loading.cs:26`), `"ConversationItems"` (`:167`), `"Df"` (`:205, :227`) and `"UpdateUI"` (`ConversationResolver.cs:277`). It never raises `"Expanded"`, so the subscription at `:667` fires but the body at `:749-753` never executes; background-loaded conversation rows never reach the topic thread through this path. |
| **D-2** | `EfcItemController.cs:691` | `RegisterActions` silently drops every action whose key is not already registered. `_keyboardHandler.CharActions[action.Key] = action.Value` uses the `KbdActions<>` indexer setter (`KbdActions.cs:38-47`), which does `Find(key)` and only assigns when the element is **non-null** — a missing key is a no-op, not an insert. Combined with the `!overwriteDuplicates` filter at `:687-690` (which removes exactly the keys that *are* present), the `overwriteDuplicates: false` path is guaranteed to register nothing. The method also has zero call sites today, so the defect is currently latent. |
| **D-3** | `EfcItemController.cs:931-956` vs `:862-905` | The async expansion path and the sync expansion path are not equivalent. `ToggleExpansion(ToggleState.On)` registers `'B'`/`'D'` in `CharActions` (`:879-888`) and `ToggleExpansion(Off)` removes them (`:902-903`), but `ToggleExpansionOn()` (`:944-956`) and `ToggleExpansionOff()` (`:931-942`) — the bodies dispatched by `ToggleExpansionAsync` (`:913, :922`) — do neither. Expanding through the async path therefore leaves the body/detail jump keys unregistered. |
| **D-4** | `EfcItemController.cs:879-888` | `KbdActions<>.Add` throws `ArgumentException` when the `(sourceId, key)` pair already exists (`KbdActions.cs:92-98`). Because of D-3, a sync-On → async-Off → sync-On sequence leaves the `"Item"`/`'B'` and `"Item"`/`'D'` entries in place and the second sync-On throws on a UI-thread call path. |
| **D-5** | `EfcItemController.cs:257` | `Cleanup()` dereferences `Buttons` (i.e. `_buttons`) unconditionally. `_buttons` is only assigned in `ResolveControlGroups` (`:341`), which the `(globals, homeController, parent, itemViewer, token)` constructor never runs. Cleaning up a controller built through that constructor without a subsequent `InitializeWithoutData()`/`InitializeDataFields()` throws `NullReferenceException`. `Cleanup` also never nulls `_buttons` while nulling 15 sibling fields, and sets `_timer = null` (`:277`) **without disposing it**, leaking an armed `System.Threading.Timer` whenever the item is cleaned up while expanded and unread. `_itemViewer = null` is also written twice (`:264` and `:276`). |
| **D-6** | `EfcItemController.cs:610-613` vs `:595-598` | `Subject` reads `_itemViewer.LblSubject.Text` while `Sender` and `To` read `_itemInfo`. After `Cleanup()` nulls `_itemViewer` (`:264`), `Subject` throws while `Sender` still works; before `PopulateControls` runs, `Subject` returns the designer placeholder rather than the mail subject. |
| **D-7** | `EfcItemController.cs:184` and `:217` | The `CoreWebView2EnvironmentOptions` additional-browser-arguments string is `"–incognito "` using **U+2013 EN DASH**, not the two ASCII hyphens a Chromium switch requires. The commented-out alternative directly above (`:182`, `:215`) correctly uses `"--disk-cache-size=1 "`. The intended incognito mode is therefore never applied. The identical defect exists at `QfcItemController.ViewerSetup.cs:52`, so a single issue should cover both call sites. |
| **D-8** | `EfcItemController.cs:441-448` | `DarkMode`'s getter passes `_globals.Ol` as a dependency argument, which is evaluated **before** `Initializer.DependenciesNotNull` can inspect it. When `_globals` is null (the post-`Cleanup` state) the getter throws `NullReferenceException` instead of returning the intended `false` default. `DarkMode_Changed` (`:803`) has the same exposure via `nameof(_globals.Ol.DarkMode)` — that one is compile-time-safe, but line 805 (`_globals.Ol.DarkMode`) is not. |
| **D-9** | `EfcItemController.cs:777` | `throw (e.InitializationException)` rethrows a captured exception, resetting its stack trace, from inside a WebView2 event handler on the UI thread — an unhandled UI-thread exception rather than a logged, recoverable failure. `throw new InvalidOperationException(..., e.InitializationException)` (or logging plus a guarded return) would preserve context. |
| **D-10** | `EfcItemController.cs:741`, `:882`, `:887`, `:704`, `:711`, `:716` | Multiple `async void` / async-lambda-into-`Action<char>` conversions. `CharActions` is `KbdActions<char, KaChar, Action<char>>` (`IQfcKeyboardHandler.cs:21`), so `async (x) => await JumpToAsync(...)` at `:882` and `:887` compiles as an `async void` lambda: any fault is raised on the thread pool and crashes the process rather than surfacing to the caller. |
| **D-11** | `EfcItemController.cs:174-205` and `:680-692` | `InitializeWebView()` and `RegisterActions(...)` have zero call sites repo-wide (verified by grep across `QuickFiler/`, `QuickFiler.Test/`). Dead `internal` code. `:381` (`_selectorsCtrls`) is likewise initialized to `null` and never assigned before being passed to `SetupThemes` as the `selectors` argument (`:97`, `:144`). |
| **D-12** | `EfcItemController.cs:44-57` | The `(globals, homeController, parent, itemViewer, dataModel, bool async, token)` constructor overload has zero call sites; only the 6-argument (`:30`) and 5-argument (`:59`) forms are used by `EfcFormController.cs:87` and `:69`. |

---

## 8. Acceptance-risk notes for the plan

1. **Coverage arithmetic.** After the split, the 80% line / 75% branch gates apply **per file**, not
   to the type. The two riskiest files are `EfcItemController.WebView.cs` (contains the one method
   with a genuine third-party-args construction problem) and `EfcItemController.cs` (contains the
   constructors and `Cleanup`). Sequence the plan so those two files' tests land first, and measure
   after each, rather than measuring only at the end.

2. **New files take the 90% bar,** per the epic's "Mid-Wave File Creation" rule 4. That applies to
   all eight partials (they are new `<Compile Include>` entries even though the code is moved) and
   to every seam file in §3. Confirm with F1 whether a partial produced by a pure move inherits the
   80% bar or the 90% new-file bar — the epic text is ambiguous here and the answer changes the
   test count for `EfcItemController.Properties.cs`.

3. **Cobertura aggregation.** Per the epic's harness directives, the eight partials of one type
   produce `<class>` elements that share nothing — each partial has its own `filename` — but the
   compiler-generated `<>c` closure classes for the lambdas at `:84`, `:99`, `:101`, `:146`, `:148`,
   `:160`, `:198`, `:233`, `:646`, `:673`, `:688`, `:691`, `:699`, `:704`, `:711`, `:716`, `:734`,
   `:882`, `:887`, `:1017`, `:1021`, `:1033`, `:1039`, `:1051`, `:1065`, `:1074`, `:1103`, `:1137`,
   `:1164` will be attributed to whichever partial's file they were declared in. The union-by-
   `filename`, max-hits-per-line rule is load-bearing for this file specifically.

4. **Shared-file conflicts.** F9 edits `QuickFiler.csproj` (7 new production entries plus 7 renamed/
   added partial entries) and `QuickFiler.Test.csproj` (12 new test entries). Both are additive;
   expect conflicts at fan-in and resolve by keeping both sides. Preserve CRLF.

5. **Sibling coupling inside F9.** `EfcFormController.cs` (the other 1,086-line file in this child)
   must gain `IEfcExpansionStyleHost` on its declaration (§3 S2). Sequence F9's plan so the
   interface file and that one-line declaration change land before the `EfcItemController` seam
   tasks, or the intermediate commits will not build.
