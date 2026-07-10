# Coverage-Exemption Inventory Reconciliation (P7-T1)

- Timestamp: 2026-07-10T00-01
- Scope: `[ExcludeFromCodeCoverage]` reconciliation against source under the maintainer-ratified STA last-resort refinement.
- Method: `grep -rn "ExcludeFromCodeCoverage"` across the touched `TaskVisualization` production files.

## File-level exemptions (unchanged by this refactor)

| File / unit | Exemption | Irreducible dependency | Reducibility note |
|---|---|---|---|
| `TaskVisualization/TaskViewer.cs` (partial `TaskViewer`) | Class-level `[ExcludeFromCodeCoverage]` (line 18) | Form-derived; thin facade delegation to live controls; accept/cancel wiring. | Facade is thin pass-through; verified indirectly via the mocked `ITaskViewer` in controller tests. The single class-level attribute on the `TaskViewer.cs` partial covers the whole type INCLUDING `TaskViewer.Designer.cs` (annotating both partials would raise CS0579), so `TaskViewer.Designer.cs` carries no separate attribute. |
| `TaskVisualization/TagPromptService.cs` | Class-level `[ExcludeFromCodeCoverage]` (line 18) | Dialog host: constructs `Tags.TagViewer` + `Tags.TagController` and calls `ShowDialog()`. | The `ITagPromptService` interface (not the adapter) is the testable seam; the four assign-dialog paths are covered against `Mock<ITagPromptService>`. |

## Method/branch-level exemptions

| File — member | Named irreducible dependency | Reducibility / seam-infeasibility note |
|---|---|---|
| `TaskController.Accelerator.cs` — `WireKeyPressHandlers` (line 45) | Walks the live control tree of the concrete `TaskViewer` (`form.ForAllControls`, `control.KeyPress +=`). | Guarded on the concrete type (`if (_viewer is TaskViewer form)`), so pump-less STA tests over a mock viewer skip it; production `_viewer` is a `TaskViewer`. Not reducible without a live form. |
| `TaskController.Accelerator.cs` — `PostMessage` `extern` (line 60) | Win32 `user32.dll` P/Invoke; posts to a live `hWnd`. | No managed body; exempt by nature. |
| `TaskController.Accelerator.cs` — `FocusTextBox` (line 284) | `TextBox.Select()` / caret positioning require input focus on a live handled control. | Called only from the `ExecuteXlAction` `TextBox` branch; the dispatch line itself is measured, only the focus body is exempt. |
| `TaskController.Accelerator.cs` — `FocusComboBox` (line 292) | `ComboBox.Select()` / `DroppedDown` require input focus and a live window handle. | Same as above for the `ComboBox` branch. |
| `TaskController.Accelerator.cs` — `DispatchDateTimePickerClick` (line 301) | The picker's live window `Handle` and the Windows message pump (`PostMessage(dt.Handle, ...)`). | Extracted from `ExecuteXlAction` so the rest of that method stays measured; the `DateTimePicker` dispatch cannot run under a never-shown STA control. |
| `TaskController.Flags.cs` — `ApplyChanges` (line 35) | COM-iteration wiring: `FlagChangeGroup` construction from a live `MailItem`, the static `ToDoEvents.Editing` edit-count queue, `WriteFlagsBatchAsync` (live UDF/category writes), and `Globals.TD.FlagChangeTrainingQueue.Enqueue`. | Method-level exemption per the plan's inventory. Verified this session that driving `ApplyChanges` over Moq doubles does not terminate deterministically (the `ToDoEvents.Editing` / `WriteFlagsBatchAsync` COM-iteration path depends on live COM semantics). The extractable host-neutral units it calls — both `ApplyChange` overloads and `AreCollectionsEqual` — remain measured (see `TaskControllerFlagsTests`). |

## Confirmations

- NO file-level `[ExcludeFromCodeCoverage]` on `TaskController.Accelerator.cs`, `TaskController.ControlMaps.cs`, or `TaskController.ControlRelationships.cs` (grep count = 0 for file/class-level on those partials).
- NO class-level `[ExcludeFromCodeCoverage]` on `TaskController` (removed in P4-T1; `TaskController.cs` line 20 declares `public partial class TaskController` with no attribute).
- The control-identity builders (`GetControlRelationships`, `Get*Lookup`, `OptionsGroups`, `NavTips`) and the measured accelerator state machine (`ToggleXl`, `UpdateCaptions`, non-Focus/non-DateTimePicker branches of `ExecuteXlAction`, `ToggleXlGroupNav`, `DeactivateActiveXlGroup`, `ActivateXlGroup` x3, `RecurseXl`, `KeyboardHandler_KeyDown`/`KeyPress`, `InitializeAccelerators`) carry NO exemption and are measured via the STA tests (`TaskControllerControlMaps.StaTests`, `TaskControllerAccelerator.StaTests`, `TaskControllerAcceleratorKeyboard.StaTests`).

## Seam-infeasibility rationale (condition a) for newly-STA-covered regions

- `TipsController`/`NavTips` throw without a real parented `TableLayoutPanel`/`Panel`; the lookup dictionaries key on real control object identity; `TLP.BackColor` and `Button.PerformClick` require real `Control` instances. A mock of those members would either throw or not exercise the identity semantics under test, so an STA harness supplying real never-shown in-memory controls through `ITaskViewerControls` is the least-privilege approach (it removes only the `Form`-type dependency, not the real-control dependency).

## Not-exempt-but-uncovered residue (counted against coverage, not exempted)

- The `InvokeRequired == true` re-invoke one-liners in `OK_Action` / `MergeFlag`, the `OK_Action` category-selected branch tail (it awaits the exempt `ApplyChanges`), and the `ExecuteXlAction` `TextBox`/`ComboBox` dispatch lines that call the exempt focus helpers are left in the denominator and simply counted as uncovered. They are not exempted. The measured-core aggregate still meets the 80% floor.

## Remediation update (2026-07-10) — `SetFlag(Taskname)` / `Shortcut_ReadingNews` now covered

- Prior state: the `SetFlag` `Taskname` case wrote `_active.TaskSubject` directly, a get-only interop `MailItem.TaskSubject` property recoverable only against a live COM object, so it and `Shortcut_ReadingNews` (which calls `SetFlag(..., Taskname)`) were left uncovered residue (see the entry removed above).
- Fix: added an optional-with-default constructor seam `Action<string> setActiveTaskSubject = null` to both `TaskController` constructors (mirroring the existing `_showWarning` / `_mailItemHelperFactory` seam pattern). `InitializeSeams` wires `_setActiveTaskSubject = setActiveTaskSubject ?? (v => _active.TaskSubject = v);` so production keeps the original direct write by default, while tests inject a capturing delegate.
- `SetFlag`'s `Taskname` case now calls `_setActiveTaskSubject(value)` instead of writing `_active.TaskSubject` directly.
- New tests `SetFlag_Taskname_WritesSubjectAndFacade` and `Shortcut_ReadingNews_SetsAllFlagsAndFocusesDuration` (in `TaskControllerActionsTests.cs`) inject a capturing delegate and assert the captured `TaskSubject` value, the `TaskNameText` facade write, and (for the shortcut) `mock.Verify(v => v.FocusDuration())`.
- Both methods are now measured; no exemption was added or removed for either method (they were never method-level exempted — only the residue note above is retired).
