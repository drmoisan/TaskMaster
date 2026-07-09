# Research Findings — TaskVisualization Secondary Testability Refactor (Issue #298)

- Feature: `2026-07-09-taskvisualization-secondary-testability-298`
- Parent epic: winforms-testability-refactor (#295)
- Depends on: #297 (taskvisualization-core-testability-refactor) — both modify `TaskVisualization.csproj` and `TaskVisualization.Test`
- Author: task-researcher
- Timestamp: 2026-07-09T15-31Z
- Status: research complete; design-phase input for spec + atomic plan
- Scope constraint honored: no production or test code was modified during this research.

This artifact is implementation-ready design research. It classifies every in-scope file,
designs the two viewer interfaces and the helper seams, enumerates every in-repo caller and its
required change, coordinates explicitly with #297, proposes a file decomposition, and maps each
unit to a concrete test approach. All findings are grounded in reads of the current tree.

---

## 0. Executive Summary

- The project already carries prior COM/VSTO exemption work from issue #197: `EditFilterController`
  has a **class-level** `[ExcludeFromCodeCoverage]`; `FlagChangeGroup` has **method-level**
  exemptions on its four Outlook-bound members with `TryEnqueue` preserved measured; the other
  helpers (`AutoCreateProject`, `AutoAssignContext`, `AutoAssignPeople`, `FlagTasks`,
  `EditFilterViewer`, `ManageFilters`) carry **class-level** exemptions;
  `FlagChangeTrainingQueue` and `FlagChangeItem` are unexempted (measured).
- #298 inverts #197's "exempt the whole class" posture for the controllers and helpers: it
  introduces seams so the host-neutral logic becomes measured and tested, and it narrows the
  remaining exemptions to genuinely UI/COM-bound wiring. This is consistent with the epic NFR
  "No `[ExcludeFromCodeCoverage]` on testable seams."
- Two viewer interfaces are required: `IEditFilterViewer` and `IManageFiltersViewer`, both deriving
  from `UtilitiesCS.Interfaces.IWinForm.IForm`. Because the controllers manipulate individual child
  controls (`Label.Text`, `Button.Click`, `TextBox.Text`), the interfaces must expose **behavioral
  members** (string properties + `event EventHandler ...Click`) rather than raw WinForms control
  objects, so a Moq mock satisfies them without instantiating any `Control`.
- The single largest coverage lever is `FlagTasks`: its pure static helpers (`GetFlagsToSet`,
  `ConvertFlagStringsToEnum`, `GetSymbolsDictionary`) are already host-neutral but entangled behind
  COM-touching entry points. Extracting them into a host-neutral file makes them measurable without
  a live Outlook process.
- **Hard external constraint:** `FlagTasks`'s public constructor shape
  `FlagTasks(IApplicationGlobals, IList, bool, IntPtr, string)` is consumed by a QuickFiler factory
  seam `Func<IApplicationGlobals, List<MailItem>, bool, IntPtr, FlagTasks>`
  (`QuickFiler/Controllers/QfcItemController.Initialization.cs:42,390` and matching QuickFiler.Test
  seam tests). The refactor MUST preserve that constructor signature and the `FlagTasks` concrete
  type name, or it breaks QuickFiler (out of scope for this feature).

---

## A. Per-File Content Classification

Legend: **HN** = host-neutral logic (no WinForms/Interop), **WF** = WinForms interaction,
**COM** = Outlook-Interop/COM interaction, **MIX** = a single member mixing categories.

### A.1 `EditFilterController.cs` (231 lines, `internal`, class-level `[ExcludeFromCodeCoverage]`)

Bound to concrete `EditFilterViewer` field `_viewer` (line 73).

| Member | Lines | Class | Notes |
|---|---|---|---|
| `EditFilterController()` | 25 | HN | empty parameterless ctor |
| `EditFilterController(IApplicationGlobals)` | 27-30 | HN | field assignment only (private) |
| `EditFilterController(IApplicationGlobals, Action<...,FilterEntry>)` | 32-41 | MIX | sets fields, `new FilterEntry()`, calls `Initialize()` (WF/COM) |
| `EditFilterController(IApplicationGlobals, FilterEntry)` | 43-49 | MIX | clones FilterEntry (HN), calls `Initialize()` |
| `DeleteFilterDialog(IApplicationGlobals, FilterEntry)` (static) | 51-68 | WF | `InitializeFactory()`, `viewer.Text`, `viewer.ShowDialog()`, maps `DialogResult` |
| `Initialize()` | 79-102 | MIX | `new EditFilterViewer()` (WF), `_globals.Ol.NamespaceMAPI.Categories` (COM), control `.Text` sets (WF), `GetTips()` (WF), `RegisterEventHandlers()`, `_viewer.Show()` (WF) |
| `InitializeFactory()` | 104-122 | MIX | same as Initialize minus event wiring/Show; returns viewer |
| `SelectItems(FlagTranslator, FlagTranslator, IPrefix, Label)` | 128-149 | MIX | builds sorted dict (HN), `new TagViewer()` + `new TagController(viewer, dictOptions)` + `ShowDialog()` (WF), writes `selections.AsStringNoPrefix` and `label.Text` (HN + WF) |
| `SetUpDeleteDialog()` | 151 | HN | empty body (dead) |
| `RegisterEventHandlers()` | 157-166 | WF | 7 `control.Click +=` wirings |
| `CategorySelection_Click` | 168-177 | MIX | `_defaults.PrefixList.Find(...)` (HN) + `SelectItems(...)` (WF) |
| `PeopleSelection_Click` | 179-188 | MIX | same shape |
| `ProjectSelection_Click` | 190-199 | MIX | same shape |
| `TopicSelection_Click` | 201-205 | MIX | same shape |
| `FoldersSelected_Click` | 207 | HN | empty body (dead) |
| `BtnCancel_Click` | 209-216 | MIX | `_viewer.Close()` (WF) + `_filterEntry.RevertToCopy(_copy)` (HN) guarded by `_callback is null` |
| `BtnOk_Click` | 218-227 | MIX | `_viewer.Hide()`, `_viewer.FilterName.Text` (WF), set `_filterEntry.Name` (HN), invoke `_callback` (HN), `_viewer.Dispose()` (WF) |

Viewer/control surface consumed by the controller (drives `IEditFilterViewer`, section B):
`Text` (set), `ShowDialog()`, `Show()`, `Close()`, `Hide()`, `Dispose()`, `GetTips()`,
`ContextSelection.Text`, `PeopleSelection.Text`, `ProjectSelection.Text`, `TopicSelection.Text`,
`FilterName.Text`, and the `.Click` events of `ContextSelection`, `PeopleSelection`,
`ProjectSelection`, `TopicSelection`, `FoldersSelected`, `BtnOk`, `BtnCancel`.

### A.2 `EditFilterViewer.cs` (35 lines, `public partial : Form`, class-level exempt)

`WF` only. Ctor calls `InitializeComponent()`. `GetTips()` returns `List<Label>` of the eight `Xl*`
tip labels. Designer field inventory (from `EditFilterViewer.designer.cs:476-502`, control names the
controller/tips reference): controls are declared `internal` — `ContextSelection`, `PeopleSelection`,
`ProjectSelection`, `TopicSelection` (Labels), `FoldersSelected` (Label), `BtnOk`, `BtnCancel`
(Buttons), `FilterName` (TextBox), and tip labels `XlContext/XlProject/XlPeople/XlTopic/XlCancel/
XlOk/XlFilterName/XlFolders`. `internal` access is why the controller can read them today.

### A.3 `EditFilterViewer.designer.cs` (503 lines)

`WF` only, generated. Covered by the form partial class's `[ExcludeFromCodeCoverage]`. Control
construction only; no logic. Not to be refactored beyond adding the interface implementation on the
non-designer partial.

### A.4 `ManageFilters.cs` (57 lines, `public partial : Form`, class-level exempt)

| Member | Lines | Class | Notes |
|---|---|---|---|
| ctor | 19-22 | WF | `InitializeComponent()` |
| `LoadFilters(IApplicationGlobals)` | 26-30 | MIX | stores globals (HN), `FiltersOlv.SetObjects(_globals.AF.Filters)` (WF, ObjectListView) |
| `BtnEditFilter_Click` | 32-36 | MIX | cast `FiltersOlv.SelectedItem.RowObject` to `FilterEntry` (WF read) + `new EditFilterController(_globals, filterEntry)` (HN construct) |
| `BtnAddFilter_Click` | 38-43 | MIX | `new EditFilterController(_globals, EditFilterCallback)` + `FiltersOlv.SetObjects/BuildList` (WF) |
| `EditFilterCallback(EditFilterController, FilterEntry)` | 45-50 | MIX | `_globals.AF.Filters.Add/Serialize` (HN/IO) + `FiltersOlv.BuildList()` (WF) |
| `BtnDelete_Click` | 52-55 | WF | reads selected row; body otherwise dead |

Designer (`ManageFilters.Designer.cs:157-163`): `FiltersOlv` (`FastObjectListView`, internal),
`FilterName`/`Description` (`OLVColumn`, internal), `BtnAddFilter`/`BtnEditFilter`/`BtnDelete`
(Buttons, private). The logic worth extracting is thin: the add/edit/callback orchestration around
`_globals.AF.Filters`.

### A.5 `FlagTasks.cs` (242 lines, `public`, class-level exempt)

| Member | Lines | Class | Notes |
|---|---|---|---|
| ctor `(IApplicationGlobals, IList, bool, IntPtr, string)` | 35-76 | MIX | `globals.Ol.App.ActiveExplorer()` (COM), `InitializeToDoList` (COM), `MessageBox.Show` (WF), `GetFlagsToSet` (HN), `new TaskViewer()` (WF), `new AutoAssignPeople/AutoCreateProject/AutoAssignContext`, `new TaskController(...)` (WF/COM) |
| `Run(bool)` | 78-93 | WF | `_controller.Initialize()`, `_viewer.ShowDialog()/Show()` |
| `InitializeToDoList(IList, IApplicationGlobals)` (static) | 95-133 | MIX | COM enumeration + `MessageBox.Show` + `ToDoItem` shaping |
| `PopulateUdf(IList, IApplicationGlobals)` (static) | 135-140 | MIX | `InitializeToDoList` (COM) + `GetFlagsToSet` (HN) + `WriteFlagsBatch` |
| `GetSelection(Explorer)` (static) | 146-149 | COM | `olExplorer.Selection.Cast<object>()` |
| `GetFlagsToSet(int)` (static) | 156-170 | HN* | pure branch on count; calls `GetUserInputFlagsToAdjust` (WF) only when count>1 |
| `ConvertFlagStringsToEnum(List<string>)` (static) | 172-193 | HN | pure enum bit-or over strings |
| `GetSymbolsDictionary()` (static) | 195-214 | HN | pure enum -> sorted dict |
| `GetUserInputFlagsToAdjust(SortedDictionary)` (static) | 216-240 | WF | `new TagViewer()` + `new TagController(...)` + `ShowDialog()` |

`*GetFlagsToSet` is HN except for its `>1` branch delegating to a WF dialog. That dialog call is the
seam boundary (section D.5).

### A.6 `AutoCreateProject.cs` (211 lines, `public : IAutoAssign`, class-level exempt)

Primary-ctor `AutoCreateProject(IApplicationGlobals globals)`.

| Member | Lines | Class | Notes |
|---|---|---|---|
| `FilterList` | 23 | HN | projects `_globals.TD.CategoryFilters` |
| `AddChoicesToDict(...)` | 25-33 | HN | `throw new NotImplementedException()` |
| `AddColorCategory(IPrefix, string)` | 35-64 | MIX | `StripPrefix` (HN), `ProjInfo.Contains_ProjectName`/`Add`/`Serialize` (HN/IO), `TryAutoExtractProgram` (HN), `ChooseOrCreateProgramName` (WF), `CreateCategoryModule.CreateCategory(olNS=...NamespaceMAPI...)` (COM), `CreateProjectTaskItem` (COM) |
| `GetNextProjectID(string)` | 66-75 | HN | pure LINQ over `ProjInfo` + `IDList.GetNextToDoID` |
| `ChooseOrCreateProgramName()` | 77-105 | MIX | `_globals.Ol.StoresWrapper...` (COM), `new TagLauncher(...)` + `Viewer.ShowDialog()` (WF), program-id allocation (HN/IO) |
| `TryAutoExtractProgram(string, out string)` | 107-121 | HN | pure substring scan over `ProgramInfo.Keys` |
| `CreateProjectTaskItem(string, string)` | 123-133 | COM | `GetTaskItems().Add(olTaskItem)`, `new OutlookItem(taskItem)`, `ToDoItem` writes |
| `GetTaskItems()` | 135-141 | COM | `_globals.Ol.App.Session.GetDefaultFolder(...)` |
| `StripPrefix(string, string)` | 143-153 | HN | pure string replace |
| `AutoFind(object)` | 155-159 | HN | `throw new NotImplementedException()` |
| `AutoFindAsync(object)` | 161-176 | COM | `ToHelper` + `CategoryClassifierGroup.CreateEngineAsync` |
| `ToHelper(object)` | 178-209 | COM | `MailItemHelper.FromMailItemAsync` on live MailItem |

Testable HN seam density is high here: `FilterList`, `GetNextProjectID`, `TryAutoExtractProgram`,
`StripPrefix`, and the NotImplemented throwers are all measurable with a mocked `IApplicationGlobals`.

### A.7 `FlagChangeGroup.cs` (157 lines, `public : IFlagChangeGroup`; method-level exemptions)

| Member | Lines | Class | Notes |
|---|---|---|---|
| ctor `(IApplicationGlobals, MailItem)` | 27-32 | COM | `[ExcludeFromCodeCoverage]` present; takes live MailItem |
| `Globals`/`Item`/`Subject`/`FlagChangeItems` props | 38-42 | HN | `virtual` accessors (mockable via subclass) |
| `TryEnqueue(string, IEnumerable<string>, IEnumerable<string>)` | 48-71 | HN | **preserved measured seam**: `original.CompareTo(revised)` (UtilitiesCS `IEnumerableExtensions.CompareTo<T>` line 61) then conditionally adds a `FlagChangeItem` |
| `ProcessGroupAsync(CancellationToken)` | 76-101 | COM | exempt; `MailItemHelper.FromMailItemAsync`, tokenize, drain queue |
| `TryProcessFlagItemAsync(...)` | 106-125 | COM | exempt; try/catch wrapper |
| `ProcessFlagItemAsync(...)` | 130-153 | COM | exempt; classifier train/untrain + `Serialize()` |

Note the members are `virtual` — the class is already subclass-mockable, and `TryEnqueue` depends
only on the in-memory `FlagChangeItems` collection and the pure `CompareTo` extension.

### A.8 `AutoAssignContext.cs` (96 lines, `public : IAutoAssign`, class-level exempt)

| Member | Lines | Class | Notes |
|---|---|---|---|
| ctor | 19-22 | HN | field assign |
| `FilterList` | 24 | HN | `_globals.TD.CategoryFilters` |
| `AddChoicesToDict(...)` | 26-34 | HN | `throw NotImplementedException` |
| `AddColorCategory(...)` | 36-39 | HN | `throw NotImplementedException` |
| `AutoFind(object)` | 41-44 | HN | `throw NotImplementedException` |
| `AutoFindAsync(object)` | 46-60 | COM | `ToHelper` + classifier engine |
| `ToHelper(object)` | 62-94 | COM | `MailItemHelper.FromMailItemAsync` |

### A.9 `AutoAssignPeople.cs` (95 lines, `internal : IAutoAssign`, class-level exempt)

| Member | Lines | Class | Notes |
|---|---|---|---|
| ctor (primary) | 17-19 | HN | field assign |
| `FilterList` | 21-24 | HN | `_globals.TD.CategoryFilters` |
| `AutoFindAsync(object)` | 26-36 | COM | wraps `AutoFind` in `Task.Run` |
| `AutoFind(object)` | 38-74 | MIX | type-dispatch on `objItem` (HN control flow) but each branch constructs `new MailItemHelper(... MailItem ...)` (COM) and calls `AutoFile.AutoFindPeople` (COM/IO); the `null` and unknown-type branches return `[]` (HN, testable) |
| `AddChoicesToDict(...)` | 76-84 | COM | `_globals.TD.People.AddMissingEntries(olMail)` |
| `AddColorCategory(...)` | 86-93 | COM | `CreateCategoryModule.CreateCategory(NamespaceMAPI, ...)` |

### A.10 `FlagChangeTrainingQueue.cs` (78 lines, `public : IFlagChangeTrainingQueue`, unexempted/measured)

| Member | Lines | Class | Notes |
|---|---|---|---|
| ctor | 20 | HN | empty |
| `Init()` | 22-26 | HN | builds `TimedAsyncTask(500ms, ConsumeAsync)` and returns self |
| `Options` prop | 28-29 | HN | default `Timed` |
| `Cancel`/`Queue`/`Consumer`/`ConsumerTimer` internal state | 30-35 | HN | mockable via `InternalsVisibleTo` |
| `ConsumeAsync()` | 37-60 | MIX | `Task.Run` drains `Queue`, calls `item.ProcessGroupAsync()` on each `IFlagChangeGroup` (the group is an interface, so injectable/mockable), resets guard |
| `Enqueue(IFlagChangeGroup)` | 62-76 | HN | adds to queue; branches on `Options` (Immediate vs Timed) driving guard/timer |

This class is host-neutral by construction (no WinForms/Interop `using`s). Its timer field
(`TimedAsyncTask`, 500ms) is the only nondeterminism risk (section H notes the determinism seam).

### A.11 `FlagChangeItem.cs` (23 lines, `public : IFlagChangeItem`, unexempted/measured)

Pure POCO: `ClassifierName`, `UntrainFlags`, `TrainFlags`. HN only. Fully testable already; near-zero
cost to reach 100%.

---

## B. `IEditFilterViewer` Design (derives from `IForm`)

### B.1 Problem: raw controls cannot be mocked

`IForm` (`UtilitiesCS/Interfaces/IWinForm/IForm.cs`) already declares the `Form`-level surface,
including `ShowDialog()`, `Close()`, and `event` members, and — critically — it does **not** declare
`Text`, `Show()` (parameterless), `Hide()`, `Dispose()`, or any child controls. The controller today
reaches into `_viewer.ContextSelection.Text`, wires `_viewer.BtnOk.Click += ...`, etc. Exposing those
child controls as `Label`/`Button`/`TextBox` on an interface would still force a mock to return real
`Control` instances (WinForms objects the test must not construct). The ITagViewer pattern named in
the epic manifest resolves this by projecting each consumed control interaction into a **behavioral
member**: a string property for text, and an `event EventHandler` for the click. This is the
approach `IEditFilterViewer` must follow.

### B.2 Proposed `IEditFilterViewer` members (add to what `IForm` already provides)

```
public interface IEditFilterViewer : UtilitiesCS.Interfaces.IWinForm.IForm
{
    // Text surface (was Label.Text / TextBox.Text)
    string ContextSelectionText { get; set; }
    string PeopleSelectionText  { get; set; }
    string ProjectSelectionText { get; set; }
    string TopicSelectionText   { get; set; }
    string FilterNameText       { get; set; }

    // Window lifecycle members the controller uses that IForm does NOT declare
    string Text { get; set; }   // IForm lacks Control.Text
    void Show();                 // IForm only has Show(IWin32Window)
    void Hide();
    void Dispose();

    // Click events (was control.Click +=)
    event EventHandler ContextSelectionClick;
    event EventHandler PeopleSelectionClick;
    event EventHandler ProjectSelectionClick;
    event EventHandler TopicSelectionClick;
    event EventHandler FoldersSelectedClick;
    event EventHandler OkClick;
    event EventHandler CancelClick;

    // Tip labels: GetTips() returns List<Label> today. Replace with a host-neutral
    // toggle contract so the controller no longer touches Label. See B.3.
    void ResetTips();   // performs the "toggle all tips Off" the controller does at init
}
```

Reconciliation with `IForm`: `IForm` already supplies `ShowDialog()`, `Close()`, `DialogResult`,
and every `Form` property/event, so those are NOT re-declared. Only `Text`, `Show()`, `Hide()`,
`Dispose()`, the five text properties, the seven click events, and `ResetTips()` are additive. On the
concrete `EditFilterViewer` partial class these become thin pass-throughs
(`public string ContextSelectionText { get => ContextSelection.Text; set => ContextSelection.Text = value; }`
and `public event EventHandler OkClick { add => BtnOk.Click += value; remove => BtnOk.Click -= value; }`).

### B.3 `GetTips()` disposition

Today `Initialize()` does `_viewer.GetTips().Select(l => new QfcTipsDetails(l)).ToList()` then toggles
each off. `QfcTipsDetails` wraps a `Label`, so returning `List<Label>` keeps a WinForms type on the
interface. Two options:

- Preferred: move the tip-toggle behavior fully into the concrete viewer as `ResetTips()` (the
  `QfcTipsDetails`/toggle bookkeeping is view concern), so the controller no longer references
  `Label` or `QfcTipsDetails`. The controller calls `_viewer.ResetTips()` and holds no tip state.
- Fallback (only if `QfcTipsDetails` state must live on the controller): expose the tips as an
  `IReadOnlyList<IQfcTipsDetails>` if such an interface exists/can be reused; do not expose `Label`.
  Verify whether a tips abstraction already exists in QuickFiler before creating a new one.

`SelectItems(...)` currently takes a `System.Windows.Forms.Label label` parameter and writes
`label.Text`. After retargeting, the four click handlers should pass a setter delegate
(`Action<string> setLabelText`) or the corresponding `...SelectionText` property write, so
`SelectItems` no longer takes a `Label`. This keeps the extracted selection logic host-neutral except
for the `TagViewer`/`TagController` dialog call, which becomes an injected seam (section D.1).

### B.4 Controller retarget

Change field `private EditFilterViewer _viewer;` to `private IEditFilterViewer _viewer;`. Replace the
seven `_viewer.X.Click += Handler` wirings with `_viewer.XClick += Handler`. Replace control `.Text`
reads/writes with the new string properties. Because the controller is `internal` and constructs its
own viewer in `Initialize()`/`InitializeFactory()`, introduce a viewer factory seam
(`Func<IEditFilterViewer>` defaulting to `() => new EditFilterViewer()`) so tests inject a mock
instead of the real form. This mirrors the QuickFiler factory-seam pattern already in the repo.

---

## C. `IManageFiltersViewer` Design + Logic Extraction

`ManageFilters` is a Form-derived class that holds orchestration logic against `_globals.AF.Filters`
and constructs `EditFilterController`. The extraction separates that orchestration from the form.

### C.1 New `ManageFiltersController` (host-neutral, extracted)

Move the non-WinForms logic out of the form into a new controller class that depends on
`IManageFiltersViewer` and `IApplicationGlobals`:

- `LoadFilters()` -> sets `viewer` filter rows via an abstracted `SetFilters(IEnumerable<FilterEntry>)`.
- `EditSelected()` (was `BtnEditFilter_Click` body): read `viewer.SelectedFilter`, construct an
  `EditFilterController` via an injected `Func<IApplicationGlobals, FilterEntry, EditFilterController>`
  seam (so tests assert the controller is created without launching a form).
- `AddFilter()` (was `BtnAddFilter_Click` body): construct `EditFilterController` with the
  `EditFilterCallback`, then refresh rows.
- `EditFilterCallback(EditFilterController, FilterEntry)`: `_globals.AF.Filters.Add`/`Serialize`
  (host-neutral + IO) then `viewer.RebuildList()`. Testable with a mocked globals whose `AF.Filters`
  is a fake list.
- `DeleteSelected()` (was `BtnDelete_Click`): currently reads the selected row and does nothing else;
  formalize as reading `viewer.SelectedFilter` (behavior preserved: no deletion side effect today).

### C.2 Proposed `IManageFiltersViewer` members

```
public interface IManageFiltersViewer : UtilitiesCS.Interfaces.IWinForm.IForm
{
    FilterEntry SelectedFilter { get; }          // was FiltersOlv.SelectedItem.RowObject cast
    void SetFilters(IEnumerable<FilterEntry> filters);  // was FiltersOlv.SetObjects(...)
    void RebuildList();                                 // was FiltersOlv.BuildList()
    void Show();                                        // used by EfcFormController caller

    event EventHandler AddFilterClick;    // was BtnAddFilter.Click
    event EventHandler EditFilterClick;   // was BtnEditFilter.Click
    event EventHandler DeleteClick;       // was BtnDelete.Click
}
```

`FastObjectListView`/`OLVColumn` (BrightIdeasSoftware) stay entirely inside the concrete form; the
interface exposes only `FilterEntry` and `IEnumerable<FilterEntry>`. The form partial implements these
as pass-throughs to `FiltersOlv`.

### C.3 Thin wiring left on the form

`ManageFilters` keeps only: `InitializeComponent()`, the `IManageFiltersViewer` pass-through members,
and event forwarding (its designer button `Click` handlers subscribe to the controller, or the
controller subscribes to the interface events). The `_globals` field and all `AF.Filters` logic move
to `ManageFiltersController`. Result: `ManageFilters.cs` becomes near-pure view wiring (a candidate
for a narrowed, ratified exemption; see H), while the tested logic lives in `ManageFiltersController`.

---

## D. Helper Seam Designs (Interop/COM + dialog touchpoints)

Order of preference per `.claude/rules/csharp.md`: interface seam > injectable delegate > adapter.

### D.1 `EditFilterController.SelectItems` — Tag dialog seam

Touchpoints: `new TagViewer()` (139), `new TagController(viewer, dictOptions)` (141),
`viewer.ShowDialog()` (142), `controller.ExitType`/`SelectionAsString()` (143-145).
Seam (injectable delegate): `Func<SortedDictionary<string,bool>, (bool cancelled, string selection)>
_tagSelector`, production default constructs the real `TagViewer`/`TagController` and shows the
dialog. Test injects a delegate returning a canned `(false, "A;B")`. The dict-build and label-write
logic around the dialog then becomes measured.

### D.2 `AutoCreateProject` — three seams

- **ProgramName chooser** (`ChooseOrCreateProgramName`, 77-105 via `new TagLauncher(...)` +
  `ShowDialog()`): injectable delegate `Func<IEnumerable<string>, string> _chooseProgram` (default:
  `TagLauncher` path). Test returns a canned selection; the program-id allocation branch below the
  chooser (HN/IO) then becomes measured.
- **Category creation** (`CreateCategoryModule.CreateCategory(olNS=NamespaceMAPI,...)`): adapter seam
  `Func<IPrefix, string, Category> _createCategory` (default calls `CreateCategoryModule`). Keeps the
  COM MAPI call injectable so `AddColorCategory`'s surrounding logic is measured.
- **Task-item creation** (`CreateProjectTaskItem`/`GetTaskItems` -> `Ol.App.Session.GetDefaultFolder`):
  injectable `Func<Items> _getTaskItems` (default: real folder). Test supplies a Moq `Items`.

`GetNextProjectID`, `TryAutoExtractProgram`, `StripPrefix`, `FilterList`, and the two
`NotImplementedException` throwers need no seam — they are already host-neutral against a mocked
`IApplicationGlobals`.

### D.3 `AutoAssignContext` / `AutoAssignPeople` — MailItemHelper seam

Both funnel through a private `ToHelper(object)` / `AutoFind` that constructs `MailItemHelper` from a
live `MailItem` and runs a classifier engine. Seam (injectable delegate):
`Func<object, Task<MailItemHelper>> _toHelper` (default: current body). Tests then cover:
the `null`/unknown-type early-return `[]` branches (pure, no seam needed) and, with a stubbed helper,
the classifier-dispatch path if the classifier engine is itself mockable. If `CategoryClassifierGroup.
CreateEngineAsync` cannot be seamed cheaply, leave the engine-invocation lines exempt and test the
type-dispatch/guard logic, which is the majority of branch count in `AutoFind`.

### D.4 `FlagChangeGroup` — already seamed by `virtual` + interface

`TryEnqueue` is host-neutral and needs no seam (test directly). The four COM members stay exempt.
Because members are `virtual` and `Globals`/`Item` are `virtual` internal properties, a test subclass
can override `Item`/`Globals` if any additional coverage of `TryEnqueue`'s interaction with
`FlagChangeItems` is desired. `CompareTo` is `UtilitiesCS.Extensions.IEnumerableExtensions.CompareTo<T>`
(line 61) — pure, so `TryEnqueue` is fully deterministic.

### D.5 `FlagTasks` — dialog + COM seams, constructor preserved

Constraint: preserve `FlagTasks(IApplicationGlobals, IList, bool, IntPtr, string)` (QuickFiler seam).
Approach: extract the pure statics into a host-neutral helper (section G) rather than seaming the
constructor. The remaining seams inside `FlagTasks`:
- **Flag-selection dialog** (`GetUserInputFlagsToAdjust`, `new TagViewer()`+`TagController`+
  `ShowDialog()`): injectable `Func<SortedDictionary<string,bool>, List<string>> _flagSelector`
  (default: current body). This makes `GetFlagsToSet(count>1)` measurable end-to-end with a stub.
- **Explorer/selection COM** (`GetSelection`, `InitializeToDoList`, `_globals.Ol.App.ActiveExplorer()`):
  these read live Outlook. They remain COM-bound; the reachable HN portions (`GetFlagsToSet`,
  `ConvertFlagStringsToEnum`, `GetSymbolsDictionary`) move out (section G) and are tested directly.
- **MessageBox.Show** calls (48, 104): dialog seam `Action<string,...>`/`Func<...,DialogResult>` if
  the surrounding method (`InitializeToDoList`) is to be partially covered; otherwise these stay in
  the COM-bound method and are exempt.

### D.6 `FlagChangeTrainingQueue` — timer determinism seam

`ConsumeAsync`/`Enqueue` are host-neutral but `Init()` creates a `TimedAsyncTask(500ms, ...)`. For
deterministic tests, do NOT wait on the 500ms timer. Test `Enqueue` with `Options = Immediate` (drives
`ConsumeAsync` synchronously via the guard) using a mocked `IFlagChangeGroup` whose
`ProcessGroupAsync` returns `Task.CompletedTask`, and assert queue draining + guard reset. Test the
`Timed` branch by asserting `ConsumerTimer.RequestOrResetTask()` is invoked (verify whether
`TimedAsyncTask` exposes an observable/injectable hook; if not, cover `Enqueue`'s `Timed` branch by
asserting the item lands in `Queue`). Avoid `Thread.Sleep`/`Task.Delay` entirely.

---

## E. In-Repo Callers and Required Changes

Verified by repo-wide grep of production `*.cs` (docs/evidence excluded).

| Type | Caller (file:line) | Current usage | Required change |
|---|---|---|---|
| `EditFilterController` | `ManageFilters.cs:35` | `new EditFilterController(_globals, filterEntry)` | Moves into `ManageFiltersController.EditSelected` via the `Func<...,EditFilterController>` seam (C.1). Constructor signature unchanged. |
| `EditFilterController` | `ManageFilters.cs:40,45` | `new EditFilterController(_globals, EditFilterCallback)` + callback | Moves into `ManageFiltersController.AddFilter`/`EditFilterCallback`. Signature unchanged. |
| `EditFilterController.DeleteFilterDialog` | (static; no external caller found) | — | Currently unreferenced externally; keep behavior; may test via the viewer-factory seam. |
| `ManageFilters` | `QuickFiler/Controllers/EfcFormController.cs:562-564` | `new ManageFilters(); .LoadFilters(_globals); .Show()` | Keep public `ManageFilters` type + `LoadFilters`/`Show`. Internally `LoadFilters` delegates to `ManageFiltersController`. **Preserve this three-call surface** so EfcFormController is untouched. |
| `FlagTasks` (ctor) | `TaskMaster/Ribbon/RibbonController.cs:224` | `new FlagTasks(Globals); .Run()` | Preserve ctor + `Run()`. No change required. |
| `FlagTasks.PopulateUdf` | `TaskMaster/Ribbon/RibbonController.Intelligence.cs:176` | `FlagTasks.PopulateUdf(null, Globals)` | Preserve static signature. No change. |
| `FlagTasks` (ctor) | `QuickFiler/Legacy/QfcController.cs:1549` | `new FlagTasks(...)` | Preserve ctor. No change. |
| `FlagTasks` (ctor, via factory) | `QuickFiler/Controllers/QfcItemController.Initialization.cs:42,390` | `Func<IApplicationGlobals, List<MailItem>, bool, IntPtr, FlagTasks>` -> `new FlagTasks(globals, itemList, blFile, hWndCaller)` | **Hard constraint**: preserve the 4-usable-arg ctor and the `FlagTasks` type name. Extracting statics (G) does not affect this. |
| `FlagTasks` (factory seam type) | `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs`, `...EventHandlersTests.cs` | tests build `Func<...,FlagTasks>` | Do not change `FlagTasks`'s constructor shape or these tests break. |
| `AutoCreateProject` | `TaskMaster/Ribbon/TryFunctionalityInConstruction.cs:252` | `new AutoCreateProject(AppGlobals)` | Preserve primary ctor. If seams are added as optional ctor params with defaults, keep the single-arg form valid. |
| `AutoCreateProject` | `FlagTasks.cs:60` | `new AutoCreateProject(globals)` | Same; internal to feature. |
| `AutoAssignPeople` | `FlagTasks.cs:59` | `new AutoAssignPeople(globals)` | `internal`; feature-internal. Preserve or add optional seam params with defaults. |
| `AutoAssignContext` | `FlagTasks.cs:61` | `new AutoAssignContext(globals)` | `public`; feature-internal caller. Preserve ctor. |
| `FlagChangeGroup` | `TaskVisualization/TaskController.cs:953,1054` | constructed in TaskController | **Owned by #297** (TaskController is #297's file). #298 must not edit TaskController; keep `FlagChangeGroup(IApplicationGlobals, MailItem)` ctor stable. |
| `FlagChangeTrainingQueue` | `TaskMaster/AppGlobals/AppToDoObjects.cs:498` | `new FlagChangeTrainingQueue().Init()` returns `IFlagChangeTrainingQueue` | Preserve parameterless ctor + `Init()` returning the interface. No change. |
| `FlagChangeItem` | `FlagChangeGroup.cs:58` (only) | object initializer | Preserve POCO. No change. |
| `IFlagChangeTrainingQueue`/`IFlagChangeGroup`/`IFlagChangeItem` | `UtilitiesCS/Interfaces/IToDo/*`, `TaskMaster.Test`, `UtilitiesCS.Test` | interface consumers | Interfaces live in UtilitiesCS; do not change them. |

Net caller impact: the only production caller that must keep an exact surface is `EfcFormController`
(`ManageFilters` three-call surface) and the QuickFiler `FlagTasks` factory (constructor shape). All
other changes are internal to the `TaskVisualization` project. No caller signature needs editing if
the public surfaces above are preserved.

---

## F. Coordination With #297 (executes first)

**State of #297 at research time:** `issue.md` is populated, but `spec.md` and
`plan.2026-07-09T16-07.md` are still template stubs — they do not yet name the concrete seam types,
the `ITaskViewer` member list, or the decomposed file names. Therefore #298 cannot bind to specific
#297 artifacts by name; it must record assumptions to verify at execution time (after #297 merges to
`epic/winforms-testability-refactor-integration`).

