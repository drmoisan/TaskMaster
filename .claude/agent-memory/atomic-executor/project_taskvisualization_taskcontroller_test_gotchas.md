---
name: taskvisualization-taskcontroller-test-gotchas
description: TaskController (#297) unit-test gotchas — ApplyChanges hangs over Moq, get-only MailItem.TaskSubject, STA harness parenting/NavTips warmup, C# 7.3 test project
metadata:
  type: project
---

Non-obvious constraints when unit-testing `TaskVisualization/TaskController` (from #297; sibling #298 reuses `ITaskViewer`/`ITagPromptService`).

**ApplyChanges hangs over Moq doubles.** Driving `TaskController.ApplyChanges` (and therefore the `OK_Action` category-selected branch) over Moq-backed `ToDoItem`s does NOT terminate — the `ToDoEvents.Editing` edit-count queue + `WriteFlagsBatchAsync` + `FlagChangeGroup` COM-iteration path depends on live COM semantics and spins/blocks the testhost (observed: single test never completes, testhost stays alive with no output). It only enters this path when a real `IApplicationGlobals` is supplied (else `FlagChangeGroup` ctor fast-fails on `globals.ThrowIfNull()`).
- **Why:** production runs against live Outlook COM; the mock `MailItem`'s `EntryID`/property semantics don't satisfy the queue/write loop's exit conditions.
- **How to apply:** mark `ApplyChanges` `[ExcludeFromCodeCoverage]` (method-level, sanctioned by the exemption inventory) and do NOT write a test that calls it or `OK_Action` with a category selected. Cover `ApplyChange` (both overloads) + `AreCollectionsEqual` directly instead.

**`MailItem.TaskSubject` is get-only on the Moq proxy.** `SetFlag(_, Taskname)` and `Shortcut_ReadingNews` write `_active.TaskSubject`, which routes to `OutlookItemExtensions.TrySetPropertyValue` → reflection `InvokeMember(SetProperty,"TaskSubject")` → `MissingMethodException` on the Castle proxy. Production recovers via the `catch(COMException)` → `Subject`-alt fallback, but that needs a live COM object; the mock raises `MissingMethodException` (not `COMException`) so it propagates.
- **How to apply:** treat the `Taskname` write path as COM-bound; don't assert it via a Moq `MailItem`. (Scalar setters that are field-only, e.g. `ReminderTime`, are safe; ones using `SetAndSave` to a get-only interop prop, e.g. `TaskSubject`/`DueDate`, are not.)

**STA harness for the control-identity/accelerator regions.** The lookup dictionaries key on real control object identity and `TipsController`/`NavTips` throw without a real parented `TableLayoutPanel`/`Panel`, so a `Mock<ITaskViewer>.As<ITaskViewerControls>()` whose getters return REAL never-shown controls parented in a real `TableLayoutPanel` (each control in its own styled column) is required. Gotchas: (a) don't set `.Text` on `DateTimePicker` (must parse as a date — `FormatException`); (b) warm `controller.NavTips` (property) before invoking `KeyboardHandler_*`/`ToggleXlGroupNav`/`RecurseXl`-single-match, because `ToggleXlGroupNav` reads the `_navTips` FIELD directly (null until the `NavTips` property is first accessed — production warms it via `InitializeAccelerators`). Use `[STATestClass]`/`[STATestMethod]` (MSTest 4.2.2), `using(){}` blocks (see below), dispose all controls.

**`TaskVisualization.Test` is C# 7.3** — no `using var` declarations (use `using (var x = ...) { }`); tuple deconstruction is fine. See also [[project_build_test_env]] (C# 7.3 in QuickFiler.Test).

**Long/hanging vstest runs:** `vstest.console ... | tail` buffers ALL output until the process exits, so a hung run shows a 0-byte output file indefinitely. Use direct stdout redirect (no `tail` pipe) and poll the file, or a `/logger:trx`, to observe progress and catch hangs. `/InIsolation` is required for this Moq assembly.
