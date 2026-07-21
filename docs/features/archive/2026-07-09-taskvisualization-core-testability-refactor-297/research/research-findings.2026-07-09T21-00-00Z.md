# Research: TaskVisualization Core Testability Refactor (#297)

- Feature: `2026-07-09-taskvisualization-core-testability-refactor-297`
- Epic: winforms-testability-refactor (#295); child #297, wave 0, no upstream dependency; downstream sibling #298 depends on #297.
- Scope: `TaskVisualization/TaskController.cs` (1861 lines) decomposition; introduce `ITaskViewer`; make `TaskViewer` implement it; retarget the controller; add MSTest+Moq+FluentAssertions coverage of the refactored core to >= 80% line coverage.
- Out of scope (owned by #298): `EditFilterController`, `EditFilterViewer`, `ManageFilters`, `Flag*`/`AutoCreate*`/`AutoAssign*` helpers, except where `TaskController` depends on them. Reusable pieces for #298 are flagged inline.
- Constraint reminder: research only. No production/test code was written.

---

## A. Region / Section Map of `TaskController.cs`

`TaskController` is a single `public class` carrying a class-level `[ExcludeFromCodeCoverage]` (line 20). The `_viewer` field is the concrete `TaskViewer` (line 256). Classification legend: HN = host-neutral logic, WF = WinForms interaction, COM = Outlook-Interop, MX = mixed.

| Region (source `#region`) | Lines | Members | Classification |
|---|---|---|---|
| Constructors and Initializers | 23–227 | 2 ctors (35–136), `Initialize` (141–210), `ActivateOptions` (215–225) | MX. Ctors: HN state setup + COM (`Categories` enumeration 67–68/117–119) + WF (`SetController`, `AcceptButton`/`CancelButton` wiring, lookup building from live controls). `Initialize`: WF (writes control text/selection/checked/value from `_active`) + accelerator deactivation + `ForAllControls` keypress wiring. `ActivateOptions`: WF (control `Enabled`/`Visible`). |
| Public Properties | 229–294 | `Globals`, `_options`, `Options` (239–247), `ChangedFlags`, `PostMessage` P/Invoke (251–252), `WM_LBUTTONDOWN`, `_viewer`, `_todo_list`, `_active`, `_dict_categories`, XL lookup dictionaries, `_defaults`, `_autoAssign`, `_userEmailAddress`, `ProjectAssign`, `ContextAssign`, `ProjectsToPrograms` | MX. Field/property store is HN; `Options` setter calls `ActivateOptions` (WF); `PostMessage` is a Win32 P/Invoke (WF). |
| Public Major Actions | 296–548 | `AssignPeople` (301), `AssignContext` (339), `AssignProject` (373), `AssignTopic` (414), `Assign_KB` (449), `Assign_Priority` (455), `Today_Change` (473), `Bullpin_Change` (479), `FlagAsTask_Change` (485), `MouseFilter_FormClicked` (490), `OK_Action` (503), `Cancel_Action` (540) | MX. Assign{People,Context,Project,Topic} construct+`ShowDialog` a live `TagViewer` (WF dialog) and read `_active.OlItem.InnerObject` (COM); the model-update tail is HN. `Assign_KB`/`Assign_Priority`/`Today_Change`/`Bullpin_Change`/`FlagAsTask_Change` map a single control value to `_active` (MX, becomes HN through the facade). `OK_Action`: WF (`InvokeRequired`/`Invoke`/`Hide`/`DialogResult`/`Dispose`) + orchestration + `Task.Run(ApplyChanges)`. `Cancel_Action`: WF lifecycle. |
| Public Shortcuts | 550–616 | `Shortcut_Personal` (553), `Shortcut_Meeting`, `Shortcut_Email`, `Shortcut_Calls`, `Shortcut_PreRead`, `Shortcut_WaitingFor`, `Shortcut_Unprocessed`, `Shortcut_ReadingBusiness`, `Shortcut_ReadingNews` (607) | HN-through-facade. Each sets constant strings into `_active` + control text via `SetFlag`; `Shortcut_ReadingNews` also calls `_viewer.Duration.Focus()` (WF). |
| Public Keyboard Events and Properties | 618–768 | `AutoAssignAllAsync` (620), `KeyboardHandler_KeyDown` (649), `KeyboardHandler_KeyPress` (749), `SuppressKeystrokes` (763) | MX/COM/WF. `AutoAssignAllAsync`: COM (`MailItem`, `MailItemHelper.FromMailItemAsync`) + `IAutoAssign.AutoFindAsync` (mockable) + `MergeFlag`. `KeyboardHandler_KeyDown`/`_KeyPress`: accelerator state-machine driving control visibility (WF). |
| Private Helper Properties and Functions | 770–1088 | `AnyCategorySelected` (776), `SetFlag` (793), `MergeToCollection` (836), `MergeFlag` (851), `CaptureDuration` (910), `ApplyChanges` (943), `ApplyChange(flag,...)` (1037), `ApplyChange(fcg,...)` (1053), `AreCollectionsEqual` (1074) | MX. `AnyCategorySelected`/`SetFlag`/`MergeFlag` read/write control text (HN through facade). `MergeToCollection`/`AreCollectionsEqual` are pure HN. `CaptureDuration`: parse logic (HN) + `MessageBox.Show` (WF dialog) + `_viewer.Duration.Text` (WF). `ApplyChanges`: COM iteration (`FlagChangeGroup`, `c.OlItem`, `WriteFlagsBatchAsync`, `ToDoEvents.Editing` static, `Globals.TD.FlagChangeTrainingQueue`); `ApplyChange` overloads are HN field-diff decisions. |
| Keyboard UI | 1090–1380 | `ToggleXl` (1092), `UpdateCaptions` (1118), `ExecuteXlAction` (1124), `ToggleXlGroupNav` (1190), `DeactivateActiveXlGroup` (1197), `ActivateXlGroup` x3 (1217/1260/1275), `RecurseXl` (1289) | WF. Operate on `Label`/`Control` identity, `.Visible`, `.Text`, `.BackColor`, `.Handle`, `PerformClick`, `PostMessage`, `TipsController`. Irreducibly UI-bound. |
| Data Groupings | 1382–1859 | `GetOptionsLookup` x2, `GetCaptionLookup` x2, `GetControlLookup` x2, `GetControlRelationships` (1527–1736, ~210 lines), `ControlRelationship` struct (1738–1762), `OptionsGroups` (1764–1833), `NavTips` (1835–1857) | WF. Build dictionaries keyed by live `Label` instances referencing ~50 concrete controls; `NavTips` constructs `TipsController` which throws unless each `Label` has a real `TableLayoutPanel`/`Panel` parent. Irreducibly UI-bound. |

Approximate split by classification: WF-bound (Keyboard UI + Data Groupings + accelerator handlers + control-facing lifecycle) ~ 900 lines; host-neutral or facade-reachable logic ~ 700 lines; COM iteration in `ApplyChanges`/`AutoAssignAllAsync` ~ 120 lines.

---

## B. Viewer / Control Member Inventory and `ITaskViewer` Design

### B.1 Every viewer member the controller touches

Non-control (form-level) members used on `_viewer`:
- `SetController(TaskController)` (ctor, lines 55/106).
- `AcceptButton` set (56/107), `CancelButton` set (57/108) — assigned from `OKButton`/`Cancel_Button`.
- `OKButton`, `Cancel_Button` (read as `IButtonControl` targets; also appear in control maps).
- `InvokeRequired` (505, 853), `Invoke(Delegate)` (507, 855).
- `Hide()` (525, 542), `Dispose()` (532, 545), `DialogResult` set (530, 544).
- `ForAllControls(Action<Control>)` extension (203) — operates on the concrete `Control` tree.

Data-bearing controls (the business surface). All are `internal` on `TaskViewer.Designer.cs`; access mode used by controller in parentheses:
- `TaskName` (TextBox): `.Text` get/set (143, 518, 519, 611, 824).
- `CategorySelection` (Label): `.Text` get/set (145, 331, 368, 556, 781, 800, 871).
- `PeopleSelection` (Label): `.Text` get/set (147, 331... 782, 806, 880).
- `ProjectSelection` (Label): `.Text` get/set (149, 403, 560, 783, 812, 889).
- `TopicSelection` (Label): `.Text` get/set (151, 443, 817, 898).
- `PriorityBox` (ComboBox): `.SelectedItem` get/set (157/162/167, 457).
- `KbSelector` (ComboBox): `.SelectedItem` get/set (172, 451).
- `Duration` (TextBox): `.Text` get/set (178, 518/915, 829), `.Focus()` (613).
- `DtReminder` (DateTimePicker): `.Value` set, `.Checked` set (182–183).
- `DtDuedate` (DateTimePicker): `.Value` set, `.Checked` set (187–188).
- `CbxToday` (CheckBox): `.Checked` get (475).
- `CbxBullpin` (CheckBox): `.Checked` get (481).
- `CbxFlagAsTask` (CheckBox): `.Checked` get (487, 513).

Accelerator / nav controls (identity-keyed, WF only): `XlTopic, XlProject, XlPeople, XlContext, XlTaskname, XlImportance, XlKanban, XlWorktime, XlOk, XlCancel, XlAutotag, XlReminder, XlDuedate, XlSector1..4, XlSc*` (12 shortcut accelerators), `C1S1..C4S4` (nav cells), `Lbl*` caption labels, `Shortcut*` buttons, `AutoTagButton`, plus `.Text` reads on captions. These appear only inside `GetControlRelationships`, `OptionsGroups`, `NavTips`, `ExecuteXlAction`.

### B.2 `ITaskViewer` design — recommendation: intent-named facade (for the data surface) + `IForm` base

Recommended shape:

```
public interface ITaskViewer : UtilitiesCS.Interfaces.IWinForm.IForm
{
    void SetController(TaskController controller);

    // Intent-named data facade (primitives only; no WinForms Control types leak)
    string TaskNameText { get; set; }
    string ContextText { get; set; }        // CategorySelection.Text
    string PeopleText { get; set; }         // PeopleSelection.Text
    string ProjectText { get; set; }        // ProjectSelection.Text
    string TopicText { get; set; }          // TopicSelection.Text
    string DurationText { get; set; }
    object PrioritySelectedItem { get; set; }   // PriorityBox.SelectedItem
    object KbSelectedItem { get; set; }         // KbSelector.SelectedItem
    bool TodayChecked { get; }              // CbxToday.Checked
    bool BullpinChecked { get; }            // CbxBullpin.Checked
    bool FlagAsTaskChecked { get; set; }    // CbxFlagAsTask.Checked
    DateTime ReminderValue { set; }
    bool ReminderChecked { set; }
    DateTime DueDateValue { set; }
    bool DueDateChecked { set; }
    void FocusDuration();                    // Duration.Focus()
}
```

Rationale for intent-named facade over interface-typed control abstractions:
1. Moq setup is trivial: `mock.SetupProperty(v => v.TaskNameText)` (or `SetupAllProperties`). No need to fabricate `Label`/`TextBox`/`ComboBox` instances, which is the exact thing the unit-test policy forbids for determinism.
2. The business logic only ever reads/writes `Text`/`Checked`/`Value`/`SelectedItem`. Intent-named members collapse control-type noise and keep the host-neutral controller free of `System.Windows.Forms` control types (COM/logic separation, epic pattern item 3).
3. `IForm` already supplies the Form-level surface (`AcceptButton`, `CancelButton`, `DialogResult`, `Show`/`ShowDialog`, and — through `IForm : IContainerControl, IScrollableControl : IControl` — `InvokeRequired`, `Invoke(Delegate)`, `Hide()`, `Dispose()`, `Focus()`, `Controls`). Verified: `IControl` (`UtilitiesCS/Interfaces/IWinForm/IControl.cs:9`) extends `IComponent, ISynchronizeInvoke, IWin32Window, IDisposable`, so `InvokeRequired`/`Invoke`/`Dispose` resolve through the base; no duplication is needed on `ITaskViewer`.

Reconciling overlap with `IForm`:
- `AcceptButton`/`CancelButton` come from `IForm` (typed `IButtonControl`). The current ctor wiring `formInstance.AcceptButton = formInstance.OKButton;` references `OKButton`/`Cancel_Button`, which are not on the facade. Recommendation: relocate this two-line accept/cancel wiring into `TaskViewer.SetController` (pure UI wiring), removing the controller's need to expose `OKButton`/`Cancel_Button`. This keeps `ITaskViewer` minimal and removes a WinForms coupling from the controller.
- `Invoke(() => ...)` currently passes an `Action` lambda; `IControl.Invoke(Delegate method)` accepts it (lambda is convertible to `Delegate`). No change to call sites.

Why the accelerator controls are deliberately NOT on `ITaskViewer`: `GetControlRelationships`/`OptionsGroups`/`NavTips` use the `Label`/`Control` instances as dictionary keys and by object identity, and `TipsController` (`UtilitiesCS/HelperClasses/ToolTips/TipsController.cs`, namespace `TaskVisualization`) throws in its constructor unless each label's `Parent` is a live `TableLayoutPanel`/`Panel`. These cannot be represented as intent-named primitives and cannot be satisfied by a Moq mock without constructing real controls. They stay bound to the concrete `TaskViewer` and are reached from exempt partials via a single encapsulated cast `private TaskViewer Form => (TaskViewer)_viewer;` (see Section D). This confines all WinForms-identity access to exempt code while the testable core sees only `ITaskViewer`.

`TaskViewer` implements the facade by delegating each member to its control (for example `public string TaskNameText { get => TaskName.Text; set => TaskName.Text = value; }`). This is thin, Form-derived, and exempt.

---

## C. Outlook-Interop / COM touchpoints and dialog/popup calls — seam design

### C.1 Dialog / popup calls

| Call | Lines | Nature | Seam (preferred → fallback) | Production default |
|---|---|---|---|---|
| `new TagViewer(); ...ShowDialog()` in `AssignPeople`/`AssignContext`/`AssignProject`/`AssignTopic` | 315–333, 352–370, 386–408, 427–445 | Constructs a live `Tags.TagViewer` form + modal dialog + `TagController` | Interface seam: `ITagPromptService` with one method per prompt returning the selected string list and an exit flag, e.g. `TagPromptResult PromptTags(TagPromptRequest request)`. Fallback: injectable `Func<TagPromptRequest, TagPromptResult>`. | Production adapter constructs `TagViewer`+`TagController`, shows the dialog, and returns the result. Tests inject a stub returning a canned selection or a `Cancel`. |
| `MessageBox.Show(...)` in `CaptureDuration` | 923, 930 | WinForms popup on parse failure | Injectable delegate seam: `Action<string> _showWarning` (narrow, single call path). Fallback: `IUserNotifier.Warn(string)`. | Production default `MessageBox.Show`. Tests inject a capturing `Action<string>` and assert message content. |

`TagViewer`/`TagController` construction is a natural shared abstraction to hand to #298 (`EditFilterController`/`ManageFilters` open the same class of Tags dialogs). Define `ITagPromptService` in `TaskVisualization` (or a shared location) so #298 reuses it — flagged as a reusable seam.

### C.2 Outlook-Interop / COM touchpoints

| Call | Lines | Nature | Seam | Production default |
|---|---|---|---|---|
| `foreach (Category cat in olCategories)` | 67–68, 117–119 | Enumerates `Microsoft.Office.Interop.Outlook.Categories` into `_dict_categories` | The ctor already receives `Categories` as a parameter; tests pass a Moq `Categories` (the existing `MoqOlToDo.MockCategories` already does exactly this). No new seam required; keep as constructor input. | Real `globals.Ol.NamespaceMAPI.Categories`. |
| `_active.OlItem.InnerObject` cast to `MailItem` | 324, 622, 642, 955 | Reads the COM item behind a `ToDoItem` | `ToDoItem`/`IOutlookItem` is already an injectable model seam; `MoqOlToDo` builds `MailItem` mocks. No new seam; the `is not MailItem` guard (622) makes the non-mail path testable. | Real `MailItem`. |
| `MailItemHelper.FromMailItemAsync(mailItem, Globals, ...)` | 626–628 | COM/async helper building a mining DTO | Adapter/delegate seam: `Func<MailItem, Task<MailItemHelper>>` injected, default `m => MailItemHelper.FromMailItemAsync(m, Globals, default, false)`. Enables `AutoAssignAllAsync` to be tested without live Outlook. | Real static call. |
| `ProjectAssign.AutoFindAsync` / `ContextAssign.AutoFindAsync` / `_autoAssign.AutoFindAsync` | 630, 636, 642 | `IAutoAssign` (interface already) | Already interface-typed and constructor-injected → mock directly with Moq. | Real `AutoAssignPeople`/`AutoCreateProject`/`AutoAssignContext`. |
| `new FlagChangeGroup(Globals, mailItem)`, `c.OlItem.GetOlItemType()`, `c.WriteFlagsBatchAsync`, `fcg.TryEnqueue`, `Globals.TD.FlagChangeTrainingQueue.Enqueue` | 953–1026 | COM item write + training-queue side effects inside `ApplyChanges` | The per-field decision is already isolated in `ApplyChange` (testable). For the iteration wiring, the smallest seam is a `Func<ToDoItem, FlagChangeGroup>` factory plus keeping `ToDoEvents`/queue interactions in the orchestration method. `ToDoItem.WriteFlagsBatchAsync` is exercised against a Moq-backed `ToDoItem` (MoqOlToDo pattern). | Real factory `c => new FlagChangeGroup(Globals, c.OlItem.InnerObject as MailItem)`. |
| `ToDoEvents.Editing.AddOrUpdate/UpdateOrRemove` | 947, 1028 | Static concurrent-dictionary side effect | Static; leave in the orchestration method. It is deterministic in-process state (no COM), so a test that drives `ApplyChanges` with mocked `ToDoItem`s can assert its net effect, or the lines fall under the orchestration exemption if a static seam is disproportionate. | Unchanged. |

### C.3 Irreducibly UI-thread-bound calls (isolated so tests never reach them)

- `_viewer.InvokeRequired` / `_viewer.Invoke(...)` marshalling (505–508, 853–856): guard clauses at the top of `OK_Action` and `MergeFlag`. In tests, mock `InvokeRequired => false`, so the marshalling branch is skipped and the body runs inline. The `true` branch (re-invoke) is a thin one-liner eligible for targeted exemption.
- `PostMessage` P/Invoke and `dt.Handle` (251, 1156): live window handle simulated click inside `ExecuteXlAction` — WF-only, lives in the exempt accelerator partial.
- `Label.Visible`/`.BackColor`/`.Text`, `Button.PerformClick`, `TipsController` (Keyboard UI + Data Groupings): require real controls with real parents — exempt accelerator/control-map partials.
- `_viewer.Hide()`/`Dispose()`/`DialogResult` (lifecycle): thin, driven from `OK_Action`/`Cancel_Action`; assert via the mocked `ITaskViewer` (`mock.Verify(v => v.Hide())`) rather than executing a real form.

---

## D. Proposed File Decomposition

Design goal: every production file <= 500 lines; host-neutral files must not reference `System.Windows.Forms` control types or `Microsoft.Office.Interop.Outlook` beyond the already-injected model seams. Remove the class-level `[ExcludeFromCodeCoverage]` (line 20) so the testable partials count toward coverage; apply narrow exemptions only where noted.

Recommended approach: **partial classes for the controller** (preserves shared private fields `_xlCtrlsActive`, `_altActive`, `_altLevel`, `_activeNavGroup`, `_active`, `_options` without widening visibility) **plus extracted pure-logic helper classes** for the genuinely reusable calculations. The concrete-control access in the accelerator/control-map partials is funneled through one private `TaskViewer Form => (TaskViewer)_viewer;` accessor, so the retarget to `ITaskViewer` does not force ~50 control properties onto the interface.

| # | File | Class / partial | Contents (methods moved here) | Approx lines | Classification / coverage |
|---|---|---|---|---|---|
| 1 | `ITaskViewer.cs` | `interface ITaskViewer : IForm` | Facade members (Section B.2) | ~40 | Interface-only; excluded from coverage measurement per general-unit-test type-only clarification. |
| 2 | `TaskController.cs` | `partial class TaskController` | Fields, ctors, `Options`/`ChangedFlags`/assign properties, `Initialize` → `InitializeData` (facade writes) + delegation to `InitializeAccelerators`, `ActivateOptions` split (data part). `_viewer` typed `ITaskViewer`; `Form` cast accessor declared here. | ~320 | Testable core. No class-level exemption. |
| 3 | `TaskController.Actions.cs` | `partial class TaskController` | `AssignPeople/Context/Project/Topic` (using `ITagPromptService`), `Assign_KB`, `Assign_Priority`, `Today_Change`, `Bullpin_Change`, `FlagAsTask_Change`, `Shortcut_*`, `AnyCategorySelected`, `SetFlag`, `MergeFlag`, `MergeToCollection`, `OK_Action`, `Cancel_Action`, `CaptureDuration` (using duration parser + notifier seam). | ~380 | Testable through `ITaskViewer` + seams. `Invoke==true` branch + `FocusDuration` targeted-exempt. |
| 4 | `TaskController.Flags.cs` | `partial class TaskController` | `ApplyChanges` (COM iteration/orchestration), `ApplyChange` (both overloads), `AreCollectionsEqual`. | ~180 | `ApplyChange`/`AreCollectionsEqual` testable; `ApplyChanges` COM-iteration lines targeted-exempt where a `FlagChangeGroup` factory seam is disproportionate. |
| 5 | `TaskController.Accelerator.cs` | `partial class TaskController` | `KeyboardHandler_KeyDown`, `KeyboardHandler_KeyPress`, `SuppressKeystrokes`, `MouseFilter_FormClicked`, `AutoAssignAllAsync`, Keyboard UI region (`ToggleXl`, `UpdateCaptions`, `ExecuteXlAction`, `ToggleXlGroupNav`, `DeactivateActiveXlGroup`, `ActivateXlGroup` x3, `RecurseXl`), `InitializeAccelerators`, `PostMessage` P/Invoke. | ~430 | WF-bound. File-level `[ExcludeFromCodeCoverage]` on the partial, maintainer-ratified (see G). Uses `Form` accessor. `AutoAssignAllAsync` is COM but its `IAutoAssign`/`MergeFlag` logic can be partially covered — keep it here but leave a test hook. |
| 6 | `TaskController.ControlMaps.cs` | `partial class TaskController` | `GetOptionsLookup`/`GetCaptionLookup`/`GetControlLookup` (x2 each), `GetControlRelationships`, `ControlRelationship` struct, `OptionsGroups`, `NavTips`. | ~400 | WF-bound (live `Label`/`TipsController`). File-level exempt, maintainer-ratified. Uses `Form` accessor. |
| 7 | `TaskDurationParser.cs` | `static class` or small class | Pure parse+validate of the duration string → `(bool ok, int minutes, string error)`. | ~40 | Fully testable; target >= 90%. Reusable by #298. |
| 8 | `TaskPriorityMapper.cs` | `static class` | `OlImportance` <-> display string ("High"/"Low"/"Normal"), both directions (used by `Initialize` and `Assign_Priority`). | ~40 | Fully testable; >= 90%. |
| 9 | `ITagPromptService.cs` (+ `TagPromptService.cs` adapter) | interface + adapter | Prompt seam for the four assign dialogs (Section C.1). Adapter is the only place that constructs `TagViewer`/`TagController`. | interface ~25; adapter ~90 | Interface testable via mock; adapter is WF/dialog, exempt. Reusable by #298. |

Notes:
- `TaskViewer.cs` gains `: ITaskViewer` and the thin facade property implementations, plus the relocated accept/cancel wiring in `SetController`; it remains Form-derived and exempt (already `[ExcludeFromCodeCoverage]`).
- Dependency direction: files 2–4, 7–8 and the `ITagPromptService` interface reference only `ITaskViewer`, model types (`ToDoItem`, `IAutoAssign`, `Enums`, `ToDoDefaults`), and primitives — no `System.Windows.Forms` control types. Files 5–6 and the adapter are the only host-bound units and hold the concrete-control access. This satisfies epic pattern item 3 (COM/logic separation).
- Every proposed file is well under 500 lines; the largest (`TaskController.Actions.cs` ~380, file 5 ~430, file 6 ~400) have margin.

Alternative considered and rejected: full extraction of the accelerator subsystem into a standalone `TaskAcceleratorController` composed by `TaskController`. Rejected because the accelerator state machine shares four mutable private fields and calls back into `AssignProject/People/Topic/Context`; extraction would require exposing those fields/callbacks through a new interface purely to relocate code that stays exempt regardless. Partial classes preserve behavior with lower risk. The extraction is noted only as a possible future step.

---

## E. In-repo Callers and Required Changes

Repo-wide grep for `TaskController` / `TaskViewer` (`*.cs`) returns four files; VB/other hosts do not reference them.

| File | Reference | Change required |
|---|---|---|
| `TaskVisualization/FlagTasks.cs` | `new TaskViewer()` (57); `new TaskController(formInstance: _viewer, ...)` (62–74); `_viewer.ShowDialog()`/`_viewer.Show()` (84–86) | None functionally. `_viewer` is a `TaskViewer`, which will implement `ITaskViewer`, so it satisfies the retargeted constructor parameter (`ITaskViewer formInstance`). `_viewer.Show()/ShowDialog()` still resolve on the concrete Form. If the constructor also newly requires `ITagPromptService`/notifier/`MailItemHelper` factory seams, add them as optional parameters with production defaults so `FlagTasks` needs no edit; if made required, add the two production adapters at the `new TaskController(...)` call site. Recommend optional-with-default to keep caller churn zero. |
| `TaskVisualization/TaskViewer.cs` | `private TaskController _controller`; `SetController(TaskController)`; event handlers call `_controller.<Method>()` | Add `: ITaskViewer` + facade implementations + relocated accept/cancel wiring in `SetController`. Because the partial split preserves all public/internal method names (`OK_Action`, `Cancel_Action`, `KeyboardHandler_KeyDown`, `AssignX`, `Shortcut_X`, `AutoAssignAllAsync`, `Today_Change`, etc.), the event-routing calls are unchanged. |
| `TaskVisualization/TaskController.cs` | self | Decomposed per Section D. |
| `TaskVisualization.Test/MoqOlToDo.cs` | `using TaskVisualization;` (test support only; no direct `TaskController`/`TaskViewer` construction) | No change; reused as-is for building mock `Categories`/`MailItem`/`IApplicationGlobals`. |

`FlagTasks` itself is #298-adjacent (it is a `Flag*` helper), but it is the production entry point that constructs the controller, so its call-site compatibility is a #297 concern and the recommendation above keeps it edit-free.

---

## F. Existing `TaskVisualization.Test` Project — patterns, reuse, gaps

Project: `TaskVisualization.Test/TaskVisualization.Test.csproj`, net4.8.1, MSTest 4.2.2 (`MSTest.TestFramework`, `MSTest.TestAdapter`, Microsoft.Testing.Platform 2.2.2), Moq 4.20.72, FluentAssertions 8.9.0 — all already referenced. Analyzer stack (Meziantou/Sonar/Roslynator/AsyncFixer/BannedApi) wired in, matching the production project. `ProjectReference`s: `TaskVisualization`, `ToDoModel`, `UtilitiesCS`. `InternalsVisibleTo("TaskVisualization.Test")` is declared in `FlagTasks.cs` (line 16), so `internal` controller members are visible to tests.

Current test files:
- `FlagTasks_Test.cs`: namespace `Z.Disabled.TaskVisualization.Test`; the single `[TestMethod]` is fully commented out (dead/disabled). No active assertions.
- `MoqOlToDo.cs`: reusable Moq factory. Builds mock `PropertyAccessor`, `UserProperty`/`UserProperties`, `MailItem` (`MailItemMock`), `Category`/`Categories` (`MockCategory`/`MockCategories`), `Selection`/`Explorer`/`Application`/`NameSpace`/`IOlObjects`, and `IApplicationGlobals` (`MockGlobals`). This is directly reusable to construct `Categories` for the ctor and `MailItem`-backed `ToDoItem`s.
- `Properties/AssemblyInfo.cs`: standard.

Reusable as-is: the whole `MoqOlToDo` helper (COM mock builders), MSTest+Moq+FluentAssertions toolchain, `InternalsVisibleTo`. `MailItemMock` uses `DateTime.Now` (lines 189–199) — acceptable in a test helper, but new tests should avoid time-dependent assertions (prefer fixed `DateTime` inputs; a `TimeProvider` is not required by this refactor).

Gaps the new tests need:
1. A `Mock<ITaskViewer>` builder (facade) with `SetupAllProperties()` plus explicit `InvokeRequired => false` — new helper, e.g. `MoqTaskViewer`.
2. A `ToDoItem` builder for `_todo_list[0]` that survives `DeepCopy()` and exposes `Context/People/Projects/Topics/Program/KB` (`FlagTranslator`) plus scalar fields — buildable from `new ToDoItem(IOutlookItem)` over a `MoqOlToDo` mail mock; `ToDoItem(IOutlookItem)` and `DeepCopy()` are confirmed present (`ToDoModel/Data Model/ToDo/ToDoItem.cs:32,127`).
3. Stubs for the new seams: `Mock<ITagPromptService>`, capturing `Action<string>` notifier, `Func<MailItem, Task<MailItemHelper>>` (or a mockable factory), and `Mock<IAutoAssign>`.
4. `ToDoDefaults` with a `PrefixList` containing the `Context`/`People`/`Project`/`Topic` prefixes the assign/shortcut methods look up (`_defaults.PrefixList.Find(x => x.Key == "...")`). `ToDoDefaults` is a real constructible class (`ToDoModel/.../ToDoDefaults.cs`); a small fixture seeds the four prefixes.
5. New test files should live under `TaskVisualization.Test/` mirroring source names (`TaskControllerTests.cs`, `TaskControllerActionsTests.cs`, `TaskControllerFlagsTests.cs`, `TaskDurationParserTests.cs`, `TaskPriorityMapperTests.cs`) and be added to the test `.csproj` `<Compile>` group; the current `Z.Disabled` namespace should not be reused.

---

## G. Unit-to-Test Mapping, Exemptions, Determinism

### G.1 Test plan by unit

- `TaskDurationParser` (file 7): positive ("15" → ok,15), zero ("0" → ok,0), negative ("-3" → not ok, ArgumentOutOfRange message), non-integer ("abc" → not ok, cast message), empty/whitespace. Assert tuple with FluentAssertions. Pure — no mocks.
- `TaskPriorityMapper` (file 8): each `OlImportance` → string and each string → `OlImportance`, plus default/unknown fallback to Normal. Round-trip property-style cases.
- `CaptureDuration` (via `TaskController.Actions`): valid duration sets `_active.TotalWork`; invalid path invokes the injected notifier `Action<string>` (assert captured message) and leaves `TotalWork` unchanged; assert through the mocked `ITaskViewer.DurationText`.
- `SetFlag`: for each `FlagsToSet` case, assert `_active.<Field>.AsStringNoPrefix` and the corresponding `ITaskViewer` facade setter were written (`mock.VerifySet(v => v.ContextText = value)`).
- `MergeFlag`: null/empty input → no-op; non-empty merges into `_active` collection (order-independent) and updates facade text; with `InvokeRequired=false` the inline path runs. Negative: `InvokeRequired=true` re-invokes (assert `Invoke` called) — thin branch.
- `MergeToCollection` / `AreCollectionsEqual`: pure set-semantics — equal sets (any order), disjoint, null handling (`AreCollectionsEqual(null,null)` true; one null false), duplicate collapse.
- `AnyCategorySelected`: returns false only when all four facade texts equal their placeholder strings; true if any differs. Edge: exactly one differs.
- `Assign_KB` / `Assign_Priority` / `Today_Change` / `Bullpin_Change` / `FlagAsTask_Change`: map facade value onto `_active` — positive per branch (High/Low/Normal for priority) and the checkbox getters.
- `Shortcut_*`: assert the constant strings land in `_active` and facade (delegates to `SetFlag`). `Shortcut_ReadingNews` also calls `FocusDuration` (assert `mock.Verify(v => v.FocusDuration())`).
- `AssignPeople/Context/Project/Topic`: inject `Mock<ITagPromptService>`; Cancel result leaves `_active`/facade unchanged; a non-cancel selection updates `_active.<Field>` and facade text; `AssignProject` additionally maps program via `ProjectsToPrograms` (inject a `Func<string,string>` stub).
- `ApplyChange` (both overloads): option flag off → no change; on with equal collections → no change/no `ChangedFlags` bit; on with differing collections → writes `current` and sets the flag bit; the `fcg`-overload with a mock/factory verifies enqueue path.
- `ApplyChanges`: drive with a two-item `_todo_list` of Moq-backed `ToDoItem`s, `_options` covering several flags; assert net field application and `ChangedFlags` reset per item. COM-only side effects (`FlagChangeGroup` enqueue, `ToDoEvents`) asserted via the injected factory/mock where seamed; otherwise those specific lines are exemption candidates.
- `OK_Action`: `InvokeRequired=false`; when `AnyCategorySelected` false → no apply, no `DialogResult`; when true → captures FlagAsTask/taskname/duration, calls `Hide`, runs `ApplyChanges`, sets `DialogResult.OK`, calls `Dispose` (verify on mock). Determinism: `await Task.Run(ApplyChanges)` completes against in-memory mocks.
- `Cancel_Action`: verifies `Hide`, `DialogResult.Cancel`, `Dispose`.
- `AutoAssignAllAsync`: non-mail item (`OlItem.InnerObject` not `MailItem`) → early return; mail item with mock `IAutoAssign` returning tags → `MergeFlag` invoked per category (assert facade text). Uses injected `MailItemHelper` factory to avoid live Outlook.
- `Options` setter: toggles `ActivateOptions` data effects observable via facade Enabled/Visible only if surfaced — otherwise limited to the data-group portion; the control-`Enabled/Visible` writes are exempt.

### G.2 Exemptions (minimize; testable seams never exempt)

Maintainer-ratified `[ExcludeFromCodeCoverage]` scope, per the COM/VSTO/WinForms exemption in project policy and epic NFR:
- File-level on `TaskController.Accelerator.cs` and `TaskController.ControlMaps.cs` — irreducible: `Label`/`Control` identity, `.Visible`/`.BackColor`/`.Handle`, `PerformClick`, `PostMessage`, and `TipsController` (which throws without a live parent). No injectable seam exists without constructing real controls, which the unit-test policy forbids.
- Method/branch-level within testable partials, limited to: the `InvokeRequired==true` re-invoke one-liners (505–508, 853–856), `FocusDuration`/`Duration.Focus()`, `_viewer.Hide/Dispose` execution lines only if a mock cannot verify them (they can, so prefer no exemption), and the `ApplyChanges` COM-iteration lines (`FlagChangeGroup`/`ToDoEvents`/queue) where a factory seam is disproportionate.
- `TaskViewer.cs`, `TaskViewer.Designer.cs`, and the `TagPromptService` adapter remain exempt (Form-derived / Designer / dialog host).
- Explicitly NOT exempt: `ITaskViewer`-driven controller logic, `TaskDurationParser`, `TaskPriorityMapper`, `SetFlag`/`MergeFlag`/`Merge`/`AreCollectionsEqual`/`ApplyChange`, assign/shortcut model updates, `OK_Action`/`Cancel_Action` decisions. The removal of the current class-level `[ExcludeFromCodeCoverage]` (line 20) is required so these count.

Coverage target: the refactored core (files 2–4, 7–8 and the `ITagPromptService` mock paths) reaches >= 80% line coverage for #297; new helper classes target >= 90% per policy. The exempt accelerator/control-map partials are removed from the denominator via the ratified exemption, so the testable-denominator floor is met without weakening assertions.

### G.3 Determinism confirmation

- No real `Form`/`Control` instantiation in tests: `ITaskViewer` is fully mocked (facade primitives); `Categories`/`MailItem`/`ToDoItem` use the existing `MoqOlToDo` mock builders.
- No popups: `MessageBox.Show` replaced by an injected `Action<string>`; `TagViewer.ShowDialog` replaced by `ITagPromptService`.
- No `Thread.Sleep`/`Task.Delay`: none introduced; `Task.Run(ApplyChanges)` is awaited to completion over in-memory mocks (BannedApiAnalyzers already bans `Thread.Sleep`/`Task.Delay`).
- No temp files, no network, no mutable global state relied upon (the `ToDoEvents.Editing` static is in-process and reset per item; tests that touch it assert net effect deterministically or the lines are exempt).
- Time: tests supply fixed `DateTime` inputs; no `DateTime.Now`/`UtcNow` in the refactored core (both are banned symbols). The existing `MoqOlToDo.MailItemMock` uses `DateTime.Now`, so new tests must not assert on time-derived values from that helper.

---

## Automation Feasibility

This refactor is code-only. It introduces one interface, decomposes one class into partial files plus small pure-logic helpers, adds an internal prompt/notifier seam, and adds MSTest tests. It requires no third-party UI, no external portal, no credentialed service, and no human step at implementation time. Verification is fully automatable through the repository's standard C# toolchain in order: `dotnet tool run csharpier .` (format) → `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzers) → `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` (nullable type-check) → `vstest.console.exe <TaskVisualization.Test.dll> /EnableCodeCoverage` (MSTest + coverage). All test doubles are Moq-based and deterministic; no live Outlook process or WinForms message loop is needed to run or gate the suite. The maintainer-ratified `[ExcludeFromCodeCoverage]` decisions are the only non-automatable governance step, and they are a review-time approval, not an implementation-time blocker.