### F.1 Seams/abstractions #297 is expected to create that #298 should REUSE (not duplicate)

- **`ITaskViewer`** (`TaskViewer` -> interface). `FlagTasks` constructs `new TaskViewer()` and passes
  it to `new TaskController(formInstance: _viewer, ...)`. After #297, `TaskController` depends on
  `ITaskViewer`. #298's `FlagTasks` should construct the viewer through whatever factory/seam #297
  introduces rather than a second bespoke seam. Verify the post-#297 `TaskController` constructor
  parameter type for `formInstance`.
- **Dialog/`MessageBox` seam.** #297's spec explicitly plans "seams intercepting `MessageBox`/input
  dialogs." `FlagTasks` (48, 104) and `EditFilterController`/`AutoCreateProject` all show dialogs.
  #298 should reuse #297's dialog seam abstraction (e.g., an `IMessageBoxService`/`IDialogService`
  type) if #297 introduces one, instead of adding per-call delegates. **Assumption to verify:** the
  name and shape of #297's dialog seam.
- **Outlook-Interop adapter types.** #297 plans "Outlook Interop boundaries mocked behind seams."
  `AutoCreateProject.GetTaskItems`, `FlagTasks.GetSelection`/`InitializeToDoList`, and the
  `AutoAssign*` `MailItemHelper` construction all cross the same Interop boundary. If #297 introduces
  an adapter (e.g., wrapping `Ol.App.Session.GetDefaultFolder` or `MailItemHelper.FromMailItemAsync`),
  #298 should consume that adapter. **Assumption to verify:** whether such an adapter exists post-#297
  and its surface.
- **`TaskController` decomposition + `FlagChangeGroup` usage.** #297 owns `TaskController.cs`
  (constructs `FlagChangeGroup` and enqueues into `FlagChangeTrainingQueue`). #298 must not edit
  `TaskController`; it only needs `FlagChangeGroup`'s ctor and `FlagChangeTrainingQueue`'s
  `Init`/`Enqueue` to remain stable, which #297 is not chartered to change.
- **`TaskVisualization.Test` shared fixtures.** #297 creates the first substantial test suite in
  `TaskVisualization.Test`. #298 should reuse the existing `MoqOlToDo` helper
  (`TaskVisualization.Test/MoqOlToDo.cs`, already mocks `IApplicationGlobals`, `IOlObjects`,
  `Categories`, `MailItem`, `UserProperties`) and any globals-builder #297 adds, rather than
  duplicating mock scaffolding. Note the current `FlagTasks_Test.cs` lives under a `Z.Disabled.*`
  namespace and is inert; #298 can replace/enable it.

### F.2 Explicit assumptions #298's atomic plan must verify at execution time

1. `ITaskViewer` exists and `TaskController`'s `formInstance` parameter is `ITaskViewer` (or a
   compatible base). If not, `FlagTasks`'s viewer construction seam must be self-contained.
2. A reusable dialog/`MessageBox` seam type exists from #297. If absent, #298 introduces narrow
   per-call delegate seams (D.1/D.5) rather than blocking.
3. A reusable Interop adapter (folder/items/`MailItemHelper`) exists from #297. If absent, #298 uses
   local injectable delegates at each boundary.
4. `FlagChangeGroup(IApplicationGlobals, MailItem)`, `FlagChangeTrainingQueue.Init()`, and the
   `IFlagChange*` interfaces are unchanged by #297.
5. `TaskVisualization.csproj` and `TaskVisualization.Test.csproj` compile clean at the post-#297
   integration head before #298 adds its `<Compile Include>` entries (both projects are non-SDK
   packages.config; new files are registered manually).
6. #197's exemption annotations (`[ExcludeFromCodeCoverage]`) are still present on the in-scope files
   at the post-#297 head; #298 removes/narrows them as it introduces seams.

Additional cross-feature note (not a #297 dependency): `EditFilterController.SelectItems` and
`FlagTasks.GetUserInputFlagsToAdjust` construct `TagViewer` + `TagController` from the `Tags` project,
which sibling **#293** retargets to `ITagViewer`. #298 depends only on #297, but #293 also lands on
the integration branch. **Verify at execution** whether #293's `TagController` constructor overloads
(`TagController(TagViewer, SortedDictionary<string,bool>)` and the 5-arg overload) still accept the
arguments these call sites pass; if #293 changed them to `ITagViewer`, the call sites compile as long
as `TagViewer : ITagViewer`. Recording as a watch item, not a hard dependency.

---

## G. Proposed File Decomposition (all <= 500 lines; HN files free of WinForms/Interop)

No in-scope production file currently exceeds 500 lines (largest is
`EditFilterViewer.designer.cs` at 503 — a generated file). **`EditFilterViewer.designer.cs` at 503
lines technically exceeds the 500-line limit but is Designer-generated**; per the General Code Change
Policy, generated designer code is not hand-edited, and the repo exempts designer files from the file
budget by convention. #298 should not attempt to hand-split the designer; if a mechanical reduction is
required, note it as generated-code carve-out in the plan. All other files are comfortably under 500.

Proposed new files (added to `TaskVisualization.csproj` `<Compile Include>` manually):

| New file | Responsibility | Category | Approx lines |
|---|---|---|---|
| `IEditFilterViewer.cs` | interface (B.2) | HN (interface-only) | ~40 |
| `IManageFiltersViewer.cs` | interface (C.2) | HN (interface-only) | ~25 |
| `ManageFiltersController.cs` | extracted filter-management logic (C.1) | HN (+ seam delegates) | ~90 |
| `FlagTasksFlagSelection.cs` (or `FlagCalculations.cs`) | extracted pure statics `GetFlagsToSet`/`ConvertFlagStringsToEnum`/`GetSymbolsDictionary` | HN | ~70 |

Modified existing files (retarget + seam, no size violation introduced):
`EditFilterController.cs` (retarget to `IEditFilterViewer` + viewer factory + `_tagSelector` seam;
remove class-level exemption, keep method-level on genuinely UI-bound members),
`EditFilterViewer.cs` (implement `IEditFilterViewer` pass-throughs + `ResetTips`),
`ManageFilters.cs` (implement `IManageFiltersViewer`; delegate logic to `ManageFiltersController`),
`FlagTasks.cs` (call extracted statics; add `_flagSelector` seam; narrow exemption),
`AutoCreateProject.cs` (add optional seam ctor params with safe defaults; narrow exemption),
`AutoAssignContext.cs` / `AutoAssignPeople.cs` (add `_toHelper` seam; narrow exemption),
`FlagChangeTrainingQueue.cs` / `FlagChangeItem.cs` (no structural change; add tests).

Interface-only files (`IEditFilterViewer.cs`, `IManageFiltersViewer.cs`) are legitimately 0%
executable coverage and are excluded from measurement per the standard interface-only clarification;
they must not reference WinForms control types (only `FilterEntry`, `IEnumerable<FilterEntry>`,
`EventHandler`, `string`) so they stay host-neutral aside from deriving `IForm` (which is a WinForms
surface interface already established by the epic pattern).

---

## H. Per-Unit Test Approach and Coverage Path to >= 80%

Framework: MSTest + Moq + FluentAssertions. Reuse `TaskVisualization.Test/MoqOlToDo.cs` for
`IApplicationGlobals` mocks. No real `Form`/`Control` instantiation, no popups, no `Thread.Sleep`/
`Task.Delay`, no temp files, deterministic.

### H.1 High-value host-neutral units (largest coverage lever, no seam needed)

- **`FlagCalculations`/`FlagTasks` statics** (extracted): `GetSymbolsDictionary` (asserts excluded
  `All`/`None`, sorted keys), `ConvertFlagStringsToEnum` (empty -> `All`; valid strings -> bit-or;
  invalid ignored), `GetFlagsToSet(1)` -> `All`, `GetFlagsToSet(>1)` via injected `_flagSelector`
  stub -> parsed enum. Positive/negative/edge covered with pure inputs.
- **`FlagChangeItem`**: POCO round-trip; defaults `UntrainFlags`/`TrainFlags` non-null empty. Trivial
  100%.
- **`FlagChangeGroup.TryEnqueue`**: (a) no difference -> returns false, nothing enqueued;
  (b) additions/removals -> returns true, one `FlagChangeItem` with correct `TrainFlags`/`UntrainFlags`
  from `CompareTo`. Uses a subclass overriding `Item`/`Globals` (both `virtual`) so no live MailItem.
- **`FlagChangeTrainingQueue`**: `Init()` returns self and sets `ConsumerTimer`; `Enqueue` with
  `Options=Immediate` + mocked `IFlagChangeGroup` -> `ProcessGroupAsync` invoked, queue drained,
  guard reset; `Enqueue` with `Options=Timed` -> item present in `Queue` (assert via
  `InternalsVisibleTo` access) and timer requested. Determinism: never wait on the 500ms timer; drive
  the Immediate path or assert enqueue state directly.
- **`AutoCreateProject`** HN members: `FilterList` (mocked `TD.CategoryFilters`), `GetNextProjectID`
  (mocked `ProjInfo`/`IDList` -> asserts seed selection + next-id), `TryAutoExtractProgram`
  (substring match true/false + longest-first ordering), `StripPrefix` (prefix present/absent/empty),
  `AddChoicesToDict`/`AutoFind` -> assert `NotImplementedException`.
- **`AutoAssignContext`** HN members: `FilterList`; `AddChoicesToDict`/`AddColorCategory`/`AutoFind`
  -> `NotImplementedException`. `AutoFindAsync` with a stubbed `_toHelper` returning null -> `[]`.
- **`AutoAssignPeople`**: `FilterList`; `AutoFind(null)` -> `[]`; `AutoFind(unknownType)` -> `[]`
  (both pure branches). The MailItemHelper-constructing branches stay behind the `_toHelper` seam or
  remain exempt.

### H.2 Controller/viewer-interface units (mocked interface, no live form)

- **`EditFilterController`**: construct with a `Mock<IEditFilterViewer>` via the injected viewer
  factory; a `Mock<IApplicationGlobals>` (MoqOlToDo). Tests:
  - `RegisterEventHandlers` wires each `...Click` event (assert `mock.SetupAdd`/raise triggers the
    handler).
  - `BtnOk_Click`: raises `OkClick`; asserts `_filterEntry.Name = viewer.FilterNameText`, callback
    invoked when set, `Hide()`/`Dispose()` called (verify on mock).
  - `BtnCancel_Click`: with null callback -> `Close()` called and `RevertToCopy` applied; with
    callback -> no close.
  - `Initialize`/`InitializeFactory`: with a mocked viewer, assert the four selection texts are set
    from `_filterEntry.Flags.*` and `ResetTips()` is called.
  - `SelectItems`: with an injected `_tagSelector` returning `(false,"X;Y")`, assert the target text
    property is written; with `(true, _)` (cancelled) assert no write.
- **`ManageFiltersController`**: `Mock<IManageFiltersViewer>` + mocked globals.
  - `LoadFilters` -> `viewer.SetFilters(globals.AF.Filters)` called.
  - `AddFilter` -> the `Func<...,EditFilterController>` seam invoked with the callback; `RebuildList`
    called.
  - `EditFilterCallback` -> `globals.AF.Filters.Add`/`Serialize` and `RebuildList` invoked.
  - `EditSelected`/`DeleteSelected` -> read `viewer.SelectedFilter` (behavior preserved).

### H.3 Irreducibly UI/COM-bound lines (justified, maintainer-ratified `[ExcludeFromCodeCoverage]`)

Minimize exemptions to genuine wiring. Candidates after refactor:
- Concrete `EditFilterViewer` / `ManageFilters` form partial classes (control pass-throughs,
  `InitializeComponent`) — designer + thin view wiring. Follow existing repo policy: the form partial
  class carries class-level `[ExcludeFromCodeCoverage]`, which also covers the `.designer.cs` partial.
  This is the established pattern (both forms already annotated this way).
- `FlagTasks` constructor and `Run` (construct `TaskViewer`/`TaskController`, `ShowDialog`) and the
  COM-reading statics `GetSelection`, `InitializeToDoList`, `PopulateUdf` — method-level exemption on
  the genuinely Outlook-bound members; the extracted pure statics are measured.
- `AutoCreateProject.CreateProjectTaskItem`/`GetTaskItems`/`ToHelper`/`AutoFindAsync` and the MAPI
  `CreateCategory` line inside `AddColorCategory` — method-level (or seam-delegated) exemption for the
  live-Interop portions only.
- `AutoAssign*.ToHelper`/`AutoFindAsync` classifier-engine invocation lines — method-level exemption
  where the engine cannot be cheaply seamed.
- `FlagChangeGroup`'s four Outlook-bound members — keep #197's existing method-level exemptions.

Testable seams are never exempt: `TryEnqueue`, the extracted `FlagCalculations`, `FlagChangeItem`,
`FlagChangeTrainingQueue` logic, `AutoCreateProject` HN members, and both controllers stay measured.

### H.4 Coverage arithmetic (project-wide >= 80%)

The `TaskVisualization` denominator after #297 splits `TaskController` into measured logic files plus
`ITaskViewer`-mocked controller tests. #298 adds the secondary denominator: the extracted HN files
(`FlagCalculations`, `ManageFiltersController`), the retargeted `EditFilterController`, and the helper
HN members. The two Designer files and the two form partials are exempt (per established pattern), so
they do not dilute the denominator. Given the high HN density catalogued in section A (the throwers,
pure allocators, POCO, queue logic, and both controllers), reaching >= 80% line coverage on the
non-exempt denominator is achievable primarily through the H.1 pure-unit tests plus the H.2
mocked-interface controller tests, without any live form or Outlook process. The exact post-#297
denominator must be recomputed against the integration head (assumption F.2.5) before final targets
are locked; capture a baseline coverage artifact at plan Phase 0 under
`<FEATURE>/evidence/baseline/`.

### H.5 Determinism confirmation

- No `Form`/`Control` is instantiated in any test (interfaces mocked with Moq).
- No `ShowDialog`/`Show`/`MessageBox` executes (all behind factory/delegate seams returning canned
  results).
- No `Thread.Sleep`/`Task.Delay`/real timer wait (the `FlagChangeTrainingQueue` 500ms timer is never
  awaited; the Immediate path is driven synchronously). `Thread.Sleep`/`Task.Delay` are also
  banned-API per `.claude/rules/csharp.md` BannedSymbols.
- No temp files, no network, no external process. `IApplicationGlobals` is fully mocked
  (`MoqOlToDo`).
- Async tests await mocked `Task.CompletedTask`/canned results; no wall-clock dependence.

---

## Automation Feasibility

This refactor is code-only. It requires no third-party UI, no external portal, and no human-in-the-loop
step at implementation time. Every change is source edits within the `TaskVisualization` project and
its test project, plus manual `<Compile Include>` registration in the two non-SDK csproj files. The
work is fully automatable through the standard C# toolchain in order: `csharpier` (format) ->
`.NET analyzers` via msbuild (lint) -> nullable/`TreatWarningsAsErrors` msbuild (type-check) ->
`vstest.console.exe ... /EnableCodeCoverage` (MSTest). No step depends on a live Outlook process
(all Interop is mocked or seamed), no popup or dialog is shown during tests, and coverage is measured
against the standard vstest instrumentation. The only sequencing dependency is external to automation:
#298 executes after #297 merges to the integration branch, per the epic's declared dependency.

---

## Rejected Alternatives (brief)

- **Expose raw WinForms controls (`Label`/`Button`/`TextBox`) on the viewer interfaces.** Rejected:
  a Moq mock would have to return real `Control` instances, forcing WinForms object construction in
  tests, violating the "no live form" mandate. Behavioral members (string props + events) avoid this.
- **Seam `FlagTasks`'s constructor to inject the viewer/controller.** Rejected: the QuickFiler factory
  `Func<IApplicationGlobals, List<MailItem>, bool, IntPtr, FlagTasks>` pins the constructor shape.
  Extracting the pure statics into a host-neutral file achieves the coverage goal without touching the
  constructor contract.
- **Keep #197's class-level exemptions and rely on them for the 80% floor.** Rejected: contradicts the
  epic NFR ("no exemptions on testable seams") and the feature's charter to make the secondary viewers
  and helpers genuinely testable. Exemptions are narrowed to irreducible UI/COM wiring only.
